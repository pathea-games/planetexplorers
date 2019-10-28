//using UnityEngine;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Xml;
//using System.Xml.Serialization;
//using System;
//using TownData;
//using RandomTownXML;
//using NativeCampXML;
 


//public class RandomTownManager:MonoBehaviour
//{
//    public RandomTownDsc randomTownInfo;
//    private Town town;
//    private int townTypeNum;
//    //for random
//    //Dictionary<int, int> pool;
//    //int weightSum;
//    //Dictionary<int, Dictionary<int, int>> LevelPool;
//    //Dictionary<int, int> LevelWeightSum;
//    Dictionary<int, WeightPool> LevelPool;
    
//    private NpcIdNum[] npcIdNum;
//    private BuildingNum[] buildingNum;
//    private Cell[] cell;

//    public NativeCampDesc randomCampInfo;

//    //start information
//    public int missionStartBuildingID=-1;
//    public int missionStartNpcID=-1;
//    public IntVector2 playerStartPos;

//    //the data manager which is to be optimized
//    private Dictionary<int, TownInfo> townIdInfo;
//    private Dictionary<IntVector2, TownInfo> townPosInfo;//townCenter,townInfo
//    public Dictionary<IntVector2, TownInfo> TownPosInfo
//    {
//        get { return townPosInfo; }
//    }

//    Dictionary<IntVector2, List<IntVector2>> townChunk;//chunkXZ,townCenter
//    public Dictionary<IntVector2, List<IntVector2>> TownChunk
//    {
//        get { return townChunk; }
//        set { townChunk = value; }
//    }

//    Dictionary<int, List<TownInfo>> levelNpcTown;

//    public Dictionary<int, int> mCreatedTownId;
//    public Dictionary<IntVector2,int> mRenderedChunk;
//    public Dictionary<int,int> capturedCampId;
//    //Dictionary<IntVector2, Dictionary<IntVector2, IntVector2>> indexTileTownCenter;

//    //public Dictionary<IntVector2, GameObject> unknownMask;

//    //const int IndexShift = 6;


//    //private SimplexNoise myNoise;
//    private int seed;
//    private System.Random myRand;

//    public const int levelCount = 5;

//    int minX = -19200;

//    public int MinX
//    {
//        get { return minX; }
//        set { minX = value; }
//    }

//    int minZ = -19200;

//    public int MinZ
//    {
//        get { return minZ; }
//        set { minZ = value; }
//    }
//    int maxX = 19200;

//    public int MaxX
//    {
//        get { return maxX; }
//        set { maxX = value; }
//    }
//    int maxZ = 19200;

//    public int MaxZ
//    {
//        get { return maxZ; }
//        set { maxZ = value; }
//    }
//    int levelRadius = 4000;

//    public int LevelRadius
//    {
//        get { return levelRadius; }
//        set { levelRadius = value; }
//    }
//    int detectedChunkNum = 32;

//    public int DetectedChunkNum
//    {
//        get { return detectedChunkNum; }
//        set { detectedChunkNum = value; }
//    }

//    int townDistanceX;
//    int townDistanceZ;

//    static RandomTownManager instance;
//    public static RandomTownManager Instance
//    {
//        get
//        {
//            return instance;
//        }
//    }


//    void Awake()
//    {
//        instance = this;
//        townIdInfo = new Dictionary<int, TownInfo>();
//        townPosInfo = new Dictionary<IntVector2, TownInfo>();
//        townChunk = new Dictionary<IntVector2, List<IntVector2>>();
//        playerStartPos = new IntVector2();
//        LevelPool = new Dictionary<int, WeightPool>();
//        levelNpcTown = new Dictionary<int, List<TownInfo>>();
//        mCreatedTownId = new Dictionary<int, int>();
//        mRenderedChunk = new Dictionary<IntVector2, int>();
//        capturedCampId = new Dictionary<int, int>();
//        //unknownMask = new Dictionary<IntVector2, GameObject>();
//        //NativeCampXMLParser.TestXxmlCreating ();

//        LoadXMLAtPath();
//    }

//    public void Clear()
//    {
//        townIdInfo = new Dictionary<int, TownInfo>();
//        townPosInfo = new Dictionary<IntVector2, TownInfo>();
//        townChunk = new Dictionary<IntVector2, List<IntVector2>>();
//        playerStartPos = new IntVector2();
//        LevelPool = new Dictionary<int, WeightPool>();
//        levelNpcTown = new Dictionary<int, List<TownInfo>>();
//        mCreatedTownId = new Dictionary<int, int>();
//        mRenderedChunk = new Dictionary<IntVector2, int>();
//        capturedCampId = new Dictionary<int, int>();
//        //unknownMask = new Dictionary<IntVector2, GameObject>();
//        RandomCampManager.Instance.Clear();
//    }

//    public void LoadXMLAtPath()
//    {
//        string xmlPath = "RandomTown/RandomTown";
//        TextAsset xmlResource = Resources.Load(xmlPath, typeof(TextAsset)) as TextAsset;
//        StringReader reader = new StringReader(xmlResource.text);
//        if (null == reader)
//            return;

//        XmlSerializer serializer = new XmlSerializer(typeof(RandomTownDsc));
//        randomTownInfo = (RandomTownDsc)serializer.Deserialize(reader);
//        reader.Close();
//        //LogManager.Error(randomTownInfo.town[1].buildingNum[1].bid);
//        //myNoise = new SimplexNoise(RandomMapConfig.RandSeed);
//        //LogManager.Error("RandSeed: "+RandomMapConfig.RandSeed);
        

//        townDistanceX = randomTownInfo.distanceX;
//        townDistanceZ = randomTownInfo.distanceZ;
//    }

//    public void CreateBuildings()
//    {
//        seed = RandomMapConfig.RandSeed;
//        townTypeNum = randomTownInfo.town.Count();
//        //LogManager.Error("Seed: " + seed);

//        for (int i = 0; i < randomTownInfo.town.Count(); i++)
//        {
//            Town t = randomTownInfo.town[i];
//            if (t.level >= levelCount)
//            {
//                LogManager.Error("RandomTownXml error!", t.tid);
//            }
//            if (!LevelPool.ContainsKey(t.level))
//            {
//                LevelPool.Add(t.level, new WeightPool());
//            }
//            LevelPool[t.level].Add(t.weight, t.tid);
//        }
//        RandomCampManager.Instance.Init();
//        //when it's single mode, "Record" thing should be considered
//        if (Pathea.PeGameMgr.IsSingleAdventure)
//        {
//            //LogManager.Debug("TownNpcManager.Instance.InitAdTownNpc()");
//            TownNpcManager.Instance.InitAdTownNpc();
//            //LogManager.Debug("BuildingInfoManager.Instance.InitAdTownNpc()");
//            BuildingInfoManager.Instance.InitAdBuildingNpc();
//        }
        
//        townPosInfo = new Dictionary<IntVector2, TownInfo>();
//        townChunk = new Dictionary<IntVector2, List<IntVector2>>();
//        int id = 0;
//        //LogManager.Debug(BuildingInfoManager.Instance.mBuildingInfoMap.Count());


