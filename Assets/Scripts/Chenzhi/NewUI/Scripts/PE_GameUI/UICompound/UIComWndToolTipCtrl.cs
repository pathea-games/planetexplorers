using UnityEngine;
using System.Collections;
using ItemAsset;
using SkillAsset;
using System;

public class UIComWndToolTipCtrl : MonoBehaviour 
{
	private ItemSample mItemSample = null;
	private int mItemId = 0;
	ListItemType mType;
	
	public void SetToolTipInfo(ListItemType type,int id)
	{
		mType = type;
		mItemId = id;
	}


	public int GetItemID()
	{
		return mItemId;
	}
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

    ItemObject _itemObj = null;

	void OnTooltip (bool show)
	{
		
		if( ListItemType.mItem == mType)
		{
			if(show == true && mItemSample == null  && mItemId != 0)
				mItemSample = new ItemSample(mItemId);
			else if(show == false)
				mItemSample = null;
			
			if(mItemSample != null)
			{
				//string  tipStr = PELocalization.GetString(mItemSample.protoData.descriptionStringId);
				
                //EffSkill skill = EffSkill.s_tblEffSkills.Find(iterSkill1=>EffSkill.MatchId(iterSkill1,mItemSample.mItemData.m_SkillID));
                //if(skill != null && skill.m_skillIdsGot != null)
                //{
                //    bool learnAllSkill = true;
                //    foreach(int id in skill.m_skillIdsGot)
                //    {
                //        if(!PlayerFactory.mMainPlayer.m_skillBook.m_mergeSkillIDs.Contains(id))
                //        {
                //            learnAllSkill = false;
                //            break;
                //        }
                //    }
                //    if(learnAllSkill)
                //        tipStr += PELocalization.GetString(4000001);
                //}
				
                //foreach(ItemProperty pro in Enum.GetValues(typeof(ItemProperty)))
                //{
                //    string replaceName = "$" + (int)pro + "$";
                //    switch(pro)
                //    {
                //    case ItemProperty.DamageReduceByShortAttack:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.DamageReduceByShortAttack))
                //            tipStr = tipStr.Replace(replaceName,((int)(mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.DamageReduceByShortAttack] * 100)).ToString() + "%");
                //        break;
                //    case ItemProperty.DamageReduceByProjectile:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.DamageReduceByProjectile))
                //            tipStr = tipStr.Replace(replaceName,((int)(mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.DamageReduceByProjectile] * 100)).ToString() + "%");
                //        break;
                //    case ItemProperty.ShieldEnergyCostFacter:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.ShieldEnergyCostFacter))
                //            tipStr = tipStr.Replace(replaceName,((int)(mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.ShieldEnergyCostFacter] * 100)).ToString() + "%");
                //        break;
                //    case ItemProperty.BatteryPower:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.BatteryPowerMax))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.BatteryPowerMax]).ToString());
                //        break;
                //    case ItemProperty.Durability:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.DurabilityMax))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.DurabilityMax]).ToString());
                //        break;
                //    case ItemProperty.ShieldEnergy:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(ItemProperty.ShieldMax))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[ItemProperty.ShieldMax]).ToString());
                //        break;
                //    default:
                //        if(mItemSample.prototypeData.mEquipBaseProperty.ContainsKey(pro))
                //            tipStr = tipStr.Replace(replaceName,((int)mItemSample.prototypeData.mEquipBaseProperty[pro]).ToString());
                //        else
                //            tipStr = tipStr.Replace(replaceName, "0");
                //        break;
                //    }
                //}
                _itemObj = ItemMgr.Instance.CreateItem(mItemId);
                string tipStr = _itemObj.GetTooltip();
                ToolTipsMgr.ShowText(tipStr);
			}
			else
			{
                ItemMgr.Instance.DestroyItem(_itemObj);
				ToolTipsMgr.ShowText(null);
			}
		}
	}
}
