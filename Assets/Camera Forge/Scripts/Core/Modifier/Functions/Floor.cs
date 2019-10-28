using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Floor", 6)]
	public class Floor : FunctionNode
	{
		public Floor ()
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
				if (x.type == EVarType.Float)
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
			return Mathf.Floor(x);
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}

		public Slot X;
	}
}
