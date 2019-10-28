using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineWaveTracer : glWaveTracer 
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

	/// <summary>
	/// Delta time. 
	/// </summary>
	public float DeltaTime = 0.5f;
	
	public float ScaleRate = 0.01f;
	
	public float DefualtScale = 0;
	
	public float DefualtScaleFactor = 2f;

	#region Parent_Override_Func
	
	float curIntervalTime;
	Drawer lastWave;
	Vector3 prevPos = Vector3.zero;
	bool canGen = false;

	public float Distance = 256;

	public Vector3 Pos {
		get
		{
			if (TracerTrans != null)
				return TracerTrans.position;

			return transform.position;
		}
	}

	public bool CheckValid ()
	{
		if (Vector3.SqrMagnitude(Pos - WaveRenderer.transform.position) > Distance * Distance)
			return false;

		return true;
	}

	public override void CustomUpdate ()
	{
		if (!CheckValid())
			return;

		if (AutoGenWave)
		{
			if (!canGen)
			{
				if (Vector3.Magnitude( prevPos - Position) > 0.05f)
				{
					canGen = true;
				}
			}
			else
			{
				prevPos = Position;
				
			}
			
			if (curIntervalTime >= IntervalTime)
			{
				if (lastWave != null)
				{
					lastWave.LockTracer();
					lastWave = null;
				}
				
				if (canGen)
				{
					lastWave = new Drawer();
					lastWave.Init(this);
					m_Waves.Add(lastWave);
					
					curIntervalTime = 0;
					
					canGen = false;
				}
			}
			else
			{
				curIntervalTime += Time.deltaTime;
				
			}
		}
		else
		{
			if (lastWave != null)
			{
				lastWave.LockTracer();
				lastWave = null;
			}
			
			curIntervalTime = IntervalTime + 0.001f;
			canGen = false;
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
		if (!CheckValid())
			return;

		foreach (Drawer wave in m_Waves)
		{
			wave.Draw();
		}
	}
	
	#endregion

	/// <summary>
	/// Line Drawer 
	/// </summary>
	class Drawer
	{
		public bool Destroyed = false;

		private Vector3 m_BeginPos;
		private LineWaveTracer m_Tracer;
		
		private Vector3 m_EndPos;
		private float m_CurTime;
		private bool m_Running = false;
		
		private Material material;
		private Transform trans;
		private float duration = 0.0f;

		bool locked = false;
		
		float waterHeight;
		float strength;
		float frequency;
		float defaultScale;
		float defaultScaleFactor;
		float scaleRate;
		float maxDuration;
		float deltaTime;

		// orientation
		Vector3 dircetion = Vector3.right;
		float scale = 2;
		Vector3 center = Vector3.zero;

		public  void Init (LineWaveTracer tracer)
		{
			m_Tracer = tracer;
			m_BeginPos = tracer.Position;

			m_Running = true;
			waterHeight = tracer.WaterHeight;
			strength 	= tracer.Strength;
			frequency	= tracer.Frequency;
			defaultScale 		= tracer.DefualtScale;
			defaultScaleFactor	= tracer.DefualtScaleFactor;
			scaleRate			= tracer.ScaleRate;
			maxDuration			= tracer.WaveDuration;
			deltaTime			= tracer.DeltaTime;
			
			material = Instantiate(LineMat) as Material;
		}
		
		public void LockTracer()
		{
			locked = true;
		}
		
		private float _growingScale = 0.0f;
		Vector2 v1, v2, v3, v4;
		public  void Update ()
		{
			if (m_Running)
			{
				if (duration < maxDuration)
				{
					duration += Time.deltaTime;
					
					if (!locked)
						m_EndPos = m_Tracer.Position;
					
					m_BeginPos.y = waterHeight;
					m_EndPos.y = waterHeight;
					
					Vector3 v = m_EndPos - m_BeginPos; 
					float mag =  new Vector2(v.x, v.z).magnitude;
					
					if (mag < 0.02f)
						dircetion = Vector3.right;
					else
						dircetion = v.normalized;
					
					float relDis =  mag / 2;
					
					_growingScale += scaleRate;
					float max_scale = 50;
					float sqrt_dura =  Mathf.Min(Mathf.Sqrt(duration) * defaultScaleFactor, defaultScale);
					
					if (relDis > 0.5f)
					{
						relDis = 0.5f;
						
						// position
						center = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + dircetion*0.1f;
						// scale
						scale = Mathf.Min(sqrt_dura + 2 * mag + _growingScale, max_scale);
						
					}
					else
					{
						scale = Mathf.Min( sqrt_dura + 2 + _growingScale, max_scale);
						center = Vector3.Lerp(m_BeginPos, m_EndPos, 0.5f) + dircetion * (0.5f - relDis) * scale * 0.5f;
					}
					
					material.SetFloat("_Distance", relDis * 2);
					
					material.SetFloat("_Strength", strength);
					material.SetFloat("_Frequency", frequency);
					material.SetFloat("_TimeFactor", duration + Mathf.Pow(m_Tracer.TimeOffsetFactor + 1, 0.4f) - 1) ;
					material.SetFloat("_DeltaTime", deltaTime);
					material.SetFloat("_Speed", m_Tracer.WaveSpeed);
					
					
					Vector2 dir2 = new Vector2(dircetion.x, dircetion.z);
					Vector2 orth_dir = new Vector3(dir2.y, -dir2.x);
					Vector2 center2 = new Vector2(center.x, center.z);
					
					v1 = center2 - (dir2 + orth_dir) * scale * 0.5f;
					v2 = center2 + (dir2 - orth_dir) * scale * 0.5f;
					
					v3 = center2 + (dir2 + orth_dir) * scale * 0.5f;
					v4 = center2 - (dir2 - orth_dir) * scale * 0.5f;
					
					if (Application.isEditor)
					{
						Vector3 _v1 = new Vector3(v1.x, waterHeight, v1.y);
						Vector3 _v2 = new Vector3(v2.x, waterHeight, v2.y);
						Vector3 _v3 = new Vector3(v3.x, waterHeight, v3.y);
						Vector3 _v4 = new Vector3(v4.x, waterHeight, v4.y);
						Debug.DrawLine(_v1, _v2, Color.yellow);
						Debug.DrawLine(_v2, _v3, Color.yellow);
						Debug.DrawLine(_v3, _v4, Color.yellow);
						Debug.DrawLine(_v4, _v1, Color.yellow);
					}
				}
				else
				{
					Destroyed = true;
				}
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
