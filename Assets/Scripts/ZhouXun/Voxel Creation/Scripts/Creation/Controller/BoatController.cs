//#define PLANET_EXPLORERS
////#define BOAT_TEST
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;



//namespace OLD
//{
//	/*
//	public class BoatController : DrivingController
//	{
//		// Parts
//		public List<VCPShipPropellerFunc> m_Propellers;
//		public List<VCPShipRudderFunc> m_Rudders;
//		public List<VCPHeadLightFunc> m_Lights;

//		public float HorzSpeed { get { return new Vector2(m_Rigidbody.velocity.x, m_Rigidbody.velocity.z).magnitude; } }
//		public float VertSpeed { get { return m_Rigidbody.velocity.y; } }

//		public override void Init(CreationData crd, int itemInstanceId)
//		{
//			base.Init(crd, itemInstanceId);

//			m_DeactiveTime = 20f;

//			m_Rigidbody.drag = 1f;
//			m_Rigidbody.angularDrag = 2.5f;
//			m_Rigidbody.centerOfMass = new Vector3(0, m_CreationData.m_Attribute.m_CenterOfMass.y, 0);

//			LoadParts(ref m_Propellers);
//			LoadParts(ref m_Rudders);
//			LoadParts(ref m_Lights);

//			(m_Cockpit as VCPShipCockpitFunc).m_Controller = this;
//			if (m_Propellers.Count > 0)
//			{
//				float maxz = m_Propellers[0].transform.localPosition.z;	// We using it to get propellers' balance position
//				float minz = m_Propellers[0].transform.localPosition.z;	// We using it to get propellers' balance position
//				foreach (VCPShipPropellerFunc propeller in m_Propellers)
//				{
//					if (propeller.transform.localPosition.z > maxz)
//						maxz = propeller.transform.localPosition.z;
//					if (propeller.transform.localPosition.z < minz)
//						minz = propeller.transform.localPosition.z;
//				}
//				// Avoid maxz <= minz 
//				if (maxz <= minz)
//				{
//					maxz = minz;
//					minz -= 0.001f;
//				}

//				foreach (VCPShipPropellerFunc propeller in m_Propellers)
//				{
//					// Calculate propellers' balance position
//					propeller.m_BalancePosition = (propeller.transform.localPosition.z - (maxz + minz) * 0.5F) / ((maxz - minz) * 0.5F);
//					propeller.m_Controller = this;
//				}
//			}
//			foreach (VCPShipRudderFunc rudder in m_Rudders)
//			{
//				rudder.m_Controller = this;
//			}
//			general_target_pos = m_Cockpit.m_CameraPoint.localPosition;
//		}

//		public override byte[] GetState()
//		{
//			byte[] retval = null;
//			using (MemoryStream ms = new MemoryStream())
//			{
//				BinaryWriter w = new BinaryWriter(ms);
//				// 12 B
//				w.Write(transform.position.x);
//				w.Write(transform.position.y);
//				w.Write(transform.position.z);
//				// 4 B
//				int euler = VCUtils.CompressEulerAngle(transform.eulerAngles);
//				w.Write(euler);

//				// If have lights 1 B; If no light 0 B
//				if (m_Lights.Count > 0)
//				{
//					byte b = (byte)(m_Lights[0].m_IsTurnOn ? 1 : 0);
//					w.Write(b);
//				}

//				// If have ctrlturrets 6 B; If no ctrlturret 0 B
//				if (m_CtrlTurrets.Count > 0)
//				{
//					short x = (short)Mathf.RoundToInt(m_CtrlTurrets[0].NetworkTarget.x);
//					short y = (short)Mathf.RoundToInt(m_CtrlTurrets[0].NetworkTarget.y);
//					short z = (short)Mathf.RoundToInt(m_CtrlTurrets[0].NetworkTarget.z);
//					w.Write(x);
//					w.Write(y);
//					w.Write(z);
//				}

//				w.Close();
//				retval = ms.ToArray();
//				ms.Close();
//			}
//			return retval;
//		}

//		public override void SetState(byte[] buffer)
//		{
//			using (MemoryStream ms = new MemoryStream(buffer))
//			{
//				BinaryReader r = new BinaryReader(ms);
//				float px = r.ReadSingle();
//				float py = r.ReadSingle();
//				float pz = r.ReadSingle();
//				int euler = r.ReadInt32();

//				if (m_Lights.Count > 0)
//				{
//					byte b = r.ReadByte();
//					bool is_turn_on = (b != 0);
//					foreach (VCPHeadLightFunc light in m_Lights)
//					{
//						light.m_IsTurnOn = is_turn_on;
//					}
//				}
//				if (m_CtrlTurrets.Count > 0)
//				{
//					short tx = r.ReadInt16();
//					short ty = r.ReadInt16();
//					short tz = r.ReadInt16();
//					foreach (VCPCtrlTurretFunc ct in m_CtrlTurrets)
//					{
//						ct.SetTarget(new Vector3(tx, ty, tz));
//					}
//				}

