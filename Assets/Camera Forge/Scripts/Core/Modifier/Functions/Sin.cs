using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Math/Trigon/Sin",0)]
	public class Sin : FunctionNode
	{
		public Sin ()
		{
			A = new Slot ("Scale");
			W = new Slot ("Omega");
			X = new Slot ("X");
			Y = new Slot ("Phi");
			B = new Slot ("Offset");
		}

		public override Var Calculate ()
		{
			A.Calculate();
			W.Calculate();
			X.Calculate();
			Y.Calculate();
			B.Calculate();

			Vector4 scale = Vector4.one;
			Vector4 omega = Vector4.one;
			Vector4 phi = Vector4.zero;
			Vector4 offset = Vector4.zero;
			Vector4 _x = Vector4.zero;

			if (!A.value.isNull)
				scale = A.value.value_v;
			if (!W.value.isNull)
				omega = W.value.value_v;
			if (!X.value.isNull)
				_x = X.value.value_v;
			if (!Y.value.isNull)
				phi = Y.value.value_v;
			if (!B.value.isNull)
				offset = B.value.value_v;

			if (A.value.type == EVarType.Vector || A.value.type == EVarType.Color ||
			    W.value.type == EVarType.Vector || W.value.type == EVarType.Color ||
			    X.value.type == EVarType.Vector || X.value.type == EVarType.Color ||
			    Y.value.type == EVarType.Vector || Y.value.type == EVarType.Color ||
			    B.value.type == EVarType.Vector || B.value.type == EVarType.Color)
			{
				Vector4 sin = Vector4.zero;
				sin.x = scale.x * Mathf.Sin(omega.x*_x.x + phi.x) + offset.x;
				sin.y = scale.y * Mathf.Sin(omega.y*_x.y + phi.y) + offset.y;
				sin.z = scale.z * Mathf.Sin(omega.z*_x.z + phi.z) + offset.z;
				sin.w = scale.w * Mathf.Sin(omega.w*_x.w + phi.w) + offset.w;
				return sin;
			}
			else
			{
				float sin = scale.x * Mathf.Sin(omega.x*_x.x + phi.x) + offset.x;
				return sin;
			}
		}

		public override Slot[] slots
		{
			get { return new Slot[5] {A, W, X, Y, B}; }
		}

		public Slot A;
		public Slot W;
		public Slot X;
		public Slot Y;
		public Slot B;
	}
}