//        myRand = new System.Random(seed);
//        bool hasStartTown = false;
//        for (int i = minX; i < maxX; i += townDistanceX * 2)
//        {
//            for (int j = minZ; j < maxZ; j += townDistanceZ * 2)
//            {
//                int randomX = myRand.Next(townDistanceX*2-randomTownInfo.townSize);
//                int randomZ = myRand.Next(townDistanceZ * 2 - randomTownInfo.townSize);
//                //generate random pos of town
//                IntVector2 townPosStart = new IntVector2(i + randomX, j + randomZ);
//                if(!VFDataRTGen.IsTownAvailable(townPosStart.x,townPosStart.y)){
//                    continue;
//                }
//                if (IsTownChunk(new IntVector2(townPosStart.x << VoxelTerrainConstants._shift, townPosStart.x << VoxelTerrainConstants._shift)))
//                {
//                    continue;
//                }
//                //if (IsTownChunk(new IntVector2(townPosStart.x + 20 << VoxelTerrainConstants._shift, townPosStart.x + 20 << VoxelTerrainConstants._shift)))
//                //{
//                //    continue;
//                //}
//                //if (IsTownChunk(new IntVector2(townPosStart.x + VoxelTerrainConstants._numVoxelsPerAxis << VoxelTerrainConstants._shift, townPosStart.x << VoxelTerrainConstants._shift)))
//                //{
//                //    continue;
//                //}
//                //if (IsTownChunk(new IntVector2(townPosStart.x << VoxelTerrainConstants._shift, townPosStart.x + VoxelTerrainConstants._numVoxelsPerAxis << VoxelTerrainConstants._shift)))
//                //{
//                //    continue;
//                //} 
//                int level = 0;
//                if (townPosStart.x < levelRadius && townPosStart.x >= -levelRadius
//                    && townPosStart.y < levelRadius && townPosStart.y >= -levelRadius)
//                {
//                    level = 0;
//                }
//                else if (townPosStart.x < levelRadius * 2 && townPosStart.x >= -levelRadius * 2
//                   && townPosStart.y < levelRadius * 2 && townPosStart.y >= -levelRadius * 2)
//                {
//                    level = 1;
//                }
//                else if (townPosStart.x < levelRadius * 3 && townPosStart.x >= -levelRadius * 3
//                   && townPosStart.y < levelRadius * 3 && townPosStart.y >= -levelRadius * 3)
//                {
//                    level = 2;
//                }
//                else if (townPosStart.x < levelRadius * 4 && townPosStart.x >= -levelRadius * 4
//                   && townPosStart.y < levelRadius * 4 && townPosStart.y >= -levelRadius * 4)
//                {
//                    level = 3;
//                }else
//                {
//                    level = 4;
//                }

//                int tid=0;
//                if (Pathea.PeGameMgr.IsSingleAdventure)
//                {
//                    if (!hasStartTown && i >= -2000 && j >= -2000)
//                    {
//                        tid = -1;
//                        town = randomTownInfo.startTown;
//                        missionStartNpcID = town.npcIdNum[0].nid;
//                        missionStartBuildingID = town.buildingNum[0].bid;
//                        hasStartTown = true;
//                    }
//                    else
//                    {
//                        tid = LevelPool[level].GetRandID(myRand);
//                        town = randomTownInfo.town[tid];
//                    }
//                }
//                else {
//                    tid = LevelPool[level].GetRandID(myRand);
//                    town = randomTownInfo.town[tid];
//                }
                
//                npcIdNum = town.npcIdNum;
//                buildingNum = town.buildingNum;
//                cell = town.cell;

//                //TownInfo townInfo = new TownInfo(TownType.NpcTown,townPosStart, town.cellNumX, town.cellNumZ, town.cellSizeX, town.cellSizeZ, townId);
//                TownInfo townInfo = new TownInfo(town,townPosStart);
//                if (IsContained(townInfo))
//                {
//                    if (tid == -1)
//                    {
//                        hasStartTown = false;
//                    }
//                    continue;
//                }

//                townInfo.Id = id++;
//                if (townInfo.Tid == -1)
//                {
//                    playerStartPos = townInfo.PosCenter;
//                }

//                //Debug.Log("ID: "+townInfo.Id+" TownPosCenter: " + townInfo.PosCenter + "tid: " + tid);
                
//                //save the town's Pos and template Data int the dataManager part 
//                townPosInfo.Add(townInfo.PosCenter, townInfo);
//                if (!levelNpcTown.ContainsKey(level))
//                {
//                    List<TownInfo> townInfoList = new List<TownInfo>();
//                    townInfoList.Add(townInfo);
//                    levelNpcTown.Add(level, townInfoList);
//                }
//                else {
//                    levelNpcTown[level].Add(townInfo);
//                }
//                townIdInfo.Add(townInfo.Id,townInfo);
//                LinkToChunk(townInfo);
//                AIErodeMap.AddErode(new Vector3(townInfo.PosCenter.x, VFDataRTGen.GetPosHeight(townInfo.PosCenter), townInfo.PosCenter.y), townInfo.radius + 64);

//                //generate  nativeCamp
//                List<TownInfo> nciList = RandomCampManager.Instance.GenNativeCamp(townPosStart,level);

//                //to do-- save the NativeCamp Data to dataManager part
//                for (int ii = 0; ii < nciList.Count; ii++)
//                {
//                    nciList[ii].Id = id++;
//                    townPosInfo.Add(nciList[ii].PosCenter, nciList[ii]);
//                    townIdInfo.Add(nciList[ii].Id, nciList[ii]);
//                    LinkToChunk(nciList[ii]);
//                    //to do-- erase the monster;
//                    AIErodeMap.AddErode(new Vector3(nciList[ii].PosCenter.x, VFDataRTGen.GetPosHeight(nciList[ii].PosCenter), nciList[ii].PosCenter.y), nciList[ii].radius + 64);
//                }
                    
//                //int townSize = town.cellNumX * town.cellNumZ;
//                //int cellAmount = cell.Count(); 
//                //List<int> buildingList = new List<int>();
//                //int buildingAmount = 0;
//                //int buildingTypeNum = buildingNum.Count();
//                //for (int t = 0; t < buildingTypeNum;t++ )
//                //{
//                //    for (int n = 0; n < town.buildingNum[t].num; n++)
//                //    {
//                //        buildingList.Add(town.buildingNum[t].bid);
//                //    }
//                //    buildingAmount += town.buildingNum[t].num;
//                //}

//                //if (cellAmount>buildingAmount)
//                //{
//                //    for (int t = 0; t < cellAmount - buildingAmount; t++)
//                //    {
//                //        buildingList.Add(-1);
//                //    }
//                //}
                
//                //RandomTownUtil.Instance.shuffle(buildingList,myRand);

//                //CreateBuildingInfo(townInfo, cell,buildingList);

//                //CreateTownNPC(townInfo, npcIdNum);


//            }
//        }
//        //LogManager.Error(num1, " ", num2, " ", num3," ",num4);


//        //if(Pathea.PeGameMgr.IsSingleAdventure)
//        //{
//        //    List<int> idList = new List<int>();
//        //    idList.AddRange(mCreatedTownId.Keys);
//        //    if(idList.Count>0)
//        //    {
//        //        for (int p = 0; p < idList.Count; p++)
//        //        {
//        //            TownInfo tInfo = townIdInfo[idList[p]];
//        //            Debug.Log("mCreatedTownId:"+ idList[p]+" center:"+tInfo.PosCenter);
//        //            tInfo.IsExplored = true;
//        //            DetectTowns(tInfo.PosCenter);
//        //        }
//        //    }
//        //}

//        if (Application.isEditor)
//        {
//            PrintTownPos();
//        }
//    }

