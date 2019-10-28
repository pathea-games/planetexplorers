using ItemAsset;
using ItemAsset.SlotListHelper;
using ItemAsset.PackageHelper;
using System.Collections.Generic;
using System.Linq;

namespace Pathea
{
    public class ShortCutItem : ItemSample
    {
		int m_ItemInstanceId;
        public int itemInstanceId
		{
			get { return m_ItemInstanceId; }
			set { m_ItemInstanceId = value; }
		}

		public bool UseProtoID { get { return (null != protoData) ? (protoData.maxStackNum > 1) : false; } }

		public override void Export(System.IO.BinaryWriter w)
        {
            PETools.Serialize.WriteData(base.Export, w);
            w.Write(itemInstanceId);
        }

        public override void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, (r) =>
            {
                byte[] baseBuff = PETools.Serialize.ReadBytes(r);
                base.Import(baseBuff);
                itemInstanceId = r.ReadInt32();
            });
        }
    }

    public class ShortCutSlotList : IEnumerable<ShortCutItem>
    {
        ItemPackage mPkg;
		EquipmentCmpt mEquip;
        ShortCutItem[] mShortCutItems;

		public event System.Action onListUpdate;

		public ShortCutSlotList(int count, ItemPackage pkg, EquipmentCmpt equip)
        {
            mShortCutItems = new ShortCutItem[count];
            mPkg = pkg;
			mEquip = equip;
        }

        public int length
        {
            get
            {
                return mShortCutItems.Length;
            }
        }

        public ItemObject GetItemObj(int index)
        {
            ShortCutItem shortCutItem = GetItem(index);
            if (null == shortCutItem)
            {
                return null;
            }

            ItemObject itemObj = ItemMgr.Instance.Get(shortCutItem.itemInstanceId);

            if (null == itemObj)
            {
				if(shortCutItem.UseProtoID)
				{
					if(null != mPkg)
						return mPkg.GetItemByProtoID(shortCutItem.protoId);
				}

				return null;
            }

			if (null != mPkg)
			{
				SlotList slotList = mPkg.GetSlotList(shortCutItem.protoId);
				if (null != slotList && slotList.HasItem(shortCutItem.itemInstanceId))
					return itemObj;
			}

			if(null != mEquip)
			{
				for(int i = 0; i < mEquip._ItemList.Count; i++)
					if(mEquip._ItemList[i].instanceId == shortCutItem.itemInstanceId)
						return mEquip._ItemList[i];
			}

			return null;
        }

        public ShortCutItem GetItem(int index)
        {
            if (index >= mShortCutItems.Length || index < 0)
            {
                return null;
            }

            return mShortCutItems[index];
        }

        public void PutItemObj(ItemObject itemObj, int index)
        {
            if (null == itemObj)
                return;

            ShortCutItem shortCutItem = new ShortCutItem()
            {
                protoId = itemObj.protoId,
                itemInstanceId = itemObj.instanceId
            };

            PutItem(shortCutItem, index);
        }

        public void PutItem(ShortCutItem item, int index, bool updateList = true)
        {
            if (index >= mShortCutItems.Length || index < 0)
            {
                return;
            }

            mShortCutItems[index] = item;

			if(updateList)
				UpdateShortCut();
        }

        public void UpdateShortCut()
        {
            if (null == mPkg)
            {
                return;
            }

            for (int i = 0; i < mShortCutItems.Length; i++)
            {
                ShortCutItem item = mShortCutItems[i];

				if (null == item || null == item.protoData)
                    continue;
				
				bool useProtoID = item.protoData.maxStackNum > 1 && 60 != item.protoId; //grenade == 60

				if(useProtoID)
				{
					int itemCount = mPkg.GetCount(item.protoId);
					if(itemCount == 0)
						PutItem(null, i, false);
					else
						item.SetStackCount(itemCount);
				}
				else
				{
					ItemObject itemObj = ItemMgr.Instance.Get(item.itemInstanceId);
					if(null == itemObj)
					{
						PutItem(null, i, false);
						continue;
					}
					if(itemObj.protoData.maxStackNum > 1)
						item.SetStackCount(itemObj.GetCount());

					CheckEquipInPackage(item, i);
				}
            }
			SendUpdateMsg();
        }

		public void UpdateShortCut(ItemObject itemObj)
		{ 
			if (null == mPkg)
			{
				return;
			}			
			bool updateCount = itemObj.protoData.maxStackNum > 1;//grenade == 60

			if(!updateCount)
				return;
			for (int i = 0; i < mShortCutItems.Length; i++)
			{
				ShortCutItem item = mShortCutItems[i];
				if (null == item)
					continue;
				if(item.protoId == itemObj.protoId)
				{
					if(60 == item.protoId)
						item.SetStackCount(itemObj.GetCount());
					else
						item.SetStackCount(mPkg.GetCount(item.protoId));
					return;
				}
			}
		}

		void CheckEquipInPackage(ShortCutItem item, int index)
		{
			if (null != mPkg)
			{
				SlotList slotList = mPkg.GetSlotList(item.protoId);
				if (null != slotList && slotList.HasItem(item.itemInstanceId))
					return;
			}
			
			if(null != mEquip && mEquip.IsEquipNow(item.itemInstanceId))
				return;
			
			PutItem(null, index, false);
		}

		void SendUpdateMsg()
		{
			if(null != onListUpdate)
				onListUpdate();
		}

        IEnumerator<ShortCutItem> IEnumerable<ShortCutItem>.GetEnumerator()
        {
            return mShortCutItems.AsEnumerable<ShortCutItem>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return mShortCutItems.GetEnumerator();
        }

        #region Serialize
		public void Export(System.IO.BinaryWriter w)
        {
            w.Write((int)mShortCutItems.Length);

            for (int i = 0; i < mShortCutItems.Length; i++)
            {
                if(mShortCutItems[i] != null){
					PETools.Serialize.WriteData(mShortCutItems[i].Export, w);
				} else {
					PETools.Serialize.WriteData(null, w);
				}
            }
        }

        public void Import(byte[] buff)
        {
            PETools.Serialize.Import(buff, (r) =>
            {
                int len = r.ReadInt32();
                mShortCutItems = new ShortCutItem[len];

                for (int i = 0; i < len; i++)
                {
                    byte[] data = PETools.Serialize.ReadBytes(r);
                    if (null == data)
                    {
                        mShortCutItems[i] = null;
                    }
                    else
                    {
                        mShortCutItems[i] = new ShortCutItem();

                        mShortCutItems[i].Import(data);

						if(null == mShortCutItems[i].protoData)
							mShortCutItems[i] = null;
                    }
                }
				
				SendUpdateMsg();
            });
        }
        #endregion
    }
}