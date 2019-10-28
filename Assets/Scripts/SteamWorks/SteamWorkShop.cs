using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

using Steamworks;
using CustomData;
using ItemAsset;
using SkillAsset;
using System.Xml;
public delegate void SteamUploadEventHandler(int Id, bool bOK,ulong hash);

public class SendIsoCache
{
    public SteamUploadEventHandler callBackSteamUploadResult;
    public string name;
    public string desc;
    public byte[] preData;
    public byte[] data;
    public string[] tags;
    public bool sendToServer;
    public int id;
    public ulong hash;
    public bool bPublish;
}
public class DungeonIsos
{
    public enum IsoState
    {
        IsoState_None,
        IsoState_Exporting,
        IsoState_Exported,
    }
    public class DungeonIsoData
    {
        public ulong _fileId;
        public ulong _publishId;
        public IsoState _export;
        public ulong _hashCode;
    }
    public int _amount;
    public int _dungeonId;
    public List<DungeonIsoData> _isos = new List<DungeonIsoData>();

    public DungeonIsos(int amount, int dungeonId)
    {
        _amount = amount;
        _dungeonId = dungeonId;
    }
    public static List<DungeonIsos> DungeonIsosMgr = new List<DungeonIsos>();
    public static bool AddItem(int amount, int dungenoId)
    {
        foreach (var iter in DungeonIsosMgr)
        {
            if (iter._dungeonId == dungenoId)
                return false;
        }
        DungeonIsos iso = new DungeonIsos(amount, dungenoId);
        DungeonIsosMgr.Add(iso);
        return true;
    }
    public static bool AddIsos(int dungeonId, ulong[] fileIds, ulong[] publishIds)
    {
        foreach (var iter in DungeonIsosMgr)
        {
            if (iter._dungeonId == dungeonId && iter._isos.Count == 0)
            {
                for (int i = 0; i < fileIds.Length; i++)
                {
                    DungeonIsoData data = new DungeonIsoData();
                    data._fileId = fileIds[i];
                    data._publishId = publishIds[i];
                    iter._isos.Add(data);
                }
                return true;
            }
        }
        return false;
    }
    public static bool CheckExported(int dungeonId, int index)
    {
        foreach (var iter in DungeonIsosMgr)
        {
            if (iter._dungeonId == dungeonId)
            {
                if (iter._isos[index]._export == IsoState.IsoState_None)
                    return true;
                else
                    return false;
            }
        }
        return false;
    }
    public static void SetIsoState(int dungeonId, int index, IsoState state)
    {
        foreach (var iter in DungeonIsosMgr)
        {
            if (iter._dungeonId == dungeonId)
            {
                iter._isos[index]._export = state;
            }
        }
    }
    public static void SetHashCode(int dungeonId, int index, ulong hashcode)
    {
        foreach (var iter in DungeonIsosMgr)
        {
            if (iter._dungeonId == dungeonId)
            {
                iter._isos[index]._hashCode = hashcode;
            }
        }
    }
    public static bool IsDungeonIso(ulong hashcode, out int dungeonId)
    {
        foreach (var iter in DungeonIsosMgr)
        {
            foreach (var iter1 in iter._isos)
            {
                if (iter1._hashCode == hashcode && iter1._export == IsoState.IsoState_Exporting)
                {
                    dungeonId = iter._dungeonId;
                    iter1._export = IsoState.IsoState_Exported;
                    return true;
                }
            }
        }
        dungeonId = -1;
        return false;
    }
    public static ulong GetFileId(int dungeonId,int index)
    {
        foreach (var iter in DungeonIsosMgr)
        {
            if (iter._dungeonId == dungeonId)
            {
                return iter._isos[index]._fileId;
            }
        }
        return 0;
    }
}
public class SteamWorkShop : MonoBehaviour
{
	private static SteamWorkShop _instance;
	internal static SteamWorkShop Instance { get { return _instance; } } 

	private bool _steamInit;

	internal static List<string> Tags = new List<string> { "Item", "Weapon" };

