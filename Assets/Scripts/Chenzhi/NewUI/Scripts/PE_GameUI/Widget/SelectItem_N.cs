using ItemAsset;
using ItemAsset.PackageHelper;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemPlaceType
{
    IPT_Null,
    IPT_Bag,
    IPT_HotKeyBar,
    IPT_Equipment,
    IPT_ServantEqu,
    IPT_ServantInteraction,
    IPT_ServantInteraction2,
    IPT_ServantSkill,
    IPT_ConolyServantEquPersonel,
    IPT_ColonyServantInteractionPersonel,//lz-2016.09.02 基地NPC背包分成两个枚举
    IPT_ColonyServantInteraction2Personel, 
    IPT_ConolyServantSkillPersonel,
    IPT_ConolyServantEquTrain,
    IPT_ConolyServantInteractionTrain,
    IPT_ConolyServantSkillTrain,
    IPT_Hospital,
    IPT_Warehouse,
    IPT_Shop,
    IPT_ItemGet,
    IPT_HBStorage,
    IPT_HBEngineering,
    IPT_HBNPCEqu,
    IPT_HBNPCUse,
    IPT_HBNPCSkill,
    IPT_Repair,
    IPT_PublicInventory,
    // CS Enum
    IPT_CSStorage,
    IPT_ItemBox,
    IPT_DropItem,
    IPT_Rail,
    IPT_NPCStorage,
    IPT_ColonyTradingPost //lz-2016.10.20 基地贸易战
}

public class SelectItem_N : MonoBehaviour
{
    static SelectItem_N mInstance;
    public static SelectItem_N Instance { get { return mInstance; } }

    public UIAtlas mIconAtlas;

    public UIAtlas mButtonUIAtlas;

    public UITexture mIcontex;

    ItemPlaceType mItemPlace;
    ItemSample mItemSample;
    int mItemIndex;
    Grid_N mGrid;
    ItemSkillMar mItemSkillmar;
    public ItemSkillMar ItemSkillmar { get { return mItemSkillmar; } }

    public Grid_N Grid { get { return mGrid; } }

    public ItemPlaceType Place { get { return mItemPlace; } }
    public ItemObject ItemObj { get { return mItemSample as ItemObject; } }
    public ItemSample ItemSample { get { return mItemSample; } }
    public int Index { get { return mItemIndex; } }

    UISprite mItemSpr;
    UITexture mItemTex;


    bool mPutBackFlag = false;
    bool mDiscardFlag = false;
    bool mPutEnable = false;
    bool mCreatEnable = false;
    bool mHasCreated = false;

    //AssetReq mCurrenReq;

    GridMask mGridMask = GridMask.GM_Any;
    public GridMask GridMask { get { return mGridMask; } }

    void Awake()
    {
        mInstance = this;
        mItemPlace = ItemPlaceType.IPT_Null;
        mItemSample = null;
        mItemIndex = 0;

        mItemSkillmar = new ItemSkillMar();
        ItemMgr.Instance.DestoryItemEvent -= DestoryItemEvent;
        ItemMgr.Instance.DestoryItemEvent += DestoryItemEvent;
    }

    void Update()
    {
        CheckDrawItemState();

        UpdateObjectTransform();

        CheckIfCreat();

        CheckItemPutDownAction();
    }

    void CheckDrawItemState()
    { 
        if (mDiscardFlag)
        {
            mDiscardFlag = false;
            if (null != mItemSample && mItemPlace == ItemPlaceType.IPT_HotKeyBar)
                GameUI.Instance.mUIMainMidCtrl.RemoveItemWithIndex(mItemIndex);

        }

        if (mPutBackFlag)
        {
            mPutBackFlag = false;
            CancelDrop();
        }

		if (PeInput.Get (PeInput.LogicFunction.Item_CancelDrag) || Input.GetMouseButtonUp(0))
            mPutBackFlag = true;

		if (Input.GetMouseButtonUp(0) && !PeInput.Get (PeInput.LogicFunction.Item_CancelDrag))
            mDiscardFlag = true;
    }

