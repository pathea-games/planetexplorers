using UnityEngine;
using System.Collections;
using Pathea;
using System.Collections.Generic;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtNpcPackage;
using ItemAsset.PackageHelper;
using System;
using SkillSystem;
using System.IO;

public class EntityCreateMgr : MonoBehaviour
{
	public const bool DbgUseLegacyCode = false;
    public const int CreateNpcNum = 3;
    public const int CreateMonsterNum = 5;
    public const int CreateNpcRadius = 128;
    public const int TowerDefencePlayerID = 8;
    public const int TowerDefenceCampID = 26;

    int TowerMissionID;
    int TowerStep;
    int TowerIdxI;
    AISpawnWaveData TowerAiwd;
    bool bTowerStarted = true;

    public struct Min_Max_Int
    {
        public int m_Min;
        public int m_Max;

        public int Random()
        {
            return UnityEngine.Random.Range(m_Min, m_Max);
        }
    }

    public class StoryEntityMgr
    {
		//public bool created = false;
        public IntVec2 m_IdxPos;
		public List<int> m_FixCreateID;
        public List<IntVec3> m_RandomCreatePosMap;
        public Dictionary<EntityType, Dictionary<IntVec3, EntityPosAgent>> m_CreatedEntityMap;
        public StoryEntityMgr()
        {
			m_FixCreateID = new List<int>();
            m_RandomCreatePosMap = new List<IntVec3>();
            m_CreatedEntityMap = new Dictionary<EntityType, Dictionary<IntVec3, EntityPosAgent>>();
        }
		public void Clear()
		{
			m_FixCreateID.Clear();
			m_RandomCreatePosMap.Clear();
			foreach(KeyValuePair<EntityType, Dictionary<IntVec3, EntityPosAgent>> p0 in m_CreatedEntityMap)
			{
				foreach(KeyValuePair<IntVec3, EntityPosAgent> p1 in p0.Value)
				{
					SceneMan.RemoveSceneObj(p1.Value);
				}
			}
		}
    }

    public Dictionary<int, PeEntity> m_TowerDefineMonsterMap;
    public Dictionary<IntVec2, StoryEntityMgr> m_StoryEntityMgr;
	public void RemoveStoryEntityMgr(IntVec2 key)
	{
		if (null == key)
			return;

		StoryEntityMgr m;
		if(m_StoryEntityMgr.TryGetValue(key, out m))
		{
			m.Clear();
			m_StoryEntityMgr.Remove(key);
		}
	}
    public bool npcLoaded = false;
    static EntityCreateMgr mInstance;
    public static EntityCreateMgr Instance
    {
        get
        {
            return mInstance;
        }
    }
    PeTrans m_PlayerTrans = null;
	public Transform mPlayerTrans
	{
		get
		{
			if(m_PlayerTrans == null)
				m_PlayerTrans = PeCreature.Instance.mainPlayer.peTrans;

			return m_PlayerTrans.trans;
		}
	}

    void Awake()
    {
        m_StoryEntityMgr = new Dictionary<IntVec2, StoryEntityMgr>();
        m_TowerDefineMonsterMap = new Dictionary<int, PeEntity>();
        mInstance = this;
    }

    void Update ()
    {
		if (!DbgUseLegacyCode) 			return; 

		/* if true return
		 * 
        if (!EntityCreatedArchiveMgr.m_Finished && !NetworkInterface.IsClient)
            return;

        if (!PeGameMgr.IsStory)
            CreateEntity(CreateNpcNum, CreateMonsterNum, CreateNpcRadius);
        else
            CreateEntity(0, CreateMonsterNum, CreateNpcRadius);
            */
    }

    public void New ()
    {
		if (!DbgUseLegacyCode) 			return; 

		/* if true return
        if (PeGameMgr.IsStory)
            StartCoroutine(InitNpc());
        */
    }

    IEnumerator InitNpc()
    {
        while (PeCreature.Instance.mainPlayer == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (PeGameMgr.IsSingleStory)
        {           
            InitDefaultNpc();

        }
		InitRandomNpc();
        yield return 2;
        npcLoaded = true;
    }

    void InitRandomNpc()
    {
        foreach (KeyValuePair<int, NpcMissionData> iter in NpcMissionDataRepository.dicMissionData)
        {
            if (iter.Value.m_Rnpc_ID == -1)
                continue;

			if(PeGameMgr.IsMulti)
			{
                //PlayerNetwork.MainPlayer.RequestCreateStRdNpc(iter.Key, iter.Value.m_Pos);
			}
			else
			{
				PeEntity entity = PeCreature.Instance.CreateRandomNpc(iter.Value.m_Rnpc_ID, iter.Key, Vector3.zero, Quaternion.identity, Vector3.one);
	            if (null == entity)
	            {
	                //Debug.LogError("create monster with path:" + PeCreature.MonsterPrefabPath);
	                return;
	            }

	            PeTrans view = entity.peTrans;

	            if (null == view)
	            {
	                Debug.LogError("entity has no ViewCmpt");
	                return;
	            }

	            //PeTrans playerView = PeCreature.Instance.mainPlayer.peTrans;
	            view.position = iter.Value.m_Pos;

	            entity.SetUserData(iter.Value);
				entity.SetBirthPos(iter.Value.m_Pos);
	            SetNpcShopIcon(entity);
			}
        }
    }

    void InitDefaultNpc()
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPC");
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NPC_ID")));
            int protoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PrototypeNPC")));

			if(PeGameMgr.IsMulti)
			{
				//NetworkManager.Instance.RPCServer();
			}
			else
			{
				PeEntity entity = PeCreature.Instance.CreateNpc(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
	            if (entity == null)
	                continue;

	            InitNpcWithDb(entity, reader);

	            NpcMissionData nmd = NpcMissionDataRepository.GetMissionData(entity.Id);
	            entity.SetUserData(nmd);
	            SetNpcShopIcon(entity);
			}
        }
    }

