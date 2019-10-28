using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using System.IO;
using ItemAsset;

public class CSRecordMgr 
{
	public const int VERSION = 0x0103;
	
	static private CSRecordMgr s_Instance;
	static public  CSRecordMgr Instance
	{
		get
		{
			if (s_Instance == null)
				s_Instance = new CSRecordMgr();
			
			return s_Instance;
		}
	}
	
	public Dictionary<int, CSObjectData>	m_ObjectDatas;
	
	public Dictionary<int, CSPersonnelData>	  m_PersonnelDatas;
	
	public CSRecordMgr ()
	{
		//m_ObjectDatas = new Dictionary<int, CSDefaultData>();
		m_ObjectDatas = new Dictionary<int, CSObjectData>();
		m_PersonnelDatas = new Dictionary<int, CSPersonnelData>();
	}
	
	//  <CETC> Assign Record Data
	public bool AssignData (int id, int type, ref CSDefaultData refData)
	{
		// Personnel
		if (type == CSConst.dtPersonnel)
		{
			if (m_PersonnelDatas.ContainsKey(id))
			{
				Debug.Log("The Personnel Data ID [" + id.ToString() + "] is exist.");
				refData = m_PersonnelDatas[id];
				return false;
			}
			else
			{
				refData = new CSPersonnelData();
				refData.ID = id;
				m_PersonnelDatas.Add(id, refData as CSPersonnelData);
				return true;
			}
			
		}
		// Object
		else
		{
			if (m_ObjectDatas.ContainsKey(id))
			{
				Debug.Log("The Object data ID [" + id.ToString() + "] is exist." );
				refData = m_ObjectDatas[id];
				return false;
			}
			else
			{
				switch (type)
				{
				case CSConst.dtAssembly:
					refData = new CSAssemblyData();
					break;
				case CSConst.dtStorage:
					refData = new CSStorageData();
					break;
				case CSConst.dtEngineer:
					refData = new CSEngineerData();
					break;
				case CSConst.dtEnhance:
					refData = new CSEnhanceData();
					break;
				case CSConst.dtRepair:
					refData = new CSRepairData();
					break;
				case CSConst.dtRecyle:
					refData = new CSRecycleData();
					break;
				case CSConst.dtppCoal:
					refData = new CSPPCoalData();
					break;
				case CSConst.dtDwelling:
					refData = new CSDwellingsData();
					break;
				case CSConst.dtFactory:
					refData = new CSFactoryData();
					break;
                case CSConst.dtProcessing:
                    refData = new CSProcessingData();
                    break;
				default:
					//refData = new CSDefaultData();
					refData = new CSObjectData();
					break;
				}
				
				refData.ID = id;
				m_ObjectDatas.Add(id, refData as CSObjectData);
				return true;
			}
		}
	}
	
	public void RemoveData (int id, int type)
	{
		if (type == CSConst.dtPersonnel)
			RemovePersonnelData(id);
		else
			RemoveObjectData(id);
	}
	
	public void RemoveObjectData (int id)
	{
		if (!m_ObjectDatas.ContainsKey(id))
			Debug.LogWarning("You want to remove a object data, but it not exist!");
		else
			m_ObjectDatas.Remove(id);
	}
	
	public void RemovePersonnelData (int id)
	{
		if (!m_PersonnelDatas.ContainsKey(id))
			Debug.LogWarning("You want to remove a Personnel data, but it not exist!");
		else
			m_PersonnelDatas.Remove(id);
	}
	
//	public Dictionary<int, CSDefaultData>.Enumerator GetRecords()
//	{
//		return m_ObjectDatas.GetEnumerator();
//	}
	
	public Dictionary<int, CSObjectData>.ValueCollection GetObjectRecords()
	{
		return m_ObjectDatas.Values;
	}
	
	public Dictionary<int, CSPersonnelData>.ValueCollection GetPersonnelRecords()
	{
		return m_PersonnelDatas.Values;
	}
	
	public void ClearData ()
	{
		m_ObjectDatas.Clear();
		m_PersonnelDatas.Clear();
	}
	
