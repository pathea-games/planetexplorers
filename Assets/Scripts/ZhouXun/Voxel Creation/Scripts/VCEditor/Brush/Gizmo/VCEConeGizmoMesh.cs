using UnityEngine;
using System.Collections;

public class VCEConeGizmoMesh : MonoBehaviour
{
	MeshFilter m_TargetMeshFilter;
	Mesh m_ConeMesh;

	public float m_PositiveScale = 0;
	public float m_NegativeScale = 1;
	public int m_Segment = 24;

	float last_pos_scl = 0;
	float last_neg_scl = 0;

	Vector3[] m_verts;
	Vector3[] m_normals;
	int[] m_indices;

	// Use this for initialization
	void Start ()
	{
		m_verts = new Vector3[m_Segment*12] ;
		m_normals = new Vector3[m_Segment*12] ;
		m_indices = new int[m_Segment*12] ;
		m_TargetMeshFilter = GetComponent<MeshFilter>();
		if ( m_TargetMeshFilter != null )
		{
			m_ConeMesh = new Mesh ();
			m_TargetMeshFilter.mesh = m_ConeMesh;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_PositiveScale != last_pos_scl || m_NegativeScale != last_neg_scl )
		{
			last_pos_scl = m_PositiveScale;
			last_neg_scl = m_NegativeScale;

			GenCone();
		}
	}

	void OnDestroy ()
	{
		if ( m_ConeMesh != null )
		{
			Mesh.Destroy(m_ConeMesh);
			m_ConeMesh = null;
		}
	}

	void GenCone ()
	{
		if ( m_ConeMesh != null )
		{
			m_ConeMesh.Clear();

			for ( int i = 0; i < m_Segment; ++i )
			{
				int ofs = i * 12;
				float angle0 = (float)(i) / (float)m_Segment * Mathf.PI * 2.0f;
				float angle1 = (float)(i+1) / (float)m_Segment * Mathf.PI * 2.0f;
				float negr = m_NegativeScale * 0.5f;
				float posr = m_PositiveScale * 0.5f;

				m_verts[ofs+0] = new Vector3 (negr*Mathf.Cos(angle0), -0.5f, negr*Mathf.Sin(angle0));
				m_verts[ofs+1] = new Vector3 (negr*Mathf.Cos(angle1), -0.5f, negr*Mathf.Sin(angle1));
				m_verts[ofs+2] = new Vector3 (posr*Mathf.Cos(angle1), 0.5f, posr*Mathf.Sin(angle1));
				
				m_verts[ofs+3] = new Vector3 (posr*Mathf.Cos(angle1), 0.5f, posr*Mathf.Sin(angle1));
				m_verts[ofs+4] = new Vector3 (posr*Mathf.Cos(angle0), 0.5f, posr*Mathf.Sin(angle0));
				m_verts[ofs+5] = new Vector3 (negr*Mathf.Cos(angle0), -0.5f, negr*Mathf.Sin(angle0));
				
				m_verts[ofs+6] = new Vector3 (negr*Mathf.Cos(angle1), -0.5f, negr*Mathf.Sin(angle1));
				m_verts[ofs+7] = new Vector3 (negr*Mathf.Cos(angle0), -0.5f, negr*Mathf.Sin(angle0));
				m_verts[ofs+8] = new Vector3 (0.0f, -0.5f, 0.0f);
				
				m_verts[ofs+9] = new Vector3 (posr*Mathf.Cos(angle0), 0.5f, posr*Mathf.Sin(angle0));
				m_verts[ofs+10] = new Vector3 (posr*Mathf.Cos(angle1), 0.5f, posr*Mathf.Sin(angle1));
				m_verts[ofs+11] = new Vector3 (0.0f, 0.5f, 0.0f);
				
				m_normals[ofs+0] = new Vector3 (Mathf.Cos(angle0), 0, Mathf.Sin(angle0));
				m_normals[ofs+1] = new Vector3 (Mathf.Cos(angle1), 0, Mathf.Sin(angle1));
				m_normals[ofs+2] = new Vector3 (Mathf.Cos(angle1), 0, Mathf.Sin(angle1));

				m_normals[ofs+3] = new Vector3 (Mathf.Cos(angle1), 0, Mathf.Sin(angle1));
				m_normals[ofs+4] = new Vector3 (Mathf.Cos(angle0), 0, Mathf.Sin(angle0));
				m_normals[ofs+5] = new Vector3 (Mathf.Cos(angle0), 0, Mathf.Sin(angle0));

				m_normals[ofs+6] = new Vector3 (0.0f, -1.0f, 0.0f);
				m_normals[ofs+7] = new Vector3 (0.0f, -1.0f, 0.0f);
				m_normals[ofs+8] = new Vector3 (0.0f, -1.0f, 0.0f);

				m_normals[ofs+9] = new Vector3 (0.0f, 1.0f, 0.0f);
				m_normals[ofs+10] = new Vector3 (0.0f, 1.0f, 0.0f);
				m_normals[ofs+11] = new Vector3 (0.0f, 1.0f, 0.0f);
				
				for ( int j = 0; j < 12; ++j ) m_indices[ofs+j] = ofs+j;
			}

			m_ConeMesh.vertices = m_verts;
			m_ConeMesh.normals = m_normals;
			m_ConeMesh.SetTriangles(m_indices, 0);
		}
	}
}
