using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//getprelistprocess
public class SteamGetPreFileListProcess :ISteamGetFile
{
	CallResult<RemoteStorageEnumerateWorkshopFilesResult_t> RemoteStorageEnumerateWorkshopFilesResult;
	public  SteamGetPreFileListProcess(GetPreListCallBackEventHandler callBackGetPreListResult,    uint startIndex,string [] tags,uint days = 0,uint count = 9,  EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRankedByVote)
	{
		RemoteStorageEnumerateWorkshopFilesResult = CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>.Create(OnRemoteStorageEnumerateWorkshopFilesResultAllUser);

		CallBackGetPreListResult = callBackGetPreListResult;
        tags = SteamWorkShop.AddNewVersionTag(tags);
        GetPreFileList (startIndex,tags,days,count,orderBy);
		_StartIndex = startIndex;
	}
	GetPreListCallBackEventHandler CallBackGetPreListResult;
	public List< PublishedFileId_t> _FileIDLsit = new List<PublishedFileId_t>();
	uint _StartIndex;

	void Finish( List<PublishedFileId_t> publishIDList,int totalResults,uint startIndex, bool bOK)
	{
		if ( CallBackGetPreListResult != null )
		{
			CallBackGetPreListResult(publishIDList,totalResults,startIndex, bOK);
        }
		ProcessList.Remove (this);
    }

	public void GetPreFileList( uint startIndex,string [] tags,uint days = 0,uint count = 9,EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent)
	{//GetPreFileList step 1
		try
		{


			SteamAPICall_t handle = SteamRemoteStorage.EnumeratePublishedWorkshopFiles(orderBy, startIndex, count, days, tags, null);
			RemoteStorageEnumerateWorkshopFilesResult.Set(handle); 
		}
		catch(Exception e)
		{

			Finish(_FileIDLsit,0,_StartIndex, false);

			Debug.Log( "SteamGetPreFileListProcess GetMyPreFileList " + e.ToString() );

        }
    }  
	void OnRemoteStorageEnumerateWorkshopFilesResultAllUser(RemoteStorageEnumerateWorkshopFilesResult_t pCallback, bool bIOFailure) 
	{//GetPreFileList step 2

		//Debug.Log ("[" + RemoteStorageEnumerateWorkshopFilesResult_t.k_iCallback + " - RemoteStorageEnumerateWorkshopFilesResult] - " + pCallback.m_eResult + " -- " + pCallback.m_nResultsReturned + " -- " + pCallback.m_nTotalResultCount + " -- " + pCallback.m_rgPublishedFileId + " -- " + pCallback.m_rgScore + " -- " + pCallback.m_nAppId + " -- " + pCallback.m_unStartIndex);
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			for ( int i = 0; i < pCallback.m_nResultsReturned; i++ )
			{
				_FileIDLsit.Add( pCallback.m_rgPublishedFileId[i]);
				//for test
				//SteamProcessMgr.Instance.GetPreFileDetail(null,pCallback.m_rgPublishedFileId[i]);
            }

			Finish(_FileIDLsit,pCallback.m_nTotalResultCount,_StartIndex,true);

			//for (int i = 0; i < pCallback.m_nResultsReturned; ++i) {
			//	Debug.Log (i + ": " + pCallback.m_rgPublishedFileId [i]);
			//}
		}
		else
		{

			Finish(_FileIDLsit,0,_StartIndex,false);

		}
	}	  	    
}