	private static Dictionary<ulong, string> _SteamItems = new Dictionary<ulong, string>();
	private static List<RegisteredISO> _DownloadList = new List<RegisteredISO>();
    static List<ulong> _CurDownloadList = new List<ulong>();
    private static ulong _CurDownload;
	public static string _SteamPersonaName { get; set;}
	public static List<CreationOriginData> CreationList = new List<CreationOriginData>();
	static CallResult<RemoteStorageDownloadUGCResult_t> RemoteStorageDownloadPreUGCResult;
    #region SendIsoCache
    public static string NewVersionTag = "2.3";
    static Dictionary<ulong, SendIsoCache> SendIsoCacheMgr = new Dictionary<ulong, SendIsoCache>();
    public static bool AddToIsoCache(SendIsoCache iso)
    {
        if(SendIsoCacheMgr.ContainsKey( iso.hash))
        {
            return false;
        }
        SendIsoCacheMgr.Add(iso.hash, iso);
        return true;
    }
    public static void RemoveIsoCache(SendIsoCache iso)
    {
        SendIsoCacheMgr.Remove(iso.hash);
    }
    public static void SendCacheIso(ulong hash)
    {
        if (SendIsoCacheMgr.ContainsKey(hash))
        {
            SendIsoCache iso = SendIsoCacheMgr[hash];
            SteamFileItem item = new SteamFileItem(iso.callBackSteamUploadResult, iso.name, iso.desc, iso.preData, iso.data, iso.hash, iso.tags, iso.bPublish, iso.sendToServer, iso.id,0,true);
            item.StartSend();
            RemoveIsoCache(iso);
        }
    }
    public static SendIsoCache GetCacheIso(ulong hash)
    {
        if (SendIsoCacheMgr.ContainsKey(hash))
        {
            return SendIsoCacheMgr[hash];
        }
        return null;
    }
    public static string[] AddNewVersionTag(string[] tags)
    {
        for(int i = 0; i < tags.Length;i++)
        {
            if (tags[i] == NewVersionTag)
                return tags;
        }
        List<string> strTags = new List<string>();
        if(tags.Length > 0)
            strTags.AddRange(tags);
        strTags.Add(NewVersionTag);
        return strTags.ToArray();
    }
    #endregion
    #region getcreation
    public static CreationData GetCreation(int objId)
	{
		CreationOriginData cData = CreationList.Find( iter=> iter.ObjectID == objId );
		if ( cData != null)
		{
            ItemProto.Mgr.Instance.Remove(objId);

			/*CreationData data = */CreationMgr.NewCreation(cData.ObjectID, cData.HashCode, (float)cData.Seed);
//			if(data != null && cData.HP != -1 && cData.Fuel != -1)
//			{
//				data.m_Hp = cData.HP;
//				data.m_Fuel = cData.Fuel;
//			}

			return CreationMgr.GetCreation(objId);
		}
		else
			return null;
	}

	public static string GetCreationPath(int objId)
	{
		CreationOriginData cData = CreationList.Find( iter=> iter.ObjectID == objId );
		if ( cData != null)
		{
			string HashString = cData.HashCode.ToString("X").PadLeft(16, '0');
			string fn1 = VCConfig.s_CreationPath + HashString + VCConfig.s_CreationFileExt;
			string fn2 = VCConfig.s_CreationNetCachePath + HashString + VCConfig.s_CreationNetCacheFileExt;
			
			string fn = "";
			if ( File.Exists(fn1) )
				fn = fn1;
			else if ( File.Exists(fn2) )
                fn = fn2;
            if ( fn.Length == 0 )
                return null;
			return fn;
        }
        else
            return null;
	}

	public static VCIsoHeadData GetCreateionHead(int objId)
	{
		string filename = GetCreationPath (objId);
		VCIsoHeadData headData;
		VCIsoData.ExtractHeader (filename, out headData);

        if (ItemProto.Mgr.Instance.Get(objId) != null)
        {
            return headData;
        }

		ItemProto itemData = GenItemData (headData,objId);
		if (itemData != null)
		{
			ItemProto.Mgr.Instance.Add (itemData);
			byte[] buff = headData.IconTex;
			if (null != buff)
			{
				Texture2D iconTex = new Texture2D(2, 2);
				iconTex.LoadImage(buff);
				iconTex.Apply(false, true);				
				itemData.iconTex = iconTex;
			}
		}
        return headData;

    }

