using UnityEngine;
using System.Collections;

public class TRGatling : Trajectory
{
	public float speed;
	public float maxOffset;
	
	Vector3 moveDirection;
	Vector3 offset = Vector3.up;

	void Start()
	{
		if(null != m_Emitter)
			Emit(m_Emitter.forward);
		else
			GameObject.Destroy(gameObject);
	}

	public void Emit(Vector3 fwd)
	{
		moveDirection = fwd;
		Vector3.OrthoNormalize(ref moveDirection, ref offset);
		transform.position += Quaternion.AngleAxis (Random.value * 360f, moveDirection) * (offset * maxOffset);
		transform.forward = moveDirection;
	}
	
	public override Vector3 Track(float deltaTime)
	{
		return moveDirection * speed * deltaTime;
	}
	
//	void Start()
//    {
//        if (rigidbody != null)
//        {
//            rigidbody.useGravity = false;
//			rigidbody.velocity = moveDirection.normalized * speed;
//        }
//    }
//
//    void FixedUpdate(){}
}
