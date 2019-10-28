using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值缩放
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Transform/Scale")]
	public class TweenScale : TweenBase
	{
		public Vector3 from = Vector3.one;
		public Vector3 to = Vector3.one;

		public int mask = -1;
		Vector3 _temp;

		Vector3 _original;


		public override void OnTween(float factor)
		{
			_temp = transform.localScale;
			if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
			if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;
			if (mask.GetBit(2)) _temp.z = from.z + (to.z - from.z) * factor;
			transform.localScale = _temp;
		}


		public override void OnRecord() { _original = transform.localScale; }

		public override void OnRestore() { transform.localScale = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = transform.localScale;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = transform.localScale;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			transform.localScale = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			transform.localScale = to;
		}
	}
}