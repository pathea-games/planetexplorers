using UnityEngine;
using Pathea;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;

namespace ItemAsset
{
	public class SelectItem 
	{
		static EquipInfo EInfo = new EquipInfo(); 
		public static bool IsEquipItemObject(ItemAsset.ItemObject obj)
		{
			if(obj == null)
				return false;

			return SwichEquipInfo(obj) != null;
		}

		public  static EquipInfo SwichEquipInfo(ItemAsset.ItemObject obj)
		{
			if(obj == null)
				return null;
		

			switch(obj.protoData.id)
			{
			//手榴弹处理
			case  60:EInfo.SetEquipInfo(EequipEditorType.gun,EeqSelect.combat);
				     return EInfo;
			//能源盾
			case 167:
			case 168:
			case 169:
			case 170:
				EInfo.SetEquipInfo(EequipEditorType.energy_sheild,EeqSelect.energy_sheild);
				return EInfo;
			//电池
			case 228:EInfo.SetEquipInfo(EequipEditorType.battery,EeqSelect.energy);
				     return EInfo;

			default:
				break;
			}

			EequipEditorType type = (EequipEditorType)obj.protoData.editorTypeId;
			switch(type)
			{
			case EequipEditorType.sword:
				 EInfo.SetEquipInfo(type,EeqSelect.combat);
				 break;
			case EequipEditorType.axe:
				EInfo.SetEquipInfo(type,EeqSelect.tool);
				break;
			case EequipEditorType.bow:
				 EInfo.SetEquipInfo(type,EeqSelect.combat);
				break;
			case EequipEditorType.gun:
				 EInfo.SetEquipInfo(type,EeqSelect.combat);
				break;
			case EequipEditorType.shield:
				 EInfo.SetEquipInfo(type,EeqSelect.protect);
				break;
			default:
				return null;
			}

			return EInfo;

		}

		public static EeqSelect GetEquipSelect(ItemAsset.ItemObject obj)
		{
			EquipInfo info = SwichEquipInfo(obj);

			return info == null ? EeqSelect.None : info._selectType;
		}

        //public static bool ChangeEquip(Pathea.PeEntity entity, EeqSelect select)
        //{
        //    List<ItemObject> objs = entity.GetEquipObjs(select);
        //    if (objs.Count > 0 && GameUI.Instance.mServantWndCtrl.ServantIsNotNull)
        //    {
        //        EquipmentCmpt.Receiver receiver = entity.packageCmpt;
        //        if (GameUI.Instance.mServantWndCtrl.EquipItem(objs[0], receiver))
        //        {
        //            return entity.RemoveFromBag(objs[0]);
        //        }
        //    }
        //    return false;
        //}

		public static bool EquipByObj(Pathea.PeEntity entity,ItemObject obj)
		{
			if(obj == null || entity.equipmentCmpt == null)
				return false;

			EquipmentCmpt.Receiver receiver = entity.packageCmpt;
			if (GameConfig.IsMultiMode)
			{
				if(entity.equipmentCmpt.NetTryPutOnEquipment(obj,true,receiver))
				{
                    entity.netCmpt.RequestUseItem(obj.instanceId);
                    //entity.RemoveFromBag(obj);
                    return true;
				}
			}
			else
			{
				if(entity.equipmentCmpt.PutOnEquipment(obj,true,receiver))
				{
                    if (SelectItem_N.Instance.ItemObj != null && SelectItem_N.Instance.ItemObj.Equals(obj))
                        SelectItem_N.Instance.SetItem(null);

					entity.RemoveFromBag(obj);
					return true;
				}
			}
			return false;
		}
		
