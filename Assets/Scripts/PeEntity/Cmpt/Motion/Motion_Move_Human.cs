using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PEIK;
using RootMotion.FinalIK;

namespace Pathea
{
	public enum MoveType
	{
		Direction = 0,
		Target,
	}

	public class Motion_Move_Human : Motion_Move, IPeMsg
	{
		//Depend class
		HumanPhyCtrl 		m_PhyCtrl;
        PEPathfinder        m_Pathfinder;
		SkAliveEntity 		m_SkEntity;

		//Move
		public LayerMask	m_TerrainLayer;
		public float		m_CheckForwardDis = 1.5f;
		public float		m_CheckDisMin = 0.1f;
		public float		m_CheckUpDis = 1f;
		public float		m_CheckDisInterval = 0.3f;
		public const float	AnimLerpSpeed = 5f;
		public float		m_AngleAnimLerpF = 1f;
		public float		m_WaterStateLerpF = 5f;

		public float		m_DefaultRunSpeed = 5f;
		public float		m_DefaultWalkSpeed = 2f;
		public float		m_DefaultSprintSpeed = 8f;

		//NetMove
		public float		m_MaxNetDelay = 4f;
		public int			m_NetSpeedDownCount = 3;
		public float		m_NetSpeedUpDelay = 0.5f;
		public float		m_NetMoveMinSqrDis = 0.25f;
		public float		m_NetLerpSpeed = 5f;
		public float		m_NetSpeedScaleF = 0.5f;
		//float 				m_NetPreMoveTime = 0.1f;
		Vector3				m_NetMovePos = Vector3.zero;
		public Vector3 NetMovePos
		{
			set
			{
				m_NetMovePos = value;
			}
		}

//		Vector3 			m_NetRotationDir = Vector3.zero;
//		float				m_NetRotationTime;
//		float				m_NetRotationInterval = 1f;
		bool				m_NetMove;

		//Stuck
		float				m_LastMoveTime;
		float				m_StuckSpeed = 1f;
		Vector3				m_CurMovement = Vector3.zero;
		Vector3				m_MoveRequest = Vector3.zero;
		Vector3             m_CurAvoidDirection = Vector3.zero;
		//int                 m_AvoidCnt = 0;

		//Fall
		public float		m_FallStartTime = 0.3f;
		public float		m_FallHeight = 3f;
		float				m_LastOnGroundTime;
		float				m_LastOnGroundHeight;
        Vector3             m_MoveDestination;

		//Water
		float				m_InWaterLevel;
		bool				m_HeadInWater = true;
		AudioController		m_SwimmingSound;

		static readonly int[] SwimmingID = new int[4]{941, 942, 951, 952};

		public AnimationCurve m_ForwardAngleFootRotate;
		GrounderFBBIK		m_GroundIk;

		MoveParam				m_Param;

		public Action_Move		m_Move;
		public Action_Sprint	m_Sprint;
		public Action_Rotate 	m_Rotate;
		public Action_Step 		m_Step;
		public Action_Jump		m_Jump;
		public Action_Fall		m_Fall;
		public Action_Climb		m_ClimbLadder;
		public Action_Drive		m_Drive;
		Action_Train			m_Train;
		Action_Halt				m_Halt;
        Action_Ride             m_Ride;


        bool m_MoveToModel;
		bool m_Avoid;

		const float NetMoveMin = 0.01f;

		//SafePos
		const int UnSafePosCountNum = 5;
		Vector3 m_LastSafePos;
		List<Vector3> m_UnSafePos = new List<Vector3>(UnSafePosCountNum);

		float changeToRunTime;
		[SerializeField]
		float m_MinRunTime = 0.2f;


//		[HideInInspector]
		bool m_FirstPersonCtrl;
		bool firstPersonCtrl
		{
			get{ return m_FirstPersonCtrl; }
			set
			{
				m_FirstPersonCtrl = value;
				UpdateMoveSubState();
			}
		}

		bool isMainPlayer { get { return MainPlayer.Instance.entity == Entity ; } }

		public bool autoRotate{ get { return m_Move.m_AutoRotate; } }

		double mNetJumpTime = -1;

		#region IMotionComponent implementation

		public override void Start ()
		{
			base.Start ();
			m_SkEntity = Entity.aliveEntity;
			m_NetMove = false;
			m_MoveToModel = false;
			m_Avoid = false;
			//Move
			mode = MoveMode.ForwardOnly;
			style = MoveStyle.Normal;
			baseMoveStyle = style;
			state = MovementState.Air;
			speed = SpeedState.Run;

			Entity.motionMgr.AddAction(m_Move);

			m_Sprint.m_UseStamina = isMainPlayer;
			Entity.motionMgr.AddAction(m_Sprint);

			Entity.animCmpt.AnimEvtString += m_Rotate.AnimEvent;
			Entity.motionMgr.AddAction(m_Rotate);

			m_Step.m_UseStamina = isMainPlayer;
			Entity.motionMgr.AddAction(m_Step);

			Entity.motionMgr.AddAction(m_Jump);

			Entity.motionMgr.AddAction(m_Fall);

			Entity.motionMgr.AddAction(m_ClimbLadder);

			m_Drive.skillTreeMgr = Entity.GetCmpt<SkillTreeUnitMgr>();
			Entity.motionMgr.AddAction(m_Drive);

			m_Train = new Action_Train();
			Entity.motionMgr.AddAction(m_Train);

			m_Halt = new Action_Halt();
			Entity.motionMgr.AddAction(m_Halt);

            //lz-2016.12.21
            m_Ride = new Action_Ride();
            Entity.motionMgr.AddAction(m_Ride);

            m_LastOnGroundTime = Time.time;
			m_LastOnGroundHeight = Entity.position.y;
		}

//		void OnEnable()
//		{
//			if(null != m_PhyCtrl)
//			{
//				m_PhyCtrl.freezeUpdate = false;
//				m_PhyCtrl._rigidbody.isKinematic = false;
//			}
//		}
//
//		void OnDisable() 
//		{
//			if(null != m_PhyCtrl)
//			{
//				m_PhyCtrl.freezeUpdate = true;
//				m_PhyCtrl.desiredMovementDirection = Vector3.zero;
//				m_PhyCtrl.velocity = Vector3.zero;
//				m_PhyCtrl._rigidbody.isKinematic = true;
//			}
//		}

