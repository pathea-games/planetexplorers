using UnityEngine;
using System.Collections;

namespace CameraForge
{
	public class FreeLook : ScriptModifier
	{
		public FreeLook ()
		{
			Sens = new Slot ("Sensitivity");
			Damp = new Slot ("Damp Rate");
			Lock = new Slot ("Lock");
			Lock.value = false;
			PitchMax = new Slot ("Pitch Max");
			PitchMax.value = 55f;
			PitchMin = new Slot ("Pitch Min");
			PitchMin.value = -70f;
			DistLimit = new Slot ("Distance Limit");
			DistLimit.value = 30f;
		}

		public override Pose Calculate ()
		{
			Col.Calculate();
			Sens.Calculate();
			Damp.Calculate();
			Lock.Calculate();
			PitchMax.Calculate();
			PitchMin.Calculate();
			DistLimit.Calculate();
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
				float sens = Sens.value.value_f;
				bool rotButton = InputModule.Axis("Mouse Right Button").value_b;
				float mouseX = InputModule.Axis("Mouse X").value_f;
				float mouseY = InputModule.Axis("Mouse Y").value_f;
				float JoystickX = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime : 0f;
				float JoystickY = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime : 0f;
				float mouseW = InputModule.Axis("Mouse ScrollWheel").value_f;
				bool inverseX = GetBool("Inverse X");
				bool inverseY = GetBool("Inverse Y");
				float pitchMax = PitchMax.value.value_f;
				float pitchMin = PitchMin.value.value_f;
				float damp = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
				float distLimit = DistLimit.value.value_f;
				float dt = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);

				// Calc target
				Vector3 target = Vector3.zero;
				if (character != null)
				{
					target = character.position;
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
				float dw = mouseW * 8;
				if (Prev.value.lockCursor || rotButton)
				{
					dx = Mathf.Clamp(mouseX * sens * (inverseX ? -1f : 1f), -200, 200);
					dy = Mathf.Clamp(mouseY * sens * (inverseY ? -1f : 1f), -200, 200);
				}

				//float mdx = 0, mdy = 0;
				float jdx = 0, jdy = 0;

				//mdx = dx;
				//mdy = dy;

				jdx = JoystickX * sens * (inverseX ? -1f : 1f);
				jdy = JoystickY * sens * (inverseY ? -1f : 1f);

				dx += jdx;
				dy += jdy;

				yawWanted += dx;
				pitchWanted += dy;

				yawWanted = Mathf.Repeat(yawWanted, 360f);
				pitchWanted = Mathf.Clamp(pitchWanted, pitchMin, pitchMax);

				yaw = Mathf.LerpAngle(yaw, yawWanted, damp);
				pitch = Mathf.LerpAngle(pitch, pitchWanted, damp);

				Pose pose = Prev.value;
				pose.eulerAngles = new Vector3 (-pitch, yaw, 0f);
				pose.lockCursor = false;

				Vector3 forward = pose.rotation * Vector3.forward;
				Vector3 right = pose.rotation * Vector3.right;
				//Vector3 up = pose.rotation * Vector3.up;
				if (PeInput.Get(PeInput.LogicFunction.MoveForward))
				{
					pose.position = pose.position + forward * dt * 15f;
				}
				if (PeInput.Get(PeInput.LogicFunction.MoveBackward))
				{
					pose.position = pose.position - forward * dt * 15f;
				}
				if (PeInput.Get(PeInput.LogicFunction.MoveLeft))
				{
					pose.position = pose.position - right * dt * 15f;
				}
				if (PeInput.Get(PeInput.LogicFunction.MoveRight))
				{
					pose.position = pose.position + right * dt * 15f;
				}

				if (dw != 0)
				{
					pose.position = pose.position + forward * dw;
				}

				Vector3 ofs = pose.position - target;
				ofs = Vector3.ClampMagnitude(ofs, distLimit);
				pose.position = target + ofs;

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
			get { return new Slot[8] {Name, Col, Sens, Damp, Lock, PitchMin, PitchMax, DistLimit}; }
		}

		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}

		public Slot Sens;
		public Slot Damp;
		public Slot Lock;
		public Slot PitchMin;
		public Slot PitchMax;
		public Slot DistLimit;
	}
}