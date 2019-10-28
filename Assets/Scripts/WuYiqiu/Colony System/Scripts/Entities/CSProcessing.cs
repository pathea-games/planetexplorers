using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSRecord;
using Pathea.Operate;
using ItemAsset;
using ItemAsset.PackageHelper;
using Mono.Data.SqliteClient;
using System.IO;
using Pathea;

#region model
public class ProcessingObjInfo{
    public int protoId;
    public int tab;
    public int max;
    public float time;

    static Dictionary<int, ProcessingObjInfo> pobInfoDict = new Dictionary<int, ProcessingObjInfo>();
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("collectfield");

        while (reader.Read())
        {
            ProcessingObjInfo pbi = new ProcessingObjInfo();
            pbi.protoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemid")));
            pbi.tab = Convert.ToInt32(reader.GetString(reader.GetOrdinal("tab")));
            pbi.max = Convert.ToInt32(reader.GetString(reader.GetOrdinal("max")));
            pbi.time = Convert.ToSingle(reader.GetString(reader.GetOrdinal("time")));
            pobInfoDict.Add(pbi.protoId,pbi);
        }
    }

    public static float GetPobTime (int protoId){
        if (!pobInfoDict.ContainsKey(protoId))
        {
            return -1;
        }
        return pobInfoDict[protoId].time / pobInfoDict[protoId].max;
    }

    public static int GetPobMax(int protoId)
    {
        if (!pobInfoDict.ContainsKey(protoId))
        {
            return -1;
        }
        return pobInfoDict[protoId].max;
    }

    public static ICollection<ProcessingObjInfo> GetAllInfo()
    {
        return pobInfoDict.Values;
    }
}


public class ProcessingTask{
    public List<ItemIdCount> itemList;
    public List<CSPersonnel> npcList;
	public int runCount;
    public CounterScript cs;
    public float m_CurTime;
    public float m_Time;
    //public RandomItemObj resultItem;

    public delegate void TaskEvent(ProcessingTask task,float percent);
    public TaskEvent taskAccomplished;

    public ProcessingTask()
    {
        itemList = new List<ItemIdCount>();
        npcList= new List<CSPersonnel> ();
		runCount =1;
        cs = null;
    }
	public void SetRunCount(int count){
		runCount = count;
		if(cs!=null)
			cs.SetRunCount(count);
	}
	public void CountDownRepeat(){
		if(runCount>0)
			runCount--;
	}
	public bool NeedRepeat(){
		return runCount>=1;
	}
    public bool CanStart(out int errorCode)
    {
        errorCode = -1;
        if (npcList.Count <= 0)
        {
            errorCode = ProcessingConst.NEED_HUMAN;
            return false;
        }
        if (itemList.Count <= 0)
            return false;
        if (cs != null)
            return false;
        return true;
    }
    public bool CanStart()
    {
        if (npcList.Count <= 0)
        {
            return false;
        }
        if (itemList.Count <= 0)
            return false;
        if (cs != null)
            return false;
        return true;
    }

    public float TimeLeft
    {
        get
        {
            if (cs != null)
            {
                return cs.FinalCounter - cs.CurCounter;
            }
            else
            {
                return CountTime();
            }
        }
    }
    public void Start()
    {
        if (npcList.Count==0)
            return;
        float time = CountTime();
        StartCounter(time);

        foreach (CSPersonnel npc in npcList)
        {
            npc.resultItems = null;
            npc.IsProcessing = true;
        }
    }

	//test
	public void Start(float time)
	{
		if (npcList.Count==0)
			return;
		StartCounter(time);
		
		foreach (CSPersonnel npc in npcList)
		{
			npc.resultItems = null;
			npc.IsProcessing = true;
		}
	}

    public void StartCounter(float timeNeed)
    {
        StartCounter(0, timeNeed);

    }

    public void StartCounterFromRecord()
    {
        StartCounter(m_CurTime, m_Time);
        if (m_Time > 0)
        {
            foreach (CSPersonnel npc in npcList)
            {
                npc.resultItems = null;
                npc.IsProcessing = true;
            }
        }
    }
	public void StartCounterFromNet(float cur,float final,int runCount)
	{
		StartCounter(cur, final,runCount);
		if (m_Time > 0)
		{
			foreach (CSPersonnel npc in npcList)
			{
				npc.resultItems = null;
				npc.IsProcessing = true;
			}
		}
	}

    public void StartCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;

