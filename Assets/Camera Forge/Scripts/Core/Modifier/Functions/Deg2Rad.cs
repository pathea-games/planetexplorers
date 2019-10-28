using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Constant/Deg2Rad",2)]
	public class Deg2Rad : FunctionNode
	{
		public Deg2Rad ()
		{
			X = new Slot("Deg");
		}
		
		public override Var Calculate ()
		{
			X.Calculate();

			if (X.value.isNull)
				return Var.Null;
			if (X.value.type == EVarType.Vector)
				return X.value.value_v * 0.0174532924f;
			if (X.value.type == EVarType.Color)
				return X.value.value_c * 0.0174532924f;
			return X.value.value_f * 0.0174532924f;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}
		
		public Slot X;
	}
}
