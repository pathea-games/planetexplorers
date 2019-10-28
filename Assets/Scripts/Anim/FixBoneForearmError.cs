using UnityEngine;
using System.Collections;

public class FixBoneForearmError : MonoBehaviour 
{
	public Transform m_HandTrans;
	Vector3 	m_DefaultLocalPos;
	Quaternion	m_DefaultHandLocalRot;
	// Use this for initialization
	void Start () 
	{
		if(null != m_HandTrans)
		{
			m_DefaultLocalPos = transform.localPosition;
			m_DefaultHandLocalRot = transform.localRotation;
		}
	}
	
	// Update is called once per framef
	void LateUpdate () 
	{
		if(null != m_HandTrans)
		{
			transform.localPosition = m_DefaultLocalPos;
			transform.localRotation = m_DefaultHandLocalRot;
		}	
	}
}
