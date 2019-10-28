using UnityEngine;
using System;

namespace CameraForge
{
	public abstract class OutputNode : Node
	{
		public override Var Calculate ()
		{
			return Var.Null;
		}
		public abstract Pose Output ();
	}
}
