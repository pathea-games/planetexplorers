using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Editor Scene
public class VCEScene
{
	// new scene
	public VCEScene (VCESceneSetting setting)
	{
		New(setting);
	}
	public VCEScene (string iso_path)
	{
		m_IsoData = new VCIsoData ();
		if ( !LoadIso(iso_path) )
		{
			Destroy();
			throw new Exception("Load ISO error");
		}
		
		VCESceneSetting isosetting = m_IsoData.m_HeadInfo.FindSceneSetting();
		
		if ( isosetting != null )
		{
			m_Setting = isosetting;
			m_Stencil = new VCIsoData ();
			m_Stencil.Init(VCIsoData.ISO_VERSION, m_Setting, new VCIsoOption(true));
			m_TempIsoMat.Init();
			m_DocumentPath = iso_path;
			VCEditor.Instance.m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
			m_MeshComputer = new VCMCComputer ();
			m_MeshComputer.Init(m_Setting.m_EditorSize, VCEditor.Instance.m_MeshMgr);
			m_CreationAttr = new CreationAttr ();
		}
		else
		{
			Destroy();
			throw new Exception("Scene setting error");
		}
	}
	public VCEScene (VCESceneSetting setting, int template)
	{
		TextAsset asset = Resources.Load<TextAsset>("Isos/" + setting.m_Id.ToString() + "/" + template.ToString());
		if (asset == null)
		{
			New(setting);
			return;
		}

		m_IsoData = new VCIsoData ();
		if (!m_IsoData.Import(asset.bytes, new VCIsoOption(true)))
		{
			Destroy();
			throw new Exception("Load Template ISO error");
		}

		VCESceneSetting isosetting = m_IsoData.m_HeadInfo.FindSceneSetting();

		if ( isosetting != null )
		{
			m_Setting = isosetting;
			m_Stencil = new VCIsoData ();
			m_Stencil.Init(VCIsoData.ISO_VERSION, m_Setting, new VCIsoOption(true));
			m_TempIsoMat.Init();
			m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled" + VCConfig.s_IsoFileExt;
			int i = 2;
			while ( File.Exists(VCConfig.s_IsoPath + m_DocumentPath) )
			{
				m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled (" + i.ToString() + ")" + VCConfig.s_IsoFileExt;
				++i;
			}
			VCEditor.Instance.m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
			m_MeshComputer = new VCMCComputer ();
			m_MeshComputer.Init(m_Setting.m_EditorSize, VCEditor.Instance.m_MeshMgr);
			m_CreationAttr = new CreationAttr ();
		}
		else
		{
			Destroy();
			throw new Exception("Scene setting error");
		}
	}
	
	private void New (VCESceneSetting setting)
	{
		m_Setting = setting;
		m_IsoData = new VCIsoData ();
		m_IsoData.Init(VCIsoData.ISO_VERSION, setting, new VCIsoOption(true));
		m_IsoData.m_HeadInfo.Category = setting.m_Category;
		m_Stencil = new VCIsoData ();
		m_Stencil.Init(VCIsoData.ISO_VERSION, setting, new VCIsoOption(true));
		m_TempIsoMat.Init();
		m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled" + VCConfig.s_IsoFileExt;
		int i = 2;
		while ( File.Exists(VCConfig.s_IsoPath + m_DocumentPath) )
		{
			m_DocumentPath = VCConfig.s_Categories[setting.m_Category].m_DefaultPath + "/Untitled (" + i.ToString() + ")" + VCConfig.s_IsoFileExt;
			++i;
		}
		VCEditor.Instance.m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
		m_MeshComputer = new VCMCComputer ();
		m_MeshComputer.Init(m_Setting.m_EditorSize, VCEditor.Instance.m_MeshMgr);
		m_CreationAttr = new CreationAttr ();
#if false
		this.GenSomeVoxelForTest();
#endif
	}

#if false
	private void GenSomeVoxelForTest()
	{
		for ( int x = 0; x < 31; ++x )
		{
			for ( int y = 0; y < 12; ++y )
			{
				for ( int z = 0; z < 31; ++z )
				{
					if ( (x+y+z) % 3 == 0 )
					{
						VCVoxel voxel = new VCVoxel((byte)(UnityEngine.Random.value*128 + 128), 1);
						m_IsoData.SetVoxel(VCIsoData.IPosToKey(new IntVector3(x,y,z)), voxel);
						m_MeshComputer.AlterVoxel(x,y,z,voxel);
					}
				}
			}
		}
	}
#endif
	
	/// <summary>
	/// Builds the scene when iso was loaded but object/vcmesh haven't been created.
	/// </summary>
	public void BuildScene()
	{
		// Materials
		this.GenerateIsoMat();
		// Components
		foreach ( VCComponentData cdata in m_IsoData.m_Components )
		{
			cdata.CreateEntity(true, null);
		}
		// Voxels
		foreach ( KeyValuePair<int, VCVoxel> kvp in m_IsoData.m_Voxels )
		{
			m_MeshComputer.AlterVoxel(kvp.Key, kvp.Value);
		}
		
		// Colors
		// ...
	}
	
	// destroy old scene
	public void Destroy ()
	{
		if ( m_IsoData != null )
		{
			m_IsoData.Destroy();
			m_IsoData = null;
		}
		if ( m_Stencil != null )
		{
			m_Stencil.Destroy();
			m_Stencil = null;
		}
		m_TempIsoMat.Destroy();
		if ( m_MeshComputer != null )
		{
			m_MeshComputer.Destroy();
			m_MeshComputer = null;
		}
		VCEditor.Instance.m_MeshMgr.FreeGameObjects();
	}
	
