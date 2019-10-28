using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class ItemGridPGBWnd_N : PageGridBox 
{
	public Grid_N mPrefab;
	
	List<Grid_N> mGridList;

	public List<Grid_N> GridList{ get { return mGridList; } }
	
	List<int> mItemList;
	
	ItemPlaceType mItemPlace;
	
	public void Init()
	{
		mGridList = new List<Grid_N>();
		for(int i = 0; i < mPageNum; i++)
		{
			Grid_N newGrid = Instantiate(mPrefab) as Grid_N;
			newGrid.name = "Item" + i;
			newGrid.transform.parent = mGrid.transform;
			newGrid.transform.localScale = Vector3.one;
			newGrid.transform.localPosition = Vector3.back;
			mGridList.Add(newGrid);
		}
		mGrid.Reposition();
	}
	
	public void SetItemList(List<int> itemList, ItemPlaceType itemPlace)
	{
		mItemList = itemList;
		mItemPlace = itemPlace;
		mMaxPagIndex = (mItemList.Count - 1) / mPageNum;
		if(mPagIndex > mMaxPagIndex)
			mPagIndex = mMaxPagIndex;
		UpdateList();
	}
	
	protected override void UpdateList ()
	{
		if(null != mItemList)
		{
			base.UpdateList ();
			int startIndex = mPagIndex * mPageNum;
			
			for(int i = 0; i < mPageNum; i++)
			{
				if(i + startIndex < mItemList.Count)
				{
					mGridList[i].gameObject.SetActive(true);
					mGridList[i].SetItem(ItemMgr.Instance.Get(mItemList[i + startIndex]));
					mGridList[i].SetItemPlace(mItemPlace, i + startIndex);
				}
				else
				{
					mGridList[i].SetItem(null);
					mGridList[i].gameObject.SetActive(false);
				}
			}
		}
	}
}
