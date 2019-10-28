using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/Normalize",1)]
	public class Normalize : FunctionNode
	{
		public Normalize ()
		{
			X = new Slot ("X");
		}
		
		public override Var Calculate ()
		{
			X.Calculate();
			
			Var x = X.value;
			Var c = x;
			if (!x.isNull)
			{
				if (x.type == EVarType.Int)
				{
					if (x.value_i > 0)
						return (Var)((int)1);
					else if (x.value_i < 0)
						return (Var)((int)-1);
					else
						return (Var)((int)0);
				}
				else if (x.type == EVarType.Float)
				{
					return (Var)(sign(x.value_f));
				}
				else if (x.type == EVarType.Vector)
				{
					return (Var)(x.value_v.normalized);
				}
			}
			return c;
		}

		float sign (float x)
		{
			if (x > 0)
				return 1f;
			else if (x < 0)
				return -1f;
			else
				return 0f;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[1] {X}; }
		}
		
		public Slot X;
	}
}
