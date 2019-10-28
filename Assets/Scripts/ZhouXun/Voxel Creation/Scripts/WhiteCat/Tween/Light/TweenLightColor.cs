using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值 Light 颜色
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Light/Color")]
	[RequireComponent(typeof(Light))]
	public class TweenLightColor : TweenBase
	{
		public Color from = Color.white;
		public Color to = Color.white;

		public int mask = -1;
		Color _temp;

		Light _light;
		new Light light { get { return _light ? _light : _light = GetComponent<Light>(); } }

		Color _original;


		public override void OnTween(float factor)
		{
			_temp = light.color;

			if (mask.GetBit(0)) _temp.r = from.r + (to.r - from.r) * factor;
			if (mask.GetBit(1)) _temp.g = from.g + (to.g - from.g) * factor;
			if (mask.GetBit(2)) _temp.b = from.b + (to.b - from.b) * factor;
			if (mask.GetBit(3)) _temp.a = from.a + (to.a - from.a) * factor;

			light.color = _temp;
		}


		public override void OnRecord() { _original = light.color; }

		public override void OnRestore() { light.color = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent() { from = light.color; }

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent() { to = light.color; }

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom() { light.color = from; }

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo() { light.color = to; }
	}
}