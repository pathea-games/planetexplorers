//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//
//public class AnimCmpt_Animator : MonoBehaviour
//{
//	class AnimAction
//	{
//		public class AnimEventReq
//		{
//			public float 	mTime;
//			public Action	mEventFunc;
//		}
//		
//		Animator mAnimator;
//		string mAnimName;
//		int mLayer;
//		bool mFindAnimInfo;
//		AnimatorStateInfo mAnimInfo;
//		List<AnimEventReq> mEventReqList;
//		
//		UTimer mTimer;
//		
//		public AnimAction(Animator animator, string name)
//		{
//			mAnimator = animator;
//			mAnimName = name;
//			mTimer = new UTimer();
//			mTimer.ElapseSpeed = -1;
//			mTimer.Second = 0.5f;
//			mEventReqList = new List<AnimEventReq>(1);
//			mFindAnimInfo = false;
//		}
//		
//		// Return end action
//		public bool Update()
//		{
//			if(mFindAnimInfo)
//			{
//				GetAnimInfo();
//				mTimer.Update(Time.deltaTime);
//				if(mTimer.Second < 0)
//					return true;
//				return false;
//			}
//			
//			UpdateEvent();
//			
//			return CheckEnd();
//		}
//		
//		public void AddEvent(float time, System.Action EventFunc)
//		{
//			AnimEventReq eventReq = new AnimEventReq();
//			eventReq.mTime = time;
//			eventReq.mEventFunc = EventFunc;
//			mEventReqList.Add(eventReq);
//		}
//		
//		void GetAnimInfo()
//		{
//			int layerCount = mAnimator.layerCount;
//			for(int i = 0; i < mAnimator.layerCount; i++)
//			{
//				string layerAnimName = mAnimator.GetLayerName(i) + "." + mAnimName;
//				AnimatorStateInfo animInfo = mAnimator.GetCurrentAnimatorStateInfo(i);
//				if(animInfo.IsName(layerAnimName))
//				{
//					mLayer = i;
//					mAnimInfo = animInfo;
//					mFindAnimInfo = true;
//					return;
//				}
//				animInfo = mAnimator.GetNextAnimatorStateInfo(i);
//				if(mAnimator.IsInTransition(i) && animInfo.IsName(layerAnimName))
//				{
//					mLayer = i;
//					mAnimInfo = animInfo;
//					mFindAnimInfo = true;
//					return;
//				}
//			}
//		}
//		
//		void UpdateEvent()
//		{
//			for(int i = mEventReqList.Count - 1; i >= 0; i--)
//			{
//				if(mAnimInfo.length * mAnimInfo.normalizedTime >= mEventReqList[i].mTime)
//				{
//					mEventReqList[i].mEventFunc();
//					mEventReqList.RemoveAt(i);
//				}
//			}
//		}
//		
//		bool CheckEnd()
//		{
//			AnimatorStateInfo animInfo = mAnimator.GetCurrentAnimatorStateInfo(mLayer);
//			if(!animInfo.IsName(mAnimator.GetLayerName(mLayer) + "." + mAnimName))
//			{
//				animInfo = mAnimator.GetNextAnimatorStateInfo(mLayer);
//				if(!animInfo.IsName(mAnimator.GetLayerName(mLayer) + "." + mAnimName))
//				{
//					foreach(AnimEventReq req in mEventReqList)
//						req.mEventFunc();
//					return true;
//				}
//			}
//			return false;
//		}
//	}
//	
//	Dictionary<string, AnimAction> mAnimDic;
//
//	Animator mAnimator;
//
//	public AnimCmpt_Animator ()
//	{
//		mAnimDic = new Dictionary<string, AnimAction>();
//	}
//
//	bool CheckAnimator()
//	{
//		if(null == mAnimator)
//		{
//			mAnimator = Entity.GetGameObject().GetComponentInChildren<Animator>();
//			ResetLayerWeight();
//		}
//
//		return null != mAnimator;
//	}
//	
//	bool GetAnimInfo(string animName, out AnimatorStateInfo animInfo)
//	{
//		for(int i = 0; i < mAnimator.layerCount; i++)
//		{
//			string layerAnimName = mAnimator.GetLayerName(i) + "." + animName;
//			AnimatorStateInfo findAnimInfo = mAnimator.GetCurrentAnimatorStateInfo(i);
//			if(findAnimInfo.IsName(layerAnimName))
//			{
//				animInfo = findAnimInfo;
//				return true;
//			}
//			if(mAnimator.IsInTransition(i))
//			{
//				findAnimInfo = mAnimator.GetNextAnimatorStateInfo(i);
//				if(findAnimInfo.IsName(layerAnimName))
//				{
//					animInfo = findAnimInfo;
//					return true;
//				}
//			}
//		}
//		animInfo = new AnimatorStateInfo();
//		return false;
//	}
//	
//	void ResetLayerWeight()
//	{
//		for(int i = 0; i < mAnimator.layerCount; i++)
//			mAnimator.SetLayerWeight(i, 1f);
//	}
//	
//	#region implemented abstract members of AnimCtrl
//	public override void PlayAnim (string animName, bool immediately = false)
//	{
//		if(CheckAnimator())
//		{
//			mAnimator.SetTrigger(animName);
//			if(immediately)
//				mAnimator.Play(animName);
//			
//			if(!mAnimDic.ContainsKey(animName))
//				mAnimDic[animName] = new AnimAction(mAnimator, animName);
//		}
//	}
//	
//	public override void SetAnimState (string animName, bool play)
//	{
//		if(CheckAnimator())
//		{
//			mAnimator.SetBool(animName, play);
//			if(play)
//			{
//				if(!mAnimDic.ContainsKey(animName))
//					mAnimDic[animName] = new AnimAction(mAnimator, animName);
//			}
//			else
//				mAnimDic.Remove(animName);
//		}
//	}
//	
//	public override void SetAnimState (string animName, float f)
//	{
//		if(CheckAnimator())
//			mAnimator.SetFloat(animName, f);
//	}
//	
//	public override void SetAnimState (string animName, int i)
//	{
//		if(CheckAnimator())
//			mAnimator.SetFloat(animName, i);
//	}
//	
//	public override void SetAnimEvent (string animName, float time, System.Action EventFunc)
//	{
//		if(mAnimDic.ContainsKey(animName))
//			mAnimDic[animName].AddEvent(time, EventFunc);
//	}
//	
//	public override bool IsAnimPlaying (string animName)
//	{
//		return mAnimDic.ContainsKey(animName);
//	}
//	
//	public override float GetAnimPlayTime (string animName)
//	{
//		if(CheckAnimator())
//		{
//			AnimatorStateInfo animInfo;
//			if(GetAnimInfo(animName, out animInfo))
//				return animInfo.normalizedTime * animInfo.length;
//		}
//		return 0;
//	}
//	
//	public override void Update ()
//	{
//		if(CheckAnimator())
//		{
//			List<string> removeList = new List<string>(2);
//			foreach(string animName in mAnimDic.Keys)
//				if(mAnimDic[animName].Update())
//					removeList.Add(animName);
//			foreach(string animName in removeList)
//			{
//				mAnimDic.Remove(animName);
//				OnAnimEnd(animName);
//			}
//		}
//		else
//		{
//			foreach(string animName in mAnimDic.Keys)
//				OnAnimEnd(animName);
//			mAnimDic.Clear();
//		}
//	}
//	#endregion
//}