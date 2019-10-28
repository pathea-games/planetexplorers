using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CSRecord;
using Pathea.Operate;
using Pathea;
using Pathea.PeEntityExt;
using Mono.Data.SqliteClient;
using System.IO;

#region model
public class CSTreatment
{
    public int abnormalId;
    public int npcId;
    public string npcName;
    public int needTreatTimes;

    public string diseaseName;
    public string treatName;
    public List<ItemIdCount> medicineList = new List<ItemIdCount>();
    public float treatTime;
    public float restoreTime;
    public int treatState=0;//0-treat,1-restore
    public static List<CSTreatment> CreateTreatment(PeEntity npc)
    {
        List<int> npcAbnormalTypeId = new List<int>();
        foreach (PEAbnormalType abTypeId in npc.GetCmpt<NpcCmpt>().illAbnormals)
        {
            npcAbnormalTypeId.Add((int)abTypeId);
        }
        List<CSTreatment> treatmentList = new List<CSTreatment>();
        foreach (int id in npcAbnormalTypeId)
        {

            CSTreatment cst = GenTreatmentFromDatabase(npc,id);
            treatmentList.Add(cst);
        }
        //--to do
        return treatmentList;
    }

    public static CSTreatment GetRandomTreatment(PeEntity npc)
    {
        int abnormalId = AbnormalTypeTreatData.GetRandomData().abnormalId;
        return GenTreatmentFromDatabase(npc, abnormalId);
    }
    public static CSTreatment GenTreatmentFromDatabase(PeEntity npc, int abnormalId)
    {
        CSTreatment cst = new CSTreatment();
        AbnormalTypeTreatData attd = AbnormalTypeTreatData.GetTreatment(abnormalId);
        cst.abnormalId = abnormalId;
        cst.npcName = npc.ExtGetName();
        cst.diseaseName = AbnormalData.GetData((PEAbnormalType)abnormalId).name;
        cst.treatName = PELocalization.GetString(attd.treatDescription);
        cst.medicineList.Add(new ItemIdCount(attd.treatItemId[0], attd.treatItemNum));
        cst.npcId = npc.Id;
        cst.needTreatTimes = attd.treatNum;
        cst.treatTime = attd.treatTime;
        cst.restoreTime = attd.restoreTime;
        return cst;
    }

    #region save/load
    public void InitFromRecord()
    {
        AbnormalTypeTreatData attd = AbnormalTypeTreatData.GetTreatment(abnormalId); 
        diseaseName = AbnormalData.GetData((PEAbnormalType)abnormalId).name;
        treatName = PELocalization.GetString(attd.treatDescription);
        medicineList.Add(new ItemIdCount(attd.treatItemId[0],attd.treatItemNum));
        treatTime = attd.treatTime;
        restoreTime = attd.restoreTime;
    }

    public void _writeTreatmentData(BinaryWriter w)
    {
            w.Write(abnormalId);
            w.Write(npcId);
            w.Write(npcName);
            w.Write(needTreatTimes);
    }
    public static CSTreatment _readTreatmentData(BinaryReader r, int version)
    {
        CSTreatment cst = new CSTreatment();
        if (version >= CSDataMgr.VERSION001)
        {
            cst.abnormalId = r.ReadInt32();
            cst.npcId = r.ReadInt32();
            cst.npcName = r.ReadString();
            cst.needTreatTimes = r.ReadInt32();
        }
        cst.InitFromRecord();
        return cst;
    }
    #endregion

