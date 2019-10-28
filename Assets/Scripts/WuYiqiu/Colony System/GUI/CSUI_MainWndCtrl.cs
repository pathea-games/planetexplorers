using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class CSUI_MainWndCtrl : UIBaseWnd
{
    private CSCreator m_Creator;
    public CSCreator Creator
    {
        get
        {
            return m_Creator;
        }

        set
        {
            if (m_Creator != null)
            {
                m_Creator.UnregisterListener(OnCreatorEventListener);
                m_Creator.UnregisterPeronnelListener(OnCreatorEventListenerForPersonnel);
            }

            m_Creator = value;
            if (m_Creator != null)
            {
                RefreshMenu();
                if (isShow)
                {
                    SelectBackupOrDefault();
                }
                m_Creator.RegisterListener(OnCreatorEventListener);
                m_Creator.RegisterPersonnelListener(OnCreatorEventListenerForPersonnel);
            }
        }
    }
    #region CREATOR_EVENT
    void OnCreatorEventListener(int event_type, CSEntity entity)
    {
        RefreshMenu();
        if (event_type == CSConst.cetAddEntity)
        {
            if (entity.m_Type == CSConst.etAssembly)
            {
                CSAssembly assem = entity as CSAssembly;
                MapMaskData data = new MapMaskData();
                data.mDescription = "Colony";
                data.mId = -1;
                data.mIconId = 15;
                data.mPosition = new Vector3(entity.Position.x, entity.Position.y + 4f, entity.Position.z);
                data.mRadius = assem.Radius;
            }
            else if (entity.m_Type == CSConst.etCheck)
            {//检查
                m_Windows.m_HospitalUI.SetCheckIcon();
            }
            else if (entity.m_Type == CSConst.etTreat)
            {//治疗
                m_Windows.m_HospitalUI.SetTreatIcon();
            }
            else if (entity.m_Type == CSConst.etTent)
            {//住院
                m_Windows.m_HospitalUI.SetTentIcon();
            }
            if (this.isShow)
            {
                this.ShowWndPart(entity);
            }
        }
        else if (event_type == CSConst.cetRemoveEntity)
        {
            if (entity.m_Type == CSConst.etAssembly)
            {
            }
            else if (entity.m_Type == CSConst.etCheck)
            {//检查
                m_Windows.m_HospitalUI.ClearCheckIcon();
            }
            else if (entity.m_Type == CSConst.etTreat)
            {//治疗
                m_Windows.m_HospitalUI.ClearTreatIcon();
            }
            else if (entity.m_Type == CSConst.etTent)
            {//住院
                m_Windows.m_HospitalUI.ClearTentIcon();
            }
            if (this.isShow)
            {
                //log:lz-2016.05.26 Hospital中三个医疗设备没有移完就还是显示Hospital
                if (((entity.m_Type == CSConst.etCheck || entity.m_Type == CSConst.etTreat || entity.m_Type == CSConst.etTent) && m_Menu.m_HospitalMI.m_EntityList.Count > 0))
                {
                    ShowWndPart(m_Menu.m_HospitalMI, m_Menu.m_HospitalMI.m_Type);
                }
                //lz-2016.10.17 如果Engineering中的机器移除完，就继续显示Engineering
                else if ((entity.m_Type == CSConst.dtEnhance || entity.m_Type == CSConst.dtRepair || entity.m_Type == CSConst.dtRecyle) && m_Menu.m_EngineeringlMI.m_EntityList.Count > 0)
                {
                    ShowWndPart(m_Menu.m_EngineeringlMI, m_Menu.m_EngineeringlMI.m_Type);
                }
                //lz-2016.10.17 如果PPCoal中的机器移除完，就继续显示PPCoal
                else if ((entity.m_Type == CSConst.dtPowerPlant || entity.m_Type == CSConst.dtppCoal || entity.m_Type == CSConst.dtppSolar || entity.m_Type == CSConst.dtppFusion) && m_Menu.m_PPCoalMI.m_EntityList.Count > 0)
                {
                    ShowWndPart(m_Menu.m_PPCoalMI, m_Menu.m_PPCoalMI.m_Type);
                }
                else
                {
                    ShowWndPart(m_Menu.m_PersonnelMI, m_Menu.m_PersonnelMI.m_Type);
                }
            }
        }
        m_Windows.m_PersonnelUI.m_NPCOccupaUI.UpdatePopupList();
    }

    void OnCreatorEventListenerForPersonnel(int event_type, CSPersonnel p)
    {
        if (event_type == CSConst.cetAddPersonnel)
        {
            m_Windows.m_PersonnelUI.OnCreatorAddPersennel(p);
        }
        else if (event_type == CSConst.cetRemovePersonnel)
        {
            m_Windows.m_PersonnelUI.OnCreatorRemovePersennel(p);
        }

    }

    #endregion


    static private CSUI_MainWndCtrl m_Instance;
    static public CSUI_MainWndCtrl Instance
    {
        get
        {
            return m_Instance;
        }
    }

    // Popup hint
    [SerializeField]
    CSUI_PopupHint m_PopupHintPrefab;

    // Windows 
    [System.Serializable]
    public class WindowPart
    {
        public CSUI_Assembly m_AssemblyUI;
        public CSUI_PPCoal m_PPCoalUI;
        public CSUI_Storage m_StorageUI;
        public CSUI_Engineering m_EngineeringUI;
        public CSUI_Dwellings m_DwellingsUI;
        public CSUI_Farm m_FarmUI;
        public CSUI_Factory m_FactoryUI;
        public CSUI_Personnel m_PersonnelUI;
        public CSUI_TradingPost m_TradingPostUI;
        public CSUI_CollectWnd m_CollectUI;
        public CSUI_TrainMgr m_TrainUI;
        public CSUI_Hospital m_HospitalUI;
    }

    [SerializeField]
    WindowPart m_Windows;

    public CSUI_Assembly AssemblyUI { get { return m_Windows.m_AssemblyUI; } }
    public CSUI_PPCoal PPCoalUI { get { return m_Windows.m_PPCoalUI; } }
    public CSUI_Storage StorageUI { get { return m_Windows.m_StorageUI; } }
    public CSUI_Engineering EngineeringUI { get { return m_Windows.m_EngineeringUI; } }
    public CSUI_Dwellings DwellingsUI { get { return m_Windows.m_DwellingsUI; } }
    public CSUI_Farm FarmUI { get { return m_Windows.m_FarmUI; } }
    public CSUI_Factory FactoryUI { get { return m_Windows.m_FactoryUI; } }
    public CSUI_Personnel PersonnelUI { get { return m_Windows.m_PersonnelUI; } }
    public CSUI_TradingPost TradingPostUI { get { return m_Windows.m_TradingPostUI; } }
    public CSUI_CollectWnd CollectUI { get { return m_Windows.m_CollectUI; } }
    public CSUI_TrainMgr TrainUI { get { return m_Windows.m_TrainUI; } }
    public CSUI_Hospital HospitalUI { get { return m_Windows.m_HospitalUI; } }

    public CSUI_Base m_OptionWnd;
    public CSUI_WorkWnd m_WorkWnd;
    public CSEntity mSelectedEnntity
    {
        set
        {
            m_OptionWnd.m_Entity = value;
            m_WorkWnd.m_Entity = value;
        }
    }
    //[SerializeField]
    //private GameObject mSelectedWndPart = null;
    [SerializeField]
    private CSUI_LeftMenuItem mSelectedMenuItem = null;

    [System.Serializable]
    public class MenuPart
    {
        public UIGrid mLeftMenuGrid;

        public CSUI_LeftMenuItem m_PersonnelMI;
        public CSUI_LeftMenuItem m_AssemblyMI;
        public CSUI_LeftMenuItem m_PPCoalMI;
        public CSUI_LeftMenuItem m_FarmMI;
        public CSUI_LeftMenuItem m_FactoryMI;
        public CSUI_LeftMenuItem m_StorageMI;
        public CSUI_LeftMenuItem m_EngineeringlMI;
        public CSUI_LeftMenuItem m_DwellingsMI;
        public CSUI_LeftMenuItem m_TransactionMI;
        public CSUI_LeftMenuItem m_CollectMI;
        public CSUI_LeftMenuItem m_HospitalMI;
        public CSUI_LeftMenuItem m_TrainingMI;
    }

    [SerializeField]
    public MenuPart m_Menu;

    [SerializeField]
    private List<CSUI_LeftMenuItem> m_ActiveMenuList = new List<CSUI_LeftMenuItem>();

    [SerializeField]
    GameObject mSkillLock = null;

    public float WorkDistance = 100;

    public enum EWorkType
    {
        Working,
        OutOfDistance,
        NoAssembly,
        UnKnown
    }
    private EWorkType m_WorkMode;

    public static EWorkType WorkType { get { if (m_Instance == null) return EWorkType.UnKnown; return m_Instance.m_WorkMode; } }

    public static bool IsWorking(bool bShowMsg = true)
    {
        if (m_Instance == null)
            return false;

        if (m_Instance.m_WorkMode == EWorkType.Working)
            return true;

        if (m_Instance.m_WorkMode == EWorkType.OutOfDistance)
        {
            if (bShowMsg)
                //				ShowStatusBar("You need to use this item within the static field of a settlement!!", Color.red);
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNeedStaticField.GetString(), Color.red);
            return false;
        }
        else if (m_Instance.m_WorkMode == EWorkType.NoAssembly)
        {
            if (bShowMsg)
                //				ShowStatusBar("You need to place an assembly core to make it work!", Color.red);
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNeedAssembly.GetString(), Color.red);
            return false;
        }
        else
            return false;
    }

    private bool IsUnLock(int type)
    {
        if (GameUI.Instance == null || GameUI.Instance.mSkillWndCtrl == null || GameUI.Instance.mSkillWndCtrl._SkillMgr == null)
            return true;
        return GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockColony(type);

    }

    void SelectBackupOrDefault()
    {
        //lz-2016.10.16 如果没有选中的，就默认选中m_PersonnelMI
        if (null == mSelectedMenuItem || !m_ActiveMenuList.Contains(mSelectedMenuItem))
        {
            mSelectedMenuItem = m_Menu.m_PersonnelMI;
        }
        ShowWndPart(mSelectedMenuItem, mSelectedMenuItem.m_Type);
    }

    void RefreshMenu()
    {
        HideAllMenu();
        m_ActiveMenuList.Clear();

        //lz-22016.10.16 Personnel界面无条件开启
        m_Menu.m_PersonnelMI.gameObject.SetActive(true);
        m_ActiveMenuList.Add(m_Menu.m_PersonnelMI);

        bool value = (m_Creator.Assembly != null) ? true : false;
        if (value)
        {
            m_Menu.m_AssemblyMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_AssemblyMI);
        }

        m_Menu.m_AssemblyMI.m_EntityList.Clear();
        m_Menu.m_AssemblyMI.m_EntityList.Add(m_Creator.Assembly);

        // Clear MI EntityList
        m_Menu.m_PPCoalMI.m_EntityList.Clear();
        m_Menu.m_FarmMI.m_EntityList.Clear();
        m_Menu.m_FactoryMI.m_EntityList.Clear();
        m_Menu.m_StorageMI.m_EntityList.Clear();
        m_Menu.m_EngineeringlMI.m_EntityList.Clear();
        m_Menu.m_DwellingsMI.m_EntityList.Clear();
        m_Menu.m_HospitalMI.m_EntityList.Clear();
        m_Menu.m_TrainingMI.m_EntityList.Clear();

        //luwei
        m_Menu.m_TransactionMI.m_EntityList.Clear();
        m_Menu.m_CollectMI.m_EntityList.Clear();

        Dictionary<int, CSCommon> entities = m_Creator.GetCommonEntities();

        foreach (CSCommon entity in entities.Values)
        {
            //			if(!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockColony(entity.m_Type))
            //				continue;
            switch (entity.m_Type)
            {
                // PPCoalMI
                case CSConst.dtPowerPlant:
                case CSConst.dtppCoal:
                case CSConst.dtppSolar:
                case CSConst.dtppFusion: //lz-2016.07.23 添加核电站
                    {
                        m_Menu.m_PPCoalMI.m_EntityList.Add(entity);
                    }
                    break;
                //Farm
                case CSConst.dtFarm:
                    {
                        m_Menu.m_FarmMI.m_EntityList.Add(entity);
                    }
                    break;
                //Factory
                case CSConst.dtFactory:
                    {
                        m_Menu.m_FactoryMI.m_EntityList.Add(entity);
                    }
                    break;
                // Storage
                case CSConst.dtStorage:
                    {
                        m_Menu.m_StorageMI.m_EntityList.Add(entity);
                    }
                    break;
                // Engineering
                case CSConst.dtEnhance:
                case CSConst.dtRepair:
                case CSConst.dtRecyle:
                    {
                        m_Menu.m_EngineeringlMI.m_EntityList.Add(entity);
                    }
                    break;
                //Dwellings
                case CSConst.dtDwelling:
                    {
                        m_Menu.m_DwellingsMI.m_EntityList.Add(entity);
                    }
                    break;

                //Transaction
                case CSConst.dtTrade:
                    {
                        m_Menu.m_TransactionMI.m_EntityList.Add(entity);
                    }
                    break;

                //Collect
                case CSConst.dtProcessing:
                    {
                        m_Menu.m_CollectMI.m_EntityList.Add(entity);
                    }
                    break;
                //Hospital
                case CSConst.dtCheck:
                case CSConst.dtTreat:
                case CSConst.dtTent:
                    {
                        m_Menu.m_HospitalMI.m_EntityList.Add(entity);
                    }
                    break;
                //Train
                case CSConst.dtTrain:
                    {
                        m_Menu.m_TrainingMI.m_EntityList.Add(entity);
                    }
                    break;
            }
        }

        // PPCoalMI
        value = (m_Menu.m_PPCoalMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_PPCoalMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_PPCoalMI);
        }

        //Farm
        value = (m_Menu.m_FarmMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_FarmMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_FarmMI);
        }

        //Factory
        value = (m_Menu.m_FactoryMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_FactoryMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_FactoryMI);
        }

        // Storage
        value = (m_Menu.m_StorageMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_StorageMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_StorageMI);
        }

        //Engineering
        value = (m_Menu.m_EngineeringlMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_EngineeringlMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_EngineeringlMI);
        }

        //Dwellings
        value = (m_Menu.m_DwellingsMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_DwellingsMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_DwellingsMI);
        }

        //Transaction
        value = (m_Menu.m_TransactionMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_TransactionMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_TransactionMI);
        }

        //Collect
        value = (m_Menu.m_CollectMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_CollectMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_CollectMI);
        }

        //Hospital
        value = (m_Menu.m_HospitalMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_HospitalMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_HospitalMI);
        }

        //Train
        value = (m_Menu.m_TrainingMI.m_EntityList.Count) > 0 ? true : false;
        if (value)
        {
            m_Menu.m_TrainingMI.gameObject.SetActive(true);
            m_ActiveMenuList.Add(m_Menu.m_TrainingMI);
        }

        m_Menu.mLeftMenuGrid.repositionNow = true;
    }

    public void HideWndByType(int mWndPartType)
    {
        GameObject wnd = null;
        // Assembly
        if (mWndPartType == CSConst.dtAssembly)
            wnd = m_Windows.m_AssemblyUI.gameObject;
        //etStorage
        else if (mWndPartType == CSConst.dtStorage)
            wnd = m_Windows.m_StorageUI.gameObject;
        //etPowerPlant
        else if (mWndPartType == CSConst.dtppCoal)
            wnd = m_Windows.m_PPCoalUI.gameObject;
        //dtDwelling
        else if (mWndPartType == CSConst.dtDwelling)
            wnd = m_Windows.m_DwellingsUI.gameObject;
        //dtEngineer
        else if (mWndPartType == CSConst.dtEngineer)
            wnd = m_Windows.m_EngineeringUI.gameObject;
        //dtFarm
        else if (mWndPartType == CSConst.dtFarm)
            wnd = m_Windows.m_FarmUI.gameObject;
        //dtFactory
        else if (mWndPartType == CSConst.dtFactory)
            wnd = m_Windows.m_FactoryUI.gameObject;
        //etPersonnel
        else if (mWndPartType == CSConst.dtPersonnel)
            wnd = m_Windows.m_PersonnelUI.gameObject;
        //Transaction
        else if (mWndPartType == CSConst.dtTrade)
            wnd = m_Windows.m_TradingPostUI.gameObject;
        //Collect
        else if (mWndPartType == CSConst.dtProcessing)
            wnd = m_Windows.m_CollectUI.gameObject;
        //Hospital
        else if (mWndPartType == CSConst.dtCheck || mWndPartType == CSConst.dtTreat || mWndPartType == CSConst.dtTent)
            wnd = m_Windows.m_HospitalUI.gameObject;
        //Train
        else if (mWndPartType == CSConst.dtTrain)
            wnd = m_Windows.m_TrainUI.gameObject;
        if (wnd != null)
        {
            wnd.SetActive(false);
        }
    }

    void HideAllMenu()
    {
        m_Menu.m_PersonnelMI.IsSelected = false;
        m_Menu.m_PersonnelMI.gameObject.SetActive(false);

        m_Menu.m_AssemblyMI.IsSelected = false;
        m_Menu.m_AssemblyMI.gameObject.SetActive(false);

        m_Menu.m_PPCoalMI.IsSelected = false;
        m_Menu.m_PPCoalMI.gameObject.SetActive(false);

        m_Menu.m_FarmMI.IsSelected = false;
        m_Menu.m_FarmMI.gameObject.SetActive(false);

        m_Menu.m_FactoryMI.IsSelected = false;
        m_Menu.m_FactoryMI.gameObject.SetActive(false);

        m_Menu.m_StorageMI.IsSelected = false;
        m_Menu.m_StorageMI.gameObject.SetActive(false);

        m_Menu.m_EngineeringlMI.IsSelected = false;
        m_Menu.m_EngineeringlMI.gameObject.SetActive(false);

        m_Menu.m_DwellingsMI.IsSelected = false;
        m_Menu.m_DwellingsMI.gameObject.SetActive(false);

        m_Menu.m_TransactionMI.IsSelected = false;
        m_Menu.m_TransactionMI.gameObject.SetActive(false);

        m_Menu.m_CollectMI.IsSelected = false;
        m_Menu.m_CollectMI.gameObject.SetActive(false);

        m_Menu.m_HospitalMI.IsSelected = false;
        m_Menu.m_HospitalMI.gameObject.SetActive(false);

        m_Menu.m_TrainingMI.IsSelected = false;
        m_Menu.m_TrainingMI.gameObject.SetActive(false);
    }

    //	public bool ExistEntityOfType(int type)
    //	{
    //		bool exist = false;
    //		Dictionary<int, CSCommon> entities = m_Creator.GetCommonEntities();
    //		foreach (CSCommon entity in entities.Values)
    //		{
    //			if (entity.m_Type == type)
    //			{
    //				exist = true;
    //				break;
    //			}
    //		}
    //		return exist;
    //	}
    //
    //	public List<CSEntity> FindEntityOfType(int type)
    //	{
    //		List<CSEntity> entityList = new List<CSEntity>();
    //		entityList.Clear();
    //		Dictionary<int, CSCommon> entities = m_Creator.GetCommonEntities();
    //		foreach (CSCommon entity in entities.Values)
    //		{
    //			if (entity.m_Type == type)
    //				entityList.Add(entity);
    //		}
    //		return entityList;
    //	}



    public void ShowWndPart(CSEntity entity)
    {
        if (!isShow)
        {
            base.Show();
            CheckFirstOpenColony();
        }

        CSUI_LeftMenuItem menu;
        switch (entity.m_Type)
        {
            //assembly
            case CSConst.dtAssembly:
                {
                    menu = m_Menu.m_AssemblyMI;
                }
                break;
            // PPCoalMI
            case CSConst.dtPowerPlant:
            case CSConst.dtppCoal:
            case CSConst.dtppSolar:
            case CSConst.dtppFusion: //lz-2016.09.13 添加核电厂的判断
                {
                    menu = m_Menu.m_PPCoalMI;
                }
                break;
            //Farm
            case CSConst.dtFarm:
                {
                    menu = m_Menu.m_FarmMI;
                }
                break;
            //Factory
            case CSConst.dtFactory:
                {
                    menu = m_Menu.m_FactoryMI;
                }
                break;
            // Storage
            case CSConst.dtStorage:
                {
                    menu = m_Menu.m_StorageMI;
                }
                break;
            // Engineering
            case CSConst.dtEnhance:
            case CSConst.dtRepair:
            case CSConst.dtRecyle:
                {
                    menu = m_Menu.m_EngineeringlMI;
                }
                break;
            //Dwellings
            case CSConst.dtDwelling:
                {
                    menu = m_Menu.m_DwellingsMI;
                }
                break;
            //Hospital
            case CSConst.dtCheck:
            case CSConst.dtTreat:
            case CSConst.dtTent:
                {
                    menu = m_Menu.m_HospitalMI;
                }
                break;
            //Processing
            case CSConst.dtProcessing:
                {
                    menu = m_Menu.m_CollectMI;
                }
                break;
            //Trade
            case CSConst.dtTrade:
                {
                    menu = m_Menu.m_TransactionMI;
                }
                break;
            //Train
            case CSConst.dtTrain:
                {
                    menu = m_Menu.m_TrainingMI;
                }
                break;
            default:
                {
                    menu = null;
                }
                break;
        }
        if (menu == null)
            return;
        ShowWndPart(menu, menu.m_Type, entity);
    }

    [HideInInspector]
    public int mWndPartTag = -1;
    public void ShowWndPart(CSUI_LeftMenuItem menuItem, int mWndPartType, CSEntity selectEntity = null)
    {
        if (!isShow)
        {
            base.Show();
            CheckFirstOpenColony();
        }

        if (null == menuItem || !m_ActiveMenuList.Contains(menuItem))
            menuItem = m_Menu.m_PersonnelMI;

        if (!menuItem.IsSelected)
            menuItem.SelectSprite(true);

        if (mSelectedMenuItem != null && mSelectedMenuItem != menuItem)
        {
            mSelectedMenuItem.IsSelected = false;
        }
        mSelectedMenuItem = menuItem;

        //log:lz-2016.05.26 只要打开都提示是否有电
        if (menuItem.NotHaveAssembly || menuItem.NotHaveElectricity)
        {
            string entityNameStr = CSUtils.GetEntityName(menuItem.m_Type);

            //log:lz-2016.05.26 Hospital里面三个机械要分别提示
            if (menuItem == m_Menu.m_HospitalMI && (m_Menu.m_HospitalMI.NotHaveAssembly || m_Menu.m_HospitalMI.NotHaveElectricity))
            {
                List<string> names = new List<string>();

                if (menuItem.AssemblyLevelInsufficient)
                {
                    names = menuItem.GetNamesByAssemblyLevelInsufficient();
                }
                else if (menuItem.NotHaveAssembly)
                {
                    names = menuItem.GetNamesByNotHaveAssembly();
                }
                else if (menuItem.NotHaveElectricity)
                {
                    names = menuItem.GetNamesByNotHaveElectricity();
                }
                if (null != names && names.Count > 0)
                {
                    entityNameStr = "";
                    for (int i = 0; i < names.Count; i++)
                    {
                        entityNameStr += names[i];
                        entityNameStr += ",";
                    }
                    entityNameStr = entityNameStr.Substring(0, entityNameStr.Length - 1);
                }
            }

            if (!string.IsNullOrEmpty(entityNameStr))
            {
                //lz-2016.06.08 检测顺序为：1.检测基地核心等级是否足够  2.检测是否有基地核心  3.检测是否有电 (注意：这个顺序不能修改，因为核心等级不足会满足没有核心的条件，具体参见menuItem.AssemblyLevelInsufficient的实现)
                if (menuItem.AssemblyLevelInsufficient)
                {
                    ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkAssemblyLevelInsufficient.GetString(), entityNameStr), Color.red);
                }
                else if (menuItem.NotHaveAssembly)
                {
                    ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutAssembly.GetString(), entityNameStr), Color.red);
                }
                else if (menuItem.NotHaveElectricity)
                {
                    ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), entityNameStr), Color.red);
                }
            }
        }

        m_Windows.m_EngineeringUI.CloseLock();
        mWndPartTag = mWndPartType;
        mSkillLock.gameObject.SetActive(!IsUnLock(mWndPartType));
