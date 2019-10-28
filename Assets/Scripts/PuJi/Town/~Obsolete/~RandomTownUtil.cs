//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using RandomTownXML;
//using NativeCampXML;


//namespace TownData
//{
//    public class RandomTownUtil
//    {
//        public static int spawnRadius0 = 0;
//        public static int spawnRadius = 0;
//        public const int boundaryShadeWidth = 8;
//        public const int boundaryShadeLength = 8;
//        public const int ladderLength = 12;
//        public const int ladderWidth = 8;

//        static RandomTownUtil instance;
//        public static RandomTownUtil Instance
//        {
//            get
//            {
//                if (null == instance)
//                {
//                    instance = new RandomTownUtil();
//                }
//                return instance;
//            }
//        }
//        public IntVector3 CeilToIntVector3(Vector3 pos)
//        {
//            return new IntVector3(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
//        }

//        public Dictionary<IntVector3, B45Block> AdjustBuildingRotation(Dictionary<IntVector3, B45Block> rebuild, int rot)
//        {
//            return null;
//        }

//        public void Shuffle(List<int> id, System.Random myRand)
//        {
//            int size = id.Count;
//            List<int> temp = new List<int>();
//            for (int i = 0; i < size; i++)
//            {
//                temp.Add(id[i]);
//            }

//            int index = 0;
//            while (temp.Count > 0)
//            {
//                int i = myRand.Next(temp.Count);
//                id[index] = temp[i];
//                index++;
//                temp.RemoveAt(i);
//            }
//        }

//        //to judge whether a posXZ is in a town
//        public IntVector2 IsInTown(IntVector2 posXZ)
//        {
//            int chunkX = posXZ.x >> VoxelTerrainConstants._shift;
//            int chunkZ = posXZ.y >> VoxelTerrainConstants._shift;
//            IntVector2 chunkPos = new IntVector2(chunkX, chunkZ);

//            //if (!RandomTownManager.Instance.TownChunk.ContainsKey(chunkPos))
//            //{
//            //    return null;
//            //}
//            //IntVector2 townCenter = RandomTownManager.Instance.TownChunk[chunkPos];

//            if (!RandomTownManager.Instance.IsTownChunk(chunkPos))
//            {
//                return null;
//            }
//            IntVector2 townCenter = RandomTownManager.Instance.GetTownCenterByTileAndPos(chunkPos, posXZ);
//            return townCenter;
//        }

//        //to judge whether a posXZ is in a town
//        public IntVector2 IsInTown(Vector3 pos)
//        {
//            IntVector2 posXZ = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
//            return IsInTown(posXZ);
//        }

//        public List<int> RandomChoose(int num, int minValue, int maxValue, System.Random randomSeed)
//        {
//            List<int> pool = new List<int>();
//            for (int i = minValue; i < maxValue + 1; i++)
//            {
//                pool.Add(i);
//            }

//            List<int> rezult = new List<int>();
//            for (int i = 0; i < num; i++)
//            {
//                int index = randomSeed.Next(maxValue - minValue + 1 - i);
//                rezult.Add(pool[index]);
//                pool.RemoveAt(index);
//            }
//            return rezult;
//        }

//        public IntVector2 GenCampByZone(IntVector2 center, int zoneNo, int distanceMin, int distanceMax, System.Random randomSeed)
//        {
//            IntVector2 result = new IntVector2(center.x, center.y);
//            switch (zoneNo)
//            {
//                case 0:
//                    result.x += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
//                    result.y += randomSeed.Next(distanceMin, distanceMax);
//                    break;
//                case 1:
//                    result.x += randomSeed.Next(distanceMin, distanceMax);
//                    result.y += randomSeed.Next(distanceMin, distanceMax);
//                    break;
//                case 2:
//                    result.x += randomSeed.Next(distanceMin, distanceMax);
//                    result.y += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
//                    break;
//                case 3:
//                    result.x += randomSeed.Next(distanceMin, distanceMax);
//                    result.y -= randomSeed.Next(distanceMin, distanceMax);
//                    break;
//                case 4:
//                    result.x += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
//                    result.y -= randomSeed.Next(distanceMin, distanceMax);
//                    break;
//                case 5:
//                    result.x -= randomSeed.Next(distanceMin, distanceMax);
//                    result.y -= randomSeed.Next(distanceMin, distanceMax);
//                    break;
//                case 6:
//                    result.x -= randomSeed.Next(distanceMin, distanceMax);
//                    result.y += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
//                    break;
//                case 7:
//                    result.x -= randomSeed.Next(distanceMin, distanceMax);
//                    result.y += randomSeed.Next(distanceMin, distanceMax);
//                    break;
//                default:
//                    result.x += randomSeed.Next(distanceMin, distanceMax);
//                    result.y += randomSeed.Next(distanceMin, distanceMax);
//                    break;
//            }
//            return result;
//        }

