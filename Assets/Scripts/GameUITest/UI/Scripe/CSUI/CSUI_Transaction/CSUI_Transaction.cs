using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using ItemAsset.PackageHelper;

public class CSUI_ItemInfo
{
	public int protoId;
	public int  Number;
}

public class CSUI_Transaction : MonoBehaviour 
{

	[SerializeField] public UIGrid mMidGrid;
	[SerializeField] public GameObject m_ExchangeItemPrefab;

	[SerializeField] public UIGrid mGetGrid;
	[SerializeField] public GameObject m_GetItemPrefab;

	[SerializeField] public UIGrid mCampGrid;
	[SerializeField] public GameObject m_CampItemPrefab;

	[SerializeField] public GameObject m_TradeSuccessful;

	[SerializeField] public GameObject m_UpdateTime;
	[SerializeField] public GameObject m_NPCInfo;

	[SerializeField] UITexture mNPCTexture;
	[SerializeField] UISprite mNPCHead;
	[SerializeField] UILabel mNpcNameLb;
	[SerializeField] UILabel mNpcTalkLb;
	[SerializeField] UILabel mUpdateTiemLb;
	[SerializeField] N_ImageButton mExhangeBtn;
	// Use this for initialization


	private List<CSUI_CampItem>	m_CampItem = new List<CSUI_CampItem>();
	private List<CSUI_EXchangeItem>	m_EXchangeItem = new List<CSUI_EXchangeItem>();
	private List<CSUI_GetItem>	m_GetItem = new List<CSUI_GetItem>();

	public  List<CampInfo>	m_CampList = new List<CampInfo>();
	public List<ExchangeInfo>	m_ExchangeList = new List<ExchangeInfo>();
	public List<GetInfo>	m_GetList = new List<GetInfo>();


	public delegate void ExchangeClick(object sender);
	public event ExchangeClick e_ExchangeClick = null;

	public delegate void UpdateTrade(object sender);
	public event UpdateTrade e_UpdateTrade = null;

	public delegate void CampClick(CSUI_CampItem CampItem);
	public event CampClick e_CampClick = null;

	private PlayerPackageCmpt playerPackageCmpt;
	private PeEntity player;

	public class CampInfo
	{
		public string Name;
	}

	public class ExchangeInfo
	{
		public Texture mTexture;
		public string mIcon;
		public int mMaxNum;
		public int mCurentNum;
		public int mProtoId;
	}

	public class GetInfo
	{
		public string mIcon;
		public int mMaxNum;
		public int mProtoId;
	}

	public Texture NPCTexture
	{
		set
		{
			mNPCTexture.mainTexture = value ;
		}
	}

	public string NpcName
	{
		set
		{
			mNpcNameLb.text = value ;
		}
	}

	void Awake ()
	{
		UpdateEvent();
	}

	public void UpdateEvent()
	{
		if(e_UpdateTrade != null )
		{
			e_UpdateTrade(this);
		}

		foreach (CSUI_CampItem item in m_CampItem)
		{
			if(item == null)
				return ;
			
			item.SetChoeBg(true);
			if(e_CampClick != null)
			{
				e_CampClick(item);
			}
			mExhangeBtn.disable = false;
			return ;
		}
	}

	void Start()
	{
		foreach (CSUI_CampItem item in m_CampItem)
		{
			if(item == null)
				return ;
			
			item.SetChoeBg(true);
			if(e_CampClick != null)
			{
				e_CampClick(item);
			}
			return ;
		}

		

	}
	// Update is called once per frame
	void Update () 
	{
		UpdatePackageNum();
	}

	void UpdatePackageNum()
	{
		if (PeCreature.Instance.mainPlayer != null)
		{
			if (playerPackageCmpt == null || player == null || player != PeCreature.Instance.mainPlayer)
			{
				player = PeCreature.Instance.mainPlayer;
				playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
			}
			//bool bEnough = true;
			
			foreach (CSUI_EXchangeItem item in m_EXchangeItem)
			{
				if(item.ProtoId >0)
				{
					item.PackageNum =  playerPackageCmpt.package.GetCount(item.ProtoId);
				}

			}

			foreach (CSUI_GetItem item in m_GetItem)
			{
				if(item.ProtoId >0)
				{
					item.PackageNum =  playerPackageCmpt.package.GetCount(item.ProtoId);
				}
			}
		}
	}

