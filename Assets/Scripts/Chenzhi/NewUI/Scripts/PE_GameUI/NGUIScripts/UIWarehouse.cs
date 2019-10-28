using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;


public class UIWarehouse : UIBaseWnd
{
    public static System.Action OnShow;
    int mRow = 5;
    int mColumn = 6;
    int mPageCount;

    public UILabel mPageCountText;
    public Grid_N mGridPrefab;
    public UICheckbox mItemCheckbox;
    public UICheckbox mEquipCheckbox;
    public UICheckbox mResCheckbox;
    public UICheckbox mArmorCheckbox;
    public GameObject mGridsContent;
    List<Grid_N> mItems;

    public int mCurrentPickTab;
    int mPageIndex;

    SlotList m_CurrentPack;
    ItemPackage mItemPackage;

    Transform mOpObject;
    Pathea.PlayerPackageCmpt playerPackage;
    WareHouseObject _wareObj;

    void Start()
    {
        Show();
    }

    public override void Show()
    {
        playerPackage = GameUI.Instance.mMainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();

        ResetItem();
        if (GameUI.Instance.mItemPackageCtrl != null)
            if (!GameUI.Instance.mItemPackageCtrl.isShow)
                GameUI.Instance.mItemPackageCtrl.Show();
            else
            {
                mCurrentPickTab = GameUI.Instance.mItemPackageCtrl.CurrentPickTab;
                mPageIndex = 0;
            }

        base.Show();
        if (OnShow != null)
            OnShow.Invoke();
    }

    public void ResetItemPacket(ItemPackage itemPackage, Transform obj, WareHouseObject wareObj)
    {
        mItemPackage = itemPackage;
        mOpObject = obj;
        _wareObj = wareObj;
    }

    protected override void InitWindow()
    {
        base.InitWindow();

    }

    public override void OnCreate()
    {
        base.OnCreate();
        InitGrid();
        mCurrentPickTab = 0;
        mPageIndex = 0;
    }

    void InitGrid()
    {
        mItems = new List<Grid_N>();
        mPageCount = mRow * mColumn;
        for (int i = 0; i < mPageCount; i++)
        {
            mItems.Add(Instantiate(mGridPrefab) as Grid_N);
            mItems[i].gameObject.name = "WarehouseGrid" + i;
            mItems[i].transform.parent = mGridsContent.transform;
            mItems[i].transform.localPosition = new Vector3(i % mColumn * 55, -i / mColumn * 52, 0);
            mItems[i].transform.localRotation = Quaternion.identity;
            mItems[i].transform.localScale = Vector3.one;

            mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
            mItems[i].onRightMouseClicked = OnRightMouseCliked;
            mItems[i].onDropItem = OnDropItem;
        }
    }

    void OnItemBtn()
    {
        mCurrentPickTab = 0;
        mPageIndex = 0;
        ResetItem();
        ChangeItemPackageTab();
    }

    void OnEquipmentBtn()
    {
        mCurrentPickTab = 1;
        mPageIndex = 0;
        ResetItem();
        ChangeItemPackageTab();
    }

    void OnResourceBtn()
    {
        mCurrentPickTab = 2;
        mPageIndex = 0;
        ResetItem();
        ChangeItemPackageTab();
    }

    void OnArmorBtn()
    {
        mCurrentPickTab = 3;
        mPageIndex = 0;
        ResetItem();
        ChangeItemPackageTab();
    }

    void ChangeItemPackageTab()
    {
        if (GameUI.Instance.mItemPackageCtrl != null)
            if (GameUI.Instance.mItemPackageCtrl.isShow)
                GameUI.Instance.mItemPackageCtrl.ResetItem(mCurrentPickTab, mPageIndex);
    }