//        public IntVector2 GetSpawnPos()
//        {
//            System.Random seed = new System.Random(System.DateTime.Now.Millisecond);
//            if (Pathea.PeGameMgr.IsMultiAdventure)
//            {
//                if (Pathea.PeGameMgr.IsMultiCoop)
//                {
//                    if (RandomMapConfig.mapSize == 0 || RandomMapConfig.mapSize == 1)
//                    {
//                        List<TownInfo> townList = null;
//                        int level = 0;
//                        while (townList == null)
//                        {
//                            townList = RandomTownManager.Instance.GetLevelTowns(level++);
//                        }
//                        //List<TownInfo> townList = new List<TownInfo>();
//                        //for (int i = 0; i < townsLevel0.Count; i++)
//                        //{
//                        //    if (Mathf.Abs(townsLevel0[i].PosStart.x) <= 1600 && Mathf.Abs(townsLevel0[i].PosStart.y) <= 1600)
//                        //    {
//                        //        townList.Add(townsLevel0[i]);
//                        //    }
//                        //}
//                        if (townList == null)
//                        {
//                            LogManager.Error("RandomTown not Initialized! ");
//                            return null;
//                        }

//                        //Debug.LogError("spawn count: " + townList.Count);
//                        int count = seed.Next(townList.Count);
//                        return townList[count].PosCenter;
//                    }
//                    else
//                    {
//                        List<TownInfo> allTownList = new List<TownInfo>();
//                        allTownList.AddRange(RandomTownManager.Instance.TownPosInfo.Values);
//                        List<TownInfo> townList = new List<TownInfo>();
//                        for (int i = 0; i < allTownList.Count; i++)
//                        {
//                            if (allTownList[i].Type == TownType.NpcTown && Mathf.Abs(allTownList[i].PosStart.x) <= 5000 && Mathf.Abs(allTownList[i].PosStart.y) <= 5000)
//                            {
//                                townList.Add(allTownList[i]);
//                            }
//                        }
//                        if (townList == null)
//                        {
//                            LogManager.Error("RandomTown not Initialized! ");
//                            return null;
//                        }

//                        //Debug.LogError("spawn count: " + townList.Count);
//                        int count = seed.Next(townList.Count);
//                        return townList[count].PosCenter;
//                    }

//                }
//                else if (Pathea.PeGameMgr.IsMultiVS)
//                {
//                    if (RandomMapConfig.mapSize == 0 || RandomMapConfig.mapSize == 1)
//                    {
//                        //decide town

