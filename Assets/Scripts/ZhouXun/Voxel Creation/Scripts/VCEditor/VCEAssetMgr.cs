using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class VCEAssetMgr
{
	// Cons & Des
	public static void Init ()
	{
		LoadMaterials();
		LoadDecals();
	}
	public static void Destroy ()
	{
		if ( s_Materials != null )
		{
			foreach ( KeyValuePair<ulong, VCMaterial> kvp in s_Materials )
			{
				kvp.Value.Destroy();
			}
			s_Materials.Clear();
			s_Materials = null;
		}
		if ( s_TempMaterials != null )
		{
			foreach ( KeyValuePair<ulong, VCMaterial> kvp in s_TempMaterials )
			{
				kvp.Value.Destroy();
			}
			s_TempMaterials.Clear();
			s_TempMaterials = null;
		}
		if ( s_Decals != null )
		{
			foreach ( KeyValuePair<ulong, VCDecalAsset> kvp in s_Decals )
			{
				kvp.Value.Destroy();
			}
			s_Decals.Clear();
			s_Decals = null;
		}
		if ( s_TempDecals != null )
		{
			foreach ( KeyValuePair<ulong, VCDecalAsset> kvp in s_TempDecals )
			{
				kvp.Value.Destroy();
			}
			s_TempDecals.Clear();
			s_TempDecals = null;
		}
	}
		
	// 
	// Load materials work
	//
	// Load materials main
	public static void LoadMaterials ()
	{
		if ( s_Materials == null )
			s_Materials = new Dictionary<ulong, VCMaterial> ();
		if ( s_TempMaterials == null )
			s_TempMaterials = new Dictionary<ulong, VCMaterial> ();
		if ( Directory.Exists(VCConfig.s_MaterialPath) )
		{
			string[] matfiles = Directory.GetFiles(VCConfig.s_MaterialPath, "*" + VCConfig.s_MaterialFileExt);
			if ( matfiles.Length > 0 )
			{
				LoadMaterialFromList(matfiles);
			}
			else
			{
				SendDefaultMaterials();
			}
		}
		else
		{
			Directory.CreateDirectory(VCConfig.s_MaterialPath);
			SendDefaultMaterials();
		}
	}
	// Send some default materials if user's material count is 0
	private static void SendDefaultMaterials ()
	{
		foreach ( KeyValuePair<int, VCMatterInfo> kvp in VCConfig.s_Matters )
		{
			VCMatterInfo matter = kvp.Value;
			VCMaterial vcmat = new VCMaterial ();
			vcmat.m_Name = "Default".ToLocalizationString() + " " + matter.Name;
			vcmat.m_MatterId = matter.ItemIndex;
			vcmat.m_UseDefault = true;
			try
			{
				byte[] buffer = vcmat.Export();
				ulong guid = CRC64.Compute(buffer);
				string sguid = guid.ToString("X").PadLeft(16,'0');
				FileStream fs = new FileStream (VCConfig.s_MaterialPath + sguid + VCConfig.s_MaterialFileExt, FileMode.Create, FileAccess.ReadWrite);
				fs.Write(buffer, 0, buffer.Length);
				fs.Close();
				vcmat.Import(buffer);
			}
			catch (Exception e)
			{
				vcmat.Destroy();
				Debug.LogError("Save material [" + vcmat.m_Name + "] failed ! \r\n" + e.ToString());
				continue;
			}
			if ( s_Materials.ContainsKey(vcmat.m_Guid) )
			{
				s_Materials[vcmat.m_Guid].Destroy();
				s_Materials[vcmat.m_Guid] = vcmat;
			}
			else
			{
				s_Materials.Add(vcmat.m_Guid, vcmat);
			}
		}
	}
	// Read file and add material work
	private static void LoadMaterialFromList (string[] files)
	{
		foreach ( string s in files )
		{
			VCMaterial vcmat = new VCMaterial ();
			try
			{
				FileStream fs = new FileStream (s, FileMode.Open, FileAccess.ReadWrite);
				byte[] buffer = new byte [(int)(fs.Length)];
				fs.Read(buffer, 0, (int)(fs.Length));
				fs.Close();
				vcmat.Import(buffer);
				// Check file valid
				string guid_from_filename = new FileInfo(s).Name.ToUpper();
				string filename_should_be = (vcmat.GUIDString + VCConfig.s_MaterialFileExt).ToUpper();
				if ( guid_from_filename != filename_should_be )
				{
					throw new Exception("The name and GUID doesn't match!");
				}
			}
			catch (Exception e)
			{
				vcmat.Destroy();
				Debug.LogError("Load material [" + s + "] failed ! \r\n" + e.ToString());
				continue;
			}
			if ( s_Materials.ContainsKey(vcmat.m_Guid) )
			{
				s_Materials[vcmat.m_Guid].Destroy();
				s_Materials[vcmat.m_Guid] = vcmat;
			}
			else
			{
				s_Materials.Add(vcmat.m_Guid, vcmat);
			}
		}
	}

	// Clear temporary materials
	public static void ClearTempMaterials()
	{
		if ( s_TempMaterials != null )
		{
			foreach ( KeyValuePair<ulong, VCMaterial> kvp in s_TempMaterials )
			{
				kvp.Value.Destroy();
			}
			s_TempMaterials.Clear();
		}
	}
	
	// Delete material data file
	public static bool DeleteMaterialDataFile(ulong guid)
	{
		string filename = VCConfig.s_MaterialPath + guid.ToString("X").PadLeft(16,'0') + VCConfig.s_MaterialFileExt;
		try
		{
			File.Delete(filename);
		}
		catch (Exception)
		{
			return false;
		}
		return !File.Exists(filename);
	}
	// Create material data file
	public static bool CreateMaterialDataFile(VCMaterial vcmat)
	{
		try
		{
			byte[] buffer = vcmat.Export();
			ulong guid = CRC64.Compute(buffer);
			string sguid = guid.ToString("X").PadLeft(16,'0');
			FileStream fs = new FileStream (VCConfig.s_MaterialPath + sguid + VCConfig.s_MaterialFileExt, FileMode.Create, FileAccess.ReadWrite);
			fs.Write(buffer, 0, buffer.Length);
			fs.Close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
	public static bool DeleteMaterial(ulong guid)
	{
		return s_Materials.Remove(guid) ? DeleteMaterialDataFile(guid) : false;
	}
	public static bool AddMaterialFromTemp(ulong guid)
	{
		if ( s_TempMaterials.ContainsKey(guid) )
		{
			VCMaterial vcmat = s_TempMaterials[guid];
			s_TempMaterials.Remove(guid);
			s_Materials.Add(guid, vcmat);
			return CreateMaterialDataFile(vcmat);
		}
		else
		{
			return false;
		}
	}

	//
	// Load decals work
	//
	// Load decals main
	public static void LoadDecals ()
	{
		if ( s_Decals == null )
			s_Decals = new Dictionary<ulong, VCDecalAsset> ();
		if ( s_TempDecals == null )
			s_TempDecals = new Dictionary<ulong, VCDecalAsset> ();
		if ( Directory.Exists(VCConfig.s_DecalPath) )
		{
			string[] dclfiles = Directory.GetFiles(VCConfig.s_DecalPath, "*" + VCConfig.s_DecalFileExt);
			if ( dclfiles.Length > 0 )
			{
				LoadDecalFromList(dclfiles);
			}
			else
			{
				SendDefaultDecals();
			}
		}
		else
		{
			Directory.CreateDirectory(VCConfig.s_DecalPath);
			SendDefaultDecals();
		}
	}
	// Send some default decals if user's decal count is 0
	private static void SendDefaultDecals ()
	{
		for ( int i = 0; i < 6; ++i )
		{
			TextAsset ta = Resources.Load("Decals/Default" + i.ToString("00")) as TextAsset;
			if ( ta == null )
				continue;
			VCDecalAsset vcdcl = new VCDecalAsset ();
			vcdcl.Import(ta.bytes);
			try
			{
				byte[] buffer = vcdcl.Export();
				ulong guid = CRC64.Compute(buffer);
				string sguid = guid.ToString("X").PadLeft(16,'0');
				FileStream fs = new FileStream (VCConfig.s_DecalPath + sguid + VCConfig.s_DecalFileExt, FileMode.Create, FileAccess.ReadWrite);
				fs.Write(buffer, 0, buffer.Length);
				fs.Close();
			}
			catch (Exception e)
			{
				vcdcl.Destroy();
				Debug.LogError("Save decal [" + vcdcl.GUIDString + "] failed ! \r\n" + e.ToString());
				continue;
			}
			if ( s_Decals.ContainsKey(vcdcl.m_Guid) )
			{
				s_Decals[vcdcl.m_Guid].Destroy();
				s_Decals[vcdcl.m_Guid] = vcdcl;
			}
			else
			{
				s_Decals.Add(vcdcl.m_Guid, vcdcl);
			}
		}
	}
	// Read file and add decal work
	private static void LoadDecalFromList (string[] files)
	{
		foreach ( string s in files )
		{
			VCDecalAsset vcdcl = new VCDecalAsset ();
			try
			{
				FileStream fs = new FileStream (s, FileMode.Open, FileAccess.ReadWrite);
				byte[] buffer = new byte [(int)(fs.Length)];
				fs.Read(buffer, 0, (int)(fs.Length));
				fs.Close();
				vcdcl.Import(buffer);
				// Check file valid
				string guid_from_filename = new FileInfo(s).Name.ToUpper();
				string filename_should_be = (vcdcl.GUIDString + VCConfig.s_DecalFileExt).ToUpper();
				if ( guid_from_filename != filename_should_be )
				{
					throw new Exception("The name and GUID doesn't match!");
				}
			}
			catch (Exception e)
			{
				vcdcl.Destroy();
				Debug.LogError("Load decal [" + s + "] failed ! \r\n" + e.ToString());
				continue;
			}
			if ( s_Decals.ContainsKey(vcdcl.m_Guid) )
			{
				s_Decals[vcdcl.m_Guid].Destroy();
				s_Decals[vcdcl.m_Guid] = vcdcl;
			}
			else
			{
				s_Decals.Add(vcdcl.m_Guid, vcdcl);
			}
		}
	}
	
	// Clear temporary decals
	public static void ClearTempDecals()
	{
		if ( s_TempDecals != null )
		{
			foreach ( KeyValuePair<ulong, VCDecalAsset> kvp in s_TempDecals )
			{
				kvp.Value.Destroy();
			}
			s_TempDecals.Clear();
		}
	}
	
	// Delete decal data file
	public static bool DeleteDecalDataFile(ulong guid)
	{
		string filename = VCConfig.s_DecalPath + guid.ToString("X").PadLeft(16,'0') + VCConfig.s_DecalFileExt;
		try
		{
			File.Delete(filename);
		}
		catch (Exception)
		{
			return false;
		}
		return !File.Exists(filename);
	}
	// Create decal data file
	public static bool CreateDecalDataFile(VCDecalAsset vcdcl)
	{
		try
		{
			byte[] buffer = vcdcl.Export();
			ulong guid = CRC64.Compute(buffer);
			string sguid = guid.ToString("X").PadLeft(16,'0');
			FileStream fs = new FileStream (VCConfig.s_DecalPath + sguid + VCConfig.s_DecalFileExt, FileMode.Create, FileAccess.ReadWrite);
			fs.Write(buffer, 0, buffer.Length);
			fs.Close();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
	public static bool DeleteDecal(ulong guid)
	{
		return s_Decals.Remove(guid) ? DeleteDecalDataFile(guid) : false;
	}
	public static bool AddDecalFromTemp(ulong guid)
	{
		if ( s_TempDecals.ContainsKey(guid) )
		{
			VCDecalAsset vcdcl = s_TempDecals[guid];
			s_TempDecals.Remove(guid);
			s_Decals.Add(guid, vcdcl);
			return CreateDecalDataFile(vcdcl);
		}
		else
		{
			return false;
		}
	}

	public struct IsoFileInfo
	{
		public bool m_IsFolder;
		public string m_Path;
		public string m_Name;
	}

	public static List<IsoFileInfo> SearchIso ( string path, string keyword )
	{
		if ( path == null )
			return new List<IsoFileInfo> ();
		if ( keyword == null )
			keyword = "";

		path = path.Trim();
		keyword = keyword.Trim();
		string pattern = "*" + VCConfig.s_IsoFileExt;

		string[] dirs = null;
		string[] files = null;
		try
		{
			if ( keyword.Length < 1 )
			{
				dirs = Directory.GetDirectories(path, "*");
				files = Directory.GetFiles(path, pattern);
			}
			else
			{
				dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
				files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
			}
		}
		catch
		{
			return new List<IsoFileInfo> ();
		}

		List<IsoFileInfo> retval = new List<IsoFileInfo> ();
		foreach ( string s in dirs )
		{
			string name = new DirectoryInfo(s).Name;
			if ( keyword.Length < 1 || name.IndexOf(keyword) >= 0 )
			{
				IsoFileInfo isof = new IsoFileInfo ();
				isof.m_IsFolder = true;
				isof.m_Name = name;
				isof.m_Path = s;
				retval.Add(isof);
			}
		}
		foreach ( string s in files )
		{
			string name = new FileInfo(s).Name;
			name = name.Substring(0,name.Length - VCConfig.s_IsoFileExt.Length);
			if ( keyword.Length < 1 || name.IndexOf(keyword) >= 0 )
			{
				IsoFileInfo isof = new IsoFileInfo ();
				isof.m_IsFolder = false;
				isof.m_Name = name;
				isof.m_Path = s;
				retval.Add(isof);
			}
		}
		return retval;
	}
	
	// Materials <guid, material>
	public static Dictionary<ulong, VCMaterial> s_Materials = null;
	// Temporary materials in the current loaded iso that not in s_Materials. (Others' material but not mine)
	public static Dictionary<ulong, VCMaterial> s_TempMaterials = null;
	// It's strongly recommand to query a material by calling GetMaterial instead of s_Material[..]
	public static VCMaterial GetMaterial(ulong guid)
	{
		if ( s_Materials.ContainsKey(guid) )
			return s_Materials[guid];
		else if ( s_TempMaterials.ContainsKey(guid) )
			return s_TempMaterials[guid];
		else
			return null;
	}

	// Decal <guid, decal>
	public static Dictionary<ulong, VCDecalAsset> s_Decals = null;
	// Temporary decals in the current loaded iso that not in s_Decals. (Others' decal but not mine)
	public static Dictionary<ulong, VCDecalAsset> s_TempDecals = null;
	// It's strongly recommand to query a decal by calling GetDecal instead of s_Decals[..]
	public static VCDecalAsset GetDecal(ulong guid)
	{
		if ( s_Decals.ContainsKey(guid) )
			return s_Decals[guid];
		else if ( s_TempDecals.ContainsKey(guid) )
			return s_TempDecals[guid];
		else
			return null;
	}
}
