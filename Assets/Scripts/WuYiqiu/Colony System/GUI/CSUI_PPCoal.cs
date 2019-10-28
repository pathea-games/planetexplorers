using ItemAsset;
using ItemAsset.PackageHelper;
using System.Collections.Generic;
using UnityEngine;
using Pathea;

public class CSUI_PPCoal : MonoBehaviour
{
    private PeEntity player;
    private PlayerPackageCmpt playerPackageCmpt;
    public CSPPCoal m_PPCoal;
    public CSEntity m_Entity;
    // Work about
    [System.Serializable]
    public class WorkPart
    {
        public UILabel m_TimePer;
        public UILabel m_TimeLeft;
        public N_ImageButton m_Button;

        public UIGrid m_Root;
        public CSUI_MaterialGrid m_MatGridPrefab;
    }
    [SerializeField]
    private WorkPart m_Work;

    private List<CSUI_MaterialGrid> m_MatGrids = new List<CSUI_MaterialGrid>();

    // Supply Object
    [System.Serializable]
    public class SupplyPart
    {
        public UIGrid m_Root;
        public CSUI_BuildingIcon m_IconPrefab;
    }
    [SerializeField]
    private SupplyPart m_Supply;

    private List<CSUI_BuildingIcon> m_Icons = new List<CSUI_BuildingIcon>();

    // Charging Item
    [System.Serializable]
    public class ChargePart
    {
        public UIGrid m_Root;
        public CSUI_ChargingGrid m_ChargeGridPrefab;
    }

    [SerializeField]
    private UICheckbox m_HideElectricLine;

    [SerializeField]
    private ChargePart m_Charging;

    private List<CSUI_ChargingGrid> m_ChargingGrids = new List<CSUI_ChargingGrid>();
    public UIGrid mIcoGrid;
    public CSUI_CommonIcon m_CommonIcoPrefab;
    List<CSUI_CommonIcon> mUIPPCoalIcoList = new List<CSUI_CommonIcon>();

    public void SetEntityList(List<CSEntity> entityList, CSEntity selectEntity)
    {
        //lz-2016.10.13 错误 #4045 空对象
        if (null == entityList || null == mUIPPCoalIcoList)
            return;

        for (int i = 0; i < mUIPPCoalIcoList.Count; i++)
        {
            if (i < entityList.Count)
            {
                mUIPPCoalIcoList[i].gameObject.SetActive(true);
                mUIPPCoalIcoList[i].Common = entityList[i] as CSCommon;
            }
            else
            {
                mUIPPCoalIcoList[i].gameObject.SetActive(false);
                mUIPPCoalIcoList[i].Common = null;
            }
        }

        CSEntity curSelectEntity = null;

        //lz-2016.09.13 需要选中的
        if (null != selectEntity)
        {
            curSelectEntity = selectEntity;
        }
        //lz-2016.09.13 上次选中的
        else if (null != m_Entity)
        {
            curSelectEntity = m_Entity;
        }

        //lz - 2016.09.13 上面两个不满足就选第一个
        if (null == curSelectEntity || !entityList.Contains(curSelectEntity))
        {
            curSelectEntity = entityList[0];
        }

        CSUI_CommonIcon icon = mUIPPCoalIcoList.Find(a => a.Common == selectEntity);
        if (null!=icon)
        {
            SetEntity(curSelectEntity);
            icon.mCheckBox.isChecked = true;
        }

		mIcoGrid.repositionNow = true;
    }


	bool  IsEntiyContain(List<CSEntity> entityList)
	{
		return entityList.Contains(m_Entity);
	}

