using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using ItemAsset.PackageHelper;

public class CSUI_CollectWnd : MonoBehaviour
{

    #region delegate event
    public delegate void UpdateCollectEvent(object sender);
	public event UpdateCollectEvent e_UpdateCollect = null;

	public delegate void InitCollectEvent(object sender);
	public event InitCollectEvent e_InitCollectEvent = null;

    public delegate void BaseFristClick(object sender, bool active);
    public event BaseFristClick e_BaseFristClick = null;

    public delegate void AutoClick(object sender, bool active);
    public event AutoClick e_AutoClick = null;

    public delegate void StarrClick(object sender, int Index);
    public event StarrClick e_StartClick = null;

    public delegate void StopClick(object sender, int Index);
    public event StopClick e_StopClick = null;

    public delegate void AddtoClick(object sender, int index, int protoId, int curCout);
    public event AddtoClick e_AddtoClick = null;

    public delegate void RemoveEvent(object sender, int index, int protoId);
    public event RemoveEvent e_RemoveEvent = null;

    public delegate void ProcessChoseEvent(object sender, int index);
    public event ProcessChoseEvent e_ProcessChoseEvent = null;

    //lz-2016.07.14 设置运行次数事件
    public delegate void SetRunCount(object sender, int index, int runCount);
    public event SetRunCount e_SetRunCountEvent;

    #endregion

    #region serialize field
    [SerializeField]
    public CSUI_PrcoessMgr[] m_Processes;
    public Dictionary<int, List<ProcessInfo>> mProlists = new Dictionary<int, List<ProcessInfo>>();
    public bool isInited = false;
    [SerializeField]UIInput mNumInput;
    [SerializeField]UIInput mRunCountInput;
    [SerializeField]N_ImageButton mStartBtn;
    [SerializeField]N_ImageButton mStopBtn;
    [SerializeField]N_ImageButton mAddBtn;
    public int CurProtoID;
    public int MaxNum
    {
        get
        {
            return m_MaxNum;
        }
        set
        {
            m_MaxNum = value;
            mNewNum = 1;
        }
    }

    public int MaxRunCount
    {
        get { return m_MaxRunCount; }
        set { m_MaxRunCount = value; mNewRunCount = 0;}
    }
    #endregion

    #region private field
    int m_Index = 0;
    //number op
    bool mAddNumBtnPress = false;
    bool mSubNumBtnPress = false;
    float mNumOpStarTime;
    int mCurrentNum=0;
    int mNewNum=1;
    float mOpDurNum = 0;
    int m_MaxNum = 10;
    //runCount op
    bool mAddRunCountBtnPress = false;
    bool mSubRunCountBtnPress = false;
    float mRunCountOpStartTime;
    int mCurrentRunCount=0;
    int mNewRunCount=1;
    float mOpDurRunCount;
    int m_MaxRunCount = 10;

    //lz-2016.07.14 避免没有变化的时候发消息
    int m_BackRunCount;
    #endregion

    #region Unity
    void Awake()
	{
	//	InitProcess();
		InitWnd();

        //lz-2016.07.14 对输入的数范围进行限制
        UIEventListener.Get(mRunCountInput.gameObject).onSelect += (go,isSelect) => {
            if (!isSelect)
            {
                if (mRunCountInput.text == "")
                {
                    mNewRunCount= 1;
                }
                else
                    mNewRunCount = Mathf.Clamp(int.Parse(mRunCountInput.text), 1, m_MaxRunCount);
                OnSetRunCount();
            }
        };

        //lz-2016.07.14 对输入的数范围进行限制
        UIEventListener.Get(mNumInput.gameObject).onSelect += (go, isSelect) =>
        {
            if (!isSelect)
            {
                if (mNumInput.text == "")
                    mNewNum = 1;
                else
                    mNewNum = Mathf.Clamp(int.Parse(mNumInput.text), 1, m_MaxNum);
            }
        };
	}
	void Start ()
	{
		InitEnvent();
        UpdateCollect();
	}

	public void InitWnd()
	{
		if(isInited == false)
		{
		   //InitProcess();
		   isInited = true;
		}
		 
	}

	public void Init()
	{
		if(isInited == false)
		{
			InitProcess();
			isInited = true;
		}
	}

