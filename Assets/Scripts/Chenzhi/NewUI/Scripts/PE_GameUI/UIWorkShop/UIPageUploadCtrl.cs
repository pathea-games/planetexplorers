using UnityEngine;
using System.Collections.Generic;
using Steamworks;
using System;
using WhiteCat;

public class UIPageUploadCtrl : MonoBehaviour 
{
	public GameObject mLeftListItem;
	public UIGrid mleftListGrid;
	
	public UIPageGridCtrl mUploadGridCtrl;
	
	public VCEUIStatisticsPanel mRinghtPanelCtrl;
	public GameObject mVCERightPanel;
	public UILabel mLbInfoMsg;
    public UIInput mInput;
	
	private List<UIWorkShopListItem> mLeftList = null; 
	private int mLeftListSelectedIndex = -1;

	private SteamPreFileAndVoteDetail mSelectedDetail;
    //private int m_index_0;
	public WorkShopMgr mMyWorkShopMgr;
    [SerializeField]
    private N_ImageButton m_DeleteBtn;
     [SerializeField]
    private N_ImageButton m_DownloadBtn;
    [SerializeField]
    private UIPopupList m_PopupList0;

    void Awake()
	{
		InitLeftList();
		mUploadGridCtrl.mUpdateGrid += UpdateUpLoadGrid;
		mUploadGridCtrl.ClickUpload += OnClickWorkShopItem;
		mUploadGridCtrl.ClickUpLoadBtnReLoad += OnClickItemBtnReLoad;
	
		mMyWorkShopMgr = new WorkShopMgr();
		mMyWorkShopMgr.e_GetItemID += OnGetItemIDList;
		mMyWorkShopMgr.e_GetItemDetail += GetGridItemDetail;
        mMyWorkShopMgr.e_DeleteFile += OnDeleteFile;
        this.m_DeleteBtn.isEnabled = false;
        m_PopupList0.items.Clear();
        m_PopupList0.items.Add(PELocalization.GetString(8000698));
        m_PopupList0.items.Add(PELocalization.GetString(8000697));
        m_PopupList0.selection = m_PopupList0.items[0];
    }

	void InitLeftList()
	{
		mLeftList = new List<UIWorkShopListItem>();
		for (int i = 0; i < PEVCConfig.isoNames.Count; i++)
		{
			GameObject go = Instantiate(mLeftListItem) as GameObject;
			go.transform.parent = mleftListGrid.gameObject.transform;
			go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;

			UIWorkShopListItem item = go.GetComponent<UIWorkShopListItem>();
			item.SetText(PEVCConfig.isoNames[i].GetNameByID());

			item.SetIndex(i);
			item.SetSelected(false);
			item.ItemClick += OnCheckLeftList;
			mLeftList.Add(item);
		}
		mleftListGrid.repositionNow = true;
	}

	// Use this for initialization
	void Start () 
	{
        mUploadGridCtrl.mPagIndex = 0;
        mUploadGridCtrl.mMaxPagIndex = 0;
        mMyWorkShopMgr.InitItemList(true);
		mLeftList[0].SetSelected(true);
		mLeftListSelectedIndex = 0;
	}


	// Update is called once per frame
	int tempFrame = 0;
	void Update () 
	{
		tempFrame++;
		if (tempFrame % 10==0)
		{
			UpdateDownLoadState();
			tempFrame=0; 
		}

	}

    void GetPageGrid()
    {
        mMyWorkShopMgr.GetNewItemList();
    }


	void OnCheckLeftList(int index)
	{
        if (index<0||index >= mLeftList.Count)
            return;
        //lz-2016.08.09 选择类型的时候清一下Item列表
        mMyWorkShopMgr.ClearIsoMap();

		if (mLeftListSelectedIndex != -1)
			mLeftList[mLeftListSelectedIndex].SetSelected(false);
		mLeftList[index].SetSelected(true);
		mLeftListSelectedIndex = index;
		
		mMyWorkShopMgr.mGetIdStarIndex =0;
        mUploadGridCtrl.mPagIndex = 0;
        mUploadGridCtrl.mMaxPagIndex = 0;
		
		if (mMyWorkShopMgr.mTagList.Count == 1)
		{
			mMyWorkShopMgr.mTagList.Add(PEVCConfig.isoNames[index].Tag);
		}
		else
		{
			mMyWorkShopMgr.mTagList[1] = PEVCConfig.isoNames[index].Tag;
		}
        this.GetPageGrid();
	}


