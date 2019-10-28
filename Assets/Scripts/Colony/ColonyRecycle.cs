using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using CSRecord;
public class ColonyRecycle : ColonyBase
{
	//CSRecycleData _MyData;
	public Dictionary<int, int> m_RecycleItems = new Dictionary<int, int>(); 
	public ColonyRecycle( ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSRecycleData();
		//_MyData = (CSRecycleData)_RecordData;
		
	}

}

