using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Distance",4)]
	public class Distance : FunctionNode
	{
		public Distance ()
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
				if (a.type == EVarType.Bool && b.type == EVarType.Bool)
				{
					c = Mathf.Abs(a.value_i - b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Int)
				{
					c = Mathf.Abs(a.value_i - b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Float)
				{
					c = Mathf.Abs(a.value_f - b.value_f);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Int)
				{
					c = Mathf.Abs(a.value_f - b.value_f);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Float)
				{
					c = Mathf.Abs(a.value_f - b.value_f);
				}
				else if (a.type == EVarType.Vector && b.type == EVarType.Vector)
				{
					c = Vector3.Distance(a.value_v, b.value_v);
				}
				else if (a.type == EVarType.Color && b.type == EVarType.Color)
				{
					c = Vector3.Distance(a.value_v, b.value_v);
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
