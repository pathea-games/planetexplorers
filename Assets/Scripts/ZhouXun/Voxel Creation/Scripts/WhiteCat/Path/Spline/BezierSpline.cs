using System;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 贝塞尔样条
	/// </summary>
	[Serializable]
	public class BezierSpline : PathSpline
	{
		public BezierSpline(float error)
			: base(error)
		{
		}
	}
}