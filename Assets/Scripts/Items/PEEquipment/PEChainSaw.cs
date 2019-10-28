using UnityEngine;
using System.Collections;
using Pathea;

public class PEChainSaw : PEAxe, IHeavyEquipment
{
	public Pathea.MoveStyle baseMoveStyle { get { return m_HandChangeAttr.m_BaseMoveStyle; } }

	[SerializeField] Renderer m_Renderer;

	[SerializeField] float m_TexSpeed = 3f;

	[SerializeField] float m_EnergyCostSpeed = 5f;

	bool m_Active = false;

	Vector2 m_TexOffset = Vector2.zero;

	EquipmentActiveEffect m_Effect;

	#region IActiveableEquipment implementation
	public string activeAnim { get { return m_HandChangeAttr.m_PutOnAnim; }	}

	public string deactiveAnim { get { return m_HandChangeAttr.m_PutOffAnim; } }

	public PEActionMask mask { get { return m_HandChangeAttr.m_HoldActionMask; } }

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Effect = GetComponent<EquipmentActiveEffect>();
	}

	public override void SetActiveState(bool active)
	{
		m_Active = active;
		if(null != m_Effect)
			m_Effect.SetActiveState(active);
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
	#endregion

	void Update()
	{
		if(m_Active)
		{
			UpdateChainUV();
			UpdateEnCost();
		}
	}

	void UpdateChainUV()
	{
		m_TexOffset += m_TexSpeed * Time.deltaTime * Vector2.right;
		m_TexOffset.x = m_TexOffset.x%1f;
		m_Renderer.materials[1].SetTextureOffset("_MainTex", m_TexOffset);
	}

	void UpdateEnCost()
	{
		if(null != m_Entity)
		{
			float curEn = m_Entity.GetAttribute(AttribType.Energy);
			curEn -= Time.deltaTime * m_EnergyCostSpeed;
			if(curEn <= 0)
			{
				curEn = 0;
				EndAction();
			}
			m_Entity.SetAttribute(AttribType.Energy, curEn);
		}
	}

	void EndAction()
	{
		if(null == m_Entity.motionMgr)
			return;
		m_Entity.motionMgr.EndImmediately(PEActionType.Fell);
		m_Entity.motionMgr.EndAction(m_HandChangeAttr.m_ActiveActionType);
	}
}
