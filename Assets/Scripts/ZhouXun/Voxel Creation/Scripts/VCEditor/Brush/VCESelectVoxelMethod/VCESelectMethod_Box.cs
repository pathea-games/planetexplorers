using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Select method of selecting voxels (Box selecting) 
public class VCESelectMethod_Box : VCESelectMethod
{
	private IntVector3 m_Begin = new IntVector3 (-1,-1,-1);
	private IntVector3 m_End = new IntVector3 (-1,-1,-1);
	private float m_PlanePos = -1;
	private ECoordPlane m_Coord = ECoordPlane.XZ;
	private int m_NormalSign = 0;
	public int m_Depth = 1;
	// Advanced
	public bool m_PlaneFeather = true;
	public int m_FeatherLength = 0;
	public bool m_MaterialSelect = false;
	public bool m_MaterialSelectChange = false;
	
	// Temp
	public static int s_RecentDepth = 1;
	public static bool s_RecentPlaneFeather = true;
	public static int s_RecentFeatherLength = 0;
	public static bool s_RecentMaterialSelect = false;
	
	// UI Inspector
	private VCEUIBoxMethodInspector m_Inspector;
	
	void Start ()
	{
		m_Depth = s_RecentDepth;
		m_FeatherLength = s_RecentFeatherLength;
		m_PlaneFeather = s_RecentPlaneFeather;
		m_MaterialSelect = s_RecentMaterialSelect;
		m_Inspector = VCEUIBoxMethodInspector.Instantiate(m_Parent.m_BoxMethodInspectorRes) as VCEUIBoxMethodInspector;
		m_Inspector.gameObject.SetActive(false);
		m_Inspector.transform.parent = m_Parent.m_MainInspector.transform;
		m_Inspector.transform.localPosition = new Vector3 (0,-66,0);
		m_Inspector.transform.localScale = Vector3.one;
		m_Inspector.m_SelectMethod = this;
		m_Inspector.gameObject.SetActive(true);
	}
	
	void OnDestroy ()
	{
		if ( m_Inspector != null )
		{
			GameObject.Destroy(m_Inspector.gameObject);
			m_Inspector = null;
		}
	}
	
	void Update ()
	{
		s_RecentDepth = m_Depth;
		s_RecentFeatherLength = m_FeatherLength;
		s_RecentMaterialSelect = m_MaterialSelect;
		s_RecentPlaneFeather = m_PlaneFeather;
		VCEditor.Instance.m_NearVoxelIndicator.enabled = !m_Selecting;
		m_GUIAlpha = Mathf.Lerp(m_GUIAlpha, 0, 0.05f);
	}
	
