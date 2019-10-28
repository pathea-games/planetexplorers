using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
public class SteamVoteDetail
{
	public int m_nVotesFor;
	public int m_nVotesAgainst;
	public int m_nReports;
	public float m_fScore;
}
public class SteamPreFileAndVoteDetail
{
	public PublishedFileId_t m_nPublishedFileId;
	public UGCHandle_t m_hFile;			// The handle of the primary file
	public UGCHandle_t m_hPreviewFile;		// The handle of the preview file
	public string m_pchFileName;		// The name of the primary file
	public byte [] m_aPreFileData;
	public string m_sUploader;
	public string m_rgchTitle;		// title of document
	public string m_rgchDescription;	// description of document
	public SteamVoteDetail m_VoteDetail;
    public string m_rgchTags;
    public SteamPreFileAndVoteDetail()
	{
		m_VoteDetail = new SteamVoteDetail ();
	}
}

public class ISteamGetFile
{
	//get preview call back
	public delegate void SteamGetPreFileCallBackEventHandler(PublishedFileId_t publishedFileId, SteamPreFileAndVoteDetail detail,bool bOK);
	
	//download file callback
	public delegate void GetPrimaryFileResultEventHandler(byte [] fileData,PublishedFileId_t publishedFileId,bool bOK,int index ,int dungeonId = -1);
	
	//vote call back
	public delegate void VoteResultEventHandler( PublishedFileId_t publishID,bool bFor,bool bOK);

	//deletefile callback
	public delegate void DeleteFileResultEventHandler( string fileName,PublishedFileId_t publishID,bool bOK );

	//get preview list call back
	public delegate void GetPreListCallBackEventHandler( List<PublishedFileId_t> publishIDList,int totalResults,uint startIndex, bool bOK);

    //get Random iso idlist call back
    public delegate void GetRandomIsoListCallBackEventHandler(List<ulong> fileIDsList,List<ulong> publishIds,int dungeonId, bool bOK);




    //published file detail callback
    public delegate void PublishedFileDetailsResultEventHandler( PublishedFileId_t fileIDList, UGCHandle_t	 fileHandle  ,UGCHandle_t	 preFileHandle,bool bOK);
			
	//download prefile callback
	public delegate void DownloadPreUGCResultEventHandler(byte [] fileByte,string fileName,bool bOK);
		
	//vote detail call back
	public delegate void SteamPreFileAndVoteDetailEventHandler( SteamPreFileAndVoteDetail detail,bool bOK);

	//vote detail call back
	public delegate void SteamVoteDetailEventHandler(PublishedFileId_t publishedFileId, SteamVoteDetail detail,bool bOK);
	public static List<ISteamGetFile> ProcessList = new List<ISteamGetFile>();

}
public class SteamProcessMgr : ISteamGetFile
{

	SteamProcessMgr(){ _instance = this;}
	private static SteamProcessMgr _instance = new SteamProcessMgr();
	internal static SteamProcessMgr Instance { get { return _instance; } }
//
//	List<SteamGetMyPreListProcess> SteamGetMyPreListProcessList = new List<SteamGetMyPreListProcess>();
//	List<SteamSearchMyProcess> SteamSearchMyProcessList = new List<SteamSearchMyProcess>();
//	List<SteamGetPreFileListProcess> SteamGetPreFileListProcessList = new List<SteamGetPreFileListProcess>();
//	List<SteamSearchProcess> SteamSearchProcessList = new List<SteamSearchProcess>();
//	List<SteamGetPreFileDetailProcess> SteamGetPreFileDetailProcessList = new List<SteamGetPreFileDetailProcess>();
//	List<SteamGetPrimaryFileProcess> SteamGetPrimaryFileProcessList = new List<SteamGetPrimaryFileProcess>();
//	List<SteamVoteProcess> SteamVoteProcessList = new List<SteamVoteProcess>();
//	List<SteamDeleteProcess> SteamDeleteProcessList = new List<SteamDeleteProcess>();
//	List<SteamRefreshVoteDetailProcess> SteamRefreshVoteDetailProcessList = new List<SteamRefreshVoteDetailProcess>();




	void Awake()
	{
#if SteamVersion
		_instance = this;
#endif
	}


	public void GetMyPreFileList(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex,string [] tags,uint days = 0,uint count = 9,EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote,string searchText = "")
	{
		if(searchText.CompareTo("") == 0)
		{
			SteamGetMyPreListProcess item = new  SteamGetMyPreListProcess (callBackGetPreListResult,startIndex,tags,count);
			ProcessList.Add(item);
			
		}
		else
		{

			SteamSearchMyProcess item = new SteamSearchMyProcess(callBackGetPreListResult,startIndex,tags,count,searchText);
			ProcessList.Add(item);
		}
	}
	public void GetPreFileList(GetPreListCallBackEventHandler callBackGetPreList, uint startIndex,string [] tags,uint days = 0,uint count = 9,EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote,string searchText = "")
	{
		if(searchText.CompareTo("") == 0)
		{
			SteamGetPreFileListProcess item = new SteamGetPreFileListProcess (callBackGetPreList,startIndex,tags,days,count,orderBy);
			ProcessList.Add(item);
		}
		else
		{
			SteamSearchProcess item = new SteamSearchProcess(callBackGetPreList,startIndex,tags,count,searchText);
			ProcessList.Add(item);
		}
	}
	public void GetPreFileDetail(SteamGetPreFileCallBackEventHandler callBackGetPreFileResult,PublishedFileId_t publishID)
	{
		SteamGetPreFileDetailProcess item = new SteamGetPreFileDetailProcess (callBackGetPreFileResult,publishID);
		ProcessList.Add (item);
	}
	public void GetPrimaryFile(GetPrimaryFileResultEventHandler callBackDownloadFileUGCResult, UGCHandle_t file,PublishedFileId_t publishID,int index = -1,int dungeonId = -1 )
	{
		SteamGetPrimaryFileProcess item = new SteamGetPrimaryFileProcess (callBackDownloadFileUGCResult,file,publishID,index, dungeonId);
		ProcessList.Add (item);
	}

	public void Vote(VoteResultEventHandler callBackVoteResult,PublishedFileId_t publishID , bool bFor)
	{
		SteamVoteProcess item = new SteamVoteProcess (callBackVoteResult, publishID, bFor);
		ProcessList.Add (item);
	}

	public void DeleteFile(DeleteFileResultEventHandler callBackDeleteFileResult, string fileName,PublishedFileId_t publishID)
	{
		SteamDeleteProcess item = new SteamDeleteProcess (callBackDeleteFileResult,fileName,publishID);
		ProcessList.Add (item);
	}

	public void RefreshVoteDetail( SteamVoteDetailEventHandler callBackRefreshVoteDetail,PublishedFileId_t publishID )
	{
		SteamRefreshVoteDetailProcess item = new SteamRefreshVoteDetailProcess (callBackRefreshVoteDetail, publishID);
		ProcessList.Add(item);
	}

	public bool GetDownProgress(UGCHandle_t file,out int downloadedBytes, out int totalBytes)
	{
		return SteamRemoteStorage.GetUGCDownloadProgress (file, out downloadedBytes, out totalBytes);
	}

    public void RandomGetIsosFromWorkShop(GetRandomIsoListCallBackEventHandler callback, int amount,int dungeonId,string tag)
    {
        SteamRandomGetIsoProcess item = new SteamRandomGetIsoProcess(callback, amount, dungeonId,tag);
        ProcessList.Add(item);
    }
}

