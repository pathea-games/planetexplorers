using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ItemAsset;
using System.Linq;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtPlayerPackage;
using Pathea.PeEntityExtFollow;

// : There is another class BaseTowerDefense
public class MissionTowerDefense
{
	public static PlayerMission playerMission{ get { return MissionManager.Instance.m_PlayerMission; } }
	public static void ProcessMission (int missionID, int targetID)
	{
        if (!PeGameMgr.IsMulti)
			SceneEntityCreator.self.AddMissionPoint (missionID, targetID);
        else
            NetworkManager.WaitCoroutine(PlayerNetwork.RequestTowerDefense(missionID, targetID));
    }

}
public class MissionMonsterKill
{
	public static PlayerMission playerMission{ get { return MissionManager.Instance.m_PlayerMission; } }
	public static void ProcessMission (int missionID, int targetID)
	{
        if (!PeGameMgr.IsMulti)
			SceneEntityCreator.self.AddMissionPoint (missionID, targetID);
		else
			NetworkManager.WaitCoroutine(PlayerNetwork.RequestKillMonster(missionID, targetID));
	}
}