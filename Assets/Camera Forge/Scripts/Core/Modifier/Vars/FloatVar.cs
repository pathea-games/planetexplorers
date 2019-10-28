using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/Float", 2)]
	public class FloatVar : VarNode
	{
		public FloatVar ()
		{
			V = new Slot ("Value");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return V.value.value_f;
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
