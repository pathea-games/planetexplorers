//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;
using Pathea;
using DunGen;
using System.Collections.Generic;
using ItemAsset;
using DunGen.Graph;
using System.Linq;
//using UnityEditor;

public class RandomDungenMgr:MonoBehaviour
{

	static RandomDungenMgr mInstance;

	public static RandomDungenMgr Instance{
		get{return mInstance;}
	}

	void Awake(){
		mInstance=this;
	}

	void Start(){

	}

	//[AddComponentMenu("DunGen/Runtime Dungeon")]
//public DungeonGenerator generator = new DungeonGenerator ();
	const string dungeonFlowPath = "Prefab/Item/Rdungeon/DungeonFlow_Main";
	DungeonGenerator generator;

	
	public bool isActive = false;
	public GameObject manager;

	//entrance
	UnityEngine.Object entrancePrefab0;
	const string EntrancePath0 ="Prefab/RandomDunGen/RandomDunEntrance_Native";
	UnityEngine.Object entrancePrefab1;
	const string EntrancePath1 ="Prefab/RandomDunGen/RandomDunEntrance_Cave";
	GameObject dungeonWaterPrefab;
	const string dungeonWaterPath = "Prefab/RandomDunGen/DungeonWater";
	GameObject dungeonWater;
	public static List<Vector3> entrancesToAdd = new List<Vector3> (); 
	//bool isAdding = false;

	public void Init(){
		manager=gameObject;
		entrancePrefab0 = Resources.Load(EntrancePath0);
		entrancePrefab1 = Resources.Load(EntrancePath1);
		dungeonWaterPrefab= Resources.Load(dungeonWaterPath) as GameObject;
		allEntrance = new Dictionary<Vector3, DunEntranceObj>();
		entranceArea = new Dictionary<IntVector2,DunEntranceObj>();
	}
	public void CreateInitTaskEntrance(){
		if(RandomDungenMgrData.initTaskEntrance.Count>0)
			foreach(KeyValuePair<IntVector2,int> kvp in RandomDungenMgrData.initTaskEntrance)
				GenTaskEntrance(kvp.Key,kvp.Value);
	}

	public void EnterDungen(Vector3 entrancePos,int dungeonDataId){
		RandomDungenMgrData.Clear();
		MissionManager.Instance.m_PlayerMission.AbortFollowMission();
		RandomDungenMgrData.AddServants();
		LoadDataFromId(dungeonDataId);
		SetWaterType(entrancePos);
		if(PeGameMgr.IsMulti){
			MessageBox_N.ShowMaskBox(MsgInfoType.DungeonGeneratingMask,"Generating",20f);
			RandomDungenMgrData.entrancePos = entrancePos;
			RandomDungenMgrData.enterPos = PeCreature.Instance.mainPlayer.position;
			PlayerNetwork.mainPlayer.RequestEnterDungeon(entrancePos);
		}
		else{
			isActive =true;
			//UILoadScenceEffect.Instance.EnableProgress(false);
			MissionManager.Instance.yirdName = AdventureScene.Dungen.ToString();
			//origin
			RandomDungenMgrData.SetPosByEnterPos(entrancePos);
			MissionManager.Instance.transPoint = RandomDungenMgrData.revivePos;
			//test
//			MissionManager.Instance.transPoint = new Vector3(enterPos.x,-100,enterPos.z);
//			RandomDungenMgrData.RevivePos = MissionManager.Instance.transPoint;
//			RandomDungenMgrData.genPos = RandomDungenMgrData.RevivePos+new Vector3 (0,0,-2);
			//TransFollower(RandomDungenMgrData.enterPos);
			MissionManager.Instance.SceneTranslate();
		}
	}

