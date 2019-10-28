using CustomData;
using System;
using System.Collections.Generic;
using uLobby;
using UnityEngine;

public class GameClientLobby : LobbyInterface
{
    public bool actionOk = true;

    public static RoleInfo role { get; private set; }

    public List<RoleInfo> myRoles = new List<RoleInfo>();
    public List<RoleInfo> myRolesExisted = new List<RoleInfo>();
    public List<RoleInfo> myRolesDeleted = new List<RoleInfo>();     

    public List<RoleInfoProxy> m_RolesInLobby = new List<RoleInfoProxy>();
	public List<ServerInfo> m_ServerList = new List<ServerInfo>();

	private static GameClientLobby self;
	public static GameClientLobby Self { get { return self; } }

	public static event Action OnLobbyDisconnectedEvent;

	void Awake()
	{
		self = this;
	}

    void Start()
    {
        Lobby.AddListener(this);
        Lobby.OnConnected += Lobby_OnConnected;
        Lobby.OnDisconnected += Lobby_OnDisconnected;
        Lobby.OnFailedToConnect += Lobby_OnFailedToConnect;

		RegisterMsgHandlers();

#if UNITY_EDITOR
		uLink.Network.config.timeoutDelay = 1800;
#else
        uLink.Network.config.timeoutDelay = 180;
#endif

		OnLobbyDisconnectedEvent += OnLobbyDisconnected;
		MyServerManager.OnServerHostEvent += OnServerHost;
	}

	void OnDestroy()
	{
		StopAllCoroutines();

		Lobby.OnConnected -= Lobby_OnConnected;
		Lobby.OnDisconnected -= Lobby_OnDisconnected;
		Lobby.OnFailedToConnect -= Lobby_OnFailedToConnect;
		OnLobbyDisconnectedEvent -= OnLobbyDisconnected;
		MyServerManager.OnServerHostEvent -= OnServerHost;
	}

