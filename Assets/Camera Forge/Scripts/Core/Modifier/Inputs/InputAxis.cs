using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/InputAxis", -1)]
	public class InputAxis : InputNode
	{
		public InputAxis ()
		{
			A = new Slot ("Axis");
		}

		public override Var Calculate ()
		{
			A.Calculate();

			if (A.value.isNull)
				return 0f;

			string axis = A.value.value_str;

			if (string.IsNullOrEmpty(axis))
				return 0f;

			return InputModule.Axis(axis);
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {A}; }
		}

		public Slot A;
	}
}
