using UnityEngine;
using System.Collections;
using SkillAsset;

public class TRShotgun : Trajectory
{
	public float speed;
	public float maxAngle;

	//Vector3 vertical = Vector3.up;

	public override void SetData (Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData (caster, emitter, target, targetPosition, index);
		Emit(GetTargetCenter() - transform.position);
	}
	
	protected void Emit(Vector3 fwd)
	{
		m_Velocity = fwd.normalized;
		Vector3 up = (null != m_Emitter) ? m_Emitter.up : Vector3.up;
		Vector3 right = Vector3.zero;
		Vector3.OrthoNormalize(ref m_Velocity, ref up, ref right);
		m_Velocity = Quaternion.AngleAxis((Random.value - 0.5f) * maxAngle, up) * Quaternion.AngleAxis((Random.value - 0.5f) * maxAngle, right) * m_Velocity;
		transform.rotation = Quaternion.LookRotation(m_Velocity, up);
		m_Velocity *= speed;
	}
	
	public override Vector3 Track(float deltaTime)
	{
		return m_Velocity * deltaTime;
	}
}
