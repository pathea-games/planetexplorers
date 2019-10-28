using UnityEngine;
using ItemAsset;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;
using PETools;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;

namespace Behave.Runtime
{

    [BehaveAction(typeof(BTNpcThreat), "NpcThreat")]
    public class BTNpcThreat : BTNormal
    {

        BehaveResult Tick(Tree sender)
        {

            if (Enemy.IsNullOrInvalid(selectattackEnemy) || selectattackEnemy.GroupAttack == EAttackGroup.Attack)
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTNpcAttack), "NpcAttack")]
    public class BTNpcAttack : BTNormal
    {

        BehaveResult Tick(Tree sender)
        {

            if (Enemy.IsNullOrInvalid(selectattackEnemy) || selectattackEnemy.GroupAttack == EAttackGroup.Threat)
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }
    }


    [BehaveAction(typeof(BTConfirmAttackMode), "ConfirmAttackMode")]
    public class BTConfirmAttackMode : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (entity == null || entity.target == null || Enemy.IsNullOrInvalid(selectattackEnemy))
                return BehaveResult.Failure;

            if (entity.motionEquipment == null)
                return BehaveResult.Failure;

            if(!ItemAsset.SelectItem.MatchEnemyAttack(entity, selectattackEnemy.entityTarget))
            {
                selectattackEnemy.entityTarget.target.RemoveMelee(entity);
                return BehaveResult.Success;
            }
                

            ItemObject itemObj = entity.motionEquipment.ActiveableEquipment != null ? entity.motionEquipment.ActiveableEquipment.m_ItemObj : null;
            if (itemObj == null)
            {
                //is Golves
                int n = selectattackEnemy.entityTarget.monsterProtoDb != null && selectattackEnemy.entityTarget.monsterProtoDb.AtkDb != null ? selectattackEnemy.entityTarget.monsterProtoDb.AtkDb.mNumber : 3;
                selectattackEnemy.entityTarget.target.AddMelee(entity,n);
                return BehaveResult.Success;
            }

            //收回不能用来攻击的武器
            if (itemObj.protoData != null && itemObj.protoData.weaponInfo == null)
                SelectItem.TakeOffEquip(entity);

            if (selectattackEnemy.entityTarget.target == null)//建筑
                return BehaveResult.Success;

            AttackMode[] modes = itemObj.protoData != null && itemObj.protoData.weaponInfo != null ? itemObj.protoData.weaponInfo.attackModes : null;
            if(modes == null || modes.Length == 0)
                return BehaveResult.Failure;

            int num = selectattackEnemy.entityTarget.monsterProtoDb != null && selectattackEnemy.entityTarget.monsterProtoDb.AtkDb != null ? selectattackEnemy.entityTarget.monsterProtoDb.AtkDb.mNumber : 3;
            AttackType type = modes[0].type;
            if (type == AttackType.Melee)
                selectattackEnemy.entityTarget.target.AddMelee(entity,num);
            else
                selectattackEnemy.entityTarget.target.RemoveMelee(entity);

            return BehaveResult.Success;
        }
    }



    [BehaveAction(typeof(BTThreatRunAway), "ThreatRunAway")]
    public class BTThreatRunAway : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float RunRadius;
            [BehaveAttribute]
            public float minHpPercent;

            public float minRadius = 32.0f;
        }
        Data m_Data;

        Vector3 runPos;
        //float startRunTime = 0.0f;
        //float CHECK_TIME = 10.0f;

        //float startHideTime = 0.0f;
        //float CHECK_Hide_TIME = 1.0f;

        void OnPathComplete(Pathfinding.Path path)
        {
            if (path != null && path.vectorPath.Count > 0)
            {
                Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                runPos = pos;
            }
        }

        Vector3 GetPatrolPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
        {
            if (AstarPath.active != null)//PEUtil.IsInAstarGrid(position))
            {
                Pathfinding.RandomPath path = Pathfinding.RandomPath.Construct(position, (int)Random.Range(minRadius, maxRadius) * 100, OnPathComplete);
                path.spread = 40000;
                path.aimStrength = 1f;
                path.aim = PEUtil.GetRandomPosition(position, direction, minRadius, maxRadius, -75.0f, 75.0f);
                AstarPath.StartPath(path);

                return Vector3.zero;
            }
            return Vector3.zero;
        }

        Vector3 GetRunDir(PeEntity npc, float radius = 32.0f)
        {
            Vector3 dir = Vector3.zero;
            for (int i = 0; i < Enemies.Count; i++)
            {
                float d = PETools.PEUtil.Magnitude(npc.position, Enemies[i].position);
                if (d > radius)
                    continue;

                dir += (npc.position - Enemies[i].position).normalized * (1.0f - d / radius);
            }

            return dir;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Enemy.IsNullOrInvalid(selectattackEnemy))
                return BehaveResult.Failure;

            //startRunTime = Time.time;
            //startHideTime = Time.time;

            Vector3 v = GetRunDir(entity);
            if (v == Vector3.zero)
                return BehaveResult.Failure;
            GetPatrolPosition(position, v, 16.0f, 32.0f);

            if (selectattackEnemy.entityTarget.Field == MovementField.Sky)
                m_Data.minRadius = 32.0f;
            else if (selectattackEnemy.entityTarget.IsBoss)
                m_Data.minRadius = 128.0f;
            else
                m_Data.minRadius = 16.0f;

            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (Enemy.IsNullOrInvalid(selectattackEnemy) || selectattackEnemy.GroupAttack != EAttackGroup.Threat)
                return BehaveResult.Success;

            bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
            if(_IsUnderBlock)
            {
                StopMove();
            }
            else
            {
                if (!IsReached(position, selectattackEnemy.position, false, m_Data.minRadius))
                {
                    FaceDirection(selectattackEnemy.position - position);
                    StopMove();
                }
                else
                {
                    if (IsReached(runPos, position, false) || Stucking())
                    {
                        Vector3 v = GetRunDir(entity);
                        GetPatrolPosition(position, v, 16.0f, 32.0f);
                    }

                    MoveToPosition(runPos, SpeedState.Run);
                }
            }
           
            return BehaveResult.Running;
        }
    }
}
