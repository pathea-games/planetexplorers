using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值 Light 强度
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Light/Intensity")]
	[RequireComponent(typeof(Light))]
	public class TweenLightIntensity : TweenBase
	{
		public float from = 0.5f;
		public float to = 0.5f;
		
		Light _light;
		new Light light { get { return _light ? _light : _light = GetComponent<Light>(); } }

		float _original;


		public override void OnTween(float factor)
		{
			light.intensity = from + (to - from) * factor;
		}


		public override void OnRecord() { _original = light.intensity; }

		public override void OnRestore() { light.intensity = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent() { from = light.intensity; }

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent() { to = light.intensity; }

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom() { light.intensity = from; }

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo() { light.intensity = to; }
	}
}