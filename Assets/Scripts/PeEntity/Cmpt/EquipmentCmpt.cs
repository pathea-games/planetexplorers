using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SkillSystem;
using ItemAsset;
using Pathea.PeEntityExt;

namespace Pathea
{
	public class EquipmentCmpt : PeCmpt, IPeMsg //, IDroppableItemList //w for ragdoll bug, equipments are not droppable.
    {
        public class EventArg : PeEvent.EventArg
        {
			public bool isAdd;
			public ItemObject itemObj;
        }

        PeEvent.Event<EventArg> mEventor = new PeEvent.Event<EventArg>();

		public System.Action<List<SuitSetData.MatchData>> onSuitSetChange;

        public PeEvent.Event<EventArg> changeEventor
        {
            get
            {
                return mEventor;
            }
        }

        const int VersionID = 1;
		SkEntity mSkEntity;
		BiologyViewCmpt mViewCmpt;
		NpcCmpt mNPC;
		Motion_Equip mMotionEquip;

        [HideInInspector]
        public bool mShowModel;

		//ISkillTree mSkillTree;

		public void SetSkillBook(ISkillTree skillTree)
		{
			//mSkillTree = skillTree;
		}

        public interface Receiver
        {
            bool CanAddItemList(List<ItemObject> items);
            void AddItemList(List<ItemObject> items);
        }
        public Receiver mItemReciver;

        List<ItemObject> mItemList;
        List<PEEquipment> mEquipments;
		Dictionary<int, PEEquipmentLogic> mLogics;

		public List<ItemObject> _ItemList { get { return mItemList; } }

		private List<ItemObject> m_InitItems = new List<ItemObject>();

		private List<PEEquipment> _equipmentList{ get{ return mEquipments; } }
		
		List<IWeapon> m_RetList = new List<IWeapon>();

		public List<IWeapon> _Weapons
		{
			get
			{
				m_RetList.Clear();
				for(int i = 0; i < _equipmentList.Count; ++i)
				{
					IWeapon weapon = _equipmentList[i] as IWeapon;
					if(null != weapon && !weapon.Equals(null))
						m_RetList.Add(weapon);
				}
				return m_RetList;
			}
		}

		public bool handEmpty
		{
			get
			{
				for(int i = 0; i < _equipmentList.Count; ++i)
				{
					PEEquipment equip = _equipmentList[i];
					if(equip is IWeapon)
						return false;
					if(equip is PEHoldAbleEquipment)
						return false;
					if(equip is PEWaterPitcher)
						return false;
				}
				return true;
			}
		}

		public ItemObject mainHandEquipment{ get; set; }

        public EquipType mEquipType = EquipType.Null;

		Transform m_EquipmentLogicParent;

		bool m_HideEquipmentByFirstPerson = false;
		
		bool m_HideEquipmentByVehicle = false;

		bool m_HideEquipmentByRagdoll = false;

		List<ItemObject> m_TakeOffEquip;

		bool m_EquipmentDirty;

		List<int> m_EXBuffs = new List<int>();
		List<SuitSetData.MatchData> m_SuitSetMatchDatas = new List<SuitSetData.MatchData>();
		public List<SuitSetData.MatchData> matchDatas { get { return m_SuitSetMatchDatas; } }

//		List<int> m_EquipExBuffs = new List<int>();
//		lis

		public override void Awake ()
		{
			base.Awake ();
			mItemList = new List<ItemObject>();
            mEquipments = new List<PEEquipment>();
			mLogics = new Dictionary<int, PEEquipmentLogic>();
			m_TakeOffEquip = new List<ItemObject>();
			m_EquipmentLogicParent = (new GameObject("EquipmentLogic")).transform;
			m_EquipmentLogicParent.parent = transform;
		}

        public override void Start()
        {
			base.Start ();
			mSkEntity = Entity.skEntity;
            mViewCmpt = Entity.biologyViewCmpt;
			mNPC = Entity.NpcCmpt;
			mMotionEquip = Entity.motionEquipment;
			if(null != m_InitItems && m_InitItems.Count > 0)
			{
				for(int i = 0; i < m_InitItems.Count; ++i)
					PutOnEquipment(m_InitItems[i]);
				m_InitItems.Clear();
				Entity.lodCmpt.onConstruct += e=>e.StartCoroutine(PreLoad());
			}
//			StartCoroutine(InitEquipment());
        }

//		IEnumerator InitEquipment()
//		{
//			yield return new WaitForSeconds(1f);
//		}