    void BtnLeftOnClick()
    {
        if (mPageIndex > 0)
        {
            mPageIndex -= 1;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
    }

    void BtnRightOnClick()
    {
        if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
        {
            mPageIndex += 1;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
    }

    void BtnLeftEndOnClick()
    {
        if (mPageIndex > 0)
        {
            mPageIndex = 0;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
    }

    void BtnRightEndOnClick()
    {
        if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
        {
            mPageIndex = (m_CurrentPack.Count - 1) / mPageCount;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
    }



    public void ResetItem(int type, int pageIndex)
    {
        if (mItemPackage == null)
            return;

        mCurrentPickTab = type;
        switch (type)
        {
            case 0:
                mItemCheckbox.isChecked = true;
                break;
            case 1:
                mEquipCheckbox.isChecked = true;
                break;
            case 2:
                mResCheckbox.isChecked = true;
                break;
            case 3:
                mArmorCheckbox.isChecked = true;
                break;
        }

        m_CurrentPack = mItemPackage.GetSlotList((ItemPackage.ESlotType)type);

        if ((m_CurrentPack.Count - 1) / mPageCount < pageIndex)
            pageIndex = (m_CurrentPack.Count - 1) / mPageCount;

        mPageIndex = pageIndex;

        int itemCount;

        if ((m_CurrentPack.Count - 1) / mPageCount == mPageIndex)
            itemCount = (m_CurrentPack.Count - pageIndex * mPageCount);
        else
            itemCount = mPageCount;

        for (int index = 0; index < itemCount; index++)
        {
            mItems[index].SetItem(m_CurrentPack[index + pageIndex * mPageCount]);
            mItems[index].SetItemPlace(ItemPlaceType.IPT_Warehouse, index + pageIndex * mPageCount);

            switch (mCurrentPickTab)
            {
                case 0:
                    mItems[index].SetGridMask(GridMask.GM_Item);
                    break;
                case 1:
                    mItems[index].SetGridMask(GridMask.GM_Equipment);
                    break;
                case 2:
                    mItems[index].SetGridMask(GridMask.GM_Resource);
                    break;
                case 3:
                    mItems[index].SetGridMask(GridMask.GM_Armor);
                    break;
            }

        }
        mPageCountText.text = (mPageIndex + 1).ToString() + "/" + ((m_CurrentPack.Count - 1) / mPageCount + 1);
    }

    public void ResetItem()
    {
        ResetItem(mCurrentPickTab, mPageIndex);
    }

    public bool SetItemWithIndex(ItemObject itemObj, int index = -1)
    {
        bool resoult = false;
        if (index == -1)
            resoult = m_CurrentPack.Add(itemObj);
        else
        {
            if (index < 0 || index > m_CurrentPack.Count)
            {
                resoult = false;
            }
            if (m_CurrentPack != null)
            {
                m_CurrentPack[index] = itemObj;
                resoult = true;
            }
        }
        ResetItem();
        return resoult;
    }

    void OnLeftMouseCliked(Grid_N grid)
    {
        ActiveWnd();
        SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
    }

    void OnRightMouseCliked(Grid_N grid)
    {
        ActiveWnd();
        GameUI.Instance.mItemPackageCtrl.Show();

        if (grid.ItemObj != null && playerPackage != null)
        {
            if (PeGameMgr.IsMulti)
            {
                if (_wareObj != null)
                    _wareObj._objNet.GetItem(grid.ItemObj.instanceId);
            }
            else
            {
                if (playerPackage.Add(grid.ItemObj))
                    SetItemWithIndex(null, grid.ItemIndex);
                else //lz-2016.09.14 提示背包已满
                    PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);

            }
        }
    }

    void OnDropItem(Grid_N grid)
    {
        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar )
            return;
        ActiveWnd();
        if (PeGameMgr.IsMulti)
        {
            //lz-2017.02.27 错误 #8998  多人storage只支持从背包放入，不支持storage内部交换
            if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)
            {
                //lz-2016.11.16 多人是所有类型的一个总列表，index需要通过类型和页数处理一下
                if (_wareObj != null)
                    _wareObj._objNet.InsertItemList(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex + (mCurrentPickTab * mPageCount));
            }
            return;
        }
        if (grid.ItemObj == null)
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_HotKeyBar:
                    SelectItem_N.Instance.SetItem(null);
                    break;
                default:
                    if (SelectItem_N.Instance.GridMask != GridMask.GM_Mission)
                    {
                        SelectItem_N.Instance.RemoveOriginItem();
                        grid.SetItem(SelectItem_N.Instance.ItemObj);
                        m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
                    }
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
        else
        {
            //lz-2016.11.16 添加仓库内交换和背包交换功能
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_Warehouse:
                    ItemObject temp = SelectItem_N.Instance.ItemObj;
                    m_CurrentPack[SelectItem_N.Instance.Index] = grid.ItemObj;
                    m_CurrentPack[grid.ItemIndex] = temp;
                    SelectItem_N.Instance.SetItem(null);
                    ResetItem();
                    break;
                case ItemPlaceType.IPT_Bag:
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        GameUI.Instance.mItemPackageCtrl.SetItemWithIndex(grid.ItemObj, SelectItem_N.Instance.Index);
                        m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
                        SelectItem_N.Instance.SetItem(null);
                        ResetItem();
                    }
                    break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }

    void Update()
    {
        if (GameUI.Instance == null)
            return;

        if (null != GameUI.Instance.mMainPlayer && null != mOpObject)
        {
			if (Vector3.Distance(GameUI.Instance.mMainPlayer.position, mOpObject.position) > WareHouseObject.MaxOperateDistance)
                OnClose();
        }
    }
}
