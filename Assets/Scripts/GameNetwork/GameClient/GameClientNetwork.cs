using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameClientNetwork : MonoBehaviour
{
    #region Static Variables
    private static int m_MasterId;
	private static GameClientNetwork _instance;
    private static string m_ServerName;
	#endregion

	#region Static Properties
	public static GameClientNetwork Self { get { return _instance; } }
    public static int MasterId { get { return m_MasterId; } }
    public static string ServerName { get { return m_ServerName; } }
	#endregion

	#region Static Event
	public static event Action OnDisconnectEvent;
	public static event Action OnFailedConnectEvent;
	#endregion

	#region Variables
	public List<RoomPlayerInfo> RolesInGame = new List<RoomPlayerInfo> ();
	public List<uLink.HostData> LocalServers = new List<uLink.HostData> ();
	#endregion

	#region Internal APIs
	void Awake ()
	{
		_instance = this;
	}

	void Start ()
	{
		uLink.Network.isAuthoritativeServer = true;
		uLink.Network.requireSecurityForConnecting = true;

#if UNITY_EDITOR
		uLink.Network.config.timeoutDelay = 1800;
#else
        uLink.Network.config.timeoutDelay = 180;
#endif

		uLink.Network.maxManualViewIDs = 10;
		uLink.Network.minimumAllocatableViewIDs = 5000;
		uLink.Network.minimumUsedViewIDs = 10;
		uLink.Network.sendRate = 10;

		uLink.Network.publicKey = new uLink.PublicKey (@"<RSAKeyValue><Modulus>oipCMkge/+BwcenDk3XdJMeBXwW+V6WVEtP/U7YKoFfJokNbqffWW65zUCSCUCJyalnqtKen5fbQiOtFyNwsuxdksUTiRDTSwW/gMOtyZ84YAED+W8OOmLRCWtnt/YBqxIVnKUVX2oT3aQ/pGOxmtZS7krThKyuO2RwDAWoETDM=</Modulus><Exponent>EQ==</Exponent></RSAKeyValue>");
		MyServerManager.OnServerHostEvent += OnServerHost;
	}

    void OnDestroy()
    {
		StopAllCoroutines();

        OnDisconnectEvent -= OnDisconnectFromServer;
		P2PManager.OnServerRegisteredEvent -= OnServerRegistered;
		MyServerManager.OnServerHostEvent -= OnServerHost;
	}
#endregion

#region uLink Callback APIs
	//	void uLink_OnMasterServerEvent(uLink.MasterServerEvent e)
	//	{
	//		switch (e)
	//		{
	//			case uLink.MasterServerEvent.HostListReceived:
	//				if (null != HostListReceivedEvent)
	//					HostListReceivedEvent(this, EventArgs.Empty);
	//				break;
	//
	//			default:
	//				break;
	//		}
	//	}

	//void uLink_OnPreBufferedRPCs(uLink.NetworkBufferedRPC[] bufferedArray)
	//{
	//	foreach (uLink.NetworkBufferedRPC rpc in bufferedArray)
	//	{
	//		if (rpc.isInstantiate)
	//			rpc.DontExecuteOnConnected();
	//	}
	//}

	void uLink_OnConnectedToServer()
	{
		if (LogFilter.logDebug) Debug.Log("Server connected");
		MessageBox_N.CancelMask(MsgInfoType.ServerLoginMask);
	}

	void uLink_OnFailedToConnect(uLink.NetworkConnectionError error)
	{
		MessageBox_N.CancelMask(MsgInfoType.ServerLoginMask);

		if (LogFilter.logDebug) Debug.LogErrorFormat("Failed to connect:{0}", error);

		if (null != OnFailedConnectEvent)
			OnFailedConnectEvent();

		switch (error)
		{
			case uLink.NetworkConnectionError.InvalidPassword:
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000037));
				}
				break;
			case uLink.NetworkConnectionError.RSAPublicKeyMismatch:
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000045));
				}
				break;
			case uLink.NetworkConnectionError.ConnectionBanned:
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000046));
				}
				break;
			case uLink.NetworkConnectionError.LimitedPlayers:
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000826));
				}
				break;
			case (uLink.NetworkConnectionError)200:
				{
					// Incompatible record version
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000754));
				}
				break;
			case (uLink.NetworkConnectionError)201:
				{
					// Server had a fatal error
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000755));
				}
				break;
			case (uLink.NetworkConnectionError)202:
				{
					// full custom player in custom mode
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000827));
				}
				break;
			default:
				{
					MessageBox_N.ShowYNBox(PELocalization.GetString(8000502), Connect);
				}
				break;
		}
	}
	
	void uLink_OnDisconnectedFromServer (uLink.NetworkDisconnection mode)
	{
		MessageBox_N.CancelMask (MsgInfoType.ServerLoginMask);

		if (mode == uLink.NetworkDisconnection.LostConnection)
			if (LogFilter.logDebug) Debug.Log ("Lost connection");
		else
			if (LogFilter.logDebug) Debug.Log ("Disconnected. Mode:" + mode);

		RolesInGame.Clear ();

		if (null != OnDisconnectEvent)
			OnDisconnectEvent();

        Cursor.lockState = Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
        Cursor.visible = true;
		PeCamera.SetVar("ForceShowCursor", true);
	}
