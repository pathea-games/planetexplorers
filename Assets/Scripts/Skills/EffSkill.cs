#define TMP_CODE // durCost code

using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ItemAsset;
using AiAsset;
using NaturalResAsset;
using SkillSystem;
using Pathea;

namespace SkillAsset
{

    public enum Buff_Sp
    {
        BUFFSP_NONE,
        MOVE_NOT,
        ATTACK_NOT,
        ATTACKED_NOT,
        REBOUND,
        HATRED_ADD,
        STUNNED,
        STRUKEDOWN,
        INVENSIBLE,
		BLOW_FLY,
		RESURRECTION_FULLBLOOD,
		BLIND,
        BUFF_MAX
    }

    // Properties 
    public class EffSkillBuff
    {

        public static object elem(int id, string colname)
        {
            EffSkillBuff buff = s_tblEffSkillBuffs.Find(iterSkill0 => EffSkillBuff.MatchId(iterSkill0, id));
            if (buff != null)
            {
                if (colname.CompareTo("_timeCoolingChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_spdChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_typeSpd") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_type3Time") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_type4Time") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_damage") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_atkChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_defChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_block") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_jumpHeight") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_fallInjuries") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_hpMaxChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_satiationMaxChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_comfortMaxChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_sitiationDecSpdChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_comfortDecSpdChange") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_extralPoint") == 0)
                {
                    return buff.m_timeCoolingChange;
                }
                else if (colname.CompareTo("_extralResource") == 0)
                {
                    return buff.m_timeCoolingChange;
                }

            }
            return null;
        }
		
        public int m_id;
        public string m_buffName;
        public string m_iconImgPath;
		public string m_buffHint;
        public int m_buffType;
		public int m_StackLimit;
		public int m_Priority;
        public float m_timeActive;
        public float m_timeInterval;
        public float m_hpChange;
        public short m_buffSp;
        public float m_timeCoolingChange;
//        public List<short> m_typeTimeCoolingChange = new List<short>();
        public float m_spdChange;
