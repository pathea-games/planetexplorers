//------------------------------------------------------------------------------
// 2016年7月23日10:16:53
// by Pugee
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using CSRecord;
public class ColonyPPFusion : ColonyPPCoal
{
	public ColonyPPFusion( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSPPFusionData();
		_MyData = (CSPPFusionData)_RecordData;
	}
}

