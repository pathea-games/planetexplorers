using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Component/Get W",3)]
	public class GetW : FunctionNode
	{
		public GetW ()
		{
			V = new Slot ("Vector");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return (Var)V.value.value_v.w;
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
