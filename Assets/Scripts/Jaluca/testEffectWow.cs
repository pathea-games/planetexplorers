using System.Collections;
using UnityEngine;

class testEffectWow : MonoBehaviour
{
	public float speed = 20f;
	public float missileIndex = 1;
	public float missileCount = 1;
	public float targetPosZ = 100f;
	public float maxProgress = 1f;
	public bool destroy = false;

	float startPosZ = 0f;
	//float startDistance;
	float totalDistance;
	float distance = 0f;
	float progress = 0f;
	float timeNow = 0f;
	float speedScalar = 1f;
	float modelPitch = 0f;
	float modelYaw = 0f;
	float modelRoll = 0f;
	Vector3 offset = Vector3.zero;
	float transMag = 0;
	float transUp;
	float transFront = 0;
	float transRight = 0;
	float transAngle = 0;
	//float rand1;
	//float rand2;
	//float rand3;

	//local:

	float magnitude;

	public void Awake (){
		transform.position = Vector3.zero;
		transform.forward = Vector3.forward;
		//startDistance = targetPosZ - startPosZ;
		totalDistance = targetPosZ - startPosZ; //i guess this is real-time distance
	}
	public void Start (){
	//local:
		magnitude = totalDistance * 2f;
	}
	public void Update (){
		timeNow += Time.deltaTime;
		if(timeNow <= 0f)
			return;
		distance += speed * speedScalar * Time.deltaTime;
		progress = distance / totalDistance;
		//rand1 = Random.value;
		//rand2 = Random.value;
		//rand3 = Random.value;
		if(progress > maxProgress)
		{
			if(destroy)
				Destroy(gameObject);
			transform.position = Vector3.zero;
			timeNow = -2f;
			distance = 0f;
			progress = 0f;
			//startDistance = targetPosZ - startPosZ;
			return;
		}
		WowCode();
	}
	public void LateUpdate (){
		offset = new Vector3(Mathf.Sin(transAngle * Mathf.PI / 180f), Mathf.Cos(transAngle * Mathf.PI / 180f), 0);
		transform.position = distance * Vector3.forward + transMag * offset;
		transform.position += transFront * Vector3.forward + transUp * Vector3.up + transRight * Vector3.right;
		transform.rotation = Quaternion.AngleAxis(modelPitch, Vector3.right) * Quaternion.AngleAxis(modelYaw, Vector3.up) * Quaternion.AngleAxis(modelRoll, Vector3.forward);
	}

	public void WowCode(){
		transUp = magnitude * ( 1 - progress );
		transFront = DistanceToImpactPos();
	}

	public float DistanceToImpactPos()
	{
		return targetPosZ - distance;
	}
}