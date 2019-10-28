using UnityEngine;
using System;
using PEIK;
using RootMotion.FinalIK;
using Pathea.Operate;

namespace Pathea
{
	[Serializable]
	public class Action_Move : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Move; } }

		public HumanPhyCtrl phyCtrl{ get; set; }
		
		[HideInInspector]
		public MoveParam m_Param;

		float m_RunSpeed = 5f;
		float m_WalkSpeed = 2f;
		bool  m_IsWalk = false;

		[HideInInspector]
		public bool m_AutoRotate;
		[HideInInspector]
		public bool m_ApplyStopIK;
		[HideInInspector]
		public float rotateSpeedScale = 1f;
		
		float 		m_MoveSpeed = 5f;
		Vector3 	m_Target;
		MoveType 	m_MoveType;
		bool		m_EndFlag;
		Vector3		m_LastMoveDir;
		Vector3		m_LastVelocity;
		bool		m_InputMove;
		Vector3		m_LookDir;
		bool		m_RestLookDir;
		float		m_SpeedScale;

		public void SetLookDir(Vector3 lookDir)
		{
			m_RestLookDir = true;
			m_LookDir = lookDir;
		}

		public override bool CanDoAction (PEActionParam para = null)
		{			
			PEActionParamNV paramNV = para as PEActionParamNV;
			MoveType moveType = (MoveType)paramNV.n;
			Vector3 target = paramNV.vec;
			if (moveType == MoveType.Direction) {
				if (target == Vector3.zero)
					return false;
			} 
			else 
			{
				if(Vector3.SqrMagnitude(trans.position - target) < MoveParam.AutoMoveStopSqrDis * MoveParam.AutoMoveStopSqrDis)
					return false;
			}
			return true;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == phyCtrl || null == anim || null == anim.animator)
				return;

			Animator animator = anim.animator;
			PEActionParamNV paramNV = para as PEActionParamNV;
			m_MoveType = (MoveType)paramNV.n;
			m_Target = paramNV.vec;
			m_EndFlag = false;
			phyCtrl.ResetSpeed(m_MoveSpeed);
			m_LastVelocity = phyCtrl.desiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
			m_RestLookDir = false;
			Vector3 LocalkMoveSpeed = Quaternion.Inverse(trans.rotation) * m_LastVelocity * m_MoveSpeed;
			animator.SetFloat("ForwardSpeed", LocalkMoveSpeed.z);
			animator.SetFloat("RightSpeed", LocalkMoveSpeed.x);
			animator.SetBool("StartMove", true);
			if(null == m_Param && null != trans)
				m_SpeedScale = m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle);
			else
				m_SpeedScale = 0;
		}
		
		public override void ResetAction (PEActionParam para)
		{
			PEActionParamNV paramNV = para as PEActionParamNV;
			m_MoveType = (MoveType)paramNV.n;
			m_Target = paramNV.vec;
		}
		
		public override void PauseAction ()
		{
			if(null != anim && null != anim.animator)
				anim.animator.SetBool("StartMove", false);
			
			if(null != phyCtrl)
				phyCtrl.desiredMovementDirection = Vector3.zero;
		}
		
		public override void ContinueAction ()
		{
			if(null != anim && null != anim.animator)
				anim.animator.SetBool("StartMove", true);
			if(null != phyCtrl)
				m_LastVelocity = phyCtrl.desiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
		}
		
		public override bool Update ()
		{
			if(null == phyCtrl || null == anim || null == m_Param || m_EndFlag)
			{
				EndImmediately();
				return true;
			}
			if(pauseAction)
				return false;
			
			Vector3 wantDirInWorld = Vector3.zero;
			switch(m_MoveType)
			{
			case MoveType.Direction:
				if(m_Target != Vector3.zero)
					m_Target.Normalize();
				wantDirInWorld = m_Target;
				break;
			case MoveType.Target:
				Vector3 dir = m_Target - trans.position;
				if(dir.sqrMagnitude > MoveParam.AutoMoveStopSqrDis * MoveParam.AutoMoveStopSqrDis)
				{
					wantDirInWorld = dir.normalized;
				}
				else
					EndAction();
				break;
			}
			
			if(m_AutoRotate)
			{
				if(wantDirInWorld != Vector3.zero)
				{
					Vector3 rotateDir = wantDirInWorld;
					if(!phyCtrl.spineInWater)
						rotateDir = Vector3.ProjectOnPlane(rotateDir, Vector3.up).normalized;

					float angle = Vector3.Angle(trans.forward, rotateDir);
					if(angle > 150f)
						rotateDir = Vector3.Slerp(trans.forward,trans.existent.right, rotateSpeedScale * m_Param.m_MoveRotateSpeed * Time.deltaTime);
					else
						rotateDir = Vector3.Slerp(trans.forward, rotateDir, rotateSpeedScale * m_Param.m_MoveRotateSpeed * Time.deltaTime);
					trans.rotation = Quaternion.LookRotation(rotateDir, Vector3.up);
				}
				else
				{
					if(phyCtrl.spineInWater)
					{
						Vector3 rotateDir = Vector3.Slerp(trans.forward, Vector3.ProjectOnPlane(trans.forward, Vector3.up).normalized,
						                                  rotateSpeedScale * m_Param.m_MoveRotateSpeed * Time.deltaTime);
						trans.rotation = Quaternion.LookRotation(rotateDir, Vector3.up);
					}
				}

			}
			else if(m_RestLookDir)
			{
				if(m_LookDir != Vector3.zero){
					Vector3 levelDir = Vector3.ProjectOnPlane(m_LookDir, Vector3.up).normalized;
					trans.rotation = Quaternion.LookRotation(levelDir, Vector3.up);
				}
			}
			
			if(wantDirInWorld != Vector3.zero)
			{
				wantDirInWorld = Quaternion.AngleAxis(-phyCtrl.forwardGroundAngle, trans.existent.right) * wantDirInWorld;

				if(phyCtrl.spineInWater)
					m_SpeedScale = Mathf.Lerp(m_SpeedScale, 1f, m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
				else
					m_SpeedScale = Mathf.Lerp(m_SpeedScale, m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle)
					                          , m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
				phyCtrl.ResetSpeed(m_MoveSpeed * m_SpeedScale);
			}

			if(motionMgr.isInAimState)
				m_LastVelocity = wantDirInWorld;
			else
				m_LastVelocity = Vector3.Lerp(m_LastVelocity, wantDirInWorld, (m_ApplyStopIK?1f:3f) * m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);

			
			Animator animator = anim.animator;
			if(null != animator)
			{
				phyCtrl.desiredMovementDirection = m_LastVelocity * animator.GetFloat("RunSpeedF");
				Vector3 LocalkMoveSpeed = Quaternion.Inverse(trans.rotation) * wantDirInWorld * m_MoveSpeed;
				animator.SetFloat("ForwardSpeed", Mathf.Lerp(animator.GetFloat("ForwardSpeed"), LocalkMoveSpeed.z * phyCtrl.netMoveSpeedScale, motionMgr.isInAimState ? 1f : Motion_Move_Human.AnimLerpSpeed * Time.deltaTime));
				animator.SetFloat("RightSpeed", Mathf.Lerp(animator.GetFloat("RightSpeed"), LocalkMoveSpeed.x, motionMgr.isInAimState ? 1f : Motion_Move_Human.AnimLerpSpeed * Time.deltaTime));
			}
			return false;
		}
		
		public override void EndAction ()
		{
			motionMgr.DoAction(PEActionType.Halt);
			m_Target = Vector3.zero;
			m_EndFlag = true;
			if(null != phyCtrl)
				phyCtrl.desiredMovementDirection = Vector3.zero;
		}
		
		public override void EndImmediately ()
		{
			if(null != anim.animator)
				anim.animator.SetBool("StartMove", false);
			if(null != phyCtrl)
				phyCtrl.desiredMovementDirection = Vector3.zero;
		}

		public void SetWalkState(bool walk)
		{
			m_IsWalk = walk;
			if(m_IsWalk)
				m_MoveSpeed = m_WalkSpeed;
			else
				m_MoveSpeed = m_RunSpeed;
		}

		public float runSpeed
		{
			get{ return m_RunSpeed; }
			set{ m_RunSpeed = value; if(!m_IsWalk) m_MoveSpeed = m_RunSpeed; }
		}

		public float walkSpeed
		{
			get{ return m_WalkSpeed; }
			set{ m_WalkSpeed = value; if(m_IsWalk) m_MoveSpeed = m_WalkSpeed; }
		}
	}
	
	[Serializable]
	public class Action_Sprint : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Sprint; } }

		public HumanPhyCtrl phyCtrl{ get; set; }

		[HideInInspector]
		public MoveParam m_Param;
		[HideInInspector]
		public float m_MoveSpeed = 8f;

		Vector3 	m_Target;
		bool		m_EndFlag;
		bool		m_FastRotat;
		Vector3		m_LastVelocity;
		MoveType	m_MoveType;

		float 		m_SpeedScale;

		[HideInInspector]
		public bool m_ApplyStopIK;
		public bool m_UseStamina;

		public override bool CanDoAction (PEActionParam para = null)
		{
			PEActionParamNVB paramNVB = para as PEActionParamNVB;
			MoveType moveType = (MoveType)paramNVB.n;
			Vector3 target = paramNVB.vec;
			if(m_UseStamina && null != m_Param && motionMgr.Entity.GetAttribute(AttribType.Stamina) < m_Param.m_MinStamina)
				return false;
			if(moveType == MoveType.Direction && target == Vector3.zero)
				return false;
			if(moveType == MoveType.Target && Vector3.SqrMagnitude(trans.position - m_Target) < MoveParam.AutoMoveStopSqrDis * MoveParam.AutoMoveStopSqrDis)
				return false;
			return true;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == phyCtrl || null == anim || null == trans)
				return;
			
			PEActionParamNVB paramNVB = para as PEActionParamNVB;
			m_MoveType = (MoveType)paramNVB.n;
			m_Target = paramNVB.vec;
			m_FastRotat = paramNVB.b;
			m_EndFlag = false;
			motionMgr.SetMaskState(PEActionMask.Sprint, true);

			phyCtrl.ResetSpeed(m_MoveSpeed);
			phyCtrl.desiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
			
			Vector3 LocalkMoveSpeed = Quaternion.Inverse(trans.rotation) * phyCtrl.velocity;
			if(null != anim.animator)
			{
				anim.animator.SetFloat("ForwardSpeed", LocalkMoveSpeed.z);
				anim.animator.SetBool("StartMove", true);
			}
			m_SpeedScale = m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle);
		}

		public override void ResetAction (PEActionParam para = null)
		{
			PEActionParamNVB paramNVB = para as PEActionParamNVB;
			m_MoveType = (MoveType)paramNVB.n;
			m_Target = paramNVB.vec;
		}
		
		public override void PauseAction ()
		{
			if(null != phyCtrl)
				phyCtrl.desiredMovementDirection = Vector3.zero;
			if(null != anim && null != anim.animator)
				anim.animator.SetBool("StartMove", false);
		}
		
		public override void ContinueAction ()
		{
			if(null != anim && null != anim.animator)
				anim.animator.SetBool("StartMove", true);
			
			if(null != phyCtrl)
				phyCtrl.desiredMovementDirection = phyCtrl.currentDesiredMovementDirection;
		}
		
		public override bool Update ()
		{
			if(null == phyCtrl || null == anim || null == trans || null == m_Param || m_EndFlag)
			{
				EndImmediately();
				return true;
			}
			if(pauseAction)
				return false;


			if(m_UseStamina)
			{
				float stamina = motionMgr.Entity.GetAttribute(AttribType.Stamina);
				stamina -= m_Param.m_StaminaCostSpeed * motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent) * Time.deltaTime;

				if(stamina <= PETools.PEMath.Epsilon)
				{
					stamina = 0;
					motionMgr.EndImmediately(ActionType);
					return true;
				}
				motionMgr.Entity.SetAttribute(AttribType.Stamina, stamina, false);
			}

			Vector3 wantDirInWorld = Vector3.zero;

			switch(m_MoveType)
			{
			case MoveType.Direction:
				if(m_Target!= Vector3.zero)
					wantDirInWorld = m_Target.normalized;
				break;
			case MoveType.Target:
				wantDirInWorld = m_Target - trans.position;
				if(wantDirInWorld.magnitude < MoveParam.AutoMoveStopSqrDis)
					EndAction();
				if(wantDirInWorld.sqrMagnitude > 1)
					wantDirInWorld.Normalize();
				break;
			}
			
			float rotateScale = 1f;
			if(m_FastRotat)
				rotateScale *= m_Param.m_FastRotatScale;
			
			if(wantDirInWorld != Vector3.zero)
			{
				Vector3 rotateDir = wantDirInWorld.normalized;
				if(!phyCtrl.spineInWater)
					rotateDir = Vector3.ProjectOnPlane(rotateDir, Vector3.up).normalized;
				
				if(Vector3.Angle(trans.forward, rotateDir) > 150f)
					rotateDir = Vector3.Slerp(trans.forward,trans.existent.right, m_Param.m_MoveRotateSpeed * Time.deltaTime);
				else
					rotateDir = Vector3.Slerp(trans.forward, rotateDir, m_Param.m_SprintRotateSpeed * rotateScale * Time.deltaTime);
				trans.rotation = Quaternion.LookRotation(rotateDir, Vector3.up);
			}
			else
			{
				if(phyCtrl.spineInWater)
				{
					Vector3 rotateDir = Vector3.Slerp(trans.forward, Vector3.ProjectOnPlane(trans.forward, Vector3.up).normalized,
					                                  m_Param.m_SprintRotateSpeed * Time.deltaTime);
					trans.rotation = Quaternion.LookRotation(rotateDir, Vector3.up);
				}
			}
			
			Vector3 rotatedTarget = Quaternion.AngleAxis(-phyCtrl.forwardGroundAngle, trans.existent.right) * wantDirInWorld;
			
			Vector3 moveLocalDir = Quaternion.Inverse(trans.rotation) * rotatedTarget.normalized;
			
			if(wantDirInWorld == Vector3.zero)
			{
				moveLocalDir.z = 0;
				moveLocalDir.y = 0;
			}
			else
				moveLocalDir.z = 1f;
			moveLocalDir.x *= m_Param.m_SprintSizeScale;

			if(phyCtrl.spineInWater)
				m_SpeedScale = Mathf.Lerp(m_SpeedScale, 1f, m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
			else
				m_SpeedScale = Mathf.Lerp(m_SpeedScale, m_Param.m_AngleSpeedScale.Evaluate(phyCtrl.forwardGroundAngle),
				                          m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);
			
			if(wantDirInWorld != Vector3.zero)
				phyCtrl.ResetSpeed(m_MoveSpeed * m_SpeedScale);
			
			float speed = m_MoveSpeed * m_SpeedScale * moveLocalDir.magnitude;

			if(motionMgr.isInAimState)
				phyCtrl.desiredMovementDirection = trans.rotation * moveLocalDir;
			else
				phyCtrl.desiredMovementDirection = Vector3.Lerp(phyCtrl.desiredMovementDirection, trans.rotation * moveLocalDir,
				                                                 m_Param.m_SpeedToLerpF.Evaluate(phyCtrl.velocity.magnitude) * Time.deltaTime);

			
			if(null != anim && null != anim.animator)
			{
				Animator animator = anim.animator;
				phyCtrl.desiredMovementDirection  *= animator.GetFloat("RunSpeedF");
				Vector3 LocalkMoveSpeed = Quaternion.Inverse(trans.rotation) * phyCtrl.velocity.normalized	* speed;
				LocalkMoveSpeed.x = 0f;
				animator.SetFloat("ForwardSpeed", Mathf.Lerp(animator.GetFloat("ForwardSpeed"), LocalkMoveSpeed.z, motionMgr.isInAimState ? 1f : Motion_Move_Human.AnimLerpSpeed * Time.deltaTime));
				animator.SetFloat("RightSpeed", Mathf.Lerp(animator.GetFloat("RightSpeed"), LocalkMoveSpeed.x, motionMgr.isInAimState ? 1f : Motion_Move_Human.AnimLerpSpeed * Time.deltaTime));
			}
			return false;
		}
		
		public override void EndAction ()
		{
			motionMgr.DoAction(PEActionType.Halt);
			m_EndFlag = true;
			if(null != anim.animator)
				anim.animator.SetBool("StartMove", false);
			motionMgr.SetMaskState(PEActionMask.Sprint, false);
			if(null != phyCtrl)
				phyCtrl.desiredMovementDirection = Vector3.zero;
		}
		
		public override void EndImmediately ()
		{
			if(null != anim.animator)
				anim.animator.SetBool("StartMove", false);
			motionMgr.SetMaskState(PEActionMask.Sprint, false);
			if(null != phyCtrl)
				phyCtrl.desiredMovementDirection = Vector3.zero;
		}
	}
	
	[Serializable]
	public class Action_Rotate : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Rotate; } }

		public HumanPhyCtrl phyMotor{ get; set; }

		public float m_MinAngle = 3f;
		public float m_AnimRotateAngleLimit = 160f;
		public float m_SpeedThreshold = 1f;
		public float m_SpeedThresholdRun = 3f;
		public float m_AnimRotateSpeedMax = 100f;
		public float m_AnimRotateAcc = 15f;

		public float	m_StartRotateAngle = 2f;
		public float 	m_MaxAngleThreshold = 30f;
		public float	m_RotateAnglePerSecond = 50f;

		private Vector3 m_TargetDir;
		private float	m_AnimRotateSpeed;
		
		Vector3			m_AddDir;
		bool			m_EndRote;
		public bool		m_FullAnimRotate;
		bool			m_CampMaxThreshold;
		
		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null == trans || null == phyMotor)
				return false;
			
			PEActionParamVBB paramVBB = para as PEActionParamVBB;
			Vector3 targetDir = paramVBB.vec;
			if(targetDir == Vector3.zero)
				return false;
			bool fullAnimRotate = paramVBB.b1;
			float angle = Vector3.Angle(targetDir, trans.forward);
			if(angle < m_MinAngle)
				return false;
			float speed = phyMotor.velocity.magnitude;
			if(fullAnimRotate)
			{
				if(!phyMotor.spineInWater && angle > m_AnimRotateAngleLimit 
				   && (/*speed < m_SpeedThreshold || */speed > m_SpeedThresholdRun))
					return true;
			}
			else
			{
				if(angle > m_StartRotateAngle && speed < m_SpeedThreshold)
					return true;
			}
			return false;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == trans || null == anim)
				return;
			PEActionParamVBB paramVBB = para as PEActionParamVBB;
			m_TargetDir = paramVBB.vec;
			if(PETools.PE.PointInWater(trans.position) < 0.5f )
				m_TargetDir = Vector3.ProjectOnPlane(m_TargetDir, Vector3.up);

			if(null == phyMotor)
			{
				trans.rotation = Quaternion.LookRotation(m_TargetDir, Vector3.up);
				return;
			}


			m_FullAnimRotate = paramVBB.b1;
			m_CampMaxThreshold = paramVBB.b2;
			m_TargetDir.Normalize();
			Animator animator = anim.animator;
			if(null != animator)
			{
				animator.SetFloat("RotateSpeed", 0f);
				animator.SetBool("EndRotate", false);
			}
			m_EndRote = false;

			float speed = phyMotor.velocity.magnitude;
			if(m_FullAnimRotate)
			{
				motionMgr.SetMaskState(PEActionMask.Rotate, true);
				if(null != animator)
				{
					animator.SetFloat("RotationAgr", 0);
					animator.SetFloat("RunRotate", (speed > m_SpeedThresholdRun)?1f:0f);
					animator.SetTrigger("RotateAnim");
				}
				m_AnimRotateSpeed = 0;
				motionMgr.EndImmediately(PEActionType.Move);
				motionMgr.EndImmediately(PEActionType.Sprint);
			}
			else
			{
				if(null != animator)
				{
					animator.SetFloat("RotationAgr", 0);
					animator.SetFloat("RunRotate", 0f);
				}
			}
		}

		public override void ResetAction (PEActionParam para = null)
		{
			base.ResetAction (para);
			PEActionParamVBB paramVBB = para as PEActionParamVBB;
			m_TargetDir = paramVBB.vec;
			m_TargetDir.Normalize();
			m_AddDir = m_TargetDir;
			if(m_AddDir.sqrMagnitude > 1f)
				m_AddDir.Normalize();
		}
		
		public override bool Update ()
		{
			if(null == trans || null == anim || null == phyMotor)
				return true;

			Animator animator = anim.animator;
			
			if(m_FullAnimRotate)
			{
				if(m_EndRote)
				{
					motionMgr.SetMaskState(PEActionMask.Rotate, false);
					return true;
				}
				m_AnimRotateSpeed = Mathf.Lerp(m_AnimRotateSpeed, m_AnimRotateSpeedMax * Vector3.Project(m_AddDir, m_TargetDir).magnitude,
				                               m_AnimRotateAcc * Time.deltaTime);
				if(null != animator)
					animator.SetFloat("RotateSpeed", m_AnimRotateSpeed);
				if(!anim.IsInTransition(0))
				{
//					trans.rotation = anim.m_LastRot * trans.rotation;
//					phyMotor.ApplyMoveRequest(Vector3.Project(anim.m_LastMove, m_TargetDir), Time.deltaTime);
					trans.rotation = anim.m_LastRot * trans.rotation;
					Vector3 forward = Vector3.ProjectOnPlane(trans.forward, Vector3.up);
					trans.rotation = Quaternion.LookRotation(forward, Vector3.up);
					phyMotor.ApplyMoveRequest(anim.m_LastMove/Time.deltaTime);
				}
			}
			else
			{
				float angle = Vector3.Angle(trans.forward, m_TargetDir);
				if(m_TargetDir == Vector3.zero || angle < 1f)
				{
					if(null != animator)
						animator.SetFloat("RotationAgr", 0);
					motionMgr.SetMaskState(PEActionMask.Rotate, false);
					return true;
				}

				float rotateAngle = Mathf.Clamp(m_RotateAnglePerSecond * Time.deltaTime, 0, angle);
				if(m_CampMaxThreshold && angle - rotateAngle > m_MaxAngleThreshold)
					rotateAngle = angle - m_MaxAngleThreshold;

				Vector3 fowardDir = Vector3.Slerp(trans.forward, m_TargetDir.normalized, rotateAngle / angle);
				trans.rotation = Quaternion.LookRotation(fowardDir, Vector3.up);
				float rightAngle = Vector3.Angle(trans.existent.right, m_TargetDir);
				if(rightAngle > 90f)
					angle *= -1f;
				if(null != animator)
					animator.SetFloat("RotationAgr", angle);
			}
			
			return false;
		}
		
		public void AnimEvent(string name)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				switch(name)
				{
				case "EndRotate":
					m_EndRote = true;
					break;
				}
			}
		}
		
		public override void EndImmediately ()
		{
			if(null != phyMotor)
				phyMotor.CancelMoveRequest();
			if(m_FullAnimRotate)
			{
				motionMgr.ContinueAction(PEActionType.Move, ActionType);
				motionMgr.ContinueAction(PEActionType.Sprint, ActionType);
			}
			m_TargetDir = trans.forward;
			anim.SetBool("EndRotate", true);
			anim.SetFloat("RotationAgr", 0);
			motionMgr.SetMaskState(PEActionMask.Rotate, false);
		}
	}
	
	[Serializable]
	public class Action_Step : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Step; } }

		public HumanPhyCtrl phyMotor{ get; set; }
		
		public float m_InertiaF = 0.2f;
		public float m_StepTime = 0.6f;
		public float m_ReActiveTime = 0.3f;
		public float m_EndFlagTime = 0.5f;
		public float m_StaminaCost = 10f;
		public float m_InvincibleTime = 0.1f;
		[HideInInspector]
		public bool m_UseStamina;

		float m_StartTime;
		Vector3 m_MoveDir;
		bool m_Invincible;

		public override bool CanDoAction (PEActionParam para = null)
		{
			return CheckStamina();
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == phyMotor)
				return;
			//				float elapseTime = Time.time - m_StartTime;
			motionMgr.SetMaskState(PEActionMask.Step, true);
			m_StartTime = Time.time;
			Vector3 oldDir = phyMotor.velocity;
			if(oldDir.sqrMagnitude > 1f)
				oldDir.Normalize();
			PEActionParamV paramV = para as PEActionParamV;
			m_MoveDir = (paramV.vec + phyMotor.velocity * m_InertiaF).normalized;
			m_MoveDir.Normalize();
			anim.SetTrigger("Step");
			
			Vector3 LocalkMoveSpeed = Quaternion.Inverse(trans.rotation) * m_MoveDir;
			anim.SetFloat("StepForward", LocalkMoveSpeed.z);
			anim.SetFloat("StepRight", LocalkMoveSpeed.x);

			if(null != viewCmpt)
				viewCmpt.ActivateInjured(false);
			m_Invincible = true;

			CostStamina();
		}
		
		public override void ResetAction (PEActionParam para = null)
		{
			if(Time.time - m_StartTime > m_ReActiveTime 
			   && CheckStamina()
			   && !motionMgr.GetMaskState(PEActionMask.InAir))
				DoAction(para);
		}
		
		public override bool Update ()
		{
			float elapseTime = Time.time - m_StartTime;
			if(m_Invincible && m_InvincibleTime < elapseTime)
			{
				m_Invincible = false;
				if(null != viewCmpt)
					viewCmpt.ActivateInjured(true);
			}
			if(elapseTime < m_StepTime)
			{
				if(elapseTime > m_EndFlagTime)
					motionMgr.SetMaskState(PEActionMask.Step, false);
				if(!anim.IsInTransition(0))
				{
					trans.rotation = anim.m_LastRot * trans.rotation;
					if(null != phyMotor)
						phyMotor.ApplyMoveRequest(anim.m_LastMove/Time.deltaTime);
				}
				elapseTime += Time.deltaTime;
				skillCmpt._lastestTimeOfConsumingStamina = Time.time;
				return false;
			}

			OnEndAction();

			return true;
		}
		
		public override void EndImmediately ()
		{
			OnEndAction();
			if(null != phyMotor)
			{
				phyMotor.CancelMoveRequest();
				phyMotor.desiredMovementDirection =  Vector3.zero;
			}
		}

		void OnEndAction()
		{
			motionMgr.SetMaskState(PEActionMask.Step, false);
			if(null != viewCmpt)
				viewCmpt.ActivateInjured(true);
		}

		bool CheckStamina()
		{
			if(m_UseStamina)
				return motionMgr.Entity.GetAttribute(AttribType.Stamina) >= m_StaminaCost * motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent);
			return true;
		}
		
		void CostStamina()
		{
			if(m_UseStamina)
			{
				float stamina = motionMgr.Entity.GetAttribute(AttribType.Stamina);
				stamina = Mathf.Clamp(stamina - m_StaminaCost * motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent), 0, stamina);
				motionMgr.Entity.SetAttribute(AttribType.Stamina, stamina, false);
			}
		}
	}
	
	[Serializable]
	public class Action_Jump : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Jump; } }
		public float	m_JumpHeight = 2f;
		public float	m_JumpMoveF = 0.3f;
		public float	m_JumpDT = 0.1f;
		public float	m_LongJumpSpeed = 3.5f;
		public float 	m_RotateAcc = 5f;
		public float	m_AddGravityTime = 3f;
		public float	m_AddGravityPower = 1f;
		public float	m_JumpEndTime = 3f;

		public float	m_StaminaCost = 10f;
		public float	m_MinSpeed = 3f;

		public float	m_GroundHeight = 0.3f;
		public float	m_CheckRadius = 0.3f;

		public HumanPhyCtrl phyMotor{ get; set; }
		public FullBodyBipedIK fBBIK { get; set; }

		[HideInInspector]
		public bool m_AutoRotate;
		
		Vector3 m_JumpStartVelocity;
		Vector3 m_AddMove;
		bool 	m_EndFlag;
		float	m_ElapseTime;

		Vector3		m_LookDir;
		bool		m_RestLookDir;

		public override bool CanDoAction (PEActionParam para = null)
		{
//			if(null != anim)
//				return !anim.IsInTransition(0);
//			return base.CanDoAction (para);
			return motionMgr.Entity.GetAttribute(AttribType.Stamina) >= m_StaminaCost * motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent);
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == phyMotor || null == trans || null == anim)
				return;
			if(PeGameMgr.IsMulti)
			{
				if(skillCmpt.IsController())
				{
					if(null != entity.netCmpt && null != entity.netCmpt.network)
					{
						SkNetworkInterface net = entity.netCmpt.network as SkNetworkInterface;
						if(null != net)
							net.RequestJump(GameTime.Timer.Second);
					}
				}
				else
				{
					anim.SetTrigger("Jump");
					return;
				}
			}

			motionMgr.Entity.SetAttribute(AttribType.Stamina, motionMgr.Entity.GetAttribute(AttribType.Stamina) - m_StaminaCost * motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent), false);
			if(phyMotor.moveSpeed < m_MinSpeed)
				phyMotor.ResetSpeed(m_MinSpeed);

			m_JumpStartVelocity = Quaternion.Inverse(trans.rotation) * phyMotor.currentDesiredMovementDirection;
			anim.ResetTrigger("EndJump");
			anim.SetTrigger("Jump");
			anim.SetFloat("JumpForward", m_JumpStartVelocity.z * phyMotor.moveSpeed);

			motionMgr.SetMaskState(PEActionMask.Jump, true);
			phyMotor.ApplyImpact(Mathf.Sqrt(20f * m_JumpHeight) * Vector3.up);
			m_EndFlag = false;
			m_ElapseTime = 0;
			m_RestLookDir = false;
			if(null != fBBIK)
				fBBIK.solver.SetIKPositionWeight(0);
		}
		
		public void SetMoveDir(Vector3 dir)
		{
			m_AddMove = dir;
		}
		
		public void SetLookDir(Vector3 lookDir)
		{
			m_RestLookDir = true;
			m_LookDir = lookDir;
		}
		
		public override bool Update ()
		{
//			if(PeGameMgr.IsMulti && !skillCmpt.IsController())
//				return false;

			if(m_EndFlag || null == phyMotor || null == trans)
			{
				OnEndAction();
				return true;
			}

			Animator animator = anim.animator;

			bool ground = CheckGround() || phyMotor.grounded;
			if(m_StaminaCost > PETools.PEMath.Epsilon)
				skillCmpt._lastestTimeOfConsumingStamina = Time.time;
			if(m_ElapseTime > m_JumpDT && (ground || phyMotor.feetInWater))
			{
				if(null != animator)
				{
					if(ground)
						animator.SetBool("OnGround", true);
					animator.SetFloat("JumpForward", m_AddMove.magnitude * animator.GetFloat("JumpForward"));
				}
				phyMotor.desiredMovementDirection = Vector3.zero;
				motionMgr.SetMaskState(PEActionMask.Jump, false);
				if(null != fBBIK)
					fBBIK.solver.SetIKPositionWeight(1f);
				return true;
			}
			else
			{
				m_ElapseTime += Time.deltaTime;
				if(m_ElapseTime > m_AddGravityTime)
					phyMotor.GetComponent<Rigidbody>().AddForce(m_AddGravityPower * Vector3.down, ForceMode.Acceleration);
				phyMotor.desiredMovementDirection = m_AddMove * m_JumpMoveF + trans.rotation * m_JumpStartVelocity * (1f - m_JumpMoveF);
				if(m_AutoRotate)
				{
					if(m_AddMove.sqrMagnitude > PETools.PEMath.Epsilon)
					{
						Vector3 targetDir = Vector3.Slerp(trans.forward, m_AddMove.normalized, m_RotateAcc * m_AddMove.magnitude * Time.deltaTime);
						trans.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
					}
				}
				else if(m_RestLookDir)
					trans.rotation = Quaternion.LookRotation(m_LookDir, Vector3.up);

				if(null != animator)
					animator.SetFloat("JumpForward", Mathf.Lerp(animator.GetFloat("JumpForward")
					                                        , (Quaternion.Inverse(trans.rotation) * phyMotor.velocity).z, 5f * Time.deltaTime));
				m_AddMove = Vector3.zero;
				
				if(m_ElapseTime > m_JumpEndTime)
				{
					motionMgr.SetMaskState(PEActionMask.Jump, false);
					if(null != fBBIK)
						fBBIK.solver.SetIKPositionWeight(1f);
					return true;
				}
			}
			return false;
		}

		public override void EndAction ()
		{
			m_EndFlag = true;
			OnEndAction();
		}
		
		public override void EndImmediately ()
		{
			OnEndAction();
			if(null != anim)
				anim.SetTrigger("EndJump");
		}

		void OnEndAction()
		{
			if(null != phyMotor)
				phyMotor.desiredMovementDirection = Vector3.zero;
			motionMgr.SetMaskState(PEActionMask.Jump, false);
			if(null != fBBIK)
				fBBIK.solver.SetIKPositionWeight(1f);
		}

		bool CheckGround()
		{
			if(null != phyMotor)
			{
				RaycastHit hitInfo;
				if(Physics.CapsuleCast(trans.position + m_CheckRadius * Vector3.up, trans.position + 2 * m_CheckRadius * Vector3.up, m_CheckRadius,
				                       Vector3.down, out hitInfo, m_GroundHeight, phyMotor.m_GroundLayer.value))
				{
					if(hitInfo.distance < m_GroundHeight)
						return true;
				}
			}

			return false;
		}
	}
	
	[Serializable]
	public class Action_Fall : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Fall; } }

		public HumanPhyCtrl phyMotor{ get; set; }

		public float m_RotateAcc = 5f;
		public float m_MinSpeed = 3f;
		
		public AnimationCurve m_MoveAcc;
		public AnimationCurve m_CenterAcc;
		
		Vector3 	m_Target;
		
		public void SetMoveDir(Vector3 dir)
		{
			m_Target = dir;
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			return move.state == MovementState.Air;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == phyMotor || null == anim)
				return;
			if(phyMotor.moveSpeed < m_MinSpeed)
				phyMotor.ResetSpeed(m_MinSpeed);
			phyMotor.desiredMovementDirection = phyMotor.currentDesiredMovementDirection;
			anim.SetTrigger("Fall");
			motionMgr.SetMaskState(PEActionMask.Fall, true);

		}
		
		public override bool Update ()
		{
			if(null == phyMotor || null == anim)
			{
				motionMgr.SetMaskState(PEActionMask.Fall, false);
				return true;
			}
			
			if(phyMotor.grounded || phyMotor.feetInWater)
			{
				anim.ResetTrigger("Fall");
				if(phyMotor.grounded)
					anim.SetBool("OnGround", true);

				anim.SetFloat("JumpForward", m_Target.magnitude * anim.GetFloat("JumpForward"));

				if(null != phyMotor)
					phyMotor.desiredMovementDirection = Vector3.zero;
				motionMgr.SetMaskState(PEActionMask.Fall, false);
				return true;
			}
			else
			{
				if(m_Target != Vector3.zero 
				   && !(motionMgr.GetMaskState(PEActionMask.AimEquipHold) 
				     || motionMgr.GetMaskState(PEActionMask.GunHold) 
				     || motionMgr.GetMaskState(PEActionMask.BowHold)))
				{
					Vector3 targetDir = Vector3.Slerp(trans.forward, m_Target.normalized, m_RotateAcc * m_Target.magnitude * Time.deltaTime);
					trans.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
				}
				if(null != anim.animator)
					anim.animator.SetFloat("JumpForward", Mathf.Lerp(anim.animator.GetFloat("JumpForward")
				                                        , (Quaternion.Inverse(trans.rotation) * phyMotor.velocity).z, 5f * Time.deltaTime));
				phyMotor.desiredMovementDirection = Vector3.Lerp(phyMotor.desiredMovementDirection, m_Target, 5f * Time.deltaTime);
			}
			return false;
		}
		
		public override void EndImmediately ()
		{
			if(null != phyMotor)
				phyMotor.desiredMovementDirection = Vector3.zero;
			motionMgr.SetMaskState(PEActionMask.Fall, false);
			if(null != anim)
				anim.ResetTrigger("Fall");
		}
	}

	[HideInInspector]
	public class Action_Halt : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Halt; } }

		public IKAnimEffectCtrl animEffect{ get; set; }
		public HumanPhyCtrl phyMotor{ get; set; }

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(PeGameMgr.IsMulti && !skillCmpt.IsController())
				return false;
			return !motionMgr.isInAimState;
		}

		public override void DoAction (PEActionParam para = null)
		{
			if(null != animEffect && null != phyMotor)
				animEffect.StopMove(phyMotor.velocity);
			motionMgr.SetMaskState(PEActionMask.Halt, true);
		}

		public override bool Update ()
		{
			if(PeGameMgr.IsMulti && null != skillCmpt && !skillCmpt.IsController())
			{
				motionMgr.SetMaskState(PEActionMask.Halt, false);
				return true;
			}
			if(null != animEffect && null != phyMotor && animEffect.moveEffectRunning)
				return false;
			motionMgr.SetMaskState(PEActionMask.Halt, false);
			return true;
		}

		public override void EndImmediately ()
		{
			if(null != animEffect)
				animEffect.EndMoveEffect();
			motionMgr.SetMaskState(PEActionMask.Halt, false);
		}
	}

    public class Action_Ride : PEAction
    {
        public override PEActionType ActionType { get { return PEActionType.Ride; } }

        string _animName;
        bool _endAction;
        PeEntity _monsterEntity;
        TameMonsterManager _tameMonsterMgr;
        FullBodyBipedIK _fullbodyBipedIK;
        Transform _ridePosTrans;
        int _monsterID;
        bool _enityIsMe = false;
        PERide _ride;
        int _backupIKIterations =1;

        Interaction_Ride m_Interaction;
        public override void DoAction(PEActionParam para = null)
        {

            PEActionParamVQSNS paramVQSNS = para as PEActionParamVQSNS;

            motionMgr.SetMaskState(PEActionMask.Ride, true);
            _endAction = false;

            if (null != trans)
            {
                trans.position = paramVQSNS.vec;
                trans.rotation = paramVQSNS.q;
            }

            if (null != EntityMgr.Instance && paramVQSNS.enitytID > 0)
            {
                _monsterEntity = EntityMgr.Instance.Get(paramVQSNS.enitytID);
            }

            if (entity)
            {
                //lz-2017.02.23 备份玩家原本的IK迭代次数，设置为坐骑配置的迭代次数
                _fullbodyBipedIK = entity.GetComponentInChildren<FullBodyBipedIK>();
                if (_fullbodyBipedIK && null!= _fullbodyBipedIK.solver)
                {
                    _backupIKIterations = _fullbodyBipedIK.solver.iterations;
                    _fullbodyBipedIK.solver.iterations = TameMonsterConfig.instance.IKIterationSize;
                }
            }


            //lz-2017.02.15 保证Operator关系建立
            if (entity && entity.operateCmpt)
            {
                if (_monsterEntity && _monsterEntity.biologyViewCmpt && _monsterEntity.biologyViewCmpt.biologyViewRoot && _monsterEntity.biologyViewCmpt.biologyViewRoot.modelController)
                {
                    PERides rides = _monsterEntity.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<PERides>();
                    if (rides)
                    {
                        if (rides.HasOperater(entity.operateCmpt))
                        {
                            _ride = rides.GetRideByOperater(entity.operateCmpt);
                        }
                        else
                        {
                            _ride = rides.GetUseable();
                            if (null != _ride)
                            {
                                _ride.Operator = entity.operateCmpt;
                                entity.operateCmpt.Operate = _ride;
                            }
                        }
                    }
                }
            }

            //lz-2016.12.22 开始行为关闭碰撞
            viewCmpt.ActivateCollider(false);

            //lz-2017.02.14 释放物理碰撞
            motionMgr.FreezePhyState(GetType(), true);

            //lz-2017.02.21 关闭玩家和怪物GroundIK
            if (null != motionMgr.Entity.IKCmpt)
                motionMgr.Entity.IKCmpt.EnableGroundFBBIK = false;

            if (null != _monsterEntity.IKCmpt)
                _monsterEntity.IKCmpt.ikEnable = false;

            if (_monsterEntity) {  _monsterID = _monsterEntity.Id; }

            //lz-2017.02.14 做动作
            if (null != anim)
            {
                _animName = paramVQSNS.strAnima;
                anim.ResetTrigger("ResetFullBody");
                anim.SetBool(_animName, true);
            }

            //lz-2017.02.14 开启IK
            if (entity && _monsterEntity)
            {
                m_Interaction = new Interaction_Ride();
                m_Interaction.Init(entity.transform, _monsterEntity.transform);
                m_Interaction.StartInteraction();
            }
            else
            {
                Debug.LogFormat("Ride ik open failed! player is null:{0} , monster is null:{1}", (null == entity), (null == _monsterEntity));
            }

            if (null != _monsterEntity && null != _monsterEntity.biologyViewCmpt)
            {
                _ridePosTrans = _monsterEntity.biologyViewCmpt.GetModelTransform(paramVQSNS.boneStr);
            }

            //lz-2017.02.15 如果执行这个行为的entity是我，就走驯服流程
            if (entity && entity.Id == MainPlayer.Instance.entityId)
            {
                if (_ridePosTrans && _monsterEntity)
                {

                    _tameMonsterMgr = entity.gameObject.AddComponent<TameMonsterManager>();
                    //lw:屏蔽驯服过程
                    _tameMonsterMgr.LoadTameSucceed(entity, _monsterEntity, _ridePosTrans, _monsterEntity.monstermountCtrl.ctrlType != ECtrlType.Mount);


                    //if (_monsterEntity.monstermountCtrl.ctrlType == ECtrlType.Mount)
                    //{
                    //    //lz-2017.02.14 已经驯服的加载驯服成功
                    //    _tameMonsterMgr.LoadTameSucceed(entity, _monsterEntity, _ridePosTrans);
                    //}
                    //else
                    //{
                    //    //lz-2017.02.14 没有驯服的开始驯服
                    //    _tameMonsterMgr.StartTame(entity, _monsterEntity, _ridePosTrans);
                    //}

                    //lz-2017.02.17 多人建立坐骑和玩家的关系
                    if (PeGameMgr.IsMulti)
                        PlayerNetwork.RequestAddRideMonster(_monsterID);
                }
                _enityIsMe = true;
            }
            else
                _enityIsMe = false;
        }

        public override bool Update()
        {
            if (!_endAction)
            {
                //lz - 2017.02.16 本地模拟其他玩家移动
                if (!_enityIsMe && null != _ridePosTrans)
                {
                    entity.peTrans.position = _ridePosTrans.position + TameMonsterConfig.instance.IkRideOffset;
                }

                if (_enityIsMe)
                { 
                    //lz-2017.02.17 特殊情况结束行为
                    if (null == _monsterEntity || _monsterEntity.IsDeath() || _monsterEntity.isRagdoll || null == entity || entity.IsDeath() || entity.isRagdoll)
                    {
                        if (entity && _ride) _ride.StopOperate(entity.operateCmpt, EOperationMask.Ride);
                    }
                }
            }
            return false;
        }

        public override void EndImmediately()
        {
            if (null != anim)
                anim.SetTrigger("ResetFullBody");
            OnEndAction();
        }

        void OnEndAction()
        {
            //lz-2017.02.14 任何情况下结束行为，都把Operate的关系重置
            if (entity && entity.operateCmpt) entity.operateCmpt.Operate = null;
            if (_ride) _ride.Operator = null;

            //lz-2017.02.15 如果执行这个行为的entity是我，就结束驯服流程
            if (_enityIsMe)
            {
                if (_tameMonsterMgr)
                {
                    _tameMonsterMgr.ResetInfo();
                    GameObject.Destroy(_tameMonsterMgr);
                }

                //lz-2017.02.17 是自己就结束行为就解散坐骑
                if (entity && entity.mountCmpt) entity.mountCmpt.DelMount();

                //lz-2017.02.17 多人解散
                if (PeGameMgr.IsMulti)
                    PlayerNetwork.RequestDelMountMonster(_monsterID);
            }

            //lz-2017.02.23 恢复原本的IK迭代次数
            if (_fullbodyBipedIK && null != _fullbodyBipedIK.solver)
            {
                _fullbodyBipedIK.solver.iterations = _backupIKIterations;
            }

            //lz-2017.02.14 结束IK
            if (null != m_Interaction)
            {
                m_Interaction.EndInteraction();
                m_Interaction = null;
            }

            //lz-2017.02.14 关闭动作
            if (null != anim)
                anim.SetBool(_animName, false);

            //lz-2017.02.16 如果是我自己就下来
            if (_enityIsMe)
            {
                if (trans && _monsterEntity && _monsterEntity.peTrans && _monsterEntity.peTrans.realTrans)
                {
                    //Vector3 standPos = Vector3.zero;

                    //lw:在坐骑背上下坐骑
                    trans.position = new Vector3(trans.position.x, trans.position.y + _monsterEntity.peTrans.bound.size.y * 0.5f, trans.position.z);

                    ////lz-2016.12.30 先尝试使用第一种方法找一安全的下坐骑的位置
                    //if (PETools.PEUtil.GetNearbySafetyPos(_monsterEntity.peTrans, PETools.PEUtil.GetOffRideMask, PETools.PEUtil.Standlayer, ref standPos))
                    //{
                    //    trans.position = standPos;
                    //}
                    //else
                    //{
                    //    //lz-2016.12.30 如果第一种没找到就使用第二种
                    //    float minRadus = Mathf.Max(_monsterEntity.peTrans.bound.size.x, _monsterEntity.peTrans.bound.size.z);
                    //    standPos = PETools.PEUtil.GetEmptyPositionOnGround(_monsterEntity.peTrans.position + _monsterEntity.peTrans.bound.center, minRadus, minRadus + 3);


                    //    trans.position = standPos;
                    //}
                }
            }

            motionMgr.FreezePhyState(GetType(), false);

            motionMgr.SetMaskState(PEActionMask.Ride, false);

            //lz-2017.02.21  关闭玩家和怪物GroundIK
            if (null != motionMgr.Entity.IKCmpt)
                motionMgr.Entity.IKCmpt.EnableGroundFBBIK = true;

            if (null != _monsterEntity.IKCmpt)
                _monsterEntity.IKCmpt.ikEnable = true;

            //lz-2016.12.22 结束行为关闭碰撞
            viewCmpt.ActivateCollider(true);

            _endAction = true;
        }
    }
}
