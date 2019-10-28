using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using System;
using System.IO;
using System.Text.RegularExpressions;

public class WorkShopMgr
{
	public bool isActve = false;
	public delegate void _GetItemID(int ItemCount,int allItemCount);
	public event _GetItemID e_GetItemID = null;
	
	public delegate void _GetItemDetail(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail);
	public event _GetItemDetail e_GetItemDetail = null;
	
	public delegate void _VoteDetailCallBack(PublishedFileId_t p_id, bool bFor);
	public event _VoteDetailCallBack e_VoteDetailCallBack = null;

	public delegate void _DeleteFile(PublishedFileId_t p_id, bool bOk);
	public event _DeleteFile e_DeleteFile = null;

//	public delegate void _DownLoadFile(string filePath, bool bOk);
//	public event _DownLoadFile e_DownLoadFile = null;

	public EWorkshopEnumerationType mIdEnumType;
	
	public uint mGetIdStarIndex;
    public uint mGetIdCount { get { return UIWorkShopCtrl.GetCurRequestCount(); } }  //lz-2016.05.23 因为要适应分辨率，所以请求的格子数量在变，所有由UIWorkShopCtrl统一管理
	public uint mGetIdDays;
	public string mQuereText;
	public int mTatileCount;
	
	public List<string> mTagList = new List<string>();

	
	public Dictionary<int ,PublishedFileId_t> mIndexMap;
	public Dictionary<PublishedFileId_t ,SteamPreFileAndVoteDetail> mItemsMap;
	public Dictionary<PublishedFileId_t,int> mGetCountMap;
	public Dictionary<PublishedFileId_t,byte> mVoteMap;
	public Dictionary<PublishedFileId_t,int> mDownMap;


	bool mIsPrivate = false;
	
	
	public WorkShopMgr()
	{
		mIndexMap = new Dictionary<int,PublishedFileId_t>();
		mItemsMap = new Dictionary<PublishedFileId_t,SteamPreFileAndVoteDetail>(); 
		mGetCountMap  = new Dictionary<PublishedFileId_t,int>();
		mVoteMap = new Dictionary<PublishedFileId_t,byte>();
		mDownMap = new Dictionary<PublishedFileId_t,int>();
		
		mIdEnumType = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote;
		mGetIdStarIndex =0;
		mGetIdDays = 0;
		mTagList.Add("Creation");
		mQuereText = "";

		isActve = true;
	}
	
	public void ClearIsoMap()
	{
		mIndexMap.Clear();
		mItemsMap.Clear();
		mGetCountMap.Clear();
	}
	
	public void InitItemList(bool isPrevate) // IsPrevate 自己的
	{
		mIndexMap.Clear();
		mItemsMap.Clear();
		mGetCountMap.Clear();
		mGetIdStarIndex =0;
		mIsPrivate = isPrevate;
        this.GetNewItemList();
	}
	
	public void GetNewItemList()
	{
        //Debug.Log(string.Format("StartIndex:{0}  GetIdCount {1}", mGetIdStarIndex, mGetIdCount));
		if (mIsPrivate)
		{
			SteamProcessMgr.Instance.GetMyPreFileList(GetItemCallBack,mGetIdStarIndex,mTagList.ToArray(),mGetIdDays,mGetIdCount,mIdEnumType,mQuereText);
		}
		else
		{
			SteamProcessMgr.Instance.GetPreFileList(GetItemCallBack,mGetIdStarIndex,mTagList.ToArray(),mGetIdDays,mGetIdCount,mIdEnumType,mQuereText);
		}
	}

	void GetItemCallBack(List<PublishedFileId_t > mIdLsit,int totalResults,uint starIndex, bool bOK)
	{
		if (mIdLsit == null || bOK == false || isActve == false)
			return;
		
		for (int i=0;i<mIdLsit.Count;i++)
			mIndexMap[Convert.ToInt32(starIndex +i) ] = mIdLsit[i];

		
		if (e_GetItemID != null) 
			e_GetItemID(mIdLsit.Count,totalResults);
		
		for (int i=0;i<mIdLsit.Count;i++)
		{
			mGetCountMap[mIdLsit[i]] = 0;
			GetItemDetail(mIdLsit[i]);
		}
	}


	//public 
	
	public void GetItemDetail(PublishedFileId_t p_id)
	{
		mGetCountMap[p_id] ++;
		SteamProcessMgr.Instance.GetPreFileDetail(GetItemDetailCallBack, p_id);
	}

