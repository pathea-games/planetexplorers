using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AiAsset;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtPlayerPackage;
using System.Linq;

public class UIShopWnd : UIBaseWnd
{
    int mRow = 5;
    int mColumn = 10;
    int mPageCount;

    public UILabel mPageCountText;

    public Grid_N mGridPrefab;

    public UIPanel mSellOpLayer;
    public UILabel mPriceLabel;
    public UIInput mOpNumLabel;
    public UILabel mTotalLabel;

    public UILabel mOKBtn;
    public Grid_N mOpItem;

    public UICheckbox mBtnAll;
    public UILabel mLbNpcMoney;
    public UILabel mLbNpcName;
    public UITexture mTxNpcIco;
    public UISprite mSpNpcIco;
    public Transform mItemGridsContent;

    public UISlicedSprite mMeatSprite;
    public UISlicedSprite mMoneySprite;

    List<Grid_N> mItems;

    int mPageIndex;
    int mCurrentPickTab = 0;

    public int CurrentTab { get { return mCurrentPickTab; } }
    bool mIsBuy;
    int MaxPage;
    public bool isShopping { private set; get; }

    Grid_N mOpGrid;

    int mPrice;
    float mCurrentNum;

    float mOpDurNum = 0;

    List<ItemObject> mBuyItemList;
    List<ItemObject> mRepurchaseList;

    List<ItemObject> mTypeOfBuyItemList;
    public ItemLabel.Root shopSelectItemType;

    public List<ItemObject> RepurchaseList { get { return mRepurchaseList; } }

    List<ItemObject> m_CurrentPack;

    List<int> m_ShopIDList;
    List<int> m_TypeShopIDList;
    public int m_CurNpcID;
    public PeEntity npc = null;

    float mOpStarTime;
    float mLastOpTime;

    bool mAddBtnPress = false;
    bool mSubBtnPress = false;


    protected override void InitWindow()
    {
        base.InitWindow();

    }

    public override void OnCreate()
    {
        base.OnCreate();
        mItems = new List<Grid_N>();
        mPageCount = mRow * mColumn;
        for (int i = 0; i < mPageCount; i++)
        {
            mItems.Add(Instantiate(mGridPrefab) as Grid_N);
            mItems[i].transform.parent = mItemGridsContent;
            mItems[i].transform.localPosition = new Vector3(-234 + i % mColumn * 52, 92 - i / mColumn * 54, -1);
            mItems[i].transform.localRotation = Quaternion.identity;
            mItems[i].transform.localScale = Vector3.one;
            mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
            mItems[i].onRightMouseClicked = OnRightMouseCliked;
        }
        mCurrentPickTab = 0;
        mPageIndex = 0;

        mBuyItemList = new List<ItemObject>();
        mRepurchaseList = new List<ItemObject>();
        m_CurrentPack = new List<ItemObject>();
        mTypeOfBuyItemList = new List<ItemObject>();


        m_ShopIDList = new List<int>();
        m_TypeShopIDList = new List<int>();

        if (Money.Digital == false)
        {
            mMeatSprite.gameObject.SetActive(true);
            mMoneySprite.gameObject.SetActive(false);
        }
        else
        {
            mMeatSprite.gameObject.SetActive(false);
            mMoneySprite.gameObject.SetActive(true);
        }
    }

    Vector3 prePos;

    public override void Show()
    {
        mSellOpLayer.gameObject.SetActive(false);
        mOpItem.mShowNum = false;

        mBtnAll.isChecked = true;
        shopSelectItemType = ItemLabel.Root.all;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);

        npc = EntityMgr.Instance.Get(m_CurNpcID);
        EntityInfoCmpt info = npc.GetCmpt<EntityInfoCmpt>();
        if (npc != null)
        {
            //npc.CmdIdle = true;
            if (info != null)
                SetNpcName(info.characterName.fullName);
            NpcPackageCmpt npcpc = npc.GetCmpt<NpcPackageCmpt>();
            if (npcpc == null)
                return;

            SetNpcMoney(npcpc.money.current);

            Texture icon_tex = null;
            string icon_str = "";
            if (EntityCreateMgr.Instance.IsRandomNpc(npc))
            {
                icon_tex = npc.ExtGetFaceTex();
                if (icon_tex == null)
                    icon_str = npc.ExtGetFaceIcon();
            }
            else
                icon_str = npc.ExtGetFaceIcon();

            if (icon_tex != null)
                SetNpcICO(icon_tex);
            else
                SetNpcICO(icon_str);

            StroyManager.Instance.SetTalking(EntityMgr.Instance.Get(m_CurNpcID));
            isShopping = true;
            //Invoke("ShopSetTalking", 0.25f);
        }


