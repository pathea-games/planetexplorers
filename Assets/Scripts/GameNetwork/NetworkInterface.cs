using System;
using System.Collections.Generic;
using System.Linq;
using uLink;
using UnityEngine;

public enum EShutdownType
{
    Normal,
    Exception
}

[RequireComponent(typeof(uLink.NetworkView))]
public class NetworkInterface : uLink.MonoBehaviour
{
	#region Variables

	private static Dictionary<int, NetworkInterface> netIds = new Dictionary<int, NetworkInterface>();
	protected static Transform RootTrans;

	protected int _id;
	protected int _teamId;
    protected int _worldId;
    protected int lastAuthId;
	protected Dictionary<EPacketType, Action<uLink.BitStream, uLink.NetworkMessageInfo>> _actionEvent;
	protected uLink.NetworkView _netView;
	protected CommonInterface runner;

	#endregion Variables

	#region Properties

	public Vector3 _pos { get; set; }
	public Quaternion rot { get; set; }
	public bool canGetAuth { get; protected set; }

	public int Id { get { return _id; } }

	public int TeamId { get { return _teamId; } }

    public int WorldId { get { return _worldId; } }
	public int authId { get; protected set; }

	public uLink.NetworkView OwnerView { get { return _netView; } }
	public CommonInterface Runner { get { return runner; } }
	public bool hasOwnerAuth { get { return authId == PlayerNetwork.mainPlayerId; } }
	public bool hasAuth { get { return authId > 0; } }
	public static bool IsClient { get { return uLink.Network.isClient && uLink.Network.status == NetworkStatus.Connected && uLink.Network.peerType == uLink.NetworkPeerType.Client; } }
	public bool IsOwner { get { return IsClient && _netView != null && _netView.isOwner; } }
	public bool IsProxy { get { return IsClient && _netView != null && _netView.isProxy; } }

	#endregion Properties

	#region Static APIs

	protected static void Add(NetworkInterface obj)
	{
		if (null == obj)
			return;

		netIds[obj.Id] = obj;
	}

	protected static void Remove(int id)
	{
		if (ExistedId(id))
			netIds.Remove(id);
	}

	public static bool ExistedId(int id)
	{
		return netIds.ContainsKey(id);
	}

	public static NetworkInterface Get(int id)
	{
		return ExistedId(id) ? netIds[id] : null;
	}

	public static T Get<T>(int id) where T : NetworkInterface
	{
		return ExistedId(id) ? netIds[id] as T : null;
	}

	public static List<T> Get<T>() where T : NetworkInterface
	{
		var objs = from NetworkInterface iter in netIds.Values
				   where iter is T
				   select iter as T;

		return objs.ToList<T>();
	}

	public static void Connect(string host, int remotePort, string password, params object[] objs)
	{
		if (uLink.Network.status == uLink.NetworkStatus.Disconnected)
			uLink.Network.Connect(host, remotePort, password, objs);
	}

	public static void Connect(uLink.HostData host, string password, params object[] objs)
	{
		if (uLink.Network.status == uLink.NetworkStatus.Disconnected)
			uLink.Network.Connect(host, password, objs);
	}

	public static void Disconnect()
	{
        Disconnect(200);
	}

    static void Disconnect(int timeout)
    {
        uLink.Network.Disconnect(timeout);
    }

	#endregion Static APIs

	#region Internal APIs

	private void Awake()
	{
		_netView = GetComponent<uLink.NetworkView>();
		_actionEvent = new Dictionary<EPacketType, Action<uLink.BitStream, uLink.NetworkMessageInfo>>();

		OnPEAwake();
	}

	private void Start()
	{
		OnPEStart();

		Add(this);
	}

	private void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
	{
		OnPEInstantiate(info);
	}

	private void OnDestroy()
	{
		Remove(Id);
		OnPEDestroy();
	}

	public virtual void SetTeamId(int teamId)
	{
		_teamId = teamId;
	}

	protected void BindAction(EPacketType type, Action<uLink.BitStream, uLink.NetworkMessageInfo> action)
	{
		if (_actionEvent.ContainsKey(type))
		{
			if (LogFilter.logDebug) Debug.LogWarningFormat("Replace msg handler:{0}", type);
			_actionEvent.Remove(type);
		}

		if (LogFilter.logDev) Debug.LogWarningFormat("Register msg handler:{0}", type);
		_actionEvent.Add(type, action);
	}

