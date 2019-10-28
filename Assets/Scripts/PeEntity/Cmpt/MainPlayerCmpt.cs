using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using ItemAsset;
using SkillSystem;

namespace Pathea
{
	public class MainPlayerCmpt : PeCmpt, IPeMsg
	{
		public static MainPlayerCmpt gMainPlayer;
		[HideInInspector]
		public Transform _camTarget = null;
		[HideInInspector]
		public Transform _bneckModel = null;
		[HideInInspector]
		public Transform _bneckRagdoll = null;

		MotionMgrCmpt		mMotionMgr;
		Motion_Move_Human	mMove;
		Motion_Equip		mEquip;
		BiologyViewCmpt		mView;
		PeTrans             mTrans;
		IKCmpt				mIK;
		SkAliveEntity		m_Skill;
		PackageCmpt			mPackage;
		PassengerCmpt		mPassenger;
		HumanPhyCtrl		m_PhyCtrl;
		AbnormalConditionCmpt m_Abnormalcmpt;

		public LayerMask 	m_ShootLayer;
		public float		m_ShootMinDis = 10f;
		float 				m_DefautShootDis = 100f;

		public float		m_DiveMinY = -0.2f;

		bool				m_InShootMode;

		bool				m_AutoRun;
        public bool AutoRun        { get { return m_AutoRun; } }

		const float	DeathDropRange = 10f;

		public bool m_MouseMoveMode = false;

		public float waterUpSpeed = 3f;
		public float waterJumpHeight = 1.5f;

		bool m_MoveWalk;

		[Range(0, 1f)]
		public float subTelescopeDamp = 0.8f;
		bool m_ShowSubTelescope;
		Vector3 m_SubTelescopePos;

		[Header("FallDamage")]
		public int 	MoveRecordCount = 10;
		public int 	CurrentSpeedFramCount = 4;
		public float FallDamageSpeedThreshold = 15f;
		public float FallDamageSpeedToDamage = 15f;
		List<Vector3> m_MoveState;

		List<Vector3> m_MoveRequest;
		int MoveRequestCount = 5;
		Vector3 m_MoveDir = Vector3.zero;
		Vector3 m_MouseHitPos = Vector3.zero;

		[HideInInspector]
		public bool	m_ActionEnable = true;

		bool m_DisableActionByUI;

		public event Action<int> onEquipmentAttack;

        public event Action<bool> onBuildMode;

		public event Action onDurabilityDeficiency;

		public const int StartInvincibleSkillID = 30100750;		
		public const int RemoveInvincibleSkillID = 30100751;

		public static bool isCameraRollable {
			get {
				if (null != gMainPlayer)
					return gMainPlayer.mMotionMgr.IsActionRunning (PEActionType.Glider);
				return false;
			}
		}

		public float m_FadeTime = 0.1f;
		bool m_FirstPersonCtrl;

		public bool firstPersonCtrl
		{
			get { return m_FirstPersonCtrl; }
			set
			{
				m_FirstPersonCtrl = value;
				PeCamera.is1stPerson = m_FirstPersonCtrl;
				Entity.SendMsg(EMsg.View_FirstPerson, m_FirstPersonCtrl, this.GetType());
			}
		}

		[Header("MouseOperation")]
		Action_Gather m_ActionGather;
		Action_Gather actionGather
		{
			get
			{
				if(null == m_ActionGather)
					m_ActionGather = mMotionMgr.GetAction<Action_Gather>();
				return m_ActionGather;
			}
		}
		
		Action_Fell m_Fell;
		Action_Fell actionFell
		{
			get
			{
				if(null == m_Fell)
					m_Fell = mMotionMgr.GetAction<Action_Fell>();
				if(null != m_Fell && null != m_Fell.m_Axe)
					return m_Fell;
				return null;
			}
		}

		public MouseOpMgr.MouseOpCursor actionOpCursor;

		float m_UpdateMouseStateInterval = 0.2f;
		float m_UpdateMouseStateTime;

		PEAbnormalNotice[] m_AbnormalNotices;

