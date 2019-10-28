using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using System.IO;
using ItemAsset.PackageHelper;
using NetworkHelper;

public class PlayerPackage
{
    public ItemPackage _playerPak;
    public static ItemPackage _missionPak;
    string quest_item = "Quest Item";

    public PlayerPackage(int itemMax,bool createMisPkg = true)
    {
        
        if (createMisPkg)
            _missionPak = new ItemPackage(itemMax);
        _playerPak = new ItemPackage(itemMax);
    }
    #region package
    public void ExtendPackage(int itemMax, int equipmentMax, int recourceMax, int armMax)
    {
        _playerPak.ExtendPackage(itemMax, equipmentMax, recourceMax, armMax);
        _missionPak.ExtendPackage(itemMax, equipmentMax, recourceMax, armMax);
    }

    public void Clear(ItemPackage.ESlotType type = ItemPackage.ESlotType.Max, bool isMissionPkg = false)
    {
        if (type == ItemPackage.ESlotType.Max)
        {
            GetSlotList(ItemPackage.ESlotType.Item).Clear();
            GetSlotList(ItemPackage.ESlotType.Equipment).Clear();
            GetSlotList(ItemPackage.ESlotType.Resource).Clear();
            GetSlotList(ItemPackage.ESlotType.Armor).Clear();
            GetSlotList(ItemPackage.ESlotType.Item, true).Clear();
        }
        else
        {
            if (isMissionPkg)
                _missionPak.GetSlotList(type).Clear();
            else
                _playerPak.GetSlotList(type).Clear();
        }
        if (isMissionPkg)
            _missionPak.changeEventor.Dispatch(new ItemPackage.EventArg() { op = ItemPackage.EventArg.Op.Clear, itemObj = null });
        else
            _playerPak.changeEventor.Dispatch(new ItemPackage.EventArg() { op = ItemPackage.EventArg.Op.Clear, itemObj = null });
    }

    public void UpdateNewFlag(float deltaTime, bool isMissionPkg = false)
    {
        if (isMissionPkg)
            _missionPak.UpdateNewFlag(deltaTime);
        else
            _playerPak.UpdateNewFlag(deltaTime);
    }

    public SlotList GetSlotList(int prototypeId)
    {
        ItemProto itemData = ItemProto.GetItemData(prototypeId);
        if (itemData == null)
            return null;
        if (itemData.category == quest_item)
            return _missionPak.GetSlotList(ItemPackage.GetSlotType(prototypeId));
        else
            return _playerPak.GetSlotList(ItemPackage.GetSlotType(prototypeId));
    }

    public SlotList GetSlotList(ItemPackage.ESlotType type, bool isMissionPkg = false)
    {
        if (isMissionPkg)
            return _missionPak.GetSlotList(type);
        return _playerPak.GetSlotList(type);
    }

    public bool CanAddItemList(IEnumerable<ItemObject> items)
    {
        if (items.Any(delegate(ItemObject o)
        {
            if (o.protoData.category == quest_item)
                return true;
            return false;
        }))
            return _missionPak.CanAddItemList(items);
        else
            return _playerPak.CanAddItemList(items);
    }

    public bool AddItemList(IEnumerable<ItemObject> items, bool isNew = false)
    {
        foreach (ItemObject obj in items)
        {
			AddItem(obj, isNew);
        }
        return true;
    }

	public int AddItem(ItemObject itemObject, bool isNew = false)
    {
        if (itemObject == null)
			return ItemPackage.InvalidIndex;
        if (itemObject.protoData.category == quest_item)
			return _missionPak.AddItem(itemObject, isNew);
        else
			return _playerPak.AddItem(itemObject, isNew);
    }

    public ItemObject FindItemByProtoId(int itemId)
    {
        ItemProto itemData = ItemProto.GetItemData(itemId);
        if (null == itemData)
            return null;

        if (itemData.category == quest_item)
            return _missionPak.FindItemByProtoId(itemId);
        else
            return _playerPak.FindItemByProtoId(itemId);
    }

