using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;


public enum MissionType
{
    MissionType_Talk,       //只谈话
    MissionType_Main,       //主线
    MissionType_Sub,        //支线任务
    MissionType_Time,       //时间任务
    MissionType_Mul,        
    MissionType_Var,
    MissionType_Unkown,
};

public enum TargetType
{
    TargetType_Unkown,     //错误类型
    TargetType_KillMonster, //杀怪
    TargetType_Collect,     //收集
    TargetType_Follow,     //跟随
    TargetType_Discovery,  //探索
    TargetType_UseItem,     //使用道具
    TargetType_Messenger,   //送信
    TargetType_TowerDif,   //塔防
};

public enum Story_Info
{
    Story_Info_Get = 1,
    Story_Info_Ing,
    Story_Info_Complete,
    Story_Info_Fail,
}

public struct MonsterIDNum
{
    public Vector3 pos;
    public int id;
    public int num;
    public int radius;
}

public struct MissionIDNum
{
    public int id;
    public int num;
};

public class PreLimit
{
    public int type;
    public List<int> idlist = new List<int>();
}

public struct StoryInfo
{
    public Story_Info type;
    public int storyid;
}

public struct MissionRand
{
    public int dist;
    public int radius;
}

public struct AdMissionRand
{
    public ReferToType refertoType;
    public int referToID;
    public int radius1;
    public int radius2;
}

public struct AdNpcInfo
{
    public int dist;
    public int num;
}

public struct ReputationPreLimit 
{
    public int type;
    public int min;
    public int max;
    public int campID;
}

public struct NpcType 
{
    public List<int> npcs;
    public int type;
}

public struct CreMons
{
    public int type;
    public int monID;
    public int monNum;
}

public struct CreDungeon
{
    public bool effect;
    public int npcID;
    public int radius;
    public int dungeonLevel;
}

public class MissionCommonData   //任务表  Quest_List
{
    public int m_ID;                    //任务ID
    public string m_MissionName;        //显示在任务主界面的任务名称
    public int m_iNpc;                  //发布任务NPC
    public int m_iReplyNpc;             //交任务NPC
    public MissionType m_Type;          //任务类型 0-普通对话， 1-主线任务， 2-支线任务
    public string m_ScriptID;           //脚本ID
    public int m_MaxNum;                //最多完成次数
    public string m_Description;        //任务描述
    public string m_MulDesc;
    public int m_VarValueID;
    public int m_VarValue;
    public List<int> m_TargetIDList;    //目标索引
    public PreLimit m_PreLimit;         //前置任务限制
    public PreLimit m_AfterLimit;       //完成和接取后不能再接取
    public PreLimit m_MutexLimit;       //互斥任务
    public List<ReputationPreLimit> m_reputationPre;
    public List<int> m_GuanLianList;    //关联任务
    public int[] m_PlayerTalk;

    public List<MissionIDNum> m_Get_DemandItem;         //领取时需求物品
    public List<MissionIDNum> m_Get_DeleteItem;         //领取时删除物品
    public List<MissionIDNum> m_Get_MissionItem;        //领取时获得物品
    public List<MissionIDNum> m_Com_RewardItem;         //固定奖励
    public Dictionary<int, List<MissionIDNum>> m_Com_MulRewardItem;         //
    public List<MissionIDNum> m_Com_SelRewardItem;      //选择奖励
    public List<MissionIDNum> m_Com_RemoveItem;         //完成任务移除物品

    public List<int> m_TalkOP;                      //任务开始谈话: 领取任务时进行的谈话
    public List<int> m_OPID;                        //由开始对话触发的任务ID
    public List<int> m_TalkIN;                      //任务中谈话: 特定任务在进行时会触发的谈话ID。
    public List<int> m_INID;                        //由任务中对话触发的任务ID
    public List<int> m_TalkED;                      //任务结束谈话: 任务完成时进行的谈话
    public List<int> m_EDID;                        //由结束对话触发的任务ID

    public bool m_bGiveUp;          //放弃任务
    public List<int> m_ResetID;     //任务失败或放弃后，重置的任务ID
    public List<int> m_DeleteID;    //接任务删除该任务


    public List<int> m_PromptOP;
    public List<int> m_PromptIN;
    public List<int> m_PromptED;
    public List<StoryInfo> m_StoryInfo;
    public int m_NeedTime;
    public int m_timeOverToPlot;
    public List<int> m_iColonyNpcList;
    public int[] m_ColonyMis;
    public bool isAutoReply = false;
    public int addSpValue;
    public List<NpcType> m_npcType;
    public List<int> m_tempLimit;
    public int m_replyIconId;
    public bool m_increaseChain;
    public int[] m_changeReputation;
    public CreDungeon creDungeon;
    public List<NpcType> m_failNpcType;

    public MissionCommonData()
    {
        m_MissionName = "";
        m_ScriptID = "";
        m_Description = "";

        m_PreLimit = new PreLimit();
        m_AfterLimit = new PreLimit();
        m_MutexLimit = new PreLimit();
        m_TargetIDList = new List<int>();
        m_GuanLianList = new List<int>();
        m_PlayerTalk = new int[2];

        m_Get_DemandItem = new List<MissionIDNum>();
        m_Get_DeleteItem = new List<MissionIDNum>();
        m_Get_MissionItem = new List<MissionIDNum>();
        m_Com_RewardItem = new List<MissionIDNum>();
        m_Com_MulRewardItem = new Dictionary<int, List<MissionIDNum>>();
        m_Com_SelRewardItem = new List<MissionIDNum>();
        m_Com_RemoveItem = new List<MissionIDNum>();

        m_TalkOP = new List<int>();
        m_OPID = new List<int>();
        m_TalkIN = new List<int>();
        m_INID = new List<int>();
        m_TalkED = new List<int>();
        m_EDID = new List<int>();
        m_ResetID = new List<int>();
        m_DeleteID = new List<int>();
        m_PromptOP = new List<int>();
        m_PromptIN = new List<int>();
        m_PromptED = new List<int>();
        m_StoryInfo = new List<StoryInfo>();
        m_iColonyNpcList = new List<int>();
        m_ColonyMis = new int[2];
        m_npcType = new List<NpcType>();
        m_tempLimit = new List<int>();
        m_changeReputation = new int[3];
        m_reputationPre = new List<ReputationPreLimit>();
        creDungeon = new CreDungeon();
        m_failNpcType = new List<NpcType>();
    }

    public bool IsTalkMission()
    {
        if (m_Type == MissionType.MissionType_Time)
            return false;

        if (m_Type == MissionType.MissionType_Talk)
            return true;

        if (m_TargetIDList.Count == 0)
            return true;

        return false;
    }

    public bool IsTimeMission()
    {
        if (m_Type == MissionType.MissionType_Time)
            return true;

        return false;
    }

    public List<int> HasStory(Story_Info type)
    {
        List<int> tmpList = new List<int>();

        if(m_StoryInfo.Count == 0)
            tmpList.Add(-1);

        for(int i=0; i<m_StoryInfo.Count; i++)
        {
            StoryInfo si = m_StoryInfo[i];
            if((int)si.type == (int)type)
                tmpList.Add(si.storyid);
        }
        
        return tmpList;
    }
}

