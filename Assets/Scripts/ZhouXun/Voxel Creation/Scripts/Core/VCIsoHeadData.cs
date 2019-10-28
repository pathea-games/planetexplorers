#define PLANET_EXPLORERS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Voxel Creation ISO Head data structure
public struct VCIsoHeadData
{
	public int Version;
	public EVCCategory Category;
	public string Author;
	public string Name;
	public string Desc;
	public string Remarks;
	public int xSize;
	public int ySize;
	public int zSize;
	public byte[] IconTex;

	public void Init ()
	{
		Version = 0;
		Category = EVCCategory.cgAbstract;
		Author = "";
		Name = "";
		Remarks = "";
		xSize = 0;
		ySize = 0;
		zSize = 0;
		IconTex = new byte[0];
	}

#if PLANET_EXPLORERS
	public byte[] SteamPreview
	{
		get
		{
			using ( MemoryStream ms = new MemoryStream () )
			{
				BinaryWriter w = new BinaryWriter (ms);
				ms.Write(IconTex, 0, IconTex.Length);
				int ofs = (int)(ms.Length);
				w.Write(Remarks);
				w.Write(ofs);
				w.Close();
				return ms.ToArray();
			}
		}
		set
		{
			if ( value == null )
			{
				IconTex = null;
				Remarks = "";
				EnsureIconTexValid();
			}
			else
			{
				using ( MemoryStream ms = new MemoryStream (value) )
				{
					BinaryReader r = new BinaryReader (ms);
					ms.Seek(-4, SeekOrigin.End);
					int ofs = r.ReadInt32();
					ms.Seek(0, SeekOrigin.Begin);
					IconTex = new byte[ofs] ;
					ms.Read(IconTex, 0, ofs);
					ms.Seek(ofs, SeekOrigin.Begin);
					Remarks = r.ReadString();
					r.Close();
				}
			}
		}
	}

	public string SteamDesc
	{
		get
		{
			return "Author: " + (Author.Trim().Length > 1 ? Author.Trim() : "-") + "\r\n" + Desc;
		}
	}
#endif
	
	public VCESceneSetting FindSceneSetting ()
	{
		if ( VCConfig.s_EditorScenes == null )
			return null;
		foreach ( VCESceneSetting setting in VCConfig.s_EditorScenes )
		{
			if ( setting.m_Category == Category
			    && setting.m_EditorSize.x == xSize
			    && setting.m_EditorSize.y == ySize
			    && setting.m_EditorSize.z == zSize )
			{
				return setting;
			}
		}
		return null;
	}

	public string[] ScenePaths ()
	{
		VCESceneSetting scene = FindSceneSetting();
		List<string> _scenes = new List<string> ();
        while (null != scene)
		{
			_scenes.Add(scene.m_Name);
			int pid = scene.m_ParentId;
			scene = null;
			foreach ( VCESceneSetting ss in VCConfig.s_EditorScenes )
			{
				if (ss.m_Id == pid)
				{
					scene = ss;
					break;
				}
			}
		}
		if (_scenes.Count == 0)
			return new string[0] ;
        
        string[] scenes = new string[_scenes.Count] ;
		int i = scenes.Length;
		foreach ( string sp in _scenes )
			scenes[--i] = sp;
		scenes[0] = "Creation";
		return scenes;
	}

    

    public void EnsureIconTexValid ()
	{
		Texture2D tex = new Texture2D (2,2);
		if ( IconTex == null || IconTex.Length < 32 || !tex.LoadImage(IconTex) )
		{
			tex = Texture2D.Instantiate(Resources.Load("Textures/default_iso_icon") as Texture2D) as Texture2D;
			IconTex = tex.EncodeToJPG(25);
		}
		Texture2D.Destroy(tex);
	}
	
	public byte[] GetExtendedIconTex (int width, int height)
	{
		Texture2D icontex = new Texture2D (2,2);
		icontex.LoadImage(IconTex);
		width = Mathf.Clamp(width, icontex.width, 512);
		height = Mathf.Clamp(height, icontex.height, 512);
		
		Texture2D tex = new Texture2D (width, height, TextureFormat.ARGB32, false);
		for ( int x = 0; x < width; ++x )
		{
			for ( int y = 0; y < height; ++y )
			{
				tex.SetPixel(x,y,Color.white);
			}
		}
		
		int ox = Mathf.FloorToInt((width - icontex.width) * 0.5f);
		int oy = Mathf.FloorToInt((height - icontex.height) * 0.5f);
		
		for ( int x = 0; x < icontex.width; ++x )
		{
			for ( int y = 0; y < icontex.height; ++y )
			{
				tex.SetPixel(x+ox,y+oy,icontex.GetPixel(x,y));
			}
		}
		tex.Apply();
		byte[] retval = tex.EncodeToPNG();

		Texture2D.Destroy(icontex);
		Texture2D.Destroy(tex);
		return retval;
	}

	public ulong HeadSignature
	{
		get
		{
			string sigstr = "#s;al.t" + ((int)(Category)).ToString() + Author + Name + Desc + Remarks 
				+ xSize.ToString() + ySize.ToString() + zSize.ToString() + "e.nd;s,a+lt";

			byte[] sigbuff = System.Text.Encoding.UTF8.GetBytes(sigstr);
			return CRC64.Compute(sigbuff);
		}
	}
}
