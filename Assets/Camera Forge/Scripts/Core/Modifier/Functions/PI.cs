using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Constant/PI",0)]
	public class PI : FunctionNode
	{
		public PI () {}

		public override Var Calculate ()
		{
			return 3.1415928125f;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
