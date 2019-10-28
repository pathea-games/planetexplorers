using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMapCtrl : MonoBehaviour 
{
	public OnGuiBtnClicked mMapMove = null;
	public OnGuiBtnClicked mMapZoomed = null;
	
	public Camera mMapCamera;
	public float mCameraSizes;  //没一像素 对应1米时 色相机的 size
	public int mMaxSzieCount;
	public int mSpritLeng;  //spite像素长度
	public UILabel mLbMapScale;
	public int mCameraSizeCount = 2;
	public int mMaxMapHight;
	public int mMaxMapWidth;
	

	void Awake()
	{
		ChangeCameraSize();
	}
	// Use this for initialization
	void Start () 
	{


	}
	
	// Update is called once per frame
	void Update () 
	{
		if(UICamera.hoveredObject == gameObject)
		{
			float ds = Input.GetAxis("Mouse ScrollWheel");
			if(Mathf.Abs(ds) > PETools.PEMath.Epsilon)
				UpdateZoomed(ds);
		}
		//if (Input.GetAxis("Mouse ScrollWheel")
	}



	void UpdateZoomed(float ds)
	{
		if(isChangeZoomed)
			return;
		isChangeZoomed = true;
		if(ds > 0)
			ZoomedOut();
		else 
			ZoomedIn ();

		Invoke("SetChageZoomedFale",0.1f);
	}

	bool isChangeZoomed = false;
	void SetChageZoomedFale()
	{
		isChangeZoomed = false;
	}





	// fang da
	void ZoomedIn()
	{
		if(mCameraSizeCount < mMaxSzieCount)
		{
			mCameraSizeCount = mCameraSizeCount*2;
			ChangeCameraSize();
		}
	}

	// suo xiao
	void ZoomedOut()
	{
		if(mCameraSizeCount > 1)
		{
			mCameraSizeCount = mCameraSizeCount/2;
			ChangeCameraSize();
		}
	}


	void BtnZoomedIn()
	{
		ZoomedIn();
	}
	void BtnZoomedOut()
	{
		ZoomedOut();
	}

	void OnDrag(Vector2 DopPos)
	{
		MoveMapCamera(DopPos);
	}


	void ChangeCameraSize()
	{
		mMapCamera.orthographicSize = mCameraSizes * mCameraSizeCount;
		mLbMapScale.text = (1 * mSpritLeng * mCameraSizeCount).ToString() + "m";
		if(mMapZoomed != null)
			mMapZoomed();
	}


	void MoveMapCamera( Vector2 pos)
	{
		Vector3 mCameraPos = mMapCamera.transform.localPosition;
		mCameraPos.x -= pos.x * mCameraSizeCount;
		mCameraPos.y -= pos.y * mCameraSizeCount;
		if (Pathea.PeGameMgr.IsSingleStory)
		{
			if(mCameraPos.x >= 0 && mCameraPos.x <= mMaxMapWidth  && mCameraPos.y>=0 && mCameraPos.y< mMaxMapHight)
			{
				mMapCamera.transform.localPosition =mCameraPos;
				if (mMapMove != null)
					mMapMove();
			}
		}
		else 
		{
			mMapCamera.transform.localPosition =mCameraPos;
			if (mMapMove != null)
				mMapMove();
		}
	}
	


}
