using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BSBrush : GLBehaviour 
{

	public IBSDataSource dataSource;

	public BSPattern	pattern;

	public int minvol = BSMath.MC_ISO_VALUE;

	public EBSBrushMode mode = EBSBrushMode.Add;

	public virtual Bounds brushBound
	{
		get { return new Bounds(); }
	}

	protected abstract void Do ();

	public virtual void Cancel () {} 

	public byte materialType;


	public static T Create<T> (string res_path, Transform parent) where T : BSBrush
	{
		GameObject go_res = Resources.Load(res_path) as GameObject;
		if (go_res == null)
			return null;

		T brush_res = go_res.GetComponent<T>();
		if (brush_res == null)
			return null;

		T brush = Object.Instantiate(brush_res) as T;
		if (brush == null)
			return null;

		brush.gameObject.name = go_res.name;
		brush.transform.parent = parent;
		brush.transform.localPosition = Vector3.zero;
		brush.transform.localRotation = Quaternion.identity;
		brush.transform.localScale = Vector3.one;

		return brush;
	}

	public static T Create<T> (GameObject prefab, Transform parent) where T : BSBrush
	{
		if (prefab == null)
			return null;

		T brush_res = prefab.GetComponent<T>();
		if (brush_res == null)
			return null;

		T brush = Object.Instantiate(brush_res) as T;
		if (brush == null)
			return null;

		brush.gameObject.name = prefab.name;
		brush.transform.parent = parent;
		brush.transform.localPosition = Vector3.zero;
		brush.transform.localRotation = Quaternion.identity;
		brush.transform.localScale = Vector3.one;
		
		return brush;
	}


	public override void OnGL()
	{

	}

	#region Help_Func

	protected static Vector3 CalcCursor (BSMath.DrawTarget target, IBSDataSource ds, int size)
	{
		Vector3 cursor = Vector3.zero;
		IntVector3 realPos = new IntVector3( Mathf.FloorToInt(target.cursor.x * ds.ScaleInverted), 
		                                    Mathf.FloorToInt( target.cursor.y  * ds.ScaleInverted), 
		                                    Mathf.FloorToInt( target.cursor.z * ds.ScaleInverted));
		
		float offset = Mathf.FloorToInt(size * 0.5f) * ds.Scale;
		
		
		cursor = realPos.ToVector3() * ds.Scale - new Vector3(offset, offset, offset);
		int sign = size % 2 == 0? 1 : 0; 
		
		if (offset != 0)
		{
			Vector3 offset_v = Vector3.zero;
			
			if (target.rch.normal.x > 0)
				offset_v.x += offset;
			else if (target.rch.normal.x < 0)
				offset_v.x -= (offset - ds.Scale * sign);
			else
				offset_v.x = 0;
			
			if (target.rch.normal.y > 0)
				offset_v.y += offset;
			else if (target.rch.normal.y < 0)
				offset_v.y -= (offset - ds.Scale * sign);
			
			else
				offset_v.y = 0;
			
			if (target.rch.normal.z > 0)
				offset_v.z += offset;
			else if (target.rch.normal.z < 0)
				offset_v.z -=  (offset - ds.Scale * sign);
			else
				offset_v.z = 0;
			
			cursor += offset_v;
		}
		
		return cursor;
	}

	protected static Vector3 CalcSnapto (BSMath.DrawTarget target, IBSDataSource ds, BSPattern pt)
	{
		Vector3 snapto = Vector3.zero;
		// Update gizmo Position
		IntVector3 realPos = new IntVector3( Mathf.FloorToInt(target.snapto.x * ds.ScaleInverted), 
		                                    Mathf.FloorToInt( target.snapto.y  * ds.ScaleInverted), 
		                                    Mathf.FloorToInt( target.snapto.z * ds.ScaleInverted));
		
		float offset = Mathf.FloorToInt(pt.size * 0.5f) * ds.Scale;
		
		snapto = realPos.ToVector3() * ds.Scale - new Vector3(offset, offset, offset);
		int sign = pt.size % 2 == 0? 1 : 0; 
		
		if (offset != 0)
		{
			Vector3 offset_v = Vector3.zero;
			
			if (target.rch.normal.x > 0)
				offset_v.x -= (offset - ds.Scale * sign);
			else if (target.rch.normal.x < 0)
				offset_v.x += offset;
			else
				offset_v.x = 0;
			
			if (target.rch.normal.y > 0)
				offset_v.y -= (offset - ds.Scale * sign);
			else if (target.rch.normal.y < 0)
				offset_v.y += offset;
			
			else
				offset_v.y = 0;
			
			if (target.rch.normal.z > 0)
				offset_v.z -= (offset - ds.Scale * sign);
			else if (target.rch.normal.z < 0)
				offset_v.z +=  offset;
			else
				offset_v.z = 0;
			
			snapto += offset_v;
		}

		return snapto;

	}

	protected static void FindExtraExtendableVoxels (IBSDataSource ds, List<BSVoxel> new_voxels, List<BSVoxel> old_voxels, List<IntVector3> indexes, Dictionary<IntVector3, int> refMap)
	{
		List<BSVoxel> ext_new_voxels = new List<BSVoxel>();
		List<IntVector3> ext_indexes = new List<IntVector3>();
		List<BSVoxel> ext_old_voxels = new List<BSVoxel>();

		for (int i = 0; i < indexes.Count; i++)
		{
			List<IntVector4> ext_posList = null;
			List<BSVoxel> ext_voxels = null;
			
			if (ds.ReadExtendableBlock (new IntVector4(indexes[i], 0), out ext_posList, out ext_voxels))
			{
				for (int j = 0; j < ext_voxels.Count; j++)
				{
					IntVector3 _ipos = new IntVector3(ext_posList[j].x, ext_posList[j].y, ext_posList[j].z);
					
					
					if (!refMap.ContainsKey(_ipos))
					{
						BSVoxel v = ds.Read(_ipos.x, _ipos.y, _ipos.z);
						ext_old_voxels.Add(v);
						ext_indexes.Add(_ipos);
						ext_new_voxels.Add(new BSVoxel());
					}
					
				}
			}
		}

		indexes.AddRange(ext_indexes);
		new_voxels.AddRange(ext_new_voxels);
		old_voxels.AddRange(ext_old_voxels);
	}

	#endregion
}
