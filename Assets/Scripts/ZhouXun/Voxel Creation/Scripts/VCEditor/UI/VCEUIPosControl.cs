using UnityEngine;
using System.Collections;

public class VCEUIPosControl : MonoBehaviour
{
	public float m_BottomDist = 0;
		
	// Use this for initialization
	void Start ()
	{
		Vector3 pos = transform.localPosition;
		pos.y = m_BottomDist - Screen.height;
		transform.localPosition = pos;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 pos = transform.localPosition;
		pos.y = m_BottomDist - Screen.height;
		transform.localPosition = pos;
	}
}
