using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea.Maths;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TownData;
using VArtifactTownXML;
using Pathea;
using System.Collections;
using VANativeCampXML;

public class VATileInfo
{
    public List<VArtifactUnit> unitList;
    public VArtifactTown town;

    public VATileInfo(List<VArtifactUnit> unitList, VArtifactTown town)
    {
        this.unitList = unitList;
        this.town = town;
    }

    public void AddUnit(VArtifactUnit unit)
    {
        if (unitList == null)
        {
            unitList = new List<VArtifactUnit>();
        }
        unitList.Add(unit);
    }
}

public class VArtifactTownManager:MonoBehaviour
{
    const int version000 =20160902;
	const int currentVersion = version000;
    static VArtifactTownManager mInstance;
    public static VArtifactTownManager Instance
    {
        get
        {
            return mInstance;
        }
    }

    List<IntVector2> renderReadyList=new List<IntVector2> ();

    public Dictionary<int, VArtifactTown> townIdData = new Dictionary<int, VArtifactTown>();
    public Dictionary<IntVector2, VArtifactTown> townPosInfo = new Dictionary<IntVector2, VArtifactTown>();
    public Dictionary<IntVector2, VArtifactTown> TownPosInfo
    {
        get { return townPosInfo; }
    }

    public Dictionary<IntVector2, VArtifactUnit> unitCenterData = new Dictionary<IntVector2, VArtifactUnit>();
    public Dictionary<IntVector2, VATileInfo> townTile = new Dictionary<IntVector2, VATileInfo>();

	public delegate void TownDestroyed(int allyId);
	public event TownDestroyed TownDestroyedEvent;    
	public void RegistTownDestryedEvent(TownDestroyed eventListener){
		TownDestroyedEvent-=eventListener;
		TownDestroyedEvent+=eventListener;
	}
	public void UnRegistTownDestryedEvent(TownDestroyed eventListener){
		TownDestroyedEvent-=eventListener;
	}

    public List<VArtifactUnit> OutputedTownList = new List<VArtifactUnit>();
    public VArtifactUnit outputedTown;
    const int CACHE_COUNT = 2;
       
    public const int levelCount = 5;
    Dictionary<int, WeightPool> LevelPool = new Dictionary<int, WeightPool>();
    //int townDistanceX;
    //int townDistanceZ;
	List<int> townNamePool =new List<int> ();
    #region xml
    VArtifactTownDesc VartifactTownXmlInfo = new VArtifactTownDesc();
    private VATown startTown;
	List<VATown> vatownList;
    #endregion

    public int missionStartBuildingID = -1;
	public int missionStartNpcID = -1;
	public int missionStartNpcEntityId = -1;
    public Vector3 playerStartPos = new Vector3();
    //int townTemplateNum;

    int minX = -19200;

    public int MinX
    {
        get { return minX; }
        set { minX = value; }
    }

    int minZ = -19200;

    public int MinZ
    {
        get { return minZ; }
        set { minZ = value; }
    }
    int maxX = 19200;

    public int MaxX
    {
        get { return maxX; }
        set { maxX = value; }
    }
    int maxZ = 19200;

    public int MaxZ
    {
        get { return maxZ; }
        set { maxZ = value; }
    }
    int levelRadius = 3000;

    public int LevelRadius
    {
        get { return levelRadius; }
        set { levelRadius = value; }
    }
    int detectedChunkNum = 32;

    public int DetectedChunkNum
    {
        get { return detectedChunkNum; }
        set { detectedChunkNum = value; }
    }
	const int detectedChunkAroundNum = 8;

    Dictionary<int, List<VArtifactTown>> levelNpcTown = new Dictionary<int,List<VArtifactTown>> ();

    public Dictionary<IntVector2,int> mRenderedChunk;


    #region saveData

    public Dictionary<int, int> capturedCampId=new Dictionary<int,int> ();

    public List<IntVector2> mDetectedTowns = new List<IntVector2>();
	public Dictionary<int,VATSaveData> mVATSaveData = new Dictionary<int,VATSaveData>();
	public Dictionary<int,List<int>> mAliveBuildings = new Dictionary<int, List<int>>();
	public Dictionary<int,List<SceneEntityPosAgent>> MonsterPointAgentDic = new Dictionary<int,List<SceneEntityPosAgent>>();


	public void SetSaveData(int townId,int ms_id){
		if(!mVATSaveData.ContainsKey(townId)){
			mVATSaveData.Add(townId,new VATSaveData ());
		}
		VATSaveData vs = mVATSaveData[townId];
		vs.townId = townId;
		vs.ms_id = ms_id;
	}

	public void SetSaveData(int townId,double lastHour,double nextHour){
		if(!mVATSaveData.ContainsKey(townId)){
			mVATSaveData.Add(townId,new VATSaveData ());
		}
		VATSaveData vs = mVATSaveData[townId];
		vs.townId = townId;
		vs.lastHour= lastHour;
		vs.nextHour = nextHour;
	}
    #endregion


    void Awake()
    {
        mInstance = this;

        InitData();

    }
	int counter=0;
	void Update(){
		counter++;
		if(counter % 120==0){
			counter=0;
			DetectTownsAround();
		}

		if(renderTownList.Count > 0){
			lock(renderTownList){
				foreach(IntVector2 tileIndex in renderTownList){
					RenderReady(tileIndex);
				}
				renderTownList.Clear();
			}
		}
	}
//	public delegate void RenderTownEvent(IntVector2 tileIndex);
//	public  event RenderTownEvent AddRenderTownListener;
	public List<IntVector2> renderTownList= new List<IntVector2> ();

    void InitData(){
        
        
    }

	void InitVATData(){
		foreach(KeyValuePair<int,VATSaveData> vs in mVATSaveData)
		{
			if(townIdData[vs.Key]!=null){
				townIdData[vs.Key].ms_id = vs.Value.ms_id;
				townIdData[vs.Key].lastHour = vs.Value.lastHour;
				townIdData[vs.Key].nextHour = vs.Value.nextHour;
			}
			if(MonsterSiege_Town.Instance!=null)
			MonsterSiege_Town.Instance.OnNewTown(townIdData[vs.Key]);
		}
	}

    public void LoadXMLAtPath()
    {
        string xmlPath = "RandomTown/VArtifactTown";
        TextAsset xmlResource = Resources.Load(xmlPath, typeof(TextAsset)) as TextAsset;
        StringReader reader = new StringReader(xmlResource.text);
        if (null == reader)
            return;

        XmlSerializer serializer = new XmlSerializer(typeof(VArtifactTownDesc));
        VartifactTownXmlInfo = (VArtifactTownDesc)serializer.Deserialize(reader);
        reader.Close();
        //townDistanceX = VartifactTownXmlInfo.distanceX;
        //townDistanceZ = VartifactTownXmlInfo.distanceZ;
    }

    public void InitISO()
    {
        string ISOPath = GameConfig.PEDataPath + @"RandomTownArt";
        VArtifactUtil.ISOPath = ISOPath;
        List<string> IsoPathNames = new List<string>();
        List<string> fileNames= new List<string> ();
        foreach(Town_artifacts ta in VArtifactUtil.townArtifactsData.Values){
            if (!fileNames.Contains(ta.isoName))
            {
                fileNames.Add(ta.isoName);
                IsoPathNames.Add(VArtifactUtil.GetISONameFullPath(ta.isoName));
            }
        }
        foreach (string iso in IsoPathNames)
        {
            VArtifactUtil.LoadIso(iso);
        }
    }


    public void Clear()
    {
        townIdData.Clear();
        townPosInfo.Clear();

        unitCenterData.Clear();
        townTile.Clear();

        OutputedTownList.Clear();

        LevelPool.Clear();
        levelNpcTown.Clear();
        capturedCampId.Clear();
		mAliveBuildings.Clear();
		MonsterPointAgentDic.Clear();
        VArtifactUtil.Clear();
		VATownGenerator.Instance.ClearData();
		TownDestroyedEvent = null;
    }

