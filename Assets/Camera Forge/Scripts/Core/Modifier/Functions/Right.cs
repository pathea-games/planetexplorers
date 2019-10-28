using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Right", 24)]
	public class Right : FunctionNode
	{
		public Right ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			Vector3 retval = Vector3.right;
			if (!V.value.isNull)
				retval = V.value.value_q * Vector3.right;
			return retval;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
