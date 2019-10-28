using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TownData;
using Pathea;

public class VATownNpcManager : MonoBehaviour
{
    static VATownNpcManager mInstance;
    public static VATownNpcManager Instance
    {
        get
        {
            return mInstance;
        }
    }

    public Dictionary<IntVector2, VATownNpcInfo> npcInfoMap;
    public List<int> createdNpcIdList;
    public List<IntVector2> createdPosList;

    void Awake()
    {
        mInstance = this;
        npcInfoMap = new Dictionary<IntVector2, VATownNpcInfo>();
        createdNpcIdList = new List<int>();
        createdPosList = new List<IntVector2>();
    }
    //public TownNpcManager(){
    //    npcInfoMap = new Dictionary<IntVector2, TownNpcInfo>();
    //    createdNpcIdList = new List<int>();
    //    createdPosList = new List<IntVector2>();
    //}


    public void Clear()
    {
        npcInfoMap = new Dictionary<IntVector2, VATownNpcInfo>();
        createdNpcIdList = new List<int>();
        createdPosList = new List<IntVector2>();
    }

    //public bool AddNpc(IntVector2 posXZ, int id)
    //{
    //    if (npcInfoMap.ContainsKey(posXZ))
    //    {
    //        return false;
    //    }
    //    VATownNpcInfo townNpcInfo = new VATownNpcInfo(posXZ, id);
    //    npcInfoMap.Add(posXZ, townNpcInfo);
    //    return true;
    //}

    public bool AddNpc(VATownNpcInfo npcInfo)
    {
        IntVector2 posXZ = new IntVector2(Mathf.RoundToInt(npcInfo.getPos().x), Mathf.RoundToInt(npcInfo.getPos().z));
        if (npcInfoMap.ContainsKey(posXZ))
        {
            return false;
        }
        npcInfoMap.Add(posXZ, npcInfo);
        return true;
    }

    public bool IsCreated(IntVector2 pos)
    {
        return createdPosList.Contains(pos);
    }
    public bool IsCreated(Vector3 pos)
    {
        IntVector2 posXZ = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        return createdPosList.Contains(posXZ);
    }

    //public void CreateNpcReady(IntVector4 nodePosLod)
    //{
    //    //return;
    //    IntVector3 chunkCenter = new Vector3(nodePosLod.x, nodePosLod.y, nodePosLod.z);
    //    //LogManager.Error("createNpcReady", chunkCenter);
    //    for (int i = chunkCenter.x; i < chunkCenter.x + VoxelTerrainConstants._numVoxelsPerAxis; i++)
    //    {
    //        for (int j = chunkCenter.z; j < chunkCenter.z + VoxelTerrainConstants._numVoxelsPerAxis; j++)
    //        {
    //            IntVector2 posXZ = new IntVector2(i, j);
    //            if (!npcInfoMap.ContainsKey(posXZ))
    //            {
    //                continue;
    //            }
    //            VATownNpcInfo townNpcInfo = npcInfoMap[posXZ];
    //            if (!nodePosLod.ContainInTerrainNode(townNpcInfo.getPos()))
    //            {
    //                LogManager.Error("!nodePosLod.ContainInTerrainNode");
    //                continue;
    //            }
    //            if (townNpcInfo.PosY == -1)
    //            {
    //                continue;
    //            }
    //            int Id = townNpcInfo.getId();
    //            if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(Id))
    //            {
    //                continue;
    //            }
    //            //LogManager.Error("TownNPC: ID="+townNpcInfo.getId()+" pos="+townNpcInfo.getPos());
    //            if (Pathea.PeGameMgr.IsSingleAdventure)
    //            {
    //                AdNpcData adNpcData = NpcMissionDataRepository.m_AdRandMisNpcData[Id];
    //                int RNpcId = adNpcData.mRnpc_ID;
    //                int Qid = adNpcData.mQC_ID;

    //                //int npcid = NpcManager.Instance.RequestRandomNpc(RNpcId, townNpcInfo.getPos(), StroyManager.Instance.OnRandomNpcCreated1, adNpcData);
    //                //createdNpcIdList.Add(npcid);

    //                //NpcRandom nr = NpcManager.Instance.CreateRandomNpc(RNpcId, townNpcInfo.getPos());
    //                //StroyManager.Instance.NpcTakeMission(RNpcId, Qid, townNpcInfo.getPos(), nr, adNpcData.m_CSRecruitMissionList);
    //                //LogManager.Error("npc Created!Pos: " + townNpcInfo.getPos() + " id: " + RNpcId);
    //                //createdNpcIdList.Add(nr.mNpcId);
    //                createdPosList.Add(posXZ);
    //                npcInfoMap.Remove(posXZ);

    //            }
    //            else if (GameConfig.IsMultiMode)
    //            {
    //                SPTerrainEvent.instance.CreateAdNpcByIndex(townNpcInfo.getPos(), Id);
    //                npcInfoMap.Remove(posXZ);
    //            }
    //        }
    //    }
    //}