    void Lobby_OnFailedToConnect(LobbyConnectionError error)
    {
		MessageBox_N.CancelMask(MsgInfoType.LobbyLoginMask);

		switch (error)
		{
            case LobbyConnectionError.RSAPublicKeyMismatch:
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000034));
                break;
            //lz-2016.10.31 ∑≠“Î¡¨Ω”¥ÌŒÛ¬Î
            case LobbyConnectionError.CreateSocketOrThreadFailure:
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000860));
                break;
            case LobbyConnectionError.ConnectionTimeout:
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000861));
                break;
            case LobbyConnectionError.ConnectionFailed:
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000862));
                break;
            default:
                MessageBox_N.ShowOkBox(error.ToString());
                break;
        }

        //LoginGui_N.Instance.HideMask();
        LogManager.Error("Failed to connect lobby server:", error);
    }

    void Lobby_OnDisconnected()
    {
		MessageBox_N.CancelMask(MsgInfoType.LobbyLoginMask);

		if (LogFilter.logDebug) Debug.Log("Disconnected from lobby server");

		if (null != OnLobbyDisconnectedEvent)
			OnLobbyDisconnectedEvent();

		Cursor.lockState = Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
		Cursor.visible = true;
		PeCamera.SetVar("ForceShowCursor", true);

		NetworkInterface.Disconnect();
		if(Pathea.PeFlowMgr.Instance != null && Pathea.PeFlowMgr.Instance.curScene != Pathea.PeFlowMgr.EPeScene.MainMenuScene)
			Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.MainMenuScene,false);
    }

	static void OnLobbyDisconnected()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000035),PeSceneCtrl.Instance.GotoMainMenuScene);
	}

	void OnServerHost()
	{
		MessageBox_N.ShowMaskBox(MsgInfoType.NoticeOnly, PELocalization.GetString(8000756), 20);
	}

	void Lobby_OnConnected()
    {
		if (LogFilter.logDebug) Debug.Log("Connected to lobby server");
    }

	public static void ConnectToLobby()
	{
		if (Lobby.connectionStatus == LobbyConnectionStatus.Disconnected)
		{
			Lobby.publicKey = new PublicKey(@"<RSAKeyValue><Modulus>njj4wBQW593lzN1CMkd/soo6yiz4Q1pOzGjGqq0GwR1S/PKdKiNxdyWFING69FGf6V6Almf5oVHXmoN0LNfIDUOw1Lfsq3hORXkUuz2L2dMp98RkkfKprQ+S4w0Y/HRVmp9kEO2PxSqxTwoCcaq/g65XcXs1lhGF26PQRv//pAk=</Modulus><Exponent>EQ==</Exponent></RSAKeyValue>");
			Lobby.ConnectAsClient(ClientConfig.LobbyIP, ClientConfig.LobbyPort);
		}
	}

	public static void Disconnect()
	{
		OnLobbyDisconnectedEvent -= OnLobbyDisconnected;
		Lobby.Disconnect();
	}

	void RegisterMsgHandlers()
	{
		MessageHandlers handlers = new MessageHandlers();
		handlers.RegisterHandler(ELobbyMsgType.SendMsg, RPC_L2C_SendMsg);
		handlers.RegisterHandler(ELobbyMsgType.RoleInfoAllGot, RPC_L2C_RoleInfoAllGot);
		handlers.RegisterHandler(ELobbyMsgType.RoleInfoNone, RPC_L2C_RoleInfoNone);
		handlers.RegisterHandler(ELobbyMsgType.SteamLogin, RPC_L2C_SteamLogin);
		handlers.RegisterHandler(ELobbyMsgType.RolesInLobby, RPC_L2C_RolesInLobby);
		handlers.RegisterHandler(ELobbyMsgType.RepeatLogin, RPC_L2C_RepeatLogin);
		handlers.RegisterHandler(ELobbyMsgType.AccountLogout, RPC_L2C_AccountLogOut);
		handlers.RegisterHandler(ELobbyMsgType.RoleLogin, RPC_L2C_RoleLoggedIn);
		handlers.RegisterHandler(ELobbyMsgType.CloseServer, RPC_L2C_CloseServer);
		handlers.RegisterHandler(ELobbyMsgType.RoleCreateSuccess, RPC_L2C_CreateRoleSuccess);
		handlers.RegisterHandler(ELobbyMsgType.RoleCreateFailed, RPC_L2C_CreateRoleFailed);
		handlers.RegisterHandler(ELobbyMsgType.EnterLobbySuccess, RPC_L2C_EnterLobbySuccuss);
		handlers.RegisterHandler(ELobbyMsgType.EnterLobbyFailed, RPC_L2C_EnterLobbyFailed);
		handlers.RegisterHandler(ELobbyMsgType.DeleteRoleSuccess, RPC_L2C_TryDeleteRoleSuccess);
		handlers.RegisterHandler(ELobbyMsgType.DeleteRoleFailed, RPC_L2C_TryDeleteRoleFailed);
		handlers.RegisterHandler(ELobbyMsgType.SteamInvite, RPC_L2C_Invite);
		handlers.RegisterHandler(ELobbyMsgType.SteamInviteData, RPC_L2C_SyncInviteData);
		handlers.RegisterHandler(ELobbyMsgType.ShopData, RPC_L2C_ShopData);
		handlers.RegisterHandler(ELobbyMsgType.ShopDataAll, RPC_L2C_ShopDataAll);
		handlers.RegisterHandler(ELobbyMsgType.BuyItems, RPC_L2C_BuyItems);
		handlers.RegisterHandler(ELobbyMsgType.QueryLobbyExp, RPC_L2C_QueryLobbyExp);
		handlers.RegisterHandler(ELobbyMsgType.UploadISO, RPC_L2C_UploadIso);
		SetHandlers(handlers);
	}

	enum EAccountType
    {
        Normal = 0,
        Steam = 1,
        Max
    }

    internal static void AccountLoginSteamWorks(byte[] tokenByteArray, uint tokenLen, ulong steamId)
    {
		LobbyRPC(ELobbyMsgType.AccountLogin, tokenByteArray, tokenLen, steamId);
    }

    public void ResetActionOK(){
        actionOk = true;
    }
    public void TryCreateRole() {
        if (actionOk)
        {
			PeSceneCtrl.Instance.GotoRoleScene();
            actionOk = false;

            Invoke("ResetActionOK", 3.0f);
        }
    }

    public void TryEnterLobby(int roleId)
    {   
        if (actionOk)
        {
            LobbyRPC(ELobbyMsgType.EnterLobby, roleId);
            actionOk = false;
            Invoke("ResetActionOK", 3.0f);
        }
    }

	public void TryDeleteRole(int roleId)
	{
        if (actionOk)
        {
            LobbyRPC(ELobbyMsgType.DeleteRole, roleId);
            actionOk = false;
            Invoke("ResetActionOK", 3.0f);
        }
    }
	public void QueryLobbyExp()
	{
		LobbyRPC(ELobbyMsgType.QueryLobbyExp);
	}
    public void BackToRole() {
		PeSceneCtrl.Instance.GotoMultiRoleScene();
    }

	public void GetShopData(int index = 0,int count = 20,int tabIndex = 0)
	{
		LobbyRPC(ELobbyMsgType.ShopData, index,count,tabIndex);
	}
	public void GetShopDataAll()
	{
		LobbyRPC(ELobbyMsgType.ShopDataAll);
	}

	void RPC_L2C_SendMsg(uLink.BitStream stream, LobbyMessageInfo info)
	{
		/*EMsgType msgType = */stream.Read<EMsgType>();
		/*ulong steamId = */stream.Read<ulong>();
		string roleName = stream.Read<string>();
		string msg = stream.Read<string>();

		if (UILobbyMainWndCtrl.Instance != null && UILobbyMainWndCtrl.Instance.isShow)
			UILobbyMainWndCtrl.Instance.AddTalk(roleName, msg);
	}

    void RPC_L2C_RoleInfoAllGot(uLink.BitStream stream, LobbyMessageInfo info)
    {
        RoleInfo[] _myRoles;
        stream.TryRead<RoleInfo[]>(out _myRoles);
        myRoles = new List<RoleInfo>();
        myRolesExisted = new List<RoleInfo>();
        myRolesDeleted = new List<RoleInfo>();
		AccountItems.self.balance = stream.Read<float> ();
		AccountItems.self.ImportData(stream.Read<byte[]>());
        myRoles.AddRange(_myRoles);
        for (int m = 0; m < myRoles.Count; m++)
        {
            if (myRoles[m].deletedFlag != 1 && myRolesExisted.Count <= 3)
            {
                myRolesExisted.Add(myRoles[m]);
            }
            else
            {
                myRolesDeleted.Add(myRoles[m]);
            }
        }
        
		PeSceneCtrl.Instance.GotoMultiRoleScene();
        
#if SteamVersion
        if (TitleMenuGui_N.Instance)
            TitleMenuGui_N.Instance.Hide();
#else
//		if(LoginGui_N.Instance)
//			LoginGui_N.Instance.HideWindow();
#endif

        LogManager.Info("RoleInfoAllGot");
    }


    //to do-- need to be finished
    void RPC_L2C_RoleInfoNone(uLink.BitStream stream, LobbyMessageInfo info)
    {
        int _error;
        stream.TryRead<int>(out _error);
        if (_error == 1)
        {
            myRoles = new List<RoleInfo>(); 
            myRolesExisted = new List<RoleInfo>();
            myRolesDeleted = new List<RoleInfo>();
            Debug.Log("RoleInfoNone");
			PeSceneCtrl.Instance.GotoMultiRoleScene();
			AccountItems.self.balance = stream.Read<float> ();
			AccountItems.self.ImportData(stream.Read<byte[]>());
        }
        else {
            //to do--throw the error
            Debug.LogError("DataBase query failed!!!");
        }
    }

	void RPC_L2C_SteamLogin(uLink.BitStream stream, LobbyMessageInfo info)
	{
		MessageBox_N.CancelMask(MsgInfoType.LobbyLoginMask);
		Debug.Log("Steam validate ticket.");
	}

	void RPC_L2C_RolesInLobby(uLink.BitStream stream, LobbyMessageInfo info)
	{
        RoleInfoProxy[] roles = stream.Read<RoleInfoProxy[]>();

		m_RolesInLobby.Clear();
		m_RolesInLobby.AddRange(roles);
		Debug.Log("Roles In Lobby. Num:" + roles.Length);
	}

	void RPC_L2C_RepeatLogin(uLink.BitStream stream, LobbyMessageInfo info)
	{
		Disconnect();
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000041));

