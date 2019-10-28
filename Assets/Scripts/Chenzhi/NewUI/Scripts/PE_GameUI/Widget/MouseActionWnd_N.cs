using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseActionWnd_N : MonoBehaviour 
{
	static MouseActionWnd_N mInstance;
	public static MouseActionWnd_N Instance{ get { return mInstance; } }
	
	public Transform 		mWnd;
	public UILabel 			mText;
	public UISlicedSprite	mBgSpr;
	
	public UISprite			mSprPrefab;
	
	List<UISprite>			mSprList;
	
	const float	BorderLength = 12f;
	
	string mCurrentContent = "";
	
	void Awake()
	{
		mInstance = this;
		mSprList = new List<UISprite>();
	}
	
	// Update is called once per frame
	void Update () 
	{
        mWnd.localPosition = PeCamera.mousePos + 10 * Vector3.forward;
	}
	
	public void SetText(string content)
	{
		if(!string.IsNullOrEmpty(content) && mCurrentContent != content)
		{
			Clear();
			mCurrentContent = content;
			mText.enabled = true;
			mBgSpr.enabled = true;
			float MaxLength = 0;
			string[] strList = content.Split('\n');
			for(int i = 0; i < strList.Length; i++)
			{
				if(strList[i].Contains("$"))
				{
					string iconName = strList[i].Split('$')[1];
					strList[i] = strList[i].Replace("$" + iconName + "$", "");
					float length = mText.font.CalculatePrintedSize(strList[i], true, UIFont.SymbolStyle.None).x * mText.font.size;
					UISprite spr = Instantiate(mSprPrefab) as UISprite;
					spr.transform.parent = mWnd;
					spr.transform.localScale = Vector3.zero;
					spr.transform.localPosition = new Vector3(BorderLength + length + 2, -BorderLength - i * mText.font.size - 2);
					spr.spriteName = iconName;
					spr.MakePixelPerfect();
					length += spr.transform.localScale.x + 2;
					mSprList.Add(spr);
					if(length > MaxLength)
						MaxLength = length;
				}
				else
				{
					float length = mText.font.CalculatePrintedSize(strList[i], true, UIFont.SymbolStyle.None).x * mText.font.size;
					if(length > MaxLength)
						MaxLength = length;
				}
			}
			
			//Reset Bg size
			mBgSpr.transform.localScale = new Vector3(MaxLength + 2 * BorderLength, strList.Length * mText.font.size + 2 * BorderLength, 1);
			string finalStr = strList[0];
			for(int i = 1; i < strList.Length; i++)
				finalStr += "\n" + strList[i];
			mText.text = finalStr;
		}
	}
	
	public void Clear()
	{
		foreach(UISprite apr in mSprList)
			Destroy(apr.gameObject);
		mSprList.Clear();
		mText.enabled = false;
		mBgSpr.enabled = false;
		mCurrentContent = "";
	}
}
