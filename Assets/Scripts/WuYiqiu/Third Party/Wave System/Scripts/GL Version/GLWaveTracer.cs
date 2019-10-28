using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class glWaveTracer : MonoBehaviour
{
	public WaveRenderer WaveRenderer = null;

	public float WaterHeight = 95.5f;

	public Transform TracerTrans;

	public Vector3 Position 
	{ 
		get { 
			if (TracerTrans != null)
				return TracerTrans.position;
			return transform.position;
		} 
	}

	private static Material s_SpotMat;
	public static Material SpotMat 
	{ 
		get 
		{
			if (s_SpotMat == null)
			{
				s_SpotMat = Resources.Load("Materials/SpotWaveMat") as Material;
			}
			return s_SpotMat;
		} 
	}

	private static Material s_LineMat;
	public static Material LineMat
	{
		get
		{
			if (s_LineMat == null)
			{
				s_LineMat = Resources.Load("Materials/LineWaveMat") as Material;
			}
			return s_LineMat;
		}
	}

	private WaveRenderer m_WaveRenderer = null;
	public void InitUpdate()
	{
		if (m_WaveRenderer != WaveRenderer)
		{
			if (m_WaveRenderer != null)
				m_WaveRenderer.Remove(this);

			m_WaveRenderer = WaveRenderer;
			if (m_WaveRenderer != null)
				m_WaveRenderer.Add(this);
		}
	}

	protected virtual void Init ()
	{

	}

	// when the Wave Renderer  is set it will invoke
	public abstract void CustomUpdate();

	public abstract void Draw();

	protected void Awake ()
	{
		WaveRenderer.s_Tracers.Add(this);
		Init();
	}

	protected void OnDestroy ()
	{
		WaveRenderer.s_Tracers.Remove(this);
		if (WaveRenderer != null)
			WaveRenderer.Remove(this);
	}


}

