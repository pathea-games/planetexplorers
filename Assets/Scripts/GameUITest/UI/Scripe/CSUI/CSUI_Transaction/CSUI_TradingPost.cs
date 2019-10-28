using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using System;

/// <summary>
/// lz-2016.10.21 基地新的贸易站控制UI
/// </summary>
public class CSUI_TradingPost : MonoBehaviour
{

    [SerializeField]
    UISlicedSprite m_SpriteMeat;
    [SerializeField]
    UISlicedSprite m_SpriteMoney;
    [SerializeField]
    UILabel m_LbCurrency;

    [SerializeField]
    Grid_N m_GridPrefab;
    [SerializeField]
    UIGrid m_ItemGrid;

    [SerializeField]
    UIPanel mSellOpLayer;
    [SerializeField]
    Grid_N m_CurOpGrid;
    [SerializeField]
    UIInput m_InputOpNum;
    [SerializeField]
    UILabel m_LbTotal;
    [SerializeField]
    UILabel m_LbPrice;
    [SerializeField]
    UILabel m_BtnOp;
    [SerializeField]
    GameObject m_MeshGo;

    [SerializeField]
    UILabel m_LbPage;


    public Action RequestRefreshUIEvent;   //请求刷新UI事件
    public Action<int, int> BuyItemEvent;   //购买事件
    public Action<int, int> RepurchaseItemEvent; //赎回事件
    public Action<int, int> SellItemEvent; //卖入事件

    public bool IsShow { get { return gameObject.activeInHierarchy; } }
    public bool IsShowAndCanUse { get { return IsShow && m_CanUseThis; } }
    public int CurPackTab { get { return m_CurPackTab; } }
    public bool CanUseThis { get { return m_CanUseThis; } }

    CSUI_LeftMenuItem m_CurMenuItem;
    bool m_CanUseThis;
    bool m_NewUseState;

    const int m_Row = 6;
    const int m_Column = 13;
    int m_PageCount;
    List<Grid_N> m_ItemGridList;
    List<ItemObject> m_BuyItemList;
    List<ItemObject> m_RepurchaseList;
    int m_Currency;

    int m_CurPageIndex;
    int m_CurPackTab = 0;    //0:背包，1:赎回,
    ItemObject m_CurOpItem;
    ItemLabel.Root m_CurType;
    int m_CurPrice;

    bool m_IsBuy;
    int m_MaxPage;

    float m_OpDurNum;
    float m_OpStarTime;
    float m_LastOpTime;
    float m_CurrentNum;

    bool mAddBtnPress = false;
    bool mSubBtnPress = false;

    bool m_Init = false;
    Vector3 m_PlayerPackagePos;
    Vector3 m_ColonyPos;


    #region mono methods

