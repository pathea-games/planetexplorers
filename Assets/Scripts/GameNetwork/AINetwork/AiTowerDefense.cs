using UnityEngine;
using System.Collections;

public class AiTowerDefense : AiCommonTD
{
    public static AiTowerDefense mInstance;
	private EntityMonsterBeacon _mbEntity;
	private int _missionId;
	private int _targetId;

    public int MissionId { get { return _missionId; } }
    public int TargetId { get { return _targetId; } }
    
	PeMap.MonsterBeaconMark m_Mark;

	protected bool isStart;

    protected override void OnPEAwake()
    {
        base.OnPEAwake();
        mInstance = this;
	}

    protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>();
		_missionId = info.networkView.initialData.Read<int>();
		_targetId = info.networkView.initialData.Read<int>();
		authId = info.networkView.initialData.Read<int>();
		_teamId = info.networkView.initialData.Read<int>();

		_pos = transform.position;
		rot = transform.rotation;
		StartCoroutine(WaitForMainPlayer());
	}

    protected override void OnPEStart()
    {
        base.OnPEStart();

		BindAction(EPacketType.PT_InGame_TDStartInfo, RPC_S2C_TDStartInfo);
        BindAction(EPacketType.PT_InGame_TDInitData, RPC_S2C_ResponseInitData);
        BindAction(EPacketType.PT_InGame_TDInfo, RPC_S2C_TDInfo);
        BindAction(EPacketType.PT_InGame_TDMonsterDeath, RPC_S2C_MonsterDeath);

        if (IsAuth())
        {
            m_Mark = new PeMap.MonsterBeaconMark();
            m_Mark.Position = GetAuthPos();
            PeMap.LabelMgr.Instance.Add(m_Mark);
        }
	}

	protected override void OnPEDestroy ()
	{
		base.OnPEDestroy ();
        mInstance = null;
		Pathea.PeCreature.Instance.Destory(Id);
		if(null != m_Mark)
			PeMap.LabelMgr.Instance.Remove(m_Mark);

		if (null != UITowerInfo.Instance)
			UITowerInfo.Instance.isShow = false;
	}

    public static void OnMonsterAdd(int id, AiNetwork ai, Pathea.PeEntity entity)
    {
        EntityMonsterBeacon mbEntity = Pathea.EntityMgr.Instance.Get(id) as EntityMonsterBeacon;
        if (null != mbEntity)
        {
            mbEntity.OnMonsterCreated(entity);
        }
        else
        {
            if (null != entity)
            {
				Pathea.CommonCmpt cc = entity.GetCmpt<Pathea.CommonCmpt>();
				if (cc != null)
				{
					AiTowerDefense td = AiTowerDefense.Get<AiTowerDefense>(id);
					if (null != td)
						cc.TDpos = td._pos;
				}

				Pathea.SkAliveEntity sae = entity.GetCmpt<Pathea.SkAliveEntity>();
                if (sae != null)
                {
                    sae.SetAttribute(Pathea.AttribType.DefaultPlayerID, 8);
                    sae.SetAttribute(Pathea.AttribType.CampID, 26);
                }
            }
        }
    }

    public static bool IsAuth()
    {
        return null == mInstance ? false : mInstance.hasOwnerAuth;
    }

    public static Vector3 GetAuthPos()
    {
        PlayerNetwork net = PlayerNetwork.GetPlayer(mInstance.authId);
		if (null == net)
			return Vector3.zero;
		else
			return net._pos;
    }

    IEnumerator WaitForMainPlayer()
	{
		while (null == PlayerNetwork.mainPlayer)
			yield return null;

		if (hasOwnerAuth)
		{
			if (-1 == _missionId && -1 == _targetId)
			{
				ItemAsset.ItemObject item = ItemAsset.ItemMgr.Instance.Get(Id);
				if (null == item)
					yield break;

				ItemAsset.Drag drag = item.GetCmpt<ItemAsset.Drag>();
				if (null == drag)
					yield break;

				DragArticleAgent.Create(drag, _pos, Vector3.one, Quaternion.identity, Id, this);
                StartCoroutine(WaitForActivate());
            }
			else
			{
				SceneEntityCreator.self.AddMissionPoint(_missionId, _targetId, Id);
				StartCoroutine(WaitForActivate());
			}
		}
		else
		{
			RPCServer(EPacketType.PT_InGame_TDInitData);
		}
    }

	void SyncTDInfo(int totalCount, int waveIndex, float preTime, float coolTime)
	{
		RPCServer(EPacketType.PT_InGame_TDInfo, totalCount, waveIndex, preTime, coolTime);
	}

	void SyncTDStartInfo(int totalWave, float preTime)
	{
		RPCServer(EPacketType.PT_InGame_TDStartInfo, totalWave, preTime);
	}

	void OnActivate()
	{
        if (null != _mbEntity)
        {
			isStart = false;
			_mbEntity.handlerNewWave += OnNewWave;
            _mbEntity.handerNewEntity += OnNewEntity;

			Vector3 tdPos;
			if (-1 == MissionId && -1 == TargetId)
			{
				tdPos = _pos;
			}
			else
			{
				GetTdGenPos(out tdPos);
				_pos = transform.position = _mbEntity.TargetPosition = tdPos;
			}

			if (null != _mbEntity.SpData)
            {
                float preTime = _mbEntity.SpData._timeToStart + _mbEntity.SpData._waveDatas[0]._delayTime;
                TypeTowerDefendsData data = MissionManager.GetTypeTowerDefendsData(TargetId);
                if (Pathea.PeGameMgr.IsMultiStory && data != null)
                    preTime = data.m_Time;
                totalWave = _mbEntity.SpData._waveDatas.Count;
                _mbEntity.UpdateUI(_missionId, 0, totalWave, preTime);
				SyncTDStartInfo(totalWave, preTime);
            }
        }
    }

    public static Vector3 GetTdGenPos( int targetId)
    {
        Vector3 pos = Vector3.zero;
        TypeTowerDefendsData data = MissionManager.GetTypeTowerDefendsData(targetId);
        if (null != data)
        {
            switch (data.m_Pos.type)
            {
                case TypeTowerDefendsData.PosType.getPos:
                    pos = Pathea.PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.pos:
                    pos = data.m_Pos.pos;
                    break;
                case TypeTowerDefendsData.PosType.npcPos:
                    pos = Pathea.EntityMgr.Instance.Get(data.m_Pos.id).position;
                    break;
                case TypeTowerDefendsData.PosType.doodadPos:
                    pos = Pathea.EntityMgr.Instance.GetDoodadEntities(data.m_Pos.id)[0].position;
                    break;
                case TypeTowerDefendsData.PosType.conoly:
                    if (!CSMain.GetAssemblyPos(out pos))
                        pos = Pathea.PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.camp:
                    if (!VArtifactUtil.GetTownPos(data.m_Pos.id, out pos))
                        pos = Pathea.PeCreature.Instance.mainPlayer.position;
                    break;
                default:
                    break;
            }
        }
        return pos;
    }

    void GetTdGenPos(out Vector3 pos)
	{
		pos = Vector3.zero;
		TypeTowerDefendsData data = MissionManager.GetTypeTowerDefendsData(TargetId);
		if (null != data)
		{
			switch (data.m_Pos.type)
			{
				case TypeTowerDefendsData.PosType.getPos:
					pos = Pathea.PeCreature.Instance.mainPlayer.position;
					break;
				case TypeTowerDefendsData.PosType.pos:
					pos = data.m_Pos.pos;
					break;
				case TypeTowerDefendsData.PosType.npcPos:
					pos = Pathea.EntityMgr.Instance.Get(data.m_Pos.id).position;
					break;
				case TypeTowerDefendsData.PosType.doodadPos:
					pos = Pathea.EntityMgr.Instance.GetDoodadEntities(data.m_Pos.id)[0].position;
					break;
				case TypeTowerDefendsData.PosType.conoly:
					if (!CSMain.GetAssemblyPos(out pos))
						pos = Pathea.PeCreature.Instance.mainPlayer.position;
					break;
				case TypeTowerDefendsData.PosType.camp:
					if (!VArtifactUtil.GetTownPos(data.m_Pos.id, out pos))
						pos = Pathea.PeCreature.Instance.mainPlayer.position;
					break;
				default:
					break;
			}
			data.finallyPos = pos;
		}
	}

    IEnumerator WaitForActivate()
    {
        while (true)
        {
            yield return null;
            _mbEntity = Pathea.EntityMgr.Instance.Get(Id) as EntityMonsterBeacon;
            if (null != _mbEntity)
                break;
		}

        OnActivate();
    }

	void OnNewWave(AISpawnTDWavesData.TDWaveSpData tdData, int wave)
	{
		wave++;

		if (null != tdData)
		{
			float coolTime = 0;
			float preTime = 0;

			if (tdData._waveDatas.Count != wave)
			{
				coolTime = tdData._timeToCool;
				preTime = tdData._waveDatas[wave]._delayTime;
			}

			SyncTDInfo(totalCount, wave, preTime, coolTime);

			if (null != _mbEntity)
				_mbEntity.UpdateUI(_missionId, totalCount, totalWave, preTime);
		}
	}

    void OnNewEntity(SceneEntityPosAgent agent)
    {
        totalCount++;
    }

    IEnumerator CouterCoroutine()
    {
        while (true)
        {
			while (coolTime != 0)
			{
				yield return new WaitForSeconds(1);
				coolTime -= 1;
			}

            MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = Mathf.Clamp(preTime, 0, 1000f);
			yield return new WaitForSeconds(1);
			preTime -= 1;
		}
    }

	protected void RPC_S2C_TDStartInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == PlayerNetwork.mainPlayer || isStart)
			return;

		isStart = true;
		totalWave = stream.Read<int>();
		deathCount = stream.Read<int>();
		int waveIndex = stream.Read<int>();
		preTime = stream.Read<float>();
		coolTime = 0;

		MissionManager.Instance.m_PlayerMission.m_TowerUIData.TotalWaves = totalWave;
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurWavesRemaining = totalWave - waveIndex;
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.bRefurbish = true;
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.MissionID = MissionId;
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = preTime;

		if (UITowerInfo.Instance != null)
		{
			UITowerInfo.Instance.SetInfo(MissionManager.Instance.m_PlayerMission.m_TowerUIData);
			UITowerInfo.Instance.Show();
			StartCoroutine(CouterCoroutine());
		}
	}

	protected void RPC_S2C_ResponseInitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (hasOwnerAuth || null == PlayerNetwork.mainPlayer || isStart)
            return;

		isStart = true;
		totalWave = stream.Read<int>();
        deathCount = stream.Read<int>();
        int waveIndex = stream.Read<int>();
        preTime = stream.Read<float>();
		coolTime = stream.Read<float>();

		MissionManager.Instance.m_PlayerMission.m_TowerUIData.TotalWaves = totalWave;
        MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurWavesRemaining = totalWave - waveIndex;
        MissionManager.Instance.m_PlayerMission.m_TowerUIData.bRefurbish = true;
        MissionManager.Instance.m_PlayerMission.m_TowerUIData.MissionID = MissionId;
        MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = preTime;

		if (UITowerInfo.Instance != null)
        {
            UITowerInfo.Instance.SetInfo(MissionManager.Instance.m_PlayerMission.m_TowerUIData);
            UITowerInfo.Instance.Show();
            StartCoroutine(CouterCoroutine());
        }
    }

	protected void RPC_S2C_TDInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == PlayerNetwork.mainPlayer)
			return;

		totalCount = stream.Read<int>();
		deathCount = stream.Read<int>();
		int waveIndex = stream.Read<int>();
		preTime = stream.Read<float>();
		coolTime = stream.Read<float>();

		MissionManager.Instance.m_PlayerMission.m_TowerUIData.bRefurbish = true;
        MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurWavesRemaining = totalWave - waveIndex;
        MissionManager.Instance.m_PlayerMission.m_TowerUIData.MaxCount = totalCount;
        MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = preTime;
	}

	void RPC_S2C_MonsterDeath(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (hasOwnerAuth || null == PlayerNetwork.mainPlayer)
			return;

		deathCount = stream.Read<int>();
		MissionManager.Instance.m_PlayerMission.m_TowerUIData.CurCount = deathCount;
	}
}
