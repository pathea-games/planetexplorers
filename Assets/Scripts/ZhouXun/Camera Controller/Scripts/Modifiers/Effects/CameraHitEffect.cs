using UnityEngine;
using System.Collections;

public class CameraHitEffect : CamEffect
{
    public Vector3 m_Dir;
    private Vector3 m_TargetPos;
    public Vector3 m_RotDir;
    public AnimationCurve m_Curve;
    public float m_Distance = 1;
    public int m_Angle = -20;
    private float _time = 0;
    public override void Do ()
    {
        _time += 0.03f;
        float bias = m_Distance * m_Curve.Evaluate(_time);
        float bias_r = m_Curve.Evaluate(_time);

        m_TargetCam.transform.position = m_TargetCam.transform.position + bias * m_Dir;
        m_TargetCam.transform.Rotate(m_RotDir, m_Angle * bias_r, Space.World);
    }
}