        public static bool TakeOffEquip(Pathea.PeEntity entity)
        {
            if (entity == null || entity.motionEquipment == null || entity.motionEquipment.PEHoldAbleEqObj == null)
                return false;

			if(entity.equipmentCmpt == null)
				return false;

            EquipmentCmpt.Receiver receiver = entity.packageCmpt;
			if (GameConfig.IsMultiMode)
			{
				if (entity.equipmentCmpt.TryTakeOffEquipment(entity.motionEquipment.PEHoldAbleEqObj,true,receiver))
                {
                    PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(entity.Id, entity.motionEquipment.PEHoldAbleEqObj.instanceId, -1);
                    return true;
                }
                  
            }
			else
			{
				return entity.equipmentCmpt.TakeOffEquipment(entity.motionEquipment.PEHoldAbleEqObj,true,receiver);
			}
			return false;
        }

		public static bool EquipCanAttack(PeEntity npc,ItemObject obj)
		{
			if(obj == null)
				return false;

			if(obj.protoData.weaponInfo == null)
				return false;

			if(obj.protoData.weaponInfo.attackModes == null)
				return false;

            if (obj.protoData.weaponInfo.attackModes[0].damage <= PETools.PEMath.Epsilon)
				return false;

			if(npc.NpcCmpt == null)
				return false;

			if(!npc.NpcCmpt.HasConsume)
				return true;

            //bow can not use in water
            bool npcFeetInwater =  npc.biologyViewCmpt != null &&  npc.biologyViewCmpt.monoPhyCtrl != null ? npc.biologyViewCmpt.monoPhyCtrl.feetInWater : false;
            if (npcFeetInwater && obj.protoData.weaponInfo.costItem == SupplyNum.ARROW)
                return false;

			Durability durability = obj.GetCmpt<ItemAsset.Durability>();
			if(durability != null && durability.floatValue.current < PETools.PEMath.Epsilon)
				return false;


			if(obj.protoData.weaponInfo.useEnergry)
			{
				if(npc.GetAttribute(AttribType.Energy) > 10.0f)
					return true;

				Energy enery = obj.GetCmpt<Energy>();
				bool objHasEnergy = enery != null && enery.floatValue.current > 10.0f;
				if(objHasEnergy)
					return true;

				GunAmmo ammo = obj.GetCmpt<GunAmmo>();
				bool hasAmmo = ammo != null && ammo.count >5;
				if(hasAmmo)
					return true;


				List<ItemAsset.ItemObject> objs = npc.GetEquipObjs(EeqSelect.energy);
				for(int i=0;i<objs.Count;i++)
				{
					Energy enery0 = objs[i].GetCmpt<Energy>();
					if(enery0 != null && enery0.floatValue.current > 10.0f)
					{
						return true;
					}
				}

				return  false;
			}

			if(obj.protoData.weaponInfo.costItem >0)
			{
				GunAmmo ammo = obj.GetCmpt<GunAmmo>();
				 
				return  (ammo != null && ammo.count >0) || npc.GetItemCount(obj.protoData.weaponInfo.costItem) > 0;
			}

			return true;
	
		}

		public static bool EqToolCanUse(PeEntity npc,ItemObject obj)
		{

			if(obj.protoId == 1469)
			{
				if(npc.GetAttribute(AttribType.Energy) < PETools.PEMath.Epsilon)
				{
					return false;
				}
			}
			return true;

		}

		public static bool EqEnergySheildCanUse(PeEntity npc,ItemObject obj)
		{
			ItemAsset.Equip equip = obj.GetCmpt<ItemAsset.Equip>();
			if (null == equip)
			{
				return false;
			}

			if (!Pathea.PeGender.IsMatch(equip.sex,npc.ExtGetSex()))
			{
				return false;
			}
			return true;
		}

