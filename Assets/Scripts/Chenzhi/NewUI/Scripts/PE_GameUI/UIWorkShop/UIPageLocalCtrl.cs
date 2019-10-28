using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class UIPageLocalCtrl : MonoBehaviour 
{
	public UIInput mQueryInput;
	public UIPageGridCtrl mLocalGridCtrl;
	public UILabel mIcoPath;
	public BoxCollider mBxBtnGoBack;
	public VCEUIStatisticsPanel mRinghtPanelCtrl;
	public GameObject mVCERightPanel;
	public UILabel mLbInfoMsg;
	public List<VCEAssetMgr.IsoFileInfo> mLocalIsoInfo = null;

	private string mLocalIsoPath;
	private string mDefoutIcoPath;
    private List<ulong> m_UploadingIsoHashList;       //log:lz-2016.05.16 上传中的Iso,避免重复上传
    private bool m_ImportIsoing;        //log:lz-2016.05.16 避免快速点击，导入相同的Iso报错

	public int mSelectedIsoIndex = -1;

	public Dictionary<int,string> mUpLoadMap = null;
	public Dictionary<string,int> mUpLoadStateMap = null; // 0 100
	int mUpLoadIndex =-1;
	int starIndex = 0;

	void Awake()
	{
		mLocalGridCtrl.mUpdateGrid += UpdateLocalGrid;
		mLocalGridCtrl.DoubleClickLocalFloder += OnDoubleClickFolder;
		mLocalGridCtrl.ClickLocalIso += OnClickIso;
		mLocalGridCtrl.ClickLocalFloder += OnClickFolder;
		mDefoutIcoPath = VCConfig.s_IsoPath.Replace('\\','/');

		mUpLoadMap = new Dictionary<int, string >(); 
		mUpLoadStateMap = new Dictionary<string,int>();
        this.m_UploadingIsoHashList = new List<ulong>();
        this.m_ImportIsoing = false;
	}

	// Use this for initialization
	void Start () 
	{
		GetLocalItem(mDefoutIcoPath);
	}


	// Update is called once per frame
	void Update () 
	{
        
	}




	void GetLocalItem(string _isoPath)
	{
		mLocalIsoPath = _isoPath;
		mLocalIsoPath = mLocalIsoPath.Replace('\\','/');
		mIcoPath.text = mLocalIsoPath.Replace(mDefoutIcoPath,"[ISO]/");

		if (mLocalIsoPath.Length <= mDefoutIcoPath.Length )
			mBxBtnGoBack.enabled = false;
		else 
			mBxBtnGoBack.enabled = true;


		if(mLocalIsoInfo != null)
		{
			mLocalIsoInfo.Clear();
			mLocalIsoInfo = null;
		}
		string _KeyWord = mQueryInput.text.Trim();


		mLocalIsoInfo = VCEAssetMgr.SearchIso(mLocalIsoPath,_KeyWord);

		mLocalGridCtrl.ReSetGrid(mLocalIsoInfo.Count);
		mLocalGridCtrl._UpdatePagText();
		mSelectedIsoIndex = -1;
		mVCERightPanel.SetActive(false);
	}

	// event 回调
	void UpdateLocalGrid(int index_0)
	{
		starIndex = index_0;
		for (int i=0;i< mLocalGridCtrl.mUIItems.Count;i++)
		{
			if ( (index_0 +i) < mLocalIsoInfo.Count)
				{
					SetLocalGridItemInfo(mLocalIsoInfo[index_0+ i],mLocalGridCtrl.mUIItems[i]);
					mLocalGridCtrl.mUIItems[i].gameObject.SetActive(true);
				}
				else 
				{
					mLocalGridCtrl.mUIItems[i].gameObject.SetActive(false);
				}
			mLocalGridCtrl.mUIItems[i].SetSelected(false);
		}

		mLocalGridCtrl.mGrid.repositionNow = true;
		UpdateDownLoadInfo();
	}


	void SetLocalGridItemInfo(VCEAssetMgr.IsoFileInfo _isoFileInfo,UIWorkShopGridItem item)
	{
		string isoName = _isoFileInfo.m_Name;

		if (_isoFileInfo.m_IsFolder)
		{
			item.InitItem(WorkGridItemType.mLocalFloder,isoName);
			item.SetIco(null);
		}
		else 
		{
			VCIsoHeadData headData;
			VCIsoData.ExtractHeader(_isoFileInfo.m_Path,out headData);
			
			item.InitItem(WorkGridItemType.mLocalIcon,isoName);

			Texture2D texture = new Texture2D(256,256);
			texture.LoadImage(headData.IconTex);

			item.SetIco(texture);
			item.SetAuthor(headData.Author);
		}
	}

	void OnDoubleClickFolder(int index)
	{
		if (index<0 || index > mLocalIsoInfo.Count)
		{
			Debug.LogError("Update Work shop local folder error!");
			return;
		}
		GetLocalItem(mLocalIsoInfo[index].m_Path + "/");
	}

	void OnClickFolder(int index)
	{
		mVCERightPanel.SetActive(false);
		mSelectedIsoIndex = -1;
	}

	void OnClickIso(int index)
	{
		if (index < 0 && index >= mLocalIsoInfo.Count )
			return;

		VCIsoHeadData headData;
		if (VCIsoData.ExtractHeader(mLocalIsoInfo[index].m_Path,out headData) > 0)
		{
			mVCERightPanel.SetActive(true);
			mRinghtPanelCtrl.m_NonEditorIcon = headData.IconTex ;
			mRinghtPanelCtrl.m_NonEditorISODesc = headData.Desc;
			mRinghtPanelCtrl.m_NonEditorISOName = headData.Name;
			mRinghtPanelCtrl.m_NonEditorRemark = headData.Remarks;
            string version = ((headData.Version >> 24) & 0xff) + "." + ((headData.Version >> 16) & 0xff); //Log: lz-2016.05.13 Add version field
            mRinghtPanelCtrl.m_NonEditorISOVersion = version;
			mRinghtPanelCtrl.SetIsoIcon();
			mRinghtPanelCtrl.OnCreationInfoRefresh();
			mSelectedIsoIndex = index;
		}
		else
		{
			mVCERightPanel.SetActive(false);
			mSelectedIsoIndex = -1;
		}
	}


	void BtnGoBackOnClick()
	{
		if ( mLocalIsoPath.Length > 0 )
			mLocalIsoPath = new DirectoryInfo(mLocalIsoPath).Parent.FullName + "/";
		GetLocalItem(mLocalIsoPath);
	}


	void BtnUploadOnClick()
	{
        if (this.m_ImportIsoing)
            return;
        this.m_ImportIsoing = true;
        if (mSelectedIsoIndex == -1 || mSelectedIsoIndex >= mLocalIsoInfo.Count)
		{
			SetInfoMsg(UIMsgBoxInfo.mCZ_WorkShopUpNeedSeletedIso.GetString());
            this.m_ImportIsoing = false;
			return;
		}
        int i = mSelectedIsoIndex % mLocalGridCtrl.mMaxGridCount;
        if (i < 0 || i >= mLocalGridCtrl.mMaxGridCount)
        {
            this.m_ImportIsoing = false;
            return;
        }
		VCEAssetMgr.IsoFileInfo fileInfo =  mLocalIsoInfo[mSelectedIsoIndex];
		VCIsoData iso = new VCIsoData ();
		try
		{
			string fullpath = fileInfo.m_Path;
			using ( FileStream fs = new FileStream (fullpath, FileMode.Open, FileAccess.Read) )
			{
				byte[] iso_buffer = new byte [(int)(fs.Length)];
				fs.Read(iso_buffer, 0, (int)(fs.Length));
				fs.Close();
				iso.Import(iso_buffer, new VCIsoOption(true));

                //VCGameMediator.SendIsoDataToServer();
                if (iso.m_HeadInfo.Version < 0x02030001)
                {
                    MessageBox_N.ShowOkBox(PELocalization.GetString(8000487));
                    this.m_ImportIsoing = false;
                    return;
                }

				string isoPath = fullpath.Replace('\\','/');
				isoPath =  isoPath.Replace(mDefoutIcoPath,"[ISO]/");
				mUpLoadIndex++;

                //lz-2016.05.17 存储上传中Iso 的 hash
                ulong hash = CRC64.Compute(iso_buffer);
                if (null != this.m_UploadingIsoHashList && this.m_UploadingIsoHashList.Contains(hash))
                {
                    MessageBox_N.ShowOkBox(PELocalization.GetString(8000488));
                    this.m_ImportIsoing = false;
                    return;
                }
                this.m_UploadingIsoHashList.Add(hash);

				mUpLoadMap[mUpLoadIndex] = isoPath;
				mUpLoadStateMap[ mUpLoadMap[mUpLoadIndex] ] = 0;
                mLocalGridCtrl.mUIItems[i].ActiveUpDown(true);
                mLocalGridCtrl.mUIItems[i].UpdteUpDownInfo(PELocalization.GetString(8000908)); //"Uploading"

                SteamWorkShop.SendFile(UpLoadFileCallBack,iso.m_HeadInfo.Name,iso.m_HeadInfo.SteamDesc,
				                       iso.m_HeadInfo.SteamPreview,iso_buffer,SteamWorkShop.AddNewVersionTag( iso.m_HeadInfo.ScenePaths()),false,mUpLoadIndex);
			}
		}
		catch (Exception e)
		{
            mLocalGridCtrl.mUIItems[i].ActiveUpDown(false);
            mLocalGridCtrl.mUIItems[i].UpdteUpDownInfo("");
			Debug.Log(" WorkShop Loading ISO Error : " + e.ToString());
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000489));
		}
        this.m_ImportIsoing = false;
	}	 


	void UpLoadFileCallBack(int _upLoadindex, bool bOK,ulong hash)
	{
		string path = "";
		if ( mUpLoadMap.ContainsKey(_upLoadindex) )
		{
			path = mUpLoadMap[_upLoadindex];
			path= path.Replace('\\','/');
			path = path.Replace(mDefoutIcoPath,"[ISO]/");
		}

		string text = "";
		if (path.Length>0)
            text = "'" + path + "' " + UIMsgBoxInfo.mCZ_WorkShopUpLoadIsoFailed.GetString();
        if (bOK)
        {
            mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = 100;
        }
        else
        {
            mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = -1;
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000490));
        }

        //log:lz-2016.05.17 移除上传完成或者失败的hash
        if (null != this.m_UploadingIsoHashList && this.m_UploadingIsoHashList.Contains(hash))
        {
            this.m_UploadingIsoHashList.Remove(hash);
        }

		UpdateDownLoadInfo();
		SetInfoMsg(text);
	}


	void UpdateDownLoadInfo()
	{
		int count = mLocalIsoInfo.Count;

		for (int i=starIndex, j=0;i< count && j<mLocalGridCtrl.mMaxGridCount;i++,j++)
		{
			string mPath = mLocalIsoInfo[i].m_Path;
			mPath= mPath.Replace('\\','/');
			mPath = mPath.Replace(mDefoutIcoPath,"[ISO]/");

			if (!mUpLoadStateMap.ContainsKey(mPath))
			{
				mLocalGridCtrl.mUIItems[j].ActiveUpDown(false);
				mLocalGridCtrl.mUIItems[j].UpdteUpDownInfo("");
			}
			else
			{				
				if (mUpLoadStateMap[mPath] == 100 )
				{
					mLocalGridCtrl.mUIItems[j].ActiveUpDown(true);
					mLocalGridCtrl.mUIItems[j].UpdteUpDownInfo(PELocalization.GetString(8000909)); //"Publishing!"
                }
                else if (mUpLoadStateMap[mPath] == 101)
                {
                    mLocalGridCtrl.mUIItems[j].ActiveUpDown(true);
                    mLocalGridCtrl.mUIItems[j].UpdteUpDownInfo(PELocalization.GetString(8000910)); //"Succeeded!"
                }
                else if (mUpLoadStateMap[mPath] == -1)
                {
                    mLocalGridCtrl.mUIItems[j].ActiveUpDown(true);
                    mLocalGridCtrl.mUIItems[j].UpdteUpDownInfo(PELocalization.GetString(8000911)); // "Failed!"
                }
                else
                {
                    mLocalGridCtrl.mUIItems[j].ActiveUpDown(true);
                    mLocalGridCtrl.mUIItems[j].UpdteUpDownInfo(PELocalization.GetString(8000908)); //"Uploading"
                }
			}
		}
	}
	
	void SetInfoMsg(string msg)
	{
		mLbInfoMsg.text = msg;
	}

	void BtnQueryOnClick()
	{
		GetLocalItem(mLocalIsoPath);
	}
	
	void BtnQueryClearOnClick()
	{
		mQueryInput.text = "";
		GetLocalItem(mLocalIsoPath);
    }

    #region public methods
    public void PublishFinishCellBack(int _upLoadindex, ulong publishID, ulong hash)
    {
        string path = "";
        if (mUpLoadMap.ContainsKey(_upLoadindex))
        {
            path = mUpLoadMap[_upLoadindex];
            path = path.Replace('\\', '/');
            path = path.Replace(mDefoutIcoPath, "[ISO]/");
        }

        string text = "";
        if (path.Length > 0 && publishID != 0)
        {
            mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = 101;
            text = "'" + path + "' " + UIMsgBoxInfo.mCZ_WorkShopUpLoadIso.GetString();
        }
        else
        {
            mUpLoadStateMap[mUpLoadMap[_upLoadindex]] = -1;
            text = "'" + path + "' " + UIMsgBoxInfo.mCZ_WorkShopUpLoadIsoFailed.GetString();
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000491));
        }
        if (null != m_UploadingIsoHashList && m_UploadingIsoHashList.Contains(hash))
        {
            m_UploadingIsoHashList.Remove(hash);
        }
        UpdateDownLoadInfo();
        SetInfoMsg(text);
    }

    #endregion

}
