//#define ATTACK_OLD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Random = UnityEngine.Random;
using Pathea;
using System;

namespace Behave.Runtime.Action
{
    public interface IAttack
    {
        //int GetAttackMode();
        //int GetAttackTerrain();
        bool CanInterrupt();
        bool IsRunning(Enemy enemy);
        bool IsReadyCD(Enemy enemy);
        bool ReadyAttack(Enemy enemy);
        bool CanAttack(Enemy enemy);
        bool IsBlocked(Enemy enemy);
        float Weight { get; }
    }

    public interface IAttackPositive : IAttack
    {
        Vector3 GetAttackPosition(Enemy enemy);
        float MinRange { get; }
        float MaxRange { get; }
        float MinHeight { get; }
        float MaxHeight { get; }
        bool IsInRange(Enemy enemy);
        bool IsInAngle(Enemy enemy);
    }

    public interface IAttackTop : IAttackPositive
    {

    }

    public interface IAttackRanged : IAttackPositive
    {

    }

    public class BTAttackBase : BTNormal
    {
        
    }

    [BehaveAction(typeof(BTCalculateAttackMode), "CalculateAttackMode")]
    public class BTCalculateAttackMode : BTNormal
    {
        float m_LastTime;

        List<IAttack> m_Attacks = new List<IAttack>();

        BehaveResult Tick(Tree sender)
        {
            if (entity == null || entity.target == null || attackEnemy == null)
                return BehaveResult.Failure;

            IAttack tmp = null;

            List<IAttack> atts = entity.target.Attacks;

            m_Attacks.Clear();

            float totalWeight = 0;
            float curWeight = 0.0f;
            float randValue = Random.value;

            for (int i = 0; i < atts.Count; i++)
            {
                if (atts[i].IsReadyCD(attackEnemy) && (atts[i].ReadyAttack(attackEnemy) || (atts[i] is IAttackTop)))
                {
                    totalWeight += atts[i].Weight;
                    m_Attacks.Add(atts[i]);
                }
            }

            for (int i = 0; i < m_Attacks.Count; i++)
            {
                curWeight += (m_Attacks[i].Weight / totalWeight);

                if(randValue <= curWeight)
                {
                    tmp = m_Attacks[i];
                    break;
                }
            }

            if(tmp == null && Time.time - m_LastTime > 2.0f)
            {
                m_Attacks.Clear();

                totalWeight = 0.0f;
                curWeight = 0.0f;
                randValue = Random.value;

                for (int i = 0; i < atts.Count; i++)
                {
                    if (atts[i] is IAttackPositive)
                    {
                        totalWeight += atts[i].Weight;
                        m_Attacks.Add(atts[i]);
                    }
                }

                for (int i = 0; i < m_Attacks.Count; i++)
                {
                    curWeight += (m_Attacks[i].Weight / totalWeight);
                    if(randValue <= curWeight)
                    {
                        tmp = m_Attacks[i];
                        m_LastTime = Time.time;
                        break;
                    }
                }
            }

            if (tmp != null)
            {
                attackEnemy.Attack = tmp;

                if (attackEnemy.entityTarget.target != null)
                {
                    if (tmp is BTMelee || tmp is BTMeleeAttack)
                        attackEnemy.entityTarget.target.AddMelee(entity);

                    if (tmp is BTAttackRanged)
                        attackEnemy.entityTarget.target.RemoveMelee(entity);
                }
            }

            if(attackEnemy.Attack != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTMeleeAttack), "MeleeAttack")]
    public class BTMeleeAttack : BTAttackBase
    {
        class Data : IAttack
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
            public float minHpPercent = 0.0f;
            [BehaveAttribute]
            public float maxHpPercent = 1.0f;
            [BehaveAttribute]
            public Vector3 axis = Vector3.zero;
            [BehaveAttribute]
            public float angle = 0.0f;
            [BehaveAttribute]
            public bool isBlock = true;
            [BehaveAttribute]
            public int[] skillStr = new int[0];

            List<int> m_Skills = new List<int>();

            public int m_SkillID = 0;
            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;

            bool IsInHpRange(Enemy enemy)
            {
                return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
            }

            public float Weight { get { return prob; } }

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
                if (!IsInHpRange(enemy))
                    return false;

                if (enemy.SqrDistanceLogic < minRange * minRange || enemy.SqrDistanceLogic > maxRange * maxRange)
                    return false;

                if (!enemy.Inside && !PEUtil.IsScopeAngle(enemy.Direction, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle))
                    return false;

                if(axis != Vector3.zero)
                {
                    Vector3 axisDir = enemy.entity.tr.TransformDirection(axis);
                    if (Vector3.Angle(axisDir, enemy.Direction) > angle)
                        return false;
                }

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

                return !IsBlocked(enemy);
            }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(m_SkillID, false);
            }

            public bool IsBlocked(Enemy enemy)
            {
                return isBlock && PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
            }

            public bool CanInterrupt()
            {
                return true;
            }
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (PEUtil.IsDamageBlock(entity))
            {
                m_Data.m_SkillID = m_Data.GetRandomSkill(attackEnemy);
                if (m_Data.m_SkillID <= 0)
                    return BehaveResult.Failure;

                if (!entity.IsSkillRunable(m_Data.m_SkillID))
                    return BehaveResult.Failure;

                StartSkillSkEntity(Block45Man.self, m_Data.m_SkillID);
                return BehaveResult.Running;
            }
            else
            {
                if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                    return BehaveResult.Failure;

                if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                    return BehaveResult.Failure;

                m_Data.m_SkillID = m_Data.GetRandomSkill(attackEnemy);
                if (m_Data.m_SkillID <= 0)
                    return BehaveResult.Failure;

                StopMove();
                m_Data.m_LastCDTime = Time.time;
                m_Data.m_StartTime = Time.time;
                //StartSkill(attackEnemy.entityTarget, m_Data.m_SkillID);
                StartSkill(null, m_Data.m_SkillID);
                return BehaveResult.Running;
            }
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (IsSkillRunning(m_Data.m_SkillID) || entity.IsAttacking)
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if (IsSkillRunning(m_Data.m_SkillID))
                    StopSkill(m_Data.m_SkillID);

                m_Data.m_StartTime = 0.0f;
            }
        }
    }

