using UnityEngine;
using System;

namespace CameraForge
{
	
	public class PoseBlend : PoseNode
	{
		public PoseBlend ()
		{
			Name = new Slot ("Name");
			Count = new Slot ("Count");
			Index = new Slot ("Index");
			Poses = new PoseSlot[0] ;

			Name.value = "Pose Blend";
			UpdateCount(2);
			Index.value = 0;
		}

		public void UpdateCount (int count)
		{
			count = Mathf.Clamp(count, 1, 16);
			Count.input = null;
			Count.value = count;
			PoseSlot[] new_poses = new PoseSlot[count] ;
			int old_count = Poses.Length;
			int min_count = Mathf.Min(count, old_count);
			Array.Copy(Poses, new_poses, min_count);
			for (int i = 0; i < count; ++i)
			{
				if (new_poses[i] == null)
					new_poses[i] = new PoseSlot ("P[" + i.ToString() + "]");
			}
			Poses = new_poses;
			Weights = new float[count] ;
			Weights[currentIndex] = 1f;
		}

		public int currentIndex
		{
			get { return Mathf.RoundToInt(Index.value.value_f) % Poses.Length; }
		}
		
		public override Pose Calculate ()
		{
			Fade();

			Pose blended_pose = Pose.Zero;
			double px = 0, py = 0, pz = 0;
			float w = 0;
			float lockcur = 0;

			for (int i = 0; i < Poses.Length; ++i)
			{
				if (Weights[i] < 0.000001f)
					continue;


				Poses[i].Calculate();

				px += Poses[i].value.position.x * Weights[i];
				py += Poses[i].value.position.y * Weights[i];
				pz += Poses[i].value.position.z * Weights[i];
				blended_pose.rotation = Quaternion.Slerp(Poses[i].value.rotation, blended_pose.rotation, w / (w + Weights[i]));
				blended_pose.fov += Poses[i].value.fov * Weights[i];
				blended_pose.nearClip += Poses[i].value.nearClip * Weights[i];
				lockcur += (Poses[i].value.lockCursor ? 1f : 0f) * Weights[i];
				blended_pose.cursorPos += Poses[i].value.cursorPos * Weights[i];
				blended_pose.saturate += Poses[i].value.saturate * Weights[i];
				blended_pose.motionBlur += Poses[i].value.motionBlur * Weights[i];
				w += Weights[i];
			}
			px /= w;
			py /= w;
			pz /= w;
			blended_pose.position = new Vector3 ((float)px, (float)py, (float)pz);
			blended_pose.fov /= w;
			blended_pose.nearClip /= w;
			lockcur /= w;
			blended_pose.lockCursor = lockcur > 0.99f;
			blended_pose.cursorPos /= w;
			blended_pose.saturate /= w;
			blended_pose.motionBlur /= w;

			return blended_pose;
		}
		
		public override PoseSlot[] poseslots
		{
			get
			{
				PoseSlot[] ps = new PoseSlot[Poses.Length] ;
				Array.Copy(Poses, ps, Poses.Length);
				return ps;
			}
		}
		
		public override Slot[] slots
		{
			get { return new Slot[3] {Name, Count, Index}; }
		}

		internal override void Read (System.IO.BinaryReader r)
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

			UpdateCount(Mathf.RoundToInt(Count.value.value_f));
			
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
		}
		
		public Slot Name;
		public Slot Count;
		public Slot Index;
		public PoseSlot[] Poses;

		private float[] Weights;
		public float GetWeight (int index) { return Weights[index]; }
		private float CrossFadeSpeed = 1;
		private void Fade ()
		{
			for (int i = 0; i < Weights.Length; ++i)
			{
				if (i == currentIndex)
				{
					Weights[i] = Mathf.Lerp(Weights[i], 1, CrossFadeSpeed);
					if (Weights[i] > 0.999999f)
						Weights[i] = 1f;
				}
				else
				{
					Weights[i] = Mathf.Lerp(Weights[i], 0, CrossFadeSpeed);
					if (Weights[i] < 0.000001f)
						Weights[i] = 0f;
				}
			}
		}
		public void CrossFade (int index, float speed = 0.3f)
		{
			Index.input = null;
			Index.value = index;
			CrossFadeSpeed = speed;
			if (speed < 0.001f)
				speed = 0.001f;
			if (speed > 1f)
				speed = 1f;
		}
	}
}
