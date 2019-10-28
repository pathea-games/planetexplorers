//------------------------------------------------------------------------------
// 2016年7月16日11:11:26
// by Pugee
//to generat town position
//to maintain town's related data
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VATConnectionLine{
	public IntVector2 leftPos;
	public IntVector2 rightPos;
	public VATConnectionLine(IntVector2 s,IntVector2 e){
		if(s.x<e.x){
			leftPos = s;
			rightPos = e;
		}else if(s.x>e.x){
			leftPos = e;
			rightPos = s;
		}else{
			//if(same x),left is down
			if(s.y<e.y){
				leftPos=s;
				rightPos=e;
			}else{
				leftPos=e;
				rightPos=s;
			}
		}
	}
	public override bool Equals (object obj)
	{
		if(null == obj)
			return false;
		VATConnectionLine vec = (VATConnectionLine) obj;
		if(vec !=null)
		{
			return (leftPos.Equals(vec.leftPos) && rightPos.Equals(vec.rightPos))||(leftPos.Equals(vec.rightPos) && rightPos.Equals(vec.leftPos));
		}
		return false;
	}
	public override int GetHashCode ()
	{
		return leftPos.GetHashCode()+rightPos.GetHashCode();
	}
}

public enum AllyType{
	Player=0,
	Puja,
	Paja,
	Npc
}
/// <summary>
/// real means position after mirror and rotation
/// origin means position before mirror and rotation
/// </summary>
public class VATownGenerator
{
	static VATownGenerator mInstance;
	public static VATownGenerator Instance
	{
		get
		{
			if(mInstance==null)
				mInstance = new VATownGenerator ();
			return mInstance;
		}
	}
	//areaId, mainTown
	Dictionary<int,List<VArtifactTown>> MainTownDic = new Dictionary<int, List<VArtifactTown>>();
	List<VArtifactTown> mainTownList = new List<VArtifactTown> ();
	List<VArtifactTown> branchTownList = new List<VArtifactTown> (); 
	Dictionary<IntVector2,List<VArtifactTown>> areaTown = new Dictionary<IntVector2, List<VArtifactTown>>();
	List<VArtifactTown> emptyTowns = new List<VArtifactTown> ();

	List<VATConnectionLine> townConnection = new List<VATConnectionLine> ();
	const int LinkAreaIndex = 8;
	Dictionary<IntVector2,List<VATConnectionLine>> areaConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();
	Dictionary<IntVector2,List<VATConnectionLine>> areaTypeConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();
	//first mirror,then rotation, get the actual pos
	
	Dictionary<int,List<VArtifactTown>> allyTownDic = new Dictionary<int,List<VArtifactTown>>();
	public int EnemyNpcAllyCount;
	public int PujaAllyCount;
	public int PajaAllyCount;
	Dictionary<int,int> allyPlayerIdDic = new Dictionary<int,int>();

	public int playerStartTownCount;
	public int branchTownCountMax;
	public int playerEmptyTownDistanceMin;
	public int playerEmptyTownDistanceMax;
	Dictionary<int,int> allyAreaCount = new Dictionary<int, int>();
	Dictionary<int,int> allyAreaThreashold = new Dictionary<int, int>();

	Dictionary<int,int> allyColor = new Dictionary<int, int>();
	Dictionary<int,int> allyName = new Dictionary<int, int>();
	Dictionary<int,List<int>> allyNamePool =new Dictionary<int,List<int>> ();

	public void ClearData(){
		MainTownDic = new Dictionary<int, List<VArtifactTown>>();
		mainTownList = new List<VArtifactTown> ();
		branchTownList = new List<VArtifactTown> (); 
		areaTown = new Dictionary<IntVector2, List<VArtifactTown>>();
		townConnection = new List<VATConnectionLine> ();
		areaConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();
		areaTypeConnections = new Dictionary<IntVector2, List<VATConnectionLine>>();
		allyTownDic = new Dictionary<int, List<VArtifactTown>>();
		allyPlayerIdDic= new Dictionary<int, int>();
		emptyTowns = new List<VArtifactTown> ();
		allyNamePool = new Dictionary<int,List<int>> ();
	}

	static float ConnectionAreaWidth{
		get{return VFDataRTGen.TownConnectionAreaWidth;}
	}
	static float ConnectionAreaTypeWidth{
		get{return VFDataRTGen.TownConnectionAreaTypeWidth;}
	}
	static float TownAreaMaxDistance{
		get{return VFDataRTGen.TownChangeMaxDistance;}
	}
	static float TownAreaMaxFactor{
		get{return VFDataRTGen.TownChangeMaxFactor;}
	}
	bool Mirror{
		get{return RandomMapConfig.mirror;}
	}
	float RotationF{
		get{return RandomMapConfig.rotation*90;}
	}
	int MapRadius{
		get{return RandomMapConfig.Instance.mapRadius;}
	}
	int MapSizeId{
		get{return RandomMapConfig.mapSize;}
	}
	int PickedLineIndex{
		get{return RandomMapConfig.pickedLineIndex;}
	}
	int PickedLevelIndex{
		get{return RandomMapConfig.pickedLevelIndex;}
	}

	int BiomaCount{
		get{return RandomMapConfig.RandomMapTypeCount;}
	}

	int AllyCount{
		get{return RandomMapConfig.allyCount;}
	}

