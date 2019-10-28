using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class CSUI_Farm : MonoBehaviour
{
    private CSFarm m_Farm;
    public CSFarm Farm { get { return m_Farm; } }

    public CSEntity m_Entity;

    [System.Serializable]
    public class PlantsPart
    {
        public UIGrid m_Root;
        public CSUI_PlantGrid m_PlantGridPrefab;
    }

    [SerializeField]
    PlantsPart m_PlantPart;

    private List<CSUI_PlantGrid> m_PlantGrids = new List<CSUI_PlantGrid>(1);

    // Tools and seeds
    [SerializeField]
    CSUI_Grid m_GridPrefab;
    [SerializeField]
    UIGrid m_SeedsRoot;
    [SerializeField]
    UIGrid m_ToolsRoot;
    [SerializeField]
    UICheckbox m_SequentialPlantingCB;

    private List<CSUI_Grid> m_SeedsGrids = new List<CSUI_Grid>();
    private List<CSUI_Grid> m_ToolsGrids = new List<CSUI_Grid>();

    // Workers
    //[System.Serializable]
    //public class NPCPart
    //{
    //    public UIGrid m_Root;
    //    public CSUI_NPCGrid m_NpcGridPrefab;
    //    public UIButton		m_AuttoSettleBtn;
    //    public UIButton		m_DisbandAllBtn;
    //}

    //[SerializeField] NPCPart m_NPCPart;

    private List<CSUI_NPCGrid> m_NpcGrids = new List<CSUI_NPCGrid>();


    public void SetFarm(CSEntity enti)
    {
        if (enti == null)
        {
            Debug.LogWarning("Reference Entity is null.");
            return;
        }

        m_Entity = enti;
        CSFarm farm = enti as CSFarm;

        m_SequentialPlantingCB.isChecked = farm.Data.m_SequentialPlanting;

        if (farm == m_Farm)
            return;

        if (m_Farm != null)
        {
            m_Farm.CreatePlantEvent -= OnCreatePlant;
            m_Farm.RemovePlantEvent -= OnRemovePlant;
            m_Farm.CreatePlantListEvent -= OnCreatAllPlants;
            m_Farm.ClearAllPlantEvent -= OnClearAllPlants;
            m_Farm.RemoveEventListener(OnEntityEvent);
        }

        m_Farm = farm;

        m_Farm.CreatePlantEvent += OnCreatePlant;
        m_Farm.RemovePlantEvent += OnRemovePlant;
        m_Farm.CreatePlantListEvent += OnCreatAllPlants;
        m_Farm.ClearAllPlantEvent += OnClearAllPlants;
        m_Farm.AddEventListener(OnEntityEvent);

        int i = 0;
        foreach (FarmPlantLogic p in farm.Plants.Values)
        {
            if (i < m_PlantGrids.Count)
            {

            }
            else
            {
                CSUI_PlantGrid pg = _createPlantGrid(p);

                UICheckbox cb = pg.gameObject.GetComponent<UICheckbox>();
                if (i == 0)
                    cb.startsChecked = true;
            }
            i++;
        }

        for (int j = i; j < m_PlantGrids.Count; )
        {
            GameObject.Destroy(m_PlantGrids[j].gameObject);
            m_PlantGrids.RemoveAt(j);
        }

        m_PlantPart.m_Root.repositionNow = true;


        // Seed an tool grid
        for (int j = 0; j < CSFarm.PLANTS_SEEDS_COUNT; j++)
        {
            ItemObject io = m_Farm.GetPlantSeed(j);
            m_SeedsGrids[j].m_Grid.SetItem(io);
        }

        for (int j = 0; j < CSFarm.PLANTS_TOOLS_COUNT; j++)
        {
            ItemObject io = m_Farm.GetPlantTool(j);
            m_ToolsGrids[j].m_Grid.SetItem(io);
        }
    }


    void OnPlantGridDestroySelf(CSUI_PlantGrid plantGrid)
    {
        m_PlantGrids.Remove(plantGrid);
        m_PlantPart.m_Root.repositionNow = true;
    }

    public void Init()
    {
        if (m_SeedsGrids.Count != 0)
            return;

        for (int i = 0; i < CSFarm.PLANTS_SEEDS_COUNT; i++)
        {
            CSUI_Grid grid = _createGrid(m_SeedsRoot.transform, i);
            grid.onCheckItem = OnGridCheckItem;
            grid.OnItemChanged = OnSeedGridItemChanged;
            m_SeedsGrids.Add(grid);
            if (GameConfig.IsMultiMode)
            {
                grid.OnDropItemMulti = OnSeedsDropItemMulti;
                grid.OnLeftMouseClickedMulti = OnSeedsLeftMouseClickedMulti;
                grid.OnRightMouseClickedMulti = OnSeedsRightMouseClickedMulti;
            }
        }

        m_SeedsRoot.repositionNow = true;

        for (int i = 0; i < CSFarm.PLANTS_TOOLS_COUNT; i++)
        {
            CSUI_Grid grid = _createGrid(m_ToolsRoot.transform, i);
            if (i == 0)
            {
                grid.onCheckItem = OnGridCheckItem_ToolWater;
                grid.m_Grid.mScript.spriteName = "blackico_water";
            }
            else if (i == 1)
            {
                grid.onCheckItem = OnGridCheckItem_ToolWeed;
                grid.m_Grid.mScript.spriteName = "blackico_herbicide";
            }
            grid.OnItemChanged = OnToolsGridItemChanged;

            m_ToolsGrids.Add(grid);
            if (GameConfig.IsMultiMode)
            {
                grid.OnDropItemMulti = OnToolsDropItemMulti;
                grid.OnLeftMouseClickedMulti = OnToolsLeftMouseClickedMulti;
                grid.OnRightMouseClickedMulti = OnToolsRightMouseClickedMulti;
            }
        }

        m_ToolsRoot.repositionNow = true;

        // Create npc grid
        //for (int i = 0; i < 8; i ++)
        //{
        //    CSUI_NPCGrid grid = Instantiate(m_NPCPart.m_NpcGridPrefab) as CSUI_NPCGrid;
        //    grid.transform.parent = m_NPCPart.m_Root.transform;
        //    CSUtils.ResetLoacalTransform(grid.transform);
        //    grid.m_UseDeletebutton = false;

        //    UICheckbox cb = grid.gameObject.GetComponent<UICheckbox>();
        //    cb.radioButtonRoot = m_NPCPart.m_Root.transform;
        //    if (i != 0)
        //        cb.startsChecked = false;
        //    m_NpcGrids.Add(grid);
        //}

        //m_NPCPart.m_Root.repositionNow = true;
    }


    CSUI_Grid _createGrid(Transform parent, int index = -1)
    {
        CSUI_Grid grid = Instantiate(m_GridPrefab) as CSUI_Grid;
        grid.transform.parent = parent;
        CSUtils.ResetLoacalTransform(grid.transform);

        grid.m_Index = index;

        return grid;
    }

    CSUI_PlantGrid _createPlantGrid(FarmPlantLogic p)
    {
        CSUI_PlantGrid pg = Instantiate(m_PlantPart.m_PlantGridPrefab) as CSUI_PlantGrid;
        pg.transform.parent = m_PlantPart.m_Root.transform;
        CSUtils.ResetLoacalTransform(pg.transform);
        pg.m_Plant = p;
        pg.OnDestroySelf = OnPlantGridDestroySelf;
        //ItemObject itemObj = ItemMgr.Instance.Get( pg.m_Plant.mInstanceId);
        //if (itemObj != null)
        //{
        string[] iconStr = ItemProto.Mgr.Instance.Get(pg.m_Plant.protoTypeId).icon;
        if (iconStr.Length != 0)
            pg.IconSpriteName = iconStr[0];
        else
            pg.IconSpriteName = "";
        //}
        m_PlantGrids.Add(pg);

        UICheckbox cb = pg.gameObject.GetComponent<UICheckbox>();
        cb.radioButtonRoot = m_PlantPart.m_Root.transform;
        cb.startsChecked = false;

        return pg;
    }

    public void UpdateNpcGrids()
    {
        for (int i = 0; i < m_NpcGrids.Count; i++)
        {
            CSPersonnel worker = m_Farm.Worker(i);
            m_NpcGrids[i].m_Npc = worker;
        }
    }
    #region CALL_BACK

    // CSUI_Grid
    bool OnGridCheckItem(ItemObject item, CSUI_Grid.ECheckItemType check_type)
    {
        if (item == null)
            return true;
        if (PlantInfo.GetPlantInfoByItemId(item.protoId) != null)//--to do: judge if a seed;
            return true;

        CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotPlantSeed.GetString(), item.protoData.GetName()), Color.red);
        return false;
    }

    void OnSeedGridItemChanged(ItemObject item, ItemObject oldItem, int index)
    {
        m_Farm.SetPlantSeed(index, item);

        if (oldItem != null)
        {
            if (item == null)
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), oldItem.protoData.GetName(), CSUtils.GetEntityName(CSConst.etFarm)));
            else if (item == oldItem)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
            else
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(CSConst.etFarm)));
        }
        else if (item != null)
            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(CSConst.etFarm)));
    }

    bool OnGridCheckItem_ToolWater(ItemObject item, CSUI_Grid.ECheckItemType check_type)
    {
        if (item == null)
            return true;

        if (item.protoId == ProtoTypeId.WATER)
            return true;

        CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mOnlyCanPutWater.GetString(), Color.red);
        return false;
    }

    bool OnGridCheckItem_ToolWeed(ItemObject item, CSUI_Grid.ECheckItemType check_type)
    {
        if (item == null)
            return true;

        if (item.protoId == ProtoTypeId.INSECTICIDE)
            return true;

        CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mOnlyCanPutInsecticider.GetString(), Color.red);
        return false;
    }

    void OnToolsGridItemChanged(ItemObject item, ItemObject oldItem, int index)
    {
        m_Farm.SetPlantTool(index, item);

        if (oldItem != null)
        {
            if (item == null)
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), oldItem.protoData.GetName(), CSUtils.GetEntityName(CSConst.etFarm)));
            else if (item == oldItem)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
            else
                CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(CSConst.etFarm)));
        }
        else if (item != null)
            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(CSConst.etFarm)));
    }

    // CSFarm
    void OnCreatePlant(FarmPlantLogic plant)
    {
        if (plant != null)
        {
            _createPlantGrid(plant);
            m_PlantPart.m_Root.repositionNow = true;
        }
    }

    void OnRemovePlant(FarmPlantLogic plant)
    {
        if (plant != null)
        {
            int index = m_PlantGrids.FindIndex(item0 => item0.m_Plant == plant);
            if (index != -1)
            {
                DestroyImmediate(m_PlantGrids[index].gameObject);
                m_PlantGrids.RemoveAt(index);
                m_PlantPart.m_Root.repositionNow = true;
            }
        }
    }

    void OnClearAllPlants()
    {
        for (int i = 0; i < m_PlantGrids.Count; i++)
        {
            DestroyImmediate(m_PlantGrids[i].gameObject);
		}
		m_PlantGrids.Clear();
		m_PlantPart.m_Root.repositionNow = true;
	}
	
	void OnCreatAllPlants(List<FarmPlantLogic> _list)
    {
        if (_list == null)
            return;

        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i] != null)
            {
                _createPlantGrid(_list[i]);
            }
        }
        m_PlantPart.m_Root.repositionNow = true;
    }

    // Auto Settle and Disband all buttons
    void OnAutoSettleBtn()
    {
        if (m_Farm != null)
        {
            m_Farm.AutoSettleWorkers();
            //			CSUI_MainWndCtrl.ShowStatusBar("Auto settle some workers in this machine!");

        }

    }

    void OnDisbandAllBtn()
    {
        if (m_Farm != null)
        {
            m_Farm.ClearWorkers();
            //			CSUI_MainWndCtrl.ShowStatusBar("Disband all workers who work for this machine");
        }
    }

    // Check box: Auto planting and Sequential planting
    void OnSequentialActive(bool active)
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != m_Entity)
            {
                m_Entity._ColonyObj._Network.SetSequentialActive(active);
            }
        }
        else
        {
            //lz-2016.10.14 ¿Õ¶ÔÏóbug
            if (m_Farm != null)
            { 
                m_Farm.Data.m_SequentialPlanting = active;

                if (active)
                    CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mPlantSequence.GetString());
            //			CSUI_MainWndCtrl.ShowStatusBar("The Workers will plant in the sequence.");
            }
        }
    }

    // Farm Event handler
    void OnEntityEvent(int event_type, CSEntity cse, object arg)
    {
        if (event_type == CSConst.eetFarm_OnPlant)
        {
            int index = (int)arg;
            if (m_SeedsGrids[index].m_Grid.Item != null)
            {
                if (m_SeedsGrids[index].m_Grid.Item.GetCount() <= 0)
                    m_SeedsGrids[index].m_Grid.SetItem(null);
            }
        }
    }

    #endregion

    #region UNITY_FUNC

    void OnEnable()
    {
        SetFarm(m_Farm);
    }

