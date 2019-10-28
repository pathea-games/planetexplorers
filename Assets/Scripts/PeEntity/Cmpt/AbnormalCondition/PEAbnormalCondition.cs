using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using SkillSystem;
using Pathea;

public enum PEAbnormalType : int
{
	None = 0,
	Bleeding,
	FoodPoisoning,
	InjurePoisoning,
	GRV_Player_Low,
	GRV_Player_High,//5
	GRV_NPC_Low, 
	GRV_NPC_High,
	Hunger_Level1,
	Gastropathy,
	StrengthOverdraft, //10
	Suffocate_Player, 
	Suffocate_NPC,
	Drowning_Player,
	Drowning_NPC,
	Injure_Low,	//15
	Injure_High, 
	Tinnitus,	
	Dazzling,
	Flashlight,
	BlurredVision, //20
	Deafness,
	BacterialInfection,
	MedicalAccident,
	Hunger_Level2,
	Hunger_Level3, //25
	Hunger_Level4,
	Hunger_Level5,
	Food_Meat_Level1,
	Food_Meat_Level2,
	Food_Meat_Level3, //30
	Food_Bean_Level1,
	Food_Bean_Level2,
	Food_Bean_Level3,
	Food_Fruit_Level1,
	Food_Fruit_Level2, //35
	Food_Energy_Level1,
	Food_Energy_Level2,
	Food_Balanced_Level1,
	Food_Balanced_Level2,
	VirusInfection,
	LymeDisease,
	Dehydration,
	Infection,
	Cold_Water,
	Cold_Rain,
	Fever,
	Muzzy_Level1,
	Muzzy_Level2,
	PhysicalOverdraft_Level1,
	PhysicalOverdraft_Level2,
	Max
}

public class AbnormalData
{
	public class HitAttr
	{
		public AttribType attrType;
		public float minThreshold;
		public float maxThreshold;
		public float minRate;
		public float maxRate;
		
		public float GetRate(float val)
		{
			if (val < minThreshold || val > maxThreshold)
				return 0;			
			return Mathf.Lerp(minRate, maxRate, (val - minThreshold)/(maxThreshold - minThreshold));
		}
		
		public float GetRate(PeEntity entity)
		{
			if (null == entity)
				return 0;
			return GetRate (entity.GetAttribute (attrType));
		}

		public static HitAttr GetHitAttr(SqliteDataReader reader, string fieldName)
		{
			return GetHitAttr(reader.GetString(reader.GetOrdinal(fieldName)));
		}

		static HitAttr GetHitAttr(string attrStr)
		{
			if("0" == attrStr)
				return null;
			HitAttr ret = new HitAttr();
			string[] strs = attrStr.Split(',');
			ret.attrType = (AttribType)Convert.ToInt32(strs[0]);
			ret.minThreshold = Convert.ToSingle(strs[1]);
			ret.maxThreshold = Convert.ToSingle(strs[2]);
			ret.minRate = Convert.ToSingle(strs[3]);
			ret.maxRate = Convert.ToSingle(strs[4]);
			return ret;
		}

		public static HitAttr[] GetHitAttrArray(SqliteDataReader reader, string fieldName)
		{
			string str = reader.GetString(reader.GetOrdinal(fieldName));
			if("0" == str)
				return null;

			string[] subStrs = str.Split(';');
			HitAttr[] ret = new HitAttr[subStrs.Length];
			for(int i = 0; i < subStrs.Length; ++i)
				ret[i] = GetHitAttr(subStrs[i]);
			return ret;
		}
	}
	
	public class ThresholdData
	{
		public int 		type;
		public float 	threshold;

		static ThresholdData GetThresholdData(string str)
		{
			float[] readValues = PETools.Db.ReadFloatArray(str);
			if(null == readValues)
				return null;
			ThresholdData ret = new ThresholdData();
			ret.type = Mathf.RoundToInt(readValues[0]);
			ret.threshold = readValues[1];
			return ret;
		}

