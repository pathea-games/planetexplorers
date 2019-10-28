using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using ItemAsset;
using TownData;
using CustomData;

public class TeamData
{
	private int _teamId = -1;
	private int _leaderId = -1;
	public List<PlayerNetwork> Members = new List<PlayerNetwork>();

	public int LeaderId { get { return _leaderId; } }
	public int TeamId { get { return _teamId; } }

	public TeamData(int teamId)
	{
		_teamId = teamId;
	}

	public void SetLeader(int leaderId)
	{
		_leaderId = leaderId;
	}

	public void AddMember(PlayerNetwork player)
	{
		if (!Members.Contains(player))
			Members.Add(player);
	}

	public void RemoveMember(PlayerNetwork player)
	{
		if (Members.Contains(player))
			Members.Remove(player);
	}
	
	public void Reset()
	{
		_teamId = -1;
		_leaderId = -1;

		Members.Clear();
	}
}

public class GroupNetwork
{
	internal const int minTeamID = 10000;
	internal const int maxTeamID = 19999;

	private static ItemPackage _itemPackage = new ItemPackage (200);
    private static List<TeamData> _teamInfo = new List<TeamData>();
	private static List<PlayerNetwork> joinReqeust = new List<PlayerNetwork>();

	public static SlotList GetSlotList(ItemPackage.ESlotType type)
	{
		return _itemPackage.GetSlotList(type);
	}

    public static bool TeamExists(int teamId)
    {
        return _teamInfo.Exists(iter => iter.TeamId == teamId);
    }

	static TeamData NewTeam(int teamId)
	{
		TeamData td = new TeamData(teamId);
		_teamInfo.Add(td);
		return td;
	}

	public static void AddJoinRequest(int teamId, PlayerNetwork player)
	{
		if (!joinReqeust.Contains(player))
			joinReqeust.Add(player);
	}

	public static void GetJoinRequest(List<PlayerNetwork> players)
	{
		for (int i = 0; i < joinReqeust.Count; i++)
		{
			if (!players.Contains(joinReqeust[i]))
				players.Add(joinReqeust[i]);
		}
	}

	public static bool IsJoinRequest(PlayerNetwork player)
	{
		return null == player ? false : joinReqeust.Contains(player);
	}

	public static void DelJoinRequest(PlayerNetwork player)
	{
		if (joinReqeust.Contains(player))
			joinReqeust.Remove(player);
	}

	public static void ClearJoinRequest()
	{
		joinReqeust.Clear();
	}

	public static TeamData AddToTeam(int teamId, PlayerNetwork player)
	{
		if (-1 == teamId || null == player)
			return null;

        TeamData td = _teamInfo.Find(iter => iter.TeamId == teamId);
		if (null == td)
			td = NewTeam(teamId);

        td.AddMember(player);

        return td;
	}

	public static void RemoveFromTeam(int teamId, PlayerNetwork player)
	{
        TeamData td = _teamInfo.Find(iter => iter.TeamId == teamId);
        if (null == td)
            return;

		td.RemoveMember(player);
	}

    public static void GetMembers(int teamId, ref List<PlayerNetwork> members)
    {
        TeamData td = _teamInfo.Find(iter => iter.TeamId == teamId);
        if (null == td)
            return;

        members.AddRange(td.Members);
    }

	public static int GetLeaderId(int teamId)
	{
        TeamData td = _teamInfo.Find(iter => iter.TeamId == teamId);
        if (null == td)
            return -1;

        return td.LeaderId;
	}

	public static bool IsEmpty(int teamId)
	{
		int index = _teamInfo.FindIndex(iter => iter.TeamId == teamId);
		return -1 == index ? false : _teamInfo[index].Members.Count <= 1;
	}

	public static TeamData GetTeamInfo(int teamId)
	{
        return _teamInfo.Find(iter => iter.TeamId == teamId);
    }

	public static TeamData[] GetTeamInfos()
	{
		return _teamInfo.ToArray();
	}

	//void Start()
	//{
	//    _Items.ExtendPackage(200, 200, 200);
	//    _Items.Clear();
	//}

	//	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
	//	{
	//		teamNum = group = networkView.group;
	//
	//		_Items.Clear();
	//		_Items.ExtendPackage(200, 200, 200);
	//
	//		GameGui_N.Instance.mPublicInventoryGui.SetItempackage(_Items);
	//
	//		RPC("RPC_C2S_RequestPackage");
	//	}

	//[RPC]
	//void RPC_S2C_SendMsg(TMsgInfo msg)
	//{
	//	if (null != GameGui_N.Instance && null != msg)
	//		GameGui_N.Instance.mChatGUI.AddChat(msg.name, msg.msg, (int)msg.msgtype);
	//}

	//[RPC]
	//void RPC_S2C_GroupAccessMission(uLink.BitStream stream)
	//{
	//	int nMissionID = stream.Read<int>();
	//	int nNpcID = stream.Read<int>();
	//	byte[] adrmData = stream.Read<byte[]>();

	//	AdRMRepository.Import(adrmData);
	//	AiNpcObject npc = NpcManager.Instance.GetNpc(nNpcID);
	//	AiAdNpcNetwork adNpc = NpcManager.Instance.GetMutNpcData(nNpcID);

	//	GameGui_N.Instance.mMissionGui.ProcessSingleMode(nMissionID, npc, 1, true, adNpc);
	//}

	////同步任务
	//[RPC]
	//void RPC_S2C_GroupSyncMissions(uLink.BitStream stream)
	//{
	//	if (null == PlayerFactory.mMainPlayer.m_PlayerMission)
	//		return;

	//	byte[] pmData = stream.Read<byte[]>();
	//	byte[] adrmData = stream.Read<byte[]>();

	//	if (pmData == null)
	//		return;

	//	if (adrmData == null)
	//		return;

	//	AdRMRepository.Import(adrmData);
	//	PlayerFactory.mMainPlayer.m_PlayerMission.ImportNetwork(pmData, 1);
	//	PlayerFactory.mMainPlayer.InitPlayerMission();
	//}

	//[RPC]
	//void RPC_S2C_GroupModifyMissionFlag(uLink.BitStream stream)
	//{
	//	int missionid;
	//	string missionflag, missionvalue;
	//	stream.TryRead<int>(out missionid);
	//	stream.TryRead<string>(out missionflag);
	//	stream.TryRead<string>(out missionvalue);

	//	PlayerFactory.mMainPlayer.ModifyQuestVariable(missionid, missionflag, missionvalue);
	//}

	//[RPC]
	//void RPC_S2C_ResponseGroupDeleteMission(uLink.BitStream stream)
	//{
	//	int missionid;
	//	stream.TryRead<int>(out missionid);

	//	PlayerFactory.mMainPlayer.AbortMission(missionid);
	//}

	//[RPC]
	//void RPC_S2C_GroupCompleteTarget(uLink.BitStream stream)
	//{
	//	int targetid, missionid;
	//	stream.TryRead<int>(out targetid);
	//	stream.TryRead<int>(out missionid);

	//	PlayerFactory.mMainPlayer.CompleteTarget(targetid, missionid);
	//}

	//[RPC]
	//void RPC_S2C_GroupReplyCompleteMission(uLink.BitStream stream)
	//{
	//	int nMissionID;
	//	int nTargetID;

	//	nTargetID = stream.Read<int>();
	//	nMissionID = stream.Read<int>();

	//	PlayerFactory.mMainPlayer.m_PlayerMission.CompleteMission(nMissionID, nTargetID);
	//}
}
