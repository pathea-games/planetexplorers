using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Max", 11)]
	public class Max : FunctionNode
	{
		public Max ()
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

			if (!a.isNull && !b.isNull)
			{
				Var c = a;
				if (a.type == EVarType.Bool && b.type == EVarType.Bool)
				{
					c = (a.value_b || b.value_b);
				}
				else if (a.type == EVarType.Bool && b.type == EVarType.Int)
				{
					c = (a.value_i > b.value_i) ? a.value_i : b.value_i;
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Bool)
				{
					c = (a.value_i > b.value_i) ? a.value_i : b.value_i;
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Int)
				{
					c = (a.value_i > b.value_i) ? a.value_i : b.value_i;
				}
				else if (a.type == EVarType.Int && b.type == EVarType.Float)
				{
					c = (a.value_f > b.value_f) ? a.value_f : b.value_f;
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Int)
				{
					c = (a.value_f > b.value_f) ? a.value_f : b.value_f;
				}
				else if (a.type == EVarType.Float && b.type == EVarType.Float)
				{
					c = (a.value_f > b.value_f) ? a.value_f : b.value_f;
				}
				else if (a.type == EVarType.Vector && b.type == EVarType.Vector)
				{
					Vector4 v = Vector4.zero;
					v.x = (a.value_v.x > b.value_v.x) ? a.value_v.x : b.value_v.x;
					v.y = (a.value_v.y > b.value_v.y) ? a.value_v.y : b.value_v.y;
					v.z = (a.value_v.z > b.value_v.z) ? a.value_v.z : b.value_v.z;
					v.w = (a.value_v.w > b.value_v.w) ? a.value_v.w : b.value_v.w;
					c = v;
				}
				else if (a.type == EVarType.Color && b.type == EVarType.Color)
				{
					Color v = Color.clear;
					v.r = (a.value_c.r > b.value_c.r) ? a.value_c.r : b.value_c.r;
					v.g = (a.value_c.g > b.value_c.g) ? a.value_c.g : b.value_c.g;
					v.b = (a.value_c.b > b.value_c.b) ? a.value_c.b : b.value_c.b;
					v.a = (a.value_c.a > b.value_c.a) ? a.value_c.a : b.value_c.a;
					c = v;
				}
				else if (a.type == EVarType.String && b.type == EVarType.String)
				{
					c = (string.Compare(a.value_str, b.value_str) > 0) ? a.value_str : b.value_str;
				}
				return c;
			}

			if (!a.isNull) return a;
			else if (!b.isNull) return b;
			else return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {A, B}; }
		}

		public Slot A;
		public Slot B;
	}
}