    public void GenTown()
	{
		VATownGenerator.Instance.ClearData();
		
		LoadXMLAtPath();
		VATownGenerator.Instance.InitAllyDistribution(new System.Random(RandomMapConfig.AllyGenSeed));
		System.Random myRand = new System.Random(RandomMapConfig.TownGenSeed);
		startTown = VartifactTownXmlInfo.vaStartTown;
//		if(startTown.artifactUnitArray.Length<1){
//			Debug.LogError("startTown artifactUnitArray length = "+startTown.artifactUnitArray.Length);
//			LoadXMLAtPath();
//		}else if(startTown.artifactUnitArray[0].npcIdNum.Length<1){
//			Debug.LogError("startTown npcIdNum length = "+startTown.artifactUnitArray[0].npcIdNum.Length);
//			LoadXMLAtPath();
//		}else if(startTown.artifactUnitArray[0].buildingIdNum.Length<1){
//			Debug.LogError("startTown buildingIdNum length = "+startTown.artifactUnitArray[0].buildingIdNum.Length);
//			LoadXMLAtPath();
//		}
		#region init 
		vatownList = VartifactTownXmlInfo.vaTown.ToList();
		//townTemplateNum = vatownList.Count;


		//init LevelPool
//		Debug.Log("init LevelPool");
		for (int i = 0; i < vatownList.Count; i++)
        {
			VATown t = vatownList[i];
			if (t.level >= levelCount)
            {
                LogManager.Error("level >= levelCount Xml error!", t.tid);
            }
            if (!LevelPool.ContainsKey(t.level))
            {
                LevelPool.Add(t.level, new WeightPool());
            }
            LevelPool[t.level].Add(t.weight, t.tid);
        }

		townNamePool = new List<int> ();
		townNamePool.AddRange(VArtifactUtil.townNameData.Keys);


		//check xml&dataBase
//		Debug.Log("check xml&dataBase");
		foreach(ArtifactUnit au in VartifactTownXmlInfo.vaStartTown.artifactUnitArray){
			Town_artifacts townDataFromDataBase = VArtifactUtil.townArtifactsData[Convert.ToInt32(au.id)];
			foreach(BuildingIdNum bin in au.buildingIdNum){
				if(bin.posIndex>=townDataFromDataBase.buildingCell.Count){
					Debug.LogError("townXML&DataBase error! Tid: "+VartifactTownXmlInfo.vaStartTown.tid+",vauId: "+au.id);
					Debug.LogError("PosIndex Too large! bid: " + bin.bid + " PosIndex: " +bin.posIndex + " B_Position Count: " + townDataFromDataBase.buildingCell.Count);
				}
			}
		}
		foreach(VATown vat in vatownList){
			foreach(ArtifactUnit au in vat.artifactUnitArray){
				Town_artifacts townDataFromDataBase = VArtifactUtil.townArtifactsData[Convert.ToInt32(au.id)];
				foreach(BuildingIdNum bin in au.buildingIdNum){
					if(bin.posIndex>=townDataFromDataBase.buildingCell.Count){
						Debug.LogError("townXML&DataBase error! Tid: "+vat.tid+",vauId: "+au.id);
						Debug.LogError("PosIndex Too large! bid: " + bin.bid + " PosIndex: " +bin.posIndex + " B_Position Count: " + townDataFromDataBase.buildingCell.Count);
						
					}
				}
			}
		}

        VANativeCampManager.Instance.Init();
        InitParam();
		unitCenterData = new Dictionary<IntVector2, VArtifactUnit>();
		#endregion
        //unitTile = new Dictionary<IntVector2, List<IntVector2>>();
//		Debug.Log("genStartTown");
		List<int> genLine = VATownGenerator.Instance.GetPickedGenLine();
		int genCountMax = VATownGenerator.Instance.GetTownAmountMax();
		int genCountMin = VATownGenerator.Instance.GetTownAmountMin();
		int townId = 0;

		for(int i=0;i<genLine.Count;i++){
			int level = VATownGenerator.Instance.GetLevelByLineIndex(i);
			IntVector2 genPosStart;
			int generatedCount =0;
			//1.gen start town
			int genAreaId = genLine[i];
			if (i==0)
			{
				genPosStart = VATownGenerator.Instance.GenTownPos(genAreaId,myRand);
				//templateId = -1;
				missionStartNpcID = startTown.artifactUnitArray[0].npcIdNum[0].nid;
				missionStartBuildingID = startTown.artifactUnitArray[0].buildingIdNum[0].bid;
				
				VArtifactTown townData = new VArtifactTown (startTown,genPosStart);
				VArtifactUtil.GetArtifactUnit(townData,startTown.artifactUnitArray,myRand);
				if(!VArtifactUtil.CheckTownAvailable(townData))
					continue;
				townData.townId=townId++;
				townData.townNameId = PickATownName(myRand);
				townData.areaId = genAreaId;
				townData.AllyId = TownGenData.PlayerAlly;
				townData.isMainTown = true;
				VATownGenerator.Instance.AddAllyTown(townData);
				InitTownData(townData,genPosStart,myRand);
				generatedCount++;
//				Debug.Log("genEmptyTown");
				GenEmptyTown(genPosStart,0,ref townId,genAreaId,myRand);
			}
			int genCount = genCountMax-generatedCount;
			GenSomeTowns(genCount,level,ref townId,genAreaId,myRand,ref generatedCount);
			for(int g=0;g<3;g++){
				if(generatedCount<genCountMin)
			   	{
					genCount = genCountMax-generatedCount;
					GenSomeTowns(genCount,level,ref townId,genAreaId,myRand,ref generatedCount);
				}else{
					break;
				}
			}
		}


		//gen branch Town
//		Debug.Log("genBranchTown");
		int generatedBranchCount =0;
		for(int i=0;i<TownGenData.AreaCount;i++){
			int level = VATownGenerator.Instance.GetLevelByLineIndex(i);
			int genAreaId = genLine[i];
			GenBranchTowns(VATownGenerator.Instance.branchTownCountMax,level,ref townId,genAreaId,myRand,ref generatedBranchCount);
		}

		VATownGenerator.Instance.ConnectMainTowns();
		VATownGenerator.Instance.GenerateConnection(myRand);
        VArtifactUtil.ClearAllISO();

		//test
		//TestNewTownGen();
		//TestNewTerrain();
		InitTownHeight();
		if(Application.isEditor)
			PrintTownPos();

		if(PeGameMgr.IsMulti){
			List<int> idList = new List<int> ();
			List<Vector3> posList = new List<Vector3> ();
			foreach(int key in townIdData.Keys)
			{
				idList.Add(key);
				posList.Add(townIdData[key].TransPos);
			}
			NetworkManager.SyncServer(EPacketType.PT_Common_InitTownPos,idList.ToArray(),posList.ToArray());
        }
    }
	
	public void GenEmptyTown(IntVector2 townPos,int level,ref int townId,int genAreaId,System.Random myRand){
		int genSuccess = 0;
		while(genSuccess<3){
			List<int> genZone = VArtifactUtil.RandomChoose(TownGenData.PlayerStartEmptyTownCount,0,7,myRand);
			for (int i = 0; i < genZone.Count; i++) {
				IntVector2 townPosStart = VArtifactUtil.GenCampByZone(townPos, genZone[i], VATownGenerator.Instance.playerEmptyTownDistanceMin, VATownGenerator.Instance.playerEmptyTownDistanceMax, myRand);
				//1.get the 
				if(Mathf.Abs(townPosStart.x)>RandomMapConfig.Instance.mapRadius||Mathf.Abs(townPosStart.y)>RandomMapConfig.Instance.mapRadius)
					continue;
				IntVector2 genPosStart = VATownGenerator.Instance.GenTownPos(genAreaId,myRand);
				int templateId;
				VArtifactTown townData;
				templateId = LevelPool[level].GetRandID(myRand);
				startTown = vatownList.Find(it=>it.tid==templateId);
				townData= new VArtifactTown (startTown,genPosStart);
				VArtifactUtil.GetArtifactUnit(townData,startTown.artifactUnitArray,myRand);
				if(!VArtifactUtil.CheckTownAvailable(townData))
					continue;
				townData.townId = townId++;
				townData.townNameId = 0;
				townData.areaId = genAreaId;
				townData.AllyId = TownGenData.PlayerAlly;
				townData.isEmpty = true;
				townData.isMainTown =false; // VATownGenerator.Instance.DecideMainTownAndAlliance(myRand,townData);
				
				InitTownData(townData,genPosStart,myRand);
				genSuccess++;
				VATownGenerator.Instance.AddEmtpyTown(townData);
				Debug.Log("emptyTown:"+genPosStart);
				if(genSuccess>=3)
					break;
			}
		}
	}

	public void GenSomeTowns(int genCount,int level,ref int townId,int genAreaId,System.Random myRand,ref int generatedCount){
		for(int j=0;j<genCount;j++){
			IntVector2 genPosStart = VATownGenerator.Instance.GenTownPos(genAreaId,myRand);
			AllyType at = VATownGenerator.Instance.GetAllyTypeByAreaId(genAreaId);
			int templateId;
			VArtifactTown townData;
			if(at==AllyType.Player||at==AllyType.Npc||VATownGenerator.Instance.GetAllyTownCount(TownGenData.PlayerAlly)<TownGenData.PlayerStartTownCount){
				if(townId==TownGenData.SpecifiedTownId)
					templateId = LevelPool[TownGenData.SpecifiedTownLevel].GetRandID(myRand);
				else if(townId ==TownGenData.SpecifiedTownId1)
					templateId = LevelPool[TownGenData.SpecifiedTownLevel1].GetRandID(myRand);
				else
					templateId = LevelPool[level].GetRandID(myRand);
				startTown = vatownList.Find(it=>it.tid==templateId);
				townData= new VArtifactTown (startTown,genPosStart);
				VArtifactUtil.GetArtifactUnit(townData,startTown.artifactUnitArray,myRand);
				if(!VArtifactUtil.CheckTownAvailable(townData))
					continue;
			}else{
				if(at==AllyType.Puja)
					templateId = VANativeCampManager.Instance.LevelPoolPuja[level].GetRandID(myRand);
				else
					templateId = VANativeCampManager.Instance.LevelPoolPaja[level].GetRandID(myRand);
				NativeCamp nc = VANativeCampManager.Instance.nativeCampsList.Find(it=>it.cid==templateId);
				townData= new VArtifactTown (nc,genPosStart);
				VArtifactUtil.GetArtifactUnit(townData,nc.artifactUnitArray,myRand);
				if(!VArtifactUtil.CheckTownAvailable(townData))
					continue;
			}

			townData.townId = townId++;
			townData.townNameId = PickATownName(myRand);
			townData.areaId = genAreaId;
			if(VATownGenerator.Instance.GetAllyTownCount(TownGenData.PlayerAlly)<TownGenData.PlayerStartTownCount)
				townData.AllyId = TownGenData.PlayerAlly;
			else
				townData.AllyId = VATownGenerator.Instance.GetAllyIdByAreaId(genAreaId);
			VATownGenerator.Instance.AddAllyTown(townData);
			townData.isMainTown =true; // VATownGenerator.Instance.DecideMainTownAndAlliance(myRand,townData);
			InitTownData(townData,genPosStart,myRand);
			generatedCount++;
		}
	}

