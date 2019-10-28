using UnityEngine;
using System.Collections;

public class TRStraight : Trajectory
{
    [SerializeField] float speed;
	[SerializeField] bool towardsTarget;

	public override void SetData (Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData (caster, emitter, target, targetPosition, index);
		Emit();
	}

    public void Emit()
    {
		if(!towardsTarget && null != m_Emitter)
		{
			m_Velocity = m_Emitter.forward;
		}
		else
		{
			m_Velocity = ((null != m_Target)?GetTargetCenter(m_Target):m_TargetPosition) - transform.position;
			m_Velocity.Normalize();
		}
		transform.rotation = Quaternion.LookRotation(m_Velocity, (null != m_Emitter)?m_Emitter.up:Vector3.up);
		m_Velocity *= speed;
    }

    public override Vector3 Track(float deltaTime)
    {
		return m_Velocity * deltaTime;
    }

    //void Start()
    //{
    //    if (rigidbody != null)
    //    {
    //        rigidbody.useGravity = false;
    //    }
    //    this.transform.forward = moveDirection;
    //}

    //void FixedUpdate()
    //{
    //    if (rigidbody == null)
    //        return;

    //    if (moveDirection == Vector3.zero)
    //        return;

    //    rigidbody.AddForce(moveDirection * speed - rigidbody.velocity, ForceMode.VelocityChange);
    //}
}
