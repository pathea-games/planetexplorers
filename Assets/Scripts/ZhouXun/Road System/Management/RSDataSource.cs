using UnityEngine;
using Pathea.Maths;
using System.IO;
using System.Collections.Generic;

public class RSDataSource
{
	private Dictionary<int, RSTile> m_Tiles;
	public Dictionary<int, RSTile> Tiles { get { return m_Tiles; } }

	public RSDataSource ()
	{
		m_Tiles = new Dictionary<int, RSTile>();
	}
	~RSDataSource ()
	{
		Destroy();
	}
	public void Destroy ()
	{
		foreach ( KeyValuePair<int, RSTile> kvp in m_Tiles )
		{
			if ( kvp.Value != null )
			{
				kvp.Value.Destroy();
				kvp.Value.CacheData = null;
			}
		}
		m_Tiles.Clear();
	}

	public RSTile GetTile (INTVECTOR2 pos)
	{
		int hash = pos.hash;
		if ( m_Tiles.ContainsKey(hash) )
			return m_Tiles[hash];
		return null;
	}

	public void AlterRoadCell (int tile_hash, int cell_hash, RoadCell rc)
	{
		if ( rc.type > 0 )
		{
			if ( !m_Tiles.ContainsKey(tile_hash) )
				m_Tiles.Add(tile_hash, new RSTile (tile_hash, true));
			m_Tiles[tile_hash].SetRoadCell(cell_hash, rc);
		}
		else
		{
			if ( m_Tiles.ContainsKey(tile_hash) )
			{
				RSTile tile = m_Tiles[tile_hash];
				tile.SetRoadCell(cell_hash, rc);
				if ( tile.Road.Count == 0 )
				{
					tile.Destroy();
					m_Tiles.Remove(tile_hash);
				}
			}
		}
	}

	public void AlterRoadCell (Vector3 world_pos, RoadCell rc)
	{
		int tile_hash = RSTile.QueryTile(world_pos).hash;
		int cell_hash = RSTile.WorldPosToRoadCellPos_s(tile_hash, world_pos).hash;
		AlterRoadCell(tile_hash, cell_hash, rc);
	}

	public RoadCell GetRoadCell (int tile_hash, int cell_hash)
	{
		if ( m_Tiles.ContainsKey(tile_hash) )
			return m_Tiles[tile_hash].GetRoadCell(cell_hash);
		return new RoadCell (0);
	}
	
	public RoadCell GetRoadCell (Vector3 world_pos)
	{
		int tile_hash = RSTile.QueryTile(world_pos).hash;
		if ( m_Tiles.ContainsKey(tile_hash) )
			return m_Tiles[tile_hash].GetRoadCell(world_pos);
		return new RoadCell (0);
	}
}
