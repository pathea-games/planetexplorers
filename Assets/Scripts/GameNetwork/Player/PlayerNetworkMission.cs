using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using SkillAsset;
using Pathea;


/*
 * 任务网络类
 */

public partial class PlayerNetwork
{
	public static bool _missionInited = false;

	List<string> _commandCache = new List<string>();
    int _mission953Item;

    bool StroyNpcInitCheck()
	{
        //if (!PeGameMgr.IsMultiStory)
        //    return true;
		List<AiAdNpcNetwork> npcList = AiAdNpcNetwork.Get<AiAdNpcNetwork>();
        if (npcList == null || npcList.Count == 0)
            return false;

        foreach (var iter in npcList)
        {
            if (iter == null || iter.npcCmpt == null || !iter._npcMissionInited)
            {
                return false;
            }
        }

        return true;
	}

    void AdventureInitStart()
    {
        if (PeGameMgr.IsAdventure && !PeGameMgr.IsVS)
        {
            if (!MissionManager.Instance.m_PlayerMission.HasMission(9027))
            {
                if (!MissionManager.Instance.m_PlayerMission.HadCompleteMission(9027))
                {
                    MissionManager.Instance.StartCoroutine(AdventureInit());
                }
            }
        }
    }

    IEnumerator AdventureInit()
    {
        while (VArtifactTownManager.Instance == null)
        {
            yield return 0;
        }
        int missionStartNpcEntityId = 1;
        while (!EntityMgr.Instance.Get(missionStartNpcEntityId))
        {
            yield return 0;
        }
        PeEntity npc = EntityMgr.Instance.Get(missionStartNpcEntityId);
        GameUI.Instance.mNpcWnd.m_CurSelNpc = npc;
        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(9027, 1);
        GameUI.Instance.mNPCTalk.PreShow();
        MissionManager.Instance.m_PlayerMission.UpdateAllNpcMisTex();
    }
    IEnumerator WaitForMissionModule (byte[] pmData, byte[] tmData)
	{
		while (null == MissionManager.Instance)
			yield return null;

        //MissionManager.Instance.m_PlayerMission.ClearMission();
        bool hasData = false;
		if (IsOwner)
        {
            hasData = MissionManager.Instance.m_PlayerMission.ImportNetwork(pmData);
            hasData |= MissionManager.Instance.m_PlayerMission.ImportNetwork(tmData);
        }            
		else
        {
            hasData = MissionManager.Instance.m_PlayerMission.ImportNetwork(pmData, 1);
            hasData |= MissionManager.Instance.m_PlayerMission.ImportNetwork(tmData, 1);
        }
            

		while (null == PlayerNetwork.mainPlayer || !StroyNpcInitCheck() || PeCreature.Instance.mainPlayer == null)
			yield return null;
        if (hasData)
        {
            MissionManager.Instance.InitPlayerMission();
            ExcuCacheCommands();
            if (PeGameMgr.IsMultiStory)
            {
                StroyManager.Instance.InitMission();
                GameUI.Instance.mUIMissionWndCtrl.ReGetAllMission();
            }
        }
        else
            MissionManager.Instance.m_bHadInitMission = true;
        ExcuCacheCommands();
        _missionInited = true;
        if(PeGameMgr.IsAdventure && !PeGameMgr.IsVS)
            AdventureInitStart();
    }

   

    public static IEnumerator RequestKillMonster(int missionId, int targetId)
	{
		while (null == mainPlayer)
			yield return null;

		mainPlayer.RPCServer(EPacketType.PT_InGame_MissionKillMonster, missionId, targetId);
	}

	public static IEnumerator RequestTowerDefense(int missionId, int targetId)
	{
		while (null == mainPlayer)
			yield return null;

        Vector3 mPos = AiTowerDefense.GetTdGenPos(targetId);
        mainPlayer.RPCServer(EPacketType.PT_InGame_MissionTowerDefense, missionId, targetId, mPos);
	}
	#region mission functions