        if (!GameUI.Instance.mItemPackageCtrl.isShow)
            GameUI.Instance.mItemPackageCtrl.Show();

        prePos = GameUI.Instance.mItemPackageCtrl.transform.localPosition;

        GameUI.Instance.mItemPackageCtrl.transform.localPosition = UIDefaultPostion.Instance.pos_ItemPackge;
        transform.localPosition = UIDefaultPostion.Instance.pos_Shop;

        base.Show();
    }

    protected override void OnClose()
    {
        GameUI.Instance.mItemPackageCtrl.transform.localPosition = prePos;
        base.OnClose();
    }

    public void Sell(Grid_N grid, int num)
    {
        //lz-2016.08.10 执行Sell的时候重置包裹当前的操作Item
        GameUI.Instance.mItemPackageCtrl.ResetCurOpItem();

        if (null == grid || null == grid.ItemObj)
        {
            Debug.LogError("Sell item is null");
            return;
        }

        int proid = grid.ItemObj.protoId;
        int instanceid = grid.ItemObj.instanceId;
		int maxStackCount = grid.ItemObj.GetStackMax();
        int cost = grid.ItemObj.GetSellPrice();//(int)(grid.ItemObj.prototypeData.m_CurrencyValue * num * (0.8f * grid.ItemObj.Durability / grid.ItemObj.DurabilityMax + 0.2f));
        //        string itemname = grid.ItemObj.protoData.GetName();


        if (npc == null)
            return;

        NpcPackageCmpt npcpc = npc.GetCmpt<NpcPackageCmpt>();
        if (npcpc == null)
            return;

        cost *= num;

        if (cost > npcpc.money.current)
        {
            string errInfo = "";
            EntityInfoCmpt cmpt = npc.GetCmpt<EntityInfoCmpt>();
            if (cmpt != null)
            {
                string _name = "";
//                if (npc.entityProto.proto == EEntityProto.RandomNpc)
//                {
//                    _name = cmpt.characterName.fullName;
//                }
//                else if (npc.entityProto.proto == EEntityProto.Npc)
//                {
//                    _name = cmpt.characterName.givenName;
//                }
				_name = cmpt.characterName.fullName;
                //lz-2016.10.31 have no money to pay for you!
                errInfo = _name + " " + PELocalization.GetString(8000853);
            }
            else
            {
                //lz-2016.10.31 have no money to pay for you!
                errInfo = npc.name + " "+ PELocalization.GetString(8000853);
            }
            new PeTipMsg(errInfo, PeTipMsg.EMsgLevel.Error);
            return;
        }

        //Add money to player
        if (PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RequestSell(npc.Id, instanceid, num);
        }
        else
        {
            npcpc.money.current -= cost;

			
			ItemObject SellItemObj;
			if(maxStackCount==1)
				SellItemObj = grid.ItemObj;
            else
				SellItemObj = ItemMgr.Instance.CreateItem(proid);
            PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            if (null == pkg)
            {
                return;
            }
			if(maxStackCount==1)
				pkg.Remove(SellItemObj);
            else
				pkg.DestroyItem(instanceid, num);
            //if (grid.ItemObj.GetCount() > num)
            //{
            //    SellItemObj = ItemMgr.Instance.CreateItem(grid.ItemObj.protoId); // single
            //    SellItemObj.IncreaseStackCount(num);
            //    grid.ItemObj.DecreaseStackCount(num);
            //}
            //else
            //{
            //    SellItemObj = grid.ItemObj;
            //    PlayerFactory.mMainPlayer.GetItemPackage().RemoveItem(grid.ItemObj);
            //    GameUI.Instance.mUIItemPackageCtrl.ResetItem();
            //}
            //SellItemObj.SetProperty(ItemProperty.SellItemMark, 1f);
			AddRepurchase(SellItemObj, num);

            //            if (cost != 0 && !PeCreature.Instance.mainPlayer.AddToPkg(StroyManager.PRICE_ID, cost))
            //            {
            //                return;
            //            }

            pkg.money.current += cost;
            ResetItem();
        }


        //GameGui_N.Instance.mChatGUI.AddChat(PromptData.PromptType.PromptType_14, itemname, num, cost);

        mOpItem.SetItem(null);
        mOpGrid = null;
        mSellOpLayer.gameObject.SetActive(false);

        return;
    }


    public bool UpdataShop(StoreData npc)
    {
        if (!mInit)
            InitWindow();

        PeEntity ainpc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
        if (ainpc == null)
            return false;

        GameUI.Instance.mNpcWnd.Hide();
        mBuyItemList.Clear();
        if (npc == null)
            return false;

        int iSize = npc.itemList.Count;
        if (iSize < 1)
            return false;

        m_ShopIDList.Clear();
        m_CurNpcID = ainpc.Id;

        if (!StroyManager.Instance.m_BuyInfo.ContainsKey(m_CurNpcID))
            StroyManager.Instance.InitBuyInfo(npc, m_CurNpcID);

        Dictionary<int, stShopData> shoplist = StroyManager.Instance.m_BuyInfo[m_CurNpcID].ShopList;
        bool bPass = true;
        foreach (int key in shoplist.Keys)
        {
            ShopData data = ShopRespository.GetShopData(key);
            if (data == null)
                continue;

            bPass = true;
            for (int i = 0; i < data.m_LimitMisIDList.Count; i++)
            {
                if (data.m_LimitType == 1)
                {
                    if (MissionManager.Instance.HadCompleteMission(data.m_LimitMisIDList[i]))
                        break;
                }
                else
                {
                    if (!MissionManager.Instance.HadCompleteMission(data.m_LimitMisIDList[i]))
                    {
                        bPass = false;
                        break;
                    }
                }
            }

            if (!bPass)
                continue;

            if (GameTime.Timer.Second - shoplist[key].CreateTime > data.m_RefreshTime)
            {
                if (shoplist[key].ItemObjID != 0)
                    ItemMgr.Instance.DestroyItem(shoplist[key].ItemObjID);

                ItemObject itemObj = ItemMgr.Instance.CreateItem(data.m_ItemID); // single
                itemObj.stackCount = data.m_LimitNum;
                shoplist[key].ItemObjID = itemObj.instanceId;
                //itemObj.SetProperty(ItemProperty.NewFlagTime, 0f);
				shoplist[key].CreateTime = GameTime.Timer.Second;
                mBuyItemList.Add(itemObj);
                m_ShopIDList.Add(data.m_ID);
            }
            else
            {
                if (shoplist[key].ItemObjID == 0)
                    continue;
                else
                {
                    ItemObject itemObj = ItemMgr.Instance.Get(shoplist[key].ItemObjID);
                    if (null != itemObj)
                    {
                        //itemObj.stackCount = data.m_LimitNum;
                        shoplist[key].ItemObjID = itemObj.instanceId;
                        //itemObj.SetProperty(ItemProperty.NewFlagTime, 0f);
						shoplist[key].CreateTime = GameTime.Timer.Second;
                        mBuyItemList.Add(itemObj);
                        m_ShopIDList.Add(data.m_ID);
                    }
                }

            }
        }

        mRepurchaseList.Clear();
        List<ItemObject> selllist;
        if (StroyManager.Instance.m_SellInfo.ContainsKey(m_CurNpcID))
        {
            selllist = StroyManager.Instance.m_SellInfo[m_CurNpcID];
            for (int i = 0; i < selllist.Count; i++)
                mRepurchaseList.Add(selllist[i]);
        }
        else
            StroyManager.Instance.m_SellInfo.Add(m_CurNpcID, new List<ItemObject>());

		CSMain.AddTradeNpc(m_CurNpcID,shoplist.Keys.ToList());
        ResetItem();
        return true;
    }


    public void AddRepurchase(ItemObject item, int num)
    {
        if (num > 1)
            item.SetStackCount(num);

        mRepurchaseList.Add(item);
    }

    public void RemoveBuyItem(int index, int num)
    {

        if (mBuyItemList[index].GetCount() < num)
            Debug.LogError("Remove num is big than item you have.");
        else if (mBuyItemList[index].GetCount() > num)
            mBuyItemList[index].DecreaseStackCount(num);
        else
        {
            mBuyItemList.RemoveAt(index);
            if (index >= 0 && index < m_ShopIDList.Count)
                m_ShopIDList.RemoveAt(index);
        }
    }

    public void RemoveRepurchase(int index, int num)
    {
        if (mRepurchaseList.Count <= index || mRepurchaseList[index].GetCount() < num)
        {
            Debug.LogError("Remove num is big than item you have.");
            return;
        }

        if (mRepurchaseList[index].GetCount() > num)
            mRepurchaseList[index].DecreaseStackCount(num);
        else
            mRepurchaseList.RemoveAt(index);
    }



    public void ResetItem(int type, int pageIndex)
    {
        if (type == 0)
        {
            UpdateBuyItemList(shopSelectItemType);
            m_CurrentPack = mTypeOfBuyItemList;

        }
        else
            m_CurrentPack = mRepurchaseList;

        MaxPage = (m_CurrentPack.Count - 1) / mPageCount;
        if (MaxPage < 0)
            MaxPage = 0;

        if (MaxPage < pageIndex)
            pageIndex = MaxPage;

        mPageIndex = pageIndex;

        int itemCount;

        if (MaxPage == mPageIndex)
            itemCount = (m_CurrentPack.Count - pageIndex * mPageCount);
        else
            itemCount = mPageCount;

        for (int index = 0; index < mPageCount; index++)
        {
            if (index < itemCount)
                mItems[index].SetItem(m_CurrentPack[index + mPageCount * mPageIndex]);
            else
                mItems[index].SetItem(null);

            mItems[index].SetItemPlace(ItemPlaceType.IPT_Shop, index + pageIndex * mPageCount);
            mItems[index].SetGridMask(GridMask.GM_Any);
        }

        mPageCountText.text = (mPageIndex + 1).ToString() + "/" + ((m_CurrentPack.Count - 1) / mPageCount + 1);
    }

    public void ResetItem()
    {
        ResetItem(mCurrentPickTab, mPageIndex);
        if (npc == null)
            return;

        NpcPackageCmpt npcpc = npc.GetCmpt<NpcPackageCmpt>();
        if (npcpc == null)
            return;

        SetNpcMoney(npcpc.money.current);
    }




    void Buy(Grid_N grid, int num)
    {
        //lw:2017.4.6 crash bug ：NullReferenceException
        if (null == m_TypeShopIDList || null == m_ShopIDList)
            return;

        bool repurchase;
        int shopID = 0;
        int cost = 0;
        if (mCurrentPickTab == 0)
        {
            repurchase = false;
            if (m_TypeShopIDList.Count <= grid.ItemIndex)
                return;

            shopID = m_TypeShopIDList[grid.ItemIndex];

            ShopData data = ShopRespository.GetShopData(shopID);
            if (data != null)
                cost = data.m_Price;
        }
        else
        {
            repurchase = true;
            if (grid.ItemObj.protoData != null)
            {
                cost = grid.ItemObj.protoData.currency;
                mSellOpLayer.gameObject.SetActive(false);
            }
        }

        if (PeGameMgr.IsMulti)
        {
            mSellOpLayer.gameObject.SetActive(false);

            if (repurchase)
                PlayerNetwork.mainPlayer.RequestRepurchase(npc.Id, grid.ItemObj.instanceId,num);
            else
                PlayerNetwork.mainPlayer.RequestBuy(npc.Id, grid.ItemObj.instanceId, num);
            return;
        }
        else
        {
            //lz-2016.09.19 购买失败情况比较多，详细提示放在里面了
            if (!StroyManager.Instance.BuyItem(grid.ItemObj, num, shopID, m_CurNpcID, !repurchase))
            {
                //lz-2016.09.19 失败需要返回，避免后面删除Npc的东西和给npc加钱
                return;
            }

            //if (!PlayerFactory.mMainPlayer.BuyItem(grid.ItemObj, num, shopID, m_CurNpcName, !repurchase))
            //{
            //    MessageBox_N.ShowOkBox(PELocalization.GetString(8000076));
            //        return;
            //}
        }
        cost *= num;
        //GameGui_N.Instance.mChatGUI.AddChat(PromptData.PromptType.PromptType_13, grid.ItemObj.protoData.GetName(), num, cost);

        //if (grid.ItemObj.GetCount() > num)
        //{
        //    grid.ItemObj.IncreaseStackCount(num);
        //}
        //else
        //{
        //    m_CurrentPack[grid.ItemIndex] = null;
        //    grid.SetItem(null);
        //    mOpGrid = null;
        //    if(mCurrentPickTab == 1)
        //        mRepurchaseList.RemoveAt(grid.ItemIndex);
        //}

        
        if (npc != null)
        {
            NpcPackageCmpt npcpc = npc.GetCmpt<NpcPackageCmpt>();
            if (npcpc == null)
                return;

            npcpc.money.current += cost;
        }

        //lz-2016.09.19 是回购就移除回购的，不是回购就移除正常的
        if (repurchase)
        {
            RemoveRepurchase(grid.ItemIndex, num);
        }
        else
        {
            int index = -1;
            if (grid.ItemIndex < m_TypeShopIDList.Count)
            {
                for (int i = 0; i < m_ShopIDList.Count; i++)
                {
                    if (m_ShopIDList[i] == m_TypeShopIDList[grid.ItemIndex])
                    {
                        index = i;
                        break;
                    }
                }

                mSellOpLayer.gameObject.SetActive(false);
                if (index!=-1)
                {
                    RemoveBuyItem(index, num);
                }
            }
        }

        ResetItem();
    }

    public void PreSell(Grid_N grid)
    {
        if (grid.ItemObj == null)
            return;

        mSellOpLayer.gameObject.SetActive(true);
        mIsBuy = false;
        mOpGrid = grid;
        mOpItem.SetItem(grid.ItemObj);
        mCurrentNum = 1;

        mPrice = grid.ItemObj.GetSellPrice();// (int)(mOpGrid.Item.prototypeData.m_CurrencyValue * (0.8f * grid.ItemObj.Durability / grid.ItemObj.DurabilityMax + 0.2f));

        mOpNumLabel.text = mCurrentNum.ToString();
        mPriceLabel.text = mPrice.ToString();
        mTotalLabel.text = mPrice.ToString();
        mOKBtn.text = PELocalization.GetString(8000555);

        mOpItem.SetItem(grid.ItemObj);
    }

    public void CloseSellWnd()
    {
        if (mSellOpLayer.gameObject.activeSelf&&!mIsBuy)
        {
            mSellOpLayer.gameObject.SetActive(false);
        }
    }


    public void OnLeftMouseCliked(Grid_N grid)
    {
        if (grid.ItemObj == null)
            return;

        if (grid.ItemObj.GetCount() == 0)
        {
            new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
            return;
        }

        ActiveWnd();

        if (mSellOpLayer.gameObject.activeSelf)
            return;

        SelectItem_N.Instance.SetItem(null);

        mSellOpLayer.gameObject.SetActive(true);
        mIsBuy = true;
        mOpGrid = grid;
        mOpItem.SetItem(grid.ItemObj);
        mCurrentNum = 1;

        if (mCurrentPickTab == 0)
        {
            if (m_TypeShopIDList.Count <= grid.ItemIndex)
                return;

            int shopID = m_TypeShopIDList[grid.ItemIndex];
            ShopData shopData = ShopRespository.GetShopData(shopID);
            if (shopData == null)
                mPrice = 0;
            else
                mPrice = grid.ItemObj.GetBuyPrice();
        }
        else
            mPrice = grid.ItemObj.GetSellPrice();


        mOpNumLabel.text = mCurrentNum.ToString();
        mPriceLabel.text = mPrice.ToString();
        mTotalLabel.text = mPrice.ToString();
        mOKBtn.text = PELocalization.GetString(8000556);    
    }

    public void OnRightMouseCliked(Grid_N grid)
    {
        if (grid.ItemObj == null)
            return;

        if (grid.ItemObj.GetCount() == 0)
        {
            new PeTipMsg(PELocalization.GetString(82209007), PeTipMsg.EMsgLevel.Warning);
            return;
        }

        if (mSellOpLayer.gameObject.activeSelf)
            return;

        ActiveWnd();

        SelectItem_N.Instance.SetItem(null);

        mOpGrid = grid;

        //lz-2016.10.27 购买所有的时候提示加上购买所有的价格
        int count = mOpGrid.Item.GetCount();
		int price;
		if(mRepurchaseList.Contains(mOpGrid.ItemObj))
			price = mOpGrid.ItemObj.GetSellPrice(); 
		else
			price = mOpGrid.ItemObj.GetBuyPrice();

        string name = mOpGrid.Item.protoData.GetName();
        string msgStr = string.Format("{0} {1}\n{2} {3}", PELocalization.GetString(8000077), name + " X " + count, PELocalization.GetString(8000253), (count * price));
        MessageBox_N.ShowYNBox(msgStr, BuyAll);
    }

    public void BuyAll()
    {
        //lz-2016.06.14 修改【错误 #2351买东西点太快报错】
        if (null == mOpGrid || null == mOpGrid.ItemObj)
            return;
        Buy(mOpGrid, mOpGrid.ItemObj.GetCount());
    }

    void OnBuyBtn()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }

    void OnSellBtn()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;
        mCurrentPickTab = 1;
        ResetItem(mCurrentPickTab, 0);
    }

    void OnOpOkBtn()
    {
        if (null == mOpGrid)
            return;
        if (mIsBuy)
            Buy(mOpGrid, (int)mCurrentNum);
        else
            Sell(mOpGrid, (int)mCurrentNum);
    }

    void OnOpCancelBtn()
    {
        mSellOpLayer.gameObject.SetActive(false);
        //lz-2016.08.10 执行CancelSell的时候重置包裹当前的操作Item
        GameUI.Instance.mItemPackageCtrl.ResetCurOpItem();
    }

    void OnPageDown()
    {
        if (mPageIndex > 0)
        {
            mPageIndex -= 1;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
    }

    void OnPageUp()
    {
        if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
        {
            mPageIndex += 1;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
    }


    //lz-2017.01.16 翻到最后一页
    void BtnRightEndOnClick()
    {
        if (mPageIndex < (m_CurrentPack.Count - 1) / mPageCount)
        {
            mPageIndex = (m_CurrentPack.Count - 1) / mPageCount;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
    }

    //lz-2017.01.16 翻到第一页
    void BtnLeftEndOnClick()
    {
        if (mPageIndex > 0)
        {
            mPageIndex = 0;
            ResetItem(mCurrentPickTab, mPageIndex);
        }
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
        mCurrentNum = (int)(mCurrentNum + mOpDurNum);
        mOpDurNum = 0;
        mOpNumLabel.text = ((int)(mCurrentNum)).ToString();
        mTotalLabel.text = (mPrice * (int)(mCurrentNum)).ToString();
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
        mCurrentNum = (int)(mCurrentNum + mOpDurNum);
        mOpDurNum = 0;
        mOpNumLabel.text = ((int)(mCurrentNum)).ToString();
        mTotalLabel.text = (mPrice * (int)(mCurrentNum)).ToString();
    }

    protected override void OnHide()
    {
        if (mInit && isShow)
        {
            PeEntity npc = EntityMgr.Instance.Get(m_CurNpcID);
            //lz-2016.10.09 这里不可以返回，不然后面关闭执行不到
            if (npc != null)
            {
                StroyManager.Instance.RemoveReq(npc, EReqType.Dialogue);
            }
            
            if (!PeGameMgr.IsMulti && StroyManager.Instance.m_SellInfo != null)
            {
                if (StroyManager.Instance.m_SellInfo.ContainsKey(m_CurNpcID))
                {
                    StroyManager.Instance.m_SellInfo[m_CurNpcID].Clear();
                    for (int i = 0; i < mRepurchaseList.Count; i++)
                    {
                        StroyManager.Instance.m_SellInfo[m_CurNpcID].Add(mRepurchaseList[i]);
                    }
                }
            }
            m_CurNpcID = 0;
        }
        base.OnHide();
        isShopping = false;
    }

    #region MutilPlayerMode
    /// <summary>
    /// 多人模式初始化商店所售物品
    /// </summary>
    public void InitNpcShopWhenMultiMode(int npcid, int[] ids)
    {
        if (!mInit)
            InitWindow();
        //npc = NpcManager.Instance.GetNpc(npcid);
        //m_CurNpcName = npc.NpcName;

        GameUI.Instance.mNpcWnd.Hide();

        mBuyItemList.Clear();
        m_ShopIDList.Clear();
        foreach (int id in ids)
        {
            ItemObject itemObj = ItemMgr.Instance.Get(id);
            if (null != itemObj)
            {
                //LogManager.Debug("a item is on sell!ID:" + objID);
                ShopData data = ShopRespository.GetShopDataByItemId(itemObj.protoId);
                if (data == null)
                {
                    LogManager.Error("data is null! itemID = ", itemObj.protoId);
                    mBuyItemList.Add(null);
                    m_ShopIDList.Add(-1);
                    continue;
                }

                //itemObj.CountUp(data.m_LimitNum);
                mBuyItemList.Add(itemObj);
                m_ShopIDList.Add(data.m_ID);
            }
            else
            {
                LogManager.Error("itemObj is null!");
                mBuyItemList.Add(null);
                m_ShopIDList.Add(-1);
                continue;
            }

        }

        m_CurNpcID = npcid;

        //mRepurchaseList.Clear();
        //foreach (int objID in repurcaseId)
        //{
        //	ItemObject itemObj = ItemMgr.Instance.Get(objID);

        //	if (null != itemObj)
        //	{
        //		//LogManager.Debug("a item is on sell!ID:" + objID);
        //		mRepurchaseList.Add(itemObj);
        //	}
        //	else
        //	{
        //		mRepurchaseList.Add(null);
        //		continue;
        //	}

        //}

        ResetItem();

        if (!isShow)
        {
            Show();
        }
    }


    public bool InitShopWhenMutipleMode(int[] objIDs)
    {
        if (!mInit)
            InitWindow();
        return UpdataShop(objIDs);
    }
    public bool UpdataShop(int[] objIDs)
    {
        mBuyItemList.Clear();
        m_ShopIDList.Clear();
        foreach (int objID in objIDs)
        {
            ItemObject itemObj = ItemMgr.Instance.Get(objID);

            if (null != itemObj)
            {
                //LogManager.Debug("a item is on sell!ID:" + objID);
                ShopData data = ShopRespository.GetShopDataByItemId(itemObj.protoId);
                if (data == null)
                {
                    LogManager.Error("data is null! itemID = ", itemObj.protoId);
                    mBuyItemList.Add(null);
                    m_ShopIDList.Add(-1);
                    continue;
                }

                //itemObj.CountUp(data.m_LimitNum);
                mBuyItemList.Add(itemObj);
                m_ShopIDList.Add(data.m_ID);
            }
            else
            {
                LogManager.Error("itemObj is null! ID = ", objID);
                mBuyItemList.Add(null);
                m_ShopIDList.Add(-1);
                continue;
            }
        }
        return true;
    }

    public bool AddNewItemOnSell(int objID, int index)
    {
        ItemObject itemObj = ItemMgr.Instance.Get(objID);

        if (null != itemObj)
        {
            LogManager.Info("An item is on sell!ID:", objID);
            ShopData data = ShopRespository.GetShopDataByItemId(itemObj.protoId);
            itemObj.IncreaseStackCount(data.m_LimitNum);
            mBuyItemList[index] = itemObj;
            m_ShopIDList[index] = data.m_ID;
        }
        else
        {
            LogManager.Error("itemObj is null! ID = ", objID);
            return false;
        }
        return true;
    }

    public bool AddNewRepurchaseItem(int objID)
    {
        ItemObject itemObj = ItemMgr.Instance.Get(objID);
        if (null != itemObj)
        {
            LogManager.Info("a item is on repurchase!ID:", objID);
            mRepurchaseList.Add(itemObj);
        }
        else
        {
            LogManager.Error("itemObj is null! ID = ", objID);
            return false;
        }
        return true;
    }

    public bool RemoveRepurchaseItem(int objID)
    {
        ItemObject itemObj = ItemMgr.Instance.Get(objID);
        if (itemObj != null)
        {
            mRepurchaseList.Remove(itemObj);
            LogManager.Info("RemoveRepurchase: objID=" + objID);
            ResetItem();
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    void Update()
    {
        if (null != mOpGrid && null != mOpGrid.Item)
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


                mOpDurNum = Mathf.Clamp(mOpDurNum + mCurrentNum, 1, mOpGrid.ItemObj.GetCount()) - mCurrentNum;
                mOpNumLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
                mTotalLabel.text = (mPrice * (int)(mOpDurNum + mCurrentNum)).ToString();
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

                mOpDurNum = Mathf.Clamp(mOpDurNum + mCurrentNum, 1, mOpGrid.ItemObj.GetCount()) - mCurrentNum;
                mOpNumLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
                mTotalLabel.text = (mPrice * (int)(mOpDurNum + mCurrentNum)).ToString();
            }
            else
            {
                if ("" == mOpNumLabel.text)
                    mCurrentNum = 1;
                else
                    mCurrentNum = Mathf.Clamp(System.Convert.ToInt32(mOpNumLabel.text), 1, mOpGrid.Item.GetCount());
                if (!UICamera.inputHasFocus)
                {
                    mOpNumLabel.text = mCurrentNum.ToString();
                    mTotalLabel.text = (mPrice * (int)mCurrentNum).ToString();
                }
            }
        }
    }


    void UpdateBuyItemList(ItemLabel.Root _type)
    {
        //lz-2016.01.03 crash bug 错误 #8074
        if (mTypeOfBuyItemList == null|| null==mBuyItemList||null== m_TypeShopIDList|| null==m_ShopIDList)
            return;

        mTypeOfBuyItemList.Clear();


        for (int i = 0; i < mBuyItemList.Count; i++)
        {
            if (_type == ItemLabel.Root.all) mTypeOfBuyItemList.Add(mBuyItemList[i]);
            else
            {
                //lz-2017.02.27 错误 #9323 crash bug
                if (null!= mBuyItemList[i] && null != mBuyItemList[i].protoData && null != mBuyItemList[i].protoData)
                {
                    ItemLabel.Root roottype = mBuyItemList[i].protoData.rootItemLabel;
                    if (roottype == _type)
                    {
                        mTypeOfBuyItemList.Add(mBuyItemList[i]);
                    }
                }
            }
        }

        m_TypeShopIDList.Clear();
        for (int i = 0; i < m_ShopIDList.Count; i++)
        {
            if (_type == ItemLabel.Root.all)
                m_TypeShopIDList.Add(m_ShopIDList[i]);
            else
            {
                ShopData sd = ShopRespository.GetShopData(m_ShopIDList[i]);
                if (sd == null)
                    continue;
                ItemProto data = ItemProto.GetItemData(sd.m_ItemID);
                if (null != data)
                {
                    ItemLabel.Root roottype = data.rootItemLabel;
                    if (roottype == _type)
                    {
                        m_TypeShopIDList.Add(m_ShopIDList[i]);
                    }
                }
            }
        }
    }



    void SetNpcName(string strName)
    {
        mLbNpcName.text = strName;
    }

    void SetNpcMoney(int npcMoney)
    {
        mLbNpcMoney.text = npcMoney.ToString();
    }

    void SetNpcICO(string _sprName)
    {
        if (mSpNpcIco == null)
            return;

        mSpNpcIco.spriteName = _sprName;
        mSpNpcIco.gameObject.SetActive(true);
        mTxNpcIco.gameObject.SetActive(false);
    }

    void SetNpcICO(Texture _contentTexture)
    {
        if (mSpNpcIco == null)
            return;

        mTxNpcIco.mainTexture = _contentTexture;
        mTxNpcIco.gameObject.SetActive(true);
        mSpNpcIco.gameObject.SetActive(false);
    }

    #region menu item on click

    //--------------------------

    void BtnAllOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.all;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }

    void BtnWeaponOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.weapon;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }

    void BtnEquipOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.equipment;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);

    }

    void BtnToolOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.tool;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);

    }

    void BtnTurretOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.turret;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }


    void BtnConsumOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.consumables;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }

    void BtnResoureOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.resoure;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }

    void BtnPartOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.part;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }

    void BtnDecorationOnClick()
    {
        if (mSellOpLayer.gameObject.activeSelf)
            return;

        shopSelectItemType = ItemLabel.Root.decoration;
        mCurrentPickTab = 0;
        ResetItem(mCurrentPickTab, 0);
    }

    void BtnRebuyOnClick()
    {
        OnSellBtn();
    }

    #endregion



}
