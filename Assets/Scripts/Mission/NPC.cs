using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NpcMissionData
{
    public byte mCurComMisNum = 0;
    public int mCompletedMissionCount;
    public bool m_bRandomNpc = false;

    public int m_RandomMission;
    public List<int> m_MissionList = new List<int>();         //任务提示表(接)
    public List<int> m_MissionListReply = new List<int>();    //任务提示表(交)
    public List<int> m_RecruitMissionList = new List<int>();
    public List<int> m_CSRecruitMissionList = new List<int>();
    public int m_RecruitMissionNum;
    public int m_Rnpc_ID = -1;
    public Vector3 m_Pos;
    public int m_CurMissionGroup = 1;
    public int m_CurGroupTimes = 0;
    public int m_QCID;
    public bool m_bColonyOrder = true;
    public bool mInFollowMission;

    public void AddMissionListReply(int id)
    {
        if (m_MissionListReply.Contains(id))
            return;

        m_MissionListReply.Add(id);
    }

    public void RemoveMissionListReply(int id)
    {
        if (m_MissionListReply.Contains(id))
            return;

        m_MissionListReply.Remove(id);
    }

    public byte[] Write(int npcid)
    {
        MemoryStream ms = new MemoryStream();
        BinaryWriter _out = new BinaryWriter(ms);

        _out.Write(npcid);
        _out.Write(m_Rnpc_ID);
        _out.Write(m_QCID);
        _out.Write(m_CurMissionGroup);
        _out.Write(m_CurGroupTimes);
        _out.Write(mCurComMisNum);
        _out.Write(mCompletedMissionCount);
        _out.Write(m_RandomMission);
        _out.Write(m_RecruitMissionNum);

        _out.Write(m_MissionList.Count);
        for (int m = 0; m < m_MissionList.Count; m++)
            _out.Write(m_MissionList[m]);

        _out.Write(m_MissionListReply.Count);
        for (int m = 0; m < m_MissionListReply.Count; m++)
            _out.Write(m_MissionListReply[m]);

        byte[] temp = ms.ToArray();
        _out.Close();
        ms.Close();
        return temp;
    }

    public int Read(byte[] buffer)
    {
        if (buffer.Length == 0)
            return -1;
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);
        int npcid = _in.ReadInt32();
        if (npcid == -1)
            return npcid;
        m_Rnpc_ID = _in.ReadInt32();
        m_QCID = _in.ReadInt32();
        m_CurMissionGroup = _in.ReadInt32();
        m_CurGroupTimes = _in.ReadInt32();
        mCurComMisNum = _in.ReadByte();
        mCompletedMissionCount = _in.ReadInt32();
        m_RandomMission = _in.ReadInt32();
        m_RecruitMissionNum = _in.ReadInt32();

        int num = _in.ReadInt32();
        for (int j = 0; j < num; j++)
            m_MissionList.Add(_in.ReadInt32());

        num = _in.ReadInt32();
        for (int j = 0; j < num; j++)
            m_MissionListReply.Add(_in.ReadInt32());
        _in.Close();
        ms.Close();
        return npcid;
    }

	public byte[] Serialize()
	{
		return PETools.Serialize.Export((w) =>
		{
			BufferHelper.Serialize(w, mCurComMisNum);
			BufferHelper.Serialize(w, mCompletedMissionCount);
			BufferHelper.Serialize(w, m_RandomMission);
			BufferHelper.Serialize(w, m_RecruitMissionNum);
			BufferHelper.Serialize(w, m_Rnpc_ID);
			BufferHelper.Serialize(w, m_CurMissionGroup);
			BufferHelper.Serialize(w, m_CurGroupTimes);
			BufferHelper.Serialize(w, m_QCID);
			BufferHelper.Serialize(w, m_Pos);

			BufferHelper.Serialize(w, m_MissionList.Count);
			foreach (int id in m_MissionList)
				BufferHelper.Serialize(w, id);

			BufferHelper.Serialize(w, m_MissionListReply.Count);
			foreach (int id in m_MissionListReply)
				BufferHelper.Serialize(w, id);

			BufferHelper.Serialize(w, m_RecruitMissionList.Count);
			foreach (int id in m_RecruitMissionList)
				BufferHelper.Serialize(w, id);
		});
	}

	public void Deserialize(byte[] buffer)
	{
		PETools.Serialize.Import(buffer, (r) =>
		{
			mCurComMisNum = BufferHelper.ReadByte(r);
			mCompletedMissionCount = BufferHelper.ReadInt32(r);
			m_RandomMission = BufferHelper.ReadInt32(r);
			m_RecruitMissionNum = BufferHelper.ReadInt32(r);
			m_Rnpc_ID = BufferHelper.ReadInt32(r);
			m_CurMissionGroup = BufferHelper.ReadInt32(r);
			m_CurGroupTimes = BufferHelper.ReadInt32(r);
			m_QCID = BufferHelper.ReadInt32(r);
			BufferHelper.ReadVector3(r, out m_Pos);

			int count = BufferHelper.ReadInt32(r);
			for (int i = 0; i < count; i++)
			{
				int id = BufferHelper.ReadInt32(r);
				m_MissionList.Add(id);
			}

			count = BufferHelper.ReadInt32(r);
			for (int i = 0; i < count; i++)
			{
				int id = BufferHelper.ReadInt32(r);
				m_MissionListReply.Add(id);
			}

			count = BufferHelper.ReadInt32(r);
			for (int i = 0; i < count; i++)
			{
				int id = BufferHelper.ReadInt32(r);
				m_RecruitMissionList.Add(id);
			}
			m_bRandomNpc = true;
		});
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		var mission = new NpcMissionData();
		mission.mCurComMisNum = stream.Read<byte>();
		mission.mCompletedMissionCount = stream.Read<int>();
		mission.m_RandomMission = stream.Read<int>();
		mission.m_RecruitMissionNum = stream.Read<int>();
		mission.m_Rnpc_ID = stream.Read<int>();
		mission.m_CurMissionGroup = stream.Read<int>();
		mission.m_CurGroupTimes = stream.Read<int>();
		mission.m_QCID = stream.Read<int>();
		mission.m_Pos = stream.Read<Vector3>();

		int[] missionList = stream.Read<int[]>();
		mission.m_MissionList.Clear();
		mission.m_MissionList.AddRange(missionList);

		int[] missionReplyList = stream.Read<int[]>();
		mission.m_MissionListReply.Clear();
		mission.m_MissionListReply.AddRange(missionReplyList);

		int[] missionRecruitList = stream.Read<int[]>();
		mission.m_RecruitMissionList.Clear();
		mission.m_RecruitMissionList.AddRange(missionRecruitList);

		return mission;
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		var mission = (NpcMissionData)value;

		stream.Write(mission.mCurComMisNum);
		stream.Write(mission.mCompletedMissionCount);
		stream.Write(mission.m_RandomMission);
		stream.Write(mission.m_RecruitMissionNum);
		stream.Write(mission.m_Rnpc_ID);
		stream.Write(mission.m_CurMissionGroup);
		stream.Write(mission.m_CurGroupTimes);
		stream.Write(mission.m_QCID);
		stream.Write(mission.m_Pos);

		stream.Write(mission.m_MissionList.ToArray());
		stream.Write(mission.m_MissionListReply.ToArray());
		stream.Write(mission.m_RecruitMissionList.ToArray());
	}
}

