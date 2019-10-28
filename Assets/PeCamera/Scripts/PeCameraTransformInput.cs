using UnityEngine;
using System.Collections;

public class PeCameraTransformInput : MonoBehaviour
{
	public string VarName;

	// Use this for initialization
	void Start ()
	{
		PeCamera.SetTransform(VarName, this.transform);
	}
	
	// Update is called once per frame
	void Update ()
	{
		PeCamera.SetTransform(VarName, this.transform);
	}
}
