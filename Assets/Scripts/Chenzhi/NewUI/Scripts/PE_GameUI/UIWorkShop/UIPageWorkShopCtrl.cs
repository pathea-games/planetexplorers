 using UnityEngine;
using System.Collections.Generic;
using Steamworks;
using System;
using WhiteCat;
using System.Linq;

public class UIPageWorkShopCtrl : MonoBehaviour
{
	public GameObject mLeftListItem;
	public UIGrid mleftListGrid;
	public UIPageGridCtrl mWorkShopGridCtrl;
    public WorkShopMgr mWorkShopMgr;
	public VCEUIStatisticsPanel mRinghtPanelCtrl;
	public GameObject mVCERightPanel;
	public UILabel mLbInfoMsg;
	public UIInput mInput;
    [SerializeField]
    private N_ImageButton m_DownloadBtn;
    [SerializeField]
    private UIPopupList m_PopupList0;


    private List<UIWorkShopListItem> mLeftList = null; 
	private int mLeftListSelectedIndex = -1;
	private SteamPreFileAndVoteDetail mSelectedDetail;
    //log:2016.05.17 避免多次顶或踩操作
    private List<int> m_DingList;   
    private List<int> m_CaiList;
    private uint mGetIdCount;
	

	void Awake()
	{
		InitLeftList();
		
		mWorkShopGridCtrl.mUpdateGrid += UpdateWorkShopGrid;
		mWorkShopGridCtrl.ClickWorkShop += OnClickWorkShopItem;
		mWorkShopGridCtrl.ClickWorkShopBtnReLoad += OnClickItemBtnReLoad;
		mWorkShopGridCtrl.ClickWorkShopBtnDing += OnClickItemBtnDing;
		mWorkShopGridCtrl.ClickWorkShopBtnCai += OnClickItemBtnCai;
		
		mWorkShopMgr = new WorkShopMgr();
		mWorkShopMgr.e_GetItemID += OnGetItemIDList;
		mWorkShopMgr.e_GetItemDetail += GetGridItemDetail;
		mWorkShopMgr.e_VoteDetailCallBack += OnVoteCallBack;
        this.m_DingList = new List<int>();
        this.m_CaiList = new List<int>();
        this.m_DownloadBtn.isEnabled = false;
        m_PopupList0.items.Clear();
        m_PopupList0.items.Add(PELocalization.GetString(8000698));
        m_PopupList0.items.Add(PELocalization.GetString(8000697));
        m_PopupList0.selection = m_PopupList0.items[0];
    }

	// Use this for initialization
	void Start ()
	{
        mWorkShopGridCtrl.mPagIndex = 0;
        mWorkShopGridCtrl.mMaxPagIndex = 0;
        mWorkShopMgr.InitItemList(false);
		mLeftList[0].SetSelected(true);
		mLeftListSelectedIndex = 0;
    }

	int tempFrame = 0;
	// Update is called once per frame
	void Update () 
	{
		tempFrame++;
		if (tempFrame % 10==0)
		{
			UpdateDownLoadState();
			tempFrame=0; 
		}

	}
	
	
	
	void InitLeftList()
	{
		mLeftList = new  List<UIWorkShopListItem>();
		for(int i=0; i < PEVCConfig.isoNames.Count; i++)
		{
			GameObject go = Instantiate(mLeftListItem) as GameObject;
			go.transform.parent = mleftListGrid.gameObject.transform;
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero ;

			UIWorkShopListItem item = go.GetComponent<UIWorkShopListItem>();
			item.SetText(PEVCConfig.isoNames[i].GetNameByID());

			item.SetIndex(i);
			item.SetSelected(false);
			item.ItemClick += OnChickLeftList;
			mLeftList.Add(item);
		}
		mleftListGrid.repositionNow = true;
	}
	
	void GetPageGrid()
	{
        mWorkShopMgr.GetNewItemList();
	}

