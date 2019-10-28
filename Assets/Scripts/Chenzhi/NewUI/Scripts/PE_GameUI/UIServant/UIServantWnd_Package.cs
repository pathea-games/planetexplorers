using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Pathea;
using ItemAsset;

public partial class UIServantWnd : UIBaseWnd
{
    #region Package Func
    public bool SetItemWithIndex(ItemObject itemObj, int index = -1)
    {
        //lz-2016.08.15 往往仆从背包放东西，如果仆从背包页签没打开，就切换到仆从页签
        if (!mCkInventory.isChecked)
        {
            mCkInventory.isChecked = true;
        }

        if (index == -1)
            return mInteractionPackage.Add(itemObj);
        else
        {
            if (index < 0 || index > mInteractionPackage.Count)
                return false;
            if (mInteractionPackage != null)
                mInteractionPackage[index] = itemObj;
        }
        return true;
    }

    public bool SetItemWithIndexWithPackage2(ItemObject itemObj, int index = -1)
    {
        if (index == -1)
            return mInteraction2Package.Add(itemObj);
        else
        {
            if (index < 0 || index > mInteraction2Package.Count)
                return false;
            if (mInteraction2Package != null)
                mInteraction2Package[index] = itemObj;
        }
        return true;
    }

    void GetNpcPakgeSlotList()
    {
        if (mInteractionPackage != null)
            mInteractionPackage.eventor.Unsubscribe(InteractionpackageChange);

        mInteractionPackage = packageCmpt.GetSlotList();
        mInteractionPackage.eventor.Subscribe(InteractionpackageChange);

        if (mInteraction2Package != null)
            mInteraction2Package.eventor.Unsubscribe(InteractionPackage2Change);
        mInteraction2Package = packageCmpt.GetHandinList();
        mInteraction2Package.eventor.Subscribe(InteractionPackage2Change);

        if (mPrivatePakge != null)
            mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);

