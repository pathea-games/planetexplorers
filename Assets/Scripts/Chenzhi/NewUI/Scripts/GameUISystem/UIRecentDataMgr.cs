using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class UIRecentDataMgr : MonoBehaviour
{
    static UIRecentDataMgr mInstance = null;
    public static UIRecentDataMgr Instance { get { return mInstance; } }
    private Dictionary<string, int> mIntMap;
    private Dictionary<string, float> mFloatMap;
    private Dictionary<string, string> mStringMap;
    private Dictionary<string, Vector3> mVector3Map;

    // stream
    FileStream mFileStream;
    void Awake()
    {
        mInstance = this;
        mIntMap = new Dictionary<string, int>();
        mFloatMap = new Dictionary<string, float>();
        mStringMap = new Dictionary<string, string>();
        mVector3Map = new Dictionary<string, Vector3>();

        OpenFile();

        Load();
    }

    void OnDestroy()
    {
		try{
        UIStateMgr.Instance.SaveUIPostion();
        CloseFile();
		}catch{
		}
    }

    public void SaveUIRecentData()
    {
        Save();
    }

    // get value
    public int GetIntValue(string key, int DefaltValue)
    {
        if (!mIntMap.ContainsKey(key))
            mIntMap[key] = DefaltValue;
        return mIntMap[key];
    }

    public float GetFloatValue(string key, float DefaltValue)
    {
        if (!mFloatMap.ContainsKey(key))
            mFloatMap[key] = DefaltValue;
        return mFloatMap[key];
    }

    public string GetStringValue(string key, string DefaltValue)
    {
        if (!mStringMap.ContainsKey(key))
            mStringMap[key] = DefaltValue;
        return mStringMap[key];
    }

    public Vector3 GetVector3Value(string key, Vector3 DefaltValue)
    {
        if (!mVector3Map.ContainsKey(key))
            mVector3Map[key] = DefaltValue;
        return mVector3Map[key];
    }

    // set value
    public void SetIntValue(string key, int vlaue)
    {
        mIntMap[key] = vlaue;
    }

    public void SetFloatValue(string key, float value)
    {
        mFloatMap[key] = value;
    }

    public void SetStringValue(string key, string value)
    {
        mStringMap[key] = value;
    }

    public void SetVector3Value(string key, Vector3 value)
    {
        mVector3Map[key] = value;
    }

    void OpenFile()
    {
        string FilePath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir + "/";
        if (!Directory.Exists(FilePath))
            Directory.CreateDirectory(FilePath);
        FilePath += "UIRencent.urds";

        if (!File.Exists(FilePath))
            File.Create(FilePath);

        try
        {
            mFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);

        }
        catch
        {
            Debug.LogError("Open UIRencent Error!");
        }
    }

    void CloseFile()
    {
        if (mFileStream != null)
            mFileStream.Close();
    }

    bool Load()
    {
        if (mFileStream == null)
            return false;

        try
        {

            BinaryReader _br = new BinaryReader(mFileStream);
            _br.BaseStream.Seek(0, SeekOrigin.Begin);
            ReadData(_br);

            return true;
        }
        catch
        {
            Debug.LogError("Read UIRecent file faild");
            return false;
        }
    }

    bool Save()
    {
        try
        {
            BinaryWriter bw = new BinaryWriter(mFileStream);
            bw.Seek(0, SeekOrigin.Begin);
            SaveData(bw);
            return true;
        }
        catch
        {
            Debug.LogError("Read UIRecent file faild");
            return false;
        }
    }



    bool LoadConfigFile()
    {
        string FilePath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir + "/";
        if (!Directory.Exists(FilePath))
            Directory.CreateDirectory(FilePath);
        FilePath += "UIRencent.urds";

        if (!File.Exists(FilePath))
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
        catch //( Exception e )
        {
            Debug.LogError("Load UIRencent Error!");
            return false;
        }
    }


    void SaveConFigFile()
    {
        string FilePath = GameConfig.GetUserDataPath() + GameConfig.ConfigDataDir + "/";
        if (!Directory.Exists(FilePath))
            Directory.CreateDirectory(FilePath);
        FilePath += "UIRencent.urds";

        try
        {
            using (FileStream fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            {
                BinaryWriter bw = new BinaryWriter(fileStream);
                SaveData(bw);
                bw.Close();
                fileStream.Close();
            }

            return;
        }
        catch // ( Exception e )
        {
            Debug.LogError("Save UIRencent Error!");
            return;
        }

    }



    void SaveData(BinaryWriter bw)
    {
        bw.Write(GetGameVersion());
        // write int 
        int count = mIntMap.Keys.Count;
        bw.Write(count);
        foreach (string key in mIntMap.Keys)
        {
            bw.Write(key);
            bw.Write(mIntMap[key]);
        }
        // write float
        count = mFloatMap.Keys.Count;
        bw.Write(count);
        foreach (string key in mFloatMap.Keys)
        {
            bw.Write(key);
            bw.Write(mFloatMap[key]);
        }
        // write string 
        count = mStringMap.Keys.Count; ;
        bw.Write(count);
        foreach (string key in mStringMap.Keys)
        {
            bw.Write(key);
            bw.Write(mStringMap[key]);
        }
        // write vector3
        count = mVector3Map.Keys.Count;
        bw.Write(count);
        foreach (string key in mVector3Map.Keys)
        {
            bw.Write(key);
            bw.Write(mVector3Map[key].x);
            bw.Write(mVector3Map[key].y);
            bw.Write(mVector3Map[key].z);
        }
    }

    void ReadData(BinaryReader br)
    {
        string strVecsion = br.ReadString();
        if (GetGameVersion() != strVecsion || strVecsion.Length == 0)
        {
            Debug.LogWarning("The game version is change on load ui recent data!");
        }
        // read int 
        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = br.ReadString();
            mIntMap[key] = br.ReadInt32();
        }
        // read float
        count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = br.ReadString();
            mFloatMap[key] = br.ReadSingle();
        }
        // read string 
        count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = br.ReadString();
            mStringMap[key] = br.ReadString();
        }
        // read vector3
        count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = br.ReadString();
            float v_x = br.ReadSingle();
            float v_y = br.ReadSingle();
            float v_z = br.ReadSingle();
            mVector3Map[key] = new Vector3(v_x, v_y, v_z);
        }
    }


    string GetGameVersion()
    {
		return GameConfig.GameVersion;
    }
}
