using UnityEngine;
using System.Collections;
using CSRecord;
public class ColonyPPCoal : ColonyPowerPlant
{
	
	protected CSPPCoalData _MyData;

	public ColonyPPCoal(){}

	public ColonyPPCoal( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSPPCoalData();
		_MyData = (CSPPCoalData)_RecordData;
	}
	public override bool IsWorking()
	{
		if (_MyData.m_CurWorkedTime < _MyData.m_WorkedTime && _MyData.m_CurWorkedTime != -1)
			return true;
		return false;
	}
}

