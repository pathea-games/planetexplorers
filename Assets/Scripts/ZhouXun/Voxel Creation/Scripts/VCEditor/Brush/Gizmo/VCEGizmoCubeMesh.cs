using UnityEngine;
using System.Collections;

public class VCEGizmoCubeMesh : GLBehaviour
{
	private MeshFilter m_MeshFilter = null;
	private Mesh m_GizmoMesh = null;
	
	public float m_VoxelSize = 0.01f;
	private IntVector3 m_CubeSize = new IntVector3 (1,1,1);
	public float m_Shrink = 0.03f;

	public Transform m_ShapeGizmo = null;
	
	public IntVector3 CubeSize
	{
		get { return m_CubeSize; }
		set { m_CubeSize = new IntVector3(value); Update(); }
	}
	
	// Use this for initialization
	void Start ()
	{
		m_MeshFilter = GetComponent<MeshFilter>();
		m_GizmoMesh = m_MeshFilter.mesh;	
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_GizmoMesh.Clear();
		Vector3[] verts = new Vector3 [36];
		Vector3[] norms = new Vector3 [36];
		Vector2[] uvs = new Vector2 [36];
		int[] indices = new int [36];
	
		Vector3 size = m_VoxelSize * m_CubeSize.ToVector3();
		Vector3 min = new Vector3 (m_VoxelSize*m_Shrink, m_VoxelSize*m_Shrink, m_VoxelSize*m_Shrink);
		Vector3 max = size - min;
		
		verts[2]  = new Vector3(min.x, min.y, min.z);
		verts[1]  = new Vector3(min.x, max.y, min.z);
		verts[0]  = new Vector3(min.x, max.y, max.z);
		verts[5]  = new Vector3(min.x, max.y, max.z);
		verts[4]  = new Vector3(min.x, min.y, max.z);
		verts[3]  = new Vector3(min.x, min.y, min.z);
		
		verts[6]  = new Vector3(max.x, min.y, min.z);
		verts[7]  = new Vector3(max.x, max.y, min.z);
		verts[8]  = new Vector3(max.x, max.y, max.z);
		verts[9]  = new Vector3(max.x, max.y, max.z);
		verts[10] = new Vector3(max.x, min.y, max.z);
		verts[11] = new Vector3(max.x, min.y, min.z);
		
		verts[12+0]  = new Vector3(min.x, min.y, min.z);
		verts[12+1]  = new Vector3(max.x, min.y, min.z);
		verts[12+2]  = new Vector3(max.x, min.y, max.z);
		verts[12+3]  = new Vector3(max.x, min.y, max.z);
		verts[12+4]  = new Vector3(min.x, min.y, max.z);
		verts[12+5]  = new Vector3(min.x, min.y, min.z);

		verts[12+8]  = new Vector3(min.x, max.y, min.z);
		verts[12+7]  = new Vector3(max.x, max.y, min.z);
		verts[12+6]  = new Vector3(max.x, max.y, max.z);
		verts[12+11] = new Vector3(max.x, max.y, max.z);
		verts[12+10] = new Vector3(min.x, max.y, max.z);
		verts[12+9]  = new Vector3(min.x, max.y, min.z);
		
		verts[24+0]  = new Vector3(min.x, min.y, min.z);
		verts[24+1]  = new Vector3(min.x, max.y, min.z);
		verts[24+2]  = new Vector3(max.x, max.y, min.z);
		verts[24+3]  = new Vector3(max.x, max.y, min.z);
		verts[24+4]  = new Vector3(max.x, min.y, min.z);
		verts[24+5]  = new Vector3(min.x, min.y, min.z);

		verts[24+8]  = new Vector3(min.x, min.y, max.z);
		verts[24+7]  = new Vector3(min.x, max.y, max.z);
		verts[24+6]  = new Vector3(max.x, max.y, max.z);
		verts[24+11] = new Vector3(max.x, max.y, max.z);
		verts[24+10] = new Vector3(max.x, min.y, max.z);
		verts[24+9]  = new Vector3(min.x, min.y, max.z);
		
		norms[0] = Vector3.left;
		norms[1] = Vector3.left;
		norms[2] = Vector3.left;
		norms[3] = Vector3.left;
		norms[4] = Vector3.left;
		norms[5] = Vector3.left;
		
		norms[6+0] = Vector3.right;
		norms[6+1] = Vector3.right;
		norms[6+2] = Vector3.right;
		norms[6+3] = Vector3.right;
		norms[6+4] = Vector3.right;
		norms[6+5] = Vector3.right;
		
		norms[12+0] = Vector3.down;
		norms[12+1] = Vector3.down;
		norms[12+2] = Vector3.down;
		norms[12+3] = Vector3.down;
		norms[12+4] = Vector3.down;
		norms[12+5] = Vector3.down;
		
		norms[18+0] = Vector3.up;
		norms[18+1] = Vector3.up;
		norms[18+2] = Vector3.up;
		norms[18+3] = Vector3.up;
		norms[18+4] = Vector3.up;
		norms[18+5] = Vector3.up;
		
		norms[24+0] = Vector3.back;
		norms[24+1] = Vector3.back;
		norms[24+2] = Vector3.back;
		norms[24+3] = Vector3.back;
		norms[24+4] = Vector3.back;
		norms[24+5] = Vector3.back;
		
		norms[30+0] = Vector3.forward;
		norms[30+1] = Vector3.forward;
		norms[30+2] = Vector3.forward;
		norms[30+3] = Vector3.forward;
		norms[30+4] = Vector3.forward;
		norms[30+5] = Vector3.forward;
				
		for ( int f = 0; f < 6; ++f )
		{
			float w = 1, h = 1;
			switch (f)
			{
			case 0: h = m_CubeSize.y; w = m_CubeSize.z; break;
			case 1: w = m_CubeSize.y; h = m_CubeSize.z; break;
			case 2: w = m_CubeSize.x; h = m_CubeSize.z; break;
			case 3: h = m_CubeSize.x; w = m_CubeSize.z; break;
			case 4: w = m_CubeSize.y; h = m_CubeSize.x; break;
			case 5: h = m_CubeSize.y; w = m_CubeSize.x; break;
			}
			uvs[f*6+0] = new Vector2(0,0);
			uvs[f*6+1] = new Vector2(w,0);
			uvs[f*6+2] = new Vector2(w,h);
			uvs[f*6+3] = new Vector2(w,h);
			uvs[f*6+4] = new Vector2(0,h);
			uvs[f*6+5] = new Vector2(0,0);
			for ( int i = 0; i < 6; ++i )
			{
				int index = f*6+i;
				indices[index] = index;
			}
		}
		m_GizmoMesh.vertices = verts;
		m_GizmoMesh.normals = norms;
		m_GizmoMesh.uv = uvs;
		m_GizmoMesh.SetTriangles(indices, 0);

		if ( m_ShapeGizmo != null )
		{
			m_ShapeGizmo.transform.localScale = m_CubeSize.ToVector3() * m_VoxelSize;
			m_ShapeGizmo.transform.localPosition = m_CubeSize.ToVector3() * m_VoxelSize * 0.5f;
			m_ShapeGizmo.GetComponentsInChildren<Renderer>(true)[0].material.renderQueue = 3000;
		}
	}

