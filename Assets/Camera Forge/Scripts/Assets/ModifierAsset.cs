using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace CameraForge
{
	public class ModifierAsset : ScriptableObject
	{
		public const int CURRENT_VERSION = 10000;
		public Modifier modifier;

		[SerializeField] public int version;
		[SerializeField] public byte[] data;

		public void Save ()
		{
			version = CURRENT_VERSION;
			using (MemoryStream ms = new MemoryStream ())
			{
				BinaryWriter w = new BinaryWriter (ms);
				w.Write("ModifierAsset");
				w.Write(CURRENT_VERSION);
				w.Write(modifier.nodes.Count);
				for (int i = 0; i < modifier.nodes.Count; ++i)
				{
					Node node = modifier.nodes[i];
					w.Write(node.GetType().ToString());
				}
				for (int i = 0; i < modifier.nodes.Count; ++i)
				{
					Node node = modifier.nodes[i];
					node.Write(w);
				}
				if (modifier.output != null)
				{
					w.Write(modifier.output.GetType().ToString());
					modifier.output.Write(w);
				}
				else
				{
					w.Write("");
				}
				w.Write(modifier.graphCenter.x);
				w.Write(modifier.graphCenter.y);
				data = ms.ToArray();
				w.Close();
			}
		}

		public void Load (bool bCreate = true)
		{
			if (modifier == null || bCreate)
				modifier = new Modifier (this.name);
			using (MemoryStream ms = new MemoryStream (data))
			{
				BinaryReader r = new BinaryReader (ms);
				string header = r.ReadString();
				if (header == "ModifierAsset")
				{
					version = r.ReadInt32();
					if (version == 10000)
					{
						Assembly asm = Assembly.GetAssembly(typeof(Node));
						int node_count = r.ReadInt32();
						if (node_count < 0 || node_count > 4096)
						{
							throw new Exception("read node count error");
						}
						for (int i = 0; i < node_count; ++i)
						{
							string node_type = r.ReadString();
							Node node = asm.CreateInstance(node_type) as Node;
							modifier.AddNode(node);
						}
						for (int i = 0; i < node_count; ++i)
						{
							modifier.nodes[i].Read(r);
						}
						string output_type_name = r.ReadString();
						if (string.IsNullOrEmpty(output_type_name))
						{
							modifier.output = null;
						}
						else
						{
							modifier.output = asm.CreateInstance(output_type_name) as OutputNode;
							modifier.output.modifier = modifier;
							modifier.output.Read(r);
						}
						modifier.graphCenter.x = r.ReadSingle();
						modifier.graphCenter.y = r.ReadSingle();
					}
				}
				else
				{
					throw new Exception("Invalid modifier asset!");
				}
				r.Close();
			}
		}
	}
}
