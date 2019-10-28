using UnityEngine;
using System.Collections;

public class VCEGizmoMesh : MonoBehaviour
{
	public float m_BorderSize = 0.003f;
	public float m_BorderUVSize = 0.46f;
	public float m_MeshSizeX = 0.01f;
	public float m_MeshSizeY = 0.01f;
	private MeshFilter m_MeshFilter = null;
	private Mesh m_GizmoMesh = null;
	
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
		Vector3[] verts = new Vector3 [54];
		Vector3[] norms = new Vector3 [54];
		Vector2[] uvs = new Vector2 [54];
		int[] indices = new int [54];
		
		float lowx = -m_MeshSizeX/2;
		float lowbx = lowx + m_BorderSize;
		float highx = m_MeshSizeX/2;
		float highbx = highx - m_BorderSize;
		float lowy = -m_MeshSizeY/2;
		float lowby = lowy + m_BorderSize;
		float highy = m_MeshSizeY/2;
		float highby = highy - m_BorderSize;
		float uvlow = 0;
		float uvlowb = m_BorderUVSize;
		float uvhigh = 1;
		float uvhighb = 1 - m_BorderUVSize;
		float[] coordx = new float [4] { lowx, lowbx, highbx, highx };
		float[] coordy = new float [4] { lowy, lowby, highby, highy };
		float[] uvcoord = new float [4] { uvlow, uvlowb, uvhighb, uvhigh };
		for ( int i = 0; i < 3; ++i )
		{
			for ( int j = 0; j < 3; ++j )
			{
				int offset = (i*3+j)*6;
				verts[offset+0] = new Vector3(coordx[i], coordy[j], 0);
				verts[offset+1] = new Vector3(coordx[i+1], coordy[j], 0);
				verts[offset+2] = new Vector3(coordx[i+1], coordy[j+1], 0);
				verts[offset+3] = new Vector3(coordx[i+1], coordy[j+1], 0);
				verts[offset+4] = new Vector3(coordx[i], coordy[j+1], 0);
				verts[offset+5] = new Vector3(coordx[i], coordy[j], 0);
				uvs[offset+0] = new Vector2(uvcoord[i], uvcoord[j]);
				uvs[offset+1] = new Vector2(uvcoord[i+1], uvcoord[j]);
				uvs[offset+2] = new Vector2(uvcoord[i+1], uvcoord[j+1]);
				uvs[offset+3] = new Vector2(uvcoord[i+1], uvcoord[j+1]);
				uvs[offset+4] = new Vector2(uvcoord[i], uvcoord[j+1]);
				uvs[offset+5] = new Vector2(uvcoord[i], uvcoord[j]);
				for ( int v = 0; v < 6; ++v )
				{
					norms[offset+v] = Vector3.forward;
					indices[offset+v] = offset+v;
				}
			}
		}
		m_GizmoMesh.vertices = verts;
		m_GizmoMesh.normals = norms;
		m_GizmoMesh.uv = uvs;
		m_GizmoMesh.SetTriangles(indices, 0);
	}
}