//        public float m_durAtkChange;    // Here some special code for durAtk(35% for insuitable action, 100% for right action and attack)
        public float m_atkChange;
		public float m_atkChangeP;
        public float m_atkDistChange;
        public float m_defChange;
        public float m_defChangeP;
        public float m_block;
        public float m_jumpHeight;
        public float m_fallInjuries;
        public float m_hpMaxChange;
        public float m_hpMaxChangeP;
        public float m_satiationMaxChange;
        public float m_comfortMaxChange;
        public float m_satiationDecSpdChange;
        public float m_comfortDecSpdChange;
        public float m_collectLv;
		
		
        public float m_satiationChange;
        public float m_comfortChange;
        public short m_resGotMultiplier;
		public float m_resGotRadius;
        public int m_changeCamp;
		
		//static int mMaxBuffId = 0;

        public IEnumerator Exec(SkillRunner caster, SkillRunner buffHost, EffSkillBuffInst buffInst)   // for normal buff
        {
            if(!buffHost.m_effSkillBuffManager.Add(buffInst))
				yield break;

            int times = 1;
            float timeInterval = m_timeActive;
            if (timeInterval > -PETools.PEMath.Epsilon)
            {
                if (m_timeInterval > PETools.PEMath.Epsilon)
                {
                    times = (int)(m_timeActive / m_timeInterval);
                    timeInterval = m_timeInterval;
                }
            }
            else // for -1
            {
                times = 0;
                while (true)
                {
                    yield return 0;
                }
            }

			for (int i = 0; i < times; i++)
			{
				if (!GameConfig.IsMultiMode)
				{
					buffHost.ApplyHpChange(null, m_hpChange, 0, 0);
					buffHost.ApplyComfortChange(m_comfortChange);
					buffHost.ApplySatiationChange(m_satiationChange);
					buffHost.ApplyBuffContinuous(caster, m_buffSp);
				}

				yield return new WaitForSeconds(timeInterval);
			}

            buffHost.m_effSkillBuffManager.Remove(buffInst);
        }


        public const int MIN_CD_TYPE = 1;
        public const int NUM_CD_TYPE = 12;
        public const int MAX_CD_TYPE = MIN_CD_TYPE + NUM_CD_TYPE - 1;
		public const int CustomIdStart = 100000000;
        public static List<string> s_tblEffSkillBuffsColName_CN, s_tblEffSkillBuffsColName_EN;
        public static List<EffSkillBuff> s_tblEffSkillBuffs;
        public static void LoadData()
        {
			//mMaxBuffId = 0;
			
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("buff");
            int nFieldCount = reader.FieldCount;

            s_tblEffSkillBuffsColName_CN = new List<string>(nFieldCount);
            reader.Read();
            for (int i = 0; i < nFieldCount; i++)
                s_tblEffSkillBuffsColName_CN.Add(reader.GetString(i));

            s_tblEffSkillBuffsColName_EN = new List<string>(nFieldCount);
            reader.Read();
            for (int i = 0; i < nFieldCount; i++)
                s_tblEffSkillBuffsColName_EN.Add(reader.GetString(i));

            s_tblEffSkillBuffs = new List<EffSkillBuff>();
            while (reader.Read())
            {
                EffSkillBuff buff = new EffSkillBuff();
                buff.m_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_buffid")));
                buff.m_buffName = reader.GetString(reader.GetOrdinal("_buffname"));
                buff.m_iconImgPath = reader.GetString(reader.GetOrdinal("_bufficon"));
				buff.m_buffHint = PELocalization.GetString(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_buffhint"))));
                buff.m_buffType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_type")));
				buff.m_StackLimit = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_buffStackLimit")));
				buff.m_Priority = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_priority")));
                buff.m_changeCamp = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_changeCamp")));
                buff.m_timeActive = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeActive")));
                buff.m_timeInterval = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeInterval")));
                buff.m_hpChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_hpChange")));
                //                buff.m_comfortChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_comfortChange")));
                //                buff.m_satiationChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_satiationChange")));
                ReadBuffSp(reader, buff);
                buff.m_timeCoolingChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeCoolingChange")));////////////////////////////
//                ReadCDChangeType(reader, buff);
                buff.m_spdChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_spdChange")));////////////////////////
//                buff.m_durAtkChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_damage")));////////////////////////
                buff.m_atkChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_atkChange")));///////////////////////////////
				buff.m_atkChangeP = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_atkChangeP")));
                buff.m_atkDistChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_atkDistChange")));
                buff.m_defChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_defChange")));////////////////////////
				buff.m_defChangeP = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_defChangeP")));////////////////////////
                buff.m_block = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_block")));///////////////////////////
                buff.m_jumpHeight = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_jumpHeight")));////////////////////////
                buff.m_fallInjuries = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_fallInjuries")));//////////////////
                buff.m_hpMaxChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_hpMaxChange")));/////////////////////////////
				buff.m_hpMaxChangeP = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_hpMaxChangeP")));/////////////////////////////
                buff.m_satiationMaxChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_satiationMaxChange")));//////////////////////////
                buff.m_comfortMaxChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_comfortMaxChange")));//////////////////////////////////////
                buff.m_satiationDecSpdChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_sitiationDecSpdChange")));////////////////////////////
                buff.m_comfortDecSpdChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_comfortDecSpdChange")));////////////////////////////////
                buff.m_collectLv = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_collectLv")));////////////////////////
                buff.m_resGotMultiplier = Convert.ToInt16(reader.GetString(reader.GetOrdinal("_resoucesGot")));
                buff.m_resGotRadius = Convert.ToInt16(reader.GetString(reader.GetOrdinal("_resGotRadius")));
                s_tblEffSkillBuffs.Add(buff);
            }
        }
		
		public static void AddNewBuff(EffSkillBuff buff)
		{
			int oldIndex = s_tblEffSkillBuffs.FindIndex(itr => MatchId(itr, buff.m_id));
			if(-1 == oldIndex)
				s_tblEffSkillBuffs.Add(buff);
			else
				s_tblEffSkillBuffs[oldIndex] = buff;
		}
		
		public static void ClearCustomBuff()
		{
			for(int i = s_tblEffSkillBuffs.Count - 1; i >= 0; i--)
			{
				if(s_tblEffSkillBuffs[i].m_id >= CustomIdStart)
					s_tblEffSkillBuffs.RemoveAt(i);
				else
					break;
			}
		}

		public static void RemoveBuff(EffSkillBuff buff)
		{
			if(s_tblEffSkillBuffs.Contains(buff))
				s_tblEffSkillBuffs.Remove(buff);
		}

        public static bool MatchId(EffSkillBuff iter, int id)
        {
            return iter.m_id == id;
        }
//        private static void ReadCDChangeType(SqliteDataReader reader, EffSkillBuff buff)
//        {
//            string desc = reader.GetString(reader.GetOrdinal("_typeTimeCoolingChange"));
//            string[] strings = desc.Split(',');
//            for (int i = 0; i < strings.Length; i++)
//            {
//                int idx = Convert.ToInt32(strings[i]);
//                if (idx < MIN_CD_TYPE || idx > MAX_CD_TYPE)
//                    return;
//                buff.m_typeTimeCoolingChange.Add((short)(idx - MIN_CD_TYPE));
//            }
//        }
        private static void ReadBuffSp(SqliteDataReader reader, EffSkillBuff buff)
        {
            string desc = reader.GetString(reader.GetOrdinal("_buffSp"));
            string[] strings = desc.Split(',');
            buff.m_buffSp = 0;
            foreach (string s in strings)
            {
                int bit = Convert.ToInt32(s);
                if (bit > 0)
                {
                    buff.m_buffSp |= (short)(1 << (bit - 1));
                }
            }
        }
    }
    public class EffSkillBuffInst
    {
        public EffSkillBuff m_buff;
        public CoroutineStoppable m_runner;
        public int m_skillId;
        public static EffSkillBuffInst TakeEffect(SkillRunner caster, SkillRunner buffHost, EffSkillBuff buff, int parentSkillId)
        {
            EffSkillBuffInst buffInst = new EffSkillBuffInst();
            buffInst.m_buff = buff;
            buffInst.m_skillId = buff.m_timeActive > PETools.PEMath.Epsilon ? 0 : parentSkillId;    // parent skill Id, Now only needed for equipements
            buffInst.m_runner = new CoroutineStoppable(buffHost, buff.Exec(caster, buffHost, buffInst));
            return buffInst;
        }
    }
    public class EffSkillBuffManager
    {
        internal List<EffSkillBuffInst> m_effBuffInstList = new List<EffSkillBuffInst>();
		
		Dictionary<int, List<EffSkillBuffInst>> mTypeManagedMap = new Dictionary<int, List<EffSkillBuffInst>>();
		
        internal bool m_bEffBuffDirty = true;
        public bool Add(EffSkillBuffInst buffInst)  // add in buff exec
        {			
			if(!mTypeManagedMap.ContainsKey(buffInst.m_buff.m_buffType))
				mTypeManagedMap[buffInst.m_buff.m_buffType] = new List<EffSkillBuffInst>();
			
			if(mTypeManagedMap[buffInst.m_buff.m_buffType].Count > 0)
			{
				if(mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_buff.m_id == buffInst.m_buff.m_id)
				{
					if(mTypeManagedMap[buffInst.m_buff.m_buffType].Count == buffInst.m_buff.m_StackLimit)
					{
						m_effBuffInstList.Remove(mTypeManagedMap[buffInst.m_buff.m_buffType][0]);
						if(null != mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_runner)
							mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_runner.stop = true;
						mTypeManagedMap[buffInst.m_buff.m_buffType].RemoveAt(0);
					}
				}
				else
				{
					if(mTypeManagedMap[buffInst.m_buff.m_buffType][0].m_buff.m_Priority <= buffInst.m_buff.m_Priority)
					{
						foreach(EffSkillBuffInst removeBuff in mTypeManagedMap[buffInst.m_buff.m_buffType])
						{
							if(null != removeBuff.m_runner)
								removeBuff.m_runner.stop = true;
							m_effBuffInstList.Remove(removeBuff);
						}
						mTypeManagedMap[buffInst.m_buff.m_buffType].Clear();
					}
					else
						return false;
				}
			}
			
			mTypeManagedMap[buffInst.m_buff.m_buffType].Add(buffInst);
            m_effBuffInstList.Add(buffInst);
            m_bEffBuffDirty = true;
			return true;
        }
		
        public void Remove(EffSkillBuffInst buffInst)    // remove in buff exec
        {
			if(null != buffInst.m_runner)
				buffInst.m_runner.stop = true;
			if(mTypeManagedMap.ContainsKey(buffInst.m_buff.m_buffType))
			{
				mTypeManagedMap[buffInst.m_buff.m_buffType].Remove(buffInst);
	            m_effBuffInstList.Remove(buffInst);
	            m_bEffBuffDirty = true;
			}
        }
		
        public void Remove(int skillId)    // remove in request 
        {
			if(skillId == 0)
				return;
            for (int i = 0; i < m_effBuffInstList.Count; i++)
            {
                if (m_effBuffInstList[i].m_skillId == skillId)
                {
					int buffType = m_effBuffInstList[i].m_buff.m_buffType;
					if(mTypeManagedMap.ContainsKey(buffType))
					{
						foreach(EffSkillBuffInst removeBuff in mTypeManagedMap[buffType])
						{
							if(null != removeBuff.m_runner)
								removeBuff.m_runner.stop = true;
							m_effBuffInstList.Remove(removeBuff);
						}
						mTypeManagedMap[buffType].Clear();
	                    m_bEffBuffDirty = true;
					}
                    break;
                }
            }
        }
		
		public void AddBuff(EffSkillBuff addBuff)
		{
			EffSkillBuffInst addBuffIns = new EffSkillBuffInst();
			addBuffIns.m_buff = addBuff;
			
			Add(addBuffIns);
			
//			m_effBuffInstList.Add(addBuffIns);
//			
//			if(!mTypeManagedMap.ContainsKey(addBuff.m_buffType))
//				mTypeManagedMap[addBuff.m_buffType] = new List<EffSkillBuffInst>();
//			mTypeManagedMap[addBuff.m_buffType].Add(addBuffIns);
		}
		
		public void RemoveBuff(EffSkillBuff removeBuff)
		{
			int buffType = removeBuff.m_buffType;
			if(mTypeManagedMap.ContainsKey(buffType))
			{
				foreach(EffSkillBuffInst remBuff in mTypeManagedMap[buffType])
				{
					if(null != remBuff.m_runner)
						remBuff.m_runner.stop = true;
					m_effBuffInstList.Remove(remBuff);
				}
				mTypeManagedMap[buffType].Clear();
	            m_bEffBuffDirty = true;
			}
		}
		
		public EffSkillBuff GetBuff(int ID)
		{
			EffSkillBuff retBuff = null;
			foreach(EffSkillBuffInst buff in m_effBuffInstList)
			{
				if(buff.m_buff.m_id == ID)
				{
					retBuff = buff.m_buff;
					break;
				}
			}
			return retBuff;
		}

        public bool IsAppendBuff(int buffId)    // remove in request 
        {
            for (int i = 0; i < m_effBuffInstList.Count; i++)
            {
                if (m_effBuffInstList[i].m_buff.m_id == buffId)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class EffScope
    {
        internal int m_centerType;
        internal float m_radius;
        internal float m_degStart;
        internal float m_degEnd;
        internal static List<EffScope> Create(SqliteDataReader reader)
        {
            string desc = reader.GetString(reader.GetOrdinal("_scope"));
            string[] strings = desc.Split(',');
            if (strings.Length < 4)
            {
                return null;
            }

            List<EffScope> ret = new List<EffScope>();
            for (int i = 3; i < strings.Length; i += 4)
            {
                EffScope scope = new EffScope();
                scope.m_centerType = Convert.ToInt32(strings[i - 3]);
                scope.m_radius = Convert.ToSingle(strings[i - 2]);
                scope.m_degStart = Convert.ToSingle(strings[i - 1]);
                scope.m_degEnd = Convert.ToSingle(strings[i]);
                ret.Add(scope);
            }
            return ret;
        }
    }
	
	public class GroundScope
	{
		internal int 	mType; //0: nothing 1: block only 2: block and terrain
		internal float 	mRadius;
		internal Vector3 mCenter;
		
        internal static GroundScope Create(SqliteDataReader reader)
        {
            string desc = reader.GetString(reader.GetOrdinal("_rScope"));
            string[] strings = desc.Split(',');
            GroundScope ret = new GroundScope();
            ret.mType = Convert.ToInt32(strings[0]);
            ret.mRadius = Convert.ToSingle(strings[1]);
            ret.mCenter = new Vector3(Convert.ToSingle(strings[2]), Convert.ToSingle(strings[3]), Convert.ToSingle(strings[4]));
			if(ret.mType == 0)
				return null;
            return ret;
        }
	}
		
    public class EffCoolDownInfo
    {
        internal float m_timeCost;
        internal short m_type;
        internal float m_timeShared;
        internal static EffCoolDownInfo Create(SqliteDataReader reader)
        {
            EffCoolDownInfo ret = new EffCoolDownInfo();
            ret.m_timeCost = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeCooling")));
            ret.m_type = Convert.ToInt16(reader.GetString(reader.GetOrdinal("_typeTimeCooling")));
            ret.m_timeShared = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_shareTimeCooling")));
            return ret;
        }
    }
    public class EffPrepareInfo
    {
        internal float m_timeCost;
        internal List<int> m_effIdList;
        internal List<string> m_animNameList;
        internal List<int> m_ReadySound;
        internal float m_DelayR;
        internal static EffPrepareInfo Create(SqliteDataReader reader)
        {
            float timeCost = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timePrep")));
            List<int> effIdList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("precast_eff")));
            List<string> animNameList = EffSkill.ToListString(reader.GetString(reader.GetOrdinal("_prepareAction")));

            List<int> readySound = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("_readySound")));
            float delayR = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_delayR")));

            if (timeCost < PETools.PEMath.Epsilon && effIdList == null && animNameList == null)
            {
                return null;
            }

            EffPrepareInfo ret = new EffPrepareInfo();
            ret.m_timeCost = timeCost;
            ret.m_effIdList = effIdList;
            ret.m_animNameList = animNameList;
            ret.m_ReadySound = readySound;
            ret.m_DelayR = delayR;
            return ret;
        }
        internal void Prepare(SkillRunner caster, ISkillTarget target)
        {
            // play effects and anims
            //             if (!GameConfig.IsServer || GameConfig.IsMultiMode() && uLink.Network.isClient)
            //             {
            if (m_animNameList != null/* && caster as AiTower*/)
            {
                caster.ApplyAnim(m_animNameList);
            }
            //  }


            if (m_effIdList != null /*&& caster as AiTower*/)
            {
                caster.ApplyEffect(m_effIdList, target);
            }
        }

        //internal void PrepareHandle(SkillRunner caster)
        //{
        //    if (m_effIdList != null)
        //    {
        //        caster.ApplyEffect(m_effIdList);
        //    }
        //}
    }
    public class EffGuidanceInfo
    {
		internal int m_damageType;
        internal float m_timeCost;
        internal float m_timeInterval;
        internal float m_hpChangeOnce;
        internal float m_hpChangePercent;
        internal float m_ComfortChange;
        internal float m_OxygenChange;
        internal float m_durChangeOnce;
        internal float m_sitiationChangeOnce;
        internal float m_thirstLvChangeOnce;
        internal float m_distRepelOnce;
		internal List<EffSkillBuff> m_buffList;
		internal List<int> m_buffIDList;
        internal List<int> m_effIdList;
        internal List<string> m_animNameList;
        internal List<int> m_TargetEffIDList;
        internal List<int> m_GuidanceSound;
		
		internal Dictionary<int, float> mPropertyChange;

        internal float m_DelayTime;
        internal float m_SoundDelayTime;
		
		internal GroundScope m_GroundScope;
        //
        internal EffGuidanceInfo()
        {
			m_buffIDList = new List<int>();
			m_buffList = new List<EffSkillBuff>();
			m_effIdList = new List<int>();
			m_animNameList = new List<string>();
			m_TargetEffIDList = new List<int>();
			m_GuidanceSound = new List<int>();
			mPropertyChange = new Dictionary<int, float>();
        }

        //
        public EffGuidanceInfo(int buffID)
        {
            m_timeCost = 0;
            m_timeInterval = 0;
            m_hpChangeOnce = 0;
            m_hpChangePercent = 0;
            m_durChangeOnce = 0;
            m_sitiationChangeOnce = 0;
            m_thirstLvChangeOnce = 0;
            m_distRepelOnce = 0;
            m_effIdList = null;
            m_TargetEffIDList = null;

            EffSkillBuff buff = EffSkillBuff.s_tblEffSkillBuffs.Find(iter0 => EffSkillBuff.MatchId(iter0, buffID));
            if (buff != null)
                m_buffList.Add(buff);
        }

        internal static EffGuidanceInfo Create(SqliteDataReader reader)
        {
            EffGuidanceInfo ret = new EffGuidanceInfo();
			ret.m_damageType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_attacktype")));
            ret.m_timeCost = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_guidanceTime")));
            ret.m_timeInterval = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_timeInterval")));
            ret.m_hpChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_harmBase")));
            ret.m_hpChangePercent = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_harmPercent")));
            ret.m_ComfortChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_sitiationReply"))); ;
            ret.m_OxygenChange = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_waterReply")));
            ret.m_durChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_damage")));
            ret.m_sitiationChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_sitiationReply")));
            ret.m_thirstLvChangeOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_waterReply")));
            ret.m_distRepelOnce = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_distRepel")));
			ret.m_buffIDList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("_addBuff")));
            ret.m_effIdList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("casting_eff")));
            ret.m_TargetEffIDList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("target_eff")));
            ret.m_GuidanceSound = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("_guidanceSound")));
            ret.m_DelayTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_guidanceDelay")));
            ret.m_SoundDelayTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_delayG")));
			ret.m_GroundScope = GroundScope.Create(reader);

			string proStr = reader.GetString(reader.GetOrdinal("_prChange1"));
			if("0" != proStr)
			{
				string[] strings = reader.GetString(reader.GetOrdinal("_prChange1")).Split(';');
				for(int i = 0; i < strings.Length; i++)
				{
					string[] values = strings[i].Split(',');
					ret.mPropertyChange[Convert.ToInt32(values[0])] = Convert.ToSingle(values[1]);
				}
			}
			if (ret.m_buffIDList != null)
            {
				for (int i = 0; i < ret.m_buffIDList.Count; i++)
                {
					EffSkillBuff buff = EffSkillBuff.s_tblEffSkillBuffs.Find(iter0 => EffSkillBuff.MatchId(iter0, ret.m_buffIDList[i]));
                    if (buff != null)
                        ret.m_buffList.Add(buff);
                }
            }
            //            ret.m_effIdList = EffSkill.ToListInt32P(reader.GetString(reader.GetOrdinal("_guidanceEffect")));
            ret.m_animNameList = EffSkill.ToListString(reader.GetString(reader.GetOrdinal("_guidanceAction")));
            return ret;
        }

		internal bool TakeEffectProxy(SkillRunner caster, List<ISkillTarget> targetList, int skillId, bool bAffectCaster)
		{
			if (m_TargetEffIDList != null)
			{
				foreach (int id in m_TargetEffIDList)
				{
					foreach (ISkillTarget target in targetList)
						EffectManager.Instance.Instantiate(id, target.GetPosition(), Quaternion.identity, null);
				}
			}

			bool finish = false;

			if (bAffectCaster) targetList.Add(caster);
			SkillRunner runnerTarget;
			for (int i = 0; i < targetList.Count; i++)
			{
				if ((runnerTarget = targetList[i] as SkillRunner) != null)   // _harm,_sitiationReply,_waterReply,_addBuff,_distRepel
				{
					if (m_buffList != null)
					{
						for (int j = 0; j < m_buffList.Count; j++)
						{
							if (m_buffList[j].m_timeActive < -2.0f + PETools.PEMath.Epsilon)
							{
								//runnerTarget.ApplyBuffPermanent(m_buffList[j]);
							}
							else if (m_buffList[j].m_timeActive < -1.0f + PETools.PEMath.Epsilon)
							{
								EffSkillBuffInst.TakeEffect(caster, runnerTarget, m_buffList[j], skillId);
							}
							else
							{
								EffSkillBuffInst.TakeEffect(caster, runnerTarget, m_buffList[j], skillId);
							}
						}
						for(int j = 0; j < m_buffIDList.Count; j++)
						{
							runnerTarget.BuffAttribs += m_buffIDList[j];
						}
					}
				}
			}

			if (bAffectCaster) targetList.Remove(caster);
			return finish;
		}
		
        // client 
        internal bool TakeEffect(SkillRunner caster, List<ISkillTarget> targetList, int skillId)
        {
            return true;
            // -- if target is a skillrunner, then skill affect target, else skill affect caster
            // play effects and anims

//            if(m_TargetEffIDList != null)
//            {
//                foreach(int id in m_TargetEffIDList)
//                {
//                    foreach(ISkillTarget target in targetList)
//                        EffectManager.Instance.Instantiate(id, target.GetPosition(),Quaternion.identity, null);
//                }
//            }

//            bool finish = false;

//            SkillRunner runnerTarget;
//            INaturalResTarget resTarget;
//            for (int i = 0; i < targetList.Count; i++)
//            {
//                if ((resTarget = targetList[i] as INaturalResTarget) != null)
//                {
//                    if (m_durChangeOnce > PETools.PEMath.Epsilon || m_durChangeOnce < -PETools.PEMath.Epsilon)   // _damage != 0
//                    {
//                        float durAtk = m_durChangeOnce;
//                        if (durAtk < -PETools.PEMath.Epsilon)
//                        {	// -1 means to get durAtk from player
//                            durAtk = caster.GetAttribute(AttribType.ResDamage);
//                            durAtk *= AiDamageTypeData.GetDamageScale((m_damageType == 0) ? AiDamageTypeData.GetDamageType(caster) : m_damageType, 10);
//                        }
						
//                        short itemBonus = (short)caster.GetAttribute(AttribType.ResBouns);
//                        float radius = caster.GetAttribute(AttribType.ResBouns);
						
//                        if (radius < 0.1f)
//                            radius = 0.1f;

//                        // 网络版 count%100 == 0 不会得到物品
//                        // 服务器端得到物品
//                        int count = resTarget.GetDestroyed(caster, durAtk, radius);
						
//                        //Player selfPlayer = caster as Player;
//                        //if(selfPlayer != PlayerFactory.mMainPlayer)
//                        //    selfPlayer = null;

//                        if(!GameConfig.IsMultiMode && null != selfPlayer && resTarget.GetTargetType() != ESkillTargetType.TYPE_Herb)
//                        {
//                            IHuman human = caster as IHuman;
//                            if(null != human)
//                                human.ApplyDurabilityReduce(0);
//                        }
						
//                        finish = (count / 100 == 1) ? true : false;
//                        count = count % 100;
//                        if (0 != count)
//                        {
//                            ItemPackage pack = caster.GetItemPackage();
//                            if (pack != null)
//                            {
//                                List<ItemSample> itemGridList = resTarget. ReturnItems((short)itemBonus, count);
//                                List<int> itemIDList = new List<int>();
//                                List<int> itemCountList = new List<int>();

//                                if (itemGridList == null)
//                                    continue;

//                                for (int k = 0; k < itemGridList.Count; k++)
//                                {
//                                    itemIDList.Add(itemGridList[k].protoId);
//                                    itemCountList.Add(itemGridList[k].GetCount());
//                                }

//                                if (null != itemGridList && itemGridList.Count > 0)
//                                {
//                                    for (int k = 0; k < itemGridList.Count; k++)
//                                        GlobalShowGui_N.Instance.AddShow(itemGridList[k]);
									
//                                    if(null != selfPlayer)
//                                    {
//                                        for (int k = 0; k < itemGridList.Count; k++)
//                                            PlayerFactory.mMainPlayer.AddItem(itemGridList[k]);
//                                        GameUI.Instance.mUIItemPackageCtrl.ResetItem();
//                                        MainMidGui_N.Instance.UpdateLink();
//                                    }
//                                    //else
//                                    //    pack.AddItemList(itemGridList);
//                                }

//                            }
//                        }
//                    }
//                }
//                else if ((runnerTarget = targetList[i] as SkillRunner) != null)   // _harm,_sitiationReply,_waterReply,_addBuff,_distRepel
//                {
//                    if (!GameConfig.IsMultiMode)
//                    {
//                        if ((m_hpChangeOnce > PETools.PEMath.Epsilon || m_hpChangeOnce < -PETools.PEMath.Epsilon) ||
//                            (m_hpChangePercent > PETools.PEMath.Epsilon || m_hpChangePercent < -PETools.PEMath.Epsilon))
//                            runnerTarget.ApplyHpChange(caster, m_hpChangeOnce, m_hpChangePercent, m_damageType);

//                        if (Math.Abs(m_ComfortChange) > PETools.PEMath.Epsilon)
//                            runnerTarget.ApplyComfortChange((int)m_ComfortChange);

//                        if (Math.Abs(m_OxygenChange) > PETools.PEMath.Epsilon)
//                            runnerTarget.ApplySatiationChange((int)m_OxygenChange);

//                        if (m_durChangeOnce > PETools.PEMath.Epsilon || m_durChangeOnce < -PETools.PEMath.Epsilon)
//                            runnerTarget.ApplyDurChange(caster, m_durChangeOnce, m_damageType);

//                        if (m_distRepelOnce > PETools.PEMath.Epsilon || m_distRepelOnce < -PETools.PEMath.Epsilon)
//                            runnerTarget.ApplyDistRepel(caster, m_distRepelOnce);

////						if (m_sitiationChangeOnce > PETools.PEMath.Epsilon || m_sitiationChangeOnce < -PETools.PEMath.Epsilon)
////							runnerTarget.ApplySatiationChange(m_sitiationChangeOnce);

//                        if (m_thirstLvChangeOnce > PETools.PEMath.Epsilon || m_thirstLvChangeOnce < -PETools.PEMath.Epsilon)
//                            runnerTarget.ApplyThirstLvChange(m_thirstLvChangeOnce);

//                    }
//                    else
//                    {
//                        if (null != caster && null != caster.OwnerView)
//                        {
//                            runnerTarget.RPCServer(EPacketType.PT_InGame_SkillTakeEffect, skillId, caster.OwnerView.viewID);
//                        }
//                        else
//                        {
//                            runnerTarget.RPCServer(EPacketType.PT_InGame_SkillTakeEffect, skillId, uLink.NetworkViewID.unassigned);
//                        }
//                    }

//                    if(null != runnerTarget.mLifeFormController)
//                        runnerTarget.mLifeFormController.ApplyPropertyChange(mPropertyChange);

//                    // 客户端只用做显示。以服务器为准
//                    if (m_buffList != null)
//                    {
//                        for (int j = 0; j < m_buffList.Count; j++)
//                        {
//                            if (m_buffList[j].m_timeActive < -2.0f + PETools.PEMath.Epsilon)
//                            {
////								runnerTarget.ApplyBuffPermanent(m_buffList[j]);
//                            }
//                            else if (m_buffList[j].m_timeActive < -1.0f + PETools.PEMath.Epsilon)
//                            {
//                                EffSkillBuffInst.TakeEffect(caster, runnerTarget, m_buffList[j], skillId);
//                            }
//                            else
//                            {
//                                EffSkillBuffInst.TakeEffect(caster, runnerTarget, m_buffList[j], skillId);
//                            }
//                        }
						
////						for(int j = 0; j < m_buffIDList.Count; j++)
////						{
////							if(null != runnerTarget.BuffAttribs)
////								runnerTarget.BuffAttribs += m_buffIDList[j];
////						}
//                    }
//                }
//            }
//            if(null != m_GroundScope)
//                DestoryTerrain(caster);
//            return finish;
        }
		
		void DestoryTerrain(SkillRunner caster)
		{
			float durAtk = m_durChangeOnce;
			if (durAtk < -PETools.PEMath.Epsilon)
			{	// -1 means to get durAtk from player
				durAtk = caster.GetAttribute(AttribType.Atk) * m_hpChangePercent;
				durAtk *= AiDamageTypeData.GetDamageScale((m_damageType == 0) ? AiDamageTypeData.GetDamageType(caster) : m_damageType, 10);
			}

            //if (GameConfig.IsMultiMode)
            //{
            //    if (caster is AiMonster)
            //    {
            //        DigTerrainManager.DestroyTerrainInRangeNetwork(m_GroundScope.mType, caster
            //            , caster.transform.position + caster.transform.rotation * (m_GroundScope.mCenter * (caster as AiMonster).SizeScale)
            //            , durAtk, m_GroundScope.mRadius * (caster as AiMonster).SizeScale);
            //    }
            //    else
            //    {
            //        DigTerrainManager.DestroyTerrainInRangeNetwork(m_GroundScope.mType, caster
            //            , caster.transform.position + caster.transform.rotation * m_GroundScope.mCenter
            //            , durAtk, m_GroundScope.mRadius);
            //    }
					
            //}
            //else
            //{
            //    if (caster is AiMonster)
            //    {
            //        DigTerrainManager.self.DestroyTerrainInRange(m_GroundScope.mType
            //            , caster.transform.position + caster.transform.rotation * (m_GroundScope.mCenter * (caster as AiMonster).SizeScale)
            //            , durAtk, m_GroundScope.mRadius);
            //    }
            //    else
            //        DigTerrainManager.self.DestroyTerrainInRange(m_GroundScope.mType
            //            , caster.transform.position + caster.transform.rotation * m_GroundScope.mCenter
            //            , durAtk, m_GroundScope.mRadius);
            //}
		}
    }

    public class EffItemCast
    {
        internal int m_itemId;
        internal int m_skillId;
        internal int m_effId;
        internal int m_impactEffId;
        internal int m_trajectoryId;
        internal float m_speed;
        internal string m_castPosName;

        internal float m_dampRange;
        internal float m_dampValueMin;
		
		//The id of the skill for cast this item
		internal int m_castSkillId;
		
        internal static EffItemCast Create(SqliteDataReader reader, int id)
        {
            string desc = reader.GetString(reader.GetOrdinal("_dummy"));
            string[] strings = desc.Split(',');
            if (strings.Length < 9)
            {
                return null;
            }

            EffItemCast ret = new EffItemCast();
			ret.m_castSkillId = id;
            ret.m_itemId = Convert.ToInt32(strings[0]);
            ret.m_skillId = Convert.ToInt32(strings[1]);
            ret.m_effId = Convert.ToInt32(strings[2]);
            ret.m_impactEffId = Convert.ToInt32(strings[3]);
            ret.m_trajectoryId = Convert.ToInt32(strings[4]);
            ret.m_speed = Convert.ToSingle(strings[5]);
            ret.m_dampRange = Convert.ToSingle(strings[6]);
            ret.m_dampValueMin = Convert.ToSingle(strings[7]);
            ret.m_castPosName = strings[8];
            return ret;
        }

        internal void Cast(SkillRunner caster, ISkillTarget target)
        {
            // TODO : code to create a item with skill(m_skillId) and cast it, its trajectory's id is m_trajectoryId

            ItemProto data = ItemProto.Mgr.Instance.Get(m_itemId);
			if ( data == null )
				return;
//            ModeInfo fModeInfo = ModeInfo.s_tblModeInfo.Find(ModeInfo0 => ModeInfo.MatchID(ModeInfo0, data.m_ModelID));
			
			Transform castTrans;
			SkillRunner cast = caster;
			if(m_castPosName.Equals("0"))
				castTrans = caster.GetCastTransform(this);
			else
				castTrans = AiUtil.GetChild(caster.transform, m_castPosName);
			
            if (castTrans == null) return;

			float random1 = UnityEngine.Random.value;
			for(int i=0; i<m_effId; i++){
				//effid: the serial number of projectile,for multiple shooting in one time
            	GameObject itemPrefab = MonoBehaviour.Instantiate(Resources.Load(data.resourcePath), castTrans.position, castTrans.rotation) as GameObject;

            	Projectile pro = itemPrefab.GetComponent<Projectile>();
            	if (pro != null && target != null)
            	{
                    AudioManager.instance.Create(castTrans.position, m_trajectoryId);

					Projectile parentCaster = caster as Projectile;
					if(parentCaster != null)
						cast = parentCaster;
                    if(m_skillId <= 0)
						pro.Init((byte)(i + 1), cast, target, castTrans, random1);
                    else
						pro.Init((byte)(i + 1), cast, target, castTrans, m_skillId, random1);
    	        }
			}
        }
    }
    public class EffItemGot
    {	//This class structure is changed by yinrui
        internal int m_numMax;
        internal int m_numMin;
		internal List<RandGetItem> randList = new List<RandGetItem>();
		internal List<FixGetItem> fixList = new List<FixGetItem>();
		
		internal class RandGetItem{
			internal int m_id;
        	internal float m_probablity;
		}
		
		internal class FixGetItem{
			internal int m_id;
        	internal int m_num;
		}
		
        internal static EffItemGot Create(SqliteDataReader reader)
        {
            string desc = reader.GetString(reader.GetOrdinal("_itemsGot"));
			
            string[] strings = desc.Split(';');
			string[] part1 = strings[0].Split(',');
            
			EffItemGot ret = new EffItemGot();
			
			if (part1.Length >= 4 && part1.Length % 2 == 0)
            {
                ret.m_numMin = Convert.ToInt32(part1[0]);
				ret.m_numMax = Convert.ToInt32(part1[1]);
            	for (int i = 2; i < part1.Length; )
            	{
                	RandGetItem itemGot = new RandGetItem();
               		itemGot.m_id = Convert.ToInt32(part1[i++]);
                	itemGot.m_probablity = Convert.ToSingle(part1[i++]);
                	ret.randList.Add(itemGot);
            	}
            }
			
			if(desc.Contains(";")){
				string[] part2 = strings[1].Split(',');
				if(part2.Length >= 2 && part2.Length %2 == 0)
				{
					for(int i = 0 ; i < part2.Length; )
					{
						FixGetItem itemGot = new FixGetItem();
						itemGot.m_id = Convert.ToInt32(part2[i++]);
						itemGot.m_num = Convert.ToInt32(part2[i++]);
						ret.fixList.Add(itemGot);
					}
				}
			}
            return ret;
        }

        public List<ItemSample> GetItemSample()
        {
            List<ItemSample> listTmp = new List<ItemSample>(10);

            Dictionary<int, int> dicItem = new Dictionary<int, int>(10);

            int getNumber = UnityEngine.Random.Range(m_numMin, m_numMax);
            for (int i = 0; i <= getNumber; i++)
            {
                foreach (RandGetItem j in randList)
                {
                    if (UnityEngine.Random.value < j.m_probablity)
                    {
                        if (dicItem.ContainsKey(j.m_id))
                        {
                            dicItem[j.m_id] += 1;
                        }
                        else
                        {
                            dicItem.Add(j.m_id, 1);
                        }

                        break;
                    }
                }
            }

            foreach (KeyValuePair<int, int> pair in dicItem)
            {
                listTmp.Add(new ItemSample(pair.Key, pair.Value));
            }

            foreach (FixGetItem i in fixList)
            {
                ItemSample itemGrid = new ItemSample(i.m_id, i.m_num);
                listTmp.Add(itemGrid);
            }

            return listTmp;
        }

        internal void PutIntoPack(ItemPackage pack)
        {
            //List<ItemSample> addItems = GetItemSample();
            //if(null != pack && addItems.Count > 0)
            //{
            //    //pack.AddItemList(addItems);
            //    if(null != PlayerFactory.mMainPlayer && pack == PlayerFactory.mMainPlayer.GetItemPackage())
            //    {
            //        foreach(ItemSample item in addItems)
            //            GlobalShowGui_N.Instance.AddShow(item);
            //    }
            //}
        }
    }

    public class EffCastData
    {
        public int m_id;
        public string m_path;
        public float m_delaytime;
        public float m_liveTime;
        public int m_soundid;
        public int m_direction;
        public string m_posStr;
        public bool m_bind;
		public Vector3 mOffsetPos;

        private static Dictionary<int, EffCastData> m_data = new Dictionary<int, EffCastData>();
        public static EffCastData GetEffCastData(int pID)
        {
            return m_data.ContainsKey(pID) ? m_data[pID] : null;
        }

        public static void LoadData()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("spellEffect");
            reader.Read();
            while (reader.Read())
            {
                EffCastData data = new EffCastData();
				data.mOffsetPos = Vector3.zero;
                data.m_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                data.m_path = reader.GetString(reader.GetOrdinal("path"));
                data.m_delaytime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("time_delay")));
                data.m_liveTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("time_live")));
                data.m_soundid = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sound")));
                data.m_direction = Convert.ToInt32(reader.GetString(reader.GetOrdinal("direction")));
                data.m_posStr = reader.GetString(reader.GetOrdinal("position"));
                data.m_bind = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("bind")));
				data.mOffsetPos.x = Convert.ToSingle(reader.GetString(reader.GetOrdinal("offset_right")));
				data.mOffsetPos.y = Convert.ToSingle(reader.GetString(reader.GetOrdinal("offset_Up")));
				data.mOffsetPos.z = Convert.ToSingle(reader.GetString(reader.GetOrdinal("offset_front")));
                m_data.Add(data.m_id, data);
            }
        }
    }

    public class EffCastList
    {
        public int m_id;
        public int m_preEffId;
        public int m_castingEffId;
        public int m_targetEffId;
        public int m_buffEffId;

        public bool m_isMissile;
        public int m_missileEffId;

        private static Dictionary<int, EffCastList> m_data = new Dictionary<int, EffCastList>();

        public static EffCastList GetEffCastListData(int pID)
        {
            return m_data.ContainsKey(pID) ? m_data[pID] : null;
        }

        public static void LoadData()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("spellVisual");
            //reader.Read();
            while (reader.Read())
            {
                EffCastList data = new EffCastList();

                data.m_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                data.m_preEffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("precast_eff")));
                data.m_castingEffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("casting_eff")));
                data.m_targetEffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("target_eff")));
                data.m_buffEffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("buff_eff")));
                data.m_isMissile = Convert.ToInt32(reader.GetString(reader.GetOrdinal("has_missile"))) == 0 ? false : true;
                data.m_missileEffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("missile_eff")));

                m_data.Add(data.m_id, data);
            }
        }
    }


    public class EffSkillBuffSum    // members are part of EffSkillBuff
    {
        public int m_camp;
        public short m_buffSp;
        public float m_spd = 1.0f;
        public float m_atk;
        public float m_atkDist;
        public float m_def;
        public float m_block = 1.0f;    // TODO : think about it
        public float m_hpMax;
        public float m_satiationMax;
        public float m_comfortMax;
        public float m_satiationDecSpd;
        public float m_comfortDecSpd;
		public float m_jumpHeight = 0;
        public short m_resGotMultiplier;
		public float m_resGotRadius;
//        public float[] m_timeCDChange = new float[EffSkillBuff.NUM_CD_TYPE];	// TODO : use a enum instead of just number; 12 kinds of coolingtime
//        public float[] m_durAtk = new float[EffSkillBuff.NUM_CD_TYPE];

        public EffSkillBuffSum()
        {
            m_camp = -1;
            m_buffSp = 0;
            m_spd = 1.0f;
            m_atk = 0;
            m_atkDist = 0;
            m_def = 0;
            m_block = 1.0f;
            m_hpMax = 0;
            m_satiationMax = 0;
            m_comfortMax = 0;
            m_satiationDecSpd = 1f;
            m_comfortDecSpd = 1f;
			m_jumpHeight = 0;
            m_resGotMultiplier = 0;
			m_resGotRadius = 0.1f;
        }

        public void Clear()
        {
            m_camp = -1;
            m_buffSp = 0;
            m_spd = 1.0f;
            m_atk = 0;
            m_atkDist = 0;
            m_def = 0;
            m_block = 1.0f;
            m_hpMax = 0;
            m_satiationMax = 0;
            m_comfortMax = 0;
            m_satiationDecSpd = 1f;
            m_comfortDecSpd = 1f;
            m_jumpHeight = 0;
            m_resGotMultiplier = 0;
			m_resGotRadius = 0.1f;
        }

        public EffSkillBuffSum SumupToMe(List<EffSkillBuffInst> buffInstList)
        {
            EffSkillBuffSum ret = this;
            for (int i = 0; i < buffInstList.Count; i++)
            {
                EffSkillBuff buff = buffInstList[i].m_buff;
                ret.m_camp = buff.m_changeCamp;
                ret.m_buffSp |= buff.m_buffSp;
                ret.m_spd *= 1 + buff.m_spdChange;
                ret.m_atk += buff.m_atkChange;
                ret.m_atkDist += buff.m_atkDistChange;
                ret.m_def += buff.m_defChange;
                ret.m_block *= 1 + buff.m_block;
                ret.m_hpMax += buff.m_hpMaxChange;
                ret.m_satiationMax += buff.m_satiationMaxChange;
                ret.m_comfortMax += buff.m_comfortMaxChange;
                ret.m_satiationDecSpd *= 1 + buff.m_satiationDecSpdChange;
                ret.m_comfortDecSpd *= 1 + buff.m_comfortDecSpdChange;
				ret.m_jumpHeight += buff.m_jumpHeight;
                ret.m_resGotMultiplier += buff.m_resGotMultiplier;
                ret.m_resGotRadius += buff.m_resGotRadius;				
//                for (int j = 0; j < buff.m_typeTimeCoolingChange.Count; j++)
//                {
//                    int idx = buff.m_typeTimeCoolingChange[j];
//                    ret.m_timeCDChange[idx] += buff.m_timeCoolingChange;
//                    ret.m_durAtk[idx] += buff.m_durAtkChange;
//                }
//
//                //tmp code --- tbd with yinrui
//                if (buff.m_typeTimeCoolingChange.Count == 0)
//                {
//                    ret.m_durAtk[0] += buff.m_durAtkChange;
//                }
            }
            return ret;
        }
        public static EffSkillBuffSum Sumup(EffSkillBuffSum buffSum, List<EffSkillBuffInst> buffInstList)
        {
            EffSkillBuffSum ret;
            if (buffSum != null)
            {
                ret = (EffSkillBuffSum)buffSum.MemberwiseClone();
            }
            else
            {
                ret = new EffSkillBuffSum();
            }
            return ret.SumupToMe(buffInstList);
        }
        public static EffSkillBuffSum Sumup(List<EffSkillBuffInst> buffInstList)
        {
            return Sumup(null, buffInstList);
        }
    }
	
	public class EffSkillBuffMultiply
	{
		public float mAtkChangeP;
		public float mDefChangeP;
		public float mMaxHpChangeP;
		public float mFallInjurse;
		public EffSkillBuffMultiply()
		{
			Rest();
		}
		
		void Rest()
		{
			mAtkChangeP = 1f;
			mDefChangeP = 1f;
			mMaxHpChangeP = 1f;
			mFallInjurse = 1f;
		}
		
		public void ResetBuffMultiply(List<EffSkillBuffInst> buffInstList)
		{
			Rest();
			foreach(EffSkillBuffInst buffIns in buffInstList)
			{
				mAtkChangeP *= 1 + buffIns.m_buff.m_atkChangeP;
				mDefChangeP *= 1 + buffIns.m_buff.m_defChangeP;
				mMaxHpChangeP *= 1 + buffIns.m_buff.m_hpMaxChangeP;
				mFallInjurse *= 1 + buffIns.m_buff.m_fallInjuries;
			}
		}
		
	}

    // All items(armor,weapon,drugs,etc) may have skills
    // So Skill.Exec should be invoked while changing armors, using items, etc
    public class EffSkill
    {
        internal int m_id;
        internal string[] m_name = new string[2];	//name for display, 0 for chinese, 1 for english
        internal string[] m_desc = new string[2];
        internal short m_type;
        internal string m_iconImgPath;
        internal bool m_interruptable;
        internal int m_targetMask;
        internal float m_distOfSkill;
        internal List<EffScope> m_scopeOfSkill;
        internal EffCoolDownInfo m_cdInfo;
        internal EffPrepareInfo m_prepInfo;
        internal EffItemCast m_itemCast;
        internal EffGuidanceInfo m_guidInfo;
        internal EffItemGot m_itemsGot;
        internal List<int> m_skillIdsGot;
		internal List<int> m_metalScanID;
        internal string m_endAction;
        internal float m_endAniTime;

        public bool CheckTargetsValid(SkillRunner caster, ISkillTarget target)
        {
            if (m_scopeOfSkill != null)
                return true;

            // single target in dist
            // These skills allow no target
            if (m_cdInfo.m_type <= 4 && m_cdInfo.m_type >= 1)
            {
                //if (caster is Player)
                //    return true;
            }
			
			Projectile pro = caster.GetComponent<Projectile>();
			if(pro != null)	return true;
			
            if (target != null)
            {
                float atkDist = m_distOfSkill < 0f ? caster.GetAtkDist(target) : m_distOfSkill;

                if (((target.GetPosition() - caster.GetPosition()).sqrMagnitude <= atkDist * atkDist
					|| (atkDist < PETools.PEMath.Epsilon || atkDist > -PETools.PEMath.Epsilon)) &&
                    (m_targetMask & ESkillTarget.Type2Mask(target.GetTargetType())) != 0)
                {
                    return true;
                }
            }

            return false;
        }
        // return: null means failed to run, else skill will continue to check dist even if targetList is empty
        public List<ISkillTarget> GetTargetList(SkillRunner caster, ISkillTarget target)
        {
            List<ISkillTarget> targetList = new List<ISkillTarget>();
            if (m_scopeOfSkill == null)
            {	// single target
                float atkDist = m_distOfSkill < 0f ? caster.GetAtkDist(target) : m_distOfSkill;
                if (target == null || ((target.GetPosition() - caster.GetPosition()).sqrMagnitude > atkDist * atkDist && (atkDist > PETools.PEMath.Epsilon)))
                {
                    // TODO : check strategy
					if ( target != null )
                    //Debug.Log("No target or target is too far, try to find a closer target");
                    if ((target = caster.GetTargetInDist(atkDist, m_targetMask)) != null)
                    {
                        targetList.Add(target);
                    }
                }
                else
                {
                    switch (target.GetTargetType())
                    {
                        default:
//                            int sum = m_targetMask & ESkillTarget.Type2Mask(target.GetTargetType());
                            if ((m_targetMask & ESkillTarget.Type2Mask(target.GetTargetType())) != 0)
                            {
                                targetList.Add(target);
                            }
                            break;
                        case ESkillTargetType.TYPE_Building:
                            if ((m_targetMask & (int)ESkillTarget.TAR_PartnerBuilding) != 0 && !caster.IsEnemy(target))
                            {
                                targetList.Add(target);
                            }
                            else
                                if ((m_targetMask & (int)ESkillTarget.TAR_EnemyBuilding) != 0 && caster.IsEnemy(target))
                                {
                                    targetList.Add(target);
                                }
                            break;
                        case ESkillTargetType.TYPE_SkillRunner:
                            if ((m_targetMask & (int)ESkillTarget.TAR_Partner) != 0 && !caster.IsEnemy(target) && target != caster)
                            {
                                targetList.Add(target);
                            }
                            else
                                if ((m_targetMask & (int)ESkillTarget.TAR_Enemy) != 0 && caster.IsEnemy(target))
                                {
                                    targetList.Add(target);
                                }
                                else if ((m_targetMask & (int)ESkillTarget.TAR_Self) != 0 && !caster.IsEnemy(target))
                                {
                                    targetList.Add(target);
                                }
                            break;
                    }
                }
            }
            else
            {						// multi targets
                for (int i = 0; i < m_scopeOfSkill.Count; i++)
                {
                    List<ISkillTarget> targets = caster.GetTargetlistInScope(m_scopeOfSkill[i], m_targetMask, target);
                    if (targets != null)
                        targetList.AddRange(targets);
                }
            }
            return targetList;
        }
        public bool StopGuidOrNot(List<ISkillTarget> targetList)
        {
            foreach (ISkillTarget target in targetList)
            {
                INaturalResTarget resTarget = target as INaturalResTarget;
                if (resTarget == null || !resTarget.IsDestroyed())
                    return false;
            }
            return true;
        }
		
        // client and server
        public IEnumerator Exec(SkillRunner caster, ISkillTarget target, EffSkillInstance inst)
        {
            caster.m_effSkillInsts.Add(inst);
            float timeStart = Time.time;

            inst.m_section = EffSkillInstance.EffSection.Start;

            // mark: single / net client

            //1: Preparation
            if (m_prepInfo != null)
            {
                //   if (!GameConfig.IsServer || (GameConfig.IsMultiMode() && uLink.Network.isServer))
                //  {
                m_prepInfo.Prepare(caster, target);
                //   }
                if (m_prepInfo.m_DelayR > 0)
                    yield return new WaitForSeconds(m_prepInfo.m_DelayR);

                //    if (!GameConfig.IsServer || GameConfig.IsMultiClient)
                //   {
                if (m_prepInfo.m_ReadySound != null && m_prepInfo.m_ReadySound.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, 100) % m_prepInfo.m_ReadySound.Count;
                    caster.ApplySound(m_prepInfo.m_ReadySound[index]);
                }
                //      }
                //Debug.Log("Skill prepare use "+((Time.time-timeStart)*1000));
                if (m_prepInfo.m_timeCost > PETools.PEMath.Epsilon)
                    yield return new WaitForSeconds(m_prepInfo.m_timeCost - m_prepInfo.m_DelayR);


                //m_prepInfo.PrepareHandle(caster);
            }

            inst.m_section = EffSkillInstance.EffSection.Running;
			
			inst.mSkillCostTimeAdd = false; //don't add time befor guid;
			
            //3:guidance -- if target is a skillrunner, then skill affect target, else skill affect caster
			if (m_guidInfo.m_timeInterval > PETools.PEMath.Epsilon)
            {
                int skillCostTime = (int)(m_guidInfo.m_timeCost / m_guidInfo.m_timeInterval);
                float timeInterval = m_guidInfo.m_timeInterval;
                for (int i = 0; i < skillCostTime; i++)
                {
                    bool finish = false;
                    if (m_guidInfo.m_animNameList != null)
                    {
                        caster.ApplyAnim(m_guidInfo.m_animNameList);
                    }

                    if (m_guidInfo.m_DelayTime <= m_guidInfo.m_SoundDelayTime)
                    {
                        //LogManager.Debug("m_guidInfo.m_DelayTime <= m_guidInfo.m_SoundDelayTime");
                        if (m_guidInfo.m_DelayTime > 0 && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
                            yield return new WaitForSeconds(m_guidInfo.m_DelayTime);

                        if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
                        {
                            int index = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
                            caster.ApplySound(m_guidInfo.m_GuidanceSound[index]);
                        }

                        List<ISkillTarget> targetList = GetTargetList(caster, target);

                        if (m_guidInfo.m_effIdList != null)
                            caster.ApplyEffect(m_guidInfo.m_effIdList, target);

                        if (m_guidInfo.m_TargetEffIDList != null)
                        {
                            for (int j = 0; j < targetList.Count; j++)
                                for (int k = 0; k < m_guidInfo.m_TargetEffIDList.Count; k++)
                                    if (m_guidInfo.m_TargetEffIDList[k] != 0)
                                        EffectManager.Instance.Instantiate(m_guidInfo.m_TargetEffIDList[k], targetList[j].GetPosition(), Quaternion.identity, null);
                        }
						
						if(m_guidInfo.m_SoundDelayTime > m_guidInfo.m_DelayTime)
							yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime - m_guidInfo.m_DelayTime);

						//2:dummy item cast
						if (m_itemCast != null && m_itemCast.m_itemId > 0)
							m_itemCast.Cast(caster, target);

						finish = m_guidInfo.TakeEffect(caster, targetList, m_id);

						if(timeInterval > m_guidInfo.m_SoundDelayTime)
							yield return new WaitForSeconds(timeInterval - m_guidInfo.m_SoundDelayTime);
                        //						if(StopGuidOrNot(targetList))
                        //						{
                        //							break;
                        //						}
                    }
                    else
                    {
                        //LogManager.Debug("!m_guidInfo.m_DelayTime <= m_guidInfo.m_SoundDelayTime");
                        if (m_guidInfo.m_effIdList != null)
                        {
                            caster.ApplyEffect(m_guidInfo.m_effIdList, target);
                        }

                        List<ISkillTarget> targetList = GetTargetList(caster, target);

						if(m_guidInfo.m_SoundDelayTime > 0)
                        	yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime);

                        if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
                        {
                            int index = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
                            caster.ApplySound(m_guidInfo.m_GuidanceSound[index]);
                        }

                        if (m_guidInfo.m_DelayTime > 0 && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
                            yield return new WaitForSeconds(m_guidInfo.m_DelayTime - m_guidInfo.m_SoundDelayTime);

						//2:dummy item cast
						if (m_itemCast != null && m_itemCast.m_itemId > 0)
							m_itemCast.Cast(caster, target);
						
						finish = m_guidInfo.TakeEffect(caster, targetList, m_id);

						if(timeInterval > m_guidInfo.m_DelayTime)
                        	yield return new WaitForSeconds(timeInterval - m_guidInfo.m_DelayTime);
                        //						if(StopGuidOrNot(targetList))
                        //						{
                        //							break;
                        //						}
                    }
					
					if (inst.mSkillCostTimeAdd)
                    {
                        skillCostTime = i + 2;
                        inst.mSkillCostTimeAdd = false;
                    }
					
					if(finish && inst.mNextTarget == null)
						break;
					if(inst.mNextTarget != null)
					{
						VFTerrainTarget terrainTaget = inst.mNextTarget as VFTerrainTarget;
						if(terrainTaget != null)
						{
							VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(terrainTaget.m_intPos.x, terrainTaget.m_intPos.y, terrainTaget.m_intPos.z);
							if(voxel.Volume > 127)
							{
								target = inst.mNextTarget;
								inst.mNextTarget = null;
							}
							else
							{
								inst.mNextTarget = null;
								break;
							}
						}
						else
						{
							inst.mNextTarget = null;
							break;
						}
					}
                }
            }
            else
            {
				//2:dummy item cast
				if (m_itemCast != null && m_itemCast.m_itemId > 0)
					m_itemCast.Cast(caster, target);

				List<ISkillTarget> targetList = GetTargetList(caster, target);

				// Take effect (hp change, item got and lose and so on)
				m_guidInfo.TakeEffect(caster, targetList, m_id);

				// Apply sound
				if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
                    caster.ApplySound(m_guidInfo.m_GuidanceSound[index]);
                }

				// Apply effect
				if (m_guidInfo.m_effIdList != null)
                   caster.ApplyEffect(m_guidInfo.m_effIdList, target);
            }


			// Only stand alone implement below
			if (!GameConfig.IsMultiMode)
			{
				//4:items got -- skill affect caster
				if (m_itemsGot != null)
				{
					ItemPackage pack = caster.GetItemPackage();
					if (pack != null)
					{
						m_itemsGot.PutIntoPack(pack);
					}
				}

				//5:skill learned -- skill affect caster
				// TODO : learn skill
				if (m_skillIdsGot != null)
					caster.ApplyLearnSkill(m_skillIdsGot);
				if(m_metalScanID != null)
					caster.ApplyMetalScan(m_metalScanID);
            }
			//else if (m_itemsGot != null || m_skillIdsGot != null)
			//{
			//	if (null != caster && null != caster.OwnerView) { 
			//		caster.RPC("RPC_C2S_SkillItemGot", m_id, caster.OwnerView.viewID);
			//	}
			//}

