using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CSRecord;

using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtFollow;
using System.IO;


public class CSMgCreator : CSCreator 
{
	#region  ABOUT_ENTITY
	private CSAssembly m_Assembly;
	public  override CSAssembly Assembly		{ get { return m_Assembly; } }
	
	private Dictionary<int, CSCommon>	m_CommonEntities;

	public Dictionary<int, CSBuildingLogic> allBuildingLogic = new Dictionary<int, CSBuildingLogic>();

    public List<CSTreatment> m_TreatmentList
    {
        get { return m_DataInst.treatmentList; }
    }
    public void InitMultiCSTreatment(List<CSTreatment> cstList)
    {
        m_DataInst.treatmentList = cstList;
    }

	public void InitMultiData(byte[] packData){
		ParseData(packData);
	}

	public void ParseData(byte[] packData)
	{
		using (MemoryStream ms = new MemoryStream(packData))
		using (BinaryReader reader = new BinaryReader(ms))
		{
			ColonyMoney = BufferHelper.ReadInt32(reader);
			int sIdCount = BufferHelper.ReadInt32(reader);
			List<int> storeIdList = new List<int> ();
			for(int i=0;i<sIdCount;i++){
				storeIdList.Add(BufferHelper.ReadInt32(reader));
			}
			m_DataInst.addedStoreId = storeIdList;
		}
	}
	// Clod
	public CSClod  m_Clod;

    //Monster Siege
    private bool m_IsSiege;
    public bool IsSiege { get { return m_IsSiege; } }

	public delegate void UpdateAddedStoreIdEvent();
	public event UpdateAddedStoreIdEvent UpdateAddedStoreIdListener;

	public delegate void UpdateMoneyEvent();
	public event UpdateMoneyEvent UpdateMoneyListener;
	
	public delegate void StoreIdAddedEvent(List<int> storeIdList);
	public event StoreIdAddedEvent StoreIdAddedListener;

	public void RegistStoreIdAddedEvent(StoreIdAddedEvent addEvent){
		StoreIdAddedListener-=addEvent;
		StoreIdAddedListener+=addEvent;
	}
	public void UnRegistStoreIdAddedEvent(StoreIdAddedEvent addEvent){
		StoreIdAddedListener-=addEvent;
	}

	public void RegistUpdateAddedStoreIdEvent(UpdateAddedStoreIdEvent addEvent){
		UpdateAddedStoreIdListener-=addEvent;
		UpdateAddedStoreIdListener+=addEvent;
	}
	public void UnRegistUpdateAddedStoreIdEvent(UpdateAddedStoreIdEvent addEvent){
		UpdateAddedStoreIdListener-=addEvent;
	}

	public void RegistUpdateMoneyEvent(UpdateMoneyEvent addEvent){
		UpdateMoneyListener-=addEvent;
		UpdateMoneyListener+=addEvent;
	}
	public void UnRegistUpdateMoneyEvent(UpdateMoneyEvent addEvent){
		UpdateMoneyListener-=addEvent;
	}

	public int ColonyMoney{
		set{
			if(m_DataInst!=null)
				m_DataInst.colonyMoney=value;
		}
		get{
			if(m_DataInst==null)
				return -1;
			return m_DataInst.colonyMoney;}
	}
	// 
//	private CSSimulatorMgr m_SimulatorMgr;
//	public CSSimulatorMgr  SimulatorMgr 	{ get { return m_SimulatorMgr;} }
	
	// <CETC> Create Managed Entity
	public override int CreateEntity (CSEntityAttr attr, out CSEntity outEnti)
	{
		outEnti = null;
		if (attr.m_Type == CSConst.etAssembly)
		{
            Vector3 travelPos = Vector3.zero;

            CSBuildingLogic csb = attr.m_LogicObj == null ? null : attr.m_LogicObj.GetComponent<CSBuildingLogic>();
            //lz-2017.01.17 基地逻辑预制物体加载成功并且有传送点就用配置的传送点的位置
            if (csb != null && csb.travelTrans != null)
                travelPos = csb.travelTrans.position;
            //lz-2017.01.17 否则就用基地核心向上偏移两米的位置
            else
                travelPos = m_Assembly.Position + new Vector3(0, 2, 0);

            if (m_Assembly == null)
            {
                if (GameConfig.IsMultiMode && attr.m_ColonyBase == null)
                {
                    return CSConst.rrtUnkown;
                }
				outEnti = _createEntity(attr);
				m_Assembly.ChangeState();

                if (CSMain.s_MgCreator == this)
				{
                    ColonyLabel.Remove(travelPos);
                    new ColonyLabel(travelPos);
                }
			}
			else if (m_Assembly.ID == attr.m_InstanceId)
			{
				outEnti = m_Assembly;
                outEnti.gameLogic = attr.m_LogicObj;
				outEnti.gameObject = attr.m_Obj;
				outEnti.Position = attr.m_Pos;
				outEnti.ItemID = attr.m_protoId;
				outEnti.Bound = attr.m_Bound;
                m_Assembly.Data.m_Alive = true;
                if (CSMain.s_MgCreator == this)
                {
                    ColonyLabel.Remove(travelPos);
                    new ColonyLabel(travelPos);
                }
			}
			else
			{
				if (m_Assembly != null)
					return CSConst.rrtHasAssembly;
			}
				
		}
		else
		{
			if (m_CommonEntities.ContainsKey(attr.m_InstanceId))
			{
                outEnti = m_CommonEntities[attr.m_InstanceId];
                outEnti.gameLogic = attr.m_LogicObj;
				outEnti.gameObject = attr.m_Obj;
				outEnti.Position = attr.m_Pos;
				outEnti.ItemID = attr.m_protoId;
				outEnti.BaseData.m_Alive = true;
				outEnti.Bound = attr.m_Bound;
            }
            else
            {
                if (m_Assembly == null)
                    return CSConst.rrtNoAssembly;

                if (!m_Assembly.InRange(attr.m_Pos))
                    return CSConst.rrtOutOfRadius;

                // Is powerplant ?
                CSConst.ObjectType obj_type;
//                if ((attr.m_Type & CSConst.etPowerPlant) != 0)
//                    obj_type = CSConst.ObjectType.PowerPlant;
//                else
                obj_type = (CSConst.ObjectType)attr.m_Type;

                if (!m_Assembly.OutOfCount(obj_type))
                    return CSConst.rrtOutOfRange;

                if (GameConfig.IsMultiMode && attr.m_ColonyBase == null)
                {
                    return CSConst.rrtUnkown;
                }
                outEnti = _createEntity(attr);
                CSCommon csc = outEnti as CSCommon;
                m_Assembly.AttachCommonEntity(csc);
            }
		}
        ExecuteEvent(CSConst.cetAddEntity, outEnti);
		return CSConst.rrtSucceed;
	}
	
