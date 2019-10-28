using UnityEngine;
using System;
using System.Collections.Generic;
using SkillSystem;

namespace Pathea
{
	public class Motion_Equip : PeCmpt, IPeMsg
	{
		public const string GlovesPrefabPath = "Prefab/Item/Equip/Weapon/Other/Gloves";
		// Sword
		[HideInInspector][SerializeField]
		Action_HandChangeEquipHold m_HandChangeHold;
		[HideInInspector][SerializeField]
		Action_HandChangeEquipPutOff m_HandChangePutOff;
		[SerializeField]
		Action_SwordAttack 	m_SwordAttack;

		// TwoHandWeapon
		[HideInInspector][SerializeField]
		Action_TwoHandWeaponHold m_TwoHandWeaponHold;
		[HideInInspector][SerializeField]
		Action_TwoHandWeaponPutOff m_TwoHandWeaponPutOff;
		[HideInInspector][SerializeField]
		Action_TwoHandWeaponAttack m_TwoHandWeaponAttack;

		// Sheild
		[HideInInspector][SerializeField]
		Action_SheildHold 	m_SheildHold;

		// Gun
		[HideInInspector][SerializeField]
		Action_GunHold 		m_GunHold;
		[HideInInspector][SerializeField]
		Action_GunPutOff 	m_GunPutOff;
		[SerializeField]
		Action_GunFire 		m_GunFire;
		[SerializeField]
		Action_GunReload 	m_GunReload;
		[SerializeField]
		Action_GunMelee		m_GunMelee;

		//Bow
		[SerializeField]
		 Action_BowHold		m_BowHold;
		[HideInInspector][SerializeField]
		Action_BowPutOff	m_BowPutOff;
		[SerializeField]
		Action_BowShoot		m_BowShoot;
		[SerializeField]
		Action_BowReload	m_BowReload;

		//Tool
		[SerializeField]
		Action_AimEquipHold		m_AimEquipHold;
		[SerializeField]
		Action_AimEquipPutOff	m_AimEquipPutOff;
		[SerializeField]
		Action_DigTerrain		m_DigTerrain;
		[SerializeField]
		Action_Fell				m_Fell;

//		[SerializeField]
//		Action_ChainSawActive	m_ChainSawActive;
//		[SerializeField]
//		Action_ChainSawDeactive	m_ChainSawDeactive;
//		[SerializeField]
//		Action_ChainSawAttack	m_ChainSawAttack;
//		[SerializeField]
//		Action_ChainSawFell		m_ChainSawFell;
		[SerializeField]
		Action_JetPack			m_JetPackAction;
		[SerializeField]
		Action_Parachute		m_ParachuteAction;
		[SerializeField]
		Action_Glider			m_GliderAction;
		[SerializeField]
		Action_DrawWater		m_DrawWater;
		[SerializeField]
		Action_PumpWater		m_PumpWater;
		[SerializeField]
		Action_Throw			m_ThrowGrenade;
		[SerializeField]
		Action_FlashLight		m_FlashLightAction;
		[SerializeField]
		Action_RopeGunShoot		m_RopeGunAction;

		MotionMgrCmpt		m_MotionMgr;
		public MotionMgrCmpt motionMgr { get { return m_MotionMgr; } }

		PeSword				m_Sword;
		PEAxe				m_Axe;
		PETwoHandWeapon 	m_TwoHandWeapon;
		PESheild			m_Sheild;
		PEEnergySheildLogic	m_EnergySheild;
		PEGun 				m_Gun;
		PEBow				m_Bow;
		PEDigTool			m_DigTool;
		PEParachute			m_Parachute;
		PEGlider			m_Glider;
		PEGloves			m_Gloves;
		PEWaterPitcher		m_WaterPitcher;
		
		public PEEnergySheildLogic energySheild { get { return m_EnergySheild; } }
		public PEGun gun { get { return m_Gun; } }

		public PEBow bow { get { return m_Bow; } }

		public PESheild sheild { get { return m_Sheild; } }
		public PEAxe axe { get { return m_Axe; } }
		public PEDigTool digTool{ get {return m_DigTool;} }
		public PEGloves gloves {get {return m_Gloves;}}

		public ItemAsset.ItemObject PEHoldAbleEqObj {get {return (null != m_ActiveableEquipment)?m_ActiveableEquipment.m_ItemObj:null;}}

		public PEHoldAbleEquipment ActiveableEquipment {get {return m_ActiveableEquipment;}}

		// Request cmpt
		IKAimCtrl			m_IKAimCtrl;
		PeTrans 			m_Trans;
		SkAliveEntity		m_Skill;
		
		BiologyViewCmpt		m_View;
		EquipmentCmpt		m_EquipCmpt;
		PackageCmpt			m_Package;
		NpcCmpt				m_NPC;
		AnimatorCmpt		m_Anim;

		PEHoldAbleEquipment m_ActiveableEquipment;

		int m_WeaponID = -1;

         IWeapon m_Weapon;

        public IWeapon Weapon
        {
            get 
            {
                if (m_Weapon != null && !m_Weapon.Equals(null))
				{
					PEHoldAbleEquipment peEquipment = m_Weapon as PEHoldAbleEquipment;
					if(null != peEquipment && !motionMgr.IsActionRunning(peEquipment.m_HandChangeAttr.m_ActiveActionType))
					{
						m_Weapon = null;
						m_WeaponID = -1;
					}
                    return m_Weapon;
				}

                return null;
            }
        }