//                        //to do while level0 no town
//                        List<TownInfo> townList = null;
//                        int level = 0;
//                        while (townList == null)
//                        {
//                            townList = RandomTownManager.Instance.GetLevelTowns(level++);
//                        }
//                        Debug.LogError("StartTownLevel:" + (level - 1));
//                        int townCount = townList.Count;
//                        List<int> indexGroup = new List<int>();
//                        for (int i = 0; i < townCount; i++)
//                        {
//                            indexGroup.Add(i);
//                        }
//                        Shuffle(indexGroup, new System.Random(RandomMapConfig.RandSeed));
//                        int townIndex = indexGroup[BaseNetwork.MainPlayer.TeamId % townCount];
//                        TownInfo townInfo = townList[townIndex];
//                        IntVector2 townCenter = townInfo.PosCenter;
//                        IntVector2 spawnPos = new IntVector2(townCenter.x + seed.Next(-spawnRadius, spawnRadius), townCenter.y + seed.Next(-spawnRadius, spawnRadius));
//                        //if (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //{
//                        //    int turn = 0;
//                        //    int xPlus = -5;
//                        //    int zPlus = -5;
//                        //    if (spawnPos.x < 0)
//                        //    {
//                        //        xPlus = 5;
//                        //    }
//                        //    if (spawnPos.y < 0)
//                        //    {
//                        //        zPlus = 5;
//                        //    }
//                        //    while (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //    {
//                        //        if (turn % 2 == 0)
//                        //        {
//                        //            spawnPos.x += xPlus;
//                        //        }
//                        //        else
//                        //        {
//                        //            spawnPos.y += zPlus;
//                        //        }
//                        //        turn++;
//                        //    }
//                        //}
//                        Debug.Log("TeamNum: " + BaseNetwork.MainPlayer.TeamId);
//                        //                        if (!townInfo.IsExplored)
//                        //                            GameUI.Instance.mLimitWorldMapGui.AddUnKownMask(townCenter);
//                        return spawnPos;
//                    }
//                    else
//                    {
//                        List<TownInfo> allTownList = new List<TownInfo>();
//                        allTownList.AddRange(RandomTownManager.Instance.TownPosInfo.Values);
//                        List<TownInfo> townList = new List<TownInfo>();
//                        for (int i = 0; i < allTownList.Count; i++)
//                        {
//                            if (allTownList[i].Type == TownType.NpcTown && Mathf.Abs(allTownList[i].PosStart.x) <= 5000 && Mathf.Abs(allTownList[i].PosStart.y) <= 5000)
//                            {
//                                townList.Add(allTownList[i]);
//                            }
//                        }
//                        int townCount = townList.Count;
//                        List<int> indexGroup = new List<int>();
//                        for (int i = 0; i < townCount; i++)
//                        {
//                            indexGroup.Add(i);
//                        }
//                        Shuffle(indexGroup, new System.Random(RandomMapConfig.RandSeed));
//                        int townIndex = indexGroup[BaseNetwork.MainPlayer.TeamId % townCount];
//                        TownInfo townInfo = townList[townIndex];
//                        IntVector2 townCenter = townInfo.PosCenter;
//                        IntVector2 spawnPos = new IntVector2(townCenter.x + seed.Next(-spawnRadius, spawnRadius), townCenter.y + seed.Next(-spawnRadius, spawnRadius));
//                        //if (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //{
//                        //    int turn = 0;
//                        //    int xPlus = -5;
//                        //    int zPlus = -5;
//                        //    if (spawnPos.x < 0)
//                        //    {
//                        //        xPlus = 5;
//                        //    }
//                        //    if (spawnPos.y < 0)
//                        //    {
//                        //        zPlus = 5;
//                        //    }
//                        //    while (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //    {
//                        //        if (turn % 2 == 0)
//                        //        {
//                        //            spawnPos.x += xPlus;
//                        //        }
//                        //        else
//                        //        {
//                        //            spawnPos.y += zPlus;
//                        //        }
//                        //        turn++;
//                        //    }
//                        //}
//                        Debug.Log("TeamNum: " + BaseNetwork.MainPlayer.TeamId);
//                        //                        if (!townInfo.IsExplored)
//                        //                            GameUI.Instance.mLimitWorldMapGui.AddUnKownMask(townCenter);
//                        return spawnPos;
//                    }

//                }
//                else //GameConfig.IsMultiSurvive
//                {
//                    if (RandomMapConfig.mapSize == 0 || RandomMapConfig.mapSize == 1)
//                    {
//                        //List<TownInfo> townsLevel0 = RandomTownManager.Instance.GetLevelTowns(0);
//                        //List<TownInfo> townList = new List<TownInfo>();

//                        List<TownInfo> townList = null;
//                        int level = 0;
//                        while (townList == null)
//                        {
//                            townList = RandomTownManager.Instance.GetLevelTowns(level++);
//                        }

