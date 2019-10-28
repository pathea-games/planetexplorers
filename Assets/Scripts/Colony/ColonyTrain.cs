using CSRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ColonyTrain:ColonyBase
{
    //CSTrainData _MyData;
    public ColonyTrain(ColonyNetwork network)
    {
        SetNetwork(network);
        _RecordData = new CSTrainData();
        //_MyData = (CSTrainData)_RecordData;
    }

}