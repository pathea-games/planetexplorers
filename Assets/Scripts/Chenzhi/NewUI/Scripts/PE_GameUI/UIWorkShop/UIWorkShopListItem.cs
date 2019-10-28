using UnityEngine;
using System.Collections;

public class UIWorkShopListItem : MonoBehaviour 
{
	
	public event OnGuiIndexBaseCallBack ItemClick = null;

	public UILabel mLbText;
	public UISlicedSprite mMouseSprite;
	public UISlicedSprite mSelectedSprite;
	public int mIndex = -1;
	public bool isSelected =false;

	public void SetText(string _text)
	{
		mLbText.text = _text;
	}


	public void SetIndex(int index)
	{
		mIndex = index;
	}


	public void SetSelected(bool isSelected)
	{
		if(isSelected)
		{
			isSelected = true;
			mMouseSprite.enabled = false;
			mSelectedSprite.enabled = true;
		}
		else
		{
			isSelected = false;
			mSelectedSprite.enabled = false;
		}
	}



	void OnMouseOver()
	{
		if(isSelected == false)
			mMouseSprite.enabled = true;
	}
	
	void OnMoseOut()
	{
		if(isSelected == false)
			mMouseSprite.enabled = false;
	}



	private void ListItemOnClick()
	{
		if(ItemClick != null)
			ItemClick(mIndex);
	}
}