	void ExcuCacheCommands()
	{
		foreach(var iter in _commandCache)
		{
			string [] command = iter.Split('@');
			if(command[0] == "CreateMission")
			{
				CreateMission(Convert.ToInt32(command[1]),Convert.ToInt32(command[2]),Convert.ToInt32(command[3]));
			}
			else if(command[0] == "AccessMission")
			{
				if(IsOwner)
				{
					AccessMission(Convert.ToInt32(command[1]),Convert.ToInt32(command[2]));
				}
				else
				{
					AccessMission(Convert.ToInt32(command[1]),Convert.ToInt32(command[2]), Convert.ToBoolean(command[3]),System.Text.Encoding.Default.GetBytes(command[4]));
				}
			}
			else if(command[0] == "DeleteMission")
			{
				DeleteMission(Convert.ToInt32(command[1]));
			}
			else if(command[0] == "ModifyMissionFlag")
			{
				ModifyMissionFlag(Convert.ToInt32(command[1]),command[2],command[3]);
			}
			else if(command[0] == "CompleteTarget")
			{
				CompleteTarget(Convert.ToInt32(command[1]),Convert.ToInt32(command[2]),Convert.ToInt32(command[3]));
			}
			else if(command[0] == "ReplyCompleteMission")
			{
				ReplyCompleteMission(Convert.ToInt32(command[1]),Convert.ToInt32(command[2]),Convert.ToBoolean(command[3]));
			}
			else if(command[0] == "FailMission")
			{
				FailMission(Convert.ToInt32(command[1]));
			}
		}
		_commandCache.Clear();
	}

	void CreateMission(int nMissionID,int idx,int rewardIdx )
	{
		MissionCommonData data = MissionManager.GetMissionCommonData (nMissionID);
		if (data == null)
			return;
		if (PeGameMgr.IsMultiStory)
			RMRepository.CreateRandomMission (nMissionID, idx, rewardIdx);
		else
        {
            AdRMRepository.CreateRandomMission(nMissionID, idx, rewardIdx);
        }
			
	}

    void AccessMission(int nMissionID, int nNpcID, bool bCheck = true, byte[] adrmData = null)
    {
        PeEntity npc = EntityMgr.Instance.Get(nNpcID);

        if (IsOwner)
        {
            MissionManager.Instance.ProcessSingleMode(nMissionID, npc, MissionManager.TakeMissionType.TakeMissionType_Get, bCheck);
        }
        else
        {
            if (PeGameMgr.IsMultiStory)
                RMRepository.Import(adrmData);
            else
                AdRMRepository.Import(adrmData);

            AiAdNpcNetwork adNpc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(nNpcID);
            MissionManager.Instance.ProcessSingleMode(nMissionID, npc, MissionManager.TakeMissionType.TakeMissionType_Get, bCheck, adNpc);
        }

        if (null != npc)
            npc.SetAttribute(AttribType.DefaultPlayerID, Id, false);
    }

	void DeleteMission(int missionid)
	{
        MissionManager.Instance.m_PlayerMission.AbortMission(missionid);
	}
    void UpdateMissionMapLabelPos(int missionId, int targetId,Vector3 pos)
    {
        UIMissionMgr.MissionView missview = UIMissionMgr.Instance.GetMissionView(missionId);
        if (missview == null)
            return;

        UIMissionMgr.TargetShow tarshow = missview.mTargetList.Find(ite => UIMissionMgr.MissionView.MatchID(ite, targetId));
        if (tarshow != null)
        {

            MissionLabel label = PeMap.LabelMgr.Instance.Find(item =>
            {
                if (item is MissionLabel)
                {
                    MissionLabel misLabel = item as MissionLabel;
                    if (misLabel.m_missionID == missionId&& misLabel.m_type== MissionLabelType.misLb_target && misLabel.m_target == tarshow)
                        return true;
                }
                return false;
            }) as MissionLabel;

            if (label != null)
                label.SetLabelPos(pos,true);
        }
    }
    void ModifyMissionFlag(int missionid,string missionflag, string missionvalue)
	{
		MissionManager.Instance.ModifyQuestVariable (missionid, missionflag, missionvalue);
	}

	void CompleteTarget(int targetid, int missionid,int playerId)
	{
		MissionManager.Instance.CompleteTarget(targetid, missionid,false,true);
	}

	void ReplyCompleteMission(int nMissionID,  int nTargetID,  bool bCheck)
	{
		MissionManager.Instance.CompleteMission(nMissionID, nTargetID, bCheck);
	}

