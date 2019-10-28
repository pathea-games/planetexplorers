using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
public class ColonyTent:ColonyBase
{
    //CSTentData _MyData;
    public ColonyTent(ColonyNetwork network)
	{
		SetNetwork (network);
        _RecordData = new CSTentData();
        //_MyData = (CSTentData)_RecordData;
	}
}
