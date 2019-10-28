using UnityEngine;
using System.Collections;

public class TRMultiR04_s : Trajectory
{
	static Vector3[] originPos = new Vector3[3]{
		new Vector3(0, -2.85f, 2.07f),
		new Vector3(0.002362892f, -6.941441f, 3.017968f),
		new Vector3(0.003171829f, -9.184122f, 1.870163f)
	};
	public float[] fwdAngle = new float[6]{90, 90, 90, 90, 90, 90};
    public float speed = 80f;

	void Start()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().useGravity = false;
        }
		if (fwdAngle.Length != 6)
		{
			Debug.LogWarning("length <> 6");
			return;
		}

		Vector3 axis;
		int rand1 = Random.Range(0,6);
		int rand2 = Random.Range(0,3);
		transform.rotation = Quaternion.identity;
		this.transform.position += Quaternion.AngleAxis(rand1 * 60f, Vector3.up) * originPos [rand2];
		axis = Quaternion.AngleAxis(rand1 * 60f, Vector3.up) * Vector3.forward;
		this.transform.forward = Vector3.Slerp(Vector3.down, axis, (fwdAngle[rand2 * 2] + (fwdAngle[rand2 * 2 + 1] - fwdAngle[rand2 * 2]) * Random.value) / 90f);
    }

    public override Vector3 Track(float deltaTime)
    {
        return transform.forward * speed * deltaTime;
    }
}