	public void SaveInDungeon(){
		MissionManager.Instance.TransPlayerAndMissionFollower(RandomDungenMgrData.enterPos);
		TransFollower(RandomDungenMgrData.enterPos);
		SinglePlayerTypeLoader singlePlayerTypeLoader = SinglePlayerTypeArchiveMgr.Instance.singleScenario;
		singlePlayerTypeLoader.SetYirdName(AdventureScene.MainAdventure.ToString());
	}
	public void TransFromDungeon(Vector3 pos){
		MissionManager.Instance.TransMissionFollower(pos);
		TransFollower(pos);
		SinglePlayerTypeLoader singlePlayerTypeLoader = SinglePlayerTypeArchiveMgr.Instance.singleScenario;
		singlePlayerTypeLoader.SetYirdName(AdventureScene.MainAdventure.ToString());
	}
	public void TransBackToDungeon(Vector3 pos){
		PeTrans view = PeCreature.Instance.mainPlayer.peTrans;
		if (view == null)
			return;
		view.position = pos;
		MissionManager.Instance.TransMissionFollower(pos);
		TransFollower(pos);
		SinglePlayerTypeLoader singlePlayerTypeLoader = SinglePlayerTypeArchiveMgr.Instance.singleScenario;
		singlePlayerTypeLoader.SetYirdName(AdventureScene.Dungen.ToString());
	}
	public void TransFromDungeonMultiMode(Vector3 pos){
		MissionManager.Instance.TransMissionFollower(pos);
		TransFollower(pos);
	}
	public void TransFollower(Vector3 pos){
		List<PeEntity> follower = GetAllFollower();
		if(PeGameMgr.IsSingle){
			foreach(PeEntity pe in follower){
				pe.position = pos;
			}
		}
		else{
			foreach(PeEntity pe in follower){
				pe.position = pos;
			}
		}
	}
	List<PeEntity> GetAllFollower(){
		List<PeEntity> allFollowers = new List<PeEntity> ();
		NpcCmpt[] servants = PeCreature.Instance.mainPlayer.GetComponent<ServantLeaderCmpt>().GetServants();
		foreach (NpcCmpt nc in servants)
		{
			if (nc != null)
				allFollowers.Add(nc.Entity);
		}
		return allFollowers;
	}

	public void ExitDungen(){
		if(PeGameMgr.IsMulti){
			if(allEntrance.ContainsKey(RandomDungenMgrData.entrancePos))
			{
				DunEntranceObj rde = allEntrance[RandomDungenMgrData.entrancePos];
				rde.ShowEnterOrNot = true;
			}
			PlayerNetwork.mainPlayer.RequestExitDungeon();
		}else{
			isActive =true;
			//UILoadScenceEffect.Instance.EnableProgress(false);
			MissionManager.Instance.yirdName = AdventureScene.MainAdventure.ToString();
			MissionManager.Instance.transPoint = RandomDungenMgrData.enterPos;
			//		Debug.Log(MissionManager.Instance.yirdName);
			//		GenDungen(PeCreature.Instance.mainPlayer.position+new Vector3 (0,20,0));
			//		PeCreature.Instance.mainPlayer.position = manager.transform.position+new Vector3(0,2,0);
			TransFollower(RandomDungenMgrData.enterPos);
			DestroyDungeon();
			MissionManager.Instance.SceneTranslate();
		}
	}

    public void LoadPathFinding()
    {
        StartCoroutine(LoadPathFinder());
    }

    public void ResetPathFinding()
    {
        StartCoroutine(ResetPathFinder());
    }

    IEnumerator ResetPathFinder()
    {
        if (AstarPath.active != null)
        {
            if (AstarPath.active.transform.parent != null)
                GameObject.Destroy(AstarPath.active.transform.parent.gameObject);
            else
                GameObject.Destroy(AstarPath.active.gameObject);
        }

        yield return null;

        GameObject obj = Resources.Load("Prefab/PathfinderStd") as GameObject;
        if (obj != null)
        {
            Instantiate(obj);
        }

        yield return null;
		long tickStart = System.DateTime.UtcNow.Ticks; 
		if(AstarPath.active != null)
		{
	        AstarPath.active.Scan();
	        Debug.Log("AstarPath.active.Scan(): " + (System.DateTime.UtcNow.Ticks - tickStart) / 10000L + "ms");
		}
    }

