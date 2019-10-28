using UnityEngine;
using System.Collections;

namespace CameraForge
{
	public class ThirdPersonDrive : ScriptModifier
	{
		public ThirdPersonDrive ()
		{
			// Init Inputs
			Sens = new Slot ("Sensitivity");
			Sens.value = 1f;
			Damp = new Slot ("Damp Rate");
			Damp.value = 0.3f;
			FS = new Slot ("Follow Speed");
			FS.value = 0f;
			Lock = new Slot ("Lock Cursor");
			Lock.value = false;
			DistMax = new Slot ("Distance Max");
			DistMax.value = 10f;
			DistMin = new Slot ("Distance Min");
			DistMin.value = 1.8f;
			PitchMax = new Slot ("Pitch Max");
			PitchMax.value = 55f;
			PitchMin = new Slot ("Pitch Min");
			PitchMin.value = -70f;
			RollCoef = new Slot ("Roll Coef");
			RollCoef.value = 0.0f;
			FovCoef = new Slot ("Fov Coef");
			FovCoef.value = 20.0f;
			BlurCoef = new Slot ("Blur Coef");
			BlurCoef.value = 0.0f;
			OffsetUp = new Slot ("Offset Up");
			OffsetUp.value = Vector3.zero;
			Offset = new Slot ("Offset");
			Offset.value = Vector3.zero;
			OffsetDown = new Slot ("Offset Down");
			OffsetDown.value = Vector3.zero;
		}

		// Internal vars
		float vyaw = 0;
		float vpitch = 0;
		Vector3 velUsed = Vector3.zero;
		float noControlTime = 0;
		float recalcDistTime = 0;
		float lockDistAfterClampTime = 0;
		float fovIncr = 0;
		float blur = 0;

