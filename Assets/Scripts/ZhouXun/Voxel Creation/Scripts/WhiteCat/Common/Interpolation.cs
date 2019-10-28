using UnityEngine;

namespace WhiteCat
{
	/// <summary>
	/// 插值方法
	/// </summary>
	public struct Interpolation
	{
#region 基本插值

		/// <summary> 线性插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float Linear(float t)
		{
			return t;
		}


		/// <summary> 方波插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float Square(float t)
		{
			return t < 0.5f ? 0f : 1f;
		}


		/// <summary> 随机插值 </summary>
		public static float Random(float t)
		{
			return UnityEngine.Random.Range(0f, 1f);
		}

#endregion


#region 缓入缓出插值

		/// <summary> 缓入插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float EaseIn(float t)
		{
			return t * t;
		}


		/// <summary> 缓出插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float EaseOut(float t)
		{
			return t * (2f - t);
		}


		/// <summary> 缓入缓出插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float EaseInEaseOut(float t)
		{
			return (3f - t - t) * t * t;
		}

#endregion


#region 强缓入缓出插值

		/// <summary> 强缓入插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float EaseInStrong(float t)
		{
			return t * t * t;
		}


		/// <summary> 强缓出插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float EaseOutStrong(float t)
		{
			t = 1f - t;
			return 1f - t * t * t;
		}


		/// <summary> 强缓入缓出插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float EaseInEaseOutStrong(float t)
		{
			float k = t * t;
			return (6f * k - 15f * t + 10f) * k * t;
		}

#endregion


#region 回入回出插值

		/// <summary> 回入缓出插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float BackInEaseOut(float t)
		{
			float k = t * t;
			return (-7f * k + 12f * t - 4f) * k;
		}


		/// <summary> 缓入回出插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float EaseInBackOut(float t)
		{
			float k = t * t;
			return (7f * k - 16f * t + 10f) * k;
		}


		/// <summary> 回入回出插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float BackInBackOut(float t)
		{
			float k = t * t;
			return ((24f * t - 60f) * k + 46f * t - 9f) * k;
		}

#endregion


#region 对称插值

		/// <summary> 三角插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float Triangle(float t)
		{
			return t < 0.5f ? t + t : 2f - t - t;
		}


		/// <summary> 抛物线插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float Parabolic(float t)
		{
			return 4f * t * (1f - t);
		}


		/// <summary> 钟形插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float Bell(float t)
		{
			float k = 1f - t;
			k = k * t;
			return 16f * k * k;
		}


		/// <summary> 正弦插值 </summary>
		/// <param name="t">[0, 1] 范围的值</param>
		public static float Sine(float t)
		{
			return Mathf.Sin((t + t + 1.5f) * Mathf.PI) * 0.5f + 0.5f;
		}

#endregion
	}

}