//		if (null != LoginGui_N.Instance)
//			LoginGui_N.Instance.HideMask();
	}

	void RPC_L2C_AccountLogOut(uLink.BitStream stream, LobbyMessageInfo info)
	{
		ulong steamId = stream.Read<ulong>();

		m_RolesInLobby.RemoveAll(iter => iter.steamId == steamId);
	}

	void RPC_L2C_RoleLoggedIn(uLink.BitStream stream, LobbyMessageInfo info)
	{
        RoleInfoProxy role = stream.Read<RoleInfoProxy>();

		m_RolesInLobby.RemoveAll(iter => iter.steamId == role.steamId);
		m_RolesInLobby.Add(role);
	}

	void RPC_L2C_CloseServer(uLink.BitStream stream, LobbyMessageInfo info)
	{
		string msg = stream.Read<string>();

		MessageBox_N.CancelMask(MsgInfoType.ServerDeleteMask);
		MessageBox_N.ShowOkBox(msg);
	}

    void RPC_L2C_CreateRoleSuccess(uLink.BitStream stream, LobbyMessageInfo info)
    {

        RoleInfo _newRole;
        stream.TryRead<RoleInfo>(out _newRole);
        myRoles.Add(_newRole);
        myRolesExisted.Add(_newRole);

        MLPlayerInfo.Instance.SetSelectedRole(_newRole.name);
        //for test
        //Debug.Log(myRoles.Count);
        //for (int i = 0; i < myRoles.Count; i++)
        //{
        //    Debug.Log(myRoles[i].name);
        //}

		PeSceneCtrl.Instance.GotoMultiRoleScene();
        Debug.Log("Create Role Success");
    }

    void RPC_L2C_CreateRoleFailed(uLink.BitStream stream, LobbyMessageInfo info)
    {
        int _error;
        stream.TryRead<int>(out _error);
        if (_error == 1)//name used
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(ErrorMessage.NAME_HAS_EXISTED));
        }
        else if (_error == 2)//something must be wrong
        {
            RoleInfo[] _myRoles;
            stream.TryRead<RoleInfo[]>(out _myRoles);
            myRoles = new List<RoleInfo>();
            myRolesExisted = new List<RoleInfo>();
            myRolesDeleted = new List<RoleInfo>();
            myRoles.AddRange(_myRoles);
            for (int m = 0; m < myRoles.Count; m++)
            {
                if (myRoles[m].deletedFlag != 1 && myRolesExisted.Count <= 3)
                {
                    myRolesExisted.Add(myRoles[m]);
                }
                else
                {
                    myRolesDeleted.Add(myRoles[m]);
                }
            }
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000496), PeSceneCtrl.Instance.GotoMultiRoleScene);
        }
        Debug.Log("Create Role Failed");
    }

    void RPC_L2C_EnterLobbySuccuss(uLink.BitStream stream, LobbyMessageInfo info)
    {
		role = stream.Read<RoleInfo>();
		PeSceneCtrl.Instance.GotoLobbyScene();
        Debug.Log("Enter Lobby Succuss!");
    }

    void RPC_L2C_EnterLobbyFailed(uLink.BitStream stream, LobbyMessageInfo info)
    {
        RoleInfo[] _myRoles;
        stream.TryRead<RoleInfo[]>(out _myRoles);
        myRoles = new List<RoleInfo>();
        myRolesExisted = new List<RoleInfo>();
        myRolesDeleted = new List<RoleInfo>();
        myRoles.AddRange(_myRoles);
        for (int m = 0; m < myRoles.Count; m++)
        {
            if (myRoles[m].deletedFlag != 1 && myRolesExisted.Count <= 3)
            {
                myRolesExisted.Add(myRoles[m]);
            }
            else
            {
                myRolesDeleted.Add(myRoles[m]);
            }
        }
        MessageBox_N.ShowOkBox(PELocalization.GetString(8000496), MLPlayerInfo.Instance.UpdateScene);
        //MLPlayerInfo.Instance.UpdateScene();
        Debug.Log("Enter Lobby Failed");
    }

	void RPC_L2C_TryDeleteRoleSuccess(uLink.BitStream stream, LobbyMessageInfo info)
	{
		int roleId = stream.Read<int>();

		//to do-- delete the role and update
		RoleInfo role = myRoles.Find(it => it.roleID == roleId);
		role.deletedFlag = 1;
		myRolesExisted.Remove(role);
		myRolesDeleted.Clear();
		myRolesDeleted.Add(role);
		MLPlayerInfo.Instance.DeleteRole(roleId);
		Debug.Log("Try Delete Role Success");
	}

    void RPC_L2C_TryDeleteRoleFailed(uLink.BitStream stream, LobbyMessageInfo info)
    {
        //to do-- report error "role data error" and update
        RoleInfo[] _myRoles;
        stream.TryRead<RoleInfo[]>(out _myRoles);
        myRoles = new List<RoleInfo>();
        myRolesExisted = new List<RoleInfo>();
        myRolesDeleted = new List<RoleInfo>();
        myRoles.AddRange(_myRoles);
        for (int m = 0; m < myRoles.Count; m++)
        {
            if (myRoles[m].deletedFlag != 1 && myRolesExisted.Count <= 3)
            {
                myRolesExisted.Add(myRoles[m]);
            }
            else
            {
                myRolesDeleted.Add(myRoles[m]);
            }
        }
        MessageBox_N.ShowOkBox(PELocalization.GetString(8000496), MLPlayerInfo.Instance.UpdateScene);
        //MLPlayerInfo.Instance.UpdateScene();
        Debug.Log("Try Delete Role Failed");
    }

	void RPC_L2C_Invite(uLink.BitStream stream, LobbyMessageInfo info)
	{
		ulong inviteSteamId = stream.Read<ulong>();
		long serverID =  stream.Read<long>();
		PeSteamFriendMgr.Instance.ReciveInvite(inviteSteamId, serverID);
		//SteamFriendPrcMgr.Instance.SetInviteState (serverID, inviteUserName);

        //		if (UILobbyMainWndCtrl.Instance != null)
//		{
//			UILobbyMainWndCtrl.Instance.SetInviteServerData (serverID,inviteUserName);
//			//for test
//			//UILobbyMainWndCtrl.Instance.JoinToServerByInvite ();
//		}
		
	}

	void RPC_L2C_SyncInviteData(uLink.BitStream stream, LobbyMessageInfo info)
	{
		long [] serverUID = stream.Read<long[]> ();
		ulong[] steamIds = stream.Read<ulong[]> ();
		if(serverUID.Length > 0)
		{
			for(int i = 0; i < serverUID.Length; i++)
			{
				PeSteamFriendMgr.Instance.ReciveInvite(steamIds[i], serverUID[i]);
			}
		}
	}

	void RPC_L2C_ShopData(uLink.BitStream stream, LobbyMessageInfo info)
	{
		LobbyShopData [] shopdata = stream.Read<LobbyShopData[]> ();
		int startIndex = stream.Read<int> ();
		int tabIndex = stream.Read<int> ();
		LobbyShopMgr.AddRange (shopdata,startIndex,tabIndex);
	}

	void RPC_L2C_ShopDataAll(uLink.BitStream stream, LobbyMessageInfo info)
	{
		LobbyShopData [] shopdata = stream.Read<LobbyShopData[]> ();
		LobbyShopMgr.AddAll (shopdata);
	}

	void RPC_L2C_BuyItems(uLink.BitStream stream, LobbyMessageInfo info)
	{
		int itemType = stream.Read<int> ();
		int amount = stream.Read<int> ();
		float balance = stream.Read<float> ();
		if(AccountItems.self != null)
		{
			AccountItems.self.AddItems(itemType,amount);
			AccountItems.self.balance = balance;
			UIMallWnd.Instance.SetMyBalance((int)balance);
			UILobbyShopItemMgr._self.MallItemEvent (0, UIMallWnd.Instance.mCurrentTab);
		}
	}

	void RPC_L2C_QueryLobbyExp(uLink.BitStream stream, LobbyMessageInfo info)
	{
		float exp = stream.Read<float> ();
		float balance = stream.Read<float> ();
		role.lobbyExp = exp;
		AccountItems.self.balance = balance;
	}

    void RPC_L2C_UploadIso(uLink.BitStream stream, LobbyMessageInfo info)
    {
        ulong isohashcode = stream.Read<ulong>();
        bool sucessed = stream.Read<bool>();
        if(sucessed)
        {
            SteamWorkShop.SendCacheIso(isohashcode);
        }
        else
        {
            SendIsoCache iso = SteamWorkShop.GetCacheIso(isohashcode);
            if(iso != null && iso.callBackSteamUploadResult != null)
                iso.callBackSteamUploadResult(iso.id, false, isohashcode);
        }
    }
}