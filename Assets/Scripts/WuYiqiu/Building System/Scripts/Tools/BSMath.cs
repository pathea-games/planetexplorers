using UnityEngine;
using System.Collections;

public static class BSMath 
{
	static Vector3 Extent = new Vector3(64, 32, 64);

	public const int MC_ISO_VALUE = 128;
	public const float MC_ISO_VALUEF = 127.5f;

	public const float Epsilon = 0.0001F;
	public const float Upsilon = 10000F;


	public struct DrawTarget
	{
		public RaycastHit rch;
		public Vector3 snapto;
		public Vector3 cursor;

		public IBSDataSource ds;

		public IntVector3 iSnapto 
		{ 
			get 
			{ 
				return new IntVector3 ( Mathf.FloorToInt(snapto.x), Mathf.FloorToInt(snapto.y), Mathf.FloorToInt(snapto.z));
			}
		}

		public IntVector3 iCursor
		{
			get
			{
				return new IntVector3 ( Mathf.FloorToInt(cursor.x), Mathf.FloorToInt(cursor.y), Mathf.FloorToInt(cursor.z));
			}
		}
	}



	public static Bounds GetSafeBound (IBSDataSource ds)
	{
		Bounds lod0_bound = ds.Lod0Bound;
		
		float x_extent = Mathf.Min(Extent.x, lod0_bound.extents.x);
		float y_extent = Mathf.Min(Extent.y, lod0_bound.extents.y);
		float z_extent = Mathf.Min(Extent.z, lod0_bound.extents.z);
		
		Bounds bound = new Bounds(VFVoxelTerrain.self.LodMan._Lod0ViewBounds.center, new Vector3(x_extent * 2, y_extent * 2, z_extent * 2));
		return bound;
	}

