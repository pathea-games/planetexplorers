using UnityEngine;
using System.Collections;

public class UIFriendItem : MonoBehaviour 
{
	[SerializeField] UILabel mLbName;
	[SerializeField] UITexture mTexIco;

	[HideInInspector] int mIndex = -1;

	public int Index{get{return mIndex;}}
	public bool mIsOnLine = false;
	
	public void SetFriendInfo(Texture2D texIco, string ifno,int index , bool isOnline)
	{
		mLbName.text = ifno;
		if (texIco != null)
			mTexIco.mainTexture = texIco;
		mIndex = index;
		mLbName.color = isOnline ?  Color.white :Color.gray ;
		mIsOnLine = isOnline;
	}

	public delegate void ItemEvent(int index);
	public event ItemEvent e_ShowToolTip = null;
//	void OnTooltip (bool show)
//	{
//		if(show == true && e_ShowToolTip != null)
//			e_ShowToolTip(mIndex);
//	}

	public event ItemEvent e_ShowFrienMenu = null;
	void OnMouseRightClick()
	{
		if (e_ShowFrienMenu != null)
			e_ShowFrienMenu(mIndex);
	}


}
