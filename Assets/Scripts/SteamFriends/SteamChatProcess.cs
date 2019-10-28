using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

public class SteamChatProcess : ISteamFriends
{
	private static SteamChatProcess _instance ;
	internal static SteamChatProcess Instance { get { return _instance; } }


	void Awake()
	{
		_instance = this;
	}

	internal static bool SendMsg(int index,string text)
	{
		if ( text.Length == 0)
			return false;
		if ( _FriendsList.ContainsKey ( index ) )
		{
			if(_FriendsList[index] != null)
			{
				return SteamFriends.ReplyToFriendMessage(_FriendsList[index]._SteamID,text);
			}
		}
		return false;
	}

}

