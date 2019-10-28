#define DestroyLeftMonster
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using SkillSystem;

public class EntityMonsterBeacon : PeEntity
{
	public const int TowerDefenseSpType_Beg = 500;
	const int MonsterBeaconPlayerID = 8;
	const int MonsterBeaconCampID = 26;
	const int MonsterKillBeaconID = -2;
	const float TimeStep = 1f;

	protected int _idxWave = 0;
	protected float _preTime = 0;
	protected List<ISceneObjAgent> _agents = new List<ISceneObjAgent>();

	protected int _campCol = -1;
	protected int _spDataId;
    protected Vector3 _position;
    protected Vector3 _forward;
	protected TowerInfoUIData _uiData = null;
	protected AISpawnTDWavesData.TDWaveSpData _spData = null;
	protected float PreTime{ get { return _preTime; } set { _preTime = value; if(_uiData!=null){_uiData.PreTime = _preTime;} } }

	public Action handlerOneDeath = null;
	public Action<SceneEntityPosAgent> handerNewEntity = null;
	public Action<AISpawnTDWavesData.TDWaveSpData, int> handlerNewWave = null;

    public Vector3 TargetPosition { set { _position = value; } }
	public int CampColor{ get { return _campCol; } }
	public bool IsMonsterKill{ get { return Id == MonsterKillBeaconID; } }
	public AISpawnTDWavesData.TDWaveSpData SpData { get { return _spData; } }

	static EntityMonsterBeacon s_spBeacon = null;
    static List<EntityMonsterBeacon> s_Beacons = new List<EntityMonsterBeacon>();

    bool isSweep = false;
	MonsterAirborne _airborne = null;

	PeMap.MonsterBeaconMark m_Mark;

    public static bool IsRunning() {        return s_Beacons.Count > 0;    }
	public static bool IsBcnMonsterProtoId(int code)
	{
		return (code & EntityProto.IdBcnLvlTypeMask) != 0;
	}
	public static int EncodeBcnMonsterProtoId(int spType, int dif, int spawnType){
		spawnType += 1;
		if (spawnType < 0)						spawnType = 0;
		return Pathea.EntityProto.IdBcnLvlTypeMask | ((dif << 14)&0xffc000) | ((spType<<4)&0x3ff0) | (spawnType&0xf);
	}
	public static void DecodeBcnMonsterProtoId(int code, out int spType, out int dif, out int spawnType){
		spawnType = (code & 0xf) - 1;
		spType = (code & 0x3ff0) >> 4;
		dif = (code & 0xffc000) >> 14;
		if (spType >= TowerDefenseSpType_Beg) {
			dif = -1;
			spawnType = -1;
		}
	}

	public static EntityMonsterBeacon GetSpBeacon4MonsterKillTask()
    {
        if (s_spBeacon == null)
        {
            GameObject go = new GameObject("SpBeacon4MK");
            s_spBeacon = go.AddComponent<EntityMonsterBeacon4Kill>();
			EntityMgr.Instance.AddAfterAssignId(s_spBeacon, MonsterKillBeaconID);
        }
        return s_spBeacon;
    }

