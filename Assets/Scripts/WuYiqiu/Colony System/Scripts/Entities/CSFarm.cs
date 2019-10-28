using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;

public class CSFarm : CSElectric 
{
    public override bool IsDoingJob()
    {
        return IsRunning;
    }
    public const int TOOL_INDEX_WATER = 0;
    public const int TOOL_INDEX_INSECTICIDE = 1;
    public const int PLANTS_SEEDS_COUNT = 12;
    public const int PLANTS_TOOLS_COUNT = 2;
    public const int MAX_WORKER_COUNT = 8;

	public const int CYCLE_MIN_WATER = 100;
	public const int CYCLE_MIN_INSECTICIDE = 100;
	public const int CYCLE_ADD_WATER = 88;
	public const int CYCLE_ADD_INSECTICIDE = 88;

	public float curFarmerGrowRate = 0;
	public float FarmerGrowRate{
		get{return curFarmerGrowRate;}
	}
    const double VarPerOp = 30.0;
    public delegate void PlantEvent(FarmPlantLogic plant);
    public event PlantEvent CreatePlantEvent;
    public event PlantEvent RemovePlantEvent;
	public delegate void PlantListEvent(List<FarmPlantLogic> plant);
	public event PlantListEvent CreatePlantListEvent;
	public delegate void ClearPlantEvent();
	public event ClearPlantEvent ClearAllPlantEvent;

	private	CSFarmData  m_FData;
	public  CSFarmData  Data 	
	{ 
		get { 
			if (m_FData == null)
				m_FData = m_Data as CSFarmData;
			return m_FData; 
		} 
	}

	//  information
	public  CSFarmInfo m_FInfo;
	public  CSFarmInfo Info
	{
		get
		{
			if (m_FInfo == null)
				m_FInfo = m_Info as CSFarmInfo;
			return m_FInfo;
		}
	}

	// Plants list
	private Dictionary<int,FarmPlantLogic>	m_Plants;
	public Dictionary<int, FarmPlantLogic>  Plants { get { return m_Plants; } }
    //private Queue<int>	m_WateringIDs;
    //private Queue<int>	m_CleaningIDs;
    //private Queue<int>	m_DeadIDs;
    //private Queue<int>  m_RipedIDs;

	private List<int> m_WateringIds;
	private List<int> m_CleaningIds;
	private List<int> m_DeadIds;
	private List<int> m_RipedIds;


	public FarmPlantLogic AssignOutWateringPlant () 
	{  
		if ( m_WateringIds.Count == 0 )
			return null;

		FarmPlantLogic p = m_Plants[m_WateringIds[0]];
		//Debug.LogError("water!"+p.mPlantInstanceId);
		m_WateringIds.RemoveAt(0);
		return p;
	}

	public void RestoreWateringPlant (FarmPlantLogic plant)
	{
		if (!m_Plants.ContainsKey(plant.mPlantInstanceId))
			return;

		if (plant.NeedWater)
			m_WateringIds.Add(plant.mPlantInstanceId);
	}

	public FarmPlantLogic AssignOutCleaningPlant ()
	{
		if ( m_CleaningIds.Count == 0)
			return null;

		FarmPlantLogic p = m_Plants[m_CleaningIds[0]];
		m_CleaningIds.RemoveAt(0);
		return p;
	}

	public void RestoreCleaningPlant (FarmPlantLogic plant)
	{
		if (!m_Plants.ContainsKey(plant.mPlantInstanceId))
			return;
		
		if (plant.NeedWater)
			m_CleaningIds.Add(plant.mPlantInstanceId);
	}

	public FarmPlantLogic AssignOutDeadPlant ()
	{
		if ( m_DeadIds.Count == 0)
			return null;
		
		FarmPlantLogic p = m_Plants[m_DeadIds[0]];
		m_DeadIds.RemoveAt(0);
		return p;
	}

	public FarmPlantLogic AssignOutRipePlant ()
	{
		if ( m_RipedIds.Count == 0)
			return null;
		
		FarmPlantLogic p = m_Plants[m_RipedIds[0]];
		m_RipedIds.RemoveAt(0);
		return p;
	}

	public void RestoreRipePlant (FarmPlantLogic plant)
	{
		if (!m_Plants.ContainsKey(plant.mPlantInstanceId))
			return;
		
		if (plant.IsRipe)
			m_RipedIds.Add(plant.mPlantInstanceId);
	}

