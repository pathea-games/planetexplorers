using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public struct BSIsoHeadData  
{
	public int Version;
	public EBSVoxelType Mode;
	public string Author;
	public string Name;
	public string Desc;
	public string Remarks;
	public int xSize;
	public int ySize;
	public int zSize;
	public byte[] IconTex;

	public Dictionary<byte, UInt32> costs;

	public void Init ()
	{
		Version = 0;
		Author = "";
		Name = "";
		Remarks = "";
		xSize = 0;
		ySize = 0;
		zSize = 0;
		IconTex = new byte[0];
		costs = new Dictionary<byte, UInt32>();
	}

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

	public void EnsureIconTexValid ()
	{
		Texture2D tex = new Texture2D (2,2);
		if ( IconTex == null || IconTex.Length < 32 || !tex.LoadImage(IconTex) )
		{
			tex = Texture2D.Instantiate(Resources.Load("Textures/default_iso_icon") as Texture2D) as Texture2D;
			IconTex = tex.EncodeToPNG();
		}
		Texture2D.Destroy(tex);
	}
}