//				proxyPosition = new Vector3(px, py, pz);
//				proxyRotation = Quaternion.Euler(VCUtils.UncompressEulerAngle(euler));

//				r.Close();
//				ms.Close();
//			}
//		}

//		// Input
//		private bool m_ForwardInput = false;
//		private bool m_BackwardInput = false;
//		private bool m_LeftInput = false;
//		private bool m_RightInput = false;
//		private bool m_LightInput = false;
//		private bool m_JetInput = false;
//		private void UserInput()
//		{
//			if (m_NetChar != ENetCharacter.nrOwner)
//				return;
//#if PLANET_EXPLORERS
//			if (InputManager.Instance != null)
//			{
//				m_ForwardInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveForward);
//				m_BackwardInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveBack);
//				m_LeftInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveLeft);
//				m_RightInput = InputManager.Instance.IsKeyPressed(KeyFunction.MoveRight);
//				m_LightInput = InputManager.Instance.IsKeyDown(KeyFunction.Vehicle_Light);
//				m_JetInput = Input.GetKey(KeyCode.Space);
//			}
//			else
//#endif
//			{
//				m_ForwardInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
//				m_BackwardInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
//				m_LeftInput = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
//				m_RightInput = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
//				m_LightInput = Input.GetKeyDown(KeyCode.L);
//				m_JetInput = Input.GetKey(KeyCode.Space);
//			}
//		}

//		public float FluidDisplacement(Vector3 wpos)
//		{
//#if BOAT_TEST
//		return Mathf.Clamp01(97 - wpos.y);
//#elif PLANET_EXPLORERS
//			if (VFVoxelWater.self != null && VFVoxelWater.self.Voxels != null)
//			{
//				int x = Mathf.RoundToInt(wpos.x);
//				int z = Mathf.RoundToInt(wpos.z);

//				float base_y = Mathf.Floor(wpos.y - 0.5f) + 0.5f;
//				float bias_y = wpos.y - base_y;
//				int y0 = Mathf.FloorToInt(base_y) + 1;
//				int y1 = y0 + 1;
//				VFVoxel voxel0 = VFVoxelWater.self.Voxels.SafeRead(x, y0, z);
//				VFVoxel voxel1 = VFVoxelWater.self.Voxels.SafeRead(x, y1, z);
//				return Mathf.Clamp01(Mathf.Lerp((float)voxel0.Volume, (float)voxel1.Volume, bias_y) / 255.0f);
//			}
//			return 0;
//#else
//		return 0;
//#endif
//		}

//		// Main logic
//		Vector3 proxyPosition = Vector3.zero;
//		Quaternion proxyRotation = Quaternion.identity;
//		protected override void Update()
//		{
//			base.Update();

//			// Active
//			if (m_Active)
//			{
//				if (m_NetChar == ENetCharacter.nrOwner)
//				{
//					// No matter is driving or not

//					// Fuel
//					float fuelCost = 0;
//					foreach (VCPShipPropellerFunc propeller in m_Propellers)
//						fuelCost += Mathf.Abs(propeller.m_CurrPower) * Time.deltaTime * 0.001f;
//					foreach (VCPJetExhaustFunc jet in m_JetExhausts)
//						fuelCost += jet.CurrentPower * Time.deltaTime * 0.02f;

//					if (fuelCost > 0)
//						m_CreationData.m_Fuel -= fuelCost;
//					if (m_CreationData.m_Fuel < 0)
//						m_CreationData.m_Fuel = 0;
//					proxyPosition = transform.position;
//					proxyRotation = transform.rotation;

//					// Update Camera Target
//					UpdateCameraTarget(m_Cockpit.m_CameraPoint);
//				}
//				else
//				{
//					transform.position = Vector3.Lerp(transform.position, proxyPosition, Time.deltaTime * 5);
//					transform.rotation = Quaternion.Slerp(transform.rotation, proxyRotation, Time.deltaTime * 8);
//					GetComponent<Rigidbody>().velocity = Vector3.zero;
//				}

//				//		SyncPassengers();

//				// Debug
//				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, transform.up * 3f, Color.green);
//				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, -transform.up * 3f, Color.green);
//				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, transform.right * 3f, Color.red);
//				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, -transform.right * 3f, Color.red);
//				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, transform.forward * 3f, Color.blue);
//				Debug.DrawRay(m_Rigidbody.worldCenterOfMass, -transform.forward * 3f, Color.blue);
//			} // End 'Active'
//		}

