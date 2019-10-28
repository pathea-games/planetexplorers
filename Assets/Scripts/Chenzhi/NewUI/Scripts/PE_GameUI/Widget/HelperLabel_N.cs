using UnityEngine;
using System.Collections;

public class HelperLabel_N : MonoBehaviour 
{
	static HelperLabel_N mInstance;
	public static HelperLabel_N Instance{get{return mInstance;}}
	
	UILabel	mLabel;
	
	//float	mShowTime = 10f;
	//float	mElapseTime = 0f;
	
	void Awake()
	{
		mInstance = this;
		mLabel = GetComponent<UILabel>();
	}

	void Start ()
	{
		mLabel.text = PELocalization.GetString(8000106);
	}
	
	public void SetText(string text)
	{
		mLabel.text = text;
		//mElapseTime = 0;
		mLabel.color = Color.white;
	}
	
//	void Update()
//	{
//		if(mElapseTime < mShowTime)
//		{
//			mElapseTime += Time.deltaTime;
//		}
//		else if(mElapseTime < mShowTime * 1.5f)
//		{
//			mElapseTime += Time.deltaTime;
//			mLabel.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - 2f * (mElapseTime - mShowTime)/mShowTime));
//		}
//	}
}
