using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using Pathea;
using SkillSystem;

public class RepProcessor
{
    public enum ERepType
    {
        Player_Puja,
        Player_Paja,
    }
    struct RepVal
    {
        public int _id;
        public int[] _vals;
    }
    static Dictionary<int, RepVal> s_dicRepVals = new Dictionary<int, RepVal>();
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("ReputationValue");
        int idxId = reader.GetOrdinal("ID");
        int idxFear = reader.GetOrdinal("Fear");
        int idxHate = reader.GetOrdinal("Hatred");
        int idxAnim = reader.GetOrdinal("Animosity");
        int idxCold = reader.GetOrdinal("Cold");
        int idxNeut = reader.GetOrdinal("Neutral");
        int idxCord = reader.GetOrdinal("Cordial");
        int idxAmit = reader.GetOrdinal("Amity");
        int idxResp = reader.GetOrdinal("Respectful");
        int idxRevr = reader.GetOrdinal("Reverence");
        while (reader.Read())
        {
            RepVal desc = new RepVal();
            desc._id = reader.GetInt32(idxId);
            desc._vals = new int[(int)ReputationSystem.ReputationLevel.MAX];
            desc._vals[(int)ReputationSystem.ReputationLevel.Fear] = reader.GetInt32(idxFear);
            desc._vals[(int)ReputationSystem.ReputationLevel.Hatred] = reader.GetInt32(idxHate);
            desc._vals[(int)ReputationSystem.ReputationLevel.Animosity] = reader.GetInt32(idxAnim);
            desc._vals[(int)ReputationSystem.ReputationLevel.Cold] = reader.GetInt32(idxCold);
            desc._vals[(int)ReputationSystem.ReputationLevel.Neutral] = reader.GetInt32(idxNeut);
            desc._vals[(int)ReputationSystem.ReputationLevel.Cordial] = reader.GetInt32(idxCord);
            desc._vals[(int)ReputationSystem.ReputationLevel.Amity] = reader.GetInt32(idxAmit);
            desc._vals[(int)ReputationSystem.ReputationLevel.Respectful] = reader.GetInt32(idxResp);
            desc._vals[(int)ReputationSystem.ReputationLevel.Reverence] = reader.GetInt32(idxRevr);
            s_dicRepVals.Add(desc._id, desc);
        }
    }

    public static void OnMonsterDeath(SkEntity cur, SkEntity caster)
    {
        caster = PETools.PEUtil.GetCaster(caster);
        SkAliveEntity curAlive = cur as SkAliveEntity;
        SkAliveEntity casAlive = caster as SkAliveEntity;
        if (curAlive != null && casAlive != null)
        {
            int curPlayerID = Mathf.RoundToInt(curAlive.GetAttribute(AttribType.DefaultPlayerID));
            if (ReputationSystem.IsReputationTarget(curPlayerID))
            {
                int casPlayerID = Mathf.RoundToInt(casAlive.GetAttribute(AttribType.DefaultPlayerID));
                int casLvl = (int)ReputationSystem.Instance.GetReputationLevel(casPlayerID, curPlayerID);   //operate value depend the one be killed
                //lz-2017.05.08 冒险模式如果声望等级大于Cold的时候，攻击一下直接掉为Cold
                if (PeGameMgr.IsAdventure && casLvl > (int)ReputationSystem.ReputationLevel.Cold)
                {
                    int targetLvl = (int)ReputationSystem.ReputationLevel.Cold;
                    int reduceValue = 0;
                    for (int i = targetLvl; i < casLvl; i++)
                    {
                        reduceValue -= ReputationSystem.GetLevelThreshold((ReputationSystem.ReputationLevel)i);
                    }
                    ReputationSystem.Instance.ChangeReputationValue(casPlayerID, curPlayerID, reduceValue, true);
                }
                else
                {
                    MonsterProtoDb.Item protoItem = MonsterProtoDb.Get(curAlive.Entity.entityProto.protoId);
                    RepVal repVal;
                    if (protoItem != null && s_dicRepVals.TryGetValue(protoItem.repValId, out repVal))
                    {
                        int reduceValue = (int)(repVal._vals[casLvl] * ReputationSystem.ChangeValueProportion);         //reduceValue = addvalue * ChangeValueProportion;
                        ReputationSystem.Instance.ChangeReputationValue(casPlayerID, curPlayerID, reduceValue, true);   // reudce self no mater other side would be add;
                    }
                }
            }
        }
    }

    public static void OnDoodadDeath(SkEntity cur, SkEntity caster)
    {
        caster = PETools.PEUtil.GetCaster(caster);
        SkAliveEntity curAlive = cur as SkAliveEntity;
        SkAliveEntity casAlive = caster as SkAliveEntity;
        if (curAlive != null && casAlive != null)
        {
            int curPlayerID = Mathf.RoundToInt(curAlive.GetAttribute(AttribType.DefaultPlayerID));
            if (ReputationSystem.IsReputationTarget(curPlayerID))
            {
                int casPlayerID = Mathf.RoundToInt(casAlive.GetAttribute(AttribType.DefaultPlayerID));
                int casLvl = (int)ReputationSystem.Instance.GetReputationLevel(casPlayerID, curPlayerID);   //operate value depend the one be killed
                //lz-2017.05.11 冒险模式如果声望等级大于Cold的时候，攻击一下直接掉为Cold
                if (PeGameMgr.IsAdventure && casLvl > (int)ReputationSystem.ReputationLevel.Cold)
                {
                    int targetLvl = (int)ReputationSystem.ReputationLevel.Cold;
                    int reduceValue = 0;
                    for (int i = targetLvl; i < casLvl; i++)
                    {
                        reduceValue -= ReputationSystem.GetLevelThreshold((ReputationSystem.ReputationLevel)i);
                    }
                    ReputationSystem.Instance.ChangeReputationValue(casPlayerID, curPlayerID, reduceValue, true);
                }
                else
                {
                    DoodadProtoDb.Item protoItem = DoodadProtoDb.Get(curAlive.Entity.entityProto.protoId);
                    RepVal repVal;
                    if (protoItem != null && s_dicRepVals.TryGetValue(protoItem.repValId, out repVal))
                    {
                        int reduceValue = (int)(repVal._vals[casLvl] * ReputationSystem.ChangeValueProportion);         //reduceValue = addvalue * ChangeValueProportion;
                        ReputationSystem.Instance.ChangeReputationValue(casPlayerID, curPlayerID, reduceValue, true);   // reudce self no mater other side would be add;
                    }
                }
            }
        }
    }

}
