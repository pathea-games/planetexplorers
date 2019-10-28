using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CSRecord;
using Pathea.Operate;
using Pathea;
using System.IO;


public class CSMedicalTentConst{
    public const int BED_AMOUNT_MAX = 8;
}
public class Sickbed
{
    public int bedId;
    public CSMedicalTent tentBuilding;
    public PELay bedLay;
    public Transform bedTrans;
    public PeEntity Npc
    {
        set
        {
            if (value != null)
                npcId = value.Id;
            else
                npcId = -1;
            npc = value;
        }
        get
        {
            return npc;
        }
    }
    public PeEntity npc;
    public int npcId=-1;
    public CounterScript cs;
    public float m_CurTime = -1;
    public float m_Time = -1;
    public bool isNpcReady = false;
    public bool occupied = false;
    

    public bool IsOccupied
    {
        get
        {
            return occupied;
        }
    }

    public void OccupyMachine()
    {
        occupied = true;
    }

    public void ReleaseMachine()
    {
        occupied = false;
    }

    public Sickbed() { }
    public Sickbed(CSMedicalTent cst)
    {
        this.tentBuilding = cst;
    }
    public Sickbed(int bedId)
    {
        this.bedId = bedId;
    }

    public CSPersonnel csp
    {
        get
        {
            if (Npc == null)
                return null;
            return tentBuilding.m_Creator.GetNpc(Npc.Id);
        }
    }

    public bool IsPatientReady()
    {
		return Npc != null && npc.GetCmpt<NpcCmpt>().MedicalState == ENpcMedicalState.In_Hospital;;// &&// (bedTrans.position - Npc.position).magnitude < 3 && 
            //isNpcReady;
    }
    

    public float TimeLeft
    {
        get
        {
            if (cs == null)
                return 0;
            else
                return cs.FinalCounter - cs.CurCounter;
        }
    }
    public void StartCounter(float timeNeed)
    {
        StartCounter(0, timeNeed);
    }

    public void StartCounterFromRecord()
    {
        StartCounter(m_CurTime, m_Time);
    }

    public void StartCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;

        if (cs == null)
        {
            cs = CSMain.Instance.CreateCounter("MedicalTent", curTime, finalTime);
        }
        else
        {
            cs.Init(curTime, finalTime);
        }
        //if (!GameConfig.IsMultiMode)
        //{
            cs.OnTimeUp = OnTentFinish;
        //}
    }

    public void StopCounter()
    {
        m_CurTime = -1F;
        m_Time = -1F;
        CSMain.Instance.DestoryCounter(cs);
        cs = null;

    }
    public void Update()
    {
        if (cs != null)
        {
            m_CurTime = cs.CurCounter;
            m_Time = cs.FinalCounter;
        }
        else
        {
            m_CurTime = -1F;
            m_Time = -1F;
        }
    }

    public void OnTentFinish(){
        tentBuilding.OnTentFinish(this);
    }
}

public class CSMedicalTent:CSHealth
{
    public override bool IsDoingJob()
    {
        return IsRunning && IsTent;//--to do
    }
    public CSMedicalTent(CSCreator creator)
    {
        m_Creator = creator;
        m_Type = CSConst.dtTent;

        // Init Workers
        m_Workers = new CSPersonnel[WORKER_AMOUNT_MAX];

        m_WorkSpaces = new PersonnelSpace[WORKER_AMOUNT_MAX];
        for (int i = 0; i < m_WorkSpaces.Length; i++)
        {
            m_WorkSpaces[i] = new PersonnelSpace(this);
        }

        //allSickbeds = new Sickbed[BED_AMOUNT_MAX];

        m_Grade = CSConst.egtLow;
        if (IsMine)
        {
            BindEvent();
        }
    }

    public const int WORKER_AMOUNT_MAX = 1;
    public CSUI_Hospital uiObj = null;

    Sickbed[] allSickbeds
    {
        set { Data.allSickbeds = value; }
        get { return Data.allSickbeds; }
    }
    
