using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem
{
	public class AttAction
	{
		protected SkEntity mSkEntity;
		
		public AttAction(SkEntity skEntity)
		{
			mSkEntity = skEntity;
		}
		
		public virtual void Do() { }
	}
	
	public class AttAction_Skill : AttAction
	{
		int mSkillID;
		
		public AttAction_Skill(SkEntity skEntity, string[] para) : base(skEntity)
		{
			mSkillID = Convert.ToInt32(para[1]);
		}
		
		public override void Do ()
		{
			mSkEntity.StartSkill(mSkEntity, mSkillID);
		}
	}
	
	public class AttAction_Att : AttAction
	{
		int mType; // 0:SelfValue 1:ConPercent
		int mTargetAtt;
		int mConAtt;
		float mTargetValue;
		
		public AttAction_Att(SkEntity skEntity, string[] para) : base(skEntity)
		{
			mType = (para.Length == 4) ?1:0 ;
			mTargetAtt = Convert.ToInt32(para[1]);
			switch(mType)
			{
			case 0:
				mTargetValue = Convert.ToSingle(para[2]);
				break;
			case 1:
				mConAtt = Convert.ToInt32(para[2]);
				mTargetValue = Convert.ToSingle(para[3]);
				break;
			}
		}
		
		public override void Do ()
		{
			switch(mType)
			{
			case 0:
				mSkEntity.attribs.sums[mTargetAtt] = mTargetValue;
				break;
			case 1:
				mSkEntity.attribs.sums[mTargetAtt] = mTargetValue * mSkEntity.attribs.sums[mConAtt];
				break;
			}
		}
	}
	
	public class AttAction_Action : AttAction
	{
		AttRuleCtrl mRuleCtrl;
		bool	mIsAdd;
		int 	mRuleID;
		
		public AttAction_Action(SkEntity skEntity, string[] para, AttRuleCtrl ruleCtrl) : base(skEntity)
		{
			mIsAdd = para[1].ToLower() == "add";
			mRuleID = Convert.ToInt32(para[2]);
			mRuleCtrl = ruleCtrl;
		}
		
		public override void Do ()
		{
			if(mIsAdd)
				mRuleCtrl.AddRule(mRuleID);
			else
				mRuleCtrl.RemoveRule(mRuleID);
		}
	}

	public class AttAction_Func : AttAction
	{
		string mFuncName;
		public AttAction_Func(SkEntity skEntity, string[] para) : base(skEntity)
		{
			mFuncName = para[1];
		}

		public override void Do ()
		{
			mSkEntity.SendMessage(mFuncName, SendMessageOptions.DontRequireReceiver);
		}
	}

	public class AttActionCtrl
	{
		List<AttAction> mActions;
		public AttActionCtrl(AttRuleCtrl ctrl, SkEntity skEntity, string para)
		{
			mActions = new List<AttAction>();
			string[] condStrs = para.Split(';');
			foreach(string condStr in condStrs)
			{
				string[] paraStr = condStr.Split(',');
				switch(paraStr[0].ToLower())
				{
				case "skill":
					AttAction_Skill actSkill = new AttAction_Skill(skEntity, paraStr);
					mActions.Add(actSkill);
					break;
				case "camp":
					AttAction_Att actAtt = new AttAction_Att(skEntity, paraStr);
					mActions.Add(actAtt);
					break;
				case "func":
					AttAction_Func actFunc = new AttAction_Func(skEntity, paraStr);
					mActions.Add(actFunc);
					break;
				case "Action":
					AttAction_Action actAct = new AttAction_Action(skEntity, paraStr, ctrl);
					mActions.Add(actAct);
					break;
				}
			}
		}
		public void DoAction()
		{
			foreach(AttAction act in mActions)
				act.Do();
		}
	}

}
