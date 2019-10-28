using UnityEngine;
using System.Collections;
using Pathea;

public class TRStun : Trajectory
{
	MotionMgrCmpt m_MotionMgr;
	public override void SetData (Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData (caster, emitter, target, targetPosition, index);
		Stun();
	}

	public override Vector3 Track (float deltaTime)
	{
		Vector3 moveDir = Vector3.zero;
		if(null != m_Target)
			moveDir = GetTargetCenter(m_Target) - transform.position;

		return moveDir;
	}

	void Stun()
	{
		if(null != m_Target)
		{
			PeEntity entity = m_Target.GetComponentInParent<PeEntity>();
			if(null != entity && entity.proto != EEntityProto.Monster)
			{
				m_MotionMgr = entity.GetComponent<MotionMgrCmpt>();
				if(null != m_MotionMgr)
				{
					m_MotionMgr.DoAction(PEActionType.Stuned);
					return;
				}
			}
		}

		GameObject.Destroy(gameObject);
	}


	void OnDestroy()
	{
		if(null != m_MotionMgr)
			m_MotionMgr.EndImmediately(PEActionType.Stuned);
	}
}
