using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Power", 4)]
	public class Power : FunctionNode
	{
		public Power ()
		{
			X = new Slot ("X");
			P = new Slot ("P");
		}

		public override Var Calculate ()
		{
			X.Calculate();
			P.Calculate();

			Var a = X.value;
			Var b = P.value;
			Var c = a;

			if (!a.isNull && !b.isNull)
			{
				float pow = b.value_f;
				if (a.type == EVarType.Bool || a.type == EVarType.Int || a.type == EVarType.Float)
				{
					float f = 0;
					try { f = Mathf.Pow(a.value_f, pow); }
					catch { f = 0; }
					if (float.IsNaN(f))
						f = 0;
					c = f;
				}
				if (a.type == EVarType.Vector)
				{
					Vector4 v = a.value_v;
					try { v.x = Mathf.Pow(v.x, pow); }
					catch { v.x = 0; }
					if (float.IsNaN(v.x))
						v.x = 0;
					try { v.y = Mathf.Pow(v.y, pow); }
					catch { v.y = 0; }
					if (float.IsNaN(v.y))
						v.y = 0;
					try { v.z = Mathf.Pow(v.z, pow); }
					catch { v.z = 0; }
					if (float.IsNaN(v.z))
						v.z = 0;
					try { v.w = Mathf.Pow(v.w, pow); }
					catch { v.w = 0; }
					if (float.IsNaN(v.w))
						v.w = 0;
					c = v;
				}
				if (a.type == EVarType.Color)
				{
					Color v = a.value_c;
					try { v.r = Mathf.Pow(v.r, pow); }
					catch { v.r = 0; }
					if (float.IsNaN(v.r))
						v.r = 0;
					try { v.g = Mathf.Pow(v.g, pow); }
					catch { v.g = 0; }
					if (float.IsNaN(v.g))
						v.g = 0;
					try { v.b = Mathf.Pow(v.b, pow); }
					catch { v.b = 0; }
					if (float.IsNaN(v.b))
						v.b = 0;
					try { v.a = Mathf.Pow(v.a, pow); }
					catch { v.a = 0; }
					if (float.IsNaN(v.a))
						v.a = 0;
					c = v;
				}
			}
			return c;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {X, P}; }
		}

		public Slot X;
		public Slot P;
	}
}
