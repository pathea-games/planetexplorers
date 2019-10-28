using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using ItemAsset;

public enum PageType
{
	Ore,
	Herb,
	Other,
	Max
}

public class GridInfo
{
	public string[] IconName;
	public int MaxNum;
	public int mProtoId;
	public float CurrentNum;

	public GridInfo()
	{
		IconName = new string[3];
	}

//	public void AddIconNameByProtoId(int protoId)
//	{
//		ItemProto itemprto = ItemProto.Mgr.Instance.Get(protoId);
//		if(itemprto != null )
//		{
//			mProtoId = protoId;
//			IconName = itemprto.icon;
//		}
//	}
}

public class GridList
{
	public  List<GridInfo>	OreList;
	public  List<GridInfo>	HerbList;
	public  List<GridInfo>	OtherList;

	public void ClearList()
	{
		OreList.Clear();
		HerbList.Clear();
		OtherList.Clear();
	}

	public GridList()
	{
		OreList = new List<GridInfo>();
		HerbList = new List<GridInfo>();
		OtherList = new List<GridInfo>();
	}
	
}
public class CSUI_PlanMgr : MonoBehaviour {


	static CSUI_PlanMgr 							mInstance;
	public static CSUI_PlanMgr 					Instance{ get{ return mInstance; } }

	[SerializeField] public UIGrid mOrePageGrid;
	[SerializeField] public UIGrid mHerbPageGrid;
	[SerializeField] public UIGrid mOtherPageGrid;

	[SerializeField] public GameObject m_GridPrefab;
	[SerializeField] public GameObject m_AdjustObj;


	private List<CSUI_PageGrid>	m_OreList = new List<CSUI_PageGrid>();
	private List<CSUI_PageGrid>	m_HerbList = new List<CSUI_PageGrid>();
	private List<CSUI_PageGrid>	m_OtherList = new List<CSUI_PageGrid>();

	PageType mPageType;
	public Dictionary<PageType,List<GridInfo>> mPageList = new Dictionary<PageType, List<GridInfo>>();
	List<GridInfo> curPage = new List<GridInfo>();
	//private List<GridInfo>  m_InfoList = new List<GridInfo>();
	 
	GridList m_PlanList;
	public GridList PlanList
	{
		set
		{
			m_PlanList = value ;
			InitPlan();
		}
		get
		{
			return m_PlanList;
		}
	}
	// Use this for initialization
	void Awake()
	{
		mInstance = this;
	}
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	#region interFace
	PageType PageTye;

	void InitPlan()
	{
		mPageList[PageType.Ore] = new List<GridInfo>();
		mPageList[PageType.Ore] = m_PlanList.OreList;

		mPageList[PageType.Herb] = new List<GridInfo>();
		mPageList[PageType.Herb] = m_PlanList.HerbList;

		mPageList[PageType.Other] = new List<GridInfo>();
		mPageList[PageType.Other] = m_PlanList.OtherList;
		Relash(PageType.Ore);
	}

	public void AddOrePage(int protoId,int MaxNum)
	{
		PageTye = PageType.Ore;
		AddPageInfo(protoId,MaxNum);
	}

	public void AddHerbPage(int protoId,int MaxNum)
	{
		PageTye = PageType.Herb;
		AddPageInfo(protoId,MaxNum);
	}

	public void AddOtherPage(int protoId,int MaxNum)
	{
		PageTye = PageType.Other;
		AddPageInfo(protoId,MaxNum);
	}

	public void RemoveOrePage(int protoId)
	{
		PageTye = PageType.Ore;
		RemovePageInfo(protoId);
	}

	public void RemoveHerbPage(int protoId)
	{
		PageTye = PageType.Herb;
		RemovePageInfo(protoId);
	}

	public void RemoveOtherPage(int protoId)
	{
		PageTye = PageType.Other;
		RemovePageInfo(protoId);
	}

	public void ClearOrePage()
	{
		mPageList[PageType.Ore].Clear();
		Relash(PageType.Ore);
	}

	public void ClearHerbPage()
	{
		mPageList[PageType.Herb].Clear();
		Relash(PageType.Herb);
	}

	public void ClearOtherPage()
	{
		mPageList[PageType.Other].Clear();
		Relash(PageType.Other);
	}
	
	void AddPageInfo(int protoId,int MaxNum)
	{
		GridInfo Info = new GridInfo ();

		ItemProto itemprto = ItemProto.Mgr.Instance.Get(protoId);
		if(itemprto != null )
		{
			Info.IconName = itemprto.icon;
			Info.mProtoId = protoId;
			Info.MaxNum = MaxNum;
		}

		if(mPageList.Count == 0) 
		{
			mPageList[PageType.Ore] = new List<GridInfo>();
			mPageList[PageType.Herb] = new List<GridInfo>();		
			mPageList[PageType.Other] = new List<GridInfo>();
		}

		mPageList[PageTye].Add(Info);
		Relash(PageTye);
	}

