using System;
using UnityEngine;

namespace WhiteCat.Internal
{
	/// <summary>
	/// 基数样条
	/// </summary>
	[Serializable]
	public class CardinalSpline : PathSpline
	{
		[SerializeField] float _tension;		// 张力


		// 张力
		public float tension
		{
			get { return _tension; }
			set { _tension = Mathf.Clamp(value, 0.001f, 1000); }
		}


		public CardinalSpline(float error, float tension)
			: base(error)
		{
			this.tension = tension;
		}
	}
}