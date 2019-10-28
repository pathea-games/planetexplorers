using UnityEngine;
using System.Collections;

public class ScanningCircleHandler : MonoBehaviour
{
	public float m_Brightness = 1;
	public float m_CircleBrightness = 0.5f;

	private Renderer m_Renderer;

	// Use this for initialization
	void Start ()
	{
		m_Renderer = GetComponentInChildren<Renderer>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		TakeEffect();
	}

	public void TakeEffect ()
	{
		if ( m_Renderer == null )
			m_Renderer = GetComponentInChildren<Renderer>();

		if ( m_Renderer != null )
		{
			m_Renderer.material.SetFloat("_Brightness", m_Brightness);
			m_Renderer.material.SetFloat("_CircleBrightness", m_CircleBrightness);
		}
	}
}
