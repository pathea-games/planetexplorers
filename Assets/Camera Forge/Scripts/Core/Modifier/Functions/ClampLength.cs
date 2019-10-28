using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Vector/ClampLength",6)]
	public class ClampLength : FunctionNode
	{
		public ClampLength ()
		{
			V = new Slot ("Vector");
			Length = new Slot ("Length");
		}
		
		public override Var Calculate ()
		{
			V.Calculate();
			Length.Calculate();

			Var v = V.value;
			Var length = Length.value;
			Var c = v;
			
			if (!v.isNull && !length.isNull)
			{
				float len = length.value_f;
				if (v.type == EVarType.Vector)
				{
					Vector4 newvec = v.value_v;
					if (newvec.magnitude > len)
						newvec = newvec.normalized * len;
					c = newvec;
				}
				if (v.type == EVarType.Color)
				{
					Color newcolor = v.value_c;
					float gray = newcolor.grayscale;
					if (gray > len)
					{
						newcolor /= gray;
						newcolor *= len;
					}
					c = newcolor;
				}
			}
			return c;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[2] {V, Length}; }
		}
		
		public Slot V;
		public Slot Length;
	}
}
