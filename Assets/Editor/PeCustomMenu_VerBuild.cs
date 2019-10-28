using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;

public class SvnManager{
	public string FolderName { get; private set; }
	public SvnManager( string FolderName )
	{
		this.FolderName = FolderName;    
	}
#if true
	const string ENTRIES = "entries";
	public string RevisionNumber    
	{        
		get{
			string SvnSubfolder = FolderName + "/.svn";
			if ( Directory.Exists( SvnSubfolder ) ) 
			{
				string EntriesFile = Directory.GetFiles( SvnSubfolder, ENTRIES ).FirstOrDefault();
				if ( !string.IsNullOrEmpty( EntriesFile ) )
				{
					string[] Lines = File.ReadAllLines( EntriesFile );
					if ( Lines.Length > 3 )
						return Lines[3];
				}
			}
			return string.Empty;
		}
	}
#else
	// For Tortoise v1.7, sqlite db in root's .svn subfolder used instead of .svn subfolder in each folder
	const string DB      = "wc.db";     
	const string PATTERN = "/!svn/ver/(?'version'[0-9]*)/";     
	public string RevisionNumber     {         
		get
		{
			string SvnSubfolder = FolderName + "/.svn";
			if ( Directory.Exists( SvnSubfolder ) )
			{
				int maxVer = int.MinValue;
				string EntriesFile = Directory.GetFiles( SvnSubfolder, DB ).FirstOrDefault();
				if ( !string.IsNullOrEmpty( EntriesFile ) )
				{
					byte[] fileData = File.ReadAllBytes( EntriesFile );
					string fileDataString = Encoding.Default.GetString( fileData );
					Regex regex = new Regex( PATTERN );
					foreach ( Match match in regex.Matches( fileDataString ) )
					{
						string version = match.Groups["version"].Value;
						int curVer;
						if ( int.TryParse( version, out curVer ) )
							if ( curVer > maxVer )
								maxVer = curVer;
					}
					if ( maxVer > int.MinValue )
						return maxVer.ToString();
				}
			}
			return string.Empty;
		}
	}
#endif
}

public partial class PeCustomMenuc : EditorWindow 
{
    public enum EVersion
    {
        DailyVer,
        GDCVer,
        ReleaseVer,
    }

    public const EVersion Version = EVersion.GDCVer;

	static string ComposeBuildPath()
	{
		string buildPath = buildTargetPath[(int)Version]+string.Format("_Build{0:yyMMddHHmm}",DateTime.Now);
//		string[] args = Environment.GetCommandLineArgs();
//		int len = args.Length;
//		if(args[len-1].IndexOf("BuildVersion") != -1)
		{
			SvnManager manager = new SvnManager( "Assets" );
			string revisionNumber = manager.RevisionNumber;
			buildPath += "_Ver0."+revisionNumber;
		}
		buildPath += "/";
		return buildPath;
	}
	
	static void BuildLevelsWithoutCopy(string[] levels, string targetPath, BuildOptions buildOptions)
	{
		string targetName = "PE";
		string targetFilePathName = targetPath + targetName + ".exe";
		//string tagetDataPath = targetPath + targetName+"_Data/";
		BuildPipeline.BuildPlayer (levels, targetFilePathName, BuildTarget.StandaloneWindows, BuildOptions.Development|BuildOptions.AllowDebugging);
	}
	static void BuildLevels(string[] levels, string targetPath, BuildOptions buildOptions)
	{
		string copyParam = DetectWindows7or2008R2() ? 
			" /MT /S /XF *.meta /XD .svn" : 
				" /S /XF *.meta /XD .svn";	// mt function only in win7&ws2008R2
		string targetName = "PE";
		string targetFilePathName = targetPath + targetName + ".exe";
		string tagetDataPath = targetPath + targetName+"_Data/";
		BuildPipeline.BuildPlayer (levels, targetFilePathName, BuildTarget.StandaloneWindows, BuildOptions.Development|BuildOptions.AllowDebugging);
		string[] cmds = {// ASP.NET hasn't enought permission to perform XCOPY, so we use robocopy
			"robocopy "+"Assets/"+GameConfig.AssetBundleDir +" "+tagetDataPath+GameConfig.AssetBundleDir + copyParam,
			"robocopy "+"Assets/"+GameConfig.MapDataDir_Zip +" "+tagetDataPath+GameConfig.MapDataDir_Zip + copyParam,
			"robocopy "+"Assets/"+GameConfig.Network_MapDataDir_Zip +" "+tagetDataPath+GameConfig.Network_MapDataDir_Zip + copyParam,
			"robocopy "+"Assets/"+GameConfig.MapDataDir_Plant +" "+tagetDataPath+GameConfig.MapDataDir_Plant + copyParam,
			"robocopy "+"Assets/"+GameConfig.UserDataDir +" "+tagetDataPath+GameConfig.UserDataDir + copyParam,
			};
		for(int i = 0; i < cmds.Length; i++)
			ExecuteCommandSync(cmds[i]);
	}
	
	static readonly string[] buildTargetPath = {
		"../../GameBuild/Dev",
		"../../GameBuild/GDC",
		"../../GameBuild/Rel",
	};
	static readonly BuildOptions[] buildOptions = new BuildOptions[]{
		BuildOptions.Development|BuildOptions.AllowDebugging,
		BuildOptions.None,
		BuildOptions.None,
	};
	
	//=====================================================================
	//[MenuItem("PeCustomMenu/Build Version")]
	static void BuildVersion ()
	{
		string buildPath = ComposeBuildPath();
		string[] buildLevels = {
			"Assets/Scenes/"+GameConfig.StartSceneName+".unity",
			"Assets/Scenes/"+GameConfig.RoleSceneName+".unity",
			"Assets/Scenes/"+GameConfig.MainSceneName+".unity",
		};

        BuildLevels(buildLevels, buildPath, buildOptions[(int)Version]);
	}
	
	//[MenuItem("PeCustomMenu/Build Version(No Data Copy)")]
	static void BuildVersionNoData ()
	{
		string buildPath = ComposeBuildPath();
		string[] buildLevels = {
			"Assets/Scenes/"+GameConfig.StartSceneName+".unity",
			"Assets/Scenes/"+GameConfig.RoleSceneName+".unity",
			"Assets/Scenes/"+GameConfig.MainSceneName+".unity",
		};

        BuildLevelsWithoutCopy(buildLevels, buildPath, buildOptions[(int)Version]);
	}
}
