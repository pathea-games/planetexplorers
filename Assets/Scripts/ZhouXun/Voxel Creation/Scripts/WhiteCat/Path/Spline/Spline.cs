using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 样条
	/// </summary>
	[Serializable]
	public class Spline
	{
		const float minError = 0.001f;			// 最小误差
		const float maxError = 1000f;			// 最大误差
		const int minSegments = 8;				// 最小分段数
		const int maxSegments = 80000;			// 最大分段数
		const float segmentsFactor = 0.1f;		// 分段数 = 估算长度 / 误差 * 此系数


		[SerializeField] Vector3 _c0;			// t^0 系数
		[SerializeField] Vector3 _c1;			// t^1 系数
		[SerializeField] Vector3 _c2;			// t^2 系数
		[SerializeField] Vector3 _c3;			// t^3 系数

		[SerializeField] float _error = 0.01f;	// 长度误差
		[SerializeField] Vector2[] _samples;	// 参数 t 对应的长度采样表


		/// <summary>
		/// 设置基数样条参数
		/// </summary>
		public void SetCardinalParameters(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float tension = 0.5f)
		{
			_c0 = p1;
			_c1 = (p2 - p0) * tension;
			_c2 = (p2 - p1) * 3 - (p3 - p1) * tension - (_c1 + _c1);
			_c3 = (p3 - p1) * tension - (p2 - p1) * 2 + _c1;

			_samples = null;
		}


		/// <summary>
		/// 设置贝塞尔样条参数
		/// </summary>
		public void SetBezierParameters(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			_c0 = p0;
			_c1 = 3 * (p1 - p0);
			_c2 = 3 * (p2 - 2 * p1 + p0);
			_c3 = p3 + 3 * (p1 - p2) - p0;

			_samples = null;
		}


		/// <summary>
		/// 根据参数 t 获取点坐标
		/// </summary>
		public Vector3 GetPoint(float t)
		{
			return _c0 + t * _c1 + t * t * _c2 + t * t * t * _c3;
		}


		/// <summary>
		/// 根据参数 t 获取导数
		/// </summary>
		public Vector3 GetDerivative(float t)
		{
			return _c1 + 2 * t * _c2 + 3 * t * t * _c3;
		}


		/// <summary>
		/// 根据参数 t 获取二阶导数
		/// </summary>
		public Vector3 GetSecondDerivative(float t)
		{
			return _c2 + _c2 + 6 * t * _c3;
		}


		/// <summary>
		/// 长度误差
		/// </summary>
		public float error
		{
			get { return _error; }
			set
			{
				_error = Mathf.Clamp(value, minError, maxError);
				_samples = null;
			}
		}


		/// <summary>
		/// 采样数据是否无效
		/// </summary>
		public bool isSamplesInvalid
		{
			get { return Utility.IsNullOrEmpty(_samples); }
		}


		/// <summary>
		/// 清楚采样数据
		/// </summary>
		public void ClearSamples()
		{
			_samples = null;
		}


		/// <summary>
		/// 采样总数
		/// </summary>
		public int samplesCount
		{
			get
			{
				if (Utility.IsNullOrEmpty(_samples)) CalculateLength();
				return _samples.Length;
			}
		}


		/// <summary>
		/// 获取采样点数据
		/// </summary>
		public Vector2 GetSample(int index)
		{
			if (Utility.IsNullOrEmpty(_samples)) CalculateLength();
			return _samples[Mathf.Clamp(index, 0, _samples.Length-1)];
		}


		/// <summary>
		/// 总长度
		/// </summary>
		public float totalLength
		{
			get
			{
				if (Utility.IsNullOrEmpty(_samples)) CalculateLength();
				return _samples[_samples.Length - 1].y;
			}
		}


		/// <summary>
		/// 计算长度
		/// </summary>
		public void CalculateLength()
		{
			if (Utility.IsNullOrEmpty(_samples))
			{
				Vector3 lastPoint = _c0, currentPoint;
				Vector2 currentSample = Vector2.zero, baseSample = Vector2.zero, newSample;
				float currentMaxSlope, maxSlope = float.MaxValue;
				float currentMinSlope, minSlope = float.MinValue;

				// 估算长度、分段数
				for (int i = 1; i <= minSegments; i++)
				{
					currentPoint = GetPoint(i / (float)minSegments);
					currentSample.y += (currentPoint - lastPoint).magnitude;
					lastPoint = currentPoint;
				}
				int segments = Mathf.Clamp((int)(currentSample.y * segmentsFactor / _error) + 1, minSegments, maxSegments);

				List<Vector2> samples = new List<Vector2>((int)(segments * 0.1f) + 1);
				samples.Add(baseSample);
				lastPoint = _c0;
				currentSample = Vector2.zero;

				for (int i = 1; i <= segments; i++)
				{
					// 计算长度
					currentPoint = GetPoint(currentSample.x = i / (float)segments);
					currentSample.y += (currentPoint - lastPoint).magnitude;
					lastPoint = currentPoint;

					// 计算斜率范围
					currentMaxSlope = (currentSample.y + _error - baseSample.y) / (currentSample.x - baseSample.x);
					currentMinSlope = (currentSample.y - _error - baseSample.y) / (currentSample.x - baseSample.x);

					// 斜率范围无交集，需要添加记录采样
					if (currentMaxSlope < minSlope || currentMinSlope > maxSlope)
					{
						// 添加上一个位置，取斜率范围平均值
						newSample.x = (i - 1) / (float)segments;
						newSample.y = (newSample.x - baseSample.x) * (minSlope + maxSlope) * 0.5f + baseSample.y;
						samples.Add(baseSample = newSample);

						// 重置斜率范围
						maxSlope = (currentSample.y + _error - baseSample.y) / (currentSample.x - baseSample.x);
						minSlope = (currentSample.y - _error - baseSample.y) / (currentSample.x - baseSample.x);
					}
					else
					{
						// 计算斜率范围交集
						if (currentMaxSlope < maxSlope) maxSlope = currentMaxSlope;
						if (currentMinSlope > minSlope) minSlope = currentMinSlope;
					}
				}

				// 添加最后一个采样点
				samples.Add(currentSample);
				_samples = samples.ToArray();
			}
		}


		/// <summary>
		/// 根据参数 t 获取长度
		/// </summary>
		public float GetLength(float t)
		{
			if (Utility.IsNullOrEmpty(_samples)) CalculateLength();

			if (t >= 1f) return _samples[_samples.Length - 1].y;
			if (t <= 0f) return 0f;

			int index = (int)(t * _samples.Length);
			Vector2 baseSample;

			if (_samples[index].x > t)
			{
				while (_samples[--index].x > t) ;
				baseSample = _samples[index++];
			}
			else
			{
				while (_samples[++index].x < t) ;
				baseSample = _samples[index - 1];
			}

			return baseSample.y + (t - baseSample.x) * (_samples[index].y - baseSample.y) / (_samples[index].x - baseSample.x);
		}


		/// <summary>
		/// 通过长度获取位置（返回值为参数 t）
		/// </summary>
		public float GetPositionAtLength(float s)
		{
			if (Utility.IsNullOrEmpty(_samples)) CalculateLength();

			if (s >= _samples[_samples.Length - 1].y) return 1f;
			if (s <= 0f) return 0f;

			int index = (int)(s / _samples[_samples.Length - 1].y * _samples.Length);
			Vector2 baseSample;

			if (_samples[index].y > s)
			{
				while (_samples[--index].y > s) ;
				baseSample = _samples[index++];
			}
			else
			{
				while (_samples[++index].y < s) ;
				baseSample = _samples[index - 1];
			}

			return baseSample.x + (s - baseSample.y) * (_samples[index].x - baseSample.x) / (_samples[index].y - baseSample.y);
		}


		/// <summary>
		/// 求曲线上与给定点最近的点
		/// </summary>
		/// <param name="given">给定点</param>
		/// <param name="segmentLength">分段长度，用来估算分段数量</param>
		/// <param name="minSegments">限制的最少分段数量</param>
		/// <param name="maxSegments">限制的最大分段数量</param>
		/// <returns>样条的 time 参数</returns>
		public float GetClosestPosition(Vector3 given, float segmentLength, int minSegments = 8, int maxSegments = 64)
		{
			int segments = Mathf.Clamp((int)(totalLength / segmentLength), minSegments, maxSegments);

			float bestTime = 0, time = 0;
			float bestSqrMagnitude = (given - _c0).sqrMagnitude, sqrMagnitude;

			for (int i = 1; i <= segments; i++)
			{
				sqrMagnitude = (given - GetPoint(time = (float)i / segments)).sqrMagnitude;

				if (sqrMagnitude < bestSqrMagnitude)
				{
					bestTime = time;
					bestSqrMagnitude = sqrMagnitude;
				}
			}

			Vector3 vector = given - GetPoint(bestTime);
			Vector3 tangent = GetDerivative(bestTime);

			if(Vector3.Dot(vector, tangent) > 0)
			{
				return GetPositionAtLength(GetLength(bestTime) + Vector3.Project(vector, tangent).magnitude);
			}
			else
			{
				return GetPositionAtLength(GetLength(bestTime) - Vector3.Project(vector, tangent).magnitude);
			}
		}




#if UNITY_EDITOR

		public void Draw(Color color, float width)
		{
			UnityEditor.Handles.DrawBezier(
				_c0,
				_c0 + _c1 + _c2 + _c3,
				_c0 + _c1 / 3f,
				_c0 + (_c2 + _c1 + _c1) / 3f,
				color, null, width);
		}

#endif
	}
}