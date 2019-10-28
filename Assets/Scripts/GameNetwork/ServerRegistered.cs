using UnityEngine;
using System;

public class ServerRegistered
{
    internal long ServerUID;
	internal int ServerID;
	internal int LimitedConn;
	internal int CurConn;
	internal int ServerStatus;
	internal int GameType;
	internal int GameMode;
	internal int PasswordStatus;
	internal int Ping;
	internal int Port;
    internal string ServerVersion;
    internal string ServerName;
	internal string ServerMasterAccount;
	internal string ServerMasterName;
    internal string IPAddress;
    internal string UID;
    internal string MapName;
    internal bool UseProxy;
	internal bool IsLan;

    internal string QueryInformation
    {
        get
        {
            return ServerID.ToString() + ServerName.ToLower() + ServerMasterName.ToLower();
        }
    }

	public override bool Equals(object obj)
	{
		if (!(obj is ServerRegistered))
			return false;

		return this.ServerID.Equals(((ServerRegistered)obj).ServerID);
	}

    public override int GetHashCode()
    {
        return ServerID;
    }

    public override string ToString()
    {
        return string.Format("name:{0}, id:{1}, mode:{2}, type:{3}, master:{4} proxy:{5}", ServerName, ServerID, GameMode, GameType, ServerMasterName, UseProxy);
    }

    public virtual void AnalyseServer(uLink.HostData data, bool isLan)
	{
		try
		{
            IPAddress = data.ipAddress;
            Port = data.port;
            LimitedConn = data.playerLimit;
            CurConn = data.connectedPlayers;
            Ping = data.ping;
            ServerName = data.gameName;
            UseProxy = data.useProxy;
            IsLan = isLan;

            string[] args = data.comment.Split(new char[] { ',' });

            int.TryParse(args[0], out ServerStatus);
            int.TryParse(args[1], out GameMode);
            int.TryParse(args[2], out GameType);
            ServerMasterName = MyServer.RecoverStr(args[3]);
            ServerVersion = args[4];
            int.TryParse(args[5], out PasswordStatus);
            int.TryParse(args[6], out ServerID);
            long.TryParse(args[7], out ServerUID);

            if (GameMode == (int)Pathea.PeGameMgr.ESceneMode.Custom)
            {
                UID = args[8];
                MapName = args[9];
            }
        }
		catch (Exception e)
		{
			if (LogFilter.logError) Debug.LogWarningFormat("{0}\r\n{1}", e.Message, e.StackTrace);
		}
	}

	public void AnalyseServer(uLobby.ServerInfo data)
	{
        try
        {
            IPAddress = data.host;
            Port = data.port;

            uLink.BitStream stream = data.data.GetRemainingBitStream();
            LimitedConn = stream.Read<int>();
            CurConn = stream.Read<int>();
            ServerStatus = stream.Read<int>();
            GameMode = stream.Read<int>();
            GameType = stream.Read<int>();
            ServerName = stream.Read<string>();
            ServerMasterName = stream.Read<string>();
            ServerVersion = stream.Read<string>();
            PasswordStatus = stream.Read<int>();
            ServerID = stream.Read<int>();
            ServerUID = stream.Read<long>();

            if (GameMode == (int)Pathea.PeGameMgr.ESceneMode.Custom)
            {
                UID = stream.Read<string>();
                MapName = stream.Read<string>();
            }

            UseProxy = false;
            IsLan = false;
        }
        catch (Exception e)
        {
			if (LogFilter.logError) Debug.LogWarningFormat("{0}\r\n{1}", e.Message, e.StackTrace);
		}
	}
}

public class ProxyServerRegistered : ServerRegistered
{
	public uLink.HostData ProxyServer;

	public override void AnalyseServer(uLink.HostData data, bool isLan)
	{
		base.AnalyseServer(data, isLan);
		ProxyServer = data;
        UseProxy = true;
	}
}
