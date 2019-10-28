using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mono.Data.SqliteClient;
using System.IO;

public struct GroupInfo
{
    public int id;
    public int radius;
}

public class AdRandomGroup
{
    public int m_ID;
    public bool m_requstAll;
    public List<int> m_preLimit;
    public int m_FinishTimes;
    public int m_Area;
    public bool m_IsMultiMode;
    public Dictionary<int, List<GroupInfo>> m_GroupList;

    public AdRandomGroup()
    {
        m_preLimit = new List<int>();
        m_GroupList = new Dictionary<int, List<GroupInfo>>();
    }
}


public class AdRMRepository
{
    public static Dictionary<int, TypeMonsterData> m_AdTypeMonster = new Dictionary<int, TypeMonsterData>();
    public static Dictionary<int, TypeCollectData> m_AdTypeCollect = new Dictionary<int, TypeCollectData>();
    public static Dictionary<int, TypeFollowData> m_AdTypeFollow = new Dictionary<int, TypeFollowData>();
    public static Dictionary<int, TypeSearchData> m_AdTypeSearch = new Dictionary<int, TypeSearchData>();
    public static Dictionary<int, TypeUseItemData> m_AdTypeUseItem = new Dictionary<int, TypeUseItemData>();
    public static Dictionary<int, TypeMessengerData> m_AdTypeMessenger = new Dictionary<int, TypeMessengerData>();
    public static Dictionary<int, TowerDefendsInfoData> m_AdTDInfoMap = new Dictionary<int, TowerDefendsInfoData>();
    public static Dictionary<int, TypeTowerDefendsData> m_AdTypeTowerDefends = new Dictionary<int, TypeTowerDefendsData>();

    public static Dictionary<int, MissionCommonData> m_AdRandMisMap = new Dictionary<int, MissionCommonData>();
    public static Dictionary<int, RandomField> m_AdRandomFieldMap = new Dictionary<int, RandomField>();
    public static Dictionary<int, AdRandomGroup> m_AdRandomGroup = new Dictionary<int, AdRandomGroup>();

    public static AdRandomGroup GetAdRandomGroup(int id)
    {
        if (m_AdRandomGroup.ContainsKey(id))
            return m_AdRandomGroup[id];

        return null;
    }

    public static MissionCommonData GetAdRandomMission(int id)
    {
        if (m_AdRandMisMap.ContainsKey(id))
            return m_AdRandMisMap[id];

        return null;
    }

    public static bool HasAdRandomMission(int misid)
    {
        return m_AdRandMisMap.ContainsKey(misid);
    }

    public static void AddAdTypeMonsterData(int id, TypeMonsterData data)
    {
        if (m_AdTypeMonster.ContainsKey(id))
            return;

        m_AdTypeMonster.Add(id, data);
    }

    public static TypeMonsterData GetAdTypeMonsterData(int MissionID)
    {
        if (!m_AdTypeMonster.ContainsKey(MissionID))
            return null;

        return m_AdTypeMonster[MissionID];
    }

    public static void AddAdTypeCollectData(int id, TypeCollectData data)
    {
        if (m_AdTypeCollect.ContainsKey(id))
            return;

        m_AdTypeCollect.Add(id, data);
    }

    public static TypeCollectData GetAdTypeCollectData(int MissionID)
    {
        if (!m_AdTypeCollect.ContainsKey(MissionID))
            return null;

        return m_AdTypeCollect[MissionID];
    }

    public static void AddAdTypeFollowData(int id, TypeFollowData data)
    {
        if (m_AdTypeFollow.ContainsKey(id))
            return;

        m_AdTypeFollow.Add(id, data);
    }

    public static TypeFollowData GetAdTypeFollowData(int MissionID)
    {
        if (!m_AdTypeFollow.ContainsKey(MissionID))
            return null;

        return m_AdTypeFollow[MissionID];
    }

    public static void AddAdTypeSearchData(int id, TypeSearchData data)
    {
        if (m_AdTypeSearch.ContainsKey(id))
            return;

        m_AdTypeSearch.Add(id, data);
    }

    public static TypeSearchData GetAdTypeSearchData(int MissionID)
    {
        if (!m_AdTypeSearch.ContainsKey(MissionID))
            return null;

        return m_AdTypeSearch[MissionID];
    }

    public static void AddAdTypeUseItemData(int id, TypeUseItemData data)
    {
        if (m_AdTypeUseItem.ContainsKey(id))
            return;

        m_AdTypeUseItem.Add(id, data);
    }

    public static TypeUseItemData GetAdTypeUseItemData(int MissionID)
    {
        if (!m_AdTypeUseItem.ContainsKey(MissionID))
            return null;

        return m_AdTypeUseItem[MissionID];
    }

    public static void AddAdTypeMessengerData(int id, TypeMessengerData data)
    {
        if (m_AdTypeMessenger.ContainsKey(id))
            return;

        m_AdTypeMessenger.Add(id, data);
    }

    public static TypeMessengerData GetAdTypeMessengerData(int MissionID)
    {
        if (!m_AdTypeMessenger.ContainsKey(MissionID))
            return null;

        return m_AdTypeMessenger[MissionID];
    }

    public static void AddAdTypeTowerDefendsData(int id, TypeTowerDefendsData data)
    {
        if (m_AdTypeTowerDefends.ContainsKey(id))
            return;

        m_AdTypeTowerDefends.Add(id, data);
    }

    public static TypeTowerDefendsData GetAdTypeTowerDefendsData(int MissionID)
    {
        if (!m_AdTypeTowerDefends.ContainsKey(MissionID))
            return null;

        return m_AdTypeTowerDefends[MissionID];
    }

    public static int GetRandomMission(int qcid, int groupidx)
    {
        AdRandomGroup agi = GetAdRandomGroup(qcid);
        if (agi == null)
            return 0;

        if (!agi.m_GroupList.ContainsKey(groupidx))
            return 0;

        List<GroupInfo> giList = agi.m_GroupList[groupidx];

        if (giList.Count == 0)
            return 0;

        int count = giList[giList.Count - 1].radius;

        int idx = UnityEngine.Random.Range(0, count);

        for (int i = 0; i < giList.Count; i++)
        {
            GroupInfo gi = giList[i];
            if (idx < gi.radius)
                return gi.id;
        }

        return 0;
    }