        public static bool HasCanAttackEquip(PeEntity entity, AttackType type)
        {
            List<ItemObject> objs = entity.GetEquipObjs(EeqSelect.combat);
            if (objs == null) return false; //is player
            for (int i = 0; i < objs.Count; i++)
            {
                if (SelectItem.EquipCanAttack(entity, objs[i]))
                {
                    AttackMode mode = objs[i].protoData.weaponInfo.attackModes[0];
                    if (mode.type == type)
                        return true;
                }

            }

            ItemObject curobj = entity.motionEquipment.PEHoldAbleEqObj;
            EeqSelect curEq = SelectItem.GetEquipSelect(curobj);

            bool IsNeedTakeOff = (curobj != null && curobj.protoData.weaponInfo != null && curobj.protoData.weaponInfo.attackModes[0].type == AttackType.Ranged) ? true : false;
            bool canAttack = SelectItem.EquipCanAttack(entity, curobj);
            if (IsNeedTakeOff && !canAttack)
                TakeOffEquip(entity);

            if (curEq == EeqSelect.combat && canAttack)
            {
                AttackMode mode = curobj.protoData.weaponInfo.attackModes[0];
                if (mode.type == type)
                    return true;
            }

            return false;

        }

		public static bool HasCanUseEquip(PeEntity entity,EeqSelect selcet)
		{
			List<ItemObject> objs = entity.GetEquipObjs(selcet);
			 
			return objs != null && objs.Count >0 ? true : false;
		}

		public static bool HasCanEquip(PeEntity entity,EeqSelect selcet,AttackType type)
		{
			switch(selcet)
			{
			case EeqSelect.combat:       return HasCanAttackEquip(entity,type);
			case EeqSelect.protect:      return HasCanUseEquip(entity,selcet);
			case EeqSelect.energy:       return HasCanUseEquip(entity,selcet);
			case EeqSelect.energy_sheild:return HasCanUseEquip(entity,selcet);
			case EeqSelect.tool:         return HasCanUseEquip(entity,selcet);
			default:return false;
			}
		}

        /***************************************
        *匹配是否有合适的武器
        *************************************/    
        public static bool MatchEnemyEquip(PeEntity npc ,PeEntity target)
        {
            if (npc == null || target == null)
                return false;

			//外星人NPC不做检测，都默认为可以攻击
			if(npc.IsMotorNpc)
				return true;

			if(target.IsBoss)
				return HasCanEquip(npc,EeqSelect.combat,AttackType.Ranged);

            MovementField mFeild = target.Field;
            switch (mFeild)
            {
                case MovementField.Land:
                    return true;
                case MovementField.Sky:
                    return HasCanEquip(npc,EeqSelect.combat,AttackType.Ranged);
                case MovementField.water:
                    return true;
                case MovementField.Amphibian:
                    return true;
                default:
                    break;
            }
            return false;
        }

        /***************************************
         * 匹配是否可以攻击怪物
         * 
         * 
         *************************************/

		public static bool MatchEnemyAttack(PeEntity npc ,PeEntity target)
		{
			if (npc == null || target == null)
				return false;

			//NPC 不攻击 ragdoll状态怪物
			if(target.isRagdoll)
				return false;

			//外星人NPC不做检测，都默认为可以攻击
			if(npc.IsMotorNpc)
				return true;

			bool hasRangeEquip = HasCanEquip(npc,EeqSelect.combat,AttackType.Ranged);
			//bool hasCloseEquip = HasCanEquip(npc,EeqSelect.combat,AttackType.Melee);
			bool isBoss = target.IsBoss;

			float hpNpc  = npc.GetAttribute(AttribType.Hp);
			float hpNpcMax  = npc.GetAttribute(AttribType.HpMax);
			//float atkNpc = npc.GetAttribute(AttribType.Atk);
			//float defNpc = npc.GetAttribute(AttribType.Def);
			//float enemyAtk = target.GetAttribute(AttribType.Atk);
            bool  hpEnough = hpNpc > hpNpcMax * NPCConstNum.ATK_MIN_HP;//enemyAtk - defNpc < hpNpc * 0.3f;

			MovementField mFeild = target.Field;
			switch (mFeild)
			{
				case MovementField.Land:
                case MovementField.water:
				{
					if(hasRangeEquip) return true;
					if(isBoss)        return false;  

					return hpEnough;
				}
				case MovementField.Sky:
				{
					return hasRangeEquip;
				}
				default:
					break;
			}
			return true;
		}

	}
	
