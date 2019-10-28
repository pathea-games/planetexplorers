using UnityEngine;
using System.Collections;

public class GLNearVoxelIndicator : GLBehaviour
{
	public static IntVector3 InvalidPos { get { return new IntVector3 (-100,-100,-100); } }
	public IntVector3 m_Center = InvalidPos;
	public int m_Expand = 4;
	public Gradient m_BoxColors;
	
	void Update ()
	{
		VCEMath.DrawTarget dtar;
		if ( VCEInput.s_MouseOnUI )
		{
			m_Center = InvalidPos;
		}
		else
		{
			if ( VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out dtar, 1) )
			{
				m_Center.x = dtar.snapto.x;
				m_Center.y = dtar.snapto.y;
				m_Center.z = dtar.snapto.z;
			}
			else
			{
				m_Center = InvalidPos;
			}
		}
	}
	
	void OnDisable()
	{
		m_Center = InvalidPos;
	}
	
	public override void OnGL ()
	{
		if ( m_Center.x < -50 )
			return;
		for ( int x = -m_Expand; x <= m_Expand; ++x )
		{
			for ( int y = -m_Expand; y <= m_Expand; ++y )
			{
				for ( int z = -m_Expand; z <= m_Expand; ++z )
				{
					IntVector3 pos = new IntVector3 (x+m_Center.x, y+m_Center.y, z+m_Center.z);
					if ( VCEditor.s_Scene.m_IsoData.GetVoxel(VCIsoData.IPosToKey(pos)).Volume > 0 )
					{
						IntBox ibox = new IntBox ();
						ibox.xMin = (short)pos.x; ibox.xMax = (short)pos.x;
						ibox.yMin = (short)pos.y; ibox.yMax = (short)pos.y;
						ibox.zMin = (short)pos.z; ibox.zMax = (short)pos.z;
						Bounds box = new Bounds (Vector3.zero, Vector3.zero);
						box.SetMinMax(new Vector3(ibox.xMin - 0.03f, ibox.yMin - 0.03f, ibox.zMin - 0.03f) * VCEditor.s_Scene.m_Setting.m_VoxelSize, 
							          new Vector3(ibox.xMax + 1.03f, ibox.yMax + 1.03f, ibox.zMax + 1.03f) * VCEditor.s_Scene.m_Setting.m_VoxelSize);
						float sv = (float)(Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z))) / (float)(m_Expand);
						
						Color lc = m_BoxColors.Evaluate(sv);
						Color bc = lc;
						lc.a *= 1.5f;
						
						// Edge
						GL.Begin(GL.LINES);
						GL.Color(lc);
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
					
						// Face
						GL.Begin(GL.QUADS);
						GL.Color(bc);
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
		}
	}
}