//    public bool IsContained(TownInfo townInfo)
//    {
//        List<IntVector2> chunkIndexList = LinkedChunkIndex(townInfo);
//        for (int i = 0; i < chunkIndexList.Count; i++)
//        {
//            IntVector2 chunkIndex = chunkIndexList[i];
//            if (townChunk.ContainsKey(chunkIndex))
//            {
//                return true;
//            }
//        }
//        return false;
//    }

//    public List<IntVector2> LinkedChunkIndex(TownInfo townInfo)
//    {
//        IntVector2 startPos = townInfo.PosStart;
//        IntVector2 endPos = townInfo.PosEnd;

//        List<IntVector2> startIndexList = Link1PointToChunk(startPos);
//        List<IntVector2> endIndexList = Link1PointToChunk(endPos);

//        IntVector2 startIndex = GetMinChunkIndex(startIndexList);
//        IntVector2 endIndex = GetMaxChunkIndex(endIndexList);

//        List<IntVector2> chunkIndexList =  GetChunkIndexListFromStartEnd(startIndex, endIndex);

//        return chunkIndexList;
//    }

//    public static List<IntVector2> GetChunkIndexListFromStartEnd(IntVector2 startIndex, IntVector2 endIndex)
//    {
//        List<IntVector2> ChunkIndexList = new List<IntVector2> ();
//        for (int x = startIndex.x; x <= endIndex.x; x++)
//        {
//            for (int z = startIndex.y; z <= endIndex.y; z++)
//            {
//                IntVector2 index = new IntVector2 (x,z);
//                if (!ChunkIndexList.Contains(index))
//                    ChunkIndexList.Add(index);
//            }
//        }
//        return ChunkIndexList;
//    }

//    public static IntVector2 GetMinChunkIndex(List<IntVector2> startIndexList)
//    {
//        if (startIndexList == null || startIndexList.Count <= 0)
//            return null;
//        IntVector2 minIndex = new IntVector2();
//        minIndex = startIndexList[0];
//        for (int i = 0; i < startIndexList.Count; i++)
//        {
//            if (startIndexList[i].x <= minIndex.x && startIndexList[i].y <= minIndex.y)
//                minIndex = startIndexList[i];
//        }

//        return minIndex;
//    }

//    public static IntVector2 GetMaxChunkIndex(List<IntVector2> endIndexList)
//    {
//        if (endIndexList == null || endIndexList.Count <= 0)
//            return null;
//        IntVector2 maxIndex = new IntVector2();
//        maxIndex = endIndexList[0];
//        for (int i = 0; i < endIndexList.Count; i++)
//        {
//            if (endIndexList[i].x >= maxIndex.x && endIndexList[i].y >= maxIndex.y)
//                maxIndex = endIndexList[i];
//        }

//        return maxIndex;
//    }


//    public static List<IntVector2> Link1PointToChunk(IntVector2 pos)
//    {
//        List<IntVector2> indexList = new List<IntVector2>();
//        int i = pos.x;
//        int j = pos.y;
//        int x = i >> VoxelTerrainConstants._shift;
//        int z = j >> VoxelTerrainConstants._shift;
//        IntVector2 chunkIndexXZ = new IntVector2(x, z);
//        indexList.Add(chunkIndexXZ);

//        int x2 = x;
//        if ((i + VoxelTerrainConstants._numVoxelsPrefix) % VoxelTerrainConstants._numVoxelsPerAxis == 0)
//        {
//            x2 = (i + VoxelTerrainConstants._numVoxelsPrefix) >> VoxelTerrainConstants._shift;
//        }
//        else if (i % VoxelTerrainConstants._numVoxelsPerAxis < VoxelTerrainConstants._numVoxelsPostfix)
//        {
//            x2 = (i - 2) >> VoxelTerrainConstants._shift;
//        }

//        int z2 = z;
//        if ((j + VoxelTerrainConstants._numVoxelsPrefix) % VoxelTerrainConstants._numVoxelsPerAxis == 0)
//        {
//            z2 = (j + VoxelTerrainConstants._numVoxelsPrefix) >> VoxelTerrainConstants._shift;
//        }
//        else if (j % VoxelTerrainConstants._numVoxelsPerAxis < VoxelTerrainConstants._numVoxelsPostfix)
//        {
//            z2 = (j - VoxelTerrainConstants._numVoxelsPostfix) >> VoxelTerrainConstants._shift;
//        }


//        if (x2 != x && z2 == z)
//        {
//            chunkIndexXZ = new IntVector2(x2, z);
//            if (!indexList.Contains(chunkIndexXZ))
//            {
//                indexList.Add(chunkIndexXZ);
//            }
//        }
//        else if (x2 == x && z2 != z)
//        {
//            chunkIndexXZ = new IntVector2(x, z2);
//            if (!indexList.Contains(chunkIndexXZ))
//            {
//                indexList.Add(chunkIndexXZ);
//            }
//        }
//        else if (x2 != x && z2 != z)
//        {
//            chunkIndexXZ = new IntVector2(x2, z);
//            if (!indexList.Contains(chunkIndexXZ))
//            {
//                indexList.Add(chunkIndexXZ);
//            }

//            chunkIndexXZ = new IntVector2(x, z2);
//            if (!indexList.Contains(chunkIndexXZ))
//            {
//                indexList.Add(chunkIndexXZ);
//            }

//            chunkIndexXZ = new IntVector2(x2, z2);
//            if (!indexList.Contains(chunkIndexXZ))
//            {
//                indexList.Add(chunkIndexXZ);
//            }
//        }
//        return indexList;
//    }




//    public void LinkToChunk(TownInfo townInfo)
//    {
//        List<IntVector2> chunkIndexList = LinkedChunkIndex(townInfo);
//        for (int i = 0; i < chunkIndexList.Count; i++)
//        {
//            IntVector2 chunkIndexXZ = chunkIndexList[i];
//            if (!townChunk.ContainsKey(chunkIndexXZ))
//            {
//                List<IntVector2> ls = new List<IntVector2>();
//                ls.Add(townInfo.PosCenter);
//                townChunk.Add(chunkIndexXZ, ls);
//            }
//        }


//        //for (int i = townInfo.PosStart.x; i < townInfo.PosEnd.x; i++)
//        //{
//        //    for (int j = townInfo.PosStart.y; j < townInfo.PosEnd.y; j++)
//        //    {
//        //        int x = i >> VoxelTerrainConstants._shift;
//        //        int z = j >> VoxelTerrainConstants._shift;
//        //        IntVector2 chunkIndexXZ = new IntVector2(x, z);
//        //        if (!townChunk.ContainsKey(chunkIndexXZ))
//        //        {
//        //            List<IntVector2> ls  = new List<IntVector2>();
//        //            ls.Add(townInfo.PosCenter);
//        //            townChunk.Add(chunkIndexXZ, ls);
//        //        }else{
//        //            if (!townChunk[chunkIndexXZ].Contains(townInfo.PosCenter))
//        //            {
//        //                townChunk[chunkIndexXZ].Add(townInfo.PosCenter);
//        //            }
//        //        }

//        //        int x2 = x;
//        //        if ((i + VoxelTerrainConstants._numVoxelsPrefix) % VoxelTerrainConstants._numVoxelsPerAxis == 0)
//        //        {
//        //            x2 = (i + VoxelTerrainConstants._numVoxelsPrefix) >> VoxelTerrainConstants._shift;
//        //        }
//        //        else if (i % VoxelTerrainConstants._numVoxelsPerAxis < VoxelTerrainConstants._numVoxelsPostfix)
//        //        {
//        //            x2 = (i - 2) >> VoxelTerrainConstants._shift;
//        //        }