    IEnumerator LoadPathFinder()
    {
        if(AstarPath.active != null)
        {
            if (AstarPath.active.transform.parent != null)
                GameObject.Destroy(AstarPath.active.transform.parent.gameObject);
            else
                GameObject.Destroy(AstarPath.active.gameObject);
        }

        yield return null;

		GameObject obj = Resources.Load("Prefab/Pathfinder_Dungeon") as GameObject;
        if (obj != null)
        {
            Instantiate(obj);
        }

        yield return null;
		long tickStart = System.DateTime.UtcNow.Ticks;
		if(AstarPath.active != null){
        	AstarPath.active.Scan();
			Debug.Log("AstarPath.active.Scan(): "+(System.DateTime.UtcNow.Ticks-tickStart) / 10000L +"ms");
		}
    }

	public bool GenDungeon(int seed =-1){
		if(PeGameMgr.IsSingle)
			RandomDungenMgrData.DungeonId++;

		generator = new DungeonGenerator(dungeonData.dungeonFlowPath);
		manager.transform.position = RandomDungenMgrData.genDunPos;
		if(manager==null||manager.transform==null)
		{
			Debug.LogError("manager==null||manager.transform==null!");
			return false;
		}
		bool genSuccess = false;
		if(seed==-1){
			genSuccess = generator.Generate(manager);//new
			if(!genSuccess)
				return false;
			Debug.Log("gen success without seed");
			GenWater_SafeBottom(RandomDungenMgrData.waterType);
			GeneralSet(true);
			GenContent();
		}
		else{
			genSuccess = generator.GenerateWithSeed(manager,seed);
			if(!genSuccess)
				return false;
			Debug.Log("gen success with seed");
			GenWater_SafeBottom(RandomDungenMgrData.waterType);
			GeneralSet(true);
		}
		if(PeGameMgr.IsSingle){

		}

		if(PeGameMgr.IsMulti){
			seed = generator.ChosenSeed;
			PlayerNetwork.mainPlayer.RequestUploadDungeonSeed(RandomDungenMgrData.entrancePos,seed);
			Debug.Log("dun Seed: "+seed);

			ChangeOther(true);
		}
		Debug.Log("RemoveTerrainDependence");
		SceneMan.RemoveTerrainDependence();
		return genSuccess;
	}

	public void GenContent(){
		int seed = (int)(System.DateTime.UtcNow.Ticks%Int32.MaxValue);
		System.Random rand = new System.Random(seed);
		RandomDungenMgrData.allMonsters = manager.GetComponentsInChildren<MonsterGenerator>().ToList();
//		foreach(MonsterGenerator mgm in RandomDungenMgrData.allMonsters){
//			MonsterGenerator.GenMonsters(mgm);
//		}
		GenMonsters(RandomDungenMgrData.allMonsters,rand);
		RandomDungenMgrData.allMinBoss = manager.GetComponentsInChildren<MinBossGenerator>().ToList();
		GenMinBoss(RandomDungenMgrData.allMinBoss,rand);
		RandomDungenMgrData.allBoss = manager.GetComponentsInChildren<BossMonsterGenerator>().ToList();
		GenBoss(RandomDungenMgrData.allBoss,rand);

		RandomDungenMgrData.allItems = manager.GetComponentsInChildren<DunItemGenerator>().ToList();
//		foreach(DunItemGeneratorMgr digm in RandomDungenMgrData.allItems){
//			DunItemGeneratorMgr.GenItem(digm);
//		}
		GenItems(RandomDungenMgrData.allItems,rand);

		RandomDungenMgrData.allRareItems = manager.GetComponentsInChildren<DunRareItemGenerator>().ToList();
		List<ItemIdCount> specifiedItems = RandomDungenMgrData.dungeonBaseData.specifiedItems;
		List<IdWeight> rareItemTags = RandomDungenMgrData.dungeonBaseData.rareItemTags;
		GenRareItem(RandomDungenMgrData.allRareItems,rareItemTags,rand,specifiedItems);
	}


