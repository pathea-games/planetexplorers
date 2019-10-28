using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Steer3D
{
	public class Flees : SteeringBehaviour
	{
		public class FleeTarget
		{
			public bool active = true;
			public Vector3 target;
			public Transform targetTrans;
			public float weight = 1f;
			public float affectRadius = 5f;
			public float fleeRadius = 2f;
			public float forbiddenRadius = 0.2f;
		}

		public List<FleeTarget> targets = new List<FleeTarget> ();

		public override bool idle
		{
			get
			{
				if (!active)
					return true;

				if (targets == null)
					return true;

				foreach (FleeTarget tar in targets)
				{
					if (!tar.active)
						continue;
					Vector3 tarpos = tar.targetTrans != null ? tar.targetTrans.position : tar.target;
					if ((tarpos - position).sqrMagnitude <= tar.affectRadius * tar.affectRadius)
						return false;
				}
				return true;
			}
		}
		
		public override void Behave ()
		{
			if (targets == null)
				return;

			foreach (FleeTarget tar in targets)
			{
				if (tar.targetTrans != null)
					tar.target = tar.targetTrans.position;
				if (tar.active)
				{
					Vector3 to_target = tar.target - position;
					if (to_target.magnitude <= tar.affectRadius)
					{
						Vector3 desired_vel = -to_target.normalized;
						float strength = 0;
						if (tar.affectRadius > tar.fleeRadius)
							strength = Mathf.Clamp01(Mathf.InverseLerp(tar.affectRadius, tar.fleeRadius, to_target.magnitude));
						else
							strength = to_target.magnitude > tar.fleeRadius ? 0f : 1f;
						desired_vel *= strength;
						agent.AddDesiredVelocity(desired_vel, tar.weight, 0.75f);
					}
				}
			}
		}
	}
}
