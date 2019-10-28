using UnityEngine;
using System.Collections;

public class UISelfRotate : MonoBehaviour
{
	public float Speed = 20;

	// Update is called once per frame
	void Update ()
	{
		transform.eulerAngles = transform.eulerAngles + Vector3.forward * Speed * Time.deltaTime;
	}
}