    #region uLink 
    public static object Deserialize(uLink.BitStream r, params object[] codecOptions)
    {
        try
        {
            CSTreatment cst = new CSTreatment();
            cst.abnormalId = r.ReadInt32();
            cst.npcId = r.ReadInt32();
            cst.needTreatTimes = r.ReadInt32();

            AbnormalTypeTreatData attd = AbnormalTypeTreatData.GetTreatment(cst.abnormalId);
			if(EntityMgr.Instance.Get(cst.npcId)==null){
				cst.npcName="?";
			}else{
				cst.npcName = EntityMgr.Instance.Get(cst.npcId).ExtGetName();
			}
            cst.diseaseName = AbnormalData.GetData((PEAbnormalType)cst.abnormalId).name;
            cst.treatName = PELocalization.GetString(attd.treatDescription);
            cst.medicineList.Add(new ItemIdCount(attd.treatItemId[0], attd.treatItemNum));
            cst.treatTime = attd.treatTime;
            cst.restoreTime = attd.restoreTime;
            return cst;
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
    {
        try
        {
            CSTreatment to = value as CSTreatment;
            stream.WriteInt32(to.abnormalId);
            stream.WriteInt32(to.npcId);
            stream.WriteInt32(to.needTreatTimes);
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    
    #endregion

    public override bool Equals(object obj)
    {
        CSTreatment cp = obj as CSTreatment;
        return abnormalId == cp.abnormalId && npcId == cp.npcId && needTreatTimes == cp.needTreatTimes;
    }

    public override int GetHashCode()
    {
        return abnormalId * 73856093 ^ npcId * 19349663 ^ needTreatTimes * 83492791;
    }
}

public class AbnormalTypeTreatData
{
    public static bool debug = false;
    public int no;
    public int abnormalId;
    public int treatDescription;
    public List<int> itemId= new List<int>();
    public int itemNum;
    public bool isMedicalLab;
    public List<int> treatItemId = new List<int>();
    public int treatItemNum;
    public float diagnoseTime;
    public float treatTime;
    public int treatNum;
    public float restoreTime;
	public int cureSkillId;

	public static List<AbnormalTypeTreatData> treatmentDatas = new List<AbnormalTypeTreatData>();
	public static AbnormalTypeTreatData GetDataById(int id){
		int n = treatmentDatas.Count;
		for (int i = 0; i < n; i++) {
			if(treatmentDatas[i].abnormalId == id)
				return treatmentDatas[i];
		}
		return null;
	}
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("abnormaltypetreat");
		while (reader.Read())
        {
            AbnormalTypeTreatData dd = new AbnormalTypeTreatData();
            dd.no = Convert.ToInt32(reader.GetString(reader.GetOrdinal("treatno")));
            dd.abnormalId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("abnormalid")));
            dd.treatDescription = Convert.ToInt32(reader.GetString(reader.GetOrdinal("translationid")));
            string[]  itemStrArry = reader.GetString(reader.GetOrdinal("itemid")).Split(',');
            foreach(string itemidStr in itemStrArry)
            {
                dd.itemId.Add(Convert.ToInt32(itemidStr));
            }
            dd.itemNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemnumber")));
            if ( Convert.ToInt32(reader.GetString(reader.GetOrdinal("ismedicallab"))) > 0)
                dd.isMedicalLab = true;
            else
                dd.isMedicalLab = false;

            itemStrArry = reader.GetString(reader.GetOrdinal("treatitemid")).Split(',');
            foreach (string itemidStr in itemStrArry)
            {
                dd.treatItemId.Add(Convert.ToInt32(itemidStr));
            }

            dd.treatItemNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("treatitemnumber")));
            dd.treatNum = Convert.ToInt32(reader.GetString(reader.GetOrdinal("treatnumber")));
			dd.cureSkillId = Convert.ToInt32 (reader.GetString(reader.GetOrdinal("skBuff_ID")));

            if (!debug)
            {
                dd.diagnoseTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("diagnosistime")));
                dd.treatTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("treattime")));
                dd.restoreTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("restoretime")));
            }
            else
            {
                dd.diagnoseTime = 20;
                dd.treatTime = 20;
                dd.restoreTime = 20;
            }
            treatmentDatas.Add(dd);
        }
    }

    public static AbnormalTypeTreatData GetTreatment(int abnormalId)
    {
		AbnormalTypeTreatData data = GetDataById (abnormalId);
		if(data == null || !data.isMedicalLab)
            return null;
		return data;
    }
    public static AbnormalTypeTreatData GetRandomData()
    {
        int index = new System.Random().Next(treatmentDatas.Count);
		return treatmentDatas[index];
    }

    public static float GetDiagnosingTime(int abnormalId){
		AbnormalTypeTreatData data = GetDataById (abnormalId);
		if (data == null || !data.isMedicalLab)
            return -1;
        return data.diagnoseTime;
    }
    public static float GetRestoreTime(int abnormalId)
    {
		AbnormalTypeTreatData data = GetDataById (abnormalId);
		if (data == null || !data.isMedicalLab)
            return -1;
        return data.restoreTime;
    }
    public static bool CanBeTreatInColony(int abnormalId)
    {
		AbnormalTypeTreatData data = GetDataById (abnormalId);
		if (data == null)
            return false;
        return data.isMedicalLab;
    }

	public static int GetCureSkillId(int abnormalId)
	{ 
		AbnormalTypeTreatData data = GetDataById (abnormalId);
		if (data == null)
		   return -1;
		return data.cureSkillId;
	}
}
#endregion

