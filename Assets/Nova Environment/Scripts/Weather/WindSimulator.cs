using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public class WindSimulator : MonoBehaviour
	{
		[HideInInspector] public Executor Executor;
		public float FreqX = 3.31f;
		public float FreqY = 1.53f;
		public float FreqZ = 2.67f;
		public Vector3 WindDirection;
		//Vector3 _lastWindDirection;

		// Update is called once per frame
		void Update ()
		{
			WindDirection.x = (float)(System.Math.Sin(Executor.LocalDay * FreqX + 0.78));
			WindDirection.y = (float)(System.Math.Sin(Executor.LocalDay * FreqY + 3.14));
			WindDirection.z = (float)(System.Math.Sin(Executor.LocalDay * FreqZ + 1.57));

			WindDirection *= (Mathf.Pow(Executor.WetCoef, 3.0f) * 2.0f + 1.0f);

#if false
			if (Application.isEditor && Application.isPlaying && _lastWindDirection != Vector3.zero)
			{
				Color c = new Color(WindDirection.x * 0.5f + 0.5f, WindDirection.y * 0.5f + 0.5f, WindDirection.z * 0.5f + 0.5f);
				Debug.DrawLine(_lastWindDirection, WindDirection, c, 10);
			}
			_lastWindDirection = WindDirection;
#endif
		}
	}
}
