using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
using UnityEngine;
using Pathea.Operate;
using System.IO;
using Pathea;

public enum ETrainingType{
    Skill=0,
    Attribute
}
public enum ETrainerType
{
    none = 0 ,
    Instructor ,
    Trainee
}

public class CSTraining:CSElectric
{
    public override bool IsDoingJob()
    {
        return IsRunning && IsTrainning;//--to do
    }
    public const int WORKER_AMOUNT_MAX = 24;
    public const int MAX_INSTRUCTOR_NUM = 8;
    public const int MAX_TRAINEE_NUM = 8;

	public const int MAX_SKILL_COUNT =5;
    #region init
    public CSTraining(CSCreator creator)
    {
        m_Creator = creator;
        m_Type = CSConst.etTrain;

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
                
				PETrainner trainerMachine = gameLogic.GetComponent<PETrainner>();
				if (trainerMachine != null)
				{
					for (int i = 0; i < m_WorkSpaces.Length; i++)
					{
						m_WorkSpaces[i].TrainerMachine = trainerMachine;
					}
				}

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

//	public Transform InstructorTrans{
//		get{
//			if(workTrans==null)
//				return null;
//			if(workTrans.Length<1)
//				return null;
//			return workTrans[0];
//		}
//	}
    #endregion
    
    // Counter Script
    private CounterScript m_Counter;
	CSPersonnel csp_instructorNpc;
	CSPersonnel csp_traineeNpc;

    public CSUI_NpcInstructor uiObjSet;
	public CSUI_TrainMgr uiObjTrain;

    #region Data
    public CSTrainingInfo m_TInfo;
    public CSTrainingInfo Info
    {
        get
        {
            if (m_TInfo == null)
                m_TInfo = m_Info as CSTrainingInfo;
            return m_TInfo;
        }
    }
    private CSTrainData m_TData;
    public CSTrainData Data
    {
        get
        {
            if (m_TData == null)
                m_TData = m_Data as CSTrainData;
            return m_TData;
        }
    }

    public List<int> InstructorList
    {
        get { return Data.instructors; }
    }

    public List<int> TraineeList
    {
        get { return Data.trainees; }
    }
    public ETrainingType trainingType
    {
        get { return (ETrainingType)Data.trainingType; }
        set { Data.trainingType = (int)value;}
    }

    public int InstructorNpcId
    {
        get { return Data.instructorNpcId; }
        set { Data.instructorNpcId = value;}
    }

    public int TraineeNpcId
    {
        get { return Data.traineeNpcId; }
        set { Data.traineeNpcId = value;}
    }

    public List<int> LearningSkills
    {
        get { return Data.LearningSkillIds; }
    }

    public float m_CurTime
    {
        get { return Data.m_CurTime; }
        set { Data.m_CurTime = value; }
    }
    public float m_Time
    {
        get { return Data.m_Time; }
        set { Data.m_Time = value; }
    }
    #endregion

    #region UI event
    void BindEvent()
    {
		if (uiObjSet==null&&GameUI.Instance != null && 
		    GameUI.Instance.mCSUI_MainWndCtrl!= null &&
		    GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI!=null &&
		    GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NpcInstructor!= null &&
		    GameUI.Instance.mCSUI_MainWndCtrl.TrainUI)
        {
            uiObjSet = GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NpcInstructor;

            uiObjSet.onInstructorClicked += SetInstructor;
            uiObjSet.onTraineeClicked += SetTrainee;
            uiObjSet.onLabelChanged += SetCount;

			uiObjTrain = GameUI.Instance.mCSUI_MainWndCtrl.TrainUI;
			uiObjTrain.OnStartTrainingEvent += OnStartTraining;
			uiObjTrain.OnStopTrainingEvent += OnStopTraining;
        }
    }

    void UnbindEvent()
	{
		if(uiObjSet!=null){

		uiObjSet.onInstructorClicked -= SetInstructor;
		uiObjSet.onTraineeClicked -= SetTrainee;
		uiObjSet.onLabelChanged -= SetCount;
		}
		if(uiObjTrain!=null){
			uiObjTrain.OnStartTrainingEvent -= OnStartTraining;
			uiObjTrain.OnStopTrainingEvent -= OnStopTraining;
		}
		uiObjSet=null;
		uiObjTrain = null;
    }

	void LockUI(bool flag){
		if(uiObjTrain!=null){
			uiObjTrain.mTrainingLock=flag;
		}
	}
	void SetTrainingTime(float time){
		if(uiObjTrain!=null){
			uiObjTrain.LearnSkillTimeShow(time);
		}
    }
	void PreviewTrainingTime(){
		if(uiObjTrain!=null){
			float finalTime=0;
			if(uiObjTrain.m_TrainLearnPageCtrl.TrainingType == ETrainingType.Skill)
				finalTime = CountSkillFinalTime( uiObjTrain.GetStudyList());
			else
				finalTime = CountAttributeFinalTime();
			uiObjTrain.LearnSkillTimeShow(finalTime);
		}
	}
	void UpdateUI(){
		if(uiObjTrain!=null){
			uiObjTrain.RefreshAfterTraining(m_MgCreator.GetNpc(InstructorNpcId),m_MgCreator.GetNpc(TraineeNpcId));
		}
	}

	public void SetStopBtn(bool flag){
		if(uiObjTrain!=null){
			uiObjTrain.SetBtnState(flag);
		}
	}
	public void SetInstructorNpcUI(int instructorId){
		if(uiObjTrain!=null){
			uiObjTrain.m_TrainLearnPageCtrl.InsNpc = m_MgCreator.GetNpc(instructorId);
		}
	}
	public void SetTraineeNpcUI(int traineeId){
		if(uiObjTrain!=null){
			uiObjTrain.m_TrainLearnPageCtrl.TraineeNpc = m_MgCreator.GetNpc(traineeId);
		}
	}
	public void SetLearnSkillListUI(List<int> skillIds){
		if(uiObjTrain!=null){
			uiObjTrain.SetStudyListInterface(skillIds);
		}
	}

    void OnStartTraining(ETrainingType ttype, List<int> skillIds, CSPersonnel instructorNpc, CSPersonnel traineeNpc)
    {
		if(!IsRunning)
			return;
		if(instructorNpc==null||traineeNpc==null)
			return;
		if(m_Counter!=null)
			return;
        //1.check
        if (!CheckInstructorAndTraineeId(instructorNpc.ID, traineeNpc.ID))
		{
			return;
		}
			
		if(!CheckNpcState(instructorNpc,traineeNpc)){
			return;
		}

		if(ttype == ETrainingType.Skill &&
			(skillIds==null||skillIds.Count==0))
			return ;

		//check trainning times
		if(ttype== ETrainingType.Attribute )
		{
			if(!traineeNpc.CanUpgradeAttribute())
				return;
		}


        if (PeGameMgr.IsMulti)
        {
            switch (ttype)
            {
                case ETrainingType.Skill: 
					_Net.RPCServer(EPacketType.PT_CL_TRN_StartSkillTraining, skillIds.ToArray(), instructorNpc.ID, traineeNpc.ID); 
					break;
                case ETrainingType.Attribute: 
					_Net.RPCServer(EPacketType.PT_CL_TRN_StartAttributeTraining, instructorNpc.ID, traineeNpc.ID); 
					break;
            
            }
        }
        else
        {
            switch (ttype)
            {
                case ETrainingType.Skill: 
					StartSkillCounter(skillIds); 
					break;
                case ETrainingType.Attribute: 
					StartAttributeCounter(); 
					break;
            }

            instructorNpc.trainingType = ttype;
            traineeNpc.trainingType = ttype;
            trainingType = ttype;
            InstructorNpcId = instructorNpc.ID;
            TraineeNpcId = traineeNpc.ID;
			instructorNpc.IsTraining = true;
			traineeNpc.IsTraining = true;
			SetStopBtn(true);
		}
		if(m_MgCreator.GetNpc(InstructorNpcId)!=null&&m_MgCreator.GetNpc(TraineeNpcId)!=null)
			CSUI_MainWndCtrl.ShowStatusBar(
				CSUtils.GetNoFormatString(PELocalization.GetString(CSTrainMsgID.START_TRAINING)
			                          ,m_MgCreator.GetNpc(InstructorNpcId).FullName
			                          ,m_MgCreator.GetNpc(TraineeNpcId).FullName));
    }
	public void OnStopTraining(){
		if(!IsRunning)
			return;
		if (PeGameMgr.IsMulti)
		{
			_Net.RPCServer(EPacketType.PT_CL_TRN_StopTraining);
		}else{
			StopCounter();
		}
	}

    public void SetInstructor(CSPersonnel p)
	{
		if(!IsRunning)
			return;
		if(!CheckNpcState(p))
			return;
		if (PeGameMgr.IsMulti)
        {
            _Net.RPCServer(EPacketType.PT_CL_TRN_SetInstructor, p.ID);
        }
        else
        {
            AddInstructor(p);
        }
    }

    public void SetTrainee(CSPersonnel p)
	{
		if(!IsRunning)
			return;
		if(!CheckNpcState(p))
			return;
		if (PeGameMgr.IsMulti)
        {
            _Net.RPCServer(EPacketType.PT_CL_TRN_SetTrainee, p.ID);
        }
        else
        {
            AddTrainee(p);
        }
    }
    public void SetCount()
	{
		if(!IsRunning)
			return;
		if (uiObjSet!=null)
            uiObjSet.SetCountLabel(InstructorList.Count,MAX_INSTRUCTOR_NUM,TraineeList.Count,MAX_TRAINEE_NUM);
    }
    #endregion
    public bool AddInstructor(CSPersonnel p)
    {
        if (InstructorList.Contains(p.ID))
            return true;
        if (InstructorList.Count >= MAX_INSTRUCTOR_NUM)
            return false;
		if (TraineeNpcId == p.ID)
		{
			if (m_Counter != null)
				StopCounter();
			TraineeNpcId=-1;
		}
		TraineeList.Remove(p.ID);
        InstructorList.Add(p.ID);
        p.trainerType = ETrainerType.Instructor;
		
		UpdateUI();
        return true;
    }
    public bool AddTrainee(CSPersonnel p)
	{
        if (TraineeList.Contains(p.ID))
			return true;
        if (TraineeList.Count >= MAX_TRAINEE_NUM)
            return false;
		if (InstructorNpcId == p.ID)
		{
			if (m_Counter != null)
				StopCounter();
			InstructorNpcId = -1;
		}
		InstructorList.Remove(p.ID);
        TraineeList.Add(p.ID);
		p.trainerType = ETrainerType.Trainee;
		
		UpdateUI();
        return true;
    }
	public void SetNpcIsTraining(bool flag){
		CSPersonnel instructorNpc= m_MgCreator.GetNpc( InstructorNpcId);
		CSPersonnel traineeNpc= m_MgCreator.GetNpc( TraineeNpcId);
		if(instructorNpc!=null)
		{
			instructorNpc.IsTraining = flag;
			instructorNpc.trainingType = trainingType;
			if(!flag)
				InstructorNpcId=-1;
		}else{
			InstructorNpcId = -1;
		}
		if(traineeNpc!=null){
			traineeNpc.IsTraining = flag;
			traineeNpc.trainingType = trainingType;
			if(!flag)
				TraineeNpcId = -1;
		}else{
			TraineeNpcId = -1;
		}
	}

    #region counter
    private float CountSkillFinalTime(List<int> skillIds)
    {
        //--to do count from final time
		float sumTime = 0;
		foreach(int id in skillIds){
			sumTime+= NpcAblitycmpt.GetLearnTime(id);
		}
		return sumTime;
    }
    private float CountAttributeFinalTime()
    {
        //--to do count from final time
        return Info.m_BaseTime;
    }
    public void StartSkillCounter(List<int> skillIds)
    {
        float finalTime = CountSkillFinalTime(skillIds);
		Data.LearningSkillIds = skillIds;
        StartCounter(0, finalTime,ETrainingType.Skill);
    }
    public void StartAttributeCounter()
	{
        float finalTime = CountAttributeFinalTime();
        StartCounter(0, finalTime,ETrainingType.Attribute);
    }
    public void StartCounterFromRecord(float curTime,float finalTime)
    {
        StartCounter(curTime,finalTime,trainingType);
    }
    public void StartCounter(float curTime, float finalTime,ETrainingType ttype)
    {
        if (finalTime < 0F)
            return;
		
		LockUI(true);
        if (m_Counter == null)
        {
            m_Counter = CSMain.Instance.CreateCounter("Train", curTime, finalTime);
        }
        else
        {
            m_Counter.Init(curTime, finalTime);
        }
        if (!GameConfig.IsMultiMode)
        {
            if (ttype == ETrainingType.Skill)
            {
                m_Counter.OnTimeUp = OnLearnSkillFinish;
            }
            else if(ttype==ETrainingType.Attribute){
                m_Counter.OnTimeUp = OnTrainAttributeFinish;
            }
        }
    }

    public void StopCounter()
    {
        Data.m_CurTime = -1F;
        Data.m_Time = -1F;
        CSMain.Instance.DestoryCounter(m_Counter);
        m_Counter = null;
		SetNpcIsTraining(false);
		LockUI(false);
		SetStopBtn(false);
    }

	public void OnLearnSkillFinish()
	{
		m_CurTime = -1F;
		m_Time = -1F;
		CSMain.Instance.RemoveCounter(m_Counter);
		if (PeGameMgr.IsMulti)
		{
			return;
		}
		//--to do:
		CSPersonnel tn = m_MgCreator.GetNpc( TraineeNpcId);
		Ablities cur_skill = tn.GetNpcAllSkill;
		Ablities coverSkill = new Ablities();
		foreach (int ls in LearningSkills)
		{
			//--to do getCoverSkill
			List<int> cSkills = NpcAblitycmpt.GetCoverAbilityId(ls);
			foreach(int coverS in cSkills){
				if(cur_skill.Contains(coverS)){
					coverSkill.Add(coverS);
				}
			}
		}
		cur_skill.RemoveAll(it => coverSkill.Contains(it));
		//--to do: remove redundant skill
		int redundantCount = cur_skill.Count+LearningSkills.Count- MAX_SKILL_COUNT;
		if(redundantCount>0){
			System.Random removeRand = new System.Random();
			for(int i=0;i<redundantCount;i++){
				cur_skill.RemoveAt(removeRand.Next(cur_skill.Count));
			}
		}
		
		cur_skill.AddRange(LearningSkills);


		tn.GetNpcAllSkill = cur_skill;
		SetNpcIsTraining(false);
		SetStopBtn(false);
		LockUI(false);
		UpdateUI();
	}
	
	public void OnTrainAttributeFinish()
	{
		m_CurTime = -1F;
		m_Time = -1F;
		CSMain.Instance.RemoveCounter(m_Counter);
		if (PeGameMgr.IsMulti)
		{
			return;
		}
		//1.get the top attribute
		AttribType mType;		
		csp_instructorNpc = m_Creator.GetNpc(InstructorNpcId);
		csp_traineeNpc = m_Creator.GetNpc(TraineeNpcId);
		if(csp_instructorNpc.IsRandomNpc)
			mType= AttPlusNPCData.GetRandMaxAttribute(csp_instructorNpc.m_Npc.entityProto.protoId, csp_instructorNpc.m_Npc.GetCmpt<SkAliveEntity>());
		else
			mType= AttPlusNPCData.GetProtoMaxAttribute(csp_instructorNpc.m_Npc.entityProto.protoId, csp_instructorNpc.m_Npc.GetCmpt<SkAliveEntity>());
		//2.count attribute 
		AttPlusNPCData.AttrPlus.RandomInt randomInt = new AttPlusNPCData.AttrPlus.RandomInt();
		AttPlusNPCData.GetRandom(csp_instructorNpc.m_Npc.entityProto.protoId, mType, out randomInt);
		float value = new System.Random().Next(randomInt.m_Min, randomInt.m_Max+1);

		//3.apply skill
		Debug.LogError("Train sucess: "+mType.ToString()+":"+value);
		csp_traineeNpc.UpgradeAttribute(mType,value);
		
		SetNpcIsTraining(false);
		SetStopBtn(false);
		LockUI(false);
		UpdateUI();
	}

    #endregion
    public bool CheckNpc()
    {
        if(InstructorNpcId<=0||TraineeNpcId<=0)
            return false;
        CSPersonnel instructorP = m_MgCreator.GetNpc(InstructorNpcId);
        if (instructorP == null)
            return false;
        if(instructorP.Occupation!=CSConst.potTrainer)
        {
            InstructorList.Remove(instructorP.ID);
            instructorP.trainerType = ETrainerType.none;
            InstructorNpcId = -1;
            return false;
        }
        CSPersonnel traineeP = m_MgCreator.GetNpc(TraineeNpcId);
        if (traineeP == null)
            return false;
        if (traineeP.Occupation != CSConst.potTrainer)
        {
            TraineeList.Remove(traineeP.ID);
            traineeP.trainerType = ETrainerType.none;
            TraineeNpcId = -1;
            return false;
        }
        return true;
    }

	public bool CheckNpcPosition(){
		if(InstructorNpcId<=0||TraineeNpcId<=0)
			return false;
		CSPersonnel instructorP = m_MgCreator.GetNpc(InstructorNpcId);
		if (instructorP == null)
			return false;
		if((instructorP.m_Pos -Position).magnitude > 4)
			return false;
		CSPersonnel traineeP = m_MgCreator.GetNpc(TraineeNpcId);
		if (traineeP == null)
			return false;
		if((traineeP.m_Pos-Position).magnitude>4)
			return false;
		//Debug.Log(""+(instructorP.m_Pos -Position).magnitude+","+(instructorP.m_Pos -Position).magnitude);
		return true;
	}
    
    public bool IsTrainning{
        get
        {
            return m_Time>0;
        }
    }

	#region override
	public override void Update()
	{
		base.Update();
		if (!CheckNpc() && m_Counter != null)
			StopCounter();

		if (IsRunning)
		{
			if (m_Counter != null&&CheckNpcPosition())
			{
				m_Counter.enabled = true;
				SetTrainingTime(m_Counter.FinalCounter - m_Counter.CurCounter);
			}else{
				if (m_Counter != null)
				{
					m_Counter.enabled = false;
				}else{
					PreviewTrainingTime();
				} 
			}
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

	public override void CreateData()
	{
		CSDefaultData ddata = null;
		bool isNew;
		if (GameConfig.IsMultiMode)
		{
			isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtTrain, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtTrain, ref ddata);
        }
        m_Data = ddata as CSTrainData;

        InitNPC();

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
            //--to do train
        }
	}
	void StartTrainingFromRecord(float m_CurTime,float m_Time){
		SetInstructorNpcUI(InstructorNpcId);
		SetTraineeNpcUI(TraineeNpcId);
		if(trainingType==ETrainingType.Skill){
			SetLearnSkillListUI(LearningSkills);
		}
		SetNpcIsTraining(true);
		SetStopBtn(true);
		StartCounterFromRecord(m_CurTime,m_Time);
		LockUI(true);
	}
	public override void InitAfterAllDataReady(){
		if (m_Time > 0)
		{
			StartTrainingFromRecord(m_CurTime, m_Time);
		}
	}
	void InitNPC()
	{
		CSMgCreator mgC = m_Creator as CSMgCreator;
		if (mgC != null)
		{
			foreach (CSPersonnel csp in mgC.Trainers)
			{
				if (!AddWorker(csp))
					continue;
				csp.WorkRoom = this;
			}
		}
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

	public override bool AddWorker(CSPersonnel npc){
		if(base.AddWorker(npc)){
			if (npc.trainerType!=ETrainerType.none)
			{
				switch (npc.trainerType)
				{
				case ETrainerType.Instructor: 
					AddInstructor(npc); 
					break;
				case ETrainerType.Trainee: 
					AddTrainee(npc); 
					break;
				}
			}
			return true;
		}
		return false;
	}
	
	public override void RemoveWorker(CSPersonnel npc)
	{
		base.RemoveWorker(npc);
		if (InstructorNpcId == npc.ID)
		{
            if (m_Counter != null)
				StopCounter();
			TraineeNpcId = -1;
        }

        if (TraineeNpcId == npc.ID)
        {
            if (m_Counter != null)
                StopCounter();
			TraineeNpcId = -1;
		}
		InstructorList.Remove(npc.ID);
		TraineeList.Remove(npc.ID);

		UpdateUI();
    }

    public override void UpdateDataToUI()
	{
		if (IsMine)
		{
			BindEvent();
			LockUI(false);
			if(InstructorNpcId>0)
				SetInstructorNpcUI(InstructorNpcId);
			if(TraineeNpcId>0)
				SetTraineeNpcUI(TraineeNpcId);
			if(m_Counter==null)
				LockUI(false);
			else
				LockUI(true);
		}
	}

    public override void DestroySomeData()
	{
		StopCounter();
		if (IsMine)
		{
			UnbindEvent();
		}
    }

	public override void StopWorking(int npcId){
		if(npcId==TraineeNpcId||npcId==InstructorNpcId)
		{
			if(m_Counter!=null)
				StopCounter();
		}
	}
	#endregion

    #region interface
    public static void ParseData(byte[] data, CSTrainData recordData)
    {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {

            int iCount = BufferHelper.ReadInt32(reader);
            recordData.instructors = new List<int>();
            for (int i = 0; i < iCount; i++)
            {
                recordData.instructors.Add(BufferHelper.ReadInt32(reader));
            }
            int tCount = BufferHelper.ReadInt32(reader);
            recordData.trainees = new List<int>();
            for (int i = 0; i < tCount; i++)
            {
                recordData.trainees.Add(BufferHelper.ReadInt32(reader));
            }
            recordData.instructorNpcId = BufferHelper.ReadInt32(reader);
            recordData.traineeNpcId = BufferHelper.ReadInt32(reader);
            recordData.trainingType = BufferHelper.ReadInt32(reader);

            int lCount = BufferHelper.ReadInt32(reader);
            recordData.LearningSkillIds = new List<int>();
            for (int i = 0; i < lCount; i++)
            {
                recordData.LearningSkillIds.Add(BufferHelper.ReadInt32(reader));
            }
            recordData.m_CurTime = BufferHelper.ReadSingle(reader);
            recordData.m_Time = BufferHelper.ReadSingle(reader);
        }
    }
    public bool CheckInstructorAndTraineeId(int instructorId, int traineeId)
    {
        CSPersonnel instructorNpc = m_MgCreator.GetNpc(instructorId);
        CSPersonnel traineeNpc = m_MgCreator.GetNpc(traineeId);
        if (instructorNpc == null || traineeNpc == null)
            return false;
        if (instructorNpc.m_Occupation != CSConst.potTrainer || traineeNpc.m_Occupation != CSConst.potTrainer)
            return false;
        if (!InstructorList.Contains(instructorId) || !TraineeList.Contains(traineeId))
            return false;
        return true;
    }
    public bool CheckTrainerId(int trainerId)
    {
        CSPersonnel trainer = m_MgCreator.GetNpc(trainerId);
        if (trainer == null || trainer.m_Occupation != CSConst.potTrainer)
            return false;
        return true;
    }
    public bool CheckInstructorId(int instructorId)
    {
        CSPersonnel instructor = m_MgCreator.GetNpc(instructorId);
        if (instructor == null || instructor.m_Occupation != CSConst.potTrainer)
            return false;
        if (!InstructorList.Contains(instructorId))
            return false;
        return true;
    }
    public bool CheckTraineeId(int traineeId)
    {
        CSPersonnel trainee = m_MgCreator.GetNpc(traineeId);
        if (trainee == null || trainee.m_Occupation != CSConst.potTrainer)
            return false;
        if (!TraineeList.Contains(traineeId))
            return false;
        return true;
    }

	public bool CheckNpcState(CSPersonnel _instructorNpc,CSPersonnel _traineeNpc){
		if(!_instructorNpc.CanTrain){
			CSUtils.ShowCannotWorkReason(_instructorNpc.CannotWorkReason,_instructorNpc.FullName);
			return false;
		}
		if(!_traineeNpc.CanTrain){
			CSUtils.ShowCannotWorkReason(_traineeNpc.CannotWorkReason,_traineeNpc.FullName);
			return false;
		}
		return true;
	}
	public bool CheckNpcState(CSPersonnel trainerNpc){
		if(!trainerNpc.CanTrain){
			CSUtils.ShowCannotWorkReason(trainerNpc.CannotWorkReason,trainerNpc.FullName);
			return false;
		}
		return true;
	}
    #endregion
    #region multimode
	public void OnStartSkillTrainingResult(List<int> skillIds,int instructorId,int traineeId){
		trainingType = ETrainingType.Skill;
		InstructorNpcId = instructorId;
		TraineeNpcId = traineeId;
		SetInstructorNpcUI(instructorId);
		SetTraineeNpcUI(traineeId);
		SetLearnSkillListUI(skillIds);
		SetNpcIsTraining(true);
		SetStopBtn(true);
		StartSkillCounter(skillIds);
		LockUI(true);
		if(IsMine)
			if(m_MgCreator.GetNpc(InstructorNpcId)!=null&&m_MgCreator.GetNpc(TraineeNpcId)!=null)
				CSUI_MainWndCtrl.ShowStatusBar(
					CSUtils.GetNoFormatString(PELocalization.GetString(CSTrainMsgID.START_TRAINING)
				                          ,m_MgCreator.GetNpc(InstructorNpcId).FullName
				                          ,m_MgCreator.GetNpc(TraineeNpcId).FullName));
	}

	public void OnTrainAttributeTrainingResult(int instructorId,int traineeId){
		trainingType = ETrainingType.Attribute;
		InstructorNpcId = instructorId;
		TraineeNpcId = traineeId;
		SetInstructorNpcUI(instructorId);
		SetTraineeNpcUI(traineeId);
		SetNpcIsTraining(true);
		SetStopBtn(true);
		StartAttributeCounter();
		LockUI(true);
		if(IsMine)
			if(m_MgCreator.GetNpc(InstructorNpcId)!=null&&m_MgCreator.GetNpc(TraineeNpcId)!=null)
				CSUI_MainWndCtrl.ShowStatusBar(
					CSUtils.GetNoFormatString(PELocalization.GetString(CSTrainMsgID.START_TRAINING)
				                          ,m_MgCreator.GetNpc(InstructorNpcId).FullName
				                          ,m_MgCreator.GetNpc(TraineeNpcId).FullName));
	}
	public void StopTrainingrResult(){
		StopCounter();
	}
	public void LearnSkillFinishResult(Ablities skillIds,int traineeId)
    {
		//--to do

		CSPersonnel tn = m_MgCreator.GetNpc(traineeId);
		if(tn!=null)
			tn.GetNpcAllSkill = skillIds;

		StopCounter();
		UpdateUI();
    }

    public void AttributeFinish(int instructorId, int traineeId,int upgradeTimes)
    {
		//--to do: train times ++
		CSPersonnel tn = m_MgCreator.GetNpc(traineeId);
		tn.UpgradeTimes = upgradeTimes;
		StopCounter();
		UpdateUI();
    }
	public void SetCounter(float curTime,float finalTime){
		if (finalTime < 0F)
			return;
		
		LockUI(true);
		if (m_Counter == null)
		{
			m_Counter = CSMain.Instance.CreateCounter("Train", curTime, finalTime);
		}
		else
		{
			m_Counter.Init(curTime, finalTime);
		}
		SetTrainingTime(m_Counter.FinalCounter - m_Counter.CurCounter);
	}
    #endregion
}