    bool InitNpcWithDb(PeEntity entity, Mono.Data.SqliteClient.SqliteDataReader reader)
    {
        //string npcName = reader.GetString(reader.GetOrdinal("NPC_name"));
        //string showName = reader.GetString(reader.GetOrdinal("NPC_showname"));
        //CharacterName characterName = new CharacterName(npcName, showName, CharacterName.DefaultFamilyName);
        ////string npcIcon = reader.GetString(reader.GetOrdinal("NPC_Icon"));
        ////string npcmode = reader.GetString(reader.GetOrdinal("NPC_Model"));

        //EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();
        //if (null != info)
        //{
        //    info.Name = characterName;
        //    //info.FaceIcon = npcIcon;
        //}

        string strTemp = reader.GetString(reader.GetOrdinal("startpoint"));
		if(PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
		{
			strTemp = reader.GetString(reader.GetOrdinal("training_pos"));
			if(strTemp == "0,0,0")
				strTemp = "50000,0,0";
		}
        string[] pos = strTemp.Split(',');
        if (pos.Length < 3)
        {
            Debug.LogError("Npc's StartPoint is Error");
        }
        else
        {
            float x = System.Convert.ToSingle(pos[0]);
            float y = System.Convert.ToSingle(pos[1]);
            float z = System.Convert.ToSingle(pos[2]);

            PeTrans view = entity.peTrans;
            if (null != view)
            {
                view.position = new Vector3(x, y, z);
            }

			NpcCmpt npcCmpt = entity.NpcCmpt;
			if(null != npcCmpt)
			{
				npcCmpt.FixedPointPos = new Vector3(x, y, z);
			}
        }

        //SetModelData(entity, npcmode);

        SetNpcMoney(entity, reader.GetString(reader.GetOrdinal("money")));

        return true;
    }

    public void CreateEntityFromCustom(int id, int proid, string name, int Count, int spawnNum, float scale, int re)
    {

    }

    public void CreateEntity(int NpcNum, int monsterNum, int Radius)
    {
        List<IntVec2> idxPos = new List<IntVec2>();
        bool havePoint = GetMapCreateCenterPosList(NpcNum + monsterNum, Radius, ref idxPos);
        if (!havePoint)
            return;

        if (NetworkInterface.IsClient && null != PlayerNetwork.mainPlayer)
        {
            byte[] binPos = PETools.Serialize.Export((w) =>
            {
                w.Write(NpcNum);
                w.Write(monsterNum);
                w.Write(idxPos.Count);

                foreach (IntVec2 pos in idxPos)
                {
					int index = pos.x << 16;
					index |= pos.y;
                    w.Write(index);
                }
            });

            PlayerNetwork.mainPlayer.SyncSpawnPos(binPos);
        }

        for (int i = 0; i < idxPos.Count; i++)
        {
            if (!m_StoryEntityMgr.ContainsKey(idxPos[i]))
                continue;

            StoryEntityMgr sem = m_StoryEntityMgr[idxPos[i]];
            if (sem == null)
                continue;

			List<int> adnpcList = NpcMissionDataRepository.GetAdRandListByWild(1);
			int n = 0;
			int cnt = sem.m_RandomCreatePosMap.Count;
            for (; n < NpcNum; n++)
            {
                int idx = UnityEngine.Random.Range(1, adnpcList.Count);
				CreateAdRandomNpcMgr(adnpcList[idx], sem.m_RandomCreatePosMap[n], sem);
				/*
				if(CreateAdRandomNpcMgr(adnpcList[idx], sem.m_RandomCreatePosMap[n], sem) && n == 0)
				{
					if(sem.created)
					{
						int aaaaa = 0;
					}
					else
					{
						sem.created = true;
					}
				}
				*/
            }
			for (; n < cnt; n++)
            {
				CreateMonsterMgr(sem.m_RandomCreatePosMap[n], sem);
            }
			// FixPoint monster
			if (PeGameMgr.IsStory)
			{
				foreach(int pointID in sem.m_FixCreateID)
				{
					AISpawnPoint point = AISpawnPoint.s_spawnPointData[pointID];
					CreateFixPosMonsterMgr(point.resId, new IntVec3(point.Position), sem);
				}
			}
        }
    }

    public bool GetMapCreateCenterPosList(int MaxEntityNum, int Radius, ref List<IntVec2> indexPos)
    {
        if (PeCreature.Instance == null) return false;
        if (PeCreature.Instance.mainPlayer == null) return false;

        Vector3 playerpos = PeCreature.Instance.mainPlayer.ExtGetPos();
        int centerX, centerZ, oldX, oldZ, off;
        if (playerpos.x > 0)
            off = Radius;
        else
            off = Radius * -1;
        centerX = (int)(playerpos.x + off) / (Radius * 2);

        if (playerpos.z > 0)
            off = Radius;
        else
            off = Radius * -1;
        centerZ = (int)(playerpos.z + off) / (Radius * 2);

        oldX = centerX;
        oldZ = centerZ;

        StoryEntityMgr sem = null;
        for (int i = -1; i < 2; i++)
        {
            centerX = oldX + i;
            for (int j = -1; j < 2; j++)
            {
                centerZ = oldZ + j;
                IntVec2 idxPos = new IntVec2(centerX, centerZ);
                Vector3 centeridxpos = new Vector3(centerX * (Radius * 2), 0, centerZ * (Radius * 2));

                if (m_StoryEntityMgr.ContainsKey(idxPos))
                {
					//if(idxPos.x == 38 && idxPos.y == 29)
					//{
					//	int aaaa = 0;
					//}
                    //sem = m_StoryEntityMgr[idxPos];
                    //if (sem == null)
                    //    continue;

                    //foreach (KeyValuePair<)
                    //{
                    //}
                    //sem.m_CreatedEntityMap
                    continue;
                }
				//if(idxPos.x == 38 && idxPos.y == 29)
				//{
				//	int aaaa = 0;
				//}

                sem = new StoryEntityMgr();
                sem.m_IdxPos = idxPos;
                for (int m = 0; m < MaxEntityNum; m++)
                {
                    IntVec3 targetpos;
                    if (!PeGameMgr.IsStory)
					{
                        targetpos = GetPatrolPoint(centeridxpos, false);
						sem.m_RandomCreatePosMap.Add(targetpos);
					}
                    else
                    {
                        Vector3 tmpVec = AiUtil.GetRandomPosition(centeridxpos, -400, 400);
                        targetpos = new IntVec3(tmpVec.x, tmpVec.y, tmpVec.z);
						if(null == AIErodeMap.IsInErodeArea2D(tmpVec))
						{
							sem.m_RandomCreatePosMap.Add(targetpos);
						}
                    }
                }
				sem.m_FixCreateID = AISpawnPoint.Find(centeridxpos.x-Radius, centeridxpos.z-Radius, centeridxpos.x+Radius, centeridxpos.z+Radius);

                m_StoryEntityMgr.Add(idxPos, sem);
                if (!indexPos.Contains(idxPos))
                    indexPos.Add(idxPos);
            }
        }

        return indexPos.Count > 0;
    }

    bool CreateAdRandomNpcMgr(int adnpcid, IntVec3 npcPos, StoryEntityMgr sem)
    {
        EntityPosAgent agent = new EntityPosAgent();
        agent.idx = sem.m_IdxPos;
        agent.proid = adnpcid;
        agent.entitytype = EntityType.EntityType_Npc;
        agent.position = new Vector3(npcPos.x, npcPos.y, npcPos.z);

		if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Npc))
		{
			if (!sem.m_CreatedEntityMap[EntityType.EntityType_Npc].ContainsKey(npcPos))
			{
				sem.m_CreatedEntityMap[EntityType.EntityType_Npc][npcPos] = agent;
				SceneMan.AddSceneObj(agent);
				return true;
			}
		}
		else
		{
			Dictionary<IntVec3, EntityPosAgent> tmpDic = new Dictionary<IntVec3, EntityPosAgent>();
			tmpDic.Add(npcPos, agent);
			sem.m_CreatedEntityMap.Add(EntityType.EntityType_Npc, tmpDic);
			SceneMan.AddSceneObj(agent);
			return true;
		}
		return false;
    }
	void CreateFixPosMonsterMgr(int mid, IntVec3 pos, StoryEntityMgr sem)
	{
		EntityPosAgent agent = new EntityPosAgent();
		agent.idx = sem.m_IdxPos;
		agent.proid = mid;
		agent.entitytype = EntityType.EntityType_Monster;
		agent.position = new Vector3(pos.x, pos.y, pos.z);
		
		SceneMan.AddSceneObj(agent);
		if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Monster))
		{
			if (!sem.m_CreatedEntityMap[EntityType.EntityType_Monster].ContainsKey(pos))
				sem.m_CreatedEntityMap[EntityType.EntityType_Monster][pos] = agent;
		}
		else
		{
			Dictionary<IntVec3, EntityPosAgent> tmpDic = new Dictionary<IntVec3, EntityPosAgent>();
			tmpDic.Add(pos, agent);
			sem.m_CreatedEntityMap.Add(EntityType.EntityType_Monster, tmpDic);
		}
	}

	public PeEntity CreateRandomNpc(int proID, Vector3 pos)
    {
		AdNpcData data = NpcMissionDataRepository.GetAdNpcData(proID);
		if (data == null)
            return null;

		if (NetworkInterface.IsClient && !PeGameMgr.IsMultiStory)
		{
            if(proID > 100)
			    NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdNpc(proID, pos));
            else
                NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdMainNpc(proID, pos));
            return null;
		}
		
		int id = WorldInfoMgr.Instance.FetchRecordAutoId();
		PeEntity entity = PeCreature.Instance.CreateRandomNpc(data.mRnpc_ID, id, Vector3.zero, Quaternion.identity, Vector3.one);

        if (null == entity)
            return null;

        NpcMissionData useData = new NpcMissionData();
        useData.m_bRandomNpc = true;
        useData.m_Rnpc_ID = data.mRnpc_ID;
        useData.m_QCID = data.mQC_ID;
        PeTrans view = entity.peTrans;
        if (null == view)
        {
            Debug.LogError("entity has no ViewCmpt");
            return null;
        }

        view.position = pos;
        entity.SetBirthPos(pos);//delete npc need
        int misid = AdRMRepository.GetRandomMission(useData.m_QCID, useData.m_CurMissionGroup);
        if (misid != 0)
            useData.m_RandomMission = misid;

        for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
            useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);

        NpcMissionDataRepository.AddMissionData(data.mID, useData);

        entity.SetUserData(useData);
        //PeMousePickCmpt pmp = entity.GetCmpt<PeMousePickCmpt>();
        //if (pmp == null)
        //    return null;

        //pmp.mousePick.eventor.Subscribe(NpcMouseEventHandler);

        return entity;
    }

    void CreateMonsterMgr(IntVec3 pos, StoryEntityMgr sem)
    {
        EntityPosAgent agent = new EntityPosAgent();
        agent.idx = sem.m_IdxPos;
		agent.proid = -1;
        agent.entitytype = EntityType.EntityType_Monster;
        agent.position = new Vector3(pos.x, pos.y, pos.z);
        SceneMan.AddSceneObj(agent);

        if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Monster))
            sem.m_CreatedEntityMap[EntityType.EntityType_Monster][pos] = agent;
        else
        {
            Dictionary<IntVec3, EntityPosAgent> tmpDic = new Dictionary<IntVec3, EntityPosAgent>();
            tmpDic.Add(pos, agent);
            sem.m_CreatedEntityMap.Add(EntityType.EntityType_Monster, tmpDic);
        }
    }

    int GetMonsterProtoID(Vector3 pos, EntityPosAgent epa)
    {
        int pathID = 0, typeID = 0;
        typeID = (int)AiUtil.GetPointType(pos);
        if (PeGameMgr.IsStory)
        {
			int mapid = PeMappingMgr.Instance.GetAiSpawnMapId(new Vector2(pos.x, pos.z));
			pathID = AISpeciesData.GetRandomAI(AISpawnDataStory.GetAiSpawnData(mapid, typeID));
        }
        else if (PeGameMgr.IsAdventure)
        {
            int mapID = AiUtil.GetMapID(pos);
            int areaID = AiUtil.GetAreaID(pos);
            pathID = AISpawnDataAdvSingle.GetPathID(mapID, areaID, typeID);
        }

		return pathID;
    }

    public PeEntity CreateMonsterInst(Vector3 pos, int proid, EntityPosAgent epa)
    {
		bool save = false;
		if(proid < 0)
		{
			proid = GetMonsterProtoID(pos, epa);
			//save = true;
		}

        if (NetworkInterface.IsClient)
        {
            NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAi(proid, pos, -1, -1,-1));
            return null;
        }

		PeEntity entity = null;
		if(save)
		{
			int autoid = Pathea.WorldInfoMgr.Instance.FetchRecordAutoId();
			entity = PeCreature.Instance.CreateMonster(autoid, proid, Vector3.zero, Quaternion.identity, Vector3.one);
		}
		else
		{
			int noid = Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId();
			entity = PeEntityCreator.Instance.CreateMonster(noid, proid, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		if (epa.entitytype == EntityType.EntityType_MonsterTD)
        {
			SkAliveEntity sae = entity.GetCmpt<SkAliveEntity>();
			if (sae == null)
				return null;

            CommonCmpt cc = entity.GetCmpt<CommonCmpt>();
            if (cc != null)
                cc.TDObj = GameObject.Find("TowerMission");

            sae.SetAttribute(AttribType.DefaultPlayerID, TowerDefencePlayerID);
            sae.SetAttribute(AttribType.CampID, TowerDefenceCampID);

            if (!m_TowerDefineMonsterMap.ContainsKey(entity.Id))
                m_TowerDefineMonsterMap.Add(entity.Id, entity);

            entity.GetGameObject().name = "MisMonster";
        }

        if (null == entity)
        {
            Debug.LogError("create monster error");
            return null;
        }

		epa.createdid = entity.Id;
		LodCmpt lc = entity.lodCmpt;
		if(lc != null)
		{
			lc.onDeactivate = (e)=>{epa.DestroyEntity();};
		}

        PeTrans view = entity.peTrans;
        if (null == view)
        {
            Debug.LogError("entity has no ViewCmpt");
            return null;
        }

        Pathea.SkAliveEntity skAlive = entity.GetCmpt<Pathea.SkAliveEntity>();
        if (skAlive == null)
        {
            Debug.LogError("entity has no SkAliveEntity");
            return null;
        }

        skAlive.deathEvent += MonsterDeath;
		pos.y += MonsterProtoDb.Get(proid).hOffset;
        view.position = pos;        
        return entity;
    }

	public void MonsterDeath(SkEntity self, SkEntity caster)
    {
		SkAliveEntity skAlive = self as SkAliveEntity;
		{
			CommonCmpt cc = skAlive.Entity.GetCmpt<CommonCmpt>();
	        if (cc == null)
	            return;

	        Debug.Log(cc.entityProto.protoId);
	        if (MissionManager.Instance == null)
	            return;
        
			MissionManager.Instance.ProcessMonsterDead(cc.entityProto.protoId, skAlive.Entity.Id);
		}
    }

#region MissionInfo
    public bool CreateMisMonster(Vector3 center, float radius, int proid, int num)
    {
        for (int i = 0; i < num; i++)
        {
            EntityPosAgent agent = new EntityPosAgent();
            agent.entitytype = EntityType.EntityType_Monster;
            agent.position = AiUtil.GetRandomPosition(center, 0, radius);
            agent.bMission = true;
            agent.proid = proid;
            SceneMan.AddSceneObj(agent);
        }
         
        return true;
    }

    public void StartTowerMission(int MissionID, int step, TypeTowerDefendsData towerData, float time = 0)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        int idxI = -1;
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            if (data.m_TargetIDList[i] == towerData.m_TargetID)
            {
                idxI = i;
                break;
            }
        }

        if (idxI == -1)
            return;

        AISpawnAutomatic aisa = AISpawnAutomatic.GetAutomatic(towerData.m_TdInfoId);
        if (aisa == null)
            return;

        if (aisa.data.Count <= step)
            return;

        AISpawnWaveData aiwd = aisa.data[step];
        if (aiwd == null)
            return;

        if (aiwd.data.data.Count == 0)
            return;

        int idx = idxI * 100 + step * 10 + 0;
		string tmp = PlayerMission.MissionFlagTDMonster + idx;
        string value = MissionManager.Instance.GetQuestVariable(MissionID, tmp);
        string[] tmplist = value.Split('_');
        if (tmplist.Length != 5)
            return;

        int createdMon = Convert.ToInt32(tmplist[3]);
        if (createdMon == 1)
            return;

        MissionManager.mTowerCurWave = (step + 1).ToString();
        MissionManager.mTowerTotalWave = aisa.data.Count.ToString();

        float delaytime = aiwd.delayTime;
        if (time > 0)
            delaytime = time;

        MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = delaytime;

        TowerMissionID = MissionID;
        TowerStep = step;
        TowerIdxI = idxI;
        TowerAiwd = aiwd;
        bTowerStarted = false;
        StartCoroutine(WaitingTowerStart(MissionID, delaytime, step, idxI, aiwd, false));
		UITowerInfo.Instance.Show();
        UITowerInfo.Instance.e_BtnReady += ImmediatelyStartTower;
	}

    public IEnumerator WaitingTowerStart(int MissionID, float time, int step, int idxI, AISpawnWaveData aiwd, bool bCom)
	{
        float delaytime = 0;
        while (time > delaytime && !bTowerStarted)
        {
            yield return new WaitForSeconds(1);

            delaytime += 1;
            MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime -= 1;
            if (MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime < 0)
                MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = 0;
        }

        CreateTowerDefineMonster(MissionID, step, idxI, aiwd, bCom);
    }

    public void ImmediatelyStartTower()
    {
        if (bTowerStarted)
            return;

        CreateTowerDefineMonster(TowerMissionID, TowerStep, TowerIdxI, TowerAiwd, false);
    }

    public void CreateTowerDefineMonster(int MissionID, int step, int idxI, AISpawnWaveData aiwd, bool bCom)
	{
        if (bTowerStarted)
            return;

        MissionManager.Instance.m_PlayerMission.m_TowerUIData.PreTime = 0;
        Vector3 center = GetPlayerPos();
        Vector3 dir = GetPlayerDir();
        for (int i = 0; i < aiwd.data.data.Count; i++)
        {
            AISpawnData aisd = aiwd.data.data[i];
            if (aisd == null)
                continue;

            int idx = idxI * 100 + step * 10 + i;
			string value = MissionManager.Instance.GetQuestVariable(MissionID, PlayerMission.MissionFlagTDMonster + idx);
            string[] tmplist = value.Split('_');
            if (tmplist.Length != 5)
                return;

            //int num = Convert.ToInt32(tmplist[1]);
            int count = Convert.ToInt32(tmplist[2]);

            //count = count - num;

            for (int m = 0; m < count; m++)
            {
                EntityPosAgent agent = new EntityPosAgent();
				agent.entitytype = EntityType.EntityType_MonsterTD;
                agent.position = AiUtil.GetRandomPosition(center, 0, 45, dir, aisd.minAngle, aisd.maxAngle);
                agent.bMission = true;
                agent.proid = aisd.spID;
                SceneMan.AddSceneObj(agent);
            }

            if (!bCom)
            {
                string createmon = "_1";
                value = tmplist[0] + "_" + tmplist[1] + "_" + tmplist[2] + createmon + "_" + tmplist[4];
				MissionManager.Instance.ModifyQuestVariable(MissionID, PlayerMission.MissionFlagTDMonster + idx, value);
            }
        }

        bTowerStarted = true;
    }