#endregion

#region Internal APIs
	void OnServerRegistered(string srvName, int srvMode, int srvPort)
	{
		if (Equals(MyServerManager.LocalName, srvName))
		{
			MessageBox_N.CancelMask(MsgInfoType.NoticeOnly);
			MyServerManager.LocalPort = srvPort;
			Connect();
			P2PManager.OnServerRegisteredEvent -= OnServerRegistered;
		}
	}

	void OnServerHost()
	{
		P2PManager.OnServerRegisteredEvent += OnServerRegistered;
		StartCoroutine(OnServerHostTimeout());
	}

	IEnumerator OnServerHostTimeout()
	{
		yield return new WaitForSeconds(20);
		P2PManager.OnServerRegisteredEvent -= OnServerRegistered;
	}
#endregion

#region Static APIs
	public static void OnDisconnectFromServer()
	{
        if (Pathea.PeGameMgr.IsMultiStory)
		{
			Pathea.PeGameMgr.yirdName = null;
		}

        if (null != Pathea.PeLauncher.Instance.endLaunch)
            Pathea.PeLauncher.Instance.eventor.Subscribe(OnResponse);
        else
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000048), PeSceneCtrl.Instance.GotoLobbyScene);
    }

    static void OnResponse(object sender, PeEvent.EventArg arg)
    {
        if (!NetworkInterface.IsClient && arg is Pathea.PeLauncher.LoadFinishedArg)
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000048), PeSceneCtrl.Instance.GotoLobbyScene);
            Pathea.PeLauncher.Instance.eventor.Unsubscribe(OnResponse);
        }
    }

	public static void Connect()
	{
		MessageBox_N.ShowMaskBox(MsgInfoType.ServerLoginMask, PELocalization.GetString(8000062));
		ProxyServerRegistered proxyServer = MyServerManager.LocalHost as ProxyServerRegistered;
		if (null != proxyServer && !proxyServer.IsLan && proxyServer.UseProxy)
		{
			NetworkInterface.Connect(proxyServer.ProxyServer,
										 MyServerManager.LocalPwd,
										 GameClientLobby.role.steamId,
										 GameClientLobby.role.roleID,
										 GameClientLobby.role);
		}
		else
		{
			NetworkInterface.Connect(MyServerManager.LocalIp,
							MyServerManager.LocalPort,
							MyServerManager.LocalPwd,
							GameClientLobby.role.steamId,
							GameClientLobby.role.roleID,
							GameClientLobby.role);
		}

		OnDisconnectEvent += OnDisconnectFromServer;
	}

	public static void Disconnect()
	{
		OnDisconnectEvent -= OnDisconnectFromServer;
		NetworkInterface.Disconnect();
	}
#endregion

