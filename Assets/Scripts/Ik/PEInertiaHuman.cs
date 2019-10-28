using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.FinalIK.Demos;

namespace PEIK
{
	public class PEInertiaHuman : OffsetModifier 
	{
		[System.Serializable]
		public class Body {

			/// <summary>
			/// Linking this to an effector
			/// </summary>
			[System.Serializable]
			public class EffectorLink {
				public FullBodyBipedEffector effector;
				public float weight;
			}

			public Transform transform; // The Transform to follow, can be any bone of the character
			public EffectorLink[] effectorLinks; // Linking the body to effectors. One Body can be used to offset more than one effector.
			[Range(0.01f, 100f)] 
			public float acceleration = 5f; // The acceleration, smaller values means lazyer following
//			public AnimationCurve matchVelocity;
			public float matchVelocity = 0.3f;

			private Vector3 delta;
			private Vector3 lazyPoint;
			private Vector3 direction;
			//private Vector3 lastPosition;
			private bool firstUpdate = true;

			// Reset to Transform
			public void Reset() {
				if (transform == null) return;
				lazyPoint = transform.position;
				//lastPosition = transform.position;
				direction = Vector3.zero;
			}

			// Update this body, apply the offset to the effector
			public void Update(IKSolverFullBodyBiped solver, float weight, float deltaTime) {
				if (transform == null) return;

				// If first update, set this body to Transform
				if (firstUpdate) {
					Reset();
					firstUpdate = false;
				}
				
				delta = transform.position - lazyPoint;

				Vector3 currentSpeed = delta / deltaTime;
				Vector3 vecChgange = currentSpeed - direction;
				float dTime = vecChgange.magnitude / acceleration;
				dTime = Mathf.Min(dTime, deltaTime);
				direction += vecChgange.normalized * acceleration * dTime;

				lazyPoint += direction * deltaTime;

				// Match velocity
//				lazyPoint += delta * matchVelocity.Evaluate(direction.magnitude);
				lazyPoint += delta * matchVelocity;

				// Apply position offset to the effector
				foreach (EffectorLink effectorLink in effectorLinks) {
					solver.GetEffector(effectorLink.effector).positionOffset += (lazyPoint - transform.position) * effectorLink.weight * weight;
				}

				//lastPosition = transform.position;
			}
		}

		public Body[] bodies; // The array of Bodies

		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			// Update the Bodies
			foreach (Body body in bodies) body.Update(ik.solver, weight, deltaTime);
		}
	}
}
