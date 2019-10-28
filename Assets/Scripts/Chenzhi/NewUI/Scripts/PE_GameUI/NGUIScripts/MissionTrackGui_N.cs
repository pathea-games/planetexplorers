/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AiAsset;
using System;
using Pathea;


public class MissionTrackGui_N : GUIWindowBase
{
	public MissionTrackItem_N	mPrefab;
	public UITable				mTable;
	
	Dictionary<int,MissionTrackItem_N>	mMissionTracks = new Dictionary<int, MissionTrackItem_N>();
	
	public Dictionary<int,MissionTrackItem_N> MissionTracks {get{return mMissionTracks;}}
	
	List<int> mRemoveList = new List<int>();
	public List<int> RemoveList{get{return mRemoveList;}}
	
	
//	public MissionMainGui_N		mMissionMainGui;
	
//	int 	mMissionTrackMaxNum = 5;
	Player m_Player;
	int     mMonsterLeft = 0;



    public void SetCurPlayer(Player player)
    {
        m_Player = player;
    }
	
	public void	InitRemoveList(List<int> removeList)
	{
		foreach(int id in removeList)
			mRemoveList.Add(id);
	}

    public void SetMonsterLeft(int MissionID, int leftNum)
    {
        mMonsterLeft = leftNum;
        UpdataMissionInfo(MissionID);
    }
	
	public void UpdataMissionInfo(int id,bool Remove = false)
	{
        if (m_Player == null || m_Player.m_PlayerMission.m_MissionInfo == null || 1 == m_Player.m_PlayerMission.netWorkSyncState)
			return;

		if(id == MissionManager.m_SpecialMissionID5)
            return ;
		
		if(Remove)
		{
			OnRemoveTrack(id);
			if(RemoveList.Contains(id))
				RemoveList.Remove(id);
			return;
		}

		if(RemoveList.Contains(id))
			return;

        MissionCommonData data = MissionManager.GetMissionCommonData(id);

        if (data == null)
            return;

        if (data.IsTalkMission())
            return;

        Debug.Log("com1");
        //现在改成多目标
        for(int i=0; i<data.m_TargetIDList.Count; i++)
        {
            if(!mMissionTracks.ContainsKey(id))
                mMissionTracks[id] = CreateMissiontrack(id);

            UpdateMissionTrack(data, mMissionTracks[id]);
        }
		mTable.Reposition();
		Invoke("ReposTable", 0.05f);
	}
	
	public override void AwakeWindow ()
	{
		base.AwakeWindow ();
		UIMinMapCtrl.Instance.OpenMissionTrack();
		mTable.Reposition();
		Invoke("ReposTable", 0.05f);
	}
	
	public override bool HideWindow ()
	{
		UIMinMapCtrl.Instance.CloseMissionTrack();
		return base.HideWindow ();
	}
	
	public MissionTrackItem_N CreateMissiontrack(int id)
	{
		MissionTrackItem_N AddItem = Instantiate(mPrefab) as MissionTrackItem_N;
		AddItem.Init(id,this);
		AddItem.transform.parent = mTable.transform;
		AddItem.transform.localPosition = Vector3.zero;
		AddItem.transform.localRotation = Quaternion.identity;
		AddItem.transform.localScale = Vector3.one;
		return AddItem;
	}
	
