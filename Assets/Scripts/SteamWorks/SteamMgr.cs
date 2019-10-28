using Steamworks;
using System;
using UnityEngine;

class SteamMgr : MonoBehaviour
{
	static HAuthTicket hAuthTicket = HAuthTicket.Invalid;
	static bool m_bInitialized = false;

	public static CSteamID steamId { get; private set; }

	public static event Action<ulong> OnSteamInitEvent;

    private void Awake()
    {
		steamId = CSteamID.Nil;
#if SteamVersion
		if (SteamAPI.RestartAppIfNecessary(new AppId_t(237870u)))
		{
			Debug.LogError("Steamworks does not start from steam.");
			Application.Quit();
			return;
		}

		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError("[Steamworks] SteamAPI_Init() failed.");
			Application.Quit();
			return;
		}

		SteamClient.SetWarningMessageHook(new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook));
		SteamUtils.SetOverlayNotificationPosition(ENotificationPosition.k_EPositionTopRight);
		SteamRemoteStorage.SetCloudEnabledForApp(true);

		steamId = SteamUser.GetSteamID();
		if (LogFilter.logDebug) Debug.LogFormat("<color=red>Steam id:[{0}]</color>", steamId);

		enabled = true;
#else
		Destroy(gameObject);
#endif
	}

	void Start()
	{
#if SteamVersion && !LOCALTEST
		uLobby.Lobby.AddListener(this);
		uLobby.Lobby.OnConnected += Lobby_OnConnected;
		uLobby.Lobby.OnDisconnected += Lobby_OnDisconnected;
		uLobby.Lobby.OnFailedToConnect += Lobby_OnFailedToConnect;
#endif
    }

    void Lobby_OnConnected()
	{
		AccountLoginRequestSteamWorks();
	}

	void Lobby_OnFailedToConnect(uLobby.LobbyConnectionError error)
	{
		AccountLogoutSteamWorks();
    }

    void Lobby_OnDisconnected()
	{
		AccountLogoutSteamWorks();
	}

	void Shutdown()
	{
		if (m_bInitialized)
		{
			m_bInitialized = false;
			enabled = false;
			SteamAPI.Shutdown();
		}
	}

    void OnDestroy()
    {
		Shutdown();
    }

    void FixedUpdate()
    {
        if (m_bInitialized)
           	SteamAPI.RunCallbacks();
    }

	static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public void AccountLoginRequestSteamWorks()
	{
        if (!m_bInitialized)
            return;

        try
		{
			if (hAuthTicket != HAuthTicket.Invalid)
			{
				SteamUser.CancelAuthTicket(hAuthTicket);
				hAuthTicket = HAuthTicket.Invalid;
				return;
			}

			byte[] tokenByteArray = new byte[1024];
			uint tokenLen = 0;
			hAuthTicket = SteamUser.GetAuthSessionTicket(tokenByteArray, tokenByteArray.Length, out tokenLen);
            GameClientLobby.AccountLoginSteamWorks(tokenByteArray, tokenLen, steamId.m_SteamID);
		}
		catch (Exception e)
		{
			if (LogFilter.logError) Debug.LogErrorFormat("{0}\r\n{1}", e.Message, e.StackTrace);
			AccountLogoutSteamWorks();
		}
	}

	internal static void AccountLogoutSteamWorks()
	{
		try
		{
			if (m_bInitialized)
			{
				SteamUser.CancelAuthTicket(hAuthTicket);
				hAuthTicket = HAuthTicket.Invalid;
			}
		}
		catch (Exception e)
		{
			if (LogFilter.logError) Debug.LogErrorFormat("{0}\r\n{1}", e.Message, e.StackTrace);
		}
	}
}
