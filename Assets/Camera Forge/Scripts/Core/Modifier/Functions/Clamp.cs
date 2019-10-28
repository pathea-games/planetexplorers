using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Standard/Clamp", 13)]
	public class Clamp : FunctionNode
	{
		public Clamp ()
		{
			X = new Slot ("X");
			Min = new Slot ("Min");
			Max = new Slot ("Max");
		}
		
		public override Var Calculate ()
		{
			X.Calculate();
			Min.Calculate();
			Max.Calculate();

			Var x = X.value;
			Var min = Min.value;
			Var max = Max.value;
			Var c = x;
			
			if (!x.isNull && !min.isNull && !max.isNull)
			{
				if (x.type == EVarType.Bool || x.type == EVarType.Int)
				{
					if ((min.type == EVarType.Bool || min.type == EVarType.Int)
					    && (max.type == EVarType.Bool || max.type == EVarType.Int))
					{
						if (c.value_i < min.value_i)
							c = min.value_i;
						else if (c.value_i > max.value_i)
							c = max.value_i;
						else
							c = c.value_i;
					}
				}
				else if (x.type == EVarType.Float)
				{
					if ((min.type == EVarType.Bool || min.type == EVarType.Int || min.type == EVarType.Float)
					    && (max.type == EVarType.Bool || max.type == EVarType.Int || max.type == EVarType.Float))
					{
						if (c.value_f < min.value_f)
							c = min.value_f;
						else if (c.value_f > max.value_f)
							c = max.value_f;
						else
							c = c.value_f;
					}
				}
				else if (x.type == EVarType.Vector)
				{
					if (min.type == EVarType.Vector && max.type == EVarType.Vector)
					{
						Vector4 v = c.value_v;

						if (v.x < min.value_v.x)
							v.x = min.value_v.x;
						else if (v.x > max.value_v.x)
							v.x = max.value_v.x;

						if (v.y < min.value_v.y)
							v.y = min.value_v.y;
						else if (v.y > max.value_v.y)
							v.y = max.value_v.y;

						if (v.z < min.value_v.z)
							v.z = min.value_v.z;
						else if (v.z > max.value_v.z)
							v.z = max.value_v.z;

						if (v.w < min.value_v.w)
							v.w = min.value_v.w;
						else if (v.w > max.value_v.w)
							v.w = max.value_v.w;

						c = v;
					}
				}
				else if (x.type == EVarType.Color)
				{
					if (min.type == EVarType.Color && max.type == EVarType.Color)
					{
						Color v = c.value_c;
						
						if (v.r < min.value_c.r)
							v.r = min.value_c.r;
						else if (v.r > max.value_c.r)
							v.r = max.value_c.r;
						
						if (v.g < min.value_c.g)
							v.g = min.value_c.g;
						else if (v.g > max.value_c.g)
							v.g = max.value_c.g;
						
						if (v.b < min.value_c.b)
							v.b = min.value_c.b;
						else if (v.b > max.value_c.b)
							v.b = max.value_c.b;
						
						if (v.a < min.value_c.a)
							v.a = min.value_c.a;
						else if (v.a > max.value_c.a)
							v.a = max.value_c.a;
						
						c = v;
					}
				}
			}
			return c;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[3] {X, Min, Max}; }
		}
		
		public Slot X;
		public Slot Min;
		public Slot Max;
	}
}