	void UpdateDownLoadState()
	{
		int starIndex =Convert.ToInt32(mMyWorkShopMgr.mGetIdStarIndex);
		for (int i=0; i<mUploadGridCtrl.mUIItems.Count; i++)
		{
			if (mUploadGridCtrl.mUIItems[i].gameObject.activeSelf == false)
				continue;
			
			if(mMyWorkShopMgr.mIndexMap.ContainsKey(starIndex+i))
			{
				PublishedFileId_t p_id = mMyWorkShopMgr.mIndexMap[starIndex+i];
				if (mMyWorkShopMgr.mItemsMap.ContainsKey(p_id))
				{
					SteamPreFileAndVoteDetail detail = mMyWorkShopMgr.mItemsMap[p_id];
					if (detail != null && mUploadGridCtrl.mUIItems[i].gameObject.activeSelf)
					{
						int loadByte,totleByte;
						if (SteamProcessMgr.Instance.GetDownProgress(detail.m_hFile,out loadByte,out totleByte) )
						{
							int value = 0;
							if(totleByte != 0)
								value = Convert.ToInt32((loadByte* 100)/totleByte) ;
							if (value >0)
							{
								mUploadGridCtrl.mUIItems[i].UpdteUpDownInfo(value.ToString() + "%");
								mMyWorkShopMgr.mDownMap[p_id] = value;
							}
						}
						else
						{
                            if (mMyWorkShopMgr.mDownMap.ContainsKey(p_id))
                            {
                                if (mMyWorkShopMgr.mDownMap[p_id] <= 0)
                                    mUploadGridCtrl.mUIItems[i].UpdteUpDownInfo(PELocalization.GetString(8000911)); // "Failed!"
                                else if (mMyWorkShopMgr.mDownMap[p_id] >= 100)
                                    mUploadGridCtrl.mUIItems[i].SetDownloaded(true);
                                else
                                    mUploadGridCtrl.mUIItems[i].UpdteUpDownInfo(mMyWorkShopMgr.mDownMap[p_id].ToString() + "%");
                                this.UpdateDownloadBtnState();
                            }
						}
					}
				}
			}
			
		}
	}

	// event 回调
	void UpdateUpLoadGrid(int index_0)
	{
		if (index_0 < 0)
			index_0 = 0;
        //this.m_index_0 = index_0;
		mSelectedDetail = null;
		mVCERightPanel.SetActive(false);
		mMyWorkShopMgr.mGetIdStarIndex = Convert.ToUInt32(index_0);
		
		if (!mMyWorkShopMgr.mIndexMap.ContainsKey(index_0))
		{
			for (int i=0;i< mUploadGridCtrl.mUIItems.Count;i++)
			{
				mUploadGridCtrl.mUIItems[i].gameObject.SetActive(false);
			}
            this.GetPageGrid();
		}
		else 
		{
			for (int i=0;i< mUploadGridCtrl.mUIItems.Count;i++)
			{
				
				if ( mMyWorkShopMgr.mIndexMap.ContainsKey(index_0+ i) )
				{
					PublishedFileId_t p_id = mMyWorkShopMgr.mIndexMap[index_0+ i];
                    mUploadGridCtrl.mUIItems[i].gameObject.SetActive(true);
					if ( mMyWorkShopMgr.mItemsMap.ContainsKey(p_id) )
					{
						SteamPreFileAndVoteDetail del = mMyWorkShopMgr.mItemsMap[p_id] ;
						if (mMyWorkShopMgr.mDownMap.ContainsKey(p_id))
						{
							if (mMyWorkShopMgr.mDownMap[p_id] > 0 )
								mUploadGridCtrl.mUIItems[i].ActiveUpDown(true);
							else 
								mUploadGridCtrl.mUIItems[i].ActiveUpDown(false);
						}
						else
						{
							mUploadGridCtrl.mUIItems[i].ActiveUpDown(false);
						}
                        SetWorkShopGridItemInfo(del, mUploadGridCtrl.mUIItems[i]);
					}
					else
					{
						SetWorkShopGridItemInfo(null,mUploadGridCtrl.mUIItems[i]);
						continue;
					}
				}
				else 
				{
					mUploadGridCtrl.mUIItems[i].gameObject.SetActive(false);
				}
				mUploadGridCtrl.mUIItems[i].SetSelected(false);
			}
		}
	}


