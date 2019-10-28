using UnityEngine;
using System.Collections;

public class BSBoundGizmo : MonoBehaviour 
{
	public LineRenderer[] Edges;

	public Transform trans {get { if (m_Trans == null) m_Trans = transform; return m_Trans; }}
	private Transform m_Trans;

	public Vector3 size 
	{
		get { return trans.localScale; }
		set { trans.localScale = value; }
	}

	public Vector3 position
	{
		get { return trans.position; }
		set { trans.position = value; }
	}
}
