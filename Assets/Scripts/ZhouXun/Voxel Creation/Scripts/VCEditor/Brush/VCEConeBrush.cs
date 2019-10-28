using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEConeBrush : VCEFreeSizeBrush
{
	public VCEConeGizmoMesh m_ConeGizmo;

	private static ECoordAxis m_Direction = ECoordAxis.Y;
	private static float m_PositiveScale = 0;
	private static float m_NegativeScale = 1;
	protected override string BrushDesc ()
	{
		return "Cone Brush - Draw free size voxel cone";
	}
	protected override void ExtraAdjust ()
	{
		if ( Input.GetKeyDown(KeyCode.Tab) )
		{
			if ( m_Direction == ECoordAxis.Y )
				m_Direction = ECoordAxis.X;
			else if ( m_Direction == ECoordAxis.Z )
				m_Direction = ECoordAxis.Y;
			else if ( m_Direction == ECoordAxis.X )
				m_Direction = ECoordAxis.Z;
			else
				m_Direction = ECoordAxis.Y;
		}
		Transform shapegizmo = null;
		if ( m_GizmoCube.m_ShapeGizmo != null )
		{
			shapegizmo = m_GizmoCube.m_ShapeGizmo.GetChild(0);
		}
		switch ( m_Direction )
		{
		case ECoordAxis.X:
			if ( shapegizmo != null )
				shapegizmo.transform.localEulerAngles = new Vector3 (0,0,-90);
			break;
		case ECoordAxis.Y:
			if ( shapegizmo != null )
				shapegizmo.transform.localEulerAngles = new Vector3 (0,0,0);
			break;
		case ECoordAxis.Z:
			if ( shapegizmo != null )
				shapegizmo.transform.localEulerAngles = new Vector3 (90,0,0);
			break;
		default:
			if ( shapegizmo != null )
				shapegizmo.transform.localEulerAngles = new Vector3 (0,0,0);
			break;
		}
		if ( m_Direction == ECoordAxis.Y && Input.GetKey(KeyCode.UpArrow) 
		    || m_Direction == ECoordAxis.X && Input.GetKey(VCEInput.s_RightKeyCode) 
		    || m_Direction == ECoordAxis.Z && Input.GetKey(VCEInput.s_ForwardKeyCode) )
		{
			m_PositiveScale += 0.05f;
			if ( m_PositiveScale > 1.0f )
			{
				m_NegativeScale -= (m_PositiveScale - 1.0f);
				m_PositiveScale = 1.0f;
			}
			if ( m_NegativeScale < 0.0f )
				m_NegativeScale = 0;
		}
		if ( m_Direction == ECoordAxis.Y && Input.GetKey(KeyCode.DownArrow) 
		    || m_Direction == ECoordAxis.X && Input.GetKey(VCEInput.s_LeftKeyCode) 
		    || m_Direction == ECoordAxis.Z && Input.GetKey(VCEInput.s_BackKeyCode) )
		{
			m_NegativeScale += 0.05f;
			if ( m_NegativeScale > 1.0f )
			{
				m_PositiveScale -= (m_NegativeScale - 1.0f);
				m_NegativeScale = 1.0f;
			}
			if ( m_PositiveScale < 0.0f )
				m_PositiveScale = 0;
		}
		m_ConeGizmo.m_NegativeScale = m_NegativeScale;
		m_ConeGizmo.m_PositiveScale = m_PositiveScale;
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
		
		float maskx = (m_Direction == ECoordAxis.X) ? 0.0f : 1.0f;
		float masky = (m_Direction == ECoordAxis.Y) ? 0.0f : 1.0f;
		float maskz = (m_Direction == ECoordAxis.Z) ? 0.0f : 1.0f;
		int imaskx = (m_Direction == ECoordAxis.X) ? 0 : 1;
		int imasky = (m_Direction == ECoordAxis.Y) ? 0 : 1;
		int imaskz = (m_Direction == ECoordAxis.Z) ? 0 : 1;
		float invmaskx = (m_Direction == ECoordAxis.X) ? 1000000.0f : 1.0f;
		float invmasky = (m_Direction == ECoordAxis.Y) ? 1000000.0f : 1.0f;
		float invmaskz = (m_Direction == ECoordAxis.Z) ? 1000000.0f : 1.0f;
		
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		IntVector3 min = Min;
		IntVector3 max = Max;
		Vector3 extend = Size.ToVector3() * 0.5f;
		Vector3 center = min.ToVector3() + extend;
		Vector3 inv_scale = Vector3.one;
		float radius = Mathf.Max(extend.x * maskx, extend.y * masky, extend.z * maskz);
		float min_dimension = Mathf.Max(Mathf.Min(extend.x * invmaskx, extend.y * invmasky, extend.z * invmaskz) - 0.5f, 1);
		if ( extend.x * maskx >= extend.y * masky && extend.x * maskx >= extend.z * maskz )
			inv_scale = new Vector3 (1, extend.x/extend.y, extend.x/extend.z);
		else if ( extend.y * masky >= extend.x * maskx && extend.y * masky >= extend.z * maskz )
			inv_scale = new Vector3 (extend.y/extend.x, 1, extend.y/extend.z);
		else
			inv_scale = new Vector3 (extend.z/extend.x, extend.z/extend.y, 1);

		float sizex = max.x - min.x + 0.5f;
		float sizey = max.y - min.y + 0.5f;
		float sizez = max.z - min.z + 0.5f;
		for ( int x = min.x-2*imaskx; x <= max.x+2*imaskx; ++x )
		{
			for ( int y = min.y-2*imasky; y <= max.y+2*imasky; ++y )
			{
				for ( int z = min.z-2*imaskz; z <= max.z+2*imaskz; ++z )
				{
					float lerpt = 1;
					if ( m_Direction == ECoordAxis.X )
						lerpt = (float)(x-min.x) / sizex;
					else if ( m_Direction == ECoordAxis.Y )
						lerpt = (float)(y-min.y) / sizey;
					else if ( m_Direction == ECoordAxis.Z )
						lerpt = (float)(z-min.z) / sizez;
					float scl = Mathf.Lerp(m_NegativeScale, m_PositiveScale, lerpt);
					if ( scl * radius < 0.01f )
						scl = 0.01f/radius;

					IntVector3 pos = new IntVector3(x,y,z);
					// Mirror
					if ( VCEditor.s_Mirror.Enabled_Masked )
					{
						Vector3 sphere_coord = (pos.ToVector3() + Vector3.one * 0.5f) - center;
						sphere_coord.x *= inv_scale.x;
						sphere_coord.y *= inv_scale.y;
						sphere_coord.z *= inv_scale.z;
						sphere_coord.x *= maskx;
						sphere_coord.y *= masky;
						sphere_coord.z *= maskz;

						float _radius = scl * radius;
						float delta = (_radius - sphere_coord.magnitude)/_radius;
						float volume = Mathf.Clamp(delta * min_dimension * scl * VCEMath.MC_ISO_VALUEF + VCEMath.MC_ISO_VALUEF, 0, 255.49f);
						
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
							sphere_coord.x *= maskx;
							sphere_coord.y *= masky;
							sphere_coord.z *= maskz;
							
							float _radius = scl * radius;
							float delta = (_radius - sphere_coord.magnitude)/_radius;
							float volume = Mathf.Clamp(delta * min_dimension * scl * VCEMath.MC_ISO_VALUEF + VCEMath.MC_ISO_VALUEF, 0, 255.49f);
							
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

	protected override void ExtraGUI ()
	{
		if ( m_Phase != EPhase.Free )
		{
			string text = "Use          to change direction\r\nUse               to change shape";
			GUI.color = new Color(1,1,1,0.5f);
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 56, 100,100), text, "CursorText2" );
			GUI.color = new Color(1,1,0,0.9f);
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 56, 100,100), "       TAB\r\n       Arrows", "CursorText2" );
		}
	}
}
