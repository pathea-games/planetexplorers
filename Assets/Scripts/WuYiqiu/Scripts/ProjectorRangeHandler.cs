using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projector))]
public class ProjectorRangeHandler : MonoBehaviour 
{
	public Material m_SourceMat;

	private Material m_ProjMat;

	private Projector m_Projector;

	// Use this for initialization
	void Start () 
	{
		m_ProjMat = Material.Instantiate(m_SourceMat) as Material;
		m_Projector = GetComponent<Projector>();
		m_Projector.material = m_ProjMat;
		m_Projector.orthographic = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		m_ProjMat.SetVector("_CenterAndRadius", new Vector4(transform.position.x, transform.position.y - m_Projector.farClipPlane*0.4f, transform.position.z, m_Projector.orthographicSize));
	}

	void OnDestroy ()
	{
		if (m_Projector != null)
			m_Projector.material = null;
		Material.Destroy(m_ProjMat);
	}
}
