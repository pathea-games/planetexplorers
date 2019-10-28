using UnityEngine;
using System.Collections;

namespace Steer3D
{
	public class Seek : SteeringBehaviour
	{
		public Vector3 target;
		public Transform targetTrans;
		public float weight = 1f;
		public float slowingRadius = 5f;
		public float arriveRadius = 0.2f;

		public override bool idle
		{
			get { return !active || (target - position).sqrMagnitude < arriveRadius * arriveRadius; }
		}
		
		public override void Behave ()
		{
			if (targetTrans != null)
				target = targetTrans.position;
			if (!idle)
			{
				Vector3 to_target = target - position;
				Vector3 desired_vel = to_target.normalized;
				float strength = 1;
				if (slowingRadius > arriveRadius)
					strength = Mathf.Clamp01(Mathf.InverseLerp(arriveRadius, slowingRadius, to_target.magnitude));
				else
					strength = to_target.magnitude > arriveRadius ? 1f : 0f;
				desired_vel *= strength;
				agent.AddDesiredVelocity(desired_vel, weight, 0.75f);
			}
		}
	}
}
