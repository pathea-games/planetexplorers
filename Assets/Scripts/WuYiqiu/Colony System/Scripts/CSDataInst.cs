using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using System.IO;
using CustomData;

public class CSDataMonsterSiege
{
    public int lvl;
    public float lastHour;
    public float nextHour; 

    public void Import(BinaryReader r)
    {
        lvl         = r.ReadInt32();
        lastHour    = r.ReadSingle();
        nextHour    = r.ReadSingle();
    }

    public void Export(BinaryWriter w)
    {
        w.Write(lvl);
        w.Write(lastHour);
        w.Write(nextHour);
    }
}

public class CSDataInst
{
    bool debugSwitch = false;

	public int m_ID;

    public CSDataMonsterSiege m_Siege;

	public Dictionary<int, CSDefaultData>	m_ObjectDatas;
	
	public Dictionary<int, CSPersonnelData>	  m_PersonnelDatas;

    public List<CSTreatment> treatmentList= new List<CSTreatment> ();

	public List<int> addedTradeNpc = new List<int> ();

	public List<int> addedStoreId = new List<int> ();

	public int colonyMoney = 5000;

	public CSConst.CreatorType  m_Type;
	public CSDataInst ()
	{
        m_Siege = new CSDataMonsterSiege();
		m_ObjectDatas = new Dictionary<int, CSDefaultData>();
		m_PersonnelDatas = new Dictionary<int, CSPersonnelData>();

	}
	public bool AddData(CSPersonnelData data)
	{
		if (m_PersonnelDatas.ContainsKey(data.ID))
		{
			return false;
		}
		else
		{
			m_PersonnelDatas.Add(data.ID, data);
			return true;
		}
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
				//Debug.Log("The Object data ID [" + id.ToString() + "] is exist." );
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
				case CSConst.dtppSolar:
					refData = new CSPPSolarData();
					break;
				case CSConst.dtDwelling:
					refData = new CSDwellingsData();
					break;
				case CSConst.dtFarm:
					refData = new CSFarmData();
					break;
				case CSConst.dtFactory:
					refData = new CSFactoryData();
					break;
                case CSConst.dtProcessing:
                    refData = new CSProcessingData();
                    break;
                case CSConst.dtTrade:
                    refData = new CSTradeData();
                    break;
                case CSConst.dtTrain:
                    refData = new CSTrainData();
                    break;
                case CSConst.dtCheck:
                    refData = new CSCheckData();
                    break;
                case CSConst.dtTreat:
                    refData = new CSTreatData();
                    break;
                case CSConst.dtTent:
                    refData = new CSTentData();
					break;
				case CSConst.dtppFusion:
					refData = new CSPPFusionData();
					break;
				default:
					//refData = new CSDefaultData();
					refData = new CSDefaultData();
					break;
				}
				
				refData.ID = id;
				m_ObjectDatas.Add(id, refData as CSDefaultData);
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

	public Dictionary<int, CSDefaultData> GetObjectRecords()
	{
		return m_ObjectDatas;
	}
	
	public Dictionary<int, CSPersonnelData>.ValueCollection GetPersonnelRecords()
	{
		return m_PersonnelDatas.Values;
	}
	
	public void ClearData ()
	{
		m_ObjectDatas.Clear();
		m_PersonnelDatas.Clear();
        treatmentList.Clear();
		addedTradeNpc.Clear();
		addedStoreId.Clear();
	}

