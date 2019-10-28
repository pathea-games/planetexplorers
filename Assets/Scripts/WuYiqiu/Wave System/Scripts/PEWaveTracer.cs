#define GL_VERSION

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class PEWaveTracer : MonoBehaviour 
{
#if false
	public string Desc;

	public Transform TracerTrans;

	public int QuadCnt = 0;

	/// <summary>
	/// Wave parameter.
	/// </summary>
	[Serializable]
	public class EWaveParam
	{
		/// <summary>
		/// The type of the wave.
		/// </summary>
		public EWaveType  WaveType = EWaveType.Line;
		
		/// <summary>
		/// The wave speed. recommended value(0.2 ~ 1);
		/// </summary>
		public float WaveSpeed = 0.5f;
		
		/// <summary>
		/// The  wave life time.
		/// </summary>
		public float WaveDuration = 5f;
		
		/// <summary>
		/// The wave strength.
		/// </summary>
		public float Strength = 20;
		
		/// <summary>
		/// The wave frequency.
		/// </summary>
		public float Frequency = 40;
		
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
		
		#region Line_Wave_Param
		
		[Serializable]
		public class LineWaveParam
		{
			/// <summary>
			/// Time for point A to point
			/// </summary>
			public float DeltaTime = 0.5f;
			
		}
		
		public LineWaveParam LineWave;
		
		#endregion
		
		#region Spot_Wave_Param
		
		[Serializable]
		public class SpotWaveParam
		{
			
			
			public float minScale = 1;
			
			public float speedFactorRandom = 0.1f;
			
			public float scaleGrowRate = 1;
		}
		
		public SpotWaveParam SpotWave;
		
		#endregion
	}

	public EWaveParam Wave;


	public float Height;

	public float Width;

	public float Length;

#if GL_VERSION
	private GLWaveTracer 	m_Tracer;
#else
	private WaveTracer 		m_Tracer;
#endif
	

	void Awake ()
	{
#if GL_VERSION
		m_Tracer = TracerTrans.gameObject.AddComponent<GLWaveTracer>();
		if (PEWaveSystem.Self != null)
			PEWaveSystem.Self.WaveRenderer.Add(m_Tracer);
#else
		m_Tracer = TracerTrans.gameObject.AddComponent<WaveTracer>();
#endif
	}
	
	void Update ()
	{
		if (m_Tracer == null)
			return;

		Vector3 pos = TracerTrans.position;
		float min_x = pos.x - Length * 0.5f;
		float min_y = pos.y;
		float min_z = pos.z - Width * 0.5f;
		float max_x = pos.x + Length * 0.5f;
		float max_y = pos.y + Height;
		float max_z = pos.z + Width * 0.5f;

		float character_height = max_y - min_y;
		float y = pos.y + character_height;

		bool in_water = false;
		for (float x = min_x; x <= max_x + 0.5f; x += 1)
		{
			for (float z = min_z; z <= max_z + 0.5f; z += 1)
			{
				in_water = (PETools.PE.PointInWater(new Vector3(x, y, z) )> 0.52f);
				if (in_water)
					break;
			}	
		}
		
		float water_height = 0;
		bool gen_wave = false;
		float extra_strength = 0;
		if (!in_water)
		{
			Ray ray = new Ray(new Vector3(pos.x, pos.y + character_height, pos.z), Vector3.down);
			Vector3 outPos = Vector3.zero;
			float water_entry_height = 0;
			if (PETools.PE.RaycastVoxel(ray, out outPos, Mathf.CeilToInt( character_height), 1, 2,  true))
			{
				water_height =  (int)outPos.y + 1;
				water_entry_height = Mathf.Abs(outPos.y - pos.y);
			}
			
			extra_strength = (water_entry_height/character_height) * 20;
			
			gen_wave = true;
		}

#if GL_VERSION
		// Update parameter
		m_Tracer.WaveType 				= Wave.WaveType;
		m_Tracer.AutoGenWave 			= gen_wave;
		m_Tracer.WaveSpeed  			= Wave.WaveSpeed;
		m_Tracer.WaveDuration 			= Wave.WaveDuration;
		m_Tracer.Strength				= Wave.Strength + extra_strength;
		m_Tracer.Frequency				= Wave.Frequency;
		m_Tracer.TimeOffsetFactor		= Wave.TimeOffsetFactor;
		m_Tracer.IntervalTime			= Wave.IntervalTime;
		m_Tracer.ScaleRate				= Wave.ScaleRate;
		m_Tracer.DefualtScale			= Wave.DefualtScale;
		m_Tracer.DefualtScaleFactor 	= Wave.DefualtScaleFactor;
		m_Tracer.WaterAttribute.Height  = water_height;
		m_Tracer.LineWave.DeltaTime 	= Wave.LineWave.DeltaTime;
		m_Tracer.SpotWave.minScale		= Wave.SpotWave.minScale;
		m_Tracer.SpotWave.speedFactorRandom = Wave.SpotWave.speedFactorRandom;
		m_Tracer.enabled = gen_wave;

		QuadCnt = m_Tracer.WaveCnt;
#else

		// Update parameter
		m_Tracer.WaveType 				= Wave.WaveType;
		m_Tracer.AutoGenWave 			= gen_wave;
		m_Tracer.WaveSpeed  			= Wave.WaveSpeed;
		m_Tracer.WaveDuration 			= Wave.WaveDuration;
		m_Tracer.Strength				= Wave.Strength + extra_strength;
		m_Tracer.Frequency				= Wave.Frequency;
		m_Tracer.TimeOffsetFactor		= Wave.TimeOffsetFactor;
		m_Tracer.IntervalTime			= Wave.IntervalTime;
		m_Tracer.ScaleRate				= Wave.ScaleRate;
		m_Tracer.DefualtScale			= Wave.DefualtScale;
		m_Tracer.DefualtScaleFactor 	= Wave.DefualtScaleFactor;
		m_Tracer.WaterAttribute.Height  = water_height;
		m_Tracer.LineWave.DeltaTime 	= Wave.LineWave.DeltaTime;
		m_Tracer.SpotWave.minScale		= Wave.SpotWave.minScale;
		m_Tracer.SpotWave.scaleGrowRate = Wave.SpotWave.scaleGrowRate;
		m_Tracer.SpotWave.speedFactorRandom =Wave.SpotWave.speedFactorRandom;
		m_Tracer.enabled = gen_wave;
#endif
	}
#endif
}
