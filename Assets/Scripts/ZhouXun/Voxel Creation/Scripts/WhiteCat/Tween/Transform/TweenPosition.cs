using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值移动
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Transform/Position")]
	public class TweenPosition : TweenBase
	{
		public Space relativeTo = Space.Self;
		public Vector3 from;
		public Vector3 to;

		public int mask = -1;
		Vector3 _temp;

		Vector3 _original;


		public override void OnTween(float factor)
		{
			switch (relativeTo)
			{
				default:
				case Space.Self:
					_temp = transform.localPosition;
					if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
					if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;
					if (mask.GetBit(2)) _temp.z = from.z + (to.z - from.z) * factor;
					transform.localPosition = _temp;
					return;

				case Space.World:
					_temp = transform.position;
					if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
					if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;
					if (mask.GetBit(2)) _temp.z = from.z + (to.z - from.z) * factor;
					transform.position = _temp;
					return;
			}
		}


		public override void OnRecord() { _original = transform.localPosition; }

		public override void OnRestore() { transform.localPosition = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = relativeTo == Space.Self ? transform.localPosition : transform.position;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = relativeTo == Space.Self ? transform.localPosition : transform.position;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if (relativeTo == Space.Self) transform.localPosition = from;
			else transform.position = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (relativeTo == Space.Self) transform.localPosition = to;
			else transform.position = to;
		}
	}
}