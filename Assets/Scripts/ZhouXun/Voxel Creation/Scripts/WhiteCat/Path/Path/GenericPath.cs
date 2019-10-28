using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 泛型路径
	/// </summary>
	public abstract class GenericPath<Node, Spline> : Path where Node : PathNode where Spline : PathSpline
	{
		[SerializeField][GetSet("isCircular")] protected bool _isCircular;		// 路径是否首尾相接
		[SerializeField][GetSet("lengthError")] protected float _lengthError;	// 路径长度误差
		[SerializeField] protected List<Node> _nodes;							// 节点表
		[SerializeField] protected List<Spline> _splines;						// 样条表
		[SerializeField] protected int _invalidSplineLengthIndex;				// 第一个无效的路径长度样条下标




		// 初始化节点表和样条表
		protected abstract void InitializeNodesAndSplines();

		// 更新样条参数
		protected abstract void UpdateSplineParameters(int splineIndex);




		// 初始化
		void Awake()
		{
			if (Utility.IsNullOrEmpty(_nodes)) Reset();
			else transform.hasChanged = false;
		}


		// 重置
		void Reset()
		{
			_isCircular = false;
			_lengthError = 0.01f;
			_invalidSplineLengthIndex = 0;

			InitializeNodesAndSplines();

			transform.hasChanged = true;
		}


		/// <summary>
		/// 路径是否首尾相接
		/// </summary>
		public override bool isCircular
		{
			get { return _isCircular; }
			set
			{
				if (value != _isCircular)
				{
					_isCircular = value;
					TriggerOnChangeEvents();
				}
			}
		}


		/// <summary>
		/// 路径长度误差
		/// </summary>
		public override float lengthError
		{
			get { return _lengthError; }
			set
			{
				_lengthError = Mathf.Clamp(value, 0.001f, 1000f);
				for (int i = 0; i < _splines.Count; i++)
				{
					_splines[i].error = _lengthError;
				}
				_invalidSplineLengthIndex = 0;
				TriggerOnChangeEvents();
			}
		}


		/// <summary>
		/// 节点总数
		/// </summary>
		public override int nodesCount
		{
			get { return _nodes.Count; }
		}


		/// <summary>
		/// 长度是否有效
		/// </summary>
		public override bool isLengthValid
		{
			get { return _invalidSplineLengthIndex >= splinesCount; }
		}


		/// <summary>
		/// 路径总长度
		/// </summary>
		public override float pathTotalLength
		{
			get
			{
				int splineIndex = splinesCount - 1;
				CalculatePathLength(splineIndex);
				return _splines[splineIndex].pathLength;
			}
		}




		/// <summary>
		/// 检查是否需要重置所有样条参数，如果需要则执行重置
		/// </summary>
		public override void CheckAndResetAllSplines()
		{
			if (transform.hasChanged)
			{
				int count = _splines.Count;
				for (int i = 0; i < count; i++)
				{
					UpdateSplineParameters(i);
				}
				transform.hasChanged = false;
				TriggerOnChangeEvents();
			}
		}


		/// <summary>
		/// 计算路径长度
		/// </summary>
		public override void CalculatePathLength(int splineIndex)
		{
			CheckAndResetAllSplines();

			for (; _invalidSplineLengthIndex <= splineIndex; _invalidSplineLengthIndex++)
			{
				_splines[_invalidSplineLengthIndex].pathLength = _splines[_invalidSplineLengthIndex].totalLength
					+ (_invalidSplineLengthIndex == 0 ? 0 : _splines[_invalidSplineLengthIndex - 1].pathLength);
			}
		}


		/// <summary>
		/// 获取从路径起点到样条终点的路径长度
		/// </summary>
		public override float GetPathLength(int splineIndex)
		{
			CalculatePathLength(splineIndex);
			return _splines[splineIndex].pathLength;
		}


		/// <summary>
		/// 获取从路径起点到曲线段上某点的路径长度
		/// </summary>
		public override float GetPathLength(int splineIndex, float splineTime)
		{
			CalculatePathLength(splineIndex);
			return _splines[splineIndex].GetLength(splineTime)
				+ (splineIndex == 0 ? 0 : _splines[splineIndex - 1].pathLength);
		}


		/// <summary>
		/// 清除所有采样数据
		/// </summary>
		public override void ClearAllSamples()
		{
			for (int i = 0; i < _splines.Count; i++)
			{
				_splines[i].ClearSamples();
			}
			_invalidSplineLengthIndex = 0;
		}




		/// <summary>
		/// 获取节点本地位置
		/// </summary>
		public override Vector3 GetNodeLocalPosition(int nodeIndex)
		{
			return _nodes[nodeIndex].localPosition;
		}


		/// <summary>
		/// 获取节点世界位置
		/// </summary>
		public override Vector3 GetNodePosition(int nodeIndex)
		{
			return transform.TransformPoint(_nodes[nodeIndex].localPosition);
		}


		/// <summary>
		/// 设置节点世界位置
		/// </summary>
		public override void SetNodePosition(int nodeIndex, Vector3 position)
		{
			SetNodeLocalPosition(nodeIndex, transform.InverseTransformPoint(position));
		}


		/// <summary>
		/// 获取节点本地旋转
		/// </summary>
		public override Quaternion GetNodeLocalRotation(int nodeIndex)
		{
			return _nodes[nodeIndex].localRotation;
		}


		/// <summary>
		/// 获取节点世界旋转
		/// </summary>
		public override Quaternion GetNodeRotation(int nodeIndex)
		{
			return Quaternion.LookRotation(
				transform.TransformVector(_nodes[nodeIndex].localRotation * Vector3.forward),
				transform.TransformVector(_nodes[nodeIndex].localRotation * Vector3.up));
		}


		/// <summary>
		/// 设置节点世界旋转
		/// </summary>
		public override void SetNodeRotation(int nodeIndex, Quaternion rotation)
		{
			SetNodeLocalRotation(nodeIndex,
				Quaternion.LookRotation(
					transform.InverseTransformVector(rotation * Vector3.forward),
					transform.InverseTransformVector(rotation * Vector3.up)));
		}




		/// <summary>
		/// 获取样条上的点坐标
		/// </summary>
		public override Vector3 GetSplinePoint(int splineIndex, float splineTime)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].GetPoint(splineTime);
		}


		/// <summary>
		/// 获取样条上的导数
		/// </summary>
		public override Vector3 GetSplineDerivative(int splineIndex, float splineTime)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].GetDerivative(splineTime);
		}


		/// <summary>
		/// 获取样条上的二阶导数
		/// </summary>
		public override Vector3 GetSplineSecondDerivative(int splineIndex, float splineTime)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].GetSecondDerivative(splineTime);
		}


		/// <summary>
		/// 获取样条某点处的旋转，旋转朝向切线方向
		/// </summary>
		public override Quaternion GetSplineRotation(int splineIndex, float splineTime, Vector3 upwards, bool reverseForward = false)
		{
			CheckAndResetAllSplines();
			return Quaternion.LookRotation(
				reverseForward ? -_splines[splineIndex].GetDerivative(splineTime) : _splines[splineIndex].GetDerivative(splineTime),
				upwards);
		}


		/// <summary>
		/// 样条长度采样是否无效
		/// </summary>
		public override bool IsSplineSamplesInvalid(int splineIndex)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].isSamplesInvalid;
		}


		/// <summary>
		/// 获取样条长度采样数量
		/// </summary>
		public override int GetSplineSamplesCount(int splineIndex)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].samplesCount;
		}


		/// <summary>
		/// 获取样条的长度
		/// </summary>
		public override float GetSplineTotalLength(int splineIndex)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].totalLength;
		}


		/// <summary>
		/// 获取样条起点到指定位置的长度
		/// </summary>
		public override float GetSplineLength(int splineIndex, float splineTime)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].GetLength(splineTime);
		}


		/// <summary>
		/// 通过样条长度获取点的位置
		/// </summary>
		public override float GetSplinePositionAtLength(int splineIndex, float splineLength)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].GetPositionAtLength(splineLength);
		}


		/// <summary>
		/// 求样条上与给定点最近的点
		/// </summary>
		/// <param name="given">给定点</param>
		/// <param name="segmentLength">分段长度，用来估算分段数量</param>
		/// <param name="minSegments">限制的最少分段数量</param>
		/// <param name="maxSegments">限制的最大分段数量</param>
		/// <returns>样条的 time 参数</returns>
		public override float GetClosestSplinePosition(int splineIndex, Vector3 given, float segmentLength, int minSegments = 8, int maxSegments = 64)
		{
			CheckAndResetAllSplines();
			return _splines[splineIndex].GetClosestPosition(given, segmentLength, minSegments, maxSegments);
		}




		/// <summary>
		/// 根据路径长度获取样条位置
		/// </summary>
		/// <param name="pathLength"> 从路径起点开始的路径长度。对于环状路径，该值可以为负值或大于一圈路径长度的值。</param>
		/// <param name="splineIndex"> 输入建议的样条下标（负值表示无建议），输出为指定路径长度处的样条下标。</param>
		/// <param name="splineTime"> 输出为指定路径长度处的样条位置 </param>
		public override void GetPathPositionAtPathLength(float pathLength, ref int splineIndex, ref float splineTime)
		{
			int lastSplineIndex = splinesCount - 1;
			CalculatePathLength(lastSplineIndex);
			float pathTotalLength = _splines[lastSplineIndex].pathLength;

			if (_isCircular) pathLength = (pathTotalLength + pathLength % pathTotalLength) % pathTotalLength;
			else
			{
				if (pathLength <= 0)
				{
					splineIndex = 0;
					splineTime = 0;
					return;
				}
				if (pathLength >= pathTotalLength)
				{
					splineIndex = lastSplineIndex;
					splineTime = 1;
					return;
				}
			}

			if (splineIndex < 0) splineIndex = (int)(pathLength / pathTotalLength * lastSplineIndex);
			float baseLength;

			if (_splines[splineIndex].pathLength > pathLength)
			{
				do
				{
					if (splineIndex == 0)
					{
						splineTime = _splines[0].GetPositionAtLength(pathLength);
						return;
					}
				} while (_splines[--splineIndex].pathLength > pathLength);

				baseLength = _splines[splineIndex].pathLength;
				splineIndex++;
			}
			else
			{
				while (_splines[++splineIndex].pathLength < pathLength) ;
				baseLength = _splines[splineIndex - 1].pathLength;
			}

			splineTime = _splines[splineIndex].GetPositionAtLength(pathLength - baseLength);
		}


		/// <summary>
		/// 求路径上与给定点最近的位置
		/// </summary>
		/// <param name="given">给定点</param>
		/// <param name="segmentLength">分段长度，用来估算分段数量</param>
		/// <param name="splineIndex"> 输出的样条下标 </param>
		/// <param name="splineTime"> 输出的样条位置 </param>
		/// <param name="minSegmentsEverySpline">限制的最少分段数量</param>
		/// <param name="maxSegmentsEverySpline">限制的最大分段数量</param>
		public override void GetClosestPathPosition(
			Vector3 given, float segmentLength,
			out int splineIndex, out float splineTime,
			int minSegmentsEverySpline = 8, int maxSegmentsEverySpline = 64)
		{
			CheckAndResetAllSplines();

			float time;
			float bestSqrMagnitude = float.MaxValue, sqrMagnitude;
			splineIndex = 0;
			splineTime = 0;

			for(int i = splinesCount - 1; i >= 0; i--)
			{
				time = _splines[i].GetClosestPosition(given, segmentLength, minSegmentsEverySpline, maxSegmentsEverySpline);
				sqrMagnitude = (_splines[i].GetPoint(time) - given).sqrMagnitude;

				if(sqrMagnitude < bestSqrMagnitude)
				{
					splineIndex = i;
					splineTime = time;
					bestSqrMagnitude = sqrMagnitude;
				}
			}
		}


		/// <summary>
		/// 从指定位置开始寻找下一个满足条件的点
		/// </summary>
		/// <param name="index"> 输入为开始位置的样条下标，输出为目标位置的样条下标 </param>
		/// <param name="time"> 输入为开始位置的样条时间，输出为目标位置的样条时间 </param>
		/// <param name="minDeltaAngle"> 最小角度差，越接近 maxDeltaAngle 越慢 </param>
		/// <param name="maxDeltaAngle"> 最大角度差，越接近 minDeltaAngle 越慢 </param>
		/// <param name="getRotation"> 计算路径上某点的处的旋转的方法，null 表示默认方法 </param>
		/// <returns> 如果已经查找到了路径末端，返回 true </returns>
		public override bool GetNextSplinePosition(
			ref int index, ref float time,
			float minDeltaAngle, float maxDeltaAngle,
			Func<int, float, Quaternion> getRotation = null)
		{
			if (getRotation == null) getRotation = (i, t) => GetSplineRotation(i, t);

			int beginIndex, endIndex;
			float beginTime, endTime;
			bool over = false;

			Quaternion originalRotation = getRotation(index, time);

			while (true)
			{
				beginIndex = index;
				beginTime = time;

				time += 0.1f;
				if (time >= 1)
				{
					index++;
					time = 0;

					if (index >= splinesCount)
					{
						// 已经到路径末端
						index = splinesCount - 1;
						time = 1;
						over = true;
					}
				}

				float deltaAngle = Quaternion.Angle(originalRotation, getRotation(index, time));

				if (deltaAngle > maxDeltaAngle)
				{
					// 最近的一次 + 0.2 跳过了目标，需要折半查找

					endIndex = index;
					endTime = time;

					while (true)
					{
						// 计算中点
						time = (beginTime + endTime) * 0.5f;
						if (beginIndex != endIndex)
						{
							if (1 - beginTime > endTime)
							{
								index = beginIndex;
								time += 0.5f;
							}
							else
							{
								index = endIndex;
								time -= 0.5f;
							}
						}

						deltaAngle = Quaternion.Angle(originalRotation, getRotation(index, time));

						if (deltaAngle > maxDeltaAngle)
						{
							endIndex = index;
							endTime = time;
						}
						else if (deltaAngle < minDeltaAngle)
						{
							beginIndex = index;
							beginTime = time;
						}
						else return false;
					}
				}
				else if (deltaAngle >= minDeltaAngle || over)
				{
					// 当前位置符合要求，结束查找
					return over;
				}
			}
		}




#if UNITY_EDITOR

		static Color _normalColor = new Color(1, 0.25f, 0.5f);
		static Color _selectedColor = new Color(1, 0.75f, 0f);
		const float _width = 2f;
		int _selectedSplineIndex = -1;


		void OnDrawGizmos()
		{
			CheckAndResetAllSplines();

			_normalColor.a = UnityEditor.Selection.activeGameObject == gameObject ? 1 : 0.5f;

			for (int i = splinesCount - 1; i >= 0; i--)
			{
				_splines[i].Draw(_selectedSplineIndex == i ? _selectedColor : _normalColor, _width);
			}
		}

#endif
	}
}