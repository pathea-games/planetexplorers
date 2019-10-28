using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Component/Get Yaw",10)]
	public class GetYaw : FunctionNode
	{
		public GetYaw ()
		{
			V = new Slot ("Euler");
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
