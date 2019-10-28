using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/SetUserVar",11)]
	public class SetUserVar : MediaNode
	{
		public SetUserVar ()
		{
			V = new Slot ("Var");
			Value = new Slot ("Value");
		}

		public override Var Calculate ()
		{
			V.Calculate();
			Value.Calculate();

			if (V.value.isNull)
				return Value.value;
			
			string varname = V.value.value_str.Trim();
			
			if (string.IsNullOrEmpty(varname))
				return Value.value;

			if (modifier == null)
				return Value.value;
			if (modifier.controller == null)
				return Value.value;
			if (modifier.controller.executor == null)
				return Value.value;

			modifier.controller.executor.SetVar(varname, Value.value);
			return Value.value;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {V, Value}; }
		}

		public Slot V;
		public Slot Value;
	}
}
