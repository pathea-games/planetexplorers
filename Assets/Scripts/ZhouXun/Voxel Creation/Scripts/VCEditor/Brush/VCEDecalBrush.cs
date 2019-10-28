using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VCEDecalBrush : VCEBrush
{
	public GameObject m_DecalPrefab;
	[NonSerialized] public VCDecalHandler m_DecalInst;
	[NonSerialized] public VCEComponentTool m_Tool;
	private float m_RotateAngle;

	void OnEnable ()
	{
		float voxel_size = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		if ( m_DecalInst != null )
		{
			GameObject.Destroy(m_DecalInst.gameObject);
			m_DecalInst = null;
		}
		m_DecalInst = (GameObject.Instantiate(m_DecalPrefab) as GameObject).GetComponent<VCDecalHandler>();
		m_DecalInst.gameObject.SetActive(false);
		m_DecalInst.transform.parent = this.transform;
		m_DecalInst.transform.localPosition = Vector3.zero;
		m_DecalInst.transform.localScale = Vector3.one;
		m_DecalInst.m_Guid = VCEditor.SelectedDecalGUID;
		m_DecalInst.m_Depth = voxel_size;
		m_DecalInst.m_Size = voxel_size * 10;
		m_DecalInst.m_Mirrored = false;
		m_DecalInst.m_ShaderIndex = 0;
		m_Tool = m_DecalInst.GetComponent<VCEComponentTool>();
		m_Tool.m_IsBrush = true;
		m_Tool.m_InEditor = true;
		m_Tool.m_ToolGroup.SetActive(true);
	}

	void Update()
	{
		if ( VCEditor.SelectedDecalGUID == 0 ) return;
		if ( VCEditor.SelectedDecalIndex < 0 ) return;
		if ( VCEditor.SelectedDecal == null ) return;
		if ( VCEInput.s_Cancel )
		{
			Cancel();
			return;
		}
		m_DecalInst.m_Guid = VCEditor.SelectedDecalGUID;

		float voxel_size = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		RaycastHit rch;
		if ( !VCEInput.s_MouseOnUI && VCEMath.RayCastVoxel(VCEInput.s_PickRay, out rch, VCEMath.MC_ISO_VALUE) )
		{
			AdjustBrush();
			m_DecalInst.gameObject.SetActive(true);
			Vector3 point = rch.point * 2;
			IntVector3 ipoint = new IntVector3 (point);
			m_Tool.SetPivotPos(ipoint.ToVector3()*0.5f*voxel_size);
			m_DecalInst.transform.localRotation = Quaternion.LookRotation(-rch.normal);
			m_DecalInst.transform.Rotate(-rch.normal, m_RotateAngle, Space.World);
			
			bool canput = true;
			List<VCComponentData> poscdatas = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(m_DecalInst.transform.localPosition, VCDecalData.s_ComponentId);
			foreach ( VCComponentData cd in poscdatas )
			{
				VCDecalData ddata = cd as VCDecalData;
				if ( ddata != null )
				{
					if ( ddata.m_Guid == m_DecalInst.m_Guid )
					{
						canput = false;
						break;
					}
				}
			}

			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
				if ( m_DecalInst.transform.localPosition.x == VCEditor.s_Mirror.WorldPos.x && VCEditor.s_Mirror.XPlane_Masked )
					canput = false;
				if ( m_DecalInst.transform.localPosition.y == VCEditor.s_Mirror.WorldPos.y && VCEditor.s_Mirror.YPlane_Masked )
					canput = false;
				if ( m_DecalInst.transform.localPosition.z == VCEditor.s_Mirror.WorldPos.z && VCEditor.s_Mirror.ZPlane_Masked )
					canput = false;
			}
			if ( canput )
			{
				m_Tool.m_SelBound.m_BoundColor = GLComponentBound.s_Green;
				m_Tool.m_SelBound.m_Highlight = false;
			}
			else
			{
				m_Tool.m_SelBound.m_BoundColor = GLComponentBound.s_Red;
				m_Tool.m_SelBound.m_Highlight = true;
			}

			if ( Input.GetMouseButtonDown(0) && canput )
			{
				Do();
			}
		}
		else
		{
			m_DecalInst.gameObject.SetActive(false);
		}
	}

	private void AdjustBrush ()
	{
		float voxel_size = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		if ( VCEInput.s_Increase )
		{
			if ( m_DecalInst.m_Size < voxel_size * 99.5f )
				m_DecalInst.m_Size += voxel_size;
		}
		if ( VCEInput.s_Decrease )
		{
			if ( m_DecalInst.m_Size > voxel_size * 1.5f )
				m_DecalInst.m_Size -= voxel_size;
		}
		if ( Input.GetKeyDown(KeyCode.RightArrow) )
		{
			m_RotateAngle -= 45.0f;
			if ( m_RotateAngle < 0 )
			{
				m_RotateAngle += 360.0f;
			}
		}
		if ( Input.GetKeyDown(KeyCode.LeftArrow) )
		{
			m_RotateAngle += 45.0f;
			if ( m_RotateAngle > 360.0f )
			{
				m_RotateAngle -= 360.0f;
			}
		}
	}

	protected override void Do ()
	{
		if ( VCEditor.SelectedDecalGUID == 0 ) return;
		if ( VCEditor.SelectedDecalIndex < 0 ) return;
		if ( VCEditor.SelectedDecal == null ) return;
		m_Action = new VCEAction ();
		
		VCDecalData dcldata = VCComponentData.CreateDecal() as VCDecalData;
		if ( dcldata != null )
		{
			dcldata.m_ComponentId = VCDecalData.s_ComponentId;
			dcldata.m_Type = EVCComponent.cpDecal;
			dcldata.m_Position = m_DecalInst.transform.localPosition;
			dcldata.m_Rotation = VCEMath.NormalizeEulerAngle(m_DecalInst.transform.localEulerAngles);
			dcldata.m_Scale = Vector3.one;
			dcldata.m_Visible = true;
			dcldata.m_CurrIso = VCEditor.s_Scene.m_IsoData;
			dcldata.m_Guid = VCEditor.SelectedDecalGUID;
			dcldata.m_AssetIndex = VCEditor.SelectedDecalIndex;
			dcldata.m_Size = m_DecalInst.m_Size;
			dcldata.m_Depth = m_DecalInst.m_Depth;
			dcldata.m_Mirrored = m_DecalInst.m_Mirrored;
			dcldata.m_ShaderIndex = m_DecalInst.m_ShaderIndex;
			dcldata.m_Color = m_DecalInst.m_Color;
			dcldata.Validate();

			ulong olddcl_guid = VCEditor.s_Scene.m_IsoData.DecalGUID(VCEditor.SelectedDecalIndex);
			ulong newdcl_guid = VCEditor.SelectedDecalGUID;
			if ( olddcl_guid != newdcl_guid )
			{
				VCEAlterDecalMap modify = new VCEAlterDecalMap (VCEditor.SelectedDecalIndex, olddcl_guid, newdcl_guid);
				modify.Redo();
				m_Action.Modifies.Add(modify);
			}

			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				VCEditor.s_Mirror.MirrorComponent(dcldata);

				for ( int i = VCEditor.s_Mirror.OutputCnt - 1; i >= 0; --i )
				{
					VCComponentData image = VCEditor.s_Mirror.ComponentOutput[i];
					if ( VCEditor.s_Scene.m_IsoData.IsComponentIn(image.m_Position) )
					{
						VCEAddComponent modify = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, image);
						modify.Redo();
						m_Action.Modifies.Add(modify);
					}
				}
			}
			// No mirror
			else
			{
				VCEAddComponent modify = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, dcldata);
				modify.Redo();
				m_Action.Modifies.Add(modify);
			}
			m_Action.Register();
		}
		else
		{
			Debug.LogWarning("Decal data create failed");
		}
	}
	public override void Cancel ()
	{
		VCEditor.SelectedDecal = null;
	}
}