	public CSEntity _createEntity (CSEntityAttr attr)
	{
			// Assembly
		if (attr.m_Type == CSConst.etAssembly)
		{
			m_Assembly = new CSAssembly();
			m_Assembly.m_Info = CSInfoMgr.m_AssemblyInfo;
            m_Assembly.ID = attr.m_InstanceId;
            m_Assembly.gameLogic = attr.m_LogicObj;
			m_Assembly.gameObject = attr.m_Obj;
			m_Assembly.m_Creator = this;

            //multiMode only
            m_Assembly._ColonyObj = attr.m_ColonyBase;

			m_Assembly.CreateData();
			m_Assembly.Position = attr.m_Pos;
			m_Assembly.ItemID = attr.m_protoId;
			m_Assembly.Bound = attr.m_Bound;
			m_Assembly.Data.m_Alive = true;
			m_Assembly.InitErodeMap(attr.m_Pos, m_Assembly.Radius);
				
			m_Timer.Tick = m_Assembly.Data.m_TimeTicks;
				
				
			return m_Assembly;
		}
		else
		{
			CSCommon csc = _CreateCommon(attr.m_Type);
				
			csc.ID = attr.m_InstanceId;

            //multiMode only
			csc._ColonyObj = attr.m_ColonyBase;

			csc.CreateData();
			csc.Position = attr.m_Pos;
            csc.m_Power = attr.m_Power;
            csc.gameLogic = attr.m_LogicObj;
			csc.gameObject = attr.m_Obj;
			csc.ItemID = attr.m_protoId;
			csc.Bound = attr.m_Bound;
			csc.BaseData.m_Alive = true;

			m_CommonEntities.Add(csc.ID, csc);
			return csc;
		}
	}
	
	CSCommon _CreateCommon(int type)
	{
		CSCommon csc = null;
		switch (type)
		{
		case CSConst.etStorage:
			csc = new CSStorage();
			CSStorage css = csc as CSStorage;
			css.m_Info = CSInfoMgr.m_StorageInfo;
			css.m_Creator = this;
			css.m_Package.ExtendPackage(CSInfoMgr.m_StorageInfo.m_MaxItem, CSInfoMgr.m_StorageInfo.m_MaxEquip, CSInfoMgr.m_StorageInfo.m_MaxRecource,CSInfoMgr.m_StorageInfo.m_MaxArmor);
			break;
		case CSConst.etEnhance:
			csc = new CSEnhance();
			CSEnhance csen = csc as CSEnhance;
			csen.m_Creator = this;
			csen.m_Info	= CSInfoMgr.m_EnhanceInfo;
			break;
		case CSConst.etRepair:
			csc = new CSRepair();
			CSRepair csr = csc as CSRepair;
			csr.m_Creator = this;
			csr.m_Info = CSInfoMgr.m_RepairInfo;
			break;
		case CSConst.etRecyle:
			csc = new CSRecycle();
			CSRecycle csrc = csc as CSRecycle;
			csrc.m_Creator = this;
			csrc.m_Info	= CSInfoMgr.m_RecycleInfo;
			break;
		case CSConst.etDwelling:
			csc = new CSDwellings();
			CSDwellings csd = csc as CSDwellings;
			csd.m_Creator = this;
			csd.m_Info = CSInfoMgr.m_DwellingsInfo;
			
			// Find the npc to live
            if (!PeGameMgr.IsMulti)
            {
                int index = 0;
                foreach (KeyValuePair<int, CSCommon> kvp in m_CommonEntities)
                {
                    if (index >= csd.m_NPCS.Length)
                        break;

                    if (kvp.Value.m_Type == CSConst.etDwelling)
                    {
                        CSDwellings dwellings = kvp.Value as CSDwellings;
                        if (dwellings.IsRunning)
                            continue;
                        for (int i = 0; i < dwellings.m_NPCS.Length; i++)
                        {
                            if (dwellings.m_NPCS[i] != null)
                            {
                                csd.AddNpcs(dwellings.m_NPCS[i]);
                                dwellings.RemoveNpc(dwellings.m_NPCS[i]);
                                index++;
                            }
                        }
                    }
                }
            }
			break;
		case CSConst.etppCoal:
			csc = new CSPPCoal();
			CSPPCoal cscppc = csc as CSPPCoal;
			cscppc.m_Creator = this;
			cscppc.m_Power = 10000;
			cscppc.m_RestPower = 10000;
			cscppc.m_Info = CSInfoMgr.m_ppCoal;
			break;
		case CSConst.etppSolar:
			csc = new CSPPSolar();
			CSPPSolar cspps = csc as CSPPSolar;
			cspps.m_Creator = this;
			cspps.m_Power = 10000;
			cspps.m_RestPower = 10000;
			cspps.m_Info = CSInfoMgr.m_ppCoal;
			break;
		case CSConst.etFarm:
			csc = new CSFarm();
			csc.m_Creator = this;
			csc.m_Info = CSInfoMgr.m_FarmInfo;
			break;
		case CSConst.etFactory:
			csc = new CSFactory();
			csc.m_Creator = this;
			csc.m_Info = CSInfoMgr.m_FactoryInfo;
			break;
        case CSConst.etProcessing:
            csc = new CSProcessing(this);
            CSProcessing csp = csc as CSProcessing;
            csp.m_Info = CSInfoMgr.m_ProcessingInfo;
            break;
        case CSConst.etTrade:
            csc = new CSTrade(this);
            CSTrade cst = csc as CSTrade;
            cst.m_Info = CSInfoMgr.m_Trade;
            break;
        case CSConst.etTrain:
            csc = new CSTraining(this);
            CSTraining cstrain = csc as CSTraining;
            cstrain.m_Info = CSInfoMgr.m_Train;
            break;
        case CSConst.dtCheck:
            csc = new CSMedicalCheck(this);
            CSMedicalCheck csCheck = csc as CSMedicalCheck;
            csCheck.m_Info = CSInfoMgr.m_Check;
            break;
        case CSConst.dtTreat:
            csc = new CSMedicalTreat(this);
            CSMedicalTreat csTreat = csc as CSMedicalTreat;
            csTreat.m_Info = CSInfoMgr.m_Treat;
            break;
        case CSConst.dtTent:
            csc = new CSMedicalTent(this);
            CSMedicalTent csTent = csc as CSMedicalTent;
            csTent.m_Info = CSInfoMgr.m_Tent;
			break;
		case CSConst.dtppFusion:
			csc = new CSPPFusion();
			CSPPFusion csFusion = csc as CSPPFusion;
			csFusion.m_Creator = this;
			csFusion.m_Power = 10000;
			csFusion.m_RestPower = 100000;
			csFusion.m_Info = CSInfoMgr.m_ppFusion;
			break;
		default:
			break;
		}
		
		return csc;
	}
	
