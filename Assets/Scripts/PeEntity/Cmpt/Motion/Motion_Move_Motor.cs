using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AnimFollow;
using PETools;
using System;
using Pathea.Operate;

namespace Pathea
{
    public class Motion_Move_Motor : Motion_Move, IPeMsg
    {
        //static int VERSION = 1;
        static string ground = "";
        static string water = "";
        static string waterSurface = "";
        static string air = "";
		
		static Keyframe[] NearMoveVelocity = new Keyframe[]{new Keyframe(0f, 4f), new Keyframe(220f,4f), new Keyframe(700f, 8f)};
		static Keyframe[] LongMoveVelocity = new Keyframe[]{new Keyframe(0f, 4f), new Keyframe(220f,4f), new Keyframe(700f, 15f)};
		
		static Keyframe[] NearMoveTime = new Keyframe[]{new Keyframe(0f, 0.25f), new Keyframe(220f,0.25f), new Keyframe(700f, 0.6f)};
		static Keyframe[] LongMoveTime = new Keyframe[]{new Keyframe(0f, 0.25f), new Keyframe(220f,0.25f), new Keyframe(700f, 0.7f)};

		static Keyframe[] MoveStopTime = new Keyframe[]{new Keyframe(0f, 4f)};
		static Keyframe[] MoveWentflyTime = new Keyframe[]{new Keyframe(0f, 5f)};

        float m_gravity = -1.0f;

        MovementField m_Field = MovementField.None;

        public MovementField Field
        {
            get { return m_Field; }
            set { m_Field = value; }
        }

        bool m_CanMove = true;

        Vector3 m_MoveDirection;
        Vector3 m_MoveDestination;
        Vector3 m_RotDirection;

        PEMotor m_Motor;
        PEPathfinder m_Path;

        PeTrans m_PeTrans;
        AnimatorCmpt m_Animator;
        SkAliveEntity m_Attribute;
        TargetCmpt m_target;

        //MovementLimiter m_Limiter;

        BeatParam m_Param;

        float m_Speed;
        float m_CurrentSpeed;
        Vector3 m_CurMovement;
        Vector3 m_CurMovementDirection;
        Vector3 m_CurFaceDirection;
        //Vector3 m_CurMoveDirection;
        Vector3 m_CurMoveDestination;
        Vector3 m_CurAvoidDirection;
        float m_LastMoveTime;
        float m_SpeedTime;
        float m_SpeedScale;

        bool m_Proxy;
        Vector3 m_NetPos;
        Vector3 m_NetRot;

        public float CurrentSpeed
        {
            get { return m_CurrentSpeed; }
        }

        public PEMotor motor
        {
            get { return m_Motor; }
        }

        public override MovementState state
        {
            set
            {
                if (m_State != value)
                {
                    if (m_State != MovementState.None)
                        OnMovementStateExit(m_State);

                    m_State = value;

                    if (m_State != MovementState.None)
                        OnMovementStateEnter(m_State);
                }
            }

            get { return m_State; }
        }

        public override Vector3 velocity
        {
            get
            {
                return m_Motor != null ? m_Motor.velocity : Vector3.zero;
            }
        }

        public override Vector3 movement
        {
            get
            {
                return m_CurMovement;
            }
        }

        public override float gravity
        {
            get
            {
                if (m_Motor != null)
                    m_gravity = m_Motor.gravity;

                return m_gravity;
            }			
			set
			{
				if (m_Motor != null)
					m_Motor.gravity = value;
				
				m_gravity = value;
			}
        }

        public override void Stop()
        {
            m_MoveDestination = Vector3.zero;
            m_MoveDirection = Vector3.zero;
            m_RotDirection = Vector3.zero;

            if (m_Motor != null)
                m_Motor.Stop();
        }

        #region private function

        bool CanMove()
        {
            return m_CanMove && !Entity.isRagdoll;
        }

