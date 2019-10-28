using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Multiply",2)]
	public class Multiply : FunctionNode
	{
		public Multiply ()
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
					c = (a.value_b && b.value_b);
				}
				else if (a.type == EVarType.Bool && b.type == EVarType.Int)
				{
					c = (a.value_i * b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Bool)
				{
					c = (a.value_i * b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Int)
				{
					c = (a.value_i * b.value_i);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Float)
				{
					c = (a.value_f * b.value_f);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Int)
				{
					c = (a.value_f * b.value_f);
				}
				else if (a.type == EVarType.Bool && b.type == EVarType.Float)
				{
					c = (a.value_f * b.value_f);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Bool)
				{
					c = (a.value_f * b.value_f);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Float)
				{
					c = (a.value_f * b.value_f);
				}

				else if (a.type == EVarType.Vector && b.type == EVarType.Int)
				{
					c = (a.value_v * b.value_f);
				}
				else if (a.type == EVarType.Vector && b.type == EVarType.Float)
				{
					c = (a.value_v * b.value_f);
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Vector)
				{
					c = (a.value_f * b.value_v);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Vector)
				{
					c = (a.value_f * b.value_v);
				}
				else if (a.type == EVarType.Vector && b.type == EVarType.Vector)
				{
					Vector4 retval;
					retval.x = a.value_v.x * b.value_v.x;
					retval.y = a.value_v.y * b.value_v.y;
					retval.z = a.value_v.z * b.value_v.z;
					retval.w = a.value_v.w * b.value_v.w;
					c = retval;
				}
				else if (a.type == EVarType.Vector && b.type == EVarType.Quaternion)
				{
					c = (b.value_q * a.value_v);
				}
				else if (a.type == EVarType.Quaternion && b.type == EVarType.Vector)
				{
					c = (a.value_q * b.value_v);
				}
				else if (a.type == EVarType.Quaternion && b.type == EVarType.Quaternion)
				{
					c = (a.value_q * b.value_q);
				}

				else if (a.type == EVarType.Int && b.type == EVarType.Color)
				{
					c = (a.value_f * b.value_c);
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Color)
				{
					c = (a.value_f * b.value_c);
				}
				else if (a.type == EVarType.Color && b.type == EVarType.Int)
				{
					c = (a.value_c * b.value_f);
				}
				else if (a.type == EVarType.Color && b.type == EVarType.Float)
				{
					c = (a.value_c * b.value_f);
				}
				else if (a.type == EVarType.Color && b.type == EVarType.Color)
				{
					c = (a.value_c * b.value_c);
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
