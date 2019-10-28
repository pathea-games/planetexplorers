using UnityEngine;
using System.Collections;

namespace CameraForge
{
	public abstract class ScriptModifier : PoseNode
	{
		public ScriptModifier ()
		{
			Name = new Slot ("Name");
			Name.value = "";
			Col = new Slot ("Color");
			Col.value = new Color (0.7f,0.8f,1.0f,1.0f);
			Prev = new PoseSlot ("Prev");
		}

		public Slot Name;
		public Slot Col;

		public PoseSlot Prev;
	}
}