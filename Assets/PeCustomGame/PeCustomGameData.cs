using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using PETools;

namespace Pathea
{
    public class YirdData
    {
        public string name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(mDir);
            }
        }

        Texture2D mTexture;
        public Texture screenshot
        {
            get
            {
                if(mTexture == null)
                {
                    if (!File.Exists(screenshotPath))
                    {
                        return null;
                    }

                    mTexture = new Texture2D(100, 100, TextureFormat.ARGB32, false, true);
                    
                    byte[] data = File.ReadAllBytes(screenshotPath);

                    if (data != null)
                    {
                        mTexture.LoadImage(data);
                        mTexture.Apply(false, true);
                    }
                }

                return mTexture;
            }
        }

        string mDir;

        public YirdData(string dir)
        {
            mDir = dir;
        }

        public string screenshotPath
        {
            get
            {
                return Path.Combine(mDir, "Minimap.png");
            }
        }

        public string projDataPath
        {
            get
            {
                return Path.Combine(mDir, "ProjectSettings.dat");
            }
        }

		public string worldSettingsPath
		{
			get
			{
				return Path.Combine(mDir, "WorldSettings.xml");
			}
		}

        public string terrainPath
        {
            get
            {
                return mDir;
            }
        }

        public string treePath
        {
            get
            {
                return mDir;
            }
        }

        public string grassPath
        {
            get
            {
                return mDir;
            }
        }

        Vector3 mSize = Vector3.zero;

        public Vector3 size
        {
            get
            {
                if (mSize == Vector3.zero)
                {
                    VProjectSettings s = new VProjectSettings();
                    if (s.LoadFromFile(projDataPath))
                    {
                        return s.size;
                    }
                }

                return mSize;
            }
        }
        public Vector3 defaultPlayerPos
        {
            get
            {
                return new Vector3(size.x/2f, 128, size.z/2f);
            }
        }
		
        public IEnumerable<WEItem> GetItems()
        {
            return GetDatas<WEItem>("ITEM");
        }

        public IEnumerable<WEDoodad> GetDoodads()
        {
            return GetDatas<WEDoodad>("DOODAD");
        }

        public IEnumerable<WEMonster> GetMonsters()
        {
            return GetDatas<WEMonster>("MONSTER");
        }

        public IEnumerable<WENPC> GetNpcs()
        {
            return GetDatas<WENPC>("NPC");
        }

		public IEnumerable<WEEffect> GetEffects()
		{
			return GetDatas<WEEffect>("EFFECT");
		}

        IEnumerable<T> GetDatas<T>(string nodeName) where T : VEObject, new()
        {
            XmlNodeList nodeList = GetXmlNodeList(nodeName);
            if(null == nodeList || nodeList.Count == 0)
            {
                return null;
            }

            T[] items = new T[nodeList.Count];
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlElement node = nodeList[i] as XmlElement;
                if (null == node)
                {
                    continue;
                }

                items[i] = new T();

                items[i].Parse(node);
            }
            return items;
        }

        XmlNodeList GetXmlNodeList(string nodeName)
        {
            string path = Path.Combine(mDir, "WorldEntity.xml");

            XmlDocument doc = new XmlDocument();
			try{
            	doc.Load(path);
			} catch(System.Exception ex) {
				GameLog.HandleIOException(ex, GameLog.EIOFileType.InstallFiles);
			}

            XmlNodeList nodeList = doc.SelectNodes(@"WORLDDATA/ENTITIES//" + nodeName);
            return nodeList;
        }
    }


    public class CustomGameData
    {
		public List<string> mWorldNames = new List<string>(1);
		public List<PlayerDesc> mPlayerDescs = new List<PlayerDesc>(1);
		public List<ForceDesc> mForceDescs = new List<ForceDesc>(10);

		List<PlayerDesc> mHumanDescs = new List<PlayerDesc>(1);
		public PlayerDesc[] humanDescs { get { return mHumanDescs.ToArray();} }

		#region Determine Wolrd & Player
		int mWorldIndex;
		int mPlayerIndex;

		public YirdData curYirdData { get { return ( mList.Count <= mWorldIndex ? null : mList[mWorldIndex]); }}
		public PlayerDesc curPlayer
        {
            get
            {
                if (PeGameMgr.IsMulti)
                    return BaseNetwork.curPlayerDesc;

                return ( mPlayerDescs.Count <= mPlayerIndex ? null : mHumanDescs[mPlayerIndex]);
            }
        }
		public int  WorldIndex { get { return mWorldIndex; } set { mWorldIndex = value; } }

		const int _index = 0;

		public bool DeterminePlayer(int index)
		{
			if (mHumanDescs.Count <= index || index < -1)
			{
				return false;
			}

			mPlayerIndex = index;

			mWorldIndex = mHumanDescs[index].StartWorld;
			return true;
		}



		#endregion 

        public string name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(mDir);
            }
        }

        public Texture screenshot
        {
            get
            {
                YirdData yird = defaultYird;
                if (yird != null)
                {
                    return yird.screenshot;
                }

                return null;
            }
        }

        public Vector3 size
        {
            get
            {
                YirdData yird = defaultYird;
                if (yird != null)
                {
                    return yird.size;
                }

                return Vector3.zero;
            }
        }

        public string missionDir
        {
            get
            {
                return Path.Combine(Path.Combine(mDir, s_ScenarioDir), "Missions");
            }
        }

        string mDir;

        List<YirdData> mList = new List<YirdData>(1);

        public YirdData GetYirdData(string yirdName = null)
        {
            if (string.IsNullOrEmpty(yirdName))
            {
                return defaultYird;
            }

            return mList.Find((item) =>
            {
                if (item.name == yirdName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

		public YirdData GetYirdData(int index)
		{
			if (index < 0 || index >= mList.Count)
				return null;

			return mList[index];
		}

        public void ForEach(System.Action<YirdData> action)
        {
            mList.ForEach(action);
        }

        YirdData defaultYird
        {
            get
            {
                if (mList.Count > 0)
                {
                    return mList[0];
                }
                return null;
            }
        }

		static readonly string s_WorldsDir = "Worlds";
		static readonly string s_ScenarioDir = "Scenario";
        public bool Load(string dir)
        {
//            DirectoryInfo dirInfo = new DirectoryInfo(dir);
//
//            if (!dirInfo.Exists)
//            {
//                return false;
//            }
//
//            DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();
//
//            if (subDirInfos == null || subDirInfos.Length == 0)
//            {
//                return false;
//            }
//
//            foreach (DirectoryInfo subDirInfo in subDirInfos)
//            {
//                YirdData data = new YirdData(subDirInfo.FullName);
//
//                 mList.Add(data);
//            }
//
//            mDir = dir;

			string scenarioDir =  Path.Combine(dir, s_ScenarioDir);
			if (!Directory.Exists(scenarioDir))
			{
				return false;
			}

			//-- Load WorldSettings
			string wsfile = Path.Combine(scenarioDir, "WorldSettings.xml");
			if (!File.Exists(wsfile))
			{
				return false;
			}

			// Read world setting file to a string
			string content = "";
			content = Pathea.IO.StringIO.LoadFromFile(wsfile, System.Text.Encoding.UTF8);

			// read the world name
			XmlDocument doc = new XmlDocument();
			StringReader reader = new StringReader(content);
			doc.Load(reader);

			XmlNode root = doc.SelectSingleNode("WORLDSETTINGS");
			XmlNodeList worldList = ((XmlElement)root).GetElementsByTagName("WORLD");

			foreach (XmlNode node in worldList)
			{
				XmlElement xe = node as XmlElement;
				string path = XmlUtil.GetAttributeString(xe, "path");
				mWorldNames.Add(path);
			}

			//-- Load ForceSettings
			string fsfile = Path.Combine(scenarioDir, "ForceSettings.xml");

			if (!File.Exists(fsfile))
			{
				return false;
			}

			content = Pathea.IO.StringIO.LoadFromFile(fsfile, System.Text.Encoding.UTF8);
			ForceSetting.Instance.Load(content);

            mForceDescs.Clear();
            mPlayerDescs.Clear();

            mForceDescs.AddRange(ForceSetting.Instance.m_Forces);
            mPlayerDescs.AddRange(ForceSetting.Instance.m_Players);
            mHumanDescs.AddRange(ForceSetting.Instance.m_Players.FindAll(ret => ret.Type == EPlayerType.Human));

			//-- load world
			if (mWorldNames.Count == 0)
				return false;

			string world_dir = Path.Combine(dir, s_WorldsDir);
			foreach (string filename in mWorldNames)
			{
				string _dir = Path.Combine(world_dir, filename);

				YirdData data = new YirdData(_dir);

				mList.Add(data);
			}

			mDir = dir;


            return true;
        }

		// Cross the scene
		public class Mgr: PeSingleton<Mgr>, IPesingleton
		{
			Dictionary<string, CustomGameData> mDatas = new Dictionary<string, CustomGameData>(5);

			public CustomGameData curGameData;

			public CustomGameData GetCustomData(string UID, string Path = null)
			{
				if (mDatas.ContainsKey(UID))
					return mDatas[UID];

				if (Path == null)
					return null;

				CustomGameData data = new CustomGameData();
				data.Load(Path);

				mDatas.Add(UID, data);
				return data;
			}


			void IPesingleton.Init()
			{
			}

            public YirdData GetYirdData(string UID, string yirdName = null)
            {
				CustomGameData customGameData = GetCustomData(UID);
                if (customGameData == null)
                {
                    return null;
                }

                return customGameData.GetYirdData(yirdName);
            }

			/// <summary>
			/// --------- [!!! Obsolete !!!] ---------
			/// </summary>
			public List<CustomGameData> GetCustomGameList()
			{
				return null;
			}
		}

//        //cross scene
//        public class Mgr : PeSingleton<Mgr>, IPesingleton
//        {
//            List<CustomGameData> mList = new List<CustomGameData>(1);
//
//			public CustomGameData curGameData;
//
//			public bool Load(string dir = null)
//            {
//				if(string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
//				{
//					dir = GameConfig.CustomDataDir;
//				}
//
//                mList.Clear();
//
//				DirectoryInfo dirInfo = new DirectoryInfo(dir);
//
//                if (!dirInfo.Exists)
//                {
//                    return false;
//                }
//
//                DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();
//
//				if(subDirInfos == null || subDirInfos.Length == 0)
//				{
//					return false;
//				}
//
//                foreach (DirectoryInfo subDirInfo in subDirInfos)
//                {
//                    CustomGameData data = new CustomGameData();
//                    if (data.Load(subDirInfo.FullName))
//                    {
//                        mList.Add(data);
//                    }
//                }
//
//
//				return true;
//            }
//
//			public List<CustomGameData> GetCustomGameList()
//			{
//				return mList;
//			}
//
//            public CustomGameData GetCustomGameData(string gameName)
//            {
//                return mList.Find((item) =>
//                {
//                    if (item.name == gameName)
//                    {
//                        return true;
//                    }
//                    else
//                    {
//                        return false;
//                    }
//                });
//            }
//
//            public YirdData GetYirdData(string gameName, string yirdName = null)
//            {
//                CustomGameData customGameData = GetCustomGameData(gameName);
//                if (customGameData == null)
//                {
//                    return null;
//                }
//
//                return customGameData.GetYirdData(yirdName);
//            }
//
//            void IPesingleton.Init()
//            {
//                Load();
//            }
//        }
    }


	public class TutorialGameData
	{
		YirdData mYirdData;
		
		public YirdData yirdData  {get { return mYirdData;}}
		
		//string mDir;
		
		static readonly string s_WorldDir = "Mainland";
		public bool Load(string dir)
		{
			
			DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(dir, s_WorldDir));
			if (!dirInfo.Exists)
			{
				return false;
            }
            
            mYirdData = new YirdData(dirInfo.FullName);
            
            //mDir = dir;
            
            
            return true;
		}
	}

}