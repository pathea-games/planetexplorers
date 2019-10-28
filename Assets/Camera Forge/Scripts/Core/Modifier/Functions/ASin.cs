using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Trigon/ASin",10)]
	public class ASin : FunctionNode
	{
		public ASin ()
		{
			X = new Slot ("X");
		}

		public override Var Calculate ()
		{
			X.Calculate();

			if (!X.value.isNull)
			{
				return (Var)(Mathf.Asin(Mathf.Clamp(X.value.value_f, -1f, 1f)));
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
