using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using WhiteCat;

public class CreationWaterMask : PeCmpt
{
	public GameObject m_WaterMask;
	private CreationData m_CreationData = null;
	private VCIsoData m_IsoData = null;
	private CreationAttr m_Attribute = null;
	private VCESceneSetting m_SceneSetting = null;
	private Vector3 m_MassCenter = Vector3.zero;
	private Material m_MaskMat;
	private Texture2D m_MaskTex;

	public override void Awake ()
	{
		base.Awake ();
		var cc = GetComponent<CreationController>();
		if ( cc != null )
		{
			m_CreationData = cc.creationData;
			if ( m_CreationData != null )
			{
				m_IsoData = m_CreationData.m_IsoData;
				m_SceneSetting = m_IsoData.m_HeadInfo.FindSceneSetting();
				m_Attribute = m_CreationData.m_Attribute;
				m_MassCenter = m_Attribute.m_CenterOfMass;
			}
		}
	}

	// Use this for initialization
	public override void Start ()
	{
		base.Start ();
		// Transform
		GameObject mask = GameObject.Instantiate(VCEditor.Instance.m_WaterMaskPrefab) as GameObject;
		mask.name = "Water Mask";
		mask.transform.parent = transform;
		mask.transform.localEulerAngles = new Vector3 (90,0,0);
		mask.transform.localScale = new Vector3(m_SceneSetting.EditorWorldSize.x,
		                                        m_SceneSetting.EditorWorldSize.z, 1);
		m_WaterMask = mask;

		// Material
		Material m_MaskMat = Material.Instantiate(mask.GetComponent<Renderer>().material) as Material;
		mask.GetComponent<Renderer>().material = m_MaskMat;

		// Texture
		m_MaskTex = new Texture2D (m_SceneSetting.m_EditorSize.x, m_SceneSetting.m_EditorSize.z, TextureFormat.ARGB32, false);
		m_MaskMat.SetTexture("_MainTex", m_MaskTex);

		int maxy = 0;
		int sizeY = m_SceneSetting.m_EditorSize.y;
		int[] maxx = new int[sizeY] ;
		int[] minx = new int[sizeY] ;
		int[] maxz = new int[sizeY] ;
		int[] minz = new int[sizeY] ;
		int[] vcnt = new int[sizeY] ; 

		for ( int i = 0; i < sizeY; ++i )
		{
			maxx[i] = maxz[i] = 0;
			minx[i] = minz[i] = 1000000;
			vcnt[i] = 0;
		}

		foreach ( KeyValuePair<int, VCVoxel> kvp in m_IsoData.m_Voxels )
		{
			IntVector3 pos = VCIsoData.KeyToIPos(kvp.Key);
			if ( pos.y > maxy )
				maxy = pos.y;
			if ( pos.y >= 0 && pos.y < sizeY )
			{
				if ( pos.x < minx[pos.y] )
					minx[pos.y] = pos.x;
				if ( pos.x > maxx[pos.y] )
					maxx[pos.y] = pos.x;
				if ( pos.z < minz[pos.y] )
					minz[pos.y] = pos.z;
				if ( pos.z > maxz[pos.y] )
					maxz[pos.y] = pos.z;
				vcnt[pos.y]++;
			}
		}

		int beginy = Mathf.Min(Mathf.FloorToInt(m_MassCenter.y / m_SceneSetting.m_VoxelSize), maxy);
		int endy = Mathf.Max(Mathf.FloorToInt(m_MassCenter.y / m_SceneSetting.m_VoxelSize), maxy);
		if (endy > beginy + 16 )	endy = beginy + 16;
		if (endy >= sizeY)			endy = sizeY - 1;
		if (beginy < 0)				beginy = 0;

		int masky = beginy;
		int maxarea = 0;
		int liftchance = 1;
		int maxvol = 0;

		for ( int y = beginy; y <= endy; ++y )
		{
			int area = (maxx[y] - minx[y]) * (maxz[y] - minz[y]);
			int vol = vcnt[y];
			int circle = (maxx[y] - minx[y]) * 2 + (maxz[y] - minz[y]) * 2;

			if ( area >= maxarea && vol >= maxvol )
				masky = y;

			if ( area >= maxarea )
				maxarea = area;
			if ( vol >= maxvol && vol < circle * 5 )
				maxvol = vol;

			if ( area > 100 && vol >= circle / 3 && y >= masky + 5 && y <= beginy + 6 && liftchance > 0 )
			{
				liftchance--;
				masky = y;
			}
			if ( area < maxarea / 9 && y > beginy + 4 )
				break;
		}

		Color32[] maskc = new Color32[m_MaskTex.width*m_MaskTex.height] ;
		for ( int i = 0; i < maskc.Length; ++i )
			maskc[i] = new Color32(0,0,0,255);

		int beginx = minx[masky];
		int endx = maxx[masky];
		int beginz = minz[masky];
		int endz = maxz[masky];
		for ( int x = beginx; x <= endx; ++x )
		{
			for ( int z = beginz; z <= endz; ++z )
			{
				bool white = false;
				for ( int y = masky; y >= 0; --y )
				{
					int key = VCIsoData.IPosToKey(x,y,z);
					if ( m_IsoData.GetVoxel(key).Volume >= VCEMath.MC_ISO_VALUE )
					{
						white = true;
						break;
					}
				}
				if ( white){
					int idx = z*m_SceneSetting.m_EditorSize.x + x;
					if(idx < maskc.Length){
						maskc[idx] = new Color32 (255,255,255,255);
					}
				}
			}
		}

		m_MaskTex.SetPixels32(maskc);
		m_MaskTex.Apply();

		Vector3 ed_center = m_SceneSetting.EditorWorldSize * 0.5f;
		Vector3 lpos = ed_center - m_MassCenter;
		lpos.y = (masky + 0.5f) * m_SceneSetting.m_VoxelSize;
		mask.transform.localPosition = lpos;
	}

	public override void OnDestroy ()
	{
		if ( m_MaskTex != null )
		{
			Texture2D.Destroy(m_MaskTex);
			m_MaskTex = null;
		}
		if ( m_MaskMat != null )
		{
			Material.Destroy(m_MaskMat);
			m_MaskMat = null;
		}
	}
}
