using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

[RequireComponent(typeof(AimIK))]

public class IKAimCtrl : MonoBehaviour 
{
	public float 		m_Weight = 1f;

	public Transform 	m_Target;
	public Transform	m_Root;
	public Transform	m_DetectorCenter;
	float				m_DetectorRadius = 100f;
	float				m_MinDis = 5f;
	Vector3				m_DefaultAxis;
	[Range(0, 180f)]
	public float		m_DetectorAngle = 80f;
	public float		m_LerpSpeed = 3f;

	[Range(0.01f, 5f)]
	public float		m_FadeInTime = 0.5f;
	public float		m_FadeOutTime = 0.05f;
	private float		m_FadeWeight = 0f;
	private AimIK		m_AimIK;
	//private FullBodyBipedIK m_FBBIK;

	private Vector3		m_LastDir;
	private Vector3 	m_TargetDir;
	private Vector3		m_AimPos;

	private Vector3		m_UpdatedAimPos;

	public Vector3		m_TargetPosOffset;
	public Vector3		targetPos
	{
		get
		{
			if(null != m_AimIK)
			{
				Vector3 pos = m_AimPos;
				if(null != m_Target)
					pos = m_UpdatedAimPos;
				return pos + m_ScaledOffset;
			}
			return transform.position + transform.forward;
		}

		set { m_AimPos = value; }
	}

	Vector3 m_ScaledOffset;

	bool m_Active;

	public bool active { get { return m_Active; } }

	public bool aimed { get { return m_FadeWeight < 0.9f || m_InRange; } }

	
	public Ray aimRay { get	{ return new Ray(m_AimIK.solver.transform.position, targetPos - m_TargetPosOffset - m_AimIK.solver.transform.position); } }
	
	[Range(0.01f, 5f)]
	public float	m_SyncAimAxieFadeTime = 0.15f;
	float		m_SyncAimAxieWeight = 0;
	bool 		m_SyncAimAxie;
	Transform	m_FollowTrans;
	Transform 	m_ModelTran;
	Quaternion	m_LocalRot;
	bool 		m_InRange;

	bool		m_UseSyncTarget;

	// Use this for initialization
	void Awake () {
		m_AimIK = GetComponent<AimIK>();
//		m_AimIK.Disable();
		//m_FBBIK = GetComponent<FullBodyBipedIK>();
//		m_FBBIK.Disable();
		m_ModelTran = transform.parent.GetComponentInChildren<PEModelController>().transform;
		m_DetectorCenter = m_AimIK.solver.transform;
		m_DefaultAxis = m_AimIK.solver.axis;
	}

	void Start()
	{
		if(null == m_Root)
		{
			PEModelController mc = transform.parent.GetComponentInChildren<PEModelController>();
			if(null != mc)
				m_Root = mc.transform;
		}
	}

	// Update is called once per frame
	void LateUpdate () 
	{
		UpdateSyncAimAxie();

		UpdateIKPos();
	}

	public void SetTarget(Transform target)
	{
		m_Target = target;
	}

	public void SetActive(bool active)
	{
		m_Active = active;
//		StopAllCoroutines();
//		StartCoroutine(UpdateFade(active));
	}

	public void SetSmoothMoveState(bool smoothMove)
	{
		if(null != m_AimIK && null != m_AimIK.solver)
			m_AimIK.solver.clampSmoothing = smoothMove ? 2 : 0;
	}

