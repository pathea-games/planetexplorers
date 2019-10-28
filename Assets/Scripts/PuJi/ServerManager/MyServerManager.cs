using System;
using System.IO;
using UnityEngine;

public class MyServerManager
{
	public static string LocalIp;
	public static string LocalPwd;
	public static string LocalName;
	public static int LocalPort;
	public static ServerRegistered LocalHost;

	public static event Action OnServerHostEvent;

	static void OnServerHost()
	{
		if (null != OnServerHostEvent)
			OnServerHostEvent();
	}

	public static bool StartMyServer(MyServer ss)
	{
#if SteamVersion
		if (ss != null)
		{
            //lz-2017.01.03 crash bug 错误 #8077 服务器名字正常情况下不会为null或空
            if (string.IsNullOrEmpty(ss.gameName))
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000018));
                Debug.Log(string.Format("ServerName is null! gameName: {0},gameMode:{1},gameType:{2},seedStr:{3}", ss.gameName,ss.gameMode, ss.gameType,ss.seedStr));
                return false;
            }
			int serverport = uLink.NetworkUtility.FindAvailablePort(9900, 9915);
			if (!uLink.NetworkUtility.IsPortAvailable(serverport))
			{
                MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000498), serverport));
				return false;
			}

			string srvRoot = Path.Combine(Environment.CurrentDirectory, "Server");
			Directory.CreateDirectory(srvRoot);
			string srvDataPath = Path.Combine(srvRoot, "ServerData");
			Directory.CreateDirectory(srvDataPath);

			string srvGamePath;
            if (ss.gameMode == (int)Pathea.PeGameMgr.ESceneMode.Custom)
                srvGamePath = Path.Combine(srvDataPath, "CustomGames");
            else
                srvGamePath = Path.Combine(srvDataPath, "CommonGames");

			Directory.CreateDirectory(srvGamePath);
			string serverDir = Path.Combine(srvGamePath, ss.gameName);
			Directory.CreateDirectory(serverDir);

			string serverConfig = Path.Combine(serverDir, "config.json");
			ss.Create(serverConfig);

			string gameName = MyServer.ReplaceStr(ss.gameName);
			string args = string.Format("-batchmode startwithargs " +
					"{0}#{1}#{2}",
					gameName,
					ss.gameMode,
					SteamMgr.steamId);

            string path = string.Empty;
			System.Diagnostics.Process p = new System.Diagnostics.Process();
			switch (Application.platform)
			{
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
					{
						path = Path.Combine(srvRoot, "PE_Server.app/Contents/MacOS/PE_Server");
						p.StartInfo.FileName = path;
						p.StartInfo.Arguments = args;
					}
					break;

				case RuntimePlatform.LinuxPlayer:
					{
						path = Path.Combine(srvRoot, "PE_Server.x86_64");
						p.StartInfo.FileName = path;
						p.StartInfo.Arguments = args;
					}
					break;

				default:
					{
						path = Path.Combine(srvRoot, "PE_Server.exe");
						p.StartInfo.FileName = path;
						p.StartInfo.Arguments = args;
					}
					break;
			}

			if (!File.Exists(path))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000061) + path);
				Debug.LogError(path + " does not exists.");
				return false;
			}

			if (p.Start())
			{
				LocalIp = "127.0.0.1";
				LocalPort = serverport;
				LocalPwd = ss.gamePassword;
				LocalName = ss.gameName;
				LocalHost = null;

				OnServerHost();
				return true;
			}
		}