	void FailMission(int nMissionID)
	{
		MissionManager.Instance.FailureMission (nMissionID);
	}
    void Mission953(int nMissionID, int itemId)
    {
        if (nMissionID != MissionManager.m_SpecialMissionID93)
            return;
        ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(itemId);
        if (itemObj == null)
            return;

        int itemid = 0;
        if (itemObj.protoId > 100000000)
            itemid = StroyManager.Instance.ItemClassIdtoProtoId(itemObj.protoData.itemClassId);
        else
            itemid = itemObj.protoId;
        if (nMissionID == MissionManager.m_SpecialMissionID93 && MissionManager.Instance.m_PlayerMission.IsSpecialID(itemid) == ECreation.SimpleObject)
        {
            CreationData creationData = CreationMgr.GetCreation(itemObj.instanceId);
            if (creationData != null)
            {
                int costNum = 0;
                foreach (var cost in creationData.m_Attribute.m_Cost.Values)
                    costNum += cost;
                if (costNum <= 300)
                    StroyManager.Instance.GetMissionOrPlotById(10954);
                else
                    StroyManager.Instance.GetMissionOrPlotById(10955);
            }
        }
    }
    #endregion
    #region Action Callback APIs
    void RPC_S2C_CreateMission (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nMissionID = stream.Read<int> ();
		int idx = stream.Read<int> ();
		int rewardIdx = stream.Read<int> ();
        if (MissionManager.Instance.m_bHadInitMission)
			CreateMission( nMissionID, idx, rewardIdx);
		else
		{
			string cmd = "CreateMission@" + nMissionID.ToString() + "@" + idx.ToString() + "@" + rewardIdx.ToString() + "@";
			mainPlayer._commandCache.Add(cmd);
		}
	}

	//添加任务
	void RPC_S2C_AccessMission (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nMissionID = stream.Read<int> ();
		int nNpcID = stream.Read<int> ();
        bool bCheck = stream.Read<bool>();
		if(MissionManager.Instance.m_bHadInitMission)
		{
			if(!IsOwner)
			{
				byte[] adrmData = stream.Read<byte[]> ();
				AccessMission(nMissionID,nNpcID, bCheck,adrmData);
			}
			else
			{
				AccessMission(nMissionID,nNpcID, bCheck);
			}
		}
		else
		{
			if(!IsOwner)
			{
				byte[] adrmData = stream.Read<byte[]> ();
				string cmd = "AccessMission@"+nMissionID.ToString() + "@" + nNpcID.ToString() + "@" + System.Text.Encoding.Default.GetString(adrmData) + "@";
				mainPlayer._commandCache.Add(cmd);
			}
			else
			{
				string cmd = "AccessMission@"+nMissionID.ToString() + "@" + nNpcID.ToString() + "@" ;
				mainPlayer._commandCache.Add(cmd);
			}
		}
    }

	void RPC_S2C_DeleteMission (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int missionid;
		stream.TryRead<int> (out missionid);
		if(MissionManager.Instance.m_bHadInitMission)
			DeleteMission(missionid);
		else
		{
			string cmd = "DeleteMission@" + missionid.ToString() + "@";
			mainPlayer._commandCache.Add(cmd);
		}
	}
	void RPC_S2C_ModifyMissionFlag (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int missionid;
		string missionflag, missionvalue;
		stream.TryRead<int> (out missionid);
		stream.TryRead<string> (out missionflag);
		stream.TryRead<string> (out missionvalue);
		if(MissionManager.Instance.m_bHadInitMission)
			ModifyMissionFlag( missionid, missionflag,  missionvalue);
		else
		{
			string cmd = "ModifyMissionFlag@" + missionid.ToString() + "@" + missionflag + "@" + missionvalue + "@";
			mainPlayer._commandCache.Add(cmd);
		}
	}
	
	void RPC_S2C_CompleteTarget (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int targetid, missionid,playerId;
		stream.TryRead<int>(out targetid);
		stream.TryRead<int>(out missionid);
		stream.TryRead<int>(out playerId);
		if(MissionManager.Instance.m_bHadInitMission)
			CompleteTarget(targetid, missionid,playerId);
		else
		{
			string cmd = "CompleteTarget@" + targetid.ToString() + "@" + missionid.ToString() + "@" + playerId.ToString() + "@";
			mainPlayer._commandCache.Add(cmd);
		}
		
	}
	
