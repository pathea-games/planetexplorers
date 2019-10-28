using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System;
using System.IO;

public struct TargetListInfo
{
    public List<int> listid;
}

public class RandomField
{
    public List<TargetListInfo> TargetIDMap;
    public List<TargetListInfo> TalkOPMap;
    public List<TargetListInfo> TalkINMap;
    public List<TargetListInfo> TalkEDMap;
    public List<TargetListInfo> TalkOPSMap;
    public List<TargetListInfo> TalkINSMap;
    public List<TargetListInfo> TalkEDSMap;
    public List<List<MissionIDNum>> RewardMap;
    public List<MissionIDNum> FixedRewardMap;
    public bool keepItem;

    public RandomField()
    {
        TargetIDMap = new List<TargetListInfo>();
        TalkOPMap = new List<TargetListInfo>();
        TalkINMap = new List<TargetListInfo>();
        TalkEDMap = new List<TargetListInfo>();
        TalkOPSMap = new List<TargetListInfo>();
        TalkINSMap = new List<TargetListInfo>();
        TalkEDSMap = new List<TargetListInfo>();
        RewardMap = new List<List<MissionIDNum>>();
        FixedRewardMap = new List<MissionIDNum>();
    }
}

public class RMRepository
{
    public static Dictionary<int, RandomField> m_RandomFieldMap = new Dictionary<int, RandomField>();

    public static Dictionary<int, MissionCommonData> m_RandMisMap = new Dictionary<int, MissionCommonData>();

    public static MissionCommonData GetRandomMission(int id)
    {
        if (m_RandMisMap.ContainsKey(id))
            return m_RandMisMap[id];

        return null;
    }

    public static bool HasRandomMission(int misid)
    {
        return m_RandMisMap.ContainsKey(misid);
    }

    public static void LoadRandMission()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("RandomQuest_List");
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
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

                rf.TargetIDMap.Add(tli);
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

            strtmp = reader.GetString(reader.GetOrdinal("CoRewardItem"));
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

