using UnityEngine;
using System;
using System.Collections;

public class VCMatGenerator : MonoBehaviour
{
	private static VCMatGenerator s_Instance = null;
	public static VCMatGenerator Instance { get { return s_Instance; } }
	
	#region INSPECTOR_VALS
	public Camera m_TextureCam;
	public Transform m_TexturePlanesGroup;
	public GameObject m_TemplatePlane;
	public Material m_TemplatePlaneMat;
	public Material m_TemplateMeshMat;
	public GameObject m_IconGenGroup;
	public Camera m_IconGenCamera;
	public Renderer m_IconCubeRenderer;
	#endregion
	
	private MeshRenderer [] m_PlaneRenderers;
	private Material [] m_PlaneMaterials;
	
	#region U3D_INTERNAL_FUNCS
	// Use this for initialization
	void Awake ()
	{
		s_Instance = this;
	}
	void Start ()
	{
		// Set texture camera params, depend on VCIsoData's constants
		m_TextureCam.orthographicSize = (float)(VCIsoData.MAT_ROW_CNT)*0.5F;
		m_TextureCam.aspect = (float)(VCIsoData.MAT_COL_CNT) / (float)(VCIsoData.MAT_ROW_CNT);
		m_TextureCam.transform.position = new Vector3 (VCIsoData.MAT_COL_CNT*0.5F, 2.0F, VCIsoData.MAT_ROW_CNT*0.5F);
		
		// Init. collections
		m_PlaneRenderers = new MeshRenderer [VCIsoData.MAT_ARR_CNT];
		m_PlaneMaterials = new Material [VCIsoData.MAT_ARR_CNT];
		
		// Set collections and create render planes gos
		for ( int i = 0; i < VCIsoData.MAT_ROW_CNT; i++ )
		{
			for ( int j = 0; j < VCIsoData.MAT_COL_CNT; j++ )
			{
				// Create render planes GameObjects
				GameObject plane_go = GameObject.Instantiate(m_TemplatePlane) as GameObject;
				plane_go.name = "Render plane [" + i.ToString() + "," + j.ToString() + "]";
				plane_go.transform.parent = m_TexturePlanesGroup;
				plane_go.transform.position = new Vector3(j+0.4999F, 0F, i+0.4999F); // 0.4999 to avoid pixel error
				
				// Create render planes materials
				Material mat = Material.Instantiate(m_TemplatePlaneMat) as Material;
				mat.name = "Render material [" + i.ToString() + "," + j.ToString() + "]";
				plane_go.GetComponent<Renderer>().material = mat;
				
				// Set collections
				m_PlaneRenderers[i*VCIsoData.MAT_COL_CNT + j] = plane_go.GetComponent<MeshRenderer>();
				m_PlaneMaterials[i*VCIsoData.MAT_COL_CNT + j] = mat;
				
				// Active GameObject
				plane_go.SetActive(true);
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_TextureCam.orthographicSize = (float)(VCIsoData.MAT_ROW_CNT)*0.5F;
		m_TextureCam.aspect = (float)(VCIsoData.MAT_COL_CNT) / (float)(VCIsoData.MAT_ROW_CNT);
	}
	
	// On destroy
	void OnDestroy ()
	{
		if ( s_Instance != null )
		{
			try
			{
				if ( m_PlaneRenderers != null )
				{
					foreach ( MeshRenderer r in m_PlaneRenderers )
					{
						if ( r != null && r.gameObject != null )
							GameObject.Destroy(r.gameObject);
					}
				}
				if ( m_PlaneMaterials != null )
				{
					foreach ( Material mat in m_PlaneMaterials )
					{
						if ( mat != null )
							Material.Destroy(mat);
					}
				}
			}
			catch ( Exception e )
			{
				Debug.Log("VCMatGenerator OnDestroy have some problem" + e.ToString());
			}
			s_Instance = null;
		}
	}
	#endregion
	
	// Generate a mesh material for VOXEL CREATIONs
	public ulong GenMeshMaterial(VCMaterial[] mat_list, bool bForEditor = false)
	{
		// Prepare textures
		RenderTexture diff_rtex = null;
		RenderTexture bump_rtex = null;
		Texture2D prop_tex = null;
		
		Vector4 Setting1 = new Vector4(1.0F/((float)VCIsoData.MAT_COL_CNT), 
			                           1.0F/((float)VCIsoData.MAT_ROW_CNT), 
			                           1.0F,
			                           VCMaterial.TEX_RESOLUTION);
		
		// Get mat_list's hash code
		ulong _guid = VCMaterial.CalcMatGroupHash(mat_list);
		
		// For editor, VCEditor.s_Scene
		if ( bForEditor )
		{
			if ( !VCEditor.DocumentOpen() )
				return 0;
			diff_rtex = VCEditor.s_Scene.m_TempIsoMat.m_DiffTex;
			bump_rtex = VCEditor.s_Scene.m_TempIsoMat.m_BumpTex;
			prop_tex = VCEditor.s_Scene.m_TempIsoMat.m_PropertyTex;
			
			Material final_mat = VCEditor.s_Scene.m_TempIsoMat.m_EditorMat;
			final_mat.SetTexture("_DiffuseTex", diff_rtex);
			final_mat.SetTexture("_BumpTex", bump_rtex);
			final_mat.SetTexture("_PropertyTex", prop_tex);
			final_mat.SetVector("_Settings1", Setting1);
		}
		// Not for editor, VCMatManagr
		else
		{
			if ( VCMatManager.Instance == null )
				return 0;
			
			// Has material group in VCMatManager (hash exist)
			if ( VCMatManager.Instance.m_mapMaterials.ContainsKey(_guid) )
			{
				// Fetch textures
				diff_rtex = VCMatManager.Instance.m_mapDiffuseTexs[_guid];
				bump_rtex = VCMatManager.Instance.m_mapBumpTexs[_guid];
				prop_tex = VCMatManager.Instance.m_mapPropertyTexs[_guid];
			}
			// A new material group (hash not exist)
			else
			{
				// Create new textures
				diff_rtex = new RenderTexture (VCIsoData.MAT_COL_CNT*VCMaterial.TEX_RESOLUTION, 
					                                         VCIsoData.MAT_ROW_CNT*VCMaterial.TEX_RESOLUTION,
					                                         0, RenderTextureFormat.ARGB32);
				bump_rtex = new RenderTexture (VCIsoData.MAT_COL_CNT*VCMaterial.TEX_RESOLUTION, 
					                                         VCIsoData.MAT_ROW_CNT*VCMaterial.TEX_RESOLUTION,
					                                         0, RenderTextureFormat.ARGB32);
				prop_tex = new Texture2D (VCIsoData.MAT_ARR_CNT, 4, TextureFormat.ARGB32, false, false);
				
				diff_rtex.anisoLevel = 4;
				diff_rtex.filterMode = FilterMode.Trilinear;
				diff_rtex.useMipMap = true;
				diff_rtex.wrapMode = TextureWrapMode.Repeat;
				
				bump_rtex.anisoLevel = 4;
				bump_rtex.filterMode = FilterMode.Trilinear;
				bump_rtex.useMipMap = true;
				bump_rtex.wrapMode = TextureWrapMode.Repeat;
				
				prop_tex.anisoLevel = 0;
				prop_tex.filterMode = FilterMode.Point;
				
				// Set u3d material params
				Material final_mat = Material.Instantiate(m_TemplateMeshMat) as Material;
				final_mat.name = "VCMat #" + _guid.ToString("X").PadLeft(16, '0');
				final_mat.SetTexture("_DiffuseTex", diff_rtex);
				final_mat.SetTexture("_BumpTex", bump_rtex);
				final_mat.SetTexture("_PropertyTex", prop_tex);
				final_mat.SetVector("_Settings1", Setting1);
				
				// Add to VCMatManager
				VCMatManager.Instance.m_mapMaterials.Add(_guid, final_mat);
				VCMatManager.Instance.m_mapDiffuseTexs.Add(_guid, diff_rtex);
				VCMatManager.Instance.m_mapBumpTexs.Add(_guid, bump_rtex);
				VCMatManager.Instance.m_mapPropertyTexs.Add(_guid, prop_tex);
				VCMatManager.Instance.m_mapMatRefCounters.Add(_guid, 0);
			}
		}
		
		//
		// Render Material's Texture
		//
		
		m_TexturePlanesGroup.gameObject.SetActive(true);
		
		// Gen diffuse
		for ( int i = 0; i < mat_list.Length && i < VCIsoData.MAT_ARR_CNT; ++i )
		{
			// Set diffuses on each piece of plane.
			if ( mat_list[i] != null )
				m_PlaneRenderers[i].material.SetTexture("_MainTex", mat_list[i].m_DiffuseTex);
			else
				m_PlaneRenderers[i].material.SetTexture("_MainTex", Resources.Load(VCMaterial.s_BlankDiffuseRes) as Texture2D);
		}
		m_TextureCam.targetTexture = diff_rtex;
		m_TextureCam.Render();	// render diffuse
		
		// Gen bump
		for ( int i = 0; i < mat_list.Length && i < VCIsoData.MAT_ARR_CNT; ++i )
		{
			// Set normal maps on each piece of plane.
			if ( mat_list[i] != null )
				m_PlaneRenderers[i].material.SetTexture("_MainTex", mat_list[i].m_BumpTex);
			else
				m_PlaneRenderers[i].material.SetTexture("_MainTex", Resources.Load(VCMaterial.s_BlankBumpRes) as Texture2D);
		}
		m_TextureCam.targetTexture = bump_rtex;
		m_TextureCam.Render();	// render bump
		
		// Gen properties
		for ( int i = 0; i < mat_list.Length && i < VCIsoData.MAT_ARR_CNT; ++i )
		{
			// Property texture has several property track in y direction
			// each track have n pixels (n = VCIsoData.MAT_ARR_CNT) in x direction,
			// each pixel represents the property or the combination of several properties for one material.
			if ( mat_list[i] != null )
			{
				prop_tex.SetPixel(i, 0, mat_list[i].m_SpecularColor);
				prop_tex.SetPixel(i, 1, mat_list[i].m_EmissiveColor);
				prop_tex.SetPixel(i, 2, new Color(mat_list[i].m_BumpStrength, mat_list[i].m_SpecularStrength/2, mat_list[i].m_SpecularPower / 255, 1));
				prop_tex.SetPixel(i, 3, new Color(mat_list[i].m_Tile/17,0,0,1));
			}
			else
			{
				prop_tex.SetPixel(i, 0, Color.black);
				prop_tex.SetPixel(i, 1, Color.black);
				prop_tex.SetPixel(i, 2, new Color(0,0,5/255,1));
				prop_tex.SetPixel(i, 3, new Color(1/17,0,0,1));
			}
		}
		prop_tex.Apply();
		
		m_TextureCam.targetTexture = null;
		m_TexturePlanesGroup.gameObject.SetActive(false);
		
		return _guid;
	}
	
	// Generate an icon for a VCMaterial
	public void GenMaterialIcon(VCMaterial vcmat)
	{
		m_IconGenGroup.SetActive(true);
		if ( vcmat.m_Icon == null )
		{
			vcmat.m_Icon = new RenderTexture (64, 64, 24, RenderTextureFormat.ARGB32);
		}
		m_IconCubeRenderer.material.mainTexture = vcmat.m_DiffuseTex;
		m_IconCubeRenderer.material.SetTexture("_BumpMap", vcmat.m_BumpTex);
		m_IconGenCamera.targetTexture = vcmat.m_Icon;
		Color oldambient = RenderSettings.ambientLight;
		RenderSettings.ambientLight = Color.black;
		m_IconGenCamera.Render();
		RenderSettings.ambientLight = oldambient;
		m_IconGenGroup.SetActive(false);
	}
}
