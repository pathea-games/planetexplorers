using UnityEngine;
using System;

namespace CameraForge
{
	
	public class GetUserPose : MediaPoseNode
	{
		public GetUserPose ()
		{
			V = new Slot ("Var");
		}
		
		public override Pose Calculate ()
		{
			V.Calculate();

			if (V.value.isNull)
				return Pose.Default;
			
			string varname = V.value.value_str.Trim();
			
			if (string.IsNullOrEmpty(varname))
				return Pose.Default;

			if (controller == null)
				return Pose.Default;
			if (controller.executor == null)
				return Pose.Default;

			return controller.executor.GetPose(varname);
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[0] {}; }
		}
		
		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}
		
		public Slot V;
	}
}
