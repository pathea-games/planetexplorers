using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using CSRecord;
using ItemAsset;

public class ColonyPowerPlant : ColonyBase
{

	CSPowerPlanetData _MyData;
	public ColonyPowerPlant()
	{
	}

	public ColonyPowerPlant( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSPowerPlanetData();
		_MyData = (CSPowerPlanetData)_RecordData;
	}

	public void AddChargeItem(int index,int objId)
	{
		ItemObject item = ItemMgr.Instance.Get (objId);
		if (item == null)
			return;
		_MyData.m_ChargingItems [index] = objId;
	}

	public void RemoveChargeItem(int objId)
	{
		foreach(var item in _MyData.m_ChargingItems)
		{
			if(item.Value == objId)
			{
				_MyData.m_ChargingItems.Remove(item.Key);
				break;
			}
		}
	}

	public virtual bool IsWorking()
	{
		return false;
	}
}