#if ATTACK_OLD
    [BehaveAction(typeof(BTMeleeRotate), "MeleeRotate")]
    public class BTMeleeRotate : BTAttackBase
    {
        class Data
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
            public string skillStr = "";

            public int m_SkillID = 0;

            int[] m_SkillIDs;
            float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;
            public bool m_CanAttack;
            public bool m_Arrived;

            public void SetCDTime(float time)
            {
                m_LastCDTime = time;
            }

            public int GetRandomSkill()
            {
                if (m_SkillIDs != null && m_SkillIDs.Length > 0)
                    return m_SkillIDs[Random.Range(0, m_SkillIDs.Length)];

                return 0;
            }

            public bool Ready()
            {
                if (Cooldown())
                {
                    return Random.value <= prob;
                }

                return false;
            }

            public void Init()
            {
                if (m_SkillIDs == null)
                {
                    m_SkillIDs = PEUtil.ToArrayInt32(skillStr, ',');
                }
            }

            bool Cooldown()
            {
                return Time.time - m_LastCDTime > cdTime;
            }
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            m_Data.Init();

            if (!m_Data.Ready())
                return BehaveResult.Failure;

            if (attackEnemy.SqrDistanceLogic < m_Data.minRange * m_Data.minRange || attackEnemy.SqrDistanceLogic > m_Data.maxRange * m_Data.maxRange)
                return BehaveResult.Failure;

            m_Data.m_SkillID = m_Data.GetRandomSkill();
            if (m_Data.m_SkillID <= 0)
                return BehaveResult.Failure;

            if (!IsSkillRunnable(m_Data.m_SkillID))
                return BehaveResult.Failure;

            m_Data.m_CanAttack = true;
            m_Data.m_Arrived = false;
            m_Data.m_StartTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            Vector3 dir = attackEnemy.position - position;

            if (m_Data.m_CanAttack)
            {
                if (!PEUtil.IsScopeAngle(dir, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
                    FaceDirection(dir);
                else
                {
                    FaceDirection(Vector3.zero);
                    m_Data.m_CanAttack = false;
                    m_Data.SetCDTime(Time.time);
                    StartSkill(attackEnemy.skTarget, m_Data.m_SkillID);
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
    }
#endif

    [BehaveAction(typeof(BTMelee), "Melee")]
    public class BTMelee : BTAttackBase
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
            public int[] skillStr = new int[0];
            [BehaveAttribute]
            public bool isBlock = true;

            List<int> m_Skills = new List<int>();

            public int m_SkillID = 0;

            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;

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

                return !IsBlocked(enemy);
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

                //if (sqrDistance < minRange * minRange || sqrDistance > maxRange * maxRange)
                //    return false;

                //if (Mathf.Abs(minHeight) < PETools.PEMath.Epsilon && Mathf.Abs(maxHeight) < PETools.PEMath.Epsilon)
                //    return true;

                //float yh = Mathf.Abs(enemy.entityTarget.position.y - enemy.entity.position.y);

                //return yh >= minHeight && yh <= maxHeight;

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
                return true;
            }
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            if (PEUtil.IsDamageBlock(entity))
            {
                m_Data.m_SkillID = m_Data.GetRandomSkill(attackEnemy);
                if (m_Data.m_SkillID <= 0)
                    return BehaveResult.Failure;

                if (!entity.IsSkillRunable(m_Data.m_SkillID))
                    return BehaveResult.Failure;

                StartSkillSkEntity(Block45Man.self, m_Data.m_SkillID);
                return BehaveResult.Running;
            }
            else
            {
                if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                    return BehaveResult.Failure;

                if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                    return BehaveResult.Failure;

                m_Data.m_SkillID = m_Data.GetRandomSkill(attackEnemy);
                if (m_Data.m_SkillID <= 0)
                    return BehaveResult.Failure;

                if (!entity.IsSkillRunable(m_Data.m_SkillID))
                    return BehaveResult.Failure;

                StopMove();
                m_Data.m_LastCDTime = Time.time;
                //StartSkill(attackEnemy.entityTarget, m_Data.m_SkillID);
                StartSkill(null, m_Data.m_SkillID);
                m_Data.m_StartTime = Time.time;
                return BehaveResult.Running;
            }
        }


        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (IsSkillRunning(m_Data.m_SkillID) || entity.IsAttacking)
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if (IsSkillRunning(m_Data.m_SkillID))
                    StopSkill(m_Data.m_SkillID);

                m_Data.m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTAttackRanged), "AttackRanged")]
    public class BTAttackRanged : BTAttackBase
    {
        class Data : IAttackRanged
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
            public float minHpPercent = 0.0f;
            [BehaveAttribute]
            public float maxHpPercent = 1.0f;
            [BehaveAttribute]
            public float angle = 0.0f;
            [BehaveAttribute]
            public float pitchAngle = 0.0f;
            [BehaveAttribute]
            public string boneName = "";
            [BehaveAttribute]
            public Vector3 pivot = Vector3.forward;
            [BehaveAttribute]
            public bool isBlock = false;
            [BehaveAttribute]
            public int skillID = 0;

            public float m_LastCDTime = 0.0f;
            public float m_StartTime = 0.0f;


            public float MinRange { get { return minRange; } }
            public float MaxRange { get { return maxRange; } }

            bool IsInHpRange(Enemy enemy)
            {
                return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
            }

            public Vector3 GetAttackPosition(Enemy enemy)
            {
                float radius = ((minRange + maxRange) * 0.5f + enemy.entity.maxRadius + enemy.entityTarget.maxRadius);
                //float height = (minHeight + maxHeight) * 0.5f - enemy.entity.maxHeight * 0.5f;
                return enemy.entityTarget.position - enemy.DirectionXZ.normalized * radius /*+ height*Vector3.up*/;
            }

            public float Weight { get { return prob; } }

            public float MinHeight { get { return minHeight; } }

            public float MaxHeight { get { return maxHeight; } }

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                return IsInRange(enemy) && IsInAngle(enemy) &&  IsInHpRange(enemy) && !IsBlocked(enemy);
            }

            public bool CanAttack(Enemy enemy)
            {
                return true;
            }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID, false);
            }

            public bool IsInRange(Enemy enemy)
            {
                return enemy.SqrDistanceLogic >= minRange * minRange && enemy.SqrDistanceLogic <= maxRange * maxRange;
            }

            public bool IsInAngle(Enemy enemy)
            {
                return PEUtil.Angle(enemy.Direction, enemy.entity.tr.forward, Vector3.up) <= angle;
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
#else
            if (attackEnemy == null || attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            //Transform pitchBone = GetModelName(m_Data.boneName);
            //if (pitchBone != null)
            //{
            //    Vector3 direction = attackEnemy.position - pitchBone.position;
            //    Vector3 forward = pitchBone.TransformDirection(m_Data.pivot);
            //    if (PEUtil.Angle(forward, direction, transform.right) > m_Data.pitchAngle)
            //        return BehaveResult.Failure;
            //}

            StopMove();
            m_Data.m_StartTime = Time.time;
#if ATTACK_OLD
            m_Data.m_CanAttack = true;
            m_Data.m_Arrived = false;
#else
            m_Data.m_LastCDTime = Time.time;
            StartSkill(attackEnemy.entityTarget, m_Data.skillID);
#endif
            return BehaveResult.Running;
        }

#if ATTACK_OLD
        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
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
                    if (PEUtil.Angle(transform.forward, dir, Vector3.up) > m_Data.angle)
                        FaceDirection(dir);
                    else
                    {
                        FaceDirection(Vector3.zero);
                        StopMove();
                        m_Data.m_CanAttack = false;
                        m_Data.m_LastCDTime = Time.time;
                        StartSkill(attackEnemy.skTarget, m_Data.skillID);
                    }
                }

                if (m_Data.m_CanAttack && Time.time - m_Data.m_StartTime > 2.0f)
                    return BehaveResult.Failure;

                return BehaveResult.Running;
            }
            else
            {
                if (IsSkillRunning(m_Data.skillID))
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

            if (IsSkillRunning(m_Data.skillID) || entity.IsAttacking)
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
#endif

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if (IsSkillRunning(m_Data.skillID))
                    StopSkill(m_Data.skillID);

                if(entity.IsAttacking)
                    SetBool("Interrupt", true);


                m_Data.m_StartTime = 0.0f;
            }
        }
    }

    //[BehaveAction(typeof(BTAttack), "Attack")]
    //public class BTAttack : BTAttackBase
    //{
    //    class Data
    //    {
    //        [BehaveAttribute]
    //        public float prob = 0.0f;
    //        [BehaveAttribute]
    //        public float cdTime = 0.0f;
    //        [BehaveAttribute]
    //        public float minRange = 0.0f;
    //        [BehaveAttribute]
    //        public float maxRange = 0.0f;
    //        [BehaveAttribute]
    //        public float minHeight = 0.0f;
    //        [BehaveAttribute]
    //        public float maxHeight = 0.0f;
    //        [BehaveAttribute]
    //        public float angle = 0.0f;
    //        [BehaveAttribute]
    //        public int skillID = 0;

    //        float m_LastCDTime = 0.0f;

    //        public void SetCDTime(float time)
    //        {
    //            m_LastCDTime = time;
    //        }

    //        public bool Ready()
    //        {
    //            if (Cooldown())
    //            {
    //                return Random.value <= prob;
    //            }

    //            return false;
    //        }

    //        bool Cooldown()
    //        {
    //            return Time.time - m_LastCDTime > cdTime;
    //        }
    //    }

    //    Data m_Data;

    //    BehaveResult Init(Tree sender)
    //    {
    //        if (!GetData<Data>(sender, ref m_Data))
    //            return BehaveResult.Failure;

    //        if (!m_Data.Ready())
    //            return BehaveResult.Failure;

    //        if (!IsSkillRunnable(m_Data.skillID))
    //            return BehaveResult.Failure;

    //        Vector3 dir = attackEnemy.position - position;
    //        float sqrDistanceXZ = attackEnemy.sqrDistanceReal;
    //        if (sqrDistanceXZ < m_Data.minRange * m_Data.minRange
    //            || sqrDistanceXZ > m_Data.maxRange * m_Data.maxRange)
    //            return BehaveResult.Failure;

    //        Vector3 v1 = Vector3.ProjectOnPlane(dir, Vector3.up);
    //        Vector3 v2 = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
    //        if (Vector3.Angle(v1, v2) > m_Data.angle)
    //            return BehaveResult.Failure;

    //        m_Data.SetCDTime(Time.time);
    //        StartSkill(attackEnemy.skTarget, m_Data.skillID);
    //        return BehaveResult.Running;
    //    }

    //    BehaveResult Tick(Tree sender)
    //    {
    //        if (!GetData<Data>(sender, ref m_Data))
    //            return BehaveResult.Failure;

    //        if (IsSkillRunning(m_Data.skillID))
    //            return BehaveResult.Running;
    //        else
    //            return BehaveResult.Success;
    //    }
    //}

    [BehaveAction(typeof(BTAttackBone), "AttackBone")]
    public class BTAttackBone : BTAttackBase
    {
        class Data : IAttack
        {
            [BehaveAttribute]
            public string boneName = "";
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float angle = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;

            public float m_LastCDTime = 0.0f;

            public float Weight { get { return prob; } }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID, false);
            }

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                if (enemy.SqrDistanceLogic < minRange * minRange || enemy.SqrDistanceLogic > maxRange * maxRange)
                    return false;
                else
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
        PEBoneRotation m_Bone;

        bool m_CanAttack;
        bool m_Attacked;

        PEBoneRotation GetBone()
        {
            if(entity != null)
            {
                BiologyViewCmpt view = entity.biologyViewCmpt;
                if(view != null)
                {
                    Transform tr = view.GetModelTransform(m_Data.boneName);
                    if (tr != null)
                        return tr.GetComponent<PEBoneRotation>();
                }
            }

            return null;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;
#if ATTACK_OLD
            if (!m_Data.Ready())
                return BehaveResult.Failure;

            if (!IsSkillRunnable(m_Data.skillID))
                return BehaveResult.Failure;

//            Vector3 dir = attackEnemy.position - position;
            float sqrDistanceXZ = attackEnemy.sqrDistanceReal;
            if (sqrDistanceXZ < m_Data.minRange * m_Data.minRange
                || sqrDistanceXZ > m_Data.maxRange * m_Data.maxRange)
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            if (m_Bone == null)
                m_Bone = GetBone();

            if (m_Bone == null)
                return BehaveResult.Failure;

            m_CanAttack = false;
            m_Attacked = false;

            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if(m_CanAttack)
            {
                if(!m_Attacked)
                {
                    m_Attacked = true;
                    m_Data.m_LastCDTime = Time.time;
                    StartSkill(attackEnemy.entityTarget, m_Data.skillID);
                    return BehaveResult.Running;
                }
                else
                {
                    if (IsSkillRunning(m_Data.skillID))
                        return BehaveResult.Running;
                    else
                        return BehaveResult.Success;
                }
            }
            else
            {
                Vector3 v1 = Vector3.ProjectOnPlane(attackEnemy.position - position, Vector3.up);
                Vector3 v2 = Vector3.ProjectOnPlane(m_Bone.transform.forward, Vector3.up);

                if (Vector3.Angle(v1, v2) > m_Data.angle)
                    m_Bone.target = attackEnemy.modelTrans;
                else
                {
                    m_CanAttack = true;
                    m_Bone.target = null;
                }

                return BehaveResult.Running;
            }
        }
    }

    [BehaveAction(typeof(BTSprint), "Sprint")]
    public class BTSprint : BTAttackBase
    {
        class Data : IAttack
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public string sprint = "";
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minAngle = 0.0f;
            [BehaveAttribute]
            public float maxAngle = 0.0f;
            [BehaveAttribute]
            public float startTime = 0.0f;
            [BehaveAttribute]
            public float endTime = 0.0f;
            [BehaveAttribute]
            public float stopTime = 0.0f;
            [BehaveAttribute]
            public float speed = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;
            [BehaveAttribute]
            public bool auto = false;

            public float m_StartTime;
            public float m_StopTime;
            public float m_EndTime;
            public float m_StartRotateTime;
            public bool m_Started;
            public bool m_Stoped;
            public bool m_Face;
            float m_LastCDTime = 0.0f;

            public void SetCDTime(float time)
            {
                m_LastCDTime = time;
            }

            public void SetEndTime(float time)
            {
                m_EndTime = time;
            }

            public bool Ready()
            {
                if (Cooldown())
                {
                    return Random.value <= prob;
                }

                return false;
            }

            public float GetEndTime()
            {
                if (auto)
                    return m_EndTime;
                else
                    return endTime;
            }

            bool Cooldown()
            {
                return Time.time - m_LastCDTime > cdTime;
            }

            public float Weight { get { return prob; } }

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                if (enemy.SqrDistanceLogic < minRange * minRange || enemy.SqrDistanceLogic > maxRange * maxRange)
                    return false;

                Vector3 dir = enemy.position - enemy.entity.position;
                if (!PEUtil.IsScopeAngle(dir, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle))
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

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID, false);
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

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

