using UnityEngine;
using System.Collections;

namespace CameraForge
{
	public class ThirdPersonShoot : ScriptModifier
	{
		public ThirdPersonShoot ()
		{
			Sens = new Slot ("Sensitivity");
			Damp = new Slot ("Damp Rate");
			LockCursor = new Slot ("Lock Cursor");
			LockCursor.value = true;
			PitchMax = new Slot ("Pitch Max");
			PitchMax.value = 55f;
			PitchMin = new Slot ("Pitch Min");
			PitchMin.value = -70f;
			Offset = new Slot ("Offset");
			Offset.value = Vector3.zero;
			OffsetUp = new Slot ("Offset Up");
			OffsetUp.value = Vector3.zero;
			OffsetDown = new Slot ("Offset Down");
			OffsetDown.value = Vector3.zero;
		}

		public override Pose Calculate ()
		{
			Col.Calculate();
			Sens.Calculate();
			Damp.Calculate();
			LockCursor.Calculate();
			PitchMax.Calculate();
			PitchMin.Calculate();
			Offset.Calculate();
			OffsetUp.Calculate();
			OffsetDown.Calculate();
			Prev.Calculate();

			if (controller != null && controller.executor != null)
			{
				float yaw = GetFloat("Yaw");
				float pitch = GetFloat("Pitch");
				float yawWanted = GetFloat("YawWanted");
				float pitchWanted = GetFloat("PitchWanted");
				Vector3 defaultPos = CameraController.GetGlobalVar("Default Anchor").value_v;
				Transform anchor = CameraController.GetTransform("Anchor");
				Transform character = CameraController.GetTransform("Character");
				Transform neck_m = CameraController.GetTransform("Bone Neck M");
				Transform neck_r = CameraController.GetTransform("Bone Neck R");
				bool isRagdoll = GetBool("Is Ragdoll");
				float sens = Sens.value.value_f;
				bool rotButton = InputModule.Axis("Mouse Right Button").value_b;
				float mouseX = InputModule.Axis("Mouse X").value_f;
				float mouseY = InputModule.Axis("Mouse Y").value_f;
				float JoystickX = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime : 0f;
				float JoystickY = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime : 0f;
				bool inverseX = GetBool("Inverse X");
				bool inverseY = GetBool("Inverse Y");
				float pitchMax = PitchMax.value.value_f;
				float pitchMin = PitchMin.value.value_f;
				float damp = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
				//float dt = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);

				// Calc target
				Vector3 target = Vector3.zero;
				if (character != null)
				{
					if (neck_m != null && neck_r != null)
					{
						target = Vector3.Lerp(character.position, (isRagdoll ? neck_r : neck_m).position, 0.4f);
					}
					else
					{
						target = character.position;
					}
				}
				else if (anchor != null)
				{
					target = anchor.position;
				}
				else
				{
					target = defaultPos;
				}

				// rotate / zoom
				float dx = 0, dy = 0;
				if (Prev.value.lockCursor || rotButton)
				{
					dx = Mathf.Clamp(mouseX * sens * (inverseX ? -1f : 1f), -200, 200) * Time.timeScale;
					dy = Mathf.Clamp(mouseY * sens * (inverseY ? -1f : 1f), -200, 200) * Time.timeScale;
				}

				//float mdx = 0, mdy = 0;
				float jdx = 0, jdy = 0;

				//mdx = dx;
				//mdy = dy;

				jdx = JoystickX * sens * (inverseX ? -1f : 1f) * Time.timeScale;
				jdy = JoystickY * sens * (inverseY ? -1f : 1f) * Time.timeScale;

				dx += jdx;
				dy += jdy;

				dx = Mathf.Clamp(dx, -6 * sens, 6 * sens);
				dy = Mathf.Clamp(dy, -3 * sens, 3 * sens);

				yawWanted += dx;
				pitchWanted += dy;

				yawWanted = Mathf.Repeat(yawWanted, 360f);
				pitchWanted = Mathf.Clamp(pitchWanted, pitchMin, pitchMax);

				yaw = Mathf.LerpAngle(yaw, yawWanted, damp);
				pitch = Mathf.LerpAngle(pitch, pitchWanted, damp);

				Pose pose = Prev.value;
				pose.eulerAngles = new Vector3 (-pitch, yaw, 0);
				Vector3 forward = pose.rotation * Vector3.forward;
				Vector3 right = pose.rotation * Vector3.right;
				Vector3 up = pose.rotation * Vector3.up;
				Vector3 lofs = Vector3.zero;
				if (pitch < 0)
				{
					lofs = Vector3.Slerp(Offset.value.value_v, OffsetDown.value.value_v, pitch / pitchMin);
				}
				else
				{
					lofs = Vector3.Slerp(Offset.value.value_v, OffsetUp.value.value_v, pitch / pitchMax);
				}
				Vector3 ofs = lofs.x * right + lofs.y * up + lofs.z * forward;

				float aspect = controller.executor.camera.aspect;
				float ur = Mathf.Sqrt(1f + aspect * aspect) * Mathf.Tan(pose.fov * 0.5f * Mathf.Deg2Rad);
				LayerMask layermask = controller.executor.GetVar("Obstacle LayerMask").value_i;
				float NCR = Utils.EvaluateNearclipPlaneRadius(target, 0.05f, pose.nearClip * ur, layermask);
				pose.nearClip = NCR / ur;
				RaycastHit rch;
				if (Physics.SphereCast(new Ray(target, ofs.normalized), NCR - 0.01f, out rch, ofs.magnitude, layermask, QueryTriggerInteraction.Ignore))
				{
					ofs = ofs.normalized * rch.distance;
				}
				pose.position = target + ofs;
				pose.lockCursor = LockCursor.value.value_b;

				controller.executor.SetFloat("Yaw", yaw);
				controller.executor.SetFloat("Pitch", pitch);
				controller.executor.SetFloat("YawWanted", yawWanted);
				controller.executor.SetFloat("PitchWanted", pitchWanted);

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
			get { return new Slot[10] {Name, Col, Sens, Damp, LockCursor, PitchMin, PitchMax, OffsetUp, Offset, OffsetDown}; }
		}

		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}

		public Slot Sens;
		public Slot Damp;
		public Slot LockCursor;
		public Slot PitchMin;
		public Slot PitchMax;
		public Slot OffsetUp;
		public Slot Offset;
		public Slot OffsetDown;
	}
}