using UnityEngine;
using System.Collections;

public class UIPlayerInfoScall : MonoBehaviour 
{
	public GameObject mStage;
	public GameObject mPlayerMode;
	public int mSpeedCoeff;
	public float mObstruction;  // need > 0
	public float mScaleSpeed;
	public float mMaxSpeed;

	//BoxCollider mBoxClollider = null;

	// Use this for initialization
	void Start () 
	{
		mScaleSpeed = 0;
		//mBoxClollider = this.gameObject.GetComponent<BoxCollider>();
	}

	//float starDis = 0;
	// Update is called once per frame
	void Update () 
	{

		if (Mathf.Abs(mScaleSpeed) > mObstruction)
		{
			if (mScaleSpeed > 0)
				mScaleSpeed -= mObstruction;
			else
				mScaleSpeed += mObstruction;
		}
		else 
			mScaleSpeed  = 0;

		float y = mScaleSpeed * mSpeedCoeff;
		if (mStage != null)
		{
			Vector3 ang = mStage.transform.localEulerAngles;
			mStage.transform.localEulerAngles = new Vector3(ang.x,ang.y + y,ang.z);
		}

		if (mPlayerMode != null)
		{
			Vector3 ang = mPlayerMode.transform.localEulerAngles;
			mPlayerMode.transform.localEulerAngles = new Vector3(ang.x,ang.y + y,ang.z);
		}

//		if (Input.GetMouseButton(0) && mPlayerMode != null 
//		    //&& MouseOnMode()
//		    )
//		{
//			mScaleSpeed = 0;
//			if (starDis != 0)
//				mPlayerMode.transform.rotation *= Quaternion.AngleAxis( (starDis - Input.mousePosition.x  ) * 20 * Time.deltaTime, mPlayerMode.transform.up);
//			starDis = Input.mousePosition.x;
//		}
	}


//	bool MouseOnMode()
//	{
//		if (mBoxClollider == null || UICamera.currentCamera == null)
//		{
//			return false;;
//		}
//		
//		Ray ray = UICamera.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
//		RaycastHit rayHit;
//		
//		return mBoxClollider.Raycast(ray,out rayHit,100);
//	}
	

	void OnScroll (float delta)
	{
		mScaleSpeed += delta;
		if (mScaleSpeed > mMaxSpeed)
			mScaleSpeed = mMaxSpeed;
		if (mScaleSpeed < -mMaxSpeed )
			mScaleSpeed = -mMaxSpeed;
	}
}