	public override void OnGL ()
	{
		Vector3 center = m_VoxelSize * m_CubeSize.ToVector3() * 0.5f + transform.position;
		Vector3 extend = (m_CubeSize.ToVector3()-Vector3.one) * m_VoxelSize * 0.5f;
		for ( int i = 0; i < 4; ++i )
		{
			Vector3 ofs = Vector3.down * extend.y;
			if ( (i & 1) == 0 )
				ofs += Vector3.right * extend.x;
			else
				ofs -= Vector3.right * extend.x;
			if ( (i & 2) == 0 )
				ofs += Vector3.forward * extend.z;
			else
				ofs -= Vector3.forward * extend.z;
			Vector3 origin = center + ofs;
			float p = (Time.time/1.5f) - Mathf.Floor(Time.time/1.5f);
			RaycastHit rch;
			if ( Physics.Raycast(new Ray(origin, Vector3.down), out rch, 100, VCConfig.s_EditorLayerMask) )
			{
				GL.Begin(GL.LINES);
				GL.Color(new Color(0.5f,0.8f,1.0f,0.4f));
				GL.Vertex(origin);
				GL.Color(new Color(0.5f,0.8f,1.0f,1.0f));
				GL.Vertex(rch.point);
				GL.Vertex(origin);
				GL.Vertex(Vector3.Lerp(origin,rch.point,p));
				GL.Vertex(rch.point + Vector3.forward*m_VoxelSize*0.4f);
				GL.Vertex(rch.point - Vector3.forward*m_VoxelSize*0.4f);
				GL.Vertex(rch.point + Vector3.right*m_VoxelSize*0.4f);
				GL.Vertex(rch.point - Vector3.right*m_VoxelSize*0.4f);
				GL.End();
			}
		}
	}
}
