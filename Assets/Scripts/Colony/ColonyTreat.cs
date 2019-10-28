using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;


public class ColonyTreat:ColonyBase
{
    //CSTreatData _MyData;
    public ColonyTreat(ColonyNetwork network)
    {
        SetNetwork(network);
        _RecordData = new CSTreatData();
        //_MyData = (CSTreatData)_RecordData;
    }
}
