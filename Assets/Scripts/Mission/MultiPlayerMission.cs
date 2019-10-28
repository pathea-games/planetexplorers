using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System;
using AiAsset;



public class MutiPlayMissionRand
{
    public int m_TaskPerson;
    public int m_Level;                                                                     //任务ID
    public int MapID;
    public List<int> m_MulKillID = new List<int>();                      //monsterID
    public List<int> m_MulKillNum = new List<int>();
    public List<int> m_MulBossID = new List<int>();                   //bossID
    public int m_MulBossNum;
    public List<int> m_MulCollectID = new List<int>();               //CollectID
    public List<int> m_MulCollectNum = new List<int>();
    public List<int> m_MulEscort = new List<int>();
    public List<int> m_MulExplore = new List<int>();
    public List<int> m_MulTD = new List<int>();
    public List<int> m_MulRD = new List<int>();                        //Reward

}

public struct RandIdx
{
    public int AttIdx;
    public int PersonIdx;
}

public class MutiPlayRandRespository
{
    public static int nextRandomMissionID = 0;

    public static Dictionary<int, MutiPlayMissionRand> m_MissionRand = new Dictionary<int, MutiPlayMissionRand>();
    public static Dictionary<string, RandIdx> m_RandInfo = new Dictionary<string, RandIdx>();

    public static MutiPlayMissionRand GetMissRand(int id)
    {
        return m_MissionRand.ContainsKey(id) ? m_MissionRand[id] : null;
    }

    //Adventure版本随机任务
    public static void CreateRandomMission()
    {
        
        MutiPlayMissionRand rand = GetMissRand(996);
        if (rand == null)
            return;

        int type = UnityEngine.Random.Range(0, 2);
        string[] listName = { "Monster Hunter", "Gather" };

        int idx = 0;

        MissionCommonData data = new MissionCommonData();
        data.m_ID = 996;
        data.m_MissionName = listName[type];
        data.m_iNpc = 0;
        data.m_iReplyNpc = 0;
        data.m_Type = MissionType.MissionType_Main;
        data.m_MaxNum = 1;

        int talkid = 0;

        if (type == 0)
        {
            TypeMonsterData monData = new TypeMonsterData();

            List<int> monidlist = new List<int>();

            //for (int i = 0; i < AiManager.Manager.aiObjects.Count; i++)
            //{
            //    AiDataObject aiobj = AiManager.Manager.aiObjects[i] as AiDataObject;
            //    if (aiobj == null)
            //        continue;

            //    //NpcCommon_N npc = aiobj.GetComponent<NpcCommon_N>();
            //    //if (npc != null)
            //    //    continue;

            //    if (rand.m_MulKillID.Contains(aiobj.dataId))
            //        monidlist.Add(aiobj.dataId);
            //}

            idx = UnityEngine.Random.Range(0, monidlist.Count);
            if (monidlist.Count == 0)
            {
                //Debug.Log("Aiobject.count = " + AiManager.Manager.aiObjects.Count + '\n');
                Debug.Log("idx = " + idx);
            }

            monData.m_TargetID = 1100;
            monData.m_MonsterID = monidlist[idx];
            monData.m_Desc = "KillMonster : " + AiDataBlock.GetAIDataName(monData.m_MonsterID);

            idx = UnityEngine.Random.Range(0, rand.m_MulKillNum.Count);
            monData.m_MonsterNum = rand.m_MulKillNum[idx];

            talkid = 901;
            MissionRepository.AddTypeMonsterData(monData.m_TargetID, monData);
            data.m_TargetIDList.Add(monData.m_TargetID);
        }
        else
        {
            TypeCollectData colData = new TypeCollectData();
            idx = UnityEngine.Random.Range(0, rand.m_MulCollectID.Count);

            colData.m_TargetID = 2100;
            colData.ItemID = rand.m_MulCollectID[idx];
            idx = UnityEngine.Random.Range(0, rand.m_MulCollectNum.Count);
            colData.ItemNum = rand.m_MulCollectNum[idx];
            colData.m_Desc = "Gather : " + ItemAsset.ItemProto.GetName(colData.ItemID);

            MissionIDNum tmp;
            tmp.id = colData.ItemID;
            tmp.num = colData.ItemNum;

            data.m_Com_RemoveItem.Add(tmp);
            talkid = 906;
            MissionRepository.AddTypeCollectData(colData.m_TargetID, colData);
            data.m_TargetIDList.Add(colData.m_TargetID);
        }

        TalkData talkdata = TalkRespository.GetTalkData(talkid);
        if (talkdata != null)
            data.m_Description = talkdata.m_Content;

        MissionIDNum idnum;

        idx = UnityEngine.Random.Range(0, rand.m_MulRD.Count);

        idnum.id = 30000000;                        //统一奖励货币
        idnum.num = rand.m_MulRD[idx];
        data.m_Com_RewardItem.Add(idnum);

        if (!MissionRepository.m_MissionCommonMap.ContainsKey(data.m_ID))
            MissionRepository.m_MissionCommonMap.Add(data.m_ID, data);

        nextRandomMissionID = data.m_ID;
    }

