using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/Bool", 0)]
	public class BoolVar : VarNode
	{
		public BoolVar ()
		{
			V = new Slot ("Value");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return (bool)(Mathf.Abs(V.value.value_f) < 0.00001f);
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