		public static ThresholdData GetThresholdData(SqliteDataReader reader, string fieldName)
		{
			string str = reader.GetString(reader.GetOrdinal(fieldName));
			if(string.IsNullOrEmpty(str) || str == "0")
				return null;
			return GetThresholdData(str);
		}

		public static ThresholdData[] GetThresholdDatas(SqliteDataReader reader, string fieldName)
		{
			string str = reader.GetString(reader.GetOrdinal(fieldName));
			if(string.IsNullOrEmpty(str) || str == "0")
				return null;
			string[] subStr = str.Split(';');
			ThresholdData[] ret = new ThresholdData[subStr.Length];
			for(int i = 0; i < subStr.Length; ++i)
				ret[i] = GetThresholdData(subStr[i]);
			return ret;
		}
	}
	
//	public class EffAnim
//	{
//		public int 		type;
//		public string	animParam;
//		public float	value;
//
//		public static EffAnim GetEffAnim(SqliteDataReader reader, string fieldName)
//		{
//			string str = reader.GetString(reader.GetOrdinal(fieldName));
//			if("0" == str)
//				return null;
//			string[] subStrs = str.Split(',');
//			EffAnim ret = new EffAnim();
//			ret.type = Convert.ToInt32(subStrs[0]);
//			ret.animParam = subStrs[1];
//			ret.value = Convert.ToSingle(subStrs[2]);
//			return ret;
//		}
//	}

	public class EffCamera
	{
		public int 		type;
		public float 	value;
		public static EffCamera GetEffCamera(SqliteDataReader reader, string fieldName)
		{
			string str = reader.GetString(reader.GetOrdinal(fieldName));
			if("0" == str)
				return null;
			string[] subStrs = str.Split(',');
			EffCamera ret = new EffCamera();
			ret.type = Convert.ToInt32(subStrs[0]);
			ret.value = Convert.ToSingle(subStrs[1]);
			return ret;
		}
	}

	public PEAbnormalType 	type;
	public string			name;
	public string			iconName;
	public string			description;
	public int				target; //1:Player 2:NPC 3:Monster
	public bool				deathRemove;
	public bool				updateByModel;

	public float			trigger_TimeInterval;
	public int[]			trigger_BuffAdd;
	public int[]			trigger_ItemGet;
	public bool				trigger_Damage;
	public bool				trigger_InWater;

	public PEAbnormalType[]	hit_MutexAbnormal;
	public PEAbnormalType[]	hit_PreAbnormal;
	public int[]			hit_BuffID;
	public HitAttr[]		hit_Attr;
	public HitAttr			hit_Damage;
	public float			hit_TimeInterval;
	public float[]			hit_AreaTime;
	public float			hit_RainTime;
	public float			hit_HitRate;

	public int[]			eff_BuffAddList;
	public string			eff_Anim;
	public EffCamera		eff_Camera;
	public PEAbnormalType[]	eff_AbnormalRemove;
	public int[]			eff_Particles;
	public Color			eff_SkinColor;
	public ThresholdData[]	eff_BodyWeight;

	public bool				rt_Immediate;
	public float			rt_TimeInterval;
	public int[]			rt_BuffRemove;
	public bool				rt_EffectEnd;
	public bool				rt_OutsideWater;

	public int[]			rh_BuffList;
	public HitAttr[]		rh_Attr;

	public int[]			re_BuffRemove;
	public int[]			re_BuffAdd;
	public PEAbnormalType[]	re_AbnormalAdd;
	public string			re_Anim;
	public EffCamera		re_Camera;
	public int[]			re_Particles;

