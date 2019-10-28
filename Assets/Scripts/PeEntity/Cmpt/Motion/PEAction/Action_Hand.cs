using UnityEngine;
using System;
using PEIK;

namespace Pathea
{
	[Serializable]
	public class Action_Hand : PEAction 
	{
		public override PEActionType ActionType { get { return PEActionType.Hand; } }

		float startDis = 0.3f;
		float maxDis = 1f;
		float startAngle = 5f;
		Vector3 offset = new Vector3(-0.4f, 0, -0.2f);
		[SerializeField] float tryHandTime = 5f;
		[SerializeField] float rotateScale = 0.2f;
		[SerializeField] int   tryHandCount = 2;
		[SerializeField] float tryRotateAngle = 60f;

		MotionMgrCmpt m_TargetMotion;
		Action_Handed m_TargetAction;
		PeTrans	m_TargetTrans;

		bool m_AnimMatch;
		public bool moveable{ get { return m_AnimMatch && null != m_TargetAction && m_TargetAction.standAnimEnd; } }

		Interaction_Hand m_Interaction;

		Action_Move m_MoveAction;

		float m_StartTime;

		bool m_EndAction;

		int m_CurTryCount;

		public override void DoAction (PEActionParam para = null)
		{
			if(null == trans || null == move)
				return;
			PEActionParamN paramN = para as PEActionParamN;
			PeEntity targetEntity = EntityMgr.Instance.Get(paramN.n);
			if(null != targetEntity)
				m_TargetMotion = targetEntity.motionMgr;
			else
				return;

			if(null != m_TargetMotion)
				m_TargetAction = m_TargetMotion.GetAction<Action_Handed>();
			else
				return;

			m_TargetTrans = targetEntity.peTrans;
			if(null == m_TargetTrans)
				return;

			if(null == m_MoveAction)
				m_MoveAction = motionMgr.GetAction<Action_Move>();

			m_AnimMatch = false;
			motionMgr.SetMaskState(PEActionMask.Hand, true);
			PEActionParamN param = PEActionParamN.param;
			param.n = motionMgr.Entity.Id;
			m_TargetMotion.DoActionImmediately(PEActionType.Handed, param);
			m_EndAction = false;
			m_StartTime = Time.time;
			m_CurTryCount = 0;
			if(PeGameMgr.IsMulti && entity == PeCreature.Instance.mainPlayer)
				targetEntity.netCmpt.network.RPCServer(EPacketType.PT_NPC_RequestAiOp,(int)EReqType.Hand,entity.Id);
		}

		public override bool Update ()
		{
			if(null == trans || null == m_TargetAction || null == m_TargetMotion)
				return true;
			
			Vector3 targetPos = m_TargetTrans.position + m_TargetTrans.rotation * offset;
			float angle = Vector3.Angle(m_TargetTrans.forward, trans.forward);

			if(m_AnimMatch)
			{
				if(Vector3.SqrMagnitude(trans.position - targetPos) > maxDis * maxDis)
				{
					EndAction();
					return true;
				}
			}
			else
			{
				if(Vector3.SqrMagnitude(trans.position - targetPos) > startDis * startDis)
				{
					move.MoveTo(targetPos, SpeedState.Walk);
				}
				else if(angle > startAngle)
				{
					move.RotateTo(m_TargetTrans.forward);
				}
				else
				{
					m_AnimMatch = true;
					m_TargetAction.OnHand();
					m_Interaction = new Interaction_Hand();
					m_Interaction.Init(motionMgr.Entity.transform, m_TargetMotion.Entity.transform);
					m_Interaction.StartInteraction();
					if(null != m_MoveAction)
						m_MoveAction.rotateSpeedScale = rotateScale;
				}

				if(Time.time - m_StartTime > tryHandTime)
				{
					if(m_CurTryCount > tryHandCount)
						EndAction();
					else
					{
						m_StartTime = Time.time;
						m_CurTryCount++;
						m_TargetMotion.Entity.motionMove.RotateTo(Quaternion.AngleAxis(tryRotateAngle, m_TargetTrans.existent.up) * m_TargetTrans.forward);
					}
				}
			}
			return m_EndAction;
		}

		void OnEndAction(bool immediately)
		{
			motionMgr.SetMaskState(PEActionMask.Hand, false);
			
			if(null != m_Interaction)
			{
				m_Interaction.EndInteraction(immediately);
				m_Interaction = null;
			}
			
			if(null != m_TargetMotion)
				m_TargetMotion.EndAction(PEActionType.Handed);
			
			if(null != m_MoveAction)
				m_MoveAction.rotateSpeedScale = 1f;
		}

		public override void EndAction ()
		{
			OnEndAction(false);
			m_EndAction = true;
		}

		public override void EndImmediately ()
		{
			OnEndAction(true);
		}
	}
}