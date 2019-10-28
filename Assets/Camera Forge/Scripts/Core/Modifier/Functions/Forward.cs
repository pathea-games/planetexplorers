using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Forward", 20)]
	public class Forward : FunctionNode
	{
		public Forward ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			Vector3 retval = Vector3.forward;
			if (!V.value.isNull)
				retval = V.value.value_q * Vector3.forward;
			return retval;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
