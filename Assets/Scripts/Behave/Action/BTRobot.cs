//#define ATTACK_OLD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Pathea;
using System;
using Random = UnityEngine.Random;

namespace Behave.Runtime.Action
{
    [BehaveAction(typeof(BTIsCrouchReady), "IsCrouchReady")]
    public class BTIsCrouchReady : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if(entity.animCmpt != null && entity.animCmpt.ContainsParameter("Crouch"))
            {
                if (!entity.animCmpt.GetBool("Crouch") && !entity.animCmpt.GetBool("CrouchRunning"))
                    return BehaveResult.Success;
            }

            return BehaveResult.Failure;
        }
    }
    [BehaveAction(typeof(BTCrouch), "Crouch")]
    public class BTCrouch : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            bool isFly = hasAttackEnemy || TDObj != null || TDpos != Vector3.zero;

            if (!isFly)
            {
                if(!GetBool("Crouch"))
                {
                    SetBool("Crouch", true);
                    return BehaveResult.Running;
                }
            }
            else
            {
                if (GetBool("Crouch"))
                {
                    SetBool("Crouch", false);
                    return BehaveResult.Running;
                }
            }

            if (GetBool("CrouchRunning"))
                return BehaveResult.Running;

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTUnfold), "Unfold")]
    public class BTUnfold : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (!hasAttackEnemy)
            {
                if (GetBool("Unfold"))
                {
                    SetBool("Unfold", false);
                    return BehaveResult.Running;
                }
            }
            else
            {
                if (!GetBool("Unfold"))
                {
                    SetBool("Unfold", true);
                    return BehaveResult.Running;
                }
            }

            if (GetBool("Unfolding"))
                return BehaveResult.Running;

            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTFlyAndLand), "FlyAndLand")]
    public class BTFlyAndLand : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public bool fly = false;
        }

        Data m_Data;

        Vector3 m_FlyPosition;
        float m_StartTime;
        float m_StartLandTime;

        Vector3 GetLandPos()
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out hitInfo, 256.0f, PEConfig.GroundedLayer))
            {
                return hitInfo.point;
            }

            return Vector3.zero;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (IsFly() == m_Data.fly)
                return BehaveResult.Failure;

            if (m_Data.fly)
                m_FlyPosition = position;
            else
                m_FlyPosition = GetLandPos();

            m_StartTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (m_Data.fly)
            {
                ActivateGravity(false);
                MoveDirection(m_FlyPosition - position);

                Fly(true);

                if (GetBool("Fly") && GetBool("Rising"))
                    return BehaveResult.Running;

                if (PEUtil.Magnitude(position, m_FlyPosition) < 1.0f)
                    return BehaveResult.Success;
            }
            else
            {
                ActivateGravity(true);
                MoveDirection(m_FlyPosition - position);

                if (PEUtil.Magnitude(position, m_FlyPosition) < 1.0f || grounded)
                {
                    if(GetBool("Fly"))
                    {
                        Fly(false);
                        m_StartLandTime = Time.time;
                        return BehaveResult.Running;
                    }
                }

                if (!GetBool("Fly"))
                {
                    if (GetBool("Landing") || Time.time - m_StartLandTime < 0.25f)
                        return BehaveResult.Running;
                    else
                        return BehaveResult.Success;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_StartTime > PETools.PEMath.Epsilon)
            {
                MoveDirection(Vector3.zero);
                m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTSwitchStateSky), "SwitchStateSky")]
    public class BTSwitchStateSky : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float height = 0.0f;
        }

        Data m_Data;

        Vector3 m_FlyPosition;
		float m_StartTime;
        float m_StartLandTime;

        Vector3 GetLandPos()
        {
            RaycastHit hitInfo;
            if(Physics.Raycast(position + Vector3.up, Vector3.down, out hitInfo, 256.0f, PEConfig.GroundedLayer))
            {
                return hitInfo.point;
            }

            return Vector3.zero;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            bool isFly = hasAttackEnemy || TDObj != null || TDpos != Vector3.zero;

            if (GetBool("Fly") == isFly)
                return BehaveResult.Failure;

            if (isFly)
                m_FlyPosition = position + Vector3.up * m_Data.height;
            else
            {
                m_FlyPosition = GetLandPos();

                if(PEUtil.CheckPositionUnderWater(m_FlyPosition))
                    return BehaveResult.Failure;
            }

			m_StartTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            bool isFly = hasAttackEnemy || TDObj != null || TDpos != Vector3.zero;

            if (isFly)
            {
                if (GetBool("Fly") && GetBool("Rising"))
                    return BehaveResult.Running;

                ActivateGravity(false);
                //FaceDirection(transform.forward);
                MoveDirection(m_FlyPosition - position, SpeedState.Run);

                Fly(true);

                if (PEUtil.Magnitude(position, m_FlyPosition) <= 1.0f || Time.time - m_StartTime > 10.0f)
                    return BehaveResult.Success;
            }
            else
            {
                ActivateGravity(true);
                //FaceDirection(transform.forward);
                MoveDirection(m_FlyPosition - position, SpeedState.Run);

                if (PEUtil.Magnitude(position, m_FlyPosition) <= 1.0f || grounded || Time.time - m_StartTime > 10.0f)
                {
                    if(GetBool("Fly"))
                    {
                        Fly(false);
                        m_StartLandTime = Time.time;
                        return BehaveResult.Running;
                    }
                }

                if (!GetBool("Fly"))
                {
                    if (GetBool("Landing") || Time.time - m_StartLandTime < 0.25f)
                        return BehaveResult.Running;
                    else
                        return BehaveResult.Success;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_StartTime > PETools.PEMath.Epsilon)
            {
                MoveDirection(Vector3.zero);
                //FaceDirection(Vector3.zero);

                m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTSwitchWaterState), "SwitchWaterState")]
    public class BTSwitchWaterState : BTAttackBase
    {
        class Data
        {
            [BehaveAttribute]
            public float downHeight = 0.0f;
            [BehaveAttribute]
            public float upHeight = 0.0f;
            [BehaveAttribute]
            public string downAnim = "";
            [BehaveAttribute]
            public string upAnim = "";
        }

        Data m_Data;

        float m_StartTime = 0.0f;

        Vector3 m_TargetPosition;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (attackEnemy.IsDeepWater && !entity.monster.WaterSurface)
                return BehaveResult.Success;

            if (attackEnemy.IsShallowWater && entity.monster.WaterSurface)
                return BehaveResult.Success;

            m_StartTime = Time.time;
            m_TargetPosition = Vector3.zero;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (GetBool("Jumping"))
                return BehaveResult.Running;

            float waterHeight;

            if (attackEnemy.IsDeepWater && entity.monster.WaterSurface)
            {
                //Vector3 pos = position - Vector3.up * m_Data.downHeight;
                //if(PEUtil.CheckPositionUnderWater(pos))
                {
                    SetBool(m_Data.downAnim, true);
                    entity.monster.WaterSurface = false;
                }

                if (Time.time - m_StartTime > 6.0f)
                    return BehaveResult.Failure;

                return BehaveResult.Running;
            }

            if (attackEnemy.IsShallowWater && !entity.monster.WaterSurface)
            {
                if(m_TargetPosition == Vector3.zero)
                {
                    m_TargetPosition = PEUtil.GetRandomPositionInWater(attackEnemy.position, position-attackEnemy.position, 10.0f, 15.0f, 0.0f, 0.5f, -90.0f, 90.0f);

                    if (m_TargetPosition == Vector3.zero)
                        return BehaveResult.Failure;
                    else
                    {
                        if(PEUtil.GetWaterSurfaceHeight(m_TargetPosition, out waterHeight))
                        {
                            m_TargetPosition.y = waterHeight - m_Data.upHeight;
                        }
                    }
                }
                
                if(PEUtil.GetWaterSurfaceHeight(position, out waterHeight))
                {
                    MoveToPosition(m_TargetPosition, SpeedState.Run);

                    if (Mathf.Abs(waterHeight - position.y - m_Data.upHeight) < 0.5f)
                    {
                        MoveToPosition(Vector3.zero);
                        SetBool(m_Data.upAnim, true);
                        entity.monster.WaterSurface = true;
                    }
                }

                if (Time.time - m_StartTime > 6.0f)
                    return BehaveResult.Failure;

                return BehaveResult.Running;
            }

            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTStrike), "Strike")]
    public class BTStrike : BTAttackBase
    {
        //class Data : IAttack
        class Data
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minHpPercent = 0.0f;
            [BehaveAttribute]
            public float maxHpPercent = 1.0f;
            [BehaveAttribute]
            public float speed = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;

            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;

            bool IsInHpRange(Enemy enemy)
            {
                return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
            }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID);
            }

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                return enemy.SqrDistance >= minRange*minRange && enemy.SqrDistanceXZ <= maxRange*maxRange && IsInHpRange(enemy);
            }

            public bool CanAttack(Enemy enemy)
            {
                return true;
            }

            public bool IsProbability(float randomValue)
            {
                return randomValue <= prob;
            }

            public bool IsBlocked(Enemy enemy)
            {
                return false;
            }