		public float jetPackEnCurrent
		{
			get
			{
				if(null != m_JetPackAction.jetPackLogic)
					return m_JetPackAction.jetPackLogic.enCurrent;
				return 0;
			}
		}

		public float jetPackEnMax
		{
			get
			{
				if(null != m_JetPackAction.jetPackLogic)
					return m_JetPackAction.jetPackLogic.enMax;
				return 0;
			}
		}

		bool m_SwitchWeapon;
		bool m_PutOnNewWeapon;
		PEHoldAbleEquipment	m_OldWeapon;
		PEHoldAbleEquipment m_NewWeapon;

		HeavyEquipmentCtrl m_HeavyEquipmentCtrl = new HeavyEquipmentCtrl();
		List<IRechargeableEquipment> m_RechangeableEquipments = new List<IRechargeableEquipment>();
		Dictionary<Type, Action<PEEquipment>> m_SetEquipmentFunc = new Dictionary<Type, Action<PEEquipment>>();

		float m_CheckIgnorCostTime;
		
		bool isMainPlayer { get { return MainPlayer.Instance.entity == Entity ; } }

		List<IWeapon> retList = new List<IWeapon>();

		public ItemAsset.EquipType EquipedWeaponType
		{
			get
			{
				if(null != m_Sword)
					return m_Sword.equipType;
				else if(null != m_Gun)
					return m_Gun.equipType;
				else if(null != m_Bow)
					return m_Bow.equipType;
				return ItemAsset.EquipType.Null;
			}
		}

		void InitAction()
		{			
			m_Trans = Entity.peTrans;
			m_Skill = Entity.aliveEntity;
			m_Skill.onSheildReduce += OnSheildReduce;
			m_View = Entity.biologyViewCmpt;
			m_EquipCmpt = Entity.equipmentCmpt;
			m_Package = Entity.packageCmpt;
			m_NPC = Entity.NpcCmpt;
			m_Anim = Entity.animCmpt;
			m_MotionMgr = Entity.motionMgr;
			Invoke("CheckGloves", 0.5f);

			m_HeavyEquipmentCtrl.moveCmpt = Entity.motionMove as Motion_Move_Human;
			m_HeavyEquipmentCtrl.ikCmpt = Entity.IKCmpt;
			m_HeavyEquipmentCtrl.motionMgr = m_MotionMgr;

//			m_ChainSawActive.anim = anim;
			m_SwordAttack.m_UseStamina = isMainPlayer;
			m_TwoHandWeaponAttack.m_UseStamina = isMainPlayer;
			//Gun
			m_GunFire.m_gunHold = m_GunHold;

			m_HandChangeHold.onActiveEvt += OnActiveEquipment;
			m_HandChangeHold.onDeactiveEvt += OnDeactiveEquipment;

			m_TwoHandWeaponHold.onActiveEvt += OnActiveEquipment;
			m_TwoHandWeaponHold.onDeactiveEvt += OnDeactiveEquipment;
			
			m_GunHold.onActiveEvt += OnActiveEquipment;
			m_GunHold.onDeactiveEvt += OnDeactiveEquipment;
			
			m_BowHold.onActiveEvt += OnActiveEquipment;
			m_BowHold.onDeactiveEvt += OnDeactiveEquipment;
			
			m_AimEquipHold.onActiveEvt += OnActiveEquipment;
			m_AimEquipHold.onDeactiveEvt += OnDeactiveEquipment;

			if(null != m_MotionMgr)
			{
				m_MotionMgr.onActionEnd += OnActionEnd;
				m_MotionMgr.AddAction(m_HandChangeHold);
				m_MotionMgr.AddAction(m_HandChangePutOff);
				m_MotionMgr.AddAction(m_SwordAttack);
				m_MotionMgr.AddAction(m_TwoHandWeaponHold);
				m_MotionMgr.AddAction(m_TwoHandWeaponPutOff);
				m_MotionMgr.AddAction(m_TwoHandWeaponAttack);
				m_MotionMgr.AddAction(m_SheildHold);
				m_MotionMgr.AddAction(m_GunHold);
				m_MotionMgr.AddAction(m_GunPutOff);
				m_MotionMgr.AddAction(m_GunFire);
				m_MotionMgr.AddAction(m_GunReload);
				m_MotionMgr.AddAction(m_GunMelee);
				m_MotionMgr.AddAction(m_BowHold);
				m_MotionMgr.AddAction(m_BowPutOff);
				m_MotionMgr.AddAction(m_BowShoot);
				m_MotionMgr.AddAction(m_BowReload);
				m_MotionMgr.AddAction(m_AimEquipHold);
				m_MotionMgr.AddAction(m_AimEquipPutOff);
				m_MotionMgr.AddAction(m_DigTerrain);
				m_MotionMgr.AddAction(m_Fell);
				m_MotionMgr.AddAction(m_JetPackAction);
				m_MotionMgr.AddAction(m_ParachuteAction);
				m_MotionMgr.AddAction(m_GliderAction);
				m_MotionMgr.AddAction(m_DrawWater);
				m_MotionMgr.AddAction(m_PumpWater);
				m_MotionMgr.AddAction(m_ThrowGrenade);
				m_MotionMgr.AddAction(m_FlashLightAction);
				m_MotionMgr.AddAction(m_RopeGunAction);
			}
		}

