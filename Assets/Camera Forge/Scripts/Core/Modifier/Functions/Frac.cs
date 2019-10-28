using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Frac", 9)]
	public class Frac : FunctionNode
	{
		public Frac ()
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
					c = 0;
				}
				else if (x.type == EVarType.Float)
				{
					c = func(x.value_f);
				}
				else if (x.type == EVarType.Vector)
				{
					c = new Vector4(func(x.value_v.x), func(x.value_v.y), func(x.value_v.z), func(x.value_v.w));
				}
				else if (x.type == EVarType.Color)
				{
					c = new Color(func(x.value_c.r), func(x.value_c.g), func(x.value_c.b), func(x.value_c.a));
				}
			}
			return c;
		}

		float func (float x)
		{
			return x - Mathf.Floor(x);
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}

		public Slot X;
	}
}