    void UpdateObjectTransform()
    {
        DraggingMgr.Instance.UpdateRay();

        mIcontex.transform.localPosition = transform.localPosition;

		mPutEnable = !(null != UICamera.hoveredObject);
		mCreatEnable = !(null != UICamera.hoveredObject);

        if (!mPutEnable)
        {
            return;
        }
        if (!mCreatEnable)
        {
            return;
        }

        if (PeInput.Get (PeInput.LogicFunction.Item_RotateItem))
        {
            DraggingMgr.Instance.Rotate();
        }
    }

    void CheckItemPutDownAction()
    {
		if(PeInput.Get (PeInput.LogicFunction.Item_Drop))
        {
            if (null == UICamera.hoveredObject && mPutEnable)
            {
                PutItemDown();
            }
            else
            {
                CancelDrop();
            }
        }
    }

    void CheckIfCreat()
    {
		if(PeInput.Get (PeInput.LogicFunction.Item_Drag))
        {
            if (null == UICamera.hoveredObject && mCreatEnable)
            {
                CreatTower();
            }
        }
    }

    public void SetItemGrid(Grid_N grid)
    {
        SetItem(grid.Item, grid.ItemPlace, grid.ItemIndex, grid.ItemMask);
        mGrid = grid;
    }

    void Clear()
    {
        Grid_N.SetActiveGrid(null);
        UICursor.Clear();
        mIcontex.enabled = false;
        mIcontex.mainTexture = null;
        //mCurrenReq = null;

        mItemSample = null;
    }

    public void SetItem(ItemSample itemSample, ItemPlaceType place = ItemPlaceType.IPT_Null, int index = 0, GridMask gridMask = GridMask.GM_Any)
    {
        mGridMask = gridMask;
        mItemSample = itemSample;
        mItemPlace = place;
        mItemIndex = index;
        if (mPutBackFlag)
            CancelDrop();
        mPutBackFlag = false;
        mGrid = null;

        if (mItemSample == null)
        {
            Clear();
            return;
        }

        if (null != Grid_N.mActiveGrid)
        {
            Grid_N.mActiveGrid.mSkillCooldown.fillAmount = 0;
            Grid_N.mActiveGrid = null;
        }

        GameUI.Instance.mItemPackageCtrl.RestItemState();
        //BuildBlockManager.self.QuitBuildMode();

        SetIcon(mItemSample.iconTex, mItemSample.iconString0);

        //ItemObject obj = mItemSample as ItemObject;
        //ItemAsset.Drag drag = null;
        //if (null != obj)
        //{
        //    drag = obj.GetCmpt<ItemAsset.Drag>();
        //}

        //if (obj != null && drag != null && (place == ItemPlaceType.IPT_Bag || place == ItemPlaceType.IPT_HotKeyBar)
        //    && !GameUI.Instance.bMainPlayerDead
        //    )
        //{
        //    ItemObjDragging dragging = new ItemObjDragging(drag);

        //    DraggingMgr.Instance.Begin(dragging);
        //}
        //else
        //{
        //    mCurrenReq = null;
        //}
        mHasCreated = false;
    }

    /// <summary>
    /// �����е�����������ϵ�����ʹ�ã��ͻ�����ʵ��������ɷ���
    /// </summary>
    void CreatTower()
    {
        if (mHasCreated)
            return;
        if (mItemSample == null)
            return;
        ItemObject obj = mItemSample as ItemObject;
        ItemAsset.Drag drag = null;
        if (null != obj)
        {
            drag = obj.GetCmpt<ItemAsset.Drag>();
        }
//        if (mItemPlace == null) // unreachable code !
//            return;
        if (obj != null && drag != null && (mItemPlace == ItemPlaceType.IPT_Bag || mItemPlace == ItemPlaceType.IPT_HotKeyBar)
            && !GameUI.Instance.bMainPlayerIsDead
            )
        {
            ItemObjDragging dragging = new ItemObjDragging(drag);

            DraggingMgr.Instance.Begin(dragging);
        }
        else
        {
            //mCurrenReq = null;
        }
        mHasCreated = true;
    }