	void UpdateIKPos()
	{
		m_AimIK.solver.IKPositionWeight = m_Weight * m_FadeWeight;

		m_FadeWeight = Mathf.Clamp01(m_FadeWeight + (m_Active ? 1f:-1f) * Time.deltaTime / m_FadeInTime);

		m_UpdatedAimPos = (null == m_Target ? m_AimPos : m_Target.position);

		if(m_Active && null != m_DetectorCenter)
		{
			if(null == m_Target)
			{
				m_TargetDir = m_AimPos - m_DetectorCenter.position;
			}
			else
			{
				m_TargetDir = m_Target.position - m_DetectorCenter.position;
				m_UpdatedAimPos = m_Target.position;
			}
			
			if(Vector3.zero == m_TargetDir && null != m_Root)
				m_TargetDir = m_Root.forward;
			
			m_InRange = true;
			
			if(null != m_Root)
			{
				float angle = Vector3.Angle(m_TargetDir, m_Root.forward);
				if(angle > m_DetectorAngle)
				{
					m_TargetDir = Vector3.Slerp(m_TargetDir, m_Root.forward, (angle - m_DetectorAngle) / angle);
					m_InRange = false;
				}
			}
			
			float dis = Mathf.Clamp(m_TargetDir.magnitude, m_MinDis, m_DetectorRadius);
			m_ScaledOffset = m_TargetPosOffset * dis / m_DetectorRadius;
			
			m_TargetDir.Normalize();
			
			m_TargetDir = Vector3.Slerp(m_LastDir, m_TargetDir, m_LerpSpeed * Time.deltaTime);

			if(!m_UseSyncTarget)
				m_AimIK.solver.IKPosition = m_DetectorCenter.position + m_TargetDir * dis + m_ScaledOffset;
			
			m_LastDir = m_TargetDir;
		}
	}

//	IEnumerator UpdateFade(bool active)
//	{
//		if(active)
//		{
//			while(m_FadeWeight < 1f)
//			{
//				m_FadeWeight = Mathf.Clamp01(m_FadeWeight + Time.deltaTime / m_FadeInTime);
//				yield return null;
//			}
//		}
//		else
//		{
//			while(m_FadeWeight > 0)
//			{
//				m_FadeWeight = Mathf.Clamp01(m_FadeWeight - Time.deltaTime / m_FadeInTime);
//				yield return null;
//			}
//		}
//	}

	public void SetAimTran(Transform aimTran)
	{
		if(null != m_FollowTrans)
		{
			m_FollowTrans.localRotation = m_LocalRot;
			m_FollowTrans = null;
		}
		if(null == aimTran)
		{
			m_AimIK.solver.transform = m_DetectorCenter;
			m_AimIK.solver.axis = m_DefaultAxis;
		}
		else
		{
			m_AimIK.solver.transform = aimTran;
			m_AimIK.solver.axis = Vector3.forward;
			m_LocalRot = aimTran.localRotation;
			m_FollowTrans = aimTran;
		}
	}

	public void StartSyncAimAxie()
	{
		m_SyncAimAxie = true;
//		m_FollowTrans = followTrans;
//		SetAimTran(followTrans);
//		LateUpdate();
	}

	public void EndSyncAimAxie()
	{
		m_SyncAimAxie = false;
	}

	public float crossDir = 1f;

	void UpdateSyncAimAxie()
	{
		m_UseSyncTarget = false;
		m_SyncAimAxieWeight = Mathf.Clamp01(m_SyncAimAxieWeight + (m_SyncAimAxie?1f:-1f) * Time.deltaTime / m_SyncAimAxieFadeTime);

		if(m_SyncAimAxieWeight > 0f)
		{
			if(m_SyncAimAxie)
			{
				if(null != m_ModelTran)
				{
					if(null != m_FollowTrans)
						m_FollowTrans.rotation = Quaternion.Slerp(m_FollowTrans.rotation, Quaternion.LookRotation(m_ModelTran.forward), m_SyncAimAxieWeight);
					else
					{
						m_UseSyncTarget = true;
						float targetAngle = Vector3.Angle(m_TargetDir, Vector3.up);
						Vector3 animDir = m_DetectorCenter.rotation * m_DefaultAxis;
						float animAngle = Vector3.Angle(animDir, Vector3.up) + targetAngle - 90f;
						animDir = Quaternion.AngleAxis(animAngle, crossDir * Vector3.Cross(Vector3.up, animDir)) * Vector3.up;
						
						m_AimIK.solver.IKPosition = m_DetectorCenter.position + animDir.normalized * m_MinDis;
					}
				}
			}
			else
			{
				if(null != m_FollowTrans)
					m_FollowTrans.localRotation = Quaternion.Slerp(m_LocalRot, m_FollowTrans.localRotation, m_SyncAimAxieWeight);
			}
		}
		else
		{
			if(null != m_FollowTrans)
				m_AimIK.solver.axis = Vector3.forward;
			else
				m_AimIK.solver.axis = m_DefaultAxis;
		}
	}
}
