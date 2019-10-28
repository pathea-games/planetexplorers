using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGen : MonoBehaviour
{
	public int m_BillboardCount = 1000;
	public Mesh m_Mesh = null;
	public Vector3 m_StartCoord = Vector3.zero;
	
	// Use this for initialization
	void Start ()
	{
		m_Mesh = GetComponent<MeshFilter>().mesh;
		ReGen();
		this.enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	void OnGUI ()
	{
		if ( GUI.Button(new Rect(30,30,100,30), "Re-Generate") )
		{
			ReGen();
		}
	}
	
	void ReGen ()
	{
		m_Mesh.Clear();
		
		Vector3[] verts = new Vector3 [m_BillboardCount*4];
		Vector3[] norms = new Vector3 [m_BillboardCount*4];
		Vector2[] uvs = new Vector2 [m_BillboardCount*4];
		Vector2[] uv2s = new Vector2 [m_BillboardCount*4];
		int [] indices = new int [m_BillboardCount*6];
		//Color32[] colors32 = new Color32 [m_BillboardCount*4];
		
		Vector3 A = new Vector3 (-1, -1, 0);
		Vector3 B = new Vector3 (-1,  1, 0);
		Vector3 C = new Vector3 ( 1,  1, 0);
		Vector3 D = new Vector3 ( 1, -1, 0);
		
		m_StartCoord = this.transform.position - Vector3.one * 128;
		RaycastHit rch;
		for ( int i = 0; i < m_BillboardCount; ++i )
		{
			verts[i*4+0] = A;
			verts[i*4+1] = B;
			verts[i*4+2] = C;
			verts[i*4+3] = D;
			
			indices[i*6+0] = i*4+0;
			indices[i*6+1] = i*4+1;
			indices[i*6+2] = i*4+2;
			indices[i*6+3] = i*4+2;
			indices[i*6+4] = i*4+3;
			indices[i*6+5] = i*4+0;
			
			Vector3 origin = new Vector3(Random.value, 1, Random.value) * 256 + m_StartCoord;
			Vector3 normal = Vector3.up;
			if ( Physics.Raycast(origin, Vector3.down, out rch, 1024, 1 << Pathea.Layer.VFVoxelTerrain) ) 
			{
				Vector3 pos = rch.point;
				norms[i*4+3] = 
				norms[i*4+2] = 
				norms[i*4+1] = 
				norms[i*4+0] = 
				pos;
				normal = rch.normal;
			}
			else
			{
				Vector3 pos = Vector3.zero;
				norms[i*4+3] = 
				norms[i*4+2] = 
				norms[i*4+1] = 
				norms[i*4+0] = 
				pos;
			}
			
			Vector3 n1 = (normal * 1.25f + Vector3.up * -0.25f).normalized;
			Vector3 n2 = (normal * 0.5f + Vector3.up * 0.5f).normalized;
			
			// Saves the normal
			uvs[i*4+0] = new Vector2(n1.x, n1.z);
			uvs[i*4+1] = new Vector2(n2.x, n2.z);
			uvs[i*4+2] = new Vector2(n2.x, n2.z);
			uvs[i*4+3] = new Vector2(n1.x, n1.z);
			float type = 0;//Random.value/64.0f*3.0f;
			uv2s[i*4+0] = new Vector2(type,0);
			uv2s[i*4+1] = new Vector2(type,0);
			uv2s[i*4+2] = new Vector2(type,0);
			uv2s[i*4+3] = new Vector2(type,0);
		}
		
		m_Mesh.vertices = verts;
		m_Mesh.triangles = indices;
		m_Mesh.normals = norms;
		m_Mesh.uv = uvs;
		m_Mesh.uv2 = uv2s;
	}
}
