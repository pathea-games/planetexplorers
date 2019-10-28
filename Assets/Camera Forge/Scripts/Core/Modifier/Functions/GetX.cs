using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Component/Get X",0)]
	public class GetX : FunctionNode
	{
		public GetX ()
		{
			V = new Slot ("Vector");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return (Var)V.value.value_v.x;
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
