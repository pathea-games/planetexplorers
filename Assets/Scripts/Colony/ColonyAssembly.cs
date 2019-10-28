using UnityEngine;
using System.Collections;
using System.IO;
using CSRecord;

public class ColonyAssembly : ColonyBase
{
	//CSAssemblyData _MyData;
	public ColonyAssembly( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSAssemblyData();
		//_MyData = (CSAssemblyData)_RecordData;
	}


}

