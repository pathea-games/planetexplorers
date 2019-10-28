using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace PEIK
{
	public class IKHumanMove : IKOffsetModifier 
	{
		[System.Serializable]
		public class BodyTiltForward
		{
			/// <summary>
			/// Linking this to an effector
			/// </summary>
			[System.Serializable]
			public class EffectorLink 
			{
				public FullBodyBipedEffector m_Effector;

				public Vector3 m_Offset;
				[Range(0.1f, 100f)]
				public float	m_TileAcc = 5f; // The acceleration, smaller values means lazyer following
				public AnimationCurve m_TileWeight;
				
				private float 	m_ProVelocity;

				public void Reset()
				{
					m_ProVelocity = 0;
				}

				public void Apply(IKSolverFullBodyBiped solver, float m_Weight, Vector3 curentVelocity, float deltaTime)
				{
					//Update forward tilt
					Vector3 forwardPro = Vector3.Project(curentVelocity, solver.GetRoot().forward);
					float curProVelocity = forwardPro.magnitude;
					if(Vector3.Angle(forwardPro, solver.GetRoot().forward) > 150)
						curProVelocity *= -1;
					m_ProVelocity = Mathf.Lerp(m_ProVelocity, curProVelocity, m_TileAcc * deltaTime);

					//Apply offset
						solver.GetEffector(m_Effector).positionOffset += solver.GetRoot().TransformDirection(m_Offset * m_TileWeight.Evaluate(m_ProVelocity)) * m_Weight;
				}
			}

			public EffectorLink[] m_EffectorLinks; // Linking the body to effectors. One Body can be used to offset more than one effector.

			private Vector3 m_LastPosition;
			private Vector3 m_Velocity;

			[Range(-10f, 10f)]
			public float 	m_SubForward; //Addition forward state. 1: forward -1:backward 
			
			// Reset to Transform
			public void Reset(IKSolverFullBodyBiped solver)
			{
				m_LastPosition = solver.GetRoot().position;
				foreach(EffectorLink effectorLink in m_EffectorLinks)
					effectorLink.Reset();
			}
			
			// Update this body, apply the offset to the effector
			public void Update(IKSolverFullBodyBiped solver, float m_Weight, float deltaTime) 
			{
				m_Velocity = (solver.GetRoot().position - m_LastPosition) / deltaTime;
				
				// Apply position offset to the effector
				foreach (EffectorLink effectorLink in m_EffectorLinks)
					effectorLink.Apply(solver, m_Weight, m_Velocity + solver.GetRoot().forward * m_SubForward, deltaTime);
				
				m_LastPosition = solver.GetRoot().position;
			}
		}
		
		public BodyTiltForward m_BodyTiltForward; // The array of Bodie

		[System.Serializable]
		public class BodyTiltSide
		{
			[System.Serializable]
			public class EffectorLink 
			{
				public FullBodyBipedEffector m_Effector; // The effector type (this is just an enum)
				public Vector3 m_Offset; // Offset of the effector in this pose
				public Vector3 m_Pin; // Pin position relative to the solver root Transform
				public Vector3 m_PinWeight; // m_Pin m_Weight vector
				
				// Apply positionOffset to the effector
				public void Apply(IKSolverFullBodyBiped solver, float m_Weight, Quaternion rotation) 
				{
					// Offset
					solver.GetEffector(m_Effector).positionOffset += rotation * m_Offset * m_Weight;
					
					// Calculating pinned position
					Vector3 pinPosition = solver.GetRoot().position + rotation * m_Pin;
					Vector3 pinPositionOffset = pinPosition - solver.GetEffector(m_Effector).bone.position;
					
					Vector3 pinWeightVector = m_PinWeight * Mathf.Abs(m_Weight);
					
					// Lerping to pinned position
					solver.GetEffector(m_Effector).positionOffset = new Vector3(
						Mathf.Lerp(solver.GetEffector(m_Effector).positionOffset.x, pinPositionOffset.x, pinWeightVector.x),
						Mathf.Lerp(solver.GetEffector(m_Effector).positionOffset.y, pinPositionOffset.y, pinWeightVector.y),
						Mathf.Lerp(solver.GetEffector(m_Effector).positionOffset.z, pinPositionOffset.z, pinWeightVector.z)
						);
				}
			}

			public EffectorLink[] m_TurnLeftEffectorLinks;
			public EffectorLink[] m_TurnRightEffectorLinks;

			public float m_TiltSpeed = 6f; // Speed of tilting
			public float m_TiltSensitivity = 0.07f; // Sensitivity of tilting

			private float m_TiltAngle;
			private Vector3 m_LastForward;

			[Range(-1, 1)]
			public float testAngle; //Addition rot state  -1<- 0 ->1

			public void Reset(IKSolverFullBodyBiped solver) 
			{
				m_LastForward = solver.GetRoot().forward;
			}

			public void Update(IKSolverFullBodyBiped solver, float m_Weight, float deltaTime)
			{
				// Calculate the angular delta in character rotation
				Quaternion change = Quaternion.FromToRotation(m_LastForward, solver.GetRoot().forward);
				float deltaAngle = 0;
				Vector3 axis = Vector3.zero;
				change.ToAngleAxis(out deltaAngle, out axis);
				if (axis.y > 0) deltaAngle = -deltaAngle;
				
				deltaAngle *= m_TiltSensitivity * 0.01f;
				deltaAngle /= deltaTime;
				deltaAngle = Mathf.Clamp(deltaAngle, -1f, 1f);
				
				m_TiltAngle = Mathf.Lerp(m_TiltAngle, deltaAngle, deltaTime * m_TiltSpeed);
				
				// Applying positionOffsets
				float tiltF = Mathf.Abs(m_TiltAngle) / 1f * m_Weight + Mathf.Abs(testAngle);
				if (m_TiltAngle - testAngle < 0)
				{
					foreach(EffectorLink effectorLink in m_TurnRightEffectorLinks)
						effectorLink.Apply(solver, tiltF, solver.GetRoot().rotation);
				}
				else
				{
					foreach(EffectorLink effectorLink in m_TurnLeftEffectorLinks)
						effectorLink.Apply(solver, tiltF, solver.GetRoot().rotation);
				}
				
				// Store current character forward axis and Time
				m_LastForward = solver.GetRoot().forward;
			}
		}

		public BodyTiltSide m_BodyTiltSide;

		// FadeOutEffect when SetActive(false)
		public float m_FadeOutTime = 0.2f; 
		private float m_FadeWeight = 1f;

		private IEnumerator FadeUpdate(bool active)
		{
			while(m_FadeWeight > 0)
			{
				m_FadeWeight -= deltaTime / m_FadeOutTime;
				yield return null;
			}
			m_FadeWeight = 0;
		}

		public void SetActive(bool active)
		{
			StartCoroutine(FadeUpdate(active));
		}

		#region implemented abstract members of IKOffsetModifier

		protected override void OnModifyOffset ()
		{
			// Update the bodyInertias
			if(m_FadeWeight > 0)
			{
				m_BodyTiltForward.Update(m_FBBIK.solver, m_Weight * m_FadeWeight, deltaTime);
				m_BodyTiltSide.Update(m_FBBIK.solver, m_Weight * m_FadeWeight, deltaTime);
			}
		}

		protected override void OnInit ()
		{
			m_BodyTiltForward.Reset(m_FBBIK.solver);
			m_BodyTiltSide.Reset(m_FBBIK.solver);
			m_FadeWeight = 1f;
		}

		#endregion
	}
}