		public override void OnUpdate ()
		{
			UpdateLocation();
			UpdateAnimState();
			UpdatePathfinding();
			CheckFall();
			UpdateStuck();
			UpdateNetMove();
			UpdateContrlerPhy();
			UpdateSafePlace();
		}

		#endregion
		public override void Move(Vector3 dir, SpeedState state = SpeedState.Walk)
		{
			m_MoveToModel = false;
			if(state == SpeedState.Retreat)
				state = SpeedState.Run;
			MoveDir(dir, state, false);
		}

		void MoveDir(Vector3 dir, SpeedState state, bool rotateImmediately)
		{
			//If can't sprint try run
			if(state == SpeedState.Sprint)
			{
				if(Vector3.Angle(dir, Entity.forward) > 90f || Vector3.Project(dir, Entity.forward).sqrMagnitude < 0.01f) // 0.1*0.1
				{
					state = SpeedState.Run;
					changeToRunTime = Time.time;
				}
				else if(Entity.motionMgr.IsActionRunning(PEActionType.Sprint))
				{
					if(Entity.GetAttribute(AttribType.Stamina) <= 0)
						state = SpeedState.Run;
				}
				else
				{
					PEActionParamNVB param = PEActionParamNVB.param;
					param.n = (int)MoveType.Direction;
					param.vec = dir;
					param.b = rotateImmediately;
					if(!Entity.motionMgr.CanDoAction(PEActionType.Sprint, param))
						state = SpeedState.Run;
				}
				if(Time.time - changeToRunTime < m_MinRunTime)
					state = SpeedState.Run;
			}
			
			speed = state;

			m_MoveRequest = dir;
			
			switch(speed)
			{
			case SpeedState.Sprint:
				if(dir != Vector3.zero)
				{
					PEActionParamNVB param = PEActionParamNVB.param;
					param.n = (int)MoveType.Direction;
					param.vec = dir;
					param.b = rotateImmediately;
					Entity.motionMgr.DoAction(PEActionType.Sprint, param);
				}
				else
				{			
					Entity.motionMgr.EndAction(PEActionType.Move);
					Entity.motionMgr.EndAction(PEActionType.Sprint);
				}
				break;
			case SpeedState.Walk:
				m_Move.SetWalkState(true);
				if(dir != Vector3.zero)
				{
					PEActionParamNV param = PEActionParamNV.param;
					param.n = (int)MoveType.Direction;
					param.vec = dir;
					Entity.motionMgr.DoAction(PEActionType.Move, param);
				}
				else
				{
					Entity.motionMgr.EndAction(PEActionType.Move);
					Entity.motionMgr.EndAction(PEActionType.Sprint);
				}
				break;
			case SpeedState.Run:
				m_Move.SetWalkState(false);
				if(dir != Vector3.zero)
				{
					PEActionParamNV param = PEActionParamNV.param;
					param.n = (int)MoveType.Direction;
					param.vec = dir;
					Entity.motionMgr.DoAction(PEActionType.Move, param);
				}
				else
				{
					Entity.motionMgr.EndAction(PEActionType.Move);
					Entity.motionMgr.EndAction(PEActionType.Sprint);
				}
				break;
			}
		}

        public override void SetSpeed(float Speed)
		{
		}

		public override SpeedState speed 
		{
			set {
				if(value == m_SpeedState)
					return;
				m_SpeedState = value;
				if(m_SpeedState == SpeedState.None)
					m_SpeedState = SpeedState.Walk;
				switch(m_SpeedState)
				{
				case SpeedState.Walk:
					m_Move.SetWalkState(true);
					break;
				case SpeedState.Run:
					m_Move.SetWalkState(false);
					break;
				}
			}
		}
		public override MoveStyle baseMoveStyle 
		{
			set 
			{
				if(style == base.baseMoveStyle)
					style = value;
				base.baseMoveStyle = value;
			}
		}

		public override MoveStyle style 
		{
			get { return m_Style; }
			set 
			{
				m_Style = value;

				UpdateMoveSubState();

				MoveSpeed getSpeed = null;
				if(null != m_Param)
					getSpeed = m_Param.m_MoveSpeedList.Find(itr => itr.m_Style == m_Style);
				if(null != getSpeed)
				{
					m_Move.runSpeed = getSpeed.m_RunSpeed;
					m_Move.walkSpeed = getSpeed.m_WalkSpeed;
					m_Sprint.m_MoveSpeed = getSpeed.m_SprintSpeed;
				}
				else
				{
					m_Move.runSpeed = m_DefaultRunSpeed;
					m_Move.walkSpeed = m_DefaultWalkSpeed;
					m_Sprint.m_MoveSpeed = m_DefaultSprintSpeed;
				}
			}
		}

		public override MoveMode mode 
		{
			set 
			{
				m_Mode = value;
				UpdateMoveSubState();
			}
		}

		public void UpdateMoveDir(Vector3 moveDirWorld, Vector3 moveDirLocal)
		{
			m_ClimbLadder.SetMoveDir(moveDirLocal.z, true);
			m_Jump.SetMoveDir(moveDirWorld);
			m_Fall.SetMoveDir(moveDirWorld);
		}

		void UpdateMoveSubState()
		{
			switch(m_Style)
			{
			case MoveStyle.Rifle:
			case MoveStyle.HandGun:
			case MoveStyle.Bow:
			case MoveStyle.Shotgun:
			case MoveStyle.Carry:
			case MoveStyle.BeCarry:
			case MoveStyle.Grenade:
			case MoveStyle.Drill:
				m_Sprint.m_ApplyStopIK = m_Move.m_ApplyStopIK = false;
				m_Jump.m_AutoRotate = m_Move.m_AutoRotate = false;
				break;
			default:
				if(m_Style == MoveStyle.Abnormal && Entity.motionMgr.IsActionRunning(PEActionType.Handed))
				{
					m_Sprint.m_ApplyStopIK = m_Move.m_ApplyStopIK = false;
					m_Jump.m_AutoRotate = m_Move.m_AutoRotate = false;
				}
				else
				{
					m_Sprint.m_ApplyStopIK = m_Move.m_ApplyStopIK = !firstPersonCtrl;
					m_Jump.m_AutoRotate = m_Move.m_AutoRotate = !firstPersonCtrl;
				}
				break;
			}
		}

