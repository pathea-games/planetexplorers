using UnityEngine;
using System.Collections.Generic;
using Pathea;
using Pathea.Effect;
using SkillSystem;

public class PEAbnormalEff
{
	public virtual void Do () { }
	public virtual void Update () { }
	public virtual void End() { }
	public virtual bool effectEnd { get{ return true; } }
}

public class PEAE_Buff : PEAbnormalEff
{
	public SkEntity entity{ get; set; }
	public int[] buffList{ get; set; }
	public bool addBuff{ get; set; }

	public override void Do ()
	{
        //lw:2017.4.6 crash
		if(null == entity && buffList != null)
			return;
		for (int i = 0; i < buffList.Length; ++i) 
		{
			if(addBuff)
				SkEntity.MountBuff (entity, buffList[i], new List<int> (), new List<float> ());
			else
				entity.CancelBuffById (buffList[i]);
		}
	}

	public override void End ()
	{
		if(null == entity)
			return;
		for (int i = 0; i < buffList.Length; ++i) 
		{
			if(addBuff)
				entity.CancelBuffById (buffList[i]);
			else
				SkEntity.MountBuff (entity, buffList[i], new List<int> (), new List<float> ());
		}
	}
}

public class PEAE_Anim : PEAbnormalEff
{
	public PeEntity entity{ get; set; }
	public string effAnim{ get; set; }
	public int actionType{ get; set; }

	float original;

	float nextRetryTime;

	public override void Do ()
	{
		if("0" == effAnim || null == entity || null == entity.motionMgr)
			return;
		PEActionParamS param = PEActionParamS.param;
		param.str = effAnim;
		if(0 == actionType)
			entity.motionMgr.DoAction(PEActionType.Leisure, param);
		else
			entity.motionMgr.DoAction(PEActionType.Abnormal, param);
		nextRetryTime = Time.time + Random.Range(60f, 90f);
	}

	public override void End ()
	{
		if(null == entity || null == entity.motionMgr) return;
		entity.motionMgr.EndAction(PEActionType.Leisure);
	}

	public override void Update ()
	{
		if(Time.time >  nextRetryTime)
			Do ();
	}
}

public class PEAE_Abnormal : PEAbnormalEff
{
	public PEAbnormalType[] abnormalType{ get; set; }
	public bool addAbnormal{ get; set; }
	public AbnormalConditionCmpt abnormalCmpt{ get; set; }

	public override void Do ()
	{
		for(int i = 0; i < abnormalType.Length; ++i)
		{
			if (addAbnormal)
				abnormalCmpt.StartAbnormalCondition (abnormalType[i]);
			else
				abnormalCmpt.EndAbnormalCondition (abnormalType[i]);
		}
	}
}

public class PEAE_CameraEffect : PEAbnormalEff
{
	public PeEntity entity{ get; set; }
	public AbnormalData.EffCamera effCamera{ get; set; }
	Vector3 effectPos{ get; set; }
	float effectStrength{ get; set; }
	float effectTime{ get; set; }
	float m_ElapseTime;
	public override void Do ()
	{
		if(null == entity) return;

		if(entity != MainPlayer.Instance.entity)
		{
			m_ElapseTime = effectTime;
			return;
		}
		m_ElapseTime = 0;
		switch(effCamera.type)
		{
		case 0:
			PeCameraImageEffect.ScreenMask(Mathf.RoundToInt(effectStrength), true, effectTime);
			break;
		case 1:
			PeCameraImageEffect.SetDizzyStrength(effCamera.value * effectStrength);
			break;
		case 2:
			PeCameraImageEffect.FlashlightExplode(effCamera.value * effectStrength);
			break;
		case 3:
			PeCameraImageEffect.SetFoodPoisonStrength(effCamera.value);
			break;
		case 4:
			PeCameraImageEffect.SetInjuredPoisonStrength(effCamera.value);
			break;
		case 5:
			PeCameraImageEffect.SetGRVInfestStrength(effCamera.value);
			break;
		}
	}

	public override void End ()
	{
		switch(effCamera.type)
		{
		case 0:
			PeCameraImageEffect.ScreenMask(0, false);
			break;
		case 1:
			PeCameraImageEffect.SetDizzyStrength(0);
			break;
		case 2:
			PeCameraImageEffect.FlashlightExplode(0);
			break;
		case 3:
			PeCameraImageEffect.SetFoodPoisonStrength(0);
			break;
		case 4:
			PeCameraImageEffect.SetInjuredPoisonStrength(0);
			break;
		case 5:
			PeCameraImageEffect.SetGRVInfestStrength(0);
			break;
		}
	}

