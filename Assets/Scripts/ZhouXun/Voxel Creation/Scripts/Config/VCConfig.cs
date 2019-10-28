#define PLANET_EXPLORERS
//#define CONFIG_FOR_OTHER_PROJECT

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if PLANET_EXPLORERS
using Mono.Data.SqliteClient;
#endif

// This class is used to load game configurations into the Voxel Creation System
// (Connect VC with the game)

public static class VCConfig
{
	// Categories Info
	public static Dictionary<EVCCategory, VCCategoryInfo> s_Categories = null;
	
	// Part Types Info
	public static Dictionary<EVCComponent, VCPartTypeInfo> s_PartTypes = null;
	
	// Parts Info
	public static Dictionary<int, VCPartInfo> s_Parts = null;

	// Effect Info
	public static Dictionary<int, VCEffectInfo> s_Effects = null;

	// Scene settings
	public static List<VCESceneSetting> s_EditorScenes = null;
	public static VCESceneSetting FirstSceneSetting
	{
		get
		{
			foreach ( VCESceneSetting scsetting in s_EditorScenes )
			{
				if ( scsetting.m_Category != EVCCategory.cgAbstract )
				{
					return scsetting;
				}
			}
			return null;
		}
	}
	
	// Matter settings
	// Key: ItemIndex for every matter, it should be permanently constant
	public static Dictionary<int, VCMatterInfo> s_Matters = null;
	
	// Paths
	public static string s_MaterialPath;
	public static string s_DecalPath;
	public static string s_IsoPath;
	public static string s_CreationPath;
	public static string s_CreationNetCachePath;
	public static string s_MaterialFileExt = ".vcmat";
	public static string s_DecalFileExt = ".vcdcl";
	public static string s_IsoFileExt = ".vciso";
	public static string s_ObsoleteIsoFileExt = ".peiso";
	public static string s_CreationFileExt = ".vcres";
	public static string s_CreationNetCacheFileExt = ".~vcres";
	
	// Layers & LayerMasks
	public static int s_EditorLayer = 18;
	public static int s_ProductLayer = 19;
	public static int s_WheelLayer = 17;
	public static int s_SceneLayer = 29;
	public static int s_UILayer = 28;
	public static int s_MatGenLayer = 27;
	public static int s_EditorLayerMask = (1 << s_EditorLayer) | (1 << s_SceneLayer);
	public static int s_ProductLayerMask = 1 << s_ProductLayer;
	public static int s_UILayerMask = 1 << s_UILayer;
	public static int s_MatGenLayerMask = 1 << s_MatGenLayer;

	// Common ids
	public static int s_DyeID = 1038;

	public static void InitConfig()
	{
		// Directories
		BuildDirectories();
		
		// Collections
		LoadCategories();
		LoadEditorScenes();
		LoadMatters();
		LoadPartTypes();
		LoadParts();
		LoadEffects();
	}

	private static void BuildDirectories ()
	{
		// pathes
		string mydoc_path = "";
#if PLANET_EXPLORERS
		mydoc_path = GameConfig.GetUserDataPath();
		string folder = GameConfig.VCSystemData;
#else
		mydoc_path = GetUserDataPath();
		string folder = "/VoxelCreationSystem";		
#endif
		s_MaterialPath = mydoc_path + folder + "/Materials/";
		s_DecalPath = mydoc_path + folder + "/Decals/";
		s_IsoPath = mydoc_path + folder + "/Isos/";
		s_CreationPath = mydoc_path + folder + "/Creations/";
		s_CreationNetCachePath = mydoc_path + folder + "/Creations/NetCache/";
		
		if ( !Directory.Exists(VCConfig.s_MaterialPath) )
			Directory.CreateDirectory(VCConfig.s_MaterialPath);
		if ( !Directory.Exists(VCConfig.s_DecalPath) )
			Directory.CreateDirectory(VCConfig.s_DecalPath);
		if ( !Directory.Exists(VCConfig.s_IsoPath) )
			Directory.CreateDirectory(VCConfig.s_IsoPath);
		if ( !Directory.Exists(VCConfig.s_CreationPath) )
			Directory.CreateDirectory(VCConfig.s_CreationPath);
		if ( !Directory.Exists(VCConfig.s_CreationNetCachePath) )
			Directory.CreateDirectory(VCConfig.s_CreationNetCachePath);
	}
	
	private static string GetUserDataPath()
	{
		string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		return path;
	}

	private static void LoadCategories()
	{
		s_Categories = new Dictionary<EVCCategory, VCCategoryInfo> ();
#if PLANET_EXPLORERS
        SqliteDataReader reader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_category");
        while (reader.Read())
        {
			VCCategoryInfo category = new VCCategoryInfo ();
			category.m_Category = (EVCCategory)(Convert.ToInt32(reader.GetString(reader.GetOrdinal("id"))));
			category.m_Name = reader.GetString(reader.GetOrdinal("name"));
			category.m_DefaultPath = reader.GetString(reader.GetOrdinal("default_path"));
			List<string> part_types = VCUtils.ExplodeString(reader.GetString(reader.GetOrdinal("part_types")), ';');
			category.m_PartTypes = new List<EVCComponent> ();
			foreach ( string s in part_types )
			{
				category.m_PartTypes.Add((EVCComponent)(Convert.ToInt32(s)));
			}
            s_Categories.Add(category.m_Category, category);
        }
#endif
	}
	