	public void DestroyDungeon(){
		GeneralSet(false);
		SceneMan.AddTerrainDependence();
		if(generator!=null&&generator.CurrentDungeon!=null&&generator.CurrentDungeon.gameObject!=null)
			UnityUtil.Destroy(generator.CurrentDungeon.gameObject);
		RandomDungenMgrData.Clear();
        if (PeGameMgr.IsMulti)
        {
            ChangeOther(false);
            ResetPathFinding();
        }
		if(dungeonWater!=null)
			GameObject.Destroy(dungeonWater);
		FBSafePlane.instance.DeleteCol();
		DragItemAgent.DestroyAllInDungeon();
	}

	void GeneralSet(bool enter){
		if(enter)
		{
			if(PeGameMgr.IsSingle)
			{
				SaveOutOfDungeon();
			}
			PeEnv.CanRain(false);
			if(dungeonWater!=null)
				RandomMapConfig.SetGlobalFogHeight(dungeonWater.transform.position.y);
		}else{
			PeEnv.CanRain(true);
			RandomMapConfig.SetGlobalFogHeight();
		}
	}

	public void SaveOutOfDungeon(){
		PeTrans view = PeCreature.Instance.mainPlayer.peTrans;
		if (view == null)
			return;
		Vector3 pos = view.position;
		SaveInDungeon();
		ArchiveMgr.Instance.Save(ArchiveMgr.ESave.Auto1);
		TransBackToDungeon(pos);
	}
	//single
    void DeactiveObjNotUse()
    {
        List<int> followers = RandomDungenMgrData.GetAllFollowers();
        int mainPlayerId = PeCreature.Instance.mainPlayerId;
        IEnumerable<PeEntity> allEntities = EntityMgr.Instance.All;
        foreach(PeEntity pe in allEntities){
            if(pe.Id!=mainPlayerId&&!followers.Contains(pe.Id)){
				if(pe.gameObject!=null){
					pe.gameObject.SetActive(false);
					MapCmpt mc = pe.GetCmpt<MapCmpt>();
					pe.Remove(mc);
				}
			}
        }
		List<PeMap.ILabel> RemoveList = PeMap.LabelMgr.Instance.FindAll(itr => itr.GetType()==PeMap.ELabelType.Mission);
		foreach (PeMap.ILabel _ilabel in RemoveList)
		{
			PeMap.LabelMgr.Instance.Remove(_ilabel);
		}

    }


	//multimode
	public void ChangeOther(bool isEnter){
		UIMinMapCtrl.Instance.UpdateCameraPos();
		if(isEnter)
		{
			UIMinMapCtrl.Instance.CameraNear = 200;
			UIMinMapCtrl.Instance.CameraFar = 400;
		}else{
			UIMinMapCtrl.Instance.CameraNear = 1;
			UIMinMapCtrl.Instance.CameraFar=1000;
		}

	}

	public void ReceiveIsoObj(int dungeonId,ulong isoCode,int instanceId){
		//--to do: iosList
		//update randomRareItem
		RandomDungenMgrData.RareItemReady(instanceId,dungeonId);
	}

	public void OpenLockedDoor(SkillSystem.SkEntity caster,SkillSystem.SkEntity monster){
		LockedDoor ld = generator.CurrentDungeon.LockedDoorList.Find(it=>it.IsOpen==false);
		if(ld!=null)
			ld.Open();
	}

	public void PickUpKey(int keyId){
		RandomDungenMgrData.pickedKeys.Add(keyId);
	}

	public bool HasKey(int keyId){
		return RandomDungenMgrData.pickedKeys.Contains(keyId);
	}
	public void RemoveKey(int keyId){
		RandomDungenMgrData.pickedKeys.Remove(keyId);
	}

