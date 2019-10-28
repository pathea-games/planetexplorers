using UnityEngine;
using System.Collections;
using WhiteCat;

public class MoveSphere : MonoBehaviour
{
	public Path path;
	public float resetHeight = -1;
	public float forceScale = 1;


	void Update()
	{
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		GetComponent<Rigidbody>().AddForce(Camera.main.transform.TransformDirection(input) * forceScale);

		if(transform.position.y < resetHeight)
		{
			int splineIndex;
			float splineTime;
			path.GetClosestPathPosition(transform.position, 1, out splineIndex, out splineTime);
			transform.position = path.GetSplinePoint(splineIndex, splineTime) + Vector3.up;
		}

		Camera.main.transform.LookAt(transform);
	}
}