	private static List<int> s_spTypes0 = new List<int>(){0};
	private static List<int> s_spTypes01 = new List<int>(){0,1};
	private static List<int> s_spTypes02 = new List<int>(){0,2};
	private static List<int> s_spTypes03 = new List<int>(){0,3};
	private static List<int> GetSpawnTypeMask(bool bOnlyMonster, out int campCol)
	{
		//List<int> typeMask = new List<int>();
		campCol = -1;
		if (bOnlyMonster) {
			return s_spTypes0;
		}

		if (Pathea.PeGameMgr.IsStory) {
			// Monster, Puja or Paja (alliance would be excluded)
			int playerId = (Pathea.PeCreature.Instance.mainPlayer != null) ? ((int)Pathea.PeCreature.Instance.mainPlayer.GetAttribute (Pathea.AttribType.DefaultPlayerID)) : 0;
			bool bExcludePuja = ReputationSystem.Instance.GetReputationLevel (playerId, (int)ReputationSystem.TargetType.Puja) > ReputationSystem.ReputationLevel.Neutral;
			bool bExcludePaja = ReputationSystem.Instance.GetReputationLevel (playerId, (int)ReputationSystem.TargetType.Paja) > ReputationSystem.ReputationLevel.Neutral;
			if(bExcludePuja){
				return s_spTypes02;
			} else if(bExcludePaja){
				return s_spTypes01;
			}
			return UnityEngine.Random.value > 0.5f ? s_spTypes01 : s_spTypes02;
		} else {
			AllyType type = VATownGenerator.Instance.GetRandomExistEnemyType(out campCol);
			switch(type){
			case AllyType.Puja:
				return s_spTypes01;
			case AllyType.Paja:
				return s_spTypes02;
			case AllyType.Npc:
				return s_spTypes03;
			}
		}
		return s_spTypes0;
	}
    public static EntityMonsterBeacon CreateMonsterBeaconByTDID(int spDataId, Transform targetTrans,
        TowerInfoUIData uiData, int entityId = -1, TypeTowerDefendsData data = null, int releaseNpcid = -1, bool bOnlyMonster = false)
    {
		// Get type mask
		int campCol = -1;
		List<int> spawnTypes = GetSpawnTypeMask (bOnlyMonster, out campCol);
		AISpawnTDWavesData.TDWaveSpData spData = AISpawnTDWavesData.GetWaveSpData(spDataId, UnityEngine.Random.value, spawnTypes);
        if (spData == null) return null;

        GameObject go = new GameObject("MonsterBeacon");
        Vector3 v = new Vector3();
        if (null != data)
        {
            switch (data.m_Pos.type)
            {
                case TypeTowerDefendsData.PosType.getPos:
                    v = PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.pos:
                    v = data.m_Pos.pos;
                    break;
                case TypeTowerDefendsData.PosType.npcPos:
                    v = EntityMgr.Instance.Get(data.m_Pos.id).position;
                    break;
                case TypeTowerDefendsData.PosType.doodadPos:
                    v = EntityMgr.Instance.GetDoodadEntities(data.m_Pos.id)[0].position;
                    break;
                case TypeTowerDefendsData.PosType.conoly:
                    if (!CSMain.GetAssemblyPos(out v))
                        v = PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.camp:
                    if (!VArtifactUtil.GetTownPos(data.m_Pos.id, out v))
                        v = PeCreature.Instance.mainPlayer.position;
                    break;
                default:
                    break;
            }
            data.finallyPos = v;

            go.transform.position = v;
            go.transform.rotation = Quaternion.identity;
        }
        else if (targetTrans != null)
        {
            v = targetTrans.position;
            go.transform.position = targetTrans.position;
            go.transform.rotation = targetTrans.rotation;
        }
        EntityMonsterBeacon bcn = go.AddComponent<EntityMonsterBeacon>();
        EntityMgr.Instance.AddAfterAssignId(bcn, entityId != -1 ? entityId : Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId());
		bcn._campCol = campCol;
        bcn._uiData = uiData;
        bcn._spData = spData;
        bcn._spDataId = spDataId;
        bcn._position = go.transform.position;
        bcn._forward = go.transform.forward;
        bcn.PreTime = (float)(spData._timeToStart + spData._waveDatas[0]._delayTime);

        if (UITowerInfo.Instance != null && uiData != null)
        {
            UITowerInfo.Instance.SetInfo(uiData);
            UITowerInfo.Instance.Show();
            UITowerInfo.Instance.e_BtnReady += () => { bcn.PreTime = 0; };
        }

        bcn.StartCoroutine(bcn.RefreshTowerMission());
        return bcn;
    }

    public static bool IsController()
    {
        if (Pathea.PeGameMgr.IsSingle || (AiTowerDefense.mInstance != null && AiTowerDefense.mInstance.hasOwnerAuth))
            return true;
        return false;
    }

    public static EntityMonsterBeacon CreateMonsterBeaconBySweepID(List<int> sweepDataId, Transform targetTrans,
	TowerInfoUIData uiData, int preTime, int entityId = -1,TypeTowerDefendsData data = null, int releaseNpcid = -1)
    {
        GameObject go = new GameObject("MonsterBeacon");

        Vector3 v = new Vector3();
        if (null != data)
        {
            switch (data.m_Pos.type)
            {
                case TypeTowerDefendsData.PosType.getPos:
                    v = PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.pos:
                    v = data.m_Pos.pos;
                    break;
                case TypeTowerDefendsData.PosType.npcPos:
                    v = EntityMgr.Instance.Get(data.m_Pos.id).position;
                    break;
                case TypeTowerDefendsData.PosType.doodadPos:
                    v = EntityMgr.Instance.GetDoodadEntities(data.m_Pos.id)[0].position;
                    break;
                case TypeTowerDefendsData.PosType.conoly:
                    if (!CSMain.GetAssemblyPos(out v))
                        v = PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.camp:
                    if (VArtifactUtil.GetTownPos(data.m_Pos.id, out v))
                        v = PeCreature.Instance.mainPlayer.position;
                    break;
                default:
                    break;
            }
            data.finallyPos = v;
        }
        go.transform.position = v;

        AISpawnTDWavesData.TDWaveSpData spData = MonsterSweepData.GetWaveSpData(sweepDataId, v);
        if (spData == null) return null;

        EntityMonsterBeacon bcn = go.AddComponent<EntityMonsterBeacon>();
        bcn.isSweep = true;
		EntityMgr.Instance.AddAfterAssignId(bcn, entityId != -1 ? entityId : Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId());
        bcn._uiData = uiData;
        bcn._spData = spData;
        bcn._position = v;
        bcn._forward = Vector3.forward;
        bcn.PreTime = (float)(preTime + spData._waveDatas[0]._delayTime);
        if (UITowerInfo.Instance != null && uiData != null)
        {
            UITowerInfo.Instance.SetInfo(uiData);
            UITowerInfo.Instance.Show();
            UITowerInfo.Instance.e_BtnReady += () => { bcn.PreTime = 0; };
        }

        bcn.StartCoroutine(bcn.RefreshTowerMission());
        return bcn;
    }