	static Dictionary<PEAbnormalType, AbnormalData> g_DataDic;
	public static void LoadData()
	{
		g_DataDic = new Dictionary<PEAbnormalType, AbnormalData>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AbnormalType");
		while (reader.Read())
		{
			AbnormalData data = new AbnormalData();
			data.type = (PEAbnormalType)PETools.Db.GetInt(reader, "AbnormalId");
			data.name = PELocalization.GetString(PETools.Db.GetInt(reader, "TranslationNameId"));
			data.iconName = PETools.Db.GetString(reader, "Icon");
			data.description = PELocalization.GetString(PETools.Db.GetInt(reader, "TranslationDescribeId"));
			data.target = PETools.Db.GetInt(reader, "AbnormalTarget");
			data.deathRemove = PETools.Db.GetBool(reader, "IsDeathRemove");
			data.updateByModel = PETools.Db.GetBool(reader, "UpdateByModel");

			data.trigger_TimeInterval = PETools.Db.GetFloat(reader, "Trigger_Time");
			data.trigger_BuffAdd = PETools.Db.GetIntArray(reader, "Trigger_BuffAdd");
			data.trigger_ItemGet = PETools.Db.GetIntArray(reader, "Trigger_ItemGet");
			data.trigger_Damage = PETools.Db.GetBool(reader, "Trigger_Damage");
			data.trigger_InWater = PETools.Db.GetBool(reader, "Trigger_IntoWater");
			
			data.hit_MutexAbnormal = GetAbnormalType(reader, "Hit_MutexAbnormal");
			data.hit_PreAbnormal = GetAbnormalType(reader, "Hit_PreAbnormal");
			data.hit_BuffID = PETools.Db.GetIntArray(reader, "Hit_BuffList");
			data.hit_Attr = HitAttr.GetHitAttrArray(reader, "Hit_Attr");
			data.hit_Damage = HitAttr.GetHitAttr(reader, "Hit_Damage");
			data.hit_TimeInterval = PETools.Db.GetFloat(reader, "Hit_Time");
			data.hit_AreaTime = PETools.Db.GetFloatArray(reader, "Hit_AreaTime");
			data.hit_RainTime = PETools.Db.GetFloat(reader, "Hit_RainTime");
			data.hit_HitRate = PETools.Db.GetFloat(reader, "Hit_Rate");
			
			data.eff_BuffAddList = PETools.Db.GetIntArray(reader, "Eff_BuffAdd");
			data.eff_Anim = reader.GetString(reader.GetOrdinal("Eff_Anim"));
			data.eff_Camera = EffCamera.GetEffCamera(reader, "Eff_Camera");
			data.eff_AbnormalRemove = GetAbnormalType(reader, "Eff_RemoveAbnormal");
			data.eff_Particles = PETools.Db.GetIntArray(reader, "Eff_Particle");
			data.eff_SkinColor = PETools.Db.GetColor(reader, "Eff_SkinColor");
			data.eff_BodyWeight = ThresholdData.GetThresholdDatas(reader, "Eff_BodyWeight");
			
			data.rt_Immediate = PETools.Db.GetBool(reader, "RT_Imm");
			data.rt_TimeInterval = PETools.Db.GetFloat(reader, "RT_Time");
			data.rt_BuffRemove = PETools.Db.GetIntArray(reader, "RT_BuffRemove");
			data.rt_EffectEnd = PETools.Db.GetBool(reader, "RT_EffEnd");
			data.rt_OutsideWater = PETools.Db.GetBool(reader, "RT_OutWater");
			
			data.rh_BuffList = PETools.Db.GetIntArray(reader, "RH_BuffRemove");
			data.rh_Attr = HitAttr.GetHitAttrArray(reader, "RH_Attr");
			
			data.re_BuffRemove = PETools.Db.GetIntArray(reader, "RE_BuffRemove");
			data.re_BuffAdd = PETools.Db.GetIntArray(reader, "RE_BuffAdd");
			data.re_AbnormalAdd = GetAbnormalType(reader, "RE_AbnormalAdd");
			data.re_Anim = reader.GetString(reader.GetOrdinal("RE_Anim"));
			data.re_Camera = EffCamera.GetEffCamera(reader, "RE_Camera");
			data.re_Particles = PETools.Db.GetIntArray(reader, "RE_Particle");

			g_DataDic.Add(data.type, data);
		}
	}

