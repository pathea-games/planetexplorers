using UnityEngine;

namespace Pathea
{
	[System.Serializable]
	public class Action_Glider : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Glider; } }

		public HumanPhyCtrl	m_PhyCtrl;
		
		PEGlider m_Glider;		
		public PEGlider glider
		{
			get { return m_Glider; }
			set 
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_Glider = value;
			}
		}
		
		const float	UPAcc = 15f;
		
		float	m_ForwardAreaF;
		float	m_UpAreaF;
		Vector2	m_TargetOpDir = Vector2.zero;
		Vector2	m_CurrentOpDir = Vector2.zero;

		bool	m_PauseByPhy;
		Vector3 m_PauseVel;

		public void SetMoveDir(Vector3 moveDir)
		{
			m_TargetOpDir.x = moveDir.x;
			m_TargetOpDir.y = moveDir.z;
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null != glider && null != m_PhyCtrl)
				return m_PhyCtrl.velocity.y < glider.m_TurnOnSpeed;

			return false;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Glider, true);
			m_PauseByPhy = false;
			if(null != anim)
			{
				anim.ResetTrigger("ResetFullBody");
				anim.SetBool("Glider", true);
			}
			if(null != glider)
			{
				m_ForwardAreaF = glider.m_BoostPower / (m_PhyCtrl.m_AirDrag * glider.m_BalanceForwardSpeed * glider.m_BalanceForwardSpeed);
				m_UpAreaF = -m_PhyCtrl.gravity / (m_PhyCtrl.m_AirDrag * glider.m_BalanceDownSpeed * glider.m_BalanceDownSpeed);
				if(null != m_PhyCtrl)
				{
					m_PhyCtrl.freezeUpdate = true;
					m_PhyCtrl.desiredMovementDirection = Vector3.zero;
				}
				glider.SetOpenState(true);
			}
			m_CurrentOpDir  = Vector2.zero;
		}
		
		public override bool Update ()
		{
			if(null == glider || null == trans || null == m_PhyCtrl || (PeGameMgr.gamePause && !PeGameMgr.IsMulti))
				return false;

			if(m_PauseByPhy)
			{
				if(!m_PhyCtrl._rigidbody.isKinematic)
				{
					m_PhyCtrl.velocity = m_PauseVel;
					m_PauseByPhy = false;
				}
			}
			else if(m_PhyCtrl._rigidbody.isKinematic)
			{
				m_PauseVel = m_PhyCtrl.inertiaVelocity;
				m_PauseByPhy = true;
			}
			
			if(move.state != MovementState.Air)
			{
				motionMgr.EndImmediately(ActionType);
			}
			else
			{
				float yaw = trans.existent.eulerAngles.y;
				m_CurrentOpDir.x = Mathf.Lerp(m_CurrentOpDir.x, 30f * m_TargetOpDir.x, glider.m_RotateAcc * Time.deltaTime);
				m_CurrentOpDir.y = Mathf.Lerp(m_CurrentOpDir.y, 30f * m_TargetOpDir.y, glider.m_RotateAcc * Time.deltaTime / 2f);
				yaw += m_CurrentOpDir.x/2f * Time.deltaTime;
				Vector3 proDir = Vector3.Project(new Vector3(m_PhyCtrl.velocity.x, 0, m_PhyCtrl.velocity.z)
				                                 , new Vector3(trans.existent.forward.x, 0, trans.existent.forward.z));
				proDir.y = m_PhyCtrl.velocity.y;
				m_PhyCtrl.velocity = Vector3.Lerp(m_PhyCtrl.velocity, proDir, Time.deltaTime);
				trans.rotation = Quaternion.AngleAxis(yaw, Vector3.up)
					* Quaternion.AngleAxis(m_CurrentOpDir.y, Vector3.right)
						* Quaternion.AngleAxis(-m_CurrentOpDir.x, Vector3.forward);
				Vector3 upSpeed = Vector3.Project(m_PhyCtrl.velocity, trans.existent.up);
				Vector3 forwardSpeed = Vector3.Project(m_PhyCtrl.velocity, trans.existent.forward);
				Vector3 acc = 10f * Vector3.down;
				acc += -forwardSpeed.normalized * glider.m_AreaDragF * m_ForwardAreaF * forwardSpeed.sqrMagnitude;
				acc += -upSpeed.normalized * glider.m_AreaDragF * m_UpAreaF * upSpeed.sqrMagnitude;
				acc += glider.m_BoostPower * trans.existent.forward;
				m_PhyCtrl._rigidbody.AddForce(acc, ForceMode.Acceleration);
				m_PhyCtrl.ResetSpeed(m_PhyCtrl.velocity.magnitude);
			}
			
			return false;
		}
		
		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Glider, false);
			if(null != anim)
			{
				anim.SetTrigger("ResetFullBody");
				anim.SetBool("Glider", false);
			}
			if(null != glider)
			{
				glider.SetOpenState(false);
			}
			if(null != m_PhyCtrl)
			{
				m_PhyCtrl.freezeUpdate = false;
			}
			trans.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(trans.forward, Vector3.up));
		}
	}
}
