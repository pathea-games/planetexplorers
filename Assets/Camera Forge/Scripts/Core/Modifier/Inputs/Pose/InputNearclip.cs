using UnityEngine;
using System;

namespace CameraForge
{
	[Menu("Input/Pose/Near Clip",4)]
	public class InputNearclip : InputNode
	{
		public InputNearclip ()
		{

		}

		public override Var Calculate ()
		{
			if (modifier == null)
				return Vector3.zero;
			return modifier.Prev.value.nearClip;
		}

		public override Slot[] slots
		{
			get { return new Slot[0]; }
		}
	}
}