	public class EquipSelect
	{
		List<ItemObject> _hasSelectsAtkEq;
		List<ItemObject> _hasSelectsDefEq;
		List<ItemObject> _hasSelectsToolEq;
		List<ItemObject> _hasSelectsEnergyEq;
		List<ItemObject> _hasSelectsEnergy_SheildEq;

		List<EvaluateInfo> _hasSelectsAtkInfo;

		ItemObject _BetterAtkObj;
		public ItemObject BetterAtkObj { get {return _BetterAtkObj;}}
		List<ItemObject> _tempList = new List<ItemObject>();

		public EquipSelect()
		{
			_hasSelectsAtkEq = new List<ItemObject>();
			_hasSelectsDefEq = new List<ItemObject>();
			_hasSelectsToolEq = new List<ItemObject>();
			_hasSelectsEnergyEq = new List<ItemObject>();
			_hasSelectsEnergy_SheildEq = new List<ItemObject>();
		}


		public ItemObject GetBetterObj(PeEntity npc,Enemy enmey,EeqSelect select)
		{
			switch(select)
			{
			case EeqSelect.combat:
				return GetBetterAtkObj(npc,enmey);
			case EeqSelect.protect:
				return GetBetterDefObj();
			case EeqSelect.tool:
				return GetBetterToolObj();
			default:
				return null;
			}
		}

		public bool SetSelect(PeEntity npc,EeqSelect select)
		{
			switch(select)
			{
//			case AttackSelect.Melee:
//				return SetSelectObjsAtk(npc,select);
			case EeqSelect.combat:
				return SetSelectObjsAtk(npc,select);
			case EeqSelect.protect:
				return SetSelectObjsDef(npc,select);
			case EeqSelect.tool:
				return SetSelectObjsTool(npc,select);
			default:
				return false;
			}
		}


		public void ClearSelect()
		{
			_BetterAtkObj = null;
		}

		#region Atk fun
		public bool AddSeclectObjAtk(ItemObject obj)
		{
			if(obj == null || obj.protoData.weaponInfo == null)
				return false;

			_hasSelectsAtkEq.Add(obj);
			return true;
		}

		public void  ClearAtkSelects()
		{
			_hasSelectsAtkEq.Clear();
		}

		public bool AddSelectObjsAtk(PeEntity npc,List<ItemObject> objs)
		{
			if(objs == null)
				return false;

			for(int i=0;i<objs.Count;i++)
			{
				if(SelectItem.EquipCanAttack(npc,objs[i]))
					_hasSelectsAtkEq.Add(objs[i]);
			}

			return true;
		}

		public bool SetSelectObjsAtk(PeEntity npc,EeqSelect selcet)
		{
			AddSelectObjsAtk(npc,npc.GetEquipObjs(selcet));

			ItemObject curobj = npc.motionEquipment.PEHoldAbleEqObj;
			EeqSelect curEq = SelectItem.GetEquipSelect(curobj);

            bool IsNeedTakeOff = (curobj != null && curobj.protoData.weaponInfo != null && curobj.protoData.weaponInfo.attackModes[0].type == AttackType.Ranged) ? true : false;
			bool canAttack = SelectItem.EquipCanAttack(npc,curobj);
			if(IsNeedTakeOff && !canAttack)
				SelectItem.TakeOffEquip(npc);

			if(curEq == selcet && canAttack)
				AddSeclectObjAtk(curobj);

			return _hasSelectsAtkEq.Count >0;
		}

		List<ItemObject> FilterAtkObjs(AttackType type,List<ItemObject> objs)
		{
			if(objs == null)
				return null;

			_tempList.AddRange(objs);
			for(int i=0;i<_tempList.Count;i++)
			{
				if(_tempList[i].protoData.weaponInfo != null && _tempList[i].protoData.weaponInfo.attackModes[0].type != type)
					objs.Remove(_tempList[i]);
			}
			_tempList.Clear();
			return objs;
		}