public class TypeMonsterData   //杀怪任务  Quest_KillMonster
{
    public int m_TargetID;
    public int m_ScriptID;
    public string m_Desc;
    public List<NpcType> m_MonsterList;
    public List<CreMons> m_CreMonList;
    public int m_MonsterID;
    public int m_MonsterNum;
    public Vector3 m_TargetPos;       //目标区域
    public AdMissionRand m_mr;
    public List<int> m_ReceiveList;
    public int type;
    public bool m_mustByPlayer;
    public bool m_destroyTown;
    public List<int> m_campID;
    public List<int> m_townNum;

    public TypeMonsterData()
    {
        m_MonsterList = new List<NpcType>();
        m_CreMonList = new List<CreMons>();
        m_ReceiveList = new List<int>();
        m_campID = new List<int>();
        m_townNum = new List<int>();
    }
}

public class TypeCollectData   //收集任务  Quest_LootItem
{
    public int m_TargetID;
    public int m_ScriptID;
    public string m_Desc;
    public int m_Type;

    private int m_ItemID;
    public int ItemID
    {
        get { return m_ItemID; }
        set { m_ItemID = value; }
    }

    private int m_ItemNum;
    public int ItemNum
    {
        get { return m_ItemNum; }
        set { m_ItemNum = value; }
    }

    public int m_TargetItemID;
    public int m_MaxNum;
    public int m_Chance;
    public Vector3 m_TargetPos;
    public int m_TargetRadius;
    public List<int> m_ReceiveList;
    public int m_AdDist;          //adv
    public int m_AdRadius;        //adv
    public int[] m_randItemNum;
    public List<int> m_randItemID;

    public void RandItemActive() 
    {
        if (m_randItemID.Count > 1)
        {
             m_ItemID = m_randItemID[UnityEngine.Random.Range(0, m_randItemID.Count)];
            m_ItemNum = UnityEngine.Random.Range(m_randItemNum[0], m_randItemNum[1] + 1) * m_randItemNum[2];
        }
    }

    public void muiSetItemActive(int _itemID,int _itemNum)
    {
        m_ItemID = _itemID;
        m_ItemNum = _itemNum;
    }

    public TypeCollectData()
    {
        m_ReceiveList = new List<int>();
        m_randItemNum = new int[3];
        m_randItemID = new List<int>();
    }
}

public class TalkInfo
{
    public Vector3 pos;
    public int radius;
    public List<int> talkid;

    public TalkInfo()
    {
        talkid = new List<int>();
    }
}

public class AdTalkInfo
{
    public int dist;
    public int radius;
    public List<int> talkid;

    public AdTalkInfo()
    {
        talkid = new List<int>();
    }
}

public struct AdTalkInfo1
{
    public int time;
    public int talkid;
}

public class TypeFollowData    //护送任务  Quest_HuSong
{
    public int m_TargetID;
    public int m_ScriptID;
    public int m_SceneType;
    public string m_Desc;
    public List<int> m_iNpcList;    //需要护送的NPC
    public int m_EMode;             //护送方式---------0:npc自己走。1:npc跟随玩家走
    public int m_isAttack;          //跟随过程中行为模式-------1：主动攻击。2：被攻击后再攻击。3：完全不还手
    public int m_BuildID;
    public int m_LookNameID;       //护送目的地NPC
    public Vector3 m_DistPos;       //目的地
    public int m_DistRadius;        //目的地范围
    public int m_TrackRadius;
    public List<int> m_WaitDist;          //超过距离停住和再次启动的距离;
    public Vector3 m_ResetPos;      //完成后保存位置
    public Vector3 m_FailResetPos;  //失败后保存位置
    public List<TalkInfo> m_TalkInfo;      //任务中触发对话
    public List<MonsterIDNum> m_Monster;  //进行中刷怪ID
    public List<int> m_ComTalkID;           //完成任务对话
    public List<int> m_iFailNpc;  //任务失败要判断的npc
    public AdMissionRand m_AdDistPos;       //adv
    public AdNpcInfo m_AdNpcRadius;  //adv
    public List<AdTalkInfo1> m_AdTalkInfo;      //adv 任务中触发对话
    public List<AdTalkInfo> m_AdTalkID;      //adv 任务中触发对话
    public List<int> m_ReceiveList;
    public List<int> m_CreateNpcList;   //adv
    public bool m_isNeedPlayer;
    public List<Vector3> m_PathList;
    public bool m_isNeedReturn;
    public Dictionary<int,int[]> npcid_behindTalk_forwardTalk;  //npc等待时的对话ID

    public TypeFollowData()
    {
        m_iNpcList = new List<int>();
        m_TalkInfo = new List<TalkInfo>();
        m_Monster = new List<MonsterIDNum>();
        m_ComTalkID = new List<int>();
        m_iFailNpc = new List<int>();
        m_AdTalkInfo = new List<AdTalkInfo1>();
        m_AdTalkID = new List<AdTalkInfo>();
        m_ReceiveList = new List<int>();
        m_CreateNpcList = new List<int>();
        m_WaitDist = new List<int>();
        m_PathList = new List<Vector3>();
        npcid_behindTalk_forwardTalk = new Dictionary<int, int[]>();
    }

}

public class TypeSearchData    //探索任务  Quest_EnterArea
{
    public int m_TargetID;
    public int m_ScriptID;
    public int m_SceneType;
    public string m_Desc;
    public int m_NpcID;
    public Vector3 m_DistPos;
    public int m_DistRadius;
    public int m_TrackRadius;
    public List<int> m_Prompt;  //提示
    public List<int> m_TalkID;  //旁白
    public AdMissionRand m_mr;
    public int m_AdNpcRadius;     //在目的地生成NPC数量
    public List<int> m_ReceiveList;
    public List<AdTalkInfo> m_AdTalkID;      //adv
    public List<AdTalkInfo> m_AdPrompt;  //adv
    public List<int> m_CreateNpcList;   //adv
    public bool m_notForDungeon = true;

    public TypeSearchData()
    {
        m_Prompt = new List<int>();
        m_TalkID = new List<int>();
        m_AdPrompt = new List<AdTalkInfo>();
        m_AdTalkID = new List<AdTalkInfo>();
        m_ReceiveList = new List<int>();
        m_CreateNpcList = new List<int>();
    }
}

public class TypeUseItemData   //使用道具  Quest_UseItem
{
    public int m_TargetID;
    public int m_ScriptID;
    public string m_Desc;
    public int m_Type;
    public int m_ItemID;
    public int m_UseNum;
    public Vector3 m_Pos;
    public int m_Radius;
    public List<int> m_UsedPrompt;  //使用完提示
    public List<int> m_TalkID;  //旁白
    public List<int> m_FailPrompt;  //错误地点使用提示
    public List<int> m_ReceiveList;
    public AdMissionRand m_AdDistPos;          //adv
    public bool m_comMission;   //adv
    public bool m_allowOld;

    public TypeUseItemData()
    {
        m_UsedPrompt = new List<int>();
        m_TalkID = new List<int>();
        m_FailPrompt = new List<int>();
        m_ReceiveList = new List<int>();
        m_AdDistPos = new AdMissionRand();
    }
}