    public static int GetRandomMission(NpcMissionData missionData)
    {
        AdRandomGroup agi = GetAdRandomGroup(missionData.m_QCID);
        if (agi == null)
            return 0;
        else
        {
            if (missionData.m_CurMissionGroup > 11)
            {
                missionData.m_CurGroupTimes++;
                if (!agi.m_GroupList.ContainsKey(missionData.m_CurMissionGroup))
                    missionData.m_CurMissionGroup = -1;

                if (agi.m_FinishTimes == 0)
                    missionData.m_CurMissionGroup = -1;

                if (agi.m_FinishTimes == -1)
                    missionData.m_CurMissionGroup = 1;

                if (agi.m_FinishTimes > 0)
                {
                    if (agi.m_FinishTimes > missionData.m_CurGroupTimes)
                        missionData.m_CurMissionGroup = 1;
                    else
                        missionData.m_CurMissionGroup = -1;
                }
            }

            if (missionData.m_CurMissionGroup != -1)
                return GetRandomMission(missionData.m_QCID, missionData.m_CurMissionGroup);

            return 0;
        }
    }

    public static void LoadAdTypeMonster()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuest_KillMonster");
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

            string strTmp = reader.GetString(reader.GetOrdinal("MonsterID"));
            string[] tmpList = strTmp.Split(';');
            string[] tmpList1;
            string[] tmpList2;
            for (int i = 0; i < tmpList.Length; i++)
            {
                tmpList1 = tmpList[i].Split('_');
                if (tmpList1.Length != 2)
                    continue;

                NpcType idnum;
                idnum.npcs = new List<int>();
                tmpList2 = tmpList1[0].Split(',');
                for (int j = 0; j < tmpList2.Length; j++)
                    idnum.npcs.Add(Convert.ToInt32(tmpList2[j]));
                idnum.type = Convert.ToInt32(tmpList1[1]);

                data.m_MonsterList.Add(idnum);
            }

//            MissionRand mr = new MissionRand();
            strTmp = reader.GetString(reader.GetOrdinal("TargetPos"));
            tmpList = strTmp.Split('_');
            if (tmpList.Length == 4)
            {
                //AdMissionRand mr = new AdMissionRand();
                data.m_mr.refertoType = (ReferToType)Convert.ToInt32(tmpList[0]);
                data.m_mr.referToID = Convert.ToInt32(tmpList[1]);
                data.m_mr.radius1 = Convert.ToInt32(tmpList[2]);
                data.m_mr.radius2 = Convert.ToInt32(tmpList[3]);
            }

