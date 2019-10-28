using UnityEngine;
using System;
using RootMotion.FinalIK;

namespace PEIK
{
	[Serializable]
	public class IKMoveEffect
	{
		public IKAnimEffect m_StartEffect;
		public IKAnimEffect m_LFootStopEffect;
		public IKAnimEffect m_RFootStopEffect;

		public float m_CampAngle = 60f;
		public AnimationCurve	m_SpeedToWeight;
		public AnimationCurve	m_SpeedToTime;
		
		public void StartMove(IKSolverFullBodyBiped solver, Vector3 velocity)
		{
			//		m_StartEffect.DoEffect(this, solver.GetEffector(FullBodyBipedEffector.LeftFoot).bone.position,
			//		                       solver.GetEffector(FullBodyBipedEffector.RightFoot).bone.position, solver);
		}
		
		public void StopMove(IKSolverFullBodyBiped solver, Vector3 velocity)
		{
			velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
			StopMove(solver, velocity, ISRFootMove(solver, velocity));
		}

		public void StopMove(IKSolverFullBodyBiped solver, Vector3 velocity, bool rightFoot)
		{
			float weight = m_SpeedToWeight.Evaluate(velocity.magnitude);
			float time = m_SpeedToTime.Evaluate(velocity.magnitude);
			if(rightFoot)
			{
				m_RFootStopEffect.m_StepTime = time;
				m_RFootStopEffect.DoEffect(solver, velocity.normalized, weight);
			}
			else
			{
				m_LFootStopEffect.m_StepTime = time;
				m_LFootStopEffect.DoEffect(solver, velocity.normalized, weight);
			}
		}

		public void EndEffect()
		{
			m_StartEffect.EndEffect();
			m_RFootStopEffect.EndEffect();
			m_LFootStopEffect.EndEffect();
		}

		public bool isRunning{ get { return m_StartEffect.isRunning || m_RFootStopEffect.isRunning || m_LFootStopEffect.isRunning; } }
		
		bool ISRFootMove(IKSolverFullBodyBiped solver, Vector3 velocity)
		{
			Vector3 moveDir = Util.ProjectOntoPlane(velocity, Vector3.up);
//			float angle = Vector3.Angle(moveDir, solver.GetRoot().right);
//			if(angle < m_CampAngle)
//				return true;
//			else if(angle > 180 - m_CampAngle)
//				return false;
			Vector3 rFootToLFoot = solver.GetEffector(FullBodyBipedEffector.LeftFoot).bone.transform.position
				- solver.GetEffector(FullBodyBipedEffector.RightFoot).bone.transform.position;

			return Vector3.Angle(rFootToLFoot, moveDir) > 90f;
		}

		public void OnModifyOffset(IKSolverFullBodyBiped solver, float weight, float deltaTime)
		{
			m_StartEffect.OnModifyOffset(solver, weight, deltaTime);
			m_LFootStopEffect.OnModifyOffset(solver, weight, deltaTime);
			m_RFootStopEffect.OnModifyOffset(solver, weight, deltaTime);
		}
	}
}