        if (cs == null)
        {
            cs = CSMain.Instance.CreateCounter("Processing", curTime, finalTime);
			cs.SetRunCount(runCount);
        }
        else
        {
            cs.Init(curTime, finalTime);
        }
        if (!GameConfig.IsMultiMode)
        {
            cs.OnTimeUp = Accomplished;
        }
    }
	public void StartCounter(float curTime, float finalTime,int runCount)
	{
		if (finalTime < 0F)
			return;

		this.runCount = runCount;
		if (cs == null)
		{
			cs = CSMain.Instance.CreateCounter("Processing", curTime, finalTime);
			cs.SetRunCount(runCount);
		}
		else
		{
			cs.Init(curTime, finalTime);
		}
		if (!GameConfig.IsMultiMode)
		{
			cs.OnTimeUp = Accomplished;
		}
	}
    public void StopCounter()
    {
        if (!PeGameMgr.IsMulti)
        {
            taskAccomplished(this, cs.CurCounter / cs.FinalCounter);
            //npcList[0].resultItems = resultItem;
        }
        else
        {
            ClearItem();
        }
        CSMain.Instance.DestoryCounter(cs);
        cs = null;

        
        foreach (CSPersonnel npc in npcList)
        {
            npc.IsProcessing = false;
        }
    }
	public void SyncStopCounter(){
		if(cs!=null){
			CSMain.Instance.DestoryCounter(cs);
			cs = null;
		}
		
		foreach (CSPersonnel npc in npcList)
		{
			npc.IsProcessing = false;
		}
	}
    public float CountTime()
    {
        if (npcList.Count == 0)
        {
            return 0;
        }
        float time = 0;
        //--to do:
        foreach (ItemIdCount po in itemList)
        {
            if(po!=null){
                time+=ProcessingObjInfo.GetPobTime(po.protoId)*po.count;
            }
        }
        return time * GetFullWorkerParam(npcList);
    }

    public float GetFullWorkerParam(List<CSPersonnel> npcList)
    {
        float buffParam = 1;
        foreach (CSPersonnel csp in npcList)
        {
            buffParam *= 1 + csp.GetProcessingTimeSkill;
        }
        return buffParam / npcList.Count;
    }

    List<CSPersonnel> GenNewNpcList(List<CSPersonnel> originalNpcList, CSPersonnel changeNpc,bool isAdd)
    {
        List<CSPersonnel> newNpcList = new List<CSPersonnel>();
        newNpcList.AddRange(originalNpcList);

        if (isAdd)
            newNpcList.Add(changeNpc);
        else
            newNpcList.Remove(changeNpc);
        return newNpcList;
    }

    private void Accomplished()
    {
        taskAccomplished(this, 1);
        //npcList[0].resultItems = resultItem;
		if(runCount<=0){
			foreach (CSPersonnel npc in npcList)
			{
				npc.IsProcessing = false;
			}
		}
    }

    public void AddNpc(CSPersonnel npc)
    {
        //--to do:
        if (npcList.Contains(npc))
            return;
        //not in processing
        if (cs == null)
        {
            npcList.Add(npc);
            npc.IsProcessing = false;
        }
        //in processing
        else
        {
//            float currentParam = GetFullWorkerParam(npcList);
//            float newParam = GetFullWorkerParam(GenNewNpcList(npcList, npc, true));
//            float changeParam = newParam / currentParam;
//
//
//            cs.Init(cs.CurCounter * changeParam, cs.FinalCounter * changeParam);
            npcList.Add(npc);
			float curPercent = cs.CurCounter/cs.FinalCounter;
			float finalNew = CountTime();
			float curNew = finalNew*curPercent;
			cs.Init(curNew,finalNew);
            npc.IsProcessing = true;
        }
    }
    public void InitNpc(CSPersonnel npc)
    {
        if (npcList.Contains(npc))
            return;
        npcList.Add(npc);
        if (cs == null)
        {
            npc.IsProcessing = false;
        }
        else
        {
            npc.IsProcessing = true;
        }
    }


    public void RemoveNpc(CSPersonnel npc)
    {
        if (!npcList.Contains(npc))
            return;
        //not in processing
        if (cs == null)
        {
            npcList.Remove(npc);
        }
        //in processing
        else
        {
            npcList.Remove(npc);
			if(npcList.Count==0)
			{ 
				StopCounter();
			}
			else
			{
				float curPercent = cs.CurCounter/cs.FinalCounter;
				float finalNew = CountTime();
				float curNew = finalNew*curPercent;
				cs.Init(curNew,finalNew);
			}
		}
		npc.IsProcessing = false;
	}
	public bool AddItem(ItemIdCount po)
	{
		//--to do:
		if (cs != null)
			return false;

        bool existed = false;
        ItemIdCount existedPob = null;
        foreach (ItemIdCount pob in itemList)
        {
            if (pob.protoId == po.protoId)
            {
                existedPob = pob;
                existed = true;
            }
        }

        if (existed)
        {
            if (existedPob.count >= ProcessingObjInfo.GetPobMax(existedPob.protoId))
                return false;
            existedPob.count += po.count;
            if (existedPob.count > ProcessingObjInfo.GetPobMax(existedPob.protoId))
                existedPob.count = ProcessingObjInfo.GetPobMax(existedPob.protoId);
            return true;
        }
        else
        {
            //2.check room
            if (itemList.Count >= ProcessingConst.OBJ_MAX)
                return false;
            if (po.count > ProcessingObjInfo.GetPobMax(po.protoId))
                po.count = ProcessingObjInfo.GetPobMax(po.protoId);
            itemList.Add(po);
            return true;
        }
    }

    public bool RemoveItem(int protoId)
    {
        //--to do:
        if (cs != null)
            return false;

        bool existed = false;
        ItemIdCount existedPob = null;
        foreach (ItemIdCount pob in itemList)
        {
            if (pob.protoId == protoId)
            {
                existedPob = pob;
                existed = true;
            }
        }
        if (existed)
        {
            itemList.Remove(existedPob);
            return true;
        }

        return false;
    }

    public void ClearItem()
    {
        itemList = new List<ItemIdCount>(ProcessingConst.OBJ_MAX);
    }
    public bool IsWorking()
    {
        return cs != null;
    }
	public bool IsWorkingOn
	{
		get{return cs != null&&cs.enabled;}
	}
    public void Update()
    {
        if (cs != null)
		{
			m_CurTime 	= cs.CurCounter;
			m_Time		= cs.FinalCounter;
		}
		else
		{
			m_CurTime = 0F;
			m_Time = -1F;
		}
    }

    #region interface
    public bool HasItem()
    {
        if(itemList.Count>0)
        {
            return true;
        }
        return false;
        
    }
    public void CallBackAllNpc(){
        foreach (CSPersonnel npc in npcList)
        {
            npc.IsProcessing = false;
        }
    }
    #endregion
}
#endregion

