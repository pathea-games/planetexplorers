using ItemAsset;
using System.Collections.Generic;

namespace Pathea
{
    public abstract class PackageCmpt : PeCmpt, EquipmentCmpt.Receiver
	{
        public override void Start()
        {
			base.Start ();
			EquipmentCmpt equipment = Entity.GetCmpt<EquipmentCmpt>();
            equipment.mItemReciver = this;
        }

        Money mMoney = null;
        public Money money
        {
            get
            {
                if (null == mMoney)
                {
                    mMoney = new Money(this);
                }
                return mMoney;
            }
        }

		public abstract bool Add(ItemObject item, bool isNew = false);

        public abstract bool Remove(ItemObject item);

		public abstract bool Contain(ItemObject item);

        public abstract bool CanAddItemList(List<ItemObject> items);

        public abstract bool AddItemList(List<ItemObject> items);

        public virtual bool AdditemFromEquip(List<ItemObject> items) { return true; }

        public abstract int GetItemCount(int protoId);

		public abstract bool ContainsItem(int protoId);

        public abstract int GetCountByEditorType(int editorType);

        public abstract int GetAllItemsCount();

        public abstract bool Destory(int protoId, int count);

        public abstract bool DestroyItem(int instanceId, int count);

        public abstract bool DestroyItem(ItemObject item, int count);

        public abstract bool Add(int protoId, int count);

        public abstract bool Set(int protoId, int count);

        #region EquipmentCmpt.Receiver
        bool EquipmentCmpt.Receiver.CanAddItemList(List<ItemObject> items)
        {
            return CanAddItemList(items);
        }

        void EquipmentCmpt.Receiver.AddItemList(List<ItemObject> items)
        {
            AdditemFromEquip(items);
        }
        #endregion

        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);

            byte[] buff = PETools.Serialize.ReadBytes(r);
            money.Import(buff);
        }
        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);
            PETools.Serialize.WriteBytes(money.Export(), w);
        }
    }
}