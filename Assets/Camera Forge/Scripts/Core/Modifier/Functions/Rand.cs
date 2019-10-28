using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Rand", 50)]
	public class Rand : FunctionNode
	{
		public Rand () {}

		public override Var Calculate ()
		{
			float f = UnityEngine.Random.value;
			Var c = new Var ();
			c = f;
			return c;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
