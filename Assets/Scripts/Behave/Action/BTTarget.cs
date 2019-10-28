//#define ATTACK_OLD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Pathea;
using Pathea.Projectile;
using Pathfinding;

namespace Behave.Runtime.Action
{
    [BehaveAction(typeof(BTHasTarget), "HasTarget")]
    public class BTHasTarget : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (!hasAttackEnemy)
            {
                SetBool("Combat", false);
                return BehaveResult.Failure;
            }
            else
            {
                SetBool("Combat", true);
                return BehaveResult.Success;
            }
        }
    }

    [BehaveAction(typeof(BTHasEnemy), "HasEnemy")]
    public class BTHasEnemy : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy == null && !entity.IsAttacking)
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTReturn), "Return")]
    public class BTReturn : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float arriveRadius = 0.0f;
            [BehaveAttribute]
            public float returnRadius = 0.0f;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy != null)
                return BehaveResult.Failure;

            if (PEUtil.SqrMagnitudeH(position, entity.spawnPos) < m_Data.returnRadius * m_Data.returnRadius)
                return BehaveResult.Failure;

            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy != null)
            {
                MoveToPosition(Vector3.zero);
                return BehaveResult.Failure;
            }

            if (PEUtil.SqrMagnitudeH(position, entity.spawnPos) < m_Data.arriveRadius * m_Data.arriveRadius)
            {
                MoveToPosition(Vector3.zero);
                return BehaveResult.Failure;
            }

            MoveToPosition(entity.spawnPos);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTEnergyShield), "EnergyShield")]
    public class BTEnergyShield : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float hpPercent = 0.0f;
            [BehaveAttribute]
            public float time = 0.0f;
            [BehaveAttribute]
            public string animName = "";
        }

        Data m_Data;

        float m_StartTime;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if(m_StartTime < PETools.PEMath.Epsilon)
            {
                if(HpPercent <= m_Data.hpPercent)
                {
                    m_StartTime = Time.time;
                    ActivateEnergyShield(true);
                    SetBool(m_Data.animName, true);
                }
            }
            else
            {
                if (Time.time - m_StartTime > m_Data.time)
                    ActivateEnergyShield(false);
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTTargetRange), "TargetRange")]
    public class BTTargetRange : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            float d = attackEnemy.SqrDistanceLogic;

            if (d < m_Data.minRange * m_Data.minRange || d > m_Data.maxRange * m_Data.maxRange)
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTTargetRangeRunning), "TargetRangeRunning")]
    public class BTTargetRangeRunning : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            float d = attackEnemy.SqrDistanceLogic;

            if (d < m_Data.minRange * m_Data.minRange)
                return BehaveResult.Failure;
            else
            {
                if (d > m_Data.maxRange * m_Data.maxRange)
                {
                    MoveToPosition(attackEnemy.position, SpeedState.Run);
                    return BehaveResult.Running;
                }
                else
                    return BehaveResult.Success;
            }
        }
    }

    [BehaveAction(typeof(BTIsTargetMelee), "IsTargetMelee")]
    public class BTIsTargetMelee : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if(Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            for (int i = 0; i < SkProjectile.s_Projectiles.Count; i++)
            {
                SkProjectile projectile = SkProjectile.s_Projectiles[i];
                if (projectile == null)
                    continue;

                if (projectile.m_Target != null && projectile.m_Target.IsChildOf(entity.transform))
                    return BehaveResult.Success;

                if(projectile.m_TargetPosition != Vector3.zero)
                {
                    Vector3 emitDir = projectile.m_TargetPosition - projectile.transform.position;
                    Vector3 origin = entity.tr.InverseTransformPoint(projectile.transform.position);
                    Vector3 direction = entity.tr.InverseTransformDirection(emitDir);
                    Ray rayStart = new Ray(origin, direction);
                    if (entity.bounds.IntersectRay(rayStart))
                        return BehaveResult.Success;
                }

                if (entity.biologyViewCmpt != null && entity.biologyViewCmpt.monoModelCtrlr != null)
                {
                    Bounds b1 = entity.biologyViewCmpt.monoModelCtrlr.ColliderBounds;
                    Bounds b2 = projectile.TriggerBounds;
                    if(b1.size != Vector3.zero && b2.size != Vector3.zero)
                    {
                        b2.Encapsulate(Vector3.one*2f);

                        if (b1.Intersects(b2))
                            return BehaveResult.Success;
                    }
                }
            }

            if (attackEnemy.entityTarget.motionMgr != null
                && attackEnemy.entityTarget.motionMgr.IsActionRunning(PEActionType.SwordAttack)
                && attackEnemy.SqrDistanceXZ < 3.0f*3.0f)
                return BehaveResult.Success;

            if (attackEnemy.entityTarget.IsAttacking && attackEnemy.SqrDistanceXZ < 3.0f*3.0f)
                return BehaveResult.Success;

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTTargetAngle), "TargetAngle")]
    public class BTTargetAngle : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minAngle = 0.0f;
            [BehaveAttribute]
            public float maxAngle = 0.0f;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null || !PEUtil.IsScopeAngle(attackEnemy.position - position, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTTargetRagdoll), "TargetRagdoll")]
    public class BTTargetRagdoll : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy != null && attackEnemy.isRagdoll)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTTargetInBody), "TargetInBody")]
    public class BTTargetInBody : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy != null && InBody(attackEnemy.position + Vector3.up * 0.5f))
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTTargetWatch), "TargetWatch")]
    public class BTTargetWatch : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public bool isWatch = false;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_Data.isWatch && attackEnemy != null)
                SetIKAim(attackEnemy.CenterBone);
            else
                SetIKAim(null);

            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTRotateToTarget), "RotateToTarget")]
    public class BTRotateToTarget : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minAngle;
            [BehaveAttribute]
            public float maxAngle;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            Vector3 v = attackEnemy.position - position;

            if (PEUtil.IsScopeAngle(v, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
            {
                FaceDirection(Vector3.zero);
                return BehaveResult.Success;
            }
            else
            {
                FaceDirection(v);
                return BehaveResult.Running;
            }
        }
    }

    [BehaveAction(typeof(BTTurnOnSpot), "TurnOnSpot")]
    public class BTTurnOnSpot : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string turn = "";
            [BehaveAttribute]
            public int skillID = 0;

            public float m_StartTime;
            public Vector3 m_Direction;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            m_Data.m_Direction = attackEnemy.position - position;
            float angle = PEUtil.GetAngle(m_Data.m_Direction, transform.forward, Vector3.up);

            m_Data.m_StartTime = Time.time;
            FaceDirection(m_Data.m_Direction);
            SetBool(m_Data.turn, true);
            SetFloat("Angle", angle);
            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (PEUtil.Angle(m_Data.m_Direction, transform.forward, Vector3.up) > 5.0f 
                && Time.time - m_Data.m_StartTime <= 3.0f)
            {
                FaceDirection(m_Data.m_Direction);
                return BehaveResult.Running;
            }
            else
            {
                return BehaveResult.Success;
            }
        }
    }

    [BehaveAction(typeof(BTFollowUp), "FollowUp")]
    public class BTFollowUp : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange;
            [BehaveAttribute]
            public float maxRange;
            [BehaveAttribute]
            public float radius;

            public Vector3 m_Local;
            public Vector3 m_Patrol;

            float m_LastRandomTime = 0.0f;
            float m_CurCDTime = 0.0f;

            bool m_InitRandom;

            public bool Cooldown()
            {
                if (Time.time - m_LastRandomTime > m_CurCDTime)
                {
                    m_CurCDTime = (float)PERandom.BehaveSeed.Next(1, 5);
                    m_LastRandomTime = Time.time;
                    return true;
                }

                return false;
            }

			public bool IsReached(Vector3 pos, Vector3 target ,float Radius)
            {
				float m = PEUtil.SqrMagnitudeH(pos, target + m_Local);
				return m <= Radius * Radius;
            }

			public bool IsReached(Enemy enmy)
			{
				return enmy.SqrDistanceXZ >= 0.001f;
			}

            public void ResetInitRandom()
            {
                m_InitRandom = false;
            }

            public Vector3 GetRandomPosition(Vector3 target)
            {
                return target + m_Local;
            }

            public Vector3 GetPatrolPosition(Vector3 target)
            {
                return target + m_Local + m_Patrol;
            }

            public void RandomPosition(float argRadius)
            {
                if (m_InitRandom)
                    return;

                m_InitRandom = true;
                m_Local = PEUtil.GetRandomPosition(Vector3.zero, argRadius + minRange, argRadius + maxRange);
                m_Local = new Vector3(m_Local.x, 0.0f, m_Local.z);
            }

            public void RandomPatrol(Vector3 dir)
            {
                m_Patrol = PEUtil.GetRandomPosition(Vector3.zero, dir, 0.0f, radius, -90.0f, 90.0f);
                m_Patrol = new Vector3(m_Patrol.x, 0.0f, m_Patrol.z);
            }
        }

        Data m_Data;
	
		//float Radius_2 = 0.001f;
	
        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

