using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ItemAsset
{
	public enum EequipEditorType
	{
		sword = 4,
		axe = 7,
		shield = 9,
		bow = 10,
		gun = 11,
		battery,
		energy_sheild,
		//ammunition = 12
	}
	
	public enum EeqSelect
	{
		None,
		combat,
		protect,
		tool,
		energy,
		energy_sheild,
		Max
	}

	public class EquipInfo
	{
		public EequipEditorType _equipType;
		public EeqSelect _selectType;
		public EquipInfo()
		{}
		public EquipInfo(EequipEditorType equipType,EeqSelect select)
		{
			_equipType = equipType;
			_selectType = select;
		}
		public void SetEquipInfo(EequipEditorType equipType,EeqSelect select)
		{
			_equipType = equipType;
			_selectType = select;
		}
	}

	public class EquipItemMgr
	{
		List<ItemObject>[] mItemsDic;
		//List<ItemObject> mAttackobjs;
		ItemObject[] mEquipObjs;

		List<ItemObject>[] mAtkItems;
		public EquipItemMgr()
		{
			Init();
			//mAttackobjs = new List<ItemObject>();
		}

		void Init()
		{
			mItemsDic = new List<ItemObject>[(int)EeqSelect.Max];
            mItemsDic[(int)EeqSelect.combat] = new List<ItemObject>();
            mItemsDic[(int)EeqSelect.protect] = new List<ItemObject>();
			//mItemsDic[AttackSelect.Range] = new List<ItemObject>();
            mItemsDic[(int)EeqSelect.tool] = new List<ItemObject>();
            mItemsDic[(int)EeqSelect.energy] = new List<ItemObject>();
            mItemsDic[(int)EeqSelect.energy_sheild] = new List<ItemObject>();

            mAtkItems = new List<ItemObject>[(int)AttackType.Ranged + 1]; //new Dictionary<AttackType, List<ItemObject>>();
			mAtkItems[(int)AttackType.Melee] = new List<ItemObject>();
			mAtkItems[(int)AttackType.Ranged] = new List<ItemObject>();

		}

		public bool Add(ItemObject obj)
		{
			if(obj == null)
				return false;

			EquipInfo info = SelectItem.SwichEquipInfo(obj);
			if(info != null)
			{
                mItemsDic[(int)info._selectType].Add(obj);

				if(obj.protoData.weaponInfo != null && !mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Contains(obj))
					mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Add(obj);

				return true;
			}
			return false;
		}
		
		public bool ReMove(ItemObject obj)
		{
			if(obj == null)
				return false;

			EquipInfo info = SelectItem.SwichEquipInfo(obj);
			if(info != null)
			{
				mItemsDic[(int)info._selectType].Remove(obj);

				if(obj.protoData.weaponInfo != null && mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Contains(obj))
					mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Remove(obj);

				return true;
			}
			return false;
		}

		public bool hasAtkEquip(AttackType type)
		{
			return mAtkItems[(int)type].Count >0;
		}

		public List<ItemObject> GetAtkEquips(AttackType type)
		{
			return mAtkItems[(int)type];
		}

		public void Clear()
		{
            mItemsDic[(int)EeqSelect.combat].Clear();
            mItemsDic[(int)EeqSelect.protect].Clear(); 
			//mItemsDic[AttackSelect.Range].Clear(); 
            mItemsDic[(int)EeqSelect.tool].Clear();
            mItemsDic[(int)EeqSelect.energy].Clear();
            mItemsDic[(int)EeqSelect.energy_sheild].Clear();

			mAtkItems[(int)AttackType.Melee].Clear();
			mAtkItems[(int)AttackType.Ranged].Clear();
		}

		public List<ItemObject> GetEquipItemObjs(EeqSelect selet)
		{
			if(mItemsDic == null)
				return null;

            return mItemsDic[(int)selet];
		}

        //public List<ItemObject> GetAttackEquipObjs()
        //{
        //    mAttackobjs.Clear();
        //    mAttackobjs.AddRange(mItemsDic[(int)EeqSelect.combat]);
        //    //mAttackobjs.AddRange(mItemsDic[AttackSelect.Range]);
        //    return mAttackobjs;
        //}

	}
	
}
