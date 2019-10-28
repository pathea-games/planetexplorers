using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;

public class GrapplingHook : Gun 
{
	public Transform 		mHook;
	public List<Transform>	mLineList;
	List<Vector3>			mLinePosList;
	
	public float			mBulletSpeed = 100f;
	public float			mPushSpeed = 100f;
	public Vector3			mLineDownF = 0.01f * Vector3.down;
	public int				mFireSound;
	
	Vector3			mOffsetPos = Vector3.up;
	
	Vector3 mShootDir;
	Vector3	mTargetPos;
	
	int		mCurrentMoveIndex;
	float	mCurrentMoveDis;
	float	mClimpOffset = 0;
	
	enum HookGunState
	{
		Null,
		Shooting,
		Climbing,
		Back
	}
	
	HookGunState mState = HookGunState.Null;
	// Use this for initialization
	void Start () {
		mLinePosList = new List<Vector3>();
		for(int i = 0; i < mLineList.Count; i++)
			mLinePosList.Add(Vector3.zero);
	}
	
	void EndClimb()
	{
		List<string> anim = new List<string>();
		anim.Add("GrapplingEnd");
		mSkillRunner.ApplyAnim(anim);
		if(null != mSkillRunner.GetComponent<Rigidbody>())
			mSkillRunner.GetComponent<Rigidbody>().isKinematic = false;
	}
	
	public override void RemoveEquipment ()
	{
		EndClimb();
		base.RemoveEquipment ();
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		switch(mState)
		{
		case HookGunState.Shooting:
			UpdateBulletShootPos();
			break;
		case HookGunState.Back:
			UpdateBulletBackPos();
			break;
		case HookGunState.Climbing:
			if(null != mSkillRunner)
				UpdateUserPos();
			break;
		}
	}
	
	void UpdateBulletShootPos()
	{
		RaycastHit hitInfo;
		
		mHook.transform.position = mTargetPos;
		float moveDis = mBulletSpeed * Time.fixedDeltaTime;
		if(Physics.Raycast(mHook.transform.position,mShootDir,out hitInfo))
		{
			if(hitInfo.distance < moveDis)
			{
				if(hitInfo.transform.gameObject.layer == Pathea.Layer.VFVoxelTerrain)
				{
					mTargetPos = hitInfo.point;
					mState = HookGunState.Climbing;
					List<string> anim = new List<string>();
					anim.Add("GrapplingStart");
					mSkillRunner.ApplyAnim(anim);
					mClimpOffset = 1.5f;
					if(null != mSkillRunner.GetComponent<Rigidbody>())
						mSkillRunner.GetComponent<Rigidbody>().isKinematic = true;
					return;
				}
				else if(!hitInfo.collider.isTrigger)
				{
					mState = HookGunState.Back;
					return;
				}
			}
		}
		mTargetPos = mHook.transform.position += moveDis * mShootDir;
		if(Vector3.Distance(mHook.transform.position, mGunMuzzle[mCurrentIndex].End.position) > mRange)
			mState = HookGunState.Back;
		UpdateLinePos();
	}
	
	void UpdateBulletBackPos()
	{
		RaycastHit hitInfo;
		mHook.transform.position = mTargetPos;
		float moveDis = mBulletSpeed * Time.fixedDeltaTime;
		mShootDir = (mGunMuzzle[mCurrentIndex].End.position - mHook.transform.position).normalized;
		if(Physics.Raycast(mHook.transform.position,mShootDir,out hitInfo))
		{
			if(hitInfo.distance < moveDis)
			{
				mState = HookGunState.Null;
				for(int i = 0;i < mLineList.Count; i++)
					mLineList[i].localPosition = Vector3.zero;
				mHook.transform.localPosition = Vector3.zero;
				Invoke("EndClimb", 0.2f);				
				return;
			}
		}
//		mHook.transform.position += moveDis * mShootDir;
		mTargetPos = mHook.transform.position += moveDis * mShootDir;
		UpdateLinePos();
	}
	
	void UpdateLinePos()
	{
		Vector3 Dir = mHook.transform.position - mGunMuzzle[mCurrentIndex].End.position;
		for(int i = 0;i < mLineList.Count; i++)
		{
			Vector3 offsetPos = Dir * i / (mLineList.Count - 1);
			mLineList[i].position = mGunMuzzle[mCurrentIndex].End.position + offsetPos 
				+ (1f - Mathf.Abs(i * 2f /mLineList.Count - 1f) * Mathf.Abs(i * 2f /mLineList.Count - 1f)) * Dir.magnitude * mLineDownF;
			mLinePosList[i] = mLineList[i].position;
		}
	}
	
