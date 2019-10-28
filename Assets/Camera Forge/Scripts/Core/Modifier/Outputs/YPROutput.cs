using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Yaw Pitch Roll",0)]
	public class YPROutput : OutputNode
	{
		public YPROutput ()
		{
			Yaw = new Slot ("Yaw");
			Pitch = new Slot ("Pitch");
			Roll = new Slot ("Roll");
			Distance = new Slot ("Distance");
			Target = new Slot ("Target");
		}

		public override Pose Output ()
		{
			Yaw.Calculate();
			Pitch.Calculate();
			Roll.Calculate();
			Distance.Calculate();
			Target.Calculate();

			float yaw = 0f;
			float pitch = 0f;
			float roll = 0f;
			float dist = 0f;
			Vector3 tar = Vector3.zero;

			Pose pose = Pose.Default;
			if (modifier != null)
			{
				pose = modifier.Prev.value;
				yaw = pose.yaw;
				pitch = pose.pitch;
				roll = pose.roll;
				dist = 0f;
				tar = pose.position;
			}

			if (!Yaw.value.isNull)
				yaw = Yaw.value.value_f;
			if (!Pitch.value.isNull)
				pitch = Pitch.value.value_f;
			if (!Roll.value.isNull)
				roll = Roll.value.value_f;
			if (!Distance.value.isNull)
				dist = Distance.value.value_f;
			if (!Target.value.isNull)
				tar = Target.value.value_v;

			pose.eulerAngles = new Vector3 (-pitch, yaw, roll);
			Vector3 forward = pose.rotation * Vector3.forward;
			pose.position = tar - dist * forward;

			return pose;
		}

		public override Slot[] slots
		{
			get { return new Slot[5] {Yaw, Pitch, Roll, Distance, Target}; }
		}

		public Slot Yaw;
		public Slot Pitch;
		public Slot Roll;
		public Slot Distance;
		public Slot Target;
	}
}
