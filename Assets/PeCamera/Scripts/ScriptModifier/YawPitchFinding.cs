using UnityEngine;
using System.Collections.Generic;

namespace CameraForge
{
	public class YawPitchFinding : ScriptModifier
	{
		public YawPitchFinding ()
		{
			Enabled = new Slot ("Enabled");
			Enabled.value = true;
		}

		Vector2 vyp = Vector2.zero;
		float hideTime = 0;
		public override Pose Calculate ()
		{
			Col.Calculate();
			Prev.Calculate();
			Enabled.Calculate();

			if (!Enabled.value.value_b)
				return Prev.value;
			
			if (controller != null && controller.executor != null)
			{
				float noControlTime = controller.executor.GetVar("No Rotate Time").value_f;
				
				if (noControlTime < 0.3f)
					return Prev.value;

				float dist = controller.executor.GetVar("Dist").value_f;
				//float distWanted = controller.executor.GetVar("DistWanted").value_f;
				LayerMask layermask = controller.executor.GetVar("Obstacle LayerMask").value_i;

				Pose pose = Prev.value;
				Vector3 forward = pose.rotation * Vector3.forward;
				//Vector3 right = pose.rotation * Vector3.right;
				//Vector3 up = pose.rotation * Vector3.up;
				Vector3 target = pose.position + dist * forward;

				Vector3 backward = pose.position - target;
				float curr_yaw = controller.executor.GetVar("Yaw").value_f;
				float curr_pitch = controller.executor.GetVar("Pitch").value_f;
				float yaw = curr_yaw;
				float pitch = curr_pitch;
				float NCR = controller.executor.GetVar("NCR").value_f;
				if (NCR < 0.01f)
					NCR = 0.01f;
				float cast_dist = backward.magnitude - NCR;
				if (cast_dist < 0)
					return pose;
				if (Physics.SphereCast(new Ray(target, backward.normalized), NCR - 0.01f, cast_dist, layermask, QueryTriggerInteraction.Ignore))
				{
					hideTime += 0.02f;
					if (hideTime > 0.2f)
					{

						for (float d = 5f; d < 90.01f; d += 5f)
						{
							for (float theta = 0; theta < 180.01f; theta += 45f)
							{
								yaw = d * Mathf.Cos(theta * Mathf.Deg2Rad) + curr_yaw;
								pitch = curr_pitch - 0.8f * d * Mathf.Sin(theta * Mathf.Deg2Rad);
								if (pitch < -70)
									continue;
								Vector3 dir = Quaternion.Euler(-pitch, yaw, 0) * Vector3.back;
								if (!Physics.SphereCast(new Ray(target, dir), NCR - 0.01f, cast_dist, layermask, QueryTriggerInteraction.Ignore))
								{
									Vector2 nxtyp = Vector2.SmoothDamp(new Vector2(curr_yaw, curr_pitch), new Vector2(yaw, pitch), ref vyp, 0.25f);
									controller.executor.SetFloat("YawWanted", nxtyp.x);
									controller.executor.SetFloat("PitchWanted", nxtyp.y);
									return pose;
								}
							}
						}
					}
				}
				else
				{
					hideTime = 0;
				}

				return pose;
			}
			
			return Pose.Default;
		}

		public override Slot[] slots
		{
			get { return new Slot[3] {Name, Col, Enabled}; }
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}

		Slot Enabled;
	}
}