//        //        int z2 = z;
//        //        if ((j + VoxelTerrainConstants._numVoxelsPrefix) % VoxelTerrainConstants._numVoxelsPerAxis == 0)
//        //        {
//        //            z2 = (j + VoxelTerrainConstants._numVoxelsPrefix) >> VoxelTerrainConstants._shift;
//        //        }
//        //        else if (j % VoxelTerrainConstants._numVoxelsPerAxis < VoxelTerrainConstants._numVoxelsPostfix)
//        //        {
//        //            z2 = (j - VoxelTerrainConstants._numVoxelsPostfix) >> VoxelTerrainConstants._shift;
//        //        }


//        //        if (x2 != x && z2 == z)
//        //        {
//        //            chunkIndexXZ = new IntVector2(x2, z);
//        //            if (!townChunk.ContainsKey(chunkIndexXZ))
//        //            {
//        //                List<IntVector2> ls = new List<IntVector2>();
//        //                ls.Add(townInfo.PosCenter);
//        //                townChunk.Add(chunkIndexXZ, ls);
//        //            }
//        //            else
//        //            {
//        //                if (!townChunk[chunkIndexXZ].Contains(townInfo.PosCenter))
//        //                {
//        //                    townChunk[chunkIndexXZ].Add(townInfo.PosCenter);
//        //                }
//        //            }
//        //        }
//        //        else if (x2 == x && z2 != z)
//        //        {
//        //            chunkIndexXZ = new IntVector2(x, z2);
//        //            if (!townChunk.ContainsKey(chunkIndexXZ))
//        //            {
//        //                List<IntVector2> ls = new List<IntVector2>();
//        //                ls.Add(townInfo.PosCenter);
//        //                townChunk.Add(chunkIndexXZ, ls);
//        //            }
//        //            else
//        //            {
//        //                if (!townChunk[chunkIndexXZ].Contains(townInfo.PosCenter))
//        //                {
//        //                    townChunk[chunkIndexXZ].Add(townInfo.PosCenter);
//        //                }
//        //            }
//        //        }
//        //        else if (x2 != x && z2 != z)
//        //        {
//        //            chunkIndexXZ = new IntVector2(x2, z);
//        //            if (!townChunk.ContainsKey(chunkIndexXZ))
//        //            {
//        //                List<IntVector2> ls = new List<IntVector2>();
//        //                ls.Add(townInfo.PosCenter);
//        //                townChunk.Add(chunkIndexXZ, ls);
//        //            }
//        //            else
//        //            {
//        //                if (!townChunk[chunkIndexXZ].Contains(townInfo.PosCenter))
//        //                {
//        //                    townChunk[chunkIndexXZ].Add(townInfo.PosCenter);
//        //                }
//        //            }

//        //            chunkIndexXZ = new IntVector2(x, z2);
//        //            if (!townChunk.ContainsKey(chunkIndexXZ))
//        //            {
//        //                List<IntVector2> ls = new List<IntVector2>();
//        //                ls.Add(townInfo.PosCenter);
//        //                townChunk.Add(chunkIndexXZ, ls);
//        //            }
//        //            else
//        //            {
//        //                if (!townChunk[chunkIndexXZ].Contains(townInfo.PosCenter))
//        //                {
//        //                    townChunk[chunkIndexXZ].Add(townInfo.PosCenter);
//        //                }
//        //            }

//        //            chunkIndexXZ = new IntVector2(x2, z2);
//        //            if (!townChunk.ContainsKey(chunkIndexXZ))
//        //            {
//        //                List<IntVector2> ls = new List<IntVector2>();
//        //                ls.Add(townInfo.PosCenter);
//        //                townChunk.Add(chunkIndexXZ, ls);
//        //            }
//        //            else
//        //            {
//        //                if (!townChunk[chunkIndexXZ].Contains(townInfo.PosCenter))
//        //                {
//        //                    townChunk[chunkIndexXZ].Add(townInfo.PosCenter);
//        //                }
//        //            }
//        //        }
//        //    }
//        //}
//    }

//    //to judge wheather a tile has town
//    public bool IsTownChunk(IntVector2 tileIndex) 
//    {
//        if (townChunk.ContainsKey(tileIndex)) 
//        {
//            return true;
//        }
//        return false;
//    }


//    public List<IntVector2> GetTownCenterByTile(IntVector2 tileIndex)
//    {
//        return townChunk[tileIndex];
//    }

//    public IntVector2 GetTownCenterByTileAndPos(IntVector2 tileIndex,IntVector2 worldXZ) 
//    {
//        List<IntVector2> townCenterList = GetTownCenterByTile(tileIndex);
//        for (int i = 0; i < townCenterList.Count; i++)
//        {
//            IntVector2 townCenter = townCenterList[i];
//            TownInfo ti = RandomTownManager.Instance.GetTownInfoByCenter(townCenter);
//            IntVector2 posStart = ti.PosStart;
//            IntVector2 posEnd = ti.PosEnd;
//            if (worldXZ.x >= posStart.x && worldXZ.y >= posStart.y && worldXZ.x <= posEnd.x && worldXZ.y <= posEnd.y)
//            {
//                return ti.PosCenter;
//            }
//        }
//        return null;
//    }

//    public bool IsNpcTownTile(IntVector2 tileIndex)
//    { 
//        List<IntVector2> townCenterList = GetTownCenterByTile(tileIndex);
//        for (int i = 0; i < townCenterList.Count; i++)
//        {
//            if (GetTownInfoByCenter(townCenterList[i]).Type == TownType.NpcTown)
//            {
//                return true;
//            }
//        }
//        return false;
//    }

//    public bool IsNpcTownTile(VFTerTile terTile)
//    {
//        IntVector2 tileIndex = new IntVector2(terTile.tileX, terTile.tileZ);
//        List<IntVector2> townCenterList = GetTownCenterByTile(tileIndex);
//        for (int i = 0; i < townCenterList.Count; i++)
//        {
//            if (GetTownInfoByCenter(townCenterList[i]).Type == TownType.NpcTown)
//            {
//                return true;
//            }
//        }
//        return false;
//    }

//    public TownInfo GetTownInfoByCenter(IntVector2 townCenter) {
//        if(!TownPosInfo.ContainsKey(townCenter)){
//            return null;
//        }
//        return TownPosInfo[townCenter];
//    }




//    //gen town from a tileIndexXZ
//    public void GenTownFromTileIndex(IntVector2 tileIndex) {
//        //test new procedure
//        //return;

//        if(!IsTownChunk(tileIndex)){return;}
//        List<IntVector2> townCenterList = GetTownCenterByTile(tileIndex);
//        for (int i = 0; i < townCenterList.Count;i++ )
//        {
//            TownInfo townInfo = GetTownInfoByCenter(townCenterList[i]);
//            if (townInfo != null) {
//                if (townInfo.Height == -1) {
//                    int top = VFDataRTGen.GetPosHeight(townInfo.PosCenter);
//                    if (top > VFDataRTGen.MaxTownHeight)
//                    {
//                        top = VFDataRTGen.MaxTownHeight;
//                    }
//                    townInfo.Height = top;
//                    Debug.LogError("voxelcache townHeight:" + townInfo.Height+townInfo.PosCenter);
//                    RenderTheTown(townInfo, new IntVector2((tileIndex.x << VoxelTerrainConstants._shift)+1,(tileIndex.y<<VoxelTerrainConstants._shift)+1));
//                }
//            }
//        }
//    }