        void OnMovementStateEnter(MovementState state)
        {
            switch (state)
            {
                case MovementState.None:
                    break;
                case MovementState.Ground:
                    m_Animator.SetBool(ground, true);
                    break;
                case MovementState.Water:
                    m_Animator.SetBool(water, true);
                    break;
                case MovementState.WaterSurface:
                    m_Animator.SetBool(waterSurface, true);
                    break;
                case MovementState.Air:
                    m_Animator.SetBool(air, true);
                    break;
                default:
                    break;
            }
        }

        void OnMovementStateTick(MovementState state)
        {

        }

        void OnMovementStateExit(MovementState state)
        {
            switch (state)
            {
                case MovementState.None:
                    break;
                case MovementState.Ground:
                    //m_Animator.SetBool(ground, false);
                    break;
                case MovementState.Water:
                    //m_Animator.SetBool(water, false);
                    break;
                case MovementState.WaterSurface:
                    //m_Animator.SetBool(waterSurface, false);
                    break;
                case MovementState.Air:
                    //m_Animator.SetBool(air, false);
                    break;
                default:
                    break;
            }
        }

        void UpdateMovementState()
        {
            if (VFVoxelWater.self != null && VFVoxelWater.self.IsInWater(m_PeTrans.headTop))
                state = MovementState.Water;
            else if (VFVoxelWater.self != null && VFVoxelWater.self.IsInWater(m_PeTrans.position))
                state = MovementState.WaterSurface;
            else
            {
                if (m_Motor != null)
                {
                    if (m_Motor.grounded)
                        state = MovementState.Ground;
                    else
                        state = MovementState.Air;
                }
            }

            if (state != MovementState.None)
                OnMovementStateTick(m_State);
        }

        void UpdateRotation()
        {
            if (m_Motor == null)
                return;

            if (m_CurFaceDirection != Vector3.zero)
                m_Motor.desiredFacingDirection = m_CurFaceDirection.normalized;
            else if(m_CurMovementDirection != Vector3.zero)
                m_Motor.desiredFacingDirection = m_CurMovementDirection.normalized;
            else
                m_Motor.desiredFacingDirection = Vector3.zero;

            //Vector3 direction = m_CurFaceDirection != Vector3.zero ? m_CurFaceDirection : m_CurMovementDirection;
            Vector3 direction = m_Motor.desiredFacingDirection;

            //Debug.DrawRay(m_Motor.transform.position, direction.normalized * 16f, Color.blue);

            Vector3 velocity = Quaternion.Inverse(m_Motor.transform.rotation) * direction;
            if(PEUtil.SqrMagnitudeH(velocity) > 0.05f*0.05f)
                m_Animator.SetFloat("Angle", Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f);
            else
                m_Animator.SetFloat("Angle", 0.0f);

            PEMotorPhysics motorPhysic = m_Motor as PEMotorPhysics;
            if (motorPhysic != null)
            {
                float rotationSpeed = m_Attribute.GetAttribute(AttribType.RotationSpeed);

                if (motorPhysic.velocity.sqrMagnitude > 0.25f * 0.25f)
                {
                    if (m_SpeedState == SpeedState.Walk)
                        rotationSpeed = motorPhysic.walkSmoothSpeed;
                    else
                        rotationSpeed = motorPhysic.runSmoothSpeed;
                }

                if (m_Animator != null)
                {
                    float rotAnimSpeed = m_Animator.GetFloat("RotationSpeed");
                    rotationSpeed = rotAnimSpeed > 0.0f ? rotAnimSpeed : rotationSpeed;
                }

                motorPhysic.maxRotationSpeed = rotationSpeed;
            }
        }

