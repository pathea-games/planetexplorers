using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Logic/Default",1)]
	public class Default : FunctionNode
	{
		public Default ()
		{
			A = new Slot ("Var");
			B = new Slot ("Default");
		}

		public override Var Calculate ()
		{
			A.Calculate();
			if (A.value.isNull)
			{
				B.Calculate();
				return B.value;
			}
			else
			{
				return A.value;
			}
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {A, B}; }
		}

		public Slot A;
		public Slot B;
	}
}