	#region init
	public List<RandomMapTypePoint> InitBiomaPos(){
		System.Random myRand = new System.Random (RandomMapConfig.TownGenSeed);
		IntVector2 StartBiomaPos = GenTownPos(TownGenData.GenerationLine[PickedLineIndex].ToList()[0],myRand);
		List<IntVector2> biomaPosList= new List<IntVector2> ();
		List<RandomMapTypePoint> biomaDistList = new List<RandomMapTypePoint>();
		int biomaPosCount =BiomaCount;
		for(int i=1;i<=BiomaCount;i++){
			if(i==(int)RandomMapConfig.RandomMapID)
			{
				biomaDistList.Add(new RandomMapTypePoint ((RandomMapType)i,StartBiomaPos));
			}else{
				biomaDistList.Add(new RandomMapTypePoint ((RandomMapType)i));
			}
		}

		if(MapSizeId>1){
			for(int i=0;i<biomaPosCount;i++){
				IntVector2 p = GetRealPos(new IntVector2(myRand.Next(-MapRadius,MapRadius),myRand.Next(-MapRadius,MapRadius)));
				if(biomaPosList.Contains(p)){
					i--;
				}else if(StartBiomaPos.Distance(p)<500){
					i--;
				}
				else {
					bool isTooNear = false;
					foreach(IntVector2 pos in biomaPosList){
						if(pos.Distance(p)<VFDataRTGen.changeBiomaDiff)
						{	
							isTooNear = true;
							i--;
							break;
						}
					}
					if(!isTooNear)
						biomaPosList.Add(p);
				}
			}
		}else {
			if(MapSizeId==1){
				biomaPosCount = BiomaCount*2;
			}
			else{
				biomaPosCount = BiomaCount*4;
			}
			for(int i=0;i<biomaPosCount;i++){
				IntVector2 p = GetRealPos(new IntVector2(myRand.Next(-MapRadius,MapRadius),myRand.Next(-MapRadius,MapRadius)));
				if(biomaPosList.Contains(p)){
					i--;
				}else if(StartBiomaPos.Distance(p)<1000){
					i--;
				}
				else {
					bool isTooNear = false;
					foreach(IntVector2 pos in biomaPosList){
						if(pos.Distance(p)<VFDataRTGen.changeBiomaDiff)
						{	
							isTooNear = true;
							i--;
							break;
						}
					}
					if(!isTooNear)
						biomaPosList.Add(p);
				}
			}
		}

		for(int i=0;i<biomaPosCount;i++){
			biomaDistList[i%BiomaCount].AddPos(biomaPosList[i]);
		}
		return biomaDistList;
	}

	public List<int> GetPickedGenLine(){
		return TownGenData.GenerationLine[PickedLineIndex].ToList();
	}
	public IntVector2 GetInitPos(){
		System.Random myRand = new System.Random (RandomMapConfig.TownGenSeed);
		return GenTownPos(TownGenData.GenerationLine[PickedLineIndex].ToList()[0],myRand);
	}
	public IntVector2 GetInitPos(System.Random myRand){
		return GenTownPos(TownGenData.GenerationLine[PickedLineIndex].ToList()[0],myRand);
	}

	public IntVector2 GenTownPos(int areaId,System.Random myRand){
		IntVector2 areaIndex = GetRealPos(TownGenData.AreaIndex[areaId]);
		int minX = areaIndex.x>0?(areaIndex.x-1)*TownGenData.AreaRadius:areaIndex.x*TownGenData.AreaRadius;
		int minZ = areaIndex.y>0?(areaIndex.y-1)*TownGenData.AreaRadius:areaIndex.y*TownGenData.AreaRadius;
		int maxX = areaIndex.x>0?areaIndex.x*TownGenData.AreaRadius:(areaIndex.x+1)*TownGenData.AreaRadius;
		int maxZ = areaIndex.y>0?areaIndex.y*TownGenData.AreaRadius:(areaIndex.y+1)*TownGenData.AreaRadius;
		return new IntVector2(myRand.Next(minX,maxX),myRand.Next(minZ,maxZ));
	}

//	public bool DecideMainTownAndAlliance(System.Random myRand,VArtifactTown vat){
//		bool isMain = myRand.NextDouble()>TownGenData.BranchTownCount;
//		if(isMain)
//			DecideTownAlly(myRand,vat);
//		else
//			vat.AllyId = 0;
//		AddAllyTown(vat);
//		return isMain;
//	}

	void InitStartTownCount(){
		if(MapSizeId>2)//small
			playerStartTownCount = TownGenData.PlayerAreaCountSmall;
		else if(MapSizeId==2)
			playerStartTownCount = TownGenData.PlayerAreaCountMiddle;
		else 
			playerStartTownCount = TownGenData.PlayerAreaCountlarge;
		Debug.Log("playerStartTown: "+playerStartTownCount);

	}
	void InitBranchTownCount(){
		if(MapSizeId>2)
			branchTownCountMax = TownGenData.BranchTownCountSmallMax;
		else if(MapSizeId==2)
			branchTownCountMax = TownGenData.BranchTownCountMiddleMax;
		else if(MapSizeId==1)
			branchTownCountMax = TownGenData.BranchTownCountLargeMax;
		else 
			branchTownCountMax = TownGenData.BranchTownCountHugeMax;
		Debug.Log("branchTownCountMax: "+branchTownCountMax);
	}
	void InitEmptyTownDistance(){
		if(MapSizeId>2){
			playerEmptyTownDistanceMin = TownGenData.EmptyTownMinDistanceSmall;
			playerEmptyTownDistanceMax = TownGenData.EmptyTownMaxDistanceSmall;
		}
		else if(MapSizeId==2){
			playerEmptyTownDistanceMin = TownGenData.EmptyTownMinDistanceMiddle;
			playerEmptyTownDistanceMax = TownGenData.EmptyTownMaxDistanceMiddle;
		}
		else {
			playerEmptyTownDistanceMin = TownGenData.EmptyTownMinDistanceLarge;
			playerEmptyTownDistanceMax = TownGenData.EmptyTownMaxDistanceLarge;
		}
		Debug.Log("EmptyTownDistanceMin: "+playerEmptyTownDistanceMin+" max:"+playerEmptyTownDistanceMax);
	}
	public void InitAllyDistribution(System.Random myRand){
		Debug.Log("mirror:"+Mirror+" rotation:"+RotationF+" pickedLineIndex:"+PickedLineIndex+" pickedLevelIndex:"+PickedLevelIndex);

		InitStartTownCount();
		InitBranchTownCount();

		EnemyNpcAllyCount = 1;
		PujaAllyCount = 1;
		PajaAllyCount = 1;
		for(int i=0;i<AllyCount-4;i++)
		{
			if(myRand.NextDouble()<0.3333f)
				EnemyNpcAllyCount++;
			else if(myRand.NextDouble()<0.6666f)
				PujaAllyCount++;
			else
				PajaAllyCount++;
		}
		Debug.Log("EnemyNpc: "+EnemyNpcAllyCount+" Puja: "+PujaAllyCount+" Paja: "+PajaAllyCount);
		InitAllyColor(myRand);
		InitAllyPlayerId(myRand);
		InitAllyArea(myRand);
		InitAllyName(myRand);
	}

