//using UnityEngine;
//using System.Collections;

//public class VCPVtolRotorFunc : VCPartFunc
//{
//	private static float s_FullRPM = 2743F;

//	public VCPVtolRotorProperty m_Property;
////	public HelicopterController m_Controller;
	
//	public Transform m_PropellorSteer;
//	public Transform m_PropellorModel;
//	public float m_BalancePosition = 0;

//	// Rotor Status
//	public float m_CurrPower;
//	public float m_CurrForwardAngle;
//	public float m_CurrTurningAngle;
//	public float m_PowerTarget;
//	public Vector2 m_CurrSteering;
//	public Vector2 m_SteeringTarget;
//	public float m_CurrRPM;
//	private float m_RotorTheta;
//	private float m_MotorError = 0;

//	public ScriptableWindzoneInterface m_Windzone;

//	public AudioSource m_RunningSound;
//	public AudioSource m_IncreaseSound;
//	public AudioSource m_DecreaseSound;
//	public float m_MaxSoundVolume;
//	public bool m_CanPlaySound = true;
//	public float m_SoundPhase = 0;
//	/*
//	void Start ()
//	{
//		if ( m_Controller != null )
//		{
//			m_CurrPower = 0;
//			m_CurrForwardAngle = 0;
//			m_CurrTurningAngle = 0;
//			m_PowerTarget = 0;
//			m_CurrSteering = Vector2.zero;
//			m_SteeringTarget = Vector2.zero;
//		}
//	}

//	float running_sound_power = 0;
//	void Update ()
//	{
//		if ( m_Controller != null )
//		{
//			if ( m_CanPlaySound )
//			{
//				if ( m_RunningSound != null )
//				{
//					float power = Mathf.Clamp01(Mathf.Abs(m_CurrRPM - 30) / s_FullRPM);
//					running_sound_power = Mathf.Lerp(running_sound_power, power, 0.1f);
//					if ( running_sound_power > 0.005f )
//					{
//						m_RunningSound.volume = Mathf.Clamp01(running_sound_power*4) * m_MaxSoundVolume * VCGameMediator.SEVol;
//						m_RunningSound.pitch = Mathf.Clamp01(Mathf.Pow(running_sound_power+0.25f, 0.5f));
//						if ( !m_RunningSound.isPlaying )
//						{
//							m_RunningSound.time = m_SoundPhase * m_RunningSound.clip.length;
//							m_RunningSound.Play();
//						}
//					}
//					else
//					{
//						if ( m_RunningSound.isPlaying )
//							m_RunningSound.Stop();
//					}
//				}
//			}
//		}
//		else
//		{
//			if ( m_RunningSound != null )
//				if ( m_RunningSound.isPlaying )
//					m_RunningSound.Stop();
//		}
//	}

//	void FixedUpdate ()
//	{
//		if ( m_Controller != null )
//		{
//			if ( m_Controller.m_NetChar == ENetCharacter.nrOwner )
//			{
//				// rotor theta
//				float rpm = s_FullRPM * m_CurrPower / m_Property.m_MaxPower;
//				if ( m_CurrRPM - rpm > 0.02f )
//					m_CurrRPM = Mathf.Lerp(m_CurrRPM, rpm, m_Property.m_FreeRotorDrag);
//				else
//					m_CurrRPM = rpm;
//			}

//			m_RotorTheta += ((m_CurrRPM / 60F) * 360F) * Time.deltaTime;
//			while ( m_RotorTheta > 180 )
//				m_RotorTheta -= 360;
//			while ( m_RotorTheta < -180 )
//				m_RotorTheta += 360;
//			m_PropellorModel.localEulerAngles = new Vector3 (0, m_RotorTheta, 0);

//			if ( m_Controller.m_NetChar == ENetCharacter.nrOwner )
//			{
//				// current power
//				if ( Mathf.Abs(m_PowerTarget - m_CurrPower) < 0.01f )
//					m_CurrPower = m_PowerTarget;
//				else
//					m_CurrPower = Mathf.Lerp(m_CurrPower, m_PowerTarget, 0.3f);
//				if ( m_Controller.m_Rigidbody != null && m_Controller.m_Active )
//				{
//					m_MotorError = Mathf.Sin(Time.time*1.5f) * 0.15f + 1f;
//					float force = m_CurrPower * m_MotorError * m_Property.m_Eta * 10F * 
//								  transform.localScale.x * transform.localScale.z * 
//								  m_Controller.m_AtmosphereCoef;
//					Vector3 lift_force = transform.up * force;
//					// apply this force to rigidbody
//					m_Controller.m_Rigidbody.AddForceAtPosition(lift_force, Vector3.Lerp(m_PropellorModel.position, m_Controller.m_Rigidbody.worldCenterOfMass, 0.9f));

//					// steering
//					if ( Vector2.Distance(m_CurrSteering, m_SteeringTarget) < 0.001f )
//						m_CurrSteering = m_SteeringTarget;
//					else
//						m_CurrSteering = Vector2.Lerp(m_CurrSteering, m_SteeringTarget, 0.2f);

//					m_CurrForwardAngle = m_CurrSteering.y * m_Property.m_MaxForwardAngle;
//					m_CurrTurningAngle = m_CurrSteering.x * m_Property.m_MaxTurningAngle * m_BalancePosition;
//					m_PropellorSteer.localEulerAngles = new Vector3 (m_CurrForwardAngle, 0, -m_CurrTurningAngle);
//					m_Controller.m_Rigidbody.AddForce(m_CurrSteering.y * force * m_Controller.transform.forward);
//					m_Controller.m_Rigidbody.AddRelativeTorque(m_CurrSteering.x * Time.fixedDeltaTime * 5.0f / (float)m_Controller.m_Rotors.Count * Vector3.up, ForceMode.VelocityChange);
//				}
//			}
//			else
//			{
//				m_CurrForwardAngle = m_CurrSteering.y * m_Property.m_MaxForwardAngle;
//				m_CurrTurningAngle = m_CurrSteering.x * m_Property.m_MaxTurningAngle * m_BalancePosition;
//				m_PropellorSteer.localEulerAngles = new Vector3 (m_CurrForwardAngle, 0, -m_CurrTurningAngle);
//			}
//		}
//	}

//	public void DrivingForward ()
//	{
//		m_SteeringTarget.y = 1;
//	}
	
//	public void DrivingBackward ()
//	{
//		m_SteeringTarget.y = -1;
//	}
	
//	public void DrivingStay ()
//	{
//		m_SteeringTarget.y = 0;
//	}
	
//	public void TurnLeft ()
//	{
//		m_SteeringTarget.x = -1;
//	}
	
//	public void TurnRight ()
//	{
//		m_SteeringTarget.x = 1;
//	}
	
//	public void NotTurning ()
//	{
//		m_SteeringTarget.x = 0;
//	}
	
//	public void NotDriving ()
//	{
//		m_SteeringTarget = Vector2.zero;
//	}

//	public void ShutDown ()
//	{
//		NotDriving();
//		m_PowerTarget = 0;
//	}
//	 * */
//}
