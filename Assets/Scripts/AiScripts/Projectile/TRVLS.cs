using UnityEngine;
using System.Collections;
using SkillAsset;

public class TRVLS : Trajectory
{
	[SerializeField] float m_StartSpeed = 5;
	[SerializeField] float m_BalanceSpeed = 50f;
	[SerializeField] float m_AccelerationTime = 2f;
	[SerializeField] float m_TrackStartTime = 1.5f;
	[SerializeField] float m_TrackEndTime = 999f;
	[SerializeField] float m_TrackPower = 0.5f;
	[SerializeField] float m_OffsetRadius = 3f;
	[SerializeField] float m_OffsetPeriod = 1f;
	[SerializeField] float m_RandomF = 0.1f;

	protected Vector3 m_OffsetPhase = Vector3.zero;
	protected float m_PeriodW;
	protected float m_Time;
	protected float m_Acceleration;

	public override void SetData (Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData (caster, emitter, target, targetPosition, index);
		Init();
	}
	
	public void Init()
	{
		m_Velocity = transform.forward * m_StartSpeed;
		m_Acceleration = (m_AccelerationTime > 0) ? ((m_BalanceSpeed - m_StartSpeed) / m_AccelerationTime) : 0;
		InitOffset();
    }


    public override Vector3 Track(float deltaTime)
    {
		UpdateVelocity(deltaTime);
		return m_Velocity * deltaTime;
    }

	void InitOffset()
	{
		m_PeriodW = (m_OffsetPeriod > 0) ? (2 * Mathf.PI / m_OffsetPeriod) : 0;
		m_OffsetPhase.x = 2 * Mathf.PI * Random.value;
		m_OffsetPhase.y = 2 * Mathf.PI * Random.value;
		m_OffsetPhase.z = 2 * Mathf.PI * Random.value;
	}

	protected virtual void UpdateTrack(float deltaTime)
	{
		if(null != m_Target && m_Time > m_TrackStartTime && m_Time < m_TrackEndTime)
		{
			m_OffsetPhase.x += m_PeriodW * Random.value * m_RandomF * deltaTime;
			m_OffsetPhase.y += m_PeriodW * Random.value * m_RandomF * deltaTime;
			m_OffsetPhase.z += m_PeriodW * Random.value * m_RandomF * deltaTime;
			Vector3 offsetPos = m_OffsetRadius * new Vector3(Mathf.Sin(m_OffsetPhase.x), Mathf.Sin(m_OffsetPhase.y), Mathf.Sin(m_OffsetPhase.z));
			Vector3 targetPos = GetTargetCenter(m_Target) + offsetPos;
			
			Vector3 trackVelocity = targetPos - transform.position;
			Vector3 trackAxie = trackVelocity;
			Vector3 velocity = m_Velocity;
			Vector3.OrthoNormalize(ref velocity, ref trackAxie);
			Vector3 projet = Vector3.Project(trackVelocity, trackAxie);
			float trackPowerScale = (m_OffsetRadius > 0) ? Mathf.Clamp01(projet.magnitude/m_OffsetRadius) : 1f;
			projet = projet.normalized * m_TrackPower * trackPowerScale;
			float currentSpeed = m_Velocity.magnitude;
			m_Velocity += projet * deltaTime;
			m_Velocity = m_Velocity.normalized * currentSpeed;
		}
		m_Velocity = m_Velocity.normalized * Mathf.Lerp(m_StartSpeed, m_BalanceSpeed, Mathf.Clamp(m_Time, 0, m_AccelerationTime)  / m_AccelerationTime);
	}

	protected virtual void UpdateAcceleration(float deltaTime)
	{
		if(m_Time < m_AccelerationTime)
			m_Velocity += m_Velocity.normalized * m_Acceleration * deltaTime;
	}

	protected virtual void UpdateVelocity(float deltaTime)
	{
		m_Time += deltaTime;
		UpdateAcceleration(deltaTime);
		UpdateTrack(deltaTime);
		transform.forward = m_Velocity;
	}
}