public class AdNpcData
{
    public int mID;
    public int mRnpc_ID;
    public int mRecruitQC_ID;
    public int mArea;
    public List<GroupInfo> mQC_IDList = new List<GroupInfo>();
    public List<int> m_CSRecruitMissionList = new List<int>();
    public int mWild;

    public int mQC_ID
    {
        get
        {
            if (mQC_IDList.Count == 0)
                return -1;

            int count = mQC_IDList[mQC_IDList.Count - 1].radius;
            int idx = UnityEngine.Random.Range(0, count);
            for (int i = 0; i < mQC_IDList.Count; i++)
            {
                GroupInfo gi = mQC_IDList[i];
                if (idx < gi.radius)
                    return gi.id;
            }

            return -1;
        }
    }
}

public class NpcMissionDataRepository
{
    public static Dictionary<int, NpcMissionData> dicMissionData = new Dictionary<int, NpcMissionData>(10);
    public static Dictionary<int, AdNpcData> m_AdRandMisNpcData = new Dictionary<int, AdNpcData>();

    public static void Reset()
    {
        dicMissionData.Clear();
        m_AdRandMisNpcData.Clear();

        LoadData();
    }

    //将冒险模式AdData加入dicMissionData
    //public static void AdDataLoadToDicData()
    //{
    //    foreach (var item in m_AdRandMisNpcData)
    //    {
    //        AdNpcData data = item.Value;
    //        if (data == null)
    //            return;
    //        NpcMissionData useData = new NpcMissionData();
    //        useData.m_bRandomNpc = true;
    //        useData.m_Rnpc_ID = data.mRnpc_ID;
    //        useData.m_QCID = data.mQC_ID;

