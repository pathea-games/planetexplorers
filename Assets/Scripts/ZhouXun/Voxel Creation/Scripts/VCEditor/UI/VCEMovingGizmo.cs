using UnityEngine;
using System.Collections;

public class VCEMovingGizmo : GLBehaviour
{
	public VCESelectComponent m_ParentBrush;
	private bool m_MaterialReplaced = false;
	private float m_VoxelSize = 0;
	public float m_Length = 10;
	public BoxCollider m_XCollider;
	public BoxCollider m_YCollider;
	public BoxCollider m_ZCollider;
	private bool m_XFocused;
	private bool m_YFocused;
	private bool m_ZFocused;
	private bool m_XDragging;
	private bool m_YDragging;
	private bool m_ZDragging;
	public bool Dragging { get { return m_XDragging || m_YDragging || m_ZDragging; } }
	private Vector3 m_LastPos;
	private Vector3 m_NowPos;
	private Vector3 m_MovingOffset;
	public Vector3 MovingOffset { get { return Dragging ? (m_MovingOffset) : Vector3.zero; } }
	
	private bool m_LastDragging = false;
	public delegate void DNotify ();
	public delegate void DMovingNotify (Vector3 ofs);
	public DNotify OnDragBegin = null;
	public DNotify OnDrop = null;
	public DMovingNotify OnMoving = null;

	public void ReplaceMat ()
	{
		if ( !m_MaterialReplaced )
		{
			m_Material = Material.Instantiate(m_Material) as Material;
			m_MaterialReplaced = true;
		}
	}

	// Use this for initialization
	void Start ()
	{
		m_VoxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		transform.localScale = Vector3.one * m_VoxelSize;
		m_XFocused = false;
		m_YFocused = false;
		m_ZFocused = false;
		m_XDragging = false;
		m_YDragging = false;
		m_ZDragging = false;
		m_LastDragging = false;
	}

	void OnEnable ()
	{
		m_VoxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		transform.localScale = Vector3.one * m_VoxelSize;
		m_XFocused = false;
		m_YFocused = false;
		m_ZFocused = false;
		m_XDragging = false;
		m_YDragging = false;
		m_ZDragging = false;
		m_LastDragging = false;
	}

	void OnDisable ()
	{
		m_ParentBrush.m_MouseOnGizmo = false;
		VCECamera.Instance.FreeView();
	}

