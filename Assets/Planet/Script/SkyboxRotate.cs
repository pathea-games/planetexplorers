using UnityEngine;
using System.Collections;

public class SkyboxRotate : MonoBehaviour
{
	public Transform Planet;
	// Update is called once per frame
	void Update ()
	{
		transform.rotation = Planet.rotation;
	}
}
