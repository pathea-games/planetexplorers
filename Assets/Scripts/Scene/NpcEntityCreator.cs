using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using System.Collections;

public class NpcEntityCreator
{
	public class AgentInfo : SceneEntityPosAgent.AgentInfo
	{
		public static new AgentInfo s_defAgentInfo = new AgentInfo ();
		public override void OnSuceededToCreate (SceneEntityPosAgent agent)
		{
			SceneMan.RemoveSceneObj (agent);
		}
	}

	public static void Init()
	{
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent agent = new SceneEntityPosAgent(pos, scl, rot, EntityType.EntityType_Npc, protoId);
		agent.spInfo = AgentInfo.s_defAgentInfo;
		return agent;
	}
	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId = -1)
	{
		SceneEntityPosAgent agent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Npc, protoId);
		agent.spInfo = AgentInfo.s_defAgentInfo;
		return agent;
	}
	
    public static PeEntity CreateNpc(int protoId, Vector3 pos)
    {
		SceneEntityPosAgent agent = CreateAgent(pos, protoId);
        CreateNpc(agent);
        return agent.entity;
    }
    public static PeEntity CreateNpc(int protoId, Vector3 pos, Vector3 scl, Quaternion rot)
    {
		SceneEntityPosAgent agent = CreateAgent(pos, protoId, scl, rot);
        CreateNpc(agent);
        return agent.entity;
    }
    public static void CreateNpc(SceneEntityPosAgent agent)
    {
        agent.entity = null;
        if (agent.protoId < 0)
        {
            List<int> adnpcList = NpcMissionDataRepository.GetAdRandListByWild(1);
            int idx = UnityEngine.Random.Range(0, adnpcList.Count);
            agent.protoId = adnpcList[idx];
        }

        AdNpcData data = NpcMissionDataRepository.GetAdNpcData(agent.protoId);
        if (data == null)
            return;

        if (NetworkInterface.IsClient && !PeGameMgr.IsMultiStory)
        {
            if(agent.protoId > 100)
                NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdNpc(agent.protoId, agent.Pos));
            else
                NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAdMainNpc(agent.protoId, agent.Pos));
            return;
		}
		int id = WorldInfoMgr.Instance.FetchRecordAutoId();
