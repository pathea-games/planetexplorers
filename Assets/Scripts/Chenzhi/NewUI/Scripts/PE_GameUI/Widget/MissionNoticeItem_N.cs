using UnityEngine;
using System.Collections;

public class MissionNoticeItem_N : MonoBehaviour 
{
	public UILabel	mMissionName;
	public UILabel	mMissionState;
	public UISprite mBg;
	
	float  mTimeCounter = 9f;
	float  mFadeTimeCounter = 0;
	
	/// <summary>
	/// Inits the item.Type:0 New mission. Type:1 Mission Completed. Type:2 Mission failed
	/// </summary>
	/// <param name='type'>
	public void InitItem(int type, string name)
	{
		switch(type)
		{
		case 0:
			mBg.spriteName = "MissionNotice_b";
			mMissionState.text = "[70dcff]New Mission[-]";
			break;
		case 1:
			mBg.spriteName = "MissionNotice_y";
			mMissionState.text = "[feca7e]Mission Completed[-]";
			break;
		case 2:
			mBg.spriteName = "MissionNotice_r";
			mMissionState.text = "[ff8882]Mission Failed[-]";
			break;
		}
		mMissionName.text = name;
	}
	
	void Update()
	{
		if(mTimeCounter > 0)
		{
			mTimeCounter -= Time.deltaTime;
			if(mTimeCounter <= 0)
				Hide();
		}
		if(mFadeTimeCounter > 0)
		{
			mFadeTimeCounter -= Time.deltaTime;
			if(mFadeTimeCounter <= 0)
				Destroy(this.gameObject);
		}
	}
	
	public void GoUp()
	{
		GetComponent<TweenPosition>().Play(true);
	}
	
	public void Hide()
	{
		GetComponent<TweenScale>().Play(false);
		mFadeTimeCounter = 0.5f;
	}
}
