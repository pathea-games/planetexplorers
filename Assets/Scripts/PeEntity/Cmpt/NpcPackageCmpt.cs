using ItemAsset;
using ItemAsset.SlotListHelper;
using System.Collections.Generic;
using UnityEngine;

namespace Pathea
{
    public class AutoIncreaseMoney
    {
        bool mAdded;
        int mMax;
        int mIncPerDay;

        Money mMoney;

        public AutoIncreaseMoney(Money money)
        {
            mMoney = money;
        }

        public AutoIncreaseMoney(Money money, int max, int incPerDay, bool added):this(money)
        {
            mMax = max;
            mIncPerDay = incPerDay;
            mAdded = added;
        }

        public void Update()
        {
            if (GameTime.Timer.SecondInDay < GameTime.Timer.Day2Sec / 2)
            {
                mAdded = false;
            }
            else
            {
                if (!mAdded)
                {
                    if (null != mMoney)
                    {
                        mMoney.current += mIncPerDay;
                    }
                }

                mAdded = true;
            }
        }

        public void Import(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                mMax = r.ReadInt32();
                mIncPerDay = r.ReadInt32();
                mAdded = r.ReadBoolean();
            });
        }

        public byte[] Export()
        {
            return PETools.Serialize.Export((w) =>
            {
                w.Write(mMax);
                w.Write(mIncPerDay);
                w.Write(mAdded);
            });
        }

        public override string ToString()
        {
            return string.Format("[PeMoney, cur:{0}, max{1}, incPerDay{2}]", (null != mMoney ? mMoney.current:0), mMax, mIncPerDay);
        }
    }

	public class NpcPackageCmpt : PackageCmpt
	{
        const int PkgCapacity =15;
        const int PrivateCapacity = 30;
		const int HandinCapacity = 10;

		SlotList mPrivateSlotList = new SlotList(PrivateCapacity);
        SlotList mSlotList = new SlotList(PkgCapacity);
		SlotList mHandinList = new SlotList(HandinCapacity);
        public AutoIncreaseMoney mAutoIncreaseMoney = null;

		//List<ItemObject> _mEquipObjs = new List<ItemObject>();

        public void InitAutoIncreaseMoney(int max, int valuePerDay)
        {
            mAutoIncreaseMoney = new AutoIncreaseMoney(money, max, valuePerDay, true);
        }
		
        public SlotList GetPrivateSlotList()
        {
            return mPrivateSlotList;
        }

        public SlotList GetSlotList()
        {
            return mSlotList;
        }

		public SlotList GetHandinList()
		{
			return mHandinList;
		}

        public bool IsFull()
        {
			return mSlotList.GetVacancyCount()  == 0 && mHandinList.GetVacancyCount() == 0;
        }

		public bool HandinIsFull()
		{
			return mHandinList.GetVacancyCount() == 0;
		}

		public override bool Add(ItemObject item, bool isNew = false)
        {
            if (IsFull())
                return mHandinList.Add(item,isNew);
            else
                return mSlotList.Add(item,isNew);
        }

		public bool AddToPrivate(ItemObject item)
		{
			return mPrivateSlotList.Add(item);
		}

        public bool AddToNetHandin(ItemObject item)
        {
            return mHandinList.Add(item, true);
        }

        public bool AddToNet(ItemObject item)
        {
            return mSlotList.Add(item, true);
        }

        public  bool AddToHandin(ItemObject item)
		{
            if (!HandinIsFull())
                return mHandinList.Add(item, true);
            else
                return mSlotList.Add(item, true);
		}

        public override bool Remove(ItemObject item)
        {
            int index = mSlotList.FindItemIndexById(item.instanceId);
            //if (-1 == index)
            //{
            //    Debug.LogError("cant find item :" + item.instanceId);
            //    return false;
            //}

            if (index != -1)
                mSlotList[index] = null;

            index = mHandinList.FindItemIndexById(item.instanceId);
            if (-1 == index)
            {
                //Debug.LogError("cant find item :" + item.instanceId);
                return false;
            }

            mHandinList[index] = null;
            return true;
        }

		public override bool Contain(ItemObject item)
		{
			int index = mSlotList.FindItemIndexById(item.instanceId);
			if (index != -1)
				return true;
			
			index = mHandinList.FindItemIndexById(item.instanceId);
			if (-1 != index)		
				return true;

			return false;
		}

		public bool CanAddHandinItemList(List<ItemObject> items)
		{
			return mHandinList.GetVacancyCount() >= items.Count; 
		}

		public bool CanAddPrivateItemList(List<ItemObject> items)
		{
			return mPrivateSlotList.GetVacancyCount() >= items.Count; 
		}

		public bool CanAddItemList(List<MaterialItem> items)
		{
            return mSlotList.GetVacancyCount() + mHandinList.GetVacancyCount() >= items.Count;
		}

		public bool CanAddHandinItemList(List<MaterialItem> items)
		{
			return mHandinList.GetVacancyCount() >= items.Count;
		}

		public bool AddPrivateItemList(List<ItemObject> items)
		{
			if (!CanAddPrivateItemList(items))
			{
				return false;
			}
			
			foreach (ItemObject item in items)
			{
				if (false == AddToPrivate(item))
				{
					return false;
				}
			}
			return true;
		}

		public bool AddHandinItemList(List<ItemObject> items)
		{
			if (!CanAddHandinItemList(items))
			{
				return false;
			}
			
			foreach (ItemObject item in items)
			{
				if (false == AddToHandin(item))
				{
					return false;
				}
			}
			return true;
		}

		public void ClearHandin()
		{
			mHandinList.Clear();
		}

		public void Clear()
		{
			mSlotList.Clear();
		}
		
		public override bool CanAddItemList(List<ItemObject> items)
		{
			return mSlotList.GetVacancyCount() + mHandinList.GetVacancyCount()>= items.Count;
		}

        public override bool AddItemList(List<ItemObject> items)
        {
            if (!CanAddItemList(items))
            {
                return false;
            }

            foreach (ItemObject item in items)
            {
                if (false == Add(item))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool AdditemFromEquip(List<ItemObject> items)
        {
            return AddItemList(items);
        }

        public override int GetItemCount(int protoId)
        {
            return mSlotList.GetCount(protoId) + mHandinList.GetCount(protoId);
        }

		public override bool ContainsItem(int protoId)
		{
			return mSlotList.ConatinsItem (protoId) || mHandinList.ConatinsItem (protoId);
		}

        public override int GetCountByEditorType(int editorType)
        {
            return mSlotList.GetCountByEditorType(editorType) + mHandinList.GetCountByEditorType(editorType);
        }

        public override int GetAllItemsCount()
        {
            return mSlotList.GetAllItemsCount() + mHandinList.GetAllItemsCount();
        }

        public override bool Destory(int protoId, int count)
        {
            return mSlotList.Destroy(protoId, count) || mHandinList.Destroy(protoId, count);
        }
        public override bool DestroyItem(int instanceId, int count)
        {
            return mSlotList.DestroyItem(instanceId, count) || mHandinList.DestroyItem(instanceId, count);
        }

        public override bool DestroyItem(ItemObject item, int count)
        {
            return mSlotList.DestroyItem(item.instanceId, count) || mHandinList.DestroyItem(item.instanceId, count);
        }
        public override bool Add(int protoId, int count)
        {
            if (IsFull())
                return mHandinList.Add(protoId, count);
            else
                return mSlotList.Add(protoId, count);
        }
	    
		public  bool AddToHandin(int protoId, int count)
		{

            if (!HandinIsFull())
                return mHandinList.Add(protoId, count, true);
            else
				return mSlotList.Add(protoId,count, true);
		}


		public List<ItemObject> GetEquipItemObjs(EeqSelect selet)
		{
			List<ItemObject> eqs = new List<ItemObject>();
			eqs.AddRange(mSlotList.mEquipItem.GetEquipItemObjs(selet));
			eqs.AddRange(mHandinList.mEquipItem.GetEquipItemObjs(selet));
			return eqs;
		}

        public List<ItemObject> GetAtkEquipItemObjs(AttackType Atktype)
        {
            List<ItemObject> eqs = new List<ItemObject>();
            eqs.AddRange(mSlotList.mEquipItem.GetAtkEquips(Atktype));
            eqs.AddRange(mHandinList.mEquipItem.GetAtkEquips(Atktype));
            return eqs;
        }

        //public List<ItemObject> GetAttackEquipItemObjs()
        //{
        //    List<ItemObject> eqs = new List<ItemObject>();
        //    eqs.AddRange(mSlotList.mEquipItem.GetAttackEquipObjs());
        //    eqs.AddRange(mHandinList.mEquipItem.GetAttackEquipObjs());
        //    return eqs;
        //}

		public bool HasEq(EeqSelect Select)
        {
			return GetEquipItemObjs(Select).Count > 0;
        }

		public bool HasAtkEquip(AttackType type)
		{
			return mSlotList.mEquipItem.hasAtkEquip(type) || mHandinList.mEquipItem.hasAtkEquip(type);
		}

        public override bool Set(int protoId, int count)
        {
            return mSlotList.Set(protoId, count);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (null != mAutoIncreaseMoney)
            {
                mAutoIncreaseMoney.Update();
            }
        }
        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);
            PETools.Serialize.WriteBytes(mSlotList.Export(), w);
            PETools.Serialize.WriteBytes(mPrivateSlotList.Export(), w);


			if (null != mAutoIncreaseMoney)
            {
                PETools.Serialize.WriteBytes(mAutoIncreaseMoney.Export(), w);
            }
            else
            {
                PETools.Serialize.WriteBytes(null, w);
            }

			if(mHandinList != null)
			{
				PETools.Serialize.WriteBytes(mHandinList.Export(), w);
			}
			else
			{
				PETools.Serialize.WriteBytes(null, w);
			}
        }

        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);
            byte[] buffSlot = PETools.Serialize.ReadBytes(r);
            byte[] buffExchange = PETools.Serialize.ReadBytes(r);
            byte[] buffAutoIncrease = PETools.Serialize.ReadBytes(r);
			byte[] buffHandin = PETools.Serialize.ReadBytes(r);
			
            mSlotList.Import(buffSlot);
            mPrivateSlotList.Import(buffExchange);

            if (null != buffAutoIncrease && buffAutoIncrease.Length > 0)
            {
                mAutoIncreaseMoney = new AutoIncreaseMoney(money);
                mAutoIncreaseMoney.Import(buffAutoIncrease);
            }
            else
            {
                mAutoIncreaseMoney = null;
            }

			if(null != buffHandin && buffHandin.Length >0)
			{
				mHandinList.Import(buffHandin);
			}
        }
    }


	public abstract class PackageCmpt_Equip 
	{
		protected SlotList mEqiupSlotList = new SlotList();
		public abstract SlotList GetEquipList();

	}

    namespace PeEntityExtNpcPackage
    {
        public static class PeEntityExtNpcPackage
        {
            #region package

			public static int GetBagItemCount(this PeEntity entity)
			{
			   return 0;
			}


			public static int GetItemCount(this PeEntity entity,int protoId)
			{
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return 0;
				}

				return pkg.GetItemCount(protoId);
			}

			public static ItemAsset.ItemObject GetBagItemObj(this PeEntity entity,int prototypeId)
			{
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return null;
				}

                SlotList useList = pkg.GetSlotList();
                if (null != useList.FindItemByProtoId(prototypeId))
                    return useList.FindItemByProtoId(prototypeId);

                useList = pkg.GetHandinList();
                return useList.FindItemByProtoId(prototypeId);
			}

            public static ItemAsset.ItemObject GetBagItem(this PeEntity entity, int index)
            {
				return null;
            }

            public static bool AddToBag(this PeEntity entity, ItemAsset.ItemObject item,bool isnew = false)
            {
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return false ;
				}

				return pkg.Add(item,isnew);
            }

			public static bool RemoveFromBag(this PeEntity entity, ItemAsset.ItemObject item)
			{
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return false;
				}

				return pkg.Remove(item);
			}


			public static List<ItemAsset.ItemObject> GetAtkEquipObjs(this PeEntity entity,AttackType atkType)
			{
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return null;
				}

                return pkg.GetAtkEquipItemObjs(atkType);
			}

            public static List<ItemAsset.ItemObject> GetEquipObjs(this PeEntity entity, EeqSelect selcet)
            {
                NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
                if (null == pkg)
                {
                    return null;
                }

                return pkg.GetEquipItemObjs(selcet);
            }


			public static bool IsInPackage(this PeEntity entity,ItemObject obj)
			{
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return false;
				}

				return pkg.Contain(obj);
			}

            public static bool HasEquip(this PeEntity entity,EeqSelect sle)
            {
                NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
                if (null == pkg)
                {
                    return false;
                }

				return pkg.HasEq(sle);
            }
		
			//only Melee or range
			public static bool HasCanAttackEquip(this PeEntity entity,EeqSelect sle)
			{
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return false;
				}
			
				List<ItemObject> objs = pkg.GetEquipItemObjs(sle);
				if(objs == null)
					return false;

				for(int i=0;i<objs.Count;i++)
				{
					if(SelectItem.EquipCanAttack(entity,objs[i]))
						return true;
				}
				return false;
			}

			public static bool HasAtkEquips(this PeEntity entity,AttackType type)
			{
				NpcPackageCmpt pkg = entity.packageCmpt as NpcPackageCmpt;
				if (null == pkg)
				{
					return false;
				}

				return pkg.HasAtkEquip(type);
			}
		
            #endregion
        }
    }
}