	static PEAbnormalType[] GetAbnormalType(SqliteDataReader reader, string fieldName)
	{
		int[] intArray = PETools.Db.GetIntArray(reader, fieldName);
		if(null == intArray || intArray.Length == 0)
			return null;
		PEAbnormalType[] ret = new PEAbnormalType[intArray.Length];
		for(int i = 0; i < ret.Length; ++i)
			ret[i] = (PEAbnormalType)intArray[i];
		return ret;
	}

	public static AbnormalData GetData(PEAbnormalType type)
	{
		if(g_DataDic.ContainsKey(type))
			return g_DataDic[type];

		Debug.LogError("Can't find abnormaltype:" + type.ToString());
		return null;
	}
}

public class PEAbnormal_N
{
	public virtual PEAbnormalType type{ get { return m_Data.type; } }
	public bool hasEffect { get; protected set; }

	const int Version = 100;
	
	PeEntity m_Entity;
	AbnormalData m_Data;
	bool m_bReqApplyEff = false;
	AbnormalConditionCmpt m_AbnormalCmpt;
	event Action<PEAbnormalType> evtStart;
	event Action<PEAbnormalType> evtEnd;

	List<PEAbnormalTrigger> m_Triggers = new List<PEAbnormalTrigger>();
	List<PEAbnormalHit> m_HitRates = new List<PEAbnormalHit>();
	List<PEAbnormalEff> m_Effs = new List<PEAbnormalEff>();

	List<PEAbnormalTrigger> m_RemoveTriggers = new List<PEAbnormalTrigger>();
	List<PEAbnormalHit> m_RemoveHits = new List<PEAbnormalHit>();
	List<PEAbnormalEff> m_RemoveEffs = new List<PEAbnormalEff>();

	List<int> m_NeedSaveBuffList = new List<int>();
	List<int> m_SaveBuffList = new List<int>();

	bool m_EndImm;
	
	public bool effectEnd
	{
		get
		{
			for(int i = 0; i < m_Effs.Count; ++i)
				if(!m_Effs[i].effectEnd)
					return false;
			return true;
		}
	}
	
	public void Init(PEAbnormalType abnormalType, AbnormalConditionCmpt ctrlCmpt, PeEntity entity, Action<PEAbnormalType> startEvtFunc, Action<PEAbnormalType> endEvtFunc)
	{
		m_AbnormalCmpt = ctrlCmpt;
		m_Entity = entity;
		evtStart += startEvtFunc;
		evtEnd += endEvtFunc;
		InitData (abnormalType);
	}
	
	public byte[] Serialize()
	{
		if(!hasEffect) return null;
//		if(0 == m_NeedSaveBuffList.Count) return null;		
		m_SaveBuffList.Clear();
		for(int i = 0; i < m_NeedSaveBuffList.Count; ++i)
			if(null != m_Entity.skEntity.GetSkBuffInst(m_NeedSaveBuffList[i]))
				m_SaveBuffList.Add(m_NeedSaveBuffList[i]);

//		if(0 == m_SaveBuffList.Count) return null;

		return PETools.Serialize.Export ((w) =>
		{
			w.Write(Version);
			w.Write(m_SaveBuffList.Count);
			for(int i = 0; i < m_SaveBuffList.Count; i++)
				w.Write(m_SaveBuffList[i]);
		});

		//return null; 
	}
	public void Deserialize(byte[] data) 
	{
		if(null == data)
			return;

		PETools.Serialize.Import(data, (r) =>
		{
			int readVersion = r.ReadInt32();
			if(readVersion != Version)
				return;
			int count = r.ReadInt32 ();
			if(count > 0)
			{
				hasEffect = true;
				while(count-- > 0)
				{
					SkEntity.MountBuff(m_Entity.skEntity , r.ReadInt32 (), null, null);
				}
			}
		});
		m_bReqApplyEff = true;
	}
	
	public void OnDie()	{ if(m_Data.deathRemove) EndCondition(); }
	