//    public void RenderTheTown(TownInfo townInfo, IntVector2 worldXZ)
//    { 
            
//            //to do==check
//            if (townInfo.Height == -1) {
//                return;
//            }
//            if(townInfo.IsAdded)
//            {
//                return;
//            }

//            //if (townInfo.Type == TownType.NpcTown) 
//            //{
//            //    //if (!mCreatedTownId.ContainsKey(townInfo.Id))
//            //    //{
//            //        RandomTownInfo(townInfo);
//            //    //}
//            //    //if(!townInfo.IsExplored)
//            //    //{
//            //        AddToRenderReady(townInfo, worldXZ);
//            //    //}
//            //} 
//            //else 
//            //{
//            //    //if(townInfo.nativeBuildingRendered)
//            //    //{
//            //    //    return;
//            //    //}
//            //    //else
//            //    //{
//            //        RandomTownInfo(townInfo);
//                    AddToRenderReady(townInfo, worldXZ);
//                //}
//            //}
            
        
//            townInfo.IsAdded = true;

//        //1--random the content of the town
//        //2-- render content
//    }

//    public void RandomTownInfo(TownInfo townInfo) {
//        int townSize = townInfo.CellNumX * townInfo.CellNumZ;
//        int cellAmount = townInfo.cellInfo.Count;
//        List<int> buildingList = new List<int>();
//        int buildingAmount = 0;
//        int buildingTypeNum = townInfo.buildingNum.Count;

//        for (int t = 0; t < buildingTypeNum; t++)
//        {
//            for (int n = 0; n < townInfo.buildingNum[t].num; n++)
//            {
//                buildingList.Add(townInfo.buildingNum[t].bid);
//            }
//            buildingAmount += townInfo.buildingNum[t].num;
//        }

//        if (cellAmount > buildingAmount)
//        {
//            for (int t = 0; t < cellAmount - buildingAmount; t++)
//            {
//                buildingList.Add(-1);
//            }
//        }


//        System.Random randSeed = new System.Random(townInfo.PosCenter.x+townInfo.PosCenter.y);

//        RandomTownUtil.Instance.Shuffle(buildingList, randSeed);

//        CreateBuildingInfo(townInfo,buildingList);

//        CreateTownNPCAndNative(townInfo,randSeed);

//        townInfo.IsRandomed = true;
//    }

//    public void CreateBuildingInfo(TownInfo townInfo, List<int> bid)
//    {
//        //if (townInfo.Tid ==-1)
//        //{
//        //    LogManager.Debug();
//        //}

//        if (townInfo.Type == TownType.NpcTown) { 
//            //int npcTownId = townInfo.Tid;
//            //IntVector2 townPosStart = townInfo.PosStart;
//            //IntVector2 townPosCenter = townInfo.PosCenter;

//            //List<Cell> cells = townInfo.cellInfo;
//            //int cellNum = cells.Count;
//            //int cellSizeX = townInfo.CellSizeX;
//            //int cellSizeZ = townInfo.CellSizeZ;

//            //int buildingNo = 0;
//            //for (int i = 0; i < cellNum; i++)
//            //{
//            //    if (bid[i] == -1)
//            //    {
//            //        continue;
//            //    }
//            //    int x = townPosStart.x + cells[i].x * cellSizeX;
//            //    int z = townPosStart.y + cells[i].z * cellSizeZ;
//            //    IntVector2 bPos = new IntVector2(x, z);
//            //    int rot = cells[i].rot;
//            //    int id = bid[i];
//            //    BuildingID buildingId = new BuildingID(townInfo.Id,buildingNo);
//            //    BuildingInfo b1 = new BuildingInfo(bPos, id, rot, cellSizeX, cellSizeZ, buildingId, BuildingType.Block);
//            //    buildingNo++;
//            //    BuildingInfoManager.Instance.AddBuilding(b1);

//            //    if (npcTownId == -1 && id == missionStartBuildingID)
//            //    {
//            //        BuildingInfoManager.Instance.missionBuilding.Add(0, b1.buildingId);
//            //    }

//            //    b1.setHeight(townInfo.Height - 0.5f);


//            //    townInfo.BuildingIdList.Add(b1.buildingId);
//            //    townInfo.AddBuildingCell(cells[i].x, cells[i].z);
//            //}
//            ////if (townInfo.Tid == -1)
//            ////{
//            ////    LogManager.Error(townInfo.BuildingIdList);
//            ////}
//            //townInfo.AddStreetCell();


//            int npcTownId = townInfo.Tid;
//            IntVector2 townPosStart = townInfo.PosStart;
//            IntVector2 townPosCenter = townInfo.PosCenter;

//            List<Cell> cells = townInfo.cellInfo;
//            int cellNum = cells.Count;
//            int cellSizeX = townInfo.CellSizeX;
//            int cellSizeZ = townInfo.CellSizeZ;

//            int buildingNo = 1;
//            for (int i = 0; i < cellNum; i++)
//            {
//                if (bid[i] == -1)
//                {
//                    continue;
//                }
//                int x = townPosStart.x + cells[i].x * cellSizeX;
//                int z = townPosStart.y + cells[i].z * cellSizeZ;
//                IntVector2 bPos = new IntVector2(x, z);
//                int rot = cells[i].rot;
//                int id = bid[i];
//                BuildingID buildingId = new BuildingID(townInfo.Id, buildingNo);
//                BuildingInfo b1 = new BuildingInfo(bPos, bid[i], rot, cellSizeX, cellSizeZ, buildingId, BuildingType.Prefeb);
//                buildingNo++;
//                BuildingInfoManager.Instance.AddBuilding(b1);
//                b1.setHeight(townInfo.Height + 0.15f);

//                if (npcTownId == -1 && id == missionStartBuildingID)
//                {
//                    BuildingInfoManager.Instance.missionBuilding.Add(0, b1.buildingId);
//                }
//                b1.townInfo = townInfo;

//                townInfo.BuildingIdList.Add(b1.buildingId);
//                townInfo.AddBuildingCell(cells[i].x, cells[i].z);
//            }
//            //if (townInfo.Tid == -1)
//            //{
//            //    LogManager.Error(townInfo.BuildingIdList);
//            //}
//            townInfo.AddStreetCell();
//        }
//        else
//        {
//            int NativeCampId = townInfo.Cid;
//            IntVector2 townPosStart = townInfo.PosStart;
//            IntVector2 townPosCenter = townInfo.PosCenter;

//            List<Cell> cells = townInfo.cellInfo;
//            int cellNum = cells.Count;
//            int cellSizeX = townInfo.CellSizeX;
//            int cellSizeZ = townInfo.CellSizeZ;

//            int buildingNo = 0;


//            //add tower
//            NativeTower nt = townInfo.nativeTower;
//            int tx = townPosStart.x + nt.x * cellSizeX;
//            int tz = townPosStart.y + nt.z * cellSizeZ;
//            IntVector2 tPos = new IntVector2(tx, tz);
//            int trot = nt.rot;
//            int tid = nt.bid;
//            BuildingID tId = new BuildingID(townInfo.Id, buildingNo);
//            BuildingInfo tb1 = new BuildingInfo(tPos, tid, trot, cellSizeX, cellSizeZ, tId, BuildingType.Prefeb);
//            tb1.townInfo = townInfo;
//            tb1.pathID = nt.pathID;
//            tb1.setHeight(townInfo.Height + 0.15f);
//            buildingNo++;
//            BuildingInfoManager.Instance.AddBuilding(tb1);