//                        //for (int i = 0; i < townsLevel0.Count; i++)
//                        //{
//                        //    if (Mathf.Abs(townsLevel0[i].PosStart.x) <= 1600 && Mathf.Abs(townsLevel0[i].PosStart.y) <= 1600)
//                        //    {
//                        //        townList.Add(townsLevel0[i]);
//                        //    }
//                        //}
//                        if (townList == null)
//                        {
//                            LogManager.Error("RandomTown not Initialized! ");
//                            return null;
//                        }
//                        int count = seed.Next(townList.Count);
//                        TownInfo townInfo = townList[count];
//                        IntVector2 townCenter = townInfo.PosCenter;
//                        IntVector2 spawnPos = new IntVector2(townCenter.x + seed.Next(-spawnRadius, spawnRadius), townCenter.y + seed.Next(-spawnRadius, spawnRadius));
//                        //if (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //{
//                        //    int turn = 0;
//                        //    int xPlus = -5;
//                        //    int zPlus = -5;
//                        //    if (spawnPos.x < 0)
//                        //    {
//                        //        xPlus = 5;
//                        //    }
//                        //    if (spawnPos.y < 0)
//                        //    {
//                        //        zPlus = 5;
//                        //    }
//                        //    while (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //    {
//                        //        if (turn % 2 == 0)
//                        //        {
//                        //            spawnPos.x += xPlus;
//                        //        }
//                        //        else
//                        //        {
//                        //            spawnPos.y += zPlus;
//                        //        }
//                        //        turn++;
//                        //    }
//                        //}
//                        //                        if (!townInfo.IsExplored)
//                        //                            GameUI.Instance.mLimitWorldMapGui.AddUnKownMask(townCenter);
//                        return spawnPos;
//                    }
//                    else
//                    {
//                        List<TownInfo> allTownList = new List<TownInfo>();
//                        allTownList.AddRange(RandomTownManager.Instance.TownPosInfo.Values);
//                        List<TownInfo> townList = new List<TownInfo>();
//                        for (int i = 0; i < allTownList.Count; i++)
//                        {
//                            if (allTownList[i].Type == TownType.NpcTown && Mathf.Abs(allTownList[i].PosStart.x) <= 5000 && Mathf.Abs(allTownList[i].PosStart.y) <= 5000)
//                            {
//                                townList.Add(allTownList[i]);
//                            }
//                        }
//                        if (townList.Count == 0)
//                        {
//                            LogManager.Error("RandomTown not Initialized! ");
//                            return null;
//                        }
//                        int count = seed.Next(townList.Count);
//                        TownInfo townInfo = townList[count];
//                        IntVector2 townCenter = townInfo.PosCenter;
//                        IntVector2 spawnPos = new IntVector2(townCenter.x + seed.Next(-spawnRadius, spawnRadius), townCenter.y + seed.Next(-spawnRadius, spawnRadius));
//                        //if (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //{
//                        //    int turn = 0;
//                        //    int xPlus = -5;
//                        //    int zPlus = -5;
//                        //    if (spawnPos.x < 0)
//                        //    {
//                        //        xPlus = 5;
//                        //    }
//                        //    if (spawnPos.y < 0)
//                        //    {
//                        //        zPlus = 5;
//                        //    }
//                        //    while (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                        //    {
//                        //        if (turn % 2 == 0)
//                        //        {
//                        //            spawnPos.x += xPlus;
//                        //        }
//                        //        else
//                        //        {
//                        //            spawnPos.y += zPlus;
//                        //        }
//                        //        turn++;
//                        //    }
//                        //}
//                        //                        if (!townInfo.IsExplored)
//                        //                            GameUI.Instance.mLimitWorldMapGui.AddUnKownMask(townCenter);
//                        return spawnPos;
//                    }
//                }
//            }
//            else
//            {
//                int x = seed.Next(1000) - 500;
//                int z = seed.Next(1000) - 500;
//                IntVector2 spawnPos = new IntVector2(x, z);
//                if (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                {
//                    int turn = 0;
//                    int xPlus = -5;
//                    int zPlus = -5;
//                    if (spawnPos.x < 0)
//                    {
//                        xPlus = 5;
//                    }
//                    if (spawnPos.y < 0)
//                    {
//                        zPlus = 5;
//                    }
//                    while (!VFDataRTGen.IsSpawnAvailable(spawnPos))
//                    {
//                        if (turn % 2 == 0)
//                        {
//                            spawnPos.x += xPlus;
//                        }
//                        else
//                        {
//                            spawnPos.y += zPlus;
//                        }
//                        turn++;
//                    }
//                }
//                return spawnPos;
//            }
//        }

//        public DynamicNativePoint GetDynamicNativePoint(TownInfo townInfo)
//        {
//            DynamicNativePoint result;

