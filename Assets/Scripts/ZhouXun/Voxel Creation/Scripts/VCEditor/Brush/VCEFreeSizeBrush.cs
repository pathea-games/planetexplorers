using UnityEngine;
using System.Collections;

public abstract class VCEFreeSizeBrush : VCEBrush
{
	protected VCEMath.DrawTarget m_Target;
	
	public enum EPhase
	{
		Free,
		DragPlane,
		AdjustHeight,
		Drawing
	}
	
	protected EPhase m_Phase = EPhase.Free;
	protected Vector3 m_PointBeforeAdjustHeight;
	protected IntVector3 m_Begin;
	protected IntVector3 m_End;
	protected IntVector3 Min 
	{
		get
		{
			if ( m_Begin == null || m_End == null )
				return null;
			return new IntVector3 (Mathf.Min(m_Begin.x, m_End.x), 
				                   Mathf.Min(m_Begin.y, m_End.y), 
				                   Mathf.Min(m_Begin.z, m_End.z));
		}
	}
	protected IntVector3 Max 
	{
		get
		{
			if ( m_Begin == null || m_End == null )
				return null;
			return new IntVector3 (Mathf.Max(m_Begin.x, m_End.x), 
				                   Mathf.Max(m_Begin.y, m_End.y), 
				                   Mathf.Max(m_Begin.z, m_End.z));
		}
	}
	protected IntVector3 Size 
	{
		get
		{
			if ( m_Begin == null || m_End == null )
				return null;
			return Max - Min + IntVector3.One;
		}
	}
	
	// Gizmo
	public VCEGizmoCubeMesh m_GizmoCube;
	
	// Use this for initialization
	protected void Start ()
	{
		m_Target = new VCEMath.DrawTarget ();
		m_Begin = null;
		m_End = null;
		m_GizmoCube.m_Shrink = -0.04f;
		VCEStatusBar.ShowText(BrushDesc().ToLocalizationString(), 7, true);
	}

	// Update is called once per frame
	protected void Update ()
	{
		if ( !VCEditor.DocumentOpen()
		  || VCEditor.SelectedVoxelType < 0 )
		{
			m_Phase = EPhase.Free;
			m_GizmoCube.gameObject.SetActive(false);
			return;
		}
		
		float voxel_size = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_GizmoCube.m_VoxelSize = voxel_size;

		ExtraAdjust();
		
		if ( m_Phase == EPhase.Free )
		{
			if ( !VCEInput.s_MouseOnUI )
			{
				// Cancel
				if ( VCEInput.s_Cancel )
				{
					ResetDrawing();
					Cancel();
				}
				VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out m_Target, VCEMath.MC_ISO_VALUE);
				if ( VCEditor.s_Scene.m_IsoData.IsPointIn(m_Target.cursor) )
				{
					m_GizmoCube.CubeSize = IntVector3.One;
					m_GizmoCube.transform.position = m_Target.cursor.ToVector3() * voxel_size;
					m_GizmoCube.gameObject.SetActive(true);
					if ( Input.GetMouseButtonDown(0) )
					{
						m_Begin = new IntVector3(m_Target.cursor);
						m_End = new IntVector3(m_Target.cursor);
						m_Phase = EPhase.DragPlane;
						VCEditor.Instance.m_UI.DisableFunctions();
						VCEditor.s_ProtectLock0 = true;
						VCEStatusBar.ShowText("Drag an area".ToLocalizationString(), 2);
					}
				}
				else
				{
					m_GizmoCube.gameObject.SetActive(false);
				}
			}
			else
			{
				m_GizmoCube.gameObject.SetActive(false);
			}
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
		}
		else if ( m_Phase == EPhase.DragPlane )
		{
			// Cancel
			if ( VCEInput.s_Cancel )
			{
				ResetDrawing();
			}
			RaycastHit rch;
            if (VCEMath.RayCastCoordPlane(VCEInput.s_PickRay, ECoordPlane.XZ, m_Begin.y, out rch))
            {
                m_End = new IntVector3(Mathf.FloorToInt(rch.point.x), m_Begin.y, Mathf.FloorToInt(rch.point.z));
                VCEditor.s_Scene.m_IsoData.ClampPointI(m_End, m_Begin.x < 0.5f * VCEditor.s_Scene.m_IsoData.m_HeadInfo.xSize);
                m_PointBeforeAdjustHeight = rch.point;
                VCEditor.s_Scene.m_IsoData.ClampPointI(m_End, m_Begin.x < 0.5f * VCEditor.s_Scene.m_IsoData.m_HeadInfo.xSize);
                m_GizmoCube.CubeSize = Size;
                m_GizmoCube.transform.position = Min.ToVector3() * voxel_size;
            }
			else
			{
				ResetDrawing();
			}
			if ( Input.GetMouseButtonUp(0) )
			{
				m_Phase = EPhase.AdjustHeight;
				VCEStatusBar.ShowText("Adjust height".ToLocalizationString(), 2);
			}
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
		}
		else if ( m_Phase == EPhase.AdjustHeight )
		{
			// Cancel
			if ( VCEInput.s_Cancel )
			{
				ResetDrawing();
			}
			
			float height = m_PointBeforeAdjustHeight.y;
			VCEMath.RayAdjustHeight(VCEInput.s_PickRay, m_PointBeforeAdjustHeight, out height);
			m_End.y = Mathf.FloorToInt(height + 0.3f);

			VCEditor.s_Scene.m_IsoData.ClampPointI(m_End);
			m_GizmoCube.CubeSize = Size;
			m_GizmoCube.transform.position = Min.ToVector3() * voxel_size;
			
			// Do the brush
			if ( Input.GetMouseButtonDown(0) )
			{
				m_Phase = EPhase.Drawing;
				Do();
				VCEStatusBar.ShowText("Done".ToLocalizationString(), 2);
			}
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
		}
	}
	
	protected void ResetDrawing ()
	{
		m_Phase = EPhase.Free;
		m_GizmoCube.gameObject.SetActive(false);
		m_GizmoCube.CubeSize = IntVector3.One;
		VCEditor.Instance.m_UI.EnableFunctions();
		VCEditor.s_ProtectLock0 = false;
	}
	
	public override void Cancel ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			VCEditor.Instance.m_UI.m_DefaultBrush_Material.isChecked = true;
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
		}
	}

	protected abstract string BrushDesc ();
	protected virtual void ExtraAdjust () {}
	protected virtual void ExtraGUI () {}

	void OnGUI ()
	{
		GUI.skin = VCEditor.Instance.m_GUISkin;
		if ( m_Phase != EPhase.Free )
		{
			IntVector3 size = Size;
			string text = size.x.ToString() + " x " + size.z.ToString() + " x " + size.y.ToString();
			GUI.color = new Color(1,1,1,0.8f);
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 26, 100,100), text, "CursorText2" );
		}
		ExtraGUI();
	}
}