    void OnClickIcoItem(CSEntity enti)
    {
        SetEntity(enti);
    }
    public void SetEntity(CSEntity enti)
    {
        if (enti == null)
        {
            Debug.LogWarning("Reference Entity is null.");
            return;
        }

        m_PPCoal = enti as CSPPCoal;

        if (m_PPCoal == null)
        {
            Debug.LogWarning("Reference Entity is not a PowerPlant Entity.");
            return;
        }

        m_Entity = enti;
        CSUI_MainWndCtrl.Instance.mSelectedEnntity = enti;

        SetFuelMaterial();
        SetSupplies();

        foreach (CSUI_ChargingGrid cg in m_ChargingGrids)
        {
            cg.SetItem(m_PPCoal.GetChargingItem(cg.m_Index));
        }
        m_HideElectricLine.isChecked = !m_PPCoal.bShowElectric;
    }
    void OnHideElectric(bool active)
    {
        if (m_PPCoal == null)
            return;
        //if (!PeGameMgr.IsMulti)
        //{
            m_PPCoal.bShowElectric = !active;
        //}
        //else
        //{
        //    m_PPCoal._ColonyObj._Network.PPC_ShowElectric(!active);
        //}
    }
    void SetFuelMaterial()
    {
        for (int i = 0; i < 1; i++)
        {
            if (i < m_PPCoal.Info.m_WorkedTimeItemID.Count)
            {
                int id = m_PPCoal.Info.m_WorkedTimeItemID[i];
                int cnt = m_PPCoal.Info.m_WorkedTimeItemCnt[i];

                m_MatGrids[i].ItemID = id;
                m_MatGrids[i].MaxCnt = cnt;
            }
            else
            {
                m_MatGrids[i].ItemID = 0;
                m_MatGrids[i].MaxCnt = 0;
            }
        }
    }

    void UpdateWorkingTime()
    {        
//        float restTime = Mathf.Max(m_PPCoal.Data.m_WorkedTime - m_PPCoal.Data.m_CurWorkedTime, 0);
//        float percent = restTime / m_PPCoal.Data.m_WorkedTime;
		float restTime = m_PPCoal.RestTime;
		float percent = m_PPCoal.RestPercent;

        m_Work.m_TimePer.text = ((int)(percent * 100)).ToString() + " %";
        m_Work.m_TimeLeft.text = CSUtils.GetRealTimeMS((int)restTime);

        bool canAdd = true;
        foreach (CSUI_MaterialGrid mg in m_MatGrids)
        {
            if (mg.ItemID != 0)
            {
                mg.NeedCnt = Mathf.Max(1, Mathf.RoundToInt(mg.MaxCnt * (1 - percent)));
                mg.ItemNum = playerPackageCmpt.package.GetCount(mg.ItemID);
                if (mg.NeedCnt > mg.ItemNum)
                    canAdd = false;
            }
            else
                mg.ItemNum = -1;
        }

        m_Work.m_Button.isEnabled = canAdd;

    }

    void SetSupplies()
    {
        foreach (CSUI_BuildingIcon ic in m_Icons)
            DestroyImmediate(ic.gameObject);
        m_Icons.Clear();

        int storageCnt = 0, bedCnt = 0;
        CSUI_BuildingIcon storageBI = null, bedBI = null;
        foreach (CSElectric cse in m_PPCoal.m_Electrics)
        {
            if (cse.m_Type == CSConst.etStorage)
            {
                if (storageCnt == 0)
                    storageBI = _createIcons(cse);
                storageCnt++;
            }
            else if (cse.m_Type == CSConst.etDwelling)
            {
                if (bedCnt == 0)
                    bedBI = _createIcons(cse);
                bedCnt++;
            }
            else
                _createIcons(cse);

        }

        m_Supply.m_Root.repositionNow = true;


        if (storageBI != null)
            storageBI.Description += " X " + storageCnt.ToString();
        if (bedBI != null)
            bedBI.Description += " X " + bedBI.ToString();

    }

