using UnityEngine;
using Pathea;

public class PECrusher : PEDigTool , IHeavyEquipment
{
	public Pathea.MoveStyle baseMoveStyle { get { return m_HandChangeAttr.m_BaseMoveStyle; } }

	[SerializeField] Animation m_Anim;
	
	[SerializeField] float m_EnergyCostSpeed = 5f;

	const string AnimName = "Running";

	EquipmentActiveEffect m_Effect;
	bool m_Active;
	
	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Effect = GetComponent<EquipmentActiveEffect>();
		if(null != m_Anim)
			m_Anim.Stop();
		if(null != m_Indicator)
			m_Indicator.m_AddHeight = 1;
	}

	public override bool canHoldEquipment 
	{
		get 
		{
			if(null == m_Entity || m_Entity.GetAttribute(AttribType.Energy) < m_EnergyCostSpeed)
				return false;
			return base.canHoldEquipment;
		}
	}

	public override void SetActiveState (bool active)
	{
		base.SetActiveState (active);
		m_Active = active;
		if(null != m_Effect)
			m_Effect.SetActiveState(active);

		if(active)
		{
			if(null != m_Anim && !m_Anim.IsPlaying(AnimName))
				m_Anim.CrossFade(AnimName);
		}
		else
		{
			if(null != m_Anim)
				m_Anim.Stop(AnimName);
		}
	}
	
	public void UpdateEnCost()
	{
		if(null == m_Entity || !m_Active)
			return;
		float curEn = m_Entity.GetAttribute(AttribType.Energy);
		curEn -= Time.deltaTime * m_EnergyCostSpeed;
		if(curEn <= 0)
		{
			curEn = 0;
			EndAction();
		}
		m_Entity.SetAttribute(AttribType.Energy, curEn);
	}
	
	void EndAction()
	{
		if(null == m_Entity || null == m_Entity.motionMgr)
			return;
		m_Entity.motionMgr.EndImmediately(PEActionType.Dig);
		m_Entity.motionMgr.EndAction(m_HandChangeAttr.m_ActiveActionType);
	}
}
