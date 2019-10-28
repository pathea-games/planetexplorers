using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSGLSelectionBoxes : GLBehaviour 
{

	public List<BSTools.SelBox> m_Boxes;
	public Gradient m_LineColor;
	public Gradient m_BoxColor;

	public float scale;
	public Vector3 offset;

	
	// Use this for initialization
	void Start ()
	{
		m_Boxes = new List<BSTools.SelBox> ();
		GlobalGLs.AddGL(this);
	}
	
	public override void OnGL ()
	{
		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Lines/Colored Blended"));			
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}

		// Save camera's matrix.
		GL.PushMatrix();


		for (int i = 0; i < m_Material.passCount; i++)
		{
			m_Material.SetPass(i);
			foreach (BSTools.SelBox sbox in m_Boxes )
			{
				BSTools.IntBox ibox = sbox.m_Box;
				Bounds box = new Bounds (Vector3.zero, Vector3.zero);

				box.SetMinMax(new Vector3(ibox.xMin, ibox.yMin, ibox.zMin) * scale + offset, 
				              new Vector3(ibox.xMax + 1f, ibox.yMax + 1f, ibox.zMax + 1f) * scale + offset);

				float shrink = 0.03f;
				if (Camera.main != null)
				{
					float dist_min = (Camera.main.transform.position - box.min).magnitude;
					float dist_max = (Camera.main.transform.position - box.max).magnitude;
					
					dist_max = dist_max > dist_min ? dist_max : dist_min;
					
					shrink = Mathf.Clamp(dist_max * 0.002f, 0.03f, 0.1f);
				}

				box.min -= new Vector3(shrink, shrink, shrink);
				box.max += new Vector3(shrink, shrink, shrink);

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

		// Restore camera's matrix.
		GL.PopMatrix();
	}

}
