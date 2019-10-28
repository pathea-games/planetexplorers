using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VArtifactTownXML;
using VANativeCampXML;
using TownData;

public class VANativeCampManager
{

    static VANativeCampManager mInstance;
    public static VANativeCampManager Instance {
        get {
            if (mInstance == null) 
            {
                mInstance = new VANativeCampManager();
            }
            return mInstance;
        }
    }

    public VANativeCampDesc randomCampInfo;
    public int templateCount;
	public List<NativeCamp> nativeCampsList;

    public Dictionary<int, WeightPool> LevelPoolPuja;
	
	public Dictionary<int, WeightPool> LevelPoolPaja;

    public const int levelCount = 5;
    //private int numMin;
    //private int numMax;
    //private int distanceMin;
    //private int distanceMax;
    private const int zoneMin=0;
    private const int zoneMax=7;
    //init NativeCamp xml data, and tidy it

    public VANativeCampManager()
    {
        LoadXml();
    }
    public void LoadXml()
    {
        string xmlPath = "RandomTown/VANativeCamp";
        if (!GameConfig.IsMultiMode) {
            xmlPath = "RandomTown/VANativeCampSingleMode";
        }

        TextAsset xmlResource = Resources.Load(xmlPath, typeof(TextAsset)) as TextAsset;
        StringReader reader = new StringReader(xmlResource.text);
        if (null == reader)
            return;

        XmlSerializer serializer = new XmlSerializer(typeof(VANativeCampDesc));
        randomCampInfo = (VANativeCampDesc)serializer.Deserialize(reader);
        reader.Close();
		nativeCampsList = randomCampInfo.nativeCamps.ToList();
        templateCount = randomCampInfo.nativeCamps.Count();
        //numMin = randomCampInfo.numMin;
        //numMax = randomCampInfo.numMax;
        //distanceMin = randomCampInfo.distanceMin;
        //distanceMax = randomCampInfo.distanceMax;
    }

    public void Init()
    {
		LevelPoolPuja = new Dictionary<int, WeightPool>();
		LevelPoolPaja = new Dictionary<int, WeightPool>();
        for (int i = 0; i < randomCampInfo.nativeCamps.Count(); i++)
        {
            NativeCamp nc = randomCampInfo.nativeCamps[i];
			if(nc.nativeType==0)//puja
			{
				
				if (nc.level >= levelCount)
				{
					LogManager.Error("NativeCampXml error!", nc.cid);
				}
				if (!LevelPoolPuja.ContainsKey(nc.level))
				{
					LevelPoolPuja.Add(nc.level, new WeightPool());
				}
				LevelPoolPuja[nc.level].Add(nc.weight, nc.cid);
			}
			else{
				
				if (nc.level >= levelCount)
				{
					LogManager.Error("NativeCampXml error!", nc.cid);
				}
				if (!LevelPoolPaja.ContainsKey(nc.level))
				{
					LevelPoolPaja.Add(nc.level, new WeightPool());
				}
				LevelPoolPaja[nc.level].Add(nc.weight, nc.cid);
			}
        }
    }

//    public List<VArtifactTown> GenAllNativeCamp(IntVector2 townPos, ref int id)
//    {
//        List<VArtifactTown> resultList = new List<VArtifactTown>();
////        if (!LevelPool.ContainsKey(level)) { return resultList; }
//
//        System.Random myRand = new System.Random(townPos.x+townPos.y);
//        int CampNum = myRand.Next(numMin, numMax+1);
//        List<int> genZone = VArtifactUtil.RandomChoose(CampNum,zoneMin,zoneMax,myRand);
////        List<IntVector2> CampPosList = new List<IntVector2>(); 
//        for (int i = 0; i < genZone.Count; i++) {
//            IntVector2 townPosStart = VArtifactUtil.GenCampByZone(townPos, genZone[i], distanceMin, distanceMax, myRand);
//            //1.get the 
//			if(Mathf.Abs(townPosStart.x)>RandomMapConfig.Instance.mapRadius||Mathf.Abs(townPosStart.y)>RandomMapConfig.Instance.mapRadius)
//				continue;
//			int areaId = VATownGenerator.Instance.GetAreaIdByRealPos(townPosStart);
//			int level = VATownGenerator.Instance.GetLevelByAreaId(areaId);
//            
//			int cid = LevelPoolPuja[level].GetRandID(myRand);
//            if (cid == -1)
//            {
//                break;
//            }
//
//            NativeCamp town = randomCampInfo.nativeCamps[cid];
//            VArtifactTown townData = new VArtifactTown(town, townPosStart);
//            VArtifactUtil.GetArtifactUnit(townData,town.artifactUnitArray,myRand);
//
//            if (!VArtifactUtil.CheckTownAvailable(townData))
//            {
//                continue;
//            }
//
//            townData.townId = id++;
//			townData.areaId = areaId;
////			VATownGenerator.Instance.DecideNativeAlly(townData);
//			VArtifactTownManager.Instance.AddTownData(townData);
//
//            for (int n = 0; n < townData.VAUnits.Count; n++)
//            {
//                //--to do: GenArtifactUnit
//                VArtifactUnit vvau = townData.VAUnits[n];
//                VArtifactTownManager.Instance.GenArtifactUnit(vvau);
//            }
//            resultList.Add(townData);
//        }
//        return resultList;
//    }

    public void Clear(){
		LevelPoolPuja = new Dictionary<int, WeightPool>();
		LevelPoolPaja = new Dictionary<int, WeightPool>();
    }

    public void SetCampDistance(float scale)
    {
        //distanceMin = (int)(randomCampInfo.distanceMin * scale);
        //distanceMax = (int)(randomCampInfo.distanceMax * scale);
        VArtifactUtil.spawnRadius = Mathf.FloorToInt(VArtifactUtil.spawnRadius0 * scale);
    }
}
