using UnityEngine;
using Pathea.Maths;
using System.Collections.Generic;
using System.IO;

public class RSTile
{
	private INTVECTOR2 m_Pos;
	private Dictionary<int, RoadCell> m_Road;
	
	public delegate void DTileNotify (RSTile tile);
	public static event DTileNotify OnTileCreate;
	public static event DTileNotify OnTileDestroy;
	private bool m_Register;
	public RSTile (int hash, bool register)
	{
		m_Pos.hash = hash;
		m_Road = new Dictionary<int, RoadCell>();
		m_Register = register;
		if ( m_Register && OnTileCreate != null )
			OnTileCreate(this);
	}
	public INTVECTOR2 Pos { get { return m_Pos; } }
	public int Hash { get { return m_Pos.hash; } }
	public Dictionary<int, RoadCell> Road { get { return m_Road; } }

	#region POSITION_CALCULATIONS
	// Query a tile by world position (static)
	public static INTVECTOR2 QueryTile (Vector3 world_pos)
	{
		return new INTVECTOR2 (world_pos.x/TILE_SIZEF, world_pos.z/TILE_SIZEF);
	}
	// Calculate tile-local position from world position
	public Vector3 WorldPosToLocalPos (Vector3 wpos)
	{
		wpos.x -= (m_Pos.x * TILE_SIZEF);
		wpos.z -= (m_Pos.y * TILE_SIZEF);
		return wpos;
	}
	// Calculate a road-cell position in this tile from world position
	public INTVECTOR3 WorldPosToRoadCellPos (Vector3 wpos)
	{
		Vector3 lpos = WorldPosToLocalPos(wpos);
		return new INTVECTOR3 (lpos.x/CELL_SIZEF, lpos.y/CELL_YSIZEF, lpos.z/CELL_SIZEF);
	}
	// Calculate a road-cell position in this tile from world position
	public static INTVECTOR3 WorldPosToRoadCellPos_s (int hash, Vector3 wpos)
	{
		INTVECTOR2 tile_pos = new INTVECTOR2 ();
		tile_pos.hash = hash;
		wpos.x -= (tile_pos.x * TILE_SIZEF);
		wpos.z -= (tile_pos.y * TILE_SIZEF);
		return new INTVECTOR3 (wpos.x/CELL_SIZEF, wpos.y/CELL_YSIZEF, wpos.z/CELL_SIZEF);
	}
	// Check a road-cell position
	public bool CheckRoadCellPos (INTVECTOR3 rcpos)
	{
		if ( rcpos.x < 0 )
			return false;
		if ( rcpos.y < 0 )
			return false;
		if ( rcpos.z < 0 )
			return false;
		if ( rcpos.x >= CELL_AXIS_COUNT )
			return false;
		if ( rcpos.y >= CELL_YAXIS_COUNT )
			return false;
		if ( rcpos.z >= CELL_AXIS_COUNT )
			return false;
		return true;
	}
	#endregion
	// end POSITION_CALCULATIONS


	#region STRUCTURE_FUNCTIONS
	public delegate void DRefreshNotify (RSTile tile);
	public delegate void DDotNotify (int hash, RoadCell rc);
	public DRefreshNotify OnTileRefresh = null;
	public DDotNotify OnTileDot = null;

	public RoadCell GetRoadCell (int hash)
	{
		if ( m_Road.ContainsKey(hash) )
			return m_Road[hash];
		return new RoadCell (0);
	}
	
	public RoadCell GetRoadCell (Vector3 world_pos)
	{
		int hash = WorldPosToRoadCellPos(world_pos).hash;
		if ( m_Road.ContainsKey(hash) )
			return m_Road[hash];
		return new RoadCell (0);
	}

	public RoadCell GetRoadCell (INTVECTOR3 rcpos)
	{
		int hash = rcpos.hash;
		if ( m_Road.ContainsKey(hash) )
			return m_Road[hash];
		return new RoadCell (0);
	}

	public void SetRoadCell (int hash, RoadCell rc)
	{
		bool changed = false;
		if ( rc.type > 0 )
		{
			if ( m_Road.ContainsKey(hash) )
			{
				RoadCell old = m_Road[hash];
				if ( !rc.Equals(old) )
				{
					m_Road[hash] = rc;
					changed = true;
				}
			}
			else
			{
				m_Road.Add(hash, rc);
				changed = true;
			}
		}
		else
		{
			if ( m_Road.ContainsKey(hash) )
			{
				m_Road.Remove(hash);
				changed = true;
			}
		}
		if ( changed && OnTileDot != null )
			OnTileDot(hash, rc);
	}
	
	public void SetRoadCell (Vector3 world_pos, RoadCell rc)
	{
		int hash = WorldPosToRoadCellPos(world_pos).hash;
		SetRoadCell(hash, rc);
	}

	public void SetRoadCell (INTVECTOR3 rcpos, RoadCell rc)
	{
		int hash = rcpos.hash;
		SetRoadCell(hash, rc);
	}

	public void Clear ()
	{
		m_Road.Clear();
		if ( OnTileRefresh != null )
			OnTileRefresh(this);
	}

	public void Destroy ()
	{
		Clear();
		if ( m_Register && OnTileDestroy != null )
			OnTileDestroy(this);
	}
	#endregion
	// end STRUCTURE_FUNCTIONS

	#region IO
	public byte[] CacheData;
	public byte[] Data
	{
		get
		{
			int version = 1;
			MemoryStream ms = new MemoryStream ();
			BinaryWriter w = new BinaryWriter (ms);
			w.Write(version);
			w.Write(m_Road.Count);
			foreach ( KeyValuePair<int, RoadCell> kvp in m_Road )
			{
				w.Write(kvp.Key);
				kvp.Value.WriteToStream(w);
			}
			w.Close();
			byte [] retval = ms.ToArray();
			return retval;
		}
		set
		{
			m_Road.Clear();
			if ( value == null ) return;
			if ( value.Length < 8 ) return;
			MemoryStream ms = new MemoryStream (value);
			BinaryReader r = new BinaryReader (ms);
			int version = r.ReadInt32();
			switch ( version )
			{
			case 1:
			{
				int count = r.ReadInt32();
				for ( int i = 0; i < count; ++i )
				{
					int hash = r.ReadInt32();
					RoadCell rc = new RoadCell ();
					rc.ReadFromStream(r);
					m_Road.Add(hash, rc);
				}
				break;
			}
			default:
				break;
			}
			r.Close();
			ms.Close();
			if ( OnTileRefresh != null )
				OnTileRefresh(this);
		}
	}
	#endregion
	// end IO

	// Constants
	public const int TILE_SIZE_SHIFT = 8;
	public const int CELL_SIZE_SHIFT = 2;
	public const int CELL_YSIZE_SHIFT = 2;
	public const int MAX_HEIGHT_SHIFT = 10;

	public const int TILE_SIZE = 1 << TILE_SIZE_SHIFT;
	public const int CELL_SIZE = 1 << CELL_SIZE_SHIFT;
	public const int CELL_YSIZE = 1 << CELL_YSIZE_SHIFT;
	public const int MAX_HEIGHT = 1 << MAX_HEIGHT_SHIFT;

	public const float CELL_SIZEF = CELL_SIZE;
	public const float CELL_YSIZEF = CELL_YSIZE;
	public const float TILE_SIZEF = TILE_SIZE;
	public const float MAX_HEIGHTF = MAX_HEIGHT;

	public const int CELL_AXIS_COUNT = TILE_SIZE >> CELL_SIZE_SHIFT;
	public const int CELL_YAXIS_COUNT = MAX_HEIGHT >> CELL_SIZE_SHIFT;
}
