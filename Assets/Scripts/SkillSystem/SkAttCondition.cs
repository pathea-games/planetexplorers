using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace SkillSystem
{
	public class AttCond
	{
		protected SkEntity mSkEntity;
		public AttCond(SkEntity skEntity)
		{
			mSkEntity = skEntity;
		}
		public virtual bool Check() { return false; }
	}

	public class AttCond_Compare : AttCond
	{
		int mType; // 0:SelfValue 1:ConPercent
		bool mLess;
		int mTargetAtt;
		int mConAtt;
		float mTargetValue;
		
		//para ex: "compare,less,1,2,0.6"
		public AttCond_Compare(SkEntity skEntity, string[] para) : base(skEntity)
		{
			mType = (para.Length == 5) ? 1 : 0;
			mLess = (para[1].ToLower() == "less");
			mTargetAtt = Convert.ToInt32(para[2]);
			switch(mType)
			{
			case 0:
				mTargetValue = Convert.ToSingle(para[3]);
				break;
			case 1:
				mConAtt = Convert.ToInt32(para[3]);
				mTargetValue = Convert.ToSingle(para[4]);
				break;
			}
		}
		
		public override bool Check ()
		{
			if(mLess)
			{
				switch(mType)
				{
				case 0:
					return mSkEntity.attribs.sums[mTargetAtt] <= mTargetValue;
				case 1:
					return mSkEntity.attribs.sums[mTargetAtt] <= mTargetValue * mSkEntity.attribs.sums[mConAtt];
				}
			}
			else
			{
				switch(mType)
				{
				case 0:
					return mSkEntity.attribs.sums[mTargetAtt] >= mTargetValue;
				case 1:
					return mSkEntity.attribs.sums[mTargetAtt] >= mTargetValue * mSkEntity.attribs.sums[mConAtt];
				}
			}
			return base.Check();
		}
	}

	public class AttCond_Camp : AttCond
	{
		int mType; // 0:SelfValue 1:ConPercent
		int mTargetAtt;
		int mConAtt;
		float mMin;
		float mMax;
		
		//para ex: "camp,1,2,0.2,0.6"
		public AttCond_Camp(SkEntity skEntity, string[] para) : base(skEntity)
		{
			mType = (para.Length == 5) ? 1 : 0;
			mTargetAtt = Convert.ToInt32(para[1]);
			switch(mType)
			{
			case 0:
				mMin = Convert.ToSingle(para[2]);
				mMax = Convert.ToSingle(para[3]);
				break;
			case 1:
				mConAtt = Convert.ToInt32(para[2]);
				mMin = Convert.ToSingle(para[3]);
				mMax = Convert.ToSingle(para[4]);
				break;
			}
		}
		
		public override bool Check ()
		{
			switch(mType)
			{
			case 0:
				return mMin <= mSkEntity.attribs.sums[mTargetAtt] && mSkEntity.attribs.sums[mTargetAtt] <= mMax;
			case 1:
				return mMin * mSkEntity.attribs.sums[mConAtt] <= mSkEntity.attribs.sums[mTargetAtt] 
				&& mSkEntity.attribs.sums[mTargetAtt] <= mMax * mSkEntity.attribs.sums[mConAtt];
			}
			return base.Check();
		}
	}

	public class AttCond_RunSkill : AttCond
	{
		int 	mSkillID;
		bool 	mCheckRun;
		
		public AttCond_RunSkill(SkEntity skEntity, string[] para) : base (skEntity)
		{
			mSkillID = Convert.ToInt32(para[1]);
			mCheckRun = Convert.ToBoolean(para[2]);
		}
		
		public override bool Check ()
		{
			return mSkEntity.IsSkillRunning(mSkillID) == mCheckRun;
		}
	}

	public class AttCondCtrl
	{
		List<AttCond> mConds;
		public AttCondCtrl(SkEntity skEntity, string para) 
		{
			mConds = new List<AttCond>();
			string[] condStrs = para.Split(';');
			foreach(string condStr in condStrs)
			{
				string[] paraStr = condStr.Split(',');
				switch(paraStr[0].ToLower())
				{
				case "compare":
					AttCond_Compare condCompare = new AttCond_Compare(skEntity, paraStr);
					mConds.Add(condCompare);
					break;
				case "camp":
					AttCond_Camp condCamp = new AttCond_Camp(skEntity, paraStr);
					mConds.Add(condCamp);
					break;
				case "RunSkill":
					AttCond_RunSkill condSkill = new AttCond_RunSkill(skEntity, paraStr);
					mConds.Add(condSkill);
					break;
				}
			}
		}
		public virtual bool Check() 
		{
			foreach(AttCond cond in mConds)
				if(!cond.Check())
					return false;
			return true; 
		}
	}
}