#if ATTACK_OLD
            if (!m_Data.Ready())
                return BehaveResult.Failure;

            float sqrDistanceXZ = attackEnemy.sqrDistanceXZ;
            if (sqrDistanceXZ < m_Data.minRange * m_Data.minRange || sqrDistanceXZ > m_Data.maxRange * m_Data.maxRange)
                return BehaveResult.Failure;

            Vector3 direction = attackEnemy.position - position;
            if (!PEUtil.IsScopeAngle(direction, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
                return BehaveResult.Failure;
#else
            if (attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            m_Data.m_Started = false;
            m_Data.m_Stoped = false;
            m_Data.m_Face = false;
            m_Data.m_StartRotateTime = Time.time;

            StopMove();
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            Vector3 v = attackEnemy.position - position;
            if (!m_Data.m_Face)
                m_Data.m_Face = PEUtil.IsScopeAngle(v, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle);

            if (!m_Data.m_Face)
            {
                FaceDirection(v);

                if (Time.time - m_Data.m_StartRotateTime > 5.0f)
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            if (!m_Data.m_Started)
            {
                m_Data.m_Started = true;
                m_Data.m_StartTime = Time.time;
                m_Data.SetCDTime(Time.time);
                float d = PEUtil.MagnitudeH(position, attackEnemy.position) - radius - attackEnemy.radius;
                m_Data.SetEndTime(d / m_Data.speed);

                SetBool(m_Data.sprint, true);
                StartSkill(attackEnemy.entityTarget, m_Data.skillID);
                return BehaveResult.Running;
            }

            if (m_Data.m_Stoped && Time.time - m_Data.m_StartTime > m_Data.stopTime)
                return BehaveResult.Success;
            else
            {
                if (Time.time - m_Data.m_StartTime < m_Data.startTime)
                {
                    StopMove();
                }
                else if (Time.time - m_Data.m_StartTime > m_Data.endTime)
                {
                    if (!m_Data.m_Stoped)
                    {
                        m_Data.m_Stoped = true;
                        m_Data.m_StopTime = Time.time;
                        SetBool(m_Data.sprint, false);
                        MoveDirection(Vector3.zero);
                        FaceDirection(Vector3.zero);
                    }
                }
                else
                {
                    SetSpeed(m_Data.speed);
                    MoveDirection(transform.forward, SpeedState.Sprint);
                    FaceDirection(transform.forward);
                }

                return BehaveResult.Running;
            }
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                SetSpeed(0.0f);
                SetBool(m_Data.sprint, false);
                StopSkill(m_Data.skillID);
                MoveDirection(Vector3.zero);
                FaceDirection(Vector3.zero);

                m_Data.m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTPounce), "Pounce")]
    public class BTPounce : BTAttackBase
    {
        class Data :IAttack
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public string pounce = "";
            [BehaveAttribute]
            public float minRange = 0.0f;
            [BehaveAttribute]
            public float maxRange = 0.0f;
            [BehaveAttribute]
            public float minAngle = 0.0f;
            [BehaveAttribute]
            public float maxAngle = 0.0f;
            [BehaveAttribute]
            public float startTime = 0.0f;
            [BehaveAttribute]
            public float endTime = 0.0f;
            [BehaveAttribute]
            public float stopTime = 0.0f;
            [BehaveAttribute]
            public int skillID = 0;

            public float m_Speed;
            public float m_StartTime;
            public float m_StopTime;
            //public float m_EndTime;
            public float m_StartRotateTime;
            public bool m_Started;
            public bool m_Stoped;
            public bool m_Face;
            float m_LastCDTime = 0.0f;

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

            public bool IsReadyCD(Enemy enemy)
            {
                return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
            }

            public bool ReadyAttack(Enemy enemy)
            {
                if (enemy.SqrDistanceLogic < minRange * minRange || enemy.SqrDistanceLogic > maxRange * maxRange)
                    return false;

                Vector3 dir = enemy.position - enemy.entity.position;
                if (!PEUtil.IsScopeAngle(dir, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle))
                    return false;

                return !IsBlocked(enemy);
            }

            public bool CanAttack(Enemy enemy)
            {
                if (enemy.entity.Field == MovementField.Sky && enemy.IsInWater)
                    return false;

                if (enemy.entity.Field == MovementField.Land && !enemy.IsOnLand)
                    return false;

                if (enemy.entity.Field == MovementField.water && !enemy.IsInWater)
                    return false;

                return !IsBlocked(enemy);
            }

            public bool IsRunning(Enemy enemy)
            {
                return enemy.entity.IsSkillRunning(skillID, false);
            }

            public bool IsBlocked(Enemy enemy)
            {
                return PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
            }

            public bool CanInterrupt()
            {
                return true;
            }
        }

        Data m_Data;
        float m_Gravity;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

#if ATTACK_OLD
            if (!m_Data.Ready())
                return BehaveResult.Failure;

            float sqrDistanceXZ = attackEnemy.sqrDistanceXZ;
            if (sqrDistanceXZ < m_Data.minRange * m_Data.minRange || sqrDistanceXZ > m_Data.maxRange * m_Data.maxRange)
                return BehaveResult.Failure;

            Vector3 direction = attackEnemy.position - position;
            if (!PEUtil.IsScopeAngle(direction, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
                return BehaveResult.Failure;
#else
            if (attackEnemy == null || attackEnemy.Attack == null || !attackEnemy.Attack.Equals(m_Data))
                return BehaveResult.Failure;

            if (!attackEnemy.Attack.ReadyAttack(attackEnemy))
                return BehaveResult.Failure;
#endif

            m_Data.m_Started = false;
            m_Data.m_Stoped = false;
            m_Data.m_Face = false;
            m_Data.m_Speed = 0.0f;
            m_Data.m_StartRotateTime = Time.time;

            m_Gravity = entity.gravity;
            entity.gravity = 0.0f;

			if (entity.motionMove != null && entity.motionMove is Motion_Move_Motor) {
				Motion_Move_Motor motor = entity.motionMove as Motion_Move_Motor;
				if(motor != null && motor.motor != null){
					motor.motor.desiredMovementEffect = Vector3.zero;
				}
			}

            StopMove();
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (attackEnemy == null)
                return BehaveResult.Failure;

            Vector3 v = attackEnemy.position - position;
            if (!m_Data.m_Face)
                m_Data.m_Face = PEUtil.IsScopeAngle(v, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle);

            if (!m_Data.m_Face)
            {
                FaceDirection(v);

                if (Time.time - m_Data.m_StartRotateTime > 5.0f)
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            if (!m_Data.m_Started)
            {
                m_Data.m_Started = true;
                m_Data.m_StartTime = Time.time;

                m_Data.SetCDTime(Time.time);

                SetBool(m_Data.pounce, true);
                StartSkill(attackEnemy.entityTarget, m_Data.skillID);
                return BehaveResult.Running;
            }

            if (m_Data.m_Stoped && Time.time - m_Data.m_StartTime > m_Data.stopTime)
                return BehaveResult.Success;
            else
            {
                if (Time.time - m_Data.m_StartTime < m_Data.startTime)
                {
                    StopMove();
                }
                else if (Time.time - m_Data.m_StartTime > m_Data.endTime)
                {
                    if (!m_Data.m_Stoped)
                    {
                        StopMove();
                        m_Data.m_Stoped = true;
                        m_Data.m_StopTime = Time.time;
                        SetBool(m_Data.pounce, false);
                        MoveDirection(Vector3.zero);
                        FaceDirection(Vector3.zero);
                        MoveToPosition(attackEnemy.position, SpeedState.Run);
                        return BehaveResult.Success;
                    }
                }
                else
                {
                    if(m_Data.m_Speed < PETools.PEMath.Epsilon)
                        m_Data.m_Speed = (PEUtil.MagnitudeH(position, attackEnemy.position) + 1f) / (m_Data.endTime - m_Data.startTime);

                    SetSpeed(m_Data.m_Speed);
                    MoveDirection(transform.forward);
                    FaceDirection(attackEnemy.DirectionXZ);

                    //if (!entity.IsAttacking)
                    //    return BehaveResult.Failure;
                }

                return BehaveResult.Running;
            }
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if (m_Data.m_StartRotateTime > PETools.PEMath.Epsilon)
            {
                SetSpeed(0.0f);
                SetBool(m_Data.pounce, false);
                //StopSkill(m_Data.skillID);
                MoveDirection(Vector3.zero);
                FaceDirection(Vector3.zero);

                entity.gravity = m_Gravity;

                m_Data.m_StartRotateTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTMoveAnimator), "MoveAnimator")]
    public class BTMoveAnimator : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float startTime = 0.0f;
            [BehaveAttribute]
            public float endTime = 0.0f;
            [BehaveAttribute]
            public float time = 0.0f;
            [BehaveAttribute]
            public float speed = 0.0f;
            [BehaveAttribute]
            public int startSkill = 0;
            [BehaveAttribute]
            public int endSkill = 0;
            [BehaveAttribute]
            public Vector3 anchor = Vector3.zero;

            public float m_StartTime;
            public float m_LastCooldownTime;

            public bool m_startSkill;
            public bool m_endSkill;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCooldownTime <= m_Data.cdTime)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

			if (entity.motionMove != null && entity.motionMove is Motion_Move_Motor) {
				Motion_Move_Motor motor = entity.motionMove as Motion_Move_Motor;
				if(motor != null && motor.motor != null){
					motor.motor.desiredMovementEffect = Vector3.zero;
				}
			}

            m_Data.m_StartTime = Time.time;
            m_Data.m_startSkill = false;
            m_Data.m_endSkill = false;
            SetBool(m_Data.anim, true);
            StopMove();
            SetSpeed(m_Data.speed);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            float t = Time.time - m_Data.m_StartTime;

            if (t > m_Data.time)
            {
                m_Data.m_LastCooldownTime = Time.time;
                return BehaveResult.Success;
            }

            if (t >= m_Data.startTime && t <= m_Data.endTime)
            {
                MoveDirection(transform.TransformDirection(m_Data.anchor));
                FaceDirection(transform.forward);
                SetSpeed(m_Data.speed);
            }
            else
            {
                MoveDirection(Vector3.zero);
                FaceDirection(Vector3.zero);
            }

            if (m_Data.startSkill > 0 && !m_Data.m_startSkill && t > m_Data.startTime)
            {
                m_Data.m_startSkill = true;
                StartSkill(null, m_Data.startSkill);
            }

            if (m_Data.m_startSkill && t > m_Data.endTime && IsSkillRunning(m_Data.startSkill))
                StopSkill(m_Data.startSkill);

            if (m_Data.endSkill > 0 && !m_Data.m_endSkill && t > m_Data.endTime)
            {
                m_Data.m_endSkill = true;
                StartSkill(null, m_Data.endSkill);
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
			if (!GetData<Data>(sender, ref m_Data))
				return;

            if (m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                SetSpeed(0.0f);
                SetBool(m_Data.anim, false);

                MoveDirection(Vector3.zero);
                FaceDirection(Vector3.zero);

                m_Data.m_StartTime = 0.0f;
            }
        }
    }

    [BehaveAction(typeof(BTAttackWeapon), "AttackWeapon")]
    public class BTAttackWeapon : BTAttackBase
    {
        const float Mortar_MaxRange = 55.0f;
        const float Mortar_MinRange = 8.0f;
        const float Mortar_Offset = 30.0f;

        bool m_DamageBlock;
        bool m_Attacked;
        bool m_MortarWaitFor = false;
        int m_Index;
        float m_LastRetreatTime;
        float m_CanAttackModeTime;
        IWeapon m_Weapon;
        AttackMode m_Mode;
        Vector3 m_Local;
        Vector3 m_RetreatLocal;
        List<int> m_ModeIndex = new List<int>();

		Vector3 GetRetreatPos(Vector3 TargetPos,Transform selfTrans)
		{
			Vector3 selfPos =selfTrans.position;
			Vector3 dir = (selfPos - TargetPos).normalized;
			Vector3 retreat = PEUtil.GetRandomPosition(selfPos, dir, 5.0f, 10.0f, -75f, 75f);
			if (PEUtil.CheckPositionUnderWater(retreat) || PEUtil.CheckPositionInSky(retreat))
			{
				dir = selfTrans.right;
				retreat = selfPos + dir * 10.0f;
			}

            Vector3 newpos;
            if (AiUtil.GetNearNodePosWalkable(retreat, out newpos))
                return newpos;
            else
                return retreat;
		}

		void SingTarget( IAimWeapon _aimWeapon,Enemy _attackenmy)
		{
			if (_aimWeapon != null && _attackenmy.CenterBone != null)
			{
				_aimWeapon.SetTarget(_attackenmy.CenterBone);
			}

			if (_aimWeapon != null && _attackenmy == null)
			{
				_aimWeapon.SetTarget(null);
			}
		}

        bool CanAttackMode(AttackMode mode, Enemy enemy)
        {
            return enemy.SqrDistance >= mode.minRange * mode.minRange && enemy.SqrDistance <= mode.maxRange * mode.maxRange;
        }

        AttackMode GetMode(IWeapon weapon, int index)
        {
            AttackMode[] modes = weapon.GetAttackMode();
            if (index < 0 || index >= modes.Length)
                return null;
            else
                return modes[index];
        }

        bool CanSelectRangedWeapon(AttackMode mode)
        {
            if (mode.frequency < 1.5f || !mode.IsInCD())
            {
                if (!attackEnemy.entityTarget.Equals(PeCreature.Instance.mainPlayer))
                    return true;

                if (CanAttackMode(mode, attackEnemy))
                    m_CanAttackModeTime = Time.time;

                if (Time.time - m_CanAttackModeTime < 2.0f)
                    return true;

                //if (Time.time - attackEnemy.LastDamagetime > 5.0f && attackEnemy.MoveDir != EEnemyMoveDir.Close)
                //    return true;
            }

            return false;
        }

        int SwitchAttackIndex(IWeapon weapon, Enemy enemy)
        {
            int tmpIndex = -1;
            m_ModeIndex.Clear();

            AttackMode[] modes = weapon.GetAttackMode();
            if (m_DamageBlock)
            {
                for (int i = 0; i < modes.Length; i++)
                {
                    if (modes[i].type == AttackType.Melee)
                        m_ModeIndex.Add(i);
                }
            }
            else
            {
                //目标没有攻击目标或者攻击目标不是自己的时候用远程攻击！
                TargetCmpt targetCmpt = attackEnemy.entityTarget.target;
                if (targetCmpt != null)
                {
                    Enemy e = targetCmpt.GetAttackEnemy();
                    PeEntity targetEntity = e != null ? e.entityTarget : null;
                    if (targetEntity == null || !targetEntity.Equals(entity))
                    {
                        for (int i = 0; i < modes.Length; i++)
                        {
                            if (modes[i].type == AttackType.Ranged && CanSelectRangedWeapon(modes[i]))
                            {
                                m_ModeIndex.Add(i);
                            }
                        }
                    }
                }

                if (m_ModeIndex.Count == 0)
                {
                    for (int i = 0; i < modes.Length; i++)
                    {
                        if (modes[i].type == AttackType.Ranged
                            && (CanAttackMode(modes[i], attackEnemy))
                            && (modes[i].frequency < 1.5f || !modes[i].IsInCD()))
                            m_ModeIndex.Add(i);
                    }
                }

                if(m_ModeIndex.Count == 0)
                {
                    for (int i = 0; i < modes.Length; i++)
                    {
                        if (modes[i].type == AttackType.Melee
                            && CanAttackMode(modes[i], attackEnemy)
                            && (modes[i].frequency < 1.5f || !modes[i].IsInCD()))
                            m_ModeIndex.Add(i);
                    }
                }

                if(m_ModeIndex.Count == 0)
                {
                    float minDis = Mathf.Infinity;

                    for (int i = 0; i < modes.Length; i++)
                    {
                        float tmpMinDis = Mathf.Min(Mathf.Abs(enemy.DistanceXZ - modes[i].minRange), Mathf.Abs(enemy.DistanceXZ - modes[i].maxRange));
                        if(tmpMinDis < minDis)
                        {
                            tmpIndex = i;
                            minDis = tmpMinDis;
                        }
                    }
                }
            }

            if (tmpIndex == -1 && m_ModeIndex.Count > 0)
                tmpIndex = m_ModeIndex[Random.Range(0, m_ModeIndex.Count)];

            return tmpIndex;
        }

        Vector3 GetLocalCenterPos()
        {
            return attackEnemy.position + m_Local + Vector3.up * entity.maxHeight * 0.5f;
        }

        Vector3 GetLocalPos(Enemy e, AttackMode attack)
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
                        Vector3 pos = PEUtil.GetRandomPositionOnGround(e.position, attack.minRange, attack.maxRange);
                        Vector3 offCenter = pos + Vector3.up * entity.maxHeight * 0.5f;

                        if (!PEUtil.IsBlocked(e.entityTarget, offCenter))
                        {
                            local = pos - e.position;
                            break;
                        }
                    }

                    m_Local = local;
                }
            }

            return m_Local;
        }

        Vector3 GetMovePos(Enemy e)
        {
            return e.position + m_Local;
        }

        void StandFromSquat()
        {
            if (GetBool("Squat"))
                SetBool("Squat", false);
        }

        void MortarTakeOff()
        {
            if (Weapon != null && !Weapon.Equals(null) && GetBool("Mortar"))
            {
                Weapon.HoldWeapon(false);
            }
        }

        #region Switch Weapon
        List<IWeapon> m_Weapons = new List<IWeapon>();

        //迫击炮在最小距离内不能被选中
        bool CanAttackWeaponMotar(IWeapon weapon, Enemy enemy)
        {
            PEEquipment equip = weapon as PEEquipment;
            if (equip.m_ItemObj == null || equip.m_ItemObj.protoId != 1143)
                return true;

            AttackMode[] modes = weapon.GetAttackMode();
            for (int i = 0; i < modes.Length; i++)
            {
                if (enemy.Distance > modes[i].minRange)
                    return true;
            }

            return false;
        }


        bool CanAttackWeapon(IWeapon weapon, Enemy enemy)
        {
            AttackMode[] modes = weapon.GetAttackMode();
            for (int i = 0; i < modes.Length; i++)
            {
                if (enemy.Distance > modes[i].minRange && enemy.Distance < modes[i].maxRange)
                    return true;
            }

            return false;
        }

        bool IsRangedWeapon(IWeapon weapon)
        {
            AttackMode[] modes = weapon.GetAttackMode();
            for (int i = 0; i < modes.Length; i++)
            {
                if (modes[i].type == AttackType.Ranged)
                    return true;
            }

            return false;
        }

        float GetWeaponScale(IWeapon weapon, Enemy enemy)
        {
            float minDis = Mathf.Infinity;
            AttackMode[] modes = weapon.GetAttackMode();
            for (int i = 0; i < modes.Length; i++)
            {
                minDis = Mathf.Min(Mathf.Abs(enemy.Distance - modes[i].minRange), Mathf.Abs(enemy.Distance - modes[i].maxRange));
            }
            return minDis;
        }

        IWeapon GetWeapon()
        {
            IWeapon tmpWeapon = null;

            m_Weapons.Clear();
            m_Weapons = entity.target.GetCanUseWeaponList(attackEnemy);

            for (int i = 0; i < m_Weapons.Count; i++)
            {
                if (CanAttackWeapon(m_Weapons[i], attackEnemy))
                {
                    tmpWeapon = m_Weapons[i];

                    if (IsRangedWeapon(m_Weapons[i]))
                        break;
                }
            }

            if (tmpWeapon == null)
            {
                float minDis = Mathf.Infinity;
                for (int j = 0; j < m_Weapons.Count; j++)
                {
                    if (CanAttackWeaponMotar(m_Weapons[j], attackEnemy))
                    {
                        float tmpMinDis = GetWeaponScale(m_Weapons[j], attackEnemy);
                        if (tmpMinDis < minDis)
                        {
                            minDis = tmpMinDis;
                            tmpWeapon = m_Weapons[j];
                        }
                    }
                }
            }

            return tmpWeapon;
        }

        bool CanHoldWeapon(IWeapon weapon)
        {
            //找到的是迫击炮时：在靠近一定范围之后才放下迫击炮开始攻击
            PEEquipment equip = weapon as PEEquipment;
            if (equip.m_ItemObj != null && equip.m_ItemObj.protoId == 1143)
            {
                m_MortarWaitFor = true;
                return attackEnemy.Distance > Mortar_MinRange && attackEnemy.Distance < Mortar_Offset && attackEnemy.CanHoldWeapon();
            }
            m_MortarWaitFor = false;
            return attackEnemy.CanHoldWeapon();
        }

        #endregion

        BehaveResult Tick(Tree sender)
        {
            if (m_Attacked)
            {
                //攻击是否结束
                if (Weapon == null || Weapon.Equals(null) || Weapon.AttackEnd(m_Index))
                {
                    m_Attacked = false;
                    return BehaveResult.Success;
                }
                else
                {
                    bool isAttack = false;

                    if (attackEnemy != null)
                    {
                        float minRange = m_Mode.minRange;
                        float maxRange = m_Mode.maxRange;
                        float sqrDistanceXZ = attackEnemy.SqrDistanceLogic;
                        Vector3 direction = attackEnemy.Direction;

                        //是否被挡住
                        bool isBlock = !m_Mode.ignoreTerrain && PEUtil.IsBlocked(entity, attackEnemy.entityTarget);
                        //距离是否可以攻击
                        bool isRange = sqrDistanceXZ <= maxRange * maxRange && sqrDistanceXZ >= minRange * minRange;
                        //角度是否可以攻击
                        bool isAngle = PEUtil.IsScopeAngle(direction, transform.forward, Vector3.up, m_Mode.minAngle, m_Mode.maxAngle);
                        //是否可以攻击
                        isAttack = isRange && isAngle && !isBlock;
                    }

                    //是否连击
                    if (isAttack)
                        WeaponAttack(Weapon,attackEnemy,m_Index);
                       // Weapon.Attack(m_Index);

                    return BehaveResult.Running;
                }
            }
            else
            {
                if (attackEnemy == null)
                    return BehaveResult.Failure;

                if (Weapon == null || Weapon.Equals(null) || !CanAttackWeapon(Weapon, attackEnemy))
                {
                    if (!entity.motionEquipment.IsSwitchWeapon())
                    {
                        IWeapon tmpWeapon = GetWeapon();
                        if (tmpWeapon != null && !tmpWeapon.Equals(null))
                        {
                            if (Weapon == null || Weapon.Equals(null))
                            {
                                if (!tmpWeapon.HoldReady && CanHoldWeapon(tmpWeapon))
                                    tmpWeapon.HoldWeapon(true);
                            }
                            else
                            {
                                if (!Weapon.Equals(tmpWeapon))
                                {
                                    m_Index = -1;
                                    entity.motionEquipment.SwitchHoldWeapon(Weapon, tmpWeapon);
                                }
                            }
                        }
                    }
                }

                if (attackEnemy.GroupAttack == EAttackGroup.Threat)
                    return BehaveResult.Failure;

                if (Weapon == null || Weapon.Equals(null) || !Weapon.HoldReady)
                {
                    if (Weapon == null)
                    {
                        if(m_MortarWaitFor)
                        {
                            Vector3 dir = position - attackEnemy.position;
                            dir.y = 0.0f;
                            dir = dir.normalized * Mortar_Offset;
                            MoveToPosition(attackEnemy.position + dir, SpeedState.Run);
                        }
                        else
                        {
                            StopMove();
                        }   
                    }   
                    else
                    {
                        float minRange = Mathf.Infinity;
                        float maxRange = 0.0f;

                        AttackMode[] modes = Weapon.GetAttackMode();
                        for (int i = 0; i < modes.Length; i++)
                        {
                            minRange = Mathf.Min(minRange, modes[i].minRange);
                            maxRange = Mathf.Max(maxRange, modes[i].maxRange);
                        }

                        Vector3 dir = position - attackEnemy.position;
                        dir.y = 0.0f;
                        dir = dir.normalized * (minRange + maxRange) * 0.7f;
                        MoveToPosition(attackEnemy.position + dir, SpeedState.Run);
                    }
                }
                else
                {
                    m_DamageBlock = PEUtil.IsDamageBlock(entity);

                    m_Index = SwitchAttackIndex(Weapon, attackEnemy);

                    if (m_Index >= 0 && m_Index < Weapon.GetAttackMode().Length)
                    {
                        m_Mode = Weapon.GetAttackMode()[m_Index];

                        if (attackEnemy.entityTarget.target != null)
                        {
                            if (m_Mode.type == AttackType.Melee)
                                attackEnemy.entityTarget.target.AddMelee(entity);
                            else
                                attackEnemy.entityTarget.target.RemoveMelee(entity);
                        }

                        float minRange = m_Mode.minRange;
                        float maxRange = m_Mode.maxRange;
                        float sqrDistanceXZ = attackEnemy.SqrDistanceLogic;
                        Vector3 direction = attackEnemy.Direction;

                        //是否被挡住
                        bool isBlock = !m_Mode.ignoreTerrain && PEUtil.IsBlocked(entity, attackEnemy.entityTarget);
                        //距离是否可以攻击
                        bool isRange = sqrDistanceXZ <= maxRange * maxRange && sqrDistanceXZ >= minRange * minRange;
                        //角度是否可以攻击
                        bool isAngle = PEUtil.IsScopeAngle(direction, transform.forward, Vector3.up, m_Mode.minAngle, m_Mode.maxAngle);
                        //是否可以攻击
                        bool isAttack = isRange && isAngle && !isBlock;
                        //是否后退
                        bool isRetreat = false;
                        //是否瞄准
                        IAimWeapon aimWeapon = Weapon as IAimWeapon;
                        bool isAimed = m_Mode.type == AttackType.Melee || aimWeapon == null || aimWeapon.Aimed;

                        if (aimWeapon != null)
                        {
                            if (m_Mode.type == AttackType.Ranged)
                            {
                                aimWeapon.SetAimState(true);
                                aimWeapon.SetTarget(attackEnemy.CenterBone);
                            }
                            else
                            {
                                aimWeapon.SetAimState(false);
                                aimWeapon.SetTarget(null);
                            }
                        }

                        if (m_DamageBlock)
                        {
                            m_Attacked = true;
                            WeaponAttack(Weapon,attackEnemy, m_Index, Block45Man.self);
                           // Weapon.Attack(m_Index, Block45Man.self);
                        }
                        else
                        {
                            //寻找可攻击位置
                            m_Local = GetLocalPos(attackEnemy, m_Mode);
                            m_Local.y = 0.0f;
                            //开始后退的距离
                            float retreatRange = minRange;
                            if (m_Mode.type == AttackType.Ranged)
                                retreatRange += Mathf.Lerp(minRange, maxRange, 0.2f);

                            //攻击移动
                            if (sqrDistanceXZ > maxRange * maxRange || isBlock)
                            {
                                StandFromSquat();

                                if (!isBlock)
                                    MortarTakeOff();

                                MoveToPosition(GetMovePos(attackEnemy), SpeedState.Run);
                            }
                            else if (sqrDistanceXZ < retreatRange * retreatRange)
                            {
                                StandFromSquat();

                                if (Time.time - m_LastRetreatTime > 3.0f)
                                {
                                    m_LastRetreatTime = Time.time;
                                    m_RetreatLocal = GetRetreatPos(attackEnemy.position, transform);
                                }

                                if (!GetBool("Mortar") && m_RetreatLocal != Vector3.zero && PEUtil.SqrMagnitude(m_RetreatLocal, position) > 0.5f * 0.5f)
                                {
                                    Vector3 moveDir = m_RetreatLocal - position;
                                    Vector3 rayDir = Vector3.ProjectOnPlane(moveDir, Vector3.up);
                                    if (!Physics.Raycast(entity.centerPos, rayDir, 2.0f, PEConfig.BlockLayer))
                                    {
                                        isRetreat = true;
                                        FaceDirection(direction);
                                        MoveDirection(moveDir, SpeedState.Retreat);
                                    }
                                    else
                                    {
                                        FaceDirection(direction);
                                        MoveDirection(Vector3.zero);
                                    }
                                }
                                else
                                    StopMove();
                            }
                            else
                                StopMove();

                            //攻击旋转
                            if (isRange && !isBlock)
                            {
                                if (isAngle && !isRetreat)
                                    FaceDirection(Vector3.zero);
                                else
                                {
                                    StandFromSquat();
                                    FaceDirection(direction);
                                }
                            }

                            //是否需要瞄准
                            if (isAttack && isAimed && !Weapon.IsInCD(m_Index))
                            {
                                WeaponAttack(Weapon,attackEnemy,m_Index);
                                //Weapon.Attack(m_Index);
                                m_Attacked = true;
                            }
                        }
                    }
                }

                return BehaveResult.Running;
            }
        }

		void Reset(Tree sender)
		{
			if(IsMotionRunning(PEActionType.HoldShield))
				EndAction(PEActionType.HoldShield);

			if(Enemy.IsNullOrInvalid(attackEnemy))
			{
                //if (Weapon != null)
                //    Weapon.HoldWeapon(false);

                m_Attacked = false;
            }
		}
    }

    [BehaveAction(typeof(BTSwitchWeapon), "SwitchWeapon")]
    public class BTSwitchWeapon : BTNormal
    {
        List<IWeapon> m_Weapons = new List<IWeapon>();

        bool CanAttackWeapon(IWeapon weapon, Enemy enemy)
        {
            AttackMode[] modes = weapon.GetAttackMode();
            for (int i = 0; i < modes.Length; i++)
            {
                if (enemy.DistanceXZ > modes[i].minRange && enemy.DistanceXZ < modes[i].maxRange)
                    return true;
            }

            return false;
        }

        bool IsRangedWeapon(IWeapon weapon)
        {
            AttackMode[] modes = weapon.GetAttackMode();
            for (int i = 0; i < modes.Length; i++)
            {
                if (modes[i].type == AttackType.Ranged)
                    return true;
            }

            return false;
        }

        float GetWeaponScale(IWeapon weapon, Enemy enemy)
        {
            float minDis = Mathf.Infinity;
            AttackMode[] modes = weapon.GetAttackMode();
            for (int i = 0; i < modes.Length; i++)
            {
                minDis = Mathf.Min(Mathf.Abs(enemy.DistanceXZ - modes[i].minRange), Mathf.Abs(enemy.DistanceXZ - modes[i].maxRange));
            }
            return minDis;
        }

        IWeapon GetWeapon()
        {
            IWeapon tmpWeapon = null;

            m_Weapons.Clear();
            m_Weapons = entity.target.GetCanUseWeaponList(attackEnemy);

            for (int i = 0; i < m_Weapons.Count; i++)
            {
                if(CanAttackWeapon(m_Weapons[i], attackEnemy))
                {
                    tmpWeapon = m_Weapons[i];

                    if (IsRangedWeapon(m_Weapons[i]))
                        break;
                }
            }

            if(tmpWeapon == null)
            {
                float minDis = Mathf.Infinity;
                for (int j = 0; j < m_Weapons.Count; j++)
                {
                    float tmpMinDis = GetWeaponScale(m_Weapons[j], attackEnemy);
                    if(tmpMinDis < minDis)
                    {
                        minDis = tmpMinDis;
                        tmpWeapon = m_Weapons[j];
                    }
                }
            }

            return tmpWeapon;
        }

        BehaveResult Tick(Tree sender)
        {
            IWeapon tmpWeapon = GetWeapon();
            if(tmpWeapon != null && !tmpWeapon.Equals(null))
            {
                if(Weapon == null || Weapon.Equals(null))
                {
                    if (!tmpWeapon.HoldReady)
                        tmpWeapon.HoldWeapon(true);
                }
                else
                {
                    if (!Weapon.Equals(tmpWeapon))
                        entity.motionEquipment.SwitchHoldWeapon(Weapon, tmpWeapon);
                }
            }

            if (Weapon == null || Weapon.Equals(null))
                return BehaveResult.Failure;
            else if (!Weapon.HoldReady)
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTDamageBlock), "BTDamageBlock")]
    public class BTDamageBlock : BTNormal
    {
        //static int SkillID = 0;
        static float Radius = 5f;

        BehaveResult Tick(Tree sender)
        {
            if(entity.maxRadius > Radius)
            {

            }

            return BehaveResult.Running;
        }
    }
}