using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Component/Get Z",2)]
	public class GetZ : FunctionNode
	{
		public GetZ ()
		{
			V = new Slot ("Vector");
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
