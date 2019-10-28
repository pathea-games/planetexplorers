using UnityEngine;
using System;
using System.IO;

namespace CameraForge
{
	public abstract class PoseNode
	{
		public Controller controller;
		public Vector2 editorPos;
		public byte[] data = new byte[0] ;

		public abstract Pose Calculate ();
		public abstract PoseSlot[] poseslots { get; }
		public abstract Slot[] slots { get; }

		internal virtual void Write (BinaryWriter w)
		{
			if (controller == null)
			{
				Debug.LogError("no controller!");
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
					for (int k = 0; k < controller.nodes.Count; ++k)
					{
						if (slot.input == controller.nodes[k])
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

			PoseSlot[] _poseslots = this.poseslots;
			w.Write(_poseslots.Length);
			for (int s = 0; s < _poseslots.Length; ++s)
			{
				PoseSlot poseslot = _poseslots[s];
				w.Write(poseslot.name);
				if (poseslot.input != null)
				{
					int index = -1;
					for (int k = 0; k < controller.posenodes.Count; ++k)
					{
						if (poseslot.input == controller.posenodes[k])
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
				}
			}

			data = new byte[0] ;

			if (this is Modifier)
			{
				ModifierAsset ma = ScriptableObject.CreateInstance<ModifierAsset>();
				ma.modifier = this as Modifier;
				ma.Save();
				data = new byte[ma.data.Length] ;
				Array.Copy(ma.data, data, ma.data.Length);
				ModifierAsset.DestroyImmediate(ma);
			}

			w.Write((int)(data.Length));
			w.Write(data, 0, (int)(data.Length));
		}
		
		internal virtual void Read (BinaryReader r)
		{
			if (controller == null)
			{
				Debug.LogError("no modifier!");
				return;
			}
			
			editorPos.x = r.ReadSingle();
			editorPos.y = r.ReadSingle();
			
			Slot[] _slots = this.slots;
			int scnt = r.ReadInt32();
			for (int s = 0; s < scnt; ++s)
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
						if (index >= 0 && index < controller.nodes.Count)
							slot.input = controller.nodes[index];
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

			PoseSlot[] _poseslots = this.poseslots;
			int sscnt = r.ReadInt32();
			for (int s = 0; s < sscnt; ++s)
			{
				string _name = r.ReadString();
				PoseSlot poseslot = null;
				for (int i = 0; i < _poseslots.Length; ++i)
				{
					if (_poseslots[i].name == _name)
					{
						poseslot = _poseslots[i];
						break;
					}
				}
				
				int index = r.ReadInt32();
				if (poseslot != null)
				{
					if (index != -1)
					{
						poseslot.input = null;
						if (index >= 0 && index < controller.posenodes.Count)
							poseslot.input = controller.posenodes[index];
						if (poseslot.input == this)
							poseslot.input = null;
					}
					else
					{
						poseslot.input = null;
						poseslot.value = Pose.Default;
					}
				}
			}

			int dl = r.ReadInt32();
			data = r.ReadBytes(dl);
			
			if (this is Modifier)
			{
				ModifierAsset ma = ScriptableObject.CreateInstance<ModifierAsset>();
				ma.modifier = this as Modifier;
				ma.data = new byte[dl] ;
				Array.Copy(data, ma.data, dl);
				ma.Load(false);
				ModifierAsset.DestroyImmediate(ma);
			}
		}
	}
}
