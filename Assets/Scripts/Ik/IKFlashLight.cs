using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

[RequireComponent(typeof(AimIK))]
public class IKFlashLight : MonoBehaviour 
{
	AimIK		m_AimIK;
	public 	Transform	m_Root;
	
	[Range(0, 180f)]
	public float		m_AngleThreshold = 40f;
	public bool			m_Active;
	public float		m_FadeTime = 0.5f;
	float 				m_Radius = 10f;
	Vector3 			m_TargetPos = Vector3.zero;

	public Transform	aimTrans
	{
		get { return m_AimIK.solver.transform; }
		set { m_AimIK.solver.transform = value; }
	}
	
	public Vector3		targetPos
	{
		set{ m_TargetPos = value; }
		get{ return m_AimIK.solver.IKPosition; }
	}


	void Awake()
	{
		m_AimIK = GetComponent<AimIK>();
	}

	void UpdateActiveState()
	{
		if(null == aimTrans)
		{
			m_AimIK.solver.IKPositionWeight = 0;
			return;
		}
		if(m_Active)
		{
			if(m_AimIK.solver.IKPositionWeight < 1f)
				m_AimIK.solver.IKPositionWeight = Mathf.Clamp01(m_AimIK.solver.IKPositionWeight + Time.deltaTime / m_FadeTime);
		}
		else
		{
			if(m_AimIK.solver.IKPositionWeight > 0f)
				m_AimIK.solver.IKPositionWeight = Mathf.Clamp01(m_AimIK.solver.IKPositionWeight - Time.deltaTime / m_FadeTime);
		}
	}

	void UpdateIKTarget()
	{
		if(!m_Active || null == aimTrans)
			return;

		if(null != m_Root)
		{
			Vector3 m_TargetDir = m_TargetPos - aimTrans.position;
			if(m_TargetPos == Vector3.zero)
				m_TargetDir = m_Root.forward;
			m_TargetDir.Normalize();
			float angle = Vector3.Angle(m_TargetDir, m_Root.forward);
			if(angle > m_AngleThreshold)
				m_TargetDir = Vector3.Slerp(m_TargetDir, m_Root.forward, (angle - m_AngleThreshold) / angle);
			m_TargetDir.Normalize();
			m_AimIK.solver.IKPosition = aimTrans.position + m_TargetDir * m_Radius;
		}
		else
			m_Active = false;
	}

	void Update()
	{
		UpdateActiveState();
		UpdateIKTarget();
	}
}