    void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        if (null != RequestRefreshUIEvent)
        {
            RequestRefreshUIEvent();
        }
        TryOpenPlayerPackagPanel();
    }

    void OnDisable()
    {
        TryClosePlayerPackagePanel();
    }

    void Update()
    {
        if (null != m_CurMenuItem)
        {
            //lz-2016.10.24 增加没在范围内不可以操作
            m_NewUseState = !(m_CurMenuItem.AssemblyLevelInsufficient || m_CurMenuItem.NotHaveAssembly || m_CurMenuItem.NotHaveElectricity||!CSUI_MainWndCtrl.IsWorking(false));
            if (m_NewUseState != m_CanUseThis)
            {
                m_CanUseThis = m_NewUseState;
                ShowMeshGo(!m_CanUseThis);
            }
        }

        if (null != m_CurOpItem)
        {
            if (mAddBtnPress)
            {
                float dT = Time.time - m_OpStarTime;
                if (dT < 0.2f)
                    m_OpDurNum = 1;
                else if (dT < 1f)
                    m_OpDurNum += 2 * Time.deltaTime;
                else if (dT < 2f)
                    m_OpDurNum += 4 * Time.deltaTime;
                else if (dT < 3f)
                    m_OpDurNum += 7 * Time.deltaTime;
                else if (dT < 4f)
                    m_OpDurNum += 11 * Time.deltaTime;
                else if (dT < 5f)
                    m_OpDurNum += 16 * Time.deltaTime;
                else
                    m_OpDurNum += 20 * Time.deltaTime;

                m_OpDurNum = Mathf.Clamp(m_OpDurNum + m_CurrentNum, 1, m_CurOpItem.GetCount()) - m_CurrentNum;
                m_InputOpNum.text = ((int)(m_OpDurNum + m_CurrentNum)).ToString();
                m_LbTotal.text = (m_CurPrice * (int)(m_OpDurNum + m_CurrentNum)).ToString();
            }
            else if (mSubBtnPress)
            {
                float dT = Time.time - m_OpStarTime;
                if (dT < 0.5f)
                    m_OpDurNum = -1;
                else if (dT < 1f)
                    m_OpDurNum -= 2 * Time.deltaTime;
                else if (dT < 2f)
                    m_OpDurNum -= 4 * Time.deltaTime;
                else if (dT < 3f)
                    m_OpDurNum -= 7 * Time.deltaTime;
                else if (dT < 4f)
                    m_OpDurNum -= 11 * Time.deltaTime;
                else if (dT < 5f)
                    m_OpDurNum -= 16 * Time.deltaTime;
                else
                    m_OpDurNum -= 20 * Time.deltaTime;

                m_OpDurNum = Mathf.Clamp(m_OpDurNum + m_CurrentNum, 1, m_CurOpItem.GetCount()) - m_CurrentNum;
                m_InputOpNum.text = ((int)(m_OpDurNum + m_CurrentNum)).ToString();
                m_LbTotal.text = (m_CurPrice * (int)(m_OpDurNum + m_CurrentNum)).ToString();
            }
            else
            {
                if ("" == m_InputOpNum.text)
                    m_CurrentNum = 1;
                else
                    m_CurrentNum = Mathf.Clamp(System.Convert.ToInt32(m_InputOpNum.text), 1, m_CurOpItem.GetCount());
                if (!UICamera.inputHasFocus)
                {
                    m_InputOpNum.text = m_CurrentNum.ToString();
                    m_LbTotal.text = (m_CurPrice * (int)m_CurrentNum).ToString();
                }
            }
        }
    }

    #endregion

    #region private methods

    void CheckCantWorkTip()
    {
        //lz-2016.10.24 如果基地可以工作，说明没有超过距离，并且有核心
        if (CSUI_MainWndCtrl.IsWorking())
        {
            if (null != m_CurMenuItem)
            {
                //lz-2016.10.24 贸易站核心等级不足提示
                if (m_CurMenuItem.AssemblyLevelInsufficient)
                {
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkAssemblyLevelInsufficient.GetString(), CSUtils.GetEntityName(m_CurMenuItem.m_Type)), Color.red);
                }
                //lz-2016.10.24 贸易站没电提示
                else if (m_CurMenuItem.NotHaveElectricity)
                {
                    CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), CSUtils.GetEntityName(m_CurMenuItem.m_Type)), Color.red);
                }
            }
        }
    }

    void TryClosePlayerPackagePanel()
    {
        if (GameUI.Instance.mItemPackageCtrl.isShow)
            GameUI.Instance.mItemPackageCtrl.Hide();

        GameUI.Instance.mItemPackageCtrl.transform.localPosition = m_PlayerPackagePos;
    }

    void TryOpenPlayerPackagPanel()
    {
        if (!GameUI.Instance.mItemPackageCtrl.isShow)
            GameUI.Instance.mItemPackageCtrl.Show();

        m_PlayerPackagePos = GameUI.Instance.mItemPackageCtrl.transform.localPosition;
        Vector3 newPos = m_PlayerPackagePos;
        newPos.x = GameUI.Instance.mCSUI_MainWndCtrl.transform.localPosition.x + 675;
        newPos.y = GameUI.Instance.mCSUI_MainWndCtrl.transform.localPosition.y + 10;
        GameUI.Instance.mItemPackageCtrl.transform.localPosition = newPos;
    }


    private void Init()
    {
        if (m_Init) return;
        m_BuyItemList = new List<ItemObject>();
        m_RepurchaseList = new List<ItemObject>();
        m_ItemGridList = new List<Grid_N>();
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        m_Currency = 0;
        m_CurType = ItemLabel.Root.all;

        m_PageCount = m_Row * m_Column;
        for (int i = 0; i < m_PageCount; i++)
        {
            Grid_N grid = Instantiate(m_GridPrefab) as Grid_N;
            grid.transform.parent = m_ItemGrid.transform;
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localRotation = Quaternion.identity;
            grid.transform.localScale = Vector3.one;
            grid.onLeftMouseClicked = OnLeftMouseCliked;
            grid.onRightMouseClicked = OnRightMouseCliked;
            m_ItemGridList.Add(grid);
        }

        ShowOpPanel(false);
        m_CanUseThis=true;
        m_NewUseState = true;
        ShowMeshGo(false);
        //lz-2016.10.24 贸易站不能工作的时候点击遮罩提示不能工作的原因
        UIEventListener.Get(m_MeshGo).onClick = (go) => { CheckCantWorkTip(); };
        m_Init = true;
    }

    private Grid_N GetGridByItem(int instanceID)
    {
        return m_ItemGridList.Find(a => null != a.ItemObj && a.ItemObj.instanceId == instanceID);
    }

    private void UpdateGridsInfo()
    {
        List<ItemObject> curItemList = new List<ItemObject>();
        if (m_CurPackTab == 0)
        {
            if (m_CurType == ItemLabel.Root.all)
                curItemList = m_BuyItemList;
            else
                curItemList = m_BuyItemList.FindAll(a => null != a.protoData && a.protoData.rootItemLabel == m_CurType);
        }
        else
            curItemList = m_RepurchaseList;


        if (null != curItemList && curItemList.Count > 0)
        {
            m_MaxPage = (curItemList.Count - 1) / m_PageCount;
            if (m_MaxPage < 0)
                m_MaxPage = 0;

            if (m_MaxPage < m_CurPageIndex)
                m_CurPageIndex = m_MaxPage;
        }
        else
        {
            m_CurPageIndex = 0;
            m_MaxPage = 0;
        }

        int itemCount = 0;
        if (m_MaxPage == m_CurPageIndex)
            itemCount = (curItemList.Count - m_CurPageIndex * m_PageCount);
        else
            itemCount = m_PageCount;

        ItemObject item = null;
        int startIndex = m_CurPageIndex * m_PageCount;
        int index = startIndex;
        for (int i = 0; i < m_PageCount; i++)
        {
            index = startIndex + i;
            item = i < itemCount ? curItemList[index] : null;
            UpdateGrid(m_ItemGridList[i], item, index);
        }

        UpdatePageInfo((m_CurPageIndex + 1), (m_MaxPage + 1));
    }

    void UpdateGrid(Grid_N grid, ItemObject item, int index)
    {
        grid.SetItem(item);
        grid.SetItemPlace(ItemPlaceType.IPT_ColonyTradingPost, index);
        grid.SetGridMask(GridMask.GM_Any);
    }

    void UpdatePageInfo(int curPage, int maxPage)
    {
        m_LbPage.text = string.Format("{0}/{1}", (m_CurPageIndex + 1), (m_MaxPage + 1));
        m_LbPage.MakePixelPerfect();
    }


    private void UpdateCurrency()
    {
        m_LbCurrency.text = m_Currency.ToString();
        if (Money.Digital == false)
        {
            m_SpriteMeat.gameObject.SetActive(true);
            m_SpriteMoney.gameObject.SetActive(false);
        }
        else
        {
            m_SpriteMeat.gameObject.SetActive(false);
            m_SpriteMoney.gameObject.SetActive(true);
        }
        //lz-2016.10.24 根据货币的长度更新货币图标的位置
        Vector3 pos = m_SpriteMeat.transform.localPosition;
        pos.x = m_LbCurrency.transform.localPosition.x-m_LbCurrency.relativeSize.x* m_LbCurrency.font.size-12;
        m_SpriteMeat.transform.localPosition = pos;
        m_SpriteMoney.transform.localPosition = pos;
    }

    private void BuyAll()
    {
        if (null != m_CurOpItem)
        {
            BuyItem(m_CurOpItem.GetCount());
        }
    }

    private void BuyItem(int count)
    {
        if (null != m_CurOpItem && count <= m_CurOpItem.GetCount())
        {
            if (m_CurPackTab == 0)
            {
                if (null != BuyItemEvent)
                {
                    BuyItemEvent(m_CurOpItem.instanceId, count);
                    ResetOpInfo();
                }
            }
            else
            {
                if (null != RepurchaseItemEvent)
                {
                    RepurchaseItemEvent(m_CurOpItem.instanceId, count);
                    ResetOpInfo();
                }
            }
        }
    }

    private void SellAll()
    {
        if (null != m_CurOpItem)
        {
            SellItem(m_CurOpItem.GetCount());
        }
    }

    private void SellItem(int count)
    {
        if (null != m_CurOpItem && count <= m_CurOpItem.GetCount())
        {
            if (null != SellItemEvent)
            {
                SellItemEvent(m_CurOpItem.instanceId, count);
                ResetOpInfo();
            }
        }
    }

    private void ShowOpPanel(bool isShow)
    {
        mSellOpLayer.gameObject.SetActive(isShow);
    }

    private void UpdateOpPanelInfo()
    {
        if (null == m_CurOpItem)
            return;

        ShowOpPanel(true);
        m_CurOpGrid.SetItem(m_CurOpItem);
        m_CurrentNum = 1;
        if (m_CurPackTab == 0 && m_IsBuy)
            //lz-2016.10.24 贸易站收取一定比例的服务费
            m_CurPrice = Mathf.RoundToInt(m_CurOpItem.GetBuyPrice() * (1 + ColonyConst.TRADE_POST_CHARGE_RATE));
        else
            m_CurPrice = m_CurOpItem.GetSellPrice(); //赎回的价格等于卖出的价格
        m_InputOpNum.text = m_CurrentNum.ToString();
        m_LbTotal.text = m_CurPrice.ToString();
        m_LbPrice.text = m_CurPrice.ToString();
        m_BtnOp.text = PELocalization.GetString(m_IsBuy ? 8000556 : 8000555);
    }

    private void ResetOpInfo()
    {
        ShowOpPanel(false);
        m_CurOpItem = null;
        m_CurrentNum = 1;
        m_CurOpGrid.SetItem(null);
        if (!m_IsBuy && null != GameUI.Instance)
        {
            GameUI.Instance.mItemPackageCtrl.RestItemState();
        }
        m_IsBuy = false;
    }

    private void ShowMeshGo(bool show)
    {
        m_MeshGo.SetActive(show);
    }

    #endregion

    #region public methods

    public void SetMenu(CSUI_LeftMenuItem curMenuItem)
    {
        m_CurMenuItem = curMenuItem;
    }
    
    public void SellItemByPakcage(ItemObject item)
    {
        if (!m_Init) Init();
        m_CurOpItem = item;
        m_IsBuy = false;
        UpdateOpPanelInfo();
    }

    public void SellAllItemByPackage(ItemObject item)
    {
        if (!m_Init) Init();
        m_CurOpItem = item;
        m_IsBuy = false;
        SellAll();
    }

    public void CancelSell()
    {
        ResetOpInfo();
    }
    #endregion

    #region UIEvent methods
    private void OnLeftMouseCliked(Grid_N grid)
    {
        if (grid.ItemObj == null)
            return;

        if (mSellOpLayer.gameObject.activeSelf)
            return;

        if (grid.ItemObj.GetCount() == 0)
        {
            new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
            return;
        }

        m_CurOpItem = grid.ItemObj;
        m_IsBuy = true;
        UpdateOpPanelInfo();
        SelectItem_N.Instance.SetItem(null);
    }

    private void OnRightMouseCliked(Grid_N grid)
    {
        if (grid.ItemObj == null)
            return;

        if (mSellOpLayer.gameObject.activeSelf)
            return;

        if (grid.ItemObj.GetCount() == 0)
        {
            new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
            return;
        }

        SelectItem_N.Instance.SetItem(null);

        m_CurOpItem = grid.ItemObj;

        //lz-2016.10.27 购买所有的时候提示加上购买所有的价格
        int count = m_CurOpItem.GetCount();
        int price = 0;
        if (m_CurPackTab == 0)
            price = Mathf.RoundToInt(m_CurOpItem.GetBuyPrice() * (1 + ColonyConst.TRADE_POST_CHARGE_RATE));
        else
            price = m_CurOpItem.GetSellPrice(); //赎回的价格等于卖出的价格
        string name = m_CurOpItem.protoData.GetName();
        string msgStr = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000077), name + " X " + count, PELocalization.GetString(8000253), (count * price));
        MessageBox_N.ShowYNBox(msgStr, BuyAll, ResetOpInfo);
    }


    void BtnAllOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.all;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnWeaponOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;
        m_CurType = ItemLabel.Root.weapon;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnEquipOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.equipment;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnToolOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.tool;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnTurretOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.turret;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }


    void BtnConsumOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.consumables;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnResoureOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.resoure;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnPartOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.part;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnDecorationOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        m_CurType = ItemLabel.Root.decoration;
        m_CurPackTab = 0;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void BtnRebuyOnClick()
    {
        m_CurPackTab = 1;
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void OnOpOkBtn()
    {
        if (null != m_CurOpItem)
        {
            if (m_IsBuy)
                BuyItem((int)m_CurrentNum);
            else
                SellItem((int)m_CurrentNum);
        }
    }

    void OnOpCancelBtn()
    {
        ShowOpPanel(false);
        ResetOpInfo();
    }

    void OnPageDown()
    {
        if (m_CurPageIndex > 0)
        {
            m_CurPageIndex -= 1;
            UpdateGridsInfo();
        }
    }
    void BtnLeftEndOnClick()
    {
        m_CurPageIndex = 0;
        UpdateGridsInfo();
    }

    void OnPageUp()
    {
        if (m_CurPageIndex < m_MaxPage)
        {
            m_CurPageIndex += 1;
            UpdateGridsInfo();
        }
    }

    void BtnRightEndOnClick()
    {
        m_CurPageIndex = m_MaxPage;
        UpdateGridsInfo();
    }

    void OnAddBtnPress()
    {
        mAddBtnPress = true;
        m_OpStarTime = Time.time;
        m_OpDurNum = 0;
    }

    void OnAddBtnRelease()
    {
        mAddBtnPress = false;
        m_CurrentNum = (int)(m_CurrentNum + m_OpDurNum);
        m_OpDurNum = 0;
        m_InputOpNum.text = ((int)(m_CurrentNum)).ToString();
        m_LbTotal.text = (m_CurPrice * (int)(m_CurrentNum)).ToString();
    }

    void OnSubstructBtnPress()
    {
        mSubBtnPress = true;
        m_OpStarTime = Time.time;
        m_OpDurNum = 0;
    }

    void OnSubstructBtnRelease()
    {
        mSubBtnPress = false;
        m_CurrentNum = (int)(m_CurrentNum + m_OpDurNum);
        m_OpDurNum = 0;
        m_InputOpNum.text = ((int)(m_CurrentNum)).ToString();
        m_LbTotal.text = (m_CurPrice * (int)(m_CurrentNum)).ToString();
    }
    #endregion

    #region Logic Layer use methods

    /// <summary>更新UI所有数据</summary>
    public void UpdateUIData(List<ItemObject> buyItemList, List<ItemObject> repurchaseList, int currency)
    {
        if (null == buyItemList || null == repurchaseList)
            return;
        UpdateBuyItemList(buyItemList);
        UpdateRepurchaseList(repurchaseList);
        UpdateCurrency(currency);
    }

    /// <summary>更新可以买的商品的所有数据</summary>
    public void UpdateBuyItemList(List<ItemObject> buyItemList)
    {
        if (null == buyItemList)
            return;
        if (!m_Init) Init();
        m_BuyItemList.Clear();
        for (int i = 0; i < buyItemList.Count; i++)
        {
            m_BuyItemList.Add(buyItemList[i]);
        }
        UpdateGridsInfo();
    }

    /// <summary>更新可以赎回商品的所有数据</summary>
    public void UpdateRepurchaseList(List<ItemObject> repurchaseList)
    {
        if (null == repurchaseList)
            return;
        if (!m_Init) Init();
        m_RepurchaseList.Clear();
        for (int i = 0; i < repurchaseList.Count; i++)
        {
            m_RepurchaseList.Add(repurchaseList[i]);
        }
        UpdateGridsInfo();
    }

    /// <summary>更新货币显示</summary>
    public void UpdateCurrency(int currency)
    {
        if (!m_Init) Init();
        m_Currency = currency;
        UpdateCurrency();
    }

    /// <summary>通过已有数据刷新当前显示的格子，主要是刷新数量用</summary>
    public void RefreshCurrentyShowGrids()
    {
        if (!m_Init) Init();
        UpdateGridsInfo();
    }
    #endregion
}