    public static void CreateRandomMission(int id)
    {
        int type = 1;

        if (MissionRepository.m_MissionCommonMap.ContainsKey(id))
        {
            MissionCommonData data1 = MissionManager.GetMissionCommonData(id);

            if (data1 != null)
                return;
        }

        MutiPlayMissionRand rand = GetMissRand(id);
        if (rand == null)
            return;

        //  0小怪、1BOSS、2收集、3护送、4探索、5塔防
        if (GameConfig.IsMultiServer)
        {
            type = UnityEngine.Random.Range(1, 3);          //随机1
            //PlayerFactory.RandIds[0] = type;
        }
        else
        {
            //type = PlayerFactory.RandIds[0];
        }

        MissionType misstype = MissionType.MissionType_Main;
        TypeMonsterData monData = null;
        TypeCollectData colData = null;
        TypeMessengerData mesData = null;
        TypeFollowData folData = null;
        TypeSearchData seaData = null;
        TypeTowerDefendsData towData = null;

        //switch (type)
        //{
        //    case 0:
        //        monData = new TypeMonsterData();
        //        break;
        //    case 1:
        //        monData = new TypeMonsterData();
        //        break;
        //    case 2:
        //        colData = new TypeCollectData();
        //        break;
        //    case 3:
        //        folData = new TypeFollowData();
        //        break;
        //    case 4:
        //        seaData = new TypeSearchData();
        //        break;
        //    case 5:
        //        towData = new TypeTowerDefendsData();
        //        break;
        //}

        int idx = 0;
        MissionCommonData data = new MissionCommonData();
        data.m_ID = id;
//        string[] listName = { "Monster Hunter", "Boss Hunter", "Gather", "Escort", "Exploration", "Defend" };

        //data.m_MissionName = listName[type];
        data.m_iNpc = 0;
        data.m_iReplyNpc = 0;
        data.m_Type = misstype;
        data.m_MaxNum = 1;

        TalkData talkdata = TalkRespository.GetTalkData(911);
        if (talkdata != null)
            data.m_Description = talkdata.m_Content;

        if (type == 0 || type == 1)
        {
            if (GameConfig.IsMultiServer)
            {
                idx = 0;                          //随机2
                //PlayerFactory.RandIds[1] = idx;
            }
            else
            {
                //idx = PlayerFactory.RandIds[1];
            }

            monData.m_TargetID = 1100;
            monData.m_MonsterID = rand.m_MulKillID[idx];
            monData.m_MonsterNum = rand.m_MulKillNum[idx];
            monData.m_Desc = "KillMonster : " + AiDataBlock.GetAIDataName(monData.m_MonsterID);

            data.m_TargetIDList.Add(monData.m_TargetID);
        }
        else if (type == 2)
        {
            if (GameConfig.IsMultiServer)
            {
                idx = UnityEngine.Random.Range(0, rand.m_MulCollectID.Count);            //随机3
                //PlayerFactory.RandIds[2] = idx;
            }
            else
            {
                //idx = PlayerFactory.RandIds[2];
            }

            colData.m_TargetID = 2100;

            colData.ItemID = rand.m_MulCollectID[idx];
            idx = UnityEngine.Random.Range(0, rand.m_MulCollectNum.Count);
            colData.ItemNum = rand.m_MulCollectNum[idx];
            colData.m_Desc = "Gather : " + ItemAsset.ItemProto.GetName(colData.ItemID);

            data.m_TargetIDList.Add(colData.m_TargetID);
        }
        if (type == 5)
        {
            towData.m_TargetID = 7100;
            towData.m_Time = 5;
            towData.m_Desc = data.m_MissionName;

            //towData.m_Pos = npcobj.transform.position;
            //towData.m_NpcList.Add(npcname);
            if (idx == 1)
            {
                towData.m_Count = UnityEngine.Random.Range(3, 6);
            }
            else if (idx == 2)
            {
                towData.m_Count = UnityEngine.Random.Range(5, 9);
            }
            else
            {
                towData.m_Count = UnityEngine.Random.Range(8, 13);
            }

            //for (int i = 0; i < towData.m_Count; i++)
            //{
            //    if (idx == 1)
            //    {
            //        towData.m_TDInfoList.Add(UnityEngine.Random.Range(17, 22));
            //    }
            //    else if (idx == 2)
            //    {
            //        towData.m_TDInfoList.Add(UnityEngine.Random.Range(22, 26));
            //    }
            //    else
            //    {
            //        towData.m_TDInfoList.Add(UnityEngine.Random.Range(26, 30));
            //    }
            //}

            data.m_TargetIDList.Add(towData.m_TargetID);
        }

        ////  0.小怪、1.BOSS、2.收集、3.护送、4.探索、5.塔防
        MissionIDNum idnum = new MissionIDNum();
        switch (type)
        {
            case 0:
            case 1:
            case 2:
                {
                    if (GameConfig.IsMultiServer)
                    {
                        idx = UnityEngine.Random.Range(0, rand.m_MulRD.Count);			//随机4
                        //PlayerFactory.RandIds[3] = idx;
                    }
                    else
                    {
                        //idx = PlayerFactory.RandIds[3];
                    }

                    idnum.id = 30000000;                        //统一奖励货币
                    idnum.num = rand.m_MulRD[idx];
                    data.m_Com_RewardItem.Add(idnum);
                }
                break;
            case 3:
                {
                    if (GameConfig.IsMultiServer)
                    {
                        idx = UnityEngine.Random.Range(0, rand.m_MulEscort.Count);			//随机4
                        //PlayerFactory.RandIds[3] = idx;
                    }
                    else
                    {
                        //idx = PlayerFactory.RandIds[3];
                    }
                    idnum.id = 30000000;                        //
                    idnum.num = rand.m_MulEscort[idx];
                    data.m_Com_RewardItem.Add(idnum);
                }
                break;
            case 4:
                {
                    if (GameConfig.IsMultiServer)
                    {
                        idx = UnityEngine.Random.Range(0, rand.m_MulExplore.Count);			//随机4
                        //PlayerFactory.RandIds[3] = idx;
                    }
                    else
                    {
                        //idx = PlayerFactory.RandIds[3];
                    }

                    idnum.id = 30000000;                        //
                    idnum.num = rand.m_MulExplore[idx];
                    data.m_Com_RewardItem.Add(idnum);
                }
                break;
            case 5:
                {
                    if (GameConfig.IsMultiServer)
                    {
                        idx = UnityEngine.Random.Range(0, rand.m_MulTD.Count);				//随机4
                        //PlayerFactory.RandIds[3] = idx;
                    }
                    else
                    {
                        //idx = PlayerFactory.RandIds[3];
                    }

                    idnum.id = 30000000;                        //
                    idnum.num = rand.m_MulTD[idx];
                    data.m_Com_RewardItem.Add(idnum);
                }
                break;
        }

        if (monData != null)
            MissionRepository.AddTypeMonsterData(monData.m_TargetID, monData);
        else if (colData != null)
            MissionRepository.AddTypeCollectData(colData.m_TargetID, colData);
        else if (folData != null)
            MissionRepository.AddTypeFollowData(folData.m_TargetID, folData);
        else if (seaData != null)
            MissionRepository.AddTypeSearchData(seaData.m_TargetID, seaData);
        else if (mesData != null)
            MissionRepository.AddTypeMessengerData(mesData.m_TargetID, mesData);
        else if (towData != null)
            MissionRepository.AddTypeTowerDefendsData(towData.m_TargetID, towData);

        if (!MissionRepository.m_MissionCommonMap.ContainsKey(data.m_ID))
            MissionRepository.m_MissionCommonMap.Add(data.m_ID, data);
    }