		public override Pose Calculate ()
		{
			// Calculate Inputs
			Col.Calculate();
			Sens.Calculate();
			Damp.Calculate();
			FS.Calculate();
			Lock.Calculate();
			DistMax.Calculate();
			DistMin.Calculate();
			PitchMax.Calculate();
			PitchMin.Calculate();
			RollCoef.Calculate();
			FovCoef.Calculate();
			BlurCoef.Calculate();
			OffsetUp.Calculate();
			Offset.Calculate();
			OffsetDown.Calculate();
			Prev.Calculate();

			
			if (controller != null && controller.executor != null)
			{
				// Fetch vars
				float yaw = GetFloat("Yaw");
				float pitch = GetFloat("Pitch");
				float roll = GetFloat("Roll");
				float dist = GetFloat("Dist");

				float yawWanted = GetFloat("YawWanted");
				float pitchWanted = GetFloat("PitchWanted");
				float distWanted = GetFloat("DistWanted");
				float distLevel = GetFloat("DistLevel");
				//float distVelocity = GetFloat("DistVelocity");

				Vector3 defaultPos = CameraController.GetGlobalVar("Default Anchor").value_v;
				Transform anchor = CameraController.GetTransform("Anchor");
				Transform character = CameraController.GetTransform("Character");
				Vector3 velocity = controller.executor.GetVar("Driving Velocity").value_v;
				Vector3 rigidbody_vel = controller.executor.GetVar("Rigidbody Velocity").value_v;

				float followSpeed = FS.value.value_f;
				float sens = Sens.value.value_f;
				bool lockCursor = Lock.value.value_b;
				float damp = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
				float dt = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);

				bool rotButton = InputModule.Axis("Mouse Right Button").value_b && !GetBool("Mouse Op GUI");
				float mouseX = InputModule.Axis("Mouse X").value_f;
				float mouseY = InputModule.Axis("Mouse Y").value_f;
				float JoystickX = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime : 0f;
				float JoystickY = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime : 0f;
				float mouseW = GetBool("Mouse On Scroll") ? 0f : InputModule.Axis("Mouse ScrollWheel").value_f;
				bool inverseX = GetBool("Inverse X");
				bool inverseY = GetBool("Inverse Y");

				//float ACR = GetFloat("Activity Space Size");
				bool clampd = GetBool("Geometry Clampd");

				float distMax = DistMax.value.value_f;
				float distMin = DistMin.value.value_f;
				float pitchMax = PitchMax.value.value_f;
				float pitchMin = PitchMin.value.value_f;
				float rollCoef = RollCoef.value.value_f;
				float fovCoef = FovCoef.value.value_f;
				float blurCoef = BlurCoef.value.value_f;


				// Calc target
				Vector3 target = Vector3.zero;
				Vector3 target_up = Vector3.up;
				if (character != null)
				{
					target = character.position - character.up;
					target_up = character.up;
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
				float dw = -mouseW * 8;
				if (Prev.value.lockCursor || rotButton)
				{
					dx = Mathf.Clamp(mouseX * sens * (inverseX ? -1f : 1f), -200, 200) * Time.timeScale;
					dy = Mathf.Clamp(mouseY * sens * (inverseY ? -1f : 1f), -200, 200) * Time.timeScale;
				}
				
				float mdx = 0, mdy = 0;
				float jdx = 0, jdy = 0;
				
				mdx = dx;
				mdy = dy;
				
				jdx = JoystickX * sens * (inverseX ? -1f : 1f) * Time.timeScale;
				jdy = JoystickY * sens * (inverseY ? -1f : 1f) * Time.timeScale;
				
				dx += jdx;
				dy += jdy;
				
				dx = Mathf.Clamp(dx, -6 * sens, 6 * sens);
				dy = Mathf.Clamp(dy, -3 * sens, 3 * sens);
				
				yawWanted += dx;
				pitchWanted += dy;
				distLevel += dw;
				distLevel = Mathf.Clamp(distLevel, distMin, distMax);

				// lock dist
				if (dw != 0)
				{
					recalcDistTime = 3f;
					lockDistAfterClampTime = 0f;
				}
				if (recalcDistTime > 0)
					recalcDistTime -= Time.deltaTime;
				if (lockDistAfterClampTime > 0)
					lockDistAfterClampTime -= Time.deltaTime;
				if (clampd)
				{
					lockDistAfterClampTime = 2f;
					controller.executor.SetBool("Geometry Clampd", false);
				}
				
				distWanted = distLevel;

				// record noControlTime
				if (!Prev.value.lockCursor && rotButton)
					noControlTime = 0;
				else if (Mathf.Abs(jdx) + Mathf.Abs(jdy) > 0.2f || Mathf.Abs(mdx) + Mathf.Abs(mdy) > 8f)
					noControlTime = 0;
				else
					noControlTime += dt;
				
				controller.executor.SetFloat("No Rotate Time", noControlTime);

				// Follow
				if (noControlTime > 2.0f)
					velUsed = Vector3.Lerp(velUsed, velocity, 0.1f);
				else
					velUsed = Vector3.Lerp(velUsed, Vector3.zero, 0.5f);
				if (float.IsNaN(velUsed.x) || float.IsNaN(velUsed.y) || float.IsNaN(velUsed.z))
					velUsed = Vector3.zero;
				
				float vel_length = Mathf.Clamp01((velUsed.magnitude - 2) * 0.1f);
				float yaw_length = Mathf.Clamp01((new Vector2(velUsed.x, velUsed.z).magnitude-2) * 0.1f);
				
				Debug.DrawLine(target, target + velUsed, Color.cyan);
				if (vel_length > 0.01f)
				{
					Quaternion q = Quaternion.LookRotation(velUsed);
					Vector3 euler = q.eulerAngles;
					float fyaw = euler.y;
					float fpitch = -euler.x - 10;

					fyaw = Mathf.LerpAngle(yawWanted, fyaw, yaw_length);
					fpitch = Mathf.LerpAngle(pitchWanted, fpitch, vel_length * 0.02f);

					yawWanted = Mathf.SmoothDampAngle(yawWanted, fyaw, ref vyaw, 20.0f/followSpeed);
					pitchWanted = Mathf.SmoothDampAngle(pitchWanted, fpitch, ref vpitch, 40.0f/followSpeed);

//					float ayaw = Mathf.DeltaAngle(yawWanted, fyaw) * followSpeed * yaw_length;
//					float apitch = Mathf.DeltaAngle(pitchWanted, fpitch) * followSpeed * vel_length;
//					
//					ayaw -= (vyaw*vyaw) * Mathf.Sign(vyaw) * 1f;
//					apitch -= (vpitch*vpitch) * Mathf.Sign(vpitch) * 1f;
//					
//					ayaw = Mathf.Clamp(ayaw, -2000, 2000);
//					apitch = Mathf.Clamp(apitch, -2000, 2000);
//
//					vyaw = vyaw + ayaw * dt;
//					vpitch = vpitch + apitch * dt;
//					
//					vyaw = Mathf.Clamp(vyaw, -60, 60);
//					vpitch = Mathf.Clamp(vpitch, -60, 60);
//					
//					yawWanted += vyaw * dt;
//					pitchWanted += vpitch * dt;
				}

				yawWanted = Mathf.Repeat(yawWanted, 360f);
				pitchWanted = Mathf.Clamp(pitchWanted, pitchMin, pitchMax);
				distWanted = Mathf.Clamp(distWanted, distMin, distMax);

				yaw = Mathf.LerpAngle(yaw, yawWanted, damp);
				pitch = Mathf.LerpAngle(pitch, pitchWanted, damp);
				float vdist = controller.executor.GetVar("DistVelocity").value_f;
				if (lockDistAfterClampTime <= 0)
					dist = Mathf.SmoothDamp(dist, distWanted, ref vdist, 0.5f);
				dist = Mathf.Clamp(dist, distMin, distMax);
				controller.executor.SetVar("DistVelocity", vdist);

				Pose pose = Prev.value;
				pose.eulerAngles = new Vector3 (-pitch, yaw, 0);

				Vector3 forward = pose.rotation * Vector3.forward;
				Vector3 right = pose.rotation * Vector3.right;
				//Vector3 up = pose.rotation * Vector3.up;
				Vector3 lofs = Vector3.zero;
				if (pitch < 0)
				{
					lofs = Vector3.Slerp(Offset.value.value_v, OffsetDown.value.value_v, pitch / pitchMin);
				}
				else
				{
					lofs = Vector3.Slerp(Offset.value.value_v, OffsetUp.value.value_v, pitch / pitchMax);
				}
				float _dist = dist;
				if (distMin == distMax)
					_dist = distMin;
				pose.position = target - _dist * forward + lofs * _dist;
				pose.lockCursor = lockCursor;
				
				// Roll
				if (Mathf.Abs(rollCoef) > 0.001f)
				{
					Vector3 horz_forward = forward;
					Vector3 horz_right = right;
					horz_forward.y = 0;
					horz_forward.Normalize();
					horz_right.y = 0;
					
					float udotf = Vector3.Dot(target_up, horz_forward);
					Vector3 proj_up = target_up - udotf * horz_forward;
					float angle = Vector3.Angle(proj_up, Vector3.up);
					float falloff = 1f - Mathf.Pow((angle-90f)/90f, 2);
					if (angle < 90)
						angle = Mathf.Lerp(0, 90, falloff);
					else
						angle = Mathf.Lerp(180, 90, falloff);
					angle *= rollCoef;
					angle *= -Mathf.Sign(Vector3.Dot(proj_up, horz_right));
					roll = Mathf.Lerp(roll, angle, 0.15f);
				}
				else
				{
					roll = Mathf.Lerp(roll, 0, 0.15f);
				}

				pose.eulerAngles = new Vector3 (-pitch, yaw, roll);

				// Fov & blur
				float inc = Mathf.InverseLerp(10, 35, rigidbody_vel.magnitude);
				fovIncr = Mathf.Lerp(fovIncr, inc * fovCoef, 0.2f);
				pose.fov = Mathf.Clamp(pose.fov + fovIncr, 10f, 90f);
				blur = Mathf.Lerp(blur, inc * blurCoef, 0.2f);
				pose.motionBlur = Mathf.Clamp(blur, 0, 0.8f);

				// Set vars
				controller.executor.SetFloat("Yaw", yaw);
				controller.executor.SetFloat("Pitch", pitch);
				controller.executor.SetFloat("Roll", roll);
				controller.executor.SetFloat("Dist", dist);
				controller.executor.SetFloat("YawWanted", yawWanted);
				controller.executor.SetFloat("PitchWanted", pitchWanted);
				controller.executor.SetFloat("DistWanted", distWanted);
				controller.executor.SetFloat("DistLevel", distLevel);
				
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
			get { return new Slot[16] {Name, Col, Sens, FS, Damp, Lock, DistMin, DistMax, PitchMin, PitchMax, RollCoef, FovCoef, BlurCoef, OffsetUp, Offset, OffsetDown}; }
		}
		
		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}
		
		public Slot Sens;
		public Slot Damp;
		public Slot FS;
		public Slot Lock;
		public Slot DistMin;
		public Slot DistMax;
		public Slot PitchMin;
		public Slot PitchMax;
		public Slot RollCoef;
		public Slot FovCoef;
		public Slot BlurCoef;
		public Slot OffsetUp;
		public Slot Offset;
		public Slot OffsetDown;
	}
}