        void CalculateDestination()
        {
            m_CurMoveDestination = m_MoveDestination;

            if (m_CurMoveDestination != Vector3.zero && m_Motor != null)
            {
                float sqrDis = PEUtil.SqrMagnitude( m_Motor.transform.position, m_CurMoveDestination, m_Motor.gravity<PETools.PEMath.Epsilon);

                Enemy enemy = m_target != null ? m_target.GetAttackEnemy() : null;
                if(enemy != null && PEUtil.SqrMagnitude(m_CurMoveDestination, enemy.position, false) < 0.25f*0.25f)
                {
                    if (m_Motor.gravity < PETools.PEMath.Epsilon)
                        sqrDis = enemy.SqrDistance;
                    else
                        sqrDis = enemy.SqrDistanceXZ;
                }

                if (sqrDis < 0.1f * 0.1f)
                    m_CurMoveDestination = Vector3.zero;
            }
        }

        void CalculateSpeed()
        {
            m_SpeedScale = 1.0f;

            float curSpeed = m_Speed;

            if (m_CurrentSpeed > PETools.PEMath.Epsilon)
                m_SpeedTime = 0.0f;
            else if (m_CurMovement == Vector3.zero)
                m_SpeedTime = Time.time;

            if (m_CurMoveDestination != Vector3.zero && m_MoveDirection == Vector3.zero)
            {
                float d = 0.0f;

                float targetRadius = 0.0f;
                Enemy enemy = m_target != null ? m_target.GetAttackEnemy() : null;
                if (!Enemy.IsNullOrInvalid(enemy))
                    targetRadius = enemy.radius + m_PeTrans.radius;

                d = PEUtil.Magnitude(m_Motor.transform.position, m_CurMoveDestination, Entity.gravity < PETools.PEMath.Epsilon);
                d = Mathf.Max(0.0f, d - targetRadius);

                if (m_SpeedState == SpeedState.Run && enemy != null)
                {
                    float walkSpeed = m_Attribute.GetAttribute(AttribType.WalkSpeed);
                    float runSpeed = m_Attribute.GetAttribute(AttribType.RunSpeed);

                    if (Field == MovementField.Sky)
                        runSpeed *= Mathf.Lerp(1f, 2f, Mathf.Max(0f, enemy.DistanceXZ - 32f) / 64f);

                    curSpeed = Mathf.Lerp(walkSpeed, runSpeed, Mathf.InverseLerp(0.5f, 2.0f, d));
                }

                Vector3 v1 = Vector3.ProjectOnPlane(m_Motor.transform.forward, Vector3.up);
                Vector3 v2 = Vector3.ProjectOnPlane(m_CurMovementDirection, Vector3.up);
                float angle = Vector3.Angle(v1, v2);

                //if (d < 5.0f)
                    m_SpeedScale *= Mathf.Lerp(1.0f, 0.5f, angle / 150.0f);

                curSpeed *= m_SpeedScale;

                if (m_Motor is PEMotorPhysics)
                    curSpeed = Mathf.Lerp(0.1f, curSpeed, (Time.time - m_SpeedTime));
            }

            float fSpeed = m_CurrentSpeed > PETools.PEMath.Epsilon ? m_CurrentSpeed : curSpeed;

            m_Motor.maxVelocityChange = fSpeed;
            m_Motor.maxForwardSpeed = fSpeed;
            m_Motor.maxSidewaysSpeed = fSpeed;
            m_Motor.maxBackwardsSpeed = fSpeed;
        }