	// Settings
	public VCESceneSetting m_Setting;
	// Iso
	public VCIsoData m_IsoData = null;
	// Stencil for some edit behaviour
	public VCIsoData m_Stencil = null;
	
	public struct ISOMat
	{
		public Material m_EditorMat; // Picked from VCEditor, DO NOT DESTROY
		public RenderTexture m_DiffTex;
		public RenderTexture m_BumpTex;
		public Texture2D m_PropertyTex;
		
		public void Init ()
		{
			m_EditorMat = VCEditor.Instance.m_TempIsoMat;
			m_DiffTex = new RenderTexture (VCIsoData.MAT_COL_CNT*VCMaterial.TEX_RESOLUTION, 
				                                         VCIsoData.MAT_ROW_CNT*VCMaterial.TEX_RESOLUTION,
				                                         0, RenderTextureFormat.ARGB32);
			m_BumpTex = new RenderTexture (VCIsoData.MAT_COL_CNT*VCMaterial.TEX_RESOLUTION, 
				                                         VCIsoData.MAT_ROW_CNT*VCMaterial.TEX_RESOLUTION,
				                                         0, RenderTextureFormat.ARGB32);
			m_PropertyTex = new Texture2D (VCIsoData.MAT_ARR_CNT, 4, TextureFormat.ARGB32, false, false);
			
			m_DiffTex.anisoLevel = 4;
			m_DiffTex.filterMode = FilterMode.Trilinear;
			m_DiffTex.useMipMap = true;
			m_DiffTex.wrapMode = TextureWrapMode.Repeat;
			
			m_BumpTex.anisoLevel = 4;
			m_BumpTex.filterMode = FilterMode.Trilinear;
			m_BumpTex.useMipMap = true;
			m_BumpTex.wrapMode = TextureWrapMode.Repeat;
			
			m_PropertyTex.anisoLevel = 0;
			m_PropertyTex.filterMode = FilterMode.Point;
		}
		
		public void Destroy ()
		{
			m_EditorMat.SetTexture("_DiffuseTex", null);
			m_EditorMat.SetTexture("_BumpTex", null);
			m_EditorMat.SetTexture("_PropertyTex", null);
			if ( m_DiffTex != null )
			{
				RenderTexture.Destroy(m_DiffTex);
				m_DiffTex = null;
			}
			if ( m_BumpTex != null )
			{
				RenderTexture.Destroy(m_BumpTex);
				m_BumpTex = null;
			}
			if ( m_PropertyTex != null )
			{
				RenderTexture.Destroy(m_PropertyTex);
				m_PropertyTex = null;
			}
		}
	}
	
	public ISOMat m_TempIsoMat;

	public void PreventRenderTextureLost()
	{
		if (!m_TempIsoMat.m_DiffTex.IsCreated() || !m_TempIsoMat.m_BumpTex.IsCreated())
		{
			GenerateIsoMat();
		}
	}
	public void GenerateIsoMat()
	{
		if ( VCMatGenerator.Instance != null )
		{
			VCMaterial[] _matlist = null;
			if ( m_IsoData != null && m_IsoData.m_Materials != null )
				_matlist = m_IsoData.m_Materials;
			else
				_matlist = new VCMaterial [VCIsoData.MAT_ARR_CNT];
			VCMatGenerator.Instance.GenMeshMaterial(_matlist, true);
		}
	}
	
	public VCMCComputer m_MeshComputer = null;
	public CreationAttr m_CreationAttr = null;
    public static event Action OnSaveIso;

	//
	// Document management
	//
	/// <summary>
	/// The current iso document path. e.g. "Sword/Testdir/MyIso.vciso"
	/// If it is a new document, e.g. "[Category]/Untitled.vciso";
	/// </summary>
	public string m_DocumentPath = "";
	public bool SaveIso()
	{
		try
		{
			string fullpath = VCConfig.s_IsoPath + m_DocumentPath;
			string dir = new FileInfo(fullpath).Directory.FullName;
			if ( !Directory.Exists(dir) )
				Directory.CreateDirectory(dir);
			using ( FileStream fs = new FileStream (fullpath, FileMode.Create, FileAccess.Write) )
			{
				byte[] iso_buffer = m_IsoData.Export();
				fs.Write(iso_buffer, 0, iso_buffer.Length);
				fs.Close();
			}
			// History manager
			VCEHistory.s_Modified = false;
            if (OnSaveIso != null)
                OnSaveIso();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
	/// <summary>
	/// Saves the iso with new name.
	/// </summary>
	/// <param name='name'>
	/// Name, The path under VCConfig.s_IsoPath dir. e.g. "Sword/Testdir/MyIso.vciso"
	/// </param>
	public bool SaveIsoAs(string name)
	{
		m_DocumentPath = name;
		return SaveIso();
	}
	/// <summary>
	/// Loads an iso.
	/// </summary>
	/// <param name='path'>
	/// Path. The path under VCConfig.s_IsoPath dir.  e.g. "Sword/Testdir/MyIso.vciso"
	/// </param>
	private bool LoadIso(string path)
	{
		try
		{
			m_DocumentPath = path;
			string fullpath = VCConfig.s_IsoPath + m_DocumentPath;
			using ( FileStream fs = new FileStream (fullpath, FileMode.Open, FileAccess.Read, FileShare.Read) )
			{
				byte[] iso_buffer = new byte [(int)(fs.Length)];
				fs.Read(iso_buffer, 0, (int)(fs.Length));
				fs.Close();
				return m_IsoData.Import(iso_buffer, new VCIsoOption(true));
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning("Loading ISO Error : " + e.ToString());
			return false;
		}
	}
}
