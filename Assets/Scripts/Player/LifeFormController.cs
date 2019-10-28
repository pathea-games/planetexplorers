using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using SkillAsset;

public enum LifeFormProperty
{
	LFP_Stamina = 1,
	LFP_Oxygen = 2,
	LFP_Food_Min = 3,
	LFP_Food_Cereals = 3,
	LFP_Food_Meat = 4,
	LFP_Food_Bean = 5,
	LFP_Food_Fruit = 6,
	LFP_Food_Energy = 7,
	LFP_Food_Max = 7,
}

public class LifeFormRule
{
	static Dictionary<int, LifeFormRule> s_tblRules;
	public int 				mID;
	public float			mUpdateInterval;
	public int				mPropertyType;
	public float			mPropertyValueMax;
	public int				mConditionType;
	public float			mConditionMin;
	public float			mConditionMax;
	public int				mCostSkillID;
	
	public static void LoadData()
	{
		s_tblRules = new Dictionary<int, LifeFormRule>();
		
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("LifeFormRule");
		while (reader.Read())
		{
			LifeFormRule addRule = new LifeFormRule();
			addRule.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
			addRule.mUpdateInterval = Convert.ToSingle(reader.GetString(reader.GetOrdinal("UpdateInterval")));
			addRule.mPropertyType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("PropertyType")));
			addRule.mPropertyValueMax = Convert.ToSingle(reader.GetString(reader.GetOrdinal("PropertyValueMax")));
			addRule.mConditionType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ConditionType")));
			addRule.mConditionMin = Convert.ToSingle(reader.GetString(reader.GetOrdinal("ConditionMin")));
			addRule.mConditionMax = Convert.ToSingle(reader.GetString(reader.GetOrdinal("ConditionMax")));
			addRule.mCostSkillID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("CostSkillID")));
			s_tblRules[addRule.mID] = addRule;
		}
	}
	
	public static LifeFormRule GetRule(int id)
	{
        if (s_tblRules.ContainsKey(id))
            return s_tblRules[id];
        else
        {
            Debug.LogError("Can't find rule : " + id);
            return null;
        }
	}
}

public class LifeFormController : MonoBehaviour 
{
	public List<int>					mRules;
	
	List<int>							mActiveRules;
	Dictionary<int,float>			   	mRulesDecreaseStackCount;
	List<int>							mIndexList;
	Dictionary<int,float> 				mPropertys;
	Dictionary<int,float> 				mPropertyMaxs;
	SkillRunner							mSkillRunner;
	
	//float 								FoodCheckInterval;
	
	void Awake () 
	{
		mActiveRules = new List<int>();
		mRulesDecreaseStackCount = new Dictionary<int, float>();
		mIndexList = new List<int>();
		mPropertys = new Dictionary<int, float>();
		mPropertyMaxs = new Dictionary<int, float>();

        if (mRules == null)
            mRules = new List<int>();

		foreach(int id in mRules)
			AddRule(id);

		mSkillRunner = GetComponent<SkillRunner>();
		
		//FoodCheckInterval = 1;

//		enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//PLayer only Update
        //Player player = mSkillRunner as Player;
        //if(null != player)
        //{
        //    mPropertys[(int)LifeFormProperty.LFP_Stamina] = player.GetAttribute(Pathea.AttribType.Comfort);
        //    mPropertys[(int)LifeFormProperty.LFP_Oxygen] = player.GetAttribute(Pathea.AttribType.Oxygen);
        //    FoodCheckInterval -= Time.deltaTime;
        //    if(FoodCheckInterval < 0)
        //    {
        //        FoodCheckInterval = 1f;
        //        int Count = 0;
        //        foreach(int property in mPropertys.Keys)
        //        {
        //            if((LifeFormProperty)property >= LifeFormProperty.LFP_Food_Min
        //                && (LifeFormProperty)property <= LifeFormProperty.LFP_Food_Max
        //                && mPropertys[property] > 0)
        //            {
        //                Count++;
        //            }
        //        }
        //        if(Count > 0 && !mSkillRunner.IsEffRunning(10020 + Count))
        //            mSkillRunner.RunEff(10020 + Count, mSkillRunner);
        //    }
        //}
		//CheckRules
		foreach(int index in mIndexList)
		{
			mRulesDecreaseStackCount[index] -= Time.deltaTime;
			if(mRulesDecreaseStackCount[index] < 0)
			{
				LifeFormRule rule = LifeFormRule.GetRule(index);
				mRulesDecreaseStackCount[index] = rule.mUpdateInterval;
				if((rule.mConditionType == 0 && mPropertys[rule.mPropertyType] >= rule.mConditionMin && mPropertys[rule.mPropertyType] <= rule.mConditionMax)
					||(rule.mConditionType == 1 && mPropertys[rule.mPropertyType]/mPropertyMaxs[rule.mPropertyType] >= rule.mConditionMin 
												&& mPropertys[rule.mPropertyType]/mPropertyMaxs[rule.mPropertyType] <= rule.mConditionMax))
					mSkillRunner.RunEff(rule.mCostSkillID, mSkillRunner);
			}
		}
	}
	
	public void AddRule(int ruleID)
	{
		if(mActiveRules.Contains(ruleID))
			return;
		mActiveRules.Add(ruleID);
		LifeFormRule addRule = LifeFormRule.GetRule(ruleID);
		mRulesDecreaseStackCount[addRule.mID] = addRule.mUpdateInterval;
		mIndexList.Add(addRule.mID);
		mPropertyMaxs[addRule.mPropertyType] = addRule.mPropertyValueMax;
		mPropertys[addRule.mPropertyType] = 0;
	}
	
	public void ApplyPropertyChange(Dictionary<int, float> propertyChanges)
	{
		foreach(int property in propertyChanges.Keys)
		{
			if(mPropertys.ContainsKey(property))
			{
				mPropertys[property] = Mathf.Clamp(mPropertys[property] + propertyChanges[property], 0, mPropertyMaxs[property]);
				break;
			}
		}
	}
}
