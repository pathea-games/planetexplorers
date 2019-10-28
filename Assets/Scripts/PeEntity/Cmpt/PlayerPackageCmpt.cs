using ItemAsset;
using ItemAsset.PackageHelper;
using System.Collections.Generic;

namespace Pathea
{
    public class PlayerPackageCmpt : PackageCmpt
    {
        public static bool LockStackCount = false;

        public const int PkgCapacity = 420;

        PlayerPackage mPackage;// = new PlayerPackage(PkgCapacity);
        ShortCutSlotList mShotCutSlotList = null;

		bool mUpdateShortCut;

        public override void OnUpdate()
        {
            base.OnUpdate();
            package.UpdateNewFlag(UnityEngine.Time.deltaTime);
			if(mUpdateShortCut)
			{
				mUpdateShortCut = false;
	            shortCutSlotList.UpdateShortCut();
			}
        }

        public PlayerPackage package
        {
            get
            {
                if (mPackage == null){
					bool bCreateMisPkg = (PeGameMgr.IsMulti && PlayerNetwork.mainPlayerId != Entity.Id) ? false : true;
					mPackage = new PlayerPackage(PkgCapacity, bCreateMisPkg);
				}
                return mPackage;
            }
        }

        #region Serialize
        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);

            byte[] buffer = PETools.Serialize.ReadBytes(r);

            package.Import(buffer);

            if (Pathea.ArchiveMgr.Instance.GetCurArvhiveVersion() > Pathea.Archive.Header.Version_2)
            {
                buffer = PETools.Serialize.ReadBytes(r);

                PlayerPackage._missionPak.Import(buffer);
            }

            buffer = PETools.Serialize.ReadBytes(r);

