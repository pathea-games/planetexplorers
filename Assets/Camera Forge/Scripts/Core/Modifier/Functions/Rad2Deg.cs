using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Constant/Rad2Deg",1)]
	public class Rad2Deg : FunctionNode
	{
		public Rad2Deg ()
		{
			X = new Slot("Rad");
		}

		public override Var Calculate ()
		{
			X.Calculate();

			if (X.value.isNull)
				return Var.Null;
			if (X.value.type == EVarType.Vector)
				return X.value.value_v * 57.29578f;
			if (X.value.type == EVarType.Color)
				return X.value.value_c * 57.29578f;
			return X.value.value_f * 57.29578f;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}

		public Slot X;
	}
}