	public override void MainMethod ()
	{
		if ( VCEInput.s_Increase && !VCEInput.s_Shift )
		{
			if ( m_Depth < 300 )
				m_Depth++;
			m_GUIAlpha = 5;
		}
		else if ( VCEInput.s_Decrease && !VCEInput.s_Shift )
		{
			if ( m_Depth > 1 )
				m_Depth--;
			m_GUIAlpha = 5;
		}
		//
		// Selecting, mouse dragging
		//
		if ( m_Selecting )
		{
			if ( VCEInput.s_Cancel )
			{
				ExitSelecting();
				return;
			}
			RaycastHit rch;

			// ray cast coord plane
			if ( VCEMath.RayCastCoordPlane(VCEInput.s_PickRay, m_Coord, m_PlanePos, out rch) )
			{
				m_End.x = Mathf.FloorToInt(rch.point.x);
				m_End.y = Mathf.FloorToInt(rch.point.y);
				m_End.z = Mathf.FloorToInt(rch.point.z);
				switch ( m_Coord )
				{
				case ECoordPlane.XY: m_End.z = m_Begin.z + (m_Depth - 1) * m_NormalSign; break;
				case ECoordPlane.XZ: m_End.y = m_Begin.y == 0 && VCEditor.s_Scene.m_IsoData.GetVoxel(VCIsoData.IPosToKey(m_Begin)).Volume < 128 ? 
						VCEditor.s_Scene.m_Setting.m_EditorSize.y - 1 : 
						m_Begin.y + (m_Depth - 1) * m_NormalSign;
					break;
				case ECoordPlane.ZY: m_End.x = m_Begin.x + (m_Depth - 1) * m_NormalSign; break;
				default: ExitSelecting(); return;
				}
			}
			else
			{
				ExitSelecting();
				return;
			}
			m_Iso.ClampPointI(m_Begin);
			m_Iso.ClampPointI(m_End);
			
			// Submit this selecting action !
			if ( Input.GetMouseButtonUp(0) && m_Selecting )
			{
				Submit();
				m_Selecting = false;
				m_NeedUpdate = true;
			}
		} // End selecting
		
		//
		// Free mouse, prepare to select
		//
		else
		{
			// Mouse not on GUI
			if ( !VCEInput.s_MouseOnUI )
			{
				// Click left mouse button
				if ( Input.GetMouseButtonDown(0) )
				{
					// has draw target
					if ( m_Parent.m_Target.snapto != null && m_Parent.m_Target.cursor != null )
					{
						VCEMath.DrawTarget dtar = m_Parent.m_Target;
						// snapto has voxel
						if ( m_Iso.GetVoxel(VCIsoData.IPosToKey(dtar.snapto)).Volume > 0 )
						{
							m_Begin.x = dtar.snapto.x;
							m_Begin.y = dtar.snapto.y;
							m_Begin.z = dtar.snapto.z;
						}
						// snapto don't has voxel
						else
						{
							m_Begin.x = dtar.cursor.x;
							m_Begin.y = dtar.cursor.y;
							m_Begin.z = dtar.cursor.z;
							dtar.rch.normal = -dtar.rch.normal;
						}
						
						// Assgin end
						m_End.x = m_Begin.x;
						m_End.y = m_Begin.y;
						m_End.z = m_Begin.z;
						// Assgin coord and position
						// zy plane
						if ( Mathf.Abs(dtar.rch.normal.x) > 0.9f )
						{
							m_Coord = ECoordPlane.ZY;
							m_PlanePos = dtar.rch.point.x;
							m_Selecting = true;
							m_NormalSign = -Mathf.RoundToInt(Mathf.Sign(dtar.rch.normal.x));
						}
						// xz plane
						else if ( Mathf.Abs(dtar.rch.normal.y) > 0.9f )
						{
							m_Coord = ECoordPlane.XZ;
							m_PlanePos = dtar.rch.point.y;
							m_Selecting = true;
							m_NormalSign = -Mathf.RoundToInt(Mathf.Sign(dtar.rch.normal.y));
						}
						// xy plane
						else if ( Mathf.Abs(dtar.rch.normal.z) > 0.9f )
						{
							m_Coord = ECoordPlane.XY;
							m_PlanePos = dtar.rch.point.z;
							m_Selecting = true;
							m_NormalSign = -Mathf.RoundToInt(Mathf.Sign(dtar.rch.normal.z));
						}
						// it's impossible !!!
						else
						{
							Debug.LogError("It's impossible !!");
							ExitSelecting();
						}
					}
					// Don't have draw target
					else
					{
						// Select
						if ( !VCEInput.s_Shift && !VCEInput.s_Alt && !VCEInput.s_Control )
							m_Selection.Clear();
						m_NeedUpdate = true;
					}
				}
			}

			if(m_MaterialSelectChange)
			{
				Submit();
				m_Selecting = false;
				m_NeedUpdate = true;
			}
		} // End free mouse
	}
	
