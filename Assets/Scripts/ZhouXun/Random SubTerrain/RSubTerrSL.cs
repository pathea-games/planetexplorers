using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

public static class RSubTerrSL
{
	public const int VERSION = 0x0101;
	
	public static Dictionary<int, List<Vector3>> m_mapDelPos = null;
    public static event Action OnRSubTerrSLInitEvent;

    public static void Init()
	{
		if ( m_mapDelPos != null )
		{
			foreach ( KeyValuePair<int, List<Vector3>> kvp in m_mapDelPos )
			{
				kvp.Value.Clear();
			}
			m_mapDelPos.Clear();
			m_mapDelPos = null;
		}
		m_mapDelPos = new Dictionary<int, List<Vector3>> ();

        if (null != OnRSubTerrSLInitEvent)
            OnRSubTerrSLInitEvent();
    }
	
	public static void Clear()
	{
		if ( m_mapDelPos != null )
		{
			foreach ( KeyValuePair<int, List<Vector3>> kvp in m_mapDelPos )
			{
				kvp.Value.Clear();
			}
			m_mapDelPos.Clear();
			m_mapDelPos = null;
		}
		else
		{
			Debug.LogError("RSubTerrSL haven't initialized!");
		}
	}
	
	public static void AddDeletedTree( TreeInfo treeinfo )
	{
		AddDeletedTree(treeinfo.m_pos);
	}
	
	public static void AddDeletedTree( Vector3 pos )
	{
		if ( m_mapDelPos == null )
		{
			Debug.LogError("RSubTerrSL haven't initialized!");
			return;
		}
		int index = RSubTerrUtils.ChunkPosToIndex(Mathf.FloorToInt(pos.x/RSubTerrConstant.ChunkSizeF), Mathf.FloorToInt(pos.z/RSubTerrConstant.ChunkSizeF));
		if ( m_mapDelPos.ContainsKey(index) )
		{
			if ( !m_mapDelPos[index].Contains(pos) )
				m_mapDelPos[index].Add(pos);
		}
		else
		{
			List<Vector3> del_list = new List<Vector3> ();
			del_list.Add(pos);
			m_mapDelPos.Add(index, del_list);
		}
	}
	
	public static void Import( byte[] buffer )
	{
		if ( buffer == null )
			return;
		if ( buffer.Length < 8 )
			return;
		Init();
		MemoryStream ms = new MemoryStream (buffer);
		BinaryReader r = new BinaryReader (ms);
		int version = r.ReadInt32();
		if ( VERSION != version )
		{
			Debug.LogWarning("The version of RSubTerrSL is newer than the record.");
		}
		switch ( version )
		{
		case 0x0101:
			int count = r.ReadInt32();
			for ( int i = 0; i < count; i++ )
			{
				int key = r.ReadInt32();
				float x = r.ReadSingle();
				float y = r.ReadSingle();
				float z = r.ReadSingle();
				if ( m_mapDelPos.ContainsKey(key) )
				{
					m_mapDelPos[key].Add(new Vector3(x,y,z));
				}
				else
				{
					List<Vector3> del_list = new List<Vector3> ();
					del_list.Add(new Vector3(x,y,z));
					m_mapDelPos.Add(key, del_list);
				}
			}
			break;
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
			Debug.LogError("RSubTerrSL haven't initialized!");
			return;
		}
		
		int del_cnt = 0;
		foreach ( KeyValuePair<int, List<Vector3>> kvp in m_mapDelPos )
		{
			del_cnt += kvp.Value.Count;
		}
		
		w.Write(VERSION);
		w.Write(del_cnt);
		foreach ( KeyValuePair<int, List<Vector3>> kvp in m_mapDelPos )
		{
			foreach ( Vector3 pos in kvp.Value )
			{
				w.Write(kvp.Key);
				w.Write(pos.x);
				w.Write(pos.y);
				w.Write(pos.z);
			}
		}
	}
}
