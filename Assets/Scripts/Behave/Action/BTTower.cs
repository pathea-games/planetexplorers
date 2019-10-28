using UnityEngine;
using System.Collections;
using PETools;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Pathea;

namespace Behave.Runtime.Action
{
    [BehaveAction(typeof(BTTowerDefence), "TowerDefence")]
    public class BTTowerDefence : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (TDObj == null && TDpos == Vector3.zero)
                return BehaveResult.Failure;

            if ((entity.ProtoID == 90 || entity.ProtoID == 91) && !GetBool("Fly"))
                return BehaveResult.Failure;

            if ((entity.ProtoID == 93 || entity.ProtoID == 94) && GetBool("Crouch"))
                return BehaveResult.Failure;

            if (hasAttackEnemy)
                return BehaveResult.Success;

            if(TDpos != Vector3.zero)
                MoveToPosition(TDpos, SpeedState.Run);
            else if(TDObj != null)
                MoveToPosition(TDObj.transform.position, SpeedState.Run);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTTowerAttack), "TowerAttack")]
    public class BTTowerAttack : BTAttackBase
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
            public bool isPitch = false;
            [BehaveAttribute]
            public int skillID = 0;

            float m_LastCDTime = 0.0f;
            public bool m_CanAttack;

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
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!TowerIsEnable() || !TowerHaveCost() || TowerSkillRunning())
                return BehaveResult.Failure;

            if (!m_Data.Ready())
                return BehaveResult.Failure;

			float attackDis = attackEnemy.AttackDistance;
			float minRange = m_Data.minRange;
			float maxRange = Mathf.Max(m_Data.maxRange, attackDis * 1.5f);

			if (attackEnemy.Distance < minRange  || attackEnemy.Distance > maxRange)
                return BehaveResult.Failure;

            Vector3 direction = attackEnemy.position - position;
            if (!PEUtil.IsScopeAngle(direction, transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
                return BehaveResult.Failure;

            m_Data.m_CanAttack = true;

            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!TowerIsEnable())
                return BehaveResult.Failure;

            if (!hasAttackEnemy)
                return BehaveResult.Failure;

            Transform aim = attackEnemy.CenterBone;
            if (aim == null)
                return BehaveResult.Failure;

            SetTowerAimPosition(aim);

            if (aim != null && !TowerAngle(aim.position, 5.0f))
                return BehaveResult.Running;

            if (m_Data.isPitch && !TowerPitchAngle(aim.position, 5.0f))
                return BehaveResult.Running;

            if (aim != null && !TowerCanAttack(aim.position, attackEnemy.trans))
                return BehaveResult.Running;

            if (m_Data.m_CanAttack)
            {
                m_Data.SetCDTime(Time.time);
                TowerFire(attackEnemy.skTarget);
                m_Data.m_CanAttack = false;
                return BehaveResult.Running;
            }

            if (TowerSkillRunning())
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
    }
}