	private static void LoadEditorScenes()
	{
		s_EditorScenes = new List<VCESceneSetting> ();
#if PLANET_EXPLORERS
        SqliteDataReader reader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_scene");
        while (reader.Read())
        {
			VCESceneSetting scene = new VCESceneSetting ();
			scene.m_Id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			scene.m_ParentId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("parentid")));
			scene.m_Name = reader.GetString(reader.GetOrdinal("name"));
			scene.m_Category = (EVCCategory)(Convert.ToInt32(reader.GetString(reader.GetOrdinal("category"))));
			List<string> parts = null;
			parts = VCUtils.ExplodeString(reader.GetString(reader.GetOrdinal("editorsize")), ';');
			scene.m_EditorSize = new IntVector3 ();
			scene.m_EditorSize.x = Convert.ToInt32(parts[0]);
			scene.m_EditorSize.y = Convert.ToInt32(parts[1]);
			scene.m_EditorSize.z = Convert.ToInt32(parts[2]);
			parts.Clear();
			scene.m_VoxelSize = Convert.ToSingle(reader.GetString(reader.GetOrdinal("voxelsize")));
			parts = VCUtils.ExplodeString(reader.GetString(reader.GetOrdinal("interval")), ';');
			scene.m_MajorInterval = Convert.ToInt32(parts[0]);
			scene.m_MinorInterval = Convert.ToInt32(parts[1]);
			parts.Clear();
			parts = VCUtils.ExplodeString(reader.GetString(reader.GetOrdinal("cost")), ';');
			scene.m_BlockUnit = Convert.ToInt32(parts[0]);
			scene.m_DyeUnit = Convert.ToInt32(parts[1]);
			parts.Clear();
            s_EditorScenes.Add(scene);
        }
