using UnityEngine;
using System.Collections;

public class TRParabola : Trajectory
{
    public float speed;
    public float heightScale;
	public bool followRotate;
	public float selfRotateW;
	
	Vector3 startPos;
	float totalLenth;
	float maxMagnitude;
	Vector3 subZ;
	Vector3 subY;
	float progressOffset = 0f;
	float progress;
	float transMag;
	Vector3 offset;

	void Start()
	{
		Emit(m_Target ? GetTargetCenter(m_Target) : m_TargetPosition);
	}

    public void Emit(Vector3 target)
    {
		startPos = transform.position;
        Vector3 fixedVector = target - startPos;
		totalLenth = fixedVector.magnitude;
		maxMagnitude = totalLenth * heightScale;
		
		subZ = fixedVector.normalized;
		Vector3 subX = Vector3.Cross(Vector3.up, new Vector3(subZ.x, 0, subZ.z)).normalized;
        subY = Vector3.Cross(subZ, subX);
    }

    public override Vector3 Track(float deltaTime)
    {
        progressOffset += speed * Time.deltaTime;
        progress = progressOffset / totalLenth;
        transMag = progress * 2 - 1;
        transMag = (1 - transMag * transMag) * maxMagnitude;

        offset = subY * transMag;
        return startPos + progressOffset * subZ + offset - transform.position;
    }

    public override Quaternion Rotate(float deltaTime)
    {
		if (followRotate)
			return Quaternion.FromToRotation (Vector3.forward, moveVector);
		else
			return transform.rotation;
    }
}
