using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

using Mono.Data.SqliteClient;


public class LoadServer
{
    public static List<string> ServerNames = new List<string>();
    public static List<MyServer> ServerList = new List<MyServer>();

    public static event Action<string, int> AddServerEventHandler;

    public static void Start()
    {
        LoadServers();
    }

    static void LoadServers(string path)
    {
        string[] serverNames = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
        foreach (string name in serverNames)
        {
            MyServer server = new MyServer();
            server.Read(name);
            ServerNames.Add(server.gameName);
            ServerList.Add(server);
        }
    }

    public static void LoadServers()
    {
#if SteamVersion
        ServerNames.Clear();
        ServerList.Clear();

        string srvDir = Path.Combine(Environment.CurrentDirectory, "Server");
        Directory.CreateDirectory(srvDir);
        string srvDataDir = Path.Combine(srvDir, "ServerData");
        Directory.CreateDirectory(srvDataDir);

        string srvCommonDir = Path.Combine(srvDataDir, "CommonGames");
        Directory.CreateDirectory(srvCommonDir);
        LoadServers(srvCommonDir);

        string srvCustomDir = Path.Combine(srvDataDir, "CustomGames");
        Directory.CreateDirectory(srvCustomDir);
        LoadServers(srvCustomDir);
#endif
    }

    public static bool AddServer(string gameName, int sceneMode)
    {
#if SteamVersion
        string srvDir = Path.Combine(Environment.CurrentDirectory, "Server");
        Directory.CreateDirectory(srvDir);
        string srvDataDir = Path.Combine(srvDir, "ServerData");
        Directory.CreateDirectory(srvDataDir);

        string srvGameDir = string.Empty;
        Pathea.PeGameMgr.ESceneMode mode = (Pathea.PeGameMgr.ESceneMode)sceneMode;
        if (mode == Pathea.PeGameMgr.ESceneMode.Custom)
        {
            srvGameDir = Path.Combine(srvDataDir, "CustomGames");
            Directory.CreateDirectory(srvGameDir);
        }
        else
        {
            srvGameDir = Path.Combine(srvDataDir, "CommonGames");
            Directory.CreateDirectory(srvGameDir);
        }

        string gameDir = Path.Combine(srvGameDir, gameName);
        Directory.CreateDirectory(gameDir);
        string confPath = Path.Combine(gameDir, "config.json");
        if (File.Exists(confPath))
        {
            MyServer server = ServerList.Find(iter => iter.gameName.Equals(gameName) && iter.gameMode == sceneMode);
            if (null == server)
            {
                server = new MyServer();
                server.Read(confPath);
                ServerNames.Add(server.gameName);
                ServerList.Add(server);
            }

            OnServerAdd(gameName, sceneMode);
            return true;
        }
#endif
        return false;
    }

    public static bool DeleteServer(string serverName, int sceneMode)
    {
#if SteamVersion
        if (!ServerNames.Contains(serverName))
            return false;

        int index = ServerList.FindIndex(iter => iter.gameName.Equals(serverName) && iter.gameMode == sceneMode);
        if (-1 == index)
            return false;

        string srvDir = Path.Combine(Environment.CurrentDirectory, "Server");
        Directory.CreateDirectory(srvDir);
        string srvDataDir = Path.Combine(srvDir, "ServerData");
        Directory.CreateDirectory(srvDataDir);

        string srvGameDir = string.Empty;
        if (sceneMode == (int)Pathea.PeGameMgr.ESceneMode.Custom)
        {
            srvGameDir = Path.Combine(srvDataDir, "CustomGames");
            Directory.CreateDirectory(srvGameDir);
        }
        else
        {
            srvGameDir = Path.Combine(srvDataDir, "CommonGames");
            Directory.CreateDirectory(srvGameDir);
        }

        string path = Path.Combine(srvGameDir, serverName);
        if (Directory.Exists(path))
            Directory.Delete(path, true);

        ServerList.RemoveAt(index);
        ServerNames.Remove(serverName);
#endif
        return true;
    }


    public static bool Exist(string serverName)
    {
        if (ServerNames.Contains(serverName))
            return true;

        return false;
    }

    public static MyServer GetServer(string serverName, int gameMode)
    {
        return ServerList.Find(it => it.gameName == serverName && it.gameMode == gameMode);
    }

    static void OnServerAdd(string srvName, int sceneMode)
    {
        if (null != AddServerEventHandler)
            AddServerEventHandler(srvName, sceneMode);
    }
}