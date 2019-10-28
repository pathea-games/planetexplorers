using UnityEngine;
using System.Collections;

public class GLMirrorPlane : GLBehaviour
{
	public VCEMirrorGL m_Parent;
	public ECoordPlane m_Plane;
	public Color m_LineColor = Color.white;
	public Color m_PlaneColor = new Color(0.1f,0.1f,0.1f,0.1f);

	private Vector3 max = Vector3.zero;
	public override void OnGL ()
	{
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		// X Plane
		if ( m_Plane == ECoordPlane.ZY )
		{
			GL.Begin(GL.QUADS);
			GL.Color(m_PlaneColor);
			GL.Vertex3( transform.localPosition.x, 0, 0 );
			GL.Vertex3( transform.localPosition.x, 0, max.z );
			GL.Vertex3( transform.localPosition.x, max.y, max.z );
			GL.Vertex3( transform.localPosition.x, max.y, 0 );
			GL.End();
			
			GL.Begin(GL.LINES);
			GL.Color(m_LineColor);
			GL.Vertex3( transform.localPosition.x, 0, 0 );
			GL.Vertex3( transform.localPosition.x, 0, max.z );
			GL.Vertex3( transform.localPosition.x, 0, max.z );
			GL.Vertex3( transform.localPosition.x, max.y, max.z );
			GL.Vertex3( transform.localPosition.x, max.y, max.z );
			GL.Vertex3( transform.localPosition.x, max.y, 0 );
			GL.Vertex3( transform.localPosition.x, max.y, 0 );
			GL.Vertex3( transform.localPosition.x, 0, 0 );

			foreach ( float y in m_Parent.m_Ys )
			{
				GL.Color(new Color(1,1,0.5f,1));
				GL.Vertex3( transform.localPosition.x, y, 0 );
				GL.Vertex3( transform.localPosition.x, y, max.z );
			}
			GL.End();
		}
		// Y Plane
		else if ( m_Plane == ECoordPlane.XZ )
		{
			GL.Begin(GL.QUADS);
			GL.Color(m_PlaneColor);
			GL.Vertex3( 0, transform.localPosition.y, 0 );
			GL.Vertex3( 0, transform.localPosition.y, max.z );
			GL.Vertex3( max.x, transform.localPosition.y, max.z );
			GL.Vertex3( max.x, transform.localPosition.y, 0 );
			GL.End();

			GL.Begin(GL.LINES);
			GL.Color(m_LineColor);
			GL.Vertex3( 0, transform.localPosition.y, 0 );
			GL.Vertex3( 0, transform.localPosition.y, max.z );
			GL.Vertex3( 0, transform.localPosition.y, max.z );
			GL.Vertex3( max.x, transform.localPosition.y, max.z );
			GL.Vertex3( max.x, transform.localPosition.y, max.z );
			GL.Vertex3( max.x, transform.localPosition.y, 0 );
			GL.Vertex3( max.x, transform.localPosition.y, 0 );
			GL.Vertex3( 0, transform.localPosition.y, 0 );

			GL.End();
		}
		// Z Plane
		else if ( m_Plane == ECoordPlane.XY )
		{
			GL.Begin(GL.QUADS);
			GL.Color(m_PlaneColor);
			GL.Vertex3( 0, 0, transform.localPosition.z );
			GL.Vertex3( 0, max.y, transform.localPosition.z );
			GL.Vertex3( max.x, max.y, transform.localPosition.z );
			GL.Vertex3( max.x, 0, transform.localPosition.z );
			GL.End();

			GL.Begin(GL.LINES);
			GL.Color(m_LineColor);
			GL.Vertex3( 0, 0, transform.localPosition.z );
			GL.Vertex3( 0, max.y, transform.localPosition.z );
			GL.Vertex3( 0, max.y, transform.localPosition.z );
			GL.Vertex3( max.x, max.y, transform.localPosition.z );
			GL.Vertex3( max.x, max.y, transform.localPosition.z );
			GL.Vertex3( max.x, 0, transform.localPosition.z );
			GL.Vertex3( max.x, 0, transform.localPosition.z );
			GL.Vertex3( 0, 0, transform.localPosition.z );
			
			foreach ( float x in m_Parent.m_Xs )
			{
				GL.Color(new Color(0.5f,0.5f,1,1));
				GL.Vertex3( x, 0, transform.localPosition.z );
				GL.Vertex3( x, max.y, transform.localPosition.z );
			}
			foreach ( float y in m_Parent.m_Ys )
			{
				GL.Color(new Color(0.5f,1,1,0.8f));
				GL.Vertex3( 0, y, transform.localPosition.z );
				GL.Vertex3( max.x, y, transform.localPosition.z );
			}
			GL.End();
		}
	}
}
