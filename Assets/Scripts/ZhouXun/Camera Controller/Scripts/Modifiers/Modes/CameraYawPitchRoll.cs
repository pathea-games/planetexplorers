using UnityEngine;
using System.Collections;

public class CameraYawPitchRoll : CamMode
{
	public bool m_EnableRotate = true;
	public ECamKey m_RotateKey = ECamKey.CK_Mouse1;
	public float m_RotateSensitivity = 1;
	public float m_MaxPitch = 89;
	public float m_MinPitch = -89;

	public float m_Yaw = 0;
	public float m_Pitch = 0;
	public float m_Roll = 0;

	public float m_RotDamper = 15f;

	public override void ModeEnter ()
	{
		Vector3 euler = m_TargetCam.transform.eulerAngles;
		m_Yaw = euler.y;
		m_Pitch = euler.x;
		m_Roll = 0;
		if ( m_Pitch > 180 )
			m_Pitch -= 360;
	}

	public override void UserInput ()
	{
		bool rot = m_EnableRotate && (m_LockCursor || CamInput.GetKey(m_RotateKey) && !m_Controller.m_MouseOpOnGUI);
		Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
		Cursor.visible = !m_LockCursor;

		if (rot)
		{
			float delta_x = (CamInput.GetAxis(ECamKey.CK_MouseX) + CamInput.GetAxis(ECamKey.CK_JoyStickX)) * m_RotateSensitivity;
			float delta_y = (CamInput.GetAxis(ECamKey.CK_MouseY) + CamInput.GetAxis(ECamKey.CK_JoyStickY)) * m_RotateSensitivity;
			m_Yaw += delta_x;
			m_Pitch += delta_y;
		}
		m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
	}
	
	public override void Do ()
	{
		Quaternion q = Quaternion.identity;
		q.eulerAngles = new Vector3 (m_Pitch, m_Yaw, m_Roll);
		m_TargetCam.transform.rotation = Quaternion.Slerp(m_TargetCam.transform.rotation, q, Mathf.Clamp01(Time.deltaTime * m_RotDamper));
	}

	protected virtual void OnGUI ()
	{
		if ( m_ShowTarget )
			CamMediator.DrawAimTextureGUI(m_TargetViewportPos);
	}
}
