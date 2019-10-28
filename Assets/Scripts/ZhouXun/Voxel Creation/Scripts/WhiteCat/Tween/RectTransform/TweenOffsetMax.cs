using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值 offsetMax
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Rect Transform/Offset Max")]
	[RequireComponent(typeof(RectTransform))]
	public class TweenOffsetMax : TweenBase
	{
		public Vector2 from;
		public Vector2 to;

		public int mask = -1;
		Vector2 _temp;

		Vector2 _original;


		public override void OnTween(float factor)
		{
			_temp = rectTransform.offsetMax;

			if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
			if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;

			rectTransform.offsetMax = _temp;
		}


		public override void OnRecord() { _original = rectTransform.offsetMax; }

		public override void OnRestore() { rectTransform.offsetMax = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = rectTransform.offsetMax;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = rectTransform.offsetMax;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			rectTransform.offsetMax = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			rectTransform.offsetMax = to;
		}
	}
}