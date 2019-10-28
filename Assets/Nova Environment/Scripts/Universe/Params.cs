using UnityEngine;
using System;

namespace NovaEnv
{
	[Serializable]
	public class SunParam
	{
		public double Period = 31556926.0;
		public double Phi = 6795300.0;
		public float Obliquity = 23.43f;
	}

	[Serializable]
	public class MoonParam
	{
		public string Name;
		public Texture2D MainTex;
		public Texture2D BumpTex;
		public Rect MoonTexRect;
		public float Size;
		public float LightIntensity;
		public Color LightColor;
		public Color TintColor;
		public double Period = 2360591.0;
		public double Phi = 487370.0;
		public float Obliquity = 5.145f;
	}
}