	#region IMPORT & EXPORT
	// <CETC> Import type Data
	// <CSVD> Data Instance Import 
	public bool Import( BinaryReader r )
	{
		int version = r.ReadInt32();

        if(version >= CSDataMgr.VERSION003)
            m_Siege.Import(r);

        if (version >= CSDataMgr.VERSION000)
        {
            m_Type = (CSConst.CreatorType)r.ReadInt32();
            if (debugSwitch) Debug.Log("<color=yellow>" + "m_Type: " + m_Type + "</color>");
            int rcnt = r.ReadInt32();
            if (debugSwitch) Debug.Log("<color=yellow>" + "rcnt: " + rcnt + "</color>");
            for (int i = 0; i < rcnt; i++)
            {
                CSDefaultData csdd = null;
                int type = r.ReadInt32();
                if (debugSwitch) Debug.Log("<color=yellow>" + "type: " + type + "</color>");
                switch (type)
                {
                    case CSConst.dtAssembly:
                        csdd = _readAssemblyData(r, version);
                        break;
                    case CSConst.dtppCoal:
                        csdd = _readPPCoalData(r, version);
                        break;
                    case CSConst.dtppSolar:
                        csdd = _readPPSolarData(r, version);
                        break;
                    case CSConst.dtStorage:
                        csdd = _readStorageData(r, version);
                        break;
                    case CSConst.dtEnhance:
                        csdd = _readEnhanceData(r, version);
                        break;
                    case CSConst.dtRepair:
                        csdd = _readRepairData(r, version);
                        break;
                    case CSConst.dtRecyle:
                        csdd = _readRecycleData(r, version);
                        break;
                    case CSConst.dtDwelling:
                        csdd = _readDwellingsData(r, version);
                        break;
                    case CSConst.dtFarm:
                        csdd = _readFarmData(r, version);
                        break;
                    case CSConst.dtFactory:
                        csdd = _readFactoryData(r, version);
                        break;
                    case CSConst.dtProcessing:
                        csdd = _readProcessingData(r, version);
                        break;
                    case CSConst.dtTrade:
                        csdd = _readTradeData(r, version);
                        break;
                    case CSConst.dtTrain:
                        csdd = _readTrainData(r, version);
                        break;
                    case CSConst.dtCheck:
                        csdd = _readCheckData(r, version);
                        break;
                    case CSConst.dtTreat:
                        csdd = _readTreatData(r, version);
                        break;
                    case CSConst.dtTent:
                        csdd = _readTentData(r, version);
						break;
					case CSConst.dtppFusion:
						csdd = _readFusionData(r, version);
						break;
					default:
						break;
				}
				if (csdd != null)
					m_ObjectDatas.Add(csdd.ID, csdd);
            }
            _readPesonnelData(r, version);
        
            if (version >= CSDataMgr.VERSION000 && version < CSDataMgr.VERSION001)
            {

            }

            if (version >= CSDataMgr.VERSION001)
            {
                int treatmentCount = r.ReadInt32();
                for(int i=0;i<treatmentCount;i++){
                    treatmentList.Add(CSTreatment._readTreatmentData(r, version));
                }
            }

			if (version >= CSDataMgr.VERSION008)
			{
				int tradeNpcCount = r.ReadInt32 ();
				for(int i=0;i<tradeNpcCount;i++)
					addedTradeNpc.Add(r.ReadInt32());
				int storeIdCount = r.ReadInt32 ();
				for(int i=0;i<storeIdCount;i++)
					addedStoreId.Add(r.ReadInt32());
			}

			if(version>=CSDataMgr.VERSION010){
				colonyMoney = r.ReadInt32();
			}
            return true;
        }
		return false;

	}

