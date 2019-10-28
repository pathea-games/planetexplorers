using UnityEngine;
using System.Collections;
using System.IO;

public class VCDecalAsset
{
	// Identify
	public ulong m_Guid;
	public string GUIDString { get { return m_Guid.ToString("X").PadLeft(16, '0'); } }

	// Texture
	public const int MAX_TEX_RESOLUTION = 512;
	public byte[] m_TexData;
	public Texture2D m_Tex = null;
	private static string s_DefaultTexPath = "Decals/Unknown";

	// Destruct
	public void Destroy()
	{
		// Clean up textures
		if ( m_Tex != null )
		{
			Texture2D.Destroy(m_Tex);
			m_Tex = null;
		}
		m_TexData = null;
	}

	// I/O
	// From byte buffer
	public void Import(byte[] buffer)
	{
		Destroy();
		// Generate a ulong GUID
		m_Guid = CRC64.Compute(buffer);
		// Read the buffer
		using ( MemoryStream ms = new MemoryStream (buffer) )
		{
			m_TexData = new byte[(int)(ms.Length)];
			ms.Read(m_TexData, 0, (int)(ms.Length));
		}
		m_Tex = new Texture2D (4,4);
		if ( !m_Tex.LoadImage(m_TexData) )
		{
			Destroy();
			Texture2D tex = Resources.Load(s_DefaultTexPath) as Texture2D;
			m_TexData = tex.EncodeToPNG();
			m_Tex = new Texture2D (4,4);
			if ( !m_Tex.LoadImage(m_TexData) )
			{
				Debug.LogError("Can't find default decal texture!");
				Destroy();
				m_Tex = new Texture2D (16,16,TextureFormat.ARGB32,false);
				m_TexData = m_Tex.EncodeToPNG();
			}
		}
		m_Tex.filterMode = FilterMode.Trilinear;
		m_Tex.wrapMode = TextureWrapMode.Clamp;
	}
	
	// To byte buffer
	public byte[] Export()
	{
		if ( m_TexData != null )
			return m_TexData;
		else
			return new byte[0];
	}
	
	// Calculate a ulong GUID for this mat
	public void CalcGUID()
	{
		m_Guid = CRC64.Compute(Export());
	}
}