	void Test()
	{

		for(int i = 0; i<3;i++)
		{
			CampInfo Info = new CampInfo ();
			Info.Name = "Camp"+i.ToString();
			m_CampList.Add(Info);
		}


		for(int i=0; i<5;i++)
		{
			ExchangeInfo Info = new ExchangeInfo ();
			Info.mMaxNum = 50;
			Info.mCurentNum = 50- i;
			m_ExchangeList.Add(Info);

			GetInfo info2 = new GetInfo ();
			info2.mMaxNum = i+1;
			m_GetList.Add(info2);
		}

		Reflash();
	}


	void Test2()
	{
		for(int i = 0; i<3;i++)
		{
			string Name = "Camp"+i.ToString();
			SetCamp(Name);
		}

		for(int i=1;i<8;i++)
		{
			SetExchangeInfo(i+50,40);
		}

		for(int i=1;i<8;i++)
		{
			SetGet(i+80,40);
		}

	}

	List<CSUI_ItemInfo> InfoList =new List<CSUI_ItemInfo>();
	public List<CSUI_ItemInfo> GetTheExChangeList()
	{
		InfoList.Clear();

		foreach (CSUI_EXchangeItem item in m_EXchangeItem)
		{
			CSUI_ItemInfo Info =new CSUI_ItemInfo();
			Info.protoId =item.ProtoId;
			Info.Number = (int)item.CurrentNum;
			InfoList.Add(Info);
		}
		return InfoList;
	}

	List<CSUI_ItemInfo> InfoList2 =new List<CSUI_ItemInfo>();
	public List<CSUI_ItemInfo> GetThegetList()
	{
		InfoList2.Clear();

		foreach (CSUI_GetItem item in m_GetItem)
		{
			CSUI_ItemInfo Info =new CSUI_ItemInfo();
			Info.protoId =item.ProtoId;
			Info.Number = (int) item.CurrentNum;
			InfoList2.Add(Info);
		}
		return InfoList2;
	}

	public void ClearNpcshow()
	{
		mNPCHead.gameObject.SetActive(false);
		mNPCTexture.gameObject.SetActive(false);
		mNpcNameLb.text = "Name";
		m_NPCInfo.SetActive(false);
		mExhangeBtn.disable = true;
	
	}

	public void  SetCSEntity(List<CSEntity> menList)
	{
        if (null == menList || menList.Count <= 0)
            return;
		CSEntity entity = menList[0];
		if (menList.Count > 1)
		{
		foreach (CSEntity e in menList)
		{
			if (e.IsRunning)
			{
				entity = e;
				break;
			}
		}	
		}
		CSUI_MainWndCtrl.Instance.mSelectedEnntity = entity;
	}
	public void SetUpdaTime(float mTimes)
	{
		if(mTimes < 0)
		{
			m_UpdateTime.SetActive(false);
			return ;
		}
		int Times = (int)mTimes;

		int hours = ((Times/60)/60)%24;
		int minute = (Times/60)%60;
		int second = Times%60;

		m_UpdateTime.SetActive(true);
		mUpdateTiemLb.text = hours.ToString() +":" + minute.ToString() +":" +second.ToString();
	}

	public void SetNpcHeadShow(string Name)
	{
		m_NPCInfo.SetActive(true);
		if(Name == "")
		{
			mNPCHead.spriteName = "null";
			mNPCHead.gameObject.SetActive(false);
		}
		else 
		{
		  mNPCHead.spriteName = Name;
			mNPCHead.gameObject.SetActive(true);
		}
	}

	public void SetNpcHeadShow(Texture Tex)
	{
		m_NPCInfo.SetActive(true);
		if(Tex != null)
		{
			mNPCTexture.gameObject.SetActive(false);
		}
		else
		{
			mNPCTexture.mainTexture = Tex;
			mNPCTexture.gameObject.SetActive(true);
		}

	}

	public void ShowTradeSuccess(bool Show)
	{
		m_TradeSuccessful.SetActive(Show);
	}

	public void NPCTalk(string npctalk)
	{
		mNpcTalkLb.text = npctalk;
	}

	public void SetExchangeInfo(int protoId,int MaxNum)
	{
		ExchangeInfo Info = new ExchangeInfo ();

		Info.mMaxNum = MaxNum;
		ItemProto itemprto = ItemProto.Mgr.Instance.Get(protoId);
		if(itemprto != null )
		{
			Info.mIcon = itemprto.icon[0];
			Info.mProtoId = protoId;
		}

		AddExchange(Info);
	}

