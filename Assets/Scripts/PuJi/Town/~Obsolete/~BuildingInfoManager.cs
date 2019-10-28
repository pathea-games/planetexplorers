//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using UnityEngine;
//using TownData;
//using System.Collections;

//public class BuildingInfoManager : MonoBehaviour
//{
//    static BuildingInfoManager mInstance;
//    public static BuildingInfoManager Instance
//    {
//        get
//        {
//            //if (mInstance == null)
//            //{
//            //    mInstance = new BuildingInfoManager();
//            //}
//            return mInstance;
//        }
//    }

//    uint maxBuildingNo;
//    public Dictionary<int, BuildingID> missionBuilding;
//    public Dictionary<BuildingID, BuildingInfo> mBuildingInfoMap;
//    public Dictionary<int, List<BuildingID>> townBuildingList;
//    public Dictionary<IntVector2, BuildingID> mBuildingBlockPos;
//    public Dictionary<IntVector2, List<BuildingID>> mBuildingTile;//the tile that contains buildings
//    public Dictionary<BuildingID, bool> renderManager;
    
//    //public Dictionary<IntVector4, List<IntVector3>> mBuildingNodePosLod2Pos;

//    public Dictionary<BuildingID, int> mCreatedNpcItemBuildingIndex;
//    public List<int> createdNpcIdList;
//    //test
//    static int itemSum = 0;

//    //public int loadRecord;
//    //public const int LOADMARK = 2;

//    void Awake(){
//        mInstance = this;

//        maxBuildingNo = 0;
//        missionBuilding = new Dictionary<int, BuildingID>();
//        mBuildingInfoMap = new Dictionary<BuildingID, BuildingInfo>();
//        townBuildingList = new Dictionary<int, List<BuildingID>>();
//        mBuildingBlockPos = new Dictionary<IntVector2, BuildingID>();
//        mBuildingTile = new Dictionary<IntVector2, List<BuildingID>>();
//        renderManager = new Dictionary<BuildingID, bool>();
//        //mBuildingNodePosLod2Pos = new Dictionary<IntVector4, List<IntVector3>>();
//        mCreatedNpcItemBuildingIndex = new Dictionary<BuildingID, int>();
//        createdNpcIdList = new List<int>();
//    }
//    //BuildingInfoManager()
//    //{
//    //    maxBuildingNo = 0;
//    //    mBuildingInfoMap = new Dictionary<uint, BuildingInfo>();
//    //    mBuildingBlockPos = new Dictionary<IntVector2, uint>();
//    //    mBuildingTile = new Dictionary<IntVector2, uint>();
//    //    renderManager = new Dictionary<uint, bool>();
//    //    //mBuildingNodePosLod2Pos = new Dictionary<IntVector4, List<IntVector3>>();
//    //    mCreatedNpcItemBuildingIndex = new Dictionary<uint, int>();
//    //    createdNpcIdList = new List<int>();   
//    //}

//    public void Clear()
//    {
//        maxBuildingNo = 0;
//        missionBuilding = new Dictionary<int, BuildingID>();
//        mBuildingInfoMap = new Dictionary<BuildingID, BuildingInfo>();
//        mBuildingBlockPos = new Dictionary<IntVector2, BuildingID>();
//        townBuildingList = new Dictionary<int, List<BuildingID>>();
//        mBuildingTile = new Dictionary<IntVector2, List<BuildingID>>();
//        renderManager = new Dictionary<BuildingID, bool>();
//        //mBuildingNodePosLod2Pos = new Dictionary<IntVector4, List<IntVector3>>();
//        mCreatedNpcItemBuildingIndex = new Dictionary<BuildingID, int>();
//        createdNpcIdList = new List<int>();
//        itemSum = 0;
//    }

