using UnityEngine;
using System.Collections;
using Steamworks;
using System.Collections.Generic;
using System;
//SteamSearchProcess
public class SteamSearchMyProcess :ISteamGetFile
{
	CallResult<SteamUGCQueryCompleted_t> SteamUGCSendQueryUGCResult;
    private UGCQueryHandle_t _UGCQueryHandle;
    SteamAPICall_t _callbackHandle;
    public  SteamSearchMyProcess(GetPreListCallBackEventHandler callBackGetPreListResult, uint startIndex,string [] tags,uint count = 9,string searchText = "")
	{
		_Page = startIndex / 50 + 1;
		_StartIndex = startIndex;
		CallBackGetPreListResult = callBackGetPreListResult;
		SteamUGCSendQueryUGCResult = CallResult<SteamUGCQueryCompleted_t>.Create (CallBackSendQuery);
		_Count = count;
        //tags = SteamWorkShop.AddNewVersionTag(tags);

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
            _UGCQueryHandle = SteamUGC.CreateQueryUserUGCRequest ( SteamUser.GetSteamID().GetAccountID(),EUserUGCList.k_EUserUGCList_Published,EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,EUserUGCListSortOrder.k_EUserUGCListSortOrder_VoteScoreDesc, SteamUtils.GetAppID (), SteamUtils.GetAppID (),_Page);
            SteamUGC.SetCloudFileNameFilter(_UGCQueryHandle, searchText);

            if (tags != null && tags.Length > 0)
			{
				for( int i = 0; i < tags.Length; i++)
				{
					SteamUGC.AddRequiredTag(_UGCQueryHandle, tags[i]);
				}
			}
            _callbackHandle = SteamUGC.SendQueryUGCRequest (_UGCQueryHandle);


			SteamUGCSendQueryUGCResult.Set (_callbackHandle);
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

