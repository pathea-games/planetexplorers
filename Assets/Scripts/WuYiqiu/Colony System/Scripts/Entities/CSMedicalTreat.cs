using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathea.Operate;
using UnityEngine;
using CSRecord;
using Pathea;
using ItemAsset;
public class CSMedicalTreat:CSHealth
{
    const float DEFAULT_TREAT_CHANCE = 1f;
    public override bool IsDoingJob()
    {
        return IsRunning && IsTreat;//--to do
    }
    public CSMedicalTreat(CSCreator creator)
    {
        m_Creator = creator;
        m_Type = CSConst.dtTreat;

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
    public PeEntity lastTreatNpc;
    public CSUI_Hospital uiObj = null;
    public ItemObject medicineItem;
    public CSTreatment treatmentInUse;

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

    public CSTreatInfo m_TInfo;
    public CSTreatInfo Info
    {
        get
        {
            if (m_TInfo == null)
                m_TInfo = m_Info as CSTreatInfo;
            return m_TInfo;
        }
    }

    private CSTreatData m_TData;
    public CSTreatData Data
    {
        get
        {
            if (m_TData == null)
                m_TData = m_Data as CSTreatData;
            return m_TData;
        }
    }
    public override bool AddWorker(CSPersonnel npc)
    {
        if (base.AddWorker(npc))
        {
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
        //--to do
        isNpcReady = false;
        occupied = false;
    }
    
    #region UI event
    void BindEvent()
    {
		if (uiObj==null&&CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.HospitalUI != null)
        {
            uiObj = CSUI_MainWndCtrl.Instance.HospitalUI;
            uiObj.mMedicineRealOp += SetMedicineItem;
        }
    }

    void UnbindEvent()
    {
		if(uiObj!=null){
	        if (CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.HospitalUI != null)
	        {
				uiObj.mMedicineRealOp -= SetMedicineItem;
				uiObj=null;
	        }
		}
    }

	void UpdateTimeShow(float timeLeft){
		if(uiObj!=null)
			uiObj.TreatTimeShow(timeLeft);
	}

	void SetDoctorIcon(CSPersonnel npc){
		if(uiObj!=null)
			uiObj.TreatDoc = npc;
	}
	void SetPatientIcon(CSPersonnel npc){
		if(uiObj!=null)
			uiObj.TreatmentPatient = npc;
	}

	void ResetMissionItem(bool _isMis){
		if(IsMine)
		{
			if (_isMis)
				GameUI.Instance.mItemPackageCtrl.ResetMissionItem();
			else
				GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
	}
	void ShowMedicineNeed(ItemIdCount iic){
		if(uiObj!=null)
			uiObj.TreatMedicineShow(iic);
	}
	void ClearMedicineNeed(){
		if(uiObj!=null)
			uiObj.ClearTreatMedicine();
	}
	void SetMedicineIcon(ItemObject itemObj,bool inOrOut){
		if(uiObj!=null)
			uiObj.SetLocalGrid(itemObj,inOrOut);
	}

	void RefreshTreatment()
	{
		if(uiObj!=null)
			uiObj.RefreshGrid();
	}



    public void SetMedicineItem(ItemPackage _ip, bool _isMis, int _tabIndex, int _index, int _instanceId, bool _inorout)
    {
        //if(_inorout){
        //    ItemObject item = ItemMgr.Instance.Get(_instanceId);
        //    if(treatmentInUse!=null&&treatmentInUse.medicineList[0].protoId!=item.protoId){
        //        return;
        //    }
        //}
		if (!IsRunning)
			return;
        
        if (PeGameMgr.IsMulti)
        {
            _ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRT_SetItem,_isMis, _instanceId, _inorout, _tabIndex, _index);
        }
        else
        {
            if (_inorout)
            {
                ItemObject item = ItemMgr.Instance.Get(_instanceId);
                if (medicineItem != null)
                {
                    //exchange item
                    ItemObject itemToPackage = medicineItem;
                    medicineItem = item;
                    //exchange from package
                    _ip.PutItem(itemToPackage, _index, (ItemPackage.ESlotType)_tabIndex);
                }
                else
                {
                    //setitem
                    medicineItem = item;
                    //remove from package
                    _ip.RemoveItem(item);
                }

            }
            else
            {
                //add to package
                _ip.PutItem(medicineItem, _index, (ItemPackage.ESlotType)_tabIndex);
                //set item
                medicineItem = null;
            }

            //update data
            if (medicineItem != null)
                Data.m_ObjID = medicineItem.instanceId;
            else
                Data.m_ObjID = -1;

            //update UI
			SetMedicineIcon(medicineItem,_inorout);
			ResetMissionItem(_isMis);
        }
    }
    #endregion

    #region uiInterface
    public void AddPatientToUI(PeEntity npc)
    {
        if (npc == null)
        {
            RemovePatientFromUI();
        }
        else
        {
			treatmentInUse = m_MgCreator.FindTreatment(npc.Id, true);
			if(treatmentInUse==null||treatmentInUse.medicineList==null)
				return;
           	SetPatientIcon(m_Creator.GetNpc(npc.Id));
			ShowMedicineNeed(treatmentInUse.medicineList[0]);
        }
    }
    public void RemovePatientFromUI()
    {
		SetPatientIcon(null);
		ClearMedicineNeed();
    }

	public void UpdataPatientToUI(){
		if(allPatients.Count>0)
		{
			AddPatientToUI(allPatients[0]);
		}else{
			AddPatientToUI(null);
		}
	}
    #endregion

    private float CountFinalTime(PeEntity npc)
    {
		if(Application.isEditor)
			return 15;
        //--do do: add skill interface
        float result = GetTreatTime(npc);
        result = result * (1 + m_Workers[0].GetTreatTimeSkill);
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
            m_Counter = CSMain.Instance.CreateCounter("MedicalTreat", curTime, finalTime);
        }
        else
        {
            m_Counter.Init(curTime, finalTime);
        }
        //if (!GameConfig.IsMultiMode)
        //{
            m_Counter.OnTimeUp = OnTreatFinish;
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
        return allPatients.Count <= 0 || allPatients[0] == npc;
    }

    public void AppointTreat(PeEntity npc)
    {
        if (!allPatients.Contains(npc))
        {
            allPatients.Add(npc);
			Data.npcIds.Add(npc.Id);
            if (allPatients.Count == 1)
                AddPatientToUI(npc);
        }
    }

    public bool StartTreat(PeEntity npc)
    {
        if (allPatients.Count <= 0)
            return false;
        if (allPatients[0] != npc)
            return false;
//        if (m_Counter != null)
//            return false;
        if(!CallDoctor())
        {
            //--to do: remind player to doctor
        }
        isNpcReady = true;
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Treating;
        return true;
    }

    public override void RemoveDeadPatient(int npcId)
    {
        if (PeGameMgr.IsMulti)
        {
            _Net.RPCServer(EPacketType.PT_CL_TRT_RemoveDeadNpc, npcId);
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
                //CSMain.Instance.RemoveCounter(m_Counter);
				StopCounter();
                isNpcReady = false;
                occupied = false;
                UpdatePatientIcon();
                if (allPatients.Count > 0)
                    allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
            }
            else
            {
                allPatients.RemoveAll(it => it.Id == npcId);
            }
            Data.npcIds.Remove(npcId);
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.None;
        }
    }