	public void InitAllyColor(System.Random myRand){
		allyColor.Clear();
		allyColor.Add(0,0);
		List<int> colorList = new List<int> ();
		for(int i=1;i<AllyCount;i++){
			colorList.Add(i);
		}
		VArtifactUtil.Shuffle(colorList,myRand);
		for(int i=1;i<AllyCount;i++)
			allyColor.Add(i,colorList[i-1]);
		foreach(KeyValuePair<int,int> kvp in allyColor){
			Debug.Log("ally: "+kvp.Key+" color:"+kvp.Value);
		}
	}

	public void InitAllyArea(System.Random myRand){
		allyAreaCount.Clear();
		allyAreaThreashold.Clear();
		int otherAllyAreaCount = TownGenData.AreaCount-playerStartTownCount-AllyCount+1;
		int otherAllyCount = AllyCount-1;
		allyAreaCount.Add(0,playerStartTownCount);
		for(int i=1;i<AllyCount;i++){
			allyAreaCount.Add(i,1);
		}

		for(int i=0;i<otherAllyAreaCount;i++){
			double randNum = myRand.NextDouble();
			float baseValue= 1.0f/otherAllyCount;
			for(int j=1;j<AllyCount;j++){
				if(randNum<baseValue*j)
				{
					allyAreaCount[j]++;
					break;
				}
			}
		}
		allyAreaThreashold.Add(0,allyAreaCount[0]);
		for(int i=1;i<AllyCount;i++)
			allyAreaThreashold.Add(i,allyAreaCount[i]+allyAreaThreashold[i-1]);
		
		foreach(KeyValuePair<int,int> kvp in allyAreaThreashold){
			Debug.Log("ally: "+kvp.Key+" AreaThreashold:"+kvp.Value);
		}
	}

	public void InitAllyPlayerId(System.Random rand){
		allyPlayerIdDic.Clear();
		allyPlayerIdDic.Add (0,AdventureAlly.DefaultPlayerId);
		allyPlayerIdDic.Add (1,AdventureAlly.PujaStartPlayerId);
		int i=2;
		List<int> playerIdList = new List<int> ();
		for(int j=0;j<PujaAllyCount-1;j++)
			playerIdList.Add(AdventureAlly.PujaStartPlayerId+1+j);
		for(int j=0;j<PajaAllyCount;j++)
			playerIdList.Add(AdventureAlly.PajaStartPlayerId+j);
		for(int j=0;j<EnemyNpcAllyCount;j++)
			playerIdList.Add(AdventureAlly.EnemyNpcStartPlayerId+j);
		VArtifactUtil.Shuffle(playerIdList,rand);
		for(;i<AllyCount;i++){
			allyPlayerIdDic.Add(i,playerIdList[i-2]);
		}
		foreach(KeyValuePair<int,int> kvp in allyPlayerIdDic){
			Debug.Log("ally: "+kvp.Key+" playerId:"+kvp.Value);
		}
	}

	public void InitAllyName(System.Random rand){
		allyName = new Dictionary<int,int>();
		foreach(AllyName an in VArtifactUtil.allyNameData.Values){
			if(!allyNamePool.ContainsKey(an.raceId))
				allyNamePool.Add(an.raceId,new List<int> ());
			allyNamePool[an.raceId].Add(an.nameId);
		}
		for(int i=1;i<AllyCount;i++){
			AllyType at = GetAllyType(i);
			List<int> idList = allyNamePool[(int)at];
			int index = rand.Next(idList.Count);
			int nameId = idList[index];
			allyName.Add(i,nameId);
			idList.Remove(index);
		}
		foreach(KeyValuePair<int,int> kvp in allyName){
			Debug.Log("ally: "+kvp.Key+" nameId:"+kvp.Value);
		}
	}
	#endregion

	#region interface
	public AllyType GetAllyType(int allyId){
		int playerId = GetPlayerId(allyId);
		if(playerId==0)
			return AllyType.Player;
		if(playerId>=AdventureAlly.PujaStartPlayerId&&playerId<AdventureAlly.PajaStartPlayerId)
			return AllyType.Puja;
		if(playerId>=AdventureAlly.PajaStartPlayerId&&playerId<AdventureAlly.EnemyNpcStartPlayerId)
			return AllyType.Paja;
		if(playerId>=AdventureAlly.EnemyNpcStartPlayerId)
			return AllyType.Npc;
		return AllyType.Npc;
	}

	public int GetAllyNum(int allyId){
		int playerId = GetPlayerId(allyId);
		if(playerId>=AdventureAlly.PujaStartPlayerId&&playerId<AdventureAlly.PajaStartPlayerId)
			return playerId-AdventureAlly.PujaStartPlayerId+1;
		if(playerId>=AdventureAlly.PajaStartPlayerId&&playerId<AdventureAlly.EnemyNpcStartPlayerId)
			return playerId-AdventureAlly.PajaStartPlayerId+1;
		if(playerId>=AdventureAlly.EnemyNpcStartPlayerId)
			return playerId-AdventureAlly.EnemyNpcStartPlayerId+1;
		return 0;
	}

