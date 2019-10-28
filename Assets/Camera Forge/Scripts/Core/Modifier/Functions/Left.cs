using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Left", 25)]
	public class Left : FunctionNode
	{
		public Left ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			Vector3 retval = Vector3.left;
			if (!V.value.isNull)
				retval = V.value.value_q * Vector3.left;
			return retval;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
