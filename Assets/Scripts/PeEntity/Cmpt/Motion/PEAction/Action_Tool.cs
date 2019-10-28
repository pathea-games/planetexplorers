using UnityEngine;

namespace Pathea
{
	public interface iEquipHideAbleAction
	{
		bool hideEquipInactive{ get; set; }
	}

	[System.Serializable]
	public class Action_HandChangeEquipHold : PEAction, iEquipHideAbleAction
	{
		public override PEActionType ActionType { get { return PEActionType.EquipmentHold; } }

		bool m_HideEquipInactive;
		public bool hideEquipInactive
		{
			get{ return m_HideEquipInactive; }
			set
			{
				m_HideEquipInactive = value;
				if(null != mHandChangeEquipment)
				{
					SetHideState(!motionMgr.IsActionRunning(ActionType));
//					mHandChangeEquipment.HideEquipmentByFirstPerson(!motionMgr.IsActionRunning(ActionType));
//					Renderer[] renders = mHandChangeEquipment.gameObject.GetComponentsInChildren<Renderer>();
//					for(int i = 0; i < renders.Length; i++)
//						renders[i].enabled = !m_HideEquipInactive;
				}
			}
		}

		PEHoldAbleEquipment mHandChangeEquipment;

		public PEHoldAbleEquipment handChangeEquipment
		{
			get { return mHandChangeEquipment; }
			set 
			{			
				if(null == value || (null != mHandChangeEquipment && mHandChangeEquipment != value))
				{
					motionMgr.EndImmediately(ActionType);
				}
				mHandChangeEquipment = value;	
				if(null != mHandChangeEquipment)
				{
					m_ActionMask = mHandChangeEquipment.m_HandChangeAttr.m_HoldActionMask;
					SetHideState(true);
				}
				else
					m_ActionMask = PEActionMask.EquipmentHold;
			}
		}
		
		protected bool m_EndAction;
		protected bool m_PutOnAnimEnd;
		protected bool m_PutOffAnimEnd;

		protected bool m_HoldState;

		protected UTimer m_FixErrorTimer;

		protected PEActionMask m_ActionMask = PEActionMask.EquipmentHold;

		public event System.Action onActiveEvt;
		public event System.Action onDeactiveEvt;

		protected override void OnAnimEvent(string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				switch(eventParam)
				{
				case "PutOnAnimEnd":
					m_PutOnAnimEnd = true;
					break;
				case "PutOffAnimEnd":
					m_PutOffAnimEnd = true;
					if(null != move && m_EndAction)
						move.style = (null != handChangeEquipment) ? handChangeEquipment.m_HandChangeAttr.m_BaseMoveStyle : move.baseMoveStyle;
					break;
				case "PutOnEquipment":
					ChangeHoldState(true);
					break;
				case "PutOffEquipment":
					ChangeHoldState(false);
					break;
				case "ChangeCameraMode":
					if(null != handChangeEquipment)
						motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
					break;
				case "Active":
					if(null != handChangeEquipment)
						handChangeEquipment.SetActiveState(true);
					break;
				case "Deactive":
					if(null != handChangeEquipment)
						handChangeEquipment.SetActiveState(false);
					break;
				}
			}
		}

		public Action_HandChangeEquipHold()
		{
			if(null == m_FixErrorTimer)
			{
				m_FixErrorTimer = new UTimer();
				m_FixErrorTimer.ElapseSpeed = -1f;
			}
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			return null != anim && null != handChangeEquipment && handChangeEquipment.canHoldEquipment;
		}