		public override void Start ()
		{
			base.Start ();

			m_MoveState = new List<Vector3> (MoveRecordCount);
			m_MoveRequest = new List<Vector3> (MoveRequestCount);

			gMainPlayer = this;
			gameObject.AddComponent<Scanner> ();
			mMove = Entity.GetCmpt<Motion_Move_Human> ();
			mEquip = Entity.motionEquipment;
			mView = Entity.biologyViewCmpt;
			mTrans = Entity.peTrans;
			mIK = Entity.GetCmpt<IKCmpt> ();
			m_Skill = Entity.aliveEntity;
			mPackage = Entity.GetCmpt<PackageCmpt> ();
			mPassenger = Entity.passengerCmpt;
			mMotionMgr = Entity.motionMgr;
			mMotionMgr.onActionStart += OnActionStart;
			mMotionMgr.onActionEnd += OnActionEnd;
			m_Abnormalcmpt = Entity.Alnormal;

			if(null != m_Abnormalcmpt)
			{
				m_Abnormalcmpt.evtStart += OnStartAbnormal;
				m_Abnormalcmpt.evtEnd += OnEndAbnormal;
			}

			if (null != m_Skill) 
			{
				m_Skill.onHpReduce += OnDamage;
				m_Skill.attackEvent += OnAttack;
                m_Skill.deathEvent += OnDeath;
				m_Skill.onSkillEvent += OnSkillTarget;
                m_Skill.onWeaponAttack += OnWeaponAttack;

                m_Skill.OnBeEnemyEnter += OnBeEnemyEnter;
			}

            if(!PeGameMgr.IsTutorial)
			    StartCoroutine(UpdateAbnormalNotice());

            //历险模式下此时初始化声望系统，ForceSetting并未加载，会导致声望系统不能正常起作用
            //InitReputationSystem();
            Invoke("CheckAbnormalState", 5f);
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			UpdateMouseOperation();
			UpdateInputState ();
			UpdateUIState ();
			UpdateImpactDamage ();
			UpdateFirstPersonState();
            UpdateAnimatorSpeed();
        }

        void UpdateAnimatorSpeed()
        {
            //if (firstPersonCtrl)
            //{
            //    Entity.animCmpt.animator.speed = (mMotionMgr.IsActionRunning(PEActionType.GunHold)
            //        && !mMotionMgr.IsActionPause(PEActionType.GunHold)) ? 0.1f : 1f;
            //}
        }

        void InitReputationSystem()
        {
            if (GameConfig.IsMultiMode)
                return;

            if(m_Skill != null)
            {
				ReputationSystem.Instance.AddPlayerID((int)m_Skill.GetAttribute(AttribType.DefaultPlayerID));
				if(PeGameMgr.IsAdventure)
					ReputationSystem.Instance.ActiveReputation((int)m_Skill.GetAttribute(AttribType.DefaultPlayerID));
            }
        }