	void UpdateDownLoadState()
	{
		int starIndex =Convert.ToInt32(mWorkShopMgr.mGetIdStarIndex);
		for (int i=0; i<mWorkShopGridCtrl.mUIItems.Count; i++)
		{
			if (mWorkShopGridCtrl.mUIItems[i].gameObject.activeSelf == false)
				continue;

			if(mWorkShopMgr.mIndexMap.ContainsKey(starIndex+i))
			{
				PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[starIndex+i];
				if (mWorkShopMgr.mItemsMap.ContainsKey(p_id))
				{
					SteamPreFileAndVoteDetail detail = mWorkShopMgr.mItemsMap[p_id];
					if (detail != null && mWorkShopGridCtrl.mUIItems[i].gameObject.activeSelf)
					{
						int loadByte,totleByte;
						if (SteamProcessMgr.Instance.GetDownProgress(detail.m_hFile,out loadByte,out totleByte) )
						{
							int value = 0;
							if(totleByte != 0)
								value = Convert.ToInt32((loadByte* 100)/totleByte) ;
							if (value >0)
							{
								mWorkShopGridCtrl.mUIItems[i].UpdteUpDownInfo(value.ToString() + "%");
								mWorkShopMgr.mDownMap[p_id] = value;
							}
						}
						else
						{
							if (mWorkShopMgr.mDownMap.ContainsKey(p_id))
                            {
                                if (mWorkShopMgr.mDownMap[p_id] <= 0)
                                    mWorkShopGridCtrl.mUIItems[i].UpdteUpDownInfo(PELocalization.GetString(8000911)); // "Failed!"
                                else if (mWorkShopMgr.mDownMap[p_id] >= 100)
                                    mWorkShopGridCtrl.mUIItems[i].SetDownloaded(true);
                                else
                                    mWorkShopGridCtrl.mUIItems[i].UpdteUpDownInfo(mWorkShopMgr.mDownMap[p_id].ToString() + "%");
                                this.UpdateDownloadBtnState();
                            }
						}
					}
				}
			}
			
		}
	}
	
