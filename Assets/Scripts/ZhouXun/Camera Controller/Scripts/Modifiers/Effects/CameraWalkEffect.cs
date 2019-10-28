using UnityEngine;
using System.Collections;

public class CameraWalkEffect : CamEffect
{
	public Transform m_Character;
	public Transform m_Bip;
	public float m_BiasScale = 1;

	Vector3 m_InitialOffset = Vector3.zero;
	public override void Do ()
	{
		if ( m_Bip != null && m_Character != null )
		{
			if ( m_InitialOffset.magnitude > 0.01f )
			{
				m_TargetCam.transform.position = m_TargetCam.transform.position + (m_Bip.transform.position - m_Character.transform.position - m_InitialOffset) * m_BiasScale;
			}
			m_InitialOffset = m_Bip.transform.position - m_Character.transform.position;
		}
	}
}
