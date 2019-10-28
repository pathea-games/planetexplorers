using UnityEngine;
using System.Collections;

public class DoubleDisTrigger : TriggerController 
{
	public float 		mFirstDistnce = 64f;
	public float 		mSencondDistnce = 128f;
	public Transform	mDistanceTarget;
	
	public string		mFirstEnterFuncName = "OnEnterActiveDistance";
	public string		mFirstExitFuncName = "OnExitActiveDistance";
	
	public string		mSecondEnterFuncName = "OnEnterViewableDistance";
	public string		mSecondExitFuncName = "OnExitViewableDistance";
	
	float 				mCurrentDis;
	int					mDisState; // 0. Nothing 1.FirstEnter 2.FirstExit 3.SecondEnter 4.SecondExit
	
	protected override void InitDefault ()
	{
		base.InitDefault ();
		if(null == mDistanceTarget)
		{
            //mDistanceTarget = PlayerFactory.mMainPlayer.transform;
            //mCurrentDis = Vector3.Distance(mDistanceTarget.position, mTrigerTarget.transform.position);
		}
	}
	
	protected override bool CheckTrigger ()
	{
		if(null != mDistanceTarget)
		{
			float oldDis = mCurrentDis;
			float nowDis = Vector3.Distance(mDistanceTarget.position, mTrigerTarget.transform.position);
			mCurrentDis = nowDis;
			if(oldDis >= mFirstDistnce && nowDis < mFirstDistnce)
			{
				mDisState = 1;
				return true;
			}
			else if(oldDis <= mFirstDistnce && nowDis > mFirstDistnce)
			{
				mDisState = 2;
				return true;
			}
			else if(oldDis >= mSencondDistnce && nowDis < mSencondDistnce)
			{
				mDisState = 3;
				return true;
			}
			else if(oldDis <= mSencondDistnce && nowDis > mSencondDistnce)
			{
				mDisState = 4;
				return true;
			}
		}
		return false;
	}
	
	protected override void OnHitTraigger ()
	{
		switch(mDisState)
		{
		case 1:
			mTrigerTarget.SendMessage(mFirstEnterFuncName,SendMessageOptions.DontRequireReceiver);
			break;
		case 2:
			mTrigerTarget.SendMessage(mFirstExitFuncName,SendMessageOptions.DontRequireReceiver);
			break;
		case 3:
			mTrigerTarget.SendMessage(mSecondEnterFuncName,SendMessageOptions.DontRequireReceiver);
			break;
		case 4:
			mTrigerTarget.SendMessage(mSecondExitFuncName,SendMessageOptions.DontRequireReceiver);
			break;
		}
	}
}