		public override void MoveTo(Vector3 targetPos, SpeedState state = SpeedState.Walk,bool avoid = true)
		{
			m_Avoid = avoid;
			m_MoveToModel = true;
            m_MoveDestination = targetPos;
            speed = state;
		}

		public override void NetMoveTo (Vector3 position, Vector3 moveVelocity, bool immediately = false)
		{
			m_LastSafePos = position + 0.5f * Vector3.up;
			if(!immediately && !m_SkEntity.IsController())
			{
				if(Entity.peTrans == null)
					return;
				if(!m_NetMove)
					ResetNetMoveState(true);

				float distance = Vector3.Distance(m_NetMovePos, position);
				if(distance < NetMoveMin)
					return;

				m_NetMovePos = position;

//				if(Vector3.zero != moveVelocity)
//					m_NetMovePos += moveVelocity * m_NetPreMoveTime;
				
				if(null != Entity.viewCmpt && !Entity.viewCmpt.hasView)
				{
					Entity.position = position;
					SceneMan.SetDirty(Entity.lodCmpt);
				}
			}
			else
			{
				m_NetMovePos = position;
				Entity.position = position;
				SceneMan.SetDirty(Entity.lodCmpt);
				Stop();
				for(int i = 0; i < mNetTransInfos.Count; ++i)
					RecycleNetTranInfo(mNetTransInfos[i]);
				mNetTransInfos.Clear();
			}

			NetTranInfo netTransInfo = GetNetTransInfo();
			netTransInfo.pos = position;
			netTransInfo.rot = Entity.rotation.eulerAngles;
			netTransInfo.speed = speed;
			netTransInfo.contrllerTime = GameTime.Timer.Second;
			mNetTransInfos.Add(netTransInfo);
		}

		public void NetJump(double time)
		{
			mNetJumpTime = time;
		}

        public override void NetRotateTo(Vector3 eulerAngle)
		{
//			m_NetRotationDir = Quaternion.Euler(eulerAngle) * Vector3.forward;
			//			RotateTo(Quaternion.Euler(eulerAngle) * Vector3.forward);
			NetTranInfo netTransInfo = GetNetTransInfo();
			netTransInfo.pos = Entity.position;
			netTransInfo.rot = eulerAngle;
			netTransInfo.speed = speed;
			netTransInfo.contrllerTime = GameTime.Timer.Second;
			mNetTransInfos.Add(netTransInfo);
        }

        public override void RotateTo(Vector3 targetDir)
		{
			if(Entity.motionMgr == null)
				return;

			if(state != MovementState.Water && !Entity.motionMgr.IsActionRunning(PEActionType.Glider))
				targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up);

			if(Entity.motionMgr.IsActionRunning(PEActionType.Move))
				m_Move.SetLookDir(targetDir);
			else if(Entity.motionMgr.IsActionRunning(PEActionType.Jump))
				m_Jump.SetLookDir(targetDir);
			else
			{
				PEActionParamVBB param = PEActionParamVBB.param;
				param.vec = targetDir;
				param.b1 = false;
				param.b2 = true;
				Entity.motionMgr.DoAction(PEActionType.Rotate, param);
			}
		}

		public override void Jump ()
		{
			Entity.motionMgr.DoAction(PEActionType.Jump);
		}

		public override void Dodge (Vector3 dir)
		{
			PEActionParamV param = PEActionParamV.param;
			param.vec = dir;
			Entity.motionMgr.DoAction(PEActionType.Step, param);
		}

		public override Vector3 velocity {
			get 
			{
				if(null != m_PhyCtrl)
					return m_PhyCtrl.velocity;
				return base.velocity;
			}
			set
			{
				if(null != m_PhyCtrl)
					m_PhyCtrl.velocity = value;
			}
		}

        public override Vector3 movement
        {
            get
            {
                return m_MoveRequest;
            }
        }

        public override float gravity
        {
            get
            {
                return Mathf.Abs(Physics.gravity.y);
            }
        }

        public override bool grounded
        {
            get
            {
                return m_PhyCtrl != null ? m_PhyCtrl.grounded : false;
            }
        }

        public override void ApplyForce (Vector3 power, ForceMode mode)
		{
			if(null != m_PhyCtrl)
				m_PhyCtrl.GetComponent<Rigidbody>().AddForce(power, mode);
		}

		public override bool Stucking (float time)
		{
			return (Time.time - m_LastMoveTime) > time || (Entity.viewCmpt != null && !Entity.viewCmpt.hasView);
		}

        public override void Stop()
        {
            base.Stop();

            m_MoveDestination = Vector3.zero;

			if (null != Entity.motionMgr)
			{
	            Entity.motionMgr.EndImmediately(PEActionType.Move);
	            Entity.motionMgr.EndImmediately(PEActionType.Sprint);
			}
        }

