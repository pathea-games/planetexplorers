using UnityEngine;
using System;

namespace CameraForge
{
	public class HistoryPose : PoseNode
	{
		public HistoryPose ()
		{
			Index = new Slot ("Index");
			Index.value = 0;
		}
		
		public override Pose Calculate ()
		{
			Index.Calculate();

			int index = Mathf.RoundToInt(Index.value.value_f);
			if (controller == null)
				return Pose.Default;
			if (controller.executor == null)
				return Pose.Default;

			return controller.executor.GetHistoryPose(index);
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[0] {}; }
		}
		
		public override Slot[] slots
		{
			get { return new Slot[1] {Index}; }
		}
		
		public Slot Index;
	}
}
