using UnityEngine;
using System.Collections;

public class VCEPointBrush : VCEBrush
{
	public VCEMath.DrawTarget m_Target;
	public IntVector3 m_Offset = new IntVector3 (0,0,0);
	// Gizmo
	public GameObject m_GizmoGroup;
	public VCEGizmoCubeMesh m_CursorGizmoCube;
	public VCEGizmoCubeMesh m_OffsetGizmoCube;
	
	// Use this for initialization
	void Start ()
	{
		m_Target = new VCEMath.DrawTarget ();
		m_CursorGizmoCube.m_Shrink = -0.03f;
		m_OffsetGizmoCube.m_Shrink = 0.03f;
		VCEStatusBar.ShowText("Pencil - Dot single voxel".ToLocalizationString(), 7, true);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( !VCEditor.DocumentOpen()
		  || VCEditor.SelectedVoxelType < 0
		  || VCEInput.s_MouseOnUI )
		{
			m_GizmoGroup.gameObject.SetActive(false);
			return;
		}
		// Cancel
		if ( VCEInput.s_Cancel )
		{
			Cancel();
		}
		if ( VCEInput.s_Shift && VCEInput.s_Left )
			m_Offset = m_Offset - IntVector3.UnitX;
		if ( VCEInput.s_Shift && VCEInput.s_Right )
			m_Offset = m_Offset + IntVector3.UnitX;
		if ( VCEInput.s_Shift && VCEInput.s_Up )
			m_Offset = m_Offset + IntVector3.UnitY;
		if ( VCEInput.s_Shift && VCEInput.s_Down )
			m_Offset = m_Offset - IntVector3.UnitY;
		if ( VCEInput.s_Shift && VCEInput.s_Forward )
			m_Offset = m_Offset + IntVector3.UnitZ;
		if ( VCEInput.s_Shift && VCEInput.s_Back )
			m_Offset = m_Offset - IntVector3.UnitZ;
		
		float voxel_size = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_CursorGizmoCube.m_VoxelSize = voxel_size;
		m_OffsetGizmoCube.m_VoxelSize = voxel_size;

		if ( VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, VCEMath.MC_ISO_VALUE) )
		{
			m_GizmoGroup.gameObject.SetActive(true);
			IntVector3 draw = m_Target.cursor + m_Offset;
			m_CursorGizmoCube.transform.position = m_Target.cursor.ToVector3() * voxel_size;
			m_OffsetGizmoCube.transform.position = draw.ToVector3() * voxel_size;
			bool cursor_in = VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.cursor);
			bool offset_in = VCEditor.s_Scene.m_IsoData.IsPointIn(draw);
			bool show_offset = ( m_Offset.ToVector3().magnitude > 0.1f );
			m_CursorGizmoCube.gameObject.SetActive(cursor_in && show_offset);
			m_OffsetGizmoCube.gameObject.SetActive(offset_in);
			if ( cursor_in && offset_in && Input.GetMouseButtonDown(0) )
			{
				Do();
			}
		}
		else
		{
			m_GizmoGroup.gameObject.SetActive(false);
		}
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

		IntVector3 draw = m_Target.cursor + m_Offset;
		// Mirror
		if ( VCEditor.s_Mirror.Enabled_Masked )
		{
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			VCEditor.s_Mirror.MirrorVoxel(draw);

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
			int voxel_pos = VCIsoData.IPosToKey(draw);
			VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
			VCVoxel new_voxel = new VCVoxel(255, (byte)VCEditor.SelectedVoxelType);
			if ( old_voxel != new_voxel )
			{
				VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
				m_Action.Modifies.Add(modify);
			}
		}
		if ( m_Action.Modifies.Count > 0 )
			m_Action.Do();
		
		VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>().SetTarget(
			(draw.ToVector3()+Vector3.one*0.5f)*VCEditor.s_Scene.m_Setting.m_VoxelSize);
	}
	
	public override void Cancel ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			VCEditor.Instance.m_UI.m_DefaultBrush_Material.isChecked = true;
		}
	}
}
