using UnityEngine;
using System.Collections;

public class GLBound : GLBehaviour
{
	public Bounds m_Bound;
	public bool m_ShowLine = true;
	public Color m_LineColor;
	public bool m_ShowFace = true;
	public Color m_FaceColor;
	
	public override void OnGL ()
	{
		if ( m_ShowLine )
		{
			GL.Begin(GL.LINES);
			GL.Color(m_LineColor);
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.max.z );
			
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.max.z );
			
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.max.z );
			GL.End();
		}
		if ( m_ShowFace )
		{
			GL.Begin(GL.QUADS);
			GL.Color(m_FaceColor);
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.min.z );
			
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.min.z );
			
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.min.z );
			
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.min.z );
			
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.min.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.min.z );
			
			GL.Vertex3( m_Bound.min.x, m_Bound.min.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.min.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.max.y, m_Bound.max.z );
			GL.Vertex3( m_Bound.max.x, m_Bound.min.y, m_Bound.max.z );
			GL.End();
		}
	}
}