	public int GetPlayerId(int allyId){
		if(allyPlayerIdDic.ContainsKey(allyId))
			return allyPlayerIdDic[allyId];
		else 
			return 0;
	}
	public int GetAllyIdByPlayerId(int playerId){
		foreach(KeyValuePair<int,int> kvp in allyPlayerIdDic){
			if(kvp.Value ==playerId)
				return kvp.Key;
		} 
		return 0;
	}
	public int GetAllyIdByAreaIndex(int areaIndex){
		int allyId = 0;
		for(int i=0;i<AllyCount;i++){
			if(areaIndex<allyAreaThreashold[i]){
				allyId = i;
				break;
			}
		}
		return allyId;
	}
	public int GetAllyIdByAreaId(int areaId){
		return GetAllyIdByAreaIndex(GetGenLineIndexByAreaId(areaId));
	}
	public AllyType GetAllyTypeByAreaIndex(int areaIndex){
		return GetAllyType(GetAllyIdByAreaIndex(areaIndex));
	}

	public int GetFirstEnemyNpcAllyColor(){
		for(int i=1;i<AllyCount;i++)
		{
			if(GetAllyType(i)==AllyType.Npc){
				return GetAllyColor(i);
			}
		}
		return 1;
	}
	public int GetFirstEnemyNpcAllyPlayerId(){
		return AdventureAlly.EnemyNpcStartPlayerId;
	}
	public AllyType GetAllyTypeByAreaId(int areaId){
		return GetAllyTypeByAreaIndex(GetGenLineIndexByAreaId(areaId));
	}

	public int GetAllyColor(int allyId){
		if(allyColor.ContainsKey(allyId))
			return allyColor[allyId];
		return 0;
	}

	public int GetAllyTownCount(int allyId){
		if(!allyTownDic.ContainsKey(allyId)){
			return 0;
		}
		return allyTownDic[allyId].Count;
	}

	public int GetAllyTownDestroyedCount(int allyId){
		if(!allyTownDic.ContainsKey(allyId)){
			return 0;
		}
		else{
			List<VArtifactTown> townList = allyTownDic[allyId];
			return townList.FindAll(it=>VArtifactTownManager.Instance.capturedCampId.ContainsKey(it.townId)).Count;
		}
	}
	public int GetAllyTownExistCount(int allyId){
		if(!allyTownDic.ContainsKey(allyId)){
			return 0;
		}
		else{
			List<VArtifactTown> townList = allyTownDic[allyId];
			return townList.FindAll(it=>!VArtifactTownManager.Instance.capturedCampId.ContainsKey(it.townId)).Count;
		}
	}

	public List<VArtifactTown> GetAllyTowns(int allyId){
		if(!allyTownDic.ContainsKey(allyId)){
			return new List<VArtifactTown> ();
		}else{
			return allyTownDic[allyId];
		}
	}
	public AllyType GetRandomExistEnemyType(out int color){
		List<int> existEnemyList = new List<int> ();
		for(int i=1;i<AllyCount;i++)
		{
			if(GetAllyTownExistCount(i)>0)
				existEnemyList.Add(i);
		}
		if(existEnemyList.Count==0){
			color = -1;
			return AllyType.Player;
		}
		int allyId = existEnemyList[new System.Random().Next(existEnemyList.Count)];
		color = GetAllyColor(allyId);
		return GetAllyType(allyId);
	}
	public int GetAllyName(int allyId){
		if(!allyName.ContainsKey(allyId)){
			return -1;
		}else{
			return allyName[allyId];
		}
	}

	public float GetNearestAllyDistance(Vector3 pos,out int allyId,out Vector3 townPos){
		float minDistance = float.MaxValue;
		allyId = 0;
		townPos = Vector3.zero;
		for(int i=1;i<AllyCount;i++){
			if(!allyTownDic.ContainsKey(i))
				continue;
			List<VArtifactTown> allyExistTown = allyTownDic[i].FindAll(it=>!VArtifactTownManager.Instance.capturedCampId.ContainsKey(it.townId));

			foreach(VArtifactTown vat in allyExistTown)
			{
				float tmpDist = Vector3.Distance(pos,vat.TransPos);
				if(tmpDist<minDistance)
				{
					minDistance=tmpDist;
					allyId=i;
					townPos = vat.TransPos;
				}
			}
		}
		return minDistance;
	}
	#endregion

	#region func
	public void AddTown(int areaId,VArtifactTown townData){
//		if(townData.type==VArtifactType.NpcTown){
		if(townData.isMainTown){
			AddMainTown(areaId,townData);
		}
		else
			AddBranchTown(townData);
//		}
		LinkTownToArea(townData);
	}


	public void AddMainTown(int areaId,VArtifactTown townData){
		if(MainTownDic.ContainsKey(areaId))
			MainTownDic[areaId].Add(townData);
		else
			MainTownDic.Add(areaId,new List<VArtifactTown>(){townData});
	}

	public void AddBranchTown(VArtifactTown townData){
		branchTownList.Add(townData);
	}

	public void AddAllyTown(VArtifactTown townData){
		if(!allyTownDic.ContainsKey(townData.AllyId)){
			allyTownDic.Add(townData.AllyId,new List<VArtifactTown> ());
		}
		allyTownDic[townData.AllyId].Add(townData);
	}
	

	public void ChangeAlliance(VArtifactTown vat){
		allyTownDic[vat.allyId].Remove(vat);
		vat.allyId = TownGenData.PlayerAlly;
		allyTownDic[vat.allyId].Add(vat);
		//--to do: refresh
	}

	public void RestoreAlliance(VArtifactTown vat){
		allyTownDic[vat.allyId].Remove(vat);
		vat.allyId = vat.genAllyId;
		allyTownDic[vat.allyId].Add(vat);
		//--to do: refresh
	}

	public void AddEmtpyTown(VArtifactTown vat){
		emptyTowns.Add(vat);
	}
	public VArtifactTown GetEmptyTown(int id){
		return emptyTowns.Find(it=>it.townId==id);
	}


