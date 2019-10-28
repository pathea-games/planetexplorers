using UnityEngine;
using System.Collections;

public class ToolTipsMgr : MonoBehaviour
{
	static ToolTipsMgr 		mInstance;
	public UILabel 			mContent;
	public UISlicedSprite	mBg;
	public UISprite			mMeat;
	public UISprite			mLine;

	void Awake () 
	{
		mInstance = this;
	}

    

	void SetText(string content)
	{
		if(null != content)
		{
			mContent.text = content;
			mLine.gameObject.SetActive(true);
			if(content.Contains("[meat]"))
			{
//				mMeat.gameObject.SetActive(true);
//				string[] strlist = mContent.processedText.Split('\n');
//				for(int i=0 ; i<strlist.Length ; i++)
//				{
//					if(strlist[i].Contains("[meat]"))
//					{
//						mMeat.transform.localPosition = new Vector3(mContent.font.CalculatePrintedSize(strlist[i].Replace("[meat]","")
//							, true,UIFont.SymbolStyle.None).x * mContent.font.size + mContent.transform.localPosition.x-6
//							, -mContent.font.size * i + mContent.transform.localPosition.y,-1f);
//						break;
//					}
//				}
				mContent.text = content.Replace("[meat]","");
			}
//			else
//				mMeat.gameObject.SetActive(false);
			UITooltip.ShowText(mContent.text);
		}
		else
		{
			mMeat.gameObject.SetActive(false);
			mLine.gameObject.SetActive(false);
			UITooltip.ShowText(content);
		}
	}
	
	static public void ShowText (string content)
	{
		if(mInstance)
			mInstance.SetText(content);
	}
}
