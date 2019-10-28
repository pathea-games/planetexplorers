//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using ItemAsset;
//using SkillAsset;
//
//public class MeleeWeapon : Equipment
//{
//	
//	public List<int> 	mMoveAttackMaleId;
//	public List<int>	mMoveAttackFemaleId;
//	
//	public List<float> mMoveSpeedMale;
//	public List<float> mMoveSpeedFemale;
//	
//	public List<float> mMoveStartTimeMale;
//	public List<float> mMoveStartTimeFemale;
//	
//	public List<float> mMoveEndTimeMale;
//	public List<float> mMoveEndTimeFemale;
//	
//	
//	public List<float> mMoveSpeedMaleInRun;
//	public List<float> mMoveSpeedFemaleInRun;
//	
//	public List<float> mMoveStartTimeMaleInRun;
//	public List<float> mMoveStartTimeFemaleInRun;
//	
//	public List<float> mMoveEndTimeMaleInRun;
//	public List<float> mMoveEndTimeFemaleInRun;
//	
//	Vector3 mMoveDir = Vector3.forward;
//	
//	float mCurrentMoveSpeed;
//	float mCurrentMoveStartTime;
//	float mCurrentMoveEndTime;
//	float mCurrentMoveTime;
//	bool  mMove = false;
//	
//	WeaponTrail mWeaponTrail;
//	float		tempT;
//	
//	public override void InitEquipment (SkillRunner runner, ItemObject item)
//	{
//		base.InitEquipment (runner, item);
//		
//		Transform fTrailtransform = transform.FindChild("WeaponTrail");
//		if(fTrailtransform != null)
//		{
//			mWeaponTrail = fTrailtransform.GetComponent<WeaponTrail>();
//            if (mWeaponTrail != null)
//            {
//                mWeaponTrail.FadeOut(0.0f);
//				mWeaponTrail.ClearTrail();
//            }
//		}
//	}
//	
//	// Use this for initialization
//	public override bool CostSkill(ISkillTarget target = null, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
//	{
//        if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
//        {
//            return false;
//        }
//		if(buttonDown)
//		{
//			mCastSkillId = 0;
//			
//			float moveSpeed = 0;
//			if(null != mSkillRunner.GetComponent<Rigidbody>())
//				moveSpeed = mSkillRunner.GetComponent<Rigidbody>().velocity.magnitude;
//			else if(null != mSkillRunner.GetComponent<CharacterController>())
//				moveSpeed = mSkillRunner.GetComponent<CharacterController>().velocity.magnitude;
//			
//			mSkillIndex = 0;
//			mCastSkillId = -1;
//			switch(sex)
//			{
//			case 1:
//				for(int i = 0; i < mMoveAttackFemaleId.Count; i++)
//				{
//					if(mSkillRunner.IsEffRunning(mMoveAttackFemaleId[i]))
//					{
//						if(i == (mMoveAttackFemaleId.Count - 1))
//						{
//							mCastSkillId = mMoveAttackFemaleId[0];
//							mSkillIndex = 0;
//						}
//						else
//						{
//							mCastSkillId = mMoveAttackFemaleId[i + 1];
//							mSkillIndex = i + 1;
//						}
//						
//						mCurrentMoveSpeed = mMoveSpeedFemaleInRun[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeFemaleInRun[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeFemaleInRun[mSkillIndex];
//					}
//				}
//				
//				for(int i = 0; i < mSkillFemaleId.Count - 1; i++)
//				{
//					if(mSkillRunner.IsEffRunning(mSkillFemaleId[i]))
//					{
//						mCastSkillId = mSkillFemaleId[i + 1];
//						mSkillIndex = i + 1;
//						
//						mCurrentMoveSpeed = mMoveSpeedFemale[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeFemale[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeFemale[mSkillIndex];
//					}
//				}
//				
//				if(-1 == mCastSkillId)
//				{
//					mSkillIndex = 0;
//					if(moveSpeed > 3)
//					{
//						mCastSkillId = mMoveAttackFemaleId[0];
//						mCurrentMoveSpeed = mMoveSpeedFemaleInRun[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeFemaleInRun[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeFemaleInRun[mSkillIndex];
//					}
//					else
//					{
//						mCastSkillId = mSkillFemaleId[0];
//						mCurrentMoveSpeed = mMoveSpeedFemale[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeFemale[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeFemale[mSkillIndex];
//					}
//				}
//				
//				break;
//			case 2:
//				for(int i = 0; i < mMoveAttackMaleId.Count; i++)
//				{
//					if(mSkillRunner.IsEffRunning(mMoveAttackMaleId[i]))
//					{
//						if(i == (mMoveAttackMaleId.Count - 1))
//						{
//							mCastSkillId = mMoveAttackMaleId[0];
//							mSkillIndex = 0;
//						}
//						else
//						{
//							mCastSkillId = mMoveAttackMaleId[i + 1];
//							mSkillIndex = i + 1;
//						}
//						
//						mCurrentMoveSpeed = mMoveSpeedMaleInRun[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeMaleInRun[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeMaleInRun[mSkillIndex];
//					}
//				}
//				
//				for(int i = 0; i < mSkillMaleId.Count - 1; i++)
//				{
//					if(mSkillRunner.IsEffRunning(mSkillMaleId[i]))
//					{
//						mCastSkillId = mSkillMaleId[i + 1];
//						mSkillIndex = i + 1;
//						
//						mCurrentMoveSpeed = mMoveSpeedMale[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeMale[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeMale[mSkillIndex];
//					}
//				}
//				
//				if(-1 == mCastSkillId)
//				{
//					mSkillIndex = 0;
//					if(moveSpeed > 3)
//					{
//						mCastSkillId = mMoveAttackMaleId[0];
//						mCurrentMoveSpeed = mMoveSpeedMaleInRun[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeMaleInRun[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeMaleInRun[mSkillIndex];
//					}
//					else
//					{
//						mCastSkillId = mSkillMaleId[0];
//						mCurrentMoveSpeed = mMoveSpeedMale[mSkillIndex];
//						mCurrentMoveStartTime = mMoveStartTimeMale[mSkillIndex];
//						mCurrentMoveEndTime = mMoveEndTimeMale[mSkillIndex];
//					}
//				}
//				
////				if(moveSpeed > 3)
////				{
////					if(mMoveAttackMaleId.Count == 0)
////						return false;
////					
////					mCastSkillId = mMoveAttackMaleId[0];
////					for(int i = 0; i < mMoveAttackMaleId.Count - 1; i++)
////					{
////						if(mSkillRunner.IsEffRunning(mMoveAttackMaleId[i]))
////						{
////							mCastSkillId = mMoveAttackMaleId[i + 1];
////							mSkillIndex = i + 1;
////						}
////					}
////					
////					mCurrentMoveSpeed = mMoveSpeedMaleInRun[mSkillIndex];
////					mCurrentMoveStartTime = mMoveStartTimeMaleInRun[mSkillIndex];
////					mCurrentMoveEndTime = mMoveEndTimeMaleInRun[mSkillIndex];
////				}
////				else
////				{
////					if(mSkillMaleId.Count == 0)
////						return false;
////					
////					mCastSkillId = mSkillMaleId[0];
////					for(int i = 0; i < mSkillMaleId.Count - 1; i++)
////					{
////						if(mSkillRunner.IsEffRunning(mSkillMaleId[i]))
////						{
////							mCastSkillId = mSkillMaleId[i + 1];
////							mSkillIndex = i + 1;
////						}
////					}
////					
////					mCurrentMoveSpeed = mMoveSpeedMale[mSkillIndex];
////					mCurrentMoveStartTime = mMoveStartTimeMale[mSkillIndex];
////					mCurrentMoveEndTime = mMoveEndTimeMale[mSkillIndex];
////				}
//				
//				break;
//			}
//			
//			EffSkillInstance skillInstance = CostSkill(mSkillRunner, mCastSkillId, target);
//			
//			if(null != mSkillRunner && null != target && null != skillInstance && mSkillRunner != target)
//			{
//	            Vector3 dir = target.GetPosition() - mSkillRunner.transform.position;
//	            dir.y = 0;
//	            mSkillRunner.transform.LookAt(mSkillRunner.transform.position + dir);
//			}
//			
//			if(null != skillInstance)
//			{
//				if(null != mWeaponTrail)
//				{
//					mWeaponTrail.ClearTrail();
//					mWeaponTrail.StartTrail(0.2f, 0.2f);
//				}
//				mMoveDir = mSkillRunner.transform.forward.normalized;
//				mCurrentMoveTime = 0;
//				mMove = true;
//
//				WeaponEffect we = GetComponent<WeaponEffect>();
//				if(null != we)
//					we.mAttackStart = true;
//				
//				return true;
//			}
//		}
//		return false;
//	}
//	
//	void LateUpdate ()
//	{
//		if(mMove)
//		{
//			if(mCurrentMoveTime > mCurrentMoveStartTime)
//			{
//				PhysicsCharacterMotor phyMotor = mSkillRunner.GetComponent<PhysicsCharacterMotor>();
//				if(null != phyMotor && phyMotor.grounded)
//					mSkillRunner.GetComponent<Rigidbody>().MovePosition(mCurrentMoveSpeed * mMoveDir * Time.deltaTime);
//			}
//			mCurrentMoveTime += Time.deltaTime;
//			if(mCurrentMoveTime >= mCurrentMoveEndTime)
//				mMove = false;
//		}
//		
//		if(null != mSkillRunner)
//		{
//			if(null != mWeaponTrail && !mSkillRunner.IsEffRunning(mCastSkillId))
//				mWeaponTrail.ClearTrail();
//		}
//		
//		if(null != mWeaponTrail)
//		{
//	        float mAnimTime = Mathf.Clamp(Time.deltaTime * 1, 0, 0.066f);
//			mWeaponTrail.Itterate (Time.time - mAnimTime + tempT);
//			tempT -= mAnimTime;
//			if (mWeaponTrail.time > 0)
//				mWeaponTrail.UpdateTrail (Time.time, mAnimTime);
//		}
//	}
//}