	//private Dictionary<int, FarmPlantLogic> m_WorkingPlants;
	//private Dictionary<int, int>  m_WorkingPlantsHelp;
	//private Dictionary<int, Vector3> m_WorkingPlantPos;

	//private Dictionary<int, ClodChunk> m_WorkingChunks;


	public CSFarm ()
	{
		m_Type = CSConst.etFarm;

		// Init Workers
		m_Workers = new CSPersonnel[MAX_WORKER_COUNT];

		m_WorkSpaces = new PersonnelSpace[1];
		m_WorkSpaces[0] = new PersonnelSpace(this);

		m_Plants = new Dictionary<int, FarmPlantLogic>();
        //m_WateringIDs = new Queue<int>();
        //m_CleaningIDs = new Queue<int>();
        //m_DeadIDs	  = new Queue<int>();
        //m_RipedIDs    = new Queue<int>();
		//m_WorkingPlants = new Dictionary<int, FarmPlantLogic>();
		//m_WorkingPlantsHelp = new Dictionary<int, int>();
		//m_WorkingPlantPos = new Dictionary<int, Vector3>();

		m_WateringIds = new List<int>();
		m_CleaningIds = new List<int>();
		m_RipedIds 	  = new List<int>();
		m_DeadIds	  = new List<int>();

		//m_WorkingChunks = new Dictionary<int, ClodChunk>();

		m_Grade = CSConst.egtLow;

	}

	public void SetPlantSeed(int index, ItemObject item)
	{
		if (index < 0 || index >= PLANTS_SEEDS_COUNT )
		{
			Debug.LogError("Index is out of range!");
			return;
		}

		if (item != null)
			Data.m_PlantSeeds[index] = item.instanceId;
		else
			Data.m_PlantSeeds.Remove(index);
	}

	public ItemObject GetPlantSeed(int index)
	{
		if (index < 0 || index >= PLANTS_SEEDS_COUNT )
		{
			Debug.LogError("Index is out of range!");
			return null; 
		}

		if (Data.m_PlantSeeds.ContainsKey(index))
			return ItemMgr.Instance.Get(Data.m_PlantSeeds[index]);
		else
			return null;
	}



	public void SetPlantTool(int index, ItemObject item)
	{
		if (index < 0 || index >= PLANTS_TOOLS_COUNT)
		{
			Debug.LogError("Index is out of range");
			return;
		}

		if (item != null)
			Data.m_Tools[index] = item.instanceId;
		else
			Data.m_Tools.Remove(index);
	}

	public ItemObject GetPlantTool(int index)
	{
		if (index < 0 || index >= PLANTS_TOOLS_COUNT )
		{
			Debug.LogError("Index is out of range!");
			return null; 
		}

		if (Data.m_Tools.ContainsKey(index))
			return ItemMgr.Instance.Get(Data.m_Tools[index]);
		else
			return null;
	}

	public bool HasPlantSeed ()
	{
		ItemObject seedItem = null;
		for (int j = 0; j < PLANTS_SEEDS_COUNT; j++)
		{
			ItemObject io = GetPlantSeed(j);
			if (io != null)
			{
				seedItem = io;
				break;
			}
		}

		return seedItem != null;
	}

	public int GetPlantSeedId()
	{
		for (int j = 0; j < PLANTS_SEEDS_COUNT; j++)
		{
			ItemObject io = GetPlantSeed(j);
			if (io != null)
			{
				return io.protoId;

			}
		}

		return -1;
	}

	static int layer = 1 << Pathea.Layer.Building
			| 1 << Pathea.Layer.SceneStatic
			| 1 << Pathea.Layer.NearTreePhysics;

	public bool checkRroundCanPlant(int plantItemid,Vector3 pos)
	{
		Bounds _bo =PlantInfo.GetPlantBounds(plantItemid,pos);

		float radiu = Mathf.Max(_bo.extents.x, _bo.extents.z);
		Collider[] colliders = Physics.OverlapSphere(pos, radiu, layer);
		if(colliders != null && colliders.Length != 0)
			return false;

		foreach(var key in m_Plants.Keys)
		{
			if(_bo.Intersects(m_Plants[key].mPlantBounds))
				return false;
		}
		
		return true;
	}
	