public class CSMedicalCheck :CSHealth
{
    const float DEFAULT_CHECK_CHANGCE = 0.9f;
    public override bool IsDoingJob()
    {
        return IsRunning && IsChecking;//--to do
    }
    public CSMedicalCheck(CSCreator creator)
    {
        m_Creator = creator;
        m_Type = CSConst.dtCheck;

        // Init Workers
        m_Workers = new CSPersonnel[WORKER_AMOUNT_MAX];

        m_WorkSpaces = new PersonnelSpace[WORKER_AMOUNT_MAX];
        for (int i = 0; i < m_WorkSpaces.Length; i++)
        {
            m_WorkSpaces[i] = new PersonnelSpace(this);
        }

        m_Grade = CSConst.egtLow;
        if (IsMine)
        {
            BindEvent();
        }
    }

    public const int WORKER_AMOUNT_MAX = 1;
    public CSUI_Hospital uiObj = null;
    public PeEntity lastCheckNpc;
    public bool isNpcReady
    {
        set { Data.isNpcReady = value; }
        get { return Data.isNpcReady; }
    }
    public bool occupied
    {
        set { Data.occupied = value; }
        get { return Data.occupied; }
    }
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


                if (BuildingLogic != null)
                {
                    workTrans = BuildingLogic.m_WorkTrans;
                    resultTrans = BuildingLogic.m_ResultTrans;
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

    public CSCheckInfo m_TInfo;
    public CSCheckInfo Info
    {
        get
        {
            if (m_TInfo == null)
                m_TInfo = m_Info as CSCheckInfo;
            return m_TInfo;
        }
    }

    private CSCheckData m_TData;
    public CSCheckData Data
    {
        get
        {
            if (m_TData == null)
                m_TData = m_Data as CSCheckData;
            return m_TData;
        }
    }

    public override bool AddWorker(CSPersonnel npc)
    {
        if (base.AddWorker(npc)){
			SetDoctorIcon(npc);
            return true;
        }
        return false;
    }
    public override void RemoveWorker(CSPersonnel npc)
    {
        base.RemoveWorker(npc);
		SetDoctorIcon(null);
    }

    public override void AfterTurn90Degree()
    {
        base.AfterTurn90Degree();

        isNpcReady = false;
        occupied = false;

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

	void SetDoctorIcon(CSPersonnel p){
		if(uiObj!=null)
			uiObj.ExamineDoc = p;
	}

	void SetPatientIcon(CSPersonnel p){
		if(uiObj!=null)
			uiObj.ExaminedPatient = p;
	}

	void UpdateTimeShow(float timeLeft){
		if(uiObj!=null)
			uiObj.CheckTimeShow(timeLeft);
	}

	void RefreshTreatment()
	{
		if(uiObj!=null)
			uiObj.RefreshGrid();
	}

    #endregion
    private float CountFinalTime(PeEntity npc)
    {
		if(Application.isEditor)
			return 15;
        //--do do: add skill interface
        float result = GetDiagnoseTime(npc);
        result = result * (1 +m_Workers[0].GetDiagnoseTimeSkill);
        return result;
    }
    public void StartCounter(PeEntity npc)
    {
        float finalTime = CountFinalTime(npc);
        StartCounter(0, finalTime);
    }

    public void StartCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;

        if (m_Counter == null)
        {
            m_Counter = CSMain.Instance.CreateCounter("MedicalCheck", curTime, finalTime);
        }
        else
        {
            m_Counter.Init(curTime, finalTime);
        }
        //if (!GameConfig.IsMultiMode)
        //{
            m_Counter.OnTimeUp = OnCheckFinish;
        //}
    }

    public void StopCounter()
    {
        Data.m_CurTime = -1F;
        Data.m_Time = -1F;
        CSMain.Instance.DestoryCounter(m_Counter);
        m_Counter = null;
    }
    public bool IsReady(PeEntity npc)
    {
        return allPatients.Count <= 0||allPatients[0]==npc;
    }
    
    public bool AppointCheck(PeEntity npc)
    {
        if (npc.GetCmpt<NpcCmpt>().illAbnormals == null || npc.GetCmpt<NpcCmpt>().illAbnormals.Count <= 0)
            return false;
        if (!allPatients.Contains(npc))
        {
            allPatients.Add(npc);
            Data.npcIds.Add(npc.Id);
            if (allPatients.Count == 1)
                UpdatePatientIcon();
        }
        return true;
    }

    public void GetCheck()
    {
        if (allPatients.Count<=0)
            return;
        if (m_Counter != null)
            return;

        //--to do: set npc to check
    }

    public bool StartCheck(PeEntity npc)
    {
        if (allPatients.Count <= 0)
            return false;
        if (allPatients[0] != npc)
            return false;
//        if (m_Counter != null)
//            return false;
        if (!CallDoctor())
        {
            //--to do: remind player to doctor
        }
        isNpcReady = true;
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Diagnosing;
        return true;
        //--to do: updateUI
    }



    public bool IsPatientReady()
    {
		return allPatients.Count>0 && allPatients[0].GetCmpt<NpcCmpt>().MedicalState == ENpcMedicalState.Diagnosing;// && isNpcReady;
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
        //return (m_Workers[0].m_Pos - workTrans[0].position).magnitude < 3;
		return true;
    }
    
    public void CheckPatient()
    {
        //lz-2016.12.23 Crash bug 错误 #7789
        if (null == Data || null == Data.npcIds || null == allPatients)
        {
            return;
        }
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
            //lw:2017.2.8 -V1.0.6 from SteamId 76561198106173618
            if (m_Creator != null && m_Creator.GetNpc(allPatients[i].Id) == null)
            {
                allPatients.RemoveAt(i);
				Data.npcIds.RemoveAt(i);
                if (i == 0)
                {
                    UpdatePatientIcon();
                    isNpcReady = false;
                }
            }
        }
    }