        void CalculateMovement()
        {
            CalculateDestination();

            if (AstarPath.active != null && m_Path != null)
                m_Path.SetTargetposition(m_CurMoveDestination);

            Vector3 movement = Vector3.zero;
            if (m_MoveDirection != Vector3.zero)
                movement = m_MoveDirection;
            else
            {
                if (m_CurMoveDestination == Vector3.zero)
                    movement = Vector3.zero;
                else
                {
                    if (m_Path != null && m_Motor.gravity > PETools.PEMath.Epsilon && AstarPath.active != null)
                        movement = m_Path.CalculateVelocity(m_Motor.transform.position);

                    if (movement == Vector3.zero)
                        movement = m_CurMoveDestination - m_Motor.transform.position;

                }
            }

            CalculateAvoid(movement);

            movement = movement.normalized + m_CurAvoidDirection.normalized;

            if (movement == Vector3.zero)
                m_CurMovementDirection = Vector3.zero;
            else
            {
                if (m_CurMovementDirection == Vector3.zero)
                    m_CurMovementDirection = movement;
                else
                {
                    //m_CurMovementDirection = Util.ConstantSlerp(m_CurMovementDirection, movement, 90.0f * Time.deltaTime);

                    Vector3 v1 = m_CurMovementDirection;
                    v1.y = 0.0f;
                    Vector3 v2 = movement;
                    v2.y = 0.0f;

                    Vector3 v3 = Util.ConstantSlerp(v1, v2, 90 * Time.deltaTime);

                    m_CurMovementDirection = Quaternion.FromToRotation(v2, v3) * movement;
                }
            }

            if (m_CurMovementDirection == Vector3.zero || !CanMove())
                m_CurMovement = Vector3.zero;
            else
            {
                if (m_MoveDirection != Vector3.zero)
                    m_CurMovement = m_CurMovementDirection;
                else
                {
                    float rotateSpeed = 90f;

                    PEMotorPhysics motorPhysic = m_Motor as PEMotorPhysics;
                    if(motorPhysic != null)
                        rotateSpeed = m_SpeedState == SpeedState.Walk ? motorPhysic.walkSmoothSpeed : motorPhysic.runSmoothSpeed;

                    Vector3 v1 = m_Motor.transform.forward;
                    v1.y = 0.0f;

                    Vector3 v2 = m_CurMovementDirection;
                    v2.y = 0.0f;

                    Vector3 v3 = Util.ConstantSlerp(v1, v2, rotateSpeed * Time.deltaTime);

                    m_CurMovement = Quaternion.FromToRotation(v2, v3) * m_CurMovementDirection;
                }
            }

            if(m_CurMoveDestination != Vector3.zero)
                Debug.DrawLine(m_Motor.transform.position, m_CurMoveDestination, Color.yellow);

            Debug.DrawRay(m_Motor.transform.position, m_CurMovement.normalized * (10.0f + m_PeTrans.radius), Color.red);
            Debug.DrawRay(m_Motor.transform.position, m_CurMovementDirection.normalized * (6.0f + m_PeTrans.radius), Color.blue);
        }

        bool Match(RaycastHit hitInfo)
        {
            if (hitInfo.collider == null || hitInfo.collider.isTrigger)
                return false;

            if (hitInfo.transform.IsChildOf(transform))
                return false;

            if (m_target != null
                && m_target.GetAttackEnemy() != null
                && m_target.GetAttackEnemy().skTarget != null
                && hitInfo.collider.transform.IsChildOf(m_target.GetAttackEnemy().skTarget.transform))
                return false;

            return true;
        }

		static readonly	int layer = 1 << Pathea.Layer.AIPlayer
				| 1 << Pathea.Layer.Player
				| 1 << Pathea.Layer.Unwalkable
				| 1 << Pathea.Layer.NearTreePhysics;
		
		static readonly int voxelLayer = 1 << Pathea.Layer.Unwalkable
			| 1 << Pathea.Layer.NearTreePhysics
				| 1 << Pathea.Layer.SceneStatic
				| 1 << Pathea.Layer.Building
				| 1 << Pathea.Layer.VFVoxelTerrain;

