using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;

public class ColonyProcessing:ColonyBase
{
    //CSProcessingData _MyData;
    public ColonyProcessing(ColonyNetwork network)
	{
		SetNetwork (network);
		_RecordData = new CSProcessingData();
		//_MyData = (CSProcessingData)_RecordData;
	}
}
