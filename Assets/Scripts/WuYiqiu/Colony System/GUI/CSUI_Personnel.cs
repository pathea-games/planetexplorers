using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using Pathea;
using Pathea.PeEntityExt;
public class CSUI_Personnel : MonoBehaviour
{
    public bool Ishow { get { return gameObject.activeInHierarchy; } }
    // Type
    public enum TypeEnu
    {
        MainLine,
        Other
    }
    private TypeEnu m_Type;
    public TypeEnu Type
    {
        get { return m_Type; }
        set
        {
            m_Type = value;

            if (value == TypeEnu.MainLine)
            {
                m_MainLineRootUI.gameObject.SetActive(true);
                m_OtherRootUI.gameObject.SetActive(false);

                _refreshNPCGrids();
                CSUI_NPCGrid activeGrid = null;
                for (int i = 0; i < m_MainLineRootUI.transform.childCount; ++i)
                {
                    UICheckbox cb = m_MainLineRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
                    if (cb.isChecked)
                    {
                        activeGrid = cb.gameObject.GetComponent<CSUI_NPCGrid>();
                        break;
                    }
                }

                ActiveNpcGrid = activeGrid;
            }
            else if (value == TypeEnu.Other)
            {
                m_MainLineRootUI.gameObject.SetActive(false);
                m_OtherRootUI.gameObject.SetActive(true);

                _refreshNPCGrids();
                CSUI_NPCGrid activeGrid = null;
                for (int i = 0; i < m_OtherRootUI.transform.childCount; ++i)
                {
                    UICheckbox cb = m_OtherRootUI.transform.GetChild(i).gameObject.GetComponent<UICheckbox>();
                    if (cb.isChecked)
                    {
                        activeGrid = cb.gameObject.GetComponent<CSUI_NPCGrid>();
                        break;
                    }
                }

                ActiveNpcGrid = activeGrid;
            }

        }
    }

    // Active NPC Grid
    private CSUI_NPCGrid m_ActiveNpcGrid;

    public CSUI_NPCGrid ActiveNpcGrid
    {
        get { return m_ActiveNpcGrid; }

        set
        {
            m_ActiveNpcGrid = value;
            UpdateNPCRef(m_ActiveNpcGrid == null ? null : m_ActiveNpcGrid.m_Npc);
        }
    }

    public CSPersonnel ActiveNPC
    {
        get
        {
            if (m_ActiveNpcGrid == null)
                return null;
            return 
                m_ActiveNpcGrid.m_Npc;
        }

    }

    #region UI_WIDGET

    // NPC main information & Equip & Occupation UI
    public CSUI_NPCInfo m_NPCInfoUI;
    public CSUI_NPCEquip m_NPCEquipUI;
    public CSUI_NPCOccupation m_NPCOccupaUI;

    // NPC Specific Occupation info UI
    public CSUI_NPCWorker m_NPCWorkerUI;
    public CSUI_NPCFarmer m_NPCFarmerUI;
    public CSUI_NPCSoldier m_NPCSoldierUI;
    public CSUI_NPCFollower m_NPCFollowerUI;
    public CSUI_Processor m_NPCProcessorUI;
    public CSUI_NpcDoctor m_NpcDoctorUI;
    public CSUI_NpcInstructor m_NpcInstructor;

    // NPC member about UI
    public UIGrid m_MainLineRootUI;
    public UIGrid m_OtherRootUI;

    #endregion

    [SerializeField]
    CSUI_NPCGrid m_NpcGridPrefab;



    // NPC member list
    private List<CSUI_NPCGrid> m_MainLineGrids = new List<CSUI_NPCGrid>();
    private List<CSUI_NPCGrid> m_OtherGrids = new List<CSUI_NPCGrid>();

    private enum ENPCGridType
    {
        Dweller = CSConst.potDweller,
        Worker = CSConst.potWorker,
        Farmer = CSConst.potFarmer,
        Soldier = CSConst.potSoldier,
        Follower = CSConst.potFollower,
        Processor = CSConst.potProcessor,
        Doctor = CSConst.potDoctor,
        Instructor = CSConst.potTrainer,
        All
    }

    private ENPCGridType m_NPCType = ENPCGridType.All;