	// Submit this selecting action
	protected override void Submit ()
	{
		m_Iso.ClampPointI(m_Begin);
		m_Iso.ClampPointI(m_End);
		IntVector3 iMin = new IntVector3 (0,0,0);
		IntVector3 iMax = new IntVector3 (0,0,0);
		iMin.x = Mathf.Min(m_Begin.x, m_End.x);
		iMin.y = Mathf.Min(m_Begin.y, m_End.y);
		iMin.z = Mathf.Min(m_Begin.z, m_End.z);
		iMax.x = Mathf.Max(m_Begin.x, m_End.x);
		iMax.y = Mathf.Max(m_Begin.y, m_End.y);
		iMax.z = Mathf.Max(m_Begin.z, m_End.z);
		
		// Calculate feather effect bound
		IntVector3 fMin = new IntVector3 (0,0,0);
		IntVector3 fMax = new IntVector3 (0,0,0);
		fMin.x = iMin.x - m_FeatherLength; fMin.y = iMin.y - m_FeatherLength; fMin.z = iMin.z - m_FeatherLength;
		fMax.x = iMax.x + m_FeatherLength; fMax.y = iMax.y + m_FeatherLength; fMax.z = iMax.z + m_FeatherLength;
		if ( m_PlaneFeather )
		{
			switch (m_Coord)
			{
			case ECoordPlane.XY:
				fMin.z = iMin.z;
				fMax.z = iMax.z;
				break;
			case ECoordPlane.XZ:
				fMin.y = iMin.y;
				fMax.y = iMax.y;
				break;
			case ECoordPlane.ZY:
				fMin.x = iMin.x;
				fMax.x = iMax.x;
				break;
			}
		}
		m_Iso.ClampPointI(fMin);
		m_Iso.ClampPointI(fMax);
		
		// Select
		if ( !VCEInput.s_Shift && !VCEInput.s_Alt && !VCEInput.s_Control )
			m_Selection.Clear();
		
		for ( int x = fMin.x; x <= fMax.x; ++x )
		{
			for ( int y = fMin.y; y <= fMax.y; ++y )
			{
				for ( int z = fMin.z; z <= fMax.z; ++z )
				{
					int poskey = VCIsoData.IPosToKey(x,y,z);
					if ( m_Iso.GetVoxel(poskey).Volume < 1 )
						continue;
					int old_sv = 0;
					int alter_sv = (m_FeatherLength == 0) ? (255) : ((int)(VCEMath.BoxFeather(new IntVector3(x,y,z), iMin, iMax, m_FeatherLength) * 255.0f));
					int new_sv = 0;
					if ( alter_sv < 1 )
						continue;
					
					if ( m_Selection.ContainsKey(poskey) )
						old_sv = m_Selection[poskey];
					if ( VCEInput.s_Shift )
						new_sv = old_sv + alter_sv;
					else if ( VCEInput.s_Alt )
						new_sv = old_sv - alter_sv;
					else if ( VCEInput.s_Control )
						new_sv = Mathf.Abs(old_sv - alter_sv);
					else
						new_sv = alter_sv;
					new_sv = Mathf.Clamp(new_sv, 0, 255);
					if ( new_sv < 1 )
						m_Selection.Remove(poskey);
					else if ( old_sv < 1 )
						m_Selection.Add(poskey, (byte)new_sv);
					else
						m_Selection[poskey] = (byte)new_sv;

					if(m_MaterialSelect)
					{
						VCVoxel vcv = m_Iso.GetVoxel(poskey);
                        if(m_Iso.m_Materials[vcv.Type] != VCEditor.SelectedMaterial)
						{
							m_Selection.Remove(poskey);
						}
					}
				}
			}
		}
	}
	
	// Cancel this selecting action
	void ExitSelecting ()
	{
		m_Begin.x = -1; m_Begin.y = -1; m_Begin.z = -1;
		m_End.x = -1; m_End.y = -1; m_End.z = -1;
		m_Coord = ECoordPlane.XZ;
		m_PlanePos = 0;
		m_NormalSign = 0;
		m_Selecting = false;
	}
	
	public IntVector3 SelectingSize ()	// x,y is flat plane, z is depth
	{
		if ( !m_Selecting )
			return new IntVector3(0,0,m_Depth);
		IntVector3 iMin = IntVector3.Zero;
		IntVector3 iMax = IntVector3.Zero;
		iMin.x = Mathf.Min(m_Begin.x, m_End.x);
		iMin.y = Mathf.Min(m_Begin.y, m_End.y);
		iMin.z = Mathf.Min(m_Begin.z, m_End.z);
		iMax.x = Mathf.Max(m_Begin.x, m_End.x);
		iMax.y = Mathf.Max(m_Begin.y, m_End.y);
		iMax.z = Mathf.Max(m_Begin.z, m_End.z);
		
		switch (m_Coord)
		{
		case ECoordPlane.ZY:
			return new IntVector3(iMax.z - iMin.z + 1, iMax.y - iMin.y + 1, m_Depth);
		case ECoordPlane.XZ:
			return new IntVector3(iMax.x - iMin.x + 1, iMax.z - iMin.z + 1, m_Depth);
		case ECoordPlane.XY:
			return new IntVector3(iMax.x - iMin.x + 1, iMax.y - iMin.y + 1, m_Depth);
		}
		return new IntVector3(0,0,m_Depth);
	}