//    public BuildingInfo GetBuildingInfoByBuildingID(BuildingID buildingid)
//    {
//        //if (!mBuildingInfoMap.ContainsKey(No))
//        //{
//        //    return null;
//        //}
//        return mBuildingInfoMap[buildingid];
//    }

        
//    public void AddBuilding(BuildingInfo bdinfo)
//    {
//        if (!GameConfig.IsMultiMode)
//        {
//            //bdinfo.buildingNo = maxBuildingNo;
//            //mBuildingInfoMap.Add(maxBuildingNo, bdinfo);//add to buildingInfoMap
//            mBuildingInfoMap.Add(bdinfo.buildingId, bdinfo);
//            //townBuildingList.Add(bdinfo.buildingId.townId,b);
//        }
//        else
//        {
//            mBuildingInfoMap.Add(bdinfo.buildingId , bdinfo);
//        }

//        //for (int x = Mathf.FloorToInt(bdinfo.pos.x); x < Mathf.CeilToInt(bdinfo.pos.x + bdinfo.cellSizeX) + 1; x++)
//        //{
//        //    for (int z = Mathf.FloorToInt(bdinfo.pos.z); z < Mathf.CeilToInt(bdinfo.pos.z + bdinfo.cellSizeZ) + 1; z++)
//        //    {
//        //        if (!mBuildingBlockPos.ContainsKey(new IntVector2(x, z)))
//        //        {
//        //            mBuildingBlockPos.Add(new IntVector2(x, z), bdinfo.buildingId);//add to the Block's pos-BuildingNo information
//        //        }
//        //    }
//        //}

//        //int x2 = bdinfo.center.x >> VoxelTerrainConstants._shift;
//        //int z2 = bdinfo.center.y >> VoxelTerrainConstants._shift;

//        //IntVector2 tileIndexXZ = new IntVector2(x2, z2);
//        //if (mBuildingTile.ContainsKey(tileIndexXZ))
//        //{
//        //    mBuildingTile[tileIndexXZ].Add(bdinfo.buildingId);
//        //}
//        //else
//        //{
//        //    List<BuildingID> bdno = new List<BuildingID>();
//        //    bdno.Add(bdinfo.buildingId);
//        //    mBuildingTile.Add(tileIndexXZ, bdno);
//        //}


//        renderManager.Add(bdinfo.buildingId, false);
//        //LogManager.Debug("add BuildingNo: ",bdinfo.buildingNo," pos: ",bdinfo.root);
//        //if (!GameConfig.IsMultiMode)
//        //{
//        //    maxBuildingNo++;
//        //}
//    }


//    //get the buildinginfo from the worldPosXZ
//    //public BuildingInfo GetBuildingInfoByPos(IntVector2 worldPos)
//    //{
//    //    if(!mBuildingBlockPos.ContainsKey(worldPos)){
//    //        return null;
//    //    }
//    //    uint no = mBuildingBlockPos[worldPos];
//    //    if (!mBuildingInfoMap.ContainsKey(no))
//    //    {
//    //        LogManager.Error("mBuildingInfoMap[", no,"] not exist!");
//    //        return null;
//    //    }
//    //    return mBuildingInfoMap[no];
//    //}

//    public void RenderBuilding(BuildingID buildingId)
//    {

//        BuildingInfo b = GetBuildingInfoByBuildingID(buildingId);

//        if (b == null) { return; }
//        if(b.buildingType== BuildingType.Block)
//        { 
//            if (IsRendered(buildingId))
//            {
//                return;
//            }
//        }

//            if (b.pos.y == -1)
//            {
//                LogManager.Error("building [", buildingId, "] height not initialized!");
//                return;
//            }
//            if (b.buildingType == BuildingType.Block)
//            {
//                RenderBlockBuilding(b);
                
//            }
//            else {
//                RenderPrefebBuilding(b);
//            }
//            renderManager[buildingId] = true;
//    }