	public void AddTownConnection(IntVector2 v1,IntVector2 v2,System.Random rand){
		float dist= v1.Distance(v2);
		if(dist>TownGenData.ConnectionCutDist0){
			if(rand.NextDouble()<TownGenData.DistCutPer0)
				GenCutPointNewLine(v1,v2,rand);
			else
				AddConnection(v1,v2);
		}
		else{
			if(dist>TownGenData.ConnectionCutDist1)
			{
				if(rand.NextDouble()<TownGenData.DistCutPer1)
					GenCutPointNewLine(v1,v2,rand);
				else
					AddConnection(v1,v2);
			}else{
				if(dist>TownGenData.ConnectionCutDist2)
				{
					if(rand.NextDouble()<TownGenData.DistCutPer2)
						GenCutPointNewLine(v1,v2,rand);
					else
						AddConnection(v1,v2);
				}else{
					if(dist>TownGenData.ConnectionCutDist3)
					{
						if(rand.NextDouble()<TownGenData.DistCutPer3)
							GenCutPointNewLine(v1,v2,rand);
						else
							AddConnection(v1,v2);
					}else{
						AddConnection(v1,v2);
					}
				}
			}
		}
	}

	public void GenCutPointNewLine(IntVector2 v1,IntVector2 v2,System.Random rand){
		float dist= v1.Distance(v2);
		IntVector2 centerPoint = new IntVector2((v1.x+v2.x)/2,(v1.y+v2.y)/2);
		float radius = dist/4;
		IntVector2 result = VArtifactUtil.GetRandomPointFromPoint(centerPoint,radius,rand);
		AddTownConnection(v1,result,rand);
		AddTownConnection(result,v2,rand);
	}

	public void AddConnection(IntVector2 v1,IntVector2 v2){
		VATConnectionLine vatc = new VATConnectionLine(v1,v2);
		if(!townConnection.Contains(vatc)){
			townConnection.Add(vatc);
			LinkConnectionToArea(vatc);
		}
	}

