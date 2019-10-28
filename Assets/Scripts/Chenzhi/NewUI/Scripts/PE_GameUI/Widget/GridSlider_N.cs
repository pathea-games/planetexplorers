using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridSlider_N : MonoBehaviour 
{
	public Color mStartCol;
	public Color mMidCol;
	public Color mEndCol;
	
	public Color mDisableCol;
	
	public float mLength = 10f;
		
	public int	 mNum;
	
	public UISprite mPerfab;
	
	public UISprite mIcon;
	
	public UILabel mText;
	
	List<UISprite> mSprList;
	
	float mSliderValue = 1;
	
	public void Init()
	{
		mSprList = new List<UISprite>();
		
		for(int i = 0; i < mNum; i++)
		{
			UISprite newSpr = Instantiate(mPerfab) as UISprite;
			newSpr.transform.parent = transform;
			newSpr.transform.localPosition = new Vector3(mLength * (i+1) + 4,0,0);
			newSpr.MakePixelPerfect();
			mSprList.Add(newSpr);
		}
		ResetGrid();
	}
	
	public void SetIcon(string iconName)
	{
		if(null != mIcon)
			mIcon.spriteName = iconName;
	}
	
	void ResetGrid()
	{
		for(int i = 0; i < mNum; i++)
		{
			if((0.499f + i) / mNum < mSliderValue)
			{
				if(i < mNum/2)
					mSprList[i].color = Color.Lerp(mStartCol,mMidCol,2f * i/mNum);
				else
					mSprList[i].color = Color.Lerp(mMidCol,mEndCol,2f * (1f * i/mNum - 0.5f));
			}
			else
				mSprList[i].color = mDisableCol;
		}
	}
	
	public void SetSliderValue(float SliderValue, string TrueValue = "")
	{
		if(null != mText)
			mText.text = TrueValue;
		if(mSliderValue != SliderValue)
		{
			mSliderValue = Mathf.Clamp01(SliderValue);
			ResetGrid();
		}
	}
}