    //        int misid = AdRMRepository.GetRandomMission(data.mQC_ID, useData.m_CurMissionGroup);
    //        if (misid != 0)
    //            useData.m_RandomMission = misid;

    //        for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
    //            useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);

    //        if (!dicMissionData.ContainsKey(data.mID))
    //            NpcMissionDataRepository.AddMissionData(data.mID, useData);
    //    }
    //}

    public static NpcMissionData GetMissionData(int npcId)
    {
        if (dicMissionData.ContainsKey(npcId))
        {
            return dicMissionData[npcId];
        }

        return null;
    }

    public static AdNpcData GetAdNpcDataByIdx(int idx)
    {
        int count = 0;
        foreach (KeyValuePair<int, AdNpcData> iter in m_AdRandMisNpcData)
        {
            if (idx == count)
                return iter.Value;

            count++;
        }

        return null;
    }

    public static AdNpcData GetAdNpcData(int adid)
    {
        if (m_AdRandMisNpcData.ContainsKey(adid))
        {
            return m_AdRandMisNpcData[adid];
        }

        return null;
    }

    public static AdNpcData GetAdNpcDataByNpcID(int npcid)
    {
        foreach (KeyValuePair<int, AdNpcData> iter in m_AdRandMisNpcData)
        {
            if (iter.Value.mRnpc_ID == npcid)
                return iter.Value;
        }

        return null;
    }

    public static void AddMissionData(int npcId, NpcMissionData data)
    {
        if (dicMissionData.ContainsKey(npcId))
        {
            dicMissionData[npcId] = data;
            return;
        }

        dicMissionData.Add(npcId, data);
    }

    public static void LoadData()
    {
        
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPCMission");

        while (reader.Read())
        {
            NpcMissionData missionData = new NpcMissionData();
            int id = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("NPC_ID")));

            string strTemp = reader.GetString(reader.GetOrdinal("missionlist"));
            string[] missionlist = strTemp.Split(',');
            for (int i = 0; i < missionlist.Length; i++)
            {
                if (missionlist[i] != "0")
                    missionData.m_MissionList.Add(System.Convert.ToInt32(missionlist[i]));
            }

            strTemp = reader.GetString(reader.GetOrdinal("missionlistreply"));
            string[] missionlistreply = strTemp.Split(',');
            for (int i = 0; i < missionlistreply.Length; i++)
            {
                if (missionlistreply[i] != "0")
                    missionData.AddMissionListReply(System.Convert.ToInt32(missionlistreply[i]));
            }

            strTemp = reader.GetString(reader.GetOrdinal("Colony_RecruitMissionID"));
            missionlist = strTemp.Split(',');
            for (int i = 0; i < missionlist.Length; i++)
            {
                if (missionlist[i] != "0")
                    missionData.m_CSRecruitMissionList.Add(Convert.ToInt32(missionlist[i]));
            }

