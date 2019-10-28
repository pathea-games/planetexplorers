using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEDiagonalBrush : VCEFreeSizeBrush
{
	const float ISOVAL = 127.5f;
	const float MAXVAL = 255f;

	private static int m_Direction = 0;
	private static int m_Thickness = 0;
	private static int m_Offset = 0;

	protected override void ExtraAdjust ()
	{
		if ( Input.GetKeyDown(KeyCode.Tab) )
		{
			m_Direction++;
			m_Direction = m_Direction % 12;
		}
		if ( Input.GetKeyDown(KeyCode.UpArrow) )
		{
			m_Offset++;
		}
		if ( Input.GetKeyDown(KeyCode.DownArrow) )
		{
			m_Offset--;
		}
		if ( Input.GetKeyDown(KeyCode.LeftArrow) )
		{
			m_Thickness--;
		}
		if ( Input.GetKeyDown(KeyCode.RightArrow) )
		{
			m_Thickness++;
		}
		Transform shapegizmo = null;
		if ( m_GizmoCube.m_ShapeGizmo != null )
		{
			shapegizmo = m_GizmoCube.m_ShapeGizmo.GetChild(0);
		}
		if ( shapegizmo != null )
		{
			switch ( m_Direction )
			{
			case 0: shapegizmo.transform.localEulerAngles = new Vector3 (0,0,0); break;
			case 1: shapegizmo.transform.localEulerAngles = new Vector3 (180,0,0); break;
			case 2: shapegizmo.transform.localEulerAngles = new Vector3 (90,0,0); break;
			case 3: shapegizmo.transform.localEulerAngles = new Vector3 (270,0,0); break;

			case 4: shapegizmo.transform.localEulerAngles = new Vector3 (0,90,90); break;
			case 5: shapegizmo.transform.localEulerAngles = new Vector3 (0,270,90); break;
			case 6: shapegizmo.transform.localEulerAngles = new Vector3 (0,0,90); break;
			case 7: shapegizmo.transform.localEulerAngles = new Vector3 (0,180,90); break;

			case 8: shapegizmo.transform.localEulerAngles = new Vector3 (0,270,180); break;
			case 9: shapegizmo.transform.localEulerAngles = new Vector3 (0,90,0); break;
			case 10: shapegizmo.transform.localEulerAngles = new Vector3 (90,90,0); break;
			case 11: shapegizmo.transform.localEulerAngles = new Vector3 (270,90,0); break;

			default: shapegizmo.transform.localEulerAngles = new Vector3 (0,0,0); break;
			}
		}
	}

	protected override string BrushDesc ()
	{
		return "Diagonal Brush - Draw free size voxel diagonal";
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

		Vector3 pa = Vector3.zero;
		Vector3 pb = Vector3.zero;
		Vector3 pc = Vector3.zero;
		switch (m_Direction)
		{
		default: 
		case 0: pa = new Vector3(Min.x, Min.y, Min.z-0.5f); pb = new Vector3(Max.x+1, Max.y+1.5f, Max.z+1); pc = new Vector3(Max.x+1, Min.y, Min.z-0.5f); break;
		case 1: pa = new Vector3(Max.x+1, Max.y+1, Max.z+1.5f); pb = new Vector3(Min.x, Min.y-0.5f, Min.z); pc = new Vector3(Max.x+1, Min.y-0.5f, Min.z); break;
		case 2: pa = new Vector3(Min.x, Max.y+1.5f, Min.z); pb = new Vector3(Max.x+1, Min.y, Max.z+1.5f); pc = new Vector3(Max.x+1, Max.y+1.5f, Min.z); break;
		case 3: pa = new Vector3(Max.x+1, Min.y-0.5f, Max.z+1); pb = new Vector3(Min.x, Max.y+1, Min.z-0.5f); pc = new Vector3(Max.x+1, Max.y+1, Min.z-0.5f); break;

		case 4: pa = new Vector3(Min.x-0.5f, Min.y, Min.z); pb = new Vector3(Max.x+1, Max.y+1, Max.z+1.5f); pc = new Vector3(Min.x-0.5f, Max.y+1, Min.z); break;
		case 5: pa = new Vector3(Max.x+1.5f, Max.y+1, Max.z+1); pb = new Vector3(Min.x, Min.y, Min.z-0.5f); pc = new Vector3(Min.x, Max.y+1, Min.z-0.5f); break;
		case 6: pa = new Vector3(Max.x+1, Min.y, Min.z-0.5f); pb = new Vector3(Min.x-0.5f, Max.y+1, Max.z+1); pc = new Vector3(Max.x+1, Max.y+1, Min.z-0.5f); break;
		case 7: pa = new Vector3(Min.x, Max.y+1, Max.z+1.5f); pb = new Vector3(Max.x+1.5f, Min.y, Min.z); pc = new Vector3(Max.x+1.5f, Max.y+1, Min.z); break;

		case 8:  pa = new Vector3(Min.x, Min.y-0.5f, Min.z); pb = new Vector3(Max.x+1.5f, Max.y+1, Max.z+1); pc = new Vector3(Min.x, Min.y-0.5f, Max.z+1); break;
		case 9:  pa = new Vector3(Max.x+1, Max.y+1.5f, Max.z+1); pb = new Vector3(Min.x-0.5f, Min.y, Min.z); pc = new Vector3(Min.x-0.5f, Min.y, Max.z+1); break;
		case 10: pa = new Vector3(Max.x+1.5f, Min.y, Min.z); pb = new Vector3(Min.x, Max.y+1.5f, Max.z+1); pc = new Vector3(Max.x+1.5f, Min.y, Max.z+1); break;
		case 11: pa = new Vector3(Min.x-0.5f, Max.y+1, Max.z+1); pb = new Vector3(Max.x+1, Min.y-0.5f, Min.z); pc = new Vector3(Max.x+1, Min.y-0.5f, Max.z+1); break;
		}

		//Vector3 offset = m_Offset * Vector3.up;
		//Vector3 offset2 = (m_Offset-m_Thickness) * Vector3.up;
		Plane pl = new Plane (pa, pb, pc);
		//Plane pl2 = new Plane (pa+offset2, pc+offset2, pb+offset2);

		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		IntVector3 min = Min;
		IntVector3 max = Max;
		for ( int x = min.x; x <= max.x; ++x )
		{
			for ( int y = min.y; y <= max.y; ++y )
			{
				for ( int z = min.z; z <= max.z; ++z )
				{
					float volume = VCEMath.DetermineVolume(x,y,z,pl);
					//float volume2 = VCEMath.DetermineVolume(x,y,z,pl2);
					int vol = Mathf.RoundToInt(volume);
					if ( vol == 0 )
						continue;
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
								VCVoxel new_voxel = new VCVoxel((byte)vol, (byte)VCEditor.SelectedVoxelType);
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
						VCVoxel new_voxel = new VCVoxel((byte)vol, (byte)VCEditor.SelectedVoxelType);
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

	protected override void ExtraGUI ()
	{
		if ( m_Phase != EPhase.Free )
		{
			string text = "Use          to change direction";
			GUI.color = new Color(1,1,1,0.5f);
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 56, 100,100), text, "CursorText2" );
			GUI.color = new Color(1,1,0,0.9f);
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 56, 100,100), "       TAB", "CursorText2" );
		}
	}
}
