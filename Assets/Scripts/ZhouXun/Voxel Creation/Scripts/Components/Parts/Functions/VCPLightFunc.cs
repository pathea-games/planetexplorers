using UnityEngine;
using System.Collections;

//public class VCPLightFunc : VCPObjectFunc 
//{
//	public CreationItemController m_Controller = null;
//	public bool m_CanSync = false;
//	public Transform m_PivotPoint;
//	public Collider m_Collider;
//	public bool m_OpenLight = false;
//	public Light m_Light = null;
//	[HideInInspector] 
//	public GameObject m_Passenger;
//	public Renderer m_Render;

//	void Update()
//	{
//		if (m_Controller == null)
//			return;
		
//		UpdateSelected();
//		UpdateLightState();
//	}
	
//	private void UpdateSelected()
//	{
//		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//		RaycastHit rayHit;
//		m_Selected= m_Collider.Raycast(ray,out rayHit,100);
//		m_Distence = rayHit.distance;
//	}

//	private void UpdateLightState()
//	{
//		if (m_OpenLight)
//		{
//			m_Light.enabled = true;
//		}
//		else 
//		{
//			m_Light.enabled = false;
//		}

//	}
//}