	void UpdateMissionTrack(MissionCommonData data, MissionTrackItem_N missionTrack)
	{	
        if(data == null)
            return ;

        string monstername = "\"monsterid%\"";
        string monsternum = "\"monsternum%\"";
        string killmonname = "\"killedmosternum%\"";

        string pos = "\"position%\"";
        string npc1 = "\"npcid1%\"";
        string npc2 = "\"npcid2%\"";
        string npc3 = "\"npcid3%\"";

        string itemname = "\"itemid%\"";
        string itemnum = "\"itemnum%\"";
        string targetitem = "\"targetitemid%\"";

        string givemisnpc = "\"givenpcid%\"";
        string receivenpc = "\"receivenpcid%\"";
        string giveitemname = "\"giveitemid%\"";
        string giveitemnum = "\"giveitemnum%\"";

        string ritemnum = "\"n-ri%\"";
        string ritemname = "\"ri%\"";
        string playername = "\"name%\"";

        int num;
		
        TargetType curType;
        string missionContent = "";
        string name = "";
        string desc = "";
        AiNpcObject npc;
        int outinfo;

        if (data.m_Type == MissionType.MissionType_Mul)
        {
            missionContent = "[00BfFF]";
            missionContent += data.m_MulDesc + "[-]\n";
        }
        else
        {
            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType == TargetType.TargetType_Unkown)
                    continue;

                if (curType == TargetType.TargetType_Follow)
                {
                    TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
                    if (folData == null)
                        continue;

                    Vector3 tarPos;
                    if (folData.m_DistPos != Vector3.zero)
                        missionTrack.SetMissionPos(folData.m_DistPos, folData.m_DistRadius);
                    else if (folData.m_LookNameID != 0)
                    {
                        tarPos = StroyManager.Instance.GetNpcPos(folData.m_LookNameID);
                        if (tarPos != Vector3.zero)
                            missionTrack.SetMissionPos(tarPos, folData.m_DistRadius);
                    }
                    //else if (data.m_ReplyNpc != "0")
                    //{
                    //    tarPos = NpcManager.Instance.GetNpcPos(data.m_ReplyNpc);
                    //    if (tarPos != Vector3.zero)
                    //        missionTrack.SetMissionPos(tarPos, folData.m_DistRadius);
                    //}
                    else if (folData.m_BuildID > 0)
                    {
                        tarPos = VABuildingManager.Instance.GetMissionBuildingPos();
                        if (tarPos != Vector3.zero)
                            missionTrack.SetMissionPos(tarPos, folData.m_DistRadius);
                    }

                    if (m_Player.m_PlayerMission.HadCompleteTarget(data.m_TargetIDList[i]))
                        missionContent += "[EEFF11]";
                    else
                        missionContent += "[00BfFF]";

                    missionContent += folData.m_Desc + "[-]\n";

                    //for (int m = 0; m < folData.m_NpcList.Count; m++)
                    //{
                    //    if (int.TryParse(folData.m_NpcList[m], out outinfo))
                    //        npc = NpcManager.Instance.GetNpcRandom(outinfo);
                    //    else
                    //        npc = NpcManager.Instance.GetNpc(folData.m_NpcList[m]);

                    //    if (npc != null)
                    //    {
                    //        if (m == 0 && missionContent.Contains(npc1))
                    //            missionContent = missionContent.Replace(npc1, folData.m_NpcList[m]);
                    //        else if (m == 1 && missionContent.Contains(npc2))
                    //            missionContent = missionContent.Replace(npc2, folData.m_NpcList[m]);
                    //        else if (m == 2 && missionContent.Contains(npc3))
                    //            missionContent = missionContent.Replace(npc3, folData.m_NpcList[m]);
                    //    }
                    //}

                    if (missionContent.Contains(pos))
                        missionContent = missionContent.Replace(pos, folData.m_DistPos.ToString());
                }
                else if (curType == TargetType.TargetType_Discovery)
                {
                    TypeSearchData seaData = MissionManager.GetTypeSearchData(data.m_TargetIDList[i]);
                    if (seaData == null)
                        continue;

                    if (seaData.m_DistPos != Vector3.zero)
                        missionTrack.SetMissionPos(seaData.m_DistPos, seaData.m_DistRadius);
                    //else if (seaData.m_NpcName != "0")
                    //{
                    //    Vector3 tarPos = NpcManager.Instance.GetNpcPos(seaData.m_NpcName);
                    //    if (tarPos != Vector3.zero)
                    //        missionTrack.SetMissionPos(tarPos, seaData.m_DistRadius);
                    //}

                    if (m_Player.m_PlayerMission.HadCompleteTarget(data.m_TargetIDList[i]))
                        missionContent += "[EEFF11]";
                    else
                        missionContent += "[00BfFF]";

                    missionContent += seaData.m_Desc + "[-]\n";

                    if (missionContent.Contains(pos))
                        missionContent = missionContent.Replace(pos, seaData.m_DistPos.ToString());

                    //if (missionContent.Contains(npc1))
                    //    missionContent = missionContent.Replace(npc1, seaData.m_NpcName);

                }
                else if (curType == TargetType.TargetType_Collect)
                {
                    TypeCollectData colData = MissionManager.GetTypeCollectData(data.m_TargetIDList[i]);
                    if (colData == null)
                        continue;

                    //if (data.m_ReplyNpc != "0")
                    //{
                    //    if (int.TryParse(data.m_ReplyNpc, out outinfo))
                    //        npc = NpcManager.Instance.GetNpcRandom(outinfo);
                    //    else
                    //        npc = NpcManager.Instance.GetNpc(data.m_ReplyNpc);

                    //    if (npc != null)
                    //        missionTrack.SetMissionPos(npc.transform.position, 5);
                    //}

                    name = ItemAsset.ItemData.GetName(colData.m_ItemID);

                    num = m_Player.GetItemNum(colData.m_ItemID);

                    int type = m_Player.m_PlayerMission.IsSpecialID(colData.m_ItemID);
                    if (type > 0)
                        num = m_Player.m_PlayerMission.GetCollectSpecialItem(type, colData.m_ItemID);

                    if (num >= colData.m_ItemNum)
                        missionContent += "[EEFF11]";
                    else
                        missionContent += "[00BfFF]";

                    desc = colData.m_Desc != "0" ? colData.m_Desc : "";
                    missionContent += desc + "( " + num.ToString() + "/" + colData.m_ItemNum.ToString() + ")[-]\n";

                    if (missionContent.Contains(itemname))
                        missionContent = missionContent.Replace(itemname, ItemAsset.ItemData.GetName(colData.m_ItemID));

                    if (missionContent.Contains(itemnum))
                        missionContent = missionContent.Replace(itemnum, colData.m_ItemNum.ToString());

                    if (missionContent.Contains(targetitem))
                        missionContent = missionContent.Replace(targetitem, colData.m_TargetItemID.ToString());
                }
                else if (curType == TargetType.TargetType_KillMonster)
                {
                    TypeMonsterData monData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[i]);
                    if (monData == null)
                        continue;

                    string tmp = "";
                    for (int m = 0; m < monData.m_MonsterList.Count; m++)
                    {
                        tmp = "";
                        name = AiDataBlock.GetAIDataName(monData.m_MonsterList[m].id);
                        num = m_Player.m_PlayerMission.GetQuestVariable(data.m_ID, "monster", monData.m_MonsterList[m].id);
                        if (num >= monData.m_MonsterList[m].num)
                        {
                            num = monData.m_MonsterList[m].num;
                            tmp += "[EEFF11]";
                        }
                        else
                            tmp += "[00BfFF]";

                        //tmp += name;
                        tmp += monData.m_Desc != "0" ? monData.m_Desc : "";
                        tmp += "( " + num.ToString() + "/" + monData.m_MonsterList[m].num.ToString() + ")[-]\n";

                        if (tmp.Contains(monstername))
                            tmp = tmp.Replace(monstername, name);

                        if (tmp.Contains(monsternum))
                            tmp = tmp.Replace(monsternum, monData.m_MonsterList[m].num.ToString());

                        missionContent += tmp;
                    }
                }
                else if (curType == TargetType.TargetType_UseItem)
                {
                    TypeUseItemData useData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[i]);
                    if (useData == null)
                        continue;