	public void OnRevive() { }
	
	public void StartCondition() 
	{
		if(!hasEffect)
			ApplyEffect();
	}
	
	public void EndCondition()
	{
		if(hasEffect)
			ApplyEndEffect();
	}

	public void Update()
	{
		if (m_bReqApplyEff) {
			m_bReqApplyEff = false;
			ApplyEffect ();
		}

		if(!hasEffect && m_Data.updateByModel && !m_Entity.biologyViewCmpt.hasView)
			return;

		if(hasEffect)
		{
			bool triggerHit = m_RemoveTriggers.Count > 0;
			for(int i = 0; i < m_RemoveTriggers.Count; ++i)
				triggerHit = triggerHit && m_RemoveTriggers[i].Hit();

			if(triggerHit)
				CheckRemove();
			
			for(int i = 0; i < m_RemoveHits.Count; ++i)
				m_RemoveHits[i].Update();

			for(int i = 0; i < m_Effs.Count; ++i)
				m_Effs[i].Update();
		}
		else
		{
			bool triggerHit = m_Triggers.Count > 0;
			for(int i = 0; i < m_Triggers.Count; ++i)
				triggerHit = triggerHit && m_Triggers[i].Hit();

			if(triggerHit && !m_AbnormalCmpt.Entity.IsDeath())
				CheckHit();

			for(int i = 0; i < m_HitRates.Count; ++i)
				m_HitRates[i].Update();
		}

		for(int i = 0; i < m_Triggers.Count; ++i)
			m_Triggers[i].Update();

		for(int i = 0; i < m_RemoveTriggers.Count; ++i)
			m_RemoveTriggers[i].Update();
	}

	void CheckHit()
	{
		float hitRate = 1f;
		for(int i = 0; i < m_HitRates.Count; ++i)
		{
			m_HitRates[i].preHit = hitRate > PETools.PEMath.Epsilon;
			hitRate *= m_HitRates[i].HitRate();
		}
		if(hitRate > PETools.PEMath.Epsilon && UnityEngine.Random.value < hitRate)
		{
			ApplyEffect();
		}
	}

	void ApplyEffect()
	{
		try
		{
			for(int i = 0; i < m_Effs.Count; ++i)
				m_Effs[i].Do();
			for(int i = 0; i < m_HitRates.Count; ++i)
				m_HitRates[i].Clear();
		}
		catch (Exception e) 
		{
			UnityEngine.Debug.LogError(e);
		}
		if(null != evtStart)
			evtStart(m_Data.type);
		hasEffect = true;
		ClearTriggerHit();
		if(m_EndImm)
			ApplyEndEffect();
	}

	void CheckRemove()
	{
		float hitRate = 1f;
		for(int i = 0; i < m_RemoveHits.Count; ++i)
		{
			m_RemoveHits[i].preHit = hitRate > PETools.PEMath.Epsilon;
			hitRate *= m_RemoveHits[i].HitRate();
		}

		if(hitRate > PETools.PEMath.Epsilon && UnityEngine.Random.value < hitRate)
			ApplyEndEffect();
	}

	void ApplyEndEffect()
	{
		hasEffect = false;
		if(null != evtEnd)
			evtEnd(m_Data.type);
		try
		{
			for(int i = 0; i < m_Effs.Count; ++i)
				m_Effs[i].End();
			for(int i = 0; i < m_RemoveEffs.Count; ++i)
				m_RemoveEffs[i].Do();
			for(int i = 0; i < m_RemoveHits.Count; ++i)
				m_RemoveHits[i].Clear();
		}
		catch (Exception e) 
		{
			UnityEngine.Debug.LogError(e);
		}


		ClearTriggerHit();
	}
	
	void ClearTriggerHit()
	{
		for(int i = 0; i < m_Triggers.Count; ++i)
			m_Triggers[i].Clear();
		for(int i = 0; i < m_HitRates.Count; ++i)
			m_HitRates[i].Clear();
		for(int i = 0; i < m_RemoveTriggers.Count; ++i)
			m_RemoveTriggers[i].Clear();
		for(int i = 0; i < m_RemoveHits.Count; ++i)
			m_RemoveHits[i].Clear();
	}

