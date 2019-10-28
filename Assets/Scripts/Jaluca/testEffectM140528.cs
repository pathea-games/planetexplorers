using System.Collections;
using UnityEngine;

class testEffectM140528 : MonoBehaviour
{
	public float speed = 0;
	public int index = 0;
	public int AngleUnit = 180;
	public float delay = 0.1f;
	public float interval = 0.1f;
	public float heightMin = 0.3f;
	public float heightMax = 0.6f;
	public float offsetMin = 0.2f;
	public float offsetMax = 0.4f;
	public float circlePSCD = 3f;

	public GameObject target = null;
	float timeNow = 0f;
	Vector3 startPos;
	Vector3 lastPos;
	Vector3 forwardVector;
	Vector3 frameVector;
	float distanceNow = 0f;
	float progress = 0f;
	bool valid = false;
	float transMag;
	Vector3 subZ;
	Vector3 fsubZ;
	Vector3 subY = Vector3.up;
	Vector3 fsubY = Vector3.up;
	float angleNow = 0f;
	int angleStart;

	public void Start (){
		startPos = transform.position;
		lastPos = startPos;
		angleStart = AngleUnit * index;
	}
	public void FixedUpdate (){
		if (!valid)
			MissileSwitch ();
		if (valid)
		{
			forwardVector = target.transform.position - startPos;
			distanceNow += speed * Time.deltaTime;
			progress = distanceNow / forwardVector.magnitude;
			transMag = progress * 2f - 1f;
			transMag = (1f - (transMag * transMag)) * heightMin * forwardVector.magnitude;  //TODO
			subZ = forwardVector;
			Vector3.OrthoNormalize(ref subZ, ref subY);
			frameVector = startPos + forwardVector * progress + subY * transMag - lastPos;
			lastPos = startPos + forwardVector * progress + subY * transMag;
			fsubZ = frameVector;
			Vector3.OrthoNormalize(ref fsubZ, ref fsubY);
			angleNow += circlePSCD * Time.deltaTime * 360f;
			transform.rotation = Quaternion.FromToRotation(Vector3.forward, lastPos + Quaternion.AngleAxis(angleStart + angleNow, fsubZ) * (fsubY * offsetMin) - transform.position);
			transform.position = lastPos + Quaternion.AngleAxis(angleStart + angleNow, fsubZ) * (fsubY * offsetMin);



		}




	}
	void MissileSwitch(){
		timeNow += Time.deltaTime;
		if (timeNow > delay + interval * index - interval) 
		{
			valid = true;
			timeNow = 0f;
		}
	}

}