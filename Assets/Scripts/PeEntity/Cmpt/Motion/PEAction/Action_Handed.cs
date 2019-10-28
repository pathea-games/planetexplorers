using UnityEngine;
using System;
using PEIK;

namespace Pathea
{
	[Serializable]
	public class Action_Handed : PEAction 
	{
		public override PEActionType ActionType { get { return PEActionType.Handed; } }

		public bool standAnimEnd{ get; set; }
		Vector3 offset = new Vector3(0.4f, 0, 0.2f);
		Vector3 axie = new Vector3(-0.5f, 0, 1f);
		float rotateSpeed = 15f;
		float minFixeDis = 0.25f;
		PeTrans	m_TargetTrans;
		bool m_AnimMatch;
		public override void DoAction (PEActionParam para = null)
		{
			if(null == trans || null == move)
				return;
			PEActionParamN paramN = para as PEActionParamN;
			PeEntity entity = EntityMgr.Instance.Get(paramN.n);
			if(null != entity)
				m_TargetTrans = entity.peTrans;
			if(null == m_TargetTrans)
				return;
			
			m_AnimMatch = false;
			standAnimEnd = false;
			motionMgr.SetMaskState(PEActionMask.Hand, true);
		}

		public override bool Update ()
		{
			if(null == m_TargetTrans || null == move)
				return true;
			if(m_AnimMatch)
			{
				Vector3 targetPos = m_TargetTrans.position + m_TargetTrans.rotation * offset;
				if(!standAnimEnd)
				{
					trans.position = Vector3.Lerp(trans.position, targetPos, 5f * Time.deltaTime);
				}
				else
				{
					Vector3 targetDir = m_TargetTrans.position + m_TargetTrans.rotation * offset - trans.position;
					targetDir.y = 0;
					float dis = targetDir.magnitude;
					if(dis < 0.25f * minFixeDis) targetDir = Vector3.zero;
					float speedScale = Mathf.Clamp01((dis - 0.1f * minFixeDis) / minFixeDis);
					if(null != entity.biologyViewCmpt.monoPhyCtrl) entity.biologyViewCmpt.monoPhyCtrl.netMoveSpeedScale = speedScale;
					trans.rotation = Quaternion.LookRotation(Vector3.Lerp(trans.forward, m_TargetTrans.rotation * axie, rotateSpeed * Time.deltaTime));
					move.Move(targetDir * speedScale, SpeedState.Walk);
				}
			}
			return false;
		}

		public override void EndImmediately ()
		{
			move.Move(Vector3.zero);
			motionMgr.SetMaskState(PEActionMask.Hand, false);
			anim.SetBool("InjuredSitEX", true);
			move.baseMoveStyle = MoveStyle.Abnormal;
			if(null != entity.biologyViewCmpt.monoPhyCtrl) entity.biologyViewCmpt.monoPhyCtrl.netMoveSpeedScale = 1f;
		}

		public void OnHand()
		{
			m_AnimMatch = true;
			if(!anim.GetBool("InjuredSitEX"))
				standAnimEnd = true;
			else
				anim.SetBool("InjuredSitEX", false);
			move.style = MoveStyle.Abnormal;
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType) && "OnStandAnimEnd" == eventParam)
				standAnimEnd = true;
		}
	}
}