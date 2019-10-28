using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Abs", 20)]
	public class Abs : FunctionNode
	{
		public Abs ()
		{
			X = new Slot ("X");
		}

		public override Var Calculate ()
		{
			X.Calculate();

			Var c = X.value;
			if (!X.value.isNull)
			{
				if (X.value.type == EVarType.Int)
				{
					c = (int)(Mathf.Abs(X.value.value_i));
				}
				else if (X.value.type == EVarType.Float)
				{
					c = (float)(Mathf.Abs(X.value.value_f));
				}
				else if (X.value.type == EVarType.Vector)
				{
					c = new Vector4(Mathf.Abs(X.value.value_v.x), Mathf.Abs(X.value.value_v.y), Mathf.Abs(X.value.value_v.z), Mathf.Abs(X.value.value_v.w));
				}
				else if (X.value.type == EVarType.Color)
				{
					c = new Color(Mathf.Abs(X.value.value_c.r), Mathf.Abs(X.value.value_c.g), Mathf.Abs(X.value.value_c.b), Mathf.Abs(X.value.value_c.a));
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
