using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/DeltaAngle", 21)]
	public class DeltaAngle : FunctionNode
	{
		public DeltaAngle ()
		{
			A = new Slot ("A");
			B = new Slot ("B");
		}

		public override Var Calculate ()
		{
			A.Calculate();
			B.Calculate();

			Var a = A.value;
			Var b = B.value;

			if (!a.isNull && !b.isNull)
			{
				if (a.type == EVarType.Vector && b.type == EVarType.Vector)
				{
					float x = Mathf.DeltaAngle(a.value_v.x, b.value_v.x);
					float y = Mathf.DeltaAngle(a.value_v.y, b.value_v.y);
					float z = Mathf.DeltaAngle(a.value_v.z, b.value_v.z);
					float w = Mathf.DeltaAngle(a.value_v.w, b.value_v.w);
					return new Vector4(x,y,z,w);
				}
				return Mathf.DeltaAngle(a.value_f, b.value_f);
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
