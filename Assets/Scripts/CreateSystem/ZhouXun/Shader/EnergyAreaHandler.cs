using UnityEngine;
using System.Collections;

public class EnergyAreaHandler : MonoBehaviour
{
	public Material m_SourceMat;
	private Material m_ProjMat;
	private Projector m_Projector;
	public float m_StreamSpeed = 0.5f;
	public float m_EnergyScale = 1;
	private float m_UnitBodyIntens;
	private float m_UnitStreamIntens;
	// Use this for initialization
	void Start ()
	{
		m_ProjMat = Material.Instantiate(m_SourceMat) as Material;
		m_UnitBodyIntens = m_ProjMat.GetFloat("_BodyIntensity");
		m_UnitStreamIntens = m_ProjMat.GetFloat("_StreamIntensity");
		m_Projector = GetComponent<Projector>();
		m_Projector.material = m_ProjMat;
		m_ProjMat.name = "Energy Area - " + transform.position.ToString();
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_ProjMat.SetVector("_CenterAndRadius", new Vector4(transform.position.x, transform.position.y - m_Projector.farClipPlane*0.5f, transform.position.z, m_Projector.orthographicSize));
		m_ProjMat.SetFloat("_Speed", m_StreamSpeed);
		m_ProjMat.SetFloat("_BodyIntensity", m_UnitBodyIntens * m_EnergyScale);
		m_ProjMat.SetFloat("_StreamIntensity", m_UnitStreamIntens * m_EnergyScale);
		m_ProjMat.SetFloat("_ExhaustEffect", Mathf.Clamp01((0.25f - m_EnergyScale)*4));
	}
	
	void OnDestroy ()
	{
		if (m_Projector != null)
			m_Projector.material = null;
		Material.Destroy(m_ProjMat);
	}
}