#if true //ATTACK_OLD
            public bool Ready()
            {
                if (Cooldown())
                {
                    return Random.value <= prob;
                }

                return false;
            }

            bool Cooldown()
            {
                return Time.time - m_LastCDTime > cdTime;
            }
#endif
        }

        Data m_Data;
        Vector3 m_TargetPosition;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;
#if true //ATTACK_OLD
            if (!m_Data.Ready())
                return BehaveResult.Failure;

            float sqrDistanceXZ = attackEnemy.SqrDistance;
            if (sqrDistanceXZ > m_Data.maxRange * m_Data.maxRange
                || sqrDistanceXZ < m_Data.minRange * m_Data.minRange)
                return BehaveResult.Failure;

            if (!IsSkillRunnable(m_Data.skillID))
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            SetSpeed(m_Data.speed);
            m_Data.m_LastCDTime = Time.time;
            m_TargetPosition = attackEnemy.position;
            SetBool(m_Data.anim, true);
            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Stucking(3.0f))
                return BehaveResult.Failure;

            if (PEUtil.SqrMagnitude(position, m_TargetPosition) < 1.0f * 1.0f)
            {
                SetBool(m_Data.anim, false);
                return BehaveResult.Success;
            }
            else
            {
                MoveDirection(m_TargetPosition - position, SpeedState.Run);
                return BehaveResult.Running;
            }
        }

        void Reset(Tree sender)
        {
            if (m_TargetPosition != Vector3.zero)
            {
                SetSpeed(0.0f);
                m_TargetPosition = Vector3.zero;
                MoveDirection(Vector3.zero);
                SetBool(m_Data.anim, false);
            }
        }
    }

    [BehaveAction(typeof(BTStrikeTerrain), "StrikeTerrain")]
    public class BTStrikeTerrain : BTAttackBase
    {
        class Data : IAttack
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float radius = 0.0f;
            [BehaveAttribute]
            public float height = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;

            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID);
            }

            public float Weight { get { return prob; } }

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                return enemy.SqrDistanceXZ <= radius*radius;
            }

            public bool CanAttack(Enemy enemy)
            {
                return true;
            }

            public bool IsBlocked(Enemy enemy)
            {
                return false;
            }

            public bool CanInterrupt()
            {
                return false;
            }
