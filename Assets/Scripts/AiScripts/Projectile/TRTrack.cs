using UnityEngine;
using System.Collections;
using SkillAsset;

public class TRTrack : Trajectory
{
    public float speed;
	public float circlePerSec = 1f;
	public float wavesPerSec = 2.0f;
	public float maxMagnitude = 0.3f;

    Transform myTarget;
	Vector3 startPos;
	float startTime;
	Vector3 dynamicDir;
	float transAngle = 0f;
	float transMag;
	Vector3 subX;
	Vector3 subY;
	float random1 = -999f;
	Vector3 offset = Vector3.zero;
	Vector3 mainPath = Vector3.zero;
	float Rand1 {
		get {
			if(random1 < -1.1f){
				random1 = Random.value > 0.5f ? 1f : -1f;
			}
			return random1;
		}
	}

	bool init = true;

//	void Start()
//	{
//		Emit(m_Target);
//	}

	void Update()
	{
		if(init)
		{
			Emit(m_Target);
			init = false;
		}
	}

    public void Emit(Transform target)
    {
        this.myTarget = target;
		this.startPos = transform.position;
		this.startTime = Time.time;
		this.dynamicDir = (GetPredictPosition(this.myTarget, this.startPos, this.speed) - transform.position).normalized;
    }

    public override Vector3 Track(float deltaTime)
    {
        transAngle += circlePerSec * 360f * deltaTime;
        transAngle -= (int)(transAngle / 360f) * 360f;
        transMag = Mathf.Sin((Time.time - startTime) * wavesPerSec * 2.0f * Mathf.PI) * maxMagnitude;

		subX = Vector3.Cross(Vector3.up, new Vector3(dynamicDir.x, 0, dynamicDir.z)).normalized * Rand1;
		subY = Vector3.Cross(dynamicDir, subX) * Rand1;

        offset.x = (int)(transAngle / 90f);
        if (offset.x == 0f)
            offset = Vector3.Slerp(subY, subX, transAngle / 90f);
        else if (offset.x == 1f)
            offset = Vector3.Slerp(subX, -subY, (transAngle - 90f) / 90f);
        else if (offset.x == 2f)
            offset = Vector3.Slerp(-subY, -subX, (transAngle - 180f) / 90f);
        else
            offset = Vector3.Slerp(-subX, subY, (transAngle - 270f) / 90f);
        offset *= transMag;

        mainPath += dynamicDir * speed * deltaTime;

        return startPos + mainPath + offset - transform.position;
    }

    public override Quaternion Rotate(float deltaTime)
    {
        return Quaternion.FromToRotation(Vector3.forward, moveVector);
    }
}
