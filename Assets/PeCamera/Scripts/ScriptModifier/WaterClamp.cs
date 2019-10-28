using UnityEngine;
using System.Collections.Generic;

namespace CameraForge
{
	public class WaterClamp : ScriptModifier
	{
		public WaterClamp ()
		{
			
		}
		
		//float rise = 0;
		public override Pose Calculate ()
		{
			Col.Calculate();
			Prev.Calculate();
			
			if (controller != null && controller.executor != null)
			{
				Pose pose = Prev.value;

				float maxy = pose.nearClip / Mathf.Cos(pose.fov * 0.5f * Mathf.Deg2Rad) + 0.01f;
				RaycastHit rch;
				if (Physics.Raycast(new Ray(pose.position, Vector3.down), out rch, maxy, 1 << Pathea.Layer.Water))
				{
					float rise = maxy - rch.distance;
					pose.position += Vector3.up * rise;
				}
				if (Physics.Raycast(new Ray(pose.position + Vector3.up * maxy, Vector3.down), out rch, maxy, 1 << Pathea.Layer.Water))
				{
					float rise = -rch.distance;
					pose.position += Vector3.up * rise;
				}

				return pose;
			}
			
			return Pose.Default;
		}
		
		bool GetBool(string name)
		{
			return controller.executor.GetVar(name).value_b;
		}
		
		float GetFloat(string name)
		{
			return controller.executor.GetVar(name).value_f;
		}
		
		Vector3 GetPosition(string name)
		{
			Transform t = CameraController.GetTransform(name);
			if (t == null)
				return Vector3.zero;
			return t.position;
		}
		
		public override Slot[] slots
		{
			get { return new Slot[2] {Name, Col}; }
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}
	}
}