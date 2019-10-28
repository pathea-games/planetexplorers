using UnityEngine;
using System.Collections;

public class SphereNetHandler : MonoBehaviour
{
	private float m_TimeFactor = 0;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_TimeFactor += Time.deltaTime;
		GetComponent<Renderer>().material.SetFloat("_TimeFactor", m_TimeFactor);
	}
}
