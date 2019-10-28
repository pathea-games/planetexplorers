using UnityEngine;
using System.Collections;

public class NpcHeadInfo_N : MonoBehaviour 
{
	public UILabel 	mNameText;
	public UISprite	mIconSpr;
	
	public void SetInfo(string name, string iconName)
	{
		mNameText.text = name;
		mNameText.MakePixelPerfect();
		
		mIconSpr.spriteName = iconName;
		mIconSpr.MakePixelPerfect();
		
		float textLength = mNameText.font.CalculatePrintedSize(name, true,UIFont.SymbolStyle.None).x * mNameText.font.size;
		float iconLength =  mIconSpr.transform.localScale.x;
		float totalLength = textLength + iconLength;
		mIconSpr.transform.localPosition = new Vector3(-totalLength/2f - 2f, 0, 0);
		mNameText.transform.localPosition = new Vector3(iconLength - totalLength/2f + 2, 0, 0);
	}
}