		public override void OnUpdate ()
		{
			CheckEXBuffs();
		}

		public void AddInitEquipment(ItemObject itemObj)
		{
			m_InitItems.Add(itemObj);
		}

        public bool HasEquip(EquipType equipType)
        {
            return null != GetEquip(equipType);
        }

        public PEEquipment GetEquip(EquipType equipType)
        {
            return mEquipments.Find(itr => itr.equipType == equipType);
        }

		public bool IsEquipNow(int itemInstanceID)
		{
			for(int i = 0; i < _ItemList.Count; i++)
				if(_ItemList[i].instanceId == itemInstanceID)
					return true;
			return false;
		}

		void ReduceWeaponDurability(ItemObject itemObj)
		{
			if(null != itemObj)
			{
				if (GameConfig.IsMultiMode)
				{
					PlayerNetwork.mainPlayer.RequestWeaponDurability(Entity.Id, itemObj.instanceId);
				}
				else
				{
					ItemAsset.Equip equipCmpt = itemObj.GetCmpt<ItemAsset.Equip>();
					equipCmpt.ExpendAttackDurability(mSkEntity);
				}
			}
		}

		void ReduceArmorDurability(float damage, SkEntity caster)
		{
			if (GameConfig.IsMultiMode)
			{
				int[] equipIds = mItemList.Select(iter => null != iter ? iter.instanceId : -1).ToArray();
				PlayerNetwork.mainPlayer.RequestArmorDurability(Entity.Id, equipIds, damage, caster);
			}
			else
			{
				for(int i = 0; i < mItemList.Count; ++i)
				{
					ItemObject itemObj = mItemList[i];
					if(null != itemObj)
					{
						ItemAsset.Equip equipCmpt = itemObj.GetCmpt<ItemAsset.Equip>();
						equipCmpt.ExpendDefenceDurability(mSkEntity, damage);
					}
				}
			}
		}

		public bool NetTryPutOnEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null)
		{
			if(null != mViewCmpt && mViewCmpt.IsRagdoll)
				return false;
			Receiver currentReceiver = (null == receiver)?mItemReciver:receiver;			
			if(null == itemObj)
			{
				return false;
			}
			if(mItemList.Contains(itemObj))
				return false;
			
			ItemAsset.Equip equip = itemObj.GetCmpt<ItemAsset.Equip>();
			
			if (null == equip)
			{
				return false;
			}
			
			if (!Pathea.PeGender.IsMatch(equip.sex, Entity.ExtGetSex()))
			{
				return false;
			}
			SkillTreeUnitMgr learntSkills = Entity.GetCmpt<SkillTreeUnitMgr>();
			if(learntSkills != null && RandomMapConfig.useSkillTree)
			{
				if(!learntSkills.CheckEquipEnable(equip.protoData.itemClassId,equip.itemObj.level))
				{
					return false;
				}
			}

			m_TakeOffEquip.Clear();
			for(int i = 0; i < mItemList.Count; ++i)
			{
				ItemObject item = mItemList[i];
				if (item == itemObj)
					return false;
				ItemAsset.Equip existEquip = item.GetCmpt<ItemAsset.Equip>();
				if (null != existEquip)
				{
					if (System.Convert.ToBoolean(equip.equipPos & existEquip.equipPos))
					{
						m_TakeOffEquip.Add(item);
					}
				}
			}
			
			for(int i = 0; i < mEquipments.Count; ++i)
				if (m_TakeOffEquip.Contains(mEquipments[i].m_ItemObj) && !mEquipments[i].CanTakeOff())
					return false;
			
			if (null != currentReceiver)
			{
				if (!currentReceiver.CanAddItemList(m_TakeOffEquip))
					return false;
			}

			return true;
		}