#if ATTACK_OLD
            public bool Ready()
            {
                if (Cooldown())
                {
                    return Random.value <= prob;
                }

                return false;
            }

            bool Cooldown()
            {
                return Time.time - m_LastCDTime > cdTime;
            }
#endif
        }

        Data m_Data;
        Vector3 m_TargetPosition;
        bool m_Up;
        bool m_Skill;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;
#if ATTACK_OLD
            if (!m_Data.Ready())
                return BehaveResult.Failure;

            float sqrDistanceXZ = attackEnemy.sqrDistanceXZ;
            if (sqrDistanceXZ > m_Data.radius * m_Data.radius)
                return BehaveResult.Failure;

            if (!IsSkillRunnable(m_Data.skillID))
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            m_Up = false;
            m_Skill = false;
            m_Data.m_LastCDTime = Time.time;
            m_TargetPosition = attackEnemy.position + Vector3.up*15.0f;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if(!m_Up)
            {
                //FaceDirection(transform.forward);
                MoveDirection(m_TargetPosition - position, SpeedState.Run);

                if(PEUtil.SqrMagnitude(position, m_TargetPosition) < 1.0f*1.0f)
                {
                    m_Up = true;
                    SetBool(m_Data.anim, true);
                    return BehaveResult.Running;
                }
            }
            else
            {
                //FaceDirection(transform.forward);
                MoveDirection(Vector3.down, SpeedState.Sprint);

                if (!m_Skill)
                {
                    if (grounded)
                    {
                        m_Skill = true;
                        SetBool(m_Data.anim, false);

                        if(attackEnemy != null)
                            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
                    }
                }
                else
                {
                    if (!IsSkillRunning(m_Data.skillID))
                        return BehaveResult.Success;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_TargetPosition != Vector3.zero)
            {
                StopSkill(m_Data.skillID);
                SetBool(m_Data.anim, false);
                MoveDirection(Vector3.zero);
                //FaceDirection(Vector3.zero);
                m_TargetPosition = Vector3.zero;
            }
        }
    }

    [BehaveAction(typeof(BTRotatingLightsaber), "RotatingLightsaber")]
    public class BTRotatingLightsaber : BTAttackBase
    {
        class Data : IAttack
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minAngle = 0.0f;
            [BehaveAttribute]
            public float maxAngle = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;

            float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;

            public void SetCDTime(float time)
            {
                m_LastCDTime = time;
            }

            public bool Ready()
            {
                if (Cooldown())
                {
                    return Random.value <= prob;
                }

                return false;
            }

            bool Cooldown()
            {
                return Time.time - m_LastCDTime > cdTime;
            }

            public float Weight { get { return prob; } }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID);
            }

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                if (enemy.SqrDistanceLogic < minRange * minRange || enemy.SqrDistanceLogic > maxRange * maxRange)
                    return false;

                if (!PEUtil.IsScopeAngle(enemy.Direction, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle))
                    return false;

                return true;
            }

            public bool CanAttack(Enemy enemy)
            {
                return true;
            }

            public bool IsBlocked(Enemy enemy)
            {
                return false;
            }

            public bool CanInterrupt()
            {
                return false;
            }
        }

        Data m_Data;

        float m_StartTime;
        //float m_Angle;
        bool m_Arrived;

        Vector3 m_TargetPosition;
        Vector3 m_EndPosition;
        Vector3 m_FaceDirection;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;