    public int PutItem(ItemObject item, int slotIndex, ItemPackage.ESlotType slotType)
    {
        if (item.protoData.category == quest_item)
            return _missionPak.PutItem(item, slotIndex, slotType);
        else
            return _playerPak.PutItem(item, slotIndex, slotType);
    }

    public void PutItem(ItemObject item, int codedIndex)
    {
        if (item.protoData.category == quest_item)
            _missionPak.PutItem(item, codedIndex);
        else
            _playerPak.PutItem(item, codedIndex);
    }

    public ItemObject GetItem(int codeIndex, bool isMissionPkg = false)
    {
        if (isMissionPkg)
            return _missionPak.GetItem(codeIndex);
        return _playerPak.GetItem(codeIndex);
    }

    public ItemObject GetItem(ItemPackage.ESlotType slotType, int index, bool isMissionPkg = false)
    {
        if (isMissionPkg)
            return _missionPak.GetItem(slotType, index);
        return _playerPak.GetItem(slotType, index);
    }

    public bool RemoveItem(int codedIndex, bool isMissionPkg = false)
    {
        if (isMissionPkg)
            return _missionPak.RemoveItem(codedIndex);
        return _playerPak.RemoveItem(codedIndex);
    }

    public int GetItemIndexById(int instanceId, out bool isMissionPkg)               //通过实例化的ID，找ITEM
    {
        if (_playerPak.GetItemIndexById(instanceId) != -1)
        {
            isMissionPkg = false;
            return _playerPak.GetItemIndexById(instanceId);
        }
        else
        {
            isMissionPkg = true;
            return _missionPak.GetItemIndexById(instanceId);
        }
    }

    public bool RemoveItemById(int instanceId)
    {
        bool tmp;
        int codedIndex = GetItemIndexById(instanceId, out tmp);

        if (tmp)
            return _missionPak.RemoveItem(codedIndex);
        return RemoveItem(codedIndex);
    }

    public bool RemoveItem(ItemObject item)
    {
        return RemoveItemById(item.instanceId);
    }

    public bool HasItemObj(ItemObject itemObject)
    {
        if (itemObject == null)
            return false;
        if (itemObject.protoData.category == quest_item)
            return _missionPak.HasItemObj(itemObject);
        else
            return _playerPak.HasItemObj(itemObject);
    }

    public int GetVacancySlotIndex(ItemPackage.ESlotType slotType, bool isMissionPkg = false)
    {
        if (isMissionPkg)
            _missionPak.GetVacancySlotIndex(slotType);
        return _playerPak.GetVacancySlotIndex(slotType);
    }

    public void Sort(ItemPackage.ESlotType type)
    {
        _playerPak.Sort(type);
        _missionPak.Sort(type);
    }

    #region Serialize
	public void Export(BinaryWriter bw)
    {
        _playerPak.Export(bw);
    }

    public void Import(byte[] buffer)
    {
		_missionPak = new ItemPackage(Pathea.PlayerPackageCmpt.PkgCapacity);
        _playerPak.Import(buffer);
		_playerPak.ExtendPackage(Pathea.PlayerPackageCmpt.PkgCapacity, Pathea.PlayerPackageCmpt.PkgCapacity,
		                         Pathea.PlayerPackageCmpt.PkgCapacity, Pathea.PlayerPackageCmpt.PkgCapacity);
    }
    #endregion

    #endregion

    #region note
    //public void DeleteItem(ItemObject itemObj)
    //{
    //    if (itemObj == null)
    //        return;
    //    if (itemObj.protoData.category == quest_item)
    //    {
    //        _missionPak.RemoveItem(itemObj);
    //        ItemManager.RemoveItem(itemObj.instanceId);
    //    }
    //    else
    //    {
    //        _playerPak.RemoveItem(itemObj);
    //        ItemManager.RemoveItem(itemObj.instanceId);
    //    }
    //}

