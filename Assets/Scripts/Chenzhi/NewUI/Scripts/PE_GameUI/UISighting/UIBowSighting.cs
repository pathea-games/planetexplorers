using UnityEngine;
using System.Collections;

public class UIBowSighting : UIBaseSighting 
{
	[SerializeField] int mMaxAddPos;
	[SerializeField] float mTime_t = 0.2f;

	[SerializeField] UISprite mSprTopLeft;
	[SerializeField] UISprite mSprTopRight;
	[SerializeField] UISprite mSprButtomLeft;
	[SerializeField] UISprite mSprButtomRight;
	
	Vector3 mTopLeft;
	Vector3 mTopRight;
	Vector3 mButtomLeft;
	Vector3 mButtomRight;

	// Use this for initialization
	protected override void Start ()
	{
		base.Start ();
		mTopLeft = mSprTopLeft.transform.localPosition;
		mTopRight = mSprTopRight.transform.localPosition;
		mButtomLeft = mSprButtomLeft.transform.localPosition;
		mButtomRight = mSprButtomRight.transform.localPosition;
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();
		UpdatePos(mTopLeft,mSprTopLeft.transform,new Vector2(-1,1));
		UpdatePos(mTopRight,mSprTopRight.transform,new Vector2(1,1));
		UpdatePos(mButtomLeft,mSprButtomLeft.transform,new Vector2(-1,-1));
		UpdatePos(mButtomRight,mSprButtomRight.transform,new Vector2(1,-1));
	}

	void UpdatePos(Vector3 pos, Transform ts , Vector2 dir)
	{
		Vector3 local = ts.localPosition;
		float x = pos.x + mMaxAddPos * dir.x * Value;
		float y = pos.y + mMaxAddPos * dir.y * Value;
		ts.localPosition = Vector3.Lerp(local,new Vector3(x,y,pos.z),mTime_t);
	}
}
