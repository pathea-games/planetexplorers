using UnityEngine;
using System.Collections;

namespace CameraForge
{
	public class ThirdPersonFollow : ScriptModifier
	{
		public ThirdPersonFollow ()
		{
			Sens = new Slot ("Sensitivity");
			Damp = new Slot ("Damp Rate");
			FS = new Slot ("Follow Speed");
			AnimFactor = new Slot ("Animation Factor");
			AnimFactor.value = 0.4f;
			Lock = new Slot ("Lock Cursor");
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
			Offset = new Slot ("Offset");
			Offset.value = Vector3.zero;
		}

		float vyaw = 0;
		float vpitch = 0;
		Vector3 velUsed = Vector3.zero;
		float noControlTime = 0;
		float recalcDistTime = 0;
		float lockDistAfterClampTime = 0;

		public override Pose Calculate ()
		{
			Col.Calculate();
			Sens.Calculate();
			Damp.Calculate();
			FS.Calculate();
			AnimFactor.Calculate();
			Lock.Calculate();
			DistMax.Calculate();
			DistMin.Calculate();
			PitchMax.Calculate();
			PitchMin.Calculate();
			RollCoef.Calculate();
			Offset.Calculate();
			Prev.Calculate();

			if (controller != null && controller.executor != null)
			{
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
				Transform neck_m = CameraController.GetTransform("Bone Neck M");
				Transform neck_r = CameraController.GetTransform("Bone Neck R");
				bool isRagdoll = GetBool("Is Ragdoll");
				Vector3 velocity = controller.executor.GetVar("Character Velocity").value_v;
				float followSpeed = FS.value.value_f;
				float sens = Sens.value.value_f;
				bool lockCursor = Lock.value.value_b;
				bool rotButton = InputModule.Axis("Mouse Right Button").value_b && !GetBool("Mouse Op GUI");
				float mouseX = InputModule.Axis("Mouse X").value_f;
				float mouseY = InputModule.Axis("Mouse Y").value_f;
				float JoystickX = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickX * 25f * Time.deltaTime : 0f;
				float JoystickY = SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickY * 12f * Time.deltaTime : 0f;
				float mouseW = GetBool("Mouse On Scroll") ? 0f : InputModule.Axis("Mouse ScrollWheel").value_f;
				bool inverseX = GetBool("Inverse X");
				bool inverseY = GetBool("Inverse Y");
				float ACR = GetFloat("Activity Space Size");
				bool clampd = GetBool("Geometry Clampd");
				float distMax = DistMax.value.value_f;
				float distMin = DistMin.value.value_f;
				float pitchMax = PitchMax.value.value_f;
				float pitchMin = PitchMin.value.value_f;
				float rollCoef = RollCoef.value.value_f;
				float damp = Mathf.Clamp(Damp.value.value_f, 0.005f, 1f);
				float dt = Mathf.Clamp(Time.deltaTime, 0.001f, 0.1f);

				// Calc target
				Vector3 target = Vector3.zero;
				Vector3 target_up = Vector3.up;
				//if (character != null)
				//{
                    //	if (neck_m != null && neck_r != null)
                    //	{
                    //		target = Vector3.Lerp(character.position, (isRagdoll ? neck_r : neck_m).position, AnimFactor.value.value_f);
                    //	}
                    //	else
                    //	{
                    //		target = character.position;
                    //	}
                    //	target_up = character.up;
                    //}
                    //else 
                    if (anchor != null)
				{
					target = anchor.position;
				}
				else
				{
					target = defaultPos;
				}
				//target -= velocity * 0.1f;

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

				if (dw != 0)
				{
					recalcDistTime = 3f;
					lockDistAfterClampTime = 0f;
				}
				if (recalcDistTime > 0)
					recalcDistTime -= Time.deltaTime;
				if (lockDistAfterClampTime > 0)
					lockDistAfterClampTime -= dt;
				if (clampd)
				{
					lockDistAfterClampTime = 2f;
					controller.executor.SetBool("Geometry Clampd", false);
				}

				float maxdist = Mathf.Clamp(ACR*2f, distMin, distMax);
				
				float distcoef = 1;
				distcoef = Mathf.Clamp(ACR * 0.15f, 0.15f, 1f);

				distWanted = distLevel * distcoef;

				if (!Prev.value.lockCursor && rotButton)
					noControlTime = 0;
				else if (Mathf.Abs(jdx) + Mathf.Abs(jdy) > 0.2f || Mathf.Abs(mdx) + Mathf.Abs(mdy) > 8f)
					noControlTime = 0;
				else
					noControlTime += dt;

				controller.executor.SetFloat("No Rotate Time", noControlTime);

				if (noControlTime > 1.3f)
					velUsed = Vector3.Lerp(velUsed, velocity, 0.2f);
				else
					velUsed = Vector3.Lerp(velUsed, Vector3.zero, 0.2f);
				if (float.IsNaN(velUsed.x) || float.IsNaN(velUsed.y) || float.IsNaN(velUsed.z))
					velUsed = Vector3.zero;

				float vel_length = Mathf.Clamp01(velUsed.magnitude * 0.2f);
				float yaw_length = Mathf.Clamp01(new Vector2(velUsed.x, velUsed.z).magnitude * 0.2f);

				Debug.DrawLine(target, target + velUsed, Color.cyan);
				if (vel_length > 0.01f)
				{
					Quaternion q = Quaternion.LookRotation(velUsed);
					Vector3 euler = q.eulerAngles;
					float fyaw = euler.y;
					float fpitch = -euler.x - 10;

					if (Mathf.DeltaAngle(yawWanted, fyaw) > 120f || Mathf.DeltaAngle(yawWanted, fyaw) < -120f)
					{
						fyaw = yawWanted;
					}
					fyaw = Mathf.LerpAngle(yawWanted, fyaw, yaw_length);
					fpitch = Mathf.LerpAngle(pitchWanted, fpitch, vel_length);

					Debug.DrawLine(target, target + Quaternion.Euler(new Vector3(-fpitch, fyaw, 0)) * Vector3.forward, Color.red);

					if (distcoef < 0.2f)
						followSpeed = followSpeed * 4f;
					else if (distcoef < 0.3f)
						followSpeed = followSpeed * 3f;
					else if (distcoef < 0.4f)
						followSpeed = followSpeed * 2.5f;
					else if (distcoef < 0.5f)
						followSpeed = followSpeed * 2f;
					else if (distcoef < 0.6f)
						followSpeed = followSpeed * 1.5f;

					yawWanted = Mathf.SmoothDampAngle(yawWanted, fyaw, ref vyaw, 20.0f/followSpeed);
					pitchWanted = Mathf.SmoothDampAngle(pitchWanted, fpitch, ref vpitch, 40.0f/followSpeed);

//					float ayaw = Mathf.DeltaAngle(yawWanted, fyaw) * followSpeed * yaw_length;
//					float apitch = Mathf.DeltaAngle(pitchWanted, fpitch) * followSpeed * vel_length;
//
//					ayaw -= (vyaw*vyaw) * Mathf.Sign(vyaw) * 0.2f;
//					apitch -= (vpitch*vpitch) * Mathf.Sign(vpitch) * 0.2f;
//
//					ayaw = Mathf.Clamp(ayaw, -2000, 2000);
//					apitch = Mathf.Clamp(apitch, -2000, 2000);
//					
//					Debug.DrawLine(target, target + Quaternion.Euler(new Vector3(-apitch, ayaw, 0)) * Vector3.forward, Color.green);
//
//					vyaw = vyaw + ayaw * dt;
//					vpitch = vpitch + apitch * dt;
//
//					vyaw = Mathf.Clamp(vyaw, -60, 60);
//					vpitch = Mathf.Clamp(vpitch, -60, 60);
//
//					Debug.DrawLine(target, target + Quaternion.Euler(new Vector3(-vpitch, vyaw, 0)) * Vector3.forward, Color.blue);
//					
//					yawWanted += vyaw * dt;
//					pitchWanted += vpitch * dt;
				}


				yawWanted = Mathf.Repeat(yawWanted, 360f);
				pitchWanted = Mathf.Clamp(pitchWanted, pitchMin, pitchMax);
				distWanted = Mathf.Clamp(distWanted, distMin, maxdist);

				yaw = Mathf.LerpAngle(yaw, yawWanted, damp);
				pitch = Mathf.LerpAngle(pitch, pitchWanted, damp);
				float vdist = controller.executor.GetVar("DistVelocity").value_f;
				if (lockDistAfterClampTime <= 0)
					dist = Mathf.SmoothDamp(dist, distWanted, ref vdist, 0.5f);
				else if (dist < distMin)
					dist = Mathf.SmoothDamp(dist, distMin, ref vdist, 0.5f);
				controller.executor.SetVar("DistVelocity", vdist);

				Pose pose = Prev.value;
				pose.eulerAngles = new Vector3 (-pitch, yaw, 0);
				Vector3 forward = pose.rotation * Vector3.forward;
				Vector3 right = pose.rotation * Vector3.right;
				Vector3 up = pose.rotation * Vector3.up;
				Vector3 ofs = Offset.value.value_v.x * right + Offset.value.value_v.y * up + Offset.value.value_v.z * forward;
				float _dist = dist;
				if (distMin == distMax)
					_dist = distMin;
                //pose.position = target - _dist * forward + ofs;
                pose.position = target - _dist * forward;
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
					if (angle > 90)
						angle = 90;
					angle = angle * Mathf.Pow(angle/90f, 2) * rollCoef;
					angle *= -Mathf.Sign(Vector3.Dot(proj_up, horz_right));
					roll = Mathf.Lerp(roll, angle, 0.15f);
				}
				else
				{
					roll = Mathf.Lerp(roll, 0, 0.15f);
				}
				pose.eulerAngles = new Vector3 (-pitch, yaw, roll);

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
			get { return new Slot[13] {Name, Col, Sens, FS, AnimFactor, Damp, Lock, DistMin, DistMax, PitchMin, PitchMax, RollCoef, Offset}; }
		}

		public override PoseSlot[] poseslots
		{
			get { return new PoseSlot[1] {Prev}; }
		}

		public Slot Sens;
		public Slot Damp;
		public Slot FS;
		public Slot AnimFactor;
		public Slot Lock;
		public Slot DistMin;
		public Slot DistMax;
		public Slot PitchMin;
		public Slot PitchMax;
		public Slot RollCoef;
		public Slot Offset;
	}
}