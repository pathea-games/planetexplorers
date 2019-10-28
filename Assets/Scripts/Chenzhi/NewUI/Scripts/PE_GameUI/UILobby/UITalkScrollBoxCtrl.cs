using UnityEngine;
using System.Collections;
using System;

public class UITalkScrollBoxCtrl : MonoBehaviour
{

	public UISlicedSprite mWndBg;

	public float mScrollBoxWidth; 
	public float mScrollBoxHeight; 

	UIScrollBox mSrollboxCtrl;
	// Use this for initialization
	void Start ()
	{
		mSrollboxCtrl = this.gameObject.GetComponent<UIScrollBox>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(mSrollboxCtrl != null)
			UpdateScollBoxState();
	}

	void UpdateScollBoxState()
	{
		if( mScrollBoxWidth != mWndBg.gameObject.transform.localScale.x - 70)
		{
			mScrollBoxWidth = mWndBg.gameObject.transform.localScale.x - 70;
			mSrollboxCtrl.m_Width = Convert.ToInt32(mScrollBoxWidth);

		}

		if( mScrollBoxHeight != mWndBg.gameObject.transform.localScale.y - 10)
		{
			mScrollBoxHeight = mWndBg.gameObject.transform.localScale.y - 10;
			Vector3 Pos = gameObject.transform.localPosition;
			gameObject.transform.localPosition = new Vector3(Pos.x,mWndBg.gameObject.transform.localScale.y,Pos.z);

			mSrollboxCtrl.m_Height =  Convert.ToInt32(mScrollBoxHeight);
		}
	}
}