//		// Physics
//		protected override void FixedUpdate()
//		{
//			base.FixedUpdate();
//			// Active
//			if (m_Active)
//			{
//				if (m_Rigidbody == null) return;
//				float _mass = Mathf.Max(m_Rigidbody.mass, 10000);
//				CreationBound cb = GetComponent<CreationBound>();
//				float r = 10;
//				if (cb != null)
//					r = cb.m_Bound.extents.magnitude * 0.8f;
//				if (r < 3)
//					r = 3;

//				m_Rigidbody.inertiaTensor = new Vector3(_mass * r, _mass * r, _mass * r * 0.5f);
//				m_Rigidbody.centerOfMass = new Vector3(0, m_CreationData.m_Attribute.m_CenterOfMass.y, 0);

//				bool isInWater = false;
//				if (m_NetChar == ENetCharacter.nrOwner)
//				{
//					if (m_CreationData.m_Attribute.m_FluidDisplacement != null)
//					{
//						foreach (VolumePoint vp in m_CreationData.m_Attribute.m_FluidDisplacement)
//						{
//							Vector3 wpos = transform.TransformPoint(vp.localPosition);
//							float buoyancy = 1000 * m_Gravity * vp.pos_volume * FluidDisplacement(wpos) * (Mathf.Sin(Time.time * 3f) * 0.03f + 0.985f);
//							if (buoyancy > 0.01f)
//							{
//								isInWater = true;
//								m_Rigidbody.AddForceAtPosition(Vector3.up * buoyancy, wpos);
//							}
//						}
//					}
//				}
//				else
//				{
//					GetComponent<Rigidbody>().velocity = Vector3.zero;
//				}
//				if (isInWater)
//				{
//					m_Rigidbody.drag = Mathf.Max(HorzSpeed * 0.120f, 1);
//					m_Rigidbody.angularDrag = 2.5f;
//				}
//				else
//				{
//					m_Rigidbody.drag = 0.08f;
//					m_Rigidbody.angularDrag = 0.05f;
//				}

//			} // End 'Active'
//			if (!m_Cockpit.m_IsDriving)
//			{
//				is_underwater_camera = false;
//			}
//		}

//		bool is_underwater_camera = false;
//		Vector3 general_target_pos = Vector3.zero;
//		void UpdateCameraTarget(Transform target)
//		{
//			if (PECameraMan.Instance == null)
//				return;

//			if (Input.GetKeyDown(KeyCode.Tab))
//				is_underwater_camera = !is_underwater_camera;
//			float dir = is_underwater_camera ? -1f : 1f;
//			float min = GetComponent<CreationBound>().m_Bound.extents.y * 2;

//			float camdist = (PECameraMan.Instance.m_Controller.currentMode as CameraThirdPerson).m_DistanceWanted;
//			float angle_x = PECameraMan.Instance.m_Controller.m_TargetCam.transform.localEulerAngles.x;
//			if (angle_x > 180)
//				angle_x -= 360;
//			float pitch = Mathf.Sin(Mathf.Max(-angle_x * dir * Mathf.Deg2Rad, 0));
//			float lift = 0;
//			if (is_underwater_camera)
//				lift = dir * (camdist * pitch + min);
//			else
//				lift = dir * (camdist + min) * pitch;

//			RaycastHit rch;
//			Ray ray = new Ray(m_Cockpit.m_CameraPoint.parent.TransformPoint(general_target_pos), Vector3.up * Mathf.Sign(lift));
//			if (Mathf.Abs(lift) > 0.01f &&
//				Physics.Raycast(ray, out rch, Mathf.Abs(lift), 1 << Pathea.Layer.VFVoxelTerrain))
//			{
//				target.position = ray.GetPoint(rch.distance - 1);
//			}
//			else
//			{
//				target.localPosition = general_target_pos + Vector3.up * lift;
//			}
//		}

//		// Driving
//		protected override void Driving()
//		{
//			base.Driving();
//			// Has fuel
//			if (m_CreationData.m_Fuel > 0)
//			{
//				UserInput();
//				bool on_key_turing = false;
//				float steer_target = 0f;
//				// Motor and Brake
//				if (m_ForwardInput)
//				{
//					foreach (VCPShipPropellerFunc propeller in m_Propellers)
//						propeller.DrivingForward();
//				}
//				else if (m_BackwardInput)
//				{
//					foreach (VCPShipPropellerFunc propeller in m_Propellers)
//						propeller.DrivingBackward();
//				}
//				else
//				{
//					foreach (VCPShipPropellerFunc propeller in m_Propellers)
//						propeller.DrivingStay();
//				}

