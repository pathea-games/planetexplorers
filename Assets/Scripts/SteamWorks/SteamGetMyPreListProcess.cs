using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//getmyprelistprocess
public class SteamGetMyPreListProcess :ISteamGetFile
{
	CallResult<RemoteStorageEnumerateUserSharedWorkshopFilesResult_t> remoteStorageEnumerateUserSharedWorkshopFilesResult;
	public SteamGetMyPreListProcess(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex,string [] tags,uint count = 9)
	{
       
        remoteStorageEnumerateUserSharedWorkshopFilesResult = CallResult<RemoteStorageEnumerateUserSharedWorkshopFilesResult_t>.Create(OnRemoteStorageEnumerateUserSharedWorkshopFilesResultMyPreFileList);
		CallBackGetPreListResult = callBackGetPreListResult;
        //tags = SteamWorkShop.AddNewVersionTag(tags);
        GetMyPreFileList ( startIndex, tags);
		_StartIndex = startIndex;
		_Count = count;
	}
	//get preview call back
	GetPreListCallBackEventHandler CallBackGetPreListResult;
	public List< PublishedFileId_t> _FileIDLsit = new List<PublishedFileId_t>();
	uint _StartIndex;
	uint _Count;

	void Finish( List<PublishedFileId_t> publishIDList,int totalResults,uint startIndex, bool bOK)
	{
		
		if(CallBackGetPreListResult != null)
			CallBackGetPreListResult(publishIDList,totalResults,startIndex,bOK);
		ProcessList.Remove (this);
		
	}
	public void GetMyPreFileList( uint startIndex,string [] tags)
	{//GetMyPreFileList step 1		
		try
		{
			SteamAPICall_t handle = SteamRemoteStorage.EnumerateUserSharedWorkshopFiles(SteamMgr.steamId, startIndex,tags,null);	
			remoteStorageEnumerateUserSharedWorkshopFilesResult.Set(handle);
		}
		catch( Exception e )
		{

			Finish(_FileIDLsit,0,_StartIndex,false);
			Debug.Log( "SteamGetMyPreListProcess GetMyPreFileList " + e.ToString() );
		}
	} 
	void OnRemoteStorageEnumerateUserSharedWorkshopFilesResultMyPreFileList (RemoteStorageEnumerateUserSharedWorkshopFilesResult_t pCallback, bool bIOFailure)
	{//GetMyPreFileList step 2

		if (pCallback.m_eResult == EResult.k_EResultOK) 
		{
			//if ( pCallback.m_nResultsReturned > 0 ) 
			{
				int count = pCallback.m_nResultsReturned;
				if( count > _Count)
					count = (int)_Count;
				for ( int i = 0; i < count; i++ )
				{
					_FileIDLsit.Add( pCallback.m_rgPublishedFileId[i]);
					//for test
					//SteamProcessMgr.Instance.GetPreFileDetail(null,pCallback.m_rgPublishedFileId[i]);
				}

				Finish(_FileIDLsit,pCallback.m_nTotalResultCount,_StartIndex,true);

			}
		}
		else
		{

			Finish(_FileIDLsit,0,_StartIndex,false);
			LogManager.Warning(" OnRemoteStorageEnumerateUserSharedWorkshopFilesResult error ");
		}

       // Debug.Log("[" + RemoteStorageEnumerateUserSharedWorkshopFilesResult_t.k_iCallback + " - RemoteStorageEnumerateUserSharedWorkshopFilesResult] - " + pCallback.m_eResult + " -- " + pCallback.m_nResultsReturned + " -- " + pCallback.m_nTotalResultCount + " -- " + pCallback.m_rgPublishedFileId);
	}

}

