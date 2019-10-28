//using UnityEngine;
//using System.Collections;

//public class VCPVtolThrusterFunc : VCPartFunc
//{
//	public VCPVtolThrusterProperty m_Property;
////	public HelicopterController m_Controller;

//	public Transform m_ThrustProvider;
//	public Vector3 m_TorqueOffset = Vector3.zero;

//	public Transform m_JetEffectGroup;

//	// Rotor Status
//	public float m_CurrPower;
//	public float m_PowerTarget;
//	public Vector3 m_xMovement;
//	public Vector3 m_yMovement;
//	public Vector3 m_zMovement;
//	private float m_MotorError = 0;
//	/*
//	void Start ()
//	{
//		if ( m_Controller != null )
//		{
//			m_CurrPower = 0;
//			m_PowerTarget = 0;
//			m_xMovement = Vector3.zero;
//			m_yMovement = Vector3.zero;
//			m_zMovement = Vector3.zero;
//			m_TorqueOffset = Vector3.zero;
//		}
//	}

//	void FixedUpdate ()
//	{
//		if ( m_Controller != null )
//		{
//			if ( m_Controller.m_NetChar == ENetCharacter.nrOwner )
//			{
//				// power target
//				float liftdot = Mathf.Clamp01(Vector3.Dot(m_Controller.transform.up, m_ThrustProvider.forward));
//				float movedot = Mathf.Clamp01(Vector3.Dot(m_xMovement + m_zMovement, m_ThrustProvider.forward));
//				m_PowerTarget = Mathf.Clamp(Mathf.Lerp(movedot * m_Property.m_MaxPower * 0.5f, Mathf.Max(m_yMovement.magnitude, 0), liftdot), 0, m_Property.m_MaxPower);

//				// current power
//				m_CurrPower = Mathf.Lerp(m_CurrPower, m_PowerTarget, 0.3f);

//				if ( m_Controller.m_Rigidbody != null && m_Controller.m_Active )
//				{
//					m_TorqueOffset = m_ThrustProvider.position - m_Controller.m_Rigidbody.worldCenterOfMass;
//					m_MotorError = Mathf.Sin(Time.time*1.5f) * 0.1f + 1f;
//					float force = m_CurrPower * m_MotorError * m_Property.m_Eta * 10F * 
//								  transform.localScale.magnitude * 0.5773503f *
//								  m_Controller.m_AtmosphereCoef;
//					Vector3 lift_force = m_ThrustProvider.forward * force;

//					// apply this force to rigidbody
//					Vector3 fpoint = Vector3.Lerp(m_ThrustProvider.position, m_Controller.m_Rigidbody.worldCenterOfMass, 0.97f);
//					fpoint.y = m_Controller.m_Rigidbody.worldCenterOfMass.y;
//					m_Controller.m_Rigidbody.AddForceAtPosition(lift_force, fpoint);
//				}
//			}
//			else
//			{
//				//
//			}
//			if ( m_CurrPower > m_Property.m_MaxPower * 0.01f )
//			{
//				m_JetEffectGroup.gameObject.SetActive(true);
//			}
//			else
//			{
//				m_JetEffectGroup.gameObject.SetActive(false);
//			}
//		}
//	}



//	public void DrivingForward ()
//	{
//		m_zMovement = m_Controller.transform.forward;
//	}
	
//	public void DrivingBackward ()
//	{
//		m_zMovement = -m_Controller.transform.forward;
//	}
	
//	public void DrivingStay ()
//	{
//		m_zMovement = Vector3.zero;
//	}
	
//	public void TurnLeft ()
//	{
//		m_xMovement = -Vector3.Cross(m_Controller.transform.up, m_TorqueOffset).normalized;
//	}
	
//	public void TurnRight ()
//	{
//		m_xMovement = Vector3.Cross(m_Controller.transform.up, m_TorqueOffset).normalized;
//	}
	
//	public void NotTurning ()
//	{
//		m_xMovement = Vector3.zero;
//	}
	
//	public void NotDriving ()
//	{
//		m_xMovement = Vector3.zero;
//		m_zMovement = Vector3.zero;
//		m_yMovement = Vector3.zero;
//	}

//	public void ShutDown ()
//	{
//		NotDriving();
//		m_PowerTarget = 0;
//	}
//	 * */
//}