    public bool IsPatientAndMedicineReady()
    {
		if (allPatients.Count <= 0 || allPatients[0].GetCmpt<NpcCmpt>().MedicalState!=ENpcMedicalState.Treating)//|| !isNpcReady)
            return false;
        if (treatmentInUse == null)
            return false;
		return true;//temp
//        if (m_Counter == null)
//        {
//			if (medicineItem == null)
//				return false;
//            if (treatmentInUse.medicineList[0].protoId != medicineItem.protoId)
//                return false;
//            if (medicineItem.GetCount() < treatmentInUse.medicineList[0].count)
//                return false;
//        }
//        return true;
    }

	public bool IsMedicineReady(){
		return true;//temp
//		if (treatmentInUse == null)
//			return false;
//		if (m_Counter == null)
//		{
//			if (medicineItem == null)
//				return false;
//			if (treatmentInUse.medicineList[0].protoId != medicineItem.protoId)
//				return false;
//			if (medicineItem.GetCount() < treatmentInUse.medicineList[0].count)
//				return false;
//		}
//		return true;
	}

    public bool CallDoctor()
    {
        if (m_Workers[0] == null)
            return false;
        if(!IsDoctorReady())
        {
            //--to do: call doctor
        }
        return true;
    }

    public bool IsDoctorReady()
    {
        if (m_Workers[0] == null|| workTrans==null||workTrans[0]==null)
            return false;
        //return (m_Workers[0].m_Pos - workTrans[0].position).magnitude < 5;
		return true;
    }


