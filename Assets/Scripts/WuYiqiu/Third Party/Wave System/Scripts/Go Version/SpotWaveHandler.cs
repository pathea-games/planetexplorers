using UnityEngine;
using System.Collections;

public class SpotWaveHandler : WaveHandler
{
	private WaveTracer m_Tracer = null;
	private bool m_Running = false;
	

	private float speed;

	private Material material;
	private float duration = 0.0f;
	private Transform trans;

	float waterHeight;
	float maxDuration;
	float minScale;
	float timeOffsetFactor;
	float strength;
	float frequency;

	float defaultScale;
	float defaultScaleFactor;
	float scaleRate;
	
	public override void  Init (WaveTracer tracer)
	{
		m_Tracer = tracer;
		m_Running = true;

		speed    		= tracer.WaveSpeed + Random.Range(0, tracer.SpotWave.speedFactorRandom); 
		waterHeight 	= m_Tracer.WaterAttribute.Height;
		maxDuration 	= m_Tracer.WaveDuration;
		minScale		= m_Tracer.SpotWave.minScale;

		timeOffsetFactor	= m_Tracer.TimeOffsetFactor;
		strength			= m_Tracer.Strength;
		frequency			= m_Tracer.Frequency;

		defaultScale 		= tracer.DefualtScale;
		defaultScaleFactor	= tracer.DefualtScaleFactor;
		scaleRate			= tracer.ScaleRate;
	}

	void Awake()
	{
		trans = transform;
	}

	void Start()
	{
		material = GetComponent<Renderer>().material;
	}

	private float _growingScale = 0.0f;
	void LateUpdate()
	{
		if (m_Running)
		{

			if (duration <  maxDuration)
			{
				// duration
				duration += Time.deltaTime;

				// scale

//				float extra_scale = minScale +  Mathf.Sqrt(duration) * scaleGrowRate;
//				trans.localScale = new Vector3(extra_scale, extra_scale, 0);
				_growingScale += scaleRate;
				float extra_scale  = minScale + Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
				trans.localScale = new Vector3(extra_scale, extra_scale, 0);

				// speed
				material.SetFloat("_Speed", speed);
				material.SetFloat("_TimeFactor", duration + Mathf.Pow(timeOffsetFactor + 1, 0.4f) - 1);


				Vector3 pos = trans.position;
				trans.position  = new Vector3(pos.x, waterHeight, pos.z);
			}
			else// Destroy Self
			{
				Destroy(this);
				Recycle();
			}

			material.SetFloat("_Strength", strength);
			material.SetFloat("_Frequency", frequency);

		}
	}

}
