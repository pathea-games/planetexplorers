using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;

public class ColonyCheck : ColonyBase
{
    //CSCheckData _MyData;
    public ColonyCheck(ColonyNetwork network)
    {
        SetNetwork(network);
        _RecordData = new CSCheckData();
        //_MyData = (CSCheckData)_RecordData;
    }

}
