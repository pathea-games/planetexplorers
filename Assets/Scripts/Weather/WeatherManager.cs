//using UnityEngine;
//using System;
//using System.Collections;
//
//public class WeatherManager : MonoBehaviour 
//{
//	static WeatherManager mInstance;
//	public static WeatherManager Instance { get	{return mInstance;	}	}
//	const int TimeOfDay = 93600;
//	
//	public float RainTimeMin = 3600;
//	public float RainTimeMax = 7200;
//	
//	public float SunTimeMin = 5000;
//	public float SunTimeMax = 15000;
//	
//	public float MaxRainEmission = 1000;
//	
//	public GameObject mRain;
//	
//	public GameObject mRainMist;
//	
//	private NVWeatherSys mNVWeatherSys;
//	
//	static double	mWeatherChangeTime;
//	
//	static UniSkyWeather mNextWeather;
//	
//	[SerializeField] UniSkyWeather 	mCurrentWeather;
//	
//	public UniSkyWeather CurrentWeather{get{return mCurrentWeather;}}
//	
//	
//	public float mRainCoefBase = 0;
//	public float mRainCoefBias = 0;
//	public float mRainCoefficient;
//
//    float clearChance = WeatherConfig.ClearChance;
//    float partlyCloudyChance = WeatherConfig.PartlyCloudyChance;
//    float mostlyCloudChance = WeatherConfig.MostlyCloudChance;
//    float sprinkleRainChance = WeatherConfig.SprinkleRainChance;
//    float TorrentialRainChance;
//
//
//	// Use this for initialization
//	void Start () 
//	{
//		mInstance = this;
//		mNVWeatherSys = transform.root.GetComponentInChildren<NVWeatherSys>();
//		mNextWeather = UniSkyWeather.USW_PartlyCloudy;
//		if(null != GameGui_N.Instance)
//			mWeatherChangeTime = GameTime.Timer.Second + RainTimeMin;
//        InitClimateParam();
//        //StartCoroutine(Test());
//	}
//
//    //for test
//    IEnumerator Test()
//    {
//        while (true)
//        {
//            LogManager.Error(clearChance);
//            yield return new WaitForSeconds(5.0f);
//        }
//    }
//
//    public static void Init(Transform trans)
//    {
//        GameObject go = Resources.Load("Prefab/Weather/WindZone") as GameObject;
//        GameObject wzgo = UnityEngine.Object.Instantiate(go) as GameObject;
//        wzgo.name = "WindZone";
//        go = Resources.Load("Prefab/Weather/SkyControl") as GameObject;
//        UnityEngine.Object.Instantiate(go);
//        go = Resources.Load("Prefab/Weather/NovaWeatherSystem") as GameObject;
//        go = UnityEngine.Object.Instantiate(go) as GameObject;
//
//        if (go == null)
//            return;
//        NVWeatherSys nvws = go.GetComponent<NVWeatherSys>();
//        if (nvws == null)
//            return;
//        nvws.PlayerTrans = trans;
//        nvws.LookCamera = Camera.main;
//    }
//
//	// Update is called once per frame
//	void Update () 
//	{
//		if(GameTime.Timer.Second > mWeatherChangeTime&& !GameConfig.IsMultiMode)
//		{
//			switch(mNextWeather)
//			{
//			case UniSkyWeather.USW_Random:
//				int randValue = UnityEngine.Random.Range(0,100);
//                if (randValue < clearChance)
//					mCurrentWeather = UniSkyWeather.USW_Clear;
//                else if (randValue < clearChance + partlyCloudyChance)
//					mCurrentWeather = UniSkyWeather.USW_PartlyCloudy;
//                else if (randValue < clearChance + partlyCloudyChance + mostlyCloudChance)
//					mCurrentWeather = UniSkyWeather.USW_MostlyCloud;
//                else if (randValue < clearChance + partlyCloudyChance + mostlyCloudChance + sprinkleRainChance)
//					mCurrentWeather = UniSkyWeather.USW_SprinkleRain;
//				else
//					mCurrentWeather = UniSkyWeather.USW_TorrentialRain;
//				break;
//			default:
//				mCurrentWeather = mNextWeather;
//				break;
//			}
//
//            mNextWeather = UniSkyWeather.USW_Random;
//            switch (mCurrentWeather)
//            {
//                case UniSkyWeather.USW_SprinkleRain:
//                    mWeatherChangeTime = GameTime.Timer.Second + UnityEngine.Random.Range(RainTimeMin, RainTimeMax);
//                    ExampleScript.Instance.ChangeWeather(UniSkyWeather.USW_SprinkleRain);
//                    break;
//                case UniSkyWeather.USW_TorrentialRain:
//                    mWeatherChangeTime = GameTime.Timer.Second + UnityEngine.Random.Range(RainTimeMin, RainTimeMax);
//                    ExampleScript.Instance.ChangeWeather(UniSkyWeather.USW_TorrentialRain);
//                    break;
//                case UniSkyWeather.USW_PartlyCloudy:
//                    mWeatherChangeTime = GameTime.Timer.Second + UnityEngine.Random.Range(RainTimeMin, RainTimeMax);
//                    ExampleScript.Instance.ChangeWeather(UniSkyWeather.USW_PartlyCloudy);
//                    break;
//                case UniSkyWeather.USW_MostlyCloud:
//                    mWeatherChangeTime = GameTime.Timer.Second + UnityEngine.Random.Range(RainTimeMin, RainTimeMax);
//                    ExampleScript.Instance.ChangeWeather(UniSkyWeather.USW_MostlyCloud);
//                    break;
//                default:
//                    mWeatherChangeTime = GameTime.Timer.Second + UnityEngine.Random.Range(SunTimeMin, SunTimeMax);
//                    ExampleScript.Instance.ChangeWeather(UniSkyWeather.USW_Clear);
//                    break;
//            }
//            
//		}
//		
//		switch(mCurrentWeather)
//		{
//		case UniSkyWeather.USW_Clear:
//			mRainCoefBase = 0.1f;
//			break;
//		case UniSkyWeather.USW_PartlyCloudy:
//			mRainCoefBase = 0.3f;
//			break;
//		case UniSkyWeather.USW_MostlyCloud:
//			mRainCoefBase = 0.5f;
//			break;
//		case UniSkyWeather.USW_SprinkleRain:
//			mRainCoefBase = 0.7f;
//			break;
//		case UniSkyWeather.USW_TorrentialRain:
//			mRainCoefBase = 0.9f;
//			break;
//		default:
//			break;
//		}
//		
//		// random change mRainCoefBias
//		float rand_ubb = 1f - mRainCoefBias;
//		float rand_lbb = -1f - mRainCoefBias;
//		float increasement = UnityEngine.Random.Range(rand_lbb, rand_ubb);
//		mRainCoefBias += increasement * 0.008f;
//		
//		// Lerp to rain coef wanted
//		mRainCoefficient = Mathf.Lerp( mRainCoefficient, mRainCoefBase + mRainCoefBias, Time.deltaTime * 0.1f );
//		//if ( Mathf.Abs(mRainCoefficient - (mRainCoefBase + mRainCoefBias)) < 0.01f )
//		//	mRainCoefficient = mRainCoefBase + mRainCoefBias;
//		
//		float emission_coef = Mathf.Clamp01( (mRainCoefficient - 0.6f) / 0.3f );
//		
//		if(mNVWeatherSys.CaveCoefficient < 0.5f)
//		{
//			mRain.GetComponent<ParticleEmitter>().minEmission = emission_coef * MaxRainEmission * 0.5f;
//			mRain.GetComponent<ParticleEmitter>().maxEmission = emission_coef * MaxRainEmission;
//		}
//		else
//		{
//			mRain.GetComponent<ParticleEmitter>().minEmission = 0;
//			mRain.GetComponent<ParticleEmitter>().maxEmission = 0;
//		}
//			
//		if ( emission_coef < 0.01f )
//			mRain.SetActive(false);
//		else
//			mRain.SetActive(true);
//		
//		if (mRainCoefficient > 0.73f && NVWeatherSys.Instance.CaveCoefficient < 0.5f && mNVWeatherSys.CaveCoefficient < 0.5f)
//  			mRainMist.GetComponent<Renderer>().enabled = true;
//		else
//  			mRainMist.GetComponent<Renderer>().enabled = false;
//
////		float sunT = (float)(GameTime.Timer.Second%17342219.0/17342219.0);
////		sunT = Mathf.Asin(Mathf.Sin(7f/360f * 2 * Mathf.PI) * Mathf.Cos(sunT * 2 * Mathf.PI))/(2 * Mathf.PI) * 360; // 赤角
////		mNVWeatherSys.SunUpperCulminationAngle = 53 + sunT;
////		mNVWeatherSys.SunLowerCulminationAngle = -127 - sunT;
////		
////		float moonT = (float)(GameTime.Timer.Second%1917234.0/1917234.0);
////		mNVWeatherSys.MoonPhase = Mathf.Lerp(0f,1f,moonT); // 月相
////		
////		moonT = (float)(GameTime.Timer.Second%1726378.0/1726378.0);
////		moonT = Mathf.Asin(Mathf.Sin(5f/360f * 2 * Mathf.PI) * Mathf.Cos(moonT * 2 * Mathf.PI))/(2 * Mathf.PI) * 360; // 赤角
////		mNVWeatherSys.MoonUpperCulminationAngle = 53f + moonT;
////		mNVWeatherSys.MoonLowerCulminationAngle = -127f - moonT;
//	}
//
//	public static void NextRainCoef (ref float rain_coef, ref float curr_weather_time)
//	{
//		int weather = Mathf.FloorToInt(rain_coef * 5);
//		rain_coef += UnityEngine.Random.Range(-0.01f - curr_weather_time*0.001f,0.01f + curr_weather_time*0.001f);
//		rain_coef = Mathf.Clamp(rain_coef, 0f, 0.9999f);
//		int _weather = Mathf.FloorToInt(rain_coef * 5);
//		if ( weather == _weather )
//			curr_weather_time += 0.02f;
//		else
//			curr_weather_time = 0;
//	}
//	
//	public static void ChangeWeather(UniSkyWeather wType, float time = 0)
//	{
//		mNextWeather = wType;
//		mWeatherChangeTime = GameTime.Timer.Second + time;
//	}
//
//
//    /// <summary>
//    /// for multiMode
//    /// </summary>
//    public void SetWeather(UniSkyWeather weather)
//    {
//        mCurrentWeather = weather;
//        switch (weather)
//        {
//            case UniSkyWeather.USW_SprinkleRain:
//
//            case UniSkyWeather.USW_TorrentialRain:
//
//            case UniSkyWeather.USW_PartlyCloudy:
//
//            case UniSkyWeather.USW_MostlyCloud:
//                ExampleScript.Instance.ChangeWeather(weather);
//                break;
//            default:
//                ExampleScript.Instance.ChangeWeather(UniSkyWeather.USW_Clear);
//                break;
//        }
//    }
//
//    public void InitClimateParam()
//    {
//        WeatherConfig.SetClimateType(RandomMapConfig.ScenceClimate);
//        clearChance = WeatherConfig.ClearChance;
//        partlyCloudyChance = WeatherConfig.PartlyCloudyChance;
//        mostlyCloudChance = WeatherConfig.MostlyCloudChance;
//        sprinkleRainChance = WeatherConfig.SprinkleRainChance;
//	}
//
//	#region Action Callback APIs
//	public static void RPC_S2C_WeatherChange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
//	{
//		int _currentWeather = stream.Read<int>();
//
//		if (Instance != null)
//			Instance.SetWeather((UniSkyWeather)_currentWeather);
//	}
//	#endregion
//}
