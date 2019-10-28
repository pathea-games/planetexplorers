using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat
{
	/// <summary>
	/// 路径
	/// </summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public abstract class Path : BaseBehaviour
	{
		/// <summary>
		/// 变化事件
		/// </summary>
		public event Action onChange;

		// 触发变化事件
		protected void TriggerOnChangeEvents()
		{
			if (onChange != null) onChange();
		}




		/// <summary>
		/// 路径是否首尾相接
		/// </summary>
		public abstract bool isCircular { get; set; }

		/// <summary>
		/// 路径长度误差
		/// </summary>
		public abstract float lengthError { get; set; }

		/// <summary>
		/// 节点总数
		/// </summary>
		public abstract int nodesCount { get; }

		/// <summary>
		/// 样条总数
		/// </summary>
		public abstract int splinesCount { get; }

		/// <summary>
		/// 节点是否可以移除
		/// </summary>
		public abstract bool isNodeRemovable { get; }

		/// <summary>
		/// 长度是否有效
		/// </summary>
		public abstract bool isLengthValid { get; }

		/// <summary>
		/// 路径总长度
		/// </summary>
		public abstract float pathTotalLength { get; }




		/// <summary>
		/// 检查是否需要重置所有样条参数，如果需要则执行重置
		/// </summary>
		public abstract void CheckAndResetAllSplines();

		/// <summary>
		/// 计算路径长度
		/// </summary>
		public abstract void CalculatePathLength(int splineIndex);

		/// <summary>
		/// 获取从路径起点到曲线段终点的路径长度
		/// </summary>
		public abstract float GetPathLength(int splineIndex);

		/// <summary>
		/// 获取从路径起点到曲线段上某点的路径长度
		/// </summary>
		public abstract float GetPathLength(int splineIndex, float splineTime);

		/// <summary>
		/// 清除所有采样数据
		/// </summary>
		public abstract void ClearAllSamples();

		/// <summary>
		/// 插入节点。index 范围是 [0, nodesCount], 节点位置自动计算
		/// </summary>
		public abstract void InsertNode(int nodeIndex);
		
		/// <summary>
		/// 移除节点。index 范围是 [0, nodesCount)
		/// </summary>
		public abstract void RemoveNode(int nodeIndex);




		/// <summary>
		/// 获取节点本地位置
		/// </summary>
		public abstract Vector3 GetNodeLocalPosition(int nodeIndex);

		/// <summary>
		/// 设置节点本地位置
		/// </summary>
		public abstract void SetNodeLocalPosition(int nodeIndex, Vector3 localPosition);

		/// <summary>
		/// 获取节点世界位置
		/// </summary>
		public abstract Vector3 GetNodePosition(int nodeIndex);

		/// <summary>
		/// 设置节点世界位置
		/// </summary>
		public abstract void SetNodePosition(int nodeIndex, Vector3 position);

		/// <summary>
		/// 获取节点本地旋转
		/// </summary>
		public abstract Quaternion GetNodeLocalRotation(int nodeIndex);

		/// <summary>
		/// 设置节点本地旋转
		/// </summary>
		public abstract void SetNodeLocalRotation(int nodeIndex, Quaternion localRotation);

		/// <summary>
		/// 获取节点世界旋转
		/// </summary>
		public abstract Quaternion GetNodeRotation(int nodeIndex);

		/// <summary>
		/// 设置节点世界旋转
		/// </summary>
		public abstract void SetNodeRotation(int nodeIndex, Quaternion rotation);




		/// <summary>
		/// 获取样条上的点坐标
		/// </summary>
		public abstract Vector3 GetSplinePoint(int splineIndex, float splineTime);

		/// <summary>
		/// 获取样条上的导数
		/// </summary>
		public abstract Vector3 GetSplineDerivative(int splineIndex, float splineTime);

		/// <summary>
		/// 获取样条上的二阶导数
		/// </summary>
		public abstract Vector3 GetSplineSecondDerivative(int splineIndex, float splineTime);

		/// <summary>
		/// 获取样条某点处的旋转，旋转朝向切线方向
		/// </summary>
		public abstract Quaternion GetSplineRotation(int splineIndex, float splineTime, Vector3 upwards, bool reverseForward = false);

		/// <summary>
		/// 获取样条某点处的旋转，旋转朝向切线方向，up 方向由节点旋转插值获得
		/// </summary>
		public abstract Quaternion GetSplineRotation(int splineIndex, float splineTime, bool reverseForward = false);

		/// <summary>
		/// 样条长度采样是否无效
		/// </summary>
		public abstract bool IsSplineSamplesInvalid(int splineIndex);

		/// <summary>
		/// 获取样条长度采样数量
		/// </summary>
		public abstract int GetSplineSamplesCount(int splineIndex);

		/// <summary>
		/// 获取样条的长度
		/// </summary>
		public abstract float GetSplineTotalLength(int splineIndex);

		/// <summary>
		/// 获取样条起点到指定位置的长度
		/// </summary>
		public abstract float GetSplineLength(int splineIndex, float splineTime);

		/// <summary>
		/// 通过样条长度获取样条位置
		/// </summary>
		public abstract float GetSplinePositionAtLength(int splineIndex, float splineLength);

		/// <summary>
		/// 求样条上与给定点最近的点
		/// </summary>
		/// <param name="given">给定点</param>
		/// <param name="segmentLength">分段长度，用来估算分段数量</param>
		/// <param name="minSegments">限制的最少分段数量</param>
		/// <param name="maxSegments">限制的最大分段数量</param>
		/// <returns>样条的 time 参数</returns>
		public abstract float GetClosestSplinePosition(int splineIndex, Vector3 given, float segmentLength, int minSegments = 8, int maxSegments = 64);




		/// <summary>
		/// 根据路径长度获取样条位置
		/// </summary>
		/// <param name="pathLength"> 从路径起点开始的路径长度。对于环状路径，该值可以为负值或大于一圈路径长度的值。</param>
		/// <param name="splineIndex"> 输入建议的样条下标（负值表示无建议），输出为指定路径长度处的样条下标。</param>
		/// <param name="splineTime"> 输出为指定路径长度处的样条位置 </param>
		public abstract void GetPathPositionAtPathLength(float pathLength, ref int splineIndex, ref float splineTime);

		/// <summary>
		/// 求路径上与给定点最近的位置
		/// </summary>
		/// <param name="given">给定点</param>
		/// <param name="segmentLength">分段长度，用来估算分段数量</param>
		/// <param name="splineIndex"> 输出的样条下标 </param>
		/// <param name="splineTime"> 输出的样条位置 </param>
		/// <param name="minSegmentsEverySpline">限制的最少分段数量</param>
		/// <param name="maxSegmentsEverySpline">限制的最大分段数量</param>
		public abstract void GetClosestPathPosition(
			Vector3 given, float segmentLength,
			out int splineIndex, out float splineTime,
			int minSegmentsEverySpline = 8, int maxSegmentsEverySpline = 64);

		/// <summary>
		/// 从指定位置开始寻找下一个满足条件的点
		/// </summary>
		/// <param name="index"> 输入为开始位置的样条下标，输出为目标位置的样条下标 </param>
		/// <param name="time"> 输入为开始位置的样条时间，输出为目标位置的样条时间 </param>
		/// <param name="minDeltaAngle"> 最小角度差，越接近 maxDeltaAngle 越慢 </param>
		/// <param name="maxDeltaAngle"> 最大角度差，越接近 minDeltaAngle 越慢 </param>
		/// <param name="getRotation"> 计算路径上某点的处的旋转的方法，null 表示默认方法 </param>
		/// <returns> 如果已经查找到了路径末端，返回 true </returns>
		public abstract bool GetNextSplinePosition(
			ref int index, ref float time,
			float minDeltaAngle, float maxDeltaAngle,
			Func<int, float, Quaternion> getRotation = null);
	}
}