public class TypeMessengerData //送信， 完成时自动删除物品 Quest_Delivery 
{
    public int m_TargetID;
    public int m_ScriptID;
    public string m_Desc;
    public int m_iNpc;        //给信NPC
    public int m_iReplyNpc;   //收信NPC
    public int m_ItemID;
    public int m_ItemNum;
    public List<int> m_ReceiveList;
    public MissionRand m_AdNpcRadius;     //adv 距离此范围生成NPC

    public TypeMessengerData()
    {
        m_Desc = "";
        m_ReceiveList = new List<int>();
    }
}

public class TDInfo
{
    public int dir_min;
    public int dir_max;
    public int monsterID;
    public int num;
}

public class TowerDefendsInfoData
{
    public int m_ID;
    public int m_delay;
    public List<TDInfo> m_TdInfoList = new List<TDInfo>();
}

public class TypeTowerDefendsData  //塔防   Quest_TowerDefence
{
    public enum PosType
    {
        getPos,
        pos,
        npcPos,
        doodadPos,
        conoly,
        camp
    }

    public struct MyPos
    {
        public PosType type;
        public Vector3 pos;
        public int id;
    }

    public int m_TargetID;
    public int m_ScriptID;
    public string m_Desc;
    public int m_Time;              //准备时间
    public MyPos m_Pos;           //防守坐标
    public List<int> m_iNpcList;  //守护NPC
    public List<int> m_ObjectList;    //守护物体ID
    public int m_tolTime;
    public int m_range;
    public int m_Count;             //怪物波数
    public int m_TdInfoId;			//
    public List<int> m_SweepId;
    public List<int> m_ReceiveList;

    public Vector3 finallyPos = new Vector3();

    public TypeTowerDefendsData()
    {
        m_iNpcList = new List<int>();
        m_ObjectList = new List<int>();
        m_ReceiveList = new List<int>();
        m_SweepId = new List<int>();
    }
}


public class MissionRepository
{
    public static Dictionary<int, MissionCommonData> m_MissionCommonMap = new Dictionary<int, MissionCommonData>();
    public static Dictionary<int, TypeMonsterData> m_TypeMonster = new Dictionary<int, TypeMonsterData>();
    public static Dictionary<int, TypeCollectData> m_TypeCollect = new Dictionary<int, TypeCollectData>();
    public static Dictionary<int, TypeFollowData> m_TypeFollow = new Dictionary<int, TypeFollowData>();
    public static Dictionary<int, TypeSearchData> m_TypeSearch = new Dictionary<int, TypeSearchData>();
    public static Dictionary<int, TypeUseItemData> m_TypeUseItem = new Dictionary<int, TypeUseItemData>();
    public static Dictionary<int, TypeMessengerData> m_TypeMessenger = new Dictionary<int, TypeMessengerData>();
    public static Dictionary<int, TowerDefendsInfoData> m_TDInfoMap = new Dictionary<int, TowerDefendsInfoData>();
    public static Dictionary<int, TypeTowerDefendsData> m_TypeTowerDefends = new Dictionary<int, TypeTowerDefendsData>();

    public static Dictionary<int, List<int>> m_iNpcMissionMap = new Dictionary<int, List<int>>();
    public static Dictionary<int, List<int>> m_iNpcReplyMissionMap = new Dictionary<int, List<int>>();

    public static void AddMissionCommonData(int id, MissionCommonData data)
    {
        if (m_MissionCommonMap.ContainsKey(id))
            return;

        m_MissionCommonMap.Add(id, data);
    }

    public static MissionCommonData GetMissionCommonData(int MissionID)
    {
        if(!m_MissionCommonMap.ContainsKey(MissionID))
            return null;

        return m_MissionCommonMap[MissionID];
    }

    public static void AddTypeMonsterData(int id, TypeMonsterData data)
    {
        if (m_TypeMonster.ContainsKey(id))
            return;

        m_TypeMonster.Add(id, data);
    }

    public static TypeMonsterData GetTypeMonsterData(int MissionID)
    {
        if(!m_TypeMonster.ContainsKey(MissionID))
            return null;

        return m_TypeMonster[MissionID];
    }

    public static void AddTypeCollectData(int id, TypeCollectData data)
    {
        if (m_TypeCollect.ContainsKey(id))
            return;

        m_TypeCollect.Add(id, data);
    }

    public static TypeCollectData GetTypeCollectData(int MissionID)
    {
        if(!m_TypeCollect.ContainsKey(MissionID))
            return null;

        return m_TypeCollect[MissionID];
    }

    public static void AddTypeFollowData(int id, TypeFollowData data)
    {
        if (m_TypeFollow.ContainsKey(id))
            return;

        m_TypeFollow.Add(id, data);
    }

    public static TypeFollowData GetTypeFollowData(int MissionID)
    {
        if(!m_TypeFollow.ContainsKey(MissionID))
            return null;

        return m_TypeFollow[MissionID];
    }

    public static void AddTypeSearchData(int id, TypeSearchData data)
    {
        if (m_TypeSearch.ContainsKey(id))
            return;

        m_TypeSearch.Add(id, data);
    }

    public static TypeSearchData GetTypeSearchData(int MissionID)
    {
        if(!m_TypeSearch.ContainsKey(MissionID))
            return null;

        return m_TypeSearch[MissionID];
    }

    public static void AddTypeUseItemData(int id, TypeUseItemData data)
    {
        if (m_TypeUseItem.ContainsKey(id))
            return;

        m_TypeUseItem.Add(id, data);
    }

    public static TypeUseItemData GetTypeUseItemData(int MissionID)
    {
        if(!m_TypeUseItem.ContainsKey(MissionID))
            return null;

        return m_TypeUseItem[MissionID];
    }

    public static void AddTypeMessengerData(int id, TypeMessengerData data)
    {
        if (m_TypeMessenger.ContainsKey(id))
            return;

        m_TypeMessenger.Add(id, data);
    }

    public static TypeMessengerData GetTypeMessengerData(int MissionID)
    {
        if(!m_TypeMessenger.ContainsKey(MissionID))
            return null;

        return m_TypeMessenger[MissionID];
    }

    public static void AddTypeTowerDefendsData(int id, TypeTowerDefendsData data)
    {
        if (m_TypeTowerDefends.ContainsKey(id))
            return;

        m_TypeTowerDefends.Add(id, data);
    }

    public static TypeTowerDefendsData GetTypeTowerDefendsData(int MissionID)
    {
        if(!m_TypeTowerDefends.ContainsKey(MissionID))
            return null;

        return m_TypeTowerDefends[MissionID];
    }

    public static void AddTDInfoData(int tdID, TowerDefendsInfoData data)
    {
        if (m_TDInfoMap.ContainsKey(tdID))
            return;

        m_TDInfoMap.Add(tdID, data);
    }

    public static TowerDefendsInfoData GetTDInfoData(int tdID)
    {
        if (!m_TDInfoMap.ContainsKey(tdID))
            return null;

        return m_TDInfoMap[tdID];
    }

