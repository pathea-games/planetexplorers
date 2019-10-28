using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveScene : MonoBehaviour 
{
	private static WaveScene s_Self = null;
	public static WaveScene Self { get{ return s_Self;} }

	public Camera  RenderCam;
	public int WaveLayer;

	public enum ETexPrecision
	{
		Low = 512,
		Medium = 1024,
		High = 2048
	}

	public ETexPrecision TexurePrecision = ETexPrecision.Low;

	// Tracer
	private List<WaveTracer> m_Tracers = new List<WaveTracer>();
	public void AddTracer (WaveTracer tracer)
	{
		m_Tracers.Add(tracer);
		tracer.curIntervalTime = tracer.IntervalTime + 0.001f;
	}

	#region Materials

	public Material SpotWaveMat;
	
	public Material LineWaveMat;

	private Material m_SpotWaveMat;
	private Material m_LineWaveMat;
	#endregion
	

	public RenderTexture RenderTarget { get { return m_RenderTex; } }

	private GameObject m_Go;
	private Camera m_Cam;
	RenderTexture m_RenderTex;

	#region CREATE_WAVE

	SpotWaveHandler _createSpotWave (WaveTracer tracer, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		if (!enabled )
			return null;

		GameObject go = GetGo();
		go.name = "spot Wave";
		go.GetComponent<Renderer>().material = m_SpotWaveMat;
		go.transform.position = pos;
		go.transform.localRotation = Quaternion.Euler(90, 0, 0);

		SpotWaveHandler spotWave = go.AddComponent<SpotWaveHandler>();
		spotWave.onRecycle += RecycleGo;
		go.layer = WaveLayer;
		spotWave.Init(tracer);
		return spotWave;
	}

	LineWaveHandler _createLineWave (WaveTracer tracer)
	{
		if (!enabled )
			return null;

		GameObject go = GetGo();
		go.name = "Line Wave";
		go.GetComponent<Renderer>().material = m_LineWaveMat;

		LineWaveHandler lineWave = go.AddComponent<LineWaveHandler>();
		lineWave.onRecycle += RecycleGo;
		go.layer = WaveLayer;
		lineWave.Init(tracer);
		return lineWave;

	}
	#endregion

	#region GameObject_Pool

	Queue<GameObject> m_GoPool;

	const int c_PoolGrowCount = 50;

	public int WaveCnt = 0;

	GameObject GetGo ()
	{
		if (m_GoPool.Count == 0)
		{
			for (int i = 0; i < c_PoolGrowCount; i++)
			{
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
				go.name = "Wave Object";
				go.transform.parent = transform;
				go.GetComponent<Collider>().enabled = false;
				go.gameObject.SetActive(false);

				m_GoPool.Enqueue(go);
			}
		}

		GameObject dgo = m_GoPool.Dequeue();
		dgo.SetActive(true);
		return dgo;
	}

	void RecycleGo(GameObject go)
	{
		go.gameObject.SetActive(false);

		go.name = "Wave Object";

		m_GoPool.Enqueue(go);
	}

	#endregion
	

	#region UNITY_INNER_FUNCTION

	void Awake ()
	{
		m_GoPool = new Queue<GameObject>();

		gameObject.layer = WaveLayer;
		if (RenderCam != null)
		{
			RenderCam.gameObject.SetActive(true);
			RenderCam.enabled = true;
		}

		m_SpotWaveMat = Instantiate(SpotWaveMat) as Material;
		m_LineWaveMat = Instantiate(LineWaveMat) as Material;

		s_Self = this;
	}

	void Destroy ()
	{
		if (m_RenderTex != null)
			m_RenderTex.Release();
	}
	

	void Update ()
	{
		if (m_Cam != RenderCam)
		{
			if (m_Cam != null)
			{
				m_Cam.targetTexture = null;
			}

			m_Cam = RenderCam;
			if (m_Cam != null)
			{
				RenderCam.cullingMask = (1 << WaveLayer);
				RenderCam.gameObject.SetActive(true);
				RenderCam.enabled = true;

				if (m_RenderTex != null)
				{
					if ( m_RenderTex.width != (int)TexurePrecision)
					{
						m_RenderTex.Release();
						m_RenderTex = new RenderTexture((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
					}
				}
				else
					m_RenderTex = new RenderTexture((int)TexurePrecision, (int)TexurePrecision, 16, RenderTextureFormat.ARGBHalf);
			}
		}

		if (m_Cam != null)
		{
			m_Cam.nearClipPlane = Camera.main.nearClipPlane;
			m_Cam.farClipPlane = Camera.main.farClipPlane;
			m_Cam.aspect = Camera.main.aspect;
			m_Cam.fieldOfView = Camera.main.fieldOfView;
			m_Cam.targetTexture = m_RenderTex;
		}

		// Create Wave
		for (int i = 0; i < m_Tracers.Count; i++)
		{

			WaveTracer tracer = m_Tracers[i];

			// Line wave
			if (tracer.WaveType == EWaveType.Line)
			{
				if (!m_Tracers[i].enabled)
				{
					if (tracer.lastWave != null)
					{
						tracer.lastWave.LockTracer();
						tracer.lastWave = null;
					}
					continue;
				}

				if (tracer.AutoGenWave)
				{
					if (!tracer.canGen)
					{
						if (Vector3.Magnitude( tracer.prevPos - tracer.Position) > 0.05f)
						{
							tracer.canGen = true;
						}
					}
					else
					{
						tracer.prevPos = tracer.Position;

					}

					if (tracer.curIntervalTime >= tracer.IntervalTime)
					{
						if (tracer.lastWave != null)
						{
							tracer.lastWave.LockTracer();
							tracer.lastWave = null;
						}

						if (tracer.canGen)
						{
							tracer.lastWave = _createLineWave(tracer);
							
							tracer.curIntervalTime = 0;
							
							tracer.canGen = false;
						}
					}
					else
					{
						tracer.curIntervalTime += Time.deltaTime;
						
					}
				}
				else
				{
					if (tracer.lastWave != null)
					{
						tracer.lastWave.LockTracer();
						tracer.lastWave = null;
					}

					tracer.curIntervalTime = tracer.IntervalTime + 0.001f;
					tracer.canGen = false;
				}
			}
			// spot wave
			else if (tracer.WaveType == EWaveType.Spot)
			{
				if (!m_Tracers[i].enabled)
					continue;

				if (tracer.AutoGenWave)
				{

					if (tracer.curIntervalTime >= tracer.IntervalTime)
					{

						Vector3 pos = tracer.transform.position;
						pos = new Vector3(pos.x, 95.5f, pos.z);
						_createSpotWave(tracer, pos, Quaternion.Euler(new Vector3(90, 0, 0)), Vector3.one);
						
						tracer.curIntervalTime = 0;
						
						tracer.canGen = false;
					}
					else
					{
						tracer.curIntervalTime += Time.deltaTime;
						
					}
				}
			}
		}

		WaveCnt = transform.childCount;

	}

	#endregion
	
}
