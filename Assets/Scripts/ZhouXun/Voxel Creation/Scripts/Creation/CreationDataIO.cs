using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Input & Output of CreationData resource
//
public partial class CreationData
{
	// Hash code calculate
	public void CalcHash ()
	{
		m_HashCode = CRC64.Compute(m_Resource);
	}
	
	// Load Resource
	public bool LoadRes ()
	{
		string fn1 = VCConfig.s_CreationPath + HashString + VCConfig.s_CreationFileExt;
		string fn2 = VCConfig.s_CreationNetCachePath + HashString + VCConfig.s_CreationNetCacheFileExt;
		
		string fn = "";
		if ( File.Exists(fn1) )
			fn = fn1;
		else if ( File.Exists(fn2) )
			fn = fn2;
		if ( fn.Length == 0 )
			return false;
		
		try
		{
			using ( FileStream fs = new FileStream (fn, FileMode.Open) )
			{
				m_Resource = new byte [(int)(fs.Length)];
				fs.Read(m_Resource, 0, (int)(fs.Length));
				fs.Close();
			}

#if SteamVersion
#else
			if ( m_HashCode != CRC64.Compute(m_Resource) )
			{
				Debug.LogError("Load Creation Resource Error: \r\nContent doesn't match the hash code");
				return false;
			}
#endif

			return ReadRes();
		}
		catch (Exception e)
		{
			Debug.LogError("Load Creation Resource Error: \r\n" + e);
			return false;
		}
	}
	
	public bool ReadRes ()
	{
		if ( m_IsoData != null )
			m_IsoData.Destroy();
		m_IsoData = new VCIsoData ();
		return m_IsoData.Import(m_Resource, new VCIsoOption(false));
	}
	
	// Save Resource 
	private bool SaveResToFile (string filename)
	{
		try
		{
			using ( FileStream fs = new FileStream (filename, FileMode.Create) )
			{
				fs.Write(m_Resource, 0, m_Resource.Length);
				fs.Close();
			}
			return true;
		}
		catch (Exception e)
		{
			Debug.LogError("Save Creation Resource Error: \r\n" + e);
			return false;
		}
	}
	public bool SaveRes ()
	{
		CalcHash();
		string fn = VCConfig.s_CreationPath + HashString + VCConfig.s_CreationFileExt;
		if ( !Directory.Exists(VCConfig.s_CreationPath) )
			Directory.CreateDirectory(VCConfig.s_CreationPath);
		return SaveResToFile(fn);
	}
	public bool SaveNetCache ()
	{
		CalcHash();
		string fn = VCConfig.s_CreationNetCachePath + HashString + VCConfig.s_CreationNetCacheFileExt;
		if ( !Directory.Exists(VCConfig.s_CreationNetCachePath) )
			Directory.CreateDirectory(VCConfig.s_CreationNetCachePath);
		return SaveResToFile(fn);		
	}
}
