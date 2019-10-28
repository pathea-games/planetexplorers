
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea;
using RandomItem;

public class RandomItemMgr:MonoBehaviour
{
    static RandomItemMgr mInstance;
    public static RandomItemMgr Instance
    {
        get { return mInstance; }
    }
    const float s_maxHeight = 999;
    const float a_maxHeight = 512;
    #region randomItem param
    public const float PASSTIME_T = 30;//30;
    public const float PASSDIST_T = 30;//30;
    public const float GEN_T = 2;
    public const int DISTANCE_MAX = 10;
    public const int INDEX256MAX = 16;//16;
    public const int GEN_RADIUS_MIN = 50;//50;
    public const int GEN_RADIUS_MAX = 100;//100;[min,max)

    public Dictionary<int, List<RandomItemObj>> allRandomItems = new Dictionary<int, List<RandomItemObj>>();
    public Dictionary<IntVector2, List<RandomItemObj>> index256Items = new Dictionary<IntVector2, List<RandomItemObj>>();
    public Dictionary<Vector3, RandomItemObj> mRandomItemsDic = new Dictionary<Vector3, RandomItemObj>();


    public bool generateSwitch = false;
    public PeTrans playerTrans;
    public Vector3 born_pos;
    public Vector3 start_pos;
    public Vector3 last_pos;
    public float timeCounter = 0;
    public float last_time;
    public float timePassed = 0;
    public float distancePassed = 0;
    public int counter = 0;
    #endregion

    #region feces param
    public const float f_PASSTIME_T = 60;//60;
    public const float f_PASSDIST_T = 60;//45;
    public const float f_GEN_T = 2;
    public const int f_DISTANCE_MAX = 10;
    public const int f_INDEX256MAX = 6;//6;
    public const int f_GEN_RADIUS_MIN = 50;//50;
    public const int f_GEN_RADIUS_MAX = 100;//100;[min,max)

    //public Dictionary<int, List<RandomItemObj>> allRandomFeces = new Dictionary<int, List<RandomItemObj>>();
    public Dictionary<IntVector2, List<RandomItemObj>> index256Feces = new Dictionary<IntVector2, List<RandomItemObj>>();


    public bool f_generateSwitch = false;
    public Vector3 f_start_pos;
    public Vector3 f_last_pos;
    public float f_timeCounter = 0;
    public float f_last_time;
    public float f_timePassed = 0;
    public float f_distancePassed = 0;
    public int f_counter = 0;
    #endregion



    void Awake()
    {
        mInstance = this;
		//--to do get SpawnPos
		//PeCreature.Instance.mainPlayerCreatedEventor.Subscribe(InitData);
    }

    void Update()
    {
        //randomItem
        if ((PeGameMgr.IsAdventure&&PeGameMgr.yirdName!=AdventureScene.Dungen.ToString()) || PeGameMgr.IsMultiAdventure || PeGameMgr.IsMultiBuild)
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

				if(PeCreature.Instance.mainPlayer.peTrans.position.y<-100)
					return;

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
					System.Random randSeed = new System.Random((int)System.DateTime.UtcNow.Ticks);
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
                    int boxId;
                    if (IsAreaAvalable(GenPos) && IsTerrainAvailable(GenPos, out pos, out boxId))
                    {
                        TryGenItem(pos, boxId);
                        //if (!PeGameMgr.IsMulti)
                        //{
                        //    generateSwitch = false;
						//}
						generateSwitch = false;
                    } 
                }
                