    static void LoadMissRand()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("RmultiMission");
        while (reader.Read())
        {
            MutiPlayMissionRand rand = new MutiPlayMissionRand();
            rand.m_TaskPerson = Convert.ToInt32(reader.GetString(reader.GetOrdinal("player_num")));
            rand.m_Level = Convert.ToInt32(reader.GetString(reader.GetOrdinal("tasklevel")));
            rand.MapID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("multiplayer_mapID")));

            string strTmp = reader.GetString(reader.GetOrdinal("multiplayer_monsterID"));
            string[] strList  = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulKillID.Add(Convert.ToInt32(strList[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_killNUM"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulKillNum.Add(Convert.ToInt32(strList[i]));
            }
            
            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_bossID"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulBossID.Add(Convert.ToInt32(strList[i]));
            }
            rand.m_MulBossNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("multiplayer_bossNUM")));

            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_collectID"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulCollectID.Add(Convert.ToInt32(strList[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_collectNUM"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulCollectNum.Add(Convert.ToInt32(strList[i]));
            }
            
            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_escort"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulEscort.Add(Convert.ToInt32(strList[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_explore"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulExplore.Add(Convert.ToInt32(strList[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_td"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulTD.Add(Convert.ToInt32(strList[i]));
            }

            strTmp = reader.GetString(reader.GetOrdinal("multiplayer_reward"));
            strList = strTmp.Split(',');
            for (int i = 0; i < strList.Length; i++)
            {
                rand.m_MulRD.Add(Convert.ToInt32(strList[i]));
            }

            m_MissionRand.Add(rand.m_Level, rand);

        }
    }

    public static void LoadData()
    {
        LoadMissRand();
    }
}
