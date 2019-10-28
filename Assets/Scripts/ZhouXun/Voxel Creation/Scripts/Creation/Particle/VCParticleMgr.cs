using UnityEngine;
using System.Collections;

// Creation buff manager
public class VCParticleMgr : MonoBehaviour
{
	private static VCParticleMgr s_Instance = null;
	public static VCParticleMgr Instance { get { return s_Instance; } }
	
	// Particle Objects
	public GameObject Smoke_3cm = null;
	public GameObject Smoke_10cm = null;
	public GameObject Fire_3cm = null;
	public GameObject Fire_10cm = null;
	public GameObject Explode_5x3m = null;
	public GameObject Explode_16x8m = null;
	public GameObject WreckageSpurt_3cm = null;
	public GameObject WreckageSpurt_10cm = null;
	
	void Awake ()
	{
		s_Instance = this;
	}
	
	public static GameObject GetEffect ( string effect, VCESceneSetting scene_setting )
	{
		if ( Instance == null ) return null;
		if ( scene_setting == null ) return null;
		int vsize_cm = Mathf.RoundToInt(scene_setting.m_VoxelSize*100.0f);
		int ssize_x_m = Mathf.RoundToInt(scene_setting.EditorWorldSize.x);
		int ssize_z_m = Mathf.RoundToInt(scene_setting.EditorWorldSize.z);
		
		// [VCCase] - Get creation buff effect particle by VCESceneSetting
		// voxelsize = 3cm scene
		if ( vsize_cm == 3 )
		{
			if ( effect.ToLower() == "fire" )
				return Instance.Fire_3cm;
			if ( effect.ToLower() == "smoke" )
				return Instance.Smoke_3cm;
			if ( effect.ToLower() == "wreckage spurt" )
				return Instance.WreckageSpurt_3cm;
		}
		// voxelsize = 10cm scene
		else if ( vsize_cm == 10 )
		{
			if ( effect.ToLower() == "fire" )
				return Instance.Fire_10cm;
			if ( effect.ToLower() == "smoke" )
				return Instance.Smoke_10cm;
			if ( effect.ToLower() == "wreckage spurt" )
				return Instance.WreckageSpurt_10cm;
		}
		else
		{
			if ( effect.ToLower() == "fire" )
				return Instance.Fire_3cm;
			if ( effect.ToLower() == "smoke" )
				return Instance.Smoke_3cm;
			if ( effect.ToLower() == "wreckage spurt" )
				return Instance.WreckageSpurt_3cm;
		}
		// 5m x 3m scene
		if ( ssize_x_m == 3 && ssize_z_m == 5 )
		{
			if ( effect.ToLower() == "explode" )
				return Instance.Explode_5x3m;
		}
		// 16m x 8m scene
		else if ( ssize_x_m == 8 && ssize_z_m == 16 )
		{
			if ( effect.ToLower() == "explode" )
				return Instance.Explode_16x8m;
		}
		else
		{
			if ( effect.ToLower() == "explode" )
				return Instance.Explode_5x3m;
		}
		return null;
	}
}
