using UnityEngine;
using Mono.Data.SqliteClient;
using System.Collections.Generic;
using Pathea;

public class PEAbnormalNoticeData
{
	public float 	trigger_TimeInterval;
	public int[] 	trigger_AbnormalHit;

	public float[]	hit_AreaTime;
	public float	hit_HitRate;
	public AbnormalData.HitAttr[] hit_Attr;

	public int 		eff_HumanAudio;
	public int[]	eff_Contents;

	static PEAbnormalNoticeData[] g_Datas;
	public static PEAbnormalNoticeData[] datas{ get{ return g_Datas; } }

	public static void LoadData()
	{
		List<PEAbnormalNoticeData> datas = new List<PEAbnormalNoticeData>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AbnormalNotice");
		while (reader.Read())
		{
			PEAbnormalNoticeData data = new PEAbnormalNoticeData();
			
			data.trigger_TimeInterval = PETools.Db.GetFloat(reader, "Trigger_Time");
			data.trigger_AbnormalHit = PETools.Db.GetIntArray(reader, "Trigger_Abnormal");

			data.hit_Attr = AbnormalData.HitAttr.GetHitAttrArray(reader, "Hit_Attr");
			data.hit_AreaTime = PETools.Db.GetFloatArray(reader, "Hit_AreaTime");
			data.hit_HitRate = PETools.Db.GetFloat(reader, "Hit_Rate");
			
			data.eff_HumanAudio = PETools.Db.GetInt(reader, "Eff_HumanAudio");
			data.eff_Contents = PETools.Db.GetIntArray(reader, "Eff_Content");
			
			datas.Add(data);
		}
		g_Datas = datas.ToArray();
	}
}

public class PEAbnormalNotice
{
	List<PEAbnormalTrigger> m_Triggers = new List<PEAbnormalTrigger>();
	List<PEAbnormalHit> m_HitRates = new List<PEAbnormalHit>();
	List<PEAbnormalEff> m_Effs = new List<PEAbnormalEff>();

	PeEntity m_Entity;
	PEAbnormalNoticeData m_Data;

	public void Init(PeEntity entity, PEAbnormalNoticeData data)
	{
		m_Entity = entity;
		m_Data = data;
		InitTriggers ();
		InitHits ();
		InitEffects ();
	}

	public void Update()
	{
		bool triggerHit = m_Triggers.Count > 0;
		for(int i = 0; i < m_Triggers.Count; ++i)
		{
			triggerHit = triggerHit && m_Triggers[i].Hit();
			m_Triggers[i].Update();
		}
		
		if(triggerHit && !m_Entity.IsDeath())
			CheckHit();
		
		for(int i = 0; i < m_HitRates.Count; ++i)
			m_HitRates[i].Update();
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
			ApplyEffect();
	}
	
	void ApplyEffect()
	{
		for(int i = 0; i < m_Effs.Count; ++i)
			m_Effs[i].Do();
		for(int i = 0; i < m_Triggers.Count; ++i)
			m_Triggers.Clear();
	}

	void InitTriggers()
	{
		if (m_Data.trigger_TimeInterval > PETools.PEMath.Epsilon) 
		{
			PEAT_Time time = new PEAT_Time();
			time.interval = m_Data.trigger_TimeInterval;
			m_Triggers.Add(time);
		}

		if (null != m_Data.trigger_AbnormalHit && null != m_Entity.Alnormal) 
		{
			PEAT_AbnormalHit abnormalHit = new PEAT_AbnormalHit();
			abnormalHit.hitAbnormals = m_Data.trigger_AbnormalHit;
			m_Entity.Alnormal.evtStart += abnormalHit.OnHitAbnormal;
			m_Triggers.Add(abnormalHit);
		}
	}
	
	void InitHits()
	{
		if (null != m_Data.hit_Attr) 
		{
			PEAH_Attr attr = new PEAH_Attr();
			attr.attrs = m_Data.hit_Attr;
			attr.entity = m_Entity;
			m_HitRates.Add(attr);
		}

		if (null != m_Data.hit_AreaTime) 
		{
			PEAH_AreaTime areaTime = new PEAH_AreaTime();
			areaTime.entity = m_Entity;
			areaTime.values = m_Data.hit_AreaTime;
			m_HitRates.Add(areaTime);
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
		if (0 != m_Data.eff_HumanAudio) 
		{
			PEAE_HumanAudio audio = new PEAE_HumanAudio();
			audio.sex = null != m_Entity.commonCmpt ? (int)m_Entity.commonCmpt.sex : 1;
			audio.audioID = m_Data.eff_HumanAudio;
			audio.entity = m_Entity;
			m_Effs.Add(audio);
		}
		
		if (null != m_Data.eff_Contents) 
		{
			PEAE_Content content = new PEAE_Content();
			content.contentIDs = m_Data.eff_Contents;
			content.entityInfo = m_Entity.enityInfoCmpt;
			m_Effs.Add(content);
		}
	}
}