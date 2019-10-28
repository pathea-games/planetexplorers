using UnityEngine;
using System.Collections;

public class CameraSpringEffect : CamEffect
{
	public Transform m_Character;
	public Transform m_Bone;
	private Vector3 m_LastCharPos;
	private bool m_Ignore = true;

	private Vector3 s = Vector3.zero;
	private Vector3 v = Vector3.zero;
	private Vector3 a = Vector3.zero;
	private Vector3 f = Vector3.zero;
	private Vector3 s0 = Vector3.zero;

	public float k = 1;
	public float cd = 2;
	public float amp = 7;
	public float sc = 4;
	public float rot = 0.8f;

	public override void Do ()
	{
		if (m_Bone == null)
			return;
		if (m_Character == null)
			return;
		Vector3 CharPos = m_Bone.position;
		float dist = (m_TargetCam.transform.position - CharPos).magnitude;
		if (!m_Ignore)
		{
			float dt = Mathf.Clamp(Time.deltaTime, 0.001f, 0.025f) * sc;
			s0 = Vector3.Lerp(s0, CharPos - m_LastCharPos, 0.3f);
			s0 = Vector3.ClampMagnitude(s0, 1f);
			a = (s0 - s) * k;
			f = -v * cd;
			v = (a+f) * dt + v;
			s = v * dt + s;
			s = Vector3.ClampMagnitude(s, dist / amp);
			Vector3 bias = -s * amp;
			bias = Vector3.ClampMagnitude(bias, dist);
			m_TargetCam.transform.position += bias;
			//Vector3 tp = m_Character.position + bias * Mathf.Clamp01(1f-rot);
			//m_TargetCam.transform.LookAt(tp);
		}
		m_LastCharPos = m_Bone.position;
		m_Ignore = false;
	}
}