//    IEnumerator CreateOneBuilding(Dictionary<IntVector3, B45Block> retBuild)
//    {
//         //LogManager.Debug(b.size);
//        if (Pathea.PeGameMgr.IsSingleAdventure)
//        {
//            foreach (IntVector3 index in retBuild.Keys)
//            {
//                Block45Man.self.DataSource.SafeWrite(retBuild[index], index.x, index.y, index.z, 0);
//                //LogManager.Debug("index: " + index);
//                //LogManager.Debug("BlockType: " + retBuild[index].blockType);
//                //LogManager.Debug("MaterialType: " + retBuild[index].materialType);
//            }
//        }
//        else if (GameConfig.IsMultiMode)
//        {
//            //foreach (IntVector3 index in retBuild.Keys)
//            //{
//            //    Block45Man.self.DataSource.SafeWrite(retBuild[index], index.x, index.y, index.z, 0);
//            //    //LogManager.Debug("index: " + index);
//            //    //LogManager.Debug("BlockType: " + retBuild[index].blockType);
//            //    //LogManager.Debug("MaterialType: " + retBuild[index].materialType);
//            //}

//        }
//        yield return null;
//    }

//    IEnumerator CreateNpcWithPosId(List<Vector3> npcPositionList, List<int> npcIds)
//    {
//        for (int i = 0; i < npcPositionList.Count; i++)
//        {
//            if (npcIds[i] != -1)
//            {
//                //LogManager.Error("BuildingNpc: ",npcPositionList[i], npcIds[i]);

//                CreateOneNpcWithPosId(npcPositionList[i], npcIds[i]);
//                yield return null;
//            }
//        }
        
//    }

//    void OnSpawned(GameObject go)
//    {
//        if (go == null)
//            return;

//        //if (AiManager.Manager != null)
//        //{
//        //    go.transform.parent = AiManager.Manager.transform;
//        //}
//    }

//    private void CreateOneNpcWithPosId(Vector3 pos,int id)
//    {
//        //return;
//        if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(id))
//        {
//            LogManager.Error("not exist! id = [", id, "] Pos = ", pos);
//            return;
//        }

//        pos.y += 0.2f;
//        if (Pathea.PeGameMgr.IsSingleAdventure)
//        {
//            AdNpcData adNpcData = NpcMissionDataRepository.m_AdRandMisNpcData[id];
//            int RNpcId = adNpcData.mRnpc_ID;
//            int Qid = adNpcData.mQC_ID;

//            //int npcid = NpcManager.Instance.RequestRandomNpc(RNpcId, pos, StroyManager.Instance.OnRandomNpcCreated1, adNpcData);
//            //createdNpcIdList.Add(npcid);

//            //NpcRandom nr = NpcManager.Instance.CreateRandomNpc(RNpcId, pos);
            
//            //StroyManager.Instance.NpcTakeMission(RNpcId, Qid,pos, nr, adNpcData.m_CSRecruitMissionList);

//            //To do Ai Create
//            //AIResource.Instantiate(id, pos, Quaternion.identity, OnSpawned);

//            //SPPoint.InstantiateSPPoint<SPPoint>(IntVector4.Zero, pos, 
//            //                                    Quaternion.identity, 
//            //                                    SPTerrainEvent.instance.transform, 
//            //                                    0, 
//            //                                    id, 
//            //                                    true, 
//            //                                    true, 
//            //                                    false, 
//            //                                    false);

//            //LogManager.Error("building npc Created!Pos: " + pos + " id: " + RNpcId);
//            //createdNpcIdList.Add(nr.mNpcId);
//        }
//        else if (GameConfig.IsMultiMode)
//        {
//            //--to do
//            SPTerrainEvent.instance.CreateAdNpcByIndex(pos, id, 1);

//            //To do Ai create
//            //SPTerrainEvent.instance.CreateAiMultiMode(pos, id, 1);
//        }
//    }

//    IEnumerator CreateMapItemInBuilding(BuildingID buildingId, List<CreatItemInfo> itemInfoList, Vector3 root, int id, int rotation)
//    {
//        if (Pathea.PeGameMgr.IsSingleAdventure)
//        { 
//            for (int i = 0; i < itemInfoList.Count; i++)
//            {
//                //LogManager.Error(itemInfoList[i].mItemId, itemInfoList[i].mPos, itemInfoList[i].mRotation);
//                DragItem.Mgr.Instance.PutItemByProroId(itemInfoList[i].mItemId, itemInfoList[i].mPos, itemInfoList[i].mRotation);
//                yield return null;
//            }