        void CalculateAvoid(Vector3 movement)
        {
			if(null == m_PeTrans || null == m_Motor) return;

            Vector3 avoid = Vector3.zero;

            if (movement == Vector3.zero || m_MoveDirection != Vector3.zero)
                m_CurAvoidDirection = Vector3.zero;
            else
            {
                float dis = m_Motor.maxForwardSpeed * Time.deltaTime * 10f;
                Vector3 point1 = m_PeTrans.position;
                Vector3 point2 = m_PeTrans.position + m_PeTrans.bound.size.y*Vector3.up;
                float radius = m_PeTrans.bound.extents.x + 0.5f;
                float distance = m_PeTrans.bound.extents.z + dis + 1.0f;

                if (Field == MovementField.Land)
                {

                    Vector3 dir = Vector3.zero;
                    RaycastHit[] hitInfos = Physics.CapsuleCastAll(point1, point2, radius, movement, distance, layer);
                    for (int i = 0; i < hitInfos.Length; i++)
                    {
                        Collider collider = hitInfos[i].collider;

						if(null == collider) 
							continue;

                        if (m_CurMoveDestination != Vector3.zero)
                        {
                            Bounds bound = collider.bounds;
                            Vector3 destination = m_CurMoveDestination;

                            PeEntity entity = collider.gameObject.GetComponentInParent<PeEntity>();
                            if (entity != null)
                            {
                                bound = entity.bounds;
								if(null == entity.tr)
									continue;
                                destination = entity.tr.InverseTransformPoint(destination);
                            }
                            else
                            {
                                Operation_Multiple opreate = collider.gameObject.GetComponentInParent<Operation_Multiple>();
								if (opreate != null && !opreate.Equals(null))
                                {
                                    bound = opreate.LocalBounds;
                                    destination = opreate.transform.InverseTransformPoint(destination);
                                }
                            }

                            bound.Expand(2.0f);
                            destination.y = bound.center.y;
                            if (bound.Contains(destination))
                                continue;
                        }

                        if (collider.transform.IsChildOf(transform))
                            continue;

                        if (m_target != null
                            && m_target.GetAttackEnemy() != null
                            && m_target.GetAttackEnemy().trans != null
                            && collider.transform.IsChildOf(m_target.GetAttackEnemy().trans))
                            continue;

                        if (m_target != null && m_target.Treat != null && collider.transform.IsChildOf(m_target.Treat.transform))
                            continue;

                        dir = m_PeTrans.position - collider.transform.position;
                        //dir = hitInfos[i].normal;
                        dir.y = 0.0f;

                        avoid += dir.normalized;
                    }
                }
                else if (Field == MovementField.Sky || Field == MovementField.water)
                {

                    RaycastHit hitInfo;
                    if (Physics.CapsuleCast(point1, point2, radius, movement, out hitInfo, distance, voxelLayer))
                    {
                        //avoidDir = m_PeTrans.position - hitInfo.point;
                        avoid += hitInfo.normal;
                    }
                }

                if (avoid != Vector3.zero)
                {
                    if(m_CurAvoidDirection == Vector3.zero)
                        m_CurAvoidDirection = avoid.normalized;
                    else
                        m_CurAvoidDirection = Util.ConstantSlerp(m_CurAvoidDirection, avoid, Time.deltaTime * 90.0f);
                }
                else
                {
                    //if (m_CurAvoidDirection != Vector3.zero)
                    //{
                    //    m_CurAvoidDirection = Util.ConstantSlerp(m_CurAvoidDirection, movement, Time.deltaTime * 90.0f);

                    //    if (Vector3.Angle(m_CurAvoidDirection, movement) < 5.0f)
                    //        m_CurAvoidDirection = Vector3.zero;
                    //}

                    m_CurAvoidDirection = Vector3.Lerp(m_CurAvoidDirection, Vector3.zero, 2*Time.deltaTime);
                    if (m_CurAvoidDirection.sqrMagnitude < 0.15f * 0.15f)
                        m_CurAvoidDirection = Vector3.zero;
                }
            }

            if (m_CurAvoidDirection != Vector3.zero)
                Debug.DrawRay(m_Motor.transform.position, m_CurAvoidDirection.normalized * 6.5f, Color.green);
        }

        void UpdatePosition()
        {
            if (m_Motor == null)
                return;

//            Vector3 movement = Vector3.zero;
//            Vector3 rotation = Vector3.zero;

            //m_CurMoveDirection = m_MoveDirection;
            m_CurFaceDirection = m_RotDirection;
            m_CurMoveDestination = m_MoveDestination;

            CalculateSpeed();
            CalculateMovement();

            m_Motor.desiredMovementDirection = Quaternion.Inverse(m_Motor.transform.rotation) * m_CurMovement.normalized;

        }

