using UnityEngine;
using System.Collections;

namespace Pathea
{
	[System.Serializable]
	public class Action_FlashLight : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.HoldFlashLight; } }

		PEFlashLight m_FlashLight;

		public PEFlashLight flashLight
		{
			get { return m_FlashLight; }
			set
			{
				m_FlashLight = value;
				if(null != m_FlashLight)
				{
					if(null != ikCmpt.m_IKFlashLight)
						ikCmpt.m_IKFlashLight.aimTrans = m_FlashLight.aimTrans;
					motionMgr.DoActionImmediately(PEActionType.HoldFlashLight);
				}
				else
					motionMgr.EndAction(PEActionType.HoldFlashLight);
			}
		}

		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.HoldFlashingLight, true);
			if(null != anim)
				anim.SetBool("FlashLight", true);
			if(null != ikCmpt)
			{
				ikCmpt.aimActive = true;
				ikCmpt.flashLightActive = true;
				ikCmpt.aimTargetPos = Vector3.zero;
			}
		}

		public override void PauseAction ()
		{
			if(null != anim)
				anim.SetBool("FlashLight", false);
			if(null != ikCmpt)
			{
				ikCmpt.aimActive = false;
				ikCmpt.flashLightActive = false;
			}
		}

		public override void ContinueAction ()
		{
			if(null != anim)
				anim.SetBool("FlashLight", true);
			if(null != ikCmpt)
			{
				ikCmpt.aimActive = true;
				ikCmpt.flashLightActive = true;
			}
		}

		public override void OnModelBuild ()
		{
			if(null != ikCmpt)
			{
				ikCmpt.aimActive = true;
				ikCmpt.flashLightActive = true;
				if(null != ikCmpt.m_IKFlashLight && null != flashLight)
					ikCmpt.m_IKFlashLight.aimTrans = flashLight.aimTrans;
			}
		}

		public override void OnModelDestroy ()
		{
		}

		public override bool Update ()
		{
			return false;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.HoldFlashingLight, false);
			if(null != anim)
				anim.SetBool("FlashLight", false);
			if(null != ikCmpt)
			{
				ikCmpt.aimActive = false;
				ikCmpt.flashLightActive = false;
			}
		}
	}
}