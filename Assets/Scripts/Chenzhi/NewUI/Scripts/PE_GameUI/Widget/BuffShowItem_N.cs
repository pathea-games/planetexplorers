using UnityEngine;
using System.Collections;
using SkillAsset;

public class BuffShowItem_N : MonoBehaviour 
{
	EffSkillBuffInst mBuffIns;
	
	public void SetBuff(EffSkillBuffInst buffIns)
	{
		mBuffIns = buffIns;
		if(null != mBuffIns)
		{
			GetComponent<UISprite>().spriteName = mBuffIns.m_buff.m_iconImgPath;
			GetComponent<UISprite>().MakePixelPerfect();
		}
		else
		{
			GetComponent<UISprite>().spriteName = "Null";
		}
	}
	
	void OnTooltip (bool show)
	{
		if(mBuffIns != null)
		{
			if("0" != mBuffIns.m_buff.m_buffHint)
				ToolTipsMgr.ShowText(mBuffIns.m_buff.m_buffHint);
		}
		else
			ToolTipsMgr.ShowText(null);
	}
}