            strTmp = reader.GetString(reader.GetOrdinal("PathIDTemp"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                tmpList1 = tmpList[i].Split('_');
                if (tmpList1.Length != 3)
                    continue;
                CreMons mon = new CreMons();
                mon.type = Convert.ToInt32(tmpList1[0]);
                mon.monID = Convert.ToInt32(tmpList1[1]);
                mon.monNum = Convert.ToInt32(tmpList1[2]);

                data.m_CreMonList.Add(mon);
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            tmpList = strTmp.Split(',');
            for(int i=0; i<tmpList.Length; i++)
            {
                if(tmpList[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(tmpList[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("DestroyTown"));
            tmpList = strTmp.Split('_');
            if (tmpList.Length >= 2)
            {
                data.m_destroyTown = true;
                data.m_campID.Add(Convert.ToInt32(tmpList[0]));
                data.m_townNum.Add(Convert.ToInt32(tmpList[1]));
                data.m_campID.Add(Convert.ToInt32(tmpList[2]));
                data.m_townNum.Add(Convert.ToInt32(tmpList[3]));
            }

            AddAdTypeMonsterData(data.m_TargetID, data);
        }
    }

    public static void LoadAdTypeCollect()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuest_LootItem");
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

            string strTmp = reader.GetString(reader.GetOrdinal("TargetPos"));
            string[] tmpList = strTmp.Split(',');
            if (tmpList.Length == 2)
            {
                data.m_AdDist = Convert.ToInt32(tmpList[0]);
                data.m_AdRadius = Convert.ToInt32(tmpList[1]);
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            tmpList = strTmp.Split(',');
            for(int i=0; i<tmpList.Length; i++)
            {
                if(tmpList[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(tmpList[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("RandNum"));
            string[] strs = strTmp.Split(',');
            if (strs.Length == 3)
            {
                data.m_randItemNum[0] = Convert.ToInt32(strs[0]);
                data.m_randItemNum[1] = Convert.ToInt32(strs[1]);
                data.m_randItemNum[2] = Convert.ToInt32(strs[2]);
            }

            strTmp = reader.GetString(reader.GetOrdinal("RandID"));
            strs = strTmp.Split(',');
            for (int i = 0; i < strs.Length; i++)
            {
                int n = Convert.ToInt32(strs[i]);
                if (n != 0)
                    data.m_randItemID.Add(n);
            }

            AddAdTypeCollectData(data.m_TargetID, data);
        }
    }

    public static void LoadAdTypeFollow()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuest_HuSong");
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
            float x, y, z;
            string[] listTmp;
            string strTmp = reader.GetString(reader.GetOrdinal("NpcRadius"));
            if (strTmp != "0")
            {
                listTmp = strTmp.Split(',');
                AdNpcInfo cni;
                cni.num = 1;
                cni.dist = Convert.ToInt32(listTmp[0]);
                if (listTmp.Length == 2)
                    cni.num = Convert.ToInt32(listTmp[1]);

                data.m_AdNpcRadius = cni;
            }


            listTmp = reader.GetString(reader.GetOrdinal("Emode")).Split('_');
            if (listTmp.Length == 2)
            {
                data.m_EMode = Convert.ToInt32(listTmp[0]);
                data.m_isAttack = Convert.ToInt32(listTmp[1]);
            }

            AdMissionRand mr1 = new AdMissionRand();
            strTmp = reader.GetString(reader.GetOrdinal("DistPos"));
            listTmp = strTmp.Split('_');
            if (listTmp.Length == 4)
            {
                mr1.refertoType = (ReferToType)Convert.ToInt32(listTmp[0]);
                mr1.referToID = Convert.ToInt32(listTmp[1]);
                mr1.radius1 = Convert.ToInt32(listTmp[2]);
                mr1.radius2 = Convert.ToInt32(listTmp[3]);
            }
            data.m_AdDistPos = mr1;

            data.m_TrackRadius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TrackRadius")));

            string[] listTmp1, listTmp2, listTmp3;
            strTmp = reader.GetString(reader.GetOrdinal("TalkInfo"));
            if (strTmp != "0")
            {
                listTmp = strTmp.Split(',');
                if (listTmp.Length == 2)
                {
                    AdTalkInfo1 ti;
                    ti.time = Convert.ToInt32(listTmp[0]);
                    ti.talkid = Convert.ToInt32(listTmp[1]);
                    data.m_AdTalkInfo.Add(ti);
                }
            }

            //strTmp = reader.GetString(reader.GetOrdinal("WaitTalkList"));
            //if (!strTmp.Equals("0"))
            //{
            //    listTmp = strTmp.Split(';');
            //    foreach (var item in listTmp)
            //    {
            //        listTmp1 = item.Split('_', ',');
            //        if (listTmp1.Length != 3)
            //            continue;
            //        data.npcid_behindTalk_forwardTalk.Add(Convert.ToInt32(listTmp1[0]), new int[2] { Convert.ToInt32(listTmp1[1]), Convert.ToInt32(listTmp1[2]) });
            //    }
            //}

            strTmp = reader.GetString(reader.GetOrdinal("TalkID"));
            listTmp = strTmp.Split(';');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                listTmp1 = listTmp[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                AdTalkInfo talkInfo = new AdTalkInfo();
                talkInfo.dist = Convert.ToInt32(listTmp1[0]);

                listTmp2 = listTmp1[1].Split('_');
                if (listTmp2.Length != 2)
                    continue;

                talkInfo.radius = Convert.ToInt32(listTmp2[0]);
                listTmp3 = listTmp1[1].Split('#');

                for (int j = 0; j < listTmp3.Length; j++)
                {
                    if (listTmp3[j] == "0")
                        continue;

                    talkInfo.talkid.Add(Convert.ToInt32(listTmp3[j]));
                }

                data.m_AdTalkID.Add(talkInfo);
            }


            strTmp = reader.GetString(reader.GetOrdinal("Monster"));
            listTmp = strTmp.Split(':');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                listTmp1 = listTmp[i].Split('_');
                if (listTmp1.Length != 4)
                    continue;

                MonsterIDNum tmp = new MonsterIDNum();
                tmp.id = Convert.ToInt32(listTmp1[0]);
                tmp.num = Convert.ToInt32(listTmp1[1]);
                tmp.radius = Convert.ToInt32(listTmp1[2]);

                listTmp2 = listTmp1[3].Split(',');
                if (listTmp2.Length == 3)
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
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                data.m_ComTalkID.Add(Convert.ToInt32(listTmp[i]));
            }

            //data.m_FollowDist = Convert.ToSingle(reader.GetString(reader.GetOrdinal("FollowDist")));
            strTmp = reader.GetString(reader.GetOrdinal("WaitDist"));
            listTmp = strTmp.Split(',');
            if (listTmp.Length == 2)
            {
                data.m_WaitDist.Add(Convert.ToInt32(listTmp[0]));
                data.m_WaitDist.Add(Convert.ToInt32(listTmp[1]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            data.m_BuildID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("BuildID")));

            strTmp = reader.GetString(reader.GetOrdinal("NpcNum"));
            listTmp = strTmp.Split(',');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                data.m_CreateNpcList.Add(Convert.ToInt32(listTmp[i]));
            }

            AddAdTypeFollowData(data.m_TargetID, data);
        }
    }

    public static void LoadAdTypeSearch()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuest_EnterArea");
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
            string strTmp = reader.GetString(reader.GetOrdinal("DistPos"));
            string[] listTmp = strTmp.Split('_');

            AdMissionRand mr = new AdMissionRand();
            if (listTmp.Length == 4)
            {
                mr.refertoType = (ReferToType)Convert.ToInt32(listTmp[0]);
                mr.referToID = Convert.ToInt32(listTmp[1]);
                mr.radius1 = Convert.ToInt32(listTmp[2]);
                mr.radius2 = Convert.ToInt32(listTmp[3]);
            }
            data.m_mr = mr;

            data.m_TrackRadius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TrackRadius")));

            data.m_notForDungeon = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NotForDungeon"))) == 1 ? true : false;

            data.m_AdNpcRadius = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NpcRadius")));

            strTmp = reader.GetString(reader.GetOrdinal("Prompt"));
            string[] listTmp1, listTmp2, listTmp3;
            listTmp = strTmp.Split(';');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                listTmp1 = listTmp[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                AdTalkInfo talkInfo = new AdTalkInfo();
                talkInfo.dist = Convert.ToInt32(listTmp1[0]);

                listTmp2 = listTmp1[1].Split('_');
                if (listTmp2.Length != 2)
                    continue;

                talkInfo.radius = Convert.ToInt32(listTmp2[0]);
                listTmp3 = listTmp1[1].Split('#');

                for (int j = 0; j < listTmp3.Length; j++)
                {
                    if (listTmp3[j] == "0")
                        continue;

                    talkInfo.talkid.Add(Convert.ToInt32(listTmp3[j]));
                }

                data.m_AdPrompt.Add(talkInfo);
            }


            strTmp = reader.GetString(reader.GetOrdinal("TalkID"));
            listTmp = strTmp.Split(';');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                listTmp1 = listTmp[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                AdTalkInfo talkInfo = new AdTalkInfo();
                talkInfo.dist = Convert.ToInt32(listTmp1[0]);

                listTmp2 = listTmp1[1].Split('_');
                if (listTmp2.Length != 2)
                    continue;

                talkInfo.radius = Convert.ToInt32(listTmp2[0]);
                listTmp3 = listTmp1[1].Split('#');

                for (int j = 0; j < listTmp3.Length; j++)
                {
                    if (listTmp3[j] == "0")
                        continue;

                    talkInfo.talkid.Add(Convert.ToInt32(listTmp3[j]));
                }

                data.m_AdTalkID.Add(talkInfo);
            }

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("NpcNum"));
            listTmp = strTmp.Split(',');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                data.m_CreateNpcList.Add(Convert.ToInt32(listTmp[i]));
            }

            AddAdTypeSearchData(data.m_TargetID, data);
        }
    }