        void UpdateStuck()
        {
            if (m_CurMovementDirection == Vector3.zero || velocity.sqrMagnitude > 0.15f * 0.15f)
                m_LastMoveTime = Time.time;
        }

        void UpdateNetMovement()
        {
            m_PeTrans.position = Vector3.Lerp(m_PeTrans.position, m_NetPos, 5.0f * Time.deltaTime);
        }

        void UpdateNetRot()
        {
            m_PeTrans.rotation = Quaternion.Slerp(m_PeTrans.rotation, Quaternion.Euler(m_NetRot), 5.0f*Time.deltaTime);
        }

        #endregion

        #region Inherited component
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (PeGameMgr.IsMulti && m_Proxy)
            {
                UpdateNetMovement();
                UpdateNetRot();
            }
            else
            {
                if (Entity.hasView)
                {
                    UpdateMovementState();

                    UpdatePosition();

                    UpdateRotation();

                    UpdateStuck();
                }
            }
        }

        public override void Start()
        {
			base.Start ();
            m_SpeedState = SpeedState.None;
            m_PeTrans = Entity.peTrans;
            m_Animator = Entity.GetCmpt<AnimatorCmpt>();
            m_Attribute = Entity.GetCmpt<SkAliveEntity>();
            m_target = GetComponent<TargetCmpt>();
            //m_Limiter = new MovementLimiter(this, m_Field);

            if (m_Attribute != null)
                m_Attribute.deathEvent += OnDeath;
        }

        public override bool grounded
        {
            get
            {
                return m_Motor != null ? m_Motor.grounded : false;
            }
        }

        public override bool Stucking(float time)
        {
            if (Entity.isRagdoll)
                return false;

            if (Time.time - m_LastMoveTime > time || !Entity.hasView)
                return true;
            else
                return false;
        }

        public override void ApplyForce(Vector3 power, ForceMode mode)
        {
//            float prob = m_Param != null ? m_Param.repulsedProb : 1.0f;
            if (m_Motor != null || Entity.GetAttribute(AttribType.Rigid) < 0)
                m_Motor.desiredMovementEffect = power;
        }

        #endregion

        #region Inherited motion move
        public override SpeedState speed
        {
            set
            {
                if (m_SpeedState != value)
                {
                    m_SpeedState = value;

                    if (m_Attribute != null)
                    {
                        switch (m_SpeedState)
                        {
                            case SpeedState.None:
                                m_Speed = 0.0f;
                                break;
                            case SpeedState.Walk:
                                m_Speed = m_Attribute.GetAttribute(AttribType.WalkSpeed);
                                break;
                            case SpeedState.Run:
                                m_Speed = m_Attribute.GetAttribute(AttribType.RunSpeed);
                                break;
                            case SpeedState.Sprint:
                                m_Speed = m_Attribute.GetAttribute(AttribType.SprintSpeed);
                                break;
                            case SpeedState.Retreat:
                                m_Speed = m_Attribute.GetAttribute(AttribType.WalkSpeed);
                                break;
                            default:
                                m_Speed = 0.0f;
                                break;
                        }
                    }

                    if (m_Motor != null && (m_Motor is PEMotorAnimator))
                        (m_Motor as PEMotorAnimator).speedState = m_SpeedState;
                }
            }
        }

        public override void Move(Vector3 dir, SpeedState state = SpeedState.Walk)
        {
            m_MoveDirection = dir;

            speed = state;

            if (dir != Vector3.zero)
            {
                m_MoveDestination = Vector3.zero;
                m_RotDirection = Vector3.zero;
            }
        }

        public override void SetSpeed(float Speed)
        {
            m_CurrentSpeed = Speed;
        }

        public override void MoveTo(Vector3 targetPos, SpeedState state = SpeedState.Walk,bool avoid = true)
        {
            speed = state;
            m_MoveDestination = targetPos;

            if (m_MoveDestination != Vector3.zero)
            {
                m_MoveDirection = Vector3.zero;
                m_RotDirection = Vector3.zero;
            }
        }

