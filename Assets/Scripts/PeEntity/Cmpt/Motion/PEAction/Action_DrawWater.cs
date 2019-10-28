using UnityEngine;
using SkillSystem;

namespace Pathea
{
	[System.Serializable]
	public class Action_DrawWater : PEAction 
	{
		const int DrawWaterSkillID = 20110051;
		public override PEActionType ActionType { get { return PEActionType.Draw; } }

		PEWaterPitcher m_WaterPitcher;
		public PEWaterPitcher waterPitcher
		{
			get { return m_WaterPitcher; }
			set 
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_WaterPitcher = value;
			}
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null != waterPitcher && null != skillCmpt && null != trans)
				return VFVoxelWater.self.IsInWater(trans.position);
			return false;
		}

		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Draw, true);
			if(null != waterPitcher && null != skillCmpt)
				skillCmpt.StartSkill(skillCmpt, DrawWaterSkillID);
			if(null != anim)
				anim.ResetTrigger("ResetFullBody");
		}

		public override bool Update ()
		{
			if(null != skillCmpt && skillCmpt.IsSkillRunning(DrawWaterSkillID))
				return false;
			
			motionMgr.SetMaskState(PEActionMask.Draw, false);
			return true;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Draw, false);
			if(null != anim)
				anim.SetTrigger("ResetFullBody");
			if(null != skillCmpt)
				skillCmpt.CancelSkillById(DrawWaterSkillID);
		}
	}
}