                counter = 0;
            }
        }

        //feces
		if ((PeGameMgr.IsAdventure&&PeGameMgr.yirdName!=AdventureScene.Dungen.ToString())||PeGameMgr.IsStory)
        {
            f_counter++;
            f_timeCounter += Time.deltaTime;
            if (f_counter > 60)
            {
                if (playerTrans == null)
                {
                    if (PeCreature.Instance.mainPlayer != null && PeCreature.Instance.mainPlayer.peTrans.position != Vector3.zero)
                    {
                        playerTrans = PeCreature.Instance.mainPlayer.peTrans;
                        born_pos = playerTrans.position;
						if(Application.isEditor)
                        	Debug.Log("<color=yellow>feces" + "born_pos" + born_pos + "</color>");
                        f_start_pos = born_pos;
                        f_last_pos = f_start_pos;
                        f_last_time = f_timeCounter;
                    }
                    f_counter = 0;
                    return;
                }
				
				if(PeCreature.Instance.mainPlayer.peTrans.position.y<-100)
					return;

                if (!f_generateSwitch)
                {
                    f_timePassed += f_timeCounter - f_last_time;
                    float moreDistance = Vector2.Distance(new Vector2(playerTrans.position.x, playerTrans.position.z), new Vector2(f_last_pos.x, f_last_pos.z));//--to do:
                    if (moreDistance > f_DISTANCE_MAX)
                    {
                        moreDistance = f_DISTANCE_MAX;
                    }
                    f_distancePassed += moreDistance;
                    f_last_pos = playerTrans.position;
                    f_last_time = f_timeCounter;
                    if (CheckGenerateForFeces(f_timePassed, f_distancePassed))
                    {
                        f_generateSwitch = true;
                        f_timePassed = 0;
                        f_distancePassed = 0;
                    }
                    else
                    {
                        f_counter = 0;
                        return;
                    }
                }


                if (f_generateSwitch)
                {
					System.Random randSeed = new System.Random((int)System.DateTime.UtcNow.Ticks);
                    int distance = f_GEN_RADIUS_MAX - f_GEN_RADIUS_MIN;

                    int xPlus = randSeed.Next(distance * 2) - distance;
                    int zPlus = randSeed.Next(distance * 2) - distance;
                    if (xPlus >= 0)
                        xPlus += f_GEN_RADIUS_MIN;
                    else
                        xPlus = xPlus - f_GEN_RADIUS_MIN + 1;

                    if (zPlus >= 0)
                        zPlus += f_GEN_RADIUS_MIN;
                    else
                        zPlus = zPlus - f_GEN_RADIUS_MIN + 1;

                    IntVector2 GenPos = new IntVector2((int)playerTrans.position.x + xPlus, (int)playerTrans.position.z + zPlus);
                    //--to do areaAvalable
                    Vector3 pos;
                    if (IsAreaAvalableForFeces(GenPos) && IsTerrainAvailableForFeces(GenPos, out pos))
                    {
                        TryGenFeces(pos);
                        //if (!PeGameMgr.IsMulti)
                        //{
                        //    ;
						//}
						f_generateSwitch = false;
                    }
                }
                f_counter = 0;
            }
        }
    }

    //public void InitData(object sender, PeCreature.MainPlayerCreatedArg arg)
    //{
    //    if (PeCreature.Instance.mainPlayer != null)
    //    {
    //        playerTrans = PeCreature.Instance.mainPlayer.peTrans;
    //        born_pos = playerTrans.position;
    //        Debug.LogError("born_pos" + born_pos);
    //        start_pos = born_pos;
    //        last_pos = start_pos;
    //        last_time = timeCounter;

    //    }
    //}

    #region RandomItem func
    public void TryGenItem(Vector3 pos, int boxId,System.Random rand=null)
    {
        if (ContainsPos(pos))
            return;
        //--to do:
        
        //test
        if(PeGameMgr.IsMulti){
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItem,pos,boxId);
        }else{
            //3. new & add
            string path;
            List<ItemIdCount> itemIdNum = RandomItemDataMgr.GenItemDicByBoxId(boxId, out path,rand);
            if (itemIdNum == null)
            {
                Debug.LogError("boxId error: " + boxId);
                return;
            }
            List<ItemIdCount> invalidList = itemIdNum.FindAll(it => it.protoId <= 0 || it.count <= 0 || ItemAsset.ItemProto.Mgr.Instance.Get(it.protoId) == null);
            if (invalidList!=null&&invalidList.Count > 0)
            {
                foreach (ItemIdCount idcount in invalidList)
                {
                    Debug.LogError("randomItem error:" + idcount.protoId + " " + idcount.count);
                    itemIdNum.Remove(idcount);
                }
            }
			if(itemIdNum.Count==0){
				Debug.LogError("empty boxId:"+boxId);
				return;
			}
            int[] items = new int[itemIdNum.Count * 2];
            int index = 0;
            foreach (ItemIdCount item in itemIdNum)
            {
                items[index++] = item.protoId;
                items[index++] = item.count;
            }

            RandomItemObj rio = new RandomItemObj(boxId, pos, items, path);
			if(pos.y>=0){
	            AddToAllItems(rio);
	            AddToIndex256(rio);
			}
            mRandomItemsDic.Add(pos, rio);
            //if(Application.isEditor)
            //    Debug.Log("<color=yellow>A RandomItem is Added!"+pos+"id:"+ boxId+"path:"+path+" </color>");
        } 
    }

    #region interface
    public bool IsTerrainAvailable(IntVector2 GenPos,out Vector3 pos,out int boxId)
    {
        //--to do: 1.random the data
        int height = VFDataRTGen.GetPosHeight(GenPos.x, GenPos.y, true);
        bool inWater = VFDataRTGen.IsSea(height);
        bool inCave = false;
        pos = new Vector3(GenPos.x, height - 2, GenPos.y); 
        RaycastHit hitTest;
        if (Physics.Raycast(pos, Vector3.down, out hitTest, 512, 1 << Pathea.Layer.VFVoxelTerrain))
        {
            inCave = true;
        }
        RandomMapType type = VFDataRTGen.GetXZMapType(GenPos.x, GenPos.x);

        IntVector2 tileIndex =new IntVector2( GenPos.x>>VoxelTerrainConstants._shift,GenPos.y>>VoxelTerrainConstants._shift);
        VArtifactTown vaTown=null;
        if (VArtifactTownManager.Instance != null)
        {
            for (int i = -4; i < 5; i++)
            {
                for (int j = -4; j < 5; j++)
                {
                    vaTown = VArtifactTownManager.Instance.GetTileTown(new IntVector2(tileIndex.x + i, tileIndex.y + j));
                    if (vaTown != null)
                        break;
                }
                if (vaTown != null)
                    break;
            }
        }
        
            

        List<int> genCondition = new List<int>();
        

        if (vaTown != null)
        {
            if (vaTown.type == VArtifactType.NpcTown)
                genCondition.Add(BoxMapTypeInt.NEAR_TOWN);
            else
                genCondition.Add(BoxMapTypeInt.NEAR_CAMP);
        }

        switch (type)
        {
            case RandomMapType.Desert:
                genCondition.Add(BoxMapTypeInt.DESERT);
                break;
            case RandomMapType.Redstone:
                genCondition.Add(BoxMapTypeInt.REDSTONE);
                break;
            default:
                genCondition.Add(BoxMapTypeInt.GRASSLAND);
                break;
        }

        if (inWater)
            genCondition.Add(BoxMapTypeInt.IN_WATER);

        if (inCave)
            genCondition.Add(BoxMapTypeInt.IN_CAVE);


        boxId = -1;
        List<RandomItemBoxInfo> boxInfoList =  RandomItemDataMgr.GetBoxIdByCondition(genCondition,height);
        if(boxInfoList==null||boxInfoList.Count==0)
            return false;
        List<RandomItemBoxInfo> boxInfoAvailable = new List<RandomItemBoxInfo> ();
        foreach (RandomItemBoxInfo rib in boxInfoList)
        {
            if (IsBoxNumAvailable(rib.boxId, rib.boxAmount))
                boxInfoAvailable.Add(rib);
        }
        if (boxInfoAvailable.Count == 0)
            return false;

		RandomItemBoxInfo boxInfo = boxInfoAvailable[new System.Random((int)System.DateTime.UtcNow.Ticks).Next(boxInfoAvailable.Count)];

        boxId = boxInfo.boxId;
        float deep = boxInfo.boxDepth;


        //2.check with boxId
        if (deep <= 0)
        {
            deep = -3;
        }else if(deep<2)
        {
            deep=2;
        }
        pos = new Vector3(GenPos.x, height - deep, GenPos.y);
        if (pos.y < 0)
        {
            pos.y = 0;
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(pos, Vector3.down, out hit, 512, 1 << Pathea.Layer.VFVoxelTerrain))
            {
                pos.y = hit.point.y - boxInfo.boxDepth;
            }
        }

        if (pos.y < 0)
        {
            return false;
        }

        return true;
            
    }

    public bool IsBoxNumAvailable(int boxId)
    {
        int boxNumMax = RandomItemDataMgr.GetBoxAmount(boxId);
        if(!allRandomItems.ContainsKey(boxId))
            return 0<boxNumMax;
        return allRandomItems[boxId].Count < boxNumMax;
    }
    public bool IsBoxNumAvailable(int boxId,int boxAmount)
    {
        //return true;//test
        if (!allRandomItems.ContainsKey(boxId))
            return 0 < boxAmount;
        return allRandomItems[boxId].Count < boxAmount;
    }
    public bool IsAreaAvalable(Vector2 pos)
    {
        IntVector2 index256 = new IntVector2(Mathf.RoundToInt(pos.x) >> 8, Mathf.RoundToInt(pos.y)>>8);
        if (index256Items.ContainsKey(index256))
        {
            return index256Items[index256].Count < INDEX256MAX;
        }
        else
        {
            return true;
        }
    }


    public bool ContainsPos(Vector3 pos)
    {
        return mRandomItemsDic.ContainsKey(pos);
    }
    public RandomItemObj GetRandomItemObj(Vector3 pos)
    {
        if (mRandomItemsDic.ContainsKey(pos))
            return mRandomItemsDic[pos];
        return null;
    }

    public void RemoveRandomItemObj(RandomItemObj riObj)
    {
        if (allRandomItems.ContainsKey(riObj.boxId))
            allRandomItems[riObj.boxId].Remove(riObj);

        IntVector2 index256 = new IntVector2(Mathf.RoundToInt(riObj.Pos.x) >> 8, Mathf.RoundToInt(riObj.Pos.z) >> 8);
        if (index256Items.ContainsKey(index256))
        {
            index256Items[index256].Remove(riObj);
        }

		if (index256Feces.ContainsKey(index256))
		{
			index256Feces[index256].Remove(riObj);
		}
        mRandomItemsDic.Remove(riObj.Pos);
    }

    public void AddToIndex256(RandomItemObj rio)
    {
        IntVector2 index256 = new IntVector2(Mathf.RoundToInt(rio.position.x) >> 8, Mathf.RoundToInt(rio.position.z) >> 8);
        if (!index256Items.ContainsKey(index256))
        {
            index256Items.Add(index256, new List<RandomItemObj>(){rio});
        }else{
            index256Items[index256].Add(rio);
        }
    }
    public void AddToAllItems(RandomItemObj rio)
    {
        if(!allRandomItems.ContainsKey(rio.boxId))
        {
            allRandomItems.Add(rio.boxId,new List<RandomItemObj> (){rio});
        }
        else
        {
            allRandomItems[rio.boxId].Add(rio);
        }
    }


    public void RandomTheItems()
    {

    }

    public bool CheckGenerate(float passedTime, float passedDistance)
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
        if (passedTime * passedDistance / PASSTIME_T / PASSDIST_T>GEN_T)
        {
            flag = true;
        }
        return flag;
    }

    public List<int> RandomWeightIndex(List<int> weightList,int objCount, int pickNum)
    {
        List<int> pickedIndex = new List<int>();
        for (int i = 0; i < objCount; i++)
        {
            pickedIndex.Add(i);
        }
        if (pickNum < objCount)
        {
			System.Random rand = new System.Random((int)System.DateTime.UtcNow.Ticks);
            List<int> tempList = new List<int> (weightList);
            int sum = 0;
            foreach(int weight in tempList){
                sum+=weight;
            }
            for (int i = 0; i < pickNum; i++)
            {
                int num = rand.Next(sum);
                for (int j = 0; j < tempList.Count; j++)
                {
                    num -= tempList[j];
                    if (num < 0)
                    {
                        pickedIndex.Add(j);
                        sum -= tempList[j];
                        tempList[j] = 0;
                        break;
                    }
                }
            }
        }
        return pickedIndex;
    }
    #endregion

    #region multi
    public void AddItmeResult(Vector3 pos, Quaternion rot,int templateId, int[] itemIdNum, string path)
    {
        //3. new & add
        RandomItemObj rio = new RandomItemObj(templateId, pos,rot, itemIdNum, path);
		if(rio.Pos.y>0){
	        AddToAllItems(rio);
	        AddToIndex256(rio);
		}
        mRandomItemsDic[rio.position] = rio;
        //if (Application.isEditor) 
        //    Debug.Log("<color=yellow>A RandomItem is Added!" + pos + " </color>");
    }
	public void AddRareItmeResult(Vector3 pos, Quaternion rot,int templateId, int[] itemIdNum, string path)
	{
		//3. new & add
		RandomItemObj rio = new RandomItemObj(templateId, pos,rot, itemIdNum, path);
		rio.AddRareProto(DunItemId.UNFINISHED_ISO,1);
		mRandomItemsDic[rio.position] = rio;
		if (Application.isEditor) 
			Debug.LogError("<color=yellow>A Rare RandomItem is Added!" + pos + " </color>");
	}
	#endregion
	#endregion
	
	#region
	public void TryGenRareItem(Vector3 pos, int boxId,System.Random rand=null,List<ItemIdCount> specifiedItems=null){
		if (ContainsPos(pos))
			return;

		string path;
		List<ItemIdCount> itemIdNum = RandomItemDataMgr.GenItemDicByBoxId(boxId, out path,rand);
		if (itemIdNum == null)
		{
			Debug.LogError("boxId error: " + boxId);
			itemIdNum.Add(new ItemIdCount (1,1));
		}
		if(specifiedItems!=null)
			itemIdNum.AddRange(specifiedItems);
		List<ItemIdCount> invalidList = itemIdNum.FindAll(it => it.protoId <= 0 || it.count <= 0 || ItemAsset.ItemProto.Mgr.Instance.Get(it.protoId) == null);
		if (invalidList!=null&&invalidList.Count > 0)
		{
			foreach (ItemIdCount idcount in invalidList)
			{
				Debug.LogError("randomItem error:" + idcount.protoId + " " + idcount.count);
				itemIdNum.Remove(idcount);
			}
		}
		
		int[] items = new int[itemIdNum.Count * 2];
		int index = 0;
		foreach (ItemIdCount item in itemIdNum)
		{
			items[index++] = item.protoId;
			items[index++] = item.count;
		}
		
		RandomItemObj rio = new RandomItemObj(boxId, pos, items, path);
		if(pos.y>=0){
			AddToAllItems(rio);
			AddToIndex256(rio);
		}
		rio.AddRareProto(DunItemId.UNFINISHED_ISO,5);
		mRandomItemsDic.Add(pos, rio);
		RandomDungenMgrData.AddRareItem(rio);
	}

	#endregion

	#region Processing func
	public ProcessingResultObj GenProcessingItem(Vector3 pos,int[] items){
		ProcessingResultObj pro = new ProcessingResultObj(pos, items);
		
		pro.TryGenObject();
		return pro;
	}
	public ProcessingResultObj GenProcessingItem(Vector3 pos,Quaternion rot, int[] items){
		ProcessingResultObj pro = new ProcessingResultObj(pos,rot, items);
		
		pro.TryGenObject();
		return pro;
	}
    public void AddItemToManager(RandomItemObj rio)
    {
        if (!mRandomItemsDic.ContainsKey(rio.position))
        {
            mRandomItemsDic.Add(rio.position, rio);
        }
    }
    #endregion

    #region Feces func
    public bool IsTerrainAvailableForFeces(IntVector2 GenPos, out Vector3 pos)
    {
		pos = Vector3.zero;
		if(AIErodeMap.IsInErodeArea2D(new Vector3 (GenPos.x,0,GenPos.y))!=null)
		   return false;
        if (PeGameMgr.IsStory)
        {
            Vector3 checkPos = new Vector3(GenPos.x, s_maxHeight, GenPos.y);
            RaycastHit hit;
            if (Physics.Raycast(checkPos, Vector3.down, out hit, s_maxHeight, 1 << Pathea.Layer.VFVoxelTerrain))
            {
                pos=hit.point+new Vector3(0,1,0);
                if (!VFVoxelWater.self.IsInWater(pos))
                    if (!mRandomItemsDic.ContainsKey(pos))
                        return true;
            }
            return false;
        }
        else if(PeGameMgr.IsAdventure)
        {
            int height = VFDataRTGen.GetPosHeight(GenPos.x, GenPos.y, true);
            pos = new Vector3(GenPos.x, height, GenPos.y);
            bool inWater = VFDataRTGen.IsSea(height);
            if (!inWater)
            {
                RaycastHit hit;
                if (Physics.Raycast(pos+new Vector3(0,2,0), Vector3.down, out hit, a_maxHeight, 1 << Pathea.Layer.VFVoxelTerrain))
                {
                    pos = hit.point;
                }
                pos.y += 1;
                if (!mRandomItemsDic.ContainsKey(pos))
                    return true;
            }
        }
        return false;
    }
    public bool IsAreaAvalableForFeces(Vector2 pos)
    {
        IntVector2 index256 = new IntVector2(Mathf.RoundToInt(pos.x) >> 8, Mathf.RoundToInt(pos.y) >> 8);
        if (index256Feces.ContainsKey(index256))
        {
            return index256Feces[index256].Count < f_INDEX256MAX;
        }
        else
        {
            return true;
        }
    }
    public bool CheckGenerateForFeces(float passedTime, float passedDistance)
    {
        bool flag = false;
        if (passedTime < f_PASSTIME_T)
        {
            return flag;
        }
        if (passedDistance < f_PASSDIST_T)
        {
            return flag;
        }
        if (passedTime * passedDistance / f_PASSTIME_T / f_PASSDIST_T > f_GEN_T)
        {
            flag = true;
        }
        return flag;
    }
    public void TryGenFeces(Vector3 pos)
    {
        
        //itemIdCount[0] = 995;
        //itemIdCount[1] = 1 + Mathf.FloorToInt((float)new System.Random().NextDouble() * 3);
        // = GetModelPath(new System.Random().Next(3));
        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomFeces, pos);
        }
        else
        {
            string modelpath;
            int[] itemIdCount = RandomFecesDataMgr.GenFecesItemIdCount(out modelpath);
			/*RandomItemObj feceObj = */new RandomItemObj("feces", pos, itemIdCount, Quaternion.Euler(0, (new System.Random((int)System.DateTime.UtcNow.Ticks)).Next(360), 0), modelpath);
            if (Application.isEditor)
                Debug.Log("<color=brown>A RandomFeces is Added!" + pos + " </color>");
        }
    }

    public void AddFeces(RandomItemObj rio)
    {
        if (!mRandomItemsDic.ContainsKey(rio.position))
        {
            mRandomItemsDic.Add(rio.position, rio);
        
            IntVector2 index256 = new IntVector2(Mathf.RoundToInt(rio.position.x) >> 8, Mathf.RoundToInt(rio.position.z) >> 8);
            if(!index256Feces.ContainsKey(index256))
                index256Feces[index256] = new List<RandomItemObj>();
            index256Feces[index256].Add(rio);
        }
    }

    public void AddFecesResult(Vector3 pos, Quaternion rot,  int[] itemIdNum)
    {
		//string modelPath = GetModelPath((int)(pos.x+pos.y+pos.z));
		string modelPath = FecesData.GetModelPath((int)(pos.x+pos.y+pos.z));
        /*RandomItemObj rio = */new RandomItemObj("feces", pos, itemIdNum, rot, modelPath);
        //if (Application.isEditor)
        //    Debug.Log("<color=brown>A RandomFeces is Added!" + pos + " </color>");
    }
    #endregion

	#region Factory func
	public void GenFactoryCancel(Vector3 pos,int[] items){
		ProcessingResultObj pro = new ProcessingResultObj(pos, items);
		
		pro.TryGenObject();
	}
	public void GenFactoryCancel(Vector3 pos,Quaternion rot, int[] items){
		ProcessingResultObj pro = new ProcessingResultObj(pos,rot, items);
		
		pro.TryGenObject();
	}
	#endregion
}
