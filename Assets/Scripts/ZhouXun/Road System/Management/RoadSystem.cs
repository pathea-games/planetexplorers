using UnityEngine;
using Pathea.Maths;
using System.Collections.Generic;

public class RoadSystem : MonoBehaviour
{
	private static RoadSystem s_Instance = null;
	public static RoadSystem Instance { get { return s_Instance; } }

	private RSDataSource m_DataSource;
	public static RSDataSource DataSource
	{
		get { return (s_Instance != null) ? (s_Instance.m_DataSource) : (null); }
	}

	public int m_TileExtend = 1;
	private Dictionary<int, RSTileRenderer> m_Renderers;
	public RSTileRenderer m_RendererResource;

	public Transform m_RendererGroup;

	public INTVECTOR2 m_Center = INTVECTOR2.zero;
	//private INTVECTOR2 m_LastCenter = INTVECTOR2.zero;
	private void UpdateCenter ()
	{
		//m_LastCenter = m_Center;
        //if ( PlayerFactory.mMainPlayer != null )
        //{
        //    m_Center = RSTile.QueryTile(PlayerFactory.mMainPlayer.transform.position);
        //}
	}
	
	// Use this for initialization
	void Awake ()
	{
		s_Instance = this;
		m_DataSource = new RSDataSource ();
		m_Renderers = new Dictionary<int, RSTileRenderer> ();

		int rccnt = RSTile.CELL_AXIS_COUNT * RSTile.CELL_AXIS_COUNT;
		RSTileRenderer.EmptyTexture = new Color32[rccnt];
		RSTileRenderer.EmptyHeightTexture = new Color32[rccnt];
		for ( int i = 0; i < rccnt; ++i )
		{
			RSTileRenderer.EmptyTexture[i] = new Color32 (0,0,0,0);
			RSTileRenderer.EmptyHeightTexture[i] = new Color32 (0,0,0,255);
		}

		RSTile.OnTileCreate += OnTileCreate;
		RSTile.OnTileDestroy += OnTileDestroy;
	}
	
	void OnDestroy ()
	{
		s_Instance = null;
		m_DataSource.Destroy();

		RSTile.OnTileCreate -= OnTileCreate;
		RSTile.OnTileDestroy -= OnTileDestroy;
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateCenter();
		UpdateRenderers();
	}

//	int frameCnt = 0;
	void UpdateRenderers ()
	{
        //if ( PlayerFactory.mMainPlayer != null )
        //{
        //    if ( frameCnt == 30 )
        //    {
        //        RefreshRenderers();
        //    }
        //    if ( frameCnt > 30 )
        //    {
        //        if ( m_LastCenter != m_Center )
        //            RefreshRenderers();
        //    }
        //    frameCnt++;
        //}
	}

	private void AddRenderer (INTVECTOR2 pos)
	{
		int hash = pos.hash;
		if ( !m_Renderers.ContainsKey(hash) )
		{
			RSTileRenderer tr = RSTileRenderer.Instantiate(m_RendererResource) as RSTileRenderer;
			tr.gameObject.name = "Road Tile " + pos.ToString();
			tr.transform.parent = m_RendererGroup;
			tr.transform.position = new Vector3(pos.x * RSTile.TILE_SIZE, 0, pos.y * RSTile.TILE_SIZE);
			tr.transform.rotation = Quaternion.identity;
			tr.transform.localScale = Vector3.one;
			tr.m_DataSource = m_DataSource;
			tr.m_Pos = pos;
			m_Renderers.Add(hash, tr);
		}
	}

	private void RemoveFarRenderers ()
	{
		List<int> del_list = new List<int> ();
		foreach ( KeyValuePair<int, RSTileRenderer> kvp in m_Renderers )
		{
			if ( Mathf.Abs(kvp.Value.m_Pos.x - m_Center.x) > m_TileExtend 
			  || Mathf.Abs(kvp.Value.m_Pos.y - m_Center.y) > m_TileExtend )
			{
				del_list.Add(kvp.Key);
			}
		}
		foreach ( int del in del_list )
		{
			m_Renderers[del].BeforeDestroy();
			GameObject.Destroy(m_Renderers[del].gameObject);
			m_Renderers.Remove(del);
		}
	}

	public void RefreshRenderers ()
	{
		RemoveFarRenderers();
		for ( int x = m_Center.x - m_TileExtend; x <= m_Center.x + m_TileExtend; ++x )
		{
			for ( int z = m_Center.y - m_TileExtend; z <= m_Center.y + m_TileExtend; ++z )
			{
				AddRenderer(new INTVECTOR2(x,z));
			}
		}
	}

	void OnTileCreate (RSTile tile)
	{
		int hash = tile.Hash;
		if ( m_Renderers.ContainsKey(hash) )
		{
			tile.OnTileRefresh = m_Renderers[hash].RefreshTextures;
			tile.OnTileDot = m_Renderers[hash].DotTextures;
		}
		else
		{
			tile.OnTileRefresh = null;
			tile.OnTileDot = null;
		}
	}

	void OnTileDestroy (RSTile tile)
	{
		tile.OnTileRefresh = null;
		tile.OnTileDot = null;
	}
}
