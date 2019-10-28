using UnityEngine;
using System.Collections;
using System.IO;
using CSRecord;
public class ColonyRepair : ColonyBase
{
	//CSRepairData _MyData;
	public ColonyRepair( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSRepairData();
		//_MyData = (CSRepairData)_RecordData;
	}

}

