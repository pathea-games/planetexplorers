using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

public class SteamFriendsData
{
	public CSteamID _SteamID;
	public string _PlayerName;
	public string _PlayedGameName;
	public EPersonaState _PlayerState;
	public Texture2D _avatar;
	public const int _imageHeight = 32;
	public const int _imageWidth = 32;
	public SteamFriendsData()
	{
		_SteamID = new CSteamID ();
		_avatar = new Texture2D (_imageHeight, _imageWidth, TextureFormat.RGBA32, false, true);
	}
}
public enum INVITESTATE
{
	E_INVITE_NONE,
	E_INVITE_MAINUI,
	E_INVITE_LOBBY,
	E_INVITE_READYTOPLAY,
	E_INVITE_PLAYING,
	
}
public class ISteamFriends:MonoBehaviour
{
	public delegate void GetFriendsEventHandler(Dictionary<int, SteamFriendsData> friendsList,bool bOK );
	public delegate void RecvMsgEventHandler(int index,string text ) ;
	public delegate void PersonStateChangeHandler(int index);
	public delegate void GetFriendInfoEventHandler() ;
	public static Dictionary<int, SteamFriendsData> _FriendsList  = new Dictionary<int, SteamFriendsData>();
	internal static PersonStateChangeHandler  _funPersonStateChange = null;
	internal static RecvMsgEventHandler 			_funRecvChatMsg 		  = null;
	internal static GetFriendsEventHandler 		_funGetFriends 			  = null;
	public SteamFriendsData GetData(CSteamID steamID)
	{
		foreach( var item in _FriendsList)
		{
			if(item.Value != null && item.Value._SteamID == steamID)
				return item.Value;
		}
		return null;
	}
	public SteamFriendsData GetData(int index)
	{
		if (_FriendsList.ContainsKey (index))
			return _FriendsList [index];
		return null;
	}
	public int GetIndex(CSteamID steamID)
	{
		foreach( var item in _FriendsList)
		{
			if(item.Value != null && item.Value._SteamID == steamID)
				return item.Key;
        }
        return -1;
	}
}
public class SteamFriendPrcMgr : ISteamFriends
{

	static SteamFriendPrcMgr _instance;
	public static SteamFriendPrcMgr Instance { get	{	return _instance;	}	}
	protected static Callback<PersonaStateChange_t> m_PersonaStateChange;
	protected static Callback<GameOverlayActivated_t> m_GameOverlayActivated;
	protected static Callback<GameConnectedFriendChatMsg_t> m_GameConnectedFriendChatMsg;



	void Awake()
	{
		_instance = this;
		m_PersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
		m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		m_GameConnectedFriendChatMsg = Callback<GameConnectedFriendChatMsg_t>.Create(OnGameConnectedFriendChatMsg);
		gameObject.AddComponent<SteamGetFriendsProcess>();
		gameObject.AddComponent<SteamChatProcess>();
	}

	public void Init(GetFriendsEventHandler eventHandler,RecvMsgEventHandler handler,PersonStateChangeHandler perHandler)
	{

		_funGetFriends 				= eventHandler;
		_funRecvChatMsg   		= handler;
		_funPersonStateChange   = perHandler;

	}
	void OnGameOverlayActivated(GameOverlayActivated_t pCallback) {

    }
	void OnPersonaStateChange(PersonaStateChange_t pCallback)
	{
		SteamFriendsData data = SteamGetFriendsProcess.Instance.GetFriendInfo (new CSteamID(pCallback.m_ulSteamID));
		if(_funPersonStateChange != null && data != null)
		{
			foreach(var iter in _FriendsList)
			{
				if(iter.Value._SteamID == data._SteamID)
				{
					_FriendsList[iter.Key] = data;
					_funPersonStateChange(iter.Key);
					break;
				}
			}

		}

		//Debug.Log("[" + PersonaStateChange_t.k_iCallback + " - PersonaStateChange] - " + pCallback.m_ulSteamID + " -- " + pCallback.m_nChangeFlags);
    }


	//interface
	public void SendChat(int index,string text)
	{
		if ( _FriendsList.ContainsKey ( index ) )
		{
			if(_FriendsList[index] != null)
			{
				ChatTo(_FriendsList[index]._SteamID.m_SteamID);
				SteamChatProcess.SendMsg(index,text);
			}
		}
	}
	public void SendChat(ulong steamID,string text)
	{
		CSteamID ID = new CSteamID (steamID);
		int index = GetIndex (ID);
		ChatTo(steamID);
		SteamChatProcess.SendMsg(index,text);
	}
	public void ChatTo(ulong steamID)
	{
		CSteamID ID = new CSteamID (steamID);
		SteamFriends.ActivateGameOverlayToUser ("chat", ID);
	}

	public void FriendAdd(ulong steamID)
	{
		CSteamID ID = new CSteamID (steamID);
		SteamFriends.ActivateGameOverlayToUser ("friendadd", ID);
	}

	public void FriendRequestIgnore(ulong steamID)
	{
		CSteamID ID = new CSteamID (steamID);
		SteamFriends.ActivateGameOverlayToUser ("friendrequestignore", ID);
	}

	public void FriendRemove(ulong steamID)
	{
		CSteamID ID = new CSteamID (steamID);
		SteamFriends.ActivateGameOverlayToUser ("friendremove", ID);
	}

	public void GetFriends()
	{
		StartCoroutine (SteamGetFriendsProcess.Instance.GetFriends ());
	}

	public SteamFriendsData GetMyInfo()
	{
		return SteamGetFriendsProcess.Instance.GetFriendInfo (SteamMgr.steamId);
	}

	void OnGameConnectedFriendChatMsg(GameConnectedFriendChatMsg_t pCallback) {
		string Text;
		EChatEntryType ChatEntryType;
		/*int ret = */SteamFriends.GetFriendMessage(pCallback.m_steamIDUser, pCallback.m_iMessageID, out Text, 2048, out ChatEntryType); // Must be called from within OnGameConnectedFriendChatMsg
		if (Text.Length > 0)
		{
			if(_funRecvChatMsg != null)
			{
				int index = GetIndex(pCallback.m_steamIDUser);
				if(index > -1)
					_funRecvChatMsg(index,Text);
			}
		}
	}
    public void Invite(ulong steamID,long serverUID)
    {
  		foreach(var iter in _FriendsList)
		{
			if(iter.Value._SteamID.m_SteamID == steamID)
			{
				CSteamID steamId = SteamMgr.steamId;
				LobbyInterface.LobbyRPC(ELobbyMsgType.SteamInvite, steamId.m_SteamID, steamID, serverUID);
				if(iter.Value._PlayedGameName != "Planet Explorers")
				{
					InviteToGame(steamID,serverUID);
				}
				break;
			}
		}       
	}

	bool InviteToGame(ulong steamID,long serverUID)
	{
		//string cmd = "-inviteto " + serverUID;
		return SteamFriends.InviteUserToGame (new CSteamID (steamID), "");
	}

	//interface end
}