	public void OnAbnormalAttack(PEAbnormalAttack attack, Vector3 effectPos)
	{
		bool typeMatch = false;
		switch(effCamera.type)
		{
		case 0:
			typeMatch = attack.type == PEAbnormalAttackType.BlurredVision;
			break;
		case 1:
			typeMatch = attack.type == PEAbnormalAttackType.Dazzling;
			break;
		case 2:
			typeMatch = attack.type == PEAbnormalAttackType.Flashlight;
			break;
		}
		if(typeMatch)
		{
			this.effectPos = effectPos;
			effectStrength = attack.strength;
			effectTime = attack.duration;
			switch(effCamera.type)
			{
			case 1:
			case 2:
				if(attack.radius > PETools.PEMath.Epsilon)
					effectStrength *= 0.5f + 0.5f * (entity.position - effectPos).magnitude / attack.radius;
				break;
			}
		}
	}

	public override void Update ()
	{
		m_ElapseTime += Time.deltaTime;
		switch(effCamera.type)
		{
		case 1:
			if(effectTime > 0 && m_ElapseTime <= effectTime)
				PeCameraImageEffect.SetDizzyStrength(effCamera.value * effectStrength * (1f - m_ElapseTime / effectTime));
			break;
		}
	}

	public override bool effectEnd {
		get 
		{
			return m_ElapseTime >= effectTime;
		}
	}
}

public class PEAE_ParticleEffect : PEAbnormalEff
{
	public int[] effectID { get; set; }
	public PeEntity entity { get; set; }
	List<GameObject> mEffectObj = new List<GameObject>();
	bool m_PlayEffect;
	public override void Do ()
	{
		Clear();
		for(int i = 0; i < effectID.Length; ++i)
		{
			EffectBuilder.EffectRequest request = EffectBuilder.Instance.Register(effectID[i], null, entity.biologyViewCmpt.modelTrans);
			request.SpawnEvent += OnSpawn;
		}
		m_PlayEffect = true;
	}

	public override void End ()
	{
		Clear();
	}
	public override bool effectEnd 
	{
		get 
		{
			for(int i = mEffectObj.Count - 1; i >= 0; --i)
			{
				if(null != mEffectObj[i])
				{
					return false;
				}
				else
					mEffectObj.RemoveAt(i);
			}
			return true;
		}
	}

	void Clear()
	{
		for(int i = 0; i < mEffectObj.Count; ++i)
			if(null != mEffectObj[i])
				GameObject.Destroy(mEffectObj[i]);
		
		mEffectObj.Clear();
		m_PlayEffect = false;
	}

	void OnSpawn(GameObject obj)
	{
		if(m_PlayEffect)
			mEffectObj.Add(obj);
		else
			GameObject.Destroy(obj);
	}
}

public class PEAE_SkinColor : PEAbnormalEff
{
	public AvatarCmpt avatar { get; set; }
	public Color color { get; set; }
	public override void Do ()
	{
		if(null != avatar)
		{
			avatar.apperaData.subSkinColor = color;
			avatar.UpdateSmr();
		}
	}

	public override void End ()
	{
		if(null != avatar)
		{
			avatar.apperaData.subSkinColor = Color.black;
			avatar.UpdateSmr();
		}
	}
}

public class PEAE_BodyWeight : PEAbnormalEff
{
	public AvatarCmpt avatar { get; set; }
	public AbnormalData.ThresholdData[] datas { get; set; }
	public override void Do ()
	{
		if(null != avatar)
		{
			for(int i = 0; i < datas.Length; ++i)
				avatar.apperaData.subBodyWeight[datas[i].type] = datas[i].threshold;
			avatar.UpdateSmr();
		}
	}

	public override void End ()
	{
		if(null != avatar)
		{
			for(int i = 0; i < datas.Length; ++i)
				avatar.apperaData.subBodyWeight[datas[i].type] = 0;
			avatar.UpdateSmr();
		}
	}
}

public class PEAE_HumanAudio : PEAbnormalEff
{
	public int audioID{ get; set; }
	public int sex{ get; set; }
	public PeEntity entity { get; set; }

	public override void Do ()
	{
		if(null == entity || (1 != sex && 2 != sex))
			return;
		int[] soundIDs = HumanSoundData.GetSoundID(audioID, sex);
		for(int i = 0; i < soundIDs.Length; ++i)
			AudioManager.instance.Create(entity.position, soundIDs[i]);
	}
}

public class PEAE_Content : PEAbnormalEff
{
	public int[] contentIDs{ get; set; }
	public EntityInfoCmpt entityInfo { get; set; }

	public override void Do ()
	{
		if(null != entityInfo && null != entityInfo.faceTex)
			PeTipMsg.Register(PELocalization.GetString(contentIDs[Random.Range(0, contentIDs.Length)]), entityInfo.faceTex, PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
	}
}
