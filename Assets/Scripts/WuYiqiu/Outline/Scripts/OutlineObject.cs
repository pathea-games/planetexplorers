using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OutlineObject : MonoBehaviour 
{
	public static int highlightingLayer = 26;

	// Color 
	private Color outlineColor = Color.green;
	

	private static Shader  m_OpaqueShader;
	public static Shader opaqueShader 
	{
		get
		{
			if (m_OpaqueShader == null)
				m_OpaqueShader =  Shader.Find("wuyiqiu/Outline/StencilZ");
			return m_OpaqueShader;
		}
	}

	private Material m_OpaqueMat;
	public  Material opaqueMat
	{
		get
		{
			if (m_OpaqueMat == null)
			{
				m_OpaqueMat = new Material(opaqueShader);
				m_OpaqueMat.hideFlags = HideFlags.DontSave;
				m_OpaqueMat.SetColor("_Outline", outlineColor);
			}
			return m_OpaqueMat;
		}
	}

	private class RendererCache
	{
		public Renderer m_Renderer;
		public GameObject m_Go;
		private Material[] m_SourceMats;
		private Material[] m_ReplacedMats;

		public RendererCache (Renderer rend, Material[] mats, Material sharedOpaqueMaterial)
		{
			m_Renderer = rend;
			m_SourceMats = mats;
			m_Go = rend.gameObject;

			m_ReplacedMats = new Material[mats.Length];
			for (int i = 0; i < mats.Length; i ++)
			{
				m_ReplacedMats[i] = sharedOpaqueMaterial;
			}
		}

		public void SetMaterialsState(bool state)
		{
			m_Renderer.sharedMaterials = state ? m_ReplacedMats : m_SourceMats;
		}
		public void ReplaceSource(Material old_mat, Material new_mat)
		{
			for ( int i = 0; i < m_SourceMats.Length; ++i )
			{
				if ( m_SourceMats[i] == old_mat )
				{
					m_SourceMats[i] = new_mat;
				}
			}
		}
	}

	private List<RendererCache> m_RendererCaches;

	private bool m_Once = true;

	private LayerMask[] m_LayerMasks;

	// Occluder
	public bool m_Occluder = false;
	//public bool Occluder  { get {return m_Occluder;} }

	// Occlusion color 
	private readonly Color occluderColor = new Color(0.0f, 0.0f, 0.0f, 0.005f);

	public void ReplaceInCache( Material old_mat, Material new_mat )
	{
		if (m_RendererCaches == null)
			return;
		foreach ( RendererCache rc in m_RendererCaches )
			rc.ReplaceSource(old_mat, new_mat);
	}

	void InitMaterial ()
	{
		MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
		if (mr != null)
		{
			for (int i = 0; i < mr.Length; i++)
			{
				Material[] mats = mr[i].sharedMaterials;

				m_RendererCaches.Add(new RendererCache(mr[i], mats, opaqueMat) ) ;
			}
		}

		SkinnedMeshRenderer[] smr = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (smr != null)
		{
			for (int i = 0; i < smr.Length; i++)
			{
				Material[] mats = smr[i].sharedMaterials;

				m_RendererCaches.Add(new RendererCache(smr[i], mats, opaqueMat) );
			}
		}

	}


	static int _outlinePropertyID = -1;
	static int outlinePropertyID
	{
		get
		{
			if (_outlinePropertyID == -1)
			{
				_outlinePropertyID = Shader.PropertyToID("_Outline");
            }

			return _outlinePropertyID;
        }
	}


	public Color color
	{
		get { return outlineColor; }
		set
		{
			outlineColor = value;
			opaqueMat.SetColor(outlinePropertyID, value);
		}
	}


	void OutliningEventHandler (bool active)
	{
		if (active)
		{
			bool state = (m_Once || m_Occluder);
			if (state )
			{
				// Color
				if (m_Occluder)
				{
					color = occluderColor;
					m_Occluder = false;
				}

				m_RendererCaches = new List<RendererCache>();
				InitMaterial ();
				m_Once = false;
				m_LayerMasks = new LayerMask[m_RendererCaches.Count];
			}


			for (int i = 0; i < m_RendererCaches.Count; i++)
			{
				if (m_RendererCaches[i].m_Go == null)
				{
					m_Once = true;
					continue;
				}
				m_LayerMasks[i] = m_RendererCaches[i].m_Go.layer;
				m_RendererCaches[i].m_Go.layer = highlightingLayer;
				m_RendererCaches[i].SetMaterialsState(true);
			}
		}
		else
		{
			for (int i = 0; i < m_RendererCaches.Count; i++)
			{
				if (m_RendererCaches[i].m_Go == null)
					continue;

				m_RendererCaches[i].m_Go.layer = m_LayerMasks[i];
				m_RendererCaches[i].SetMaterialsState(false);
			}
		}
	}

	void OnEnable ()
	{
		m_Once = true;

		OutlineEffect.OutliningEventHandler += OutliningEventHandler;
	}

	void OnDisable ()
	{
		OutlineEffect.OutliningEventHandler -= OutliningEventHandler;

		if (m_OpaqueMat != null)
			DestroyImmediate(m_OpaqueMat);
	}

	void OnDestroy ()
	{
		OutlineEffect.OutliningEventHandler -= OutliningEventHandler;
		
		if (m_OpaqueMat != null)
			DestroyImmediate(m_OpaqueMat);
	}
	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{

	}
}
