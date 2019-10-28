using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEFreeAirbrush : VCEBrush
{
	public bool m_Eraser = false;
	public GLAirbrushCursor m_GLCursor = null;
	private Color m_TargetColor = Color.clear;
	public Color m_UIColor = Color.clear;
	private VCMeshMgr m_MeshMgr = null;
	
	private bool m_Drawing;
	public bool DisplayCursor { get { return m_Drawing || !VCEInput.s_MouseOnUI; } }
	
	// continous drawing vars
	private Vector3 m_lastHit;		// hitpoint of last frame
	private Vector3 m_lastDraw;		// last draw point
	private float m_simDist = 1;	// when raycast nothing, sim a hitpoint with this dist
	private float m_simStep = 1;	// the test ray step
	private float m_drawStep = 1;
	List<MeshFilter> m_NeedUpdateMfs = new List<MeshFilter> ();
	
	public float m_Radius = 0.25f;
	public float m_Hardness = 1;
	public float m_Strength = 1;
	
	// recent settings
	public static float s_RecentRadius = 4f;
	public static float s_RecentHardness = 0f;
	public static float s_RecentStrength = 1f;
	
	// Inspector
	public VCEUIFreeAirbrushInspector m_InspectorRes;
	public VCEUIFreeAirbrushInspector m_Inspector;
	
	void Start ()
	{
		m_Radius = s_RecentRadius;
		m_Hardness = s_RecentHardness;
		m_Strength = s_RecentStrength;
		
		m_GLCursor.m_Airbrush = this;
		m_MeshMgr = VCEditor.Instance.m_MeshMgr;
		
		// continous drawing vars
		m_simDist = VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 0.2f;
		m_lastHit = Vector3.zero;
		m_lastDraw = Vector3.one * (-100);
		
		// Inspector
		m_Inspector = VCEUIFreeAirbrushInspector.Instantiate(m_InspectorRes) as VCEUIFreeAirbrushInspector;
		m_Inspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_Inspector.transform.localPosition = new Vector3(0,-10,0);
		m_Inspector.transform.localScale = Vector3.one;
		m_Inspector.m_ParentBrush = this;
		m_Inspector.gameObject.SetActive(true);

		if ( m_Eraser )
			VCEStatusBar.ShowText("Eraser - Erase color on voxel blocks".ToLocalizationString(), 7, true);
		else
			VCEStatusBar.ShowText("Airbrush - Paint color on voxel blocks".ToLocalizationString(), 7, true);
	}
	
	void OnDestroy ()
	{
		// Inspector
		if ( m_Inspector != null )
		{
			GameObject.Destroy(m_Inspector.gameObject);
			m_Inspector = null;
		}
	}
	
	void Update ()
	{
		// Save recent vars
		s_RecentRadius = m_Radius;
		s_RecentHardness = m_Hardness;
		s_RecentStrength = m_Strength;
		
		// Prepare collider
		if ( !VCEditor.s_Scene.m_MeshComputer.Computing )
		{
			if ( m_MeshMgr.m_ColliderDirty )
				m_MeshMgr.PrepareMeshColliders();
		}
		if ( m_MeshMgr.m_ColliderDirty ) return;
		
		// Color ( target & display )
		m_TargetColor = m_Eraser ? VCIsoData.BLANK_COLOR : VCEditor.SelectedColor;
		m_UIColor = m_TargetColor;
		m_UIColor.a = 1;
		
		// Update mesh color
		foreach ( MeshFilter mf in m_NeedUpdateMfs )
			m_MeshMgr.UpdateMeshColor(mf);
		m_NeedUpdateMfs.Clear();
		
		// Drawing
		if ( m_Drawing )
		{
			if ( VCEInput.s_Cancel )
			{
				m_Drawing = false;
				if ( m_Action != null )
				{
					m_Action.Undo();
					m_Action = null;
				}
				else
				{
					Debug.LogError("There must be some problem");
				}
			}
			else
			{
				if ( Input.GetMouseButtonUp(0) )
				{
					m_Drawing = false;
					if ( m_Action != null )
					{
						VCEUpdateColorSign sign_f = new VCEUpdateColorSign (true, false);
						m_Action.Modifies.Add(sign_f);
						if ( m_Action.Modifies.Count > 2 )
							m_Action.Do();
						m_Action = null;
					}
					else
					{
						Debug.LogError("There must be some problem");
					}
				}
				else
				{
					// Mouse is pressing
					// During drawing: From 2nd frame since mouse down
					
					// 1. Test hitpoint now
					RaycastHit rch;
					Vector3 hit;
					if ( VCEMath.RayCastMesh(VCEInput.s_PickRay, out rch) )
					{
						hit = rch.point;
						m_simDist = rch.distance;
					}
					else
					{
						hit = VCEInput.s_PickRay.GetPoint(m_simDist);
					}
					
					// 2. Step by step ray cast
					Vector3 move_vec = hit - m_lastHit;
					if ( move_vec.magnitude > m_simStep )
					{
						float ustep = m_simStep / move_vec.magnitude;
						
						// traverse all the interpolation points
						for ( float t = ustep; t < 0.999f + ustep; t += ustep )
						{
							t = Mathf.Clamp01(t);
							Vector3 inter_point = m_lastHit + t*move_vec;
							Ray test_ray = new Ray (VCEInput.s_PickRay.origin, (inter_point - VCEInput.s_PickRay.origin).normalized);
							if ( VCEMath.RayCastMesh(test_ray, out rch) )
							{
								m_simDist = rch.distance;
								if ( (rch.point - m_lastDraw).magnitude > m_drawStep )
								{
									PaintPosition(rch.point, rch.normal);
									m_lastDraw = rch.point;
									Debug.DrawRay(test_ray.origin, inter_point - VCEInput.s_PickRay.origin, m_UIColor, 10.0f);
								}
								else
								{
									Debug.DrawRay(test_ray.origin, inter_point - VCEInput.s_PickRay.origin, new Color(m_UIColor.r, m_UIColor.g, m_UIColor.b, 0.2f), 10.0f);
								}
							}
							else
							{
								Debug.DrawRay(test_ray.origin, inter_point - VCEInput.s_PickRay.origin, new Color(m_UIColor.r, m_UIColor.g, m_UIColor.b, 0.1f), 10.0f);
							}
						}
						// 3. Update last hitpoint
						m_lastHit = hit;
					}
				}
			}
		}
		else
		{
			if ( VCEInput.s_Cancel )
			{
				Cancel();
			}
			else
			{
				if ( Input.GetMouseButtonDown(0) && !VCEInput.s_MouseOnUI )
				{
					m_Drawing = true;
					if ( m_Action != null )
					{
						Debug.LogError("There must be some problem");
						m_Action = null;
					}
					m_Action = new VCEAction ();
					VCEUpdateColorSign sign_b = new VCEUpdateColorSign (false, true);
					m_Action.Modifies.Add(sign_b);
					
					// draw once
					
					// Init continous drawing vars
					m_lastHit = Vector3.zero;
					m_lastDraw = Vector3.one * (-100);
					m_simStep = Mathf.Clamp(m_Radius * 0.07f, 0.2f, 100.0f) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
					m_drawStep = Mathf.Clamp(m_Radius * 0.5f, 0.2f, 100.0f) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
					m_simDist = VCEditor.s_Scene.m_Setting.EditorWorldSize.magnitude * 0.2f;
					
					RaycastHit rch;
					// cast
					if ( VCEMath.RayCastMesh(VCEInput.s_PickRay, out rch) )
					{
						PaintPosition(rch.point, rch.normal);
						m_lastHit = rch.point;
						m_lastDraw = rch.point;
						m_simDist = rch.distance;
					}
					// no cast
					else
					{
						m_lastHit = VCEInput.s_PickRay.GetPoint(m_simDist);
					}
				}
			}
		}
	}
	
	// Paint a specified mesh position
	void PaintPosition (Vector3 wpos, Vector3 normal)
	{
		float voxel_size = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		VCEditor.s_Mirror.CalcPrepare(voxel_size);

		// calculate the right & up axis
		transform.rotation = Quaternion.LookRotation(-normal);
		Vector3 right = transform.right;
		Vector3 up = transform.up;
		transform.rotation = Quaternion.identity;
		
		float r = Mathf.Clamp(m_Radius, 0.25f, 16.0f);
		Vector3 pivot = wpos + normal * voxel_size;
		for ( float a = -r; a <= r + 0.001f; a += 0.25f )
		{
			for ( float b = -r; b <= r + 0.001f; b += 0.25f )
			{
				if ( a*a + b*b > r*r )
					continue;
				float strength = 1;
				if ( r > 0.75f && m_Hardness < 1 )
				{
					strength = (1 - Mathf.Sqrt(a*a + b*b) / r) / (1-m_Hardness);
					strength = Mathf.Pow(strength, 2);
					strength = Mathf.Clamp01(strength);
				}
				Vector3 origin = pivot + (a*right + b*up) * voxel_size;
				Ray ray = new Ray (origin, -normal);
				RaycastHit rch;
				if ( VCEMath.RayCastMesh(ray, out rch, voxel_size * 2) )
				{
					Vector3 iso_pos = rch.point/VCEditor.s_Scene.m_Setting.m_VoxelSize;
					MeshFilter mf = rch.collider.GetComponent<MeshFilter>();
					ColorPosition(iso_pos, mf, strength*m_Strength);
				}
			}
		}
	}
	
	void ColorPosition (Vector3 iso_pos, MeshFilter mf, float strength)
	{
		if ( m_Action != null && mf )
		{
			int pos = VCIsoData.IPosToColorKey(iso_pos);
			
			// Check color necessity
			int _x2 = pos & 0x3ff;
			int _y2 = (pos >> 20) & 0x3ff;
			int _z2 = (pos >> 10) & 0x3ff;
			if ( _x2 % 2 == 1 && _y2 % 2 == 1 )
				return;
			if ( _y2 % 2 == 1 && _z2 % 2 == 1 )
				return;
			if ( _z2 % 2 == 1 && _x2 % 2 == 1 )
				return;
			
			// Mirror
			if ( VCEditor.s_Mirror.Enabled_Masked )
			{
				IntVector3 color_pos = VCIsoData.IPosToColorPos(iso_pos);
				VCEditor.s_Mirror.MirrorColor(color_pos);

				for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
				{
					if ( VCEditor.s_Scene.m_IsoData.IsColorPosIn(VCEditor.s_Mirror.Output[i]) )
					{
						int _pos = VCIsoData.ColorPosToColorKey(VCEditor.s_Mirror.Output[i]);
						Color32 old_color = VCEditor.s_Scene.m_IsoData.GetColor(_pos);
						Color32 new_color = Color32.Lerp(old_color, m_TargetColor, strength);
						if ( old_color.r == new_color.r &&
						     old_color.g == new_color.g &&
						     old_color.b == new_color.b &&
						     old_color.a == new_color.a )
						{
							continue;
						}

						VCEAlterColor modify = new VCEAlterColor (_pos, old_color, new_color);
						m_Action.Modifies.Add(modify);
						modify.Redo();
						if ( !m_NeedUpdateMfs.Contains(mf) )
							m_NeedUpdateMfs.Add(mf);
					}
				}
			}
			// No mirror
			else
			{
				Color32 old_color = VCEditor.s_Scene.m_IsoData.GetColor(pos);
				Color32 new_color = Color32.Lerp(old_color, m_TargetColor, strength);
				if ( old_color.r == new_color.r &&
				     old_color.g == new_color.g &&
				     old_color.b == new_color.b &&
				     old_color.a == new_color.a )
				{
					return;
				}
				
				VCEAlterColor modify = new VCEAlterColor (pos, old_color, new_color);
				m_Action.Modifies.Add(modify);
				modify.Redo();
				if ( !m_NeedUpdateMfs.Contains(mf) )
					m_NeedUpdateMfs.Add(mf);
			}
		}
	}
	
	protected override void Do ()
	{
		
	}
	public override void Cancel ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			VCEditor.Instance.m_UI.m_DefaultBrush_Color.isChecked = true;
		}
	}
}
