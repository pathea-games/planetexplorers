using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
using System.Collections.Generic;
using Pathea;
public partial class ColonyNetwork 
{

    void RPC_S2C_InitDataStorage(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;
		recordData.m_CurDeleteTime = stream.Read <float>();
		recordData.m_CurRepairTime = stream.Read<float> ();
		recordData.m_DeleteTime = stream.Read<float> ();
		recordData.m_Durability = stream.Read<float> ();
		recordData.m_RepairTime = stream.Read<float> ();
		recordData.m_RepairValue = stream.Read<float> ();
		int[] keys = stream.Read<int[]> ();
		int[] values = stream.Read<int[]> ();
		for(int i = 0; i < keys.Length;i++)
		{
			recordData.m_Items[keys[i]] = values[i];
		}

	}


    void RPC_S2C_STO_Delete(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read <int>();
		bool suc = stream.Read <bool>();
        ItemObject itemObj = ItemMgr.Instance.Get(objId);
        if (itemObj == null)
        {
            return;
        }
        int index = -1;
        if (suc)
        {
            //1.data
            CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;

            foreach (KeyValuePair<int, int> kvp in recordData.m_Items)
            {
                if (kvp.Value == objId)
                {
                    recordData.m_Items.Remove(kvp.Key);

                    ItemPackage m_Package = ((CSStorage)m_Entity).m_Package;
                    int listCnt = m_Package.GetSlotList().Length;
                    index = kvp.Key % listCnt;
                    m_Package.GetSlotList(itemObj.protoId)[index] = null;
                    break;
                }
            }
		}
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        //2.ui
        if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
        {
            CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultDelete(suc, index, objId, (CSStorage)m_Entity);
        }
        if (suc)
        {
            ItemMgr.Instance.DestroyItem(itemObj.instanceId);
        }
	}



    void RPC_S2C_STO_Store(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read <int>();
		int objId = stream.Read <int>();
		bool suc = stream.Read <bool>();
        ItemObject itemObj = ItemMgr.Instance.Get(objId);
        if (itemObj == null)
        {
            return;
        }
        int tabIndex = itemObj.protoData.tabIndex;
        int key = IndexToKey(index, tabIndex);
        if (suc)
        {
            //1.data
            CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;
            recordData.m_Items[key] = objId;

            ItemPackage m_Package = ((CSStorage)m_Entity).m_Package;
            int listCnt = m_Package.GetSlotList().Length;
            int key2 = key % listCnt;
            m_Package.GetSlotList(itemObj.protoId)[key2] = itemObj;
        }
        if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
        {
            CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultStore(suc, index, objId, (CSStorage)m_Entity);
        }
	}


