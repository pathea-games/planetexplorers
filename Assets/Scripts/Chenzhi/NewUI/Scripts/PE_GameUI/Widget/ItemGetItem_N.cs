using ItemAsset;
using ItemAsset.PackageHelper;
using UnityEngine;

public class ItemGetItem_N : MonoBehaviour
{
	public UILabel mName;
	public Grid_N  mGrid;

	public delegate void EGetItem(ItemGetItem_N item);
	public event EGetItem e_GetItem = null;

	public void SetItem(ItemSample itemSample, int index)
	{
		mGrid.SetItem(itemSample);
		mGrid.SetItemPlace(ItemPlaceType.IPT_ItemGet,index);
		if(itemSample != null)
			mName.text = itemSample.protoData.GetName() + "[30FF30] x " + itemSample.stackCount + "[-]";
		else
			mName.text = "";
	}
	
	void OnGetItem()
	{
		if ( e_GetItem != null )
			e_GetItem(this);
	}
}
