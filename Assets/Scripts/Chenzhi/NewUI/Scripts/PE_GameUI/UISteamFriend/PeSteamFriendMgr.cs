using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PeSteamFriendMgr : MonoBehaviour
{
	// Use this for initialization
	private static PeSteamFriendMgr mInstance = null;
	public static PeSteamFriendMgr Instance {get{return mInstance;}}
	[HideInInspector]
	public UIFriendWnd mFriendWnd = null;
	private Dictionary<int, SteamFriendsData> mFriendsData = null;
	private SteamFriendsData mMyData;
	private Dictionary<int,BaseNetwork> mBaseNetWorkList = null;
	
	public long mMyServerUID = -1;

	void Awake()
	{
		mInstance = this;
        GameClientNetwork.OnDisconnectEvent += OnDisconnectServer;
    }

    void OnDestroy()
    {
        GameClientNetwork.OnDisconnectEvent -= OnDisconnectServer;
    }

	public void Init(Transform tsTopLeftAnthor,Transform tsCenterAnthor,Camera uiCamera)
	{
		InitUIFriendWnd(tsTopLeftAnthor);
		SteamFriendPrcMgr.Instance.Init(CallBackGetFriends,CallBackRecvMsg,CallBackPersonStateChange);
		mFriendWnd.e_OnShow += ReflashFriendWnd;
		mFriendWnd.e_TabChange += ReflashFriendWnd;
		RoomGui_N.Instance.e_OnShow += ReflashFriendWnd;
		mFriendWnd.e_ShowFriendMenu += ShowMenu;
		mFriendWnd.InitOptionMenu(tsCenterAnthor,uiCamera);
		mFriendWnd.InitInviteBox(tsTopLeftAnthor);
		GetFriends();

		mBaseNetWorkList = BaseNetwork.GetBaseNetworkList();
		mMyData = SteamFriendPrcMgr.Instance.GetMyInfo();
		if (mMyData != null)
			mFriendWnd.SetMyInfo(mMyData._PlayerName,mMyData._avatar);
		else 
			mFriendWnd.SetMyInfo("",null);
	}

	void InitUIFriendWnd(Transform tsPartent)
	{
		GameObject wnd = GameObject.Instantiate(Resources.Load("Prefab/GameUI/FriendWnd")) as GameObject;
		mFriendWnd = wnd.GetComponent<UIFriendWnd>();
		mFriendWnd.gameObject.transform.parent = tsPartent;
		mFriendWnd.transform.localScale = Vector3.one;
		mFriendWnd.transform.localPosition = new Vector3(-160,-400,-10);
	}

	void GetFriends()
	{
		SteamFriendPrcMgr.Instance.GetFriends();
	}
	
	void CallBackGetFriends(Dictionary<int, SteamFriendsData> friendsList,bool bOK)
	{
		if (bOK)
		{
			mFriendsData = friendsList;
            //lz-2016.10.09 防止切场景的时候调用报错
			if (null!= mFriendWnd&&mFriendWnd.isShow)
				ReflashFriendWnd();
		}
	}

	void CallBackPersonStateChange(int index)
	{
		ReflashFriendWnd();
	}



	void CallBackRecvMsg(int index,string text ) 
	{
		return;
	}

	void ReflashFriendWnd(UIBaseWidget widget = null)
	{
		if (mFriendWnd == null || SteamFriendPrcMgr.Instance == null)
			return;

		mFriendWnd.EnableTabRoomPalyer(BaseNetwork.IsInRoom());
		mFriendWnd.ClearList();

		if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Friend)
		{
            //lz-2016.10.23 错误 #5098 空对象
            if (null != mFriendsData)
            {
                foreach (var kv in mFriendsData)
                {
                    mFriendWnd.AddListItem(GetFriendInfo(kv.Value), kv.Value._avatar, kv.Key, ((int)kv.Value._PlayerState != 0));
                }
            }
		}

		else  if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Palyer)
		{
            Dictionary<int, BaseNetwork> baseNetworkList = BaseNetwork.GetBaseNetworkList();
            //lz-2016.10.23 错误 #5098 空对象
            if (null != baseNetworkList)
            {
                foreach (var kv in baseNetworkList)
                {
                    mFriendWnd.AddListItem(GetPalyerInfo(kv.Value), null, kv.Key, true);
                }
            }
		}
		mFriendWnd.RepostionList();
	}

	string GetPalyerInfo(BaseNetwork _base)
	{
		return _base.name;
	}


	string GetFriendInfo(SteamFriendsData data)
	{
		string info= data._PlayerName ;
		int state = (int)data._PlayerState;

		if (data._PlayedGameName != null && data._PlayedGameName.Length >0)
		{
			info += "[00ff00][Playing][-]" + "[ff9900]" + data._PlayedGameName + "[-]";
		}
		else
		{
			switch (state)
			{
			case 0:
				info += "[Off-line]";
				break;
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
				info += "[00ff00][Online][-]";
				break;
			default:
				break;
			}
		}
		return info;
	}

	private int mCurrentIndex = -1;
	void ShowMenu(int index)
	{
		mFriendWnd.mOptionMenu.Clear();
		if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Friend)
		{
			mFriendWnd.mOptionMenu.AddOption("ChatTo",ChatTo);
			if (BaseNetwork.IsInRoom())
				mFriendWnd.mOptionMenu.AddOption("Invite",Invite);
			mFriendWnd.mOptionMenu.AddOption("Delete",FriendRemove);
		}
		else if (mFriendWnd.mTabState == UIFriendWnd.TabState.state_Palyer)
		{
			if (mBaseNetWorkList[index].SteamID.m_SteamID == mMyData._SteamID.m_SteamID)
				return;
			if (BaseNetwork.IsInRoom())
				mFriendWnd.mOptionMenu.AddOption("Add Friend",AddFriend);
		}
		mFriendWnd.mOptionMenu.Show();
		mCurrentIndex = index;
	}


	void ChatTo(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mFriendsData.ContainsKey(mCurrentIndex))
			SteamFriendPrcMgr.Instance.ChatTo(mFriendsData[mCurrentIndex]._SteamID.m_SteamID);
	}


	void Invite(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mFriendsData.ContainsKey(mCurrentIndex) && mMyServerUID != -1)
			SteamFriendPrcMgr.Instance.Invite(mFriendsData[mCurrentIndex]._SteamID.m_SteamID,mMyServerUID);
	}

	void FriendRemove(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mFriendsData.ContainsKey(mCurrentIndex))
			SteamFriendPrcMgr.Instance.FriendRemove(mFriendsData[mCurrentIndex]._SteamID.m_SteamID);
	}

	void AddFriend(object sender)
	{
		mFriendWnd.mOptionMenu.Hide();
		if (mBaseNetWorkList.ContainsKey(mCurrentIndex))
			SteamFriendPrcMgr.Instance.FriendAdd(mBaseNetWorkList[mCurrentIndex].SteamID.m_SteamID);
	}


	//收到邀请处理 
	internal class InviteInfo
	{
		public ulong inviteSteamId;
		public long serverUID;
		public string InviteName = null;
		public DateTime reciveTime;

		public InviteInfo(ulong steamId, long uid)
		{
			inviteSteamId = steamId;
			serverUID = uid;
			reciveTime = DateTime.Now;
		}
	}
	
	private bool mIsInvite = false;
	public bool IsInvite {get{return mIsInvite;}}

	private Dictionary<long ,InviteInfo> mInviteInfoMap = new Dictionary<long, InviteInfo>();
	private long inviteServerUID = -1;

	// 退出房间
	void OnDisconnectServer()
	{
		mMyServerUID = -1;
	}


	public void ReciveInvite(ulong inviteSteamId, long serverUID)
	{
		// 已经加入该房间
		if (serverUID == mMyServerUID)
			return;
		InviteInfo info = new InviteInfo(inviteSteamId, serverUID);
		mInviteInfoMap[serverUID] = info;
	}
	
	void Update()
	{
		if (Pathea.PeGameMgr.IsSingle || mFriendWnd == null || !UILobbyMainWndCtrl.Instance.bGetRoomInfo)
			return;
		if (Pathea.PeFlowMgr.Instance.curScene == Pathea.PeFlowMgr.EPeScene.LobbyScene) 
		{
			UpdateInviteState();
		}
		else if (Pathea.PeFlowMgr.Instance.curScene == Pathea.PeFlowMgr.EPeScene.GameScene)
		{
			UpdateInviteState();
		}
	}
	
	void UpdateInviteState()
	{
		if (!mFriendWnd.mInviteBox.isShow && mInviteInfoMap.Count > 0)
		{
			foreach (var kv in mInviteInfoMap)
			{
				info = kv.Value;
				ShowInviteMsgBox();
				return;
			}
		}
		else if (inviteServerUID != -1 && mInviteInfoMap.Count == 0)
		{
			if (Pathea.PeFlowMgr.Instance.curScene == Pathea.PeFlowMgr.EPeScene.GameScene)
				PeSceneCtrl.Instance.GotoLobbyScene();
			else if (Pathea.PeFlowMgr.Instance.curScene == Pathea.PeFlowMgr.EPeScene.LobbyScene)
				InviteJionRomm();
		}

	}

	InviteInfo info;
	void ShowInviteMsgBox()
	{
		string msg =  info.InviteName + " " + UIMsgBoxInfo.mCZ_InvitePlayer.GetString() + "(" + info.reciveTime.ToString("yyyy-MM-dd HH:mm")+")" ; 
		mFriendWnd.mInviteBox.ShowMsg(msg,JionCallBack,CancelCallBack,IgnorAllCallBack,TimeOutCallBack);
	}

	void JionCallBack()
	{
		inviteServerUID = info.serverUID;
		mInviteInfoMap.Remove(info.serverUID);
		mFriendWnd.mInviteBox.Hide();
	}

	void CancelCallBack()
	{
		mInviteInfoMap.Remove(info.serverUID);
		inviteServerUID = -1;
		mFriendWnd.mInviteBox.Hide();
	}

	void IgnorAllCallBack()
	{
		mInviteInfoMap.Clear();
		inviteServerUID = -1;
		mFriendWnd.mInviteBox.Hide();
	}

	void TimeOutCallBack()
	{
		CancelCallBack();
	}

	void InviteJionRomm()
	{
		if (UILobbyMainWndCtrl.Instance == null)
			return;

		if (UILobbyMainWndCtrl.Instance.HaveServerID(inviteServerUID))
		{
			UILobbyMainWndCtrl.Instance.SetInviteServerData (inviteServerUID);
			UILobbyMainWndCtrl.Instance.JoinToServerByInvite ();
			// 重置邀请房间ID
			inviteServerUID = -1;
		}
	}

}
