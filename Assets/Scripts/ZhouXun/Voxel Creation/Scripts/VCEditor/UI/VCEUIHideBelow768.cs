using UnityEngine;
using System.Collections;

public class VCEUIHideBelow768 : MonoBehaviour
{
	public float m_minScreenHeight = 768;
	
	// Use this for initialization
	void Start ()
	{
		if ( Screen.height < m_minScreenHeight )
			this.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Screen.height < m_minScreenHeight )
			this.gameObject.SetActive(false);
	}
}