        mPrivatePakge = packageCmpt.GetPrivateSlotList();
        mPrivatePakge.eventor.Subscribe(PrivatepackageChange);
        // reset equipment receiver
        //servantEqReceiver.ResetPackage(packageCmpt);
    }

    void Reflashpackage()
    {
        RefreshInteractionpackage();
        ReflashPrivatePackage();
        RefreshInteractionPackage2();
    }

    void ClearNpcPackage()
    {
        ClearInteractionpackage();
        ClearPrivatePackage();
        //lz-2016.06.16 清除npc第二个包裹
        ClearInteractionpackage2();
    }

    //Interactionpackage
    void InteractionpackageChange(object sender, SlotList.ChangeEvent arg)
    {
        RefreshInteractionpackage();
    }
    void RefreshInteractionpackage()
    {
        ClearInteractionpackage();
        for (int i = 0; i < mInteractionGridCount; i++)
        {
            if (mInteractionPackage == null)
            {
                mInteractionList[i].SetItem(null);
            }
            else
            {
                mInteractionList[i].SetItem(mInteractionPackage[i]);
            }

        }
    }

    void ClearInteractionpackage()
    {
        foreach (Grid_N item in mInteractionList)
            item.SetItem(null);
    }

    void InteractionPackage2Change(object sender, SlotList.ChangeEvent arg)
    {
        RefreshInteractionPackage2();
    }

    void RefreshInteractionPackage2()
    {
        ClearInteractionpackage2();
        for (int i = 0; i < mInteraction2GridCount; i++)
        {
            if (mInteraction2Package == null)
            {
                mInteraction2List[i].SetItem(null);
            }
            else
            {
                mInteraction2List[i].SetItem(mInteraction2Package[i]);
            }

        }
    }

    void ClearInteractionpackage2()
    {
        foreach (Grid_N item in mInteraction2List)
            item.SetItem(null);
    }

    //Privatepackage
    int mPageIndex = 1;
    int mMaxPageIndex { get { return mPrivatePakge == null ? 1 : mPrivatePakge.Count / mPrivateItemGridCount; } }
    void PrivatepackageChange(object sender, SlotList.ChangeEvent arg)
    {
        ReflashPrivatePackage();
    }

    void ReflashPrivatePackage()
    {
        ClearPrivatePackage();
        int startIndex = (mPageIndex - 1) * mPrivateItemGridCount;
        for (int i = 0; i < mPrivateList.Count; i++)
        {
            mPrivateList[i].SetItem(mPrivatePakge[startIndex + i]);
        }
    }

    void ClearPrivatePackage()
    {
        foreach (Grid_N item in mPrivateList)
            item.SetItem(null);
    }
    #endregion

    #region ui event
    public void OnLeftMouseCliked_InterPackage(Grid_N grid)
    {
        if (null == servant)
            return;
        SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
    }

    public void OnRightMouseCliked_InterPackage(Grid_N grid)
    {
        if (null == servant)
            return;

        Pathea.UseItemCmpt useItem = servant.GetCmpt<Pathea.UseItemCmpt>();
        if (null == useItem)
            useItem = servant.Add<Pathea.UseItemCmpt>();

        if (true == useItem.Request(grid.ItemObj))
        {
            //mInteractionPackage[grid.ItemIndex] = null;
            //ReflashInteractionpackage();
            //				Reflash();
        }
    }

    public void OnDropItem_InterPackage(Grid_N grid)
    {
        if (null == servant)
            return;

        if (grid.ItemObj != null)
            return;

        if (PeGameMgr.IsMulti)
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ServantEqu:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    }
                    break;
                case ItemPlaceType.IPT_ServantInteraction2:
                case ItemPlaceType.IPT_Bag:
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    break;
            }
            SelectItem_N.Instance.SetItem(null);
        }
        else
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ServantEqu:
                case ItemPlaceType.IPT_ServantInteraction:
                case ItemPlaceType.IPT_ServantInteraction2:
                case ItemPlaceType.IPT_Bag:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        SetItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
                        grid.SetItem(SelectItem_N.Instance.ItemObj);
                        SelectItem_N.Instance.SetItem(null);
                    }
                    break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }

    public void OnLeftMouseCliked_InterPackage2(Grid_N grid)
    {
        if (null == servant)
            return;
        SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
    }

    public void OnRightMouseCliked_InterPackage2(Grid_N grid)
    {
        if (null == servant)
            return;

        Pathea.UseItemCmpt useItem = servant.GetCmpt<Pathea.UseItemCmpt>();
        if (null == useItem)
            useItem = servant.Add<Pathea.UseItemCmpt>();

        if (true == useItem.Request(grid.ItemObj))
        {
            //mInteractionPackage[grid.ItemIndex] = null;
            //ReflashInteractionpackage();
            //				Reflash();
        }
    }

    public void OnDropItem_InterPackage2(Grid_N grid)
    {
        if (null == servant)
            return;

        if (grid.ItemObj != null)
            return;

        if (PeGameMgr.IsMulti)
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ServantEqu:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    }
                    break;
                case ItemPlaceType.IPT_ServantInteraction:
                case ItemPlaceType.IPT_Bag:
                        PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
                    break;
            }

            SelectItem_N.Instance.SetItem(null);
        }
        else
        {
            switch (SelectItem_N.Instance.Place)
            {
                case ItemPlaceType.IPT_ServantEqu:
                case ItemPlaceType.IPT_ServantInteraction:
                case ItemPlaceType.IPT_ServantInteraction2:
                case ItemPlaceType.IPT_Bag:
                    //lz-2016.11.09 检测是否可以脱装备
                    if (SelectItem_N.Instance.RemoveOriginItem())
                    {
                        SetItemWithIndexWithPackage2(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
                        grid.SetItem(SelectItem_N.Instance.ItemObj);
                        SelectItem_N.Instance.SetItem(null);
                    }
                    break;
                default:
                    SelectItem_N.Instance.SetItem(null);
                    break;
            }
        }
    }
    #endregion

    //ServantEqReceiver servantEqReceiver = new ServantEqReceiver();
}


//#region EquipmentCmpt.Receiver
//// Interaction package receiver
//class ServantEqReceiver : EquipmentCmpt.Receiver
//{
//    NpcPackageCmpt packageCmpt = null;
//    public void ResetPackage(NpcPackageCmpt package){ packageCmpt = package;}

//    bool EquipmentCmpt.Receiver.CanAddItemList(List<ItemObject> items)
//    {
//        if (packageCmpt == null)
//            return false;
//        return packageCmpt.CanAddPrivateItemList(items);
//    }

//    void EquipmentCmpt.Receiver.AddItemList(List<ItemObject> items)
//    {
//        if (packageCmpt == null)
//            return;
//        packageCmpt.AddPrivateItemList(items);
//    }
//}
//#endregion