	void UpdateUserPos()
	{
		Vector3 StartPos = mLinePosList[mCurrentMoveIndex];
		Vector3 EndPos = mLinePosList[mCurrentMoveIndex + 1];
		Vector3 Dir = EndPos - StartPos;
		mOffsetPos = mSkillRunner.transform.position - mGunMuzzle[mCurrentIndex].End.position;
		float MoveDis = mPushSpeed * Time.fixedDeltaTime;
		mCurrentMoveDis += MoveDis;
		PhysicsCharacterMotor pcM = mSkillRunner.GetComponent<PhysicsCharacterMotor>();
		if(null != pcM)
		{
			if(!pcM.GetComponent<Rigidbody>().isKinematic)
				pcM.GetComponent<Rigidbody>().velocity = Vector3.zero;
			mClimpOffset -= Time.fixedDeltaTime;
			if(mClimpOffset< 0f && pcM.grounded)
			{
				mSkillRunner.transform.position = EndPos;
				mState = HookGunState.Null;
				for(int i = 0;i < mLineList.Count; i++)
					mLineList[i].localPosition = Vector3.zero;
				mHook.transform.localPosition = Vector3.zero;
				Invoke("EndClimb", 0.2f);
				
				if(Vector3.Distance(EndPos, mHook.position) < 1f)
				{
					Ray ray = new Ray(mHook.position + 0.5f * Vector3.up,mSkillRunner.transform.forward);
					if(!Physics.Raycast(ray,0.5f))
						mSkillRunner.transform.position = mHook.position + 0.5f * (Vector3.up + mSkillRunner.transform.forward);
				}
				else
				{
					Ray ray = new Ray(mSkillRunner.transform.position + 0.5f * Vector3.up,mSkillRunner.transform.forward);
					if(!Physics.Raycast(ray,0.5f))
						mSkillRunner.transform.position = mSkillRunner.transform.position + 0.5f * (Vector3.up + mSkillRunner.transform.forward);
				}
				return;
			}
		}
		if(mCurrentMoveDis > Dir.magnitude)
		{
			if(mCurrentMoveIndex == mLinePosList.Count - 2)// endMove
			{
				mSkillRunner.transform.position = EndPos;
				mState = HookGunState.Null;
				for(int i = 0;i < mLineList.Count; i++)
					mLineList[i].localPosition = Vector3.zero;
				mHook.transform.localPosition = Vector3.zero;
				
				Invoke("EndClimb", 0.2f);
				
				Ray ray = new Ray(mHook.position + 0.5f * Vector3.up,mSkillRunner.transform.forward);
				if(!Physics.Raycast(ray,0.5f))
					mSkillRunner.transform.position = mHook.position + 0.5f * (Vector3.up + mSkillRunner.transform.forward);
				return;
			}
			else
			{
				mCurrentMoveDis -= Dir.magnitude;
				mCurrentMoveIndex++;
				StartPos = mLinePosList[mCurrentMoveIndex];
				EndPos = mLinePosList[mCurrentMoveIndex + 1];
				Dir = EndPos - StartPos;
			}
		}
		
		mSkillRunner.transform.position = StartPos + Dir.normalized * mCurrentMoveDis + mOffsetPos;
		
		
		for(int i = 0;i < mLineList.Count; i++)
		{
			if(i < mCurrentMoveIndex + 1)
				mLineList[i].position = mGunMuzzle[mCurrentIndex].End.position;
			else
				mLineList[i].position = mLinePosList[i];
		}
	}
	
	public override bool CostSkill (ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
        if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
        {
            return false;
        }
		if(mState == HookGunState.Null && buttonDown)
		{
			Vector3 shootDir = target.GetPosition() - mGunMuzzle[mCurrentIndex].End.position;
			shootDir.Normalize();
//			if(shootDir.y > 0.17f) // up 10 angle
			{
				mState = HookGunState.Shooting;
				mTargetPos = mHook.transform.position;
				mCurrentMoveIndex = 0;
				mShootDir = (target.GetPosition() - mGunMuzzle[mCurrentIndex].End.position).normalized;
				mOffsetPos = mSkillRunner.transform.position - mGunMuzzle[mCurrentIndex].End.position;
				List<string> animList = new List<string>();
				animList.Add("HoldOnFire");
				mSkillRunner.ApplyAnim(animList);
				mClimpOffset = 1.5f;
				mHuman.ApplyDurabilityReduce(0);
				AudioManager.instance.Create(transform.position, mFireSound);
				return true;
			}
		}
		return false;
	}
}
