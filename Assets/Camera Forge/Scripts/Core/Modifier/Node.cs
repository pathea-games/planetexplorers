using UnityEngine;
using System.IO;

namespace CameraForge
{
	public abstract class Node
	{
		public Modifier modifier;
		public Vector2 editorPos;
		public abstract Var Calculate ();
		public abstract Slot[] slots { get; }

		public bool IsDependencyOf (Node node)
		{
			if (node == null)
				return false;
			if (this == node)
				return true;
			foreach (Slot slot in node.slots)
			{
				if (IsDependencyOf(slot.input))
					return true;
			}
			return false;
		}

		internal virtual void Write (BinaryWriter w)
		{
			if (modifier == null)
			{
				Debug.LogError("no modifier!");
				return;
			}

			w.Write(editorPos.x);
			w.Write(editorPos.y);

			Slot[] _slots = this.slots;
			w.Write(_slots.Length);
			for (int s = 0; s < _slots.Length; ++s)
			{
				Slot slot = _slots[s];
				w.Write(slot.name);
				if (slot.input != null)
				{
					int index = -1;
					for (int k = 0; k < modifier.nodes.Count; ++k)
					{
						if (slot.input == modifier.nodes[k])
						{
							index = k;
							break;
						}
					}
					w.Write(index);
				}
				else
				{
					w.Write((int)-1);
					slot.value.Write(w);
				}
			}
		}
		
		internal virtual void Read (BinaryReader r)
		{
			if (modifier == null)
			{
				Debug.LogError("no modifier!");
				return;
			}
			
			editorPos.x = r.ReadSingle();
			editorPos.y = r.ReadSingle();

			Slot[] _slots = this.slots;
			int cnt = r.ReadInt32();
			for (int s = 0; s < cnt; ++s)
			{
				string _name = r.ReadString();
				Slot slot = null;
				for (int i = 0; i < _slots.Length; ++i)
				{
					if (_slots[i].name == _name)
					{
						slot = _slots[i];
						break;
					}
				}

				if (slot != null)
				{
					int index = r.ReadInt32();
					if (index != -1)
					{
						slot.input = null;
						if (index >= 0 && index < modifier.nodes.Count)
							slot.input = modifier.nodes[index];
						if (slot.input == this)
							slot.input = null;
					}
					else
					{
						slot.input = null;
						slot.value.Read(r);
					}
				}
				else
				{
					int index = r.ReadInt32();
					if (index == -1)
					{
						Var v = Var.Null;
						v.Read(r);
					}
				}
			}
		}
	}
}