//            DynamicNative[] dns = townInfo.nativeTower.dynamicNatives;
//            System.Random randSeed = new System.Random(System.DateTime.Now.Millisecond);
//            DynamicNative dn = dns[randSeed.Next(dns.Count())];
//            result.id = dn.did;
//            result.type = dn.type;
//            if (dn.type == 1)
//            {
//                result.point = GetDynamicNativeSinglePoint(townInfo, randSeed);
//            }
//            else
//            {
//                result.point = GetDynamicNativeGroupPoint(townInfo, randSeed);
//            }

//            return result;
//        }

//        public DynamicNativePoint GetDynamicNativePoint(int townId)
//        {
//            TownInfo townInfo = RandomTownManager.Instance.GetTownByID(townId);
//            DynamicNativePoint result;

//            DynamicNative[] dns = townInfo.nativeTower.dynamicNatives;
//            System.Random randSeed = new System.Random(System.DateTime.Now.Millisecond);
//            DynamicNative dn = dns[randSeed.Next(dns.Count())];
//            result.id = dn.did;
//            result.type = dn.type;
//            if (dn.type == 1)
//            {
//                result.point = GetDynamicNativeSinglePoint(townInfo, randSeed);
//            }
//            else
//            {
//                result.point = GetDynamicNativeGroupPoint(townInfo, randSeed);
//            }

//            return result;
//        }

//        public Vector3 GetDynamicNativeSinglePoint(TownInfo townInfo, System.Random randSeed)
//        {
//            IntVector2 townPosCenter = townInfo.PosCenter;
//            IntVector2 townPosStart = townInfo.PosStart;
//            List<IntVector2> streetCellList = townInfo.StreetCellList;
//            int cellSizeX = townInfo.CellSizeX;
//            int cellSizeZ = townInfo.CellSizeZ;
//            IntVector2 nativePosXZ;

//            Vector3 SinglePoint = new Vector3();
//            IntVector2 CellXZ = streetCellList[randSeed.Next(streetCellList.Count)];
//            IntVector2 startPosXZ = townPosStart + new IntVector2(CellXZ.x * cellSizeX, CellXZ.y * cellSizeZ);
//            nativePosXZ = startPosXZ + new IntVector2(randSeed.Next(cellSizeX), randSeed.Next(cellSizeZ));
//            SinglePoint = new Vector3(nativePosXZ.x, townInfo.Height + 0.5f, nativePosXZ.y);

//            return SinglePoint;
//        }

//        public Vector3 GetDynamicNativeGroupPoint(TownInfo townInfo, System.Random randSeed)
//        {
//            IntVector2 townPosCenter = townInfo.PosCenter;
//            IntVector2 townPosStart = townInfo.PosStart;
//            List<IntVector2> streetCellList = townInfo.StreetCellList;
//            int cellSizeX = townInfo.CellSizeX;
//            int cellSizeZ = townInfo.CellSizeZ;
//            int groupMax = streetCellList.Count;
//            Vector3 GroupPoint = new Vector3();
//            IntVector2 nativePosXZ;

//            IntVector2 CellXZ = streetCellList[randSeed.Next(groupMax)];
//            IntVector2 startPosXZ = townPosStart + new IntVector2(CellXZ.x * cellSizeX, CellXZ.y * cellSizeZ);
//            nativePosXZ = startPosXZ + new IntVector2(cellSizeX / 2, cellSizeZ / 2);
//            GroupPoint = new Vector3(nativePosXZ.x, townInfo.Height + 0.5f, nativePosXZ.y);

//            return GroupPoint;
//        }


//        //public void GenNativeTowerBonus(int id, float timeout)
//        //{
//        //    if (Pathea.PeGameMgr.IsMultiAdventure)
//        //    {
//        //        PlayerFactory.mMainPlayer.RPC("RPC_C2S_CreateMapObj", id, timeout);
//        //    }
//        //}


//        public float GetBoundaryShade(IntVector2 worldXZ, TownInfo ti, out IntVector2 boundaryXZ)
//        {
//            int startX = ti.PosStart.x;
//            int startZ = ti.PosStart.y;
//            int endX = ti.PosEnd.x;
//            int endZ = ti.PosEnd.y;
//            int x = worldXZ.x;
//            int z = worldXZ.y;