	#region LoadDataBase
	DungeonBaseData dungeonData{
		get{return RandomDungenMgrData.dungeonBaseData;}
		set{RandomDungenMgrData.dungeonBaseData = value;}
	}
	public bool IsInIronDungeon{
		get{return dungeonData.IsIron;}
	}
	public DungeonType Dungeontype{
		get{return dungeonData.Type;}
	}

	public void LoadDataFromDataBase(int level){
		DungeonBaseData dbd = RandomDungeonDataBase.GetDataFromLevel(level);
		DungeonFlow df =Resources.Load(dbd.dungeonFlowPath) as DungeonFlow;
		if(df==null){
			Debug.LogError("flow null: "+dbd.dungeonFlowPath );
			dbd.dungeonFlowPath = dungeonFlowPath;
		}
		dungeonData=dbd;
	}
	public void LoadDataFromId(int id){
		DungeonBaseData dbd = RandomDungeonDataBase.GetDataFromId(id);
		DungeonFlow df =Resources.Load(dbd.dungeonFlowPath) as DungeonFlow;
		if(df==null){
			Debug.LogError("flow null: "+dbd.dungeonFlowPath );
			dbd.dungeonFlowPath = dungeonFlowPath;
		}
		dungeonData=dbd;
	}
	public void GenMonsters(List<MonsterGenerator> allPoints,System.Random rand){
		//test
//		allPoints[0].GenerateMonster(dungeonData.landMonsterId,dungeonData.waterMonsterId,dungeonData.monsterBuff,rand);

		//origin
		if(allPoints==null||allPoints.Count==0)
			return;
		float jumpIndex = 1/dungeonData.monsterAmount;
		if(jumpIndex<1)
			jumpIndex=1;
		for(float i=0;i<allPoints.Count;i+=jumpIndex){
			int index = Mathf.FloorToInt(i);
			allPoints[index].GenerateMonster(dungeonData.landMonsterId,dungeonData.waterMonsterId,dungeonData.monsterBuff,rand,dungeonData.IsTaskDungeon);
		}
	}
	public void GenItems(List<DunItemGenerator> allPoints,System.Random rand){
		//test
//		allPoints[0].GenItem(dungeonData.itemId,rand);
		//origin
		if(allPoints==null||allPoints.Count==0)
			return;
		float jumpIndex = 1/dungeonData.itemAmount;
		if(jumpIndex<1)
			jumpIndex=1;
		for(float i=0;i<allPoints.Count;i+=jumpIndex){
			int index = Mathf.FloorToInt(i);
			allPoints[index].GenItem(dungeonData.itemId,rand);
		}
	}
	public void GenMinBoss(List<MinBossGenerator> allPoints,System.Random rand){
		if(allPoints==null||allPoints.Count==0)
			return;
		MinBossGenerator.GenAllBoss(allPoints,dungeonData.minBossId,dungeonData.minBossWaterId,dungeonData.minBossMonsterBuff,rand,dungeonData.IsTaskDungeon);
	}
	public void GenBoss(List<BossMonsterGenerator> allPoints,System.Random rand){
		if(allPoints==null||allPoints.Count==0)
			return;
		BossMonsterGenerator.GenAllBoss(allPoints,dungeonData.bossId,dungeonData.minBossWaterId,dungeonData.bossMonsterBuff,rand,dungeonData.IsTaskDungeon);
	}
	public void GenRareItem(List<DunRareItemGenerator> allPoints,List<IdWeight> rareItemTags,System.Random rand,List<ItemIdCount> specifiedItems=null){
		if(allPoints==null||allPoints.Count==0)
			return;
		DunRareItemGenerator.GenAllItem(allPoints,dungeonData.rareItemId,dungeonData.rareItemChance,rareItemTags,rand,specifiedItems);
	}
	public void SetWaterType(Vector3 entrancePos){
		if(dungeonData.IsTaskDungeon)
			RandomDungenMgrData.waterType = DungeonWaterType.None;
		else{
			if (VFVoxelWater.self != null)
			{
				if(VFVoxelWater.self.IsInWater(entrancePos)){
					//				if(VFVoxelWater.self.IsInWater(entrancePos+new Vector3(0,2,0)))
					RandomDungenMgrData.waterType = DungeonWaterType.Deep;
					//			   	else
					//					RandomDungenMgrData.waterType = DungeonWaterType.Shallow;
				}else{
					RandomDungenMgrData.waterType = DungeonWaterType.None;
				}
			}else{
				RandomDungenMgrData.waterType = DungeonWaterType.None;
			}
		}

	}

