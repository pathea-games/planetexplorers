using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Sqrt", 5)]
	public class Sqrt : FunctionNode
	{
		public Sqrt ()
		{
			X = new Slot ("X");
		}

		public override Var Calculate ()
		{
			X.Calculate();

			Var x = X.value;
			Var c = x;
			if (!x.isNull)
			{
				if (x.type == EVarType.Int)
				{
					c = sqrt(x.value_i);
				}
				else if (x.type == EVarType.Float)
				{
					c = sqrt(x.value_f);
				}
				else if (x.type == EVarType.Vector)
				{
					c = new Vector4(sqrt(x.value_v.x), sqrt(x.value_v.y), sqrt(x.value_v.z), sqrt(x.value_v.w));
				}
				else if (x.type == EVarType.Color)
				{
					c = new Color(sqrt(x.value_c.r), sqrt(x.value_c.g), sqrt(x.value_c.b), sqrt(x.value_c.a));
				}
			}
			return c;
		}

		float sqrt (float x)
		{
			if (x < 0)
				return 0;
			else
				return Mathf.Sqrt(x);
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}

		public Slot X;
	}
}
