//using UnityEngine;

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Xml;
//using System.Xml.Serialization;
//using NativeCampXML;
//using TownData;

//public class NativeCampManager
//{
//    static VArtifactCampManager mInstance;
//    public static VArtifactCampManager Instance {
//        get {
//            if (mInstance == null) 
//            {
//                mInstance = new VArtifactCampManager();
//            }
//            return mInstance;
//        }
//    }

//    public NativeCampDesc randomCampInfo;
//    public int templateCount;

//    Dictionary<int, WeightPool> LevelPool;// classify the camp through level;

//    public const int levelCount = 5;
//    private int numMin;
//    private int numMax;
//    private int distanceMin;
//    private int distanceMax;
//    private const int zoneMin=0;
//    private const int zoneMax=7;
//    //init NativeCamp xml data, and tidy it

//    public VArtifactCampManager()
//    {
//        LoadXml();
//    }
//    public void LoadXml()
//    {
//        string xmlPath = "RandomTown/NativeCamp";
//        if (!GameConfig.IsMultiMode) {
//            xmlPath = "RandomTown/NativeCampSingleMode";
//        }

//        TextAsset xmlResource = Resources.Load(xmlPath, typeof(TextAsset)) as TextAsset;
//        StringReader reader = new StringReader(xmlResource.text);
//        if (null == reader)
//            return;

//        XmlSerializer serializer = new XmlSerializer(typeof(NativeCampDesc));
//        randomCampInfo = (NativeCampDesc)serializer.Deserialize(reader);
//        reader.Close();

//        templateCount = randomCampInfo.nativeCamps.Count();
//        numMin = randomCampInfo.numMin;
//        numMax = randomCampInfo.numMax;
//        distanceMin = randomCampInfo.distanceMin;
//        distanceMax = randomCampInfo.distanceMax;
//    }

//    public void Init()
//    {
//        LevelPool = new Dictionary<int, WeightPool>();
//        for (int i = 0; i < randomCampInfo.nativeCamps.Count(); i++)
//        {
//            NativeCamp nc = randomCampInfo.nativeCamps[i];
//            if (nc.level >= levelCount)
//            {
//                LogManager.Error("NativeCampXml error!", nc.cid);
//            }
//            if (!LevelPool.ContainsKey(nc.level))
//            {
//                LevelPool.Add(nc.level, new WeightPool());
//            }
//            LevelPool[nc.level].Add(nc.weight, nc.cid);
//        }
//    }

//    public List<VATownInfo> GenNativeCamp(IntVector2 townPos, int level)
//    {
//        List<VATownInfo> resultList = new List<VATownInfo>();
//        if (!LevelPool.ContainsKey(level)) { return resultList; }

//        System.Random randomSeed = new System.Random(townPos.x+townPos.y);
//        int CampNum = randomSeed.Next(numMin, numMax+1);
//        List<int> genZone = RandomTownUtil.Instance.RandomChoose(CampNum,zoneMin,zoneMax,randomSeed);
//        List<IntVector2> CampPosList = new List<IntVector2>(); 
//        for (int i = 0; i < genZone.Count; i++) {
//            IntVector2 posStart = RandomTownUtil.Instance.GenCampByZone(townPos, genZone[i], distanceMin, distanceMax, randomSeed);
//            if (!VFDataRTGen.IsTownAvailable(posStart.x, posStart.y))
//            {
//                continue;
//            }
//            if (RandomTownManager.Instance.IsTownChunk(new IntVector2(posStart.x << VoxelTerrainConstants._shift, posStart.y << VoxelTerrainConstants._shift)))
//            {
//                continue;
//            }
//            //if (RandomTownManager.Instance.IsTownChunk(new IntVector2(posStart.x + 20 << VoxelTerrainConstants._shift, posStart.y + 20 << VoxelTerrainConstants._shift)))
//            //    continue;
//            //if (RandomTownManager.Instance.IsTownChunk(new IntVector2(posStart.x + VoxelTerrainConstants._numVoxelsPerAxis << VoxelTerrainConstants._shift, posStart.y << VoxelTerrainConstants._shift)))
//            //    continue;
//            //if (RandomTownManager.Instance.IsTownChunk(new IntVector2(posStart.x << VoxelTerrainConstants._shift, posStart.y + VoxelTerrainConstants._numVoxelsPerAxis << VoxelTerrainConstants._shift)))
//            //    continue;
//            CampPosList.Add(posStart);
//        }

//        //gen id and nativeCampInfo 
//        for (int i = 0; i < CampPosList.Count; i++) { 
//            int cid = LevelPool[level].GetRandID(randomSeed);
//            if(cid==-1){
//                break;
//            }
//            NativeCamp nc = randomCampInfo.nativeCamps[cid];
//            VATownInfo nci = new VATownInfo(nc, CampPosList[i]);
//            if (RandomTownManager.Instance.IsContained(nci))
//            {
//                continue;
//            }
//            resultList.Add(nci);
//            //Debug.Log("-----NativeCampPosCenter:" + nci.PosCenter + "CampID: " + nci.Cid);
//        }
//        return resultList;
//    }

//    public void Clear(){
//        LevelPool = new Dictionary<int, WeightPool>();
//    }

//    public void SetCampDistance(float scale)
//    {
//        distanceMin = (int)(randomCampInfo.distanceMin * scale);
//        distanceMax = (int)(randomCampInfo.distanceMax * scale);
//        RandomTownUtil.spawnRadius = Mathf.FloorToInt(RandomTownUtil.spawnRadius0 * scale);
//    }
//}
