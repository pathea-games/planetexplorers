using UnityEngine;
using System;
using System.IO;

public enum RandomMapType
{
    GrassLand = 1,
    Forest,
    Desert,
    Redstone,
    Rainforest,
	Mountain,
	Swamp,
	Crater
}

public class GameConfig
{
	public const int MaxFixedUpdate = 2;

	public static bool IsInVCE = false;

	public static string PEDataPath
	{
		get
		{
#if UNITY_EDITOR
			return Application.dataPath + "/../";
#elif UNITY_STANDALONE_OSX
			return Application.dataPath + "/../../";
#else
			return Application.dataPath + "/../";
#endif
		}
	}
	static string _strGameVersion = null;
	public static string GameVersion
	{
		get{
			if(_strGameVersion == null){
				string filePath = GameConfig.PEDataPath + "ConfigFiles/version.txt";
				_strGameVersion = "";
				if (File.Exists (filePath)) {
					using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
						StreamReader reader = new StreamReader (fs);
						reader.BaseStream.Seek (0, SeekOrigin.Begin);
						_strGameVersion = reader.ReadLine ();
						reader.Close ();
						fs.Close ();
					}
				}
			}
			return _strGameVersion;
		}
	}
	
	public const string StartSceneName = "GameStart";
	public const string RoleSceneName = "GameRoleCustom";
	public const string LobbySceneName = "GameLobby";
	public const string MainMenuSceneName = "GameMainMenu";
	public const string AdventureSceneName = "GameAdventure";
	public const string BuildSceneName = "GameBuild";
	public const string MainSceneName = "GameStory";
	public const string ClientSceneName = "GameClient";
	public const string CreationSceneName = "CreationSystem";
    public const string MultiRoleSceneName = "MLoginScene";
	public const string TutorialSceneName = "GameTraining";
	
	public const string AssetBundleDir = "AssetBundles";
	public const string MapDataDir_Zip = "VoxelData/zData";
	public const string Network_MapDataDir_Zip = "VoxelData/networkvoxel";
	//public const string MapDataDir_Raw = "VoxelData/rawDat";
	public const string MapDataDir_Plant = "VoxelData/SubTerrains";
	public const string DataBaseFile = "DataBase/localData";
	//public const string RecordDataDir = "/PlanetExplorers/SaveData";
	public const string ConfigDataDir = "/PlanetExplorers/Config";
    public const string CreateSystemData = "/PlanetExplorers/CreateData";
    public const string VCSystemData = "/PlanetExplorers/VoxelCreationData";

	//public static readonly string MapDataPath_Zip = PEDataPath + MapDataDir_Zip + "/";
	//public static readonly string Network_MapDataPath_Zip = PEDataPath + Network_MapDataDir_Zip + "/";
	//public static readonly string MapDataPath_Raw = PEDataPath + MapDataDir_Raw + "/";
	//public static readonly string MapDataPath_Plant = PEDataPath + MapDataDir_Plant + "/";
	public static readonly string AssetsManifest_Base = "BaseAssets";
    public static readonly string AssetsManifest_Dynamic = "DynamicAssets";
    public static readonly string AssetsManifest_Item = "Item";
    public static readonly string AssetsManifest_Monster = "Monster";
    public static readonly string AssetsManifest_Tower = "Tower";
    public static readonly string AssetsManifest_Puja = "Native";
    public static readonly string AssetsManifest_Alien = "Alien";
    public static readonly string AssetsManifest_Group = "Group";
    public static readonly string AssetsManifest_Player = "Player";
    public static readonly string AssetsManifest_Npc = "Npc";
	//public static readonly string Network_AssetsManifest_Dynamic = "NetworkDynamicAssets/";

	public static readonly string DataBaseI18NPath = PEDataPath + "i18n.db";
	public static readonly string AssetBundlePath = PEDataPath + AssetBundleDir+"/";
	//public static readonly string AssetBundlePath_Base = AssetBundlePath+AssetsManifest_Base+"/";
	//public static readonly string AssetBundlePath_Dynamic = AssetBundlePath+AssetsManifest_Dynamic+"/";

	public static readonly string OclSourcePath = UnityEngine.Application.dataPath + "/Resources/OclKernel";
