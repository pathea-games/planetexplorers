using UnityEngine;
using System.Collections;

namespace Steer3D
{
	public class Pursue : SteeringBehaviour
	{
		public Vector3 target;
		public Transform targetTrans;
		public float weight = 1f;
		public float slowingRadius = 5f;
		public float arriveRadius = 0.2f;

		const int history_count = 8;
		private Vector3[] history_target = new Vector3[history_count];
		private float[] history_time = new float[history_count];
		private int history_cursor = 0;

		public override bool idle
		{
			get { return !active || (target - position).sqrMagnitude < arriveRadius * arriveRadius; }
		}

		public override void Behave ()
		{
			Vector3 target_vel = Vector3.zero;
			bool vel_got = false;
			if (targetTrans != null)
			{
				target = targetTrans.position;
				SteerAgent s = targetTrans.GetComponent<SteerAgent>();
				if (s != null)
				{
					vel_got = true;
					target_vel = s.velocity * s.maxSpeed;
				}
			}

			if (!idle)
			{
				if (!vel_got)
				{
					for (int i = history_count - 1; i > 0; --i)
						history_target[i] = history_target[i - 1];
					for (int i = history_count - 1; i > 0; --i)
						history_time[i] = history_time[i - 1];
					history_target[0] = target;
					history_time[0] = Time.time;

					history_cursor++;
					if (history_cursor > history_count)
						history_cursor = history_count;

					int cnt = 0;
					Vector3 sum_vel = Vector3.zero;
					for (int i = 0; i < history_cursor - 1; ++i)
					{
						Vector3 delta_pos = history_target[i] - history_target[i + 1];
						float delta_t = history_time[i] - history_time[i + 1];
						delta_t = Mathf.Clamp(delta_t, 0.001f, 0.2f);
						if (delta_pos.magnitude > 10f)
							continue;
						Vector3 vel = delta_pos / delta_t;
						sum_vel += vel;
						cnt++;
					}

					if (cnt > 0)
						target_vel = sum_vel / cnt;
				}

				Vector3 to_target = target - position;
				Vector3 predict = target + target_vel * (to_target.magnitude / agent.maxSpeed);
				to_target = predict - position;

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
