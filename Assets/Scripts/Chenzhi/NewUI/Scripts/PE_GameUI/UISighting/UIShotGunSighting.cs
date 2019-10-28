using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIShotGunSighting : UIBaseSighting 
{
	[SerializeField] int mMaxAddWidth = 0; 
	[SerializeField] int mMaxAddHeight = 0; 
	[SerializeField] float mTime_t = 0.2f;
	[SerializeField] UISprite mSprTop;
	[SerializeField] UISprite mSprButtom;
	[SerializeField] UISprite mSprLeft;
	[SerializeField] UISprite mSprRight;
	
	
	Vector3 mTopPos;
	Vector3 mButtomPos;
	Vector3 mLeftPos;
	Vector3 mRightPos;
	
	protected override void Start()
	{
		base.Start();
		mTopPos = mSprTop.transform.localPosition;
		mButtomPos = mSprButtom.transform.localPosition;
		mLeftPos = mSprLeft.transform.localPosition;
		mRightPos = mSprRight.transform.localPosition;
	}
	
	
	protected override void Update()
	{
		base.Update();
		
		int pos_x = 0;
		int pos_y = 0;
		Vector3 sprPos;
		
		// top
		pos_y = Convert.ToInt32( mTopPos.y + mMaxAddHeight * Value );
		sprPos  = mSprTop.transform.localPosition;
		mSprTop.transform.localPosition = Vector3.Lerp( sprPos , new Vector3(mTopPos.x,pos_y,mTopPos.z), mTime_t);
		// buttom
		pos_y = Convert.ToInt32( mButtomPos.y - mMaxAddHeight * Value );
		sprPos  = mSprButtom.transform.localPosition;
		mSprButtom.transform.localPosition = Vector3.Lerp( sprPos , new Vector3(mButtomPos.x,pos_y,mButtomPos.z), mTime_t);
		// left
		pos_x = Convert.ToInt32(mLeftPos.x - mMaxAddWidth * Value);
		sprPos  = mSprLeft.transform.localPosition;
		mSprLeft.transform.localPosition = Vector3.Lerp( sprPos , new Vector3(pos_x,mLeftPos.y,mLeftPos.z), mTime_t);
		// Right
		pos_x = Convert.ToInt32(mRightPos.x + mMaxAddWidth * Value);
		sprPos  = mSprRight.transform.localPosition;
		mSprRight.transform.localPosition = Vector3.Lerp( sprPos , new Vector3(pos_x,mRightPos.y,mRightPos.z), mTime_t);
	}
}