		void InitEquipment()
		{
			m_SetEquipmentFunc[typeof(PeSword)] = SetSword;
			m_SetEquipmentFunc[typeof(PETorch)] = SetSword;
			m_SetEquipmentFunc[typeof(PETwoHandWeapon)] = SetTwoHandWeapon;
			m_SetEquipmentFunc[typeof(PEAxe)] = SetAxe;
			m_SetEquipmentFunc[typeof(PEChainSaw)] = SetChainSaw;
			m_SetEquipmentFunc[typeof(PEJetPack)] = SetJetPack;
			m_SetEquipmentFunc[typeof(PEParachute)] = SetParachute;
			m_SetEquipmentFunc[typeof(PEGlider)] = SetGlider;
			m_SetEquipmentFunc[typeof(PEWaterPitcher)] = SetWaterPitcher;
			m_SetEquipmentFunc[typeof(PEWaterPump)] = SetWaterPump;
			m_SetEquipmentFunc[typeof(PESheild)] = SetSheild;
			m_SetEquipmentFunc[typeof(PEGun)] = SetGun;
			m_SetEquipmentFunc[typeof(PEPujaGun)] = SetGun;
			m_SetEquipmentFunc[typeof(PEConversionGun)] = SetGun;
			m_SetEquipmentFunc[typeof(PEBow)] = SetBow;
			m_SetEquipmentFunc[typeof(PEDigTool)] = SetDigTool;
			m_SetEquipmentFunc[typeof(PEGrenade)] = SetGrenade;
			m_SetEquipmentFunc[typeof(PEFlashLight)] = SetFlashLight;
			m_SetEquipmentFunc[typeof(PECrusher)] = SetCrusher;
			m_SetEquipmentFunc[typeof(PERopeGun)] = SetRopeGun;
		}

		public override void Start ()
		{
			base.Start();
			InitAction();
			InitEquipment();
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			UpdateHeavyEquipment();
			UpdateEnergeRecharge();
			UpdateAutoUseEquipment();
			UpdateSwitchWeapon();
			UpdateItemCostState();
		}

		void UpdateEnergeRecharge()
		{
			if(null != m_Skill && m_Skill.GetAttribute(AttribType.Energy) > PETools.PEMath.Epsilon)
			{
				for(int i = m_RechangeableEquipments.Count - 1; i >= 0; i--)
				{
					IRechargeableEquipment rechargeable = m_RechangeableEquipments[i];
					if(null != rechargeable && !rechargeable.Equals(null))
					{
						if(Time.time > rechargeable.lastUsedTime + rechargeable.rechargeDelay)
						{
							if(rechargeable.enMax - rechargeable.enCurrent <= PETools.PEMath.Epsilon)
								continue;
							float currentEnerge = m_Skill.GetAttribute(AttribType.Energy);
							float rechargeEnerge = Mathf.Clamp(rechargeable.rechargeSpeed * Time.deltaTime,
							                                   0, rechargeable.enMax - rechargeable.enCurrent);
							if(rechargeEnerge > currentEnerge)
								rechargeEnerge = currentEnerge;
							rechargeable.enCurrent += rechargeEnerge;
							m_Skill.SetAttribute(AttribType.Energy, currentEnerge - rechargeEnerge);
						}
					}
					else
						m_RechangeableEquipments.RemoveAt(i);
				}
			}
		}

		void UpdateHeavyEquipment()
		{
			m_HeavyEquipmentCtrl.Update();
		}

		void UpdateAutoUseEquipment()
		{
			if(null != m_Parachute)
				m_MotionMgr.DoAction(PEActionType.Parachute);
			
			if(null != m_Glider)
				m_MotionMgr.DoAction(PEActionType.Glider);

			if(null != m_DigTool)
				m_DigTerrain.UpdateDigPos();
		}

		void UpdateSwitchWeapon()
		{
			if(m_SwitchWeapon)
			{
				if(null == m_OldWeapon || m_OldWeapon.Equals(null) || null == m_NewWeapon || m_NewWeapon.Equals(null))
				{
					m_SwitchWeapon = false;
					return;
				}

				if(m_PutOnNewWeapon && m_MotionMgr.IsActionRunning(m_NewWeapon.m_HandChangeAttr.m_ActiveActionType))
				{
					m_SwitchWeapon = false;
					return;
				}
				else
				{
					if(m_MotionMgr.IsActionRunning(m_OldWeapon.m_HandChangeAttr.m_ActiveActionType))
					{
						ActiveEquipment(m_OldWeapon, false);
					}
					else if(!m_MotionMgr.IsActionRunning(m_NewWeapon.m_HandChangeAttr.m_ActiveActionType))
					{
						m_PutOnNewWeapon = true;
						ActiveEquipment(m_NewWeapon, true);
					}
				}
			}
		}

		void UpdateItemCostState()
		{
			if(!isMainPlayer)
			{
				if(Entity.proto == EEntityProto.Monster)
				{
					m_GunFire.m_IgnoreItem = true;
					m_GunReload.m_IgnoreItem = true;
					m_BowShoot.m_IgnoreItem = true;
					m_BowHold.m_IgnoreItem = true;
					m_BowReload.m_IgnoreItem = true;
				}
				else if(null != m_NPC)
				{
					bool npcHasConsume = m_NPC.HasConsume;
					m_GunFire.m_IgnoreItem = !npcHasConsume;
					m_GunReload.m_IgnoreItem = !npcHasConsume;
					m_BowShoot.m_IgnoreItem = !npcHasConsume;
					m_BowHold.m_IgnoreItem = !npcHasConsume;
					m_BowReload.m_IgnoreItem = !npcHasConsume;
				}
			}
		}
		