    public void OnTreatFinish()
    {
        CSMain.Instance.RemoveCounter(m_Counter);
        if (GameConfig.IsMultiMode)
        {
            isNpcReady = false;
            return;
        }
        bool treatSuccess = false;
        System.Random rand = new System.Random();
        if (rand.NextDouble() > DEFAULT_TREAT_CHANCE+m_Workers[0].GetTreatChanceSkill)
        {
            //fail
            treatSuccess = false;
        }
        else
        {
            if (treatmentInUse != null)
            {
                if (!allPatients[0].GetCmpt<NpcCmpt>().illAbnormals.Contains((PEAbnormalType)treatmentInUse.abnormalId))
                {
                    treatSuccess = false;
                }
                else { treatSuccess = true; }
            }
        }
        //--to do:
        //更新病历
        //更新npc状态

        if (treatSuccess)
        {
            if (treatmentInUse != null)
            {
                treatmentInUse.needTreatTimes -= 1;

                if (treatmentInUse.needTreatTimes > 0)
                    allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
                else
                {
                    allPatients[0].GetCmpt<NpcCmpt>().CureSick((PEAbnormalType)treatmentInUse.abnormalId);
                    m_MgCreator.UpdateTreatment();
                    if (m_MgCreator.FindTreatment(allPatients[0].Id) == null)
                        allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
                    else
                        allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
                }
            }
            else
            {
                allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
            }
        }
        else
        {
            //--to do
			//medical negligence
			m_MgCreator.RemoveNpcTreatment(allPatients[0].Id);
			allPatients[0].GetCmpt<NpcCmpt>().AddSick(PEAbnormalType.MedicalAccident);
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
        }
        
        treatmentInUse = null;
        allPatients.RemoveAt(0);
		Data.npcIds.RemoveAt (0);
        isNpcReady = false;
        if (allPatients.Count >= 1)
        {
            AddPatientToUI(allPatients[0]);
            allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
        }
        else
        {
            RemovePatientFromUI();
        }

		RefreshTreatment();
    }

    public bool IsTreat{
        get
        {
            return m_Counter!=null;
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

        for (int i = allPatients.Count - 1; i >= 0; i--)
        {
            if (m_Creator.GetNpc(allPatients[i].Id) == null)
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
    public void UpdatePatientIcon()
    {
        if (allPatients.Count > 0)
            AddPatientToUI(allPatients[0]);
        else
            RemovePatientFromUI();
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtTreat, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtTreat, ref ddata);
        }
        m_Data = ddata as CSTreatData;

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
            
            if(Data.npcIds.Count>0)
                foreach(int id in Data.npcIds){
                    PeEntity npc = EntityMgr.Instance.Get(id);
                    if (npc != null)
                        allPatients.Add(npc);
                }
            if (allPatients.Count > 0)
            {
                treatmentInUse = m_MgCreator.FindTreatment(allPatients[0].Id, true);
            }
            if (Data.m_ObjID >= 0)
            {
                medicineItem = ItemAsset.ItemMgr.Instance.Get(Data.m_ObjID);
            }

            StartCounter(Data.m_CurTime, Data.m_Time);

            //--to do update UI
        }
    }