                rf.TalkOPMap.Add(tli);
            }

            strtmp = reader.GetString(reader.GetOrdinal("TalkIN"));
            tmplist = strtmp.Split(';');
            for (int i = 0; i < tmplist.Length; i++)
            {
                //if (tmplist[i] == "0")
                //    continue;

                TargetListInfo tli;
                tli.listid = new List<int>();
                tmplist1 = tmplist[i].Split(',');
                for (int m = 0; m < tmplist1.Length; m++)
                    tli.listid.Add(Convert.ToInt32(tmplist1[m]));

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

            m_RandomFieldMap.Add(data.m_ID, rf);
            m_RandMisMap.Add(data.m_ID, data);
        }
    }

    public static void CreateRandomMission(int id,int idx = -1, int rewardIdx = -1)
    {
        if (MissionManager.Instance.HasMission(id))
            return;

        MissionCommonData data = GetRandomMission(id);
        if (data == null)
            return;

        RandomField rf;
        if (!m_RandomFieldMap.ContainsKey(id))
            return;

        rf = m_RandomFieldMap[id];
		if(idx < 0 || idx >= rf.TargetIDMap.Count)
        	idx = UnityEngine.Random.Range(0, rf.TargetIDMap.Count);
        //idx = 0;

        TargetListInfo tli = rf.TargetIDMap[idx];

        data.m_TargetIDList.Clear();
        data.m_TalkOP.Clear();
        data.m_TalkIN.Clear();
        data.m_TalkED.Clear();
        data.m_PromptOP.Clear();
        data.m_PromptIN.Clear();
        data.m_PromptED.Clear();
        data.m_Com_RewardItem.Clear();
        data.m_Com_RemoveItem.Clear();

        int tid;
        for (int i = 0; i < tli.listid.Count; i++)
        {
            tid = tli.listid[i] / 1000;
            if (tid == 2)
            {
                TypeCollectData colData = MissionManager.GetTypeCollectData(tli.listid[i]);
                if (colData == null)
                    continue;

                MissionIDNum tmp;
                tmp.id = colData.ItemID;
                tmp.num = colData.ItemNum;
                data.m_Com_RemoveItem.Add(tmp);
            }

            data.m_TargetIDList.Add(tli.listid[i]);
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
            {
                if (tli.listid[i] == 0)
                    continue;

                data.m_TalkIN.Add(tli.listid[i]);
            }
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

        if(rf.RewardMap.Count > 0)
        {
            rewardIdx = rewardIdx == -1 ? UnityEngine.Random.Range(0, rf.RewardMap.Count) : rewardIdx;
            for (int i = 0; i < rf.RewardMap[rewardIdx].Count; i++)
            {
                MissionIDNum idnum = rf.RewardMap[rewardIdx][i];
                data.m_Com_RewardItem.Add(idnum);
            }
        }

        foreach (var item in rf.FixedRewardMap)
            data.m_Com_RewardItem.Add(item);
    }


#region Record
    public static void Export(BinaryWriter bw)
    {
        //if (PlayerFactory.mMainPlayer == null)
        //    return;

        PlayerMission pm = MissionManager.Instance.m_PlayerMission;
        List<int> tmpList = new List<int>();
        foreach(KeyValuePair<int, Dictionary<string, string>> ite in pm.m_MissionInfo)
        {
            if (!m_RandMisMap.ContainsKey(ite.Key))
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
                bw.Write(data.m_TargetIDList[m]);

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
        int count = 0;
        for (int k = 0; k < iSize; k++)
        {
            int id = _in.ReadInt32();
            if (!m_RandMisMap.ContainsKey(id))
                return;

            MissionCommonData data = m_RandMisMap[id];
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

            //lz-2016.08.17 因为开始存储的不是映射,这里读到的数据是上一次语言的数据，所以不对，原本的数据是对的，不用修改
            _in.ReadString();
            //data.m_MissionName = _in.ReadString();

            data.m_iNpc = _in.ReadInt32();
            data.m_iReplyNpc = _in.ReadInt32();
            NpcMissionDataRepository.AddReplyMission(data.m_iReplyNpc, id);
            data.m_Type = (MissionType)_in.ReadInt32();
            data.m_MaxNum = _in.ReadInt32();

            //lz-2016.08.17 因为开始存储的不是映射,这里读到的数据是上一次语言的数据，所以不对，原本的数据是对的，不用修改
            _in.ReadString();
            //data.m_Description = _in.ReadString();

            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
                data.m_TargetIDList.Add(Convert.ToInt32(_in.ReadInt32()));

            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
                data.m_PlayerTalk[i] = Convert.ToInt32(_in.ReadInt32());

            MissionIDNum idnum;
            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                idnum.id = Convert.ToInt32(_in.ReadInt32());
                idnum.num = Convert.ToInt32(_in.ReadInt32());
                data.m_Get_DemandItem.Add(idnum);
            }

            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                idnum.id = Convert.ToInt32(_in.ReadInt32());
                idnum.num = Convert.ToInt32(_in.ReadInt32());
                data.m_Get_DeleteItem.Add(idnum);
            }

            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                idnum.id = Convert.ToInt32(_in.ReadInt32());
                idnum.num = Convert.ToInt32(_in.ReadInt32());
                data.m_Get_MissionItem.Add(idnum);
            }

            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                idnum.id = Convert.ToInt32(_in.ReadInt32());
                idnum.num = Convert.ToInt32(_in.ReadInt32());
                data.m_Com_RewardItem.Add(idnum);
            }

            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                idnum.id = Convert.ToInt32(_in.ReadInt32());
                idnum.num = Convert.ToInt32(_in.ReadInt32());
                data.m_Com_SelRewardItem.Add(idnum);
            }

            count = _in.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                idnum.id = Convert.ToInt32(_in.ReadInt32());
                idnum.num = Convert.ToInt32(_in.ReadInt32());
                data.m_Com_RemoveItem.Add(idnum);
            }

            count = _in.ReadInt32();
			data.m_TalkOP.Clear();
            for (int i = 0; i < count; i++)
                data.m_TalkOP.Add(Convert.ToInt32(_in.ReadInt32()));

            count = _in.ReadInt32();
			data.m_OPID.Clear();
            for (int i = 0; i < count; i++)
                data.m_OPID.Add(Convert.ToInt32(_in.ReadInt32()));

            count = _in.ReadInt32();
			data.m_TalkIN.Clear();
            for (int i = 0; i < count; i++)
                data.m_TalkIN.Add(Convert.ToInt32(_in.ReadInt32()));

            count = _in.ReadInt32();
			data.m_INID.Clear();
            for (int i = 0; i < count; i++)
                data.m_INID.Add(Convert.ToInt32(_in.ReadInt32()));

            count = _in.ReadInt32();
			data.m_TalkED.Clear();
            for (int i = 0; i < count; i++)
                data.m_TalkED.Add(Convert.ToInt32(_in.ReadInt32()));

            count = _in.ReadInt32();
			data.m_EDID.Clear();
            for (int i = 0; i < count; i++)
                data.m_EDID.Add(Convert.ToInt32(_in.ReadInt32()));

        }

        _in.Close();
        ms.Close();
    }
#endregion
}