//            mCreatedNpcItemBuildingIndex.Add(buildingId, 0);
//        }
//        else if (GameConfig.IsMultiMode)
//        {
//            while (true)
//            {
//                if (PlayerNetwork.MainPlayer == null)
//                {
//                    yield return null;
//                }else
//                {
//                    break;
//                }
//            }
//            PlayerNetwork.MainPlayer.RequestCreateBuildingWithItem(buildingId, itemInfoList, root, id, rotation);
//            yield return null;
//        }

//        yield return null;
//    }

//    public void RenderBlockBuilding(BuildingInfo buildinginfo) 
//    {
//        if (!mCreatedNpcItemBuildingIndex.ContainsKey(buildinginfo.buildingId))
//        {
//            //LogManager.Error("buildingID: ",b.id);
//            List<Vector3> npcPositionList;
//            List<CreatItemInfo> itemInfoList;
//            Dictionary<int, BuildingNpc> npcIdNum;
//            Dictionary<IntVector3, B45Block> retBuild = BuildBlockManager.self.BuildBuilding(buildinginfo.root, buildinginfo.id, buildinginfo.rotation, out buildinginfo.rootSize, out npcPositionList, out itemInfoList, out npcIdNum);

//            StartCoroutine(CreateOneBuilding(retBuild));

//            //random npc
//            if (npcIdNum.Count > 0)
//            {
//                List<int> npcIds = new List<int>();
//                foreach (KeyValuePair<int, int> npc in npcIdNum)
//                {
//                    for (int i = 0; i < npc.Value; i++)
//                    {
//                        npcIds.Add(npc.Key);
//                    }
//                }
//                if (npcPositionList.Count > npcIds.Count)
//                {
//                    int npcAmount = npcIds.Count;
//                    for (int i = 0; i < npcPositionList.Count - npcAmount; i++)
//                    {
//                        npcIds.Add(-1);
//                    }
//                }
//                System.Random posRand = new System.Random(Mathf.FloorToInt(buildinginfo.pos.x + buildinginfo.pos.y + buildinginfo.pos.z));
//                RandomTownUtil.Instance.Shuffle(npcIds, posRand);

//                //render npc

//                StartCoroutine(CreateNpcWithPosId(npcPositionList, npcIds));

//            }

//            //render item
//            //LogManager.Error("itemNum: " + itemInfoList.Count);
//            itemSum += itemInfoList.Count;
//            //LogManager.Error("itemSum: " + itemSum);
//            StartCoroutine(CreateMapItemInBuilding(buildinginfo.buildingId, itemInfoList, buildinginfo.root, buildinginfo.id, buildinginfo.rotation));

//        }


//        //LogManager.Debug(buildingNo,"building {0} is rendered!");
//    }

//    public void RenderPrefebBuilding(BuildingInfo buildinginfo)
//    {
//        int bid = buildinginfo.id;
        
//        Quaternion rotation = Quaternion.Euler(0, buildinginfo.rotation, 0);
//        Debug.Log ("RenderPrefebBuilding():"+bid);
//        if (Pathea.PeGameMgr.IsSingleAdventure) 
//        {
//            if (!buildinginfo.townInfo.nativeBuildingRendered)
//            {
//                if (buildinginfo.buildingId.buildingNo != 0)
//                {
//                    if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(bid))
//                    {
//                        LogManager.Error("bid = [", bid ,"] not exist in database!");
//                        return;
//                    }
//                    string path = BlockBuilding.s_tblBlockBuildingMap[bid].mPath;
//                    Scenestatic.instance.Register(path, buildinginfo.root, rotation, Vector3.one);
//                }
//                else if (buildinginfo.buildingId.buildingNo == 0)
//                {
//                    //tower
//                    //AIResource.Instantiate(buildinginfo.pathID, buildinginfo.root, rotation, buildinginfo.OnSpawned);
//                    if (!RandomTownManager.Instance.IsCaptured(buildinginfo.townInfo.Id))
//                    {
//                        SPPoint point = SPPoint.InstantiateSPPoint<SPPoint>(buildinginfo.root,
//                                                                            rotation,
//                                                                            IntVector4.Zero,
//                                                                            SPTerrainEvent.instance.transform,
//                                                                            0,
//                                                                            buildinginfo.pathID,
//                                                                            true,
//                                                                            true,
//                                                                            false,
//                                                                            false,
//                                                                            true,
//                                                                            null,
//                                                                            buildinginfo.OnSpawned);

