using UnityEngine;
using System;
using System.Collections.Generic;

namespace CameraForge
{
	public class Modifier : PoseNode
	{
		public Modifier ()
		{
			output = new YPROutput();
			nodes = new List<Node> ();
			Name = new Slot ("Name");
			Name.value = "Camera Mode";
			Prev = new PoseSlot ("Prev");
			time = 0;
			Life = new Slot ("Life");
			Life.value = 1e+20;
			Col = new Slot ("Color");
			Col.value = new Color (1.0f,0.8f,1.0f,1.0f);
			graphCenter = new Vector2 (-150, 100);
		}
		public Modifier (string name)
		{
			output = new YPROutput();
			nodes = new List<Node> ();
			Name = new Slot ("Name");
			Name.value = name;
			Prev = new PoseSlot ("Prev");
			time = 0;
			Life = new Slot ("Life");
			Life.value = 1e+20;
			Col = new Slot ("Color");
			Col.value = new Color (1.0f,0.8f,1.0f,1.0f);
			graphCenter = new Vector2 (-150, 100);
		}

		public override Pose Calculate ()
		{
			Name.Calculate();
			Life.Calculate();
			Col.Calculate();
			Prev.Calculate();

			// TODO: Other Media

			return output.Output();
		}

		public override Slot[] slots
		{
			get { return new Slot[3] {Name, Life, Col}; }
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}
		
		public virtual void AddNode (Node node)
		{
			node.modifier = this;
			nodes.Add(node);
		}

		public virtual void DeleteNode (Node node)
		{
			node.modifier = null;
			for (int i = nodes.Count - 1; i >= 0; --i)
			{
				foreach (Slot slot in nodes[i].slots)
				{
					if (slot.input == node)
						slot.input = null;
				}
				if (nodes[i] == node)
				{
					nodes[i] = null;
					nodes.RemoveAt(i);
				}
			}
			foreach (Slot slot in output.slots)
			{
				if (slot.input == node)
					slot.input = null;
			}
		}

		public virtual void Tick (float deltaTime)
		{
			time += deltaTime;
			if (time > Life.value.value_f)
			{
				if (controller != null)
					controller.DeletePoseNode(this);
				return;
			}
		}

		OutputNode _output;
		public OutputNode output
		{
			get { return _output; }
			set
			{
				_output = value;
				if (_output != null)
					_output.modifier = this;
			}
		}
		public List<Node> nodes;

		public Slot Name;
		public PoseSlot Prev;
		public float time;
		public Slot Life;
		public Slot Col;
		public Vector2 graphCenter;
	}
}