    CSUI_BuildingIcon _createIcons(CSElectric cse)
    {
        CSUI_BuildingIcon bi = Instantiate(m_Supply.m_IconPrefab) as CSUI_BuildingIcon;
        bi.transform.parent = m_Supply.m_Root.transform;
        bi.transform.localPosition = Vector3.zero;
        bi.transform.localRotation = Quaternion.identity;
        bi.transform.localScale = Vector3.one;

        string[] iconStr = ItemProto.GetIconName(cse.ItemID);
        if (iconStr.Length != 0)
            bi.IconName = iconStr[0];
        else
            bi.IconName = "";
        bi.Description = cse.Name;

        m_Icons.Add(bi);

        return bi;
    }


    #region NGUI_CALLBACK

    void OnAddFuelBtn()
    {
        //lz-2016.11.02 错误 #5426 空对象
        if (null==PeCreature.Instance||PeCreature.Instance.mainPlayer == null|| null == m_PPCoal || null==m_Entity)
            return;

        if (!GameConfig.IsMultiMode)
        {
            PlayerPackageCmpt packageCmpt = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            if (null == packageCmpt)
                return;
            ItemPackage accessor = packageCmpt.package._playerPak;
            if (null == accessor)
                return;

            foreach (CSUI_MaterialGrid mg in m_MatGrids)
            {
                if (null!=mg&&mg.ItemID != 0)
                {
                    accessor.Destroy(mg.ItemID, mg.NeedCnt);
                    CSUI_MainWndCtrl.CreatePopupHint(mg.transform.position, transform, new Vector3(10, -2, -5), " - " + mg.NeedCnt.ToString(), false);
                }
            }

            m_PPCoal.StartWorkingCounter();

            //		CSUI_Main.ShowStatusBar("The " +  m_Entity.Name +" is full with fuel now");
            CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mFullFuelTips.GetString(), m_Entity.Name));
        }
        else
        {
            m_PPCoal._ColonyObj._Network.PPC_AddFuel();
        }

    }
    #endregion

    void OnChargingItemChanged(int index, ItemObject item)
    {
        if (m_PPCoal == null)
            return;

        m_PPCoal.SetChargingItem(index, item);

    }

    bool OnChargingItemItemCheck(ItemObject item)
    {
        if (!CSUI_MainWndCtrl.IsWorking())
            return false;

        return true;
    }

    #region UNITY_INNER_FUNC

    void Awake()
    {
        // Create Worked material Grids
        for (int i = 0; i < 1; i++)
        {
            CSUI_MaterialGrid mg = Instantiate(m_Work.m_MatGridPrefab) as CSUI_MaterialGrid;
            mg.transform.parent = m_Work.m_Root.transform;
            mg.transform.localPosition = Vector3.zero;
            mg.transform.localRotation = Quaternion.identity;
            mg.transform.localScale = Vector3.one;

            mg.ItemID = 0;
            m_MatGrids.Add(mg);
        }
        
        for (int i = 0; i < 8; i++)
        {
            CSUI_CommonIcon ci = Instantiate(m_CommonIcoPrefab) as CSUI_CommonIcon;
            ci.transform.parent = mIcoGrid.transform;
            ci.transform.localPosition = Vector3.zero;
            ci.transform.localRotation = Quaternion.identity;
            ci.transform.localScale = Vector3.one;
            ci.e_OnClickIco += OnClickIcoItem;
			ci.gameObject.SetActive(true);

            mUIPPCoalIcoList.Add(ci);
        }
        mIcoGrid.repositionNow = true;
    }

    // Use this for initialization
    void Start()
    {
		if (m_PPCoal == null)
			return;

        // Create Charging Item
        for (int i = 0; i < m_PPCoal.GetChargingItemsCnt(); i++)
        {
            CSUI_ChargingGrid cg = Instantiate(m_Charging.m_ChargeGridPrefab) as CSUI_ChargingGrid;
            cg.transform.parent = m_Charging.m_Root.transform;
            cg.transform.localPosition = Vector3.zero;
            cg.transform.localRotation = Quaternion.identity;
            cg.transform.localScale = Vector3.one;
            cg.m_Index = i;
            cg.m_bCanChargeLargedItem = true;
            cg.m_bUseMsgBox = false;
            cg.onItemChanded = OnChargingItemChanged;
            cg.onItemCheck = OnChargingItemItemCheck;


            if (GameConfig.IsMultiMode)
            {
                cg.OnDropItemMulti = OnDropItemMulti;
                cg.OnLeftMouseClickedMulti = OnLeftMouseClickedMulti;
                cg.OnRightMouseClickedMulti = OnRightMouseClickedMulti;
            }

            m_ChargingGrids.Add(cg);
            cg.SetItem(m_PPCoal.GetChargingItem(cg.m_Index));
        }

        m_Charging.m_Root.repositionNow = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_PPCoal == null)
            return;
        if(PeCreature.Instance.mainPlayer==null)
        {
            return;
        }
        if (playerPackageCmpt == null || player == null || player != PeCreature.Instance.mainPlayer)
        {
            player = PeCreature.Instance.mainPlayer;
            playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
        }
        UpdateWorkingTime();
    }

    #endregion


    #region multimode
    public void AddFuelSuccess(CSPPCoal m_PPCoal)
    {
        if (this.m_PPCoal != m_PPCoal)
        {
            return;
        }
        foreach (CSUI_MaterialGrid mg in m_MatGrids)
        {
            if (mg.ItemID != 0)
            {
                CSUI_MainWndCtrl.CreatePopupHint(mg.transform.position, this.transform, new Vector3(10, -2, -5), " - " + mg.NeedCnt.ToString(), false);
            }
        }
        CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mFullFuelTips.GetString(), m_Entity.Name));
    }
    //operation
    public void OnDropItemMulti(int index, Grid_N grid)
    {
        CSUI_ChargingGrid cg = m_ChargingGrids[index];
        ItemObject SelectedItem = SelectItem_N.Instance.ItemObj;

        //1.check
        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        if (!cg.IsChargeable(SelectedItem))
        {
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        //2.send
        m_PPCoal._ColonyObj._Network.POW_AddChargItem(index, SelectedItem);

        //3.do
        SelectItem_N.Instance.SetItem(null);

    }
    public void OnLeftMouseClickedMulti(int index, Grid_N grid)
    {
//        CSUI_ChargingGrid cg = m_ChargingGrids[index];
        if (grid.Item == null)
            return;
        SelectItem_N.Instance.SetItemGrid(grid);
    }
    public void OnRightMouseClickedMulti(int index, Grid_N grid)
    {
//        CSUI_ChargingGrid cg = m_ChargingGrids[index];
        GameUI.Instance.mItemPackageCtrl.Show();

        if (grid.ItemObj != null)
        {
            m_PPCoal._ColonyObj._Network.POW_RemoveChargItem(grid.ItemObj.instanceId);
        }
    }

    public void AddChargeItemResult(bool success, int index, int objId, CSPPCoal entity)
    {
        if (entity == m_PPCoal)
		{
			CSUI_ChargingGrid cg = m_ChargingGrids[index];
			ItemObject itemObj = ItemMgr.Instance.Get(objId);
            if (success)
            {
                cg.SetItemUI(itemObj);
                if (!cg.m_bUseMsgBox)
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToCharge.GetString(), itemObj.protoData.GetName()));
            }
            else
            {
                if (!cg.m_bUseMsgBox)
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotToBeCharged.GetString(), itemObj.protoData.GetName()), Color.red);
            }
        }
    }

    public void GetChargItemResult(bool success, int objId, CSPPCoal entity)
    {
        if (success)
        {
            if (entity == m_PPCoal)
            {
                for (int i = 0; i < m_ChargingGrids.Count; i++)
                {
                    if (m_ChargingGrids[i].Item != null && m_ChargingGrids[i].Item.instanceId == objId)
                    {
                        m_ChargingGrids[i].SetItemUI(null);
                        break;
                    }
                }
            }
        }
    }
    #endregion
}
