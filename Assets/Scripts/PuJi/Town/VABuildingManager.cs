using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;
using Pathea;
public class VABuildingManager:MonoBehaviour
{
    static VABuildingManager mInstance;
    public static VABuildingManager Instance
    {
        get
        {
            //if (mInstance == null)
            //{
            //    mInstance = new BuildingInfoManager();
            //}
            return mInstance;
        }
    }

    public Dictionary<int, BuildingID> missionBuilding;
    public Dictionary<BuildingID, VABuildingInfo> mBuildingInfoMap;
    //public Dictionary<BuildingID, bool> renderManager;


    public Dictionary<BuildingID, int> mCreatedNpcBuildingID;
    public List<BuildingNpcIdStand> createdNpcIdList;

    //test
    //static int itemSum = 0;

    void Awake()
    {
        mInstance = this;

        missionBuilding = new Dictionary<int, BuildingID>();
        mBuildingInfoMap = new Dictionary<BuildingID, VABuildingInfo>();
        //renderManager = new Dictionary<BuildingID, bool>();
        mCreatedNpcBuildingID = new Dictionary<BuildingID, int>();
        createdNpcIdList = new List<BuildingNpcIdStand>();
    }
    public void Clear()
    {
        missionBuilding.Clear();
        mBuildingInfoMap.Clear();
        //renderManager.Clear();
        mCreatedNpcBuildingID.Clear();
        createdNpcIdList.Clear();
        //itemSum = 0;
    }

    public VABuildingInfo GetBuildingInfoByBuildingID(BuildingID buildingid)
    {
        if (!mBuildingInfoMap.ContainsKey(buildingid))
        {
            return null;
        }
        return mBuildingInfoMap[buildingid];
    }


    public void AddBuilding(VABuildingInfo bdinfo)
    {
		if(mBuildingInfoMap.ContainsKey(bdinfo.buildingId))
			return;
        if (!GameConfig.IsMultiMode)
        {
            mBuildingInfoMap.Add(bdinfo.buildingId, bdinfo);
        }
        else
        {
            mBuildingInfoMap.Add(bdinfo.buildingId, bdinfo);
        }
        //renderManager.Add(bdinfo.buildingId, false);
    }

    public void RenderBuilding(BuildingID buildingId)
    {
        VABuildingInfo b = GetBuildingInfoByBuildingID(buildingId);

        if (b == null) { return; }
        RenderBuilding(b);
    }

    public void RenderBuilding(VABuildingInfo b)
    {
        BuildingID buildingId = b.buildingId;

        if (b.pos.y == -1)
        {
            LogManager.Error("building [", buildingId, "] height not initialized!");
            return;
        }

        RenderPrefebBuilding(b);

        //renderManager[buildingId] = true;
    }

    IEnumerator CreateBuildingNpcList(List<BuildingNpc> buildingNpcList)
    {
        if (PeGameMgr.IsMulti)
            while (PeCreature.Instance.mainPlayer == null)
                yield return null;
        for (int i = 0; i < buildingNpcList.Count; i++)
        {
            CreateOneBuildingNpc(buildingNpcList[i]);
            yield return null;
        }

    }

    void OnSpawned(GameObject go)
    {
        if (go == null)
            return;

        //if (AiManager.Manager != null)
        //{
        //    go.transform.parent = AiManager.Manager.transform;
        //}
    }

    private void CreateOneBuildingNpc(BuildingNpc buildingNpc)
    {
        if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(buildingNpc.templateId))
        {
            LogManager.Error("not exist! id = [", buildingNpc.templateId, "] Pos = ", buildingNpc.pos);
            return;
        }

