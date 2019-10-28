#define PLANET_EXPLORERS
using UnityEngine;
using System.Collections.Generic;

#if PLANET_EXPLORERS
using ItemAsset;
using SkillAsset;
using WhiteCat;
using Pathea;
#endif

// Register functions of CreationData
// Mediator part
public partial class CreationData
{
	public const int ObjectStartID = 100000000;

#if PLANET_EXPLORERS

	public static int s_SubSkillStartID = 600000000;
	public static int s_BuffStartID = 100000000;
	public static int QueryNewSubSkillID(int objectid)
	{
		int max = s_SubSkillStartID + (objectid - ObjectStartID) * 1024 - 1;
		int end = max + 1024;
		foreach (EffSkill skill in SkillAsset.EffSkill.s_tblEffSkills)
		{
			if (skill.m_id > max && skill.m_id < end)
			{
				max = skill.m_id;
			}
		}
		return max + 1;
	}

#endif

#if PLANET_EXPLORERS

	public static ItemProto StaticGenItemData(int obj_id, VCIsoHeadData headinfo, CreationAttr attr)
	{
		if (attr.m_Type == ECreation.Null)
			return null;

		ItemProto item = new ItemProto();

		item.name = headinfo.Name;
//		item.nameStringId = 0;
		item.englishDescription = headinfo.Desc;
		item.itemLabel = 100;
		item.setUp = 0;
		item.resourcePath = AssetsLoader.InvalidAssetPath;
		item.resourcePath1 = AssetsLoader.InvalidAssetPath;
		item.equipReplacePos = 0;
		item.currencyValue = Mathf.CeilToInt(attr.m_SellPrice);
		item.currencyValue2 = Mathf.CeilToInt(attr.m_SellPrice);
		item.durabilityMax = Mathf.CeilToInt(attr.m_Durability);
		item.repairLevel = 1;
		item.maxStackNum = 1;
		item.equipSex = Pathea.PeSex.Undefined;
		item.id = obj_id;
		item.level = 1;

		item.repairMaterialList = new List<MaterialItem>(attr.m_Cost.Count);
		item.strengthenMaterialList = new List<MaterialItem>(attr.m_Cost.Count);

		Dictionary<int, int> repairItemDic = new Dictionary<int, int>();

		foreach (KeyValuePair<int, int> kvp in attr.m_Cost)
		{
			if (kvp.Value != 0)
			{
				Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(kvp.Key);
				int itemID = kvp.Key;
				int itemCount = kvp.Value;
				if(null == formula)
				{
					if(repairItemDic.ContainsKey(itemID))
						repairItemDic[itemID] += itemCount;
					else
						repairItemDic[itemID] = itemCount;
				}
				else
				{
					for(int i = 0; i < formula.materials.Count; ++i)
					{
						itemID = formula.materials[i].itemId;
						itemCount = formula.materials[i].itemCount * Mathf.CeilToInt((float)kvp.Value / formula.m_productItemCount);
						if(repairItemDic.ContainsKey(itemID))
							repairItemDic[itemID] += itemCount;
						else
							repairItemDic[itemID] = itemCount;
					}
				}
			}
		}
		
		foreach (KeyValuePair<int, int> kvp in repairItemDic)
		{
			int finalCount = kvp.Value / 2;
			if(finalCount > 0)
			{
				item.repairMaterialList.Add(new MaterialItem() { protoId = kvp.Key, count = finalCount });
				item.strengthenMaterialList.Add(new MaterialItem() { protoId = kvp.Key, count = finalCount });
			}
		}

		// [VCCase] - Generate item data, different types of creations
		switch (attr.m_Type)
		{
			// 剑 ------------------------
			case ECreation.Sword:
            
				item.equipType = EquipType.Sword;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 16;
				item.itemClassId = (int)CreationItemClass.Sword;
				item.tabIndex = 1;
				item.sortLabel = 9910;
				//item.level = Mathf.Clamp((int)((attr.m_Attack - 8f) / 2.5f), 1, 100);
				item.durabilityFactor = 1f;
				item.editorTypeId = 4;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.Atk, attr.m_Attack);
				item.buffId = 30200087;
				break;

                //双手与双持
            case ECreation.SwordLarge:
            case ECreation.SwordDouble:
                item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 528;
				item.itemClassId = (int)CreationItemClass.Sword;
				item.tabIndex = 1;
				item.sortLabel = 9910;
				//item.level = Mathf.Clamp((int)((attr.m_Attack - 8f) / 2.5f), 1, 100);
				item.durabilityFactor = 1f;
				item.editorTypeId = 4;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.Atk, attr.m_Attack);
				item.buffId = 30200087;
				break;
			// 弓 ------------------------
			case ECreation.Bow:
				item.equipType = EquipType.Bow;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 528;
				item.itemClassId = (int)CreationItemClass.Bow;
				item.tabIndex = 1;
				item.sortLabel = 9914;
				//item.level = Mathf.Clamp((int)((attr.m_Attack - 8f) / 2.5f), 1, 100);
				item.durabilityFactor = 1f;			
				item.editorTypeId = 10;
				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.Atk, attr.m_Attack);
				item.buffId = 30200087;
				break;

			// 斧 ------------------------
			case ECreation.Axe:
				item.equipType = EquipType.Axe;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 16;
				item.itemClassId = (int)CreationItemClass.Axe;
				item.tabIndex = 1;
				item.sortLabel = 9912;
				//item.level = Mathf.Clamp((int)((attr.m_Attack - 8f) / 2.5f), 1, 100);
				item.durabilityFactor = 1f;
				item.editorTypeId = 7;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.Atk, attr.m_Attack * WhiteCat.PEVCConfig.instance.axeAttackScale);
				item.propertyList.AddProperty(Pathea.AttribType.CutDamage, attr.m_Attack * WhiteCat.PEVCConfig.instance.axeCutDamageScale);
				item.propertyList.AddProperty(Pathea.AttribType.CutBouns, 0.03f);
				item.buffId = 30200092;
				break;

			// 盾 ------------------------
			case ECreation.Shield:
				item.equipType = EquipType.Shield_Hand;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 512;
				item.itemClassId = (int)CreationItemClass.Shield;
				item.tabIndex = 1;
				item.sortLabel = 9920;
				//item.level = Mathf.Clamp((int)((attr.m_Defense + 2f) / 2f), 1, 100);
				item.durabilityFactor = 0.01f;			
				item.editorTypeId = 9;
				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.Def, attr.m_Defense);
				item.propertyList.AddProperty(Pathea.AttribType.ShieldMeleeProtect, Mathf.Clamp(attr.m_Defense / 330 + 0.2f, 0.2f, 0.87f));
				item.buffId = 30200093;
				break;


			// 手枪/单手枪 ------------------------
			case ECreation.HandGun:
				item.equipType = EquipType.HandGun;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 528;
				item.itemClassId = (int)CreationItemClass.HandGun;
				item.tabIndex = 1;
				item.sortLabel = 9930;
				//item.level = Mathf.Clamp((int)((attr.m_Attack - 4f) / 2f), 1, 70);
				item.durabilityFactor = 1f;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.Atk, attr.m_Attack);
				item.buffId = 30200095;			
				item.editorTypeId = 11;
				break;


			// 步枪/双手枪 ------------------------
			case ECreation.Rifle:
				item.equipType = EquipType.Rifle;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 528;
				item.itemClassId = (int)CreationItemClass.Rifle;
				item.tabIndex = 1;
				item.sortLabel = 9940;
				//item.level = Mathf.Clamp((int)((attr.m_Attack - 4f) / 2f), 1, 70);
				item.durabilityFactor = 1f;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.Atk, attr.m_Attack);
				item.buffId = 30200095;			
				item.editorTypeId = 11;
				break;

			// 车 ------------------------
			case ECreation.Vehicle:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "leftup_putdown", "0" };
				item.equipPos = 0;
				item.itemClassId = (int)CreationItemClass.Vehicle;
				item.tabIndex = 0;
				item.sortLabel = 9950;
				//item.level = CalcCarrierLevel(attr.m_Durability);

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.HpMax, attr.m_Durability);
				item.propertyList.AddProperty(Pathea.AttribType.Hp, attr.m_Durability);
				item.engergyMax = (int)attr.m_MaxFuel;
				break;

			// 飞机 ------------------------
			case ECreation.Aircraft:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "leftup_putdown", "0" };
				item.equipPos = 0;
				item.itemClassId = (int)CreationItemClass.Aircraft;
				item.tabIndex = 0;
				item.sortLabel = 9960;
				//item.level = CalcCarrierLevel(attr.m_Durability);

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.HpMax, attr.m_Durability);
				item.propertyList.AddProperty(Pathea.AttribType.Hp, attr.m_Durability);
				item.engergyMax = (int)attr.m_MaxFuel;
				break;

			// 船 ------------------------
			case ECreation.Boat:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "leftup_putdown", "0" };
				item.equipPos = 0;
				item.itemClassId = (int)CreationItemClass.Boat;
				item.tabIndex = 0;
				item.sortLabel = 9970;
				//item.level = CalcCarrierLevel(attr.m_Durability);

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.HpMax, attr.m_Durability);
				item.propertyList.AddProperty(Pathea.AttribType.Hp, attr.m_Durability);
				item.engergyMax = (int)attr.m_MaxFuel;
				break;

			// 简单物体 ------------------------
			case ECreation.SimpleObject:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "leftup_putdown", "0" };
				item.equipPos = 0;
				item.itemClassId = (int)CreationItemClass.SimpleObject;
				item.sortLabel = 9980;
				item.tabIndex = 0;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.HpMax, attr.m_Durability);
				item.propertyList.AddProperty(Pathea.AttribType.Hp, attr.m_Durability);
				break;

			// 装甲 ------------------------
			case ECreation.ArmorHead:
			case ECreation.ArmorBody:
			case ECreation.ArmorArmAndLeg:
			case ECreation.ArmorHandAndFoot:
			case ECreation.ArmorDecoration:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 0;
				item.itemClassId = (int)CreationItemClass.Armor;
				item.sortLabel = 9990 + (int)attr.m_Type;
				item.tabIndex = 3;
				item.durabilityFactor = 0.01f;
				break;

			// 机器人 ------------------------
			case ECreation.Robot:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "leftup_putdown", "0" };
				item.equipPos = 0;
				item.itemClassId = (int)CreationItemClass.Robot;
				item.tabIndex = 0;
				item.sortLabel = 9945;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.HpMax, attr.m_Durability);
				item.propertyList.AddProperty(Pathea.AttribType.Hp, attr.m_Durability);
				item.engergyMax = (int)attr.m_MaxFuel;
				break;

			// 炮台 ------------------------
			case ECreation.AITurret:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "leftup_putdown", "0" };
				item.equipPos = 0;
				item.itemClassId = (int)CreationItemClass.AITurret;
				item.tabIndex = 0;
				item.sortLabel = 9946;

				if (item.propertyList == null) item.propertyList = new ItemProto.PropertyList();
				item.propertyList.AddProperty(Pathea.AttribType.HpMax, attr.m_Durability);
				item.propertyList.AddProperty(Pathea.AttribType.Hp, attr.m_Durability);
				item.engergyMax = (int)attr.m_MaxFuel;
				item.unchargeable = attr.m_Defense < 0.5f;
				break;

			default:
				item.equipType = EquipType.Null;
				item.icon = new string[3] { "0", "0", "0" };
				item.equipPos = 0;
				item.sortLabel = 10000;
				item.tabIndex = 0;
				break;
		}
		return item;
	}


	// Generate an ItemData structure from CreationData for planet explorers project
	private ItemProto GenItemData()
	{
		return StaticGenItemData(m_ObjectID, m_IsoData.m_HeadInfo, m_Attribute);
	}