	void InitData(PEAbnormalType abnormalType)
	{
		m_Data = AbnormalData.GetData(abnormalType);
		if (null == m_Data) 
		{
			Debug.LogError("Can't find AbnormalData ID:" + abnormalType.ToString());
			return;
		}

		InitTriggers ();
		InitHits ();
		InitEffects ();
		InitRemoveTriggers ();
		InitRemoveHits ();
		InitRemoveEffects ();
	}

	void InitTriggers()
	{
		if (m_Data.trigger_TimeInterval > PETools.PEMath.Epsilon) 
		{
			PEAT_Time time = new PEAT_Time();
			time.interval = m_Data.trigger_TimeInterval;
			m_Triggers.Add(time);
		}

		if (m_Data.trigger_BuffAdd != null) 
		{
			PEAT_Event_IntArray buffAdd = new PEAT_Event_IntArray();
			buffAdd.intValues = m_Data.trigger_BuffAdd;
			AddSaveBuffs(buffAdd.intValues);
			m_AbnormalCmpt.evtBuffAdd += buffAdd.OnIntEvent;
			m_Triggers.Add(buffAdd);
		}

		if (m_Data.trigger_ItemGet != null) 
		{
			PEAT_Event_IntArray itemAdd = new PEAT_Event_IntArray();
			itemAdd.intValues = m_Data.trigger_ItemGet;
			m_AbnormalCmpt.evtItemAdd += itemAdd.OnIntEvent;
			m_Triggers.Add(itemAdd);
		}

		if (m_Data.trigger_Damage) 
		{
			PEAT_Event damage = new PEAT_Event();
			m_AbnormalCmpt.evtDamage += damage.OnEvent;
			m_Triggers.Add(damage);
		}

		if(m_Data.trigger_InWater)
		{
			PEAT_InWater inWater = new PEAT_InWater();
			inWater.view = m_AbnormalCmpt.Entity.biologyViewCmpt;
			inWater.passenger = m_AbnormalCmpt.Entity.passengerCmpt;
			m_Triggers.Add(inWater);
		}
	}

	void InitHits()
	{
		if (null != m_Data.hit_MutexAbnormal) 
		{
			PEAH_Abnormal abnormal = new PEAH_Abnormal();
			abnormal.abnormalCmpt = m_AbnormalCmpt;
			abnormal.abnormals = m_Data.hit_MutexAbnormal;
			abnormal.abnormalExist = false;
			m_HitRates.Add(abnormal);
		}
		
		if (null != m_Data.hit_PreAbnormal) 
		{
			PEAH_Abnormal abnormal = new PEAH_Abnormal();
			abnormal.abnormalCmpt = m_AbnormalCmpt;
			abnormal.abnormals = m_Data.hit_PreAbnormal;
			abnormal.abnormalExist = true;
			m_HitRates.Add(abnormal);
		}

		if(null != m_Data.hit_BuffID)
		{
			PEAH_Buff buff = new PEAH_Buff();
			buff.buffList = m_Data.hit_BuffID;
			AddSaveBuffs(buff.buffList);
			buff.entity = m_Entity.skEntity;
			buff.buffExist = true;
			m_HitRates.Add(buff);
		}

		if (null != m_Data.hit_Attr) 
		{
			PEAH_Attr attr = new PEAH_Attr();
			attr.attrs = m_Data.hit_Attr;
			attr.entity = m_Entity;
			m_HitRates.Add(attr);
		}

		if (null != m_Data.hit_Damage) 
		{
			PEAH_Damage damage = new PEAH_Damage();
			damage.attr = m_Data.hit_Damage;
			m_AbnormalCmpt.evtDamage += damage.OnGetDamage;			
			m_HitRates.Add(damage);
		}

		if(m_Data.hit_TimeInterval > PETools.PEMath.Epsilon)
		{
			PEAH_TimeThreshold time = new PEAH_TimeThreshold();
			time.time = m_Data.hit_TimeInterval;
			m_HitRates.Add(time);
		}

		if (null != m_Data.hit_AreaTime) 
		{
			PEAH_AreaTime areaTime = new PEAH_AreaTime();
			areaTime.entity = m_Entity;
			areaTime.values = m_Data.hit_AreaTime;
			m_HitRates.Add(areaTime);
		}

		if (m_Data.hit_RainTime > PETools.PEMath.Epsilon) 
		{
			PEAH_RainTime rainTime = new PEAH_RainTime();
			rainTime.time = m_Data.hit_RainTime;
			m_HitRates.Add(rainTime);
		}

		if(m_Data.hit_HitRate > PETools.PEMath.Epsilon)
		{
			PEAH_Rate rate = new PEAH_Rate();
			rate.rate = m_Data.hit_HitRate;
			m_HitRates.Add(rate);
		}
	}
	
