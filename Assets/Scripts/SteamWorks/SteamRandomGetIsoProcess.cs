using UnityEngine;
using System.Collections.Generic;
using Steamworks;
public class SteamRandomGetIsoProcess : ISteamGetFile
{

    int _getAmount;
    GetRandomIsoListCallBackEventHandler GetRandomIsoListCallBackEvent;
    CallResult<RemoteStorageEnumerateWorkshopFilesResult_t> GetTotalCountResult;
    List<CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>> GetFileInfoResult = new List<CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>>();
    List<CallResult<RemoteStorageGetPublishedFileDetailsResult_t>> GetFileDetailsResult = new List<CallResult<RemoteStorageGetPublishedFileDetailsResult_t>>();
    List<ulong> _fileIDs = new List<ulong>();
    List<ulong> _publishIDs = new List<ulong>();
    string [] _tags;
    int _dungeonId;
    public SteamRandomGetIsoProcess(GetRandomIsoListCallBackEventHandler callback, int amount,int dungeonId,string tag)
    {
        GetRandomIsoListCallBackEvent = callback;
        _getAmount = amount;        
        string [] tags = new string[2];
        tags[0] = SteamWorkShop.NewVersionTag;
		tags[1] = tag;
        GetTotalCount(1, tags);
        _tags = tags;
        _dungeonId = dungeonId;
    }

    void AddToList( ulong fileId,ulong publishId )
    {
        _fileIDs.Add(fileId);
        _publishIDs.Add(publishId);
    }

    void GetTotalCount(uint startIndex, string[] tags, uint days = 0, uint count = 1, EWorkshopEnumerationType orderBy = EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent)
    {//GetPreFileList step 1
        try
        {
            GetTotalCountResult = CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>.Create(OnGetTotalCountResult);
            SteamAPICall_t handle = SteamRemoteStorage.EnumeratePublishedWorkshopFiles(orderBy, startIndex, count, days, tags, null);
            GetTotalCountResult.Set(handle);
        }
        catch (System.Exception e)
        {
            Debug.Log("SteamRandomGetIsoProcess GetPreFileList " + e.ToString());
            Finish(_fileIDs, _publishIDs, false);
        }
    }

    void OnGetTotalCountResult(RemoteStorageEnumerateWorkshopFilesResult_t pCallback, bool bIOFailure)
    {
        try
        {
            if (pCallback.m_eResult == EResult.k_EResultOK)
            {
                for (int i = 0; i < _getAmount; i++)
                {
                    int randIdx = Random.Range(1, pCallback.m_nTotalResultCount);
                    GetFileInfoResult.Add(CallResult<RemoteStorageEnumerateWorkshopFilesResult_t>.Create(OnGetFileInfoResult));
                    SteamAPICall_t handle = SteamRemoteStorage.EnumeratePublishedWorkshopFiles(EWorkshopEnumerationType.k_EWorkshopEnumerationTypeRecent, (uint)randIdx, 1, 0, _tags, null);
                    GetFileInfoResult[i].Set(handle);
                }
            }
            else
                Finish(_fileIDs, _publishIDs, false);
        }
        catch (System.Exception e)
        {           
            Finish(_fileIDs, _publishIDs, false);
            Debug.LogError(e.ToString());
        }

    }
    void OnGetFileInfoResult(RemoteStorageEnumerateWorkshopFilesResult_t pCallback, bool bIOFailure)
    {
        try
        {
            if (pCallback.m_eResult == EResult.k_EResultOK)
            {
                GetFileDetailsResult.Add(CallResult<RemoteStorageGetPublishedFileDetailsResult_t>.Create(OnGetFinalInfo));
                SteamAPICall_t handle = SteamRemoteStorage.GetPublishedFileDetails(pCallback.m_rgPublishedFileId[0], 0);
                GetFileDetailsResult[GetFileDetailsResult.Count - 1].Set(handle);
            }
            else
                Finish(_fileIDs, _publishIDs, false);
        }
        catch (System.Exception e)
        {
            Finish(_fileIDs, _publishIDs, false);
            Debug.LogError(e.ToString());
        }
    }
    void OnGetFinalInfo(RemoteStorageGetPublishedFileDetailsResult_t pCallback, bool bIOFailure)
    {
        try
        {
            if (pCallback.m_eResult == EResult.k_EResultOK && pCallback.m_hFile.m_UGCHandle != 0)
            {
                AddToList(pCallback.m_hFile.m_UGCHandle, pCallback.m_nPublishedFileId.m_PublishedFileId);
                if (_getAmount == _fileIDs.Count)
                {
                    Finish(_fileIDs, _publishIDs, true);
                }
            }
            else
                Finish(_fileIDs, _publishIDs, false);
        }
        catch (System.Exception e)
        {
            Finish(_fileIDs, _publishIDs, false);
            Debug.LogError(e.ToString());
        }
    }
    void Finish(List<ulong> fileIDsList, List<ulong> publishIds, bool bOK)
    {
        if (GetRandomIsoListCallBackEvent != null)
        {
            GetRandomIsoListCallBackEvent(fileIDsList, publishIds, _dungeonId, bOK);
        }
        ProcessList.Remove(this);
    }
}
