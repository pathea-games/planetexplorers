using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class ClientConfig
{
	public static string LobbyIP = @"74.81.173.196";
	public static string ProxyIP = @"119.28.5.150";
	public static int LobbyPort = 12534;
	public static int ProxyPort = 12535;

	public static void InitClientConfig()
	{
#if SteamVersion
		string configPath = Path.Combine(GameConfig.PEDataPath, "ConfigFiles");
		Directory.CreateDirectory(configPath);
		string configName = Path.Combine(configPath, "ClientConfig.conf");

		try
		{
			ReadConfig(configName);
		}
		catch (Exception e)
		{
			LogManager.Warning("Incompatible config file, default config file created.", e.Source, e.StackTrace);
			CreateConfig(configName);
		}
#endif
	}

	static void ReadConfig(string configFile)
	{
		string configData = File.ReadAllText(configFile);
		Jboy.JsonReader reader = new Jboy.JsonReader(configData);
		reader.ReadObjectStart();
		
		reader.ReadPropertyName("LobbyIP");
		LobbyIP = Jboy.Json.ReadObject<string>(reader);
		
		reader.ReadPropertyName("LobbyPort");
		LobbyPort = Jboy.Json.ReadObject<int>(reader);
		
		reader.ReadPropertyName("ProxyIP");
		ProxyIP = Jboy.Json.ReadObject<string>(reader);
		
		reader.ReadPropertyName("ProxyPort");
		ProxyPort = Jboy.Json.ReadObject<int>(reader);
		
		reader.ReadObjectEnd();
		reader.Close();
	}

	static void CreateConfig(string configFile)
	{
		Jboy.JsonWriter writer = new Jboy.JsonWriter(true, true, 2);
		writer.WriteObjectStart();
		
		writer.WritePropertyName("LobbyIP");
		Jboy.Json.WriteObject(LobbyIP, writer);
		
		writer.WritePropertyName("LobbyPort");
		Jboy.Json.WriteObject(LobbyPort, writer);
		
		writer.WritePropertyName("ProxyIP");
		Jboy.Json.WriteObject(ProxyIP, writer);
		
		writer.WritePropertyName("ProxyPort");
		Jboy.Json.WriteObject(ProxyPort, writer);
		
		writer.WriteObjectEnd();

		File.WriteAllText(configFile, writer.ToString());
	}
}