//            townInfo.BuildingIdList.Add(tb1.buildingId);
//            townInfo.AddBuildingCell(nt.x, nt.z);
            
//            //add building
//            for (int i = 0; i < cellNum; i++)
//            {
//                if (bid[i] == -1)
//                {
//                    continue;
//                }
//                int x = townPosStart.x + cells[i].x * cellSizeX;
//                int z = townPosStart.y + cells[i].z * cellSizeZ;
//                IntVector2 bPos = new IntVector2(x, z);
//                int rot = cells[i].rot;
//                int id = bid[i];
//                BuildingID buildingId = new BuildingID(townInfo.Id,buildingNo);
//                BuildingInfo b1 = new BuildingInfo(bPos, id, rot, cellSizeX, cellSizeZ, buildingId, BuildingType.Prefeb);
//                b1.townInfo = townInfo;
//                b1.setHeight(townInfo.Height+0.15f);
//                buildingNo++;
//                BuildingInfoManager.Instance.AddBuilding(b1);

//                townInfo.BuildingIdList.Add(b1.buildingId);
//                townInfo.AddBuildingCell(cells[i].x, cells[i].z);
//            }
            

//            townInfo.AddStreetCell();
//        }
//    }

//    private void CreateTownNPCAndNative(TownInfo townInfo,System.Random randSeed)
//    {
//        if (TownType.NpcTown == townInfo.Type)
//        {
//            IntVector2 townPosCenter = townInfo.PosCenter;
//            IntVector2 townPosStart = townInfo.PosStart;
//            IntVector2 npcPosXZ;
//            List<IntVector2> streetCellList = townInfo.StreetCellList;
//            int cellSizeX = townInfo.CellSizeX;
//            int cellSizeZ = townInfo.CellSizeZ;
//            List<NpcIdNum> npcIdNum = townInfo.npcIdNum;
//            for (int i = 0; i < npcIdNum.Count; i++)
//            {
//                int nid = npcIdNum[i].nid;
//                int num = npcIdNum[i].num;
//                for (int j = 0; j < num; j++)
//                {
//                    if (townInfo.Tid == -1 && nid == missionStartNpcID)
//                    {
//                        npcPosXZ = new IntVector2(Mathf.RoundToInt(townInfo.PosCenter.x), Mathf.RoundToInt(townInfo.PosCenter.y + 1));

//                        if (Pathea.PeGameMgr.IsSingleAdventure && !TownNpcManager.Instance.IsCreated(npcPosXZ))
//                        {
//                            TownNpcInfo townNpcInfo = new TownNpcInfo(npcPosXZ, nid);
//                            townNpcInfo.setPosY(townInfo.Height+0.5f);
//                            TownNpcManager.Instance.AddNpc(townNpcInfo);
//                            townInfo.TownPosList.Add(npcPosXZ);
//                            //LogManager.Error("!!!!!!!StartMissionNPC!!!!!!! id=" + nid + " pos=" + npcPosXZ);
//                        }
//                        continue;
//                    }
//                    IntVector2 CellXZ = streetCellList[randSeed.Next(streetCellList.Count)];
//                    IntVector2 startPosXZ = townPosStart + new IntVector2(CellXZ.x * cellSizeX, CellXZ.y * cellSizeZ);
//                    npcPosXZ = startPosXZ + new IntVector2(randSeed.Next(cellSizeX), randSeed.Next(cellSizeZ));
//                    if (TownNpcManager.Instance.GetNpcInfoByPos(npcPosXZ) != null)
//                    {
//                        j--;
//                        continue;
//                    }
//                    if ((Pathea.PeGameMgr.IsSingleAdventure||GameConfig.IsMultiMode) && !TownNpcManager.Instance.IsCreated(npcPosXZ))
//                    {
//                        TownNpcInfo townNpcInfo = new TownNpcInfo(npcPosXZ, nid);
//                        townNpcInfo.setPosY(townInfo.Height + 0.5f);
//                        TownNpcManager.Instance.AddNpc(townNpcInfo);
//                        townInfo.TownPosList.Add(npcPosXZ);
//                        //LogManager.Debug(nid + " " + npcPosXZ);
//                    }
//                }
//            }
//        }
//        else { 
//            //to do-- NativeCamp
//            //gen the point to produce native
//            IntVector2 townPosCenter = townInfo.PosCenter;
//            IntVector2 townPosStart = townInfo.PosStart;
//            IntVector2 nativePosXZ;
//            List<IntVector2> streetCellList = townInfo.StreetCellList;
//            int cellSizeX = townInfo.CellSizeX;
//            int cellSizeZ = townInfo.CellSizeZ;
//            List<NativeIdNum> nativeIdNum = townInfo.nativeIdNum;
//            for (int i = 0; i < nativeIdNum.Count; i++)
//            {
//                int aid = nativeIdNum[i].aid;
//                int num = nativeIdNum[i].num;
//                for (int j = 0; j < num; j++)
//                {
//                    IntVector2 CellXZ = streetCellList[randSeed.Next(streetCellList.Count)];
//                    IntVector2 startPosXZ = townPosStart + new IntVector2(CellXZ.x * cellSizeX, CellXZ.y * cellSizeZ);
//                    nativePosXZ = startPosXZ + new IntVector2(randSeed.Next(cellSizeX), randSeed.Next(cellSizeZ));
//                    if (NativePointManager.Instance.GetNativePointByPosXZ(nativePosXZ) != null)
//                    {
//                        j--;
//                        continue;
//                    }
//                    if ((Pathea.PeGameMgr.IsSingleAdventure || GameConfig.IsMultiMode) 
//                        //&& !NativePointManager.Instance.IsCreated(nativePosXZ)
//                        )
//                    {
//                        NativePointInfo nativePointInfo = new NativePointInfo(nativePosXZ, aid);
//                        nativePointInfo.SetPosY(townInfo.Height + 0.5f);
//                        nativePointInfo.townId = townInfo.Id;
//                        NativePointManager.Instance.AddNative(nativePointInfo);
//                        townInfo.TownPosList.Add(nativePosXZ);
//                        //LogManager.Debug(nid + " " + npcPosXZ);
//                    }
//                }
//            }
//        }
//    }



//    public void AddToRenderReady(TownInfo townInfo, IntVector2 worldXZ)
//    { 
//        //1.active by delegate from terrain--before
//        int renderY = townInfo.Height;
//        IntVector3 pos = new Vector3(townInfo.PosCenter.x, renderY, townInfo.PosCenter.y);
//        Debug.Log("Adjust befor:" + pos);
//        LODOctreeMan.self.AttachNodeEvents(null, null, RandomTownManager.Instance.RenderReady, null, null, pos);
//        if (pos.y % 32 < PETools.PEMath.Epsilon)
//        {
//            pos.y -= 1;
//            Debug.Log("Adjust after:" + pos);
//            LODOctreeMan.self.AttachNodeEvents(null, null, RandomTownManager.Instance.RenderReady, null, null, pos);
//            Debug.Log("AddToRenderReady: " + townInfo.Id);
//        }
        
//    }



