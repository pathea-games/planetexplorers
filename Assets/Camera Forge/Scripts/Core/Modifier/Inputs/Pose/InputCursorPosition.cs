using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Pose/Cursor Position",3)]
	public class InputCursorPosition : InputNode
	{
		public InputCursorPosition ()
		{

		}

		public override Var Calculate ()
		{
			if (modifier == null)
				return Vector3.zero;
			return modifier.Prev.value.cursorPos;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
