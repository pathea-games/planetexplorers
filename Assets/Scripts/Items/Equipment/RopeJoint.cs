using UnityEngine;
using System.Collections;

public class RopeJoint : MonoBehaviour 
{
	public Transform mConnectedBody;
	
	public float mRopeLength = 0.12f;
	
	public float mForcepower = 1f;
	
	public float mFanpower = 0.001f;

	[HideInInspector]
	public Rigidbody m_Rigidbody;
	
	void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate()
	{
		if(null == m_Rigidbody)
			return;
		Vector3 dir = transform.position - mConnectedBody.position;
		if(dir.magnitude > mRopeLength)
		{
			transform.position = mConnectedBody.position + mRopeLength * dir.normalized;
			m_Rigidbody.AddForce(-dir.normalized * (dir.magnitude - mRopeLength) * mForcepower, ForceMode.Acceleration);
			if(null != mConnectedBody.gameObject.GetComponent<Rigidbody>())
				mConnectedBody.gameObject.GetComponent<Rigidbody>().AddForce(dir.normalized * (dir.magnitude - mRopeLength) * mForcepower * mFanpower, ForceMode.Acceleration);
		}
	}
}