    //internal void RemoveItem(int itemID, int count, ref List<ItemObject> effItems)
    //{
    //    ItemProto itemData = ItemProto.GetItemData(itemID);
    //    if (null == itemData)
    //        return;
    //    if (itemData.category == ItemCategory.IC_QuestItem)
    //    {
    //        _missionPak.RemoveItem(itemID, count, ref effItems);
    //    }
    //    else
    //    {
    //        _playerPak.RemoveItem(itemID, count, ref effItems);
    //    }
    //}

    //internal ItemObject[] RemoveItem(IEnumerable<ItemSample> items)
    //{
    //    var finalItems = new List<ItemObject>(10);
    //    foreach (ItemSample item in items)
    //        RemoveItem(item.protoId, item.stackCount, ref finalItems);

    //    return finalItems.ToArray();
    //}

    //internal void AddSameItems(int itemID, int count, ref List<ItemObject> effItems)
    //{
    //    ItemProto itemData = ItemProto.GetItemData(itemID);
    //    if (null == itemData)
    //        return;
    //    if (itemData.category == ItemCategory.IC_QuestItem)
    //        _missionPak.AddSameItems(itemID, count, ref effItems);
    //    else
    //        _playerPak.AddSameItems(itemID, count, ref effItems);
    //}

    //internal ItemObject[] AddSameItems(IEnumerable<ItemSample> items)
    //{
    //    if (!CanAdd(items))
    //        return null;

    //    var finalItems = new List<ItemObject>(10);
    //    foreach (ItemSample iter in items)
    //    {
    //        if (iter.protoData.category == ItemCategory.IC_QuestItem)
    //            _missionPak.AddSameItems(iter, ref finalItems);
    //        else
    //            _playerPak.AddSameItems(iter, ref finalItems);
    //    }

    //    return finalItems.ToArray();
    //}

    //internal void AddSameItems(ItemSample item, ref List<ItemObject> effItems)
    //{
    //    AddSameItems(item.protoId, item.stackCount, ref effItems);
    //}

    //public ItemObject GetItemById(int objId)
    //{
    //    ItemObject item = ItemManager.GetItemByID(objId);
    //    if (item == null)
    //        return null;
    //    if (item.protoData.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.GetItemById(objId);
    //    else
    //        return _playerPak.GetItemById(objId);
    //}
    //public int RemoveItem(ItemObject itemObj)
    //{
    //    if (null == itemObj)
    //        return -1;

    //    if (itemObj.protoData.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.RemoveItem(itemObj);
    //    else
    //        return _playerPak.RemoveItem(itemObj);
    //}

    //public List<ItemObject> GetValidItemList(int type)
    //{
    //    return _playerPak.GetValidItemList(type);
    //}

    //public byte[] GetChangedIndex()
    //{
    //    return _playerPak.GetChangedIndex();
    //}
    //internal IEnumerable<int> GetItemObjIDs(int tab)
    //{
    //    return _playerPak.GetItemObjIDs(tab);
    //}
    //public void ExtendPackage(int itemMax, int equipmentMax, int resourceMax, int armorMax)
    //{
    //    _playerPak.ExtendPackage(itemMax, equipmentMax, resourceMax, armorMax);
    //}

    //public int AddItem(ItemObject item, int index)
    //{
    //    if (!CanAdd(item))
    //        return -1;

    //    if (item.protoData.category == ItemCategory.IC_QuestItem)
    //    {
    //        return _missionPak.AddItem(item, index);
    //    }
    //    else
    //    {
    //        return _playerPak.AddItem(item, index);
    //    }
    //}

    //public bool CanAdd(IEnumerable<ItemSample> items)
    //{
    //    foreach (ItemObject iter in items)
    //    {
    //        if (iter.protoData.category == ItemCategory.IC_QuestItem)
    //            if (!_missionPak.CanAdd(iter))
    //                return false;
    //            else
    //                if (!_playerPak.CanAdd(iter))
    //                    return false;
    //    }
    //    return true;
    //}