		void UpdateLocation()
		{
			if(null != m_PhyCtrl)
			{
//				m_InWaterLevel = PETools.PE.PointInWater(Entity.position + m_PhyMotor.m_CheckWaterHeight * Vector3.up);
				if(m_PhyCtrl.feetInWater)
					state = MovementState.Water;
				else if(m_PhyCtrl.enabled && m_PhyCtrl.grounded)
					state = MovementState.Ground;
				else
					state = MovementState.Air;

				if(Entity.motionMgr.GetMaskState(PEActionMask.Ground) != m_PhyCtrl.grounded)
					Entity.motionMgr.SetMaskState(PEActionMask.Ground, m_PhyCtrl.grounded);
				if(Entity.motionMgr.GetMaskState(PEActionMask.InWater) != m_PhyCtrl.spineInWater)
					Entity.motionMgr.SetMaskState(PEActionMask.InWater, m_PhyCtrl.spineInWater);
				if(!m_NetMove)
				{
					if(Entity.motionMgr.GetMaskState(PEActionMask.InAir) != (state == MovementState.Air))
						Entity.motionMgr.SetMaskState(PEActionMask.InAir, state == MovementState.Air);
				}
				else
					Entity.motionMgr.SetMaskState(PEActionMask.InAir, false);

                bool currentHeadInwater = m_PhyCtrl.headInWater && (null == Entity.passengerCmpt || !Entity.passengerCmpt.IsOnCarrier());
                if (m_HeadInWater != currentHeadInwater)
				{
					m_HeadInWater = currentHeadInwater;
					Entity.SendMsg(EMsg.state_Water, m_HeadInWater);
				}

				if(m_PhyCtrl.spineInWater)
				{
					Entity.motionMgr.EndImmediately(PEActionType.AimEquipHold);
//					Entity.motionMgr.EndImmediately(PEActionType.GunHold);
					Entity.motionMgr.EndImmediately(PEActionType.BowHold);
				}

				if(!Entity.motionMgr.IsActionRunning(PEActionType.Move)
				   && !Entity.motionMgr.IsActionRunning(PEActionType.Sprint)
					&& !Entity.motionMgr.IsActionRunning(PEActionType.Glider)
				   && !Entity.motionMgr.isInAimState)
				{
					Entity.peTrans.rotation = Quaternion.Lerp(Entity.peTrans.rotation, 
					       	Quaternion.LookRotation(Vector3.ProjectOnPlane(Entity.forward, Vector3.up), Vector3.up), m_Param.m_MoveRotateSpeed * Time.deltaTime);
				}

				if(m_PhyCtrl.spineInWater && !m_PhyCtrl.headInWater
				   && (Entity.motionMgr.IsActionRunning(PEActionType.Move) || Entity.motionMgr.IsActionRunning(PEActionType.Sprint)))
				{
					if(null != m_SwimmingSound && !m_SwimmingSound.isPlaying)
					{
						m_SwimmingSound.Delete();
						m_SwimmingSound = null;
					}

					if(null == m_SwimmingSound)
					{
						m_SwimmingSound = AudioManager.instance.Create(Entity.position, SwimmingID[UnityEngine.Random.Range(0, SwimmingID.Length)], Entity.tr, false, false);
						m_SwimmingSound.PlayAudio(0.3f);
					}
				}
				else if(null != m_SwimmingSound && m_SwimmingSound.isPlaying)
				{
					m_SwimmingSound.StopAudio(0.5f);
					m_SwimmingSound = null;
				}
			}
		}
		
		void CheckFall()
		{
			if(null != m_PhyCtrl && !m_NetMove)
			{
				if(m_PhyCtrl.grounded 
				   || Entity.motionMgr.IsActionRunning(PEActionType.Drive)
				   || Entity.motionMgr.IsActionRunning(PEActionType.GetOnTrain)
				   || Entity.motionMgr.IsActionRunning(PEActionType.Glider))
				{
					m_LastOnGroundTime = Time.time;
					m_LastOnGroundHeight = Entity.position.y;
				}
				else
				{
					if(Time.time - m_LastOnGroundTime > m_FallStartTime || m_LastOnGroundHeight - Entity.position.y < -m_FallHeight)
						if(!Entity.motionMgr.IsActionRunning(PEActionType.Fall))
							Entity.motionMgr.DoAction(PEActionType.Fall);
				}
			}
		}

		void UpdateStuck()
		{
			if(m_MoveRequest == Vector3.zero || velocity.sqrMagnitude > m_StuckSpeed * m_StuckSpeed || Entity.isRagdoll) 
				m_LastMoveTime = Time.time;
		}

