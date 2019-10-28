using UnityEngine;
using System.Collections;

public class VCEUIPosClip : MonoBehaviour
{
	public bool m_Left = true;
	public float m_XMin = 0;
	public float m_XMax = 0;
	public float m_YMin = 0;
	public float m_YMax = 0;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	void LateUpdate ()
	{
		if ( m_Left )
		{
			Vector3 pos = transform.localPosition;
			pos.x = Mathf.Clamp(pos.x, m_XMin, Screen.width - m_XMax);
			pos.y = Mathf.Clamp(pos.y, m_YMax - Screen.height, -m_YMin);
			transform.localPosition = pos;
		}
		else
		{
			Vector3 pos = transform.localPosition;
			pos.x = Mathf.Clamp(pos.x, -(Screen.width - m_XMin), -m_XMax);
			pos.y = Mathf.Clamp(pos.y, m_YMax - Screen.height, -m_YMin);
			transform.localPosition = pos;
		}
	}
}
