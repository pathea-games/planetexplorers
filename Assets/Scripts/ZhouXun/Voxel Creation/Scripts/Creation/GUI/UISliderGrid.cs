using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UISliderGrid : MonoBehaviour 
{
	[SerializeField] UIGrid mGrid;
	[SerializeField] UISprite mGridF_Prefab;
	[SerializeField] Color mGridF_Color;
	[SerializeField] int mItemCount = 12;
	[SerializeField] float mValue = 0; // 0-1

	public float value {get{return mValue;}set{mValue = value;}}
	private List<UISprite> mItems = new List<UISprite>();
	public List<UISprite> Items{get{return mItems;}}

	void Awake()
	{
		mItems.Clear();
		for (int i=0;i<mItemCount;i++)
		{
			UISprite sprItem = GameObject.Instantiate(mGridF_Prefab) as UISprite;
			sprItem.transform.parent = mGrid.transform;
			sprItem.transform.localPosition = new Vector3(i,0,0);
			sprItem.transform.localRotation = Quaternion.identity;
			sprItem.MakePixelPerfect();
			Color color = new Color(mGridF_Color.r,(Convert.ToSingle(i)/mItemCount) + 0.3f ,mGridF_Color.b);
			sprItem.color = color;
			sprItem.gameObject.SetActive(true);
			sprItem.enabled = false;
			sprItem.gameObject.transform.localPosition = new Vector3(mGrid.cellWidth * i,0,0);
			mItems.Add(sprItem);
		}
	}

	public void Repostion()
	{
		mGrid.repositionNow = true;
	}
	// Use this for initialization
		
	// Update is called once per frame
	void Update () 
	{
		for (int i=0;i< mItems.Count; i++)
		{
			mItems[i].enabled = mValue > (Convert.ToSingle(i) /mItemCount) ? true : false;  
		}
	}
}
