using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Trigon/ATan",12)]
	public class ATan : FunctionNode
	{
		public ATan ()
		{
			X = new Slot ("X");
		}

		public override Var Calculate ()
		{
			X.Calculate();

			if (!X.value.isNull)
			{
				return (Var)(Mathf.Atan(X.value.value_f));
			}
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}

		public Slot X;
	}
}
