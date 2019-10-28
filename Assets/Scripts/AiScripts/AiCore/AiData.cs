using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public enum LifeHabit { Day, night }
public enum Dietary { Herbivorous, Carnivorous }
public enum Nature { Land, Water, Sky, Amphibious }
public enum Invade { Initiative, Passive }

namespace AiAsset
{
    public class AiData
    {
        public static void LoadData()
        {
            AiHatredData.LoadData();
            AiHarmData.LoadData();
            AiDataBlock.LoadData();
            AiDamageTypeData.LoadData();

            AISpawnDataRepository.LoadData();
        }
    }

    /* *怪物属性加载 */
    public class AiDataBlock
    {
        public struct DropData
        {
            public int id;
            public float pro;
        }

        public class ItemDrop
        {
            public int count;
            public List<DropData> DropList = new List<DropData>();

            public void Clear()
            {
                count = 0;
                DropList.Clear();
            }
        }

        public Dietary dietary = Dietary.Herbivorous;
        public Nature nature = Nature.Land;

        public bool cavern = false;
        public bool darkness = false;
        public bool social = false;
        public bool isboss = false;

        public string name = "";
        public string uiName = "";
        public string xmlPath = "";
        public int dataId = 0;
        public int model = 0;
        public int camp = 0;
        public int harm = 0;
        public int life = 100;
        public int music = 0;
        public int attackType = 8;
        public int defenseType = 7;
        public float walkSpeed = 5;
        public float runSpeed = 10;
        public float turnSpeed = 5;
        public float jumpHeight = 2;
        public int damage = 100;
        public int buildDamage = 100;
        public int defence = 100;
        public float wanderRange = 10;
        public float moveRange = 10;
        public float alertRange = 10;
        public float chaseRange = 10;
        public float hearRange = 8;
        public float horizonRange = 8;
        public float attackRangeMin = 2;
        public float attackRangeMax = 60;
        public float attackAngle = 30;
        public float pitchAngle = 60;
        public float horizonAngle = 60;
        public float escapeRange = 10;

        public float minScale = 1.0f;
        public float maxScale = 1.0f;
        //public string itemDrop = "";
        public int strongWeight = 0;
        public int normalWeight = 0;
        public int sickWeight = 0;
        public int pregnancyWeight = 0;
        public float pregnancyTime = 0;
        public float eggDrop = 0;

        public float restTimeMin = 0;
        public float restTimeMax = 0;
        public float fatigueMulDay = 0;
        public float fatigueMulNight = 0;
        public float wakeRate = 0;
        public int deathSkill = 0;
        public int drinkSkill = 0;
        public int sleepSkill = 0;
        public float callRate = 0;
        public float calledRate = 0;
        public float escapePercent = 0;

        public float damageSimulate;
        public float maxHpSimulate;

        public int colonyLevDamage;
        public int colonyLevEscape;

        public float colonyEscapeValue;
        public float colonyEscapeRate;

        public int[] deathEffect;
        public int[] equipmentIDs;
        public int[] lifeFormIDs;
        public SkillData[] attackSkill;
        //public Dictionary<int, ItemDrop> ItemDropMap = new Dictionary<int, ItemDrop>();
        public ItemDrop m_ItemDrop = new ItemDrop();
        //public Min_Max_Int MeatDrop;
        //public List<ID_Num> MissDropList = new List<ID_Num>();

        int mCamp = 0;
        int mHarm = 0;

        public int Camp { get { return mCamp; } }
        public int Harm { get { return mHarm; } }

        private static Dictionary<int, AiDataBlock> m_data = new Dictionary<int, AiDataBlock>();

        public static AiDataBlock GetAIDataBase(int pID)
        {
            return m_data.ContainsKey(pID) ? m_data[pID] : null;
        }

        public static void Reset()
        {
            foreach (KeyValuePair<int, AiDataBlock> kv in m_data)
            {
                kv.Value.ResetCamp();
                kv.Value.ResetHarm();
            }
        }