//			if(!NpcThinkDb.CanDo(entity,EThinkingType.Combat))
//				return BehaveResult.Failure;

            float rad = radius + attackEnemy.radius;
//            float distance = PEUtil.MagnitudeH(position, attackEnemy.position);
//            float dis = Mathf.Max(0.0f, distance - rad);
            Vector3 dir = attackEnemy.position - position;
            dir.y = 0.0f;
//			float Radius = radius + attackEnemy.radius;

			if (Weapon != null)
			{
				IAimWeapon aimWeapon = Weapon as IAimWeapon;
				if (aimWeapon != null && attackEnemy.CenterBone != null)
					aimWeapon.SetTarget(attackEnemy.CenterBone);
			}
		
			//if (!m_Data.IsReached(position, attackEnemy.position,Radius))
			if(!m_Data.IsReached(attackEnemy))//PETools.PEMath.Epsilon
            {
                m_Data.RandomPosition(rad);
                SetMoveMode(MoveMode.ForwardOnly);
				FaceDirection(attackEnemy.position - position);
                MoveToPosition(m_Data.GetRandomPosition(attackEnemy.position), SpeedState.Run);
                return BehaveResult.Failure;
            }
            else
            {
                m_Data.ResetInitRandom();

                if (m_Data.Cooldown())
                    m_Data.RandomPatrol(-dir);

                SetMoveMode(MoveMode.EightAaxis);
                FaceDirection(dir);
                return BehaveResult.Success;
            }


		}
	}

	[BehaveAction(typeof(BTMoveToCombat), "MoveToCombat")]
    public class BTMoveToCombat : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange;
            [BehaveAttribute]
            public float maxRange;
        }

        Data m_Data;

        Vector3 m_Local;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            float d = PEUtil.MagnitudeH(position, attackEnemy.position) - radius - attackEnemy.radius;
            if (d <= m_Data.maxRange)
                return BehaveResult.Success;

            m_Local = PEUtil.GetRandomPosition(Vector3.zero, position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, -75.0f, 75.0f);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Stucking(5.0f))
                return BehaveResult.Failure;

            Vector3 des = attackEnemy.position + m_Local;
            if (PEUtil.MagnitudeH(position, des) < 1.0f * 1.0f)
            {
                MoveToPosition(Vector3.zero);
                return BehaveResult.Success;
            }

            MoveToPosition(des, SpeedState.Run);
            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTMoveToTarget), "MoveToTarget")]
    public class BTMoveToTarget : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange;
            [BehaveAttribute]
            public float maxRange;

            public Vector3 m_Local = Vector3.zero;
        }

        Data m_Data;
        Vector3 m_Local;

        Vector3 GetLocalCenterPos()
        {
            Vector3 vg;
            if(PEUtil.CheckPositionOnGround(attackEnemy.position + m_Local, out vg, 32.0f, 32.0f, GameConfig.GroundLayer))
            {
                return vg + entity.tr.up * entity.maxHeight * 0.5f;
            }

            return attackEnemy.position + m_Local + Vector3.up*entity.maxHeight*0.5f;
        }

        Vector3 GetRandomPosistion(Enemy e, IAttackPositive attack)
        {
            if(field == MovementField.water)
            {
                Vector3 pos1 = attackEnemy.entityTarget.peTrans.centerUp;
                Vector3 pos2 = attackEnemy.entityTarget.peTrans.position;

                float minHeight = VFVoxelWater.self.UpToWaterSurface(pos1.x, pos1.y, pos1.z);
                float maxHeight = VFVoxelWater.self.UpToWaterSurface(pos2.x, pos2.y, pos2.z);

                if (attackEnemy.IsDeepWater)
                    return PEUtil.GetRandomPositionInWater(e.position, position - e.position, attack.MinRange, attack.MaxRange, minHeight, maxHeight, -90.0f, 90.0f);
                else
                    return PEUtil.GetRandomPositionInWater(e.position, position - e.position, attack.MinRange, attack.MaxRange, 0.0f, entity.maxHeight*0.5f, -90.0f, 90.0f);
            }
            else
                return PEUtil.GetRandomPositionOnGround(e.position, attack.MinRange, attack.MaxRange);
        }

        Vector3 GetLocalPos(Enemy e, IAttackPositive attack)
        {
            if (!PEUtil.IsBlocked(e.entity, e.entityTarget) && !Stucking())
                m_Local = Vector3.zero;
            else
            {
                if (m_Local == Vector3.zero || PEUtil.IsBlocked(e.entityTarget, GetLocalCenterPos()))
                {
                    Vector3 local = Vector3.zero;
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 pos = GetRandomPosistion(e, attack);
                        Vector3 offCenter = pos + entity.tr.up * entity.maxHeight * 0.5f;

                        if (!PEUtil.IsBlocked(e.entityTarget, offCenter))
                        {
                            local = pos - e.position;
                            break;
                        }
                    }

                    m_Local = local;
                }
            }

            if (m_Local != Vector3.zero)
                return m_Local;
            else
                return Vector3.zero;
        }