		#region IPeMsg implementation
		
		public void OnMsg (EMsg msg, params object[] args)
		{
			switch(msg)
			{
			case EMsg.Skill_CheckLoop:
				SkFuncInOutPara funcInOut = args[0] as SkFuncInOutPara;
				if(null != funcInOut)
				{
					if(null != funcInOut._para)
					{
						string param = funcInOut._para as string;
						if(null != param && param == "SwordAttack")
						{
							if(null != m_TwoHandWeaponAttack.sword)
								funcInOut._ret = m_TwoHandWeaponAttack.CheckContinueAttack();
							else if(null != m_SwordAttack.sword)
								funcInOut._ret = m_SwordAttack.CheckContinueAttack();
						}
					}
				}
				break;
            case EMsg.View_Prefab_Build:
			BiologyViewCmpt obj = args[0] as BiologyViewCmpt;
				HumanPhyCtrl phyCtrl = obj.monoPhyCtrl;
				m_SwordAttack.phyMotor = phyCtrl;
				m_TwoHandWeaponAttack.phyMotor = phyCtrl;
				m_JetPackAction.m_PhyCtrl = phyCtrl;
				m_ParachuteAction.m_PhyCtrl = phyCtrl;
				m_GliderAction.m_PhyCtrl = phyCtrl;
				m_HeavyEquipmentCtrl.phyCtrl = phyCtrl;
				m_RopeGunAction.phyCtrl = phyCtrl;
				m_IKAimCtrl = obj.monoIKAimCtrl;
				m_GunFire.ikAim = m_IKAimCtrl;
				m_BowShoot.ikAim = m_IKAimCtrl;
				CheckGloves();
				Invoke("ResetWeapon", 0.5f);
				break;
			case EMsg.View_Prefab_Destroy:
				DeletGloves();
				break;
			case EMsg.Battle_OnShoot:
				if(null != m_Gun)
					m_GunHold.OnFire();
				if(null != m_Bow)
					m_BowHold.OnFire();
				break;
			case EMsg.View_FirstPerson:
				m_SwordAttack.firstPersonAttack = (bool)args[0];
				m_TwoHandWeaponAttack.firstPersonAttack = (bool)args[0];
				break;
			}
		}
		
		#endregion

        public bool IsSwitchWeapon()
        {
            return m_SwitchWeapon;
        }

        public IWeapon GetHoldWeapon()
        {
            if (null != m_EquipCmpt && m_EquipCmpt._Weapons != null)
                return m_EquipCmpt._Weapons.Find(ret => ret != null && !ret.Equals(null) && ret.HoldReady);

            return null;
        }

		public List<IWeapon> GetWeaponList()
		{
			retList.Clear ();
			if (null != m_EquipCmpt)
				retList.AddRange (m_EquipCmpt._Weapons);


			bool useGloves = null == m_HeavyEquipmentCtrl.heavyEquipment;
			if(useGloves)
			{
				for (int i = 0; i < retList.Count; ++i)
				{
					if (retList [i] is PeSword)
					{
						useGloves = false;
						break;
					}
				}
			}
			if(null != m_Gloves && useGloves)
				retList.Add(m_Gloves);

	
			return retList;
		}

		public List<IWeapon> GetCanUseWeaponList(PeEntity entity)
		{
			retList.Clear ();
			if (null != m_EquipCmpt)
				retList.AddRange (m_EquipCmpt._Weapons);

			for(int i=0;i<retList.Count;i++)
			{
				if(PETools.PE.WeaponCanCombat(entity,retList[i]))
					return retList;
			}

			//no can combat Weapon
			retList.Clear ();
			bool useGloves = null == m_HeavyEquipmentCtrl.heavyEquipment;
			if(useGloves)
			{
				for (int i = 0; i < retList.Count; ++i)
				{
					if (retList [i] is PeSword)
					{
						useGloves = false;
						break;
					}
				}
			}
			if(null != m_Gloves && useGloves)
				retList.Add(m_Gloves);
		
			return retList;
		}


		public void UpdateMoveDir(Vector3 moveDir, Vector3 localDir)
		{
			m_ParachuteAction.SetMoveDir(moveDir.normalized);
			m_GliderAction.SetMoveDir(localDir.normalized);
			m_JetPackAction.SetMoveDir(moveDir);
		}

		public void SetEquipment(PEEquipment equipment, bool isPutOn)
		{
			ActiveGloves (false, true);

			Type equipmentType = equipment.GetType();
			foreach(Type type in m_SetEquipmentFunc.Keys)
			{
				if(type == equipmentType)
				{
					if(isPutOn)
						m_SetEquipmentFunc[type](equipment);
					else
						m_SetEquipmentFunc[type](null);
				}
			}

			ResetAccuracy(equipment);

			if(null != m_Anim && equipment.m_EquipAnim != "")
				m_Anim.SetBool(equipment.m_EquipAnim, isPutOn);

			CheckGloves();
		}

		// reset aimequipment's accuracy .UseNpcdata later
		float accuracyScale = 0.5f;
		float accuracyRangeAdd = 3f;

