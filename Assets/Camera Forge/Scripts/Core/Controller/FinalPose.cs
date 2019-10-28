using UnityEngine;
using System;

namespace CameraForge
{
	
	public class FinalPose : PoseNode
	{
		public FinalPose ()
		{
			Final = new PoseSlot ("Final");
		}
		
		public override Pose Calculate ()
		{
			Final.Calculate();
			return Final.value;
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Final}; }
		}
		
		public override Slot[] slots
		{
			get { return new Slot[0] {}; }
		}
		
		public PoseSlot Final;
	}
}