		void UpdateNetMove()
		{
//			if(!PeGameMgr.IsMulti)
//				return;
			if(Entity.motionMgr.IsActionRunning(PEActionType.GetOnTrain)
			   || Entity.motionMgr.IsActionRunning(PEActionType.Drive)
               || Entity.HasMount) //lz-2017.02.17 有坐骑之后不用行为去移动位置，而是本地模拟
				return;
			if(m_NetMove)
			{
                if (!Entity.viewCmpt.hasView)
                {
					if(mNetTransInfos.Count > 0)
						m_NetMovePos = mNetTransInfos[mNetTransInfos.Count - 1].pos;
					if(mNetTransInfos.Count > 1)
						for(int i = mNetTransInfos.Count - 2; i >= 0; --i)
							mNetTransInfos.RemoveAt(i);
					if(Vector3.SqrMagnitude(Entity.peTrans.position - m_NetMovePos) > 0.01f)
					{
						if(null != Entity.netCmpt && Entity.netCmpt.IsPlayer)
							SceneMan.SetDirty(Entity.lodCmpt);
	                    Entity.peTrans.position = m_NetMovePos;
					}
                    return;
                }

				if(Vector3.zero == m_NetMovePos)
					m_NetMovePos = Entity.position;
				if(mNetTransInfos.Count > 0 && Vector3.Distance(Entity.position, mNetTransInfos[0].pos) > 32f)
					Entity.position = mNetTransInfos[0].pos;

				if(mNetTransInfos.Count > 1)
				{
					int removeCount = 0;
					for(int i = 0; i < mNetTransInfos.Count - 1; ++i, ++removeCount)
					{
						if(GameTime.Timer.Second > mNetTransInfos[i].contrllerTime + m_MaxNetDelay * GameTime.Timer.ElapseSpeed)
							continue;
						Vector3 indexDir = mNetTransInfos[i + 1].pos - mNetTransInfos[i].pos;
						if(indexDir.sqrMagnitude < PETools.PEMath.Epsilon
						   || Vector3.SqrMagnitude(mNetTransInfos[i + 1].pos - Entity.position) < m_NetMoveMinSqrDis)
							continue;
						Vector3 currentDir = Entity.position - mNetTransInfos[i].pos;
						Vector3 projectDir = Vector3.Project(currentDir, indexDir);
						if(projectDir.sqrMagnitude < PETools.PEMath.Epsilon 
						   || Vector3.Angle(projectDir, indexDir) > 90f
						   || projectDir.sqrMagnitude < indexDir.sqrMagnitude)
						{
							break;
						}
					}
					
					for(int i = 0; i < removeCount; ++i)
					{
						RecycleNetTranInfo(mNetTransInfos[0]);
						mNetTransInfos.RemoveAt(0);
					}
					if(mNetTransInfos.Count > 1)
					{
						if(mNetTransInfos.Count > 2)
							m_NetMovePos = 0.5f * (mNetTransInfos[1].pos + mNetTransInfos[2].pos);
						else
							m_NetMovePos = mNetTransInfos[1].pos;

						for(int i = 0; i < mNetTransInfos.Count - 1; ++i)
						{
							Debug.DrawLine(mNetTransInfos[i].pos, mNetTransInfos[i + 1].pos, Color.yellow);
							Debug.DrawLine(mNetTransInfos[i].pos, mNetTransInfos[i].pos + 0.1f * Vector3.up, Color.green);
						}
						Debug.DrawLine(m_NetMovePos + 0.5f * Vector3.left, m_NetMovePos - 0.5f * Vector3.left, Color.red);
						Debug.DrawLine(m_NetMovePos + 0.5f * Vector3.up, m_NetMovePos - 0.5f * Vector3.up, Color.red);
					}
					else if(mNetTransInfos.Count > 0)
					{
						m_NetMovePos = mNetTransInfos[0].pos;
					}
					speed = mNetTransInfos[0].speed;

					if(mNetJumpTime > 0 && mNetJumpTime <= mNetTransInfos[0].contrllerTime)
					{
						if(null != m_PhyCtrl)
							m_PhyCtrl.ApplyImpact(Vector3.up);
						if(null != Entity && null != Entity.animCmpt)
							Entity.animCmpt.SetTrigger("Jump");
						mNetJumpTime = -1;
					}
					SceneMan.SetDirty(Entity.lodCmpt);
				}
				else if(mNetTransInfos.Count > 0)
					m_NetMovePos = mNetTransInfos[0].pos;

				if(null != m_PhyCtrl)
				{
					double dt = GameTime.Timer.Second - (mNetTransInfos.Count > 1 ? mNetTransInfos[1].contrllerTime:GameTime.Timer.Second);
					if(GameTime.Timer.ElapseSpeed > PETools.PEMath.Epsilon)
						dt /= GameTime.Timer.ElapseSpeed;
					if(mNetTransInfos.Count <= m_NetSpeedDownCount)
						m_PhyCtrl.netMoveSpeedScale = Mathf.Lerp(m_PhyCtrl.netMoveSpeedScale, 1f - m_NetSpeedScaleF, m_NetLerpSpeed * Time.deltaTime);
					else if(dt <= m_NetSpeedUpDelay)
						m_PhyCtrl.netMoveSpeedScale = Mathf.Lerp(m_PhyCtrl.netMoveSpeedScale, 1f, m_NetLerpSpeed * Time.deltaTime);
					else
						m_PhyCtrl.netMoveSpeedScale = Mathf.Lerp(m_PhyCtrl.netMoveSpeedScale, 1f + m_NetSpeedScaleF, m_NetLerpSpeed * Time.deltaTime);;
				}

				if(null != Entity.biologyViewCmpt && Entity.biologyViewCmpt.IsRagdoll)
				{
					if(null != m_PhyCtrl)
					{
						m_PhyCtrl.desiredMovementDirection = Vector3.zero;
						m_PhyCtrl.CancelMoveRequest();
					}
					Entity.motionMgr.EndImmediately(PEActionType.Move);
					Entity.motionMgr.EndImmediately(PEActionType.Sprint);
				}
				else if(Entity.motionMgr.IsActionRunning(PEActionType.Climb))
				{
					float dy = m_NetMovePos.y - Entity.position.y;
					if(Mathf.Abs(dy) > 0.5f)
						m_ClimbLadder.SetMoveDir(dy, false);
					else
						m_ClimbLadder.SetMoveDir(0, false);
					if(mNetTransInfos.Count > 0)
						Entity.rotation = Quaternion.LookRotation(Quaternion.Euler(mNetTransInfos[0].rot) * Vector3.forward);
				}
				else if(Entity.motionMgr.IsActionRunning(PEActionType.Glider)
				   	|| Entity.motionMgr.IsActionRunning(PEActionType.Parachute)
				   	|| Entity.motionMgr.IsActionRunning(PEActionType.Fall)
				   	|| Entity.motionMgr.IsActionRunning(PEActionType.JetPack)
			        || Entity.motionMgr.IsActionRunning(PEActionType.Sit)
			        || Entity.motionMgr.IsActionRunning(PEActionType.Repulsed)
			        || Entity.motionMgr.IsActionRunning(PEActionType.SwordAttack)
				   )
				{
					Entity.position = Vector3.Lerp(Entity.position, mNetTransInfos[(mNetTransInfos.Count > 1 ? 1 : 0)].pos, m_NetLerpSpeed * Time.deltaTime);
					if(mNetTransInfos.Count > 0)
						Entity.rotation = Quaternion.LookRotation(Quaternion.Euler(mNetTransInfos[0].rot) * Vector3.forward);
				}
				else
				{
					if(mNetTransInfos.Count > 0)
						RotateTo (Quaternion.Euler(mNetTransInfos[0].rot) * Vector3.forward);
					switch(speed)
					{
					case SpeedState.Walk:
					case SpeedState.Run:
						PEActionParamNV paramNV = PEActionParamNV.param;
						paramNV.n = (int)MoveType.Target;
						paramNV.vec = m_NetMovePos;
						Entity.motionMgr.DoAction(PEActionType.Move, paramNV);
						break;
					case SpeedState.Sprint:							
						PEActionParamNVB paramNVB = PEActionParamNVB.param;
						paramNVB.n = (int)MoveType.Target;
						paramNVB.vec = m_NetMovePos;
						paramNVB.b = true;
						Entity.motionMgr.DoAction(PEActionType.Sprint, paramNVB);
						break;
					}
				}
			}

//			if (m_NetRotationDir != Vector3.zero && null != Entity.motionMgr && !Entity.motionMgr.isInAimState 
//			    && Vector3.Angle(m_NetRotationDir, Entity.forward) > 20f
//			    && !Entity.motionMgr.IsActionRunning (PEActionType.Move) && !Entity.motionMgr.IsActionRunning (PEActionType.Sprint))
//			{
//				if(Time.time - m_NetRotationTime > m_NetRotationInterval)
//					RotateTo (m_NetRotationDir);
//			}
//			else
//				m_NetRotationTime = Time.time;
		}



		//static float XXX  = 5.0f;
		static readonly int layer = 1 << Pathea.Layer.Unwalkable
				| 1 << Pathea.Layer.SceneStatic
				| 1 << Pathea.Layer.Building
				| 1 << Pathea.Layer.NearTreePhysics
				| 1 << Pathea.Layer.AIPlayer;

