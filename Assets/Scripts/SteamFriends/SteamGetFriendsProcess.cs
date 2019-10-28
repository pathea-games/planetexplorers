using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
public class SteamGetFriendsProcess : ISteamFriends
{
	private static SteamGetFriendsProcess _instance;
	internal static SteamGetFriendsProcess Instance { get { return _instance; } }

	byte[] _imageData = new byte[SteamFriendsData._imageHeight * SteamFriendsData._imageWidth * 4];
	byte[] _imageTurnData = new byte[SteamFriendsData._imageHeight * SteamFriendsData._imageWidth * 4];
	int _FriendsCount = 0;

	void Awake()
	{
		_instance = this;
	}
	internal IEnumerator GetFriends()
	{
		if(_funGetFriends == null)
			yield break;
		_FriendsList.Clear ();
		AppId_t[] appList;
		appList = new AppId_t[1];
		SteamAppList.GetInstalledApps(appList, 1);
		SteamAppList.GetNumInstalledApps ();
		_FriendsCount =SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate|EFriendFlags.k_EFriendFlagIgnoredFriend);
        for(int i = 0; i < _FriendsCount; i ++)
		{
			try
	    	{
				SteamFriendsData data = new SteamFriendsData();
				//get steamid
				data._SteamID = SteamFriends.GetFriendByIndex(i,EFriendFlags.k_EFriendFlagImmediate|EFriendFlags.k_EFriendFlagIgnoredFriend);
				var fgi = new FriendGameInfo_t();
				//get playing game name
				bool ret = SteamFriends.GetFriendGamePlayed(data._SteamID, out fgi);
				if ( ret )
				{
					if(fgi.m_gameID.AppID() == SteamUtils.GetAppID())
					{
						data._PlayedGameName = "Planet Explorers";
					}
                    else
                    {
                        data._PlayedGameName = "Another game";
					}
					//SteamAppList.GetAppName(fgi.m_gameID.AppID(), out data._PlayedGameName, 256);
				}
				//get player state
				data._PlayerState = SteamFriends.GetFriendPersonaState(data._SteamID);
				//get player name
				data._PlayerName = SteamFriends.GetFriendPersonaName(data._SteamID);
				//get avatar
				int FriendAvatar = SteamFriends.GetSmallFriendAvatar(data._SteamID);		
				uint ImageWidth;
				uint ImageHeight;
				ret = SteamUtils.GetImageSize(FriendAvatar, out ImageWidth, out ImageHeight);
				
				if (ret && ImageWidth > 0 && ImageHeight > 0) 
				{			
					ret = SteamUtils.GetImageRGBA(FriendAvatar, _imageData, SteamFriendsData._imageHeight * SteamFriendsData._imageWidth * 4);
					
					for (int n = 0 ; n<_imageData.Length; n+=4)
					{
						int x =  (n/4) % SteamFriendsData._imageWidth;
						int y =  (n/4) / SteamFriendsData._imageHeight;
						int tag = (SteamFriendsData._imageWidth * ( SteamFriendsData._imageHeight - y -1) + x) * 4;

						_imageTurnData[n] = _imageData[tag];
						_imageTurnData[n+1] = _imageData[tag+1];
						_imageTurnData[n+2] = _imageData[tag+2];
						_imageTurnData[n+3] = _imageData[tag+3];
					}
		
					data._avatar.LoadRawTextureData(_imageTurnData);
					data._avatar.Apply();
				}
				_FriendsList[i] = data;
			}
			catch(Exception e)
			{
                Debug.Log("SteamGetFriendsProcess GetFriends " + e.ToString());
                _funGetFriends(null,false);				
	    	}
			yield return 0;
		}
		_funGetFriends(_FriendsList,true);		
	}

	internal SteamFriendsData GetFriendInfo(CSteamID ID)
	{
		SteamFriendsData data = new SteamFriendsData();
		data._SteamID = ID;
		var fgi = new FriendGameInfo_t();
		//get playing game name
		bool ret = SteamFriends.GetFriendGamePlayed(data._SteamID, out fgi);
		if ( ret )
		{
			if(fgi.m_gameID.AppID() == SteamUtils.GetAppID())
			{
				data._PlayedGameName = "Planet Explorers";
			}
			else
			{
				data._PlayedGameName = "Another game";
			}

			//SteamAppList.GetAppName(fgi.m_gameID.AppID(), out data._PlayedGameName, 256);
		}
		else
			data._PlayedGameName = "";
		//get player state
		data._PlayerState = SteamFriends.GetFriendPersonaState(data._SteamID);
		//get player name
		data._PlayerName = SteamFriends.GetFriendPersonaName(data._SteamID);

		//get avatar
		int FriendAvatar = SteamFriends.GetSmallFriendAvatar(data._SteamID);		
		uint ImageWidth;
		uint ImageHeight;
		ret = SteamUtils.GetImageSize(FriendAvatar, out ImageWidth, out ImageHeight);		
		if (ret && ImageWidth > 0 && ImageHeight > 0) {			
			ret = SteamUtils.GetImageRGBA(FriendAvatar, _imageData, SteamFriendsData._imageHeight * SteamFriendsData._imageWidth * 4);

			for (int n = 0 ; n<_imageData.Length; n+=4)
			{
				int x =  (n/4) % SteamFriendsData._imageWidth;
				int y =  (n/4) / SteamFriendsData._imageHeight;
				int tag = (SteamFriendsData._imageWidth * ( SteamFriendsData._imageHeight - y -1) + x) * 4;
				
				_imageTurnData[n] = _imageData[tag];
				_imageTurnData[n+1] = _imageData[tag+1];
				_imageTurnData[n+2] = _imageData[tag+2];
				_imageTurnData[n+3] = _imageData[tag+3];
			}

			data._avatar.LoadRawTextureData(_imageTurnData);
			data._avatar.Apply();
		}
		else
			return null;
		return data;
    }
}

