using UnityEngine;
using UnityEngine.UI;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 插值 Canvas Group 不透明度
	/// </summary>
	[AddComponentMenu("White Cat/Tween/UI/Canvas Group Alpha")]
	[RequireComponent(typeof(CanvasGroup))]
	public class TweenCanvasGroupAlpha : TweenBase
	{
		[Range(0, 1.0f)] public float from = 1;
		[Range(0, 1.0f)] public float to = 1;
		float _original;


		CanvasGroup _group;
		CanvasGroup group { get { return _group ? _group : _group = GetComponent<CanvasGroup>(); } }


		public override void OnTween(float factor)
		{
			group.alpha = from + (to - from) * factor;
		}


		public override void OnRecord() { _original = group.alpha; }

		public override void OnRestore() { group.alpha = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent() { from = group.alpha; }

		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent() { to = group.alpha; }

		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom() { group.alpha = from; }

		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo() { group.alpha = to; }
	}
}