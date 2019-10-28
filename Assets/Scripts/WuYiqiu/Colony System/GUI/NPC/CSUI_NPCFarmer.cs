using UnityEngine;
using System.Collections;

public class CSUI_NPCFarmer : MonoBehaviour
{
    #region UI_WIDGET

    [SerializeField]
    UIPopupList m_ModeUI;
    [SerializeField]
    UISprite m_ManageUI;
    [SerializeField]
    UISprite m_HarvestUI;
    [SerializeField]
    UISprite m_PlantUI;

    [SerializeField]
    UILabel m_FarmUI;
    [SerializeField]
    UILabel m_TotalFamersUI;
    [SerializeField]
    UILabel m_FullTipUI;

    [SerializeField]
    UILabel m_MangeNumUI;
    [SerializeField]
    UILabel m_HarvestNumUI;
    [SerializeField]
    UILabel m_PlantNumUI;

    #endregion

    private CSPersonnel m_RefNpc;
    public CSPersonnel RefNpc
    {
        get { return m_RefNpc; }
        set
        {
            m_RefNpc = value;
            UpdateModeUI();
        }
    }

    public delegate void SelectItemDel(string item);
    public SelectItemDel onSelectChange;

    #region ACTIVE_PART

    private bool m_Active = true;
    public void Activate(bool active)
    {
        if (m_Active != active)
        {
            m_Active = active;
            _activate();
        }
        else
            m_Active = active;
    }

    private void _activate()
    {
        if (!m_Active)
        {
            m_ModeUI.items.Clear();
            if (m_RefNpc != null)
                m_ModeUI.items.Add(CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode));
            else
                m_ModeUI.items.Add("None");
        }
        else
            UpdateModeUI();
    }

    #endregion

    public void Init()
    {
        CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);
    }

    void Awake()
    {
    }

    void OnDestroy()
    {
        CSPersonnel.UnregisterOccupaChangedListener(OnOccupationChange);
    }

    void Start()
    {
        _activate();
    }

    // Update is called once per frame
    void Update()
    {
        if (RefNpc == null)
            return;

        //bool isNeedFuillTip = false;
        CSMgCreator mgCreator = RefNpc.m_Creator as CSMgCreator;
        if (mgCreator != null)
        {
            if (mgCreator.Assembly != null)
            {
                int farmCnt = mgCreator.Assembly.GetEntityCnt(CSConst.ObjectType.Farm);
                int maxFarmCnt = mgCreator.Assembly.GetLimitCnt(CSConst.ObjectType.Farm);

                m_FarmUI.text = "[" + farmCnt.ToString() + "/" + maxFarmCnt.ToString() + "]";

                if (farmCnt != 0)
                {
                    CSFarm farm = mgCreator.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Farm][0] as CSFarm;
                    int curFarmerCnt = farm.WorkerCount;
                    int maxFarmerCnt = farm.WorkerMaxCount;
                    m_TotalFamersUI.text = curFarmerCnt.ToString() + "/" + maxFarmerCnt.ToString();
                    //if (curFarmerCnt >= maxFarmerCnt)
                    //    isNeedFuillTip = true;
                    if (curFarmerCnt >= maxFarmerCnt)
                    {
                        m_TotalFamersUI.color = Color.red;
                    }
                    else
                    {
                        m_TotalFamersUI.color = Color.white;
                    }
                }
                else
                    m_TotalFamersUI.text = "0/0";

                m_MangeNumUI.text = mgCreator.FarmMgNum.ToString();
                m_HarvestNumUI.text = mgCreator.FarmHarvestNum.ToString();
                m_PlantNumUI.text = mgCreator.FarmPlantNum.ToString();
            }
        }

        m_FullTipUI.gameObject.SetActive(false);
    }

    void UpdateModeUI()
    {
        if (!m_Active)
        {
            _activate();
            return;
        }

        m_ModeUI.items.Clear();

        if (m_RefNpc != null)
        {
            m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtFarmForMag));
            m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtFarmForHarvest));
            m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtFarmForPlant));

            m_ModeUI.selection = CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode);
        }
        else
            m_ModeUI.items.Add("None");
    }
    #region CALL_BACK

    bool ShowStatusTips = true;
    void OnSelectionChange(string item)
    {
        if (item == CSUtils.GetWorkModeName(CSConst.pwtFarmForMag))
        {
            m_ManageUI.enabled = true;
            m_HarvestUI.enabled = false;
            m_PlantUI.enabled = false;

            if (m_RefNpc != null)
                m_RefNpc.TrySetWorkMode(CSConst.pwtFarmForMag);

            if (ShowStatusTips)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mFarmerForManage.GetString(), 6f);
        }
        else if (item == CSUtils.GetWorkModeName(CSConst.pwtFarmForHarvest))
        {
            m_ManageUI.enabled = false;
            m_HarvestUI.enabled = true;
            m_PlantUI.enabled = false;

            if (m_RefNpc != null)
                m_RefNpc.TrySetWorkMode(CSConst.pwtFarmForHarvest);

            if (ShowStatusTips)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mFarmerForHarvest.GetString(), 6f);
        }
        else if (item == CSUtils.GetWorkModeName(CSConst.pwtFarmForPlant))
        {
            m_ManageUI.enabled = false;
            m_HarvestUI.enabled = true;
            m_PlantUI.enabled = false;

            if (m_RefNpc != null)
                m_RefNpc.TrySetWorkMode(CSConst.pwtFarmForPlant);

            if (ShowStatusTips)
                CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mFarmerForPlant.GetString(), 6f);
        }

        if (onSelectChange != null)
            onSelectChange(item);
    }

    void OnOccupationChange(CSPersonnel person, int prvState)
    {
        if (person != m_RefNpc)
            return;

        UpdateModeUI();
    }

    void OnPopupListClick()
    {
        if (!m_Active)
            CSUI_StatusBar.ShowText(UIMsgBoxInfo.mCantHandlePersonnel.GetString(), Color.red, 5.5f);
    }
    #endregion
}
