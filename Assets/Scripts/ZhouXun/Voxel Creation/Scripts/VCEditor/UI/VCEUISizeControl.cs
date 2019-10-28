using UnityEngine;
using System.Collections;

public class VCEUISizeControl : MonoBehaviour
{
	public float m_YReserve = 0;
		
	// Use this for initialization
	void Start ()
	{
		Vector3 scale = transform.localScale;
		scale.y = Screen.height - m_YReserve;
		transform.localScale = scale;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 scale = transform.localScale;
		scale.y = Screen.height - m_YReserve;
		transform.localScale = scale;
	}
}
