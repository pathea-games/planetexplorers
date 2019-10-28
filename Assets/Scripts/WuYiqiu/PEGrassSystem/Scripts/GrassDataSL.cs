using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using Pathea.Maths;

public class GrassDataSL 
{
	public const int VERSION = 0x0101;

	public static Dictionary<INTVECTOR3, INTVECTOR3> m_mapDelPos = null;
    public static event Action OnGrassDataInitEvent;

	public static void Init()
	{
		if ( m_mapDelPos != null )
		{
			m_mapDelPos.Clear();
			//m_mapDelPos = null;
		}
		else
			m_mapDelPos = new Dictionary<INTVECTOR3, INTVECTOR3> ();

        if (null != OnGrassDataInitEvent)
            OnGrassDataInitEvent();
    }
	
	public static void Clear()
	{
		if ( m_mapDelPos != null )
		{
			m_mapDelPos.Clear();
			//m_mapDelPos = null;
		}
		else
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
		}
	}

	public static void AddDeletedGrass( INTVECTOR3 pos )
	{
		if ( m_mapDelPos == null )
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
			return;
		}

		if (!m_mapDelPos.ContainsKey(pos))
			m_mapDelPos.Add(pos, pos);
	}

	public static void Import( byte[] buffer )
	{
		if ( buffer == null )
			return;
		if ( buffer.Length < 8 )
			return;

		MemoryStream ms = new MemoryStream (buffer);
		BinaryReader r = new BinaryReader (ms);
		int version = r.ReadInt32();
		if ( VERSION != version )
		{
			Debug.LogWarning("The version of LSubTerrSL is newer than the record.");
		}

		switch ( version )
		{
		case 0x0101:
		{
			int conut = r.ReadInt32(); 
			for (int i = 0; i < conut; ++i)
			{
				INTVECTOR3 pos = new INTVECTOR3();
				pos.x = r.ReadInt32();
				pos.y = r.ReadInt32();
				pos.z = r.ReadInt32();
					
				m_mapDelPos.Add(pos, pos);
			}
		} break;
		default:
			break;
		}

		r.Close();
		ms.Close();
	}

	public static void Export(BinaryWriter w)
	{
		if ( m_mapDelPos == null )
		{
			Debug.LogError("LSubTerrSL haven't initialized!");
			return;
		}

		w.Write(VERSION);
		w.Write(m_mapDelPos.Count);
		foreach(INTVECTOR3 pos in m_mapDelPos.Values)
		{
			w.Write(pos.x);
			w.Write(pos.y);
			w.Write(pos.z);
		}
	}
}
