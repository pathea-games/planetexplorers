using UnityEngine;
using System.Collections;

public class GLComponentBound : GLBehaviour
{
	public VCEComponentTool m_ParentComponent;
	public Color m_BoundColor = s_Green;
	public bool m_Highlight = false;
	public static Color s_Green = new Color (0.5f, 1.0f, 0.0f, 1.0f);
	public static Color s_Red = new Color (1.0f, 0.0f, 0.135f, 1.0f);
	public static Color s_Blue = new Color (0.0f, 0.385f, 1.0f, 1.0f);
	public static Color s_Orange = new Color (1.0f, 0.5f, 0.0f, 1.0f);
	public static Color s_Yellow = new Color (1.0f, 1.0f, 0.0f, 1.0f);
	public override void OnGL ()
	{
		Vector3[] vec = new Vector3 [8]
		{
			new Vector3(  0.5f,  0.5f,  0.5f),
			new Vector3( -0.5f,  0.5f,  0.5f),
			new Vector3( -0.5f, -0.5f,  0.5f),
			new Vector3(  0.5f, -0.5f,  0.5f),
			new Vector3(  0.5f,  0.5f, -0.5f),
			new Vector3( -0.5f,  0.5f, -0.5f),
			new Vector3( -0.5f, -0.5f, -0.5f),
			new Vector3(  0.5f, -0.5f, -0.5f)
		};
		for ( int i = 0; i < 8; ++i )
			vec[i] = transform.TransformPoint(vec[i]);
		
		Color lineColor = m_BoundColor;
		Color faceColor = m_BoundColor;
		
		lineColor.a = 1.0f;
		faceColor *= m_Highlight ? 1f : 0.65f;
		faceColor.a = 1.0f;
		faceColor.a *= m_Highlight ? (0.25f + Mathf.Sin(Time.time*6f) * 0.1f) : 0.1f;
		
		GL.Begin(GL.LINES);
		GL.Color(lineColor);
		GL.Vertex(vec[0]);
		GL.Vertex(vec[1]);
		GL.Vertex(vec[1]);
		GL.Vertex(vec[2]);
		GL.Vertex(vec[2]);
		GL.Vertex(vec[3]);
		GL.Vertex(vec[3]);
		GL.Vertex(vec[0]);
		GL.Vertex(vec[4]);
		GL.Vertex(vec[5]);
		GL.Vertex(vec[5]);
		GL.Vertex(vec[6]);
		GL.Vertex(vec[6]);
		GL.Vertex(vec[7]);
		GL.Vertex(vec[7]);
		GL.Vertex(vec[4]);
		GL.Vertex(vec[0]);
		GL.Vertex(vec[4]);
		GL.Vertex(vec[1]);
		GL.Vertex(vec[5]);
		GL.Vertex(vec[2]);
		GL.Vertex(vec[6]);
		GL.Vertex(vec[3]);
		GL.Vertex(vec[7]);
		GL.End();
		
		GL.Begin(GL.QUADS);
		GL.Color(faceColor);
		GL.Vertex(vec[0]);
		GL.Vertex(vec[1]);
		GL.Vertex(vec[2]);
		GL.Vertex(vec[3]);
		GL.Vertex(vec[4]);
		GL.Vertex(vec[5]);
		GL.Vertex(vec[6]);
		GL.Vertex(vec[7]);
		GL.Vertex(vec[0]);
		GL.Vertex(vec[4]);
		GL.Vertex(vec[5]);
		GL.Vertex(vec[1]);
		GL.Vertex(vec[1]);
		GL.Vertex(vec[5]);
		GL.Vertex(vec[6]);
		GL.Vertex(vec[2]);
		GL.Vertex(vec[2]);
		GL.Vertex(vec[6]);
		GL.Vertex(vec[7]);
		GL.Vertex(vec[3]);
		GL.Vertex(vec[3]);
		GL.Vertex(vec[7]);
		GL.Vertex(vec[4]);
		GL.Vertex(vec[0]);
		GL.End();
	}
}
