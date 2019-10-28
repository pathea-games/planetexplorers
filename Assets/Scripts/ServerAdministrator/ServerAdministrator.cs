using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum AdminMask : ulong
{
	None = 0,
	BlackRole = 1,
	AssistRole = 1 << 1,
	BuildLock = 1 << 2,
	AdminRole = 1 << 3 | 1 << 1
}

public class UserAdmin
{
	private int _id;
	private ulong _privileges;
	private string _roleName;

	public int Id { get { return _id; } }
	public ulong Privileges { get { return _privileges; } }
	public string RoleName { get { return _roleName; } }

	public UserAdmin(int id, string roleName, ulong privileges)
	{
		_id = id;
		_roleName = roleName;
		_privileges = privileges;
	}

	public void AddPrivileges(AdminMask mask)
	{
		_privileges |= (ulong)mask;
	}

	public void SetPrivileges(ulong mask)
	{
		_privileges = mask;
	}

	public void RemovePrivileges(AdminMask mask)
	{
		if (HasPrivileges(mask))
			_privileges ^= (ulong)mask;
	}

	public void Reset()
	{
		_privileges = 0;
	}

	public bool HasPrivileges(AdminMask mask)
	{
		return (_privileges & (ulong)mask) != 0;
	}
}

public class ServerAdministrator : MonoBehaviour
{
	#region Static Variables
	public static bool updataFlag = false;
	private static ServerAdministrator _instance;
	public static Dictionary<string, ulong> RoleAdmin = new Dictionary<string, ulong>();
	public static List<UserAdmin> UserAdminList = new List<UserAdmin>();
	private static List<int> LockedArea = new List<int>();
	#endregion

	#region Variables
	private static bool _allowJoin;
	private static bool _allowModify;
	#endregion

	#region Event
	public static Action<UserAdmin> PrivilegesChangedEvent;
	public static Action<int, bool> LockAreaChangedEvent;
	public static Action<bool> PlayerBanChangedEvent;
	public static Action<bool> BuildLockChangedEvent;
	#endregion

	#region Static Properties
	public static ServerAdministrator Instance { get { return _instance; } }

	public static bool AllowJoin
	{
		get { return _allowJoin; }
		set { _allowJoin = value; }
	}
	public static bool AllowModify
	{
		get { return _allowModify; }
		set { _allowModify = value; }
	}
	#endregion

	#region Properties
	public IEnumerable<UserAdmin> BlackRoles { get { return UserAdminList.Where(iter => IsBlack(iter.Id)); } }
	#endregion

	#region Unity Internal APIs
	void Awake()
    {
		_instance = this;

		init();
    }
	#endregion

	#region Internal APIs
	public void init()
    {
		UserAdminList.Clear();
		LockedArea.Clear();
		_allowJoin = true;
		_allowModify = true;
    }
	#endregion

	#region Static Internal APIs
	public static bool IsAdmin(int id)
	{
		return UserAdminList.Exists(iter => iter.Id == id && iter.HasPrivileges(AdminMask.AdminRole));
	}

	public static bool IsAssistant(int id)
	{
		return UserAdminList.Exists(iter => iter.Id == id && iter.HasPrivileges(AdminMask.AssistRole));
	}

	public static bool IsBlack(int id)
	{
		return UserAdminList.Exists(iter => iter.Id == id && iter.HasPrivileges(AdminMask.BlackRole));
	}

	public static bool IsBuildLock(int id)
	{
		if (!AllowModify)
			return true;

		return UserAdminList.Exists(iter => iter.Id == id && iter.HasPrivileges(AdminMask.BuildLock));
	}

	public static bool IsLockedArea(int areaIndex)
	{
		return LockedArea.Contains(areaIndex);
	}

