using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveRenderer : MonoBehaviour 
{
	public enum ETexPrecision
	{
		Low = 512,
		Medium = 1024,
		High = 2048
	}

	public ETexPrecision TexurePrecision = ETexPrecision.Low;

	public enum ECameraMode
	{
		Perspective,
		Orthographical
	}

	public ECameraMode CameraMode = ECameraMode.Perspective;

	/// <summary>
	/// The follow transform.
	/// </summary>
	public Transform FollowTrans;

	/// <summary>
	/// The renderer camera auto follow the followCamera
	/// </summary>
	public bool AutoFollow = true;
	
	private List<glWaveTracer> m_Tracers;
	public void Add(glWaveTracer tracer)
	{
		m_Tracers.Add(tracer);
		tracer.WaveRenderer = this;
	}

	public void Remove(glWaveTracer tracer)
	{
		m_Tracers.Remove(tracer);
		tracer.WaveRenderer = this;
	}

	public RenderTexture RenderTarget { get { return m_RenderTex; } }
	RenderTexture m_RenderTex;

	Camera m_Cam = null;

	// Static
	public static List<glWaveTracer> s_Tracers = new List<glWaveTracer>();
	private static bool s_DoInitUpdate = false;

	void Awake()
	{
		m_Tracers = new List<glWaveTracer>();
		m_Cam = gameObject.GetComponent<Camera>();

		if (m_RenderTex != null)
		{
			if ( m_RenderTex.width != (int)TexurePrecision)
			{
				RenderTexture.ReleaseTemporary(m_RenderTex);
//				m_RenderTex = new RenderTexture((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
				m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
			}
		}
		else
			m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
	}

	void Update()
	{
		// RenderTarget
		if (m_RenderTex != null)
		{
			if ( m_RenderTex.width != (int)TexurePrecision)
			{
				RenderTexture.ReleaseTemporary(m_RenderTex);
				//				m_RenderTex = new RenderTexture((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
				m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
			}
		}
		else
			m_RenderTex = RenderTexture.GetTemporary((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);

		// Cemera mode 
		m_Cam.clearFlags	  = CameraClearFlags.SolidColor;
		m_Cam.backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);

		if (CameraMode == ECameraMode.Perspective)
		{
			Camera cam = FollowTrans == null ? Camera.main : FollowTrans.gameObject.GetComponent<Camera>();

			if (cam != null)
			{
				m_Cam.nearClipPlane = cam.nearClipPlane;
				m_Cam.farClipPlane = cam.farClipPlane;
				m_Cam.aspect = cam.aspect;
				m_Cam.fieldOfView = cam.fieldOfView;
			}
			m_Cam.orthographic = false;

//			if (AutoFollow)
//			{
//				m_Cam.transform.position = cam.transform.position;
//				m_Cam.transform.rotation = cam.transform.rotation;
//			}

			m_Cam.targetTexture = m_RenderTex;	
		}
		else if (CameraMode == ECameraMode.Orthographical)
		{
			m_Cam.nearClipPlane = 0.3f;
			m_Cam.farClipPlane = 200f;
			m_Cam.orthographicSize = (float)TexurePrecision / 2;

			m_Cam.orthographic = true;

//			if (AutoFollow)
//			{
//				Transform trans = FollowTrans == null ? Camera.main.transform : FollowTrans;
//
//				Vector3 pos = trans.position;
//				m_Cam.transform.position = new Vector3(pos.x, pos.y + 10, pos.z);
//				m_Cam.transform.rotation = Quaternion.Euler(90, 0, 0);
//			}

			m_Cam.targetTexture = m_RenderTex;	
		}

		// Total Tracer Init Update
		if (!s_DoInitUpdate)
		{
			foreach ( glWaveTracer tracer in s_Tracers )
			{
				if (tracer == null)
					continue;

				tracer.InitUpdate();
			}

			s_DoInitUpdate = true;
		}

		// update
		foreach ( glWaveTracer tracer in m_Tracers )
		{
			if (tracer == null)
				continue;

			tracer.WaveRenderer = this;
			tracer.CustomUpdate();
		}

	}

	void LateUpdate ()
	{
		if (CameraMode == ECameraMode.Perspective)
		{
			Camera cam = FollowTrans == null ? Camera.main : FollowTrans.gameObject.GetComponent<Camera>();

			if (AutoFollow)
			{
				m_Cam.transform.position = cam.transform.position;
				m_Cam.transform.rotation = cam.transform.rotation;
			}
		}
		else if (CameraMode == ECameraMode.Orthographical)
		{
			if (AutoFollow)
			{
				Transform trans = FollowTrans == null ? Camera.main.transform : FollowTrans;
				
				Vector3 pos = trans.position;
				m_Cam.transform.position = new Vector3(pos.x, pos.y + 10, pos.z);
				m_Cam.transform.rotation = Quaternion.Euler(90, 0, 0);
			}
		}

		s_DoInitUpdate = false;
	}

	void OnPostRender()
	{
		foreach ( glWaveTracer tracer in m_Tracers )
		{
			// If the traver is active
			if ( tracer != null && tracer.gameObject.activeInHierarchy)
			{
				tracer.Draw();
			}
		}

	}
}
