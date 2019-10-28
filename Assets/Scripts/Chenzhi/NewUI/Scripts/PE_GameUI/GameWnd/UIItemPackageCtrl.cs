using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

using ItemAsset.PackageHelper;
using Pathea;
using Pathea.PeEntityExt;
using System;

public class UIItemPackageCtrl : UIBaseWnd
{
    public Grid_N mGridPrefab;
    public GameObject ItemsContent;

    public UICheckbox mItemCheckbox;
    public UICheckbox mEquipCheckbox;
    public UICheckbox mResCheckbox;
    public UICheckbox mArmorCheckbox;
    public UICheckbox mMissionCheckbox;

    public Transform mSplitOpWnd;
    public UIInput mSplitNumlabel;

    public UILabel mPageCountText;
    public GameObject mDropItemBtn;
    public GameObject DropItemWndPrefab;
    public UIAtlas mNewUIAtlas;
    public int CurrentPickTab { get { return mCurrentPickTab; } }
    public int CurrentPageIndex { get { return mPageIndex; } }

    //lz-2016.08.02 切换页签分类事件
    public Action<GridMask> SelectPageEvent;

    //lz-2016.08.10 获取当前操作的Item
    public ItemSample CurOperateItem { get { return (null == m_CurOpItem || null == m_CurOpItem.Item) ? null : m_CurOpItem.Item; } }


    //luwei
    public UILabel mMoneyCurrent;

    public GameObject nMoneyRoot;

    [HideInInspector]
    public UIDropItemWnd mDropItemWnd = null;
    ItemPackage mItemPackage;
    public ItemPackage ItemPackage { get { return mItemPackage; } }

    Grid_N m_CurOpItem;


    int mRow = 7;
    int mColumn = 6;
    int mPageCount = 0;
    int mPageIndex = 0;
    int mCurrentPickTab = 0;
    int mOpType = 0;//0 no. 1 SplitItem. 2 DeleteItem

    float mSplitNumDur = 1;
    float mOpDurNum = 0;
    bool mAddBtnPress = false;
    bool mSubBtnPress = false;
    float mOpStarTime;

    int mOpBagID = -1;

    List<Grid_N> mItems;
    SlotList m_CurrentPack;

    //PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>();
    public delegate void OnResetItem(int packTab, int pageIndex);
    public event OnResetItem e_OnResetItem = null;

    public delegate void OnOpenPackage();
    public event OnOpenPackage e_OnOpenPackage = null;

    // [CSUI]  Delegate
    public event System.Action<Grid_N> onRightMouseCliked;
    public event System.Action<ItemAsset.ItemObject> onItemSelected;

    #region mono methods

    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    AISpawnPoint.Activate(4, true);
        //    AISpawnPoint.SpawnImmediately(4);
        //    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(4, true);

        //    AISpawnPoint.Activate(5, true);
        //    AISpawnPoint.SpawnImmediately(5);
        //    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(5, true);

        //    AISpawnPoint.Activate(6, true);
        //    AISpawnPoint.SpawnImmediately(6);
        //    SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(6, true);
        //}


        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    mOpType = 0;
        //    UICursor.Clear();
        //}

        if (Input.GetKeyDown(KeyCode.PageUp))
            BtnLeftOnClick();

        if (Input.GetKeyDown(KeyCode.PageDown))
            BtnRightOnClick();

        if ((PeInput.Get(PeInput.LogicFunction.OpenItemMenu) || Input.GetMouseButtonDown(1)) && !mSplitOpWnd.gameObject.activeSelf && m_CurOpItem == null)
        {
            mOpType = 0;
            UICursor.Clear();
        }

        if (mOpType != 0)
        {
            if (IsMouseHovered())
            {
                switch (mOpType)
                {
                    case 1:
                        UICursor.Set(mNewUIAtlas, "icocai");
                        break;
                    case 2:
                        UICursor.Set(mNewUIAtlas, "icodelete");
                        break;
                }
            }
            else
            {
                UICursor.Clear();
            }
        }





        //switch (mOpType)
        //{
        //    case 1:
        //        UICursor.Set(mNewUIAtlas, "icocai");
        //        break;
        //    case 2:
        //        UICursor.Set(mNewUIAtlas, "icodelete");
        //        break;
        //}

        //SplitNumOp

        //lz-2016.10.11 空对象【错误 #3905】
        if (null != m_CurOpItem&& null!=m_CurOpItem.Item)
        {
            if (mSplitOpWnd.gameObject.activeSelf)
            {
                if (mAddBtnPress)
                {
                    float dT = Time.time - mOpStarTime;
                    if (dT < 0.2f)
                        mOpDurNum = 1;
                    else if (dT < 1f)
                        mOpDurNum += 2 * Time.deltaTime;
                    else if (dT < 2f)
                        mOpDurNum += 4 * Time.deltaTime;
                    else if (dT < 3f)
                        mOpDurNum += 7 * Time.deltaTime;
                    else if (dT < 4f)
                        mOpDurNum += 11 * Time.deltaTime;
                    else if (dT < 5f)
                        mOpDurNum += 16 * Time.deltaTime;
                    else
                        mOpDurNum += 20 * Time.deltaTime;

                    mOpDurNum = Mathf.Clamp(mOpDurNum + mSplitNumDur, 1, m_CurOpItem.Item.GetCount() - 1) - mSplitNumDur;
                    mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
                }
                else if (mSubBtnPress)
                {
                    float dT = Time.time - mOpStarTime;
                    if (dT < 0.5f)
                        mOpDurNum = -1;
                    else if (dT < 1f)
                        mOpDurNum -= 2 * Time.deltaTime;
                    else if (dT < 2f)
                        mOpDurNum -= 4 * Time.deltaTime;
                    else if (dT < 3f)
                        mOpDurNum -= 7 * Time.deltaTime;
                    else if (dT < 4f)
                        mOpDurNum -= 11 * Time.deltaTime;
                    else if (dT < 5f)
                        mOpDurNum -= 16 * Time.deltaTime;
                    else
                        mOpDurNum -= 20 * Time.deltaTime;

                    mOpDurNum = Mathf.Clamp(mOpDurNum + mSplitNumDur, 1, m_CurOpItem.Item.GetCount() - 1) - mSplitNumDur;
                    mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
                }
                else
                {
                    if ("" == mSplitNumlabel.text)
                        mSplitNumDur = 1;
                    else
                        mSplitNumDur = Mathf.Clamp(System.Convert.ToInt32(mSplitNumlabel.text), 1, m_CurOpItem.Item.GetCount() - 1);
                    if (!UICamera.inputHasFocus)
                        mSplitNumlabel.text = mSplitNumDur.ToString();
                }
            }
        }