    void Start()
    {
        if(!(this is EntityMonsterBeacon4Kill))
            s_Beacons.Add(this);

		if(!PeGameMgr.IsMulti && !IsMonsterKill)
		{
			m_Mark = new PeMap.MonsterBeaconMark();
			m_Mark.Position = _position;
            m_Mark.IsMonsterSiege = true;
            PeMap.LabelMgr.Instance.Add(m_Mark);
		}
    }

	void OnDestroy()
	{
		if (_airborne != null) {
			MonsterAirborne.DestroyAirborne(_airborne, false);
		}

#if DestroyLeftMonster
        if (!isSweep)
        {
            for (int i = 0; i < _agents.Count; i++) {
				MonEscape (_agents [i] as SceneEntityPosAgent, transform.position);
			}
        }
#endif
		SceneMan.RemoveSceneObjs(_agents);
		_agents.Clear();

		if (UITowerInfo.Instance != null && _uiData != null) {
			UITowerInfo.Instance.Hide();
		}

        s_Beacons.Remove(this);
		
		if(!PeGameMgr.IsMulti && null != m_Mark)
		{
			PeMap.LabelMgr.Instance.Remove(m_Mark);
			m_Mark = null;
		}

		if(null != CSMain.Instance)
		{
			List<CSAssembly> assemblyList = CSMain.Instance.GetAllAssemblies();
			if(0 < assemblyList.Count)
			{
				for(int i = 0; i < assemblyList.Count; ++i)
				{
					CSAssembly assembly = assemblyList[i];
					if(null != assembly && assembly.InRange(_position))
						DigTerrainManager.ClearColonyBlockInfo(assembly);
				}
			}
		}
    }
	
	void MonEscape(SceneEntityPosAgent agent, Vector3 center) 
	{
		if (agent.entity == null)
			return;
		Vector3 dir = agent.entity.position - center;
		dir.Normalize();
		if (agent.entity.monster != null)
		{
			//RequestCmpt rc = entity.GetCmpt<RequestCmpt>();
			Request moveTo = agent.entity.monster.Req_MoveToPosition(agent.entity.position + (dir * 50), 1, true, SpeedState.Run);
			moveTo.AddRelation(EReqType.Attack, EReqRelation.Block);
		}
		PeLogicGlobal.Instance.DestroyEntity(agent.entity.aliveEntity, 10);
	}

