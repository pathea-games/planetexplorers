//using UnityEngine;
//using System.Collections;
//
//public class BuildBrushGrid_N : MonoBehaviour 
//{
//	public UISprite mBrushIcon;
//	public UILabel	mSizeText;
//	BuildBrushData		mBrush;
//	BuildBlockGui_N		mParentGui;
//	
//	public void SetParentGui(BuildBlockGui_N parent)
//	{
//		mParentGui = parent;
//	}
//	
//	public void SetBrush(BuildBrushData brush)
//	{
//		mBrush = brush;
//		if(null != mBrush)
//		{
//			mBrushIcon.spriteName = mBrush.mIconName;
//			mBrushIcon.MakePixelPerfect();
//			if(brush.mBrushSize > 1)
//				mSizeText.text = "X" + brush.mBrushSize;
//			else
//				mSizeText.text = "";
//		}
//		else
//			mBrushIcon.spriteName = "Null";
//	}
//	
//	void OnClick ()
//	{
//		if(null != mBrush && null != mParentGui)
//			mParentGui.OnBrushSelected(mBrush);
//	}
//}