#endregion



#region NpcInfo
    public void SetNpcShopIcon(PeEntity npc)
    {
        string icon = StoreRepository.GetStoreNpcIcon(npc.Id);
        if (icon == "0")
            return;

        npc.SetShopIcon(icon);
    }

    //void SetModelData(PeEntity entity, string name)
    //{
    //    AvatarCmpt avatar = entity.GetCmpt<AvatarCmpt>();

    //    CustomCharactor.AvatarData nudeAvatarData = new CustomCharactor.AvatarData();

    //    nudeAvatarData.SetPart(CustomCharactor.AvatarData.ESlot.HairF, "Model/Npc/" + name);

    //    avatar.SetData(new AppearBlendShape.AppearData(), nudeAvatarData);
    //}

    public void SetNpcMoney(PeEntity entity, string text)
    {
        NpcPackageCmpt pkg = entity.GetCmpt<NpcPackageCmpt>();

        string[] groupStrArray = text.Split(';');
        if (groupStrArray.Length != 3)
        {
            return;
        }

        string[] strArray = groupStrArray[0].Split(',');
        if (strArray.Length != 2)
        {
            return;
        }

        Min_Max_Int initValue;
        if (!int.TryParse(strArray[0], out initValue.m_Min))
        {
            return;
        }
        if (!int.TryParse(strArray[1], out initValue.m_Max))
        {
            return;
        }

        strArray = groupStrArray[1].Split(',');
        if (strArray.Length != 2)
        {
            return;
        }

        Min_Max_Int incValue;
        if (!int.TryParse(strArray[0], out incValue.m_Min))
        {
            return;
        }
        if (!int.TryParse(strArray[1], out incValue.m_Max))
        {
            return;
        }

        int max = 0;
        if (!int.TryParse(groupStrArray[2], out max))
        {
            return;
        }

        pkg.InitAutoIncreaseMoney(max, incValue.Random());

        pkg.money.current = initValue.Random();
    }