	void RPC_S2C_ReplyCompleteMission (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nMissionID;
		int nTargetID;
		bool bCheck;
		
		nTargetID = stream.Read<int> ();
		nMissionID = stream.Read<int> ();
		bCheck = stream.Read<bool>();
		if(MissionManager.Instance.m_bHadInitMission)
        {
            ReplyCompleteMission(nMissionID, nTargetID, bCheck);
            Mission953(nMissionID, _mission953Item);
        }			
		else
		{
			string cmd = "ReplyCompleteMission@" + nMissionID.ToString() + "@" + nTargetID.ToString() + "@" + bCheck.ToString() + "@";
			mainPlayer._commandCache.Add(cmd);
		}
		
	}


	void RPC_S2C_FailMission (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nMissionID;
		stream.TryRead<int> (out nMissionID);
		if(MissionManager.Instance.m_bHadInitMission)
			FailMission(nMissionID);
		else
		{
			string cmd = "FailMission@" + nMissionID.ToString() + "@" ;
			mainPlayer._commandCache.Add(cmd);
		}

	}
	//同步任务
	void RPC_S2C_SyncMissions (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] pmData = stream.Read<byte[]> ();
		byte[] adrmData = stream.Read<byte[]> ();
        byte[] tmData = stream.Read<byte[]>();
        byte[] tmadrmData = stream.Read<byte[]>();