	public static bool RayCastV (Ray ray, IBSDataSource ds, out RaycastHit rch, int minvol = 1)
	{
		ray.origin -= ds.Offset;

		rch = new RaycastHit();

		IntVector3 origin_index = new IntVector3(Mathf.FloorToInt(ray.origin.x * ds.ScaleInverted),
		                                         Mathf.FloorToInt(ray.origin.y * ds.ScaleInverted),
		                                         Mathf.FloorToInt(ray.origin.z * ds.ScaleInverted));

		// ray origin inside a voxel
		BSVoxel voxel = ds.SafeRead(origin_index.x, origin_index.y, origin_index.z);
		if (!ds.VoxelIsZero(voxel, minvol))
		{
			return false;
		}

		float xStart, yStart, zStart;
		float xAdder, yAdder, zAdder;
		float xMin, yMin, zMin;
		float xMax, yMax, zMax;

		if ( ray.direction.x > Epsilon )
			xAdder = ds.Scale;
		else if ( ray.direction.x < -Epsilon )
			xAdder = -ds.Scale;
		else
			xAdder = 0;
		
		if ( ray.direction.y > Epsilon )
			yAdder = ds.Scale;
		else if ( ray.direction.y < -Epsilon )
			yAdder = -ds.Scale;
		else
			yAdder = 0;
		
		if ( ray.direction.z > Epsilon )
			zAdder = ds.Scale;
		else if ( ray.direction.z < -Epsilon )
			zAdder = -ds.Scale;
		else
			zAdder = 0;

		Bounds bound = GetSafeBound(ds);
		
		xMin = bound.min.x;
		yMin = bound.min.y;
		zMin = bound.min.z;
		xMax = bound.max.x;
		yMax = bound.max.y;
		zMax = bound.max.z;
		
		xStart = (int)(((int) ray.origin.x) + ds.Scale + xAdder*0.5f);
		yStart = (int)(((int) ray.origin.y) + ds.Scale + yAdder*0.5f);
		zStart = (int)(((int) ray.origin.z) + ds.Scale + zAdder*0.5f);
		
		// Clamp the start
		xStart = Mathf.Clamp(xStart, xMin, xMax);
		yStart = Mathf.Clamp(yStart, yMin, yMax);
		zStart = Mathf.Clamp(zStart, zMin, zMax);

		// Normalize the ray
		ray.direction = ray.direction.normalized;
		
		// Define the Enter
		float EnterX = Upsilon + 1;
		float EnterY = Upsilon + 1;
		float EnterZ = Upsilon + 1;

		// Find the X-cast Enter
		if ( xAdder != 0 )
			for ( float x = xStart; x > xMin-0.1f && x <= xMax + 0.1f; x += xAdder )
		{
			float enter = (x - ray.origin.x) / ray.direction.x ;
			Vector3 Hit = ray.origin + ray.direction * enter;
			
			// Cast Block? 
			voxel =  ds.Read(Mathf.FloorToInt((Hit.x + xAdder * 0.5f) * ds.ScaleInverted), 
			                  Mathf.FloorToInt(Hit.y * ds.ScaleInverted), 
			                  Mathf.FloorToInt(Hit.z * ds.ScaleInverted));
			if (!ds.VoxelIsZero (voxel, minvol))
			{
				EnterX = enter;
				break;
			}
			
			
		}

		// Find the Y-cast Enter
		if ( yAdder != 0 )
			for ( float y = yStart; y >= yMin -0.1f && y <= yMax + 0.1f; y += yAdder )
		{
			float enter = (y - ray.origin.y) / ray.direction.y;
			Vector3 Hit = ray.origin + ray.direction * enter;
			
			// Cast Block? 
			voxel =  ds.Read(Mathf.FloorToInt(Hit.x * ds.ScaleInverted), 
			                 Mathf.FloorToInt((Hit.y + yAdder * 0.5f) * ds.ScaleInverted), 
			                 Mathf.FloorToInt(Hit.z  * ds.ScaleInverted));
			if (!ds.VoxelIsZero (voxel, minvol))
			{
				EnterY = enter;
				break;
			}
			
		}
		
		// Find the Z-cast Enter
		if ( zAdder != 0 )
			for ( float z = zStart; z >= zMin-0.1f && z <= zMax + 0.1f; z += zAdder )
		{
			float enter = (z  - ray.origin.z) / ray.direction.z;
			Vector3 Hit = (ray.origin + ray.direction * enter);
			
			voxel =  ds.Read(Mathf.FloorToInt(Hit.x * ds.ScaleInverted),
			                 Mathf.FloorToInt(Hit.y * ds.ScaleInverted), 
			                 Mathf.FloorToInt((Hit.z + zAdder * 0.5f) * ds.ScaleInverted));
			if ( !ds.VoxelIsZero (voxel, minvol))
			{
				EnterZ = enter;
				break;
			}
		}

		// cull negative x,y,z cast result
		if ( EnterX < 0 )
			EnterX = Upsilon + 1;
		if ( EnterY < 0 )
			EnterY = Upsilon + 1;
		if ( EnterZ < 0 )
			EnterZ = Upsilon + 1;
		
		// no cast
		if ( EnterX > Upsilon && EnterY > Upsilon && EnterZ > Upsilon )
		{
			return false;			
		}

		// x enter is the smallest
		if ( EnterX < EnterY && EnterX < EnterZ )
		{
			rch.point = ray.GetPoint(EnterX);
			rch.normal = Vector3.left * xAdder;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		// y enter is the smallest
		else if ( EnterY < EnterZ && EnterY < EnterX )
		{
			rch.point = ray.GetPoint(EnterY);
			rch.normal = Vector3.down * yAdder;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		// z enter is the smallest
		else if ( EnterZ < EnterX && EnterZ < EnterY )
		{
			rch.point = ray.GetPoint(EnterZ);
			rch.normal = Vector3.back * zAdder;
			rch.distance = (rch.point - ray.origin).magnitude;
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool RayCastDrawTarget (Ray ray, IBSDataSource ds, out DrawTarget target, int minvol, bool ignoreDiagonal = true, params IBSDataSource[] relative_ds)
	{
		target = new DrawTarget();

		RaycastHit rch;
		float dis = Upsilon;
//		bool cast = false;

		if (ds == null)
			return false;

		IBSDataSource fds = null;
		if (RayCastV(ray, ds, out rch, minvol))
		{
			dis = rch.distance;
//			cast = true;
			fds = ds;
		}


		foreach (IBSDataSource _ds in relative_ds)
		{
			RaycastHit _rch;
			if (_ds != ds && RayCastV(ray, _ds, out _rch, minvol))
			{
				if (dis > _rch.distance)
				{
					rch = _rch;
					dis = _rch.distance;
					fds = _ds;
				}
			}
		}

		target.ds = fds;
		if (fds != null)
		{
			if (fds == ds)
			{
				target.rch = rch;
				target.snapto = new Vector3 (Mathf.FloorToInt((rch.point.x - rch.normal.x * 0.5f) * ds.ScaleInverted) * ds.Scale, 
				                             Mathf.FloorToInt((rch.point.y - rch.normal.y * 0.5f) * ds.ScaleInverted) * ds.Scale , 
				                             Mathf.FloorToInt((rch.point.z - rch.normal.z * 0.5f) * ds.ScaleInverted) * ds.Scale);
				target.cursor = new Vector3 (Mathf.FloorToInt((rch.point.x + rch.normal.x * 0.5f) * ds.ScaleInverted) * ds.Scale, 
				                             Mathf.FloorToInt((rch.point.y + rch.normal.y * 0.5f) * ds.ScaleInverted) * ds.Scale, 
				                             Mathf.FloorToInt((rch.point.z + rch.normal.z * 0.5f) * ds.ScaleInverted) * ds.Scale);


				IntVector3 offset = DiagonalOffset(rch.point * ds.ScaleInverted, rch.normal * ds.ScaleInverted, ds.DiagonalOffset);

				if (offset.x != 0)
				{
					Vector3 _snap = target.snapto * ds.ScaleInverted+ new Vector3(offset.x, 0, 0);
					BSVoxel voxel = ds.SafeRead((int)_snap.x, (int)_snap.y, (int)_snap.z);
					if (!ds.VoxelIsZero(voxel, minvol))
						offset.x = 0;
				}

				
				if (offset.y != 0)
				{
					Vector3 _snap = target.snapto * ds.ScaleInverted + new Vector3(0, offset.y, 0);
					BSVoxel voxel = ds.SafeRead((int)_snap.x, (int)_snap.y, (int)_snap.z);
					if (!ds.VoxelIsZero(voxel, minvol))
						offset.y = 0;
				}

				
				if (offset.z != 0)
				{
					Vector3 _snap = target.snapto * ds.ScaleInverted + new Vector3(0, 0, offset.z);
					BSVoxel voxel = ds.SafeRead((int)_snap.x, (int)_snap.y, (int)_snap.z);
					if (!ds.VoxelIsZero(voxel, minvol))
						offset.z = 0;
				}

				if (offset.x != 0 && offset.y != 0)
				{
					Vector3 _snap = target.snapto * ds.ScaleInverted + new Vector3(offset.x, offset.y, 0);
					BSVoxel voxel = ds.SafeRead((int)_snap.x, (int)_snap.y, (int)_snap.z);
					if (!ds.VoxelIsZero(voxel, minvol))
					{
						offset.x = 0;
						offset.y = 0;
					}
				}

				if (offset.x != 0 && offset.z != 0)
				{
					Vector3 _snap = target.snapto * ds.ScaleInverted + new Vector3(offset.x, 0, offset.z);
					BSVoxel voxel = ds.SafeRead((int)_snap.x, (int)_snap.y, (int)_snap.z);
					if (!ds.VoxelIsZero(voxel, minvol))
					{
						offset.x = 0;
						offset.z = 0;
					}
				}

				if (offset.y != 0 && offset.z != 0)
				{
					Vector3 _snap = target.snapto * ds.ScaleInverted + new Vector3(0, offset.y, offset.z);
					BSVoxel voxel = ds.SafeRead((int)_snap.x, (int)_snap.y, (int)_snap.z);
					if (!ds.VoxelIsZero(voxel, minvol))
					{
						offset.y = 0;
						offset.z = 0;
					}
				}

				target.cursor = target.cursor + (offset.ToVector3()) * ds.Scale;

				if (ds == BuildingMan.Blocks)
				{
					if (!ignoreDiagonal)
					{
						Vector3 _snap = target.snapto * ds.ScaleInverted;
						BSVoxel voxel = ds.SafeRead((int)_snap.x, (int)_snap.y, (int)_snap.z);
						if (voxel.blockType > 128)
						{
							if ( voxel.blockType >> 2 != 63)
								return false;
						}

					}
				}


			}
			else
			{
				rch.point += fds.Offset;
				rch.point -= ds.Offset;

				target.rch = rch;
				float factor = Mathf.Min( fds.Scale , ds.Scale);
				target.snapto = new Vector3 (Mathf.FloorToInt((rch.point.x - rch.normal.x * 0.5f * factor) * ds.ScaleInverted) * ds.Scale, 
				                             Mathf.FloorToInt((rch.point.y - rch.normal.y * 0.5f * factor) * ds.ScaleInverted) * ds.Scale, 
				                             Mathf.FloorToInt((rch.point.z - rch.normal.z * 0.5f * factor) * ds.ScaleInverted) * ds.Scale);
				target.cursor = new Vector3 (Mathf.FloorToInt((rch.point.x + rch.normal.x * 0.5f * factor) * ds.ScaleInverted) * ds.Scale, 
				                             Mathf.FloorToInt((rch.point.y + rch.normal.y * 0.5f * factor) * ds.ScaleInverted) * ds.Scale, 
				                             Mathf.FloorToInt((rch.point.z + rch.normal.z * 0.5f * factor) * ds.ScaleInverted) * ds.Scale);
			}

			return true;
		}

		return false;
	}

	public static bool RayCastCoordPlane (Ray ray, ECoordPlane coordplane, float position, out RaycastHit rch)
	{
		rch = new RaycastHit ();
		float epsilon = 0.0001F;
		ray.direction.Normalize();
		if ( coordplane == ECoordPlane.XY )
		{
			if ( Mathf.Abs(ray.direction.z) < epsilon ) return false;
			float enter = (position - ray.origin.z) / ray.direction.z;
			if ( enter < 0.01f ) return false;
			rch.point = ray.GetPoint(enter);
			rch.distance = enter;
			rch.normal = ray.direction.z > 0 ? Vector3.back : Vector3.forward;
			return true;
		}
		else if ( coordplane == ECoordPlane.XZ )
		{
			if ( Mathf.Abs(ray.direction.y) < epsilon ) return false;
			float enter = (position - ray.origin.y) / ray.direction.y;
			if ( enter < 0.01f ) return false;
			rch.point = ray.GetPoint(enter);
			rch.distance = enter;
			rch.normal = ray.direction.y > 0 ? Vector3.down : Vector3.up;
			return true;
		}
		else if ( coordplane == ECoordPlane.ZY )
		{
			if ( Mathf.Abs(ray.direction.x) < epsilon ) return false;
			float enter = (position - ray.origin.x) / ray.direction.x;
			if ( enter < 0.01f ) return false;
			rch.point = ray.GetPoint(enter);
			rch.distance = enter;
			rch.normal = ray.direction.x > 0 ? Vector3.left : Vector3.right;
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool RayAdjustHeight(Ray ray, Vector3 basepoint, out float height)
	{
		height = basepoint.y;
		
		if ( Mathf.Abs(ray.direction.x) < 0.001f && Mathf.Abs(ray.direction.z) < 0.001f )
		{
			return false;
		}
		else
		{
			Vector3 horz = Vector3.Cross(Vector3.up, ray.direction).normalized;
			Plane ray_plane = new Plane (ray.origin, ray.GetPoint(10), ray.origin + horz*10);
			Ray vert = new Ray (basepoint + Vector3.up*2000, Vector3.down);
			float enter = 0;
			if ( ray_plane.Raycast(vert, out enter) )
			{
				height = vert.GetPoint(enter).y;
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public static bool RayAdjustHeight (Ray ray, ECoordAxis height_axis, Vector3 basepoint, out float height)
	{
		height = 0;
		if (height_axis == ECoordAxis.X)
		{
			height = basepoint.x;

			if ( Mathf.Abs(ray.direction.y) < 0.001f && Mathf.Abs(ray.direction.z) < 0.001f )
			{
				return false;
			}
			else
			{
				Vector3 horz = Vector3.Cross(Vector3.right, ray.direction).normalized;
				Plane ray_plane = new Plane (ray.origin, ray.GetPoint(10), ray.origin + horz*10);
				Ray vert = new Ray (basepoint + Vector3.right*2000, Vector3.left);
				float enter = 0;
				if ( ray_plane.Raycast(vert, out enter) )
				{
					height = vert.GetPoint(enter).x;
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		else if (height_axis == ECoordAxis.Y)
		{
			height = basepoint.y;
			if ( Mathf.Abs(ray.direction.x) < 0.001f && Mathf.Abs(ray.direction.z) < 0.001f )
			{
				return false;
			}
			else
			{
				Vector3 horz = Vector3.Cross(Vector3.up, ray.direction).normalized;
				Plane ray_plane = new Plane (ray.origin, ray.GetPoint(10), ray.origin + horz*10);
				Ray vert = new Ray (basepoint + Vector3.up*2000, Vector3.down);
				float enter = 0;
				if ( ray_plane.Raycast(vert, out enter) )
				{
					height = vert.GetPoint(enter).y;
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		else if (height_axis == ECoordAxis.Z)
		{
			height = basepoint.z;
			if ( Mathf.Abs(ray.direction.x) < 0.001f && Mathf.Abs(ray.direction.y) < 0.001f )
			{
				return false;
			}
			else
			{
				Vector3 horz = Vector3.Cross(Vector3.forward, ray.direction).normalized;
				Plane ray_plane = new Plane (ray.origin, ray.GetPoint(10), ray.origin + horz*10);
				Ray vert = new Ray (basepoint + Vector3.forward*2000, Vector3.back);
				float enter = 0;
				if ( ray_plane.Raycast(vert, out enter) )
				{
					height = vert.GetPoint(enter).z;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		return false;
	}

	static IntVector3 DiagonalOffset (Vector3 v, Vector3 n, float eps = 0.15f)
	{
		int ix = Mathf.FloorToInt(v.x);
		int iy = Mathf.FloorToInt(v.y);
		int iz = Mathf.FloorToInt(v.z);
		
		float fracx = v.x - ix;
		float fracy = v.y - iy;
		float fracz = v.z - iz;
		
		int rx = 0;
		int ry = 0;
		int rz = 0;
		
		float inv_eps = 1 - eps;
		if (fracx < eps)
			rx--;
		else if (fracx > inv_eps)
			rx++;
		if (fracy < eps)
			ry--;
		else if (fracy > inv_eps)
			ry++;
		if (fracz < eps)
			rz--;
		else if (fracz > inv_eps)
			rz++;
		
		if (n.x != 0)
			rx = 0;
		if (n.y != 0)
			ry = 0;
		if (n.z != 0)
			rz = 0;
		
		return new Vector3(rx,ry,rz);
	}

}