		static readonly int layer1 = 1 << Pathea.Layer.Unwalkable
                | 1 << Pathea.Layer.Building
                | 1 << Pathea.Layer.NearTreePhysics
                | 1 << Pathea.Layer.AIPlayer;

        Vector3 needAvoid()
		{
			float _speed = 0.0f;
			if(speed == SpeedState.Walk)
				_speed = m_DefaultWalkSpeed;
			else if(speed == SpeedState.Run)
				_speed = m_DefaultRunSpeed;
			else if(speed == SpeedState.Sprint)
				_speed = m_DefaultSprintSpeed;

			Vector3 avoid = Vector3.zero;


            float dis = Mathf.Max(1.5f, _speed * Time.deltaTime * 5.0f);
			Collider[] colliders = Physics.OverlapSphere(Entity.peTrans.position, Entity.peTrans.radius + dis, layer);
			for (int i = 0; i < colliders.Length; i++)
			{
				Collider collider = colliders[i];
				
				if (collider.transform.IsChildOf(transform))
					continue;
				
				if (Entity.target != null
				    && Entity.target.GetAttackEnemy() != null
				    && Entity.target.GetAttackEnemy().trans != null
				    && collider.transform.IsChildOf(Entity.target.GetAttackEnemy().trans))
					continue;
				
				if (Entity.target != null && Entity.target.Treat != null && collider.transform.IsChildOf(Entity.target.Treat.transform))
					continue;
				
				avoid += Vector3.ProjectOnPlane((Entity.peTrans.position - collider.transform.position), Vector3.up).normalized;

			}
			return avoid;
		}

		void HumanCalculateAvoid(Vector3 movement)
		{
			Vector3 avoid = Vector3.zero;

             bool canAvoid = true;
            if(Entity.NpcCmpt != null)
            {
                Collider[] colliders = Physics.OverlapSphere(m_MoveDestination, 5.0f, layer);
                canAvoid = colliders == null ? true : colliders.Length <= 0;
            }
             //
            if (!canAvoid || !m_Avoid || movement == Vector3.zero)
				m_CurAvoidDirection = Vector3.zero;
			else
			{
				float dis = m_DefaultRunSpeed * Time.deltaTime * 5;
				Vector3 point1 = Entity.position;
				Vector3 point2 = Entity.position + Entity.bounds.size.y*Vector3.up;
				float radius = Entity.bounds.extents.x + 0.1f;
				float distance = Entity.bounds.extents.z + dis;

                int castLayer = Entity.proto == EEntityProto.Monster ? layer1 : layer;

                Vector3 dir = Vector3.zero;
				RaycastHit[] hitInfos = Physics.CapsuleCastAll(point1, point2, radius, movement, distance, castLayer);
				if(null == hitInfos) return;
				for (int i = 0; i < hitInfos.Length; i++)
				{
					Collider collider = hitInfos[i].collider;
					if (null == collider || collider.transform.IsChildOf(transform))
						continue;

                    PeEntity _entity = collider.GetComponentInParent<PeEntity>();
                    if (_entity != null && m_MoveDestination != Vector3.zero)
                    {
						Bounds bound = new Bounds(_entity.position, _entity.bounds.size);
                        Vector3 destination = m_MoveDestination;
                        destination.y = bound.center.y;
                        if (bound.Contains(destination))
                            continue;
                    } 
                    //Bounds bound = collider.bounds;
                    //if(m_MoveDestination != Vector3.zero && bound.Contains(m_MoveDestination))
                    //    continue;
					
					if (Entity.target != null
					    && Entity.target.GetAttackEnemy() != null
					    && Entity.target.GetAttackEnemy().trans != null
					    && collider.transform.IsChildOf(Entity.target.GetAttackEnemy().trans))
						continue;
					
					dir = Entity.position - collider.transform.position;
					//dir = hitInfos[i].normal;
					dir.y = 0.0f;
					avoid += dir.normalized;
				}
				
				if (avoid != Vector3.zero)
				{
					if(m_CurAvoidDirection == Vector3.zero)
						m_CurAvoidDirection = avoid.normalized;
					else
						m_CurAvoidDirection = Util.ConstantSlerp(m_CurAvoidDirection, avoid, Time.deltaTime * 120.0f);
				}
				else
				{
					if (m_CurAvoidDirection != Vector3.zero)
					{
						m_CurAvoidDirection = Util.ConstantSlerp(m_CurAvoidDirection, movement, Time.deltaTime * 120.0f);
						
						if (Vector3.Angle(m_CurAvoidDirection, movement) < 5.0f)
							m_CurAvoidDirection = Vector3.zero;
					}
					//m_CurAvoidDirection = Vector3.Lerp(m_CurAvoidDirection, Vector3.zero, Time.deltaTime);
				}
			}
		}
		
