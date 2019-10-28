using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using ItemAsset;
using WhiteCat;

public class Gun : ShootEquipment 
{
 	public VCPGunHandle			mGunHandle;
	public List<VCPGunMuzzle> 	mGunMuzzle;
	List<float>					    mCoolDownTime;
	protected int 				    mCurrentIndex;
	//private int                     mLastIndex;
	public  float                   mRuntimeAccuracyScale = 1;
	
	public override void InitEquipment (SkillRunner runner, ItemObject item)
	{
		mCurrentIndex = 0;
		//mLastIndex = 0;
		mCoolDownTime = new List<float>();
        for(int i = 0; i < mGunMuzzle.Count; i++)
		//foreach (VCPGunMuzzle muzzle in mGunMuzzle)
			mCoolDownTime.Add(Time.time);
		base.InitEquipment(runner,item);
	}
	
	public virtual EArmType GetArmType ()
	{
		return mGunMuzzle[0].ArmType;
	}
	
	public override bool CostSkill (ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
        if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
        {
            return false;
        }
		
		if (mShootState == ShootState.Aim || mShootState == ShootState.Fire)
		{
			for (int i = 0; i < mGunMuzzle.Count; i++)
			{
				int idx = (i + mCurrentIndex + 1) % mGunMuzzle.Count;
				if (( mGunMuzzle[idx].Multishot && Time.time - mCoolDownTime[idx] > mGunMuzzle[idx].FireInterval && (buttonDown || buttonPressed) )
					|| (!mGunMuzzle[idx].Multishot && buttonDown))
				{
					if (mHuman.CheckAmmoCost(mGunMuzzle[idx].ArmType, mGunMuzzle[idx].CostItemId))
					{
						mCurrentIndex = idx;
						Transform muzzle = mGunMuzzle[mCurrentIndex].End;
						float base_acc = 0.5f;
						if ( mGunMuzzle[mCurrentIndex] != null )
							base_acc = mGunMuzzle[mCurrentIndex].Accuracy;
						float accuracy = mRuntimeAccuracyScale*base_acc;
						float xErr = (Random.value-0.5f)*1.8f*accuracy;
						float yErr = (Random.value-0.5f)*1.8f*accuracy;
						muzzle.localEulerAngles = new Vector3(xErr,yErr,0);

						EffSkillInstance skillInstance = CostSkill(mSkillRunner, mGunMuzzle[idx].SkillId, target);
						if (null != skillInstance)
						{
							mHuman.ApplyDurabilityReduce(0);
							mCoolDownTime[idx] = Time.time;
							mGunMuzzle[idx].PlayMuzzleEffect();
							mHuman.ApplyAmmoCost(mGunMuzzle[idx].ArmType, mGunMuzzle[idx].CostItemId);
							return true;
						}
					}
				}
			}
		}
		return false;
	}
	
//	internal override Transform GetCastTransform (SkillAsset.EffItemCast cast)
//	{
//		Transform muzzle = mGunMuzzle[mCurrentIndex].End;
//		return muzzle;
//	}
}
