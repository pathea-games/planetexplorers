using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Component/Get Pitch",11)]
	public class GetPitch : FunctionNode
	{
		public GetPitch ()
		{
			V = new Slot ("Euler");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
				return (Var)(-V.value.value_v.x);
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
