using System;
using System.Collections.Generic;
using uLobby;
using UnityEngine;
using BitStream = uLink.BitStream;

public enum ELobbyMsgType
{
	AccountLogin = 100,
	AccountLogout,
	RepeatLogin,
	TestLogin,

	EnterLobby,
	EnterLobbySuccess,
	EnterLobbyFailed,

	RoleLogin,
	RoleInfoAllGot,
	RoleInfoNone,
	DeleteRole,
	DeleteRoleSuccess,
	DeleteRoleFailed,
	RoleCreate,
	RoleCreateSuccess,
	RoleCreateFailed,
	RolesInLobby,

	SteamLogin,
	SteamInvite,
	SteamInviteData,

	ShopData,
	ShopDataAll,
	BuyItems,
	UseItems,
	CreateItem,

	QueryLobbyExp,
	AddLobbyExp,

	UploadISO,
	UploadISOSuccess,

	SendMsg,

	CloseServer,

	ServerRegisterDebug,
	ServerRegister,
	MasterRegister,
	MasterUpdate,

	Max
}

public class MessageHandlers
{
	public Dictionary<ELobbyMsgType, Action<BitStream, LobbyMessageInfo>> msgHandlers;

	public Action<BitStream, LobbyMessageInfo> this[ELobbyMsgType msgType] { get { return msgHandlers[msgType]; } }

	public MessageHandlers()
	{
		msgHandlers = new Dictionary<ELobbyMsgType, Action<BitStream, LobbyMessageInfo>>();
	}

	public bool CheckHandler(ELobbyMsgType msgType)
	{
		return msgHandlers.ContainsKey(msgType);
	}

	public void RegisterHandler(ELobbyMsgType msgType, Action<BitStream, LobbyMessageInfo> handler)
	{
		if (msgHandlers.ContainsKey(msgType))
		{
			if (LogFilter.logWarn) Debug.LogWarningFormat("Replace msg handler:{0}", msgType);
			msgHandlers.Remove(msgType);
		}

		if (LogFilter.logDev) Debug.LogWarningFormat("Register msg handler:{0}", msgType);
		msgHandlers.Add(msgType, handler);
	}

	public void RegisterHandlerSafe(ELobbyMsgType msgType, Action<BitStream, LobbyMessageInfo> handler)
	{
		if (msgHandlers.ContainsKey(msgType))
		{
			if (LogFilter.logError) Debug.LogWarningFormat("Duplicate msg handler:{0}", msgType);
			return;
		}

		if (LogFilter.logDev) Debug.LogWarningFormat("Register msg handler:{0}", msgType);
		msgHandlers.Add(msgType, handler);
	}
}
