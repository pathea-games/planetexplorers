using UnityEngine;
using System;
using System.Collections;

namespace SkillSystem
{
	public class AttFilter
	{
		public virtual bool CheckFilter() { return false; }
		public virtual void Destroy() { }
	}
	
	public class AttFilter_Event : AttFilter
	{
		SkEntity mSke;
		int mTargetAtt = 0;
		Action mCheckFunc;
		public AttFilter_Event(SkEntity ske, int targetAtt, Action checkFunc)
		{
			mSke = ske;
			mCheckFunc = checkFunc;
			mSke._attribs._OnAlterNumAttribs += OnAttChange;
		}
		
		public void OnAttChange(int att, float oldValue, float newValue)
		{
			if(att == mTargetAtt && null != mCheckFunc)
				mCheckFunc();
		}
		
		public override void Destroy ()
		{
			base.Destroy ();
			mSke._attribs._OnAlterNumAttribs -= OnAttChange;
		}
	}
	
	public class AttFilter_Time : AttFilter
	{
		float mIntervalTime;
		UTimer mTimer;
		
		public AttFilter_Time(float intervalTime)
		{
			mIntervalTime = intervalTime;
			mTimer = new UTimer();
			mTimer.ElapseSpeed = -1;
			mTimer.Second = mIntervalTime;
			
		}
		public override bool CheckFilter ()
		{
			mTimer.Update(Time.deltaTime);
			if(mTimer.Second <= 0)
			{
				mTimer.Second = mIntervalTime;
				return true;
			}
			return base.CheckFilter ();
		}
	}
	
	public class AttFilterCtrl
	{
		AttFilter mFilter;
		public AttFilterCtrl(SkEntity skEntity, string para, Action func)
		{
			string[] condStrs = para.Split(';');
			foreach(string condStr in condStrs)
			{
				string[] paraStr = condStr.Split(',');
				switch(paraStr[0].ToLower())
				{
				case "event":
					mFilter = new AttFilter_Event(skEntity, Convert.ToInt32(paraStr[1]), func);
					break;
				case "time":
					mFilter = new AttFilter_Time(Convert.ToSingle(paraStr[1]));
					break;
				}
			}
		}
		public bool CheckFilter()
		{
			return mFilter.CheckFilter();
		}
		public void Destroy()
		{
			mFilter.Destroy();
		}
	}
}
