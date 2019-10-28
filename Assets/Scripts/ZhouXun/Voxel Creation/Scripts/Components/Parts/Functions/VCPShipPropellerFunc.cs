using UnityEngine;
using System.Collections;

//public class VCPShipPropellerFunc : VCPartFunc
//{
	
//	private static float s_FullRPM = 347F;

//	public VCPShipPropellerProperty m_Property;
////	public BoatController m_Controller;
	
//	public Transform m_PropellorSteer;
//	public Transform m_PropellorModel;
//	public float m_BalancePosition = 0;

//	// Propeller Status
//	public float m_CurrPower;
//	public float m_CurrForwardAngle;
//	public float m_CurrTurningAngle;
//	public float m_PowerTarget;
//	public float m_CurrSteering;
//	public float m_SteeringTarget;
//	public float m_CurrRPM;
//	private float m_PropellerTheta;
//	private float m_MotorError = 0;
	/*
	void Start ()
	{
		if ( m_Controller != null )
		{
			m_CurrPower = 0;
			m_CurrForwardAngle = 0;
			m_CurrTurningAngle = 0;
			m_PowerTarget = 0;
			m_CurrSteering = 0;
			m_SteeringTarget = 0;
		}
	}

	void FixedUpdate ()
	{
		if ( m_Controller != null )
		{
			if ( m_Controller.m_NetChar == ENetCharacter.nrOwner )
			{
				// rotor theta
				float rpm = s_FullRPM * m_CurrPower / m_Property.m_MaxPower;
				m_CurrRPM = Mathf.Lerp(m_CurrRPM, rpm, Time.fixedDeltaTime);
			}

			m_PropellerTheta += ((m_CurrRPM / 60F) * 360F) * Time.deltaTime;
			while ( m_PropellerTheta > 180 )
				m_PropellerTheta -= 360;
			while ( m_PropellerTheta < -180 )
				m_PropellerTheta += 360;
			m_PropellorModel.localEulerAngles = new Vector3 (0, 0, m_PropellerTheta);

			if ( m_Controller.m_NetChar == ENetCharacter.nrOwner )
			{
				// current power
				if ( Mathf.Abs(m_PowerTarget - m_CurrPower) < 0.01f )
					m_CurrPower = m_PowerTarget;
				else
					m_CurrPower = Mathf.Lerp(m_CurrPower, m_PowerTarget, Time.deltaTime * 0.3f);
				if ( m_Controller.m_Rigidbody != null && m_Controller.m_Active )
				{
					// steering
					if ( Mathf.Abs(m_CurrSteering - m_SteeringTarget) < 0.001f )
						m_CurrSteering = m_SteeringTarget;
					else
						m_CurrSteering = Mathf.Lerp(m_CurrSteering, m_SteeringTarget, 0.2f);
					
					m_CurrTurningAngle = m_CurrSteering * m_Property.m_MaxTurningAngle * m_BalancePosition;
					m_PropellorSteer.localEulerAngles = new Vector3 (0, m_CurrTurningAngle, 0);

					Vector3 point = m_PropellorModel.position;
					float scale = m_Controller.m_Rigidbody.mass / 1000;
					scale = Mathf.Clamp(scale, 50, 100);
					float power = Mathf.Max(0, (Mathf.Abs(m_CurrPower) - m_Property.m_MaxPower * 0.1f) / 0.9f) * Mathf.Sign(m_CurrPower);
					float force = power * m_Property.m_Eta * scale * 
						transform.localScale.x * transform.localScale.y * 
						m_Controller.FluidDisplacement(m_PropellorModel.position);
					Vector3 thrust = m_PropellorSteer.forward * force;
					// apply this force to rigidbody
					point.y = m_Controller.m_Rigidbody.worldCenterOfMass.y;
					m_Controller.m_Rigidbody.AddForceAtPosition(thrust, point);
				}
			}
			else
			{
				m_CurrTurningAngle = m_CurrSteering * m_Property.m_MaxTurningAngle * m_BalancePosition;
				m_PropellorSteer.localEulerAngles = new Vector3 (0, m_CurrTurningAngle, 0);
			}
		}
	}

	public void DrivingForward ()
	{
		m_PowerTarget = m_Property.m_MaxPower;
	}
	
	public void DrivingBackward ()
	{
		m_PowerTarget = -m_Property.m_MaxPower;
	}
	
	public void DrivingStay ()
	{
		m_PowerTarget = 0;
	}
	
	public void TurnLeft ()
	{
		m_SteeringTarget = -1;
	}
	
	public void TurnRight ()
	{
		m_SteeringTarget = 1;
	}
	
	public void NotTurning ()
	{
		m_SteeringTarget = 0;
	}
	
	public void NotDriving ()
	{
		m_PowerTarget = 0;
		m_SteeringTarget = 0;
	}
	 * */
//}