		public bool SetSelectObjsAtk(PeEntity npc,AttackType type)
		{
			AddSelectObjsAtk(npc,FilterAtkObjs(type,npc.GetEquipObjs(EeqSelect.combat)));

			ItemObject curobj = npc.motionEquipment.PEHoldAbleEqObj;
			EeqSelect curEq = SelectItem.GetEquipSelect(curobj);

            bool IsNeedTakeOff = (curobj != null && curobj.protoData.weaponInfo != null && curobj.protoData.weaponInfo.attackModes[0].type == AttackType.Ranged) ? true : false;
			bool canAttack = SelectItem.EquipCanAttack(npc,curobj);
			if(IsNeedTakeOff && !canAttack)
				SelectItem.TakeOffEquip(npc);

			if(curEq == EeqSelect.combat && curobj.protoData.weaponInfo.attackModes[0].type == type && canAttack)
				AddSeclectObjAtk(curobj);

			return _hasSelectsAtkEq.Count >0;
		}

		public ItemObject GetBetterAtkObj(PeEntity npc,Enemy enemy)
		{
			List<EvaluateInfo> infos = EvaluateMgr.Evaluates(npc,enemy,_hasSelectsAtkEq);
			if(infos == null || infos.Count <= 0)
			{
				//在水中需要收回当前装备的武器（如果远程 INFOS可能是0个，此时应用拳头）
				if(npc.biologyViewCmpt != null && !npc.biologyViewCmpt.monoPhyCtrl.Equals(null) && npc.biologyViewCmpt.monoPhyCtrl.feetInWater)
					SelectItem.TakeOffEquip(npc);

				return null;
			}
			;
			if(infos.Count == 1)
			{
				_BetterAtkObj = infos[0].ItemObj;
				return _BetterAtkObj;
			}

			EvaluateInfo tmpInfo = infos[0];
			for(int i=1;i<infos.Count;i++)
			{
				if(tmpInfo.EvaluateValue < infos[i].EvaluateValue)
					tmpInfo = infos[i];

				EvaluateMgr.Recyle(infos[i]);
			}

			_BetterAtkObj = tmpInfo.ItemObj;
			return _BetterAtkObj;
		}
		#endregion

		#region Def Fun
		public void ClearDefSelects()
		{
			_hasSelectsDefEq.Clear();
		}

		public bool AddSeclectObjDef(ItemObject obj)
		{
			if(obj == null)
				return false;
			
			_hasSelectsDefEq.Add(obj);
			return true;
		}


		public bool SetSelectObjsDef(List<ItemObject> objs)
		{
			if(objs == null)
				return false;
			
			ClearDefSelects();
			_hasSelectsDefEq.AddRange(objs);
			return true;
		}

		public bool SetSelectObjsDef(PeEntity npc,EeqSelect selcet)
		{
			//ItemObject curobj;
			SetSelectObjsDef(npc.GetEquipObjs(selcet));
			if(npc.motionEquipment.sheild != null && npc.motionEquipment.sheild.m_ItemObj != null)
			{
				AddSeclectObjDef(npc.motionEquipment.sheild.m_ItemObj);
			}

			return _hasSelectsDefEq.Count > 0;
		}

		public ItemObject GetBetterDefObj()
		{
			//找到最优防御武器
			
			if(_hasSelectsDefEq == null || _hasSelectsDefEq.Count <=0 )
				return null;
			
			if(_hasSelectsDefEq.Count == 1)
				return _hasSelectsDefEq[0];
			
			ItemObject tempObj = _hasSelectsDefEq[0];
			for(int i=1;i<_hasSelectsDefEq.Count;i++)
			{
				if(_hasSelectsDefEq[i].protoData.propertyList.GetProperty(AttribType.Def) > tempObj.protoData.propertyList.GetProperty(AttribType.Def))
					tempObj = _hasSelectsDefEq[i];
			}
			
			return tempObj ;
		}
		#endregion

		#region Tool Fun
		public void  ClearToolSelects()
		{
			_hasSelectsToolEq.Clear();
		}

