using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//getprelistprocess
public class SteamGetPreFileDetailProcess :ISteamGetFile
{
	CallResult<RemoteStorageGetPublishedFileDetailsResult_t> remoteStorageGetPublishedFileDetailsResult;
	CallResult<RemoteStorageDownloadUGCResult_t> remoteStorageDownloadPreUGCResult;
	CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t> RemoteStorageGetPublishedItemVoteDetailsResult;
	public  SteamGetPreFileDetailProcess(SteamGetPreFileCallBackEventHandler callBackGetPreFileResult, PublishedFileId_t publishID)
	{
		remoteStorageGetPublishedFileDetailsResult = CallResult<RemoteStorageGetPublishedFileDetailsResult_t>.Create(OnRemoteStorageGetPublishedFileDetailsResult);
		remoteStorageDownloadPreUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnRemoteStorageDownloadPreUGCResult);
		RemoteStorageGetPublishedItemVoteDetailsResult = CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t>.Create(OnRemoteStorageGetPublishedItemVoteDetailsResult);

		CallBackGetPreFileResult = callBackGetPreFileResult;
		GetPreFileDetail (publishID);
		_PublishID = publishID;
	}

	SteamGetPreFileCallBackEventHandler CallBackGetPreFileResult;

	SteamPreFileAndVoteDetail _PreFileDetail = new SteamPreFileAndVoteDetail();
	PublishedFileId_t _PublishID;
	void Finish(PublishedFileId_t publishedFileId, SteamPreFileAndVoteDetail detail,bool bOK)
	{
		if ( CallBackGetPreFileResult != null)
		{
			CallBackGetPreFileResult (publishedFileId, detail,bOK );
		}
		ProcessList.Remove (this);
    }
    public void GetPreFileDetail( PublishedFileId_t publishID )
	{//GetPreFileDetail step 1
		try
		{
			if( publishID.m_PublishedFileId == 0)
			{
				ProcessList.Remove (this);
				return;
			}
			SteamAPICall_t handle = SteamRemoteStorage.GetPublishedFileDetails( publishID,0 );	
			remoteStorageGetPublishedFileDetailsResult.Set(handle);
		}
		catch(Exception e)
		{
			Finish (_PublishID, null,false );
			Debug.Log( "SteamGetPreFileDetailProcess GetPreFileDetail " + e.ToString() );   
		}
	}
	void OnRemoteStorageGetPublishedFileDetailsResult(RemoteStorageGetPublishedFileDetailsResult_t pCallback, bool bIOFailure)
	{//GetPreFileDetail step 2
		try
		{
			//Debug.Log("[" + RemoteStorageGetPublishedFileDetailsResult_t.k_iCallback + " - RemoteStorageGetPublishedFileDetailsResult] - " + pCallback.m_eResult + " -- " + pCallback.m_nPublishedFileId + " -- " + pCallback.m_nCreatorAppID + " -- " + pCallback.m_nConsumerAppID + " -- " + pCallback.m_rgchTitle + " -- " + pCallback.m_rgchDescription + " -- " + pCallback.m_hFile + " -- " + pCallback.m_hPreviewFile + " -- " + pCallback.m_ulSteamIDOwner + " -- " + pCallback.m_rtimeCreated + " -- " + pCallback.m_rtimeUpdated + " -- " + pCallback.m_eVisibility + " -- " + pCallback.m_bBanned + " -- " + pCallback.m_rgchTags + " -- " + pCallback.m_bTagsTruncated + " -- " + pCallback.m_pchFileName + " -- " + pCallback.m_nFileSize + " -- " + pCallback.m_nPreviewFileSize + " -- " + pCallback.m_rgchURL + " -- " + pCallback.m_eFileType + " -- " + pCallback.m_bAcceptedForUse);
			if (pCallback.m_eResult == EResult.k_EResultOK) {
				_PreFileDetail.m_hFile = pCallback.m_hFile;
				_PreFileDetail.m_hPreviewFile = pCallback.m_hPreviewFile;
				_PreFileDetail.m_nPublishedFileId = pCallback.m_nPublishedFileId;
				_PreFileDetail.m_pchFileName = pCallback.m_pchFileName;	
				_PreFileDetail.m_rgchTitle = pCallback.m_rgchTitle;
				_PreFileDetail.m_rgchDescription = pCallback.m_rgchDescription;
                string[] str = pCallback.m_rgchTags.Split(',');
                if(str.Length > 0)
                    _PreFileDetail.m_rgchTags = str[str.Length - 1];
                //for test
                //SteamProcessMgr.Instance.DeleteFile(null,pCallback.m_pchFileName,_PublishID);

                SteamAPICall_t handle = SteamRemoteStorage.UGCDownload(pCallback.m_hPreviewFile,0);


				remoteStorageDownloadPreUGCResult.Set(handle);
			}
			else
			{

				Finish (_PublishID, null,false);
				LogManager.Warning( "OnRemoteStorageGetPublishedFileDetailsResult error") ;

			}
		}
		catch( Exception e )
		{
			Finish(_PublishID,null,false);
			Debug.Log( "SteamGetPreFileDetailProcess OnRemoteStorageGetPublishedFileDetailsResultAllUser " + e.ToString() );		
		}
		
	}
