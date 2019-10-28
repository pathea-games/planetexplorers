using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/Quaternion", 4)]
	public class QuaternionVar : VarNode
	{
		public QuaternionVar ()
		{
			V = new Slot ("Value");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			if (!V.value.isNull)
			{
				if (V.value.type == EVarType.Vector)
					return Quaternion.Euler(new Vector3(V.value.value_v.x, V.value.value_v.y, V.value.value_v.z));
				else
					return V.value.value_q;
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