        public static AiDataBlock GetAiDataBaseByAiName(string name)
        {
            foreach (AiDataBlock item in m_data.Values)
            {
                if (item != null && item.xmlPath.Equals(name))
                    return item;
            }

            return null;
        }

        public static void SetCamp(int dataID, int camp)
        {
            AiDataBlock data = GetAIDataBase(dataID);

            if (data != null && data.mCamp != camp)
            {
                data.mCamp = camp;
            }
        }

        public static void SetHarm(int dataID, int harm)
        {
            AiDataBlock data = GetAIDataBase(dataID);

            if (data != null && data.mHarm != harm)
            {
                data.mHarm = harm;
            }
        }

        public static string GetAIDataName(int id)
        {
            if (m_data.ContainsKey(id))
                return m_data[id].name;

            return "";
        }

        public static string GetIconName(int id)
        {
            if (m_data.ContainsKey(id))
                return m_data[id].uiName;

            return "";
        }

        public static ItemDrop GetItemDrop(int id)
        {
            AiDataBlock data = GetAIDataBase(id);
            if (data == null)
                return null;

            return data.m_ItemDrop;
        }

        public static void LoadData()
        {

            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("ai_data");

            while (reader.Read())
            {

                AiDataBlock data = new AiDataBlock();

                data.dataId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("monster_ID")));
                data.uiName = reader.GetString(reader.GetOrdinal("ui_name"));
                data.name = PELocalization.GetString(Convert.ToInt32(reader.GetString(reader.GetOrdinal("eng_name"))));
                data.xmlPath = reader.GetString(reader.GetOrdinal("tree_path"));
                data.model = Convert.ToInt32(reader.GetString(reader.GetOrdinal("model_ID")));
                data.nature = (Nature)Convert.ToInt32(reader.GetString(reader.GetOrdinal("environment")));
                data.dietary = (Dietary)Convert.ToInt32(reader.GetString(reader.GetOrdinal("feeding_habits")));
                data.social = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("social"))));
                data.cavern = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("cavern"))));
                data.darkness = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("darkness"))));
                data.isboss = Convert.ToBoolean(Convert.ToInt32(reader.GetString(reader.GetOrdinal("isboss"))));
                data.camp = Convert.ToInt32(reader.GetString(reader.GetOrdinal("camp")));
                data.harm = Convert.ToInt32(reader.GetString(reader.GetOrdinal("harm")));
                data.damage = Convert.ToInt32(reader.GetString(reader.GetOrdinal("attack")));
                data.buildDamage = Convert.ToInt32(reader.GetString(reader.GetOrdinal("damage")));
                data.defence = Convert.ToInt32(reader.GetString(reader.GetOrdinal("defense")));
                data.life = Convert.ToInt32(reader.GetString(reader.GetOrdinal("hp")));
                data.music = Convert.ToInt32(reader.GetString(reader.GetOrdinal("music")));
                data.attackType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("attack_type")));
                data.defenseType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("defense_type")));
                data.walkSpeed = Convert.ToSingle(reader.GetString(reader.GetOrdinal("walking_speed")));
                data.runSpeed = Convert.ToSingle(reader.GetString(reader.GetOrdinal("running_speed")));
                data.jumpHeight = Convert.ToSingle(reader.GetString(reader.GetOrdinal("jump_height")));
                data.alertRange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("alert_rad")));
                data.wanderRange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("patrol_rad")));
                data.moveRange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("active_range")));
                //data.chaseRange  = Convert.ToSingle(reader.GetString(reader.GetOrdinal("chase_distance")));
                data.horizonAngle = Convert.ToSingle(reader.GetString(reader.GetOrdinal("horizon_angle")));
                //data.pitchAngle = Convert.ToSingle(reader.GetString(reader.GetOrdinal("pitch_angle")));
                data.horizonRange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("horizon_rad")));
				data.hearRange    = Convert.ToSingle(reader.GetString(reader.GetOrdinal("hear_rad")));
                //data.hearRange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("hear_rad")));
                data.attackRangeMin = Convert.ToSingle(reader.GetString(reader.GetOrdinal("damage_rad_min")));
                data.attackRangeMax = Convert.ToSingle(reader.GetString(reader.GetOrdinal("damage_rad_max")));
                data.attackAngle = Convert.ToSingle(reader.GetString(reader.GetOrdinal("damage_angle")));
                data.turnSpeed = Convert.ToSingle(reader.GetString(reader.GetOrdinal("turn_speed")));
                data.escapeRange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("escape_distance")));
                data.damageSimulate = Convert.ToSingle(reader.GetString(reader.GetOrdinal("dps")));
                data.maxHpSimulate = Convert.ToSingle(reader.GetString(reader.GetOrdinal("hp_relative")));
                //data.itemDrop = reader.GetString(reader.GetOrdinal("item_drop"));
                //				data.strongWeight = Convert.ToInt32(reader.GetString(reader.GetOrdinal("strong")));
                //				data.normalWeight = Convert.ToInt32(reader.GetString(reader.GetOrdinal("normal")));
                //				data.sickWeight = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sick")));
                //				data.pregnancyWeight = Convert.ToInt32(reader.GetString(reader.GetOrdinal("pregnancy")));
                data.pregnancyTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("pregnancy_time")));
                data.eggDrop = Convert.ToSingle(reader.GetString(reader.GetOrdinal("egg_drop")));

                data.restTimeMin = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_restTimeMin")));
                data.restTimeMax = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_restTimeMax")));
                data.fatigueMulDay = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_fatigueMulDaytime")));
                data.fatigueMulNight = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_fatigueMulNight")));
                data.wakeRate = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_sleepWaked")));
                data.deathSkill = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_deathSkill")));
                data.colonyLevDamage = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_DSLvInjureEnable")));
                data.colonyLevEscape = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_DSLvEscape")));

                string colonyString = reader.GetString(reader.GetOrdinal("_DSEscapeInfo"));
                string[] infos = AiUtil.Split(colonyString, ',');
                if (infos.Length == 2)
                {
                    data.colonyEscapeValue = Convert.ToSingle(infos[0]);
                    data.colonyEscapeRate = Convert.ToSingle(infos[1]);
                }

                string deathParticleStr = reader.GetString(reader.GetOrdinal("_deathEffect"));
                if (deathParticleStr != "0")
                {
                    string[] deathParticle = deathParticleStr.Split(',');
                    data.deathEffect = new int[deathParticle.Length];
                    for (int i = 0; i < data.deathEffect.Length; i++)
                    {
                        data.deathEffect[i] = Convert.ToInt32(deathParticle[i]);
                    }
                }

                string equipmentStr = reader.GetString(reader.GetOrdinal("_equip"));
                if (!string.IsNullOrEmpty(equipmentStr))
                {
                    string[] equipments = equipmentStr.Split(',');
                    data.equipmentIDs = new int[equipments.Length];
                    for (int i = 0; i < data.equipmentIDs.Length; i++)
                    {
                        data.equipmentIDs[i] = Convert.ToInt32(equipments[i]);
                    }
                }

                data.drinkSkill = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_drinkSkill")));
                data.sleepSkill = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_sleepSkill")));

                string attackStr = reader.GetString(reader.GetOrdinal("_attackSkill"));
                if (attackStr != "0")
                {
                    string[] attackSkills = attackStr.Split(',');

                    data.attackSkill = new SkillData[attackSkills.Length];
                    for (int i = 0; i < data.attackSkill.Length; i++)
                    {
                        //Debug.LogError(attackSkills[i]);
                        string[] skillData = attackSkills[i].Split(':');
                        data.attackSkill[i].id = Convert.ToInt32(skillData[0]);
                        data.attackSkill[i].probability = Convert.ToSingle(skillData[1]);
                    }
                }

                data.callRate = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_callProbability")));
                data.calledRate = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_calledProbability")));
                data.escapePercent = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_escapeHpPercent")));

                string strTmp = reader.GetString(reader.GetOrdinal("item_drop"));
                string[] strlist0 = strTmp.Split(';');
                if (strlist0.Length == 2)
                {
                    int count = Convert.ToInt32(strlist0[0]);
                    if (count > 0)
                    {
                        //ItemDrop itemDrop = new ItemDrop();
                        //itemDrop.count = count;

                        //string[] stritemlist = strlist0[1].Split(',');
                        //if(stritemlist.Length > 0)
                        //{
                        //    for (int i = 0; i < stritemlist.Length; i++)
                        //    {
                        //        string[] strlist1 = stritemlist[i].Split('_');
                        //        if (strlist1.Length == 2)
                        //        {
                        //            DropData dropData = new DropData();
                        //            dropData.id = Convert.ToInt32(strlist1[0]);
                        //            dropData.pro = Convert.ToSingle(strlist1[1]);
                        //            itemDrop.DropList.Add(dropData);
                        //        }
                        //    }
                        //}
                        //data.ItemDropMap.Add(data.dataId, itemDrop);

                        data.m_ItemDrop.count = count;

                        string[] stritemlist = strlist0[1].Split(',');
                        if (stritemlist.Length > 0)
                        {
                            for (int i = 0; i < stritemlist.Length; i++)
                            {
                                string[] strlist1 = stritemlist[i].Split('_');
                                if (strlist1.Length == 2)
                                {
                                    DropData dropData = new DropData();
                                    dropData.id = Convert.ToInt32(strlist1[0]);
                                    dropData.pro = Convert.ToSingle(strlist1[1]);
                                    data.m_ItemDrop.DropList.Add(dropData);
                                }
                            }
                        }

                    }
                }

                //data.MeatDrop = new Min_Max_Int();
                strTmp = reader.GetString(reader.GetOrdinal("meat_drop"));
                strlist0 = strTmp.Split(';');
                if (strlist0.Length == 2)
                {
                    //data.MeatDrop.m_Min = Convert.ToInt32(strlist0[0]);
                    //data.MeatDrop.m_Max = Convert.ToInt32(strlist0[1]);
                }

                strTmp = reader.GetString(reader.GetOrdinal("item_carry"));
                strlist0 = strTmp.Split(';');
                for (int i = 0; i < strlist0.Length; i++)
                {
                    string[] strlist1 = strlist0[i].Split(',');
                    if (strlist1.Length == 2)
                    {
                        //ID_Num md = new ID_Num();
                        //md.id = Convert.ToInt32(strlist1[0]);
                        //md.missionId = md.id % 10000 / 10;
                        //md.num = Convert.ToInt32(strlist1[1]);

                        //data.MissDropList.Add(md);
                    }
                }

                string lifeForms = reader.GetString(reader.GetOrdinal("_lfrList"));
                string[] lifes = AiUtil.Split(lifeForms, ',');
                data.lifeFormIDs = new int[lifes.Length];
                for (int i = 0; i < data.lifeFormIDs.Length; i++)
                {
                    data.lifeFormIDs[i] = Convert.ToInt32(lifes[i]);
                }

                data.ResetCamp();
                data.ResetHarm();

                m_data.Add(data.dataId, data);
            }
        }

        public void ResetCamp()
        {
            mCamp = camp;
        }

        public void ResetHarm()
        {
            mHarm = harm;
        }
    }

    /* *仇恨列表加载 */
    public class AiHatredData
    {
        public static int MaxHatredIndex = 10000;
        public static int PlayerCamp = 1;

        public string m_camName;
        public int m_campID;
        public int[] m_campData;
        public static int CampCount = 0;

        public static List<AiHatredData> s_tblCampData;

        public static void LoadData()
        {
            s_tblCampData = new List<AiHatredData>();
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("ai_campnew");
            CampCount = reader.FieldCount - 3;

            //reader.Read();

            while (reader.Read())
            {
                AiHatredData campData = new AiHatredData();
                campData.m_campData = new int[CampCount];
                campData.m_campID = Convert.ToInt32(reader.GetString(0));
                campData.m_camName = reader.GetString(1);

                for (int i = 0; i < CampCount; i++)
                {
                    campData.m_campData[i] = Convert.ToInt32(reader.GetString(i + 3));
                }
                s_tblCampData.Add(campData);
            }
        }

        public static AiHatredData GetHatredData(int campID)
        {
            foreach (AiHatredData data in s_tblCampData)
            {
                if (data.m_campID == campID)
                {
                    AiHatredData _data = new AiHatredData();
                    _data = data;
                    return _data;
                }
            }

            return null;
        }

		/// <summary>
		/// Gets the hatred value.
		/// 多人玩家阵营均大于MaxHatredIndex
		/// 玩家阵营之间的仇恨根据模式不同而不同
		/// 玩家阵营（包括炮塔等）与普通AI阵营的仇恨由普通AI阵营仇恨列表决定（相互一致，主要影响玩家炮塔与普通AI的仇恨）
		/// </summary>
		/// <returns>The hatred value.</returns>
		/// <param name="srcCamp">Source camp.</param>
		/// <param name="dstCamp">Dst camp.</param>
        public static int GetHatredValue(int srcCamp, int dstCamp)
        {
			if (srcCamp <= -1 || dstCamp <= -1)
				return 0;

			if (srcCamp >= MaxHatredIndex && dstCamp >= MaxHatredIndex)
            {
				if (Pathea.PeGameMgr.IsMultiVS)
					return srcCamp == dstCamp ? 0 : 10;
				else
					return srcCamp == dstCamp ? 0 : 0;
            }

			int src = srcCamp >= MaxHatredIndex ? PlayerCamp : srcCamp;
			int dst = dstCamp >= MaxHatredIndex ? PlayerCamp : dstCamp;

			AiHatredData data = s_tblCampData.Find(ret => ret.m_campID == src);
			if (data == null || dst >= data.m_campData.Length)
				return 0;

			return data.m_campData[dst];
        }
    }

    public class AiHarmData
    {
        public static int MaxHarmIndex = 10000;
        public static int PlayerHarm = 1;

        public string m_harmName;
        public int m_harmID;
        public int[] m_harmData;
        public static int HarmCount = 0;

        public static List<AiHarmData> s_tblHarmData;

        public static void LoadData()
        {
            s_tblHarmData = new List<AiHarmData>();
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("ai_harmdata");
            HarmCount = reader.FieldCount - 3;

            //reader.Read();

            while (reader.Read())
            {
                AiHarmData campData = new AiHarmData();
                campData.m_harmData = new int[HarmCount];
                campData.m_harmID = Convert.ToInt32(reader.GetString(0));
                campData.m_harmName = reader.GetString(1);

                for (int i = 0; i < HarmCount; i++)
                {
                    campData.m_harmData[i] = Convert.ToInt32(reader.GetString(i + 3));
                }
                s_tblHarmData.Add(campData);
            }
        }

        public static AiHarmData GetHarmData(int id)
        {
            return s_tblHarmData.Find(ret => ret.m_harmID == id);
        }

		/// <summary>
		/// Gets the harm value.
		/// 多人玩家阵营均大于MaxHarmIndex
		/// 多人玩家对非同阵营对象均可以造成伤害
		/// </summary>
		/// <returns>The harm value.</returns>
		/// <param name="srcHarmID">Source harm I.</param>
		/// <param name="dstHarmID">Dst harm I.</param>
        public static int GetHarmValue(int srcHarmID, int dstHarmID)
        {
			if (srcHarmID <= -1 || dstHarmID <= -1)
				return 0;

			if (srcHarmID >= MaxHarmIndex && dstHarmID >= MaxHarmIndex)
				return srcHarmID == dstHarmID ? 0 : 1;

			int src = srcHarmID >= MaxHarmIndex ? AiHatredData.PlayerCamp : srcHarmID;
			int dst = dstHarmID >= MaxHarmIndex ? AiHatredData.PlayerCamp : dstHarmID;

			AiHarmData data = GetHarmData(src);
			if (data == null || dst >= data.m_harmData.Length)
				return 0;
			
			return data.m_harmData[dst];
        }
    }

    public class AiDamageTypeData
    {
        public int m_damageTypeId;
        public float[] m_damageData;
        public static int DamageTypeCount = 0;

        public static List<AiDamageTypeData> s_tblDamageData;

        public static void LoadData()
        {
            s_tblDamageData = new List<AiDamageTypeData>();
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("adtype");
            DamageTypeCount = reader.FieldCount - 1;

            //reader.Read();

            while (reader.Read())
            {
                AiDamageTypeData damageData = new AiDamageTypeData();
                damageData.m_damageData = new float[DamageTypeCount];
                damageData.m_damageTypeId = Convert.ToInt32(reader.GetString(0));
                damageData.m_damageData[0] = 1.0f;

                for (int i = 1; i < DamageTypeCount; i++)
                {
                    damageData.m_damageData[i] = Convert.ToSingle(reader.GetString(i + 1));
                }

                s_tblDamageData.Add(damageData);
            }
        }

        public static AiDamageTypeData GetDamageData(int damageId)
        {
            return s_tblDamageData.Find(ret => ret.m_damageTypeId == damageId);
        }

        public static float GetDamageScale(int damageType, int defenceType)
        {
            AiDamageTypeData data = s_tblDamageData.Find(ret => ret.m_damageTypeId == damageType);
            if (data == null) return 1.0f;

            if (defenceType < 0 || defenceType >= DamageTypeCount) return 1.0f;

            return data.m_damageData[defenceType];
        }

        public static int GetDamageType(SkillAsset.SkillRunner caster)
        {
            if (caster == null)
                return 0;

            Projectile p = caster as Projectile;
            if (p != null && p.emitRunner != null)
            {
                caster = p.emitRunner;
            }

            //AiDataObject ado = caster as AiDataObject;

            //return ado != null ? ado.dataBlock.attackType : 0;
            return 0;
        }
    }

    /* *静态变量加载 */
    public class AiGlobal
    {
        public static float PatrolTimeMin = 5;
        public static float PatrolTimeMax = 7;
        public static float AISpawnAreaDist = 0;
        public static float AISpawnGroupDist = 0;
        public static int FrameTimeOut = 0;
        public static int SatiationLimit = 0;
        public static int DrinkingLimit = 0;
        public static int FatigueLimit = 0;
        public static int SatiationFallMin = 0;
        public static int SatiationFallMax = 0;
        public static int DrinkingFallMin = 0;
        public static int DrinkingFallMax = 0;
        public static int FatigueFallMin = 0;
        public static int FatigueFallMax = 0;
        public static float FeedPercentMin = 0.0f;
        public static float FeedPercentMax = 0.0f;
        public static float DrinkPercent = 0.0f;
        public static float SleepPercent = 0.0f;

        public static void LoadData()
        {

            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("ai_global");

            while (reader.Read())
            {
                AiGlobal.AISpawnAreaDist = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_groupDist")));
                AiGlobal.AISpawnGroupDist = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_areaDist")));
                AiGlobal.FrameTimeOut = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_propertyDecInt")));
                AiGlobal.SatiationLimit = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_satiationLimit")));
                AiGlobal.DrinkingLimit = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_drinkingLimit")));
                AiGlobal.FatigueLimit = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_fatigueLimit")));
                AiGlobal.SatiationFallMin = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_satiationDecMin")));
                AiGlobal.SatiationFallMax = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_satiationDecMax")));
                AiGlobal.DrinkingFallMin = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_drinkingDecMin")));
                AiGlobal.DrinkingFallMax = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_drinkingDecMax")));
                AiGlobal.FatigueFallMin = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_fatigueDecMin")));
                AiGlobal.FatigueFallMax = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_fatigueDecMax")));
                AiGlobal.FeedPercentMin = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_feedingCriMin")));
                AiGlobal.FeedPercentMax = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_feedingCriMax")));
                AiGlobal.SleepPercent = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_sleepCritical")));

            }
        }
    }
}