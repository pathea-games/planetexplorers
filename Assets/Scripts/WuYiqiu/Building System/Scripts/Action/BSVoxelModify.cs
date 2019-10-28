using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSVoxelModify : BSModify
{

	// Event
	public delegate bool DNotify (int opType, IntVector3[] index, BSVoxel[] voxel, BSVoxel[] oldvoxel, EBSBrushMode mode, IBSDataSource dt);
	public static event DNotify  onModifyCheck;
	public static event DNotify onAfterDo;

	IBSDataSource dataSource = null;

	EBSBrushMode m_Mode;
	
	BSVoxel[]	  m_OldVoxels;
	IntVector3[]  m_Index;
	BSVoxel[]     m_NewVoxels;

	Bounds m_Bound;

	public Bounds Bound { get { return m_Bound; } }

	public BSVoxelModify(IntVector3[] index, BSVoxel[] old_voxels,  BSVoxel[] new_voxels, IBSDataSource ds, EBSBrushMode mode)
	{
		m_OldVoxels = old_voxels;
		m_Index = index;
		m_NewVoxels = new_voxels;

		dataSource = ds;
		m_Mode = mode;

		// Calculate bound
		if (index.Length > 0)
		{
			Vector3 min = new Vector3(index[0].x * dataSource.Scale, index[0].y * dataSource.Scale, index[0].z * dataSource.Scale);
			Vector3 max = min;
			m_Bound = new Bounds();
			foreach (IntVector3 ipos in index)
			{
				Vector3 pos = new Vector3(ipos.x * dataSource.Scale, ipos.y * dataSource.Scale, ipos.z * dataSource.Scale);
				if (min.x > pos.x)
					min.x = pos.x;
				else if (max.x < pos.x)
					max.x = pos.x;

				if (min.y > pos.y)
					min.y = pos.y;
				else if (max.y < pos.y)
					max.y = pos.y;

				if (min.z > pos.z)
					min.z = pos.z;
				else if (max.z < pos.z)
					max.z = pos.z;
			}

			m_Bound.min = min;
			m_Bound.max = max;
		}
	}


	public override bool Redo ()
	{
		if (m_Mode == EBSBrushMode.Add)
		{
			if (onModifyCheck == null || onModifyCheck(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource))
			{
				for (int i = 0; i < m_Index.Length; i++)
				{
					dataSource.Write(m_NewVoxels[i], m_Index[i].x, m_Index[i].y, m_Index[i].z);
				}

				if (onAfterDo != null)
					onAfterDo(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource);

				return true;
			}
			else
				return false;
		}
		else if (m_Mode == EBSBrushMode.Subtract)
		{
			if (onModifyCheck == null || onModifyCheck(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource))
			{
				for (int i = 0; i < m_Index.Length; i++)
				{
					dataSource.Write(m_NewVoxels[i], m_Index[i].x, m_Index[i].y, m_Index[i].z);
				}

				if (onAfterDo != null)
					onAfterDo(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource);

				return true;
			}
			else
				return false;
		}

		return true;
	}

	public override bool Undo ()
	{
		if (m_Mode == EBSBrushMode.Add)
		{
			if (onModifyCheck == null || onModifyCheck(0, m_Index, m_OldVoxels, m_NewVoxels, EBSBrushMode.Subtract, dataSource))
			{
				for (int i = 0; i < m_Index.Length; i++)
				{
					dataSource.Write(m_OldVoxels[i], m_Index[i].x, m_Index[i].y, m_Index[i].z);
				}

				if (onAfterDo != null)
					onAfterDo(0, m_Index, m_OldVoxels, m_NewVoxels, m_Mode, dataSource);

				return true;
			}
			else
				return false;
		}
		else if (m_Mode == EBSBrushMode.Subtract)
		{
			if (onModifyCheck == null || onModifyCheck(0, m_Index, m_OldVoxels, m_NewVoxels, EBSBrushMode.Add, dataSource))
			{
				for (int i = 0; i < m_Index.Length; i++)
				{
					dataSource.Write(m_OldVoxels[i], m_Index[i].x, m_Index[i].y, m_Index[i].z);
				}

				if (onAfterDo != null)
					onAfterDo(0, m_Index, m_OldVoxels, m_NewVoxels, m_Mode, dataSource);

				return true;
			}
			else
				return false;
		}

		return false;
	}

	public void Do ()
	{
		Redo();
	}

	public override bool IsNull ()
	{
		Vector3 min = m_Bound.min;
		Vector3 max = m_Bound.max;

		Vector3 lod0_min = dataSource.Lod0Bound.min;
		Vector3 lod0_max = dataSource.Lod0Bound.max;

		if (min.x < lod0_min.x || max.x > lod0_max.x
		    || min.y < lod0_min.y || max.y > lod0_max.y
		    || min.z < lod0_min.z || max.z > lod0_max.z)
			return true;
		 
		return (m_OldVoxels.Length == 0 && m_NewVoxels.Length == 0);
	}
}
