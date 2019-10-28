using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


//////////////////////////////////////////////////////////////////////////
//by PuJi
//////////////////////////////////////////////////////////////////////////
public enum ClimateType : int
{
    CT_Dry = 0,
    CT_Temperate,
    CT_Wet,
    CT_Random,
}

public class WeatherConfig
{
    WeatherConfig mInstance;
    public WeatherConfig Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new WeatherConfig();
            }
            return mInstance;
        }
    }

    public static ClimateType climate=ClimateType.CT_Dry;
    public static float ClearChance = 80;
    public static float PartlyCloudyChance = 10;
    public static float MostlyCloudChance = 10;
    public static float SprinkleRainChance = 0;
    public static float TorrentialRainChance;
	public const float NO_RAIN = 0.5f;
	public const float LESS_RAIN = 1f;
	public const float FULL_RAIN = 1;
	public const float Rain_Threshold = 0.55f;
	public const float HeavyRain_Threshold = 0.7f;

	public static bool IsRaining{
		get{return PeEnv.Nova.WetCoef>Rain_Threshold;}
	}
	public static bool IsRainingHeavily{
		get{return PeEnv.Nova.WetCoef>HeavyRain_Threshold;}
	}

    public static void SetClimateType(ClimateType climate,RandomMapType mapType)
    {
        switch (climate)
        {
            case ClimateType.CT_Dry:
//                ClearChance = 80;
//                PartlyCloudyChance = 10;
//                MostlyCloudChance = 10;
//                SprinkleRainChance = 0;
				PeEnv.SetControlRain(NO_RAIN);
                break;
            case ClimateType.CT_Temperate:
//                ClearChance = 60;
//                PartlyCloudyChance = 14;
//                MostlyCloudChance = 12;
//				SprinkleRainChance = 8;
				PeEnv.SetControlRain(LESS_RAIN);
                break;
            case ClimateType.CT_Wet:
//                ClearChance = 60;
//                PartlyCloudyChance = 10;
//                MostlyCloudChance = 5;
//                SprinkleRainChance = 1;
				PeEnv.SetControlRain(FULL_RAIN);
                break;
            case ClimateType.CT_Random:
                switch ((int)Time.time % 3)
                {
                    case 0:
                        SetClimateType(ClimateType.CT_Dry,mapType);
                        break;
                    case 1:
                        SetClimateType(ClimateType.CT_Temperate,mapType);
                        break;
                    case 2:
                        SetClimateType(ClimateType.CT_Wet,mapType);
                        break;
                }
                return;
        }

		if(mapType==RandomMapType.Desert)
			PeEnv.SetControlRain(NO_RAIN);

        WeatherConfig.climate = climate;
        RandomMapConfig.ScenceClimate = WeatherConfig.climate;
    }

}

