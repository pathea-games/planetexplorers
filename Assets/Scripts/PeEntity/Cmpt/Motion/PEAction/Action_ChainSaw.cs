//using UnityEngine;
//using System;
//
//namespace Pathea
//{
//	[Serializable]
//	public class Action_ChainSawActive : PEAction
//	{
//		public override PEActionType ActionType { get { return PEActionType.ChainSawActive; } }
//
//		PEChainSaw m_ChainSaw;
//		public PEChainSaw chainSaw
//		{
//			get { return m_ChainSaw; }
//			set 
//			{
//				m_ChainSaw = value;
//				if(null == m_ChainSaw)
//					m_MotionMgr.EndImmediately(ActionType);
//			}
//		}
//
//		AnimatorCmpt m_Anim;
//		public AnimatorCmpt anim
//		{
//			get { return m_Anim; }
//			set
//			{
//				m_Anim = value;
//				if(null != m_Anim)
//					m_Anim.AnimEvtString += AnimEvent;
//			}
//		}
//
//		bool m_Active;
//		bool m_EndAction;
//
//		public override void DoAction (IPEActionParam para = null)
//		{
//			if(null != chainSaw && anim.hasAnimator)
//				anim.SetTrigger(chainSaw.activeAnim);
//			else
//				m_Active = true;
//			m_EndAction = false;
//		}
//
//		public override void EndAction ()
//		{
//			m_EndAction = true;
//			if(null != chainSaw && anim.hasAnimator)
//				anim.SetTrigger(chainSaw.deactiveAnim);
//			else
//				m_Active = false;
//		}
//
//		public override void EndImmediately ()
//		{
//			anim.SetTrigger("ResetUpbody");
//			SetActiveState(false);
//		}
//
//		public override bool Update ()
//		{
//			return m_EndAction && !m_Active;
//		}
//
//		public override void OnModelBuild ()
//		{
//			if(m_Active)
//				SetActiveState(true);
//		}
//
//		void SetActiveState(bool active)
//		{
//			m_Active = active;
//			if(null != chainSaw)
//				chainSaw.SetActiveState(m_Active);
//			m_MotionMgr.SetMaskState(PEActionMask.ChainSawActive, m_Active);
//		}
//
//		void AnimEvent(string animEvent)
//		{
//			if(m_MotionMgr.IsActionRunning(ActionType))
//			{
////				switch(animEvent)
////				{
////				case "PutOnAnimEnd":
////					SetActiveState(true);
////					break;
////				case "PutOffAnimEnd":
////					SetActiveState(false);
////					break;
////				}
//				if("SwitchDown" == animEvent)
//				{
//					SetActiveState(!m_EndAction);
//				}
//			}
//		}
//	}
//
//	
//	[Serializable]
//	public class Action_ChainSawDeactive : PEAction
//	{		
//		public override PEActionType ActionType { get { return PEActionType.ChainSawDeactive; } }
//
//		public override void DoAction (IPEActionParam para = null)
//		{
//			base.DoAction (para);
//			m_MotionMgr.EndAction(PEActionType.ChainSawActive);
//		}
//	}
//
//	[Serializable]
//	public class Action_ChainSawAttack : Action_SwordAttack
//	{
//		public override PEActionType ActionType { get { return PEActionType.ChainSawAttack; } }
//	}
//
//	[Serializable]
//	public class Action_ChainSawFell : Action_Fell
//	{
//		public override PEActionType ActionType { get { return PEActionType.ChainSawFell; } }
//		protected override string fellAnim { get { return "ChainSawFell"; } }
//	}
//}