//            if (x <= (startX + endX) / 2.0f + boundaryShadeWidth / 2.0f
//                && x >= (startX + endX) / 2.0f - boundaryShadeWidth / 2.0f
//                && z < startZ + boundaryShadeLength
//                && z >= startZ)
//            {
//                //south
//                boundaryXZ = new IntVector2(Mathf.RoundToInt((startX + endX) / 2.0f), startZ);
//                return (boundaryShadeLength - (z - startZ)) * 1.0f / boundaryShadeLength;
//            }
//            else if (x < startX + boundaryShadeLength
//               && x >= startX
//               && z <= (startZ + endZ) / 2.0f + boundaryShadeWidth / 2.0f
//               && z >= (startZ + endZ) / 2.0f - boundaryShadeWidth / 2.0f)
//            {
//                //west
//                boundaryXZ = new IntVector2(startX, Mathf.RoundToInt((startZ + endZ) / 2.0f));
//                return (boundaryShadeLength - (x - startX)) * 1.0f / boundaryShadeLength;
//            }
//            else if (x <= (startX + endX) / 2.0f + boundaryShadeWidth / 2.0f
//               && x >= (startX + endX) / 2.0f - boundaryShadeWidth / 2.0f
//                && z <= endZ
//                && z > endZ - boundaryShadeLength)
//            {
//                //north
//                boundaryXZ = new IntVector2(Mathf.RoundToInt((startX + endX) / 2.0f), endZ);
//                return (boundaryShadeLength - (endZ - z)) * 1.0f / boundaryShadeLength;
//            }
//            else if (x <= endX
//               && x > endX - boundaryShadeLength
//               && z <= (startZ + endZ) / 2.0f + boundaryShadeWidth / 2.0f
//               && z >= (startZ + endZ) / 2.0f - boundaryShadeWidth / 2.0f)
//            {
//                //east
//                boundaryXZ = new IntVector2(endX, Mathf.RoundToInt((startZ + endZ) / 2.0f));
//                return (boundaryShadeLength - (endX - x)) * 1.0f / boundaryShadeLength;
//            }
//            else
//            {
//                boundaryXZ = ti.PosCenter;
//                return 0;
//            }
//        }

//        public bool IsRandomedLadderCell(TownInfo ti, IntVector2 worldXZ, out IntVector2 boundaryXZ, out int rot)
//        {
//            bool isLadderCell = false;
//            boundaryXZ = new IntVector2();
//            rot = -1;
//            System.Random randLadder = new System.Random(ti.Height);
//            List<Ladder> selectLadders = new List<Ladder>();
//            int ladderCount = ti.ladderInfo.Count();
//            if (ladderCount < ti.ladderSelectNum)
//            {
//                Debug.LogError("xmlError ladder:tid=" + ti.Tid + " cid=" + ti.Cid);
//                ti.ladderSelectNum = ladderCount;
//            }
//            List<int> ladderIndex = new List<int>();
//            for (int i = 0; i < ladderCount; i++)
//            {
//                ladderIndex.Add(i);
//            }

//            Shuffle(ladderIndex, randLadder);
//            for (int i = 0; i < ti.ladderSelectNum; i++)
//            {
//                selectLadders.Add(ti.ladderInfo[ladderIndex[i]]);
//            }

//            for (int i = 0; i < selectLadders.Count; i++)
//            {
//                if (IsLadderCell(ti, selectLadders[i], worldXZ, out boundaryXZ))
//                {
//                    rot = selectLadders[i].rot;
//                    isLadderCell = true;
//                    break;
//                }
//            }
//            return isLadderCell;
//        }