//MissEnd:
            if (m_endAction != "0")
            {
                List<string> f_EndAction = new List<string>();
                f_EndAction.Add(m_endAction);
                caster.ApplyAnim(f_EndAction);
            }

            if (m_endAniTime > PETools.PEMath.Epsilon)
                yield return new WaitForSeconds(m_endAniTime);

            inst.m_section = EffSkillInstance.EffSection.Completed;


            if (Time.time < timeStart + m_cdInfo.m_timeCost)
            {
                yield return new WaitForSeconds((timeStart + m_cdInfo.m_timeCost) - Time.time);
            }
            //Debug.Log("[EFFSKILL]:SkillId:"+m_id+",cooling:"+m_cdInfo.m_timeCost+",timeEclapsed:"+(Time.time-timeStart));
            caster.m_effSkillInsts.Remove(inst);
        }

		public IEnumerator ExecProxy(SkillRunner caster, ISkillTarget target, EffSkillInstance inst)
		{
			caster.m_effSkillInsts.Add(inst);
			float timeStart = Time.time;

            inst.m_section = EffSkillInstance.EffSection.Start;
			// mark: single / net client

			//1: Preparation
			if (m_prepInfo != null)
			{
				//   if (!GameConfig.IsServer || (GameConfig.IsMultiMode() && uLink.Network.isServer))
				//  {
				m_prepInfo.Prepare(caster, target);
				//   }
				if (m_prepInfo.m_DelayR > 0)
					yield return new WaitForSeconds(m_prepInfo.m_DelayR);

				//    if (!GameConfig.IsServer || GameConfig.IsMultiClient)
				//   {
				if (m_prepInfo.m_ReadySound != null && m_prepInfo.m_ReadySound.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, 100) % m_prepInfo.m_ReadySound.Count;
					caster.ApplySound(m_prepInfo.m_ReadySound[index]);
				}
				//      }
				//Debug.Log("Skill prepare use "+((Time.time-timeStart)*1000));
				if (m_prepInfo.m_timeCost > PETools.PEMath.Epsilon)
					yield return new WaitForSeconds(m_prepInfo.m_timeCost - m_prepInfo.m_DelayR);


				//m_prepInfo.PrepareHandle(caster);
			}

            inst.m_section = EffSkillInstance.EffSection.Running;

			inst.mSkillCostTimeAdd = false; //don't add time befor guid;

			//3:guidance -- if target is a skillrunner, then skill affect target, else skill affect caster
			bool bAffectCaster = (m_targetMask & ESkillTarget.Type2Mask(ESkillTargetType.TYPE_SkillRunner)) == 0 ? true : false;
			if (m_guidInfo.m_timeInterval > PETools.PEMath.Epsilon)
			{
				int skillCostTime = (int)(m_guidInfo.m_timeCost / m_guidInfo.m_timeInterval);
				float timeInterval = m_guidInfo.m_timeInterval;
				for (int i = 0; i < skillCostTime; i++)
				{
					bool finish = false;
					if (m_guidInfo.m_animNameList != null)
					{
						caster.ApplyAnim(m_guidInfo.m_animNameList);
					}
					if (m_guidInfo.m_DelayTime <= m_guidInfo.m_SoundDelayTime)
					{
						if (m_guidInfo.m_DelayTime > 0 && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
							yield return new WaitForSeconds(m_guidInfo.m_DelayTime);

						if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
						{
							int index = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
							caster.ApplySound(m_guidInfo.m_GuidanceSound[index]);
						}

						List<ISkillTarget> targetList = GetTargetList(caster, target);

						if (m_guidInfo.m_effIdList != null)
							caster.ApplyEffect(m_guidInfo.m_effIdList, target);
						if (m_guidInfo.m_TargetEffIDList != null)
						{
							for (int j = 0; j < targetList.Count; j++)
								for (int k = 0; k < m_guidInfo.m_TargetEffIDList.Count; k++)
									if (m_guidInfo.m_TargetEffIDList[k] != 0)
										EffectManager.Instance.Instantiate(m_guidInfo.m_TargetEffIDList[k], targetList[j].GetPosition(), Quaternion.identity, null);
						}

						if (m_guidInfo.m_SoundDelayTime > m_guidInfo.m_DelayTime)
							yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime - m_guidInfo.m_DelayTime);

						//2:dummy item cast
						if (m_itemCast != null && m_itemCast.m_itemId > 0)
							m_itemCast.Cast(caster, target);

						finish = m_guidInfo.TakeEffectProxy(caster, targetList, m_id, bAffectCaster);

						if (timeInterval > m_guidInfo.m_SoundDelayTime)
							yield return new WaitForSeconds(timeInterval - m_guidInfo.m_SoundDelayTime);
					}
					else
					{
						if (m_guidInfo.m_effIdList != null)
						{
							caster.ApplyEffect(m_guidInfo.m_effIdList, target);
						}

						List<ISkillTarget> targetList = GetTargetList(caster, target);

						if (m_guidInfo.m_SoundDelayTime > 0)
							yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime);

						if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
						{
							int index = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
							caster.ApplySound(m_guidInfo.m_GuidanceSound[index]);
						}

						if (m_guidInfo.m_DelayTime > 0 && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
							yield return new WaitForSeconds(m_guidInfo.m_DelayTime - m_guidInfo.m_SoundDelayTime);

						//2:dummy item cast
						if (m_itemCast != null && m_itemCast.m_itemId > 0)
							m_itemCast.Cast(caster, target);

						finish = m_guidInfo.TakeEffectProxy(caster, targetList, m_id, bAffectCaster);

						if (timeInterval > m_guidInfo.m_DelayTime)
							yield return new WaitForSeconds(timeInterval - m_guidInfo.m_DelayTime);
					}

					if (inst.mSkillCostTimeAdd)
					{
						skillCostTime = i + 2;
						inst.mSkillCostTimeAdd = false;
					}

					if (finish)
					{
						if (inst.mNextTarget != null)
						{
							VFTerrainTarget terrainTaget = inst.mNextTarget as VFTerrainTarget;
							if (terrainTaget != null)
							{
								VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(terrainTaget.m_intPos.x, terrainTaget.m_intPos.y, terrainTaget.m_intPos.z);
								if (voxel.Volume > 127)
								{
									target = inst.mNextTarget;
									inst.mNextTarget = null;
								}
								else
								{
									inst.mNextTarget = null;
									break;
								}
							}
							else
							{
								inst.mNextTarget = null;
								break;
							}
						}
						else
							break;
					}
				}
			}
			else
			{
				//2:dummy item cast
				if (m_itemCast != null && m_itemCast.m_itemId > 0)
					m_itemCast.Cast(caster, target);

				List<ISkillTarget> targetList = GetTargetList(caster, target);

				// Take effect (hp change, item got and lose and so on)
				m_guidInfo.TakeEffectProxy(caster, targetList, m_id, bAffectCaster);

				// Apply sound
				if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
					caster.ApplySound(m_guidInfo.m_GuidanceSound[index]);
				}

				// Apply effect
				if (m_guidInfo.m_effIdList != null)
					caster.ApplyEffect(m_guidInfo.m_effIdList, target);
			}

