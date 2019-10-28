using UnityEngine;
using Pathea.Maths;
using System.Collections.Generic;

public class RSTileRenderer : MonoBehaviour
{
	private bool m_Started = false;
	public RSDataSource m_DataSource;
	public INTVECTOR2 m_Pos;
	public Projector m_Projector;

	public Material m_TileMatResource;
	public Material m_TileMat;
	public Texture2D m_RoadGraph;
	public Texture2D m_HeightGraph0;
	public Texture2D m_HeightGraph1;

	public static Color32[] EmptyTexture = null;
	public static Color32[] EmptyHeightTexture = null;
	
	void Awake ()
	{
		CreateTextures();
		ClearTextures();
		ApplyTexturesChange();
		m_Started = false;
	}

	// Use this for initialization
	void Start ()
	{
		if ( m_DataSource == null )
			return;
		
		m_TileMat = Material.Instantiate(m_TileMatResource) as Material;
		m_TileMat.SetVector("_TileOffset", new Vector4(m_Pos.x * RSTile.TILE_SIZEF, 0, m_Pos.y * RSTile.TILE_SIZEF, 1));
		m_TileMat.SetFloat("_TileSize", RSTile.TILE_SIZEF);
		m_TileMat.SetFloat("_MaxHeight", RSTile.MAX_HEIGHTF);
		m_TileMat.SetTexture("_xzMask", m_RoadGraph);
		m_TileMat.SetTexture("_y0Mask", m_HeightGraph0);
		m_TileMat.SetTexture("_y1Mask", m_HeightGraph1);
		m_Projector.nearClipPlane = 0;
		m_Projector.farClipPlane = RSTile.MAX_HEIGHTF + RSTile.CELL_SIZEF;
		m_Projector.orthographicSize = RSTile.TILE_SIZEF * 0.5f;
		m_Projector.material = m_TileMat;
		m_Projector.transform.localPosition = new Vector3 (RSTile.TILE_SIZEF*0.5f, RSTile.MAX_HEIGHTF, RSTile.TILE_SIZEF*0.5f);

		RSTile TileData = m_DataSource.GetTile(m_Pos);
		if ( TileData != null )
		{
			TileData.OnTileDot = DotTextures;
			TileData.OnTileRefresh = RefreshTextures;
			TileData.Data = TileData.CacheData;
		}
		m_Started = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_DataSource == null )
			return;
		RSTile TileData = m_DataSource.GetTile(m_Pos);
		if ( TileData != null )
		{
			m_Projector.gameObject.SetActive(true);
		}
		else
		{
			m_Projector.gameObject.SetActive(false);
		}
	}

	public void BeforeDestroy ()
	{
		RSTile TileData = m_DataSource.GetTile(m_Pos);
		if ( TileData != null )
		{
			TileData.OnTileDot = null;
			TileData.OnTileRefresh = null;
			TileData.CacheData = TileData.Data;
			TileData.Clear();
		}
	}

	void OnDestroy ()
	{
		DestroyTextures();
	}

	#region TEXTURE_FUNCTIONS
	private void CreateTextures ()
	{
		m_RoadGraph = new Texture2D (RSTile.CELL_AXIS_COUNT, RSTile.CELL_AXIS_COUNT, TextureFormat.ARGB32, false);
		m_HeightGraph0 = new Texture2D (RSTile.CELL_AXIS_COUNT, RSTile.CELL_AXIS_COUNT, TextureFormat.ARGB32, false);
		m_HeightGraph1 = new Texture2D (RSTile.CELL_AXIS_COUNT, RSTile.CELL_AXIS_COUNT, TextureFormat.ARGB32, false);
	}

	private void DestroyTextures ()
	{
		if ( m_RoadGraph != null )
		{
			Texture2D.Destroy(m_RoadGraph);
			m_RoadGraph = null;
		}
		if ( m_HeightGraph0 != null )
		{
			Texture2D.Destroy(m_HeightGraph0);
			m_HeightGraph0 = null;
		}
		if ( m_HeightGraph1 != null )
		{
			Texture2D.Destroy(m_HeightGraph1);
			m_HeightGraph1 = null;
		}
		if ( m_TileMat != null )
		{
			Material.Destroy(m_TileMat);
			m_TileMat = null;
		}
	}

	private void ClearTextures ()
	{
		m_RoadGraph.SetPixels32(EmptyTexture);
		m_HeightGraph0.SetPixels32(EmptyHeightTexture);
		m_HeightGraph1.SetPixels32(EmptyHeightTexture);
	}

	private void WriteTileToTextures (RSTile tile)
	{
		foreach ( KeyValuePair<int, RoadCell> kvp in tile.Road )
			WriteRoadCellToTextures(kvp.Key, kvp.Value);
	}

	private int WriteRoadCellToTextures (int hash, RoadCell rc)
	{
		INTVECTOR3 rcpos = new INTVECTOR3 ();
		rcpos.hash = hash;
		byte h = (byte)(rcpos.y + 1);
		if ( rc.type > 0 )
		{
			m_RoadGraph.SetPixel(rcpos.x, rcpos.z, (Color)(rc.color_type));
			Color32 hc = new Color32 (0,0,0,0);

			hc = SafeColorConvert(m_HeightGraph0.GetPixel(rcpos.x, rcpos.z));
			if ( hc.r == h || hc.g == h || hc.b == h )
			{
				return -1;
			}
			if ( hc.r == 0 )
			{
				hc.r = h;
				m_HeightGraph0.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 0;
			}
			if ( hc.g == 0 )
			{
				hc.g = h;
				m_HeightGraph0.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 0;
			}
			if ( hc.b == 0 )
			{
				hc.b = h;
				m_HeightGraph0.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 0;
			}
			hc = SafeColorConvert(m_HeightGraph1.GetPixel(rcpos.x, rcpos.z));
			if ( hc.r == h || hc.g == h || hc.b == h )
			{
				return -1;
			}
			if ( hc.r == 0 )
			{
				hc.r = h;
				m_HeightGraph1.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 1;
			}
			if ( hc.g == 0 )
			{
				hc.g = h;
				m_HeightGraph1.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 1;
			}
			if ( hc.b == 0 )
			{
				hc.b = h;
				m_HeightGraph1.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 1;
			}
			return -1;
		}
		else
		{
			m_RoadGraph.SetPixel(rcpos.x, rcpos.z, new Color(0,0,0,0));
			Color32 hc = new Color32 (0,0,0,0);
			hc = SafeColorConvert(m_HeightGraph0.GetPixel(rcpos.x, rcpos.z));
			if ( hc.r == h )
			{
				hc.r = 0;
				m_HeightGraph0.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 0;
			}
			if ( hc.g == h )
			{
				hc.g = 0;
				m_HeightGraph0.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 0;
			}
			if ( hc.b == h )
			{
				hc.b = 0;
				m_HeightGraph0.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 0;
			}
			hc = SafeColorConvert(m_HeightGraph1.GetPixel(rcpos.x, rcpos.z));
			if ( hc.r == h )
			{
				hc.r = 0;
				m_HeightGraph1.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 1;
			}
			if ( hc.g == h )
			{
				hc.g = 0;
				m_HeightGraph1.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 1;
			}
			if ( hc.b == h )
			{
				hc.b = 0;
				m_HeightGraph1.SetPixel(rcpos.x, rcpos.z, (Color)(hc));
				return 1;
			}
			return -1;
		}
	}

	private void ApplyTexturesChange (int mask = 1)
	{
		m_RoadGraph.Apply();
		if ( mask >= 0 )
			m_HeightGraph0.Apply();
		if ( mask >= 1 )
			m_HeightGraph1.Apply();
	}

	public void RefreshTextures (RSTile tile)
	{
		ClearTextures();
		WriteTileToTextures(tile);
		ApplyTexturesChange();
	}

	public void DotTextures (int hash, RoadCell rc)
	{
		if ( !m_Started ) return;
		int mask = WriteRoadCellToTextures(hash, rc);
		ApplyTexturesChange(mask);
	}

	public static Color32 SafeColorConvert(Color c)
	{
		return new Color32 ((byte)(Mathf.Clamp01 (c.r) * 255f + 0.01f), 
		                    (byte)(Mathf.Clamp01 (c.g) * 255f + 0.01f),
		                    (byte)(Mathf.Clamp01 (c.b) * 255f + 0.01f),
		                    (byte)(Mathf.Clamp01 (c.a) * 255f + 0.01f));
	}
	#endregion
	// End TEXTURE_FUNCTIONS
}
