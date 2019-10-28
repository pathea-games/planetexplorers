using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值锚点位置
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Rect Transform/Anchored Position")]
	[RequireComponent(typeof(RectTransform))]
	public class TweenAnchoredPosition : TweenBase
	{
		public Vector2 from;
		public Vector2 to;

		public int mask = -1;
		Vector2 _temp;

		Vector2 _original;


		public override void OnTween(float factor)
		{
			_temp = rectTransform.anchoredPosition;

			if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
			if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;

			rectTransform.anchoredPosition = _temp;
		}


		public override void OnRecord() { _original = rectTransform.anchoredPosition; }

		public override void OnRestore() { rectTransform.anchoredPosition = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = rectTransform.anchoredPosition;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = rectTransform.anchoredPosition;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			rectTransform.anchoredPosition = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			rectTransform.anchoredPosition = to;
		}
	}
}