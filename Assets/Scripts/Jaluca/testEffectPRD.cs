using System.Collections;
using UnityEngine;

class testEffectPRD : MonoBehaviour
{
	public float speed = 0;
	public float existTime = 0;

	public GameObject target = null;
	float timeNow = 0f;
	void Start(){
		Vector3 reverseVec = transform.position - target.transform.position;
		float sqrv22v12 = Mathf.Sqrt(speed * speed - target.GetComponent<Rigidbody>().velocity.sqrMagnitude);
		float cos2 = Mathf.Cos(Vector3.Angle(reverseVec, target.GetComponent<Rigidbody>().velocity) / 180f * Mathf.PI);
		float temp1 = reverseVec.sqrMagnitude * target.GetComponent<Rigidbody>().velocity.sqrMagnitude * cos2 * cos2;
		float predictTime;
		if(Vector3.Angle(reverseVec, target.GetComponent<Rigidbody>().velocity) <= 90f)
			predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22v12 / sqrv22v12) - Mathf.Sqrt(temp1 / sqrv22v12 / sqrv22v12)) / sqrv22v12;
		else
			predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22v12 / sqrv22v12) + Mathf.Sqrt(temp1 / sqrv22v12 / sqrv22v12)) / sqrv22v12;
		Vector3 predictPos = target.transform.position + target.GetComponent<Rigidbody>().velocity * predictTime;

		transform.GetComponent<Rigidbody>().velocity = (predictPos - transform.position).normalized * speed;
		transform.rotation = Quaternion.FromToRotation(Vector3.forward, predictPos - transform.position);
	}

	void FixedUpdate(){
		timeNow += Time.deltaTime;
		if((target.transform.position - transform.position).sqrMagnitude <= 0.25f)
			Destroy(gameObject);
		if(timeNow >= existTime)
			Destroy(gameObject);
	}
}