    public List<CSPersonnel> patientsInTent
    {
        get { 
            List<CSPersonnel> patientsList= new List<CSPersonnel> ();
            foreach(Sickbed sb in allSickbeds){
                if (sb.Npc != null)
                {
                    CSPersonnel csp = m_Creator.GetNpc(sb.Npc.Id);
                    patientsList.Add(csp);
                }
            }
            return patientsList;
            }
    }

    public override GameObject gameLogic
    {
        get { return base.gameLogic; }
        set
        {
            base.gameLogic = value;

            if (gameLogic != null)
            {
                PEMachine workmachine = gameLogic.GetComponent<PEMachine>();
                if (workmachine != null)
                {
                    for (int i = 0; i < m_WorkSpaces.Length; i++)
                    {
                        m_WorkSpaces[i].WorkMachine = workmachine;
                    }
                }
                PEDoctor hospitalMachine = gameLogic.GetComponent<PEDoctor>();
                if (hospitalMachine != null)
                {
                    for (int i = 0; i < m_WorkSpaces.Length; i++)
                    {
                        m_WorkSpaces[i].HospitalMachine = hospitalMachine;
                    }
                }

                PEPatients LayCmpt = gameLogic.GetComponent<PEPatients>();
                pePatient = LayCmpt;
                if(pePatient!=null)
                    for (int i = 0; i < allSickbeds.Length; i++)
                    {
                        if(allSickbeds[i]!=null)
                        allSickbeds[i].bedLay = pePatient.Lays[i];
                    }

                if (BuildingLogic != null)
                {
                    workTrans = BuildingLogic.m_WorkTrans;
                    resultTrans = BuildingLogic.m_ResultTrans;
                    for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
                    {
                        if (allSickbeds[i] != null)
                            allSickbeds[i].bedTrans = resultTrans[i];
                    }
                }
            }

        }
    }

    public CSBuildingLogic BuildingLogic
    {
        get { return gameLogic.GetComponent<CSBuildingLogic>(); }
    }
    // Counter Script
    private CounterScript m_Counter;

    public CSTentInfo m_TInfo;
    public CSTentInfo Info
    {
        get
        {
            if (m_TInfo == null)
                m_TInfo = m_Info as CSTentInfo;
            return m_TInfo;
        }
    }

    private CSTentData m_TData;
    public CSTentData Data
    {
        get
        {
            if (m_TData == null)
                m_TData = m_Data as CSTentData;
            return m_TData;
        }
    }

    public override bool AddWorker(CSPersonnel npc)
    {
        if (base.AddWorker(npc))
        {
			SetNurseIcon(npc);
            return true;
        }
        return false;
    }
    public override void RemoveWorker(CSPersonnel npc)
    {
        base.RemoveWorker(npc);
		SetNurseIcon(null);
    }

    public override void AfterTurn90Degree()
    {
        base.AfterTurn90Degree();
        foreach (Sickbed sb in allSickbeds)
        {
            sb.isNpcReady = false;
            sb.occupied = false;
        }
    }

    #region UI event
    void BindEvent()
    {
		if (uiObj==null&&CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.HospitalUI != null)
        {
            uiObj = CSUI_MainWndCtrl.Instance.HospitalUI;
        }
    }

    void UnbindEvent()
    {
		uiObj=null;
    }

	void SetNurseIcon(CSPersonnel p){
		if(uiObj!=null)
			uiObj.Nurse = p;
	}

	void RefreshPatientGrids(List<CSPersonnel> pl){
		if(uiObj!=null)
			uiObj.RefreshPatientGrids(pl);
	}

	void UpdataGridTime(CSPersonnel p,float timeLeft)
	{
		if(uiObj!=null)
			uiObj.UpdateNpcGridTime(p, timeLeft);
	}
    #endregion

    public Sickbed FindEmptyBed(PeEntity npc)
    {
        foreach (Sickbed sb in allSickbeds)
        {
            if (sb.Npc == npc)
            {
                return sb;
            }
        }

        foreach (Sickbed sb in allSickbeds)
        {
            if (sb.Npc == null)
            {
                sb.Npc = npc;

				RefreshPatientGrids(patientsInTent);
                return sb;
            }
        }
        return null;
    }
    public bool IsReady(PeEntity npc,out Sickbed sickBed)
    {
        sickBed = FindEmptyBed(npc);
        return sickBed != null;
    }

