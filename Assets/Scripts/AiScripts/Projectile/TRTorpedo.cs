using UnityEngine;
using System.Collections;

public class TRTorpedo : TRVLS
{
	[SerializeField] float m_Gravity = -Physics.gravity.y;
	[SerializeField] GameObject m_Effect;
	bool m_InWater;
	bool m_UpInWater;
	float m_RotateSpeed = 5f;

	protected override void UpdateVelocity (float deltaTime)
	{
		CheckInWaterState();
		m_Time += deltaTime;
		UpdateAcceleration(deltaTime);
		UpdateTrack(deltaTime);
		if(m_InWater)
			transform.forward = Vector3.Lerp(transform.forward, m_Velocity, m_RotateSpeed * deltaTime);
	}

	void CheckInWaterState()
	{
		m_InWater = PETools.PE.PointInWater(transform.position) > 0.5f;
		m_UpInWater = PETools.PE.PointInWater(transform.position + 0.5f * Vector3.up) > 0.5f;
		if(null != m_Effect && m_Effect.activeSelf != m_InWater)
			m_Effect.SetActive(m_InWater);
	}

	protected override void UpdateTrack (float deltaTime)
	{
		if(m_InWater)
		{
			base.UpdateTrack (deltaTime);
			if(!m_UpInWater && (m_Velocity.y > 0 || null == m_Target))
			{
				Vector3 velocity = m_Velocity;
				velocity.y = 0;
				velocity = Vector3.Slerp(m_Velocity, velocity, m_RotateSpeed * deltaTime);
				m_Velocity = velocity.normalized * m_Velocity.magnitude;
			}
		}
	}

	protected override void UpdateAcceleration (float deltaTime)
	{
		if(!m_InWater)
			m_Velocity += m_Gravity * Vector3.down * deltaTime;
		base.UpdateAcceleration (deltaTime);
	}
}
