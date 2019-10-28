using UnityEngine;
using WhiteCat.Internal;
using WhiteCat.BitwiseOperationExtension;

namespace WhiteCat
{
	/// <summary>
	/// 插值欧拉角
	/// </summary>
	[AddComponentMenu("White Cat/Tween/Transform/Euler Angles")]
	public class TweenEulerAngles : TweenBase
	{
		public Space relativeTo = Space.Self;
		public Vector3 from;
		public Vector3 to;

		public int mask = -1;
		Vector3 _temp;

		Quaternion _original;


		public override void OnTween(float factor)
		{
			switch (relativeTo)
			{
				default:
				case Space.Self:
					_temp = transform.localEulerAngles;
					if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
					if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;
					if (mask.GetBit(2)) _temp.z = from.z + (to.z - from.z) * factor;
					transform.localEulerAngles = _temp;
					return;

				case Space.World:
					_temp = transform.eulerAngles;
					if (mask.GetBit(0)) _temp.x = from.x + (to.x - from.x) * factor;
					if (mask.GetBit(1)) _temp.y = from.y + (to.y - from.y) * factor;
					if (mask.GetBit(2)) _temp.z = from.z + (to.z - from.z) * factor;
					transform.eulerAngles = _temp;
					return;
			}
		}
		

		public override void OnRecord() { _original = transform.localRotation; }

		public override void OnRestore() { transform.localRotation = _original; }


		[ContextMenu("Set 'From' to current")]
		public void SetFromToCurrent()
		{
			from = relativeTo == Space.Self ? transform.localEulerAngles : transform.eulerAngles;
		}


		[ContextMenu("Set 'To' to current")]
		public void SetToToCurrent()
		{
			to = relativeTo == Space.Self ? transform.localEulerAngles : transform.eulerAngles;
		}


		[ContextMenu("Set current to 'From'")]
		public void SetCurrentToFrom()
		{
			if(relativeTo == Space.Self) transform.localEulerAngles = from;
			else transform.eulerAngles = from;
		}


		[ContextMenu("Set current to 'To'")]
		public void SetCurrentToTo()
		{
			if (relativeTo == Space.Self) transform.localEulerAngles = to;
			else transform.eulerAngles = to;
		}
	}
}