using UnityEngine;
using System;

namespace CameraForge
{
	
	public class SetUserPose : MediaPoseNode
	{
		public SetUserPose ()
		{
			V = new Slot ("Var");
			P = new PoseSlot ("Pose");
		}
		
		public override Pose Calculate ()
		{
			V.Calculate();
			P.Calculate();
			
			if (V.value.isNull)
				return P.value;
			
			string varname = V.value.value_str.Trim();
			
			if (string.IsNullOrEmpty(varname))
				return P.value;

			if (controller == null)
				return P.value;
			if (controller.executor == null)
				return P.value;

			controller.executor.SetPose(varname, P.value);

			return P.value;
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {P}; }
		}
		
		public override Slot[] slots
		{
			get { return new Slot[1] {V}; }
		}
		
		public Slot V;
		public PoseSlot P;
	}
}
