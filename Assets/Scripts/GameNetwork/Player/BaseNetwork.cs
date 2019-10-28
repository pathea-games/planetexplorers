using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomData;
using uLink;
using System.IO;
using System.Linq;
using System;
using TownData;
using ItemAsset;
using Steamworks;

public class BaseNetwork : NetworkInterface
{
	#region Static Variables
	static Dictionary<int, RoomPlayerInfo> RolesInRoom = new Dictionary<int, RoomPlayerInfo>();
	static Dictionary<int, BaseNetwork> BasePeers = new Dictionary<int, BaseNetwork>();
	static BaseNetwork _mainPlayer;
	public static PlayerDesc curPlayerDesc { get; protected set; }
	static List<Vector3> delTreePos = new List<Vector3>();
	static List<Vector3> delGrassPos = new List<Vector3>();
	#endregion

	#region Static Properties
	public static BaseNetwork MainPlayer { get { return _mainPlayer; } }
	#endregion

	#region Static APIs
	public static BaseNetwork GetBaseNetwork(int id)
	{
		return BasePeers.ContainsKey(id) ? BasePeers[id] : null;
	}

    public static bool HasBaseNetwork(int id)
    {
        return BasePeers.ContainsKey(id);
    }

	public static Dictionary<int, BaseNetwork> GetBaseNetworkList()
	{
		return BasePeers;
	}

    void AddBaseNetwork()
    {
        if (BasePeers.ContainsKey(Id))
			BasePeers.Remove(Id);

		BasePeers.Add(Id, this);
    }

    void DelBaseNetwork()
    {
		if (BasePeers.ContainsKey(Id))
			BasePeers.Remove(Id);
    }

	public static bool IsInRoom()
	{
		if (RolesInRoom != null && RolesInRoom.Count > 0)
			return true;
		return false;
	}
	public static Nullable<CSteamID> GetSteamID(string roleName)
	{
		foreach (var role in BasePeers)
		{
			if (role.Value.RoleName == roleName)
			{
				return role.Value.SteamID;
			}
		}
		return null;
	}
	#endregion

	#region Variables
	private RoleInfo _role;
	private ENetworkState _networkState;
	private CSteamID _steamID;
    private int _colorIndex;
    private int _descId;
	private bool _useNewPos;
	#endregion

	#region Properties
	public string RoleName { get { return Role.name; } }
	public byte Sex { get { return Role.sex; } }
	public RoleInfo Role { get { return _role; } }
	public ENetworkState NetworkState { get { return _networkState; } }
	public CSteamID SteamID { get { return _steamID; } }
    public int ColorIndex { get { return _colorIndex; } }
    public int DescId { get { return _descId; } }
	public bool UseNewPos { get { return _useNewPos; } }
	#endregion

	

	#region Internal APIs
	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_role = info.networkView.initialData.Read<RoleInfo>();

		_networkState = ENetworkState.Null;
		_id = Role.roleID + 3000000;
		name = Role.name;