#if UNITY_5
        bool active = isActiveAndEnabled;
#endif
        // Assembly
        if (mWndPartType == CSConst.dtAssembly)
        {
            m_Windows.m_AssemblyUI.gameObject.SetActive(true);

            m_Windows.m_AssemblyUI.SetEntity(m_Creator.Assembly);
            mSelectedEnntity = m_Creator.Assembly;
        }
        //etStorage
        else if (mWndPartType == CSConst.dtStorage)
        {
            m_Windows.m_StorageUI.gameObject.SetActive(true);
            m_Windows.m_StorageUI.Replace(menuItem.m_EntityList);

        }
        //etPowerPlant
        else if (menuItem.m_Type == CSConst.dtppCoal)
        {
            m_Windows.m_PPCoalUI.gameObject.SetActive(true);
            m_Windows.m_PPCoalUI.SetEntityList(menuItem.m_EntityList, selectEntity);
        }
        //dtDwelling
        else if (mWndPartType == CSConst.dtDwelling)
        {
            m_Windows.m_DwellingsUI.gameObject.SetActive(active);
            m_Windows.m_DwellingsUI.SetEntityList(menuItem.m_EntityList);
        }
        //dtEngineer
        else if (mWndPartType == CSConst.dtEngineer)
        {
            m_Windows.m_EngineeringUI.Replace(menuItem.m_EntityList, selectEntity);
            m_Windows.m_EngineeringUI.gameObject.SetActive(active);
        }
        //dtFarm
        else if (menuItem.m_Type == CSConst.dtFarm)
        {
            if (menuItem.m_EntityList.Count >= 1)
            {
                m_Windows.m_FarmUI.gameObject.SetActive(active);
                CSFarm f = menuItem.m_EntityList[0] as CSFarm;
                if (menuItem.m_EntityList.Count > 1)
                    foreach (CSEntity e in menuItem.m_EntityList)
                    {
                        CSFarm tempF = e as CSFarm;
                        if (tempF.IsRunning)
                        {
                            f = tempF;
                            break;
                        }
                    }
                m_Windows.m_FarmUI.SetFarm(f);
                mSelectedEnntity = f;
            }
        }
        //dtFactory
        else if (menuItem.m_Type == CSConst.dtFactory)
        {
            //lz-2016.11.14 错误 #6335 Crush Bug
            if (menuItem.m_EntityList.Count >= 1)
            {
                NGUITools.SetActive(m_Windows.m_FactoryUI.gameObject, active);
                CSEntity f = menuItem.m_EntityList[0];
                if (menuItem.m_EntityList.Count > 1)
                    foreach (CSEntity e in menuItem.m_EntityList)
                    {
                        if (e.IsRunning)
                        {
                            f = e;
                            break;
                        }
                    }
                m_Windows.m_FactoryUI.SetEntity(f);
            }
        }
        //etPersonnel
        else if (mWndPartType == CSConst.dtPersonnel)
        {
            m_Windows.m_PersonnelUI.gameObject.SetActive(true);
        }

        //Transaction
        else if (mWndPartType == CSConst.dtTrade)
        {
            m_Windows.m_TradingPostUI.gameObject.SetActive(true);
            m_Windows.m_TradingPostUI.SetMenu(menuItem);
            //lz-2016.10.31 刷新OptionWnd和WorkWnd的Entity
            if (null!=menuItem&&menuItem.m_EntityList.Count > 0)
            {
                mSelectedEnntity = menuItem.m_EntityList[0];
            }
        }

        //Collect
        else if (mWndPartType == CSConst.dtProcessing)
        {
            m_Windows.m_CollectUI.gameObject.SetActive(true);
            m_Windows.m_CollectUI.UpdateCollect();
            //lz-2016.11.28 错误 #7098 crash bug 
            if (menuItem.m_EntityList.Count > 0)
            {
                CSEntity f = menuItem.m_EntityList[0];
                if (menuItem.m_EntityList.Count > 1)
                    foreach (CSEntity e in menuItem.m_EntityList)
                    {
                        if (e.IsRunning)
                        {
                            f = e;
                            break;
                        }
                    }
                m_Windows.m_CollectUI.SetEnity(f);
            }
            
        }

        //Hospital
        else if (mWndPartType == CSConst.dtCheck || mWndPartType == CSConst.dtTreat || mWndPartType == CSConst.dtTent)
        {
            m_Windows.m_HospitalUI.gameObject.SetActive(true);
            //lw 2017.2.21:治疗的三个机器添加修理功能
            m_Windows.m_HospitalUI.RefleshMechine(m_Windows.m_HospitalUI.m_CheckedPartType, menuItem.m_EntityList,selectEntity);
        }
        //Train
        else if (mWndPartType == CSConst.dtTrain)
        {
            m_Windows.m_TrainUI.gameObject.SetActive(true);
            //lz-2016.01.03 选中的entity切换为训练所
            if (null != menuItem && menuItem.m_EntityList.Count > 0)
            {
                mSelectedEnntity = menuItem.m_EntityList[0];
            }
        }
        //删除bed界面子窗口
        DestroyChildWindowOfBed();
    }


    public static void ShowStatusBar(string text, float time = 4.5F)
    {
        if (m_Instance == null)
            return;

        CSUI_StatusBar.ShowText(text, new Color(0.0f, 0.2f, 1.0f, 0.0f), time);
    }

    public static void ShowStatusBar(string text, Color col, float time = 4.5F)
    {
        if (m_Instance == null)
            return;

        CSUI_StatusBar.ShowText(text, col, time);
    }




    public override void OnCreate()
    {
        m_Instance = this;
        base.OnCreate();
        if (Creator == null)
            Creator = CSMain.GetCreator(CSConst.ciDefMgCamp);

        FactoryUI.Init();

    }

    protected override void InitWindow()
    {
        base.InitWindow();

        m_Menu.m_PersonnelMI.m_Type = CSConst.dtPersonnel;
        m_Menu.m_AssemblyMI.m_Type = CSConst.dtAssembly;
        m_Menu.m_PPCoalMI.m_Type = CSConst.dtppCoal;
        m_Menu.m_FarmMI.m_Type = CSConst.dtFarm;
        m_Menu.m_FactoryMI.m_Type = CSConst.dtFactory;
        m_Menu.m_StorageMI.m_Type = CSConst.dtStorage;
        m_Menu.m_EngineeringlMI.m_Type = CSConst.dtEngineer;
        m_Menu.m_DwellingsMI.m_Type = CSConst.dtDwelling;
        m_Menu.m_TransactionMI.m_Type = CSConst.dtTrade;
        m_Menu.m_CollectMI.m_Type = CSConst.dtProcessing;
        m_Menu.m_HospitalMI.m_Type = CSConst.dtCheck;
        m_Menu.m_TrainingMI.m_Type = CSConst.dtTrain;

        m_Windows.m_StorageUI.Init();
        m_Windows.m_FarmUI.Init();
        m_Windows.m_CollectUI.Init();


        //		if (m_MainLineNpcMI == null)
        //			m_MainLineNpcMI = _addNPCMenuItem(PELocalization.GetString(82230011));
        //		if (m_OthersNpcMI == null)
        //			m_OthersNpcMI  = _addNPCMenuItem(PELocalization.GetString(82230012));

        //		m_Windows.m_FarmUI.Init();

    }


    // Use this for initialization
    void Awake()
    {
        if (m_Instance == null)
            m_Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Pathea.PeCreature.Instance.mainPlayer == null || m_Creator == null)
            m_WorkMode = EWorkType.UnKnown;
        else if (m_Creator.Assembly == null)
            m_WorkMode = EWorkType.NoAssembly;
        else
        {
            Vector3 playerPos = Pathea.PeCreature.Instance.mainPlayer.position;
            Vector3 assmPos = m_Creator.Assembly.Position;
            if ((playerPos - assmPos).sqrMagnitude < WorkDistance * WorkDistance)
                m_WorkMode = EWorkType.Working;
            else
                m_WorkMode = EWorkType.OutOfDistance;
        }
        UpdatePerson();

        //if (CSMain.HasCSAssembly())
        //    mMachinePos = CSMain.GetAssemblyEntity().gameObject.GetComponent<CSAssemblyObject>().transform.GetComponentInParent<PeEntity>().transform.position;

        //离开基地球，关闭基地UI
        //if (null != GameUI.Instance.mMainPlayer && CSMain.HasCSAssembly())
        //{
        //    if (!CSMain.GetAssemblyEntity().InRange(GameUI.Instance.mMainPlayer.position))
        //        OnClose();
        //}

    }

    void UpdatePerson()
    {
        //m_Windows.m_PersonnelUI.gameObject.SetActive(m_Menu.m_PersonnelMI.isChecked);
        //m_Windows.m_AssemblyUI.gameObject.SetActive(m_Menu.m_AssemblyMI.isChecked);
        //m_Windows.m_FactoryUI.gameObject.SetActive(m_Menu.m_FactoryMI.isChecked);
        //m_Windows.m_FarmUI.gameObject.SetActive(m_Menu.m_FarmMI.isChecked);
        //m_Windows.m_DwellingsUI.gameObject.SetActive(m_Menu.m_DwellingsMI.isChecked);
        //m_Windows.m_EngineeringUI.gameObject.SetActive(m_Menu.m_EngineeringlMI.isChecked);
        //m_Windows.m_PPCoalUI.gameObject.SetActive(m_Menu.m_PPCoalMI.isChecked);
        //m_Windows.m_StorageUI.gameObject.SetActive(m_Menu.m_StorageMI.isChecked);
        //m_Windows.m_TransactionUI.gameObject.SetActive(m_Menu.m_TransactionMI.isChecked);
        //m_Windows.m_HospitalUI.gameObject.SetActive(m_Menu.m_HospitalMI.isChecked);
        //m_Windows.m_TrainUI.gameObject.SetActive(m_Menu.m_TrainingMI.isChecked);
    }

    public static CSUI_PopupHint CreatePopupHint(Vector3 pos, Transform parent, Vector3 offset, string text, bool bGreen = true)
    {
        if (m_Instance == null)
            return null;
        if (m_Instance.m_PopupHintPrefab == null)
        {
            return null;
        }

        CSUI_PopupHint ph = GameObject.Instantiate(m_Instance.m_PopupHintPrefab) as CSUI_PopupHint;
        ph.transform.parent = parent;
        ph.transform.position = pos;
        ph.transform.localScale = Vector3.one;
        ph.transform.localPosition = new Vector3(ph.transform.localPosition.x + offset.x, ph.transform.localPosition.y + offset.y, offset.z);
        ph.m_Pos = ph.transform.position;

        ph.Text = text;
        ph.bGreen = bGreen;

        ph.Tween();
        return ph;
    }

    //床界面有个repair或者delete的子窗口，这里是控制其关闭

    [HideInInspector]
    public GameObject m_ChildWindowOfBed = null;

    private void DestroyChildWindowOfBed()
    {
        if (m_ChildWindowOfBed != null)
        {
            Destroy(m_ChildWindowOfBed);
            m_ChildWindowOfBed = null;
        }
    }

    public override void Show()
    {
        base.Show();
        //lz-2016.10.12 基地打开的时候刷新当前应该显示的界面，避免显示已经关闭的功能界面
        RefreshMenu();
        SelectBackupOrDefault();
        CheckFirstOpenColony();
    }

    protected override void OnClose()
    {
        DestroyChildWindowOfBed();
        base.OnClose();
    }

    /// <summary>
    /// 跳转到个人的工作设置页面
    /// </summary>
    public void GoToPersonnelWorkWnd()
    {
        ShowWndPart(m_Menu.m_PersonnelMI, m_Menu.m_PersonnelMI.m_Type);
        m_Windows.m_PersonnelUI.m_NPCInfoUI.m_WorkCk.isChecked = true;
    }

    //lz-2016.06.27 跳转到采集场
    public void GoToCollectWnd(int processIndex)
    {
        //lz-2017.01.04 Npc那边会记录职业，有可能采集场已经关闭了，所以跳转的时候要判断 #错误 #8209
        if (m_Menu.m_CollectMI.m_EntityList.Count > 0)
        {
            ShowWndPart(m_Menu.m_CollectMI, m_Menu.m_CollectMI.m_Type);
            m_Windows.m_CollectUI.SelectProcessByIndex(processIndex);
        }
        else
        {
            //lz-2017.01.04 增加没有采集工厂提示
            ShowStatusBar(PELocalization.GetString(ProcessingConst.NotHaveCollect));
        }
    }

    //lz-2016.10.17 跳转到帮助界面
    private void OnHelpBtnClick()
    {
        if (null != GameUI.Instance && null != GameUI.Instance.mPhoneWnd)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
        }
    }

    /// <summary> 检测第一次打开基地，添加基地所有的引导</summary>
    private void CheckFirstOpenColony()
    {
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID8, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID7, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID6, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID5, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID4, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID3, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID2, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID1, false);
        TutorialData.AddActiveTutorialID(TutorialData.ColonyID0);
    }

}