    private void SetIcon(Texture iconTex, string iconString)
    {
        if (null != iconTex)
        {
            UICursor.Set(mIconAtlas, "Null");
            mIcontex.enabled = true;
            mIcontex.mainTexture = iconTex;
            mIcontex.transform.localScale = new Vector3(48, 48, 1);
        }
        else
        {
            UICursor.Set(mIconAtlas, iconString);
            mIcontex.enabled = false;
        }
    }

    void PutItemDown()
    {
        //BuildBlockManager.self.QuitBuildMode();
        DraggingMgr.Instance.End();
    }




    void CancelDrop()
    {
        DraggingMgr.Instance.Cancel();
        Clear();
    }

    public bool HaveOpItem()
    {
        return mItemSample != null;
    }

    public bool RemoveOriginItem()
    {
        bool result = false;
        switch (mItemPlace)
        {
            case ItemPlaceType.IPT_Bag:
                GameUI.Instance.mItemPackageCtrl.SetItemWithIndex(null, mItemIndex);
                result = true;
                break;
            case ItemPlaceType.IPT_Equipment:
                //lz-2016.11.09 里面需要处理多人
                result=GameUI.Instance.mUIPlayerInfoCtrl.RemoveEquipmentByIndex(mItemIndex);
                break;
            case ItemPlaceType.IPT_Warehouse:
                result=GameUI.Instance.mWarehouse.SetItemWithIndex(null, mItemIndex);
                break;
            case ItemPlaceType.IPT_ServantInteraction:
                result=GameUI.Instance.mServantWndCtrl.SetItemWithIndex(null, mItemIndex);
                break;
            case ItemPlaceType.IPT_ServantInteraction2:
                result=GameUI.Instance.mServantWndCtrl.SetItemWithIndexWithPackage2(null, mItemIndex);
                break;
            case ItemPlaceType.IPT_ServantEqu:
                //lz-2016.11.09 里面需要处理多人
                result = GameUI.Instance.mServantWndCtrl.RemoveEqByIndex(mItemIndex);
                break;
            case ItemPlaceType.IPT_ConolyServantInteractionTrain:
                result=GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetItemWithIndex(null, mItemIndex);
                break;
            case ItemPlaceType.IPT_ConolyServantEquTrain:
                result=GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetItemWithIndex(null, mItemIndex);
                break;
            case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
                result=GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteractionItemWithIndex(null, mItemIndex);
                break;
            case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
                result=GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCInfoUI.SetInteraction2ItemWithIndex(null, mItemIndex);
                break;
            case ItemPlaceType.IPT_ConolyServantEquPersonel:
                //lz-2016.11.09 里面需要处理多人
                result = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.EquipRemoveOriginItem(mItemIndex);
                break;
            case ItemPlaceType.IPT_Repair:
                GameUI.Instance.mRepair.RemoveItem();
                result = true;
                break;
            case ItemPlaceType.IPT_Rail:
                PERailwayCtrl.RemoveTrain(ItemObj);
                result = true;
                break;

        }

        // - [CSUI] RemoveOriginItem
        if (mGrid != null && mGrid.onRemoveOriginItem != null)
        {
            mGrid.onRemoveOriginItem(mGrid);
            result = true;
        }

        return result;
    }

    public void ExchangeItem(ItemObject io)
    {
        switch (mItemPlace)
        {
            case ItemPlaceType.IPT_Bag:
                GameUI.Instance.mItemPackageCtrl.ExchangeItem(io);
                break;
            default:
                break;
        }
    }

    /// <summary>Item对象在ItemMgr中删除的时候检测操作的对象是否相同，避免操作非法对象</summary>
    void DestoryItemEvent(int instanceId)
    {
        if (null != ItemObj && ItemObj.instanceId == instanceId)
        {
            SetItem(null);
        }
    }
}
