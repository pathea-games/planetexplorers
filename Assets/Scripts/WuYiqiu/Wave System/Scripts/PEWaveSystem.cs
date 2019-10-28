#define GL_VERSION

using UnityEngine;
using System.Collections;
using Pathea;

public class PEWaveSystem : MonoBehaviour 
{
#if false
	private static PEWaveSystem s_Self;
	public static PEWaveSystem Self { get { return s_Self;} }

#if GL_VERSION
	private WaveRenderer m_WaveRenderer;
	public WaveRenderer WaveRenderer  { get {return m_WaveRenderer;} }
 #else
	private WaveScene scene = null;
#endif

	public RenderTexture Target
	{
		get
		{
			#if GL_VERSION
			return m_WaveRenderer.RenderTarget;
			#else
			return scene.RenderTarget;
			#endif
		}
	}

	void Awake()
	{

		GameObject go = new GameObject("Wave Camera");
		Camera cam = go.AddComponent<Camera>();
		go.transform.parent = Camera.main.transform;
		go.transform.localPosition  = Vector3.zero;
		go.transform.localRotation  = Quaternion.identity;
		go.transform.localScale     = Vector3.one; 

		cam.clearFlags = CameraClearFlags.Color;
		cam.backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
		cam.cullingMask = 0;

#if GL_VERSION
		m_WaveRenderer = cam.gameObject.AddComponent<WaveRenderer>();
#else
		scene = gameObject.GetComponent<WaveScene>();
		

	
		
		scene.RenderCam = cam;
#endif

		s_Self = this;
	}

#endif

	private static PEWaveSystem s_Self;
	public static PEWaveSystem Self { get { return s_Self;} }

	private WaveRenderer m_WaveRenderer;
	public WaveRenderer WaveRenderer  { get {return m_WaveRenderer;} }

	private WaveRenderer m_GrassWaveRenderer; 
	public WaveRenderer GrassWaveRenderer { get {return m_GrassWaveRenderer;}}

	public RenderTexture Target
	{
		get
		{
			return m_WaveRenderer.RenderTarget;
		}
	}

	public RenderTexture GrassTarget
	{
		get
		{
			return m_GrassWaveRenderer.RenderTarget;
		}
	}

	void Awake()
	{
		// Water Wave Camera
		GameObject go = new GameObject("Water Wave Camera");
		Camera cam = go.AddComponent<Camera>();
//		go.transform.parent = Camera.main.transform;
		go.transform.localPosition  = Vector3.zero;
		go.transform.localRotation  = Quaternion.identity;
		go.transform.localScale     = Vector3.one; 
		
		cam.clearFlags = CameraClearFlags.Color;
		cam.backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
		cam.cullingMask = 0;
        cam.depth = -2;

		m_WaveRenderer = cam.gameObject.AddComponent<WaveRenderer>();
		m_WaveRenderer.CameraMode = WaveRenderer.ECameraMode.Perspective;

		// Grass Wave Camera
		go = new GameObject("Grass Wave Camera");
		cam = go.AddComponent<Camera>();
//		go.transform.parent = Camera.main.transform;
		go.transform.localPosition  = Vector3.zero;
		go.transform.localRotation  = Quaternion.identity;
		go.transform.localScale     = Vector3.one; 

		cam.clearFlags = CameraClearFlags.Color;
		cam.backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
		cam.cullingMask = 0;
        cam.depth = -2;

		m_GrassWaveRenderer = cam.gameObject.AddComponent<WaveRenderer>();
		m_GrassWaveRenderer.CameraMode = WaveRenderer.ECameraMode.Orthographical;


		s_Self = this;
	}

	void Update()
	{

		PeGrassSystem.SetWaveTexture(GrassWaveRenderer.RenderTarget);
		if (GrassWaveRenderer.RenderTarget != null)
		{
			Vector4 wave_center = Vector4.zero;
			if (Pathea.PeCreature.Instance.mainPlayer == null)
			{
				Vector3 pos = GrassWaveRenderer.transform.position;
				wave_center = new Vector4(pos.x, pos.z, GrassWaveRenderer.RenderTarget.width, GrassWaveRenderer.RenderTarget.height);
			}
			else
			{
				PeTrans trans = Pathea.PeCreature.Instance.mainPlayer.peTrans;
				GrassWaveRenderer.FollowTrans = trans.trans;
				Vector3 pos = GrassWaveRenderer.transform.position;
				wave_center = new Vector4(pos.x, pos.z, GrassWaveRenderer.RenderTarget.width, GrassWaveRenderer.RenderTarget.height);
			}

			PeGrassSystem.SetWaveCenter(wave_center);
		}


	}
}