	public void GenBranchTowns(int genCount,int level,ref int townId,int genAreaId,System.Random myRand,ref int generatedCount){
		for(int j=0;j<genCount;j++){
			IntVector2 genPosStart = VATownGenerator.Instance.GenTownPos(genAreaId,myRand);
			int templateId = LevelPool[level].GetRandID(myRand);
			startTown = vatownList.Find(it=>it.tid==templateId);
			VArtifactTown townData= new VArtifactTown (startTown,genPosStart);
			VArtifactUtil.GetArtifactUnit(townData,startTown.artifactUnitArray,myRand);
			if(!VArtifactUtil.CheckTownAvailable(townData))
				continue;
			townData.townId = townId++;
			townData.townNameId = PickATownName(myRand);
			townData.areaId = genAreaId;
			townData.AllyId = TownGenData.PlayerAlly;
			VATownGenerator.Instance.AddAllyTown(townData);
			townData.isMainTown =false; // VATownGenerator.Instance.DecideMainTownAndAlliance(myRand,townData);
			InitTownData(townData,genPosStart,myRand);
			generatedCount++;
		}
	}

	public void InitTownData(VArtifactTown townData,IntVector2 genPosStart,System.Random myRand){
		AddTownData(townData);
		for (int n = 0; n < townData.VAUnits.Count; n++)
		{
			VArtifactUnit vvau = townData.VAUnits[n];
			GenArtifactUnit(vvau);
		}
	}

	public void AddTownData(VArtifactTown townData){
		townIdData.Add(townData.townId, townData);
		townPosInfo.Add(townData.PosCenter,townData);

		if(townData.type==VArtifactType.NpcTown){
			if (!levelNpcTown.ContainsKey(townData.level))
			{
				List<VArtifactTown> townInfoList = new List<VArtifactTown>();
				townInfoList.Add(townData);
				levelNpcTown.Add(townData.level, townInfoList);
			}
			else
			{
				levelNpcTown[townData.level].Add(townData);
			}
		}
		VATownGenerator.Instance.AddTown(townData.areaId,townData);
	}
	public void SwitchTownId(VArtifactTown t1,VArtifactTown t2){
		int tmpId = t2.townId;
		t2.townId = t1.townId;
		t1.townId = tmpId;
		townIdData[t1.townId] = t1;
		townIdData[t2.townId] = t2;
	}
	int PickATownName(System.Random rand){
		int pickIndex = rand.Next(townNamePool.Count);
		int nameId =VArtifactUtil.GetTownNameId(townNamePool[pickIndex]);
		townNamePool.RemoveAt(pickIndex);
		return nameId;
	}

	void InitTownHeight(){
		foreach(VArtifactTown vat in townPosInfo.Values){
			foreach(VArtifactUnit vau in vat.VAUnits){
				vau.SetHeight( GetMinHeight(vau.PosStart,vau.PosEnd)-ArtifactTownConst.DEPTH_IN_TERRAIN);
			}
			vat.height = Mathf.CeilToInt(vat.VAUnits[0].worldPos.y + vat.VAUnits[0].vaSize.z);
			if(vat.templateId==-1){
				VArtifactUnit vauStart = vat.VAUnits[0];
				Quaternion q = new Quaternion();
				q.eulerAngles = new Vector3(0, vauStart.rot, 0);
				Vector3 ofs = vauStart.npcPos[0]+new Vector3(0,1,-1) - vauStart.worldPos;
				ofs.x += vauStart.isoStartPos.x;
				ofs.y += vauStart.worldPos.y;
				ofs.z += vauStart.isoStartPos.y;
				Vector3 npcStartWorldPos = vauStart.worldPos + q * ofs;
				playerStartPos = npcStartWorldPos;
			}
//			if(vat.type==VArtifactType.NpcTown){
				if(vat.templateId==-1)
					AIErodeMap.AddErode(new Vector3(vat.PosCenter.x, VFDataRTGen.GetPosHeight(vat.PosCenter), vat.PosCenter.y), vat.radius + 96);
//				else if(vat.level==0)
//					AIErodeMap.AddErode(new Vector3(vat.PosCenter.x, VFDataRTGen.GetPosHeight(vat.PosCenter), vat.PosCenter.y), vat.radius + 64);
				else
					AIErodeMap.AddErode(new Vector3(vat.PosCenter.x, VFDataRTGen.GetPosHeight(vat.PosCenter), vat.PosCenter.y), vat.radius + 64);
//			}
		}
	}

	float GetAvgHeight(IntVector2 startPos,IntVector2 endPos){
		return (VFDataRTGen.GetPosHeight(startPos.x,(startPos.y+endPos.y)/2)
		        +VFDataRTGen.GetPosHeight((startPos.x+endPos.x)/2,startPos.y)
		        +VFDataRTGen.GetPosHeight(endPos.x,(startPos.y+endPos.y)/2)
		        +VFDataRTGen.GetPosHeight((startPos.x+endPos.x)/2,endPos.y)
		        +VFDataRTGen.GetPosHeight((startPos.x+endPos.x)/2,(startPos.y+endPos.y)/2)
	             )/5.0f;
	}	
	float GetMinHeight(IntVector2 startPos,IntVector2 endPos){
		return Mathf.Min(VFDataRTGen.GetPosHeight(startPos.x,(startPos.y+endPos.y)/2)
		                 ,VFDataRTGen.GetPosHeight((startPos.x+endPos.x)/2,startPos.y)
		                 ,VFDataRTGen.GetPosHeight(endPos.x,(startPos.y+endPos.y)/2)
		                 ,VFDataRTGen.GetPosHeight((startPos.x+endPos.x)/2,endPos.y)
		                 ,VFDataRTGen.GetPosHeight((startPos.x+endPos.x)/2,(startPos.y+endPos.y)/2)
		        );
	}	

	void TestNewTownGen()
	{
		Dictionary<IntVector2,int> testDic = new Dictionary<IntVector2,int>();
		int t1 = System.DateTime.Now.Second;
		int plainCount =0;
		int seaCount =0;
		int grasslandCount =0;
		int desertCount =0;
		for(int xt=-4000;xt<=4000;xt+=64)
		for(int zt = -4000;zt<=4000;zt+=64){
			IntVector2 worldXZ = new IntVector2(xt,zt);
			int height = VFDataRTGen.GetPosHeight(worldXZ,true);
			if(VFDataRTGen.IsSea(height))
				seaCount++;
			if(height>20&&height<50){
				plainCount++;
				testDic.Add(worldXZ,height);
				//Debug.LogError(worldXZ);
			}
			RandomMapType xzType = VFDataRTGen.GetXZMapType(worldXZ);
			if(xzType==RandomMapType.GrassLand)
				grasslandCount++;
			if(xzType==RandomMapType.Desert)
				desertCount++;
		}
		Dictionary<IntVector2,int> testTownDic = new Dictionary<IntVector2, int>();
		Dictionary<IntVector2,int> testTownDic2 = new Dictionary<IntVector2, int>();
		Dictionary<int,Dictionary<IntVector2,int>> districtDic = new Dictionary<int,Dictionary<IntVector2,int>>();
		for(int di=0;di<16;di++){
			districtDic.Add(di,new Dictionary<IntVector2, int>());
		}
		foreach(KeyValuePair<IntVector2,int> posHeight in testDic)
		{
			if(testDic.ContainsKey(posHeight.Key+new IntVector2(-64,0))&&
			   testDic.ContainsKey(posHeight.Key+new IntVector2(64,0))&&
			   testDic.ContainsKey(posHeight.Key+new IntVector2(0,-64))&&
			   testDic.ContainsKey(posHeight.Key+new IntVector2(0,64)))
			{
				testTownDic.Add(posHeight.Key,posHeight.Value);
				
				
				int districX  = posHeight.Key.x/2000;
				if(posHeight.Key.x<0)
					districX--;
				int districZ = posHeight.Key.y/2000;
				if(posHeight.Key.y<0)
					districZ--;
				//Debug.LogError("x,z:"+posHeight.Key+"districtX,Z: "+districX+","+districZ);
				int districtNo = districX+2+4*(1-districZ);
				if(districtDic.ContainsKey(districtNo))
					districtDic[districtNo].Add(posHeight.Key,posHeight.Value);
				else
					Debug.LogError("not containsKey: "+districtNo);
			}
		}
		foreach(KeyValuePair<IntVector2,int> posHeight in testDic)
		{
			if(testDic.ContainsKey(new IntVector2(posHeight.Key.x+64,posHeight.Key.y)))
				testTownDic2.Add(posHeight.Key,posHeight.Value);
		}
		int t2 = System.DateTime.Now.Second;
		Debug.LogError("time: "+(t2-t1)+"seaCount:"+seaCount+"plainCount:"+plainCount+"grassland:"+grasslandCount+"desert:"+desertCount+"townCount:"+testTownDic.Count+"townCount2:"+testTownDic2.Count);
		for(int di=0;di<16;di++){
			Debug.LogError("districtTownCount: "+di+"_"+districtDic[di].Count);
		}
		int testX = (-1)>>VoxelTerrainConstants._shift;
		int testX2 = (-1)/32;
		int testX3 = 1>>5;
		Debug.LogError("testX1:"+testX+"testX2:"+testX2+"testX3:"+testX3);
	}

