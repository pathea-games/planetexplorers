using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour
{
	public Camera 		mCam;
	public GameObject	mWaterObj;
	
	void Update ()
	{
		if(mWaterObj != null)
			transform.position = new Vector3(mCam.transform.position.x,mWaterObj.transform.position.y,mCam.transform.position.z);
		else
			transform.position = new Vector3(mCam.transform.position.x,0,mCam.transform.position.z);
	}
}