#if ATTACK_OLD
            if (!m_Data.Ready())
                return BehaveResult.Failure;

            float sqrDistanceXZ = attackEnemy.sqrDistanceReal;
            if (sqrDistanceXZ < m_Data.minRange * m_Data.minRange
                || sqrDistanceXZ > m_Data.maxRange * m_Data.maxRange)
                return BehaveResult.Failure;

            Vector3 dir = attackEnemy.position - position;
            if (!PEUtil.IsScopeAngle(dir, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
                return BehaveResult.Failure;

            if (!IsSkillRunnable(m_Data.skillID))
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            StopMove();
            //m_Angle = 0.0f;
            m_Arrived = false;
            m_StartTime = Time.time;

            Vector3 dir = attackEnemy.position - position;
            Vector3 dirNormal = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
            Vector3 offDir = Vector3.ProjectOnPlane(dirNormal * 5.0f + Vector3.up * 2.0f, transform.right);
            m_TargetPosition = attackEnemy.centerPos + offDir;

            m_Data.SetCDTime(Time.time);

            SetBool(m_Data.anim, true);
            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Stucking(3.0f))
                return BehaveResult.Failure;

            if (!m_Arrived)
                m_Arrived = PEUtil.SqrMagnitudeH(position, m_TargetPosition) < 1.0f * 1.0f;

            if (!m_Arrived)
            {
                FaceDirection(transform.forward);

                if(GetBool("LaserRunning"))
                    MoveDirection(m_TargetPosition - position, SpeedState.Run);
            }
            else
            {
                if(GetBool(m_Data.anim))
                    SetBool(m_Data.anim, false);
                else
                {
                    if (!GetBool("Lasering"))
                        return BehaveResult.Success;
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if(m_StartTime > PETools.PEMath.Epsilon)
            {
                StopSkill(m_Data.skillID);
                SetBool(m_Data.anim, false);

                MoveDirection(Vector3.zero);
                FaceDirection(Vector3.zero);

                m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTAttackSummon), "AttackSummon")]
    public class BTAttackSummon : BTAttackBase
    {
        class Data : IAttack
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public int count = 0;
            [BehaveAttribute]
            public int protoID = 0;
            [BehaveAttribute]
            public float hpPercent = 0.0f;
            [BehaveAttribute]
            public float delayTime = 0.0f;
            [BehaveAttribute]
            public Vector3 center = Vector3.zero;
            [BehaveAttribute]
            public Vector3 extend = Vector3.zero;

            public bool m_Summoned = false;
            public float m_StartTime = 0.0f;

            public bool IsRunning(Enemy enemy)
            {
                return m_StartTime > 0.0f && Time.time - m_StartTime <= delayTime;
            }

            public float Weight { get { return prob; } }

            public bool IsReadyCD(Enemy enemy)
            {
                return true;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                return !m_Summoned && enemy.entity.HPPercent <= hpPercent;
            }

            public bool CanAttack(Enemy enemy)
            {
                return true;
            }

            public bool IsProbability(float randomValue)
            {
                return true;
            }

            public bool IsBlocked(Enemy enemy)
            {
                return false;
            }

            public bool CanInterrupt()
            {
                return false;
            }
        }

        Data m_Data;
        float m_StartTime;

        Vector3 GetRandomPos(Transform root)
        {
            if(root != null)
            {
                float rx = UnityEngine.Random.Range(-Mathf.Abs(m_Data.extend.x), Mathf.Abs(m_Data.extend.x));
                float ry = UnityEngine.Random.Range(-Mathf.Abs(m_Data.extend.y), Mathf.Abs(m_Data.extend.y));
                float rz = UnityEngine.Random.Range(-Mathf.Abs(m_Data.extend.z), Mathf.Abs(m_Data.extend.z));

                return root.TransformPoint(m_Data.center + new Vector3(rx, ry, rz));
            }

            return Vector3.zero;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;
#if ATTACK_OLD
            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (m_Data.m_Summoned || HpPercent > m_Data.hpPercent)
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            if (m_Data.m_StartTime < PETools.PEMath.Epsilon)
            {
                m_Data.m_StartTime = Time.time;
                SetBool(m_Data.anim, true);
            }

            if (Time.time - m_Data.m_StartTime < m_Data.delayTime)
                return BehaveResult.Running;

            for (int i = 0; i < m_Data.count; i++)
            {
                SceneMan.AddSceneObj(MonsterEntityCreator.CreateAgent(GetRandomPos(transform), m_Data.protoID));
            }

            m_Data.m_Summoned = true;
            return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTBlink), "Blink")]
    public class BTBlink : BTAttackBase
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float radius = 0.0f;
        }

        Data m_Data;

        Vector3 GetRandomBlinkPos()
        {
            int layer = 1 << Pathea.Layer.VFVoxelTerrain
                        | 1 << Pathea.Layer.SceneStatic
                        | 1 << Pathea.Layer.Unwalkable
                        | 1 << Pathea.Layer.Building
                        | 1 << Pathea.Layer.Default
                        | 1 << Pathea.Layer.Player
                        | 1 << Pathea.Layer.AIPlayer;

            int layer1 = 1 << Pathea.Layer.VFVoxelTerrain
                        | 1 << Pathea.Layer.SceneStatic
                        | 1 << Pathea.Layer.Unwalkable
                        | 1 << Pathea.Layer.Building
                        | 1 << Pathea.Layer.Default;
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = PEUtil.GetRandomPosition(position, 0.0f, m_Data.radius, true);
                if (!Physics.CheckSphere(pos, radius, layer)
                    && !Physics.Raycast(position, pos - position, Vector3.Distance(pos, position), layer1))
                    return pos;
            }

            return Vector3.zero;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (UnityEngine.Random.value > m_Data.prob)
                return BehaveResult.Failure;

            Vector3 pos = GetRandomBlinkPos();
            if(pos != Vector3.zero)
            {
                SetPosition(pos);
                return BehaveResult.Success;
            }
            
            return BehaveResult.Failure;  // unreachable code !
        }
    }

    [BehaveAction(typeof(BTAttackSuicide), "AttackSuicide")]
    public class BTAttackSuicide : BTAttackBase
    {
        class Data : IAttack
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float hpPercent = 0.0f;
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float time = 0.0f;
            [BehaveAttribute]
            public float radius = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;

            public float Weight { get { return prob; } }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID);
            }

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID);
            }

            public bool ReadyAttack(Enemy enemy)
            {
                return enemy.entity.HPPercent <= hpPercent;
            }

            public bool CanAttack(Enemy enemy)
            {
                return true;
            }

            public bool IsBlocked(Enemy enemy)
            {
                return false;
            }

            public bool CanInterrupt()
            {
                return false;
            }
        }

        Data m_Data;
        float m_StartTime = 0.0f;

        void Explode()
        {
            //SetAttribute((int)AttribType.Hp, 0.0f);
            SetViewActive(false);
            GameObject.Destroy(entity.gameObject, 0.2f);
            StartSkill(attackEnemy.entityTarget, m_Data.skillID);

            BehaveCmpt behave = entity.GetComponent<BehaveCmpt>();
            if (behave != null) behave.Pause(true);
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;
#if ATTACK_OLD
            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            if (HpPercent > m_Data.hpPercent)
                return BehaveResult.Failure;

            if (!IsSkillRunnable(m_Data.skillID))
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            m_StartTime = Time.time;
            SetBool(m_Data.anim, true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (Time.time - m_StartTime > m_Data.time || 
                attackEnemy.SqrDistanceLogic < m_Data.radius * m_Data.radius)
            {
                Explode();
                return BehaveResult.Success;
            }

            FaceDirection(attackEnemy.position - position);
            MoveDirection(attackEnemy.position - position, SpeedState.Sprint);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTFollowAttack), "FollowAttack")]
    public class BTFollowAttack : BTAttackBase
    {
        class Data : IAttackPositive
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minAngle = 0.0f;
            [BehaveAttribute]
            public float maxAngle = 0.0f;
            [BehaveAttribute]
            public float minHeight = 0.0f;
            [BehaveAttribute]
            public float maxHeight = 0.0f;
            [BehaveAttribute]
            public float minHpPercent = 0.0f;
            [BehaveAttribute]
            public float maxHpPercent = 1.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
            [BehaveAttribute]
            public float speed = 0.0f;
            [BehaveAttribute]
            public int[] skillStr = new int[0];
            [BehaveAttribute]
            public bool isBlock = false;

            List<int> m_Skills = new List<int>();

            public int m_SkillID = 0;

            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;
            public float m_Time = 0.0f;

            public float MinRange { get { return minRange; } }
            public float MaxRange { get { return maxRange; } }

            bool IsInHpRange(Enemy enemy)
            {
                return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
            }

            public int GetRandomSkill(Enemy enemy)
            {
                m_Skills.Clear();

                for (int i = 0; i < skillStr.Length; i++)
                {
                    if (enemy.entity.IsSkillRunable(skillStr[i]))
                        m_Skills.Add(skillStr[i]);
                }

                if (m_Skills.Count > 0)
                    return m_Skills[Random.Range(0, m_Skills.Count)];
                else
                    return 0;
            }

            public float Weight { get { return prob; } }

            public float MinHeight { get { return minHeight; } }

            public float MaxHeight { get { return maxHeight; } }

            public bool IsReadyCD(Enemy enemy)
            {
                if (Time.time - m_LastCDTime > cdTime)
                {
                    for (int i = 0; i < skillStr.Length; i++)
                    {
                        if (enemy.entity.IsSkillRunable(skillStr[i]))
                            return true;
                    }
                }

                return false;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                if (!IsInRange(enemy))
                    return false;

                if (!IsInAngle(enemy))
                    return false;

                if (!IsInHpRange(enemy))
                    return false;

                if (IsBlocked(enemy))
                    return false;

                return true;
            }

            public bool CanAttack(Enemy enemy)
            {
                if (enemy.entity.Field == MovementField.Sky && enemy.IsInWater)
                    return false;

                if (enemy.entity.Field == MovementField.Land && !enemy.IsOnLand)
                    return false;

                if (enemy.entity.Field == MovementField.water && !enemy.IsInWater)
                    return false;

                return true;
            }

            public Vector3 GetAttackPosition(Enemy enemy)
            {
                float radius = ((minRange + maxRange) * 0.5f + enemy.entity.maxRadius + enemy.entityTarget.maxRadius);
                //float height = (minHeight + maxHeight) * 0.5f - enemy.entity.maxHeight * 0.5f;
                return enemy.entityTarget.position - enemy.DirectionXZ.normalized * radius/* + height*Vector3.up*/;
            }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(m_SkillID, false);
            }

            public bool IsInRange(Enemy enemy)
            {
                float sqrDistance = enemy.SqrDistanceLogic;

                return sqrDistance >= minRange * minRange && sqrDistance <= maxRange * maxRange;
            }

            public bool IsInAngle(Enemy enemy)
            {
                if (enemy.Inside)
                    return true;

                return PEUtil.IsScopeAngle(enemy.Direction, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle);
            }

            public bool IsBlocked(Enemy enemy)
            {
                return isBlock && PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
            }

            public bool CanInterrupt()
            {
                return false;
            }

