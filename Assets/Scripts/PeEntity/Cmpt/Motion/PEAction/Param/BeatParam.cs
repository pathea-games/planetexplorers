using UnityEngine;
using System.Collections;

public class BeatParam : MonoBehaviour 
{
	[Header("HitIKParam")]
	public AnimationCurve m_AngleThresholdScale;
	public AnimationCurve m_AngleForceScale;
	public AnimationCurve m_ForceToHitWeight;
	public AnimationCurve m_ForceToHitTime;
	
	public float m_RandomScale = 0.1f;
	public float m_ThresholdScaleInAir = 0.5f;

	[Header("RepulsedParam")]
	public AnimationCurve 	m_TimeVelocityScale;
	public AnimationCurve 	m_ForceToVelocity;
	public AnimationCurve 	m_ForceToMoveTime;
	public AnimationCurve 	m_ApplyMoveStopTime;
	public AnimationCurve 	m_WentflyTimeCurve;
	public float			m_ToWentflyForceScale = 0.5f;
	public Transform		m_ApplyWentflyBone;

    [Header("RepulsedParamMonster")]
    [Range(0.0f, 1.0f)]
    public float repulsedProb = 0.3f;
    [Range(0.0f, 1.0f)]
    public float repulsedDamp = 0.5f;
}