	void TestNewTerrain(){
		for(int xt=-4000;xt<=4000;xt+=64)
		for(int zt = -4000;zt<=4000;zt+=64){
			IntVector2 worldXZ = new IntVector2(xt,zt);
			float ftertype = VFDataRTGen.GetfNoise12D1ten(xt,zt);
			if(ftertype>90f)
				Debug.Log("pos:"+worldXZ+" ftertype:"+ftertype);
		}

	}

    public void GenArtifactUnit(VArtifactUnit vau)
    {
        unitCenterData.Add(vau.PosCenter, vau);
        LinkToChunk(vau);
    }


    public void LinkToChunk(VArtifactUnit vaUnit)
    {
        List<IntVector2> chunkIndexList = VArtifactUtil.LinkedChunkIndex(vaUnit);
        for (int i = 0; i < chunkIndexList.Count; i++)
        {
            IntVector2 chunkIndexXZ = chunkIndexList[i];
            if (!townTile.ContainsKey(chunkIndexXZ))
            {
                //List<IntVector2> ls = new List<IntVector2>();
                //ls.Add(townInfo.PosCenter);
                //unitTile.Add(chunkIndexXZ, ls);
                List<VArtifactUnit> vauList = new List<VArtifactUnit> ();
                vauList.Add(vaUnit);
                VATileInfo vati = new VATileInfo(vauList, vaUnit.vat);
                //TileXZUnitList.Add(chunkIndexXZ, vauList);
                townTile.Add(chunkIndexXZ, vati);
            }
            else
            {
                //unitTile[chunkIndexXZ].Add(townInfo.PosCenter);
                townTile[chunkIndexXZ].AddUnit(vaUnit);
            }
        }
    }

    public List<VArtifactUnit> OutputTownData(IntVector2 tileIndex)
    {
        List<VArtifactUnit> townList = GetTileUnitList(tileIndex);
		if(townList==null)
			return null;
		townList.RemoveAll(it=>it.vat.isEmpty);
		if(townList.Count==0)
			return null;

        List<VArtifactUnit> needOutput = new List<VArtifactUnit> ();
        List<int> notAvailableIndex = new List<int> ();
        //1.find need output
        for (int i = 0; i < townList.Count; i++)
        {
            VArtifactUnit outputTown = townList[i];
            if (!OutputedTownList.Contains(outputTown))
            {
                needOutput.Add(outputTown);
            }else{
                notAvailableIndex.Add(OutputedTownList.FindIndex(it=>it==outputTown));
            }
        }
        //2.check if need to clear
        if (needOutput.Count + OutputedTownList.Count <= CACHE_COUNT)
        {
            //3.not necessary to clear
            for (int i = 0; i < needOutput.Count; i++)
            {
                VArtifactUtil.OutputVoxels(needOutput[i].worldPos, needOutput[i],needOutput[i].rot);
                OutputedTownList.Add(needOutput[i]);
            }
        }
        else
        {
            //3.need to clear
            //3.1 find availble index (to clear)
            List<int> availableIndex = new List<int>();
            for (int i = 0; i < CACHE_COUNT; i++)
            {
                if (!notAvailableIndex.Contains(i))
                {
                    availableIndex.Add(i);
                }
            }

            //3.2. clear enough room
            int removeCount = OutputedTownList.Count+needOutput.Count-CACHE_COUNT;
            for (int i = 0; i < removeCount; i++)
            {
                int removeIndex = availableIndex[i]-i;
                bool clearISOFlag = true;
                for (int j = 0; j < OutputedTownList.Count; j++)
                {
                    if (OutputedTownList[j].isoGuId == OutputedTownList[removeIndex].isoGuId && j != removeIndex)
                    {
                        clearISOFlag = false;
                        break;
                    }
                }
                if (clearISOFlag)
                {
                    VArtifactUtil.isos.Remove(OutputedTownList[removeIndex].isoGuId);
                }
                OutputedTownList[removeIndex].Clear();
                OutputedTownList.RemoveAt(removeIndex);
                //Debug.LogError(" remove:" + (availableIndex[i] - i));
            }
            //Debug.LogError("removeCount:" + removeCount);

            for (int i = 0; i < needOutput.Count; i++)
            {
                VArtifactUtil.OutputVoxels(needOutput[i].worldPos, needOutput[i],needOutput[i].rot);
                OutputedTownList.Add(needOutput[i]);
            }
            //Debug.LogError("AddCount:" + needOutput.Count);
        }
        //Debug.LogError("test: OutputedTownList:" + OutputedTownList.Count);
        return townList;   
    }
    

    public void GenTownFromTileIndex(IntVector2 tileIndex) 
    {
        if (!TileContainsTown(tileIndex))
        {
            return;
        }
        List<VArtifactUnit> vauList = GetTileUnitList(tileIndex);

        for (int i = 0; i < vauList.Count; i++)
        {
            ArtifactAddToRender(vauList[i], tileIndex);
        }
    }

    public void RandomArtifactTown(int townId)
    {
        RandomArtifactTown(townIdData[townId]);
    }
    public void RandomArtifactTown(VArtifactTown vat)
    {
        if (vat.isRandomed)
        {
            return;
        }
        for (int i = 0; i < vat.VAUnits.Count;i++ )
        {
            RandomArtifactUnit(vat.VAUnits[i]);
        }
        vat.isRandomed = true;
    }

