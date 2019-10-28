using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Quaternion/LookAt", 60)]
	public class LookAt : FunctionNode
	{
		public LookAt ()
		{
			F = new Slot ("Forward");
			U = new Slot ("Up");
			U.value = Vector3.up;
		}

		public override Var Calculate ()
		{
			F.Calculate();
			if (!F.value.isNull)
			{
				Vector3 forward = F.value.value_v;
				if (forward == Vector3.zero)
					return Quaternion.identity;
				else
					return Quaternion.LookRotation(F.value.value_v, U.value.value_v);
			}
			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {F,U}; }
		}

		public Slot F;
		public Slot U;
	}
}