	void PlanInit()
	{	
//		GridList Plan = new GridList();
//		
//		for(int i=0;i<20;i++)
//		{
//			GridInfo Info = new GridInfo();
//			Info.IconName[0] = "build_soil";
//			Info.MaxNum = i*5;
//			Plan.OreList.Add(Info);
//		}
//		
//		for(int i=0;i<20;i++)
//		{
//			GridInfo Info = new GridInfo();
//			Info.IconName[0] = "herb_laurel";
//			Info.MaxNum = i*5;
//			Plan.HerbList.Add(Info);
//		}
//		
//		for(int i=0;i<20;i++)
//		{
//			GridInfo Info = new GridInfo();
//			Info.IconName[0] = "debris_046";
//			Info.MaxNum = i*5;
//			Plan.OtherList.Add(Info);
//		}
//		AddMainPlan(Plan);
	}
	
	void Update () 
	{
		UpdataInput();
		ButtonCtr();
	}
	#endregion
	
	#region Interface

	public void SetEnity(CSEntity enity)
	{
		if (enity == null)
		{
			Debug.LogWarning("Reference Entity is null.");
			return;
		}

		CSUI_MainWndCtrl.Instance.mSelectedEnntity = enity;
	}

	public void AddMainPlan(GridList Plan)
	{

		if(CSUI_PlanMgr.Instance == null)
			return ;

		CSUI_PlanMgr.Instance.PlanList = Plan;

	}

	public void AddOreList(List<ItemIdCount> list)
	{
		if(list == null)
			return ;

		foreach(ItemIdCount ItemId in list)
		{
			AddOrePage(ItemId.protoId,ItemId.count);
		}
	}

	public void AddHerbList(List<ItemIdCount> list)
	{
		if(list == null)
			return ;
		
		foreach(ItemIdCount ItemId in list)
		{
			AddHerbPage(ItemId.protoId,ItemId.count);
		}
	}

	public void AddOtherList(List<ItemIdCount> list)
	{
		if(list == null)
			return ;
		
		foreach(ItemIdCount ItemId in list)
		{
			AddOtherPage(ItemId.protoId,ItemId.count);
		}
	}

	public void ClearOreList()
	{
		if(CSUI_PlanMgr.Instance == null)
			return;
		
		CSUI_PlanMgr.Instance.ClearOrePage();
		return;
	}

	public void ClearHerbList()
	{
		if(CSUI_PlanMgr.Instance == null)
			return;
		
		CSUI_PlanMgr.Instance.ClearHerbPage();
		return;
	}

	public void ClearOtherList()
	{
		if(CSUI_PlanMgr.Instance == null)
			return;
		
		CSUI_PlanMgr.Instance.ClearOtherPage();
		return;
	}
	

	public bool AddOrePage(int protoId,int maxNum)
	{
		if(CSUI_PlanMgr.Instance == null)
			return false;

		CSUI_PlanMgr.Instance.AddOrePage(protoId,maxNum);
		return true;
	}

	public bool  AddHerbPage(int protoId,int maxNum)
	{
		if(CSUI_PlanMgr.Instance == null)
			return false;

		CSUI_PlanMgr.Instance.AddHerbPage(protoId,maxNum);

		return true;
	}

	public bool AddOtherPage(int protoId,int maxNum)
	{
		if(CSUI_PlanMgr.Instance == null)
			return false;

		CSUI_PlanMgr.Instance.AddOtherPage(protoId,maxNum);
		return true;
	}

	public bool  RemoveOrePage(int protoId)
	{
		if(CSUI_PlanMgr.Instance == null)
			return false;

		CSUI_PlanMgr.Instance.RemoveOrePage(protoId);

		return true;
	}

	public bool  RemoveHerbPage(int protoId)
	{
		if(CSUI_PlanMgr.Instance == null)
			return false;
		
		CSUI_PlanMgr.Instance.RemoveHerbPage(protoId);
		
		return true;
	}

	public bool  RemoveOtherPage(int protoId)
	{
		if(CSUI_PlanMgr.Instance == null)
			return false;
		
		CSUI_PlanMgr.Instance.RemoveOtherPage(protoId);
		
		return true;
	}


	public void ClearPlan()
	{
		if(CSUI_PlanMgr.Instance == null)
			return ;

		if(CSUI_PlanMgr.Instance.PlanList == null)
			return ;

		CSUI_PlanMgr.Instance.PlanList.ClearList();
	}


	public void UpdateProcess(int index,List<ItemIdCount> list,int NpcNum,float times,int runCount)
	{
		ClearProcess(index);
		foreach(ItemIdCount Item in list)
		{
			AddProcess(index,Item.protoId,Item.count);
		}
		m_Processes[index].SetTime(times);
		m_Processes[index].SetNpcNum(NpcNum);
        m_Processes[index].SetRunCount(runCount);
	}

	public void UpdateTimes(int index,float times)
	{
		m_Processes[index].SetTime(times);
	}

	public void UpdateNpcNum(int index,int NpcNum)
	{
		m_Processes[index].SetNpcNum(NpcNum);
	}

