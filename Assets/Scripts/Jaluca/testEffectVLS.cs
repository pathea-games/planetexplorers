using System.Collections;
using UnityEngine;

class testEffectVLS : MonoBehaviour
{
	public float maxTraction = 25f;
	public float upDistance = 10f;
	public float dragCoefficient = 0.15f;
	public float gravity = 9.8f;
	public float lifeTime = 2f;
	public float maxHalfAngle = 90f;
	public float smoothAngleStart = 30f;
	public float smoothPercent = 0.6f;
	//F=0.5 * C * rho * S * V2
	//rho = 1.205 kg/m3
	//S = 0.17m * 0.17m * pi = 0.0908 m2
	//0.5 * rho * S = 0.054707
	//G = m(1kg) * g(9.8) = 9.8N

	public GameObject target = null;
	float timeNow = 0f;
	bool track = false;
	Vector3 startPos;
	Vector3 velocity;
	Vector3 drag;
	Vector3 traction;
	Vector3 acceleration;
	float angle;
	Vector3 subX;
	Vector3 subY;
	float percent;
	float maxv = 0f;
	public void Start (){
		startPos = transform.position;
		transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
		traction = Vector3.up * maxTraction;
		acceleration = traction + Vector3.down * gravity;
		transform.GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);
	}
	public void FixedUpdate (){
		timeNow += Time.deltaTime;
		velocity = transform.GetComponent<Rigidbody>().velocity;
		if(velocity.magnitude > maxv)
			maxv = velocity.magnitude;
		if(timeNow >= lifeTime)
		{
			Destroy(gameObject);
		}
		if(Vector3.Distance(transform.position, target.transform.position) <= 1f)
		{
			Destroy(gameObject);
		}
			
		drag = velocity.normalized * 0.054707f * dragCoefficient * velocity.sqrMagnitude + Vector3.up * gravity;
		if(!track)
		{
			if((transform.position - startPos).sqrMagnitude < upDistance * upDistance)
			{
				transform.GetComponent<Rigidbody>().AddForce(Vector3.up * maxTraction + drag, ForceMode.Acceleration);
				return;
			}
			else
				track = true;
		}
		subX = Vector3.Cross(velocity, target.transform.position - transform.position);
		subY = Vector3.Cross(subX, velocity).normalized;
		if(Vector3.Angle(velocity, target.transform.position - transform.position) <= smoothAngleStart)
		{
			if(drag.sqrMagnitude >= maxTraction * maxTraction)
			{
				traction = drag.normalized * maxTraction;
				transform.GetComponent<Rigidbody>().AddForce(traction - drag, ForceMode.Acceleration);
			}
			else
			{
				percent = Vector3.Angle(velocity, target.transform.position - transform.position) / smoothAngleStart * smoothPercent + 1f - smoothPercent;
				if(Vector3.Angle(velocity, target.transform.position - transform.position) < 1f)
				   percent = 0f;
				traction = Vector3.Lerp(velocity.normalized, subY, percent);
				angle = Vector3.Angle(traction, drag);
				traction *= Mathf.Sqrt(maxTraction * maxTraction - drag.sqrMagnitude * Mathf.Sin(angle / 180f * Mathf.PI) * Mathf.Sin(angle / 180f * Mathf.PI)) - drag.magnitude * Mathf.Cos(angle / 180f * Mathf.PI);
				transform.GetComponent<Rigidbody>().AddForce(traction, ForceMode.Acceleration);
			}
		}
		else
		{
			percent = maxHalfAngle / 90f;
			traction = Vector3.Lerp(velocity.normalized, subY, percent) * maxTraction;
			transform.GetComponent<Rigidbody>().AddForce(traction - drag, ForceMode.Acceleration);
		}

		transform.rotation = Quaternion.FromToRotation(Vector3.forward, velocity);
		/*Debug.DrawLine(transform.position, transform.position + velocity.normalized * 5f,Color.green);
		Debug.DrawLine(transform.position, transform.position - drag.normalized * 5f,Color.red);
		Debug.DrawLine(transform.position, transform.position + traction.normalized * 5f,Color.cyan);
		Debug.DrawLine(transform.position, target.transform.position);*/
	}
}