        buildingNpc.pos.y += 0.2f;
        if (PeGameMgr.IsSingleAdventure)
        {
            //AdNpcData adNpcData = NpcMissionDataRepository.m_AdRandMisNpcData[buildingNpc.templateId];
            //int RNpcId = adNpcData.mRnpc_ID;
            //int Qid = adNpcData.mQC_ID;

			PeEntity npc = NpcEntityCreator.CreateNpc(buildingNpc.templateId, buildingNpc.pos,Vector3.one,Quaternion.Euler(0,buildingNpc.rotY,0));
            if (npc == null)
            {
                Debug.LogError("npc id error: templateId = " + buildingNpc.templateId);
                return;
            }
            if(buildingNpc.isStand)
            {
                VArtifactUtil.SetNpcStandRot(npc, buildingNpc.rotY,true);
                createdNpcIdList.Add(new BuildingNpcIdStand(npc.Id, true, buildingNpc.rotY));
            }
            else
            {
                createdNpcIdList.Add(new BuildingNpcIdStand(npc.Id, false, buildingNpc.rotY));
            }
            //--to do: npc
            //NpcRandom nr = NpcManager.Instance.CreateRandomNpc(RNpcId, buildingNpc.pos);

            //StroyManager.Instance.NpcTakeMission(RNpcId, Qid, buildingNpc.pos, nr, adNpcData.m_CSRecruitMissionList);

            //if(buildingNpc.isStand)
            //{
            //    nr.SetRotY(buildingNpc.rotY);
            //    nr.CloseAi();
            //    createdNpcIdList.Add(new BuildingNpcIdStand(nr.mNpcId, true, buildingNpc.rotY));
            //}
            //else
            //{
            //    createdNpcIdList.Add(new BuildingNpcIdStand(nr.mNpcId, false, buildingNpc.rotY));
            //}
        }
        else if (GameConfig.IsMultiMode)
        {
            //SPTerrainEvent.instance.CreateAdNpcByIndex(buildingNpc.pos, buildingNpc.templateId, 1, buildingNpc.isStand, buildingNpc.rotY);
            PlayerNetwork.mainPlayer.RequestTownNpc(buildingNpc.pos, buildingNpc.templateId, 1, buildingNpc.isStand, buildingNpc.rotY);
        }
    }

    IEnumerator CreateMapItemInBuilding(BuildingID buildingId, List<CreatItemInfo> itemInfoList, Vector3 root, int id, int rotation)
    {
        if (PeGameMgr.IsSingleAdventure)
        {
            for (int i = 0; i < itemInfoList.Count; i++)
            {
                //LogManager.Error(itemInfoList[i].mItemId, itemInfoList[i].mPos, itemInfoList[i].mRotation);
                DragArticleAgent.PutItemByProroId(itemInfoList[i].mItemId, itemInfoList[i].mPos, itemInfoList[i].mRotation);
                yield return null;
            }

            mCreatedNpcBuildingID.Add(buildingId, 0);
        }
        else if (GameConfig.IsMultiMode)
        {
            while (true)
            {
                if (PeCreature.Instance.mainPlayer == null)
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }
            //--to do: createBuildingWithItem
            //PlayerNetwork.MainPlayer.CreateBuildingWithItem(buildingId, itemInfoList, root, id, rotation);
            yield return null;
        }

        yield return null;
    }

    public void RenderPrefebBuilding(VABuildingInfo buildinginfo)
    {
        int bid = buildinginfo.id;

        Quaternion rotation = Quaternion.Euler(0, buildinginfo.rotation, 0);

        if (PeGameMgr.IsSingleAdventure)
        {
                if (buildinginfo.buildingId.buildingNo != VArtifactTownConstant.NATIVE_TOWER_BUILDING_ID)
                {
                    if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(bid))
                    {
                        LogManager.Error("bid = [", bid, "] not exist in database!");
                        return;
                    }

					int campId = SceneDoodadDesc.c_neutralCamp;
					int damageId = SceneDoodadDesc.c_neutralDamage;
					if(buildinginfo.vau.vat.type==VArtifactType.NpcTown){
						if(!buildinginfo.vau.vat.IsPlayerTown){
							if(bid==ColonyNoMgrMachine.DOODAD_ID_REPAIR||bid==ColonyNoMgrMachine.DOODAD_ID_SOLARPOWER)
								return;
							campId = AllyConstants.EnemyNpcCampId;
							damageId = AllyConstants.EnemyNpcDamageId;
						}
					}else{
						if(buildinginfo.vau.vat.nativeType==NativeType.Puja){
							campId = AllyConstants.PujaCampId;
							damageId = AllyConstants.PujaDamageId;
						}else{
							campId = AllyConstants.PajaCampId;
							damageId = AllyConstants.PajaDamageId;
						}
					}
					int playerId = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
					if(!buildinginfo.vau.isDoodadNpcRendered)
						VArtifactTownManager.Instance.AddAliveBuilding(buildinginfo.vau.vat.townId,
					                                               DoodadEntityCreator.CreateRandTerDoodad(BlockBuilding.s_tblBlockBuildingMap[bid].mDoodadProtoId, buildinginfo.root, Vector3.one, rotation,buildinginfo.buildingId.townId,campId,damageId,playerId).Id);
					
                    //building npc
                    if (!mCreatedNpcBuildingID.ContainsKey(buildinginfo.buildingId))
                    {
                        BlockBuilding building = BlockBuilding.s_tblBlockBuildingMap[bid];
                        List<BuildingNpc> buildingNpcs;
                        building.GetNpcInfo(out buildingNpcs);
                        for (int bni = 0; bni < buildingNpcs.Count; bni++)
                        {
                            BuildingNpc bn = buildingNpcs[bni];
                            VArtifactUtil.GetPosRotFromPointRot(ref bn.pos, ref bn.rotY, buildinginfo.root, buildinginfo.rotation);
                        }
                        if (buildingNpcs != null && buildingNpcs.Count > 0)
						{
							if(buildinginfo.vau.vat.IsPlayerTown){
								if(!buildinginfo.vau.isDoodadNpcRendered){
		                            StartCoroutine(CreateBuildingNpcList(buildingNpcs));
	                            	mCreatedNpcBuildingID.Add(buildinginfo.buildingId, 0);
								}
							}else{
								GenEnemyNpc(buildingNpcs,buildinginfo.vau.vat.townId,buildinginfo.vau.vat.AllyId);
							}
                        }
                    }
                    
                }
                else if (buildinginfo.buildingId.buildingNo == VArtifactTownConstant.NATIVE_TOWER_BUILDING_ID)
                {
					//tower
					if(!buildinginfo.vau.isDoodadNpcRendered){
						int playerId = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
						VArtifactTownManager.Instance.AddAliveBuilding(buildinginfo.vau.vat.townId,
						                                               DoodadEntityCreator.CreateRandTerDoodad(buildinginfo.pathID, buildinginfo.root, Vector3.one, rotation,buildinginfo.vau.vat.townId,buildinginfo.campID,buildinginfo.damageID,playerId).Id
					                                               );
					}
                }
                if (missionBuilding.ContainsKey(0))
                {
                    if (buildinginfo.buildingId != missionBuilding[0])
                        RemoveBuilding(buildinginfo.buildingId);
                }
                else
                {
                    RemoveBuilding(buildinginfo.buildingId);
                }
        }
        else if (PeGameMgr.IsMulti)
        {
            if (buildinginfo.buildingId.buildingNo != VArtifactTownConstant.NATIVE_TOWER_BUILDING_ID)
            {
                if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(bid))
                {
                    LogManager.Error("bid = [", bid, "] not exist in database!");
                    return;
                }
                Debug.Log("RenderPrefebBuilding():" + bid);
				//              int campId = SceneDoodadDesc.c_neutralCamp;
				int campId = SceneDoodadDesc.c_neutralCamp;
				int damageId = SceneDoodadDesc.c_neutralDamage;
				if(buildinginfo.vau.vat.type==VArtifactType.NpcTown){
					if(!buildinginfo.vau.vat.IsPlayerTown){
						if(bid==ColonyNoMgrMachine.DOODAD_ID_REPAIR||bid==ColonyNoMgrMachine.DOODAD_ID_SOLARPOWER)
							return;
						campId = AllyConstants.EnemyNpcCampId;
						damageId = AllyConstants.EnemyNpcDamageId;
					}
				}else{
					if(buildinginfo.vau.vat.nativeType==NativeType.Puja){
						campId = AllyConstants.PujaCampId;
						damageId = AllyConstants.PujaDamageId;
					}else{
						campId = AllyConstants.PajaCampId;
						damageId = AllyConstants.PajaDamageId;
					}
				}
				int playerId = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
				if(!buildinginfo.vau.isDoodadNpcRendered)
					PlayerNetwork.RequestServer(EPacketType.PT_Common_TownDoodad,buildinginfo.buildingId, BlockBuilding.s_tblBlockBuildingMap[bid].mDoodadProtoId, buildinginfo.root, Vector3.one, rotation, buildinginfo.vau.vat.townId, campId,damageId,playerId);
                //building npc
                if (!mCreatedNpcBuildingID.ContainsKey(buildinginfo.buildingId))
                {
                    BlockBuilding building = BlockBuilding.s_tblBlockBuildingMap[bid];
                    List<BuildingNpc> buildingNpcs;
                    building.GetNpcInfo(out buildingNpcs);
                    for (int bni = 0; bni < buildingNpcs.Count; bni++)
                    {
                        BuildingNpc bn = buildingNpcs[bni];
                        VArtifactUtil.GetPosRotFromPointRot(ref bn.pos, ref bn.rotY, buildinginfo.root, buildinginfo.rotation);
                    }
                    if (buildingNpcs != null && buildingNpcs.Count > 0)
                    {
						if (buildingNpcs != null && buildingNpcs.Count > 0)
						{
							if(buildinginfo.vau.vat.IsPlayerTown){
								if(!buildinginfo.vau.isDoodadNpcRendered){
									StartCoroutine(CreateBuildingNpcList(buildingNpcs));
									mCreatedNpcBuildingID.Add(buildinginfo.buildingId, 0);
								}
							}else{
 								GenEnemyNpc(buildingNpcs,buildinginfo.vau.vat.townId,buildinginfo.vau.vat.AllyId);
							}
						}
					}
				}
			}
			else if (buildinginfo.buildingId.buildingNo == VArtifactTownConstant.NATIVE_TOWER_BUILDING_ID)
			{
				if(!buildinginfo.vau.isDoodadNpcRendered){
					int playerId = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
					PlayerNetwork.RequestServer(EPacketType.PT_Common_NativeTowerCreate,buildinginfo.buildingId,  buildinginfo.pathID, buildinginfo.root, Vector3.one, rotation, buildinginfo.vau.vat.townId, buildinginfo.campID, buildinginfo.damageID,playerId);
				}
			}
			RemoveBuilding(buildinginfo.buildingId);
        }
    }
	public void GenEnemyNpc(List<BuildingNpc> bNpcs,int townId,int allyId){
		VATownNpcManager.Instance.GenEnemyNpc(bNpcs,townId,allyId);
	}
    //to judge if a building is rendered
    //public bool IsRendered(BuildingID buildingNo)
    //{
    //    if (renderManager.ContainsKey(buildingNo))
    //        return renderManager[buildingNo];
    //    else
    //        return true;
    //}


	public void Export(BinaryWriter bw)
    {
        bw.Write(createdNpcIdList.Count);
        for (int i = 0; i < createdNpcIdList.Count; i++) {
			BuildingNpcIdStand ite = createdNpcIdList [i];
			int npcid = ite.npcId;
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
				bw.Write (ite.isStand);
				bw.Write (ite.rotY);
			}
		}

        bw.Write(mCreatedNpcBuildingID.Count);
        foreach (BuildingID b in mCreatedNpcBuildingID.Keys)
        {
            bw.Write(b.townId);
            bw.Write(b.buildingNo);
        }
    }

    public void Import(byte[] buffer)
    {
        //LogManager.Error("BuildingInfoManager.Instance.Import()");
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

            bool isStand = _in.ReadBoolean();
            float rotY = _in.ReadSingle();
            createdNpcIdList.Add(new BuildingNpcIdStand(npcid, isStand, rotY));
            NpcMissionDataRepository.AddMissionData(npcid, data);
        }

        iSize = _in.ReadInt32();
        for (int i = 0; i < iSize; i++)
        {
            BuildingID no = new BuildingID();
            no.townId = _in.ReadInt32();
            no.buildingNo = _in.ReadInt32();
            mCreatedNpcBuildingID.Add(no, 0);
        }


        _in.Close();
        ms.Close();
        //loadRecord = LOADMARK;
    }

    public void InitAdBuildingNpc()
    {
        StartCoroutine(InitAdNpc());
    }

    IEnumerator InitAdNpc()
    {
        while (PeCreature.Instance.mainPlayer == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

//        foreach (BuildingNpcIdStand npcIdStand in createdNpcIdList)
//        {
            //NpcMissionData useData = NpcMissionDataRepository.GetMissionData(npcIdStand.npcId);
            //if (useData == null)
            //    continue;

            //NpcManager.Instance.RequestRandomNpc(npcid, useData.m_Rnpc_ID, Vector3.zero, StroyManager.Instance.OnRandomNpcCreated, useData);

            //--to do: npc
            //NpcRandom npcRandom = NpcManager.Instance.CreateRandomNpc(npcIdStand.npcId, useData.m_Rnpc_ID, Vector3.zero);
            //if (npcRandom == null)
            //    continue;

            //npcRandom.UserData = useData;
            //if (npcIdStand.isStand)
            //{
            //    npcRandom.CloseAi();
            //}
            //StroyManager.Instance.SetNpcShopIcon(npcRandom);
            //npcRandom.MouseCtrl.MouseEvent.SubscribeEvent(StroyManager.Instance.NpcMouseEventHandler);
//        }
    }

    public Vector3 GetMissionBuildingPos(int missionId = 0)
    {
        if (missionBuilding.ContainsKey(missionId))
        {
            VABuildingInfo bdinfo = GetBuildingInfoByBuildingID(missionBuilding[missionId]);


            if (bdinfo != null)
            {
                //Debug.LogError("missionPos: "+bdinfo.frontDoorPos);
                return bdinfo.frontDoorPos;
            }
        }
        return Vector3.zero;
    }


    public void RemoveBuilding(BuildingID buildingId)
    {
        if(mBuildingInfoMap.ContainsKey(buildingId))
            mBuildingInfoMap.Remove(buildingId);
    }
}



public class BuildingNpc
{
    public int templateId;
    public Vector3 pos;
    public float rotY;
    public bool isStand;
    
    public BuildingNpc(int id,Vector3 pos,float rotY,bool isStand)
    {
        this.templateId = id;
        this.pos = pos;
        this.rotY = rotY;
        this.isStand = isStand;
    }
}

public class BuildingNpcIdStand
{
    public int npcId;
    public bool isStand;
    public float rotY;
    public BuildingNpcIdStand(int npcId, bool isStand,float rotY)
    {
        this.npcId = npcId;
        this.isStand = isStand;
        this.rotY = rotY;
    }
}