    public void OnCheckFinish()
    {
        Data.m_CurTime = -1;
        Data.m_Time = -1;
        CSMain.Instance.RemoveCounter(m_Counter);
//		StopCounter();
        if (GameConfig.IsMultiMode)
        {
            isNpcReady = false;
            return;
        }
        PeEntity checkNpc = allPatients[0];
        List<CSTreatment> csts = new List<CSTreatment>();

        System.Random rand = new System.Random();
        if (rand.NextDouble() > DEFAULT_CHECK_CHANGCE+m_Workers[0].GetDiagnoseChanceSkill)
        {
            csts.Add(CSTreatment.GetRandomTreatment(checkNpc));
        }else{
            csts = CSTreatment.CreateTreatment(checkNpc);
        }
        //--to do:
        //生成病历
        //更改npc状态  eg.diagnosing,treating, rest

        m_MgCreator.RemoveNpcTreatment(checkNpc.Id);
        if (csts != null && csts.Count > 0)
        {

            m_MgCreator.AddTreatment(csts);
            checkNpc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
        }
        else
        {
            checkNpc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
        }

        allPatients.RemoveAt(0);
		Data.npcIds.RemoveAt(0);
        isNpcReady = false;
        if (allPatients.Count >= 1)
        {
			SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
            allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
        }
        else
        {
			SetPatientIcon(null);
        }

		RefreshTreatment();
    }

