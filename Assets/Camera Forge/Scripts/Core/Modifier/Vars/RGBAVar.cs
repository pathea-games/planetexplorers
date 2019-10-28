using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/RGBA", 7)]
	public class RGBAVar : VarNode
	{
		public RGBAVar ()
		{
			R = new Slot ("R");
			G = new Slot ("G");
			B = new Slot ("B");
			A = new Slot ("A");
		}
		
		public override Var Calculate ()
		{
			R.Calculate();
			G.Calculate();
			B.Calculate();
			A.Calculate();

			Color c = Color.clear;

			if (!R.value.isNull)
				c.r = R.value.value_f;
			if (!G.value.isNull)
				c.g = G.value.value_f;
			if (!B.value.isNull)
				c.b = B.value.value_f;
			if (!A.value.isNull)
				c.a = A.value.value_f;

			return c;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[4] {R, G, B, A}; }
		}

		public Slot R;
		public Slot G;
		public Slot B;
		public Slot A;
	}
}
