using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ItemAsset
{
    namespace SlotListHelper
    {
        public static class SlotListAccessor
        {
			public static bool ConatinsItem(this SlotList slotList, int prototypeId)
			{
				foreach (ItemAsset.ItemObject item in slotList)
				{
					if (null != item && item.protoId == prototypeId)
					{
						return true;
					}
				}
				return false;
			}

            public static int GetCount(this SlotList slotList, int prototypeId)
            {
                int count = 0;
                if (slotList == null) return count; //lz-2017.07.27 空对象

                foreach (ItemAsset.ItemObject item in slotList)
                {
                    if (null == item)
                    {
                        continue;
                    }

                    if (item.protoId == prototypeId)
                    {
                        count += item.stackCount;
                    }
                }

                return count;
            }

            public static int GetCountByEditorType(this SlotList slotList, int editorType)
            {
                int count = 0;

                foreach (ItemAsset.ItemObject item in slotList)
                {
                    if (null == item)
                    {
                        continue;
                    }
                    ItemProto ip = ItemProto.Mgr.Instance.Get(item.protoId);
                    if (ip == null)
                        continue;
                    if (ip.editorTypeId == editorType)
                    {
                        count += item.stackCount;
                    }
                }

                return count;
            }

            public static int GetAllItemsCount(this SlotList slotList) 
            {
                int count = 0;

                foreach (ItemAsset.ItemObject item in slotList)
                {
                    if (null == item)
                    {
                        continue;
                    }
                    count += item.stackCount;
                }

                return count;
            }

			public static ItemObject GetItemByProtoID(this SlotList slotList, int protoId)
			{
				for(int i = 0; i < slotList.Count; i++)
				{
					if(null != slotList[i] && slotList[i].protoId == protoId)
						return slotList[i];
				}
				return null;
			}

			public static List<ItemObject> GetAllItemByProtoID(this SlotList slotList, int protoId){
				List<ItemObject> allItems = new List<ItemObject> ();
				for(int i = 0; i < slotList.Count; i++)
				{
					if(null != slotList[i] && slotList[i].protoId == protoId)
						allItems.Add(slotList[i]);
				}
				return allItems;
			}

            public static int GetCreationCount(this SlotList slotList, ECreation type) 
            {
                int count = 0;

                foreach (var item in slotList)
                {
                    if (item == null)
                        continue;
                    if (WhiteCat.CreationHelper.GetCreationItemClass(item) == WhiteCat.CreationHelper.ECreationToItemClass(type))
                        count += item.stackCount;
                }

                return count;
            }

            public static List<int> GetCreationInstanceId(this SlotList slotList,ECreation type) 
            {
                List<int> tmp = new List<int>();
                foreach (var item in slotList)
                {
                    if (item == null)
                        continue;
                    if (WhiteCat.CreationHelper.GetCreationItemClass(item) != WhiteCat.CreationHelper.ECreationToItemClass(type))
                        continue;
                    tmp.Add(item.instanceId);
                }
                return tmp;
            }

            public static int GetNeedSlotCount(int prototypeId, int count)
            {
                ItemProto item = ItemProto.Mgr.Instance.Get(prototypeId);
                if (null == item)
                {
                    return 0;
                }
                return (count - 1) / item.maxStackNum + 1;
            }

            public static bool CanAdd(this SlotList slotList, int prototypeId, int count)
            {
                return slotList.GetVacancyCount() >= GetNeedSlotCount(prototypeId, count);
            }

            public static bool CanAdd(this SlotList slotList, ItemSample itemSample)
            {
                return CanAdd(slotList, itemSample.protoId, itemSample.stackCount);
            }

            public static bool Add(this SlotList slotList, int prototypeId, int count, bool isNew = false)
            {
                if (count <= 0)
                {
                    return false;
                }

                int existCount = GetCount(slotList, prototypeId);
                if (existCount > 0)
                {
                    Destroy(slotList, prototypeId, existCount);
                }
                count += existCount;

                return AddAsNew(slotList, prototypeId, count, isNew, existCount);
            }

            public static bool Set(this SlotList slotList, int prototypeId, int count)
            {
                int exist = GetCount(slotList, prototypeId);
                if (count == exist)
                {
                    return true;
                }
                else if (count > exist)
                {
                    return Add(slotList, prototypeId, count - exist);
                }
                else
                {
                    return Destroy(slotList, prototypeId, exist - count);
                }
            }

            public static bool AddAsNew(this SlotList slotList, int prototypeId, int count, bool isNew = false, int existCount = 0)
            {
                if (count <= 0)
                {
                    return false;
                }

                ItemProto prototypeData = ItemProto.Mgr.Instance.Get(prototypeId);
                if (prototypeData == null)
                {
                    Debug.LogError("cant find item proto by id:" + prototypeId);
                    return false;
                }

                int itemCount = count / prototypeData.maxStackNum;
                for (int i = 0; i < itemCount; i++)
                {
                    ItemObject item = ItemMgr.Instance.CreateItem(prototypeId);
                    if (null != item)
                    {
                        item.SetStackCount(prototypeData.maxStackNum);
                        slotList.Add(item, isNew);
                    }
                }

                int remainderCount = count % prototypeData.maxStackNum;
                if (remainderCount > 0)
                {
                    ItemObject remainderItem = ItemMgr.Instance.CreateItem(prototypeId);
                    remainderItem.SetStackCount(remainderCount);
                    slotList.Add(remainderItem, isNew);
                }
                return true;
            }

            public static bool Destroy(this SlotList slotList, int prototypeId, int count)
            {
                if (GetCount(slotList, prototypeId) < count)
                {
                    return false;
                }

				for (int j = slotList.Length - 1; j >= 0; --j)
                {
                    if (null == slotList[j])
                    {
                        continue;
                    }

                    if (slotList[j].protoId == prototypeId)
                    {
						slotList.newFlagMgr.Remove(j);
                        if (count >= slotList[j].stackCount)
                        {
                            count -= slotList[j].stackCount;

                            ItemMgr.Instance.DestroyItem(slotList[j].instanceId);
                            slotList[j] = null;
                        }
                        else
                        {
                            slotList[j].stackCount -= count;
                            count = 0;
                            break;
                        }
                    }
                }

                slotList.SendUpdateEvent();

                return true;
            }

            public static bool DestroyItem(this SlotList slotList, int instanceId, int count)
            {
                int index = slotList.FindItemIndexById(instanceId);
                if (index < 0)
                {
                    return false;
                }
				slotList.newFlagMgr.Remove (index);
                if (slotList[index].stackCount == count)
                {
                    ItemMgr.Instance.DestroyItem(instanceId);
                    slotList[index] = null;
                }
                else
                {
                    slotList[index].stackCount -= count;
                }
                slotList.SendUpdateEvent();
                return true;
            }

            public static void Reduce(this SlotList slotList)
            {
                List<ItemObject> list = new List<ItemObject>(slotList);
                slotList.Clear();

                foreach (ItemObject item in list)
                {
                    if (null == item)
                    {
                        continue;
                    }

                    if (item.GetStackMax() > 1)
                    {
                        Add(slotList, item.protoId, item.stackCount);

                        ItemMgr.Instance.DestroyItem(item.instanceId);
                    }
                    else
                    {
                        slotList.Add(item);
                    }
                }
            }
        }
    }

    public class SlotList : IEnumerable<ItemObject>
    {
        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;

        ItemObject[] mSlots;

		//ItemObject[] mSlots_Equip;

        //public bool isMissionSlot() 
        //{
        //    if (mSlots.Any(delegate(ItemObject o)
        //    {
        //        if (o.protoData.category == "Quest Item")
        //            return true;
        //        return false;
        //    }))
        //        return true;
        //    else
        //        return false;
        //}

        public SlotList(int capacity)
        {
            mSlots = new ItemObject[capacity];
			_equipItem = new EquipItemMgr();
        }

        //used to import
        public SlotList() { }

        //public SlotList(ItemObject[] items)
        //{
        //    mSlots = items;
        //}

        NewFlagMgr mNewFlagMgr;
        public NewFlagMgr newFlagMgr
        {
            get
            {
                if (mNewFlagMgr == null)
                {
                    mNewFlagMgr = new NewFlagMgr();
                }
                return mNewFlagMgr;
            }
        }

		EquipItemMgr _equipItem;
		public EquipItemMgr mEquipItem
		{
			get{
				if(_equipItem == null)
					_equipItem = new EquipItemMgr();
				
				return _equipItem;
			}
		}

        public class ChangeEvent : PeEvent.EventArg
        {
            public enum Op
            {
                Set,
                Reset,
                Clear,
                Sort,
                Update,
                Max
            }

            public Op op;
            public int index;
        }

        PeEvent.Event<ChangeEvent> mEventor = new PeEvent.Event<ChangeEvent>();
        public PeEvent.Event<ChangeEvent> eventor
        {
            get
            {
                return mEventor;
            }
        }

        public void SendUpdateEvent()
        {
            eventor.Dispatch(new ChangeEvent() { op = ChangeEvent.Op.Update, index = -1 });
        }

		private void AddEquipItem(ItemObject obj)
		{
			mEquipItem.Add(obj);
		}

        public ItemObject this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    return null;
                }

                return mSlots[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    return;
                }



				if(mSlots[index] == null && value != null)
				{
					mEquipItem.Add(value);
				}

				if(mSlots[index] != null && value == null)
				{
					mEquipItem.ReMove(mSlots[index]);
				}

				if(mSlots[index] != null && value != null)
				{
					mEquipItem.ReMove(mSlots[index]);
					mEquipItem.Add(value);
				}

                mSlots[index] = value;

                eventor.Dispatch(new ChangeEvent() { op = (null == value ? ChangeEvent.Op.Reset : ChangeEvent.Op.Set), index = index });

				if(null == mSlots[index])
					newFlagMgr.Remove(index);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < mSlots.Length; i++)
            {
                mSlots[i] = null;
            }
			mEquipItem.Clear();
            eventor.Dispatch(new ChangeEvent() { op = ChangeEvent.Op.Clear, index = -1 });
        }

        public void Sort()
        {
            List<ItemObject> list = new List<ItemObject>(mSlots);

            list.RemoveAll((item) =>
            {
                if (null == item)
                {
                    return true;
                }

                return false;
            });

            list.Sort((item1, item2) =>
            {
                if (item1.protoData.sortLabel > item2.protoData.sortLabel)
                {
                    return 1;
                }
                else if (item1.protoData.sortLabel < item2.protoData.sortLabel)
                {
                    return -1;
                }
                else
				{ 	
					if (item1.protoId > item2.protoId)
					{
						return 1;
					}
					else if (item1.protoId < item2.protoId)
					{
						return -1;
					}
					else{
						if(item1.stackCount>item2.stackCount)
							return 1;
						else if(item1.stackCount<item2.stackCount)
							return -1;
					}
					return 0;
                }
            });

            for (int i = 0; i < mSlots.Length; i++)
            {
                if (i >= list.Count)
                {
                    mSlots[i] = null;
                }
                else
                {
                    this[i] = list[i];
                }
            }

            eventor.Dispatch(new ChangeEvent() { op = ChangeEvent.Op.Sort, index = -1 });

			newFlagMgr.RemoveAll ();
        }

        public void UpdateNewFlag(float deltaTime)
        {
            newFlagMgr.Update(deltaTime);
        }

        public bool Add(ItemObject itemObject, bool isNew = false)
        {
            if (null == itemObject)
            {
                return false;
            }

            int index = VacancyIndex();
            if (-1 == index)
            {
                return false;
            }

            if (isNew)
            {
                newFlagMgr.Add(index);
            }
            this[index] = itemObject;
            if (itemObject.GetCmpt<OwnerData>() != null)
            {
                if (OwnerData.deadNPC != null) 
                {
                    itemObject.GetCmpt<OwnerData>().npcID = OwnerData.deadNPC.npcID;
                    itemObject.GetCmpt<OwnerData>().npcName = OwnerData.deadNPC.npcName;
                }
            }
            return true;
        }
        //public bool Remove(int objectId)
        //{
        //    int index = FindItemIndexById(objectId);
        //    if (-1 == index)
        //    {
        //        Debug.LogError("cant find item :" + objectId);
        //        return false;
        //    }

        //    newFlagMgr.Remove(index);
        //    this[index] = null;
        //    return true;
        //}

        public int VacancyIndex()
        {
            for (int i = 0; i < Length; i++)
            {
                if (this[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetVacancyCount()
        {
            return Length - GetItemCount();
        }


		public int vacancyCount
		{
			get { return Length - GetItemCount(); }
		}


		public int GetItemCount()
        {
            int total = 0;

            for (int i = 0; i < mSlots.Length; i++) {
				if (null != mSlots [i]) {
					total++;
				}
			}
            return total;
        }

        public int Count
        {
            get
            {
                return Length;
            }
        }

        public int Length
        {
            get
            {
                return mSlots.Length;
            }
        }

        //Lz-2016.04.27 不是Null的放在前面 a==null 为1 放后面，不是为0放前面
        public void OderByIsNull()
        {
            this.mSlots=this.mSlots.OrderBy(a => a == null).ToArray();
        }

        public bool Swap(int index1, int index2)
        {
            if (index1 >= Length || index2 > Length || index1 < 0 || index2 < 0)
            {
                return false;
            }

            ItemObject temp = this[index1];
            this[index1] = this[index2];
            this[index2] = temp;

			newFlagMgr.Remove (index1);
			newFlagMgr.Remove (index2);

            return true;
        }

        public List<ItemObject> ToList()
        {
            List<ItemObject> tmp = new List<ItemObject>(mSlots);
            tmp.RemoveAll((item) =>
            {
                if (null == item)
                {
                    return true;
                }

                return false;
            });

            return tmp;
        }

        public int FindItemIndexByProtoId(int protoId)
        {
            for (int i = 0; i < Length; i++)
            {
                if (null == mSlots[i])
                {
                    continue;
                }

                if (protoId == mSlots[i].protoId)
                {
                    return i;
                }
            }

            return -1;
        }

        public ItemObject FindItemByProtoId(int protoId)
        {
            return this[FindItemIndexByProtoId(protoId)];
        }

        public int FindItemIndexById(int instanceId)
        {
            for (int i = 0; i < Length; i++)
            {
                if (null == mSlots[i])
                {
                    continue;
                }

                if (instanceId == mSlots[i].instanceId)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool HasItem(int instanceId)
        {
            return -1 != FindItemIndexById(instanceId);
        }

        public static SlotList ResetCapacity(SlotList origin, int capacity)
        {
            SlotList slots = new SlotList(capacity);
            if (null == origin)
            {
                return slots;
            }

            Array.Copy(origin.mSlots, slots.mSlots, Mathf.Min(origin.Length, capacity));
            return slots;
        }

        IEnumerator<ItemObject> IEnumerable<ItemObject>.GetEnumerator()
        {
            return mSlots.AsEnumerable<ItemObject>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mSlots.GetEnumerator();
        }

        public byte[] Export()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(ms))
                {
                    w.Write(CURRENT_VERSION);
                    w.Write(Length);

                    for (int i = 0; i < Length; i++)
                    {

                        ItemObject item = this[i];

                        if (null != item)
                        {
                            w.Write((int)item.instanceId);
                        }
                        else
                        {
                            w.Write((int)-1);
                        }
                    }
                }

                return ms.ToArray();
            }
        }

        public void Import(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer, false))
            {
                using (BinaryReader r = new BinaryReader(ms))
                {
                    int version = r.ReadInt32();
                    if (version > CURRENT_VERSION)
                    {
                        Debug.LogError("error version:" + version);
                    }
                    int length = r.ReadInt32();
					if(mSlots == null || mSlots.Length < length){
						mSlots = new ItemObject[length];
					} else {
						Array.Clear(mSlots, 0, mSlots.Length);
					}

                    for (int i = 0; i < length; i++)
                    {
                        int id = r.ReadInt32();

                        ItemObject item = ItemMgr.Instance.Get(id);
                        mSlots[i] = item;
						AddEquipItem(item);
                    }
                }
            }
        }
    }
}