    //public bool CanAdd(ItemObject item)
    //{
    //    if (item == null)
    //        return false;
    //    if (item.protoData.category == ItemCategory.IC_QuestItem)
    //    {
    //        return _missionPak.CanAdd(item);
    //    }
    //    else
    //    {
    //        return _playerPak.CanAdd(item);
    //    }
    //}

    //public bool CanAdd(ItemSample item)
    //{
    //    return CanAdd(item.protoId, item.stackCount);
    //}

    //public bool CanAdd(int itemId, int num)
    //{
    //    ItemProto itemData = ItemProto.GetItemData(itemId);
    //    if (null == itemData)
    //        return false;

    //    if (itemData.category == ItemCategory.IC_QuestItem)
    //    {
    //        return _missionPak.CanAdd(itemId, num);
    //    }
    //    else
    //    {
    //        return _playerPak.CanAdd(itemId, num);
    //    }

    //}

    //public void AddItemList(IEnumerable<ItemObject> items)
    //{
    //    foreach (ItemObject iter in items)
    //    {
    //        if (iter.protoData.category == ItemCategory.IC_QuestItem)
    //            _missionPak.AddItem(iter, -1);
    //        else
    //            _playerPak.AddItem(iter, -1);
    //    }
    //}

    //public int GetEmptyGridCount(ItemProto protoData)
    //{
    //    if (protoData.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.GetEmptyGridCount(protoData.tabIndex);
    //    else
    //        return _playerPak.GetEmptyGridCount(protoData.tabIndex);
    //}

    //public ItemObject[] Sort(int type)
    //{
    //    return _playerPak.Sort(type);
    //}

    //public bool HasEnoughItems(IEnumerable<ItemSample> items)
    //{
    //    foreach (ItemSample item in items)
    //    {
    //        if (null == item || -1 == item.protoId)
    //            continue;


    //        if (item.protoData.category == ItemCategory.IC_QuestItem)
    //            if (!_missionPak.HasEnoughItems(items))
    //                return false;
    //            else
    //                if (!_playerPak.HasEnoughItems(items))
    //                    return false;
    //    }

    //    return true;
    //}

    //public ItemObject GetItemByIndex(int idx, int type)
    //{
    //    return _playerPak.GetItemByIndex(idx, type);
    //}

    //internal bool ExistID(ItemObject item)
    //{
    //    if (null == item)
    //        return false;

    //    if (item.protoData.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.ExistID(item);
    //    else
    //        return _playerPak.ExistID(item);
    //}

    //internal bool ExistID(int objID)
    //{
    //    ItemObject itemObj = ItemManager.GetItemByID(objID);
    //    if (null == itemObj)
    //    {
    //        return false;
    //    }

    //    if (itemObj.protoData.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.ExistID(objID);
    //    else
    //        return _playerPak.ExistID(objID);
    //}

    //public int GetEmptyIndex(ItemProto protoData)
    //{
    //    if (protoData == null)
    //        return -1;
    //    if (protoData.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.GetEmptyIndex(protoData.tabIndex);
    //    else
    //        return _playerPak.GetEmptyIndex(protoData.tabIndex);
    //}

    //public int ItemNotBindCount()
    //{
    //    return _playerPak.ItemNotBindCount();
    //}

    //public List<ItemObject> GetValidItemListNotBind(int type)
    //{
    //    return _playerPak.GetValidItemListNotBind(type);
    //}

    //public int GetItemCount(int itemId)
    //{
    //    ItemProto proto = ItemProto.GetItemData(itemId);
    //    if (proto == null)
    //        return -1;
    //    if (proto.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.GetItemCount(itemId);
    //    else
    //        return _playerPak.GetItemCount(itemId);
    //}

    //public bool HasEnoughSpace(List<MissionIDNum> missionItems)
    //{
    //    int itemcount = 0;
    //    int equipcount = 0;
    //    int resourcescount = 0;
    //    int armorcount = 0;
    //    int mItemcount = 0;
    //    int mEquipcount = 0;
    //    int mResourcescount = 0;
    //    int mArmorcount = 0;

