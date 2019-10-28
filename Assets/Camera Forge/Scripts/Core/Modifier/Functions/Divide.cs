using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Divide",3)]
	public class Divide : FunctionNode
	{
		public Divide ()
		{
			A = new Slot ("A");
			B = new Slot ("B");
		}

		public override Var Calculate ()
		{
			A.Calculate();
			B.Calculate();

			Var a = A.value;
			Var b = B.value;
			Var c = Var.Null;
			
			if (!a.isNull && !b.isNull)
			{
				if (Mathf.Abs(b.value_f) < 0.00001f)
					return Var.Null;
				if (a.type == EVarType.Bool && b.type == EVarType.Int)
				{
					c = (a.value_i / b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Bool)
				{
					c = (a.value_i / b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Int)
				{
					c = (a.value_i / b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Float)
				{
					c = (a.value_f / b.value_f);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Int)
				{
					c = (a.value_f / b.value_f);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Float)
				{
					c = (a.value_f / b.value_f);
				}

				else if (a.type == EVarType.Vector && b.type == EVarType.Int)
				{
					c = (a.value_v / b.value_f);
				}
				else if (a.type == EVarType.Vector && b.type == EVarType.Float)
				{
					c = (a.value_v / b.value_f);
				}
				else if (a.type == EVarType.Color && b.type == EVarType.Int)
				{
					c = (a.value_c / b.value_f);
				}
				else if (a.type == EVarType.Color && b.type == EVarType.Float)
				{
					c = (a.value_c / b.value_f);
				}
			}

			return c;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {A, B}; }
		}

		public Slot A;
		public Slot B;
	}
}
