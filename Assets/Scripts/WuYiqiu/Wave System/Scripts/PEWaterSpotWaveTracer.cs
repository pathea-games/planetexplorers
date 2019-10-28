using UnityEngine;
using System.Collections;

public class PEWaterSpotWaveTracer : SpotWaveTracer
{
	public string Desc;
	
	/// <summary>
	/// detect the water height.
	/// </summary>
	public float Height;
	
	/// <summary>
	/// detect the Width height.
	/// </summary>
	public float Width;
	
	/// <summary>
	/// detect the Length height.
	/// </summary>
	public float Length;

	/// <summary>
	/// The custom Update is Valid
	/// </summary>
	public bool IsValid = true;

	protected override void Init ()
	{
		base.Init ();
		
		if (PEWaveSystem.Self != null)
			PEWaveSystem.Self.WaveRenderer.Add(this);
	}

	public override void CustomUpdate ()
	{
		if (!IsValid)
			return;

		Vector3 pos = TracerTrans.position;
		// SafeCheck
		if (pos.x < -99999 || pos.x > 99999)
			return;
		if (pos.z < -99999 || pos.z > 99999)
			return;

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
		float final_strength = Strength;
		AutoGenWave = gen_wave;
		WaterHeight = water_height;
		Strength = final_strength + extra_strength;
		
		base.CustomUpdate ();
		
		Strength = final_strength;
	}

	public override void Draw ()
	{
		if (!IsValid)
			return;

		base.Draw ();
	}
}
