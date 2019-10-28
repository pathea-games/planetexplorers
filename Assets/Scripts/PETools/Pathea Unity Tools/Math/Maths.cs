using UnityEngine;
using System;

namespace Pathea
{
	namespace Maths
	{
		public static class Maths
		{
			public const float Epsilon = 0.00001f;
			public const float Upsilon = 1e30f;
			public static bool IsPointInRect( float x, float y, Rect rect )
			{
				if ( x >= rect.x && x <= rect.x+rect.width &&
				    y >= rect.y && y <= rect.y+rect.height )
					return true;
				return false;
			}

			public static Vector3 ClampPointInBounds (Vector3 pos, Bounds bound)
			{
				if ( pos.x < bound.min.x )
					pos.x = bound.min.x;
				else if ( pos.x > bound.max.x )
					pos.x = bound.max.x;
				if ( pos.y < bound.min.y )
					pos.y = bound.min.y;
				else if ( pos.y > bound.max.y )
					pos.y = bound.max.y;
				if ( pos.z < bound.min.z )
					pos.z = bound.min.z;
				else if ( pos.z > bound.max.z )
					pos.z = bound.max.z;
				return pos;
			}
		}
	}
}
