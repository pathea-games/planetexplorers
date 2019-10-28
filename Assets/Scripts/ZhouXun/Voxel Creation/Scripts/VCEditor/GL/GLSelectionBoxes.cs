using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GLSelectionBoxes : GLBehaviour
{
	public List<SelBox> m_Boxes;
	public Gradient m_LineColor;
	public Gradient m_BoxColor;
	
	// Use this for initialization
	void Start ()
	{
		m_Boxes = new List<SelBox> ();
	}
	
	public override void OnGL ()
	{
		foreach ( SelBox sbox in m_Boxes )
		{
			IntBox ibox = sbox.m_Box;
			Bounds box = new Bounds (Vector3.zero, Vector3.zero);
			box.SetMinMax(new Vector3(ibox.xMin - 0.05f, ibox.yMin - 0.05f, ibox.zMin - 0.05f) * VCEditor.s_Scene.m_Setting.m_VoxelSize, 
				          new Vector3(ibox.xMax + 1.05f, ibox.yMax + 1.05f, ibox.zMax + 1.05f) * VCEditor.s_Scene.m_Setting.m_VoxelSize);
			float sv = (float)(sbox.m_Val) / 255.0f;
			
			// Edge
			if ( sv > 0.99f )
			{
				GL.Begin(GL.LINES);
				GL.Color(m_LineColor.Evaluate(sv));
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				GL.End();
			}
		
			// Face
			GL.Begin(GL.QUADS);
			GL.Color(m_BoxColor.Evaluate(sv));
			GL.Vertex3( box.min.x, box.min.y, box.min.z );
			GL.Vertex3( box.min.x, box.min.y, box.max.z );
			GL.Vertex3( box.min.x, box.max.y, box.max.z );
			GL.Vertex3( box.min.x, box.max.y, box.min.z );
			
			GL.Vertex3( box.max.x, box.min.y, box.min.z );
			GL.Vertex3( box.max.x, box.min.y, box.max.z );
			GL.Vertex3( box.max.x, box.max.y, box.max.z );
			GL.Vertex3( box.max.x, box.max.y, box.min.z );
			
			GL.Vertex3( box.min.x, box.min.y, box.min.z );
			GL.Vertex3( box.min.x, box.min.y, box.max.z );
			GL.Vertex3( box.max.x, box.min.y, box.max.z );
			GL.Vertex3( box.max.x, box.min.y, box.min.z );
			
			GL.Vertex3( box.min.x, box.max.y, box.min.z );
			GL.Vertex3( box.min.x, box.max.y, box.max.z );
			GL.Vertex3( box.max.x, box.max.y, box.max.z );
			GL.Vertex3( box.max.x, box.max.y, box.min.z );
			
			GL.Vertex3( box.min.x, box.min.y, box.min.z );
			GL.Vertex3( box.min.x, box.max.y, box.min.z );
			GL.Vertex3( box.max.x, box.max.y, box.min.z );
			GL.Vertex3( box.max.x, box.min.y, box.min.z );
			
			GL.Vertex3( box.min.x, box.min.y, box.max.z );
			GL.Vertex3( box.min.x, box.max.y, box.max.z );
			GL.Vertex3( box.max.x, box.max.y, box.max.z );
			GL.Vertex3( box.max.x, box.min.y, box.max.z );
			GL.End();
		}
	}
}