        Quaternion _cameraRotation;
        void UpdateMoveState ()
		{
			if (PeCamera.isFreeLook || (!PeGameMgr.IsMulti && PeGameMgr.gamePause)) {
				mMove.Move (Vector3.zero);
				return;
			}			

			m_MoveDir = Vector3.zero;

		    Vector3 moveDirLocal = Vector3.zero;
            
            moveDirLocal = PeInput.GetAxisH() * Vector3.right + PeInput.GetAxisV() * Vector3.forward;

            if (m_AutoRun && moveDirLocal == Vector3.zero && (!UIStateMgr.Instance.isTalking || GameUI.Instance.mNPCTalk.type != UINPCTalk.NormalOrSp.Normal))
				moveDirLocal += Vector3.forward;
			
			if(PeInput.Get (PeInput.LogicFunction.SwitchWalkRun)
			   && !Entity.passengerCmpt.IsOnCarrier())
				m_MoveWalk = !m_MoveWalk;

			float inputDirLength = moveDirLocal.magnitude;
			bool stickWalk = inputDirLength > 0.1f && inputDirLength < 0.9f;
			
			if (PeInput.Get (PeInput.LogicFunction.AutoRunOnOff))
				m_AutoRun = !m_AutoRun;
			
			if (PeInput.GetAxisV () < -0.5f || Entity.passengerCmpt.IsOnCarrier())
				m_AutoRun = false;
			
			Vector3 dodgeDir = Vector3.zero;
			if (PeInput.Get (PeInput.LogicFunction.DodgeForward))
				dodgeDir += Vector3.forward;
			if (PeInput.Get (PeInput.LogicFunction.DodgeRight))
				dodgeDir += Vector3.right;
			if (PeInput.Get (PeInput.LogicFunction.DodgeBackward))
				dodgeDir += Vector3.back;
			if (PeInput.Get (PeInput.LogicFunction.DodgeLeft))
				dodgeDir += Vector3.left;

            if (!PeInput.Get(PeInput.LogicFunction.LiberatiePerspective))
            {
                _cameraRotation = PETools.PEUtil.MainCamTransform.rotation;
            }

            //			if(PeInput.Get(PeInput.LogicFunction.SwimUp)) dodgeDir += Vector3.up;
            if (dodgeDir != Vector3.zero && (PeGameMgr.IsMulti || !PeGameMgr.gamePause))
			{
				dodgeDir = Vector3.ProjectOnPlane (_cameraRotation * dodgeDir, Vector3.up).normalized;
				mMove.Dodge (dodgeDir);
			}
			
			if (mMove.state == MovementState.Water)
				m_MoveDir = _cameraRotation * moveDirLocal;
			else
				m_MoveDir = Vector3.ProjectOnPlane (_cameraRotation * moveDirLocal, Vector3.up);
			
			if (null != m_PhyCtrl && m_PhyCtrl.spineInWater) 
			{
				if (!m_PhyCtrl.headInWater) 
				{
					if (m_MoveDir.y < 0 && m_MoveDir.y > m_DiveMinY)
						m_MoveDir.y = 0;
					if(PeInput.Get (PeInput.LogicFunction.Jump)
					   && !mView.IsRagdoll
					   && !mMotionMgr.IsActionRunning(PEActionType.Dig)
					   && !mMotionMgr.IsActionRunning(PEActionType.Gather))
						m_PhyCtrl.ApplyImpact (Mathf.Sqrt(20f * waterJumpHeight) * Vector3.up);
				}
				if (PeInput.Get (PeInput.LogicFunction.SwimUp)
				    && !mView.IsRagdoll
				    && !mMotionMgr.IsActionRunning(PEActionType.Dig)
				    && !mMotionMgr.IsActionRunning(PEActionType.Gather))
					m_PhyCtrl.ApplyMoveRequest (waterUpSpeed * Vector3.up);
			}
			
			if (!m_MouseMoveMode) 
			{
				if (mMove.autoRotate) 
				{
					if (m_MoveRequest.Count == MoveRequestCount)
						m_MoveRequest.RemoveAt (0);
					for(int i = 0; i < m_MoveRequest.Count; i++)
					{
						if (Vector3.Angle (m_MoveRequest[i], moveDirLocal) > 150f) 
						{
							PEActionParamVBB param = PEActionParamVBB.param;
							param.vec = m_MoveDir.normalized;
							param.b1 = true;
							param.b2 = false;
							if (mMotionMgr.DoAction (PEActionType.Rotate, param))
								m_MoveRequest.Clear ();
							break;
						}
					}
					m_MoveRequest.Add (moveDirLocal);
				}
				if(mMotionMgr.IsActionRunning(PEActionType.Hand))
				{
					if(null == m_Hand)
						m_Hand = mMotionMgr.GetAction<Action_Hand>();
					if(m_Hand.moveable)
						mMove.Move (m_MoveDir.normalized, SpeedState.Walk);
				}
				else
				{
					SpeedState state = SpeedState.Run;
					if(PeInput.Get(PeInput.LogicFunction.Sprint))
						state = SpeedState.Sprint;
					else if(m_MoveWalk || stickWalk)
						state = SpeedState.Walk;
					mMove.Move (m_MoveDir.normalized, state);
				}
				mMove.UpdateMoveDir (m_MoveDir, moveDirLocal);
			}
			
			mEquip.UpdateMoveDir (m_MoveDir, moveDirLocal);

			if (PeInput.Get (PeInput.LogicFunction.Jump))
				mMove.Jump ();
		}
		Action_Hand m_Hand;

