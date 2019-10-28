using UnityEngine;
using System.Collections;

public class VCEAlterVoxel : VCEModify
{
	public int m_Pos;
	public VCVoxel m_Old;
	public VCVoxel m_New;

	public VCEAlterVoxel ( int position, VCVoxel old_voxel, VCVoxel new_voxel )
	{
		m_Pos = position;
		m_Old = old_voxel;
		m_New = new_voxel;
	}
	
	public override void Undo ()
	{
		VCEditor.s_Scene.m_IsoData.SetVoxel(m_Pos, m_Old);
		VCEditor.s_Scene.m_MeshComputer.AlterVoxel(m_Pos, m_Old);
	}
	public override void Redo ()
	{
		VCEditor.s_Scene.m_IsoData.SetVoxel(m_Pos, m_New);
		VCEditor.s_Scene.m_MeshComputer.AlterVoxel(m_Pos, m_New);
	}
}
