using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PageGridBox : MonoBehaviour 
{
	public UILabel mTextLabel;
	
	public UIGrid mGrid;
	
	[HideInInspector]
	public event OnGuiBtnClicked PageBtnLeft;
	[HideInInspector]
	public event OnGuiBtnClicked PageBtnRight;
	[HideInInspector]
	public event OnGuiBtnClicked PageBtnLeftEnd;
	[HideInInspector]
	public event OnGuiBtnClicked PageBtnRightEnd;
	
	public int mPageNum;
	protected int mPagIndex;
	protected int mMaxPagIndex;
	
	void BtnLeftEndOnClick()
	{
		if(mPagIndex < 1)
			return;
		mPagIndex = 0;
		UpdateList();

		if(PageBtnLeftEnd != null)
			PageBtnLeftEnd();
	}

	void BtnLeftOnClick()
	{
		if(mPagIndex < 1)
			return;
		mPagIndex--;
		UpdateList();

		if(PageBtnLeft != null)
			PageBtnLeft();
	}

	void BtnRightEndOnClick()
	{
		if(mPagIndex < mMaxPagIndex)
		{
			mPagIndex = mMaxPagIndex;
			UpdateList();
	
			if(PageBtnRightEnd != null)
				PageBtnRightEnd();
		}
	}

	void BtnRightOnClick()
	{
		if(mPagIndex < mMaxPagIndex)
		{
			mPagIndex ++;
			UpdateList();
	
			if(PageBtnRight != null)
				PageBtnRight();
		}
	}
		
	protected virtual void UpdateList()
	{
		UpdatePagText();
	}
	
	void UpdatePagText()
	{
		int maxCount = (mMaxPagIndex + 1).ToString().Length;
		int pageCount = (mPagIndex + 1).ToString().Length;
		string page0Str = "";
		for(int i = 0; i < maxCount - pageCount; i++)
			page0Str += "0";
		
		mTextLabel.text =  page0Str + (mPagIndex + 1).ToString() + "/" + (mMaxPagIndex + 1).ToString();
	}
}
