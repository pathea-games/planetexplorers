using UnityEngine;
using System.Collections;

public class TRFlare : Trajectory
{
    public float range;
    public float speed;
	
	Vector3 direction;
	float distance = 0f;

	void Start()
	{
		if(m_Target != null)
			
			Emit(GetTargetCenter(m_Target));
		else
			Emit(m_TargetPosition);
	}

	public void Emit(Vector3 targetPos)
    {
		this.direction = null != m_Emitter ? m_Emitter.forward : Vector3.up;
    }

    public override Vector3 Track(float deltaTime)
    {
        distance += (1 - distance / range) * speed * deltaTime;
        return (1 - distance / range) * speed * deltaTime * direction.normalized;
    }

    //void Start()
    //{
    //    if (rigidbody != null)
    //    {
    //        rigidbody.useGravity = false;
    //    }
    //}

    //void FixedUpdate()
    //{
    //    if (rigidbody == null || direction == Vector3.zero)
    //        return;
		
    //    rigidbody.AddForce((1 - distance / range) * speed * direction - rigidbody.velocity, ForceMode.VelocityChange);
		
    //    distance += (1 - distance / range) * speed * Time.deltaTime;
    //}
}