void OnRemoteStorageDownloadPreUGCResult(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure) 
	{//GetPreFileDetail step 3
		try
		{

			if( pCallback.m_eResult == EResult.k_EResultOK )
			{
				byte[] Data = new byte[pCallback.m_nSizeInBytes];
				SteamRemoteStorage.UGCRead (pCallback.m_hFile, Data, pCallback.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_Close);
				//combin data for Prefile and votedetail
				// parsing
				_PreFileDetail.m_aPreFileData = ParsingName(Data,out _PreFileDetail.m_sUploader);
				GetVoteDetail(_PreFileDetail.m_nPublishedFileId);
				//Debug.Log("[" + RemoteStorageDownloadUGCResult_t.k_iCallback + " - RemoteStorageDownloadUGCResult] - " + pCallback.m_eResult + " -- " + pCallback.m_hFile + " -- " + pCallback.m_nAppID + " -- " + pCallback.m_nSizeInBytes + " -- " + pCallback.m_pchFileName + " -- " + pCallback.m_ulSteamIDOwner);
			}
			else
			{

				Finish (_PublishID, null,false);
				LogManager.Warning( "OnRemoteStorageDownloadPreUGCResult ", pCallback.m_eResult) ;

			}
		}
		catch(Exception e)
		{

			Finish (_PublishID, null,false);         
            Debug.Log( "SteamGetPreFileDetailProcess OnRemoteStorageDownloadPreUGCResultAllUser " + e.ToString() );
		}
	}
	
	public void GetVoteDetail( PublishedFileId_t publishID)
	{//GetPreFileDetail step 4
		try
		{
			SteamAPICall_t handle = SteamRemoteStorage.GetPublishedItemVoteDetails(publishID);


			RemoteStorageGetPublishedItemVoteDetailsResult.Set(handle);
			//Debug.Log("GetPublishedItemVoteDetails(" + publishID + ") - " + handle);
		}
		catch ( Exception e )
		{

			Finish(_PublishID,null,false);
			Debug.Log( "SteamGetPreFileDetailProcess GetVoteDetailAllUser " + e.ToString() );
		}
	}
	void OnRemoteStorageGetPublishedItemVoteDetailsResult(RemoteStorageGetPublishedItemVoteDetailsResult_t pCallback, bool bIOFailure)
	{//GetPreFileDetail step 5		
		if ( CallBackGetPreFileResult != null)
		{
			if( pCallback.m_eResult == EResult.k_EResultOK )
			{
				if (_PreFileDetail != null)
				{
					_PreFileDetail.m_VoteDetail.m_fScore = pCallback.m_fScore;
					_PreFileDetail.m_VoteDetail.m_nVotesFor = pCallback.m_nVotesFor;
					_PreFileDetail.m_VoteDetail.m_nVotesAgainst = pCallback.m_nVotesAgainst;
					_PreFileDetail.m_VoteDetail.m_nReports = pCallback.m_nReports;
				}
				Finish( _PublishID,_PreFileDetail,true);

			}
			else
			{
				Finish( _PublishID,null,false); 
			}
		}
		//for test
		//SteamProcessMgr.Instance.GetPrimaryFile (null, _PreFileDetail.m_hFile);
		//Debug.Log("[" + RemoteStorageGetPublishedItemVoteDetailsResult_t.k_iCallback + " - RemoteStorageGetPublishedItemVoteDetailsResult_t] - " + pCallback.m_eResult + " -- " + pCallback.m_unPublishedFileId + " -- " + pCallback.m_nVotesFor + " -- " + pCallback.m_nVotesAgainst + " -- " + pCallback.m_nReports + " -- " + pCallback.m_fScore);
		
	}
	private byte[] ParsingName(byte[] data,out string uploader)
	{
		uploader = "";
		int len = data.Length;
		int index = -1;
		int loopcount = 0;
		for( int i = len-1; i > 0; i-- )
		{
			if ( i < 7)
			{
				return null;
			}
			if ( loopcount > 256 )
				break;
			//find $sinwa$
			if(data[i] == 36 && data[i - 1] == 97  && data[i - 2] == 119  && data[i - 3] == 110 && data[i - 4] == 105 && data[i - 5] == 115 && data[i - 6] == 36)
			{
				index = i - 6;
			}
			loopcount++;
		}
		if( index > 0)
		{
			byte[] NameData = new byte[len - index];
			for(int i = 0; i < NameData.Length;i++)
			{
				NameData[i] = data[index + i];
				data[index + i] = 0;
			}
			uploader = System.Text.Encoding.UTF8.GetString(NameData);
			uploader = uploader.Replace("$sinwa$","");
			byte[] newData = new byte[index];
			Array.Copy(data,newData,index);
			return newData;
		}
		return null;
	}
}

