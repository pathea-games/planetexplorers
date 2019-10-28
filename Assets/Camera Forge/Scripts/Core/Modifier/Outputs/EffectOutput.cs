using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Effect Params",99)]
	public class EffectOutput : OutputNode
	{
		public EffectOutput ()
		{
			Sat = new Slot ("Saturate");
			Blur = new Slot ("Motion Blur");
		}

		public override Pose Output ()
		{
			Sat.Calculate();
			Blur.Calculate();

			Pose pose = Pose.Default;
			if (modifier != null)
			{
				pose = modifier.Prev.value;
			}

			if (!Sat.value.isNull)
				pose.saturate = Sat.value.value_f;
			
			if (!Blur.value.isNull)
				pose.motionBlur = Blur.value.value_f;
			
			return pose;
		}

		public override Slot[] slots
		{
			get { return new Slot[2] {Sat, Blur}; }
		}

		public Slot Sat;
		public Slot Blur;
	}
}