	public override CSEntity RemoveEntity (int id, bool bRemoveData = true)
	{
		//if (!GameConfig.IsMultiMode)
		//{
		CSEntity cse = null;
		if (m_Assembly != null && m_Assembly.ID == id)
		{
			cse = m_Assembly;
			m_Assembly.Data.m_Alive = false;
			m_Assembly.RemoveErodeMap();
			
			if (bRemoveData)
				m_Assembly.RemoveData();
            if (CSMain.s_MgCreator == this)
			{
                Vector3 travelPos = Vector3.zero;

                CSBuildingLogic csb = m_Assembly.gameLogic.GetComponent<CSBuildingLogic>();
                //lz-2017.01.17 基地逻辑预制物体加载成功并且有传送点就用配置的传送点的位置
                if (csb != null && csb.travelTrans != null)
                {
                    travelPos = csb.travelTrans.position;
                    ColonyLabel.Remove(travelPos);
                }
                else
                {
                    //lz-2017.01.17 否则就用基地核心向上偏移两米的位置
                    travelPos = m_Assembly.Position + new Vector3(0, 2, 0);
                    ColonyLabel.Remove(travelPos);
                }
            }
			m_Assembly.DestroySelf();
            
			m_Assembly = null;
            ExecuteEvent(CSConst.cetRemoveEntity, cse);
			
		}
		else if (m_CommonEntities.ContainsKey(id))
		{
			cse = m_CommonEntities[id];
			cse.BaseData.m_Alive = false;
			
			if (bRemoveData)
				m_CommonEntities[id].RemoveData();
			m_CommonEntities.Remove(id);
			ExecuteEvent(CSConst.cetRemoveEntity, cse);
			cse.DestroySelf();
		}
		else
			Debug.LogWarning("The Common Entity that you want to Remove is not contained!");
		
		// Simulator
//        if (!GameConfig.IsMultiMode)
//		    m_SimulatorMgr.RemoveSimulator(id, true);
		return cse;
		//}
		//else
		//{
		
		//}
	}

    //Set monster siege
    public void SetSiege(bool value)
    {
        if(m_IsSiege != value)
        {
            m_IsSiege = value;
        }
    }

	public override void RemoveLogic(int id){
		allBuildingLogic.Remove(id);
	}
	public override void AddLogic(int id,CSBuildingLogic csb){
		allBuildingLogic.Add(id,csb);
	}

	public override CSCommon GetCommonEntity (int ID)
	{
		if (m_CommonEntities.ContainsKey(ID))
			return m_CommonEntities[ID];
		
		return null;
	}
	public override int GetCommonEntityCnt ()
	{
		return m_CommonEntities.Count;
	}
	
	public override Dictionary<int, CSCommon> GetCommonEntities ()
	{
		return m_CommonEntities;
	}
	
	public override int CanCreate (int type, Vector3 pos)
	{
		if(RandomDungenMgrData.InDungeon)
			return CSConst.rrtAreaUnavailable;
		if (type == CSConst.etAssembly)
		{
			if (m_Assembly != null)
				return CSConst.rrtHasAssembly;
            int n;
            if (!MissionManager.CanDragAssembly(pos, out n))
            {
                switch (n)
                {
                    case 0:
                        return CSConst.rrtTooCloseToNativeCamp;
                    case 1:
                        return CSConst.rrtTooCloseToNativeCamp1;
                    case 2:
                        return CSConst.rrtTooCloseToNativeCamp2;
                    default:
                        break;
                }
            }
		}
		else
		{
			if (m_Assembly == null)
				return CSConst.rrtNoAssembly;
			
			if (!m_Assembly.InRange(pos))
				return CSConst.rrtOutOfRadius;
			
			// Is powerplant ?
			CSConst.ObjectType obj_type;
//			if ((type & CSConst.etPowerPlant) != 0)
//				obj_type = CSConst.ObjectType.PowerPlant;
//			else
				obj_type = (CSConst.ObjectType)type;
			
			if (!m_Assembly.OutOfCount(obj_type))
				return CSConst.rrtOutOfRange;
		}
		
		return CSConst.rrtSucceed;
	}
	