	void  GetItemDetailCallBack(PublishedFileId_t p_id, SteamPreFileAndVoteDetail detail,bool bOK)
	{
		if (isActve == false)
			return;

		if(bOK)
			mItemsMap[p_id] = detail;
		else 
		{
			if (!mGetCountMap.ContainsKey(p_id))
				mGetCountMap[p_id] = 0;

			if (mGetCountMap[p_id] <=3)
			{
				GetItemDetail(p_id);
				return;
			}
			mItemsMap[p_id] = null;
		}

		if (e_GetItemDetail != null)
			e_GetItemDetail(p_id,detail);
	}
	
	
	public void Vote( PublishedFileId_t p_id,bool bFor)
	{
		if (mVoteMap.ContainsKey(p_id))
		{
			if (bFor && mVoteMap[p_id] == 1)
				return;
			else if (!bFor && mVoteMap[p_id] == 2)
				return;
		}
		
		SteamProcessMgr.Instance.Vote(OnVoteCallBack,p_id,bFor );
	}
	
	void OnVoteCallBack( PublishedFileId_t p_id,bool bFor,bool bOK)
	{
		if (isActve == false)
			return;

        if (bOK)
        {
            if (bFor)
                mVoteMap[p_id] = 1;
            else
                mVoteMap[p_id] = 2;

            if (e_VoteDetailCallBack != null)
                e_VoteDetailCallBack(p_id, bFor);
        }
        else
        {
            mVoteMap[p_id] = 0;
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000491));
        }
	}
	
	
	
	public bool DownLoadFile(SteamPreFileAndVoteDetail detail)
	{
		if (mDownMap.ContainsKey(detail.m_nPublishedFileId))
		{
			if (mDownMap[detail.m_nPublishedFileId] >= 0) 
				return false;
		}
		mDownMap[detail.m_nPublishedFileId] = 0; // 已经下载过的
		
		SteamProcessMgr.Instance.GetPrimaryFile(OnDownLoadFileCallBack,detail.m_hFile,detail.m_nPublishedFileId);
		return true;
	}
	void OnDownLoadFileCallBack(byte[] fileData, PublishedFileId_t p_id,bool bOK,int index = -1,int dungeonId = -1)
	{
		if (isActve == false)
			return;

		//bool DonLoadSucess = false;
		if (bOK)
		{
			if (mItemsMap.ContainsKey(p_id))
			{
				if (mItemsMap[p_id] != null)
				{
                    SteamPreFileAndVoteDetail detail = mItemsMap[p_id];

                    VCIsoHeadData headData;

                    string creation = "Object";
                    if (VCIsoData.ExtractHeader(fileData, out headData) > 0)
                    {
                        creation = headData.Category.ToString();
                        creation = creation.Substring(2, creation.Length - 2);
                    }

                    string downLoadFilePath = VCConfig.s_IsoPath + string.Format("/Download/{0}/", creation);
                    string netCacheFilePath = VCConfig.s_CreationNetCachePath;

                    string downLoadFileName = detail.m_rgchTitle;
                    string netCacheFileName = CRC64.Compute(fileData).ToString();

					if(SaveToFile(fileData,downLoadFileName,downLoadFilePath,VCConfig.s_IsoFileExt))
					{
                        UIWorkShopCtrl.AddDownloadFileName(mItemsMap[p_id].m_rgchTitle + VCConfig.s_IsoFileExt, mIsPrivate);
						mDownMap[p_id] = 100;
					}

                    //lz-2016.05.30 保存一份到NetCache路径下，避免NetCache重复下载
                    SaveToFile(fileData, netCacheFileName, netCacheFilePath, VCConfig.s_CreationNetCacheFileExt);
				}
			}
		}
		else
		{
			mDownMap[p_id] = -1;
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000493));
		}

//		if (e_DownLoadFile != null)
//			e_DownLoadFile(filePath,DonLoadSucess);
	}

	public bool DeleteMyIsoFile(SteamPreFileAndVoteDetail detail)
	{
		if (null==detail|| !mItemsMap.ContainsKey(detail.m_nPublishedFileId))
			return false;
		SteamProcessMgr.Instance.DeleteFile(DeleteMyIsoFileCallBack,detail.m_pchFileName,detail.m_nPublishedFileId);
		return true;
	}

	public void DeleteMyIsoFileCallBack( string fileName,PublishedFileId_t p_id,bool bOK)
	{
		if ( isActve == false )
			return;

        if (e_DeleteFile != null && bOK)
        {
            e_DeleteFile(p_id, bOK);
        }
	}

    private bool SaveToFile(byte[] fileData, string fileName, string filePath, string fileExt)
    {
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        fileName=UIWorkShopCtrl.GetValidFileName(fileName);

        string TempPath = filePath;
        filePath += fileName + fileExt;

        int i = 0;
        while (File.Exists(filePath))
        {
            i++;
            filePath = TempPath + fileName + i.ToString() + fileExt;
        }

        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryWriter bw = new BinaryWriter(fileStream);
                bw.Write(fileData);
                bw.Close();
                fileStream.Close();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Save ISO to filepath:{0} Error:{1}", filePath, e.ToString()));
            return false;
        }
    }
}




