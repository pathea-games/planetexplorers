using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace PEIK
{
	public class IKHumanInertia : IKOffsetModifier
	{
		[System.Serializable]
		public class InertiaEffect
		{
			[System.Serializable]
			public class InertiaBody
			{
				[System.Serializable]
				public class EffectorLink
				{
					public FullBodyBipedEffector m_Effector;
					public float m_Weight = 1f;
				}

				public EffectorLink[] 	m_EffectorLink;

				public Transform 		m_FollowTarget;
				
				[Range(0.1f, 100f)]
				public float			m_InertiaAcc = 5f;

				public float			m_MachVelocity = 0.2f;

				public AnimationCurve 	m_VelocityFacter;
				
				private Vector3 		m_LastVelocity;

				private Vector3 		m_LazyPoint;
				
				public void Init()
				{
					m_LastVelocity = Vector3.zero;
					m_LazyPoint = m_FollowTarget.position;
				}
				
				public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime, float velocity)
				{
					//Update inertia
					Vector3 delta = m_FollowTarget.position - m_LazyPoint;
					
					Vector3 currentSpeed = delta / deltaTime;
					Vector3 vecChgange = currentSpeed - m_LastVelocity;
					float dTime = vecChgange.magnitude / m_InertiaAcc;
					dTime = Mathf.Min(dTime, deltaTime);
					m_LastVelocity += vecChgange.normalized * m_InertiaAcc * dTime;

					//
					m_LazyPoint += m_LastVelocity * deltaTime;
					
					delta = m_FollowTarget.position - m_LazyPoint;
					
					// Match velocity
					m_LazyPoint += delta * m_MachVelocity;
					
					delta = m_LazyPoint - m_FollowTarget.position;
					
					foreach(EffectorLink effectorLink in m_EffectorLink)
						solver.GetEffector(effectorLink.m_Effector).positionOffset += delta * effectorLink.m_Weight * weight * m_VelocityFacter.Evaluate(velocity);
				}
			}

			public InertiaBody[] m_Bodies;

			public float m_Weight = 1f;

			public float m_FadeTime = 0.2f;

			private float m_FadeWeight = 1f;

			private Vector3 m_LastPos = Vector3.zero;

			public void Init()
			{
				foreach(InertiaBody body in m_Bodies)
					body.Init();
			}

			public IEnumerator UpdateFadeState(bool fadein)
			{
				if(fadein)
				{
					while(m_FadeWeight < 1f - PETools.PEMath.Epsilon)
					{
						if(m_FadeTime > 0)
							m_FadeWeight = Mathf.Clamp01(m_FadeWeight + Time.deltaTime / m_FadeTime);
						else
							m_FadeWeight = 1f;
						yield return null;
					}
				}
				else
				{
					while(m_FadeWeight > PETools.PEMath.Epsilon)
					{
						if(m_FadeTime > 0)
							m_FadeWeight = Mathf.Clamp01(m_FadeWeight - Time.deltaTime / m_FadeTime);
						else
							m_FadeWeight = 0;
						yield return null;
					}
				}
			}

			public void Update(IKSolverFullBodyBiped solver, float deltaTime)
			{
				float velocity = (solver.GetRoot().position - m_LastPos).magnitude / deltaTime;
				m_LastPos = solver.GetRoot().position;
				foreach(InertiaBody body in m_Bodies)
					body.Update(solver, m_Weight * m_FadeWeight, deltaTime, velocity);
			}
		}

		public enum InertiaType
		{
			Null,
			Move,
			Blow
		}

		public InertiaEffect m_MoveInertia;

		public InertiaEffect m_BlowInertia;

		public void SetType(InertiaType type)
		{
			StopAllCoroutines();
			StartCoroutine(m_MoveInertia.UpdateFadeState(type == InertiaType.Move));
			StartCoroutine(m_BlowInertia.UpdateFadeState(type == InertiaType.Blow));
		}

		#region implemented abstract members of IKOffsetModifier

		protected override void OnModifyOffset ()
		{
			m_MoveInertia.Update(m_FBBIK.solver, deltaTime);
			m_BlowInertia.Update(m_FBBIK.solver, deltaTime);
		}

		protected override void OnInit ()
		{
			m_MoveInertia.Init();
			m_BlowInertia.Init();
			SetType(InertiaType.Move);
		}

		#endregion
	}
}