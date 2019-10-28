using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIPageGridBox : MonoBehaviour 
{
	public delegate void ReflashGridEvent(int starIndex);
	public event ReflashGridEvent e_RefalshGrid;

	public GameObject mGridItemPrefab;
	public GameObject mLeftBtns;
	public GameObject mRightBtns;
	public UILabel mPageTextLabel;
	public UIGrid mGrid;
	
	
	
	public int BtnMovePos = 12;
	public int mPagIndex = 0;
	public int mMaxPagIndex = 0;
	public int mMaxGridCount = 12;
	
	
	public List<GameObject> mItemsObject = new List<GameObject>();
	public int mItemCount = 0;
	[HideInInspector]
	public int mSelectedIndex = -1;
	[HideInInspector]
	public int mUISeletedIndex = -1;
	[HideInInspector]
	public int mStartIndex = -1;
	

	bool isInit = false;

	void Awake()
	{
		InitGrid();
	}

	public void InitGrid()
	{
		if (isInit)
			return;

		for (int i=0;i<mMaxGridCount;i++)
		{
			GameObject o = GameObject.Instantiate(mGridItemPrefab) as GameObject;
			o.transform.parent = mGrid.gameObject.transform;
			o.transform.localRotation = Quaternion.identity;
			o.transform.localScale = Vector3.one;
			o.transform.localPosition = new Vector3(0,0,0);
			mItemsObject.Add(o);  
		}
		mGrid.repositionNow = true;
		isInit = true;
	}
	
	public void ResetItemCount( int _itemCount)
	{
		mItemCount = _itemCount;
	}
	
	
	int tempResult = 0;
	public void ReflashGridCotent()
	{
		
		
		if(mItemCount > 0 && mPagIndex == 0)
			mPagIndex = 1;
		
		if(mItemCount <= mMaxGridCount)
		{
			if(mItemCount == 0)
				mMaxPagIndex = 0;
			else
				mMaxPagIndex = 1;
			
			if(mPagIndex > mMaxPagIndex)
				mPagIndex = mMaxPagIndex;
		}
		else
		{
			mMaxPagIndex = (mItemCount-1) / mMaxGridCount + 1;
			
			if(mPagIndex > mMaxPagIndex)
				mPagIndex = mMaxPagIndex;
		}
		
		int Result = 1;
		int MaxResult = 1;
		
		if(mPagIndex>9)
		{
			int temp = mPagIndex;
			while (temp >= 10)
			{
				temp =  temp/10;
				Result++;
			}
		}
		
		if(mMaxPagIndex>9)
		{
			int temp = mMaxPagIndex;
			while (temp >= 10)
			{
				temp =  temp/10;
				MaxResult++;
			}
		}
		
		if(tempResult != MaxResult-1)
		{
			Vector3 btnPos_r =  mRightBtns.transform.localPosition;
			btnPos_r.x +=  BtnMovePos* ( (MaxResult-1) - tempResult);
			mRightBtns.transform.localPosition = btnPos_r;
			
			Vector3 btnPos_l =  mLeftBtns.transform.localPosition;
			btnPos_l.x -=  BtnMovePos* ( (MaxResult-1) - tempResult);
			mLeftBtns.transform.localPosition = btnPos_l;
			
			tempResult = MaxResult-1;
		}
		
		string indexText="";
		int m = MaxResult - Result;
		while ( m > 0)
		{
			indexText += "0";
			m--;
		}
		
		mPageTextLabel.text =  indexText + mPagIndex.ToString() + "/" + mMaxPagIndex.ToString();
		
		mSelectedIndex = -1;
		mUISeletedIndex = -1;

		mStartIndex = 0;
		if (mPagIndex != 0)
			mStartIndex = mMaxGridCount * (mPagIndex-1);
		
		
		
		
		if(e_RefalshGrid != null)
			e_RefalshGrid(mStartIndex);
	}
	
	
	
	private void BtnLeftEndOnClick()
	{
		if(mPagIndex <= 1)
			return;
		mPagIndex = 1;
		ReflashGridCotent();
	}
	
	private void BtnLeftOnClick()
	{
		if(mPagIndex <= 1)
			return;
		mPagIndex --;
		ReflashGridCotent();
	}
	
	
	private void BtnRightEndOnClick()
	{
		if(mPagIndex >= mMaxPagIndex)
			return;
		mPagIndex = mMaxPagIndex;
		ReflashGridCotent();
	}
	
	private void BtnRightOnClick()
	{
		
		if(mPagIndex >= mMaxPagIndex)
			return;
		mPagIndex ++;
		ReflashGridCotent();
	}
}
