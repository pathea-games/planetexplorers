using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Trigon/ATan2",13)]
	public class ATan2 : FunctionNode
	{
		public ATan2 ()
		{
			X = new Slot ("X");
			Y = new Slot ("Y");
		}

		public override Var Calculate ()
		{
			X.Calculate();
			Y.Calculate();

			if (!X.value.isNull && !Y.value.isNull)
			{
				return (Var)(Mathf.Atan2(Y.value.value_f, X.value.value_f));
			}
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {X, Y}; }
		}

		public Slot X;
		public Slot Y;
	}
}
