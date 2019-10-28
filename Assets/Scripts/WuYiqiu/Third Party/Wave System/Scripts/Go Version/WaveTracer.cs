using UnityEngine;
using System.Collections;
using System;


public class WaveTracer : MonoBehaviour
{
	/// <summary>
	/// The wave scene which can manage this tracer
	/// </summary>
	public WaveScene  Scene = null;

	/// <summary>
	/// The type of the wave.
	/// </summary>
	public EWaveType WaveType = EWaveType.Line;

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

	[Serializable]
	public class WaterAttr
	{
		/// <summary>
		/// The slope of water plane
		/// </summary>
		public float Slope = 1;

		/// <summary>
		/// The height of water,
		/// </summary>
		public float Height = 95.5f;
	}

	public WaterAttr WaterAttribute = new WaterAttr();

	#region Line_Wave_Param

	[Serializable]
	public class LineWaveParam
	{
		/// <summary>
		/// Time for point A to point
		/// </summary>
		public float DeltaTime = 0.5f;

	}

	public LineWaveParam LineWave = new LineWaveParam();

	#endregion

	#region Spot_Wave_Param

	[Serializable]
	public class SpotWaveParam
	{
		public float minScale = 1;

		public float speedFactorRandom = 0.1f;
	}

	public SpotWaveParam SpotWave = new SpotWaveParam();

	#endregion

	#region WAVE_SECEN_USE

	// Dont change this value if you do not know what them mean.

	[NonSerialized] public float curIntervalTime;
	[NonSerialized] public LineWaveHandler lastWave;
	[NonSerialized] public Vector3 prevPos = Vector3.zero;
	[NonSerialized] public bool canGen = false;
	#endregion
	

	public Vector3 Position { get { return transform.position;} }

	void Awake()
	{
		Scene = WaveScene.Self;
		if (Scene != null)
			Scene.AddTracer(this);
	}

	void Start ()
	{
		prevPos = Position;
	}
}