#region Action Callback APIs
	public static void RPC_S2C_ServerInfo (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		m_ServerName = stream.Read<string>();
		RandomMapConfig.RandSeed = stream.Read<int> ();
		Pathea.PeGameMgr.sceneMode = (Pathea.PeGameMgr.ESceneMode)stream.Read<int> ();
		if(Pathea.PeGameMgr.sceneMode!=Pathea.PeGameMgr.ESceneMode.Story
		   &&Pathea.PeGameMgr.sceneMode!=Pathea.PeGameMgr.ESceneMode.Adventure
		   &&Pathea.PeGameMgr.sceneMode!=Pathea.PeGameMgr.ESceneMode.Build
		   &&Pathea.PeGameMgr.sceneMode!=Pathea.PeGameMgr.ESceneMode.Custom)
			Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Adventure;
		Pathea.PeGameMgr.gameType = (Pathea.PeGameMgr.EGameType)Mathf.Clamp(stream.Read<int> (),0,2);
		Pathea.PeGameMgr.monsterYes = stream.Read<bool> ();
		RandomMapConfig.RandomMapID = (RandomMapType)Mathf.Clamp(stream.Read<int> (),1,8);
		RandomMapConfig.vegetationId = (RandomMapType)Mathf.Clamp(stream.Read<int> (),1,8);
		RandomMapConfig.ScenceClimate = (ClimateType)Mathf.Clamp(stream.Read<int> (),0,3);
        if (RandomMapConfig.ScenceClimate == ClimateType.CT_Random)
            RandomMapConfig.ScenceClimate = ClimateType.CT_Temperate;

		int teamNum = Mathf.Clamp(stream.Read<int> (),1,32);
		int numberTeam = Mathf.Clamp(stream.Read<int> (),1,32);
		Pathea.PeGameMgr.unlimitedRes = stream.Read<bool> ();
		RandomMapConfig.useSkillTree = stream.Read<bool>();

		RandomMapConfig.TerrainHeight = stream.Read<int> ();
		if(RandomMapConfig.TerrainHeight<=128)
			RandomMapConfig.TerrainHeight=128;
		else if(RandomMapConfig.terrainHeight<=256)
			RandomMapConfig.TerrainHeight=256;
		else if(RandomMapConfig.TerrainHeight<=512)
			RandomMapConfig.TerrainHeight = 512;
		else 
			RandomMapConfig.TerrainHeight = 512;
		RandomMapConfig.mapSize = Mathf.Clamp(stream.Read<int> (),0,4);
		RandomMapConfig.riverDensity = Mathf.Clamp(stream.Read<int> (),1,100);
		RandomMapConfig.riverWidth = Mathf.Clamp(stream.Read<int> (),1,100);
		RandomMapConfig.plainHeight =  Mathf.Clamp(stream.Read<int> (),1,100);
		RandomMapConfig.flatness =  Mathf.Clamp(stream.Read<int> (),1,100);
		RandomMapConfig.bridgeMaxHeight =  Mathf.Clamp(stream.Read<int> (),0,100);
		RandomMapConfig.allyCount = Mathf.Clamp(stream.Read<int> (),4,8);
		//2016-7-29 12:10:33
		RandomMapConfig.mirror = stream.Read<bool> ();
		RandomMapConfig.rotation =  stream.Read<int> ();
		RandomMapConfig.pickedLineIndex =  stream.Read<int> ();
		RandomMapConfig.pickedLevelIndex =  stream.Read<int> ();

		m_MasterId = stream.Read<int> ();
        Pathea.PeGameMgr.gameName = ServerName;

		Debug.Log (string.Format ("Game Mode With No Mask:{0}, Game Type:{1}, Team Num:{2}, Num Per Team:{3},Monster:{4},MapSize:{5}, MapSeed:{6}"
            , Pathea.PeGameMgr.sceneMode
            , Pathea.PeGameMgr.gameType
            , teamNum
            , numberTeam
            , Pathea.PeGameMgr.monsterYes
            , RandomMapConfig.mapSize
		    , RandomMapConfig.RandSeed));

		BattleManager.InitBattleInfo (teamNum, numberTeam);
        ForceSetting.Instance.InitRoomForces(teamNum, numberTeam);
        if (null != UILobbyMainWndCtrl.Instance)
			UILobbyMainWndCtrl.Instance.Hide ();

		if (null != RoomGui_N.Instance) {
			RoomGui_N.Instance.Show ();
		}

        if (Pathea.PeGameMgr.sceneMode != Pathea.PeGameMgr.ESceneMode.Story &&
            Pathea.PeGameMgr.sceneMode != Pathea.PeGameMgr.ESceneMode.Custom)
		    RandomMapConfig.Instance.SetMapParam ();
        if (Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Story)
            VFVoxelWater.c_fWaterLvl = 97.0f;
    }
#endregion
}
