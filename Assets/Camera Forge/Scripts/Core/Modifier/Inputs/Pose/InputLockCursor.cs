using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Pose/Lock Cursor",3)]
	public class InputLockCursor : InputNode
	{
		public InputLockCursor ()
		{

		}

		public override Var Calculate ()
		{
			if (modifier == null)
				return Vector3.zero;
			return modifier.Prev.value.lockCursor;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
