#define CANNOT_CANCELALLMETHOD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCESelectVoxel : VCESelect
{
	public VCEMath.DrawTarget m_Target;
	public VCEVoxelSelection m_SelectionMgr;
	public VCEUIBoxMethodInspector m_BoxMethodInspectorRes;
	private EVCESelectMethod m_Method;
	public EVCESelectMethod SelectMethod
	{
		get { return m_Method; }
		set
		{
			if ( value == m_Method )
				return;
			if ( m_MethodExec != null )
			{
				MonoBehaviour.Destroy(m_MethodExec);
				m_MethodExec = null;
			}
			m_Method = value;
			switch ( m_Method )
			{
			case EVCESelectMethod.Box:
				m_MethodExec = gameObject.AddComponent<VCESelectMethod_Box>();
				break;
			default:
				m_MethodExec = null;
				break;
			}
			if ( m_MethodExec != null )
				m_MethodExec.Init(this);
		}
	}
	private VCESelectMethod m_MethodExec;

	void Start ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			m_SelectionMgr = VCEditor.Instance.m_VoxelSelection;
			CreateMainInspector();
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
		}
		else
		{
			GameObject.Destroy(this.gameObject);
		}
	}
	
	void OnDestroy ()
	{
		if ( m_MainInspector != null )
		{
			GameObject.Destroy(m_MainInspector);
			m_MainInspector = null;
		}
	}
	
	void CreateMainInspector ()
	{
		m_MainInspector = GameObject.Instantiate(m_MainInspectorRes) as GameObject;
		m_MainInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_MainInspector.transform.localPosition = Vector3.zero;
		m_MainInspector.transform.localScale = Vector3.one;
		m_MainInspector.SetActive(false);
		VCEUISelectVoxelInspector svi = m_MainInspector.GetComponent<VCEUISelectVoxelInspector>();
		svi.m_SelectBrush = this;
	}
	void ShowMainInspector ()
	{
		m_MainInspector.SetActive(true);
	}
	void HideMainInspector ()
	{
		m_MainInspector.SetActive(false);
	}
	
	public override void ClearSelection ()
	{
		m_SelectionMgr.ClearSelection();
	}
	
	public override void Cancel () {}

	private float tips_counter = 8f;
	void OnGUI ()
	{
		GUI.skin = VCEditor.Instance.m_GUISkin;
		if ( !Input.GetMouseButton(0) && VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.snapto) )
		{
			string text = "Press     you can set\r\n" +
				          "view center here!";
			GUI.color = new Color(1,1,1,Mathf.Clamp01(tips_counter));
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 26, 100,100), text, "CursorText2" );
			GUI.color = new Color(1,1,0,Mathf.Clamp01(tips_counter));
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 26, 100,100), "          F", "CursorText2" );
		}
	}

	void Update ()
	{
		// Common draw target
		VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, 1, true);
		
		// Inspector
		if ( VCEditor.s_Scene.m_IsoData.m_Voxels.Count > 0 )
			ShowMainInspector();
		else
			HideMainInspector();
		
		// Cancel
		if ( VCEInput.s_Cancel )
		{
#if CAN_CANCELALLMETHOD
			if ( m_MethodExec == null )
				m_SelectionMgr.ClearSelection();
			else
				m_MainInspector.GetComponent<VCEUISelectVoxelInspector>().CancelAllMethod();
#else
			m_SelectionMgr.ClearSelection();
#endif
		}
		
		// Execute selection method
		if ( m_MethodExec != null )
		{
			m_MethodExec.MainMethod();
			if ( m_MethodExec.m_NeedUpdate )
			{
				m_SelectionMgr.RebuildSelectionBoxes();
				m_MethodExec.m_NeedUpdate = false;
			}
		}
		else
		{
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
		}

		if ( !m_MainInspector.activeInHierarchy )
			m_MainInspector.GetComponent<VCEUISelectVoxelInspector>().Update();

		if ( m_SelectionMgr.m_Selection.Count > 0 )
		{
			if ( VCEInput.s_Shift && VCEInput.s_Left )
			{
				ExtrudeSelection(-1,0,0);
				VCEStatusBar.ShowText("Extrude left".ToLocalizationString(), 2);
			}
			if ( VCEInput.s_Shift && VCEInput.s_Right )
			{
				ExtrudeSelection(1,0,0);
				VCEStatusBar.ShowText("Extrude right".ToLocalizationString(), 2);
			}
			if ( VCEInput.s_Shift && VCEInput.s_Up )
			{
				ExtrudeSelection(0,1,0);
				VCEStatusBar.ShowText("Extrude up".ToLocalizationString(), 2);
			}
			if ( VCEInput.s_Shift && VCEInput.s_Down )
			{
				ExtrudeSelection(0,-1,0);
				VCEStatusBar.ShowText("Extrude down".ToLocalizationString(), 2);
			}
			if ( VCEInput.s_Shift && VCEInput.s_Forward )
			{
				ExtrudeSelection(0,0,1);
				VCEStatusBar.ShowText("Extrude forward".ToLocalizationString(), 2);
			}
			if ( VCEInput.s_Shift && VCEInput.s_Back )
			{
				ExtrudeSelection(0,0,-1);
				VCEStatusBar.ShowText("Extrude back".ToLocalizationString(), 2);
			}
		}
		// Tips
		if ( !Input.GetMouseButton(0) && VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.snapto) )
		{
			tips_counter -= Time.deltaTime;
		}
	}
	
	// 
	// Functions
	//
	public void ExtrudeSelection(int x, int y, int z)
	{
		if ( x == 0 && y == 0 && z == 0 )
			return;
		m_Action = new VCEAction ();
		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		List<int> new_select_keys = new List<int> ();
		List<int> new_select_values = new List<int> ();
		foreach ( KeyValuePair<int, byte> kvp in m_SelectionMgr.m_Selection )
		{
			float strength = (float)(kvp.Value) / 255.0f;
			IntVector3 ipos = VCIsoData.KeyToIPos(kvp.Key);
			ipos.x += x;
			ipos.y += y;
			ipos.z += z;
			if ( VCEditor.s_Scene.m_IsoData.IsPointIn(ipos) )
			{
				int voxel_pos = VCIsoData.IPosToKey(ipos);
				new_select_keys.Add(voxel_pos);
				new_select_values.Add((int)kvp.Value);
				// Mirror
				if ( VCEditor.s_Mirror.Enabled_Masked )
				{
					//IntVector3 real = ipos;
					VCEditor.s_Mirror.MirrorVoxel(ipos);
					for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
					{
						IntVector3 image = VCEditor.s_Mirror.Output[i];
						if ( VCEditor.s_Scene.m_IsoData.IsPointIn(image) )
						{
							voxel_pos = VCIsoData.IPosToKey(image);
							VCVoxel src_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(kvp.Key);
							VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
							VCVoxel new_voxel = src_voxel;
							if ( strength == 1 )
								new_voxel.VolumeF = old_voxel.VolumeF + src_voxel.VolumeF;
							else
								new_voxel.VolumeF = old_voxel.VolumeF + src_voxel.VolumeF * Mathf.Lerp(Mathf.Pow(strength, 0.3f),1,0.3f);
							
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
					VCVoxel src_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(kvp.Key);
					VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
					VCVoxel new_voxel = src_voxel;
					if ( strength == 1 )
						new_voxel.VolumeF = old_voxel.VolumeF + src_voxel.VolumeF;
					else
						new_voxel.VolumeF = old_voxel.VolumeF + src_voxel.VolumeF * Mathf.Lerp(Mathf.Pow(strength, 0.3f),1,0.3f);

					if ( old_voxel != new_voxel )
					{
						VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
						m_Action.Modifies.Add(modify);
					}
				}
			}
		}

		// Clear new area's color
		ColorSelection(VCIsoData.BLANK_COLOR, false, true);

		if ( m_Action.Modifies.Count > 0 )
		{
			m_Action.Do();
		}

		// Select new extruded area
		m_SelectionMgr.m_Selection.Clear();
		for ( int i = 0; i < new_select_keys.Count; ++i )
		{
			if ( !VCEditor.s_Scene.m_IsoData.IsGarbageVoxel(new_select_keys[i]) )
				m_SelectionMgr.m_Selection.Add(new_select_keys[i], (byte)(new_select_values[i]));
		}
		m_SelectionMgr.RebuildSelectionBoxes();
	}

	public void DeleteSelection()
	{
		m_Action = new VCEAction ();

		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		foreach ( KeyValuePair<int, byte> kvp in m_SelectionMgr.m_Selection )
		{
			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				IntVector3 pos = VCIsoData.KeyToIPos(kvp.Key);
				VCEditor.s_Mirror.MirrorVoxel(pos);
				float strength = (float)(kvp.Value) / 255.0f;
				for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
				{
					if ( VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]) )
					{
						int voxel_pos = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
						VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
						VCVoxel new_voxel = old_voxel;
						new_voxel.VolumeF = new_voxel.VolumeF * (1-strength);
						if ( old_voxel != new_voxel )
						{
							VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
							modify.Redo();
							m_Action.Modifies.Add(modify);
						}
					}
				}
			}
			// No mirror
			else
			{
				float strength = (float)(kvp.Value) / 255.0f;
				int voxel_pos = kvp.Key;
				VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
				VCVoxel new_voxel = old_voxel;
				new_voxel.VolumeF = new_voxel.VolumeF * (1-strength);
				if ( old_voxel != new_voxel )
				{
					VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
					m_Action.Modifies.Add(modify);
				}
			}
		}
		ColorSelection(VCIsoData.BLANK_COLOR, false, true);
		if ( m_Action.Modifies.Count > 0 )
		{
			m_Action.DoButNotRegister();
			VCUtils.ISOCut(VCEditor.s_Scene.m_IsoData, m_Action);
			m_Action.Register();
			VCEStatusBar.ShowText("Selected voxels have been removed".ToLocalizationString(), 2);
		}
		ClearSelection();
	}
	
	public void TextureSelection()
	{
		if ( !VCEditor.Instance.m_UI.m_MaterialTab.isChecked )
			return;
		if ( VCEditor.SelectedVoxelType < 0 )
			return;
		
		m_Action = new VCEAction ();
		
		ulong oldmat_guid = VCEditor.s_Scene.m_IsoData.MaterialGUID(VCEditor.SelectedVoxelType);
		ulong newmat_guid = VCEditor.SelectedMaterial.m_Guid;
		if ( oldmat_guid != newmat_guid )
		{
			VCEAlterMaterialMap modify = new VCEAlterMaterialMap (VCEditor.SelectedVoxelType, oldmat_guid, newmat_guid);
			m_Action.Modifies.Add(modify);
		}

		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		foreach ( KeyValuePair<int, byte> kvp in m_SelectionMgr.m_Selection )
		{
			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				IntVector3 pos = VCIsoData.KeyToIPos(kvp.Key);
				VCEditor.s_Mirror.MirrorVoxel(pos);
				float strength = (float)(kvp.Value) / 255.0f;
				if ( strength < 0.5f )
					continue;
				for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
				{
					if ( VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]) )
					{
						int voxel_pos = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
						VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
						VCVoxel new_voxel = new VCVoxel(old_voxel.Volume, (byte)(VCEditor.SelectedVoxelType));
						if ( old_voxel != new_voxel )
						{
							VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
							modify.Redo();
							m_Action.Modifies.Add(modify);
						}
					}
				}
			}
			// No mirror
			else
			{
				float strength = (float)(kvp.Value) / 255.0f;
				if ( strength < 0.5f )
					continue;
				int voxel_pos = kvp.Key;
				VCVoxel old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
				VCVoxel new_voxel = new VCVoxel(old_voxel.Volume, (byte)(VCEditor.SelectedVoxelType));
				if ( old_voxel != new_voxel )
				{
					VCEAlterVoxel modify = new VCEAlterVoxel(voxel_pos, old_voxel, new_voxel);
					m_Action.Modifies.Add(modify);
				}
			}
		}
		if ( m_Action.Modifies.Count > 0 )
		{
			m_Action.Do();
			VCEStatusBar.ShowText("Selected voxels have been textured".ToLocalizationString(), 2);
		}
	}
	
	public void ColorSelection(Color32 color, bool consider_strength = true, bool action_segment = false)
	{
		if ( !VCEditor.Instance.m_UI.m_PaintTab.isChecked )
			return;
		
		if ( !action_segment )
			m_Action = new VCEAction ();

		bool modified = false;

		VCEUpdateColorSign sign_b = new VCEUpdateColorSign (false, true);
		m_Action.Modifies.Add(sign_b);

		VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		foreach ( SelBox sb in m_SelectionMgr.m_GL.m_Boxes )
		{
			float t = (float)(sb.m_Val) / 255.0f;
			byte code;	// This is a filter code, when color key was not on edge or vertex, their is no point to color this key.
			float x,y,z;
			for ( x = sb.m_Box.xMin, code = 0; x <= sb.m_Box.xMax + 1.01f; x += 0.5f, code ^= 1 )
			{
				for ( y = sb.m_Box.yMin, code &= 1; y <= sb.m_Box.yMax + 1.01f; y += 0.5f, code ^= 2 )
				{
					if ( code == 0 || code == 4 )
						continue;	// code 0, 4, no point to color
					for ( z = sb.m_Box.zMin, code &= 3; z <= sb.m_Box.zMax + 1.01f; z += 0.5f, code ^= 4 )
					{
						if ( code == 1 || code == 2 )
							continue;	// code 1, 2, no point to color
						// Mirror
						if ( VCEditor.s_Mirror.Enabled_Masked )
						{
							IntVector3 color_pos = VCIsoData.IPosToColorPos(new Vector3(x,y,z));
							VCEditor.s_Mirror.MirrorColor(color_pos);

							for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
							{
								if ( VCEditor.s_Scene.m_IsoData.IsColorPosIn(VCEditor.s_Mirror.Output[i]) )
								{
									int key = VCIsoData.ColorPosToColorKey(VCEditor.s_Mirror.Output[i]);
									Color32 old_color = VCEditor.s_Scene.m_IsoData.GetColor(key);
									Color32 new_color = consider_strength ? Color32.Lerp(old_color, color, t) : color;
									if ( old_color.r == new_color.r && old_color.g == new_color.g 
									    && old_color.b == new_color.b && old_color.a == new_color.a )
										continue;
									VCEAlterColor modify = new VCEAlterColor (key, old_color, new_color);
									modify.Redo();
									m_Action.Modifies.Add(modify);
									modified = true;
								}
							}
						}
						// No mirror
						else
						{
							int key = VCIsoData.IPosToColorKey(new Vector3(x,y,z));
							Color32 old_color = VCEditor.s_Scene.m_IsoData.GetColor(key);
							Color32 new_color = consider_strength ? Color32.Lerp(old_color, color, t) : color;
							if ( old_color.r == new_color.r && old_color.g == new_color.g 
							  && old_color.b == new_color.b && old_color.a == new_color.a )
								continue;
							VCEAlterColor modify = new VCEAlterColor (key, old_color, new_color);
							m_Action.Modifies.Add(modify);
							modified = true;
						}
					}
				}
			}
		}
		
		VCEUpdateColorSign sign_f = new VCEUpdateColorSign (true, false);
		m_Action.Modifies.Add(sign_f);
		if ( action_segment && !modified )
		{
			m_Action.Modifies.RemoveRange(m_Action.Modifies.Count-2, 2);
		}
		if ( !action_segment && m_Action.Modifies.Count > 2 )
		{
			m_Action.Do();
			if ( color.r == VCIsoData.BLANK_COLOR.r
			    && color.g == VCIsoData.BLANK_COLOR.g
			    && color.b == VCIsoData.BLANK_COLOR.b
			    && color.a == VCIsoData.BLANK_COLOR.a )
				VCEStatusBar.ShowText("Selection color have been erased".ToLocalizationString(), 2);
			else
				VCEStatusBar.ShowText("Selected voxels have been painted".ToLocalizationString(), 2);
		}
	}
}
