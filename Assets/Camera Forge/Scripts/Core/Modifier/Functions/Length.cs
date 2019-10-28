using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Length",0)]
	public class Length : FunctionNode
	{
		public Length ()
		{
			X = new Slot ("X");
		}

		public override Var Calculate ()
		{
			X.Calculate();

			Var x = (Var)(X.value);
			Var c = x;
			if (!x.isNull)
			{
				if (x.type == EVarType.Bool)
				{
					c = x.value_i;
				}
				else if (x.type == EVarType.Int)
				{
					c = (int)(Mathf.Abs(x.value_i));
				}
				else if (x.type == EVarType.Float)
				{
					c = (float)(Mathf.Abs(x.value_f));
				}
				else if (x.type == EVarType.Vector)
				{
					c = (float)(x.value_v.magnitude);
				}
				else if (x.type == EVarType.Quaternion)
				{
					c = 1.0f;
				}
				else if (x.type == EVarType.Color)
				{
					c = x.value_c.grayscale * x.value_c.a;
				}
				else if (x.type == EVarType.String)
				{
					c = x.value_str.Length;
				}
			}
			return c;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}

		public Slot X;
	}
}
