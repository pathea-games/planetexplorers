using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//SteamSearchProcess
public class SteamSearchProcess :ISteamGetFile
{
    CallResult<SteamUGCQueryCompleted_t> OnSteamUGCQueryCompletedCallResult;
    private UGCQueryHandle_t _UGCQueryHandle;
    SteamAPICall_t _callbackHandle;
    public  SteamSearchProcess(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex,string [] tags,uint count = 9,string searchText = "")
	{
        OnSteamUGCQueryCompletedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(CallBackSendQuery);
		_Page = startIndex / 50 + 1;
		_StartIndex = startIndex;
		CallBackGetPreListResult = callBackGetPreListResult;
		_Count = count;
        tags = SteamWorkShop.AddNewVersionTag(tags);
        Search (tags, searchText);

	}
	GetPreListCallBackEventHandler CallBackGetPreListResult;
	public List< PublishedFileId_t> _FileIDLsit = new List<PublishedFileId_t>();
	uint _StartIndex;
	uint _Count;
	uint _Page;

	void Finish( List<PublishedFileId_t> publishIDList,int totalResults,uint startIndex, bool bOK)
	{
		if ( CallBackGetPreListResult != null )
		{
			CallBackGetPreListResult(publishIDList,totalResults, startIndex,bOK);
        }
		ProcessList.Remove (this);
    }

	public void Search(string []tags,string searchText)
	{
		try
		{
            if (!SteamUser.BLoggedOn())
                return;
            _UGCQueryHandle = SteamUGC.CreateQueryAllUGCRequest (EUGCQuery.k_EUGCQuery_RankedByVote, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, SteamUtils.GetAppID (), SteamUtils.GetAppID (), _Page);
			SteamUGC.SetSearchText (_UGCQueryHandle, searchText);
			if(tags != null && tags.Length > 0)
			{
				for( int i = 0; i < tags.Length; i++)
				{
					SteamUGC.AddRequiredTag(_UGCQueryHandle, tags[i]);
				}
			}
            _callbackHandle = SteamUGC.SendQueryUGCRequest (_UGCQueryHandle);
            OnSteamUGCQueryCompletedCallResult.Set(_callbackHandle); 
			ProcessList.Remove (this);
		}
		catch(Exception)
		{
			Finish(null,0,_StartIndex,false);
		}
	}
	public void CallBackSendQuery(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
	{
		try
		{
			if( pCallback.m_eResult == EResult.k_EResultOK)
			{
				uint count = pCallback.m_unNumResultsReturned;
				uint index = _StartIndex%50;
				if( count - index > _Count)
				{
					count = _Count;
				}
				SteamUGCDetails_t detail = new SteamUGCDetails_t();
				for( uint i = index; i < count + index; i++)
				{
					SteamUGC.GetQueryUGCResult(_UGCQueryHandle, i,out detail);
					_FileIDLsit.Add(detail.m_nPublishedFileId);
					Debug.LogWarning("CallBackSendQuery PublishedFileId_t " + i.ToString() + " = " + detail.m_nPublishedFileId);
				}

				Finish(_FileIDLsit,(int)pCallback.m_unTotalMatchingResults,_StartIndex,true);

			}
			else
			{

				Finish(null,0,_StartIndex,false);

			}
		}
		catch(Exception)
		{

			Finish(null,0,_StartIndex,false);

		}
	}
}