		void UpdateAimTarget ()
		{
			Ray cameraRay = PeCamera.mouseRay;
			
			if (null != TestPEEntityCamCtrl.Instance) {
				Camera cam = TestPEEntityCamCtrl.Instance.GetCam ();
				if (null != cam) {
					cameraRay = cam.ScreenPointToRay (new Vector3 (Screen.width / 2f, Screen.height / 2f, 0));
				}
			}
			
			if (mMotionMgr.isInAimState) {
				cameraRay = PeCamera.cursorRay;
			}
			
			RaycastHit[] hitResults = Physics.RaycastAll (cameraRay, m_DefautShootDis, m_ShootLayer.value, QueryTriggerInteraction.Ignore);
			float minDis = m_DefautShootDis;
			for(int i = 0; i < hitResults.Length; i++)
				if (hitResults[i].distance > 0 
				    && hitResults[i].distance < minDis
				    && !hitResults[i].collider.transform.IsChildOf(transform))
					minDis = hitResults[i].distance;
			minDis = Mathf.Clamp (minDis, m_ShootMinDis, m_DefautShootDis);
			
			m_MouseHitPos = cameraRay.origin + cameraRay.direction * minDis;

			if (!mMotionMgr.IsActionRunning (PEActionType.SwordAttack) 
			    && !mMotionMgr.IsActionRunning (PEActionType.TwoHandSwordAttack))
				mIK.aimTargetPos = m_MouseHitPos;

			if(mMotionMgr.isInAimState && Vector3.zero != mIK.aimRay.direction)
			{
				RaycastHit hitResult;
				float disToHitPos = Vector3.Distance(m_MouseHitPos, mIK.aimRay.origin);

				bool showSubTelescope = Physics.Raycast(mIK.aimRay, out hitResult, disToHitPos, m_ShootLayer.value, QueryTriggerInteraction.Ignore) 
					&& disToHitPos >  hitResult.distance + 0.1f
						&& !mMotionMgr.IsActionRunning(PEActionType.GunReload)
						&& !mMotionMgr.IsActionRunning(PEActionType.BowReload);
				if(m_ShowSubTelescope != showSubTelescope)
				{
					m_ShowSubTelescope = showSubTelescope;
					UISightingTelescope.Instance.EnableOrthoAimPoint(m_ShowSubTelescope);
				}

				if(m_ShowSubTelescope && !mMotionMgr.IsActionRunning(PEActionType.GunFire)&& !mMotionMgr.IsActionRunning(PEActionType.BowShoot))
				{
					Vector3 curPos = Camera.main.WorldToScreenPoint(hitResult.point);
					curPos = Vector3.Lerp(curPos, m_SubTelescopePos, subTelescopeDamp);
					m_SubTelescopePos = curPos;
					curPos.x = Mathf.RoundToInt(curPos.x);
					curPos.y = Mathf.RoundToInt(curPos.y);
					curPos.z = Mathf.RoundToInt(curPos.z);
					UISightingTelescope.Instance.SetOrthoAimPointPos(curPos);
				}
			}
		}

		void UpdateOtherAction ()
		{
			
			#if UNITY_EDITOR			
			if (Input.GetKeyDown (KeyCode.Keypad5)) 
			{
				mPackage.Add (1277, 1);
				PeMap.StaticPoint.Mgr.Instance.UnveilAll ();
			}
			#endif

			if (PeCamera.isFreeLook || !m_ActionEnable)
				return;

			
			if (PeInput.Get (PeInput.LogicFunction.Jet))
				mMotionMgr.DoAction (PEActionType.JetPack);
			else
				mMotionMgr.EndImmediately (PEActionType.JetPack);
			
			if (PeInput.Get (PeInput.LogicFunction.ClimbForwardLadderOnOff)) 
			{
				DragItemMousePickLadder ladder = MousePicker.Instance.curPickObj as DragItemMousePickLadder;
				if (null != ladder)
				{
					ladder.TryClimbLadder(this);
				}
			}
			
			mEquip.HoldSheild (PeInput.Get (PeInput.LogicFunction.Block));
			
			if (PeInput.Get (PeInput.LogicFunction.DrawWeapon))
			{
				if(m_DisableActionByUI && mEquip.ISAimWeapon)
					m_DisableActionByUI = false;
				mEquip.ActiveWeapon (true);
			}
			
			if (PeInput.Get (PeInput.LogicFunction.Attack)) {
				if (SystemSettingData.Instance.AttackWhithMouseDir) {
					Vector3 dir = GetMouseClickDir ();
					mEquip.SwordAttack (dir);
					mEquip.TwoHandWeaponAttack(dir);
				} else {
					mEquip.SwordAttack (m_MoveDir.normalized);
					mEquip.TwoHandWeaponAttack(m_MoveDir.normalized);
				}
			}
			
			if (PeInput.Get (PeInput.LogicFunction.SheatheWeapon))
				mEquip.ActiveWeapon (false);
			
			if (PeInput.Get (PeInput.LogicFunction.GatherHerb))
			{
				mMotionMgr.DoAction (PEActionType.Gather);
			}
			
			if (PeInput.Get (PeInput.LogicFunction.DrawWater)) 
			{
				mMotionMgr.DoAction (PEActionType.Draw);
			}

			if (PeInput.Get (PeInput.LogicFunction.TakeForwardVehicleOnOff)) {
				if (null != mPassenger) {
					if (mPassenger.IsOnVCCarrier)
						mPassenger.GetOffCarrier ();
					else if (null != MousePicker.Instance.curPickObj) 
					{
						DragItemMousePickCarrier carrier = MousePicker.Instance.curPickObj as DragItemMousePickCarrier;
						if (null != carrier) {
							WhiteCat.CarrierController controller = carrier.GetComponent<WhiteCat.CarrierController> ();
							if (null != controller) {
								int seatIndex = controller.FindEmptySeatIndex ();
								if (seatIndex > -2) {
									if (GameConfig.IsMultiMode) 
									{
										PEActionParamDrive param = PEActionParamDrive.param;
										param.controller = controller;
										param.seatIndex = seatIndex;
										if (mMotionMgr.CanDoAction (PEActionType.Drive, param)) {
											WhiteCat.CreationSkEntity skEntity = controller.GetComponent<WhiteCat.CreationSkEntity>();
											if (skEntity != null && skEntity._net != null)
											{
												if (!ForceSetting.Instance.Conflict(skEntity._net.TeamId, PlayerNetwork.mainPlayerId))
													PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_GetOnVehicle, skEntity._net.Id);
												else
													new PeTipMsg(PELocalization.GetString(82209000), PeTipMsg.EMsgLevel.Warning);
											}
										}
									} else
										mPassenger.GetOn (controller, seatIndex, true);
								}
							}
						}
					}
				}
			}
			
