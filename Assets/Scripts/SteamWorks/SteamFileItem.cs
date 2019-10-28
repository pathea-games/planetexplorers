using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using Steamworks;
using Pathea;

public enum ExportStep
{
    None = 0,
    PreFileSending = 1,
    RealFileSending = 2,
    Shareing = 3,
    WaitingOtherPlayer = 4,
    ExportSuccessed = 5,
    ExportFailed = 6,
    NotEnoughResources = 7,
}

internal class SteamFileItem
{
    public int ID { get { return _ID; } }
    public int instanceId;
    static int curinstanceId = 0;


    private string _RealFileName;
	private string _FileName;
	private string _PreFileName;
	private string _FileDescription;
	private int _FileSize;
	private ulong _hashCode;
	private string[] _Tags;
	private byte[] _PreData;
	internal byte[] _Data;
	private UGCHandle_t _UGCHandle;
	private PublishedFileId_t _PublishedFileId;
	public bool _SendToServer;
	private bool _Publish;
	private int _ID;
    public bool _free = false;
    ExportStep _step = ExportStep.None;

    internal ulong FileID { get { return _UGCHandle.m_UGCHandle; } }
	internal ulong PublishID { get { return _PublishedFileId.m_PublishedFileId; } }
	internal ulong HashCode { get { return _hashCode; } }

	internal string FileName { get { return _FileName; } }

	internal string RealFileName { get { return _RealFileName; } }

	private CallResult<RemoteStorageFileShareResult_t> RemoteStoragePreFileShareResult;
	private CallResult<RemoteStorageFileShareResult_t> RemoteStorageFileShareResult;
	private CallResult<RemoteStoragePublishFileResult_t> RemoteStorageFilePublishResult;
	SteamUploadEventHandler CallBackSteamUploadResult;

	private event EventHandler FileShareEvent;
	private event EventHandler FilePublishEvent;
    List<SteamAPICall_t> _handler = new List<SteamAPICall_t>();

    static List<SteamFileItem> _mgr = new List<SteamFileItem>();

    internal SteamFileItem()
	{
		RemoteStoragePreFileShareResult = CallResult<RemoteStorageFileShareResult_t>.Create(OnRemoteStoragePreFileShareResult);
		RemoteStorageFileShareResult = CallResult<RemoteStorageFileShareResult_t>.Create(OnRemoteStorageFileShareResult);
		RemoteStorageFilePublishResult = CallResult<RemoteStoragePublishFileResult_t>.Create(OnRemoteStorageFilePublishResult);
		FileShareEvent += SteamWorkShop.OnFileSharedEvent;
		FilePublishEvent += SteamWorkShop.OnFilePublishEvent;
        _mgr.Add(this);
        curinstanceId++;
        instanceId = curinstanceId;

    }
        
	internal SteamFileItem(SteamUploadEventHandler callBackSteamUploadResult,string name, string desc, byte[] preData, byte[] data, ulong hashCode,string[] tags,bool publish = true,bool sendToServer = true,int id = -1,ulong fileId = 0,bool free = false)
	:this()
	{        
		_ID = id;
        _RealFileName = name;
		_FileName = hashCode.ToString();
		_PreFileName = hashCode.ToString() + "_preview";
		_Data = data;
		_FileDescription = desc;
		_hashCode = hashCode;
		_Tags = tags;
		_SendToServer = sendToServer;
		_Publish = publish;
		CallBackSteamUploadResult = callBackSteamUploadResult;
		//add uploader name
		byte[] Uploader=System.Text.Encoding.UTF8.GetBytes(SteamWorkShop._SteamPersonaName);	
		int length = preData.Length + Uploader.Length;
		_PreData = new byte[length];
		preData.CopyTo (_PreData, 0);
		Uploader.CopyTo (_PreData, preData.Length);
        _UGCHandle = new UGCHandle_t(fileId);
        _free = free;
        _mgr.Add(this);
        curinstanceId++;
        instanceId = curinstanceId;
    }
	public void StartSend()
	{
		SteamPreFileWrite ();
	}
	internal void SteamPreFileWrite()
    {//step 1 正在上传预览文件到steam workshop
        try
		{
            if ( _PreData != null && _PreData.Length > 0)
			{
                _step = ExportStep.PreFileSending;
                ShowMsg(_step);
                if (!IsShared(FileName))
                {
                    if (!IsWorking(FileName))
                    {
                        if (!SteamRemoteStorage.FileWrite(_PreFileName, _PreData, _PreData.Length))
                            throw new Exception("File write failed. File Name: " + _PreFileName);
                    }
                    else
                        return;
                }
                else
                {
                    OnFileShared();
                }
                
                SteamAPICall_t handler = SteamRemoteStorage.FileShare(_PreFileName);
                _handler.Add(handler);
                RemoteStoragePreFileShareResult.Set(handler);
			}
			else
			{
				throw new Exception("Invalid file data. File Name: " + _PreFileName);
			}
		}
		catch (Exception)
		{			
			if( CallBackSteamUploadResult != null)
			{
				CallBackSteamUploadResult(_ID,false,_hashCode);
			}
            //step 6 导出失败
            _step = ExportStep.ExportFailed;
            ShowMsg(_step);
            
        }
    }
	internal void SteamFileWrite()
    {//step 2 正在上传iso主体文件到steam workshop；
        try
		{
            ShowMsg(ExportStep.RealFileSending);
            if (!SteamRemoteStorage.FileWrite(_FileName, _Data, _Data.Length))
                throw new Exception("File write failed. File Name:" + _FileName);
            SteamAPICall_t handler = new SteamAPICall_t();
            //step 3、正在为其他玩家共享iso；
            _step = ExportStep.Shareing;
            ShowMsg(_step);
            handler = SteamRemoteStorage.FileShare(_FileName);
            _handler.Add(handler);
            RemoteStorageFileShareResult.Set(handler);
        }
		catch (Exception)
		{
			if( CallBackSteamUploadResult != null)
			{
				CallBackSteamUploadResult(_ID,false,_hashCode);
            }
			SteamRemoteStorage.FileDelete(_PreFileName);
            //step 6 导出失败
            _step = ExportStep.ExportFailed;
            ShowMsg(_step);
        }
    }