    public void UpdateRepeat(int index,int repeat)
    {
        m_Processes[index].SetRunCount(repeat);
    }
	

	public void AddProcess(int index,int ProtoID,int NeedNum)
	{
		ProcessInfo Info = new ProcessInfo();

		ItemProto itemprto = ItemProto.Mgr.Instance.Get(ProtoID);
		if(itemprto != null )
		{
			Info.IconName = itemprto.icon[0];
			Info.ProtoId = ProtoID;
			Info.m_NeedNum = NeedNum;
			Info.ProcessNum = index;
			Info.ItemId = mProlists[index].Count;
			AddProcessInfo(index,Info);
		}

	}

    public void SelectProcessByIndex(int index)
    {
        if (index >= 0 && index < m_Processes.Length)
        {
            m_Index = index;
            m_Processes[index].SelectProcess();
        }
    }

    void AddProcessInfo(int index,ProcessInfo Info)
	{
		mProlists[index].Add(Info);
		UpdateProcesses(index);

	}
	
	public void ClearProcess()
	{
		for(int i=0; i<m_Processes.Length; i++)
		{
			m_Processes[i].ClearGrideInfo();
			m_Processes[i].SetTime(0.0f);
			m_Processes[i].SetNpcNum(0);
            m_Processes[i].SetRunCount(1);
			mProlists[i].Clear();
			UpdateProcesses(i);

		}
	}

	public bool ClearProcess(int index)
	{
		if(index <0 || index > m_Processes.Length)
			return false ;

		m_Processes[index].ClearGrideInfo();
		m_Processes[index].SetTime(0.0f);
		m_Processes[index].SetNpcNum(0);
        m_Processes[index].SetRunCount(1);

        if (mProlists.ContainsKey(index))
        {
            //Prolist1.Clear();
            mProlists[index].Clear();
        }
		return true;
	}

	 void UpdateProcesses(int index)
	{
		m_Processes[index].UpdateGridInfo(mProlists[index]);
	}

	public void ShowStartBtn(bool show)
	{
		mStartBtn.gameObject.SetActive(show);
		mStopBtn.gameObject.SetActive(!show);
	}

	#endregion

	#region ProcessItem

	void InitProcess()
	{
		
		for(int i=0;i<m_Processes.Length;i++)
		{
			m_Processes[i].e_ProcesssClickEvent += OnProcessClick;
			m_Processes[i].e_ItemRemove += OnRemoveItem;
			mProlists[i] = new List<ProcessInfo>();
			m_Processes[i].InitWnd();
		}
		
	}
	#endregion

	#region Input_Num
    
	void UpdataInput()
	{
        //number
		if(mAddNumBtnPress)
		{
            SetNumberByTime(Time.time - mNumOpStarTime,ref mOpDurNum);
            if (mOpDurNum + mCurrentNum<=MaxNum)
			{
                mNewNum = (int)(mOpDurNum + mCurrentNum);
			}
			else
			{
                mNewNum = MaxNum;
			}
		}
		else if(mSubNumBtnPress)
		{
            SetNumberByTime(Time.time - mNumOpStarTime, ref mOpDurNum);
			if(mCurrentNum-mOpDurNum>=1)
			{
                mNewNum = (int)(mCurrentNum - mOpDurNum);
			}
			else 
			{
                mNewNum = 1;
			}
		} 
        else if(mCurrentNum != mNewNum)
        {
            mCurrentNum = mNewNum;
        }

        if (!mNumInput.selected)
        {
            mNumInput.text = mNewNum.ToString();
        }

        //runCount
        if (mAddRunCountBtnPress)
        {
            SetNumberByTime(Time.time - mRunCountOpStartTime, ref mOpDurRunCount);
            if (mOpDurRunCount + mCurrentRunCount<=MaxRunCount)
            {
                mNewRunCount = (int)(mOpDurRunCount + mCurrentRunCount);
            }
            else
            {
                mNewRunCount = MaxRunCount;
            }
        }
        else if (mSubRunCountBtnPress)
        {
            SetNumberByTime(Time.time - mRunCountOpStartTime, ref mOpDurRunCount);
            if (mCurrentRunCount-mOpDurRunCount>=1)
            {
                mNewRunCount = (int)(mCurrentRunCount - mOpDurRunCount);
            }
            else
            {
                mNewRunCount = 1;
            }
        }
        else if (mNewRunCount != mCurrentRunCount)
        {
            mCurrentRunCount = mNewRunCount;
        }

        if(!mRunCountInput.selected)
        {
            mRunCountInput.text = mNewRunCount.ToString();
        }
	}

