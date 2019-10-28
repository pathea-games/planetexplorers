using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;

public class Trajectory : MonoBehaviour
{
    [SerializeField]
    protected Transform m_Caster;
    [SerializeField]
    protected Transform m_Emitter;
    [SerializeField]
    protected Transform m_Target;
    [SerializeField]
    protected Vector3 m_TargetPosition;

	[SerializeField] bool m_InitRot;

    protected int m_Index;
	BiologyViewCmpt m_TargetView;
    PeTrans m_TargetTrans;
	PEDefenceTrigger m_DefenceTrigger;
	Vector3 m_MoveVector;
    public Vector3 moveVector { get { return m_MoveVector; } set{ m_MoveVector = value; } }

	protected Vector3 m_Velocity;

    public virtual Vector3 Track(float deltaTime) { return Vector3.zero; }
    public virtual Quaternion Rotate(float deltaTime) { return transform.rotation; }

	public Transform target{ set { m_Target = value; } }

	public bool rayCast = false;

	public bool isActive { get; set; }

    public virtual void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index = 0)
    {
        m_Caster = caster;
        m_Emitter = emitter;
        m_Target = target;
        m_TargetPosition = targetPosition;
        m_Index = index;

        if(m_Target != null)
        {
            m_TargetView = m_Target.GetComponentInParent<BiologyViewCmpt>();
            m_TargetTrans = m_Target.GetComponentInParent<PeTrans>();
			if(null != m_TargetView)
				m_DefenceTrigger = m_TargetView.GetComponentInChildren<PEDefenceTrigger>();
        }

		if(m_InitRot)
			transform.rotation = Quaternion.identity;
		isActive = true;
    }

    protected virtual bool Overlook(Collider c)
    {
        if (c == null || c.isTrigger)
            return true;

        if (c.tag == "WorldCollider")
            return true;

        if (c.transform.IsChildOf(transform))
            return true;

        if (m_Emitter != null && c.transform.IsChildOf(m_Emitter))
            return true;

        return false;
    }

	protected Vector3 GetPredictPosition(Transform target, Vector3 startPos, float startSpeed)
	{
		Vector3 targetCenter = GetTargetCenter(target);
		Vector3 targetVelocity = GetTargetVeloctiy(target);
		float targetV_2 = targetVelocity.sqrMagnitude;

		if(targetV_2 < 0.05f)
		{
			Vector3 reverseVec = startPos - targetCenter;
			float sqrv22mv12 = Mathf.Sqrt(startSpeed * startSpeed - targetV_2);
			float cos2 = Mathf.Cos(Vector3.Angle(reverseVec, targetVelocity) / 180f * Mathf.PI);
			float temp1 = reverseVec.sqrMagnitude * targetV_2 * cos2 * cos2;
			float predictTime;
			if(Vector3.Angle(reverseVec, targetVelocity) <= 90f)
				predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22mv12 / sqrv22mv12) - Mathf.Sqrt(temp1 / sqrv22mv12 / sqrv22mv12)) / sqrv22mv12;
			else
				predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22mv12 / sqrv22mv12) + Mathf.Sqrt(temp1 / sqrv22mv12 / sqrv22mv12)) / sqrv22mv12;
			Vector3 predictPos = targetCenter + targetVelocity * predictTime;
			return predictPos;
		}
		else
			return targetCenter;
	}

    protected Vector3 GetTargetCenter(Transform target = null)
    {
        if (m_Target != null)
        {
			if(null != m_DefenceTrigger && null != m_DefenceTrigger.centerBone)
				return m_DefenceTrigger.centerBone.position;
            else if (m_TargetView != null && m_TargetView.centerTransform != null)
                return m_TargetView.centerTransform.position;
            else if (m_TargetTrans != null)
                return m_TargetTrans.center;
            else
				return m_Target.position;
		}
        else
            return m_TargetPosition;
    }

    protected Vector3 GetTargetVeloctiy(Transform target)
    {
        if (target == null)
            return Vector3.zero;

        Motion_Move mover = target.GetComponentInParent<Motion_Move>();
        if (mover != null)
            return mover.velocity;
        else
            return Vector3.zero;
    }
}