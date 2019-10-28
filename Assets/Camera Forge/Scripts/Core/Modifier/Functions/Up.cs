using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Up", 22)]
	public class Up : FunctionNode
	{
		public Up ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			Vector3 retval = Vector3.up;
			if (!V.value.isNull)
				retval = V.value.value_q * Vector3.up;
			return retval;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
