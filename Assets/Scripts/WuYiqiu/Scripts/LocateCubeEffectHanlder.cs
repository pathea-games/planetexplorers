using UnityEngine;
using System.Collections;

public class LocateCubeEffectHanlder : MonoBehaviour 
{
	public float m_MaxHeightScale = 1.0f;
	public float m_MaxLengthScale = 0.8f;
	public float m_CubeLen = 1.0f;

	Renderer m_Renderer;
	Transform m_Trans;

	public bool m_Start = false;
	private float m_CurHeightScale = 0;
	private float m_CurLengthScale = 0;
	// Use this for initialization
	void Start () 
	{
		m_Renderer = GetComponent<Renderer>();

		m_Trans = transform;

		m_Start = true;

		m_Trans.localScale = new Vector3(m_CurLengthScale, m_CurHeightScale, m_CurLengthScale);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_Start)
		{
			m_CurHeightScale = Mathf.Lerp(m_CurHeightScale, m_MaxHeightScale, 0.2f);
			m_CurLengthScale = Mathf.Lerp(m_CurLengthScale, m_MaxLengthScale, 0.2f);

			if (m_MaxHeightScale - m_CurHeightScale < 0.05f)
			{
				m_CurHeightScale = m_MaxHeightScale;
				m_CurLengthScale = m_MaxLengthScale;
				m_Start = false;
			}

			m_Trans.localScale = new Vector3(m_CurLengthScale, m_CurHeightScale, m_CurLengthScale);
		}
		else
		{
			m_CurHeightScale = 0;
			m_CurLengthScale = 0;
			m_Trans.localScale = new Vector3(m_MaxLengthScale, m_MaxHeightScale, m_MaxLengthScale);
		}

		m_Renderer.material.SetVector("_CenterWorldPos", transform.position);
		m_Renderer.material.SetFloat("_Length", m_Trans.lossyScale.x * m_CubeLen * 0.5f);
		m_Renderer.material.SetFloat("_Height", m_Trans.localScale.y * m_CubeLen * 0.5f);
	}
}
