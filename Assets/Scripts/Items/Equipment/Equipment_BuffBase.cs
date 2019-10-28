using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;

public class Equipment_BuffBase : Equipment 
{
	public int mBuffID;
	
	EffSkillBuff mBuff;
	
	public override void InitEquipment (SkillRunner runner, ItemObject item)
	{
		base.InitEquipment (runner, item);
		if(mBuffID != 0)
		{
			mBuff = EffSkillBuff.s_tblEffSkillBuffs.Find(iter0 => EffSkillBuff.MatchId(iter0, mBuffID));
			if(null != mBuff)
				mSkillRunner.m_effSkillBuffManager.AddBuff(mBuff);
		}
	}
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		if(null != mBuff)
			mSkillRunner.m_effSkillBuffManager.RemoveBuff(mBuff);
	}
}
