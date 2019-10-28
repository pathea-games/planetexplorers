using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat
{
	/// <summary>
	/// 插值旋转
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Transform/Rotation")]
	public class TweenRotation : TweenBase
	{
		public Space relativeTo = Space.Self;
		[EulerAngles] public Quaternion from = Quaternion.identity;
		[EulerAngles] public Quaternion to = Quaternion.identity;
		Quaternion _original;


		public override void OnTween(float factor)
		{
			switch (relativeTo)
			{
				default:
				case Space.Self:
					transform.localRotation = Quaternion.Slerp(from, to, factor);
					return;

				case Space.World:
					transform.rotation = Quaternion.Slerp(from, to, factor);
					return;
			}
		}


		public override void OnRecord() { _original = transform.localRotation; }

		public override void OnRestore() { transform.localRotation = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = relativeTo == Space.Self ? transform.localRotation : transform.rotation;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = relativeTo == Space.Self ? transform.localRotation : transform.rotation;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (relativeTo == Space.Self) transform.localRotation = from;
			else transform.rotation = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (relativeTo == Space.Self) transform.localRotation = to;
			else transform.rotation = to;
		}
	}
}