	private float m_GUIAlpha = 1;
	void OnGUI ()
	{
		if ( VCEInput.s_MouseOnUI )
			return;
		GUI.skin = VCEditor.Instance.m_GUISkin;
		
		GUI.color = Color.white;
		if ( m_Selecting )
		{
			GUI.color = Color.white;
		}
		else
		{
			GUI.color = new Color(1,1,1,Mathf.Clamp01(m_GUIAlpha));
		}
		if ( m_Depth > 1 )
			GUI.Label( new Rect(Input.mousePosition.x - 105, Screen.height - Input.mousePosition.y - 50, 100,100), "Depth x " + m_Depth.ToString(), "CursorText1" );
		else if (m_Begin.y > 0)
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 26, 100,100), "Use [Up]/[Down] arrow to set depth", "CursorText1" );
		
		GUI.color = new Color(1,1,1,0.5f);
		if ( VCEInput.s_Shift )
			GUI.Label( new Rect(Input.mousePosition.x - 105, Screen.height - Input.mousePosition.y - 75, 100,100), "ADD", "CursorText1" );
		else if ( VCEInput.s_Alt )
			GUI.Label( new Rect(Input.mousePosition.x - 105, Screen.height - Input.mousePosition.y - 75, 100,100), "SUBTRACT", "CursorText1" );
		else if ( VCEInput.s_Control )
			GUI.Label( new Rect(Input.mousePosition.x - 105, Screen.height - Input.mousePosition.y - 75, 100,100), "CROSS", "CursorText1" );
	}
	
	public override void OnGL ()
	{
		// Draw selecting box
		if ( m_Selecting )
		{
			Vector3 iMin = Vector3.zero;
			Vector3 iMax = Vector3.zero;
			iMin.x = Mathf.Min(m_Begin.x, m_End.x);
			iMin.y = Mathf.Min(m_Begin.y, m_End.y);
			iMin.z = Mathf.Min(m_Begin.z, m_End.z);
			iMax.x = Mathf.Max(m_Begin.x, m_End.x);
			iMax.y = Mathf.Max(m_Begin.y, m_End.y);
			iMax.z = Mathf.Max(m_Begin.z, m_End.z);
			iMin -= Vector3.one * 0.07f;
			iMax += Vector3.one * 1.07f;
			Vector3[] vec = new Vector3 [8]
			{
				new Vector3(iMax.x, iMax.y, iMax.z),
				new Vector3(iMin.x, iMax.y, iMax.z),
				new Vector3(iMin.x, iMin.y, iMax.z),
				new Vector3(iMax.x, iMin.y, iMax.z),
				new Vector3(iMax.x, iMax.y, iMin.z),
				new Vector3(iMin.x, iMax.y, iMin.z),
				new Vector3(iMin.x, iMin.y, iMin.z),
				new Vector3(iMax.x, iMin.y, iMin.z)
			};
			for ( int i = 0; i < 8; ++i )
				vec[i] = vec[i] * VCEditor.s_Scene.m_Setting.m_VoxelSize;
			
			Color lineColor = new Color(0.0f, 0.2f, 0.5f, 1.0f);
			Color faceColor = new Color(0.0f, 0.2f, 0.5f, 1.0f);
			
			lineColor.a = 1.0f;
			faceColor.a *= 0.4f + Mathf.Sin(Time.time*6f) * 0.1f;
			
			GL.Begin(GL.LINES);
			GL.Color(lineColor);
			GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[1]); GL.Vertex(vec[2]);
			GL.Vertex(vec[2]); GL.Vertex(vec[3]); GL.Vertex(vec[3]); GL.Vertex(vec[0]);
			GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[5]); GL.Vertex(vec[6]);
			GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[7]); GL.Vertex(vec[4]);
			GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[1]); GL.Vertex(vec[5]);
			GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[3]); GL.Vertex(vec[7]);
			GL.End();
			
			GL.Begin(GL.QUADS);
			GL.Color(faceColor);
			GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[2]); GL.Vertex(vec[3]);
			GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[7]);
			GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[1]);
			GL.Vertex(vec[1]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[2]);
			GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[3]);
			GL.Vertex(vec[3]); GL.Vertex(vec[7]); GL.Vertex(vec[4]); GL.Vertex(vec[0]);
			GL.End();
		}
		// Draw pre-select face direction
		else
		{
			if ( !VCEInput.s_MouseOnUI )
			{
				VCEMath.DrawTarget dtar = m_Parent.m_Target;
				if ( dtar.cursor != null && dtar.snapto != null )
				{
					if ( m_Iso.GetVoxel(VCIsoData.IPosToKey(dtar.snapto)).Volume == 0 )
						return;
					Vector3 iMin = dtar.rch.point;
					Vector3 iMax = dtar.rch.point;
					iMin += dtar.rch.normal * 0.03f;
					iMax += dtar.rch.normal * 0.06f;
					
					Color indicator_color = Color.white;
					// zy plane
					if ( Mathf.Abs(dtar.rch.normal.x) > 0.9f )
					{
						iMin.y = Mathf.Floor(iMin.y);
						iMin.z = Mathf.Floor(iMin.z);
						iMax.y = Mathf.Floor(iMax.y) + 1;
						iMax.z = Mathf.Floor(iMax.z) + 1;
						indicator_color = new Color(0.9f, 0.1f, 0.2f, 1.0f);
					}
					// xz plane
					else if ( Mathf.Abs(dtar.rch.normal.y) > 0.9f )
					{
						iMin.x = Mathf.Floor(iMin.x);
						iMin.z = Mathf.Floor(iMin.z);
						iMax.x = Mathf.Floor(iMax.x) + 1;
						iMax.z = Mathf.Floor(iMax.z) + 1;
						indicator_color = new Color(0.5f, 1.0f, 0.1f, 1.0f);
					}
					// xy plane
					else if ( Mathf.Abs(dtar.rch.normal.z) > 0.9f )
					{
						iMin.y = Mathf.Floor(iMin.y);
						iMin.x = Mathf.Floor(iMin.x);
						iMax.y = Mathf.Floor(iMax.y) + 1;
						iMax.x = Mathf.Floor(iMax.x) + 1;
						indicator_color = new Color(0.1f, 0.6f, 1.0f, 1.0f);
					}
					Vector3[] vec = new Vector3 [8]
					{
						new Vector3(iMax.x, iMax.y, iMax.z),
						new Vector3(iMin.x, iMax.y, iMax.z),
						new Vector3(iMin.x, iMin.y, iMax.z),
						new Vector3(iMax.x, iMin.y, iMax.z),
						new Vector3(iMax.x, iMax.y, iMin.z),
						new Vector3(iMin.x, iMax.y, iMin.z),
						new Vector3(iMin.x, iMin.y, iMin.z),
						new Vector3(iMax.x, iMin.y, iMin.z)
					};
					for ( int i = 0; i < 8; ++i )
						vec[i] = vec[i] * VCEditor.s_Scene.m_Setting.m_VoxelSize;
					
					Color lineColor = indicator_color;
					Color faceColor = indicator_color;
					
					lineColor.a = 1.0f;
					faceColor.a *= 0.7f + Mathf.Sin(Time.time*6f) * 0.1f;
					
					GL.Begin(GL.LINES);
					GL.Color(lineColor);
					GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[1]); GL.Vertex(vec[2]);
					GL.Vertex(vec[2]); GL.Vertex(vec[3]); GL.Vertex(vec[3]); GL.Vertex(vec[0]);
					GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[5]); GL.Vertex(vec[6]);
					GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[7]); GL.Vertex(vec[4]);
					GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[1]); GL.Vertex(vec[5]);
					GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[3]); GL.Vertex(vec[7]);
					GL.End();
					
					GL.Begin(GL.QUADS);
					GL.Color(faceColor);
					GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[2]); GL.Vertex(vec[3]);
					GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[7]);
					GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[1]);
					GL.Vertex(vec[1]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[2]);
					GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[3]);
					GL.Vertex(vec[3]); GL.Vertex(vec[7]); GL.Vertex(vec[4]); GL.Vertex(vec[0]);
					GL.End();
				}
			} // End 'mouse on gui'
		} // End 'draw preselect'
	}
}
