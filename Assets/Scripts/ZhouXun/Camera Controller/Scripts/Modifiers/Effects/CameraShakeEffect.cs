using UnityEngine;
using System.Collections;

public class CameraShakeEffect : CamEffect
{
	public float m_Multiplier = 1f;
	public float m_AmplitudeY = 0.08f;
	public float m_AmplitudeXZ = 0.02f;
	public float m_AmplitudeRoll = 0.0f;
	public float m_Damper = 5F;
	public float m_Freq = 20;
	public float m_FreqAdder = 0;
	public bool m_ShakeNow = false;
	public AnimationCurve m_Curve;
	private float _currAmplitude_y = 0;
	private float _currAmplitude_xz = 0;
	private float _currAmplitude_r = 0;
	private float _freq = 0;
	private float _time = 0;
	public override void Do ()
	{
		if ( m_ShakeNow )
		{
			m_ShakeNow = false;
			Shake();
		}

		float damper_1 = Mathf.Clamp01(1F - Time.deltaTime * m_Damper);
		_currAmplitude_y *= damper_1;
		_currAmplitude_xz *= damper_1;
		_currAmplitude_r *= damper_1;
		_freq += m_FreqAdder * 0.02f;
		_time += _freq * 0.02f;
		float bias_y = m_Multiplier * _currAmplitude_y * m_Curve.Evaluate(_time);
		float bias_x = m_Multiplier * _currAmplitude_xz * m_Curve.Evaluate(_time*0.81f+0.2f);
		float bias_z = m_Multiplier * _currAmplitude_xz * m_Curve.Evaluate(_time*1.13f+0.4f);
		float bias_r = m_Multiplier * _currAmplitude_r * m_Curve.Evaluate(_time);
		m_TargetCam.transform.position = m_TargetCam.transform.position + new Vector3(bias_x, bias_y, bias_z);
		m_TargetCam.transform.Rotate(m_TargetCam.transform.forward, bias_r, Space.World);
	}
	public void Shake ()
	{
		_currAmplitude_y = m_AmplitudeY;
		_currAmplitude_xz = m_AmplitudeXZ;
		_currAmplitude_r = m_AmplitudeRoll;
		_freq = m_Freq;
		_time = 0;
	}
}