		bool SetSelectObjsTool(PeEntity npc,List<ItemObject> objs)
		{
			if(objs == null)
				return false;
			
			ClearToolSelects();
			for(int i=0;i<objs.Count;i++)
			{
				if(ItemAsset.SelectItem.EqToolCanUse(npc,objs[i]))
					AddSelectToolObj(objs[i]);
			}
			return true;
		}

		public void AddSelectToolObj(ItemObject obj)
		{
			if(obj == null)
				return ;

			_hasSelectsToolEq.Add(obj);
		}
		
		public bool SetSelectObjsTool(PeEntity npc,EeqSelect selcet)
		{
			SetSelectObjsTool(npc,npc.GetEquipObjs(selcet));
			if(npc.motionEquipment.axe != null && npc.motionEquipment.axe.m_ItemObj != null)
			{
				AddSelectToolObj(npc.motionEquipment.axe.m_ItemObj);
			}
			return _hasSelectsToolEq.Count > 0;
		}
		
		public ItemObject GetBetterToolObj()
		{
			//找到最优工具
			
			if(_hasSelectsToolEq == null || _hasSelectsToolEq.Count <=0 )
				return null;
			
			if(_hasSelectsToolEq.Count == 1)
				return _hasSelectsToolEq[0];
			
			ItemObject tempObj = _hasSelectsToolEq[0];
			for(int i=1;i<_hasSelectsToolEq.Count;i++)
			{
				if(_hasSelectsToolEq[i].protoData.propertyList.GetProperty(AttribType.Atk) > tempObj.protoData.propertyList.GetProperty(AttribType.Atk))
					tempObj = _hasSelectsToolEq[i];
			}
			
			return tempObj ;
		}





		#endregion

		#region energy Fun

		public bool SetSelectObjsEnergy(PeEntity npc,EeqSelect selcet)
		{
			if(npc.GetAttribute(AttribType.Energy) > PETools.PEMath.Epsilon)
				return false;

			SetSelectObjsEnergy(npc.GetEquipObjs(selcet));
			return _hasSelectsEnergyEq.Count >0;
		}


		public ItemObject GetBetterEnergyObj()
		{
			if(_hasSelectsEnergyEq == null || _hasSelectsEnergyEq.Count <=0)
				return null;

			if(_hasSelectsEnergyEq.Count == 1)
				return _hasSelectsEnergyEq[0];

			ItemObject tempObj = _hasSelectsEnergyEq[0];
			for(int i=1;i<_hasSelectsEnergyEq.Count;i++)
			{
				tempObj = CompearEnery(_hasSelectsEnergyEq[i],tempObj);	 
			}
			return tempObj;
		}

		private void SetSelectObjsEnergy(List<ItemObject> objs)
		{
			if(objs == null)
				return;

			_hasSelectsEnergyEq.Clear();
			for(int i=0;i<objs.Count;i++)
			{
				if(EnergyObjCanUse(objs[i]))
					_hasSelectsEnergyEq.Add(objs[i]);
			}

		}

		public bool EnergyObjCanUse(ItemObject obj)
		{
			if(obj == null )
				return false;

			Energy enery = obj.GetCmpt<Energy>();
			return  enery != null && enery.floatValue.current > PETools.PEMath.Epsilon ;
		}

		private ItemObject CompearEnery(ItemObject obj0,ItemObject obj1)
		{
			Energy enery0 = obj0.GetCmpt<Energy>();
			Energy enery1 = obj1.GetCmpt<Energy>();
			if(enery0.floatValue.current > enery1.floatValue.current)
				return obj0;
			else
				return obj1;
		}


		#endregion

		#region energy_sheild Fun
		public bool SetSelectObjsEnergySheild(PeEntity npc,EeqSelect selcet)
		{
			if(npc.motionEquipment == null || npc.motionEquipment.energySheild != null)
				return false;

			setSelectObjEnergySheild(npc,npc.GetEquipObjs(selcet));
//			if( && npc.motionEquipment.energySheild.itemObject != null)
//				AddEnergySheildObj(npc.motionEquipment.energySheild.itemObject);

			return _hasSelectsEnergy_SheildEq.Count >0;
		}


