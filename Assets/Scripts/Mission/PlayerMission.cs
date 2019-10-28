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

public class TowerInfoUIData
{
    int m_MissionID;
    float m_PreTime;
    float m_RemainTime = -1.0f;
    int m_CurCount;
    int m_MaxCount;
    bool m_bRefurbish = false;
    int m_CurWavesRemaining;      //lz-2016.07.27 当前剩余波次
    int m_TotalWaves;             //lz-2016.07.27 总波次

    public int MissionID
    {
        get { return m_MissionID; }
        set { m_MissionID = value; }
    }

    public float PreTime
    {
        get { return m_PreTime; }
        set 
        { 
            m_PreTime = value;
			UITowerInfo.Instance.Show();
            UITowerInfo.Instance.SetPrepTime(m_PreTime);
        }
    }

    public float RemainTime
    {
        get { return m_RemainTime; }
        set { m_RemainTime = value;}
    }

    public int MaxCount
    {
        get { return m_MaxCount; }
        set { m_MaxCount = value; }
    }

	public int CurCount
	{
		get { return m_CurCount; }
		set { m_CurCount = value; }
	}

    public bool bRefurbish
    {
        get { return m_bRefurbish; }
        set { m_bRefurbish = value; }
    }

    //lz-2016.07.27 当前剩余波次
    public int CurWavesRemaining
    {
        get{ return m_CurWavesRemaining;}
        set{ m_CurWavesRemaining=value; }
    }

    //lz-2016.07.27 总波次
    public int TotalWaves
    {
        get { return m_TotalWaves; }
        set { m_TotalWaves = value; }
    }

    public int curCount()
    {
		if (!m_bRefurbish || !EntityCreateMgr.DbgUseLegacyCode)
            return m_CurCount;

        m_bRefurbish = false;
        m_CurCount = MissionManager.Instance.GetTowerDefineKillNum(m_MissionID);
        return m_CurCount;
    }
    
    public void Cleanup()
    {
        m_MissionID = 0;
        m_PreTime = 0;
        m_CurCount = 0;
        m_MaxCount = 0;
        m_bRefurbish = false;
    }
}

public partial class PlayerMission
{
	#region missionFlagdef
	public static string MissionFlagItem = "ITEM";
	public static string MissionFlagStep = "STEP";
	public static string MissionFlagMonster = "MONSTER";
	public static string MissionFlagTDMonster = "TDMONS";
	#endregion
    public string m_FollowPlayerName;
    public enum MissionInfo
    {
        MAX_MISSIONFLAG_LENGTH = 10,    //任务标记类型长度
        MAX_MISSIONVALUE_LENGTH = 18,   //任务标记值长度
        MAX_MISSION_COUNT = 20000,        //任务总数
    };

    public Dictionary<int, Dictionary<string, string>> m_RecordMisInfo = new Dictionary<int, Dictionary<string, string>>();   //用于读取
    public List<Vector3> m_SpeVecList = new List<Vector3>();
    public List<int> m_GetRewards = new List<int>();
    public Dictionary<int, int> m_MissionTargetState = new Dictionary<int, int>();
    public Dictionary<int, int> m_MissionState = new Dictionary<int, int>();
    public Dictionary<int, Dictionary<string, string>> m_MissionInfo = new Dictionary<int, Dictionary<string, string>>();   //已接任务
    public Dictionary<int, double> m_MissionTime = new Dictionary<int, double>();
    public Dictionary<int, Vector3> m_iCurFollowStartPos = new Dictionary<int, Vector3>();
    public List<NpcCmpt> followers = new List<NpcCmpt>();                        //跟随任务当中跟随NPC
    public List<NpcCmpt> pathFollowers = new List<NpcCmpt>();


    public const int Version_0 = 0;
    public const int Version_1 = Version_0 + 1;
    public const int Version_2 = Version_1 + 1;
    public const int Version_3 = Version_2 + 1;
    public const int Version_4 = Version_3 + 1;
    public const int Version_5 = Version_4 + 1;
    public const int Version_6 = Version_5 + 1;
    public const int Version_7 = Version_6 + 1;
    public const int Version_8 = Version_7 + 1;
    public const int Version_9 = Version_8 + 1;
    public const int Version_10 = Version_9 + 1;
    const int CurrentVersion = Version_10;
    int languegeSkill;
    public Dictionary<int, string> recordNpcName = new Dictionary<int, string>();
    public int LanguegeSkill
    {
        get { return languegeSkill; }
        set { languegeSkill = value; }
    }

    public TowerInfoUIData m_TowerUIData = new TowerInfoUIData();

    public bool HasGetRewards(int MissionID)
    {
        return m_GetRewards.Contains(MissionID);
    }

    int AddMissionFlagType(int MissionID, string MissionFlag, string MissionValue)
    {
        if (MissionFlag.Length > (int)MissionInfo.MAX_MISSIONFLAG_LENGTH)
            return -1;

        if (MissionValue.Length > (int)MissionInfo.MAX_MISSIONVALUE_LENGTH)
            return -1;

        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        MissionFlag.ToUpper();

        if (missionFlagType != null)
        {
            ModifyQuestVariable(MissionID, MissionFlag, MissionValue);

            return 0;
        }
        else
        {
            missionFlagType = new Dictionary<string, string>();
            missionFlagType.Add(MissionFlag, MissionValue);
            m_MissionInfo.Add(MissionID, missionFlagType);
            //GameGui_N.Instance.mChatGUI.AddChat(PromptData.PromptType.PromptType_1, data.m_MissionName);

            
            //if (GameConfig.GameModeNoMask == GameConfig.EGameMode.Singleplayer_Story)
            //{
            //    List<int> storyid = data.HasStory(Story_Info.Story_Info_Get);
            //    StroyManager.Instance.PushStoryList(storyid);
            //}

            return 1;
        }
    }


    //查询当前是否有该任务
    public bool ConTainsMission(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        return m_MissionInfo.ContainsKey(MissionID);
    }

    //查询已接任务
    public Dictionary<string, string> GetMissionFlagType(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return null;

        return m_MissionInfo.ContainsKey(MissionID) ? m_MissionInfo[MissionID] : null;
    }

    //查询已接任务标记类型
    public bool HasQuestVariable(int MissionID, string MissionFlag)
    {
        if (m_MissionInfo.Count == 0)
            return false;

        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        MissionFlag.ToUpper();

        if (missionFlagType != null)
        {
            return missionFlagType.ContainsKey(MissionFlag);
        }

        return false;
    }

    //获取已接任务标记值
    public string GetQuestVariable(int MissionID, string MissionFlag)
    {
        
        if (m_MissionInfo.Count == 0)
            return "0";

        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return "0";

        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        MissionFlag = MissionFlag.ToUpper();

        if (missionFlagType != null)
        {
            if (missionFlagType.ContainsKey(MissionFlag))
                return missionFlagType[MissionFlag];
        }

        return "0";
    }

    public int GetQuestVariable(int MissionID,  int id)
    {
        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);

        if (missionFlagType == null)
            return 0;

        int result = 0;
        foreach(KeyValuePair<string, string> ite in missionFlagType)
        {
            if (ite.Value == "0")
                continue;

            string[] value = ite.Value.Split('_');
            
            if (value.Length != 2)
                continue;

            if (value[0] != id.ToString())
                continue;

            int num = Convert.ToInt32(value[1]);
            result = num > result ? num : result;
        }

