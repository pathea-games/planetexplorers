using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(uLink.NetworkP2P))]
public class P2PManager : uLink.MonoBehaviour
{
	static List<ConnectedServer> connectedServers = new List<ConnectedServer>();
	static uLink.NetworkP2P p2p;

	public static event Action<string, int> OnServerDisconnectedEvent;
	public static event Action<string, int, int> OnServerRegisteredEvent;

    void Awake()
	{
		if (null == p2p)
			p2p = GetComponent<uLink.NetworkP2P>();
	}

	void Start()
	{
		MyServerController.OnServerCloseEvent += CloseServer;

		LoadServer.Start();
    }

	void OnDestroy()
	{
		MyServerController.OnServerCloseEvent -= CloseServer;
	}

	static void P2PRPC(string func, uLink.NetworkPeer peer, params object[] args)
	{
		try
		{
			p2p.RPC(func, peer, args);
		}
		catch (Exception e)
		{
			if (LogFilter.logFatal) Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", func, e.Message, e.StackTrace);
		}
	}

	public void uLink_OnPeerDisconnected(uLink.NetworkPeer peer)
	{
		ConnectedServer server = connectedServers.Find(it => it.peer == peer);
		if (server != null)
		{
            connectedServers.Remove(server);

			if (null != OnServerDisconnectedEvent)
				OnServerDisconnectedEvent(server.serverName, (int)server.sceneMode);
			
			if (LogFilter.logDebug) Debug.LogFormat("{0} closed", server.serverName);
		}
	}

	//[RPC]
	void RPC_S2C_CreateNewServer(uLink.BitStream stream, uLink.NetworkP2PMessageInfo info)
	{
		try
		{
			string srvName = stream.Read<string>();
			int srvMode = stream.Read<int>();
			int srvPort = stream.Read<int>();

			ConnectedServer cServer = connectedServers.Find(it => it.peer == info.sender);
			if (cServer == null)
			{
				ConnectedServer server = new ConnectedServer(srvName, srvMode, info.sender);
				connectedServers.Add(server);
			}

			LoadServer.AddServer(srvName, srvMode);

			if (null != OnServerRegisteredEvent)
				OnServerRegisteredEvent(srvName, srvMode, srvPort);
		}
		catch (Exception e)
		{
			if (LogFilter.logFatal) Debug.LogErrorFormat("RPC_S2C_CreateNewServer error\r\n{0}\r\n{1}", e.Message, e.StackTrace);
		}
	}

	public static void CloseAllServer()
	{
		if (null == p2p)
			return;

		foreach (ConnectedServer cs in connectedServers)
			P2PRPC("RPC_C2S_CloseServer", cs.peer);
	}

	static void CloseServer(string serverName, int sceneMode)
	{
		ConnectedServer server = connectedServers.Find(it => it.serverName.Equals(serverName) && it.sceneMode == (Pathea.PeGameMgr.ESceneMode)sceneMode);
		if (server != null)
			P2PRPC("RPC_C2S_CloseServer", server.peer);
	}

    public static ConnectedServer GetServer(string srvName, int sceneMode)
    {
        return connectedServers.Find(iter => iter.serverName.Equals(srvName) && iter.sceneMode == (Pathea.PeGameMgr.ESceneMode)sceneMode);
    }
}


public class ConnectedServer
{
	public Pathea.PeGameMgr.ESceneMode sceneMode;
	public uLink.NetworkPeer peer;
    public string serverName;

    public ConnectedServer(string serverName, int sceneMode, uLink.NetworkPeer peer)
	{
		this.serverName = serverName;
		this.sceneMode = (Pathea.PeGameMgr.ESceneMode)sceneMode;
		this.peer = peer;
	}
}