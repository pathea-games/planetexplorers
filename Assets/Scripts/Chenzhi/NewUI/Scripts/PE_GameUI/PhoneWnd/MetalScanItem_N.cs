using UnityEngine;
using System.Collections;

public class MetalScanItem_N : MonoBehaviour 
{
	public UICheckbox	mCheckBox;
	public UISprite		mBgSpr;
	public UISprite		mSelecSpr;
	public Color		mColor;
	public byte			mType;
    public ShowToolTipItem_N mToolTip_N;

	public delegate void OnClickEvent(object sender);
	public event OnClickEvent e_OnClick = null;

	
	public void SetItem(string name, Color col, byte type,int tipID)
	{
		mType = type;
		mColor = col;
		mBgSpr.spriteName = name;
		mSelecSpr.spriteName = name;
        mToolTip_N.mStrID = tipID;
    }
	
	void OnClick()
	{
		if(0 != mType && e_OnClick != null)
			e_OnClick(this);
	}
}