    public bool IsChecking{
        get
        {
            return m_Counter!=null;
        }
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtCheck, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtCheck, ref ddata);
        }
        m_Data = ddata as CSCheckData;

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
            
            if (Data.npcIds.Count > 0)
                foreach (int id in Data.npcIds)
                {
                    PeEntity npc = EntityMgr.Instance.Get(id);
                    if (npc != null)
                        allPatients.Add(npc);
                }
            StartCounter(Data.m_CurTime, Data.m_Time);
            //--to do update UI
        }
    }

    public override void Update()
    {
        base.Update(); 
        CheckPatient();
        if (IsRunning && IsDoctorReady()&&IsPatientReady())
        {
            if (lastCheckNpc != allPatients[0])
            {
                StopCounter();
                lastCheckNpc = allPatients[0];
				UpdateTimeShow(0);
            }
            else
            {
                if (m_Counter == null)
                {
                    StartCheckingNpc(allPatients[0]);
                }
                if (m_Counter != null)
                {
                    m_Counter.enabled = true;
					UpdateTimeShow(m_Counter.FinalCounter - m_Counter.CurCounter);
                }
            }
        }
        else
        {
            if(m_Counter!=null)
                m_Counter.enabled = false;
        }
        if (m_Counter != null)
        {
            Data.m_CurTime = m_Counter.CurCounter;
            Data.m_Time = m_Counter.FinalCounter;
        }
        else
        {
            Data.m_CurTime = -1F;
            Data.m_Time = -1F;
        }
    }

    public void StartCheckingNpc(PeEntity npc)
    {
        StartCounter(npc);
        SetPatientIcon(m_Creator.GetNpc(npc.Id));
    }
    public void UpdatePatientIcon()
    {
        if (allPatients.Count>0)
			SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
    }

    public override void RemoveData()
    {
        m_Creator.m_DataInst.RemoveObjectData(ID);
    }

    public override void DestroySelf()
    {
        base.DestroySelf();
        DestroySomeData();
    }

    public override void DestroySomeData()
    {
		//1.destroy data
        foreach (PeEntity npc in allPatients)
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
        }
        allPatients.Clear();
        Data.npcIds.Clear();
        isNpcReady = false;
        occupied = false;
		StopCounter();
		//2. update view
		//UpdateDataToUI();
		//3. disconnect UI
		if (IsMine)
		{
			UnbindEvent();
		}
    }

    public override void UpdateDataToUI()
	{
		if (IsMine)
		{
			BindEvent();
	        uiObj.SetCheckIcon();
			SetDoctorIcon(m_Workers[0]);
	        if (allPatients.Count > 0)
	            SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
	        else
				SetPatientIcon(null);
		}
    }

    public float GetDiagnoseTime(PeEntity npc)
    {
        List<PEAbnormalType> npcAbnormal = npc.GetCmpt<NpcCmpt>().illAbnormals;
		if(npcAbnormal==null||npcAbnormal.Count==0)
			return 0;
        float maxTime = AbnormalTypeTreatData.GetDiagnosingTime((int)npcAbnormal[0]);
        for (int i = 1; i<npcAbnormal.Count;i++ )
        {
            float time = AbnormalTypeTreatData.GetDiagnosingTime((int)npcAbnormal[i]);
            if ( time> maxTime)
            {
                maxTime = time;
            }
        }
        return maxTime;
    }
    public override void RemoveDeadPatient(int npcId)
    {
        if (PeGameMgr.IsMulti)
        {
            _Net.RPCServer(EPacketType.PT_CL_CHK_RemoveDeadNpc, npcId);
        }
        else
        {
            base.RemoveDeadPatient(npcId);
            PeEntity npc = EntityMgr.Instance.Get(npcId);
            if (allPatients.Count == 0 || !allPatients.Contains(npc))
                return;
            if (allPatients[0].Id == npcId)
            {
				allPatients.RemoveAt(0);
				StopCounter();
                isNpcReady = false;
                occupied = false;
                UpdatePatientIcon();
                if (allPatients.Count > 0)
                    allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
            }
            else
            {
                allPatients.RemoveAll(it => it.Id == npcId);
            }
			
			Data.npcIds.Remove(npcId);
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
        }
    }

	public override bool IsDoingYou(PeEntity npc){
		if(allPatients.Count==0)
			return false;
		if(allPatients[0]==npc)
			return true;
		else 
			return false;
	}
    #region multiMode
    public void AddNpcResult(List<int> npcIds)
    {
        allPatients.Clear();
        foreach (int npcid in npcIds)
        {
            PeEntity npc = EntityMgr.Instance.Get(npcid);
            allPatients.Add(npc);
        }
        Data.npcIds = npcIds;
    }

    public void TryStartResult(int npcId)
    {
        isNpcReady = true;
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Diagnosing;
    }

    public void SetDiagnose()
    {
        allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
        UpdatePatientIcon();
    }

    public void RemoveDeadPatientResult(int npcId)
    {
        base.RemoveDeadPatient(npcId);
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        if (allPatients.Count == 0 || !allPatients.Contains(npc))
            return;
        if (allPatients[0].Id == npcId)
        {
			allPatients.RemoveAt(0);
			StopCounter();
            isNpcReady = false;
            occupied = false;
        }
        else
        {
            allPatients.RemoveAll(it => it.Id == npcId);
        }
		Data.npcIds.Remove(npcId);
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
    }

    public void CheckFinish(int npcId, List<CSTreatment> csts)
    {
        //CSMain.Instance.RemoveCounter(m_Counter);
        StopCounter();
        PeEntity npc = EntityMgr.Instance.Get(npcId);

        int teamId = _ColonyObj._Network.TeamId;
        CSMgCreator creator = MultiColonyManager.GetCreator(teamId);

        creator.RemoveNpcTreatment(npcId);
        if (csts != null && csts.Count > 0)
        {

            creator.AddTreatment(csts);
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
        }
        else
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
        }

        allPatients.RemoveAll(it => it.Id == npcId);
		Data.npcIds.Remove(npcId);
        isNpcReady = false;
        if (allPatients.Count >= 1)
        {
            SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
            allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
        }
        else
        {
			SetPatientIcon(null);
        }
		RefreshTreatment();
    }
    #endregion
}