    public override void Update()
    {
        base.Update();

        CheckPatient();
        if (IsRunning && IsDoctorReady() && IsPatientAndMedicineReady())
        {
            
//            if (lastTreatNpc != allPatients[0])
//            {
//                StopCounter();
//                lastTreatNpc = allPatients[0];
//                uiObj.TreatTimeShow(0);
//            }
//            else
//            {
                if (m_Counter == null)
                {
					if(PeGameMgr.IsSingle)
                    	StartTreatingNpc(allPatients[0]);
                }
                if (m_Counter != null)
                {
                    m_Counter.enabled = true;
					UpdateTimeShow(m_Counter.FinalCounter - m_Counter.CurCounter);
                }
//            }
        }
        else
        {
            if (m_Counter != null)
            {
                m_Counter.enabled = false;
            }
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

    public void StartTreatingNpc(PeEntity npc)
    {
        StartCounter(npc);
		return;//temp
//        medicineItem.DecreaseStackCount(treatmentInUse.medicineList[0].count);
//        if (medicineItem.stackCount == 0)
//            DeleteMedicine(medicineItem.instanceId);
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
        foreach (PeEntity npc in allPatients)
        {
            npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
        }
        allPatients.Clear();
        Data.npcIds.Clear();
        isNpcReady = false;
        occupied = false;
        if (medicineItem != null)
            Data.m_ObjID = medicineItem.instanceId;
        else
            Data.m_ObjID = -1;
		StopCounter();
		//UpdateDataToUI();
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
			uiObj.SetTreatIcon();
			uiObj.TreatDoc = m_Workers[0];
			UpdatePatientIcon();
			uiObj.SetLocalGrid(medicineItem);
		}
    }
    public float GetTreatTime(PeEntity npc)
    {
        if (treatmentInUse.npcId != npc.Id)
            treatmentInUse = m_MgCreator.FindTreatment(npc.Id,true);
        if (treatmentInUse != null)
            return treatmentInUse.treatTime;
        else
            return 0;
    }

	public override bool IsDoingYou(PeEntity npc){
		if(allPatients.Count==0)
			return false;
		if(allPatients[0]==npc)
			return true;
		else 
			return false;
	}
	public override List<ItemIdCount> GetRequirements ()
	{
		//--to do:
		return base.GetRequirements ();
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
    public void RemoveDeadPatientResult(int npcId)
    {
        base.RemoveDeadPatient(npcId);
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        if (allPatients.Count == 0 || !allPatients.Contains(npc))
            return;
        if (allPatients[0].Id == npcId)
        {
            allPatients.RemoveAt(0);
            //CSMain.Instance.RemoveCounter(m_Counter);
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

    public void TryStartResult(int npcId)
    {
        isNpcReady = true;
        PeEntity npc = EntityMgr.Instance.Get(npcId);
        npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Treating;
    }
	public void StartCounterResult(){
		StartCounter(allPatients[0]);
	}

    public void SetTreat(CSTreatment tInUse)
    {
        allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;

        treatmentInUse = m_MgCreator.GetTreatment(tInUse.abnormalId,tInUse.npcId,tInUse.needTreatTimes);
        if (treatmentInUse == null)
        {
            Debug.LogError("treatmentInUse == null!");
        }
        else
        {
           	SetPatientIcon(m_Creator.GetNpc(allPatients[0].Id));
            ShowMedicineNeed(treatmentInUse.medicineList[0]);
        }
    }
    public void SetItemResult(int objId,bool inorout)
    {
        //--to do
        if(objId!=-1){
            medicineItem = ItemMgr.Instance.Get(objId);
        }else{
            medicineItem=null;
        }
        SetMedicineIcon(medicineItem, inorout);
        Data.m_ObjID = objId;
    }
    public void DeleteMedicine(int instanceId)
    {
        if (medicineItem != null && medicineItem.instanceId == instanceId)
        {
			SetMedicineIcon(null, false);
            ItemMgr.Instance.DestroyItem(instanceId);
			medicineItem=null;
			Data.m_ObjID=-1;
        }
    }
    public void TreatFinish(int npcId, bool treatSuccess)
    {
        //CSMain.Instance.RemoveCounter(m_Counter);
        StopCounter();
//        PeEntity npc = EntityMgr.Instance.Get(npcId);


        //--to do
        if (treatSuccess)
        {
            if (treatmentInUse != null)
            {
                treatmentInUse.needTreatTimes -= 1;

                if (treatmentInUse.needTreatTimes > 0)
                {
                    allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
                }
                else
                {
                    allPatients[0].GetCmpt<NpcCmpt>().CureSick((PEAbnormalType)treatmentInUse.abnormalId);
                    m_MgCreator.UpdateTreatment();
                    if (m_MgCreator.FindTreatment(allPatients[0].Id) == null)
                    {
                        allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.Cure;
                    }
                    else
                    {
                        allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
                    }
                }
            }
            else
            {
                allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchHospital;
            }
        }
        else
		{
			m_MgCreator.RemoveNpcTreatment(allPatients[0].Id);
			allPatients[0].GetCmpt<NpcCmpt>().AddSick(PEAbnormalType.MedicalAccident);
			allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;
            //--to do
        }

        treatmentInUse = null;
        allPatients.RemoveAt(0);
		Data.npcIds.RemoveAt(0);
        isNpcReady = false;
        if (allPatients.Count >= 1)
        {
            AddPatientToUI(allPatients[0]);
            allPatients[0].GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchTreat;
        }
        else
        {
            RemovePatientFromUI();
        }


		RefreshTreatment();
    }

	public void ResetNpcToCheck(int npcId){
		PeEntity npc = EntityMgr.Instance.Get(npcId);
		if(npc!=null)
		{
			npc.GetCmpt<NpcCmpt>().MedicalState = ENpcMedicalState.SearchDiagnos;

		}
		allPatients.RemoveAll(it=>it.Id==npcId);
		Data.npcIds.Remove(npcId);
		UpdataPatientToUI();
	}
    #endregion
}