//		MissEnd:
			if (m_endAction != "0")
			{
				List<string> f_EndAction = new List<string>();
				f_EndAction.Add(m_endAction);
				caster.ApplyAnim(f_EndAction);
			}

            if (m_endAniTime > PETools.PEMath.Epsilon)
                yield return new WaitForSeconds(m_endAniTime);

            inst.m_section = EffSkillInstance.EffSection.Completed;

			if (Time.time < timeStart + m_cdInfo.m_timeCost)
			{
				yield return new WaitForSeconds((timeStart + m_cdInfo.m_timeCost) - Time.time);
			}

			caster.m_effSkillInsts.Remove(inst);
		}

        public IEnumerator SkipExec(SkillRunner caster, EffSkillInstance inst)
        {
            caster.m_effSkillInsts.Add(inst);
            float timeStart = Time.time;

            if (Time.time < timeStart + m_cdInfo.m_timeCost)
            {
                yield return new WaitForSeconds((timeStart + m_cdInfo.m_timeCost) - Time.time);
            }
            caster.m_effSkillInsts.Remove(inst);
        }

        public IEnumerator SharingCooling(SkillRunner caster, EffSkillInstance inst)
        {
            if (m_cdInfo.m_timeShared <= 0)
                yield break;

            caster.m_effShareSkillInsts.Add(inst);

            yield return new WaitForSeconds(m_cdInfo.m_timeShared);

            caster.m_effShareSkillInsts.Remove(inst);
        }

        public static List<string> s_tblEffSkillsColName_CN, s_tblEffSkillsColName_EN;
        public static List<EffSkill> s_tblEffSkills;
        public static void LoadData()
        {
            if (s_tblEffSkills != null)
                return;

            if (EffSkillBuff.s_tblEffSkillBuffs == null)
                EffSkillBuff.LoadData();

            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("skill");
            int nFieldCount = reader.FieldCount;

            s_tblEffSkillsColName_CN = new List<string>(nFieldCount);
            reader.Read();
            for (int i = 0; i < nFieldCount; i++)
                s_tblEffSkillsColName_CN.Add(reader.GetString(i));

            s_tblEffSkillsColName_EN = new List<string>(nFieldCount);
            reader.Read();
            for (int i = 0; i < nFieldCount; i++)
                s_tblEffSkillsColName_EN.Add(reader.GetString(i));

            s_tblEffSkills = new List<EffSkill>();
            while (reader.Read())
            {
                EffSkill skill = new EffSkill();
                skill.m_id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_id")));
                skill.m_name[0] = reader.GetString(reader.GetOrdinal("_name"));
                skill.m_name[1] = reader.GetString(reader.GetOrdinal("_engname"));
                skill.m_type = Convert.ToInt16(reader.GetString(reader.GetOrdinal("_type")));
                skill.m_iconImgPath = reader.GetString(reader.GetOrdinal("_icon"));
                skill.m_desc[0] = PELocalization.GetString(Convert.ToInt32(reader.GetString(reader.GetOrdinal("_desc"))));
                skill.m_desc[1] = reader.GetString(reader.GetOrdinal("_engdesc"));
                skill.m_interruptable = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_interruptable"))) == 0 ? false : true;
                skill.m_targetMask = ToBitMask(reader.GetString(reader.GetOrdinal("_target")));
                skill.m_distOfSkill = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_distCast")));
                skill.m_scopeOfSkill = EffScope.Create(reader);
                skill.m_cdInfo = EffCoolDownInfo.Create(reader);
                skill.m_prepInfo = EffPrepareInfo.Create(reader);
                skill.m_itemCast = EffItemCast.Create(reader, skill.m_id);
                skill.m_guidInfo = EffGuidanceInfo.Create(reader);
                skill.m_itemsGot = EffItemGot.Create(reader);
                skill.m_skillIdsGot = ToListInt32P(reader.GetString(reader.GetOrdinal("_learnSkill")));
				skill.m_metalScanID = ToListInt32P(reader.GetString(reader.GetOrdinal("_mineupdate")));
                skill.m_endAction = reader.GetString(reader.GetOrdinal("_endAction"));
                skill.m_endAniTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("_endTime")));
                s_tblEffSkills.Add(skill);
            }
        }

        //
        public static void AddWeaponSkill(int id, string name, int buffID)
        {
            EffSkill skill = new EffSkill();

            skill.m_id = id;
            skill.m_name[0] = name;
            skill.m_name[1] = name;
            skill.m_type = 4;
            skill.m_iconImgPath = "";
            skill.m_desc[0] = "0";
            skill.m_desc[1] = "0";
            skill.m_interruptable = false;
            skill.m_targetMask = ToBitMask("3");
            skill.m_distOfSkill = 0;
            skill.m_scopeOfSkill = null;
            skill.m_cdInfo = new EffCoolDownInfo();
            skill.m_prepInfo = null;
            skill.m_itemCast = null;
            skill.m_guidInfo = new EffGuidanceInfo(buffID);
            skill.m_itemsGot = null;
            skill.m_skillIdsGot = null;
            skill.m_endAction = "0";

            //
            s_tblEffSkills.Add(skill);
        }

        public static bool MatchId(EffSkill iter, int id)
        {
            return iter.m_id == id;
        }
        public static int ToBitMask(string desc)
        {
            string[] strings = desc.Split(',');
            int bitMask = 0;
            for (int i = 0; i < strings.Length; i++)
            {
                byte v = Convert.ToByte(strings[i]);
                if (v > 0)
                {
                    bitMask |= 1 << (v - 1);
                }
            }
            return bitMask;
        }
        public static List<byte> ToListByteP(string desc)
        {
            string[] strings = desc.Split(',');
            byte v = Convert.ToByte(strings[0]);
            if (v <= 0)
                return null;

            List<byte> list = new List<byte>();
            list.Add(v);
            for (int i = 1; i < strings.Length; i++)
            {
                list.Add(Convert.ToByte(strings[i]));
            }
            return list;
        }
        public static List<int> ToListInt32P(string desc)
        {
            string[] strings = desc.Split(',');
            int v = Convert.ToInt32(strings[0]);
            if (v <= 0)
                return null;

            List<int> list = new List<int>();
            list.Add(v);
            for (int i = 1; i < strings.Length; i++)
            {
                list.Add(Convert.ToInt32(strings[i]));
            }
            return list;
        }
        public static List<string> ToListString(string desc)
        {
            string[] strings = desc.Split(',');
            if (0 == strings[0].CompareTo("0"))
                return null;

            List<string> list = new List<string>(strings);
            return list;
        }
    }

}
