using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Pathea;

namespace Behave.Runtime.Action
{
    [BehaveAction(typeof(BTSwitchAttackGroup), "SwitchAttackGroup")]
    public class BTSwitchAttackGroup : BTNormalGroup
    {
        float m_LastCheckTime;

        BehaveResult Tick(Tree sender)
        {
            return BehaveResult.Failure;

            //BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            //if (group == null || group.Leader == null)
            //    return BehaveResult.Failure;

            //for (int i = 0; i < group.Entities.Count; i++)
            //{
            //    if (group.Entities[i] == null || group.Entities[i].target == null)
            //        continue;

            //    Enemy e1 = group.Entities[i].target.GetAttackEnemy();
            //    if(e1 != null)
            //    {
            //        if (e1.GroupAttack == EAttackGroup.Attack)
            //        {
            //            if (e1.MoveDir == EEnemyMoveDir.Away)
            //            {
            //                for (int j = 0; j < group.Entities.Count; j++)
            //                {
            //                    if (group.Entities[j] == null
            //                        || group.Entities[j] == group.Entities[i]
            //                        || group.Entities[j].target == null)
            //                        continue;

            //                    Enemy e2 = group.Entities[j].target.GetAttackEnemy();
            //                    if (e2 != null && e2.GroupAttack == EAttackGroup.Threat)
            //                    {
            //                        if (Mathf.Sqrt(e2.SqrDistanceLogic) < Mathf.Sqrt(e1.SqrDistanceLogic) * 0.6f)
            //                        {
            //                            e1.GroupAttack = EAttackGroup.Threat;
            //                            e2.GroupAttack = EAttackGroup.Attack;
            //                        }
            //                    }
            //                }
            //            }

            //            if (e1.entity.HPPercent < 0.4f)
            //            {
            //                for (int j = 0; j < group.Entities.Count; j++)
            //                {
            //                    if (group.Entities[j] == null
            //                        || group.Entities[j] == group.Entities[i]
            //                        || group.Entities[j].target == null)
            //                        continue;

            //                    Enemy e2 = group.Entities[j].target.GetAttackEnemy();
            //                    if (e2 != null && e2.GroupAttack == EAttackGroup.Threat)
            //                    {
            //                        if (e2.entity.HPPercent > 0.75f)
            //                        {
            //                            e1.GroupAttack = EAttackGroup.Threat;
            //                            e2.GroupAttack = EAttackGroup.Attack;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //        else if(e1.GroupAttack == EAttackGroup.Threat)
            //        {
            //            if(Time.time - m_LastCheckTime > 5.0f)
            //            {
            //                if(Random.value < 0.3f)
            //                {
            //                    for (int j = 0; j < group.Entities.Count; j++)
            //                    {
            //                        if (group.Entities[j] == null
            //                            || group.Entities[j] == group.Entities[i]
            //                            || group.Entities[j].target == null)
            //                            continue;

            //                        Enemy e2 = group.Entities[j].target.GetAttackEnemy();
            //                        if (e2 != null && e2.GroupAttack == EAttackGroup.Attack)
            //                        {
            //                            e1.GroupAttack = EAttackGroup.Attack;
            //                            e2.GroupAttack = EAttackGroup.Threat;
            //                        }
            //                    }
            //                }

            //                m_LastCheckTime = Time.time;
            //            }
            //        }
            //    }
            //}

            //for (int i = 0; i < group.Entities.Count; i++)
            //{
            //    if (group.Entities[i] == null || group.Entities[i].target == null)
            //        continue;

            //    Enemy e = group.Entities[i].target.GetAttackEnemy();
            //    if (e != null && e.GroupAttack == EAttackGroup.None)
            //        e.GroupAttack = EAttackGroup.Threat;
            //}

            //int attackCount = 0;
            //for (int i = 0; i < group.Entities.Count; i++)
            //{
            //    if (group.Entities[i] == null || group.Entities[i].target == null)
            //        continue;

            //    Enemy e = group.Entities[i].target.GetAttackEnemy();
            //    if (e != null && e.GroupAttack == EAttackGroup.Attack)
            //        attackCount++;
            //}

            //for (int i = 0; i < group.Entities.Count; i++)
            //{
            //    if (group.Entities[i] == null || group.Entities[i].target == null)
            //        continue;

            //    if (attackCount > group.atkMax)
            //    {
            //        Enemy e = group.Entities[i].target.GetAttackEnemy();
            //        if (e != null && e.GroupAttack == EAttackGroup.Attack)
            //        {
            //            attackCount--;
            //            e.GroupAttack = EAttackGroup.Threat;
            //        }
            //    }
            //}

            //for (int i = 0; i < group.Entities.Count; i++)
            //{
            //    if (group.Entities[i] == null || group.Entities[i].target == null)
            //        continue;

            //    if (attackCount < group.atkMin)
            //    {
            //        Enemy e = group.Entities[i].target.GetAttackEnemy();
            //        if (e != null && e.GroupAttack == EAttackGroup.Threat)
            //        {
            //            attackCount++;
            //            e.GroupAttack = EAttackGroup.Attack;
            //        }
            //    }
            //}

            //for (int i = 0; i < group.Entities.Count; i++)
            //{
            //    if (group.Entities[i] == null || group.Entities[i].target == null)
            //        continue;

            //    Enemy e = group.Entities[i].target.GetAttackEnemy();
            //    if (e == null || e.GroupAttack == EAttackGroup.Attack)
            //        continue;

            //    List<IAttack> attacks = group.Entities[i].target.Attacks;
            //    for (int j = 0; j < attacks.Count; j++)
            //    {
            //        IAttackRanged attack = attacks[j] as IAttackRanged;
            //        if(attack != null && attack.IsInRange(e))
            //        {
            //            e.GroupAttack = EAttackGroup.Attack;
            //            break;
            //        }
            //    }

            //    if(e.GroupAttack != EAttackGroup.Attack && group.Entities[i].motionEquipment != null)
            //    {
            //        IWeapon weapon = group.Entities[i].motionEquipment.Weapon;
            //        if(weapon != null)
            //        {
            //            AttackMode[] modes = weapon.GetAttackMode();
            //            for (int j = 0; j < modes.Length; j++)
            //            {
            //                if(modes[j].type == AttackType.Ranged)
            //                {
            //                    e.GroupAttack = EAttackGroup.Attack;
            //                    break;
            //                }
            //            }
            //        }
            //    }

            //    if(e.entityTarget.Group != null)
            //        e.GroupAttack = EAttackGroup.Attack;
            //}

            //return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTIsMemberSurround), "IsMemberSurround")]
    public class BTIsMemberSurround : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (attackEnemy == null || attackEnemy.GroupAttack == EAttackGroup.Attack)
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTHasEnemyGroup), "HasEnemyGroup")]
    public class BTHasEnemyGroup : BTNormalGroup
    {
        BehaveResult Tick(Tree sender)
        {
            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (group.HasAttackEnemy())
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTHasEscapeGroup), "HasEscapeGroup")]
    public class BTHasEscapeGroup : BTNormalGroup
    {
        BehaveResult Tick(Tree sender)
        {
            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (group.HasEscapeEnemy())
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTAttackGroup), "AttackGroup")]
    public class BTAttackGroup : BTNormalGroup
    {
        BehaveResult Tick(Tree sender)
        {
            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (group.HasAttackEnemy())
            {
                group.PauseMemberBehave(false);
                return BehaveResult.Running;
            }
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTEscapeGroup), "EscapeGroup")]
    public class BTEscapeGroup : BTNormalGroup
    {
        //float m_StartTime;
        float m_EscapeTime;
        float m_LastRandomTime;

        Vector3 m_CurEscapeposition;

        Vector3 GetEscapePosition(BehaveGroup group, Vector3 center, Vector3 direction, float minRadius, float maxRadius)
        {
            if (group.Leader.Field == MovementField.Sky)
            {
                if (group.Leader.IsFly)
                    return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, 25.0f, 50.0f, -135.0f, 135.0f);
                else
                    return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, -135.0f, 135.0f);
            }
            else if (group.Leader.Field == MovementField.water)
                return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 5.0f, 25.0f, -135.0f, 135.0f);
            else
                return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, -135.0f, 135.0f);
        }

        BehaveResult Init(Tree sender)
        {
            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (!group.HasEscapeEnemy())
                return BehaveResult.Failure;

            //m_StartTime = Time.time;
            m_LastRandomTime = 0.0f;

            group.PauseMemberBehave(true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (group.EscapeEnemy == null)
                return BehaveResult.Failure;

            if(m_CurEscapeposition == Vector3.zero
                || PEUtil.SqrMagnitudeH(group.Leader.position, m_CurEscapeposition) < 1.0f * 1.0f
                || Time.time - m_LastRandomTime > 10.0f )
            {
                m_LastRandomTime = Time.time;

                PeTrans tr = group.Leader.GetComponent<PeTrans>();
                m_CurEscapeposition = GetEscapePosition(group, tr.position, tr.position - group.EscapeEnemy.position, 25.0f, 35.0f);
                if (m_CurEscapeposition == Vector3.zero)
                    return BehaveResult.Failure;
                else
                    group.MoveToPosition(m_CurEscapeposition, SpeedState.Run);
            }

           return BehaveResult.Running;
        }
    }
}