		void UpdatePathfinding()
		{
			m_MoveRequest = Vector3.zero;
			if(!Entity.viewCmpt.hasView || m_NetMove)
			{
				m_MoveDestination = Vector3.zero;
				return;
			}
			
			if(m_MoveToModel)
			{
				Vector3 curDestination = m_MoveDestination;
				if (curDestination != Vector3.zero && m_PhyCtrl != null)
				{
					if (state == Pathea.MovementState.Ground)
					{
						if (PETools.PEUtil.SqrMagnitudeH(m_PhyCtrl.transform.position, curDestination) < 0.0625f) // 0.25*0.25
							curDestination = Vector3.zero;
					}
					else
					{
						if (PETools.PEUtil.SqrMagnitude(m_PhyCtrl.transform.position, curDestination) < 4) //2*2
							curDestination = Vector3.zero;
					}
				}
				
				if (m_Pathfinder != null)
					m_Pathfinder.SetTargetposition(curDestination);
				
				if (curDestination != Vector3.zero)
				{
					Vector3 movement = curDestination - Entity.position;
					
					if (m_Param == null || movement.sqrMagnitude >= MoveParam.AutoMoveStopSqrDis*MoveParam.AutoMoveStopSqrDis)
					{
						if (m_Pathfinder != null && AstarPath.active != null && state != MovementState.Water)
							m_MoveRequest = m_Pathfinder.CalculateVelocity(Entity.position);
						
						if (m_MoveRequest == Vector3.zero)
                            m_MoveRequest = movement;
					}
				
					if (m_MoveRequest != Vector3.zero)
	                {
						if(Vector3.Angle(m_MoveRequest, Vector3.up) < 30f)
							m_MoveRequest = Vector3.Slerp(Vector3.up, Entity.forward, 0.333f);

						if(state != MovementState.Water && !Entity.motionMgr.IsActionRunning(PEActionType.Glider))
							m_MoveRequest = Vector3.ProjectOnPlane(m_MoveRequest, Vector3.up);
					
						Profiler.BeginSample("HumanCalculateAvoid");
						HumanCalculateAvoid(m_MoveRequest);
						Profiler.EndSample();

						m_MoveRequest = m_MoveRequest.normalized + m_CurAvoidDirection.normalized;

						if(m_MoveRequest == Vector3.zero)
							m_CurMovement = Vector3.zero;
						else
						{
							if(m_CurMovement == Vector3.zero)
								m_CurMovement = Entity.forward;
							else
								m_CurMovement = Util.ConstantSlerp(m_CurMovement, m_MoveRequest, 180f * Time.deltaTime);;
						}

                        Debug.DrawRay(Entity.position + Vector3.up, m_MoveRequest * 10.0f, Color.blue);
                        Debug.DrawRay(Entity.position + Vector3.up, m_CurMovement * 10.0f, Color.red);
                        Debug.DrawLine(Entity.position, curDestination, Color.yellow);

						//MoveDir(m_CurMovement, speed, true);
						MoveDir(m_MoveRequest, speed, true);
	                }
	                else
	                {
						m_CurAvoidDirection = Vector3.zero;
	                    Entity.motionMgr.EndImmediately(PEActionType.Sprint);
	                    Entity.motionMgr.EndImmediately(PEActionType.Move);
	                }
				}
				else
				{
					m_CurAvoidDirection = Vector3.zero;
					Entity.motionMgr.EndImmediately(PEActionType.Sprint);
					Entity.motionMgr.EndImmediately(PEActionType.Move);
				}	
			}
        }
		
