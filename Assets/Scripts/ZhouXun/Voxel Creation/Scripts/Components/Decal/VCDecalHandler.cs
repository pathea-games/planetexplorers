using UnityEngine;
using System.Collections;
using System;

public class VCDecalHandler : MonoBehaviour, ISerializationCallbackReceiver
{
	public Projector m_Projector;
	public VCEComponentTool m_Tool;
	public ulong m_Guid = 0;
	public VCIsoData m_Iso = null;
	public int m_AssetIndex = -1;
	public float m_Depth = 0.01f;
	public float m_Size = 0.1f;
	public bool m_Mirrored = false;
	public int m_ShaderIndex = 0;
	public Color m_Color = Color.white;
	public Material[] m_DecalMats;


	// Use this for initialization
	void Start ()
	{
		for ( int i = 0; i < m_DecalMats.Length; ++i )
		{
			if ( m_DecalMats[i] != null )
			{
				Material new_mat = Material.Instantiate(m_DecalMats[i]) as Material;
				m_DecalMats[i] = new_mat;
			}
		}
		m_Projector.material = m_DecalMats[0];
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		VCDecalAsset decal_asset = VCEAssetMgr.GetDecal(m_Guid);
		if ( decal_asset == null )
		{
			if ( m_Iso != null && m_Iso.m_DecalAssets != null && m_AssetIndex >= 0 && m_AssetIndex < VCIsoData.DECAL_ARR_CNT )
			{
				decal_asset = m_Iso.m_DecalAssets[m_AssetIndex];
			}
		}
		if ( decal_asset == null )
		{
			m_Projector.gameObject.SetActive(false);
			return;
		}

		if ( VCEditor.DocumentOpen() && m_Tool != null && m_Tool.m_SelBound != null )
		{
			m_Tool.m_SelBound.transform.localScale = new Vector3(m_Size, m_Size, m_Depth*2-0.002f);
			m_Tool.m_SelBound.transform.localPosition = new Vector3(0, 0, 0);
		}
		Material usedMaterial = null;
		if ( m_ShaderIndex >= 0 && m_ShaderIndex < m_DecalMats.Length )
			usedMaterial = m_DecalMats[m_ShaderIndex];

		m_Projector.gameObject.SetActive(usedMaterial != null);

		if ( usedMaterial != null )
		{
			m_Projector.material = usedMaterial;
			m_Projector.nearClipPlane = 0.001f - m_Depth;
			m_Projector.farClipPlane = m_Depth - 0.001f;
			m_Projector.orthographicSize = m_Size*0.5f;
			usedMaterial.SetTexture("_Texture", decal_asset.m_Tex);
			usedMaterial.SetColor("_TintColor", m_Color);
			usedMaterial.SetFloat("_Size", m_Size*0.5f);
			usedMaterial.SetFloat("_Depth", m_Depth);
			usedMaterial.SetVector("_Center", new Vector4(transform.position.x, transform.position.y, transform.position.z, 1));
			usedMaterial.SetVector("_Forward", new Vector4(transform.forward.x, transform.forward.y, transform.forward.z, 0));
			if ( m_Mirrored )
				usedMaterial.SetVector("_Right", -new Vector4(transform.right.x, transform.right.y, transform.right.z, 0));
			else
				usedMaterial.SetVector("_Right", new Vector4(transform.right.x, transform.right.y, transform.right.z, 0));
			usedMaterial.SetVector("_Up", new Vector4(transform.up.x, transform.up.y, transform.up.z, 0));
		}
	}


	static VCIsoData _isoData;


	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		_isoData = m_Iso;
    }


	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_Iso = _isoData;
    }
}