	// event 回调
	void  UpdateWorkShopGrid(int index_0)
	{
		if (index_0 < 0)
			index_0 = 0;
        
		mSelectedDetail = null;
		mVCERightPanel.SetActive(false);

		mWorkShopMgr.mGetIdStarIndex = Convert.ToUInt32(index_0);
		
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index_0))
		{
			for (int i=0;i< mWorkShopGridCtrl.mUIItems.Count;i++)
			{
				mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(false);
				mWorkShopGridCtrl.mUIItems[i].ActiveUpDown(false);
			}
            this.GetPageGrid();
		}
		else 
		{
			for (int i=0;i< mWorkShopGridCtrl.mUIItems.Count;i++)
			{
				
				if ( mWorkShopMgr.mIndexMap.ContainsKey(index_0+ i) )
				{
					PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index_0+ i];
                    mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(true);
					if ( mWorkShopMgr.mItemsMap.ContainsKey(p_id) )
					{
						SteamPreFileAndVoteDetail del = mWorkShopMgr.mItemsMap[p_id] ;
						if (mWorkShopMgr.mDownMap.ContainsKey(p_id))
						{
							if (mWorkShopMgr.mDownMap[p_id] > 0 )
								mWorkShopGridCtrl.mUIItems[i].ActiveUpDown(true);
							else 
								mWorkShopGridCtrl.mUIItems[i].ActiveUpDown(false);
						}
						else
						{
							mWorkShopGridCtrl.mUIItems[i].ActiveUpDown(false);
						}
                        SetWorkShopGridItemInfo(del, mWorkShopGridCtrl.mUIItems[i]);
					}
					else
					{
						SetWorkShopGridItemInfo(null,mWorkShopGridCtrl.mUIItems[i]);
						continue;
					}
				}
				else 
				{
					mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(false);
				}
				mWorkShopGridCtrl.mUIItems[i].SetSelected(false);
			}
		}

	}
	
	void OnGetItemIDList(int count,int allCount)
	{
        //Debug.Log(string.Format("Count: {0} AllCount:{1}",count,allCount));
		mWorkShopGridCtrl.ReSetGrid(allCount);
        mWorkShopGridCtrl._UpdatePagText();

		for (int i=0;i< mWorkShopGridCtrl.mUIItems.Count;i++)
		{
			if ( i < count)
			{
				mWorkShopGridCtrl.mUIItems[i].ActiveLoadingItem(true);
				mWorkShopGridCtrl.mUIItems[i].InitItem(WorkGridItemType.mWorkShop, PELocalization.GetString(8000699));
				mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(true);
			}
			else 
			{
				mWorkShopGridCtrl.mUIItems[i].gameObject.SetActive(false);
			}
			mWorkShopGridCtrl.mUIItems[i].SetSelected(false);
		}
		mWorkShopGridCtrl.mGrid.repositionNow = true;
	}
	
	
	
	void GetGridItemDetail(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail)
	{
		int findIndex = FindIndexInGrid(p_id);
		if(findIndex == -1)
			return;
		
		int i = findIndex % mWorkShopGridCtrl.mMaxGridCount;
		if (i > mWorkShopGridCtrl.mMaxGridCount || i<0)
			return;
		SetWorkShopGridItemInfo(detail,mWorkShopGridCtrl.mUIItems[i]);
	}
	
	
	void SetWorkShopGridItemInfo(SteamPreFileAndVoteDetail del ,UIWorkShopGridItem item)
	{
		if (item == null)
			return;
        item.gameObject.SetActive(true);
		if (del != null)
		{
			item.SetAuthor(del.m_sUploader);
			
			item.ActiveVoteUI(true);
			
			item.SetDingText(del.m_VoteDetail.m_nVotesFor.ToString());
			item.SetCaiText(del.m_VoteDetail.m_nVotesAgainst.ToString());
			
			item.SetIsoName(del.m_rgchTitle);
            item.SetDownloaded(UIWorkShopCtrl.CheckDownloadExist(del.m_rgchTitle + VCConfig.s_IsoFileExt));
            if (del.m_aPreFileData != null)
            {
                VCIsoHeadData headData = new VCIsoHeadData();
                headData.SteamPreview = del.m_aPreFileData;

                Texture2D texture = new Texture2D(4, 4);
                texture.LoadImage(headData.IconTex);

                item.SetIco(texture);
            }
            else
            {
                item.SetIco(null);
            }
		}
		else
		{
			item.ActiveVoteUI(false);
			item.SetIco(null);
			item.SetIsoName(PELocalization.GetString(8000695));
			item.SetAuthor(PELocalization.GetString(8000695));
		}
		
		item.ActiveLoadingItem(false);
	}
	
	
	
	
	
	
	void OnClickWorkShopItem(int index)
	{
		mSelectedDetail = null;

		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			mVCERightPanel.SetActive(false);
			return;
		}
		
		PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index]; 
		if (p_id.m_PublishedFileId == 0)
		{
			mVCERightPanel.SetActive(false);
			return;
		}
		
		SteamPreFileAndVoteDetail del;
		if (mWorkShopMgr.mItemsMap.ContainsKey(p_id))
			del= mWorkShopMgr.mItemsMap[p_id];
		else 
			del = null;

		mSelectedDetail = del;
        
		if (del == null)
		{
			mVCERightPanel.SetActive(false);
			return;
		}
		mVCERightPanel.SetActive(true);
		
		VCIsoHeadData headData = new VCIsoHeadData();
		headData.SteamPreview = del.m_aPreFileData;
		
		Texture2D texture = new Texture2D(4,4);
		texture.LoadImage(headData.IconTex);
		
		mRinghtPanelCtrl.m_NonEditorIcon = headData.IconTex ;
		mRinghtPanelCtrl.m_NonEditorISODesc = del.m_rgchDescription;
		mRinghtPanelCtrl.m_NonEditorISOName = del.m_rgchTitle;
		mRinghtPanelCtrl.m_NonEditorRemark = headData.Remarks;
        mRinghtPanelCtrl.m_NonEditorISOVersion = del.m_rgchTags;//Log: lz-2016.05.13 Add version field
		mRinghtPanelCtrl.SetIsoIcon();
		mRinghtPanelCtrl.OnCreationInfoRefresh();
        this.UpdateDownloadBtnState();
	}
	
	
	
	void OnChickLeftList(int index)
	{
        if (index < 0 || index >= mLeftList.Count)
			return;
        //lz-2016.08.09 选择类型的时候清一下Item列表
        mWorkShopMgr.ClearIsoMap();

		if (mLeftListSelectedIndex != -1)
			mLeftList[mLeftListSelectedIndex].SetSelected(false);
		mLeftList[index].SetSelected(true);
		mLeftListSelectedIndex = index;

        mWorkShopGridCtrl.mPagIndex = 0;
        mWorkShopGridCtrl.mMaxPagIndex = 0;
		mWorkShopMgr.mGetIdStarIndex =0;
		
		if (mWorkShopMgr.mTagList.Count == 1)
		{
			mWorkShopMgr.mTagList.Add(PEVCConfig.isoNames[index].Tag);
		}
		else
		{
			mWorkShopMgr.mTagList[1] = PEVCConfig.isoNames[index].Tag;
		}
		GetPageGrid();
	}


	void BtnQueryOnClick()
	{
		//if (mWorkShopMgr.mQuereText == mInput.text)
		//	return;
		mWorkShopMgr.mQuereText = mInput.text;
		GetPageGrid();
	}

	void BtnQueryClearOnClick()
	{
		mInput.text = "";
		if (mWorkShopMgr.mQuereText.Length == 0)
			return;
		mWorkShopMgr.mQuereText = "";
		GetPageGrid();
	}

	
	void OnClickItemBtnReLoad(int index)
	{
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
			return;
		
		PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index]; 
		if (p_id.m_PublishedFileId != 0)
		{
			int i = index % mWorkShopGridCtrl.mMaxGridCount;
			if (i > mWorkShopGridCtrl.mMaxGridCount || i<0)
				return;
			
			mWorkShopMgr.GetItemDetail(p_id);
			mWorkShopGridCtrl.mUIItems[i].ActiveLoadingItem(true);
		}
	}
	
	// ----------- Vote----------------------------------------------------------------
	void OnClickItemBtnDing(int index)
	{
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
			return;
        if (this.m_DingList != null)
        {
            if (this.m_DingList.Contains(index))
            {
                return;
            }
            this.m_DingList.Add(index);
        }

		PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index]; 
		mWorkShopMgr.Vote(p_id,true);
	}
	
	void OnClickItemBtnCai(int index)
	{
		if (!mWorkShopMgr.mIndexMap.ContainsKey(index))
			return;
        if (this.m_CaiList != null)
        {
            if (this.m_CaiList.Contains(index))
            {
                 return;
            }
            this.m_CaiList.Add(index);
        }
		PublishedFileId_t p_id = mWorkShopMgr.mIndexMap[index]; 
		mWorkShopMgr.Vote(p_id,false);
	}
	
	void OnVoteCallBack(PublishedFileId_t p_id, bool bFor)
	{
		int findIndex = FindIndexInGrid(p_id);
		if(findIndex == -1)
			return;
		
		int i = findIndex % mWorkShopGridCtrl.mMaxGridCount;
		if (i > mWorkShopGridCtrl.mMaxGridCount || i<0)
			return;

		if (mWorkShopMgr.mItemsMap.ContainsKey(p_id))
		{
			SteamPreFileAndVoteDetail detail = mWorkShopMgr.mItemsMap[p_id];
			if (bFor)
				detail.m_VoteDetail.m_nVotesFor++;
			else
				detail.m_VoteDetail.m_nVotesAgainst++;
		
			if (mWorkShopGridCtrl.mUIItems[i] == null)
				return;

			mWorkShopGridCtrl.mUIItems[i].SetDingText(detail.m_VoteDetail.m_nVotesFor.ToString());
			mWorkShopGridCtrl.mUIItems[i].SetCaiText(detail.m_VoteDetail.m_nVotesAgainst.ToString());
		}
	}
	

	int FindIndexInGrid(PublishedFileId_t p_id)
	{
		int findIndex = -1;

		int indexStar = (mWorkShopGridCtrl.mPagIndex-1) * mWorkShopGridCtrl.mMaxGridCount;
		int indexEnd = mWorkShopGridCtrl.mPagIndex  * mWorkShopGridCtrl.mMaxGridCount;
        
		foreach(var kv in mWorkShopMgr.mIndexMap)
		{
			if (kv.Value == p_id)
			{
				if (kv.Key >= indexStar && kv.Key  < indexEnd)
				{
					findIndex = kv.Key ;
					break;
				}
			}
		}
		return findIndex;   
	}


	// ---------DownLoad-----------------------------------------------------------------
	void BtnDownloadOnClick()
	{
        //SteamWorkShop.Instance.GetRandIsos(1,5);
        //return;
		if (mSelectedDetail != null)
		{
			if (mWorkShopMgr.DownLoadFile(mSelectedDetail))
			{
				int findIndex = FindIndexInGrid(mSelectedDetail.m_nPublishedFileId);
				if(findIndex == -1)
					return;
				
				int i = findIndex % mWorkShopGridCtrl.mMaxGridCount;
				if (i > mWorkShopGridCtrl.mMaxGridCount || i<0)
					return;
				mWorkShopGridCtrl.mUIItems[i].ActiveUpDown(true);
				mWorkShopGridCtrl.mUIItems[i].UpdteUpDownInfo("0%");
			}
		}

	}
   
    void OnPop1SelectionChange(string strText)
	{
        EWorkshopEnumerationType type;
		if (strText == PELocalization.GetString(8000698))
		{
			type= EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote;
		}
        else if (strText == PELocalization.GetString(8000697))
		{
			type= EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent;
		}
		else
		{
			return;
		}
        if (type == mWorkShopMgr.mIdEnumType) return;
        mWorkShopMgr.mIdEnumType = type;
        GetPageGrid();
	}

	void OnPop2SelectionChange(string strText)
	{
        uint getIdDays = 0;
        if (strText == "All time")
        {
            getIdDays = 0;
        }
		if (strText == "Today" )
		{
            getIdDays = 1;
		}
		else if (strText == "This week" )
		{
            getIdDays = 7;
		}
		else if (strText == "This month" )
		{
			 getIdDays= 31;
		}
		else
		{
			return;
		}
        if (mWorkShopMgr.mGetIdDays == getIdDays) return;
        mWorkShopMgr.mGetIdDays = getIdDays;
	    GetPageGrid();
	}

    void UpdateDownloadBtnState()
    {
        if (null != mSelectedDetail)
        {
            this.m_DownloadBtn.isEnabled = !UIWorkShopCtrl.CheckDownloadExist(mSelectedDetail.m_rgchTitle + VCConfig.s_IsoFileExt);
            return;
        }
        this.m_DownloadBtn.isEnabled = true;
    }

    public void SetItemIsDownloadedByFileName(string fileName)
    {
        UIWorkShopGridItem item=this.mWorkShopGridCtrl.GetWorkShopItemByFileName(fileName);
        if (null != item)
            item.SetDownloaded(true);
    }

}