    //    int itemempty = _playerPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Item);
    //    int equipempty = _playerPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Equipment);
    //    int resourcesempty = _playerPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Resource);
    //    int armorempty = _playerPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Armor);
    //    int mItemempty = _missionPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Item);
    //    int mEquipempty = _missionPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Equipment);
    //    int mResourcesempty = _missionPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Resource);
    //    int mArmorempty = _missionPak.GetEmptyGridCount((int)ItemPackage.ESlotType.Armor);

    //    for (int i = 0; i < missionItems.Count; i++)
    //    {
    //        ItemProto itemdata = ItemProto.GetItemData(missionItems[i].id);

    //        if (itemdata == null)
    //            continue;
    //        if (itemdata.category == ItemCategory.IC_QuestItem)
    //        {
    //            if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Item)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    mItemcount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //            else if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Equipment)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    mEquipcount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //            else if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Resource)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    mResourcescount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //            else if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Armor)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    mArmorcount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //        }
    //        else
    //        {
    //            if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Item)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    itemcount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //            else if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Equipment)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    equipcount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //            else if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Resource)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    resourcescount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //            else if (itemdata.tabIndex == (int)ItemPackage.ESlotType.Armor)
    //            {
    //                if (itemdata.maxStackNum > 0)
    //                    armorcount += ((missionItems[i].num - 1) / itemdata.maxStackNum) + 1;
    //            }
    //        }
    //    }

    //    if (itemcount > itemempty || equipcount > equipempty || resourcescount > resourcesempty || armorcount > armorempty)
    //        return false;

    //    if (mItemcount > mItemempty || mEquipcount > mEquipempty || mResourcescount > mResourcesempty || mArmorcount > mArmorempty)
    //        return false;
    //    return true;
    //}

    //public int GetItemIndex(ItemObject item)
    //{
    //    if (item == null)
    //        return -1;
    //    if (item.protoData.category == ItemCategory.IC_QuestItem)
    //        return _missionPak.GetItemIndex(item);
    //    else
    //        return _playerPak.GetItemIndex(item);
    //}

    //public void Clear(ItemPackage.ESlotType type = ItemPackage.ESlotType.Max)
    //{
    //    _playerPak.Clear(type);
    //    _missionPak.Clear(type);
    //}
    #endregion