	public void RPC(string func, params object[] args)
	{
		try
		{
			if (CheckPeer())
				_netView.RPC(func, uLink.RPCMode.Server, args);
		}
		catch (Exception e)
		{
			if (LogFilter.logError) Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), e.Message, e.StackTrace);
		}
	}

	public void RPCServer(params object[] args)
	{
		try
		{
            if (CheckPeer())
				_netView.RPC("RPC_Sync", uLink.RPCMode.Server, args);
		}
		catch (Exception e)
		{
			if (LogFilter.logError) Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), e.Message, e.StackTrace);
		}
	}

	public void URPCServer(params object[] objs)
	{
		try
		{
			if (CheckPeer())
				_netView.UnreliableRPC("URPC_Sync", uLink.RPCMode.Server, objs);
		}
		catch (Exception e)
		{
			if (LogFilter.logError) Debug.LogErrorFormat("{0}\r\n{1}\r\n{2}", GetType(), e.Message, e.StackTrace);
		}
	}

	bool CheckPeer()
	{
		return !IsClient || null == _netView ? false : (_netView.viewID == uLink.NetworkViewID.unassigned ? false : true);
	}

	[RPC]
	protected void URPC_Sync(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		EPacketType type = EPacketType.PT_MAX;

		try
		{
			type = stream.Read<EPacketType>();
			_actionEvent[type](stream, info);
		}
		catch (Exception e)
		{
			if (_actionEvent.ContainsKey(type))
			{
				if (LogFilter.logError) Debug.LogErrorFormat("Message:[{0}]\r\n{1}\r\n{2}\r\n{3}", GetType(), type, e.Message, e.StackTrace);
			}
			else
			{
				if (LogFilter.logDev) Debug.LogWarningFormat("Message:[{0}]|[{1}] does not implement\r\n{2}", type, GetType(), e.StackTrace);
			}
		}
	}

	[RPC]
	protected void RPC_Sync(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		EPacketType type = EPacketType.PT_MAX;

		try
		{
			type = stream.Read<EPacketType>();
			_actionEvent[type](stream, info);
		}
		catch (Exception e)
		{
			if (_actionEvent.ContainsKey(type))
			{
				if (LogFilter.logError) Debug.LogErrorFormat("Message:[{0}]\r\n{1}\r\n{2}\r\n{3}", GetType(), type, e.Message, e.StackTrace);
			}
			else
			{
				if (LogFilter.logDev) Debug.LogWarningFormat("Message:[{0}]|[{1}] does not implement\r\n{2}", type, GetType(), e.StackTrace);
			}
		}
	}

	protected virtual void OnPEAwake()
	{
		if (null == RootTrans)
		{
			GameObject RootObj = new GameObject("NetObjMgr");
			RootTrans = RootObj.transform;
			DontDestroyOnLoadCmpt cmpt = RootObj.GetComponent<DontDestroyOnLoadCmpt>();
			if (null == cmpt)
				RootObj.AddComponent<DontDestroyOnLoadCmpt>();
		}

		transform.parent = RootTrans;
	}

	protected virtual void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void OnPEStart()
	{
	}

	protected virtual void OnPEDestroy()
	{
        StopAllCoroutines();

        if (null != Runner)
            Runner.InitNetworkLayer(null);

        Pathea.EntityMgr.Instance.Destroy(Id);
        ISceneObjAgent obj = SceneMan.GetSceneObjById(Id);
        if (null != obj)
            SceneMan.RemoveSceneObj(obj);
    }

	public virtual void OnPeMsg(Pathea.EMsg msg, params object[] args)
	{
	}

	public virtual void OnSpawned(GameObject obj)
	{
		if (obj == null)
			throw new Exception("GameObject can not be null");

		runner = obj.GetComponentInChildren<CommonInterface>();
		if (null == runner)
			runner = obj.AddComponent<CommonNetworkObject>();

		Runner.InitNetworkLayer(this, obj);
	}

	public virtual void InitForceData()
	{

	}
	#endregion Internal APIs
}