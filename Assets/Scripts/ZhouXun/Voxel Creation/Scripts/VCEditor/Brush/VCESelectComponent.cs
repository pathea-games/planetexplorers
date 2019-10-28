using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WhiteCat;

public class VCESelectComponent : VCESelect
{
	[Serializable]
	public class SelectInfo
	{
		public VCEComponentTool m_Component = null;
		public Vector3 m_OldPosition = Vector3.zero;
		public Vector3 m_OldRotation = Vector3.zero;
		public Vector3 m_OldScale = Vector3.one;
		public Vector3 m_DragPosition = Vector3.zero;
		public Vector3 m_DragRotation = Vector3.zero;
		public Vector3 m_DragScale = Vector3.one;
		public Vector3 m_NewPosition = Vector3.zero;
		public Vector3 m_NewRotation = Vector3.zero;
		public Vector3 m_NewScale = Vector3.one;
	}

	public GameObject m_DataInspector;
	
	public List<SelectInfo> m_Selection;
	public SelectInfo FindSelected (VCEComponentTool ct)
	{
		if ( ct == null )
			return null;
		foreach ( SelectInfo si in m_Selection )
		{
			if ( si.m_Component == ct )
				return si;
		}
		return null;
	}

	public static VCEComponentTool s_LastCreate = null;

	public VCEHoloBoard m_DescBoard;
	
	// Transform Gizmo VARS
	public VCEMovingGizmo m_MovingGizmo;
	public VCERotatingGizmo m_RotatingGizmo;
	public bool m_MouseOnGizmo = false;
	public bool UsingGizmo { get { return m_MovingGizmo.Dragging || m_RotatingGizmo.Dragging; } }
	public VCEAction m_GizmoAction = null;
	
