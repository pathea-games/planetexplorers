using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class VAMaterial
{

// Voxel creation material class
//
// VCEditor has a material dictionary for edit work, and for every iso will choose VCIsoData.MAT_ARR_CNT(maximum) of them.
// Iso's materials was saved as VAMaterial[], the index of each material will apply to voxel types.
// When loading an iso, the iso's materials will merge to the editor temporality. ( merge by GUID comparision )
// 

	// Version control
	public const int VERSION = 0x1000;
	
	// Identify
	public ulong m_Guid;
	public string GUIDString { get { return m_Guid.ToString("X").PadLeft(16, '0'); } }
	public string m_Name = "";
	
	// Use game item
	public int m_MatterId;	// index in the config
	private int m_ItemId;
	public int ItemId { get { return m_ItemId; } set { m_ItemId = value; } }
	
	// Flag
	public bool m_UseDefault;
	
	// Properties
	public float m_BumpStrength;
	public Color32 m_SpecularColor;
	public float m_SpecularStrength;
	public float m_SpecularPower;
	public Color32 m_EmissiveColor;
	
	// Texture
	public const int TEX_RESOLUTION = 128;
	public float m_Tile;
	public byte[] m_DiffuseData;
	public byte[] m_BumpData;
	public Texture2D m_DiffuseTex = null;
	public Texture2D m_BumpTex = null;
	public RenderTexture m_Icon = null;
	
	// Destruct
	public void Destroy()
	{
		// Clean up textures
		if ( m_DiffuseTex != null )
		{
			Texture2D.Destroy(m_DiffuseTex);
			m_DiffuseTex = null;
		}
		if ( m_BumpTex != null )
		{
			Texture2D.Destroy(m_BumpTex);
			m_BumpTex = null;
		}
		if ( m_Icon != null )
		{
			RenderTexture.Destroy(m_Icon);
			m_Icon = null;
		}
		// Clean up data
		m_DiffuseData = null;
		m_BumpData = null;
	}
	public void FreeIcon()
	{
		if ( m_Icon != null )
		{
			RenderTexture.Destroy(m_Icon);
			m_Icon = null;
		}
	}

	// I/O
	// From byte buffer
	public void Import(byte[] buffer)
	{
		// Read the buffer
		using ( MemoryStream ms = new MemoryStream (buffer) )
		{
			BinaryReader r = new BinaryReader (ms);
			// Read version
			int version = r.ReadInt32();
			// Version control
			switch ( version )
			{
			case 0x1000:
			{
				// Read name & game mat item index
				m_Name = r.ReadString();
				m_MatterId = r.ReadInt32();
				
				// Find mat item config
//				VCMatterInfo mc = null;
				if ( VCConfig.s_Matters != null && VCConfig.s_Matters.ContainsKey(m_MatterId) )
				{
					//mc = VCConfig.s_Matters[m_MatterId];
				}
				else
				{
                    //Debug.LogError("VCConfig.s_Matters was corrupt.");
                    //break;
				}
				
				// Fetch item id
				m_ItemId = 0;
				
				m_UseDefault = r.ReadBoolean();
				m_UseDefault = false;
				// Use default set
				if ( m_UseDefault )
				{
//					// Load properties
//					m_BumpStrength = mc.DefaultBumpStrength;
//					m_SpecularColor = mc.DefaultSpecularColor;
//					m_SpecularStrength = mc.DefaultSpecularStrength;
//					m_SpecularPower = mc.DefaultSpecularPower;
//					m_EmissiveColor = mc.DefaultEmissiveColor;
//					m_Tile = mc.DefaultTile;
//					
//					// Load texure from resource
//					UnityEngine.Object temp = null;
//					
//					temp = Resources.Load(mc.DefaultDiffuseRes);
//					if ( temp != null ) m_DiffuseTex = Texture2D.Instantiate(temp) as Texture2D;
//					if ( m_DiffuseTex == null ) m_DiffuseTex = new Texture2D (4,4,TextureFormat.ARGB32,false);
//					m_DiffuseData = m_DiffuseTex.EncodeToPNG();
//					
//					temp = Resources.Load(mc.DefaultBumpRes);
//					if ( temp != null ) m_BumpTex = Texture2D.Instantiate(temp) as Texture2D;
//					if ( m_BumpTex == null ) m_BumpTex = new Texture2D (4,4,TextureFormat.ARGB32,false);
//					m_BumpData = m_BumpTex.EncodeToPNG();
				}
				// User customize
				else
				{
					// Load properties
					m_BumpStrength = r.ReadSingle();
					m_SpecularColor.r = r.ReadByte();
					m_SpecularColor.g = r.ReadByte();
					m_SpecularColor.b = r.ReadByte();
					m_SpecularColor.a = r.ReadByte();
					m_SpecularStrength = r.ReadSingle();
					m_SpecularPower = r.ReadSingle();
					m_EmissiveColor.r = r.ReadByte();
					m_EmissiveColor.g = r.ReadByte();
					m_EmissiveColor.b = r.ReadByte();
					m_EmissiveColor.a = r.ReadByte();
					
					
					// Load texture data and create texture from the data, 
					// then resize to TEX_RESOLUTION and update the byte data
					m_Tile = r.ReadSingle();
					
					int l = 0;
					l = r.ReadInt32();
					m_DiffuseData = r.ReadBytes(l);
                    //m_DiffuseTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    //m_DiffuseTex.LoadImage(m_DiffuseData);
                    //m_DiffuseData = m_DiffuseTex.EncodeToPNG();
					
					l = r.ReadInt32();
					m_BumpData = r.ReadBytes(l);
                    //m_BumpTex = new Texture2D (2, 2, TextureFormat.ARGB32, false);
                    //m_BumpTex.LoadImage(m_BumpData);
                    //m_BumpData = m_BumpTex.EncodeToPNG();
				}
				break;
			}
			default:
				break;	
			}
			r.Close();
			ms.Close();
		}
		// Generate a ulong GUID
		CalcGUID();
	}
	
	// To byte buffer
	public byte[] Export()
	{
		// Prepare stream
		MemoryStream ms = new MemoryStream ();
		BinaryWriter w = new BinaryWriter (ms);
		
		// Write standard info
		w.Write(VERSION);	// 0x1000 now
		w.Write(m_Name);
		w.Write(m_MatterId);
		w.Write(m_UseDefault);
		
		// If it is customized
		if ( !m_UseDefault )
		{
			// Write properties
			w.Write(m_BumpStrength);
			w.Write(m_SpecularColor.r);
			w.Write(m_SpecularColor.g);
			w.Write(m_SpecularColor.b);
			w.Write(m_SpecularColor.a);
			w.Write(m_SpecularStrength);
			w.Write(m_SpecularPower);
			w.Write(m_EmissiveColor.r);
			w.Write(m_EmissiveColor.g);
			w.Write(m_EmissiveColor.b);
			w.Write(m_EmissiveColor.a);
			
			w.Write(m_Tile);
			// Write texture data
			if ( m_DiffuseData != null && m_DiffuseData.Length > 0 )
			{
				w.Write(m_DiffuseData.Length);
				w.Write(m_DiffuseData, 0, m_DiffuseData.Length);
			}
			else
			{
				int zero = 0;
				w.Write(zero);
			}
			if ( m_BumpData != null && m_BumpData.Length > 0 )
			{
				w.Write(m_BumpData.Length);
				w.Write(m_BumpData, 0, m_BumpData.Length);
			}
			else
			{
				int zero = 0;
				w.Write(zero);
			}
		}
		w.Close();
		byte [] retval = ms.ToArray();
		ms.Close();
		return retval;
	}
	
	// Calculate a ulong GUID for this mat
	public ulong CalcGUID()
	{
		m_Guid = (ulong)(Mathf.RoundToInt(m_SpecularPower));
		return m_Guid;
	}
	
	// Load Textures
	public void LoadCustomizeTexture(string path_d, string path_n)
	{
		FileStream fs = null;
		
		bool has_diffuse = true;
		// Load diffuse
		try
		{
			fs = new FileStream (path_d, FileMode.Open, FileAccess.Read);
			m_DiffuseData = new byte [(int)(fs.Length)];
			fs.Read(m_DiffuseData, 0, (int)(fs.Length));
			m_DiffuseTex = new Texture2D (2, 2, TextureFormat.ARGB32, false);
			if ( !m_DiffuseTex.LoadImage(m_DiffuseData) )
			{
				throw new Exception("invalid diffuse");
			}
			fs.Close();
			fs = null;
			has_diffuse = true;
		}
		catch (Exception)
		{
			// No such file, read default blank diffuse resource
			has_diffuse = false;
			m_DiffuseTex = Texture2D.Instantiate(Resources.Load(VAMaterial.s_BlankDiffuseRes)) as Texture2D;
		}
		m_DiffuseData = m_DiffuseTex.EncodeToPNG();
		
		// Load normal map
		try
		{
			fs = new FileStream (path_n, FileMode.Open, FileAccess.Read);
			m_BumpData = new byte [(int)(fs.Length)];
			fs.Read(m_BumpData, 0, (int)(fs.Length));
			m_BumpTex = new Texture2D (2, 2, TextureFormat.ARGB32, false);
			if ( !m_BumpTex.LoadImage(m_BumpData) )
			{
				throw new Exception("invalid normal map");
			}
			fs.Close();
			fs = null;
		}
		catch (Exception)
		{
			// No such file, read default blank normal map resource
			if ( has_diffuse )
				m_BumpTex = Texture2D.Instantiate(Resources.Load(VAMaterial.s_BlankBumpRes_)) as Texture2D;	// Have diffuse, no bump map
			else
				m_BumpTex = Texture2D.Instantiate(Resources.Load(VAMaterial.s_BlankBumpRes)) as Texture2D;		// Don't have diffuse or bump map
		}
		m_BumpData = m_BumpTex.EncodeToPNG();
		
		CalcGUID();
	}
	
	//
	// --------- static ---------
	//
	public static string s_BlankDiffuseRes = "Textures/vc_default_diffuse";
	public static string s_BlankBumpRes = "Textures/vc_default_bump";
	public static string s_BlankBumpRes_ = "Textures/vc_default_bump_";
	
	// Calculate a group of made-material's hash code (64 bit)
	public static ulong CalcMatGroupHash(VAMaterial[] mats)
	{
		string mat_guids = "";
		for ( int i = 0; i < mats.Length; i++ )
		{
			if ( mats[i] != null )
				mat_guids += mats[i].m_Guid.ToString();
			else
				mat_guids += "null";
		}
		return CRC64.Compute(System.Text.Encoding.UTF8.GetBytes(mat_guids));
	}
}