		public void AddEnergySheildObj(PeEntity npc,ItemObject obj)
		{
			_hasSelectsEnergy_SheildEq.Add(obj);
		}

		private void setSelectObjEnergySheild(PeEntity npc,List<ItemObject> objs)
		{
			if(objs == null)
				return ;

			_hasSelectsEnergy_SheildEq.Clear();
			for(int i=0;i<objs.Count;i++)
			{
				if(ItemAsset.SelectItem.EqEnergySheildCanUse(npc,objs[i]))
					_hasSelectsEnergy_SheildEq.Add(objs[i]);
			}
			return ;
		}

		public ItemObject GetBetterEnergySheild()
		{
			if(_hasSelectsEnergy_SheildEq == null)
				return null;

			if(_hasSelectsEnergy_SheildEq.Count == 1)
				return _hasSelectsEnergy_SheildEq[0];

			return _hasSelectsEnergy_SheildEq[Random.Range(0,_hasSelectsEnergy_SheildEq.Count)];
		}


		#endregion

	}


    /**************************************************
     * 评估信息：最终以_EvaluateValue 决定评估
     * 
     *  */
    public class EvaluateInfo
	{
		float _DPS;
		float  _SurplusCnt = 1.0f;
		float _IsEquipment;
		float _InInRange;
		float _LongRange = 1.0f;
		float _rangeFir = 1.0f;
		ItemObject _ItemObj;

		public ItemObject ItemObj { get { return _ItemObj;} }
		float  _EvaluateValue;
		public float  EvaluateValue
		{
			get
			{
				_EvaluateValue = _DPS *_IsEquipment * _InInRange * _SurplusCnt * _LongRange * _rangeFir;
				return _EvaluateValue;
			}
		}

		public void SetRangeFir(float value)
		{
			_rangeFir = value;
		}

		public void SetLongRange(float value)
		{
			_LongRange = value;
		}

		public void setEquipment(float value)
		{
			_IsEquipment = value;
		}

		public void SetRangeValue(float value)
		{
			_InInRange = value;
		}

		public void SetDPS(float dps)
		{
			_DPS = dps;
		}

		public void SetSurplusCnt(float value)
		{
			_SurplusCnt = value;
		}

		public void SetObj(ItemObject Obj)
		{
			_ItemObj = Obj;
		}

	}

	public class EvaluateMgr
	{
		static List<EvaluateInfo> infos = new List<EvaluateInfo>();
	
		static Stack<EvaluateInfo> infoPool = new Stack<EvaluateInfo>();
		
		static EvaluateInfo GetEvaluateInfo()
		{
			EvaluateInfo retInfo = null;
			if(infoPool.Count > 0)
				retInfo = infoPool.Pop();
			if(null == retInfo)
				retInfo = new EvaluateInfo();
			return retInfo;
		}
		
		public  static void Recyle(EvaluateInfo info)
		{
			infoPool.Push(info);
		}

