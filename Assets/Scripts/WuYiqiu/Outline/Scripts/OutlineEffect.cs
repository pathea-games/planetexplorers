using UnityEngine;
using System.Collections;

public class OutlineEffect : MonoBehaviour 
{
	public delegate void outliningEventHandler(bool active);
	public static event outliningEventHandler OutliningEventHandler;

	public LayerMask layerMask;

	public int stencilBufferDepth = 16;

	#region BLUR_ABOUT
	// Stencil  buffer size downsample factor
	public int downsampleFactor = 4;
	// Blur iterations
	public int iterations = 2;
	// Blur minimal spread
	public float blurMinSpread = 0.65f;
	// Blur spread per iteration
	public float blurSpread = 0.25f;
	// Blurring intensity for the blur material
	public float blurIntensity = 0.3f;

	// Blur Shader
	private static Shader m_BlurShader;
	private static Shader blurShader
	{
		get
		{
			if (m_BlurShader == null)
				m_BlurShader = Shader.Find("wuyiqiu/Outline/Blur");
//				m_BlurShader = Shader.Find("Hidden/Highlighted/Blur");
			return m_BlurShader;
		}
	}

	// Blur Material
	private static Material m_BlurMat = null;
	private static Material blurMaterial
	{
		get
		{
			if (m_BlurMat == null)
			{
				m_BlurMat = new Material(blurShader);
				m_BlurMat.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMat;
		}
	}

	#endregion

	private static Shader m_CompShader = null;
	private static Shader compShader
	{
		get
		{
			if (m_CompShader == null)
				m_CompShader = Shader.Find("wuyiqiu/Outline/Composite");
//				m_CompShader = Shader.Find("Hidden/Highlighted/Composite");
			return m_CompShader;
		}
	}

	private static Material m_CompMat = null;
	private static Material compMaterial
	{
		get
		{
			if (m_CompMat == null)
			{
				m_CompMat = new Material(compShader);
				m_CompMat.hideFlags = HideFlags.DontSave;
			}
			return m_CompMat;
		}
	}

	private RenderTexture m_StencilBuffer; 

	//private int m_StencilBufferDepth;
	
	private GameObject m_StencilRendererGo;

	

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnDestroy()
	{
		if (m_StencilRendererGo != null)
			DestroyImmediate(m_StencilRendererGo);

		if (m_StencilBuffer != null)
			RenderTexture.ReleaseTemporary(m_StencilBuffer);

		if (m_BlurMat != null)
			DestroyImmediate(m_BlurMat);

		if (m_CompMat != null)
			DestroyImmediate(m_CompMat);
	}

	void OnPreRender()
	{
		if (m_StencilBuffer != null)
		{
			RenderTexture.ReleaseTemporary(m_StencilBuffer);
			m_StencilBuffer = null;
		}


		if (this.enabled == false || gameObject.activeInHierarchy == false)
			return;


		if (OutliningEventHandler == null)
			return;
		else
		{
			OutliningEventHandler(true);
		}

		m_StencilBuffer = RenderTexture.GetTemporary((int)GetComponent<Camera>().pixelWidth, (int)GetComponent<Camera>().pixelHeight, stencilBufferDepth, RenderTextureFormat.ARGB32);

		Camera cam = null;

		if (m_StencilRendererGo == null)
		{
			m_StencilRendererGo = new GameObject("Stencil renderer camera" + GetInstanceID(), typeof(Camera), typeof(Skybox));
			//go.hideFlags = HideFlags.HideAndDontSave;

		}

		cam = m_StencilRendererGo.GetComponent<Camera>();
		cam.enabled = false;

		cam.CopyFrom(GetComponent<Camera>());
		cam.cullingMask = layerMask;
		cam.renderingPath = RenderingPath.VertexLit;
		cam.rect =  new Rect(0f, 0f, 1f, 1f);
		cam.hdr = false;
		cam.useOcclusionCulling = false;
		cam.backgroundColor = Color.clear;
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.targetTexture = m_StencilBuffer;
		cam.Render();

		if (OutliningEventHandler != null)
			OutliningEventHandler(false);
	}

	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float off = blurMinSpread + iteration * blurSpread;
		blurMaterial.SetFloat("_OffsetScale", off);
		Graphics.Blit(source, dest, blurMaterial);
	}
	
	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		float off = 1.0f;
		blurMaterial.SetFloat("_OffsetScale", off);
		Graphics.Blit(source, dest, blurMaterial);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (m_StencilBuffer == null)
		{
			Graphics.Blit(source, destination);
			return;
		}

		int width = source.width  / downsampleFactor;
		int heigt = source.height / downsampleFactor;
		RenderTexture buff 	= RenderTexture.GetTemporary(width, heigt, stencilBufferDepth, RenderTextureFormat.ARGB32);
		RenderTexture buff2 = RenderTexture.GetTemporary(width, heigt, stencilBufferDepth, RenderTextureFormat.ARGB32);

		DownSample4x(m_StencilBuffer, buff);

		bool flag = false;
		for (int i = 0; i < iterations; i++)
		{
			if (!flag)
				FourTapCone(buff, buff2, i);
			else
				FourTapCone(buff2, buff, i);

			flag = !flag;
		}

//		if (!flag)
//			Graphics.Blit(buff2, destination);
//		else
//			Graphics.Blit(buff, destination);
		compMaterial.SetTexture("_StencilTex", m_StencilBuffer);
		compMaterial.SetTexture("_BlurTex", flag ? buff : buff2);
		Graphics.Blit(source, destination, compMaterial);

		RenderTexture.ReleaseTemporary(buff);
		RenderTexture.ReleaseTemporary(buff2);
	}
}