		void ResetAccuracy(PEEquipment equipment)
		{
			if(isMainPlayer || Entity.proto == EEntityProto.Monster)
				return;

			PEAimAbleEquip aimAbleEquipment = equipment as PEAimAbleEquip;
			if(null != aimAbleEquipment)
			{
				aimAbleEquipment.m_AimAttr.m_FireStability *= accuracyScale;
				aimAbleEquipment.m_AimAttr.m_AccuracyMin += accuracyRangeAdd;
				aimAbleEquipment.m_AimAttr.m_AccuracyMax += accuracyRangeAdd;
				aimAbleEquipment.m_AimAttr.m_AccuracyPeriod *= 1f / accuracyScale;
				aimAbleEquipment.m_AimAttr.m_AccuracyDiffusionRate += 2f;
				aimAbleEquipment.m_AimAttr.m_AccuracyShrinkSpeed *= accuracyScale;
			}
		}

		public void SetWeapon(PEEquipment equip)
		{
			m_Weapon = equip as IWeapon;
			m_WeaponID = (m_Weapon != null && m_Weapon.ItemObj != null ? m_Weapon.ItemObj.instanceId : -1);
		}

		void ResetWeapon()
		{
			if (-1 != m_WeaponID && null != m_ActiveableEquipment && null != m_ActiveableEquipment.m_ItemObj && m_ActiveableEquipment.m_ItemObj.instanceId == m_WeaponID)
				m_Weapon = m_ActiveableEquipment as IWeapon;
			else if (null != m_Gloves && null != m_MotionMgr && m_MotionMgr.IsActionRunning (m_Gloves.m_HandChangeAttr.m_ActiveActionType))
				m_Weapon = m_Gloves as IWeapon;
		}
		
		void SetSword(PEEquipment sword)
		{
			if(null != m_Sword && null != sword)
			{
				m_HandChangeHold.handChangeEquipment = null;
				m_SwordAttack.sword = null;
				m_ActiveableEquipment = null;
			}
			m_Sword = sword as PeSword;
			m_HandChangeHold.handChangeEquipment = m_Sword;
			m_SwordAttack.sword = m_Sword;
			m_ActiveableEquipment = m_Sword;
		}
		
		void SetTwoHandWeapon(PEEquipment weapon)
		{
			m_TwoHandWeapon = weapon as PETwoHandWeapon;
			m_TwoHandWeaponHold.twoHandWeapon = m_TwoHandWeapon;
			m_TwoHandWeaponAttack.sword = m_TwoHandWeapon;
			m_ActiveableEquipment = m_TwoHandWeapon;
		}

		void SetAxe(PEEquipment equipment)
		{
			m_Axe = equipment as PEAxe;
			m_Fell.m_Axe = m_Axe;
			m_HandChangeHold.handChangeEquipment = m_Axe;
			m_ActiveableEquipment = m_Axe;
		}

		void SetChainSaw(PEEquipment chainSaw)
		{
			SetAxe(chainSaw);
			m_HeavyEquipmentCtrl.heavyEquipment = chainSaw as IHeavyEquipment;
		}

		void SetJetPack(PEEquipment jetPack)
		{
			m_JetPackAction.jetPack = jetPack as PEJetPack;
		}

		public void SetJetPackLogic(PEJetPackLogic jetPackLogic)
		{
			m_JetPackAction.jetPackLogic = jetPackLogic;
			if(null != jetPackLogic)
				m_RechangeableEquipments.Add(jetPackLogic);
		}

		void SetParachute(PEEquipment parachute)
		{
			m_Parachute = parachute as PEParachute;
			m_ParachuteAction.parachute = m_Parachute;
		}

		void SetGlider(PEEquipment glider)
		{
			m_Glider = glider as PEGlider;
			m_GliderAction.glider = m_Glider;
		}

		void SetWaterPitcher(PEEquipment waterPitcher)
		{
			m_WaterPitcher = waterPitcher as PEWaterPitcher;
			m_DrawWater.waterPitcher = m_WaterPitcher;
		}

		void SetWaterPump(PEEquipment equipment)
		{
			PEWaterPump waterPump = equipment as PEWaterPump;
			m_GunHold.aimAbleEquip = waterPump;
			m_PumpWater.waterPump = waterPump;
			m_ActiveableEquipment = waterPump;
		}
		
		void SetSheild(PEEquipment sheild)
		{
			m_Sheild = sheild as PESheild;
			m_SheildHold.sheild = m_Sheild;
		}

		public void SetEnergySheild(PEEnergySheildLogic energySheild)
		{
			m_EnergySheild = energySheild;
			if(null != m_EnergySheild)
				m_RechangeableEquipments.Add(m_EnergySheild);
		}
		
		void SetGun(PEEquipment gun)
		{
			m_Gun = gun as PEGun;
			m_GunFire.gun = m_Gun;
			m_GunHold.aimAbleEquip = m_Gun;
			m_GunReload.gun = m_Gun;
			m_GunMelee.gun = m_Gun;
			m_ActiveableEquipment = m_Gun;
		}
		
		public void SetEnergyGunLogic(PEEnergyGunLogic gun)
		{
			m_GunFire.energyGunLogic = gun;
			if(null != gun)
				m_RechangeableEquipments.Add(gun);
		}