	#region IMPORT & EXPORT
	
	// <CETC> Import type Data
	public void Import( byte[] buffer )
	{
		if ( buffer == null )
			return;
		if ( buffer.Length < 8 )
			return;
		
		MemoryStream ms = new MemoryStream (buffer);
		BinaryReader r = new BinaryReader (ms);
		int version = r.ReadInt32();
		
		if ( VERSION != version )
		{
			Debug.LogWarning("The version of ColonyrecordMgr is newer than the record.");
		}
		
		switch ( version )
		{
		#region Version_0x0101
		case 0x0101:
		{
			int rcnt = r.ReadInt32();
			
			for (int i = 0; i < rcnt; i++)
			{
				CSObjectData csdd = null;
				int type = r.ReadInt32();
				switch (type)
				{
				case CSConst.dtAssembly:
				{
					csdd = new CSAssemblyData();	
					CSAssemblyData cssa = csdd as CSAssemblyData;
					_readCSObjectData(r, cssa, version);
					
					cssa.m_Level			= r.ReadInt32();
					cssa.m_UpgradeTime		= r.ReadSingle();
					cssa.m_CurUpgradeTime	= r.ReadSingle();
					
				}break;
				case CSConst.dtppCoal:
				{
					csdd = new CSPPCoalData();
					CSPPCoalData csppc = csdd as CSPPCoalData;
					_readCSObjectData(r, csppc, version);

					csppc.m_CurWorkedTime 	= r.ReadSingle();
					csppc.m_WorkedTime		= r.ReadSingle();
				}break;
				case CSConst.dtStorage:
				{
					csdd = new CSStorageData();
					CSStorageData cssd = csdd as CSStorageData;
					_readCSObjectData(r,cssd, version);
									
					int itemCnt = r.ReadInt32();
					for (int j = 0; j < itemCnt; j++)
						cssd.m_Items.Add(r.ReadInt32(), r.ReadInt32());
				}break;
				case CSConst.dtEngineer:
				{
					csdd = new CSEngineerData();
					CSEngineerData csed = csdd as CSEngineerData;
					_readCSObjectData(r, csed, version);
					
					csed.m_EnhanceItemID 	= r.ReadInt32();
					csed.m_CurEnhanceTime	= r.ReadSingle();
					csed.m_EnhanceTime		= r.ReadSingle();
					csed.m_PatchItemID		= r.ReadInt32();
					csed.m_CurPatchTime		= r.ReadSingle();
					csed.m_PatchTime		= r.ReadSingle();
					csed.m_RecycleItemID	= r.ReadInt32();
					csed.m_CurRecycleTime	= r.ReadSingle();
					csed.m_RecycleTime		= r.ReadSingle();
				}break;
				case CSConst.dtEnhance:
				{
					csdd = new CSEnhanceData();
					CSEnhanceData cseh = csdd as CSEnhanceData;
					_readCSObjectData(r, cseh, version);
					
					cseh.m_ObjID		= r.ReadInt32();
					cseh.m_CurTime		= r.ReadSingle();
					cseh.m_Time			= r.ReadSingle();
				}break;
				case CSConst.dtRepair:
				{
					csdd = new CSRepairData();
					CSRepairData csrd	= csdd as CSRepairData;
					_readCSObjectData(r, csrd, version);
					
					csrd.m_ObjID		= r.ReadInt32();
					csrd.m_CurTime		= r.ReadSingle();
					csrd.m_Time			= r.ReadSingle();
				}break;
				case CSConst.dtRecyle:
				{
					csdd = new CSRecycleData();
					CSRecycleData csrd = csdd as CSRecycleData;
					_readCSObjectData(r, csrd, version);
					
					csrd.m_ObjID		= r.ReadInt32();
					csrd.m_CurTime		= r.ReadSingle();
					csrd.m_Time			= r.ReadSingle();
				}break;
				case CSConst.dtDwelling:
				{
					csdd = new CSDwellingsData();
					CSDwellingsData csdw = csdd as CSDwellingsData;
					_readCSObjectData(r, csdw, version);
					
				}break;
				default:
					csdd = new CSObjectData();
					break;
				}
				
				m_ObjectDatas.Add(csdd.ID, csdd);
				
			}
			
			rcnt = r.ReadInt32();
			for (int i = 0; i < rcnt; i++)
			{
				CSPersonnelData cspd = new CSPersonnelData();
				cspd.ID 			= r.ReadInt32();
				cspd.dType			= r.ReadInt32();
				cspd.m_State		= r.ReadInt32();
				cspd.m_DwellingsID	= r.ReadInt32();
				cspd.m_WorkRoomID	= r.ReadInt32();
				
				m_PersonnelDatas.Add(cspd.ID, cspd);
			}
			
			
		}break;
		#endregion

		case 0x0102:
		case 0x0103:
		{
			int rcnt = r.ReadInt32();
			
			for (int i = 0; i < rcnt; i++)
			{
				CSObjectData csdd = null;
				int type = r.ReadInt32();
				switch (type)
				{
				case CSConst.dtAssembly:
				{
					csdd = new CSAssemblyData();	
					CSAssemblyData cssa = csdd as CSAssemblyData;
					_readCSObjectData(r, cssa, version);
					
					cssa.m_Level			= r.ReadInt32();
					cssa.m_UpgradeTime		= r.ReadSingle();
					cssa.m_CurUpgradeTime	= r.ReadSingle();
					
				}break;
				case CSConst.dtppCoal:
				{
					csdd = new CSPPCoalData();
					CSPPCoalData csppc = csdd as CSPPCoalData;
					_readCSObjectData(r, csppc, version);

					int cnt = r.ReadInt32();
					for (int j = 0; j < cnt; j++)
					{
						csppc.m_ChargingItems.Add(r.ReadInt32(), r.ReadInt32());
					}
					csppc.m_CurWorkedTime 	= r.ReadSingle();
					csppc.m_WorkedTime		= r.ReadSingle();
				}break;
				case CSConst.dtStorage:
				{
					csdd = new CSStorageData();
					CSStorageData cssd = csdd as CSStorageData;
					_readCSObjectData(r,cssd, version);
					
					int itemCnt = r.ReadInt32();
					for (int j = 0; j < itemCnt; j++)
						cssd.m_Items.Add(r.ReadInt32(), r.ReadInt32());
				}break;
				case CSConst.dtEngineer:
				{
					csdd = new CSEngineerData();
					CSEngineerData csed = csdd as CSEngineerData;
					_readCSObjectData(r, csed, version);
					
					csed.m_EnhanceItemID 	= r.ReadInt32();
					csed.m_CurEnhanceTime	= r.ReadSingle();
					csed.m_EnhanceTime		= r.ReadSingle();
					csed.m_PatchItemID		= r.ReadInt32();
					csed.m_CurPatchTime		= r.ReadSingle();
					csed.m_PatchTime		= r.ReadSingle();
					csed.m_RecycleItemID	= r.ReadInt32();
					csed.m_CurRecycleTime	= r.ReadSingle();
					csed.m_RecycleTime		= r.ReadSingle();
				}break;
				case CSConst.dtEnhance:
				{
					csdd = new CSEnhanceData();
					CSEnhanceData cseh = csdd as CSEnhanceData;
					_readCSObjectData(r, cseh, version);
					
					cseh.m_ObjID		= r.ReadInt32();
					cseh.m_CurTime		= r.ReadSingle();
					cseh.m_Time			= r.ReadSingle();
				}break;
				case CSConst.dtRepair:
				{
					csdd = new CSRepairData();
					CSRepairData csrd	= csdd as CSRepairData;
					_readCSObjectData(r, csrd, version);
					
					csrd.m_ObjID		= r.ReadInt32();
					csrd.m_CurTime		= r.ReadSingle();
					csrd.m_Time			= r.ReadSingle();
				}break;
				case CSConst.dtRecyle:
				{
					csdd = new CSRecycleData();
					CSRecycleData csrd = csdd as CSRecycleData;
					_readCSObjectData(r, csrd, version);
					
					csrd.m_ObjID		= r.ReadInt32();
					csrd.m_CurTime		= r.ReadSingle();
					csrd.m_Time			= r.ReadSingle();
				}break;
				case CSConst.dtDwelling:
				{
					csdd = new CSDwellingsData();
					CSDwellingsData csdw = csdd as CSDwellingsData;
					_readCSObjectData(r, csdw, version);
					
				}break;
				default:
					csdd = new CSObjectData();
					break;
				}
				
				m_ObjectDatas.Add(csdd.ID, csdd);
				
			}
			
			rcnt = r.ReadInt32();
			for (int i = 0; i < rcnt; i++)
			{
				CSPersonnelData cspd = new CSPersonnelData();
				cspd.ID 			= r.ReadInt32();
				cspd.dType			= r.ReadInt32();
				cspd.m_State		= r.ReadInt32();
				cspd.m_DwellingsID	= r.ReadInt32();
				cspd.m_WorkRoomID	= r.ReadInt32();
				
				m_PersonnelDatas.Add(cspd.ID, cspd);
			}
		}break;
		default:
			break;
		}
	}
	
	
	// <CETC> export type Data
	public byte[] Export()
	{
		MemoryStream ms = new MemoryStream ();
		BinaryWriter w = new BinaryWriter (ms);
		
		w.Write(VERSION);
		w.Write(m_ObjectDatas.Count);
		
		foreach (KeyValuePair<int, CSObjectData> kvp in m_ObjectDatas)
		{
			//w.Write((byte)kvp.Key);
			w.Write(kvp.Value.dType);
			switch (kvp.Value.dType)
			{
			case CSConst.dtAssembly:
			{
				CSAssemblyData cssa = kvp.Value as CSAssemblyData;
				_writeCSObjectData (w, cssa);
				
				w.Write(cssa.m_Level);
				w.Write(cssa.m_UpgradeTime);
				w.Write(cssa.m_CurUpgradeTime);
				
			}break;
			case CSConst.dtppCoal:
			{
				CSPPCoalData csppc = kvp.Value as CSPPCoalData;
				_writeCSObjectData(w, csppc);

				w.Write(csppc.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> kvp2 in csppc.m_ChargingItems)
				{
					w.Write(kvp2.Key);
					w.Write(kvp2.Value);
				}
				w.Write(csppc.m_CurWorkedTime);
				w.Write(csppc.m_WorkedTime);
				
			}break;
			case CSConst.dtStorage:
			{
				CSStorageData cssd = kvp.Value as CSStorageData;
				_writeCSObjectData(w, cssd);
				
				w.Write(cssd.m_Items.Count);
				foreach (KeyValuePair<int, int> kvp2 in cssd.m_Items)
				{
					w.Write(kvp2.Key);
					w.Write(kvp2.Value);
				}
				
			}break;
			case CSConst.dtEngineer:
			{
				CSEngineerData csed = kvp.Value as CSEngineerData;
				_writeCSObjectData(w, csed);
				
				w.Write(csed.m_EnhanceItemID);
				w.Write(csed.m_CurEnhanceTime);
				w.Write(csed.m_EnhanceTime);
				w.Write(csed.m_PatchItemID);
				w.Write(csed.m_CurPatchTime);
				w.Write(csed.m_PatchTime);
				w.Write(csed.m_RecycleItemID);
				w.Write(csed.m_CurRecycleTime);
				w.Write(csed.m_RecycleTime);
			}break;
			case CSConst.dtEnhance:
			{
				CSEnhanceData csed  = kvp.Value as CSEnhanceData;
				_writeCSObjectData(w, csed);
				
				w.Write(csed.m_ObjID);
				w.Write(csed.m_CurTime);
				w.Write(csed.m_Time);
			}break;
			case CSConst.dtRepair:
			{
				CSRepairData csrd = kvp.Value as CSRepairData;
				_writeCSObjectData(w, csrd);
				
				w.Write(csrd.m_ObjID);
				w.Write(csrd.m_CurTime);
				w.Write(csrd.m_Time);
			}break;
			case CSConst.dtRecyle:
			{
				CSRecycleData csrd = kvp.Value as CSRecycleData;
				_writeCSObjectData(w, csrd);
				
				w.Write(csrd.m_ObjID);
				w.Write(csrd.m_CurTime);
				w.Write(csrd.m_Time);
			}break;
			case CSConst.dtDwelling:
			{
				CSDwellingsData csdw = kvp.Value as CSDwellingsData;
				_writeCSObjectData(w, csdw);
			}break;
			default:
				break;
			}
		}
		
		w.Write(m_PersonnelDatas.Count);
		
		foreach (KeyValuePair<int, CSPersonnelData> kvp in m_PersonnelDatas)
		{
			w.Write(kvp.Value.ID);
			w.Write(kvp.Value.dType);
			w.Write(kvp.Value.m_State);
			w.Write(kvp.Value.m_DwellingsID);
			w.Write(kvp.Value.m_WorkRoomID);
		}
		
		w.Close();
		byte [] retval = ms.ToArray();
		return retval;
	}
	