	#endregion
	
	#region TIMER_SHAFT
	
	private PETimer m_Timer = null;
	
	public override PETimer Timer  { get { return m_Timer; } }
	
	
	#endregion
	
	#region  MANAGE_PERSONNEL

    private List<CSPersonnel> m_MainNpcs;
    public List<CSPersonnel> MainNpcs
    {
        get { return m_RandomNpcs; }
    }
	
	private List<CSPersonnel> m_RandomNpcs;
    public List<CSPersonnel> RandomNpcs
    {
        get { return m_RandomNpcs; }
    }
	
	//	const int c_NpcListCapacity = 108;
	
	private int m_CurPatrolNpcNum;
	public int CurPatrolNpcNum  { get {return m_CurPatrolNpcNum;} }
	
	private int m_SoldierNum;
	public int SoldierNpcNum 	{ get {return m_SoldierNum; } }
	private HashSet<CSPersonnel>   m_Soldiers = new HashSet<CSPersonnel>();
	public HashSet<CSPersonnel>		Soldiers 	{ get { return m_Soldiers; } }
	
	private int m_CurWorkerNum;
	public int CurWorkerNum 	{ get {return m_CurWorkerNum;} }
	
	private int m_WorkerNum;
	public int WorkerNum		{ get {return m_WorkerNum;} }
	private HashSet<CSPersonnel>  m_Workers = new HashSet<CSPersonnel>();
	public HashSet<CSPersonnel>	  Workers  { get { return m_Workers; } }
	
	private int m_FarmerNum;
	public int FarmerNum		{ get {return m_FarmerNum;} }
	private HashSet<CSPersonnel>  m_Farmers = new HashSet<CSPersonnel>();
	public HashSet<CSPersonnel>	  Farmers	{ get { return m_Farmers; } }
	
	private int m_FarmMgNum; 
	public int FarmMgNum		{ get {return m_FarmMgNum;} }
	
	private int m_FarmHarvestNum;
	public int FarmHarvestNum 		{ get {return m_FarmHarvestNum;} }
	
	private int m_FarmPlantNum;
	public int FarmPlantNum			{ get {return m_FarmPlantNum;} }



    private int m_CurProcessorNum = 0;
    public int CurProcessorNum { get { return m_CurProcessorNum; } }

    private int m_ProcessorNum;
    public int ProcessorNum { get { return m_ProcessorNum; } }
    private HashSet<CSPersonnel> m_Processors = new HashSet<CSPersonnel>();
    public HashSet<CSPersonnel> Processors { get { return m_Processors; } }


    private int m_TrainerNum;
    public int TrainerNum { get { return m_TrainerNum; } }
    private HashSet<CSPersonnel> m_Trainers = new HashSet<CSPersonnel>();
    public HashSet<CSPersonnel> Trainers { get { return m_Trainers; } }

	public int GetEmptyBedCnt ()
	{
		if (m_Assembly == null)
			return 0;
		
		CSCommon[] dwellings = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
		if (dwellings.Length == 0)
			return 0;
		
		int empty_cnt = 0;
		foreach (CSCommon csc in dwellings)
		{
			CSDwellings dw = csc as CSDwellings;
			empty_cnt += dw.GetEmptySpace();
		}
		
		return empty_cnt;
		
		
	}
	
	public bool ObjectInPowerPlant(Vector3 pos)
	{
		if (Assembly == null)
			return false;
		
		List<CSCommon> cscs = Assembly.AllPowerPlants;
		for (int i = 0; i < cscs.Count; i++)
		{
			CSPowerPlant pp = cscs[i] as CSPowerPlant;
			if ( pp.InRange(pos) )
				return true;
		}
		
		return false;
	}
	
	public override bool CanAddNpc ()
	{
		if (m_Assembly == null)
		{
			Debug.Log("There is no assembly In the word!");
			return false;
		}
		
		CSCommon[] dwellings = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
		if (dwellings.Length == 0)
		{
			Debug.Log("There is not enough Dwellings for this NPC");
			return false;
		}
		
		foreach (CSCommon csc in dwellings)
		{
			CSDwellings dw = csc as CSDwellings;
			if (dw.HasSpace())
				return true;
		}
		
		return false;
	}
	
	public override bool AddNpc (PeEntity npc, bool bSetPos = false)
	{
		if (npc.IsRecruited())
		{
			Debug.Log("This npc is already a CSPersonnel object!");
			return false;
		}

		
		CSPersonnel personnel = new CSPersonnel();

        personnel.ID = npc.Id;
		personnel.NPC = npc;
		personnel.m_Creator = this;
		personnel.CreateData();
		
		// Have enough dwellings?
		if (personnel.Dwellings == null)
		{
			if (m_Assembly == null)
			{
				Debug.Log("There is no assembly In the word!");
				personnel.RemoveData();
				return false;
			}
			
			CSCommon[] dwellings = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
//			int dwellingsID = 0;
			if (dwellings.Length == 0)
			{
				Debug.Log("There is not enough Dwellings for this NPC");
				personnel.RemoveData();
				return false;
			}
			foreach (CSCommon csc in dwellings)
			{
				CSDwellings dw = csc as CSDwellings;
				if (dw.AddNpcs(personnel))
				{
//					dwellingsID = dw.ID;
					break;
				}
			}
			
			if (personnel.Dwellings == null)
			{
				Debug.Log("There is not enough Dwellings for this NPC");
				personnel.RemoveData();
				return false;
			}
		}


        //RandomNpc
        if (npc.IsRandomNpc())
        {
            m_RandomNpcs.Add(personnel);
			PeEntityCreator.RecruitRandomNpc(npc);
        }
        // Main Npc
        else
        {
            m_MainNpcs.Add(personnel);
			PeEntityCreator.RecruitMainNpc(npc);
        }

        //colony add npc talk
        if (npc.NpcCmpt != null)
            npc.NpcCmpt.SendTalkMsg((int)ENpcTalkType.Conscribe_succeed, 0, ENpcSpeakType.Both);

        //set npc fixpiont near colony
        //if (m_Assembly.Position != Vector3.zero && npc.NpcCmpt != null)
        //    npc.NpcCmpt.SetFixPos(m_Assembly.Position);
                
		ExecuteEventPersonnel(CSConst.cetAddPersonnel, personnel);
		
		_increaseOccupationNum(personnel, personnel.Occupation);
		_increaseWorkModeNum(personnel, personnel.m_WorkMode);

        personnel.UpdateNpcCmpt();
		return true;
	}
	
