using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

//class WndFormPostion
//{
//	public string mWndName;
//	public int mVector_x;
//	public int mVector_y;
//	public int mVector_z;
//}
/*
public class UILocalPostionMgr : MonoBehaviour
{

	static UILocalPostionMgr mInstance = null;
	public static UILocalPostionMgr Instance{ get { return mInstance; } }
	public List<GUIWindowBase> mWndList;

	private Dictionary<string,WndFormPostion>  mWndPosMap;

	bool isInit =false;
	private List<WndFormPostion> mWndPosList;

	void Awake()
	{
		mInstance = this;
		mWndPosList = new List<WndFormPostion>(); 
		mWndPosMap = new Dictionary<string,WndFormPostion>();
	}
	// Use this for initialization
	void Start () 
	{
		UpdateUIPostion();
		isInit = true;
	}





	public void UpdateUIPostion()
	{
		bool ok = LoadConfigFile();
		if (ok)
		{
			for (int i=0;i<mWndList.Count;i++)
			{
				if (mWndList[i] == null)
					continue;

				string WndObjName = mWndList[i].gameObject.name;
				if ( mWndPosMap.ContainsKey(WndObjName) )
				{
					Vector3 wndPos = new Vector3(mWndPosMap[WndObjName].mVector_x,mWndPosMap[WndObjName].mVector_y,mWndPosMap[WndObjName].mVector_z);
					mWndList[i].gameObject.transform.localPosition = wndPos;
				}
			}
		}
	}

	public void SaveUIPostion()
	{
		if (!isInit)
			return;

		for (int i=0;i<mWndList.Count;i++)
		{
			if (mWndList[i] == null)
				continue;

			string WndObjName = mWndList[i].gameObject.name;
			if ( mWndPosMap.ContainsKey(WndObjName) )
			{
				mWndPosMap[WndObjName].mVector_x = Convert.ToInt32(mWndList[i].gameObject.transform.localPosition.x);
				mWndPosMap[WndObjName].mVector_y = Convert.ToInt32(mWndList[i].gameObject.transform.localPosition.y);
				mWndPosMap[WndObjName].mVector_z = Convert.ToInt32(mWndList[i].gameObject.transform.localPosition.z);
			}
			else
			{

				WndFormPostion wndPos = new WndFormPostion();
				wndPos.mWndName =  mWndList[i].gameObject.name;
				wndPos.mVector_x = Convert.ToInt32(mWndList[i].gameObject.transform.localPosition.x);
				wndPos.mVector_y = Convert.ToInt32(mWndList[i].gameObject.transform.localPosition.y);
				wndPos.mVector_z = Convert.ToInt32(mWndList[i].gameObject.transform.localPosition.z);
				mWndPosMap[wndPos.mWndName] = wndPos;
				mWndPosList.Add(wndPos);
			}
		}

		SaveConFigFile();
	}



	bool LoadConfigFile()
	{
		string FilePath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir + "/";
		if (!Directory.Exists(FilePath))
			Directory.CreateDirectory(FilePath);
		FilePath +=  "UIRencent.urds";
		
		if(!File.Exists(FilePath))
			return false;
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
			Debug.LogError( "Load UIRencent Error:  " + e.ToString() );
			return false;
		}
	}







	void SaveConFigFile()
	{
		string FilePath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir + "/";
		if (!Directory.Exists(FilePath))
			Directory.CreateDirectory(FilePath);
		FilePath +=  "UIRencent.urds";
		
		try
		{
			using(FileStream fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter bw = new BinaryWriter(fileStream);
				SaveData(bw);
				bw.Close();
				fileStream.Close();
			}
			
			return;
		}
		catch  ( Exception e )
		{
			Debug.LogError( "Save UIRencent Error:  " + e.ToString() );
			return;
		}

	}



	void SaveData(BinaryWriter bw)
	{
		bw.Write(GetGameVersion());
		int count = mWndPosList.Count;
		bw.Write(count);
		for (int i=0;i<count;i++)
		{
			bw.Write(mWndPosList[i].mWndName);
			bw.Write(mWndPosList[i].mVector_x);
			bw.Write(mWndPosList[i].mVector_y);
			bw.Write(mWndPosList[i].mVector_z);
		}
	}
	
	void ReadData(BinaryReader br)
	{

		string strVecsion = br.ReadString();
		mWndPosList.Clear();
		if (GetGameVersion() != strVecsion || strVecsion.Length == 0)
		{
			return;
		}
		int count = br.ReadInt32();
		for (int i=0;i<count;i++)
		{	
			WndFormPostion wndPos = new WndFormPostion();
			wndPos.mWndName = br.ReadString();
			wndPos.mVector_x = br.ReadInt32();
			wndPos.mVector_y = br.ReadInt32();
			wndPos.mVector_z = br.ReadInt32();
			mWndPosMap[wndPos.mWndName] = wndPos;
			mWndPosList.Add(wndPos);
		}
	}


	string GetGameVersion()
	{
		return GameConfig.GameVersion;
	}
}*/