	public static void AddBlacklist(int playerId)
    {
		UserAdmin ua = UserAdminList.Find(iter => iter.Id == playerId);
		if (null == ua)
			return;

		ua.RemovePrivileges(AdminMask.AssistRole);
		ua.AddPrivileges(AdminMask.BlackRole);
		OnPrivilegesChanged(ua);

//		if (null != GameGui_N.Instance)
//		{
//			GameGui_N.Instance.mPersonnelManageGui.OnBlackListInfoChange();
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
//		}
    }

    public static void DeleteBlacklist(int id)
    {
		if (!IsBlack(id))
			return;

		UserAdmin ua = UserAdminList.Find(iter => iter.Id == id);
		if (null == ua)
			return;

		ua.RemovePrivileges(AdminMask.BlackRole);
		OnPrivilegesChanged(ua);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnBlackListInfoChange();
    }

    public static void ClearBlacklist()
    {
		foreach (UserAdmin iter in UserAdminList)
			DeleteBlacklist(iter.Id);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnBlackListInfoChange();
    }

	public static void AddAssistant(int playerId)
    {
		UserAdmin ua = UserAdminList.Find(iter => iter.Id == playerId);
		if (null == ua)
			return;

		ua.RemovePrivileges(AdminMask.BlackRole);
		ua.AddPrivileges(AdminMask.AssistRole);
		OnPrivilegesChanged(ua);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
    }
    
    public static void DeleteAssistant(int id)
    {
		if (!IsAssistant(id))
			return;

		UserAdmin ua = UserAdminList.Find(iter => iter.Id == id);
		if (null == ua)
			return;

		ua.RemovePrivileges(AdminMask.AssistRole);
		OnPrivilegesChanged(ua);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
    }
    public static void ClearAssistant()
    {
		foreach (UserAdmin iter in UserAdminList)
			DeleteAssistant(iter.Id);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
    }

	public static void BuildLock(int playerId)
    {
		UserAdmin ua = UserAdminList.Find(iter => iter.Id == playerId);
		if (null == ua)
			return;

		ua.AddPrivileges(AdminMask.BuildLock);
		OnPrivilegesChanged(ua);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
    }
    //
	public static void BuildUnLock(int id)
    {
		if (!IsBuildLock(id))
			return;

		UserAdmin ua = UserAdminList.Find(iter => iter.Id == id);
		if (null == ua)
			return;

		ua.RemovePrivileges(AdminMask.BuildLock);
		OnPrivilegesChanged(ua);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
    }
	public static void ClearBuildLock()
    {
		foreach (UserAdmin iter in UserAdminList)
			BuildUnLock(iter.Id);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
    }

	public static void LockArea(int index)
    {
		if (!LockedArea.Contains(index))
		{
			LockedArea.Add(index);
			OnLockAreaChanged(index, true);
		}
    }

	public static void UnLockArea(int index)
    {
		LockedArea.Remove(index);
		OnLockAreaChanged(index, false);
    }

    public static bool SetBuildChunk(bool flag)
    {
		_allowModify = flag;
		OnBuildLockChanged(flag);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();

		return true;
    }

    public static bool SetJoinGame(bool flag)
    {
		_allowJoin = flag;
		OnPlayerBanChanged(flag);

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();

		return true;
    }

	static void OnPrivilegesChanged(UserAdmin ua)
	{
		if (null != PrivilegesChangedEvent)
			PrivilegesChangedEvent(ua);
	}

	static void OnLockAreaChanged(int index, bool isLock)
	{
		if (null != LockAreaChangedEvent)
			LockAreaChangedEvent(index, isLock);
	}

	static void OnBuildLockChanged(bool flag)
	{
		if (null != BuildLockChangedEvent)
			BuildLockChangedEvent(flag);
	}

	static void OnPlayerBanChanged(bool flag)
	{
		if (null != PlayerBanChangedEvent)
			PlayerBanChangedEvent(flag);
	}