	void SetWorkShopGridItemInfo(SteamPreFileAndVoteDetail del ,UIWorkShopGridItem item)
	{
		if (item == null)
			return;
        item.gameObject.SetActive(true);

		if (del != null)
		{
			item.SetIsoName(del.m_rgchTitle);
            item.SetDownloaded(UIWorkShopCtrl.CheckDownloadExist(del.m_rgchTitle + VCConfig.s_IsoFileExt));
            if (del.m_aPreFileData != null)
            {
                VCIsoHeadData headData = new VCIsoHeadData();
                headData.SteamPreview = del.m_aPreFileData;
                item.SetAuthor(PELocalization.GetString(8000696));
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
			item.SetIco(null);
			item.SetIsoName(PELocalization.GetString(8000695));
			item.SetAuthor(PELocalization.GetString(8000695));
		}
		
		item.ActiveLoadingItem(false);
	}


	void OnClickWorkShopItem(int index)
	{
		mSelectedDetail = null;
		
		if (!mMyWorkShopMgr.mIndexMap.ContainsKey(index))
		{
			mVCERightPanel.SetActive(false);
			return;
		}

		PublishedFileId_t p_id = mMyWorkShopMgr.mIndexMap[index]; 
		if (p_id.m_PublishedFileId == 0)
		{
			mVCERightPanel.SetActive(false);
			return;
		}
		
		SteamPreFileAndVoteDetail del;
		if (mMyWorkShopMgr.mItemsMap.ContainsKey(p_id))
			del= mMyWorkShopMgr.mItemsMap[p_id];
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
        this.UpdateDeleteBtnState();
        this.UpdateDownloadBtnState();
	}

	void OnClickItemBtnReLoad(int index)
	{
		if (!mMyWorkShopMgr.mIndexMap.ContainsKey(index))
			return;
		
		PublishedFileId_t p_id = mMyWorkShopMgr.mIndexMap[index]; 
		if (p_id.m_PublishedFileId != 0)
		{
			int i = index % mUploadGridCtrl.mMaxGridCount;
			if (i > mUploadGridCtrl.mMaxGridCount || i<0)
				return;
			
			mMyWorkShopMgr.GetItemDetail(p_id);
			mUploadGridCtrl.mUIItems[i].ActiveLoadingItem(true);
		}
	}


	void OnGetItemIDList(int count,int allCount)
	{
		mUploadGridCtrl.ReSetGrid(allCount);
        mUploadGridCtrl._UpdatePagText();
		
		for (int i=0;i< mUploadGridCtrl.mUIItems.Count;i++)
		{
			if ( i < count)
			{
				mUploadGridCtrl.mUIItems[i].ActiveLoadingItem(true);
				mUploadGridCtrl.mUIItems[i].InitItem(WorkGridItemType.mUpLoad, PELocalization.GetString(8000699));
				mUploadGridCtrl.mUIItems[i].gameObject.SetActive(true);
			}
			else 
			{
				mUploadGridCtrl.mUIItems[i].gameObject.SetActive(false);
			}
			mUploadGridCtrl.mUIItems[i].SetSelected(false);
		}
		mUploadGridCtrl.mGrid.repositionNow = true;
	}

    void OnDeleteFile(PublishedFileId_t p_id, bool bOk)
    {
        int findIndex = FindIndexInGrid(p_id);
        if (findIndex == -1)
            return;
        int i = findIndex % mUploadGridCtrl.mMaxGridCount;
        if (i > mUploadGridCtrl.mMaxGridCount || i < 0)
            return;
        this.SetWorkShopGridItemInfo(null,mUploadGridCtrl.mUIItems[i]);
        MessageBox_N.ShowOkBox(PELocalization.GetString(8000030));
    }

	void GetGridItemDetail(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail)
	{
		int findIndex = FindIndexInGrid(p_id);
		if(findIndex == -1)
			return;
		
		int i = findIndex % mUploadGridCtrl.mMaxGridCount;
		if (i > mUploadGridCtrl.mMaxGridCount || i<0)
			return;
		SetWorkShopGridItemInfo(detail,mUploadGridCtrl.mUIItems[i]);
	}

	int FindIndexInGrid(PublishedFileId_t p_id)
	{
		int findIndex = -1;
		
		int indexStar = (mUploadGridCtrl.mPagIndex-1) * mUploadGridCtrl.mMaxGridCount;
		int indexEnd = mUploadGridCtrl.mPagIndex  * mUploadGridCtrl.mMaxGridCount;
		
		foreach(var kv in mMyWorkShopMgr.mIndexMap)
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


	void BtnDownloadOnClick()
	{
		if (mSelectedDetail != null)
		{
			if (mMyWorkShopMgr.DownLoadFile(mSelectedDetail))
			{
				int findIndex = FindIndexInGrid(mSelectedDetail.m_nPublishedFileId);
				if(findIndex == -1)
					return;
				
				int i = findIndex % mUploadGridCtrl.mMaxGridCount;
				if (i > mUploadGridCtrl.mMaxGridCount || i<0)
					return;
				mUploadGridCtrl.mUIItems[i].ActiveUpDown(true);
				mUploadGridCtrl.mUIItems[i].UpdteUpDownInfo("0%");
			}

		}
//		UGCHandle_t handle =new UGCHandle_t();
//		handle.m_UGCHandle = 3318339173234368542;
//		PublishedFileId_t id = new PublishedFileId_t();
//		id.m_PublishedFileId = 0;
//		SteamProcessMgr.Instance.GetPrimaryFile(null,handle,id);
		
	}

    void BtnQueryOnClick()
    {
        //if (mWorkShopMgr.mQuereText == mInput.text)
        //	return;
        mMyWorkShopMgr.mQuereText = mInput.text;
        GetPageGrid();
    }

    void BtnQueryClearOnClick()
    {
        mInput.text = "";
        if (mMyWorkShopMgr.mQuereText.Length == 0)
            return;
        mMyWorkShopMgr.mQuereText = "";
        GetPageGrid();
    }


    void OnPop1SelectionChange(string strText)
    {
        EWorkshopEnumerationType type;
        if (strText == PELocalization.GetString(8000698))
        {
            type = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote;
        }
        else if (strText == PELocalization.GetString(8000697))
        {
            type = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent;
        }
        else
        {
            return;
        }
        if (type == mMyWorkShopMgr.mIdEnumType) return;
        mMyWorkShopMgr.mIdEnumType = type;
        GetPageGrid();
    }

    void OnPop2SelectionChange(string strText)
    {
        uint getIdDays = 0;
        if (strText == "All time")
        {
            getIdDays = 0;
        }
        if (strText == "Today")
        {
            getIdDays = 1;
        }
        else if (strText == "This week")
        {
            getIdDays = 7;
        }
        else if (strText == "This month")
        {
            getIdDays = 31;
        }
        else
        {
            return;
        }
        if (mMyWorkShopMgr.mGetIdDays == getIdDays) return;
        mMyWorkShopMgr.mGetIdDays = getIdDays;
        GetPageGrid();
    }


	void BtnDeleteOnClick()
	{
        if (null != mSelectedDetail)
        {
            float curVersion =0f;
            float newVersion = float.Parse(SteamWorkShop.NewVersionTag);
            if (float.TryParse(mSelectedDetail.m_rgchTags, out curVersion))
            {
                if (curVersion >= newVersion)
                {
                    MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000492), newVersion));
                    return;
                }
            }
            MessageBox_N.ShowYNBox(PELocalization.GetString(8000015), () => mMyWorkShopMgr.DeleteMyIsoFile(mSelectedDetail));
        }
	}

    //log:lz-2016.05.17
    void UpdateDeleteBtnState()
    {
        if (null != mSelectedDetail)
        {
            float curVersion = 0f;
            float newVersion = float.Parse(SteamWorkShop.NewVersionTag);
            if (float.TryParse(mSelectedDetail.m_rgchTags, out curVersion))
            {
                if (curVersion >= newVersion)
                {
                    this.m_DeleteBtn.isEnabled = false;
                    return;
                }
            }
        }
        this.m_DeleteBtn.isEnabled = true;
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
        UIWorkShopGridItem item = this.mUploadGridCtrl.GetWorkShopItemByFileName(fileName);
        if (null != item)
            item.SetDownloaded(true);
    }

}