#endif

	// Main register function >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	public void Register()
	{
		if (m_Attribute.m_Type == ECreation.Null)
			return;
		if (m_Prefab == null)
			return;

#if PLANET_EXPLORERS
		/////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////
		ItemProto itemdata = GenItemData();

		AddWeaponInfo(itemdata, m_Prefab);

		if (itemdata == null)
			return;

		byte[] buff = this.m_IsoData.m_HeadInfo.IconTex;
		if (null != buff)
		{
			Texture2D iconTex = new Texture2D(2, 2);
			iconTex.LoadImage(buff);
			iconTex.Apply(false, true);

			itemdata.iconTex = iconTex;
		}

		if (null != ItemProto.Mgr.Instance.Get(itemdata.id))
		{
			return;
		}

		// Add itemdata
		//ItemAsset.ItemData.s_tblItemData.Add(itemdata);
		ItemProto.Mgr.Instance.Add(itemdata);
#endif
	}

	void AddWeaponInfo(ItemProto itemProto, GameObject obj)
	{
		AttackMode attackMode;
		switch(m_Attribute.m_Type)
		{
		case ECreation.Sword:
        case ECreation.SwordLarge:
        case ECreation.SwordDouble:
			PeSword[] swords = obj.GetComponentsInChildren<PeSword>(true);
			if(null != swords && swords.Length > 0)
			{
				PeSword sword = swords[0];
				attackMode = new AttackMode();
				itemProto.weaponInfo = new ItemProto.WeaponInfo();
				itemProto.weaponInfo.attackModes = new AttackMode[1];
				itemProto.weaponInfo.attackModes[0] = attackMode;
				attackMode.type = sword.m_AttackMode[0].type;
				attackMode.minRange = sword.m_AttackMode[0].minRange;
				attackMode.maxRange = sword.m_AttackMode[0].maxRange;
				attackMode.minSwitchRange = sword.m_AttackMode[0].minSwitchRange;
				attackMode.maxSwitchRange = sword.m_AttackMode[0].maxSwitchRange;
				attackMode.minAngle = sword.m_AttackMode[0].minAngle;
				attackMode.maxAngle = sword.m_AttackMode[0].maxAngle;
				attackMode.frequency = sword.m_AttackMode[0].frequency;
				attackMode.damage = itemProto.propertyList.GetProperty(Pathea.AttribType.Atk);
			}
			break;
		case ECreation.Bow:
			PEBow[] bows = obj.GetComponentsInChildren<PEBow>(true);
			if(null != bows && bows.Length > 0)
			{
				PEBow bow = bows[0];
				attackMode = new AttackMode();
				itemProto.weaponInfo = new ItemProto.WeaponInfo();
				itemProto.weaponInfo.attackModes = new AttackMode[1];
				itemProto.weaponInfo.attackModes[0] = attackMode;
				attackMode.type = bow.m_AttackMode[0].type;
				attackMode.minRange = bow.m_AttackMode[0].minRange;
				attackMode.maxRange = bow.m_AttackMode[0].maxRange;
				attackMode.minSwitchRange = bow.m_AttackMode[0].minSwitchRange;
				attackMode.maxSwitchRange = bow.m_AttackMode[0].maxSwitchRange;
				attackMode.minAngle = bow.m_AttackMode[0].minAngle;
				attackMode.maxAngle = bow.m_AttackMode[0].maxAngle;
				attackMode.frequency = bow.m_AttackMode[0].frequency;
				attackMode.damage = itemProto.propertyList.GetProperty(Pathea.AttribType.Atk);
				itemProto.weaponInfo.costItem = bow.m_CostItemID[0];
			}
			break;
		case ECreation.HandGun:
		case ECreation.Rifle:
			PEGun[] guns = obj.GetComponentsInChildren<PEGun>(true);
			if(null != guns && guns.Length > 0)
			{
				PEGun gun = guns[0];
				attackMode = new AttackMode();
				itemProto.weaponInfo = new ItemProto.WeaponInfo();
				itemProto.weaponInfo.attackModes = new AttackMode[1];
				itemProto.weaponInfo.attackModes[0] = attackMode;
				attackMode.type = gun.m_AttackMode[0].type;
				attackMode.minRange = gun.m_AttackMode[0].minRange;
				attackMode.maxRange = gun.m_AttackMode[0].maxRange;
				attackMode.minSwitchRange = gun.m_AttackMode[0].minSwitchRange;
				attackMode.maxSwitchRange = gun.m_AttackMode[0].maxSwitchRange;
				attackMode.minAngle = gun.m_AttackMode[0].minAngle;
				attackMode.maxAngle = gun.m_AttackMode[0].maxAngle;
				attackMode.frequency = gun.m_AttackMode[0].frequency;
				attackMode.damage = itemProto.propertyList.GetProperty(Pathea.AttribType.Atk);
				itemProto.weaponInfo.costItem = gun.m_AmmoItemIDList.Length > 0 ? gun.m_AmmoItemIDList[0] : 0;
				itemProto.weaponInfo.useEnergry = gun.m_AmmoType == AmmoType.Energy;
				itemProto.weaponInfo.costPerShoot = itemProto.weaponInfo.useEnergry ? (int)gun.m_EnergyPerShoot : 1;
			}
			break;
		}
	}


	// Unregister function
	public static void UnregisterAll()
	{
#if PLANET_EXPLORERS
		//if ( ItemData.s_tblItemData == null )
		//    return;
		if (EffSkill.s_tblEffSkills == null)
			return;
		if (EffSkillBuff.s_tblEffSkillBuffs == null)
			return;

		ItemProto.Mgr.Instance.ClearCreation();

		for (int i = EffSkill.s_tblEffSkills.Count - 1; i >= 0; i--)
		{
			if (EffSkill.s_tblEffSkills[i].m_id >= CreationData.s_SubSkillStartID)
			{
				EffSkill.s_tblEffSkills.RemoveAt(i);
			}
		}
		for (int i = EffSkillBuff.s_tblEffSkillBuffs.Count - 1; i >= 0; i--)
		{
			if (EffSkillBuff.s_tblEffSkillBuffs[i].m_id >= CreationData.s_BuffStartID)
			{
				EffSkillBuff.s_tblEffSkillBuffs.RemoveAt(i);
			}
		}
#endif
	}
}
