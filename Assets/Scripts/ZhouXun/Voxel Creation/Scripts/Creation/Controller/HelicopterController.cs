#define PLANET_EXPLORERS
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OLD
{
	/*
	public class HelicopterController : DrivingController
	{
		// Parts
		public List<VCPVtolRotorFunc> m_Rotors;
		public List<VCPVtolThrusterFunc> m_Thrusters;
		public List<VCPHeadLightFunc> m_Lights;
		public List<VCPLandingGearFunc> m_LandingGears;

		public bool m_UseLandHeightMap = false;
		public float m_AtmosphereCoef = 1;
		public float m_LiftLevel = 0;
		private float m_SpeedScale = 0.5f;
		public float TargetVertSpeed { get { return m_LiftLevel * m_SpeedScale; } }
		private float m_LastVertSpeed = 0;

		private Vector3 m_UpWanted = Vector3.up;
		public float HorzSpeed { get { return new Vector2(m_Rigidbody.velocity.x, m_Rigidbody.velocity.z).magnitude; } }
		public float VertSpeed { get { return m_Rigidbody.velocity.y; } }

		public override void Init(CreationData crd, int itemInstanceId)
		{
			base.Init(crd, itemInstanceId);

			m_Rigidbody.drag = 0.05f;
			m_Rigidbody.angularDrag = 3f;
			m_Rigidbody.centerOfMass = new Vector3(0, m_CreationData.m_Attribute.m_CenterOfMass.y, 0);

			LoadParts(ref m_Rotors);
			LoadParts(ref m_Thrusters);
			LoadParts(ref m_Lights);
			LoadParts(ref m_LandingGears);

			(m_Cockpit as VCPVtolCockpitFunc).m_Controller = this;
			if (m_Rotors.Count > 0)
			{
				float maxz = m_Rotors[0].transform.localPosition.z;	// We using it to get rotors' balance position
				float minz = m_Rotors[0].transform.localPosition.z;	// We using it to get rotors' balance position
				foreach (VCPVtolRotorFunc rotor in m_Rotors)
				{
					if (rotor.transform.localPosition.z > maxz)
						maxz = rotor.transform.localPosition.z;
					if (rotor.transform.localPosition.z < minz)
						minz = rotor.transform.localPosition.z;
				}
				// Avoid maxz <= minz 
				if (maxz <= minz)
				{
					maxz = minz;
					minz -= 0.001f;
				}

				foreach (VCPVtolRotorFunc rotor in m_Rotors)
				{
					// Calculate rotors' balance position
					rotor.m_BalancePosition = (rotor.transform.localPosition.z - (maxz + minz) * 0.5F) / ((maxz - minz) * 0.5F);
					rotor.m_Controller = this;
				}
			}
			foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
			{
				thruster.m_Controller = this;
			}
			foreach (VCPJetExhaustFunc jet in m_JetExhausts)
				jet.m_Torque = true;
		}

		/*
		// 17/18/23/24(Bytes)
		public override byte[] GetState ()
		{
			byte[] retval = null;
			using ( MemoryStream ms = new MemoryStream () )
			{
				BinaryWriter w = new BinaryWriter (ms);
				// 12 B
				w.Write(transform.position.x);
				w.Write(transform.position.y);
				w.Write(transform.position.z);
				// 4 B
				int euler = VCUtils.CompressEulerAngle(transform.eulerAngles);
				w.Write(euler);

				// Always 1 B
				if ( m_Rotors.Count > 0 )
				{
					sbyte b = (sbyte)(Mathf.RoundToInt( Mathf.Clamp(m_Rotors[0].m_CurrRPM / 8, -128, 127) ));
					w.Write(b);
				}
			
				// If have lights 1 B; If no light 0 B
				if ( m_Lights.Count > 0 )
				{
					byte b = (byte)(m_Lights[0].m_IsTurnOn ? 1 : 0);
					w.Write(b);
				}
			
				// If have ctrlturrets 6 B; If no ctrlturret 0 B
				if ( m_CtrlTurrets.Count > 0 )
				{
					short x = (short)Mathf.RoundToInt(m_CtrlTurrets[0].NetworkTarget.x);
					short y = (short)Mathf.RoundToInt(m_CtrlTurrets[0].NetworkTarget.y);
					short z = (short)Mathf.RoundToInt(m_CtrlTurrets[0].NetworkTarget.z);
					w.Write(x);
					w.Write(y);
					w.Write(z);
				}
			
				w.Close();
				retval = ms.ToArray();
				ms.Close();
			}
			return retval;
		}
		


		const int _byteCount = 32;
		static byte[] _syncData = new byte[_byteCount];
		static MemoryStream _memoryStream = new MemoryStream(_byteCount);
		static BinaryWriter _binaryWriter = new BinaryWriter(_memoryStream);

		static Vector3 _tempV3;


		public override byte[] GetState()
		{
			_binaryWriter.Seek(0, SeekOrigin.Begin);

			// 写入位置 (12 bytes)
			_tempV3 = transform.position;
			_binaryWriter.Write(_tempV3.x);
			_binaryWriter.Write(_tempV3.y);
			_binaryWriter.Write(_tempV3.z);

			// 写入旋转 (4 bytes)
			_binaryWriter.Write(WhiteCat.Utility.CompressEulerAngles(transform.eulerAngles));


			return _syncData;
		}


		public override void SetState(byte[] buffer)
		{
			using (MemoryStream ms = new MemoryStream(buffer))
			{
				BinaryReader r = new BinaryReader(ms);
				float px = r.ReadSingle();
				float py = r.ReadSingle();
				float pz = r.ReadSingle();
				int euler = r.ReadInt32();
				if (m_Rotors.Count > 0)
				{
					sbyte b = r.ReadSByte();
					foreach (VCPVtolRotorFunc rotor in m_Rotors)
					{
						rotor.m_CurrRPM = (int)(b) * 8.0f;
					}
				}
				if (m_Lights.Count > 0)
				{
					byte b = r.ReadByte();
					bool is_turn_on = (b != 0);
					foreach (VCPHeadLightFunc light in m_Lights)
					{
						light.m_IsTurnOn = is_turn_on;
					}
				}
				if (m_CtrlTurrets.Count > 0)
				{
					short tx = r.ReadInt16();
					short ty = r.ReadInt16();
					short tz = r.ReadInt16();
					foreach (VCPCtrlTurretFunc ct in m_CtrlTurrets)
					{
						ct.SetTarget(new Vector3(tx, ty, tz));
					}
				}

				proxyPosition = new Vector3(px, py, pz);
				proxyRotation = Quaternion.Euler(VCUtils.UncompressEulerAngle(euler));

				r.Close();
				ms.Close();
			}
		}

		// Input
		private bool m_ForwardInput = false;
		private bool m_BackwardInput = false;
		private bool m_LeftInput = false;
		private bool m_RightInput = false;
		private bool m_UpInput = false;
		private bool m_DownInput = false;
		private bool m_LightInput = false;
		private bool m_JetInput = false;
		private void UserInput()
		{
			if (m_NetChar != ENetCharacter.nrOwner)
				return;
#if PLANET_EXPLORERS
			if (InputManager.Instance != null)
			{
				m_ForwardInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveForward);
				m_BackwardInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveBack);
				m_LeftInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveLeft);
				m_RightInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveRight);
				m_UpInput = Input.GetKey(KeyCode.Space);
				m_DownInput = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
				m_LightInput = InputManager.Instance.IsKeyDown(KeyFunction.Vehicle_Light);
				m_JetInput = Input.GetKey(KeyCode.Space);
			}
			else
#endif
			{
				m_ForwardInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
				m_BackwardInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
				m_LeftInput = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
				m_RightInput = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
				m_UpInput = Input.GetKey(KeyCode.Space);
				m_DownInput = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
				m_LightInput = Input.GetKeyDown(KeyCode.L);
				m_JetInput = Input.GetKey(KeyCode.Space);
			}
		}

		// Main logic
		Vector3 proxyPosition = Vector3.zero;
		Quaternion proxyRotation = Quaternion.identity;
		protected override void Update()
		{
			base.Update();

			// Active
			if (m_Active)
			{
				if (m_NetChar == ENetCharacter.nrOwner)
				{
					if (InWater)
						m_Rigidbody.drag = HorzSpeed * 0.1f + 4f;
					else
						m_Rigidbody.drag = HorzSpeed * 0.007f + 0.07f;

					// No matter is driving or not
					m_AtmosphereCoef = (m_Cockpit as VCPVtolCockpitFunc).AtmosphereCoef;
					m_LastVertSpeed = m_Rigidbody.velocity.y;

					// Fuel
					float fuelCost = 0;
					foreach (VCPVtolRotorFunc rotor in m_Rotors)
						fuelCost += rotor.m_CurrPower * Time.deltaTime * 0.0006f;
					foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
						fuelCost += thruster.m_CurrPower * Time.deltaTime * 0.0004f;
					foreach (VCPJetExhaustFunc jet in m_JetExhausts)
						fuelCost += jet.CurrentPower * Time.deltaTime * 0.02f;

					if (fuelCost > 0)
						m_CreationData.m_Fuel -= fuelCost;
					if (m_CreationData.m_Fuel < 0)
						m_CreationData.m_Fuel = 0;
					proxyPosition = transform.position;
					proxyRotation = transform.rotation;
				}
				else
				{
					transform.position = Vector3.Lerp(transform.position, proxyPosition, Time.deltaTime * 5);
					transform.rotation = Quaternion.Slerp(transform.rotation, proxyRotation, Time.deltaTime * 8);
					GetComponent<Rigidbody>().velocity = Vector3.zero;
				}

				//			SyncPassengers();

				// Debug
				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, transform.up * 3f, Color.green);
				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, -transform.up * 3f, Color.green);
				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, transform.right * 3f, Color.red);
				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, -transform.right * 3f, Color.red);
				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, transform.forward * 3f, Color.blue);
				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, -transform.forward * 3f, Color.blue);
			} // End 'Active'
			else
			{
				m_AtmosphereCoef = 1;
			}
		}

		// Physics
		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			// Active
			if (m_Active)
			{
				if (m_Rigidbody == null) return;
				float _mass = m_Rigidbody.mass;
				CreationBound cb = GetComponent<CreationBound>();
				float r = 10;
				if (cb != null)
					r = cb.m_Bound.extents.magnitude * 0.7f;
				if (r < 1)
					r = 1;
				m_Rigidbody.inertiaTensor = new Vector3(_mass * r * 0.1f, _mass * r * 0.1f, _mass * r * 0.1f);
				if (m_CreationData == null)
				{
					Debug.LogError("m_CreationData is null");
				}
				m_Rigidbody.centerOfMass = new Vector3(0, m_CreationData.m_Attribute.m_CenterOfMass.y, 0);
				if (m_NetChar == ENetCharacter.nrOwner)
				{

				}
				else
				{
					GetComponent<Rigidbody>().velocity = Vector3.zero;
				}
			} // End 'Active'
		}

		// Driving
		protected override void Driving()
		{
			base.Driving();
			// Has fuel
			if (m_CreationData.m_Fuel > 0 && !InWater)
			{
				UserInput();
				bool on_key_turing = false;
				// Motor and Brake
				if (m_ForwardInput)
				{
					foreach (VCPVtolRotorFunc rotor in m_Rotors)
						rotor.DrivingForward();
					foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
						thruster.DrivingForward();
				}
				else if (m_BackwardInput)
				{
					foreach (VCPVtolRotorFunc rotor in m_Rotors)
						rotor.DrivingBackward();
					foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
						thruster.DrivingBackward();
				}
				else
				{
					foreach (VCPVtolRotorFunc rotor in m_Rotors)
						rotor.DrivingStay();
					foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
						thruster.DrivingStay();
				}

				// Turning
				if (m_Cockpit.m_TurningMode == ETurningMode.vcmKeyboardTurning ||
					m_Cockpit.m_TurningMode == ETurningMode.vcmMixTurning)
				{
					if (m_LeftInput && !m_RightInput)
					{
						foreach (VCPVtolRotorFunc rotor in m_Rotors)
							rotor.TurnLeft();
						foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
							thruster.TurnLeft();
						on_key_turing = true;
						m_UpWanted = Vector3.Lerp(Vector3.up,
												   Vector3.Cross(transform.forward, Vector3.up).normalized,
												   Mathf.Clamp(0.01f * HorzSpeed, 0, 0.2f));
					}
					else if (!m_LeftInput && m_RightInput)
					{
						foreach (VCPVtolRotorFunc rotor in m_Rotors)
							rotor.TurnRight();
						foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
							thruster.TurnRight();
						on_key_turing = true;
						m_UpWanted = Vector3.Lerp(Vector3.up,
												   -Vector3.Cross(transform.forward, Vector3.up).normalized,
												   Mathf.Clamp(0.01f * HorzSpeed, 0, 0.2f));
					}
					else
					{
						foreach (VCPVtolRotorFunc rotor in m_Rotors)
							rotor.NotTurning();
						foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
							thruster.NotTurning();
						on_key_turing = false;
						m_UpWanted = Vector3.up;
					}
				}
				if (m_Cockpit.m_TurningMode == ETurningMode.vcmCameraTurning
					|| m_Cockpit.m_TurningMode == ETurningMode.vcmMixTurning && !on_key_turing)
				{
					if (Camera.main != null)
					{
						Vector3 cam_forward = Camera.main.transform.forward;
						Vector3 vehicle_up = transform.up;
						Vector3 vehicle_right = transform.right;
						Vector3 vehicle_forward = transform.forward;
						cam_forward = (cam_forward - Vector3.Dot(cam_forward, vehicle_up) * vehicle_up).normalized;
						float steering_value = Vector3.Dot(cam_forward, vehicle_right);
						if (Vector3.Dot(cam_forward, vehicle_forward) < 0)
							steering_value = (steering_value > 0) ? (2 - steering_value) : (-2 - steering_value);
						steering_value *= .5F;

						m_UpWanted = Vector3.Lerp(Vector3.up,
												   -Vector3.Cross(transform.forward, Vector3.up).normalized,
												  Mathf.Clamp(0.01f * HorzSpeed, 0, 0.2f) * steering_value);
						foreach (VCPVtolRotorFunc rotor in m_Rotors)
						{
							rotor.m_SteeringTarget.x = steering_value;
						}
					}
				}

				// Light
				if (m_LightInput)
				{
					foreach (VCPHeadLightFunc light in m_Lights)
						light.m_IsTurnOn = !light.m_IsTurnOn;
				}
				// Jet
				foreach (VCPJetExhaustFunc jet in m_JetExhausts)
					jet.m_Jeting = m_JetInput;

				// Lift motor
				if (m_LiftLevel > -10.5f)
				{
					float delta_vy = TargetVertSpeed - m_Rigidbody.velocity.y;
					float acc = (m_Rigidbody.velocity.y - m_LastVertSpeed) / Time.deltaTime;
					float delta_ay = delta_vy * 0.7f - acc;
					float factor = delta_ay;

					foreach (VCPVtolRotorFunc rotor in m_Rotors)
					{
						rotor.m_PowerTarget += factor * rotor.m_Property.m_MaxPower * Time.deltaTime * 0.1f;
						rotor.m_PowerTarget = Mathf.Clamp(rotor.m_PowerTarget, 0, rotor.m_Property.m_MaxPower);
					}
					foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
					{
						thruster.m_yMovement += Vector3.up * (factor * thruster.m_Property.m_MaxPower * Time.deltaTime * 0.1f);
						if (thruster.m_yMovement.y < 0)
							thruster.m_yMovement.y = 0;
					}
					// Balancing
					Vector3 adjust = m_UpWanted - transform.up;
					Vector3 torque = Vector3.Cross(m_UpWanted, adjust).normalized * adjust.magnitude * m_Rigidbody.mass * 10f;
					m_Rigidbody.AddTorque(torque);

					// Rigidbody's velocity dump to horz forward
					if (HorzSpeed > 0.1f)
					{
						Vector3 horz_forward = Vector3.Cross(Vector3.down, transform.right).normalized;
						Vector3 proj_vel = Vector3.Dot(m_Rigidbody.velocity, horz_forward) * transform.forward;
						Vector3 side_vel = m_Rigidbody.velocity - proj_vel;
						Vector3 vert_vel = Vector3.up * side_vel.y;
						side_vel.y = 0;
						m_Rigidbody.velocity = proj_vel + 0.85f * side_vel + vert_vel;

					}
				}
				else
				{
					foreach (VCPVtolRotorFunc rotor in m_Rotors)
						rotor.ShutDown();
					foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
						thruster.ShutDown();
				}
			} // End 'HasFuel'

			// No Fuel
			else
			{
				foreach (VCPVtolRotorFunc rotor in m_Rotors)
					rotor.ShutDown();
				foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
					thruster.ShutDown();
				foreach (VCPHeadLightFunc light in m_Lights)
					light.m_IsTurnOn = false;
				foreach (VCPJetExhaustFunc jet in m_JetExhausts)
					jet.m_Jeting = false;
			}
		}

		// Idle
		protected override void Idle()
		{
			base.Idle();
			foreach (VCPVtolRotorFunc rotor in m_Rotors)
				rotor.ShutDown();
			foreach (VCPVtolThrusterFunc thruster in m_Thrusters)
				thruster.ShutDown();
			foreach (VCPHeadLightFunc light in m_Lights)
				light.m_IsTurnOn = false;
			foreach (VCPJetExhaustFunc jet in m_JetExhausts)
				jet.m_Jeting = false;
		}

		// Switches

		public bool m_ShowDebugInfo = false;
		void OnGUI()
		{
			if (m_ShowDebugInfo)
			{
				GUI.color = Color.black;
				GUI.Label(new Rect(50, 50, 500, 20), "Speed: " + m_Rigidbody.velocity.magnitude.ToString("#,##0.00") + " m/s    " + (m_Rigidbody.velocity.magnitude * 3.6f).ToString("#,##0.00") + " km/h");
				GUI.Label(new Rect(50, 80, 500, 20), "Fuel: " + m_CreationData.m_Fuel.ToString("#,##0"));
			}
		}
	}
	*/
}