			if (PeInput.Get (PeInput.LogicFunction.Cut))
				mMotionMgr.DoAction (PEActionType.Fell);
			else
				mMotionMgr.EndAction (PEActionType.Fell);
			
			if (PeInput.Get (PeInput.LogicFunction.EndShooting)) 
			{
				mMotionMgr.EndAction (PEActionType.GunFire);
				mMotionMgr.EndAction (PEActionType.Pump);
			}
			else if (PeInput.Get (PeInput.LogicFunction.BegShooting))
			{
				PEActionParamB param = PEActionParamB.param;
				param.b = false;
				mMotionMgr.DoAction (PEActionType.GunFire, param);
				mMotionMgr.DoAction (PEActionType.BowShoot);
				mMotionMgr.DoAction (PEActionType.Throw);
				mMotionMgr.DoAction (PEActionType.Pump);
				mMotionMgr.DoAction (PEActionType.RopeGunShoot);
			}
			
			if (m_MouseMoveMode) {
				if (PeInput.Get (PeInput.LogicFunction.BegShooting))
					mMove.MoveTo (m_MouseHitPos, SpeedState.Sprint);
				if (Input.GetMouseButtonDown (1))
					mMove.MoveTo (Vector3.zero, SpeedState.Sprint);
			}
			
			if (PeInput.Get (PeInput.LogicFunction.EndDigging))
				mMotionMgr.EndAction (PEActionType.Dig);
			else if (PeInput.Get (PeInput.LogicFunction.BegDigging))
			{
				PEActionParamV param = PEActionParamV.param;
				param.vec = Vector3.zero;
				mMotionMgr.DoAction (PEActionType.Dig, param);
			}
			
			if (PeInput.Get (PeInput.LogicFunction.Reload))
				mEquip.Reload ();

            if (PeInput.Get(PeInput.LogicFunction.BuildMode)){
                if (mMotionMgr.IsActionRunning(PEActionType.Build))
                    mMotionMgr.EndAction(PEActionType.Build);
                else
                {
					if(RandomDungenMgrData.InDungeon){
						new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
						return;
					}
                    if (Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.MainLand
                    || Pathea.SingleGameStory.curType == Pathea.SingleGameStory.StoryScene.TrainingShip)
                        mMotionMgr.DoAction(PEActionType.Build);
                    else
                        new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
                }
            }

		}

		void UpdateInputState ()
		{
			UpdateAimTarget ();

			UpdateMoveState ();

			UpdateOtherAction ();
		}

		void UpdateUIState ()
		{
			if (m_InShootMode && null != UISightingTelescope.Instance)
				UISightingTelescope.Instance.Scale = mEquip.GetAimPointScale ();
		}

		void UpdateImpactDamage ()
		{
			if(!PeGameMgr.IsMulti && PeGameMgr.gamePause)
				return;

			if (null != m_PhyCtrl) {
				if(Entity.IsDeath()
				   || mView.IsRagdoll || mMotionMgr.freezePhyState
				   || mMotionMgr.IsActionRunning(PEActionType.Step)
				   || mMotionMgr.IsActionRunning(PEActionType.RopeGunShoot)
				   || Entity.passengerCmpt.IsOnCarrier())
				{
					m_MoveState.Clear();
					return;
				}
				if (m_MoveState.Count >= MoveRecordCount)
					m_MoveState.RemoveAt (0);
				m_MoveState.Add (m_PhyCtrl.velocity);
				if (m_MoveState.Count < MoveRecordCount)
					return;
				Vector3 oldVelocity = Vector3.zero;
				for (int i = 0; i < MoveRecordCount - CurrentSpeedFramCount; i++)
					oldVelocity += m_MoveState [i];
				oldVelocity /= MoveRecordCount - CurrentSpeedFramCount;
				Vector3 newVelocity = Vector3.zero;
				for (int i = 0; i < CurrentSpeedFramCount; i++)
					newVelocity += m_MoveState [MoveRecordCount - 1 - i];
				newVelocity /= CurrentSpeedFramCount;
				float dtSpeed = Vector3.Distance (newVelocity, oldVelocity);
				if(mMotionMgr.IsActionRunning(PEActionType.SwordAttack)
				   || mMotionMgr.IsActionRunning(PEActionType.TwoHandSwordAttack))
					dtSpeed = Mathf.Abs(newVelocity.y - oldVelocity.y);
				if (dtSpeed > FallDamageSpeedThreshold) {
					float hp = Entity.GetAttribute (AttribType.Hp);
					hp = Mathf.Clamp (hp - (dtSpeed - FallDamageSpeedThreshold) * FallDamageSpeedToDamage, 0, hp);
					Entity.SetAttribute (AttribType.Hp, hp, false);
					m_MoveState.Clear ();
				}
			}
		}

