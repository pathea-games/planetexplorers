using UnityEngine;
using System.Collections;

public class TRMultiR04_l : Trajectory
{
	static Vector3[] originPos = new Vector3[2]{
		new Vector3(1.7f, -1.71f, 0f),
		new Vector3(1.27f, -6.06f, 0f)
	};
	static Vector3 offset = new Vector3(-0.04f, -0.375f, 0f);
	public float[] fwdAngle = new float[4]{90, 90, 90, 90};
	public float speed = 80f;
	
	public byte index;
	int rand1;
	int rand2;
	float rand3;
	Vector3 axis;

	public void Emit(byte index){
		this.index = index;
	}

	void Start()
	{

		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().useGravity = false;
		}
		if (fwdAngle.Length != 4)
		{
			Debug.LogWarning("length <> 4");
			return;
		}

		Random.seed = (int)(Time.time % 10 * 1000);
		rand1 = Random.Range (0, 6);
		rand2 = Random.Range (0, 2);
		rand3 = Random.value;
		transform.rotation = Quaternion.identity;
		transform.position += Quaternion.AngleAxis(rand1 * 60f, Vector3.up) * originPos [rand2] + offset * (index - 1);
		axis = Quaternion.AngleAxis(rand1 * 60f, Vector3.up) * Vector3.right;
		transform.forward = Vector3.Slerp(Vector3.down, axis, (fwdAngle[rand2 * 2] + (fwdAngle[rand2 * 2 + 1] - fwdAngle[rand2 * 2]) * rand3) / 90f);
	}

    public override Vector3 Track(float deltaTime)
    {
        return transform.forward * speed * deltaTime;
    }
}