	private int m_PlantSequence = 0;

	public FarmPlantLogic PlantTo (Vector3 pos)
	{
        FarmPlantLogic fpl = null;
		// Sequential planting
		if (Data.m_SequentialPlanting)
		{
			int orgin = m_PlantSequence;
			for (int i = m_PlantSequence; i < PLANTS_SEEDS_COUNT; i++)
			{
				ItemObject io = GetPlantSeed(i);

				if (io != null)
				{
                    fpl=_plant(io, pos, i);
					m_PlantSequence++;
					if (m_PlantSequence >= PLANTS_SEEDS_COUNT)
						m_PlantSequence = PLANTS_SEEDS_COUNT - 1;
					break;
				}
			}

			if (orgin == m_PlantSequence)
			{
				for (int i = 0; i < orgin; i++)
				{
					ItemObject io = GetPlantSeed(i);
					if (io != null)
					{
                        fpl=_plant(io, pos, i);
						m_PlantSequence = i + 1;
						if (m_PlantSequence >= PLANTS_SEEDS_COUNT)
							m_PlantSequence = PLANTS_SEEDS_COUNT - 1;
						break;
					}
				}
			}


		}
		// Normal plant
		else 
		{
			for (int i = 0; i < PLANTS_SEEDS_COUNT; i++)
			{
				ItemObject io = GetPlantSeed(i);

				if (io != null)
				{
                    fpl=_plant(io, pos, i);
					break;
				}
			}
		}
        return fpl;
		//DrawItemManager.Instance.CreateMapItem()
	}

	private FarmPlantLogic _plant(ItemObject io, Vector3 pos, int index)
	{
		//PlantInfo info = PlantInfo.GetPlantInfoByItemId(io.protoId);
//		Vector3 new_pos = new Vector3(pos.x, pos.y, pos.z)
		DragArticleAgent dragItem = DragArticleAgent.PutItemByProroId(io.protoId, pos, Quaternion.identity);
        
        //FarmManager.Instance.CreatePlant(dragItem.itemInstanceId, info.mTypeID, pos);
		io.DecreaseStackCount(1);
		if (io.GetCount() <= 0)
		{
            ItemMgr.Instance.DestroyItem(io.instanceId);
			
			SetPlantSeed(index, null);
		}

		// Event
		ExcuteEvent(CSConst.eetFarm_OnPlant, index);
        return dragItem.itemLogic as FarmPlantLogic;
	}

	#region ENTITY_OVERRIDE_FUNC

