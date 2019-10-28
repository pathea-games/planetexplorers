using UnityEngine;
using System.Collections;

public class ShowToolTipItem_N : MonoBehaviour
{
	public string 	mTipContent;
	public int		mStrID;
	void OnTooltip (bool show)
	{
		if(0 != mStrID)
		{
			UITooltip.ShowText(PELocalization.GetString(mStrID));
			return;
		}
		else if(mTipContent != "")
		{
			UITooltip.ShowText(mTipContent);
			return;
		}
		UITooltip.ShowText(null);
	}
}