    public void AppointTent(PeEntity npc)
    {
        if (!allPatients.Contains(npc))
        {
            allPatients.Add(npc);
			Data.npcIds.Add (npc.Id);
        }
    }

    public override void RemoveDeadPatient(int npcId)
    {
        if (PeGameMgr.IsMulti)
        {
            _Net.RPCServer(EPacketType.PT_CL_TET_RemoveDeadNpc, npcId);
        }
        else
        {
            base.RemoveDeadPatient(npcId);
            PeEntity npc = EntityMgr.Instance.Get(npcId);
            if (allPatients.Count == 0 || !allPatients.Contains(npc))
                return;
            if (allPatients.FindIndex(it => it == npc)<CSMedicalTentConst.BED_AMOUNT_MAX)
            {
                allPatients.Remove(npc);
                for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
                {
                    if (allSickbeds[i].Npc == npc)
                    {
                        allSickbeds[i].Npc = null;
                        allSickbeds[i].StopCounter();
                        allSickbeds[i].isNpcReady = false;
                        allSickbeds[i].occupied = false;
						RefreshPatientGrids(patientsInTent);
                    }
                }
                if (allPatients.Count >= CSMedicalTentConst.BED_AMOUNT_MAX)
                    allPatients[CSMedicalTentConst.BED_AMOUNT_MAX - 1].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
            }
            else
            {
                allPatients.Remove(npc);
            }
			Data.npcIds.Remove(npc.Id);
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
        }
    }

    

    public bool StartTent(PeEntity npc)
    {
        if (allPatients.Count <= 0)
            return false;
        //if (allPatients[0] != npc)
        //    return false;
        Sickbed npcBed = null;
        bool npcLinkBed = false;
        for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
        {
            if (allSickbeds[i].Npc == npc)
            {
                npcLinkBed = true;
                npcBed = allSickbeds[i];
            }
        }
        if(!npcLinkBed)
            return false;

        if (!CallDoctor())
        {
            //--to do: remind player to doctor

        }
        npcBed.isNpcReady = true;
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.In_Hospital;
        return true;

    }

    public bool CallDoctor()
    {
        if (m_Workers[0] == null)
            return false;
        if (!IsDoctorReady())
        {
            //--to do: call doctor
        }
        return true;
    }
    public bool IsDoctorReady()
    {
        if (m_Workers[0] == null || workTrans == null || workTrans[0] == null)
            return false;
        //return (m_Workers[0].m_Pos - workTrans[0].position).magnitude < 8;
		return true;
    }

    private float CountFinalTime(PeEntity npc)
    {
		if(Application.isEditor)
			return 15;
        float result = GetRestoreTime(npc);
        result = result * (1+m_Workers[0].GetTentTimeSkill);
        return result;
    }

    public void StartCounter(int index,PeEntity npc)
    {
        float finalTime = CountFinalTime(npc);
        StartCounter(0, finalTime,index);
    }

    public void StartCounter(float curTime, float finalTime,int index)
    {
        allSickbeds[index].m_Time = finalTime;
        allSickbeds[index].StartCounter(finalTime);
    }

