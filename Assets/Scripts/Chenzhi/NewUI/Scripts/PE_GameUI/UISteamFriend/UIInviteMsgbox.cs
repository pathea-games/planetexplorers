using UnityEngine;
using System.Collections;

public class UIInviteMsgbox : UIStaticWnd 
{
	[SerializeField] UILabel mLbText;
	[SerializeField] TweenPosition mTweenPos;

	public delegate void CallBackFunc();

	public CallBackFunc mJoinFunc = null;
	public CallBackFunc mCancelFunc = null;
	public CallBackFunc mIgnorAllFunc = null;
	public CallBackFunc mTimeOutFunc = null;

	private UTimer mTimer = new UTimer();

	public void ShowMsg(string msg, CallBackFunc joinFunc,CallBackFunc cancelFunc, CallBackFunc ignorAllFunc, CallBackFunc timeOutFunc)
	{
		mLbText.text = msg;
		mJoinFunc += joinFunc;
		mCancelFunc += cancelFunc;
		mIgnorAllFunc += ignorAllFunc;
		mTimeOutFunc += timeOutFunc;
		Show();
		mTimer.Second = 60.0;
		mTimer.ElapseSpeed = -1;
	}

	void BtnJionOnClick()
	{
		if (mJoinFunc != null)
			mJoinFunc();
		Hide();
	}

	void BtnCancelOnClick()
	{
		if (mCancelFunc != null)
			mCancelFunc();
		Hide();
	}

	void BtnIgnorAllOnClick()
	{
		if (mIgnorAllFunc != null)
			mIgnorAllFunc();
		Hide();
	}


	void Update()
	{
//		mTimer.Update(Time.deltaTime);
//		if (mTimer.Second <= 0)
//		{
//			Hide();
//			if (mTimeOutFunc != null)
//				mTimeOutFunc();
//		}
	}

	bool isHide;
	public override void Show ()
	{
		isHide = false;
		base.Show();
		mTweenPos.Play(true);
	}
	protected override void OnHide()
	{
		isHide = true;
		mTweenPos.Play(false);
	}
	
	void MoveFinished()
	{
		if(isHide)
			base.Hide();
	}
}
