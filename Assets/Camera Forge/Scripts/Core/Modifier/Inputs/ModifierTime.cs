using System;

namespace CameraForge
{
	[Menu("Input/Time/ModifierTime", 2)]
	public class ModifierTime : InputNode
	{
		public ModifierTime () {}

		public override Var Calculate ()
		{
			return (Var)((modifier != null) ? modifier.time : 0.0f);
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
