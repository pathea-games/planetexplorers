using UnityEngine;
using System.Collections;

public class CameraFirstPerson : CameraYawPitchRoll
{
	public Transform m_Character;
	private Vector3 m_CharacterPos;

	public bool m_EnableFov = true;
	public ECamKey m_FovAxis = ECamKey.CK_MouseWheel;
	public float m_FovSensitivity = 2;
	public float m_MaxFov = 20;
	public float m_MinFov = 0;

	public float m_Fov = 0;
	public float m_FovWanted = 0;

	public bool m_SyncYaw = false;
	public float m_YawDamper = 5f;
	private float m_YawWanted = 0f;
	
	public bool m_SyncRoll = false;
	public float m_RollCoef = 0.7f;
	
	public float m_FovDamper = 5f;
	public float m_PosDamper = 25f;
	public float m_RollDamper = 3f;

	public override void ModeEnter ()
	{
		base.ModeEnter();
		m_Fov = 0;
	}

	public override void UserInput ()
	{
		if ( m_SyncYaw && m_Character != null && !CamInput.GetKey(m_RotateKey) )
		{
			m_YawWanted = Mathf.Atan2(m_Character.transform.forward.x, m_Character.transform.forward.z) * Mathf.Rad2Deg;
			int trunc = Mathf.RoundToInt((m_Yaw - m_YawWanted)/360.0f);
			m_Yaw -= trunc * 360.0f;
			m_Yaw = Mathf.Lerp(m_Yaw, m_YawWanted, Mathf.Clamp01(Time.deltaTime * m_YawDamper));
		}
		base.UserInput();
		if ( m_EnableFov && !m_Controller.m_MouseOnScroll )
		{
			float delta_fov = m_FovSensitivity * CamInput.GetAxis(m_FovAxis) * 10;
			m_FovWanted += delta_fov;
			m_FovWanted = Mathf.Clamp(m_FovWanted, Mathf.Max(-30, m_MinFov), Mathf.Min(90, m_MaxFov));
			m_Fov = Mathf.Lerp(m_Fov, m_FovWanted, Mathf.Clamp01(Time.deltaTime * m_FovDamper));
		}
	}

	public override void Do ()
	{
		if ( m_Character != null )
			m_CharacterPos = m_Character.position;
		else
			m_CharacterPos = m_TargetCam.transform.position;

		if ( m_SyncRoll && m_Character != null )
		{
			float roll_wanted = 0;
			Vector3 horz_right = Vector3.Cross(Vector3.up, m_Character.forward).normalized;
			if ( horz_right.magnitude > 0.01f )
			{
				float angle_r = Vector3.Angle(horz_right, m_Character.up);
				float angle_l = 180 - angle_r;
				float angle_y = Vector3.Angle(Vector3.up, m_Character.up);
				float roll_r = 0, roll_l = 0;
				if ( angle_y < 90 )
				{
					roll_r = angle_r - 90;
					roll_l = angle_l - 90;
				}
				else
				{
					roll_r = 270 - angle_r;
					roll_l = 270 - angle_l;
				}
				roll_r *= m_RollCoef;
				roll_l *= m_RollCoef;
				roll_wanted = Mathf.Lerp(roll_r, roll_l, (1 - Vector3.Dot(m_TargetCam.transform.forward, m_Character.forward))*0.5f);
			}
			while ( m_Roll - roll_wanted > 180 )
				m_Roll -= 360;
			while ( m_Roll - roll_wanted <= -180 )
				m_Roll += 360;
			m_Roll = Mathf.Lerp(m_Roll, roll_wanted, Mathf.Clamp01(Time.deltaTime * m_RollDamper));
		}

		base.Do();
		m_TargetCam.transform.position = Vector3.Lerp(m_TargetCam.transform.position, m_CharacterPos, Mathf.Clamp01(Time.deltaTime * m_PosDamper));
		m_TargetCam.fieldOfView += m_Fov;
	}
}