	// Update is called once per frame
	void Update ()
	{
		ReplaceMat();
		m_VoxelSize = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		Vector3 camDir = (VCEditor.Instance.m_MainCamera.transform.position - this.transform.position);

		// Idle state
		if ( !Dragging )
		{
			m_XCollider.size = new Vector3 (m_Length*2+6, 1.2f, 1.2f);
			m_YCollider.size = new Vector3 (1.2f, m_Length*2+6, 1.2f);
			m_ZCollider.size = new Vector3 (1.2f, 1.2f, m_Length*2+6);
			
			if ( camDir.magnitude < m_VoxelSize )
			{
				m_XCollider.gameObject.SetActive(false);
				m_YCollider.gameObject.SetActive(false);
				m_ZCollider.gameObject.SetActive(false);
			}
			else
			{
				camDir.Normalize();
				m_XCollider.gameObject.SetActive(Mathf.Abs(camDir.x) < 0.93f);
				m_YCollider.gameObject.SetActive(Mathf.Abs(camDir.y) < 0.93f);
				m_ZCollider.gameObject.SetActive(Mathf.Abs(camDir.z) < 0.93f);
			}
			
			m_XFocused = false;
			m_YFocused = false;
			m_ZFocused = false;
			
			RaycastHit rchx = new RaycastHit ();
			RaycastHit rchy = new RaycastHit ();
			RaycastHit rchz = new RaycastHit ();
			bool xcast, ycast, zcast;
			if ( VCEInput.s_MouseOnUI )
			{
				xcast = ycast = zcast = false;
			}
			else
			{
				xcast = m_XCollider.Raycast(VCEInput.s_PickRay, out rchx, 100f);
				ycast = m_YCollider.Raycast(VCEInput.s_PickRay, out rchy, 100f);
				zcast = m_ZCollider.Raycast(VCEInput.s_PickRay, out rchz, 100f);
			}

			if ( xcast || ycast || zcast )
			{
				if ( !xcast ) rchx.distance = 100000;
				if ( !ycast ) rchy.distance = 100000;
				if ( !zcast ) rchz.distance = 100000;
				if ( xcast && rchx.distance <= rchy.distance && rchx.distance <= rchz.distance )
					m_XFocused = true;
				if ( ycast && rchy.distance <= rchx.distance && rchy.distance <= rchz.distance )
					m_YFocused = true;
				if ( zcast && rchz.distance <= rchy.distance && rchz.distance <= rchx.distance )
					m_ZFocused = true;

				m_ParentBrush.m_MouseOnGizmo = true;

				if ( Input.GetMouseButtonDown(0) )
				{
					if ( m_XFocused )
					{
						m_XDragging = true;
					}
					if ( m_YFocused )
					{
						m_YDragging = true;
					}
					if ( m_ZFocused )
					{
						m_ZDragging = true;
					}
				}
			}
			else
			{
				m_ParentBrush.m_MouseOnGizmo = false;
			}
		} // End 'Idle state'

		// Any time mouse up will exit the dragging mode
		if ( !Input.GetMouseButton(0) )
		{
			m_XDragging = false;
			m_YDragging = false;
			m_ZDragging = false;
			m_XCollider.size = new Vector3 (m_Length*2+6, 1.2f, 1.2f);
			m_YCollider.size = new Vector3 (1.2f, m_Length*2+6, 1.2f);
			m_ZCollider.size = new Vector3 (1.2f, 1.2f, m_Length*2+6);
		}

		// Enter dragging
		if ( Dragging && !m_LastDragging )
		{
			float expandsize = m_Length*500;
			// X Collider
			if ( m_XDragging )
			{
				if ( Mathf.Abs(camDir.y) < Mathf.Abs(camDir.z) )
					m_XCollider.size = new Vector3 (expandsize, expandsize, 1.2f);
				else
					m_XCollider.size = new Vector3 (expandsize, 1.2f, expandsize);
			}
			else
			{
				m_XCollider.size = new Vector3 (m_Length*2+6, 1.2f, 1.2f);
			}
			// Y Collider
			if ( m_YDragging )
			{
				if ( Mathf.Abs(camDir.x) < Mathf.Abs(camDir.z) )
					m_YCollider.size = new Vector3 (expandsize, expandsize, 1.2f);
				else
					m_YCollider.size = new Vector3 (1.2f, expandsize, expandsize);
			}
			else
			{
				m_YCollider.size = new Vector3 (1.2f, m_Length*2+6, 1.2f);
			}
			// Z Collider
			if ( m_ZDragging )
			{
				if ( Mathf.Abs(camDir.x) < Mathf.Abs(camDir.y) )
					m_ZCollider.size = new Vector3 (expandsize, 1.2f, expandsize);
				else
					m_ZCollider.size = new Vector3 (1.2f, expandsize, expandsize);
			}
			else
			{
				m_ZCollider.size = new Vector3 (1.2f, 1.2f, m_Length*2+6);
			}
			if ( OnDragBegin != null )
				OnDragBegin();
			VCECamera.Instance.FixView();
		} // End 'Enter dragging'

		// Exit dragging
		if ( !Dragging && m_LastDragging )
		{
			if ( OnDrop != null )
				OnDrop();
			VCECamera.Instance.FreeView();
		}

		// Dragging
		if ( Dragging )
		{
			m_ParentBrush.m_MouseOnGizmo = true;
			RaycastHit rch = new RaycastHit ();
			// X Dragging
			if ( m_XDragging && m_XCollider.Raycast(VCEInput.s_PickRay, out rch, 500f ) )
			{
				m_NowPos = rch.point;
			}
			// Y Dragging
			if ( m_YDragging && m_YCollider.Raycast(VCEInput.s_PickRay, out rch, 500f) )
			{
				m_NowPos = rch.point;
			}
			// Z Dragging
			if ( m_ZDragging && m_ZCollider.Raycast(VCEInput.s_PickRay, out rch, 500f) )
			{
				m_NowPos = rch.point;
			}
			if ( m_LastDragging )
			{
				m_MovingOffset = m_NowPos - m_LastPos;
				if ( !m_XDragging ) m_MovingOffset.x = 0;
				if ( !m_YDragging ) m_MovingOffset.y = 0;
				if ( !m_ZDragging ) m_MovingOffset.z = 0;
				if ( MovingOffset.magnitude > 0 && OnMoving != null )
					OnMoving(MovingOffset);
			}
			else
			{
				m_MovingOffset = Vector3.zero;
			}
			m_LastPos = m_NowPos;
		} // End 'Dragging'
		m_LastDragging = Dragging;
	}