//        public bool IsLadderCell(TownInfo ti, Ladder ladderInfo, IntVector2 point, out IntVector2 boundaryXZ)
//        {
//            boundaryXZ = new IntVector2();
//            IntVector2 ladderInfoStart = new IntVector2(ti.PosStart.x + ladderInfo.x * ti.CellSizeX, ti.PosStart.y + ladderInfo.z * ti.CellSizeZ);
//            IntVector2 ladderInfoEnd = new IntVector2(ladderInfoStart.x + ti.CellSizeX, ladderInfoStart.y + ti.CellSizeZ);
//            if (!(point.x >= ladderInfoStart.x && point.x <= ladderInfoEnd.x && point.y >= ladderInfoStart.y && point.y <= ladderInfoEnd.y))
//            {
//                return false;
//            }
//            switch (ladderInfo.rot)
//            {
//                case 0:
//                    boundaryXZ.x = ladderInfoStart.x + ti.CellSizeX / 2;
//                    boundaryXZ.y = ladderInfoStart.y - 1;
//                    break;
//                case 1:
//                    boundaryXZ.x = ladderInfoStart.x - 1;
//                    boundaryXZ.y = ladderInfoStart.y + ti.CellSizeZ / 2;
//                    break;
//                case 2:
//                    boundaryXZ.x = ladderInfoStart.x + ti.CellSizeX / 2;
//                    boundaryXZ.y = ladderInfoEnd.y + 1;
//                    break;
//                case 3:
//                    boundaryXZ.x = ladderInfoEnd.x + 1;
//                    boundaryXZ.y = ladderInfoStart.y + ti.CellSizeZ / 2;
//                    break;
//                default:
//                    boundaryXZ = null;
//                    break;
//            }
//            if (boundaryXZ == null)
//            {
//                LogManager.Error("ladder xml Error: tid= " + ti.Tid);
//                return false;
//            }
//            return true;
//        }

//        public bool IsLadderPoint(TownInfo ti, Ladder ladderInfo, IntVector2 point, out IntVector2 boundaryXZ)
//        {
//            boundaryXZ = new IntVector2();
//            IntVector2 ladderInfoStart = new IntVector2(ti.PosStart.x + ladderInfo.x * ti.CellSizeX, ti.PosStart.y + ladderInfo.z * ti.CellSizeZ);
//            IntVector2 ladderInfoEnd = new IntVector2(ladderInfoStart.x + ti.CellSizeX, ladderInfoStart.y + ti.CellSizeZ);
//            if (!(point.x >= ladderInfoStart.x && point.x <= ladderInfoEnd.x && point.y >= ladderInfoStart.y && point.y <= ladderInfoEnd.y))
//            {
//                return false;
//            }

//            IntVector2 ladderAreaStart = new IntVector2();
//            IntVector2 ladderAreaEnd = new IntVector2();
//            switch (ladderInfo.rot)
//            {
//                case 0: ladderAreaStart.x = ladderInfoStart.x + (ti.CellSizeX - ladderLength) / 2;
//                    ladderAreaStart.y = ladderInfoStart.y;
//                    ladderAreaEnd.x = ladderAreaStart.x + ladderLength;
//                    ladderAreaEnd.y = ladderAreaStart.y + ladderWidth;
//                    boundaryXZ.x = ladderAreaStart.x + ladderLength / 2;
//                    boundaryXZ.y = ladderInfoStart.y - 1;
//                    break;
//                case 1:
//                    ladderAreaStart.x = ladderInfoStart.x;
//                    ladderAreaStart.y = ladderInfoStart.y + (ti.CellSizeZ - ladderLength) / 2;
//                    ladderAreaEnd.x = ladderAreaStart.x + ladderWidth;
//                    ladderAreaEnd.y = ladderAreaStart.y + ladderLength;
//                    boundaryXZ.x = ladderInfoStart.x - 1;
//                    boundaryXZ.y = ladderAreaStart.y + ladderLength / 2;
//                    break;
//                case 2:
//                    ladderAreaStart.x = ladderInfoStart.x + (ti.CellSizeX - ladderLength) / 2;
//                    ladderAreaStart.y = ladderInfoEnd.y - ladderWidth;
//                    ladderAreaEnd.x = ladderAreaStart.x + ladderLength;
//                    ladderAreaEnd.y = ladderInfoEnd.y;
//                    boundaryXZ.x = ladderAreaStart.x + ladderLength / 2;
//                    boundaryXZ.y = ladderInfoEnd.y + 1;
//                    break;
//                case 3:
//                    ladderAreaStart.x = ladderInfoEnd.x - ladderWidth;
//                    ladderAreaStart.y = ladderInfoStart.y + (ti.CellSizeZ - ladderLength) / 2;
//                    ladderAreaEnd.x = ladderAreaStart.x + ladderWidth;
//                    ladderAreaEnd.y = ladderAreaStart.y + ladderLength;
//                    boundaryXZ.x = ladderInfoEnd.x + 1;
//                    boundaryXZ.y = ladderAreaStart.y + ladderLength / 2;
//                    break;
//                default:
//                    ladderAreaStart = null;
//                    break;
//            }
//            if (ladderAreaStart == null)
//            {
//                LogManager.Error("ladder xml Error: tid= " + ti.Tid + " rot=" + ladderInfo.rot);
//                return false;
//            }
//            if (!(point.x >= ladderAreaStart.x && point.x <= ladderAreaEnd.x && point.y >= ladderAreaStart.y && point.y <= ladderAreaEnd.y))
//            {
//                return false;
//            }
//            else
//            {
//                return true;
//            }
//        }

