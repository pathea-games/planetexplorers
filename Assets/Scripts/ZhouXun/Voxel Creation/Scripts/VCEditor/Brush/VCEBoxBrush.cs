using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEBoxBrush : VCEFreeSizeBrush
{
	protected override string BrushDesc ()
	{
		return "Box Brush - Draw free size voxel cube";
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
		for ( int x = min.x; x <= max.x; ++x )
		{
			for ( int y = min.y; y <= max.y; ++y )
			{
				for ( int z = min.z; z <= max.z; ++z )
				{
					// Mirror
					if ( VCEditor.s_Mirror.Enabled_Masked )
					{
						VCEditor.s_Mirror.MirrorVoxel(new IntVector3(x,y,z));
						for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
						{
							if ( VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]) )
							{
								int voxel_pos = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
								VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
								VCVoxel new_voxel = new VCVoxel(255, (byte)VCEditor.SelectedVoxelType);
								if ( old_voxel != new_voxel )
								{
									VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
									m_Action.Modifies.Add(modify);
								}
							}
						}
					}
					// No mirror
					else
					{
						int voxel_pos = VCIsoData.IPosToKey(new IntVector3(x,y,z));
						VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
						VCVoxel new_voxel = new VCVoxel(255, (byte)VCEditor.SelectedVoxelType);
						if ( old_voxel != new_voxel )
						{
							VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
							m_Action.Modifies.Add(modify);
						}
					}
				}
			}
		}
		if ( m_Action.Modifies.Count > 0 )
			m_Action.Do();
		ResetDrawing();
	}
}
