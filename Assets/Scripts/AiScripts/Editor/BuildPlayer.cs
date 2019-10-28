using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BuildPlayer : EditorWindow
{
	//Todo : set asset bundles in GameBuild and test
	const string PublishPathOfVoxelEditor = "\"../GameBuild/VoxelEditor/win32\"";

	[MenuItem("Release/Pathea")]
    static void BuildPathea()
    {
		if(!PreBuild()) return;

        if(ExecuteBuild(BuildParameterFactory.Pathea_PC))
			if(ExecuteBuild(BuildParameterFactory.Pathea_Mac))
				if(ExecuteBuild(BuildParameterFactory.Pathea_Linux))
        			PostBuild();
    }

	[MenuItem("Release/Pathea X64")]
	static void BuildPatheaX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_PC_X64))
		   if(ExecuteBuild(BuildParameterFactory.Pathea_Mac_X64))
				if(ExecuteBuild(BuildParameterFactory.Pathea_Linux_X64))
					PostBuild();
	}
	
	[MenuItem("Release/Steam")]
    static void BuildSteam()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_PC))
		   if(ExecuteBuild(BuildParameterFactory.Steam_Mac))
				if(ExecuteBuild(BuildParameterFactory.Steam_Linux))
					PostBuild();
    }

	[MenuItem("Release/Steam X64")]
	static void BuildSteamX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_PC_X64))
			if(ExecuteBuild(BuildParameterFactory.Steam_PC))
				if(ExecuteBuild(BuildParameterFactory.Steam_Mac_X64))
					if(ExecuteBuild(BuildParameterFactory.Steam_Linux_X64))
						PostBuild();
	}

	[MenuItem("Release/Steam X64 Demo")]
	static void BuildSteamX64Demo()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_PC_X64_Demo))
			if(ExecuteBuild(BuildParameterFactory.Steam_Mac_X64_Demo))
				if(ExecuteBuild(BuildParameterFactory.Steam_Linux_X64_Demo))
					PostBuild();
	}
	
	[MenuItem("Release/Pathea PC")]
    static void BuildPatheaPC()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Pathea_PC))
			PostBuild();
    }

	[MenuItem("Release/Pathea PC X64")]
	static void BuildPatheaPCX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_PC_X64))
			PostBuild();
	}
	
	[MenuItem("Release/Pathea Mac")]
	static void BuildPatheaMac()
	{
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Pathea_Mac))
			PostBuild();
	}

	[MenuItem("Release/Pathea Mac X64")]
	static void BuildPatheaMacX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_Mac_X64))
			PostBuild();
	}
	
	[MenuItem("Release/Pathea Linux")]
    static void BuildPatheaLinux()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Pathea_Linux))
			PostBuild();
    }

	[MenuItem("Release/Pathea Linux X64")]
	static void BuildPatheaLinuxX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_Linux_X64))
			PostBuild();
	}
	
	[MenuItem("Release/Steam PC")]
    static void BuildSteamPC()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_PC))
			PostBuild();
    }

	[MenuItem("Release/Steam PC X64")]
	static void BuildSteamPCX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_PC_X64))
			PostBuild();
	}
	
	[MenuItem("Release/Steam Mac")]
    static void BuildSteamMac()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_Mac))
			PostBuild();
    }

	[MenuItem("Release/Steam Mac X64")]
	static void BuildSteamMacX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_Mac_X64))
			PostBuild();
	}
	
	[MenuItem("Release/Steam Linux")]
    static void BuildSteamLinux()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_Linux))
			PostBuild();
    }

	[MenuItem("Release/Steam Linux X64")]
	static void BuildSteamLinuxX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_Linux_X64))
			PostBuild();
	}
	
	//debug
	[MenuItem("Debug/Pathea")]
    static void BuildPatheaDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Pathea_PC_Debug))
		   if(ExecuteBuild(BuildParameterFactory.Pathea_Mac_Debug))
				if(ExecuteBuild(BuildParameterFactory.Pathea_Linux_Debug))
					PostBuild();
    }

	[MenuItem("Debug/Pathea X64")]
	static void BuildPatheaDebug_X64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_PC_X64_Debug))
		   if(ExecuteBuild(BuildParameterFactory.Pathea_Mac_X64_Debug))
			   if(ExecuteBuild(BuildParameterFactory.Pathea_Linux_X64_Debug))
					PostBuild();
	}
	
	[MenuItem("Debug/Steam")]
    static void BuildSteamDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_PC_Debug))
			if(ExecuteBuild(BuildParameterFactory.Steam_Mac_Debug))
				if(ExecuteBuild(BuildParameterFactory.Steam_Linux_Debug))
					PostBuild();
    }

	[MenuItem("Debug/Steam X64")]
	static void BuildSteamDebug_X64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_PC_X64_Debug))
			if(ExecuteBuild(BuildParameterFactory.Steam_Mac_X64_Debug))
				if(ExecuteBuild(BuildParameterFactory.Steam_Linux_X64_Debug))
					PostBuild();
	}
	
	[MenuItem("Debug/Pathea PC")]
    static void BuildPatheaPCDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Pathea_PC_Debug))
			PostBuild();
    }

	[MenuItem("Debug/Pathea PC X64")]
	static void BuildPatheaPCDebug_X64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_PC_X64_Debug))
			PostBuild();
	}
	
	[MenuItem("Debug/Pathea Mac")]
    static void BuildPatheaMacDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Pathea_Mac_Debug))
			PostBuild();
    }

	[MenuItem("Debug/Pathea Mac X64")]
	static void BuildPatheaMacDebugX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_Mac_X64_Debug))
			PostBuild();
	}
	
	[MenuItem("Debug/Pathea Linux")]
    static void BuildPatheaLinuxDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Pathea_Linux_Debug))
			PostBuild();
    }

	[MenuItem("Debug/Pathea Linux X64")]
	static void BuildPatheaLinuxDebugX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Pathea_Linux_X64_Debug))
			PostBuild();
	}
	
	[MenuItem("Debug/Steam PC")]
    static void BuildSteamPCDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_PC_Debug))
			PostBuild();
    }

	[MenuItem("Debug/Steam PC X64")]
	static void BuildSteamPCDebugX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_PC_X64_Debug))
			PostBuild();
	}
	
	[MenuItem("Debug/Steam Mac")]
    static void BuildSteamMacDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_Mac_Debug))
			PostBuild();
    }

	[MenuItem("Debug/Steam Mac X64")]
	static void BuildSteamMacDebugX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_Mac_X64_Debug))
			PostBuild();
	}

	
	[MenuItem("Debug/Steam Linux")]
    static void BuildSteamLinuxDebug()
    {
		if(!PreBuild()) return;

		if(ExecuteBuild(BuildParameterFactory.Steam_Linux_Debug))
			PostBuild();
    }

	[MenuItem("Debug/Steam Linux X64")]
	static void BuildSteamLinuxDebugX64()
	{
		if(!PreBuild()) return;
		
		if(ExecuteBuild(BuildParameterFactory.Steam_Linux_X64_Debug))
			PostBuild();
	}

	static bool PreBuild()
	{
		// Select output directory
		BuildParameterFactory.BuildPath = EditorUtility.OpenFolderPanel("Build Project", EditorPrefs.GetString("BuildPath"), "");
		if (BuildParameterFactory.BuildPath.Length <= 0)
			return false;
		EditorPrefs.SetString("BuildPath", BuildParameterFactory.BuildPath);

		// Update svn
		PeCustomMenuc.ExecuteCommandSync("svn update");
		PeCustomMenuc.ExecuteCommandSync("svn add AssetBundles/* --force");
		PeCustomMenuc.ExecuteCommandSync("svn commit AssetBundles -m \"Build version\"");
		return true;
	}
	static bool PostBuild()
	{
		// Reset build settings
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);

		// Update asset bundles to game build
		PeCustomMenuc.ExecuteCommandSync("svn update " + PublishPathOfVoxelEditor);
		Debug.Log ("Build Succeeded.");
		return true;
	}
	
	static class BuildParameterFactory
    {
        public static string BuildPath = "";

        static string BuildName = "PE_Client";
		static string BuildPath_Pathea_PC { get { return BuildPath + "/Planet Explorers_PC/"; } }
		static string BuildPath_Pathea_Mac { get { return BuildPath + "/Planet Explorers_Mac/"; } }
		static string BuildPath_Pathea_Linux { get { return BuildPath + "/Planet Explorers_Linux/"; } }
		static string BuildPath_Steam_PC { get { return BuildPath + "/Planet Explorers_Steam_PC/"; } }
		static string BuildPath_Steam_Mac { get { return BuildPath + "/Planet Explorers_Steam_Mac/"; } }
		static string BuildPath_Steam_Linux { get { return BuildPath + "/Planet Explorers_Steam_Linux/"; } }

		static string BuildPath_Pathea_PC_X64 { get { return BuildPath + "/Planet Explorers_PC_X64/"; } }
//		static string BuildPath_Pathea_Mac_X64 { get { return BuildPath + "/Planet Explorers_Mac_X64/"; } }
//		static string BuildPath_Pathea_Linux_X64 { get { return BuildPath + "/Planet Explorers_Linux_X64/"; } }
		static string BuildPath_Pathea_Mac_X64 { get { return BuildPath + "/Planet Explorers_Mac/"; } }
		static string BuildPath_Pathea_Linux_X64 { get { return BuildPath + "/Planet Explorers_Linux/"; } }
		static string BuildPath_Steam_PC_X64 { get { return BuildPath + "/Planet Explorers_Steam_PC_X64/"; } }
		static string BuildPath_Steam_Mac_X64 { get { return BuildPath + "/Planet Explorers_Steam_Mac/"; } }
		static string BuildPath_Steam_Linux_X64 { get { return BuildPath + "/Planet Explorers_Steam_Linux/"; } }
//		static string BuildPath_Steam_Mac_X64 { get { return BuildPath + "/Planet Explorers_Steam_Mac_X64/"; } }
//		static string BuildPath_Steam_Linux_X64 { get { return BuildPath + "/Planet Explorers_Steam_Linux_X64/"; } }

//		static string BuildPath_Pathea_PC_X64 { get { return BuildPath + "/Planet Explorers_PC/"; } }
//		static string BuildPath_Pathea_Mac_X64 { get { return BuildPath + "/Planet Explorers_Mac/"; } }
//		static string BuildPath_Pathea_Linux_X64 { get { return BuildPath + "/Planet Explorers_Linux/"; } }
//		static string BuildPath_Steam_PC_X64 { get { return BuildPath + "/Planet Explorers_Steam_PC/"; } }
//		static string BuildPath_Steam_Mac_X64 { get { return BuildPath + "/Planet Explorers_Steam_Mac/"; } }
//		static string BuildPath_Steam_Linux_X64 { get { return BuildPath + "/Planet Explorers_Steam_Linux/"; } }
		
        public static BuildParameter Pathea_PC
        {
            get 
            {
                return new BuildParameter(BuildPath_Pathea_PC, 
                                         BuildName, 
				                          "FMOD_LIVEUPDATE", 
                                         BuildTarget.StandaloneWindows, 
                                         BuildOptions.None, 
                                         BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Pathea_PC_X64
		{
			get 
			{
				return new BuildParameter(BuildPath_Pathea_PC_X64, 
				                          BuildName, 
				                          "FMOD_LIVEUPDATE", 
				                          BuildTarget.StandaloneWindows64, 
				                          BuildOptions.None, 
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Pathea_PC_Debug
		{
			get
			{
				return new BuildParameter(BuildPath_Pathea_PC,
                                         BuildName,
				                          "FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneWindows,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Pathea_PC_X64_Debug
		{
			get
			{
				return new BuildParameter(BuildPath_Pathea_PC_X64,
				                          BuildName,
				                          "FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneWindows64,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Pathea_Mac
		{
			get
			{
                return new BuildParameter(BuildPath_Pathea_Mac,
                                         BuildName,
				                          "FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneOSXIntel,
                                         BuildOptions.None,
                                         BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Pathea_Mac_X64
		{
			get
			{
				return new BuildParameter(BuildPath_Pathea_Mac_X64,
				                          BuildName,
				                          "FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneOSXIntel64,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Pathea_Mac_Debug
		{
			get
            {
                return new BuildParameter(BuildPath_Pathea_Mac,
                                         BuildName,
				                          "FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneOSXIntel,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Pathea_Mac_X64_Debug
		{
			get
			{
				return new BuildParameter(BuildPath_Pathea_Mac_X64,
				                          BuildName,
				                          "FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneOSXUniversal,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Pathea_Linux
		{
			get
			{
				return new BuildParameter(BuildPath_Pathea_Linux,
                                         BuildName,
				                          "FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneLinux,
                                         BuildOptions.None,
                                         BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Pathea_Linux_X64
		{
			get
			{
				return new BuildParameter(BuildPath_Pathea_Linux_X64,
				                          BuildName,
				                          "FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneLinuxUniversal,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Pathea_Linux_Debug
		{
            get
            {
                return new BuildParameter(BuildPath_Pathea_Linux,
                                         BuildName,
				                          "FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneLinux,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Pathea_Linux_X64_Debug
		{
			get
			{
				return new BuildParameter(BuildPath_Pathea_Linux_X64,
				                          BuildName,
				                          "FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneLinuxUniversal,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		
		public static BuildParameter Steam_PC
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_PC,
                                         BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE;Win32Ver",
                                         BuildTarget.StandaloneWindows,
                                         BuildOptions.None,
                                         BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Steam_PC_X64
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_PC_X64,
				                          BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneWindows64,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Steam_PC_X64_Demo
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_PC_X64,
				                          BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE;DemoVersion",
				                          BuildTarget.StandaloneWindows64,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Steam_PC_Debug
		{
			get
            {
                return new BuildParameter(BuildPath_Steam_PC,
                                         BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE;Win32Ver",
                                         BuildTarget.StandaloneWindows,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Steam_PC_X64_Debug
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_PC_X64,
				                          BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneWindows64,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Steam_Mac
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Mac,
                                         BuildName,
				                         "SteamVersion;FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneOSXIntel,
                                         BuildOptions.None,
                                         BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Steam_Mac_X64
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Mac_X64,
				                          BuildName,
				                          "SteamVersion",
				                          BuildTarget.StandaloneOSXUniversal,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}

		public static BuildParameter Steam_Mac_X64_Demo
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Mac_X64,
				                          BuildName,
				                          "SteamVersion;DemoVersion",
				                          BuildTarget.StandaloneOSXUniversal,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Steam_Mac_Debug
		{
			get
            {
                return new BuildParameter(BuildPath_Steam_Mac,
                                         BuildName,
				                         "SteamVersion;FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneOSXIntel,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Steam_Mac_X64_Debug
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Mac_X64,
				                          BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneOSXUniversal,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public static BuildParameter Steam_Linux
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Linux,
                                         BuildName,
				                         "SteamVersion;FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneLinux,
                                         BuildOptions.None,
                                         BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Steam_Linux_X64
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Linux_X64,
				                          BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneLinuxUniversal,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}

		public static BuildParameter Steam_Linux_X64_Demo
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Linux_X64,
				                          BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE;DemoVersion",
				                          BuildTarget.StandaloneLinuxUniversal,
				                          BuildOptions.None,
				                          BuildTargetGroup.Standalone);
			}
		}

		public static BuildParameter Steam_Linux_Debug
		{
			get
            {
                return new BuildParameter(BuildPath_Steam_Linux,
                                         BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE",
                                         BuildTarget.StandaloneLinux,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
            }
        }

		public static BuildParameter Steam_Linux_X64_Debug
		{
			get
			{
				return new BuildParameter(BuildPath_Steam_Linux_X64,
				                          BuildName,
				                          "SteamVersion;FMOD_LIVEUPDATE",
				                          BuildTarget.StandaloneLinuxUniversal,
				                          BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler,
				                          BuildTargetGroup.Standalone);
			}
		}
		
		public class BuildParameter
		{
			string mPath;
			string mName;
            string mSymbols;
            string[] mScenes;

            string mError;

            BuildTarget mTarget;
            BuildOptions mOptions;
            BuildTargetGroup mTargets;

            public BuildParameter(string path,
                                 string name,
                                 string symbols,
                                 BuildTarget target,
                                 BuildOptions options,
                                 BuildTargetGroup targets)
            {
                mPath = path;
                mName = name;
                mSymbols = symbols;
                mTarget = target;
                mOptions = options;
                mTargets = targets;

                mScenes = FindBuildScenes();
            }

            public string[] scenes { get { return mScenes; } }
            public string name { get { return mName; } }
            public string directory { get { return mPath; } }
            public string fileName { get { return Path.Combine(mPath, mName); } }
            public string path { get { return fileName + GetExtension(mTarget); } }
            public string symbols { get { return mSymbols; } }
            public string error { get { return mError; } }
            public BuildTarget target { get { return mTarget; } }
            public BuildOptions options { get { return mOptions; } }
            public BuildTargetGroup targets { get { return mTargets; } }

            public override string ToString()
            {
                string ss = "BuildParamter[\ntarget:" + mTarget + "\nOption:" + mOptions + "\nname:" + path + "\nscene[";
                foreach (string s in mScenes)
                {
                    ss += s + ",";
                }
                ss += "]\nSymbols:" + mSymbols
                    + "\n]";
                return ss;
            }

            string[] FindBuildScenes()
            {
                List<string> sceneList = new List<string>();
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    if (!scene.enabled)
                    {
                        continue;
                    }

                    sceneList.Add(scene.path);
                }

                return sceneList.ToArray();
            }

            string GetExtension(BuildTarget target)
            {
                switch (target)
                {
                    //case BuildTarget.Android:
                    //    break;
                    //case BuildTarget.BB10:
                    //    break;
                    //case BuildTarget.FlashPlayer:
                    //    break;
                    //case BuildTarget.MetroPlayer:
                    //    break;
                    //case BuildTarget.NaCl:
                    //    break;
                    //case BuildTarget.PS3:
                    //    break;
                    //case BuildTarget.StandaloneGLESEmu:
                    //    break;
                    case BuildTarget.StandaloneLinux:
                        return ".x86";
                    case BuildTarget.StandaloneLinux64:
                        return ".x64";
                    case BuildTarget.StandaloneLinuxUniversal:
                        return ".x64";
                    case BuildTarget.StandaloneOSXIntel:
                        return ".app";
                    case BuildTarget.StandaloneOSXIntel64:
                        return ".app";
                    case BuildTarget.StandaloneOSXUniversal:
                        return ".app";
                    case BuildTarget.StandaloneWindows:
                        return ".exe";
                    case BuildTarget.StandaloneWindows64:
                        return ".exe";
                    //case BuildTarget.Tizen:
                    //    break;
                    //case BuildTarget.WP8Player:
                    //    break;
                    //case BuildTarget.WebPlayer:
                    //    break;
                    //case BuildTarget.WebPlayerStreamed:
                    //    break;
                    //case BuildTarget.Wii:
                    //    break;
                    //case BuildTarget.XBOX360:
                    //    break;
                    //case BuildTarget.iPhone:
                    //    break;
                    default:
                        mError += "Failed to get extension : " + target.ToString();
                        return "";
                }
            }
        }
    }

    static bool ExecuteBuild(BuildParameterFactory.BuildParameter parameter)
    {
        if (!string.IsNullOrEmpty(parameter.error)) {
            Debug.LogError(parameter.error);
			return EditorUtility.DisplayDialog("!Failed", "ParaError:"+parameter.error, "Continue", "Abort");
        }

        if (BuildPipeline.isBuildingPlayer) {
			return EditorUtility.DisplayDialog("!Failed", "IsBuildingPlayer", "Continue", "Abort");
        }

        if (!PrepareBuildDir(parameter)) {
			return EditorUtility.DisplayDialog("!Failed", "Failed to prapare build dir", "Continue", "Abort");
        }

        PreparePlayerSetting(parameter);
        PrepareBuildSetting(parameter);

        string error = BuildPipeline.BuildPlayer(parameter.scenes, parameter.path, parameter.target, parameter.options);
        if (string.IsNullOrEmpty (error)) {
			CopyFile (parameter);
			Debug.Log ("Succed to build player : " + parameter);
		} else {
			Debug.LogError (error);
			return EditorUtility.DisplayDialog("!Failed", error, "Continue", "Abort");
		}
		return true;
    }

    static void PrepareBuildSetting(BuildParameterFactory.BuildParameter parameter)
    {
        if (EditorUserBuildSettings.activeBuildTarget != parameter.target)
        {
            Debug.Log("Switch to target : " + parameter.target);

            EditorUserBuildSettings.SwitchActiveBuildTarget(parameter.target);
        }
    }

    static void PreparePlayerSetting(BuildParameterFactory.BuildParameter parameter)
    {
        if (PlayerSettings.GetScriptingDefineSymbolsForGroup(parameter.targets) != parameter.symbols)
        {
            Debug.Log("Set symbols : " + parameter.symbols);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(parameter.targets, parameter.symbols);
        }
    }

    static bool PrepareBuildDir(BuildParameterFactory.BuildParameter parameter)
    {
        if (!System.IO.Directory.Exists(parameter.directory))
        {
            try
            {
                Directory.CreateDirectory(parameter.directory);

                Debug.Log("Succeed to create directory : " + parameter.directory);
                return true;
            }
            catch (System.Exception e)
            {
				Debug.LogError("Failed to create directory : " + e);
                return false;
            }
        }

        return true;
    }

    static void CopyFile(BuildParameterFactory.BuildParameter parameter)
    {
        if (parameter.target == BuildTarget.StandaloneLinux || parameter.target == BuildTarget.StandaloneLinuxUniversal) {
			string src = Directory.GetCurrentDirectory () + "/config";
			string dst = parameter.directory + parameter.name + "_Data/Mono/etc/mono/config";
			try {
				FileUtil.ReplaceFile (src, dst);
			} catch (System.Exception e) {
				Debug.LogError ("Failed to copy file : " + e);
			}
		} 
		/* After ver 4.6(included), .app/Contents/MacOS/* would use build name instead of ProductName
		else if (parameter.target == BuildTarget.StandaloneOSXIntel || parameter.target == BuildTarget.StandaloneOSXUniversal) {
			string src = parameter.directory + parameter.name + ".app/Contents/MacOS/PE_Client";
			string dst = parameter.directory + parameter.name + ".app/Contents/MacOS/Planet Explorers";			
			try {
				FileUtil.DeleteFileOrDirectory(dst);
				FileUtil.MoveFileOrDirectory(src, dst);
				//FileUtil.CopyFileOrDirectory(src, dst);
			} catch (System.Exception e) {
				Debug.LogError("Failed to copy file : " + e);
			}
		}
		*/
	}

	
	[MenuItem("Release/Test/SVN Update")]
	static void SvnUpdateAll()
	{
		//PeCustomMenu.ExecuteCommandSync("svn update");
	}
	[MenuItem("Release/Test/SVN Commit assetbundle")]
	static void SvnCommitAssetbundles()
	{
		//PeCustomMenu.ExecuteCommandSync("svn help");
		PeCustomMenuc.ExecuteCommandSync("svn add AssetBundles/* --force");
		PeCustomMenuc.ExecuteCommandSync("svn commit AssetBundles -m \"Build version\"");
	}
}
