using UnityEngine;
using System.Collections;

public class Parachute : Equipment 
{
	public float		mTurnOnSpeed = 10f;
	public GameObject	mGlideObj;
	
	public int			mSoundID;

	const float Gravity = 10f;
	bool 		mGlideActive = false;
	Animator	mAnimator;

	protected PhysicsCharacterMotor	mPCM;

	public override void InitEquipment (SkillAsset.SkillRunner runner, ItemAsset.ItemObject item)
	{
		base.InitEquipment (runner, item);
		if(null != mGlideObj)
			mAnimator = mGlideObj.GetComponent<Animator>();
		mPCM = runner.GetComponent<PhysicsCharacterMotor>();
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		if(null != mPCM)
		{
			if(mGlideActive)
			{
				if(mPCM.GetComponent<Rigidbody>().velocity.y < -mTurnOnSpeed)
					mPCM.GetComponent<Rigidbody>().AddForce(1.5f * Gravity * Vector3.up,ForceMode.Acceleration);
			}
		}
	}

	public void LateUpdate()
	{
		if(mMainPlayerEquipment && null != mPCM)
		{
			if(mGlideActive)
			{
				if(mPCM.GetComponent<Rigidbody>().isKinematic || mPCM.GetComponent<Rigidbody>().velocity.y > -1)
				{
					EndParachute();

                    //if (GameConfig.IsMultiMode && mMainPlayerEquipment)
                    //    PlayerFactory.mMainPlayer.SyncParachuteStatus(false);
				}

			}
			else if(mPCM.GetComponent<Rigidbody>().velocity.y < -mTurnOnSpeed)
			{
				StartParachute();

                //if (GameConfig.IsMultiMode && mMainPlayerEquipment)
                //    PlayerFactory.mMainPlayer.SyncParachuteStatus(true);
			}
		}
	}

	public void StartParachute()
	{
		mGlideActive = true;
		if(null != mAnimator)
		{
			mAnimator.SetBool("Open", true);
			mAnimator.SetBool("Close", false);
			AudioManager.instance.Create(transform.position, mSoundID);
		}
	}

	public void EndParachute()
	{
		mGlideActive = false;
		if(null != mAnimator)
		{
			mAnimator.SetBool("Open", false);
			mAnimator.SetBool("Close", true);
		}
	}
}
