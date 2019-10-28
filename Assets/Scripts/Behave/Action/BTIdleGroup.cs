using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;
using Pathea;

namespace Behave.Runtime.Action
{
    public class BTNormalGroup : BTAction
    {
        protected bool GetData<T>(Tree sender, ref T t)
        {
            if (m_TreeDataList.ContainsKey(sender.ActiveStringParameter))
            {
                try
                {
                    t = (T)m_TreeDataList[sender.ActiveStringParameter];
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(ex);
                    return false;
                }
            }
            else
            {
                //Debug.LogError("Do not find data : " + sender.ActiveStringParameter);
                return false;
            }
        }
    }

    [BehaveAction(typeof(BTIdleGroup), "IdleGroup")]
    public class BTIdleGroup : BTNormalGroup
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;

            public float m_StartIdleTime;
            public float m_CurrentIdleTime;
        }

        Data m_Data;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_Data.m_StartIdleTime = Time.time;
            m_Data.m_CurrentIdleTime = Random.Range(m_Data.minTime, m_Data.maxTime);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (group.HasAttackEnemy() || group.HasEscapeEnemy())
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartIdleTime > m_Data.m_CurrentIdleTime)
                return BehaveResult.Success;

            group.PauseMemberBehave(false);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTPatrolGroup), "PatrolGroup")]
    public class BTPatrolGroup : BTNormalGroup
    {
        class Data
        {
            [BehaveAttribute]
            public int field = 0;
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
            public float m_SwitchTime = 0.0f;
            public float m_LastSwitchTime = 0.0f;
            public float m_StartPatrolTime = 0.0f;
            public Vector3 m_CurrentPatrolPosition = Vector3.zero;
        }

        Data m_Data;

        Enemy m_Escape;
        Enemy m_Threat;

        Vector3 GetPatrolPosition(Tree sender)
        {
            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return Vector3.zero;

            PeTrans tr = group.Leader.GetComponent<PeTrans>();
            Vector3 pos = tr.position;
            if (m_Data.field == (int)MovementField.water)
                return PEUtil.GetRandomPositionInWater(pos, tr.trans.forward, m_Data.minRadius, m_Data.maxRadius, m_Data.minHeight, m_Data.maxHeight, -135.0f, 135.0f);
            else if (m_Data.field == (int)MovementField.Sky)
            {
                if(group.Leader.IsFly)
                    return PEUtil.GetRandomPositionInSky(pos, tr.trans.forward, m_Data.minRadius, m_Data.maxRadius, m_Data.minHeight, m_Data.maxHeight, -135.0f, 135.0f);
                else
                    return PEUtil.GetRandomPositionOnGround(pos, tr.trans.forward, m_Data.minRadius, m_Data.maxRadius, -135.0f, 135.0f);
            }
            else
                return PEUtil.GetRandomPositionOnGround(pos, tr.trans.forward, m_Data.minRadius, m_Data.maxRadius, -135.0f, 135.0f);
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_Data.m_StartPatrolTime = Time.time;
            m_Data.m_LastSwitchTime = 0.0f;
            m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            m_Data.m_SwitchTime = Random.Range(5.0f, 10.0f);

            group.PauseMemberBehave(true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (group.HasAttackEnemy() || group.HasEscapeEnemy())
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartPatrolTime > m_Data.m_Time)
                return BehaveResult.Success;

            if (Time.time - m_Data.m_LastSwitchTime > m_Data.m_SwitchTime)
            {
                m_Data.m_LastSwitchTime = Time.time;
                m_Data.m_SwitchTime = Random.Range(5.0f, 10.0f);

                m_Data.m_CurrentPatrolPosition = GetPatrolPosition(sender);
                if (m_Data.m_CurrentPatrolPosition != Vector3.zero)
                    group.MoveToPosition(m_Data.m_CurrentPatrolPosition, SpeedState.Walk);
            }

            //if (m_Data.m_CurrentPatrolPosition != Vector3.zero)
            //{
            //    //group.Patrol(m_Data.m_CurrentPatrolPosition);
            //    foreach (PeEntity skEntity in group.Entities)
            //    {
            //        if (skEntity != null && !skEntity.IsDeath())
            //        {
            //            Motion_Move mover = skEntity.GetComponent<Motion_Move>();
            //            if (mover != null && group.Leader != null)
            //            {
            //                PeTrans tr1 = mover.GetComponent<PeTrans>();
            //                PeTrans tr2 = group.Leader.GetComponent<PeTrans>();
            //                if (tr1 != null && tr2 != null)
            //                {
            //                    if(skEntity == group.Leader)
            //                        mover.MoveTo(m_Data.m_CurrentPatrolPosition);
            //                    else
            //                    {
            //                        if(PEUtil.SqrMagnitudeH(tr1.position, tr2.position) > tr1.radius * tr1.radius * 16)
            //                        {
            //                            mover.MoveTo(m_Data.m_CurrentPatrolPosition + (tr1.position-tr2.position).normalized * tr1.radius * Random.Range(3.0f, 5.0f));
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTFlyAndLandGroup), "FlyAndLandGroup")]
    public class BTFlyAndLandGroup : BTNormalGroup
    {
        class Data
        {
            [BehaveAttribute]
            public bool fly = false;
        }

        Data m_Data;
        float m_StartTime;


        bool IsFlyEquals(BehaveGroup group)
        {
            foreach (PeEntity skEntity in group.Entities)
            {
                if (skEntity != null && !skEntity.IsDeath())
                {
                    MonsterCmpt cmpt = skEntity.GetComponent<MonsterCmpt>();
                    if (cmpt != null && cmpt.IsFly != m_Data.fly)
                        return true;
                }
            }

            return false;
        }

        BehaveResult Init(Tree sender)
        {
			if (!GetData<Data>(sender, ref m_Data))
				return BehaveResult.Failure;

            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (!IsFlyEquals(group))
                return BehaveResult.Failure;

            m_StartTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            BehaveGroup group = sender.ActiveAgent as BehaveGroup;
            if (group == null || group.Leader == null)
                return BehaveResult.Failure;

            if (m_Data.fly)
            {
                if (Time.time - m_StartTime < 0.5f)
                    return BehaveResult.Running;

                group.ActivateGravity(false);
                group.Fly(true);

                return BehaveResult.Success;
            }
            else
            {
                group.ActivateGravity(true);

                bool running = false;

                for (int i = 0; i < group.Entities.Count; i++)
                {
                    PeEntity e = group.Entities[i];
                    if (e != null)
                    {
                        MonsterCmpt monster = e.GetComponent<MonsterCmpt>();
                        Motion_Move mover = e.GetComponent<Motion_Move>();
                        BehaveCmpt behave = e.GetComponent<BehaveCmpt>();

                        if(monster != null && mover != null && behave != null)
                        {
                            if(mover.grounded)
                            {
                                monster.Fly(false);
                                behave.Pause(false);

                                running = true;
                            }
                        }
                    } 
                }

                if (running)
                    return BehaveResult.Running;
                else
                    return BehaveResult.Success;
            }
        }
    }
}
