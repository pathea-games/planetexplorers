#define PLANET_EXPLORERS
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace OLD
{
	/*
	public abstract class DrivingController : RigidbodyController
	{
		[Header("Weapon")]
		public bool isShootingMode;
		public Vector3 screenAimPoint = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

		// Parts
		[Header("Original")]
		public VCPCockpitFunc m_Cockpit;
		public List<VCPSideSeatFunc> m_SideSeats;
		public List<VCPJetExhaustFunc> m_JetExhausts;
		public List<VCPCtrlTurretFunc> m_CtrlTurrets;
		public List<VCPFrontCannonFunc> m_FrontCannons;
		public List<VCPMissileLauncherFunc> m_MissileLaunchers;
		public List<VCPAITurretFunc> m_AITurrets;
		public bool HasWeapon { get { return (m_CtrlTurrets.Count + m_FrontCannons.Count + m_MissileLaunchers.Count + m_AITurrets.Count) > 0; } }
		public bool HasControlledWeapon { get { return (m_CtrlTurrets.Count + m_MissileLaunchers.Count) > 0; } }
		private float _JetForceCoef = 1;
		public float JetForceCoef { get { return _JetForceCoef; } }
		public List<VCPMissileLauncherFunc> m_SelectedMissileLaunchers;

		public override void Init(CreationData crd, int itemInstanceId)
		{
			base.Init(crd, itemInstanceId);
			LoadParts(ref m_Cockpit);
			LoadParts(ref m_SideSeats);
			LoadParts(ref m_JetExhausts);
			LoadParts(ref m_CtrlTurrets);
			LoadParts(ref m_FrontCannons);
			LoadParts(ref m_MissileLaunchers);
			LoadParts(ref m_AITurrets);
			foreach (VCPJetExhaustFunc iter in m_JetExhausts)
				iter.Controller = this;
			foreach (VCPCtrlTurretFunc iter in m_CtrlTurrets)
				iter.Controller = this;
			foreach (VCPFrontCannonFunc iter in m_FrontCannons)
				iter.Controller = this;
			foreach (VCPMissileLauncherFunc iter in m_MissileLaunchers)
				iter.Controller = this;
			foreach (VCPAITurretFunc iter in m_AITurrets)
				iter.Controller = this;

			// Calc jet force coef
			if (m_JetExhausts.Count > 0)
			{
				Vector3 sum_force = Vector3.zero;
				foreach (VCPJetExhaustFunc iter in m_JetExhausts)
				{
					sum_force += iter.transform.forward * iter.m_Property.m_MaxForce;
				}
				float sum_weight = sum_force.magnitude / 9.8f;
				float max_weight_allowed = VCEMath.SmoothConstraint(sum_weight, GetComponent<Rigidbody>().mass * 1f, 1);
				_JetForceCoef = max_weight_allowed / sum_weight;
			}
			else
			{
				_JetForceCoef = 1;
			}

			m_SelectedMissileLaunchers = new List<VCPMissileLauncherFunc>();
			foreach (VCPMissileLauncherFunc iter in m_MissileLaunchers)
				m_SelectedMissileLaunchers.Add(iter);


			///////////////////////////////////////


			gameObject.AddComponent<ItemScript_Carrier>();
			gameObject.AddComponent<DragItemMousePickCarrier>();
			gameObject.AddComponent<ItemDraggingCarrier>();
			gameObject.AddComponent<Pathea.PeTrans>();

			var carrierEntity = gameObject.AddComponent<WhiteCat.CarrierEntity>();
			carrierEntity.onHpChange += OnHpChange;


			ItemScript script = GetComponent<ItemScript>();
			if (script != null)
			{
				ItemAsset.ItemObject item = ItemAsset.ItemMgr.Instance.Get(itemInstanceId);
				if (item != null)
				{
					carrierEntity.m_Attrs = new Pathea.PESkEntity.Attr[3];

					carrierEntity.m_Attrs[0] = new Pathea.PESkEntity.Attr();
					carrierEntity.m_Attrs[0].m_Type = Pathea.AttribType.Hp;
					carrierEntity.m_Attrs[0].m_Value = item.GetCmpt<ItemAsset.LifeLimit>().floatValue.current;

					carrierEntity.m_Attrs[1] = new Pathea.PESkEntity.Attr();
					carrierEntity.m_Attrs[1].m_Type = Pathea.AttribType.CampID;
					carrierEntity.m_Attrs[1].m_Value = 1;

					carrierEntity.m_Attrs[2] = new Pathea.PESkEntity.Attr();
					carrierEntity.m_Attrs[2].m_Type = Pathea.AttribType.DefaultPlayerID;
					carrierEntity.m_Attrs[2].m_Value = 1;

					carrierEntity.InitEntity();
				}
			}
		}


		void OnHpChange(SkillSystem.SkEntity skEntity, float deltaHp)
		{
			ItemScript script = GetComponent<ItemScript>();
			if (script != null)
			{
				ItemAsset.ItemObject item = ItemAsset.ItemMgr.Instance.Get(script.itemObjectId);
				if (item != null)
				{
					ItemAsset.LifeLimit life = item.GetCmpt<ItemAsset.LifeLimit>();
					life.floatValue.Change(deltaHp);
				}
			}
		}


		public void ChangeEnergy(float deltaEnergy)
		{
			ItemScript script = GetComponent<ItemScript>();
			if (script != null)
			{
				ItemAsset.ItemObject item = ItemAsset.ItemMgr.Instance.Get(script.itemObjectId);
				if (item != null)
				{
					ItemAsset.Energy energy = item.GetCmpt<ItemAsset.Energy>();
					energy.floatValue.Change(deltaEnergy);
				}
			}
		}


		protected override void Update()
		{
			base.Update();
			if (m_Rigidbody == null) return;
			if (m_Active)
			{
				if (m_NetChar == ENetCharacter.nrOwner)
				{
					if (m_Cockpit.m_IsDriving)
						Driving();
					else
						Idle();
				}
			}
		}
		public bool InWater
		{
			get
			{
#if PLANET_EXPLORERS
				if (m_Cockpit != null)
				{
					Vector3 pos = m_Cockpit.transform.position;
					if (VFVoxelWater.self != null && VFVoxelWater.self.Voxels != null)
					{
						VFVoxel voxel = VFVoxelWater.self.Voxels.SafeRead(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
						if (voxel.Volume > 200)
							return true;
						else
							return false;
					}
					return false;
				}
#endif
				return false;
			}
		}

		//protected virtual void SyncPassengers ()
		//{
		//	if (null != m_Cockpit.m_Passenger)
		//	{
		//		m_Cockpit.m_Passenger.transform.position = m_Cockpit.m_PivotPoint.position;
		//		m_Cockpit.m_Passenger.transform.rotation = m_Cockpit.m_PivotPoint.rotation;
		//	}
		//	foreach (VCPSideSeatFunc sit in m_SideSeats)
		//	{
		//		if (null != sit.m_Passenger && sit.m_CanSync)
		//		{
		//			sit.m_Passenger.transform.position = sit.transform.position;
		//			sit.m_Passenger.transform.rotation = sit.transform.rotation;
		//		}
		//	}
		//}
		protected override void FixedUpdate()
		{
			base.FixedUpdate();
			if (m_Rigidbody == null)
				return;

			// Avoid anything throw it into the air
			Vector3 v = m_Rigidbody.velocity;
			if (v.y > 30)
			{
				v.y = 30;
				m_Rigidbody.velocity = v;
			}
			if (v.x < -100)
			{
				v.x = -100;
				m_Rigidbody.velocity = v;
			}
			if (v.x > 100)
			{
				v.x = 100;
				m_Rigidbody.velocity = v;
			}
			if (v.z < -100)
			{
				v.z = -100;
				m_Rigidbody.velocity = v;
			}
			if (v.z > 100)
			{
				v.z = 100;
				m_Rigidbody.velocity = v;
			}
		}




		const float _maxAimDistance = 250;
		static LayerMask _layerMask = -1;

		Ray _ray;
		RaycastHit _rayHitInfo;

		[System.NonSerialized]
		public GameObject rayHitGameObject;
		[System.NonSerialized]
		public Vector3 rayHitPoint;


		protected virtual void Driving()
		{
			if (Input.GetKeyDown(KeyCode.F)) isShootingMode = !isShootingMode;

			if (isShootingMode)
			{
				if (Physics.Raycast(_ray = Camera.main.ScreenPointToRay(screenAimPoint), out _rayHitInfo, _maxAimDistance, _layerMask))
				{
					rayHitGameObject = _rayHitInfo.transform.gameObject;
					rayHitPoint = _rayHitInfo.point;
				}
				else
				{
					rayHitGameObject = null;
					rayHitPoint = _ray.direction * _maxAimDistance + _ray.origin;
				}
			}
		}


		protected virtual void Idle()
		{
			m_Cockpit.m_ArmMode = false;
			m_IdleTime += Time.deltaTime;
			if (m_IdleTime > m_DeactiveTime)
			{
				m_Active = false;
				m_IdleTime = 0.0f;
			}
		}


		// Switches
		protected float m_IdleTime = 0.0f;
		protected float m_DeactiveTime = 4.0f;
		//public void GetOn (GameObject driver) { m_Active = true; m_Cockpit.m_IsDriving = true; m_IdleTime = 0.0f; m_Cockpit.m_Passenger = driver; }
		//public void GetOn (GameObject passenger, VCPSideSeatFunc seat) { seat.m_Passenger = passenger; }
		//public void GetOff () { QuitArmMode(); m_Cockpit.m_IsDriving = false; m_IdleTime = 0.0f; m_Cockpit.m_Passenger = null; }
		//public void GetOff (VCPSideSeatFunc seat) { seat.m_Passenger = null; }
		public void EnterArmMode() { m_Cockpit.m_ArmMode = true; }
		public void QuitArmMode() { m_Cockpit.m_ArmMode = false; }
	}
	 * */
}