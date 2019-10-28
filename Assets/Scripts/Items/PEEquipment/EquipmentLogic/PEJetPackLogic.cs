using UnityEngine;
using System.Collections;

public class PEJetPackLogic : PEEquipmentLogic, IRechargeableEquipment 
{
	public float		m_BoostPowerUp = 12f;
	public float		m_MaxUpSpeed = 7f;
	public float		m_BoostHorizonalSpeed = 2f;
	public float		m_EnergyMax = 50f;
	public float		m_EnergyThreshold = 20;
	public float		m_CostSpeed = 5f;
	public float		m_RechargeSpeed = 2f;
	public float		m_RechargeDelay = 2f;

	float 				m_LastNetValue;
	UTimer 				m_Time;
	ItemAsset.JetPkg	m_ItemAttr;

	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Equip.SetJetPackLogic(this);
		m_ItemAttr = itemObj.GetCmpt<ItemAsset.JetPkg>();
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = GameConfig.NetUpdateInterval;
		m_LastNetValue = enCurrent;
	} 
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		m_Equip.SetJetPackLogic(null);
	}
	
	void Update()
	{
		if(GameConfig.IsMultiMode)
		{
			if(null != m_Time)
			{
				m_Time.Update(Time.deltaTime);
				if(m_Time.Second < 0 && Mathf.Abs(m_LastNetValue - enCurrent) > GameConfig.NetMinUpdateValue)
				{
					m_Time.Second = GameConfig.NetUpdateInterval;
					m_LastNetValue = enCurrent;
					//SendMsg
					PlayerNetwork.mainPlayer.RequestJetPackEnergyReload(m_Entity.Id, m_ItemObj.instanceId, m_LastNetValue);
				}
			}
		}
	}

	#region IRechargeable implementation
	
	public float enMax { get { return m_EnergyMax;	} }
	
	public float enCurrent
	{
		get{ return (null != m_ItemAttr) ? m_ItemAttr.energy : 0f; }
		set{ if(null != m_ItemAttr) m_ItemAttr.energy = value; }
	}
	
	public float rechargeSpeed { get { return m_RechargeSpeed; } }
	
	public float lastUsedTime {	get; set; }
	
	public float rechargeDelay { get { return m_RechargeDelay; } }
	#endregion
}
