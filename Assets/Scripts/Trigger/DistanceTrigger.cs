//using UnityEngine;
//using System.Collections;


//public class DistanceTrigger : TriggerController 
//{
//    public float 		mTriggerDistnce = 256f;
//    public Transform	mDistanceTarget;
	
//    public string		mEnterFuncName = "OnEnterRange";
	
//    public string		mExitFuncName = "OnExitRange";
	
//    float 				mCurrentDis;
//    int					mDisState; // 0. Nothing 1.Enter 2.Exit
	
//    protected override void InitDefault ()
//    {
//        base.InitDefault ();
//        //if(null == mDistanceTarget && null != PlayerFactory.mMainPlayer)
//        //{
//        //    mDistanceTarget = PlayerFactory.mMainPlayer.transform;
//        //    mCurrentDis = Vector3.Distance(mDistanceTarget.position, mTrigerTarget.transform.position);
//        //    if(mCurrentDis <= mTriggerDistnce)
//        //        mTrigerTarget.SendMessage(mEnterFuncName, SendMessageOptions.DontRequireReceiver);
//        //    else
//        //        mTrigerTarget.SendMessage(mExitFuncName, SendMessageOptions.DontRequireReceiver);
//        //}
//    }
	
//    protected override bool CheckTrigger ()
//    {
//        if(null != mDistanceTarget)
//        {
//            float oldDis = mCurrentDis;
//            float nowDis = Vector3.Distance(mDistanceTarget.position, mTrigerTarget.transform.position);
//            mCurrentDis = nowDis;
//            if(oldDis >= mTriggerDistnce && nowDis < mTriggerDistnce)
//            {
//                mDisState = 1;
//                return true;
//            }
//            else if(oldDis <= mTriggerDistnce && nowDis > mTriggerDistnce)
//            {
//                mDisState = 2;
//                return true;
//            }
//        }
//        return false;
//    }
	
//    protected override void OnHitTraigger ()
//    {
//        switch(mDisState)
//        {
//        case 1:
//            mTrigerTarget.SendMessage(mEnterFuncName,SendMessageOptions.DontRequireReceiver);
//            break;
//        case 2:
//            mTrigerTarget.SendMessage(mExitFuncName,SendMessageOptions.DontRequireReceiver);
//            break;
//        }
//    }
//}
