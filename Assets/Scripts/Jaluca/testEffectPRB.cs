using System.Collections;
using UnityEngine;

class testEffectPRB : MonoBehaviour
{
	public float speed = 4f;
	public float minMagnitude = 1f;
	public float maxMagnitude = 1.5f;
	public float angleScope = 180f;
	public float distance = 10f;

	
	Vector3 speedVector;
	Vector3 speedY;
	float progress = 0f;
	float mag;
	float angle;
	float transMag;
	float dis = 0f;
	public void Start(){
		mag = Random.Range(minMagnitude, maxMagnitude);
		angle = Random.Range(180f - angleScope, angleScope);
		transform.position = Vector3.zero;
	}

	public void FixedUpdate () {
		dis += speed * Time.deltaTime;
		progress = dis / distance;
		if(progress > 1f)
			Destroy(this.gameObject);
		transMag = progress * 2f - 1f;
		transMag = (1f - (transMag * transMag)) * mag;
		transform.position = dis * Vector3.forward + new Vector3(Mathf.Cos(angle / 180f * Mathf.PI),Mathf.Sin(angle / 180f * Mathf.PI),0f) * transMag;

	}
}