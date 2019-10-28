using UnityEngine;
using System.Collections;

public class CameraAutoThirdPerson : CameraThirdPerson
{
	public bool m_AutoFollow = false;
	public float m_AutoFollowDelay = 1f;
	private Vector3 _lastCharacterPos = Vector3.zero;
	private float _moving_time = 0f;
	public override void UserInput ()
	{
		base.UserInput ();
		if ( m_Character != null )
		{
			Vector3 forward = m_Character.transform.forward;
			if ( m_AutoFollow )
			{
				if ( !CamInput.GetKey(m_RotateKey) )
				{
					if ( _lastCharacterPos.sqrMagnitude > 0.01f )
					{
						float ofs = (m_Character.position - _lastCharacterPos).magnitude;
						if ( ofs > 0.03f )
						{
							if ( _moving_time > m_AutoFollowDelay )
							{
								m_YawWanted = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
								int trunc = Mathf.RoundToInt((m_Yaw - m_YawWanted)/360.0f);
								m_Yaw -= trunc * 360.0f;
								m_Yaw = Mathf.Lerp(m_Yaw, m_YawWanted, Mathf.Clamp01(0.03f * m_YawDamper));
							}
							_moving_time += Time.deltaTime;
						}
						else
						{
							if ( _moving_time > m_AutoFollowDelay )
								_moving_time = m_AutoFollowDelay;
							_moving_time -= Time.deltaTime;
							if ( _moving_time < 0 )
								_moving_time = 0;
						}
					}
					else
					{
						_moving_time = 0;
					}
				}
			}
			_lastCharacterPos = m_Character.position;
		}
	}
}