#if ATTACK_OLD
            public bool m_CanAttack;
            public bool m_Arrived;

            public bool Ready()
            {
                if (Cooldown())
                {
                    return Random.value <= prob;
                }

                return false;
            }

            bool Cooldown()
            {
                return Time.time - m_LastCDTime > cdTime;
            }
#endif
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;
#if ATTACK_OLD
            if (!m_Data.Ready())
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (IsSkillRunning(m_Data.m_SkillID))
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            m_Data.m_SkillID = m_Data.GetRandomSkill(attackEnemy);
            if (m_Data.m_SkillID <= 0)
                return BehaveResult.Failure;

            if (!entity.IsSkillRunable(m_Data.m_SkillID))
                return BehaveResult.Failure;

            StopMove();
#if ATTACK_OLD
            m_Data.m_CanAttack = true;
            m_Data.m_Arrived = false;
#else
            m_Data.m_LastCDTime = Time.time;
            StartSkill(attackEnemy.entityTarget, m_Data.m_SkillID);
#endif
            m_Data.m_StartTime = Time.time;
            m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            SetSpeed(m_Data.speed);
            return BehaveResult.Running;
        }

#if ATTACK_OLD
        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            Vector3 dir = attackEnemy.position - position;

            if (m_Data.m_CanAttack)
            {
                if (!m_Data.m_Arrived)
                {
                    float sqrDistanceXZ = attackEnemy.sqrDistanceReal;

                    if (sqrDistanceXZ > m_Data.maxRange * m_Data.maxRange)
                    {
                        MoveToPosition(attackEnemy.position + attackEnemy.velocity, SpeedState.Run);
                    }
                    else if (sqrDistanceXZ < m_Data.minRange * m_Data.minRange)
                    {
                        MoveDirection(-dir);
                        FaceDirection(dir);
                    }
                    else
                    {
                        StopMove();
                        m_Data.m_Arrived = true;
                    }
                }
                else
                {
                    if (!PEUtil.IsScopeAngle(dir, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
                        FaceDirection(dir);
                    else
                    {
                        FaceDirection(Vector3.zero);
                        m_Data.m_CanAttack = false;
                        m_Data.m_LastCDTime = Time.time;
                        StartSkill(attackEnemy.skTarget, m_Data.m_SkillID);
                    }
                }

                if (m_Data.m_CanAttack && Time.time - m_Data.m_StartTime > 2.0f)
                    return BehaveResult.Failure;

                return BehaveResult.Running;
            }
            else
            {
                if (IsSkillRunning(m_Data.m_SkillID))
                    return BehaveResult.Running;
                else
                    return BehaveResult.Success;
            }
        }
#else
        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
                return BehaveResult.Success;

            float offHeight = attackEnemy.height + (m_Data.minHeight + m_Data.maxHeight) * 0.5f;
            MoveToPosition(attackEnemy.position + Vector3.up * offHeight);
            return BehaveResult.Running;
        }