	void Start ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			m_Selection = new List<SelectInfo> ();
			CreateMainInspector();
		}
		else
		{
			GameObject.Destroy(this.gameObject);
		}

		// Gizmos events
		m_MovingGizmo.OnDragBegin = OnGizmoBegin;
		m_MovingGizmo.OnDrop = OnGizmoEnd;
		m_MovingGizmo.OnMoving = OnMoving;
		m_RotatingGizmo.OnDragBegin = OnGizmoBegin;
		m_RotatingGizmo.OnDrop = OnGizmoEnd;
		m_RotatingGizmo.OnRotating = OnRotating;
	}
	
	void CreateMainInspector ()
	{
		m_MainInspector = GameObject.Instantiate(m_MainInspectorRes) as GameObject;
		m_MainInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_MainInspector.transform.localPosition = new Vector3 (0,-6,0);
		m_MainInspector.transform.localScale = Vector3.one;
		m_MainInspector.SetActive(false);
		VCEUISelectComponentInspector sci = m_MainInspector.GetComponent<VCEUISelectComponentInspector>();
		sci.m_SelectBrush = this;
	}
	void ShowMainInspector ()
	{
		m_MainInspector.SetActive(true);
	}
	void ShowDataInspector (string respath, VCComponentData setdata)
	{
		HideDataInspector();
		m_DataInspector = GameObject.Instantiate(Resources.Load(respath) as GameObject) as GameObject;
		m_DataInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_DataInspector.transform.localPosition = new Vector3 (0,-30,-1f);
		m_DataInspector.transform.localScale = Vector3.one;
		m_DataInspector.SetActive(true);
		VCEUIComponentInspector cins = m_DataInspector.GetComponent<VCEUIComponentInspector>();
		cins.m_SelectBrush = this;
		cins.Set(setdata);

		// Bone Panel
		ShowBonePanel();
    }

	void ShowBonePanel()
	{
		if (m_Selection != null && m_Selection.Count == 1)
		{
			var part = m_Selection[0].m_Component.GetComponent<VCPArmorPivot>();
			if (part != null)
			{
				VCEditor.Instance.m_UI.bonePanel.Show(part);
			}
        }
	}

	void HideBonePanel()
	{
		if (VCEditor.Instance && VCEditor.Instance.m_UI && VCEditor.Instance.m_UI.bonePanel)
			VCEditor.Instance.m_UI.bonePanel.Hide();
	}

	void HideDataInspector ()
	{
		if ( m_DataInspector != null )
		{
			// Bone Panel
			HideBonePanel();

			GameObject.Destroy(m_DataInspector);
			m_DataInspector = null;
        }
	}

	void HideInspectors ()
	{
		m_MainInspector.SetActive(false);
		HideDataInspector();
    }
	
	void OnDisable ()
	{
		ClearSelection();
		ToggleComponentColliders(false);
		VCEditor.s_ProtectLock0 = false;
	}
	void OnDestroy ()
	{
		ClearSelection();
		ToggleComponentColliders(false);
		if ( m_MainInspector != null )
		{
			GameObject.Destroy(m_MainInspector);
			m_MainInspector = null;
		}
		HideDataInspector();
		s_LastCreate = null;
		VCEditor.s_ProtectLock0 = false;
	}
	
	void ToggleComponentColliders(bool _enabled)
	{
		if ( !VCEditor.DocumentOpen() )
			return;
		foreach ( VCComponentData cd in VCEditor.s_Scene.m_IsoData.m_Components )
		{
			VCEComponentTool c = cd.m_Entity.GetComponent<VCEComponentTool>();
			c.m_SelBound.enabled = _enabled;
			c.m_SelBound.GetComponent<Collider>().enabled = _enabled;
		}
	}

	void OnSelectionChange()
	{
		s_LastCreate = null;
		if ( m_Selection.Count == 1 )
		{
			VCComponentData sel_data = m_Selection[0].m_Component.m_Data;
			VCPartData sel_part_data = sel_data as VCPartData;
			VCDecalData sel_decal_data = sel_data as VCDecalData;
			if ( sel_part_data != null )
				ShowDataInspector(VCConfig.s_PartTypes[sel_part_data.m_Type].m_InspectorRes, sel_data);
			else if ( sel_decal_data != null )
				ShowDataInspector("GUI/Prefabs/Inspectors/Components Inspectors/inspector decal image", sel_data);
		}
		if ( m_Selection.Count > 0 )
		{
			VCEStatusBar.ShowText(m_Selection.Count.ToString() + " " + "component(s)".ToLocalizationString() + " " + "selected".ToLocalizationString(), 4);
		}
	}

	public List<VCEComponentTool> MirrorImage ( VCEComponentTool c )
	{
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		// Mirror
		if ( VCEditor.s_Mirror.Enabled_Masked )
		{
			List<VCEComponentTool> list_ret = new List<VCEComponentTool> ();
			VCEditor.s_Mirror.MirrorComponent(c.m_Data);
			for ( int i = 1; i < VCEditor.s_Mirror.OutputCnt; ++i )
			{
				VCComponentData image = VCEditor.s_Mirror.ComponentOutput[i];
				List<VCComponentData> projections = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(image.m_Position, image.m_ComponentId);
				foreach ( VCComponentData iter in projections )
				{
					if ( !VCEMath.IsEqualRotation(iter.m_Rotation, image.m_Rotation) )
						continue;
					if ( iter.m_Scale != image.m_Scale )
						continue;
					if ( iter.m_Visible != image.m_Visible )
						continue;
					if ( image is IVCMultiphaseComponentData && iter is IVCMultiphaseComponentData )
					{
						if ( (image as IVCMultiphaseComponentData).Phase != (iter as IVCMultiphaseComponentData).Phase )
							continue;
					}
					VCEComponentTool imct = iter.m_Entity.GetComponent<VCEComponentTool>();
					if ( imct == null )
						continue;
					if ( imct == c )
						continue;
					list_ret.Add(imct);
				}
			}
			return list_ret;
		}
		// No mirror
		else
		{
			return new List<VCEComponentTool> ();
		}
	}

	void NormalizeSelection ()
	{
		SelectInfo del_si = null;
		bool changed = false;
		do
		{
			foreach ( SelectInfo si in m_Selection )
			{
				VCEComponentTool ct = si.m_Component;
				List<VCEComponentTool> images = MirrorImage(ct);
				foreach ( VCEComponentTool image in images )
				{
					del_si = m_Selection.Find(iter => iter.m_Component == image);
					if ( del_si != null )
					{
						changed = true;
						goto DEL;
					}
				}
			}
	DEL:
			m_Selection.Remove(del_si);
		}
		while (del_si != null);

		List<SelectInfo> del_list = new List<SelectInfo> ();
		foreach ( SelectInfo si in m_Selection )
		{
			if ( si.m_Component == null )
			{
				del_list.Add(si);
				changed = true;
			}
		}
		foreach ( SelectInfo si in del_list )
		{
			m_Selection.Remove(si);
		}
		if ( changed )
			OnSelectionChange();
	}

	private int pick_order = 0;
	private Vector3 lastMousePos = Vector3.zero;
	void Update ()
	{
		if ( !VCEditor.DocumentOpen() )
			return;

		// Check 's_LastCreate'
		if ( s_LastCreate != null )
		{
			ClearSelection();
			SelectInfo si = new SelectInfo ();
			si.m_Component = s_LastCreate;
			m_Selection.Add(si);
			OnSelectionChange();
		}

		VCEditor.s_ProtectLock0 = UsingGizmo;
		NormalizeSelection();

		// Update Pick Order
		Vector3 mousePos = Input.mousePosition;
		if ( (mousePos - lastMousePos).magnitude > 3.1f )
		{
			pick_order = 0;
			lastMousePos = mousePos;
		}

		#region PART_DESC
		// Part Description Board
		if ( m_DescBoard != null )
		{
			m_DescBoard.m_Position = Vector3.Lerp(m_DescBoard.m_Position, VCEInput.s_PickRay.GetPoint(1.5f), 0.5f);
			List<VCEComponentTool> pickall = new List<VCEComponentTool> ();
			if ( !UsingGizmo && !VCEInput.s_MouseOnUI )
				pickall = VCEMath.RayPickComponents(VCEInput.s_PickRay);
			List<VCPart> pps = new List<VCPart> ();
			Vector3 v = Vector3.zero;
			if ( pickall.Count > 0 )
			{
				v = pickall[0].transform.localPosition;
			}
			foreach ( VCEComponentTool ct in pickall )
			{
				if ( Vector3.Distance(ct.transform.localPosition, v) > VCEditor.s_Scene.m_Setting.m_VoxelSize * 2 )
					continue;
				VCPart pp = ct.GetComponent<VCPart>();
				if ( pp != null )
					pps.Add(pp);
			}
			if ( pickall.Count > 0 )
			{
				m_DescBoard.FadeIn();
				VCEditor.Instance.m_UI.m_HoloBoardCamera.enabled = true;
			}
			else
			{
				VCEditor.Instance.m_UI.m_HoloBoardCamera.enabled = false;
				m_DescBoard.FadeOut();
			}
			pickall.Clear();
			VCEditor.Instance.m_UI.m_PartDescList.SyncList(pps);
			pps.Clear();
			pickall = null;
			pps = null;
		}
		#endregion

		// Pick Component
		VCEComponentTool picked = null;
		if ( !VCEInput.s_MouseOnUI && !m_MouseOnGizmo )
			picked = VCEMath.RayPickComponent(VCEInput.s_PickRay, pick_order);
		if ( Input.GetMouseButtonDown(0) && !VCEInput.s_MouseOnUI && !m_MouseOnGizmo )
		{
			if ( !VCEInput.s_Shift && !VCEInput.s_Control && !VCEInput.s_Alt )
				m_Selection.Clear();
			if ( picked != null )
			{
				SelectInfo si = FindSelected(picked);
				if ( si != null )
				{
					m_Selection.Remove(si);
				}
				else
				{
					si = new SelectInfo ();
					si.m_Component = picked;
					List<SelectInfo> del_list = new List<SelectInfo> ();
					foreach ( SelectInfo sel in m_Selection )
					{
						VCEComponentTool ct = sel.m_Component;
						List<VCEComponentTool> images = MirrorImage(ct);
						if (images.Contains(si.m_Component))
							del_list.Add(sel);
					}
					m_Selection.Add(si);
					foreach ( SelectInfo del in del_list )
						m_Selection.Remove(del);
				}
				pick_order++;
			}
			OnSelectionChange();
		}
		
		// Update bound color & highlight
		foreach ( VCComponentData cd in VCEditor.s_Scene.m_IsoData.m_Components )
		{
			VCEComponentTool c = cd.m_Entity.GetComponent<VCEComponentTool>();
			c.m_SelBound.enabled = true;
			c.m_SelBound.GetComponent<Collider>().enabled = true;
			c.m_SelBound.m_Highlight = (c == picked);
			c.m_SelBound.m_BoundColor = GLComponentBound.s_Blue;
		}

		// Update selection and mirror's bound color
		if ( m_Selection.Count > 0 )
		{
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			foreach ( SelectInfo si in m_Selection )
			{
				VCEComponentTool c = si.m_Component;
				List<VCEComponentTool> mirrors = MirrorImage(c);
				foreach ( VCEComponentTool m in mirrors )
					m.m_SelBound.m_BoundColor = GLComponentBound.s_Orange;
			}
			foreach ( SelectInfo si in m_Selection )
			{
				VCEComponentTool c = si.m_Component;
				c.m_SelBound.m_BoundColor = GLComponentBound.s_Yellow;
			}
		}

		// Update Inspectors
		if ( m_Selection.Count < 1 )
		{
			HideInspectors();
		}
		else if ( m_Selection.Count == 1 )
		{
			ShowMainInspector();
			
			VCEUISelectComponentInspector sci = m_MainInspector.GetComponent<VCEUISelectComponentInspector>();
			if ( m_Selection[0].m_Component != null )
				sci.m_SelectInfo.text = m_Selection[0].m_Component.gameObject.name;
			else
				sci.m_SelectInfo.text = "< Deleted >";
		}
		else
		{
			ShowMainInspector();
			HideDataInspector();
			VCEUISelectComponentInspector sci = m_MainInspector.GetComponent<VCEUISelectComponentInspector>();
			sci.m_SelectInfo.text = m_Selection.Count.ToString() + " " + "objects".ToLocalizationString() + " " + "selected".ToLocalizationString();
		}

		// Transform Gizmos
		if ( m_Selection.Count > 0 )
		{
			Vector3 avePos = Vector3.zero;
			int rotmask = 7;
			foreach ( SelectInfo si in m_Selection )
			{
				avePos += si.m_Component.transform.position;
				VCPartData part_data = si.m_Component.m_Data as VCPartData;
				if ( part_data != null )
					rotmask &= VCConfig.s_PartTypes[si.m_Component.m_Data.m_Type].m_RotateMask;
			}
			avePos /= ((float)m_Selection.Count);
			m_MovingGizmo.transform.position = avePos;
			m_RotatingGizmo.transform.position = avePos;
			m_MovingGizmo.gameObject.SetActive(VCEditor.TransformType == EVCETransformType.Move);
			m_RotatingGizmo.gameObject.SetActive(VCEditor.TransformType == EVCETransformType.Rotate);
			m_RotatingGizmo.m_AxisMask = rotmask;
		}
		else
		{
			m_MovingGizmo.gameObject.SetActive(false);
			m_RotatingGizmo.gameObject.SetActive(false);
		}
	}

	// Gizmo Functions
	// All Gizmo Begin
	void OnGizmoBegin ()
	{
		m_GizmoAction = new VCEAction ();
		foreach ( SelectInfo si in m_Selection )
		{
			si.m_OldPosition = si.m_DragPosition = si.m_NewPosition = si.m_Component.m_Data.m_Position;
			si.m_OldRotation = si.m_DragRotation = si.m_NewRotation = si.m_Component.m_Data.m_Rotation;
			si.m_OldScale = si.m_DragScale = si.m_NewScale = si.m_Component.m_Data.m_Scale;
		}
	}
	// All Gizmo End
	void OnGizmoEnd ()
	{
		if ( m_GizmoAction != null )
		{
			VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
			foreach ( SelectInfo si in m_Selection )
			{
				bool modified = false;
				if ( !VCEMath.IsEqualVector(si.m_NewPosition, si.m_OldPosition) )
					modified = true;
				if ( !VCEMath.IsEqualRotation(si.m_NewRotation, si.m_OldRotation) )
					modified = true;
				if ( !VCEMath.IsEqualVector(si.m_NewScale, si.m_OldScale) )
					modified = true;
				if ( modified )
				{
					VCEAlterComponentTransform modify = new VCEAlterComponentTransform ( 
					    VCEditor.s_Scene.m_IsoData.GetComponentIndex(si.m_Component.m_Data),
					    si.m_OldPosition, si.m_OldRotation, si.m_OldScale,
					    si.m_NewPosition, si.m_NewRotation, si.m_NewScale );

					m_GizmoAction.Modifies.Add(modify);

					// Mirror
					if ( VCEditor.s_Mirror.Enabled_Masked )
					{
						Vector3[] old_poses = new Vector3[8];
						Vector3[] old_rots = new Vector3[8];
						Vector3[] old_scales = new Vector3[8];
						VCEditor.s_Mirror.MirrorComponent(si.m_Component.m_Data);
						for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
						{
							old_poses[i] = VCEditor.s_Mirror.ComponentOutput[i].m_Position;
							old_rots[i] = VCEditor.s_Mirror.ComponentOutput[i].m_Rotation;
							old_scales[i] = VCEditor.s_Mirror.ComponentOutput[i].m_Scale;
						}
						
						VCComponentData tmpdata = si.m_Component.m_Data.Copy();
						tmpdata.m_Position = si.m_NewPosition;
						tmpdata.m_Rotation = si.m_NewRotation;
						tmpdata.m_Scale = si.m_NewScale;
						VCEditor.s_Mirror.MirrorComponent(tmpdata);
						
						for ( int i = 1; i < VCEditor.s_Mirror.OutputCnt; ++i )
						{
							VCComponentData image = VCEditor.s_Mirror.ComponentOutput[i];
							image.Validate();
							List<VCComponentData> edits = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(old_poses[i], image.m_ComponentId);
							foreach ( VCComponentData iter in edits )
							{
								if ( !VCEMath.IsEqualRotation(iter.m_Rotation, old_rots[i]) )
									continue;
								if ( iter.m_Scale != old_scales[i] )
									continue;
								if ( iter.m_Visible != image.m_Visible )
									continue;
								if ( image is IVCMultiphaseComponentData && iter is IVCMultiphaseComponentData )
								{
									if ( (image as IVCMultiphaseComponentData).Phase != (iter as IVCMultiphaseComponentData).Phase )
										continue;
								}
								if ( m_Selection.Find(it => it.m_Component.m_Data == iter) == null )
								{
									VCEAlterComponentTransform modify_mirror = new VCEAlterComponentTransform ( 
									    VCEditor.s_Scene.m_IsoData.GetComponentIndex(iter),
									    iter.m_Position, iter.m_Rotation, iter.m_Scale,
									    image.m_Position, image.m_Rotation, image.m_Scale );
									
									m_GizmoAction.Modifies.Add(modify_mirror);
								}
							}
						}
					}
					// End 'mirror'
				}	
			}
			if ( m_GizmoAction.Modifies.Count > 0 )
			{
				m_GizmoAction.Do();
				m_GizmoAction = null;
			}
		}
		else
		{
			Debug.LogWarning("Must be some problem here!");
		}
	}
	// Moving Gizmo
	void OnMoving (Vector3 offset)
	{
		float hvs = VCEditor.s_Scene.m_Setting.m_VoxelSize * 0.5f;
		VCEditor.s_Mirror.CalcPrepare(hvs*2);
		foreach ( SelectInfo si in m_Selection )
		{
			si.m_DragPosition += offset;
			si.m_NewPosition.x = Mathf.Round(si.m_DragPosition.x / hvs) * hvs;
			si.m_NewPosition.y = Mathf.Round(si.m_DragPosition.y / hvs) * hvs;
			si.m_NewPosition.z = Mathf.Round(si.m_DragPosition.z / hvs) * hvs;
			Vector3 tmp_pos = si.m_Component.m_Data.m_Position;
			si.m_Component.m_Data.m_Position = si.m_NewPosition;
			si.m_Component.m_Data.Validate();
			si.m_NewPosition = si.m_Component.transform.position = si.m_Component.m_Data.m_Position;
			
			if ( m_DataInspector != null )
				m_DataInspector.GetComponent<VCEUIComponentInspector>().Set(si.m_Component.m_Data);
			
			si.m_Component.m_Data.m_Position = tmp_pos;

			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				Vector3[] old_poses = new Vector3[8];
				VCEditor.s_Mirror.MirrorComponent(si.m_Component.m_Data);
				for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
					old_poses[i] = VCEditor.s_Mirror.ComponentOutput[i].m_Position;

				VCComponentData tmpdata = si.m_Component.m_Data.Copy();
				tmpdata.m_Position = si.m_NewPosition;
				VCEditor.s_Mirror.MirrorComponent(tmpdata);

				for ( int i = 1; i < VCEditor.s_Mirror.OutputCnt; ++i )
				{
					VCComponentData image = VCEditor.s_Mirror.ComponentOutput[i];
					List<VCComponentData> edits = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(old_poses[i], image.m_ComponentId);
					foreach ( VCComponentData iter in edits )
					{
						if ( !VCEMath.IsEqualRotation(iter.m_Rotation, image.m_Rotation) )
							continue;
						if ( iter.m_Scale != image.m_Scale )
							continue;
						if ( iter.m_Visible != image.m_Visible )
							continue;
						if ( image is IVCMultiphaseComponentData && iter is IVCMultiphaseComponentData )
						{
							if ( (image as IVCMultiphaseComponentData).Phase != (iter as IVCMultiphaseComponentData).Phase )
								continue;
						}
						if ( m_Selection.Find(it => it.m_Component.m_Data == iter) == null )
						{
							iter.m_Entity.transform.position = image.m_Position;
						}
					}
				}
			}
			// End 'mirror'
		}
	}

	void OnRotating (Vector3 axis, float angle)
	{
		float hvs = VCEditor.s_Scene.m_Setting.m_VoxelSize * 0.5f;
		VCEditor.s_Mirror.CalcPrepare(hvs*2);
		foreach ( SelectInfo si in m_Selection )
		{
			Vector3 cached_rot = si.m_Component.transform.eulerAngles;
			si.m_Component.transform.eulerAngles = si.m_OldRotation;
			si.m_Component.transform.Rotate(axis, angle, Space.World);
			si.m_DragRotation = si.m_Component.transform.eulerAngles;
			si.m_DragRotation = VCEMath.NormalizeEulerAngle(si.m_DragRotation);
			si.m_NewRotation = si.m_DragRotation;
			si.m_Component.transform.eulerAngles = cached_rot;

			Vector3 tmp_rot = si.m_Component.m_Data.m_Rotation;
			si.m_Component.m_Data.m_Rotation = si.m_NewRotation;
			si.m_Component.m_Data.Validate();
			if ( si.m_Component.m_Data.m_Rotation != si.m_NewRotation )
			{
				si.m_NewRotation = cached_rot;
				si.m_Component.m_Data.m_Rotation = si.m_NewRotation;
				si.m_Component.m_Data.Validate();
			}
			si.m_NewRotation = si.m_Component.transform.eulerAngles = si.m_Component.m_Data.m_Rotation;
			
			if ( m_DataInspector != null )
				m_DataInspector.GetComponent<VCEUIComponentInspector>().Set(si.m_Component.m_Data);
			
			si.m_Component.m_Data.m_Rotation = tmp_rot;
			
			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				Vector3[] old_rots = new Vector3[8];
				VCEditor.s_Mirror.MirrorComponent(si.m_Component.m_Data);
				for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
					old_rots[i] = VCEditor.s_Mirror.ComponentOutput[i].m_Rotation;
				
				VCComponentData tmpdata = si.m_Component.m_Data.Copy();
				tmpdata.m_Rotation = si.m_NewRotation;
				VCEditor.s_Mirror.MirrorComponent(tmpdata);
				
				for ( int i = 1; i < VCEditor.s_Mirror.OutputCnt; ++i )
				{
					VCComponentData image = VCEditor.s_Mirror.ComponentOutput[i];
					List<VCComponentData> edits = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(image.m_Position, image.m_ComponentId);
					foreach ( VCComponentData iter in edits )
					{
						if ( !VCEMath.IsEqualRotation(iter.m_Rotation, old_rots[i]) )
							continue;
						if ( iter.m_Scale != image.m_Scale )
							continue;
						if ( iter.m_Visible != image.m_Visible )
							continue;
						if ( image is IVCMultiphaseComponentData && iter is IVCMultiphaseComponentData )
						{
							if ( (image as IVCMultiphaseComponentData).Phase != (iter as IVCMultiphaseComponentData).Phase )
								continue;
						}
						if ( m_Selection.Find(it => it.m_Component.m_Data == iter) == null )
						{
							iter.m_Entity.transform.eulerAngles = image.m_Rotation;
						}
					}
				}
			}
			// End 'mirror'
		}
	}
	// Deselect
	public override void ClearSelection ()
	{
		m_Selection.Clear();
	}

	// Delete Selected Components
	public void DeleteSelection ()
	{
		if ( UsingGizmo )
			return;
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);

		m_Action = new VCEAction ();
		int del_cnt = 0;
		foreach ( SelectInfo si in m_Selection )
		{
			VCEComponentTool ct = si.m_Component;
			List<VCEComponentTool> images = MirrorImage(ct);
			images.Add(ct);

			foreach (VCEComponentTool image in images)
			{
				int index = VCEditor.s_Scene.m_IsoData.GetComponentIndex(image.m_Data);
				if ( index < 0 ) continue;
				VCEDelComponent modify = new VCEDelComponent(index, image.m_Data);
				modify.Redo();
				m_Action.Modifies.Add(modify);
				del_cnt++;
			}
		}
		if ( m_Action.Modifies.Count > 0 )
		{
			m_Action.Register();
			ClearSelection();
		}
		if ( del_cnt > 0 )
			VCEStatusBar.ShowText(del_cnt.ToString() + " " + "component(s) have been removed".ToLocalizationString(), 4);
		m_MouseOnGizmo = false;
	}
	
	public void ApplyInspectorChange ()
	{
		if ( m_Selection.Count != 1 )
			return;
		if ( m_DataInspector == null )
			return;

		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		m_Action = new VCEAction ();
		VCComponentData old_data = m_Selection[0].m_Component.m_Data;
		VCComponentData new_data = m_DataInspector.GetComponent<VCEUIComponentInspector>().Get();

		// Mirror
		if ( VCEditor.s_Mirror.Enabled_Masked )
		{
			VCComponentData[] old_img_data = new VCComponentData [8];
			VCEditor.s_Mirror.MirrorComponent(old_data);
			for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
				old_img_data[i] = VCEditor.s_Mirror.ComponentOutput[i].Copy();

			VCEditor.s_Mirror.MirrorComponent(new_data);
			for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
			{
				VCComponentData image = VCEditor.s_Mirror.ComponentOutput[i];
				List<VCComponentData> edits = VCEditor.s_Scene.m_IsoData.FindComponentsAtPos(old_img_data[i].m_Position, image.m_ComponentId);
				foreach ( VCComponentData iter in edits )
				{
					if ( i > 0 && m_Selection.Find(it => it.m_Component.m_Data == iter) != null )
						continue;
					if ( !VCEMath.IsEqualRotation(iter.m_Rotation, old_img_data[i].m_Rotation) )
						continue;
					if ( !VCEMath.IsEqualVector(iter.m_Scale, old_img_data[i].m_Scale) )
						continue;
					if ( iter.m_Visible != old_img_data[i].m_Visible )
						continue;
					if ( old_img_data[i] is IVCMultiphaseComponentData && iter is IVCMultiphaseComponentData )
					{
						if ( (old_img_data[i] as IVCMultiphaseComponentData).Phase != (iter as IVCMultiphaseComponentData).Phase )
							continue;
					}
					int index = VCEditor.s_Scene.m_IsoData.GetComponentIndex(iter);
					if ( index < 0 ) continue;
					// Special process for wheels
					if ( i != 0 && image is VCQuadphaseFixedPartData )
					{
						(image as VCQuadphaseFixedPartData).m_Phase = 
							((image as VCQuadphaseFixedPartData).m_Phase & 1) | ((iter as VCQuadphaseFixedPartData).m_Phase & 2);
					}

					VCEAlterComponent modify = new VCEAlterComponent (index, iter, image);
					m_Action.Modifies.Add(modify);
				}
			}
		}
		// No mirror
		else
		{
			int index = VCEditor.s_Scene.m_IsoData.GetComponentIndex(old_data);
			if ( index < 0 ) return;
			VCEAlterComponent modify = new VCEAlterComponent (index, old_data, new_data);
			m_Action.Modifies.Add(modify);
		}
		VCEStatusBar.ShowText("Changes applied".ToLocalizationString(), 2);
		m_Action.Do();
	}

    public VCPArmorPivot GetVCPArmorPivotByIndex(int index)
    {
        return m_Selection.Count >0 && m_Selection.Count >= index +1 ? m_Selection[index].m_Component.GetComponent<VCPArmorPivot>() : null;
    }
}