	public bool AddNpc (PeEntity npc, CSPersonnelData data,bool bSetPos = false)
	{
		
		if (npc.IsRecruited())
		{
			Debug.Log("This npc is already a CSPersonnel object!");
			return false;
		}
		
		
		CSPersonnel personnel = new CSPersonnel();
		
		personnel.ID = npc.Id;
        personnel.NPC = npc;
		personnel.m_Creator = this;
		personnel.CreateData (data);
//		if (personnel.Dwellings == null)
//		{
//			if (m_Assembly == null)
//			{
//				Debug.Log("There is no assembly In the word!");
//				personnel.RemoveData();
//				return false;
//			}
//			
//			CSCommon[] dwellings = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
//			int dwellingsID = 0;
//			if (dwellings.Length == 0)
//			{
//				Debug.Log("There is not enough Dwellings for this NPC");
//				personnel.RemoveData();
//				return false;
//			}
//			foreach (CSCommon csc in dwellings)
//			{
//				CSDwellings dw = csc as CSDwellings;
//				if(dw.ID == data.m_DwellingsID)
//				{
//					if (dw.AddNpcs(personnel))
//					{
//						dwellingsID = dw.ID;
//						break;
//					}
//					else
//						return false;
//				}
//			}
//			
//			if (personnel.Dwellings == null)
//			{
//				Debug.Log("There is not enough Dwellings for this NPC");
//				personnel.RemoveData();
//				return false;
//			}
//		}

//        personnel.InitProcessingState();

//		CSPersonnelObject po = npc.GetGameObject().AddComponent<CSPersonnelObject>();
//		po.m_Personnel = personnel;
//		personnel.m_Object = po;


        //RandomNpc
        if (npc.IsRandomNpc())
        {
            m_RandomNpcs.Add(personnel);
			PeEntityCreator.RecruitRandomNpc(npc);
        }
        // Main Npc
        else 
        {
            m_MainNpcs.Add(personnel);
			PeEntityCreator.RecruitMainNpc(npc);
        }
		


        // Set pos if need
//		if (bSetPos)
//		{			
//			Vector2 randomFactor = Random.insideUnitCircle;
//			Vector3 pos = new Vector3(m_Assembly.Position.x + 5, 1000, m_Assembly.Position.z) 
//				+ new Vector3(randomFactor.x, 0, randomFactor.y) * (m_Assembly.Radius - 10);
//			
//			// Find the Postion Y
//			RaycastHit hit;
//			if (Physics.Raycast(pos, Vector3.down, out hit, 1000, 1 << Pathea.Layer.VFVoxelTerrain))
//			{
//				pos.y = hit.point.y + 1;
//			}
//			else 
//				pos.y = m_Assembly.Position.y + 5;
//			
//			
//		}
//		else
//		{
//			if (personnel.Data.m_State == CSConst.pstPrepare)
//				personnel.Data.m_State = CSConst.pstIdle;
//		}

//		if(personnel.WorkRoom!=null)
//		{
//			if(personnel.WorkRoom.IsRunning){
//				personnel.WorkRoom.UpdateDataToUI();
//			}
//		}
		ExecuteEventPersonnel(CSConst.cetAddPersonnel, personnel);
		
		_increaseOccupationNum(personnel, personnel.Occupation);
		_increaseWorkModeNum(personnel, personnel.m_WorkMode);

        personnel.UpdateNpcCmpt();
		return true;
	}

    #region multiMode
    public bool AddNpcInMultiMode(PeEntity npc,int dwellingId,  bool bSetPos = false)
    {

        if (npc.IsRecruited())
        {
            Debug.Log("This npc is already a CSPersonnel object!");
            return false;
        }


        CSPersonnel personnel = new CSPersonnel();

        personnel.ID = npc.Id;
        personnel.NPC = npc;
        personnel.m_Creator = this;
        personnel.CreateData();

        // Have enough dwellings?
        if (personnel.Dwellings == null)
        {
            if (m_Assembly == null)
            {
                Debug.Log("There is no assembly In the word!");
                personnel.RemoveData();
                return false;
            }

            //CSCommon[] dwellings = m_Assembly.m_BelongObjectsMap[CSConst.ObjectType.Dwelling].ToArray();
            //if (dwellings.Length == 0)
            //{
            //    Debug.Log("There is not enough Dwellings for this NPC");
            //    personnel.RemoveData();
            //    return false;
            //}
            //foreach (CSCommon csc in dwellings)
            //{
            //    CSDwellings dw = csc as CSDwellings;
            //    if (dw.ID == dwellingId)
            //    {
            //        dw.AddNpcs(personnel);
            //        break;
            //    }
            //}
            if (m_CommonEntities.ContainsKey(dwellingId))
            {
                CSDwellings dw = m_CommonEntities[dwellingId] as CSDwellings;
                if (dw != null)
                {
                    dw.AddNpcs(personnel);
                }
            }

           

            if (personnel.Dwellings == null)
            {
                Debug.Log("There is not enough Dwellings for this NPC");
                personnel.RemoveData();
                return false;
            }
        }

        //RandomNpc
        if (npc.IsRandomNpc())
        {
            m_RandomNpcs.Add(personnel);
        }
        // Main Npc
        else
        {
            m_MainNpcs.Add(personnel);
        }

		ExecuteEventPersonnel(CSConst.cetAddPersonnel, personnel);

        _increaseOccupationNum(personnel, personnel.Occupation);
        _increaseWorkModeNum(personnel, personnel.m_WorkMode);

        personnel.UpdateNpcCmpt();
        return true;
    }
    #endregion