	void OnRemoteStoragePreFileShareResult(RemoteStorageFileShareResult_t pCallback, bool bIOFailure)
	{
		try
		{
            if (pCallback.m_eResult != EResult.k_EResultOK )
                throw new Exception("Upload iso data failed.");
            SteamFileWrite();
		}
		catch (Exception)
		{

            Debug.Log( "workshop error:" + pCallback.m_eResult);
            if ( CallBackSteamUploadResult != null)
			{
                CallBackSteamUploadResult(_ID, false, _hashCode);
            }
            //step 6 导出失败
            _step = ExportStep.ExportFailed;
            ShowMsg(_step);
        }
    }

	void OnRemoteStorageFileShareResult(RemoteStorageFileShareResult_t pCallback, bool bIOFailure)
	{
		try
		{
            if (pCallback.m_eResult != EResult.k_EResultOK )
			{
				if( CallBackSteamUploadResult != null)
				{
                    CallBackSteamUploadResult(_ID, false, _hashCode);
                }
                _step = ExportStep.ExportFailed;
                ShowMsg(_step);
                return;
			}

			_UGCHandle = pCallback.m_hFile;
			OnFileShared();
			if( CallBackSteamUploadResult != null)
			{
                CallBackSteamUploadResult(_ID, true, _hashCode);
            }
			if( !_Publish ) return;
			SteamAPICall_t handler = SteamRemoteStorage.PublishWorkshopFile(_FileName, _PreFileName, SteamUtils.GetAppID(),
			                                                                _RealFileName, _FileDescription,ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic,
			                                                                _Tags, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            _handler.Add(handler);
            RemoteStorageFilePublishResult.Set(handler);
		}
		catch (Exception)
		{
            Debug.Log("workshop error:" + pCallback.m_eResult);
            if ( CallBackSteamUploadResult != null)
			{
                CallBackSteamUploadResult(_ID, false, _hashCode);
            }
            //step 6 导出失败
            _step = ExportStep.ExportFailed;
            ShowMsg(_step);
        }
    }

	void OnFileShared()
    {//step 4、等待其他玩家从steam workshop中自动下载iso
        foreach (var iter in SteamFileItem._mgr)
        {
            if (iter._FileName == FileName && iter._step < ExportStep.WaitingOtherPlayer)
            {
                iter._UGCHandle = _UGCHandle;
                iter._step = ExportStep.WaitingOtherPlayer;
                iter.ShowMsg(iter._step);
                if (null != iter.FileShareEvent && iter._SendToServer)
                    iter.FileShareEvent(iter, EventArgs.Empty);
            }
        }
	}

	void OnRemoteStorageFilePublishResult(RemoteStoragePublishFileResult_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			_PublishedFileId = pCallback.m_nPublishedFileId;
			LobbyInterface.LobbyRPC(ELobbyMsgType.UploadISOSuccess, _hashCode, SteamMgr.steamId.m_SteamID);
        }
        if(EResult.k_EResultInsufficientPrivilege == pCallback.m_eResult)
        {
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000491));
        }
        OnFilePublished();
    }

	void OnFilePublished()
	{
		if (null != FilePublishEvent)
			FilePublishEvent(this, EventArgs.Empty);
        _mgr.Remove(this);
	}
    public static void RPC_S2C_IsoExportSuccessed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {//step 5 导出成功
        int instanceid = stream.Read<int>();
        bool suc = stream.Read<bool>();
        foreach(var iter in SteamFileItem._mgr)
        {
            if(iter.instanceId == instanceid)
            {
                if(suc)
                    IsoUploadInfoWndCtrl.Instance.UpdateIsoState(instanceid, iter.RealFileName, (int)ExportStep.ExportSuccessed);
                else
                    IsoUploadInfoWndCtrl.Instance.UpdateIsoState(instanceid, iter.RealFileName, (int)ExportStep.NotEnoughResources);
                break;
            }
        }
    }
    bool IsShared(string filename)
    {
        foreach (var iter in SteamFileItem._mgr)
        {
            if (iter._FileName == filename && iter._step == ExportStep.ExportSuccessed)
            {
                return true;
            }
        }
        return false;
    }
    bool IsWorking(string filename)
    {
        if (_step > ExportStep.PreFileSending)
            return true;
        return false;
    }
    void SetUGCHandle(string filename)
    {
        foreach (var iter in SteamFileItem._mgr)
        {
            if (iter._FileName == filename && iter._step == ExportStep.ExportSuccessed)
            {
                _UGCHandle = iter._UGCHandle;
            }
        }
    }
    void ShowMsg(ExportStep step)
    {
        if (!_free)
        {
            IsoUploadInfoWndCtrl.Instance.UpdateIsoState(instanceId, RealFileName, (int)step);
        }
    }
}