            shortCutSlotList.Import(buffer);
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);

            PETools.Serialize.WriteData(package.Export, w);

            PETools.Serialize.WriteData(PlayerPackage._missionPak.Export, w);

			PETools.Serialize.WriteData(shortCutSlotList.Export, w);
        }
        #endregion

        #region ShortCutSlotList
        public ShortCutSlotList shortCutSlotList
        {
            get
            {
                if (null == mShotCutSlotList)
                {
					EquipmentCmpt equip = Entity.GetComponent<EquipmentCmpt>();
                    //log lz-2016.05.04 因为第一个格子用来显示当前的武器了，所以这个地方减少3个
					mShotCutSlotList = new ShortCutSlotList(27, package._playerPak, equip);
					package._playerPak.changeEventor.Subscribe(OnItemPakChange);
					if(null != equip)
						equip.changeEventor.Subscribe(OnItemPakChange);
                }
                return mShotCutSlotList;
            }
        }
		void OnItemPakChange()
		{
			mUpdateShortCut = true;
		}

		void OnItemPakChange(object sender, ItemPackage.EventArg arg)
		{
			OnItemPakChange();
		}

		void OnItemPakChange(object sender, EquipmentCmpt.EventArg arg)
		{
			OnItemPakChange();
		}

		public void NetOnItemUpdate(ItemObject itemObj = null)
		{
			if(null == itemObj)
			{
				OnItemPakChange();
			}
			else
			{
				shortCutSlotList.UpdateShortCut(itemObj);
			}
		}

        public bool PutItemToShortCutList(int pkgIndex, int shortCutListIndex,bool isMission = false)
        {
            ItemObject itemObj = package.GetItem(pkgIndex, isMission);
            if (null == itemObj)
            {
                return false;
            }

            shortCutSlotList.PutItemObj(itemObj, shortCutListIndex);
            return true;
        }

        #endregion

        public class GetItemEventArg : PeEvent.EventArg
        {
            public int protoId;
            public int count;
        }

        PeEvent.Event<GetItemEventArg> mEventor = new PeEvent.Event<GetItemEventArg>();

        public PeEvent.Event<GetItemEventArg> getItemEventor
        {
            get
            {
                return mEventor;
            }
        }

        void SendGetItemEvent(ItemObject item)
        {
            SendGetItemEvent(item.protoId, item.stackCount);
        }

        void SendGetItemEvent(int protoId, int count)
        {
            getItemEventor.Dispatch(new GetItemEventArg() { protoId = protoId, count = count });
        }

        public override bool Add(ItemObject item, bool isNew = false)
        {
			if(package.CanAdd(item))
			{
				package.AddItem(item, isNew);
	            SendGetItemEvent(item);
                CheckMainPlayerGetItem(item.protoId);
                return true;
			}
			return false;
        }

        public override bool Remove(ItemObject item)
        {
            return package.RemoveItem(item);
        }

		public override bool Contain(ItemObject item)
		{
			return false;
		}

        public override bool CanAddItemList(List<ItemObject> items)
        {
            return package.CanAddItemList(items);
        }

        public override bool AddItemList(List<ItemObject> items)
        {
            foreach (ItemObject item in items)
            {
                SendGetItemEvent(item);
            }

            return package.AddItemList(items, true);
        }

        public override bool AdditemFromEquip(List<ItemObject> items)
        {
            return package.AddItemList(items);
        }

        public override int GetItemCount(int protoId)
        {
            return package.GetCount(protoId);
        }
		
		public override bool ContainsItem(int protoId)
		{
			return package.ContainsItem (protoId);
		}

        public override int GetCountByEditorType(int editorType)
        {
            return package.GetCountByEditorType(editorType);
        }

        public override int GetAllItemsCount()
        {
            return package.GetAllItemsCount();
        }

        public override bool Destory(int protoId, int count)
        {
            if (LockStackCount)
            {
                return true;
            }

            return package.Destroy(protoId, count);
        }

        public override bool DestroyItem(int instanceId, int count)
        {
            if (LockStackCount)
            {
                return true;
            }

            return package.DestroyItem(instanceId, count);
        }

        public override bool DestroyItem(ItemObject item, int count)
        {
            if (LockStackCount)
            {
                return true;
            }

            return package.DestroyItem(item, count);
        }

        public override bool Add(int protoId, int count)
        {
			if(package.CanAdd(protoId, count))
			{
				package.Add(protoId, count, true);
	            SendGetItemEvent(protoId, count);
                CheckMainPlayerGetItem(protoId);
                return true;
			}
            return false;
        }

        public override bool Set(int protoId, int count)
        {
            return package.Set(protoId, count);
        }

        //lz-2016.08.22 引导检测玩家得到物品
        private void CheckMainPlayerGetItem(int itemID)
        {
            if (Entity.IsMainPlayer)
            {
                InGameAidData.CheckGetItem(itemID);
            }
        }
    }

    namespace PeEntityExtPlayerPackage
    {
        public static class PeEntityExtPlayerPackage
        {
            public static int GetPkgItemCount(this PeEntity entity, int prototypeId)
            {
                PlayerPackageCmpt pkg = entity.GetCmpt<PlayerPackageCmpt>();
                if (null == pkg)
                {
                    return 0;
                }

                return pkg.package.GetCount(prototypeId);
            }

            public static bool AddToPkg(this PeEntity entity, int prototypeId, int count)
            {
                PlayerPackageCmpt pkg = entity.GetCmpt<PlayerPackageCmpt>();
                if (null == pkg)
                {
                    return false;
                }

                return pkg.Add(prototypeId, count);
            }

            public static bool RemoveFromPkg(this PeEntity entity, int prototypeId, int count)
            {
                PlayerPackageCmpt pkg = entity.GetCmpt<PlayerPackageCmpt>();
                if (null == pkg)
                {
                    return false;
                }

                return pkg.Destory(prototypeId, count);
            }

            public static int GetCreationItemCount(this PeEntity entity,ECreation type)
            {
                PlayerPackageCmpt pkg = entity.GetCmpt<PlayerPackageCmpt>();
                if (null == pkg)
                    return 0;

                return pkg.package.GetCreationCount(type);
            }
        }
    }
}