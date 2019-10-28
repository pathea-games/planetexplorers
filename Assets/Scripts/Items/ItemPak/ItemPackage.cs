using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using ItemAsset.SlotListHelper;

namespace ItemAsset
{
    public class NewFlagMgr
    {
		//Remove after 5 min
        static readonly float NewFlagTime = 60f * 5;

        class NewItemIndex
        {
            public float time;
            public int index;
        }

        List<NewItemIndex> mNewItemIndexList = new List<NewItemIndex>(10);
        public bool IsNew(int index)
        {
            return null != mNewItemIndexList.Find((item) =>
            {
                if (item.index == index)
                {
                    return true;
                }
                return false;
            }
            );
        }

        public void Add(int pkgIndex)
        {
            mNewItemIndexList.Add(new NewItemIndex() { index = pkgIndex, time = NewFlagTime });
        }

        public void Remove(int index)
        {
            mNewItemIndexList.RemoveAll((item) =>
            {
                if (item.index == index)
                {
                    return true;
                }
                return false;
            });
        }

		public void RemoveAll()
		{
			mNewItemIndexList.Clear ();
		}

        public void Update(float deltaTime)
        {
            NewItemIndex removeItem = null;
			for (int i = mNewItemIndexList.Count - 1; i >= 0; i--) 
			{
				removeItem = mNewItemIndexList[i];
				removeItem.time -= deltaTime;
				if (removeItem.time < PETools.PEMath.Epsilon)
				{
					//remove only one item per frame to avoid new list.
					mNewItemIndexList.RemoveAt(i);
					return;
				}
			}
        }
    }

    namespace PackageHelper
    {
        public static class PackageAccessor
        {
			public static bool ContainsItem(this ItemPackage package, int prototypeId)
			{
				for (int i = 0; i < (int)ItemAsset.ItemPackage.ESlotType.Max; i++)
				{
					SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)i);
					if(accessor.ConatinsItem(prototypeId))
						return true;
				}				
				return false;
			}

