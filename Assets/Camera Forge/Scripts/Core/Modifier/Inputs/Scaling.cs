using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Transform/Scaling", 3)]
	public class Scaling : InputNode
	{
		public Scaling ()
		{
			O = new Slot ("Object");
		}

		public override Var Calculate ()
		{
			O.Calculate();
			
			if (O.value.isNull)
				return Var.Null;
			
			string name = O.value.value_str;
			if (string.IsNullOrEmpty(name))
				return Var.Null;
			
			if (modifier == null)
				return Var.Null;
			
			if (modifier.controller == null)
				return Var.Null;
			
			if (modifier.controller.executor == null)
				return Var.Null;
			
			Transform t = CameraController.GetTransform(name);
			
			if (t == null)
				return Var.Null;
			else
				return t.localScale;
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {O}; }
		}

		public Slot O;
	}
}
