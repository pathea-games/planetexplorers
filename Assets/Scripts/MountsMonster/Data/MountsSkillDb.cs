using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System;
using DbField = PETools.DbReader.DbFieldAttribute;

public class MountsSkillDb
{
    private static  int[] _mounstersIndex;//key:monsterProtoID,value:id
    private static  int[][][] _MskillDatas;//key0:monsterProtoID ;key1:MountsSkillKey;key2:index(monsterSkills)
    private static Dictionary<int, string> _prounceData;
    public static void LoadData()
    {
        Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MountsSkill");
        _mounstersIndex = new int[100];
        _MskillDatas = new int[50][][];
        _prounceData = new Dictionary<int, string>();
        try
        {
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
                int MonsterID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("MonsterID")));
                _mounstersIndex[MonsterID] = id;

                _MskillDatas[id] = new int[(int)Pathea.MountsSkillKey.Max][];
                _MskillDatas[id][(int)Pathea.MountsSkillKey.Mskill_L] = PETools.PEUtil.ToArrayInt32(reader.GetString(reader.GetOrdinal("SkillLeft")), ',');
                _MskillDatas[id][(int)Pathea.MountsSkillKey.Mskill_Space] = PETools.PEUtil.ToArrayInt32(reader.GetString(reader.GetOrdinal("SkillSpace")), ',');

                string[] str0 =  PETools.PEUtil.ToArrayString(reader.GetString(reader.GetOrdinal("SkillJump")), ',');
                List<int> jum = new List<int>();
                if(str0 != null)
                {
                    for(int i=0;i<str0.Length;i++)
                    {
                       string[] str1 = PETools.PEUtil.ToArrayString(str0[i], ':');
                        if(str1.Length == 2)
                        {
                            jum.Add(System.Convert.ToInt32(str1[0]));
                            _prounceData.Add(System.Convert.ToInt32(str1[0]), str1[1]);
                        }
                    }
                }
                _MskillDatas[id][(int)Pathea.MountsSkillKey.Mskill_pounce] = jum.ToArray();
                _MskillDatas[id][(int)Pathea.MountsSkillKey.Mskill_tame] = PETools.PEUtil.ToArrayInt32(reader.GetString(reader.GetOrdinal("RideAnim")), ',');
            }
        }
        catch
        {
            Debug.LogError("mounstsSKill !!!!");
        }
       
    }

    public static void Relese()
    {
        _mounstersIndex = null;
        _MskillDatas = null;
        _prounceData = null;

    }

    public static  int[] GetSkills(int monstersProID, Pathea.MountsSkillKey key)
    {
        return _MskillDatas != null && _mounstersIndex[monstersProID] != 0 && _MskillDatas[_mounstersIndex[monstersProID]] != null
            ? _MskillDatas[_mounstersIndex[monstersProID]][(int)key] : null ;
    }

    public static int GetRandomSkill(int monstersProID, Pathea.MountsSkillKey key)
    {
        int[] skills = GetSkills(monstersProID,key);
        return skills != null && skills.Length >0 ? skills[UnityEngine.Random.Range(0, skills.Length)] : 0;
    }

    public static string GetpounceAnim(int pounceSkillId)
    {
        return _prounceData !=null && _prounceData.ContainsKey(pounceSkillId) ? _prounceData[pounceSkillId] : "";
    }
}


