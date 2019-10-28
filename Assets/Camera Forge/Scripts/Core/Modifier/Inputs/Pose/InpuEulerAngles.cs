using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Pose/EulerAngles",1)]
	public class InputEulerAngles : InputNode
	{
		public InputEulerAngles ()
		{

		}

		public override Var Calculate ()
		{
			if (modifier == null)
				return Vector3.zero;
			return modifier.Prev.value.eulerAngles;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
