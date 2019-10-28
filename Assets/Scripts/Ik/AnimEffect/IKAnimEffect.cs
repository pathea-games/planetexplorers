using UnityEngine;
using System;
using System.Collections;
using RootMotion.FinalIK;
using PEIK;

[Serializable]
public class IKAnimEffect
{
	public Vector3 	m_EffectDir = Vector3.forward;
	public float	m_Weight = 1f;
	[Range(0.01f, 10f)]
	public float m_StepTime = 2f;
	[Range(0.01f, 1f)]
	public float m_FadeTime = 0.1f;
	
	private Vector3 m_StartPos;
	//private Vector3 m_DeltaDir;
	private float m_RemainingTime;
	
	#if UNITY_EDITOR
	public bool m_Test = false;
	#endif
	
	[Serializable]
	public class EffectortLink
	{
		public string m_Name;
		public FullBodyBipedEffector m_Effector;
		public AnimationCurve m_ForceDir;
		public AnimationCurve m_UpDir;
		public bool m_PinWordPos = false; // Wether pin effect in world position
		
		public void OnModifyOffset (IKSolverFullBodyBiped solver, Vector3 dir, float timeP, float weight, Vector3 deltaDir)
		{
			if(null == solver)
				return;
			solver.GetEffector(m_Effector).positionOffset 
				+= (m_ForceDir.Evaluate(timeP) * dir + m_UpDir.Evaluate(timeP) * Vector3.up - (m_PinWordPos?deltaDir:Vector3.zero)) * weight;
		}
	}
	
	public EffectortLink[] m_Effects;
	
	public void DoEffect(IKSolverFullBodyBiped solver, Vector3 dir, float weight)
	{
		if(null == solver)
			return;
		m_EffectDir = dir;
		m_Weight = weight;
		m_StartPos = solver.GetRoot().position;
		//m_DeltaDir = Vector3.zero;
		m_RemainingTime = m_StepTime;
	}

	public void EndEffect()
	{
		m_RemainingTime = 0;
	}

	public bool isRunning{ get{ return m_RemainingTime > 0f; } }
	
	public void OnModifyOffset (IKSolverFullBodyBiped solver, float weight, float deltaTime)
	{
		if(null == solver)
		{
			m_RemainingTime = 0;
			return;
		}
		if(m_RemainingTime > 0f)
		{
			float elapseTime = m_StepTime - m_RemainingTime;
			float crossFadeWeight = Mathf.Clamp01(elapseTime / m_FadeTime);
			Vector3 deltaDir = solver.GetRoot().position - m_StartPos;
			foreach(EffectortLink effect in m_Effects)
				effect.OnModifyOffset(solver, m_EffectDir, elapseTime / m_StepTime, weight * crossFadeWeight * m_Weight, deltaDir);
			m_RemainingTime -= deltaTime;
		}
		#if UNITY_EDITOR
		if(m_Test)
		{
			m_Test = false;
			DoEffect(solver, m_EffectDir, m_Weight);
		}
		#endif
	}
}