		public override void DoAction (PEActionParam para = null)
		{
			if (null != onActiveEvt)
				onActiveEvt ();

			m_EndAction = false;
			m_PutOnAnimEnd = false;
			m_PutOffAnimEnd = false;
			m_HoldState = false;

			m_FixErrorTimer.Second = 5f;

			SetHideState(false);

			if(null != anim && null != handChangeEquipment)
			{
				if(!string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim))
				{
					if(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim != handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
						anim.SetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim);
					else
						anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, true);
				}
				else
				{
					ChangeHoldState(true);
					m_PutOnAnimEnd = true;
				}
			}
			if(null != move && null != handChangeEquipment)
				move.style = handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
			if(null != ikCmpt)
			{
				ikCmpt.SetSpineEffectDeactiveState(this.GetType(), true);
				if(null != ikCmpt.m_IKAimCtrl && null != handChangeEquipment)
					ikCmpt.m_IKAimCtrl.m_DetectorAngle = handChangeEquipment.m_HandChangeAttr.m_AimIKAngleRange;
			}
		}

		public override void OnModelBuild ()
		{
			if(!m_EndAction)
			{
				m_PutOnAnimEnd = true;
				ChangeHoldState(true, false, false);
				if(null != handChangeEquipment)
				{
					motionMgr.SetMaskState(m_ActionMask, true);
					entity.motionEquipment.SetWeapon(handChangeEquipment);
					motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
					if(null != move)
						move.style = handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
				}
				if(null != ikCmpt)
					ikCmpt.SetSpineEffectDeactiveState(this.GetType(), true);

				if(null != anim && null != handChangeEquipment)
				{
					if(!string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
					   && handChangeEquipment.m_HandChangeAttr.m_PutOnAnim == handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
						anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, true);
				}
			}
		}

		public override void ContinueAction ()
		{
			if(!m_EndAction)
			{
				m_PutOnAnimEnd = true;
				ChangeHoldState(true, false, false);
				if(null != handChangeEquipment)
				{
					motionMgr.SetMaskState(m_ActionMask, true);
					entity.motionEquipment.SetWeapon(handChangeEquipment);
					motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
					if(null != move)
						move.style = handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
				}
				if(null != ikCmpt)
					ikCmpt.SetSpineEffectDeactiveState(this.GetType(), true);
				
				if(null != anim && null != handChangeEquipment)
				{
					if(!string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
					   && handChangeEquipment.m_HandChangeAttr.m_PutOnAnim == handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
						anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, true);
				}
			}
		}
		
		public override void OnModelDestroy ()
		{
			if(m_EndAction)
				base.OnModelDestroy ();
		}

		public override bool Update ()
		{
			UpdateActiveMask();

			FixAnimError();

			if(m_EndAction && m_PutOffAnimEnd)
			{
				OnEndAction();
				return true;
			}
			
			return false;
		}

		public override void EndAction ()
		{
			if (m_EndAction)
				return;
			m_EndAction = true;
			if(null != anim && null != handChangeEquipment)
			{
				motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, CameraModeData.DefaultCameraData);
				if(null != anim)
				{
					if(!string.IsNullOrEmpty(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim))
					{
						if(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim != handChangeEquipment.m_HandChangeAttr.m_PutOffAnim)
							anim.SetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim);
						else
							anim.SetBool(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim, false);
					}
					else
						m_PutOffAnimEnd = true;
				}
				motionMgr.SetMaskState(m_ActionMask, false);
				entity.motionEquipment.SetWeapon(null);
				if(null != ikCmpt)
					ikCmpt.SetSpineEffectDeactiveState(this.GetType(), false);
			}
			else
			{
				m_PutOffAnimEnd = true;
			}
			
			if(null != move)
				move.style = (null != handChangeEquipment) ? handChangeEquipment.m_HandChangeAttr.m_BaseMoveStyle : move.baseMoveStyle;

			m_FixErrorTimer.Second = 5f;
		}

		public override void EndImmediately ()
		{
			m_EndAction = true;
			if(null != handChangeEquipment)
			{
				motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, CameraModeData.DefaultCameraData);
				viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
				
				if(null != ikCmpt)
					ikCmpt.SetSpineEffectDeactiveState(this.GetType(), false);
			}
			if(null != move)
				move.style = (null != handChangeEquipment) ? handChangeEquipment.m_HandChangeAttr.m_BaseMoveStyle : move.baseMoveStyle;
			if(null != anim)
			{
				anim.SetTrigger("ResetUpbody");
				if(null != handChangeEquipment)
				{
					anim.ResetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOnAnim);
					anim.ResetTrigger(handChangeEquipment.m_HandChangeAttr.m_PutOffAnim);
				}
			}
			OnEndAction();
		}

		protected virtual void OnEndAction()
		{
			SetHideState(true);
			motionMgr.SetMaskState(m_ActionMask, false);
			
			if (null != onDeactiveEvt)
				onDeactiveEvt ();
		}

		protected virtual void UpdateActiveMask()
		{
			if(null == handChangeEquipment)
				return;
			if(!m_EndAction && !motionMgr.GetMaskState(m_ActionMask) && m_PutOnAnimEnd)
			{
				motionMgr.SetMaskState(m_ActionMask, true);
				entity.motionEquipment.SetWeapon(handChangeEquipment);
			}
		}

		protected virtual void FixAnimError()
		{
			if(null == handChangeEquipment)
				return;
			if(m_EndAction)
			{
				if(!m_PutOffAnimEnd)
				{
					m_FixErrorTimer.Update(Time.deltaTime);
					if(m_FixErrorTimer.Second <= 0)
					{
						m_PutOffAnimEnd = true;
						ChangeHoldState(false);
						motionMgr.SetMaskState(m_ActionMask, false);
						entity.motionEquipment.SetWeapon(null);
						if(null != ikCmpt)
							ikCmpt.SetSpineEffectDeactiveState(this.GetType(), false);
					}
				}
				else
				{
					if(null != handChangeEquipment.transform.parent && handChangeEquipment.transform.parent.name != handChangeEquipment.m_HandChangeAttr.m_PutOffBone)
						viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
				}
			}
			else
			{
				if(!m_PutOnAnimEnd)
				{
					m_FixErrorTimer.Update(Time.deltaTime);
					if(m_FixErrorTimer.Second <= 0)
					{
						m_PutOnAnimEnd = true;
						ChangeHoldState(true);
						motionMgr.SetMaskState(m_ActionMask, true);
						entity.motionEquipment.SetWeapon(handChangeEquipment);
						if(null != handChangeEquipment)
							motionMgr.Entity.SendMsg(EMsg.Camera_ChangeMode, handChangeEquipment.m_HandChangeAttr.m_CamMode);
						if(null != ikCmpt)
							ikCmpt.SetSpineEffectDeactiveState(this.GetType(), true);
					}
				}
				else
				{
					if(null != handChangeEquipment.transform.parent && handChangeEquipment.transform.parent.name != handChangeEquipment.m_HandChangeAttr.m_PutOnBone)
						viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOnBone);
				}
			}
		}
		
		public virtual void ChangeHoldState(bool holdEquip, bool checkState = true, bool isReattach = true)
		{
			if(null == handChangeEquipment || handChangeEquipment.m_HandChangeAttr.m_PutOffBone == handChangeEquipment.m_HandChangeAttr.m_PutOnBone)
				return;
			if(holdEquip)
			{
				if(!m_HoldState || !checkState)
				{
					m_HoldState = true;
					if(null != viewCmpt)
					{
						if(isReattach)
							viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOnBone);
						else
							viewCmpt.AttachObject(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOnBone);

					}
				}
			}
			else
			{
				if(m_HoldState || !checkState)
				{
					m_HoldState = false;
					if(null != viewCmpt)
					{
						if(isReattach)
							viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
						else
							viewCmpt.AttachObject(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
					}
				}
			}
		}

		void SetHideState(bool hide)
		{
			if(hideEquipInactive && null != handChangeEquipment)
			{
				mHandChangeEquipment.HideEquipmentByFirstPerson(hide);
//				Renderer[] renders = handChangeEquipment.gameObject.GetComponentsInChildren<Renderer>();
//				for(int i = 0; i < renders.Length; i++)
//					renders[i].enabled = !hide;
			}
		}
	}

	[System.Serializable]
	public class Action_HandChangeEquipPutOff : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.EquipmentPutOff; } }
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.EndAction(PEActionType.EquipmentHold);
		}
	}

	[System.Serializable]
	public class Action_AimEquipHold : Action_HandChangeEquipHold 
	{
		public override PEActionType ActionType { get { return PEActionType.AimEquipHold; } }

		public PEAimAbleEquip aimAbleEquip
		{
			get { return handChangeEquipment as PEAimAbleEquip; }
			set { handChangeEquipment = value; }
		}

		public float		m_RotateSpeed = 80f;
		public float		m_StopRotAngle = 10f;
		public float		m_MinAngle = 20f;
		public float		m_MaxAngle = 45f;
		public float 		m_AimPointLerpSpeed = 5f;
		protected float 	m_CurAccuracy;
		protected Vector3 	m_Phase;
		protected Vector3	m_TargetPhase;
		protected Vector3 	m_PhaseT;
		protected float 	m_CenterHeight;

		bool m_StartRot;

		public override void DoAction (PEActionParam para = null)
		{
			if(null == anim || null == aimAbleEquip)
				return;
			base.DoAction(para);
			
			InitAimPoint();
		}

		public override void OnModelBuild ()
		{
			base.OnModelBuild ();
			if(!m_EndAction)
			{
				InitAimPoint();
				if(null != ikCmpt)
					ikCmpt.SetSpineEffectDeactiveState(this.GetType(), true);
				if(null != aimAbleEquip)
					motionMgr.Entity.SendMsg(EMsg.Battle_EnterShootMode, aimAbleEquip.m_AimAttr.m_AimPointType);
				if(null != move && null != handChangeEquipment)
					move.style = handChangeEquipment.m_HandChangeAttr.m_MoveStyle;
				if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
				{
					ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
					if(!pauseAction && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
						ikCmpt.m_IKAimCtrl.SetActive(true);
					if(!m_PutOnAnimEnd)
						ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
				}
			}
		}

		public override void PauseAction ()
		{
			if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
			{
				ikCmpt.m_IKAimCtrl.SetActive(false);
				ikCmpt.m_IKAimCtrl.SetAimTran(null);
			}
		}
		
		public override void ContinueAction ()
		{
			if(motionMgr.GetMaskState(m_ActionMask) && null != ikCmpt && null != ikCmpt.m_IKAimCtrl
			   && !m_EndAction && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
			{
				ikCmpt.m_IKAimCtrl.SetActive(true);
				ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
			}
		}
		
		public override bool Update ()
		{
			UpdateAimPoint();

//			UpdateRotate();

			UpdateActiveMask();

			FixAnimError();

			return UpdateEndAction();
		}

		protected override void OnEndAction ()
		{
			base.OnEndAction ();
			motionMgr.Entity.SendMsg(EMsg.Battle_ExitShootMode);
			if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
			{
				ikCmpt.m_IKAimCtrl.m_TargetPosOffset = Vector3.zero;
				ikCmpt.m_IKAimCtrl.SetAimTran(null);
				ikCmpt.m_IKAimCtrl.SetActive(false);
			}
		}
		
		public override void EndAction ()
		{
			base.EndAction();
			OnEndAction();
		}
		
		public override void EndImmediately ()
		{
			base.EndImmediately();
			OnEndAction();
			if(null != anim)
				anim.SetFloat("RotationAgr", 0f);
		}
		
		protected override void OnAnimEvent (string eventParam)
		{
			base.OnAnimEvent (eventParam);
			
			if(null != aimAbleEquip && motionMgr.IsActionRunning(ActionType))
			{
				switch(eventParam)
				{
				case "StartUpbodyAnim":
					if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK && aimAbleEquip.m_AimAttr.m_SyncIKWhenAnim)
						ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
					break;
				case "PutOnAnimEnd":
				case "EndUpbodyAnim":
					if(motionMgr.IsActionRunning(PEActionType.BowReload) || motionMgr.IsActionRunning(PEActionType.GunReload))
						return;
					if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK && aimAbleEquip.m_AimAttr.m_SyncIKWhenAnim)
						ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
					break;
				case "EnterShootMode":
					if(!m_EndAction)
					{
						if(null != aimAbleEquip)
							motionMgr.Entity.SendMsg(EMsg.Battle_EnterShootMode, aimAbleEquip.m_AimAttr.m_AimPointType);
						if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
						{
							ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
							if(!pauseAction && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
								ikCmpt.m_IKAimCtrl.SetActive(true);
							if(!m_PutOnAnimEnd)
								ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
						}
					}
					break;
				}
			}
		}

		protected override void FixAnimError ()
		{
			if(null == handChangeEquipment)
				return;
			if(m_EndAction)
			{
				if(!m_PutOffAnimEnd)
				{
					m_FixErrorTimer.Update(Time.deltaTime);
					if(m_FixErrorTimer.Second <= 0)
					{
						m_PutOffAnimEnd = true;
						ChangeHoldState(false);
						motionMgr.SetMaskState(m_ActionMask, false);
						entity.motionEquipment.SetWeapon(null);
						if(null != ikCmpt)
							ikCmpt.SetSpineEffectDeactiveState(this.GetType(), false);
					}
				}
				else
				{
					if(null != handChangeEquipment.transform.parent && handChangeEquipment.transform.parent.name != handChangeEquipment.m_HandChangeAttr.m_PutOffBone)
						viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOffBone);
				}
			}
			else
			{
				if(!m_PutOnAnimEnd)
				{
					m_FixErrorTimer.Update(Time.deltaTime);
					if(m_FixErrorTimer.Second <= 0)
					{
						m_PutOnAnimEnd = true;
						ChangeHoldState(true);
						motionMgr.SetMaskState(m_ActionMask, true);
						entity.motionEquipment.SetWeapon(handChangeEquipment);
						if(null != ikCmpt)
							ikCmpt.SetSpineEffectDeactiveState(this.GetType(), true);
						if(null != aimAbleEquip)
							motionMgr.Entity.SendMsg(EMsg.Battle_EnterShootMode, aimAbleEquip.m_AimAttr.m_AimPointType);
						if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl && null != aimAbleEquip && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
						{
							ikCmpt.m_IKAimCtrl.SetAimTran(aimAbleEquip.m_AimAttr.m_AimTrans);
							if(!pauseAction && aimAbleEquip.m_AimAttr.m_ApplyAimIK)
								ikCmpt.m_IKAimCtrl.SetActive(true);
							if(!m_PutOnAnimEnd)
								ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
						}
					}
				}
				else
				{
					if(null != handChangeEquipment.transform.parent && handChangeEquipment.transform.parent.name != handChangeEquipment.m_HandChangeAttr.m_PutOnBone)
						viewCmpt.Reattach(handChangeEquipment.gameObject, handChangeEquipment.m_HandChangeAttr.m_PutOnBone);
				}
			}
		}

		public void OnFire()
		{
			if(null == aimAbleEquip)
				return;
			float twoPI = 2f * Mathf.PI;
			float inverseStability = 1f - aimAbleEquip.m_AimAttr.m_FireStability;
			m_TargetPhase.x += UnityEngine.Random.Range(0f, 0.5f * inverseStability) * twoPI;
			m_TargetPhase.y += UnityEngine.Random.Range(0f, 0.5f * inverseStability) * twoPI;
			m_TargetPhase.z += UnityEngine.Random.Range(0f, 0.5f * inverseStability) * twoPI;
			m_CurAccuracy = Mathf.Min(m_CurAccuracy + aimAbleEquip.m_AimAttr.m_AccuracyDiffusionRate, aimAbleEquip.m_AimAttr.m_AccuracyMax);
			m_CenterHeight =  Mathf.Min(m_CenterHeight + aimAbleEquip.m_AimAttr.m_CenterUpDisPerShoot, aimAbleEquip.m_AimAttr.m_CenterUpDisMax);
		}
		
		void UpdateAimPoint()
		{
			if(null == aimAbleEquip)
				return;

			if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
			{
				float twoPI = 2f * Mathf.PI;
				m_CurAccuracy = Mathf.Max(m_CurAccuracy - aimAbleEquip.m_AimAttr.m_AccuracyShrinkSpeed * Time.deltaTime, aimAbleEquip.m_AimAttr.m_AccuracyMin);
				m_CenterHeight = Mathf.Max(m_CenterHeight - aimAbleEquip.m_AimAttr.m_CenterUpShrinkSpeed * Time.deltaTime, 0);
				m_PhaseT.x = aimAbleEquip.m_AimAttr.m_AccuracyPeriod * UnityEngine.Random.Range(1 - aimAbleEquip.m_AimAttr.m_FireStability, 1 + aimAbleEquip.m_AimAttr.m_FireStability);
				m_PhaseT.y = aimAbleEquip.m_AimAttr.m_AccuracyPeriod * UnityEngine.Random.Range(1 - aimAbleEquip.m_AimAttr.m_FireStability, 1 + aimAbleEquip.m_AimAttr.m_FireStability);
				m_PhaseT.z = aimAbleEquip.m_AimAttr.m_AccuracyPeriod * UnityEngine.Random.Range(1 - aimAbleEquip.m_AimAttr.m_FireStability, 1 + aimAbleEquip.m_AimAttr.m_FireStability);
				m_TargetPhase.x += twoPI * (Time.deltaTime / m_PhaseT.x);
				m_TargetPhase.y += twoPI * (Time.deltaTime / m_PhaseT.y);
				m_TargetPhase.z += twoPI * (Time.deltaTime / m_PhaseT.z);
				
				m_Phase = Vector3.Lerp(m_Phase, m_TargetPhase, m_AimPointLerpSpeed * Time.deltaTime);
				
				float dis = (ikCmpt.m_IKAimCtrl.targetPos - ikCmpt.m_IKAimCtrl.m_DetectorCenter.position).magnitude;
				float scale = Mathf.Clamp01(dis / PEAimAttr.AccuracyDis);
				ikCmpt.m_IKAimCtrl.m_TargetPosOffset.x = scale * m_CurAccuracy * Mathf.Sin(m_Phase.x);
				ikCmpt.m_IKAimCtrl.m_TargetPosOffset.y = scale * m_CurAccuracy * Mathf.Sin(m_Phase.y) + m_CenterHeight;
				ikCmpt.m_IKAimCtrl.m_TargetPosOffset.z = scale * m_CurAccuracy * Mathf.Sin(m_Phase.z);
			}
		}

		void UpdateRotate()
		{
			if(pauseAction)
			{
				if(null != anim)
					anim.SetFloat("RotationAgr", 0);
				return;
			}

			if(null != aimAbleEquip && !motionMgr.GetMaskState(aimAbleEquip.m_HandChangeAttr.m_HoldActionMask))
				return;

			if(null != trans && null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
			{
				Vector3 dir = ikCmpt.m_IKAimCtrl.targetPos - trans.position;
				dir = Vector3.ProjectOnPlane(dir, trans.existent.up).normalized;
				float forwardAngle = Vector3.Angle(dir, trans.existent.forward);
//				float rightAngle = Vector3.Angle(dir, m_Trans.existent.right);
//				if(rightAngle > 90)
//					forwardAngle *= -1f;
				if(m_StartRot || forwardAngle > m_MinAngle)
				{
					m_StartRot = true;
					float rotateAngle = Mathf.Clamp(m_RotateSpeed * Time.deltaTime, 0, forwardAngle);
					if(forwardAngle - rotateAngle > m_MaxAngle)
						rotateAngle = forwardAngle - m_MaxAngle;
					if(forwardAngle - rotateAngle < m_StopRotAngle)
						m_StartRot = false;
					
					float rightAngle = Vector3.Angle(dir, trans.existent.right);
					if(rightAngle > 90)
						rotateAngle *= -1f;
					trans.rotation = Quaternion.AngleAxis(rotateAngle, trans.existent.up) * trans.rotation;
//					if(null != anim)
//						anim.SetFloat("RotationAgr", forwardAngle);
				}
				else
				{
					if(null != anim)
						anim.SetFloat("RotationAgr", 0);
				}
			}
		}
		
		void InitAimPoint()
		{
			if(null == aimAbleEquip)
				return;
			m_CurAccuracy = aimAbleEquip.m_AimAttr.m_AccuracyMin;
			m_CenterHeight = 0;
			float twoPI = 2f * Mathf.PI;
			m_TargetPhase.x = UnityEngine.Random.Range(0f, 1f) * twoPI;
			m_TargetPhase.y = UnityEngine.Random.Range(0f, 1f) * twoPI;
			m_TargetPhase.z = UnityEngine.Random.Range(0f, 1f) * twoPI;
			m_Phase = m_TargetPhase;
		}

		public float GetAimPointScale()
		{
			if(null == aimAbleEquip)
				return 0;
			if(motionMgr.IsActionRunning(PEActionType.Move)
			   || motionMgr.IsActionRunning(PEActionType.Sprint)
			   || motionMgr.IsActionRunning(PEActionType.Fall)
			   || motionMgr.IsActionRunning(PEActionType.Jump))
				return 1f;
			if(null != aimAbleEquip)
				return m_CurAccuracy / aimAbleEquip.m_AimAttr.m_AccuracyMax;
			return 0;
		}

		bool UpdateEndAction()
		{
			if(m_EndAction && m_PutOffAnimEnd)
			{
				if(null != anim)
					anim.SetFloat("RotationAgr", 0f);
				OnEndAction();
				return true;
			}
			return false;
		}
	}

	[System.Serializable]
	public class Action_AimEquipPutOff : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.AimEquipPutOff; } }
		public override bool CanDoAction (PEActionParam para = null)
		{
			return motionMgr.IsActionRunning(PEActionType.AimEquipHold);
		}

		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.EndAction(PEActionType.AimEquipHold);
		}
	}
}