    public void RandomArtifactUnit(VArtifactUnit vau)
    {
        if(vau.isRandomed)
        {
            return;
        }
		//Debug.Log("start RandomArtifactUnit TownId:"+vau.vat.townId);
        List<int> randomBuildingList = new List<int>();
        Dictionary<int,int> appointedBuildings = new Dictionary<int,int> ();
        int randomBuildingNum = 0;
        int buildingTypeNum = vau.buildingIdNum.Count;
		int appointedBuildingCount = 0;
		int cellCount = vau.buildingCell.Count;
        
        for (int t = 0; t < buildingTypeNum; t++)
        {
            if (vau.buildingIdNum[t].posIndex != -1)
            {
                if (vau.buildingIdNum[t].posIndex >= vau.buildingCell.Count)
                {
                    Debug.LogError("PosIndex Too large! Tid: " + vau.vat.templateId + " bid: " + vau.buildingIdNum[t] + " PosIndex: " + vau.buildingIdNum[t].posIndex + " B_Position Count: " + vau.buildingCell.Count);
					for (int n = 0; n < vau.buildingIdNum[t].num; n++)
					{
						randomBuildingList.Add(vau.buildingIdNum[t].bid);
					}
					randomBuildingNum += vau.buildingIdNum[t].num;
					continue;
                }
                appointedBuildings.Add(vau.buildingIdNum[t].posIndex,vau.buildingIdNum[t].bid);
                appointedBuildingCount++;
            }
            else
            {
                for (int n = 0; n < vau.buildingIdNum[t].num; n++)
                {
                    randomBuildingList.Add(vau.buildingIdNum[t].bid);
                }
                randomBuildingNum += vau.buildingIdNum[t].num;
            }
        }

		//add empty cell
		int randomCellAmount = cellCount - appointedBuildingCount;
        if (randomCellAmount > randomBuildingNum)
        {
            for (int t = 0; t < randomCellAmount - randomBuildingNum; t++)
            {
                randomBuildingList.Add(-1);
            }
        }


        System.Random randSeed = new System.Random(vau.PosCenter.x + vau.PosCenter.y);

        VArtifactUtil.Shuffle(randomBuildingList, randSeed);


        if (vau.vat.templateId == -1)
        {
            if (!appointedBuildings.ContainsValue(missionStartBuildingID))
            {
                if (randomBuildingNum > randomCellAmount)
                {
                    randomBuildingList.RemoveRange(randomCellAmount, randomBuildingNum - randomCellAmount);
                }
                if (!randomBuildingList.Contains(missionStartBuildingID))
                {
                    int missionBuildingIndex = randSeed.Next(randomCellAmount);
                    randomBuildingList[missionBuildingIndex] = missionStartBuildingID;
                }
            }
        }

		List<int> allBuildingIdList = new List<int>();
		for(int i=0;i<cellCount;i++){
			if(appointedBuildings.ContainsKey(i)){
				allBuildingIdList.Add(appointedBuildings[i]);
			}else{
				allBuildingIdList.Add(randomBuildingList[0]);
				randomBuildingList.RemoveAt(0);
			}
		}


		CreateBuildingInfo(vau, allBuildingIdList);

        List<Vector3> npcPos = vau.npcPos;
        List<NpcIdNum> npcIdNum = vau.npcIdNum;
        List<int> npcIdList= new List<int> ();
        for(int i=0;i<npcIdNum.Count;i++){
            int nid = npcIdNum[i].nid;
            int num = npcIdNum[i].num;
            for(int j=0;j<num;j++){
                npcIdList.Add(nid);
            }
        }

        int npcPosCount = npcPos.Count;
        int npcIdCount = npcIdList.Count;
        if (vau.type == VArtifactType.NpcTown && vau.vat.templateId == -1 && vau.unitIndex == 0)
        {
            npcPosCount--;
            npcIdList.Remove(missionStartNpcID);
			npcIdCount--;
        }
        if (npcPosCount > npcIdCount)
        {
            for (int i = 0; i < npcPosCount - npcIdCount; i++)
            {
                npcIdList.Add(-1);
            }
        }
        VArtifactUtil.Shuffle(npcIdList, randSeed);

        CreateTownNPCAndNative(vau, npcIdList);

        vau.isRandomed = true;
		
		//Debug.Log("end RandomArtifactUnit TownId:"+vau.vat.townId);
    }


    
    public void CreateBuildingInfo(VArtifactUnit vau, List<int> buildingIdList)
    {
        if (vau.type == VArtifactType.NpcTown)
        {
            //--to do: cell
            int cellCount = vau.buildingCell.Count();
            for(int i=0;i<cellCount;i++)
            {
                if (buildingIdList[i] == -1)
                {
                    continue;
                }
                int bid = buildingIdList[i];
                if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(bid))
                {
                    LogManager.Error("bid = [", bid, "] not exist in database!");
                    return;
                }
                BuildingCell vaubc = vau.buildingCell[i];
                Vector3 buildingPos = VArtifactUtil.GetPosAfterRotation(vau, vaubc.cellPos);
                float buildingRot = vaubc.cellRot + vau.rot;
                int buildingNo = vau.vat.buildingNo++;
                BuildingID buildingId = new BuildingID(vau.vat.townId,buildingNo);

                Vector2 size = new Vector2(20,20);
                BlockBuilding bb = BlockBuilding.s_tblBlockBuildingMap[bid];
                size = bb.mSize;
                //Debug.Log("Pos:"+buildingPos+" size:"+size);
                VABuildingInfo bdInfo = new VABuildingInfo(buildingPos, buildingRot, bid, buildingId, VABuildingType.Prefeb, vau, size);
                VABuildingManager.Instance.AddBuilding(bdInfo);
                if (vau.vat.templateId == -1 && bid == missionStartBuildingID)
                {
                    VABuildingManager.Instance.missionBuilding.Add(0, buildingId);
                    Debug.Log("<color=yellow>mission Building pos: " + buildingPos+"</color>");
					PlayerMission.StoreBuildingPos(0,bdInfo.frontDoorPos);
                }
                vau.buildingPosID[buildingPos] = buildingId;
            }
        }
        else
        {
			vau.towerPos.y-=0.5f;
            if (vau.unitIndex == 0)
            {
                //add nativeTower
				vau.towerPos.y-=0.5f;
                BuildingCell towerCell = new BuildingCell();
                towerCell.cellPos = vau.towerPos;
                towerCell.cellRot = 0;
                int towerBuildingNo = VArtifactTownConstant.NATIVE_TOWER_BUILDING_ID;
                Vector3 towerPos = VArtifactUtil.GetPosAfterRotation(vau, towerCell.cellPos);
                float towerRot = towerCell.cellRot + vau.rot;
                BuildingID towerBuildingId = new BuildingID(vau.vat.townId, towerBuildingNo);

                Vector2 size = new Vector2(5, 5);

                VABuildingInfo towerInfo = new VABuildingInfo(towerPos, towerRot, 0, towerBuildingId, VABuildingType.Prefeb, vau,size);
                towerInfo.pathID = vau.vat.nativeTower.pathID;
				towerInfo.campID = vau.vat.nativeTower.campID;
				towerInfo.damageID = vau.vat.nativeTower.damageID;
                VABuildingManager.Instance.AddBuilding(towerInfo);
                vau.buildingPosID[towerPos] = towerBuildingId;
            }

            int cellCount = vau.buildingCell.Count();
            int startCellCount = 0;

            for (int i = startCellCount; i < cellCount; i++)
            {
                int idListIndex = i - startCellCount;
                if (buildingIdList[idListIndex] == -1)
                {
                    continue;
                }
                int bid = buildingIdList[idListIndex];
                if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(bid))
                {
                    LogManager.Error("bid = [", bid, "] not exist in database!");
                    return;
                }

                BuildingCell vaubc = vau.buildingCell[i];
				//correction
				vaubc.cellPos.y-=0.5f;
                Vector3 buildingPos = VArtifactUtil.GetPosAfterRotation(vau, vaubc.cellPos);
                float buildingRot = vaubc.cellRot + vau.rot;
                int buildingNo = vau.vat.buildingNo++;
                BuildingID buildingId = new BuildingID(vau.vat.townId, buildingNo);

                Vector2 size = new Vector2(20, 20);
                BlockBuilding bb = BlockBuilding.s_tblBlockBuildingMap[bid];
                size = bb.mSize;

                VABuildingInfo bdInfo = new VABuildingInfo(buildingPos, buildingRot, bid, buildingId, VABuildingType.Prefeb, vau,size);
                VABuildingManager.Instance.AddBuilding(bdInfo);
                vau.buildingPosID[buildingPos] = buildingId;
            }
        }
    }


    public void CreateTownNPCAndNative(VArtifactUnit vau, List<int> npcIdList)
    {
        if (VArtifactType.NpcTown == vau.type)
        {
            List<Vector3> npcPos = vau.npcPos;
            int startCount = 0;
            if (vau.vat.templateId == -1 && vau.unitIndex == 0)
            {
				if(PeGameMgr.IsSingle||PeGameMgr.IsMultiAdventureCoop)
				{
	                Vector3 npcWorldPos = VArtifactUtil.GetPosAfterRotation(vau, npcPos[0]) + new Vector3(0, 5, 0);
	                int terrainHeight = VFDataRTGen.GetPosHeight(npcWorldPos.x, npcWorldPos.z);
	                if (terrainHeight >= npcWorldPos.y - 1)
	                {
	                    npcWorldPos.y = terrainHeight + 1;
	                }
	                
	                if (!VATownNpcManager.Instance.IsCreated(npcWorldPos))
	                {
	                    VATownNpcInfo townNpcInfo = new VATownNpcInfo(npcWorldPos, missionStartNpcID);
						townNpcInfo.townId = vau.vat.townId;
	                    VATownNpcManager.Instance.AddNpc(townNpcInfo);
	                    vau.npcPosInfo.Add(npcWorldPos, townNpcInfo);
	                }
				}
                startCount++;
            }

            for (int i = startCount; i < npcPos.Count; i++)
            {
                int idListIndex = i - startCount;
                if (npcIdList[idListIndex] == -1)
                {
                    continue;
                }
                
                Vector3 npcWorldPos = VArtifactUtil.GetPosAfterRotation(vau, npcPos[i]) + new Vector3(0, 5, 0);
                int terrainHeight = VFDataRTGen.GetPosHeight(npcWorldPos.x,npcWorldPos.z);
                if (terrainHeight >= npcWorldPos.y - 1)
                {
                    npcWorldPos.y = terrainHeight + 1;
                }
                
				if (!VATownNpcManager.Instance.IsCreated(npcWorldPos)|| !vau.vat.IsPlayerTown)
                {
                    VATownNpcInfo townNpcInfo = new VATownNpcInfo(npcWorldPos, npcIdList[idListIndex]);
					townNpcInfo.townId = vau.vat.townId;
                    VATownNpcManager.Instance.AddNpc(townNpcInfo);
                    vau.npcPosInfo.Add(npcWorldPos, townNpcInfo);
                }
            }
        }
        else { 
            //--to do: native
            List<Vector3> npcPos = vau.npcPos;
            for (int i = 0; i < npcPos.Count; i++)
            {
                if ((PeGameMgr.IsSingleAdventure || GameConfig.IsMultiMode)
                        //&& !NativePointManager.Instance.IsCreated(nativePosXZ)
                        )
                {
					if (npcIdList[i] == -1)
					{
						continue;
					}
					Vector3 npcWorldPos = VArtifactUtil.GetPosAfterRotation(vau, npcPos[i]) + new Vector3(0, 5, 0);
                    int terrainHeight = VFDataRTGen.GetPosHeight(npcWorldPos.x, npcWorldPos.z);
                    if (terrainHeight >= npcWorldPos.y - 1)
                    {
                        npcWorldPos.y = terrainHeight+1;
                    }
                
                    NativePointInfo nativePointInfo = new NativePointInfo(npcWorldPos, npcIdList[i]);
//					NativePointInfo nativePointInfo = new NativePointInfo(npcWorldPos, 102);//for test
                    nativePointInfo.townId = vau.vat.townId;
                    VANativePointManager.Instance.AddNative(nativePointInfo);
                    vau.nativePointInfo.Add(npcWorldPos, nativePointInfo);
                    //LogManager.Debug(nid + " " + npcPosXZ);
                }
            }


        }
    }



    public void ArtifactAddToRender(VArtifactUnit vau, IntVector2 tileIndex)
    {
        if (vau.isAddedToRender)
        {
            return;
        }
		AddToRenderReady(vau,tileIndex.x,tileIndex.y);
        vau.isAddedToRender = true;
    }

    public void AddToRenderReady(VArtifactUnit vau, int x,int z)
    {
        //1.active by delegate from terrain--before
//		Debug.Log("AddToRenderReady townId:"+vau.vat.townId+" vau.worldPos:"+vau.worldPos+" tileIndex:"+tileIndex);
//        float renderY = vau.worldPos.y;
//        for (int i = Mathf.FloorToInt(renderY)-32; i <= VFDataRTGen.s_noiseHeight-1; i += 32)
//        {
//            Vector3 pos = new Vector3(tileIndex.x << VoxelTerrainConstants._shift, i, tileIndex.y << VoxelTerrainConstants._shift);
//            if (LODOctreeMan.self != null)
//                LODOctreeMan.self.AttachNodeEvents(null, null, VArtifactTownManager.Instance.RenderReady, null, null, pos);
//        }
		lock(renderTownList){
			renderTownList.Add(new IntVector2 (x,z));
		}
    }



	public void RenderReady(IntVector2 tileIndexXZ)
	{
//		IntVector2 tileIndexXZ = new IntVector2(nodePosLod.x >> VoxelTerrainConstants._shift, nodePosLod.z >> VoxelTerrainConstants._shift);
		//Debug.Log("start RenderReady tileIndexXZ:"+tileIndexXZ);
        if (!TileContainsTown(tileIndexXZ))
        {
            //Debug.LogError("tileIndexXZ no artifact!"+tileIndexXZ);
            return;
        }
        if (!renderReadyList.Contains(tileIndexXZ))
        {
			renderReadyList.Add(tileIndexXZ);
			//Debug.Log("RenderReady renderReadyList:"+tileIndexXZ);
            StartCoroutine(RenderAllWaitMainPlayer(tileIndexXZ));
        }
		
		//Debug.Log("end RenderReady tileIndexXZ:"+tileIndexXZ);
    }
    IEnumerator RenderAllWaitMainPlayer(IntVector2 tileIndexXZ){
        while (PeCreature.Instance.mainPlayer == null||PeLauncher.Instance.isLoading)
        {
            yield return null;
		}
		//Debug.Log("start RenderAllWaitMainPlayer tileIndexXZ:"+tileIndexXZ);
        List<VArtifactUnit> vauList = GetTileUnitList(tileIndexXZ);
        for (int val = 0; val < vauList.Count; val++)
        {
            foreach (VArtifactUnit vau in vauList[val].vat.VAUnits)
            {
				RenderArtifactAllContent(vau);
				vau.isDoodadNpcRendered = true;
            }
                

            if (!vauList[val].vat.IsExplored)
            {
                AddTown(vauList[val].vat);
                vauList[val].vat.IsExplored = true;
            }

        }
        renderReadyList.Remove(tileIndexXZ);
		
		//Debug.Log("end RenderAllWaitMainPlayer tileIndexXZ:"+tileIndexXZ);
    }
    public void RenderArtifactAllContent(VArtifactUnit vau)
    {
		//Debug.Log("start RenderArtifactAllContent town:"+vau.vat.townId);
		if(vau.vat.isEmpty)
			return;
        if (!vau.vat.isRandomed)
        {
            RandomArtifactTown(vau.vat);
		}
		List<BuildingID> buildingIdList = vau.buildingPosID.Values.ToList();
		for (int i = 0; i < buildingIdList.Count; i++)
		{
			BuildingID buildingId = buildingIdList[i];
			VABuildingManager.Instance.RenderBuilding(buildingId);
		}
        if (vau.type == VArtifactType.NpcTown)
		{
			if((vau.vat.IsPlayerTown&&!vau.isDoodadNpcRendered)
			   ||(!vau.vat.IsPlayerTown&&!capturedCampId.ContainsKey(vau.vat.townId)))
			{
				List<VATownNpcInfo> npcs = vau.npcPosInfo.Values.ToList();
	            for (int i = 0; i < npcs.Count; i++)
	            {
	                VATownNpcInfo npcInfo = npcs[i];
	                VATownNpcManager.Instance.RenderTownNPC(npcInfo);
	            }
			}
        }
        else
        {
            if (!capturedCampId.ContainsKey(vau.vat.townId))
            {
                //when try it every time
                List<NativePointInfo> nativePointInfoList = vau.nativePointInfo.Values.ToList() ;
                for (int i = 0; i < nativePointInfoList.Count; i++)
                {
                    NativePointInfo nativePointInfo = nativePointInfoList[i];
                    VANativePointManager.Instance.RenderNative(nativePointInfo);
                }
            }

//            if (PeGameMgr.IsSingleAdventure)
//            {
//                vau.buildingPosID.Clear();
//                vau.nativePointInfo.Clear();
//            }
//            else
//            {
//                Vector3 nativeTowerPos= Vector3.zero;
//                BuildingID nativeTowerBuildingId=null;
//                foreach (KeyValuePair<Vector3, BuildingID> tempKvp in vau.buildingPosID)
//                {
//                    if (tempKvp.Value.buildingNo != VArtifactTownConstant.NATIVE_TOWER_BUILDING_ID)
//                    {
//                        nativeTowerPos = tempKvp.Key;
//                        nativeTowerBuildingId = tempKvp.Value;
//                        break;
//                    }
//                }
//                vau.buildingPosID.Clear();
//                if (nativeTowerBuildingId != null)
//                {
//                    vau.buildingPosID.Add(nativeTowerPos, nativeTowerBuildingId);
//                }
//            }
		}
		vau.buildingPosID.Clear();
		vau.npcPosInfo.Clear();
		vau.nativePointInfo.Clear();
		//Debug.Log("end RenderArtifactAllContent town:"+vau.vat.townId);
    }

    //public void SetCreatedTown(List<int> townId)
    //{
    //    //mCreatedTownId = new Dictionary<int, int>();
    //    for (int i = 0; i < townId.Count; i++)
    //    {
    //        if (!mCreatedTownId.ContainsKey(townId[i]))
    //        {
    //            mCreatedTownId.Add(townId[i], 0);
    //        }
    //    }
    //}
    public void SetCapturedCamp(List<int> townId)
    {
        //mCreatedTownId = new Dictionary<int, int>();
        for (int i = 0; i < townId.Count; i++)
        {
            if (!capturedCampId.ContainsKey(townId[i]))
            {
                capturedCampId.Add(townId[i], 0);
            }
        }
    }

    public List<VArtifactTown> GetLevelTowns(int level)
    {
        if (!levelNpcTown.ContainsKey(level))
        {
            return null;
        }
        return levelNpcTown[level];
    }
	public List<VArtifactTown> GetLevelMainTowns(int level)
	{
		List<VArtifactTown> levelMainTownList=new List<VArtifactTown> ();
		if (levelNpcTown.ContainsKey(level))
		{
			foreach(VArtifactTown vat in levelNpcTown[level])
			{
				if(vat.isMainTown)
					levelMainTownList.Add(vat);
			}
		}
		return levelMainTownList;
	}
    public VArtifactTown GetTownByID(int id)
    {
        if (!townIdData.ContainsKey(id))
        {
            return null;
        }
        return townIdData[id];
    }

    public void SetCaptured(VArtifactTown townInfo)
    {
        if (!capturedCampId.ContainsKey(townInfo.townId))
            capturedCampId.Add(townInfo.townId, 0);
    }

    public void SetCaptured(int townId)
    {
        Debug.Log("SetCaptured");
//        if (PeGameMgr.IsSingleAdventure)
//        {
            if (!capturedCampId.ContainsKey(townId))
                capturedCampId.Add(townId, 0);
//        }
//        else if (PeGameMgr.IsMultiAdventure)
//        {
//            Debug.Log("RPC_C2S_DestroyNativeTower:" + townId);
//            NetworkManager.SyncServer(EPacketType.PT_Common_NativeTowerDestroyed, townId);
//        }
    }

    public bool IsCaptured(int campId)
    {
        return capturedCampId.ContainsKey(campId);
    }

    public void SetTownDistance(float scale)
    {
        //townDistanceX = (int)(VartifactTownXmlInfo.distanceX * scale);
        //townDistanceZ = (int)(VartifactTownXmlInfo.distanceZ * scale);
    }

    public void PrintTownPos()
    {
        List<VArtifactTown> allTown = new List<VArtifactTown>();
        allTown.AddRange(townPosInfo.Values);
        for (int i = 0; i < allTown.Count; i++)
        {
            VArtifactTown t = allTown[i];
            if (t.VAUnits[0].type == VArtifactType.NpcTown)
            {
				Debug.Log("<color=#007440FF>" + t.VAUnits[0].type.ToString() + " start:" + t.PosStart + " id:" + t.townId + " tid:" + t.templateId + " level:" + t.level + " ally:"+t.allyId+ " isEmpty:"+t.isEmpty.ToString()+"</color>");
            }
            else
            {
				Debug.Log("<color=#AA00EAFF>"+"|----" + t.VAUnits[0].type.ToString() + " start:" + t.PosCenter + " id:" + t.townId + " cid:" + t.templateId + " level:" + t.level+" ally:"+t.allyId+" isEmpty:"+t.isEmpty.ToString()+"</color>");
            }
        }
    }

    public bool IsTownChunk(IntVector2 tileIndex)
    {
        if (TileContainsTown(tileIndex))
        {
            return true;
        }
        return false;
    }

    public float GetTownCenterByTileAndPos(IntVector2 tileIndex, IntVector2 worldXZ)
    {
        List<VArtifactUnit> VAUnitList = GetTileUnitList(tileIndex);
        if (VAUnitList == null)
            return 0;
        for (int i = 0; i < VAUnitList.Count; i++)
        {
            VArtifactUnit vau = VAUnitList[i];

            IntVector2 posStart = vau.PosStart;
            IntVector2 posEnd = vau.PosEnd;
            if (worldXZ.x >= posStart.x && worldXZ.y >= posStart.y && worldXZ.x <= posEnd.x && worldXZ.y <= posEnd.y)
            {
                return vau.vaSize.z+vau.worldPos.y;
            }
        }
        return 0;
    }


    public void AddTown(VArtifactTown vat)
    {

        if (!PeGameMgr.IsMulti)
        {
			DetectTowns(vat);
        }

        if (vat.type == VArtifactType.NpcTown)
        {
            if (PeGameMgr.IsMulti)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_TownArea, vat.TransPos);
            }
            else if (PeGameMgr.IsSingleAdventure)
            {
                DetectedTownMgr.Instance.AddDetectedTown(vat);
                RandomMapIconMgr.AddTownIcon(vat);
            }
        }
        else
        {
            if (PeGameMgr.IsMulti)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_CampArea, vat.TransPos);
            }
            else if (PeGameMgr.IsSingleAdventure)
            {
                RandomMapIconMgr.AddNativeIcon(vat);
            }
        }
        if (!mDetectedTowns.Contains(vat.PosCenter))
        {
            mDetectedTowns.Add(vat.PosCenter);
        }
    }

    public void RestoreTownIcon()
    {
		foreach(KeyValuePair<int,List<int>> townIdBuildings in mAliveBuildings)
		{
			VArtifactTown vat = GetTownByID(townIdBuildings.Key);
			if(vat!=null)
			{
				if(townIdBuildings.Value.Count==0){
					RandomMapIconMgr.AddDestroyedTownIcon(vat);
					capturedCampId.Add(vat.townId,0);
				}
				else if (vat.type == VArtifactType.NpcTown)
				{
					DetectedTownMgr.Instance.AddDetectedTown(vat);
					RandomMapIconMgr.AddTownIcon(vat);
				}
				else{
					RandomMapIconMgr.AddNativeIcon(vat);
				}

				mDetectedTowns.Add(vat.PosCenter);
				foreach (VArtifactUnit vau in vat.VAUnits)
				{
					vau.isDoodadNpcRendered = true;
				}
				vat.IsExplored = true;
			}
		}
    }

	public void DetectTowns(VArtifactTown centerVat)
    {
		IntVector2 townCenter = centerVat.PosCenter;
        IntVector2 indexCenter = new IntVector2(townCenter.x >> VoxelTerrainConstants._shift, townCenter.y >> VoxelTerrainConstants._shift);
        for (int i = indexCenter.x - detectedChunkNum; i < indexCenter.x + detectedChunkNum + 1; i++)
        {
            for (int j = indexCenter.y - detectedChunkNum; j < indexCenter.y + detectedChunkNum + 1; j++)
            {
                IntVector2 tileIndex = new IntVector2(i, j);

                if (indexCenter.Equals(tileIndex))
                {
                    continue;
                }

                if (TileContainsTown(tileIndex))
                {
					VArtifactTown vat = GetTileTown(tileIndex);
					if(centerVat.areaId!=vat.areaId)
						continue;
					Vector3 markPos = vat.TransPos;
                    if (!vat.IsExplored && !vat.PosCenter.Equals(townCenter)&& !vat.isEmpty)
                    {
						if (!RandomMapIconMgr.HasTownLabel(markPos))
                            UnknownLabel.AddMark(markPos);
                        //if (!unknownMask.ContainsKey(centerPos))
                        //{ 
                        //    unknownMask.Add(centerPos,GameGui_N.Instance.mLimitWorldMapGui.AddUnKownMask(new Vector3(centerPos.x, 60, centerPos.y)));
                        //    Debug.Log("UnKnown pos:" + centerPos.x + " " + centerPos.y);
                        //}
                        //GameGui_N.Instance.mLimitWorldMapGui.AddUnKownMask(vat.PosCenter);
                        //Debug.Log("UnKnown pos:" + centerPos);
                    }
                    else
                    {
                        UnknownLabel.Remove(markPos);
                        //if (GameGui_N.Instance.mLimitWorldMapGui.DeleteUnKownMask(vat.PosCenter))
                        //{
                        //    //Debug.Log("Destroy unKnown pos:" + centerPos);
                        //}
                    }
                }
            }
		}
        if (townPosInfo.ContainsKey(townCenter))
        {
            VArtifactTown thisTown = townPosInfo[townCenter];
            if (thisTown != null)
            {
                thisTown.IsExplored = true;
            }
        }
	}

	public void DetectTownsAround(){
		if(PeCreature.Instance.mainPlayer!=null)
		{
			Vector3 detectCenter = PeCreature.Instance.mainPlayer.position;
			IntVector2 indexCenter = new IntVector2((int)detectCenter.x >> VoxelTerrainConstants._shift, (int)detectCenter.z >> VoxelTerrainConstants._shift);
			for (int i = indexCenter.x - detectedChunkAroundNum; i < indexCenter.x + detectedChunkAroundNum + 1; i++)
			{
				for (int j = indexCenter.y - detectedChunkAroundNum; j < indexCenter.y + detectedChunkAroundNum + 1; j++)
				{ 
					IntVector2 tileIndex = new IntVector2(i, j);
					if (TileContainsTown(tileIndex))
					{
						VArtifactTown vat = GetTileTown(tileIndex);
						if(VATownGenerator.Instance.GetAreaIdByRealPos(new IntVector2 (Mathf.RoundToInt(detectCenter.x),Mathf.RoundToInt(detectCenter.y)))!=
						   vat.areaId)
							continue;
						Vector3 markPos = vat.TransPos;
						if (!vat.IsExplored&&!vat.isEmpty)
						{
							if(!RandomMapIconMgr.HasTownLabel(markPos))
								UnknownLabel.AddMark(markPos);
						}
					}
				}
			}
		}
	}


    #region record
    public void Import(byte[] buffer)
	{
        if (buffer.Length == 0)
            return;

        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);
		_in.ReadInt32();
      
		int vatSaveDataCount = _in.ReadInt32();
		for(int i= 0;i<vatSaveDataCount;i++){
			VATSaveData vs = new VATSaveData ();
			vs.townId = _in.ReadInt32();
			vs.ms_id = _in.ReadInt32();;
			vs.lastHour = _in.ReadDouble();
			vs.nextHour = _in.ReadDouble();
			mVATSaveData.Add(vs.townId,vs);
		}
		InitVATData();
		int buildingTownCount = _in.ReadInt32();
		for(int i=0;i<buildingTownCount;i++){
			int townId = _in.ReadInt32 ();
			mAliveBuildings.Add(townId,new List<int> ());
			int bCount = _in.ReadInt32();
			for(int j=0;j<bCount;j++){
				mAliveBuildings[townId].Add(_in.ReadInt32());
			}
		}
		RestoreTownIcon();

        _in.Close();
        ms.Close();
    }

	public void Export(BinaryWriter bw)
    {
		bw.Write(currentVersion);
        
		bw.Write(mVATSaveData.Keys.Count);
		foreach(int townId in mVATSaveData.Keys){
			bw.Write(townId);
			bw.Write(mVATSaveData[townId].ms_id);
			bw.Write(mVATSaveData[townId].lastHour);
			bw.Write(mVATSaveData[townId].nextHour);
		}

		bw.Write(mAliveBuildings.Keys.Count);
		foreach(int townId in mAliveBuildings.Keys){
			bw.Write(townId);
			bw.Write(mAliveBuildings[townId].Count);
			foreach(int entityID in mAliveBuildings[townId]){
				bw.Write(entityID);
			}
		}
    }
    #endregion

    public bool TileContainsTown(IntVector2 tileIndex)
    {
        return townTile.ContainsKey(tileIndex);
    }

    public List<VArtifactUnit> GetTileUnitList(IntVector2 tileIndex)
    {
        if (townTile.ContainsKey(tileIndex))
        {
            return townTile[tileIndex].unitList;
        }
        else
            return null;
    }


    public VArtifactTown GetTileTown(IntVector2 tileIndex)
    {
        if (townTile.ContainsKey(tileIndex))
            return townTile[tileIndex].town;
        else
            return null;
    }

    public VArtifactTown GetTown(Vector3 position)
    {
        return null;
    }

    //public void AddAllTownMapForTest()
    //{
    //    if(townPosInfo!=null && townPosInfo.Count>0)
    //        foreach (VArtifactTown vat in townPosInfo.Values)
    //        {
    //            if (vat.Type == VArtifactType.NpcTown)
    //            {
    //                WorldMapManager.Instance.AddTown(vat.townId, vat.TransPos);
    //            }              
    //            else
    //            {
    //                WorldMapManager.Instance.AddCamp(vat.TransPos);
    //            }
    //        }
    //}


    static void ClearRandomTownSystem()
    {
        Instance.Clear();
        VABuildingManager.Instance.Clear();
        VATownNpcManager.Instance.Clear();
    }

	public static IEnumerator WaitForArtifactTown(int[] capturedCampId)
	{
		while (null == Instance || null == VABuildingManager.Instance || null == VATownNpcManager.Instance)
			yield return null;

		//ClearRandomTownSystem();

		if (null != capturedCampId)
			Instance.SetCapturedCamp(capturedCampId.ToList());
	}

    #region Action Callback APIs
    public static void RPC_S2C_NativeTowerDestroyed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int campId;
        stream.TryRead<int>(out campId);
        if (!Instance.capturedCampId.ContainsKey(campId))
            Instance.capturedCampId.Add(campId, 0);
    }
    #endregion

    #region init
    void InitParam(){
        switch (RandomMapConfig.mapSize)
        {
            case 0:
                ////boundary
                //SetBoundary(-20000, 20000, -20000, 20000, 200);
                ////VFDataRTGen mineral
                //VFDataRTGen.MetalReduceSwitch = true;
                //VFDataRTGen.MetalReduceArea = 4000;
                ////VFDataRTGen.MineFrequency0 = 0.5;
                ////VFDataRTGen.MineFrequency1 = 2;

                //VFDataRTGen.SetMapTypeFrequency(1f);
                ////VArtifactTownManager town level campDistance detectarea
                SetTownBoundary(-19200, 19200, -19200, 19200);
                VArtifactTownManager.Instance.LevelRadius = 4000;
                SetTownDistance(1, 1);
                VArtifactTownManager.Instance.DetectedChunkNum = 32;
                break;
            case 1:
                ////boundary
                //SetBoundary(-10000, 10000, -10000, 10000, 100);
                ////VFDataRTGen mineral
                //VFDataRTGen.MetalReduceSwitch = true;
                //VFDataRTGen.MetalReduceArea = 2000;
                ////VFDataRTGen.MineFrequency0 = 0.5;
                ////VFDataRTGen.MineFrequency1 = 2;

                //VFDataRTGen.SetMapTypeFrequency(1f);
                //VArtifactTownManager town level
                SetTownBoundary(-9600, 9600, -9600, 9600);
                VArtifactTownManager.Instance.LevelRadius = 2000;
                SetTownDistance(1, 1);
                VArtifactTownManager.Instance.DetectedChunkNum = 32;
                break;
            case 2:
                ////boundary
                //SetBoundary(-4000, 4000, -4000, 4000, 40);
                ////VFDataRTGen mineral
                //VFDataRTGen.MetalReduceSwitch = false;
                ////VFDataRTGen.MineFrequency0 = 0.25;
                ////VFDataRTGen.MineFrequency1 = 0.25;
                ////maptype
                //VFDataRTGen.SetMapTypeFrequency(1.5f);
                ////VFDataRTGen.SetTerrainFrequency(1.5f);
                //VArtifactTownManager town level campDistance detectarea
                SetTownBoundary(-3860, 3860, -3860, 3860);
                VArtifactTownManager.Instance.LevelRadius = 800;
                SetTownDistance(0.5f, 0.8f);
                VArtifactTownManager.Instance.DetectedChunkNum = 16;
                break;
            case 3:
                ////boundary
                //SetBoundary(-2000, 2000, -2000, 2000, 20);
                ////VFDataRTGen mineral
                //VFDataRTGen.MetalReduceSwitch = false;
                ////VFDataRTGen.MineFrequency0 = 1;
                ////VFDataRTGen.MineFrequency1 = 2;
                //VFDataRTGen.SetMapTypeFrequency(3f);
                ////VFDataRTGen.SetTerrainFrequency(3f);
                //VArtifactTownManager town level campDistance detectarea
                SetTownBoundary(-1920, 1920, -1920, 1920);
                VArtifactTownManager.Instance.LevelRadius = 400;
                SetTownDistance(0.5f, 0.8f);
                VArtifactTownManager.Instance.DetectedChunkNum = 12;
                break;
            case 4:
                ////boundary
                //SetBoundary(-1000, 1000, -1000, 1000, 10);
                ////VFDataRTGen mineral
                //VFDataRTGen.MetalReduceSwitch = false;
                ////VFDataRTGen.MineFrequency0 = 1;
                ////VFDataRTGen.MineFrequency1 = 2;
                //VFDataRTGen.SetMapTypeFrequency(3f);
                ////VFDataRTGen.SetTerrainFrequency(3f);
                //VArtifactTownManager town level campDistance detectarea
                SetTownBoundary(-960, 960, -960, 960);
                VArtifactTownManager.Instance.LevelRadius = 200;
                SetTownDistance(0.25f, 0.8f);
                VArtifactTownManager.Instance.DetectedChunkNum = 6;
                break;
        }
    }
    public void SetTownBoundary(int minX, int maxX, int minZ, int maxZ)
    {
        VArtifactTownManager.Instance.MinX = minX;
        VArtifactTownManager.Instance.MaxX = maxX;
        VArtifactTownManager.Instance.MinZ = minZ;
        VArtifactTownManager.Instance.MaxZ = maxZ;
    }

    public void SetTownDistance(float townDistanceScale, float campDistanceScale)
    {
        VArtifactTownManager.Instance.SetTownDistance(townDistanceScale);
        VANativeCampManager.Instance.SetCampDistance(campDistanceScale);
    }
    #endregion

	#region townDestroy
	public void AddAliveBuilding(int townId,int entityId){
		if(!mAliveBuildings.ContainsKey(townId))
			mAliveBuildings.Add(townId,new List<int> ());
		mAliveBuildings[townId].Add(entityId);
	}

	public void OnTownDestroyed(VArtifactTown vat){
		//1.changetownIcon
		RandomMapIconMgr.DestroyTownIcon(vat);
		Debug.Log("OnTownDestroyed id:"+vat.townId);
		SetCaptured(vat.townId);
		RemoveNativePointAgent(vat.townId);
		DetectedTownMgr.Instance.RemoveDetectedTown(vat);
		if(TownDestroyedEvent!=null)
		{
			TownDestroyedEvent(vat.AllyId);
		}
	}

	public void OnTownDestroyed(int townId){
		//1.changetownIcon
		VArtifactTown vat = GetTownByID(townId);
		RandomMapIconMgr.DestroyTownIcon(vat);
		Debug.Log("OnTownDestroyed id:"+vat.townId);
		SetCaptured(townId);
		RemoveNativePointAgent(townId);
		DetectedTownMgr.Instance.RemoveDetectedTown(vat);
		if(TownDestroyedEvent!=null)
		{
			TownDestroyedEvent(vat.AllyId);
		}
		//2.--to do
	}

	public void OnBuildingDeath(int townId,int entityId,bool isSignalTower){
		Debug.Log("OnBuildingDeath id:"+townId+" ,"+entityId);
		if(mAliveBuildings.ContainsKey(townId)){
			List<int> buildings = mAliveBuildings[townId];
			buildings.Remove(entityId);
			if(buildings.Count==0)
				OnTownDestroyed(townId);
		}
	}

	public void AddMonsterPointAgent(int townId,SceneEntityPosAgent nativeAgent){
		if(!MonsterPointAgentDic.ContainsKey(townId))
			MonsterPointAgentDic.Add(townId,new List<SceneEntityPosAgent> ());
		MonsterPointAgentDic[townId].Add(nativeAgent);
	}
	public void RemoveNativePointAgent(int townId){
		if(MonsterPointAgentDic.ContainsKey(townId)){
			foreach(SceneEntityPosAgent agent in MonsterPointAgentDic[townId]){
				if((agent.protoId & EntityProto.IdGrpMask) != 0)
				{
					(agent.entity as EntityGrp).RemoveAllAgent();
				}
				SceneMan.RemoveSceneObj(agent);
			}
			MonsterPointAgentDic.Remove (townId);
		}
	}
	#endregion
}
