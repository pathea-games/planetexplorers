using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour
{
	public CameraForge.CameraController camCtrl;

	// Use this for initialization
	void Start ()
	{
		CameraForge.CameraController.SetTransform("Character", GameObject.Find("Character").transform);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			camCtrl.CrossFade("Test Blender", 0, 0.2f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			camCtrl.CrossFade("Test Blender", 1, 0.2f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			camCtrl.CrossFade("Test Blender", 2, 0.2f);
		}
	}
}
