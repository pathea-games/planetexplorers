using UnityEngine;
using System.Collections;

public class TRMultiR03 : Trajectory
{
    public float speed = 80f;
	public float offsetRadius= 7f;
	public float posAngleMin = 20f;
	public float posAngleMax = 65f;
	public float fwdAngleMin = 35f;
	public float fwdAngleMax = 70f;

	float percent;
	Vector3 axis;

    void Start()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().useGravity = false;
        }

		axis = Quaternion.AngleAxis(Random.value * 360f, Vector3.up) * Vector3.forward;
		percent = Random.value;
		transform.position += Vector3.Slerp(Vector3.down, axis, (posAngleMin + (posAngleMax - posAngleMin) * percent) / 90f) * offsetRadius;
		transform.forward = Vector3.Slerp(Vector3.down, axis, (fwdAngleMin + (fwdAngleMax - fwdAngleMin) * percent) / 90f);
    }

    public override Vector3 Track(float deltaTime)
    {
        return transform.forward * speed * deltaTime;
    }
}
