using UnityEngine;
using UnityEngine.UI;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值 Image 颜色
	/// </summary>
	[AddComponentMenu("White Cat/Tween/UI/Image Color")]
	[RequireComponent(typeof(Image))]
	public class TweenImageColor : TweenBase
	{
		public Color from = Color.white;
		public Color to = Color.white;

		public int mask = -1;
		Color _temp;

		Image _image;
		Image image { get { return _image ? _image : _image = GetComponent<Image>(); } }

		Color _original;


		public override void OnTween(float factor)
		{
			_temp = image.color;

			if (mask.GetBit(0)) _temp.r = from.r + (to.r - from.r) * factor;
			if (mask.GetBit(1)) _temp.g = from.g + (to.g - from.g) * factor;
			if (mask.GetBit(2)) _temp.b = from.b + (to.b - from.b) * factor;
			if (mask.GetBit(3)) _temp.a = from.a + (to.a - from.a) * factor;

			image.color = _temp;
		}


		public override void OnRecord() { _original = image.color; }

		public override void OnRestore() { image.color = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent() { from = image.color; }

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent() { to = image.color; }

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom() { image.color = from; }

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo() { image.color = to; }
	}
}