	public static void DeserializeAdminData(byte[] data)
	{
		Instance.init();

		using (MemoryStream ms = new MemoryStream(data))
		using (BinaryReader reader = new BinaryReader(ms))
		{
			_allowJoin = BufferHelper.ReadBoolean(reader);
			_allowModify = BufferHelper.ReadBoolean(reader);

			OnPlayerBanChanged(_allowJoin);
			OnBuildLockChanged(_allowModify);

			int count = BufferHelper.ReadInt32(reader);
			for (int i = 0; i < count; i++)
			{
				int roleId = BufferHelper.ReadInt32(reader);
				string roleName = BufferHelper.ReadString(reader);
				ulong typeMask = BufferHelper.ReadUInt64(reader);

				UserAdmin ua = UserAdminList.Find(iter => iter.Id == roleId);
				if (null == ua)
				{
					ua = new UserAdmin(roleId, roleName, typeMask);
					UserAdminList.Add(ua);
				}
				else
				{
					ua.SetPrivileges(typeMask);
				}

				OnPrivilegesChanged(ua);
			}

			count = BufferHelper.ReadInt32(reader);
			for (int i = 0; i < count; i++)
			{
				int index =  BufferHelper.ReadInt32(reader);
				if (!LockedArea.Contains(index))
				{
					LockedArea.Add(index);
					OnLockAreaChanged(index, true);
				}
			}
		}
	}

	public static void ProxyPlayerAdmin(PlayerNetwork player)
	{
		if (null == player)
			return;

		UserAdmin ua = UserAdminList.Find(iter => iter.Id == player.Id);
		if (null == ua)
		{
			ua = new UserAdmin(player.Id, player.RoleName, 0);
			UserAdminList.Add(ua);
			OnPrivilegesChanged(ua);
		}

//		if (null != GameGui_N.Instance)
//			GameGui_N.Instance.mPersonnelManageGui.OnPersonnelInfoChange();
	}

	public static void RequestAddBlackList(int id)
	{
		if (PlayerNetwork.mainPlayerId == id)
			return;

		if (IsAdmin(PlayerNetwork.mainPlayerId) ||
			(IsAssistant(PlayerNetwork.mainPlayerId) && !IsAssistant(id)))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AddBlackList, id);
	}

	public static void RequestDeleteBlackList(int id)
	{
		if (PlayerNetwork.mainPlayerId == id)
			return;

		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_DelBlackList, id);
	}

	public static void RequestClearBlackList()
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearBlackList);
	}

	public static void RequestAddAssistants(int id)
	{
		if (PlayerNetwork.mainPlayerId == id)
			return;

		if (IsAdmin(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AddAssistant, id);
	}

	public static void RequestDeleteAssistants(int id)
	{
		if (PlayerNetwork.mainPlayerId == id)
			return;

		if (IsAdmin(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_DelAssistant, id);
	}

	public static void RequestClearAssistants()
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearAssistant);
	}

	public static void RequestBuildLock(int id)
	{
		if (PlayerNetwork.mainPlayerId == id)
			return;

		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_BuildLock, id);
	}

	public static void RequestBuildUnLock(int id)
	{
		if (PlayerNetwork.mainPlayerId == id)
			return;

		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_BuildUnLock, id);
	}

	public static void RequestClearBuildLock()
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearBuildLock);
	}

	public static void RequestClearVoxelData(int index)
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearVoxel, index);
	}

	public static void RequestClearAllVoxelData()
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearAllVoxel);
	}

	public static void RequestLockArea(int index)
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AreaLock, index);
	}

	public static void RequestUnLockArea(int index)
	{
		if (IsAssistant(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AreaUnLock, index);
	}

	public static void RequestSetBuildChunk(bool flag)
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_BlockLock, flag);
	}

	public static void RequestSetJoinGame(bool flag)
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_LoginBan, flag);
	}

	public static void RequestKickPlayer()
	{
		if (IsAdmin(PlayerNetwork.mainPlayerId))
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Kick);
	}
	#endregion
}