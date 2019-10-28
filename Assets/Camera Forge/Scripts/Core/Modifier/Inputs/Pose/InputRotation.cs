using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Pose/Rotation",2)]
	public class InputRotation : InputNode
	{
		public InputRotation ()
		{

		}

		public override Var Calculate ()
		{
			if (modifier == null)
				return Vector3.zero;
			return modifier.Prev.value.rotation;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
