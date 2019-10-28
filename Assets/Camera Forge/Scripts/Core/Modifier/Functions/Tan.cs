using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Trigon/Tan",2)]
	public class Tan : FunctionNode
	{
		public Tan ()
		{
			X = new Slot ("X");
		}

		public override Var Calculate ()
		{
			X.Calculate();
			if (!X.value.isNull)
			{
				float f = Mathf.Tan(X.value.value_f);
				if (float.IsNaN(f))
					f = 0f;
				return (Var)(f);
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
