using UnityEngine;
using System.Collections;

public class CloudController : MonoBehaviour
{
	public const int WeatherCloud = 0;
	
	public const int TerrainCloud = 1;
	
	public Color mBasColor = Color.white;
	
	public int CloudType = WeatherCloud;
	
	public Light mSun;
	
	public void InitCloud(Light sun, Cloud3D cloud)
	{
		mSun = sun;
		CloudType = cloud.mCloudType;
		mBasColor = cloud.mBaseColor;
		transform.localPosition = cloud.mPosition;
	}
	
	void Update ()
	{
		if(mSun == null)
			return;
		switch(CloudType)
		{
		case WeatherCloud:
			GetComponent<Renderer>().material.SetColor("_TintColor",mSun.color*0.8f);
			break;
		case TerrainCloud:
			GetComponent<Renderer>().material.SetColor("_TintColor",mSun.color * 0.4f + mBasColor * 0.6f);
			break;
		}
	}
}