		void SetBow(PEEquipment bow)
		{
			m_Bow = bow as PEBow;
			m_BowHold.bow = m_Bow;
			m_BowReload.bow = m_Bow;
			m_BowShoot.bow = m_Bow;
			m_ActiveableEquipment = m_Bow;
		}

		void SetDigTool(PEEquipment digTool)
		{
			m_DigTool = digTool as PEDigTool;
			m_HandChangeHold.handChangeEquipment = m_DigTool;
			m_DigTerrain.digTool = m_DigTool;
			m_ActiveableEquipment = m_DigTool;
		}

		void SetGrenade(PEEquipment equipment)
		{
			PEGrenade grenade = equipment as PEGrenade;
			m_GunHold.aimAbleEquip = grenade;
			m_ThrowGrenade.grenade = grenade;
			m_ActiveableEquipment = grenade;
		}
		
		void SetFlashLight(PEEquipment flashLight)
		{
			m_FlashLightAction.flashLight = flashLight as PEFlashLight;
		}

		void SetCrusher(PEEquipment crusher)
		{
			m_DigTool = crusher as PEDigTool;
			m_AimEquipHold.aimAbleEquip = m_DigTool;
			m_DigTerrain.digTool = m_DigTool;
			m_ActiveableEquipment = m_DigTool;
			m_HeavyEquipmentCtrl.heavyEquipment = m_DigTool as IHeavyEquipment;
		}

		void SetRopeGun(PEEquipment ropeGun)
		{
			m_RopeGunAction.ropeGun = ropeGun as PERopeGun;
			m_GunHold.aimAbleEquip = m_RopeGunAction.ropeGun;
			m_ActiveableEquipment = m_RopeGunAction.ropeGun;
		}

		public bool WeaponCanUse(IWeapon weapon)
		{
			if(!isMainPlayer && null != m_NPC && !m_NPC.HasConsume)
				return true;
			PeSword sword = weapon as PeSword;
			if(null != sword)
				return true;
			PEGun gun = weapon as PEGun;
			if(null != gun)
			{
				if(m_GunFire.m_IgnoreItem) return true;
				if(gun.m_AmmoType == AmmoType.Bullet)
					return gun.durability > PETools.PEMath.Epsilon 
						&& (gun.magazineValue > PETools.PEMath.Epsilon || null == m_Package || m_Package.GetItemCount(gun.curItemID) > 0);

				return gun.durability > PETools.PEMath.Epsilon 
					&& (gun.magazineValue > PETools.PEMath.Epsilon || Entity.GetAttribute(AttribType.Energy) > PETools.PEMath.Epsilon);
			}
			PEBow bow = weapon as PEBow;
			if(null != bow)
			{
				if(m_BowShoot.m_IgnoreItem) return true;
				return bow.durability > PETools.PEMath.Epsilon && null == m_Package || m_Package.GetItemCount(bow.curItemID) > 0;
			}

			return true;
		}
	

		bool EquipmentCanUse()
		{
			if(null != m_ActiveableEquipment)
			{
				if(m_ActiveableEquipment is IWeapon)
					return WeaponCanUse(m_ActiveableEquipment as IWeapon);
				return m_ActiveableEquipment.durability > PETools.PEMath.Epsilon;
			}
			return false;
		}

		public bool CheckEquipmentDurability()
		{
			if(null != m_ActiveableEquipment)
			{
				return m_ActiveableEquipment.durability > PETools.PEMath.Epsilon;
			}
			return false;
		}
		 

		public bool CheckEquipmentAmmunition()
		{
			if(!(m_ActiveableEquipment is IWeapon))
				return true;

			IWeapon weapon = m_ActiveableEquipment as IWeapon;
			PEGun gun = weapon as PEGun;
			if(null != gun)
			{
				if(gun.m_AmmoType == AmmoType.Bullet)
					return null != m_Package && m_Package.GetItemCount(gun.curItemID) > 0;
				
				return gun.magazineValue > PETools.PEMath.Epsilon || Entity.GetAttribute(AttribType.Energy) > PETools.PEMath.Epsilon;
			}
			PEBow bow = weapon as PEBow;
			if(null != bow)
				return  null != m_Package && m_Package.GetItemCount(bow.curItemID) > 0;

			return true;
		}


		public bool IsWeaponActive()
		{
			if(null != m_ActiveableEquipment && m_ActiveableEquipment is IWeapon)
				return m_MotionMgr.IsActionRunning(m_ActiveableEquipment.m_HandChangeAttr.m_ActiveActionType);
			return false;
		}

		public void SetTarget(SkEntity skEntity)
		{
			m_SwordAttack.targetEntity = skEntity;
			m_TwoHandWeaponAttack.targetEntity = skEntity;
			m_BowShoot.targetEntity = skEntity;
			m_ThrowGrenade.targetEntity = skEntity;
			m_GunFire.targetEntity = skEntity;
			m_GunMelee.targetEntity = skEntity;
		}

		public void SwordAttack(Vector3 dir, int attackModeIndex = 0, int time = 0)
		{
			if(null != m_SwordAttack.sword 
			   && m_SwordAttack.sword == m_HandChangeHold.handChangeEquipment
			   && m_TwoHandWeapon == null)
			{
				PEActionParamVVNN param = PEActionParamVVNN.param;
				param.vec1 = m_Trans.position;
				param.vec2 = dir;
				param.n1 = time;
				param.n2 = attackModeIndex;
				m_MotionMgr.DoAction(PEActionType.SwordAttack, param);
			}
		}