public class CSProcessing: CSCommon
{
    public CSUI_CollectWnd uiObj;
    //bool uiInit = false;
    ProcessingTask selectProcess;
    
    public override bool IsDoingJob()
    {
        return IsRunning;
    }
	public override bool IsDoingJobOn
	{
		get{
			if(!IsRunning)
				return false;
			if(mTaskTable!=null)
				foreach(ProcessingTask pt in mTaskTable){
					if(pt!=null&&pt.IsWorkingOn){
						return true;
					}
				}
			return false;
			}
	}
    //setting
    public bool IsAuto {
        get { return Data.isAuto; }
        set { Data.isAuto = value; }
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
                if(BuildingLogic!=null)
                    workTrans = BuildingLogic.m_WorkTrans;
            }
            
        }
    }

    public CSBuildingLogic BuildingLogic
    {
		get { return gameLogic==null?null:gameLogic.GetComponent<CSBuildingLogic>(); }
    }
    public CSProcessingInfo m_PInfo;
    public CSProcessingInfo Info
    {
        get
        {
            if (m_PInfo == null)
                m_PInfo = m_Info as CSProcessingInfo;
            return m_PInfo;
        }
    }

    private CSProcessingData m_PData;
    public CSProcessingData Data
    {
        get
        {
            if (m_PData == null)
                m_PData = m_Data as CSProcessingData;
            return m_PData;
        }
    }
    //obj
    public List<ItemIdCount> mProcessingObjData;

    public ProcessingTask[] mTaskTable
    {
        get { return Data.mTaskTable; }
    }
    //npc
    public List<CSPersonnel> allProcessor
    {
        get { return m_Workers.ToList(); }
    }

    public delegate void TaskEvent();
    public TaskEvent taskComplished;

    
    public CSProcessing(CSCreator creator)
    {
        m_Creator = creator;
        m_Type = CSConst.etProcessing;

        // Init Workers
        m_Workers = new CSPersonnel[ProcessingConst.WORKER_AMOUNT_MAX];

        m_WorkSpaces = new PersonnelSpace[ProcessingConst.WORKER_AMOUNT_MAX];
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


	public override bool AddWorker (CSPersonnel csp)
	{
		if(base.AddWorker(csp)){
//			if (csp.ProcessingIndex >= 0)
//			{
//				if (mTaskTable[csp.ProcessingIndex] == null)
//				{
//					mTaskTable[csp.ProcessingIndex] = new ProcessingTask();
//				}
//				mTaskTable[csp.ProcessingIndex].InitNpc(csp);
//			}
			return true;
		}
		return false;
	}


    void InitNPC(){
        CSMgCreator mgC = m_Creator as CSMgCreator;
        if (mgC != null)
        {
            foreach (CSPersonnel csp in mgC.Processors)
            {
                if (!AddWorker(csp))
                    continue;
                csp.WorkRoom = this;
				InitNpcProcessingIndex(csp);
            }
        }
    }

    void InitNpcProcessingIndex(CSPersonnel csp){
		if(csp.ProcessingIndex>=0&&csp.ProcessingIndex<mTaskTable.Length){
	        if (mTaskTable[csp.ProcessingIndex] == null)
	        {
	            mTaskTable[csp.ProcessingIndex] = new ProcessingTask();
	        }
	        mTaskTable[csp.ProcessingIndex].InitNpc(csp);
		}
    }

    void CheckAllProcessor()
	{
		for(int i=0;i<mTaskTable.Length;i++){
			if(mTaskTable[i]!=null)
			{
				for(int j=mTaskTable[i].npcList.Count-1;j>=0;j--){
					CSPersonnel csp = mTaskTable[i].npcList[j];
					if(!csp.CanProcess&&!csp.IsProcessing)
						csp.StopWork();
					else if(csp.IsProcessing&& csp.ShouldStopProcessing)
						csp.StopWork();
				}
			}
		}
    }


    #region UI Interface
    void BindEvent()
    {
        CSPersonnel.RegisterProcessingIndexChangedListener(SetNpcProcessingIndex);
        CSPersonnel.RegisterProcessingIndexInitListener(InitNpcProcessingIndex);
        if (CSUI_MainWndCtrl.Instance != null&&CSUI_MainWndCtrl.Instance.CollectUI!=null)
        {
            uiObj = CSUI_MainWndCtrl.Instance.CollectUI;
            uiObj.e_InitCollectEvent += InitUI;
            uiObj.e_UpdateCollect += UpdateDataToUi;
            uiObj.e_AddtoClick += OnAddClick;
            uiObj.e_RemoveEvent += OnRemoveClick;
            uiObj.e_StartClick += TryStartProcessing;
            uiObj.e_ProcessChoseEvent += OnSelectProcess;
            uiObj.e_StopClick += TryStop;
            uiObj.e_AutoClick += SetAuto;
            //lz-2016.07.14 SetRunCount操作事件
            uiObj.e_SetRunCountEvent += OnSetRunCount;
        }
    }

    void UnbindEvent()
    {
        CSPersonnel.UnRegisterProcessingIndexChangedListener(SetNpcProcessingIndex);
        CSPersonnel.UnRegisterProcessingIndexInitListener(InitNpcProcessingIndex);
        if (CSUI_MainWndCtrl.Instance != null && CSUI_MainWndCtrl.Instance.CollectUI != null)
        {
            uiObj.e_InitCollectEvent -= InitUI;
            uiObj.e_UpdateCollect -= UpdateDataToUi;
            uiObj.e_AddtoClick -= OnAddClick;
            uiObj.e_RemoveEvent -= OnRemoveClick;
            uiObj.e_StartClick -= TryStartProcessing;
            uiObj.e_ProcessChoseEvent -= OnSelectProcess;
            uiObj.e_StopClick -= TryStop;
            uiObj.e_AutoClick -= SetAuto;
            //lz-2016.07.14 SetRunCount操作事件
            uiObj.e_SetRunCountEvent -= OnSetRunCount;
        }
    }

    void InitUI(object obj)
    {
        //if (!IsRunning)
        //    return;
        SetSelect();
    }
    void UpdateDataToUi(object obj)
    {
        if (!IsRunning)
            return;
        for (int index = 0; index < mTaskTable.Length; index++)
        {
            if (mTaskTable[index]!=null)
				(obj as CSUI_CollectWnd).UpdateProcess(index, mTaskTable[index].itemList, mTaskTable[index].npcList.Count, mTaskTable[index].CountTime(),mTaskTable[index].runCount);
        }
    }
    void OnAddClick(object sender, int index, int protoId, int curCout)
    {
        if (!IsRunning)
            return;
        if (PeGameMgr.IsMulti)
        {
            _ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_AddItem, protoId, curCout, index);
        }
        else
        {
            if (mTaskTable[index] == null)
                mTaskTable[index] = new ProcessingTask();
            //1.find same item
            ProcessingTask pTask = mTaskTable[index];

            if(pTask.AddItem(new ItemIdCount(protoId, curCout)))
                //--to do: update UI
                UpdateLineToUI(index);
        }
    }

    //lz-2016.07.14 设置运行次数
    void OnSetRunCount(object sender, int index, int runCount)
    {
        if (PeGameMgr.IsMulti)
        {
			//向服务器发送设置Rounds消息
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_SetRound, index,runCount);
		}
		else
        {
            if (null == mTaskTable[index])
                mTaskTable[index] = new ProcessingTask();
            ProcessingTask pTaks = mTaskTable[index];
            pTaks.SetRunCount(runCount);
            UpdateLineToUI(index);
        }
    }

    public void OnRemoveClick(object sender, int index, int protoId)
    {
        if (!IsRunning)
            return;
        if (PeGameMgr.IsMulti)
        {
            _ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_RemoveItem, index, protoId);
        }
        else
        {
            if (mTaskTable[index] == null)
            {
                return;
            }
            ProcessingTask pTask = mTaskTable[index];
            pTask.RemoveItem(protoId);
            UpdateLineToUI(index);
        }
    }

    public void TryStartProcessing(object sender,int index)
    {
        if (!IsRunning)
            return;
        if (PeGameMgr.IsMulti)
        {
            _ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_Start, index);
        }
        else
        {
            if (mTaskTable[index] == null)
                return;
            int errorCode;
            if (mTaskTable[index].CanStart(out errorCode))
            {
                StartProcessing(index);
            }
            else
            {
                if (errorCode > 0){
                    CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(errorCode));
					CSUI_MainWndCtrl.Instance.GoToPersonnelWorkWnd();
				}
            }
            UpdateSelectedTask();
        }
    }
    void OnSelectProcess(object sender, int index)
    {
//        if (!IsRunning)
//            return;
        selectProcess = mTaskTable[index];
        UpdateSelectedTask();
    }
    public void TryStop(object sender, int index)
    {
        if (!IsRunning)
            return;
        if (PeGameMgr.IsMulti)
        {
            _ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_Stop, index);
        }
        else
        {
            Stop(index);
            UpdateSelectedTask();
        }
    }
    void SetAuto(object sender, bool flag)
    {
        if (!IsRunning)
            return;
        if (PeGameMgr.IsMulti)
        {
            BuildingLogic.network.RPCServer(EPacketType.PT_CL_PRC_SetAuto, flag);
        }
        IsAuto = flag;
    }
    #endregion

    void UpdateLineToUI(int index)
    {
        if(index>=0 && index<mTaskTable.Length && mTaskTable[index]!=null)
            if (uiObj != null)
            {
                uiObj.Init();
				uiObj.UpdateProcess(index, mTaskTable[index].itemList, mTaskTable[index].npcList.Count, mTaskTable[index].CountTime(),mTaskTable[index].runCount);
			}
	}
	
    void UpdateSelectedTask(){
        if (selectProcess != null)
        {
            if (selectProcess.cs != null)
            {
                if (uiObj != null)
                    uiObj.ShowStartBtn(false);
            }
            else
            {
                if (uiObj != null)
                    uiObj.ShowStartBtn(true);
            }
        }
        else
        {
            if (uiObj != null)
                uiObj.ShowStartBtn(true);
        }
    }

    public void SetSelect()
    {
        List<ItemIdCount> orclist = new List<ItemIdCount>();
        List<ItemIdCount> herblist = new List<ItemIdCount>();
        List<ItemIdCount> otherlist = new List<ItemIdCount>();
        foreach (ProcessingObjInfo pob in ProcessingObjInfo.GetAllInfo())
        {
            switch (pob.tab)
            {
                case 0: orclist.Add(new ItemIdCount(pob.protoId, pob.max)); break;
                case 1: herblist.Add(new ItemIdCount(pob.protoId, pob.max)); break;
                default: otherlist.Add(new ItemIdCount(pob.protoId, pob.max)); break;
            }
        }

        uiObj.ClearPlan();
        uiObj.AddOreList(orclist);
        uiObj.AddHerbList(herblist);
        uiObj.AddOtherList(otherlist);
        uiObj.ClearProcess();
    }


    public override void Update()
    {
        base.Update();
		CheckAllProcessor();
        if (!IsRunning)
        {
            for (int i = 0; i < mTaskTable.Length; i++)
            {
                if (mTaskTable[i] != null)
                {
                    if (mTaskTable[i].cs != null)
                    {
                        mTaskTable[i].cs.enabled = false;
                    }
                }
            }
            return;
        }
        else
        {
            for (int i = 0; i < mTaskTable.Length; i++)
            {
                if (mTaskTable[i] != null)
                {
                    if (mTaskTable[i].cs != null)
                        mTaskTable[i].cs.enabled = true;
                }
            }
        }

        for (int i = 0; i < mTaskTable.Length; i++)
        {
            
            if(mTaskTable[i]!=null){
                mTaskTable[i].Update();
                if (mTaskTable[i].cs != null && uiObj != null)
                    uiObj.UpdateTimes(i, mTaskTable[i].TimeLeft);
            }
            if (!PeGameMgr.IsMulti)
            {
                if (IsAuto)
                {
                    DoAuto();
                }
            }
        }
    }
    public void LoadData()
    {
        //--to do:
    }

    public void AddItemToTask(int protoId,int count,int taskIndex)
    {
        if (PeGameMgr.IsMulti)
        {
            _ColonyObj._Network.RPCServer(EPacketType.PT_CL_PRC_AddItem, protoId, count, taskIndex);
        }
        else
        {
            if (mTaskTable[taskIndex] == null)
            {
                mTaskTable[taskIndex] = new ProcessingTask();
            }
            if (!mTaskTable[taskIndex].IsWorking())
                mTaskTable[taskIndex].itemList.Add(new ItemIdCount(protoId, count));
            else
            {
                //--to do: fail prompt
            }
        }
    }

    public void AddNpcToTask(CSPersonnel npc,int index)
    {
        if (mTaskTable[index] == null)
        {
            mTaskTable[index] = new ProcessingTask();
        }
        mTaskTable[index].AddNpc(npc);
        UpdateLineToUI(index);
    }

    public override void RemoveWorker(CSPersonnel npc)
    {
        base.RemoveWorker(npc);
        if(npc.ProcessingIndex>=0){
			npc.ProcessingIndex = -1;
		}
    }

    public void RemoveNpcFromTask(CSPersonnel npc, int index)
    {
        //lw: Bug report Win64 Steam Version:V1.0.5 from SteamId 76561198109774152
        //收采集厂时 mTaskTable[index]可能为空？
        if (mTaskTable[index] != null)
        {
            mTaskTable[index].RemoveNpc(npc);
            UpdateLineToUI(index);
        }
    }

    public void SetNpcProcessingIndex(CSPersonnel npc,int old, int index)
    {
        if (!AddWorker(npc))
        {
            return;
        }
        if (old == -1 && index != -1)
        {
            AddNpcToTask(npc, index);
        }
        else if (old != -1 && index == -1)
        {
            RemoveNpcFromTask(npc, old);
        }
        else if (old != -1 && index != -1)
        {
            NpcLineChange(npc, old, index);
        }
        UpdateSelectedTask();
    }
    
    public void StartProcessing(int index)
    {
        //--to do: check npc can
		if(Application.isEditor&&InTest)
			mTaskTable[index].Start(5);
		else
        	mTaskTable[index].Start();
		mTaskTable[index].taskAccomplished = TaskAccomplished;
		UpdateLineToUI(index);
		
		if (uiObj != null)
            uiObj.ShowStartBtn(false);
    }

    

    public void Stop(int index)
    {
        if (mTaskTable[index] == null )
        {
            return;
        }
        if (PeGameMgr.IsSingle && mTaskTable[index].cs == null)
        {
            return;
        }

        mTaskTable[index].StopCounter();
        if (uiObj != null)
            uiObj.ShowStartBtn(true);
    }
	public void SyncStop(int index)
	{
		if (mTaskTable[index] == null )
		{
			return;
		}
		
		mTaskTable[index].SyncStopCounter();
		if (uiObj != null)
			uiObj.ShowStartBtn(true);
	}
    public void TaskAccomplished(ProcessingTask pTask,float percent=1)
    {
        //get resource & renew task
        GetTaskResult(percent, pTask.itemList);
		int index = mTaskTable.ToList().FindIndex(it=>it==pTask);
		if(percent<1){
			pTask.ClearItem();
			//--to do: updateUI
			UpdateLineToUI(index);
			if(pTask==selectProcess)
				if (uiObj != null)
					uiObj.ShowStartBtn(true);
			pTask.SetRunCount(0);
		}
		else{
			if(pTask.runCount<=1){
				pTask.ClearItem();
				//--to do: updateUI
				UpdateLineToUI(index);
				if(pTask==selectProcess)
					if (uiObj != null)
						uiObj.ShowStartBtn(true);
				pTask.SetRunCount(0);
			}
			else{
				pTask.CountDownRepeat();
				StartProcessing(index);
			}
		}
    }

    public RandomItemObj GetTaskResult(float percent,List<ItemIdCount> itemList)
    {
        System.Random rand = new System.Random();
        //--to do: wait
        //count item 10%random
        //create item
        //add to npcpackage
		List<ItemIdCount> resourceGot = new List<ItemIdCount> ();

        foreach (ItemIdCount item in itemList)
        {
			int getCount = Mathf.FloorToInt(item.count * percent);
			resourceGot.Add(new ItemIdCount (item.protoId,getCount));
        }
		List<ItemIdCount> itemIdNum = resourceGot.FindAll(it => it.count > 0);
        if (itemIdNum.Count <= 0)
            return null;

		//put into storage
		bool addInStorage = false;
		if(Assembly!=null&&Assembly.Storages!=null){
			List<MaterialItem> materialList = CSUtils.ItemIdCountToMaterialItem(itemIdNum);
			foreach(CSCommon css in Assembly.Storages){
				CSStorage storage = css as CSStorage;
				if(storage.m_Package.CanAdd(materialList)){
					storage.m_Package.Add(materialList);
					addInStorage = true;
					//--to do: inform player
					CSUtils.ShowTips(ProcessingConst.INFORM_FINISH_TO_STORAGE);
					break;
				}else{
					if(CSAutocycleMgr.Instance!=null)
						CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
				}
			}
		}
		if(addInStorage)
			return null;
        
		int[] items = CSUtils.ItemIdCountListToIntArray(itemIdNum);

        Vector3 resultPos = Position+new Vector3(0f,0.72f,0f);
		if(BuildingLogic!=null)
        if (BuildingLogic.m_ResultTrans.Length > 0 )
        {
            Transform trans = BuildingLogic.m_ResultTrans[rand.Next(BuildingLogic.m_ResultTrans.Count())];
            if(trans!=null)
                resultPos = trans.position;
		}
		while (RandomItemMgr.Instance.ContainsPos(resultPos))
		{
			resultPos += new Vector3(0, 0.01f, 0);
		}
        //return new RandomItemObj(resultPos + new Vector3((float)rand.NextDouble()*0.15f, 0, (float)rand.NextDouble()*0.15f), items);
		CSUtils.ShowTips(ProcessingConst.INFORM_FINISH_TO_RANDOMITEM);
		return RandomItemMgr.Instance.GenProcessingItem(resultPos + new Vector3((float)rand.NextDouble()*0.15f, 0, (float)rand.NextDouble()*0.15f),items);
    }

    public void DoAuto()
    {
		//1.find freeNpc
		List<CSPersonnel> freeWorkers = GetFreeWorkers();
		if(freeWorkers.Count==0)
			return;

		//2.assign task to freeNpc
		foreach(CSPersonnel worker in freeWorkers){
            if(worker!=null&&(worker.ProcessingIndex<0||!mTaskTable[worker.ProcessingIndex].CanStart())){
                int taskIndex = FindTaskNotEmptyIndex();
                if (taskIndex >= 0)
                    worker.ProcessingIndex = taskIndex;
            }
        }
        //3.start task when has npc
        foreach(ProcessingTask pt in mTaskTable){
            if (pt != null && pt.CanStart())
            {
				if(Application.isEditor&&InTest)
					pt.Start(5);
				else
                	pt.Start();
                pt.taskAccomplished = TaskAccomplished;
            }
        }
    }

    public void NpcLineChange(CSPersonnel npc,int oldIndex, int newIndex)
    {
        RemoveNpcFromTask(npc,oldIndex);
		AddNpcToTask(npc,newIndex);
        UpdateLineToUI(oldIndex);
        UpdateLineToUI(newIndex);
        UpdateSelectedTask();
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (PeGameMgr.IsMulti)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtProcessing, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtProcessing, ref ddata);
        }
        m_Data = ddata as CSProcessingData;

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
            for (int i = 0; i < Data.mTaskTable.Length; i++)
            {
                if (Data.mTaskTable[i] != null)
                {
                    Data.mTaskTable[i].StartCounterFromRecord();
                    Data.mTaskTable[i].taskAccomplished = TaskAccomplished;
                }
            }
        }
    }

    public bool HasLine(int index)
    {
        if (mTaskTable[index] == null)
        {
            return false;
        }
        if (mTaskTable[index].itemList == null)
        {
            return false;
        }
        if (mTaskTable[index].itemList.Find(item=>item!=null) == null)
        {
            return false;
        }
        return true;
    }

    public override void RemoveData()
    {
        m_Creator.m_DataInst.RemoveObjectData(ID);
    }

    void DestroyCounter()
    {
        for (int i = 0; i < ProcessingConst.TASK_NUM; i++)
        {
            if (mTaskTable[i] != null && mTaskTable[i].cs != null)
            {
				mTaskTable[i].StopCounter();
            }
        }
    }
    public override void DestroySelf()
    {
        DestroyCounter();
        base.DestroySelf();
        if (IsMine)
        {
            UnbindEvent();
        }
    }
	public override void StopWorking(int npcId){
		CSPersonnel npc = m_MgCreator.GetNpc(npcId);
		if(npc==null)
			return;
		if(m_Workers.Contains(npc)){
			if(npc.ProcessingIndex>=0){
				npc.ProcessingIndex=-1;
			}
		}
	}

	public override void UpdateDataToUI ()
	{
		for(int i=0;i<ProcessingConst.TASK_NUM;i++)
			UpdateLineToUI(i);
	}

    #region multimode updateUI
    //additem result
    public void AddItemResult(int taskIndex)
    {
        UpdateLineToUI(taskIndex);
    }
    //removeitem result
    public void RemoveItemResult(int taskIndex)
    {
        UpdateLineToUI(taskIndex);
    }
    //addnpc result
    public void AddNpcResult()
    {

    }
    //removenpc result
    public void RemoveNpcResult()
    {

    }
	public void SetRoundResult(int taskIndex){
		UpdateLineToUI(taskIndex);
	}
    //setauto result
    public void SetAutoResult()
    {

    }
    //start result
    public void StartResult(int taskIndex)
    {

    }
    //stop result
    public void StopResult(int taskIndex)
    {
        UpdateLineToUI(taskIndex);
    }

	public void SetCounter(int index,float cur,float final,int runCount){
		if (mTaskTable[index] == null)
		{
			mTaskTable[index] = new ProcessingTask();
		}
		mTaskTable[index].StartCounterFromNet(cur,final,runCount);
	}
    #endregion

    #region interface
    public int FindTaskNotEmptyIndex()
    {
        for (int i = 0; i < mTaskTable.Length; i++)
        {
            if (mTaskTable[i] != null)
            {
                if (mTaskTable[i].HasItem())
                    return i;
            }
        }
        return -1;
    }

	public List<ItemIdCount> GetItemsInProcessing(){
		List<ItemIdCount> processingItems = new List<ItemIdCount> ();
		for (int i = 0; i < mTaskTable.Length; i++)
		{
			if (mTaskTable[i] != null)
			{
				if (mTaskTable[i].IsWorking())
				{
					ProcessingTask pt = mTaskTable[i];
					if(pt.itemList.Count>0)
					{
						foreach(ItemIdCount iic in pt.itemList){
							CSUtils.AddItemIdCount(processingItems,iic.protoId,iic.count);
						}
					}
				}
			}
		}
		return processingItems;
	}

	public List<CSPersonnel> GetFreeWorkers(){
		List<CSPersonnel> freeNpcs = new List<CSPersonnel> ();
		foreach(CSPersonnel npc in m_Workers)
		{
			if(npc!=null&&!npc.IsProcessing&&npc.CanProcess)
				freeNpcs.Add(npc);
		}
		return freeNpcs;
	}

	public void CreateNewTaskWithItems(List<ItemIdCount> allItems){
		//1.split task
		//List<ItemIdCount> taskObjList = new List<ItemIdCount> ();

		List<List<ItemIdCount>> taskList = new List<List<ItemIdCount>> ();
		taskList.Add(new List<ItemIdCount> ());
		int startIndex = 0;//first index has room
		foreach(ItemIdCount iic in allItems){
			int countMax = ProcessingObjInfo.GetPobMax(iic.protoId);
			if(countMax<0)
				continue;
			//find the first room
			while(taskList[startIndex].Count>=ProcessingConst.OBJ_MAX)
			{
				startIndex++;
				if(taskList.Count<=startIndex)
					taskList.Add(new List<ItemIdCount> ());
			}
			int addIndex = startIndex;//current index to add;
			while(iic.count>countMax){
				if(taskList.Count<=addIndex)
					taskList.Add(new List<ItemIdCount> ());
				if(taskList[addIndex].Count<ProcessingConst.OBJ_MAX){
					taskList[addIndex].Add(new ItemIdCount (iic.protoId,countMax));
					iic.count-=countMax;
				}
				addIndex++;
			}
			if(taskList.Count<=addIndex)
				taskList.Add(new List<ItemIdCount> ());
			taskList[addIndex].Add(iic);
		}

		//2.assign npc
			//1-find all free npc
		List<CSPersonnel> freeNpc = GetFreeWorkers();
		if(freeNpc.Count==0)
			return;
			//2-get free Taks room 
		List<int> freeTask = new List<int> ();
		for(int i =0;i<ProcessingConst.TASK_NUM;i++){
			if(mTaskTable[i]==null||mTaskTable[i].itemList.Count==0)
				freeTask.Add(i);
		}
		if(freeTask.Count==0)
			return;
		int needTaskNum=taskList.Count;
		int npcNum = freeNpc.Count;
		int freeTaskNum = freeTask.Count;
			//3-assign Npc
		int runTaskNum = Mathf.Min(needTaskNum,npcNum,freeTaskNum);
		int npcPerTask = npcNum/runTaskNum;
		if(runTaskNum==1&&npcNum>1)
			npcPerTask = npcNum-1;
		for(int i=0;i<runTaskNum;i++){
			int taskIndex = freeTask[i];
			if(mTaskTable[taskIndex]==null)
				mTaskTable[taskIndex]=new ProcessingTask ();
			for(int j=0;j<npcPerTask;j++)
				freeNpc[j].TrySetProcessingIndex(taskIndex);
			freeNpc.RemoveRange(0,npcPerTask);
			mTaskTable[taskIndex].itemList = taskList[i];
		}

		List<int> runRange = freeTask.GetRange(0,runTaskNum);
		foreach(CSPersonnel csp in freeNpc){
				if(runRange.Contains(csp.ProcessingIndex)){
					int changeToIndex = FindNextTaskNoStart(runRange);
					if(changeToIndex>=0)
						csp.TrySetProcessingIndex(changeToIndex);
				}
		}
		//3.start task
		for(int i=0;i<runTaskNum;i++){
			int taskIndex = freeTask[i];
			StartProcessing(taskIndex);
		}
		if(runTaskNum>0)
			CSAutocycleMgr.Instance.ShowProcessFor(taskList[0]);
		//4.updateUI
		UpdateDataToUI();
	}
	int FindNextTaskNoStart(List<int> runRange){
		for(int i =0;i<ProcessingConst.TASK_NUM;i++){
			if(runRange.Contains(i))
				continue;
			if(mTaskTable[i]!=null&&mTaskTable[i].cs!=null)
				continue;
			return i;
		}
		return -1;
	}


    public static void ParseData(byte[] data,CSProcessingData recordData)
    {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            recordData.isAuto = BufferHelper.ReadBoolean(reader);
            int taskCount = BufferHelper.ReadInt32(reader);
            for (int i = 0; i < taskCount; i++)
            {
                int taskIndex = BufferHelper.ReadInt32(reader);
                ProcessingTask pt = new ProcessingTask();
                int itemCount = BufferHelper.ReadInt32(reader);
                for (int j = 0; j < itemCount; j++)
                {
                    ItemIdCount po = new ItemIdCount();
                    po.protoId = BufferHelper.ReadInt32(reader);
                    po.count = BufferHelper.ReadInt32(reader);
                    pt.itemList.Add(po);
                }
				pt.runCount = BufferHelper.ReadInt32(reader);
                pt.m_CurTime = BufferHelper.ReadSingle(reader);
                pt.m_Time = BufferHelper.ReadSingle(reader);
                recordData.mTaskTable[taskIndex] = pt;
            }
        }
    }

	public static bool CanProcessItem(int protoId){
		return ProcessingObjInfo.GetPobTime(protoId)>0;
	}
	
    #endregion
}
