using UnityEngine;
using System.Collections;

public class PointMesh : MonoBehaviour
{
	public int vertex_count = 1000;
	MeshFilter mf;
	
	// Use this for initialization
	void Start ()
	{
//		mf = GetComponent<MeshFilter>();
//		mf.mesh.Clear();
//		Vector3[] verts = new Vector3 [vertex_count];
//		int[] indices = new int [vertex_count];
//		Color[] colors = new Color [vertex_count];
//		for ( int i = 0; i < vertex_count; i++ )
//		{
//			verts[i] = 20 * Random.insideUnitSphere;
//			indices[i] = i;
//			colors[i] = new Color(Random.value, Random.value, Random.value, Random.value);
//		}
//		mf.mesh.vertices = verts;
//		mf.mesh.SetIndices(indices, MeshTopology.Points, 0);
//		mf.mesh.colors = colors;
		
		mf = GetComponent<MeshFilter>();
		mf.mesh.Clear();
		Vector3[] verts = new Vector3 [6];
		Vector3[] norms = new Vector3 [6];
		Vector2[] uvs = new Vector2 [6];
		int[] indices = new int [6];
		
		verts[0] = new Vector3(-0.5f,0, -0.5f);
		verts[1] = new Vector3(0.5f,0, -0.5f);
		verts[2] = new Vector3(0.5f,0, 0.5f);
		verts[3] = new Vector3(0.5f,0, 0.5f);
		verts[4] = new Vector3(-0.5f,0, 0.5f);
		verts[5] = new Vector3(-0.5f,0, -0.5f);
		
		norms[0] = new Vector3(0f, 1f, 0f);
		norms[1] = new Vector3(0f, 1f, 0f);
		norms[2] = new Vector3(0f, 1f, 0f);
		norms[3] = new Vector3(0f, 1f, 0f);
		norms[4] = new Vector3(0f, 1f, 0f);
		norms[5] = new Vector3(0f, 1f, 0f);
		
		indices[0] = 0;
		indices[1] = 1;
		indices[2] = 2;
		indices[3] = 3;
		indices[4] = 4;
		indices[5] = 5;
		
		uvs[0] = new Vector2(0,0);
		uvs[1] = new Vector2(1,0);
		uvs[2] = new Vector2(1,1);
		uvs[3] = new Vector2(1,1);
		uvs[4] = new Vector2(0,1);
		uvs[5] = new Vector2(0,0);
		
		mf.mesh.vertices = verts;
		mf.mesh.normals = norms;
		mf.mesh.SetTriangles(indices, 0);
		mf.mesh.uv = uvs;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