		/// <summary>
		/// Twos the hand weapon attack.
		/// </summary>
		/// <param name="dir">Dir.</param>
		/// <param name="handType">Hand type 0:TwoHand 1:RightHand 2:LeftHand</param>
		/// <param name="time">Time.</param>

		public void TwoHandWeaponAttack(Vector3 dir, int attackModeIndex = 0, int time = 0)
		{
			if(null != m_TwoHandWeapon)
			{
				PEActionParamVVNN param = PEActionParamVVNN.param;
				param.vec1 = m_Trans.position;
				param.vec2 = dir;
				param.n1 = time;
				param.n2 = attackModeIndex;
				m_MotionMgr.DoAction(PEActionType.TwoHandSwordAttack, param);
			}
  		}

		public event System.Action OnActiveWeapon;
		public event System.Action OnDeactiveWeapon;
		public void ActiveWeapon(bool active)
		{
			if(null != m_WaterPitcher)
				return;
			if(null != m_ActiveableEquipment)
			{
				if(EquipmentCanUse())
				{
					if(m_HandChangeHold.handChangeEquipment == m_Gloves 
					   && motionMgr.IsActionRunning(PEActionType.EquipmentHold))
					{
						motionMgr.EndImmediately(PEActionType.SwordAttack);
						motionMgr.EndImmediately(PEActionType.EquipmentHold);
					}
					else
						ActiveEquipment(m_ActiveableEquipment, active);
				}
				else
				{
					if(m_ActiveableEquipment != m_Axe || m_HandChangeHold.handChangeEquipment == m_Axe)
						ActiveEquipment(m_ActiveableEquipment, false);
					ActiveGloves(active);
				}
			}
			else
				ActiveGloves(active);
		}

		public bool ISAimWeapon
		{
			get
			{
				if (null != m_ActiveableEquipment && m_ActiveableEquipment is PEAimAbleEquip)
					return m_ActiveableEquipment.m_HandChangeAttr.m_CamMode.camModeIndex3rd == 1;
				return false;
			}
		}

		void ActiveGloves(bool active, bool immediately = false)
		{
			if(null != m_Gloves)
			{
				if(active)
				{
					if(m_HandChangeHold.handChangeEquipment != m_Gloves)
					{
						m_HandChangeHold.handChangeEquipment = null;
						m_HandChangeHold.handChangeEquipment = m_Gloves;
					}
					if(m_SwordAttack.sword != m_Gloves)
					{
						m_SwordAttack.sword = null;
						m_SwordAttack.sword = m_Gloves;
					}
				}
				else
				{
					if(m_HandChangeHold.handChangeEquipment == m_Gloves && null != m_ActiveableEquipment
					   && m_ActiveableEquipment.m_HandChangeAttr.m_ActiveActionType == PEActionType.EquipmentHold)
					{
						m_HandChangeHold.handChangeEquipment = null;					
						m_HandChangeHold.handChangeEquipment = m_ActiveableEquipment;
					}

					if(m_SwordAttack.sword == m_Gloves)
						m_SwordAttack.sword = null;
					if(null != m_Sword)
						m_SwordAttack.sword = m_Sword;
				}
				ActiveEquipment(m_Gloves, active, immediately);
			}
		}

		void ActiveEquipment(PEHoldAbleEquipment equipment, bool active, bool immediately = false)
		{
			if (active)
			{
				if(!m_MotionMgr.IsActionRunning(equipment.m_HandChangeAttr.m_ActiveActionType)
					&& m_MotionMgr.DoAction (equipment.m_HandChangeAttr.m_ActiveActionType))
				{
					m_Weapon = equipment as IWeapon;
					if(null != m_Weapon && null != equipment.m_ItemObj)
						m_WeaponID = equipment.m_ItemObj.instanceId;
					else
						m_WeaponID = -1;
				}
			}
			else
			{
				if((immediately && m_MotionMgr.EndImmediately(equipment.m_HandChangeAttr.m_ActiveActionType))
				   || (m_MotionMgr.IsActionRunning(equipment.m_HandChangeAttr.m_ActiveActionType) && m_MotionMgr.DoAction(equipment.m_HandChangeAttr.m_UnActiveActionType)))
				{
					m_Weapon = null;
					m_WeaponID = -1; 
				}
			}
		}

