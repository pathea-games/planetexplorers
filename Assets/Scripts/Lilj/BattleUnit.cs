using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

//[Flags]
//public enum BattleUnitType
//{
//	MoveAbleUnit = 1,
//	Nature = 1<<1,
//	Machine = 1<<2,
//	EnergySupply = 1<<3,
//	AmmoSupply = 1<<4,
//	Attacker = 1<<5,
//	Healer = 1<<6
//}

public enum EffectRange
{
	Single = 0,
	Range,
	spread,
}

public class BattleUnitData
{
    public static List<BattleUnitData> s_tblBattleUnitData;
	public int	 mID;
	public int 	 mType;
	public string mName;
	public string mPerfabPath;
	
	public float mMaxHp;
	public float mMaxEn;
	public float mMaxAmmo;
	
	public float mAtk;
	public int 	 mAtkType;
	public float mAtkInterval;
	public EffectRange	 mAtkRange;
	public float mDef;
	public int 	 mDefType;
	public float mRps;
	
	public float mHealPs;
	public int	 mHealType;
	public EffectRange	 mHealRange;
	
	public float mEnCostPs;
	public float mAmmoCostPs;
	
	public bool  mPlayerForce;
	public float mMoveInterval;
	public float mSpreadFactor;
	
	public static BattleUnitData GetBattleUnitData(int id)
	{
		return s_tblBattleUnitData.Find(itr=> (itr.mID == id));
	}
	
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("simulation");
        s_tblBattleUnitData = new List<BattleUnitData>();
        while (reader.Read())
        {
            BattleUnitData bud = new BattleUnitData();
			bud.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
			bud.mName = reader.GetString(reader.GetOrdinal("Name"));
			bud.mPerfabPath = reader.GetString(reader.GetOrdinal("PerfabPath"));
			bud.mType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
			bud.mMaxHp = Convert.ToSingle(reader.GetString(reader.GetOrdinal("MaxHP")));
			bud.mAtk = Convert.ToSingle(reader.GetString(reader.GetOrdinal("Atk")));
			bud.mAtkType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Atk_T")));
			bud.mDef = Convert.ToSingle(reader.GetString(reader.GetOrdinal("Def")));
			bud.mDefType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Def_T")));
			bud.mAtkInterval = Convert.ToSingle(reader.GetString(reader.GetOrdinal("Atk_P")));
			bud.mAtkRange = (EffectRange)Convert.ToInt32(reader.GetString(reader.GetOrdinal("AtkRange")));
			bud.mRps = Convert.ToSingle(reader.GetString(reader.GetOrdinal("RPS")));
			bud.mHealPs = Convert.ToSingle(reader.GetString(reader.GetOrdinal("HealPS")));
			bud.mHealType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Heal_T")));
			bud.mHealRange = (EffectRange)Convert.ToInt32(reader.GetString(reader.GetOrdinal("HealRange")));
			bud.mMaxEn = Convert.ToSingle(reader.GetString(reader.GetOrdinal("EN")));
			bud.mMaxAmmo = Convert.ToSingle(reader.GetString(reader.GetOrdinal("Ammo")));
			bud.mEnCostPs = Convert.ToSingle(reader.GetString(reader.GetOrdinal("EnCostPS")));
			bud.mAmmoCostPs = Convert.ToSingle(reader.GetString(reader.GetOrdinal("AmmoCost")));
			bud.mPlayerForce = (Convert.ToInt32(reader.GetString(reader.GetOrdinal("PlaterForce"))) != 0);
			bud.mMoveInterval = Convert.ToSingle(reader.GetString(reader.GetOrdinal("MoveInt")));
			bud.mSpreadFactor = Convert.ToSingle(reader.GetString(reader.GetOrdinal("SpreadFac")));
            s_tblBattleUnitData.Add(bud);
        }
    }
}

public class BattleUnit : MonoBehaviour 
{
	public int	 mID;
	public int 	 mType;
	
	public float mMaxHp;
	public float mHp;
	public float mMaxEn;
	public float mEn;
	public float mMaxAmmo;
	public float mAmmo;
	
	public float mAtk;
	public int 	 mAtkType;
	public float mAtkInterval;
	public EffectRange	 mAtkRange;
	public float mDef;
	public int 	 mDefType;
	
	public float mRps;
	
	public float mHealPs;
	public int	 mHealType;
	public EffectRange	 mHealRange;
	
	public float mEnCostPs;
	public float mAmmoCostPs;
	
	public bool  mPlayerForce;
	
	public float mMoveInterval = 5;
	float		 mMoveCooldownTime;
	
	public float mSpreadFactor;
	
	public float mBE; //BattleEffectiveness
	
	void Start()
	{
		ReCountBE();
		mHp = mMaxHp;
	}
	
	public void ReCountBE()
	{
		mBE = mAtk * mMaxHp * (mDef + 200f) / mDef;
	}
	
	public void SetData(BattleUnitData bud)
	{
		mID = bud.mID;
		mType = bud.mType;
		mHp = mMaxHp = bud.mMaxHp;
		mEn = mMaxEn = bud.mMaxEn;
		mAmmo = mMaxAmmo = bud.mMaxAmmo;
		
		mAtk = bud.mAtk;
		mAtkType = bud.mAtkType;
		mAtkInterval = bud.mAtkInterval;
		mAtkRange = bud.mAtkRange;
		mDef = bud.mDef;
		mDefType = bud.mDefType;
		
		mRps = bud.mRps;
		
		mHealPs = bud.mHealPs;
		mHealType = bud.mHealType;
		mHealRange = bud.mHealRange;
		
		mEnCostPs = bud.mEnCostPs;
		mAmmoCostPs = bud.mAmmoCostPs;
		
		mPlayerForce = bud.mPlayerForce;
		mMoveInterval = bud.mMoveInterval;
		mSpreadFactor = bud.mSpreadFactor;
		ReCountBE();
	}
	
	public void Move()
	{
		mMoveCooldownTime = mMoveInterval;
	}
	
	public bool CanMove()
	{
		return mMoveCooldownTime <= 0;
	}
	
	void Update()
	{
		if(mMoveCooldownTime > 0)
			mMoveCooldownTime -= Time.deltaTime;
	}
}