        if (PeGameMgr.IsMultiStory)
        {
            if(adrmData != null)
                RMRepository.Import(adrmData);
            if(tmadrmData != null)
                RMRepository.Import(tmadrmData);
        }
        else
        {
            if(adrmData != null)
                AdRMRepository.Import(adrmData);
            if(tmadrmData != null)
                AdRMRepository.Import(tmadrmData);
        }
		StartCoroutine (WaitForMissionModule (pmData,tmData));
	}

	void RPC_S2C_CreateKillMonsterPos (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos;
		float radius;
		int[] idlist;
		int[] numlist;

		stream.TryRead<Vector3> (out pos);
		stream.TryRead<float> (out radius);
		stream.TryRead<int[]> (out idlist);
		stream.TryRead<int[]> (out numlist);

		List<Vector3> vcPosList = new List<Vector3> ();
		//Vector3 cpos;
		for (int i = 0; i < numlist.Length; i++) {
			for (int j = 0; j < numlist[i]; j++) {
				//cpos = SPMission.CreatePos(pos, radius, idlist[i]);
				//vcPosList.Add(cpos);
			}
		}

		Vector3[] poss = vcPosList.ToArray ();

		if (null != PlayerNetwork.mainPlayer)
			PlayerNetwork.mainPlayer.RPCServer (EPacketType.PT_InGame_MissionMonsterPos, poss, idlist, numlist);
	}

	void RPC_S2C_CreateFollowPos (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        float x, y;
        int targetid,missionId;
		stream.TryRead<float> (out x);
		stream.TryRead<float> (out y);
		stream.TryRead<int> (out targetid);
        stream.TryRead<int>(out missionId);

        Vector3 pos = new Vector3(x, VFDataRTGen.GetPosTop(new IntVector2((int)x, (int)y)), y);

		if (null != PlayerNetwork.mainPlayer)
			PlayerNetwork.mainPlayer.RPCServer (EPacketType.PT_InGame_MissionFollowPos, pos, targetid);

		TypeFollowData data = MissionManager.GetTypeFollowData (targetid);
		if (data == null)
			return;

		data.m_DistPos = pos;
        data.m_DistRadius = data.m_AdDistPos.radius2;
        if (data.m_AdNpcRadius.num > 0)
            data.m_LookNameID = StroyManager.Instance.CreateMissionRandomNpc(data.m_DistPos, data.m_AdNpcRadius.num);
        if(IsOwner)
        {
            if (data.m_AdDistPos.refertoType == ReferToType.Transcript)
                RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)data.m_DistPos.x, (int)data.m_DistPos.z), data.m_AdDistPos.referToID);

            for (int i = 0; i < data.m_CreateNpcList.Count; i++)
            {
                Vector3 createpos = StroyManager.Instance.GetPatrolPoint(data.m_DistPos, 3, 8, false);
                EntityCreateMgr.Instance.CreateRandomNpc(data.m_CreateNpcList[i], createpos);
            }
            MissionManager.Instance.m_PlayerMission.ProcessFollowMission(missionId, targetid);
        }
        UpdateMissionMapLabelPos(missionId, targetid,pos);


    }

    

	void RPC_S2C_CreateDiscoveryPos (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        float x = stream.Read<float>();
        float y = stream.Read<float>();
        int targetid = stream.Read<int>();
        int missionId = stream.Read<int>();

        TypeSearchData data = MissionManager.GetTypeSearchData (targetid);
		if (data == null)
			return;

        data.m_DistPos = new Vector3(x, VFDataRTGen.GetPosHeight(x, y), y);
        data.m_DistRadius = data.m_mr.radius2;
        if ( IsOwner)
        {
            if (data.m_mr.refertoType == ReferToType.Transcript)
                RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)data.m_DistPos.x, (int)data.m_DistPos.z), data.m_mr.referToID);

            if (MissionManager.Instance.m_bHadInitMission)
            {
                for (int i = 0; i < data.m_CreateNpcList.Count; i++)
                {
                    Vector3 createpos = StroyManager.Instance.GetPatrolPoint(data.m_DistPos, 3, 8, false);
                    EntityCreateMgr.Instance.CreateRandomNpc(data.m_CreateNpcList[i], createpos);
                }
            }
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionDiscoveryPos, data.m_DistPos, targetid);
        }
        UpdateMissionMapLabelPos(missionId, targetid, data.m_DistPos);
    }

	void RPC_S2C_SyncUseItemPos (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int targetid = stream.Read<int>();
        Vector3 pos = stream.Read<Vector3>();
        int missionId = stream.Read<int>();

		TypeUseItemData data = MissionManager.GetTypeUseItemData (targetid);
		if (data == null)
			return;

		data.m_Pos = pos;
        data.m_Radius = data.m_AdDistPos.radius2;
        UpdateMissionMapLabelPos(missionId, targetid, pos);
    }

	void RPC_S2C_AddNpcToColony (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		//--to do:
		int id = stream.Read<int> ();
		int teamNum = stream.Read<int> ();
		int dweelingId = stream.Read<int> ();
 
		MultiColonyManager.Instance.AddNpcToColony (id, teamNum, dweelingId);
	}

	void RPC_S2C_MissionKillMonster (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int missionId = stream.Read<int> ();
		int targetId = stream.Read<int> ();

		SceneEntityCreator.self.AddMissionPoint (missionId, targetId);
	}

    void RPC_S2C_SetMission(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int missionId = stream.Read<int>();
        MissionManager.Instance.m_PlayerMission.SetMission(missionId);
    }

    void RPC_S2C_SetCollectItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int _targetId = stream.Read<int>();
        int _itemId = stream.Read<int>();
        int _itemNum = stream.Read<int>();
     
        TypeCollectData col = MissionManager.GetTypeCollectData(_targetId);
        if (col != null)
            col.muiSetItemActive(_itemId,_itemNum);
    }
    public static List<int> _storyPlot = new List<int>();
    void RPC_S2C_EntityReach(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int plotId = stream.Read<int>();
        int playerId = stream.Read<int>();
        int id = plotId % 10000;
        if (PlayerNetwork.mainPlayerId == playerId || _storyPlot.Contains(id) || (id != 446  && id != 449 && id != 476 && id != 477 && id != 479 && id != 480 && id != 416))
            return;
        StroyManager.Instance.GetMissionOrPlotById(plotId); 
    }
    void RPC_S2C_RequestAdMissionData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        Vector3 pos = stream.Read<Vector3>();
        int id = stream.Read<int>();
        RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)pos.x, (int)pos.z), id);
    }

    

    void RPC_S2C_Mission953(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        _mission953Item = stream.Read<int>();
    }
    void RPC_S2C_LanguegeSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        MissionManager.Instance.m_PlayerMission.LanguegeSkill = stream.Read<int>();
    }
    void RPC_S2C_MonsterBook(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        bool ownerData = stream.Read<bool>();
        if(ownerData)
        {
            MonsterHandbookData.Deserialize(stream.Read<byte[]>());            
        }
        else
        {
            MonsterHandbookData.AddMhByKilledMonsterID(stream.Read<int>());
        }
        
    }
    
    #endregion
}


