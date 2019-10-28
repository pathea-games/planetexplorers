using UnityEngine;
using System;
using ItemAsset.PackageHelper;
using Pathea.PeEntityExt;

namespace Pathea
{
	[Serializable]
	public class Action_GunHold : Action_AimEquipHold
	{
		public override PEActionType ActionType { get { return PEActionType.GunHold; } }
	}
	
	[Serializable]
	public class Action_GunPutOff : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.GunPutOff; } }
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.EndAction(PEActionType.GunHold);
		}
	}
	
	[Serializable]
	public class Action_GunFire : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.GunFire; } }

		public IKAimCtrl		ikAim { get; set; }
		public SkillSystem.SkEntity targetEntity{ get; set; }

		public Action_GunHold	m_gunHold;

		public float	m_ChargeEffectDelayTime = 0.8f;
		bool 			m_EndFire;
		AudioController m_Audio;
		float			m_HoldFireTime;
		float			m_LastShootTime;

		Vector3			m_IKAimDirWorld;
		Vector3			m_IKAimDirLocal;
		Quaternion 		m_IK;

		PEGun		m_Gun;
		public PEGun gun
		{
			get { return m_Gun; }
			set 
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_Gun = value;
			}
		}

		public PEEnergyGunLogic energyGunLogic;

		bool m_EndAfterShoot;

		public bool   	m_IgnoreItem = false;

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null == skillCmpt || null == gun || null == entity)
				return false;
			if(gun.durability <= PETools.PEMath.Epsilon)
			{
				entity.SendMsg(EMsg.Action_DurabilityDeficiency);
				return false;
			}
			if(m_IgnoreItem)
				return true;
			if(gun.m_AmmoType == AmmoType.Bullet)
				return true;
			else if(gun.magazineValue >= gun.m_EnergyPerShoot)
				return true;
			return false;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == skillCmpt || null == gun)
				return;
			PEActionParamB paramB = para as PEActionParamB;
			m_EndAfterShoot = paramB.b;
			m_EndFire = false;
			m_HoldFireTime = 0;
			switch(gun.m_ShootMode)
			{
			case ShootMode.SingleShoot:
			case ShootMode.MultiShoot:
				if(gun.m_AmmoType == AmmoType.Bullet)
				{
					if(gun.magazineValue > 0)
					{
						if(Time.time - m_LastShootTime > gun.m_FireRate)
							OnFire(1);
					}
					else
					{
                        if (m_IgnoreItem || (null != packageCmpt && packageCmpt.GetItemCount(gun.curItemID) > 0))
                        {
                            PEActionParamN param = PEActionParamN.param;
                            param.n = gun.curAmmoItemIndex;
                            motionMgr.DoAction(PEActionType.GunReload, param);
                        }
                        else
                            skillCmpt.StartSkill(skillCmpt, gun.m_DryFireSoundID);
                        //audiomanager.instance.create(gun.m_aimattr.m_aimtrans.position,
                        //                             gun.m_dryfiresoundid,
                        //                             gun.m_aimattr.m_aimtrans);
					}
				}
				else
				{
					if(gun.magazineValue >= gun.m_EnergyPerShoot)
					{
						if(Time.time - m_LastShootTime > gun.m_FireRate)
							OnFire(gun.m_EnergyPerShoot);
					}
					else
						AudioManager.instance.Create(gun.m_AimAttr.m_AimTrans.position,
						                             gun.m_DryFireSoundID,
						                             gun.m_AimAttr.m_AimTrans);
				}
				break;
			}
		}

		public override bool Update ()
		{
			if(null == gun)
				return true;
			if(null != energyGunLogic && !energyGunLogic.Equals(null))
				energyGunLogic.lastUsedTime = Time.time;
			switch(gun.m_ShootMode)
			{
			case ShootMode.SingleShoot:
				return true;
			case ShootMode.MultiShoot:
				if(gun.m_AmmoType == AmmoType.Bullet)
				{
					if(gun.magazineValue < 1)
						m_EndFire = true;
					else if(Time.time - m_LastShootTime > gun.m_FireRate)
						OnFire(1);
				}
				else
				{
					if(gun.magazineValue < gun.m_EnergyPerShoot)
						m_EndFire = true;
					else if(Time.time - m_LastShootTime > gun.m_FireRate)
						OnFire(gun.m_EnergyPerShoot);
				}
				if(m_EndFire)
				{
					if(null != m_Audio)
					{
						m_Audio.Delete(0.1f);
						m_Audio = null;
					}
					return true;
				}

				break;
			case ShootMode.ChargeShoot:
				if(Time.time - m_LastShootTime > gun.m_FireRate)
				{
					if(m_EndAfterShoot)
						m_EndFire = true;
					if(m_HoldFireTime > m_ChargeEffectDelayTime)
					{
						if(null != gun.m_ChargeEffectGo && !gun.m_ChargeEffectGo.activeSelf)
						{
							m_Audio = AudioManager.instance.Create(gun.m_AimAttr.m_AimTrans.position,
							                                       gun.m_ChargeSoundID,
							                                       gun.m_AimAttr.m_AimTrans, true, false);
							gun.m_ChargeEffectGo.SetActive(true);
							gun.magazineValue -= gun.m_EnergyPerShoot;
						}
						
						if(gun.magazineValue <= gun.m_ChargeEnergySpeed * Time.deltaTime)
						{
							m_EndFire = true;
							gun.magazineValue = 0;
						}
						else
						{
							gun.magazineValue -= gun.m_ChargeEnergySpeed * Time.deltaTime;
						}
						
						if(m_EndFire)
						{
							OnFire(0, GetChargeLevel(m_HoldFireTime));
							skillCmpt.StartSkill(skillCmpt, gun.m_ShootSoundID);

							if(null != m_Audio)
							{
								m_Audio.Delete();
								m_Audio = null;
							}
							if(null != gun.m_ChargeEffectGo)
								gun.m_ChargeEffectGo.SetActive(false);
							return true;
						}

					}
					else
					{
						if(m_EndFire)
						{
							OnFire(gun.m_EnergyPerShoot, GetChargeLevel(m_HoldFireTime));
							skillCmpt.StartSkill(skillCmpt, gun.m_ShootSoundID);
							return true;
						}
					}
					
					int oldChargeLevel = GetChargeLevel(m_HoldFireTime);
					m_HoldFireTime += Time.deltaTime;
					if(GetChargeLevel(m_HoldFireTime) > oldChargeLevel)
					{
						AudioManager.instance.Create(gun.transform.position, gun.m_ChargeLevelUpSoundID, gun.transform);
						Effect.EffectBuilder.Instance.Register(gun.m_ChargeLevelUpEffectID, null, gun.m_AimAttr.m_AimTrans);
					}
				}
				else if(m_EndFire)
				{
					EndImmediately();
					return true;
				}
				break;
			}
			return false;
		}

		public override void EndAction ()
		{
			m_EndFire = true;
		}

		public override void EndImmediately ()
		{
			if(null != m_Audio)
			{
				m_Audio.Delete();
				m_Audio = null;
			}
			if(null != gun && gun.m_ShootMode == ShootMode.ChargeShoot)
			{
				if(null != gun.m_ChargeEffectGo)
					gun.m_ChargeEffectGo.SetActive(false);
			}
			if(null != energyGunLogic && !energyGunLogic.Equals(null))
				energyGunLogic.lastUsedTime = Time.time;
		}
		
		int GetChargeLevel(float chargeTime)
		{
			if(null == gun)
				return 0;
			for(int i = 0; i < gun.m_ChargeTime.Length; i++)
				if(chargeTime < gun.m_ChargeTime[i])
					return i;
			return gun.m_ChargeTime.Length;
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(null != gun)
			{
				switch(eventParam)
				{
				case "ShellCase":
					if(null != gun && 0 != gun.m_ShellCaseEffectID && null != gun.m_ShellCaseTrans)
						Effect.EffectBuilder.Instance.Register(gun.m_ShellCaseEffectID, null, gun.m_ShellCaseTrans);
					break;
				}
			}
		}

		void OnFire(float magazineCost, int skillIndex = 0)
		{
			if(null != gun && null != gun.m_AimAttr && null != gun.m_AimAttr.m_AimTrans && null != gun.ItemObj && null != entity
			   && null != skillCmpt && null != gun.m_AttackMode)
			{
				skillCmpt.StartSkill(skillCmpt, gun.m_ShootSoundID);
//				AudioManager.instance.Create(gun.m_AimAttr.m_AimTrans.position,
//				                              gun.m_ShootSoundID,
//				                              gun.m_AimAttr.m_AimTrans);
				SkillSystem.ShootTargetPara target = new SkillSystem.ShootTargetPara();
				if(null != ikAim)
					target.m_TargetPos = ikAim.targetPos;
				else
					target.m_TargetPos = entity.position + entity.forward;

				skillCmpt.StartSkill(targetEntity, gun.GetSkillID(skillIndex), target);

                if(PeGameMgr.IsSingle)
                {
                    if (!(m_IgnoreItem && gun.m_AmmoType == AmmoType.Energy))
                        gun.magazineValue -= magazineCost;
                }
                else
                {
					if (!m_IgnoreItem && null != PlayerNetwork.mainPlayer)
                    {
                        PlayerNetwork.mainPlayer.RequestAttrChanged(entity.Id, gun.ItemObj.instanceId, magazineCost, gun.curItemID);
                    }
                }				

				m_LastShootTime = Time.time;
				if(m_EndAfterShoot && null != motionMgr)
					motionMgr.EndAction(ActionType);
				
				entity.SendMsg(EMsg.Battle_EquipAttack, gun.ItemObj);

				if(!PeGameMgr.IsMulti)
				{
					if(gun.m_AttackMode.Length > 0)
						entity.SendMsg(EMsg.Battle_OnAttack, gun.m_AttackMode[0], gun.transform, gun.curItemID);
				}
			}
		}
	}
	
	[Serializable]
	public class Action_GunReload : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.GunReload; } }
		
		PEGun		m_Gun;
		public PEGun gun
		{
			get { return m_Gun; }
			set { m_Gun = value; if(null == m_Gun) motionMgr.EndImmediately(ActionType); }
		}

		int				m_TargetAmmoIndex;
		bool 			m_ReloadEnd;
		bool			m_AnimEnd;
		AudioController	m_Audio;

		public bool   	m_IgnoreItem = false;

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null != gun && m_IgnoreItem)
				return true;
			PEActionParamN paramN = PEActionParamN.param;
			int targetAmmIndex = paramN.n;
			if(m_IgnoreItem)
				return null != gun && null != packageCmpt && gun.m_AmmoType == AmmoType.Bullet
					&& gun.magazineValue < gun.magazineSize;

			return null != gun && null != packageCmpt && gun.m_AmmoType == AmmoType.Bullet
				&& gun.magazineValue < gun.magazineSize && (targetAmmIndex != gun.curAmmoItemIndex || packageCmpt.GetItemCount(gun.curItemID) > 0);
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			PEActionParamN paramN = PEActionParamN.param;
			motionMgr.SetMaskState(PEActionMask.GunReload, true);
			m_TargetAmmoIndex = paramN.n;
			if(null != gun && null != anim && motionMgr.IsActionRunning(PEActionType.GunHold))
			{
				anim.SetTrigger(gun.m_ReloadAnim);
				m_Audio = AudioManager.instance.Create(gun.transform.position, gun.m_ReloadSoundID, gun.transform);
				m_ReloadEnd = false;
				m_AnimEnd = false;
			}
			else
			{
				Reload();
			}
			if(null != gun && null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
				ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
		}
		
		public override bool Update ()
		{
			if(null == gun)
			{
				motionMgr.SetMaskState(PEActionMask.GunReload, false);
				return true;
			}
			if(m_ReloadEnd)
			{
				if(null != anim)
				{
					if(m_AnimEnd)
					{
						motionMgr.SetMaskState(PEActionMask.GunReload, false);
						if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
							ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
						return true;
					}
				}
				else
					return true;
			}
			if(null != gun && null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
				ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
			return false;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.GunReload, false);
			if(null != anim)
			{
				anim.SetTrigger("ResetUpbody");
				if(null != gun)
					anim.ResetTrigger(gun.m_ReloadAnim);
			}
			if(null != m_Audio)
			{
				m_Audio.Delete();
				m_Audio = null;
			}
			
			if(null != gun && null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
				ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
		}
		
		void Reload()
		{
			if(null != gun && motionMgr.IsActionRunning(ActionType) && null != entity)
			{
				m_ReloadEnd = true;

				if (null != gun.m_MagazineObj)
					gun.m_MagazineObj.SetActive(false);

				if(null == gun.m_AmmoItemIDList || gun.m_AmmoItemIDList.Length <= m_TargetAmmoIndex 
				   || gun.m_AmmoItemIDList.Length <= gun.curAmmoItemIndex)
				{
					gun.magazineValue = gun.magazineSize;
					return;
				}
				int oldAmmoItemId = gun.m_AmmoItemIDList[m_TargetAmmoIndex];
				if(GameConfig.IsMultiMode && !m_IgnoreItem && null != PlayerNetwork.mainPlayer)
					PlayerNetwork.mainPlayer.RequestReload(entity.Id, gun.ItemObj.instanceId, oldAmmoItemId, gun.m_AmmoItemIDList[m_TargetAmmoIndex], gun.magazineSize);

				if (!GameConfig.IsMultiMode && gun.magazineValue > 0 && null != packageCmpt && !m_IgnoreItem)
                {
                    PlayerPackageCmpt playerPackage = packageCmpt as PlayerPackageCmpt;
                    if (playerPackage != null)
                    {
                        playerPackage.package.Add(gun.m_AmmoItemIDList[gun.curAmmoItemIndex], Mathf.RoundToInt(gun.magazineValue));
                    }
					else
                    {
                        packageCmpt.Add(gun.m_AmmoItemIDList[gun.curAmmoItemIndex], Mathf.RoundToInt(gun.magazineValue));
                    }
                }

				gun.curAmmoItemIndex = m_TargetAmmoIndex;

				int packageNum = Mathf.RoundToInt(gun.magazineSize);

				if (!m_IgnoreItem && null != packageCmpt)
					packageNum = packageCmpt.GetItemCount(gun.m_AmmoItemIDList[m_TargetAmmoIndex]);

				if (packageNum > 0)
				{
					int addCount = Mathf.Min(packageNum, Mathf.RoundToInt(gun.magazineSize));
					gun.magazineValue = addCount;
					if(!m_IgnoreItem && 0 != packageNum) //!GameConfig.IsMultiMode && 
                        packageCmpt.Destory(gun.m_AmmoItemIDList[m_TargetAmmoIndex], addCount);
				}
			}
		}

		void MagazineOff()
		{
			if(null != gun && null != gun.m_MagazinePos && 0 != gun.m_MagazineEffectID)
				Effect.EffectBuilder.Instance.Register(gun.m_MagazineEffectID, null, gun.m_MagazinePos);
		}

		void MagazineShow()
		{
			if(null != gun && null != gun.m_MagazineObj)
				gun.m_MagazineObj.SetActive(true);
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(null != gun && motionMgr.IsActionRunning(ActionType))
			{
				switch(eventParam)
				{
				case "Reload":
					Reload();
					break;
				case "ReloadEnd":
					m_AnimEnd = true;
					break;
				case "MagazineOff":
					MagazineOff();
					break;
				case "MagazineShow":
					MagazineShow();
					break;
				}
			}
		}
	}

	
	[Serializable]
	public class Action_GunMelee : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.GunMelee; } }
		
		public SkillSystem.SkEntity targetEntity{ get; set; }
		PEGun m_Gun;
		public PEGun gun
		{
			get{ return m_Gun; }
			set
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_Gun = value;
			}
		}

		SkillSystem.SkInst m_SkillInst;

		int m_ModeIndex;

		public override bool CanDoAction (PEActionParam para = null)
		{
			return null != gun;
		}

		public override void DoAction (PEActionParam para = null)
		{
			PEActionParamN paramN = para as PEActionParamN;
			m_ModeIndex = paramN.n;
			if(null != gun && null != gun.m_SkillIDList && gun.m_SkillIDList.Length > m_ModeIndex)
				m_SkillInst = skillCmpt.StartSkill(targetEntity, gun.m_SkillIDList[m_ModeIndex]);
			
			if(null != entity && null != gun.m_AttackMode && gun.m_AttackMode.Length > m_ModeIndex)
				entity.SendMsg(EMsg.Battle_OnAttack, gun.m_AttackMode[m_ModeIndex], gun.transform, gun.curItemID);
		}

		public override bool Update ()
		{
			if(null == anim)
				return true;
			if(null != gun && null != gun.m_SkillIDList && gun.m_SkillIDList.Length > m_ModeIndex)
				return !skillCmpt.IsSkillRunning(gun.m_SkillIDList[m_ModeIndex]);
			return true;
		}

		public override void EndImmediately ()
		{
			if(null != gun && null != gun.m_SkillIDList && gun.m_SkillIDList.Length > m_ModeIndex)
				skillCmpt.CancelSkillById(gun.m_SkillIDList[m_ModeIndex]);
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(null != gun && motionMgr.IsActionRunning(ActionType))
			{
				if(eventParam == "EndAction")
					motionMgr.EndImmediately(ActionType);
				if(eventParam == "MonsterEndAttack" && null != m_SkillInst)
					m_SkillInst.SkipWaitAll = true;
			}
		}
	}
}