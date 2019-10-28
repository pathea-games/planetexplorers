/// <summary>
/// 2016年7月23日10:14:21
/// by Pugee
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;

public class CSPPFusion : CSPPCoal
{
	int fuelID =  CSInfoMgr.m_ppFusion.m_WorkedTimeItemID[0];
	public override int FuelID{
		get{return fuelID;}
	}
	int fuelMaxCount = CSInfoMgr.m_ppFusion.m_WorkedTimeItemCnt[0];
	public override int FuelMaxCount{
		get{return fuelMaxCount;}
	}
	float autoPercent = 0.2f;
	public override float AutoPercent{
		get{return autoPercent;}
	}
	int autoCount = 15;
	public override int AutoCount{
		get{return autoCount;}
	}
	public CSPPFusion()
	{
		m_Type = CSConst.etppFusion;
	}

	CSPPFusionData m_PPData;
	public new CSPPFusionData Data
	{
		get
		{
			if (m_PPData == null)
				m_PPData = m_Data as CSPPFusionData;
			return m_PPData;
		}
	}

	public override void CreateData()
	{
		CSDefaultData ddata = null;
		
		bool isNew;
		if (GameConfig.IsMultiMode)
		{
			isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.etppFusion, ref ddata, _ColonyObj);
		}
		else
		{
			isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.etppFusion, ref ddata);
		}
		
		m_Data = ddata as CSPPFusionData;
		
		if (isNew)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			StartWorkingCounter();
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
			
			StartWorkingCounter(Data.m_CurWorkedTime, Data.m_WorkedTime);
			
			// Get Charging Items
			foreach (KeyValuePair<int, int> kvp in Data.m_ChargingItems)
			{
				ItemObject itemObj =  ItemMgr.Instance.Get(kvp.Value);
				if(itemObj != null)
					m_ChargingItems[kvp.Key] = ItemMgr.Instance.Get(kvp.Value).GetCmpt<Energy>();
			}
		}
	}
}
