using UnityEngine;
using System.Collections;

public class PEGrassPokeWaveTracer : PokeWaveTracer 
{
	/// <summary>
	/// detect the Width height.
	/// </summary>
	public float Width = 1;
	
	/// <summary>
	/// detect the Length height.
	/// </summary>
	public float Length = 1;

	/// <summary>
	/// detect the water height.
	/// </summary>
	public float Height = 2;

	private bool m_GenWave = false;

	public float MaxScale = 1;

	public float TweeFactor = 0.5f;

	protected override void Init ()
	{
		base.Init ();

		if (PEWaveSystem.Self != null)
			PEWaveSystem.Self.GrassWaveRenderer.Add(this);

		_targetScale = MaxScale;
	}
	
	//float _curScale = 0;
	float _targetScale = 0;
	float _curTime = 0;

	public override void CustomUpdate ()
	{
		Vector3 pos = TracerTrans.position;
		// SafeCheck
		if (pos.x < -99999 || pos.x > 99999)
			return;
		if (pos.z < -99999 || pos.z > 99999)
			return;

		float min_x = pos.x - Length * 0.5f;
		//float min_y = pos.y;
		float min_z = pos.z - Width * 0.5f;
		float max_x = pos.x + Length * 0.5f;
		//float max_y = pos.y + Height;
		float max_z = pos.z + Width * 0.5f;
		WaterHeight = pos.y;

		bool in_terrain = false;
		for (float x = min_x; x <= max_x + 0.5f; x += 1)
		{
			for (float z = min_z; z <= max_z + 0.5f; z += 1)
			{
				in_terrain = (PETools.PE.PointInTerrain(new Vector3(x, pos.y, z) )> 0.52f);
				if (in_terrain)
					break;
			}	
		}

		bool prev_gen = m_GenWave;
		if (!in_terrain)
		{
			Ray ray = new Ray(new Vector3(pos.x, pos.y, pos.z), Vector3.down);
			Vector3 outPos = Vector3.zero;
			//float water_entry_height = 0;
			if (PETools.PE.RaycastVoxel(ray, out outPos, Mathf.CeilToInt( Height), 1, 1,  true))
			{
				m_GenWave = true;
			}
			else
				m_GenWave = false;
		}
		else
			m_GenWave = true;


		if (prev_gen != m_GenWave)
		{
			if (prev_gen)
			{
				_targetScale = 0;
				_curTime = Time.time;
			}
			else
			{
				_targetScale = MaxScale;
				_curTime = Time.time;
			}

		}

		scale = Mathf.Lerp(scale, _targetScale, Mathf.Clamp01(Mathf.Pow( (Time.time - _curTime) , 2f ) * TweeFactor) );
		base.CustomUpdate ();
	}

	public override void Draw ()
	{
		base.Draw();
	}

}