	public void SetCamp(string  CampName)
	{
		CampInfo Info = new CampInfo ();
		Info.Name = CampName;
		AddCamp(Info);
	}

	public void SetGet(int protoId,int GetNum)
	{
		GetInfo Info = new GetInfo ();
		Info.mMaxNum  = GetNum;
		ItemProto itemprto = ItemProto.Mgr.Instance.Get(protoId);
		if(itemprto != null )
		{
			Info.mIcon = itemprto.icon[0];
			Info.mProtoId = protoId;
		}

		AddGet(Info);
	}

	public void ClearExchange()
	{
		m_ExchangeList.Clear();
		ReflashMid();
	}

	public void ClearCamp()
	{
		m_CampList.Clear();
		ReflashCamp();
	}

	public void ClearGet()
	{
		m_GetList.Clear();
		ReflashGet();
	}

	public void AddCamp(CampInfo Info)
	{
		m_CampList.Add(Info);
		ReflashCamp();
	}

	public void AddExchange(ExchangeInfo Info)
	{
		m_ExchangeList.Add(Info);
		ReflashMid();
	}

	public void AddGet(GetInfo Info)
	{
		m_GetList.Add(Info);
		ReflashGet();
	}

	void Reflash()
	{
		ReflashMid();
		ReflashGet();
		ReflashCamp();
	}

	void ReflashMid()
	{
		ClearExhangeItem();

		foreach (ExchangeInfo Info in m_ExchangeList)
		{
			AddMidItem(Info);
		}
	}


	void AddMidItem(ExchangeInfo Info)
	{
		GameObject obj = GameObject.Instantiate(m_ExchangeItemPrefab) as GameObject;
		obj.transform.parent = mMidGrid.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);

		CSUI_EXchangeItem exchangeItem = obj.GetComponent<CSUI_EXchangeItem>();
	

		exchangeItem.SetMaxNum(Info.mMaxNum);
		exchangeItem.SetIcon(Info.mIcon);
		exchangeItem.ProtoId = Info.mProtoId;
		exchangeItem.Type = ListItemType.mItem;

		m_EXchangeItem.Add(exchangeItem);
		mMidGrid.repositionNow = true;
	}

	void ClearExhangeItem()
	{
		foreach (CSUI_EXchangeItem item in m_EXchangeItem)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_EXchangeItem.Clear();
	}

	void ClearGetItem()
	{
		foreach (CSUI_GetItem item in m_GetItem)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_GetItem.Clear();
	}

	void ClearCampItem()
	{
		foreach (CSUI_CampItem item in m_CampItem)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_CampItem.Clear();
	}

	void ReflashGet()
	{
		ClearGetItem();
		
		foreach (GetInfo Info in m_GetList)
		{
			AddGetItem(Info);
		}
	}

	void AddGetItem(GetInfo Info)
	{
		GameObject obj = GameObject.Instantiate(m_GetItemPrefab) as GameObject;
		obj.transform.parent = mGetGrid.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);

		CSUI_GetItem getItem = obj.GetComponent<CSUI_GetItem>();
		getItem.SetTexture(Info.mIcon);
		getItem.SetCurrentNum(Info.mMaxNum);
		getItem.ProtoId = Info.mProtoId;
		getItem.Type = ListItemType.mItem;

		m_GetItem.Add(getItem);
		mGetGrid.repositionNow = true;
	}

	void ReflashCamp()
	{
		ClearCampItem();
		
		foreach (CampInfo Info in m_CampList)
		{
			AddCampItem(Info);
		}
	}

	void AddCampItem(CampInfo Info)
	{
		GameObject obj = GameObject.Instantiate(m_CampItemPrefab) as GameObject;
		obj.transform.parent = mCampGrid.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);

		CSUI_CampItem CampItem = obj.GetComponent<CSUI_CampItem>();
		CampItem.e_ItemOnClick += CampChose;
		CampItem.SetCampName(Info.Name);
		m_CampItem.Add(CampItem);

		mCampGrid.repositionNow = true;
	}

	void CampChose(object sender)
	{
		CSUI_CampItem CampItem = sender as CSUI_CampItem;
		if(CampItem != null)
		{
			foreach (CSUI_CampItem item in m_CampItem)
			{
				item.SetChoeBg(false);
			}
			CampItem.SetChoeBg(true);

			if(e_CampClick != null)
			{
				e_CampClick(CampItem);
			}
		}
	}

	void OnExchangeBtn()
	{
		if(e_ExchangeClick != null)
		{
			e_ExchangeClick(this);
		}
	}

}
