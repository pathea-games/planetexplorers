using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	// The executor of the Nova Environment System

	public class Executor : MonoBehaviour
	{
		[HideInInspector] public Settings Settings;
		[HideInInspector] public Output Output;

		//
		// Internal Objects ------------------------------------------------
		//

		private Transform UniverseGroup;
		private Transform WeatherGroup;
		private Transform CloudGroup;

		[HideInInspector] public Texture2D NoiseTexture;

		private GameObject SkySphere;
		private GameObject FogSphere;
		private GameObject UnderwaterMask;
		[HideInInspector] public Material SkySphereMat;
		[HideInInspector] public Material FogSphereMat;

		[HideInInspector] public Sky Sky;
		[HideInInspector] public Sun Sun;
		[HideInInspector] public Moon[] Moons = new Moon[0];
		[HideInInspector] public CloudLayer SunCloudLayer;
		[HideInInspector] public CloudLayer RainCloudLayer;
		[HideInInspector] public Storm Storm;
		[HideInInspector] public ScreenDropsEmitter DropsEmitter;
		[HideInInspector] public Thunder Thunder;
		[HideInInspector] public WindSimulator Wind;

		public delegate void DNotify ();
		public event DNotify OnExecutorCreate;
		public event DNotify OnExecutorDestroy;

		//
		// MonoBehaviour Functions -----------------------------------------
		//

		#region MONO_FUNCTIONS
		void Awake ()
		{
			Init();
		}

		void Start ()
		{
			if (OnExecutorCreate != null)
				OnExecutorCreate();
		}

		void OnDestroy ()
		{
			if (OnExecutorDestroy != null)
				OnExecutorDestroy();
			Free();
		}

		void Update ()
		{
			UpdateEditor();
			UpdateTime();
		}

		void LateUpdate ()
		{
			UpdateTransform();
			UpdateSunAndMoons();
			UpdateCave();
			UpdateWeather();
			UpdateThemes();
		}
		#endregion

		//
		// User Inspector Editor Vars --------------------------------------
		//

		#region INSPECTOR_VARS
		public double LocalTime;
		public SunParam SunSettings = new SunParam ();
		public MoonParam[] MoonsSettings = new MoonParam[0];
		public bool DoRefreshUniverse = false;

		public BiomoTheme[] BiomoThemes = new BiomoTheme[0];
		public WeatherTheme WeatherTheme = new WeatherTheme ();
		public CaveTheme CaveTheme = new CaveTheme ();

		public int BiomoIndex = 0;
		public float WetCoef = 0;
		public float Temperature = 20;
		public float CaveCoef = 0;
		public float Underwater = 0f;
		public float WaterReflectionMasterBlend = 1f;
		#endregion


		//
		// Properties ------------------------------------------------------
		//

		public double LocalDay
		{
			get { return LocalTime/Settings.SecondsPerDay; }
		}

		public double SunTime
		{
			get
			{
				double delta_longi = Settings.LocalLongitude - Settings.LocalTimeZone * 15f;
				return (delta_longi / 360.0) * Settings.SecondsPerDay + LocalTime;
			}
		}

		public double UTC
		{
			get
			{
				return LocalTime - (Settings.LocalTimeZone / 24f) * Settings.SecondsPerDay;
			}
		}

		public double SunDay
		{
			get { return SunTime / Settings.SecondsPerDay; }
		}

		public double FracSunDay
		{
			get
			{
				double sun_day = SunTime / Settings.SecondsPerDay;
				return sun_day - System.Math.Floor(sun_day);
			}
		}

		public double SunYear //   0 = Spring    0.5 = Autumn
		{
			get { return (LocalTime - SunSettings.Phi) / SunSettings.Period; }
		}

		public double FracSunYear
		{
			get
			{
				double sun_year = (LocalTime - SunSettings.Phi) / SunSettings.Period;
				return sun_year - System.Math.Floor(sun_year);
			}
		}

		public Vector3 SunDirection
		{
			get { return Sun.Direction; }
		}

		public Vector3 Moon0Direction
		{
			get { return Moons[0].transform.forward; }
		}
		
		public Vector3 MoonDirection(int moonindex)
		{
			return Moons[moonindex].transform.forward;
		}

		public float UnderwaterDensity
		{
			get { return (float)(System.Math.Log(Underwater + 1.0, System.Math.E) + 1.0); }
		}

		//
		// Global ----------------------------------------------------------
		//

		void Init ()
		{
			Settings = GetComponent<Settings>();
			Output = GetComponent<Output>();
			Output.Executor = this;
			gameObject.layer = LayerMask.NameToLayer(Settings.EnvironmentLayer);
			CreateObjects();
		}

		void Free ()
		{
			Texture2D.Destroy(NoiseTexture);
			Material.Destroy(SkySphereMat);
			GameObject.Destroy(UnderwaterMask);
		}

		//
		// Creates ---------------------------------------------------------
		//

		void CreateObjects ()
		{
			CreateStructure();
			GenerateNoiseTexture();
			CreateUniverse();
			CreateWeather();
			CreateCameraEffects();
		}

		void CreateStructure ()
		{
			UniverseGroup = Utils.CreateGameObject(null, "Universe", transform).transform;
			WeatherGroup = Utils.CreateGameObject(null, "Weather", transform).transform;
			WeatherGroup.SetAsLastSibling();
		}

		void CreateUniverse ()
		{
			CreateSkySphere();
			CreateFogSphere();
			RefreshUniverse();
		}

		void CreateWeather ()
		{
			CreateWind();
			CreateCloud();
			CreateStorm();
			CreateThunder();
		}

		void CreateCameraEffects ()
		{
			CreateUnderwaterMask();
		}

		void CreateSkySphere ()
		{
			SkySphereMat = new Material (Settings.SkySphereShader);
			SkySphereMat.SetTexture("_StarNoiseTexture", NoiseTexture);
			SkySphereMat.SetTexture("_UniverseTexture", Settings.UniverseCloud);
			SkySphere = Utils.CreateGameObject(PrimitiveType.Sphere, "SkySphere", UniverseGroup);
			SkySphere.transform.localScale = Settings.SkySize * 0.99f * Vector3.one * 2;
			SkySphere.GetComponent<Renderer>().material = SkySphereMat;
			Sky = SkySphere.AddComponent<Sky>();
			Sky.Executor = this;
		}

		void CreateFogSphere ()
		{
			FogSphereMat = new Material (Settings.FogSphereShader);
			FogSphere = Utils.CreateGameObject(PrimitiveType.Sphere, "FogSphere", SkySphere.transform);
			FogSphere.transform.localScale = Vector3.one * 0.99f;
			FogSphere.GetComponent<Renderer>().material = FogSphereMat;
		}

		void CreateUnderwaterMask ()
		{
			UnderwaterMask = Utils.CreateGameObject(Resources.Load("Underwater/UnderwaterMask") as GameObject, "UnderwaterMask", Camera.main.transform);
			UnderwaterMask.transform.localScale = Vector3.one * 5f;
			UnderwaterMask.transform.localPosition = Vector3.forward;
		}

		void CreateWind ()
		{
			Wind = Utils.CreateGameObject<WindSimulator>(null, "Wind Simulator", WeatherGroup);
			Wind.Executor = this;
		}
		
		void CreateCloud ()
		{
			CloudGroup = Utils.CreateGameObject(null, "Cloud Layers", WeatherGroup).transform;
			SunCloudLayer = CreateCloudLayer("Sun Cloud", 0);
			RainCloudLayer = CreateCloudLayer("Rain Cloud", -10);
		}

		void CreateStorm ()
		{
			Storm = Utils.CreateGameObject(Settings.StormPrefab, "Storm", WeatherGroup).GetComponent<Storm>();
			Storm.Executor = this;
			Storm.gameObject.SetActive(false);

			DropsEmitter = Utils.CreateGameObject(Settings.DropsEmitterPrefab, "Screen Drops Emitter", WeatherGroup).GetComponent<ScreenDropsEmitter>();
			DropsEmitter.Executor = this;
		}

		void CreateThunder ()
		{
			Thunder = Utils.CreateGameObject(Settings.ThunderPrefab, "Thunder", WeatherGroup).GetComponent<Thunder>();
			Thunder.Executor = this;
			Thunder.gameObject.SetActive(true);
		}

		CloudLayer CreateCloudLayer (string name, int layerIndex)
		{
			CloudLayer cl = Utils.CreateGameObject(null, name, CloudGroup).AddComponent<CloudLayer>();
			cl.Executor = this;
			cl.LayerIndex = layerIndex;
			return cl;
		}

		void CreateSun ()
		{
			if (Sun == null)
				Sun = Utils.CreateGameObject(null, "Sun", UniverseGroup).AddComponent<Sun>();
			Sun.Executor = this;
		}

		void CreateMoons ()
		{
			Moon[] moons = new Moon[MoonsSettings.Length];
			System.Array.Copy(Moons, moons, Moons.Length < moons.Length ? Moons.Length : moons.Length);

			for (int i = moons.Length; i < Moons.Length; ++i)
				GameObject.Destroy(Moons[i].gameObject);

			for (int i = 0; i < moons.Length; ++i)
			{
				string _name = MoonsSettings[i].Name;
				if (string.IsNullOrEmpty(_name))
					_name = "Moon " + (i+1).ToString();
				if (moons[i] == null)
					moons[i] = Utils.CreateGameObject(null, _name, UniverseGroup).AddComponent<Moon>();
				moons[i].Index = i;
			}

			Moons = moons;

			foreach (Moon moon in Moons)
				moon.Executor = this;
		}

		public void RefreshUniverse ()
		{
			CreateSun();
			CreateMoons();
		}

		void GenerateNoiseTexture()
		{
			const int NoiseSize = 256;
			NoiseTexture = new Texture2D(NoiseSize, NoiseSize, TextureFormat.ARGB32, false);
			NoiseTexture.filterMode = FilterMode.Point;
			
			Color[] pixels;
			pixels = new Color[NoiseSize*NoiseSize];
			
			for(int i = 0; i < NoiseSize; i++)
			{
				for(int j = 0; j < NoiseSize; j++)
				{
					int offset = (i*NoiseSize+j);
					
					pixels[offset].r = Random.Range(0f, 1f);
					pixels[offset].g = Random.Range(0f, 1f);
					pixels[offset].b = Random.Range(0f, 1f);
					pixels[offset].a = Random.Range(0f, 1f);
				}
			}
			NoiseTexture.SetPixels(pixels);
			NoiseTexture.Apply();
		}

		//
		// Updates ---------------------------------------------------------
		//

		void UpdateEditor ()
		{
			if (Application.isEditor && Application.isPlaying)
			{
				if (DoRefreshUniverse)
				{
					DoRefreshUniverse = false;
					RefreshUniverse();
				}
			}
		}

		void UpdateTime ()
		{
			if (Settings.ManageTimeElapse)
			{
				LocalTime += Settings.TimeElapseSpeed * Time.deltaTime;
			}
		}

		void UpdateTransform ()
		{
			if (Settings.MainCamera != null)
			{
				UniverseGroup.transform.position = Settings.MainCamera.transform.position;
				WeatherGroup.transform.position = Settings.MainCamera.transform.position;
				FogSphere.transform.localPosition = Vector3.zero;
				Vector3 fogPos = FogSphere.transform.position;

				fogPos.y = Settings.SeaHeight;
				FogSphere.transform.position = fogPos;
				FogSphere.transform.localScale = Vector3.one * (Underwater > 0f ? 0.3f : 0.99f);
			}
		}

		void UpdateSunAndMoons ()
		{
			Sun.Tick();
			foreach (Moon moon in Moons)
				moon.Tick();
		}

		public void UpdateCameraEffects ()
		{
			UnderwaterMask.SetActive(Underwater > 0);
		}

#if false
		void ThemeSerialize ()
		{
			for (int i = 0; i < BiomoThemes.Length; ++i)
			{
				Theme theme = BiomoThemes[i];
				if (theme.Serialize)
				{
					theme.Serialize = false;
					Theme.SerializeToFile(theme);
				}
				if (theme.Deserialize)
				{
					theme.Deserialize = false;
					BiomoThemes[i] = Theme.DeserializeToFile(theme.GetType(), theme.Name) as BiomoTheme;
				}
			}

			for (int i = 0; i < WeatherThemes.Length; ++i)
			{
				Theme theme = WeatherThemes[i];
				if (theme.Serialize)
				{
					theme.Serialize = false;
					Theme.SerializeToFile(theme);
				}
				if (theme.Deserialize)
				{
					theme.Deserialize = false;
					WeatherThemes[i] = Theme.DeserializeToFile(theme.GetType(), theme.Name) as WeatherTheme;
				}
			}
		}
#endif

		void UpdateThemes ()
		{
			if (BiomoThemes.Length <= 0)
				return;

			BiomoIndex = BiomoIndex % BiomoThemes.Length;

			Output.Normalize(0);
			for (int i = 0; i < BiomoThemes.Length; ++i)
			{
				BiomoThemes[i].Weight = Mathf.Lerp(BiomoThemes[i].Weight, BiomoIndex == i ? 1f : 0f, BiomoThemes[BiomoIndex].FadeInSpeed);
				BiomoThemes[i].Execute(this);
			}

			Output.Normalize(1);
			WeatherTheme.Execute(this);
			CaveTheme.Execute(this);

			Output.Apply();
		}

		void UpdateWeather ()
		{
			WeatherTheme.Weight = WetCoef;
			if (WetCoef > 0.5f)
			{
				Storm.gameObject.SetActive(true);
				Storm.Strength = WetCoef * 2f - 1f;
				Vector3 camforward = Vector3.forward;
				Vector3 up = Vector3.up;
				if (Settings.MainCamera != null)
				{
					Storm.transform.position = Settings.MainCamera.transform.position;
					camforward = Settings.MainCamera.transform.forward;
					camforward.y = 0;
					camforward.Normalize();
				}
				Vector3 wind = Wind.WindDirection;
				wind.y = 0;
				up -= wind * Storm.WindTiltCoef;
				up.Normalize();
				Storm.transform.rotation = Quaternion.LookRotation(camforward, up);
			}
			else
			{
				Storm.Strength = 0;
				if (Storm.RainDropsAudio.volume < 0.01f)
					Storm.gameObject.SetActive(false);
			}
			if (Settings.MainCamera != null)
			{
				DropsEmitter.transform.position = Settings.MainCamera.transform.position + Settings.MainCamera.transform.forward * 1.5f;
				DropsEmitter.transform.rotation = Settings.MainCamera.transform.rotation;
			}
		}

		void UpdateCave ()
		{
			CaveTheme.Weight = CaveCoef;
		}
	}
}
