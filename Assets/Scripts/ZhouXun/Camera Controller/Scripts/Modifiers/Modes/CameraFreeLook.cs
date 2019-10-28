using UnityEngine;
using System.Collections;

public class CameraFreeLook : CameraYawPitchRoll
{
	public ECamKey m_MoveLeftKey = ECamKey.CK_MoveLeft;
	public ECamKey m_MoveRightKey = ECamKey.CK_MoveRight;
	public ECamKey m_MoveForwardKey = ECamKey.CK_MoveForward;
	public ECamKey m_MoveBackKey = ECamKey.CK_MoveBack;
	public ECamKey m_MoveUpKey = ECamKey.CK_Null;
	public ECamKey m_MoveDownKey = ECamKey.CK_Null;

	public float m_MoveSpeed = 10;
	public float m_MoveAcceleration = 2f;

	private float m_CurrSpeed;
	private Vector3 m_MoveDir;

	public override void UserInput ()
	{
		base.UserInput();
		Vector3 move = Vector3.zero;
		if ( CamInput.GetKey(m_MoveLeftKey) )
			move -= m_TargetCam.transform.right;
		if ( CamInput.GetKey(m_MoveRightKey) )
			move += m_TargetCam.transform.right;
		if ( CamInput.GetKey(m_MoveForwardKey) )
			move += m_TargetCam.transform.forward;
		if ( CamInput.GetKey(m_MoveBackKey) )
			move -= m_TargetCam.transform.forward;
		if ( CamInput.GetKey(m_MoveUpKey) )
			move += m_TargetCam.transform.up;
		if ( CamInput.GetKey(m_MoveDownKey) )
			move -= m_TargetCam.transform.up;

		m_MoveDir = move;

		if ( move.sqrMagnitude > 0 )
			m_CurrSpeed += (m_MoveAcceleration * Time.deltaTime);
		else
			m_CurrSpeed = m_MoveSpeed;
	}

	public override void Do ()
	{
		m_TargetCam.transform.position += (m_MoveDir*m_CurrSpeed*Time.deltaTime);
		base.Do();
	}
}
