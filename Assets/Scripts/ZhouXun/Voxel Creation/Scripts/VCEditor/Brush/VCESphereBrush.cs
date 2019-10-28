using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCESphereBrush : VCEFreeSizeBrush
{
	protected override string BrushDesc ()
	{
		return "Sphere Brush - Draw free size voxel sphere or ellipsoid";
	}
	protected override void Do ()
	{
		m_Action = new VCEAction ();
		
		ulong oldmat_guid = VCEditor.s_Scene.m_IsoData.MaterialGUID(VCEditor.SelectedVoxelType);
		ulong newmat_guid = VCEditor.SelectedMaterial.m_Guid;
		if ( oldmat_guid != newmat_guid )
		{
			VCEAlterMaterialMap modify = new VCEAlterMaterialMap (VCEditor.SelectedVoxelType, oldmat_guid, newmat_guid);
			m_Action.Modifies.Add(modify);
		}
		
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		IntVector3 min = Min;
		IntVector3 max = Max;
		Vector3 extend = Size.ToVector3() * 0.5f;
		Vector3 center = min.ToVector3() + extend;
		Vector3 inv_scale = Vector3.one;
		float radius = Mathf.Max(extend.x, extend.y, extend.z);
		float min_dimension = Mathf.Max(Mathf.Min(extend.x, extend.y, extend.z) - 0.5f, 1);
		if ( extend.x >= extend.y && extend.x >= extend.z )
			inv_scale = new Vector3 (1, extend.x/extend.y, extend.x/extend.z);
		else if ( extend.y >= extend.x && extend.y >= extend.z )
			inv_scale = new Vector3 (extend.y/extend.x, 1, extend.y/extend.z);
		else
			inv_scale = new Vector3 (extend.z/extend.x, extend.z/extend.y, 1);
		
		for ( int x = min.x-2; x <= max.x+2; ++x )
		{
			for ( int y = min.y-2; y <= max.y+2; ++y )
			{
				for ( int z = min.z-2; z <= max.z+2; ++z )
				{
					IntVector3 pos = new IntVector3(x,y,z);
					// Mirror
					if ( VCEditor.s_Mirror.Enabled_Masked )
					{
						Vector3 sphere_coord = (pos.ToVector3() + Vector3.one * 0.5f) - center;
						sphere_coord.x *= inv_scale.x;
						sphere_coord.y *= inv_scale.y;
						sphere_coord.z *= inv_scale.z;
						
						float delta = (radius - sphere_coord.magnitude)/radius;
						float volume = Mathf.Clamp(delta * min_dimension * VCEMath.MC_ISO_VALUEF + VCEMath.MC_ISO_VALUEF, 0, 255.49f);

						VCEditor.s_Mirror.MirrorVoxel(pos);

						for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
						{
							pos = VCEditor.s_Mirror.Output[i];
							if ( VCEditor.s_Scene.m_IsoData.IsPointIn(pos) )
							{
								int voxel_pos = VCIsoData.IPosToKey(pos);
								VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
								VCVoxel new_voxel = 0;
								if ( old_voxel.Volume == 0 )
								{
									new_voxel = new VCVoxel ((byte)(volume), (byte)VCEditor.SelectedVoxelType);
								}
								else if ( old_voxel.Volume < VCEMath.MC_ISO_VALUE )
								{
									if ( volume < VCEMath.MC_ISO_VALUE )
										new_voxel = old_voxel;
									else
										new_voxel = new VCVoxel ((byte)(volume), (byte)VCEditor.SelectedVoxelType);
								}
								else
								{
									if ( volume < VCEMath.MC_ISO_VALUE )
										new_voxel = old_voxel;
									else
										new_voxel = new VCVoxel ((byte)(volume), (byte)VCEditor.SelectedVoxelType);
								}
								VCVoxel old_st_voxel = VCEditor.s_Scene.m_Stencil.GetVoxel(voxel_pos);
								new_voxel.Volume = (new_voxel.Volume > old_st_voxel.Volume) ? (new_voxel.Volume) : (old_st_voxel.Volume);
								VCEditor.s_Scene.m_Stencil.SetVoxel(voxel_pos, new_voxel);
							}
						}
					}
					// No mirror
					else
					{
						if ( VCEditor.s_Scene.m_IsoData.IsPointIn(pos) )
						{
							Vector3 sphere_coord = (pos.ToVector3() + Vector3.one * 0.5f) - center;
							sphere_coord.x *= inv_scale.x;
							sphere_coord.y *= inv_scale.y;
							sphere_coord.z *= inv_scale.z;
							
							float delta = (radius - sphere_coord.magnitude)/radius;
							float volume = Mathf.Clamp(delta * min_dimension * VCEMath.MC_ISO_VALUEF + VCEMath.MC_ISO_VALUEF, 0, 255.49f);
							
							int voxel_pos = VCIsoData.IPosToKey(pos);
							VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
							VCVoxel new_voxel = 0;
							if ( old_voxel.Volume == 0 )
							{
								new_voxel = new VCVoxel ((byte)(volume), (byte)VCEditor.SelectedVoxelType);
							}
							else if ( old_voxel.Volume < VCEMath.MC_ISO_VALUE )
							{
								if ( volume < VCEMath.MC_ISO_VALUE )
									new_voxel = old_voxel;
								else
									new_voxel = new VCVoxel ((byte)(volume), (byte)VCEditor.SelectedVoxelType);
							}
							else
							{
								if ( volume < VCEMath.MC_ISO_VALUE )
									new_voxel = old_voxel;
								else
									new_voxel = new VCVoxel ((byte)(volume), (byte)VCEditor.SelectedVoxelType);
							}
							VCEditor.s_Scene.m_Stencil.SetVoxel(voxel_pos, new_voxel);
						}
					}
				}
			}
		}
		VCEditor.s_Scene.m_Stencil.NormalizeAllVoxels();
		foreach ( KeyValuePair<int, VCVoxel> kvp in VCEditor.s_Scene.m_Stencil.m_Voxels )
		{
			VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(kvp.Key);
			VCVoxel new_voxel = VCEditor.s_Scene.m_Stencil.GetVoxel(kvp.Key);
			if ( old_voxel != new_voxel )
			{
				VCEAlterVoxel modify = new VCEAlterVoxel (kvp.Key, old_voxel, new_voxel);
				m_Action.Modifies.Add(modify);
			}
		}
		VCEditor.s_Scene.m_Stencil.Clear();
		if ( m_Action.Modifies.Count > 0 )
			m_Action.Do();
		ResetDrawing();
	}
}
