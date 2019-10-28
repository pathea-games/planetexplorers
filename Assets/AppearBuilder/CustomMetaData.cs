//#define PrintBonesName
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using System;
using System.Linq;

namespace CustomCharactor
{
	public class CustomPartInfo
	{
		public string ModelFilePath{ get; private set; }
		public string ModelName{ get; private set; }
		public SkinnedMeshRenderer Smr{ get; private set; }

		public CustomPartInfo(string modelPathName)
		{
			if (!string.IsNullOrEmpty(modelPathName)) {
				string[] partSubstr = modelPathName.Split(':');
				if (partSubstr.Length < 2){
					partSubstr = modelPathName.Split('-');
				}	
				if (partSubstr.Length >= 2) {
					ModelFilePath = partSubstr [0];
					ModelName = partSubstr [1];
					Smr = GetPartSmr();
					return;
				}
			}
			//Debug.LogError ("[CustomPartPathName]error part info string:" + modelPathName);
			ModelFilePath = ModelName = string.Empty;
		}
		SkinnedMeshRenderer GetPartSmr()
		{
			GameObject goModel = AssetsLoader.Instance.LoadPrefabImm(ModelFilePath, true) as GameObject;
			if (null == goModel){
#if UNITY_EDITOR
				//Debug.LogError("[CustomPartPathName]load part error:" + ModelFilePath);
#endif
				return null;
			}
			Transform tPart = goModel.transform.FindChild(ModelName);
			if (null == tPart){
#if UNITY_EDITOR
				//Debug.LogError("[CustomPartPathName]part:" + ModelFilePath + " has no model:" + ModelName);
#endif
				return null;
			}
			return tPart.GetComponent<SkinnedMeshRenderer>();
		}
		public static string GetModelFilePath(string modelPathName)
		{
			if (!string.IsNullOrEmpty(modelPathName)) {
				string[] partSubstr = modelPathName.Split(':');
				if (partSubstr.Length < 2){
					partSubstr = modelPathName.Split('-');
				}	
				if (partSubstr.Length >= 2) {
					return partSubstr [0];
				}
			}
			//Debug.LogError ("[CustomPartPathName]error part info string:" + modelPathName);
			return string.Empty;
		}
	}

    public class CustomMetaData
    {
        public class Hair
        {
			public int Id;
            public string icon;
            public string[] modelPath;
        }
        public class Head
        {
			public int Id;
            public string icon;
            public string modelPath;
        }	
		List<Head> headArray;
		List<Hair> hairArray;
		public string baseModel;
		public string torso;
		public string legs;
		public string hands;
		public string feet;

        static CustomMetaData instanceMale;		
		static CustomMetaData instanceFemale;
		public static CustomMetaData InstanceMale
        {
            get
            {
                if (instanceMale == null)
                {
                    instanceMale = new CustomMetaData();
                    instanceMale.InitMale();
                }

                return instanceMale;
            }
        }
		public static CustomMetaData InstanceFemale
		{
			get
			{
				if (instanceFemale == null)
				{
					instanceFemale = new CustomMetaData();
					instanceFemale.InitFemale();
				}
				
				return instanceFemale;
			}
		}

        void InitMale()
        {
			headArray = new List<Head>();
			hairArray = new List<Hair>();
			foreach( MetaData data in mMetaDataList)
			{
				if (data.mSex == 2) // male
				{
					if (data.mType == 1) // head
					{
						Head head = new Head();
						head.Id = data.mID;
						head.icon = data.mIcon;
						head.modelPath = data.mModelPath;
						headArray.Add(head);
#if PrintBonesName
						string pathName = head.modelPath;
						new CustomPartPathName(pathName).OutputBonesName();
#endif
					}
					else if (data.mType == 2) // hair
					{
						Hair hair = new Hair();
						hair.Id = data.mID;
						hair.icon = data.mIcon;
						hair.modelPath = data.mModelPath.Split(';');
						hairArray.Add(hair);
#if PrintBonesName
						foreach(string pathName in hair.modelPath){
							new CustomPartPathName(pathName).OutputBonesName();
						}
#endif
					}
				}
			}

			baseModel = "Prefab/PlayerPrefab/MaleModel";
            torso = "Model/PlayerModel/Male-torso_0";
			legs = "Model/PlayerModel/Male-legs_0";
            hands = "Model/PlayerModel/Male-hands_0";
            feet = "Model/PlayerModel/Male-feet_0";
        }
        void InitFemale()
        {
			headArray = new List<Head>();
			hairArray = new List<Hair>();
			foreach( MetaData data in mMetaDataList)
			{
				if (data.mSex == 1) // female
				{
					if (data.mType == 1) // head
					{
						Head head = new Head();
						head.Id = data.mID;
						head.icon = data.mIcon;
						head.modelPath = data.mModelPath;
						headArray.Add(head);
					}
					else if (data.mType == 2) // hair
					{
						Hair hair = new Hair();
						hair.Id = data.mID;
						hair.icon = data.mIcon;
						hair.modelPath = data.mModelPath.Split(';');
						hairArray.Add(hair);
					}
				}
			}

			baseModel = "Prefab/PlayerPrefab/FemaleModel";
			torso = "Model/PlayerModel/Female-torso_0";
			legs = "Model/PlayerModel/Female-legs_0";
            hands = "Model/PlayerModel/Female-hands_0";
            feet = "Model/PlayerModel/Female-feet_0";
        }
        public int GetHeadCount()
        {
            return headArray.Count;
        }
        public Head GetHead(int index)
        {
            if (index >= headArray.Count || index < 0)
            {
                return null;
            }

            return headArray[index];
        }
		public int GetHeadIndex(string path)
		{
			for (int i=0;i<headArray.Count;i++)
				if (headArray[i].modelPath == path)
					return i;
			return -1;
		}
        public int GetHairCount()
        {
            return hairArray.Count;
        }
        public Hair GetHair(int index)
        {
			if (index >= hairArray.Count || index < 0)
            {
                return null;
            }

            return hairArray[index];
        }
		public int GetHairIndex(string hairPath_0)
		{
			for (int i=0;i<hairArray.Count;i++)
				if (hairArray[i].modelPath[0] == hairPath_0)
					return i;
			return -1;
		}

		// dataBase
		public class MetaData
		{
			public int mID;
			public int mType;  // enum, 1:head; 2:face
			public int mSex;  // enum , 1:female; 2:male
			public string mIcon;
			public string mModelPath;

			public MetaData(SqliteDataReader reader)
			{
				mID = Convert.ToInt32( reader.GetString(reader.GetOrdinal("ID")) );
				mType = Convert.ToInt32( reader.GetString(reader.GetOrdinal("Type")) );
				mSex = Convert.ToInt32( reader.GetString(reader.GetOrdinal("Sex")) );
				mIcon = reader.GetString(reader.GetOrdinal("Icon"));
				mModelPath = reader.GetString(reader.GetOrdinal("ModelPath"));
			}
		}
		private static List<MetaData> mMetaDataList = new List<MetaData> ();
		// Load DataBase
		public static void LoadData()
		{
			mMetaDataList.Clear();
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("CustomMetaData");
			while (reader.Read())
			{
				MetaData data = new MetaData(reader);
				mMetaDataList.Add(data);
			}
		}
		public static void CachePrefab()
		{
			int n = mMetaDataList.Count;
			for (int i = 0; i < n; i++) {
				string[] strs = mMetaDataList[i].mModelPath.Split('-');
				AssetsLoader.Instance.LoadPrefabImm(strs[0], true);
			}
		}
    }
}