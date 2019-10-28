using UnityEngine;
using System.Collections;

public class SelfRotate : MonoBehaviour
{
	public float Speed = 5;
	// Update is called once per frame
	void Update ()
	{
		transform.localEulerAngles = new Vector3 (0, Time.time * Speed, 0);
	}
}