	public static CreationAttr GetCreationAttr( string xml )
	{
		CreationAttr attribute = new CreationAttr();

		try
		{
			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml(xml);
			XmlNode attr_node = xmldoc.DocumentElement["ATTR"];
			XmlNode common_node = attr_node["COMMON"];
			XmlNode prop_node = attr_node["PROP"];
			attribute.m_Type = (ECreation)(XmlConvert.ToInt32(common_node.Attributes["type"].Value));
			attribute.m_Volume = XmlConvert.ToSingle(common_node.Attributes["vol"].Value);
			attribute.m_Weight = XmlConvert.ToSingle(common_node.Attributes["weight"].Value);
			attribute.m_Durability = XmlConvert.ToSingle(common_node.Attributes["dur"].Value);
			attribute.m_SellPrice = XmlConvert.ToSingle(common_node.Attributes["sp"].Value);
			attribute.m_Attack = XmlConvert.ToSingle(prop_node.Attributes["atk"].Value);
			attribute.m_Defense = XmlConvert.ToSingle(prop_node.Attributes["def"].Value);
			attribute.m_MuzzleAtkInc = XmlConvert.ToSingle(prop_node.Attributes["inc"].Value);
			attribute.m_FireSpeed = XmlConvert.ToSingle(prop_node.Attributes["fs"].Value);
			attribute.m_Accuracy = XmlConvert.ToSingle(prop_node.Attributes["acc"].Value);
			attribute.m_DragCoef = XmlConvert.ToSingle(prop_node.Attributes["dc"].Value);
			attribute.m_MaxFuel = XmlConvert.ToSingle(prop_node.Attributes["fuel"].Value);
			foreach (XmlNode node in common_node.ChildNodes)
			{
				int id = XmlConvert.ToInt32(node.Attributes["id"].Value);
				int cnt = XmlConvert.ToInt32(node.Attributes["cnt"].Value);
				attribute.m_Cost.Add(id, cnt);
			}
			return attribute;
		}
		catch (Exception e)
		{
			LogManager.Warning(e);
			attribute.m_Errors.Add("error");
			return attribute;
		}
	}
	private static ItemProto GenItemData (VCIsoHeadData headData,int objId)
    {
		CreationAttr attribute = GetCreationAttr (headData.Remarks);
		// 0.9 id修改 转换检测
		attribute.CheckCostId();
		ItemProto item =  CreationData.StaticGenItemData(objId,headData,attribute);	
        return item;
    }
    
    
    public static void AddCreation(CreationOriginData data)
    {
        if(!CreationList.Contains(data) && data != null)
            CreationList.Add(data);
	}

    public static ulong GetFileHandle(ulong hashCode)
    {
        foreach(var iter in _DownloadList)
        {
            if (iter._hashCode == hashCode)
                return iter.UGCHandle;
        }
        return 0;
    }
    #endregion
    #region system
    void Awake()
	{
#if SteamVersion
		_instance = this;
#else
		Destroy(gameObject);
#endif
	}
	void Update()
	{
		if(GameConfig.IsMultiMode)
			CreationMgr.CheckForDel();
	}
	void Start()
	{
		LoadSteamItems();

		CreationMgr.Init();

		_CurDownload = 0;
		_DownloadList.Clear();
		StartCoroutine(DownLoadCoroutine());

		_SteamPersonaName = "$sinwa$"+SteamFriends.GetPersonaName ();
		SteamFriends.SetListenForFriendsMessages (true);
	}