                    name = ItemAsset.ItemData.GetName(useData.m_ItemID);
                    num = m_Player.m_PlayerMission.GetQuestVariable(data.m_ID, "item", useData.m_ItemID);

                    if (num >= useData.m_UseNum && useData.m_TargetID != 5004)   //埋骨灰盒，暂时特殊处理
                        missionContent += "[EEFF11]";
                    else
                        missionContent += "[00BfFF]";

                    desc = useData.m_Desc != "0" ? useData.m_Desc : "";
                    missionContent += desc + "( " + num.ToString() + "/" + useData.m_UseNum.ToString() + ")[-]\n";

                    if (useData.m_Pos != Vector3.zero)
                        missionTrack.SetMissionPos(useData.m_Pos, useData.m_Radius);

                    if (missionContent.Contains(pos))
                        missionContent = missionContent.Replace(pos, useData.m_Pos.ToString());

                    if (missionContent.Contains(itemname))
                        missionContent = missionContent.Replace(itemname, ItemAsset.ItemData.GetName(useData.m_ItemID));

                    if (missionContent.Contains(itemnum))
                        missionContent = missionContent.Replace(itemnum, useData.m_UseNum.ToString());
                }
                else if (curType == TargetType.TargetType_Messenger)
                {
                    TypeMessengerData mesData = MissionManager.GetTypeMessengerData(data.m_TargetIDList[i]);
                    if (mesData == null)
                        continue;

                    desc = mesData.m_Desc != "0" ? mesData.m_Desc : "";
                    missionContent += desc;

                    //if (missionContent.Contains(givemisnpc))
                    //    missionContent = missionContent.Replace(givemisnpc, mesData.m_Npc);

                    //if (missionContent.Contains(receivenpc))
                    //    missionContent = missionContent.Replace(receivenpc, mesData.m_ReplyNpc);

                    if (missionContent.Contains(itemname))
                        missionContent = missionContent.Replace(itemname, ItemAsset.ItemData.GetName(mesData.m_ItemID));

                    if (missionContent.Contains(itemnum))
                        missionContent = missionContent.Replace(itemnum, mesData.m_ItemNum.ToString());
                }
                else if (curType == TargetType.TargetType_TowerDif)
                {
                    TypeTowerDefendsData towData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[i]);
                    if (towData == null)
                        continue;

                    missionContent += "[00BfFF]";
                    desc = towData.m_Desc != "0" ? towData.m_Desc : "";
                    missionContent += desc + "( " + mMonsterLeft.ToString() + "/" + towData.m_Count.ToString() + ")[-]\n";
                }
            }
        }

        missionTrack.RestTrack(data.m_MissionName,missionContent);

		GameUI.Instance.mUIMinMapCtrl.ChangeMissionTrackState(data,true);

        UpdateMapGui(data, true);
	}

    void UpdateMapGui(MissionCommonData data, bool open)
    {
        if (VFVoxelTerrain.RandomMap)
            GameUI.Instance.mLimitWorldMapGui.ChangeMissionTrackState(data, open);
        else
            GameUI.Instance.mWorldMapGui.ChangeMissionTrackState(data, open);
    }

	public void ChangeMissionTrackState(MissionCommonData missionData, bool open )
	{
		GameUI.Instance.mUIMinMapCtrl.ChangeMissionTrackState(missionData,open);

        UpdateMapGui(missionData, open);

		if(open)
		{
			if(mRemoveList.Contains(missionData.m_ID))
				mRemoveList.Remove(missionData.m_ID);

			if(!mMissionTracks.ContainsKey(missionData.m_ID))
			{
				mMissionTracks[missionData.m_ID] = CreateMissiontrack(missionData.m_ID);
				UpdateMissionTrack(missionData, mMissionTracks[missionData.m_ID]);
			}
		}
		else if(!open)
			OnRemoveTrack(missionData.m_ID,true);
			
	}
	
	public void OnRemoveTrack(int id,bool fromMissionMain = false)
	{
		if(!mRemoveList.Contains(id))
			mRemoveList.Add(id);

        MissionCommonData data = MissionManager.GetMissionCommonData(id);

        if (data == null)
            return ;
		
		GameUI.Instance.mUIMinMapCtrl.ChangeMissionTrackState(data,false);

        UpdateMapGui(data, false);

        for(int i=0; i<data.m_TargetIDList.Count; i++)
        {
		    if(mMissionTracks.ContainsKey(id))
		    {
				mMissionTracks[id].transform.parent = null;
			    Destroy(mMissionTracks[id].gameObject);
			    mMissionTracks.Remove(id);
		    }
        }

		if(!fromMissionMain)
			mMissionMainGui.OnMissionTrackClose(id);
		mTable.Reposition();
		Invoke("ReposTable", 0.05f);
	}
	
	public void OnMissionSel(int id)
	{
		mMissionMainGui.OnMutexBtnClick(id);
	}
	
	void ReposTable()
	{
		mTable.Reposition();
	}
}
*/
