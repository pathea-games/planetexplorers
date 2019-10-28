using UnityEngine;
using System.Collections;

public class GLMassCenter : GLBehaviour
{
	public GUISkin GSkin;
	public Color m_LineXColor = Color.white;
	public Color m_LineYColor = Color.white;
	public Color m_LineZColor = Color.white;

	private float vs = 1;
	private Vector3 max = Vector3.zero;
	public override void OnGL ()
	{
		vs = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		float length = vs * 10;
		Color lx = m_LineXColor;
		Color ly = m_LineYColor;
		Color lz = m_LineZColor;
		lx.a = 0.3f;
		ly.a = 0.3f;
		lz.a = 0.3f;
		GL.Begin(GL.LINES);
		GL.Color(lx);
		GL.Vertex3( 0, transform.localPosition.y, transform.localPosition.z );
		GL.Vertex3( max.x, transform.localPosition.y, transform.localPosition.z );
		GL.Color(ly);
		GL.Vertex3( transform.localPosition.x, 0, transform.localPosition.z );
		GL.Vertex3( transform.localPosition.x, max.y, transform.localPosition.z );
		GL.Color(lz);
		GL.Vertex3( transform.localPosition.x, transform.localPosition.y, 0 );
		GL.Vertex3( transform.localPosition.x, transform.localPosition.y, max.z );
		GL.Color(m_LineXColor);
		GL.Vertex( transform.localPosition + Vector3.right * length );
		GL.Vertex( transform.localPosition + Vector3.left * length );
		GL.Color(m_LineYColor);
		GL.Vertex( transform.localPosition + Vector3.up * length );
		GL.Vertex( transform.localPosition + Vector3.down * length );
		GL.Color(m_LineZColor);
		GL.Vertex( transform.localPosition + Vector3.forward * length );
		GL.Vertex( transform.localPosition + Vector3.back * length );
		GL.End();
	}

	void OnGUI ()
	{
//		if ( VCEditor.Instance.m_UI.m_ISOTab.isChecked )
//			return;
//		if ( VCEditor.Instance.m_UI.m_PaintTab.isChecked )
//			return;
//		GUI.skin = GSkin;
//		GUI.color = Color.yellow;
//		Vector3 screen_pos = VCEditor.Instance.m_MainCamera.WorldToScreenPoint(transform.position);
//		GUI.Label(new Rect(Mathf.Round(screen_pos.x*0.2f+1)*5, Mathf.Round((Screen.height - screen_pos.y)*0.2f+1)*5, 150, 50), "Mass Center", "CursorText2");
	}
}
