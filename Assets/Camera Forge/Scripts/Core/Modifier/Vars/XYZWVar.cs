using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/XYZW", 6)]
	public class XYZWVar : VarNode
	{
		public XYZWVar ()
		{
			X = new Slot ("X");
			Y = new Slot ("Y");
			Z = new Slot ("Z");
			W = new Slot ("W");
		}
		
		public override Var Calculate ()
		{
			X.Calculate();
			Y.Calculate();
			Z.Calculate();
			W.Calculate();

			Vector4 v = Vector4.zero;

			if (!X.value.isNull)
				v.x = X.value.value_f;
			if (!Y.value.isNull)
				v.y = Y.value.value_f;
			if (!Z.value.isNull)
				v.z = Z.value.value_f;
			if (!W.value.isNull)
			    v.w = W.value.value_f;

			return v;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[4] {X, Y, Z, W}; }
		}
		
		public Slot X;
		public Slot Y;
		public Slot Z;
		public Slot W;
	}
}
