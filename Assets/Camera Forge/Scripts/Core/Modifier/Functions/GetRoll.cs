using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Component/Get Roll",12)]
	public class GetRoll : FunctionNode
	{
		public GetRoll ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return (Var)V.value.value_v.z;
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
