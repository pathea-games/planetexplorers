using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

namespace NovaEnv
{
	public class Output : MonoBehaviour
	{
		[HideInInspector] public Executor Executor;

		public float WeightTotal;

		public Color SkyColor;
		
		public Color FogBrightColor;
		public Color FogDarkColor;
		public float FogSkyInterpolator;
		public float FogHeight;
		public float FogIntensity;
		public float FogStartDistance;
		public float FogEndDistance;
		
		public Color SunGlowColor;
		public float SunGlowPower;
		
		public Color SunBodyColor;
		public float SunBodySize;
		
		public Color SunLightColor;
		public float SunLightIntensity;
		public float SunlightIntensityBase = -1.0f;

		public float SkyOvercast;
		
		public Color CloudBrightColor1;
		public Color CloudBrightColor2;
		public Color CloudDarkColor1;
		public Color CloudDarkColor2;
		public float RainCloudDensity;
		public float RainCloudThreshold;
		public float RainCloudOvercast;
		
		public Color WaterDepthColor;
		public Color WaterReflectionColor;
		public Color WaterFresnelColor;
		public Color WaterSpecularColor;
		public Color WaterFoamColor;
		public float WaterDepthDensity;
		public float WaterReflectionBlend;
		
		public float ShadowStrength;
		public Color AmbientSkyColor;
		public Color AmbientEquatorColor;
		public Color AmbientGroundColor;
		public float AmbientIntensity;

		public Color SunShaftColor;
		public float SunShaftIntensity;
		public float BloomThreshold;
		public float BloomIntensity;

		public void Normalize (float multiplier = 1)
		{
			if (WeightTotal <= 0.0001f)
				multiplier = 0;
			else
				multiplier /= WeightTotal;

			WeightTotal = WeightTotal * multiplier;

			SkyColor = SkyColor * multiplier;

			FogBrightColor = FogBrightColor * multiplier;
			FogDarkColor = FogDarkColor * multiplier;
			FogSkyInterpolator = FogSkyInterpolator * multiplier;
			FogHeight = FogHeight * multiplier;
			FogIntensity = FogIntensity * multiplier;
			FogStartDistance = FogStartDistance * multiplier;
			FogEndDistance = FogEndDistance * multiplier;

			SunGlowColor = SunGlowColor * multiplier;
			SunGlowPower = SunGlowPower * multiplier;

			SunBodyColor = SunBodyColor * multiplier;
			SunBodySize = SunBodySize * multiplier;

			SunLightColor = SunLightColor * multiplier;
			SunLightIntensity = SunLightIntensity * multiplier;

			CloudBrightColor1 = CloudBrightColor1 * multiplier;
			CloudBrightColor2 = CloudBrightColor2 * multiplier;
			CloudDarkColor1 = CloudDarkColor1 * multiplier;
			CloudDarkColor2 = CloudDarkColor2 * multiplier;

			WaterDepthColor = WaterDepthColor * multiplier;
			WaterReflectionColor = WaterReflectionColor * multiplier;
			WaterFresnelColor = WaterFresnelColor * multiplier;
			WaterSpecularColor = WaterSpecularColor * multiplier;
			WaterFoamColor = WaterFoamColor * multiplier;
			WaterDepthDensity = WaterDepthDensity * multiplier;
			WaterReflectionBlend = WaterReflectionBlend * multiplier;

			ShadowStrength = ShadowStrength * multiplier;
			AmbientSkyColor = AmbientSkyColor * multiplier;
			AmbientEquatorColor = AmbientEquatorColor * multiplier;
			AmbientGroundColor = AmbientGroundColor * multiplier;
			AmbientIntensity = AmbientIntensity * multiplier;

			SunShaftColor = SunShaftColor * multiplier;
			SunShaftIntensity = SunShaftIntensity * multiplier;
			BloomThreshold = BloomThreshold * multiplier;
			BloomIntensity = BloomIntensity * multiplier;
		}