		void UpdateFirstPersonState()
		{
			bool firstPersonCamera = SystemSettingData.Instance.FirstPersonCtrl
					&& !mMotionMgr.IsActionRunning(PEActionType.Build) 
					&& !PeCamera.isFreeLook 
					&& !mMotionMgr.IsActionRunning(PEActionType.Drive);
			if(firstPersonCtrl != firstPersonCamera)
				firstPersonCtrl = firstPersonCamera;
		}

		void UpdateMouseOperation()
		{
			if(mMotionMgr.isInAimState)
			{
				actionOpCursor = MouseOpMgr.MouseOpCursor.Null;
				return;
			}
			if(m_UpdateMouseStateTime > Time.time) return;
			actionOpCursor = MouseOpMgr.MouseOpCursor.Null;
			m_UpdateMouseStateTime += m_UpdateMouseStateInterval;
			if(null != actionGather && actionGather.UpdateOPTreeInfo())
				actionOpCursor = MouseOpMgr.MouseOpCursor.Gather;

			if(null != actionFell && actionFell.UpdateOPTreeInfo() 
			   && MouseOpMgr.MouseOpCursor.Null == actionOpCursor
			   && mMotionMgr.IsActionRunning(PEActionType.EquipmentHold))
				actionOpCursor = MouseOpMgr.MouseOpCursor.Fell;
		}

		Vector3 GetMouseClickDir ()
		{
			Ray ray = PeCamera.mouseRay;
			Plane plane = new Plane (mTrans.existent.up, mTrans.position + mTrans.existent.up);
			float dis;
			if (plane.Raycast (ray, out dis)) 
			{
				return (ray.GetPoint (dis) - (mTrans.position + mTrans.existent.up)).normalized;
			}

			return mTrans.existent.forward;
		}

		public void UpdateCamDirection (Vector3 camForward)
		{
			if (firstPersonCtrl || m_InShootMode && mMotionMgr.isInAimState) 
			{
				if (null != m_PhyCtrl && m_PhyCtrl.spineInWater)
					mTrans.rotation = Quaternion.LookRotation (camForward, Vector3.up);
				else
					mTrans.rotation = Quaternion.LookRotation (Vector3.ProjectOnPlane (camForward, Vector3.up).normalized, Vector3.up);
			}
		}

		void ResetFirstCtrl()
		{
			if(m_FirstPersonCtrl)
				firstPersonCtrl = m_FirstPersonCtrl;
		}