//    public void RenderReady(IntVector4 nodePosLod)
//    {
//        IntVector2 tileIndexXZ = new IntVector2(nodePosLod.x >> VoxelTerrainConstants._shift, nodePosLod.z >> VoxelTerrainConstants._shift);
//        //if (mRenderedChunk.ContainsKey(tileIndexXZ))
//        //{
//        //    return;
//        //}
//        //mRenderedChunk.Add(tileIndexXZ, 0);
//        Debug.Log("RenderReady TileIndex: " + tileIndexXZ);
//        if (RandomTownManager.Instance.IsTownChunk(tileIndexXZ))
//        {
//            List<IntVector2> townCenterList = GetTownCenterByTile(tileIndexXZ);
//            for (int i = 0; i < townCenterList.Count; i++)
//            {
//                TownInfo ti = RandomTownManager.Instance.GetTownInfoByCenter(townCenterList[i]);
//                if (ti != null)
//                {
//                    if(ti.Type==TownType.NpcTown)
//                    {
//                        //if (!mCreatedTownId.ContainsKey(ti.Id))
//                        //{
//                            Debug.Log("RenderTownAllContent-npcTown: " + ti.Id);
//                            RenderTownAllContent(ti);
//                            if (!mCreatedTownId.ContainsKey(ti.Id)) 
//                            { 
//                                mCreatedTownId.Add(ti.Id, 0);
//                            }
//                            ti.nativeBuildingRendered = true;
//                        //}

//                    }
//                    else
//                    {
//                        Debug.Log("RenderTownAllContent-nativeCamp: " + ti.Id);
//                        RenderTownAllContent(ti);
//                        ti.nativeBuildingRendered = true;
//                    }

//                    if (!ti.IsExplored)
//                    {
//                        AddTown(ti);
//                        ti.IsExplored = true;
//                    }

//                    //test
//                    //if (!ti.IsExplored && ti.level==0)
//                    //{
//                    //    List<TownInfo> townInfoList = levelNpcTown[ti.level];
//                    //    for (int m = 0; m < townInfoList.Count; m++)
//                    //    {
//                    //        AddTown(townInfoList[m]);
//                    //        townInfoList[m].IsExplored = true;
//                    //    }
//                    //    townInfoList = levelNpcTown[ti.level+1];
//                    //    for (int m = 0; m < townInfoList.Count; m++)
//                    //    {
//                    //        AddTown(townInfoList[m]);
//                    //        townInfoList[m].IsExplored = true;
//                    //    }
//                    //    townInfoList = levelNpcTown[ti.level+2];
//                    //    for (int m = 0; m < townInfoList.Count; m++)
//                    //    {
//                    //        AddTown(townInfoList[m]);
//                    //        townInfoList[m].IsExplored = true;
//                    //    }
//                    //    townInfoList = levelNpcTown[ti.level+3];
//                    //    for (int m = 0; m < townInfoList.Count; m++)
//                    //    {
//                    //        AddTown(townInfoList[m]);
//                    //        townInfoList[m].IsExplored = true;
//                    //    }
//                    //    townInfoList = levelNpcTown[ti.level+4];
//                    //    for (int m = 0; m < townInfoList.Count; m++)
//                    //    {
//                    //        AddTown(townInfoList[m]);
//                    //        townInfoList[m].IsExplored = true;
//                    //    }
//                    //}
//                    //}
//                    //else if (!ti.IsExplored && ti.Type == TownType.NativeCamp)
//                    //{
//                    //    AddTown(ti);
//                    //    ti.IsExplored = true;
//                    //}

//                    //test all
//                    //if (!ti.IsExplored)
//                    //{
//                    //    foreach (TownInfo townInfo in townIdInfo.Values) {
//                    //        AddTown(townInfo);
//                    //        townInfo.IsExplored = true;
//                    //    }
//                    //}

//                    if (GameConfig.IsMultiMode)
//                        NetworkManager.SyncServer(EPacketType.PT_Common_TownCreate, ti.Id);
//                }
//            }
//        }
//    }

//    public void RenderTownAllContent(TownInfo townInfo)
//    {
//        if (!townInfo.IsRandomed)
//        {
//            RandomTownInfo(townInfo);
//        }
//        if (townInfo.Type == TownType.NpcTown)
//        {
//            List<BuildingID> buildingIdList = townInfo.BuildingIdList;
//            for (int i = 0; i < buildingIdList.Count;i++ )
//            {
//                BuildingID buildingId = buildingIdList[i];
//                BuildingInfoManager.Instance.RenderBuilding(buildingId);
//            }

//            List<IntVector2> npcs = townInfo.TownPosList;
//            for (int i = 0; i < npcs.Count;i++ )
//            {
//                IntVector2 npcKey = npcs[i];
//                TownNpcManager.Instance.RenderTownNPC(npcKey);
//            }
//        }
//        else { 
//            //to do--nativeCamp
//            //render building, gen native, gen repeatnative
//            List<BuildingID> buildingIdList = townInfo.BuildingIdList;
//            for (int i = 0; i < buildingIdList.Count;i++ )
//            {
//                //if(mCreatedTownId.ContainsKey(townInfo.Id) && i==0 && GameConfig.IsMultiMode){
//                //    continue;
//                //}
//                BuildingID buildingId = buildingIdList[i];
//                BuildingInfoManager.Instance.RenderBuilding(buildingId);
//            }
//            //if(mCreatedTownId.ContainsKey(townInfo.Id)){
//            //    return;
//            //}
//            if (!RandomTownManager.Instance.IsCaptured(townInfo.Id))
//            {
//                if (Pathea.PeGameMgr.IsMultiAdventure || (Pathea.PeGameMgr.IsSingleAdventure && !townInfo.nativeBuildingRendered))
//                {
//                    List<IntVector2> nativePointXZ = townInfo.TownPosList;
//                    for (int i = 0; i < nativePointXZ.Count; i++)
//                    {
//                        IntVector2 npXZ = nativePointXZ[i];
//                        NativePointManager.Instance.RenderNative(npXZ);
//                    }
//                }
//            }
//        }
//    }




//    //public void JustRenderPrefebBuilding(TownInfo townInfo)
//    //{
//    //    List<BuildingID> buildingIdList = townInfo.BuildingIdList;
//    //    for (int i = 1; i < buildingIdList.Count; i++)
//    //    {
//    //        BuildingID buildingId = buildingIdList[i];
//    //        BuildingInfoManager.Instance.RenderBuilding(buildingId);
//    //    }

//    //}

//    public void AddTown(TownInfo ti)
//    {

//        if(!GameConfig.IsMultiMode)
//        {
//            DetectTowns(ti.PosCenter);
//        }

//        if (ti.Type == TownType.NpcTown)
//        {
//            if (GameConfig.IsMultiMode)
//            {
//                //WorldMapManager.Instance.AddTownArea(new Vector3(ti.PosCenter.x, ti.Height, ti.PosCenter.y));
//            }
//            else if (Pathea.PeGameMgr.IsSingleAdventure)
//            {
//                //WorldMapManager.Instance.AddTown(ti.Id, new Vector3(ti.PosCenter.x, ti.Height, ti.PosCenter.y));
//            }
//        }
//        else
//        {
//            //to do-- add nativeCamp mark
//            if (GameConfig.IsMultiMode)
//            {
//                //WorldMapManager.Instance.AddCampArea(new Vector3(ti.PosCenter.x, ti.Height, ti.PosCenter.y));
//            }
//            else if (Pathea.PeGameMgr.IsSingleAdventure)
//            {
//                //WorldMapManager.Instance.AddCamp(new Vector3(ti.PosCenter.x, ti.Height, ti.PosCenter.y));
//            }
//        }
//    }

