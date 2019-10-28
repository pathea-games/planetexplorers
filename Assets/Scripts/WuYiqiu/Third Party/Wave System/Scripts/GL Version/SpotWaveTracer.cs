using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpotWaveTracer : glWaveTracer 
{
	/// <summary>
	/// Automatically generate wave
	/// </summary>
	public bool AutoGenWave = true;
	
	/// <summary>
	/// The wave speed. recommended value(0.2 ~ 1);
	/// </summary>
	public float WaveSpeed = 0.5f;
	
	/// <summary>
	/// The  wave life time.
	/// </summary>
	public float WaveDuration = 5f;
	
	/// <summary>
	/// The wave frequency.
	/// </summary>
	public float Frequency = 40;
	
	/// <summary>
	/// The wave strength.
	/// </summary>
	public float Strength = 20;
	
	/// <summary>
	/// When wave begin, the time is set
	/// </summary>
	public float TimeOffsetFactor = 2;
	
	/// <summary>
	/// How long the manager will generate a wave
	/// </summary>
	public float IntervalTime = 0.2f;
	
	public float ScaleRate = 0.01f;

	public float DefualtScale = 0;
	
	public float DefualtScaleFactor = 2f;

	public float minScale = 1;
	
	public float speedFactorRandom = 0.1f;

	#region Parent_Override_Func
	
	float curIntervalTime;
	Drawer lastWave;
	
	public override void CustomUpdate ()
	{

		if (AutoGenWave)
		{
			
			if (curIntervalTime >= IntervalTime)
			{
				Drawer wave = new Drawer();
				wave.Init(this);
				m_Waves.Add(wave);
				
				
				curIntervalTime = 0;
				
			}
			else
			{
				curIntervalTime += Time.deltaTime;
				
			}
		}
		
		foreach (Drawer wave in m_Waves)
		{
			wave.Update();
		}
		
		for (int i = 0; i < m_Waves.Count; )
		{
			if (m_Waves[i].Destroyed)
			{
				m_Waves[i].DestroySelf();
				m_Waves.RemoveAt(i);
			}
			else
				i++;
		}
	}

	public override void Draw ()
	{
		foreach (Drawer wave in m_Waves)
		{
			wave.Draw();
		}
	}


	#endregion

	/// <summary>
	/// Spot Drawer 
	/// </summary>
	class Drawer
	{
		public bool Destroyed = false;

		private SpotWaveTracer m_Tracer = null;
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
		
		float scale = 2;
		Vector3 center = Vector3.zero;
		
		public void  Init (SpotWaveTracer tracer)
		{
			m_Tracer = tracer;
			m_Running = true;
			
			speed    		= tracer.WaveSpeed + Random.Range(0, tracer.speedFactorRandom); 
			waterHeight 	= m_Tracer.WaterHeight;
			maxDuration 	= m_Tracer.WaveDuration;
			minScale		= m_Tracer.minScale;
			
			timeOffsetFactor	= m_Tracer.TimeOffsetFactor;
			strength			= m_Tracer.Strength;
			frequency			= m_Tracer.Frequency;
			defaultScale 		= tracer.DefualtScale;
			defaultScaleFactor	= tracer.DefualtScaleFactor;
			scaleRate			= tracer.ScaleRate;
			
			center 				= m_Tracer.Position;
			
			material = Instantiate(SpotMat) as Material;
		}
		
		Vector3 v1, v2, v3, v4;
		private float _growingScale = 0.0f;
		public void Update ()
		{
			if (m_Running)
			{
				if (duration <  maxDuration)
				{
					// duration
					duration += Time.deltaTime;
					
					// scale
					_growingScale += scaleRate;
					//					scale = minScale +  Mathf.Min Mathf.Sqrt(duration) * scaleGrowRate;
					scale = minScale + Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
					
				}
				else// Destroy Self
				{
					Destroyed = true;
				}
				
				// speed
				material.SetFloat("_Speed", speed);
				material.SetFloat("_TimeFactor", duration + Mathf.Pow(timeOffsetFactor + 1, 0.4f) - 1);
				material.SetFloat("_Strength", strength);
				material.SetFloat("_Frequency", frequency);
				
				Vector2 dir2 = new Vector2(1, 0);
				Vector2 orth_dir = new Vector3(0, 1);
				Vector2 center2 = new Vector2(center.x, center.z);
				
				v1 = center2 - (dir2 + orth_dir) * scale * 0.5f;
				v2 = center2 + (dir2 - orth_dir) * scale * 0.5f;
				
				v3 = center2 + (dir2 + orth_dir) * scale * 0.5f;
				v4 = center2 - (dir2 - orth_dir) * scale * 0.5f;
			}
			
		}

		public void DestroySelf ()
		{
			Material.Destroy(material);
		}
		
		public void Draw ()
		{
			GL.PushMatrix();
			int pcnt = material.passCount;
			for (int i = 0; i < pcnt; i ++)
			{
				material.SetPass(i);
				
				GL.Begin(GL.QUADS);
				GL.Color(Color.white);
				
				
				
				GL.TexCoord2(0, 0);
				GL.Vertex(new Vector3( v1.x, waterHeight, v1.y));
				GL.TexCoord2(1, 0);
				GL.Vertex(new Vector3(v2.x, waterHeight, v2.y));
				GL.TexCoord2(1, 1);
				GL.Vertex(new Vector3(v3.x, waterHeight, v3.y));
				GL.TexCoord2(0, 1);
				GL.Vertex(new Vector3(v4.x, waterHeight, v4.y));
				GL.End();
			}
			GL.PopMatrix();
		}
	}

	List<Drawer> m_Waves = new List<Drawer>();

}
