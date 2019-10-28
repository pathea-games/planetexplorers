using UnityEngine;
using System.Collections;

public class LineWaveHandler : WaveHandler 
{
	private Vector3 m_BeginPos;
	private WaveTracer m_Tracer;

	private Vector3 m_EndPos;
	private float m_CurTime;
	private bool m_Running = false;

	private Material material;
	private Transform trans;
	private float duration = 0.0f;

	const float c_DefaultMeter = 1.0f;

	bool locked = false;

	float waterHeight;
	float strength;
	float frequency;
	float defaultScale;
	float defaultScaleFactor;
	float scaleRate;
	float maxDuration;
	float deltaTime;

	public override void Init (WaveTracer tracer)
	{
		m_Tracer = tracer;
		m_BeginPos = tracer.Position;
		m_Running = true;
		waterHeight = tracer.WaterAttribute.Height;
		strength 	= tracer.Strength;
		frequency	= tracer.Frequency;
		defaultScale 		= tracer.DefualtScale;
		defaultScaleFactor	= tracer.DefualtScaleFactor;
		scaleRate			= tracer.ScaleRate;
		maxDuration			= tracer.WaveDuration;
		deltaTime			= tracer.LineWave.DeltaTime;
	}

	public void LockTracer()
	{
		locked = true;
	}

	void Awake()
	{
		trans = transform;
	}

	void Start ()
	{
		material = GetComponent<Renderer>().material;
	}
	
	void Update()
	{
	}

	private float _growingScale = 0.0f;
	void LateUpdate ()
	{
		if (m_Running)
		{
			if (duration < maxDuration)
			{
				duration += Time.deltaTime;


				// direction
				if (!locked)
				{
					m_EndPos = m_Tracer.Position;

				}

				// Set Height First
				m_BeginPos.y = waterHeight;
				m_EndPos.y = waterHeight;

				Vector3 v = m_EndPos - m_BeginPos; 
				float mag =  new Vector2(v.x, v.z).magnitude;
				Vector3 dir = v.normalized;

				float relDis =  mag / 2;

				_growingScale += scaleRate;
				float max_scale = 50;
				float sqrt_dura =  Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
				
				if (relDis > 0.5f)
				{
					relDis = 0.5f;
					
					// position
					trans.position = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + dir*0.1f;
					// scale
					float scale = Mathf.Min(sqrt_dura + 2 * mag + _growingScale, max_scale);
					trans.localScale = new Vector3(scale, scale, 1);
					// rotate
					trans.rotation = Quaternion.identity;
					trans.right = dir;
					trans.Rotate(new Vector3(90, 0, 0));
					 
				}
				else
				{

					if (mag < 0.01f)
					{
						trans.rotation = Quaternion.identity;
						float scale = Mathf.Min( sqrt_dura + 2 + _growingScale, max_scale);
						trans.localScale = new Vector3(scale, scale);
						trans.position = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + Vector3.right* (0.5f - relDis) * scale * 0.5f;
						trans.Rotate(new Vector3(90, 0, 0));
					}
					else
					{
						trans.rotation = Quaternion.identity;
						float scale = Mathf.Min( sqrt_dura + 2 + _growingScale, max_scale);
						trans.localScale = new Vector3(scale, scale, 1);
						trans.position = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + dir * (0.5f - relDis) * scale * 0.5f;
						trans.right = dir;
						trans.Rotate(new Vector3(90, 0, 0));
					}


				}
				
				material.SetFloat("_Distance", relDis * 2);

				material.SetFloat("_Strength", strength);
				material.SetFloat("_Frequency", frequency);
				material.SetFloat("_TimeFactor", duration + Mathf.Pow(m_Tracer.TimeOffsetFactor + 1, 0.4f) - 1) ;
				material.SetFloat("_DeltaTime", deltaTime);
				//				material.SetFloat("_Speed", Mathf.Max(0.1f, m_Tracer.WaveSpeed - Mathf.Sqrt(duration) * 0.1f));
				//				material.SetFloat("_Speed", Mathf.Max(0.1f, m_Tracer.WaveSpeed * (1 - Mathf.Sqrt(duration/m_Tracer.WaveDuration) * 0.4f)));
//				material.SetFloat("_Speed", Mathf.Max(0.25f, m_Tracer.WaveSpeed * Mathf.Clamp01(1 - Mathf.Sqrt(duration/3.14159f)* 0.35f)));
				material.SetFloat("_Speed", m_Tracer.WaveSpeed);
			}
			else
			{
				Destroy(this);
				Recycle();
			}

		}
	}
}
