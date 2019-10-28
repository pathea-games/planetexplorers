using UnityEngine;
using System.Collections;
using System;
using Pathea;

public class PEAnimatorSkill : PEAnimatorEvent
{
    public int skillID;

    internal override void OnTrigger()
	{
		if(PeGameMgr.IsMulti && (null == Entity || null == Entity.netCmpt || !Entity.netCmpt.IsController))
			return;

        if(Entity != null && skillID > 0)
        {
            if(Entity.attackEnemy != null)
                Entity.StartSkill(Entity.attackEnemy.skTarget, skillID);
            else
                Entity.StartSkill(null, skillID);
        }
    }
}