#endregion


#region SaveEntityInfo
	public void SaveEntityCreated(BinaryWriter bw)
    {
        bw.Write(m_StoryEntityMgr.Count);
        foreach (KeyValuePair<IntVec2, StoryEntityMgr> ite in m_StoryEntityMgr)
        {
            StoryEntityMgr sem = ite.Value;
            if (sem == null)
                continue;

            bw.Write(ite.Key.x);
            bw.Write(ite.Key.y);

            bw.Write(sem.m_CreatedEntityMap.Count);
            foreach(KeyValuePair<EntityType, Dictionary<IntVec3, EntityPosAgent>> iteEntityMap in sem.m_CreatedEntityMap)
            {
                bw.Write((int)iteEntityMap.Key);
                bw.Write(iteEntityMap.Value.Count);
                foreach (KeyValuePair<IntVec3, EntityPosAgent> iteChild in iteEntityMap.Value)
                {
                    EntityPosAgent epa = iteChild.Value;
                    if (epa == null)
                        continue;

                    if (epa.createdid == 0)
                    {
                        bw.Write(false);
                        continue;
                    }

                    bw.Write(true);
                    bw.Write(iteChild.Key.x);
                    bw.Write(iteChild.Key.y);
                    bw.Write(iteChild.Key.z);
                    bw.Write(epa.proid);
                    PETools.Serialize.WriteVector3(bw, epa.position);
                }
            }
        }
    }

    public void ReadEntityCreated(byte[] buffer)
    {
        if (buffer == null)
            return;

        if (buffer.Length == 0)
            return;

        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);

		foreach(KeyValuePair<IntVec2, StoryEntityMgr> pair in m_StoryEntityMgr)
		{
			pair.Value.Clear();
		}
        m_StoryEntityMgr.Clear();

        int iMgrSize = _in.ReadInt32();
        for (int i = 0; i < iMgrSize; i++)
        {
            int x = _in.ReadInt32();
            int y = _in.ReadInt32();
            IntVec2 iv2 = new IntVec2(x, y);

            int iEntityMapCount = _in.ReadInt32();
            for (int m = 0; m < iEntityMapCount; m++)
            {
                EntityType et = (EntityType)_in.ReadInt32();
                int size = _in.ReadInt32();
                for (int j = 0; j < size; j++)
                {
                    if (_in.ReadBoolean() == false)
                        continue;

                    x = _in.ReadInt32();
                    y = _in.ReadInt32();
                    int z = _in.ReadInt32();
                    IntVec3 iv3 = new IntVec3(x, y, z);

                    EntityPosAgent epa = new EntityPosAgent();
                    epa.idx = iv2;
                    epa.proid = _in.ReadInt32();
                    epa.entitytype = et;
                    epa.position = PETools.Serialize.ReadVector3(_in);

                    Dictionary<IntVec3, EntityPosAgent> tmpDic = new Dictionary<IntVec3, EntityPosAgent>();
                    tmpDic.Add(iv3, epa);

                    StoryEntityMgr sem = new StoryEntityMgr();
                    sem.m_IdxPos = iv2;
                    sem.m_CreatedEntityMap.Add(et, tmpDic);
                }
            }
        }

        _in.Close();
        ms.Close();
    }

    void SaveNpcMissionData(int npcID, NpcMissionData data, BinaryWriter _out)
    {
        if (data == null)
        {
            _out.Write(-1);
            _out.Write(npcID);
            //_out.Write(EntityMgr.Instance.Get(npcID).GetComponent<NpcCmpt>().IsFollower);
        }
        else
        {
            _out.Write(npcID);
            _out.Write(data.m_Rnpc_ID);
            _out.Write(data.m_QCID);
            PETools.Serialize.WriteVector3(_out, data.m_Pos);
            _out.Write(data.m_CurMissionGroup);
            _out.Write(data.m_CurGroupTimes);
            _out.Write(data.mCurComMisNum);
            _out.Write(data.mCompletedMissionCount);
            _out.Write(data.m_RandomMission);
            _out.Write(data.m_RecruitMissionNum);
            _out.Write(data.m_bRandomNpc);
            _out.Write(data.m_bColonyOrder);
            _out.Write(data.mInFollowMission);

            _out.Write(data.m_MissionList.Count);
            for (int m = 0; m < data.m_MissionList.Count; m++)
                _out.Write(data.m_MissionList[m]);

            _out.Write(data.m_MissionListReply.Count);
            for (int m = 0; m < data.m_MissionListReply.Count; m++)
                _out.Write(data.m_MissionListReply[m]);

            _out.Write(data.m_RecruitMissionList.Count);
            for (int m = 0; m < data.m_RecruitMissionList.Count; m++)
                _out.Write(data.m_RecruitMissionList[m]);

            _out.Write(data.m_CSRecruitMissionList.Count);
            for (int m = 0; m < data.m_CSRecruitMissionList.Count; m++)
                _out.Write(data.m_CSRecruitMissionList[m]);
        }
    }

	public void Export(BinaryWriter bw)
    {
        int versionNum = 0;
		bw.Write(versionNum);
        bw.Write(NpcMissionDataRepository.dicMissionData.Count);
        foreach (KeyValuePair<int, NpcMissionData> iter in NpcMissionDataRepository.dicMissionData)
        {
            if (iter.Value.m_Rnpc_ID == -1)
            {
                bw.Write(-2);
                bw.Write(iter.Key);

                bw.Write(iter.Value.m_MissionList.Count);
                for (int m = 0; m < iter.Value.m_MissionList.Count; m++)
                    bw.Write(iter.Value.m_MissionList[m]);

                bw.Write(iter.Value.m_MissionListReply.Count);
                for (int m = 0; m < iter.Value.m_MissionListReply.Count; m++)
                    bw.Write(iter.Value.m_MissionListReply[m]);

                bw.Write(iter.Value.m_RecruitMissionList.Count);
                for (int m = 0; m < iter.Value.m_RecruitMissionList.Count; m++)
                    bw.Write(iter.Value.m_RecruitMissionList[m]);

                bw.Write(iter.Value.m_CSRecruitMissionList.Count);
                for (int m = 0; m < iter.Value.m_CSRecruitMissionList.Count; m++)
                    bw.Write(iter.Value.m_CSRecruitMissionList[m]);
                continue;
            }

            SaveNpcMissionData(iter.Key,iter.Value, bw);
        }
    }

    void ReadNpcMissionData(NpcMissionData data, BinaryReader _in)
    {
        data.m_MissionList.Clear();
        data.m_MissionListReply.Clear();
        data.m_CSRecruitMissionList.Clear();
        data.m_RecruitMissionList.Clear();

        data.m_Rnpc_ID = _in.ReadInt32();
        if (data.m_Rnpc_ID == -1)
            return;

        data.m_QCID = _in.ReadInt32();
        data.m_Pos = PETools.Serialize.ReadVector3(_in);

        data.m_CurMissionGroup = _in.ReadInt32();
        data.m_CurGroupTimes = _in.ReadInt32();
        data.mCurComMisNum = _in.ReadByte();
        data.mCompletedMissionCount = _in.ReadInt32();
        data.m_RandomMission = _in.ReadInt32();
        data.m_RecruitMissionNum = _in.ReadInt32();
        data.m_bRandomNpc = _in.ReadBoolean();
        data.m_bColonyOrder = _in.ReadBoolean();
        data.mInFollowMission = _in.ReadBoolean();

        int num = _in.ReadInt32();
        for (int j = 0; j < num; j++)
            data.m_MissionList.Add(_in.ReadInt32());

        num = _in.ReadInt32();
        for (int j = 0; j < num; j++)
            data.m_MissionListReply.Add(_in.ReadInt32());

        num = _in.ReadInt32();
        for (int j = 0; j < num; j++)
            data.m_RecruitMissionList.Add(_in.ReadInt32());

        num = _in.ReadInt32();
        for (int j = 0; j < num; j++)
            data.m_CSRecruitMissionList.Add(_in.ReadInt32());
    }

    public void Import(byte[] buffer)
    {
        //m_CreatedNpcList.Clear();
        if (buffer == null)
            return;

        if (buffer.Length == 0)
            return;

        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);

        int version = Pathea.ArchiveMgr.Instance.GetCurArvhiveVersion();

        if (version <= Pathea.Archive.Header.Version_0)
        {
            int iSize = _in.ReadInt32();
            int smaller = iSize < NpcMissionDataRepository.dicMissionData.Count ? iSize : NpcMissionDataRepository.dicMissionData.Count;
            PeEntity npc = null;
            //if (iSize != NpcMissionDataRepository.dicMissionData.Count)
            //    return;
            //PeEntity npc = null;
            foreach (KeyValuePair<int, NpcMissionData> iter in NpcMissionDataRepository.dicMissionData)
            {
                smaller--;
                if (smaller <= -1)
                    break;
                if (iter.Value.m_Rnpc_ID == -1)
                    npc = EntityMgr.Instance.Get(iter.Key);
                else
                {
                    ReadNpcMissionData(iter.Value, _in);
                    npc = EntityMgr.Instance.Get(iter.Key);
                }

                if (npc == null)
                    continue;

                npc.SetUserData(iter.Value);
            }
        }
        else
        {
            /*int versionNum = */_in.ReadInt32();     //目前未用
            int iSize = _in.ReadInt32();
            NpcMissionDataRepository.dicMissionData.Clear();

            PeEntity npc = null;
            NpcMissionData tmp = null;
            for (int i = 0; i < iSize; i++)
            {
                int firstByte = _in.ReadInt32();
                if (firstByte == -2)
                {
                    tmp = new NpcMissionData();
                    tmp.m_Rnpc_ID = -1;
                    int npcID = _in.ReadInt32();
                    npc = EntityMgr.Instance.Get(npcID);

                    int num = _in.ReadInt32();
                    for (int j = 0; j < num; j++)
                        tmp.m_MissionList.Add(_in.ReadInt32());

                    num = _in.ReadInt32();
                    for (int j = 0; j < num; j++)
                        tmp.m_MissionListReply.Add(_in.ReadInt32());

                    num = _in.ReadInt32();
                    for (int j = 0; j < num; j++)
                        tmp.m_RecruitMissionList.Add(_in.ReadInt32());

                    num = _in.ReadInt32();
                    for (int j = 0; j < num; j++)
                        tmp.m_CSRecruitMissionList.Add(_in.ReadInt32());

					NpcMissionDataRepository.AddMissionData(npcID, tmp);
                }
                else if (firstByte == -1)
                {
                    int npcID = _in.ReadInt32();
                    NpcMissionDataRepository.AddMissionData(npcID, null);
                    continue;
                }
                else
                {
                    tmp = new NpcMissionData();
                    int npcID = firstByte;
                    ReadNpcMissionData(tmp, _in);
                    NpcMissionDataRepository.AddMissionData(npcID, tmp);
					npc = EntityMgr.Instance.Get(npcID);
                }
                if (npc == null)
                    continue;
                npc.SetUserData(tmp);
            }
        }

        _in.Close();
        ms.Close();
    }
    #endregion

