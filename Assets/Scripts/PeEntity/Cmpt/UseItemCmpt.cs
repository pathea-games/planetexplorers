using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
    public class UseItemCmpt : PeCmpt
    {
        const int ReviveItemProtoId = 937;

        public class EventArg : PeEvent.EventArg
        {
            public ItemAsset.ItemObject itemObj;
        }

        PeEvent.Event<EventArg> mEventor = new PeEvent.Event<EventArg>();

        public PeEvent.Event<EventArg> eventor
        {
            get
            {
                return mEventor;
            }
        }

        NetCmpt mNet;
        PackageCmpt mPkg;
        SkillSystem.SkEntity mSkEntity;

        SkillSystem.SkEntity skEntity
        {
            get
            {
                if (mSkEntity == null)
                {
                    mSkEntity = Entity.GetGameObject().GetComponent<SkillSystem.SkEntity>();
                }
                return mSkEntity;
            }
        }

        public override void Start()
        {
            base.Start();

            mPkg = Entity.GetCmpt<PackageCmpt>();

            if (NetworkInterface.IsClient)
            {
                mNet = Entity.GetCmpt<NetCmpt>();
            }
        }

        bool ExtractBundle(ItemAsset.Bundle bundle)
        {
            if (null == bundle)
            {
                return false;
            }

            PackageCmpt pkg = Entity.GetCmpt<PackageCmpt>();
            if (null == pkg)
            {
                return false;
            }

            IEnumerable<ItemAsset.ItemObject> items = bundle.Extract();
            if (null == items)
            {
                return false;
            }

            List<ItemAsset.ItemObject> itemList = new List<ItemAsset.ItemObject>(items);
            if (!pkg.CanAddItemList(itemList))
            {
                return false;
            }

            pkg.AddItemList(itemList);
            return true;
        }

        bool ConsumeItem(ItemAsset.Consume consume)
        {
            if (null == consume)
            {
                return false;
            }

            if (null == skEntity)
            {
                return false;
            }

            return null != consume.StartSkSkill(skEntity);
        }

        bool LearnReplicatorFormula(ItemAsset.ReplicatorFormula formula, bool bLearn = true)
        {
            if (null == formula || formula.formulaId == null || formula.formulaId.Length <= 0)
            {
                return false;
            }

            ReplicatorCmpt replicatorCmpt = Entity.GetCmpt<ReplicatorCmpt>();
            if (null == replicatorCmpt)
            {
                return false;
            }

            Replicator replicator = replicatorCmpt.replicator;
            if (null == replicator)
            {
                return false;
            }

			if (bLearn) {
				bool ret = false;
				for (int i = 0; i < formula.formulaId.Length; i++) {
					if (replicator.AddFormula (formula.formulaId [i])) {
						ret = true;
					}
				}
				if (ret == false) {
					new PeTipMsg (PELocalization.GetString (4000001), PeTipMsg.EMsgLevel.Warning);
					return ret;
				}
			}

            LearnEffectAndSound();
            return true;
        }

        const int LearnEffectId = 88;
        const int LearnSoundId = 19;

        public void LearnEffectAndSound()
        {
            PeTrans trans = Entity.peTrans;
            if (trans == null){
                return;
            }
            Pathea.Effect.EffectBuilder.Instance.Register(LearnEffectId, null, trans.position, trans.rotation);
            AudioManager.instance.Create(trans.position, LearnSoundId);
        }

		bool LearnMetalScan(ItemAsset.MetalScan metalScan, bool bLearn = true)
        {
            if (null == metalScan){
                return false;
            }

            if (Entity.Id != PeCreature.Instance.mainPlayerId){
                return false;
            }

            foreach (int metalId in metalScan.metalIds)
            {
                if (!MetalScanData.HasMetal(metalId))
                {
					if(bLearn){
                    	MetalScanData.AddMetalScan(metalScan.metalIds);
					}
                    LearnEffectAndSound();
                    return true;
                }
            }
            return false;
        }

        bool TakeOnEquipment(ItemAsset.Equip equip)
        {
            if (null == equip)
            {
                return false;
            }

            Pathea.EquipmentCmpt equipCmpt = Entity.GetCmpt<Pathea.EquipmentCmpt>();

            if (null == equipCmpt)
            {
                return false;
            }

            if (equipCmpt.PutOnEquipment(equip.itemObj, true))
            {
                //lz-2016.08.31 装备成功播放音效
                GameUI.Instance.PlayPutOnEquipAudio();
                return true;
            }
            return false;
        }

        public bool RequestRevive()
        {
            if (!NetworkInterface.IsClient)
            {
                Revive();
            }
            else
            {
                if (null != PlayerNetwork.mainPlayer)
                {
                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerRevive, Entity.position);
                    return true;
                }
            }

            return false;
        }
        public bool ReviveServent(bool usePlayer = true)
        {
            NpcPackageCmpt Serventpackage = this.GetComponent<NpcPackageCmpt>();

            if (Serventpackage == null)
            {
                return false;
            }
            ItemAsset.SlotList package = Serventpackage.GetSlotList();
            ItemAsset.ItemObject Obj = package.FindItemByProtoId(ReviveItemProtoId);

            if (null == Obj)
            {
                package = Serventpackage.GetHandinList();
                Obj = package.FindItemByProtoId(ReviveItemProtoId);
            }

			if(Obj == null && !usePlayer )
				return false;

			if (Obj == null)
            {
                if (GameUI.Instance.mMainPlayer == null)
                    return false;
                PlayerPackageCmpt playerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();

                if (playerPackage == null)
                    return false;
                ItemAsset.ItemObject Obj2 = playerPackage.package.FindItemByProtoId(ReviveItemProtoId);
                if (Obj2 == null)
                    return false;

                return Use(Obj2, playerPackage);
                //return false ;
            }

            return Use(Obj);
        }

        public bool Revive()
        {
            //only player package
            PlayerPackageCmpt playerPkg = mPkg as PlayerPackageCmpt;

            if (playerPkg == null)
            {
                return false;
            }

            ItemAsset.ItemObject Obj = playerPkg.package.FindItemByProtoId(ReviveItemProtoId);
            if (Obj == null)
                return false;

            return Use(Obj);

        }

        public bool Request(ItemAsset.ItemObject item)
        {
            if (null == item)
            {
                return false;
            }

            if (item.protoId == ReviveItemProtoId)
            {
                return false;
            }

            Pathea.SkAliveEntity a = Entity.GetCmpt<Pathea.SkAliveEntity>();
            if (a != null)
            {
                if (a.isDead)
                {
                    return false;
                }
            }

            //net
            if (NetworkInterface.IsClient && null != mNet)
            {
                float cd = GetCdByItemProtoId(item.protoId);
                if (cd > 0)
                    return false;

                //ConsumeItem(item.GetCmpt<ItemAsset.Consume>());
                ItemAsset.Equip equip = item.GetCmpt<ItemAsset.Equip>();
                if (null != equip)
                {
                    if (mNet.network is PlayerNetwork)
                    {
                        Pathea.EquipmentCmpt cmpt = (mNet.network as PlayerNetwork).PlayerEntity.GetCmpt<Pathea.EquipmentCmpt>();
                        if (null != cmpt)
                        {
                            if (!cmpt.NetTryPutOnEquipment(item))
                            {
                                return false;
                            }
                        }
                    }
                }
				LearnReplicatorFormula(item.GetCmpt<ItemAsset.ReplicatorFormula>(), false);
				LearnMetalScan(item.GetCmpt<ItemAsset.MetalScan>(), false);
				mNet.RequestUseItem(item.instanceId);
                return true;
            }
            else
            {
                return Use(item);
            }
        }

        public bool Use(ItemAsset.ItemObject item, PlayerPackageCmpt UsePkg)
        {
            CheckMainPlayerUseItem(item.protoId);
            bool destroy = false;
            destroy = ExtractBundle(item.GetCmpt<ItemAsset.Bundle>()) || destroy;
            destroy = ConsumeItem(item.GetCmpt<ItemAsset.Consume>()) || destroy;
            destroy = LearnReplicatorFormula(item.GetCmpt<ItemAsset.ReplicatorFormula>()) || destroy;
            destroy = LearnMetalScan(item.GetCmpt<ItemAsset.MetalScan>()) || destroy;

            bool remove = TakeOnEquipment(item.GetCmpt<ItemAsset.Equip>());

            if (UsePkg != null)
            {
                if (destroy)
                {
                    UsePkg.DestroyItem(item, 1);
                }
                else if (remove)
                {
                    UsePkg.Remove(item);
                }
            }

            bool ret = destroy || remove;

            if (ret)
            {
                eventor.Dispatch(new EventArg() { itemObj = item }, this);
            }

            return ret;
        }

        public bool Use(ItemAsset.ItemObject item)
        {
            CheckMainPlayerUseItem(item.protoId);
            bool destroy = false;
            destroy = ExtractBundle(item.GetCmpt<ItemAsset.Bundle>()) || destroy;
            destroy = ConsumeItem(item.GetCmpt<ItemAsset.Consume>()) || destroy;
            destroy = LearnReplicatorFormula(item.GetCmpt<ItemAsset.ReplicatorFormula>()) || destroy;
            destroy = LearnMetalScan(item.GetCmpt<ItemAsset.MetalScan>()) || destroy;

            bool remove = TakeOnEquipment(item.GetCmpt<ItemAsset.Equip>());

            if (mPkg != null)
            {
                if (destroy)
                {
                    mPkg.DestroyItem(item, 1);
                }
                else if (remove)
                {
                    mPkg.Remove(item);
                }
            }

            bool ret = destroy || remove;

            if (ret)
            {
                eventor.Dispatch(new EventArg() { itemObj = item }, this);
            }

            return ret;
        }

        public void UseFromNet(ItemAsset.ItemObject item)
        {
            ConsumeItem(item.GetCmpt<ItemAsset.Consume>());
        }

        //lz-2016.08.22 检测引导用
        private void CheckMainPlayerUseItem(int itemID)
        {
            if (Entity.IsMainPlayer)
            {
                InGameAidData.CheckUseItem(itemID);
            }
        }

        #region cool down
        public float GetCd(int skillId)
        {
            return SkillSystem.SkInst.GetSkillCoolingPercent(skEntity, skillId);
        }

        public float GetNpcSkillCd(SkillSystem.SkEntity npcSkentiy, int SkillId)
        {
            return SkillSystem.SkInst.GetSkillCoolingPercent(npcSkentiy, SkillId);
        }

        public float GetCdByItemProtoId(int itemProtoId)
        {
            ItemAsset.ItemProto proto = ItemAsset.ItemProto.Mgr.Instance.Get(itemProtoId);

            return GetCdByItemProto(proto);
        }

        public float GetCdByItemProto(ItemAsset.ItemProto proto)
        {
            if (proto == null)
            {
                return 0f;
            }

            return GetCd(proto.skillId);
        }

        public float GetCdByItemInstanceId(int itemInstanceId)
        {
            ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(itemInstanceId);

            return GetCdByItemInstance(itemObj);
        }

        public float GetCdByItemInstance(ItemAsset.ItemObject itemObj)
        {
            if (itemObj == null)
            {
                return 0f;
            }

            return GetCd(itemObj.protoData.skillId);
        }
        #endregion

        //public class CdList
        //{
        //    class Item
        //    {
        //        int mSkillId;
        //        float mPrivateCd;
        //        float mSharedCd;

        //        public Item(int skillId, float privateCd, float sharedCd)
        //        {
        //            mSkillId = skillId;
        //            mPrivateCd = privateCd;
        //            mSharedCd = sharedCd;
        //        }

        //        public float cdTime
        //        {
        //            get
        //            {
        //                return mPrivateCd > mSharedCd ? mPrivateCd : mSharedCd;
        //            }
        //        }

        //        public void Update(float deltaTime)
        //        {
        //            mPrivateCd -= deltaTime;
        //            mSharedCd -= deltaTime;
        //        }
        //    }

        //    List<Item> mList = new List<Item>(1);

        //    int GetSkillId(int instanceId)
        //    {
        //        ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(instanceId);
        //        if (itemObj == null)
        //        {
        //            return 0;
        //        }

        //        return itemObj.protoData.skillId;
        //    }

        //    public void AddByItemInstanceId(int instanceId)
        //    {
        //        int skillId = GetSkillId(instanceId);

        //        if (skillId <= 0)
        //        {
        //            return;
        //        }

        //        Item item = new Item(skillId, 0f, 0f);

        //        mList.Add(item);
        //    }

        //    public float GetCdTime(int skillId)
        //    {

        //        return 0f;
        //    }

        //    public void Update()
        //    {
        //        mList.ForEach((item) =>
        //        {
        //            item.Update(Time.deltaTime);
        //        });

        //        mList.RemoveAll((item) =>
        //        {
        //            if (item.cdTime <= 0f)
        //            {
        //                return true;
        //            }

        //            return false;
        //        });
        //    }
        //}


        #region RIGHT CLICK

        public void RightMouseClickArmorItem(ItemAsset.ItemObject item)
        {
            GetComponent<WhiteCat.PlayerArmorCmpt>().QuickEquipArmorPartFromPackage(item);
        }

        #endregion
    }
}