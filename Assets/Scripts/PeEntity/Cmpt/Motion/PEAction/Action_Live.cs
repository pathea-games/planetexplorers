using UnityEngine;
using System;

namespace Pathea
{
	[Serializable]
	public class Action_Sleep : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Sleep; } }

		public event Action<int>		startSleepEvt;
		public event Action<int>		endSleepEvt;

		public HumanPhyCtrl		m_PhyCtrl;
		public float			m_MoveTime = 0.01f;


		float		m_MoveElapseTime;
		Vector3		m_StartPos;
		Quaternion	m_StartRot;
		Vector3 	m_TargetPos;
		Quaternion	m_TargetRot;
		int			m_BuffID;

		string		m_AnimName;
		bool 		m_EndAnim;
		bool 		m_EndAction;

		public override void PreDoAction ()
		{
			base.PreDoAction ();
			motionMgr.SetMaskState(PEActionMask.Sleep, true);
		}

		public override void DoAction (PEActionParam para = null)
		{
			PEActionParamVQNS paramVQNS = para as PEActionParamVQNS;

			if(null != trans)
			{
				m_StartPos = trans.position;
				m_StartRot = trans.rotation;
				m_TargetPos = paramVQNS.vec;
				m_TargetRot = paramVQNS.q;		
				m_MoveElapseTime = 0;
			}
			m_AnimName = paramVQNS.str;
			if(null != anim)
			{
				anim.SetBool(m_AnimName, true);
				anim.ResetTrigger("ResetFullBody");
			}


			m_BuffID = paramVQNS.n;

			if(null != skillCmpt && 0 != m_BuffID)
				SkAliveEntity.MountBuff(skillCmpt, m_BuffID, new System.Collections.Generic.List<int>(), new System.Collections.Generic.List<float>());

			m_EndAnim = false;
			m_EndAction = false;

			if(null != ikCmpt)
				ikCmpt.ikEnable = false;

			if(null != m_PhyCtrl)
			{
				m_PhyCtrl.velocity = Vector3.zero;
				m_PhyCtrl.CancelMoveRequest();
			}

			if(null != startSleepEvt)
				startSleepEvt(m_BuffID);
		}

		public override void EndAction ()
		{
			m_EndAction = true;
			if(null != anim)
			{
				anim.SetBool(m_AnimName, false);
			}
			if(null != skillCmpt && 0 != m_BuffID)
				skillCmpt.CancelBuffById(m_BuffID);
			
			if(null != endSleepEvt)
				endSleepEvt(m_BuffID);
		}

		public override bool Update ()
		{
			if(null != trans)
			{
				if(m_MoveElapseTime < m_MoveTime)
				{
					m_MoveElapseTime = Mathf.Clamp(m_MoveElapseTime + Time.deltaTime, 0, m_MoveTime);
					if(m_MoveElapseTime > m_MoveTime)
						m_MoveElapseTime = m_MoveTime;
					trans.position = Vector3.Lerp(m_StartPos, m_TargetPos, m_MoveElapseTime / m_MoveTime);
					trans.rotation = Quaternion.Lerp(m_StartRot, m_TargetRot, m_MoveElapseTime / m_MoveTime);
				}
			}

			if( m_EndAction && (m_EndAnim || null == anim.animator))
			{
				OnEndAction();
				return true;
			}
			if(!m_EndAction)
				CheckOperateEnd ();
			return false;
		}

		void CheckOperateEnd()
		{
			if (MainPlayer.Instance.entity == motionMgr.Entity
				&& !entity.operateCmpt.HasOperate 
			    && (!PeGameMgr.IsMulti || skillCmpt.IsController()))
				motionMgr.EndImmediately (ActionType);
		}

		public override void EndImmediately ()
		{
			if(null != anim)
			{
				anim.SetTrigger("ResetFullBody");
				anim.SetBool(m_AnimName, false);
			}
			
			if(null != endSleepEvt)
				endSleepEvt(m_BuffID);
			OnEndAction();
		}

		void OnEndAction()
		{
			if(null != skillCmpt && 0 != m_BuffID)
				skillCmpt.CancelBuffById(m_BuffID);
			
			if(null != ikCmpt)
				ikCmpt.ikEnable = true;
			
			motionMgr.SetMaskState(PEActionMask.Sleep, false);

			if(null != motionMgr.Entity.operateCmpt && motionMgr.Entity.operateCmpt.Operate is Pathea.Operate.PESleep)
				motionMgr.Entity.operateCmpt.Operate.StopOperate(motionMgr.Entity.operateCmpt, Pathea.Operate.EOperationMask.Sleep);
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				if("EndSleep" == eventParam)
				{
					m_EndAnim = true;
				}
			}
		}
	}
	
	[Serializable]
	public class Action_Eat : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Eat; } }

		public float	m_MoveTime = 0.01f;
		
		float		m_MoveElapseTime;
		Vector3		m_StartPos;
		Quaternion	m_StartRot;
		Vector3 	m_TargetPos;
		Quaternion	m_TargetRot;
		bool 		m_EndAnim;
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null != anim)
			{
				anim.SetTrigger("Eat");
				anim.ResetTrigger("ResetUpbody");
			}
			
			if(null != trans)
			{
				m_StartPos = trans.position;
				m_StartRot = trans.rotation;
				m_TargetPos = trans.position;
				m_TargetRot = trans.rotation;				
				m_MoveElapseTime = 0;
			}

			m_EndAnim = false;
			motionMgr.SetMaskState(PEActionMask.Eat, true);
		}
		
		public override bool Update ()
		{			
			if(null != trans)
			{
				if(m_MoveElapseTime < m_MoveTime)
				{
					m_MoveElapseTime = Mathf.Clamp(m_MoveElapseTime + Time.deltaTime, 0, m_MoveTime);
					if(m_MoveElapseTime > m_MoveTime)
						m_MoveElapseTime = m_MoveTime;
					trans.position = Vector3.Lerp(m_StartPos, m_TargetPos, m_MoveElapseTime / m_MoveTime);
					trans.rotation = Quaternion.Lerp(m_StartRot, m_TargetRot, m_MoveElapseTime / m_MoveTime);
				}
			}
			
			if(null != anim)
			{
				if(m_EndAnim)
					motionMgr.SetMaskState(PEActionMask.Eat, false);
				return m_EndAnim;
			}
			else
			{
				motionMgr.SetMaskState(PEActionMask.Eat, false);
				return true;
			}
		}
		
		public override void EndImmediately ()
		{
			if(null != anim)
			{
				anim.SetTrigger("ResetUpbody");
				anim.ResetTrigger("Eat");
			}
			motionMgr.SetMaskState(PEActionMask.Eat, false);
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				if("EatEnd" == eventParam)
				{
					m_EndAnim = true;
				}
			}
		}
	}

	[Serializable]
	public class Action_Sit : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Sit; } }

		string		m_AnimName;
		bool 		m_EndAction;
		int			m_BuffID;

		public override void DoAction (PEActionParam para = null)
		{
			
			PEActionParamVQSN paramVQSN = para as PEActionParamVQSN;

			motionMgr.SetMaskState(PEActionMask.Sit, true);
			m_EndAction  = false;

			if(null != trans)
			{
				trans.position = paramVQSN.vec;
				trans.rotation = paramVQSN.q;
			}

			if(null != anim)
			{
				m_AnimName = paramVQSN.str;
				anim.SetBool(m_AnimName, true);
				anim.ResetTrigger("ResetFullBody");
			}
			m_BuffID = paramVQSN.n;
			if (0 != m_BuffID && null != motionMgr.Entity.skEntity)
				PESkEntity.MountBuff (motionMgr.Entity.skEntity, m_BuffID, new System.Collections.Generic.List<int>(), new System.Collections.Generic.List<float>());
			motionMgr.FreezePhyState (GetType(), true);
			if (null != motionMgr.Entity.IKCmpt)
				motionMgr.Entity.IKCmpt.ikEnable = false;
		}
		
		public override bool Update ()
		{
			if(m_EndAction)
			{
				OnEndAction();
				return true;
			}
			if(!m_EndAction)
				CheckOperateEnd ();
			return false;
		}
		
		void CheckOperateEnd()
		{
			if (MainPlayer.Instance.entity == motionMgr.Entity
				&& !entity.operateCmpt.HasOperate 
			    && (!PeGameMgr.IsMulti || skillCmpt.IsController()))
				motionMgr.EndImmediately (ActionType);
		}
		
		public override void EndAction ()
		{
			m_EndAction = true;
			if(null != anim)
				anim.SetBool(m_AnimName, false);
		}
		
		public override void EndImmediately ()
		{
			if(null != anim)
				anim.SetTrigger("ResetFullBody");
			OnEndAction ();
		}

		void OnEndAction()
		{
			if(null != anim)
				anim.SetBool(m_AnimName, false);
			if (0 != m_BuffID && null != motionMgr.Entity.skEntity)
				motionMgr.Entity.skEntity.CancelBuffById (m_BuffID);
			motionMgr.FreezePhyState (GetType(),false);
			motionMgr.SetMaskState(PEActionMask.Sit, false);
			if (null != motionMgr.Entity.IKCmpt)
				motionMgr.Entity.IKCmpt.ikEnable = true;
		}
	}

	[Serializable]
	public class Action_Operation : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Operation; } }
		
		string		m_AnimName;
		bool 		m_EndAction;

		public override void DoAction (PEActionParam para = null)
		{
			PEActionParamVQS paramVQS = para as PEActionParamVQS;
			motionMgr.SetMaskState(PEActionMask.Operation, true);
			m_EndAction  = false;
			if(null != trans)
			{
				trans.position = paramVQS.vec;
				trans.rotation = paramVQS.q;
			}
			
			if(null != anim)
			{
				m_AnimName = paramVQS.str;
				anim.ResetTrigger("ResetFullBody");
				anim.SetBool(m_AnimName, true);
			}

//			Debug.LogError("Do operation " + m_AnimName);
		}
		
		public override bool Update ()
		{
			if(m_EndAction)
			{
				motionMgr.SetMaskState(PEActionMask.Operation, false);
				return true;
			}
			return false;
		}
		
		public override void EndAction ()
		{
			m_EndAction = true;
			if(null != anim)
				anim.SetBool(m_AnimName, false);
//			Debug.LogError("End operation " + m_AnimName);
		}
		
		public override void EndImmediately ()
		{
			if(null != anim)
			{
				anim.SetTrigger("ResetFullBody");
				anim.SetBool(m_AnimName, false);
//				Debug.LogError("End operation " + m_AnimName);
			}
			motionMgr.SetMaskState(PEActionMask.Operation, false);
		}
	}
	
	[Serializable]
	public class Action_Build : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Build; } }
		
		const string AnimName = "Build";
		bool 		m_EndAction;
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Build, true);
			m_EndAction  = false;
			
			if(null != anim)
			{
				anim.ResetTrigger("ResetUpbody");
				anim.SetBool(AnimName, true);
			}
			motionMgr.Entity.SendMsg(EMsg.Build_BuildMode, true);
		}
		
		public override bool Update ()
		{
			if(m_EndAction)
			{
				motionMgr.SetMaskState(PEActionMask.Build, false);
				motionMgr.Entity.SendMsg(EMsg.Build_BuildMode, false);
				return true;
			}
			return false;
		}
		
		public override void EndAction ()
		{
			m_EndAction = true;
			if(null != anim)
				anim.SetBool(AnimName, false);
		}
		
		public override void EndImmediately ()
		{
			if(null != anim)
			{
				anim.SetTrigger("ResetUpbody");
				anim.SetBool(AnimName, false);
			}
			motionMgr.SetMaskState(PEActionMask.Build, false);
			motionMgr.Entity.SendMsg(EMsg.Build_BuildMode, false);
		}
	}

	
	[Serializable]
	public class Action_Stuned : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Stuned; } }
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Stuned, true);
			
			if(null != anim)
				anim.speed = 0f;
		}
		
		public override bool Update ()
		{
			return false;
		}
		
		public override void EndImmediately ()
		{
			if(null != anim)
				anim.speed = 1f;
			motionMgr.SetMaskState(PEActionMask.Stuned, false);
		}
	}

	[Serializable]
	public class Action_PickUpItem : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.PickUpItem; } }

		bool 		m_EndAnim;

		public override void DoAction (PEActionParam para = null)
		{
			if(null != anim)
			{
				anim.ResetTrigger("ResetFullBody");
				anim.SetTrigger("Gather");
			}
			motionMgr.SetMaskState(PEActionMask.PickUpItem, true);
		}

		public override bool Update ()
		{
			if(null != anim && !m_EndAnim)
				return false;
			
			motionMgr.SetMaskState(PEActionMask.PickUpItem, false);
			return true;
		}

		public override void EndImmediately ()
		{
			if(null != anim)
			{
				anim.SetTrigger("ResetFullBody");
				anim.ResetTrigger("Gather");
			}
			motionMgr.SetMaskState(PEActionMask.PickUpItem, false);
		}

		protected override void OnAnimEvent (string eventParam)
		{			
			if(motionMgr.IsActionRunning(ActionType))
			{
				if("GatherEnd" == eventParam)
				{
					m_EndAnim = true;
				}
			}
		}
	}

	[Serializable]
	public class Action_Death : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Death; } }

		public override void DoAction (PEActionParam para = null)
		{
			if(null != viewCmpt)
			{
                if (!viewCmpt.IsRagdoll)
					viewCmpt.ActivateRagdoll(null, false);
				motionMgr.FreezePhyState(GetType(), true);
			}
			motionMgr.SetMaskState(PEActionMask.Death, true);
			motionMgr.Entity.SendMsg(EMsg.State_Die);
		}

		public override void ResetAction (PEActionParam para = null)
		{
			if(null != viewCmpt)
			{
				if (!viewCmpt.IsRagdoll)
					viewCmpt.ActivateRagdoll(null, false);
			}
		}

		public override bool Update ()
		{
			return false;
		}

		public override void OnModelBuild ()
		{
			if(null != viewCmpt)
			{
				if (!viewCmpt.IsRagdoll)
					viewCmpt.ActivateRagdoll(null, false);
			}
		}

		public override void OnModelDestroy ()
		{
		}
		
		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Death, false);
			motionMgr.FreezePhyState(GetType(), false);
			if (null != viewCmpt) 
			{
				viewCmpt.ActivateInjured (true);
				viewCmpt.DeactivateRagdoll();
			}
		}
	}

	[Serializable]
	public class Action_AlienDeath : Action_Death
	{
		public override PEActionType ActionType { get { return PEActionType.AlienDeath; } }
	}

	[Serializable]
	public class Action_Revive : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Revive; } }

		public override bool CanDoAction (PEActionParam para = null)
		{
			return null == viewCmpt || viewCmpt.IsReadyGetUp();
		}

		public override void DoAction (PEActionParam para = null)
		{
			if(null != skillCmpt)
			{
				if(skillCmpt.GetAttribute(AttribType.Hp) < 1)
					skillCmpt.SetAttribute(AttribType.Hp, 1f);
				skillCmpt.SetAttribute(AttribType.Oxygen, skillCmpt.GetAttribute(AttribType.OxygenMax));
				skillCmpt.SetAttribute(AttribType.Stamina, skillCmpt.GetAttribute(AttribType.StaminaMax));
			}
			
			motionMgr.SetMaskState(PEActionMask.Revive, true);
			motionMgr.FreezePhyState(GetType(), true);
			motionMgr.Entity.SendMsg(EMsg.State_Revive);

			PEActionParamB paramB = para as PEActionParamB;

			bool immediately = paramB.b;

			if(null != viewCmpt)
			{
				viewCmpt.DeactivateRagdoll(immediately);
				viewCmpt.ActivateInjured(false);
			}
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
			motionMgr.SetMaskState(PEActionMask.Revive, false);
			motionMgr.FreezePhyState(GetType(), false);
			if(null != viewCmpt)
				viewCmpt.ActivateInjured(true, 1.5f);
		}
	}

	[Serializable]
	public class Action_Lie : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Lie; } }

		float	m_TimeCount;
		bool 	m_EndAction;

		public override void DoAction (PEActionParam para = null)
		{
			if(null != anim)
				anim.SetBool("Lie", true);
			m_EndAction = false;
			motionMgr.SetMaskState(PEActionMask.Lie, true);
			if(null != ikCmpt)
				ikCmpt.ikEnable = false;
		}

		public override void OnModelBuild ()
		{
			if(null != anim)
				anim.SetBool("Lie", true);
			m_EndAction = false;
			motionMgr.SetMaskState(PEActionMask.Lie, true);
			if(null != ikCmpt)
				ikCmpt.ikEnable = false;
		}

		public override void OnModelDestroy ()
		{

		}


		public override bool Update ()
		{
			if(m_EndAction)
			{
				m_TimeCount -= Time.deltaTime;
				if(m_TimeCount < 0)
				{
					motionMgr.SetMaskState(PEActionMask.Lie, false);
					if(null != ikCmpt)
						ikCmpt.ikEnable = true;
					return true;
				}
			}
			return false;
		}

		public override void EndAction ()
		{
			m_EndAction = true;
			m_TimeCount = 4.5f;
			if(null != anim)
				anim.SetBool("Lie", false);
		}
	}

	[Serializable]
	public class Action_Cutscene : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Cutscene; } }
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.Cutscene, true);
		}

		public override bool Update ()
		{
			return false;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Cutscene, false);
		}
	}
	
	[Serializable]
	public class Action_Cure : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Cure; } }
		
		float	m_TimeCount;
		bool 	m_EndAction;

		string 	m_AnimName;

		Vector3 m_EndPos;
		float	m_EndRotY;

		// vec3 pos1 float rot1 vec3 pos2 float rot2 string animName
		public override void DoAction (PEActionParam para = null)
		{
			PEActionParamVFVFS paramVFVFS = para as PEActionParamVFVFS;
			motionMgr.Entity.position = paramVFVFS.vec1;
			motionMgr.Entity.rotation = Quaternion.Euler(0, paramVFVFS.f1, 0);
			m_EndPos = paramVFVFS.vec2;
			m_EndRotY = paramVFVFS.f2;
			m_AnimName = paramVFVFS.str;

			if(null != anim)
				anim.SetBool(m_AnimName, true);
			m_EndAction = false;
			motionMgr.SetMaskState(PEActionMask.Cure, true);
			if(null != ikCmpt)
				ikCmpt.ikEnable = false;			
			motionMgr.FreezePhyState(GetType(), true);
		}
		
		public override void OnModelDestroy ()
		{
			
		}
		
		public override bool Update ()
		{
			if(m_EndAction)
			{
				m_TimeCount -= Time.deltaTime;
				if(m_TimeCount < 0)
				{
					OnEndCure();
					return true;
				}
			}
			return false;
		}
		
		public override void EndAction ()
		{
			m_EndAction = true;
			m_TimeCount = 0;
			if(null != anim)
				anim.SetBool(m_AnimName, false);
		}

		public override void EndImmediately ()
		{
			if(null != anim)
				anim.SetTrigger("ResetFullBody");
			OnEndCure();
		}

		void OnEndCure()
		{
			motionMgr.SetMaskState(PEActionMask.Cure, false);
			if(null != anim)
				anim.SetBool(m_AnimName, false);
			
			if(null != ikCmpt)
				ikCmpt.ikEnable = true;

			motionMgr.FreezePhyState(GetType(), false);
			
			motionMgr.Entity.position = m_EndPos;
			motionMgr.Entity.rotation = Quaternion.Euler(0, m_EndRotY, 0);
		}
	}
}