    //public void RenderTownNPC(IntVector2 posXZ)
    //{
    //    if (!npcInfoMap.ContainsKey(posXZ))
    //    {
    //        return;
    //    }
    //    VATownNpcInfo townNpcInfo = npcInfoMap[posXZ];
    //    if (townNpcInfo.PosY == -1)
    //    {
    //        return;
    //    }

    //    RenderTownNPC(townNpcInfo);
    //}

    public void RenderTownNPC(VATownNpcInfo townNpcInfo)
    {
        IntVector2 posXZ = townNpcInfo.Index;
        if (!npcInfoMap.ContainsKey(posXZ))
        {
            return;
        }
        int Id = townNpcInfo.getId();
        //if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(Id))
        //{
        //    return;
        //}
        if (Pathea.PeGameMgr.IsSingleAdventure)
        {
            //AdNpcData adNpcData = NpcMissionDataRepository.m_AdRandMisNpcData[Id];
            //int RNpcId = adNpcData.mRnpc_ID;
            //int Qid = adNpcData.mQC_ID;

			int	enemyNpcId = GetEnemyNpcId(Id);
			int allyId = VArtifactTownManager.Instance.GetTownByID(townNpcInfo.townId).AllyId;
			int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
			int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
			if(allyId!=TownGenData.PlayerAlly)
			{
				SceneEntityPosAgent agent = MonsterEntityCreator.CreateAdAgent(townNpcInfo.getPos(), enemyNpcId,allyColor,playerId);
				SceneMan.AddSceneObj(agent);
				VArtifactTownManager.Instance.AddMonsterPointAgent(townNpcInfo.townId,agent);
			}
			else{
				PeEntity npc = NpcEntityCreator.CreateNpc(Id,townNpcInfo.getPos(),Vector3.one,Quaternion.Euler(0,180,0));
				if (npc == null)
				{
					Debug.LogError("npc id error: templateId = " + Id);
					return;
				}
				//Debug.Log("created town npc:"+ npc.name+"_"+npc.position);
				VArtifactUtil.SetNpcStandRot(npc,180,false);//test
				if(Id==VArtifactTownManager.Instance.missionStartNpcID){
					VArtifactTownManager.Instance.missionStartNpcEntityId = npc.Id;
				}
				createdPosList.Add(posXZ);
			}

            //Debug.Log("Id: " + Id + " npcPos:" + townNpcInfo.getPos());

            //int npcid = NpcManager.Instance.RequestRandomNpc(RNpcId, townNpcInfo.getPos(), StroyManager.Instance.OnRandomNpcCreated1, adNpcData);
            //createdNpcIdList.Add(Id);

            //NpcRandom nr = NpcManager.Instance.CreateRandomNpc(RNpcId, townNpcInfo.getPos());
            //StroyManager.Instance.NpcTakeMission(RNpcId, Qid, townNpcInfo.getPos(), nr, adNpcData.m_CSRecruitMissionList);
            //LogManager.Error("npc Created!Pos: " + townNpcInfo.getPos() + " id: " + RNpcId);
            //createdNpcIdList.Add(nr.mNpcId);
            npcInfoMap.Remove(posXZ);

        }
        else if (GameConfig.IsMultiMode)
		{	int	enemyNpcId = GetEnemyNpcId(Id);
			int allyId = VArtifactTownManager.Instance.GetTownByID(townNpcInfo.townId).AllyId;
			int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
			int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
			if(allyId!=TownGenData.PlayerAlly)
			{
				SceneEntityPosAgent agent = MonsterEntityCreator.CreateAdAgent(townNpcInfo.getPos(), enemyNpcId,allyColor,playerId);
				SceneMan.AddSceneObj(agent);
				VArtifactTownManager.Instance.AddMonsterPointAgent(townNpcInfo.townId,agent);
			}else{
	            StartCoroutine(RenderOneNpc(townNpcInfo.getPos(), Id));
			}
            npcInfoMap.Remove(posXZ);
        }
    }

    IEnumerator RenderOneNpc(Vector3 pos, int id)
    {
        while (PeCreature.Instance.mainPlayer==null)
            yield return null;
        PlayerNetwork.mainPlayer.RequestTownNpc(pos, id);
    }

    public int GetRNpcIdByPos(IntVector2 posXZ)
    {
        if (!npcInfoMap.ContainsKey(posXZ))
        {
            return -1;
        }

        return npcInfoMap[posXZ].getId();
    }

    //public VATownNpcInfo GetNpcInfoByPos(IntVector2 posXZ)
    //{
    //    if (!npcInfoMap.ContainsKey(posXZ))
    //    {
    //        return null;
    //    }

    //    return npcInfoMap[posXZ];
    //}