    public override void RemoveNpc (PeEntity npc)
	{
		CSPersonnel csp = CSMain.GetColonyNpc(npc.Id);
		if(csp==null){
			Debug.LogWarning("The npc you want to kick out is not a recruit.");
			return;
		}
				
		// Remove data

		csp.RemoveData();
		
		if ( csp.Dwellings != null)
		{
			csp.Dwellings.RemoveNpc(csp);
		}
		
		if ( csp.WorkRoom != null)
		{
			csp.WorkRoom.RemoveWorker(csp);
		}

        //CSBehaveMgr.RemoveQueue(npc.Id);
		m_RandomNpcs.Remove(csp);
		m_MainNpcs.Remove(csp);

		ExecuteEventPersonnel(CSConst.cetRemovePersonnel, csp);
		
		//_decreaseStateNum(po.m_Personnel, po.m_Personnel.State);
		_decreaseOccupationNum(csp, csp.Occupation);
		_decreaseWorkModeNum(csp, csp.m_WorkMode);

		csp.m_Creator = null;
		if(npc.IsRandomNpc())
		{
			PeEntityCreator.ExileRandomNpc(npc);
		}
		else
		{
			PeEntityCreator.ExileMainNpc(npc);
		}
		csp.UpdateNpcCmpt();
		PeNpcGroup.Instance.OnRemoveCsNpc(npc);
	}
	
	public override CSPersonnel[] GetNpcs ()
	{
		List<CSPersonnel> allNpcs = new List<CSPersonnel>();
		allNpcs.AddRange(m_RandomNpcs);
		allNpcs.AddRange(m_MainNpcs);
		return allNpcs.ToArray();
	}

	public int GetNpcCount{
		get{
			return m_RandomNpcs.Count+m_MainNpcs.Count;
		}
	}

    public override CSPersonnel GetNpc(int id)
    {
		if(id<0)
			return null;
        foreach(CSPersonnel csp in m_RandomNpcs){
            if(csp.ID==id)
                return csp;
        }
        foreach(CSPersonnel csp in m_MainNpcs){
            if(csp.ID==id)
                return csp;
        }
        return null;
    }

	#endregion
	
	void _increaseStateNum (CSPersonnel person, int state)
	{
		if (state == CSConst.pstPatrol)
			m_CurPatrolNpcNum++;
		else if (state == CSConst.pstWork)
			m_CurWorkerNum++;
	}
	
	void _decreaseStateNum (CSPersonnel person, int state)
	{
		if (state == CSConst.pstPatrol)
			m_CurPatrolNpcNum = Mathf.Max(0, m_CurPatrolNpcNum-1);
		else if (state == CSConst.pstWork)
			m_CurWorkerNum = Mathf.Max(0, m_CurWorkerNum-1);
	}
	
	void _increaseOccupationNum (CSPersonnel person, int occupation)
	{
		if (occupation == CSConst.potWorker)
		{
			if ( !m_Workers.Contains(person) )
				m_Workers.Add(person);
			m_WorkerNum++;
		}
		else if (occupation == CSConst.potSoldier)
		{
			if ( !m_Soldiers.Contains(person))
				m_Soldiers.Add(person);
			m_SoldierNum++;
		}
		else if (occupation == CSConst.potFarmer)
		{
			if ( !m_Farmers.Contains(person))
				m_Farmers.Add(person);
			m_FarmerNum++;
        }
        else if (occupation == CSConst.potProcessor)
        {
            if (!m_Processors.Contains(person))
                m_Processors.Add(person);
            m_ProcessorNum++;
        }
        else if (occupation == CSConst.potTrainer)
        {
            if (!Trainers.Contains(person))
                Trainers.Add(person);
            m_TrainerNum++;
        }
	}
	
	void _decreaseOccupationNum (CSPersonnel person,  int occupation)
	{
		if (occupation == CSConst.potWorker)
		{
			m_Workers.Remove(person);
			m_WorkerNum = Mathf.Max(0, m_WorkerNum-1);
		}
		else if (occupation == CSConst.potSoldier)
		{
			m_Soldiers.Remove(person);
			m_SoldierNum = Mathf.Max(0, m_SoldierNum-1);
		}
		else if (occupation == CSConst.potFarmer)
		{
			m_Farmers.Remove(person);
			m_FarmerNum = Mathf.Max(0, m_FarmerNum-1);
        }
        else if (occupation == CSConst.potProcessor)
        {
            m_Processors.Remove(person);
            m_ProcessorNum = Mathf.Max(0, m_ProcessorNum - 1);
        }
        else if (occupation == CSConst.potTrainer)
        {
            m_Trainers.Remove(person);
            m_TrainerNum = Mathf.Max(0, m_TrainerNum - 1);
        }
	}
	
	void _increaseWorkModeNum (CSPersonnel person, int mode)
	{
		if (mode == CSConst.pwtFarmForMag)
			m_FarmMgNum++;
		else if (mode == CSConst.pwtFarmForHarvest)
			m_FarmHarvestNum++;
		else if (mode == CSConst.pwtFarmForPlant)
			m_FarmPlantNum++;
	}
	
