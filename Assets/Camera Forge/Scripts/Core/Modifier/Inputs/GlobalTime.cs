using System;

namespace CameraForge
{
	[Menu("Input/Time/GlobalTime", 0)]
	public class GlobalTime : InputNode
	{
		public GlobalTime () {}

		public override Var Calculate ()
		{
			return (Var)(UnityEngine.Time.time);
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