    // Update NPC reference for sub UIs
    void UpdateNPCRef(CSPersonnel npc)
    {
        m_NPCInfoUI.RefNpc = npc;
        m_NPCEquipUI.RefNpc = npc != null ? npc.NPC : null;
        m_NPCOccupaUI.RefNpc = npc;
        m_NPCWorkerUI.RefNpc = npc;
        m_NPCFarmerUI.RefNpc = npc;
        m_NPCSoldierUI.RefNpc = npc;
        m_NPCFollowerUI.RefNpc = npc;
        m_NPCProcessorUI.RefNpc = npc;
        m_NpcDoctorUI.RefNpc = npc;
        m_NpcInstructor.RefNpc = npc;

        if (npc != null)
        {
            if (npc.m_Occupation == CSConst.potWorker)
            {
                m_NPCWorkerUI.gameObject.SetActive(true);
                m_NPCFarmerUI.gameObject.SetActive(false);
                m_NPCSoldierUI.gameObject.SetActive(false);
                m_NPCFollowerUI.gameObject.SetActive(false);
                m_NPCProcessorUI.gameObject.SetActive(false);
                m_NpcDoctorUI.gameObject.SetActive(false);
                m_NpcInstructor.gameObject.SetActive(false);
            }
            else if (npc.m_Occupation == CSConst.potFarmer)
            {
                m_NPCWorkerUI.gameObject.SetActive(false);
                m_NPCFarmerUI.gameObject.SetActive(true);
                m_NPCSoldierUI.gameObject.SetActive(false);
                m_NPCFollowerUI.gameObject.SetActive(false);
                m_NPCProcessorUI.gameObject.SetActive(false);
                m_NpcDoctorUI.gameObject.SetActive(false);
                m_NpcInstructor.gameObject.SetActive(false);
            }
            else if (npc.m_Occupation == CSConst.potSoldier)
            {
                m_NPCWorkerUI.gameObject.SetActive(false);
                m_NPCFarmerUI.gameObject.SetActive(false);
                m_NPCSoldierUI.gameObject.SetActive(true);
                m_NPCFollowerUI.gameObject.SetActive(false);
                m_NPCProcessorUI.gameObject.SetActive(false);
                m_NpcDoctorUI.gameObject.SetActive(false);
                m_NpcInstructor.gameObject.SetActive(false);
            }
            else if (npc.m_Occupation == CSConst.potFollower)
            {
                m_NPCWorkerUI.gameObject.SetActive(false);
                m_NPCFarmerUI.gameObject.SetActive(false);
                m_NPCSoldierUI.gameObject.SetActive(false);
                m_NPCFollowerUI.gameObject.SetActive(true);
                m_NPCProcessorUI.gameObject.SetActive(false);
                m_NpcDoctorUI.gameObject.SetActive(false);
                m_NpcInstructor.gameObject.SetActive(false);
            }
            else if (npc.m_Occupation == CSConst.potProcessor)
            {
                m_NPCWorkerUI.gameObject.SetActive(false);
                m_NPCFarmerUI.gameObject.SetActive(false);
                m_NPCSoldierUI.gameObject.SetActive(false);
                m_NPCFollowerUI.gameObject.SetActive(false);
                m_NPCProcessorUI.gameObject.SetActive(true);
                m_NpcDoctorUI.gameObject.SetActive(false);
                m_NpcInstructor.gameObject.SetActive(false);
            }
            else if (npc.m_Occupation == CSConst.potDoctor)
            {
                m_NPCWorkerUI.gameObject.SetActive(false);
                m_NPCFarmerUI.gameObject.SetActive(false);
                m_NPCSoldierUI.gameObject.SetActive(false);
                m_NPCFollowerUI.gameObject.SetActive(false);
                m_NPCProcessorUI.gameObject.SetActive(false);
                m_NpcDoctorUI.gameObject.SetActive(true);
                m_NpcInstructor.gameObject.SetActive(false);
            }
            else if (npc.m_Occupation == CSConst.potTrainer)
            {
                m_NPCWorkerUI.gameObject.SetActive(false);
                m_NPCFarmerUI.gameObject.SetActive(false);
                m_NPCSoldierUI.gameObject.SetActive(false);
                m_NPCFollowerUI.gameObject.SetActive(false);
                m_NPCProcessorUI.gameObject.SetActive(false);
                m_NpcDoctorUI.gameObject.SetActive(false);
                m_NpcInstructor.gameObject.SetActive(true);
            }
            else
            {
                m_NPCWorkerUI.gameObject.SetActive(false);
                m_NPCFarmerUI.gameObject.SetActive(false);
                m_NPCSoldierUI.gameObject.SetActive(false);
                m_NPCFollowerUI.gameObject.SetActive(false);
                m_NPCProcessorUI.gameObject.SetActive(false);
                m_NpcDoctorUI.gameObject.SetActive(false);
                m_NpcInstructor.gameObject.SetActive(false);
            }

            //lz-2016.09.20 npc在任务中的时候提示在任务中，不能正常工作
            if (npc.m_Occupation != CSConst.potDweller)
            {
               bool canwork = NpcTypeDb.CanRun(npc.NPC.NpcCmpt.NpcControlCmdId,ENpcControlType.Work);
                if (!canwork)
                {
                    CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000703));
                }
            }

        }
        else
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(false);
        }
    }

    // Add a NPC Grid in main UI
    public void AddPersonnel(CSPersonnel npc)
    {
        if (npc == null)
            Debug.LogWarning("The giving npc is null.");

        // Random Npc
        if (npc.IsRandomNpc)
        {
            CSUI_NPCGrid npcGrid = _createNPCGird(npc, m_OtherRootUI.transform);
            m_OtherRootUI.repositionNow = true;
            m_OtherGrids.Add(npcGrid);
        }
        // Main Line Npc
        else
        {
            CSUI_NPCGrid npcGrid = _createNPCGird(npc, m_MainLineRootUI.transform);
            m_MainLineRootUI.repositionNow = true;
            m_MainLineGrids.Add(npcGrid);
        }
        GridRange();
    }


    private CSUI_NPCGrid _createNPCGird(CSPersonnel npc, Transform root)
    {
        CSUI_NPCGrid npcGrid = Instantiate(m_NpcGridPrefab) as CSUI_NPCGrid;
        npcGrid.transform.parent = root;
        CSUtils.ResetLoacalTransform(npcGrid.transform);
        npcGrid.m_UseDeletebutton = true;
        npcGrid.OnDestroySelf = OnNPCGridDestroySelf;

        npcGrid.m_Npc = npc;

        UICheckbox cb = npcGrid.gameObject.GetComponent<UICheckbox>();
        cb.radioButtonRoot = root;

        UIEventListener.Get(npcGrid.gameObject).onActivate = OnNPCGridActive;

        return npcGrid;
    }

    // Remove a NPC grid which reference the npc
    public void RemovePersonnel(CSPersonnel npc)
    {
        if (npc == null)
            Debug.LogWarning("The giving npc is null");

        List<CSUI_NPCGrid> npc_grids_list = null;
        if (npc.IsRandomNpc)
            npc_grids_list = m_OtherGrids;
        else
            npc_grids_list = m_MainLineGrids;
        
        int index = npc_grids_list.FindIndex(item0 => item0.m_Npc == npc);
        if (index != -1)
        {
            bool oldIsChecked = npc_grids_list[index].gameObject.GetComponent<UICheckbox>().isChecked;
            DestroyImmediate(npc_grids_list[index].gameObject);
            npc_grids_list.RemoveAt(index);
            //lz-2016.08.29 删除后要刷新当前应该显示的范围，避免后面还有几排会留空的情况
            GridRange();

            //lz-2016.08.29 重写删除一个npc，选中转移方法
            if (oldIsChecked)
            {
                if (npc_grids_list.Count > 0)
                {
                    int startIndex = mGridPageIndex * NPC_GRID_COUNT;
                    int EndIndex = Mathf.Min(startIndex+NPC_GRID_COUNT - 1, npc_grids_list.Count-1);
                    //1.删除一个npc，它的index会被后面替代，如果存在并没有超出显示范围就选中替代的，超出的话限制在最大或者最小
                    int newIndex = 0;
                    //lz-2016.10.23 错误 #5077 数组越界
                    if (startIndex >= EndIndex)
                        newIndex = EndIndex;
                    else
                        newIndex = Mathf.Clamp(index, startIndex, EndIndex);
                    npc_grids_list[newIndex].gameObject.GetComponent<UICheckbox>().isChecked = true;
                }
                else
                {
                    ActiveNpcGrid = null;
                }
            }
        }
        else
            Debug.LogWarning("The giving npc is not a Settler");
    }



    public void ResetUI()
    {
        foreach (CSUI_NPCGrid mg in m_MainLineGrids)
            GameObject.Destroy(mg.gameObject);

        foreach (CSUI_NPCGrid mg in m_OtherGrids)
            GameObject.Destroy(mg.gameObject);


        ActiveNpcGrid = null;

    }


    #region UNITY_INNER_FUNC

    void OnEnable()
    {
        //lz-2016.10.08 界面重新激活的时候刷新UI
        UpdateNPCRef(m_ActiveNpcGrid == null ? null : m_ActiveNpcGrid.m_Npc);
    }



    void OnDisable()
    {
    }

    void Awake()
    {
        m_NPCWorkerUI.Init();
        m_NPCFarmerUI.Init();
        m_NPCSoldierUI.Init();
        m_NPCFollowerUI.Init();
        m_NPCProcessorUI.Init();
        m_NpcDoctorUI.Init();
        m_NpcInstructor.Init();

        // test code
        //		for (int i=0;i<30;i++)
        //		{
        //			CSUI_NPCGrid npcGrid = _createNPCGird(null, m_MainLineRootUI.transform);
        //			m_MainLineGrids.Add(npcGrid);
        //		}

    }

    // Use this for initialization
    void Start()
    {
        m_NPCOccupaUI.onSelectChange = OnOccupationSelectChange;

    }

    // Update is called once per frame
    void Update()
    {
        if (ActiveNPC != null)
        {
            m_NPCOccupaUI.Activate(ActiveNPC.Running);
            m_NPCWorkerUI.Activate(ActiveNPC.Running);
            m_NPCFarmerUI.Activate(ActiveNPC.Running);
            m_NPCSoldierUI.Activate(ActiveNPC.Running);
            m_NPCFollowerUI.Activate(ActiveNPC.Running);
            m_NPCProcessorUI.Activate(ActiveNPC.Running);
            m_NpcDoctorUI.Activate(ActiveNPC.Running);
            m_NpcInstructor.Activate(ActiveNPC.Running);
        }

        //UpdateGridPos();
    }

    #endregion

    #region CALLBACK

    // CSUI_NPCGrid OnActive call back
    private void OnNPCGridActive(GameObject go, bool actvie)
    {
        if (!actvie) return;

        ActiveNpcGrid = go.GetComponent<CSUI_NPCGrid>();
    }

    // CSUI_NPCOccupation call back
    void OnOccupationSelectChange(string item)
    {
        if (item == CSUtils.GetOccupaName(CSConst.potWorker))
        {
            m_NPCWorkerUI.gameObject.SetActive(true);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(false);
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potFarmer))
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(true);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(false);
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potSoldier))
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(true);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(false);
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potFollower))
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(true);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(false);
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potProcessor))
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(true);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(false);
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potDoctor))
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(true);
            m_NpcInstructor.gameObject.SetActive(false);
        }
        else if (item == CSUtils.GetOccupaName(CSConst.potTrainer))
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(true);
        }
        else
        {
            m_NPCWorkerUI.gameObject.SetActive(false);
            m_NPCFarmerUI.gameObject.SetActive(false);
            m_NPCSoldierUI.gameObject.SetActive(false);
            m_NPCFollowerUI.gameObject.SetActive(false);
            m_NPCProcessorUI.gameObject.SetActive(false);
            m_NpcDoctorUI.gameObject.SetActive(false);
            m_NpcInstructor.gameObject.SetActive(false);
        }
    }

    //void OnNpcStateChangedListener (CSPersonnel csp, int prvState)
    //{
    //    if (ActiveNpcGrid == null || ActiveNpcGrid.m_Npc != csp)
    //        return;

    //    string npcName = csp.Name;
    //    string str = "The " + npcName;
    //    if (csp.State == CSConst.pstIdle)
    //        str += " is wandering aroud.";
    //    else if (csp.State == CSConst.pstPrepare)
    //        str += " is going to destination";
    //    else if (csp.State == CSConst.pstRest)
    //        str += " is resting, he will restore health.";
    //    else if (csp.State == CSConst.pstFollow)
    //        str += " is following you.";
    //    else if (csp.State == CSConst.pstDead)
    //        str += " died";
    //    else if (csp.State == CSConst.pstWork)
    //        str += " is working in the " + csp.WorkRoom.Name;
    //    else if (csp.State == CSConst.pstAtk)
    //        str += " is Fighting for the monster.";
    //    else
    //        str = "";

    //    CSUI_MainWndCtrl.ShowStatusBar(str);
    //}

    // NPC Grid DestroySelf
    void OnNPCGridDestroySelf(CSUI_NPCGrid grid)
    {
        if (grid != null && grid.m_Npc != null)
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000101), grid.m_Npc.KickOut);
    }

    // Settlers : Check buttons CallBack
    void OnAllActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.All;


        _refreshNPCGrids();

        mGridPageIndex = 0;
        GridRange();

    }

    void OnFarmerActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.Farmer;

        _refreshNPCGrids();
        mGridPageIndex = 0;
        GridRange();
    }

    void OnFollowerActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.Follower;

        _refreshNPCGrids();
        mGridPageIndex = 0;
        GridRange();
    }

    void OnSoldierActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.Soldier;

        _refreshNPCGrids();
        mGridPageIndex = 0;
        GridRange();
    }

    void OnWorkerActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.Worker;

        _refreshNPCGrids();
        mGridPageIndex = 0;
        GridRange();
    }

    void OnProcessorActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.Processor;

        _refreshNPCGrids();
        mGridPageIndex = 0;
        GridRange();
    }

    void OnDoctorActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.Doctor;

        _refreshNPCGrids();
        mGridPageIndex = 0;
        GridRange();
    }

    void OnInstructorActivate(bool active)
    {
        if (!active) return;

        m_NPCType = ENPCGridType.Instructor;

        _refreshNPCGrids();
        mGridPageIndex = 0;
        GridRange();
    }

    int mGridPageIndex = 0;
    const int NPC_GRID_COUNT = 11;

    void BtnGridLeftOnClick()
    {
        if (mGridPageIndex <= 0)
            return;

        mGridPageIndex--;

        GridRange();
    }

    void BtnGridRightOnClick()
    {
        List<CSUI_NPCGrid> npc_grids_list = null;
        if (Type == TypeEnu.Other)
            npc_grids_list = m_OtherGrids;
        else
            npc_grids_list = m_MainLineGrids;

        int npcCount = npc_grids_list.Count;
        if (npcCount <= (mGridPageIndex + 1) * NPC_GRID_COUNT)
            return;
        mGridPageIndex++;
        GridRange();

    }

    void UpdateGridPos()
    {
        float pos_x = -mGridPageIndex * 60; //* NPC_GRID_COUNT;

        Transform gridTs = null;
        if (Type == TypeEnu.Other)
            gridTs = m_OtherRootUI.transform;
        else
            gridTs = m_MainLineRootUI.transform;

        gridTs.localPosition = Vector3.Lerp(gridTs.localPosition, new Vector3(pos_x, 0, 0), 0.2f);
        if (Mathf.Abs(gridTs.localPosition.x - pos_x) < 1)
            gridTs.localPosition = new Vector3(pos_x, 0, 0);
    }


    void GridRange()
    {

        List<CSUI_NPCGrid> npc_grids_list = null;
        if (Type == TypeEnu.Other)
            npc_grids_list = m_OtherGrids;
        else
            npc_grids_list = m_MainLineGrids;

        foreach (CSUI_NPCGrid grid0 in npc_grids_list)
        {
            grid0.gameObject.SetActive(false);
            //lz-2016.07.05 翻页的时候把选中状态去掉
            grid0.OnActivate(false);
        }

        for (int i = mGridPageIndex * NPC_GRID_COUNT; i < npc_grids_list.Count; i++)
        {
            if (i >= NPC_GRID_COUNT * (mGridPageIndex + 1))
                break;

            npc_grids_list[i].gameObject.SetActive(true);
            //lz-2016.07.05 翻页的时候如果这一页有被选中的grid，就把这个格子设为选中状态
            if (npc_grids_list[i] == m_ActiveNpcGrid)
            {
                npc_grids_list[i].OnActivate(true);
            }
        }


        if (Type == TypeEnu.Other)
            m_OtherRootUI.repositionNow = true;
        else
            m_MainLineRootUI.repositionNow = true;
    }

    void MainNpcOnActivate(bool active)
    {
        if (active)
            Type = TypeEnu.MainLine;

        GridRange();

    }
    void OtherNpcOnActivate(bool active)
    {
        if (active)
            Type = TypeEnu.Other;

        GridRange();
    }

    // Call by CSUI_main in OnCreatorEventListenerForPersonnel
    public void OnCreatorAddPersennel(CSPersonnel personnel)
    {
        switch (m_NPCType)
        {
            case ENPCGridType.All:
                AddPersonnel(personnel);
                break;
            default:
                if ((int)m_NPCType == personnel.Occupation)
                    AddPersonnel(personnel);
                break;
        }
    }

    public void OnCreatorRemovePersennel(CSPersonnel personnel)
    {
        switch (m_NPCType)
        {
            case ENPCGridType.All:
                RemovePersonnel(personnel);
                break;
            default:
                if ((int)m_NPCType == personnel.Occupation)
                    RemovePersonnel(personnel);
                break;
        }
    }


    #endregion


    void _refreshNPCGrids()
    {
        if (CSUI_MainWndCtrl.Instance == null) return;

        mGridPageIndex = 0;


        List<CSUI_NPCGrid> npc_grids_list = null;
        if (Type == TypeEnu.Other)
        {
            npc_grids_list = m_OtherGrids;
            m_OtherRootUI.gameObject.transform.localPosition = Vector3.zero;
            m_OtherRootUI.repositionNow = true;

        }
        else
        {
            npc_grids_list = m_MainLineGrids;
            m_MainLineRootUI.gameObject.transform.localPosition = Vector3.zero;
            m_MainLineRootUI.repositionNow = true;
        }

        CSPersonnel[] npcs = CSUI_MainWndCtrl.Instance.Creator.GetNpcs();

        int Len = 0;
        if (m_NPCType == ENPCGridType.All)
        {
            for (int i = 0; i < npcs.Length; ++i)
            {
                if (Type == TypeEnu.Other && npcs[i].IsRandomNpc)
                {
                    // Already has a CSUI_NPCGrid? just replace the npc reference!
                    if (Len < npc_grids_list.Count)
                        npc_grids_list[Len].m_Npc = npcs[i];
                    else
                    {
                        CSUI_NPCGrid npcGrid = _createNPCGird(npcs[i], m_OtherRootUI.transform);
                        npc_grids_list.Add(npcGrid);
                    }

                    Len++;
                }
                else if (Type == TypeEnu.MainLine && !npcs[i].IsRandomNpc)
                {
                    // Already has a CSUI_NPCGrid? just replace the npc reference!
                    if (Len < npc_grids_list.Count)
                        npc_grids_list[Len].m_Npc = npcs[i];
                    else
                    {
                        CSUI_NPCGrid npcGrid = _createNPCGird(npcs[i], m_MainLineRootUI.transform);
                        npc_grids_list.Add(npcGrid);
                    }

                    Len++;
                }
            }
        }
        else
        {
            for (int i = 0; i < npcs.Length; ++i)
            {
                if ((int)m_NPCType == npcs[i].Occupation)
                {
                    if (Type == TypeEnu.Other && npcs[i].IsRandomNpc)
                    {
                        // Already has a CSUI_NPCGrid? just replace the npc reference!
                        if (Len < npc_grids_list.Count)
                            npc_grids_list[Len].m_Npc = npcs[i];
                        else
                        {
                            CSUI_NPCGrid npcGrid = _createNPCGird(npcs[i], m_OtherRootUI.transform);
                            npc_grids_list.Add(npcGrid);
                        }

                        Len++;
                    }
                    else if (Type == TypeEnu.MainLine && !npcs[i].IsRandomNpc)
                    {
                        // Already has a CSUI_NPCGrid? just replace the npc reference!
                        if (Len < npc_grids_list.Count)
                            npc_grids_list[Len].m_Npc = npcs[i];
                        else
                        {
                            CSUI_NPCGrid npcGrid = _createNPCGird(npcs[i], m_MainLineRootUI.transform);
                            npc_grids_list.Add(npcGrid);
                        }

                        Len++;
                    }
                }
            }
        }

        // Has redundant grid? just destroy it
        if (Len < npc_grids_list.Count)
        {
            for (int i = Len; i < npc_grids_list.Count; )
            {
                DestroyImmediate(npc_grids_list[i].gameObject);
                npc_grids_list.RemoveAt(i);
            }
        }

        if (npc_grids_list.Count != 0)
        {
            npc_grids_list[0].gameObject.GetComponent<UICheckbox>().isChecked = true;
            OnNPCGridActive(npc_grids_list[0].gameObject, true);
        }
        else
            ActiveNpcGrid = null;

    }

}
