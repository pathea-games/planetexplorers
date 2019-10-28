using UnityEngine;
using System.Collections;

public class GridItem : MonoBehaviour
{
	//public delegate void BaseMsgEvent(object sender)
	public UILabel		mNumCount;
	public UIFilledSprite mSkillCooldown;

	public UITexture	mTexContent;
	public UISprite 	mSpContent_0;
	public UISprite 	mSpContent_1;
	public UISprite		mSpContent_2;

	public UISprite		mSpContent_Bg;
	public UISprite		mSpBg;
	public UISprite 	mNewMark;

	[HideInInspector]
	public object mData = null;


	// events
	public delegate void BaseMsgEvent(object sender);

	public BaseMsgEvent e_OnClick 		= null;
	public BaseMsgEvent e_BeginDrag 	= null;
	public BaseMsgEvent e_Drag 			= null;
	public BaseMsgEvent e_Drop 			= null;
	public BaseMsgEvent e_OnGetDrag 	= null;

	public delegate void OnToolTipEvent(bool show,object sender);
	public OnToolTipEvent e_OnToolTip 	= null;







}
