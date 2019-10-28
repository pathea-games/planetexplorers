using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.Operate;
using PETools;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;

namespace Behave.Runtime
{
    [BehaveAction(typeof(BTNpcIsBase), "NpcIsBase")]
    public class BTNpcIsBase : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (IsNpcBase)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    /**************************************
    * 基地休闲行为
    * 内容：在基地NPC无其他安排时，进行的闲逛行为
    * *************************************/
    [BehaveAction(typeof(BTNpcBaseWander), "NpcBaseWander")]
    public class BTNpcBaseWander : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float wanderTime;


            //建筑内wander

            //无寻路数据时
            public float sWalkTime0 = 2.0f;
            public float sWalkTime1 = 20.0f;
            public float sWalkTime2 = 30.0f;
            public float LastWalkTime = 0.0f;
            public float LastStopTime = 0.0f;

            int TIMES = 4;
            bool m_CalculatedDir;
            Vector3 m_AnchorDir;
            int underBlockTimes = 0;
            public bool EndUnderBlock()
            {
                if (underBlockTimes > TIMES)
                {
                    underBlockTimes = 0;
                    return true;
                }
                return false;
            }

            public Vector3 GetAnchorDir()
            {
                return m_AnchorDir;
            }

            public bool hasCalculatedDir { get { return m_CalculatedDir; } }
            public void ResetCalculatedDir()
            {
                m_CalculatedDir = false;
                underBlockTimes++;
            }
            public bool GetCanMoveDirtion(PeEntity entity, float minAngle)
            {
                if (!m_CalculatedDir)
                {
                    for (int i = 1; i < (int)360.0f / minAngle; i++)
                    {
                        m_AnchorDir = Quaternion.AngleAxis(minAngle * i, Vector3.up) * entity.peTrans.forward;
                        Debug.DrawRay(entity.position + Vector3.up, m_AnchorDir * 3.0f, Color.cyan);
                        if (!PETools.PEUtil.IsForwardBlock(entity, m_AnchorDir, 3.0f))
                        {
                            m_CalculatedDir = true;
                            return true;
                        }
                        m_AnchorDir = Vector3.zero;
                    }
                    return false;
                }
                return false;
            }
        }
        Data m_Data;

        Vector3 m_WanderCenter;
        Vector3 m_CurWanderPos;
        float m_WanderRadius;
        //float m_starTime;
        float mWanderStartTime;
        //float mWanderTime = 40.0f;

        //bool m_HasCallBack = false;
        //float m_BeStuckTimes = 0.0f;

        EThinkingType mStroll = EThinkingType.Stroll;
        Vector3 GetRandomPositionForWander()
        {
            return PEUtil.GetRandomPositionOnGroundForWander(m_WanderCenter, m_WanderRadius - 5.0f, m_WanderRadius);
        }

        void OnPathComplete(Pathfinding.Path path)
        {
            if (Creater != null && Creater.Assembly != null && PETools.PEUtil.IsInAstarGrid(position) && path != null && path.vectorPath.Count > 15)
            {
                Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                float dis = PEUtil.Magnitude(Creater.Assembly.Position, pos);
                if (dis < Creater.Assembly.Radius)//NpcMgr.IsIncenterAraound(Creater.Assembly.Position, Creater.Assembly.Radius,pos))
                    m_CurWanderPos = pos;
                else
                {
                    if (Creater != null && Creater.Assembly != null)
                        NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_CurWanderPos);
                }
            }
            else
            {
                if (Creater != null && Creater.Assembly != null)
                    NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_CurWanderPos);
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (hasAnyRequest)
                return BehaveResult.Failure;

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Stroll))
            {
                StopMove();
                return BehaveResult.Failure;
            }

            if (!NpcThinkDb.CanDo(entity, mStroll))
                return BehaveResult.Failure;

            if (!CanNpcWander)
                return BehaveResult.Failure;

            if (entity.NpcCmpt.lineType != ELineType.IDLE)
                return BehaveResult.Failure;

            if (Creater.Assembly != null)
            {
                float r = PEUtil.Magnitude(Creater.Assembly.Position, position);
                if (r > Creater.Assembly.Radius)
                    return BehaveResult.Failure;
            }


            if (Creater.Assembly != null)
            {
                m_WanderCenter = Creater.Assembly.Position;
                m_WanderRadius = Creater.Assembly.Radius;
            }

            if (m_WanderCenter == Vector3.zero || m_WanderRadius <= 0.0f)
                return BehaveResult.Failure;

            //            if (!NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_CurWanderPos))
            //                return BehaveResult.Failure;

            if (m_CurWanderPos == Vector3.zero)//NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_CurWanderPos);
                NpcMgr.GetRandomPathForCsWander(entity, Creater.Assembly.Position, transform.forward, 15.0f, Creater.Assembly.Radius, OnPathComplete);


            SetNpcState(ENpcState.Patrol);
            //m_starTime = Time.time;
            mWanderStartTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!IsNpcBase || hasAnyRequest)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Stroll))
            {
                StopMove();
                return BehaveResult.Failure;
            }

            if (!NpcThinkDb.CanDoing(entity, mStroll) || entity.NpcCmpt.lineType != ELineType.IDLE)
            {
                StopMove();
                return BehaveResult.Failure;
            }

            if (Creater.Assembly != null)
            {
                float r = PEUtil.Magnitude(Creater.Assembly.Position, position);
                if (r > Creater.Assembly.Radius)
                    return BehaveResult.Failure;
            }

            if (IsMotionRunning(PEActionType.HoldShield))
                EndAction(PEActionType.HoldShield);

            if (PEUtil.SqrMagnitudeH(position, m_CurWanderPos) < 0.5f * 0.5f)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Success;
            }

            if (m_CurWanderPos == Vector3.zero && Time.time - mWanderStartTime >= m_Data.wanderTime)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Success;
            }

            if (Time.time - m_Data.LastWalkTime > m_Data.sWalkTime1)
            {
                m_Data.LastWalkTime = Time.time;
                m_Data.ResetCalculatedDir();
            }

            bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
            bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, entity.peTrans.forward, 2.0f);
            bool _canPointMove = m_Data.hasCalculatedDir && !_IsForwardBlock && !_IsUnderBlock;


            if (!_canPointMove)
            {
                if (m_Data.EndUnderBlock())
                {
                    m_Data.LastWalkTime = Time.time;
                    m_Data.ResetCalculatedDir();
                    if (NpcCanWalkPos(m_WanderCenter, m_WanderRadius, out m_CurWanderPos))
                        SetPosition(m_CurWanderPos);

                    return BehaveResult.Success;
                }

                bool canMove = m_Data.GetCanMoveDirtion(entity, 30.0f) || Time.time - m_Data.LastWalkTime < m_Data.sWalkTime0;
                if (canMove)
                {
                    if (m_Data.GetAnchorDir() != Vector3.zero)
                        MoveDirection(m_Data.GetAnchorDir(), SpeedState.Walk);
                    else
                        StopMove();
                }
                else
                {
                    StopMove();
                }
            }
            else
            {
                if (Stucking(1.0f))
                {
                    if (entity.viewCmpt != null && entity.viewCmpt.hasView)
                    {
                        //有模型寻路卡住
                        if (NpcCanWalkPos(m_WanderCenter, m_WanderRadius, out m_CurWanderPos))
                        {
                            if (Stucking(10.0f))
                                SetPosition(m_CurWanderPos);
                        }
                        else
                        {
                            StopMove();
                        }

                    }
                    else
                    {
                        //无模型卡住
                        if (Creater.Assembly != null)
                        {
                            float r = PEUtil.Magnitude(Creater.Assembly.Position, position);
                            bool outRadius = r > Creater.Assembly.Radius;
                            Vector3 pos;
                            if (outRadius && NpcCanWalkPos(m_WanderCenter, m_WanderRadius, out m_CurWanderPos))
                            {
                                //NpcMgr.GetRandomPathForCsWander(entity,Creater.Assembly.Position,transform.forward,15.0f, Creater.Assembly.Radius,OnPathComplete);
                                pos = m_CurWanderPos;
                                if (pos != Vector3.zero)
                                {
                                    SetPosition(pos);
                                    m_CurWanderPos = pos;
                                    return BehaveResult.Failure;
                                }
                            }
                            else
                                StopMove();
                        }
                        else
                            StopMove();

                    }

                }


                MoveToPosition(m_CurWanderPos, SpeedState.Walk);
            }
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            m_CurWanderPos = Vector3.zero;
            //m_BeStuckTimes = 0;
        }
    }


    /**************************************
    * 基地睡觉行为
    * 到了时间段，被安排到的基地NPC执行此行为
    * **********       *****************/
    [BehaveAction(typeof(BTNpcBaseSleep), "NpcBaseSleep")]
    public class BTNpcBaseSleep : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float minComfort;
            [BehaveAttribute]
            public float maxComfort;
            [BehaveAttribute]
            public float minSleepTime;
            [BehaveAttribute]
            public float maxSleepTime;
            [BehaveAttribute]
            public string sleepTimeSlots;
            [BehaveAttribute]
            public int buffId;
            [BehaveAttribute]
            public string Anim;

            public List<Pathea.CheckSlot> slots = new List<CheckSlot>();

            public float m_StartSleepTime;
            public float m_CurSleepTime;
            public PESleep m_Sleep;
            bool m_Init = false;

            public void Init(PeEntity npc)
            {
                if (!m_Init)
                {
                    if (npc.NpcCmpt != null)
                        npc.NpcCmpt.npcCheck.ClearSleepSlots();

                    if (sleepTimeSlots != "")
                    {
                        string[] dataStr = PEUtil.ToArrayString(sleepTimeSlots, ',');
                        foreach (string item in dataStr)
                        {
                            float[] data = PEUtil.ToArraySingle(item, '_');
                            if (data.Length == 2)
                            {
                                Pathea.CheckSlot slot = new CheckSlot(data[0], data[1]);

                                slots.Add(slot);
                                if (npc.NpcCmpt != null)
                                    npc.NpcCmpt.npcCheck.AddSleepSlots(slot.minTime, slot.maxTime);
                            }
                        }
                    }
                    m_Init = true;
                }
            }

            public bool IsTimeSlot(float timeSlot)
            {
                return slots.Find(ret => ret.InSlot(timeSlot)) != null;
            }
        }


        Data m_Data;
        bool Iscomfort = false;
        EThinkingType mSleepTh = EThinkingType.Sleep;
        bool StopSleepAction()
        {
            SetNpcState(ENpcState.UnKnown);
            if (Operator != null && !Operator.Equals(null) && Operator.Operate != null && !Operator.Operate.Equals(null) && Operator.Operate.ContainsOperator(Operator))
            {
                return Operator.Operate.StopOperate(Operator, EOperationMask.Sleep);
            }
            return true;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            m_Data.Init(entity);

            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Sleep))
                return BehaveResult.Failure;

            if (!NpcThinkDb.CanDo(entity, mSleepTh))
                return BehaveResult.Failure;

            if (entity.NpcCmpt.lineType != ELineType.TeamSleep)
                return BehaveResult.Failure;

            if (Sleep == null || Sleep.Equals(null)
                || Operator == null || Operator.Equals(null))
                return BehaveResult.Failure;

            if (!Sleep.CanOperateMask(EOperationMask.Sleep))
                return BehaveResult.Failure;

            if (entity.NpcCmpt != null && !(entity.NpcCmpt.IsUncomfortable || entity.NpcCmpt.lineType == ELineType.TeamSleep))//m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay)))
            {
                entity.NpcCmpt.ThinkAgent.RemoveThink(mSleepTh);
                return BehaveResult.Failure;
            }

            PEBed bed = Sleep as PEBed;
            m_Data.m_Sleep = bed.GetStartOperate(EOperationMask.Sleep) as PESleep;
            if (m_Data.m_Sleep == null || m_Data.Equals(null))
                return BehaveResult.Failure;

            Iscomfort = entity.NpcCmpt.IsUncomfortable;
            entity.NpcCmpt.AddTalkInfo(ENpcTalkType.BaseNpc_strike_sleep, ENpcSpeakType.Both, true);
            SetNpcState(ENpcState.Rest);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || Sleep == null || Sleep.Equals(null) || Operator == null || Operator.Equals(null) || m_Data.m_Sleep == null || m_Data.Equals(null))
            {
                if (StopSleepAction())
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Sleep))
            {
                if (StopSleepAction())
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            if (!NpcThinkDb.CanDoing(entity, mSleepTh))
            {
                if (StopSleepAction())
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            SetNpcAiType(ENpcAiType.NpcBaseSleep);
            if (!Sleep.CanOperateMask(EOperationMask.Sleep) && !Sleep.ContainsOperator(Operator))
            {
                if (StopSleepAction())
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            if (!Sleep.ContainsOperator(Operator))
            {
                if (!Sleep.CanOperate(transform))
                {
                    bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
                    bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, entity.peTrans.forward, 2.0f);
                    if (_IsUnderBlock)
                        SetPosition(m_Data.m_Sleep.Trans.position);//MoveDirection(existent.forward,SpeedState.Run);
                    else
                        MoveToPosition(m_Data.m_Sleep.Trans.position, SpeedState.Run, false);

                    if (_IsForwardBlock)
                        SetPosition(m_Data.m_Sleep.Trans.position);

                    if (Stucking())
                        SetPosition(m_Data.m_Sleep.Trans.position);

                    if (IsReached(position, m_Data.m_Sleep.Trans.position, false))
                        SetPosition(m_Data.m_Sleep.Trans.position);
                }
                else
                {
                    MoveToPosition(Vector3.zero);
                    m_Data.m_StartSleepTime = Time.time;
                    m_Data.m_CurSleepTime = Random.Range(m_Data.minSleepTime, m_Data.maxSleepTime);

                    PEBed bed = Sleep as PEBed;
                    m_Data.m_Sleep = bed.GetStartOperate(EOperationMask.Sleep) as PESleep;
                    if (m_Data.m_Sleep != null && !m_Data.Equals(null))
                    {
                        SetPosition(m_Data.m_Sleep.Trans.position);
                        m_Data.m_Sleep.StartOperate(Operator, EOperationMask.Sleep);
                        SetNpcState(ENpcState.Rest);
                    }
                }
            }
            else
            {
                if (Iscomfort)
                {
                    float comfort = GetAttribute(AttribType.Comfort);
                    float comfortMax = GetAttribute(AttribType.ComfortMax);
                    if (comfort >= comfortMax * 0.9f)
                    {
                        m_Data.m_Sleep.StopOperate(Operator, EOperationMask.Sleep);
                        entity.NpcCmpt.ThinkAgent.RemoveThink(mSleepTh);
                        return BehaveResult.Success;
                    }
                }

                if (!Iscomfort && entity.NpcCmpt.lineType != ELineType.TeamSleep)// !m_Data.IsTimeSlot((float)GameTime.Timer.HourInDay))
                {
                    if (StopSleepAction())
                    {
                        entity.NpcCmpt.ThinkAgent.RemoveThink(mSleepTh);
                        return BehaveResult.Success;
                    }
                    else
                        return BehaveResult.Running;
                }

                if (Operator.Operate == null || Operator.Operate.Equals(null) || !Operator.IsActionRunning(PEActionType.Sleep))
                    return BehaveResult.Failure;
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (Creater != null && Creater.Assembly != null && NpcJobStae == ENpcState.Rest)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Sleep);
            }
            SetNpcState(ENpcState.UnKnown);
        }

    }

    /**************************************
    *基地吃饭行为
    * 到了时间点，被安排的基地NPC执行此行为
    * **********       *****************/
    [BehaveAction(typeof(BTNpcBaseTakeFood), "NpcBaseTakeFood")]
    public class BTNpcBaseTakeFood : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string eatAnim;
            [BehaveAttribute]
            public float minHunger;
            [BehaveAttribute]
            public float maxHunger;
            [BehaveAttribute]
            public float minEatTime;
            [BehaveAttribute]
            public float maxEatTime;
            [BehaveAttribute]
            public float full;
            [BehaveAttribute]
            public float interval;
            [BehaveAttribute]
            public string eatTimeSlots;
            [BehaveAttribute]
            public string eatIdsStr;
            [BehaveAttribute]
            public float minBlood;
            [BehaveAttribute]
            public float maxBlood;
            [BehaveAttribute]
            public float minComfort;
            [BehaveAttribute]
            public float maxComfort;

            public List<Pathea.CheckSlot> slots = new List<CheckSlot>();
            public string[] eatAnims;

            public string CurEatAnim;

            public float m_StartEatTime;
            public float m_CurEatTime;
            //public float m_LastEatTime;

            bool m_Init = false;

            CSStorage m_Storage;

            public CSStorage Storage
            {
                get { return m_Storage; }
                set
                {
                    if (m_Storage != value)
                    {
                        m_Storage = value;

                        if (m_Storage != null && m_Storage.gameLogic != null)
                        {
                            m_Tables = m_Storage.gameLogic.GetComponent<PETable>() as IOperation;
                            m_Table = m_Storage.gameLogic.GetComponent<PETable>().Singles.Find(ret => ret != null && ret.CanOperateMask(EOperationMask.Eat)) as IOperation;
                        }
                    }
                }
            }

            IOperation m_Table;
            IOperation m_Tables;


            public IOperation Table
            {
                get { return m_Table; }
            }

            public IOperation Tables
            {
                get { return m_Tables; }
            }

            public void Init(PeEntity npc)
            {
                if (!m_Init)
                {
                    if (npc.NpcCmpt != null)
                        npc.NpcCmpt.npcCheck.ClearEatSlots();

                    if (eatTimeSlots != "")
                    {
                        string[] dataStr = PEUtil.ToArrayString(eatTimeSlots, ',');
                        foreach (string item in dataStr)
                        {
                            float[] data = PEUtil.ToArraySingle(item, '_');
                            if (data.Length == 2)
                            {
                                Pathea.CheckSlot slot = new CheckSlot(data[0], data[1]);
                                slots.Add(slot);
                                if (npc.NpcCmpt != null)
                                    npc.NpcCmpt.npcCheck.AddEatSlots(slot.minTime, slot.maxTime);
                            }
                        }
                    }

                    if (eatAnim != "")
                    {
                        eatAnims = PEUtil.ToArrayString(eatAnim, ',');
                    }

                    CurEatAnim = eatAnims[Random.Range(0, eatAnims.Length - 1)];
                    m_Init = true;
                }
            }

            public void InitTable()
            {
                if (m_Storage != null && m_Storage.gameLogic != null)
                {
                    // m_Table = m_Storage.gameLogic.GetComponent<PETable>() as IOperation;
                    m_Tables = m_Storage.gameLogic.GetComponent<PETable>() as IOperation;
                    m_Table = m_Storage.gameLogic.GetComponent<PETable>().Singles.Find(ret => ret != null && ret.CanOperateMask(EOperationMask.Eat)) as IOperation;
                }
            }

            public bool IsTimeSlot(float timeSlot)
            {
                return slots.Find(ret => ret.InSlot(timeSlot)) != null;
            }

            bool IspooolHunger(PeEntity entity)
            {
                float p = entity.GetAttribute(AttribType.Hunger) / entity.GetAttribute(AttribType.HungerMax);
                return p < minHunger;
            }

            bool IspooolHp(PeEntity entity)
            {
                float p = entity.GetAttribute(AttribType.Hp) / entity.GetAttribute(AttribType.HpMax);
                return p < minBlood;
            }

            bool IspooolComfort(PeEntity entity)
            {
                float p = entity.GetAttribute(AttribType.Comfort) / entity.GetAttribute(AttribType.ComfortMax);
                return p < minComfort;
            }

            bool NotHunger(PeEntity entity)
            {
                float p = entity.GetAttribute(AttribType.Hunger) / entity.GetAttribute(AttribType.HungerMax);
                return p > maxHunger;
            }

            bool NotHp(PeEntity entity)
            {
                float p = entity.GetAttribute(AttribType.Hp) / entity.GetAttribute(AttribType.HpMax);
                return p > maxBlood;
            }

            bool NotComfort(PeEntity entity)
            {
                float p = entity.GetAttribute(AttribType.Comfort) / entity.GetAttribute(AttribType.ComfortMax);
                return p > maxComfort;
            }


            public bool BeDispatchEat(PeEntity entity)
            {
                return IspooolHp(entity) || IspooolComfort(entity) || IspooolHunger(entity);
            }

            public bool IsEndDispatch(PeEntity entity)
            {
                return NotHp(entity) && NotHunger(entity) && NotComfort(entity);
            }

            public void NpcTalkSth(PeEntity npc)
            {
                List<NpcRandomTalkDb.Item> Items = NpcRandomTalkDb.GetTalkItems(npc);
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == null || Items[i].Type == AttribType.Max || Items[i].Level == ETalkLevel.Max)
                        continue;

                    npc.NpcCmpt.AddTalkInfo(Items[i].TalkType, ENpcSpeakType.Both, true);
                }
            }
        }

        Data m_Data;
        EThinkingType mDiningTh = EThinkingType.Dining;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;


            if (!IsNpcBase)
                return BehaveResult.Failure;

            m_Data.Init(entity);
            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Dining))
                return BehaveResult.Failure;

            if (!NpcThinkDb.CanDo(entity, mDiningTh))
                return BehaveResult.Failure;

            if (entity.NpcCmpt.lineType != ELineType.TeamEat)
                return BehaveResult.Failure;

            if (m_Data.Storage == null || m_Data.Storage.gameLogic == null)
                m_Data.Storage = CSUtils.GetTargetStorage(NpcEatDb.GetWantEatIds(entity), entity);

            if (m_Data.Table == null || m_Data.Tables == null || m_Data.Table.Equals(null) || m_Data.Tables.Equals(null))
                m_Data.InitTable();

            if (m_Data.Storage == null || m_Data.Table == null || m_Data.Tables == null || m_Data.Table.Equals(null) || m_Data.Tables.Equals(null))
            {
                if (NpcEatDb.IsNeedEatsth(entity) && Enemy.IsNullOrInvalid(attackEnemy))
                    m_Data.NpcTalkSth(entity);

                entity.NpcCmpt.ThinkAgent.RemoveThink(mDiningTh);
                return BehaveResult.Failure;
            }

            if (!NpcEatDb.CanEatFromStorage(entity, m_Data.Storage) || !m_Data.Tables.CanOperateMask(EOperationMask.Eat))
            {
                if (NpcEatDb.IsNeedEatsth(entity) && Enemy.IsNullOrInvalid(attackEnemy))
                    m_Data.NpcTalkSth(entity);

                entity.NpcCmpt.ThinkAgent.RemoveThink(mDiningTh);
                return BehaveResult.Failure;
            }
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            m_Data.Init(entity);
            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Dining))//|| m_Data.IsEndDispatch(entity)
            {
                m_Data.Table.StopOperate(Operator, EOperationMask.Eat);
                return BehaveResult.Failure;
            }

            if (!NpcThinkDb.CanDoing(entity, mDiningTh))
            {
                m_Data.Table.StopOperate(Operator, EOperationMask.Eat);
                return BehaveResult.Failure;
            }

            if (m_Data.Storage == null || m_Data.Table == null || m_Data.Tables == null || m_Data.Table.Equals(null) || m_Data.Tables.Equals(null))
                return BehaveResult.Failure;

            SetNpcAiType(ENpcAiType.NpcBaseTakeFood);

            if (entity.hasView && m_Data.Tables.CanOperateMask(EOperationMask.Eat) && !m_Data.Tables.ContainsOperator(Operator))
            {
                if (!m_Data.Tables.CanOperate(transform))
                {
                    //室内
                    bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
                    bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, entity.peTrans.forward, 2.0f);
                    if (_IsUnderBlock)
                        SetPosition(m_Data.Table.Trans.position);//MoveDirection(m_Data.Table.Trans.position - position, SpeedState.Run);
                    else
                        MoveToPosition(m_Data.Table.Trans.position, SpeedState.Run);

                    if (_IsForwardBlock)
                        SetPosition(m_Data.Table.Trans.position, false);

                    if (Stucking())
                        SetPosition(m_Data.Table.Trans.position, false);

                    if (IsReached(position, m_Data.Table.Trans.position, false))
                        SetPosition(m_Data.Table.Trans.position, false);
                }
                else
                {
                    MoveToPosition(Vector3.zero);
                    m_Data.m_StartEatTime = Time.time;
                    m_Data.m_CurEatTime = Random.Range(m_Data.minEatTime, m_Data.maxEatTime);
                    m_Data.Tables.StartOperate(Operator, EOperationMask.Eat);
                }
            }
            else
            {
                ItemAsset.ItemObject item;
                CSStorage storage;
                if (NpcEatDb.CanEatSthFromStorages(entity, Creater.Assembly.Storages, out item, out storage))//NpcEatDb.IsContinueEatFromStorage(entity, m_Data.Storage, out item))
                {
                    m_Data.Storage = storage;
                    if (entity.UseItem.GetCdByItemProtoId(item.protoId) < PETools.PEMath.Epsilon)
                    {
                        UseItem(item);
                        PEActionParamS param = PEActionParamS.param;
                        param.str = m_Data.CurEatAnim;
                        DoAction(PEActionType.Eat, param);
                        CSUtils.DeleteItemInStorage(m_Data.Storage, item.protoId);
                        //Debug.LogWarning("Hunger : " + GetAttribute(AttribType.Hunger));
                    }
                }
                else
                {
                    m_Data.Tables.StopOperate(Operator, EOperationMask.Eat);
                    entity.NpcCmpt.ThinkAgent.RemoveThink(mDiningTh);
                    return BehaveResult.Success;
                }

                //				if (Operator.Operate == null || Operator.Operate.Equals(null) || !Operator.IsActionRunning(PEActionType.Eat))
                //                    return BehaveResult.Failure;
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (m_Data != null && m_Data.Table != null && !m_Data.Table.Equals(null) && Operator != null && !Operator.Equals(null) && m_Data.Tables.ContainsOperator(Operator))
            {
                m_Data.Tables.StopOperate(Operator, EOperationMask.Eat);
            }
        }

    }

    /**************************************
    * 基地工作
    * 工作：修理机，回收机，合成、强化
    * 内容：跑到指定点播放指定动作
    * **********       *****************/
    [BehaveAction(typeof(BTNpcBaseWork), "NpcBaseWork")]
    public class BTNpcBaseWork : BTNormal
    {

        //Vector3 mStandPostion;
        bool mChangePlace;
        void StopWork()
        {
            SetNpcState(ENpcState.UnKnown);
            if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                Operator.Operate.StopOperate(Operator, EOperationMask.Work);

            return;
        }

        float startEndActionTime = 0.0f;
        float endActionTime = 2.0f;
        BehaveResult EndOperate(IOperation _operation, IOperator _IOperator)
        {
            if ((_operation != null && _operation.ContainsOperator(_IOperator)) || IsMotionRunning(PEActionType.Operation))
            {
                if (startEndActionTime == 0)
                    startEndActionTime = Time.time;
            }

            if (Time.time - startEndActionTime < endActionTime)
            {
                if (_operation != null && _operation.ContainsOperator(_IOperator))
                    _operation.StopOperate(_IOperator, EOperationMask.Work);
                else
                    if (_IOperator != null && !_IOperator.Equals(null) && _IOperator.Operate != null && !_IOperator.Equals(null)
                       && _IOperator.Operate.ContainsOperator(_IOperator))
                    {
                        _IOperator.Operate.StopOperate(_IOperator, EOperationMask.Work);
                    }

                return BehaveResult.Running;
            }
            else
            {
                startEndActionTime = 0;
                return BehaveResult.Failure;
            }

        }

        PEMachine mMachine;
        PEWork mWork;

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Worker)
                return BehaveResult.Failure;

            if (Work == null || Work.Equals(null)
                || Operator == null || Operator.Equals(null))
                return BehaveResult.Failure;

            mMachine = Work as PEMachine;
            if (mMachine == null)
                return BehaveResult.Failure;

            mWork = mMachine.GetStartOperate(EOperationMask.Work) as PEWork;
            if (mWork == null)
                return BehaveResult.Failure;

            SetNpcState(ENpcState.Prepare);
            //mStandPostion = mWork.Trans.position;
            mChangePlace = false;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Worker || hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy) ||
                Work == null || Work.Equals(null) || Operator == null || Operator.Equals(null))
            {
                return EndOperate(Work, Operator);

            }

            if (mChangePlace)
                return EndOperate(Work, Operator);

            //在指定的机器上，但是播放的不是对应的动作？
            if (Work != null && Work.ContainsOperator(Operator) && !IsMotionRunning(PEActionType.Operation))
            {
                //睡觉动作未正常结束，结束它！！
                if (IsActionRunning(PEActionType.Sleep))
                    EndAction(PEActionType.Sleep);

                mWork.StopOperate(Operator, EOperationMask.Work);
            }


            SetNpcAiType(ENpcAiType.NpcBaseWorker);
            PEMachine curMachine = Work as PEMachine;
            if (NpcJobStae == ENpcState.Work && curMachine != null && !mMachine.Equals(curMachine))//mStandPostion != mWork.Trans.position)
            {
                mChangePlace = true;
                //mStandPostion = mWork.Trans.position;
            }

            if (!Work.ContainsOperator(Operator))
            {
                if (!Work.CanOperate(transform))
                {

                    bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
                    bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, entity.peTrans.forward, 2.0f);
                    if (_IsUnderBlock)
                        SetPosition(mWork.Trans.position);//MoveDirection(mWork.Trans.position - position , SpeedState.Run);
                    else
                        MoveToPosition(mWork.Trans.position, SpeedState.Run);

                    if (_IsForwardBlock)
                        SetPosition(mWork.Trans.position, false);

                    if (Stucking())
                        SetPosition(mWork.Trans.position, false);

                    if (IsReached(position, mWork.Trans.position, false))
                        SetPosition(mWork.Trans.position, false);
                }
                else
                {
                    SetNpcState(ENpcState.Work);
                    MoveToPosition(Vector3.zero);

                    mWork = mMachine.GetStartOperate(EOperationMask.Work) as PEWork;
                    //mStandPostion = mWork.Trans.position;

                    mChangePlace = false;
                    mWork.StartOperate(Operator, EOperationMask.Work);
                }
            }
            else if (position != mWork.Trans.position)
            {
                SetPosition(mWork.Trans.position, false);
                SetRotation(mWork.Trans.rotation);
            }

            return BehaveResult.Running;
        }


        void Reset(Tree sender)
        {
            if (Creater != null && Creater.Assembly != null && NpcJobStae == ENpcState.Work)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Work);
            }
            SetNpcState(ENpcState.UnKnown);
        }
    }


    /**************************************
    * 工作等待
    * 工作不能正常执行时，等待工作行为
    * *************************************/
    [BehaveAction(typeof(BTNpcBaseWait), "NpcBaseJobWait")]
    public class BTNpcBaseWait : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public int Jobtype;
            [BehaveAttribute]
            public float WaitTime;
            [BehaveAttribute]
            public float RadiuMax;
            [BehaveAttribute]
            public float RadiuMin;

            public Vector3 mWaitPos;
            public float mStartWaitTime;
            public ENpcJob mNpcJobType { get { return (ENpcJob)Jobtype; } }


            //无寻路数据时
            public float sWalkTime0 = 2.0f;
            public float sWalkTime1 = 20.0f;
            public float sWalkTime2 = 30.0f;
            public float LastWalkTime = 0.0f;
            public float LastStopTime = 0.0f;

            int TIMES = 4;
            bool m_CalculatedDir;
            Vector3 m_AnchorDir;
            int underBlockTimes = 0;
            public bool EndUnderBlock()
            {
                if (underBlockTimes > TIMES)
                {
                    underBlockTimes = 0;
                    return true;
                }
                return false;
            }

            public Vector3 GetAnchorDir()
            {
                return m_AnchorDir;
            }

            public bool hasCalculatedDir { get { return m_CalculatedDir; } }
            public void ResetCalculatedDir()
            {
                m_CalculatedDir = false;
                underBlockTimes++;
            }
            public bool GetCanMoveDirtion(PeEntity entity, float minAngle)
            {
                if (!m_CalculatedDir)
                {
                    for (int i = 1; i < (int)360.0f / minAngle; i++)
                    {
                        m_AnchorDir = Quaternion.AngleAxis(minAngle * i, Vector3.up) * entity.peTrans.forward;
                        Debug.DrawRay(entity.position + Vector3.up, m_AnchorDir * 3.0f, Color.cyan);
                        if (!PETools.PEUtil.IsForwardBlock(entity, m_AnchorDir, 3.0f))
                        {
                            m_CalculatedDir = true;
                            return true;
                        }
                        m_AnchorDir = Vector3.zero;
                    }
                    return false;
                }
                return false;
            }
        }

        Data m_Data;


        void OnPathComplete(Pathfinding.Path path)
        {
            if (IsNpcBase && PETools.PEUtil.IsInAstarGrid(position) && path != null && path.vectorPath.Count > 15)
            {
                Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                if (NpcMgr.IsIncenterAraound(Creater.Assembly.Position, Creater.Assembly.Radius, pos))
                    m_Data.mWaitPos = pos;
                else
                    NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mWaitPos);
            }
            else
            {
                if (IsNpcBase)
                    NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mWaitPos);
            }

        }

        bool StopWait()
        {
            switch (m_Data.mNpcJobType)
            {
                case ENpcJob.Trainer:
                    return WorkEntity != null && WorkEntity.workTrans != null && NpcTrainerType != ETrainerType.none && IsNpcTrainning;
                case ENpcJob.Worker:
                    return WorkEntity != null && WorkEntity.workTrans != null;
                case ENpcJob.Processor:
                    return IsNpcProcessing;
                case ENpcJob.Follower:
                    return entity != null && entity.NpcCmpt != null && entity.NpcCmpt.IsServant;
                case ENpcJob.Doctor:
                    return Cured != null && !Cured.Equals(null) && WorkEntity != null && WorkEntity.workTrans != null;
                case ENpcJob.Farmer:
                    return IsStopFormerWait();
                default:
                    break;
            }
            return false;
        }


        bool IsStopFormerWait()
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
                return true;

            if (ContainsTitle(ENpcTitle.Plant))
            {
                return !CSUtils.FarmPlantReady(entity) && CSUtils.FindPlantPosNewChunk(entity) != null;
            }
            else if (ContainsTitle(ENpcTitle.Manage))
            {
                return (!CSUtils.FarmWaterReady(entity) && CSUtils.FindPlantToWater(entity) != null)
                    || (!CSUtils.FarmCleanReady(entity) && CSUtils.FindPlantToClean(entity) != null);
            }
            else if (ContainsTitle(ENpcTitle.Harvest))
            {
                return CSUtils.FindPlantGet(entity) != null
                       || CSUtils.FindPlantRemove(entity) != null;
            }

            return true;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != m_Data.mNpcJobType)//NpcJob == ENpcJob.Resident || NpcJob == ENpcJob.None)
                return BehaveResult.Failure;

            if (Creater == null || Creater.Assembly == null || hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (m_Data.mWaitPos == Vector3.zero)
                NpcMgr.GetRandomPathForCsWander(entity, Creater.Assembly.Position, transform.forward, 15.0f, Creater.Assembly.Radius, OnPathComplete);

            m_Data.mStartWaitTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!Enemy.IsNullOrInvalid(attackEnemy) || hasAnyRequest)
                return BehaveResult.Failure;

            if (!IsNpcBase || m_Data.mNpcJobType != NpcJob || hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))//NpcJob == ENpcJob.Resident || NpcJob == ENpcJob.None)
                return BehaveResult.Failure;

            //等待结束条件
            if (StopWait())
                return BehaveResult.Failure;

            if (IsMotionRunning(PEActionType.HoldShield))
                EndAction(PEActionType.HoldShield);

            if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
            {
                if (Operator.IsActionRunning(PEActionType.Sleep))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Sleep);
            }

            if (Operator.IsActionRunning(PEActionType.Sleep))
                Operator.EndAction(PEActionType.Sleep);

            if (Time.time - m_Data.LastWalkTime > m_Data.sWalkTime1)
            {
                m_Data.LastWalkTime = Time.time;
                m_Data.ResetCalculatedDir();
            }

            bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
            bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, entity.peTrans.forward, 2.0f);
            bool _canPointMove = m_Data.hasCalculatedDir && !_IsForwardBlock && !_IsUnderBlock;
            if (!_canPointMove)
            {
                if (m_Data.EndUnderBlock())
                {
                    m_Data.LastWalkTime = Time.time;
                    m_Data.ResetCalculatedDir();
                    if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mWaitPos))
                        SetPosition(m_Data.mWaitPos);

                    return BehaveResult.Success;
                }

                bool canMove = m_Data.GetCanMoveDirtion(entity, 30.0f) || Time.time - m_Data.LastWalkTime < m_Data.sWalkTime0;
                if (canMove)
                {
                    if (m_Data.GetAnchorDir() != Vector3.zero)
                        MoveDirection(m_Data.GetAnchorDir(), SpeedState.Walk);
                    else
                        StopMove();
                }
                else
                {
                    StopMove();
                }
            }
            else
            {
                if (IsReached(position, m_Data.mWaitPos, false))
                {
                    StopMove();
                    return BehaveResult.Success;
                }

                if (Stucking())
                {
                    if (entity.viewCmpt != null && entity.viewCmpt.hasView)
                    {
                        //寻路卡住
                        //有模型寻路卡住
                        if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mWaitPos))
                        {
                            if (Stucking(10.0f))
                            {
                                SetPosition(m_Data.mWaitPos);
                            }

                        }
                        else
                        {
                            StopMove();
                        }

                    }
                    else
                    {
                        //无模型卡住
                        if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mWaitPos))
                        {
                            SetPosition(m_Data.mWaitPos);
                        }
                        else
                            StopMove();

                    }
                    return BehaveResult.Success;
                }
                MoveToPosition(m_Data.mWaitPos, SpeedState.Walk);
            }
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            m_Data.mWaitPos = Vector3.zero;
        }
    }


    /**************************************
    * 是否继续等待
    * 
    * *************************************/
    [BehaveAction(typeof(BTIsWaitJob), "IsWaitJob")]
    public class BTIsWaitJob : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public int Jobtype;

            public ENpcJob mNpcJobType { get { return (ENpcJob)Jobtype; } }
        }

        bool StopWait()
        {
            switch (m_Data.mNpcJobType)
            {
                case ENpcJob.Trainer:
                    return WorkEntity != null && WorkEntity.workTrans != null && NpcTrainerType != ETrainerType.none && IsNpcTrainning;
                case ENpcJob.Worker:
                    return Work != null && !Work.Equals(null); //WorkEntity != null && WorkEntity.workTrans != null;
                case ENpcJob.Processor:
                    return IsNpcProcessing;
                case ENpcJob.Follower:
                    return entity != null && entity.NpcCmpt != null && entity.NpcCmpt.IsServant;
                case ENpcJob.Doctor:
                    return Cured != null && !Cured.Equals(null) && WorkEntity != null && WorkEntity.workTrans != null;
                case ENpcJob.Farmer:
                    return IsStopFormerWait();
                default:
                    break;
            }
            return false;
        }
        Data m_Data;

        bool IsStopFormerWait()
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
                return true;

            if (ContainsTitle(ENpcTitle.Plant))
            {
                return !CSUtils.FarmPlantReady(entity) && CSUtils.FindPlantPosNewChunk(entity) != null;
            }
            else if (ContainsTitle(ENpcTitle.Manage))
            {
                return (!CSUtils.FarmWaterReady(entity) && CSUtils.FindPlantToWater(entity) != null)
                    || (!CSUtils.FarmCleanReady(entity) && CSUtils.FindPlantToClean(entity) != null);
            }
            else if (ContainsTitle(ENpcTitle.Harvest))
            {
                return CSUtils.FindPlantGet(entity) != null
                       || CSUtils.FindPlantRemove(entity) != null;
            }

            return true;
        }
        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (NpcJob != m_Data.mNpcJobType)
                return BehaveResult.Failure;

            if (StopWait())
                return BehaveResult.Failure;
            else
                return BehaveResult.Success;
        }

    }

    /**************************************
    * 
    * 
    * *************************************/
    [BehaveAction(typeof(BTNpcBasePlant), "NpcBasePlant")]
    public class BTNpcBasePlant : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string plantAnim;
            [BehaveAttribute]
            public float plantTime;
            [BehaveAttribute]
            public float plantEndTime;
            [BehaveAttribute]
            public float plantRadius;
            [BehaveAttribute]
            public float plantWaitTime;

            public bool m_Plant;
            public bool m_Start;
            public float m_StartTime;
            //public bool m_WaitOrnot;
            //public float m_WaittingTime;
        }

        FarmWorkInfo m_Work;
        FarmWorkInfo m_CurWork;

        Vector3 WalkPos;
        Data m_Data;

        bool Reached(FarmWorkInfo work)
        {
            if (PEUtil.SqrMagnitudeH(work.m_Pos, position) < m_Data.plantRadius * m_Data.plantRadius)
                return true;
            else
                return false;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
                return BehaveResult.Failure;

            if (!ContainsTitle(ENpcTitle.Plant))
                return BehaveResult.Failure;

            if (!CSUtils.FarmPlantReady(entity))
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;
                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_Work = CSUtils.FindPlantPosNewChunk(entity);
            if (m_Work == null)
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;
                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_CurWork = m_Work;
            m_Data.m_Plant = false;
            m_Data.m_Start = false;
            // m_Data.m_WaitOrnot = false;
            m_Data.m_StartTime = 0.0f;

            SetNpcState(ENpcState.Prepare);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (!ContainsTitle(ENpcTitle.Plant))
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (IsMotionRunning(PEActionType.HoldShield))
            {
                EndAction(PEActionType.HoldShield);
            }

            //if (m_Data.m_WaitOrnot)
            //{
            //    if (Time.time - m_Data.m_WaittingTime <= m_Data.plantWaitTime)
            //    {
            //        MoveToPosition(WalkPos);
            //        if (Stucking())
            //            SetPosition(WalkPos);

            //        return BehaveResult.Running;
            //    }
            //    else
            //        return BehaveResult.Failure;

            //}

            SetNpcAiType(ENpcAiType.NpcBaseJobFarmer_Plant);
            if (!Reached(m_CurWork))
            {
                MoveToPosition(m_CurWork.m_Pos, SpeedState.Walk, false);

                if (Stucking() || IsReached(m_CurWork.m_Pos, position, false))
                {
                    SetPosition(m_CurWork.m_Pos);
                }
                else
                    return BehaveResult.Running;
            }
            else
            {
                MoveToPosition(Vector3.zero);

                if (!m_Data.m_Start)
                {
                    if (CSUtils.CheckFarmPlantAround(m_CurWork.m_Pos, entity))
                    {
                        if (!CSUtils.FarmPlantReady(entity))
                        {
                            //m_Data.m_WaitOrnot = true;
                            //m_Data.m_WaittingTime = Time.time;
                            //NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos);
                            return BehaveResult.Failure;
                        }
                        else
                        {
                            SetBool(m_Data.plantAnim, true);
                            m_Data.m_Start = true;
                            m_Data.m_StartTime = Time.time;
                            SetNpcState(ENpcState.Plant);
                        }
                    }
                    else
                    {

                        m_Work = CSUtils.FindPlantPosNewChunk(entity);
                        m_CurWork = m_Work;
                        if (m_Work == null)
                            return BehaveResult.Failure;
                    }

                }
                else
                {
                    if (Time.time - m_Data.m_StartTime >= m_Data.plantTime)
                    {
                        if (!m_Data.m_Plant)
                        {
                            m_Data.m_Plant = CSUtils.TryPlant(m_CurWork, entity);
                        }

                        if (!m_Data.m_Plant)
                        {
                            SetNpcState(ENpcState.UnKnown);
                            return BehaveResult.Failure;
                        }

                        if (Time.time - m_Data.m_StartTime >= m_Data.plantEndTime)
                        {
                            m_Data.m_Plant = false;
                            m_Data.m_Start = false;
                            m_Data.m_StartTime = 0.0f;

                            m_CurWork = null;

                            if (CSUtils.FarmPlantReady(entity))
                            {
                                if (m_Work != null)
                                {
                                    //检测此位置是否已经被种植,不是则找到新的clod
                                    m_CurWork = CSUtils.FindPlantPosSameChunk(m_Work, entity);

                                    // m_CurWork = CSUtils.FindPlantPos(m_Work, entity);
                                }

                                if (m_CurWork == null)
                                {
                                    //重新找到新的种植位置chuck
                                    CSUtils.ReturnCleanChunk(m_Work, entity);
                                    m_Work = CSUtils.FindPlantPosNewChunk(entity);

                                    //m_Work = CSUtils.FindPlantPos(entity);
                                    m_CurWork = m_Work;
                                }
                            }

                            if (m_CurWork == null)
                            {
                                SetNpcState(ENpcState.UnKnown);
                                return BehaveResult.Failure;
                            }
                        }
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (m_Work != null)
            {
                CSUtils.ReturnCleanChunk(m_Work, entity);
                m_Work = null;
            }
        }
    }

    [BehaveAction(typeof(BTNpcBasePlantWater), "NpcBasePlantWater")]
    public class BTNpcBasePlantWater : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string waterAnim;
            [BehaveAttribute]
            public float waterTime;
            [BehaveAttribute]
            public float waterEndTime;
            [BehaveAttribute]
            public float waterRadius;
            [BehaveAttribute]
            public float waterWaitTime;

            public bool m_Water;
            public bool m_Start;
            public float m_StartTime;
            //public bool m_WaitOrnot;
            //public float m_WaittingTime;
        }

        FarmWorkInfo m_Work;
        FarmWorkInfo m_CurWork;
        Vector3 WalkPos;

        Data m_Data;
        bool mCanWater;
        bool Reached(FarmWorkInfo work)
        {
            if (PEUtil.SqrMagnitudeH(work.m_Pos, position) < m_Data.waterRadius * m_Data.waterRadius)
                return true;
            else
                return false;
        }

        void PlantMoveTo(Vector3 pos, SpeedState state = SpeedState.Walk, bool avoid = true)
        {
            MoveToPosition(pos, state, avoid);
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
                return BehaveResult.Failure;

            if (!ContainsTitle(ENpcTitle.Manage))
                return BehaveResult.Failure;

            if (!CSUtils.FarmWaterReady(entity))
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;
                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_Work = CSUtils.FindPlantToWater(entity);
            if (m_Work == null)
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;

                //if (Creater == null || Creater.Assembly == null)
                //    return BehaveResult.Failure;

                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_Data.m_Water = false;
            m_Data.m_Start = false;
            // m_Data.m_WaitOrnot = false;
            m_Data.m_StartTime = 0.0f;

            m_CurWork = m_Work;

            mCanWater = false;
            SetNpcState(ENpcState.Prepare);

            return BehaveResult.Success;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (!ContainsTitle(ENpcTitle.Manage))
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (IsMotionRunning(PEActionType.HoldShield))
            {
                EndAction(PEActionType.HoldShield);
            }

            //SetNpcAiType(ENpcAiType.NpcBaseJobFarmer_PlantWater);
            //if (m_Data.m_WaitOrnot)
            //{
            //    if (Time.time - m_Data.m_WaittingTime <= m_Data.waterWaitTime)
            //    {
            //        PlantMoveTo(WalkPos,SpeedState.Walk,false);
            //        if (Stucking())
            //            SetPosition(WalkPos);

            //        return BehaveResult.Running;
            //    }
            //    else
            //        return BehaveResult.Failure;

            //}
            if (!mCanWater)
            {
                mCanWater = Reached(m_CurWork);
                PlantMoveTo(m_CurWork.m_Pos, SpeedState.Run, false);

                if (Stucking())
                {
                    if (!IsReached(position, m_CurWork.m_Pos, false, m_Data.waterRadius))
                        SetPosition(m_CurWork.m_Pos);

                    mCanWater = true;
                }
                else
                    return BehaveResult.Running;
            }
            else
            {
                PlantMoveTo(Vector3.zero, SpeedState.Walk, false);

                if (!m_Data.m_Start)
                {
                    if (m_CurWork != null && !CSUtils.FarmWaterEnough(entity, m_CurWork.m_Plant))
                    {
                        //m_Data.m_WaitOrnot = true;
                        //m_Data.m_WaittingTime = Time.time;
                        //if (Creater == null || Creater.Assembly == null)
                        //    return BehaveResult.Failure;

                        //NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos);
                        return BehaveResult.Failure;
                    }
                    else
                    {
                        SetBool(m_Data.waterAnim, true);
                        m_Data.m_Start = true;
                        m_Data.m_StartTime = Time.time;
                        SetNpcState(ENpcState.Watering);
                    }

                }
                else
                {
                    if (Time.time - m_Data.m_StartTime >= m_Data.waterTime)
                    {
                        if (!m_Data.m_Water)
                        {
                            m_Data.m_Water = CSUtils.TryWater(m_CurWork, entity);
                        }

                        if (!m_Data.m_Water)
                        {
                            SetNpcState(ENpcState.UnKnown);
                            return BehaveResult.Failure;
                        }

                        if (Time.time - m_Data.m_StartTime >= m_Data.waterEndTime)
                        {
                            m_Data.m_Water = false;
                            m_Data.m_Start = false;
                            m_Data.m_StartTime = 0.0f;
                            mCanWater = false;

                            m_CurWork = null;

                            m_Work = CSUtils.FindPlantToWater(entity);
                            m_CurWork = m_Work;

                            if (m_CurWork == null)
                            {
                                SetNpcState(ENpcState.UnKnown);
                                return BehaveResult.Success;
                            }
                        }
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (m_Work != null)
            {
                CSUtils.ReturnWaterPlant(entity, m_Work);
                m_Work = null;
            }
        }
    }

    [BehaveAction(typeof(BTNpcBasePlantClean), "NpcBasePlantClean")]
    public class BTNpcBasePlantClean : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string cleanAnim;
            [BehaveAttribute]
            public float cleanTime;
            [BehaveAttribute]
            public float cleanEndTime;
            [BehaveAttribute]
            public float cleanRadius;
            [BehaveAttribute]
            public float cleanWaitTime;

            public bool m_Clean;
            public bool m_Start;
            public float m_StartTime;
            //public bool m_WaitOrnot;
            //public float m_WaittingTime;
        }

        FarmWorkInfo m_Work;
        FarmWorkInfo m_CurWork;
        Vector3 WalkPos;
        bool mCanWater;
        Data m_Data;

        void CleanMoveTo(Vector3 pos, SpeedState state = SpeedState.Walk, bool avoid = true)
        {
            MoveToPosition(pos, state, avoid);
        }

        bool Reached(FarmWorkInfo work)
        {
            if (PEUtil.SqrMagnitudeH(work.m_Pos, position) < m_Data.cleanRadius * m_Data.cleanRadius)
                return true;
            else
                return false;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
                return BehaveResult.Failure;


            if (!ContainsTitle(ENpcTitle.Manage))
                return BehaveResult.Failure;

            if (!CSUtils.FarmCleanReady(entity))
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;
                //if (Creater == null || Creater.Assembly == null)
                //    return BehaveResult.Failure;

                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_Work = CSUtils.FindPlantToClean(entity);
            if (m_Work == null)
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;
                //if (Creater == null || Creater.Assembly == null)
                //    return BehaveResult.Failure;

                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_Data.m_Clean = false;
            m_Data.m_Start = false;
            // m_Data.m_WaitOrnot = false;
            m_Data.m_StartTime = 0.0f;
            mCanWater = false;
            m_CurWork = m_Work;

            SetNpcState(ENpcState.Prepare);
            return BehaveResult.Success;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (!ContainsTitle(ENpcTitle.Manage))
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (IsMotionRunning(PEActionType.HoldShield))
            {
                EndAction(PEActionType.HoldShield);
            }

            //SetNpcAiType(ENpcAiType.NpcBaseJobFarmer_Plantclean);
            //if (m_Data.m_WaitOrnot)
            //{
            //    if (Time.time - m_Data.m_WaittingTime <= m_Data.cleanWaitTime)
            //    {
            //        CleanMoveTo(WalkPos,SpeedState.Walk,false);
            //        if (Stucking())
            //            SetPosition(WalkPos);
            //        return BehaveResult.Running;
            //    }
            //    else
            //        return BehaveResult.Failure;

            //}
            if (!mCanWater)
            {
                mCanWater = Reached(m_CurWork);
                CleanMoveTo(m_CurWork.m_Pos, SpeedState.Run, false);

                if (Stucking())
                {
                    if (!IsReached(position, m_CurWork.m_Pos, false, m_Data.cleanRadius))
                        SetPosition(m_CurWork.m_Pos);

                    mCanWater = true;
                }
                else
                    return BehaveResult.Running;
            }
            else
            {
                CleanMoveTo(Vector3.zero, SpeedState.Walk, false);

                if (!m_Data.m_Start)
                {
                    if (!CSUtils.FarmCleanEnough(entity, m_CurWork.m_Plant))
                    {
                        //m_Data.m_WaitOrnot = true;
                        //m_Data.m_WaittingTime = Time.time;
                        //if (Creater == null || Creater.Assembly == null)
                        //    return BehaveResult.Failure;

                        //NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos);
                        return BehaveResult.Failure;
                    }
                    else
                    {
                        SetBool(m_Data.cleanAnim, true);
                        m_Data.m_Start = true;
                        m_Data.m_StartTime = Time.time;
                        SetNpcState(ENpcState.Weeding);
                    }

                }
                else
                {
                    if (Time.time - m_Data.m_StartTime >= m_Data.cleanTime)
                    {
                        if (!m_Data.m_Clean)
                        {
                            m_Data.m_Clean = CSUtils.TryClean(m_CurWork, entity);
                        }

                        if (!m_Data.m_Clean)
                        {
                            SetNpcState(ENpcState.UnKnown);
                            return BehaveResult.Failure;
                        }

                        if (Time.time - m_Data.m_StartTime >= m_Data.cleanEndTime)
                        {
                            m_Data.m_Clean = false;
                            m_Data.m_Start = false;
                            m_Data.m_StartTime = 0.0f;
                            mCanWater = false;

                            m_CurWork = null;
                            m_Work = CSUtils.FindPlantToClean(entity);
                            m_CurWork = m_Work;

                            if (m_CurWork == null)
                            {
                                SetNpcState(ENpcState.UnKnown);
                                return BehaveResult.Success;
                            }
                        }
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (m_Work != null)
            {
                CSUtils.ReturnCleanPlant(entity, m_Work);
                m_Work = null;
            }
        }
    }

    [BehaveAction(typeof(BTNpcBasePlantHarvest), "NpcBasePlantHarvest")]
    public class BTNpcBasePlantHarvest : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string harvestAnim;
            [BehaveAttribute]
            public float harvestTime;
            [BehaveAttribute]
            public float harvestEndTime;
            [BehaveAttribute]
            public float harvestRadius;
            [BehaveAttribute]
            public float harvestWaitTime;

            public bool m_Harvest;
            public bool m_Start;
            public float m_StartTime;
            //public bool m_WaitOrnot;
            //public float m_WaittingTime;
        }

        FarmWorkInfo m_Work;
        FarmWorkInfo m_CurWork;
        Vector3 WalkPos;
        bool mCanWater;
        Data m_Data;

        bool Reached(FarmWorkInfo work)
        {
            if (PEUtil.SqrMagnitudeH(work.m_Pos, position) < m_Data.harvestRadius * m_Data.harvestRadius)
                return true;
            else
                return false;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
                return BehaveResult.Failure;

            if (!ContainsTitle(ENpcTitle.Harvest))
                return BehaveResult.Failure;

            m_Work = CSUtils.FindPlantGet(entity);
            if (m_Work == null)
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;
                //if (Creater == null || Creater.Assembly == null)
                //    return BehaveResult.Failure;

                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_Data.m_Harvest = false;
            m_Data.m_Start = false;
            //  m_Data.m_WaitOrnot = false;
            m_Data.m_StartTime = 0.0f;
            mCanWater = false;
            m_CurWork = m_Work;

            SetNpcState(ENpcState.Prepare);
            return BehaveResult.Success;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (!ContainsTitle(ENpcTitle.Harvest))
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (IsMotionRunning(PEActionType.HoldShield))
            {
                EndAction(PEActionType.HoldShield);
            }

            //SetNpcAiType(ENpcAiType.NpcBaseJobFarmer_Harvest);
            //if (m_Data.m_WaitOrnot)
            //{
            //    if (Time.time - m_Data.m_WaittingTime <= m_Data.harvestWaitTime)
            //    {
            //        MoveToPosition(WalkPos,SpeedState.Walk,false);
            //        if (Stucking())
            //            SetPosition(WalkPos);
            //        return BehaveResult.Running;
            //    }
            //    else
            //        return BehaveResult.Failure;

            //}
            if (!mCanWater)
            {
                mCanWater = Reached(m_CurWork);
                MoveToPosition(m_CurWork.m_Pos, SpeedState.Run, false);

                if (Stucking())
                {
                    if (!IsReached(position, m_CurWork.m_Pos, false, m_Data.harvestRadius))
                        SetPosition(m_CurWork.m_Pos);

                    mCanWater = true;
                }
                else
                    return BehaveResult.Running;
            }
            else
            {
                MoveToPosition(Vector3.zero, SpeedState.Walk, false);

                if (!m_Data.m_Start)
                {
                    SetBool(m_Data.harvestAnim, true);
                    m_Data.m_Start = true;
                    m_Data.m_StartTime = Time.time;
                    SetNpcState(ENpcState.Gain);
                }
                else
                {
                    if (Time.time - m_Data.m_StartTime >= m_Data.harvestTime)
                    {
                        if (!m_Data.m_Harvest)
                        {
                            bool success = CSUtils.TryHarvest(m_CurWork, entity);
                            m_Data.m_Harvest = true;

                            if (!success)
                            {
                                SetNpcState(ENpcState.UnKnown);
                                return BehaveResult.Failure;
                            }
                        }

                        if (Time.time - m_Data.m_StartTime >= m_Data.harvestEndTime)
                        {
                            m_Data.m_Harvest = false;
                            m_Data.m_Start = false;
                            m_Data.m_StartTime = 0.0f;
                            mCanWater = false;

                            m_CurWork = null;

                            m_Work = CSUtils.FindPlantGet(entity);
                            m_CurWork = m_Work;

                            if (m_CurWork == null)
                            {
                                SetNpcState(ENpcState.UnKnown);
                                return BehaveResult.Success;
                            }
                        }
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (m_Work != null)
            {
                CSUtils.ReturnHarvestPlant(entity, m_Work);
                m_Work = null;
            }
        }
    }

    [BehaveAction(typeof(BTNpcBaseClearPlant), "NpcBaseClearPlant")]
    public class BTNpcBaseClearPlant : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string clearAnim;
            [BehaveAttribute]
            public float clearTime;
            [BehaveAttribute]
            public float clearEndTime;
            [BehaveAttribute]
            public float clearRadius;
            [BehaveAttribute]
            public float clearWaitTime;

            public bool m_Clear;
            public bool m_Start;
            public float m_StartTime;
            //public bool m_WaitOrnot;
            //public float m_WaittingTime;
        }

        FarmWorkInfo m_Work;
        FarmWorkInfo m_CurWork;
        Vector3 WalkPos;
        bool mCanWater;
        Data m_Data;

        bool Reached(FarmWorkInfo work)
        {
            if (PEUtil.SqrMagnitudeH(work.m_Pos, position) < m_Data.clearRadius * m_Data.clearRadius)
                return true;
            else
                return false;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
                return BehaveResult.Failure;


            if (!ContainsTitle(ENpcTitle.Harvest))
                return BehaveResult.Failure;

            m_Work = CSUtils.FindPlantRemove(entity);
            if (m_Work == null)
            {
                //m_Data.m_WaitOrnot = true;
                //m_Data.m_WaittingTime = Time.time;
                //if (Creater == null || Creater.Assembly == null)
                //    return BehaveResult.Failure;

                //if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out WalkPos))
                //    return BehaveResult.Success;
                //else
                return BehaveResult.Failure;
            }

            m_Data.m_Clear = false;
            m_Data.m_Start = false;
            //m_Data.m_WaitOrnot = false;
            m_Data.m_StartTime = 0.0f;
            mCanWater = false;
            m_CurWork = m_Work;

            SetNpcState(ENpcState.Prepare);
            return BehaveResult.Success;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || NpcJob != ENpcJob.Farmer)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (!ContainsTitle(ENpcTitle.Harvest))
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            //SetNpcAiType(ENpcAiType.NpcBaseJobFarmer_Harvest);
            //if (m_Data.m_WaitOrnot)
            //{
            //    if (Time.time - m_Data.m_WaittingTime <= m_Data.clearWaitTime)
            //    {
            //        MoveToPosition(WalkPos,SpeedState.Walk,false);
            //        if (Stucking())
            //            SetPosition(WalkPos);
            //        return BehaveResult.Running;
            //    }
            //    else
            //        return BehaveResult.Failure;

            //}
            if (!mCanWater)
            {
                mCanWater = Reached(m_CurWork);
                MoveToPosition(m_CurWork.m_Pos, SpeedState.Run, false);

                if (Stucking())
                {
                    if (!IsReached(position, m_CurWork.m_Pos, false, m_Data.clearRadius))
                        SetPosition(m_CurWork.m_Pos);

                    mCanWater = true;
                }
                else
                    return BehaveResult.Running;
            }
            else
            {
                MoveToPosition(Vector3.zero, SpeedState.Walk, false);

                if (!m_Data.m_Start)
                {
                    SetBool(m_Data.clearAnim, true);
                    m_Data.m_Start = true;
                    m_Data.m_StartTime = Time.time;
                    SetNpcState(ENpcState.Gain);
                }
                else
                {
                    if (Time.time - m_Data.m_StartTime >= m_Data.clearTime)
                    {
                        if (!m_Data.m_Clear)
                        {
                            bool success = CSUtils.TryRemove(m_CurWork, entity);
                            m_Data.m_Clear = true;

                            if (!success)
                            {
                                SetNpcState(ENpcState.UnKnown);
                                return BehaveResult.Failure;
                            }
                        }

                        if (Time.time - m_Data.m_StartTime >= m_Data.clearEndTime)
                        {
                            m_Data.m_Clear = false;
                            m_Data.m_Start = false;
                            m_Data.m_StartTime = 0.0f;
                            mCanWater = false;

                            m_CurWork = null;

                            m_Work = CSUtils.FindPlantRemove(entity);
                            m_CurWork = m_Work;

                            if (m_CurWork == null)
                            {
                                SetNpcState(ENpcState.UnKnown);
                                return BehaveResult.Success;
                            }
                        }
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (m_Work != null)
            {
                CSUtils.ReturnHarvestPlant(entity, m_Work);
                m_Work = null;
            }
        }
    }

    [BehaveAction(typeof(BTNpcBasePatrol), "NpcBasePatrol")]
    public class BTNpcBasePatrol : BTNormal
    {
        Vector3 m_CurPatrolPos;
        float mStartTime;
        float mPatrolTime = 35.0f;
        bool GetPatrolPos(out Vector3 guardPos)
        {
            if (BaseEntities != null)
            {
                List<CSEntity> entities = BaseEntities.FindAll(ret => ret.gameObject != null);
                if (entities.Count > 0)
                {
                    CSEntity entity = entities[Random.Range(0, entities.Count)];
                    float vx = Random.Range(entity.Bound.extents.x + 1.0f, entity.Bound.extents.x + 3.0f);
                    float vz = Random.Range(entity.Bound.extents.z + 1.0f, entity.Bound.extents.z + 3.0f);
                    vx *= Random.value < 0.5f ? 1 : -1;
                    vz *= Random.value < 0.5f ? 1 : -1;
                    Vector3 pos = entity.gameObject.transform.TransformPoint(new Vector3(vx, 0.0f, vz));
                    Vector3 newPos;
                    if (PEUtil.GetPositionLayer(pos, out newPos, WanderLayer, IgnoreWanderLayer))
                    {
                        guardPos = newPos;
                        return true;
                    }
                }
            }

            guardPos = Vector3.zero;
            return false;
        }

        bool GetCanWalkPos(out Vector3 guardPos)
        {
            Vector3 PatrolPos = Vector3.zero;
            if (GetPatrolPos(out PatrolPos))
            {
                if (AiUtil.GetNearNodePosWalkable(PatrolPos, out guardPos))
                {
                    return true;
                }
            }
            guardPos = Vector3.zero;
            return false;
        }

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Soldier || NpcSoldier != ENpcSoldier.Patrol)
                return BehaveResult.Failure;

            //			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Patrol))
            //			{
            //				return BehaveResult.Failure;
            //			}
            //			
            //			if(!NpcThinkDb.CanDo(m_Npc,EThinkingType.Patrol))
            //				return BehaveResult.Failure;

            if (hasAttackEnemy)
                return BehaveResult.Failure;

            if (!GetCanWalkPos(out m_CurPatrolPos))
                return BehaveResult.Failure;

            SetNpcState(ENpcState.Patrol);
            mStartTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Soldier || NpcSoldier != ENpcSoldier.Patrol)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            //			if(!NpcTypeDb.CanRun(NpcCmdId,ENpcControlType.Patrol))
            //			{
            //				return BehaveResult.Failure;
            //			}
            //			
            //			if(!NpcThinkDb.CanDoing(m_Npc,EThinkingType.Patrol))
            //				return BehaveResult.Failure;

            if (hasAttackEnemy)
                return BehaveResult.Failure;

            SetNpcAiType(ENpcAiType.NpcBaseSoldier_Patrol);
            if (Stucking(5.0f))
            {
                SetNpcState(ENpcState.UnKnown);
                SetPosition(m_CurPatrolPos);
                return BehaveResult.Success;
            }

            if (PEUtil.SqrMagnitudeH(position, m_CurPatrolPos) < 0.5f * 0.5f)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Success;
            }

            if (Time.time - mStartTime > mPatrolTime)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Success;
            }

            MoveToPosition(m_CurPatrolPos);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTNpcBaseGuard), "NpcBaseGuard")]
    public class BTNpcBaseGuard : BTNormal
    {
        Vector3 m_CurWanderPos;

        bool GetWanderPos(out Vector3 guardPos)
        {
            if (BaseEntities != null)
            {
                List<CSEntity> entities = BaseEntities.FindAll(ret => ret.gameObject != null);
                if (entities.Count > 0)
                {
                    CSEntity entity = entities[Random.Range(0, entities.Count)];
                    float vx = Random.Range(entity.Bound.extents.x + 1.0f, entity.Bound.extents.x + 3.0f);
                    float vz = Random.Range(entity.Bound.extents.z + 1.0f, entity.Bound.extents.z + 3.0f);
                    vx *= Random.value < 0.5f ? 1 : -1;
                    vz *= Random.value < 0.5f ? 1 : -1;
                    Vector3 pos = entity.gameObject.transform.TransformPoint(new Vector3(vx, 0.0f, vz));
                    Vector3 newPos;
                    if (PEUtil.GetPositionLayer(pos, out newPos, WanderLayer, IgnoreWanderLayer))
                    {
                        guardPos = newPos;
                        return true;
                    }
                }
            }

            guardPos = Vector3.zero;
            return false;
        }

        bool GetCanWalkPos(out Vector3 walkPos)
        {
            Vector3 newPos;
            if (GetWanderPos(out newPos))
            {
                if (AiUtil.GetNearNodePosWalkable(newPos, out walkPos))
                    return true;
            }
            walkPos = Vector3.zero;
            return false;
        }

        Vector3 GetGuardPos(Vector3 nowPos, Vector3 centorPos, float Maxradiu, float minRadiu)
        {
            float radiu = PEUtil.Magnitude(nowPos, centorPos);
            if (radiu < Maxradiu && radiu > minRadiu)
            {
                return nowPos;
            }
            else
            {
                Vector3 pos;
                if (NpcCanWalkPos(centorPos, Maxradiu, out pos))
                {
                    return pos;
                }
                else
                    return centorPos;
            }
        }

        Vector3 mGuardPos;
        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Soldier || NpcSoldier != ENpcSoldier.Guard)
                return BehaveResult.Failure;

            //if (!GetCanWalkPos(out m_CurWanderPos))
            //    return BehaveResult.Failure;

            if (NpcMgr.IsOutRadiu(position, Creater.Assembly.Position, Creater.Assembly.Radius))
                return BehaveResult.Failure;

            mGuardPos = GetGuardPos(position, Creater.Assembly.Position, Creater.Assembly.Radius, 5.0f);
            SetNpcState(ENpcState.Patrol);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcBaseSoldier_Guard);
            if (!IsNpcBase || NpcJob != ENpcJob.Soldier || NpcSoldier != ENpcSoldier.Guard)
            {
                SetNpcState(ENpcState.UnKnown);
                return BehaveResult.Failure;
            }

            if (NpcMgr.IsOutRadiu(position, Creater.Assembly.Position, Creater.Assembly.Radius))
                return BehaveResult.Failure;

            if (Stucking() || PEUtil.Magnitude(position, mGuardPos) < 1.0f)
            {
                SetNpcState(ENpcState.Patrol);
                StopMove();
                return BehaveResult.Running;
            }

            if (Stucking())
            {
                mGuardPos = GetGuardPos(position, Creater.Assembly.Position, Creater.Assembly.Radius, 5.0f);
            }

            MoveToPosition(mGuardPos, SpeedState.Walk);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(NpcBaseCollect), "NpcBaseCollect")]
    public class NpcBaseCollect : BTNormal
    {

        static Vector3 s_Position = new Vector3(0.0f, -10000.0f, 0.0f);

        float m_StartTime = 0.0f;

        Vector3 m_Moveposition;
        //Vector3 m_Backposition;
        //Vector3 Backpos;

        bool m_Transparent;
        bool m_SetPos;

        //bool Reached;

        Vector3 GetMovePosition()
        {
            return PEUtil.GetRandomPosition(position, 1024.0f, 2048.0f);
        }

        Vector3 GetPosition()
        {
            return PEUtil.GetRandomPositionOnGroundForWander(Creater.Assembly.Position, Creater.Assembly.Radius * 0.7f, Creater.Assembly.Radius);
        }

        Vector3 GetCSWokePosition(CSEntity WokeEnity)
        {
            return WokeEnity.Position;
        }

        Vector3 GetCollectPos(CSEntity WokeEnity)
        {
            return (WokeEnity != null && WokeEnity.workTrans.Length > 0) ? WokeEnity.workTrans[8].position : WokeEnity.Position;
        }

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Processor)
                return BehaveResult.Failure;

            if (!IsNpcProcessing || WorkEntity == null)
                return BehaveResult.Failure;

            m_SetPos = false;
            m_Transparent = false;
            //Reached = false;
            m_StartTime = Time.time;
            m_Moveposition = GetMovePosition();
            //m_Backposition = GetCSWokePosition(WorkEntity);

            //if (WorkEntity.workTrans != null)
            //    Backpos = WorkEntity.workTrans[Random.Range(0, WorkEntity.workTrans.Length)].position;
            //else
            //    Backpos = PEUtil.GetRandomPosition(Creater.Assembly.Position, 4, 9.0f);

            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcBaseJobProcessor_Collect);
            if (!IsNpcBase || NpcJob != ENpcJob.Processor)
                return BehaveResult.Failure;

            if (NpcJob != ENpcJob.Processor && IsNpcProcessing)
            {
                Fadein(3.0f);
                Vector3 v3 = GetPosition();
                SetPosition(v3);
                return BehaveResult.Success;
                //生成背包
            }

            //正常结束
            if (!IsNpcProcessing)
            {
                Fadein(3.0f);
                Vector3 v3 = GetPosition();
                SetPosition(v3);
                return BehaveResult.Success;
                //生成背包
            }

            float time = Time.time - m_StartTime;
            if (time < 10.0f)
            {
                bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
                bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, existent.forward, 2.0f);
                if (_IsUnderBlock)
                    SetPosition(m_Moveposition);
                else
                    MoveToPosition(m_Moveposition, SpeedState.Run);

                if (_IsForwardBlock)
                    SetPosition(m_Moveposition);

            }
            else if (time < 13.0f)
            {
                if (!m_Transparent)
                {
                    Fadeout(3.0f);
                    m_Transparent = true;
                }
            }
            else
            {
                if (!m_SetPos)
                {
                    // SetPosition(GetCollectPos(WorkEntity));
                    //StopMove();
                    SetPosition(s_Position);
                    m_SetPos = true;
                }
            }

            ItemAsset.ItemObject item = null;
            CSStorage storage = null;
            if (NpcEatDb.CanEatSthFromStorages(entity, Creater.Assembly.Storages, out item, out storage))
            {
                if (entity.UseItem.GetCdByItemProtoId(item.protoId) < PETools.PEMath.Epsilon)
                {
                    UseItem(item);
                    CSUtils.DeleteItemInStorage(storage, item.protoId);
                }
            }

            return BehaveResult.Running;
        }

    }


    [BehaveAction(typeof(NpcBaseWaitCollect), "NpcBaseWaitCollect")]
    public class NpcBaseWaitCollect : BTNormal
    {
        Vector3 mWaitPos;

        void OnPathComplete(Pathfinding.Path path)
        {
            if (PETools.PEUtil.IsInAstarGrid(position) && path != null && path.vectorPath.Count > 0)
            {
                Vector3 pos = path.vectorPath[path.vectorPath.Count - 1];
                if (NpcMgr.IsIncenterAraound(Creater.Assembly.Position, Creater.Assembly.Radius, pos))
                    mWaitPos = pos;
                else if (Creater != null && Creater.Assembly != null)
                    NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out mWaitPos);
            }
            else
            {
                if (Creater != null && Creater.Assembly != null)
                    NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out mWaitPos);
            }

        }

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Processor)
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy) || hasAnyRequest)
                return BehaveResult.Failure;

            if (IsNpcProcessing || WorkEntity == null)
                return BehaveResult.Failure;

            if (Creater == null || Creater.Assembly == null)
                return BehaveResult.Failure;

            if (!NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out mWaitPos))
                return BehaveResult.Failure;

            //NpcMgr.GetRandomPathForCsWander(entity,Creater.Assembly.Position,transform.forward,15.0f, Creater.Assembly.Radius,OnPathComplete);
            //mWaitPos = PEUtil.GetRandomPositionOnGroundForWander(Creater.Assembly.Position, Creater.Assembly.Radius *0.7f, Creater.Assembly.Radius);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Processor)
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy) || hasAnyRequest)
                return BehaveResult.Failure;

            if (IsNpcProcessing || WorkEntity == null)
                return BehaveResult.Failure;

            if (IsMotionRunning(PEActionType.HoldShield))
                EndAction(PEActionType.HoldShield);

            if (IsReached(position, mWaitPos, false))
            {
                return BehaveResult.Failure;
            }
            else
            {
                if (Stucking())
                {
                    SetPosition(mWaitPos);
                    return BehaveResult.Failure;
                }
                MoveToPosition(mWaitPos, SpeedState.Walk);
            }
            return BehaveResult.Running;
        }
    }


    [BehaveAction(typeof(NpcBaseDoctor), "NpcBaseDoctor")]
    public class NpcBaseDoctor : BTNormal
    {
        Vector3 m_MoveToPostion;
        //Vector3 m_DoorPostion = Vector3.zero;
        //float m_StartTime = 0;
        //bool HasReached;
        //float m_Roata;
        bool mChangeAction;

        static float endActionTime = 2.0f;
        float startEndActionTime = 0.0f;
        Vector3 GetCSWokePosition(CSEntity WokeEnity)
        {
            return WokeEnity.Position;
        }

        BehaveResult EndOperate()
        {
            if ((Cured != null && Cured.ContainsOperator(Operator)) || IsMotionRunning(PEActionType.Operation))
            {
                if (startEndActionTime == 0)
                    startEndActionTime = Time.time;
            }

            if (Time.time - startEndActionTime < endActionTime)
            {
                if (Cured != null && Cured.ContainsOperator(Operator))
                    Cured.StopOperate(Operator, EOperationMask.Cure);
                else
                    if (Operator != null && !Operator.Equals(null) && Operator.Operate != null && !Operator.Equals(null)
                            && Operator.Operate.ContainsOperator(Operator))
                    {
                        Operator.Operate.StopOperate(Operator, EOperationMask.Cure);
                    }

                return BehaveResult.Running;
            }
            else
            {
                //				if (m_DoorPostion != Vector3.zero)
                //					SetPosition(m_DoorPostion);

                startEndActionTime = 0;
                return BehaveResult.Failure;
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase || NpcJob != ENpcJob.Doctor)
                return BehaveResult.Failure;

            if (WorkEntity == null || WorkEntity.workTrans == null)
                return BehaveResult.Failure;

            if (Cured == null || Cured.Equals(null) || Operator == null || Operator.Equals(null))
                return BehaveResult.Failure;

            m_MoveToPostion = WorkEntity.workTrans[0].position;
            //m_Roata = WorkEntity.workTrans[0].rotation.eulerAngles.y;
            //if (WorkEntity.workTrans.Length == 2)
                //m_DoorPostion = WorkEntity.workTrans[1].position;

            SetNpcState(ENpcState.Prepare);
            //HasReached = false;
            mChangeAction = false;
            //m_StartTime = 0;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcBaseJobDoctor);
            if (!IsNpcBase || NpcJob != ENpcJob.Doctor)
            {
                return EndOperate();
            }

            if (WorkEntity == null || WorkEntity.gameLogic == null)
            {
                return EndOperate();

            }

            if (hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
            {
                return EndOperate();
            }

            if (mChangeAction)
            {
                return EndOperate();
            }

            //在正确的机器上，但是不是播放的该机器对应的动作？
            if (Cured != null && Cured.ContainsOperator(Operator) && !IsActionRunning(PEActionType.Operation))
            {
                //结束睡觉动作（睡觉可能会被特殊打断动作未结束）
                if (IsActionRunning(PEActionType.Sleep))
                    EndAction(PEActionType.Sleep);

                Cured.StopOperate(Operator, EOperationMask.Cure);
            }

            if (WorkEntity != null && m_MoveToPostion != WorkEntity.workTrans[0].position)
            {
                mChangeAction = true;
                m_MoveToPostion = WorkEntity.workTrans[0].position;
                //m_Roata = WorkEntity.workTrans[0].rotation.eulerAngles.y;
                //HasReached = false;

            }

            if (!Cured.ContainsOperator(Operator))
            {
                if (!Cured.CanOperate(transform))
                {
                    Vector3 v3 = (WorkEntity.workTrans.Length == 2) ? WorkEntity.workTrans[1].position : m_MoveToPostion;
                    MoveToPosition(v3, SpeedState.Run);

                    if (IsReached(position, v3, false))
                        SetPosition(v3);

                    if (Stucking())
                        SetPosition(v3);

                }
                else
                {
                    StopMove();
                    SetPosition(WorkEntity.workTrans[0].position);
                    SetRotation(WorkEntity.workTrans[0].rotation);
                    Cured.StartOperate(Operator, EOperationMask.Cure);
                    SetNpcState(ENpcState.Work);
                    //HasReached = true;
                    mChangeAction = false;
                }
            }
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (Creater != null && Creater.Assembly != null && NpcJobStae == ENpcState.Work)
            {
                //				Vector3 WalkPos;
                //				if(NpcCanWalkPos(Creater.Assembly.Position,Creater.Assembly.Radius,out WalkPos))
                //					SetPosition(WalkPos);

                if (Cured != null && Cured.ContainsOperator(Operator))
                    Cured.StopOperate(Operator, EOperationMask.Cure);
            }
            m_MoveToPostion = Vector3.zero;
            SetNpcState(ENpcState.UnKnown);
        }
    }


    [BehaveAction(typeof(NpcMedicalDiagnose), "NpcMedicalDiagnose")]
    public class NpcMedicalDiagnose : BTNormal
    {

        class Data
        {
            [BehaveAttribute]
            public string WaitAnim;
            [BehaveAttribute]
            public float WaitTime;

        }

        Data m_Data;

        bool IsReadyDiagnose;
        bool HasReached;
        CSMedicalCheck m_CSMedicalCheck;
        //PEPatients m_pePatitents;
        Vector3 m_Moveposition;
        float m_Roate;
        //float m_StartTime = 0.0f;

        float m_WaitingTime = 0.0f;
        Vector3 m_WaitingPos;
        //string mWaitAnim = "BreathOnOperatingTable";
        Vector3 GetPosition(Vector3 pos)
        {
            return PEUtil.GetRandomPosition(pos, 7.0f, 7.0f) + Vector3.up * 2.0f;
        }

        bool EndCureSleep()
        {
            if (Sleep != null && !Sleep.Equals(null) && Sleep.ContainsOperator(Operator))
            {
                return Sleep.StopOperate(Operator, EOperationMask.Sleep);
            }
            else if (Operator != null && !Operator.Equals(null) && !Operator.Operate.Equals(null) && Operator.Operate.ContainsOperator(Operator))
            {
                return Operator.Operate.StartOperate(Operator, EOperationMask.Sleep);
            }
            return true;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Cure))
                return BehaveResult.Failure;

            if (!NpcThinkDb.CanDo(entity, EThinkingType.Cure))
                return BehaveResult.Failure;

            if (NpcMedicalState == ENpcMedicalState.SearchTreat || NpcMedicalState == ENpcMedicalState.SearchHospital || NpcMedicalState == ENpcMedicalState.Treating)
                return BehaveResult.Success;


            if (!IsNeedMedicine)
                return BehaveResult.Failure;

            m_CSMedicalCheck = CSMain.FindMedicalCheck(out IsReadyDiagnose, entity);
            if (m_CSMedicalCheck == null || m_CSMedicalCheck.resultTrans == null || !m_CSMedicalCheck.IsDoctorReady())
            {
                m_WaitingTime = Time.time;
                SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
                return BehaveResult.Success;
            }

            if (IsReadyDiagnose && Sleep != null && Sleep.ContainsOperator(Operator))
            {
                Sleep.StopOperate(Operator, EOperationMask.Sleep);
            }

            m_Moveposition = m_CSMedicalCheck.resultTrans[1].position;
            m_Roate = m_CSMedicalCheck.resultTrans[1].rotation.eulerAngles.y;
            //m_pePatitents = m_CSMedicalCheck.pePatient;

            if (NpcMedicalState == ENpcMedicalState.Diagnosing)
            {
                if (!CSMain.TryGetCheck(entity))
                    return BehaveResult.Failure;

                SetPosition(m_CSMedicalCheck.resultTrans[1].position);
                SetRotation(m_CSMedicalCheck.resultTrans[1].rotation);
                m_CSMedicalCheck.pePatient.StartOperate(Operator, EOperationMask.Lay);
                SetPosition(m_CSMedicalCheck.resultTrans[0].position);
                HasReached = true;
                return BehaveResult.Success;
            }

            m_WaitingTime = 0.0f;
            if (IsReadyDiagnose)
                SetMedicineSate(ENpcMedicalState.SearchDiagnos);
            else
            {
                m_WaitingTime = Time.time;
                SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
            }

            HasReached = false;
            //m_StartTime = Time.time;
            return BehaveResult.Success;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!IsNpcBase || Sleep == null || Sleep.Equals(null))
            {
                if (EndCureSleep())
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            if (!IsNeedMedicine)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Cure))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                if (Sleep != null && Sleep.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Sleep))
                    Sleep.StopOperate(Operator, EOperationMask.Sleep);

                return BehaveResult.Failure;
            }

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Cure) && m_CSMedicalCheck != null)
            {
                if (m_CSMedicalCheck.pePatient.ContainsOperator(Operator))
                    m_CSMedicalCheck.pePatient.StopOperate(Operator, EOperationMask.Lay);

                CSMain.KickOutFromHospital(entity);
                return BehaveResult.Failure;
            }

            if (NpcMedicalState == ENpcMedicalState.Cure || NpcMedicalState == ENpcMedicalState.SearchTreat || NpcMedicalState == ENpcMedicalState.Treating || NpcMedicalState == ENpcMedicalState.SearchHospital)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Cure))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                return BehaveResult.Success;
            }

            if (NpcMedicalState != ENpcMedicalState.Diagnosing && !NpcThinkDb.CanDoing(entity, EThinkingType.Cure))
            {
                if (EndCureSleep())
                    return BehaveResult.Failure;
                else
                    return BehaveResult.Running;
            }

            if (NpcMedicalState == ENpcMedicalState.WaitForDiagnos)
            {
                if (Time.time - m_WaitingTime > m_Data.WaitTime)
                {
                    SetMedicineSate(ENpcMedicalState.SearchDiagnos);
                    return BehaveResult.Failure;
                }

                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Cure))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                if (Sleep != null && !Sleep.ContainsOperator(Operator))
                {
                    if (!Sleep.CanOperate(transform))
                    {
                        MoveToPosition(Sleep.Trans.position, SpeedState.Walk);
                        if (Stucking(3.0f))
                            SetPosition(Sleep.Trans.position);

                        if (IsReached(position, Sleep.Trans.position, false))
                            SetPosition(Sleep.Trans.position);

                    }
                    else
                    {

                        PEBed bed = Sleep as PEBed;
                        PESleep sleep = bed.GetStartOperate(EOperationMask.Sleep) as PESleep;
                        if (null != sleep)
                        {
                            PEActionParamVQNS param = PEActionParamVQNS.param;
                            param.vec = sleep.transform.position;
                            param.q = sleep.transform.rotation;
                            param.n = 0;
                            param.str = m_Data.WaitAnim;
                            bed.StartOperate(Operator, EOperationMask.Sleep, param);
                        }
                    }
                }

                if (Sleep != null && Sleep.ContainsOperator(Operator) && !IsMotionRunning(PEActionType.Sleep))
                {
                    Sleep.StopOperate(Operator, EOperationMask.Sleep);
                }

                return BehaveResult.Running;
            }

            if (HasReached && NpcMedicalState == ENpcMedicalState.SearchDiagnos)
            {
                if (!CSMain.TryGetCheck(entity))
                {
                    m_WaitingTime = Time.time;
                    SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
                    HasReached = false;
                    return BehaveResult.Running;
                }

                if (!m_CSMedicalCheck.pePatient.ContainsOperator(Operator))
                {
                    SetPosition(m_CSMedicalCheck.resultTrans[1].position);
                    SetRotation(m_CSMedicalCheck.resultTrans[1].rotation);
                    m_CSMedicalCheck.pePatient.StartOperate(Operator, EOperationMask.Lay);
                    SetPosition(m_CSMedicalCheck.resultTrans[0].position);
                }
            }

            if (NpcMedicalState == ENpcMedicalState.Diagnosing)
            {
                if (!m_CSMedicalCheck.pePatient.ContainsOperator(Operator))
                {
                    SetPosition(m_CSMedicalCheck.resultTrans[1].position);
                    SetRotation(m_CSMedicalCheck.resultTrans[1].rotation);
                    m_CSMedicalCheck.pePatient.StartOperate(Operator, EOperationMask.Lay);
                    SetPosition(m_CSMedicalCheck.resultTrans[0].position);
                }
                return BehaveResult.Running;
            }

            if (NpcMedicalState == ENpcMedicalState.Cure)
            {
                m_CSMedicalCheck.pePatient.StopOperate(Operator, EOperationMask.Lay);
                return BehaveResult.Success;
            }


            CSMedicalCheck _CSMedicalCheck = CSMain.FindCheckMachine(entity);
            if (_CSMedicalCheck == null)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
                return BehaveResult.Failure;
            }

            if (_CSMedicalCheck != null && NpcMedicalState == ENpcMedicalState.Diagnosing && m_Roate != _CSMedicalCheck.resultTrans[1].rotation.eulerAngles.y)//
            {
                _CSMedicalCheck.pePatient.StopOperate(Operator, EOperationMask.Lay);
                if (!IsMotionRunning(PEActionType.Cure))
                {
                    _CSMedicalCheck.pePatient.StartOperate(Operator, EOperationMask.Lay);
                    m_Roate = _CSMedicalCheck.resultTrans[1].rotation.eulerAngles.y;
                }

            }

            if (!HasReached && Stucking(5.0f))
            {
                StopMove();
                HasReached = true;

                if (m_CSMedicalCheck.pePatient.ContainsOperator(Operator))
                    m_CSMedicalCheck.pePatient.StopOperate(Operator, EOperationMask.Lay);
            }

            if (!HasReached)
            {
                if (ReachToPostion(m_Moveposition, SpeedState.Walk))
                {
                    StopMove();
                    HasReached = true;

                    if (m_CSMedicalCheck.pePatient.ContainsOperator(Operator))
                        m_CSMedicalCheck.pePatient.StopOperate(Operator, EOperationMask.Lay);
                }
            }
            return BehaveResult.Running;
        }


    }

    [BehaveAction(typeof(NpcMedicalTreat), "NpcMedicalTreat")]
    public class NpcMedicalTreat : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string WaitAnim;
            [BehaveAttribute]
            public float WaitTime;
        }

        Data m_Data;

        bool IsReadyTreat;
        bool HasReached;
        CSMedicalTreat m_CSMedicalTreat;
        Vector3 m_Moveposition;
        float m_Roate;
        //float m_StartTime = 0.0f;
        //PEPatients m_pePatitents;

        float mWaitingTime = 0.0f;
        Vector3 m_WaitingPos;
        Vector3 GetPosition(Vector3 pos)
        {
            return PEUtil.GetRandomPosition(pos, 10.0f, 10.0f) + Vector3.up * 2.0f;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (!IsNeedMedicine)
                return BehaveResult.Failure;

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Cure))
                return BehaveResult.Failure;

            if (!NpcThinkDb.CanDo(entity, EThinkingType.Cure))
                return BehaveResult.Failure;

            if (NpcMedicalState == ENpcMedicalState.SearchHospital)
                return BehaveResult.Success;

            if (NpcMedicalState != ENpcMedicalState.Treating)
            {
                if (NpcMedicalState != ENpcMedicalState.SearchTreat)
                    return BehaveResult.Failure;
            }

            m_CSMedicalTreat = CSMain.FindMedicalTreat(out IsReadyTreat, entity);
            if (m_CSMedicalTreat == null || m_CSMedicalTreat.resultTrans == null || !m_CSMedicalTreat.IsDoctorReady() || !m_CSMedicalTreat.IsMedicineReady())
            {
                mWaitingTime = Time.time;
                SetMedicineSate(ENpcMedicalState.WaitForTreat);
                return BehaveResult.Success;
            }


            if (IsReadyTreat && Sleep != null && Sleep.ContainsOperator(Operator))
                Sleep.StopOperate(Operator, EOperationMask.Sleep);

            m_Moveposition = m_CSMedicalTreat.resultTrans[1].position;
            m_Roate = m_CSMedicalTreat.resultTrans[1].rotation.eulerAngles.y;
            //m_pePatitents = m_CSMedicalTreat.pePatient;

            if (NpcMedicalState == ENpcMedicalState.Treating)
            {
                if (!CSMain.TryGetTreat(entity))
                    return BehaveResult.Failure;

                SetPosition(m_CSMedicalTreat.resultTrans[1].position);
                SetRotation(m_CSMedicalTreat.resultTrans[1].rotation);
                m_CSMedicalTreat.pePatient.StartOperate(Operator, EOperationMask.Lay);
                SetPosition(m_CSMedicalTreat.resultTrans[0].position);
                HasReached = true;
                return BehaveResult.Success;
            }

            if (!IsReadyTreat)
            {
                mWaitingTime = Time.time;
                SetMedicineSate(ENpcMedicalState.WaitForTreat);
            }

            HasReached = false;
            //m_StartTime = Time.time;
            return BehaveResult.Success;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcMedicalTreat);
            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (!IsNeedMedicine)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Cure))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                if (Sleep != null && Sleep.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Sleep))
                    Sleep.StopOperate(Operator, EOperationMask.Sleep);

                return BehaveResult.Failure;
            }

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Cure) && m_CSMedicalTreat != null)
            {
                if (m_CSMedicalTreat.pePatient.ContainsOperator(Operator))
                    m_CSMedicalTreat.pePatient.StopOperate(Operator, EOperationMask.Lay);

                CSMain.KickOutFromHospital(entity);
                return BehaveResult.Failure;
            }

            if (NpcMedicalState == ENpcMedicalState.Cure || NpcMedicalState == ENpcMedicalState.SearchHospital || NpcMedicalState == ENpcMedicalState.SearchDiagnos)
            {
                if (m_CSMedicalTreat != null && m_CSMedicalTreat.pePatient.ContainsOperator(Operator))
                    m_CSMedicalTreat.pePatient.StopOperate(Operator, EOperationMask.Lay);

                return BehaveResult.Success;
            }

            if (NpcMedicalState != ENpcMedicalState.Treating && !NpcThinkDb.CanDoing(entity, EThinkingType.Cure))
            {
                //CSMain.KickOutFromHospital(entity);
                return BehaveResult.Failure;
            }

            if (NpcMedicalState == ENpcMedicalState.WaitForTreat)
            {
                if (Time.time - mWaitingTime >= m_Data.WaitTime)
                {
                    //NpcCanWalkPos(Creater.Assembly.Position,Creater.Assembly.Radius,out m_WaitingPos);
                    SetMedicineSate(ENpcMedicalState.SearchTreat);
                    return BehaveResult.Failure;
                }

                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Cure))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                if (Sleep != null && !Sleep.ContainsOperator(Operator))
                {
                    if (!Sleep.CanOperate(transform))
                    {
                        MoveToPosition(Sleep.Trans.position, SpeedState.Walk);
                        if (Stucking(3.0f))
                            SetPosition(Sleep.Trans.position);
                    }
                    else
                    {
                        PESleep sleep = Sleep as PESleep;
                        if (null != sleep)
                        {
                            PEActionParamVQNS param = PEActionParamVQNS.param;
                            param.vec = sleep.transform.position;
                            param.q = sleep.transform.rotation;
                            param.n = 0;
                            param.str = m_Data.WaitAnim;
                            sleep.StartOperate(Operator, EOperationMask.Sleep, param);
                        }
                    }
                }

                if (Sleep != null && Sleep.ContainsOperator(Operator) && !IsMotionRunning(PEActionType.Sleep))
                {
                    Sleep.StopOperate(Operator, EOperationMask.Sleep);
                }

                return BehaveResult.Running;
            }

            if (HasReached && NpcMedicalState == ENpcMedicalState.SearchTreat)
            {
                if (!CSMain.TryGetTreat(entity))
                {
                    mWaitingTime = Time.time;
                    SetMedicineSate(ENpcMedicalState.WaitForTreat);
                    HasReached = false;
                    return BehaveResult.Running;
                }

                if (m_CSMedicalTreat.pePatient.CanOperateMask(EOperationMask.Lay) && !m_CSMedicalTreat.pePatient.ContainsOperator(Operator))
                {
                    SetPosition(m_CSMedicalTreat.resultTrans[1].position);
                    SetRotation(m_CSMedicalTreat.resultTrans[1].rotation);
                    m_CSMedicalTreat.pePatient.StartOperate(Operator, EOperationMask.Lay);
                    SetPosition(m_CSMedicalTreat.resultTrans[0].position);
                }
            }

            if (NpcMedicalState == ENpcMedicalState.Treating)
            {
                if (m_CSMedicalTreat.pePatient.CanOperateMask(EOperationMask.Lay) && !m_CSMedicalTreat.pePatient.ContainsOperator(Operator))
                {
                    SetPosition(m_CSMedicalTreat.resultTrans[1].position);
                    SetRotation(m_CSMedicalTreat.resultTrans[1].rotation);
                    m_CSMedicalTreat.pePatient.StartOperate(Operator, EOperationMask.Lay);
                    SetPosition(m_CSMedicalTreat.resultTrans[0].position);
                }
            }

            if (NpcMedicalState == ENpcMedicalState.Cure)
            {
                m_CSMedicalTreat.pePatient.StopOperate(Operator, EOperationMask.Lay);
                return BehaveResult.Success;
            }

            CSMedicalTreat _CSMedicalTreat = CSMain.FindTreatMachine(entity);
            if (_CSMedicalTreat == null)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                SetMedicineSate(ENpcMedicalState.WaitForTreat);
                return BehaveResult.Failure;
            }

            if (_CSMedicalTreat != null && m_Roate != _CSMedicalTreat.resultTrans[1].rotation.eulerAngles.y) //&& NpcMedicalState == ENpcMedicalState.Treating
            {
                _CSMedicalTreat.pePatient.StopOperate(Operator, EOperationMask.Lay);
                if (!IsMotionRunning(PEActionType.Cure))
                {
                    _CSMedicalTreat.pePatient.StartOperate(Operator, EOperationMask.Lay);
                    m_Roate = _CSMedicalTreat.resultTrans[1].rotation.eulerAngles.y;
                }
            }

            if (!HasReached)
            {
                if (ReachToPostion(m_Moveposition, SpeedState.Walk) || Stucking(5.0f))
                {
                    StopMove();
                    HasReached = true;

                    if (m_CSMedicalTreat.pePatient.ContainsOperator(Operator))
                        m_CSMedicalTreat.pePatient.StopOperate(Operator, EOperationMask.Lay);
                }
            }


            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(NpcInHospital), "NpcInHospital")]
    public class NpcInHospital : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string WaitAnim;
            [BehaveAttribute]
            public float WaitTime;

        }

        Data m_Data;

        bool IsReadyTent;
        bool HasReached;
        bool HasLay;
        //bool IsInHospital;
        CSMedicalTent m_CSMedicalTent;
        Vector3 m_TentPos;
        //Vector3 m_Moveposition;
        Sickbed m_sickbed;
        float m_Roate;

        float m_waitingTime;
        Vector3 m_WaitingPos;
        Vector3 GetPosition(Vector3 pos)
        {
            return PEUtil.GetRandomPositionOnGroundForWander(pos, 15.0f, 6.0f) + Vector3.up * 2.0f;
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (!IsNeedMedicine)
                return BehaveResult.Failure;

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Cure))
                return BehaveResult.Failure;

            if (!NpcThinkDb.CanDo(entity, EThinkingType.Cure))
                return BehaveResult.Failure;

            if (NpcMedicalState == ENpcMedicalState.Cure)
            {
                //SetMedicineSate(ENpcMedicalState.None);
                return BehaveResult.Success;
            }

            if (NpcMedicalState != ENpcMedicalState.SearchHospital)
                return BehaveResult.Failure;

            m_CSMedicalTent = CSMain.FindMedicalTent(out IsReadyTent, entity, out m_sickbed);
            if (m_CSMedicalTent == null || m_sickbed == null || m_sickbed.bedLay == null || m_sickbed.bedLay.m_StandTrans == null || !m_CSMedicalTent.IsDoctorReady())
            {
                m_waitingTime = Time.time;
                SetMedicineSate(ENpcMedicalState.WaitForTent);
                return BehaveResult.Success;
            }

            if (IsReadyTent && Sleep != null && Sleep.ContainsOperator(Operator))
                Sleep.StopOperate(Operator, EOperationMask.Sleep);

            //m_Moveposition = m_sickbed.bedLay.m_StandTrans.position;
            m_Roate = m_CSMedicalTent.workTrans[0].rotation.eulerAngles.y;

            if (NpcMedicalState == ENpcMedicalState.In_Hospital)
            {
                if (!CSMain.TryGetTent(entity))
                    return BehaveResult.Failure;

                SetPosition(m_sickbed.bedLay.m_StandTrans.position);
                SetRotation(m_sickbed.bedLay.m_StandTrans.rotation);
                m_sickbed.bedLay.StartOperate(Operator, EOperationMask.Lay);
                HasReached = true;
                return BehaveResult.Success;
            }

            if (!IsReadyTent)
            {
                m_waitingTime = Time.time;
                SetMedicineSate(ENpcMedicalState.WaitForTent);
            }

            HasReached = false;
            return BehaveResult.Success;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcInHospital);
            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (!IsNeedMedicine)
                return BehaveResult.Failure;

            if (!NpcTypeDb.CanRun(NpcCmdId, ENpcControlType.Cure) && m_sickbed != null)
            {
                if (m_sickbed.bedLay.ContainsOperator(Operator))
                    m_sickbed.bedLay.StopOperate(Operator, EOperationMask.Lay);

                CSMain.KickOutFromHospital(entity);
                return BehaveResult.Failure;
            }

            if (NpcMedicalState == ENpcMedicalState.Cure || NpcMedicalState == ENpcMedicalState.SearchTreat || NpcMedicalState == ENpcMedicalState.SearchDiagnos)
            {
                if (m_sickbed != null && m_sickbed.bedLay.ContainsOperator(Operator))
                    m_sickbed.bedLay.StopOperate(Operator, EOperationMask.Lay);

                return BehaveResult.Success;
            }

            if (NpcMedicalState != ENpcMedicalState.In_Hospital && !NpcThinkDb.CanDoing(entity, EThinkingType.Cure))
            {
                //CSMain.KickOutFromHospital(entity);
                return BehaveResult.Failure;
            }

            if (NpcMedicalState == ENpcMedicalState.WaitForTent)
            {
                if (Time.time - m_waitingTime >= m_Data.WaitTime)
                {
                    //NpcCanWalkPos(Creater.Assembly.Position,Creater.Assembly.Radius,out m_WaitingPos);
                    SetMedicineSate(ENpcMedicalState.SearchHospital);
                    return BehaveResult.Failure;
                }

                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator) && IsMotionRunning(PEActionType.Cure))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                if (Sleep != null && !Sleep.ContainsOperator(Operator))
                {
                    if (!Sleep.CanOperate(transform))
                    {
                        MoveToPosition(Sleep.Trans.position, SpeedState.Walk);
                        if (Stucking(3.0f))
                            SetPosition(Sleep.Trans.position);
                    }
                    else
                    {
                        PESleep sleep = Sleep as PESleep;
                        if (null != sleep)
                        {
                            PEActionParamVQNS param = PEActionParamVQNS.param;
                            param.vec = sleep.transform.position;
                            param.q = sleep.transform.rotation;
                            param.n = 0;
                            param.str = m_Data.WaitAnim;
                            sleep.StartOperate(Operator, EOperationMask.Sleep, param);
                        }
                    }
                }

                if (Sleep != null && Sleep.ContainsOperator(Operator) && !IsMotionRunning(PEActionType.Sleep))
                {
                    Sleep.StopOperate(Operator, EOperationMask.Sleep);
                }
                return BehaveResult.Running;
            }

            if (HasReached && NpcMedicalState == ENpcMedicalState.SearchHospital)
            {
                if (!CSMain.TryGetTent(entity))
                {
                    m_waitingTime = Time.time;
                    SetMedicineSate(ENpcMedicalState.WaitForTent);
                    HasReached = false;
                    return BehaveResult.Running;
                }

                if (m_sickbed != null && m_sickbed.bedLay.CanOperateMask(EOperationMask.Lay) && !m_sickbed.bedLay.ContainsOperator(Operator))
                {
                    SetPosition(m_sickbed.bedLay.m_StandTrans.position);
                    SetRotation(m_sickbed.bedLay.m_StandTrans.rotation);
                    m_sickbed.bedLay.StartOperate(Operator, EOperationMask.Lay);
                }
            }

            if (NpcMedicalState == ENpcMedicalState.In_Hospital)
            {

                if (m_sickbed != null && m_sickbed.bedLay.CanOperateMask(EOperationMask.Lay) && !m_sickbed.bedLay.ContainsOperator(Operator))
                {
                    SetPosition(m_sickbed.bedLay.m_StandTrans.position);
                    SetRotation(m_sickbed.bedLay.m_StandTrans.rotation);
                    m_sickbed.bedLay.StartOperate(Operator, EOperationMask.Lay);
                }
                return BehaveResult.Running;
            }


            CSMedicalTent _CSMedicalTent = CSMain.FindTentMachine(entity);
            if (_CSMedicalTent == null)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Lay);

                SetMedicineSate(ENpcMedicalState.WaitForTent);
                return BehaveResult.Failure;
            }

            if (m_sickbed != null && m_Roate != _CSMedicalTent.workTrans[0].rotation.eulerAngles.y && NpcMedicalState == ENpcMedicalState.In_Hospital)
            {
                m_sickbed.bedLay.StopOperate(Operator, EOperationMask.Lay);
                if (!IsMotionRunning(PEActionType.Cure))
                {
                    m_sickbed.bedLay.StartOperate(Operator, EOperationMask.Lay);
                    m_Roate = _CSMedicalTent.workTrans[0].rotation.eulerAngles.y;
                    m_sickbed.ReleaseMachine();
                }

            }


            if (!HasReached)
            {
                if (ReachToPostion(_CSMedicalTent.workTrans[1].position, SpeedState.Walk) || Stucking(5.0f))
                {
                    StopMove();
                    HasReached = true;

                    if (m_sickbed != null && m_sickbed.bedLay != null && m_sickbed.bedLay.ContainsOperator(Operator))
                        m_sickbed.bedLay.StopOperate(Operator, EOperationMask.Lay);
                }
            }

            return BehaveResult.Running;
        }
    }

    //教练
    [BehaveAction(typeof(NpcBaseInstructor), "NpcBaseInstructor")]
    public class NpcBaseInstructor : BTNormal
    {
        Transform TeachTrans;
        IOperation mInstructor;
        Vector3 mDoorPos;
        float m_Roate;
        int mIndex = 0;
        void swichWorkTrans(int index)
        {
            TeachTrans = WorkEntity.workTrans[index];
            PETrainner trainer = Trainner as PETrainner;
            if (trainer != null)
            {
                mInstructor = trainer.Singles[index];
            }
        }

        void StarTrain()
        {
            StopMove();
            SetPosition(TeachTrans.position);
            SetRotation(TeachTrans.rotation);
            mInstructor.StartOperate(Operator, EOperationMask.Practice);
        }

        void EndTrain()
        {

        }

        float startEndActionTime = 0.0f;
        float endActionTime = 2.0f;
        BehaveResult EndOperate(IOperation _operation, IOperator _IOperator)
        {
            if ((_operation != null && _operation.ContainsOperator(_IOperator)) || IsMotionRunning(PEActionType.Operation))
            {
                if (startEndActionTime == 0)
                    startEndActionTime = Time.time;
            }

            if (Time.time - startEndActionTime < endActionTime)
            {
                if (_operation != null && _operation.ContainsOperator(_IOperator))
                    _operation.StopOperate(_IOperator, EOperationMask.Practice);
                else
                    if (_IOperator != null && !_IOperator.Equals(null) && _IOperator.Operate != null && !_IOperator.Equals(null)
                       && _IOperator.Operate.ContainsOperator(_IOperator))
                    {
                        _IOperator.Operate.StopOperate(_IOperator, EOperationMask.Practice);
                    }

                return BehaveResult.Running;
            }
            else
            {
                startEndActionTime = 0;
                return BehaveResult.Failure;
            }

        }

        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (NpcJob != ENpcJob.Trainer)
                return BehaveResult.Failure;

            if (NpcTrainerType != ETrainerType.Instructor)
                return BehaveResult.Failure;

            if (WorkEntity == null || WorkEntity.workTrans == null)
                return BehaveResult.Failure;

            if (Trainner == null)
                return BehaveResult.Failure;

            mIndex = (int)NpcTrainingType;
            swichWorkTrans(mIndex);
            if (TeachTrans == null || mInstructor == null)
                return BehaveResult.Failure;

            if (!IsNpcTrainning)
                return BehaveResult.Failure;

            mDoorPos = WorkEntity.workTrans[4].position;
            m_Roate = WorkEntity.workTrans[4].rotation.eulerAngles.y;
            SetNpcState(ENpcState.Prepare);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcBaseJobTrain_Instructor);
            //职业切换中断行为
            if (!IsNpcBase || NpcJob != ENpcJob.Trainer)
            {
                return EndOperate(mInstructor, Operator);
            }

            //教练变学员切换
            if (NpcTrainerType != ETrainerType.Instructor)
            {
                return EndOperate(mInstructor, Operator);
            }

            //机器被收回或者电厂没电
            if (WorkEntity == null || WorkEntity.workTrans == null)
            {
                return EndOperate(mInstructor, Operator);
            }

            if (TeachTrans == null || Trainner == null || TeachTrans == null || mInstructor == null)
            {
                return EndOperate(mInstructor, Operator);
            }




            //训练所方向切换
            if (m_Roate != WorkEntity.workTrans[4].rotation.eulerAngles.y)
            {
                return EndOperate(mInstructor, Operator);
            }

            //训练正常结束
            if (!IsNpcTrainning)
            {
                return EndOperate(mInstructor, Operator);
            }

            if (!mInstructor.ContainsOperator(Operator))
            {
                if (!Trainner.CanOperate(transform))
                {
                    MoveToPosition(mDoorPos, SpeedState.Run);
                    if (Stucking() && PEUtil.CheckErrorPos(mDoorPos))
                    {
                        SetPosition(mDoorPos);
                    }

                    if (IsReached(position, mDoorPos, false))
                        SetPosition(mDoorPos);

                }
                else
                {
                    StopMove();
                    SetPosition(TeachTrans.position);
                    SetRotation(TeachTrans.rotation);
                    mInstructor.StartOperate(Operator, EOperationMask.Practice);
                    SetNpcState(ENpcState.Work);
                    //do action
                }
            }
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (Creater != null && Creater.Assembly != null && NpcJobStae == ENpcState.Work)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Practice);
            }

            SetNpcState(ENpcState.UnKnown);
        }
    }

    //学员
    [BehaveAction(typeof(NpcBaseTrainee), "NpcBaseTrainee")]
    public class NpcBaseTrainee : BTNormal
    {
        Transform LearnTrans;
        IOperation mTrainee;
        int mIndex = 0;
        float m_Roate;
        Vector3 mDoorPos;
        void swichWorkTrans(int index)
        {
            LearnTrans = WorkEntity.workTrans[index];
            PETrainner trainer = Trainner as PETrainner;
            if (trainer != null)
            {
                mTrainee = trainer.Singles[index];
            }
        }

        float startEndActionTime = 0.0f;
        float endActionTime = 2.0f;
        BehaveResult EndOperate(IOperation _operation, IOperator _IOperator)
        {
            if ((_operation != null && _operation.ContainsOperator(_IOperator)) || IsMotionRunning(PEActionType.Operation))
            {
                if (startEndActionTime == 0)
                    startEndActionTime = Time.time;
            }

            if (Time.time - startEndActionTime < endActionTime)
            {
                if (_operation != null && _operation.ContainsOperator(_IOperator))
                    _operation.StopOperate(_IOperator, EOperationMask.Practice);
                else
                    if (_IOperator != null && !_IOperator.Equals(null) && _IOperator.Operate != null && !_IOperator.Equals(null)
                       && _IOperator.Operate.ContainsOperator(_IOperator))
                    {
                        _IOperator.Operate.StopOperate(_IOperator, EOperationMask.Practice);
                    }

                return BehaveResult.Running;
            }
            else
            {
                startEndActionTime = 0;
                return BehaveResult.Failure;
            }

        }

        BehaveResult Init(Tree sender)
        {

            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (NpcJob != ENpcJob.Trainer)
                return BehaveResult.Failure;

            if (NpcTrainerType != ETrainerType.Trainee)
                return BehaveResult.Failure;

            if (WorkEntity == null || WorkEntity.workTrans == null)
                return BehaveResult.Failure;

            if (Trainner == null)
                return BehaveResult.Failure;

            mIndex = (int)NpcTrainingType + 2;
            swichWorkTrans(mIndex);
            if (LearnTrans == null || mTrainee == null)
                return BehaveResult.Failure;

            if (!IsNpcTrainning)
                return BehaveResult.Failure;

            mDoorPos = WorkEntity.workTrans[4].position;
            m_Roate = WorkEntity.workTrans[4].rotation.eulerAngles.y;
            SetNpcState(ENpcState.Prepare);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            SetNpcAiType(ENpcAiType.NpcBaseJobTrain_Trainee);
            //职业切换
            if (!IsNpcBase || NpcJob != ENpcJob.Trainer)
            {
                return EndOperate(mTrainee, Operator);
            }

            //学员变教练切换
            if (NpcTrainerType != ETrainerType.Trainee)
            {
                return EndOperate(mTrainee, Operator);
            }

            //训练所被收回或者电厂没电
            if (WorkEntity == null || WorkEntity.workTrans == null || Trainner == null || LearnTrans == null || mTrainee == null)
            {
                return EndOperate(mTrainee, Operator);
            }


            //训练所方向切换
            if (m_Roate != WorkEntity.workTrans[4].rotation.eulerAngles.y)
            {
                return EndOperate(mTrainee, Operator);
            }

            //正常结束
            if (!IsNpcTrainning)
            {
                return EndOperate(mTrainee, Operator);
            }

            if (!mTrainee.ContainsOperator(Operator))
            {
                if (!Trainner.CanOperate(transform))
                {
                    MoveToPosition(mDoorPos, SpeedState.Run);
                    if (Stucking() && PEUtil.CheckErrorPos(mDoorPos))
                    {
                        SetPosition(mDoorPos);
                    }

                    if (IsReached(position, mDoorPos, false))
                        SetPosition(mDoorPos);

                }
                else
                {
                    StopMove();
                    SetPosition(LearnTrans.position);
                    SetRotation(LearnTrans.rotation);
                    mTrainee.StartOperate(Operator, EOperationMask.Practice);
                    SetNpcState(ENpcState.Work);
                    //do action
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (Creater != null && Creater.Assembly != null && NpcJobStae == ENpcState.Work)
            {
                if (Operator != null && Operator.Operate != null && Operator.Operate.ContainsOperator(Operator))
                    Operator.Operate.StopOperate(Operator, EOperationMask.Practice);

            }
            SetNpcState(ENpcState.UnKnown);

        }
    }

    [BehaveAction(typeof(BTCheckLine), "CheckLine")]
    public class BTCheckLine : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public int LineType;
        }
        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (entity == null || entity.NpcCmpt == null || !IsNpcBase)
                return BehaveResult.Failure;

            if (hasAnyRequest)
                return BehaveResult.Failure;

            if (entity.NpcCmpt.lineType == (ELineType)m_Data.LineType)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTCSmoveToPoint), "CSmoveToPoint")]
    public class BTCSmoveToPoint : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public int LineType;
        }
        Data m_Data;
        ChatTeamDb m_chatTeamDb;
        BehaveResult Init(Tree sender)
        {
            if (!IsNpcBase)
                return BehaveResult.Failure;

            m_chatTeamDb = TeamDb.LoadchatTeamDb(entity);
            if (m_chatTeamDb == null)
                return BehaveResult.Failure;

            if (m_chatTeamDb.TRMovePos == Vector3.zero)
                return BehaveResult.Failure;

            if (entity.NpcCmpt.lineType == ELineType.IDLE)
                return BehaveResult.Failure;

            return BehaveResult.Running;
        }
        BehaveResult Tick(Tree sender)
        {
            //			if (!GetData<Data>(sender, ref m_Data))
            //				return BehaveResult.Failure;

            if (IsReached(position, m_chatTeamDb.TRMovePos, false))
            {
                SetPosition(m_chatTeamDb.TRMovePos);
                StopMove();
                return BehaveResult.Success;
            }

            if (Stucking())
                SetPosition(m_chatTeamDb.TRMovePos);

            if (entity.NpcCmpt.lineType == ELineType.IDLE)
                return BehaveResult.Failure;

            MoveToPosition(m_chatTeamDb.TRMovePos, SpeedState.Walk);
            return BehaveResult.Running;
        }
    }

    [BehaveAction(typeof(BTSitRest), "SitRest")]
    public class BTSitRest : BTNormal
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

            public ChatTeamDb m_chatTeamDb;
        }

        Data m_Data;

        Enemy m_Escape;
        Enemy m_Threat;


        bool DoingRelax()
        {
            for (int i = 0; i < m_Data.relax.Length; i++)
            {
                if (entity.animCmpt != null && entity.animCmpt.animator != null
                    && entity.animCmpt.ContainsParameter(m_Data.relax[i])
                    && entity.animCmpt.animator.GetBool(m_Data.relax[i]))
                    return true;
            }
            return false;
        }

        void EndRelax()
        {
            for (int i = 0; i < m_Data.relax.Length; i++)
            {
                if (GetBool(m_Data.relax[i]))
                    SetBool(m_Data.relax[i], false);
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase)
                return BehaveResult.Failure;

            if (m_Data.m_chatTeamDb == null)
                m_Data.m_chatTeamDb = TeamDb.LoadchatTeamDb(entity);

            if (m_Data.m_chatTeamDb == null)
                return BehaveResult.Failure;

            if (hasAnyRequest || NpcJob != ENpcJob.Resident)
                return BehaveResult.Failure;

            float r0 = PEUtil.Magnitude(position, Creater.Assembly.Position);
            if (r0 > Creater.Assembly.Radius)
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            m_Data.m_StartRestTime = Time.time;
            m_Data.m_StartIdleTime = Time.time;
            m_Data.m_CurrentIdleTime = Random.Range(m_Data.minTime, m_Data.maxTime);

            StopMove();
            SetBool(m_Data.idle, true);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (hasAnyRequest || !IsNpcBase || !Enemy.IsNullOrInvalid(attackEnemy) || NpcJob != ENpcJob.Resident)
            {
                if (DoingRelax())
                {
                    EndRelax();
                    return BehaveResult.Running;
                }
                else
                    return BehaveResult.Failure;
            }

            if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
                return BehaveResult.Running;

            float r0 = PEUtil.Magnitude(position, Creater.Assembly.Position);
            if (r0 > Creater.Assembly.Radius)
            {
                if (DoingRelax())
                {
                    EndRelax();
                    return BehaveResult.Running;
                }
                else
                    return BehaveResult.Failure;
            }

            if (entity.NpcCmpt.lineType != ELineType.TeamChat)
            {
                if (DoingRelax())
                {
                    EndRelax();
                    return BehaveResult.Running;
                }
                else
                    return BehaveResult.Success;
            }

            FaceDirection(m_Data.m_chatTeamDb.CenterPos - position);
            if (Time.time - m_Data.m_StartRestTime > m_Data.relaxTime)
            {
                m_Data.m_StartRestTime = Time.time;
                if (!GetBool("BehaveWaiting") && !GetBool("Leisureing") && Random.value < m_Data.relaxProb)
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

            if (m_Data.m_StartIdleTime > PETools.PEMath.Epsilon)
            {
                m_Data.m_StartRestTime = 0.0f;
                m_Data.m_StartIdleTime = 0.0f;
                m_Data.m_CurrentIdleTime = 0.0f;

                if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
                    SetBool("Interrupt", true);

                EndRelax();
                SetBool(m_Data.idle, false);
                m_Data.m_chatTeamDb = null;
            }
        }
    }

    [BehaveAction(typeof(BTCsCallBack), "CsCallBack")]
    public class BTCsCallBack : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float walkTime;


            public float startBackTime;
            public Vector3 mDirPos;
        }

        Data m_Data;
        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (NpcJob == ENpcJob.Processor)
                return BehaveResult.Failure;

            float r0 = PEUtil.Magnitude(position, Creater.Assembly.Position);
            if (r0 <= Creater.Assembly.Radius)
                return BehaveResult.Failure;

            if (!NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mDirPos))
            {
                m_Data.mDirPos = Creater.Assembly.Position;
            }

            m_Data.startBackTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            float r0 = PEUtil.Magnitude(position, Creater.Assembly.Position);
            if (r0 <= Creater.Assembly.Radius)
                return BehaveResult.Failure;

            if (entity.NpcCmpt != null)
                entity.NpcCmpt.SetCsBacking(true);

            if (Stucking() || Time.time - m_Data.startBackTime > m_Data.walkTime)
            {
                m_Data.startBackTime = Time.time;
                if (NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mDirPos))
                {
                    SetPosition(m_Data.mDirPos);
                    return BehaveResult.Failure;
                }

                float r1 = PEUtil.Magnitude(position, Creater.Assembly.Position);
                if (r1 > Creater.Assembly.Radius)
                {
                    SetPosition(Creater.Assembly.Position);
                }

                return BehaveResult.Running;
            }

            MoveToPosition(m_Data.mDirPos, SpeedState.Run);
            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (entity.NpcCmpt != null)
                entity.NpcCmpt.SetCsBacking(false);
        }
    }

    [BehaveAction(typeof(BTCsMoveAway), "CsMoveAway")]
    public class BTCsMoveAway : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float awayTime;
            [BehaveAttribute]
            public float minRadiu = 0;
            [BehaveAttribute]
            public float maxRadiu = 0;


            public Vector3 mWanderWalkPos;
            public float startWanderTime;
            //public Vector3 mDirPos;
        }

        Data m_Data;
        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Creater == null || Creater.Assembly == null)
                return BehaveResult.Failure;

            if (!NpcCanWalkPos(Creater.Assembly.Position, Creater.Assembly.Radius, out m_Data.mWanderWalkPos))
                return BehaveResult.Failure;

            m_Data.startWanderTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Creater == null || Creater.Assembly == null)
                return BehaveResult.Failure;

            if (hasAnyRequest || !Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            //			if (Stucking())
            //				SetPosition(m_Data.mWanderWalkPos);

            bool _IsForwardBlock = PETools.PEUtil.IsForwardBlock(entity, entity.peTrans.forward, 2.0f);
            if (_IsForwardBlock || Time.time - m_Data.startWanderTime > m_Data.awayTime)
            {
                StopMove();
                return BehaveResult.Success;
            }

            if (IsReached(position, m_Data.mWanderWalkPos, true)) // || Stucking()
            {
                StopMove();
                return BehaveResult.Success;
            }


            bool _IsUnderBlock = PETools.PEUtil.IsUnderBlock(entity);
            if (_IsUnderBlock)
                MoveDirection(m_Data.mWanderWalkPos - position, SpeedState.Run);
            else
                MoveToPosition(m_Data.mWanderWalkPos, SpeedState.Run);

            return BehaveResult.Running;
        }

    }

    [BehaveAction(typeof(BTCsWanderIdle), "CsWanderIdle")]
    public class BTCsWanderIdle : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float WanderTime;


            //public Vector3 mWanderWalkPos;
            //public float startWanderTime;
            //public Vector3 mDirPos;
        }

        Data m_Data;
        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;


            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            return BehaveResult.Running;
        }

    }

    [BehaveAction(typeof(BTCsNoIdleRun), "CsNoIdleRun")]
    public class BTCsNoIdleRun : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float RunRadius;
        }
        Data m_Data;
        FindHidePos mFind;
        float startRunTime = 0.0f;
        float CHECK_TIME = 5.0f;

        float startHideTime = 0.0f;
        float CHECK_Hide_TIME = 1.0f;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!IsNpcBase || entity.NpcCmpt.csCanIdle)
                return BehaveResult.Failure;

            mFind = new FindHidePos(m_Data.RunRadius, false, 15.0f);
            startRunTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (entity.NpcCmpt.csCanIdle)
                return BehaveResult.Failure;

            if (Time.time - startRunTime > CHECK_TIME)
            {
                entity.NpcCmpt.SetCanIdle(true);
                return BehaveResult.Failure;
            }

            if (Time.time - startHideTime > CHECK_Hide_TIME)
            {
                Vector3 dir = mFind.GetHideDir(PeCreature.Instance.mainPlayer.peTrans.position, position, Enemies);
                if (mFind.bNeedHide)
                {
                    Vector3 dirPos = position + dir.normalized * 15.0f;
                    MoveToPosition(dirPos, SpeedState.Run);
                }
                else
                {
                    if (entity.target.beSkillTarget)
                    {
                        MoveDirection(transform.right, SpeedState.Run);
                    }
                    else
                    {
                        StopMove();
                        FaceDirection(PeCreature.Instance.mainPlayer.peTrans.position - position);
                    }

                }

                startHideTime = Time.time;
            }

            return BehaveResult.Running;
        }
    }


}
