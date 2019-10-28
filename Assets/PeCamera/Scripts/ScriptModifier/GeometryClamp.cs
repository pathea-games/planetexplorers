using UnityEngine;
using System.Collections.Generic;

namespace CameraForge
{
	public class GeometryClamp : ScriptModifier
	{
		public GeometryClamp ()
		{

		}

		float rise = 0;
		public override Pose Calculate ()
		{
			Col.Calculate();
			Prev.Calculate();

			if (controller != null && controller.executor != null)
			{
				bool isShootMode = GetBool("Shoot Mode");
				float dist = GetFloat("Dist");
				LayerMask layermask = controller.executor.GetVar("Obstacle LayerMask").value_i;
				float acr_coef = Mathf.Clamp(GetFloat("Activity Space Size") * 0.2f, 0.4f, 1f);

				Pose pose = Prev.value;
				Vector3 forward = pose.rotation * Vector3.forward;
				Vector3 right = pose.rotation * Vector3.right;
				Vector3 up = pose.rotation * Vector3.up;
				Vector3 target = pose.position + dist * forward;

				float aspect = controller.executor.camera.aspect;
				float ur = Mathf.Sqrt(1f + aspect * aspect) * Mathf.Tan(pose.fov * 0.5f * Mathf.Deg2Rad);

				pose.nearClip *= acr_coef;
				float NCR = Utils.EvaluateNearclipPlaneRadius(target, 0.05f, pose.nearClip * ur, layermask);
				controller.executor.SetFloat("NCR", NCR);

				pose.nearClip = NCR / ur;

				Vector3 eye = pose.position + pose.nearClip * forward;

				bool dont_clamp = false;

				for (float r = 0f; r < 3.01f; r += 1.5f)
				{
					int n = 8;
					if (r < 2f)
						n = 4;
					else if (r < 1f)
						n = 1;
					for (int i = 0; i < n; ++i)
					{
						float rad = i * (360 / n) * Mathf.Deg2Rad;
						Vector3 test_dir = r * (Mathf.Cos(rad) * right + Mathf.Sin(rad) * up);
						Vector3 test_point = test_dir + target;
						Ray ray = new Ray (target, test_dir.normalized);
						if (r == 0f || !Physics.Raycast(ray, r, layermask, QueryTriggerInteraction.Ignore))
						{
							if (!Physics.Linecast(test_point, eye, layermask, QueryTriggerInteraction.Ignore))
							{
								dont_clamp = true;
								goto SPHERETEST;
							}
						}
					}
				}

			SPHERETEST:

				{
					if (!dont_clamp || Physics.OverlapSphere(eye, NCR, layermask, QueryTriggerInteraction.Ignore).Length != 0)
					{
						float cast_dist = Vector3.Distance(target, eye);
						float curr_dist = 0;
						RaycastHit cast_hit;
						Vector3 cast_dir = (eye-target).normalized;
						while (true)
						{
							float next_dist = cast_dist - curr_dist;
							if (next_dist <= 0)
								break;

							Ray cast_ray = new Ray (target + cast_dir * curr_dist, cast_dir);
							if (Physics.SphereCast(cast_ray, NCR - 0.01f, out cast_hit, next_dist, layermask, QueryTriggerInteraction.Ignore))
							{
								cast_hit.distance += curr_dist;
								eye = target + cast_dir * cast_hit.distance;
								controller.executor.SetFloat("DistVelocity", 0);
								controller.executor.SetBool("Geometry Clampd", true);
								break;
							}
							else
							{
								break;
							}
						}

						pose.position = eye - pose.nearClip * forward;
					}
				}

				dist = Vector3.Distance(target, pose.position);

				// If dist is too short, rise to a new position

				float max_riseup = NCR;
				RaycastHit rch;
				Vector3 rise_dir = Vector3.up + forward * 0.3f;
				if (Physics.SphereCast(new Ray(eye, rise_dir.normalized), NCR - 0.02f, out rch, NCR * 10, layermask, QueryTriggerInteraction.Ignore))
					max_riseup = rch.distance * 0.95f;

				float riseWanted = 0;
				if (dist < pose.nearClip * 2f)
				{
					float u = dist / (pose.nearClip * 2f);
					riseWanted = Mathf.Sqrt(1.0001f - u*u) * pose.nearClip * 2f;
				}
				if (riseWanted > max_riseup)
					riseWanted = max_riseup;

				if (isShootMode)
					riseWanted = 0;

				rise = Mathf.Lerp(rise, riseWanted, 0.1f);
				pose.position += rise_dir * rise;
				controller.executor.SetFloat("Dist", dist);
				return pose;
			}

			return Pose.Default;
		}

		bool GetBool(string name)
		{
			return controller.executor.GetVar(name).value_b;
		}
		
		float GetFloat(string name)
		{
			return controller.executor.GetVar(name).value_f;
		}
		
		Vector3 GetPosition(string name)
		{
			Transform t = CameraController.GetTransform(name);
			if (t == null)
				return Vector3.zero;
			return t.position;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {Name, Col}; }
		}

		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}
	}
}