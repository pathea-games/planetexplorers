using UnityEngine;
using System;
using System.Collections.Generic;

namespace CameraForge
{
	public class Controller : Modifier
	{
		public Controller () : base ()
		{
			// controller
			controller = this;
			final = new FinalPose ();
			final.controller = this;
			nodes = new List<Node> ();
			posenodes = new List<PoseNode> ();
		}
		public Controller (string name) : base (name)
		{
			// controller
			controller = this;
			final = new FinalPose ();
			final.controller = this;
			nodes = new List<Node> ();
			posenodes = new List<PoseNode> ();
		}

		public void AddPoseNode (PoseNode node)
		{
			node.controller = this;
			posenodes.Add(node);
		}
		
		public void DeletePoseNode (PoseNode node)
		{
			for (int i = posenodes.Count - 1; i >= 0; --i)
			{
				foreach (PoseSlot slot in posenodes[i].poseslots)
				{
					if (slot.input == node)
					{
						if (node is Modifier)
							slot.input = (node as Modifier).Prev.input;
						else
							slot.input = null;
					}
				}
				if (posenodes[i] == node)
				{
					posenodes[i] = null;
					posenodes.RemoveAt(i);
				}
			}
			if (final.Final.input == node)
			{
				if (node is Modifier)
					final.Final.input = (node as Modifier).Prev.input;
				else
					final.Final.input = null;
			}
		}
		
		public override void AddNode (Node node)
		{
			node.modifier = this;
			nodes.Add(node);
		}
		
		public override void DeleteNode (Node node)
		{
			for (int i = posenodes.Count - 1; i >= 0; --i)
			{
				foreach (Slot slot in posenodes[i].slots)
				{
					if (slot.input == node)
						slot.input = null;
				}
			}
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
		}
		
		public override void Tick (float deltaTime)
		{
			time += deltaTime;
			foreach (PoseNode node in posenodes)
			{
				if (node is Modifier)
					(node as Modifier).Tick(deltaTime);
			}
		}

		public override Pose Calculate ()
		{

			// TODO: Other Media

			return final.Calculate();
		}

		public CameraController executor;
		public FinalPose final;
		public List<PoseNode> posenodes;
	}
}