	public void Export(BinaryWriter bw)
    {
        bw.Write(createdNpcIdList.Count);
        for (int i = 0; i < createdNpcIdList.Count; i++) {
			int npcid = createdNpcIdList [i];
			NpcMissionData data = NpcMissionDataRepository.GetMissionData (npcid);
			if (data == null) {
				bw.Write (-1);
			}
			else {
				bw.Write (npcid);
				bw.Write (data.m_Rnpc_ID);
				bw.Write (data.m_QCID);
				bw.Write (data.m_CurMissionGroup);
				bw.Write (data.m_CurGroupTimes);
				bw.Write (data.mCurComMisNum);
				bw.Write (data.mCompletedMissionCount);
				bw.Write (data.m_RandomMission);
				bw.Write (data.m_RecruitMissionNum);
				bw.Write (data.m_MissionList.Count);
				for (int m = 0; m < data.m_MissionList.Count; m++)
					bw.Write (data.m_MissionList [m]);
				bw.Write (data.m_MissionListReply.Count);
				for (int m = 0; m < data.m_MissionListReply.Count; m++)
					bw.Write (data.m_MissionListReply [m]);
			}
		}

		bw.Write(createdPosList.Count);
        for (int i = 0; i < createdPosList.Count; i++) {
			IntVector2 posXZ = createdPosList [i];
			bw.Write (posXZ.x);
			bw.Write (posXZ.y);
		}
    }

    public void Import(byte[] buffer)
    {
        LogManager.Info("TownNpcManager.Instance.Import()");
        Clear();
        if (buffer.Length == 0)
            return;

        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);

        int iSize = _in.ReadInt32();
        for (int i = 0; i < iSize; i++)
        {
            NpcMissionData data = new NpcMissionData();
            int npcid = _in.ReadInt32();

            if (npcid == -1)
                continue;

            data.m_Rnpc_ID = _in.ReadInt32();
            data.m_QCID = _in.ReadInt32();
            data.m_CurMissionGroup = _in.ReadInt32();
            data.m_CurGroupTimes = _in.ReadInt32();
            data.mCurComMisNum = _in.ReadByte();
            data.mCompletedMissionCount = _in.ReadInt32();
            data.m_RandomMission = _in.ReadInt32();
            data.m_RecruitMissionNum = _in.ReadInt32();

            int num = _in.ReadInt32();
            for (int j = 0; j < num; j++)
                data.m_MissionList.Add(_in.ReadInt32());

            num = _in.ReadInt32();
            for (int j = 0; j < num; j++)
                data.m_MissionListReply.Add(_in.ReadInt32());

            createdNpcIdList.Add(npcid);
            NpcMissionDataRepository.AddMissionData(npcid, data);
        }

        iSize = _in.ReadInt32();
        for (int i = 0; i < iSize; i++)
        {
            int x = _in.ReadInt32();
            int z = _in.ReadInt32();
            createdPosList.Add(new IntVector2(x, z));
        }


        _in.Close();
        ms.Close();
    }

    public void InitAdTownNpc()
    {
        StartCoroutine(InitAdNpc());
    }

    IEnumerator InitAdNpc()
    {
        //while (PlayerFactory.mMainPlayer == null)
        //{
        //    yield return new WaitForSeconds(0.1f);
        //}

		//foreach (int npcid in createdNpcIdList)
		//{
            //NpcMissionData useData = NpcMissionDataRepository.GetMissionData(npcid);
            //if (useData == null)
            //    continue;
            //NpcManager.Instance.RequestRandomNpc(npcid, useData.m_Rnpc_ID, Vector3.zero, StroyManager.Instance.OnRandomNpcCreated, useData);

            //NpcRandom npcRandom = NpcManager.Instance.CreateRandomNpc(npcid, useData.m_Rnpc_ID, Vector3.zero);

            //if (npcRandom == null)
            //    continue;

            //npcRandom.UserData = useData;
            //StroyManager.Instance.SetNpcShopIcon(npcRandom);
            //npcRandom.MouseCtrl.MouseEvent.SubscribeEvent(StroyManager.Instance.NpcMouseEventHandler);
		//}

        yield return 0;
    }

	public int GetEnemyNpcId(int id){
		return NpcMissionDataRepository.GetRNpcId(id)+AllyConstants.EnemyNpcIdAddNum;
	}

	public void GenEnemyNpc(List<BuildingNpc> bNpcs,int townId,int allyId){
		foreach(BuildingNpc bnpc in bNpcs){
			int	enemyNpcId = GetEnemyNpcId(bnpc.templateId);
			int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
			int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
			SceneEntityPosAgent agent = MonsterEntityCreator.CreateAdAgent(bnpc.pos, enemyNpcId,allyColor,playerId);
			SceneMan.AddSceneObj(agent);
			VArtifactTownManager.Instance.AddMonsterPointAgent(townId,agent);
		}
	}
}