    #region Helper
    public int GetCount(int prototypeId)
    {
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData == null)
            return 0;
        if (itemData.category == quest_item)
            return _missionPak.GetCount(prototypeId);
        else
            return _playerPak.GetCount(prototypeId);
    }

	public bool ContainsItem(int prototypeId)
	{
		ItemProto itemData = ItemProto.GetItemData(prototypeId);
		if (itemData == null)
			return false;
		if (itemData.category == quest_item)
			return _missionPak.ContainsItem(prototypeId);
		else
			return _playerPak.ContainsItem(prototypeId);
	}

    public int GetCountByEditorType(int editorType) 
    {
        ItemProto itemData = ItemProto.GetItemDataByEditorType(editorType);
        if (itemData.category == quest_item)
            return _missionPak.GetCountByEditorType(editorType);
        else
            return _playerPak.GetCountByEditorType(editorType);
    }

    public int GetAllItemsCount() 
    {
        return _missionPak.GetAllItemsCount() + _playerPak.GetAllItemsCount();
    }

    public int GetCreationCount(ECreation type) 
    {
        return _playerPak.GetCreationCount(type);
    }

    public List<int> GetCreationInstanceId(ECreation type) 
    {
        return _playerPak.GetCreationInstanceId(type);
    }

    public bool CanAdd(int prototypeId, int count)
    {
        ItemProto itemData = ItemProto.GetItemData(prototypeId);
        if (itemData.category == quest_item)
            return _missionPak.CanAdd(prototypeId, count);
        else
            return _playerPak.CanAdd(prototypeId, count);
    }

    public bool CanAdd(ItemSample itemSample)
    {
        if (itemSample.protoData.category == quest_item)
            return _missionPak.CanAdd(itemSample);
        else
            return _playerPak.CanAdd(itemSample);
    }

    public bool Split(int instanceId, int count)
    {
        ItemObject item = ItemMgr.Instance.Get(instanceId);
		if(null == item) return false;
        if (item.protoData.category == quest_item)
            return _missionPak.Split(instanceId, count);
        else
            return _playerPak.Split(instanceId, count);
    }

    public bool AddAsOneItem(int prototypeId, int count, bool newFlag = false)
    {
        ItemProto itemData = ItemProto.GetItemData(prototypeId);
        if (itemData.category == quest_item)
            return _missionPak.AddAsOneItem(prototypeId, count, newFlag);
        else
            return _playerPak.AddAsOneItem(prototypeId, count, newFlag);
    }

    public bool Add(int prototypeId, int count, bool newFlag = false)
    {
        ItemProto itemData = ItemProto.GetItemData(prototypeId);
        if (itemData.category == quest_item)
            return _missionPak.Add(prototypeId, count, newFlag);
        else
            return _playerPak.Add(prototypeId, count, newFlag);
    }

    public bool Set(int prototypeId, int count)
    {
        ItemProto itemData = ItemProto.GetItemData(prototypeId);
        if (itemData.category == quest_item)
            return _missionPak.Set(prototypeId, count);
        else
            return _playerPak.Set(prototypeId, count);
    }

    public bool Destroy(int prototypeId, int count)
    {
        ItemProto itemData = ItemProto.GetItemData(prototypeId);
        if (itemData.category == quest_item)
            return _missionPak.Destroy(prototypeId, count);
        else
            return _playerPak.Destroy(prototypeId, count);
    }

    public bool DestroyItem(int instanceId, int count)
    {
        ItemObject item = ItemMgr.Instance.Get(instanceId);
		if(null == item)
			return false;
        if (item.protoData.category == quest_item)
            return _missionPak.DestroyItem(instanceId, count);
        else
            return _playerPak.DestroyItem(instanceId, count);
    }

    public bool DestroyItem(ItemObject item, int count)
    {
        if (item.protoData.category == quest_item)
            return _missionPak.DestroyItem(item, count);
        else
            return _playerPak.DestroyItem(item, count);
    }

    public bool CanAdd(IEnumerable<ItemAsset.MaterialItem> list)
    {
        if (list.Any(delegate(ItemAsset.MaterialItem tmp)
        {
            ItemProto itemData = ItemProto.GetItemData(tmp.protoId);
            if (itemData.category == quest_item)
                return true;
            return false;
        }))
            return _missionPak.CanAdd(list);
        else
            return _playerPak.CanAdd(list);
    }

    public bool Add(IEnumerable<ItemAsset.MaterialItem> list)
    {
        if (list.Any(delegate(ItemAsset.MaterialItem tmp)
        {
            ItemProto itemData = ItemProto.GetItemData(tmp.protoId);
            if (itemData.category == quest_item)
                return true;
            return false;
        }))
            return _missionPak.Add(list);
        else
            return _playerPak.Add(list);
    }
    #endregion

    #region NetHelper
    public void ResetPackageItems(int tab, int index, int id, bool bMission)
    {
        if (bMission)
            _missionPak.ResetPackageItems(tab, index, id);
        else
            _playerPak.ResetPackageItems(tab, index, id);
    }

    public void ResetPackageItems(int tab, int[] ids)
    {
        if (ids.Any(delegate(int tmp)
        {
            ItemProto protoData = ItemProto.GetItemData(tmp);
            if (protoData.category == quest_item)
                return true;
            return false;
        }))
            _missionPak.ResetPackageItems(tab, ids);
        else
            _playerPak.ResetPackageItems(tab, ids);

    }
    #endregion
}

