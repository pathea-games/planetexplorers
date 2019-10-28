using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/Int", 1)]
	public class IntVar : VarNode
	{
		public IntVar ()
		{
			V = new Slot ("Value");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return V.value.value_i;
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
