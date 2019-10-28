using UnityEngine;
using System.Collections;

namespace CameraForge
{
	public class ShakeEffect : ScriptModifier
	{
		public ShakeEffect ()
		{
			Enabled = new Slot ("Enabled");
			Enabled.value = false;

			YPRScale = new Slot ("YPRScale");
			YPRScale.value = Vector3.zero;
			YPROmega = new Slot ("YPROmega");
			YPROmega.value = Vector3.one;
			YPRPhi = new Slot ("YPRPhi");
			YPRPhi.value = Vector3.zero;

			OffsetScale = new Slot ("OffsetScale");
			OffsetScale.value = Vector3.zero;
			OffsetOmega = new Slot ("OffsetOmega");
			OffsetOmega.value = Vector3.one;
			OffsetPhi = new Slot ("OffsetPhi");
			OffsetPhi.value = Vector3.zero;

			Tick = new Slot("Time");
			Duration = new Slot ("Duration");
			Falloff = new Slot ("Falloff");
		}
		
		public override Pose Calculate ()
		{
			Col.Calculate();
			Prev.Calculate();
			Enabled.Calculate();
			if (!Enabled.value.value_b)
				return Prev.value;
			YPRScale.Calculate();
			YPROmega.Calculate();
			YPRPhi.Calculate();
			OffsetScale.Calculate();
			OffsetOmega.Calculate();
			OffsetPhi.Calculate();
			Tick.Calculate();
			Duration.Calculate();
			Falloff.Calculate();

			if (controller != null && controller.executor != null)
			{
				Pose pose = Prev.value;

				float t = Tick.value.value_f;
				float d = Mathf.Max(0.001f, Duration.value.value_f);
				float f = Mathf.Max(0.001f, Falloff.value.value_f);
				if (t < d && t >= 0)
				{
					float scale = Mathf.Pow(1f - t / d, f);
					pose.yaw += Mathf.Sin((YPROmega.value.value_v.x * t + YPRPhi.value.value_v.x) * 2.0f * Mathf.PI) * YPRScale.value.value_v.x * scale;
					pose.pitch += Mathf.Sin((YPROmega.value.value_v.y * t + YPRPhi.value.value_v.y) * 2.0f * Mathf.PI) * YPRScale.value.value_v.y * scale;
					pose.roll += Mathf.Sin((YPROmega.value.value_v.z * t + YPRPhi.value.value_v.z) * 2.0f * Mathf.PI) * YPRScale.value.value_v.z * scale;

					Vector3 right = pose.rotation * Vector3.right;
					Vector3 up = pose.rotation * Vector3.up;
					Vector3 forward = pose.rotation * Vector3.forward;

					Vector3 ofs = Vector3.zero;
					ofs += right * Mathf.Sin((OffsetOmega.value.value_v.x * t + OffsetPhi.value.value_v.x) * 2.0f * Mathf.PI) * OffsetScale.value.value_v.x * scale;
					ofs += up * Mathf.Sin((OffsetOmega.value.value_v.y * t + OffsetPhi.value.value_v.y) * 2.0f * Mathf.PI) * OffsetScale.value.value_v.y * scale;
					ofs += forward * Mathf.Sin((OffsetOmega.value.value_v.z * t + OffsetPhi.value.value_v.z) * 2.0f * Mathf.PI) * OffsetScale.value.value_v.z * scale;

					pose.position += ofs;

					return pose;
				}
				else
				{
					return pose;
				}
			}
			
			return Pose.Default;
		}

		public override Slot[] slots
		{
			get { return new Slot[12] {Name, Col, Enabled, YPRScale, YPROmega, YPRPhi, OffsetScale, OffsetOmega, OffsetPhi, Tick, Duration, Falloff}; }
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}

		Slot Enabled;

		Slot YPRScale;
		Slot YPROmega;
		Slot YPRPhi;

		Slot OffsetScale;
		Slot OffsetOmega;
		Slot OffsetPhi;

		Slot Tick;
		Slot Duration;
		Slot Falloff;
	}
}
