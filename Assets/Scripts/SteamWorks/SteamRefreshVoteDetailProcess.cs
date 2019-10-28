using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//getprelistprocess
public class SteamRefreshVoteDetailProcess :ISteamGetFile
{
	CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t> RemoteStorageGetPublishedItemVoteDetailsResult;
	public SteamRefreshVoteDetailProcess(SteamVoteDetailEventHandler callBackRefreshVoteDetail,PublishedFileId_t publishID)
	{                                                        
		RemoteStorageGetPublishedItemVoteDetailsResult = CallResult<RemoteStorageGetPublishedItemVoteDetailsResult_t>.Create(OnRemoteStorageGetPublishedItemVoteDetailsResult);
		CallBackRefreshVoteDetail = callBackRefreshVoteDetail;
		RefreshVoteDetail (publishID);
		_VoteDetail = new SteamVoteDetail();
		_PublishID = publishID;

	}
	SteamVoteDetailEventHandler CallBackRefreshVoteDetail ;
	SteamVoteDetail _VoteDetail;
	PublishedFileId_t _PublishID;
	void Finish(PublishedFileId_t publishedFileId, SteamVoteDetail detail,bool bOK)
	{
		if ( CallBackRefreshVoteDetail != null )
		{
			CallBackRefreshVoteDetail(publishedFileId,detail, bOK);
		}
		ProcessList.Remove (this);
	}

	public void RefreshVoteDetail( PublishedFileId_t publishID)
	{//GetPreFileDetail step 4
		try
		{
			SteamAPICall_t handle = SteamRemoteStorage.GetPublishedItemVoteDetails(publishID);
			RemoteStorageGetPublishedItemVoteDetailsResult.Set(handle);
		}
		catch ( Exception e )
		{
			Finish(_PublishID,null,false);
			Debug.Log( "SteamRefreshVoteDetailProcess RefreshVoteDetail " + e.ToString() );
		}
	}
	void OnRemoteStorageGetPublishedItemVoteDetailsResult(RemoteStorageGetPublishedItemVoteDetailsResult_t pCallback, bool bIOFailure)
	{//GetPreFileDetail step 5		
		if ( CallBackRefreshVoteDetail != null)
		{
			if( pCallback.m_eResult == EResult.k_EResultOK )
			{
				if (_VoteDetail != null)
				{
					_VoteDetail.m_fScore = pCallback.m_fScore;
					_VoteDetail.m_nVotesFor = pCallback.m_nVotesFor;
					_VoteDetail.m_nVotesAgainst = pCallback.m_nVotesAgainst;
					_VoteDetail.m_nReports = pCallback.m_nReports;
				}
				Finish( _PublishID,_VoteDetail,true);
			}
			else
			{
				Finish( _PublishID,null,false);
			}
		}				
	}
}