//                        SPTerrainEvent.instance.RegisterSPPoint(point, true);
//                    }
//                }
//            }
//        }
//        else if (Pathea.PeGameMgr.IsMultiAdventure)
//        {
//            if (buildinginfo.buildingId.buildingNo != 0&& !buildinginfo.townInfo.nativeBuildingRendered)
//            {
//                //only gen once when normal building
//                if (IsRendered(buildinginfo.buildingId))
//                {
//                    return;
//                }
//                if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(bid)) {
//                    LogManager.Error("bid = [", bid, "] not exist in database!");
//                    return;
//                }
//                string path = BlockBuilding.s_tblBlockBuildingMap[bid].mPath;
//                Scenestatic.instance.Register(path, buildinginfo.root, rotation, Vector3.one);
//            }
//            else if(buildinginfo.buildingId.buildingNo==0)
//            {
//                if (!RandomTownManager.Instance.IsCaptured(buildinginfo.townInfo.Id))
//                {
//                    //SPTerrainRect rect = SPTerrainEvent.instance.GetSPTerrainRect(buildinginfo.root);
//                    SPTerrainEvent.instance.RegisterNativeTower(IntVector4.Zero, buildinginfo.root, buildinginfo.pathID, buildinginfo.townInfo.Id);
//                    Debug.Log("Create MultiMode NativeTower!!");
//                }
//            }
//        }
//    }

    

//    public void RenderReady(IntVector4 nodePosLod)
//    {
//        //IntVector3 chunkCenter = new IntVector3(nodePosLod.x, nodePosLod.y, nodePosLod.z);
//        IntVector2 tileIndexXZ = new IntVector2(nodePosLod.x>>VoxelTerrainConstants._shift,nodePosLod.z>>VoxelTerrainConstants._shift);

//        //if (mBuildingTile.ContainsKey(tileIndexXZ))
//        //{
//        //    foreach (BuildingID no in mBuildingTile[tileIndexXZ])
//        //    {
//        //        renderBuildings(no);
//        //    }
//        //}

        
//    }



//    //to judge if a building is rendered
//    public bool IsRendered(BuildingID buildingNo)
//    {
//        return renderManager[buildingNo];
//    }


//    //to judge if a positionXZ contains part of a building
//    public bool IsBuildingPos(IntVector2 posXZ)
//    {
//        return mBuildingBlockPos.ContainsKey(posXZ);
//    }

//    //public void RenderBuildingNpc(IntVector4 nodePosLod)
//    //{
        
//    //}



//    public byte[] Export()
//    {
//        MemoryStream ms = new MemoryStream();
//        BinaryWriter _out = new BinaryWriter(ms);

//        _out.Write(createdNpcIdList.Count);
//        foreach (int ite in createdNpcIdList)
//        {
//            int npcid = ite;
//            NpcMissionData data = NpcMissionDataRepository.GetMissionData(npcid);
//            if (data == null)
//            {
//                _out.Write(-1);
//            }
//            else
//            {
//                _out.Write(npcid);
//                _out.Write(data.m_Rnpc_ID);
//                _out.Write(data.m_QCID);
//                _out.Write(data.m_CurMissionGroup);
//                _out.Write(data.m_CurGroupTimes);
//                _out.Write(data.mCurComMisNum);
//                _out.Write(data.mCompletedMissionCount);
//                _out.Write(data.m_RandomMission);
//                _out.Write(data.m_RecruitMissionNum);

//                _out.Write(data.m_MissionList.Count);
//                for (int m = 0; m < data.m_MissionList.Count; m++)
//                    _out.Write(data.m_MissionList[m]);

