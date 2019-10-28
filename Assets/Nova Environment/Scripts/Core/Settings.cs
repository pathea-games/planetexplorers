using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	// Settings of the Nova Environment System

	public class Settings : MonoBehaviour
	{
		public string EnvironmentLayer = "Default";
		public Camera MainCamera;

		public float SoundVolume = 1;
		public float LocalLongitude = 106.3f; // W [-180, 180] E
		public float LocalLatitude = 29.3f;   // S  [-90, 90]  N
		public float LocalTimeZone = 8;       // W  [-12, 12]  E   

		public double TimeElapseSpeed = 1;    // Enable if (ManageTimeElapse == true)
		public double SecondsPerDay = 86400;
		public float SeaHeight = 0;
		public float SkySize = 3000;
		public float CloudHeight = 600;
		public float CloudArea = 1500;
		public float MaxFogEndDistance = 1024;
		public float MaxRainParticleEmissiveRate = 500;

		public Shader SkySphereShader;
		public Shader FogSphereShader;
		public Shader CloudShader;
		public Shader MoonBodyShader;

		public Material WaterMaterial;
		public string WaterDepthColorProp;
		public string WaterReflectionColorProp;
		public string WaterFresnelColorProp;
		public string WaterSpecularColorProp;
		public string WaterFoamColorProp;
		public string WaterDepthDensityProp;
		public string WaterReflectionBlendProp;
		public string WaterSunLightDirProp;

		public GameObject CloudLayerModel;
		public GameObject StormPrefab;
		public GameObject DropsEmitterPrefab;
		public GameObject ThunderPrefab;

		public Texture2D UniverseCloud;

		public bool ManageTimeElapse = true;
		public bool MoonLightEnable = false;

		public LayerMask LightCullingMask = -1;
	}
}
