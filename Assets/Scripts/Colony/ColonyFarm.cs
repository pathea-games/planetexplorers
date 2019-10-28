using UnityEngine;
using System.Collections;
using System.IO;
using CSRecord;
public class ColonyFarm : ColonyBase
{
	//CSFarmData _MyData;
	public ColonyFarm( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSFarmData();
		//_MyData = (CSFarmData)_RecordData;
	}

}