//                _out.Write(data.m_MissionListReply.Count);
//                for (int m = 0; m < data.m_MissionListReply.Count; m++)
//                    _out.Write(data.m_MissionListReply[m]);
//            }
//        }

//        _out.Write(mCreatedNpcItemBuildingIndex.Count);
//        foreach (BuildingID b in mCreatedNpcItemBuildingIndex.Keys)
//        {
//            _out.Write(b.townId);
//            _out.Write(b.buildingNo);
//        }


//        _out.Close();
//        ms.Close();
//        byte[] retval = ms.ToArray();

//        return retval;
//    }

//    public void Import(byte[] buffer)
//    {
//        //LogManager.Error("BuildingInfoManager.Instance.Import()");
//        Clear();
//        if (buffer.Length == 0)
//            return;

//        MemoryStream ms = new MemoryStream(buffer);
//        BinaryReader _in = new BinaryReader(ms);

//        int iSize = _in.ReadInt32();
//        for (int i = 0; i < iSize; i++)
//        {
//            NpcMissionData data = new NpcMissionData();
//            int npcid = _in.ReadInt32();

//            if (npcid == -1)
//                continue;

//            data.m_Rnpc_ID = _in.ReadInt32();
//            data.m_QCID = _in.ReadInt32();
//            data.m_CurMissionGroup = _in.ReadInt32();
//            data.m_CurGroupTimes = _in.ReadInt32();
//            data.mCurComMisNum = _in.ReadByte();
//            data.mCompletedMissionCount = _in.ReadInt32();
//            data.m_RandomMission = _in.ReadInt32();
//            data.m_RecruitMissionNum = _in.ReadInt32();

//            int num = _in.ReadInt32();
//            for (int j = 0; j < num; j++)
//                data.m_MissionList.Add(_in.ReadInt32());

//            num = _in.ReadInt32();
//            for (int j = 0; j < num; j++)
//                data.m_MissionListReply.Add(_in.ReadInt32());

//            createdNpcIdList.Add(npcid);
//            NpcMissionDataRepository.AddMissionData(npcid, data);
//        }

//        iSize = _in.ReadInt32();
//        for (int i = 0; i < iSize; i++)
//        {
//            BuildingID no = new BuildingID() ;
//            no.townId= _in.ReadInt32();
//            no.buildingNo = _in.ReadInt32();
//            mCreatedNpcItemBuildingIndex.Add(no, 0);
//        }


//        _in.Close();
//        ms.Close();
//        //loadRecord = LOADMARK;
//    }

//    public void InitAdBuildingNpc()
//    {
//        StartCoroutine(InitAdNpc());
//    }

//    IEnumerator InitAdNpc()
//    {
//        //while (PlayerFactory.mMainPlayer == null)
//        //{
//        //    yield return new WaitForSeconds(0.1f);
//        //}

//        foreach (int npcid in createdNpcIdList)
//        {
//            NpcMissionData useData = NpcMissionDataRepository.GetMissionData(npcid);
//            if (useData == null)
//                continue;

//            //NpcManager.Instance.RequestRandomNpc(npcid, useData.m_Rnpc_ID, Vector3.zero, StroyManager.Instance.OnRandomNpcCreated, useData);

//            //NpcRandom npcRandom = NpcManager.Instance.CreateRandomNpc(npcid, useData.m_Rnpc_ID, Vector3.zero);
//            //if (npcRandom == null)
//            //    continue;

//            //npcRandom.UserData = useData;
//            //npcRandom.MouseCtrl.MouseEvent.SubscribeEvent(StroyManager.Instance.NpcMouseEventHandler);
//        }

//        yield return 0;
//    }

//    public Vector3 GetMissionBuildingPos(int missionId=0)
//    {
//        if(missionBuilding.ContainsKey(missionId)){
//            BuildingInfo bdinfo  = GetBuildingInfoByBuildingID(missionBuilding[missionId]);
            

//            if (bdinfo!=null)
//            {
//                Vector3 pos = bdinfo.frontDoorPos;
//                return pos;
//            }
//        }
//        return Vector3.zero;
//    }
//}
