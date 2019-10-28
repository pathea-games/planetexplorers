using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class BattleManager
{
    internal static Dictionary<int, BattleInfo> CampList = new Dictionary<int, BattleInfo>();
	internal static Dictionary<int, List<PlayerBattleInfo>> BattleInfoDict = new Dictionary<int, List<PlayerBattleInfo>>();

    static int mTeamNum;
    static int mNumberTeam;

    public static int teamNum
    {
        get
        {
            return mTeamNum;
        }
    }

    public static int numberTeam
    {
        get
        {
            return mNumberTeam;
        }
    }

    internal static void InitBattleInfo(int teamNum = 1, int numberTeam = 4)
    {
        mTeamNum = teamNum;
        mNumberTeam = numberTeam;

		CampList.Clear();
		BattleInfoDict.Clear();

		for (int i = 0; i < teamNum; i++)
		{
			CampList[i] = new BattleInfo();
			BattleInfoDict[i] = new List<PlayerBattleInfo>();
		}
    }

    internal static BattleInfo GetBattleInfo(int camp)
    {
        if (!CampList.ContainsKey(camp))
            return null;

        return CampList[camp];
    }

	#region Action Callback APIs
	public static void RPC_S2C_BattleOver(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*int winTeam = */stream.Read<int>();

//		if (null != GameGui_N.Instance)
//		{
//			GameGui_N.Instance.mMultyScoreGui.AwakeWindow();
//			GameGui_N.Instance.mMultyScoreGui.SetGameResult(BaseNetwork.MainPlayer.TeamId == winTeam);
//		}
	}

	public static void RPC_S2C_BattleInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		BattleInfo battleInfo = stream.Read<BattleInfo>();

		if (CampList.ContainsKey(battleInfo._group))
			CampList[battleInfo._group].Update(battleInfo);
		else
			CampList[battleInfo._group] = battleInfo;
	}

	public static void RPC_S2C_BattleInfos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		BattleInfo[] battleInfos = stream.Read<BattleInfo[]>();
		foreach (BattleInfo battleInfo in battleInfos)
		{
			if (CampList.ContainsKey(battleInfo._group))
				CampList[battleInfo._group].Update(battleInfo);
			else
				CampList[battleInfo._group] = battleInfo;
		}
	}
	#endregion
}
