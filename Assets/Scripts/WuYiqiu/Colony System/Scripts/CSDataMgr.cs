/**************************************************************
 *                       [CSDataMgr.cs]
 *
 *    Colony System Data Managers, 
 *
 *     Import or export Main Datas for Colony System
 *
 *
 **************************************************************/

//
//  Searching Keywords for New VERSION :
//
//   <CSVD> -  Colony System VERSION Data, code considers .
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public static class CSDataMgr
{
    static bool debugSwitch = false;
	public const int VERSION000 = 15042418;
    public const int VERSION001 = 15091800;
    public const int VERSION002 = 15110600;
	public const int VERSION003 = 15111720;
	public const int VERSION004 = 16052514;
	public const int VERSION005 = 16071518;
	public const int VERSION006 = 16072317;
	public const int VERSION007 = 16091400;
	public const int VERSION008 = 16101900;
	public const int VERSION009 = 16102000;
	public const int VERSION010 = 16102100;
	public const int CUR_VERSION = VERSION010;
	// Colony System Datas
	public static Dictionary<int, CSDataInst>  m_DataInsts = new Dictionary<int, CSDataInst>();

	// 

	public static bool HasDataInst()
	{
		return m_DataInsts.Count != 0;
	}
	
	public static CSDataInst CreateDataInst(int id,CSConst.CreatorType type)
	{
		if (m_DataInsts.ContainsKey(id))
		{
			Debug.Log("Still have this data inst.");
			return m_DataInsts[id];
		}
		else
		{
			CSDataInst dataInst = new CSDataInst();
			dataInst.m_ID = id;
            dataInst.m_Type = type;
			m_DataInsts.Add(id, dataInst);
			return dataInst;
		}
	}

	public static void RemoveDataInst(int ID)
	{
		if (m_DataInsts.ContainsKey(ID))
		{
			m_DataInsts[ID].ClearData();
			return;
		}
	}

	public static void Clear()
	{
		foreach(CSDataInst dataInst in m_DataInsts.Values)
			dataInst.ClearData();
		m_DataInsts.Clear();
		CSClodMgr.Clear(); 
		CSClodsMgr.Clear();
	}

	public static int GenerateNewID()
	{
		int maxId = 0;
		foreach (int id in m_DataInsts.Keys)
		{
			if (maxId < id)
				maxId = id;
		}

		return maxId + 1;
	}
	

	// <CSVD> Main Data Managers Imports 
	public static void Import( byte[] buffer )
    {
        Clear();
		if ( buffer == null )
			return;
		if ( buffer.Length < 8 )
			return;

        if (debugSwitch) Debug.Log("<color=yellow>" + "Start to Import CSDataMgr" + "</color>");
		MemoryStream ms = new MemoryStream (buffer);
		BinaryReader r = new BinaryReader (ms);
		int version = r.ReadInt32();
        if (debugSwitch) Debug.Log("<color=yellow>" + "version:" + version + "</color>");
		if ( CUR_VERSION != version )
		{
            if (debugSwitch) Debug.LogWarning("The version of ColonyrecordMgr is newer than the record.");
		}

		if(version>=VERSION000){
            int count = r.ReadInt32();
			    for (int i = 0; i < count; i++)
			    {
				    CSDataInst dataInst = new CSDataInst();
                    if (debugSwitch) Debug.Log("<color=yellow>" + i + " count: " + count + "</color>");
				    dataInst.m_ID = r.ReadInt32();
                    if (debugSwitch) Debug.Log("<color=yellow>" + "m_ID: " + dataInst.m_ID + "</color>");
				    dataInst.Import(r);
				    m_DataInsts.Add(dataInst.m_ID, dataInst);
			    }
			
			    CSClodMgr.Init();
			    CSClodMgr.Instance.Import(r);
			
			    CSClodsMgr.Init();
			    CSClodsMgr.Instance.Import(r);

			    //CSSimulatorDataMgr.ImportMgrs(r, version);
		}
	}

	public static void Export(BinaryWriter w)
    {
        if (debugSwitch) Debug.Log("<color=yellow>" + "Start to Export CSDataMgr" + "</color>");

		w.Write(CUR_VERSION); 
		w.Write(m_DataInsts.Count); 

		foreach (KeyValuePair<int, CSDataInst> kvp in m_DataInsts)
        {
            if (debugSwitch) Debug.Log("<color=yellow>" + "Key(m_ID): " + kvp.Key + "</color>");
            w.Write(kvp.Key);
			kvp.Value.Export(w);
		}

		CSClodMgr.Instance.Export(w);
		//int index = 1001;
		//w.Write(index);
		CSClodsMgr.Instance.Export(w);

		//CSSimulatorDataMgr.ExportMgrs(w);
	}
}