    public void OnTentFinish(Sickbed sickbed)
    {
        CSMain.Instance.RemoveCounter(sickbed.cs);
        if (GameConfig.IsMultiMode)
        {
            sickbed.isNpcReady = false;
            return;
        }
        //--to do: 更新npc状态,更新sickbed
        PeEntity npc = sickbed.Npc;
        sickbed.Npc = null;
        sickbed.isNpcReady = false;
        if (m_MgCreator.FindTreatment(npc.Id) != null)
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
        }
        else
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
        }
        allPatients.Remove(npc);
		Data.npcIds.Remove(npc.Id);
        if (allPatients.Count >= CSMedicalTentConst.BED_AMOUNT_MAX)
        {
            allPatients[CSMedicalTentConst.BED_AMOUNT_MAX - 1].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
        }
		RefreshPatientGrids(patientsInTent);
    }

    public bool IsTent{
        get
        {
            return allPatients.Count >= CSMedicalTentConst.BED_AMOUNT_MAX;
        }
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtTent, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtTent, ref ddata);
        }
        m_Data = ddata as CSTentData;

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
            for (int i = 0; i < allSickbeds.Length; i++)
            {
                allSickbeds[i].tentBuilding = this;
                if(pePatient!=null)
                    allSickbeds[i].bedLay = pePatient.Lays[i];
                if (resultTrans != null)
                    allSickbeds[i].bedTrans = resultTrans[i];
            }
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
            //--to do 
            if (Data.npcIds.Count > 0)
                foreach (int id in Data.npcIds)
                {
                    PeEntity npc = EntityMgr.Instance.Get(id);
                    if (npc != null)
                        allPatients.Add(npc);
                }

            for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
            {
                Sickbed sb = allSickbeds[i];
                if (sb.npcId >= 0)
                    sb.npc = EntityMgr.Instance.Get(sb.npcId);
                else
                    sb.npc = null;
                sb.StartCounterFromRecord();
                sb.tentBuilding = this;
                if(pePatient!=null)
                    sb.bedLay = pePatient.Lays[i];
                if (resultTrans != null)
                    sb.bedTrans = resultTrans[i];
            }

        }
    }
	public void CheckPatient()
	{
		if(Data.npcIds.Count!=allPatients.Count)
		{
			allPatients.Clear();
			foreach (int id in Data.npcIds)
			{
				PeEntity npc = EntityMgr.Instance.Get(id);
				if (npc != null)
					allPatients.Add(npc);
			}
		}
		
		for (int i = allPatients.Count-1; i >= 0; i--)
		{
			if (m_Creator.GetNpc(allPatients[i].Id) == null)
			{
				allPatients.RemoveAt(i);
				Data.npcIds.RemoveAt(i);
				if (i <CSMedicalTentConst.BED_AMOUNT_MAX)
				{
					RefreshPatientGrids(patientsInTent);
				}
			}
		}
	}
    public override void Update()
    {
        base.Update();
		CheckPatient();
        if(IsRunning&&IsDoctorReady())
        {
            for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
            {
                if (allSickbeds[i].IsPatientReady())
                {
                    if (allSickbeds[i].cs == null)
                    {
                        StartCounter(i, allSickbeds[i].npc);
                    }
                    if (allSickbeds[i].cs != null)
                    {
                        allSickbeds[i].cs.enabled = true;
						UpdataGridTime(allSickbeds[i].csp, allSickbeds[i].TimeLeft);
                    }
                }
                else
                {
                    if (allSickbeds[i].cs != null)
                        allSickbeds[i].cs.enabled = false;
                    if (allSickbeds[i].csp != null)
                    {
						UpdataGridTime(allSickbeds[i].csp, allSickbeds[i].TimeLeft);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
            {
                if (allSickbeds[i].cs != null)
                {
                    allSickbeds[i].cs.enabled = false; 
                }
            }
        }
        for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
        {
            allSickbeds[i].Update();
        }
    }

    public override void RemoveData()
    {
        m_Creator.m_DataInst.RemoveObjectData(ID);
    }

    

    public override void UpdateDataToUI()
	{
		if (IsMine)
		{
			BindEvent();
			uiObj.SetTentIcon();
			SetNurseIcon( m_Workers[0]);
			RefreshPatientGrids(patientsInTent);
		}
    }
    public override void DestroySelf()
    {
		base.DestroySelf();
		DestroySomeData();
    }
    public override void DestroySomeData()
    {
        foreach (PeEntity npc in allPatients)
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
        }
        allPatients.Clear();
        Data.npcIds.Clear();
        for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
        {
            allSickbeds[i].Npc = null;
            allSickbeds[i].StopCounter();
		}
		//UpdateDataToUI();
		if (IsMine)
		{
			UnbindEvent();
		}
    }
    public float GetRestoreTime(PeEntity npc)
    {
        CSTreatment csTreatment = m_MgCreator.FindTreatment(npc.Id, true);
        if (csTreatment != null)
            return csTreatment.restoreTime;
        else
            return 0;
    }
	public override bool IsDoingYou(PeEntity npc){
		if(allPatients.Count==0)
			return false;
		if(allPatients.FindIndex(it=>it==npc)<CSMedicalTentConst.BED_AMOUNT_MAX)
			return true;
		else 
			return false;
	}
    #region multiMode
    public void AddNpcResult(List<int> npcIds,int npcid,int index)
    {
        allPatients.Clear();
        foreach (int id in npcIds)
        {
            PeEntity npc = EntityMgr.Instance.Get(id);
            allPatients.Add(npc);
        }
        if (index >= 0)
        {
            allSickbeds[index].Npc = EntityMgr.Instance.Get(npcid);
        }
        Data.npcIds = npcIds;
    }

    public Sickbed CheckNpcBed(PeEntity npc)
    {
        foreach (Sickbed sb in allSickbeds)
        {
            if (sb.Npc == npc)
            {
				RefreshPatientGrids(patientsInTent);
                return sb;
            }
        }
        return null;
    }
    public Sickbed CheckNpcBed(int npcid)
    {
        foreach (Sickbed sb in allSickbeds)
        {
            if (sb.npcId == npcid)
            {
				RefreshPatientGrids(patientsInTent);
                return sb;
            }
        }
        return null;
    }
    public void RemoveDeadPatientResult(int npcId)
    {
        base.RemoveDeadPatient(npcId);
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        if (allPatients.Count == 0 || !allPatients.Contains(npc))
            return;
        allPatients.Remove(npc);
		Data.npcIds.Remove(npc.Id);
        for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
        {
            if (allSickbeds[i].Npc == npc)
            {
                allSickbeds[i].Npc = null;
                allSickbeds[i].StopCounter();
                allSickbeds[i].isNpcReady = false;
                allSickbeds[i].occupied = false;
				RefreshPatientGrids(patientsInTent);
            }
        }
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
    }
    public void TryStartResult(int npcId)
    {
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        foreach (Sickbed sb in allSickbeds)
        {
            if(sb.npc == npc)
            {
                sb.isNpcReady = true;
                npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.In_Hospital;
                return;
            }
        }
    }

    public void SetTent(int npcId)
    {
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
    }

    public void TentFinish(int npcId)
    {
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        Sickbed sickbed = CheckNpcBed(npcId);
        sickbed.Npc = null;
        sickbed.isNpcReady = false;
        if (m_MgCreator.FindTreatment(npc.Id) != null)
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
        }
        else
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
        }
        allPatients.Remove(npc);
		Data.npcIds.Remove(npc.Id);
        if (allPatients.Count >= CSMedicalTentConst.BED_AMOUNT_MAX)
        {
            allPatients[CSMedicalTentConst.BED_AMOUNT_MAX - 1].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
        }
		RefreshPatientGrids(patientsInTent);
    }

    #endregion
    #region interface
    public static void ParseData(byte[] data,CSTentData recordData)
    {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
		{
			int npcCount = BufferHelper.ReadInt32(reader);
			for (int j = 0; j < npcCount; j++)
			{
				recordData.npcIds.Add(BufferHelper.ReadInt32(reader));
			}
            for (int i = 0; i < CSMedicalTentConst.BED_AMOUNT_MAX; i++)
            {
                Sickbed sb = recordData.allSickbeds[i];
                sb.npcId= BufferHelper.ReadInt32(reader);
                sb.m_CurTime = BufferHelper.ReadSingle(reader);
                sb.m_Time = BufferHelper.ReadSingle(reader);
                sb.isNpcReady = BufferHelper.ReadBoolean(reader);
                sb.occupied = BufferHelper.ReadBoolean(reader);
            }
        }
    }
    
    #endregion
}
