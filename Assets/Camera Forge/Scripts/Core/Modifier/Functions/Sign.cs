using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Sign", 20)]
	public class Sign : FunctionNode
	{
		public Sign ()
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
					if (x.value_i > 0)
						return (Var)((int)1);
					else if (x.value_i < 0)
						return (Var)((int)-1);
					else
						return (Var)((int)0);
				}
				else if (x.type == EVarType.Float)
				{
					return (Var)(sign(x.value_f));
				}
				else if (x.type == EVarType.Vector)
				{
					c = new Vector4(sign(x.value_v.x), sign(x.value_v.y), sign(x.value_v.z), sign(x.value_v.w));
				}
				else if (x.type == EVarType.Color)
				{
					c = new Color(sign(x.value_c.r), sign(x.value_c.g), sign(x.value_c.b), sign(x.value_c.a));
				}
			}
			return c;
		}

		float sign (float x)
		{
			if (x > 0)
				return 1f;
			else if (x < 0)
				return -1f;
			else
				return 0f;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}
		
		public Slot X;
	}
}
