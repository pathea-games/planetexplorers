using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;
using Pathfinding;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;

namespace Behave.Runtime.Action
{
	[BehaveAction(typeof(BTIsField), "IsField")]
	public class BTIsField : BTNormal
	{
		BehaveResult Tick(Tree sender)
		{
			if(!IsNpcFollower && !IsNpcBase && !IsNpcCampsite && !hasAnyRequest)
				return BehaveResult.Success;
			else
				return BehaveResult.Failure;
			
		}
	}

    [BehaveAction(typeof(BTIdle), "Idle")]
    public class BTIdle : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string idle = "";
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
            [BehaveAttribute]
            public float relaxProb = 0.0f;
            [BehaveAttribute]
            public float relaxTime = 0.0f;
            [BehaveAttribute]
            public string[] relax = new string[0];

            public float m_StartIdleTime;
            public float m_CurrentIdleTime;
            public float m_StartRestTime;
        }

        Data m_Data;

        Enemy m_Escape;
        Enemy m_Threat;

        public static float _IK_Aim_Time0 = 5.0f;
        public static float _Ik_Aim_Time1 = 8.0f;
        public static float _Ik_Aim_Time2 = 3.0f;
        float startIkTime = 0.0f;
        bool inIKAim = false;

		bool DoingRelax()
		{
            if (entity.commonCmpt.Race != ERace.Mankind)
                return false;

			for(int i=0;i < m_Data.relax.Length;i++)
			{
                if (entity.animCmpt != null && entity.animCmpt.animator != null 
				    && entity.animCmpt.ContainsParameter(m_Data.relax[i]) 
				    && entity.animCmpt.animator.GetBool(m_Data.relax[i]))//GetBool(m_Data.relax[i]))
					return true;
			}
			return false;
		}

        void NpcIKVeer()
        {
            if (entity.NpcCmpt != null && Time.time - startIkTime >= _IK_Aim_Time0 && Time.time - startIkTime <= _IK_Aim_Time0 + _Ik_Aim_Time1)
            {
                if (PeCreature.Instance != null
                   && PEUtil.InAimDistance(PeCreature.Instance.mainPlayer.position, position, NPCConstNum.IK_Aim_Distance_Min, NPCConstNum.IK_Aim_Distance_Max)
                   && PEUtil.InAimAngle(PeCreature.Instance.mainPlayer.peTrans.existent, existent, NPCConstNum.IK_Aim_AngleH_Idle))
                {
                    inIKAim = true;
                    SetIKTargetPos(PEUtil.CalculateAimPos(PeCreature.Instance.mainPlayer.peTrans.existent.position, position));
                    SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpspeed_0);
                    //SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTime_0);
                    SetIKActive(inIKAim);

                }

                if (inIKAim && (!PEUtil.InAimDistance(PeCreature.Instance.mainPlayer.position, position, NPCConstNum.IK_Aim_Distance_Min, NPCConstNum.IK_Aim_Distance_Max)
                    || !PEUtil.InAimAngle(PeCreature.Instance.mainPlayer.peTrans.existent, existent, NPCConstNum.IK_Aim_AngleH_Idle))
                    )
                {
                    SetIKTargetPos(PEUtil.CalculateAimPos(position + existent.forward * 3.0f, position));
                    //SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTime_0);
                    SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpspeed_0);
                }

            }
            else if (entity.NpcCmpt != null && inIKAim && Time.time - startIkTime > _IK_Aim_Time0 + _Ik_Aim_Time1 &&
                Time.time - startIkTime <= _IK_Aim_Time0 + _Ik_Aim_Time1 + _Ik_Aim_Time2)
            {
                SetIKTargetPos(PEUtil.CalculateAimPos(position + existent.forward * 3.0f, position));
                SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpspeed_0);
                //SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTime_0);
            }
            else if (entity.NpcCmpt != null && Time.time - startIkTime > _IK_Aim_Time0 + _Ik_Aim_Time1 + _Ik_Aim_Time2)
            {
                inIKAim = false;
                startIkTime = Time.time;
                SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpdefault);
                //SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTimedefault);
                SetIKActive(inIKAim);
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (hasAnyRequest || entity.Food != null || entity.IsDarkInDaytime || entity.Chat != null)
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (entity.Treat != null && !entity.Treat.IsDeath() && entity.Treat.hasView)
                return BehaveResult.Failure;

            if (!EvadePolarShield(position))
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            if (entity.NpcCmpt != null && !NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Leisure))
               return BehaveResult.Failure;
            

            m_Data.m_StartRestTime = Time.time;
            m_Data.m_StartIdleTime = Time.time;
			startIkTime = Time.time;
            inIKAim = false;
			m_Data.m_CurrentIdleTime = Random.Range(m_Data.relaxTime, m_Data.maxTime);

            StopMove();
            SetBool(m_Data.idle, true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

			SetNpcAiType(ENpcAiType.FieldNpcIdle_Idle);

            if (entity.NpcCmpt != null && !NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Leisure))
                return BehaveResult.Failure;

            if( !Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (entity.Treat != null && !entity.Treat.IsDeath() && entity.Treat.hasView)
                return BehaveResult.Failure;

            if (!EvadePolarShield(position))
                return BehaveResult.Failure;

            if (IsMotionRunning(PEActionType.HoldShield))
                EndAction(PEActionType.HoldShield);

            if (hasAnyRequest || entity.Food != null || entity.IsDarkInDaytime || entity.Chat != null)
                return BehaveResult.Failure;

            NpcIKVeer();
			if (GetBool("BehaveWaiting") || GetBool("Leisureing") || DoingRelax() || inIKAim)
                return BehaveResult.Running;

			if (GetBool("OperateMed"))
			{
				SetBool("OperateMed",false);
			}
			if (GetBool("OperateCom"))
			{
				SetBool("OperateCom",false);
			}
			if (GetBool("FixLifeboat"))
			{
				SetBool("FixLifeboat",false);
			}

            if (Time.time - m_Data.m_StartIdleTime > m_Data.m_CurrentIdleTime)
			{
				  return BehaveResult.Success;
			}

            if (Time.time - m_Data.m_StartRestTime > m_Data.relaxTime)
            {
				m_Data.m_StartRestTime = Time.time;					
				if (!GetBool("BehaveWaiting") && !GetBool("Leisureing")  && !DoingRelax() && Random.value < m_Data.relaxProb)
				{
					SetBool(m_Data.relax[Random.Range(0, m_Data.relax.Length)], true);
				}
            }


			
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartIdleTime > PETools.PEMath.Epsilon)
            {
                m_Data.m_StartRestTime = 0.0f;
                m_Data.m_StartIdleTime = 0.0f;
                m_Data.m_CurrentIdleTime = 0.0f;

                if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
                    SetBool("Interrupt", true);

                for (int i = 0; i < m_Data.relax.Length; i++) {
					if(GetBool(m_Data.relax[i]))
						SetBool(m_Data.relax[i], false);
				}

                SetBool(m_Data.idle, false);
            }

			if(entity.NpcCmpt != null && !entity.NpcCmpt.Req_Contains(EReqType.Dialogue))
            {
                SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpdefault);
                //SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTimedefault);
                SetIKActive(false);
            }
				
        }
    }


    [BehaveAction(typeof(BTIdleWeapon), "IdleWeapon")]
    public class BTIdleWeapon : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
            [BehaveAttribute]
            public float relaxProb = 0.0f;
            [BehaveAttribute]
            public float relaxTime = 0.0f;

            public float m_StartIdleTime;
            public float m_CurrentIdleTime;
            public float m_StartRestTime;
        }

        Data m_Data;

        float m_StartTime;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_StartTime < PETools.PEMath.Epsilon)
                m_StartTime = Time.time;

            if (Time.time - m_StartTime < 10.0f)
                return BehaveResult.Failure;

            if (hasAnyRequest || entity.Food != null || entity.IsDarkInDaytime || entity.Chat != null)
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_Data.m_StartRestTime = Time.time;
            m_Data.m_StartIdleTime = Time.time;
            m_Data.m_CurrentIdleTime = Random.Range(m_Data.minTime, m_Data.maxTime);

            StopMove();
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
                return BehaveResult.Running;

			SetNpcAiType(ENpcAiType.FieldNpcIdle_Idle);

            if( !Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (hasAnyRequest || entity.Food != null || entity.IsDarkInDaytime || entity.Chat != null)
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartIdleTime > m_Data.m_CurrentIdleTime)
                return BehaveResult.Success;

            if (Time.time - m_Data.m_StartRestTime > m_Data.relaxTime)
            {
                m_Data.m_StartRestTime = Time.time;

                if (Random.value < m_Data.relaxProb)
                {
                    List<IWeapon> weapons = entity.GetWeaponlist();

                    if(weapons != null && weapons.Count > 0)
                    {
                        IWeapon weapon = weapons[Random.Range(0, weapons.Count)];
                        if(weapon != null && !weapon.Equals(null) && weapon.leisures != null && weapon.leisures.Length > 0)
                        {
                            SetBool(weapon.leisures[Random.Range(0, weapon.leisures.Length)], true);
                        }
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartIdleTime > PETools.PEMath.Epsilon)
            {
                m_Data.m_StartRestTime = 0.0f;
                m_Data.m_StartIdleTime = 0.0f;
                m_Data.m_CurrentIdleTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTIdleAnimation), "IdleAnimation")]
    public class BTIdleAnimation : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public string[] relaxs = new string[0];
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (hasAttackEnemy || hasAnyRequest)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            StopMove();
            SetBool(m_Data.relaxs[Random.Range(0, m_Data.relaxs.Length)], true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTPatrol), "Patrol")]
    public class BTPatrol : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
            [BehaveAttribute]
            public float minRadius = 0.0f;
            [BehaveAttribute]
            public float maxRadius = 0.0f;
            [BehaveAttribute]
            public float minHeight = 0.0f;
            [BehaveAttribute]
            public float maxHeight = 0.0f;
            [BehaveAttribute]
            public bool spawnCenter = false;

            public float m_Time = 0.0f;
            public float m_StartPatrolTime = 0.0f;
            public Vector3 m_CurrentPatrolPosition = Vector3.zero;
        }

        Data m_Data;

        Enemy m_Escape;
        Enemy m_Threat;

        Vector3 m_PatrolPosition;
        float m_LastFollowTime;
        float m_FollowTime;

        bool m_Falied;

        Vector3 GetPatrolCenter()
        {
            return behave.PatrolMode == BHPatrolMode.SpawnCenter ? entity.spawnPos : position;
        }

        float GetMinPatrolRadius()
        {
            return behave.MinPatrolRadius > PETools.PEMath.Epsilon ? behave.MinPatrolRadius : m_Data.minRadius;
        }

        float GetMaxPatrolRadius()
        {
            return behave.MaxPatrolRadius > PETools.PEMath.Epsilon ? behave.MaxPatrolRadius : m_Data.maxRadius;
        }

        void OnPathComplete(Path path)
        {
            if(path != null)
            {
                path.Claim(path);

                if (path.vectorPath != null && path.vectorPath.Count > 0)
                {
                    Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                    if (EvadePolarShield(pos))
                    {
                        if (!PEUtil.CheckPositionUnderWater(pos - Vector3.up * 0.6f))
                            m_PatrolPosition = pos;
                        else
                            m_Falied = true;
                    }
                }

                path.Release(path);
            }
            else
            {
                m_Falied = true;
            }
        }

        Vector3 GetPatrolePosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minAngle, float maxAngle)
        {
            if (field == MovementField.Sky)
            {
                if (IsFly())
                    return PEUtil.GetRandomPositionInSky(center, direction, minRadius, maxRadius, m_Data.minHeight, m_Data.maxHeight, minAngle, maxAngle);
                else
                    return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, false);
            }
            else if (field == MovementField.water)
            {
                if(entity.monster != null && entity.monster.WaterSurface)
                    return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 0, entity.maxHeight * 0.5f, minAngle, maxAngle, false);
                else
                    return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, m_Data.minHeight, m_Data.maxHeight, minAngle, maxAngle, false);
            }
            else
                return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, false);
        }

        Vector3 GetPatrolPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
        {
            if ( !m_Falied && entity.Field == MovementField.Land && EvadePolarShield(position) && PEUtil.IsInAstarGrid(position) && behave.PatrolMode == BHPatrolMode.CurrentCenter)
            {
                RandomPath path = RandomPath.Construct(position, (int)Random.Range(minRadius, maxRadius)*100, OnPathComplete);
                path.spread = 40000;
                path.aimStrength = 1f;
                path.aim = PEUtil.GetRandomPosition(position, direction, minRadius, maxRadius, -75.0f, 75.0f);
                AstarPath.StartPath(path);

                return Vector3.zero;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 newPos = GetPatrolePosition(center, direction, minRadius, maxRadius, -75.0f, 75.0f);
                    if (newPos != Vector3.zero && EvadePolarShield(newPos))
                        return newPos;
                }

                for (int i = 0; i < 5; i++)
                {
                    Vector3 newPos = GetPatrolePosition(center, direction, minRadius, maxRadius, 135.0f, -135.0f);
                    if (newPos != Vector3.zero && EvadePolarShield(newPos))
                        return newPos;
                }

                for (int i = 0; i < 5; i++)
                {
                    Vector3 newPos = Vector3.zero;

                    if (field == MovementField.Land || (field == MovementField.Sky && !IsFly()))
                        newPos = PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75.0f, 75.0f);
                    else
                        newPos = PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75.0f, 75.0f, -5.0f, 5.0f);

                    if (newPos != Vector3.zero && (EvadePolarShield(newPos) || !EvadePolarShield(position)))
                        return newPos;
                }

                return GetEvadePolarShieldPosition(position);
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (behave.PatrolMode == BHPatrolMode.None)
                return BehaveResult.Failure;

			if (hasAnyRequest || entity.Food != null || entity.IsDarkInDaytime || entity.Chat != null)
                return BehaveResult.Failure;

            if( !Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (entity.Treat != null && !entity.Treat.IsDeath() && entity.Treat.hasView)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_Data.m_StartPatrolTime = Time.time;
            m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);

            m_Falied = false;
            m_PatrolPosition = Vector3.zero;
            MoveToPosition(Vector3.zero);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (behave.PatrolMode == BHPatrolMode.None)
                return BehaveResult.Failure;

			if (hasAnyRequest || entity.Food != null || entity.IsDarkInDaytime || entity.Chat != null)
                return BehaveResult.Failure;

            if( !Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (entity.Treat != null && !entity.Treat.IsDeath() && entity.Treat.hasView)
                return BehaveResult.Failure;

            //if (!EvadePolarShield(position))
            //    return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartPatrolTime > m_Data.m_Time || Stucking(5.0f))
                return BehaveResult.Failure;

            SpeedState state = SpeedState.Walk;

            if(entity.Leader == null || entity.IsLeader || entity.Leader.Stucking(2.0f) || Stucking(2.0f))
            {
                if (Stucking(2.0f) || PEUtil.SqrMagnitude(position, m_PatrolPosition, entity.gravity < PETools.PEMath.Epsilon) < 2.0f * 2.0f)
                    m_PatrolPosition = Vector3.zero;

                if (m_PatrolPosition == Vector3.zero)
                    m_PatrolPosition = GetPatrolPosition(GetPatrolCenter(), transform.forward, GetMinPatrolRadius(), GetMaxPatrolRadius());
            }
            else
            {
                if (Time.time - m_LastFollowTime > m_FollowTime)
                {
                    m_PatrolPosition = Vector3.zero;

                    m_LastFollowTime = Time.time;
                    m_FollowTime = Random.Range(3.0f, 6.0f);
                }

                if (PEUtil.SqrMagnitude(position, m_PatrolPosition, entity.gravity < PETools.PEMath.Epsilon) < 2.0f * 2.0f)
                    m_PatrolPosition = Vector3.zero;

                if(m_PatrolPosition == Vector3.zero)
                    m_PatrolPosition = entity.Group.FollowLeader(entity);

                float d = Mathf.Max(0.0f, PEUtil.Magnitude(position, entity.Leader.position, false) - radius - entity.Leader.maxRadius);
                if(d > m_Data.maxRadius)
                {
                    state = SpeedState.Run;
                }
            }

            if (m_PatrolPosition != Vector3.zero)
                m_Falied = false;

            MoveToPosition(m_PatrolPosition, state);
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if(m_PatrolPosition != Vector3.zero)
            {
                MoveToPosition(Vector3.zero);
                m_PatrolPosition = Vector3.zero;
            }
        }
    }


	[BehaveAction(typeof(BTPatrolNPC), "PatrolNPC")]
	public class BTPatrolNPC : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float prob = 0.0f;
			[BehaveAttribute]
			public float minTime = 0.0f;
			[BehaveAttribute]
			public float maxTime = 0.0f;
			[BehaveAttribute]
			public float minRadius = 0.0f;
			[BehaveAttribute]
			public float maxRadius = 0.0f;
			[BehaveAttribute]
			public float minHeight = 0.0f;
			[BehaveAttribute]
			public float maxHeight = 0.0f;
			[BehaveAttribute]
			public bool spawnCenter = false;
			
			public float m_Time = 0.0f;
			public float m_StartPatrolTime = 0.0f;
			public Vector3 m_CurrentPatrolPosition = Vector3.zero;
		}
		
		Data m_Data;


		bool GetCanWalkPos(out Vector3 walkPos)
		{
            Vector3 newpos = PEUtil.GetRandomPositionOnGroundForWander(FixedPointPostion, 0.0f, 64.0f);
			if(newpos !=Vector3.zero && AiUtil.GetNearNodePosWalkable(newpos,out walkPos))
			{
				return true;
			}
			walkPos = Vector3.zero;
			return false;
		}

        void OnPathComplete(Path path)
        {
            if (path != null && path.vectorPath.Count > 0)
            {
                Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                m_Data.m_CurrentPatrolPosition = pos;
            }
        }

        Vector3 GetPatrolPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
        {
            if (PEUtil.IsInAstarGrid(position))
            {
                RandomPath path = RandomPath.Construct(position, (int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
                path.spread = 40000;
                path.aimStrength = 1f;
                path.aim = PEUtil.GetRandomPosition(position, direction, minRadius, maxRadius, -75.0f, 75.0f);
                AstarPath.StartPath(path);

                return Vector3.zero;
            }
            else
            { 
                Vector3 pos  = Vector3.zero;
                if (GetCanWalkPos(out pos))
                {
                    return pos;
                }
                return Vector3.zero;
            }

        }
		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			
			if (hasAnyRequest || hasAttackEnemy)
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
			{
				StopMove();
				return BehaveResult.Failure;
			}

			if(m_Data.prob == 0)
			{
				StopMove();
				return BehaveResult.Failure;
			}

            m_Data.m_CurrentPatrolPosition = GetPatrolPosition(entity.position, transform.forward, m_Data.minRadius, m_Data.maxRadius);
           // NpcMgr.GetRandomPathForCsWander();
            //if(!GetCanWalkPos(out m_Data.m_CurrentPatrolPosition))
            //    return BehaveResult.Failure;
			
			m_Data.m_StartPatrolTime = Time.time;
			m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
			return BehaveResult.Running;
		}
		
		BehaveResult Tick(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			SetNpcAiType(ENpcAiType.FieldNpcIdle_Patrol);
			if (hasAttackEnemy)
			{
				MoveToPosition(Vector3.zero);
				return BehaveResult.Failure;
			}
			
			if(hasAnyRequest)
			{
				MoveToPosition(Vector3.zero);
				return BehaveResult.Failure;
			} 

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
			{
				StopMove();
				return BehaveResult.Failure;
			}

			if (PEUtil.SqrMagnitudeH(position, m_Data.m_CurrentPatrolPosition) < 0.5f * 0.5f)
			{
				MoveToPosition(Vector3.zero);
				return BehaveResult.Success;
			}

			if (Time.time - m_Data.m_StartPatrolTime > m_Data.m_Time)
			{
				MoveToPosition(Vector3.zero);
				return BehaveResult.Failure;
			}

			MoveToPosition(m_Data.m_CurrentPatrolPosition);
			return BehaveResult.Running;
		}
	   
	}

	[BehaveAction(typeof(BTFixedPoint), "FixedPoint")]
	public class BTFixedPoint :BTNormal
	{

        const float Fix_Dis_min = 2.0f;
        const float Fix_Dis_max = 5.0f;

		class Data
		{
			[BehaveAttribute]
			public float prob = 0.0f;
			[BehaveAttribute]
			public float minTime = 0.0f;
			[BehaveAttribute]
			public float maxTime = 0.0f;
			[BehaveAttribute]
			public float minRadius = 0.0f;
			[BehaveAttribute]
			public float maxRadius = 0.0f;
			
			public float mBackPointTime =0.0f;
			public float m_StartPatrolTime = 0.0f;
		}
		
		Data m_Data;
		float dis;

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			
			if (hasAnyRequest || hasAttackEnemy)
				return BehaveResult.Failure;

			if(PeGameMgr.IsMultiStory)
				return BehaveResult.Failure;

			if(FixedPointPostion == Vector3.zero)
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
			{
				StopMove();
				return BehaveResult.Failure;
			}

			dis = PEUtil.MagnitudeH(FixedPointPostion,transform.position);

            if (Mathf.Abs(dis) <= Fix_Dis_max)//  dis <= 3.0f && dis >= -3.0f)
				return BehaveResult.Failure;
			
			m_Data.m_StartPatrolTime = Time.time;
			m_Data.mBackPointTime = Random.Range(m_Data.minTime,m_Data.maxTime);
			return BehaveResult.Success;
			
		}
		
		BehaveResult Tick(Tree sender)
		{
            if (PeGameMgr.IsMultiStory)
				return BehaveResult.Failure;

			SetNpcAiType(ENpcAiType.FieldNpcIdle_FixePoint);
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;
			
			if (hasAnyRequest || hasAttackEnemy )
				return BehaveResult.Failure;
			
			if(FixedPointPostion == Vector3.zero)
				return BehaveResult.Failure;

			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Stroll))
			{
				StopMove();
				return BehaveResult.Failure;
			}

            if (Stucking(5.0f))
            {
                SetPosition(FixedPointPostion);
            }
			float dir0 = PEUtil.MagnitudeH(FixedPointPostion,transform.position);
			if(Mathf.Abs(dir0) <= Fix_Dis_min)//dir0<=0.3f && dir0 >-0.3f)
			{
				float rota = Random.value <0.5 ? 90.0f:270.0f;
				SetRotation(Quaternion.Euler(0,rota,0));
				return BehaveResult.Success;
			}

			MoveToPosition(FixedPointPostion,SpeedState.Walk);
			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{

//			if(FixedPointPostion != Vector3.zero)
//			{
//				float  dis0 = PEUtil.MagnitudeH(FixedPointPostion,transform.position);
//				if(dis0 <= 3.0f && dis0 >= -3.0f)
//				{
//
//				}
//				else
//				{
//					SetPosition(FixedPointPostion);
//				}
//			}

		}
	}

    [BehaveAction(typeof(BTTakeFood), "TakeFood")]
    public class BTTakeFood : BTNormal
    {
        class Data
		{
            [BehaveAttribute]
			public string anim = "";
			[BehaveAttribute]
			public float minTime = 0.0f;
			[BehaveAttribute]
			public float maxTime = 0.0f;

            public float m_Time;
            public float m_StartTime;
		}

        Data m_Data;

        bool m_Arrived;
        bool m_Face;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

            if (entity.Food == null)
                return BehaveResult.Failure;

            m_Arrived = false;
            m_Face = false;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (entity.Food == null)
                return BehaveResult.Failure;

            if (m_Data.m_StartTime > PETools.PEMath.Epsilon 
                && m_Data.m_Time > PETools.PEMath.Epsilon 
                && Time.time - m_Data.m_StartTime > m_Data.m_Time)
                return BehaveResult.Success;

            if (!m_Arrived)
                m_Arrived = PEUtil.SqrMagnitude(entity.tr, entity.bounds, entity.Food.tr, entity.Food.bounds, false) <= 1.0f*1.0f;

            if (m_Arrived)
                MoveToPosition(Vector3.zero);
            else
            {
                MoveToPosition(entity.Food.position, SpeedState.Run);
                return BehaveResult.Running;
            }

            if (!m_Face)
                m_Face = PEUtil.IsScopeAngle(entity.Food.position - entity.position, transform.forward, Vector3.up, -15.0f, 15.0f);

            if (m_Face)
                FaceDirection(Vector3.zero);
            else
            {
                FaceDirection(entity.Food.position - entity.position);
                return BehaveResult.Running;
            }

            if (m_Data.m_StartTime < PETools.PEMath.Epsilon)
            {
                SetBool(m_Data.anim, true);

                m_Data.m_StartTime = Time.time;
                m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                entity.Food = null;
                SetBool(m_Data.anim, false);

                m_Data.m_Time = 0.0f;
                m_Data.m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTDark), "Dark")]
    public class BTDark : BTNormal
    {
        Vector3 m_Position;
        float m_Time;
        float m_LastTime;

        BehaveResult Tick(Tree sender)
        {
            if (!entity.IsDarkInDaytime)
                return BehaveResult.Failure;

            if(Time.time - m_LastTime > m_Time || Stucking(3.0f))
            {
                Vector3 forward = transform.forward;
                if (PeCreature.Instance.mainPlayer != null)
                    forward = position - PeCreature.Instance.mainPlayer.position;

                m_Position = PEUtil.GetRandomPositionOnGround(position, forward, 32.0f, 64.0f, -75.0f, 75.0f);

                m_LastTime = Time.time;
                m_Time = Random.Range(5.0f, 10.0f);
            }

            if(PeCreature.Instance.mainPlayer != null)
            {
                if(PEUtil.SqrMagnitude(position, PeCreature.Instance.mainPlayer.position) > 64.0f*64.0f)
                {
                    PeLogicGlobal.Instance.DestroyEntity(entity.skEntity);
                }
            }

            MoveToPosition(m_Position, SpeedState.Run);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTPhotophobia), "Photophobia")]
    public class BTPhotophobia : BTNormal
    {
        LightUnit m_Light;
        Vector3 m_Position;

        float m_RandomTime;
        float m_LastRandomTime;

        float m_BeatTime;
        float m_StartBeatTime;

        BehaveResult Init(Tree sender)
        {
            if (Time.time - m_StartBeatTime < m_BeatTime)
                return BehaveResult.Failure;

            if(m_Light == null)
                m_Light = LightMgr.Instance.GetLight(entity.tr, entity.bounds);

            if (m_Light == null || !m_Light.isActiveAndEnabled)
                return BehaveResult.Failure;

            m_LastRandomTime = Time.time;
            m_Position = m_Light.GetPositionOutOfLight(entity.position); 
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (m_Light == null || !m_Light.isActiveAndEnabled || m_Position == Vector3.zero)
                return BehaveResult.Failure;

            if(PEUtil.SqrMagnitude(position, m_Position, false) <= 1.0f*1.0f || Stucking(3.0f))
            {
                if(!m_Light.IsInLight(entity.tr, entity.bounds))
                    return BehaveResult.Success;
                else
                    m_Position = m_Light.GetPositionOutOfLight(entity.position);
            }
            else
            {
                if(Time.time - m_LastRandomTime > m_RandomTime)
                {
                    m_LastRandomTime = Time.time;
                    m_RandomTime = Random.Range(2.0f, 5.0f);

                    if(!m_Light.IsInLight(entity.tr, entity.bounds))
                        return BehaveResult.Success;
                    else
                    {
                        if (attackEnemy != null && attackEnemy.MoveDir == EEnemyMoveDir.Close && Random.value < 0.3f)
                        {
                            m_StartBeatTime = Time.time;
                            m_BeatTime = Random.Range(5.0f, 10.0f);
                            return BehaveResult.Failure;
                        }

                        m_Position = m_Light.GetPositionOutOfLight(entity.position);
                    }
                }
            }

            FaceDirection(m_Light.transform.position - position);
            MoveDirection(m_Position - position, SpeedState.Run);

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if(m_Light != null)
            {
                m_Light = null;
                FaceDirection(Vector3.zero);
            }
        }
    }

	[BehaveAction(typeof(BTCheckStandPos), "CheckStandPos")]
	public class BTCheckStandPos : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float Radius;
		}
		Data m_Data;
		
		BehaveResult Tick(Tree sender)
		{
		    if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

            if (Pathea.PeGameMgr.IsTutorial)
                return BehaveResult.Success;
		
			bool _IsnearLeague = entity.NpcCmpt.HasNearleague;
			bool _IsBlockBrush = AiUtil.CheckBlockBrush(entity);
			bool _IsnearDig =   PeCreature.Instance != null ? AiUtil.CheckDig(entity, PeCreature.Instance.mainPlayer) : false;
			bool _IsDragging =  AiUtil.CheckDraging(entity);
            bool _IsNearCreation = AiUtil.CheckCreation(entity);
            bool _needAvoid = _IsnearLeague || _IsnearDig || _IsBlockBrush || _IsDragging || _IsNearCreation;

            if (_needAvoid)
				return BehaveResult.Failure;
			else
				return BehaveResult.Success;
		}
	}

	[BehaveAction(typeof(BTMoveAvoid), "MoveAvoid")]
	public class BTMoveAvoid : BTNormal
	{
		class Data
		{
			[BehaveAttribute]
			public float  Radius;
			[BehaveAttribute]
			public float firAvoid;
			[BehaveAttribute]
			public float sndAvoid;
			[BehaveAttribute]
			public float trdAvoid;
//			public float AvoidMin;
//			[BehaveAttribute]
//			public float AvoidMax;

		}
		
		Vector3 GetAvoidPos(Vector3 dirtion)
		{
            //AVA营地 不能检测过高
            float upd = IsNpcCampsite ? 15.0f : 128.0f;
            float downd = IsNpcCampsite ? 18.0f : 256.0f;

            return PEUtil.GetRandomPosition(position, dirtion, m_Data.firAvoid, m_Data.sndAvoid, -30.0f, 30.0f, PETools.PEUtil.Standlayer, upd, downd);
		}
		bool IsReach(Vector3 self,Vector3 target)
		{
			float dis = PEUtil.Magnitude(target,self);
			if(Mathf.Abs(dis) < 0.5)//dis <= 0.3f && dis >= -0.3f)
				return true;

			return false;
		}

		Vector3 m_AvoidPos;
		Data m_Data;

		SpeedState m_avoidSpeed;

		BehaveResult Init(Tree sender)
		{
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

			if(Pathea.PeGameMgr.IsSingleAdventure && entity.NpcCmpt.IsStoreNpc)
				return BehaveResult.Failure;

            if (Pathea.PeGameMgr.IsTutorial)
                return BehaveResult.Failure;

			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;

			if(!PEUtil.CheckErrorPos(position))
				return BehaveResult.Failure;

			m_avoidSpeed = SpeedState.Walk;
            Vector3 avoidDir0 = Vector3.zero;
            Vector3 avoidDir1 = Vector3.zero;
            Vector3 avoidDir2 = Vector3.zero;
            Vector3 avoidDir3 = Vector3.zero;
            bool _IsnearLeague = entity.NpcCmpt.HasNearleague;
            bool _IsBlockBrush = AiUtil.CheckBlockBrush(entity, out avoidDir0);
            bool _IsnearDig = PeCreature.Instance != null ? AiUtil.CheckDig(entity, PeCreature.Instance.mainPlayer, out avoidDir1) : false;
            bool _IsDragging = AiUtil.CheckDraging(entity, out avoidDir2);
            bool _IsNearCreation = AiUtil.CheckCreation(entity, out avoidDir3);
            bool _needAvoid = _IsnearLeague || _IsnearDig || _IsBlockBrush || _IsDragging || _IsNearCreation;

            Vector3 avoid0 = avoidDir0 != Vector3.zero ? position - avoidDir0 : Vector3.zero;
            Vector3 avoid1 = avoidDir1 != Vector3.zero ? position - avoidDir1 : Vector3.zero;
            Vector3 avoid2 = avoidDir2 != Vector3.zero ? position - avoidDir2 : Vector3.zero;
            Vector3 avoid3 = avoidDir3 != Vector3.zero ? position - avoidDir3 : Vector3.zero;
            Vector3 _AvoidDir = entity.peTrans.forward + avoid0 + avoid1 + avoid2 + avoid3;

            if (!_needAvoid)
				return BehaveResult.Failure;

            if (_IsBlockBrush || _IsnearDig || _IsDragging || _IsNearCreation)
				m_avoidSpeed = SpeedState.Run;

            m_AvoidPos = GetAvoidPos(_AvoidDir);
			if(!PEUtil.CheckErrorPos(m_AvoidPos))
				return BehaveResult.Failure;

			return BehaveResult.Running;
		}

		BehaveResult Tick(Tree sender)
		{
			if(hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
				return BehaveResult.Failure;


            Vector3 avoidDir0 = Vector3.zero;
            Vector3 avoidDir1 = Vector3.zero;
            Vector3 avoidDir2 = Vector3.zero;
            Vector3 avoidDir3 = Vector3.zero;
            bool _IsnearLeague = entity.NpcCmpt.HasNearleague;
            bool _IsBlockBrush = AiUtil.CheckBlockBrush(entity, out avoidDir0);
            bool _IsnearDig = PeCreature.Instance != null ? AiUtil.CheckDig(entity, PeCreature.Instance.mainPlayer,out avoidDir1) : false;
            bool _IsDragging = AiUtil.CheckDraging(entity, out avoidDir2);
            bool _IsNearCreation = AiUtil.CheckCreation(entity, out avoidDir3);
            bool _needAvoid = _IsnearLeague || _IsnearDig || _IsBlockBrush || _IsDragging || _IsNearCreation;

            Vector3 avoid0 = avoidDir0 != Vector3.zero ? position - avoidDir0 : Vector3.zero;
            Vector3 avoid1 = avoidDir1 != Vector3.zero ? position - avoidDir1 : Vector3.zero;
            Vector3 avoid2 = avoidDir2 != Vector3.zero ? position - avoidDir2 : Vector3.zero;
            Vector3 avoid3 = avoidDir3 != Vector3.zero ? position - avoidDir3 : Vector3.zero;
            Vector3 _AvoidDir = entity.peTrans.forward + avoid0 + avoid1 + avoid2 + avoid3;

			if(!_needAvoid)
				return BehaveResult.Failure;

			if(!IsReach(position,m_AvoidPos))
			{
                bool _IsunderBlock = PETools.PEUtil.IsUnderBlock(entity);
                bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, existent.forward, 2.0f);
                if(_IsunderBlock)
                {
                    if(_IsForwardBlock || Stucking())
                    {
                        SetPosition(m_AvoidPos);
                        return BehaveResult.Failure;
                    }
                    else
                    {
                        MoveDirection(_AvoidDir, SpeedState.Walk);
                    }
                }
                else
                {
                    if (Stucking())
                    {
                        SetPosition(m_AvoidPos);
                        return BehaveResult.Failure;
                    }
                    MoveToPosition(m_AvoidPos, m_avoidSpeed);
                }
			}
			else
			{
                if (_needAvoid)
				{
                    m_AvoidPos = GetAvoidPos(_AvoidDir);
					if(!PEUtil.CheckErrorPos(m_AvoidPos))
						return BehaveResult.Failure;
				}
				else
				{
					return BehaveResult.Failure;
				}
					
			}

			return BehaveResult.Running;
		}

		void Reset(Tree sender)
		{
			m_AvoidPos = Vector3.zero;

		}
	}

    [BehaveAction(typeof(BTSleep), "Sleep")]
    public class BTSleep : BTNormal
    {
        class Target
        {
            public PeEntity m_Entity;
            public float m_Time;

            public Target(PeEntity entity, float time)
            {
                m_Entity = entity;
                m_Time = time;
            }
        }

        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float checkTime = 0.0f;

            //public float m_Time;
            public float m_StartTime;
            public float m_LastCDTime = -1000.0f;
            public float m_LastCheckTime = -1000.0f;
            public float m_WakeupTime;
            public float m_LastWakeupTime = 0;
            public float m_AwakenedTime;
            public float m_LastAwakenedTime;
        }

        Data m_Data;

        List<Target> m_Targets = new List<Target>();

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!GameConfig.IsNight)
                return BehaveResult.Failure;

            if (RandomDunGenUtil.IsInDungeon(entity))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(escapeEnemy))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCheckTime < m_Data.checkTime)
                return BehaveResult.Failure;

            if(entity.Leader == null || entity.IsLeader)
            {
                if (Random.value > m_Data.prob)
                    return BehaveResult.Failure;
            }
            else
            {
                if (entity.Leader.animCmpt == null || !entity.Leader.animCmpt.GetBool("Sleep"))
                    return BehaveResult.Failure;
            }

            StopMove();

            SetBool("Sleep", true);
            m_Data.m_StartTime = Time.time;
            m_Data.m_LastCheckTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime < 0.5f)
                return BehaveResult.Running;
            
            if(GetBool("Sleep"))
            {
                if(Time.time - m_Data.m_LastWakeupTime > m_Data.m_WakeupTime)
                {
                    if (!GameConfig.IsNight)
                        SetBool("Sleep", false);
                    
                    m_Data.m_WakeupTime = Random.Range(60f, 120f);
                }

                if (attackEnemy != null && attackEnemy.ThreatDamage > 0.0f)
                    SetBool("Sleep", false);

                if (escapeEnemy != null && escapeEnemy.ThreatDamage > 0.0f)
                    SetBool("Sleep", false);

                if(Time.time - m_Data.m_LastAwakenedTime > m_Data.m_AwakenedTime)
                {
                    m_Data.m_LastAwakenedTime = Time.time;
                    m_Data.m_AwakenedTime = Random.Range(1f, 5f);

                    if (attackEnemy != null && attackEnemy.SqrDistanceXZ < 5f*5f)
                        SetBool("Sleep", false);

                    if (escapeEnemy != null && escapeEnemy.SqrDistanceXZ < 5f*5f)
                        SetBool("Sleep", false);

                    if(entity.Leader != null 
                        && !entity.IsLeader 
                        && entity.Leader.animCmpt != null 
                        && !entity.Leader.animCmpt.GetBool("Sleep"))
                        SetBool("Sleep", false);

                    //List<PeEntity> entities = EntityMgr.Instance.GetEntities(position, radius+5f, false);
                    //entities.Remove(entity);

                    //if(entities.Count > 0 && Random.value < 0.5f)
                    //    SetBool("Sleep", false);
                }

                for (int i = m_Targets.Count - 1; i >= 0; i--)
                {
                    if(m_Targets[i].m_Entity == null)
                    {
                        m_Targets.RemoveAt(i);
                        continue;
                    }

                    if (Time.time - m_Targets[i].m_Time > 15.0f)
                    {
                        SetBool("Sleep", false);
                        return BehaveResult.Running;
                    }

                    float dis = radius + m_Targets[i].m_Entity.maxRadius;
                    if(PEUtil.SqrMagnitude(position, m_Targets[i].m_Entity.position, false) > dis*dis)
                    {
                        m_Targets.RemoveAt(i);
                    }
                }

                List<PeEntity> ents = EntityMgr.Instance.GetEntities(position, radius+10f, false);
                ents.Remove(entity);
                for (int i = 0; i < ents.Count; i++)
                {
                    Target taget = m_Targets.Find(ret => ret.m_Entity == ents[i]);
                    if(taget == null)
                    {
                        m_Targets.Add(new Target(ents[i], Time.time));
                    }
                }
            }

            if (GetBool("Sleeping"))
                return BehaveResult.Running;

            m_Data.m_LastCDTime = Time.time;
            return BehaveResult.Success;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if (GetBool("Sleep"))
                    SetBool("Sleep", false);

                m_Data.m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTCanIdle), "CanIdle")]
    public class BTCanIdle : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
			if (entity != null && entity.NpcCmpt != null &&  entity.NpcCmpt.csCanIdle && NpcThinkDb.CanDoing(entity, EThinkingType.Stroll ))
            {
                return BehaveResult.Success;
            }
            else
            {
                return BehaveResult.Failure;
            }
        }
    }

	[BehaveAction(typeof(BTStareAt), "StareAt")]
	public class BTStareAt : BTNormal
	{
		bool  InStareRadiu(Vector3 pickPos,Transform target,out float  weigtht)
		{
			float r = 3.0f;
			float angleXY = 30.0f;
			float angleXZ = 10.0f;

			weigtht = 0.0f;
			float distance = PETools.PEUtil.Magnitude(pickPos,target.position);
			if(distance > r)
				return false;

			Vector3 tgtPosXZ = target.position + Vector3.up;
			tgtPosXZ.y = pickPos.y;

			Vector3 tgtPosXY = target.position + Vector3.up;
			tgtPosXY.x = pickPos.x;

			Vector3 targtForword = target.forward;

			float agxy = Vector3.Angle(tgtPosXY,targtForword);
			float agxz = Vector3.Angle(tgtPosXZ,targtForword);

			if(Mathf.Abs(agxy) < angleXY && Mathf.Abs(agxz) < angleXZ)
			{
				float w1 =  (1 - Mathf.Abs(agxy)/angleXY) * 0.5f;
				float w2 =  (1 - Mathf.Abs(agxz)/angleXZ) * 0.5f;
				weigtht = w1 + w2;

				return true;
			}

			return false ;
		}

		BehaveResult Tick(Tree sender)
		{
			if (entity != null && entity.NpcCmpt != null && NpcThinkDb.CanDoing(entity, EThinkingType.Stroll))
			{
				return BehaveResult.Success;
			}
			else
			{
				return BehaveResult.Failure;
			}
		}
	}
	
}