    void RPC_S2C_STO_FetchItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int>();
		bool suc = stream.Read<bool>();
        ItemObject itemObj = ItemMgr.Instance.Get(objId);
        if (itemObj == null)
        {
            return;
        }
        if (suc)
        {
            //1.data
            CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;
            
            foreach (var kvp in recordData.m_Items)
            {
                if(kvp.Value == objId)
                {
                    recordData.m_Items.Remove(kvp.Key);


                    ItemPackage m_Package = ((CSStorage)m_Entity).m_Package;
                    //int listCnt = m_Package.GetItemList(0).Count;
                    //index = kvp.Key % listCnt;
                    //m_Package.GetItemList(itemObj.mItemData.mTabIndex)[index] = null;

                    m_Package.RemoveItem(itemObj);

                    break;
                }
            }

        }
            //2.ui
        if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
        {
            CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultFetch(suc, objId, (CSStorage)m_Entity);
        }
        
	}

    void RPC_S2C_STO_Exchange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read <int>();
		int destIndex = stream.Read<int> ();
		int destId = stream.Read<int> ();
		int originIndex = stream.Read<int> ();
		bool suc =  stream.Read<bool> ();
        
        ItemObject originObj = ItemMgr.Instance.Get(objId);
        if (originObj == null)
        {
            return;
        }

        ItemObject destObj;
        if (destId == -1)
        {
            destObj = null;
        }
        else
        {
            destObj = ItemMgr.Instance.Get(destId);
            if (destObj == null)
            {
                return;
            }
        }
        int type= originObj.protoData.tabIndex;
        int destKey = IndexToKey(destIndex, type);
        int originKey = IndexToKey(originIndex, type);
        if(suc){
            //1.data
            CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;
            recordData.m_Items[destKey] = objId;
           
            ItemPackage m_Package = ((CSStorage)m_Entity).m_Package;
			SlotList slotList = m_Package.GetSlotList((ItemAsset.ItemPackage.ESlotType)type);
            slotList.Swap(originIndex, destIndex);

            if (destObj != null)
            {
                recordData.m_Items[originKey] = destId;                
            }
            else
            {
                recordData.m_Items.Remove(originKey);
            }
        }
        //2.ui
        if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
        {
            CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultExchange(suc, objId, destIndex, destId, originIndex, (CSStorage)m_Entity);
        }
	}


    void RPC_S2C_STO_Split(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read <int>();
		int destIndex = stream.Read<int> ();
		bool suc = stream.Read <bool>();
        ItemObject itemObj = ItemMgr.Instance.Get(objId);
        if (itemObj == null)
        {
            return;
        }
        if (suc)
        {
            //1.data
            CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;
            int key = IndexToKey(destIndex,itemObj.protoData.tabIndex);
            recordData.m_Items[key] = objId;

            ItemPackage m_Package = ((CSStorage)m_Entity).m_Package;
            SlotList slotList = m_Package.GetSlotList(itemObj.protoId);
            slotList[destIndex] = itemObj;
		}
        //2.ui
        if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
        {
            CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreResultSplit(suc, objId, destIndex, (CSStorage)m_Entity);
        }
	}

    void RPC_S2C_STO_Sort(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool suc = stream.Read <bool>();
		if(suc)
		{
			int tabIndex = stream.Read <int>();
			int [] ids = stream.Read<int[]> ();
            //1.data
            CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;
            int keyStart = IndexToKey(0,tabIndex);

            ItemPackage m_Package = ((CSStorage)m_Entity).m_Package;
			SlotList slotList = m_Package.GetSlotList((ItemAsset.ItemPackage.ESlotType)tabIndex);

            int listCnt = slotList.Length;
            
            for (int i = 0; i < ids.Length; i++)
            {
                int key = keyStart + i;

                int index = key % listCnt;
                if (ids[i] == -1)
                {
                    recordData.m_Items.Remove(key);
                    slotList[index] = null;
                }
                else
                {
                    recordData.m_Items[key] = ids[i];
                    ItemObject ioject = ItemMgr.Instance.Get(ids[i]);
                    if (ioject != null)
                    {
                        slotList[index] = ioject; 
                    }
                }
            }

            //2.ui
            if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
            {
                CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStoreSortSuccess(tabIndex, ids, (CSStorage)m_Entity);
            }
		}
	}

	void RPC_S2C_STO_SyncItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int[] indexList = stream.Read<int[]>();
		int[] ids = stream.Read<int[]>();
		CSStorageData recordData = (CSStorageData)_ColonyObj._RecordData;
		ItemPackage m_Package = ((CSStorage)m_Entity).m_Package;

		for(int i=0;i<indexList.Length;i++){
			int key = indexList[i];
			int type;
			int index = KeyToIndex(key,out type);
			SlotList slotList = m_Package.GetSlotList((ItemAsset.ItemPackage.ESlotType)type);
			if (ids[i] == -1)
			{
				recordData.m_Items.Remove(key);
				slotList[index] = null;
			}else{
				ItemObject ioject = ItemMgr.Instance.Get(ids[i]);
				if (ioject != null)
				{
					recordData.m_Items[key] = ids[i];
					slotList[index] = ioject; 
				}
			}
		}
		//2.ui
		if (CSUI_MainWndCtrl.Instance.StorageUI != null && CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI != null)
		{
			CSUI_MainWndCtrl.Instance.StorageUI.StorageMainUI.CSStorageResultSyncItemList((CSStorage)m_Entity);
		}
	}


	public void Delete(int objId)
	{
        RPCServer(EPacketType.PT_CL_STO_Delete,objId);
	}
	public void STO_Store(int index,ItemObject item)
	{
		if(item == null)
            return;
        if (!(PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.HasItemObj(item)
             || PeCreature.Instance.mainPlayer.GetCmpt<EquipmentCmpt>()._ItemList.Contains(item))// equiped item
            )
			return;
        RPCServer(EPacketType.PT_CL_STO_Store,index,item.instanceId);
	}
	public void STO_Fetch(int objId,int index)
	{
        RPCServer(EPacketType.PT_CL_STO_Fetch,index,objId);
	}
	public void STO_Exchange(int objId,int originIndex,int destIndex)
	{
        RPCServer(EPacketType.PT_CL_STO_Exchange,objId,originIndex,destIndex);
	}
	public void STO_Split(int objId,int num)
	{
        RPCServer(EPacketType.PT_CL_STO_Split,objId,num);
	}
	public void STO_Sort(int tabIndex)
	{
        RPCServer(EPacketType.PT_CL_STO_Sort,tabIndex);
	}

    public int IndexToKey(int index,int type)
    {
        int ListCount = ((CSStorage)m_Entity).m_Package.GetSlotList().Length;
        int x=0;
        switch (type)
        {
            case ItemTabIndex.EQUIPMENT:
                x = 1;
                break;
            case ItemTabIndex.RESOURCE:
                x = 2;
                break;
            default:
                break;
        }
        return index + x * ListCount;
    }
	public int KeyToIndex(int key,out int type){
		if(key<CSInfoMgr.m_StorageInfo.m_MaxItem){
			type = 0;
			return key;
		}
		key = key-CSInfoMgr.m_StorageInfo.m_MaxItem;
		if(key<CSInfoMgr.m_StorageInfo.m_MaxEquip){
			type = 1;
			return key;
		}
		key = key-CSInfoMgr.m_StorageInfo.m_MaxEquip;
		if(key<CSInfoMgr.m_StorageInfo.m_MaxRecource){
			type = 2;
			return key;
		}
		key = key-CSInfoMgr.m_StorageInfo.m_MaxRecource;
		if(key<CSInfoMgr.m_StorageInfo.m_MaxArmor){
			type = 3;
			return key;
		}
		//error, index overflow
		type=-1;
		return -1;
	}
}

