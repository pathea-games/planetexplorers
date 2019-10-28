using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Angle",5)]
	public class Angle : FunctionNode
	{
		public Angle ()
		{
			A = new Slot ("A");
			B = new Slot ("B");
		}

		public override Var Calculate ()
		{
			A.Calculate();
			B.Calculate();

			if (!A.value.isNull && !B.value.isNull)
			{
				if (A.value.type == EVarType.Vector && B.value.type == EVarType.Vector)
					return (Var)(Vector3.Angle(new Vector3(A.value.value_v.x, A.value.value_v.y, A.value.value_v.z),
					                           new Vector3(B.value.value_v.x, B.value.value_v.y, B.value.value_v.z)));
				return (Var)(Mathf.DeltaAngle(A.value.value_f, B.value.value_f));
			}

			return Var.Null;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {A, B}; }
		}

		public Slot A;
		public Slot B;
	}
}
