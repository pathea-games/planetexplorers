using UnityEngine;
using System.Collections;
using System.IO;
using CSRecord;
public class ColonyFactory : ColonyBase
{
	//CSFactoryData _MyData;
	public ColonyFactory( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSFactoryData();
		//_MyData = (CSFactoryData)_RecordData;
	}
	
}

