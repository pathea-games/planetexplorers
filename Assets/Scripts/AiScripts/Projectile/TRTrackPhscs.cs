using UnityEngine;
using System.Collections;
using SkillAsset;

public class TRTrackPhscs : Trajectory
{
	public float maxTraction = 40f;
	public float dragCoefficient = 0.15f;
	public float gravity = 9.8f;
	public float maxHalfAngle = 90f;
	public float smoothAngleStart = 30f;
	public float smoothPercent = 0.7f;
	//F=0.5 * C * rou * S * V2
	//rou = 1.205 kg/m3
	//S = 0.17m * 0.17m * pi = 0.0908 m2
	//0.5 * rou * S = 0.054707
	//G = m(1kg) * g(9.8) = 9.8N

    Transform myTarget;
	float timeNow = 0f;
	//Vector3 startPos;
	Vector3 startFwd;
	Vector3 velocity = Vector3.zero;
	Vector3 velocityNext = Vector3.zero;
	Vector3 drag;
	Vector3 traction;
	Vector3 acceleration;
	float angle;
	Vector3 subX;
	Vector3 subY;
	Vector3 targetPos;
	float percent;

    public void Emit(Transform target, Vector3 emitFwd)
    {
		this.myTarget = target;
		this.startFwd = emitFwd;
    }

    void Start()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().useGravity = false;
			//this.startPos = transform.position;
			transform.rotation = Quaternion.FromToRotation(Vector3.forward, startFwd);
			traction = startFwd * maxTraction;
			acceleration = traction + Vector3.down * gravity;
			velocityNext = 0.5f * acceleration * 0.01f;
			//transform.rigidbody.AddForce(acceleration, ForceMode.Acceleration);
        }
		if(null != m_Emitter)
			Emit(m_Target, m_Emitter.forward);
		else
			GameObject.Destroy(gameObject);
    }

    public override Vector3 Track(float deltaTime)
    {
        if (myTarget == null)
            return Vector3.zero;

		timeNow += Time.deltaTime;
		velocity = velocityNext;
		targetPos = GetTargetCenter(myTarget);
		drag = velocity.normalized * 0.054707f * dragCoefficient * velocity.sqrMagnitude + Vector3.up * gravity;

		subX = Vector3.Cross(velocity, targetPos - transform.position);
		subY = Vector3.Cross(subX, velocity).normalized;
		if(Vector3.Angle(velocity, targetPos - transform.position) <= smoothAngleStart)
		{
			if(drag.sqrMagnitude >= maxTraction * maxTraction)
			{
				traction = drag.normalized * maxTraction;
				acceleration = traction - drag;					
			}
			else
			{
				percent = Vector3.Angle(velocity, targetPos - transform.position) / smoothAngleStart * smoothPercent + 1f - smoothPercent;
				if(Vector3.Angle(velocity, targetPos - transform.position) < 1f)
					percent = 0f;
				traction = Vector3.Lerp(velocity.normalized, subY, percent);
				angle = Vector3.Angle(traction, drag);
				traction *= Mathf.Sqrt(maxTraction * maxTraction - drag.sqrMagnitude * Mathf.Sin(angle / 180f * Mathf.PI) * Mathf.Sin(angle / 180f * Mathf.PI)) - drag.magnitude * Mathf.Cos(angle / 180f * Mathf.PI);
				acceleration = traction;					
			}
		}
		else
		{
			percent = maxHalfAngle / 90f;
			traction = Vector3.Lerp(velocity.normalized, subY.normalized, percent) * maxTraction;
			acceleration = traction - drag;				
		}			

		velocityNext = velocity + acceleration * deltaTime;
		return velocity * deltaTime + 0.5f * acceleration * deltaTime * deltaTime;
    }

    public override Quaternion Rotate(float deltaTime)
    {
        return Quaternion.FromToRotation(Vector3.forward, velocity);
    }
}