#region EntityEvent
    public void NpcMouseEventHandler(object sender, EntityMgr.RMouseClickEntityEvent e)
    {
        PeEntity npc = e.entity;
        if (npc == null)
            return;

        float dist = Vector3.Distance(npc.position, GetPlayerPos());
        if (dist > GameConfig.NPCControlDistance)
            return;

        if (IsRandomNpc(npc) && npc.IsDead())
        {
            if (npc.Id == 9203 || npc.Id == 9204)
                return;

            if (npc.Id == 9214 || npc.Id == 9215)
            {
                if (!MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID10))
                    return;
            }

            if (GameConfig.IsMultiMode)
            {
                //if (null != PlayerFactory.mMainPlayer)
                //    PlayerFactory.mMainPlayer.RequestDeadObjItem(npc.OwnerView);
            }
            else
            {
                //if (GameUI.Instance.mItemGetGui.UpdateItem(npc))
                //{
                //    GameUI.Instance.mItemGetGui.Show();
                //}
            }

            if (npc.IsRecruited())
            {
                GameUI.Instance.mRevive.ShowServantRevive(npc);
            }

            return;
        }
    }

	public void NpcTalkRequest(object sender, EntityMgr.NPCTalkEvent e)
	{
		PeEntity npc = e.entity;
		if (npc == null) return;

        if (npc.proto != EEntityProto.Npc && npc.proto != EEntityProto.RandomNpc)
            return;
		
		float dist = Vector3.Distance(npc.position, GetPlayerPos());
		if (dist > 2f * GameConfig.NPCControlDistance) return;

		if (IsRandomNpc(npc) && npc.IsDead()) return;
		
		if (IsRandomNpc(npc) && npc.IsFollower()) return;
		
		if (!npc.GetTalkEnable()) return;
		
		if (IsRandomNpc(npc) && !npc.IsDead())
		{
			NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
			if (null != missionData && 0 != missionData.m_RandomMission)
			{
				if (!MissionManager.Instance.HasMission(missionData.m_RandomMission))
				{
					if (PeGameMgr.IsSingleStory)
						RMRepository.CreateRandomMission(missionData.m_RandomMission,missionData.mCurComMisNum);
					else if (PeGameMgr.IsMultiAdventure || PeGameMgr.IsMultiBuild || PeGameMgr.IsMultiStory)
						PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, missionData.m_RandomMission,npc.Id);
					else
						AdRMRepository.CreateRandomMission(missionData.m_RandomMission);
				}
			}
		}
		if (!npc.IsDead() && npc.NpcCmpt.CanTalk && !GameUI.Instance.mNPCTalk.isPlayingTalk)
		{
			if (GameUI.Instance.mNpcWnd.IsOpen())
				StroyManager.Instance.RemoveReq(GameUI.Instance.mNpcWnd.m_CurSelNpc, EReqType.Dialogue);
			GameUI.Instance.mNpcWnd.SetCurSelNpc(npc, true);
			GameUI.Instance.mNpcWnd.Show();
		}
	}
