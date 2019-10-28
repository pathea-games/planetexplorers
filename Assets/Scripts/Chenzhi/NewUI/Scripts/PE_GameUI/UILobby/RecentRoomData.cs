using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class RecentRoomData  
{
	public long mUID;
	public string mRoomName;
	public string mCreator;
	public string mVersion;

	public RecentRoomData()
	{
		mUID = 0;
		mRoomName = "";
		mCreator = "";
		mVersion = "";
	}
}




public class RecentRoomDataManager
{
	public RecentRoomDataManager(string _roleName)
	{
		if (!String.IsNullOrEmpty (_roleName)) {
			roleFileName = _roleName + ".rds";
			char[] invalidChars = Path.GetInvalidFileNameChars ();
			foreach (char c in invalidChars){
				roleFileName = roleFileName.Replace(c, '_');
			}
		}
	}

	public List<RecentRoomData> mRecentRoomList = new List<RecentRoomData>();
	string roleFileName = String.Empty;


	public void AddItem(long _UID,string _RoomName,string _Creator,string _Version )
	{


		RecentRoomData mServerData = mRecentRoomList.Find(
			delegate(RecentRoomData rd)
			{
			return rd.mUID == _UID;
		});

		if(mServerData == null)
		{

			RecentRoomData data = new RecentRoomData();
			
			data.mUID = _UID;
			data.mRoomName = _RoomName;
			data.mCreator = _Creator;
			data.mVersion = _Version;

			mRecentRoomList.Insert(0,data);
			SaveToFile();
		}
	}

	public void DeleteItem(long _UID)
	{
		RecentRoomData mServerData = mRecentRoomList.Find(
			delegate(RecentRoomData rd)
			{
			return rd.mUID == _UID;
		});
		if(mServerData != null)
		{
			mRecentRoomList.Remove(mServerData);
			SaveToFile();
		}
	}


	public bool SaveToFile()
	{
		if (String.IsNullOrEmpty (roleFileName))
			return false;

		string FilePath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir + "/";
		if (!Directory.Exists(FilePath))
			Directory.CreateDirectory(FilePath);
		FilePath += roleFileName;
		
		try
		{
			using(FileStream fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter bw = new BinaryWriter(fileStream);
				SaveData(bw);
				bw.Close();
				fileStream.Close();
			}			
			return true;
		}
		catch  ( Exception e )
		{
			Debug.LogWarning( "Save RecentData Error:  " + e.ToString() );
			return false;
		}
	}
	
	public bool LoadFromFile()
	{
		if (String.IsNullOrEmpty (roleFileName))
			return false;		
		
		string FilePath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir + "/";
		if (!Directory.Exists(FilePath))
			Directory.CreateDirectory(FilePath);

		FilePath += roleFileName;		
		if(!File.Exists(FilePath))
			return true;
		try
		{
			using (FileStream _fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
			{
				BinaryReader _br = new BinaryReader(_fileStream);
				
				ReadData(_br);
				
				_br.Close();
				_fileStream.Close();
			}
			return true;
		}
		catch ( Exception e )
		{
			Debug.LogWarning( "Load RecentData Error:  " + e.ToString() );
			return false;
		}
	}



	void SaveData(BinaryWriter bw)
	{
		int count = mRecentRoomList.Count;
		bw.Write(count);
		for (int i=0;i<count;i++)
		{
			bw.Write(mRecentRoomList[i].mUID);
			bw.Write(mRecentRoomList[i].mRoomName);
			bw.Write(mRecentRoomList[i].mCreator);
			bw.Write(mRecentRoomList[i].mVersion);
		}
	}

	void ReadData(BinaryReader br)
	{
		mRecentRoomList.Clear();

		int count = br.ReadInt32();
		for (int i=0;i<count;i++)
		{
			RecentRoomData data = new RecentRoomData();

			data.mUID = br.ReadInt64();
			data.mRoomName = br.ReadString();
			data.mCreator = br.ReadString();
			data.mVersion = br.ReadString();

			mRecentRoomList.Add(data);
		}
	}
}