        return result;
    }

    //删除已接任务标记类型
    public bool DelQuestVariable(int MissionID, string MissionFlag)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        //如果该任务已经完成
        if (HadCompleteMission(MissionID))
            return false;

        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        MissionFlag.ToUpper();

        if (missionFlagType != null)
        {
            if (missionFlagType.ContainsKey(MissionFlag))
                missionFlagType.Remove(MissionFlag);
        }

        return true;
    }

    public bool ModifyQuestVariable(int MissionID, string MissionFlag, string MissionValue)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        if (MissionFlag.Length > (int)MissionInfo.MAX_MISSIONFLAG_LENGTH)
            return false;

        if (MissionValue.Length > (int)MissionInfo.MAX_MISSIONVALUE_LENGTH)
            return false;

        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        if (missionFlagType == null)
            return false;

        MissionFlag.ToUpper();
        missionFlagType[MissionFlag] = MissionValue;

        string[] flagvalue = MissionValue.Split('_');
        if (flagvalue.Length < 2)
            return false;
        string n = MissionFlag.Substring(4, 1);
        int targetID = MissionManager.GetMissionCommonData(MissionID).m_TargetIDList.Find(delegate(int ite)
        {
            TypeMonsterData mkData = MissionManager.GetTypeMonsterData(ite);
            if (mkData == null)
                return false;
            foreach (var item in mkData.m_MonsterList)
            {
                if (item.npcs.Contains(Convert.ToInt32(flagvalue[0])))
                    return true;
            }
            return false;
        });
        if (MissionFlag.Substring(0, 4) == MissionFlagItem)
            MissionManager.Instance.UpdateUseMissionTrack(MissionID, Convert.ToInt32(n), Convert.ToInt32(flagvalue[1]));
        else
        {
            if(targetID == 0)
                MissionManager.Instance.UpdateMissionTrack(MissionID, Convert.ToInt32(flagvalue[1]));
            else
                MissionManager.Instance.UpdateMissionTrack(MissionID, Convert.ToInt32(flagvalue[1]),targetID);
        }
        UpdateAllNpcMisTex();
        return true;
    }

    public List<string> recordCreationName = new List<string>();
    public List<Vector3> recordCretionPos = new List<Vector3>();
    public bool isRecordCreation = false;
    public bool ModifyQuestVariable(int MissionID, string MissionFlag, int ItemID, int num)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        if (missionFlagType == null)
            return false;

        MissionFlag.ToUpper();
        if (!missionFlagType.ContainsKey(MissionFlag))
            return false;

        string MissionValue = missionFlagType[MissionFlag];


        string[] flagvalue = MissionValue.Split('_');
        if (flagvalue.Length < 2)
            return false;

        int itemid = Convert.ToInt32(flagvalue[0]);

        if (itemid != ItemID)
            return false;

        if (MissionID == MissionManager.m_SpecialMissionID89)
            isRecordCreation = true;

        int itemnum = Convert.ToInt32(flagvalue[1]) + num;

		string value = flagvalue[0] + "_" + itemnum.ToString();

        ModifyQuestVariable(MissionID, MissionFlag, value);
        

        return true;
    }

    //是否已接该任务
    public bool HasMission(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        if (m_MissionInfo.ContainsKey(MissionID))
            return true;

        return false;
    }

    //已接任务当中是否有此targetID
    public bool HasTarget(int targetID) 
    {
        MissionCommonData data;
        bool result = false;
        foreach (var item in m_MissionInfo.Keys)
        {
            data = MissionManager.GetMissionCommonData(item);
            if (data == null)
                continue;
            if (data.m_TargetIDList.Contains(targetID)) 
            {
                result = true;
                break;
            }
        }
        return result;
    }

    public List<int> GetCollectMissionListByID(int ItemID)
    {
        MissionCommonData data;
        List<int> misList = new List<int>();
        foreach (int ite in m_MissionInfo.Keys)
        {
            data = MissionManager.GetMissionCommonData(ite);
            if (data == null)
                continue;

            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType == TargetType.TargetType_Collect)
                {
                    TypeCollectData colData = MissionManager.GetTypeCollectData(data.m_TargetIDList[i]);
                    if (colData == null)
                        continue;

                    if (colData.ItemID == ItemID)
                    {
                        misList.Add(ite);
                        continue;
                    }

                    if (colData.m_TargetItemID == ItemID)
                    {
                        misList.Add(ite);
                        continue;
                    }
                }
            }
        }

        return misList;
    }

    public void ProcessUseItemByID(int ItemID, Vector3 pos ,int addOrSubstract = 1,ItemObject itemobj = null)
    {
        if (ItemID == 1339)
            KillNPC.ashBox_inScene++;
		if (ItemID == 1541) 
			MissionManager.Instance.m_PlayerMission.LanguegeSkill += 1;
        MissionCommonData data;
//        List<int> misList = new List<int>();
        float dist;
        Vector2 v1, v2;
        List<int> curMissions = new List<int>(m_MissionInfo.Keys);
        isRecordCreation = false;
        foreach (int item in curMissions)
        {
            if (item == 242 || item == 629)
                continue;
            data = MissionManager.GetMissionCommonData(item);
            if (data == null)
                continue;

            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType == TargetType.TargetType_UseItem)
                {
                    TypeUseItemData useData = MissionManager.GetTypeUseItemData(data.m_TargetIDList[i]);
                    if (useData == null)
                        continue;

                    if (useData.m_Type == 1)
                    {
                        if (PeGameMgr.IsStory)
                        {
                            if (item == MissionManager.m_SpecialMissionID65)
                            {
                                GameObject gomc = GameObject.Find("satellite_receiver_base");
                                //MissionCylinder
                                if (gomc == null)
                                    continue;

                                dist = Vector3.Distance(pos, gomc.transform.position);
                            }
                            else 
                            {
                                if (useData.m_Pos == new Vector3(-255, -255, -255))
                                {
                                    Vector3 csMain;
                                    if (CSMain.GetAssemblyPos(out csMain))
                                        dist = Vector3.Distance(pos, csMain);
                                    else
                                        continue;
                                }
                                else
                                    dist = Vector3.Distance(pos, useData.m_Pos);
                            }
                        }
                        else
                        {
                            v1 = new Vector2(pos.x, pos.z);
                            v2 = new Vector2(useData.m_Pos.x, useData.m_Pos.z);

                            dist = Vector2.Distance(v1, v2);
                        }

                        if (dist > useData.m_Radius)
                        {
                            //GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(item, 4, useData.m_TargetID);
                            //GameUI.Instance.mNPCTalk.PreShow(); 
                            continue;
                        }
                    }

                    string flag = MissionFlagItem + i;

                    ModifyQuestVariable(data.m_ID, flag, ItemID, addOrSubstract);
                    MissionManager.Instance.CompleteTarget(data.m_TargetIDList[i], data.m_ID);

                    if (item == MissionManager.m_SpecialMissionID93 && IsSpecialID(ItemID) == ECreation.SimpleObject)
                    {
                        CreationData creationData = CreationMgr.GetCreation(itemobj.instanceId);
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

                    //if (item == 550)
                    //{
                    //    Vector3 pos = Vector3.zero;
                    //    pos *= BuildingMan.Blocks.ScaleInverted;

                    //    BSVoxel voxel = BuildingMan.Blocks.Read(pos.x, pos.y, pos.z);
                    //    if (!BuildingMan.Blocks.VoxelIsZero(voxel,1))
                    //    {
                    //        BSBlockMatMap.s_MatToItem[voxel.materialType]
                    //    }
                    //}
                }
                else if (curType == TargetType.TargetType_Collect)
                {
                    TypeCollectData useData = MissionManager.GetTypeCollectData(data.m_TargetIDList[i]);
                    if (useData.ItemID == ItemID)
                        MissionManager.Instance.UpdateMissionTrack(item);
                }
            }
        }
    }

    public bool HadCompleteTarget(int TargetID)
    {
        if (m_MissionTargetState.ContainsKey(TargetID))
            return true;

        return false;
    }

    //是否已经完成该任务
    public bool HadCompleteMission(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return false;

        if (data.m_MaxNum == -1)
            return false;

        if (m_MissionState.ContainsKey(MissionID))
        {
            if (m_MissionState[MissionID] >= data.m_MaxNum)
                return true;

            return false;
        }

        return false;
    }

    //log:lz-2016.07.12 是否完成过这个任务，不限制完成次数，任务邮件用，尹犇说只要完成一次任务就可以显示邮件了
    public bool HadCompleteMissionAnyNum(int MissionID)
    {
        if (m_MissionState.ContainsKey(MissionID))
        {
            return true;
        }
        return false;
    }

    //设置任务完成状况
    public bool SetMission(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return false;

        UIMissionMgr.Instance.DeleteGetableMission(MissionID);

        if (data.m_MaxNum == -1)
            return false;

        if (m_MissionState.ContainsKey(MissionID))
        {
            m_MissionState[MissionID]++;
        }
        else
            m_MissionState.Add(MissionID, 1);

        //lz-2016.07.13 通过完成任务的ID检测添加Message
        MessageData.AddMsgByCompletedMissionID(MissionID);
        MissionManager.Instance.CheckAllGetableMission();

        //lz-2016.07.19 完成383任务大地图上显示查找npc的范围
        if (PeGameMgr.IsStory&&MissionID == PeUIMap.UIStroyMap.ShowFindNpcRangeMissionID)
        { 
            if(null!=GameUI.Instance)
            {
                PeUIMap.UIStroyMap stroyMap=(PeUIMap.UIStroyMap)GameUI.Instance.mUIWorldMap.CurMap;
                stroyMap.ShowFindNpcRange = true;
            }
            
        }
        return true;
    }

    //删除已接任务
    public void DelMissionInfo(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data != null)
        {
            AiAdNpcNetwork npc = AiAdNpcNetwork.Get<AiAdNpcNetwork>(data.m_iNpc);
            if (null != npc)
                npc.InitForceData();
        }

        if (HasMission(MissionID))
        {
            m_MissionInfo.Remove(MissionID);
        }
    }

    //获取已接任务数量
    public int GetMissionInfoCount()
    {
        return m_MissionInfo.Count;
    }

    //是否能接取任务
    public bool IsGetTakeMission(int MissionID,bool isPreLimitOn = true)
    {
        if (MissionID <= 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        //已接取或已完成的不能接
        if (HasMission(MissionID) || HadCompleteMission(MissionID))
            return false;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return false;

        //前置任务
        bool bpass = false;
        if (isPreLimitOn && data.m_PreLimit.idlist.Count > 0)
        {
            if (data.m_PreLimit.type == 2)
            {
                bpass = true;
                for (int i = 0; i < data.m_PreLimit.idlist.Count; i++)
                {
                    if (data.m_PreLimit.idlist[i] > 999 && data.m_PreLimit.idlist[i] < 9000)
                    {
                        if(!HadCompleteTarget(data.m_PreLimit.idlist[i]))
                            return false;
                    }
                    else
                    {
                        if (!HadCompleteMission(data.m_PreLimit.idlist[i]))
                            return false;
                    }
                }
            }
            else
            {
                //type包括1或0
                for (int i = 0; i < data.m_PreLimit.idlist.Count; i++)
                {
                    if(data.m_PreLimit.idlist[i] > 999 && data.m_PreLimit.idlist[i] < 9000)
                    {
                        if (HadCompleteTarget(data.m_PreLimit.idlist[i]))
                        {
                            bpass = true;
                            break;
                        }
                    }
                    else
                    {
                        if (HadCompleteMission(data.m_PreLimit.idlist[i]))
                        {
                            bpass = true;
                            break;
                        }
                    }

                }
            }

            if (!bpass)
                return false;
        }

        bpass = true;
        //完成和接取后不能再接取
        if (data.m_AfterLimit.idlist.Count > 0)
        {
            for (int i = 0; i < data.m_AfterLimit.idlist.Count; i++)
            {
                //接取后不能再接取
                if (data.m_AfterLimit.type == 1)
                {
                    if (data.m_AfterLimit.idlist[i] > 999 && data.m_AfterLimit.idlist[i] < 9000)
                    {
                        if (HasTarget(data.m_AfterLimit.idlist[i]) || HadCompleteTarget(data.m_AfterLimit.idlist[i]))
                        {
                            bpass = false;
                            break;
                        }
                    }
                    else
                    {
                        if (HasMission(data.m_AfterLimit.idlist[i]) || HadCompleteMission(data.m_AfterLimit.idlist[i]))
                        {
                            bpass = false;
                            break;
                        }
                    }
                }
                else  //完成后不能再接取
                {
                    if (data.m_AfterLimit.idlist[i] > 999 && data.m_AfterLimit.idlist[i] < 9000) 
                    {
                        if (HadCompleteTarget(data.m_AfterLimit.idlist[i]))
                        {
                            bpass = false;
                            break;
                        }
                    }
                    else
                    {
                        if (HadCompleteMission(data.m_AfterLimit.idlist[i]))
                        {
                            bpass = false;
                            break;
                        }
                    }
                }
            }

            if (!bpass)
                return false;
        }

        //判断互斥任务
        bpass = true;
        if (data.m_MutexLimit.idlist.Count > 0)
        {
            if (data.m_MutexLimit.type == 1)
            {
                bpass = false;
                for (int i = 0; i < data.m_MutexLimit.idlist.Count; i++)
                {
                    if (data.m_MutexLimit.idlist[i] > 999 && data.m_MutexLimit.idlist[i] < 9000)
                    {
                        if (HadCompleteTarget(data.m_MutexLimit.idlist[i]) || HasTarget(data.m_MutexLimit.idlist[i]))
                            return false;
                    }
                    else
                    {
                        if (HadCompleteMission(data.m_MutexLimit.idlist[i]) || HasMission(data.m_MutexLimit.idlist[i]))
                            return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.m_MutexLimit.idlist.Count; i++)
                {
                    if (data.m_MutexLimit.idlist[i] > 999 && data.m_MutexLimit.idlist[i] < 9000) 
                    {
                        if (!HadCompleteTarget(data.m_MutexLimit.idlist[i]))
                        {
                            bpass = false;
                            break;
                        }
                    }
                    else
                    {
                        if (!HadCompleteMission(data.m_MutexLimit.idlist[i]))
                        {
                            bpass = false;
                            break;
                        }
                    }
                }
            }

            if (bpass)
                return false;
        }

        int itemcount = 0;
        int equipcount = 0;
        int resourcescount = 0;
        //领取时需求物品
        for (int i = 0; i < data.m_Get_DemandItem.Count; i++)
        {
            ECreation type = IsSpecialID(data.m_Get_DemandItem[i].id);
            if (type != ECreation.Null)
                itemcount = PeCreature.Instance.mainPlayer.GetCreationItemCount(type);
            else
                itemcount = PeCreature.Instance.mainPlayer.GetPkgItemCount(data.m_Get_DemandItem[i].id); 
            if (itemcount < data.m_Get_DemandItem[i].num)
                return false;
        }

        //领取时获得物品
        for (int i = 0; i < data.m_Get_MissionItem.Count; i++)
        {
            ItemProto itemdata = ItemProto.GetItemData(data.m_Get_MissionItem[i].id);

            if (itemdata == null)
                continue;

            if (itemdata.tabIndex == 0)
            {
                if (itemdata.maxStackNum > 0)
                    itemcount += ((data.m_Get_MissionItem[i].num - 1) / itemdata.maxStackNum) + 1;
            }
            if (itemdata.tabIndex == 1)
            {
                if (itemdata.maxStackNum > 0)
                    equipcount += ((data.m_Get_MissionItem[i].num - 1) / itemdata.maxStackNum) + 1;
            }
            else
            {
                if (itemdata.maxStackNum > 0)
                    resourcescount += ((data.m_Get_MissionItem[i].num - 1) / itemdata.maxStackNum) + 1;
            }
        }

		if(null == PeCreature.Instance.mainPlayer)
			return false;

        if (PeCreature.Instance.mainPlayer != null)
        {
            int playerID = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
            for (int i = 0; i < data.m_reputationPre.Count; i++)
            {
                int campID;
                if (data.m_reputationPre[i].campID == -1)
                {
                    if (GameUI.Instance.mNpcWnd.m_CurSelNpc == null)
                        continue;
                    campID = Mathf.RoundToInt(GameUI.Instance.mNpcWnd.m_CurSelNpc.GetAttribute(AttribType.DefaultPlayerID));
                }
                else
                    campID = data.m_reputationPre[i].campID;
                if (data.m_reputationPre[i].type == 1)
                {
                    if (!(data.m_reputationPre[i].min < ReputationSystem.Instance.GetReputationValue(playerID, campID)
                          && ReputationSystem.Instance.GetReputationValue(playerID, campID) <= data.m_reputationPre[i].max))
                        return false;
                }
                else
                {
                    if (data.m_reputationPre[i].min < ReputationSystem.Instance.GetReputationValue(playerID, campID)
                          && ReputationSystem.Instance.GetReputationValue(playerID, campID) <= data.m_reputationPre[i].max)
                        return false;
                }
            }
        }

        return true;
    }

    public void AbortFollowMission() 
    {
        int misID = -1;
        if (PeGameMgr.IsMulti)
        {
            misID = MissionManager.Instance.HasFollowMissionNet();
            if (misID == -1)
                return;
        }
        MissionCommonData data;
		List<int> result = new List<int> ();
        foreach (var item in m_MissionInfo.Keys)
        {
            data = MissionManager.GetMissionCommonData(item);
            if (data == null)
                continue;            
            int targetID = data.m_TargetIDList.Find(ite => MissionRepository.GetTargetType(ite) == TargetType.TargetType_Follow);
			if (targetID != 0)
				result.Add (item);
        }
		for (int i = 0; i < result.Count; i++) {
			AbortMission (result [i]);
		}
    }

    public void AbortMission(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return;

        if (!HasMission(MissionID))
            return;
		
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return;

		if(!EntityCreateMgr.DbgUseLegacyCode)
			SceneEntityCreator.self.RemoveMissionPoint(MissionID, -1);

		DelMissionInfo(MissionID);

        if (PeGameMgr.IsStory)
        {
            List<int> storyid = data.HasStory(Story_Info.Story_Info_Fail);
            StroyManager.Instance.PushStoryList(storyid);
        }
        else if (PeGameMgr.IsAdventure)
        {
            List<int> storyid = data.HasStory(Story_Info.Story_Info_Fail);
            StroyManager.Instance.PushAdStoryList(storyid, data.m_iNpc);
        }

        PeEntity obj;
        obj = EntityMgr.Instance.Get(data.m_iNpc);
        if (obj != null)
        {
            MissionManager.Instance.UpdateMissionMainGUI(MissionID);
            UIMissionMgr.GetableMisView gmv = new UIMissionMgr.GetableMisView(MissionID, data.m_MissionName, obj.ExtGetPos(), obj.Id);
            gmv.TargetNpcInfo.mName = obj.ExtGetName();
            gmv.TargetNpcInfo.mNpcIcoStr = obj.ExtGetFaceIcon();
            UIMissionMgr.Instance.AddGetableMission(gmv);
        }
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            if (curType == TargetType.TargetType_Follow)
            {
                TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
                if (folData == null)
                    continue;

                if (folData.m_LookNameID != 0) 
                {
                    PeEntity npc = EntityMgr.Instance.Get(folData.m_LookNameID);
                    if (npc != null && npc.IsRecruited())
                        npc.NpcCmpt.BaseNpcOutMission = false;
                }

                if (data.m_TargetIDList[i] == 3031)
                {
                    PeCreature.Instance.mainPlayer.passengerCmpt.onGetOnCarrier -= MissionJoyrideOn;
                    PeCreature.Instance.mainPlayer.passengerCmpt.onGetOffCarrier -= MissionJoyrideOff;
                }

				for (int m=0; m<folData.m_iNpcList.Count; m++)
				{
                    obj = EntityMgr.Instance.Get(folData.m_iNpcList[m]);

	                if(obj == null)
	                    continue;

                    NpcCmpt nc = obj.NpcCmpt;
                    if (nc != null)
                    {
                        nc.Battle = ENpcBattle.Defence;
                        nc.FixedPointPos = obj.position;
                        nc.CanTalk = true;
                    }

                    if (folData.m_EMode == 1)
					{
						followers.Remove(obj.NpcCmpt);
                         StroyManager.Instance.RemoveReq(obj, EReqType.FollowTarget);
					}
					else
						StroyManager.Instance.RemoveReq(obj, EReqType.MoveToPoint);

                    if (folData.m_PathList.Count > 0)
                        StroyManager.Instance.RemoveReq(obj, EReqType.FollowPath);

                    NpcMissionData missionData = obj.GetUserData() as NpcMissionData;
                    if (missionData != null)
                        missionData.mInFollowMission = false;
                    
                    obj.SetAttackMode(EAttackMode.Attack);
                    obj.SetInvincible(true);

	                if(MissionManager.HasRandomMission(MissionID))
	                    GoHome(obj);
				}
            }
            else if(curType == TargetType.TargetType_TowerDif)
            {
                //GameGui_N.Instance.mLifeShowGui.Activate(null, false);
                TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[i]);
                if (towerData == null)
                    continue;

                if(towerData.m_iNpcList.Count == 0)
                    continue;

                obj = EntityMgr.Instance.Get(towerData.m_iNpcList[0]);

                if(obj == null)
                    continue;

/*              need add
                SPTowerDefence st = obj.transform.GetComponentInChildren<SPTowerDefence>();
                if(st == null)
                    continue;

                MonoBehaviour.DestroyObject(st.gameObject);*/
                //GameGui_N.Instance.mMissionTrackGui.SetMonsterLeft(-1, 0);
            }
            else if (curType == TargetType.TargetType_Discovery)
            {
                TypeSearchData serchData = MissionManager.GetTypeSearchData(data.m_TargetIDList[i]);
                if (serchData == null)
                    continue;

                if (serchData.m_NpcID != 0)
                {
                    PeEntity npc = EntityMgr.Instance.Get(serchData.m_NpcID);
                    if (npc != null && npc.IsRecruited())
                        npc.NpcCmpt.BaseNpcOutMission = false;
                }
            }
        }

        if(MissionID == MissionManager.m_SpecialMissionID51
            || MissionID == MissionManager.m_SpecialMissionID52)
        {
            obj = EntityMgr.Instance.Get(9003);
            if (obj != null)
            {
                obj.ExtSetPos(Vector3.zero);
                obj.SetStayPos(Vector3.zero);
            }
        }
        else if(MissionID == MissionManager.m_SpecialMissionID43)
        {
            obj = EntityMgr.Instance.Get(9019);
            if (obj != null)
            {
                obj.ExtSetPos(Vector3.zero);
                obj.SetStayPos(Vector3.zero);
            }
        }
        else if(MissionManager.HasRandomMission(MissionID))
        {
            obj = EntityMgr.Instance.Get(data.m_iReplyNpc);
            if (obj != null)
            {
                NpcMissionData missionData = obj.GetUserData() as NpcMissionData;
                if (missionData != null)
                    missionData.m_MissionListReply.Clear();
            }
        }

        if (data.m_failNpcType.Count > 0)
        {
            PeEntity npc;
            NpcCmpt nc;
            foreach (var item in data.m_failNpcType)
            {
                item.npcs.ForEach(delegate (int n)
                {
                    npc = EntityMgr.Instance.Get(n);
                    if (npc != null && (nc = npc.GetComponent<NpcCmpt>()))
                        nc.NpcControlCmdId = item.type;
                });
            }
        }

        //重置ID
        for (int i = 0; i < data.m_ResetID.Count; i++)
        {
            if (m_MissionState.ContainsKey(data.m_ResetID[i]))
            {
                m_MissionState.Remove(data.m_ResetID[i]);
                MissionCommonData data1 = MissionManager.GetMissionCommonData(data.m_ResetID[i]);
                for (int m = 0; m < data1.m_TargetIDList.Count; m++)
                    m_MissionTargetState.Remove(data1.m_TargetIDList[m]);
            }
        }

        //UpdateRelatedMisTex(MissionID);
        UpdateAllNpcMisTex();

        if (PeGameMgr.IsMulti)
		{
			for (int i = 0; i < data.m_TargetIDList.Count; i++)
			{
				m_MissionTargetState.Remove(data.m_TargetIDList[i]);
			}
		}


        if (GameConfig.IsMultiMode)
            ReplyDeleteMission(MissionID);
        //UpdateNpcMissionTex(data);
    }

    public void FailureMission(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return;

        if (!HasMission(MissionID))
            return;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return;

        if (!data.m_MissionName.Equals("0"))
        {
            new PeTipMsg("[C8C800]" + PELocalization.GetString(8000158) + data.m_MissionName + "[-]", PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
        }
		
		if(!EntityCreateMgr.DbgUseLegacyCode)
			SceneEntityCreator.self.RemoveMissionPoint(MissionID, -1);

		DelMissionInfo(MissionID);

        MissionManager.Instance.UpdateMissionMainGUI(MissionID);

        if (PeGameMgr.IsStory)
        {
            List<int> storyid = data.HasStory(Story_Info.Story_Info_Fail);
            StroyManager.Instance.PushStoryList(storyid);
        }
        else if (PeGameMgr.IsAdventure)
        {
            List<int> storyid = data.HasStory(Story_Info.Story_Info_Fail);
            StroyManager.Instance.PushAdStoryList(storyid, data.m_iNpc);
        }

        PeEntity obj;
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            if (curType == TargetType.TargetType_Follow)
            {
                TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
                if (folData == null)
                    continue;

                if (folData.m_LookNameID != 0) 
                {
                    PeEntity npc = EntityMgr.Instance.Get(folData.m_LookNameID);
                    if (npc != null && npc.IsRecruited()) 
                        npc.NpcCmpt.BaseNpcOutMission = false;
                }

                if (data.m_TargetIDList[i] == 3031)
                {
                    PeCreature.Instance.mainPlayer.passengerCmpt.onGetOnCarrier -= MissionJoyrideOn;
                    PeCreature.Instance.mainPlayer.passengerCmpt.onGetOffCarrier -= MissionJoyrideOff;
                }

                for(int m=0; m<folData.m_iNpcList.Count; m++)
                {
                    obj = EntityMgr.Instance.Get(folData.m_iNpcList[m]);
                    
                    if (obj == null)
                        continue;

                    NpcMissionData missionData = obj.GetUserData() as NpcMissionData;
                    if (missionData != null)
                        missionData.mInFollowMission = false;
                    

                    NpcCmpt nc = obj.NpcCmpt;
                    if (nc != null)
                    {
                        nc.Battle = ENpcBattle.Defence;
                        nc.FixedPointPos = obj.position;
                        nc.CanTalk = true;
                    }

                    if (folData.m_EMode == 1)
					{
						followers.Remove(obj.NpcCmpt);
						StroyManager.Instance.RemoveReq(obj, EReqType.FollowTarget);
					}
                    if (folData.m_PathList.Count > 0)
                        StroyManager.Instance.RemoveReq(obj, EReqType.FollowPath);
                    if (MissionManager.HasRandomMission(MissionID))
                        GoHome(obj);
                }
            }
            else if(curType == TargetType.TargetType_TowerDif)
            {
    //            if(EntityCreateMgr.DbgUseLegacyCode)
				//{
				//	GameObject go = GameObject.Find("TowerMission");
				//	GameObject.Destroy(go);
				//}
				if (UITowerInfo.Instance != null)
					UITowerInfo.Instance.Hide();

                TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[i]);
                if (towerData == null)
                    continue;

                for(int m=0; m<towerData.m_iNpcList.Count; m++)
                {
                    obj = EntityMgr.Instance.Get(towerData.m_iNpcList[m]);

                    if(obj == null)
                        continue;

                    obj.SetInvincible(true);
                }
            }
            else if (curType == TargetType.TargetType_Discovery)
            {
                TypeSearchData searchData = MissionManager.GetTypeSearchData(data.m_TargetIDList[i]);
                if (searchData == null)
                    continue;

                if (searchData.m_NpcID != 0) 
                {
                    PeEntity npc = EntityMgr.Instance.Get(searchData.m_NpcID);
                    if (npc != null && npc.IsRecruited()) 
                        npc.NpcCmpt.BaseNpcOutMission = false;
                }
            }
        }

        if (data.m_failNpcType.Count > 0)
        {
            PeEntity npc;
            NpcCmpt nc;
            foreach (var item in data.m_failNpcType)
            {
                item.npcs.ForEach(delegate (int n)
                {
                    npc = EntityMgr.Instance.Get(n);
                    if (npc != null && (nc = npc.GetComponent<NpcCmpt>()))
                        nc.NpcControlCmdId = item.type;
                });
            }
        }

        //重置ID
        for (int i = 0; i < data.m_ResetID.Count; i++)
        {
            if(m_MissionState.ContainsKey(data.m_ResetID[i]))
            {
                m_MissionState.Remove(data.m_ResetID[i]);

                MissionCommonData data1 = MissionManager.GetMissionCommonData(data.m_ResetID[i]);

                for(int m=0; m<data1.m_TargetIDList.Count; m++)
                    m_MissionTargetState.Remove(data1.m_TargetIDList[m]);
            }
        }

        //UpdateRelatedMisTex(MissionID);
        UpdateAllNpcMisTex();
        SpecialMissionFailureHandle(MissionID);
    }

    void SpecialMissionFailureHandle(int missionID)
    {
        switch (missionID)
        {
            case 18:
                PeEntity cater = EntityMgr.Instance.Get(9009);
                PeEntity gerdy = EntityMgr.Instance.Get(9008);
                NpcCmpt npcCmpt = gerdy.NpcCmpt;
                npcCmpt.Req_Remove(EReqType.Salvation);
                StroyManager.Instance.CarryUp(cater, 9008, false);
                gerdy.NpcCmpt.Req_Remove(EReqType.Idle);

                BiologyViewCmpt view = gerdy.biologyViewCmpt;
                if (null != view) view.ActivateInjured(false);
                StroyManager.Instance.SetIdle(gerdy, "InjuredSit");
                break;
            default:
                break;
        }
    }

    public bool IsReplyTarget(int MissionID, int TargetID)
    {
        int num;

        PeEntity npc;
        if (TargetID == 3031 || TargetID == 3032)
        {
            if (!HadCompleteTarget(TargetID))
            {
                npc = EntityMgr.Instance.Get(9004);
                if (npc == null)
                    return false;

                if (!npc.IsOnCarrier())
                    return false;
            }
        }
        else if (TargetID == 5004)
        {
			if (!IsReplySpeMis())
                return false;
        }

        //第二个埋骨灰任务 TargetID == 5017

        //轻轨上的护送任务，目前还不启用
        //else if(TargetID == 3046)
        //{
        //    TypeFollowData folData = MissionManager.GetTypeFollowData(TargetID);
        //    if (folData == null)
        //        return false;

        //    for (int m = 0; m < folData.m_iNpcList.Count; m++)
        //    {
        //        npc = EntityMgr.Instance.Get(folData.m_iNpcList[m]);

        //        if (!npc.IsOnTrain())
        //            return false;
        //    }
        //}

        TargetType curType = MissionRepository.GetTargetType(TargetID);
        if (curType == TargetType.TargetType_Collect)
        {
            TypeCollectData colData = MissionManager.GetTypeCollectData(TargetID);
            if (colData == null)
                return false;

            ECreation type = IsSpecialID(colData.ItemID);
            if(type != ECreation.Null)
            {
                num = PeCreature.Instance.mainPlayer.GetCreationItemCount(type); 
                if (num < colData.ItemNum)
                    return false;
            }
            else
            {
                if (PeCreature.Instance.mainPlayer.GetPkgItemCount(colData.ItemID) < colData.ItemNum)
                    return false;
            }

        }
        else if (curType == TargetType.TargetType_KillMonster)
        {
            TypeMonsterData monData = MissionManager.GetTypeMonsterData(TargetID);
            if (monData == null)
                return false;

            for(int m=0; m<monData.m_MonsterList.Count; m++)
            {
                num = GetQuestVariable(MissionID, monData.m_MonsterList[m].npcs[UnityEngine.Random.Range(0, monData.m_MonsterList[m].npcs.Count)]);
                if(PeGameMgr.IsMulti)
                {
                    foreach(var iter in monData.m_MonsterList[m].npcs)
                    {
                        num += GetQuestVariable(MissionID, iter);
                    }
                }
                if (num < monData.m_MonsterList[m].type)
                    return false;
            }
        }
        else if (curType == TargetType.TargetType_UseItem)
        {
            TypeUseItemData useData = MissionManager.GetTypeUseItemData(TargetID);
            if (useData == null)
                return false;

            num = GetQuestVariable(MissionID,  useData.m_ItemID);

            if (num < useData.m_UseNum)
                return false;
        }
        else if (curType == TargetType.TargetType_TowerDif)
        {
            Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
            if (missionFlagType == null)
                return false;

            foreach (KeyValuePair<string, string> ite in missionFlagType)
            {
				if (ite.Key == MissionFlagStep)
                    continue;

                string[] tmplist = ite.Value.Split('_');
                if (tmplist.Length != 5)
                    return false;

                num = Convert.ToInt32(tmplist[1]);
                int count = Convert.ToInt32(tmplist[2]);

                if (count > num)
                    return false;

                int createdMon = Convert.ToInt32(tmplist[3]);
                if (createdMon != 1)
                    return false;

                int comTar = Convert.ToInt32(tmplist[4]);
                if (comTar == 0)
                    return false;
            }
        }

        return true;
    }

    public bool IsShowWnd(NpcCmpt npc) 
    {
        if (followers.Contains(npc)
            || ServantLeaderCmpt.Instance.mFollowers.Contains(npc)
            || ServantLeaderCmpt.Instance.mForcedFollowers.Contains(npc))
            return false;
        return true;
    }

	public bool IsReplySpeMis()
    {
        if (MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID10))
        {
            int count = 0;
            for (int i = 0; i < MissionManager.Instance.m_PlayerMission.m_SpeVecList.Count; i++)
            {
                if (MissionManager.Instance.m_PlayerMission.m_SpeVecList[i] == Vector3.zero)
                    return false;

				Vector3 pos = MissionManager.Instance.m_PlayerMission.m_SpeVecList[i];
//                IntVector3 index = WorldPosToBuildIndex(BestMatchPosition(MissionManager.Instance.m_PlayerMission.m_SpeVecList[i]));
				IntVector3 index = new IntVector3(Mathf.FloorToInt(pos.x * Block45Constants._scaleInverted),
				                                  Mathf.FloorToInt(pos.y * Block45Constants._scaleInverted),
				                                  Mathf.FloorToInt(pos.z * Block45Constants._scaleInverted));

                bool bpass = false;
                for (int j = 0; j < 5; j++)
                {
                    B45Block block = Block45Man.self.DataSource.SafeRead(index.x, index.y + j, index.z);
                    if (0 == block.blockType >> 2)
                        continue;

                    bpass = true;
                    break;
                }

                if(bpass)
                    count++;
            }

            if(count > 1)
                return true;
        }

        return false;
    }

    //是否能交任务
    public bool IsReplyMission(int MissionID)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return false;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return false;

        //如果不是谈话任务，那么未接取或已完成的不能交
        if (!data.IsTalkMission())
        {
            if (!HasMission(MissionID) || HadCompleteMission(MissionID))
                return false;
        }

        if(m_MissionTime.ContainsKey(MissionID))
        {
            double startTime = m_MissionTime[MissionID];
            if(GameTime.Timer.Second - startTime < data.m_NeedTime)
                return false;
        }

        //固定奖励
        int itemcount = 0;
        int equipcount = 0;
        int resourcescount = 0;

        for (int i = 0; i < data.m_Com_RewardItem.Count; i++)
        {
            ItemProto itemdata = ItemProto.GetItemData(data.m_Com_RewardItem[i].id);
            
            if (itemdata == null)
                continue;

            if(itemdata.tabIndex == 0)
            {
                if (itemdata.maxStackNum > 0)
                    itemcount += ((data.m_Com_RewardItem[i].num - 1) / itemdata.maxStackNum) + 1;
            }
            if (itemdata.tabIndex == 1)
            {
                if (itemdata.maxStackNum > 0)
                    equipcount += ((data.m_Com_RewardItem[i].num - 1) / itemdata.maxStackNum) + 1;
            }
            else
            {
                if (itemdata.maxStackNum > 0)
                    resourcescount += ((data.m_Com_RewardItem[i].num - 1) / itemdata.maxStackNum) + 1;
            }
        }

        //int itemempty = m_Player.GetItemPackage().GetEmptyGridCount();
        //int equipempty = m_Player.GetItemPackage().GetEmptyGridCount(1);
        //int resourcesempty = m_Player.GetItemPackage().GetEmptyGridCount(2);

        //if (itemcount > itemempty || equipcount > equipempty || resourcescount > resourcesempty)
        //{
        //    MessageBox_N.ShowOkBox(PELocalization.GetString(8000006));
        //    return false;
        //}
        
        if(data.m_Type != MissionType.MissionType_Mul)
        {
            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType != TargetType.TargetType_Discovery)
                {
                    if (curType == TargetType.TargetType_Follow && data.m_TargetIDList[i] % 1000 <= 500
                        && !HadCompleteTarget(data.m_TargetIDList[i]))
                        return false;
                    else if (!IsReplyTarget(MissionID, data.m_TargetIDList[i]))
                        return false;
                }
            }
            //if (MissionManager.HasRandomMission(data.m_ID) && data.m_TargetIDList.Count == 0)
            //    return false;
            //!IsReplyTarget(MissionID, data.m_TargetIDList[i])
        }
        else
        {
            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                if (IsReplyTarget(MissionID, data.m_TargetIDList[i]))
                    break;
            }
        }

        if (MissionID == MissionManager.m_SpecialMissionID64)
        {
            CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
            if (creator == null)
                return false;

            if (creator.Assembly == null)
                return false;

            Vector3 v1 = creator.Assembly.Position;

            MapMaskData mmd = MapMaskData.s_tblMaskData.Find(ret => ret.mId == 29);
            if (mmd == null)
                return false;

            Vector3 v2 = mmd.mPosition;

            return PERailwayCtrl.HasRoute(v1, v2);
        }

        return true;
    }

    void ProcessRandomMission(int MissionID, MissionCommonData data)
    {
        //先删除任务数据库里面的这个任务
        if (MissionRepository.m_MissionCommonMap.ContainsKey(MissionID))
        {
            if (data.m_TargetIDList.Count > 0)
                MissionRepository.DeleteRandomMissionData(data.m_TargetIDList[0]);

            MissionRepository.m_MissionCommonMap.Remove(MissionID);
        }

        if (PeGameMgr.IsBuild)
            return;
        
        
        //先删除可交任务
        PeEntity npc = EntityMgr.Instance.Get(data.m_iReplyNpc != 0 ? data.m_iReplyNpc : data.m_iNpc);
		if (npc == null)	//safe check
			return;

        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
        if (missionData == null)
            return;

        if (missionData.m_RandomMission != MissionID)
            return;

        if (missionData.m_MissionListReply.Contains(MissionID))
            missionData.m_MissionListReply.Remove(MissionID);

        missionData.mCurComMisNum++;
        //if (RMRepository.m_RandomFieldMap.ContainsKey(MissionID))
        //{
        //    if (missionData.mCurComMisNum > RMRepository.m_RandomFieldMap[MissionID].TargetIDMap.Count)
        //        missionData.m_MissionList.Remove(MissionID);
        //}
        
        if (PeGameMgr.IsAdventure || PeGameMgr.IsMultiAdventure || PeGameMgr.IsMultiBuild )
        {
            if(data.m_MaxNum != -1)
                missionData.m_CurMissionGroup++;

            int misid = AdRMRepository.GetRandomMission(missionData);
            missionData.m_RandomMission = misid;

            if (MissionManager.IsTalkMission(MissionID))
            {
                if(PeGameMgr.IsMulti)
                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, missionData.m_RandomMission,npc.Id);
                else
                    AdRMRepository.CreateRandomMission(missionData.m_RandomMission);
            }                
        }
    }

	int ProcessCompleteTarget(int TargetID, int MissionID,bool bFromNet = false)
	{
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
			return -1;;

        int tmp = TargetID % 1000;
        if (tmp < 500)
		{
			if(PeGameMgr.IsMulti && !bFromNet )
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_CompleteTarget,TargetID,MissionID);
				return 0;
			}
            m_MissionTargetState[TargetID] = 1;
		}

        PeEntity npc;
        List<int> idList = null;
        TargetType curType = MissionRepository.GetTargetType(TargetID);
        if (curType == TargetType.TargetType_Collect)
        {
            TypeCollectData ColData = MissionManager.GetTypeCollectData(TargetID);
            if (ColData != null)
                idList = ColData.m_ReceiveList;
        }
        else if (curType == TargetType.TargetType_Follow)
        {
			if ((PeCreature.Instance.mainPlayer.ExtGetName() != m_FollowPlayerName && m_FollowPlayerName != null))
                return -1;

            TypeFollowData folData = MissionManager.GetTypeFollowData(TargetID);
            if (folData == null)
                return -1;

            if (folData.m_LookNameID != 0) 
            {
                npc = EntityMgr.Instance.Get(folData.m_LookNameID);
                if(npc.IsRecruited())
                    npc.NpcCmpt.BaseNpcOutMission = false;
            }

            if (TargetID == 3031) 
            {
                PeCreature.Instance.mainPlayer.passengerCmpt.onGetOnCarrier -= MissionJoyrideOn;
                PeCreature.Instance.mainPlayer.passengerCmpt.onGetOffCarrier -= MissionJoyrideOff;
            }
            
            idList = folData.m_ReceiveList;
            if (folData.m_ComTalkID.Count > 0)
            {
                GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_ID, 5, TargetID);
                GameUI.Instance.mNPCTalk.PreShow();
            }
            
            for (int i = 0; i < folData.m_iNpcList.Count; i++)
            {
                //湖中的连续跟随任务
                if (TargetID == 3099)
                    break;

                npc = EntityMgr.Instance.Get(folData.m_iNpcList[i]);

                if (npc == null)
                    continue;

                NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
                if (missionData == null)
                    return -1;

                if(missionData.m_bRandomNpc)
                {
                    npc.SetAttackMode(EAttackMode.Attack);
                    npc.SetInvincible(true);
                }

                missionData.mInFollowMission = false;

                if (TargetID == 3018)
                    npc.DisableMoveCheck();

                NpcCmpt nc = npc.NpcCmpt;
                if (null != nc)
                {
                    if (!folData.m_isNeedReturn)
                        nc.FixedPointPos = npc.position;
                    if (folData.m_EMode == 1)
                    {
                        StroyManager.Instance.RemoveReq(npc, EReqType.FollowTarget);
                        npc.NpcCmpt.CanTalk = true;
                        if (followers.Contains(nc))
                            followers.Remove(nc);
                    }
                    nc.Battle = ENpcBattle.Defence;
                }
            }
        }
        else if (curType == TargetType.TargetType_Discovery)
        {
            TypeSearchData seaData = MissionManager.GetTypeSearchData(TargetID);
            if (seaData != null)
            {
                if (seaData.m_NpcID != 0) 
                {
                    npc = EntityMgr.Instance.Get(seaData.m_NpcID);
                    if(npc.IsRecruited())
                        npc.NpcCmpt.BaseNpcOutMission = false;
                }

                idList = seaData.m_ReceiveList;

                if (seaData.m_Prompt.Count > 0 || seaData.m_TalkID.Count > 0)
                {
                   GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_ID, 5, TargetID);
                   GameUI.Instance.mNPCTalk.PreShow();
                }
            }
        }
        else if (curType == TargetType.TargetType_UseItem)
        {
            TypeUseItemData UseData = MissionManager.GetTypeUseItemData(TargetID);
            if (UseData != null)
                idList = UseData.m_ReceiveList;
        }
        else if (curType == TargetType.TargetType_Messenger)
        {
            TypeMessengerData mesData = MissionManager.GetTypeMessengerData(TargetID);
            if (mesData != null)
            {
                idList = mesData.m_ReceiveList;
				if(PeGameMgr.IsSingle)
                	PeCreature.Instance.mainPlayer.RemoveFromPkg(mesData.m_ItemID, mesData.m_ItemNum);
				else
					Debug.LogError("PeCreature.Instance.mainPlayer.RemoveFromPkg");
            }
        }
		else if (curType == TargetType.TargetType_KillMonster)
		{
			TypeMonsterData MonData = MissionManager.GetTypeMonsterData(TargetID);
			if (MonData != null)
				idList = MonData.m_ReceiveList;
		}
		else if (curType == TargetType.TargetType_TowerDif)
		{
			TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(TargetID);
            if (towerData != null)
            {
                idList = towerData.m_ReceiveList;

                for (int m = 0; m < towerData.m_iNpcList.Count; m++)
                {
                    npc = EntityMgr.Instance.Get(towerData.m_iNpcList[m]);

                    if (npc == null)
                        continue;

                    npc.SetInvincible(true);
                }
                
				if (UITowerInfo.Instance != null)
					UITowerInfo.Instance.Hide();
            }
        }

        if (idList != null && GameUI.Instance != null && MissionManager.Instance != null)
        {
            for (int i = 0; i < idList.Count; i++)
            {
                if (MissionRepository.HaveTalkOP(idList[i]))
                {
                    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(idList[i], 1);
                    GameUI.Instance.mNPCTalk.PreShow();
                }
                else
                {
                    if (IsGetTakeMission(idList[i]))
                        MissionManager.Instance.SetGetTakeMission(idList[i], null, MissionManager.TakeMissionType.TakeMissionType_Get);
                }
            }
        }

        if (curType == TargetType.TargetType_Follow || curType == TargetType.TargetType_Discovery)
            MissionManager.Instance.UpdateMissionTrack(MissionID, 0, TargetID);
		return 1;
    }

    public void CompleteTarget(int TargetID, int MissionID, bool forceComplete = false, bool bFromNet = false, bool isOwner = true)
    {
		if (!forceComplete && HadCompleteTarget(TargetID))
            return;

		if(!forceComplete && !IsReplyTarget(MissionID, TargetID))
            return ;

		if(ProcessCompleteTarget(TargetID, MissionID,bFromNet) == 0)
			return;

		if(GameConfig.IsMultiMode )
		{
			if(PeGameMgr.IsMultiStory)
			{
				MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
				
				if (data == null)
					return;
				
				int tmp = TargetID % 1000;
				
				bool bpass = true;
				for (int i = 0; i < data.m_TargetIDList.Count; i++)
				{
					if (!HadCompleteTarget(data.m_TargetIDList[i]) && (tmp < 500))
					{
						bpass = false;
						break;
					}
				}
				
				if (!bpass)
					return;
                if (isOwner)
				    RequestCompleteMission(MissionID, -1, false);
			}
			else if (isOwner)
                RequestCompleteMission(MissionID, TargetID, false);

		}
        else
        {
            MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

            if (data == null)
                return;

            int tmp = TargetID % 1000;

            bool bpass = true;
            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                if (!HadCompleteTarget(data.m_TargetIDList[i]) && (tmp < 500))
                {
                    bpass = false;
                    break;
                }
                else if (ForceCompleteMission(data.m_TargetIDList[i]))
                {
                    bpass = true;
                    break;
                }
            }

            if (!bpass)
                return;

            UIMissionMgr.MissionView view = UIMissionMgr.Instance.GetMissionView(MissionID);
            if (view != null)
                view.mComplete = true;
            TargetType type = MissionRepository.GetTargetType(TargetID);
            if (data.isAutoReply || MissionRepository.IsAutoReplyMission(MissionID)
                || (RMRepository.HasRandomMission(MissionID) && (type == TargetType.TargetType_Follow || type == TargetType.TargetType_Discovery)))
                MissionManager.Instance.CompleteMission(MissionID, -1, false);
            else
                UpdateAllNpcMisTex();
		}
	}

    bool ForceCompleteMission(int targetid)
    {
        TargetType type = MissionRepository.GetTargetType(targetid);
        if (type != TargetType.TargetType_UseItem)
            return false;
        TypeUseItemData data = MissionManager.GetTypeUseItemData(targetid);
        if (data == null)
            return false;
        return data.m_comMission;
    }

    //完成任务
    public void CompleteMission(int MissionID, int TargetID = -1, bool bCheck = true, bool pushStory = true)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return;

		MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);		
		if (data == null)
			return;
		
		if (!data.IsTalkMission())
		{
			if (bCheck && (!HasMission(MissionID) || HadCompleteMission(MissionID)))
                return;
		}

        if(!GameConfig.IsMultiMode)
        {
            //再次判断一下是否可以交任务
            if (bCheck && !IsReplyMission(MissionID))
                return;
        }

        if (!data.m_MissionName.Equals("0"))
        {
            new PeTipMsg("[C8C800]" + PELocalization.GetString(8000157) + data.m_MissionName + "[-]", PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
        }

        //lz-2016.08.22 
        InGameAidData.CheckCompleteTask(MissionID);

        if (PeGameMgr.IsAdventure && RandomMapConfig.useSkillTree && data.addSpValue != 0)   //历险模式中完成任务增加玩家sp技能点数
        {
            SkillSystem.SkEntity sk = PeCreature.Instance.mainPlayer.GetComponent<SkillSystem.SkEntity>();
            if (null != sk)
            {
                List<int> type = new List<int>();
                type.Add(0);
                List<float> value = new List<float>();
                value.Add(5f);
                SkillSystem.SkEntity.MountBuff(sk, 30200126, type, value);
                new PeTipMsg("[C8C800]" + PELocalization.GetString(82209005) + value[0], PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
            }
        }

        if (data.m_increaseChain)
        {
            PeEntity npc1 = EntityMgr.Instance.Get(data.m_iReplyNpc != 0 ? data.m_iReplyNpc : data.m_iNpc);
            if (npc1 == null)    //safe check
                return;

            NpcMissionData missionData = npc1.GetUserData() as NpcMissionData;
            if (missionData == null)
                return;
            missionData.m_CurMissionGroup++;
            missionData.m_RandomMission = AdRMRepository.GetRandomMission(missionData);
        }

        if (data.m_changeReputation[0] == 1)
        {
            int playerID = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
            int targetPlayerID = Mathf.RoundToInt(GameUI.Instance.mNpcWnd.m_CurSelNpc.GetAttribute(AttribType.DefaultPlayerID));
            if (data.m_changeReputation[1] == 0)
                ReputationSystem.Instance.SetReputationValue(playerID,targetPlayerID,data.m_changeReputation[2]);
            else
                ReputationSystem.Instance.ChangeReputationValue(playerID,targetPlayerID,data.m_changeReputation[1] * data.m_changeReputation[2]);
        }

        if (!EntityCreateMgr.DbgUseLegacyCode)
			SceneEntityCreator.self.RemoveMissionPoint(MissionID, TargetID);

        SpecialMissionEndHandle(MissionID);

        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            if (curType == TargetType.TargetType_Collect)
            {
                GetSpecialItem.RemoveLootSpecialItem(data.m_TargetIDList[i]);

				TypeCollectData col = MissionManager.GetTypeCollectData(data.m_TargetIDList[i]);
                if(col != null && col.m_randItemID.Count > 1)
                    PeCreature.Instance.mainPlayer.RemoveFromPkg(col.ItemID,col.ItemNum);
            }
        }

        DelMissionInfo(MissionID);
        MissionManager.Instance.UpdateMissionMainGUI(MissionID);

        //设置完成状态
        if (!MissionManager.HasRandomMission(MissionID))
        {
            if (data.m_GuanLianList.Count == 0)
                SetMission(MissionID);
        }
        else
        {
            if (GameConfig.IsMultiMode)
            {
                if (PeCreature.Instance.mainPlayer.ExtGetName() == m_FollowPlayerName || m_FollowPlayerName == null)
                    ProcessRandomMission(MissionID, data);
            }
            else
                ProcessRandomMission(MissionID, data);
            SetMission(MissionID);
        }

        string sysinfo = "";
        string rewards = "receive item :";
        string extStr = "";

        if (false == GameConfig.IsMultiMode)
        {
            ECreation type;
            //删除任务道具
            for (int i = 0; i < data.m_Com_RemoveItem.Count; i++)
            {
                type = IsSpecialID(data.m_Com_RemoveItem[i].id);
                if(type != ECreation.Null)
                {
                    DelSpecialItem((int)type, data.m_Com_RemoveItem[i].id, data.m_Com_RemoveItem[i].num);
                }
                else if(data.m_Com_RemoveItem[i].id > 0)
                {
                    PeCreature.Instance.mainPlayer.RemoveFromPkg(data.m_Com_RemoveItem[i].id, data.m_Com_RemoveItem[i].num);
                }
                ProcessCollectMissionByID(data.m_Com_RemoveItem[i].id);
            }

            //任务奖励  固定奖励
            List<MissionIDNum> tmpList = null;
            if (data.m_Type == MissionType.MissionType_Mul)
            {
                int count = 0;
                for (int i = 0; i < data.m_TargetIDList.Count; i++)
                {
                    if (IsReplyTarget(data.m_ID, data.m_TargetIDList[i]))
                        count++;
                }

                count--;

                if (count >= 0 && data.m_Com_MulRewardItem.ContainsKey(count))
                {
                    tmpList = data.m_Com_MulRewardItem[count];
                }
            }
            else
            {
                tmpList = data.m_Com_RewardItem;
            }

            if (tmpList != null)
            {
                for (int i = 0; i < tmpList.Count; i++)
                {
                    if (tmpList[i].id <= 0)
                        continue;

                    ItemProto item = ItemProto.GetItemData(tmpList[i].id);
                    if (item == null)
                        continue;

                    if (Pathea.PeGender.IsMatch(item.equipSex, PeCreature.Instance.mainPlayer.ExtGetSex()))
                    {
                        PeCreature.Instance.mainPlayer.AddToPkg(tmpList[i].id, tmpList[i].num);
                        sysinfo += "GetItem: " + item.GetName() + "  Num: " + tmpList[i].num.ToString();
                        sysinfo += "\n";

                        extStr += " " + item.GetName() + " x" + tmpList[i].num.ToString();
                    }
                }
            }

            if (null != GameUI.Instance)
                GameUI.Instance.mItemPackageCtrl.ResetItem();
        }


        if (PeGameMgr.IsStory && pushStory)
        {
            List<int> storyid = data.HasStory(Story_Info.Story_Info_Complete);
            StroyManager.Instance.PushStoryList(storyid);
        }
        else if (PeGameMgr.IsAdventure)
        {
            List<int> storyid = data.HasStory(Story_Info.Story_Info_Complete);
            StroyManager.Instance.PushAdStoryList(storyid, data.m_iNpc);
        }

        if (MissionRepository.HaveTalkED(data.m_ID, TargetID) || data.m_PromptED.Count > 0)
        {
            if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
            {
                GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_ID, 3, TargetID);
                GameUI.Instance.mNPCTalk.PreShow();
            }
            else
                GameUI.Instance.mNPCTalk.AddNpcTalkInfo(data.m_ID, 3, TargetID);


            if (data.m_PromptED.Count > 0)
            {
                GameUI.Instance.mNPCTalk.SpTalkSymbol(true);

                if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    GameUI.Instance.mNPCTalk.AddNpcTalkInfo(data.m_ID, 8, 0, false, true);
                else
                    GameUI.Instance.mNPCTalk.AddNpcTalkInfo(data.m_ID, 8);
                if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    GameUI.Instance.mNPCTalk.SPTalkClose();
            }
        }
        else
        {
            for (int i = 0; i < data.m_EDID.Count; i++)
            {

                if (PeGameMgr.IsAdventure && MissionManager.HasRandomMission(data.m_EDID[i]))
                {
                    if(PeGameMgr.IsSingle)
                        AdRMRepository.CreateRandomMission(data.m_EDID[i]);
                    else
                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, data.m_EDID[i],data.m_iNpc);
                }

                if (IsGetTakeMission(data.m_EDID[i]))
                {
                    if (MissionRepository.HaveTalkOP(data.m_EDID[i]))
                    {
                        GameUI.Instance.mNPCTalk.NormalOrSP(0);
                        if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                        {
                            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_EDID[i], 1);
                            GameUI.Instance.mNPCTalk.PreShow();
                        }
                        else
                            GameUI.Instance.mNPCTalk.AddNpcTalkInfo(data.m_EDID[i], 1);
                    }
                    else
                    {
                        if (IsGetTakeMission(data.m_EDID[i]))
                        {
                            if (PeGameMgr.IsAdventure)
                                MissionManager.Instance.SetGetTakeMission(data.m_EDID[i], data.m_iNpc != 0 ? EntityMgr.Instance.Get(data.m_iNpc) : GameUI.Instance.mNpcWnd.m_CurSelNpc,
                                    MissionManager.TakeMissionType.TakeMissionType_Get);
                            else
                            {
                                if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
                                    MissionManager.Instance.SetGetTakeMission(data.m_EDID[i], GameUI.Instance.mNpcWnd.m_CurSelNpc, MissionManager.TakeMissionType.TakeMissionType_Get);
                                else
                                {
                                    MissionCommonData mcd = MissionManager.GetMissionCommonData(data.m_EDID[i]);
                                    MissionManager.Instance.SetGetTakeMission(data.m_EDID[i], EntityMgr.Instance.Get(mcd.m_iNpc), MissionManager.TakeMissionType.TakeMissionType_Get);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
        {
            GameUI.Instance.mNpcWnd.m_CurSelNpc.CmdFaceToPoint(Vector3.zero);
        }

        //UpdateRelatedMisTex(MissionID);
        UpdateAllNpcMisTex();

        if (false == GameConfig.IsMultiMode)
        {
            if (data.m_Type == MissionType.MissionType_Var)
            {
                if (data.m_VarValueID != 0)
                {
					string varSValue = GetQuestVariable(MissionID, MissionFlagStep);
                    int varIValue = Convert.ToInt32(varSValue) + data.m_VarValue;
					ModifyQuestVariable(data.m_VarValueID, MissionFlagStep, varIValue.ToString());
                }
            }
        }

        if (data.IsTalkMission())
            return;
        //----------------
		
		//GameGui_N.Instance.mMissionNoticeGui.AddNotice(1,data.m_MissionName);
        //GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionID, 8);
        for(int i=0; i<data.m_PromptED.Count; i++)
        {
            GameUI.Instance.mServantTalk.AddTalk(data.m_PromptED[i]);
        }
        
        if(extStr != "")
            extStr = rewards + extStr;

        //GameGui_N.Instance.mChatGUI.AddChat(PromptData.PromptType.PromptType_3, data.m_MissionName, 0, 0, 0, 0, extStr);

        Debug.Log("mission is complete!!!!");
		if(GameUI.Instance != null && GameUI.Instance.mNpcWnd != null)
			GameUI.Instance.mNpcWnd.UpdateMission();
        //m_Player.mPlayerAudio.PlaySoundById(131);

        SteamAchievementsSystem.Instance.OnMissionChange(MissionID,0);
    }

    void SpecialMissionEndHandle(int missionID) 
    {
        switch (missionID)
        {
            case MissionManager.m_SpecialMissionID10:
                m_SpeVecList.Clear();
                break;
            case MissionManager.m_SpecialMissionID80:
                m_SpeVecList.Clear();
                break;
            case MissionManager.m_SpecialMissionID68:
                GlobalEvent.OnPlayerGetOnTrain -= StroyManager.Instance.PlayerGetOnTrain;
                break;
            case 125:
                {
                    GameObject obj = GameObject.Find("alien_cage_01B");
                    if (obj != null)
                    {
                        BoxCollider col = obj.GetComponentInChildren<BoxCollider>();
                        if (col != null)
                            col.enabled = false;
                    }
                    obj = GameObject.Find("alien_cage_01A");
                    if (obj != null)
                    {
                        MeshCollider col = obj.GetComponent<MeshCollider>();
                        if (col != null)
                            col.enabled = false;
                    }
                }
                break;
            case 126:
                {
                    GameObject obj = GameObject.Find("alien_cage_01B");
                    if (obj != null)
                    {
                        BoxCollider col = obj.GetComponentInChildren<BoxCollider>();
                        if (col != null)
                            col.enabled = false;
                    }
                    obj = GameObject.Find("alien_cage_01A");
                    if (obj != null)
                    {
                        MeshCollider col = obj.GetComponent<MeshCollider>();
                        if (col != null)
                            col.enabled = false;
                    }
                }
                break;
            case MissionManager.m_SpecialMissionID82:
                {
                    PeEntity npc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
                    NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
                    if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID82))
                        missionData.m_MissionList.Remove(MissionManager.m_SpecialMissionID82);
                }
                break;
            case MissionManager.m_SpecialMissionID81:
                {
                    NpcMissionData missionData;
                    foreach (var item in EntityMgr.Instance.All)
                    {
                        if (item.proto != EEntityProto.Npc && item.proto != EEntityProto.RandomNpc)
                            continue;
                        missionData = item.GetUserData() as NpcMissionData;
                        if (missionData == null)
                            continue;
                        if (missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID82))
                            missionData.m_MissionList.Remove(MissionManager.m_SpecialMissionID82);
                    }
                }
                break;
            case 848:
                {
                    for (int i = 0; i < recordCreationName.Count; i++)
                    {
                        GameObject zj = GameObject.Find(recordCreationName[i]);
                        if (zj == null)
                            continue;
                        DragItemMousePick pick = zj.GetComponent<DragItemMousePick>();
                        if (pick == null)
                            continue;
                        pick.cancmd = true;
                    }
                    isRecordCreation = false;
                }
                break;
            default:
                break;
        }
        if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
            TrainingScene.TrainingTaskManager.Instance.CompleteMission(missionID);
    }

    public bool ProcessMonsterDead(int proid, int autoid)
    {
        Dictionary<int, int> comtarlist = new Dictionary<int, int>();
        MissionCommonData data;
        foreach (KeyValuePair<int, Dictionary<string, string>> ite in m_MissionInfo)
        {
            data = MissionManager.GetMissionCommonData(ite.Key);
            if (data == null)
                continue;

            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType == TargetType.TargetType_KillMonster)
                {
                    TypeMonsterData monData = MissionManager.GetTypeMonsterData(data.m_TargetIDList[i]);
                    if (monData == null)
                        continue;

                    for (int m = 0; m < monData.m_MonsterList.Count; m++)
                    {
                        int idx = i * 10 + m;
                        if (!monData.m_MonsterList[m].npcs.Contains(proid))
                            continue;

						string tmp = MissionFlagMonster + idx;
                        string value = GetQuestVariable(ite.Key, tmp);
                        string[] tmplist = value.Split('_');
                        int num = Convert.ToInt32(tmplist[1]) + 1;
                        value = tmplist[0] + "_" + num.ToString();
                        ModifyQuestVariable(ite.Key, tmp, value);

                        if (MissionRepository.IsAutoReplyMission(data.m_ID))
                        {
                            //CompleteTarget(data.m_TargetIDList[i], data.m_ID);
                            if (!comtarlist.ContainsKey(data.m_TargetIDList[i]))
                                comtarlist.Add(data.m_TargetIDList[i], data.m_ID);
                            continue;
                        }
                    }
                }
                else if(curType == TargetType.TargetType_TowerDif)
                {
                    if (!EntityCreateMgr.Instance.m_TowerDefineMonsterMap.ContainsKey(autoid))
                        continue;

                    TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[i]);
                    if (towerData == null)
                        continue;

                    AISpawnAutomatic aisa = AISpawnAutomatic.GetAutomatic(towerData.m_TdInfoId);
                    if (aisa == null)
                        continue;

                    for (int m = 0; m < aisa.data.Count; m++)
                    {
                        AISpawnWaveData aiwd = aisa.data[m];
                        if (aiwd == null)
                            continue;

                        bool bComplete = true;
                        for (int n = 0; n < aiwd.data.data.Count; n++)
                        {
                            AISpawnData aisd = aiwd.data.data[n];
                            if (aisd == null)
                                continue;

                            int idx = i * 100 + m * 10 + n;
							string tmp = MissionFlagTDMonster + idx;
                            string value = GetQuestVariable(ite.Key, tmp);
                            string[] tmplist = value.Split('_');
                            if (tmplist.Length != 5)
                            {
                                bComplete = false;
                                continue;
                            }

                            int createdMon = Convert.ToInt32(tmplist[3]);
                            if (createdMon != 1)
                            {
                                bComplete = false;
                                break;
                            }

                            int comTar = Convert.ToInt32(tmplist[4]);
                            if (comTar == 1)
                            {
                                bComplete = false;
                                break;
                            }

                            int num = Convert.ToInt32(tmplist[1]);
                            int count = Convert.ToInt32(tmplist[2]);

                            if (Convert.ToInt32(tmplist[0]) != proid)
                            {
                                if(count > num)
                                    bComplete = false;
                                
                                continue;
                            }

                            value = tmplist[0] + "_" + (++num).ToString() + "_" + tmplist[2] + "_" + createdMon + "_" + comTar;
                            ModifyQuestVariable(ite.Key, tmp, value);
                            m_TowerUIData.bRefurbish = true;

                            //int tmpkn = Convert.ToInt32(MissionManager.mTowerKillNum);
                            //MissionManager.mTowerKillNum = (++tmpkn).ToString();
                            if (count > num)
                            {
                                bComplete = false;
                                continue;
                            }
                        }

                        if(bComplete)
                        {
                            for (int n = 0; n < aiwd.data.data.Count; n++)
                            {
                                AISpawnData aisd = aiwd.data.data[n];
                                if (aisd == null)
                                    continue;

                                int idx = i * 100 + m * 10 + n;
								string tmp = MissionFlagTDMonster + idx;
                                string value = GetQuestVariable(ite.Key, tmp);
                                string[] tmplist = value.Split('_');
                                if (tmplist.Length != 5)
                                {
                                    bComplete = false;
                                    break;
                                }

                                string comtar = "_1";
                                value = tmplist[0] + "_" + tmplist[1] + "_" + tmplist[2] + "_" + tmplist[3] + comtar;
                                ModifyQuestVariable(ite.Key, tmp, value);
                            }

                            if (m + 1 >= aisa.data.Count)
                            {
                                if (MissionRepository.IsAutoReplyMission(ite.Key))
                                {
                                    if (!comtarlist.ContainsKey(towerData.m_TargetID))
                                        comtarlist.Add(towerData.m_TargetID, ite.Key);
                                }
                                    //CompleteTarget(towerData.m_TargetID, ite.Key);
                            }
                            else
                                EntityCreateMgr.Instance.StartTowerMission(ite.Key, m + 1, towerData);
                            break;
                        }
                    }
                }
            }
        }

        foreach(KeyValuePair<int, int> ite in comtarlist)
        {
			MissionManager.Instance.CompleteTarget(ite.Key, ite.Value);
        }
        return true;
    }

    public bool CheckHeroMis()
    {
        if (GameUI.Instance.mNpcWnd.m_CurSelNpc == null)
            return false;

        NpcMissionData missionData = GameUI.Instance.mNpcWnd.m_CurSelNpc.GetUserData() as NpcMissionData;
        if (missionData == null)
            return false;

        if (!HasMission(missionData.m_RandomMission))
            return true;

       GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(MissionManager.m_SpecialMissionID69, 1);
       GameUI.Instance.mNPCTalk.PreShow();
       return false;
    }

    public bool CheckCSCreatorMis(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return false;

        if (data.m_ColonyMis[0] == 0)
        {
            for (int i = 0; i < data.m_iColonyNpcList.Count; i++)
            {
                PeEntity npc = EntityMgr.Instance.Get(data.m_iColonyNpcList[i]);
                if (npc == null)
                    continue;

                if (!npc.IsRecruited())
                {
                    List<int> talkidList = new List<int>();
                    talkidList.Add(2173);
                   GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(talkidList, data);
                   GameUI.Instance.mNPCTalk.PreShow();
                   return false;
                }
            }
        }

        return true;
    }

	int mOpMissionID;
	string mOpMissionFlag;
	string mOpMissionValue;

	void DeleteMission()
	{
		MissionCommonData data = MissionManager.GetMissionCommonData(mOpMissionID);
		if (data == null)
			return ;
		
		for (int i = 0; i < data.m_DeleteID.Count; i++)
		{
			if(PeGameMgr.IsMulti)
			{
				//MissionManager.Instance.RequestDeleteMission(data.m_DeleteID[i]);
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, data.m_DeleteID[i]);
			}
			else
			{
				DelMissionInfo(data.m_DeleteID[i]);
            	MissionManager.Instance.UpdateMissionMainGUI(data.m_DeleteID[i]);
            	//GameGui_N.Instance.mMissionMainGui.UpdateMission(data.m_DeleteID[i]);
			}
			
			if (GameUI.Instance.mNpcWnd.isShow)
			{
				GameUI.Instance.mNpcWnd.Hide();
			}
		}

		if(PeGameMgr.IsMulti)
		{
			mOpMissionID = 0;
			mOpMissionFlag = "";
			mOpMissionValue = "";
			return;
		}
		SetQuestVariable1(mOpMissionID, mOpMissionFlag, mOpMissionValue);
        Debug.Log("字段的ID:" + mOpMissionID);
		
		mOpMissionID = 0;
		mOpMissionFlag = "";
		mOpMissionValue = "";
	}

    bool CheckGetMission(int MissionID, string MissionFlag, string MissionValue)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return false;

        string msg = PELocalization.GetString(8000008);
        bool bhasdeletemis = false;
        if (data.m_DeleteID.Count > 0)
        {
            for (int i = 0; i < data.m_DeleteID.Count; i++)
            {
                if (HasMission(data.m_DeleteID[i]))
                    bhasdeletemis = true;

                msg += "\"" + MissionRepository.GetMissionName(data.m_DeleteID[i]) + "\"";
                if (data.m_DeleteID.Count - 1 > i)
                    msg += ", ";
            }

            if (bhasdeletemis)
            {
                msg += PELocalization.GetString(8000009);
				mOpMissionID = MissionID;
				mOpMissionFlag = MissionFlag;
				mOpMissionValue = MissionValue;
				MessageBox_N.ShowYNBox(msg, DeleteMission);
                return false;
            }
        }

        MissionCommonData tmpData;
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            if (curType == TargetType.TargetType_TowerDif
                || curType == TargetType.TargetType_Follow)
            {
                //if (SPAutomatic.IsSpawning())
                //{
                //    msg = PELocalization.GetString(8000007);
                //    MessageBox_N.ShowMsgBox(MsgBoxType.Msg_OK, MsgInfoType.CannotGetMission, msg);
                //}

                //if(!PeGameMgr.IsMultiStory)
                {
                    foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_MissionInfo)
                    {
                        tmpData = MissionManager.GetMissionCommonData(iter.Key);

                        if (tmpData == null)
                            continue;

                        for (int m = 0; m < tmpData.m_TargetIDList.Count; m++)
                        {
                            TargetType curType1 = MissionRepository.GetTargetType(tmpData.m_TargetIDList[m]);
                            if (curType1 == TargetType.TargetType_TowerDif
                                || curType1 == TargetType.TargetType_Follow)
                            {
                                if(tmpData.m_TargetIDList[m] != data.m_TargetIDList[i])
                                {
                                    msg = PELocalization.GetString(8000007);
                                    MessageBox_N.ShowOkBox(msg);
                                }                                
                                return false;
                            }
                        }
                    }
                }
                
            }
        }

        return true;
    }

    //接取任务
	public int SetQuestVariable(int MissionID, string MissionFlag, string MissionValue,bool pushStory = true,bool isRecord = false)
    {
        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return 0;

        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return 0;
        
		if(MissionFlag == MissionFlagStep)
        {
            if (!CheckGetMission(MissionID, MissionFlag, MissionValue))
                return 1;
        }

        bool pass = true;
        if (SingleGameStory.curType == SingleGameStory.StoryScene.MainLand && data.m_ColonyMis[0] == 0)
        {
            for (int i = 0; i < data.m_iColonyNpcList.Count; i++)
            {
                PeEntity npc = EntityMgr.Instance.Get(data.m_iColonyNpcList[i]);
                if (npc == null)
                    continue;
                Vector3 v;
                CSMain.GetAssemblyPos(out v);
                if(!npc.IsRecruited())
                {
                    List<int> talkidList = new List<int>();
                    talkidList.Add(2173);
                    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(talkidList, data);
                    GameUI.Instance.mNPCTalk.PreShow();
                    pass = false;
                    return 1;
                }
            }
        }

        if (pass)
        {
            for (int i = 0; i < data.m_iColonyNpcList.Count; i++)
            {
                PeEntity npc = EntityMgr.Instance.Get(data.m_iColonyNpcList[i]);
                if (npc == null)
                    continue;
                Vector3 v;
                CSMain.GetAssemblyPos(out v);
                if (Vector3.Distance(npc.position, v) > 150)
				{
					CSPersonnel per = CSMain.GetColonyNpc(npc.Id);
					if(per == null)
						continue;
					per.TrySetOccupation(CSConst.potDweller);
					StroyManager.Instance.MoveTo(npc, v);
				}
            }
        }

        SetQuestVariable1(MissionID, MissionFlag, MissionValue, pushStory, isRecord);
        return 1;
    }

    public int SetQuestVariable1(int MissionID, string MissionFlag, string MissionValue,bool pushStory = true,bool isRecord = false)
    {
        if (MissionID == MissionManager.m_SpecialMissionID87
                || MissionID == MissionManager.m_SpecialMissionID88)
        {
            if (MainPlayer.Instance.entity.GetCmpt<Motion_Equip>())
                MainPlayer.Instance.entity.GetCmpt<Motion_Equip>().ActiveWeapon(false);
        }

        if (MissionID < 0 || MissionID > (int)MissionInfo.MAX_MISSION_COUNT)
            return 0;
        
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

        if (data == null)
            return 0;

        if (!data.m_MissionName.Equals("0"))
        {
            new PeTipMsg("[C8C800]" + PELocalization.GetString(8000156) + data.m_MissionName + "[-]", PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
        }

        //lz-2016.08.22 
        InGameAidData.CheckJoinMission(MissionID);

        if (data.m_npcType.Count > 0) 
        {
            PeEntity npc;
            NpcCmpt nc;
            foreach (var item in data.m_npcType)
            {
                item.npcs.ForEach(delegate(int n) 
                {
                    npc = EntityMgr.Instance.Get(n);
                    if (npc != null && (nc = npc.GetComponent<NpcCmpt>()))
                        nc.NpcControlCmdId = item.type;
                });
            }
        }

        if (data.m_TargetIDList.Count > 0 && !GameUI.Instance.mMissionTrackWnd.isShow)
            GameUI.Instance.mMissionTrackWnd.Show();

        if (data.m_ID == MissionManager.m_SpecialMissionID22 ||
                data.m_ID == MissionManager.m_SpecialMissionID24 ||
                data.m_ID == TrainingScene.TrainingRoomConfig.CreateIso)
        {
            VCEditor.Open();
            TutorMgr.Load();
        }
        //对话任务直接完成,没有目标索引也当做对话任务
        if (data.IsTalkMission())
        {
            if (PeGameMgr.IsStory)
            {
                List<int> storyid = data.HasStory(Story_Info.Story_Info_Get);
                if (pushStory)
                    StroyManager.Instance.PushStoryList(storyid);
            }
            else if (PeGameMgr.IsAdventure)
            {
                List<int> storyid = data.HasStory(Story_Info.Story_Info_Get);
                StroyManager.Instance.PushAdStoryList(storyid,data.m_iNpc);
            }
            MissionManager.Instance.CompleteMission(MissionID);
        }
        else
        {
            if (AddMissionFlagType(MissionID, MissionFlag, MissionValue) < 1)
                return 0;

            UIMissionMgr.Instance.UpdateGetableMission();
            //GameGui_N.Instance.mChatGUI.AddChat(PromptData.PromptType.PromptType_1, data.m_MissionName);

            if (PeGameMgr.IsStory)
            {
                List<int> storyid = data.HasStory(Story_Info.Story_Info_Get);
                if (pushStory)
                    StroyManager.Instance.PushStoryList(storyid);
            }
            else if (PeGameMgr.IsAdventure)
            {
                List<int> storyid = data.HasStory(Story_Info.Story_Info_Get);
                StroyManager.Instance.PushAdStoryList(storyid, data.m_iNpc);
            }

		    //MissionNotice
            if (MissionManager.Instance.m_bHadInitMission)
            {
                //GameGui_N.Instance.mMissionNoticeGui.AddNotice(0, data.m_MissionName);
               // MainRightGui_N.Instance.NoticeMissionUpdate(true);
            }

            UIMissionMgr.Instance.DeleteGetableMission(MissionID);
        }

        if (MissionRepository.m_MissionCommonMap.ContainsKey(MissionID))
        {
            if (MissionRepository.m_MissionCommonMap[MissionID].m_PromptOP.Count > 0)
            {
                GameUI.Instance.mNPCTalk.SpTalkSymbol(true);

                if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    GameUI.Instance.mNPCTalk.AddNpcTalkInfo(MissionID, 6, 0, false, true);
                else
                    GameUI.Instance.mNPCTalk.AddNpcTalkInfo(MissionID, 6);
                if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    GameUI.Instance.mNPCTalk.SPTalkClose();
            }
        }
        //MissionManager.Instance.StartCoroutine(SpTalkClose());
        for (int i = 0; i < data.m_PromptOP.Count; i++)
        {
            GameUI.Instance.mServantTalk.AddTalk(data.m_PromptOP[i]);
        }

        if (MissionID == MissionManager.m_SpecialMissionID68)
        {
            GlobalEvent.OnPlayerGetOnTrain += StroyManager.Instance.PlayerGetOnTrain;
            GlobalEvent.OnPlayerGetOffTrain += StroyManager.Instance.PlayerGetOffTrain;
        }

        if(data.m_NeedTime > 0 && !m_MissionTime.ContainsKey(MissionID))
        {
            m_MissionTime.Add(MissionID, GameTime.Timer.Second);
        }

        if (MissionManager.Instance.m_bHadInitMission && !HasGetRewards(MissionID))
        {
            //领取时删除物品
            for (int i = 0; i < data.m_Get_DeleteItem.Count; i++)
            {
                if (data.m_Get_DeleteItem[i].id <= 0)
                    continue;

                PeCreature.Instance.mainPlayer.RemoveFromPkg(data.m_Get_DeleteItem[i].id, data.m_Get_DeleteItem[i].num);
                ProcessCollectMissionByID(data.m_Get_DeleteItem[i].id);
            }

            //领取时获得物品
            for (int i = 0; i < data.m_Get_MissionItem.Count; i++)
            {
                m_GetRewards.Add(MissionID);
                if (data.m_Get_MissionItem[i].id <= 0)
                    continue;

                ItemProto item = ItemProto.GetItemData(data.m_Get_MissionItem[i].id);
                if (item == null)
                    continue;

                if (Pathea.PeGender.IsMatch(item.equipSex, PeCreature.Instance.mainPlayer.ExtGetSex()))
                {
                    if (GameConfig.IsMultiMode)
                        continue;

                    PeCreature.Instance.mainPlayer.AddToPkg(data.m_Get_MissionItem[i].id, data.m_Get_MissionItem[i].num);

                }
            }
        }

        //if (data.IsTimeMission())
        //    return 1;
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
            if (!isRecord && curType == TargetType.TargetType_Collect && !PeGameMgr.IsMulti) 
            {
				TypeCollectData col = MissionManager.GetTypeCollectData(data.m_TargetIDList[i]);
                if (col != null)
                    col.RandItemActive();
            }
            if (PeGameMgr.IsSingleAdventure || PeGameMgr.IsMultiAdventure)
                MissionOperationAdRand(curType, data.m_TargetIDList[i], MissionID);
            else
                MissionOperationStory(curType, data.m_TargetIDList[i], MissionID);
        }
        UpdateAllNpcMisTex();
        SpecialMissionStartHandle(MissionID);
        
        MissionManager.Instance.UpdateMissionMainGUI(MissionID, false);
        
        return 1;
    }
    void SpecialMissionStartHandle(int missionID) 
    {
        switch (missionID)
        {
            case MissionManager.m_SpecialMissionID10:
                KillNPC.burriedNum = 0;
                break;
            case MissionManager.m_SpecialMissionID80:
                KillNPC.burriedNum = 0;
                break;
            case MissionManager.m_SpecialMissionID81:
                NpcMissionData missionData;
                NpcCmpt nc;
                foreach (var item in EntityMgr.Instance.All)
                {
                    if (item == null)
                        continue;
                    if (item.proto != EEntityProto.Npc && item.proto != EEntityProto.RandomNpc)
                        continue;
                    if (!(nc = item.NpcCmpt) || nc.IsNeedMedicine || item.Id == 9026)
                        continue;
                    missionData = item.GetUserData() as NpcMissionData;
                    if (!missionData.m_MissionList.Contains(MissionManager.m_SpecialMissionID82))
                        missionData.m_MissionList.Add(MissionManager.m_SpecialMissionID82);
                } break;
            case 9114:
                if (CSMain.s_MgCreator != null)
                {
                    int num = CSMain.GetCSNpcs().Count;
                    if (num >= 16)
                        CompleteMission(missionID);
                    else
                        CSMain.s_MgCreator.RegisterPersonnelListener(MissionAdCountCsEntity);
                }
                break;
            case 9137:
                if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
                {
                    MissionCommonData data = MissionManager.GetMissionCommonData(9137);
                    data.m_iReplyNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
                }
                break;
            case 9138:
                if (GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
                {
                    MissionCommonData data = MissionManager.GetMissionCommonData(9138);
                    data.m_iReplyNpc = GameUI.Instance.mNpcWnd.m_CurSelNpc.Id;
                }
                break;
            default:
                break;
        }
        if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
            TrainingScene.TrainingTaskManager.Instance.InitMission(missionID);
    }

	public void SetMissionState(PeEntity npc,NpcMissionState state)
	{
		if(PeGameMgr.IsMulti)
		{
			NetworkInterface net = NetworkObject.Get(npc.Id);
			if(net != null)
			{
                net.RPCServer(EPacketType.PT_NPC_MissionState, (int)state);
			}
		}
        npc.SetState(state);
	}

    void MissionAdCountCsEntity(int event_type, CSPersonnel p)
    {
        int num = CSMain.GetCSNpcs().Count;
        if (num >= 16)
        {
            if (PeGameMgr.IsMulti)
                MissionManager.Instance.RequestCompleteMission(9114);
            else
                CompleteMission(9114);
            CSMain.s_MgCreator.UnregisterPeronnelListener(MissionAdCountCsEntity);
        }
    }

    void MissionJoyrideOn(WhiteCat.CarrierController tmp) 
    {
        PeEntity adisa = EntityMgr.Instance.Get(9004);
        StroyManager.Instance.FollowTarget(adisa, PeCreature.Instance.mainPlayer.Id, Vector3.zero, 0, 0f);
        NpcMissionData missionData = adisa.GetUserData() as NpcMissionData;
        if (missionData != null)
            missionData.mInFollowMission = true;

        adisa.NpcCmpt.CanTalk = false;
        followers.Add(adisa.NpcCmpt);
        UIMissionMgr.Instance.DeleteMission(adisa);
    }

    void MissionJoyrideOff(WhiteCat.CarrierController tmp)
    {
        PeEntity adisa = EntityMgr.Instance.Get(9004);
        StroyManager.Instance.RemoveReq(adisa, EReqType.FollowTarget);
        NpcMissionData missionData = adisa.GetUserData() as NpcMissionData;
        if (missionData != null)
            missionData.mInFollowMission = false;

        adisa.NpcCmpt.CanTalk = true;
        followers.Remove(adisa.NpcCmpt);
        UIMissionMgr.Instance.AddMission(adisa);
    }

    public void ProcessFollowMission(int MissionID, int TargetID)
    {
        if(HadCompleteTarget(TargetID))
            return;

        if (TargetID == 3031)
        {
            if (PeCreature.Instance.mainPlayer.IsOnCarrier())
                MissionJoyrideOn(null);
            PeCreature.Instance.mainPlayer.passengerCmpt.onGetOnCarrier += MissionJoyrideOn;
            PeCreature.Instance.mainPlayer.passengerCmpt.onGetOffCarrier += MissionJoyrideOff;
            return;
        }

        TypeFollowData folData = MissionManager.GetTypeFollowData(TargetID);
        if (folData == null)
            return;
        ENpcBattle type = ENpcBattle.Defence;
        if (folData.m_isAttack != 0) 
            type = (ENpcBattle)(folData.m_isAttack - 1);

        Vector3 pos = folData.m_DistPos;
		PeEntity target = null;
        if (folData.m_LookNameID != 0)
        {
            target = EntityMgr.Instance.Get(folData.m_LookNameID);
            if(target != null)
                pos = target.position;
        }
        else if (folData.m_BuildID > 0)
            GetBuildingPos(MissionID == 9032 ? 0 : MissionID, out pos);

        if (pos == Vector3.zero)
            Debug.LogWarning("Exception: follow mission npc is null.");

        List<Vector3> dists = StroyManager.Instance.GetMeetingPosition(pos, folData.m_iNpcList.Count, 2);

        List<int> followNpcIds = new List<int>();
        for (int m = 0; m < folData.m_iNpcList.Count; m++)
        {
            PeEntity npc = EntityMgr.Instance.Get(folData.m_iNpcList[m]);
            if (npc == null)
                continue;

            NpcMissionData missionData = npc.GetUserData() as NpcMissionData;

			SetMissionState(npc,NpcMissionState.Max);

            if (missionData != null)
            {
                if (TargetID == 3081 && npc.Id == 9033)
                    continue;
                else
                    missionData.mInFollowMission = true;
            }

            if (!m_iCurFollowStartPos.ContainsKey(folData.m_iNpcList[m]))
                m_iCurFollowStartPos.Add(folData.m_iNpcList[m], npc.ExtGetPos());

            if (folData.m_EMode == 1)
            {
                if (target != null)
                {
                    StroyManager.Instance.FollowTarget(npc, PeCreature.Instance.mainPlayer.Id,
                        Vector3.zero,target.Id,folData.m_DistRadius,folData.m_iNpcList.Count <= 1);
                }
                else
                {
                    StroyManager.Instance.FollowTarget(npc, PeCreature.Instance.mainPlayer.Id,
                        pos, 0, folData.m_DistRadius, folData.m_iNpcList.Count <= 1);
                }
                if(folData.m_iNpcList.Count > 1)
                    followNpcIds.Add(npc.Id);

                npc.NpcCmpt.CanTalk = false;
                followers.Add(npc.NpcCmpt);
                NpcCmpt nc = ServantLeaderCmpt.Instance.mForcedFollowers.Find(delegate(NpcCmpt n)
                {
                    if (n.Entity == null)
                        return false;
                    if (n.Entity.Id == npc.Id)
                        return true;
                    return false;
                });
                if (nc != null)
                    ServantLeaderCmpt.Instance.mForcedFollowers.Remove(nc);
            }
            else
            {
				if (missionData != null && missionData.m_bRandomNpc)
                {
                    npc.SetAttackMode(EAttackMode.Defence);
                    npc.SetInvincible(false);
                }

                if (folData.m_PathList.Count > 0)
                {
                    folData.m_PathList.Add(pos);
                    folData.m_PathList.Add(dists[m]);
                    StroyManager.Instance.MoveToByPath(npc, folData.m_PathList.ToArray());
                    folData.m_PathList.Remove(pos);
                    folData.m_PathList.Remove(dists[m]);
                }
                else
                    StroyManager.Instance.MoveTo(npc, dists[m], 1, true, SpeedState.Run);
                pathFollowers.Add(npc.NpcCmpt);
                npc.NpcCmpt.FixedPointPos = dists[m];
            }
            if (npc.NpcCmpt != null)
                npc.NpcCmpt.Battle = type;
            UIMissionMgr.Instance.DeleteMission(npc);
        }
        if (followNpcIds.Count > 0 && PeGameMgr.IsMulti)
            StroyManager.Instance.FollowTarget(followNpcIds, PeCreature.Instance.mainPlayer.Id);
    }

    void ProcessTowerMission(int MissionID, int TargetID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        int idxI = -1;
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            if (data.m_TargetIDList[i] == TargetID)
            {
                idxI = i;
                break;
            }
        }

        if(idxI == -1)
            return ;

        TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(TargetID);
        if (towerData == null)
            return;

        PeEntity npc = null;
        for (int m = 0; m < towerData.m_iNpcList.Count; m++)
        {
            npc = EntityMgr.Instance.Get(towerData.m_iNpcList[m]);

            if (npc == null)
                continue;

            npc.SetInvincible(false);
        }

        AISpawnAutomatic aisa = AISpawnAutomatic.GetAutomatic(towerData.m_TdInfoId);
        if (aisa == null)
            return;

        m_TowerUIData.MaxCount = towerData.m_Count;
        m_TowerUIData.MissionID = MissionID;
        UITowerInfo.Instance.SetInfo(m_TowerUIData);
        UnityEngine.Object obj = Resources.Load("Prefab/Mission/TowerMission");
        if (null != obj)
        {
            GameObject goObj = MonoBehaviour.Instantiate(obj) as GameObject;
            goObj.name = "TowerMission";
            Vector3 v = new Vector3();
            switch (towerData.m_Pos.type)
            {
                case TypeTowerDefendsData.PosType.getPos:
                    v = PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.pos:
                    v = towerData.m_Pos.pos;
                    break;
                case TypeTowerDefendsData.PosType.npcPos:
                    v = EntityMgr.Instance.Get(towerData.m_Pos.id).position;
                    break;
                case TypeTowerDefendsData.PosType.doodadPos:
                    v = EntityMgr.Instance.GetDoodadEntities(towerData.m_Pos.id)[0].position;
                    break;
                case TypeTowerDefendsData.PosType.conoly:
                    if (!CSMain.GetAssemblyPos(out v))
                        v = PeCreature.Instance.mainPlayer.position;
                    break;
                case TypeTowerDefendsData.PosType.camp:
                    if(VArtifactUtil.GetTownPos(towerData.m_Pos.id, out v))
                        v = PeCreature.Instance.mainPlayer.position;
                    break;
                default:
                    break;
            }
            towerData.finallyPos = v;
            goObj.transform.position = v;
            if (towerData.m_iNpcList.Count > 0)
            {
                if (npc != null)
                    goObj.transform.position = npc.ExtGetPos();
            }
        }

        if(MissionManager.Instance.m_bHadInitMission)
        {
            for (int m = 0; m < aisa.data.Count; m++)
            {
                AISpawnWaveData aiwd = aisa.data[m];
                if (aiwd == null)
                    continue;

                for (int n = 0; n < aiwd.data.data.Count; n++)
                {
                    AISpawnData aisd = aiwd.data.data[n];
                    if (aisd == null)
                        continue;

                    int idx = idxI * 100 + m * 10 + n;
                    int count = UnityEngine.Random.Range(aisd.minCount, aisd.maxCount);

                    string curNum = "_0";
                    string createMon = "_0";
                    string bComTar = "_0";

                    string value = aisd.spID + curNum + "_" + count + createMon + bComTar;
					ModifyQuestVariable(MissionID, MissionFlagTDMonster + idx, value);
                }
            }

            //MissionManager.mTowerKillNum = "0";
            //MissionManager.mTowerMonCount = towerData.m_Count.ToString();
            m_TowerUIData.PreTime = towerData.m_Time;
            m_TowerUIData.bRefurbish = true;
            EntityCreateMgr.Instance.StartTowerMission(MissionID, 0, towerData, towerData.m_Time);
        }
        else
        {
            MissionManager.Instance.StartCoroutine(WaitingTowerMission(idxI, MissionID, aisa));
        }

        if (UITowerInfo.Instance == null)
            return;
        UITowerInfo.Instance.Show();
        //GameGui_N.Instance.mChatGUI.AddChat(PromptData.PromptType.PromptType_21, "", 0, 0, towerData.m_Time, towerData.m_Count);
    }

    IEnumerator WaitingTowerMission(int idxI, int MissionID, AISpawnAutomatic aisa)
    {
        while (!MissionManager.Instance.m_bHadInitMission)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Vector3 center = EntityCreateMgr.Instance.GetPlayerPos();
        Vector3 dir = EntityCreateMgr.Instance.GetPlayerDir();

        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        if (missionFlagType == null)
            yield break;

        foreach (KeyValuePair<string, string> ite in missionFlagType)
        {
			if (ite.Key == MissionFlagStep)
                continue;

            string[] tmplist = ite.Value.Split('_');
            if (tmplist.Length != 5)
                continue;

            int comTar = Convert.ToInt32(tmplist[4]);
            if (comTar == 1)
                continue;

            int createdMon = Convert.ToInt32(tmplist[3]);
            if (createdMon == 0)
                continue;

            int num = Convert.ToInt32(tmplist[1]);
            int count = Convert.ToInt32(tmplist[2]);

            num = count - num;
            if (num == 0)
                continue;

            int minAngle = 0;
            int maxAngle = 0;
            float delaytime = 0;
            for (int m = 0; m < aisa.data.Count; m++)
            {
                AISpawnWaveData aiwd = aisa.data[m];
                if (aiwd == null)
                    continue;

                //MissionManager.Instance.StartCoroutine(EntityCreateMgr.Instance.WaitingTowerStart(MissionID, delaytime, m, idxI, aiwd, true));


                for (int n = 0; n < aiwd.data.data.Count; n++)
                {
                    AISpawnData aisd = aiwd.data.data[n];
                    if (aisd == null)
                        continue;

                    int idx = idxI * 100 + m * 10 + n;
					string tmpKey = MissionFlagTDMonster + idx;
                    if (ite.Key != tmpKey)
                        continue;

                    minAngle = aisd.minAngle;
                    maxAngle = aisd.maxAngle;
                    delaytime = aiwd.delayTime;
                    goto CreateMons;
                }
            }

        CreateMons:
            m_TowerUIData.PreTime = delaytime;
            //UITowerInfo.Instance.SetPrepTime(delaytime.ToString());
            for (int i = 0; i < num; i++)
            {
                EntityPosAgent agent = new EntityPosAgent();
				agent.entitytype = EntityType.EntityType_MonsterTD;
                agent.position = AiUtil.GetRandomPosition(center, 0, 45, dir, minAngle, maxAngle);
                agent.bMission = true;
                agent.proid = Convert.ToInt32(tmplist[0]);
                SceneMan.AddSceneObj(agent);
            }
        }

        m_TowerUIData.bRefurbish = true;
        MissionManager.Instance.UpdateMissionTrack(MissionID);
    }

    //void CreateNpcFol(int id, int npcid, TypeFollowData data)
    //{
    //    PeEntity npc = EntityMgr.Instance.Get(npcid);

    //    //while (npc == null)
    //    //{
    //    //    yield return new WaitForSeconds(0.1f);

    //    //    npc = EntityMgr.Instance.Get(npcid);
    //    //}

    //    if (npc == null)
    //    {
    //        Debug.LogError("CreateNpcFol's npc is null");
    //        return;
    //    }

    //    if(HasMission(id))
    //    {
    //        if (!m_iCurFollowStartPos.ContainsKey(npcid))
    //            m_iCurFollowStartPos.Add(npcid, npc.position);

    //        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
    //        if (missionData != null)
    //            missionData.mInFollowMission = true;

    //        npc.SetState(NpcMissionState.Max);
    //        npc.PatrolMoveTo(Vector3.zero);
    //        if(! GameConfig.IsMultiMode)
    //        {
    //            //if (IDSNpcInCampPatrol.m_iNeedPatrol != null)
    //            //    IDSNpcInCampPatrol.m_iNeedPatrol.Remove(npcid);
    //        }

    //        if(data.m_EMode == 1)
    //        {
    //            if (id != MissionManager.m_SpecialMissionID55)
    //            {
    //                //if (!m_iCurFollowMap.ContainsKey(npcid))
    //                //{
    //                    //Transform trans = FollowPoint.CreateFollowTrans(m_Player.GetGameObject().transform);
    //                    //m_iCurFollowMap.Add(npcid, trans);
    //                //}
    //                npc.ExtFollow(PeCreature.Instance.mainPlayer);
    //            }
    //            else
    //                npc.EnableMoveCheck();

    //            npc.SetGetOnCarrierIfNecessary(true);
    //        }
    //        else
    //        {
    //            Vector3 pos = Vector3.zero;
    //            pos = data.m_DistPos;
    //            if (data.m_LookNameID != 0)
    //                pos = StroyManager.Instance.GetNpcPos(data.m_LookNameID);
    //            else if (data.m_BuildID > 0)
    //                pos = VABuildingManager.Instance.GetMissionBuildingPos();

    //            npc.SetAttackMode(EAttackMode.Defence);
    //            npc.SetInvincible(false);

    //            StroyManager.Instance.MoveTo(npc, pos, 1, true, SpeedState.Run);
    //            //npc.MoveTo(pos, data.m_FollowDist, -1);
    //        }
    //    }
    //}

    void MissionOperationStory(TargetType curType, int targetid, int MissionID)
    {
        if (curType == TargetType.TargetType_Follow) 
		{
			TypeFollowData data = MissionManager.GetTypeFollowData(targetid);
			if (data.m_LookNameID != 0)
			{
                PeEntity npc = EntityMgr.Instance.Get(data.m_LookNameID);
                if (npc != null && npc.IsRecruited() && !npc.NpcCmpt.BaseNpcOutMission)
                {
                    Vector3 csPos;
                    if (CSMain.GetAssemblyPos(out csPos))
                        npc.NpcCmpt.FixedPointPos = csPos;

                    if (npc.NpcCmpt.Job == ENpcJob.Processor && NpcMgr.CallBackColonyNpcImmediately(npc))
                        npc.NpcCmpt.FixedPointPos = npc.position;

                    npc.NpcCmpt.BaseNpcOutMission = true;
                }

                //lw:Npc被基地招募，但是被派出，但是位置已经被设置到地下的错误位置去了，fixpiont修复
                if (npc != null && npc.IsRecruited() && npc.NpcCmpt.BaseNpcOutMission && npc.NpcCmpt.FixedPointPos.y < -5000f)
                {
                    Vector3 csPos;
                    if (CSMain.GetAssemblyPos(out csPos))
                        npc.NpcCmpt.FixedPointPos = csPos;
                }

                //lw:Npc没有被基地招募，被派出，且位置不在世界内了，fixpiont修复
                if (npc != null && !npc.IsRecruited() && npc.NpcCmpt.BaseNpcOutMission && !WorldCollider.IsPointInWorld(npc.NpcCmpt.FixedPointPos))
                {
                    Vector3 csPos;
                    if (CSMain.GetAssemblyPos(out csPos))
                        npc.NpcCmpt.FixedPointPos = csPos;
                }
            }
			ProcessFollowMission (MissionID, targetid);
		}
        else if (curType == TargetType.TargetType_KillMonster)
        {
            MissionMonsterKill.ProcessMission(MissionID, targetid);
        }
        else if (curType == TargetType.TargetType_TowerDif)
        {
            MissionTowerDefense.ProcessMission(MissionID, targetid);
        }
        else if (curType == TargetType.TargetType_Collect)
        {
            GetSpecialItem.AddLootSpecialItem(targetid);
            TypeCollectData data = MissionManager.GetTypeCollectData(targetid);
            if(data != null)
                ProcessCollectMissionByID(data.ItemID);
        }
        else if (curType == TargetType.TargetType_Discovery)
        {
            TypeSearchData data = MissionManager.GetTypeSearchData(targetid);
            if (data.m_NpcID != 0) 
            {
                PeEntity npc = EntityMgr.Instance.Get(data.m_NpcID);
                
                if (npc != null && npc.IsRecruited() && !npc.NpcCmpt.BaseNpcOutMission)
                {
                    Vector3 csPos;
                    if (CSMain.GetAssemblyPos(out csPos))
                        npc.NpcCmpt.FixedPointPos = csPos;

                    if (npc.NpcCmpt.Job == ENpcJob.Processor && NpcMgr.CallBackColonyNpcImmediately(npc))
                        npc.NpcCmpt.FixedPointPos = npc.position;

                    npc.NpcCmpt.BaseNpcOutMission = true;
                }

                //lw:Npc被基地招募，但是被派出，但是位置已经被设置到地下的错误位置去了，fixpiont修复
                if (npc != null && npc.IsRecruited() && npc.NpcCmpt.BaseNpcOutMission && npc.NpcCmpt.FixedPointPos.y < -5000f)
                {
                    Vector3 csPos;
                    if (CSMain.GetAssemblyPos(out csPos))
                        npc.NpcCmpt.FixedPointPos = csPos;
                }

                //lw:Npc没有被基地招募，被派出，且位置不在世界内了，fixpiont修复
                if (npc != null && !npc.IsRecruited() && npc.NpcCmpt.BaseNpcOutMission && !WorldCollider.IsPointInWorld(npc.NpcCmpt.FixedPointPos))
                {
                    Vector3 csPos;
                    if (CSMain.GetAssemblyPos(out csPos))
                        npc.NpcCmpt.FixedPointPos = csPos;
                }
            }
        }
        else if (curType == TargetType.TargetType_UseItem)
        {
            TypeUseItemData data = MissionManager.GetTypeUseItemData(targetid);
            if (data == null)
                return;
        }
    }

    public IntVector2 AvoidTownPos(IntVector2 vec)
    {
        IntVector2 townCenter;
        //if (!VArtifactUtil.IsInTown(vec, out townCenter) || vec == townCenter)
        //{
        //    return vec;
        //}
        //else
        //{
        //    Vector2 newTarget = vec + ((Vector2)(vec - townCenter)).normalized * 20f;
        //    return AvoidTownPos(new IntVector2((int)newTarget.x, (int)newTarget.y));
        //}

        int num = 0;
        while (VArtifactUtil.IsInTown(vec, out townCenter))
        {
            Vector2 offset = ((Vector2)(vec - townCenter)).normalized * 10;
            vec = new IntVector2(vec.x + (int)offset.x, vec.y + (int)offset.y);
            num++;
            if (num > 20)
                break;
        }
        return vec;
    }


    void MissionOperationAdRand(TargetType curType, int targetid, int MissionID)
    {
        PeEntity npc;
        switch (curType)
        {
            case TargetType.TargetType_Collect:
                {
                    TypeCollectData data = MissionManager.GetTypeCollectData(targetid);
                    if(data == null)
                        return ;

                    if(data.m_AdDist > 0)
                    {
                        int iMin = data.m_AdDist - data.m_AdRadius;
                        int iMax = data.m_AdDist + data.m_AdRadius;
                        data.m_TargetPos = StroyManager.Instance.GetPatrolPoint(StroyManager.Instance.GetPlayerPos(), iMin, iMax, false);
				    }

                    ProcessCollectMissionByID(data.ItemID);
                }
                break;
            case TargetType.TargetType_Follow:
                {
                    TypeFollowData data = MissionManager.GetTypeFollowData(targetid);
                    if(data == null)
                        return ;
                    
                    if (PeGameMgr.IsSingle)
                    {
                        data.m_DistRadius = data.m_AdDistPos.radius2;
                        if (MissionManager.Instance.m_bHadInitMission)
                        {
                            Vector3 referToPos;
                            switch (data.m_AdDistPos.refertoType)
                            {
                                case ReferToType.Player:
                                    referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                                case ReferToType.Town:
                                    VArtifactUtil.GetTownPos(data.m_AdDistPos.referToID, out referToPos);
                                    break;
                                case ReferToType.Npc:
                                    if (adId_entityId.ContainsKey(data.m_AdDistPos.referToID))
                                    {
                                        npc = EntityMgr.Instance.Get(adId_entityId[data.m_AdDistPos.referToID]);
                                        if (npc != null)
                                            referToPos = npc.position;
                                        else
                                            referToPos = PeCreature.Instance.mainPlayer.position;
                                    }
                                    else
                                        referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                                case ReferToType.Transcript:
                                    referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                                default:
                                    referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                            }
                            Vector2 rand = UnityEngine.Random.insideUnitCircle.normalized * data.m_AdDistPos.radius1;
                            Vector2 onCircle = new Vector2(referToPos.x + rand.x, referToPos.z + rand.y);
                            IntVector2 xzpos = new IntVector2((int)onCircle.x, (int)onCircle.y);
                            if (data.m_AdDistPos.refertoType == ReferToType.Transcript)
                                xzpos = AvoidTownPos(xzpos);
                            if (VFDataRTGen.IsTownAvailable((int)onCircle.x, (int)onCircle.y))
                                data.m_DistPos = new Vector3(xzpos.x, VFDataRTGen.GetPosHeightWithTown(xzpos), xzpos.y);
                            else
                                data.m_DistPos = new Vector3(xzpos.x, VFDataRTGen.GetPosHeight(xzpos, true), xzpos.y);

                            if (data.m_AdNpcRadius.num > 0)
                                data.m_LookNameID = StroyManager.Instance.CreateMissionRandomNpc(data.m_DistPos, data.m_AdNpcRadius.num);

                            for (int i = 0; i < data.m_CreateNpcList.Count; i++)
                            {
                                Vector3 createpos = StroyManager.Instance.GetPatrolPoint(data.m_DistPos, 3, 8, false);
                                EntityCreateMgr.Instance.CreateRandomNpc(data.m_CreateNpcList[i], createpos);
                            }
                        }

                        if (data.m_AdDistPos.refertoType == ReferToType.Transcript)
                        {
                            if (RandomDungenMgr.Instance == null)
								RandomDungenMgrData.AddInitTaskEntrance(new IntVector2((int)data.m_DistPos.x,(int)data.m_DistPos.z),data.m_AdDistPos.referToID);//AvoidTownPos(new IntVector2((int)data.m_DistPos.x,(int)data.m_DistPos.z)), data.m_AdDistPos.referToID);
                            else
								RandomDungenMgr.Instance.GenTaskEntrance(new IntVector2((int)data.m_DistPos.x,(int)data.m_DistPos.z), data.m_AdDistPos.referToID);//AvoidTownPos(new IntVector2((int)data.m_DistPos.x,(int)data.m_DistPos.z)), data.m_AdDistPos.referToID);
                        }
                        ProcessFollowMission(MissionID, targetid);
                    }
                    else
                    {                       
                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RequestAdMissionData, (int)curType, targetid, MissionID);                      
                    }
                }
                break;
            case TargetType.TargetType_Discovery:
                {
                    TypeSearchData data = MissionManager.GetTypeSearchData(targetid);
                    if(data == null)
                        return ;
                    if (PeGameMgr.IsSingle)
                    {
                        data.m_DistRadius = data.m_mr.radius2;
                        if (MissionManager.Instance.m_bHadInitMission)
                        {
                            Vector3 referToPos;
                            switch (data.m_mr.refertoType)
                            {
                                case ReferToType.Player:
                                    referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                                case ReferToType.Town:
                                    VArtifactUtil.GetTownPos(data.m_mr.referToID, out referToPos);
                                    break;
                                case ReferToType.Npc:
                                    if (adId_entityId.ContainsKey(data.m_mr.referToID))
                                    {
                                        npc = EntityMgr.Instance.Get(adId_entityId[data.m_mr.referToID]);
                                        if (npc != null)
                                            referToPos = npc.position;
                                        else
                                            referToPos = PeCreature.Instance.mainPlayer.position;
                                    }
                                    else
                                        referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                                case ReferToType.Transcript:
                                    referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                                default:
                                    referToPos = PeCreature.Instance.mainPlayer.position;
                                    break;
                            }
                            Vector2 rand = UnityEngine.Random.insideUnitCircle.normalized * data.m_mr.radius1;
                            Vector2 onCircle = new Vector2(referToPos.x + rand.x, referToPos.z + rand.y);
                            IntVector2 xzpos = new IntVector2((int)onCircle.x, (int)onCircle.y);
                            if (data.m_mr.refertoType == ReferToType.Transcript)
                                xzpos = AvoidTownPos(xzpos);
                            if (VFDataRTGen.IsTownAvailable((int)onCircle.x, (int)onCircle.y))
                                data.m_DistPos = new Vector3(xzpos.x, VFDataRTGen.GetPosHeightWithTown(xzpos), xzpos.y);
                            else
                                data.m_DistPos = new Vector3(xzpos.x, VFDataRTGen.GetPosHeight(xzpos,true), xzpos.y);

                            for (int i = 0; i < data.m_CreateNpcList.Count; i++)
                            {
                                Vector3 createpos = StroyManager.Instance.GetPatrolPoint(data.m_DistPos, 3, 8, false);
                                EntityCreateMgr.Instance.CreateRandomNpc(data.m_CreateNpcList[i], createpos);
                            }
                        }

                        if (data.m_mr.refertoType == ReferToType.Transcript)
                        {
                            if (RandomDungenMgr.Instance == null)
                                RandomDungenMgrData.AddInitTaskEntrance(AvoidTownPos(new IntVector2((int)data.m_DistPos.x,(int)data.m_DistPos.z)), data.m_mr.referToID);
                            else
                                RandomDungenMgr.Instance.GenTaskEntrance(AvoidTownPos(new IntVector2((int)data.m_DistPos.x,(int)data.m_DistPos.z)), data.m_mr.referToID);
                        }
                    }
                    else
                    {
                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RequestAdMissionData, (int)curType, targetid, MissionID);
                    }
                }
                break;
            case TargetType.TargetType_UseItem:
                {
                    TypeUseItemData data = MissionManager.GetTypeUseItemData(targetid);
                    if(data == null)
                        return ;
                    if(PeGameMgr.IsSingle)
                    {
                        Vector3 referToPos;
                        switch (data.m_AdDistPos.refertoType)
                        {
                            case ReferToType.Player:
                                referToPos = PeCreature.Instance.mainPlayer.position;
                                break;
                            case ReferToType.Town:
                                VArtifactUtil.GetTownPos(data.m_AdDistPos.referToID, out referToPos);
                                break;
                            case ReferToType.Npc:
                                if (adId_entityId.ContainsKey(data.m_AdDistPos.referToID))
                                {
                                    npc = EntityMgr.Instance.Get(adId_entityId[data.m_AdDistPos.referToID]);
                                    if (npc != null)
                                        referToPos = npc.position;
                                    else
                                        referToPos = PeCreature.Instance.mainPlayer.position;
                                }
                                else
                                    referToPos = PeCreature.Instance.mainPlayer.position;
                                break;
                            default:
                                referToPos = PeCreature.Instance.mainPlayer.position;
                                break;
                        }
                        Vector2 rand = UnityEngine.Random.insideUnitCircle.normalized * data.m_AdDistPos.radius1;
                        Vector2 onCircle = new Vector2(referToPos.x + rand.x, referToPos.z + rand.y);

                        if (VFDataRTGen.IsTownAvailable((int)onCircle.x, (int)onCircle.y))
                            data.m_Pos = new Vector3(onCircle.x, VFDataRTGen.GetPosHeightWithTown(new IntVector2((int)onCircle.x, (int)onCircle.y)), onCircle.y);
                        else
                            data.m_Pos = new Vector3(onCircle.x, VFDataRTGen.GetPosHeight(new IntVector2((int)onCircle.x, (int)onCircle.y)), onCircle.y);

                        data.m_Radius = data.m_AdDistPos.radius2;
                    }
                    else
                    {
                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RequestAdMissionData, (int)curType, targetid, MissionID);
                    }
                }
                break;
            case TargetType.TargetType_Messenger:
                {
                    TypeMessengerData data = MissionManager.GetTypeMessengerData(targetid);
                    if(data == null)
                        return ;
                }
                break;
			case TargetType.TargetType_KillMonster:
                {
                    TypeMonsterData data = MissionManager.GetTypeMonsterData(targetid);
                    if (data == null)
                        return;
                    MissionMonsterKill.ProcessMission(MissionID, targetid);
                }
				break;
            case TargetType.TargetType_TowerDif:
                {
                    TypeTowerDefendsData data = MissionManager.GetTypeTowerDefendsData(targetid);
                    if(data == null)
                        return ;

					//if (EntityCreateMgr.DbgUseLegacyCode)
					//{
					//	ProcessTowerMission(MissionID, targetid);
					//}
					//else
					{
						MissionTowerDefense.ProcessMission(MissionID, targetid);
					}
                }
                break;
        }
    }

    //void UpdateRelatedMisTex(int MissionID)
    //{
    //    MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);

    //    if(data == null)
    //        return ;

    //    float time = Time.realtimeSinceStartup;
    //    List<int> updateList = new List<int>();
    //    updateList.Add(MissionID);
    //    foreach (KeyValuePair<int, MissionCommonData> ite in MissionRepository.m_MissionCommonMap)
    //    {
    //        if(ite.Value.m_PreLimit.idlist.Contains(MissionID))
    //        {
    //            if(!updateList.Contains(ite.Key))
    //                updateList.Add(ite.Key);
    //        }

    //        for(int i=0; i<data.m_TargetIDList.Count; i++)
    //        {
    //            if(ite.Value.m_PreLimit.idlist.Contains(data.m_TargetIDList[i]))
    //            {
    //                if(!updateList.Contains(ite.Key))
    //                    updateList.Add(ite.Key);
    //            }
    //        }

    //        if (null != PeCreature.Instance.mainPlayer)
    //        {
    //            int playerID = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
    //            for (int i = 0; i < ite.Value.m_reputationPre.Count; i++)
    //            {
    //                int campID;
    //                if (ite.Value.m_reputationPre[i].campID == -1)
    //                {
    //                    if (GameUI.Instance.mNpcWnd.m_CurSelNpc == null)
    //                        continue;
    //                    campID = Mathf.RoundToInt(GameUI.Instance.mNpcWnd.m_CurSelNpc.GetAttribute(AttribType.DefaultPlayerID));
    //                }
    //                else
    //                    campID = ite.Value.m_reputationPre[i].campID;
    //                if (ite.Value.m_reputationPre[i].type == 1)
	   //             {
				//		if (ite.Value.m_reputationPre[i].min < ReputationSystem.Instance.GetReputationValue(playerID, campID)
				//		    && ReputationSystem.Instance.GetReputationValue(playerID, campID) <= ite.Value.m_reputationPre[i].max)
	   //                 {
	   //                     if (!updateList.Contains(ite.Key))
	   //                         updateList.Add(ite.Key);
	   //                 }
	   //             }
	   //             else
	   //             {
				//		if (!(ite.Value.m_reputationPre[i].min < ReputationSystem.Instance.GetReputationValue(playerID, campID)
				//		      && ReputationSystem.Instance.GetReputationValue(playerID, campID) <= ite.Value.m_reputationPre[i].max))
	   //                 {
	   //                     if (!updateList.Contains(ite.Key))
	   //                         updateList.Add(ite.Key);
	   //                 }
	   //             }
    //            }
    //        }
    //    }

    //    List<int> updateNpc = new List<int>();
    //    for(int i=0; i<updateList.Count; i++)
    //    {
    //        data = MissionManager.GetMissionCommonData(updateList[i]);
    //        if(data == null)
    //            continue;

    //        if(data.m_iNpc != 0)
    //        {
    //            if (updateNpc.Contains(data.m_iNpc))
    //                continue;

    //            updateNpc.Add(data.m_iNpc);
    //        }

    //        if (data.m_iReplyNpc == 0)
    //            continue;

    //        if (updateNpc.Contains(data.m_iReplyNpc))
    //            continue;

    //        updateNpc.Add(data.m_iReplyNpc);
    //    }

    //    for(int i=0; i<updateNpc.Count; i++)
    //    {
    //        UpdateNpcMapPos(updateNpc[i]);
    //    }
    //    UpdateAllNpcMisTex();

    //    Debug.Log("UpdateRelatedMisTex " + " --> cost " + (Time.realtimeSinceStartup - time) + " seconds");
    //}

    public void UpdateAllNpcMisTex()
    {
        foreach (PeEntity npc in EntityMgr.Instance.All)
        {
            if (npc == null)
                continue;
            if (npc.proto != EEntityProto.Npc && npc.proto != EEntityProto.RandomNpc)
                continue;

            if (npc.NpcCmpt != null)
            {
                if (!npc.NpcCmpt.IsFollower)
                    UpdateNpcMissionTex(npc);
                else
                    SetMissionState(npc, NpcMissionState.Max);
            }
        }
    }

    public void UpdateAllNpcMapPos()
    {
        List<int> passnpc = new List<int>();
        foreach (KeyValuePair<int, Dictionary<string, string>> ite in m_MissionInfo)
        {
            MissionCommonData data = MissionManager.GetMissionCommonData(ite.Key);

            if (data == null)
                continue;

            for(int i=0; i<data.m_TargetIDList.Count; i++)
            {
                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
                if (curType == TargetType.TargetType_TowerDif)
                {
                    TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(data.m_TargetIDList[i]);
                    if (towerData == null)
                        continue;

                    for(int m=0; m<towerData.m_iNpcList.Count; m++)
                    {
                        if(passnpc.Contains(towerData.m_iNpcList[m]))
                            continue;

                        passnpc.Add(towerData.m_iNpcList[m]);
                    }
                }
                else if(curType == TargetType.TargetType_Follow)
                {
                    TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
                    if (folData == null)
                        continue;

                    for(int m=0; m<folData.m_iNpcList.Count; m++)
                    {
                        if(passnpc.Contains(folData.m_iNpcList[m]))
                            continue;

                        passnpc.Add(folData.m_iNpcList[m]);
                    }
                }
            }

        }

        foreach (PeEntity npc in EntityMgr.Instance.All)
        {
            if (npc == null)
                continue;

            if (passnpc.Contains(npc.Id))
                continue;

            UpdateNpcMapPos(npc);
        }
    }

    public void UpdateNpcMapPos(int npcid)
    {
        PeEntity npc = EntityMgr.Instance.Get(npcid);

        UpdateNpcMapPos(npc);
    }

    public void UpdateNpcMapPos(PeEntity npc)
    {
        if(npc == null)
            return ;

        int id;
        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;

        if (null != missionData)
        {
            for (int i = 0; i < missionData.m_MissionList.Count; i++)
            {
                id = missionData.m_MissionList[i];

                if (!MissionRepository.IsMainMission(id))
                    continue;

                if (HasMission(id))
                    continue;

                //if (IsGetTakeMission(id))
                //{
                //    GameUI.Instance.mWorldMapGui.AddMapMask(npc.Id);
                //    return;
                //}
            }
        }
//        GameUI.Instance.mWorldMapGui.RemoveMission(npc.Id);
    }

    //public void UpdateNpcMissionTexByMissionID(int misid)
    //{
    //    MissionCommonData data = MissionManager.GetMissionCommonData(misid);

    //    UpdateNpcMissionTex(data);
    //}

    //public void UpdateNpcMissionTex(MissionCommonData data)
    //{
    //    if(data == null)
    //        return ;

    //    PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
    //    UpdateNpcMissionTex(npc);

    //    npc = EntityMgr.Instance.Get(data.m_iReplyNpc);
    //    UpdateNpcMissionTex(npc);
    //    UpdateAllNpcMisTex();
    //}

    public bool IsShowNpcMapLabel(PeEntity npc) //判断一个NPC是否应该在大地图中显示其Label 
    {
        if (npc.enityInfoCmpt.MissionState == NpcMissionState.MainCanGet || npc.enityInfoCmpt.MissionState == NpcMissionState.MainCanSubmit)
            return true;
        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
        if (missionData != null && missionData.mInFollowMission)
            return true;
        return false;
    }

    public void UpdateNpcMissionTex(PeEntity npc)
    {
        if(npc == null)
            return;
        if (npc.proto != EEntityProto.Npc && npc.proto != EEntityProto.RandomNpc)
            return;

        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;

        if (null == missionData)
            return;

        bool isMain = false;
        bool isHave = false;
        if (missionData.m_MissionListReply.Count > 0)
        {
            missionData.m_MissionListReply.ForEach(delegate(int tmp)
            {
                if (!MissionRepository.NotUpdateMisTex(tmp) && IsReplyMission(tmp))
                {
                    MissionCommonData data = MissionManager.GetMissionCommonData(tmp);
                    if (data != null)
                    {
                        if (tmp == 9137 || tmp == 9138)
                        {
                            if (data.m_iReplyNpc == npc.Id)
                                isHave = true;
                        }
                        else
                            isHave = true;
                        if (data.m_Type == MissionType.MissionType_Main)
                            isMain = true;
                    }
                    
                }
            });
            if (isHave)
            {
                if (isMain)
                    SetMissionState(npc, NpcMissionState.MainCanSubmit);
                else
                    SetMissionState(npc, NpcMissionState.CanSubmit);
                return;
            }
        }

        //if (missionData.m_RandomMission != 0 && missionData.m_RandomMission != 888)
        //{
        //    if (IsReplyMission(missionData.m_RandomMission))
        //    {
        //        SetMissionState(npc, NpcMissionState.CanSubmit);
        //        return;
        //    }
        //}

        isMain = false;
        isHave = false;
        if (missionData.m_MissionList.Count > 0)
        {
            missionData.m_MissionList.ForEach(delegate(int tmp)
            {
                if (!MissionRepository.NotUpdateMisTex(tmp) && !HasMission(tmp) && IsGetTakeMission(tmp))
                {
                    isHave = true;
                    if (MissionManager.GetMissionCommonData(tmp) != null && MissionManager.GetMissionCommonData(tmp).m_Type == MissionType.MissionType_Main)
                        isMain = true;
                }
            });
            if (isHave)
            {
                if (isMain)
                    SetMissionState(npc, NpcMissionState.MainCanGet);
                else
                    SetMissionState(npc, NpcMissionState.CanGet);
                return;
            }
        }

        isMain = false;
        isHave = false;
        if (m_MissionInfo.Count > 0)
        {
            MissionCommonData tmpdata = null;
            foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_MissionInfo)
            {
                tmpdata = MissionManager.GetMissionCommonData(iter.Key);
                if (tmpdata == null)
                    continue;

                if (tmpdata.m_iReplyNpc == npc.Id)
                {
                    isHave = true;
                    if (tmpdata.m_Type == MissionType.MissionType_Main)
                        isMain = true;
                }
            }
            if (isHave)
            {
                if (isMain)
                    SetMissionState(npc, NpcMissionState.MainHasGet);
                else
                    SetMissionState(npc, NpcMissionState.HasGet);
                return;
            }
        }

        if (missionData.m_bRandomNpc)
        {
            if (missionData.m_RandomMission != 0)
            {
                if (GameUI.Instance.mNpcWnd.CheckAddMissionListID(missionData.m_RandomMission, missionData))
                {
                    MissionCommonData data = MissionManager.GetMissionCommonData(missionData.m_RandomMission);
                    if(data.m_Type == MissionType.MissionType_Main)
                        SetMissionState(npc, NpcMissionState.MainCanGet);
                    else
                        SetMissionState(npc, NpcMissionState.CanGet);
                    return;
                }
            }
        }
		SetMissionState(npc,NpcMissionState.Max);
    }

    public ECreation IsSpecialID(int itemid)
    {
        ECreation type = ECreation.Null;
        if (itemid == 1322)              //剑
            type = ECreation.Sword;
        else if (itemid == 1323)         //单手枪
            type = ECreation.HandGun;
        else if (itemid == 1324)         //双手枪
            type = ECreation.Rifle;
        else if (itemid == 1326)         //小车
            type = ECreation.Vehicle;
        else if (itemid == 1327)         //大车
            type = ECreation.Vehicle;
        else if (itemid == 1328)         //车
            type = ECreation.Vehicle;
        else if (itemid == 1329)         //直升机
            type = ECreation.Aircraft;
        else if (itemid == 1330)         //飞机
            type = ECreation.Aircraft;
        else if (itemid == 1542)
            type = ECreation.SimpleObject;

        return type;
    }

    //public bool MatchHBType(int itemid, EntityInfo.HBType bhtype)
    //{
    //    return false;
    //    EntityInfo.HBType matchhbtype = EntityInfo.HBType.End;
    //    if(itemid == 83060001)         //主基地
    //        matchhbtype = EntityInfo.HBType.Assembly;
    //    else if(itemid == 83060002)         //民居
    //        matchhbtype = EntityInfo.HBType.Dwellings;
    //    else if(itemid == 83060003)         //仓库
    //        matchhbtype = EntityInfo.HBType.Storage;
    //    else if(itemid == 83060004)         //工匠房
    //        matchhbtype = EntityInfo.HBType.Engineering;

    //    if(matchhbtype == bhtype)
    //        return true;

    //    return false;
    //}

    public bool MacthProductType(int type, int itemid)
    {
        int matchtype = (int)CreationMgr.GetCreation(itemid).m_Attribute.m_Type;

        if(type == 100)
        {
            if(matchtype == (int)(ECreation.HandGun)
                || matchtype == (int)(ECreation.Rifle))
                return true;
        }
        else if(type == 200)
        {
            if(matchtype == (int)(ECreation.Vehicle))
                return true;
        }

        return false;
    }

    public int GetCollectSpecialItem(int type, int itemid)
    {
        int count = 0;
        //List<ItemObject> itemList = PeCreature.Instance.mainPlayer.GetPkgItemCount(0);
        //count = MatchSpecialItem(type, itemid, itemList);

        //itemList = PlayerFactory.mMainPlayer.GetItemPackage().GetItemList(1);
        //count += MatchSpecialItem(type, itemid, itemList);

        //itemList = PlayerFactory.mMainPlayer.GetItemPackage().GetItemList(2);
        //count += MatchSpecialItem(type, itemid, itemList);

        return count;
    }

    public void DelSpecialItem(int type, int itemid, int delNum)
    {
        //for(int i=0; i<delNum; i++)
        //{
        //    List<ItemObject> itemList = PlayerFactory.mMainPlayer.GetItemPackage().GetItemList(0);
        //    if(MatchSpecialItem(type, itemid, itemList, true) > 0)
        //        continue;

        //    itemList = PlayerFactory.mMainPlayer.GetItemPackage().GetItemList(1);
        //    if(MatchSpecialItem(type, itemid, itemList, true) > 0)
        //        continue;

        //    itemList = PlayerFactory.mMainPlayer.GetItemPackage().GetItemList(2);
        //    if(MatchSpecialItem(type, itemid, itemList, true) > 0)
        //        continue;
        //}
    }

    void GoHome(PeEntity npc)
    {
        if (npc == null)
            return;

        if (!m_iCurFollowStartPos.ContainsKey(npc.Id))
            return;

        //npc.SetFadeOut(5, m_iCurFollowStartPos[npc.Id] + Vector3.up * 0.8f);
        StroyManager.Instance.MoveTo(npc, m_iCurFollowStartPos[npc.Id], 1, true, SpeedState.Run);
        //npc.MoveTo(m_iCurFollowStartPos[npc.Id], 2, -1);
        m_iCurFollowStartPos.Remove(npc.Id);
    }

    public int GetTowerDefineKillNum(int MissionID)
    {
        Dictionary<string, string> missionFlagType = GetMissionFlagType(MissionID);
        if (missionFlagType == null)
            return 0;

        int count = 0;
        foreach (KeyValuePair<string, string> ite in missionFlagType)
        {
			if (ite.Key == MissionFlagStep)
                continue;

            string[] tmplist = ite.Value.Split('_');
            if (tmplist.Length != 5)
                continue;

            int num = Convert.ToInt32(tmplist[1]);
            count += num;
        }

        return count;
    }

    public int pajaLanguageBePickup = 0;
    public void ProcessCollectMissionByID(int ItemID)
    {
        List<int> misList = GetCollectMissionListByID(ItemID);

        if (misList.Count > 0)
        {
            for (int i = 0; i < misList.Count; i++)
            {
                CheckAutoCollectMissionComplete(misList[i]);
                MissionManager.Instance.UpdateMissionTrack(misList[i]);
                UpdateAllNpcMisTex();
            }
        }
    }


    void CheckAutoCollectMissionComplete(int missionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(missionID);
        List<int> CollectTarget = data.m_TargetIDList.FindAll(ite => ite / 1000 == 2);
        if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Story)
        {
            if (data == null || data.m_iReplyNpc != 0 || CollectTarget.Count == 0 )
                return;
        }
        if (PeGameMgr.sceneMode == PeGameMgr.ESceneMode.Adventure)
        {
            if (data == null || data.isAutoReply == false || CollectTarget.Count == 0 )
                return;
        }

        int sum = 0;
        foreach (int targetID in CollectTarget)
        {
            TypeCollectData colData = MissionManager.GetTypeCollectData(targetID);
            if (colData == null)
                continue;
            ECreation type = IsSpecialID(colData.ItemID);
            if (type != ECreation.Null) 
            {
                if (PeCreature.Instance.mainPlayer.GetCreationItemCount(type) >= colData.ItemNum)
                    sum++;
            }
            else
            {
                if (PeCreature.Instance.mainPlayer.GetPkgItemCount(colData.ItemID) >= colData.ItemNum)
                    sum++;
            }
        }
        if (sum >= CollectTarget.Count)
		{
			if (PeGameMgr.IsMulti)
				MissionManager.Instance.RequestCompleteMission(missionID);       
			else
				MissionManager.Instance.CompleteMission(missionID);
		}
    }

    List<Vector3> GetPos(Vector3 center, float radius, List<MissionIDNum> data)
    {
        if (data == null || data.Count == 0)
        {
            return new List<Vector3>();
        }
        List<Vector3> li = new List<Vector3>();
        foreach (MissionIDNum it in data)
        {
            for (int i = 0; i < it.num; i++)
            {
                Vector3 pos = AiUtil.GetRandomPosition(center, 0.0f, radius, 10.0f, AiUtil.groundedLayer, 10);
                if (pos != Vector3.zero)
                    li.Add(pos);
                else
                {
                    i--;
                }
            }
        }
        return li;
    }

    static Dictionary<int, Vector3> mID_buildPos = new Dictionary<int,Vector3>();
    static public void StoreBuildingPos(int missionID, Vector3 pos)
    {
        mID_buildPos[missionID] = pos;
    }

    static public bool GetBuildingPos(int missionID,out Vector3 pos) 
    {
        pos = Vector3.zero;
        if (mID_buildPos == null)
            return false;
        if (!mID_buildPos.ContainsKey(missionID))
            return false;
        pos = mID_buildPos[missionID];
        return true;
    }
    public Dictionary<int, Vector3> textSamples = new Dictionary<int, Vector3>();
    public List<int> recordAndHer = new List<int>();
    public List<int[]> recordKillNpcItem = new List<int[]>();
    public Dictionary<int, int> adId_entityId = new Dictionary<int, int>();

    List<int> StoreCollectRand() 
    {
        List<int> result = new List<int>(); //targetId,randItemid,randNum
        MissionCommonData data;
        List<int> targetId;
        foreach (var item in m_MissionInfo)
        {
            data = MissionManager.GetMissionCommonData(item.Key);
            targetId = data.m_TargetIDList.FindAll(ite => MissionRepository.GetTargetType(ite) == TargetType.TargetType_Collect);
            foreach (var item1 in targetId)
            {
				TypeCollectData col = MissionManager.GetTypeCollectData(item1);
                if (col.m_randItemID.Count > 1)
                {
                    result.Add(item1);
                    result.Add(col.ItemID);
                    result.Add(col.ItemNum);
                }
            }
        }
        return result;
    }

	public void Export(BinaryWriter bw)
    {
        bw.Write(m_GetRewards.Count);
        for (int i = 0; i < m_GetRewards.Count; i++)
            bw.Write(m_GetRewards[i]);

        bw.Write(m_MissionTargetState.Count);
        foreach (KeyValuePair<int, int> ite in m_MissionTargetState)
        {
            bw.Write(ite.Key);
            bw.Write(ite.Value);
        }

        bw.Write(m_MissionState.Count);
        foreach (KeyValuePair<int, int> ite in m_MissionState)
        {
            bw.Write(ite.Key);
            bw.Write(ite.Value);
        }

        bw.Write(m_MissionInfo.Count);
        foreach (KeyValuePair<int, Dictionary<string, string>> iter in m_MissionInfo)
        {
            bw.Write(iter.Key);
            bw.Write(iter.Value.Count);

            foreach (KeyValuePair<string, string> ite in iter.Value)
            {
                bw.Write(ite.Key);
                bw.Write(ite.Value);
            }
        }

        if (m_SpeVecList.Count == 0)
            bw.Write(0);
        else
        {
            bw.Write(m_SpeVecList.Count);
            for (int i = 0; i < m_SpeVecList.Count; i++)
            {
                PETools.Serialize.WriteVector3(bw, m_SpeVecList[i]);
            }
        }

        if (m_iCurFollowStartPos.Count == 0)
            bw.Write(0);
        else
        {
            bw.Write(m_iCurFollowStartPos.Count);
            foreach (KeyValuePair<int, Vector3> ite in m_iCurFollowStartPos)
            {
                bw.Write(ite.Key);
                PETools.Serialize.WriteVector3(bw, ite.Value);
            }
        }

        if (HadCompleteMission(MissionManager.m_SpecialMissionID89) && !HadCompleteMission(MissionManager.m_SpecialMissionID91)) 
        {
            bw.Write(recordCreationName.Count);
            foreach (var item in recordCreationName)
            {
                bw.Write(item);
            }
        }

        bw.Write(CurrentVersion);
        bw.Write(languegeSkill);
        bw.Write(recordNpcName.Count);
        foreach (var item in recordNpcName)
        {
            bw.Write(item.Key);
            bw.Write(item.Value);
        }
        bw.Write(pajaLanguageBePickup);

        bw.Write(mID_buildPos.Count);
        foreach (var item in mID_buildPos)
        {
            bw.Write(item.Key);
            bw.Write(item.Value.x);
            bw.Write(item.Value.y);
            bw.Write(item.Value.z);
        }

        List<int> randCollect = StoreCollectRand();
        bw.Write(randCollect.Count / 3);
        foreach (var item in randCollect)
            bw.Write(item);

        bw.Write(textSamples.Count);
        foreach (var item in textSamples)
        {
            bw.Write(item.Key);
            bw.Write(item.Value.x);
            bw.Write(item.Value.y);
            bw.Write(item.Value.z);
        }

        bw.Write(recordAndHer.Count);
        for (int i = 0; i < recordAndHer.Count; i++)
            bw.Write(recordAndHer[i]);

        bw.Write(recordKillNpcItem.Count);
        for (int i = 0; i < recordKillNpcItem.Count; i++)
        {
            bw.Write(recordKillNpcItem[i][0]);
            bw.Write(recordKillNpcItem[i][1]);
            bw.Write(recordKillNpcItem[i][2]);
        }

        bw.Write(adId_entityId.Count);
        foreach (var item in adId_entityId)
        {
            bw.Write(item.Key);
            bw.Write(item.Value);
        }

        bw.Write(recordCretionPos.Count);
        foreach (var item in recordCretionPos)
        {
            bw.Write(item.x);
            bw.Write(item.y);
            bw.Write(item.z);
        }

        bw.Write(pajaLanguageBePickup);
    }

    public void Import(byte[] buffer)
    {
        if (null == buffer || buffer.Length <= 0) 
            return;

        using (MemoryStream ms = new MemoryStream(buffer))
        {
            using (BinaryReader _in = new BinaryReader(ms))
            {
                int id, count;
                int iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
                    m_GetRewards.Add(_in.ReadInt32());
                }

                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
                    m_MissionTargetState.Add(_in.ReadInt32(), _in.ReadInt32());
                }

                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
                    m_MissionState.Add(_in.ReadInt32(), _in.ReadInt32());
                }

                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                    id = _in.ReadInt32();
                    count = _in.ReadInt32();
                    for (int m = 0; m < count; m++)
                        tmp.Add(_in.ReadString(), _in.ReadString());
                    m_RecordMisInfo.Add(id, tmp);
                }


                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
                    m_SpeVecList.Add(PETools.Serialize.ReadVector3(_in));
                }

                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
                    m_iCurFollowStartPos.Add(_in.ReadInt32(), PETools.Serialize.ReadVector3(_in));
                }
                
                if (HadCompleteMission(MissionManager.m_SpecialMissionID89) && !HadCompleteMission(MissionManager.m_SpecialMissionID91))
                {
                    int n = _in.ReadInt32();
                    for (int i = 0; i < n; i++)
                    {
                        _in.ReadString();
                        //recordCreationName.Add(_in.ReadString());
                    }
                }

                int saveVersion = _in.ReadInt32();
                if (saveVersion >= Version_1)
                {
                    languegeSkill = _in.ReadInt32();
                    if (saveVersion >= Version_2)
                    {
                        int num = _in.ReadInt32();
                        for (int i = 0; i < num; i++)
                            recordNpcName.Add(_in.ReadInt32(), _in.ReadString());
                        if(saveVersion >= Version_3)
                            _in.ReadInt32();
                    }
                }

                if (saveVersion >= Version_4) 
                {
                    if(mID_buildPos != null)
                        mID_buildPos.Clear();
                    mID_buildPos = new Dictionary<int,Vector3>();
                    int num = _in.ReadInt32();
                    for (int i = 0; i < num; i++)
                    {
                        int mID = _in.ReadInt32();
                        float x = _in.ReadSingle();
                        float y = _in.ReadSingle();
                        float z = _in.ReadSingle();
                        mID_buildPos.Add(mID, new Vector3(x, y, z));
                    }
                }

                if (saveVersion >= Version_5)
                {
                    int num = _in.ReadInt32();
                    for (int i = 0; i < num; i++)
                    {
                        TypeCollectData col = MissionManager.GetTypeCollectData(_in.ReadInt32());
                        if (col == null)
                        {
                            Debug.LogError("Mission TypeCollectData is wrong!");
                            _in.ReadInt32();
                            _in.ReadInt32();
                        }
                        else
                        {
                            col.ItemID = _in.ReadInt32();
                            col.ItemNum = _in.ReadInt32();
                        }
                    }
                }

                textSamples.Clear();
                if (saveVersion >= Version_6)
                {
                    int num = _in.ReadInt32();
                    for (int i = 0; i < num; i++)
                    {
                        int index = _in.ReadInt32();
                        float x = _in.ReadSingle();
                        float y = _in.ReadSingle();
                        float z = _in.ReadSingle();
                        StroyManager.CreateLanguageSample_byIndex(index);
                        textSamples.Add(index, new Vector3(x, y, z));
                    }

                    num = _in.ReadInt32();
                    for (int i = 0; i < num; i++)
                        StroyManager.CreateAndHeraNest(_in.ReadInt32());
                }

                recordKillNpcItem.Clear();
                if (saveVersion >= Version_7) 
                {
                    int num = _in.ReadInt32();
                    for (int i = 0; i < num; i++)
                    {
                        int npcid = _in.ReadInt32();
                        int itemProtoid = _in.ReadInt32();
                        int itemNum = _in.ReadInt32();
                        KillNPC.NPCaddItem(npcid, itemProtoid, itemNum);
                    }
                }

                if (saveVersion >= Version_8)
                {
                    int num = _in.ReadInt32();
                    for (int i = 0; i < num; i++)
                        adId_entityId.Add(_in.ReadInt32(), _in.ReadInt32());
                }

                if (saveVersion >= Version_9)
                {
                    int num = _in.ReadInt32();
                    for (int i = 0; i < num; i++)
                        recordCretionPos.Add(new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle()));
                }

                if (saveVersion >= Version_10)
                {
                    pajaLanguageBePickup = _in.ReadInt32();
                }

                _in.Close();
            }
            ms.Close();
        }
    }
	public void ClearMission()
	{
		m_GetRewards.Clear();
		m_MissionTargetState.Clear();
		m_MissionState.Clear();
		m_RecordMisInfo.Clear();
		m_SpeVecList.Clear();
		m_iCurFollowStartPos.Clear();
		m_MissionInfo.Clear();
	}
}