		public event System.Action OnEquipmentChange;
		public bool PutOnEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null, bool netRequest = false)
        {
			if(!netRequest && null != mViewCmpt && mViewCmpt.IsRagdoll)
				return false;

			Receiver currentReceiver = (null == receiver)?mItemReciver:receiver;

            if(null == itemObj)
            {
                return false;
            }

            ItemAsset.Equip equip = itemObj.GetCmpt<ItemAsset.Equip>();

            if (null == equip)
            {
                return false;
            }

            if (!Pathea.PeGender.IsMatch(equip.sex, Entity.ExtGetSex()))
            {
                return false;
            }

			if(mItemList.Contains(itemObj))
				return false;
			SkillTreeUnitMgr learntSkills = Entity.GetCmpt<SkillTreeUnitMgr>();
			if(!netRequest && learntSkills != null &&  RandomMapConfig.useSkillTree)
			{
				if(!learntSkills.CheckEquipEnable(equip.protoData.itemClassId,equip.itemObj.level))
				{
					return false;
				}
			}
			m_TakeOffEquip.Clear();		
			for(int i = 0; i < mItemList.Count; ++i)
            {
				ItemObject item = mItemList[i];
                ItemAsset.Equip existEquip = item.GetCmpt<ItemAsset.Equip>();
                if (null != existEquip)
                {
					if (System.Convert.ToBoolean(equip.equipPos & existEquip.equipPos))
					{
                        m_TakeOffEquip.Add(item);
					}
                }
            }
			for(int i = 0; i < mEquipments.Count; ++i)
				if (m_TakeOffEquip.Contains(mEquipments[i].m_ItemObj) && !netRequest && !mEquipments[i].CanTakeOff())
					return false;

			if (!netRequest && addToReceiver && null != currentReceiver)
            {
				if (!currentReceiver.CanAddItemList(m_TakeOffEquip))
				{
                    //lz-2016.08.15 如果是NPC的背包满了要单独提示
                    if (currentReceiver is NpcPackageCmpt)
                    {
                        PeTipMsg.Register(PELocalization.GetString(82209013), PeTipMsg.EMsgLevel.Warning);
                    }
                    else
                    {
                        PeTipMsg.Register(PELocalization.GetString(82209001), PeTipMsg.EMsgLevel.Warning);
                    }
                    return false;
				}
            }

            for (int i = mItemList.Count - 1; i >= 0; i--)
                if (m_TakeOffEquip.Contains(mItemList[i] as ItemObject))
                    mItemList.RemoveAt(i);
            mItemList.Add(itemObj);

			if (addToReceiver && null != currentReceiver)
            {
				currentReceiver.AddItemList(m_TakeOffEquip);
            }
			
			//Do change			
			for (int i = 0; i < m_TakeOffEquip.Count; ++i)
			{
				ItemObject item = m_TakeOffEquip[i];
				RemoveItemEff(item);
				RemoveModel(item);
				mEquipType &= ~item.protoData.equipType;
				EventArg removeEvtArg = new EventArg();
				removeEvtArg.isAdd = false;
				removeEvtArg.itemObj = item;
				changeEventor.Dispatch(removeEvtArg, this);
			}

			ApplyItemEff(itemObj);
			AddModel(itemObj);
			mEquipType |= itemObj.protoData.equipType;

			EventArg evtArg = new EventArg();
			evtArg.isAdd = true;
			evtArg.itemObj = itemObj;
			changeEventor.Dispatch(evtArg, this);
			
			if(0 != (itemObj.protoData.equipPos & (1 << 4)))
				mainHandEquipment = itemObj;

			if (OnEquipmentChange != null)
				OnEquipmentChange();

            //lz-2016.08.22 引导检测玩家穿装备
            if (Entity.IsMainPlayer)
            {
                InGameAidData.CheckPutOnEquip(itemObj.protoId);
            }

			m_EquipmentDirty = true;

            return true;
        }

		public bool TryTakeOffEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null)
		{
			Receiver currentReceiver = (null == receiver)?mItemReciver:receiver;
			if (null == currentReceiver)
				return false;
			if (mItemList.Contains (itemObj)) 
			{
				for (int i = 0; i < mEquipments.Count; ++i)
					if (mEquipments[i].m_ItemObj == itemObj && !mEquipments[i].CanTakeOff ())
						return false;
				
				m_TakeOffEquip.Clear();
				m_TakeOffEquip.Add (itemObj);
				if (addToReceiver && !currentReceiver.CanAddItemList (m_TakeOffEquip))
					return false;
			}

			return true;
		}

		public bool TakeOffEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null, bool netRequest = false)
		{
			if(!netRequest && null != mViewCmpt && mViewCmpt.IsRagdoll)
				return false;
			Receiver currentReceiver = (null == receiver)?mItemReciver:receiver;
			if (null == currentReceiver)
                return false;
            if (mItemList.Contains(itemObj))
			{
				for (int i = 0; i < mEquipments.Count; ++i)
					if (mEquipments[i].m_ItemObj == itemObj && !netRequest && !mEquipments[i].CanTakeOff())
						return false;

				m_TakeOffEquip.Clear();
                m_TakeOffEquip.Add(itemObj);
				if (addToReceiver && !currentReceiver.CanAddItemList(m_TakeOffEquip))
				{
                    //lz-2016.08.15 如果是NPC的背包满了要单独提示
                    if (currentReceiver is NpcPackageCmpt)
                    {
                        PeTipMsg.Register(PELocalization.GetString(82209013), PeTipMsg.EMsgLevel.Warning);
                    }
                    else
                    {
                        PeTipMsg.Register(PELocalization.GetString(82209001), PeTipMsg.EMsgLevel.Warning);
                    }
                    return false;
				}
                mItemList.Remove(itemObj);
				if(addToReceiver)
					currentReceiver.AddItemList(m_TakeOffEquip);

				RemoveItemEff(itemObj);
				RemoveModel(itemObj);
				mEquipType &= ~itemObj.protoData.equipType;
				
				EventArg evtArg = new EventArg();
				evtArg.isAdd = false;
				evtArg.itemObj = itemObj;
				changeEventor.Dispatch(evtArg, this);
				
				if(0 != (itemObj.protoData.equipPos & (1 << 4)))
					mainHandEquipment = null;

				if (OnEquipmentChange != null)
					OnEquipmentChange();
				
				m_EquipmentDirty = true;
				
                return true;
            }
            return false;
        }

        void DestoryItemObj(int itemId)
        {
            ItemAsset.ItemMgr.Instance.DestroyItem(itemId);
        }

        public void ModelDestroy()
		{
			for (int i = 0; i < mEquipments.Count; ++i)
                if (null != mEquipments[i])
                    mEquipments[i].RemoveEquipment();
            mEquipments.Clear();
        }

		public void ApplyEquipment(ItemObject[] itemList)
		{
			int count = mItemList.Count;
			for (int i = 0; i < count; i++)
				TakeOffEquipment(mItemList[0]);
			for (int i = 0; i < itemList.Length; i++)
				PutOnEquipment(itemList[i], false);
		}

        void ApplyItemEff(ItemObject itemObj)
        {
			ItemAsset.Equip equip = itemObj.GetCmpt<ItemAsset.Equip>();
            if (null != equip) {
                if (mSkEntity == null)
                    mSkEntity = GetComponent<SkEntity>();
                equip.AddBuff(mSkEntity);
            }
        }

        void RemoveItemEff(ItemObject itemObj)
		{
			ItemAsset.Equip equip = itemObj.GetCmpt<ItemAsset.Equip>();
			if(null != equip)
				equip.RemoveBuff(mSkEntity);
        }

		bool ISAvatarModel(ItemObject itemObj)
		{
			return itemObj.protoData.equipReplacePos != 0;
		}

        void AddModel(ItemObject itemObj)
        {
			if (ISAvatarModel(itemObj))
                AddAvatarModel(itemObj.protoData.equipReplacePos, itemObj.protoData.resourcePath);
            AddEquipment(itemObj);
        }

        void AddAvatarModel(int partMask, string path)
        {
            mViewCmpt.AddPart(partMask, path);
        }
		
		void CreateLogic(ItemObject itemObj)
		{
			if(mLogics.ContainsKey(itemObj.instanceId))
				return;

			ItemAsset.Equip equip = itemObj.GetCmpt<ItemAsset.Equip>();
			
			if (null == equip)
				return;
			
			GameObject obj = equip.CreateLogicObj();
			
			if (null == obj)
				return;

			PEEquipmentLogic equ = obj.GetComponent<PEEquipmentLogic>();
			if (null == equ)
			{
				Debug.LogError("Equip can't find:" + itemObj.nameText);
				GameObject.Destroy(obj);
				return;
			}
			obj.transform.parent = m_EquipmentLogicParent;

			equ.InitEquipment(Entity, itemObj);

			mLogics[itemObj.instanceId] = equ;
		}

		void CreateModel(ItemObject itemObj)
		{
            if (mViewCmpt == null)
                mViewCmpt = Entity.biologyViewCmpt;
			if(null == mViewCmpt.modelTrans)
				return;
			ItemAsset.Equip equip = itemObj.GetCmpt<ItemAsset.Equip>();
			
			if (null == equip)
				return;
			
			GameObject obj = equip.CreateGameObj();
			
			if (null == obj)
				return;
			
			PEEquipment equ = obj.GetComponent<PEEquipment>();
			if (null == equ)
			{
				Debug.LogError("Equip can't find:" + itemObj.nameText);
				GameObject.Destroy(obj);
				return;
			}

			//IgnoreCollision
//			PETools.PEUtil.IgnoreCollision(obj, gameObject);
			equ.InitEquipment(Entity, itemObj);
			mEquipments.Add(equ);
			HideEquipmentByFirstPerson(equ, m_HideEquipmentByFirstPerson);
			HideEquipmentByVehicle(equ, m_HideEquipmentByVehicle);
			HidEquipmentByRagdoll(equ, m_HideEquipmentByRagdoll);
			mMotionEquip.SetEquipment(equ, true);
            PreLoadEquipmentEffect(equ);
        }


        /// <summary>
        /// lz-2017.12.29 为了优化新枪第一枪特效加载卡顿，因此在装备枪的时候就预加载粒子特效和音效
        /// </summary>
        /// <param name="equipment"></param>
        void PreLoadEquipmentEffect(PEEquipment equipment)
        {
            if (equipment is PEGun)
            {
                PEGun gun = equipment as PEGun;
                SkData skData = null;

                //音效
                SkData.s_SkillTbl.TryGetValue(gun.m_ShootSoundID, out skData);
                if (skData != null && skData._effMainOneTime != null && skData._effMainOneTime._seId > 0)
                {
                    int soundEffectID = skData._effMainOneTime._seId;
                    SoundAsset.SESoundBuff buff = SoundAsset.SESoundBuff.GetSESoundData(soundEffectID);
                    if (buff != null && AudioManager.instance != null)
                    {
                        AudioManager.instance.GetAudioClip(buff.mName);
                    }
                }

                skData = null;
                //粒子特效
                SkData.s_SkillTbl.TryGetValue(gun.GetSkillID(0), out skData);
                if (skData != null && skData._effMainOneTime != null && skData._effMainOneTime._effId != null && skData._effMainOneTime._effId.Length > 0)
                {
                    int[] effectArray = skData._effMainOneTime._effId;
                    for (int i = 0; i < effectArray.Length; i++)
                    {
                        int id = effectArray[i];
                        if (id > 0)
                        {
                            Effect.EffectData data = Effect.EffectData.GetEffCastData(id);
                            if (data != null && !string.IsNullOrEmpty(data.m_path) && Effect.EffectBuilder.Instance != null)
                            {
                                Effect.EffectBuilder.Instance.GetEffect(data.m_path);
                            }
                        }
                    }
                }
            }
        }

        void AddEquipment(ItemObject itemObj)
        {
            if (null == itemObj)
            {
                return;
            }

			CreateLogic(itemObj);
			
			CreateModel(itemObj);
        }

        void RemoveModel(ItemObject itemObj)
        {
            if (itemObj.protoData.equipReplacePos != 0)
                RemoveAvatarModel(itemObj.protoData.equipReplacePos);
            RemoveEquipModel(itemObj);
        }

        void RemoveAvatarModel(int partMask)
        {
            mViewCmpt.RemovePart(partMask);
        }

        void RemoveEquipModel(ItemObject itemObj)
		{
			if(null == itemObj) return;
			if(mLogics.ContainsKey(itemObj.instanceId))
			{
				if(null != mLogics[itemObj.instanceId])
					mLogics[itemObj.instanceId].RemoveEquipment();
				mLogics.Remove(itemObj.instanceId);
			}
			for (int i = mEquipments.Count - 1; i >= 0; --i)
            {
				PEEquipment equ = mEquipments[i];
				if(null == equ)
				{
					mEquipments.RemoveAt(i);
					continue;
				}
                if (equ.m_ItemObj == itemObj)
                {
					mMotionEquip.SetEquipment(equ, false);
                    mEquipments.Remove(equ);
                    equ.RemoveEquipment();
                    return;
                }
            }
        }
		
		void HideEquipmentByFirstPerson(PEEquipment equ, bool hide)
		{
			if(null != equ)
				equ.HideEquipmentByFirstPerson(hide);
		}

		void HideEquipmentByFirstPerson(bool hide)
		{			
			m_HideEquipmentByFirstPerson = hide;
			for (int i = 0; i < mEquipments.Count; ++i)
				HideEquipmentByFirstPerson(mEquipments[i], m_HideEquipmentByFirstPerson);
		}

		void HideEquipmentByVehicle(PEEquipment equ, bool hide)
		{
			if(null != equ)
				equ.HideEquipmentByVehicle(hide);
		}

		public void HideEquipmentByVehicle(bool hide)
		{
			m_HideEquipmentByVehicle = hide;
			for (int i = 0; i < mEquipments.Count; ++i)
				HideEquipmentByVehicle(mEquipments[i], hide);
		}

		void HidEquipmentByRagdoll(PEEquipment equ, bool hide)
		{
			if(null != equ)
				equ.HideEquipmentByVehicle(hide);
		}

		public void HidEquipmentByRagdoll(bool hide)
		{
			m_HideEquipmentByVehicle = hide;
			for (int i = 0; i < mEquipments.Count; ++i)
				HideEquipmentByVehicle(mEquipments[i], hide);
		}

        #region IPEComponent implementation

        public override void Serialize(BinaryWriter _out)
        {
            _out.Write(VersionID);
            _out.Write(mItemList.Count);

            foreach (ItemObject item in mItemList)
            {
                if (null != item)
                {
                    _out.Write((int)item.instanceId);
                }
            }
        }

        public override void Deserialize(BinaryReader _in)
        {
            /*int readVersion = */_in.ReadInt32();
            int count = _in.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                int id = _in.ReadInt32();
                ItemObject itemObj = ItemMgr.Instance.Get(id);
//                PutOnEquipment(itemObj);
				m_InitItems.Add(itemObj);
            }
        }

        #endregion

		//#region IDroppableItemList
		//int IDroppableItemList.Count{ 				get{ 			return mItemList.Count;													}	}
		//bool IDroppableItemList.IsReadOnly{ 		get{ 			return ((IList<ItemObject>)mItemList).IsReadOnly;						}	}
		//ItemSample IDroppableItemList.this[int i]{	get{ 			return mItemList[i] as ItemSample;										}	
		//											set{ 			mItemList[i] = value as ItemObject;										}	}
		//void IDroppableItemList.Clear(){							while(mItemList.Count > 0) TakeOffEquipment(mItemList[0], false);			}
		//bool IDroppableItemList.Remove(ItemSample v) {				TakeOffEquipment(v as ItemObject, false);									}		
		//void IDroppableItemList.RemoveAt(int index) {				TakeOffEquipment(mItemList[index], false);									}
		//int IDroppableItemList.IndexOf(ItemSample v){				return Array.IndexOf(mItemList, v as ItemObject);							}
		//bool IDroppableItemList.Contains(ItemSample v){				return Array.IndexOf(mItemList, v as ItemObject) != -1;						}
		//void IDroppableItemList.Add(ItemSample v){					throw new NotSupportedException(("NotSupported_IDroppableItemList"));		}
		//void IDroppableItemList.Insert(int index,ItemSample v){		throw new NotSupportedException(("NotSupported_IDroppableItemList"));		}
		//void IDroppableItemList.CopyTo(ItemSample[] a, int index){	throw new NotSupportedException(("NotSupported_IDroppableItemList"));		}
		//IEnumerator IDroppableItemList.GetEnumerator(){				return mItemList.GetEnumerator();											}
		//IEnumerator<ItemSample> IEnumerable<ItemSample>.GetEnumerator(){	return (IEnumerator<ItemSample>)mItemList.GetEnumerator();			}
		//#endregion
		#region IDroppableItemList
		public int DroppableItemCount{ get{ return mItemList.Count; } }
		public ItemSample GetDroppableItemAt(int idx)
		{
			return mItemList[idx];
		}
		public void AddDroppableItem(ItemSample item)
		{
			ItemObject it = item as ItemObject;
			if(it != null)
			{
				mItemList.Add(it);
			}
		}
		public void RemoveDroppableItem(ItemSample item)
		{
			ItemObject it = item as ItemObject;
			if(it != null)
			{
				TakeOffEquipment(it, false);
			}
		}
		public void RemoveDroppableItemAll()
		{
			int idx = 0;
			while(idx < mItemList.Count)
			{
				if(!TakeOffEquipment(mItemList[idx], false))
				{
					idx++;
				}
			}
		}
		#endregion

        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
            case EMsg.View_Prefab_Build:
				mShowModel = true;
				ResetModels();
                break;

            case EMsg.View_Prefab_Destroy:
                mShowModel = false;
                ModelDestroy();
				break;
			case EMsg.Battle_EquipAttack:
				if(Entity != PeCreature.Instance.mainPlayer && null != mNPC && !mNPC.HasConsume)
					return;
				if(Entity.proto == EEntityProto.Monster && Entity.Race == ERace.Mankind)
					return;
				if(PeGameMgr.IsBuild)
					return;
				ReduceWeaponDurability((ItemObject)args[0]);
				break;
			case EMsg.Battle_BeAttacked:
				if(Entity != PeCreature.Instance.mainPlayer && null != mNPC && !mNPC.HasConsume)
					return;
				if(Entity.proto == EEntityProto.Monster && Entity.Race == ERace.Mankind)
					return;
				if(PeGameMgr.IsBuild)
					return;
				ReduceArmorDurability((float)args[0], (SkEntity)args[1]);
				break;
			case EMsg.View_FirstPerson:
				HideEquipmentByFirstPerson((bool)args[0]);
				break;
            }
        }

		public void DestroyAllEquipment()
		{
			for(int i =0; i < mItemList.Count; i++)
			{
				RemoveItemEff(mItemList[i]);
				RemoveModel(mItemList[i]);
				mEquipType &= ~mItemList[i].protoData.equipType;
				ItemMgr.Instance.DestroyItem(mItemList[i].instanceId);
			}
			mItemList.Clear();
		}

		public void ResetModels()
		{
			ModelDestroy();
			foreach (ItemObject itemObj in mItemList)
				if(!ISAvatarModel(itemObj))
					CreateModel(itemObj);
			foreach(PEEquipmentLogic logic in mLogics.Values)
				if(null != logic)
					logic.OnModelRebuild();
		}

		public void CheckEXBuffs()
		{
			if(m_EquipmentDirty)
			{
				m_EquipmentDirty = false;

				if(null == mSkEntity) return;

				if(null != m_EXBuffs)
					for(int i = 0; i < m_EXBuffs.Count; ++i)
						mSkEntity.CancelBuffById(m_EXBuffs[i]);

				m_EXBuffs.Clear();
				m_SuitSetMatchDatas.Clear();

				EquipSetData.GetSuitSetEffect(_ItemList, ref m_EXBuffs);
				SuitSetData.GetSuitSetEffect(_ItemList, ref m_EXBuffs, ref m_SuitSetMatchDatas);
				
				for(int i = 0; i < m_EXBuffs.Count; ++i)
					SkEntity.MountBuff(mSkEntity, m_EXBuffs[i], null, null);

				if(null != onSuitSetChange)
					onSuitSetChange(m_SuitSetMatchDatas);
			}
		}

		IEnumerator PreLoad()
		{
			if (mItemList != null) {
				//Object asset;
				for (int i = 0; i < mItemList.Count; i++) {	// using foreach may cause exception when mItemList changed out of this method
					ItemObject itemObj = mItemList [i];
					if (!ISAvatarModel (itemObj)) {
						ItemAsset.Equip equip = itemObj.GetCmpt<ItemAsset.Equip> ();
						if (null != equip) {
							AssetsLoader.Instance.AddReq (new AssetReq (equip.protoData.resourcePath));
							yield return new WaitForSeconds (0.2f);
						}
					}
				}
			}
		}
    }
}