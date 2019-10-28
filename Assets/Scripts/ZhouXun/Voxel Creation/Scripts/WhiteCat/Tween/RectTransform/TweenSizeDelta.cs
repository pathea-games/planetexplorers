using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值大小变化量
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Rect Transform/Size Delta")]
	[RequireComponent(typeof(RectTransform))]
	public class TweenSizeDelta : TweenBase
	{
		public Vector2 from;
		public Vector2 to;

		public int mask = -1;
		Vector2 _temp;

		Vector2 _original;


		public override void OnTween(float factor)
		{
			_temp = rectTransform.sizeDelta;

			if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
			if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;

			rectTransform.sizeDelta = _temp;
		}


		public override void OnRecord() { _original = rectTransform.sizeDelta; }

		public override void OnRestore() { rectTransform.sizeDelta = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = rectTransform.sizeDelta;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = rectTransform.sizeDelta;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			rectTransform.sizeDelta = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			rectTransform.sizeDelta = to;
		}
	}
}