	public void GenWater_SafeBottom(DungeonWaterType waterType){
		Vector3 posMin = RandomDungenMgrData.genDunPos+new Vector3 (-1000,-50,-1000);
		Vector3 posMax = posMin+new Vector3 (2000,5,2000);

		if(waterType ==DungeonWaterType.Deep){
			dungeonWater = GameObject.Instantiate(dungeonWaterPrefab);
			dungeonWater.transform.position = RandomDungenMgrData.genDunPos-new Vector3 (0,4,0);
		}else if(waterType ==DungeonWaterType.Shallow){
			dungeonWater = GameObject.Instantiate(dungeonWaterPrefab);
			dungeonWater.transform.position = RandomDungenMgrData.genDunPos-new Vector3 (0,10,0);
		}else{
//
//			dungeonWater = GameObject.Instantiate(dungeonWaterPrefab);
//			dungeonWater.transform.position = RandomDungenMgrData.genDunPos-new Vector3 (0,4,0);
		}

		if(generator.CurrentDungeon.gameObject!=null){
			Bounds bd = PETools.PEUtil.GetWordColliderBoundsInChildren(generator.CurrentDungeon.gameObject);
			posMin = bd.min+new Vector3 (-80,0,-80);
			posMax = new Vector3 (bd.max.x+80,posMin.y+5,bd.max.z+80);
			if(dungeonWater!=null){
				dungeonWater.transform.position = new Vector3 (bd.center.x,dungeonWater.transform.position.y,bd.center.z);
				dungeonWater.transform.localScale = new Vector3 ((bd.size.x+100)/10,1,(bd.size.z+100)/10);
			}
		}
			
		FBSafePlane.instance.ResetCol(posMin, posMax, RandomDungenMgrData.revivePos);
	}

	#endregion


    #region entrance
	//1.only 1 in 256X256 Area
	//2.check nearhave 256X256
	public const float PASSTIME_T = 360;
	public const float PASSDIST_T = 1000;
	public const float GEN_T = 1.5f;//2
	public const int DISTANCE_MAX = 20;
	public const int INDEX256MAX = 1;//18;
	public const int GEN_RADIUS_MIN = 50;//50;
	public const int GEN_RADIUS_MAX = 100;//100;[min,max)

	public bool generateSwitch = false;
	public float multiFactor = 4;
	public PeTrans playerTrans;
	public Vector3 born_pos;
	public Vector3 start_pos;
	public Vector3 last_pos;
	public float timeCounter = 0;
	public float last_time;
	public float timePassed = 0;
	public float distancePassed = 0;
	public int counter = 0;