    public static void LoadAdTypeUseItem()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuest_UseItem");
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
            data.m_ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemID")));
            data.m_UseNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("UseNum")));
            data.m_allowOld = Convert.ToInt32(reader.GetString(reader.GetOrdinal("AllowOld"))) == 1;

            string strTmp;
            string[] listTmp,listTmp1;

            strTmp = reader.GetString(reader.GetOrdinal("Pos"));

            listTmp = strTmp.Split(';');
            for (int i = 0; i < listTmp.Length; i++)
            {
                data.m_Type = 1;
                if (listTmp[i] == "0")
                    continue;
                listTmp1 = listTmp[i].Split('_');
                if (listTmp1.Length != 4)
                    continue;
                AdMissionRand rand = new AdMissionRand();
                rand.refertoType = (ReferToType)Convert.ToInt32(listTmp1[0]);
                rand.referToID = Convert.ToInt32(listTmp1[1]);
                rand.radius1 = Convert.ToInt32(listTmp1[2]);
                rand.radius2 = Convert.ToInt32(listTmp1[3]);
                data.m_AdDistPos = rand;
            }

            strTmp = reader.GetString(reader.GetOrdinal("UsedPrompt"));
            listTmp = strTmp.Split(',');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                data.m_UsedPrompt.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("TalkID"));
            listTmp = strTmp.Split(',');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                data.m_TalkID.Add(Convert.ToInt32(listTmp[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("FailPrompt"));
            listTmp = strTmp.Split(',');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
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

            strTmp = reader.GetString(reader.GetOrdinal("ComMission"));
            data.m_comMission = Convert.ToInt32(strTmp) == 1 ? true : false;

            AddAdTypeUseItemData(data.m_TargetID, data);
        }
    }

    public static void LoadAdTypeMessenger()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuest_Delivery");
        reader.Read();
        reader.Read();
        while (reader.Read())
        {
            TypeMessengerData data = new TypeMessengerData();
            int strid;
            data.m_TargetID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TargetID")));
            data.m_ScriptID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ScriptID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Desc")));
            data.m_Desc = PELocalization.GetString(strid);

            MissionRand mr = new MissionRand();
            string strTmp = reader.GetString(reader.GetOrdinal("NpcRadius"));
            string [] listTmp;
            if (strTmp != "0")
            {
                listTmp = strTmp.Split(',');
                mr.dist = Convert.ToInt32(listTmp[0]);
                if (listTmp.Length == 2)
                    mr.radius = Convert.ToInt32(listTmp[1]);
            }
            data.m_AdNpcRadius = mr;

            data.m_ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemID")));
            data.m_ItemNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ItemNum")));

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            AddAdTypeMessengerData(data.m_TargetID, data);
        }
    }

    public static void LoadAdTypeTowerDefends()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuest_TowerDefence");
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
            float x, y, z;
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
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

				if (!int.TryParse(listTmp[i], out strid))
					continue;

                data.m_iNpcList.Add(strid);
            }

            strTmp = reader.GetString(reader.GetOrdinal("ObjectList"));
            listTmp = strTmp.Split(',');
            for (int i = 0; i < listTmp.Length; i++)
            {
                if (listTmp[i] == "0")
                    continue;

                data.m_ObjectList.Add(Convert.ToInt32(listTmp[i]));
            }

            data.m_Count = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Count")));

            data.m_TdInfoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("td_id")));

            strTmp = reader.GetString(reader.GetOrdinal("ReceiveQuest"));
            listTmp = strTmp.Split(',');
            for(int i=0; i<listTmp.Length; i++)
            {
                if(listTmp[i] == "0")
                    continue;

                data.m_ReceiveList.Add(Convert.ToInt32(listTmp[i]));
            }

            AddAdTypeTowerDefendsData(data.m_TargetID, data);
        }
    }

    public static void LoadRMissionGroup()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdQuestChain");
        while (reader.Read())
        {
            AdRandomGroup data = new AdRandomGroup();
            List<GroupInfo> giList = new List<GroupInfo>();

            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("QC_ID")));
            string tmp = reader.GetString(reader.GetOrdinal("PreLimit"));
            string[] preLimit = tmp.Split('_');
            if (preLimit.Length == 2)
            {
                data.m_requstAll = Convert.ToInt32(preLimit[0]) == 2 ? true : false;
                string[] preLimit1 = preLimit[1].Split(',');
                for (int i = 0; i < preLimit1.Length; i++)
                    data.m_preLimit.Add(Convert.ToInt32(preLimit1[i]));
            }
            data.m_FinishTimes = Convert.ToInt32(reader.GetString(reader.GetOrdinal("FinishTimes")));
            data.m_Area = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Area")));
            int multimode = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Muti")));
            data.m_IsMultiMode = (multimode == 1) ? true : false;
            string strTmp = reader.GetString(reader.GetOrdinal("QusetGroup1"));
            string[] tmpList, listTmp1;
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList.Add(gi);
            }
            data.m_GroupList.Add(1, giList);

            List<GroupInfo> giList1 = new List<GroupInfo>();
            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup2"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList1.Add(gi);
            }
            data.m_GroupList.Add(2, giList1);

            List<GroupInfo> giList2 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup3"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList2.Add(gi);
            }
            data.m_GroupList.Add(3, giList2);

            List<GroupInfo> giList3 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup4"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList3.Add(gi);
            }
            data.m_GroupList.Add(4, giList3);

            List<GroupInfo> giList4 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup5"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList4.Add(gi);
            }
            data.m_GroupList.Add(5, giList4);

            List<GroupInfo> giList5 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup6"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList5.Add(gi);
            }
            data.m_GroupList.Add(6, giList5);

            List<GroupInfo> giList6 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup7"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList6.Add(gi);
            }
            data.m_GroupList.Add(7, giList6);

            List<GroupInfo> giList7 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup8"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList7.Add(gi);
            }
            data.m_GroupList.Add(8, giList7);

            List<GroupInfo> giList8 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup9"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList8.Add(gi);
            }
            data.m_GroupList.Add(9, giList8);

            List<GroupInfo> giList9 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup10"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList9.Add(gi);
            }
            data.m_GroupList.Add(10, giList9);

            List<GroupInfo> giList10 = new List<GroupInfo>();

            strTmp = reader.GetString(reader.GetOrdinal("QusetGroup11"));
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                giList10.Add(gi);
            }
            data.m_GroupList.Add(11, giList10);

            m_AdRandomGroup.Add(data.m_ID, data);
        }
    }

    public static void LoadAdRandMission()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdRandomQuest_List");
        reader.Read();
        while (reader.Read())
        {
            MissionCommonData data = new MissionCommonData();
            RandomField rf = new RandomField();
            int strid;
            data.m_Type = MissionType.MissionType_Sub;
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("MissionName")));
            data.m_MissionName = PELocalization.GetString(strid);
            data.m_MaxNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("MaxNum")));
            data.m_Type = (MissionType)Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));

            string strtmp = reader.GetString(reader.GetOrdinal("TargetIDList"));
            string[] tmplist = strtmp.Split(';');
            string[] tmplist1,tmplist2;

            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++){
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));
                    data.m_TargetIDList.Add(Convert.ToInt32(tmplist1[m]));
                }
                rf.TargetIDMap.Add(tli);
            }

            strtmp = reader.GetString(reader.GetOrdinal("Reputation"));
            tmplist = strtmp.Split('_');
            if (tmplist.Length == 3)
            {
                ReputationPreLimit limit = new ReputationPreLimit();
                limit.type = Convert.ToInt32(tmplist[0]);
                limit.min = Convert.ToInt32(tmplist[1]);
                limit.max = Convert.ToInt32(tmplist[2]);
                limit.campID = -1;
                data.m_reputationPre.Add(limit);
            }

            strtmp = reader.GetString(reader.GetOrdinal("PreLimit"));
            tmplist = strtmp.Split(':');
            if (tmplist.Length == 2)
            {
                data.m_PreLimit.type = Convert.ToInt32(tmplist[0]);
                tmplist1 = tmplist[1].Split(',');
                for (int i = 0; i < tmplist1.Length; i++)
                    data.m_PreLimit.idlist.Add(Convert.ToInt32(tmplist1[i]));
            }

            strtmp = reader.GetString(reader.GetOrdinal("Get_DemandItem"));
            tmplist = strtmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(tmplist1[0]);
                tmp.num = Convert.ToInt32(tmplist1[1]);
                data.m_Get_DemandItem.Add(tmp);
            }

            strtmp = reader.GetString(reader.GetOrdinal("Get_DeleteItem"));
            tmplist = strtmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(tmplist1[0]);
                tmp.num = Convert.ToInt32(tmplist1[1]);
                data.m_Get_DeleteItem.Add(tmp);
            }

            strtmp = reader.GetString(reader.GetOrdinal("Get_MissionItem"));
            tmplist = strtmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(tmplist1[0]);
                tmp.num = Convert.ToInt32(tmplist1[1]);
                data.m_Get_MissionItem.Add(tmp);
            }

            strtmp = reader.GetString(reader.GetOrdinal("CoSelRewardItem"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                tmplist1 = tmplist[i].Split(',');
                List<MissionIDNum> tmp = new List<MissionIDNum>();
                for (int j = 0; j < tmplist1.Length; j++)
                {
                    tmplist2 = tmplist1[j].Split('_');
                    if (tmplist2.Length != 2)
                        continue;

                    MissionIDNum idNum;
                    idNum.id = Convert.ToInt32(tmplist2[0]);
                    idNum.num = Convert.ToInt32(tmplist2[1]);
                    tmp.Add(idNum);
                }

                if (tmp.Count == 0)
                    continue;

                rf.RewardMap.Add(tmp);
            }

            strtmp = reader.GetString(reader.GetOrdinal("RewardItem"));
            tmplist = strtmp.Split(',');
            foreach (var item in tmplist)
            {
                tmplist1 = item.Split('_');
                if (tmplist1.Length != 2)
                    continue;
                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(tmplist1[0]);
                tmp.num = Convert.ToInt32(tmplist1[1]);
                rf.FixedRewardMap.Add(tmp);

                //data.m_Com_RewardItem.Add(tmp);
            }

            strtmp = reader.GetString(reader.GetOrdinal("CoRemoveItem"));
            tmplist = strtmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;

                MissionIDNum tmp;
                tmp.id = Convert.ToInt32(tmplist1[0]);
                tmp.num = Convert.ToInt32(tmplist1[1]);
                data.m_Com_RemoveItem.Add(tmp);
            }

            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Description")));
            data.m_Description = PELocalization.GetString(strid);

            strtmp = reader.GetString(reader.GetOrdinal("player_talk"));
            tmplist = strtmp.Split(',');
            if (tmplist.Length == 2)
            {
                data.m_PlayerTalk[0] = Convert.ToInt32(tmplist[0]);
                data.m_PlayerTalk[1] = Convert.ToInt32(tmplist[1]);
            }
            else if (tmplist.Length == 1 && tmplist[0] != "0")
            {
                data.m_PlayerTalk[0] = Convert.ToInt32(tmplist[0]);
                data.m_PlayerTalk[1] = 0;
            }

            strtmp = reader.GetString(reader.GetOrdinal("TalkOP"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

                //if (tli.listid.Count == 1)
                //    data.m_TalkOP.Add(Convert.ToInt32(tmplist1[0]));
                if (tmplist.Length == 1)
                {
                    for (int n = 0; n < tmplist1.Length; n++)
                        data.m_TalkOP.Add(Convert.ToInt32(tmplist1[n]));
                }

                rf.TalkOPMap.Add(tli);
            }

            strtmp = reader.GetString(reader.GetOrdinal("TalkIN"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

                if (tmplist.Length == 1)
                {
                    for (int n = 0; n < tmplist1.Length; n++)
                        data.m_TalkIN.Add(Convert.ToInt32(tmplist1[n]));
                }

                rf.TalkINMap.Add(tli);
            }


            strtmp = reader.GetString(reader.GetOrdinal("TalkED"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

                if (tmplist.Length == 1)
                {
                    for (int n = 0; n < tmplist1.Length; n++)
                        data.m_TalkED.Add(Convert.ToInt32(tmplist1[n]));
                }

                rf.TalkEDMap.Add(tli);
            }

            if (Convert.ToInt32(reader.GetString(reader.GetOrdinal("bGiveUp"))) == 0)
                data.m_bGiveUp = false;
            else
                data.m_bGiveUp = true;

            strtmp = reader.GetString(reader.GetOrdinal("TalkOP_SP"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

                rf.TalkOPSMap.Add(tli);
            }

            strtmp = reader.GetString(reader.GetOrdinal("TalkIN_SP"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

                rf.TalkINSMap.Add(tli);
            }

            strtmp = reader.GetString(reader.GetOrdinal("TalkED_SP"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

                rf.TalkEDSMap.Add(tli);
            }

            strtmp = reader.GetString(reader.GetOrdinal("NextQuest"));
            if (strtmp != "0")
            {
                tmplist = strtmp.Split(',');
                for (int i = 0; i < tmplist.Length; i++)
                    data.m_EDID.Add(Convert.ToInt32(tmplist[i]));
            }

            strtmp = reader.GetString(reader.GetOrdinal("AutoReply"));
            if (strtmp.Equals("1"))
                data.isAutoReply = true;
            else
                data.isAutoReply = false;

            strtmp = reader.GetString(reader.GetOrdinal("RewardSP"));
            data.addSpValue = Convert.ToInt32(strtmp);

            strtmp = reader.GetString(reader.GetOrdinal("AdvPlot"));
            tmplist = strtmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                tmplist1 = tmplist[i].Split('_');
                if (tmplist1.Length != 2)
                    continue;
                StoryInfo si = new StoryInfo();
                si.type = (Story_Info)Convert.ToInt32(tmplist1[0]);
                si.storyid = Convert.ToInt32(tmplist1[1]);

                data.m_StoryInfo.Add(si);
            }

            strtmp = reader.GetString(reader.GetOrdinal("MutexLimit"));
            tmplist = strtmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;

                if (i == 0)
                {
                    string[] conpre = tmplist[i].Split(':');
                    if (conpre.Length == 2)
                    {
                        data.m_MutexLimit.type = Convert.ToInt32(conpre[0]);
                        if (conpre[1] != "0")
                            data.m_MutexLimit.idlist.Add(Convert.ToInt32(conpre[1]));
                    }
                    else if (conpre.Length == 1)
                    {
                        //如果前置任务只有1个，可以直接填id
                        if (tmplist.Length == 1)
                        {
                            if (conpre[0] != "0")
                                data.m_MutexLimit.idlist.Add(Convert.ToInt32(conpre[0]));
                        }
                    }
                }
                else
                {
                    if (tmplist[i] != "0")
                        data.m_MutexLimit.idlist.Add(Convert.ToInt32(tmplist[i]));
                }
            }

            strtmp = reader.GetString(reader.GetOrdinal("GuanLianList"));
            tmplist = strtmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] == "0")
                    continue;
                data.m_GuanLianList.Add(Convert.ToInt32(tmplist[i]));
            }

            data.m_increaseChain = reader.GetString(reader.GetOrdinal("IncreaseChain")) == "0" ? false : true;

            strtmp = reader.GetString(reader.GetOrdinal("ChangeReputation"));
            tmplist = strtmp.Split('_');
            if (tmplist.Length == 2)
            {
                data.m_changeReputation[0] = 1;
                data.m_changeReputation[1] = Convert.ToInt32(tmplist[0]);
                data.m_changeReputation[2] = Convert.ToInt32(tmplist[1]);
            }

            rf.keepItem = Convert.ToInt32(reader.GetString(reader.GetOrdinal("KeepItem"))) == 1? true:false;


            strtmp = reader.GetString(reader.GetOrdinal("TempLimit"));
            tmplist = strtmp.Split(',');
            foreach (var item in tmplist)
            {
                int n = Convert.ToInt32(item);
                if (n == 0)
                    continue;
                data.m_tempLimit.Add(n);
            }

            strtmp = reader.GetString(reader.GetOrdinal("Dungeon"));
            tmplist = strtmp.Split('_');
            if (tmplist.Length == 3)
            {
                data.creDungeon.effect = true;
                data.creDungeon.npcID = Convert.ToInt32(tmplist[0]);
                data.creDungeon.radius = Convert.ToInt32(tmplist[1]);
                data.creDungeon.dungeonLevel = Convert.ToInt32(tmplist[2]);
            }

            m_AdRandomFieldMap.Add(data.m_ID, rf);
            m_AdRandMisMap.Add(data.m_ID, data);
        }
    }

    public static void LoadData()
    {
        LoadAdTypeMonster();
        LoadAdTypeCollect();
        LoadAdTypeFollow();
        LoadAdTypeSearch();
        LoadAdTypeUseItem();
        LoadAdTypeMessenger();
        LoadAdTypeTowerDefends();
        LoadRMissionGroup();
        LoadAdRandMission();
    }

    public static void CreateRandomMission(int id, int oidx = -1, int rewardIdx = -1)
    {
        if (MissionManager.Instance.HasMission(id))
            return;

        MissionCommonData data = GetAdRandomMission(id);
        if (data == null)
            return;

        data.m_TargetIDList.Clear();
        data.m_TalkOP.Clear();
        data.m_TalkIN.Clear();
        data.m_TalkED.Clear();
        data.m_PromptOP.Clear();
        data.m_PromptIN.Clear();
        data.m_PromptED.Clear();
        data.m_Com_RewardItem.Clear();
        data.m_Com_RemoveItem.Clear();

        RandomField rf;
        if (!m_AdRandomFieldMap.ContainsKey(id))
            return;

        rf = m_AdRandomFieldMap[id];

        int rewardId;
        if (rf.RewardMap.Count != 0)
        {
            rewardId = rewardIdx == -1 ? UnityEngine.Random.Range(0, rf.RewardMap.Count) : rewardIdx;
            if (rewardId < rf.RewardMap.Count)
            {
                for (int i = 0; i < rf.RewardMap[rewardId].Count; i++)
                    data.m_Com_RewardItem.Add(rf.RewardMap[rewardId][i]);
            }
        }

        foreach (var item in rf.FixedRewardMap)
            data.m_Com_RewardItem.Add(item);

        int idx = ((oidx == -1 ) ? (UnityEngine.Random.Range(0, rf.TargetIDMap.Count)) : (oidx));

        if (rf.TargetIDMap.Count != 0)
        {
            if (rf.TargetIDMap.Count <= idx)
                return;
        }

        TargetListInfo tli;
        int tid;

        if (rf.TargetIDMap.Count > idx)
        {
            tli = rf.TargetIDMap[idx];

            for (int i = 0; i < tli.listid.Count; i++)
            {
                tid = tli.listid[i] / 1000;
                if (tid == 2)
                {
                    TypeCollectData colData = MissionManager.GetTypeCollectData(tli.listid[i]);
                    if (colData == null)
                        continue;

                    if (!rf.keepItem)
                    {
                        MissionIDNum tmp;
                        tmp.id = colData.ItemID;
                        tmp.num = colData.ItemNum;
                        data.m_Com_RemoveItem.Add(tmp);
                    }
                }

                data.m_TargetIDList.Add(tli.listid[i]);
            }
        }

        if (rf.TalkOPMap.Count > idx)
        {
            tli = rf.TalkOPMap[idx];
            for (int i = 0; i < tli.listid.Count; i++)
                data.m_TalkOP.Add(tli.listid[i]);
        }

        if (rf.TalkINMap.Count > idx)
        {
            tli = rf.TalkINMap[idx];
            for (int i = 0; i < tli.listid.Count; i++)
                data.m_TalkIN.Add(tli.listid[i]);
        }

        if (rf.TalkEDMap.Count > idx)
        {
            tli = rf.TalkEDMap[idx];
            for (int i = 0; i < tli.listid.Count; i++)
                data.m_TalkED.Add(tli.listid[i]);
        }

        if (rf.TalkOPSMap.Count > idx)
        {
            tli = rf.TalkOPSMap[idx];
            for (int i = 0; i < tli.listid.Count; i++)
                data.m_PromptOP.Add(tli.listid[i]);
        }

        if (rf.TalkINSMap.Count > idx)
        {
            tli = rf.TalkINSMap[idx];
            for (int i = 0; i < tli.listid.Count; i++)
                data.m_PromptIN.Add(tli.listid[i]);
        }

        if (rf.TalkEDSMap.Count > idx)
        {
            tli = rf.TalkEDSMap[idx];
            for (int i = 0; i < tli.listid.Count; i++)
                data.m_PromptED.Add(tli.listid[i]);
        }
    }

