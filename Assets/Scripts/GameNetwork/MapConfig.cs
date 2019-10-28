using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class MapsConfig
{
	public static MapsConfig Self;
	
	[XmlArrayItem]
	public List<MapConfig> PatheaMapConfig;
	
	public static void InitMapConfig()
	{
#if SteamVersion
		string filePath = GameConfig.PEDataPath + "ConfigFiles";
		Directory.CreateDirectory(filePath);
		filePath += "/MapConfig.xml";
		
		if (File.Exists(filePath))
		{
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				XmlSerializer serialize = new XmlSerializer(typeof(MapsConfig));
				Self = serialize.Deserialize(fs) as MapsConfig;
			}
		}
		else
		{
			using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				XmlSerializer serialize = new XmlSerializer(typeof(MapsConfig));
				Self = new MapsConfig();
				Self.PatheaMapConfig = new List<MapConfig>();
				
				MapConfig config = new MapConfig();
				config.MapName = "PatheaMap";
				config.MapDescription = "OH, Come On";
				config.GameType = new string[] { "Cooperation", "VS", "Survival" };
				config.GameMode = new string[] { "Story", "Adventure", "Build", "Custom" };
				config.MapTeamNum = new string[] { "1", "2", "3", "4" };
				config.MapCampBalance = new string[] { "yes", "no" };
				config.MapAI = new string[] { "yes", "no" };
				config.MapTerrainType = new string[] {"Grassland", "Forest", "Desert","Redstone","Rainforest","Mountain","Swamp","Crater"};
				config.MapWeatherType = new string[] { "Random", "Dry", "Temperate", "Wet" };                  
                                                                                                               
				Self.PatheaMapConfig.Add(config);
				serialize.Serialize(fs, Self);
			}
		}
#endif
	}
}

public class MapConfig
{
	/// <summary>
	/// The name of the map.
	/// </summary>
	public string MapName;

	/// <summary>
	/// 0 = Coperation
	/// 1 = VS
	/// </summary>
	public string[] GameType;
	
	/// <summary>
	/// The map mode.
    /// 0 = Adventure
    /// 1 = Build
	/// </summary>
	public string[] GameMode;
	
	/// <summary>
	/// The map team number.
	/// </summary>
	public string[] MapTeamNum;
	
	/// <summary>
	/// The camp balance.
	/// </summary>
	public string[] MapCampBalance;
	
	/// <summary>
	/// The type of the map terrain.
    /// 0 = grassland
    /// 1 = forest
    /// 2 = desert
    /// 3 = redStone
	/// </summary>
	public string[] MapTerrainType;

	/// <summary>
	/// The type of the map weather
	/// </summary>
	public string[] MapWeatherType;
	
	/// <summary>
	/// The map AI.
	/// 0 = no
	/// 1 = yes
	/// </summary>
	public string[] MapAI;
	
	/// <summary>
	/// The map description.
	/// </summary>
	public string MapDescription;
}