		public override void NetMoveTo(Vector3 position, Vector3 moveVelocity, bool immediately = false)
        {
            m_NetPos = position;
			if(immediately && m_PeTrans != null)
				m_PeTrans.position = position;
        }

        public override void NetRotateTo(Vector3 eulerAngle)
        {
            m_NetRot = eulerAngle;
        }

        public override void RotateTo(Vector3 targetDir)
        {
            m_RotDirection = targetDir;
        }

        public override void Jump()
        {
        }
        #endregion

        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.View_Prefab_Build:
					BiologyViewRoot viewRoot = args[1] as BiologyViewRoot;
				    m_Motor = viewRoot.motor;
                    m_Steer = viewRoot.steerAgent;
                    m_Path = viewRoot.pathFinder;
                    m_Param = viewRoot.beatParam;
					InitParam();

                    if (m_Motor is PEMotorPhysics)
                        (m_Motor as PEMotorPhysics).Init(Entity);

                    m_LastMoveTime = Time.time;
                    break;

                case EMsg.Net_Controller:
                    m_Proxy = false;
                    break;

                case EMsg.Net_Proxy:
                    m_Proxy = true;
                    break;
                case EMsg.State_Die:
                    if (m_Motor != null)
                        m_Motor.Reset();
                    enabled = false;
                    break;
                case EMsg.State_Revive:
                    enabled = true;
                    break;
            }
        }

        void OnDeath(SkillSystem.SkEntity sk1, SkillSystem.SkEntity sk2)
        {
            enabled = false;
        }

        void OnEnable()
        {
            if (m_Motor != null)
            {
                m_Motor.enabled = true;

                if(m_Motor is PEMotorAnimator && Entity.Rigid != null)
                {
                    Entity.Rigid.useGravity = true;
                }
            }
        }

        void OnDisable()
        {
            if (m_Motor != null)
            {
                m_Motor.enabled = false;

                if(m_Motor is PEMotorAnimator && Entity.Rigid != null)
                {
                    Entity.Rigid.useGravity = false;
                }
            }
        }

		void InitParam()
		{
			if(null != m_Param)
			{
                if(Entity.proto == EEntityProto.Monster)
                {
                    MonsterProtoDb.Item protoData = MonsterProtoDb.Get(Entity.ProtoID);
                    if(null != protoData)
                    {
						if(protoData.RepulsedType > 0 && Entity.GetAttribute(AttribType.ThresholdRepulsed) > 550f)
							Entity.SetAttribute(AttribType.ThresholdRepulsed, 550f);

                        if (protoData.RepulsedType == 2)
                        {
                            if (Entity.GetAttribute(AttribType.ThresholdRepulsed) > 130f)
                                Entity.SetAttribute(AttribType.ThresholdRepulsed, 130f);

                            m_Param.m_ForceToVelocity.keys = LongMoveVelocity;
                            m_Param.m_ForceToMoveTime.keys = LongMoveTime;
                        }
                        else
                        {
                            m_Param.m_ForceToVelocity.keys = NearMoveVelocity;
                            m_Param.m_ForceToMoveTime.keys = NearMoveTime;
                        }
                        m_Param.m_ApplyMoveStopTime.keys = MoveStopTime;
                        m_Param.m_WentflyTimeCurve.keys = MoveWentflyTime;
                    }
                }

			}
		}
    }

    public class MovementLimiter
    {
        public delegate bool LifeFieldDelegate(ref Vector3 velocity);

        PeTrans m_Trans;
        Motion_Move_Motor m_MotionMove;

        MovementField m_MovementField;

        #region constructor
        public MovementLimiter(Motion_Move_Motor motor, MovementField movementField)
        {
            m_MotionMove = motor;
            m_Trans = m_MotionMove.GetComponent<PeTrans>();
            m_MovementField = movementField;
        }
        #endregion

        #region public interface

        public bool CalculateVelocity(ref Vector3 velocity)
        {
            if (m_MovementField == MovementField.Land)
                return CalculateVelocityLand(ref velocity);
            else if (m_MovementField == MovementField.water)
                return CalculateVelocityWater(ref velocity);
            else if (m_MovementField == MovementField.Sky)
                return CalculateVelocitySky(ref velocity);
            else if (m_MovementField == MovementField.Amphibian)
                return CalculateVelocityAmphibian(ref velocity);
            else if (m_MovementField == MovementField.All)
                return CalculateVelocityAll(ref velocity);
            else
                return true;
        }

        #endregion

        #region private function

        bool CalculateVelocityLand(ref Vector3 velocity)
        {
            Vector3 v = new Vector3(velocity.x, 0.0f, velocity.z);
            if(v.sqrMagnitude > 0.01f*0.01f)
            {
                Vector3 vv = v.normalized * Time.deltaTime * m_MotionMove.motor.maxForwardSpeed * 5.0f;

                Vector3 vff = m_Trans.forwardBottom;
                Vector3 vfc = m_Trans.forwardCenter;

                Vector3 nvff = vff + vv;
                Vector3 nvfc = vfc + vv;

                Vector3 vg;
                if (!PEUtil.CheckPositionOnGround(nvff, out vg, 5.0f, 5.0f, GameConfig.GroundLayer))
                    return false;
                else
                {
                    if(PEUtil.CheckPositionUnderWater(nvfc))
                    {
						if(!RandomDungenMgrData.InDungeon){
	                        if(PEUtil.CheckPositionOnGround(nvfc + v.normalized*32.0f, out vg, 128.0f, 128.0f, GameConfig.GroundLayer))
	                        {
	                            if (PEUtil.CheckPositionUnderWater(vg))
	                                return false;
							}
						}else{
							return false;
						}
                    }
                }
            }

            return true;
        }

        bool CalculateVelocitySky(ref Vector3 velocity)
        {
            Vector3 _velocity = velocity;
            if (_velocity.sqrMagnitude > 0.01f * 0.01f)
            {
                _velocity = _velocity.normalized * Time.deltaTime * m_MotionMove.motor.maxForwardSpeed * 10.0f;
                Vector3 _nextPosition = m_Trans.position + _velocity;

                float height;
                if (PEUtil.GetWaterSurfaceHeight(_nextPosition, out height))
                {
                    if(velocity.y < -PETools.PEMath.Epsilon)
                        velocity.y = 0.0f;
                }
            }

            return true;
        }

        bool CalculateVelocityWater(ref Vector3 velocity)
        {
            if (velocity.sqrMagnitude > 0.01f * 0.01f)
            {
                float maxSpeed = m_MotionMove.motor.maxForwardSpeed;
                Vector3 pos = m_Trans.trans.TransformPoint(m_Trans.bound.center) - Vector3.up * m_Trans.bound.extents.y;
                Vector3 v = pos + Vector3.up * (m_Trans.bound.size.y + 0.5f) + Vector3.up * Time.deltaTime * maxSpeed * 5;
                if (!PEUtil.CheckPositionUnderWater(v))
                {
                    if(velocity.y > PETools.PEMath.Epsilon)
                        velocity.y = 0.0f;

                    Vector3 movement = new Vector3(velocity.x, 0.0f, velocity.z);
                    if (movement.sqrMagnitude > 0.01f * 0.01f)
                    {
                        Vector3 dir = movement.normalized * maxSpeed * Time.deltaTime * 5.0f;
                        float distance = Mathf.Max(m_Trans.bound.extents.x, m_Trans.bound.extents.z) + dir.magnitude + 1.0f;
                        if (Physics.Raycast(pos, dir, distance, GameConfig.GroundLayer))
                            return false;
                    }
                }
            }

            return true;
        }

        bool CalculateVelocityAmphibian(ref Vector3 velocity)
        {
            return true;
        }

        bool CalculateVelocityAll(ref Vector3 velocity)
        {
            return true;
        }

        #endregion
    }
}
