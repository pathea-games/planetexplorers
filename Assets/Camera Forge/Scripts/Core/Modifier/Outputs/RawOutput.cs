using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Raw Position Rotation",1)]
	public class RawOutput : OutputNode
	{
		public RawOutput ()
		{
			Pos = new Slot ("Position");
			Rot = new Slot ("Rotation");
		}

		public override Pose Output ()
		{
			Pos.Calculate();
			Rot.Calculate();

			Pose pose = Pose.Default;
			if (modifier != null)
			{
				pose = modifier.Prev.value;
			}

			if (!Pos.value.isNull)
				pose.position = Pos.value.value_v;
			if (!Rot.value.isNull)
				pose.rotation = Rot.value.value_q;

			return pose;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {Pos, Rot}; }
		}

		public Slot Pos;
		public Slot Rot;
	}
}
