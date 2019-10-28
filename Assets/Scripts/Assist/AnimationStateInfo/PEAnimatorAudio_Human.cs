using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine.Experimental.Director;
using Pathea;

public class HumanSoundData
{
    public int id;
    public int sex;
    public List<int> owners;
    public List<List<KeyValuePair<int, float>>> sounds;

    static List<HumanSoundData> s_HumanSoundData;

    public static void LoadData()
    {
        s_HumanSoundData = new List<HumanSoundData>();

        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("human_effects");

        while (reader.Read())
        {
            HumanSoundData data = new HumanSoundData();
            data.sounds = new List<List<KeyValuePair<int, float>>>();
            data.id = reader.GetInt32(reader.GetOrdinal("ID"));
            data.sex = reader.GetInt32(reader.GetOrdinal("Sex"));
            data.owners = new List<int>(PETools.PEUtil.ToArrayInt32(reader.GetString(reader.GetOrdinal("Owner")), ','));

            string tmpStr = reader.GetString(reader.GetOrdinal("Effects"));
            string[] soundStr = PETools.PEUtil.ToArrayString(tmpStr, ';');
            foreach (string str in soundStr)
            {
                List<KeyValuePair<int, float>> tmpList = new List<KeyValuePair<int, float>>();
                string[] keyStr = PETools.PEUtil.ToArrayString(str, ',');
                //foreach (string key in keyStr)
                for(int i = 0; i < keyStr.Length;i++)
                {
                    string[] dataStr = str.Split(new char[] { '_' });
                    if (dataStr.Length == 2)
                    {
                        int id = System.Convert.ToInt32(dataStr[0]);
                        float value = System.Convert.ToSingle(dataStr[1]);
                        tmpList.Add(new KeyValuePair<int, float>(id, value));
                    }
                }

                data.sounds.Add(tmpList);
            }

            s_HumanSoundData.Add(data);
        }
    }

    public static int[] GetSoundID(int id, int sex, int owner = 0)
    {
        HumanSoundData data = s_HumanSoundData.Find(ret => ret.id == id && ret.sex == sex /*&& ret.owners.Contains(owner)*/);
        if (data != null)
            return data.GetPlaySoundID();

        return new int[0];
    }

    int[] GetPlaySoundID()
    {
        List<int> tmpList = new List<int>();
        foreach (List<KeyValuePair<int, float>> keyPair in sounds)
        {
            float random = Random.value;
            float value = 0.0f;
            foreach (KeyValuePair<int, float> pair in keyPair)
            {
                value += pair.Value;
                if(random <= value)
                {
                    tmpList.Add(pair.Key);
                    break;
                }
            }
        }

        return tmpList.ToArray();
    }
}

public class PEAnimatorAudio_Human : PEAnimatorAudio
{
    public int id;
    public float maleDelayTime;
    public float femaleDelayTime;

    int m_Gender;
    bool m_Trigger;
    int[] m_Sounds;

    float GetDelayTime(int gender)
    {
        if (gender == 1)
            return femaleDelayTime;
        else if (gender == 2)
            return maleDelayTime;
        else
            return 10.0f;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_Trigger = false;

        if(m_Gender == 0)
        {
            PeEntity entity = animator.GetComponentInParent<PeEntity>();
            if(entity != null && entity.commonCmpt != null)
                m_Gender = (int)entity.commonCmpt.sex;
        }

        if(m_Gender == 1 || m_Gender == 2)
        {
            if(m_Sounds == null)
            {
                m_Sounds = HumanSoundData.GetSoundID(id, m_Gender);
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if ((m_Gender != 1 && m_Gender != 2) || id <= 0 || m_Sounds == null || m_Sounds.Length == 0)
            return;

        if (!m_Trigger)
        {
            if (stateInfo.normalizedTime >= GetDelayTime(m_Gender))
            {
                m_Trigger = true;

                for (int i = 0; i < m_Sounds.Length; i++)
                {
                    AudioManager.instance.Create(animator.transform.position, m_Sounds[i]);
                }
            }
        }
    }
}