	public override void CreateData ()
	{
		CSDefaultData ddata = null;
		bool isNew;
        if (PeGameMgr.IsMulti)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtFarm, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtFarm, ref ddata);
        }
		m_Data = ddata as CSFarmData;

        InitNPC();

		if (isNew)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		}
	}

    public void InitNPC()
    {
        CSMgCreator mgC = m_Creator as CSMgCreator;
        if (mgC != null)
        {
            foreach (CSPersonnel csp in mgC.Farmers)
            {
                if (!AddWorker(csp))
                    continue;
                csp.WorkRoom = this;

            }
        }
    }


	public override void RemoveData ()
	{
        //m_Plants.Clear();
        //m_WateringIDs.Clear();
        //m_CleaningIDs.Clear();
        //m_DeadIDs.Clear();
	}

	public override void ChangeState ()
	{
		base.ChangeState();
		if (m_IsRunning)
		{
			if (FarmManager.Instance == null)
			{
				Debug.Log("FarmManager is missing?");
				return;
			}
			else
			{
				RefreshPlant();
			}
		}
		else
		{
			if (FarmManager.Instance != null)
			{
				FarmManager.Instance.CreatePlantEvent -= OnCreatePlant;
				FarmManager.Instance.RemovePlantEvent -= OnRemovePlant;
				FarmPlantLogic.UnregisterEventListener(PlantEventListener);
			}
		}

	}

	void RefreshPlant(){
		m_Plants.Clear();
		m_WateringIds.Clear();
		m_CleaningIds.Clear();
		m_RipedIds.Clear();
		m_DeadIds.Clear();
		if (ClearAllPlantEvent != null)
			ClearAllPlantEvent();
		Dictionary<int, FarmPlantLogic> temp_plants = new Dictionary<int, FarmPlantLogic>();
		foreach (KeyValuePair<int, FarmPlantLogic> kvp in FarmManager.Instance.mPlantMap)
		{
			if ( Assembly.InRange(kvp.Value.mPos) )
			{
				temp_plants.Add(kvp.Key,  kvp.Value);
			}
		}
		
		List<FarmPlantLogic> newPlants = new List<FarmPlantLogic> ();
		foreach(KeyValuePair<int,FarmPlantLogic> kvp in temp_plants){
			if (kvp.Value.IsRipe)
				m_RipedIds.Add(kvp.Key);
			else if (kvp.Value.mDead)
				m_DeadIds.Add(kvp.Key);
			else
			{
				if (kvp.Value.NeedWater)
					m_WateringIds.Add(kvp.Key);
				if (kvp.Value.NeedClean)
					m_CleaningIds.Add(kvp.Key);
			}
			newPlants.Add(kvp.Value);
		}
		if (CreatePlantListEvent != null)
			CreatePlantListEvent(newPlants);
		m_Plants = temp_plants;
		
		if (FarmManager.Instance != null)
		{
			FarmManager.Instance.CreatePlantEvent -= OnCreatePlant;
			FarmManager.Instance.RemovePlantEvent -= OnRemovePlant;
			FarmPlantLogic.UnregisterEventListener(PlantEventListener);
			
			FarmManager.Instance.CreatePlantEvent += OnCreatePlant;
			FarmManager.Instance.RemovePlantEvent += OnRemovePlant;
			FarmPlantLogic.RegisterEventListener(PlantEventListener);
		}
	}

	int refreshCounter=0;
	public override void Update ()
	{
		base.Update();

		if (Assembly == null)
			return;
		if(refreshCounter%60==0)
			UpdateFarmerGrowRate();

		refreshCounter++;
		if(refreshCounter>=600){
			RefreshPlant();
			refreshCounter=0;
		}
			
	}
	public void UpdateFarmerGrowRate(){
		curFarmerGrowRate = GetWorkerParam();
	}
	public override float GetWorkerParam()
	{
		float workParam = 0;
		foreach (CSPersonnel person in m_Workers)
		{
			if (person != null)
			{
				workParam+=person.GetFarmingSkill;
			}
		}
		return workParam;
	}

	public override bool NeedWorkers ()
	{
		if (m_WateringIds.Count != 0 && GetPlantTool(0) != null)
			return true;

		if (m_CleaningIds.Count != 0 && GetPlantTool(1) != null)
			return true;

		if (m_RipedIds.Count != 0 )
			return true;

		ItemObject seedIo = null;
		for (int i = 0; i < PLANTS_SEEDS_COUNT; i++)
		{
			seedIo = GetPlantSeed(i);
			if (seedIo != null)
				break;
		}

		if (seedIo != null)
		{

		}

			
		return base.NeedWorkers ();
	}

	public override void UpdateDataToUI(){
		if(GameUI.Instance!=null)
			GameUI.Instance.mCSUI_MainWndCtrl.FarmUI.RefreshTools();
	}

	public override List<ItemIdCount> GetRequirements ()
	{
		//--to do:
		List<ItemIdCount> requireItems = new List<ItemIdCount> ();
		if(GetPlantTool(TOOL_INDEX_WATER)==null||GetPlantTool(TOOL_INDEX_WATER).GetCount()<CYCLE_MIN_WATER)
		{
			requireItems.Add(new ItemIdCount (ProtoTypeId.WATER,CYCLE_ADD_WATER));
		}
		if(GetPlantTool(TOOL_INDEX_INSECTICIDE)==null||GetPlantTool(TOOL_INDEX_INSECTICIDE).GetCount()<CYCLE_MIN_INSECTICIDE)
		{
			requireItems.Add(new ItemIdCount (ProtoTypeId.INSECTICIDE,CYCLE_ADD_INSECTICIDE));
		}
		return requireItems;
	}

	public override bool MeetDemand(int protoId,int count){
		if(protoId == ProtoTypeId.WATER)
		{
			ItemObject toolItem = GetPlantTool(TOOL_INDEX_WATER);
			if(toolItem!=null)
				toolItem.IncreaseStackCount(count);
			else
			{
				ItemObject io = ItemMgr.Instance.CreateItem(protoId);
				io.SetStackCount(count);
				SetPlantTool(TOOL_INDEX_WATER, io);
			}
		}
		
		if(protoId == ProtoTypeId.INSECTICIDE)
		{
			ItemObject toolItem = GetPlantTool(TOOL_INDEX_INSECTICIDE);
			if(toolItem!=null)
				toolItem.IncreaseStackCount(count);
			else
			{
				ItemObject io = ItemMgr.Instance.CreateItem(protoId);
				io.SetStackCount(count);
				SetPlantTool(TOOL_INDEX_INSECTICIDE, io);
			}
		}
		UpdateDataToUI();
		return true;
	}

	public override bool MeetDemand(ItemIdCount supplyItem){
		return MeetDemand(supplyItem.protoId,supplyItem.count);
	}

	public override bool MeetDemands(List<ItemIdCount> supplyItems){
		foreach(ItemIdCount iic in supplyItems){
			if(iic.protoId == ProtoTypeId.WATER)
			{
				ItemObject toolItem = GetPlantTool(TOOL_INDEX_WATER);
				if(toolItem!=null)
					toolItem.IncreaseStackCount(iic.count);
				else
				{
					ItemObject io = ItemMgr.Instance.CreateItem(iic.protoId);
					io.SetStackCount(iic.count);
					SetPlantTool(TOOL_INDEX_WATER, io);
				}
			}

			if(iic.protoId == ProtoTypeId.INSECTICIDE)
			{
				ItemObject toolItem = GetPlantTool(TOOL_INDEX_INSECTICIDE);
				if(toolItem!=null)
					toolItem.IncreaseStackCount(iic.count);
				else
				{
					ItemObject io = ItemMgr.Instance.CreateItem(iic.protoId);
					io.SetStackCount(iic.count);
					SetPlantTool(TOOL_INDEX_INSECTICIDE, io);
				}
			}
		}
		UpdateDataToUI();
		return true;
	}
	#endregion

	#region CALL_BACK

	private void OnCreatePlant (FarmPlantLogic plant)
	{
		if (Assembly == null)
			return;

		if (!m_Plants.ContainsKey(plant.mPlantInstanceId) && Assembly.InRange(plant.mPos))
		{
			m_Plants.Add(plant.mPlantInstanceId, plant);
			if (CreatePlantEvent != null)
				CreatePlantEvent(plant);
		}
	}

	private void OnRemovePlant (FarmPlantLogic plant)
	{
		if (RemovePlantEvent != null)
			RemovePlantEvent(plant);
		m_Plants.Remove(plant.mPlantInstanceId);
		m_WateringIds.Remove(plant.mPlantInstanceId);
		m_CleaningIds.Remove(plant.mPlantInstanceId);
		m_RipedIds.Remove(plant.mPlantInstanceId);
		m_DeadIds.Remove(plant.mPlantInstanceId);
	}

	private void PlantEventListener (FarmPlantLogic plant, int event_type)
	{
		if (!m_Plants.ContainsKey(plant.mPlantInstanceId))
			return;

		if ( event_type == FarmPlantLogic.cEvent_NeedWater){
			if(!m_WateringIds.Contains(plant.mPlantInstanceId))
				m_WateringIds.Add(plant.mPlantInstanceId);
		}
		else if (event_type == FarmPlantLogic.cEvent_NoNeedWater)
			m_WateringIds.Remove(plant.mPlantInstanceId);
		else if (event_type == FarmPlantLogic.cEvent_NeedClean){
			if(!m_CleaningIds.Contains(plant.mPlantInstanceId))
				m_CleaningIds.Add(plant.mPlantInstanceId);
		}
		else if (event_type == FarmPlantLogic.cEvent_NoNeedClean)
			m_CleaningIds.Remove(plant.mPlantInstanceId);
		else if (event_type == FarmPlantLogic.cEvent_Dead){		
			if(!m_DeadIds.Contains(plant.mPlantInstanceId))
				m_DeadIds.Add(plant.mPlantInstanceId);
		}
		else if (event_type == FarmPlantLogic.cEvent_Ripe)
			if(!m_RipedIds.Contains(plant.mPlantInstanceId))
				m_RipedIds.Add(plant.mPlantInstanceId); 
	}
	#endregion

} 
 