	public void OnDisconnectEvent(object sender, EventArgs e)
	{
		_CurDownload = 0;
		_DownloadList.Clear();
		_FileSenderMgr.Clear();
	}
    #endregion
    #region sendfile
    internal static void OnFileSharedEvent(object sender, EventArgs args)
	{
		SteamFileItem item = (SteamFileItem)sender;
		if (!item._SendToServer)
			return;
        if (null != PlayerNetwork.mainPlayer)
        {
            VCIsoData da = new VCIsoData();
            da.Import(item._Data, new VCIsoOption(false));
            var components = from component in da.m_Components
                             where VCUtils.IsSeat(component.m_Type)
                             select (int)component.m_Type;
            if (components.Count() > 0)
            {
                NetworkManager.SyncServer(EPacketType.PT_Common_WorkshopShared, item.FileID, item.RealFileName, item.HashCode, item._free, item.instanceId,true, components.ToArray());
            }
            else
            {
                NetworkManager.SyncServer(EPacketType.PT_Common_WorkshopShared, item.FileID, item.RealFileName, item.HashCode, item._free, item.instanceId,false);
            }
        }
            

		string fielName = VCConfig.s_CreationNetCachePath + item.HashCode.ToString("X").PadLeft(16, '0') + VCConfig.s_CreationNetCacheFileExt;
		using (FileStream fs = new FileStream(fielName, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			fs.Write(item._Data, 0, item._Data.Length);
		}

		_SteamItems[item.HashCode] = item.FileName;
	}

	internal static void OnFilePublishEvent(object sender, EventArgs args)
	{
        SteamFileItem item = (SteamFileItem)sender;
        if (item != null)
        {
            UIWorkShopCtrl.PublishFinishCellBack(item.ID, item.PublishID,item.HashCode);
        }
	}

	internal static void LoadSteamItems()
	{
		_SteamItems.Clear();
		string[] fileNames = Directory.GetFiles(VCConfig.s_CreationNetCachePath, "*" + VCConfig.s_CreationNetCacheFileExt);
		foreach (string name in fileNames)
		{
			FileInfo file = new FileInfo(name);
			Stream stream = file.OpenRead();
			ulong hashCode = CRC64.Compute(stream);
			string[] nameSplit = file.Name.Split('.');
			_SteamItems[hashCode] = nameSplit[0];
		}
	}
    
    internal static void SendFile(SteamUploadEventHandler callBackSteamUploadResult,string name, string desc, byte[] preData, byte[] data,string[] tags,bool sendToServer = true,int id = -1,ulong fileId = 0,bool free = false)
	{
        ulong hash = CRC64.Compute(data);
        bool ret = false;
        try
		{            
            if (string.IsNullOrEmpty(name))
			{
				VCEMsgBox.Show(VCEMsgBoxType.EXPORT_EMPTY_NAME);
				LogManager.Error("File name cannot be null.");
                return;
			}
            if(!SteamUser.BLoggedOn())
            {
                LogManager.Error("log back in steam...");
                return;
            }
			
			bool bPublish = !sendToServer;
			if (SteamRemoteStorage.FileExists(hash.ToString()+"_preview"))
			{//file exist,don't publish it;

			}

			if (!SteamRemoteStorage.IsCloudEnabledForAccount())
				throw new Exception("Account cloud disabled.");
			
			if (!SteamRemoteStorage.IsCloudEnabledForApp())
				throw new Exception("App cloud disabled.");
            if(!bPublish)
            {
                SteamFileItem item = new SteamFileItem(callBackSteamUploadResult, name, desc, preData, data, hash, tags, bPublish, sendToServer, id, fileId,free);
                item.StartSend();
            }
			else
            {
                SendIsoCache iso = new SendIsoCache();
                iso.id = id;
                iso.hash = hash;
                iso.name = name;
                iso.preData = preData;
                iso.sendToServer = sendToServer;
                iso.tags = tags;
                iso.data = data;
                iso.desc = desc;
                iso.callBackSteamUploadResult = callBackSteamUploadResult;
                iso.bPublish = bPublish;
                if (AddToIsoCache(iso))
                {
					LobbyInterface.LobbyRPC(ELobbyMsgType.UploadISO, hash, SteamMgr.steamId.m_SteamID);
                }
                else
                    return;
            }

			VCEMsgBox.Show(VCEMsgBoxType.EXPORT_NETWORK);
            ret = true;
        }
		catch (Exception e)
		{
			VCEMsgBox.Show(VCEMsgBoxType.EXPORT_NETWORK_FAILED);
			Debug.LogWarning("workshop error :" + e.Message);
            ToolTipsMgr.ShowText(e.Message);
        }
        finally
        {
            if(!ret && callBackSteamUploadResult != null)
                callBackSteamUploadResult(id, false, hash);
        }
	}
    #endregion
    #region download
    IEnumerator DownLoadCoroutine()
	{
		int lastDownloadBytes = 0;

		while (true)
		{
			if (_DownloadList.Count >= 1 && NetworkInterface.IsClient)
			{
				string isoName = _DownloadList[0]._isoName;
				if (0 == _CurDownload)
				{
					_CurDownload = _DownloadList[0].UGCHandle;
                    DownloadUGC();
					lastDownloadBytes = 0;
				}
				else
				{
					int downloadedBytes = 0;
					int totalBytes = 0;

					if (SteamRemoteStorage.GetUGCDownloadProgress(new UGCHandle_t(_CurDownload), out downloadedBytes, out totalBytes))
					{
						if (0 != totalBytes && downloadedBytes != lastDownloadBytes)
						{
							int speed = downloadedBytes - lastDownloadBytes;
							lastDownloadBytes = downloadedBytes;
							speed /= 1024;

							RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, speed);
							RoomGui_N.SetMapInfo("Downloading " + isoName + "...[" + downloadedBytes * 100 / totalBytes + "%]");
						}
					}
					else
					{
//						string skey = "";
						foreach(KeyValuePair<string, FileSender>  iter in _FileSenderMgr)
						{
							if(iter.Value.m_FileHandle == _CurDownload && iter.Value.m_FileSize > 0)
							{
								int speed = iter.Value.m_Sended - lastDownloadBytes;
								lastDownloadBytes = iter.Value.m_Sended;
								speed /= 1024;
								totalBytes = iter.Value.m_FileSize;
								RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, speed);
								RoomGui_N.SetMapInfo("Downloading " + isoName + "...[" + downloadedBytes * 100 / totalBytes + "%]");
								break;
							}
						}
					}
				}
			}

			yield return new WaitForSeconds(0.02f);
		}
	}

	internal static void GetFiles()
	{
		int fileCount = SteamRemoteStorage.GetFileCount();
		Debug.Log("File count : " + fileCount);

		for (int i = 0; i < fileCount; i++)
		{
			int fileSize;
			/*string fileName = */SteamRemoteStorage.GetFileNameAndSize(i, out fileSize);
		}
	}

	internal static void DeleteFiles()
	{
		while (SteamRemoteStorage.GetFileCount() > 0)
		{
			int fileSize;
			string fileName = SteamRemoteStorage.GetFileNameAndSize(0, out fileSize);
            SteamRemoteStorage.FileDelete(fileName);
		}
	}

	internal static void DownloadUGC()
	{
		try
		{
			RegisteredISO iso = _DownloadList.Find(iter => iter.UGCHandle == _CurDownload);
			if (null == iso)
			{
				_DownloadList.RemoveAll(iter => iter.UGCHandle == _CurDownload);
				_CurDownload = 0;
				return;
			}

			if (!_SteamItems.ContainsKey(iso._hashCode))
			{
                //if (_CurDownloadList.Contains(_CurDownload))
                //    return;
				UGCHandle_t handler;
				handler.m_UGCHandle = _CurDownload;

				SteamAPICall_t caller = SteamRemoteStorage.UGCDownload(handler, 0);
				//new CallResult<RemoteStorageDownloadUGCResult_t>(OnSteamUGCDownloadResult, caller);
				RemoteStorageDownloadPreUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnSteamUGCDownloadResult);
				RemoteStorageDownloadPreUGCResult.Set(caller);
				RoomGui_N.SetMapInfo("Downloading " + iso._isoName + "...[0%]");
			}
			else
			{
				_DownloadList.RemoveAll(iter => iter.UGCHandle == _CurDownload);
				NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, _CurDownload);
				_CurDownload = 0;

				if (_DownloadList.Count <= 0)
				{
					RoomGui_N.SetMapInfo("Download complete");
					if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
						RoomGui_N.Instance.ActiveStartBtn();
				}
				else
				{
					RoomGui_N.SetMapInfo("Downloading " + iso._isoName + "...[100%]");
				}
			}

			RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
		}
		catch (Exception e)
		{
			LogManager.Warning(e.Message);
		}
	}

	static void OnSteamUGCDownloadResult(RemoteStorageDownloadUGCResult_t pCallback, bool bIOFailure)
	{
		Debug.Log ("[SteamUGCDownloadResult] -- " + pCallback.m_eResult + " -- Name:" + pCallback.m_pchFileName + " -- Size:" + pCallback.m_nSizeInBytes);
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			RegisteredISO iso = _DownloadList.Find(iter => iter.UGCHandle == pCallback.m_hFile.m_UGCHandle);
			if (iso != null)
			{
				string fielName = VCConfig.s_CreationNetCachePath + iso._hashCode.ToString("X").PadLeft(16, '0') + VCConfig.s_CreationNetCacheFileExt;
				byte[] fileData = new byte[pCallback.m_nSizeInBytes];
				int length = SteamRemoteStorage.UGCRead(pCallback.m_hFile, fileData, pCallback.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_Close);

				using (FileStream fs = new FileStream(fielName, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					fs.Write(fileData, 0, length);
				}

				_DownloadList.RemoveAll(iter => iter.UGCHandle == pCallback.m_hFile.m_UGCHandle);
				_SteamItems[iso._hashCode] = pCallback.m_pchFileName;
			}

            if(BaseNetwork.MainPlayer.NetworkState ==  ENetworkState.Null)
            {
                NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, pCallback.m_hFile.m_UGCHandle);
            }
            else
            {
                for (int i = 0; i < _CurDownloadList.Count; i++)
                {
                    if (_CurDownloadList[i] == pCallback.m_hFile.m_UGCHandle)
                    {
                        NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, pCallback.m_hFile.m_UGCHandle);
                        _CurDownloadList[i] = 0;
                    }
                }
            }
            
            _CurDownload = 0;

			if (_DownloadList.Count <= 0)
			{
				RoomGui_N.SetMapInfo("Download complete");
				if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
					RoomGui_N.Instance.ActiveStartBtn();
			}
			else
			{
				RoomGui_N.SetMapInfo("Downloading " + pCallback.m_pchFileName + "...[100%]");
			}

			RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
		}
		else if (pCallback.m_eResult == EResult.k_EResultFileNotFound)
		{
			Debug.LogWarning(pCallback.m_hFile.m_UGCHandle + " does not exits.");
            //_DownloadList.RemoveAll(iter => iter.UGCHandle == pCallback.m_hFile.m_UGCHandle);
            //_CurDownload = 0;
            //if (_DownloadList.Count <= 0)
            //{
            //    RoomGui_N.SetMapInfo("Download complete");
            //    if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
            //        RoomGui_N.Instance.ActiveStartBtn();
            //}
            //RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
            NetworkManager.SyncServer(EPacketType.PT_Common_InvalidUGC, _DownloadList[0]._hashCode,pCallback.m_hFile.m_UGCHandle);
        }
		else if(pCallback.m_eResult == EResult.k_EResultTimeout)
		{
			NetworkManager.SyncServer(EPacketType.PT_Common_DownTimeOut, _DownloadList[0]._hashCode,pCallback.m_hFile.m_UGCHandle);
		}
		else
		{
			RoomGui_N.SetMapInfo("Download failed");
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000116), OnYesEvent, OnNoEvent);
		}
	}

	static void OnYesEvent()
	{
		_CurDownload = 0;
	}

	static void OnNoEvent()
	{
		_CurDownload = 0;
		_DownloadList.Clear();
		PeSceneCtrl.Instance.GotoLobbyScene();
    }
    #endregion
   
    #region RPCFUNS


    public static void RPC_S2C_ResponseUGC(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int count = stream.Read<int>();
		if (count <= 0)
		{
			if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
				RoomGui_N.Instance.ActiveStartBtn();
		}
		else
		{
			RegisteredISO[] isos = stream.Read<RegisteredISO[]>();
			foreach (RegisteredISO iso in isos)
			{
				if (_SteamItems.ContainsKey(iso._hashCode))
					_SteamItems[iso._hashCode] = iso._isoName;
			}

			RoomGui_N.SetDownLoadInfo(isos.Length);

			_DownloadList.Clear();
            _CurDownloadList.Clear();
            _DownloadList.AddRange(isos);
            //foreach(var iter in _DownloadList)
            //{
            //    _CurDownloadList.Add(iter.UGCHandle);
            //}
		}
	}

	public static void RPC_S2C_WorkShopShared(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RegisteredISO iso = stream.Read<RegisteredISO>();
		if (_SteamItems.ContainsKey(iso._hashCode))
			_SteamItems[iso._hashCode] = iso._isoName;
        _DownloadList.Add(iso);
        _CurDownloadList.Add(iso.UGCHandle);

    }

	public static void RPC_S2C_UGCGenerateList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		CreationOriginData[] creations = stream.Read<CreationOriginData[]>();
        foreach (CreationOriginData iter in creations) {
			SteamWorkShop.AddCreation (iter);
		}
	}


	public static void RPC_S2C_RequestUGCData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] objIDs = stream.Read<int[]>();
        foreach (int id in objIDs)
		{
			CreationData data = CreationMgr.GetCreation(id);
			if (null == data)
			{

				CreationOriginData cData = CreationList.Find( iter=> iter.ObjectID == id );
				if ( cData != null)
					CreationMgr.NewCreation(cData.ObjectID, cData.HashCode, (float)cData.Seed);
				else
				{
					MessageBox_N.ShowOkBox(PELocalization.GetString(8000500));
					LogManager.Error("Creation item create failed. ID:" + id);
					GameClientNetwork.Disconnect();
				}

				return;
			}
			
			ItemProto item = ItemProto.GetItemData(id);
			if (null != item)
			{
				byte[] itemData = ItemProto.GetBuffer(item);
				NetworkManager.SyncServer(EPacketType.PT_Common_UGCItem, id, itemData);				
	
				var components = from component in data.m_IsoData.m_Components
					where VCUtils.IsSeat(component.m_Type)
						select (int)component.m_Type;

				float hp = data.m_Attribute.m_Durability;
				float energy = data.m_Attribute.m_MaxFuel; 
				
				if (components.Count() >= 1)
				{
					NetworkManager.SyncServer(EPacketType.PT_Common_UGCData, id, hp, energy,
					    data.m_Attribute.m_Cost.Keys.ToArray(), data.m_Attribute.m_Cost.Values.ToArray(), true,
					    components.ToArray());
				}
				else
				{
					NetworkManager.SyncServer(EPacketType.PT_Common_UGCData, id, hp, energy,
					    data.m_Attribute.m_Cost.Keys.ToArray(), data.m_Attribute.m_Cost.Values.ToArray(), false);
				}
			}
		}
	}
	public static void RPC_S2C_DownTimeOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*ulong fileHandle = */stream.Read<ulong> ();
		RoomGui_N.SetMapInfo("Download failed");
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000116), OnYesEvent, OnNoEvent);
	}
	public static void RPC_S2C_FileDontExists(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		ulong fileHandle = stream.Read<ulong> ();
		_DownloadList.RemoveAll(iter => iter.UGCHandle == fileHandle);
		_CurDownload = 0;
		if (_DownloadList.Count <= 0)
		{
			RoomGui_N.SetMapInfo("Download complete");
			if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
				RoomGui_N.Instance.ActiveStartBtn();
		}
		RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
		
	}

	public static void RPC_S2C_SendFile(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		byte [] data = stream.Read<byte[]> ();
		int count = stream.Read<int> ();
		string skey = stream.Read<string> ();

		if (data != null && count != 0) 
		{
			if(_FileSenderMgr.ContainsKey(skey) && _FileSenderMgr[skey] != null)
			{

				bool bFinish = false;
				_FileSenderMgr[skey].WriteData(data,count,ref bFinish);
				if(bFinish)
				{
					RegisteredISO iso = _DownloadList.Find(iter => iter.UGCHandle == _FileSenderMgr[skey].m_FileHandle);
					if (iso != null)
					{
						string fielName = VCConfig.s_CreationNetCachePath + _FileSenderMgr[skey].m_FileName+ VCConfig.s_CreationNetCacheFileExt;
						using (FileStream fs = new FileStream(fielName, FileMode.Create, FileAccess.Write, FileShare.Read))
						{
							fs.Write( _FileSenderMgr[skey].m_Data,0, (int)_FileSenderMgr[skey].m_FileSize);
						}
						
						_DownloadList.RemoveAll(iter => iter.UGCHandle == _FileSenderMgr[skey].m_FileHandle);
						Debug.Log(_DownloadList.Count().ToString() + "===========" + _FileSenderMgr[skey].m_FileName);
						_SteamItems[iso._hashCode] = _FileSenderMgr[skey].m_FileName;
					}
					
					NetworkManager.SyncServer(EPacketType.PT_Common_UGCDownloaded, _FileSenderMgr[skey].m_FileHandle);
					_CurDownload = 0;
					_FileSenderMgr.Remove(skey);
                    if (_DownloadList.Count <= 0)
                    {
                        RoomGui_N.SetMapInfo("Download complete");
                        if (null != RoomGui_N.Instance && RoomGui_N.Instance.isShow)
                            RoomGui_N.Instance.ActiveStartBtn();
                    }
                    else
                    {
						RoomGui_N.SetMapInfo("Downloading " + _FileSenderMgr[skey].m_FileName + "...[100%]");
                    }
					
					RoomGui_N.UpdateDownLoadInfo(_DownloadList.Count, 0);
				}
				else
				{
					NetworkManager.SyncServer(EPacketType.PT_Common_UGCUpload, skey,count,true);
				}
			}
		}
    }

	public static void RPC_S2C_StartSendFile(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		string fileName = stream.Read<string> ();
		string skey = stream.Read<string> ();
		int fileSize = stream.Read<int> ();
		ulong fileHandle = stream.Read<ulong> ();

		/*FileSender fileSender = */new FileSender(fileName,_FileSenderMgr,fileHandle,fileSize,skey);

	}
    #endregion
    #region randgetisos
    public  void GetRandIsos(int dungeonId,int amount,string tag)
    {
        DungeonIsos.AddItem(amount, dungeonId);
		SteamProcessMgr.Instance.RandomGetIsosFromWorkShop(GetRandomIsoListCallBackEventHandler, amount, dungeonId,tag);
    }
    void GetRandomIsoListCallBackEventHandler(List<ulong> fileIDsList,List<ulong> publishIds, int dungeonId,bool bOK)
    {
        if (bOK)
        {
            if(Pathea.PeGameMgr.IsSingle)
            {
                if (DungeonIsos.AddIsos(dungeonId, fileIDsList.ToArray(), publishIds.ToArray()))
                {
                    for (int i = 0; i < fileIDsList.Count; i++)
                    {
                        SteamProcessMgr.Instance.GetPrimaryFile(SteamWorkShop.Instance.GetPrimaryFileResultEventHandler, new UGCHandle_t(fileIDsList[i]), new PublishedFileId_t(publishIds[i]), i, dungeonId);
                    }
                }
            }
            else
                NetworkManager.SyncServer(EPacketType.PT_Common_SendRandIsoFileIds, dungeonId,fileIDsList.ToArray(), publishIds.ToArray());
        }
    }
    public static void RPC_S2C_GetRandIsoFileIds(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int dungeonId = stream.Read<int>();
		int amount = stream.Read<int>();
		string isoTag = stream.Read<string>();
		SteamWorkShop.Instance.GetRandIsos(dungeonId,amount,isoTag);
    }    
    public static void RPC_S2C_SendRandIsoFileIds(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int dungeonId = stream.Read<int>();
        ulong[] isos = stream.Read<ulong[]>();
        ulong[] publishs = stream.Read<ulong[]>();
        for (int i = 0; i < isos.Length; i++)
        {
            SteamProcessMgr.Instance.GetPrimaryFile(SteamWorkShop.Instance.GetPrimaryFileResultEventHandler, new UGCHandle_t(isos[i]), new PublishedFileId_t(publishs[i]), i, dungeonId);

        }
        DungeonIsos.AddIsos(dungeonId, isos, publishs);
             
    }
    public static void RPC_S2C_ExportRandIso(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int dungeonid = stream.Read<int>();
        int index = stream.Read<int>();
        string fullpath = stream.Read<string>();        
        ExportRandIso(fullpath, dungeonid, index);
    }
   
    public void GetPrimaryFileResultEventHandler(byte[] fileData, PublishedFileId_t publishedFileId, bool bOK,int index,int dungeonId)
    {
        string fullpath = UIWorkShopCtrl.DownloadFileCallBack(fileData, publishedFileId,bOK);
        if (fullpath.Length > 0)
        {
            if (Pathea.PeGameMgr.IsSingle)
            {
                ExportRandIso(fullpath, dungeonId, index);
            }
            else
                NetworkManager.SyncServer(EPacketType.PT_Common_ExportRandIso, dungeonId, index, fullpath);
        }
    }
    public static void ExportRandIso(string fullpath,int dungeonId, int index)
    {
        if (fullpath == "")
            return;

        if (!File.Exists(fullpath))
        {
            return;
        }
        ulong fileId = DungeonIsos.GetFileId(dungeonId, index);
        VCIsoData iso = new VCIsoData();
        using (FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read))
        {
            byte[] iso_buffer = new byte[(int)(fs.Length)];
            fs.Read(iso_buffer, 0, (int)(fs.Length));
            fs.Close();
            iso.Import(iso_buffer, new VCIsoOption(true));

            if(Pathea.PeGameMgr.IsMulti)
            {
                ulong hash = CRC64.Compute(iso_buffer);
                VCGameMediator.SendIsoDataToServer(iso.m_HeadInfo.Name, iso.m_HeadInfo.SteamDesc,
                                               iso.m_HeadInfo.SteamPreview, iso_buffer, SteamWorkShop.AddNewVersionTag(iso.m_HeadInfo.ScenePaths()),true, fileId,true);
                NetworkManager.SyncServer(EPacketType.PT_Common_SendRandIsoHash, dungeonId, index, hash);
            }
            else
            {
                CreationData new_creation = new CreationData();
                new_creation.m_ObjectID = CreationMgr.QueryNewId();
                new_creation.m_RandomSeed = UnityEngine.Random.value;
                new_creation.m_Resource = iso_buffer;
                new_creation.ReadRes();
                ulong hash = CRC64.Compute(iso_buffer);
                // Attr
                new_creation.GenCreationAttr();
                if (new_creation.m_Attribute.m_Type == ECreation.Null)
                {
                    Debug.LogWarning("Creation is not a valid type !");
                    new_creation.Destroy();
                    return;
                }

                // SaveRes
                if (new_creation.SaveRes())
                {
                    new_creation.BuildPrefab();
                    new_creation.Register();
                    CreationMgr.AddCreation(new_creation);
                    ItemObject item;
                    new_creation.SendToPlayer(out item,false);

                    Debug.Log("Make creation succeed !");
                    RandomDungenMgr.Instance.ReceiveIsoObj(dungeonId,hash, item.instanceId);
                }
                else
                {
                    Debug.LogWarning("Save creation resource file failed !");
                    new_creation.Destroy();
                    return;
                }
            }
        }       
    }
    #endregion
    public static Dictionary<string,FileSender>_FileSenderMgr = new Dictionary<string,FileSender>();
}


public class FileSender
{
	public ulong m_FileHandle;
	public int m_FileSize;
	public byte[] m_Data;
	public string m_FileName;
	public int 	 m_Sended;
	public string 	  m_IndexInList;
	public FileSender(string fileName,Dictionary<string,FileSender> fileSenderList,ulong fileHandle,int fileSize,string skey)
	{
		m_FileName = fileName;
		m_FileSize = fileSize;
		m_Sended = 0;
		m_FileHandle = fileHandle;
		fileSenderList [skey] = this;
		m_IndexInList = skey;
		m_Data = new byte[fileSize];
	}
	public void WriteData(byte[] data,int count,ref bool bFinish)
	{
		Array.Copy (data, 0, m_Data, m_Sended, count);
		m_Sended += count; 
		if( m_Data.Length ==  m_Sended)
		{
			bFinish = true;
		}
		else
		{
			bFinish = false;
		}

	}
}