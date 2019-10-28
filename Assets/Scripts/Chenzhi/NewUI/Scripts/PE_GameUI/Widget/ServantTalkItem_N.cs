using UnityEngine;
using System.Collections;

public class ServantTalkItem_N : MonoBehaviour
{
	public UILabel	mName;
	public UILabel	mContent;

	public UISlicedSprite mBG;
	
	float  mTimeCounter = 9f;
	float  mFadeTimeCounter = 0;
	
	public void InitItem(string name, string content)
	{
		mName.text = name;
		mContent.text = content;
		if(null != mBG)
		{
			float height = mContent.font.CalculatePrintedSize(mContent.processedText, true, UIFont.SymbolStyle.None).y * mContent.font.size;
			if(height + 30 > 88)
			{
				Vector3 bgScale = mBG.transform.localScale;
				bgScale.y = height + 30;
				mBG.transform.localScale = bgScale;
			}
		}
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
