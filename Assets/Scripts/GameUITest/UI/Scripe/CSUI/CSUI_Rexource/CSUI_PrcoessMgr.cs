using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class ProcessInfo
{
	public Texture mTexture;
	public int ItemId;
	public int ProtoId;
	public int ProcessNum;
	public int mCurentNum;
	public int m_NeedNum;
	public string IconName;
}

public class CSUI_PrcoessMgr : MonoBehaviour {

	// Use this for initialization
	[SerializeField] public UIGrid mProcessGrid;
	[SerializeField] public GameObject m_ProcessGridPrefab;
	[SerializeField] UICheckbox mProcessSeclect;

	private List<CSUI_PrcoessGrid>	m_PrcoessItemList = new List<CSUI_PrcoessGrid>();

	public UILabel mProcessNumLb;
	public UILabel mNpcNumLb;
	public UILabel mTimeLb;
    public UILabel mRunCountLb;

	private List<ProcessInfo>	m_ProcessInfoList = new List<ProcessInfo>();

    //lz-2016.07.14 记录RunCount，选中这个Process的时候设置到RunCountInput
    private int m_RunCount;
    public int RunCount { get { return m_RunCount; } }


	int mProcessId;
	public int ProcessId
	{
		get
		{
			return mProcessId;
		}
		set
		{
			mProcessId =value;
		}
	}

	public bool  IsChecked;
	float m_Times =0;
	public float Times
	{
		get
		{
			return m_Times;
		}
		set
		{
			m_Times = value;
		}
	}

	void Awake()
	{
		InitWnd();
	}

	void Start () 
	{
		//Test();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	#region InitFace

	public void InitWnd()
	{
		GetComponent<UICheckbox>().radioButtonRoot = transform.parent;

		mProcessId = System.Convert.ToInt32(mProcessNumLb.text);
		for(int i=0;i<12;i++)
		{
			ProcessInfo Gird = new ProcessInfo();
			AddProcessGrid(Gird);
		}

        //lz-2016.07.14 初始化的时候根据IsChecked状态设置mProcessSeclect
        if (IsChecked)
        {
            mProcessSeclect.isChecked = IsChecked;
        }
	}

	 public void SetNpcNum(int npcNum)
	{
		mNpcNumLb.text = "NPC:" + npcNum.ToString();
	}

	public void SetTime(float times)
	{
		int Times = (int)times;
		m_Times = times;
		int hours = ((Times/60)/60)%24;
		int minute = (Times/60)%60;
		int second = Times%60;
		
		mTimeLb.text = hours.ToString() +":" + minute.ToString() +":" +second.ToString();
	}

    public void SetRunCount(int runCount)
    {
        m_RunCount = runCount;
        mRunCountLb.text = PELocalization.GetString(8000593) + runCount.ToString();
    }

	void SetProcessNum(int Num)
	{
		ProcessId = Num;
		//mProcessNumLb.text = Num.ToString();
	}

	//test
	public void AddGerid(GridInfo Info)
	{
		if(m_ProcessInfoList.Count >12)
			return ;

		ProcessInfo pInfo = new ProcessInfo ();
		pInfo.IconName = Info.IconName[0];
		pInfo.m_NeedNum = (int)Info.CurrentNum;
		pInfo.ItemId = Info.mProtoId;
		m_ProcessInfoList.Add(pInfo);
			ReflashItem();

	}


	public void UpdateGridInfo(List<ProcessInfo> infoList)
	{
		foreach(ProcessInfo info in infoList)
		{
			SetProcessNum(info.ProcessNum);
			m_PrcoessItemList[info.ItemId].UpdateGridInfo(info);
		}
	}


	public void RemoveGrid(int itemid)
	{
		foreach(CSUI_PrcoessGrid grid in m_PrcoessItemList)
		{
			if(grid.ItemID == itemid)
			{
				grid.ClearInfo();
			}
		}
	}

	public void ClearGrideInfo()
	{
		foreach(CSUI_PrcoessGrid grid in m_PrcoessItemList)
		{
			grid.ClearInfo();
		}
	}

	public void AddProcessGrid(int protoId,int needNum)
	{

		if(m_ProcessInfoList.Count >10)
			return ;
			
		ProcessInfo Info = new ProcessInfo ();

		ItemProto itemprto = ItemProto.Mgr.Instance.Get(protoId);
		if(itemprto != null )
		{
			Info.IconName = itemprto.icon[0];
			Info.m_NeedNum = needNum;
			Info.ItemId = protoId;
			m_ProcessInfoList.Add(Info);
			ReflashItem();
		}

	}
	
	public void AddProcessGrid(ProcessInfo Info)
	{
		if(Info == null )
			return ;

		if(m_ProcessInfoList.Count >=12)
			return ;

		m_ProcessInfoList.Add(Info);
		ReflashItem();
	}

	 public void RemoveProcessGrid(ProcessInfo Info)
	{

		if(Info == null )
			return ;

		if(m_ProcessInfoList.Remove(Info))
		  ReflashItem();
	}

	 public void ClearProcessGrid()
	{
		m_ProcessInfoList.Clear();
		ReflashItem();
	}

     //lz-2016.06.27 选中当前进程
     public void SelectProcess()
     {
         this.OnProcessActivate(true);
     }

	#endregion


	#region AddProcessGrid
	void ReflashItem()
	{
		ClearGrid();
		foreach (ProcessInfo Info in m_ProcessInfoList)
		{
			AddProcessItem(Info);
		}
	}

	void AddProcessItem(ProcessInfo Info)
	{
		GameObject obj = GameObject.Instantiate(m_ProcessGridPrefab) as GameObject;
		obj.transform.parent = mProcessGrid.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);
		
		CSUI_PrcoessGrid Item = obj.GetComponent<CSUI_PrcoessGrid>();

		//Item.SetIcon(Info.IconName);
		//Item.ItemID = Info.ItemId;
		Item.mProcessInfo = Info;

		Item.e_OnDeleteClick += OnDelete;
		Item.e_OnSelectClick += OnSelect;

		m_PrcoessItemList.Add(Item);

		mProcessGrid.repositionNow = true;
	}

	void ClearGrid()
	{
		foreach (CSUI_PrcoessGrid item in m_PrcoessItemList)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_PrcoessItemList.Clear();
	}


	#endregion 


	#region Event_click

	public delegate void ProcesssClickEvent(object sender,int processId);
	public event ProcesssClickEvent e_ProcesssClickEvent = null;

	public delegate void ItemRemove(object sender,int protoId);
	public event ItemRemove e_ItemRemove = null;

	void OnDelete(object sender,int ItemId,int ProtoId)
	{
		CSUI_PrcoessGrid Gird = sender as CSUI_PrcoessGrid;
	
		if(Gird != null)
		{
			if(e_ItemRemove != null)
			{
				e_ItemRemove(this,ProtoId);
			}
		}
	}

	void OnSelect(object sender)
	{
		CSUI_PrcoessGrid Gird = sender as CSUI_PrcoessGrid;
		if(Gird != null)
		{		

		}
	}

	void OnProcesssBtn()
	{

	}

	void OnProcessActivate(bool active)
	{
		IsChecked = active;
		if(e_ProcesssClickEvent != null&&active)
		{
			e_ProcesssClickEvent(this,mProcessId);
		}
        mProcessSeclect.isChecked = active;
		foreach(CSUI_PrcoessGrid grid in m_PrcoessItemList)
		{
			grid.SetGridBox(active);
		}
	}
	#endregion
}
