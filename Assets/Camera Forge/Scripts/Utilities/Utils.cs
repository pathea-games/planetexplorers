using UnityEngine;
using System.Collections;

namespace CameraForge
{
	public static class Utils
	{
		static System.Random random = new System.Random ((int)System.DateTime.Now.Ticks);
		public static string NewGUID ()
		{
			System.DateTime dt = System.DateTime.Now;
			long tick = dt.Ticks;

			byte[] b = new byte[10];
			random.NextBytes(b);

			ushort s3, s4, s5;
			s3 = (ushort)(tick);
			s4 = (ushort)((long)tick >> 16);
			s5 = (ushort)((long)tick >> 32);

			return b[0].ToString("X").PadLeft(2, '0')
				 + b[1].ToString("X").PadLeft(2, '0')
				 + b[2].ToString("X").PadLeft(2, '0')
				 + b[3].ToString("X").PadLeft(2, '0') + "-"
				 + s5.ToString("X").PadLeft(4, '0') + "-"
				 + s4.ToString("X").PadLeft(4, '0') + "-"
				 + s3.ToString("X").PadLeft(4, '0') + "-"
				 + b[4].ToString("X").PadLeft(2, '0')
				 + b[5].ToString("X").PadLeft(2, '0')
				 + b[6].ToString("X").PadLeft(2, '0')
				 + b[7].ToString("X").PadLeft(2, '0')
				 + b[8].ToString("X").PadLeft(2, '0')
				 + b[9].ToString("X").PadLeft(2, '0');
		}

		public static float NormalizeDEG (float angle)
		{
			return Mathf.Repeat(angle + 360f, 720f) - 360f;
		}

		public static float EvaluateActivitySpaceSize (Vector3 character, float min, float max, Vector3 weightDirection, float accuracy, LayerMask layerMask)
		{
			float step = 360f / Mathf.Ceil(Mathf.Clamp(accuracy, 0.5f, 5f) * 4);
			RaycastHit rch;
			double w = 0;
			double sumdist = 0;
			float min_dist = max;
			for (float pitch = -90; pitch < 90.01f; pitch += step)
			{
				if (pitch < -35)
					continue;
				float yawbegin = Mathf.Sin(pitch*1000) * 180f;
				float step_yaw = 360f / Mathf.Ceil(Mathf.Clamp(accuracy * Mathf.Cos(pitch * Mathf.Deg2Rad), 0.01f, 5f) * 4);
				for (float yaw = yawbegin; yaw < yawbegin + 359.99f; yaw += step_yaw)
				{
					Vector3 dir = new Vector3(Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Cos(yaw * Mathf.Deg2Rad), 
					                          Mathf.Sin(pitch * Mathf.Deg2Rad),
					                          Mathf.Cos(pitch * Mathf.Deg2Rad) * Mathf.Sin(yaw * Mathf.Deg2Rad));
					bool cast = Physics.Raycast(new Ray(character, dir), out rch, max, layerMask, QueryTriggerInteraction.Ignore);
					float dist = cast ? rch.distance : max;
					float dot_weight = Mathf.Pow(Mathf.Clamp(Vector3.Dot(dir, weightDirection), -1f, 1f) + 1f, 1.5f);
					float dist_weight = max / (dist + max * 0.3f);

					if (dist < min_dist)
						min_dist = dist;

					w += dot_weight * dist_weight;
					sumdist += dot_weight * dist_weight * dist;
				}
			}
			float ave_dist = (float)(sumdist / w);
			return Mathf.Max(min_dist, ave_dist * 0.5f);
		}

		public static float EvaluateNearclipPlaneRadius (Vector3 character, float min, float max, LayerMask layerMask)
		{
			if (Physics.OverlapSphere(character, min, layerMask, QueryTriggerInteraction.Ignore).Length > 0)
				return min;
			if (Physics.OverlapSphere(character, max, layerMask, QueryTriggerInteraction.Ignore).Length == 0)
				return max;
			while (max - min > 0.005f)
			{
				float mid = (min + max) * 0.5f;
				if (Physics.OverlapSphere(character, mid, layerMask, QueryTriggerInteraction.Ignore).Length == 0)
					min = mid;
				else
					max = mid;
			}

			return min;
		}
	}
}
