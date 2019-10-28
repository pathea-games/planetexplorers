using UnityEngine;
using System.Collections;
using Pathea;

public class PEEnergyGunLogic : PEEquipmentLogic, IRechargeableEquipment 
{
	public float		m_RechargeEnergySpeed = 3f;
	public float		m_RechargeDelay = 1.5f;

	public Magazine 		m_Magazine;
	public virtual float	magazineSize{ get{ return m_Magazine.m_Size; } }
	public virtual float	magazineValue
	{
		get
		{
			if(null != m_ItemAmmoAttr && Mathf.Abs(m_ItemAmmoAttr.count - m_Magazine.m_Value) > 0.8f)
				m_Magazine.m_Value = m_ItemAmmoAttr.count;
			return m_Magazine.m_Value; 
		}
		set{ m_Magazine.m_Value = value; if(null != m_ItemAmmoAttr) m_ItemAmmoAttr.count = Mathf.RoundToInt(value); } 
	}
	
	ItemAsset.GunAmmo 		m_ItemAmmoAttr;
	float m_LastNetValue;
	UTimer m_Time;

	public override void InitEquipment(PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Equip.SetEnergyGunLogic(this);
		
		m_ItemAmmoAttr = itemObj.GetCmpt<ItemAsset.GunAmmo>();
		if (null != m_ItemAmmoAttr)
		{	
			if (null != m_Magazine)
			{
				if (m_ItemAmmoAttr.count < 0)
				{
					m_ItemAmmoAttr.count = (int)magazineSize;
				}
				
				m_Magazine.m_Value = m_ItemAmmoAttr.count;
			}
		}
		
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = GameConfig.NetUpdateInterval;
		m_LastNetValue = enCurrent;
	}
	
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
					PlayerNetwork.mainPlayer.RequestGunEnergyReload(m_Entity.Id, m_ItemObj.instanceId, enCurrent);
				}
			}
		}
	}
	
	#region IRechargeable implementation
	
	public float enMax { get { return magazineSize;	} }
	
	public float enCurrent 
	{
		get { return magazineValue;	}
		set { magazineValue = value; }
	}
	
	public float rechargeSpeed { get { return m_RechargeEnergySpeed; } }
	
	public float lastUsedTime {	get; set; }
	
	public float rechargeDelay { get { return m_RechargeDelay; } }
	
	#endregion
}