#if false

	void OnGUI ()
	{
		if (GUI.Button(new Rect(200, 200, 100, 50), "Plant"))
		{
			Vector3 pos = CSClodMgr.FindCleanClod();
			if (pos != Vector3.zero)
			{
				m_Farm.Plant(pos);
			}
		}
	}

#endif

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNpcGrids();


        m_ToolsGrids[0].m_Grid.mScript.spriteName = "blackico_water";

        m_ToolsGrids[1].m_Grid.mScript.spriteName = "blackico_herbicide";

        //int workCount = m_Farm.WorkerCount;
        //m_NPCPart.m_AuttoSettleBtn.isEnabled = (workCount != m_Farm.WorkerMaxCount);

        //m_NPCPart.m_DisbandAllBtn.isEnabled = (workCount != 0);
    }

    #endregion

    #region MultiMode
    //seed
    public void OnSeedsDropItemMulti(Grid_N grid, int m_Index)
    {
        ItemObject itemObj = SelectItem_N.Instance.ItemObj;
        //lz-2017.02.27 ´íÎó #9130 crash bug
        if (null != itemObj && null!=m_Entity && null!=m_Entity._ColonyObj && null!=m_Entity._ColonyObj._Network)
        {
            m_Entity._ColonyObj._Network.SetPlantSeed(m_Index, itemObj.instanceId);
        }
    }
    public void OnSeedsLeftMouseClickedMulti(Grid_N grid, int m_Index)
    {

    }
    public void OnSeedsRightMouseClickedMulti(Grid_N grid, int m_Index)
    {
        m_Entity._ColonyObj._Network.FetchSeedItem(m_Index);
    }

    //tool
    public void OnToolsDropItemMulti(Grid_N grid, int m_Index)
    {
        ItemObject itemObj = SelectItem_N.Instance.ItemObj;
        //lw_2017_7_14:cursh Bug
        if (null != itemObj && null != m_Entity && null != m_Entity._ColonyObj && null != m_Entity._ColonyObj._Network)
        {
            m_Entity._ColonyObj._Network.SetPlantTool(m_Index, itemObj.instanceId);
        } 
    }
    public void OnToolsLeftMouseClickedMulti(Grid_N grid, int m_Index)
    {

    }
    public void OnToolsRightMouseClickedMulti(Grid_N grid, int m_Index)
    {
        m_Entity._ColonyObj._Network.FetchToolItem(m_Index);
    }


    //result
    public void SetPlantToolResult(bool success, int objId, int index, CSEntity entity)
    {
        if (success)
        {
            if (m_Entity == entity)
            {
                ItemObject item = ItemMgr.Instance.Get(objId);
                ItemObject oldItem = m_ToolsGrids[index].m_Grid.ItemObj;
                CSUI_Grid UIgrid = m_ToolsGrids[index];
                UIgrid.m_Grid.SetItem(item);
                if (UIgrid.OnItemChanged != null)
                    UIgrid.OnItemChanged(item, oldItem, UIgrid.m_Index);
            }
        }
    }

    public void SetPlantSeedResult(bool success, int objId, int index, CSEntity entity)
    {
        if (success)
        {
            if (m_Entity == entity)
            {
                ItemObject item = ItemMgr.Instance.Get(objId);
                ItemObject oldItem = m_SeedsGrids[index].m_Grid.ItemObj;
                CSUI_Grid UIgrid = m_SeedsGrids[index];
                UIgrid.m_Grid.SetItem(item);
                if (UIgrid.OnItemChanged != null)
                    UIgrid.OnItemChanged(item, oldItem, UIgrid.m_Index);
            }
        }
    }

    //fetch result
    public void FetchSeedResult(bool success, int index, CSEntity entity)
    {
        if (success)
        {
            if (m_Entity == entity)
            {
                ItemObject item = null;
                ItemObject oldItem = m_SeedsGrids[index].m_Grid.ItemObj;
                CSUI_Grid UIgrid = m_SeedsGrids[index];
                GameUI.Instance.mItemPackageCtrl.ResetItem();
                UIgrid.m_Grid.SetItem(item);
                if (UIgrid.OnItemChanged != null)
                    UIgrid.OnItemChanged(item, oldItem, UIgrid.m_Index);
            }
        }
    }

    public void FetchToolResult(bool success, int index, CSEntity entity)
    {
        if (success)
        {
            if (m_Entity == entity)
            {
                ItemObject item = null;
                ItemObject oldItem = m_ToolsGrids[index].m_Grid.ItemObj;
                CSUI_Grid UIgrid = m_ToolsGrids[index];
                GameUI.Instance.mItemPackageCtrl.ResetItem();
                UIgrid.m_Grid.SetItem(item);
                if (UIgrid.OnItemChanged != null)
                    UIgrid.OnItemChanged(item, oldItem, UIgrid.m_Index);
            }
        }
    }

    public void SetSequentialActiveResult(bool active, CSEntity entity)
    {
        if (m_Entity == entity)
        {
            m_SequentialPlantingCB.isChecked = m_Farm.Data.m_SequentialPlanting;
            if (active)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mPlantSequence.GetString());
        }
    }
    public void DeleteSeedResult(CSEntity entity, int objId,int index)
    {
        if (m_Entity == entity)
        {
            for (int i = 0; i < m_SeedsGrids.Count; i++)
            {
                if (m_SeedsGrids[i].m_Grid.ItemObj != null && m_ToolsGrids[i].m_Grid.ItemObj.instanceId == objId)
                {
                    m_SeedsGrids[i].m_Grid.SetItem(null);
                   
                }
            }

            //// Event
            ItemAsset.ItemObject io = m_Farm.GetPlantSeed(index);
            if (io != null)
            {
                io.DecreaseStackCount(1);
                if (io.GetCount() <= 0)
                {
                    m_Farm.ExcuteEvent(CSConst.eetFarm_OnPlant, index);
                }
            }

            m_Farm.SetPlantSeed(index, null);
        }
    }
    public void DeleteToolResult(CSEntity entity, int objId)
    {
        if (m_Entity == entity)
        {
            for (int i = 0; i < m_ToolsGrids.Count; i++)
            {
                if (m_ToolsGrids[i].m_Grid.ItemObj != null && m_ToolsGrids[i].m_Grid.ItemObj.instanceId == objId)
                {
                    m_ToolsGrids[i].m_Grid.SetItem(null);
                }
            }
        }
    }
    #endregion

    #region interface

    public void RefreshTools()
    {
        if (m_ToolsGrids.Count == 0)
            return;

        if (m_Farm == null)
            return;

        for (int i = 0; i < CSFarm.PLANTS_TOOLS_COUNT; i++)
        {
            ItemObject io = m_Farm.GetPlantTool(i);
            m_ToolsGrids[i].m_Grid.SetItem(io);
        }
    }

    #endregion
}