#endregion
    public IntVec3 GetPatrolPoint(Vector3 center, bool bCheck = true)
    {
        Vector2 v2 = UnityEngine.Random.insideUnitCircle;

        while (true)
        {
            v2 = v2.normalized * UnityEngine.Random.Range(-400, 400);
            Vector3 pos = center + new Vector3(v2.x, 0.0f, v2.y);

            IntVector2 iv = new IntVector2((int)pos.x, (int)pos.z);
            bool bTmp = false;

            pos.y = VFDataRTGen.GetPosTop(iv, out bTmp);

            if (pos.y > -1.01f && pos.y < -0.99f)
                continue;

            if (!bTmp)
                continue;


            return new IntVec3(pos.x, pos.y, pos.z);
        }

    }

    public Vector3 GetPlayerPos()
    {
        if (mPlayerTrans == null)
            return Vector3.zero;

        return m_PlayerTrans.position;
    }

    public Vector3 GetPlayerDir()
    {
        if (mPlayerTrans == null)
            return Vector3.zero;

        return m_PlayerTrans.forward;
    }

    public Transform GetPlayerTrans()
    {
		return mPlayerTrans;
    }

    public bool IsRandomNpc(PeEntity npc)
    {
        if (npc == null)
            return false;

        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
        if (missionData == null)
            return false;

        return missionData.m_bRandomNpc;
    }
}