		public void Apply ()
		{
			Executor.Sky.SkyColor = SkyColor;
			Executor.Sky.FogColorA = FogBrightColor;
			Executor.Sky.FogColorB = FogDarkColor;
			Executor.Sky.FogHeight = FogHeight;
			RenderSettings.fogMode = FogMode.Linear;
			RenderSettings.fogEndDistance = Mathf.Min(FogEndDistance, Executor.Settings.MaxFogEndDistance);
			float u = RenderSettings.fogEndDistance / FogEndDistance;
			RenderSettings.fogStartDistance = FogStartDistance * u;

			if (Executor.Underwater <= 0)
			{
				if (Executor.Settings.MainCamera != null)
				{
					Vector3 pos = Executor.Settings.MainCamera.transform.forward;
					pos.y = 0;
					RenderSettings.fogColor = Executor.Sky.FogColorAtPoint(pos, Executor.SunDirection, FogSkyInterpolator) * FogIntensity;
				}
				else
				{
					RenderSettings.fogColor = (FogDarkColor * 0.7f + FogBrightColor * 0.3f) * FogIntensity;
				}
			}
			else
			{
				RenderSettings.fogColor = WaterDepthColor;
			}
			Executor.Sky.UnityFogColor = RenderSettings.fogColor;
			Executor.Sky.SunBloomColor = SunGlowColor;
			Executor.Sky.SunPower = SunGlowPower;
			Executor.Sky.SunColor = SunBodyColor;
			Executor.Sky.SunSize = SunBodySize;
			Executor.Sky.Overcast = SkyOvercast;
			Executor.Sky.Apply();
			Executor.Sun.SunLight.color = SunLightColor;
			Executor.Sun.SunLight.intensity = SunLightIntensity < SunlightIntensityBase ? SunlightIntensityBase : SunLightIntensity;
			Executor.SunCloudLayer.Color1 = CloudBrightColor1;
			Executor.SunCloudLayer.Color2 = CloudBrightColor2;
			Executor.SunCloudLayer.Color3 = CloudDarkColor1;
			Executor.SunCloudLayer.Color4 = CloudDarkColor2;
			Executor.RainCloudLayer.Color1 = CloudBrightColor1;
			Executor.RainCloudLayer.Color2 = CloudBrightColor2;
			Executor.RainCloudLayer.Color3 = CloudDarkColor1;
			Executor.RainCloudLayer.Color4 = CloudDarkColor2;
			Executor.RainCloudLayer.LayerMat.SetFloat("_CloudDensity", RainCloudDensity);
			Executor.SunCloudLayer.LayerMat.SetFloat("_CloudThreshold", Mathf.Max(RainCloudThreshold, 1.0f));
			Executor.RainCloudLayer.LayerMat.SetFloat("_CloudThreshold", RainCloudThreshold);
			Executor.SunCloudLayer.LayerMat.SetFloat("_Overcast", RainCloudOvercast * 0.7f + 0.3f);
			Executor.RainCloudLayer.LayerMat.SetFloat("_Overcast", RainCloudOvercast);
			Executor.RainCloudLayer.LayerMat.SetFloat("_CloudTile", 7.5f);
			Material watermat = Executor.Settings.WaterMaterial;
			if (watermat != null)
			{
				if (!string.IsNullOrEmpty(Executor.Settings.WaterDepthColorProp))
					watermat.SetColor(Executor.Settings.WaterDepthColorProp, WaterDepthColor);

				if (!string.IsNullOrEmpty(Executor.Settings.WaterReflectionColorProp))
					watermat.SetColor(Executor.Settings.WaterReflectionColorProp, WaterReflectionColor);

				if (!string.IsNullOrEmpty(Executor.Settings.WaterFresnelColorProp))
					watermat.SetColor(Executor.Settings.WaterFresnelColorProp, WaterFresnelColor);

				if (!string.IsNullOrEmpty(Executor.Settings.WaterSpecularColorProp))
					watermat.SetColor(Executor.Settings.WaterSpecularColorProp, WaterSpecularColor);

				if (!string.IsNullOrEmpty(Executor.Settings.WaterFoamColorProp))
					watermat.SetColor(Executor.Settings.WaterFoamColorProp, WaterFoamColor);

				if (!string.IsNullOrEmpty(Executor.Settings.WaterDepthDensityProp))
					watermat.SetVector(Executor.Settings.WaterDepthDensityProp, new Vector4(WaterDepthDensity, 0.1f, Mathf.Max(0.5f, Executor.UnderwaterDensity - 1), 0f));

				if (!string.IsNullOrEmpty(Executor.Settings.WaterReflectionBlendProp))
					watermat.SetFloat(Executor.Settings.WaterReflectionBlendProp, Mathf.Lerp(1f, WaterReflectionBlend, Executor.WaterReflectionMasterBlend));

				if (!string.IsNullOrEmpty(Executor.Settings.WaterSunLightDirProp))
					watermat.SetVector(Executor.Settings.WaterSunLightDirProp, -Executor.SunDirection);
			}
			Executor.Sun.SunLight.shadowStrength = ShadowStrength;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
			RenderSettings.ambientSkyColor = AmbientSkyColor;
			RenderSettings.ambientEquatorColor = AmbientEquatorColor;
			RenderSettings.ambientGroundColor = AmbientGroundColor;
			RenderSettings.ambientIntensity = AmbientIntensity;

			if (Executor.Settings.MainCamera != null)
			{
				SunShafts ss = Executor.Settings.MainCamera.GetComponent<SunShafts>();
				if (ss != null)
				{
					ss.sunTransform = Executor.Sun.SunLight.transform;
					ss.sunColor = SunShaftColor;
					ss.sunShaftIntensity = SunShaftIntensity;
				}

				Bloom bloom = Executor.Settings.MainCamera.GetComponent<Bloom>();
				if (bloom != null)
				{
					bloom.bloomIntensity = BloomIntensity;
					bloom.bloomThreshold = BloomThreshold;
				}

				BlurOptimized blur = Executor.Settings.MainCamera.GetComponent<BlurOptimized>();
				GlobalFog gf = Executor.Settings.MainCamera.GetComponent<GlobalFog>();

				if (blur != null)
				{
					blur.enabled = Executor.Underwater > 0;
				}
				if (gf != null)
				{
					gf.enabled = Executor.Underwater > 0;
				}
			}
			Executor.UpdateCameraEffects();
		}
	}
}