#region Record
    public static void Export(BinaryWriter bw)
    {
        //if (PlayerFactory.mMainPlayer == null)
        //    return;

        PlayerMission pm = MissionManager.Instance.m_PlayerMission;
		List<int> tmpList = new List<int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> ite in pm.m_MissionInfo)
        {
            if (!m_AdRandMisMap.ContainsKey(ite.Key))
                continue;

            tmpList.Add(ite.Key);
        }

        bw.Write(tmpList.Count);
        for (int i = 0; i < tmpList.Count; i++)
        {
            MissionCommonData data = MissionManager.GetMissionCommonData(tmpList[i]);
            if (data == null)
                continue;

            bw.Write(data.m_ID);
            bw.Write(data.m_MissionName);
            bw.Write(data.m_iNpc);
            bw.Write(data.m_iReplyNpc);
            bw.Write((int)data.m_Type);
            bw.Write(data.m_MaxNum);
            bw.Write(data.m_Description);
            bw.Write(data.m_TargetIDList.Count);
            for (int m = 0; m < data.m_TargetIDList.Count; m++)
            {
                int targetid = data.m_TargetIDList[m];
                bw.Write(targetid);

                TargetType curType = MissionRepository.GetTargetType(data.m_TargetIDList[m]);
                if(curType == TargetType.TargetType_Follow)
                {
                    bw.Write(3);
                    TypeFollowData folData = MissionManager.GetTypeFollowData(targetid);
                    if (folData == null)
                    {
						bw.Write(0);
                        continue;
                    }
                    else
                    {
						bw.Write(1);
						bw.Write(folData.m_iNpcList.Count);
                        for (int j = 0; j < folData.m_iNpcList.Count; j++)
                        {
							bw.Write(folData.m_iNpcList[j]);
                        }
						PETools.Serialize.WriteVector3(bw, folData.m_DistPos);
                    }
                }
                else if (curType == TargetType.TargetType_Discovery)
                {
					bw.Write(4);
                    TypeSearchData seaData = MissionManager.GetTypeSearchData(targetid);
                    if (seaData == null)
                    {
						bw.Write(0);
                        continue;
                    }
                    else
                    {
						bw.Write(1);
						PETools.Serialize.WriteVector3(bw, seaData.m_DistPos);
                    }

                }
                else if (curType == TargetType.TargetType_UseItem)
                {
					bw.Write(5);
                    TypeUseItemData useData = MissionManager.GetTypeUseItemData(targetid);
                    if (useData == null)
                    {
						bw.Write(0);
                        continue;
                    }
                    else
                    {
						bw.Write(1);
						PETools.Serialize.WriteVector3(bw, useData.m_Pos);
                    }
                }
                else if(curType == TargetType.TargetType_TowerDif)
                {
					bw.Write(6);
                    TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(targetid);
                    if (towerData == null)
                    {
						bw.Write(0);
                        continue;
                    }
                    else
                    {
						bw.Write(1);
						bw.Write(towerData.m_iNpcList.Count);
                        for (int j = 0; j < towerData.m_iNpcList.Count; j++)
                        {
							bw.Write(towerData.m_iNpcList[j]);
                        }
						PETools.Serialize.WriteVector3(bw, towerData.finallyPos);
                    }
                }
                else
					bw.Write(0);
            }

			bw.Write(data.m_PlayerTalk.Length);
            for (int m = 0; m < data.m_PlayerTalk.Length; m++)
				bw.Write(data.m_PlayerTalk[m]);

			bw.Write(data.m_Get_DemandItem.Count);
            for (int m = 0; m < data.m_Get_DemandItem.Count; m++)
            {
				bw.Write(data.m_Get_DemandItem[m].id);
				bw.Write(data.m_Get_DemandItem[m].num);
            }

			bw.Write(data.m_Get_DeleteItem.Count);
            for (int m = 0; m < data.m_Get_DeleteItem.Count; m++)
            {
				bw.Write(data.m_Get_DeleteItem[m].id);
				bw.Write(data.m_Get_DeleteItem[m].num);
            }

			bw.Write(data.m_Get_MissionItem.Count);
            for (int m = 0; m < data.m_Get_MissionItem.Count; m++)
            {
				bw.Write(data.m_Get_MissionItem[m].id);
				bw.Write(data.m_Get_MissionItem[m].num);
            }

			bw.Write(data.m_Com_RewardItem.Count);
            for (int m = 0; m < data.m_Com_RewardItem.Count; m++)
            {
				bw.Write(data.m_Com_RewardItem[m].id);
				bw.Write(data.m_Com_RewardItem[m].num);
            }

			bw.Write(data.m_Com_SelRewardItem.Count);
            for (int m = 0; m < data.m_Com_SelRewardItem.Count; m++)
            {
				bw.Write(data.m_Com_SelRewardItem[m].id);
				bw.Write(data.m_Com_SelRewardItem[m].num);
            }

			bw.Write(data.m_Com_RemoveItem.Count);
            for (int m = 0; m < data.m_Com_RemoveItem.Count; m++)
            {
				bw.Write(data.m_Com_RemoveItem[m].id);
				bw.Write(data.m_Com_RemoveItem[m].num);
            }

			bw.Write(data.m_TalkOP.Count);
            for (int m = 0; m < data.m_TalkOP.Count; m++)
				bw.Write(data.m_TalkOP[m]);

			bw.Write(data.m_OPID.Count);
            for (int m = 0; m < data.m_OPID.Count; m++)
				bw.Write(data.m_OPID[m]);

			bw.Write(data.m_TalkIN.Count);
            for (int m = 0; m < data.m_TalkIN.Count; m++)
				bw.Write(data.m_TalkIN[m]);

			bw.Write(data.m_INID.Count);
            for (int m = 0; m < data.m_INID.Count; m++)
				bw.Write(data.m_INID[m]);

			bw.Write(data.m_TalkED.Count);
            for (int m = 0; m < data.m_TalkED.Count; m++)
				bw.Write(data.m_TalkED[m]);

			bw.Write(data.m_EDID.Count);
            for (int m = 0; m < data.m_EDID.Count; m++)
				bw.Write(data.m_EDID[m]);
        }
    }

    public static void Import(byte[] buffer)
    {
        if (buffer.Length == 0)
            return;

        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);

        int iSize = _in.ReadInt32();
		int num = 0;
        for (int k = 0; k < iSize; k++)
        {
            int id = _in.ReadInt32();
            if (!m_AdRandMisMap.ContainsKey(id))
                return;

            MissionCommonData data = m_AdRandMisMap[id];
            if (data == null)
                continue;

            data.m_TargetIDList.Clear();
            data.m_TalkOP.Clear();
            data.m_TalkIN.Clear();
            data.m_TalkED.Clear();
            data.m_PromptOP.Clear();
            data.m_PromptIN.Clear();
            data.m_PromptED.Clear();
            data.m_Com_RewardItem.Clear();
            data.m_Com_RemoveItem.Clear();
            for (int i = 0; i < data.m_PlayerTalk.Length; i++)
                data.m_PlayerTalk[i] = 0;
            data.m_Get_DemandItem.Clear();
            data.m_Get_DeleteItem.Clear();
            data.m_Get_MissionItem.Clear();
            data.m_Com_RewardItem.Clear();
            data.m_Com_SelRewardItem.Clear();
            data.m_Com_RemoveItem.Clear();

            data.m_TalkOP.Clear();
            data.m_OPID.Clear();
            data.m_TalkIN.Clear();
            data.m_INID.Clear();
            data.m_TalkED.Clear();
            data.m_EDID.Clear();

            //lz-2016.08.17 因为开始存储的不是映射,这里读到的数据是上一次语言的数据，所以不对，原本的数据是对的，不用修改
            _in.ReadString();
            //data.m_MissionName = _in.ReadString();
            //int outinfo;
            //if (int.TryParse(data.m_MissionName, out outinfo))
            //    data.m_MissionName = PELocalization.GetString(outinfo);

            data.m_iNpc = _in.ReadInt32();
            data.m_iReplyNpc = _in.ReadInt32();
            NpcMissionDataRepository.AddReplyMission(data.m_iReplyNpc, id);
            data.m_Type = (MissionType)_in.ReadInt32();
            data.m_MaxNum = _in.ReadInt32();

            //lz-2016.08.17 因为开始存储的不是映射,这里读到的数据是上一次语言的数据，所以不对，原本的数据是对的，不用修改
            _in.ReadString();
            //data.m_Description = _in.ReadString();
            //if (int.TryParse(data.m_Description, out outinfo))
            //   data.m_Description = PELocalization.GetString(outinfo);

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                int targetid = _in.ReadInt32();
                data.m_TargetIDList.Add(targetid);
                int flag = _in.ReadInt32();
                if (flag == 3)
                {
                    TypeFollowData folData = MissionManager.GetTypeFollowData(targetid);
                    folData.m_iNpcList.Clear();
                    flag = _in.ReadInt32();
                    if (flag == 1)
                    {
                        int count = _in.ReadInt32();
                        for (int j = 0; j < count; j++)
                        {
                            id = _in.ReadInt32();
                            if (folData == null)
                                continue;

                            if(!folData.m_iNpcList.Contains(id))
                                folData.m_iNpcList.Add(id);
                        }
                        folData.m_DistPos = PETools.Serialize.ReadVector3(_in);
                    }
                }
                else if (flag == 4)
                {
                    TypeSearchData seaData = MissionManager.GetTypeSearchData(targetid);
                    flag = _in.ReadInt32();
                    if (flag == 1)
                    {
                        Vector3 tmpPos = PETools.Serialize.ReadVector3(_in);
                        if (seaData != null)
                            seaData.m_DistPos = tmpPos;
                    }
                }
                else if (flag == 5)
                {
                    TypeUseItemData useData = MissionManager.GetTypeUseItemData(targetid);
                    flag = _in.ReadInt32();
                    if (flag == 1)
                    {
                        Vector3 tmpPos = PETools.Serialize.ReadVector3(_in);
                        if (useData != null)
                            useData.m_Pos = tmpPos;
                    }
                }
                else if(flag == 6)
                {
                    TypeTowerDefendsData towerData = MissionManager.GetTypeTowerDefendsData(targetid);
                    flag = _in.ReadInt32();
                    if (flag == 1)
                    {
                        int count = _in.ReadInt32();
                        for (int j = 0; j < count; j++)
                        {
                            id = _in.ReadInt32();
                            if (towerData == null)
                                continue;

                            towerData.m_iNpcList.Add(id);
                        }
                        towerData.m_Pos.type = TypeTowerDefendsData.PosType.pos;
                        towerData.m_Pos.pos = PETools.Serialize.ReadVector3(_in);
                        towerData.finallyPos = towerData.m_Pos.pos;
                    }
                }
            }

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
                data.m_PlayerTalk[i] = _in.ReadInt32();

            MissionIDNum idnum;
            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                idnum.id = _in.ReadInt32();
                idnum.num = _in.ReadInt32();
                data.m_Get_DemandItem.Add(idnum);
            }

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                idnum.id = _in.ReadInt32();
                idnum.num = _in.ReadInt32();
                data.m_Get_DeleteItem.Add(idnum);
            }

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                idnum.id = _in.ReadInt32();
                idnum.num = _in.ReadInt32();
                data.m_Get_MissionItem.Add(idnum);
            }

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                idnum.id = _in.ReadInt32();
                idnum.num = _in.ReadInt32();
                data.m_Com_RewardItem.Add(idnum);
            }

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                idnum.id = _in.ReadInt32();
                idnum.num = _in.ReadInt32();
                data.m_Com_SelRewardItem.Add(idnum);
            }

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                idnum.id = _in.ReadInt32();
                idnum.num = _in.ReadInt32();
                data.m_Com_RemoveItem.Add(idnum);
            }

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
                data.m_TalkOP.Add(_in.ReadInt32());

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
                data.m_OPID.Add(_in.ReadInt32());

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
                data.m_TalkIN.Add(_in.ReadInt32());

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
                data.m_INID.Add(_in.ReadInt32());

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
                data.m_TalkED.Add(_in.ReadInt32());

            num = _in.ReadInt32();
            for (int i = 0; i < num; i++)
                data.m_EDID.Add(_in.ReadInt32());

        }

        _in.Close();
        ms.Close();
    }
#endregion
}
