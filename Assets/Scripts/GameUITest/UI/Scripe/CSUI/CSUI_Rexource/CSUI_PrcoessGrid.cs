using UnityEngine;
using System.Collections;
using ItemAsset;

public class CSUI_PrcoessGrid : MonoBehaviour {


	[SerializeField] GameObject mDeleteBtn;
	[SerializeField] GameObject mGridSeclect;
	[SerializeField] UISprite mIcon;
	[SerializeField] UILabel mNeedNumLb;


	public delegate void OnDelete(object sender,int ItemId,int ProtoID);
	public event OnDelete e_OnDeleteClick = null;

	public delegate void OnSelect(object sender);
	public event OnSelect e_OnSelectClick = null;
	bool bSelect = false ;
	// Use this for initialization

	int m_itemID;
	public int ItemID
	{
		set
		{
			m_itemID = value;

		}
		get
		{
			return m_itemID;
		}
	}

	int m_ProtoId;
	public int ProtoID
	{
		get
		{
			return m_ProtoId;
		}
		set
		{
			m_ProtoId = value;
		}
	}
	private ItemSample mItemSample = null;
	private ListItemType mType;
	public ListItemType Type
	{
		get
		{
			return mType;
		}
		set
		{
			mType = value ;
		}
	}

	ProcessInfo m_ProcessInfo;
	public ProcessInfo mProcessInfo
	{
		get
		{
			return m_ProcessInfo;
		}
		set
		{
			m_ProcessInfo = value;
			InitProcess();
		}
	}

	int mNeedNum;
	public int NeedNum
	{
		get
		{
			return mNeedNum;
		}
		set
		{
			mNeedNum = value;
			if(mNeedNum > 0)
			{
			  mNeedNumLb.gameObject.SetActive(true);
			  mNeedNumLb.text = mNeedNum.ToString();
			
			}
			else
			{
				mNeedNumLb.gameObject.SetActive(false);
				//mNeedNumLb.text = mNeedNum.ToString();
			}
		}
	}
	
	void Awake()
	{
		InitWnd();
	}

	void Start () 
	{
	
	}
	
	void InitProcess()
	{
		SetIcon(m_ProcessInfo.IconName);
		ItemID = m_ProcessInfo.ItemId;
		NeedNum = m_ProcessInfo.m_NeedNum;
	}

	void InitWnd()
	{
		GetComponent<UICheckbox>().radioButtonRoot = transform.parent;

	}


	public void UpdateGridInfo(ProcessInfo info)
	{
		SetIcon(info.IconName);
		ItemID = info.ItemId;
		NeedNum = info.m_NeedNum;
		ProtoID = info.ProtoId;

	}

	public void ClearInfo()
	{
		SetIcon("Null");
		ItemID = -1;
		NeedNum = 0;
		ProtoID = 0;
	}

	public void SetIcon(string SpritName)
	{
		if(SpritName == "")
		{
			mIcon.spriteName = "Null";
			 return;
		}

		mIcon.spriteName = SpritName;
		mIcon.MakePixelPerfect();
	}

	public void SetGridBox(bool active)
	{
		GetComponent<Collider>().enabled = active;
		GetComponent<UICheckbox>().isChecked = false;
	}
	
	void OnDeleteBtn()
	{
		if((e_OnDeleteClick != null ) &&(ProtoID != 0))
		{
			e_OnDeleteClick(this,ItemID,ProtoID);
			GetComponent<UICheckbox>().isChecked = false;
		}
	}

 
	void OnSelectBtn()
	{
		bSelect = !bSelect;
		if(e_OnSelectClick != null )
		{
			e_OnSelectClick(this);
		}
	}

	void OnActivate(bool active)
	{
		mGridSeclect.gameObject.SetActive((active && ProtoID != 0));
		mDeleteBtn.SetActive((active && ProtoID != 0));
	}

	void OnTooltip (bool show)
	{
		if(ListItemType.mItem == mType)
		{
			if(show == true && mItemSample == null  && m_ProtoId != 0)
				mItemSample = new ItemSample(m_ProtoId);
			else if(show == false)
				mItemSample = null;
			
			if(mItemSample != null)
			{
				string  tipStr = PELocalization.GetString(mItemSample.protoData.descriptionStringId);
				ToolTipsMgr.ShowText(tipStr);
			}
			else
			{
				ToolTipsMgr.ShowText(null);
			}
		}
		
	}
	// Update is called once per frame
	void Update () 
	{
	
	}
}
