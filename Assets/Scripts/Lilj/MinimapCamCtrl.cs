using UnityEngine;
using System.Collections;

public class MinimapCamCtrl : MonoBehaviour
{
	public Material mWater;
	public Material mRiver;
	public Light    mDirectionalLight;


	void OnPreRender ()
	{
		if(mWater)
			mWater.shader.maximumLOD = 101;
		if(mRiver)
			mRiver.shader.maximumLOD = 101;
	}

	void OnPreCull ()
	{
		if (mDirectionalLight)
		{
			mDirectionalLight.enabled = true;
			// [Edit by zx]
//			if (NVWeatherSys.Instance != null)
//				NVWeatherSys.Instance.Sun.m_Light.enabled = false;
		}
	}
	
	void OnPostRender()
	{	
		if(mWater)
			mWater.shader.maximumLOD = 501;
		if(mRiver)
			mRiver.shader.maximumLOD = 501;
		if (mDirectionalLight)
		{
			mDirectionalLight.enabled = false;
			// [Edit by zx]
//			if (NVWeatherSys.Instance != null)
//				NVWeatherSys.Instance.Sun.m_Light.enabled = true;;
		}
	}
}