	void _decreaseWorkModeNum (CSPersonnel person, int mode)
	{
		if (mode == CSConst.pwtFarmForMag)
			m_FarmMgNum = Mathf.Max(0, m_FarmMgNum-1);
		else if (mode == CSConst.pwtFarmForHarvest)
			m_FarmHarvestNum = Mathf.Max(0, m_FarmHarvestNum-1);
		else if (mode == CSConst.pwtFarmForPlant)
			m_FarmPlantNum = Mathf.Max(0, m_FarmPlantNum-1);
	}
	
	#region CALL_BACK
	
	void OnPersonnelChangeState(CSPersonnel person, int prvState)
	{
		if (person.m_Creator != this)
			return;
		
		//_increaseStateNum(person, person.State);
		_decreaseStateNum(person, prvState);
		
	}
	
	void OnPersonnelChangeOccupation(CSPersonnel person, int prvState)
	{
		if (person.m_Creator != this)
			return;
		
		_increaseOccupationNum(person, person.Occupation);
		_decreaseOccupationNum(person, prvState);
		
	}
	
	void OnPersonnelChangeWorkType(CSPersonnel person, int prvState)
	{
		if (person.m_Creator != this)
			return;
		
		_increaseWorkModeNum(person, person.m_WorkMode);
		_decreaseWorkModeNum(person, prvState);
		
	}
	
	#endregion
	
	void OnDestroy()
	{
		m_CommonEntities.Clear();
		m_MainNpcs.Clear();
		m_RandomNpcs.Clear();
		
		CSPersonnel.UnRegisterStateChangedListener(OnPersonnelChangeState);
		CSPersonnel.UnRegisterStateChangedListener(OnPersonnelChangeOccupation);
		CSPersonnel.UnregisterWorkTypeChangedListener(OnPersonnelChangeWorkType);
	}
	
	void Awake ()
	{
		m_Type = CSConst.CreatorType.Managed;
		
		m_CommonEntities = new Dictionary<int, CSCommon>();
		
		m_MainNpcs   = new List<CSPersonnel>();
		m_RandomNpcs = new List<CSPersonnel>();
		
		CSPersonnel.RegisterStateChangedListener(OnPersonnelChangeState);
		CSPersonnel.RegisterOccupaChangedListener(OnPersonnelChangeOccupation);
		CSPersonnel.RegisterWorkTypeChangedListener(OnPersonnelChangeWorkType);
		
		// Init Timer
		m_Timer = new PETimer();
	}
	
	// Use this for initialization
	void Start () 
	{
		if(!GameConfig.IsMultiMode)
		{
			// Create Simulator object
//			GameObject go = new GameObject("Simulator Mgr");
//			m_SimulatorMgr = go.gameObject.AddComponent<CSSimulatorMgr>();
//			m_SimulatorMgr.transform.parent = transform;
//			m_SimulatorMgr.Init(ID);
			
			
			// Create colony object if has  
			Dictionary<int, CSDefaultData> objRecords = m_DataInst.GetObjectRecords();
			foreach (CSDefaultData defData in objRecords.Values)
			{
				CSObjectData objData = defData as CSObjectData;
				
				if (objData != null && objData.ID != CSConst.etID_Farm)
				{
					if (!objData.m_Alive)
						continue;
					
					CSEntityAttr attr = new CSEntityAttr();
					attr.m_InstanceId = objData.ID;
					attr.m_Type = objData.dType;
					attr.m_Pos = objData.m_Position;
					attr.m_protoId = objData.ItemID;
					attr.m_Bound = objData.m_Bounds;
					CSEntity cse = _createEntity(attr);
					
					if (objData.dType == CSConst.dtAssembly)
					{
						CSAssembly csa = cse as CSAssembly;
						csa.ChangeState();
					}
					else
					{
						CSCommon csc = cse as CSCommon;
						if (m_Assembly != null)
							m_Assembly.AttachCommonEntity(csc);
					}
					
					// Create Simulator First
//					CSSimulator sim = null;
//					bool isNew = m_SimulatorMgr.CreateSimulator(objData.ID, out sim);
//					if (isNew)
//					{
//						CSSimulatorAttr cssAttr = new CSSimulatorAttr();
//						cssAttr.m_Hp = cse.m_Info.m_Durability / 0.6f;
//						
//						sim.Init(cssAttr);
//					}
//					
//					sim.Hp = sim.MaxHp * cse.DuraPercent;
//					sim.Position = cse.Position;
//					sim.noticeHpChanged = cse.OnLifeChanged;
				}
			}
			
			//delay load colony npc
			StartCoroutine(InitColonyNpc());


		}
		else
		{

            Debug.Log("<color=red>Creator start! Desc:" + gameObject.name + "</color>");
		}
	}
	
	IEnumerator InitColonyNpc()
	{
		if(GameConfig.IsMultiMode)
			yield break;
		while (Pathea.PeCreature.Instance.mainPlayer == null )
		{
			yield return new WaitForSeconds(0.1f);
		}
		
		//--to do: after Npc Created
        if (EntityMgr.Instance != null)
        {
            Dictionary<int, CSPersonnelData>.ValueCollection personnelRecords = m_DataInst.GetPersonnelRecords();
            foreach (CSPersonnelData pdata in personnelRecords)
            {
                if (pdata.m_WorkRoomID == CSConst.etID_Farm)
                    pdata.m_WorkRoomID = -1;

                PeEntity npc = EntityMgr.Instance.Get(pdata.ID);

                

                //npc.MouseCtrl.MouseEvent.SubscribeEvent(StroyManager.Instance.NpcMouseEventHandler);
                if (npc != null)
                    AddNpc(npc, pdata,true);
            }
        }
		InitColonyAfterReady();

	}
	
