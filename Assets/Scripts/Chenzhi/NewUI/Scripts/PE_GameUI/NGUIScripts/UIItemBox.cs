using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class UIItemBox : UIBaseWnd 
{
	
	public ItemGridPGBWnd_N mItemPGB;
	
	ItemBox			mOpBox;
	
	public ItemBox 	OpBox{ get { return mOpBox; } }

	void Start()
	{
		// test
		Show();
	}

	protected override void InitWindow ()
	{
		base.InitWindow ();
		mItemPGB.Init();
		foreach(Grid_N grid in mItemPGB.GridList)
			grid.onRightMouseClicked = OnRightMouseCliked;
	}
	
	public override void  Show()
	{
		base.Show();
		Invoke("Reposition",0.1f);
	}
	
	void Reposition()
	{
		mItemPGB.mGrid.Reposition();
	}
	
	public void SetItemList(ItemBox itemBox, List<int> items)
	{
		mOpBox = itemBox;
		
		mItemPGB.SetItemList(items, ItemPlaceType.IPT_ItemBox);
	}
	
	public void OnRightMouseCliked(Grid_N grid)
	{
        if (null != grid.ItemObj && null != mOpBox)
			mOpBox.SendItemToPlayer(grid.ItemObj);
	}
	
	void BtnGetOnClick()
	{
		if (null != mOpBox)
			mOpBox.SendAllItemToPlayer();
        Hide();
	}
	
	void Update()
	{
//		if(null == mOpBox || (null != GameUI.Instance.mMainPlayer && (mOpBox.transform.position - GameUI.Instance.mMainPlayer.position).sqrMagnitude > 16))
//		{
//			Hide();
//		}
	}
}