		#region IPeMsg implementation
		public void OnMsg (EMsg msg, params object[] args)
		{
			switch (msg) {
			case EMsg.View_Prefab_Build:
				BiologyViewCmpt obj = args [0] as BiologyViewCmpt;
//				TestHitBack hitBack = obj.GetComponentInChildren<TestHitBack> ();
//				if (null != hitBack) {
//					hitBack.cam = Camera.main;
//					obj.transform.position += 1.3f * Vector3.up;
//				}
				m_PhyCtrl = obj.monoPhyCtrl;

				if(obj.monoModelCtrlr != null)			_camTarget = obj.monoModelCtrlr.transform.Find("CamTarget");
				if(obj.monoModelCtrlr != null)			_bneckModel = obj.monoModelCtrlr.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck");
				if(obj.monoRagdollCtrlr != null)		_bneckRagdoll = obj.monoRagdollCtrlr.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck");


				if(null != UILoadScenceEffect.Instance && UILoadScenceEffect.Instance.isInProgress)
					StartInvincible();

                    InitReputationSystem();
                    Invoke("ResetFirstCtrl", 1f);
				break;

                //case EMsg.View_Model_Build:
                    
                //    break;

                case EMsg.Camera_ChangeMode:
				PeCamera.cameraModeData = args [0] as CameraModeData;
				break;
			case EMsg.Battle_EnterShootMode:
				m_InShootMode = true;
				if (null != TestPEEntityCamCtrl.Instance && null != mView.modelTrans) {
					TestPEEntityCamCtrl.Instance.SetCamMode (PETools.PEUtil.GetChild (mView.modelTrans, "CamTarget"),
				                                            PETools.PEUtil.GetChild (mView.modelTrans, "Bip01 Neck"), "3rd Person Shoot");
				}

				if (null != UISightingTelescope.Instance) {
					UISightingTelescope.Instance.Show ((UISightingTelescope.SightingType)args [0]);
				}
                //lz-2018.01.18 改变持枪状态，改变相机灵敏度的返回值
                SystemSettingData.Instance.holdGun = true;

				break;
			case EMsg.Battle_ExitShootMode:
				m_InShootMode = false;
				if (null != TestPEEntityCamCtrl.Instance) {
					TestPEEntityCamCtrl.Instance.SetCamMode (PETools.PEUtil.GetChild (mView.modelTrans, "CamTarget"),
					                                        PETools.PEUtil.GetChild (mView.modelTrans, "Bip01 Neck"), "Normal Mode F1");
				}
				if (null != UISightingTelescope.Instance) {
					UISightingTelescope.Instance.ExitShootMode ();
				}
                SystemSettingData.Instance.holdGun = false;
                break;

			case EMsg.Battle_OnShoot:
				if (null != UISightingTelescope.Instance)
					UISightingTelescope.Instance.OnShoot ();
				break;
			case EMsg.UI_ShowChange:
				m_DisableActionByUI = (bool)args [0];
				if (m_DisableActionByUI && mEquip.ISAimWeapon) {
					mEquip.ActiveWeapon (false);
//					for(PEActionType type = PEActionType.Move; type <= PEActionType.Stuned; type++)
//						mMotionMgr.EndImmediately(type);
				}
				break;
			case EMsg.Build_BuildMode:
				bool enterBuildMode = (bool)args [0];
				if (enterBuildMode)
					GameUIMode.Instance.GotoBuildMode ();
				else
					GameUIMode.Instance.GotoBaseMode ();

				PeCamera.SetVar ("Build Mode", enterBuildMode);

                if (null != onBuildMode)
                    onBuildMode(enterBuildMode);

				break;
			case EMsg.Battle_OnAttack:
				if(null != onEquipmentAttack)
					onEquipmentAttack((int)args [2]);
				PeEventGlobal.Instance.MainPlayerAttack.Invoke (Entity, (AttackMode)args [0]);
				break;

			//case EMsg.Battle_HPChange:
			//	SkEntity skEntity = (SkEntity)args[0];
			//	float damage = (float)args[1];
			//	if (skEntity != null && damage < PETools.PEMath.Epsilon)
			//	{
			//		OnDamage(skEntity, Mathf.Abs(damage));
			//	}
			//	break;

			case EMsg.Action_DurabilityDeficiency:
				if(null != onDurabilityDeficiency)
					onDurabilityDeficiency();
				break;
			}
		}

		#endregion

		void OnDeath (SkEntity self, SkEntity caster)
		{
			ApplyDeathDropItem ();
		}

        public void TransferHared(PeEntity targetentity, float damage)
        {
            float tansDis = targetentity.IsBoss ? 128f : 64f;
            int playerID = (int)Entity.GetAttribute(AttribType.DefaultPlayerID);
            List<PeEntity> entities = EntityMgr.Instance.GetEntities(Entity.position, tansDis, playerID, false, Entity);
            for (int i = 0; i < entities.Count; i++)
            {
                if (!entities[i].Equals(targetentity) && entities[i].target != null)
                    entities[i].target.TransferHatred(targetentity, damage);
            }
        }

        void OnAttack(SkEntity skEntity, float damage)
        {
            PeEntity tarEntity = skEntity.GetComponent<PeEntity>();

            TransferHared(tarEntity,damage);
        }

		void OnDamage (SkEntity entity, float damage)
		{
			if (null == m_Skill || null == entity)
				return;

			PeEntity peEntity = entity.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;

            TransferHared(peEntity, damage);
		}

