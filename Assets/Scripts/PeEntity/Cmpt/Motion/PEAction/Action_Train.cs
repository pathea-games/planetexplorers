using UnityEngine;
using System.Collections;

namespace Pathea
{
	public class Action_Train : PEAction 
	{
		public override PEActionType ActionType { get { return PEActionType.GetOnTrain; } }

		string mAnim;

		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.GetOnTrain, true);
			
			PEActionParamS paramS = para as PEActionParamS;
			mAnim = paramS.str;
			if(null != equipCmpt)
				equipCmpt.HideEquipmentByVehicle(true);
			if (!string.IsNullOrEmpty (mAnim) && null != anim)
				anim.SetBool (mAnim, true);
			if(null != viewCmpt)
				viewCmpt.ActivateInjured(false);
			if (null != ikCmpt)
				ikCmpt.ikEnable = false;
			if(motionMgr.Entity == MainPlayer.Instance.entity)
				PeCamera.SetBool("OnMonorail", true);
		}
		public override bool Update ()
		{
			return false;
		}
	
		public override void OnModelDestroy ()
		{
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.GetOnTrain, false);
			if(null != equipCmpt)
				equipCmpt.HideEquipmentByVehicle(false);
			if (!string.IsNullOrEmpty (mAnim) && null != anim)
				anim.SetBool (mAnim, false);
			if(null != viewCmpt)
				viewCmpt.ActivateInjured(true);
			if (null != ikCmpt)
				ikCmpt.ikEnable = true;			
			if(motionMgr.Entity == MainPlayer.Instance.entity)
				PeCamera.SetBool("OnMonorail", false);
		}
	}
}
