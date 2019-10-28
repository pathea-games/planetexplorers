using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Variable/GetUserVar",10)]
	public class GetUserVar : MediaNode
	{
		public GetUserVar ()
		{
			V = new Slot ("Var");
		}

		public override Var Calculate ()
		{
			V.Calculate();

			if (V.value.isNull)
				return Var.Null;

			string varname = V.value.value_str.Trim();

			if (string.IsNullOrEmpty(varname))
				return Var.Null;

			if (modifier == null)
				return Var.Null;
			if (modifier.controller == null)
				return Var.Null;
			if (modifier.controller.executor == null)
				return Var.Null;

			return modifier.controller.executor.GetVar(varname);
		}

		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}

		public Slot V;
	}
}
