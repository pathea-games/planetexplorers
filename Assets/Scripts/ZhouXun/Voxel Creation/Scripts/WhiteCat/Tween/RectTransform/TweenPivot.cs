using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值轴心
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Rect Transform/Pivot")]
	[RequireComponent(typeof(RectTransform))]
	public class TweenPivot : TweenBase
	{
		public Vector2 from;
		public Vector2 to;

		public int mask = -1;
		Vector2 _temp;

		Vector2 _original;


		public override void OnTween(float factor)
		{
			_temp = rectTransform.pivot;

			if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
			if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;

			rectTransform.pivot = _temp;
		}


		public override void OnRecord() { _original = rectTransform.pivot; }

		public override void OnRestore() { rectTransform.pivot = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = rectTransform.pivot;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = rectTransform.pivot;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			rectTransform.pivot = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			rectTransform.pivot = to;
		}
	}
}