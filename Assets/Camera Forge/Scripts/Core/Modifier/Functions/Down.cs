using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Down", 23)]
	public class Down : FunctionNode
	{
		public Down ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			Vector3 retval = Vector3.down;
			if (!V.value.isNull)
				retval = V.value.value_q * Vector3.down;
			return retval;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
