using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;

class ColonyTrade:ColonyBase
{
    //CSTradeData _MyData;
    public ColonyTrade(ColonyNetwork network)
	{
		SetNetwork (network);
        _RecordData = new CSTradeData();
        //_MyData = (CSTradeData)_RecordData;
	}
}