    IEnumerator RefreshTowerMission()
    {
        _idxWave = 0;
        _uiData.CurWavesRemaining = _spData._waveDatas.Count;
        _uiData.TotalWaves = _spData._waveDatas.Count;
        while (_idxWave < _spData._waveDatas.Count)
        {
            while (PreTime > 0)
            {
                yield return new WaitForSeconds(TimeStep);
                PreTime -= TimeStep;
            }
            PreTime = 0;
            Vector3 dir = _forward;
            Vector3 center = _position;
            int m = _idxWave;
            AISpawnTDWavesData.TDWaveData wd = _spData._waveDatas[m];
            if(Pathea.PeGameMgr.IsStory)
                StroyManager.Instance.PushStoryList(wd._plotID);
            int nMonsterTypes = wd._monsterTypes.Count;
            for (int n = 0; n < nMonsterTypes; n++)
            {
                int spType = wd._monsterTypes[n];
                int minAngle = wd._minDegs[n];
                int maxAngle = wd._maxDegs[n];
                int spCount = UnityEngine.Random.Range(wd._minNums[n], wd._maxNums[n]);

                for (int i = 0; i < spCount; i++)
                {
                    Vector3 pos;
					if(spType == 520 || spType == 521){	//Special code for airborne monsters
						pos = center;
					} else {
	                    if (isSweep)
	                    {
	                        pos = AiUtil.GetRandomPosition(center, 80, 100, dir, minAngle, maxAngle);
	                        transform.position = center + ((center - pos) * 1000);
	                    }
	                    else
	                        pos = AiUtil.GetRandomPosition(center, 20, 80, dir, minAngle, maxAngle);
						pos.y = SceneEntityPosAgent.PosYTBD;	// let posagent to set y
					}
					SceneEntityPosAgent agent = MonsterEntityCreator.CreateAgent(pos, EncodeBcnMonsterProtoId(spType, _spData._dif, _spData._spawnType));
                    agent.spInfo = new MonsterEntityCreator.AgentInfo(this);
                    agent.canRide = false;

                    if (handerNewEntity != null) handerNewEntity(agent);

                    _agents.Add(agent);
                    SceneMan.AddSceneObj(agent);
                }
            }
            if (handlerNewWave != null) handlerNewWave(_spData, _idxWave);

            _uiData.CurWavesRemaining--;

            _idxWave++;
            if (_idxWave < _spData._waveDatas.Count)
            {
                int cdTime = _spData._timeToCool;
                while (cdTime > 0)
                {
                    yield return new WaitForSeconds(1);
                    cdTime--;
                }
                PreTime = (float)_spData._waveDatas[_idxWave]._delayTime;
            }
        };
    }

    public virtual void OnMonsterCreated(PeEntity e)
    {
        if (e != null)
        {
            CommonCmpt cc = e.GetCmpt<CommonCmpt>();
            if (cc != null)
            {
                cc.TDObj = gameObject;
                cc.TDpos = gameObject.transform.position;
            }
            SkAliveEntity sae = e.GetCmpt<SkAliveEntity>();
            if (sae != null)
            {
                sae.deathEvent += (t, c) => OnMonsterDeath(e);
                sae.SetAttribute(AttribType.DefaultPlayerID, MonsterBeaconPlayerID);
                sae.SetAttribute(AttribType.CampID, MonsterBeaconCampID);
                LodCmpt lc = e.lodCmpt;
				if (e.lodCmpt != null && PeGameMgr.IsSingle && isSweep)
					e.lodCmpt.onDestroyView = (p) => OnMonsterEdge(lc);
            }
        }
    }

    void OnMonsterEdge(LodCmpt lc) 
    {
        lc.OnDestroy();
        if (_uiData != null) _uiData.CurCount++;
        if (handlerOneDeath != null) handlerOneDeath();
    }

	void OnMonsterDeath(PeEntity e)
	{
		if(_uiData != null) 			_uiData.CurCount++;
		if (handlerOneDeath != null)	handlerOneDeath ();
	}

    public void Delete()
    {
        GameObject.DestroyImmediate(gameObject);
    }

	public void UpdateUI(int missionId, int totalCount, int totalWave, float preTime)
	{
		if (null != _uiData)
		{
            _uiData.MaxCount = totalCount;
			_uiData.TotalWaves = totalWave;
			_uiData.MissionID = missionId;
			_uiData.PreTime = preTime;

			if (UITowerInfo.Instance != null)
			{
				UITowerInfo.Instance.SetInfo(_uiData);
				UITowerInfo.Instance.Show();
				UITowerInfo.Instance.e_BtnReady += () => { PreTime = 0; };
			}
		}
	}
	public void AddAirborneReq(SceneEntityPosAgent agent)
	{
		if (_airborne == null) {
			RaycastHit hinfo;
			Vector3 pos = _position;	// target pos
			int layer = 1 << Pathea.Layer.VFVoxelTerrain
						| 1 << Pathea.Layer.Building
						| 1 << Pathea.Layer.SceneStatic
						| 1 << Pathea.Layer.Unwalkable;
			if(Physics.Raycast(pos + 500.0f*Vector3.up, Vector3.down, out hinfo, 1000.0f, layer)){
				pos = hinfo.point;
			}
			MonsterAirborne.Type type = ((agent.protoId & Pathea.EntityProto.IdAirbornePujaMask) != 0) ? MonsterAirborne.Type.Puja : MonsterAirborne.Type.Paja;
			_airborne = MonsterAirborne.CreateAirborne(pos, type);
		}
		_airborne.AddAirborneReq (agent);
	}
}

public class EntityMonsterBeacon4Kill : EntityMonsterBeacon
{
	public override void OnMonsterCreated(PeEntity e)
	{
		// Do nothing
	}
}