        AddBaseNetwork();
    }

	protected override void OnPEStart()
	{
		BindAction(EPacketType.PT_InRoom_InitData, RPC_S2C_InitData);
		BindAction(EPacketType.PT_InRoom_TeamChange, RPC_S2C_TeamChange);
		BindAction(EPacketType.PT_InRoom_StatusChange, RPC_S2C_RoomStatusChanged);
		BindAction(EPacketType.PT_InRoom_StartLogin, RPC_S2C_StartLogin);
		BindAction(EPacketType.PT_InRoom_Message, RPC_Message);
		BindAction(EPacketType.PT_InRoom_Ping, RPC_Ping);
        BindAction(EPacketType.PT_InRoom_KickPlayer, RPC_S2C_KickPlayer);
		BindAction(EPacketType.PT_Common_RandomTownData, RPC_S2C_RandomTownData);
        BindAction(EPacketType.PT_InGame_GrassInfo, RPC_S2C_GrassInfo);
        BindAction(EPacketType.PT_InGame_TreeInfo, RPC_S2C_TreeInfo);

        if (IsOwner)
		{
			if (Pathea.PeGameMgr.IsMultiCustom || Pathea.PeGameMgr.IsMultiStory)
				LSubTerrSL.OnLSubTerrSLInitEvent += CacheDelTree;
			else
				RSubTerrSL.OnRSubTerrSLInitEvent += CacheDelTree;

			GrassDataSL.OnGrassDataInitEvent += CacheDelGrass;

			_mainPlayer = this;
			RequestUGC();
		}

		RequestInitData();
	}

	protected override void OnPEDestroy()
	{
        DelBaseNetwork();

		if (ReferenceEquals(this, MainPlayer))
		{
			if (Pathea.PeGameMgr.IsMultiCustom || Pathea.PeGameMgr.IsMultiStory)
				LSubTerrSL.OnLSubTerrSLInitEvent -= CacheDelTree;
			else
				RSubTerrSL.OnRSubTerrSLInitEvent -= CacheDelTree;

			GrassDataSL.OnGrassDataInitEvent -= CacheDelGrass;
		}

		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
			RoomGui_N.RemoveRoomPlayerByNet(Id);
	}

	public bool UseNewPosition(bool hasRecord, int recordTeamId)
	{
		if (!Pathea.PeGameMgr.IsCustom) {
			if(Pathea.PeGameMgr.IsVS)
				return (!hasRecord || recordTeamId != TeamId);
			else
				return !hasRecord;
		}

		return false;
	}

	void CacheDelTree()
	{
		if (delTreePos.Count != 0)
			DigTerrainManager.CacheDeleteTree(delTreePos);
	}

	void CacheDelGrass()
	{
		if (delGrassPos.Count != 0)
			DigTerrainManager.CacheDeleteGrass(delGrassPos);
	}
	#endregion

	#region Network Request
	public void RequestPlayerLogin(Vector3 pos)
	{
		RPCServer(EPacketType.PT_InGame_PlayerLogin, pos);
        PlayerNetwork.ResetTerrainState();
    }

	public void RequestUGC()
	{
		NetworkManager.SyncServer(EPacketType.PT_Common_RequestUGC);
	}

	public void RequestInitData()
	{
		RPCServer(EPacketType.PT_InRoom_InitData, IsOwner);
	}

	public void RequestChangeTeam(int forceId, int playerId)
	{
		RPCServer(EPacketType.PT_InRoom_TeamChange, forceId, playerId);
	}

	public void RequestChangeStatus(ENetworkState state)
	{
		RPCServer(EPacketType.PT_InRoom_StatusChange, (int)state);
	}

	public void SendMsg(string msg)
	{
		RPCServer(EPacketType.PT_InRoom_Message, msg);
	}

    public void KickPlayer(int playerId)
    {
        RPCServer(EPacketType.PT_InRoom_KickPlayer, playerId);
    }

    
	#endregion

	#region Action Callback APIs
	void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_networkState = stream.Read<ENetworkState>();
		_teamId = stream.Read<int>();
        _colorIndex = stream.Read<int>();
        _descId = stream.Read<int>();

        RoomPlayerInfo rpi= new RoomPlayerInfo();
        rpi.mId = Id;
        rpi.mPlayerInfo.mName = Role.name;
        rpi.mPlayerInfo.mSex = Role.sex;
        rpi.mPlayerInfo.mLevel = Role.level;
        rpi.mPlayerInfo.mWinnRate = Role.winrate;
        rpi.mFocreID = Pathea.PeGameMgr.IsSurvive && !Pathea.PeGameMgr.IsCustom ? -1 : TeamId;
        rpi.mRoleID = _descId;
        rpi.mState = (int)NetworkState;
        RoomGui_N.InitRoomPlayerByNet(rpi);
	}

	void RPC_S2C_StartLogin(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_pos = transform.position = stream.Read<Vector3>();
		int recordTeamId = stream.Read<int>();
		bool hasRecord = stream.Read<bool>();
		_networkState = stream.Read<ENetworkState>();

		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
			RoomGui_N.ChangePlayerStateByNet(Id, (int)NetworkState);

		_useNewPos = UseNewPosition (hasRecord, recordTeamId);

        ChunkManager.Clear();

		if (null != PeSceneCtrl.Instance)
			PeSceneCtrl.Instance.GotoGameSence();

		if (Pathea.PeGameMgr.IsCustom)
		{
			int descId = -1 == DescId ? Id : DescId;
			curPlayerDesc = ForceSetting.AddPlayer(descId, TeamId, EPlayerType.Human, RoleName);
		}
	}
	
	void RPC_S2C_TeamChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int>();
        int descId = stream.Read<int>();

        if (!Pathea.PeGameMgr.IsMultiCustom)
        {
            if (TeamId == index)
                return;
        }

        _teamId = index;
        _descId = descId;
        RoomPlayerInfo rpi;
        if (!RolesInRoom.ContainsKey(Id))
        {
            rpi = new RoomPlayerInfo();
            rpi.mId = Id;
            rpi.mPlayerInfo.mName = Role.name;
            rpi.mPlayerInfo.mSex = Role.sex;
            RolesInRoom[Id] = rpi;
        }
        else
        {
            rpi = RolesInRoom[Id];
        }

        rpi.mPlayerInfo.mLevel = Role.level;
        rpi.mPlayerInfo.mWinnRate = Role.winrate;
        rpi.mFocreID = Pathea.PeGameMgr.IsSurvive && !Pathea.PeGameMgr.IsCustom ? -1 : TeamId;
        rpi.mRoleID = _descId;
        rpi.mState = (int)NetworkState;

        _steamID = SteamMgr.steamId;

        if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
            RoomGui_N.ChangeRoomPlayerByNet(rpi);
	}

	void RPC_S2C_RoomStatusChanged(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		_networkState = stream.Read<ENetworkState>();

		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
			RoomGui_N.ChangePlayerStateByNet(Id, (int)NetworkState);

		if (IsOwner)
		{
			delGrassPos.Clear();
			delTreePos.Clear();
		}
	}

	void RPC_Message(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string msg = stream.Read<string>();
        RoomGui_N.GetNewMsgByNet(RoleName, msg);
	}

	void RPC_Ping(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int ping = stream.Read<int>();

		if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
			RoomGui_N.ChangePlayerDelayByNet(Id, ping);
	}

    void RPC_S2C_KickPlayer(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int playerInstanceId = stream.Read<int>();
        RoomGui_N.KickPlayerByNet(playerInstanceId);
	}

	void RPC_S2C_RandomTownData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] _capturedCampID;
		stream.TryRead<int[]>(out _capturedCampID);
		
		StartCoroutine(VArtifactTownManager.WaitForArtifactTown(_capturedCampID));
	}

    void RPC_S2C_GrassInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int count = stream.Read<int>();
		byte[] data = stream.Read<byte[]>();

		PETools.Serialize.Import(data, (r) =>
		{
			for (int i = 0; i < count; i++)
			{
				Vector3 pos;
				BufferHelper.ReadVector3(r, out pos);
				delGrassPos.Add(pos);
			}
		});
    }

    void RPC_S2C_TreeInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int count = stream.Read<int>();
		byte[] data = stream.Read<byte[]>();

		PETools.Serialize.Import(data, (r) =>
		{
			for (int i = 0; i < count; i++)
			{
				Vector3 pos;
				BufferHelper.ReadVector3(r, out pos);
				delTreePos.Add(pos);
			}
		});
    }
    #endregion
}