	void _readCSObjectData (BinaryReader r, CSObjectData csod, int version)
	{
		switch ( version )
		{
		case 0x0103:
		{
			csod.ID 		  		= r.ReadInt32();
			csod.m_Name 	  		= r.ReadString();
			csod.m_Position	  		= new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			csod.m_Durability 		= r.ReadSingle();
			csod.m_CurRepairTime	= r.ReadSingle();
			csod.m_RepairTime		= r.ReadSingle();
			csod.m_RepairValue		= r.ReadSingle();
			csod.m_CurDeleteTime	= r.ReadSingle();
			csod.m_DeleteTime		= r.ReadSingle();
			csod.m_Bounds			= new Bounds( new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
			                             new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
			
			int cnt = r.ReadInt32();
			for (int i = 0; i < cnt; i++)
			{
				csod.m_DeleteGetsItem.Add(r.ReadInt32(), r.ReadInt32());
			}
		}break;
		default:
		{
			csod.ID 		  		= r.ReadInt32();
			csod.m_Name 	  		= r.ReadString();
			csod.m_Position	  		= new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			csod.m_Durability 		= r.ReadSingle();
			csod.m_CurRepairTime	= r.ReadSingle();
			csod.m_RepairTime		= r.ReadSingle();
			csod.m_RepairValue		= r.ReadSingle();
			csod.m_CurDeleteTime	= r.ReadSingle();
			csod.m_DeleteTime		= r.ReadSingle(); 
			
			int cnt = r.ReadInt32();
			for (int i = 0; i < cnt; i++)
			{
				csod.m_DeleteGetsItem.Add(r.ReadInt32(), r.ReadInt32());
			}
		}break;
		}

	}
	
	void _writeCSObjectData (BinaryWriter w, CSObjectData csod)
	{
		w.Write(csod.ID);
		w.Write(csod.m_Name);
		w.Write(csod.m_Position.x);
		w.Write(csod.m_Position.y);
		w.Write(csod.m_Position.z);
		w.Write(csod.m_Durability);
		w.Write(csod.m_CurRepairTime);
		w.Write(csod.m_RepairTime);
		w.Write(csod.m_RepairValue);
		w.Write(csod.m_CurDeleteTime);
		w.Write(csod.m_DeleteTime);
		w.Write(csod.m_Bounds.center.x);
		w.Write(csod.m_Bounds.center.y);
		w.Write(csod.m_Bounds.center.z);
		w.Write(csod.m_Bounds.size.x);
		w.Write(csod.m_Bounds.size.y);
		w.Write(csod.m_Bounds.size.z);
		
		w.Write(csod.m_DeleteGetsItem.Count);
		foreach (KeyValuePair<int, int> kvp in csod.m_DeleteGetsItem)
		{
			w.Write(kvp.Key);
			w.Write(kvp.Value);
		}
		
	}
	#endregion

}