#if SteamVersion
	public static readonly string OclBinaryPath = PEDataPath + "clCache";
	public static readonly string VoxelCacheDataPath = PEDataPath + "voxelCache/";
#else
	public static string OclBinaryPath{			get{	return Path.Combine(GameConfig.GetPeUserDataPath(), "clCache");		}	}
	public static string VoxelCacheDataPath{	get{	return Path.Combine(GameConfig.GetPeUserDataPath(), "voxelCache/");	}	}
#endif

    public static string CustomDataDir
    {
        get
        {
            return System.IO.Path.Combine(GameConfig.PEDataPath, "CustomGames");
        }
    }

	//public static readonly string OriginalSubTerrainDir = PEDataPath + "VoxelData/SubTerrains/";

	// Used in merge voxel data
	public const string UserDataDir = "UserData/0";
	public static readonly string MergedUserDataPath = PEDataPath + UserDataDir + "/Merged/";
	//public static readonly string RecordDataPath = Application.dataPath + "/" + RecordDataDir + "/"; // tst code in zhu archive

    //public static bool Is64BitSystem()
    //{
    //    string pa = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
    //    return ((System.String.IsNullOrEmpty(pa) || pa.Substring(0, 3) == "x86") ? false : true);
    //}
    //public static bool IsBatchMode()
    //{
    //    return System.Environment.GetCommandLineArgs().Length > 1;
    //}

	static string _userDataPath = string.Empty;
	public static void SetUserDataPath(string userDataPath)
	{
		if (!string.IsNullOrEmpty (userDataPath)) {
			try{
				if(Directory.Exists(userDataPath)){
					_userDataPath = userDataPath;
					return;
				}
				throw new Exception("Path Not Exists!");
			} catch(Exception e) {
				Debug.LogWarning("[UserDataPath]Failed to set " + userDataPath +":" + e);
			}
		}
		_userDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
	}
	public static string GetUserDataPath()
	{
		if (string.IsNullOrEmpty (_userDataPath)) {
			return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		}
		return _userDataPath;
	}

    public static string GetPeUserDataPath()
    {
        return System.IO.Path.Combine(GetUserDataPath(), "PlanetExplorers");
    }

	public const float NetMinUpdateValue = 3f;
	public const float NetUpdateInterval = 3f;

    public static bool IsMultiMode
    {
        get
        {
            return Pathea.PeGameMgr.IsMulti;
        }
    }

    public static bool IsMultiClient { get { return uLink.Network.isClient; } }
    public static bool IsMultiServer { get { return uLink.Network.isServer; } }
	// [Edit by zx]
	public static bool IsNight { get { return GameTime.Timer.CycleInDay < -0.1; } } //return NVWeatherSys.Instance == null ? false : NVWeatherSys.Instance.IsNight; } }

    public static int GroundLayer = 1 << Pathea.Layer.VFVoxelTerrain
                                   | 1 << Pathea.Layer.SceneStatic;

	
	public static int SceneLayer = 1 << Pathea.Layer.VFVoxelTerrain
		| 1 << Pathea.Layer.SceneStatic
			| 1 << Pathea.Layer.Unwalkable
			| 1 << Pathea.Layer.TreeStatic
			| 1 << Pathea.Layer.Building
			| 1 << Pathea.Layer.GIEProductLayer;

    public static int ProjectileDamageLayer = 1 << Pathea.Layer.VFVoxelTerrain
                                            | 1 << Pathea.Layer.SceneStatic
                                            | 1 << Pathea.Layer.Unwalkable
                                            | 1 << Pathea.Layer.Damage
                                            | 1 << Pathea.Layer.TreeStatic
                                            | 1 << Pathea.Layer.Building
                                            | 1 << Pathea.Layer.GIEProductLayer;

	public const float NPCControlDistance = 4.5f;

    public static readonly string RadioSoundsPath=Path.Combine(PEDataPath, "CustomSounds/");
    public static readonly string OSTSoundsPath = Path.Combine(PEDataPath, "Soundtrack/");
}