	// <CETC> export type Data
	public void Export(BinaryWriter w)
	{

        w.Write(CSDataMgr.CUR_VERSION);
        if (debugSwitch) Debug.Log("<color=yellow>" + "version: " + CSDataMgr.VERSION000 + "</color>");

        if(CSDataMgr.CUR_VERSION >= CSDataMgr.VERSION003)
            m_Siege.Export(w);

        w.Write((int)m_Type);
        if (debugSwitch) Debug.Log("<color=yellow>" + "m_Type: " + m_Type + "</color>");
        w.Write(m_ObjectDatas.Count);
        if (debugSwitch) Debug.Log("<color=yellow>" + "m_ObjectDatas.Count: " + m_ObjectDatas.Count + "</color>");
        foreach (KeyValuePair<int, CSDefaultData> kvp in m_ObjectDatas)
		{
			w.Write(kvp.Value.dType);

            if (debugSwitch) Debug.Log("<color=yellow>" + "dType: " + kvp.Value.dType + "</color>");
			switch (kvp.Value.dType)
			{
			case CSConst.dtAssembly:
			{
				CSAssemblyData cssa = kvp.Value as CSAssemblyData;
				_writeCSObjectData (w, cssa);

				w.Write(cssa.m_ShowShield);
				w.Write(cssa.m_Level);
				w.Write(cssa.m_UpgradeTime);
				w.Write(cssa.m_CurUpgradeTime);
				w.Write(cssa.m_TimeTicks);
				//2016-9-14 14:30:48
				w.Write(cssa.m_MedicineResearchState);
				w.Write(cssa.m_MedicineResearchTimes);
			}break;
			case CSConst.dtppCoal:
			{
				CSPPCoalData csppc = kvp.Value as CSPPCoalData;
				_writeCSObjectData(w, csppc);
				w.Write(csppc.bShowElectric);
				w.Write(csppc.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> kvp2 in csppc.m_ChargingItems)
				{
					w.Write(kvp2.Key);
					w.Write(kvp2.Value);
				}
				w.Write(csppc.m_CurWorkedTime);
				w.Write(csppc.m_WorkedTime);
				
			}break;
			case CSConst.dtppFusion:
			{
				CSPPFusionData csppf = kvp.Value as CSPPFusionData;
				_writeCSObjectData(w, csppf);
				w.Write(csppf.bShowElectric);
				w.Write(csppf.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> kvp2 in csppf.m_ChargingItems)
				{
					w.Write(kvp2.Key);
					w.Write(kvp2.Value);
				}
				w.Write(csppf.m_CurWorkedTime);
				w.Write(csppf.m_WorkedTime);
			}break;
			case CSConst.dtppSolar:
			{
				CSPowerPlanetData cspp = kvp.Value as CSPowerPlanetData;
				_writeCSObjectData(w, cspp);

				w.Write(cspp.m_ChargingItems.Count);
				foreach (KeyValuePair<int, int> kvp2 in cspp.m_ChargingItems)
				{
					w.Write(kvp2.Key);
					w.Write(kvp2.Value);
				}
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
			case CSConst.dtFarm:
			{
				CSFarmData csfd = kvp.Value as CSFarmData;
				_writeCSObjectData(w, csfd);
				w.Write(csfd.m_PlantSeeds.Count);
				foreach(KeyValuePair<int, int> kvp2 in csfd.m_PlantSeeds)
				{
					w.Write(kvp2.Key);
					w.Write(kvp2.Value);
				}
				w.Write(csfd.m_Tools.Count);
				foreach(KeyValuePair<int, int> kvp2 in csfd.m_Tools)
				{
					w.Write(kvp2.Key);
					w.Write(kvp2.Value);
				}

				w.Write(csfd.m_AutoPlanting);
				w.Write(csfd.m_SequentialPlanting);
			}break;
			case CSConst.dtFactory:
			{
				CSFactoryData csfd = kvp.Value as CSFactoryData;
				_writeCSObjectData(w, csfd);
				w.Write(csfd.m_CompoudItems.Count);
				foreach (CompoudItem item in csfd.m_CompoudItems)
				{
					w.Write(item.curTime);
					w.Write(item.time);
					w.Write(item.itemID);
					w.Write(item.itemCnt);
				}

            } break;
            case CSConst.dtProcessing:
                {
                    CSProcessingData cspf = kvp.Value as CSProcessingData;
                    _writeCSObjectData(w, cspf);
                    w.Write(cspf.isAuto);
                    w.Write(cspf.TaskCount);//task count
					for (int i = 0; i < ProcessingConst.TASK_NUM; i++)
                    {
                        if (cspf.HasLine(i))
                        {
                            ProcessingTask task = cspf.mTaskTable[i];
                            w.Write(i);//taskIndex
                            List<ItemIdCount> itemList = task.itemList;
                            int count = itemList.Count;
                            w.Write(count);//itemcount
                            for (int j = 0; j < itemList.Count; j++)
                            {
                                w.Write(itemList[j].protoId);
                                w.Write(itemList[j].count);
                            }
							w.Write(task.runCount);
                            w.Write(task.m_CurTime);
                            w.Write(task.m_Time);
                        }
                    }
                }
                break;
            case CSConst.dtTrade:
                {
                    CSTradeData cstd = kvp.Value as CSTradeData;
                    _writeCSObjectData(w, cstd);
//                    w.Write(cstd.ttiiDic.Values.Count);
//                    foreach (TownTradeItemInfo ttii in cstd.ttiiDic.Values)
//                    {
//                        w.Write(ttii.pos.x);
//                        w.Write(ttii.pos.y);
//                        w.Write(ttii.m_CurTime);
//                        w.Write(ttii.m_Time);
//                        w.Write(ttii.csti.id);
//                        w.Write(ttii.needItems.Count);
//                        foreach (TradeObj to in ttii.needItems)
//                        {
//                            w.Write(to.protoId);
//                            w.Write(to.count);
//                            w.Write(to.max);
//                        }
//                        w.Write(ttii.rewardItems.Count);
//                        foreach (TradeObj to in ttii.rewardItems)
//                        {
//                            w.Write(to.protoId);
//                            w.Write(to.count);
//                            w.Write(to.max);
//                        }
//                    }
					w.Write(cstd.mShopList.Count);
					foreach(KeyValuePair<int,stShopData> shopItem in cstd.mShopList){
						w.Write(shopItem.Key);
						w.Write(shopItem.Value.ItemObjID);
						w.Write(shopItem.Value.CreateTime);
					}
                }
                break;
            case CSConst.dtCheck:
                {
                    CSCheckData cscd = kvp.Value as CSCheckData;
                    _writeCSObjectData(w, cscd);
                    _writeCSCheckData(w,cscd);
                }
                break;
            case CSConst.dtTreat:
                {
                    CSTreatData cstd = kvp.Value as CSTreatData;
                    _writeCSObjectData(w, cstd);
                    _writeCSTreatData(w,cstd);
                }
                break;
            case CSConst.dtTent:
                {
                    CSTentData cstd = kvp.Value as CSTentData;
                    _writeCSObjectData(w, cstd);
                    _writeCSTentData(w, cstd);
                }
                break;
            case CSConst.dtTrain:
                {
                    CSTrainData cstd = kvp.Value as CSTrainData;
                    _writeCSObjectData(w, cstd);
                    _writeCSTrainData(w, cstd);
                }
                break;
			default:
				break;
			}
		}
		
		w.Write(m_PersonnelDatas.Count);
        if (debugSwitch) Debug.Log("<color=yellow>" + "m_PersonnelDatas.Count: " + m_PersonnelDatas.Count + "</color>");
		foreach (KeyValuePair<int, CSPersonnelData> kvp in m_PersonnelDatas)
		{
			w.Write(kvp.Value.ID);
			w.Write(kvp.Value.dType);
			w.Write(kvp.Value.m_State);
			w.Write(kvp.Value.m_DwellingsID);
			w.Write(kvp.Value.m_WorkRoomID);
			w.Write(kvp.Value.m_Occupation);
			w.Write(kvp.Value.m_WorkMode);
			w.Write(kvp.Value.m_GuardPos.x);
			w.Write(kvp.Value.m_GuardPos.y);
			w.Write(kvp.Value.m_GuardPos.z);
            w.Write(kvp.Value.m_ProcessingIndex);
            w.Write(kvp.Value.m_IsProcessing);
            w.Write(kvp.Value.m_TrainerType);
            w.Write(kvp.Value.m_TrainingType);
            w.Write(kvp.Value.m_IsTraining);
		}

        //version 001
        w.Write(treatmentList.Count);
        for (int i = 0; i < treatmentList.Count; i++)
        {
            treatmentList[i]._writeTreatmentData(w);
        }

		//2016-10-19 18:36:48
		w.Write(addedTradeNpc.Count);
		for(int i=0;i<addedTradeNpc.Count;i++){
			w.Write(addedTradeNpc[i]);
		}
		w.Write(addedStoreId.Count);
		for(int i=0;i<addedStoreId.Count;i++){
			w.Write(addedStoreId[i]);
		}

		w.Write(colonyMoney);
	}


    #region WRITE_DATA
    public void _writeCSCheckData(BinaryWriter w,CSCheckData csd)
    {
        w.Write(csd.npcIds.Count);
        for (int i = 0; i < csd.npcIds.Count; i++)
        {
            w.Write(csd.npcIds[i]);
        }
        w.Write(csd.m_CurTime);
        w.Write(csd.m_Time);
        w.Write(csd.isNpcReady);
        w.Write(csd.occupied);
    }
    public void _writeCSTreatData(BinaryWriter w,CSTreatData csd)
    {
        w.Write(csd.npcIds.Count);
        for (int i = 0; i < csd.npcIds.Count; i++)
        {
            w.Write(csd.npcIds[i]);
        }
        w.Write(csd.m_ObjID);
        w.Write(csd.m_CurTime);
        w.Write(csd.m_Time);
        w.Write(csd.isNpcReady);
        w.Write(csd.occupied);
    }
    public void _writeCSTentData(BinaryWriter w,CSTentData csd)
    {
        w.Write(csd.npcIds.Count);
        for (int i = 0; i < csd.npcIds.Count; i++)
        {
            w.Write(csd.npcIds[i]);
        }
        for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
        {
            w.Write(csd.allSickbeds[i].npcId);
            w.Write(csd.allSickbeds[i].m_CurTime);
            w.Write(csd.allSickbeds[i].m_Time);
            w.Write(csd.allSickbeds[i].isNpcReady);
            w.Write(csd.allSickbeds[i].IsOccupied);
        }
    }

    public void _writeCSTrainData(BinaryWriter w, CSTrainData csd)
    {
        int iCount = csd.instructors.Count;
        w.Write(iCount);
        for (int i = 0; i < iCount; i++)
        {
            w.Write(csd.instructors[i]);
        }
        int tCount = csd.trainees.Count;
        w.Write(tCount);
        for (int i = 0; i < tCount; i++)
        {
            w.Write(csd.trainees[i]);
        }
        w.Write(csd.instructorNpcId);
        w.Write(csd.traineeNpcId);
        w.Write(csd.trainingType);

        int lCount = csd.LearningSkillIds.Count;
        w.Write(lCount);
        for (int i = 0; i < lCount; i++)
        {
            w.Write(csd.LearningSkillIds[i]);
        }
        w.Write(csd.m_CurTime);
        w.Write(csd.m_Time);
    }
    #endregion



    #region READ_DATA

    /// <summary>
	/// <CSVD> Read various Personnel Data for various version
	/// </summary>
	void _readPesonnelData (BinaryReader r, int version)
	{
		//CSPersonnelData cspd = null;
		
        if (version >= CSDataMgr.VERSION000)
        {
			int rcnt = r.ReadInt32();
            if (debugSwitch) Debug.Log("<color=yellow>" + "m_PersonnelDatas.Count: " + rcnt + "</color>");
			for (int i = 0; i < rcnt; i++)
			{
				CSPersonnelData cspd = new CSPersonnelData();
				cspd.ID 			= r.ReadInt32();
				cspd.dType			= r.ReadInt32();
				cspd.m_State		= r.ReadInt32();
				cspd.m_DwellingsID	= r.ReadInt32();
				cspd.m_WorkRoomID	= r.ReadInt32();
				cspd.m_Occupation	= r.ReadInt32();
				cspd.m_WorkMode		= r.ReadInt32();
				cspd.m_GuardPos     = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                cspd.m_ProcessingIndex = r.ReadInt32();
                cspd.m_IsProcessing = r.ReadBoolean();
                if(version>=CSDataMgr.VERSION002)
                {
                    cspd.m_TrainerType = r.ReadInt32();
                    cspd.m_TrainingType = r.ReadInt32();
                    cspd.m_IsTraining = r.ReadBoolean();
                }
				m_PersonnelDatas.Add(cspd.ID, cspd);
			}
		}
	}

	/// <summary>
	/// <CSVD>Read various Assembly data for various version
	/// </summary>
	CSAssemblyData _readAssemblyData (BinaryReader r, int version)
	{
		CSAssemblyData  cssa = null;
		if (version >= CSDataMgr.VERSION000)
        {
			    cssa = new CSAssemblyData();
			    _readCSObjectData(r, cssa, version);
			
			    cssa.m_ShowShield	    = r.ReadBoolean();
			    cssa.m_Level			= r.ReadInt32();
			    cssa.m_UpgradeTime		= r.ReadSingle();
			    cssa.m_CurUpgradeTime	= r.ReadSingle(); 
			    cssa.m_TimeTicks		= r.ReadInt64();
		}
		if(version>= CSDataMgr.VERSION007){
			cssa.m_MedicineResearchState = r.ReadDouble();
			cssa.m_MedicineResearchTimes = r.ReadInt32();
		}
		return cssa;
	}

	/// <summary>
	/// <CSVD> Read various coal power plant Data for various version
	/// </summary>
	CSPPCoalData _readPPCoalData (BinaryReader r, int version)
	{
		CSPPCoalData csppc = null;
		if (version >= CSDataMgr.VERSION000)
        {
			csppc = new CSPPCoalData();;
			_readCSObjectData(r, csppc, version);
			if(version>=CSDataMgr.VERSION004)
				csppc.bShowElectric = r.ReadBoolean();
			int cnt = r.ReadInt32();
			for (int j = 0; j < cnt; j++)
			{
				csppc.m_ChargingItems.Add(r.ReadInt32(), r.ReadInt32());
			}
			csppc.m_CurWorkedTime 	= r.ReadSingle();
			csppc.m_WorkedTime		= r.ReadSingle();
		}

		return csppc;
	}
	CSPPFusionData _readFusionData(BinaryReader r, int version){
		CSPPFusionData csppf = null;
		csppf = new CSPPFusionData();;
		_readCSObjectData(r, csppf, version);
		csppf.bShowElectric = r.ReadBoolean();
		int cnt = r.ReadInt32();
		for (int j = 0; j < cnt; j++)
		{
			csppf.m_ChargingItems.Add(r.ReadInt32(), r.ReadInt32());
		}
		csppf.m_CurWorkedTime 	= r.ReadSingle();
		csppf.m_WorkedTime		= r.ReadSingle();
		
		return csppf;
	}
	/// <summary>
	/// <CSVD> Read various solar power plant Data for various version
	/// </summary>
	CSPPSolarData _readPPSolarData (BinaryReader r, int version)
	{
		CSPPSolarData cspp = null;

		if (version >= CSDataMgr.VERSION000)
        {
			    cspp = new CSPPSolarData();
			    _readCSObjectData(r, cspp, version);
			
			    int cnt = r.ReadInt32();
			    for (int j = 0; j < cnt; j++)
			    {
				    cspp.m_ChargingItems.Add(r.ReadInt32(), r.ReadInt32());
			    }
		}

		return cspp;
	}

	/// <summary>
	/// <CSVD> Read various Storage  Data for various version
	/// </summary>
	CSStorageData _readStorageData (BinaryReader r, int version)
	{
		CSStorageData cssd = null;
        if (version >= CSDataMgr.VERSION000)
        {
			    cssd = new CSStorageData();
			    _readCSObjectData(r,cssd, version);
			
			    int itemCnt = r.ReadInt32();
			    for (int j = 0; j < itemCnt; j++)
				    cssd.m_Items.Add(r.ReadInt32(), r.ReadInt32());
		}

		return cssd;
	}

	/// <summary>
	/// <CSVD> Read various Enhance Data for various version
	/// </summary>
	CSEnhanceData _readEnhanceData (BinaryReader r, int version)
	{
		CSEnhanceData cseh = null;
        if (version >= CSDataMgr.VERSION000)
        {
            cseh = new CSEnhanceData();
            _readCSObjectData(r, cseh, version);

            cseh.m_ObjID = r.ReadInt32();
            cseh.m_CurTime = r.ReadSingle();
            cseh.m_Time = r.ReadSingle();
        }

		return cseh;
	}

	/// <summary>
	/// <CSVD> Read various Repair Data for various version
	/// </summary>
	CSRepairData _readRepairData (BinaryReader r, int version)
	{
		CSRepairData csrd = null;
		if (version >= CSDataMgr.VERSION000)
        {
			csrd = new CSRepairData();
			_readCSObjectData(r, csrd, version);
			
			csrd.m_ObjID		= r.ReadInt32();
			csrd.m_CurTime		= r.ReadSingle();
			csrd.m_Time			= r.ReadSingle();
		}

		return csrd;
	}

	/// <summary>
	/// <CSVD> Read various Recycle Data for various version
	/// </summary>
	CSRecycleData _readRecycleData (BinaryReader r, int version)
	{
		CSRecycleData csrd = null;
        if (version >= CSDataMgr.VERSION000)
        {
			    csrd = new CSRecycleData();
			    _readCSObjectData(r, csrd, version);
			
			    csrd.m_ObjID		= r.ReadInt32();
			    csrd.m_CurTime		= r.ReadSingle();
			    csrd.m_Time			= r.ReadSingle();
		}

		return csrd;
	}

	/// <summary>
	/// <CSVD> Read various Dwellings Data for various version
	/// </summary>
	CSDwellingsData _readDwellingsData (BinaryReader r, int version)
	{
		CSDwellingsData csdw = null;
        if (version >= CSDataMgr.VERSION000)
        {
			    csdw = new CSDwellingsData();
			    _readCSObjectData(r, csdw, version);
		}

		return csdw;
	}

	/// <summary>
	/// <CSVD> Read various Farm Data for various version
	/// </summary>
	CSFarmData _readFarmData (BinaryReader r, int version)
	{
		CSFarmData csfd = null;
        if (version >= CSDataMgr.VERSION000)
        {
			    csfd = new CSFarmData();
			    _readCSObjectData(r, csfd, version);
			    int count = r.ReadInt32();
			    for (int j = 0; j < count; j++)
			    {
				    csfd.m_PlantSeeds.Add(r.ReadInt32(), r.ReadInt32());
			    }
			
			    count = r.ReadInt32();
			    for (int j = 0; j < count; j++)
			    {
				    csfd.m_Tools.Add(r.ReadInt32(), r.ReadInt32());
			    }
			
			    csfd.m_AutoPlanting = r.ReadBoolean();
			    csfd.m_SequentialPlanting = r.ReadBoolean();
		}

		return csfd;
	}


	/// <summary>
	/// reads the factory data.
	/// </summary>
	CSFactoryData _readFactoryData (BinaryReader r, int version)
	{
		CSFactoryData csfd = null;
        
        if (version >= CSDataMgr.VERSION000)
        {
                csfd = new CSFactoryData();
			    _readCSObjectData(r, csfd, version);
			    int count = r.ReadInt32();
			    for (int i = 0; i < count; i++)
			    {
				    CompoudItem item = new CompoudItem();
				    item.curTime = r.ReadSingle();
				    item.time = r.ReadSingle();
				    item.itemID = r.ReadInt32();
				    item.itemCnt = r.ReadInt32();
				    csfd.m_CompoudItems.Add(item);
			    }
		}

		return csfd;
	}

    CSProcessingData _readProcessingData(BinaryReader r, int version)
    {
        CSProcessingData data = null;
        if (version >= CSDataMgr.VERSION000)
        {
                data = new CSProcessingData();
                _readCSObjectData(r, data, version);
                data.isAuto = r.ReadBoolean();
                int count = r.ReadInt32();//task count
                for (int i = 0; i < count; i++)
                {
                    int index = r.ReadInt32();//task index
                    ProcessingTask task = new ProcessingTask();
                    int itemListCount = r.ReadInt32();//item count
                    for(int j = 0;j<itemListCount;j++)
                    {
                        ItemIdCount po = new ItemIdCount();
                        po.protoId = r.ReadInt32();
                        po.count = r.ReadInt32();
                        task.itemList.Add(po);
				}
				if(version>= CSDataMgr.VERSION005)
					task.runCount = r.ReadInt32();
                task.m_CurTime = r.ReadSingle();
                task.m_Time = r.ReadSingle();
                data.mTaskTable[index] = task;
                }
        }
        return data;
    }


    CSTradeData _readTradeData(BinaryReader r, int version)
    {
        CSTradeData data = null;
        if (version >= CSDataMgr.VERSION000)
        {
                data = new CSTradeData();
                _readCSObjectData(r, data, version);
		}
		//if(version>=CSDataMgr.VERSION001&&version<CSDataMgr.VERSION009){
  //              int count = r.ReadInt32();
  //              for (int i = 0; i < count; i++)
  //              {
  //                  IntVector2 pos = new IntVector2(r.ReadInt32(), r.ReadInt32());
  //                  TownTradeItemInfo ttii = new TownTradeItemInfo (pos);
  //                  ttii.m_CurTime = r.ReadSingle();
  //                  ttii.m_Time = r.ReadSingle();
  //                  ttii.csti= CSTradeInfoData.GetData(r.ReadInt32());
  //                  int needItemsCount = r.ReadInt32();
  //                  for (int m = 0; m < needItemsCount; m++)
  //                  {
  //                      ttii.needItems.Add(new TradeObj(r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));
  //                  }
  //                  int rewardItemsCount = r.ReadInt32();
  //                  for (int m = 0; m < rewardItemsCount; m++)
  //                  {
  //                      ttii.rewardItems.Add(new TradeObj(r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));
  //                  }
  //              }
		//}
		if(version>=CSDataMgr.VERSION009){
			int count = r.ReadInt32();
			for(int i=0;i<count;i++){
				int storeId = r.ReadInt32 ();
				int itemObjId = r.ReadInt32();
				double createTime = r.ReadDouble();
				stShopData shopData = new stShopData (itemObjId,createTime);
				data.mShopList.Add (storeId,shopData);
			}
		}
		return data;
    }


    CSTrainData _readTrainData(BinaryReader r, int version)
    {
        CSTrainData data = null;
        if (version >= CSDataMgr.VERSION002)
        {
                data = new CSTrainData();
                _readCSObjectData(r, data, version);
                //--to do:
                int iCount = r.ReadInt32();
                for (int i = 0; i < iCount; i++)
                {
                    data.instructors.Add(r.ReadInt32());
                }
                int tCount = r.ReadInt32();
                for (int i = 0; i < tCount; i++)
                {
                    data.trainees.Add(r.ReadInt32());
                }
                data.instructorNpcId = r.ReadInt32();
                data.traineeNpcId = r.ReadInt32();
                data.trainingType = r.ReadInt32();

                int lCount = r.ReadInt32();
                for (int i = 0; i < lCount; i++)
                {
                    data.LearningSkillIds.Add(r.ReadInt32());
                }
                data.m_CurTime = r.ReadSingle();
                data.m_Time = r.ReadSingle();
        }
        return data;
    }

    CSCheckData _readCheckData(BinaryReader r, int version)
    {
        CSCheckData data = null;
        if (version >= CSDataMgr.VERSION000)
        {
            data = new CSCheckData();
            _readCSObjectData(r, data, version);
            int npcCount = r.ReadInt32();
            for (int j = 0; j < npcCount; j++)
            {
                data.npcIds.Add(r.ReadInt32());
            }
            data.m_CurTime = r.ReadSingle();
            data.m_Time = r.ReadSingle();
            data.isNpcReady = r.ReadBoolean();
            data.occupied = r.ReadBoolean();
        }
        return data;
    }

    CSTreatData _readTreatData(BinaryReader r, int version)
    {
        CSTreatData data = null;
        if (version >= CSDataMgr.VERSION000)
        {
            data = new CSTreatData();
            _readCSObjectData(r, data, version);                
            int npcCount = r.ReadInt32();
            for (int j = 0; j < npcCount; j++)
            {
                data.npcIds.Add(r.ReadInt32());
            }
            data.m_ObjID = r.ReadInt32();
            data.m_CurTime = r.ReadSingle();
            data.m_Time = r.ReadSingle();
            data.isNpcReady = r.ReadBoolean();
            data.occupied = r.ReadBoolean();
        }
        return data;
    }

    CSTentData _readTentData(BinaryReader r, int version)
    {
        CSTentData data = null;
        if (version >= CSDataMgr.VERSION000)
        {
            data = new CSTentData();
            _readCSObjectData(r, data, version);
            int npcCount = r.ReadInt32();
            for (int j = 0; j < npcCount; j++)
            {
                data.npcIds.Add(r.ReadInt32());
            }
            for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
            {
                Sickbed sb = new Sickbed ();
                sb.npcId = r.ReadInt32();
                sb.m_CurTime = r.ReadSingle();
                sb.m_Time = r.ReadSingle();
                sb.isNpcReady = r.ReadBoolean();
                sb.occupied = r.ReadBoolean();
                data.allSickbeds[i] = sb;
            }
        }
        return data;
    }
	/// <summary>
	/// <CSVD> Read various Object Data for various version
	/// </summary>
	void _readCSObjectData (BinaryReader r, CSObjectData csod, int version)
	{
		if(version < CSDataMgr.VERSION000)
        {
			csod.ID 		  		= r.ReadInt32();
			csod.ItemID				= r.ReadInt32();
			csod.m_Alive			= r.ReadBoolean();
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
        }

        if (version >= CSDataMgr.VERSION000)
        {
			csod.ID 		  		= r.ReadInt32();
			csod.ItemID				= r.ReadInt32();
			csod.m_Alive			= r.ReadBoolean();
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
		}
	}
	
	void _writeCSObjectData (BinaryWriter w, CSObjectData csod)
	{
		w.Write(csod.ID);
		w.Write(csod.ItemID);
		w.Write(csod.m_Alive);
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

	#endregion
}