	const int AREA_RADIUS = 8;//2^8=256
	public static Dictionary<IntVector2,DunEntranceObj> entranceArea = new Dictionary<IntVector2,DunEntranceObj>();
	public static Dictionary<Vector3,DunEntranceObj> allEntrance = new Dictionary<Vector3,DunEntranceObj> ();


//    public void AddEntrance(Vector3 pos){
//		isAdding=true;
//		if(!entrancesToAdd.Contains(pos)){
//
//			entrancesToAdd.Add(pos);
//		}
//		isAdding = false;
//	}
	void Update(){

		if (PeGameMgr.IsAdventure)
		{
			counter++;
			timeCounter += Time.deltaTime;
			if (counter > 240)
			{
				if (playerTrans == null)
				{
					if (PeCreature.Instance.mainPlayer != null && PeCreature.Instance.mainPlayer.peTrans.position != Vector3.zero)
					{
						playerTrans = PeCreature.Instance.mainPlayer.peTrans;
						born_pos = playerTrans.position;
						if(Application.isEditor)
							Debug.Log("<color=yellow>" + "born_pos" + born_pos + "</color>");
						start_pos = born_pos;
						last_pos = start_pos;
						last_time = timeCounter;
					}
					counter = 0;
					return;
				}

				if(RandomDungenMgrData.InDungeon)
				{
					last_pos = RandomDungenMgrData.enterPos;
					last_time = timeCounter;
					return;
				}


				if (!generateSwitch)
				{
					timePassed += timeCounter - last_time;
					float moreDistance = Vector2.Distance(new Vector2(playerTrans.position.x, playerTrans.position.z), new Vector2(last_pos.x, last_pos.z));//--to do:
					if (moreDistance > DISTANCE_MAX)
					{
						moreDistance = DISTANCE_MAX;
					}
					distancePassed += moreDistance;
					last_pos = playerTrans.position;
					last_time = timeCounter;
					
					if (CheckGenerate(timePassed, distancePassed))
					{
						generateSwitch = true;
						timePassed = 0;
						distancePassed = 0;
					}
					else
					{
						counter = 0;
						return;
					}
				}
				
				if (generateSwitch)
				{
					System.Random randSeed = new System.Random();
					int distance = GEN_RADIUS_MAX - GEN_RADIUS_MIN;
					
					int xPlus = randSeed.Next(distance * 2) - distance;
					int zPlus = randSeed.Next(distance * 2) - distance;
					if (xPlus >= 0)
						xPlus += GEN_RADIUS_MIN;
					else
						xPlus = xPlus - GEN_RADIUS_MIN + 1;
					
					if (zPlus >= 0)
						zPlus += GEN_RADIUS_MIN;
					else
						zPlus = zPlus - GEN_RADIUS_MIN + 1;
					
					IntVector2 GenPos = new IntVector2((int)playerTrans.position.x + xPlus, (int)playerTrans.position.z + zPlus);
					//--to do areaAvalable
					Vector3 pos;
					//int boxId;
					if(IsTerrainAvailable(GenPos, out pos)){
						if (IsAreaAvalable(new Vector2(pos.x,pos.z)))
						{
							InstantiateEntrance(pos-new Vector3(0,0.5f,0));
							generateSwitch = false;
						} 
					}
				}
				
				counter = 0;
			}
		}

//
//		counter++;
//		if(counter>24){
//			if (PeGameMgr.IsAdventure&&PeGameMgr.yirdName!=AdventureScene.Dungen.ToString()){
//				if(entrancesToAdd.Count>0&&!isAdding){
//					List<Vector3> posList = entrancesToAdd;
//					entrancesToAdd=new List<Vector3> ();
//					foreach(Vector3 genPos in posList){
//
//				   }
//				}
//				counter=0;
//			}
//		}
    }