//		agent.entity = PeCreature.Instance.CreateRandomNpc(data.mRnpc_ID, data.mID, agent.Pos, agent.Rot, agent.Scl);
		agent.entity = PeCreature.Instance.CreateRandomNpc(data.mRnpc_ID, id, agent.Pos, agent.Rot, agent.Scl);
        if (null == agent.entity)
        {
            Debug.LogError("[SceneEntityCreator]Failed to create npc:" + agent.protoId);
            return;
        }
        if (MissionManager.Instance && agent.protoId < 100 && !MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(agent.protoId))
            MissionManager.Instance.m_PlayerMission.adId_entityId[agent.protoId] = agent.entity.Id;

        agent.entity.SetBirthPos(agent.Pos);//delete npc need

        NpcMissionData useData = new NpcMissionData();
        useData.m_bRandomNpc = true;
        useData.m_Rnpc_ID = data.mRnpc_ID;
        useData.m_QCID = data.mQC_ID;
        int misid = AdRMRepository.GetRandomMission(useData.m_QCID, useData.m_CurMissionGroup);
        if (misid != 0)
            useData.m_RandomMission = misid;
        for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
            useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);
        NpcMissionDataRepository.AddMissionData(agent.entity.Id, useData);
        agent.entity.SetUserData(useData);
        return;
    }
    public static void CreateStoryRandNpc()	// Fixed pos random npc
    {
        foreach (KeyValuePair<int, NpcMissionData> iter in NpcMissionDataRepository.dicMissionData)
        {
            if (iter.Value.m_Rnpc_ID == -1)
                continue;
            if (PeGameMgr.IsMulti)
            {
                return;
                //PlayerNetwork.MainPlayer.RequestCreateStRdNpc(iter.Key, iter.Value.m_Pos,iter.Value.m_Rnpc_ID);
                //continue;
            }
			PeEntity entity = PeCreature.Instance.CreateRandomNpc(iter.Value.m_Rnpc_ID, iter.Key, iter.Value.m_Pos, Quaternion.identity, Vector3.one);
            if (null == entity)
            {
                //Debug.LogError("create monster with path:" + PeCreature.MonsterPrefabPath);
                return;
            }
            entity.SetUserData(iter.Value);
			entity.SetBirthPos(iter.Value.m_Pos);
            SetNpcShopIcon(entity);
        }
    }

    public static void CreateStoryLineNpcFromID(int npcID, Vector3 position)
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPC");
        NpcMissionDataRepository.Reset();
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NPC_ID")));
            int protoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PrototypeNPC")));

            if (id == npcID)
            {
                PeEntity entity = PeCreature.Instance.CreateNpc(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
                if (entity == null)
                    continue;

                InitNpcWithDb(entity, reader);
                NpcMissionData nmd = NpcMissionDataRepository.GetMissionData(entity.Id);
                entity.SetUserData(nmd);
                SetNpcShopIcon(entity);

                entity.position = position;
                break;
            }
        }
    }

    public static void CreateStoryLineNpc()
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPC");
        NpcMissionDataRepository.Reset();
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NPC_ID")));
            int protoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PrototypeNPC")));
            if (PeGameMgr.IsMultiStory)
            {
                return;
                //				string strTemp = reader.GetString(reader.GetOrdinal("startpoint"));
                //				string[] pos = strTemp.Split(',');
                //				if (pos.Length < 3)
                //				{
                //					Debug.LogError("Npc's StartPoint is Error");
                //				}
                //				else
                //				{
                //					float x = System.Convert.ToSingle(pos[0]);
                //					float y = System.Convert.ToSingle(pos[1]);
                //					float z = System.Convert.ToSingle(pos[2]);
                //					Vector3 startPos = new Vector3(x, y, z);
                //					PlayerNetwork.MainPlayer.RequestCreateStNpc(id, startPos,protoId);
                //				}
                //				continue;
            }
            PeEntity entity = PeCreature.Instance.CreateNpc(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
            if (entity == null)
                continue;

            InitNpcWithDb(entity, reader);
            NpcMissionData nmd = NpcMissionDataRepository.GetMissionData(entity.Id);
            entity.SetUserData(nmd);
            SetNpcShopIcon(entity);
        }
    }
    public static void CreateTutorialLineNpc()
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPC");
        while (reader.Read())
        {
            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NPC_ID")));
            int protoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PrototypeNPC")));
            string strTemp = reader.GetString(reader.GetOrdinal("training_pos"));

            if (strTemp != "0,0,0")
            {
				string[] strPos = strTemp.Split(',');
				Vector3 pos = Vector3.zero;
				if (strPos.Length < 3)
				{
					Debug.LogError("Npc's StartPoint is Error at NPC_ID="+id);
				}
				else
				{
					pos.x = System.Convert.ToSingle(strPos[0]);
					pos.y = System.Convert.ToSingle(strPos[1]);
					pos.z = System.Convert.ToSingle(strPos[2]);
				}
				PeEntity entity = PeCreature.Instance.CreateNpc(id, protoId, pos, Quaternion.identity, Vector3.one);
                if (entity == null)
                    continue;
                
                SetNpcMoney(entity, reader.GetString(reader.GetOrdinal("money")));
                AddWeaponItem(entity, reader.GetString(reader.GetOrdinal("weapon")), reader.GetString(reader.GetOrdinal("item")));
				SetNpcAbility(entity,reader.GetString(reader.GetOrdinal("speciality")));
                NpcMissionData nmd = NpcMissionDataRepository.GetMissionData(entity.Id);
                entity.SetUserData(nmd);
                SetNpcShopIcon(entity);
            }
        }
    }

	#region data_init	
	static bool InitNpcWithDb(PeEntity entity, Mono.Data.SqliteClient.SqliteDataReader reader)
	{
		string strTemp = reader.GetString(reader.GetOrdinal("startpoint"));
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
			
			NpcCmpt cmpt = entity.NpcCmpt;
			if(null != cmpt)
			{
				cmpt.FixedPointPos = new Vector3(x, y, z);
			}
		}
		SetNpcMoney(entity, reader.GetString(reader.GetOrdinal("money")));
		AddWeaponItem(entity, reader.GetString(reader.GetOrdinal("weapon")), reader.GetString(reader.GetOrdinal("item")));
		SetNpcAbility(entity,reader.GetString(reader.GetOrdinal("speciality")));
		return true;
	}
    static void AddWeaponItem(PeEntity npc, string weapon, string item)
    {
        if (!weapon.Equals("0"))
            SetNpcWeapon(npc, Convert.ToInt32(weapon));
        if (!item.Equals("0"))
        {
            string[] strItemData = item.Split(',', ';');
            if (strItemData.Length > 1)
            {
                for (int i = 0; i < (strItemData.Length / 2); i++)
                {
                    SetNpcPackageItem(npc, Convert.ToInt32(strItemData[i * 2]), Convert.ToInt32(strItemData[i * 2 + 1]));
                }
            }
        }
    }
    static void SetNpcWeapon(PeEntity npc, int weaponID)
    {
        if (weaponID == 0)
            return;

        npc.GetComponent<EquipmentCmpt>().AddInitEquipment(ItemAsset.ItemMgr.Instance.CreateItem(weaponID));
    }

    static void SetNpcPackageItem(PeEntity npc, int itemID, int num)
    {
        NpcPackageCmpt pkg = npc.GetCmpt<NpcPackageCmpt>();
        pkg.Add(itemID, num);
    }
    static void SetNpcShopIcon(PeEntity npc)
    {
        string icon = StoreRepository.GetStoreNpcIcon(npc.Id);
        if (icon == "0")
            return;
        npc.SetShopIcon(icon);
    }
    //static void SetModelData(PeEntity entity, string name)
    //{
    //    AvatarCmpt avatar = entity.GetCmpt<AvatarCmpt>();

    //    CustomCharactor.AvatarData nudeAvatarData = new CustomCharactor.AvatarData();

    //    nudeAvatarData.SetPart(CustomCharactor.AvatarData.ESlot.HairF, "Model/Npc/" + name);

    //    avatar.SetData(new AppearBlendShape.AppearData(), nudeAvatarData);
    //}
    static void SetNpcMoney(PeEntity entity, string text)
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

        int initMin, initMax;
        if (!int.TryParse(strArray[0], out initMin))
        {
            return;
        }
        if (!int.TryParse(strArray[1], out initMax))
        {
            return;
        }

        strArray = groupStrArray[1].Split(',');
        if (strArray.Length != 2)
        {
            return;
        }

        int incMin, incMax;
        if (!int.TryParse(strArray[0], out incMin))
        {
            return;
        }
        if (!int.TryParse(strArray[1], out incMax))
        {
            return;
        }

        int max = 0;
        if (!int.TryParse(groupStrArray[2], out max))
        {
            return;
        }
        pkg.InitAutoIncreaseMoney(max, UnityEngine.Random.Range(incMin, incMax));
        pkg.money.current = UnityEngine.Random.Range(initMin, initMax);
    }
	static void SetNpcAbility(PeEntity entity, string text)
	{
		if (string.IsNullOrEmpty(text) || text == "0")
			return ;
		
		Ablities abilitys =new Ablities();
		string[] tmp1 = text.Split(',');
		for(int i=0;i<tmp1.Length;i++)
		{
			abilitys.Add(Convert.ToInt32(tmp1[i]));
		}
		
		NpcCmpt npccmpt = entity.GetCmpt<NpcCmpt>();
		if(npccmpt != null)
		{
			npccmpt.SetAbilityIDs(abilitys);
		}
	}
	#endregion
}
