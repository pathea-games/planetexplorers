using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值锚点3D位置
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Rect Transform/Anchored Position 3D")]
	[RequireComponent(typeof(RectTransform))]
	public class TweenAnchoredPosition3D : TweenBase
	{
		public Vector3 from;
		public Vector3 to;

		public int mask = -1;
		Vector3 _temp;

		Vector3 _original;


		public override void OnTween(float factor)
		{
			_temp = rectTransform.anchoredPosition3D;

			if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
			if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;
			if (mask.GetBit(2)) _temp.z = from.z + (to.z - from.z) * factor;

			rectTransform.anchoredPosition3D = _temp;
		}


		public override void OnRecord() { _original = rectTransform.anchoredPosition3D; }

		public override void OnRestore() { rectTransform.anchoredPosition3D = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = rectTransform.anchoredPosition3D;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = rectTransform.anchoredPosition3D;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			rectTransform.anchoredPosition3D = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			rectTransform.anchoredPosition3D = to;
		}
	}
}