    void SetNumberByTime(float time,ref float number)
    {
        if (time < 0.3f)
            number = 1;
        else if (time < 1f)
            number += 2 * Time.deltaTime;
        else if (time < 2f)
            number += 4 * Time.deltaTime;
        else if (time < 3f)
            number += 7 * Time.deltaTime;
        else if (time < 4f)
            number += 11 * Time.deltaTime;
        else if (time < 5f)
            number += 16 * Time.deltaTime;
        else
            number += 20 * Time.deltaTime;
    }


    //Number
	void OnNumAddBtnPress()
	{
		mAddNumBtnPress = true;
		mNumOpStarTime = Time.time;
	}
	
	
	void OnNumAddBtnRelease()
	{
		mAddNumBtnPress = false;
	}
	
	void OnNumSubstructBtnPress()
	{
		mSubNumBtnPress = true;
		mNumOpStarTime = Time.time;
	}
	
	void OnNumSubstructBtnRelease()
	{
		mSubNumBtnPress = false;
	}

	void OnNumMaxBtn()
	{
		mNewNum = m_MaxNum;
	}

	void OnNumMinBtn()
	{
        mNewNum = 1;
	}

    //RunCount

    void OnRunCountAddBtnPress()
    {
        mAddRunCountBtnPress = true;
        mRunCountOpStartTime = Time.time;
    }

    void OnRunCountAddBtnRelease()
    {
        mAddRunCountBtnPress = false;
        OnSetRunCount();
    }

    void OnRunCountSubstructBtnPress()
    {
        mSubRunCountBtnPress = true;
        mRunCountOpStartTime = Time.time;
    }

    void OnRunCountSubstructBtnRelease()
    {
        mSubRunCountBtnPress = false;
        OnSetRunCount();
    }

    void OnRunCountMaxBtn()
    {
        mNewRunCount = m_MaxRunCount;
        OnSetRunCount();
    }

    void OnRunCountMinBtn()
    {
        mNewRunCount = 1;
        OnSetRunCount();
    }

	#endregion

	#region Click_envent
	
    bool ProcessChecked()
	{
		for(int i=0;i<m_Processes.Length;i++)
		{
			if(m_Processes[i].IsChecked)
				return true;
		}
		return false;
	}

	void ButtonCtr()
	{
		if((mCurrentNum >0) && ProcessChecked())
		{
			mAddBtn.disable = false;
		}
		else
			mAddBtn.disable = true;
	}

	void OnBaseFistBtn(bool active)
	{
		if(e_BaseFristClick != null )
		{
			e_BaseFristClick(this,active);
		}
	}

	void OnAutoBtn(bool active)
	{
		if(e_AutoClick != null )
		{
			e_AutoClick(this,active);
		}
	}

	void OnAddToBtn()
	{
		if(e_AddtoClick != null&& mCurrentNum>=0)
		{
			e_AddtoClick(this,m_Index,CurProtoID, Mathf.Clamp(mCurrentNum,1,m_MaxNum));
		}
	}

	void OnStartBtn()
	{
		if(e_StartClick != null)
		{
			e_StartClick(this,m_Index);
		}
	}

    //lz-2016.07.14
    void OnSetRunCount()
    {
        mCurrentRunCount = mNewRunCount;
        if (null != e_SetRunCountEvent && m_BackRunCount != mCurrentRunCount)
        {
            m_BackRunCount = mCurrentRunCount;
            e_SetRunCountEvent(this, m_Index,mCurrentRunCount);
        }
    }

	void OnStopBtn()
	{
		if(e_StopClick != null)
		{
			e_StopClick(this,m_Index);
		}
	}


	void InitEnvent()
	{
		if(e_InitCollectEvent != null)
		{
			e_InitCollectEvent(this);
		}
	}

	public void UpdateCollect()
	{
		if(e_UpdateCollect != null)
		{
			e_UpdateCollect(this);
		}
	}

	void OnProcessClick(object sender, int processId)
	{
		CSUI_PrcoessMgr process = sender as CSUI_PrcoessMgr;
		if(process != null)
		{
			m_Index = processId;
			if(e_ProcessChoseEvent != null)
			{
				e_ProcessChoseEvent(this,m_Index);
			}

            this.m_BackRunCount = -1;
            //lz-2016.07.14 选中这个Processe的时候把这个Processe的RunCount更新到RunCount输入框
            mNewRunCount = Mathf.Clamp(process.RunCount, 0, m_MaxRunCount);
		}
	}

	void OnRemoveItem(object sender, int processId)
	{
		if(e_RemoveEvent !=null)
		{
			e_RemoveEvent(this,m_Index,processId);
		}
	}

	#endregion

}