			NpcMissionDataRepository.AddMissionData(id, missionData);
        }

        LoadNpcRandomMissionData();
        LoadAdRandMisNpcData();
    }

    static void LoadNpcRandomMissionData()
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("RandNPCMission");
        while (reader.Read())
        {
            NpcMissionData missionData = new NpcMissionData();
            missionData.mCurComMisNum = 0;

            int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            missionData.m_Rnpc_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("RNPC_ID")));

            string tmp;
            string[] tmplist;


            tmp = reader.GetString(reader.GetOrdinal("natalplace"));
            tmplist = tmp.Split(',');
            if (tmplist.Length == 3)
            {
                float x = Convert.ToSingle(tmplist[0]);
                float y = Convert.ToSingle(tmplist[1]);
                float z = Convert.ToSingle(tmplist[2]);
                missionData.m_Pos = new Vector3(x, y, z);
            }

            tmp = reader.GetString(reader.GetOrdinal("Rmissionlist"));
            if (tmp != "0")
            {
                missionData.m_RandomMission = Convert.ToInt32(tmp);
            }

            tmp = reader.GetString(reader.GetOrdinal("missionlist"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] != "0")
                    missionData.m_MissionList.Add(Convert.ToInt32(tmplist[i]));
            }

            tmp = reader.GetString(reader.GetOrdinal("RecruitMissionID"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] != "0")
                    missionData.m_RecruitMissionList.Add(Convert.ToInt32(tmplist[i]));
            }

            missionData.mCompletedMissionCount = Convert.ToInt32(reader.GetString(reader.GetOrdinal("RecruitMissionNum")));

            tmp = reader.GetString(reader.GetOrdinal("Colony_RecruitMissionID"));
            tmplist = tmp.Split(',');
            for (int i = 0; i < tmplist.Length; i++)
            {
                if (tmplist[i] != "0")
                    missionData.m_CSRecruitMissionList.Add(Convert.ToInt32(tmplist[i]));
            }

            missionData.m_bRandomNpc = true;
			NpcMissionDataRepository.AddMissionData(id, missionData);
        }
    }

    public static void LoadAdRandMisNpcData()
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdRandNPCMission");
        while (reader.Read())
        {
            AdNpcData adNpcData = new AdNpcData();
            adNpcData.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
            adNpcData.mRnpc_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("RNPC_ID")));
            adNpcData.mRecruitQC_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("RecruitQC_ID")));
            adNpcData.mArea = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Area")));
            adNpcData.mWild = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Wild")));

            string strTmp = reader.GetString(reader.GetOrdinal("QC_ID"));
            string[] tmpList0 = strTmp.Split('_');
            string[] tmpList = tmpList0[0].Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                string[] listTmp1 = tmpList[i].Split(',');
                if (listTmp1.Length != 2)
                    continue;

                GroupInfo gi;
                gi.id = Convert.ToInt32(listTmp1[0]);
                gi.radius = Convert.ToInt32(listTmp1[1]);
                adNpcData.mQC_IDList.Add(gi);
            }

            strTmp = reader.GetString(reader.GetOrdinal("Colony_RecruitMissionChainID"));
            tmpList = strTmp.Split(',');
            for (int i = 0; i < tmpList.Length; i++)
            {
                if (tmpList[i] != "0")
                    adNpcData.m_CSRecruitMissionList.Add(Convert.ToInt32(tmpList[i]));
            }

            m_AdRandMisNpcData.Add(adNpcData.mID, adNpcData);
        }
    }

    public static void AddReplyMission(int npcid, int id)
    {
        NpcMissionData npc = GetMissionData(npcid);

        if (npc == null)
            return;

        npc.AddMissionListReply(id);
    }

    public static List<int> GetAdRandListByWild(int wild)
    {
        List<int> adlist = new List<int>();

        foreach (KeyValuePair<int, AdNpcData> iter in m_AdRandMisNpcData)
        {
            if (iter.Value.mWild == wild)
                adlist.Add(iter.Key);
        }

        return adlist;
    }

	public static int GetRNpcId(int id){
		if (m_AdRandMisNpcData.ContainsKey(id))
		{
			return m_AdRandMisNpcData[id].mRnpc_ID;
		}
		
		return -1;
	}
}