#endif
	}
	
	private static void LoadMatters()
	{
		s_Matters = new Dictionary<int, VCMatterInfo> ();
#if PLANET_EXPLORERS
        SqliteDataReader reader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_material ORDER BY cast(sort as int) ASC");
        while (reader.Read())
        {
			VCMatterInfo matter = new VCMatterInfo ();
			matter.ItemIndex = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			matter.Order = Convert.ToInt32(reader.GetString(reader.GetOrdinal("sort")));
			matter.ItemId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemid")));
//			matter.Name = reader.GetString(reader.GetOrdinal("englishname"));
			matter.Attack = Convert.ToSingle(reader.GetString(reader.GetOrdinal("attack")));
			matter.Defence = Convert.ToSingle(reader.GetString(reader.GetOrdinal("defence")));
			matter.Durability = Convert.ToSingle(reader.GetString(reader.GetOrdinal("durability")));
			matter.Hp = Convert.ToSingle(reader.GetString(reader.GetOrdinal("hp")));
			matter.SellPrice = Convert.ToSingle(reader.GetString(reader.GetOrdinal("value")));
			matter.Density = Convert.ToSingle(reader.GetString(reader.GetOrdinal("density")));
			matter.Elasticity = Convert.ToSingle(reader.GetString(reader.GetOrdinal("elasticity")));
			matter.DefaultBumpStrength = Convert.ToSingle(reader.GetString(reader.GetOrdinal("bumpstrength")));
			matter.DefaultSpecularStrength = Convert.ToSingle(reader.GetString(reader.GetOrdinal("specularstrength")));
			matter.DefaultSpecularPower = Convert.ToSingle(reader.GetString(reader.GetOrdinal("specularpower")));
			matter.DefaultTile = Convert.ToSingle(reader.GetString(reader.GetOrdinal("tile")));
			matter.DefaultDiffuseRes = reader.GetString(reader.GetOrdinal("diffusemap"));
			matter.DefaultBumpRes = reader.GetString(reader.GetOrdinal("bumpmap"));
			List<string> parts = null;
			parts = VCUtils.ExplodeString(reader.GetString(reader.GetOrdinal("specularcolor")), ';');
			matter.DefaultSpecularColor.r = (byte)(Convert.ToInt32(parts[0]));
			matter.DefaultSpecularColor.g = (byte)(Convert.ToInt32(parts[1]));
			matter.DefaultSpecularColor.b = (byte)(Convert.ToInt32(parts[2]));
			matter.DefaultSpecularColor.a = (byte)(255);
			parts.Clear();
			parts = VCUtils.ExplodeString(reader.GetString(reader.GetOrdinal("emissivecolor")), ';');
			matter.DefaultEmissiveColor.r = (byte)(Convert.ToInt32(parts[0]));
			matter.DefaultEmissiveColor.g = (byte)(Convert.ToInt32(parts[1]));
			matter.DefaultEmissiveColor.b = (byte)(Convert.ToInt32(parts[2]));
			matter.DefaultEmissiveColor.a = (byte)(255);
			parts.Clear();
			s_Matters.Add(matter.ItemIndex, matter);
        }
#endif
	}
	
	private static void LoadPartTypes()
	{
		s_PartTypes = new Dictionary<EVCComponent, VCPartTypeInfo> ();
#if PLANET_EXPLORERS
		SqliteDataReader reader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_part_type");
		while (reader.Read())
		{
			VCPartTypeInfo ptinfo = new VCPartTypeInfo ();
			ptinfo.m_Type = (EVCComponent)(Convert.ToInt32(reader.GetString(reader.GetOrdinal("id"))));
			ptinfo.m_Name = reader.GetString(reader.GetOrdinal("name"));
			ptinfo.m_ShortName = reader.GetString(reader.GetOrdinal("short_name"));
			ptinfo.m_InspectorRes = reader.GetString(reader.GetOrdinal("inspector"));
			ptinfo.m_RotateMask = Convert.ToInt32(reader.GetString(reader.GetOrdinal("rotmask")));
			s_PartTypes.Add(ptinfo.m_Type, ptinfo);
		}
#endif
	}
	
	private static void LoadParts()
	{
		s_Parts = new Dictionary<int, VCPartInfo> ();
#if PLANET_EXPLORERS
		string sql = " SELECT a.id, a.itemid, a.costcount, a.type, b._engName as name, a.path as respath, " +
					 "        b._iconId as iconpath, b.currency_value as sellprice, a.weight, a.volume, a.mirror_mask, a.symmetric " +
                     " FROM vc_part a, PrototypeItem b" +
					 " WHERE a.itemid = b.id " +
					 " ORDER BY cast(a.type as int) ASC, cast(a.sort as int) ASC";
		
		SqliteDataReader reader = LocalDatabase.Instance.ExecuteQuery(sql);
		while (reader.Read())
		{
			VCPartInfo part = new VCPartInfo ();
			try{
				part.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
				part.m_ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemid")));
				part.m_CostCount = Convert.ToInt32(reader.GetString(reader.GetOrdinal("costcount")));
				part.m_Type = (EVCComponent)(Convert.ToInt32(reader.GetString(reader.GetOrdinal("type"))));
	//			part.m_Name = reader.GetString(reader.GetOrdinal("name"));
				part.m_ResPath = reader.GetString(reader.GetOrdinal("respath"));
				//Debug.Log("[VCPart]Loading "+part.m_ResPath);
				part.m_ResObj = Resources.Load(part.m_ResPath) as GameObject;
				part.m_IconPath = reader.GetString(reader.GetOrdinal("iconpath"));
				part.m_SellPrice = Convert.ToSingle(reader.GetString(reader.GetOrdinal("sellprice")));
				part.m_Weight = Convert.ToSingle(reader.GetString(reader.GetOrdinal("weight")));
				part.m_Volume = Convert.ToSingle(reader.GetString(reader.GetOrdinal("volume")));
				part.m_MirrorMask = Convert.ToInt32(reader.GetString(reader.GetOrdinal("mirror_mask")));
				part.m_Symmetric = Convert.ToInt32(reader.GetString(reader.GetOrdinal("symmetric")));
				s_Parts.Add(part.m_ID, part);	
			}catch(Exception ex){
				Debug.LogWarning("Exception on load parts:"+part.m_ID + "\n" + ex);
			}
		}
#endif
	}

	private static void LoadEffects()
	{
		s_Effects = new Dictionary<int, VCEffectInfo> ();
#if PLANET_EXPLORERS
		SqliteDataReader reader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM vc_effects ORDER BY cast(sort as int) ASC");
		while (reader.Read())
		{
			VCEffectInfo effect = new VCEffectInfo ();
			effect.m_ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			effect.m_ItemID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("itemid")));
			effect.m_Type = (EVCEffect)(Convert.ToInt32(reader.GetString(reader.GetOrdinal("type"))));
			effect.m_Name = reader.GetString(reader.GetOrdinal("name"));
			effect.m_ResPath = reader.GetString(reader.GetOrdinal("respath"));
			effect.m_ResObj = Resources.Load(effect.m_ResPath) as GameObject;
			effect.m_IconPath = reader.GetString(reader.GetOrdinal("iconpath"));
			effect.m_IconTex = Resources.Load(effect.m_IconPath) as Texture2D;
			effect.m_SellPrice = Convert.ToSingle(reader.GetString(reader.GetOrdinal("sellprice")));
			s_Effects.Add(effect.m_ID, effect);
		}
#endif
	}

#if CONFIG_FOR_OTHER_PROJECT
	public static void SaveConfig()
	{
		using ( FileStream fs = new FileStream ("VCConfig.dat", FileMode.Create, FileAccess.Write) )
		{

		}
	}
#endif

//	private static void LoadAnyThingElse()
//	{
//		s_AnyThingElse = new ...;
//#if PLANET_EXPLORERS
//		SqliteDataReader reader = LocalDatabase.Instance.ExecuteQuery("SELECT * FROM ...");
//		while (reader.Read())
//		{
//			
//		}
//#endif
//		
//	}
}
