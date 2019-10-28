using UnityEngine;
using System.Collections;
using ItemAsset;

public class GlobalShowItem_N : MonoBehaviour
{
	public UILabel	mItemName;
	public Grid_N	mItem;
	
	float 	mUpspeed = 80;
	float	mLifeTime = 3;
	float	mLifElapse = 0;
	
	Vector3 mPos;
	
	void Update ()
	{
		mPos += mUpspeed * Vector3.up*Time.deltaTime;
		transform.localPosition = new Vector3(Mathf.Round(mPos.x),Mathf.Round(mPos.y),Mathf.Round(mPos.z));
		mLifElapse += Time.deltaTime;
		GetComponent<UIPanelAlpha>().alpha = 1 - mLifElapse/mLifeTime;
		if(mLifElapse > mLifeTime)
			Destroy(gameObject);
	}
	
	public void InitItem(ItemSample itemGrid)
	{
		if(itemGrid == null)
			return;
		mItem.SetItem(itemGrid);
		if(itemGrid.protoData.maxStackNum > 1)
			mItemName.text = itemGrid.protoData.GetName() + "+" + itemGrid.GetCount();
		else
			mItemName.text = itemGrid.protoData.GetName();
		mPos = transform.localPosition;
	}
	
	public void InitItem(string str)
	{
		if(str == "")
			return;
		mItem.SetItem(null);
		mItem.gameObject.SetActive(false);
		mItemName.pivot = UIWidget.Pivot.Center;
		mItemName.text = str;
		mPos = transform.localPosition;
	}
}
