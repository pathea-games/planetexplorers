using System.Collections;
using UnityEngine;
using SkillAsset;

class TRSiloMissile : Trajectory
{
	public int index;
	public float random1;
	public float delay = 0.1f;
	public float interval = 0.1f;
	
	public float maxSpeed = 35f;
	public float acceleration = 50f;
	public float lerpSpeed = 90f;
	
	public float parameterA = 3f;
	public float parameterB = 1.5f;
	public Vector3 fwd = Vector3.zero;
	public float maxUpSpeed = 20f;
	public float AngleUnit = 150f;
	public float angleSpeed = 3f;
	public float angleSpeed2 = 2f;
	
	public float lerpMin = 0.25f;
	public float progress1 = 0.05f;
	public float progress2 = 0.15f;
	public float progress3 = 0.5f;
	public float progress4 = 0.8f;
	public float angle1 = 0.3333f;
	public float angle2 = 0.6666f;
	public float angle3 = 1.3333f;
	public float angle4 = 2f;

	Transform myTarget;
	bool valid = false;
	bool trackChance = true;
	bool loseTarget = false;
	float angle;
	Vector3 direction;
	Vector3 mainPos;
	Vector3 refPos;
	Vector3 finalPos;
	float timeNow = 0f;
	
	Vector3 startPos;
	float progress;
	Vector3 subZ;
	float speed;
	
	Vector3 refY;
	Vector3 refX = Vector3.one;
	Vector3 refZ = Vector3.one;
	float angleStart;
	Vector3 targetCenter;
	float upSpeed;
	float coordinateX;
	float coordinateZ;
	
	float lerpX;
	float lerpY;
	//bool init = true;

	void Start()
	{
		this.index = m_Index;
		this.random1 = 0.5f;

		if(null != m_Emitter)
		{
	        if (m_Target)
	            Emit(m_Target, m_Emitter.forward);
	        else
	            Emit(m_TargetPosition, m_Emitter.forward);
		}
		else
			GameObject.Destroy(gameObject);
        //TODO RANDOM NEEDED
    }

	//void Update()
	//{
	//	if(init)
	//	{
	//		if(m_Target)
	//			Emit(m_Target, m_Emitter.forward);
	//		else
	//			Emit(m_TargetPosition, m_Emitter.forward);
	//		init = false;
	//	}
	//}

	public void Emit(Transform target, Vector3 emitfwd)
	{
		this.myTarget = target;
		angleStart = AngleUnit * index + random1 * 360f;
		subZ = GetTargetCenter(target) - startPos;
		if (fwd == Vector3.zero)
			direction = emitfwd;
		else
			direction = fwd;
		refY = direction;
		Vector3.OrthoNormalize(ref refY, ref refX, ref refZ);
		transform.transform.GetChild (0).gameObject.SetActive (false);
		transform.transform.GetChild (1).gameObject.SetActive (false);
	}

	public void Emit(Vector3 targetPos, Vector3 emitfwd)
	{
		angleStart = AngleUnit * index + random1 * 360f;
		subZ = targetPos - startPos;
		if (fwd == Vector3.zero)
			direction = emitfwd;
		else
			direction = fwd;
		refY = direction;
		Vector3.OrthoNormalize(ref refY, ref refX, ref refZ);
		transform.transform.GetChild (0).gameObject.SetActive (false);
		transform.transform.GetChild (1).gameObject.SetActive (false);
	}

	public override Vector3 Track(float deltaTime)
	{
//		if (target == null)
//			return Vector3.zero;
		if (!valid)
			MissileSwitch (deltaTime);
		if (valid)
		{
			if(myTarget)
				targetCenter = GetTargetCenter(myTarget);
			else
				targetCenter = m_TargetPosition;
			timeNow += deltaTime;
			subZ = (targetCenter - transform.position).normalized;
			speed = Mathf.Min(acceleration * timeNow, maxSpeed);
			mainPos += direction * speed * deltaTime;
			angle = Vector3.Angle(direction, subZ);
			if(trackChance && angle < 20f)
				trackChance = false;
			if(!trackChance && !loseTarget && angle > 80f)
				loseTarget = true;
			if(!loseTarget)
				direction = Vector3.Slerp(direction, subZ, lerpSpeed * deltaTime / angle);

			progress = timeNow * speed / Mathf.Min(100f, (targetCenter - startPos).magnitude);
			coordinateX = 3f * parameterB * Mathf.Cos(timeNow * angleSpeed + angleStart) + parameterA * Mathf.Cos(angleSpeed2 * timeNow * angleSpeed - angleStart);
			coordinateZ = parameterB * Mathf.Sin(timeNow * angleSpeed + angleStart) + parameterA * Mathf.Sin(angleSpeed2 * timeNow * angleSpeed - angleStart);
			coordinateX = index % 2 == 1 ? coordinateX : -coordinateX;
			coordinateZ = index % 2 == 1 ? coordinateZ : -coordinateZ;
			upSpeed = maxUpSpeed / maxSpeed * speed;
			refPos = startPos + refY * upSpeed * timeNow + refX * coordinateX + refZ * coordinateZ;
			
			if(progress <= progress1)
				lerpX = progress / progress1 * angle1 * Mathf.PI;
			else if(progress <= progress2)
				lerpX = ((progress - progress1) / (progress2- progress1) * (angle2 - angle1) + angle1) * Mathf.PI;
			else if(progress <= progress3)
				lerpX = ((progress - progress2) / (progress3- progress2) * (angle3 - angle2) + angle2) * Mathf.PI;
			else if(progress <= progress4)
				lerpX = ((progress - progress3) / (progress4- progress3) * (angle4 - angle3) + angle3) * Mathf.PI;
			else
				lerpX = 0f;
			lerpY = (Mathf.Cos(lerpX) + (1f + lerpMin) / (1f - lerpMin)) / (2f / (1f - lerpMin));
			finalPos = Vector3.Lerp(refPos, mainPos, lerpY);
			return finalPos - transform.position;
		}
		else
			return direction * 0.0001f;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		if (valid)
			return Quaternion.FromToRotation (Vector3.forward, moveVector);
		else
			return Quaternion.FromToRotation (Vector3.forward, direction);
	}

	void MissileSwitch(float deltaTime)
	{
		timeNow += deltaTime;
		if (timeNow > delay + interval * index - interval) 
		{
			valid = true;
			timeNow = 0f;
			startPos = transform.position;
			mainPos = transform.position;
			transform.transform.GetChild (0).gameObject.SetActive (true);
			transform.transform.GetChild (1).gameObject.SetActive (true);
		}
	}
	
}