#if ATTACK_OLD
        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if(Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            Vector3 dir = attackEnemy.position - center;
            float sqrDistance = attackEnemy.sqrDistanceReal;
            if(sqrDistance > m_Data.maxRange * m_Data.maxRange)
            {
                MoveToPosition(attackEnemy.position + attackEnemy.velocity, SpeedState.Run);
            }
            else if(sqrDistance < m_Data.minRange * m_Data.minRange)
            {
                MoveDirection(-dir, SpeedState.Run);
                FaceDirection(dir);
            }
            else
            {
                StopMove();
            }

            return BehaveResult.Success;
        }
#else
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy == null || attackEnemy.Attack == null)
                return BehaveResult.Success;

            IAttackPositive attack = attackEnemy.Attack as IAttackPositive;
            if (attack == null || attack.ReadyAttack(attackEnemy))
                return BehaveResult.Success;

            Vector3 local = GetLocalPos(attackEnemy, attack);

            Vector3 destination = attackEnemy.position + local + attackEnemy.velocity.normalized;

            if(attack is IAttackTop)
                destination += Vector3.up*(attack.MinHeight + attack.MaxHeight) * 0.5f;

            float sqrDistance = attackEnemy.SqrDistanceLogic;

            if (sqrDistance > attack.MaxRange * attack.MaxRange)
            {
                FaceDirection(Vector3.zero);
                MoveDirection(Vector3.zero);
                MoveToPosition(destination, SpeedState.Run);
            }
            else if (sqrDistance < attack.MinRange * attack.MinRange)
            {
                MoveToPosition(Vector3.zero);
                MoveDirection(-attackEnemy.Direction, SpeedState.Retreat);
                FaceDirection(attackEnemy.Direction);
            }
            else
            {
                if (attack.IsBlocked(attackEnemy))
                {
                    MoveToPosition(destination, SpeedState.Run);
                }
                else
                {
                    MoveToPosition(Vector3.zero);
                    MoveDirection(Vector3.zero);

                    if (!attack.IsInAngle(attackEnemy))
                        FaceDirection(attackEnemy.Direction);
                }
            }

            return BehaveResult.Success;
        }
