using System;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 路径样条
	/// </summary>
	[Serializable]
	public class PathSpline : Spline
	{
		public float pathLength;		// 从路径起点到曲线段终点的路径长度


		/// 构造方法
		public PathSpline(float error)
		{
			this.error = error;
		}
	}
}