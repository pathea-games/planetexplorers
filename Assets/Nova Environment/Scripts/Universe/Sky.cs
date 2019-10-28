using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public class Sky : MonoBehaviour
	{
		[HideInInspector] public Executor Executor;

		public Color SkyColor;
		public Color FogColorA;
		public Color FogColorB;
		public Color UnityFogColor;
		public float FogHeight = 0.7f;
		public Color SunBloomColor;
		public Color SunColor;
		public float SunSize = 0.5f;
		public float SunPower = 100;
		public float Overcast = 1;
		
		// Update is called once per frame
		public void Apply ()
		{
			Executor.SkySphereMat.SetColor("_SkyColor", SkyColor);
			Executor.SkySphereMat.SetColor("_FogColorA", FogColorA);
			Executor.SkySphereMat.SetColor("_FogColorB", FogColorB);
			Executor.FogSphereMat.SetColor("_FogColor", UnityFogColor);
			Executor.FogSphereMat.SetFloat("_FogHeight", FogHeight*0.2f);
			Executor.SkySphereMat.SetFloat("_FogHeight", FogHeight);
			Executor.SkySphereMat.SetColor("_SunBloomColor", SunBloomColor);
			Executor.SkySphereMat.SetColor("_SunColor", SunColor);
			Executor.SkySphereMat.SetFloat("_SunSize", SunSize);
			Executor.SkySphereMat.SetFloat("_SunPower", SunPower);
			Executor.SkySphereMat.SetFloat("_Overcast", Overcast);
		}

		public Color SkyColorAtPoint (Vector3 point, Vector3 sunpos)
		{
			Vector3 sunvec = sunpos.normalized;
			Vector3 skyvec = point.normalized;
			
			float dot_power = Vector3.Dot(sunvec, skyvec);
			dot_power = dot_power > 0 ? Mathf.Pow(dot_power, SunPower) : 0;
			dot_power *= 0.4f;
			float dot_power2 = Vector3.Dot(sunvec, skyvec) + 0.7f;
			Color FogColor = Color.Lerp(FogColorB, FogColorA, Mathf.Clamp01(dot_power2 * 0.7f));
			dot_power2 *= 0.3f;
			float height = Mathf.Clamp01(skyvec.y);
			float fog_density = Mathf.Pow((1 - Mathf.Clamp01(height/FogHeight)), 1.5f) * FogColor.a;
			Color retcolor = Color.Lerp(SkyColor, FogColor, Mathf.Clamp01(fog_density)) + SunBloomColor * dot_power + SunBloomColor * dot_power2;
			retcolor.a = SkyColor.a;
			
			return retcolor;
		}

		public Color FogColorAtPoint (Vector3 point, Vector3 sunpos, float interpolator)
		{
			Vector3 sunvec = sunpos.normalized;
			Vector3 skyvec = point.normalized;
			
			float dot_power = Vector3.Dot(sunvec, skyvec);
			dot_power = dot_power > 0 ? Mathf.Pow(dot_power, SunPower) : 0;
			dot_power *= 0.4f;
			float dot_power2 = Vector3.Dot(sunvec, skyvec) + 0.7f;
			Color FogColor = Color.Lerp(FogColorB, FogColorA, Mathf.Clamp01(dot_power2 * 0.7f));
			dot_power2 *= 0.3f;
			float height = Mathf.Clamp01(skyvec.y);
			float fog_density = Mathf.Pow((1 - Mathf.Clamp01(height/FogHeight)), 1.5f) * FogColor.a;
			Color retcolor = Color.Lerp(SkyColor, FogColor, Mathf.Clamp01(fog_density)) + SunBloomColor * dot_power + SunBloomColor * dot_power2;
			retcolor.a = SkyColor.a;
			
			return Color.Lerp(FogColor, retcolor, interpolator);
		}
	}
}