	bool CheckGenerate(float passedTime, float passedDistance)
	{
		bool flag = false;
		if (passedTime < PASSTIME_T)
		{
			return flag;
		}
		if (passedDistance < PASSDIST_T)
		{
			return flag;
		}

		float factor = 1;
		if(PeGameMgr.IsMulti)
			factor = multiFactor;
		if (passedTime * passedDistance / PASSTIME_T / PASSDIST_T>GEN_T*factor)
		{
			flag = true;
		}
		return flag;
	}
	bool IsAreaAvalable(Vector2 pos)
	{
		if(VArtifactTownManager.Instance.TileContainsTown(new IntVector2(Mathf.RoundToInt(pos.x) >> 5, Mathf.RoundToInt(pos.y)>>5)))
			return false;

		IntVector2 indexArea = new IntVector2(Mathf.RoundToInt(pos.x) >> AREA_RADIUS, Mathf.RoundToInt(pos.y)>>AREA_RADIUS);
		for(int i=indexArea.x-1;i<indexArea.x+2;i++)
			for(int j=indexArea.y-1;j<indexArea.y+2;j++)
				if(entranceArea.ContainsKey(new IntVector2(i,j)))
					return false;
		return true;
	}
	public bool IsTerrainAvailable(IntVector2 GenPos,out Vector3 pos)
	{
		if(!VFDataRTGen.IsDungeonEntranceAvailable(GenPos)){
			pos = Vector3.zero;
			return false;
		}
//		int y = VFDataRTGen.GetPosHeight(GenPos,true);
//
//		pos = new Vector3(GenPos.x,y+4,GenPos.y);
//		RaycastHit hit;
//		if (Physics.Raycast(pos, Vector3.down, out hit, 512, 1 << Pathea.Layer.VFVoxelTerrain))
//		{
//			pos.y = hit.point.y;
//		}else{
//			return false;
//		}
		bool result = RandomDunGenUtil.GetAreaLowestPos(GenPos,10,out pos);
		return result;
	}

	void InstantiateEntrance(Vector3 genPos,int level = -1){
		if(level==-1)
			level = RandomDunGenUtil.GetEntranceLevel(genPos);
		DungeonBaseData bd = RandomDungeonDataBase.GetDataFromLevel(level);
		if(bd!=null){
			if(PeGameMgr.IsSingle)
				GenDunEntrance(genPos,bd);
			else{
				PlayerNetwork.mainPlayer.RequestGenDunEntrance(genPos,bd.id);
			}
		}
	}

	public UnityEngine.Object GetEntrancePrefabPath(DungeonType type){
		if(type ==DungeonType.Iron)
			return entrancePrefab0;
		if(type == DungeonType.Cave)
			return entrancePrefab1;
		return entrancePrefab0;
	}

	public void GenDunEntrance(Vector3 genPos,DungeonBaseData basedata){

		if(!allEntrance.ContainsKey(genPos)){
			UnityEngine.Object entranceObj= GetEntrancePrefabPath(basedata.Type);
			DunEntranceObj entrance = new DunEntranceObj(entranceObj,genPos);
			entrance.Level=basedata.level;
			entrance.DungeonId = basedata.id;
			allEntrance[genPos] = entrance;
			if(basedata.level>=100){

			}
			else{
				IntVector2 indexArea = new IntVector2(Mathf.RoundToInt(genPos.x) >> AREA_RADIUS, Mathf.RoundToInt(genPos.z)>>AREA_RADIUS);
				entranceArea[indexArea] = entrance;
			}


			SceneMan.AddSceneObj(entrance);            
            return;
        }
	}

	public void DestroyEntrance(Vector3 entrancePos){
		if(!allEntrance.ContainsKey(entrancePos))
			return;
		allEntrance[entrancePos].DestroySelf();
		allEntrance.Remove(entrancePos);
		IntVector2 indexArea = new IntVector2(Mathf.RoundToInt(entrancePos.x) >> AREA_RADIUS, Mathf.RoundToInt(entrancePos.z)>>AREA_RADIUS);
		entranceArea.Remove(indexArea);
	}

	//method 2.
	public void GenTestEntrance(int level = -1){
		int x = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.position.x);
		int z = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.position.z+15);
		int y = VFDataRTGen.GetPosHeight(x,z);
		Vector3 genPos = new Vector3 (x,y,z);
		InstantiateEntrance(genPos,level);
	}
	//task
	public void GenTaskEntrance(IntVector2 genXZ,int level = -1){
		Vector3 genPos = RandomDunGenUtil.GetPosOnGround(genXZ);
		InstantiateEntrance(genPos,level);
	}
    #endregion
}