	public override void OnGL ()
	{
		float len = m_Length * m_VoxelSize;
		float len_a = (m_Length-2) * m_VoxelSize;
		float len_b = (m_Length+2) * m_VoxelSize;
		Vector3 camDir = (VCEditor.Instance.m_MainCamera.transform.position - this.transform.position).normalized;
		float xintens = Mathf.Clamp01(1-Mathf.Pow((Mathf.Abs(camDir.x)-0.9204f)*100, 3));
		float yintens = Mathf.Clamp01(1-Mathf.Pow((Mathf.Abs(camDir.y)-0.9204f)*100, 3));
		float zintens = Mathf.Clamp01(1-Mathf.Pow((Mathf.Abs(camDir.z)-0.9204f)*100, 3));
		if ( m_XDragging ) xintens = 1;
		if ( m_YDragging ) yintens = 1;
		if ( m_ZDragging ) zintens = 1;

		for ( float i = 0; i < 1.99f; i += 0.25f )
		{
			float angle_rad = i*Mathf.PI;
			Vector3 ofs = new Vector3 (0, m_VoxelSize*Mathf.Cos(angle_rad), m_VoxelSize*Mathf.Sin(angle_rad)) * 1.5f;
			GL.Begin(GL.QUADS);
			GL.Color(Color.white * ((m_XFocused || m_XDragging) ? 0.8f : 0.2f * xintens));
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position - transform.right * len - ofs);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position - transform.right * len + ofs);
			GL.TexCoord2(1,1);
			GL.Vertex(transform.position + transform.right * len + ofs);
			GL.TexCoord2(1,0);
			GL.Vertex(transform.position + transform.right * len - ofs);
			GL.End();
			
			GL.Begin(GL.TRIANGLES);
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position - transform.right * len_a - ofs*1.2f);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position - transform.right * len_a + ofs*1.0f);
			GL.TexCoord2(1,0.5f);
			GL.Vertex(transform.position - transform.right * len_b);
			
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position + transform.right * len_a - ofs*1.2f);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position + transform.right * len_a + ofs*1.0f);
			GL.TexCoord2(1,0.5f);
			GL.Vertex(transform.position + transform.right * len_b);
			GL.End();
		}
		for ( float i = 0; i < 1.99f; i += 0.25f )
		{
			float angle_rad = i*Mathf.PI;
			Vector3 ofs = new Vector3 (m_VoxelSize*Mathf.Cos(angle_rad), 0, m_VoxelSize*Mathf.Sin(angle_rad)) * 1.5f;
			GL.Begin(GL.QUADS);
			GL.Color(Color.white * ((m_YFocused || m_YDragging) ? 0.8f : 0.2f * yintens));
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position - transform.up * len - ofs);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position - transform.up * len + ofs);
			GL.TexCoord2(1,1);
			GL.Vertex(transform.position + transform.up * len + ofs);
			GL.TexCoord2(1,0);
			GL.Vertex(transform.position + transform.up * len - ofs);
			GL.End();
			
			GL.Begin(GL.TRIANGLES);
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position - transform.up * len_a - ofs*1.2f);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position - transform.up * len_a + ofs*1.0f);
			GL.TexCoord2(1,0.5f);
			GL.Vertex(transform.position - transform.up * len_b);
			
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position + transform.up * len_a - ofs*1.2f);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position + transform.up * len_a + ofs*1.0f);
			GL.TexCoord2(1,0.5f);
			GL.Vertex(transform.position + transform.up * len_b);
			GL.End();
		}
		for ( float i = 0; i < 1.99f; i += 0.25f )
		{
			float angle_rad = i*Mathf.PI;
			Vector3 ofs = new Vector3 (m_VoxelSize*Mathf.Cos(angle_rad), m_VoxelSize*Mathf.Sin(angle_rad), 0) * 1.5f;
			GL.Begin(GL.QUADS);
			GL.Color(Color.white * ((m_ZFocused || m_ZDragging) ? 0.8f : 0.2f * zintens));
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position - transform.forward * len - ofs);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position - transform.forward * len + ofs);
			GL.TexCoord2(1,1);
			GL.Vertex(transform.position + transform.forward * len + ofs);
			GL.TexCoord2(1,0);
			GL.Vertex(transform.position + transform.forward * len - ofs);
			GL.End();
			
			GL.Begin(GL.TRIANGLES);
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position - transform.forward * len_a - ofs*1.2f);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position - transform.forward * len_a + ofs*1.0f);
			GL.TexCoord2(1,0.5f);
			GL.Vertex(transform.position - transform.forward * len_b);
			
			GL.TexCoord2(0,0);
			GL.Vertex(transform.position + transform.forward * len_a - ofs*1.2f);
			GL.TexCoord2(0,1);
			GL.Vertex(transform.position + transform.forward * len_a + ofs*1.0f);
			GL.TexCoord2(1,0.5f);
			GL.Vertex(transform.position + transform.forward * len_b);
			GL.End();
		}
	}
}
