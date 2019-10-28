using UnityEngine;
using RootMotion.FinalIK;

namespace Pathea
{
	[System.Serializable]
	public class Action_Drive : PEAction 
	{
		public override PEActionType ActionType { get { return PEActionType.Drive; } }

		public IKDrive ikDrive{ get; set; }
		public SkillTreeUnitMgr skillTreeMgr;

		WhiteCat.CarrierController m_DrivingController;

		WhiteCat.VCPBaseSeat m_Seat;

		public WhiteCat.VCPBaseSeat seat { get { return m_Seat; } }

		string m_AnimName;

		Transform m_LHand;

		Transform m_RHand;

		public void SetSeat(string animName, WhiteCat.VCPBaseSeat seat)
		{
			m_Seat = seat;
			m_AnimName = animName;
			if(null != anim && "" != m_AnimName)
				anim.SetBool(m_AnimName, true);
		}
		
		public void SetHand(Transform lHand, Transform rHand)
		{
			m_LHand = lHand;
			m_RHand = rHand;
			if(null != ikDrive)
			{
				ikDrive.active = true;
				ikDrive.m_LHand = m_LHand;
				ikDrive.m_RHand = m_RHand;
			}
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(motionMgr.IsActionRunning(PEActionType.Build))
			{
				PeTipMsg.Register(PELocalization.GetString(9500246), PeTipMsg.EMsgLevel.Warning);
				return false;
			}

			PEActionParamDrive paramDrive = para as PEActionParamDrive;
			m_DrivingController = paramDrive.controller;
			if(m_DrivingController == null)
				return false;
			ItemAsset.ItemObject item = m_DrivingController.itemObject;
			if(item != null && skillTreeMgr != null && RandomMapConfig.useSkillTree)
			{
				if(!skillTreeMgr.CheckDriveEnable(item.protoData.itemClassId,item.protoData.level))
				{
					return false;
				}
			}
			return true;
		}

		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.OnVehicle, true);
			PEActionParamDrive paramDrive = para as PEActionParamDrive;
			m_DrivingController = paramDrive.controller;
			int seatIndex = paramDrive.seatIndex;

			m_DrivingController.GetOn(motionMgr.Entity, seatIndex);

			motionMgr.FreezePhyState(GetType(), true);
			motionMgr.FreezeCol = true;

			if(null != ikCmpt)
				ikCmpt.ikEnable = false;
			if(null != equipCmpt)
				equipCmpt.HideEquipmentByVehicle(true);
			if (null != motionMgr.Entity.biologyViewCmpt)
				motionMgr.Entity.biologyViewCmpt.ActivateInjured (false);

			motionMgr.Entity.SendMsg(EMsg.Action_GetOnVehicle, true, m_DrivingController);
		}

		public override void OnModelBuild ()
		{
			if(null != anim && "" != m_AnimName)
				anim.SetBool(m_AnimName, true);
			if(null != equipCmpt)
				equipCmpt.HideEquipmentByVehicle(true);
			
			if(null != ikDrive && null != m_LHand && null != m_RHand)
			{
				ikDrive.active = true;
				ikDrive.m_LHand = m_LHand;
				ikDrive.m_RHand = m_RHand;
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
			motionMgr.SetMaskState(PEActionMask.OnVehicle, false);
			if(null != anim)
				anim.SetBool(m_AnimName, false);

			if(null != m_Seat)
				m_Seat.GetOff();
			else if(null != entity && null != entity.passengerCmpt)
				entity.passengerCmpt.GetOffCarrier(entity.position);

			motionMgr.FreezePhyState(GetType(), false);
			motionMgr.FreezeCol = false;
			
			if(null != equipCmpt)
				equipCmpt.HideEquipmentByVehicle(false);

			if(null != ikDrive)
			{
				ikDrive.active = false;
				ikDrive.m_LHand = null;
				ikDrive.m_RHand = null;
			}
			
			if (null != motionMgr.Entity.biologyViewCmpt)
				motionMgr.Entity.biologyViewCmpt.ActivateInjured (true);
			
			if(null != ikCmpt)
				ikCmpt.ikEnable = true;

			m_Seat = null;
			m_AnimName = "";
			
			motionMgr.Entity.SendMsg(EMsg.Action_GetOnVehicle, false, m_DrivingController);
		}
	}
}
