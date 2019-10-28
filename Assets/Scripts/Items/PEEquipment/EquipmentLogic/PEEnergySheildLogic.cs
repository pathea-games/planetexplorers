using UnityEngine;
using System.Collections;
using Pathea;

public class PEEnergySheildLogic : PEEquipmentLogic, IRechargeableEquipment
{
	public float m_RechargeEnergySpeed;
	public float m_RechargeDelay;
	public float m_MaxEnergy;
	
	public bool m_Active = true;
	
	ItemAsset.Energy m_EnergyAttr;

	float m_LastNetValue;
	UTimer m_Time;
	
	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Equip.SetEnergySheild(this);
		m_EnergyAttr = m_ItemObj.GetCmpt<ItemAsset.Energy>();
		
		if(null != m_EnergyAttr)
		{
			m_Entity.SetAttribute(AttribType.Shield, m_EnergyAttr.energy.current);
			m_EnergyAttr.SetMax(m_MaxEnergy);
		}
		m_Entity.SetAttribute(AttribType.ShieldMax, m_MaxEnergy);
		
		lastUsedTime = Time.time;
		
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = GameConfig.NetUpdateInterval;
		m_LastNetValue = enCurrent;

		if(!m_Active)
			DeactiveSheild();
	}
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		m_Entity.SetAttribute(AttribType.Shield, 0);
		m_Entity.SetAttribute(AttribType.ShieldMax, 0);
	}
	
	public float enMax { get { return m_MaxEnergy; } }
	
	public float enCurrent 
	{
		get
		{
			return (null != m_Entity) ? m_Entity.GetAttribute(AttribType.Shield) : 0f;
		}
		set 
		{
			if(null != m_EnergyAttr)
				m_EnergyAttr.energy.current = value;
			if(null != m_Entity)
				m_Entity.SetAttribute(AttribType.Shield, value);
		}
	}
	
	public float rechargeSpeed { get { return m_RechargeEnergySpeed; } }
	
	public float lastUsedTime {	get; set; }
	
	public float rechargeDelay { get { return m_RechargeDelay; } }
	
	protected virtual void Update()
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
					//					PlayerNetwork.MainPlayer.RequestGunEnergyReload(m_Entity.Id, m_ItemObj.instanceId, enCurrent);
				}
			}
		}
	}
	
	public virtual void ActiveSheild(bool fullCharge = false)
	{
		m_Active = true;
		if(fullCharge)
			enCurrent = enMax;
	}
	
	public virtual void DeactiveSheild()
	{
		m_Active = false;
		enCurrent = 0;
	}
}
