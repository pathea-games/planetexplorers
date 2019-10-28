using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 插值变换
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Transform/Transform")]
	public class TweenTransform : TweenBase
	{
		public Transform from;
		public Transform to;

		Vector3 _originalPosition;
		Quaternion _originalRotation;
		Vector3 _originalScale;


		public override void OnTween(float factor)
		{
			if (from && to)
			{
				transform.position = from.position + (to.position - from.position) * factor;
				transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, factor);
				transform.localScale = from.localScale + (to.localScale - from.localScale) * factor;
			}
		}


		public override void OnRecord()
		{
			_originalPosition = transform.localPosition;
			_originalRotation = transform.localRotation;
			_originalScale = transform.localScale;
		}

		public override void OnRestore()
		{
			transform.localPosition = _originalPosition;
			transform.localRotation = _originalRotation;
			transform.localScale = _originalScale;
		}


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			if (from)
			{
				from.position = transform.position;
				from.rotation = transform.rotation;
				from.localScale = transform.localScale;
			}
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			if (to)
			{
				to.position = transform.position;
				to.rotation = transform.rotation;
				to.localScale = transform.localScale;
			}
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (from)
			{
				transform.position = from.position;
				transform.rotation = from.rotation;
				transform.localScale = from.localScale;
			}
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (to)
			{
				transform.position = to.position;
				transform.rotation = to.rotation;
				transform.localScale = to.localScale;
			}
		}
	}
}