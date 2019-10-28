using UnityEngine;
using System;

namespace NovaEnv
{
	[Serializable]
	public class BiomoTheme : Theme
	{
		public float FadeInSpeed = 0.004f;

		[Header("Sky & Fog")]
		public Gradient SkyColorChange;
		
		public Gradient FogBrightColorChange;
		public Gradient FogDarkColorChange;
		public AnimationCurve FogSkyInterpolatorChange;
		public AnimationCurve FogHeightChange;
		public AnimationCurve FogIntensityChange;
		public float FogStartDistance;
		public float FogEndDistance;
		
		[Header("Sun")]
		public Gradient SunGlowColorChange;
		public AnimationCurve SunGlowPowerChange;
		
		public Gradient SunBodyColorChange;
		public AnimationCurve SunBodySizeChange;
		
		public Gradient SunLightColorChange;
		public AnimationCurve SunLightIntensityChange;
		
		[Header("Cloud")]
		public Gradient CloudBrightColor1Change;
		public Gradient CloudBrightColor2Change;
		public Gradient CloudDarkColor1Change;
		public Gradient CloudDarkColor2Change;
		
		[Header("Water")]
		public Gradient WaterDepthColorChange;
		public Gradient WaterReflectionColorChange;
		public Gradient WaterFresnelColorChange;
		public Gradient WaterSpecularColorChange;
		public Gradient WaterFoamColorChange;
		public AnimationCurve WaterDepthDensityChange;
		public AnimationCurve WaterReflectionBlendChange;
		
		[Header("Ambient & Shadow")]
		public AnimationCurve ShadowStrengthChange;
		public Gradient AmbientSkyColorChange;
		public Gradient AmbientEquatorColorChange;
		public Gradient AmbientGroundColorChange;
		public AnimationCurve AmbientIntensityChange;

		[Header("Post-Effect")]
		public Gradient SunShaftColorChange;
		public AnimationCurve SunShaftIntensityChange;
		public AnimationCurve BloomThresholdChange;
		public AnimationCurve BloomIntensityChange;

		[Header("Underwater")]
		public Gradient UnderwaterWaterDepthColorChange;
		public float UnderwaterDensity;

		public override void Execute (Executor executor)
		{
			if (Weight < 0.001f)
				return;

			Output output = executor.Output;
			
			float day_time = (float)(executor.FracSunDay);
			float sun_height = (Mathf.Asin(executor.SunDirection.y) / Mathf.PI + .5F) * .5F;
			float ap = .5F;
			const float gap = .1F;
			if ( 0F <= day_time && day_time <= gap )
				ap = Mathf.Lerp( .5F, 0F, day_time/gap );
			else if ( gap <= day_time && day_time <= .5F - gap )
				ap = 0F;
			else if ( .5F - gap <= day_time && day_time <= .5F + gap )
				ap = (day_time - .5F)/gap * .5F + .5F;
			else if ( .5F + gap <= day_time && day_time <= 1F - gap )
				ap = 1F;
			else if ( 1F - gap <= day_time && day_time <= 1F )
				ap = Mathf.Lerp( .5F, 1F, (1F - day_time)/gap );
			else
				ap = .5F;

			output.WeightTotal += Weight;

			output.SkyColor += Evaluate(SkyColorChange, sun_height, ap) * Weight;
			output.FogBrightColor += Evaluate(FogBrightColorChange, sun_height, ap) * Weight;
			output.FogDarkColor += Evaluate(FogDarkColorChange, sun_height, ap) * Weight;
			output.FogSkyInterpolator += Evaluate(FogSkyInterpolatorChange, sun_height, ap) * Weight;

			float waterdenscoef = Mathf.Min(1, 2f / (executor.UnderwaterDensity));
			if (executor.Underwater <= 0f)
			{
				output.FogHeight += Evaluate(FogHeightChange, sun_height, ap) * Weight;
				output.FogIntensity += Evaluate(FogIntensityChange, sun_height, ap) * Weight;
				output.FogStartDistance += FogStartDistance * Weight;
				output.FogEndDistance += FogEndDistance * Weight;
			}
			else
			{
				output.FogHeight += 1f * Weight;
				output.FogIntensity += Evaluate(FogIntensityChange, sun_height, ap) * Weight;
				output.FogStartDistance += FogStartDistance * Weight;
				output.FogEndDistance += (FogEndDistance / Mathf.Max(1, UnderwaterDensity)) * Weight;
			}
			
			output.SunGlowColor += Evaluate(SunGlowColorChange, sun_height, ap) * Weight;
			output.SunGlowPower += Evaluate(SunGlowPowerChange, sun_height, ap) * Weight;
			output.SunBodyColor += Evaluate(SunBodyColorChange, sun_height, ap) * Weight;
			output.SunBodySize += Evaluate(SunBodySizeChange, sun_height, ap) * Weight;
			output.SunLightColor += Evaluate(SunLightColorChange, sun_height, ap) * Weight;
			output.SunLightIntensity += Evaluate(SunLightIntensityChange, sun_height, ap) * Weight;
			
			output.CloudBrightColor1 += Evaluate(CloudBrightColor1Change, sun_height, ap) * Weight;
			output.CloudBrightColor2 += Evaluate(CloudBrightColor2Change, sun_height, ap) * Weight;
			output.CloudDarkColor1 += Evaluate(CloudDarkColor1Change, sun_height, ap) * Weight;
			output.CloudDarkColor2 += Evaluate(CloudDarkColor2Change, sun_height, ap) * Weight;


			if (executor.Underwater <= 0f)
			{
				output.WaterDepthColor += Evaluate(WaterDepthColorChange, sun_height, ap) * Weight;
			}
			else
			{
				output.WaterDepthColor += Evaluate(UnderwaterWaterDepthColorChange, sun_height, ap) * waterdenscoef * Weight;
			}
			output.WaterReflectionColor += Evaluate(WaterReflectionColorChange, sun_height, ap) * Weight;
			output.WaterFresnelColor += Evaluate(WaterFresnelColorChange, sun_height, ap) * Weight;
			output.WaterSpecularColor += Evaluate(WaterSpecularColorChange, sun_height, ap) * Weight;
			output.WaterFoamColor += Evaluate(WaterFoamColorChange, sun_height, ap) * Weight;
			output.WaterDepthDensity += Evaluate(WaterDepthDensityChange, sun_height, ap) * Weight;
			output.WaterReflectionBlend += Evaluate(WaterReflectionBlendChange, sun_height, ap) * Weight;
			
			output.ShadowStrength += Evaluate(ShadowStrengthChange, sun_height, ap) * Weight;
			output.AmbientSkyColor += Evaluate(AmbientSkyColorChange, sun_height, ap) * Weight;
			output.AmbientEquatorColor += Evaluate(AmbientEquatorColorChange, sun_height, ap) * Weight;
			output.AmbientGroundColor += Evaluate(AmbientGroundColorChange, sun_height, ap) * Weight;
			output.AmbientIntensity += Evaluate(AmbientIntensityChange, sun_height, ap) * waterdenscoef * Weight;

			output.SunShaftColor += Evaluate(SunShaftColorChange, sun_height, ap) * Weight;
			output.SunShaftIntensity += Evaluate(SunShaftIntensityChange, sun_height, ap) * Weight;
			output.BloomThreshold += Evaluate(BloomThresholdChange, sun_height, ap) * Weight;
			output.BloomIntensity += Evaluate(BloomIntensityChange, sun_height, ap) * Weight;
		}
	}
}