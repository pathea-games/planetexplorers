using UnityEngine;
using System.Collections;

namespace Pathea
{
	public class Action_Abnormal : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Abnormal; } }
		string m_AnimName;
		public override bool CanDoAction (PEActionParam para = null)
		{
			PEActionParamS paramS = para as PEActionParamS;
			string animStr = paramS.str;
			if(animStr != "Pant" && motionMgr.IsActionRunning(PEActionType.Move))
				return false;
			return !string.IsNullOrEmpty(animStr) && animStr != "0";
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Abnormal, true);
			PEActionParamS paramS = para as PEActionParamS;
			m_AnimName = paramS.str;
			if(null != anim)
				anim.SetBool(m_AnimName, true);
		}
		
		public override bool Update ()
		{
			if (null == anim.animator) 
			{
				EndImmediately ();
				return true;
			}
			return false;
		}
		
		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Abnormal, false);
			if(null != anim)
				anim.SetBool(m_AnimName, false);
		}
		
		protected override void OnAnimEvent (string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				if("EndLeisure" == eventParam)
					motionMgr.EndImmediately(ActionType);
			}
		}
	}
}
