using UnityEngine;
using System.Collections;

public class VCERotatingGizmo : GLBehaviour
{
	public VCESelectComponent m_ParentBrush;
	public int m_AxisMask = 7;
	private bool m_MaterialReplaced = false;
	private float m_VoxelSize = 0;
	public float m_Radius = 10;
	public MeshCollider m_XCollider;
	public MeshCollider m_YCollider;
	public MeshCollider m_ZCollider;
	private bool m_XFocused;
	private bool m_YFocused;
	private bool m_ZFocused;
	private bool m_XDragging;
	private bool m_YDragging;
	private bool m_ZDragging;
	public bool Dragging { get { return m_XDragging || m_YDragging || m_ZDragging; } }
	private Vector3 m_DragStartPos;
	private Vector3 m_DragScreenPos;
	private float m_DragStartAngle;
//	private float m_DragStartDist;
	private Vector3 m_DragIdentityDir;
	private Vector3 m_RotatingOffset;
	public Vector3 RotatingOffset { get { return Dragging ? (m_RotatingOffset) : Vector3.zero; } }
	
	private bool m_LastDragging = false;
	public delegate void DNotify ();
	public delegate void DRotatingNotify (Vector3 axis, float angle);
	public DNotify OnDragBegin = null;
	public DNotify OnDrop = null;
	public DRotatingNotify OnRotating = null;

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
			if ( camDir.magnitude < m_VoxelSize * m_Radius * 1.2f )
			{
				m_XCollider.gameObject.SetActive(false);
				m_YCollider.gameObject.SetActive(false);
				m_ZCollider.gameObject.SetActive(false);
			}
			else
			{
				m_XCollider.gameObject.SetActive((m_AxisMask & 1) > 0);
				m_YCollider.gameObject.SetActive((m_AxisMask & 2) > 0);
				m_ZCollider.gameObject.SetActive((m_AxisMask & 4) > 0);
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
				else if ( ycast && rchy.distance <= rchx.distance && rchy.distance <= rchz.distance )
					m_YFocused = true;
				else if ( zcast && rchz.distance <= rchy.distance && rchz.distance <= rchx.distance )
					m_ZFocused = true;
				
				m_ParentBrush.m_MouseOnGizmo = true;
				
				if ( Input.GetMouseButtonDown(0) )
				{
					if ( m_XFocused )
					{
						m_DragStartPos = rchx.point;
						//m_DragStartDist = rchx.distance;
						m_DragIdentityDir = Vector3.Cross(Vector3.right, m_DragStartPos - transform.position).normalized * m_VoxelSize * 3;
						Vector3 sca = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos);
						Vector3 scb = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos + m_DragIdentityDir);
						m_DragIdentityDir = (scb - sca).normalized;
						m_DragScreenPos = Input.mousePosition;
						Vector3 ldpos = m_DragStartPos - transform.position;
						m_DragStartAngle = Mathf.Atan2(ldpos.z, ldpos.y) * Mathf.Rad2Deg;
						if ( m_DragStartAngle < 0 )
							m_DragStartAngle += 360;
						m_XDragging = true;
					}
					else if ( m_YFocused )
					{
						m_DragStartPos = rchy.point;
						//m_DragStartDist = rchy.distance;
						m_DragIdentityDir = Vector3.Cross(Vector3.down, m_DragStartPos - transform.position).normalized * m_VoxelSize * 3;
						Vector3 sca = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos);
						Vector3 scb = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos + m_DragIdentityDir);
						m_DragIdentityDir = (scb - sca).normalized;
						m_DragScreenPos = Input.mousePosition;
						Vector3 ldpos = m_DragStartPos - transform.position;
						m_DragStartAngle = Mathf.Atan2(ldpos.z, ldpos.x) * Mathf.Rad2Deg;
						if ( m_DragStartAngle < 0 )
							m_DragStartAngle += 360;
						m_YDragging = true;
					}
					else if ( m_ZFocused )
					{
						m_DragStartPos = rchz.point;
						//m_DragStartDist = rchz.distance;
						m_DragIdentityDir = Vector3.Cross(Vector3.forward, m_DragStartPos - transform.position).normalized * m_VoxelSize * 3;
						Vector3 sca = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos);
						Vector3 scb = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(m_DragStartPos + m_DragIdentityDir);
						m_DragIdentityDir = (scb - sca).normalized;
						m_DragScreenPos = Input.mousePosition;
						Vector3 ldpos = m_DragStartPos - transform.position;
						m_DragStartAngle = Mathf.Atan2(ldpos.y, ldpos.x) * Mathf.Rad2Deg;
						if ( m_DragStartAngle < 0 )
							m_DragStartAngle += 360;
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
		}

		// Enter dragging
		if ( Dragging && !m_LastDragging )
		{
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
			Vector3 mpos = Input.mousePosition;
			Vector3 mofs = mpos - m_DragScreenPos;
			float drag_offset = Mathf.RoundToInt(Vector3.Dot(mofs, m_DragIdentityDir) * 0.3f);

			if ( m_XDragging )
				m_RotatingOffset = new Vector3 (drag_offset,0,0);
			if ( m_YDragging )
				m_RotatingOffset = new Vector3 (0,drag_offset,0);
			if ( m_ZDragging )
				m_RotatingOffset = new Vector3 (0,0,drag_offset);

			if ( m_XDragging )
				OnRotating(Vector3.right, m_RotatingOffset.x);
			if ( m_YDragging )
				OnRotating(Vector3.up, -m_RotatingOffset.y);
			if ( m_ZDragging )
				OnRotating(Vector3.forward, m_RotatingOffset.z);
		} // End 'Dragging'
		m_LastDragging = Dragging;
	}

	void OnGUI ()
	{
		if ( Dragging )
		{
			GUI.skin = VCEditor.Instance.m_GUISkin;
			GUI.color = new Color (1,1,1,0.7f);
			Vector3 pos = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(transform.position);
			if ( m_RotatingOffset.magnitude > 0.01f )
			{
				string label_text = "";
				if ( m_XDragging )
				{
					label_text = m_RotatingOffset.x.ToString("0");
					if ( m_RotatingOffset.x > 0 )
						label_text = "+" + label_text;
					GUI.Label( new Rect(pos.x - 50, (Screen.height - pos.y) - 50,100,100), label_text, "CursorText3" );
				}
				if ( m_YDragging )
				{
					label_text = (-m_RotatingOffset.y).ToString("0");
					if ( m_RotatingOffset.y < 0 )
						label_text = "+" + label_text;
					GUI.Label( new Rect(pos.x - 50, (Screen.height - pos.y) - 50,100,100), label_text, "CursorText3" );
				}
				if ( m_ZDragging )
				{
					label_text = m_RotatingOffset.z.ToString("0");
					if ( m_RotatingOffset.z > 0 )
						label_text = "+" + label_text;
					GUI.Label( new Rect(pos.x - 50, (Screen.height - pos.y) - 50,100,100), label_text, "CursorText3" );
				}
			}
		}
	}

	public override void OnGL ()
	{
		float r = m_Radius * m_VoxelSize;
		float thickness = m_VoxelSize;
		float rad_seg = 5.0f;

		Vector3 camDir = (VCEditor.Instance.m_MainCamera.transform.position - this.transform.position);
		if ( camDir.magnitude < m_VoxelSize * m_Radius * 1.2f ) return;

		GL.Begin(GL.QUADS);
		// X
		if ( m_XCollider.gameObject.activeInHierarchy )
		{
			for ( float deg = 0; deg < 359.5f; deg += rad_seg )
			{
				bool solid = false;
				if ( m_XDragging )
				{
					float sa = m_DragStartAngle;
					float sb = sa + m_RotatingOffset.x;
					while (sb - sa > 360)
						sb -= 360;
					while (sa - sb > 360)
						sb += 360;
					
					float min = Mathf.Min(sa, sb);
					float max = Mathf.Max(sa, sb);
					float d = deg + rad_seg * 0.5f;
					if ( min <= d && d <= max || min <= d+360 && d+360 <= max || min <= d-360 && d-360 <= max )
						solid = true;

					if ( solid )
					{
						GL.Color(Color.white);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position + r*new Vector3(0, Mathf.Cos(deg*Mathf.Deg2Rad), Mathf.Sin(deg*Mathf.Deg2Rad)));
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position + r*new Vector3(0, Mathf.Cos((deg+rad_seg)*Mathf.Deg2Rad), Mathf.Sin((deg+rad_seg)*Mathf.Deg2Rad)));
					}
				}
				for ( float t = 0; t < 0.99f; t += 0.25f )
				{
					float circle_rad_a = deg*Mathf.Deg2Rad;
					float circle_rad_b = (deg+rad_seg)*Mathf.Deg2Rad;
					float tilt_rad = t*Mathf.PI;
					
					float offp = thickness*Mathf.Cos(tilt_rad);
					float offq = thickness*Mathf.Sin(tilt_rad);
					Vector3 A1,A2,B1,B2;
					A1 = new Vector3(offq, (r+offp)*Mathf.Cos(circle_rad_a), (r+offp)*Mathf.Sin(circle_rad_a));
					A2 = new Vector3(-offq, (r-offp)*Mathf.Cos(circle_rad_a), (r-offp)*Mathf.Sin(circle_rad_a));
					B1 = new Vector3(offq, (r+offp)*Mathf.Cos(circle_rad_b), (r+offp)*Mathf.Sin(circle_rad_b));
					B2 = new Vector3(-offq, (r-offp)*Mathf.Cos(circle_rad_b), (r-offp)*Mathf.Sin(circle_rad_b));
					
					A1 += transform.position;
					A2 += transform.position;
					B1 += transform.position;
					B2 += transform.position;
					
					GL.Color(Color.white * ((m_XFocused || m_XDragging) ? 0.8f : 0.2f));
					if ( m_YDragging || m_ZDragging )
						GL.Color(Color.white * 0.05f);

					GL.TexCoord2(solid ? 0.5f : 0f,0);
					GL.Vertex(A1);
					GL.TexCoord2(solid ? 0.5f : 0f,1);
					GL.Vertex(A2);
					GL.TexCoord2(solid ? 0.5f : 1f,1);
					GL.Vertex(B2);
					GL.TexCoord2(solid ? 0.5f : 1f,0);
					GL.Vertex(B1);
				}
			}
		}
		if ( m_YCollider.gameObject.activeInHierarchy )
		{
			// Y
			for ( float deg = 0; deg < 360f; deg += rad_seg )
			{
				bool solid = false;
				if ( m_YDragging )
				{
					float sa = m_DragStartAngle;
					float sb = sa + m_RotatingOffset.y;
					while (sb - sa > 360)
						sb -= 360;
					while (sa - sb > 360)
						sb += 360;
					
					float min = Mathf.Min(sa, sb);
					float max = Mathf.Max(sa, sb);
					float d = deg + rad_seg * 0.5f;
					if ( min <= d && d <= max || min <= d+360 && d+360 <= max || min <= d-360 && d-360 <= max )
						solid = true;
					
					if ( solid )
					{
						GL.Color(Color.white);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position + r*new Vector3(Mathf.Cos(deg*Mathf.Deg2Rad), 0, Mathf.Sin(deg*Mathf.Deg2Rad)));
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position + r*new Vector3(Mathf.Cos((deg+rad_seg)*Mathf.Deg2Rad), 0, Mathf.Sin((deg+rad_seg)*Mathf.Deg2Rad)));
					}
				}
				for ( float t = 0; t < 0.99f; t += 0.25f )
				{
					float circle_rad_a = deg*Mathf.Deg2Rad;
					float circle_rad_b = (deg+rad_seg)*Mathf.Deg2Rad;
					float tilt_rad = t*Mathf.PI;
					
					float offp = thickness*Mathf.Cos(tilt_rad);
					float offq = thickness*Mathf.Sin(tilt_rad);
					Vector3 A1,A2,B1,B2;
					A1 = new Vector3((r+offp)*Mathf.Cos(circle_rad_a),  offq, (r+offp)*Mathf.Sin(circle_rad_a));
					A2 = new Vector3((r-offp)*Mathf.Cos(circle_rad_a), -offq, (r-offp)*Mathf.Sin(circle_rad_a));
					B1 = new Vector3((r+offp)*Mathf.Cos(circle_rad_b),  offq, (r+offp)*Mathf.Sin(circle_rad_b));
					B2 = new Vector3((r-offp)*Mathf.Cos(circle_rad_b), -offq, (r-offp)*Mathf.Sin(circle_rad_b));

					A1 += transform.position;
					A2 += transform.position;
					B1 += transform.position;
					B2 += transform.position;
					
					GL.Color(Color.white * ((m_YFocused || m_YDragging) ? 0.8f : 0.2f));
					if ( m_XDragging || m_ZDragging )
						GL.Color(Color.white * 0.05f);

					GL.TexCoord2(solid ? 0.5f : 0f,0);
					GL.Vertex(A1);
					GL.TexCoord2(solid ? 0.5f : 0f,1);
					GL.Vertex(A2);
					GL.TexCoord2(solid ? 0.5f : 1f,1);
					GL.Vertex(B2);
					GL.TexCoord2(solid ? 0.5f : 1f,0);
					GL.Vertex(B1);
				}
			}
		}
		if ( m_ZCollider.gameObject.activeInHierarchy )
		{
			// Z
			for ( float deg = 0; deg < 360f; deg += rad_seg )
			{
				bool solid = false;
				if ( m_ZDragging )
				{
					float sa = m_DragStartAngle;
					float sb = sa + m_RotatingOffset.z;
					while (sb - sa > 360)
						sb -= 360;
					while (sa - sb > 360)
						sb += 360;
					
					float min = Mathf.Min(sa, sb);
					float max = Mathf.Max(sa, sb);
					float d = deg + rad_seg * 0.5f;
					if ( min <= d && d <= max || min <= d+360 && d+360 <= max || min <= d-360 && d-360 <= max )
						solid = true;
					
					if ( solid )
					{
						GL.Color(Color.white);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position);
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position + r*new Vector3(Mathf.Cos(deg*Mathf.Deg2Rad), Mathf.Sin(deg*Mathf.Deg2Rad), 0));
						GL.TexCoord2(0.55f,0.55f);
						GL.Vertex(transform.position + r*new Vector3(Mathf.Cos((deg+rad_seg)*Mathf.Deg2Rad), Mathf.Sin((deg+rad_seg)*Mathf.Deg2Rad), 0));
					}
				}
				for ( float t = 0; t < 0.99f; t += 0.25f )
				{
					float circle_rad_a = deg*Mathf.Deg2Rad;
					float circle_rad_b = (deg+rad_seg)*Mathf.Deg2Rad;
					float tilt_rad = t*Mathf.PI;
					
					float offp = thickness*Mathf.Cos(tilt_rad);
					float offq = thickness*Mathf.Sin(tilt_rad);
					Vector3 A1,A2,B1,B2;
					A1 = new Vector3((r+offp)*Mathf.Cos(circle_rad_a), (r+offp)*Mathf.Sin(circle_rad_a),  offq);
					A2 = new Vector3((r-offp)*Mathf.Cos(circle_rad_a), (r-offp)*Mathf.Sin(circle_rad_a), -offq);
					B1 = new Vector3((r+offp)*Mathf.Cos(circle_rad_b), (r+offp)*Mathf.Sin(circle_rad_b),  offq);
					B2 = new Vector3((r-offp)*Mathf.Cos(circle_rad_b), (r-offp)*Mathf.Sin(circle_rad_b), -offq);

					A1 += transform.position;
					A2 += transform.position;
					B1 += transform.position;
					B2 += transform.position;
					
					GL.Color(Color.white * ((m_ZFocused || m_ZDragging) ? 0.8f : 0.2f));
					if ( m_XDragging || m_YDragging )
						GL.Color(Color.white * 0.05f);

					GL.TexCoord2(solid ? 0.5f : 0f,0);
					GL.Vertex(A1);
					GL.TexCoord2(solid ? 0.5f : 0f,1);
					GL.Vertex(A2);
					GL.TexCoord2(solid ? 0.5f : 1f,1);
					GL.Vertex(B2);
					GL.TexCoord2(solid ? 0.5f : 1f,0);
					GL.Vertex(B1);
				}
			}
		}
		GL.End();
	}
}