        //luwei 
        mMoneyCurrent.text = GetcurrentMoney();
    }

    #endregion

    #region override methods

    protected override void OnHide()
    {
        SelectItem_N.Instance.SetItem(null);
        RestItemState();
        //lz-2016.09.23 错误 #2105多人故事背包关闭后，背包右面的面板自动关闭
        if (null != mDropItemWnd && mDropItemWnd.isShow)
        {
            mDropItemWnd.Hide();
        }
        DeleteCurPageTutorialEffect();
        base.OnHide();
    }

    public override void OnCreate()
    {
        base.OnCreate();

        InitGrid();
        CreatDropWnd();

    }

    protected override void InitWindow()
    {
        base.InitWindow();
        base.SelfWndType = UIEnum.WndType.ItemPackage;

        mSplitOpWnd.gameObject.SetActive(false);
        mCurrentPickTab = 0;
        mPageIndex = 0;
        mDropItemBtn.SetActive(GameConfig.IsMultiMode);

        Pathea.PlayerPackageCmpt pkg = GameUI.Instance.mMainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
        pkg.package._playerPak.changeEventor.Subscribe(ResetItem);
        PlayerPackage._missionPak.changeEventor.Subscribe(ResetItem);

        if (Money.Digital == false)
        {
            nMoneyRoot.SetActive(false);
        }
        else
        {
            nMoneyRoot.SetActive(true);
        }
    }

    #endregion 

    #region private methods

    void ResetItem(object sender, ItemPackage.EventArg arg)
    {
        if (!isMission)
            ResetItem();
        else
            ResetMissionItem();
    }

    void CreatDropWnd()
    {
        GameObject obj = GameObject.Instantiate(DropItemWndPrefab) as GameObject;
        obj.transform.parent = this.transform.parent;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        mDropItemWnd = obj.GetComponent<UIDropItemWnd>();
        mDropItemWnd.Hide();
    }

    bool IsMouseHovered()
    {
        if (UICamera.hoveredObject == null)
            return false;
        if (UICamera.hoveredObject == this.gameObject || UICamera.hoveredObject.transform.IsChildOf(this.transform))
            return true;
        return false;
    }

    //lz-2016.08.02 调用切换类型页签事件
    void ExecSelectPageEvent(GridMask type)
    {
        CloseOpWnd();
        if (null != SelectPageEvent)
        {
            SelectPageEvent(type);
        }
    }

    void CheckMission(int itemid)
    {
        if (null != GameUI.Instance && null != MissionManager.Instance
            && null != GameUI.Instance.mMainPlayer && null != MissionManager.Instance.m_PlayerMission)
            MissionManager.Instance.ProcessCollectMissionByID(itemid);
    }

    void InitGrid()
    {
        mItems = new List<Grid_N>();
        mPageCount = mRow * mColumn;
        for (int i = 0; i < mPageCount; i++)
        {
            mItems.Add(Instantiate(mGridPrefab) as Grid_N);
            mItems[i].gameObject.name = "ItemPack" + i;
            mItems[i].transform.parent = ItemsContent.transform;
            mItems[i].transform.localPosition = new Vector3(i % mColumn * 55, -i / mColumn * 52, 0);
            mItems[i].transform.localRotation = Quaternion.identity;
            mItems[i].transform.localScale = Vector3.one;

            mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
            mItems[i].onRightMouseClicked = OnRightMouseCliked;
            mItems[i].onDropItem = OnDropItem;
			mItems[i].onGridsExchangeItem = OnGridsExchangeItems;
        }
    }
	void OnGridsExchangeItems(Grid_N grid, ItemObject item)
	{
		grid.SetItem(item);
		m_CurrentPack[grid.ItemIndex]=item;
	}
    void ResetPage()
    {
        if (!isMission)
        {
            GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
            // GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.StorageMainUI.isMission = false;
        }
        //else
        //GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.StorageMainUI.isMission = true;
    }

    void OnItemBtn()
    {
        isMission = false;
        mCurrentPickTab = 0;
        ExecSelectPageEvent(GridMask.GM_Item);
        mPageIndex = 0;
        mOpType = 0;
        ResetItem();
        ResetPage();
        GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
        //GameGui_N.Instance.mPublicInventoryGui.ResetItem(mCurrentPickTab,mPageIndex);
        //CSUI_Main.Instance.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
        if (UINpcStorageCtrl.Instance != null)
            UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
    }
    //	
    void OnEquipmentBtn()
    {
        isMission = false;
        mCurrentPickTab = 1;
        ExecSelectPageEvent(GridMask.GM_Equipment);
        mPageIndex = 0;
        mOpType = 0;
        ResetItem();
        ResetPage();
        GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
        //		GameGui_N.Instance.mPublicInventoryGui.ResetItem(mCurrentPickTab,mPageIndex);
        //		CSUI_Main.Instance.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
        if (UINpcStorageCtrl.Instance != null)
            UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
    }
    //	
    void OnResourceBtn()
    {
        isMission = false;
        mCurrentPickTab = 2;
        ExecSelectPageEvent(GridMask.GM_Resource);
        mPageIndex = 0;
        mOpType = 0;
        ResetItem();
        ResetPage();
        GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
        //		GameGui_N.Instance.mPublicInventoryGui.ResetItem(mCurrentPickTab,mPageIndex);
        //		CSUI_Main.Instance.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
        if (UINpcStorageCtrl.Instance != null)
            UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
    }
    //
    void OnArmorBtn()
    {
        isMission = false;
        mCurrentPickTab = 3;
        ExecSelectPageEvent(GridMask.GM_Armor);
        mPageIndex = 0;
        mOpType = 0;
        ResetItem();
        ResetPage();
        GameUI.Instance.mWarehouse.ResetItem(mCurrentPickTab, mPageIndex);
        //		GameGui_N.Instance.mPublicInventoryGui.ResetItem(mCurrentPickTab,mPageIndex);
        //		CSUI_Main.Instance.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
        if (UINpcStorageCtrl.Instance != null)
            UINpcStorageCtrl.Instance.SetTabIndex(mCurrentPickTab);
    }

    public bool isMission = false;

    //
    void OnMissionBtn()
    {
        isMission = true;
        mPageIndex = 0;
        mOpType = 0;
        ResetMissionItem();
        //ResetPage();
        ExecSelectPageEvent(GridMask.GM_Mission);
    }
    //	
    void BtnLeftOnClick()
    {
        if (mPageIndex > 0)
        {
            mPageIndex -= 1;
            if (!isMission)
            {
                ResetItem(mCurrentPickTab, mPageIndex);
                //ResetPage();
            }
            else
                ResetMissionItem(mPageIndex);
        }

    }

    void BtnRightOnClick()
    {
        if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
        {
            mPageIndex += 1;
            if (!isMission)
            {
                ResetItem(mCurrentPickTab, mPageIndex);
                //ResetPage();
            }
            else
                ResetMissionItem(mPageIndex);
        }
    }

    void BtnLeftEndOnClick()
    {
        if (mPageIndex > 0)
        {
            mPageIndex = 0;
            if (!isMission)
            {
                ResetItem(mCurrentPickTab, mPageIndex);
                //ResetPage();
            }
            else
                ResetMissionItem(mPageIndex);
        }
    }

    void BtnRightEndOnClick()
    {
        if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
        {
            mPageIndex = (m_CurrentPack.Count - 1) / mPageCount;
            if (!isMission)
            {
                ResetItem(mCurrentPickTab, mPageIndex);
                //ResetPage();
            }
            else
                ResetMissionItem(mPageIndex);
        }
    }

    void OnResort()
    {
        if (mSplitOpWnd.gameObject.activeSelf)
            return;

        //lz-2016.09.23 点刷新的时候取消丢弃的东西
        if (null != mDropItemWnd && mDropItemWnd.isShow)
        {
            mDropItemWnd.CancelDropItems();
        }

        if (!GameConfig.IsMultiMode)
        {
            if (!isMission)
            {
                mItemPackage.Sort((ItemPackage.ESlotType)mCurrentPickTab);
                ResetItem();
                //MainMidGui_N.Instance.UpdateLink();
            }
            else
            {
                mItemPackage.Sort(ItemPackage.ESlotType.Item);
                ResetMissionItem();
            }
        }
        else
        {
            if (!isMission)
                PlayerNetwork.mainPlayer.RequestSortPackage(mCurrentPickTab);
            else
            {
                //lz-2017.01.10 服务器给任务背包排序tabindex是-1不是0
                PlayerNetwork.mainPlayer.RequestSortPackage(-1);
            }
        }
    }
    //	
    void OnSplitBtn()
    {
        if (mSplitOpWnd.gameObject.activeSelf)
            return;
        if (mOpType == 1)
        {
            mOpType = 0;
            UICursor.Clear();
        }
        else
        {
            mOpType = 1;
            UICursor.Set(mNewUIAtlas, "icocai");
        }
        this.ResetCurOpItem();
    }
    //int curcount = 20;
    void OnDeleteBtn()
    {

        if (mSplitOpWnd.gameObject.activeSelf)
            return;
        if (mOpType == 2)
        {
            mOpType = 0;
            //			Menu.SetMouseState(Menu.MouseState.MS_Normal);
            UICursor.Clear();
        }
        else
        {
            mOpType = 2;
            UICursor.Set(mNewUIAtlas, "icodelete");
        }
        this.ResetCurOpItem();
    }

    void OnAddBtnPress()
    {
        mAddBtnPress = true;
        mOpStarTime = Time.time;
        mOpDurNum = 0;
    }

    void OnAddBtnRelease()
    {
        mAddBtnPress = false;
        mSplitNumDur = mSplitNumDur + mOpDurNum;
        mOpDurNum = 0;
        mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
    }

    void OnSubstructBtnPress()
    {
        mSubBtnPress = true;
        mOpStarTime = Time.time;
        mOpDurNum = 0;
    }

    void OnSubstructBtnRelease()
    {
        mSubBtnPress = false;
        mSplitNumDur = mSplitNumDur + mOpDurNum;
        mOpDurNum = 0;
        mSplitNumlabel.text = ((int)(mSplitNumDur + mOpDurNum)).ToString();
    }

    void OnSplitOkBtn()
    {
        if (m_CurOpItem == null || m_CurOpItem.ItemObj == null)
            return;

        if (!GameConfig.IsMultiMode)
        {
            if (!isMission)
            {
                ItemPackage accessor = mItemPackage;
                accessor.Split(m_CurOpItem.ItemObj.instanceId, (int)mSplitNumDur);

                //ItemObject addItem = ItemManager.Instance.CreateItem(mOpGrid.Item.mItemID);// single
                //addItem.CountUp((int)mSplitNumDur);
                //mOpGrid.ItemObj.DecreaseStackCount((int)mSplitNumDur);
                //mItemPackage.AddItem(addItem);
                ResetItem();
                this.ResetCurOpItem();
                //wan
                mOpBagID = -1;
            }
            else
            {
                ItemPackage accessor = mItemPackage;
                accessor.Split(m_CurOpItem.ItemObj.instanceId, (int)mSplitNumDur);
                ResetMissionItem();
                this.ResetCurOpItem();
                mOpBagID = -1;
                //非网络模式中对mission物品拆分
            }
        }
        else
        {
            PlayerNetwork.mainPlayer.RequestSplitItem(m_CurOpItem.ItemObj.instanceId, (int)mSplitNumDur);
            this.ResetCurOpItem();
            mOpBagID = -1;
            mSplitNumDur = 0;
        }

        mSplitOpWnd.gameObject.SetActive(false);
    }

    void OnSplitNoBtn()
    {
        this.ResetCurOpItem();
        //wan
        mOpBagID = -1;
        mSplitOpWnd.gameObject.SetActive(false);
    }

    void OnOpenDropWnd()
    {
        if (mDropItemWnd == null)
            return;

        //lz-2016.06.27 唐小丽说这个按钮允许打开和关闭，redmine 错误 #2526
        if (mDropItemWnd.isShow)
        {
            mDropItemWnd.Hide();
        }
        else
        {
            mDropItemWnd.Show();
        }
    }

    string GetcurrentMoney()
    {
        if (PeCreature.Instance == null)
            return "";

        if (PeCreature.Instance.mainPlayer == null)
            return "";

        PackageCmpt Package = PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>();
        if (Package != null)
        {
            Money mMoney = Package.money;
            if (mMoney != null)
                return mMoney.current.ToString();
        }
        return "";
    }

    //lz-2016.09.02 关闭所有操作窗口,主要是切换页签的时候用，避免切换页签后，当前操作的Grid的数据已经变了,导致操作对象的变化
    void CloseOpWnd()
    {
        mSplitOpWnd.gameObject.SetActive(false);
        if (GameUI.Instance.mShopWnd.isShow)
        {
            GameUI.Instance.mShopWnd.CloseSellWnd();
        }
        ResetCurOpItem();
    }

    #endregion

    #region public methods
    public void SetItempackage(ItemPackage itempackage)
    {
        mItemPackage = itempackage;
    }

    public void ResetItem()
    {
        if (gameObject && gameObject.activeSelf)
        {
            if (isMission)
                ResetMissionItem();
            else
                ResetItem(mCurrentPickTab, mPageIndex);
        }
    }

    public void ResetMissionItem()
    {
        if (gameObject.activeSelf)
            ResetMissionItem(mPageIndex);
    }

    public override void Show()
    {
        if (!isMission)
            ResetItem(mCurrentPickTab, mPageIndex);
        else
            ResetMissionItem(mPageIndex);
        base.Show();
        if (e_OnOpenPackage != null)
            e_OnOpenPackage();
    }

    public void ResetItem(int type, int pageIndex)
    {

        //		if(!mInit)
        //			return;
        isMission = false;

        //lz-2016.11.14 错误 #6193 Crush bug
        //lw_2017.7.10:判断玩家，玩家可能为null
        Pathea.PlayerPackageCmpt pkg = (null==Pathea.PeCreature.Instance || null == Pathea.PeCreature.Instance.mainPlayer)?null:Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();

        if (pkg == null)
        {
            return;
        }

        mItemPackage = pkg.package._playerPak;
		if (mItemPackage == null)
		{
			return;
		}

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
		if (m_CurrentPack == null)
		{
			return;
		}

        if ((m_CurrentPack.Count - 1) / mPageCount < pageIndex)
            pageIndex = (m_CurrentPack.Count - 1) / mPageCount;

        mPageIndex = pageIndex;

        int itemCount;

        if ((m_CurrentPack.Count - 1) / mPageCount == mPageIndex)
            itemCount = (m_CurrentPack.Count - pageIndex * mPageCount);
        else
            itemCount = mPageCount;

        DeleteCurPageTutorialEffect();
        int tempIndex = 0;
        for (int index = 0; index < itemCount; index++)
        {
			if(mItems[index] == null)	continue;

            tempIndex = index + pageIndex * mPageCount;
            ItemObject itemObj=m_CurrentPack[tempIndex];

            mItems[index].SetItem(itemObj, m_CurrentPack.newFlagMgr.IsNew(tempIndex));
            mItems[index].SetItemPlace(ItemPlaceType.IPT_Bag, tempIndex);

            if (null != itemObj)
            {
                CheckAddTutorialEffect(itemObj.protoId, mItems[index].transform);
            }

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

        if (e_OnResetItem != null)
            e_OnResetItem(mCurrentPickTab, mPageIndex);

        //luwei
        //GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
    }

    public void ResetMissionItem(int pageIndex)
    {

        //		if(!mInit)
        //			return;


        mItemPackage = PlayerPackage._missionPak;


        mMissionCheckbox.isChecked = true;

        m_CurrentPack = mItemPackage.GetSlotList(ItemPackage.ESlotType.Item);

        if ((m_CurrentPack.Count - 1) / mPageCount < pageIndex)
            pageIndex = (m_CurrentPack.Count - 1) / mPageCount;

        mPageIndex = pageIndex;

        int itemCount;
        itemCount = mPageCount;

        for (int index = 0; index < itemCount; index++)
        {
			mItems[index].SetItem(m_CurrentPack[index + pageIndex * mPageCount], m_CurrentPack.newFlagMgr.IsNew(index + pageIndex * mPageCount));
            mItems[index].SetItemPlace(ItemPlaceType.IPT_Bag, index + pageIndex * mPageCount);
            mItems[index].SetGridMask(GridMask.GM_Mission);
        }

        mPageCountText.text = (mPageIndex + 1).ToString() + "/" + ((m_CurrentPack.Count - 1) / mPageCount + 1);

        if (e_OnResetItem != null)
            e_OnResetItem(mCurrentPickTab, mPageIndex);

        //luwei
        //GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.SetStorageType(mCurrentPickTab, mPageIndex);
    }

    public void SetItemWithIndex(ItemObject itemGrid, int Index)
    {
        if (m_CurrentPack != null)
            m_CurrentPack[Index] = itemGrid;
        if (!isMission)
            ResetItem();
        else
            ResetMissionItem();
    }

    public bool RemoveItemPackgeByIndex(int Index)
    {
        if (m_CurrentPack != null)
        {
            m_CurrentPack[Index] = null;
            return true;
        }

        return false;
    }

    public void ExchangeItem(ItemObject newItem)
    {
        mItemPackage.AddItem(newItem);
        if (!isMission)
            ResetItem();
        else
            ResetMissionItem();
    }

    //	public void OnDeath()
    //	{
    //       if (!GameConfig.IsMultiMode)
    //       {
    //           for(int j = 0;j<3;j++)
    //           {
    //               List<ItemObject> itemList = mItemPackage.GetItemList(j);
    //               for (int i = 0; i < itemList.Count; i++)
    //               {
    //                   if (itemList[i] != null && itemList[i].mItemData.m_SetUp > 0)
    //                   {
    //                       int num = itemList[i].GetCount();
    //                       num = num / 20;
    //                       if(num > 0)
    //                           mItemPackage.DeleteItemWithItemID(itemList[i].mItemID, num);
    //                   }
    //               }
    //           }
    //           ResetItem();
    //       }
    //	}

    public void Sell()
    {
        if (null==m_CurOpItem||null==m_CurOpItem.ItemObj)
            return;
        if (m_CurOpItem.ItemObj.protoData.category == "Quest Item")
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(82209003));
            return;
        }
        int id = m_CurOpItem.ItemObj.protoId;
        if (GameUI.Instance.mShopWnd.IsOpen())
            GameUI.Instance.mShopWnd.Sell(m_CurOpItem, m_CurOpItem.Item.GetCount());
        else
            GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.SellAllItemByPackage(m_CurOpItem.ItemObj); //lz-2016.10.21 贸易站全部卖出

        CheckMission(id);
        ResetItem();
        this.ResetCurOpItem();
    }

    public void DeleteSelectedItem()
    {
        if (null==m_CurOpItem||null==m_CurOpItem.ItemObj)
            return;

        if (m_CurOpItem.ItemObj.protoData.category == "Quest Item")//任务物品
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(82209003));
            return;
        }
        int id = m_CurOpItem.ItemObj.protoId;
        if (!GameConfig.IsMultiMode)
        {
            ItemMgr.Instance.DestroyItem(m_CurOpItem.ItemObj.instanceId);
            CheckMission(m_CurOpItem.ItemObj.protoId);
            m_CurrentPack[m_CurOpItem.ItemIndex] = null;
            this.ResetCurOpItem();
            //wan
            mOpBagID = -1;
            ResetItem();
            //MainMidGui_N.Instance.UpdateLink();
        }
        else
        {
            if (!isMission)
                PlayerNetwork.mainPlayer.RequestDeleteItem(m_CurOpItem.ItemObj.instanceId, mCurrentPickTab, mOpBagID);

            mItemPackage.RemoveItem(m_CurOpItem.ItemObj);
            this.ResetCurOpItem();
            mOpBagID = -1;
        }
        CheckMission(id);
    }
    
    //lz-2016.08.10 重置当前操作的Item
    public void ResetCurOpItem()
    {
        this.m_CurOpItem = null;
    }

    public void OnLeftMouseCliked(Grid_N grid)
    {
        if (null==GameUI.Instance||GameUI.Instance.bMainPlayerIsDead)
            return;

        if (null==grid||null==grid.Item) return;

        //lz-2016.08.10 不允许重复操作同一个Item
        if (EqualUsingItem(grid.Item,false)) return;

        if (onItemSelected != null) onItemSelected(grid.Item as ItemObject);

        ActiveWnd();

        CheckRemoveTutorialItemProtoID(grid.Item.protoId);

        switch (mOpType)
        {
            case 0:
                if (!isMission)//任务物品不能卖
                {
                    if (GameUI.Instance.mShopWnd.isShow)
                    {
                        m_CurOpItem = grid;
                        GameUI.Instance.mShopWnd.PreSell(grid);
                        return;
                    }
                    //lz-2016.12.09 错误 #7164 Crash bug
                    else if (null!=GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI&&GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.IsShowAndCanUse) //lz-2016.10.21 贸易战卖出
                    {
                        m_CurOpItem = grid;
                        GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.SellItemByPakcage(m_CurOpItem.ItemObj);
                        return;
                    }
                }
                SelectItem_N.Instance.SetItemGrid(grid);
                break;
            case 1:
                if (Input.GetMouseButtonDown(0))
                {
                    if (grid.Item.GetCount() > 1)
                    {
                        int mark = mItemPackage.GetVacancySlotIndex(grid.Item.protoData.tabIndex);
                        if (-1 == mark)
                        {
                            MessageBox_N.ShowOkBox(PELocalization.GetString(8000053));
                        }
                        else if (m_CurOpItem == null)
                        {
                            mSplitOpWnd.gameObject.SetActive(true);
                            m_CurOpItem = grid;
                            mSplitNumDur = 1;
                            mSplitNumlabel.text = "1";
                            //wan
                            mOpBagID = grid.ItemIndex;
                        }
                    }
                }
                break;
            case 2:
                if (Input.GetMouseButtonDown(0))
                {
                    m_CurOpItem = grid;
                    //wan
                    mOpBagID = grid.ItemIndex;
                    if (m_CurOpItem.Item.protoId / 10000000 == 9)
                        MessageBox_N.ShowOkBox(PELocalization.GetString(8000054));
                    else
                        MessageBox_N.ShowYNBox(PELocalization.GetString(8000055), DeleteSelectedItem, ResetCurOpItem);
                }
                break;
        }
        if (!isMission)
            ResetItem();
        else
            ResetMissionItem();
    }//修改完成，未测试

    public void OnRightMouseCliked(Grid_N grid)
    {
        //lz-2016.11.14 错误 #6191 Crush bug
        if (null==Pathea.PeCreature.Instance|| null==PeCreature.Instance.mainPlayer|| Pathea.PeCreature.Instance.mainPlayer.IsDeath()||null==GameUI.Instance)
            return;

        if (grid.Item == null) return;

        //lz-2016.08.10 不允许重复操作同一个Item
        if (EqualUsingItem(grid.Item,false)) return;

        CheckRemoveTutorialItemProtoID(grid.Item.protoId);

        Pathea.UseItemCmpt useItem = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.UseItemCmpt>();
        if (null == useItem)
        {
            useItem =Pathea.PeCreature.Instance.mainPlayer.Add<Pathea.UseItemCmpt>();
        }

        ItemObject _item = grid.ItemObj;

        //if (isMission)
        //{
        //    // useItem.RightMouseClickMissionItem(_item);
        //}
        //else
        //{
        //    switch (CurrentPickTab)
        //    {
        //        case 0:
        //            //useItem.RightMouseClickNormalItem(_item);
        //            break;

        //        case 1:
        //            //useItem.RightMouseClickResourceItem(_item);
        //            break;

        //        case 2:
        //            //useItem.RightMouseClickEquipmentItem(_item);
        //            break;

        //        case 3:
        //            useItem.RightMouseClickArmorItem(_item);
        //            return;
        //    }
        //}

        // TO BE REBUILD :

        if (GameConfig.IsMultiMode)//联机模式
        {
            if (mOpType == 0 && mDropItemWnd.isShow && !isMission)
            {
                mDropItemWnd.AddToDropList(CurrentPickTab, mPageIndex, grid);
            }
            else if (mOpType == 0 && (GameUI.Instance.mShopWnd.isShow || (GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.IsShowAndCanUse)) && !isMission)
            {
                m_CurOpItem = grid;
                //lz-2016.10.27 卖出所有的时候提示加上卖出所有的价格
                int count = m_CurOpItem.Item.GetCount();
                int price= m_CurOpItem.ItemObj.GetSellPrice();
                string name = m_CurOpItem.Item.protoData.GetName();
                string msgStr = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000056), name + " X " + count, PELocalization.GetString(8000253), (count * price));
                MessageBox_N.ShowYNBox(msgStr, Sell,this.ResetCurOpItem);
            }
            else if (onRightMouseCliked != null && !isMission)//与仓库有关，暂时不知道功能
            {
                onRightMouseCliked(grid);
            }
            else //不知道功能
            {
                if (CurrentPickTab == 3)
                    useItem.RightMouseClickArmorItem(_item);
                else
                    useItem.Request(grid.ItemObj);

            }
        }
        else//单机
        {
            if (mOpType == 0 && (GameUI.Instance.mShopWnd.IsOpen()|| GameUI.Instance.mCSUI_MainWndCtrl.TradingPostUI.IsShowAndCanUse)&& !isMission)
            {
                m_CurOpItem = grid;
                //lz-2016.10.27 卖出所有的时候提示加上卖出所有的价格
                int count = m_CurOpItem.Item.GetCount();
                int price = m_CurOpItem.ItemObj.GetSellPrice();
                string name = m_CurOpItem.Item.protoData.GetName();
                string msgStr = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000056), name + " X " + count, PELocalization.GetString(8000253), (count * price));
                MessageBox_N.ShowYNBox(msgStr, Sell, this.ResetCurOpItem);
            }
            else if (GameUI.Instance.mWarehouse.IsOpen() && !isMission)
            {
                if (GameUI.Instance.mWarehouse.SetItemWithIndex(grid.ItemObj))
                {
                    m_CurrentPack[grid.ItemIndex] = null;
                    grid.SetItem(null);
                }
            }
            // ---[CSUI]
            else if (onRightMouseCliked != null && !isMission)
            {
                onRightMouseCliked(grid);
                m_CurrentPack[grid.ItemIndex] = grid.ItemObj;
            }

            else if (GameUI.Instance.mServantWndCtrl.isShow && GameUI.Instance.mServantWndCtrl.ServantIsNotNull)
            {
                if (!GameUI.Instance.mServantWndCtrl.EquipItem(grid.ItemObj))
                {
                    if (true == GameUI.Instance.mServantWndCtrl.SetItemWithIndex(grid.ItemObj))
                    {
                        m_CurrentPack[grid.ItemIndex] = null;
                        if (!isMission)
                            ResetItem();
                        else
                            ResetMissionItem();
                    }
                }
                else
                {
                    m_CurrentPack[grid.ItemIndex] = null;
                    if (!isMission)
                        ResetItem();
                    else
                        ResetMissionItem();
                }
            }
            else if (GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.IsShow && GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc != null && GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.IsRandomNpc())
            {
                if (!GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.EquipItem(grid.ItemObj))
                {
                    if (true == GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteractionItemWithIndex(grid.ItemObj)
                        || GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteraction2ItemWithIndex(grid.ItemObj))
                    {
                        m_CurrentPack[grid.ItemIndex] = null;
                        if (!isMission)
                            ResetItem();
                        else
                            ResetMissionItem();
                    }
                }
                else
                {
                    m_CurrentPack[grid.ItemIndex] = null;
                    if (!isMission)
                        ResetItem();
                    else
                        ResetMissionItem();
                }
            }
            else
            {
                if (CurrentPickTab == 3)
                    useItem.RightMouseClickArmorItem(_item);
                else
                    useItem.Request(grid.ItemObj);
            }
        }
    }

    //public delegate void MedicineRealOp(ItemPackage _ip, int _tabIndex, int _index, int _instanceId, bool _inorout);
    //public event MedicineRealOp mMedicineRealOp;

    public void OnDropItem(Grid_N grid)
    {
        if (null == SelectItem_N.Instance.ItemObj)
            return;

        if (GameUI.Instance.bMainPlayerIsDead)
            return;

        //if (onLeftMouseEndDrag != null)
        //    onLeftMouseEndDrag();
        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
            return;

        //lz-2016.09.23 在丢弃东西的时候不允许背包内部操作拖动，或者拖入其他东西
        if (null != mDropItemWnd && mDropItemWnd.isShow&& SelectItem_N.Instance.Place!=ItemPlaceType.IPT_DropItem)
        {
            return;
        }

        if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Hospital)//医疗所
        {
            if (grid.ItemObj != null)
            {
                SelectItem_N.Instance.SetItem(null);
                return;
            }

            if (SelectItem_N.Instance.ItemObj.protoData.category == "Quest Item")//任务物品
            {
                if (isMission)
                {
                    CSUI_Hospital.Instance.mMedicineRealOp(mItemPackage, isMission, 0, grid.ItemIndex, SelectItem_N.Instance.ItemObj.instanceId, false);
                }
            }
            else
            {
                if (!isMission)
                {
                    CSUI_Hospital.Instance.mMedicineRealOp(mItemPackage, isMission, mCurrentPickTab, grid.ItemIndex, SelectItem_N.Instance.ItemObj.instanceId, false);
                }
            }
            SelectItem_N.Instance.SetItem(null);
            return;
        }
        else if (null!=SelectItem_N.Instance.ItemObj&& SelectItem_N.Instance.Place == ItemPlaceType.IPT_DropItem&&mDropItemWnd.isShow)
        {
            mDropItemWnd.RemoveFromDropList(SelectItem_N.Instance.Grid);
        }

        if (GameConfig.IsMultiMode)
        {
            if (SelectItem_N.Instance.Index != grid.ItemIndex && SelectItem_N.Instance.Place == ItemPlaceType.IPT_Bag)//背包内拖动物品
            {
                PlayerNetwork.mainPlayer.RequestExchangeItem(SelectItem_N.Instance.ItemObj, SelectItem_N.Instance.Index, grid.ItemIndex);
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Equipment)//玩家装备物品
            {
                //lz-2016.11.09 检测是否可以脱装备
                if (SelectItem_N.Instance.RemoveOriginItem())
                {
                    PlayerNetwork.mainPlayer.RequestTakeOffEquipment(SelectItem_N.Instance.ItemObj);
                }
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_PublicInventory)
            {
                PlayerNetwork.mainPlayer.RequestPublicStorageFetch(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ServantEqu)//仆从装备
            {
                //lz-2016.11.09 检测是否可以脱装备
                if (SelectItem_N.Instance.RemoveOriginItem())
                { 
                    int npcId = GameUI.Instance.mServantWndCtrl.GetCurServantId;
                    if (-1 == npcId)
                        return;

                    PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
                    //lz-2016.08.31 脱下装备成功播放音效
                    GameUI.Instance.PlayTakeOffEquipAudio();
                }
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ServantInteraction)//仆从互动
            {
                int npcId = GameUI.Instance.mServantWndCtrl.GetCurServantId;
                if (-1 == npcId)
                    return;

                PlayerNetwork.mainPlayer.RequestGetItemFromNpc(0, npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ServantInteraction2)
            {
                int npcId = GameUI.Instance.mServantWndCtrl.GetCurServantId;
                if (-1 == npcId)
                    return;

                PlayerNetwork.mainPlayer.RequestGetItemFromNpc(1, npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ConolyServantEquPersonel)//基地个人界面NPC装备物品
            {
                //lz-2016.11.09 检测是否可以脱装备
                if (SelectItem_N.Instance.RemoveOriginItem())
                {
                    int npcId = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.Id;
                    if (-1 == npcId)
                        return;

                    PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
                    //lz-2016.08.31 脱下装备成功播放音效
                    GameUI.Instance.PlayTakeOffEquipAudio();
                }
            }
            //lz-2016.09.02 基地个人界面NPC背包1
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ColonyServantInteractionPersonel)
            {
                int npcId = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.Id;
                if (-1 == npcId)
                    return;

                PlayerNetwork.mainPlayer.RequestGetItemFromNpc(0, npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
            }
            //lz-2016.09.02 基地个人界面NPC背包2
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ColonyServantInteraction2Personel)
            {
                int npcId = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.RefNpc.Id;
                if (-1 == npcId)
                    return;

                PlayerNetwork.mainPlayer.RequestGetItemFromNpc(1, npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ConolyServantEquTrain)//基地训练所界面NPC装备物品
            {
                int npcId = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.m_TrainNpcInfCtrl.Npc.m_Npc.Id;
                if (-1 == npcId)
                    return;

                PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
                //lz-2016.08.31 脱下装备成功播放音效
                GameUI.Instance.PlayTakeOffEquipAudio();
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_ConolyServantInteractionTrain)//基地训练所界面NPC互动物品
            {
                int npcId = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.m_TrainNpcInfCtrl.Npc.m_Npc.Id;
                if (-1 == npcId)
                    return;

                PlayerNetwork.mainPlayer.RequestGetItemFromNpc(0, npcId, SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_CSStorage)
            {
                PlayerNetwork.mainPlayer.RequestPersonalStorageFetch(SelectItem_N.Instance.ItemObj.instanceId, grid.ItemIndex);
            }
            else if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_Rail)
            {
                PERailwayCtrl.Instance.RemoveTrain(grid.ItemIndex);
            }
            SelectItem_N.Instance.SetItem(null);
            return;
        }

        if (grid.ItemObj == null)
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_HotKeyBar:
                    SelectItem_N.Instance.SetItem(null);
                    break;
                case ItemPlaceType.IPT_NPCStorage:
                    if (!isMission)
                    {
                        grid.SetItem(SelectItem_N.Instance.ItemObj);
                        m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
                        SelectItem_N.Instance.RemoveOriginItem();
                    }
                    SelectItem_N.Instance.SetItem(null);

                    break;
                default:
                    if (!isMission)//非任务
                    {
                        if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != mCurrentPickTab)
                            return;
                    }
                    else//任务
                    {
                        if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != 0)//标签为item
                            return;
                        if (SelectItem_N.Instance.GridMask != GridMask.GM_Mission)//并且物品的Mask为Mission
                            return;
                    }

                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        grid.SetItem(SelectItem_N.Instance.ItemObj);
                        m_CurrentPack[grid.ItemIndex] = SelectItem_N.Instance.ItemObj;
                    }
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
        else
        {
            if (!isMission)
            {
                if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != mCurrentPickTab)
                    return;
            }
            else
            {
                if (SelectItem_N.Instance.ItemObj.protoData.tabIndex != 0)
                    return;
            }
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_Bag:
                    //SelectItem_N.Instance.SetItem(grid.ItemObj,grid.ItemPlace,grid.ItemIndex);
                    ItemObject temp = SelectItem_N.Instance.ItemObj;
                    m_CurrentPack[SelectItem_N.Instance.Index] = grid.ItemObj;
                    m_CurrentPack[grid.ItemIndex] = temp;
                    SelectItem_N.Instance.SetItem(null);
                    if (!isMission)
                        ResetItem();
                    else
                        ResetMissionItem();
                    break;
                case ItemPlaceType.IPT_Equipment:
                    SelectItem_N.Instance.SetItem(null);
                    break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }

    public void RestItemState()
    {
        mOpType = 0;
        this.ResetCurOpItem();
        mSplitOpWnd.gameObject.SetActive(false);
        UICursor.Clear();
    }

    /// <summary>
    /// lz-2016.08.10 用来外部检测某个Item是不是玩家包裹正在操作的东西
    /// </summary>
    /// <returns></returns>
    public bool EqualUsingItem(ItemSample item,bool showUsingTip=true)
    {
        if (null == item || null == CurOperateItem)
            return false;
        if (CurOperateItem == item)
        {
            if (showUsingTip)
                PeTipMsg.Register(PELocalization.GetString(8000623), PeTipMsg.EMsgLevel.Error);
            return true;
        }
        else
            return false;
    }


    #endregion

    #region Grid Tutorial Effect
    [SerializeField]
    private ItemPackageGridTutorial_N m_GridTutorialPrefab;
    List<int> m_NeedTutorialItemID = new List<int>();
    List<ItemPackageGridTutorial_N> m_CurPageGridTutorials = new List<ItemPackageGridTutorial_N>();
    /// <summary> 添加需要显示Tutorial的protoID</summary>
    public void AddTutorialItemProtoID(int protoID)
    {
        if (PeGameMgr.IsTutorial&& !m_NeedTutorialItemID.Contains(protoID))
        {
            m_NeedTutorialItemID.Add(protoID);
            if (base.isShow&& null!=mItems&& mItems.Count>0)
            {
                for (int i = 0; i < mItems.Count; i++)
                {
                    if (null == mItems[i]) continue;
                    if (null != mItems[i].Item)
                        CheckAddTutorialEffect(mItems[i].Item.protoId, mItems[i].transform);
                }
            }
        }
    }

    /// <summary> 移除Tutorial的protoID</summary>
    private void CheckRemoveTutorialItemProtoID(int protoID)
    {
        if(m_NeedTutorialItemID.Contains(protoID))
        { 
            m_NeedTutorialItemID.Remove(protoID);
            List<ItemPackageGridTutorial_N> findList = m_CurPageGridTutorials.FindAll(a => a.ProtoID ==protoID);
            if (null != findList && findList.Count > 0)
            {
                ItemPackageGridTutorial_N tempItem = null;
                for (int i = 0; i < findList.Count; i++)
                {
                    tempItem = findList[i];
                    tempItem.gameObject.SetActive(false);
                    Destroy(tempItem.gameObject);
                    m_CurPageGridTutorials.Remove(tempItem);
                }
            }
        }
    }

    /// <summary>检测添加Tutorial特效 </summary>
    private void CheckAddTutorialEffect(int protoID, Transform trans)
    {
        if (PeGameMgr.IsTutorial && m_NeedTutorialItemID.Count > 0)
        {
            if (m_NeedTutorialItemID.Contains(protoID))
            {
                ItemPackageGridTutorial_N item = Instantiate(m_GridTutorialPrefab.gameObject).GetComponent<ItemPackageGridTutorial_N>();
                item.gameObject.SetActive(false);
                item.SetProtoID(protoID);
                item.transform.parent = trans.parent;
                item.transform.localPosition = trans.localPosition;
                item.transform.localScale = Vector3.one;
                item.gameObject.SetActive(true);
                m_CurPageGridTutorials.Add(item);
            }
        }
    }

    /// <summary> 删除当前页的TutorialEffect</summary>
    private void DeleteCurPageTutorialEffect()
    {
        for (int i = 0; i < m_CurPageGridTutorials.Count; i++)
        {
            m_CurPageGridTutorials[i].gameObject.SetActive(false);
            Destroy(m_CurPageGridTutorials[i].gameObject);
        }
        m_CurPageGridTutorials.Clear();
    }

    #endregion
}