//				// Turning
//				if (m_Cockpit.m_TurningMode == ETurningMode.vcmKeyboardTurning ||
//					m_Cockpit.m_TurningMode == ETurningMode.vcmMixTurning)
//				{
//					if (m_LeftInput && !m_RightInput)
//					{
//						steer_target = -1;
//						foreach (VCPShipPropellerFunc propeller in m_Propellers)
//							propeller.TurnLeft();
//						foreach (VCPShipRudderFunc rudder in m_Rudders)
//							rudder.TurnLeft();
//						on_key_turing = true;
//					}
//					else if (!m_LeftInput && m_RightInput)
//					{
//						steer_target = 1;
//						foreach (VCPShipPropellerFunc propeller in m_Propellers)
//							propeller.TurnRight();
//						foreach (VCPShipRudderFunc rudder in m_Rudders)
//							rudder.TurnRight();
//						on_key_turing = true;
//					}
//					else
//					{
//						steer_target = 0;
//						foreach (VCPShipPropellerFunc propeller in m_Propellers)
//							propeller.NotTurning();
//						foreach (VCPShipRudderFunc rudder in m_Rudders)
//							rudder.NotTurning();
//						on_key_turing = false;
//					}
//				}
//				if (m_Cockpit.m_TurningMode == ETurningMode.vcmCameraTurning
//					|| m_Cockpit.m_TurningMode == ETurningMode.vcmMixTurning && !on_key_turing)
//				{
//					if (Camera.main != null)
//					{
//						Vector3 cam_forward = Camera.main.transform.forward;
//						Vector3 vehicle_up = transform.up;
//						Vector3 vehicle_right = transform.right;
//						Vector3 vehicle_forward = transform.forward;
//						cam_forward = (cam_forward - Vector3.Dot(cam_forward, vehicle_up) * vehicle_up).normalized;
//						float steering_value = Vector3.Dot(cam_forward, vehicle_right);
//						if (Vector3.Dot(cam_forward, vehicle_forward) < 0)
//							steering_value = (steering_value > 0) ? (2 - steering_value) : (-2 - steering_value);
//						steering_value *= .5F;

//						steer_target = steering_value;
//						foreach (VCPShipPropellerFunc propeller in m_Propellers)
//							propeller.m_SteeringTarget = steering_value;
//						foreach (VCPShipRudderFunc rudder in m_Rudders)
//							rudder.m_SteeringTarget = steering_value;
//					}
//				}

//				VCPShipCockpitFunc scf =
//				(m_Cockpit as VCPShipCockpitFunc);

//				Vector3 sw_rot = scf.m_Steering.localEulerAngles;
//				if (sw_rot.y > 180.0f)
//					sw_rot.y -= 360.0f;
//				sw_rot = Vector3.Lerp(sw_rot, Vector3.up * steer_target * 25f, 0.12f);
//				scf.m_Steering.localEulerAngles = sw_rot;

//				// Light
//				if (m_LightInput)
//				{
//					foreach (VCPHeadLightFunc light in m_Lights)
//						light.m_IsTurnOn = !light.m_IsTurnOn;
//				}
//				// Jet
//				foreach (VCPJetExhaustFunc jet in m_JetExhausts)
//					jet.m_Jeting = m_JetInput;

//			} // End 'HasFuel'

//			// No Fuel
//			else
//			{
//				foreach (VCPShipPropellerFunc propeller in m_Propellers)
//					propeller.NotDriving();
//				foreach (VCPShipRudderFunc rudder in m_Rudders)
//					rudder.NotTurning();
//				foreach (VCPHeadLightFunc light in m_Lights)
//					light.m_IsTurnOn = false;
//				foreach (VCPJetExhaustFunc jet in m_JetExhausts)
//					jet.m_Jeting = false;
//			}
//		}

//		// Idle
//		protected override void Idle()
//		{
//			base.Idle();
//			foreach (VCPShipPropellerFunc propeller in m_Propellers)
//				propeller.NotDriving();
//			foreach (VCPShipRudderFunc rudder in m_Rudders)
//				rudder.NotTurning();
//			foreach (VCPHeadLightFunc light in m_Lights)
//				light.m_IsTurnOn = false;
//			foreach (VCPJetExhaustFunc jet in m_JetExhausts)
//				jet.m_Jeting = false;
//		}

//		// Switches

//		public bool m_ShowDebugInfo = false;
//		void OnGUI()
//		{
//			if (m_ShowDebugInfo)
//			{
//				GUI.color = Color.black;
//				GUI.Label(new Rect(50, 50, 500, 20), "Speed: " + m_Rigidbody.velocity.magnitude.ToString("#,##0.00") + " m/s    " + (m_Rigidbody.velocity.magnitude * 3.6f).ToString("#,##0.00") + " km/h");
//				GUI.Label(new Rect(50, 80, 500, 20), "Fuel: " + m_CreationData.m_Fuel.ToString("#,##0"));
//			}
//		}
//	}*/
//}