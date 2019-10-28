using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace CustomCharactor
{
    public class CustomDataMgr
    {
		const int MaxItemCount = 100;

        static CustomDataMgr instance;
        public static CustomDataMgr Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CustomDataMgr();
                }
                return instance;
            }
        }

        string GetFilePath(int index)
        {
            return Path.Combine(DirPath, "Custom_"+index+".dat");
        }

        string DirPath
        {
            get
            {
                //return Path.Combine(Directory.GetCurrentDirectory(), "CustomCharacter");
                return Path.Combine(GameConfig.GetUserDataPath(), "PlanetExplorers/CustomCharacter");
            }
        }

        void CheckDir()
        {
            if (!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
        }

        CustomDataMgr()
		{ 
			mCustomDataList = new List<CustomData>();
			mFilePathList = new List<string> ();
		}

        public CustomData Current
        {
            get;
            set;
        }

		private List<CustomData> mCustomDataList;
		private List<string>	mFilePathList;

        public List<CustomData> CustomDataList { get { return mCustomDataList; } }

		public CustomData GetCustomData(int index)
		{
			if (index >= dataCount)
				return null;

			CustomData data = new CustomData();
			byte[] buf = mCustomDataList[index].Serialize();
			data.Deserialize(buf);
			return data;
		}

		public Texture2D GetDataHeadIco(int index)
		{
			if (index >= dataCount)
				return null;
			return mCustomDataList[index].headIcon;
		}

		public int dataCount
		{
			get
			{
				return mCustomDataList.Count;
			}
		}

		public void LoadAllData()
		{
			mCustomDataList.Clear();
			mFilePathList.Clear();
			CheckDir();
			string[] paths = Directory.GetFiles(DirPath);
			foreach (string path in paths)
			{
				CustomData data = LoadData(path);
				if (data != null)
				{
					mCustomDataList.Add(data);
					mFilePathList.Add(path);
				}
			}
		}


		public void DeleteData(int index)
		{
			if (index >=  dataCount)
				return ; 

			if (!File.Exists(mFilePathList[index]))
				return;
			File.Delete(mFilePathList[index]);
			mFilePathList.RemoveAt(index);
			mCustomDataList.RemoveAt(index);
		}

		public bool SaveData(CustomData appearData)
		{
			int index = dataCount;
			string path;
			do
			{
				index ++ ;
				path = GetFilePath(index);
			}while(File.Exists(path));

			return SaveData(index,appearData);
		}


        bool SaveData(int index, CustomData appearData)
        {
            if (null == appearData)
            {
                return false;
            }

            CheckDir();

            using (FileStream fs = new FileStream(GetFilePath(index), FileMode.Create, FileAccess.Write))
            {
                byte[] buff = appearData.Serialize();
                fs.Write(buff, 0, buff.Length);
            }

			mCustomDataList.Add(appearData);
			mFilePathList.Add(GetFilePath(index));
            return true;
        }



        CustomData LoadData(string path)
        {
			if (!File.Exists(path))
			{
				return null;
			}
			try
			{
	            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
	            {
	                if (fs.Length > 0)
	                {
	                    byte[] buff = new byte[fs.Length];
	                    fs.Read(buff, 0, buff.Length);

	                    CustomData customData = new CustomData();
	                    customData.Deserialize(buff);

	                    return customData;
	                }
	            }
			}
			catch
			{
				return null;
			}

            return null;
        }
    }
}