	bool RemovePageInfo(int protoId)
	{

		GridInfo Info = mPageList[PageTye].Find(delegate(GridInfo item)
		                                             {
			if(item.mProtoId == protoId)
			{
				return true;
			}
			else
			{
				return false;
			}

		});

		if(Info != null)
		{
			return RemovePage(Info);
		}
		else
			return false ;
	}

    bool  RemovePage(GridInfo Info)
	{
		if(mPageList[PageTye].Contains(Info))
		{
			mPageList[PageTye].Remove(Info);
			Relash(PageTye);
			return true;
		}
		else
			return false;
	}

	#endregion 


	#region AddPlanGrid
	void Relash(PageType type)
	{
		switch(type)
		{
			case PageType.Ore:
			{
			ClearOreGrid();

			}
			  break;
			case PageType.Herb:
			{
			ClearHerbGrid();
			}
			  break;
			case PageType.Other:
			{
			ClearOtherGrid();
			}
			  break;
		}
	//	curPage.Clear();
		foreach (KeyValuePair<PageType,List<GridInfo>> item in mPageList)
		{
			if(item.Key == type)
			{
				curPage = item.Value;
				break;
			}
		}
		foreach (GridInfo Info in curPage)
		{
			switch(type)
			{
			case PageType.Ore:
			{
			  AddOreGrid(Info);
			}
				break;
			case PageType.Herb:
			{
				AddHerbGrid(Info);
			}
				break;
			case PageType.Other:
			{
				AddOtherGrid(Info);
			}
				break;
			}
		}

	}

	void ClearOreGrid()
	{
		foreach (CSUI_PageGrid item in m_OreList)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_OreList.Clear();
	}

	void ClearHerbGrid()
	{
		foreach (CSUI_PageGrid item in m_HerbList)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_HerbList.Clear();
	}

	void ClearOtherGrid()
	{
		foreach (CSUI_PageGrid item in m_OtherList)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_OtherList.Clear();
	}

	void AddOreGrid(GridInfo Info)
	{
		GameObject obj = GameObject.Instantiate(m_GridPrefab) as GameObject;
		obj.transform.parent = mOrePageGrid.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);
		
		CSUI_PageGrid grid = obj.GetComponent<CSUI_PageGrid>();

		grid.mGridInfo = Info;
		grid.Type = ListItemType.mItem;
		grid.e_ItemClick += ItemClick;
		m_OreList.Add(grid);	
		mOrePageGrid.repositionNow = true;
	}

	void AddHerbGrid(GridInfo Info)
	{
		GameObject obj = GameObject.Instantiate(m_GridPrefab) as GameObject;
		obj.transform.parent = mHerbPageGrid.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);
		
		CSUI_PageGrid grid = obj.GetComponent<CSUI_PageGrid>();
		grid.mGridInfo = Info;
		grid.e_ItemClick += ItemClick;

		m_HerbList.Add(grid);	
		mHerbPageGrid.repositionNow = true;
	}

	void AddOtherGrid(GridInfo Info)
	{
		GameObject obj = GameObject.Instantiate(m_GridPrefab) as GameObject;
		obj.transform.parent = mOtherPageGrid.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);
		
		CSUI_PageGrid grid = obj.GetComponent<CSUI_PageGrid>();
		grid.mGridInfo = Info;
		grid.e_ItemClick += ItemClick;


		m_OtherList.Add(grid);	
		mOtherPageGrid.repositionNow = true;
	}

	#endregion


	#region Click_event

    CSUI_PageGrid m_BackupGird; //log: lz-2016.05.20 用于取消上次选择Grid的效果和不重复点击同一个Gird
    void ItemClick(object sender)
	{
		CSUI_PageGrid gird = sender as CSUI_PageGrid;
        if (gird != null && gird != m_BackupGird)
		{
            if (null!=this.m_BackupGird)
                this.m_BackupGird.ShowGridSeclect(false);
            this.m_BackupGird = gird;
			m_AdjustObj.SetActive(true);
			if(CSUI_MainWndCtrl.Instance != null)
			{
			  CSUI_MainWndCtrl.Instance.CollectUI.MaxNum = gird.MaxNum;
			  CSUI_MainWndCtrl.Instance.CollectUI.CurProtoID = gird.mGridInfo.mProtoId;
			}
		}
	}

	#endregion


	#region Check_Btn
	[SerializeField] GameObject m_OrePage;
	[SerializeField] GameObject m_HerbPage;
	[SerializeField] GameObject m_OtherPage;    

	void PageOREOnActive(bool active)
	{
		m_OrePage.SetActive(active);
		if(active)
		{
		mPageType = PageType.Ore;
		Relash(mPageType);
		}
	}
	void PageHerbOnActive(bool active)
	{
		m_HerbPage.SetActive(active);

		if(active)
		{
		mPageType = PageType.Herb;
		Relash(mPageType);
		}
	}
	void PageOtherOnActive(bool active)
	{
		m_OtherPage.SetActive(active);

		if(active)
		{
		mPageType = PageType.Other;
		Relash(mPageType);
		}
	}



	#endregion
}
