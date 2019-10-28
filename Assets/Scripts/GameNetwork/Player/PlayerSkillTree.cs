using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using CustomData;
using SkillAsset;
using ItemAsset;

public partial class PlayerNetwork
{
	public SkillTreeUnitMgr _learntSkills;

	public void SKTLearn(int skillType)
	{
		_learntSkills.SKTLearn (skillType);
	}
	void RPC_S2C_InitLearntSkills(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] ids = stream.Read<int[]>();
		for(int i = 0; i < ids.Length;i++)
		{
			SkillTreeUnit skillUnit = SkillTreeInfo.GetSkillUnit(ids[i]);
			if(skillUnit != null)
				_learntSkills.AddSkillUnit(skillUnit);
		}
		_learntSkills.InitDefaultSkill();
	}		


	void RPC_S2C_SKTLevelUp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int levelUpType = stream.Read<int>();
		int level = stream.Read<int> ();
		int playerid = stream.Read<int> ();
		if(playerid == PlayerNetwork.mainPlayerId || _learntSkills == null)
			return;
		SkillTreeUnit skillUnit = _learntSkills.FindSkillUnit (levelUpType);
		if(skillUnit != null)
		{
			_learntSkills.RemoveSkillUnit(skillUnit);
		}
		SkillTreeUnit nextSkillUnit = SkillTreeInfo.GetSkillUnit (levelUpType, level);
		if(nextSkillUnit != null)
		{
			_learntSkills.AddSkillUnit(nextSkillUnit);
		}
	}
}

