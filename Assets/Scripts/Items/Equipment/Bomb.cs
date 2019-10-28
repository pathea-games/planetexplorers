using UnityEngine;
using System.Collections;
using SkillAsset;
using WhiteCat;

public class Bomb : ShootEquipment 
{
	public override bool CostSkill (ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if(buttonDown)
		{
			if(mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
				return false;
				
			EffSkillInstance skillInstance = null;
			switch(sex)
			{
			case 1:
				skillInstance = CostSkill(mSkillRunner, mSkillFemaleId[0], target);
				break;
			case 2:
				skillInstance = CostSkill(mSkillRunner, mSkillMaleId[0], target);
				break;
			}
			if(null != skillInstance)
			{
				mHuman.ApplyAmmoCost(EArmType.Bomb, mItemObj.instanceId);
			}
			return null != skillInstance;
		}
		return false;
	}
}
