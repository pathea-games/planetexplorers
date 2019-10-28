using Mono.Data.SqliteClient;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem
{
	public class AttRuleData
	{
		public int		mID;
		public string	mFilter;
		public string	mCond;
		public string	mAction;

		static Dictionary<int, AttRuleData> _RuleData;
		public static AttRuleData GetRuleData(int id)
		{
			if(_RuleData.ContainsKey(id))
				return _RuleData[id];
			return null;
		}
		public static void LoadData()
		{
			_RuleData = new Dictionary<int, AttRuleData>();
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AttRule");
			reader.Read(); // Firstline is exp
			while (reader.Read())
			{
				AttRuleData addData = new AttRuleData();
				addData.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
				addData.mFilter = reader.GetString(reader.GetOrdinal("filter"));
				addData.mCond = reader.GetString(reader.GetOrdinal("condition"));
				addData.mAction = reader.GetString(reader.GetOrdinal("action"));
			}
		}
	}

	public class AttRule
	{
		AttFilterCtrl mFilter;
		AttCondCtrl mCond;
		AttActionCtrl mAction;

		public void Update()
		{
			if(mFilter.CheckFilter())
				CheckAction();
		}

		public void CheckAction()
		{
			if(mCond.Check())
				mAction.DoAction();
		}

		public void Destroy()
		{
			mFilter.Destroy();
		}

		public static AttRule Creat(AttRuleCtrl ctrl, SkEntity skEntity, int ruleID)
		{
			AttRuleData data = AttRuleData.GetRuleData(ruleID);
			if(null != data)
			{
				AttRule rule = new AttRule();
				rule.mFilter = new AttFilterCtrl(skEntity, data.mFilter, rule.CheckAction);
				rule.mCond = new AttCondCtrl(skEntity, data.mCond);
				rule.mAction = new AttActionCtrl(ctrl, skEntity, data.mAction);
				return rule;
			}
			return null;
		}
	}

	public class AttRuleCtrl
	{
		SkEntity mSkEntity;

		Dictionary<int, AttRule> mRuleDic;

		List<int> mRemoveList;

		public AttRuleCtrl(SkEntity skEntity)
		{
			mSkEntity = skEntity;
			mRuleDic = new Dictionary<int, AttRule>();
			mRemoveList = new List<int>();
		}

		public void AddRule(int ID)
		{
			if(!mRuleDic.ContainsKey(ID))
			{
				AttRule addRule = AttRule.Creat(this, mSkEntity, ID);
				if(null != addRule)
					mRuleDic[ID] = addRule;
			}
		}

		public void RemoveRule(int ID)
		{
			if(mRuleDic.ContainsKey(ID))
				mRemoveList.Add(ID);
		}

		public void Update()
		{
			foreach(int ID in mRemoveList)
			{
				mRuleDic[ID].Destroy();
				mRuleDic.Remove(ID);
			}
			mRemoveList.Clear();
			foreach(AttRule rule in mRuleDic.Values)
				rule.Update();
		}
	}
}