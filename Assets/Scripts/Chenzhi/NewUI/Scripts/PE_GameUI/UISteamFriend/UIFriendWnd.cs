using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIFriendWnd : UIStaticWnd 
{
	[SerializeField] UITexture mTexIco; 
	[SerializeField] UILabel  mLbName; 
	[SerializeField] GameObject mItemPrefab;
	[SerializeField] UIGrid mGrid;
	[SerializeField] TweenPosition mTweenPos;
	[SerializeField] GameObject mOptionMenuPrefab;
	[SerializeField] GameObject mInviteBoxPrefab;
	[SerializeField] UICheckbox mTabSteamFriend;
	[SerializeField] UICheckbox mTabRoomPlayer;

	[HideInInspector]
	public UIOptionMenu mOptionMenu = null;
	[HideInInspector]
	public UIInviteMsgbox mInviteBox = null;
	private List<UIFriendItem> mItemList = new List<UIFriendItem>();

	public void InitOptionMenu(Transform centerTs , Camera uiCamera)
	{
		GameObject obj = GameObject.Instantiate(mOptionMenuPrefab) as GameObject;
		obj.transform.parent = centerTs;
		obj.transform.localPosition = new Vector3(0,0,-20);
		obj.transform.localScale = Vector3.one;
		mOptionMenu = obj.GetComponent<UIOptionMenu>();
		mOptionMenu.Init(uiCamera);
		mOptionMenu.Hide();
	}

	public void InitInviteBox(Transform leftTopTs)
	{
		GameObject obj = GameObject.Instantiate(mInviteBoxPrefab) as GameObject;
		obj.transform.parent = leftTopTs;
		obj.transform.localPosition = new Vector3(-220,-180,-100);
		obj.transform.localScale = Vector3.one;
		mInviteBox = obj.GetComponent<UIInviteMsgbox>();
		mInviteBox.Hide();
	}

	public void SetMyInfo(string name, Texture2D tex)
	{
		if (tex != null)
			mTexIco.mainTexture = tex;
		mLbName.text = name;
	}

	public void AddListItem(string name ,Texture2D tex,int index,bool isOnline)
	{
		GameObject o = GameObject.Instantiate(mItemPrefab) as GameObject;
		o.transform.parent = mGrid.gameObject.transform;
		o.transform.localScale = Vector3.one;
		o.transform.localPosition = Vector3.zero;
		o.SetActive(true);
		UIFriendItem item = o.GetComponent<UIFriendItem>();
		item.SetFriendInfo(tex,name,index,isOnline);
		item.e_ShowToolTip += ShowToopTip;
		item.e_ShowFrienMenu += ShowFriendMenu;
		mItemList.Add(item);
	}


	public void ClearList()
	{
		foreach (UIFriendItem item in mItemList)
		{
			item.gameObject.transform.parent =null;
			GameObject.Destroy(item.gameObject);
		}
		mItemList.Clear();
	}


	void SortListForOnlie()
	{
		foreach (UIFriendItem item in mItemList)
		{
			item.gameObject.name = item.mIsOnLine ? "0_Online_FriendItem" : "1_Offline_FriendItem";
		}
	}
	
	public void RepostionList()
	{
		SortListForOnlie();
		mGrid.repositionNow = true;
	}

	public delegate void ItemEvent(int index);
	public event ItemEvent e_ShowToolTip = null;
	void ShowToopTip(int index)
	{
		if (e_ShowToolTip != null)
			e_ShowToolTip(index);
	}

	public event ItemEvent e_ShowFriendMenu = null;
	void ShowFriendMenu(int index)
	{
		if (e_ShowFriendMenu != null)
			e_ShowFriendMenu(index);
	}

	public override void Show ()
	{
        this.gameObject.SetActive(true);
		isHide = false;
		mTweenPos.Play(true);
	}
	bool isHide;
	protected override void OnHide()
	{
		isHide = true;
		mTweenPos.Play(false);
	}

	void MoveFinished()
	{
        if (isHide)
        {
            this.gameObject.SetActive(false);
        }
	}

	public enum TabState
	{
		state_Friend,
		state_Palyer
	}
	[HideInInspector]
	public TabState mTabState = TabState.state_Friend;
	public event WndEvent e_TabChange = null;



	void TabFriendOnActive(bool active)
	{
		if (active)
		{
			mTabState = TabState.state_Friend;
			if (e_TabChange != null)
				e_TabChange();
		}

	}

	void TabPalyerOnActive(bool active)
	{
		if (active)
		{
			mTabState = TabState.state_Palyer;
			if (e_TabChange != null)
				e_TabChange();
		}
	}

	public void EnableTabRoomPalyer(bool enable)
	{
		mTabRoomPlayer.gameObject.SetActive(enable);
//		if (enable == false)
//		{
//			mTabSteamFriend.isChecked = true;
//			mTabState = TabState.state_Friend;
//		}
	}
}