//    public byte[] Export()
//    {
//        MemoryStream ms = new MemoryStream();
//        BinaryWriter _out = new BinaryWriter(ms);

//        _out.Write(mCreatedTownId.Count);
//        foreach (int ite in mCreatedTownId.Keys)
//        {
//            _out.Write(ite);
//        }
//        _out.Write(capturedCampId.Count);
//        foreach (int ite in capturedCampId.Keys)
//        {
//            _out.Write(ite);
//        }
//        _out.Close();
//        ms.Close();
//        byte[] retval = ms.ToArray();

//        return retval;
//    }

//    public void Import(byte[] buffer){
//        Clear();
//        if (buffer.Length == 0)
//            return;

//        MemoryStream ms = new MemoryStream(buffer);
//        BinaryReader _in = new BinaryReader(ms);
//        int iSize = _in.ReadInt32();
//        for (int i = 0; i < iSize; i++)
//        {
//            int townId= _in.ReadInt32();
//            mCreatedTownId.Add(townId, 0);
//        }
//        iSize = _in.ReadInt32();
//        for (int i = 0; i < iSize; i++)
//        {
//            int campId = _in.ReadInt32();
//            capturedCampId.Add(campId,0);
//        }

//        _in.Close();
//        ms.Close();
//    }

//    public void SetCreatedTown(List<int> townId) {
//        //mCreatedTownId = new Dictionary<int, int>();
//        for (int i = 0; i < townId.Count;i++ ) {
//            if (!mCreatedTownId.ContainsKey(townId[i])) {
//                mCreatedTownId.Add(townId[i],0);
//            }
//        }
//    }
//    public void SetCapturedCamp(List<int> townId)
//    {
//        //mCreatedTownId = new Dictionary<int, int>();
//        for (int i = 0; i < townId.Count; i++)
//        {
//            if (!capturedCampId.ContainsKey(townId[i]))
//            {
//                capturedCampId.Add(townId[i], 0);
//            }
//        }
//    }

//    public List<TownInfo> GetLevelTowns(int level)
//    {
//        if (!levelNpcTown.ContainsKey(level))
//        {
//            return null;
//        }
//        return levelNpcTown[level];
//    }

//    public TownInfo GetTownByID(int id)
//    {
//        if (!townIdInfo.ContainsKey(id))
//        {
//            return null;
//        }
//        return townIdInfo[id];
//    }

//    public void SetCaptured(TownInfo townInfo)
//    {
//        if(!capturedCampId.ContainsKey(townInfo.Id))
//            capturedCampId.Add(townInfo.Id, 0);
//    }

//    public void SetCaptured(int townId)
//    {
//        Debug.Log("SetCaptured");
//        if (Pathea.PeGameMgr.IsSingleAdventure) { 
//            if(!capturedCampId.ContainsKey(townId))
//                capturedCampId.Add(townId, 0);
//        }
//        else if (Pathea.PeGameMgr.IsMultiAdventure)
//        {

//            Debug.Log("RPC_C2S_DestroyNativeTower:" + townId);
//            NetworkManager.SyncServer(EPacketType.PT_Common_NativeTowerDestroyed, townId);
//        }
//    }

//    public bool IsCaptured(int campId)
//    {
//        return capturedCampId.ContainsKey(campId);
//    }

//    public bool IsCreatedTown(int townId)
//    {
//        return mCreatedTownId.ContainsKey(townId);
//    }

//    public void DetectTowns(IntVector2 townCenter)
//    {
////		if (GameUI.Instance.mLimitWorldMapGui.DeleteUnKownMask(townCenter))
////        {
////            Debug.Log("Destroy unKnown pos:" + townCenter);
////        }
//        IntVector2 indexCenter = new IntVector2(townCenter.x >> VoxelTerrainConstants._shift, townCenter.y >> VoxelTerrainConstants._shift);
//        for (int i = indexCenter.x - detectedChunkNum; i < indexCenter.x + detectedChunkNum + 1; i++)
//        {
//            for (int j = indexCenter.y - detectedChunkNum; j < indexCenter.y + detectedChunkNum + 1; j++)
//            {
//                IntVector2 chunkIndex = new IntVector2(i, j);

//                if (indexCenter.Equals(chunkIndex))
//                {
//                    continue;
//                }

//                if (townChunk.ContainsKey(chunkIndex))
//                {
//                    List<IntVector2> townInfoCenterList = townChunk[chunkIndex];
//                    foreach (IntVector2 centerPos in townInfoCenterList)
//                    {
//                        if (!townPosInfo[centerPos].IsExplored && !centerPos.Equals(townCenter))
//                        {
//                            //if (!unknownMask.ContainsKey(centerPos))
//                            //{ 
//                            //    unknownMask.Add(centerPos,GameGui_N.Instance.mLimitWorldMapGui.AddUnKownMask(new Vector3(centerPos.x, 60, centerPos.y)));
//                            //    Debug.Log("UnKnown pos:" + centerPos.x + " " + centerPos.y);
//                            //}
//                            //GameUI.Instance.mLimitWorldMapGui.AddUnKownMask(centerPos);
//                            //Debug.Log("UnKnown pos:" + centerPos);
//                        }
//                        else
//                        {
//                            //if (GameUI.Instance.mLimitWorldMapGui.DeleteUnKownMask(centerPos))
//                            //{
//                                //Debug.Log("Destroy unKnown pos:" + centerPos);
//                            //}
//                        }
//                    }
//                }

//            }
//        }
//    }

//    public void SetTownDistance(float scale){
//        townDistanceX = (int)(randomTownInfo.distanceX * scale);
//        townDistanceZ = (int)(randomTownInfo.distanceZ * scale);
//    }

//    public void PrintTownPos()
//    {
//        List<TownInfo> allTown = new List<TownInfo>();
//        allTown.AddRange(townPosInfo.Values);
//        for(int i=0;i<allTown.Count;i++)
//        {
//            TownInfo t = allTown[i];
//            Debug.Log(t.Type.ToString() + " center:" + t.PosCenter + " id:" + t.Id + " tid:" + t.Tid + " cid:" + t.Cid);
//        }
//    }

//    static void ClearRandomTownSystem()
//    {
//        Instance.Clear();
//        BuildingInfoManager.Instance.Clear();
//        TownNpcManager.Instance.Clear();
//    }

//    #region Action Callback APIs
//    public static void RPC_S2C_RandomTownData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
//    {
//        int[] _createdTownID;
//        int[] _capturedCampID;
//        stream.TryRead<int>(out RandomMapConfig.RandSeed);
//        stream.TryRead<int[]>(out _createdTownID);
//        stream.TryRead<int[]>(out _capturedCampID);

//        ClearRandomTownSystem();
//        Instance.SetCreatedTown(_createdTownID.ToList());
//        Instance.SetCapturedCamp(_capturedCampID.ToList());
//    }

//    public static void RPC_S2C_NativeTowerDestroyed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
//    {
//        int campId;
//        stream.TryRead<int>(out campId);
//        if (!Instance.capturedCampId.ContainsKey(campId))
//            Instance.capturedCampId.Add(campId, 0);
//    }
//    #endregion
//}