#endif

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if (IsSkillRunning(m_Data.m_SkillID))
                    StopSkill(m_Data.m_SkillID);

                SetSpeed(0.0f);
                m_Data.m_StartTime = 0.0f;
                MoveToPosition(Vector3.zero);
            }
        }
    }

    [BehaveAction(typeof(BTLaserIrradiation), "LaserIrradiation")]
    public class BTLaserIrradiation : BTAttackBase
    {
        class Data : IAttackPositive
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minAngle = 0.0f;
            [BehaveAttribute]
            public float maxAngle = 0.0f;
            [BehaveAttribute]
            public float minHeight = 0.0f;
            [BehaveAttribute]
            public float maxHeight = 0.0f;
            [BehaveAttribute]
            public float minHpPercent = 0.0f;
            [BehaveAttribute]
            public float maxHpPercent = 1.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
            [BehaveAttribute]
            public float speed = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;
            [BehaveAttribute]
            public bool isBlock = false;

            //List<int> m_Skills = new List<int>();

            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;
            public float m_Time = 0.0f;
            public Vector3 m_Direction;

            public float MinRange { get { return minRange; } }
            public float MaxRange { get { return maxRange; } }

            bool IsInHpRange(Enemy enemy)
            {
                return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
            }

            public float Weight { get { return prob; } }

            public float MinHeight { get { return minHeight; } }

            public float MaxHeight { get { return maxHeight; } }

            public bool IsReadyCD(Enemy enemy)
            {
                return Time.time - m_LastCDTime > cdTime && enemy.entity.IsSkillRunable(skillID);
            }

            public bool ReadyAttack(Enemy enemy)
            {
                if (!IsInRange(enemy))
                    return false;

                if (!IsInAngle(enemy))
                    return false;

                if (!IsInHpRange(enemy))
                    return false;

                if (IsBlocked(enemy))
                    return false;

                return true;
            }

            public bool CanAttack(Enemy enemy)
            {
                if (enemy.entity.Field == MovementField.Sky && enemy.IsInWater)
                    return false;

                if (enemy.entity.Field == MovementField.Land && !enemy.IsOnLand)
                    return false;

                if (enemy.entity.Field == MovementField.water && !enemy.IsInWater)
                    return false;

                if (!Physics.Raycast(enemy.entityTarget.position + Vector3.up, Vector3.down, 6f, 1<<Layer.VFVoxelTerrain))
                    return false;

                return true;
            }

            public Vector3 GetAttackPosition(Enemy enemy)
            {
                float radius = ((minRange + maxRange) * 0.5f + enemy.entity.maxRadius + enemy.entityTarget.maxRadius);
                float height = (minHeight + maxHeight) * 0.5f + enemy.entity.maxHeight;
                Vector3 newPos = enemy.entityTarget.position - enemy.DirectionXZ.normalized * radius;

                RaycastHit hitInfo;
                if (Physics.Raycast(enemy.entityTarget.position + Vector3.up * 128f, Vector3.down, out hitInfo, 256f, Layer.VFVoxelTerrain))
                    newPos = hitInfo.point + Vector3.up * height;

                return newPos;
            }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID, false);
            }

            public bool IsInRange(Enemy enemy)
            {
                float sqrDistance = enemy.SqrDistanceXZ;
                return sqrDistance >= minRange * minRange && sqrDistance <= maxRange * maxRange;
            }

            public bool IsInAngle(Enemy enemy)
            {
                if (enemy.Inside)
                    return true;

                return PEUtil.IsScopeAngle(enemy.Direction, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle);
            }

            public bool IsBlocked(Enemy enemy)
            {
                return isBlock && PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
            }

            public bool CanInterrupt()
            {
                return false;
            }
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;

            if (!entity.IsSkillRunable(m_Data.skillID))
                return BehaveResult.Failure;

            StopMove();

            m_Data.m_LastCDTime = Time.time;
            m_Data.m_StartTime = Time.time;

            if (m_Data.speed <= Mathf.Epsilon)
                m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            else
                m_Data.m_Time = (attackEnemy.DistanceXZ + 10f) / m_Data.speed;
            m_Data.m_Direction = attackEnemy.DirectionXZ;

            SetSpeed(m_Data.speed);
            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
                return BehaveResult.Success;

            FaceDirection(m_Data.m_Direction);
            MoveDirection(m_Data.m_Direction);
            Debug.DrawRay(position, m_Data.m_Direction.normalized*5, Color.grey);
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if (IsSkillRunning(m_Data.skillID))
                    StopSkill(m_Data.skillID);

                SetSpeed(0.0f);
                m_Data.m_StartTime = 0.0f;
                FaceDirection(Vector3.zero);
                MoveDirection(Vector3.zero);
            }
        }
    }

    [BehaveAction(typeof(BTAttackTop), "AttackTop")]
    public class BTAttackTop : BTAttackBase
    {
        class Data : IAttackTop
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
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
            [BehaveAttribute]
            public float minHpPercent = 0.0f;
            [BehaveAttribute]
            public float maxHpPercent = 1.0f;
            [BehaveAttribute]
            public int skillID = 0;
            [BehaveAttribute]
            public bool isBlock = false;

            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;

            public float MinRange { get { return minRange; } }
            public float MaxRange { get { return maxRange; } }

            bool IsInHpRange(Enemy enemy)
            {
                return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
            }

            public float Weight { get { return prob; } }

            public float MinHeight { get { return minHeight; } }

            public float MaxHeight { get { return maxHeight; } }

            public bool IsReadyCD(Enemy enemy)
            {
                return Time.time - m_LastCDTime >= cdTime && enemy.entity.IsSkillRunable(skillID);
            }

            public bool ReadyAttack(Enemy enemy)
            {
                if (!IsInRange(enemy))
                    return false;

                if (!IsInAngle(enemy))
                    return false;

                if (!IsInHpRange(enemy))
                    return false;

                if (IsBlocked(enemy))
                    return false;

                if (!Physics.Raycast(enemy.entityTarget.position + Vector3.up, Vector3.down, 6f, 1<<Layer.VFVoxelTerrain))
                    return false;

                return true;
            }

            public bool CanAttack(Enemy enemy)
            {
                if (enemy.entity.Field == MovementField.Sky && enemy.IsInWater)
                    return false;

                if (enemy.entity.Field == MovementField.Land && !enemy.IsOnLand)
                    return false;

                if (enemy.entity.Field == MovementField.water && !enemy.IsInWater)
                    return false;

                return true;
            }

            public Vector3 GetAttackPosition(Enemy enemy)
            {
                float radius = (minRange + maxRange) * 0.5f + enemy.entity.maxRadius + enemy.entityTarget.maxRadius;
                float height = (minHeight + maxHeight) * 0.5f + enemy.entityTarget.maxHeight;
                Vector3 newPos = enemy.entityTarget.position - enemy.DirectionXZ.normalized * radius;

                RaycastHit hitInfo;
                if (Physics.Raycast(enemy.entityTarget.position + Vector3.up * 128f, Vector3.down, out hitInfo, 256f, Layer.VFVoxelTerrain))
                    newPos = hitInfo.point + Vector3.up * height;

                return newPos;
            }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID, false);
            }

            public bool IsInRange(Enemy enemy)
            {
                float _height = enemy.entity.position.y - enemy.entityTarget.position.y - enemy.entityTarget.maxHeight;

                return enemy.SqrDistanceXZ >= minRange * minRange 
                    && enemy.SqrDistanceXZ <= maxRange * maxRange
                    && _height <= maxHeight
                    && _height >= minHeight;
            }

            public bool IsInAngle(Enemy enemy)
            {
                return Vector3.Angle(-enemy.entity.tr.up, enemy.Direction) <= angle;
            }

            public bool IsBlocked(Enemy enemy)
            {
                return isBlock && PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
            }

            public bool CanInterrupt()
            {
                return false;
            }
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;

            if (!entity.IsSkillRunable(m_Data.skillID))
                return BehaveResult.Failure;

            StopMove();

            m_Data.m_StartTime = Time.time;
            m_Data.m_LastCDTime = Time.time;
            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime < 0.5f)
                return BehaveResult.Running;

            if (!IsSkillRunning(m_Data.skillID))
                return BehaveResult.Success;
            else
                return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if (IsSkillRunning(m_Data.skillID))
                    StopSkill(m_Data.skillID);

                m_Data.m_StartTime = 0.0f;
            }
        }
    }
}