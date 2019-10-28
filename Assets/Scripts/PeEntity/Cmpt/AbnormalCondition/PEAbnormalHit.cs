using UnityEngine;
using System.Collections;
using Pathea;
using SkillSystem;

public class PEAbnormalHit
{
	public bool preHit{ get; set; }
	public virtual float HitRate(){ return 0f; }
	public virtual void Update() { }
	public virtual void Clear() { }
}

public class PEAH_Abnormal : PEAbnormalHit
{
	public PEAbnormalType[] abnormals { get; set; }
	public AbnormalConditionCmpt abnormalCmpt { get; set; }
	public bool abnormalExist{ get; set; }

	public override float HitRate ()
	{
		for (int i = 0; i < abnormals.Length; ++i) 
			if(abnormalCmpt.CheckAbnormalCondition(abnormals[i]))
				return abnormalExist?1f:0f;
		return abnormalExist?0f:1f;
	}
}

public class PEAH_Buff : PEAbnormalHit
{
	public SkEntity entity{ get; set; }
	public int[] buffList{ get; set; }
	public bool buffExist{ get; set; }
	public override float HitRate ()
	{
		for (int i = 0; i < buffList.Length; ++i) 
			if(buffExist != (null != entity.GetSkBuffInst(buffList[i])))
				return 0;
		return 1f;
	}
}

public class PEAH_Attr : PEAbnormalHit
{
	public PeEntity entity{ get; set; }
	public AbnormalData.HitAttr[] attrs{ get; set; }
	public override float HitRate ()
	{
		float retRate = 1f;
		for (int i = 0; i < attrs.Length; ++i) 
			retRate *= attrs [i].GetRate (entity);
		return retRate;
	}
}

public class PEAH_Damage : PEAbnormalHit
{
	public AbnormalData.HitAttr attr{ get; set; }
	float m_Damage;
	public override float HitRate ()
	{
		return attr.GetRate(m_Damage);
	}

	public void OnGetDamage(float damage)
	{
		m_Damage = damage;
	}
}

public class PEAH_TimeThreshold : PEAbnormalHit
{
	public float time{ get; set; }
	protected float m_ElapseTime;

	public override float HitRate ()
	{
		return (m_ElapseTime >= time)?1f:0;
	}

	public override void Update ()
	{
		m_ElapseTime += (preHit ? 1f : -1f) * Time.deltaTime;
		m_ElapseTime = Mathf.Clamp (m_ElapseTime, 0, 2f * time);
	}

	public override void Clear ()
	{
		m_ElapseTime = 0;
		base.Clear ();
	}
}

public class PEAH_AreaTime : PEAbnormalHit
{
	public PeEntity entity{ get; set; }
	public float[] values{ get; set; }
	protected float m_ElapseTime;

	protected float countTime;
	protected float nextCountTime;

	public override void Update ()
	{
		countTime += Time.deltaTime;
		if(Time.time > nextCountTime)
		{
			m_ElapseTime += (IsInArea ? 1f : -1f) * countTime;
			m_ElapseTime = Mathf.Clamp (m_ElapseTime, 0, 2f * values[0]);
			countTime = 0;
			nextCountTime = Time.time + 2 * Random.value;
		}
	}
	
	public override void Clear ()
	{
		m_ElapseTime = 0;
		base.Clear ();
	}
	
	public override float HitRate ()
	{
		return (m_ElapseTime >= values[0])?1f:0;
	}

	protected virtual bool IsInArea
	{
		get
		{
			int currentID = 0;
			if(PeGameMgr.IsCustom)
				return false;
			if(PeGameMgr.IsStory)
			{
				if(null != PeMappingMgr.Instance && SingleGameStory.curType == SingleGameStory.StoryScene.MainLand)
					currentID = PeMappingMgr.Instance.GetAiSpawnMapId(new Vector2(entity.position.x, entity.position.z));
				else
					return false;
			}
			else
				currentID = AiUtil.GetMapID(entity.position);
			for(int i = 1; i < values.Length; ++i)
				if(currentID == values[i])
					return true;
			return false;
		}
	}
}

public class PEAH_AreaTimeBetween : PEAH_AreaTime
{
	public override void Update ()
	{
		countTime += Time.deltaTime;
		if(Time.time > nextCountTime)
		{
			m_ElapseTime += (IsInArea ? 1f : -1f) * countTime;
			m_ElapseTime = Mathf.Clamp (m_ElapseTime, 0, 2f * values[1]);
			countTime = 0;
			nextCountTime = Time.time + 2 * Random.value;
		}
	}

	public override float HitRate ()
	{
		return (m_ElapseTime >= values[0] && m_ElapseTime <= values[1])?1f:0;
	}

	protected override bool IsInArea 
	{
		get
		{
			int currentID = 0;
			if(PeGameMgr.IsCustom)
				return false;
			if(PeGameMgr.IsStory)
			{
				if(null != PeMappingMgr.Instance && SingleGameStory.curType == SingleGameStory.StoryScene.MainLand)
					currentID = PeMappingMgr.Instance.GetAiSpawnMapId(new Vector2(entity.position.x, entity.position.z));
				else
					return false;
			}
			else
				currentID = AiUtil.GetMapID(entity.position);
			for(int i = 2; i < values.Length; ++i)
				if(currentID == values[i])
					return true;
			return false;
		}
	}
}

public class PEAH_RainTime : PEAH_TimeThreshold
{
	public override void Update ()
	{
		m_ElapseTime += (PeEnv.isRain ? 1f : -1f) * Time.deltaTime;
		m_ElapseTime = Mathf.Clamp (m_ElapseTime, 0, 2f * time);
	}

	public override void Clear ()
	{
		m_ElapseTime = 0;
		base.Clear ();
	}
}

public class PEAH_Rate : PEAbnormalHit
{
	public float rate{ get; set; }

	public override float HitRate ()
	{
		return rate;
	}
}