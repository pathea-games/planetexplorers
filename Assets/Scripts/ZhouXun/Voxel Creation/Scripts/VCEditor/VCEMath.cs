using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class VCEMath
{
	public const int MC_ISO_VALUE = 128;
	public const float MC_ISO_VALUEF = 127.5f;
	public enum EPickLayer : int
	{
		Part = 1,
		Effect = 2,
		Decal = 4,
		All = 0xffff
	}
	public static Ray TransformRayToIsoCoord(Ray world_ray)
	{
		// Transform the ray to iso coord.
		return new Ray( world_ray.origin / VCEditor.s_Scene.m_Setting.m_VoxelSize, world_ray.direction.normalized );
	}
	// pick a voxel
	public static bool RayCastVoxel (Ray ray, out RaycastHit rch, int minvol = 1)
	{
		rch = new RaycastHit ();
		if ( !VCEditor.DocumentOpen() )
			return false;
		VCIsoData iso = VCEditor.s_Scene.m_IsoData;
		
		// Define a small number.
		float epsilon = 0.0001F;
		// Define a large number.
		float upsilon = 10000F;
		
		ray = TransformRayToIsoCoord(ray);
		
		// ray origin inside a voxel
		if ( iso.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(ray.origin.x), 
			                                  Mathf.FloorToInt(ray.origin.y),
			                                  Mathf.FloorToInt(ray.origin.z))).Volume >= minvol )
		{
			return false;
		}
		
		// Start and adder.
		float xStart, yStart, zStart;
		float xAdder, yAdder, zAdder;
		
		if ( ray.direction.x > epsilon )
			xAdder = 1;
		else if ( ray.direction.x < -epsilon )
			xAdder = -1;
		else
			xAdder = 0;
		
		if ( ray.direction.y > epsilon )
			yAdder = 1;
		else if ( ray.direction.y < -epsilon )
			yAdder = -1;
		else
			yAdder = 0;
		
		if ( ray.direction.z > epsilon )
			zAdder = 1;
		else if ( ray.direction.z < -epsilon )
			zAdder = -1;
		else
			zAdder = 0;
		
		xStart = (int)(((int) ray.origin.x) + 1 + xAdder*0.5f);
		yStart = (int)(((int) ray.origin.y) + 1 + yAdder*0.5f);
		zStart = (int)(((int) ray.origin.z) + 1 + zAdder*0.5f);
		
		// Clamp the start
		xStart = Mathf.Clamp(xStart, 0.0f, VCEditor.s_Scene.m_Setting.m_EditorSize.x);
		yStart = Mathf.Clamp(yStart, 0.0f, VCEditor.s_Scene.m_Setting.m_EditorSize.y);
		zStart = Mathf.Clamp(zStart, 0.0f, VCEditor.s_Scene.m_Setting.m_EditorSize.z);
		
		// Normalize the ray
		ray.direction = ray.direction.normalized;
		
		// Define the Enter
		float EnterX = upsilon + 1;
		float EnterY = upsilon + 1;
		float EnterZ = upsilon + 1;
		
		// Find the X-cast Enter
		if ( xAdder != 0 )
		for ( float x = xStart; x > -0.1f && x <= VCEditor.s_Scene.m_Setting.m_EditorSize.x + 0.1f; x += xAdder )
		{
			float enter = (x - ray.origin.x) / ray.direction.x;
			Vector3 Hit = ray.origin + ray.direction * enter;
			if ( iso.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(Hit.x + xAdder * 0.5f),
				                                  Mathf.FloorToInt(Hit.y), 
				                                  Mathf.FloorToInt(Hit.z))).Volume >= minvol )
			{
				EnterX = enter;
				break;
			}
		}
		
		// Find the Y-cast Enter
		if ( yAdder != 0 )
		for ( float y = yStart; y >= -0.1f && y <= VCEditor.s_Scene.m_Setting.m_EditorSize.y + 0.1f; y += yAdder )
		{
			float enter = (y - ray.origin.y) / ray.direction.y;
			Vector3 Hit = ray.origin + ray.direction * enter;
			if ( iso.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(Hit.x),
				                                  Mathf.FloorToInt(Hit.y + yAdder * 0.5f),
				                                  Mathf.FloorToInt(Hit.z))).Volume >= minvol )
			{
				EnterY = enter;
				break;
			}
		}
		
		// Find the Z-cast Enter
		if ( zAdder != 0 )
		for ( float z = zStart; z >= -0.1f && z <= VCEditor.s_Scene.m_Setting.m_EditorSize.z + 0.1f; z += zAdder )
		{
			float enter = (z - ray.origin.z) / ray.direction.z;
			Vector3 Hit = ray.origin + ray.direction * enter;
			if ( iso.GetVoxel(VCIsoData.IPosToKey(Mathf.FloorToInt(Hit.x),
				                                  Mathf.FloorToInt(Hit.y),
				                                  Mathf.FloorToInt(Hit.z + zAdder * 0.5f))).Volume >= minvol )
			{
				EnterZ = enter;
				break;
			}
		}
		
		// cull negative x,y,z cast result
		if ( EnterX < 0 )
			EnterX = upsilon + 1;
		if ( EnterY < 0 )
			EnterY = upsilon + 1;
		if ( EnterZ < 0 )
			EnterZ = upsilon + 1;
		
		// no cast
		if ( EnterX > upsilon && EnterY > upsilon && EnterZ > upsilon )
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
	
	// cast grid planes
	public static bool RayCastGrid (Ray ray, out RaycastHit rch)
	{
		rch = new RaycastHit ();
		if ( !VCEditor.DocumentOpen() )
			return false;
		if ( Physics.Raycast(ray, out rch, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 5F, VCConfig.s_EditorLayerMask) )
		{
			if ( rch.collider.gameObject.GetComponent<GLGridPlane>() != null )
			{
				rch.point = rch.point / VCEditor.s_Scene.m_Setting.m_VoxelSize;
				rch.distance = rch.distance / VCEditor.s_Scene.m_Setting.m_VoxelSize;
				return true;
			}
			else
			{
				rch = new RaycastHit ();
				return false;
			}
		}
		return false;
	}
	public static bool RayCastCoordPlane (Ray ray, ECoordPlane coordplane, float position, out RaycastHit rch)
	{
		rch = new RaycastHit ();
		if ( !VCEditor.DocumentOpen() )
			return false;
		float epsilon = 0.0001F;
		ray = TransformRayToIsoCoord(ray);
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
	public struct DrawTarget
	{
		public RaycastHit rch;
		public IntVector3 snapto;
		public IntVector3 cursor;
	}
	public static bool RayCastDrawTarget(Ray ray, out DrawTarget target, int minvol, bool voxelbest = false)
	{
		target = new DrawTarget();
		if (!VCEditor.DocumentOpen())
			return false;
		// Cast grid and voxel above minvol
		RaycastHit rch_voxel = new RaycastHit();
		RaycastHit rch_grid = new RaycastHit();
		float dist_voxel = 10000;
		float dist_grid = 10000;
		bool to_cast_grid = true;
		if (VCEMath.RayCastVoxel(VCEInput.s_PickRay, out rch_voxel, minvol))
		{
			dist_voxel = rch_voxel.distance;
			if (voxelbest)
				to_cast_grid = false;
		}

		if (to_cast_grid && VCEMath.RayCastGrid(VCEInput.s_PickRay, out rch_grid))
		{
			if (rch_grid.normal.y > 0)
				dist_grid = rch_grid.distance;
		}

		// cast voxel
		if (dist_voxel < dist_grid)
		{
			target.rch = rch_voxel;
			target.snapto = new IntVector3(Mathf.FloorToInt(rch_voxel.point.x - rch_voxel.normal.x * 0.5f),
											Mathf.FloorToInt(rch_voxel.point.y - rch_voxel.normal.y * 0.5f),
											Mathf.FloorToInt(rch_voxel.point.z - rch_voxel.normal.z * 0.5f));
			target.cursor = new IntVector3(Mathf.FloorToInt(rch_voxel.point.x + rch_voxel.normal.x * 0.5f),
											Mathf.FloorToInt(rch_voxel.point.y + rch_voxel.normal.y * 0.5f),
											Mathf.FloorToInt(rch_voxel.point.z + rch_voxel.normal.z * 0.5f));
			return true;
		}
		// cast grid
		else if (dist_grid < dist_voxel)
		{
			target.rch = rch_grid;
			target.snapto = new IntVector3(Mathf.FloorToInt(rch_grid.point.x - rch_grid.normal.x * 0.5f),
											Mathf.FloorToInt(rch_grid.point.y - rch_grid.normal.y * 0.5f),
											Mathf.FloorToInt(rch_grid.point.z - rch_grid.normal.z * 0.5f));
			target.cursor = new IntVector3(Mathf.FloorToInt(rch_grid.point.x + rch_grid.normal.x * 0.5f),
											Mathf.FloorToInt(rch_grid.point.y + rch_grid.normal.y * 0.5f),
											Mathf.FloorToInt(rch_grid.point.z + rch_grid.normal.z * 0.5f));
			if (VCEditor.s_Scene.m_IsoData.GetVoxel(VCIsoData.IPosToKey(target.cursor)).Volume > VCEMath.MC_ISO_VALUE)
			{
				target.cursor = null;
				return false;
			}
			return true;
		}
		// cast nothing
		else
		{
			target.rch = new RaycastHit();
			target.snapto = null;
			target.cursor = null;
			return false;
		}
	}
	public static bool RayAdjustHeight(Ray ray, Vector3 basepoint, out float height)
	{
		height = basepoint.y;
		if ( !VCEditor.DocumentOpen() )
			return false;
		ray = VCEMath.TransformRayToIsoCoord(ray);
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
	public static List<VCEComponentTool> RayPickComponents(Ray ray)
	{
		if ( !VCEditor.DocumentOpen() )
			return null;
		
		RaycastHit rch_v;
		float voxel_dist = 1000000;
		if ( RayCastVoxel(ray, out rch_v, MC_ISO_VALUE) )
		{
			voxel_dist = rch_v.distance * VCEditor.s_Scene.m_Setting.m_VoxelSize;;
		}
		
		RaycastHit[] rchs = Physics.RaycastAll(ray, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 10F, VCConfig.s_EditorLayerMask);
		RaycastHit temp_rch;
		// Sort by dist
		for ( int i = 0; i < rchs.Length - 1; ++i )
		{
			for ( int j = i + 1; j < rchs.Length; ++j )
			{
				if ( rchs[i].distance > rchs[j].distance )
				{
					temp_rch = rchs[i];
					rchs[i] = rchs[j];
					rchs[j] = temp_rch;
				}
			}
		}
		List<VCEComponentTool> list_comp = new List<VCEComponentTool> ();
		foreach ( RaycastHit rch in rchs )
		{
			if ( rch.distance <= voxel_dist )
			{
				GLComponentBound glcb = rch.collider.gameObject.GetComponent<GLComponentBound>();
				if ( glcb != null )
					list_comp.Add(glcb.m_ParentComponent);
			}
		}
		return list_comp;
	}
	public static VCEComponentTool RayPickComponent(Ray ray, int order = 0)
	{
		if ( !VCEditor.DocumentOpen() )
			return null;
		
		RaycastHit rch_v;
		float voxel_dist = 1000000;
		if ( RayCastVoxel(ray, out rch_v, MC_ISO_VALUE) )
		{
			voxel_dist = rch_v.distance * VCEditor.s_Scene.m_Setting.m_VoxelSize;;
		}
		
		RaycastHit[] rchs = Physics.RaycastAll(ray, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 10F, VCConfig.s_EditorLayerMask);
		RaycastHit temp_rch;
		// Sort by dist
		for ( int i = 0; i < rchs.Length - 1; ++i )
		{
			for ( int j = i + 1; j < rchs.Length; ++j )
			{
				if ( rchs[i].distance > rchs[j].distance )
				{
					temp_rch = rchs[i];
					rchs[i] = rchs[j];
					rchs[j] = temp_rch;
				}
			}
		}
		List<VCEComponentTool> list_comp = new List<VCEComponentTool> ();
		foreach ( RaycastHit rch in rchs )
		{
			if ( rch.distance <= voxel_dist )
			{
				GLComponentBound glcb = rch.collider.gameObject.GetComponent<GLComponentBound>();
				if ( glcb != null )
					list_comp.Add(glcb.m_ParentComponent);
			}
		}
		if ( list_comp.Count == 0 )
			return null;
		else
			return list_comp[order%list_comp.Count];
	}
	
	public static bool RayCastMesh ( Ray ray, out RaycastHit rch, float dist )
	{
		if ( Physics.Raycast(ray, out rch, dist, VCConfig.s_EditorLayerMask) )
		{
			// Has cast point
			if ( VCEditor.Instance.m_MeshMgr.Exist(rch.collider.gameObject) )
			{
				return true;
			}
		}
		return false;
	}
	public static bool RayCastMesh ( Ray ray, out RaycastHit rch )
	{
		return RayCastMesh(ray, out rch, VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 2);
	}
	
	public static List<IntVector3> AffectedChunks ( IntBox box )
	{
		int shift = VoxelTerrainConstants._shift;
		box.xMax++;
		box.yMax++;
		box.zMax++;
		List<IntVector3> retlist = new List<IntVector3> ();
		for ( int x = (box.xMin >> shift); x <= (box.xMax >> shift); ++x )
		{
			for ( int y = (box.yMin >> shift); y <= (box.yMax >> shift); ++y )
			{
				for ( int z = (box.zMin >> shift); z <= (box.zMax >> shift); ++z )
				{
					retlist.Add(new IntVector3(x,y,z));
				}
			}
		}
		return retlist;
	}
	
	public static float BoxFeather(IntVector3 pos, IntVector3 min, IntVector3 max, float feather_dist)
	{
		float dx = 0;
		float dy = 0;
		float dz = 0;
		if ( pos.x < min.x )
			dx = (float)(min.x - pos.x - 0.5f) / feather_dist;
		else if ( pos.x > max.x )
			dx = (float)(pos.x - max.x - 0.5f) / feather_dist;
		if ( pos.y < min.y )
			dy = (float)(min.y - pos.y - 0.5f) / feather_dist;
		else if ( pos.y > max.y )
			dy = (float)(pos.y - max.y - 0.5f) / feather_dist;
		if ( pos.z < min.z )
			dz = (float)(min.z - pos.z - 0.5f) / feather_dist;
		else if ( pos.z > max.z )
			dz = (float)(pos.z - max.z - 0.5f) / feather_dist;
		return 1 - Mathf.Sqrt(dx*dx+dy*dy+dz*dz);
	}

	public static float DetermineVolume(int x, int y, int z, Plane p)
	{
		const float ISOVAL = 127.5f;
		const float MAXVAL = 255.0f;
		Vector3 point = new Vector3 (x+0.5f, y+0.5f, z+0.5f);
		float signx = Mathf.Sign(p.normal.x);
		float signy = Mathf.Sign(p.normal.y);
		float signz = Mathf.Sign(p.normal.z);

		float d = p.GetDistanceToPoint(point);

		if ( d > 0 )
		{
			Ray rx = new Ray (point, Vector3.left * signx);
			Ray ry = new Ray (point, Vector3.down * signy);
			Ray rz = new Ray (point, Vector3.back * signz);

			float enterx = 0f;
			float entery = 0f;
			float enterz = 0f;

			if ( !p.Raycast(rx, out enterx) )
				enterx = 1000F;
			if ( !p.Raycast(ry, out entery) )
				entery = 1000F;
			if ( !p.Raycast(rz, out enterz) )
				enterz = 1000F;

			float enter = Mathf.Min(Mathf.Min(enterx, entery), enterz);
			if ( enter >= 0.5f )
				return 0;
			else
				return (ISOVAL - MAXVAL*enter) / (1.0f - enter);
		}
		else if ( d < 0 )
		{
			Ray rx = new Ray (point, Vector3.right * signx);
			Ray ry = new Ray (point, Vector3.up * signy);
			Ray rz = new Ray (point, Vector3.forward * signz);

			float enterx = 0f;
			float entery = 0f;
			float enterz = 0f;

			if ( !p.Raycast(rx, out enterx) )
				enterx = 1000F;
			if ( !p.Raycast(ry, out entery) )
				entery = 1000F;
			if ( !p.Raycast(rz, out enterz) )
				enterz = 1000F;

			float enter = Mathf.Min(Mathf.Min(enterx, entery), enterz);
			if ( enter >= 0.5f )
				return MAXVAL;
			else
				return ISOVAL / (1.0f - enter);
		}
		else
		{
			return ISOVAL;
		}
	}

	public static float SmoothConstraint(float val, float threshold, float pressure = 1)
	{
		if ( val <= threshold )
			return val;
		if ( pressure < 1 )
			pressure = 1;
		float cf = threshold / pressure;

		return Mathf.Log10(val)*cf - Mathf.Log10(threshold)*cf + threshold;
	}

	public static Vector3 NormalizeEulerAngle(Vector3 eulerAngle)
	{
		eulerAngle.x = eulerAngle.x % 360;
		eulerAngle.y = eulerAngle.y % 360;
		eulerAngle.z = eulerAngle.z % 360;
		if ( eulerAngle.x > 180 )
			eulerAngle.x -= 360;
		if ( eulerAngle.y > 180 )
			eulerAngle.y -= 360;
		if ( eulerAngle.z > 180 )
			eulerAngle.z -= 360;
		if ( eulerAngle.x < -180 )
			eulerAngle.x += 360;
		if ( eulerAngle.y < -180 )
			eulerAngle.y += 360;
		if ( eulerAngle.z < -180 )
			eulerAngle.z += 360;
		return eulerAngle;
	}

	public static bool IsEqualVector(Vector3 vec1, Vector3 vec2)
	{
		return (Vector3.Distance(vec1,vec2) < 0.0002f);
	}
	public static bool IsEqualRotation(Vector3 rot1, Vector3 rot2)
	{
		Quaternion q1 = Quaternion.Euler(rot1);
		Quaternion q2 = Quaternion.Euler(rot2);
		double x1 = q1.x;
		double y1 = q1.y;
		double z1 = q1.z;
		double w1 = q1.w;
		double x2 = q2.x;
		double y2 = q2.y;
		double z2 = q2.z;
		double w2 = q2.w;
		double l1 = System.Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1 + w1 * w1);
		double l2 = System.Math.Sqrt(x2 * x2 + y2 * y2 + z2 * z2 + w2 * w2);
		x1 /= l1; y1 /= l1; z1 /= l1; w1 /= l1;
		x2 /= l2; y2 /= l2; z2 /= l2; w2 /= l2;
		double dot = x1*x2 + y1*y2 + z1*z2 + w1*w2;
		double angle = System.Math.Acos(System.Math.Min (System.Math.Abs (dot), 1.0)) * 2.0 * 57.295779513;
		return (angle < 0.009f);
	}
	public static float FadeInCurve( float t )
	{
		t = Mathf.Clamp01(t);
		return Mathf.Clamp01(t*t*t);
	}
}
