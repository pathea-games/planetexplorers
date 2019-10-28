using UnityEngine;
using System.Collections;
using SkillAsset;

public class TRMultiPara : Trajectory
{
	public float speed = 50f;
	public float MagScaleMin = 0.1f;
	public float MagScaleMax = 0.2f;
	public float angleScope = 160f;

    Transform myTarget;
	Vector3 startPos;
	float totalLength;
	float progressOffset = 0f;
	float progress;
	float transAngle;
	float maxMagnitude;
	float transMag;
	Vector3 subX;
	Vector3 subY;
	Vector3 subZ;
	Vector3 offset = Vector3.zero;
	Vector3 mainPath = Vector3.zero;
	public void Ini(Transform tra){

		this.startPos = transform.position;
		this.subZ = Tes(tra, this.startPos, this.speed) - startPos;
		this.totalLength = subZ.magnitude;
		this.subZ = subZ.normalized;
		this.transform.forward = subZ;
	}
	Vector3 Tes(Transform target, Vector3 startPos, float speed)
	{

			Vector3 reverseVec = startPos - target.position;
		float sqrv22mv12 = Mathf.Sqrt(speed * speed - target.GetComponent<Rigidbody>().velocity.sqrMagnitude);
		float cos2 = Mathf.Cos(Vector3.Angle(reverseVec, target.GetComponent<Rigidbody>().velocity) / 180f * Mathf.PI);
		float temp1 = reverseVec.sqrMagnitude * target.GetComponent<Rigidbody>().velocity.sqrMagnitude * cos2 * cos2;
			float predictTime;
		if(Vector3.Angle(reverseVec, target.GetComponent<Rigidbody>().velocity) <= 90f)
				predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22mv12 / sqrv22mv12) - Mathf.Sqrt(temp1 / sqrv22mv12 / sqrv22mv12)) / sqrv22mv12;
			else
				predictTime = (Mathf.Sqrt(reverseVec.sqrMagnitude + temp1 / sqrv22mv12 / sqrv22mv12) + Mathf.Sqrt(temp1 / sqrv22mv12 / sqrv22mv12)) / sqrv22mv12;
		Vector3 predictPos = target.position + target.GetComponent<Rigidbody>().velocity * predictTime;
			return predictPos;

	}
    public void Emit(Transform target)
    {
        this.myTarget = target;
		this.startPos = transform.position;
		this.subZ = GetPredictPosition(this.myTarget, this.startPos, this.speed) - startPos;
		this.totalLength = subZ.magnitude;
		this.subZ = subZ.normalized;
		this.transform.forward = subZ;
    }

    void Start()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().useGravity = false;
        }
		maxMagnitude = Random.Range(MagScaleMin, MagScaleMax);
		transAngle = Random.Range(- angleScope / 2f, angleScope / 2f);

		Emit(m_Target);
    }

    public override Vector3 Track(float deltaTime)
    {

        progressOffset += speed * Time.deltaTime;
        progress = progressOffset / totalLength;
        transMag = 4f * maxMagnitude * totalLength * (progress - progress * progress);


        subX = Vector3.Cross(Vector3.up, new Vector3(subZ.x, 0, subZ.z)).normalized;
        subY = Vector3.Cross(subZ, subX);

        if (transAngle < 0)
            transAngle += ((int)(transAngle / 360f) + 1) * 360f;
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

        mainPath = subZ * totalLength * progress;

        return startPos + mainPath + offset - transform.position;
        //transform.rotation = Quaternion.FromToRotation(Vector3.forward, (startPos + mainPath + offset - transform.position));
        //rigidbody.AddForce((startPos + mainPath + offset - transform.position) / Time.deltaTime - rigidbody.velocity, ForceMode.VelocityChange);
    }

    public override Quaternion Rotate(float deltaTime)
    {
        return Quaternion.FromToRotation(Vector3.forward, moveVector);
    }
}