public class EntityPosAgent : ISceneObjAgent
{
    int m_Id;
    int m_EntityProID;
    int m_EntityCreatedID;
    Vector3 m_Pos;
    EntityType m_EntityType;
    IntVec2 m_Idx;
    //bool bCreate = false;
    public bool bMission = false;
    public int m_MissionStep;
    public int id{						get{            return m_Id;				}
        						private set{            m_Id = value;				}	}
    public int ScenarioId { get; set; }
    public int proid{					get{            return m_EntityProID;		}
        								set{            m_EntityProID = value;		}	}
    public int createdid{				get{            return m_EntityCreatedID;	}
        								set{            m_EntityCreatedID = value;	}	}
    public EntityType entitytype{		get{            return m_EntityType;		}
        								set{            m_EntityType = value;		}	}
    public IntVec2 idx{					get{            return m_Idx;				}
        								set{            m_Idx = value;				}	}
    public Vector3 position{        	get{            return m_Pos;				}
        								set{            m_Pos = value;				}	}
	int ISceneObjAgent.Id { 			get; set; }
	GameObject ISceneObjAgent.Go { 		get{ 			return null; 				}	}
    Vector3 ISceneObjAgent.Pos{			get{            return position;			}	}	
	IBoundInScene ISceneObjAgent.Bound{	get{ 			return null;				}	}
	bool ISceneObjAgent.NeedToActivate{	get{            return true;				}   }
	bool ISceneObjAgent.TstYOnActivate{	get{            return false;				}	}
    void ISceneObjAgent.OnConstruct(){}