    public static void DeleteRandomMissionData(int TargetID)
    {
        if(m_TypeMonster.ContainsKey(TargetID))
            m_TypeMonster.Remove(TargetID);

        if(m_TypeCollect.ContainsKey(TargetID))
            m_TypeCollect.Remove(TargetID);

        if(m_TypeFollow.ContainsKey(TargetID))
            m_TypeFollow.Remove(TargetID);

        if(m_TypeSearch.ContainsKey(TargetID))
            m_TypeSearch.Remove(TargetID);

        if(m_TypeUseItem.ContainsKey(TargetID))
            m_TypeUseItem.Remove(TargetID);

        if(m_TypeMessenger.ContainsKey(TargetID))
            m_TypeMessenger.Remove(TargetID);

        if(m_TypeTowerDefends.ContainsKey(TargetID))
            m_TypeTowerDefends.Remove(TargetID);
    }

    const int finishTalkID = 353; //lz-2018.1.19 "我把你叫我做的事办好了" ,这个需要加上任务名

    public static string GetMissionNpcListName(int MissionID, bool bspe)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return "";

        int id;
        if (bspe)
            id = data.m_PlayerTalk[1];
        else
            id = data.m_PlayerTalk[0];

        TalkData talkdata = TalkRespository.GetTalkData(id);
        if (talkdata == null)
            return "";
        if(id == finishTalkID)
        {
            //lz-2018.1.19 "我把你叫我做的事办好了" ,这个需要加上任务名
            return string.Format("{0} ({1})", talkdata.m_Content, data.m_MissionName);
        }
        return talkdata.m_Content;
    }

    public static string GetMissionName(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return "";

        return data.m_MissionName;
    }

    public static MissionType GetMissionType(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return MissionType.MissionType_Unkown;

        return data.m_Type;
    }

    public static TargetType GetTargetType(int MissionID)
    {
        return (TargetType)(MissionID / 1000);
    }

    public static bool HasTargetType(int MissionID, TargetType type)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return false;

        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TargetType curType = GetTargetType(data.m_TargetIDList[i]);
            if (curType == type)
                return true;
        }

        return false;
    }

    public static bool IsAutoReplyMission(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return false;

        if(data.m_iReplyNpc == 0)
            return true;

        return false;
    }

    public static bool IsMainMission(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return true;

        if (data.m_Type == MissionType.MissionType_Main)
            return true;

        return false;
    }

    public static bool NotUpdateMisTex(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return true;

        if(data.m_Type == MissionType.MissionType_Talk)
            return true;

        return false;
    }

    public static bool HaveTalkOP(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return false;

        if (data.m_TalkOP.Count <= 0)
            return false;
        
        return true;
    }

    public static bool HaveTalkIN(int MissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return false;

        if (data.m_TalkIN.Count <= 0)
            return false;

        return true;
    }

    public static bool HaveTalkED(int MissionID, int TargetID = -1)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionID);
        if (data == null)
            return false;

        TargetType curType;
        TypeSearchData seaData;
        TypeFollowData folData;

        if (data.m_TalkED.Count > 0)
            return true;

        if(TargetID == -1)
        {
            for (int i = 0; i < data.m_TargetIDList.Count; i++)
            {
                curType = GetTargetType(data.m_TargetIDList[i]);
                if (curType == TargetType.TargetType_Follow)
                {
                    folData = GetTypeFollowData(data.m_TargetIDList[i]);
                    if (folData == null)
                        return false;

                    if (folData.m_ComTalkID.Count > 0)
                        return true;
                }
                else if (curType == TargetType.TargetType_Discovery)
                {
                    seaData = GetTypeSearchData(data.m_TargetIDList[i]);
                    if (seaData == null)
                        return false;

                    if (seaData.m_TalkID.Count > 0)
                        return true;
                }
            }
        }
        else
        {
            curType = MissionRepository.GetTargetType(TargetID);
            if (curType == TargetType.TargetType_Follow)
            {
                folData = MissionManager.GetTypeFollowData(TargetID);
                if(folData == null)
                    return false;

                if(folData.m_ComTalkID.Count > 0)
                    return true;
            }
            else if (curType == TargetType.TargetType_Discovery)
            {
                seaData = GetTypeSearchData(TargetID);
                if (seaData == null)
                    return false;

                if (seaData.m_TalkID.Count > 0)
                    return true;
            }
        }

        return false;
    }

    public static void AddNpcMissionMap(int npcid, int id)
    {
        if (m_iNpcMissionMap.ContainsKey(npcid))
            m_iNpcMissionMap[npcid].Add(id);
        else
        {
            List<int> idlist = new List<int>();
            idlist.Add(id);
            m_iNpcMissionMap.Add(npcid, idlist);
        }
    }

    public static void AddNpcReplyMissionMap(int npcid, int id)
    {
        if (m_iNpcReplyMissionMap.ContainsKey(npcid))
            m_iNpcReplyMissionMap[npcid].Add(id);
        else
        {
            List<int> idlist = new List<int>();
            idlist.Add(id);
            m_iNpcReplyMissionMap.Add(npcid, idlist);
        }
    }

    public static void LoadMissionCommon()
    {
		SqliteDataReader reader = null;
#if DemoVersion
		reader = LocalDatabase.Instance.ReadFullTable("Quest_ListDemo");
#else
        reader = LocalDatabase.Instance.ReadFullTable("Quest_List");
#endif
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            MissionCommonData data = new MissionCommonData();
            int strid;
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("MissionName")));
//			if(strid==0)
//				data.m_MissionName="";
//			else
            	data.m_MissionName = PELocalization.GetString(strid);
            data.m_iNpc = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Npc")));
            data.m_iReplyNpc = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ReplyNpc")));
            AddNpcMissionMap(data.m_iNpc, data.m_ID);
            AddNpcReplyMissionMap(data.m_iReplyNpc, data.m_ID);
            data.m_Type = (MissionType)Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
            data.m_ScriptID = reader.GetString(reader.GetOrdinal("ScriptID"));
            data.m_MaxNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("MaxNum")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Description")));
            data.m_Description = PELocalization.GetString(strid);
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("VarDesc")));
            data.m_MulDesc = PELocalization.GetString(strid);
            data.m_VarValueID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("VarValueID")));
            data.m_VarValue = Convert.ToInt32(reader.GetString(reader.GetOrdinal("VarValue")));

            string subid = reader.GetString(reader.GetOrdinal("TargetIDList"));
            string[] idlist = subid.Split(',');
			
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                data.m_TargetIDList.Add(Convert.ToInt32(idlist[i]));
            }

            subid = reader.GetString(reader.GetOrdinal("PreLimit"));
            idlist = subid.Split(',');
            for (int i = 0; i < idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                if (i == 0)
                {
                    string[] conPre = idlist[i].Split(':');
                    if (conPre.Length == 2)
                    {
                        data.m_PreLimit.type = Convert.ToInt32(conPre[0]);
                        if (conPre[1] != "0")
                            data.m_PreLimit.idlist.Add(Convert.ToInt32(conPre[1]));
                    }
                    else if (conPre.Length == 1)
                    {
                        //如果前置任务只有1个，可以直接填id
                        if (idlist.Length == 1)
                        {
                            if (conPre[0] != "0")
                                data.m_PreLimit.idlist.Add(Convert.ToInt32(conPre[0]));
                        }
                    }
                }
                else
                {
                    if (idlist[i] != "0")
                        data.m_PreLimit.idlist.Add(Convert.ToInt32(idlist[i]));
                }
            }

            subid = reader.GetString(reader.GetOrdinal("AfterLimit"));
            idlist = subid.Split(',');
            for (int i = 0; i < idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                if (i == 0)
                {
                    string[] conPre = idlist[i].Split(':');
                    if (conPre.Length == 2)
                    {
                        data.m_AfterLimit.type = Convert.ToInt32(conPre[0]);
                        if (conPre[1] != "0")
                            data.m_AfterLimit.idlist.Add(Convert.ToInt32(conPre[1]));
                    }
                }
                else
                {
                    if (idlist[i] != "0")
                        data.m_AfterLimit.idlist.Add(Convert.ToInt32(idlist[i]));
                }
            }

            subid = reader.GetString(reader.GetOrdinal("MutexLimit"));
            idlist = subid.Split(',');
            for (int i = 0; i < idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                if (i == 0)
                {
                    string[] conPre = idlist[i].Split(':');
                    if (conPre.Length == 2)
                    {
                        data.m_MutexLimit.type = Convert.ToInt32(conPre[0]);
                        if (conPre[1] != "0")
                            data.m_MutexLimit.idlist.Add(Convert.ToInt32(conPre[1]));
                    }
                    else if (conPre.Length == 1)
                    {
                        //如果前置任务只有1个，可以直接填id
                        if (idlist.Length == 1)
                        {
                            if (conPre[0] != "0")
                                data.m_MutexLimit.idlist.Add(Convert.ToInt32(conPre[0]));
                        }
                    }
                }
                else
                {
                    if (idlist[i] != "0")
                        data.m_MutexLimit.idlist.Add(Convert.ToInt32(idlist[i]));
                }
            }

            subid = reader.GetString(reader.GetOrdinal("GuanLianList"));
            idlist = subid.Split(',');
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                data.m_GuanLianList.Add(Convert.ToInt32(idlist[i]));
            }

            subid = reader.GetString(reader.GetOrdinal("player_talk"));
            idlist = subid.Split(',');
            if (idlist.Length == 2)
            {
                data.m_PlayerTalk[0] = Convert.ToInt32(idlist[0]);
                data.m_PlayerTalk[1] = Convert.ToInt32(idlist[1]);
            }
            else if(idlist.Length == 1 && idlist[0] != "0")
            {
                data.m_PlayerTalk[0] = Convert.ToInt32(idlist[0]);
                data.m_PlayerTalk[1] = 0;
            }


            string[] strtemp;
            subid = reader.GetString(reader.GetOrdinal("Get_DemandItem"));
            idlist = subid.Split(',');
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                strtemp = idlist[i].Split('_');
                if (strtemp.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(strtemp[0]);
                tmp.num = Convert.ToInt32(strtemp[1]);
                data.m_Get_DemandItem.Add(tmp);
            }

            subid = reader.GetString(reader.GetOrdinal("Get_DeleteItem"));
            idlist = subid.Split(',');
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                strtemp = idlist[i].Split('_');
                if (strtemp.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(strtemp[0]);
                tmp.num = Convert.ToInt32(strtemp[1]);
                data.m_Get_DeleteItem.Add(tmp);
            }

            subid = reader.GetString(reader.GetOrdinal("Get_MissionItem"));
            idlist = subid.Split(',');
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                strtemp = idlist[i].Split('_');
                if (strtemp.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(strtemp[0]);
                tmp.num = Convert.ToInt32(strtemp[1]);
                data.m_Get_MissionItem.Add(tmp);
            }

            subid = reader.GetString(reader.GetOrdinal("CoRewardItem"));
            idlist = subid.Split(',');
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                strtemp = idlist[i].Split('_');
                if (strtemp.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(strtemp[0]);
                tmp.num = Convert.ToInt32(strtemp[1]);
                data.m_Com_RewardItem.Add(tmp);
            }

            subid = reader.GetString(reader.GetOrdinal("CoVarRewardItem"));
            idlist = subid.Split(';');
            for (int i = 0; i < idlist.Length; i++)
            {
                List<MissionIDNum> idnumList = new List<MissionIDNum>();
                string[] idlist1 = idlist[i].Split(',');
                for (int j = 0; j < idlist1.Length; j++)
                {
                    if (idlist1[j] == "0")
                        continue;

                    strtemp = idlist1[j].Split('_');
                    if (strtemp.Length != 2)
                        continue;

                    MissionIDNum tmp;
                    tmp.id = Convert.ToInt32(strtemp[0]);
                    tmp.num = Convert.ToInt32(strtemp[1]);
                    idnumList.Add(tmp);
                }
                data.m_Com_MulRewardItem.Add(i, idnumList);
            }

            subid = reader.GetString(reader.GetOrdinal("CoSelRewardItem"));
            idlist = subid.Split(',');
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                strtemp = idlist[i].Split('_');
                if (strtemp.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(strtemp[0]);
                tmp.num = Convert.ToInt32(strtemp[1]);
                data.m_Com_SelRewardItem.Add(tmp);
            }

            subid = reader.GetString(reader.GetOrdinal("CoRemoveItem"));
            idlist = subid.Split(',');
            for(int i=0; i<idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                strtemp = idlist[i].Split('_');
                if (strtemp.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(strtemp[0]);
                tmp.num = Convert.ToInt32(strtemp[1]);
                data.m_Com_RemoveItem.Add(tmp);
            }

            subid = reader.GetString(reader.GetOrdinal("TalkOP"));
            idlist = subid.Split(':');
            if (idlist.Length > 1)
            {
                for (int i = 1; i < idlist.Length; i++)
                    data.m_OPID.Add(Convert.ToInt32(idlist[i]));
            }

            strtemp = idlist[0].Split(',');
            for (int i = 0; i < strtemp.Length; i++)
            {
                if (strtemp[i] == "0")
                    continue;

                data.m_TalkOP.Add(Convert.ToInt32(strtemp[i]));
            }

            subid = reader.GetString(reader.GetOrdinal("TalkIN"));
            idlist = subid.Split(':');
            if (idlist.Length > 1)
            {
                for (int i = 1; i < idlist.Length; i++)
                    data.m_INID.Add(Convert.ToInt32(idlist[i]));
            }

            strtemp = idlist[0].Split(',');
            for (int i = 0; i < strtemp.Length; i++)
            {
                if (strtemp[i] == "0")
                    continue;

                data.m_TalkIN.Add(Convert.ToInt32(strtemp[i]));
            }

            subid = reader.GetString(reader.GetOrdinal("TalkED"));
            idlist = subid.Split(':');
            if (idlist.Length > 1)
            {
                for (int i = 1; i < idlist.Length; i++)
                    data.m_EDID.Add(Convert.ToInt32(idlist[i]));
            }

            strtemp = idlist[0].Split(',');
            for (int i = 0; i < strtemp.Length; i++)
            {
                if (strtemp[i] == "0")
                    continue;

                data.m_TalkED.Add(Convert.ToInt32(strtemp[i]));
            }

            if(Convert.ToInt32(reader.GetString(reader.GetOrdinal("bGiveUp"))) == 0)
                data.m_bGiveUp = false;
            else
                data.m_bGiveUp = true;

            subid = reader.GetString(reader.GetOrdinal("resetid"));
            idlist = subid.Split(',');
            for (int i = 0; i < idlist.Length; i++)
            {
                if (idlist[i] == "0")
                    continue;

                data.m_ResetID.Add(Convert.ToInt32(idlist[i]));
            }


            subid = reader.GetString(reader.GetOrdinal("deleteQid"));
            idlist = subid.Split(',');
            for (int i = 0; i < idlist.Length; i++)
            {
                if (idlist[i] == "0")
                    continue;

                data.m_DeleteID.Add(Convert.ToInt32(idlist[i]));
            }

            subid = reader.GetString(reader.GetOrdinal("TalkOP_SP"));
            idlist = subid.Split(',');

            for (int i = 0; i < idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                data.m_PromptOP.Add(Convert.ToInt32(idlist[i]));
            }
            

            subid = reader.GetString(reader.GetOrdinal("TalkIN_SP"));
            idlist = subid.Split(',');

            for (int i = 0; i < idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                data.m_PromptIN.Add(Convert.ToInt32(idlist[i]));
            }
            

            subid = reader.GetString(reader.GetOrdinal("TalkED_SP"));
            idlist = subid.Split(',');

            for (int i = 0; i < idlist.Length; i++)
            {
                if(idlist[i] == "0")
                    continue;

                data.m_PromptED.Add(Convert.ToInt32(idlist[i]));
            }
            

            subid = reader.GetString(reader.GetOrdinal("StoryInfo"));
            idlist = subid.Split(',');
            for (int i = 0; i < idlist.Length; i++)
            {
                strtemp = idlist[i].Split('_');
                if(strtemp.Length != 2)
                    continue;

                StoryInfo si;
                si.type = (Story_Info)Convert.ToInt32(strtemp[0]);
                si.storyid = Convert.ToInt32(strtemp[1]);

                data.m_StoryInfo.Add(si);
            }

            subid = reader.GetString(reader.GetOrdinal("NeedTime"));
            idlist = subid.Split('_');
            data.m_NeedTime = Convert.ToInt32(idlist[0]);
            if(idlist.Length == 2)
                data.m_timeOverToPlot = Convert.ToInt32(idlist[1]);

            subid = reader.GetString(reader.GetOrdinal("ColonyNPC"));
            idlist = subid.Split(',');
            for (int i = 0; i < idlist.Length; i++)
            {
                if (idlist[i] == "0")
                    continue;


                data.m_iColonyNpcList.Add(Convert.ToInt32(idlist[i]));
            }

            subid = reader.GetString(reader.GetOrdinal("NPCmode"));
            idlist = subid.Split(',');
            if (idlist.Length == 2)
            {
                data.m_ColonyMis[0] = Convert.ToInt32(idlist[0]);
                data.m_ColonyMis[1] = Convert.ToInt32(idlist[1]);
            }

            subid = reader.GetString(reader.GetOrdinal("ReputationPuja"));
            idlist = subid.Split('_');
            if (idlist.Length == 3)
            {
                ReputationPreLimit limit = new ReputationPreLimit();
                limit.type = Convert.ToInt32(idlist[0]);
                limit.min = Convert.ToInt32(idlist[1]);
                limit.max = Convert.ToInt32(idlist[2]);
                limit.campID = 5;
                data.m_reputationPre.Add(limit);
            }

            subid = reader.GetString(reader.GetOrdinal("ReputationPaja"));
            idlist = subid.Split('_');
            if (idlist.Length == 3)
            {
                ReputationPreLimit limit = new ReputationPreLimit();
                limit.type = Convert.ToInt32(idlist[0]);
                limit.min = Convert.ToInt32(idlist[1]);
                limit.max = Convert.ToInt32(idlist[2]);
                limit.campID = 6;
                data.m_reputationPre.Add(limit);
            }

            subid = reader.GetString(reader.GetOrdinal("NPCType"));
            idlist = subid.Split(';');
            foreach (var item in idlist)
            {
                if (item.Equals("0"))
                    continue;
                NpcType nt;
                nt.npcs = new List<int>();
                nt.type = -1;
                string[] conPre = item.Split('_');
                if (conPre.Length != 2)
                    continue;
                foreach (var item1 in conPre[0].Split(','))
                {
                    nt.npcs.Add(Convert.ToInt32(item1));
                }
                nt.type = Convert.ToInt32(conPre[1]);
                data.m_npcType.Add(nt);
            }

            subid = reader.GetString(reader.GetOrdinal("TempLimit"));
            idlist = subid.Split(',');
            foreach (var item in idlist)
            {
                if (item.Equals("0"))
                    continue;
                data.m_tempLimit.Add(Convert.ToInt32(item));
            }

            data.m_replyIconId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ReplyIcon")));

            subid = reader.GetString(reader.GetOrdinal("FailNPCType"));
            idlist = subid.Split(';');
            foreach (var item in idlist)
            {
                strtemp = item.Split('_');
                if (strtemp.Length != 2)
                    continue;
                NpcType nt = new NpcType();
                nt.npcs = new List<int>();
                nt.type = Convert.ToInt32(strtemp[1]);
                foreach (var item1 in strtemp[0].Split(','))
                    nt.npcs.Add(Convert.ToInt32(item1));

                data.m_failNpcType.Add(nt);
            }

            m_MissionCommonMap.Add(data.m_ID, data);
        }
    }

    public static void LoadTypeMonster()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_KillMonster");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeMonsterData data = new TypeMonsterData();
            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Desc")));
            data.m_Desc = PELocalization.GetString(strid);
            data.m_mustByPlayer = reader.GetString(reader.GetOrdinal("TargetID")).Equals("1") ? true : false;

            string strTmp = reader.GetString(reader.GetOrdinal("MonsterID"));
            string[] tmpList = strTmp.Split(';');
            string[] tmpList1;
            string[] tmpList2;
            for(int i=0; i<tmpList.Length; i++)
            {
                tmpList1 = tmpList[i].Split('_');
                if(tmpList1.Length != 3)
                    continue;

                NpcType idnum;
                idnum.npcs = new List<int>();
                tmpList2 = tmpList1[0].Split(',');
                for (int j = 0; j < tmpList2.Length; j++)
                    idnum.npcs.Add(Convert.ToInt32(tmpList2[j]));
                idnum.type = Convert.ToInt32(tmpList1[1]);
                data.type = Convert.ToInt32(tmpList1[2]);

                data.m_MonsterList.Add(idnum);
            }

            strTmp = reader.GetString(reader.GetOrdinal("TargetPos"));
            tmpList = strTmp.Split(',');
            if(tmpList.Length == 3)
            {
                float x = Convert.ToSingle(tmpList[0]);
                float y = Convert.ToSingle(tmpList[1]);
                float z = Convert.ToSingle(tmpList[2]);
                data.m_TargetPos = new Vector3(x, y, z);
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            tmpList = strTmp.Split(',');
            for(int i=0; i<tmpList.Length; i++)
            {
                if(tmpList[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(tmpList[i]));
            }

            m_TypeMonster.Add(data.m_TargetID,data);
        }
    }

    public static void LoadTypeCollect()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_LootItem");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeCollectData data = new TypeCollectData();
            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Desc")));
            data.m_Desc = PELocalization.GetString(strid);
            data.m_Type = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
            data.ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemID")));
            data.ItemNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemNum")));
            data.m_TargetItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetItemID")));
            data.m_MaxNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("MaxNum")));
            data.m_Chance = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Chance")));

            string tmp = reader.GetString(reader.GetOrdinal("RandNum"));
            string[] strs = tmp.Split(',');
            if (strs.Length == 3)
            {
                data.m_randItemNum[0] = Convert.ToInt32(strs[0]);
                data.m_randItemNum[1] = Convert.ToInt32(strs[1]);
                data.m_randItemNum[2] = Convert.ToInt32(strs[2]);
            }

            tmp = reader.GetString(reader.GetOrdinal("RandID"));
            strs = tmp.Split(',');
            for (int i = 0; i < strs.Length; i++)
            {
                int n = Convert.ToInt32(strs[i]);
                if (n != 0)
                    data.m_randItemID.Add(n);
            }
            //data.m_randItemNum

            string strPos = reader.GetString(reader.GetOrdinal("TargetPos"));
            string[] posList = strPos.Split(',','_');
            if(posList.Length == 4)
            {
                float x = Convert.ToSingle(posList[0]);
                float y = Convert.ToSingle(posList[1]);
                float z = Convert.ToSingle(posList[2]);
                data.m_TargetPos = new Vector3(x, y, z);
                data.m_TargetRadius = Convert.ToInt32(posList[3]);
            }

            string strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            string [] listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            m_TypeCollect.Add(data.m_TargetID, data);
        }
    }

    public static void LoadTypeFollow()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_HuSong");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeFollowData data = new TypeFollowData();
            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Desc")));
            data.m_Desc = PELocalization.GetString(strid);
            float x,y,z;
            string strTmp = reader.GetString(reader.GetOrdinal("NpcList"));
            string[] listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                strid = Convert.ToInt32(listTmp[i]);
                data.m_iNpcList.Add(strid);
            }

            data.m_SceneType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Scene")));

            listTmp = reader.GetString(reader.GetOrdinal("Emode")).Split('_');
            if (listTmp.Length == 2)
            {
                data.m_EMode = Convert.ToInt32(listTmp[0]);
                data.m_isAttack = Convert.ToInt32(listTmp[1]);
            }

            data.m_LookNameID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("LookName")));

            strTmp = reader.GetString(reader.GetOrdinal("DistPos"));
            listTmp = strTmp.Split(',');
            if(listTmp.Length == 3)
            {
                x = Convert.ToSingle(listTmp[0]);
                y = Convert.ToSingle(listTmp[1]);
                z = Convert.ToSingle(listTmp[2]);
                data.m_DistPos = new Vector3(x, y, z);
            }

            data.m_DistRadius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("DistRadius")));
            data.m_TrackRadius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TrackRadius")));

            strTmp = reader.GetString(reader.GetOrdinal("ResetPos"));
            listTmp = strTmp.Split(',');
            if(listTmp.Length == 3)
            {
                x = Convert.ToSingle(listTmp[0]);
                y = Convert.ToSingle(listTmp[1]);
                z = Convert.ToSingle(listTmp[2]);
                data.m_ResetPos = new Vector3(x, y, z);
            }

            strTmp = reader.GetString(reader.GetOrdinal("FailResetPos"));
            listTmp = strTmp.Split(',');
            if(listTmp.Length == 3)
            {
                x = Convert.ToSingle(listTmp[0]);
                y = Convert.ToSingle(listTmp[1]);
                z = Convert.ToSingle(listTmp[2]);
                data.m_FailResetPos = new Vector3(x, y, z);
            }

            string[] listTmp1, listTmp2;
            strTmp = reader.GetString(reader.GetOrdinal("TalkInfo"));
            listTmp = strTmp.Split(':');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                listTmp1 = listTmp[i].Split('_');
                if(listTmp1.Length != 3)
                    continue;

                listTmp2 = listTmp1[0].Split(',');
                if(listTmp2.Length != 3)
                    continue;

                TalkInfo talkInfo = new TalkInfo();

                x = Convert.ToSingle(listTmp2[0]);
                y = Convert.ToSingle(listTmp2[1]);
                z = Convert.ToSingle(listTmp2[2]);
                talkInfo.pos = new Vector3(x, y, z);

                talkInfo.radius = Convert.ToInt32(listTmp1[1]);

                listTmp2 = listTmp1[2].Split(',');

                for(int j=0; j<listTmp2.Length; j++)
                {
                    if(listTmp2[j] == "0")
                        continue;

                    talkInfo.talkid.Add(Convert.ToInt32(listTmp2[j]));
                }

                data.m_TalkInfo.Add(talkInfo);
            }


            strTmp = reader.GetString(reader.GetOrdinal("WaitTalkList"));
            if (!strTmp.Equals("0"))
            {
                listTmp = strTmp.Split(';');
                foreach (var item in listTmp)
                {
                    listTmp1 = item.Split('_', ',');
                    if (listTmp1.Length != 3)
                        continue;
                    data.npcid_behindTalk_forwardTalk.Add(Convert.ToInt32(listTmp1[0]), new int[2] { Convert.ToInt32(listTmp1[1]), Convert.ToInt32(listTmp1[2]) });
                }
            }

            strTmp = reader.GetString(reader.GetOrdinal("PathList"));
            listTmp = strTmp.Split(';');
            for (int i = 0; i < listTmp.Length; i++)
            {
                listTmp1 = listTmp[i].Split(',');
                if (listTmp1.Length != 3)
                    continue;

                x = Convert.ToSingle(listTmp1[0]);
                y = Convert.ToSingle(listTmp1[1]);
                z = Convert.ToSingle(listTmp1[2]);

                data.m_PathList.Add(new Vector3(x, y, z));
            }

            data.m_isNeedReturn = reader.GetString(reader.GetOrdinal("NeedReturn")) == "0" ? false : true;
            
            strTmp = reader.GetString(reader.GetOrdinal("Monster"));
            listTmp = strTmp.Split(':');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                listTmp1 = listTmp[i].Split('_');
                if (listTmp1.Length != 4)
                    continue;

                MonsterIDNum tmp = new MonsterIDNum();
                tmp.id = Convert.ToInt32(listTmp1[0]);
                tmp.num = Convert.ToInt32(listTmp1[1]);
                tmp.radius = Convert.ToInt32(listTmp1[2]);

                listTmp2 = listTmp1[3].Split(',');
                if(listTmp2.Length == 3)
                {
                    x = Convert.ToSingle(listTmp2[0]);
                    y = Convert.ToSingle(listTmp2[1]);
                    z = Convert.ToSingle(listTmp2[2]);
                    tmp.pos = new Vector3(x, y, z);
                }

                data.m_Monster.Add(tmp);
            }

            strTmp = reader.GetString(reader.GetOrdinal("ComTalkID"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ComTalkID.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("FailNpc"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_iFailNpc.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            //data.m_FollowDist = Convert.ToSingle(reader.GetString(reader.GetOrdinal("FollowDist")));
            strTmp = reader.GetString(reader.GetOrdinal("WaitDist"));
            listTmp = strTmp.Split(',');
            if (listTmp.Length == 2) {
                data.m_WaitDist.Add(Convert.ToInt32(listTmp[0]));
                data.m_WaitDist.Add(Convert.ToInt32(listTmp[1]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("NeedPlayer"));
            data.m_isNeedPlayer = Convert.ToInt32(strTmp) == 1 ? true : false;

            m_TypeFollow.Add(data.m_TargetID, data);
        }
    }

    public static void LoadTypeSearch()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_EnterArea");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeSearchData data = new TypeSearchData();
            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Desc")));
            data.m_Desc = PELocalization.GetString(strid);
            data.m_NpcID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("LookName")));
            float x,y,z;
            string strTmp = reader.GetString(reader.GetOrdinal("DistPos"));
            string[] listTmp = strTmp.Split(',');
            if(listTmp.Length == 3)
            {
                x = Convert.ToSingle(listTmp[0]);
                y = Convert.ToSingle(listTmp[1]);
                z = Convert.ToSingle(listTmp[2]);
                data.m_DistPos = new Vector3(x, y, z);
            }

            data.m_SceneType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Scene")));

            data.m_DistRadius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("DistRadius")));
            data.m_TrackRadius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TrackRadius")));

            strTmp = reader.GetString(reader.GetOrdinal("Prompt"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_Prompt.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("TalkID"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_TalkID.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            m_TypeSearch.Add(data.m_TargetID, data);
        }
    }

    public static void LoadTypeUseItem()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_UseItem");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeUseItemData data = new TypeUseItemData();
            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Desc")));
            data.m_Desc = PELocalization.GetString(strid);
            data.m_Type = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
            data.m_ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemID")));
            data.m_UseNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("UseNum")));

            float x,y,z;
            string strTmp = reader.GetString(reader.GetOrdinal("Pos"));
            string[] listTmp = strTmp.Split(',');
            if(listTmp.Length == 3)
            {
                x = Convert.ToSingle(listTmp[0]);
                y = Convert.ToSingle(listTmp[1]);
                z = Convert.ToSingle(listTmp[2]);
                data.m_Pos = new Vector3(x, y, z);
            }

            data.m_Radius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Radius")));

            strTmp = reader.GetString(reader.GetOrdinal("UsedPrompt"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_UsedPrompt.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("TalkID"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_TalkID.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("FailPrompt"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_FailPrompt.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            data.m_allowOld = reader.GetString(reader.GetOrdinal("AllowOld")) == "1" ? true : false;
            
            m_TypeUseItem.Add(data.m_TargetID, data);
        }
    }

    public static void LoadTypeMessenger()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_Delivery");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeMessengerData data = new TypeMessengerData();
//            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            data.m_iNpc = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Npc")));
            data.m_iReplyNpc = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ReplyNpc")));
            data.m_ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemID")));
            data.m_ItemNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemNum")));

            string strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            string[] listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            m_TypeMessenger.Add(data.m_TargetID, data);
        }
    }

    public static void LoadTypeTowerDefends()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_TowerDefence");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeTowerDefendsData data = new TypeTowerDefendsData();
            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Desc")));
            data.m_Desc = PELocalization.GetString(strid);
            data.m_Time = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Time")));

            float x,y,z;
            string strTmp = reader.GetString(reader.GetOrdinal("Pos"));
            string[] listTmp = strTmp.Split('_');
            string[] listTmp1;
            if (listTmp.Length == 2)
            {
                data.m_Pos.type = (TypeTowerDefendsData.PosType)Convert.ToInt32(listTmp[0]);
                if (data.m_Pos.type != TypeTowerDefendsData.PosType.getPos && listTmp.Length == 2)
                {
                    listTmp1 = listTmp[1].Split(',');
                    if (listTmp1.Length == 3)
                    {
                        x = Convert.ToSingle(listTmp1[0]);
                        y = Convert.ToSingle(listTmp1[1]);
                        z = Convert.ToSingle(listTmp1[2]);
                        data.m_Pos.pos = new Vector3(x, y, z);
                    }
                    else
                        data.m_Pos.id = Convert.ToInt32(listTmp1[0]);
                }
            }
            else if (listTmp.Length == 1)
            {
                data.m_Pos.type = TypeTowerDefendsData.PosType.pos;
                listTmp = strTmp.Split(',');
                if (listTmp.Length == 3) 
                {
                    x = Convert.ToSingle(listTmp[0]);
                    y = Convert.ToSingle(listTmp[1]);
                    z = Convert.ToSingle(listTmp[2]);
                    data.m_Pos.pos = new Vector3(x, y, z);
                }
            }

            strTmp = reader.GetString(reader.GetOrdinal("NpcList"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

				if (!int.TryParse(listTmp[i], out strid))
					continue;

				data.m_iNpcList.Add(strid);
            }

            strTmp = reader.GetString(reader.GetOrdinal("ObjectList"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ObjectList.Add(Convert.ToInt32(listTmp[i]));
            }

            data.m_Count = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Count")));

            data.m_tolTime = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TolTime")));

            data.m_range = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Range")));

            data.m_TdInfoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("td_id")));

            strTmp = reader.GetString(reader.GetOrdinal("SweepID"));
            listTmp = strTmp.Split(',');
            foreach (var item in listTmp)
            {
                if (item == "0")
                    continue;
                data.m_SweepId.Add(Convert.ToInt32(item));
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            m_TypeTowerDefends.Add(data.m_TargetID, data);
        }
    }

    public static void LoadTDInfoData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Quest_TD_Info");
        reader.Read();
        string strTmp;
        string[] listTmp, listTmp1;
        while (reader.Read())
        {
            TowerDefendsInfoData data = new TowerDefendsInfoData();
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("td_id")));
            data.m_delay = Convert.ToInt32(reader.GetString(reader.GetOrdinal("delay")));

            strTmp = reader.GetString(reader.GetOrdinal("Info"));
            
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                listTmp1 = listTmp[i].Split('_');
                if(listTmp1.Length != 4)
                    continue;

                TDInfo info = new TDInfo();
                info.monsterID = Convert.ToInt32(listTmp1[0]);
                info.num = Convert.ToInt32(listTmp1[1]);
                info.dir_min = Convert.ToInt32(listTmp1[2]);
                info.dir_max = Convert.ToInt32(listTmp1[3]);
                data.m_TdInfoList.Add(info);
            }

            m_TDInfoMap.Add(data.m_ID, data);
        }
    }

    public static void LoadData()
    {
        LoadMissionCommon();
        LoadTypeMonster();
        LoadTypeCollect();
        LoadTypeFollow();
        LoadTypeSearch();
        LoadTypeUseItem();
        LoadTypeMessenger();
        LoadTypeTowerDefends();
        LoadTDInfoData();
    }
}