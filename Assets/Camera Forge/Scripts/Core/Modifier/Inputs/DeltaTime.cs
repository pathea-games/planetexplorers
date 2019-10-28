using System;

namespace CameraForge
{
	[Menu("Input/Time/DeltaTime", 3)]
	public class DeltaTime : InputNode
	{
		public DeltaTime () {}

		public override Var Calculate ()
		{
			return (Var)(UnityEngine.Time.deltaTime);
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