#endif
		return false;
	}

	public static bool StartCustomServer(MyServer srv)
	{
		if (LoadServer.ServerList.Contains(srv))
		{
			Debug.Log("servername already existed!");
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000499));
			return false;
		}

		string serverRoot = Path.Combine(Environment.CurrentDirectory, "Server");
		Directory.CreateDirectory(serverRoot);

		string serverPath = Path.Combine(serverRoot, "ServerData");
		Directory.CreateDirectory(serverPath);

		string gameDir = Path.Combine(serverPath, "CustomGames");
		Directory.CreateDirectory(gameDir);
		string serverDir = Path.Combine(gameDir, srv.gameName);
		Directory.CreateDirectory(serverDir);

		string srvScenarioPath = Path.Combine(serverDir, "Scenario");
		Directory.CreateDirectory(srvScenarioPath);

		string srvWorldPath = Path.Combine(serverDir, "Worlds");
		Directory.CreateDirectory(srvWorldPath);

		string srvForceXml = Path.Combine(srvScenarioPath, "ForceSettings.xml");
		string srvWorldXml = Path.Combine(srvScenarioPath, "WorldSettings.xml");
		string srvUidFile = Path.Combine(serverDir, "MAP.uid");

		string customRoot = Path.Combine(Environment.CurrentDirectory, "CustomGames");
		string gamePath = Path.Combine(customRoot, Pathea.PeGameMgr.gameName);
		if (!Directory.Exists(gamePath))
		{
			Debug.LogErrorFormat("Invalide custom game path:{0}", gamePath);
			return false;
		}

		string scenarioPath = Path.Combine(gamePath, "Scenario");
		string forceXml = Path.Combine(scenarioPath, "ForceSettings.xml");
		string worldXml = Path.Combine(scenarioPath, "WorldSettings.xml");
		string uidFile = Path.Combine(gamePath, "MAP.uid");

		if (!File.Exists(uidFile))
		{
			Debug.LogErrorFormat("{0} does not exist", uidFile);
			return false;
		}

		if (!File.Exists(forceXml))
		{
			Debug.LogErrorFormat("{0} does not exist", forceXml);
			return false;
		}

		if (!File.Exists(worldXml))
		{
			Debug.LogErrorFormat("{0} does not exist", worldXml);
			return false;
		}

		string worldPath = Path.Combine(gamePath, "Worlds");
		if (!Directory.Exists(worldPath))
		{
			Debug.LogErrorFormat("Invalide custom worlds path:{0}", worldPath);
			return false;
		}

		DirectoryInfo rootDir = new DirectoryInfo(worldPath);
		DirectoryInfo[] dirs = rootDir.GetDirectories();
		if (dirs.Length <= 0)
		{
			Debug.LogErrorFormat("No worlds exist");
			return false;
		}

		for (int i = 0; i < dirs.Length; i++)
		{
			string worldDir = Path.Combine(worldPath, dirs[i].Name);
			string worldEntityXml = Path.Combine(worldDir, "WorldEntity.xml");
			if (!File.Exists(worldEntityXml))
			{
				Debug.LogErrorFormat("Invalide file:{0}", worldEntityXml);
				return false;
			}

			string srvWorldDir = Path.Combine(srvWorldPath, dirs[i].Name);
			Directory.CreateDirectory(srvWorldDir);
			string srvWorldEntityXml = Path.Combine(srvWorldDir, "WorldEntity.xml");

			File.Copy(worldEntityXml, srvWorldEntityXml, true);
		}

		File.Copy(forceXml, srvForceXml, true);
		File.Copy(worldXml, srvWorldXml, true);
		File.Copy(uidFile, srvUidFile, true);

		int serverPort = uLink.NetworkUtility.FindAvailablePort(9900, 9915);
		if (!uLink.NetworkUtility.IsPortAvailable(serverPort))
		{
			MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000498),serverPort));
			return false;
		}

        string serverConfig = Path.Combine(serverDir, "config.json");
		srv.Create(serverConfig);

		string gameName = MyServer.ReplaceStr(srv.gameName);
		string args = string.Format("-batchmode startwithargs " +
				"{0}#{1}#{2}",
				gameName,
				srv.gameMode,
				SteamMgr.steamId);

        string path = string.Empty;
		System.Diagnostics.Process p = new System.Diagnostics.Process();
		switch (Application.platform)
		{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				{
					path = Path.Combine(serverRoot, "PE_Server.app/Contents/MacOS/PE_Server");
					p.StartInfo.FileName = path;
					p.StartInfo.Arguments = args;
				}
				break;

			case RuntimePlatform.LinuxPlayer:
				{
					path = Path.Combine(serverRoot, "PE_Server.x86_64");
					p.StartInfo.FileName = path;
					p.StartInfo.Arguments = args;
				}
				break;

			default:
				{
					path = Path.Combine(serverRoot, "PE_Server.exe");
					p.StartInfo.FileName = path;
					p.StartInfo.Arguments = args;
				}
				break;
		}

		if (!File.Exists(path))
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000061) + path);
			Debug.LogError(path + " does not exists.");
			return false;
		}


		if (p.Start())
		{
			LocalIp = "127.0.0.1";
			LocalPort = serverPort;
			LocalPwd = srv.gamePassword;
			LocalName = srv.gameName;
			LocalHost = null;

			OnServerHost();
			return true;
		}

		return false;
	}

	public static bool CreateNewServer(MyServer ss)
	{
#if SteamVersion
		//to do--check myServer is new
		if (LoadServer.ServerList.Contains(ss))
		{
			Debug.Log("servername already existed!");
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000499));
			return false;
		}

		//to do--start server
		if (ss != null)
		{
			int serverport = uLink.NetworkUtility.FindAvailablePort(9900, 9915);
			if (!uLink.NetworkUtility.IsPortAvailable(serverport))
			{
				MessageBox_N.ShowOkBox(string.Format(PELocalization.GetString(8000498),serverport));
				return false;
			}

			string srvRoot = Path.Combine(Environment.CurrentDirectory, "Server");
			Directory.CreateDirectory(srvRoot);

			string srvDataPath = Path.Combine(srvRoot, "ServerData");
			Directory.CreateDirectory(srvDataPath);

			string srvGamePath = Path.Combine(srvDataPath, "CommonGames");
			Directory.CreateDirectory(srvGamePath);
			string serverDir = Path.Combine(srvGamePath, ss.gameName);
			Directory.CreateDirectory(serverDir);

			string serverConfig = Path.Combine(serverDir, "config.json");
			ss.Create(serverConfig);

			string gameName = MyServer.ReplaceStr(ss.gameName);
			string args = string.Format("-batchmode startwithargs " +
					"{0}#{1}#{2}",
					gameName,
					ss.gameMode,
					SteamMgr.steamId);

            string path = string.Empty;
			System.Diagnostics.Process p = new System.Diagnostics.Process();
			switch (Application.platform)
			{
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
					{
                        path = Path.Combine(srvRoot, "PE_Server.app/Contents/MacOS/PE_Server");
                        p.StartInfo.FileName = path;
                        p.StartInfo.Arguments = args;
                    }
                    break;

				case RuntimePlatform.LinuxPlayer:
					{
                        path = Path.Combine(srvRoot, "PE_Server.x86_64");
                        p.StartInfo.FileName = path;
                        p.StartInfo.Arguments = args;
                    }
					break;

				default:
					{
						path = Path.Combine(srvRoot, "PE_Server.exe");
						p.StartInfo.FileName = path;
						p.StartInfo.Arguments = args;
					}
					break;
			}

			if (!File.Exists(path))
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000061) + path);
				Debug.LogError(path + " does not exists.");
				return false;
			}

            if (p.Start())
			{
				LocalIp = "127.0.0.1";
				LocalPort = serverport;
				LocalPwd = ss.gamePassword;
				LocalName = ss.gameName;
				LocalHost = null;

				OnServerHost();
				return true;
			}
		}
#endif
        return false;
    }
}
