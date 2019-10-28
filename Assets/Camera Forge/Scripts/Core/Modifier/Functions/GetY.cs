using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Component/Get Y",1)]
	public class GetY : FunctionNode
	{
		public GetY ()
		{
			V = new Slot ("Vector");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return (Var)V.value.value_v.y;
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
