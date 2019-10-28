using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CrossLine : MonoBehaviour
{
	private Mesh m_LineMesh;
	public Vector3 m_Begin;
	public Vector3 m_End;
	public float m_Thickness = 0.1f;
	public int m_Segment = 4;
	private Vector3 _lastBegin;
	private Vector3 _lastEnd;
	private float _thickness = 0;
	private float _seg = 0;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Vector3.Distance(m_Begin, _lastBegin) > 0.001f 
		    || Vector3.Distance(m_End, _lastEnd) > 0.001f
		    || m_Thickness != _thickness || m_Segment != _seg )
		{
			Refresh();
		}
	}

	void OnDestroy ()
	{
		FreeMesh();
	}

	public void FreeMesh ()
	{
		if ( m_LineMesh != null )
		{
			Mesh.Destroy(m_LineMesh);
			m_LineMesh = null;
		}
	}

	public void Refresh ()
	{
		_lastBegin = m_Begin;
		_lastEnd = m_End;
		_thickness = m_Thickness;
		_seg = m_Segment;

		FreeMesh();
		if ( m_Begin != m_End && _thickness != 0 && _seg > 0 )
		{
			m_LineMesh = new Mesh ();
			int vcnt = m_Segment * 4;
			int icnt = m_Segment * 6;
			Vector3[] verts = new Vector3[vcnt] ;
			Vector2[] uvs = new Vector2[vcnt] ;
			int[] indices = new int[icnt] ;

			float angle_step = Mathf.PI / (float)(m_Segment);
			transform.position = (m_End + m_Begin) * 0.5f;
			transform.LookAt(m_End);
			float half_len = (m_End - m_Begin).magnitude * 0.5f;
			
			for ( int i = 0; i < m_Segment; ++i )
			{
				float angle = angle_step * i;
				float x = Mathf.Cos(angle) * m_Thickness * 0.5f;
				float y = Mathf.Sin(angle) * m_Thickness * 0.5f;
				float z = half_len;
				verts[i*4+0] = new Vector3 (-x,-y,-z);
				verts[i*4+1] = new Vector3 ( x, y,-z);
				verts[i*4+2] = new Vector3 ( x, y, z);
				verts[i*4+3] = new Vector3 (-x,-y, z);
				uvs[i*4+0] = new Vector2 (-z + angle*2,-1);
				uvs[i*4+1] = new Vector2 (-z + angle*2, 1);
				uvs[i*4+2] = new Vector2 ( z + angle*2, 1);
				uvs[i*4+3] = new Vector2 ( z + angle*2,-1);
				indices[i*6+0] = i*4+0;
				indices[i*6+1] = i*4+1;
				indices[i*6+2] = i*4+2;
				indices[i*6+3] = i*4+2;
				indices[i*6+4] = i*4+3;
				indices[i*6+5] = i*4+0;
			}

			m_LineMesh.vertices = verts;
			m_LineMesh.uv = uvs;
			m_LineMesh.SetTriangles(indices, 0);
			GetComponent<MeshFilter>().mesh = m_LineMesh;
		}
	}
}