	void InitEffects()
	{
		if (null != m_Data.eff_BuffAddList) 
		{
			PEAE_Buff buff = new PEAE_Buff();
			buff.buffList = m_Data.eff_BuffAddList;
			AddSaveBuffs(buff.buffList);
			buff.addBuff = true;
			buff.entity = m_Entity.skEntity;
			m_Effs.Add(buff);
		}

		if (null != m_Data.eff_Anim && "0" != m_Data.eff_Anim) 
		{
			PEAE_Anim anim = new PEAE_Anim();
			string[] subStr = m_Data.eff_Anim.Split(',');
			anim.effAnim = subStr[0];
			anim.actionType = subStr.Length > 1 ? Convert.ToInt32(subStr[1]) : 1;
			anim.entity = m_Entity;
			m_Effs.Add(anim);
		}

		if (null != m_Data.eff_Camera) 
		{
			PEAE_CameraEffect cameraEffect = new PEAE_CameraEffect();
			cameraEffect.effCamera = m_Data.eff_Camera;
			cameraEffect.entity = m_Entity;
			m_AbnormalCmpt.evtAbnormalAttack += cameraEffect.OnAbnormalAttack;
			m_Effs.Add(cameraEffect);
		}

		if (null != m_Data.eff_AbnormalRemove) 
		{
			PEAE_Abnormal abnormal = new PEAE_Abnormal();
			abnormal.abnormalCmpt = m_AbnormalCmpt;
			abnormal.abnormalType = m_Data.eff_AbnormalRemove;
			abnormal.addAbnormal = false;
			m_Effs.Add(abnormal);
		}

		if(null != m_Data.eff_Particles)
		{
			PEAE_ParticleEffect particle = new PEAE_ParticleEffect();
			particle.effectID = m_Data.eff_Particles;
			particle.entity = m_Entity;
			m_Effs.Add(particle);
		}

		if(Color.black != m_Data.eff_SkinColor)
		{
			PEAE_SkinColor skinColor = new PEAE_SkinColor();
			skinColor.avatar = m_Entity.biologyViewCmpt as AvatarCmpt;
			skinColor.color = m_Data.eff_SkinColor;
			m_Effs.Add(skinColor);
		}

		if(null != m_Data.eff_BodyWeight)
		{
			PEAE_BodyWeight bodyWeight = new PEAE_BodyWeight();
			bodyWeight.avatar = m_Entity.biologyViewCmpt as AvatarCmpt;
			bodyWeight.datas = m_Data.eff_BodyWeight;
			m_Effs.Add(bodyWeight);
		}
	}

