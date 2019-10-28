using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//steamGetPrimaryFileProcess
public partial class SteamGetPrimaryFileProcess :ISteamGetFile
{
	CallResult<RemoteStorageDownloadUGCResult_t> RemoteStorageDownloadUGCResult;
    int _index;
    int _dungeonId;

    public  SteamGetPrimaryFileProcess(GetPrimaryFileResultEventHandler callBackGetPrimaryFileResult, UGCHandle_t file,PublishedFileId_t publishedFileId,int index = -1,int dungeonId = -1)
	{
		RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnRemoteStorageDownloadFileUGCResultPrimaryFile);
		CallBackGetPrimaryFileResult = callBackGetPrimaryFileResult;
		GetPrimaryFile (file);
		_PublishedFileId = publishedFileId;
        _index = index;
        _dungeonId = dungeonId;
    }
	void Finish(byte [] fileData,PublishedFileId_t publishedFileId,bool bOK )
	{
		if ( CallBackGetPrimaryFileResult != null )
		{
			CallBackGetPrimaryFileResult(fileData,publishedFileId, bOK, _index, _dungeonId);
		}
		ProcessList.Remove (this);
	}
	PublishedFileId_t _PublishedFileId;
	GetPrimaryFileResultEventHandler CallBackGetPrimaryFileResult;
	public void GetPrimaryFile( UGCHandle_t file )
	{//GetPrimaryFile step 1
		try
		{
			if( file != UGCHandle_t.Invalid)
			{
				SteamAPICall_t handle = SteamRemoteStorage.UGCDownload(file,0);

				RemoteStorageDownloadUGCResult.Set(handle);
			}
			else
			{
				LogManager.Warning( "GetFile error") ;
				Finish( null,_PublishedFileId ,false);
			}
		}
		catch ( Exception e)
		{

			Finish(null,_PublishedFileId,false);
		
			Debug.Log( "SteamGetPrimaryFileProcess GetPrimaryFile " + e.ToString() );

		}
	}
	void OnRemoteStorageDownloadFileUGCResultPrimaryFile(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure) 
	{//GetPrimaryFile step 2
		if(CallBackGetPrimaryFileResult != null)
		{
			try
			{
				if( pCallback.m_eResult == EResult.k_EResultOK )
				{
					byte[] Data = new byte[pCallback.m_nSizeInBytes];
					SteamRemoteStorage.UGCRead (pCallback.m_hFile, Data, pCallback.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_Close);
					//Debug.Log("[" + RemoteStorageDownloadUGCResult_t.k_iCallback + " - RemoteStorageDownloadUGCResult] - " + pCallback.m_eResult + " -- " + pCallback.m_hFile + " -- " + pCallback.m_nAppID + " -- " + pCallback.m_nSizeInBytes + " -- " + pCallback.m_pchFileName + " -- " + pCallback.m_ulSteamIDOwner);
					Finish( Data,_PublishedFileId ,true);			
				}
				else
				{

					Finish( null,_PublishedFileId ,false);
					LogManager.Warning( "OnRemoteStorageDownloadFileUGCResult error") ;

				}
			}
			catch ( Exception e)
			{

				Finish(null,_PublishedFileId,false);

				Debug.Log( "SteamGetPrimaryFileProcess OnRemoteStorageDownloadFileUGCResultPrimaryFile " + e.ToString() );

			}
		}
	}
}

