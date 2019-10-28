using UnityEngine;
using System.Collections;

public class UICrideMove : MonoBehaviour 
{
	public float mMoveSpeed = 80;
	public float mColorSpeed = 0.02f;
	public bool mCanMove = true;
	public int mOnceWaitTime_s = 10;

	public UITexture mBgTexcture;
	public UITexture mGuangTexture;
	public UITexture mDianTexture;
	//public int mMove



	bool mStarMove = false;
	bool mIsMove = false;
	Color mGuangColor;
	//Color mBgColor = new Color(0.6f,0.6f,0.6f,1);
	// Use this for initialization
	void Start () 
	{

	}
	
	void Update()
	{
		if (!mCanMove)
			return;

		if (mIsMove)
		{
			if (mStarMove)
				StarMove();
			UpdateMove();
		}
		else 
			UpDateTime();
	}


	bool bColorAdd = true;
	void UpdateMove()
	{
		Vector3 ang = gameObject.transform.localRotation.eulerAngles;
		ang.z -= mMoveSpeed * Time.deltaTime;
		gameObject.transform.localRotation =  Quaternion.Euler(ang);
		float scale;
		if (bColorAdd == true)
		{
			mGuangColor.r += mColorSpeed;
			mGuangColor.g += mColorSpeed;
			mGuangColor.b += mColorSpeed;
			scale =  70 + 24 * mGuangColor.r;
			mBgTexcture.transform.localScale = new Vector3(scale,scale,1);
			if (mGuangColor.r > 0.7)
				bColorAdd = false;
		}
		else
		{
			mGuangColor.r -= mColorSpeed;
			mGuangColor.g -= mColorSpeed;
			mGuangColor.b -= mColorSpeed;
			scale =  70 + 24 * mGuangColor.r;
			mBgTexcture.transform.localScale = new Vector3(scale,scale,1);
			if (mGuangColor.r < 0.06)
				EndMove ();
		}


		mBgTexcture.color =  Color.Lerp(mBgTexcture.color,mGuangColor,Time.deltaTime);
		mGuangTexture.color = Color.Lerp(mGuangTexture.color,mGuangColor,Time.deltaTime);
		mDianTexture.color = Color.Lerp(mDianTexture.color,mGuangColor,Time.deltaTime);

	}

	void StarMove()
	{
		mTime = 0;
		mGuangColor =  Color.black;
		bColorAdd = true;
		mStarMove = false;
		mBgTexcture.transform.localScale = new Vector3(70,70,1);

	}

	void EndMove()
	{
		mIsMove = false;
		mGuangColor =  Color.black;
		mBgTexcture.transform.localScale = new Vector3(70,70,1);
	}

	float mTime = 0;
	float mColorFlag = 0;
	void UpDateTime()
	{
		mTime += 1* Time.deltaTime;
		if (mTime >=  mOnceWaitTime_s)
		{
			mIsMove = true;
			mStarMove = true;

		}

		if (mTime < mOnceWaitTime_s/2)
		{
			mColorFlag = 1-mTime/mOnceWaitTime_s;
		}
		else
		{
			mColorFlag = mTime/mOnceWaitTime_s;
		}

		mColorFlag = mColorFlag * 1.0f;

		//mBgTexcture.color = new Color(mColorFlag,mColorFlag,mColorFlag,1);
	}



}
