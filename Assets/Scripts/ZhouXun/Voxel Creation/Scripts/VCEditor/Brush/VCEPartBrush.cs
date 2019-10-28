using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VCEPartBrush : VCEBrush
{
	[NonSerialized] public GameObject m_PartInst;
	[NonSerialized] public VCEComponentTool m_Tool;
	
	private VCEMath.DrawTarget m_Target;
	
	private VCPartInfo m_LastSelectedPart = null;
	void Update()
	{
		if ( VCEditor.SelectedPart == null )
			return;
		if ( VCEInput.s_Cancel )
		{
			Cancel();
			return;
		}
		
		if ( VCEditor.SelectedPart != m_LastSelectedPart )
		{
			if ( m_PartInst != null )
			{
				GameObject.Destroy(m_PartInst);
			}
			if ( VCEditor.SelectedPart.m_ResObj == null )
			{
				Debug.LogError("This part has no prefab resource: " + VCEditor.SelectedPart.m_Name);
				Cancel();
				return;
			}
			m_PartInst = GameObject.Instantiate(VCEditor.SelectedPart.m_ResObj) as GameObject;
			m_PartInst.SetActive(false);
			m_PartInst.transform.parent = this.transform;
			m_PartInst.transform.localPosition = Vector3.zero;
			m_Tool = m_PartInst.GetComponent<VCEComponentTool>();
			m_Tool.m_IsBrush = true;
			m_Tool.m_InEditor = true;
			m_Tool.m_ToolGroup.SetActive(true);
			
			Collider[] cs = m_PartInst.GetComponentsInChildren<Collider>(true);
			foreach ( Collider c in cs )
			{
				if ( c.gameObject != m_Tool.m_ToolGroup.gameObject )
					Collider.Destroy(c);
				else
					c.enabled = false;
			}

			m_LastSelectedPart = VCEditor.SelectedPart;
		}
		
		float voxel_size = VCEditor.s_Scene.m_Setting.m_VoxelSize;
        if (!VCEInput.s_MouseOnUI && VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, VCEMath.MC_ISO_VALUE) && VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.cursor))
		{
			m_PartInst.SetActive(true);
			Vector3 point = m_Target.rch.point * 2;
			IntVector3 ipoint = new IntVector3 (point);
			m_Tool.SetPivotPos(ipoint.ToVector3()*0.5f*voxel_size);
			
			bool canput = true;
			if ( VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(m_PartInst.transform.localPosition).Count > 0 )
				canput = false;
			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
				if ( m_PartInst.transform.localPosition.x == VCEditor.s_Mirror.WorldPos.x && VCEditor.s_Mirror.XPlane_Masked )
					canput = false;
				if ( m_PartInst.transform.localPosition.y == VCEditor.s_Mirror.WorldPos.y && VCEditor.s_Mirror.YPlane_Masked )
					canput = false;
				if ( m_PartInst.transform.localPosition.z == VCEditor.s_Mirror.WorldPos.z && VCEditor.s_Mirror.ZPlane_Masked )
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
			m_PartInst.SetActive(false);
		}
	}
	
	protected override void Do ()
	{
		m_Action = new VCEAction ();
		
		VCComponentData data = VCComponentData.Create(VCEditor.SelectedPart);
		if ( data != null )
		{
			data.m_ComponentId = VCEditor.SelectedPart.m_ID;
			data.m_Type = VCEditor.SelectedPart.m_Type;
			data.m_Position = m_PartInst.transform.localPosition;
			data.m_Rotation = Vector3.zero;
			data.m_Scale = Vector3.one;
			data.m_Visible = true;
			data.m_CurrIso = VCEditor.s_Scene.m_IsoData;
			data.Validate();

			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				VCEditor.s_Mirror.MirrorComponent(data);

				for ( int i = VCEditor.s_Mirror.OutputCnt - 1; i >= 0; --i )
				{
					VCComponentData image = VCEditor.s_Mirror.ComponentOutput[i];
					if ( VCEditor.s_Scene.m_IsoData.IsComponentIn(image.m_Position) )
					{
						List<VCComponentData> conflicts = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(image.m_Position, image.m_ComponentId);
						foreach ( VCComponentData conflict in conflicts )
						{
							if ( conflict.m_Scale != image.m_Scale )
								continue;
							if ( conflict.m_Visible != image.m_Visible )
								continue;

							if ( image is IVCMultiphaseComponentData && conflict is IVCMultiphaseComponentData )
							{
								if ( (image as IVCMultiphaseComponentData).Phase != (conflict as IVCMultiphaseComponentData).Phase )
									continue;
							}

							// Delete conflict
							int index = VCEditor.s_Scene.m_IsoData.GetComponentIndex(conflict);
							if ( index < 0 ) continue;
							VCEDelComponent del = new VCEDelComponent (index, conflict);
							del.Redo();
							m_Action.Modifies.Add(del);
						}
						VCEAddComponent modify = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, image);
						modify.Redo();
						m_Action.Modifies.Add(modify);
					}
				}
				m_Action.Register();
			}
			// No mirror
			else
			{
                VCFixedHandPartData fixdata = data as VCFixedHandPartData;
                if (fixdata != null) fixdata.m_LeftHand = data.m_Position.x < VCEditor.s_Scene.m_IsoData.m_HeadInfo.xSize * VCEditor.s_Scene.m_Setting.m_VoxelSize * 0.5f;

                 VCEAddComponent modify = new VCEAddComponent(VCEditor.s_Scene.m_IsoData.m_Components.Count, data);
				m_Action.Modifies.Add(modify);
				m_Action.Do();


			}
		}
		else
		{
			Debug.LogWarning("Forget to add this part's implementation ?");
		}
	}
	public override void Cancel ()
	{
		VCEditor.SelectedPart = null;
	}
}
