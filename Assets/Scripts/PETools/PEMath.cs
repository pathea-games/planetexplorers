using UnityEngine;
using System.Collections;

namespace PETools
{
	public static class PEMath
	{
#if UNITY_STANDALONE_LINUX 
		//In linux64, System.Single.Epsilon would be evaluated to zero
		public const float Epsilon = 1.192092896e-07F;
#else
		public const float Epsilon = System.Single.Epsilon;
#endif
		public const int MC_ISO_VALUE = 128;
		public const float MC_ISO_VALUEF = 127.5f;

		public struct DrawTarget
		{
			public RaycastHit rch;
			public IntVector3 snapto;
			public IntVector3 cursor;
		}

//		public static bool RayCastDrawTarget(Ray ray, out DrawTarget target, int minvol, bool voxelbest = false)
//		{
//			target = new DrawTarget();
//
//			RaycastHit rch_voxel = new RaycastHit();
//		}

		public static bool RayCastVoxel (Ray ray, out RaycastHit rch, int minvol = 1)
		{
			rch = new RaycastHit ();

			if (VFVoxelTerrain.self == null)
				return false;

			IVxDataSource ds = VFVoxelTerrain.self.Voxels;

			// Define a small number.
			float epsilon = 0.0001F;
			// Define a large number.
			//float upsilon = 10000F;

			// ray origin inside a voxel
			if ( ds.SafeRead(Mathf.FloorToInt(ray.origin.x), 
			                 Mathf.FloorToInt(ray.origin.y),
			                 Mathf.FloorToInt(ray.origin.z)).Volume >= minvol)
			{
				return false;
			}

			// Start and adder.
			float xStart, yStart, zStart;
			float xAdder, yAdder, zAdder;
			float xMin, yMin, zMin;
			float xMax, yMax, zMax;

			xAdder = ray.direction.x > epsilon ? 1 : (ray.direction.x < -epsilon ? -1 : 0);
			yAdder = ray.direction.x > epsilon ? 1 : (ray.direction.x < -epsilon ? -1 : 0);
			zAdder = ray.direction.x > epsilon ? 1 : (ray.direction.x < -epsilon ? -1 : 0);

			Bounds bound = VFVoxelTerrain.self.LodMan._Lod0ViewBounds;
			xMin = bound.min.x;
			yMin = bound.min.y;
			zMin = bound.min.z;
			xMax = bound.max.x;
			yMax = bound.max.y;
			zMax = bound.max.z;

			xStart = (int)(((int) ray.origin.x) + 1 + xAdder*0.5f);
			yStart = (int)(((int) ray.origin.y) + 1 + yAdder*0.5f);
			zStart = (int)(((int) ray.origin.z) + 1 + zAdder*0.5f);

			// Clamp the start	
			xStart = Mathf.Clamp(xStart, xMin, xMax);
			yStart = Mathf.Clamp(yStart, yMin, yMax);
			zStart = Mathf.Clamp(zStart, zMin, zMax);

			// Normalize the ray
			ray.direction = ray.direction.normalized;

			// Define the Enter
//			float EnterX = upsilon + 1;
//			float EnterY = upsilon + 1;
//			float EnterZ = upsilon + 1;

			// Find the X-cast Enter
			if ( xAdder != 0 )
				for ( float x = xStart; x > xMin-0.1f && x <= xMax + 0.1f; x += xAdder )
			{
				float enter = (x - ray.origin.x) / ray.direction.x;
				Vector3 Hit = ray.origin + ray.direction * enter;

				if (ds.Read(Mathf.FloorToInt(Hit.x + xAdder * 0.5f),  Mathf.FloorToInt(Hit.y),  Mathf.FloorToInt(Hit.z)).Volume >= minvol)
				{
					//EnterX = enter;
					break;
				}
			}

			return false;
		}

        public static bool IsNumeral(string tmp) 
        {
            try
            {
                int.Parse(tmp);
                return true;
            }
            catch
            {
                return false;
            }
        }
	}
}
