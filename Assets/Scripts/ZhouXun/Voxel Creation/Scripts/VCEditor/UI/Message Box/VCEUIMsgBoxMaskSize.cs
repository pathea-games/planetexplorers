using UnityEngine;
using System.Collections;

public class VCEUIMsgBoxMaskSize : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.localScale = new Vector3 (((int)(Screen.width/64+1))*64, ((int)(Screen.height/64+1))*64, 1);
	}
}
