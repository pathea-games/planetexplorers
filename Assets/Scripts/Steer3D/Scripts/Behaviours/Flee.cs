using UnityEngine;
using System.Collections;

namespace Steer3D
{
	public class Flee : SteeringBehaviour
	{
		public Vector3 target;
		public Transform targetTrans;
		public float weight = 1f;
		public float affectRadius = 5f;
		public float fleeRadius = 2f;
		public float forbiddenRadius = 0.2f;

		public override bool idle
		{
			get { return !active || (target - position).sqrMagnitude >= affectRadius * affectRadius; }
		}
		
		public override void Behave ()
		{
			if (targetTrans != null)
				target = targetTrans.position;
			if (!idle)
			{
				Vector3 to_target = target - position;
				Vector3 desired_vel = -to_target.normalized;
				float strength = 0;
				if (affectRadius > fleeRadius)
					strength = Mathf.Clamp01(Mathf.InverseLerp(affectRadius, fleeRadius, to_target.magnitude));
				else
					strength = to_target.magnitude > fleeRadius ? 0f : 1f;
				desired_vel *= strength;
				agent.AddDesiredVelocity(desired_vel, weight, 0.75f);
			}
		}
	}
}