		public static EvaluateInfo  Evaluate(PeEntity npc,Enemy enemy,ItemObject obj)
		{
			float RANGE_DATA = 5.0f;
			ItemProto.WeaponInfo weaponInfo = obj.protoData.weaponInfo;
			AttackMode mode = obj.protoData.weaponInfo.attackModes[0];

			EvaluateInfo info = new EvaluateInfo();
			//当前装备
			if(npc.motionEquipment.PEHoldAbleEqObj == obj)
				info.setEquipment(1.2f);
			else
				info.setEquipment(1.0f);

			if(mode.type == AttackType.Ranged)
				info.SetRangeFir(2.0f);
			else
				info.SetRangeFir(1.0f);

			//bool npcFeetInwater =  npc.biologyViewCmpt != null &&  npc.biologyViewCmpt.monoPhyCtrl != null ? npc.biologyViewCmpt.monoPhyCtrl.feetInWater : false;
			float hpNpc  = npc.GetAttribute(AttribType.Hp);
			//float atkNpc = npc.GetAttribute(AttribType.Atk);
			float defNpc = npc.GetAttribute(AttribType.Def);
			float enemyAtk = enemy.entityTarget.GetAttribute(AttribType.Atk);

            bool rangeFrist = ((enemyAtk - defNpc > hpNpc * 0.1f) && mode.type == AttackType.Ranged) ? true : false; // && !enemy.IsInWater && !npcFeetInwater

			//飞行怪物或者boss 只能远程攻击
            if (enemy.entityTarget.Field == MovementField.Sky || enemy.entityTarget.IsBoss || rangeFrist)
            {
                if (mode.type == AttackType.Ranged)
                    info.SetLongRange(2.0f);
                else
                    info.SetLongRange(0);
            }
            //}//水中 只能用近战
            //else if(npcFeetInwater)
            //{
            //    if(mode.type == AttackType.Melee)
            //        info.SetLongRange(2.0f);
            //    else
            //        info.SetLongRange(0);
            //}
            else
                info.SetLongRange(1.0f);

			//
			float distance = PETools.PEUtil.MagnitudeH(npc.position,enemy.position);
			if(mode.IsInRange(distance))
			{
				info.SetRangeValue(2.0f);
			}
			else
			{
				if(distance < mode.minRange && distance + RANGE_DATA > mode.minRange)
				{
					info.SetRangeValue(1.0f + Mathf.Clamp01(distance/mode.minRange));
				} else if(distance > mode.maxRange && mode.maxRange + RANGE_DATA > distance)
				{
					info.SetRangeValue(1.0f + Mathf.Clamp01(RANGE_DATA - (distance - mode.maxRange)/RANGE_DATA));
				}else
				{
					if(mode.type == AttackType.Ranged)
					{
						info.SetRangeValue(1.8f);
					}
					else
					{
						info.SetRangeValue(1.0f);
					}

				}
			}
			//
			info.SetDPS(mode.damage/(mode.frequency < PETools.PEMath.Epsilon ? 1.0f : mode.frequency));

			if(weaponInfo.useEnergry)
			{
				GunAmmo gunAmmo = obj.GetCmpt<GunAmmo>();
				if(gunAmmo != null)
				{
					int shootCount = gunAmmo.count / Mathf.Max(weaponInfo.costPerShoot, 1);
					//float energy = npc.GetAttribute(AttribType.Energy);
					if(shootCount > 3)						
						info.SetSurplusCnt(1.0f);
					else if(shootCount >= 1 || npc.GetAttribute(AttribType.Energy) > PETools.PEMath.Epsilon)
						info.SetSurplusCnt(0.5f);
					else
						info.SetSurplusCnt(0);
				}
				else
					info.SetSurplusCnt(0);
			}
			else if(weaponInfo.costItem > 0)
			{
				GunAmmo gunAmmo = obj.GetCmpt<GunAmmo>();
				if(gunAmmo == null)
				{
					if(npc.GetItemCount(weaponInfo.costItem) <3)
						info.SetSurplusCnt(0.5f);
					else
						info.SetSurplusCnt(1.0f);
				}
				else
				{
					if(gunAmmo.count <3)
					{
						if(npc.GetItemCount(weaponInfo.costItem) <3)
							info.SetSurplusCnt(0.5f);
						else
							info.SetSurplusCnt(1.0f);
					}
					else
						info.SetSurplusCnt(1.0f);
				}

			}
			else
				info.SetSurplusCnt(1.0f);

			info.SetObj(obj);
			return info;
		}

		public static List<EvaluateInfo> Evaluates(PeEntity npc,Enemy enemy,List<ItemObject> objs)
		{
			infos.Clear();
			for(int i=0;i<objs.Count;i++)
			{
				EvaluateInfo info = Evaluate(npc,enemy,objs[i]);
				if(info.EvaluateValue >0)
					infos.Add(info);
			}
			return infos;
		}

	}

}

