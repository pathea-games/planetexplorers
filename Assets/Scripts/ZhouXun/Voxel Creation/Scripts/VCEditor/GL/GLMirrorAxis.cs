using UnityEngine;
using System.Collections;

public class GLMirrorAxis : GLBehaviour
{
	public VCEMirrorGL m_Parent;
	public ECoordAxis m_Axis;
	public Color m_LineColor = Color.white;

	public void Update ()
	{
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		LineRenderer lr = GetComponent<LineRenderer>();
		lr.SetWidth(VCEditor.s_Scene.m_Setting.m_VoxelSize * 2, VCEditor.s_Scene.m_Setting.m_VoxelSize * 2);
		if ( m_Axis == ECoordAxis.X )
			lr.SetPosition(1, new Vector3(max.x, 0,0));
		else if ( m_Axis == ECoordAxis.Y )
			lr.SetPosition(1, new Vector3(0, max.y,0));
		else if ( m_Axis == ECoordAxis.Z )
			lr.SetPosition(1, new Vector3(0,0, max.z));
	}

	private Vector3 max = Vector3.zero;
	public override void OnGL ()
	{
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		// X Plane
		if ( m_Axis == ECoordAxis.X )
		{
			GL.Begin(GL.LINES);
			GL.Color(m_LineColor);
			GL.Vertex3( 0, 0, transform.localPosition.z );
			GL.Vertex3( 0, max.y, transform.localPosition.z );
			GL.Vertex3( 0, transform.localPosition.y, 0 );
			GL.Vertex3( 0, transform.localPosition.y, max.z );
			GL.Vertex3( max.x, 0, transform.localPosition.z );
			GL.Vertex3( max.x, max.y, transform.localPosition.z );
			GL.Vertex3( max.x, transform.localPosition.y, 0 );
			GL.Vertex3( max.x, transform.localPosition.y, max.z );
			foreach ( float x in m_Parent.m_Xs )
			{
				GL.Vertex3( x, 0, transform.localPosition.z );
				GL.Vertex3( x, max.y, transform.localPosition.z );
				GL.Vertex3( x, transform.localPosition.y, 0 );
				GL.Vertex3( x, transform.localPosition.y, max.z );
			}
			GL.End();
		}
		// Y Plane
		else if ( m_Axis == ECoordAxis.Y )
		{
			GL.Begin(GL.LINES);
			GL.Color(m_LineColor);
			GL.Vertex3( 0, 0, transform.localPosition.z );
			GL.Vertex3( max.x, 0, transform.localPosition.z );
			GL.Vertex3( transform.localPosition.x, 0, 0 );
			GL.Vertex3( transform.localPosition.x, 0, max.z );
			GL.Vertex3( 0, max.y, transform.localPosition.z );
			GL.Vertex3( max.x, max.y, transform.localPosition.z );
			GL.Vertex3( transform.localPosition.x, max.y, 0 );
			GL.Vertex3( transform.localPosition.x, max.y, max.z );
			foreach ( float y in m_Parent.m_Ys )
			{
				GL.Vertex3( 0, y, transform.localPosition.z );
				GL.Vertex3( max.x, y, transform.localPosition.z );
				GL.Vertex3( transform.localPosition.x, y, 0 );
				GL.Vertex3( transform.localPosition.x, y, max.z );
			}
			GL.End();
		}
		// Z Plane
		else if ( m_Axis == ECoordAxis.Z )
		{
			GL.Begin(GL.LINES);
			GL.Color(m_LineColor);
			GL.Vertex3( transform.localPosition.x, 0, 0 );
			GL.Vertex3( transform.localPosition.x, max.y, 0 );
			GL.Vertex3( 0, transform.localPosition.y, 0 );
			GL.Vertex3( max.x, transform.localPosition.y, 0 );
			GL.Vertex3( transform.localPosition.x, 0, max.z );
			GL.Vertex3( transform.localPosition.x, max.y, max.z );
			GL.Vertex3( 0, transform.localPosition.y, max.z );
			GL.Vertex3( max.x, transform.localPosition.y, max.z );
			foreach ( float z in m_Parent.m_Zs )
			{
				GL.Vertex3( transform.localPosition.x, 0, z );
				GL.Vertex3( transform.localPosition.x, max.y, z );
				GL.Vertex3( 0, transform.localPosition.y, z );
				GL.Vertex3( max.x, transform.localPosition.y, z );
			}
			GL.End();
		}
	}
}
