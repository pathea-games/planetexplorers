using UnityEngine;
using System;
using RootMotion.FinalIK;
using PEIK;

namespace Pathea
{
	[Serializable]
	public class Action_Whacked : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Whacked; } }

		public override void DoAction (PEActionParam para = null)
		{
			if(null != anim)
				anim.SetTrigger("BeHit");
			motionMgr.SetMaskState(PEActionMask.Whacked, true);
		}

		public override bool Update ()
		{
			if(null == anim || null == anim.animator)
				return true;
			return false;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Whacked, false);
			if(null != anim)
			{
				anim.SetTrigger("ResetFullBody");
				anim.ResetTrigger("BeHit");
			}
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(eventParam == "HitAnimEnd")
				motionMgr.EndImmediately(ActionType);
		}
	}
	
	[Serializable]
	public class Action_Repulsed : PEAction
	{
		[HideInInspector]
		public Motion_Move_Motor	m_Move;
		public BehaveCmpt		m_Behave;
		public HumanPhyCtrl 	phyCtrl{ get; set; }
		
		public AnimationCurve 	m_TimeVelocityScale;
		public AnimationCurve 	m_ForceToVelocity;
		public AnimationCurve 	m_ForceToMoveTime;
		public AnimationCurve 	m_ApplyMoveStopTime;

		[HideInInspector]
		public BeatParam 		m_Param;
		
		Vector3	m_MoveDir;
		float	m_MoveVelocity;
		float	m_FinalVelocity;
		float	m_MoveTime;
		float 	m_ElapseTime;
		float	m_MoveStopTime;
		
		bool	m_ApplyStopEffect;
		
		public override PEActionType ActionType { get { return PEActionType.Repulsed; } }

		static readonly float BlockForceScale = 0.5f;
		static readonly float LerpScale = 5f;
		
		// Change to Wentfly
		public AnimationCurve 	m_WentflyTimeCurve;
		public float			m_ForceScale = 1.5f;
		public string			m_ApplayForceBoneName = "Bip01 Spine2";
		float 					m_WentflyTime;
		float 					m_ForcePower;
		Transform				m_AddForceTrans;
		bool					m_IsBlockRepulsed;
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == trans || null == m_Param)
				return;

			m_IsBlockRepulsed = motionMgr.IsActionRunning(PEActionType.HoldShield) && Vector3.Angle(-entity.forward, m_MoveDir) < 30f;
			
			PEActionParamVVF paramVVF = PEActionParamVVF.param;
			motionMgr.SetMaskState(PEActionMask.Repulsed, true);
			trans.position = (trans.position + paramVVF.vec1) / 2f;
			m_MoveDir = paramVVF.vec2;
			m_MoveDir = Vector3.ProjectOnPlane(m_MoveDir, trans.existent.up).normalized;
			m_ForcePower = paramVVF.f * (m_IsBlockRepulsed ? BlockForceScale : 1f);
			m_AddForceTrans = m_Param.m_ApplyWentflyBone;
			
			m_MoveVelocity = m_Param.m_ForceToVelocity.Evaluate(m_ForcePower);
			m_MoveTime = m_Param.m_ForceToMoveTime.Evaluate(m_ForcePower);
			m_MoveStopTime = m_Param.m_ApplyMoveStopTime.Evaluate(m_ForcePower);
			m_WentflyTime = m_Param.m_WentflyTimeCurve.Evaluate(m_ForcePower);
			m_ElapseTime = 0f;
			m_ApplyStopEffect = false;

			if(null != anim)
				anim.SetFloat("SheildBlockF", m_IsBlockRepulsed ? 1f: 0f);

			if(null != m_Move)
				m_Move.ApplyForce(m_MoveVelocity * m_MoveDir, ForceMode.VelocityChange);
		}
		
		public override bool Update ()
		{
			if(null == trans || null == m_Param)
			{
				EndImmediately();
				return true;
			}

			if(null != m_AddForceTrans && m_ElapseTime > m_WentflyTime)
			{
				PEActionParamVFNS param = PEActionParamVFNS.param;
				param.vec = m_MoveDir.normalized;
				param.f = m_Param.m_ToWentflyForceScale * m_ForcePower;
				param.n = motionMgr.Entity.Id;
				param.str = m_AddForceTrans.name;
				motionMgr.DoAction(PEActionType.Wentfly, param);
				EndImmediately();
				return true;
			}
			
			if(m_ElapseTime < m_MoveTime)
			{
				float velocityScale = 1f;
				if(null != m_Param.m_TimeVelocityScale)
					velocityScale = m_Param.m_TimeVelocityScale.Evaluate(m_ElapseTime / m_MoveTime);
				m_FinalVelocity = m_MoveVelocity * velocityScale;

				if(null != phyCtrl)
					phyCtrl.ApplyMoveRequest(m_MoveDir * m_FinalVelocity);
				if(null != m_Move)
					m_Move.ApplyForce(m_MoveDir * m_FinalVelocity, ForceMode.VelocityChange);
				
				if(!m_ApplyStopEffect && m_ElapseTime > m_MoveStopTime)
				{
					m_ApplyStopEffect = true;
					motionMgr.DoAction(PEActionType.Halt);
				}
				m_ElapseTime += Time.deltaTime;
				
				if(null != anim && null != phyCtrl)
				{
					Vector3 LocalkMoveSpeed = Quaternion.Inverse(trans.rotation) * phyCtrl.velocity;
					anim.SetFloat("ForwardSpeed", LocalkMoveSpeed.z);
					anim.SetFloat("RightSpeed", LocalkMoveSpeed.x);
					anim.SetFloat("BeatWeight", Mathf.Lerp(anim.GetFloat("BeatWeight"), 1f, LerpScale * Time.deltaTime));
					anim.SetFloat("SheildBlockF", Mathf.Lerp(anim.GetFloat("SheildBlockF"), m_IsBlockRepulsed ? 1f: 0f, LerpScale * Time.deltaTime));
				}
				
				return false;
			}
			
			EndImmediately ();
			return true;
		}
		
		public override void EndImmediately ()
		{
			if(null != anim)
			{
				anim.SetFloat("BeatWeight", 0);
				anim.SetFloat("SheildBlockF", 0f);
			}
			motionMgr.SetMaskState(PEActionMask.Repulsed, false);
			m_ElapseTime = m_MoveTime;
			if(null != phyCtrl)
			{
				phyCtrl.desiredMovementDirection = Vector3.zero;
				phyCtrl.CancelMoveRequest();
			}
			if(null != m_Move)
				m_Move.ApplyForce(Vector3.zero, ForceMode.VelocityChange);
		}
	}
	
	[Serializable]
	public class Action_Wentfly : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Wentfly; } }

		public HumanPhyCtrl 	phyCtrl{ get; set; }
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == viewCmpt)
				return;
			motionMgr.SetMaskState(PEActionMask.Wentfly, true);

			PEActionParamVFNS paramVFNS = para as PEActionParamVFNS;

			Vector3 force = paramVFNS.vec * paramVFNS.f;

			PeEntity entity = EntityMgr.Instance.Get(paramVFNS.n);
			
			motionMgr.FreezePhyState(GetType(), true);

			if(null != entity)
			{
				Transform hitColTran = entity.biologyViewCmpt.GetRagdollTransform(paramVFNS.str);
				if(null != hitColTran)
				{
					RagdollHitInfo hitInfo = new RagdollHitInfo();
					hitInfo.hitTransform = hitColTran;
					hitInfo.hitPoint = hitColTran.position;
					hitInfo.hitForce = force;
					hitInfo.hitNormal = -force.normalized;
					viewCmpt.ActivateRagdoll(hitInfo, false);
				}
			}
			if(null != phyCtrl)
			{
				phyCtrl.desiredMovementDirection = Vector3.zero;
				phyCtrl.CancelMoveRequest();
			}
		}
		
		public override bool Update ()
		{
			if(null == viewCmpt || !viewCmpt.IsRagdoll)
			{
				if(!viewCmpt.IsRagdoll)
				{
					EndImmediately();
					return true;
				}
			}
			
			if(null == viewCmpt || viewCmpt.IsReadyGetUp())
			{
				OnEndAction();
				motionMgr.DoActionImmediately(PEActionType.GetUp);
				return true;
			}

			return false;
		}

		public override void EndAction ()
		{
			OnEndAction();
			motionMgr.DoActionImmediately(PEActionType.GetUp);
		}
		
		public override void EndImmediately ()
		{
			OnEndAction();
		}

		void OnEndAction()
		{
			motionMgr.SetMaskState(PEActionMask.Wentfly, false);
			motionMgr.FreezePhyState(GetType(), false);
		}
	}
	
	[Serializable]
	public class Action_Knocked : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Knocked; } }

		public HumanPhyCtrl 	phyCtrl{ get; set; }
		public float	m_GetUpTime = 3f;

		float	m_AutoGetUpTime;
		bool 	m_AutoGetUp;

		public override void DoAction (PEActionParam para = null)
		{
			if(null != viewCmpt)
			{
				if(!viewCmpt.IsRagdoll)
					viewCmpt.ActivateRagdoll(null, false);
				viewCmpt.ActivateInjured(false);
			}
			motionMgr.SetMaskState(PEActionMask.Knocked, true);
			motionMgr.FreezePhyState(GetType(), true);

			m_AutoGetUp = true;

			m_AutoGetUpTime = m_GetUpTime;
			if(null != phyCtrl)
			{
				phyCtrl.desiredMovementDirection = Vector3.zero;
				phyCtrl.CancelMoveRequest();
			}
		}
		
		public override bool Update ()
		{
			if(m_AutoGetUp)
			{
				m_AutoGetUpTime -= Time.deltaTime;
				if(m_AutoGetUpTime <= 0)
				{
					m_AutoGetUp = false;
					OnEndAction();
					motionMgr.DoActionImmediately(PEActionType.GetUp);
				}
			}
			return false;
		}
		
		public override void EndImmediately ()
		{
			OnEndAction();
		}

		void OnEndAction()
		{
			motionMgr.FreezePhyState(GetType(), false);
			motionMgr.SetMaskState(PEActionMask.Knocked, false);
			if(null != viewCmpt)
				viewCmpt.ActivateInjured(true, 1f);
		}
	}

	[Serializable]
	public class Action_GetUp : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.GetUp; } }
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null != viewCmpt)
			{
				viewCmpt.DeactivateRagdoll();
				viewCmpt.ActivateInjured(false);
			}
			
			motionMgr.FreezePhyState(GetType(), true);
			motionMgr.SetMaskState(PEActionMask.GetUp, true);
		}

		public override void ResetAction (PEActionParam para)
		{
			DoAction(para);
		}
		
		public override bool Update ()
		{
			if(null != viewCmpt && viewCmpt.IsRagdoll)
				return false;
			
			OnEndAction();
			return true;
		}

		public override void EndImmediately ()
		{
			OnEndAction();
		}

		void OnEndAction()
		{
			motionMgr.SetMaskState(PEActionMask.GetUp, false);
			motionMgr.FreezePhyState(GetType(), false);
			if(null != viewCmpt)
			{
				viewCmpt.DeactivateRagdoll(true);
				viewCmpt.ActivateInjured(true, 1.5f);
			}
		}
	}
}