	bool IsAdjustingClods = false;
	IEnumerator AdjustClodsProc()
	{
		if (Assembly == null)
			yield break;
		
		IsAdjustingClods = true;
		
		int frame_count = 0;
		
		List<Vector3> remove_list = new List<Vector3>();
		// Get remove list
		foreach (KeyValuePair<IntVec3, ClodChunk> kvp in m_Clod.m_ClodChunks)
		{
			foreach (Vector3 pos in kvp.Value.m_Clods.Values)
			{
				if (!Assembly.InLargestRange(pos))
					remove_list.Add(pos);
				
//				frame_count ++;
			}
			
//			if (frame_count > 1000)
//			{
//				frame_count = 0;
//				yield return 0;
//				
//				if (Assembly == null)
//				{
//					IsAdjustingClods = false;
//					yield break;
//				}
//			}
		}
		
		
		yield return 0;
		
		frame_count = 0;
		// realize remove
		if (Assembly == null)
		{
			IsAdjustingClods = false;
			yield break;
		}
		for (int i = 0; i < remove_list.Count; i++, frame_count++)
		{
			m_Clod.DeleteClod(remove_list[i]);
			
//			if (frame_count > 1000)
//			{
//				frame_count = 0;
//				yield return 0;
//				
//				if (Assembly == null)
//				{
//					IsAdjustingClods = false;
//					yield break;
//				}
//			}
		}
		
		yield return 0;
		IsAdjustingClods = false;
	}
	
	public bool m_PauseTimer = false;
	// Update is called once per frame
	void Update () 
	{
		if (!m_PauseTimer)
		{
			if (m_Assembly != null)
			{
				m_Assembly.Update();
				
				m_Timer.ElapseSpeed = GameTime.Timer.ElapseSpeed;
				m_Timer.Update(Time.deltaTime);
			}
			else
				m_Timer.ElapseSpeed = 0;
		}
		
		foreach (KeyValuePair<int, CSCommon> kvp in m_CommonEntities)
			kvp.Value.Update();
		
		foreach (CSPersonnel personnel in m_MainNpcs)
			personnel.Update();
		
		foreach (CSPersonnel personnel in m_RandomNpcs)
			personnel.Update();
		
		if (m_Clod != null&& Time.frameCount % 8192 == 0 &&Assembly!=null&&Assembly.Farm!=null&&Assembly.Farm.IsRunning)
		{
			if (!IsAdjustingClods)
				StartCoroutine(AdjustClodsProc());
		}
		if(m_Clod != null && Time.frameCount % 8192 == 4096&&Assembly!=null&&Assembly.Farm!=null&&!Assembly.isSearchingClod&&Assembly.Farm.IsRunning&&Assembly.ModelObj!=null)
			StartCoroutine(CSMain.Instance.SearchVaildClodForAssembly(Assembly));
	}

    public CSTreatment FindTreatment(int id,bool needTreat=false)
    {
        foreach (CSTreatment cst in m_TreatmentList)
        {
            if (cst.npcId == id)
            {
                if(needTreat){
                    if (cst.needTreatTimes > 0)
                        return cst;
                }else{
                    return cst;
                }
            }
        }
        return null;
    }

    public List<CSTreatment> FindNpcTreatments(int npcid)
    {
        List<CSTreatment> allTreatment = new List<CSTreatment> ();
        foreach (CSTreatment cst in m_TreatmentList)
        {
            if (cst.npcId == npcid)
                allTreatment.Add(cst);
        }
        return allTreatment;
    }
    public CSTreatment GetTreatment(int abnormalId,int npcId,int needTreatTimes)
    {
        return m_TreatmentList.Find(it => it.abnormalId == abnormalId && it.npcId == npcId && it.needTreatTimes == needTreatTimes);
    }

    public void AddTreatment(List<CSTreatment> cst)
    {
        m_TreatmentList.AddRange(cst);//--to do:
    }

    public void RemoveNpcTreatment(int npcid)
    {
        m_TreatmentList.RemoveAll(it=>it.npcId ==npcid);
    }

    public void UpdateTreatment()
    {
        m_TreatmentList.RemoveAll(it => it.needTreatTimes <= 0);
    }

	public void InitColonyAfterReady(){//afterNpcReady
		if(Assembly!=null)
			Assembly.InitAfterAllDataReady();
		foreach(CSCommon csc in m_CommonEntities.Values){
			csc.InitAfterAllDataReady();
		}
	}

	#region csTrade
	public List<int> AddedStoreId{
		get{
			return m_DataInst.addedStoreId;
		}
	}
	public List<int> AddedNpcId{
		get{
			return m_DataInst.addedTradeNpc;
		}
	}
	public void AddStoreId(List<int> storeIdList){
		List<int> addList = new List<int> ();
		foreach(int id in storeIdList){
			List<int> allIdOfSameItem = ShopRespository.GetAllIdOfSameItem(id);
			int limit = ShopRespository.GetLimitNum(id);
			List<int> idOfSameOrBingerItem = new List<int> ();
			List<int> idOfSmallerItem = new List<int> ();
			foreach(int sid in allIdOfSameItem){
				if(sid==id)
					idOfSameOrBingerItem.Add(sid);
				else if(ShopRespository.GetLimitNum(sid)>=limit)
					idOfSameOrBingerItem.Add(sid);
				else if(ShopRespository.GetLimitNum(sid)<limit)
					idOfSmallerItem.Add(sid);
			}
			bool canAdd = true;
			foreach(int sbid in idOfSameOrBingerItem)
			{
				if(AddedStoreId.Contains(sbid))
				{
					canAdd = false;
					break;
				}
			}
			if(canAdd){
				foreach(int smid in idOfSmallerItem){
					if(AddedStoreId.Contains(smid))
					{
						AddedStoreId.Remove(smid);
						break;
					}
				}
				AddedStoreId.Add(id);
				addList.Add(id);
			}
		}

		if(addList.Count>0&&UpdateAddedStoreIdListener!=null)
			UpdateAddedStoreIdListener();
	}
	public void RefreshMoney(){
		ColonyMoney = ColonyConst.START_MONEY;
		if(UpdateMoneyListener!=null)
			UpdateMoneyListener();
	}
	#endregion
}