            public static int GetCount(this ItemPackage package, int prototypeId)
            {
				// Opt
                ItemProto proto = ItemProto.Mgr.Instance.Get (prototypeId);
				if (proto != null) {
					SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)proto.tabIndex);
					return accessor != null ? accessor.GetCount(prototypeId) : 0;
				}
                return 0;
            }

            public static int GetCountByEditorType(this ItemPackage package, int editorType) 
            {
                int count = 0;
                for (int i = 0; i < (int)ItemAsset.ItemPackage.ESlotType.Max; i++)
                {
                    SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)i);
                    count += accessor.GetCountByEditorType(editorType);
                }

                return count;
            }

            public static int GetAllItemsCount(this ItemPackage package) 
            {
                int count = 0;
                for (int i = 0; i < (int)ItemAsset.ItemPackage.ESlotType.Max; i++)
                {
                    SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)i);
                    count += accessor.GetAllItemsCount();
                }
                return count;
            }

			/// <summary>
			/// Gets the first item by proto ID.
			/// </summary>
			/// <returns>The item by proto I.</returns>
			public static ItemObject GetItemByProtoID(this ItemPackage package, int protoID)
			{
				for (int i = 0; i < (int)ItemAsset.ItemPackage.ESlotType.Max; i++)
				{
					SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)i);
					ItemObject itemObj = accessor.GetItemByProtoID(protoID);
					if(null != itemObj)
						return itemObj;
				}
				return null;
			}

			public static List<ItemObject> GetAllItemByProtoId(this ItemPackage package, int protoID){
				List<ItemObject> allItems = new List<ItemObject> ();
				for (int i = 0; i < (int)ItemAsset.ItemPackage.ESlotType.Max; i++)
				{
					SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)i);
					allItems.AddRange( accessor.GetAllItemByProtoID(protoID));
				}
				return allItems;
			}

            public static int GetCreationCount(this ItemPackage package,ECreation type) 
            {
                int count = 0;
                for (int i = 0; i < (int)ItemAsset.ItemPackage.ESlotType.Max; i++)
                {
                    SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)i);
                    count += accessor.GetCreationCount(type);
                }
                return count;
            }

            public static List<int> GetCreationInstanceId(this ItemPackage package, ECreation type) 
            {
                List<int> tmp = new List<int>();
                for (int i = 0; i < (int)ItemAsset.ItemPackage.ESlotType.Max; i++)
                {
                    SlotList accessor = package.GetSlotList((ItemAsset.ItemPackage.ESlotType)i);
                    foreach (var item in accessor.GetCreationInstanceId(type))
                    {
                        tmp.Add(item);
                    }
                }
                return tmp;
            }

            public static bool CanAdd(this ItemPackage package, int prototypeId, int count)
            {
                SlotList accessor = package.GetSlotList(ItemPackage.GetSlotType(prototypeId));

                if (null == accessor)
                {
                    return false;
                }

                return accessor.CanAdd(prototypeId, count);
            }

            public static bool CanAdd(this ItemPackage package, ItemSample itemSample)
            {
                return CanAdd(package, itemSample.protoId, itemSample.stackCount);
            }

            public static bool Split(this ItemPackage package, int instanceId, int count)
            {
                ItemObject item =  ItemMgr.Instance.Get(instanceId);

                if (null == item)
                {
                    return false;
                }

                if (!package.HasItemObj(item))
                {
                    return false;
                }

                if (item.stackCount <= count)
                {
                    return false;
                }

                item.stackCount -= count;

                return AddAsOneItem(package, item.protoId, count, false);
            }

            public static bool AddAsOneItem(this ItemPackage package, int prototypeId, int count, bool newFlag = false)
            {
                if (count <= 0)
                {
                    return false;
                }

                SlotList accessor = package.GetSlotList(ItemPackage.GetSlotType(prototypeId));

                if (null == accessor)
                {
                    return false;
                }

                return accessor.AddAsNew(prototypeId, count, newFlag);
            }

            public static bool Add(this ItemPackage package, int prototypeId, int count, bool newFlag = false)
            {
                if (count <= 0)
                {
                    return false;
                }

                SlotList accessor = package.GetSlotList(ItemPackage.GetSlotType(prototypeId));

                if (null == accessor)
                {
                    return false;
                }
                if (ItemProto.Mgr.Instance.Get(prototypeId).maxStackNum > 1)
                    return accessor.Add(prototypeId, count, newFlag);
                else 
                {
                    bool result = true;
                    for (int i = 0; i < count; i++)
                    {
                        if(result)
							result = accessor.Add(ItemMgr.Instance.CreateItem(prototypeId), newFlag);
                        else
							accessor.Add(ItemMgr.Instance.CreateItem(prototypeId), newFlag);
                    }
                    return result;
                }
            }

            public static bool Set(this ItemPackage package, int prototypeId, int count)
            {
                if (count <= 0)
                {
                    return false;
                }

                SlotList accessor = package.GetSlotList(ItemPackage.GetSlotType(prototypeId));

                if (null == accessor)
                {
                    return false;
                }

                return accessor.Set(prototypeId, count);
            }

            public static bool Destroy(this ItemPackage package, int prototypeId, int count)
            {
                if (count <= 0)
                {
                    return false;
                }

                if (GetCount(package, prototypeId) < count)
                {
                    return false;
                }

                SlotList accessor = package.GetSlotList(ItemPackage.GetSlotType(prototypeId));

                if (null == accessor)
                {
                    return false;
                }

                return accessor.Destroy(prototypeId, count);


            }

            public static bool DestroyItem(this ItemPackage package, int instanceId, int count)
            {
                if (count <= 0)
                {
                    return false;
                }

                ItemObject item = ItemMgr.Instance.Get(instanceId);

                if (item.stackCount < count)
                {
                    return false;
                }

                SlotList accessor = package.GetSlotList(ItemPackage.GetSlotType(item.protoId));

                return accessor.DestroyItem(instanceId, count);
            }

            public static bool DestroyItem(this ItemPackage package, ItemObject item, int count)
            {
                if (count <= 0)
                {
                    return false;
                }

                if (item.GetCount() < count)
                {
                    return false;
                }

                SlotList accessor = package.GetSlotList(ItemPackage.GetSlotType(item.protoId));

                return accessor.DestroyItem(item.instanceId, count);
            }

            public static bool CanAdd(this ItemPackage package, IEnumerable<ItemAsset.MaterialItem> list)
            {
                int[] sum = new int[(int)ItemPackage.ESlotType.Max];

                foreach (ItemAsset.MaterialItem mi in list)
                {
                    int type = (int)ItemPackage.GetSlotType(mi.protoId);
                    if(type < sum.Length)
                    {
                        sum[type] += SlotListHelper.SlotListAccessor.GetNeedSlotCount(mi.protoId, mi.count);
                    }
                }

                for (int i = 0; i < sum.Length; i++)
                {
                    SlotList slotList = package.GetSlotList((ItemPackage.ESlotType) i);

                    if (slotList.GetVacancyCount() < sum[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public static bool Add(this ItemPackage package, IEnumerable<ItemAsset.MaterialItem> list)
            {
                foreach (ItemAsset.MaterialItem mi in list)
                {
                    if (false == Add(package, mi.protoId, mi.count))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

	public class ItemPackage
	{
        public enum ESlotType
        {
			None = -1,
            Item = 0,
            Equipment = 1,
            Resource = 2,
			Armor = 3,
            Max = 4
        }

        public class EventArg:PeEvent.EventArg
        {
            public enum Op
            {
                Put,
                Reset,
                Clear,
                Update,
                Max
            }

            public Op op;
            public ItemObject itemObj;
        }

        PeEvent.Event<EventArg> mEventor = new PeEvent.Event<EventArg>();

        public PeEvent.Event<EventArg> changeEventor
        {
            get
            {
                return mEventor;
            }
        }

        void Resend(ESlotType slotType, SlotList.ChangeEvent arg)
        {
            switch(arg.op)
            {
                case SlotList.ChangeEvent.Op.Set:
                    changeEventor.Dispatch(new EventArg() { op = EventArg.Op.Put, itemObj = GetItem(slotType, arg.index) });
                    break;
                case SlotList.ChangeEvent.Op.Reset:
                    changeEventor.Dispatch(new EventArg() { op = EventArg.Op.Reset, itemObj = GetItem(slotType, arg.index) });
                    break;
                case SlotList.ChangeEvent.Op.Update:
                    changeEventor.Dispatch(new EventArg() { op = EventArg.Op.Update, itemObj = null });
                    break;
                case SlotList.ChangeEvent.Op.Clear:
                    break;
                case SlotList.ChangeEvent.Op.Sort:
                    break;
                default:
                    break;
            }
        }

        void ItemSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
        {
            Resend(ESlotType.Item, arg);
        }

        void EquipmentSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
        {
            Resend(ESlotType.Equipment, arg);
        }

        void ResourceSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
        {
            Resend(ESlotType.Resource, arg);
        }

		void ArmorSlotListMsgHandler(object sender, SlotList.ChangeEvent arg)
		{
			Resend(ESlotType.Armor, arg);
		}

		void RegisterSlotEvent(ESlotType slotType)
        {
            SlotList list = GetSlotList(slotType);
            switch (slotType)
            {
                case ESlotType.Item:
                    if (null != list)
                    {
                        list.eventor.Subscribe(ItemSlotListMsgHandler);
                    }
                    break;
                case ESlotType.Equipment:
                    if (null != list)
                    {
                        list.eventor.Subscribe(EquipmentSlotListMsgHandler);
                    }
                    break;
                case ESlotType.Resource:
                    if (null != list)
                    {
                        list.eventor.Subscribe(ResourceSlotListMsgHandler);
                    }
                    break;
				case ESlotType.Armor:
					if (null != list)
					{
						list.eventor.Subscribe(ArmorSlotListMsgHandler);
					}
					break;
				default:
                    break;
            }
        }

        SlotList[] mSlotListArray = new SlotList[(int)ESlotType.Max];

        public ItemPackage()
        {
        }

        public ItemPackage(int capacity)
        {
            ExtendPackage(capacity, capacity, capacity,capacity);
        }

        public void ExtendPackage(int itemMax, int equipmentMax, int recourceMax,int armorMax)
        {
            SetSlotList(ESlotType.Item, SlotList.ResetCapacity(GetSlotList(ESlotType.Item), itemMax));
            SetSlotList(ESlotType.Equipment, SlotList.ResetCapacity(GetSlotList(ESlotType.Equipment), equipmentMax));
            SetSlotList(ESlotType.Resource, SlotList.ResetCapacity(GetSlotList(ESlotType.Resource), recourceMax));
            SetSlotList(ESlotType.Armor, SlotList.ResetCapacity(GetSlotList(ESlotType.Armor), armorMax));
		}

        public void UpdateNewFlag(float deltaTime)
        {
            for (int i = 0; i < mSlotListArray.Length; i++) {
				SlotList list = mSlotListArray [i];
				list.UpdateNewFlag (deltaTime);
			}
        }

		public void Clear (ESlotType type = ESlotType.Max)
		{
            if (type == ESlotType.Max)
            {
                GetSlotList(ESlotType.Item).Clear();
                GetSlotList(ESlotType.Equipment).Clear();
                GetSlotList(ESlotType.Resource).Clear();
                GetSlotList(ESlotType.Armor).Clear();
			}
            else
            {
                GetSlotList(type).Clear();
            }

            changeEventor.Dispatch(new EventArg() { op = EventArg.Op.Clear, itemObj = null });
		}
		
        bool SetSlotList(ESlotType itemClass, SlotList list)
        {
            if (itemClass == ESlotType.Max)
            {
                return false;
            }

            mSlotListArray[(int)itemClass] = list;

            RegisterSlotEvent(itemClass);

            return true;
        }

        public SlotList GetSlotList(int prototypeId)
        {
            return GetSlotList(GetSlotType(prototypeId));
        }

        public SlotList GetSlotList(ESlotType itemClass = ESlotType.Item)
        {
            if (itemClass == ESlotType.Max)
            {
                return null;
            }

            return mSlotListArray[(int)itemClass];
        }

        public bool CanAddItemList(IEnumerable<ItemObject> items)
        {
            int[] itemCount = new int[(int)ESlotType.Max];

            foreach (ItemObject obj in items)
            {
                itemCount[obj.protoData.tabIndex]++;
            }

            for (int i = 0; i < (int)ESlotType.Max; i++)
            {
                SlotList list = GetSlotList((ESlotType)i);
                if (list.GetVacancyCount() < itemCount[i])
                {
                    return false;
                }
            }

            return true;
        }

		public bool AddItemList (IEnumerable<ItemObject> items)
		{
			foreach(ItemObject obj in items)
			{
				AddItem(obj);
			}
            return true;
		}

        public static ESlotType GetSlotType(int prototypeId)
        {
            ItemProto itemData = ItemProto.GetItemData(prototypeId);
            if (null == itemData)
            {
                return ItemAsset.ItemPackage.ESlotType.Max;
            }

            return (ItemAsset.ItemPackage.ESlotType)itemData.tabIndex;
        }

        public ItemObject FindItemByProtoId(int protoId)
        {
            SlotList slotList = GetSlotList(protoId);
            if(null == slotList)
            {
                return null;
            }

            return slotList.FindItemByProtoId(protoId);
        }

        public int FindItemIndexByProtoId(int protoId)
        {
            ESlotType eSlotType = GetSlotType(protoId);
            SlotList slotList = GetSlotList(eSlotType);
            if (null == slotList)
            {
                return -1;
            }

            int index = slotList.FindItemIndexByProtoId(protoId);

            return CodeIndex(eSlotType, index);
        }

		void SetItem(SlotList slotList, ItemObject item, int index, bool isNew = false)
        {
            slotList[index] = item;

			if(isNew)
				slotList.newFlagMgr.Add (index);
			else
				slotList.newFlagMgr.Remove (index);
            EventArg e = new EventArg() { op = (item == null? EventArg.Op.Reset : EventArg.Op.Put), itemObj = item };
            
            changeEventor.Dispatch(e);
        }

		public int AddItem(ItemObject itemObject, bool isNew = false)
        {
            ESlotType eSlotType = GetSlotType(itemObject.protoId);
            SlotList slotList = GetSlotList(eSlotType);
            int vacancyIndex = slotList.VacancyIndex();
            if (-1 == vacancyIndex)
            {
                return InvalidIndex;
            }

			SetItem(slotList, itemObject, vacancyIndex, isNew);

            return CodeIndex(eSlotType, vacancyIndex);
        }

        public int PutItem(ItemObject item, int slotIndex, ESlotType slotType)
        {
            if (slotType == ESlotType.Max)
            {
                slotType = (ESlotType)item.protoData.tabIndex;
            }

            SlotList slotList = GetSlotList(slotType);

            SetItem(slotList, item, slotIndex);            

            return CodeIndex(slotType, slotIndex);
        }
        
        /// <summary>
        /// put an item
        /// </summary>
        /// <param name="item">item to put</param>
        /// <param name="codedIndex">coded index, create by CodeIndex()</param>
        public void PutItem(ItemObject item, int codedIndex)
        {
            int slotIndex;
            ESlotType slotType;

            if (!DecodeIndex(codedIndex, out slotType, out slotIndex))
            {
                return;
            }

            PutItem(item, slotIndex, slotType);
        }

        /// <summary>
        /// get an item by coded index
        /// </summary>
        /// <param name="codeIndex">coded index, create by CodeIndex()</param>
        public ItemObject GetItem(int codeIndex)
        {
            int slotIndex;
            ESlotType slotType;
            if (!DecodeIndex(codeIndex, out slotType, out slotIndex))
            {
                return null;
            }

            return GetItem(slotType, slotIndex);
        }

        public ItemObject GetItem(ESlotType slotType, int index)
        {
            SlotList slotList = GetSlotList(slotType);
            if (null == slotList)
            {
                return null;
            }

            return slotList[index];
        }

        /// <summary>
        /// remove an item by coded index
        /// </summary>
        /// <param name="codedIndex">coded index, create by CodeIndex()</param>
        public bool RemoveItem(int codedIndex)
        {
            int slotIndex;
            ESlotType slotType;

            if (!DecodeIndex(codedIndex, out slotType, out slotIndex))
            {
                return false;
            }

            SlotList slotList = GetSlotList(slotType);
            if (slotList == null)
            {
                return false;
            }

            SetItem(slotList, null, slotIndex);

            return true;
        }

        public static int InvalidIndex = ~0;
        public static int CodeIndex(ESlotType type, int slotIndex)
        {
            if (type == ESlotType.Max || slotIndex < 0)
            {
                return InvalidIndex;
            }

            return slotIndex | (((int)type) << 24);
        }

        public static bool DecodeIndex(int index, out ESlotType type, out int slotIndex)
        {
            type = (ESlotType)(index >> 24);
            slotIndex = index & 0x00ffffff;
            
            if (index == InvalidIndex)
            {
                return false;
            }

            return true;
        }
        
        public int GetItemIndexById(int instanceId)
        {
            for(int i = (int)ESlotType.Item; i < (int)ESlotType.Max; i++)
            {
                SlotList list = mSlotListArray[i];

                if (null != list)
                {
                    int slotIndex = list.FindItemIndexById(instanceId);
                    if (slotIndex != -1)
                    {
                        return CodeIndex((ESlotType)i, slotIndex);
                    }
                }
            }

            return InvalidIndex;
        }

        public bool RemoveItemById(int instanceId)
        {
            int codedIndex = GetItemIndexById(instanceId);

            return RemoveItem(codedIndex);
        }

        public bool RemoveItem(ItemObject item)
        {
            return RemoveItemById(item.instanceId);
        }

        public bool HasItemObj(ItemObject itemObject)
        {
            if (null == itemObject)
            {
                return false;
            }

            SlotList list = GetSlotList(itemObject.protoId);
            if (null == list)
            {
                return false;
            }

            return list.HasItem(itemObject.instanceId);
        }

        public int GetVacancySlotIndex(int slotType = 0)
		{
            return GetVacancySlotIndex((ESlotType)slotType);
        }

        public int GetVacancySlotIndex(ESlotType slotType)
        {
            SlotList list = GetSlotList(slotType);
            if (null == list)
            {
                return -1;
            }

            return list.VacancyIndex();
		}

        #region Serialize
		public void Export(BinaryWriter w)
        {
            for(int i = (int)ESlotType.Item; i < (int)ESlotType.Max; i++ )
            {
                byte[] buff = null;
                SlotList list = GetSlotList((ESlotType)i);
                if(null != list)
                {
                    buff = list.Export();
                }

                PETools.Serialize.WriteBytes(buff, w);
            }
        }

        public void Import(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer, false))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    for (int i = (int)ESlotType.Item; i < (int)ESlotType.Max; i++)
                    {
                        byte[] buff = PETools.Serialize.ReadBytes(r);
                        if (null != buff && buff.Length > 0)
                        {
                            SlotList list = new SlotList();
                            list.Import(buff);
                            SetSlotList((ESlotType)i, list);
                        }
                    }
                }
            }
        }
        #endregion

        public void Sort(ESlotType type)
        {
            SlotList slotList = GetSlotList(type);
            if (null == slotList)
            {
                return;
            }
			
			slotList.Reduce();
			slotList.Sort();
        }
	}
}