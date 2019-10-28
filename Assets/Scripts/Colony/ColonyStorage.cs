using UnityEngine;
using System.Collections;
using System.IO;
using Mono.Data.SqliteClient;
using CustomData;
using CSRecord;
public class ColonyStorage : ColonyBase
{
	//CSStorageData _MyData;
	public ColonyStorage( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSStorageData();
		//_MyData = (CSStorageData)_RecordData;
	}

}

