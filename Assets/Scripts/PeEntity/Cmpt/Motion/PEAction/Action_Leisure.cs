using UnityEngine;
using System.Collections.Generic;
using NaturalResAsset;

namespace Pathea
{
	[System.Serializable]
	public class Action_Leisure : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Leisure; } }

		string m_AnimName;
		public override bool CanDoAction (PEActionParam para = null)
		{
			PEActionParamS paramS = para as PEActionParamS;
			string animStr = paramS.str;
			return !string.IsNullOrEmpty(animStr) && animStr != "0";
		}

		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Talk, true);
			PEActionParamS paramS = para as PEActionParamS;
			m_AnimName = paramS.str;
			if(null != anim)
				anim.SetBool(m_AnimName, true);
			if(null != ikCmpt)
				ikCmpt.ikEnable = false;
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
			motionMgr.SetMaskState(PEActionMask.Talk, false);
			if(null != anim)
				anim.SetBool(m_AnimName, false);
			if(null != ikCmpt)
				ikCmpt.ikEnable = true;
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