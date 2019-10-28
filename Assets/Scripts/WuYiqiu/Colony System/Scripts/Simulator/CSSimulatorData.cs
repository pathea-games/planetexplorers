//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//
//public class CSSimulatorData
//{
//	public int ID;
//
//	public float m_MaxHP;
//	public float m_Dps;
//
//	public float m_SingleAtkCD;
//
//	public float m_AOECD;
//	public float m_AOERadius;
//	public int	 m_AOECount;
//
//	public float m_TherapyCD;
//	public float m_TherapyVal;
//	
//	public float m_HP;
//
//	public Vector3 m_Pos;
//
//	public void Init(CSSimulatorAttr attr)
//	{
//		m_HP  		= attr.m_Hp;
//		m_MaxHP		= attr.m_Hp;
//		m_Dps    	= attr.m_Dps;
//		
//		m_SingleAtkCD = attr.m_SingleAtk.m_CD;
//		
//		m_AOECD 		= attr.m_AOE.m_CD;
//		m_AOECount 		= attr.m_AOE.m_Count;
//		m_AOERadius 	= attr.m_AOE.m_Radius;
//		
//		m_TherapyCD  = attr.m_Terapy.m_CD;
//		m_TherapyVal = attr.m_Terapy.m_Val;
//	}
//
//	public bool IsSingleAtkValid()
//	{
//		return m_SingleAtkCD > 0;
//	}
//
//	public bool IsAOEAtkValid()
//	{
//		return (m_AOECD > 0 && m_AOERadius > 0 && m_AOECount > 0);
//	}
//
//	public bool IsTherapyValid()
//	{
//		return (m_TherapyCD > 0 && m_TherapyVal > 0);
//	}
//
//	#region IMPORT & EXPORT
//
//	public void Export(BinaryWriter w)
//	{
//		w.Write(ID);
//		w.Write(m_MaxHP);
//		w.Write(m_Dps);
//
//		w.Write(m_SingleAtkCD);
//		w.Write(m_AOECD);
//		w.Write(m_AOERadius);
//		w.Write(m_AOECount);
//
//		w.Write(m_TherapyCD);
//		w.Write(m_TherapyVal);
//
//		w.Write(m_HP);
//		w.Write(m_Pos.x);
//		w.Write(m_Pos.y);
//		w.Write(m_Pos.z);
//	
//	}
//	
//	/// <summary>
//	/// <CSVD> Simulator data import
//	/// </summary>
//	public bool Import( BinaryReader r, int VERSION )
//	{
//		switch ( VERSION )
//		{
//		case 0x0114:
//		case 0x0115:
//		case 0x0116:
//		{
//
//			ID = r.ReadInt32();
//			m_MaxHP = r.ReadSingle();
//			m_Dps	= r.ReadSingle();
//
//			m_SingleAtkCD = r.ReadSingle();
//			m_AOECD 	  = r.ReadSingle();
//			m_AOERadius	  = r.ReadSingle();
//			m_AOECount	  = r.ReadInt32();
//
//			m_TherapyCD	  = r.ReadSingle();
//			m_TherapyVal  = r.ReadSingle();
//
//			m_HP		 = r.ReadSingle();
//			m_Pos		 = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
//			
//		}return true;
//		default:
//			break;
//		}
//		
//		return false;
//	}
//	#endregion
//}
//
//
//public class CSSimulatorDataMgr
//{
//	#region MANAGERS
//	private static List<CSSimulatorDataMgr>  s_Mgrs = new List<CSSimulatorDataMgr>();
//
//	public static bool CreateDataMgr (int id, out CSSimulatorDataMgr outMgr)
//	{
//		CSSimulatorDataMgr mgr = s_Mgrs.Find(item0 => item0.ID == id);
//		if (mgr != null)
//		{
//			outMgr = mgr;
//			return false;
//		}
//
//		mgr = new CSSimulatorDataMgr();
//		s_Mgrs.Add(mgr);
//
//		outMgr = mgr;
//
//		return true;
//	}
//
//	public static bool RemoveDataMgr(int id)
//	{
//		int index = s_Mgrs.FindIndex(item0 => item0.ID == id);
//		if (index != -1)
//		{
//			s_Mgrs.RemoveAt(index);
//			return true;
//		}
//
//		return false;
//	}
//
//	public static void ClearDataMgr ()
//	{
//		s_Mgrs.Clear();
//	}
//
//	#region IMPORT & EXPORT
//
//	public static void ExportMgrs(BinaryWriter w)
//	{
//		w.Write(s_Mgrs.Count);
//		foreach (CSSimulatorDataMgr mgr in s_Mgrs)
//		{
//			w.Write(mgr.ID);
//			mgr.Export(w);
//		}
//
//	}
//	
//	/// <summary>
//	/// <CSVD> Simulator Managers data import
//	/// </summary>
//	public static bool ImportMgrs( BinaryReader r, int VERSION )
//	{
//		switch ( VERSION )
//		{
//		case 0x0114:
//		case 0x0115:
//		case 0x0116:
//		{
//			
//			int count = r.ReadInt32();
//			for (int i = 0; i < count; i++)
//			{
//				int id = r.ReadInt32();
//				CSSimulatorDataMgr data_mgr = new CSSimulatorDataMgr();
//				data_mgr.ID = id;
//				data_mgr.Import(r, VERSION);
//				s_Mgrs.Add(data_mgr);
//				
//			}
//			
//		}return true;
//		default:
//			break;
//		}
//		
//		return false;
//	}
//
//	#endregion
//
//	#endregion
//
//
//	public int ID;
//
//	private Dictionary<int, CSSimulatorData> m_Datas;
//
//	public Dictionary<int, CSSimulatorData> Datas	{ get{ return m_Datas;} }
//
//	public CSSimulatorDataMgr()
//	{
//		m_Datas = new Dictionary<int, CSSimulatorData>();
//	}
//
//
////	public static bool CreateData(int ID, out CSSimulatorData outData)
////	{
////		CSSimulatorDataMgr mgr = s_Instance;
////
////		if (mgr.m_Datas.ContainsKey(ID))
////		{
////			outData = mgr.m_Datas[ID];
////			return false;
////		}
////
////		CSSimulatorData cssdata = new CSSimulatorData();
////		cssdata.ID = ID;
////		mgr.m_Datas.Add(ID, cssdata);
////		outData = cssdata;
////		return true;
////	}
////
////	public static bool RemoveData (int ID)
////	{
////		CSSimulatorDataMgr mgr = s_Instance;
////		return mgr.m_Datas.Remove(ID);
////	}
////
////	public static CSSimulatorData GetData(int ID)
////	{
////		CSSimulatorDataMgr mgr = s_Instance;
////		if (mgr.m_Datas.ContainsKey(ID))
////			return mgr.m_Datas[ID];
////
////		return null;
////	}
//
//	public bool CreateData(int ID, out CSSimulatorData outData)
//	{	
//		if (m_Datas.ContainsKey(ID))
//		{
//			outData = m_Datas[ID];
//			return false;
//		}
//		
//		CSSimulatorData cssdata = new CSSimulatorData();
//		cssdata.ID = ID;
//		m_Datas.Add(ID, cssdata);
//		outData = cssdata;
//		return true;
//	}
//	
//	public  bool RemoveData (int ID)
//	{
//		return m_Datas.Remove(ID);
//	}
//	
//	public CSSimulatorData GetData(int ID)
//	{
//		if (m_Datas.ContainsKey(ID))
//			return m_Datas[ID];
//		
//		return null;
//	}
//
//	#region IMPORT & EXPORT
//
//	public void Export(BinaryWriter w)
//	{
//		w.Write(m_Datas.Count);
//		foreach (KeyValuePair<int, CSSimulatorData> kvp in m_Datas)
//		{
//			w.Write(kvp.Key);
//			kvp.Value.Export(w);
//		}
//	}
//
//	/// <summary>
//	/// <CSVD> Simulator Manager data import
//	/// </summary>
//	public bool Import( BinaryReader r, int VERSION )
//	{
//		switch ( VERSION )
//		{
//		case 0x0114:
//		case 0x0115:
//		case 0x0116:
//		{
//			
//			int count = r.ReadInt32();
//			for (int i = 0; i < count; i++)
//			{
//				int id = r.ReadInt32();
//				CSSimulatorData cssdata = new CSSimulatorData();
//				cssdata.ID = id;
//				cssdata.Import(r, VERSION);
//				m_Datas.Add(id, cssdata);
//
//			}
//			
//		}return true;
//		default:
//			break;
//		}
//		
//		return false;
//	}
//
//	#endregion
//	
//}