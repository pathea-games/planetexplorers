using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Back", 21)]
	public class Back : FunctionNode
	{
		public Back ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			Vector3 retval = Vector3.back;
			if (!V.value.isNull)
				retval = V.value.value_q * Vector3.back;
			return retval;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
