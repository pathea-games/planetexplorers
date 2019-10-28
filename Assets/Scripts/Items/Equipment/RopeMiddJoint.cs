using UnityEngine;
using System.Collections;

public class RopeMiddJoint : MonoBehaviour 
{
	
	public Transform mConnectedBody;
	public Transform mConnectedBody2;
	
	public float mRopeLength = 0.12f;
	public float mForcepower = 100f;
	public float mFanpower = 0.01f;

	[HideInInspector]
	public Rigidbody m_Rigidbody;

	void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		if(null == m_Rigidbody)
			return;
//		if(Vector3.Distance(mConnectedBody.transform.position, mConnectedBody2.transform.position) > 3f * mRopeLength)
//		{
//			transform.position = (mConnectedBody.transform.position + mConnectedBody2.transform.position)/2f;
//		}
		Vector3 dirTo1 = transform.position - mConnectedBody.transform.position;
		{
			m_Rigidbody.AddForce(-dirTo1.normalized * (dirTo1.magnitude - mRopeLength) * mForcepower, ForceMode.Acceleration);
			if(null != mConnectedBody.gameObject.GetComponent<Rigidbody>())
				mConnectedBody.gameObject.GetComponent<Rigidbody>().AddForce(dirTo1.normalized * (dirTo1.magnitude - mRopeLength) * mForcepower * mFanpower, ForceMode.Acceleration);
		}
		
		Vector3 dirTo2 = transform.position - mConnectedBody2.transform.position;
		{
			m_Rigidbody.AddForce(-dirTo2.normalized * (dirTo2.magnitude - mRopeLength) * mForcepower, ForceMode.Acceleration);
			if(null != mConnectedBody2.gameObject.GetComponent<Rigidbody>())
				mConnectedBody2.gameObject.GetComponent<Rigidbody>().AddForce(dirTo2.normalized * (dirTo2.magnitude - mRopeLength) * mForcepower * mFanpower, ForceMode.Acceleration);
		}
	}
}
