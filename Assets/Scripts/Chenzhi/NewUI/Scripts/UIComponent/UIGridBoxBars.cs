using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIGridBoxBars : MonoBehaviour 
{
	public int mMaxPageCount = 3;
	[SerializeField] UIGrid mGrid; 
	[SerializeField] UIGrid mGrid_2; 
	[SerializeField] UILabel mLbPageText;


	public delegate void PageIndexChange(int _PageIndex);
	public event PageIndexChange  e_PageIndexChange= null;

	private List<GameObject> mItems = new List<GameObject>();
	public List<GameObject> Items {get {return mItems;}}

	public int PageIndex  // 从1 开始
	{
		get;
		private set;
	}
	public int ItemCount   
	{         
		get;
		private set;
	}


	
	public void Init(GameObject itemPrefab ,int itemCount)
	{
		ItemCount = itemCount;
		for (int i=0 ;i <itemCount ; i++)
		{
			GameObject obj = GameObject.Instantiate(itemPrefab) as GameObject;
			if (mGrid_2 != null && i >= itemCount/2)
				obj.transform.parent = mGrid_2.gameObject.transform;
			else 
				obj.transform.parent = mGrid.gameObject.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.SetActive(true);
			obj.layer = Pathea.Layer.GUI;
			mItems.Add(obj);
		}
		PageIndex = 1;
	}

	public void Reposition()
	{
		mGrid.repositionNow = true;
		if (mGrid_2 != null)
			mGrid_2.repositionNow = true;
	}

	public void BtnPageUpOnClick()
	{
		PageIndex = (PageIndex > 1) ? PageIndex - 1 : mMaxPageCount;
		mLbPageText.text = PageIndex.ToString();
		if (e_PageIndexChange != null )
			e_PageIndexChange(PageIndex);
	}


	public void BtnPageDnOnClick()
	{
		PageIndex = (PageIndex < mMaxPageCount) ? PageIndex + 1 : 1;
		mLbPageText.text = PageIndex.ToString();
		if (e_PageIndexChange != null)
			e_PageIndexChange(PageIndex);
	}
}
