using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;

public class WaterPitcher : Equipment 
{
	public override bool CostSkill (ISkillTarget target, int sex, bool buttonDown, bool buttonPressed)
	{
        //Player player = mSkillRunner as Player;
        //DefaultPosTarget defaultTarget = new DefaultPosTarget(mSkillRunner.transform.position + mSkillRunner.transform.forward);
        //if(buttonPressed && null != player && player.PlayerStandInwater)
        //{
        //    bool success = base.CostSkill (defaultTarget, sex, buttonDown);

        //    if (GameConfig.IsMultiMode && mMainPlayerEquipment && success)
        //        player.RPCServer(EPacketType.PT_InGame_WaterPitcher);
			

        //    return success;
        //}

		return false;
	}
}
