using UnityEngine;
using Mono.Data.SqliteClient;
using System.Collections;
using System.Collections.Generic;
using System;

public class TalkData
{
    public TalkData() 
    {
        m_otherNpc = new List<int>();
        m_endOtherNpc = new List<int>();
    }
    public int m_ID;            //ID
    public int m_NpcID;
    public List<int> m_otherNpc;//需要站定的其他NPC
    public string m_Content;    //对话内容
    public int m_SoundID;       //声音
    public string m_ClipName;   //动作名称
    public bool isRadio;        //是否是无线电
    public int needLangSkill;
    public object talkToNpcidOrVecter3;
    public object moveTonpcidOrvecter3;
    public int m_moveType;      //NPC移动到指定位置的方式
    public List<int> m_endOtherNpc;//结束其他NPC的对话行为
}

public class TalkRespository
{
    public static Dictionary<int, TalkData> m_TalkMap = new Dictionary<int, TalkData>();

    public static TalkData GetTalkData(int TalkID)
    {
        return m_TalkMap.ContainsKey(TalkID) ? m_TalkMap[TalkID] : null;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Scenario");
        while (reader.Read())
        {
            TalkData data = new TalkData();
            string temp;
            string[] temp1;
            int strid;
            data.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("talk_id")));
            data.m_NpcID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("npc_id")));
            temp = reader.GetString(reader.GetOrdinal("OtherNPC"));
            if (temp != "0")
            {
                temp1 = temp.Split(',');
                foreach (var item in temp1)
                    data.m_otherNpc.Add(Convert.ToInt32(item));
            }

            temp = reader.GetString(reader.GetOrdinal("talk_content"));
            temp1 = temp.Split(':');
            if (temp1.Length == 3)
            {
                strid = Convert.ToInt32(temp1[0]);
                data.m_Content  = PELocalization.GetString(strid);
                data.m_SoundID  = Convert.ToInt32(temp1[1]);
                data.m_ClipName = temp1[2];
            }
            strid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("isRadio")));
            data.isRadio = strid == 1 ? true : false;
            data.needLangSkill = Convert.ToInt32(reader.GetString(reader.GetOrdinal("LanguageSkill")));

            temp = reader.GetString(reader.GetOrdinal("TalkTo"));
            temp1 = temp.Split(',');
            if (temp1.Length == 3)
            {
                float x, y, z;
                x = Convert.ToSingle(temp1[0]);
                y = Convert.ToSingle(temp1[1]);
                z = Convert.ToSingle(temp1[2]);
                data.talkToNpcidOrVecter3 = new Vector3(x, y, z);
            }
            else if (temp1.Length == 1)
                data.talkToNpcidOrVecter3 = Convert.ToInt32(temp1[0]);

            temp = reader.GetString(reader.GetOrdinal("MoveTo"));
            temp1 = temp.Split(',');
            if (temp1.Length == 3)
            {
                float x, y, z;
                x = Convert.ToSingle(temp1[0]);
                y = Convert.ToSingle(temp1[1]);
                z = Convert.ToSingle(temp1[2]);
                data.moveTonpcidOrvecter3 = new Vector3(x, y, z);
            }
            else if (temp1.Length == 1)
                data.moveTonpcidOrvecter3 = Convert.ToInt32(temp1[0]);

            data.m_moveType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("MoveSpeed")));

            temp = reader.GetString(reader.GetOrdinal("EndNPCTalk"));
            if (temp != "0")
            {
                temp1 = temp.Split(',');
                foreach (var item in temp1)
                    data.m_endOtherNpc.Add(Convert.ToInt32(item));
            }

            m_TalkMap.Add(data.m_ID, data);
        }
    }
}
