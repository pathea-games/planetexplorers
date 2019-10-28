using System;

namespace CameraForge
{
	[Menu("Input/Time/ControllerTime", 1)]
	public class ControllerTime : InputNode
	{
		public ControllerTime () {}

		public override Var Calculate ()
		{
			if (modifier != null && modifier.controller != null)
				return (Var)(modifier.controller.time);
			return (Var)(0f);
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
