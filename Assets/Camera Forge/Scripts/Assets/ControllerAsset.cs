using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace CameraForge
{
	public class ControllerAsset : ScriptableObject
	{
		public const int CURRENT_VERSION = 10000;
		public Controller controller;
		
		[SerializeField] int version;
		[SerializeField] byte[] data;
		
		public void Save ()
		{
			version = CURRENT_VERSION;
			using (MemoryStream ms = new MemoryStream ())
			{
				BinaryWriter w = new BinaryWriter (ms);
				w.Write("ControllerAsset");
				w.Write(CURRENT_VERSION);
				w.Write(controller.nodes.Count);
				w.Write(controller.posenodes.Count);

				for (int i = 0; i < controller.nodes.Count; ++i)
				{
					Node node = controller.nodes[i];
					w.Write(node.GetType().ToString());
				}
				for (int i = 0; i < controller.nodes.Count; ++i)
				{
					Node node = controller.nodes[i];
					node.Write(w);
				}
				
				for (int i = 0; i < controller.posenodes.Count; ++i)
				{
					PoseNode node = controller.posenodes[i];
					w.Write(node.GetType().ToString());
				}
				for (int i = 0; i < controller.posenodes.Count; ++i)
				{
					PoseNode node = controller.posenodes[i];
					node.Write(w);
				}

				controller.final.Write(w);

				w.Write(controller.graphCenter.x);
				w.Write(controller.graphCenter.y);
				data = ms.ToArray();
				w.Close();
			}
		}
		
		public void Load ()
		{
			controller = new Controller (this.name);
			using (MemoryStream ms = new MemoryStream (data))
			{
				BinaryReader r = new BinaryReader (ms);
				string header = r.ReadString();
				if (header == "ControllerAsset")
				{
					version = r.ReadInt32();
					if (version == 10000)
					{
						Assembly asm = Assembly.GetAssembly(typeof(Node));
						int node_count = r.ReadInt32();
						int posenode_count = r.ReadInt32();
						if (node_count < 0 || node_count > 4096 ||
						    posenode_count < 0 || posenode_count > 4096)
						{
							throw new Exception("read node count error");
						}
						for (int i = 0; i < node_count; ++i)
						{
							string node_type = r.ReadString();
							Node node = asm.CreateInstance(node_type) as Node;
							controller.AddNode(node);
						}
						for (int i = 0; i < node_count; ++i)
						{
							controller.nodes[i].Read(r);
						}
						for (int i = 0; i < posenode_count; ++i)
						{
							string node_type = r.ReadString();
							PoseNode node = asm.CreateInstance(node_type) as PoseNode;
							controller.AddPoseNode(node);
						}
						for (int i = 0; i < posenode_count; ++i)
						{
							controller.posenodes[i].Read(r);
						}

						controller.final = new FinalPose ();
						controller.final.controller = controller;
						controller.final.Read(r);

						controller.graphCenter.x = r.ReadSingle();
						controller.graphCenter.y = r.ReadSingle();
					}
				}
				else
				{
					throw new Exception("Invalid controller asset!");
				}
				r.Close();
			}
		}
	}
}
