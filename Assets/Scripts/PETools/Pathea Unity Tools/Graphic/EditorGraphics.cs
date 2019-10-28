using UnityEngine;
using System;

namespace Pathea
{
	namespace Graphic
	{
		public static class EditorGraphics
		{
			public static void DrawXZRect(Vector3 pos, Vector3 size, Color color)
			{
				Vector3 a = pos;
				Vector3 b = pos + Vector3.right * size.x;
				Vector3 c = b + Vector3.forward * size.z;
				Vector3 d = pos + Vector3.forward * size.z;
				Debug.DrawLine(a, b, color);
				Debug.DrawLine(b, c, color);
				Debug.DrawLine(c, d, color);
				Debug.DrawLine(d, a, color);
			}
		}
	}
}