#endif
    }

    [BehaveAction(typeof(BTTargetHover), "TargetHover")]
    public class BTTargetHover : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange;
            [BehaveAttribute]
            public float maxRange;
            [BehaveAttribute]
            public float minHeight;
            [BehaveAttribute]
            public float maxHeight;
            [BehaveAttribute]
            public float minTime;
            [BehaveAttribute]
            public float maxTime;
        }

        Data m_Data;

        Vector3 m_HoverPosition;

        float m_Time;
        float m_StartTime;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            m_HoverPosition = PEUtil.GetRandomPosition(attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight);
            if (m_HoverPosition == Vector3.zero)
                return BehaveResult.Failure;

            m_StartTime = Time.time;
            m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            MoveToPosition(m_HoverPosition, SpeedState.Run);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_StartTime > m_Time)
                return BehaveResult.Success;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Vector3.Distance(position, m_HoverPosition) < 1.0f)
                return BehaveResult.Success;

            //if(m_HoverPosition == Vector3.zero)
            //    m_HoverPosition = PEUtil.GetRandomPosition(attackEnemy.position, position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, 0.0f, 60.0f, m_Data.minHeight, m_Data.maxHeight);

            //if(m_HoverPosition != Vector3.zero)
            //{
            //    MoveToPosition(m_HoverPosition, SpeedState.Run);

            //    float sqrDistance = PEUtil.SqrMagnitude(position, m_HoverPosition);
            //    if (sqrDistance < 1.0f * 1.0f)
            //    {
            //        m_HoverPosition = Vector3.zero;
            //    }
            //}

            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTMoveArroundSuccess), "MoveArroundSuccess")]
    public class BTMoveArroundSuccess : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minHeight = 0.0f;
            [BehaveAttribute]
            public float maxHeight = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
        }

        Data m_Data;
        float m_ArroundTime;
        float m_LastArroundTime;
        Vector3 m_ArroundPosition;

        Vector3 GetAroundPosition()
        {
            float rs = radius + attackEnemy.radius;
            Vector2 rv = Random.insideUnitCircle.normalized * Random.Range(m_Data.minRange + rs, m_Data.maxRange + rs);
            return attackEnemy.position + new Vector3(rv.x, Random.Range(m_Data.minHeight, m_Data.maxHeight), rv.y);
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if(Time.time - m_LastArroundTime > m_ArroundTime)
            {
                m_LastArroundTime = Time.time;
                m_ArroundTime = Random.Range(m_Data.minTime, m_Data.maxTime);
                m_ArroundPosition = GetAroundPosition();
            }

            if(m_ArroundPosition != Vector3.zero)
            {
                float sqrDis = gravity > PETools.PEMath.Epsilon ? PEUtil.SqrMagnitude(position, m_ArroundPosition) : PEUtil.SqrMagnitudeH(position, m_ArroundPosition);
                if(sqrDis > 0.5f*0.5f)
                    MoveToPosition(m_ArroundPosition, SpeedState.Run);
                else
					MoveToPosition(Vector3.zero);

                FaceDirection(attackEnemy.position - position);
            }

            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTMoveRange), "MoveRange")]
    public class BTMoveRange : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minHeight = 0.0f;
            [BehaveAttribute]
            public float maxHeight = 0.0f;
            [BehaveAttribute]
            public float angle = 0.0f;

            public float m_StartTime = 0.0f;
            public bool m_IsReady = false;
        }

        Data m_Data;

        bool IsReady(Enemy e)
        {
            return PEUtil.Magnitude(position, e.position, false) - radius - e.radius > m_Data.maxRange * 1.5f;
        }

        Vector3 GetReadyPosition(Enemy e)
        {
            Vector3 pos = Vector3.zero;

            float d = (m_Data.maxRange * 2.0f + entity.maxRadius + e.radius);
            float h = e.height + (m_Data.minHeight + m_Data.maxHeight) * 0.5f;
            if (entity.Field == MovementField.water)
                h = e.height * 0.5f - entity.maxHeight * 0.5f;

            Vector3 v = transform.forward- attackEnemy.DirectionXZ;
            v.y = 0.0f;
            pos = attackEnemy.position + v.normalized * d + Vector3.up * h;

            Vector3 dir = pos - position;
            float dis = Vector3.Distance(pos, position);
            if (!Physics.Raycast(pos, dir, dis, PEConfig.ObstacleLayer))
                return pos;
            else
            {
                v = Quaternion.AngleAxis(90f, Vector3.up) * v;
                pos = attackEnemy.position + v.normalized * d + Vector3.up * h;
                return pos;
            }
        }

        Vector3 GetAttackPosition(Enemy e)
        {
            Vector3 v = Vector3.ProjectOnPlane(position - attackEnemy.position, Vector3.up).normalized;

            Vector3 pos = v * ((m_Data.minRange + m_Data.maxRange) * 0.5f + entity.maxRadius + e.radius);
            float h = e.height + (m_Data.minHeight + m_Data.maxHeight) * 0.5f;

            if (entity.Field == MovementField.water)
                h = e.height * 0.5f - entity.maxHeight * 0.5f;

            return attackEnemy.position + pos + Vector3.up * h;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            m_Data.m_IsReady = false;
            m_Data.m_StartTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Stucking(5.0f))
                return BehaveResult.Failure;

            if (!m_Data.m_IsReady)
                m_Data.m_IsReady = IsReady(attackEnemy);

            if(!m_Data.m_IsReady)
                MoveToPosition(GetReadyPosition(attackEnemy), SpeedState.Run);
            else
            {
                Vector3 attackPos = GetAttackPosition(attackEnemy);
                if(PEUtil.SqrMagnitude(position, attackPos) > 1f*1f)
                    MoveToPosition(attackPos, SpeedState.Run);
                else
                {
                    MoveToPosition(Vector3.zero);
                    FaceDirection(attackEnemy.Direction);

                    Vector3 v1 = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                    Vector3 v2 = Vector3.ProjectOnPlane(attackEnemy.position - position, Vector3.up);

                    float angle = Vector3.Angle(v1, v2);
                    if (angle < m_Data.angle)
                        return BehaveResult.Success;
                    else if (angle < 45)
                        return BehaveResult.Running;
                    else
                        return BehaveResult.Failure;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                m_Data.m_StartTime = 0.0f;

                FaceDirection(Vector3.zero);
            }
        }
    }

    [BehaveAction(typeof(BTMoveAround), "MoveAround")]
    public class BTMoveAround : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float cdTime = 10.0f;
            [BehaveAttribute]
            public float prob = 1.0f;
            [BehaveAttribute]
            public float minRange;
            [BehaveAttribute]
            public float maxRange;
            [BehaveAttribute]
            public float minHeight;
            [BehaveAttribute]
            public float maxHeight;
            [BehaveAttribute]
            public float minTime = 10.0f;
            [BehaveAttribute]
            public float maxTime = 10.0f;
        }

        Data m_Data;

        float m_Time;
        float m_LastTime;
		float m_StartTime;
        Vector3 m_HoverPosition;

        Vector3 GetAroundPos()
        {
            if (entity.Group == null)
            {
                if (field == MovementField.Sky)
                    return PEUtil.GetRandomPositionInSky(attackEnemy.position, transform.position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90.0f, 90.0f);
                else if (field == MovementField.water)
                    return PEUtil.GetRandomPositionInWater(attackEnemy.position, transform.position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90.0f, 90.0f);
                else
                    return PEUtil.GetRandomPositionOnGround(attackEnemy.position, transform.position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, -90.0f, 90.0f);
            }
            else
            {
                return entity.Group.FollowEnemy(entity, entity.maxRadius + attackEnemy.radius + Random.Range(m_Data.minRange, m_Data.maxRange));
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_LastTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_HoverPosition = GetAroundPos();
			m_StartTime = Time.time;
            m_LastTime = Time.time;
            m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (m_HoverPosition == Vector3.zero)
                return BehaveResult.Failure;
            else
            {
                float sqrDistance = PEUtil.SqrMagnitude(position, m_HoverPosition);
                if (sqrDistance < 1.0f * 1.0f || Stucking() || Time.time - m_StartTime > m_Time)
                {
                    MoveToPosition(Vector3.zero);
                    return BehaveResult.Success;
                }
                else
                {
                    MoveToPosition(m_HoverPosition, SpeedState.Run);
                    return BehaveResult.Running;
                }
            }
        }

        //void Reset(Tree sender)
        //{
        //    if(m_HoverPosition != Vector3.zero)
        //    {
        //        MoveDirection(Vector3.zero);
        //        FaceDirection(Vector3.zero);
        //        m_HoverPosition = Vector3.zero;
        //    }
        //}
    }

    [BehaveAction(typeof(BTMoveSurround), "MoveSurround")]
    public class BTMoveSurround : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 1.0f;
            [BehaveAttribute]
            public float minRange;
            [BehaveAttribute]
            public float maxRange;
            [BehaveAttribute]
            public float minHeight;
            [BehaveAttribute]
            public float maxHeight;
            [BehaveAttribute]
            public float minTime = 10.0f;
            [BehaveAttribute]
            public float maxTime = 15.0f;
        }

        Data m_Data;

        float m_Time;
		float m_StartTime;
        Vector3 m_HoverPosition;

        Vector3 GetAroundPos()
        {
            if (entity.Group == null)
            {
                if (field == MovementField.Sky)
                    return PEUtil.GetRandomPositionInSky(attackEnemy.position, transform.position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -75.0f, 75.0f);
                else if (field == MovementField.water)
                    return PEUtil.GetRandomPositionInWater(attackEnemy.position, transform.position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -75.0f, 75.0f);
                else
                    return PEUtil.GetRandomPositionOnGround(attackEnemy.position, transform.position - attackEnemy.position, m_Data.minRange, m_Data.maxRange, -75.0f, 75.0f);
            }
            else
            {
                return entity.Group.FollowEnemy(entity, entity.maxRadius + attackEnemy.radius + Random.Range(m_Data.minRange, m_Data.maxRange));
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_HoverPosition = GetAroundPos();
			m_StartTime = Time.time;
            m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (attackEnemy.GroupAttack != EAttackGroup.Threat)
                return BehaveResult.Failure;

            if (Time.time - m_StartTime > m_Time)
                return BehaveResult.Success;

            if (m_HoverPosition == Vector3.zero)
                return BehaveResult.Failure;
            else
            {
                Vector3 dir = m_HoverPosition - position;
                float sqrDistance = PEUtil.SqrMagnitude(position, m_HoverPosition, false);
                if (sqrDistance < 2.0f * 2.0f || Stucking())
                {
                    MoveToPosition(Vector3.zero);
                    FaceDirection(attackEnemy.DirectionXZ);
                }
                else
                {
                    if(Vector3.Dot(transform.forward, dir.normalized) > 0)
                        MoveToPosition(m_HoverPosition, SpeedState.Run);
                    else
                        MoveToPosition(m_HoverPosition, SpeedState.Retreat);
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (m_StartTime > PETools.PEMath.Epsilon)
            {
                m_StartTime = 0.0f;
                MoveDirection(Vector3.zero);
                FaceDirection(Vector3.zero);
                m_HoverPosition = Vector3.zero;
            }
        }
    }

    [BehaveAction(typeof(BTMoveAway), "MoveAway")]
    public class BTMoveAway : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float cdTime = 5.0f;
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float angle = 0.0f;
            [BehaveAttribute]
            public float distance = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;

            public float m_Time;
        }

        float m_StartTime;
        float m_LastTime;
        Vector3 m_Direction;

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_LastTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (Vector3.Angle(attackEnemy.Direction, transform.forward) < m_Data.angle || attackEnemy.SqrDistanceXZ > m_Data.distance * m_Data.distance)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_StartTime = Time.time;
            m_LastTime = Time.time;
            m_Direction = transform.forward;
            m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_StartTime < m_Data.m_Time)
            {
                MoveDirection(m_Direction, SpeedState.Run);
                FaceDirection(m_Direction);
                return BehaveResult.Running;
            }
            else
            {
                MoveDirection(Vector3.zero);
                FaceDirection(Vector3.zero);
                return BehaveResult.Success;
            }
        }
    }

    [BehaveAction(typeof(BTMoveFlee), "MoveFlee")]
    public class BTMoveFlee : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float time = 0.0f;
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float forwardSpeed = 0.0f;
            [BehaveAttribute]
            public float backSpeed = 0.0f;

            public float m_StartTime;
            public float m_LastCooldownTime;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCooldownTime <= m_Data.cdTime)
                return BehaveResult.Failure;

            m_Data.m_StartTime = Time.time;

            Vector3 direction = attackEnemy.position - position;
            direction = Vector3.ProjectOnPlane(direction, transform.up);

            if (Vector3.Dot(transform.forward, direction.normalized) > 0.0f)
            {
                SetSpeed(m_Data.backSpeed);
                MoveDirection(position - attackEnemy.position);
            }
            else
            {
                SetSpeed(m_Data.forwardSpeed);
                MoveDirection(attackEnemy.position - position);
            }

            FaceDirection(attackEnemy.position - position);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime < m_Data.time)
                return BehaveResult.Running;

            m_Data.m_LastCooldownTime = Time.time;
            MoveDirection(Vector3.zero);
            FaceDirection(Vector3.zero);
            return BehaveResult.Success;
        }

        void Reset(Tree sender)
        {
            MoveDirection(Vector3.zero);
            FaceDirection(Vector3.zero);
        }
    }

    [BehaveAction(typeof(BTCastSkill), "CastSkill")]
    public class BTCastSkill : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public int skillID = 0;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (!IsSkillRunnable(m_Data.skillID))
                return BehaveResult.Failure;

            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (IsSkillRunning(m_Data.skillID))
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTChangeWeapon), "ChangeWeapon")]
    public class BTChangeWeapon : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float checkTime = 0.0f;
            [BehaveAttribute]
            public float prob = 0.0f;
        }

        BehaveResult Tick(Tree sender)
        {
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTMoveWeapon), "MoveWeapon")]
    public class BTMoveWeapon : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy == null)
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTIsThreat), "IsThreat")]
    public class BTIsThreat : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy != null || escapeEnemy != null)
                return BehaveResult.Failure;

            if (threatEnemy != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsFollow), "IsFollow")]
    public class BTIsFollow : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy != null || escapeEnemy != null)
                return BehaveResult.Failure;

            if (followEnemy != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsHpPercent), "IsHpPercent")]
    public class BTIsHpPercent : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minHpPercent = 0.0f;
            [BehaveAttribute]
            public float maxHpPercent = 0.0f;
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (HpPercent >= m_Data.minHpPercent && HpPercent <= m_Data.maxHpPercent)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsEscape), "IsEscape")]
    public class BTIsEscape : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (escapeEnemy != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsAttack), "IsAttack")]
    public class BTIsAttack : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsAfraid), "IsAfraid")]
    public class BTIsAfraid : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy != null || escapeEnemy != null)
                return BehaveResult.Failure;

            if (afraidEnemy != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsCarrierAndSky), "IsCarrierAndSky")]
    public class BTIsCarrierAndSky : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (attackEnemy.isCarrierAndSky)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTIsThreatBeforeBattle), "BTIsThreatBeforeBattle")]
    public class BTIsThreatBeforeBattle : BTNormal
    {
        Enemy m_Enemy;

        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy == null)
                m_Enemy = null;

            if (m_Enemy != null || attackEnemy == null)
                return BehaveResult.Failure;

            m_Enemy = attackEnemy;
            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTThreat), "Threat")]
    public class BTThreat : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public string[] threatStr = new string[0];

            public float m_StartTime;
            public float m_StartThreatTime;
            public float m_LastCDTime;

        }

        Data m_Data;
        PeEntity m_Threat;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(followEnemy))
                m_Threat = followEnemy.entityTarget;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                m_Threat = attackEnemy.entityTarget;

            if (m_Threat == null)
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            if (m_Data.threatStr == null || m_Data.threatStr.Length <= 0)
                return BehaveResult.Failure;

            StopMove();

            m_Data.m_StartTime = Time.time;
            m_Data.m_StartThreatTime = 0.0f;
            m_Data.m_LastCDTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_Threat == null)
                return BehaveResult.Failure;

            if (m_Data.m_StartThreatTime <= PETools.PEMath.Epsilon && Time.time - m_Data.m_StartTime > 5.0f)
                return BehaveResult.Failure;

            if(/*attackEnemy != null || */escapeEnemy != null)
                return BehaveResult.Success;

            if (m_Data.m_StartThreatTime > PETools.PEMath.Epsilon)
            {
                if (Time.time - m_Data.m_StartThreatTime > 0.5f && !GetBool("Threating") && !GetBool("BehaveWaiting"))
                    return BehaveResult.Success;
            }
            else
            {
                Vector3 dir = m_Threat.position - transform.position;
                if (!PEUtil.IsScopeAngle(dir, transform.forward, Vector3.up, -15.0f, 15.0f))
                    FaceDirection(dir);
                else
                {
                    FaceDirection(Vector3.zero);
                    m_Data.m_StartThreatTime = Time.time;
                    SetBool(m_Data.threatStr[Random.Range(0, m_Data.threatStr.Length)], true);
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartThreatTime > PETools.PEMath.Epsilon)
            {
                if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
                    SetBool("Interrupt", true);

                m_Data.m_StartThreatTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTAfraid), "Afraid")]
    public class BTAfraid : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public string[] afraids = new string[0];

            public float m_LastCDTime;
        }

        Data m_Data;
        bool m_Face;
        bool m_AfraidAnim;
        PeEntity m_Afraid;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            if (m_Data.afraids == null || m_Data.afraids.Length <= 0)
                return BehaveResult.Failure;

            if (m_Afraid == null)
                m_Afraid = GetAfraidTarget();

            if (m_Afraid == null)
                return BehaveResult.Failure;

            m_Face = false;
            m_AfraidAnim = false;
            m_Data.m_LastCDTime = Time.time;
            MoveToPosition(Vector3.zero);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (GetBool("BehaveWaiting"))
                return BehaveResult.Running;

            if (m_Afraid == null)
                return BehaveResult.Failure;

            Vector3 dir = m_Afraid.position - transform.position;
            if (!m_Face)
                m_Face = PEUtil.IsScopeAngle(dir, transform.forward, Vector3.up, -15.0f, 15.0f);

            if (!m_Face)
                FaceDirection(dir);
            else
            {
                FaceDirection(Vector3.zero);

                if(!m_AfraidAnim)
                {
                    m_AfraidAnim = true;
                    SetBool(m_Data.afraids[Random.Range(0, m_Data.afraids.Length)], true);
                }
                else
                {
                    if (!GetBool("BehaveWaiting"))
                        return BehaveResult.Success;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            m_Afraid = null;
        }
    }

    [BehaveAction(typeof(BTFollow), "Follow")]
    public class BTFollow : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;

            public float m_Time;
            public float m_StartTime;

            public Vector3 m_FollowPosition;
        }

        Data m_Data;

        Vector3 GetFollowPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minAngle, float maxAngle)
        {
            if (field == MovementField.Sky)
            {
                if(IsFly())
                    return PEUtil.GetRandomPositionInSky(center, direction, minRadius, maxRadius, 10.0f, 25.0f, minAngle, maxAngle);
                else
                    return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, false);
            }
            else if(field == MovementField.water)
                return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 5.0f, 25.0f, minAngle, maxAngle, false);
            else
                return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, false);
        }

        Vector3 GetFollowPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 newPos = GetFollowPosition(center, direction, minRadius, maxRadius, -75.0f, 75.0f);
                if (newPos != Vector3.zero && EvadePolarShield(newPos))
                    return newPos;
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 newPos = GetFollowPosition(center, direction, minRadius, maxRadius, -135.0f, 135.0f);
                if (newPos != Vector3.zero && EvadePolarShield(newPos))
                    return newPos;
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 newPos = Vector3.zero;

                if(field == MovementField.Land || (field == MovementField.Sky && !IsFly()))
                    newPos = PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75.0f, 75.0f);
                else
                    newPos = PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75.0f, 75.0f, -5.0f, 5.0f);

                if (newPos != Vector3.zero && EvadePolarShield(newPos))
                    return newPos;
            }

            return GetEvadePolarShieldPosition(position);
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy)
                || !Enemy.IsNullOrInvalid(escapeEnemy)
                || Enemy.IsNullOrInvalid(followEnemy))
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_Data.m_FollowPosition = Vector3.zero;
            m_Data.m_StartTime = Time.time;
            m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy)
                || !Enemy.IsNullOrInvalid(escapeEnemy)
                || Enemy.IsNullOrInvalid(followEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
                return BehaveResult.Success;

            float rad = entity.maxRadius + followEnemy.entityTarget.maxRadius;

            if (PEUtil.SqrMagnitudeH(position, m_Data.m_FollowPosition) < 1.0f * 1.0f)
                m_Data.m_FollowPosition = Vector3.zero;

            if (m_Data.m_FollowPosition == Vector3.zero)
                m_Data.m_FollowPosition = GetFollowPosition(followEnemy.position, position - followEnemy.position, 5.0f + rad, 10.0f + rad);

            if (m_Data.m_FollowPosition == Vector3.zero)
                m_Data.m_FollowPosition = GetFollowPosition(position, followEnemy.position - position, 5.0f + rad, 10.0f + rad);

            if (m_Data.m_FollowPosition == Vector3.zero)
                return BehaveResult.Failure;

            MoveToPosition(m_Data.m_FollowPosition, SpeedState.Run);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTDoubt), "Doubt")]
    public class BTDoubt : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public string[] doubts = new string[0];

            public float m_LastCDTime;
        }

        Data m_Data;
        bool m_Face;
        bool m_DoubtAnim;
        PeEntity m_Doubt;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            if (m_Data.doubts == null || m_Data.doubts.Length <= 0)
                return BehaveResult.Failure;

            if (m_Doubt == null)
                m_Doubt = GetDoubtTarget();

            if (m_Doubt == null)
                return BehaveResult.Failure;

            m_Face = false;
            m_DoubtAnim = false;
            m_Data.m_LastCDTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            Vector3 dir = m_Doubt.position - transform.position;
            if (!m_Face)
                m_Face = PEUtil.IsScopeAngle(dir, transform.forward, Vector3.up, -75.0f, 75.0f);

            if (!m_Face)
                FaceDirection(dir);
            else
            {
                FaceDirection(Vector3.zero);

                if (!m_DoubtAnim)
                {
                    m_DoubtAnim = true;
                    SetBool(m_Data.doubts[Random.Range(0, m_Data.doubts.Length)], true);
                }
                else
                {
                    if (!GetBool("Afraiding"))
                        return BehaveResult.Success;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            m_Doubt = null;
        }
    }

    [BehaveAction(typeof(BTEscape), "Escape")]
    public class BTEscape : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
            [BehaveAttribute]
            public float maxDistance = 0.0f;
            [BehaveAttribute]
            public float interval = 0.0f;
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float clearRadius = 40.0f;
            [BehaveAttribute]
            public string anim = "";

            public float m_StartTime;
            public float m_FollowTime;
            public float m_LastProbTime;
            public float m_LastEscapeTime;
            public float m_LastFollowTime;
            public float m_LastRandomTime = 0;
            public Vector3 m_StartPoint;
            public Vector3 m_EscapePosition;
        }

        Data m_Data;

        bool IsClearEscape()
        {
            if (escapeEnemy == null || m_Data == null)
                return false;

            if (PEUtil.SqrMagnitudeH(position, escapeEnemy.position) <= m_Data.clearRadius * m_Data.clearRadius)
                return true;
            else
                return false;
        }

        void OnPathComplete(Path path)
        {
            if (path != null)
            {
                path.Claim(path);

                if (path.vectorPath != null && path.vectorPath.Count > 0 && !Enemy.IsNullOrInvalid(escapeEnemy))
                {
                    Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                    if (EvadePolarShield(pos))
                    {
                        if (!PEUtil.CheckPositionUnderWater(pos - Vector3.up * 0.6f) 
						    && Vector3.Angle(entity.position - escapeEnemy.position, pos - escapeEnemy.position) < 75.0f)
                            m_Data.m_EscapePosition = pos;
                    }
                }

                path.Release(path);
            }
        }

        Vector3 GetEscapePosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minAngle, float maxAngle)
        {
            if (field == MovementField.Sky)
            {
                if(IsFly())
                    return PEUtil.GetRandomPositionInSky(center, direction, minRadius, maxRadius, 10.0f, 25.0f, minAngle, maxAngle);
                else
                    return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, false);
            }
            else if(field == MovementField.water)
                return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 5.0f, 25.0f, minAngle, maxAngle, false);
            else
                return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, false);
        }

        Vector3 GetEscapePosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
        {
            if (entity.Field == MovementField.Land && EvadePolarShield(position) && PEUtil.IsInAstarGrid(position))
            {
                FleePath path = FleePath.Construct(position, direction,(int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
                path.spread = 40000;
                path.aimStrength = 1f;
				path.aim = position + direction.normalized * Random.Range(minRadius, maxRadius);
                AstarPath.StartPath(path);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 newPos = GetEscapePosition(center, direction, minRadius, maxRadius, -60.0f, 60.0f);
                if (newPos != Vector3.zero && EvadePolarShield(newPos))
                    return newPos;
            }

            for (int i = 0; i < 5; i++)
            {
                Vector3 newPos = GetEscapePosition(center, direction, minRadius, maxRadius, -135.0f, 135.0f);
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

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            //if (Time.time - m_Data.m_LastEscapeTime < m_Data.cdTime)
            //    return BehaveResult.Failure;

            if (escapeEnemy == null)
                return BehaveResult.Failure;

            if(entity.IsInjury)
                SetBool("Injured_Escape", true);
            else
                SetBool(m_Data.anim, true);

            //if(entity.Group != null && Random.value < 0.25f)
            //    entity.Group.RemoveMember(entity);

            m_Data.m_StartTime = Time.time;
            m_Data.m_LastProbTime = Time.time;
            m_Data.m_StartPoint = position;
            MoveToPosition(Vector3.zero);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (escapeEnemy == null || Stucking(3.0f))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.maxTime)
                return BehaveResult.Success;

            if (Time.time - m_Data.m_LastProbTime > m_Data.interval)
            {
                m_Data.m_LastProbTime = Time.time;

                if(Random.value < m_Data.prob)
                    return BehaveResult.Success;
            }

            if(entity.Leader == null || entity.IsLeader)
            {
                if (Stucking(2.0f) || PEUtil.SqrMagnitude(position, m_Data.m_EscapePosition, entity.gravity < PETools.PEMath.Epsilon) < 2.0f * 2.0f)
                    m_Data.m_EscapePosition = Vector3.zero;

                if (escapeEnemy == null)
                    return BehaveResult.Failure;

                if (m_Data.m_EscapePosition == Vector3.zero)
                    m_Data.m_EscapePosition = GetEscapePosition(position, position - escapeEnemy.position, 25.0f + radius, 35.0f + radius);
            }
            else
            {
                if (entity.Leader.Stucking(2.0f) || Stucking(2.0f))
                {
                    //if (Stucking(2.0f) || PEUtil.SqrMagnitude(position, m_Data.m_EscapePosition, entity.gravity < PETools.PEMath.Epsilon) < 2.0f * 2.0f)
                    //    m_Data.m_EscapePosition = Vector3.zero;

                    //if (m_Data.m_EscapePosition == Vector3.zero)
                    //    m_Data.m_EscapePosition = GetEscapePosition(position, position - escapeEnemy.position, 25.0f + radius, 35.0f + radius);

                    entity.Group.RemoveMember(entity);

                    if (escapeEnemy == null)
                        return BehaveResult.Failure;

                    m_Data.m_EscapePosition = GetEscapePosition(position, position - escapeEnemy.position, 25.0f + radius, 35.0f + radius);
                }
                else
                {
                    if(Time.time - m_Data.m_LastFollowTime > m_Data.m_FollowTime)
                    {
                        m_Data.m_EscapePosition = entity.Group.FollowLeader(entity);

                        m_Data.m_LastFollowTime = Time.time;
                        m_Data.m_FollowTime = Random.Range(1.0f, 3.0f);
                    }
                }
            }

            float speed = entity.GetAttribute(AttribType.RunSpeed);
            SetSpeed(Mathf.Lerp(0.5f, 1.0f, HpPercent) * speed);
            MoveToPosition(m_Data.m_EscapePosition, SpeedState.Run);
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartTime >PETools.PEMath.Epsilon)
            {
                SetBool("Injured_Escape", false);
                SetBool(m_Data.anim, false);

                m_Data.m_StartTime = 0.0f;
                m_Data.m_LastFollowTime = 0.0f;
                m_Data.m_StartPoint = Vector3.zero;
                m_Data.m_EscapePosition = Vector3.zero;
                m_Data.m_LastEscapeTime = Time.time;

                SetSpeed(0.0f);

                //if(IsClearEscape())
                //    ClearEscape();

                ClearEscape();
            }
        }
    }

    [BehaveAction(typeof(BTSurround), "Surround")]
    public class BTSurround : BTNormal
    {

    }

    [BehaveAction(typeof(BTAntHatch), "AntHatch")]
    public class BTAntHatch : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float deathTime = 0.0f;
            [BehaveAttribute]
            public int protoID = 0;
        }

        Data m_Data;

        int m_GroupMask = 1073741824;
        float m_SpawnTime;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_SpawnTime < PETools.PEMath.Epsilon)
                m_SpawnTime = Time.time;

            if(Time.time - m_SpawnTime >= m_Data.deathTime)
            {
                entity.SetAttribute(AttribType.Hp, 0.0f, false);
                SceneMan.AddSceneObj(MonsterEntityCreator.CreateAgent(position, m_Data.protoID + m_GroupMask));
                return BehaveResult.Success;
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTLayEggs), "LayEggs")]
    public class BTLayEggs : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string animName = "";
            [BehaveAttribute]
            public string boneName = "";
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float delayTime = 0.0f;
            [BehaveAttribute]
            public float hpPercent = 0.0f;
            [BehaveAttribute]
            public int protoID = 0;
            [BehaveAttribute]
            public int effectID = 0;
        }

        Data m_Data;

        float m_SpawnTime;
        float m_LastHatchTime;

        bool m_Spawn;

        Transform m_SpawnTrans;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_SpawnTime < PETools.PEMath.Epsilon)
                m_SpawnTime = Time.time;

            if (m_SpawnTrans == null)
                m_SpawnTrans = PEUtil.GetChild(entity.tr, m_Data.boneName);

            if (m_SpawnTrans == null)
                return BehaveResult.Failure;

            if (entity.HPPercent >= m_Data.hpPercent)
                return BehaveResult.Failure;

            if (Time.time - m_LastHatchTime < m_Data.cdTime)
                return BehaveResult.Failure;

            m_Spawn = false;
            m_LastHatchTime = Time.time;
            SetBool(m_Data.animName, true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if(!m_Spawn && Time.time - m_LastHatchTime >= m_Data.delayTime)
            {
                m_Spawn = true;

                SceneMan.AddSceneObj(MonsterEntityCreator.CreateAgent(m_SpawnTrans.position, m_Data.protoID));
                Pathea.Effect.EffectBuilder.Instance.Register(m_Data.effectID, null, m_SpawnTrans.position, Quaternion.identity, m_SpawnTrans);
            }

            if (Time.time - m_LastHatchTime < 0.5f)
                return BehaveResult.Running;

            if (entity.IsAttacking)
                return BehaveResult.Running;

            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTDeath), "Death")]
    public class BTDeath : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float deathTime = 0.0f;
        }

        Data m_Data;
        float m_SpawnTime;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_SpawnTime < PETools.PEMath.Epsilon)
                m_SpawnTime = Time.time;

            if(Time.time - m_SpawnTime >= m_Data.deathTime)
            {
                entity.SetAttribute(AttribType.Hp, 0.0f, false);
                return BehaveResult.Success;
            }

            return BehaveResult.Failure;
        }
    }
}