	public void ConnectMainTowns(){
		List<int> genLine = GetPickedGenLine();
		for(int i=0;i<genLine.Count;i++){
			if(i==0){
				List<VArtifactTown> areaTowns = new List<VArtifactTown>();
				areaTowns.AddRange(MainTownDic[genLine[i]]);
				VArtifactTown vat = areaTowns.First<VArtifactTown>();
				mainTownList.Add(vat);
				areaTowns.Remove(vat);
				int areaTownsCount = areaTowns.Count;
				for(int j=0;j<areaTownsCount;j++){
					VArtifactTown lastTown = mainTownList.Last<VArtifactTown>();
					vat = GetNearestTown(lastTown.PosCenter,areaTowns);
					if(lastTown.townId==0&&vat.townId!=4)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(4));
					else if(lastTown.townId==4&&vat.townId!=5)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(5));
					else if(lastTown.townId==5&&vat.townId!=6)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(6));
					else if(lastTown.townId==6&&vat.townId!=7)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(7));
					mainTownList.Add(vat);
					areaTowns.Remove(vat);
				}
			}else{
				if(!MainTownDic.ContainsKey(genLine[i]))
					continue;
				List<VArtifactTown> areaTowns = new List<VArtifactTown>();
				areaTowns.AddRange(MainTownDic[genLine[i]]);
				int areaTownsCount = areaTowns.Count;
				for(int j=0;j<areaTownsCount;j++){
					//VArtifactTown vat = GetNearestTown(mainTownList.Last<VArtifactTown>().PosCenter,areaTowns);
					VArtifactTown lastTown = mainTownList.Last<VArtifactTown>();
					VArtifactTown vat = GetNearestTown(lastTown.PosCenter,areaTowns);
					if(lastTown.townId==0&&vat.townId!=4)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(4));
					else if(lastTown.townId==4&&vat.townId!=5)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(5));
					else if(lastTown.townId==5&&vat.townId!=6)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(6));
					else if(lastTown.townId==6&&vat.townId!=7)
						VArtifactTownManager.Instance.SwitchTownId(vat,VArtifactTownManager.Instance.GetTownByID(7));
					mainTownList.Add(vat);
					areaTowns.Remove(vat);
				}
			}
		}
	}
	public VArtifactTown GetNearestTown(IntVector2 pos,List<VArtifactTown> vatList){
		float distance = 999999;
		VArtifactTown result=null;
		foreach(VArtifactTown vat in vatList){
			float dist = vat.PosCenter.Distance(pos);
			if(dist<distance){
				distance = dist;
				result = vat;
			}
		}
		return result;
	}
	public void GenerateConnection(System.Random rand){
		for(int i=0;i<mainTownList.Count-1;i++)
		{
			VArtifactTown thisTown = mainTownList[i];
			VArtifactTown nextTown = mainTownList[i+1];
			AddTownConnection(thisTown.PosCenter,thisTown.PosEntrance,rand);
			if(nextTown.PosStart.y>thisTown.PosStart.y)
			{
				if(nextTown.PosCenter.x<thisTown.PosCenter.x){
					AddTownConnection(thisTown.PosEntrance,thisTown.PosEntranceLeft,rand);
					AddTownConnection(thisTown.PosEntranceLeft,nextTown.PosEntrance,rand);
				}
				else{
					AddTownConnection(thisTown.PosEntrance,thisTown.PosEntranceRight,rand);
					AddTownConnection(thisTown.PosEntranceRight,nextTown.PosEntrance,rand);
				}
			}else{
				if(nextTown.PosCenter.x<thisTown.PosCenter.x){
					AddTownConnection(thisTown.PosEntrance,nextTown.PosEntranceRight,rand);
					AddTownConnection(nextTown.PosEntranceRight,nextTown.PosEntrance,rand);
				}
				else{
					AddTownConnection(thisTown.PosEntrance,nextTown.PosEntranceLeft,rand);
					AddTownConnection(nextTown.PosEntranceLeft,nextTown.PosEntrance,rand);
				}
			}
		}
		
		if(mainTownList.Count>1)
			AddTownConnection(mainTownList[mainTownList.Count-1].PosCenter,mainTownList[mainTownList.Count-1].PosEntrance,rand);
	}


	public float GetAreaTownDistance(int x,int z,out VArtifactTown vaTown){
		float distancePow = float.MaxValue;
		vaTown=null;
		IntVector2 areaIndex = new IntVector2 (x>>LinkAreaIndex,z>>LinkAreaIndex);
		if(!areaTown.ContainsKey(areaIndex)){
			return distancePow;
		}

		foreach(VArtifactTown vat in areaTown[areaIndex]){
			float d = (vat.PosCenter.x-x)*(vat.PosCenter.x-x)+(vat.PosCenter.y-z)*(vat.PosCenter.y-z);
			if(d<distancePow){
				distancePow = d;
				vaTown = vat;
			}
		}
		return Mathf.Sqrt(distancePow);
	} 

	public float GetConnectionLineDistance(IntVector2 pos,bool onConnection=false){
		float distance = float.MaxValue;
		IntVector2 areaIndex = new IntVector2 (pos.x>>LinkAreaIndex,pos.y>>LinkAreaIndex);
		List<VATConnectionLine> townConnectionList = new List<VATConnectionLine> ();
		if(onConnection){
			if(areaTypeConnections.ContainsKey(areaIndex))
				townConnectionList.AddRange(areaTypeConnections[areaIndex]);
		}else{
			if(areaConnections.ContainsKey(areaIndex))
				townConnectionList.AddRange(areaConnections[areaIndex]);
		}
			
		foreach(VATConnectionLine line in townConnectionList)
		{
			IntVector2 startPoint = line.leftPos;
			IntVector2 endPoint = line.rightPos;
			float dist = PointToSegDist(pos.x,pos.y,startPoint.x,startPoint.y,endPoint.x,endPoint.y);
			if(dist<distance)
				distance = dist;
		}
		return distance;
	}

	public int GetTownAmountMin(){
		return TownGenData.GetTownAmountMin();
	}
	public int GetTownAmountMax(){
		return TownGenData.GetTownAmountMax();
	}

	public int GetLevelByLineIndex(int lineIndex){
		return TownGenData.GetLevel(PickedLevelIndex,lineIndex);
	}

	public int GetLevelByAreaId(int areaId){
		return TownGenData.GetLevel(PickedLineIndex,PickedLevelIndex,areaId);
	}

	public int GetLevelByRealPos(IntVector2 realPosXZ){
		//1.get origin pos
		IntVector2 originPos = GetOriginPos(realPosXZ);
		//2.get no
		int areaId = TownGenData.GetAreaId(originPos);
		return TownGenData.GetLevel(PickedLineIndex,PickedLevelIndex,areaId);
	}

	public int GetAreaIdByRealPos(IntVector2 realPosXZ){
		IntVector2 originPos = GetOriginPos(realPosXZ);
		//2.get no
		return TownGenData.GetAreaId(originPos);
	}
	public int GetNextAreaId(int curAreaId){
		List<int> pickedLine = GetPickedGenLine();
		int lineIndex = pickedLine.FindIndex(it=>it==curAreaId)+1;
		if(lineIndex<pickedLine.Count-1)
			return pickedLine[lineIndex+1];
		else
			return -1;
	}

	public int GetGenLineIndexByAreaId(int areaId){
		List<int> pickedLine = GetPickedGenLine();
		return pickedLine.FindIndex(it=>it==areaId);
	}
	#endregion 

	#region mirror&rotation
	public IntVector2 GetRealPos(IntVector2 originPos){
		IntVector2 realPos = new IntVector2 (originPos.x,originPos.y);
		if(Mirror)
			realPos.x=-realPos.x;
		Vector3 v3Pos = new Vector3 (realPos.x,0,realPos.y);
		Quaternion rot = Quaternion.Euler(0,RotationF,0);
		v3Pos = rot*v3Pos;
		realPos.x = Mathf.RoundToInt(v3Pos.x);
		realPos.y = Mathf.RoundToInt(v3Pos.z);
		return realPos;
	}
	public IntVector2 GetOriginPos(IntVector2 realPos){
		Vector3 v3Pos = new Vector3 (realPos.x,0,realPos.y);
		Quaternion rot = Quaternion.Euler(0,-RotationF,0);
		v3Pos = rot*v3Pos;
		IntVector2 originPos = new IntVector2 ();
		originPos.x = Mathf.RoundToInt(v3Pos.x);
		originPos.y = Mathf.RoundToInt(v3Pos.z);
		if(Mirror)
			originPos.x = -originPos.x;
		return originPos;
	}
	#endregion

	#region tool
	public static float PointToSegDist(float x, float y, float x1, float y1, float x2, float y2)
	{
		float cross = (x2 - x1) * (x - x1) + (y2 - y1) * (y - y1);
		if (cross <= 0) return Mathf.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
		
		float d2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
		if (cross >= d2) return Mathf.Sqrt((x - x2) * (x - x2) + (y - y2) * (y - y2));
		
		float r = cross / d2;
		float px = x1 + (x2 - x1) * r;
		float py = y1 + (y2 - y1) * r;
		return Mathf.Sqrt((x - px) * (x - px) + (py - y) * (py - y));
	}

	public void AddTownArea(IntVector2 areaIndex, VArtifactTown vat){
		if(areaTown.ContainsKey(areaIndex)){
			areaTown[areaIndex].Add(vat);
		}else{
			areaTown.Add(areaIndex,new List<VArtifactTown> (){vat});
		}
	}
	public void LinkTownToArea(VArtifactTown vat){
		int maxDistance = Mathf.CeilToInt((vat.MiddleRadius+TownAreaMaxDistance)*TownAreaMaxFactor);
		int minXIndex = (vat.PosCenter.x-maxDistance)>>LinkAreaIndex;
		int minYIndex = (vat.PosCenter.y-maxDistance)>>LinkAreaIndex;
		int maxXIndex = (vat.PosCenter.x+maxDistance)>>LinkAreaIndex;
		int maxYIndex = (vat.PosCenter.y+maxDistance)>>LinkAreaIndex;
		for(int i=minXIndex;i<=maxXIndex;i++){
			for(int j = minYIndex;j<=maxYIndex;j++){
				AddTownArea(new IntVector2(i,j),vat);
			}
		}
	}

	public void AddConnectionArea(IntVector2 areaIndex,VATConnectionLine vatc){
		if(areaConnections.ContainsKey(areaIndex)){
			areaConnections[areaIndex].Add(vatc);
		}else{
			areaConnections.Add(areaIndex,new List<VATConnectionLine> (){vatc});
		}
	}
	public void AddConnectionTypeArea(IntVector2 areaIndex,VATConnectionLine vatc){
		if(areaTypeConnections.ContainsKey(areaIndex)){
			areaTypeConnections[areaIndex].Add(vatc);
		}else{
			areaTypeConnections.Add(areaIndex,new List<VATConnectionLine> (){vatc});
		}
	}
	public void LinkConnectionToArea(VATConnectionLine vatc){
		List<IntVector2> acRect = AreaConnectionRect(vatc,ConnectionAreaWidth);
		int minXIndex = GetMinX(acRect)>>LinkAreaIndex;
		int minYIndex = GetMinY(acRect)>>LinkAreaIndex;
		int maxXIndex = GetMaxX(acRect)>>LinkAreaIndex;
		int maxYIndex = GetMaxY(acRect)>>LinkAreaIndex;
		List<IntVector2> acTypeRect = AreaConnectionRect(vatc,ConnectionAreaTypeWidth);
		for(int i=minXIndex;i<=maxXIndex;i++){
			for(int j = minYIndex;j<=maxYIndex;j++){
				List<IntVector2> areaRect = new List<IntVector2> {
					new IntVector2 (i<<LinkAreaIndex,j<<LinkAreaIndex),
					new IntVector2 ((i+1)<<LinkAreaIndex,(j+1)<<LinkAreaIndex)
				};
				if(i==11&&j==-49)
					Debug.Log("pause");
				if(CheckRectIntersection(acRect,areaRect))
					AddConnectionArea(new IntVector2(i,j),vatc);

				if(CheckRectIntersection(acTypeRect,areaRect))
					AddConnectionTypeArea(new IntVector2(i,j),vatc);
			}
		}
	}

	static int GetMinX(List<IntVector2> posList){
		if(posList==null||posList.Count==0)
		{
			Debug.LogError("VATownGenerator.GetMinx(),no param!");
			return -999999;
		}
		int minX = posList[0].x;
		foreach(IntVector2 pos in posList){
			if(pos.x<minX)
				minX= pos.x;
		}
		return minX;
	}
	static int GetMinY(List<IntVector2> posList){
		if(posList==null||posList.Count==0)
		{
			Debug.LogError("VATownGenerator.GetMinY(),no param!");
			return -999999;
		}
		int minY = posList[0].y;
		foreach(IntVector2 pos in posList){
			if(pos.y<minY)
				minY= pos.y;
		}
		return minY;
	}
	static int GetMaxX(List<IntVector2> posList){
		if(posList==null||posList.Count==0)
		{
			Debug.LogError("VATownGenerator.GetMinx(),no param!");
			return -999999;
		}
		int maxX = posList[0].x;
		foreach(IntVector2 pos in posList){
			if(pos.x>maxX)
				maxX= pos.x;
		}
		return maxX;
	}
	static int GetMaxY(List<IntVector2> posList){
		if(posList==null||posList.Count==0)
		{
			Debug.LogError("VATownGenerator.GetMinx(),no param!");
			return -999999;
		}
		int maxY = posList[0].y;
		foreach(IntVector2 pos in posList){
			if(pos.y>maxY)
				maxY= pos.y;
		}
		return maxY;
	}
	public static List<IntVector2> AreaConnectionRect(VATConnectionLine vatc,float halfWidth){
		List<IntVector2> result = new List<IntVector2> ();
		if(vatc.leftPos.y==vatc.rightPos.y){
			result.Add(new IntVector2 (vatc.leftPos.x,Mathf.FloorToInt(vatc.leftPos.y-halfWidth)));
			result.Add(new IntVector2 (vatc.leftPos.x,Mathf.CeilToInt(vatc.leftPos.y+halfWidth)));
			result.Add(new IntVector2 (vatc.rightPos.x,Mathf.CeilToInt(vatc.rightPos.y+halfWidth)));
			result.Add(new IntVector2 (vatc.rightPos.x,Mathf.FloorToInt(vatc.rightPos.y-halfWidth)));
			return result;
		}
		if(vatc.leftPos.x== vatc.rightPos.x){
			result.Add(new IntVector2 (Mathf.CeilToInt(vatc.leftPos.x+halfWidth),vatc.leftPos.y));
			result.Add(new IntVector2 (Mathf.FloorToInt(vatc.leftPos.x-halfWidth),vatc.leftPos.y));
			result.Add(new IntVector2 (Mathf.FloorToInt(vatc.rightPos.x-halfWidth),vatc.rightPos.y));
            result.Add(new IntVector2 (Mathf.CeilToInt(vatc.rightPos.x+halfWidth),vatc.rightPos.y));
			return result;
		}
		if(vatc.leftPos.y>vatc.rightPos.y){
			float segmentLength = vatc.leftPos.Distance(vatc.rightPos);
			float distX = halfWidth*(vatc.leftPos.y-vatc.rightPos.y)/segmentLength;
			float distY = halfWidth*(vatc.rightPos.x-vatc.leftPos.x)/segmentLength;

			result.Add(new IntVector2 (Mathf.FloorToInt(vatc.leftPos.x-distX),Mathf.FloorToInt(vatc.leftPos.y-distY)));
			result.Add(new IntVector2 (Mathf.CeilToInt(vatc.leftPos.x+distX),Mathf.CeilToInt(vatc.leftPos.y+distY)));
			result.Add(new IntVector2 (Mathf.CeilToInt(vatc.rightPos.x+distX),Mathf.CeilToInt(vatc.rightPos.y+distY)));
			result.Add(new IntVector2 (Mathf.FloorToInt(vatc.rightPos.x-distX),Mathf.FloorToInt(vatc.rightPos.y-distY)));
		}else{
			float segmentLength = vatc.leftPos.Distance(vatc.rightPos);
			float distX = halfWidth*(vatc.rightPos.y-vatc.leftPos.y)/segmentLength;
			float distY = halfWidth*(vatc.rightPos.x-vatc.leftPos.x)/segmentLength;
			
			result.Add(new IntVector2 (Mathf.CeilToInt(vatc.leftPos.x+distX),Mathf.FloorToInt(vatc.leftPos.y-distY)));
			result.Add(new IntVector2 (Mathf.FloorToInt(vatc.leftPos.x-distX),Mathf.CeilToInt(vatc.leftPos.y+distY)));
			result.Add(new IntVector2 (Mathf.FloorToInt(vatc.rightPos.x-distX),Mathf.CeilToInt(vatc.rightPos.y+distY)));
			result.Add(new IntVector2 (Mathf.CeilToInt(vatc.rightPos.x+distX),Mathf.FloorToInt(vatc.rightPos.y-distY)));
		}
		return result;
	}


	//rect01 is a random rect(4 points) ,rect02 is parallel to axis(2 points)
	public static bool CheckRectIntersection(List<IntVector2> rect01,List<IntVector2> rect02){
		//1.segment interact
		for(int i=0;i<4;i++){
			IntVector2 p0 = rect01[i];
			IntVector2 p1;
			if(i<3)
				p1 = rect01[i+1];
			else
				p1 = rect01[0];
			for(int j=0;j<4;j++)
			{
				IntVector2 p2 = rect02[j/2];
				IntVector2 p3;
				if(j%2==0)
					p3 = new IntVector2 (rect02[0].x,rect02[1].y);
				else
					p3 = new IntVector2 (rect02[1].x,rect02[0].y);
				if(CheckLineintersect(p0,p1,p2,p3))
					return true;
			}
		}
		
		//2.Contains
		//a) 01 in 02
		bool result = true;
		for(int i=0;i<4;i++){
			if(!(rect01[i].x<=rect02[1].x&&
			     rect01[i].x>=rect02[0].x&&
			     rect01[i].y<=rect02[1].y&&
			     rect01[i].y>=rect02[0].y
			     )){
				result = false;
				break;
			}
		}
		if(result)
			return true;
		//b) 02 in 01
		result = true;
		for(int i=0;i<4;i++){
			//parallel rect point
			IntVector2 point = new IntVector2 ();
			point.x = rect02[i/2].x;
			point.y = rect02[i%2].y;
			for(int j=0;j<4;j++)
			{
				int v1;
				int v2;
				if(j<3){
					v1 = rect01[j+1].x-rect01[j].x;
					v2 = rect01[j+1].y-rect01[j].y;
				}else{
					v1 = rect01[0].x-rect01[j].x;
					v2 = rect01[0].y-rect01[j].y;
				}
				if(determinant(point.x-rect01[j].x,point.y-rect01[j].y,v1,v2)<0){
					result = false;
					break;
				}
			}
			if(!result)
				break;
		}
		
		return result;
	}
	
	///--start--check line Intersection
	static double determinant(double v1, double v2, double v3, double v4)  // 行列式  叉积
	{  
		return (v1*v4-v2*v3);  
	} 


	public static bool CheckPointOnSegment(IntVector2 aa, IntVector2 bb, IntVector2 p){
		if(p.Equals(aa)||p.Equals(bb))
			return true;
		if(aa.x==bb.x)
		{
			if(p.x!=aa.x)
				return false;
			if((p.y<bb.y&&p.y>aa.y)
			   ||(p.y>bb.y&&p.y<aa.y))
			   return true;
		}
		if(bb.x==p.x)
			return bb.y==p.y;

		if((bb.y-p.y)/(bb.x-p.x)!=(bb.y-aa.y)/(bb.x-aa.x))
			return false;
		if(
			((p.y<bb.y&&p.y>aa.y)
		   ||(p.y>bb.y&&p.y<aa.y))
		   &&
		   ((p.x<bb.x&&p.x>aa.x)
		   ||(p.x>bb.x&&p.y<aa.x))
		   )
				return true;
		return false;
	}
	public static bool CheckLineintersect(IntVector2 aa, IntVector2 bb, IntVector2 cc, IntVector2 dd)  
	{  
		double delta = determinant(bb.x-aa.x, cc.x-dd.x, bb.y-aa.y, cc.y-dd.y);  
		if ( delta<=(1e-6) && delta>=-(1e-6) )  // delta=0，表示两线段重合或平行  
		{
			IntVector2 smallA;
			IntVector2 smallB;
			IntVector2 largeA;
			IntVector2 largeB;
			if(aa.Distance(bb)<=cc.Distance (dd)){
				smallA=aa;
				smallB = bb;
				largeA =cc;
				largeB = dd;
			}else{
				largeA = aa;
				largeB = bb;
				smallA = cc;
				smallB = dd;
			}
			if(CheckPointOnSegment(largeA,largeB,smallA))
				return true;
			if(CheckPointOnSegment(largeA,largeB,smallB))
				return true;
			return false;  
		}  
		double namenda = determinant(cc.x-aa.x, cc.x-dd.x, cc.y-aa.y, cc.y-dd.y) / delta;  
		if ( namenda>1 || namenda<0 )  
		{  
			return false;  
		}  
		double miu = determinant(bb.x-aa.x, cc.x-aa.x, bb.y-aa.y, cc.y-aa.y) / delta;  
		if ( miu>1 || miu<0 )  
		{  
			return false;  
		}  
		return true;  
	} 
	//--end--check line Intersection 
	#endregion
}