    void ISceneObjAgent.OnActivate()
    {
		if(createdid <= 0)
		{
        	VFVoxelTerrain.self.StartCoroutine(CreateEntity());
		}
    }

    void ISceneObjAgent.OnDeactivate(){}

    void ISceneObjAgent.OnDestruct()
    {
        //if (m_EntityType != EntityType.EntityType_Monster)
        //    return;

		if(createdid != 0)
		{
	        PeCreature.Instance.Destory(createdid);
	        createdid = 0;
		}

        EntityCreateMgr.Instance.RemoveStoryEntityMgr(m_Idx);
    }

	public void DestroyEntity()
	{
		if(createdid != 0)
		{
			PeCreature.Instance.Destory(createdid);
			createdid = 0;
		}
	}

    IEnumerator CreateEntity()
    {
        LayerMask layer = 1 << Pathea.Layer.VFVoxelTerrain |
                          1 << Pathea.Layer.SceneStatic |
                          1 << Pathea.Layer.Unwalkable;
        Vector3 pos = new Vector3(m_Pos.x, 600, m_Pos.z);
        RaycastHit hitInfo;

        while (true)
        {
            if (Physics.Raycast(pos, Vector3.down, out hitInfo, 600, layer))
            {
                m_Pos.y = (float)Mathf.Floor(hitInfo.point.y) + 1f;
                if (m_EntityType == EntityType.EntityType_Npc)
				{
                    EntityCreateMgr.Instance.CreateRandomNpc(m_EntityProID, m_Pos);
					SceneMan.RemoveSceneObj(this);
				}
                else
                {
                    EntityCreateMgr.Instance.CreateMonsterInst(m_Pos, proid, this);                    
                }

                break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}


public class NpcUserDataArchiveMgr : ArchivableSingleton<NpcUserDataArchiveMgr>
{
    const string ArchiveKey = "ArchiveKeyNpcUserData";

    protected override bool GetYird()
    {
        return false;
    }

    protected override string GetArchiveKey()
    {
        return ArchiveKey;
    }

    protected override void SetData(byte[] data)
    {
        if (data == null)
            return;

        EntityCreateMgr.Instance.Import(data);
    }

    protected override void WriteData(BinaryWriter bw)
    {
        if (PeCreature.Instance.mainPlayer == null)
            return;

        EntityCreateMgr.Instance.Export(bw);
    }
}

public class EntityCreatedArchiveMgr : ArchivableSingleton<EntityCreatedArchiveMgr>
{
    const string ArchiveKey = "ArchiveKeyEntityCreated";
    public static bool m_Finished = false;

    public override void New()
    {
        base.New();
        m_Finished = true;
    }

    protected override bool GetYird()
    {
        return false;
    }

    protected override string GetArchiveKey()
    {
        return ArchiveKey;
    }

    protected override void SetData(byte[] data)
    {
        if (data == null)
            return;

        EntityCreateMgr.Instance.ReadEntityCreated(data);
        m_Finished = true;
    }

    protected override void WriteData(BinaryWriter bw)
    {
        if (PeCreature.Instance.mainPlayer == null)
            return;

        EntityCreateMgr.Instance.SaveEntityCreated(bw);
    }
}