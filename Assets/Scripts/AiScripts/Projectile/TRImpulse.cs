using UnityEngine;
using System.Collections;

public enum TRImpulseInitDirection{PrecisionGuided, TowardsTarget, SelfDirection}
public class TRImpulse : Trajectory
{	
    public float speed;
    public float gravity;
	public float resist = 0.1f;
	public TRImpulseInitDirection initDirection;
	
	public bool followRotate;

	public override void SetData (Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData (caster, emitter, target, targetPosition, index);
		if(null != m_Emitter)
			Emit(m_Target ? GetTargetCenter(m_Target) : targetPosition, m_Emitter, index);
		else
			GameObject.Destroy(gameObject);
	}

    protected virtual void Emit(Vector3 targetPos, Transform emitTrans, int index)
    {
		switch(initDirection)
		{
		case TRImpulseInitDirection.PrecisionGuided:
			float h = targetPos.y - transform.position.y;
			float s2 = Vector2.SqrMagnitude(new Vector2(targetPos.x - transform.position.x, targetPos.z - transform.position.z));
			float y = h - speed * speed / gravity;
			if(speed * speed * speed * speed - 2.0f * gravity * speed * speed * h - gravity * gravity * s2 > PETools.PEMath.Epsilon)
				y += Mathf.Sign(gravity) * Mathf.Sqrt((h - speed * speed / gravity) * (h - speed * speed / gravity) - s2 - h * h);
			m_Velocity = new Vector3(targetPos.x - transform.position.x, h - y, targetPos.z - transform.position.z).normalized * speed;
			transform.rotation = emitTrans.rotation;
			break;
		case TRImpulseInitDirection.TowardsTarget:
			m_Velocity = (targetPos - transform.position).normalized * speed;
			transform.rotation = Quaternion.FromToRotation(Vector3.forward, m_Velocity);
			break;
		case TRImpulseInitDirection.SelfDirection:
			m_Velocity = emitTrans.forward * speed;
			transform.rotation = emitTrans.rotation;
			break;
		}
    }

    public override Vector3 Track(float deltaTime)
    {
//		if(initDirection == TRImpulseInitDirection.PrecisionGuided)
//			m_Velocity += Vector3.down * gravity * deltaTime;
//		else		
//			m_Velocity += (Vector3.down * gravity - m_Velocity.normalized * m_Velocity.sqrMagnitude * resist) * deltaTime;

		m_Velocity += Vector3.down * gravity * deltaTime;
		m_Velocity += (-resist * m_Velocity.sqrMagnitude * m_Velocity.normalized) * Time.deltaTime;
        return m_Velocity * deltaTime;
    }

    public override Quaternion Rotate(float deltaTime)
    {
        if (followRotate)
            return Quaternion.FromToRotation(Vector3.forward, m_Velocity);
        else
            return transform.rotation;
    }

}
