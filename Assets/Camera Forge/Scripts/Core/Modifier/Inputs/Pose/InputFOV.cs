using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Pose/FOV",3)]
	public class InputFOV : InputNode
	{
		public InputFOV ()
		{

		}

		public override Var Calculate ()
		{
			if (modifier == null)
				return Vector3.zero;
			return modifier.Prev.value.fov;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