		public void ActiveWeapon(PEHoldAbleEquipment handChangeEquipment, bool active, bool immediately = false)
		{
			if (null != m_Gloves && handChangeEquipment == m_Gloves as PEHoldAbleEquipment) 
			{
				ActiveGloves (active);
				ActiveEquipment (m_Gloves, active);
			} 
			else 
			{
				if(null != m_Gloves && Weapon == m_Gloves)
					ActiveGloves (false);
				if (null != handChangeEquipment) 
				{
					if (immediately)
					{
						if (active)
						{
							if(!m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType))
							{
								m_MotionMgr.DoActionImmediately (handChangeEquipment.m_HandChangeAttr.m_ActiveActionType);
								m_Weapon = handChangeEquipment as IWeapon;
								if(null != m_Weapon && null != handChangeEquipment.m_ItemObj)
									m_WeaponID = handChangeEquipment.m_ItemObj.instanceId;
								else
									m_WeaponID = -1;
							}
						}
						else
						{
							if(m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType))
							{
								m_MotionMgr.EndImmediately (handChangeEquipment.m_HandChangeAttr.m_ActiveActionType);
								m_WeaponID = -1;
								m_Weapon = null;
							}
						}
					} 
					else 
					{
						if (active)
						{
							if(!m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType)
								&& m_MotionMgr.DoAction (handChangeEquipment.m_HandChangeAttr.m_ActiveActionType))
							{
								m_Weapon = handChangeEquipment as IWeapon;
								if(null != m_Weapon && null != handChangeEquipment.m_ItemObj)
									m_WeaponID = handChangeEquipment.m_ItemObj.instanceId;
								else
									m_WeaponID = -1;
							}
						}
						else
						{
							if(m_MotionMgr.IsActionRunning(handChangeEquipment.m_HandChangeAttr.m_ActiveActionType)
								&& m_MotionMgr.DoAction (handChangeEquipment.m_HandChangeAttr.m_UnActiveActionType))
							{
								m_Weapon = null;
								m_WeaponID = -1;
							}
						}
					}
				}
			}
		}

		public void HoldSheild(bool hold)
		{
			if(null != m_Sheild)
			{
				if(hold)
					m_MotionMgr.DoAction(PEActionType.HoldShield);
				else
					m_MotionMgr.EndImmediately(PEActionType.HoldShield);
			}
		}

		public bool SwitchHoldWeapon(IWeapon oldWeapon, IWeapon newWeapon)
		{
			if (m_SwitchWeapon)
				return false;

			if(null != oldWeapon && !oldWeapon.Equals(null) && null != newWeapon && !newWeapon.Equals(null))
			{
				PEHoldAbleEquipment oldEquip = oldWeapon as PEHoldAbleEquipment;

				if(null != oldEquip && m_MotionMgr.DoAction(oldEquip.m_HandChangeAttr.m_UnActiveActionType))
				{
					m_SwitchWeapon = true;
					m_PutOnNewWeapon = false;
					m_OldWeapon = oldWeapon as PEHoldAbleEquipment;
					m_NewWeapon = newWeapon as PEHoldAbleEquipment;
					return true;
				}
			}
			else
				Debug.LogError("SwitchHoldWeapon is null");

			return false;
		}

		public void Reload()
		{
			if(null != m_Gun)
			{
				PEActionParamN param = PEActionParamN.param;
				param.n = m_Gun.curAmmoItemIndex;
				m_MotionMgr.DoAction(PEActionType.GunReload, param);
			}
		}

		public float GetAimPointScale()
		{
			if(null != m_Gun)
				return m_GunHold.GetAimPointScale();
			else if(null != m_Bow)
				return m_BowHold.GetAimPointScale();
			return 0;
		}

		void CheckGloves()
		{
			if(null == m_Gloves && null != m_View && m_View.hasView)
			{
				if(null != m_View.GetModelTransform("mountMain"))
				{
					UnityEngine.Object res = AssetsLoader.Instance.LoadPrefabImm(GlovesPrefabPath);
					if(null != res)
					{
						GameObject gameObj = Instantiate(res) as GameObject;
						if(null != gameObj)
						{
							m_Gloves = gameObj.GetComponent<PEGloves>();						
							m_Gloves.InitEquipment(Entity, null);
						}
					}
				}
			}
			if(null != m_Gloves && null == m_HeavyEquipmentCtrl.heavyEquipment)
			{
				if(m_HandChangeHold.handChangeEquipment == null)
					m_HandChangeHold.handChangeEquipment = m_Gloves;
				if(m_SwordAttack.sword == null)
					m_SwordAttack.sword = m_Gloves;
				m_MotionMgr.EndImmediately(PEActionType.SwordAttack);
				m_MotionMgr.EndImmediately(PEActionType.Fell);
				m_MotionMgr.EndImmediately(PEActionType.Dig);
				m_MotionMgr.EndImmediately(m_Gloves.m_HandChangeAttr.m_ActiveActionType);
				//					m_ActiveableEquipment = m_Gloves;
			}
		}

		void DeletGloves()
		{
			if(null != m_Gloves)
			{
				if(m_Sword == m_Gloves)
					m_Sword = null;
				if(m_HandChangeHold.handChangeEquipment == m_Gloves)
					m_HandChangeHold.handChangeEquipment = null;
				if(m_SwordAttack.sword == m_Gloves)
					m_SwordAttack.sword = null;
				if(null != m_View)
					m_View.DetachObject(m_Gloves.gameObject);
				GameObject.Destroy(m_Gloves.gameObject);
				m_Gloves = null;
			}
		}

		void OnSheildReduce()
		{
			if(null != m_EnergySheild)
				m_EnergySheild.lastUsedTime = Time.time;
		}

		void OnActiveEquipment()
		{
			if (OnActiveWeapon != null)
				OnActiveWeapon();
		}

		void OnDeactiveEquipment()
		{
			if (OnDeactiveWeapon != null)
				OnDeactiveWeapon();
		}

		void OnActionEnd(PEActionType type)
		{
			if (null != Weapon) 
			{
				PEHoldAbleEquipment equipment = m_Weapon as PEHoldAbleEquipment;
				if(null != equipment && equipment.m_HandChangeAttr.m_ActiveActionType == type)
				{
					m_Weapon = null;
					m_WeaponID = -1;
				}
			}
		}
	}
}