		void UpdateAnimState()
		{
			if(null == Entity.animCmpt || null == Entity.animCmpt.animator || !Entity.hasView)
				return;
			Animator anim = Entity.animCmpt.animator;

			anim.SetFloat("FirstPerson", firstPersonCtrl ? 1f : 0);
			anim.SetBool("OnGround", (null != m_PhyCtrl) ? m_PhyCtrl.grounded : (state == MovementState.Ground));
			anim.SetFloat("InWater", Mathf.Lerp(anim.GetFloat("InWater"),
			                                               (null != m_PhyCtrl && m_PhyCtrl.spineInWater) ? 1f : 0f, m_WaterStateLerpF * Time.deltaTime)); 

			float moveStyleAngle = Mathf.Deg2Rad * (float)style * 20f;
			Vector3 currentDir = Vector3.zero;
			currentDir.x = anim.GetFloat("MoveStyleH");
			currentDir.y = anim.GetFloat("MoveStyleV");

			Vector3 targetDir = Vector3.zero;

			if(moveStyleAngle > 0)
			{
				targetDir.x = Mathf.Cos(moveStyleAngle);
				targetDir.y = Mathf.Sin(moveStyleAngle);
			}

			targetDir = Vector3.Lerp(currentDir, targetDir, AnimLerpSpeed * Time.deltaTime);

			anim.SetFloat("MoveStyleH", targetDir.x);
			anim.SetFloat("MoveStyleV", targetDir.y);

			anim.SetFloat("MoveStyle", Mathf.Lerp(anim.GetFloat("MoveStyle"), (float)style, AnimLerpSpeed * Time.deltaTime));

			float forwardAngle = (null != m_PhyCtrl)? m_PhyCtrl.forwardGroundAngle : 0;
			if(null != m_GroundIk)
				m_GroundIk.solver.maxFootRotationAngle = m_ForwardAngleFootRotate.Evaluate(forwardAngle);

			anim.SetFloat("ForwardAngle", Mathf.Lerp(anim.GetFloat("ForwardAngle"), forwardAngle, m_AngleAnimLerpF * Time.deltaTime)); 
			if(null != m_PhyCtrl && null != Entity.peTrans		
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Move) 
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Sprint) 
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Repulsed)
			   && !Entity.motionMgr.IsActionRunning(PEActionType.JetPack)
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Jump)
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Parachute)
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Fall)
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Glider)
			   && !Entity.motionMgr.IsActionRunning(PEActionType.Step)
			   && !Entity.motionMgr.IsActionRunning(PEActionType.SwordAttack))
			{
				Vector3 LocalkMoveSpeed = Quaternion.Inverse(Entity.peTrans.rotation) * m_PhyCtrl.currentDesiredMovementDirection;
				anim.SetFloat("ForwardSpeed", Mathf.Lerp(anim.GetFloat("ForwardSpeed"), LocalkMoveSpeed.z, Entity.motionMgr.isInAimState ? 1f : AnimLerpSpeed * Time.deltaTime));
				anim.SetFloat("RightSpeed", Mathf.Lerp(anim.GetFloat("RightSpeed"), LocalkMoveSpeed.x, Entity.motionMgr.isInAimState ? 1f : AnimLerpSpeed * Time.deltaTime));
			}
		}

		void UpdateContrlerPhy()
		{
			if(PeGameMgr.IsMulti && null != m_PhyCtrl && null != m_SkEntity)
			{
				m_PhyCtrl.gravity = m_SkEntity.IsController() ? Physics.gravity.y : 0f;
				m_PhyCtrl.m_IsContrler = m_SkEntity.IsController();
			}
		}

		void UpdateSafePlace()
		{	
			if(Entity.viewCmpt.hasView)
			{
				if(PeGameMgr.IsMulti && (!m_SkEntity.IsController() || (PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand && PlayerNetwork.mainPlayer._curSceneId != -1)))
					return;
				if((Entity.position - m_LastSafePos).sqrMagnitude < 1f)
					return;
				if(CheckPosSafe(Entity.position))
				{
					m_LastSafePos = Entity.position + 0.5f * Vector3.up;
					m_UnSafePos.Clear();
				}
				else
				{
					for(int x = -1; x <= 1; x++)
					{
						for(int z = -1; z <= 1; z++)
						{
							if(CheckPosSafe(Entity.position + new Vector3(x, 0, z)))
								return;
						}
					}
					for(int i = 0; i < m_UnSafePos.Count; i++)
					{
						if((Entity.position - m_UnSafePos[i]).sqrMagnitude < 1)
							return;
					}
					if(m_UnSafePos.Count < UnSafePosCountNum)
					{
						m_UnSafePos.Add(Entity.position);
					}
					else
					{
						if(null != m_PhyCtrl)
							m_PhyCtrl.velocity = Vector3.zero;

						Entity.position = m_LastSafePos;
						if(m_LastSafePos.y < 1500f)
							m_LastSafePos.y += 1f;
						m_UnSafePos.Clear();
					}
				}
			}
		}

		bool CheckPosSafe(Vector3 pos)
		{
			if(PETools.PEUtil.CheckPositionUnderWater(pos))
				return true;

			if(null != VFVoxelTerrain.self)
				return VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 0.5f), Mathf.RoundToInt(pos.z)).Volume < 128
					&& VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y + 1.5f), Mathf.RoundToInt(pos.z)).Volume < 128;
			return false;
		}

		public void ResetNetMoveState(bool netMove)
		{
			m_NetMove = netMove;
			if(netMove)
			{
				if (null != Entity.motionMgr)
				{
					Entity.motionMgr.FreezePhyByNet(true);
					Entity.motionMgr.EndImmediately(PEActionType.Move);
					Entity.motionMgr.EndImmediately(PEActionType.Sprint);
				}
				m_Move.m_AutoRotate = false;
				m_Move.m_ApplyStopIK = false;
				m_Sprint.m_ApplyStopIK = false;
				if(null != m_PhyCtrl)
				{
					m_PhyCtrl.desiredMovementDirection = Vector3.zero;
					m_PhyCtrl.CancelMoveRequest();
				}
				if(null != Entity.netCmpt && null != Entity.netCmpt.transform)
				{
					for(int i = 0; i < mNetTransInfos.Count; ++i)
						RecycleNetTranInfo(mNetTransInfos[i]);
					mNetTransInfos.Clear();
					NetTranInfo netTransInfo = GetNetTransInfo();
					netTransInfo.pos = Entity.netCmpt.network.transform.position;
					netTransInfo.rot = Entity.netCmpt.network.transform.eulerAngles;
					netTransInfo.speed = speed;
					netTransInfo.contrllerTime = GameTime.Timer.Second;
					mNetTransInfos.Add(netTransInfo);
				}
			}
			else
			{
				if (null != Entity.motionMgr)
					Entity.motionMgr.FreezePhyByNet(false);
				if(null != m_PhyCtrl)
					m_PhyCtrl.netMoveSpeedScale = 1f;
				UpdateMoveSubState();
			}
		}

		IEnumerator UpdateSafePos()
		{
			if(PlayerNetwork.mainPlayer != null && (PlayerNetwork.mainPlayer._curSceneId != (int)Pathea.SingleGameStory.StoryScene.MainLand && PlayerNetwork.mainPlayer._curSceneId != -1))
				yield return new WaitForSeconds(1);
			int count = 0;
			while(null == Entity.viewCmpt || null == Entity.peTrans || null == Entity.motionMgr)
				yield return new WaitForSeconds(1);
			while(Entity.viewCmpt.hasView && count < 10)
			{
				Vector3 safePos;
				if(PETools.PE.FindHumanSafePos(Entity.position + count * 10f * Vector3.up, out safePos, 10))
				{
					Entity.position = safePos;
					Entity.motionMgr.FreezePhySteateForSystem(false);
					yield break;
				}
				else
				{
					Entity.motionMgr.FreezePhySteateForSystem(true);
					yield return new WaitForSeconds(1);
				}
				++count;
			}
		}

		public void OnMsg(EMsg msg, params object[] args)
		{
			switch (msg)
			{
			case EMsg.View_Prefab_Build:
				BiologyViewRoot viewRoot = args[1] as BiologyViewRoot;
				m_Pathfinder = viewRoot.pathFinder;
				m_PhyCtrl = viewRoot.humanPhyCtrl;
				m_Drive.ikDrive = viewRoot.ikDrive;
				m_Halt.animEffect = viewRoot.ikAnimEffectCtrl;
				m_Jump.fBBIK = viewRoot.fbbik;
				m_GroundIk = viewRoot.grounderFBBIK;
				m_Param = viewRoot.moveParam;
                m_LastSafePos = Entity.peTrans.position;
				if (null != m_PhyCtrl)
				{
					m_PhyCtrl.gravity = Physics.gravity.y;
					m_PhyCtrl.m_IsContrler = true;
					m_Move.phyCtrl = m_PhyCtrl;
					m_Sprint.phyCtrl = m_PhyCtrl;
					m_Rotate.phyMotor = m_PhyCtrl;
					m_Jump.phyMotor = m_PhyCtrl;
					m_Step.phyMotor = m_PhyCtrl;
					m_Fall.phyMotor = m_PhyCtrl;
					m_Halt.phyMotor = m_PhyCtrl;
					m_ClimbLadder.m_PhyCtrl = m_PhyCtrl;
//					m_PhyCtrl.freezeUpdate = !enabled;
//					m_PhyCtrl._rigidbody.isKinematic = !enabled;
				}
				
				m_Move.m_Param = m_Param;
				m_Sprint.m_Param = m_Param;

//				StartCoroutine("UpdateSafePos");
				break;
			case EMsg.Net_Controller:
				if(null != Entity.peTrans && m_NetMove && m_NetMovePos != Vector3.zero)
				{
					Entity.position = m_NetMovePos;
					SceneMan.SetDirty(Entity.lodCmpt);
				}
				ResetNetMoveState(false);
//				StartCoroutine("UpdateSafePos");
				break;
			case EMsg.Net_Proxy:
				ResetNetMoveState(true);
				break;
			case EMsg.View_FirstPerson:
				firstPersonCtrl = (bool)args[0];
				break;
			}
		}
        public void SetIsKinematic(bool value)
        {
            m_PhyCtrl._rigidbody.isKinematic = value;
        }
    }
}
