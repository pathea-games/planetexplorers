using UnityEngine;
using System.Collections;
using System.IO;
using CSRecord;
public class ColonyEnhance : ColonyBase
{
	//CSEnhanceData _MyData;
	public ColonyEnhance( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSEnhanceData();
		//_MyData = (CSEnhanceData)_RecordData;
	}

}

