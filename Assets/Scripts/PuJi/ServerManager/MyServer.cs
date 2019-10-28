using System;
using System.Collections.Generic;
using UnityEngine;

public class MyServer
{
    public string gameName;  //0
    public string masterRoleName;
    public string gamePassword;//1
    public string mapName;//4
    public int gameMode;//advernture or build   3
    public int gameType;//cooperation or vs   2
    public string seedStr; //7
    public int teamNum;//6
    public int numPerTeam;// teamNum*PlayerNo 5 
    public int terrainType;
    public int vegetationId;
    public int sceneClimate;
    public bool monsterYes;
    public bool isPrivate;
    public bool useSkillTree;
	public bool scriptsAvailable;

	//a0.83
	public bool unlimitedRes;
    public int mapSize;
    public int riverDensity;
    public int riverWidth;

    //a0.87
    public int terrainHeight;

    public bool proxyServer;
    public int dropDeadPercent;
    public string uid;

    //a0.95
    public int plainHeight;
    public int flatness;
    public int bridgeMaxHeight;

	
	//b0.72
	public int AICount;
	public int allyCount{
		set{AICount= value-1;}
		get{return AICount+1;}
	}

    public MyServer()
    {

    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        MyServer o = obj as MyServer;
        if (o.gameName == this.gameName)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public override int GetHashCode()
    {
        return gameName.GetHashCode();
    }

	public void Read(string configFile)
	{
		string configData = System.IO.File.ReadAllText(configFile);
		Jboy.JsonReader reader = new Jboy.JsonReader(configData);

		try
		{
			object propertyName;
			Jboy.JsonToken token = Jboy.JsonToken.None;
			if (Jboy.JsonToken.ObjectStart == (token = reader.Read(out propertyName)))
			{
				while (Jboy.JsonToken.ObjectEnd != (token = reader.Read(out propertyName)))
				{
					if (token != Jboy.JsonToken.PropertyName)
						continue;

					if (propertyName.Equals("ServerName"))
					{
						gameName = Jboy.Json.ReadObject<string>(reader);
						continue;
					}

					if (propertyName.Equals("MasterRoleName"))
					{
						masterRoleName = Jboy.Json.ReadObject<string>(reader);
						continue;
					}

					if (propertyName.Equals("Password"))
					{
						gamePassword = Jboy.Json.ReadObject<string>(reader);
						continue;
					}

					if (propertyName.Equals("MapName"))
					{
						mapName = Jboy.Json.ReadObject<string>(reader);
						continue;
					}

					if (propertyName.Equals("GameMode"))
					{
						gameMode = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("GameType"))
					{
						gameType = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("MapSeed"))
					{
						seedStr = Jboy.Json.ReadObject<string>(reader);
						continue;
					}

					if (propertyName.Equals("TeamNum"))
					{
						teamNum = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("NumPerTeam"))
					{
						numPerTeam = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("TerrainType"))
					{
						terrainType = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("VegetationType"))
					{
						vegetationId = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("ClimateType"))
					{
						sceneClimate = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("MonsterYes"))
					{
						monsterYes = Jboy.Json.ReadObject<bool>(reader);
						continue;
					}

					if (propertyName.Equals("IsPrivate"))
					{
						isPrivate = Jboy.Json.ReadObject<bool>(reader);
						continue;
					}

					if (propertyName.Equals("ProxyServer"))
					{
						proxyServer = Jboy.Json.ReadObject<bool>(reader);
						continue;
					}

					if (propertyName.Equals("UnlimitedRes"))
					{
						unlimitedRes = Jboy.Json.ReadObject<bool>(reader);
						continue;
					}

					if (propertyName.Equals("TerrainHeight"))
					{
						terrainHeight = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("MapSize"))
					{
						mapSize = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("RiverDensity"))
					{
						riverDensity = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("RiverWidth"))
					{
						riverWidth = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					//a0.95
					if (propertyName.Equals("PlainHeight"))
					{
						plainHeight = Jboy.Json.ReadObject<int>(reader);
						continue;
					}
					if (propertyName.Equals("Flatness"))
					{
						flatness = Jboy.Json.ReadObject<int>(reader);
						continue;
					}
					if (propertyName.Equals("BridgeMaxHeight"))
					{
						bridgeMaxHeight = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("AICount"))
					{
						AICount = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

					if (propertyName.Equals("UseSkillTree"))
					{
						useSkillTree = Jboy.Json.ReadObject<bool>(reader);
						continue;
					}

					if(propertyName.Equals("DropDeadPercent"))
					{
						dropDeadPercent = Jboy.Json.ReadObject<int>(reader);
						continue;
					}

                    if (propertyName.Equals("UID"))
                    {
                        uid = Jboy.Json.ReadObject<string>(reader);
                        continue;
                    }

					if (propertyName.Equals("ScriptsAvailable"))
					{
						scriptsAvailable = Jboy.Json.ReadObject<bool>(reader);
						continue;
					}
				}
			}
		}
		catch (Exception e)
		{
			if (LogFilter.logError) Debug.LogErrorFormat("Read config file failed.\r\n{0}\r\n{1}\r\n{2}", configFile, e.Message, e.StackTrace);
		}
		finally
		{
			reader.Close();
		}
	}

	public void Create(string configFile)
	{
		Jboy.JsonWriter writer = new Jboy.JsonWriter(true, true, 8);

		writer.WriteObjectStart();

		writer.WritePropertyName("ServerName");
		Jboy.Json.WriteObject(gameName, writer);
		writer.WritePropertyName("MasterRoleName");
		Jboy.Json.WriteObject(masterRoleName, writer);
		writer.WritePropertyName("Password");
		Jboy.Json.WriteObject(gamePassword, writer);
		writer.WritePropertyName("MapName");
		Jboy.Json.WriteObject(mapName, writer);
		writer.WritePropertyName("GameMode");
		Jboy.Json.WriteObject(gameMode, writer);
		writer.WritePropertyName("GameType");
		Jboy.Json.WriteObject(gameType, writer);
		writer.WritePropertyName("MapSeed");
		Jboy.Json.WriteObject(seedStr, writer);
		writer.WritePropertyName("TeamNum");
		Jboy.Json.WriteObject(teamNum, writer);
		writer.WritePropertyName("NumPerTeam");
		Jboy.Json.WriteObject(numPerTeam, writer);
		writer.WritePropertyName("TerrainType");
		Jboy.Json.WriteObject(terrainType, writer);
		writer.WritePropertyName("VegetationType");
		Jboy.Json.WriteObject(vegetationId, writer);
		writer.WritePropertyName("ClimateType");
		Jboy.Json.WriteObject(sceneClimate, writer);
		writer.WritePropertyName("MonsterYes");
		Jboy.Json.WriteObject(monsterYes, writer);
		writer.WritePropertyName("IsPrivate");
		Jboy.Json.WriteObject(isPrivate, writer);
		writer.WritePropertyName("ProxyServer");
		Jboy.Json.WriteObject(proxyServer, writer);
		writer.WritePropertyName("UnlimitedRes");
		Jboy.Json.WriteObject(unlimitedRes, writer);
		writer.WritePropertyName("TerrainHeight");
		Jboy.Json.WriteObject(terrainHeight, writer);
		writer.WritePropertyName("MapSize");
		Jboy.Json.WriteObject(mapSize, writer);
		writer.WritePropertyName("RiverDensity");
		Jboy.Json.WriteObject(riverDensity, writer);
		writer.WritePropertyName("RiverWidth");
		Jboy.Json.WriteObject(riverWidth, writer);
		//a0.95
		writer.WritePropertyName("PlainHeight");
		Jboy.Json.WriteObject(plainHeight, writer);
		writer.WritePropertyName("Flatness");
		Jboy.Json.WriteObject(flatness, writer);
		writer.WritePropertyName("BridgeMaxHeight");
		Jboy.Json.WriteObject(bridgeMaxHeight, writer);
		writer.WritePropertyName("AICount");
		Jboy.Json.WriteObject(AICount, writer);

		writer.WritePropertyName("UseSkillTree");
		Jboy.Json.WriteObject(useSkillTree, writer);
		writer.WritePropertyName("DropDeadPercent");
		Jboy.Json.WriteObject(dropDeadPercent, writer);
        writer.WritePropertyName("UID");
        Jboy.Json.WriteObject(uid, writer);
		writer.WritePropertyName("ScriptsAvailable");
		Jboy.Json.WriteObject(scriptsAvailable, writer);

		writer.WriteObjectEnd();

		System.IO.File.WriteAllText(configFile, writer.ToString());
	}

    public MyServer ReplaceStrForTrans()
    {
        MyServer ms = new MyServer();

        ms.gameName = ReplaceStr(gameName);
        ms.masterRoleName = ReplaceStr(masterRoleName);
        ms.gamePassword = ReplaceStr(gamePassword);
        ms.mapName = ReplaceStr(mapName);
		ms.seedStr = ReplaceStr(seedStr);
        ms.gameMode = gameMode;
        ms.gameType = gameType;
        ms.teamNum = teamNum;
        ms.numPerTeam = numPerTeam;
        ms.terrainType = terrainType;
        ms.vegetationId = vegetationId;
        ms.sceneClimate = sceneClimate;
        ms.monsterYes = monsterYes;
        ms.isPrivate = isPrivate;
		ms.unlimitedRes = unlimitedRes;
        ms.terrainHeight = terrainHeight;
		ms.mapSize = mapSize;
		ms.riverDensity = riverDensity;
		ms.riverWidth = riverWidth;
		//a0.95
		ms.plainHeight = plainHeight;
		ms.flatness = flatness;
		ms.bridgeMaxHeight = bridgeMaxHeight;
		ms.allyCount = allyCount;

		ms.useSkillTree = useSkillTree;
		ms.dropDeadPercent = dropDeadPercent;
		ms.scriptsAvailable = scriptsAvailable;
		return ms;
    }

	public static string RecoverStr(string original)
	{
		if (string.IsNullOrEmpty(original))
			return string.Empty;

		if (original.Contains("@++@"))
			original = original.Replace("@++@", " ");

		if (original.Contains("@--@"))
			original = original.Replace("@--@", ",");

		if (original.Contains("@=@"))
			original = original.Replace("@=@", "`");

		if (original.Contains("@+@"))
			original = original.Replace("@+@", "|");

		if (original.Contains("@-@"))
			original = original.Replace("@-@", "#");

		return original;
	}

    public static string ReplaceStr(string original) {
        if (original == "" || original == null) {
            return original;
        }
        if (original.Contains(" "))
            original = original.Replace(" ", "@++@");

        if (original.Contains(","))
            original = original.Replace(",", "@--@");

        if (original.Contains("`"))
            original = original.Replace("`", "@=@");

        if (original.Contains("|"))
            original = original.Replace("|", "@+@");

        if (original.Contains("#"))
            original = original.Replace("#", "@-@");
        return original;
    }


    public List<string> ToServerDataItem()
    {
        List<string> serverItem = new List<string>();
        serverItem.Add(gameName);
        serverItem.Add(((Pathea.PeGameMgr.EGameType)gameType).ToString());
        serverItem.Add(AdventureOrBuild());
        if ((Pathea.PeGameMgr.EGameType)gameType == Pathea.PeGameMgr.EGameType.Survive)
            serverItem.Add(numPerTeam.ToString());
        else
            serverItem.Add((numPerTeam * teamNum).ToString());
        serverItem.Add(teamNum.ToString());
        serverItem.Add(seedStr);

        return serverItem;
    }

    public List<string> ToServerInfo()
    {
        List<string> serverItem = new List<string>();
        serverItem.Add(gameName);
        serverItem.Add(gamePassword);
        serverItem.Add(((Pathea.PeGameMgr.EGameType)gameType).ToString());
        serverItem.Add(AdventureOrBuild());
        serverItem.Add(MonsterOrNot());
        serverItem.Add(mapName);
        serverItem.Add(teamNum.ToString());
        serverItem.Add(numPerTeam.ToString());
        serverItem.Add(seedStr);
        serverItem.Add(MajorBiomaString());
        serverItem.Add(ClimateTypeString());

        return serverItem;
    }


    public string AdventureOrBuild() {
        switch ((Pathea.PeGameMgr.ESceneMode)gameMode)
        {
			case Pathea.PeGameMgr.ESceneMode.Adventure:
				return "Adventure";
			case Pathea.PeGameMgr.ESceneMode.Build:
				return "Build";
			case Pathea.PeGameMgr.ESceneMode.Custom:
				return "Custom";
			case Pathea.PeGameMgr.ESceneMode.Story:
				return "Story";

			default:
				return "Adventure";
		}
    }

    public static Pathea.PeGameMgr.ESceneMode AdventureOrBuild(string mode)
    {
        if (string.IsNullOrEmpty(mode))
            return Pathea.PeGameMgr.ESceneMode.Adventure;

        if (mode.Equals("Adventure"))
            return Pathea.PeGameMgr.ESceneMode.Adventure;
        else if (mode.Equals("Build"))
            return Pathea.PeGameMgr.ESceneMode.Build;
        else if (mode.Equals("Custom"))
            return Pathea.PeGameMgr.ESceneMode.Custom;
        else if (mode.Equals("Story"))
            return Pathea.PeGameMgr.ESceneMode.Story;
        else
            return Pathea.PeGameMgr.ESceneMode.Adventure;
    }

    public string MonsterOrNot() {
        if (monsterYes)
        {
            return "Yes";
        }
        else { 
            return "No"; 
        }
    }

    public string MajorBiomaString() {
        switch (terrainType)
        {
            case (int)RandomMapType.GrassLand:
                return "GrassLand";
            case (int)RandomMapType.Forest:
                return "Forest";
            case (int)RandomMapType.Desert:
                return "Desert";
            case (int)RandomMapType.Redstone:
                return "Redstone";
            case (int)RandomMapType.Rainforest:
                return "Rainforest";
            default:
                return "Grassland";
        }
    }

    public string ClimateTypeString() {
        switch (sceneClimate)
        {
            case (int)ClimateType.CT_Dry:
                return "Dry";
            case (int)ClimateType.CT_Temperate:
                return "Temperate";
            case (int)ClimateType.CT_Wet:
                return "Wet";
            default:
                return "Dry";
        }
    }
}