		void OnSkillTarget(SkEntity caster)
		{
			if (null == m_Skill || null == caster)
				return;

			int playerID = (int)m_Skill.GetAttribute ((int)AttribType.DefaultPlayerID);
            //SkEntity Ca_entity = PETools.PEUtil.GetCaster(caster);//GetCaster(caster);
			PeEntity peEntity = caster.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;
			
			float tansDis = peEntity.IsBoss ? 128f : 64f;
			bool canTrans = false;
			if (GameConfig.IsMultiClient)
			{
				if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
					canTrans = true;
			}
			else
			{
				if (ForceSetting.Instance.GetForceID(playerID) == 1)
					canTrans = true;
			}
			
			if (canTrans)
			{
				List<PeEntity> entities = EntityMgr.Instance.GetEntities (mTrans.position, tansDis, playerID, false, Entity);
				for(int i = 0; i < entities.Count; i++)
				{
					if (entities[i] == null)
						continue;

					if (!entities[i].Equals (Entity) && entities[i].target != null) 
					{
						entities[i].target.OnTargetSkill(peEntity.skEntity);
					}
				}
			}
		}

        void OnWeaponAttack(SkEntity caster)
        {
            OnSkillTarget(caster);
        }

        void OnBeEnemyEnter(PeEntity attacker)
        {
            OnSkillTarget(attacker.skEntity);
        }

		void ApplyDeathDropItem ()
		{
			if (!GameConfig.IsMultiMode)
				return;
			if (null == mPackage)
				return;

			int dropNum = 6;
			CustomData.MapObj[] mapObjs = new CustomData.MapObj[dropNum];
			int dropCount = 0;
			int mMaxCount = 100;
			while (dropCount < dropNum) {
				Vector3 offSetPos = new Vector3 (UnityEngine.Random.Range (-DeathDropRange, DeathDropRange), 
				                                5f, UnityEngine.Random.Range (-DeathDropRange, DeathDropRange));
				RaycastHit hitInfo;
				if (Physics.Raycast (mTrans.position + offSetPos, Vector3.down, out hitInfo, 10f, PEConfig.GroundedLayer, QueryTriggerInteraction.Ignore)) 
				{
					if (hitInfo.distance < 10f) {
						mapObjs [dropCount] = new CustomData.MapObj ();
						mapObjs [dropCount].pos = hitInfo.point;
						mapObjs [dropCount].objID = 0;
						++dropCount;
					}
				}
				if (mMaxCount-- <= 0)
					return;
			}
            PlayerNetwork.mainPlayer.CreateMapObj((int)DoodadType.DoodadType_Dead, mapObjs);
		}

		void OnStartAbnormal(PEAbnormalType type)
		{
			if(null != UIMainMidCtrl.Instance)
			{
				AbnormalData data = AbnormalData.GetData(type);
				if(null != data && data.iconName != "0")
					UIMainMidCtrl.Instance.AddBuffShow(data.iconName, data.description);
			}
		}

		void OnEndAbnormal(PEAbnormalType type)
		{
			if(null != UIMainMidCtrl.Instance)
			{
				AbnormalData data = AbnormalData.GetData(type);
				if(null != data && data.iconName != "0")
					UIMainMidCtrl.Instance.DeleteBuffShow(data.iconName);
			}
		}

		void OnActionStart(PEActionType actionType)
		{
			if (actionType == PEActionType.Fell)
				PeCamera.fpCameraCanRotate = false;
		}

		
		void OnActionEnd(PEActionType actionType)
		{
			if (actionType == PEActionType.Fell)
				PeCamera.fpCameraCanRotate = true;
		}

		IEnumerator UpdateAbnormalNotice()
		{
			yield return new WaitForSeconds(5f);

			PEAbnormalNoticeData[] datas =  PEAbnormalNoticeData.datas;
			m_AbnormalNotices = new PEAbnormalNotice[datas.Length];
			for(int i = 0; i < datas.Length; ++i)
			{
				m_AbnormalNotices[i] = new PEAbnormalNotice();
				m_AbnormalNotices[i].Init(Entity, datas[i]);
			}

			while(true)
			{
				for(int i = 0; i < datas.Length; ++i)
					m_AbnormalNotices[i].Update();
				yield return null;
			}
		}

		public void StartInvincible()
		{
			if(null != Entity.skEntity)
			{
				Entity.skEntity.StartSkill(Entity.skEntity, StartInvincibleSkillID);
				StartCoroutine(EndInvincible());
			}
		}

		IEnumerator EndInvincible()
		{
			while(null != UILoadScenceEffect.Instance && UILoadScenceEffect.Instance.isInProgress)
				yield return null;

			yield return new WaitForSeconds(3f);
			
			if(null != Entity.skEntity)
				Entity.skEntity.StartSkill(Entity.skEntity, RemoveInvincibleSkillID);
		}
	}
}
