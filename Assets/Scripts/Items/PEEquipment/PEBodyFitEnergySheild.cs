using UnityEngine;
using Pathea;
using Pathea.Effect;

public class PEBodyFitEnergySheild : PEEnergySheildLogic 
{
	[SerializeField] PEDefenceTrigger m_SubDefenceTrigger;

	[SerializeField] int m_StartEffectID;
	
	[SerializeField] float m_StartEffectDelayTime;

	[SerializeField] int m_EndEffectID;
	
	[SerializeField] float m_EndEffectDelayTime;

	ControllableEffect m_StartEffect;

	BiologyViewCmpt m_View;

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_View = m_Entity.biologyViewCmpt;
		SyncDefenceTrigger();
	}

	protected override void Update ()
	{
		base.Update ();

		UpdateEffectState();
		
		if(m_Active && enCurrent < PETools.PEMath.Epsilon)
			DeactiveSheild();

		if(m_Entity.IsDeath())
			DeactiveSheild();
	}

	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		if(null != m_View && null != m_View.defenceTrigger)
			m_View.defenceTrigger.active = true;
	}

	public override void OnModelRebuild ()
	{
		SyncDefenceTrigger();
		SetDefenceState(m_Active);
	}
	
	public override void ActiveSheild(bool fullCharge = false)
	{
		base.ActiveSheild(fullCharge);
		SetDefenceState(true);
		Invoke("PlayStartEffect", m_StartEffectDelayTime);
	}
	
	public override void DeactiveSheild()
	{
		base.DeactiveSheild();
		SetDefenceState(false);
		if(IsInvoking("PlayStartEffect"))
			CancelInvoke("PlayStartEffect");
		if(null != m_StartEffect)
		{
			m_StartEffect.Destory();
			m_StartEffect = null;
		}
		Invoke("PlayEndEffect", m_EndEffectDelayTime);
	}

	void SyncDefenceTrigger()
	{
		if(null != m_SubDefenceTrigger && null != m_View && null != m_View.defenceTrigger)
			m_SubDefenceTrigger.SyncBone(m_View.defenceTrigger);
	}
	
	void SetDefenceState(bool active)
	{
		if(null != m_SubDefenceTrigger)
		{
			m_SubDefenceTrigger.active = active;
			if(null != m_View && null != m_View.defenceTrigger)
				m_View.defenceTrigger.active = !active;
		}
	}

	void UpdateEffectState()
	{
		if(null != m_StartEffect && null != m_View && null != m_View.modelTrans)
			m_StartEffect.active = m_View.modelTrans.gameObject.activeSelf;
	}

	void OnDestroy()
	{
		if(null != m_StartEffect)
		{
			m_StartEffect.Destory();
			m_StartEffect = null;
		}
	}

	void PlayStartEffect()
	{
		if(null != m_View && null != m_View.modelTrans && 0 != m_StartEffectID)
			m_StartEffect = new ControllableEffect(m_StartEffectID, m_View.modelTrans);
	}

	void PlayEndEffect()
	{
		if(null != m_View && null != m_View.modelTrans && 0 != m_EndEffectID)
			EffectBuilder.Instance.Register(m_EndEffectID, null, m_View.modelTrans);
	}
}
