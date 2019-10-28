using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;

public class CSPPSolar : CSPowerPlant
{
    public override bool IsDoingJob()
    {
        return IsRunning;
    }

	public  CSPPSolarData m_PPData;
	public  CSPPSolarData Data 	
	{ 
		get { 
			if (m_PPData == null)
				m_PPData = m_Data as CSPPSolarData;
			return m_PPData; 
		} 
	}

	public CSPPSolar()
	{
		m_Type = CSConst.etppSolar;
	}

	#region OVERRIDE_FUNC

	protected override void ChargingItem (float deltaTime)
	{
		foreach (Energy item in m_ChargingItems)
		{
			if (item == null)
				continue;
			// Normal Item Object
            //if (item.instanceId < CreationData.s_ObjectStartID)
            //{
            //    float battery_power 	 = item.GetProperty(ItemProperty.BatteryPower);
            //    float battery_power_max  = item.GetProperty(ItemProperty.BatteryPowerMax);
				
            //    battery_power = Mathf.Min(battery_power_max, battery_power + Info.m_ChargingRate * deltaTime);
				
            //    item.SetProperty(ItemProperty.BatteryPower, battery_power);
            //}


			item.energy.Change(deltaTime * Info.m_ChargingRate*10000/Time.deltaTime);
		}
	}

	#endregion

	#region CSENTITY_FUNC
	public override void DestroySelf ()
	{
		base.DestroySelf();
	}

	public override void CreateData ()
	{
		CSDefaultData ddata = null;
		bool isNew =  m_Creator.m_DataInst.AssignData(ID, CSConst.etppSolar, ref ddata);
		m_Data = ddata as CSPowerPlanetData;

		if (isNew)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);

			// Get Charging Items
			foreach (KeyValuePair<int, int> kvp in Data.m_ChargingItems)
			{
				m_ChargingItems[kvp.Key] = ItemMgr.Instance.Get(kvp.Value).GetCmpt<Energy>();
			}
		}

		m_IsRunning = true;
	}

	public override void RemoveData ()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);

	}

	public override void Update ()
	{
		base.Update ();
	}

	#endregion
}
