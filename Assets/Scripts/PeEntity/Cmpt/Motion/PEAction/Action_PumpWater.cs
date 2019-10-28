using UnityEngine;
using SkillSystem;

namespace Pathea
{
	[System.Serializable]
	public class Action_PumpWater : PEAction 
	{
		const int DumpWaterSkillID = 20110052;
		public override PEActionType ActionType { get { return PEActionType.Pump; } }

		PEWaterPump m_WaterPump;
		public PEWaterPump waterPump
		{
			get { return m_WaterPump; }
			set 
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_WaterPump = value;
			}
		}
		
		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null != waterPump && null != skillCmpt && null != waterPump.m_AimAttr.m_AimTrans)
				return VFVoxelWater.self.IsInWater(waterPump.m_AimAttr.m_AimTrans.position);
			return false;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Pump, true);
			if(null != waterPump && null != skillCmpt)
				skillCmpt.StartSkill(skillCmpt, DumpWaterSkillID);
			if(null != m_WaterPump)
				m_WaterPump.SetActiveState(true);
		}
		
		public override bool Update ()
		{
			if(null != waterPump && null != skillCmpt && null != waterPump.m_AimAttr.m_AimTrans)
			{
				if(VFVoxelWater.self.IsInWater(waterPump.m_AimAttr.m_AimTrans.position))
				{
					return false;
				}
			}
			
			OnEndAction();

			return true;
		}
		
		public override void EndImmediately ()
		{
			OnEndAction();
		}

		void OnEndAction()
		{
			motionMgr.SetMaskState(PEActionMask.Pump, false);
			if(null != skillCmpt)
				skillCmpt.CancelSkillById(DumpWaterSkillID);
			if(null != m_WaterPump)
				m_WaterPump.SetActiveState(false);
		}
	}
}
