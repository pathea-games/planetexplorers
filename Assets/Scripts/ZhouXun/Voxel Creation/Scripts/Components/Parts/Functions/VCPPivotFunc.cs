using UnityEngine;
using System.Collections;

//public class VCPPivotFunc : VCPObjectFunc 
//{
//	public CreationItemController m_Controller = null;
//	public bool m_CanSync = false;
//	public Collider m_Collider;

//	[HideInInspector] 
//	public GameObject m_Passenger;
//	[HideInInspector] 
//	public bool m_OpenPivot = false;
//	[HideInInspector] 
//	public float m_PivotAng;
////	[HideInInspector]
////	public 
//	bool m_InitPivot = false;
//	GameObject m_PivotRootObj = null;
//	Quaternion mTargetQuat;
//	Quaternion mLocalQuat;


//	void Update()
//	{
//		if (m_Controller == null)
//			return;
//		if (!m_InitPivot)
//			InitPivot();
//		else
//		{
//			Quaternion mQuat = m_PivotRootObj.transform.localRotation;
//			m_PivotRootObj.transform.localRotation = Quaternion.Lerp(mQuat, (m_OpenPivot ? mTargetQuat : mLocalQuat),0.2f);
//		}


//	}
	
//	void InitPivot()
//	{
//		m_PivotRootObj = new GameObject("PivotRootObj");
//		m_PivotRootObj.transform.parent = gameObject.transform.parent;
//		m_PivotRootObj.transform.localPosition = gameObject.transform.localPosition;
//		m_PivotRootObj.transform.localRotation = gameObject.transform.localRotation;
//		m_PivotRootObj.transform.localScale = Vector3.one;
//		m_PivotRootObj.transform.parent = m_Controller.gameObject.transform;
//		gameObject.transform.parent.parent.parent = m_PivotRootObj.transform;

//		mLocalQuat = m_PivotRootObj.transform.localRotation;
//		m_PivotRootObj.transform.Rotate(new Vector3(0,1,0), m_PivotAng ,Space.Self);
//		mTargetQuat = m_PivotRootObj.transform.localRotation;
//		m_PivotRootObj.transform.localRotation = mLocalQuat;

//		m_InitPivot = true;
//	}

	

//}
