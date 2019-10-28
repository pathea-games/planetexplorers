using UnityEngine;
using System.Collections;
using Pathea;

public class PEBatteryLogic : PEEquipmentLogic 
{
	ItemAsset.Energy m_Energy;
	
	float m_LastNetValue;
	UTimer m_Time;
	
	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Energy = m_ItemObj.GetCmpt<ItemAsset.Energy>();
		if(null != m_Energy)
		{
			m_Entity.SetAttribute(AttribType.EnergyMax, m_Energy.valueMax);
			m_Entity.SetAttribute(AttribType.Energy, m_Energy.floatValue.current);
		}
		m_LastNetValue = m_Entity.GetAttribute(AttribType.Energy);
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = GameConfig.NetUpdateInterval;
	}
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		if(null != m_Energy)
		{
			m_Energy.SetMax(m_Entity.GetAttribute(AttribType.EnergyMax));
			m_Energy.floatValue.current = m_Entity.GetAttribute(AttribType.Energy);
			m_Entity.SetAttribute(AttribType.EnergyMax, 0);
			m_Entity.SetAttribute(AttribType.Energy, 0);
		}
	}
	
	void Update()
	{
		if(null != m_Energy)
			m_Energy.floatValue.current = m_Entity.GetAttribute(AttribType.Energy);
		
		if(GameConfig.IsMultiMode)
		{
			if(null != m_Time)
			{
				m_Time.Update(Time.deltaTime);
				if(m_Time.Second < 0 && Mathf.Abs(m_LastNetValue - m_Entity.GetAttribute(AttribType.Energy)) > GameConfig.NetMinUpdateValue)
				{
					m_Time.Second = GameConfig.NetUpdateInterval;
					m_LastNetValue = m_Entity.GetAttribute(AttribType.Energy);
					//SendMsg
					PlayerNetwork.mainPlayer.RequestBatteryEnergyReload(m_Entity.Id, m_ItemObj.instanceId, m_LastNetValue);
				}
			}
		}
	}
}
