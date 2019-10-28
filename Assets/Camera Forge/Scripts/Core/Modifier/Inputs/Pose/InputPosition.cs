using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Pose/Position",0)]
	public class InputPosition : InputNode
	{
		public InputPosition ()
		{

		}

		public override Var Calculate ()
		{
			if (modifier == null)
				return Vector3.zero;
			return modifier.Prev.value.position;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
