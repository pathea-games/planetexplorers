using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值右上角锚点
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Rect Transform/Anchor Max")]
	[RequireComponent(typeof(RectTransform))]
	public class TweenAnchorMax : TweenBase
	{
		public Vector2 from;
		public Vector2 to;

		public int mask = -1;
		Vector2 _temp;

		Vector2 _original;


		public override void OnTween(float factor)
		{
			_temp = rectTransform.anchorMax;

			if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
			if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;

			rectTransform.anchorMax = _temp;
		}


		public override void OnRecord() { _original = rectTransform.anchorMax; }

		public override void OnRestore() { rectTransform.anchorMax = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = rectTransform.anchorMax;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = rectTransform.anchorMax;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			rectTransform.anchorMax = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			rectTransform.anchorMax = to;
		}
	}
}