	void InitRemoveTriggers()
	{
		m_EndImm = m_Data.rt_Immediate;
		if (m_EndImm) 
			return;

		if (m_Data.rt_TimeInterval > PETools.PEMath.Epsilon) 
		{
			PEAT_Time time = new PEAT_Time();
			time.interval = m_Data.rt_TimeInterval;
			m_RemoveTriggers.Add(time);
		}

		if(null != m_Data.rt_BuffRemove)
		{
			PEAT_Event_IntArray buffRemove = new PEAT_Event_IntArray();
			buffRemove.intValues = m_Data.rt_BuffRemove;
			m_AbnormalCmpt.evtBuffRemove += buffRemove.OnIntEvent;
			m_RemoveTriggers.Add(buffRemove);
		}

		if (m_Data.rt_EffectEnd) 
		{
			PEAT_EffectEnd effectEnd = new PEAT_EffectEnd();
			effectEnd.abnormal = this;
			m_RemoveTriggers.Add(effectEnd);
		}

		if(m_Data.rt_OutsideWater)
		{
			PEAT_Event outWater = new PEAT_Event();
			m_AbnormalCmpt.evtOutWater += outWater.OnEvent;
			m_RemoveTriggers.Add(outWater);
		}
	}
	
	void InitRemoveHits()
	{
		if (null != m_Data.rh_BuffList) 
		{
			PEAH_Buff buff = new PEAH_Buff();
			buff.buffList = m_Data.hit_BuffID;
			buff.entity = m_Entity.skEntity;
			buff.buffExist = false;
			m_RemoveHits.Add(buff);
		}

		if (null != m_Data.rh_Attr) 
		{
			PEAH_Attr attr = new PEAH_Attr();
			attr.attrs = m_Data.rh_Attr;
			attr.entity = m_Entity;
			m_RemoveHits.Add(attr);
		}
	}
	
	void InitRemoveEffects()
	{
		if (null != m_Data.re_BuffRemove) 
		{
			PEAE_Buff buff = new PEAE_Buff();
			buff.entity = m_Entity.skEntity;
			buff.buffList = m_Data.re_BuffRemove;
			buff.addBuff = false;
			m_RemoveEffs.Add(buff);
		}

		if (null != m_Data.re_BuffAdd) 
		{
			PEAE_Buff buff = new PEAE_Buff();
			buff.entity = m_Entity.skEntity;
			buff.buffList = m_Data.re_BuffAdd;
			buff.addBuff = true;
			m_RemoveEffs.Add(buff);
		}

		if (null != m_Data.re_AbnormalAdd) 
		{
			PEAE_Abnormal abnormal = new PEAE_Abnormal();
			abnormal.abnormalType = m_Data.re_AbnormalAdd;
			abnormal.addAbnormal = true;
			abnormal.abnormalCmpt = m_AbnormalCmpt;
			m_RemoveEffs.Add(abnormal);
		}

		if(null != m_Data.re_Anim && "0" != m_Data.re_Anim)
		{
			PEAE_Anim anim = new PEAE_Anim();
			string[] subStr = m_Data.eff_Anim.Split(',');
			anim.effAnim = subStr[0];
			anim.actionType = subStr.Length > 1 ? Convert.ToInt32(subStr[1]) : 1;
			anim.entity = m_Entity;
			m_RemoveEffs.Add(anim);
		}

		if(null != m_Data.re_Camera)
		{
			PEAE_CameraEffect cameraEffect = new PEAE_CameraEffect();
			cameraEffect.effCamera = m_Data.re_Camera;
			cameraEffect.entity = m_Entity;
			m_AbnormalCmpt.evtAbnormalAttack += cameraEffect.OnAbnormalAttack;
			m_RemoveEffs.Add(cameraEffect);
		}

		if(null != m_Data.re_Particles)
		{
			PEAE_ParticleEffect particle = new PEAE_ParticleEffect();
			particle.effectID = m_Data.re_Particles;
			particle.entity = m_Entity;
			m_RemoveEffs.Add(particle);
		}
	}

	void AddSaveBuffs(int[] buffs)
	{
		for(int i = 0; i < buffs.Length; ++i)
			if(!m_NeedSaveBuffList.Contains(buffs[i]))
				m_NeedSaveBuffList.Add(buffs[i]);
	}
}