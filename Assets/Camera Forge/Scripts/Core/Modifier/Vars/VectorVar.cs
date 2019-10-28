using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/Vector", 3)]
	public class VectorVar : VarNode
	{
		public VectorVar ()
		{
			V = new Slot ("Value");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
			{
				if (V.value.type == EVarType.Quaternion)
					return V.value.value_q.eulerAngles;
				else
					return V.value.value_v;
			}
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
