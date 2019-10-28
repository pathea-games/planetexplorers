using UnityEngine;

namespace Pathea
{
	[System.Serializable]
	public class Action_Parachute : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Parachute; } }

		public HumanPhyCtrl	m_PhyCtrl;

		PEParachute m_Parachute;
		public PEParachute parachute
		{
			get { return m_Parachute; }
			set 
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_Parachute = value;
			}
		}

		public float m_RotAcc = 5f;

		const float	UPAcc = 15f;

		Vector3		m_MoveDir;
		
		public void SetMoveDir(Vector3 moveDir)
		{
			m_MoveDir = moveDir;
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null != parachute && null != m_PhyCtrl)
				return m_PhyCtrl.velocity.y < parachute.m_TurnOnSpeed;
			
			return false;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			m_MoveDir = Vector3.zero;
			motionMgr.SetMaskState(PEActionMask.Parachute, true);
			if(null != parachute)
			{
				if(null != m_PhyCtrl)
				{
					m_PhyCtrl.ResetSpeed(parachute.m_HorizonalSpeed);
					m_PhyCtrl.desiredMovementDirection = m_PhyCtrl.currentDesiredMovementDirection;
				}
				parachute.SetOpenState(true);
			}
			if (null != anim)
				anim.SetTrigger ("Fall");
		}
		
		public override bool Update ()
		{
			if(null != parachute)
			{
				if(null != trans)
				{
					if(m_MoveDir != Vector3.zero && !motionMgr.isInAimState)
						trans.rotation = Quaternion.Lerp(trans.rotation, Quaternion.LookRotation(m_MoveDir, Vector3.up), m_RotAcc * Time.deltaTime);
					if(null != m_PhyCtrl)
					{
						if(null != move && move.state != MovementState.Air)
						{
							motionMgr.EndAction(ActionType);
						}
						else
						{
							m_PhyCtrl.desiredMovementDirection = Vector3.Lerp(m_PhyCtrl.desiredMovementDirection, m_MoveDir, 5f * Time.deltaTime);
							m_PhyCtrl.m_SubAcc = (m_PhyCtrl.velocity.y < parachute.BalanceDownSpeed) ? UPAcc * Vector3.up : Vector3.zero;
						}
					}
				}
			}
			
			return false;
		}
		
		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Parachute, false);
			if(null != parachute)
			{
				parachute.SetOpenState(false);
			}
			if(null != m_PhyCtrl)
			{
				m_PhyCtrl.m_SubAcc = Vector3.zero;
				m_PhyCtrl.desiredMovementDirection = Vector3.zero;
			}
		}
	}
}