//        public float ComputeLadderScale(int rot, IntVector2 worldXZ, IntVector2 boundaryXZ, int heightDiff, int cellsizex, int cellsizez)
//        {
//            //1.compute width
//            int dynamicLadderWidth = heightDiff;
//            switch (rot)
//            {
//                case 0:
//                case 2:
//                    if (dynamicLadderWidth > cellsizez)
//                        dynamicLadderWidth = cellsizez;
//                    break;
//                case 1:
//                case 3:
//                    if (dynamicLadderWidth > cellsizex)
//                        dynamicLadderWidth = cellsizex;
//                    break;
//                default: LogManager.Error("ComputeLadderScale error : rot=" + rot);
//                    return 0;
//            }

//            //2.isLadder
//            switch (rot)
//            {
//                case 0:
//                    if (worldXZ.x > boundaryXZ.x + ladderLength * 1.0f / 2 || worldXZ.x < boundaryXZ.x - ladderLength * 1.0f / 2)
//                        return 1;
//                    if (worldXZ.y > boundaryXZ.y + dynamicLadderWidth)
//                        return 1;
//                    break;
//                case 1:
//                    if (worldXZ.y > boundaryXZ.y + ladderLength * 1.0f / 2 || worldXZ.y < boundaryXZ.y - ladderLength * 1.0f / 2)
//                        return 1;
//                    if (worldXZ.x > boundaryXZ.x + dynamicLadderWidth)
//                        return 1;
//                    break;
//                case 2:
//                    if (worldXZ.x > boundaryXZ.x + ladderLength * 1.0f / 2 || worldXZ.x < boundaryXZ.x - ladderLength * 1.0f / 2)
//                        return 1;
//                    if (worldXZ.y < boundaryXZ.y - dynamicLadderWidth)
//                        return 1;
//                    break;
//                case 3:
//                    if (worldXZ.y > boundaryXZ.y + ladderLength * 1.0f / 2 || worldXZ.y < boundaryXZ.y - ladderLength * 1.0f / 2)
//                        return 1;
//                    if (worldXZ.x < boundaryXZ.x - dynamicLadderWidth)
//                        return 1;
//                    break;
//                default:
//                    break;
//            }


//            switch (rot)
//            {
//                case 0:
//                    return (worldXZ.y - boundaryXZ.y - 1) * 1.0f / dynamicLadderWidth;
//                case 1:
//                    return (worldXZ.x - boundaryXZ.x - 1) * 1.0f / dynamicLadderWidth;
//                case 2:
//                    return (boundaryXZ.y - worldXZ.y - 1) * 1.0f / dynamicLadderWidth;
//                case 3:
//                    return (boundaryXZ.x - worldXZ.x - 1) * 1.0f / dynamicLadderWidth;
//                default: return 0;
//            }
//        }


//    }

//    public class WeightPool
//    {
//        public int maxValue = 0;
//        List<int> weightTree = new List<int>();
//        List<int> idTree = new List<int>();
//        public int count = 0;

//        public void Add(int weight, int id)
//        {
//            maxValue += weight;
//            weightTree.Add(maxValue);
//            idTree.Add(id);
//            count++;
//        }

//        public void Clear()
//        {
//            maxValue = 0;
//            weightTree = new List<int>();
//            idTree = new List<int>();
//            count = 0;
//        }

//        public int GetRandID(System.Random randSeed)
//        {
//            if (maxValue == 0)
//            {
//                return -1;
//            }
//            int value = randSeed.Next(maxValue);
//            for (int i = 0; i < count; i++)
//            {
//                if (value < weightTree[i])
//                {
//                    return idTree[i];
//                }
//            }
//            return -1;
//        }

//    }
//}
//public struct DynamicNativePoint
//{
//    public Vector3 point;
//    public int id;
//    public int type;//0 group,1 single
//}