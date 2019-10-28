using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//steamvoteprocess
public partial class SteamVoteProcess :ISteamGetFile
{
	CallResult<RemoteStorageUpdateUserPublishedItemVoteResult_t> RemoteStorageUpdateUserPublishedItemVoteResult;
	public  SteamVoteProcess(VoteResultEventHandler callBackVoteResult,PublishedFileId_t publishID , bool bFor)
	{
		CallBackVoteResult = callBackVoteResult;
		RemoteStorageUpdateUserPublishedItemVoteResult = CallResult<RemoteStorageUpdateUserPublishedItemVoteResult_t>.Create(OnRemoteStorageUpdateUserPublishedItemVoteResultVote);
		Vote (publishID, bFor);
		_PublishID = publishID;
		_BFor = bFor;
	}
	VoteResultEventHandler CallBackVoteResult;
	PublishedFileId_t _PublishID;
	bool _BFor;

	void Finish( PublishedFileId_t publishID,bool bFor,bool bOK)
	{
		if ( CallBackVoteResult != null )
		{
			CallBackVoteResult(publishID,bFor, bOK);
		}
		ProcessList.Remove (this);
	}

	public void Vote(PublishedFileId_t publishID , bool bFor)
	{//vote step1
		try
		{
			if( publishID.m_PublishedFileId == 0)
			{
				ProcessList.Remove (this);
				return;
			}


			SteamAPICall_t handle = SteamRemoteStorage.UpdateUserPublishedItemVote(publishID, true);
			RemoteStorageUpdateUserPublishedItemVoteResult.Set(handle);
			//Debug.Log("UpdateUserPublishedItemVote(" + publishID + ") - " + handle);
		}
		catch ( Exception e)
		{

			Finish(_PublishID,_BFor,false);

			Debug.Log( "SteamVoteProcess Vote " + e.ToString() );

		}
	}
	void OnRemoteStorageUpdateUserPublishedItemVoteResultVote(RemoteStorageUpdateUserPublishedItemVoteResult_t pCallback, bool bIOFailure) 
	{//vote step2
		if (CallBackVoteResult != null )
		{
			if( pCallback.m_eResult == EResult.k_EResultOK)
				Finish(_PublishID,_BFor,true);
			else
				Finish(_PublishID,_BFor,false);
		}
		//Debug.Log("[" + RemoteStorageUpdateUserPublishedItemVoteResult_t.k_iCallback + " - RemoteStorageUpdateUserPublishedItemVoteResult] - " + pCallback.m_eResult + " -- " + pCallback.m_nPublishedFileId);
	}
	

}

