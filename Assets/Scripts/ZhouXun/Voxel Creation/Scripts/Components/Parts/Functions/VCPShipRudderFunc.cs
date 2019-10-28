using UnityEngine;
using System.Collections;

//public class VCPShipRudderFunc : VCPartFunc
//{
	
//	public VCPShipRudderProperty m_Property;
////	public BoatController m_Controller;

//	public Transform m_RudderSteering;
//	public Transform m_RudderFace;
//	public float m_CurrSteering;
//	public float m_SteeringTarget;
//	public float m_CurrTurningAngle;

//	/*
//	void Start ()
//	{
//		if ( m_Controller != null )
//		{
//			m_CurrSteering = 0;
//			m_SteeringTarget = 0;
//		}
//	}

//	void FixedUpdate ()
//	{
//		if ( m_Controller != null )
//		{
//			if ( m_Controller.m_NetChar == ENetCharacter.nrOwner )
//			{
//				if ( m_Controller.m_Rigidbody != null && m_Controller.m_Active )
//				{
//					// steering
//					if ( Mathf.Abs(m_CurrSteering - m_SteeringTarget) < 0.001f )
//						m_CurrSteering = m_SteeringTarget;
//					else
//						m_CurrSteering = Mathf.Lerp(m_CurrSteering, m_SteeringTarget, Time.fixedDeltaTime * 3f);
					
//					m_CurrTurningAngle = m_CurrSteering * m_Property.m_MaxTurningAngle;
//					m_RudderSteering.localEulerAngles = new Vector3 (0, -m_CurrTurningAngle, 0);

//					float dot = -Vector3.Dot(m_Controller.GetComponent<Rigidbody>().velocity, m_Controller.transform.forward);
//					float scale = m_Controller.m_Rigidbody.mass / 1000;
//					scale = Mathf.Clamp(scale, 40, 100);
//					float force = Mathf.Pow(Mathf.Abs(dot), 0.7f) * Mathf.Sign(dot) * scale
//						* m_Property.m_YawingCoef * transform.localScale.y * transform.localScale.z 
//						* Mathf.Sin(m_CurrTurningAngle*Mathf.Deg2Rad);
//					Vector3 point = m_RudderFace.position;
//					Vector3 thrust = m_RudderFace.right * force;
//					point.y = m_Controller.m_Rigidbody.worldCenterOfMass.y;
//					m_Controller.m_Rigidbody.AddForceAtPosition(thrust, point);
//				}
//			}
//			else
//			{
//				m_CurrTurningAngle = m_CurrSteering * m_Property.m_MaxTurningAngle;
//				m_RudderSteering.localEulerAngles = new Vector3 (0, m_CurrTurningAngle, 0);
//			}
//		}
//	}

//	public void TurnLeft ()
//	{
//		m_SteeringTarget = -1;
//	}
	
//	public void TurnRight ()
//	{
//		m_SteeringTarget = 1;
//	}
	
//	public void NotTurning ()
//	{
//		m_SteeringTarget = 0;
//	}
//	 * */
//}
