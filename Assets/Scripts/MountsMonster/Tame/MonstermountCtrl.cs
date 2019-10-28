using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pathea
{
    public enum ECtrlType
    {
        Free,
        Taming,
        Mount,
        Wait
    }

    public class MonstermountCtrl : PeCmpt
    {
        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;

        private const int TameCampID = 28;//无伤害目标无威胁
        private const int TameDefaultPlayerID = 1;//player
        private const int TameDamageID = 28;//无伤害目标无威胁

        private const float m_MoveAttackSpeed = 4f;
        private const float m_SprintAttackSpeed = 6f;

        private const float RestoreWaitTime = 20.0f;

        [ContextMenu("StopAI")]
        public void StopAI()
        {
            if (Entity != null && Entity.BehaveCmpt != null)
            {
                Entity.BehaveCmpt.Stop();
            }
        }

        [ContextMenu("ExcuteAI")]
        void ExcuteAI()
        {
            if (Entity != null && Entity.BehaveCmpt != null)
            {
                Entity.BehaveCmpt.Excute();
            }
        }

        private Motion_Move_Motor m_Move;
        //auto move
        private Vector3 m_AutoMoveDir = Vector3.zero;
        private SpeedState m_AutoSpeed = SpeedState.Walk;
        private float m_AutoTime0 = 0.0f;//start
        private float m_AutoTime1 = 0.0f;//end
        private int m_TameSkill = 0;//
        //palyer control
        private PeEntity m_Master;
        private Vector3 m_MoveDir = Vector3.zero;
        private bool m_Init = false;
        private bool m_MoveWalk = false;

        public ECtrlType ctrlType { get; private set; }
        public delegate void AccomplishMove();
        private AccomplishMove m_CallBack;
        //force data
        public ForceData m_PlayerForceDb = new ForceData();
        public ForceData m_MountsForceDb = null;
        public BaseSkillData m_SkillData = new BaseSkillData();

        #region private fun
        /// <summary>
        /// 
        /// </summary>
        void UpdateMoveState()
        {
            if (ctrlType != ECtrlType.Mount || Entity.skEntity.IsSkillRunning())
                return;

            if (m_Move == null)
                return;

            //if(PeInput.Get(PeInput.LogicFunction.InteractWithItem) && null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())

            //计算移动方向
            Vector3 moveDirLocal = PeInput.GetAxisH() * Vector3.right + PeInput.GetAxisV() * Vector3.forward;
            m_MoveDir = Vector3.ProjectOnPlane(PETools.PEUtil.MainCamTransform.rotation * moveDirLocal, Vector3.up);

            //计算行走速度
            if (PeInput.Get(PeInput.LogicFunction.SwitchWalkRun))
                m_MoveWalk = !m_MoveWalk;
            SpeedState state = m_MoveWalk ? SpeedState.Walk : SpeedState.Run;
            if (PeInput.Get(PeInput.LogicFunction.Sprint) && Entity.GetAttribute(AttribType.SprintSpeed) > 0)
                state = SpeedState.Sprint;

            //移动
            m_Move.Move(m_MoveDir.normalized, state);


            //跳跃
            if ((m_SkillData.canSpace() || m_SkillData.canProunce()) && PeInput.Get(PeInput.LogicFunction.Jump))
            {
                Jump();
            }

            //左键攻击
            if (m_SkillData.canAttack() && PeInput.Get(PeInput.LogicFunction.Attack))
                AttackL();
            //右键攻击：还需要定义右键攻击逻辑键
            //if (PeInput.Get(PeInput.LogicFunction.Item_Use))
            //    AttackR();


        }

        /// <summary>
        /// 驯服过程中的摆动
        /// </summary>
        void UpdataTamingMove()
        {
            if (ctrlType != ECtrlType.Taming)
                return;

            if (m_AutoTime1 != -1)
            {
                //按照需求方向移动一段时间
                if (Time.time - m_AutoTime0 < m_AutoTime1)
                {
                    //移动
                    m_Move.Move(m_AutoMoveDir.normalized, m_AutoSpeed);
                }

                if (Time.time - m_AutoTime0 >= m_AutoTime1)
                {
                    m_Move.Stop();
                    m_AutoTime1 = -1;
                    if (m_CallBack != null)
                        m_CallBack();
                }
            }

            //按照需求到达指定地点

            //jump行为回调
            if (m_TameSkill != 0)
            {
                if (!Entity.skEntity.IsSkillRunning(m_TameSkill, true))
                {
                    m_TameSkill = 0;
                    if (m_CallBack != null)
                        m_CallBack();
                }
            }
        }

        /// <summary>
        /// 初始化坐骑数据（被驯服后数据）
        /// </summary>
        private bool InitMountData()
        {
            if (!Entity) return false;

            m_Move = Entity.motionMove as Motion_Move_Motor;
            m_Move.Stop();

            SetctrlType(ECtrlType.Mount);
            Pathea.FastTravelMgr.Instance.OnFastTravel += OnFastTravel;

            m_PlayerForceDb = new ForceData((int)m_Master.GetAttribute(AttribType.CampID), (int)m_Master.GetAttribute(AttribType.DamageID), (int)m_Master.GetAttribute(AttribType.DefaultPlayerID));

            if (m_MountsForceDb == null)
                m_MountsForceDb = new ForceData((int)Entity.GetAttribute(AttribType.CampID), (int)Entity.GetAttribute(AttribType.DamageID), (int)Entity.GetAttribute(AttribType.DefaultPlayerID));

            if (!m_SkillData.canUse())
                m_SkillData.Reset(MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_L),
                                  MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_Space),
                                   MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_pounce)
                                  );

            StartMountsForceDb();
            DispatchEvent(Entity);

            //lw:2017.3.13:读档后如果在晚上，怪物不能结束睡觉动作
            if (Entity.animCmpt != null)
                Entity.animCmpt.SetBool("Sleep", false);

            m_Init = true;
            return true;
        }

        /// <summary>
        /// 初始化驯服数据
        /// </summary>
        private void InitTameData(PeEntity master)
        {
            if (!Entity) return;

            m_Move = Entity.motionMove as Motion_Move_Motor;
            m_Master = master;
            SetctrlType(ECtrlType.Free);

            if (!m_SkillData.canUse())
                m_SkillData.Reset(MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_L),
                                  MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_Space),
                                   MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_pounce)
                                  );

            m_PlayerForceDb = new ForceData((int)m_Master.GetAttribute(AttribType.CampID), (int)m_Master.GetAttribute(AttribType.DamageID), (int)m_Master.GetAttribute(AttribType.DefaultPlayerID));
            if (m_MountsForceDb == null)
                m_MountsForceDb = new ForceData((int)Entity.GetAttribute(AttribType.CampID), (int)Entity.GetAttribute(AttribType.DamageID), (int)Entity.GetAttribute(AttribType.DefaultPlayerID));

        }

        /// <summary>
        /// 
        /// </summary>
        private void EndMount()
        {
            m_Move = null;
            m_Init = false;
            SetctrlType(ECtrlType.Free);
            if (Entity.target)
                Entity.target.ClearEnemy();

            Pathea.FastTravelMgr.Instance.OnFastTravel -= OnFastTravel;
            DelEvent(Entity);
        }

        /// <summary>
        /// 启动坐骑阵营数据(同玩家阵营相同)
        /// </summary>
        private void StartMountsForceDb()
        {
            if (Entity == null || m_PlayerForceDb == null) return;

            Entity.SetAttribute(AttribType.CampID, m_PlayerForceDb._campID);
            Entity.SetAttribute(AttribType.DamageID, m_PlayerForceDb._damageID);
            Entity.SetAttribute(AttribType.DefaultPlayerID, m_PlayerForceDb._defaultPlyerID);
        }

        private void StartTamingForceDb()
        {
            if (Entity == null) return;

            Entity.SetAttribute(AttribType.CampID, TameCampID);
            Entity.SetAttribute(AttribType.DamageID, TameDamageID);
            Entity.SetAttribute(AttribType.DefaultPlayerID, TameDefaultPlayerID);
        }
        /// <summary>
        /// 恢复怪物数据
        /// </summary>
        private void RestoreMonsterForceDb()
        {
            if (!Entity || m_MountsForceDb == null) return;

            Entity.SetAttribute(AttribType.CampID, m_MountsForceDb._campID);
            Entity.SetAttribute(AttribType.DamageID, m_MountsForceDb._damageID);
            Entity.SetAttribute(AttribType.DefaultPlayerID, m_MountsForceDb._defaultPlyerID);

            //清除仇恨列表
            Entity.target.ClearEnemy();

            m_MountsForceDb = null;
        }

        private SpeedState calculateSpeed(SpeedState speed)
        {
            switch (speed)
            {
                case SpeedState.Sprint:
                    if (Entity.GetAttribute(AttribType.SprintSpeed) > 0)
                        return SpeedState.Sprint;
                    else if (Entity.GetAttribute(AttribType.RunSpeed) > 0)
                        return SpeedState.Run;
                    else return SpeedState.Walk;
                case SpeedState.Run:
                    if (Entity.GetAttribute(AttribType.RunSpeed) > 0)
                        return SpeedState.Run;
                    else return SpeedState.Walk;

                default: return SpeedState.Walk;
            }


        }
        #endregion

        #region  event

        void OnAttack(SkillSystem.SkEntity skEntity, float damage)
        {
            PeEntity tarEntity = skEntity.GetComponent<PeEntity>();

            TransferHared(tarEntity, damage);
        }

        void OnDamage(SkillSystem.SkEntity entity, float damage)
        {
            if (null == Entity || null == entity)
                return;

            PeEntity peEntity = entity.GetComponent<PeEntity>();
            if (peEntity == Entity)
                return;

            TransferHared(peEntity, damage);
        }

        void OnSkillTarget(SkillSystem.SkEntity caster)
        {
            if (null == Entity || null == caster)
                return;

            PeEntity tarEntity = caster.GetComponent<PeEntity>();
            TransferHared(tarEntity, 10.0f);

        }
        private void DispatchEvent(PeEntity mount)
        {
            if (!mount || !mount.aliveEntity) return;

            mount.aliveEntity.attackEvent += OnAttack;
            mount.aliveEntity.onHpReduce += OnDamage;
            mount.aliveEntity.onSkillEvent += OnSkillTarget;
        }

        private void DelEvent(PeEntity mount)
        {
            if (!mount || !mount.aliveEntity) return;

            mount.aliveEntity.attackEvent -= OnAttack;
            mount.aliveEntity.onHpReduce -= OnDamage;
            mount.aliveEntity.onSkillEvent -= OnSkillTarget;
        }

        private void TransferHared(PeEntity targetentity, float damage)
        {
            float tansDis = targetentity.IsBoss ? 128f : 64f;
            int playerID = (int)Entity.GetAttribute(AttribType.DefaultPlayerID);
            List<PeEntity> entities = EntityMgr.Instance.GetEntities(Entity.position, tansDis, playerID, false, Entity);
            for (int i = 0; i < entities.Count; i++)
            {
                if (!entities[i].Equals(targetentity) && entities[i].target != null)
                    entities[i].target.TransferHatred(targetentity, damage);
            }
        }
        /// <summary>
        /// lw：恢复数据
        /// </summary>
        private void ResetMonster()
        {
            m_Move.Stop();
            m_Move.SetSpeed(0.0f);
            Entity.animCmpt.SetBool(m_SkillData.m_PounceAnim, false);
            m_Move.Move(Vector3.zero);
            m_Move.RotateTo(Vector3.zero);
        }

        public void OnFastTravel(Vector3 pos)
        {
            if (Entity == null || ctrlType != ECtrlType.Mount)
                return;

            Entity.peTrans.position = pos;
            SubscribeEndLoad();
        }

        void OnResponse(object sender, PeEvent.EventArg arg)
        {
            StartCoroutine(EmutiWait(arg));
        }
        #endregion

        #region public fun

        /// <summary>
        /// 配置驯服数据
        /// </summary>
        /// <param name="master"></param>
        public void InitTame(PeEntity master)
        {
            if (!Entity || !master) return;

            InitTameData(master);
        }

        /// <summary>
        /// 开启玩家控制
        /// </summary>
        public void StartPlayerCtrl(PeEntity master)
        {
            if (!Entity || !master) return;

            m_Master = master;
            Entity.BehaveCmpt.Stop();
            if (!m_Init) InitMountData();
            SetctrlType(ECtrlType.Mount);
        }

        public void LoadCtrl(PeEntity master, MousePickRides peride)
        {
            m_Master = master;
            StartCoroutine(EloadMount(peride));
        }


        /// <summary>
        /// 暂停玩家控制
        /// </summary>
        public void PausePlayerCtrl()
        {
            SetctrlType(ECtrlType.Wait);
        }

        /// <summary>
        /// 释放坐骑
        /// </summary>
        private void FreeMonster()
        {
            if (!Entity) return;

            if (m_SkillData != null)
                m_SkillData.Reset();

            StartCoroutine(ERestoreMonsterWait(Time.time));
            // RestoreMonsterForceDb();
            EndMount();
        }

        /// <summary>
        /// 怪物被驯服中
        /// </summary>
        public void Taming()
        {
            SetctrlType(ECtrlType.Taming);
            //启动驯服阵营
            StartTamingForceDb();
        }

        /// <summary>
        /// 驯服成功
        /// </summary>
        public void TameSucceed()
        {
            //暂停行为
            Entity.BehaveCmpt.Stop();
            //PeCreature.Instance.Add(Entity.Id);
            SetctrlType(ECtrlType.Mount);
            InitMountData();
            SubscribeEndLoad();
            //lw:完成 成就：怪物骑士

            SteamAchievementsSystem.Instance.OnGameStateChange(Eachievement.Mounts_Rider);
        }

        /// <summary>
        /// 驯服失败
        /// </summary>
        public void TameFailure()
        {
            //PeCreature.Instance.Remove(Entity.Id);
            SetctrlType(ECtrlType.Free);
            // StartCoroutine(RunAway(10.0f));

            //lw:释放数据前结束Coroutines
           
            StopAllCoroutines();
          
            UnsubscribeEndLoad();

            ResetMonster();

            FreeMonster();
            //RelationshipDataMgr.RemoveRalationship(m_Master.Id, Entity.ProtoID);
        }
        public void MoveDirtion(Vector3 dir, AccomplishMove callback, float time = 2.0f, SpeedState state = SpeedState.Walk)
        {
            if (dir == Vector3.zero)
                return;

            m_AutoMoveDir = dir;
            m_CallBack = callback;
            m_AutoTime0 = Time.time;
            m_AutoTime1 = time;
            m_AutoSpeed = calculateSpeed(state);
        }
        public void Forward(AccomplishMove callback, float time = 2.0f, SpeedState state = SpeedState.Walk)
        {
            Vector3 moveDirLocal = Vector3.forward;
            m_AutoMoveDir = Vector3.ProjectOnPlane(PETools.PEUtil.MainCamTransform.rotation * moveDirLocal, Vector3.up);
            m_CallBack = callback;
            m_AutoTime0 = Time.time;
            m_AutoTime1 = time;
            m_AutoSpeed = calculateSpeed(state);
        }
        public void Back(AccomplishMove callback, float time = 2.0f, SpeedState state = SpeedState.Walk)
        {
            Vector3 moveDirLocal = -Vector3.forward;
            m_AutoMoveDir = Vector3.ProjectOnPlane(PETools.PEUtil.MainCamTransform.rotation * moveDirLocal, Vector3.up);
            m_CallBack = callback;
            m_AutoTime0 = Time.time;
            m_AutoTime1 = time;
            m_AutoSpeed = calculateSpeed(state);
        }
        public void Right(AccomplishMove callback, float time = 2.0f, SpeedState state = SpeedState.Walk)
        {
            Vector3 moveDirLocal = Vector3.right;
            m_AutoMoveDir = Vector3.ProjectOnPlane(PETools.PEUtil.MainCamTransform.rotation * moveDirLocal, Vector3.up);
            m_CallBack = callback;
            m_AutoTime0 = Time.time;
            m_AutoTime1 = time;
            m_AutoSpeed = calculateSpeed(state);
        }
        public void Left(AccomplishMove callback, float time = 2.0f, SpeedState state = SpeedState.Walk)
        {
            Vector3 moveDirLocal = -Vector3.right;
            m_AutoMoveDir = Vector3.ProjectOnPlane(PETools.PEUtil.MainCamTransform.rotation * moveDirLocal, Vector3.up);
            m_CallBack = callback;
            m_AutoTime0 = Time.time;
            m_AutoTime1 = time;
            m_AutoSpeed = calculateSpeed(state);
        }
        public void Jump()
        {
            if (m_SkillData.canSpace())
            {
                //暂停移动
                m_Move.Stop();
                Entity.StartSkill(null, m_SkillData.m_SKillIDSpace);
            }
            else if (m_SkillData.canProunce() && m_Master)
            {
                m_Move.Stop();

                if (PeGameMgr.IsMulti)
                {
                    if (Entity != null && Entity.skEntity != null && Entity.skEntity._net != null && Entity.skEntity.IsController())
                    {
                        if (Entity.skEntity._net is AiNetwork)
                        {
                            (Entity.skEntity._net as AiNetwork).RequestSetBool(Animator.StringToHash(m_SkillData.m_PounceAnim), true);
                        }
                    }
                }

                Entity.animCmpt.SetBool(m_SkillData.m_PounceAnim, true);
                Entity.StartSkill(m_Master.skEntity, m_SkillData.m_Skillpounce);

                Vector3 targetPos = PETools.PEUtil.GetDirtionPostion(Entity.position, Entity.forward, 8.0f, 20.0f, 0f, 0f, 12.0f);
                if (targetPos != Vector3.zero)
                    StartCoroutine(Epounce(Time.time, targetPos));
            }
        }
        public void Jump(AccomplishMove callback)
        {
            int _rskill = MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_tame);
            if (_rskill == 0)
                return;

            //暂停移动
            m_Move.Stop();

            Entity.StartSkill(null, _rskill);
            m_TameSkill = _rskill;
            m_CallBack = callback;

        }

        public void HitFly(Vector3 forceDir, string trsStr = "Bip01 L Thigh", float forcePower = 750.0f)//Bip01 L Thigh,Bip01 R Thigh,Bip01 Spine3
        {
            if (m_Master == null || m_Master.skEntity == null || m_Master.motionBeat == null)
                return;
            //1:
            PEActionParamVFNS paramVFNS = PEActionParamVFNS.param;
            paramVFNS.vec = forceDir;
            paramVFNS.f = forcePower;
            paramVFNS.n = m_Master.Id;
            paramVFNS.str = trsStr;
            m_Master.motionMgr.DoAction(PEActionType.Wentfly, paramVFNS);

            //3:
            //if (hitColTran)
            //    m_Master.motionBeat.Beat(m_Master.skEntity, hitColTran, forceDir, forcePower);
        }

        public bool HasJump()
        {
            return MountsSkillDb.GetRandomSkill(Entity.ProtoID, MountsSkillKey.Mskill_tame) != 0;//m_SkillData != null && m_SkillData.canSpace();
        }
        public void AttackL()
        {
            if (m_SkillData.m_SkillL == 0) return;
            //暂停移动
            m_Move.Stop();
            Entity.StartSkill(null, m_SkillData.m_SkillL);
        }

        public void ResetMountsSkill(BaseSkillData data)
        {
            if (m_SkillData == null)
                m_SkillData = new BaseSkillData();

            m_SkillData.Reset(data.m_SkillL, data.m_SKillIDSpace, data.m_Skillpounce);
        }

        public void SetctrlType(ECtrlType type)
        {
            ctrlType = type;
        }

        public void SubscribeEndLoad()
        {
            Pathea.PeLauncher.Instance.eventor.Subscribe(OnResponse);
        }

        public void UnsubscribeEndLoad()
        {
            Pathea.PeLauncher.Instance.eventor.Unsubscribe(OnResponse);
        }

        #endregion

        #region IEnumerator
        IEnumerator ERestoreMonsterWait(float time)
        {
            while (Time.time - time < RestoreWaitTime)
            {
                if (ctrlType != ECtrlType.Free)
                    yield break;

                yield return null;
            }

            if (ctrlType == ECtrlType.Free) RestoreMonsterForceDb();
        }
        IEnumerator EmutiWait(PeEvent.EventArg arg)
        {
            if (PeGameMgr.IsMulti && NetworkInterface.IsClient && arg is Pathea.PeLauncher.LoadFinishedArg)
            {
                while (Entity == null || PeCreature.Instance == null
                    || null == Entity.biologyViewCmpt || null == Entity.biologyViewCmpt.biologyViewRoot
                    || null == Entity.biologyViewCmpt.biologyViewRoot.modelController)
                {
                    yield return null;
                }

                // PlayerNetwork.RequestReqMonsterCtrl(Entity.Id);
                Entity.BehaveCmpt.Stop();

                //lz-2017.03.14 传送太远怪物会被重刷，需要重新骑上去
                if (null != m_Master && m_Master.Id == PeCreature.Instance.mainPlayerId)
                {
                    MousePickRides rides = Entity.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
                    if (rides)
                    {
                        rides.RecoverExecRide(m_Master);
                    }
                }
            }

            yield return null;
        }

        IEnumerator Epounce(float time, Vector3 targetPos)
        {
            if (Entity == null || m_Move == null || Entity.animCmpt == null)
                yield break;

            PounceData m_Data = null;
            if (!MonsterXmlData.GetData<PounceData>(Entity.ProtoID, ref m_Data))
                yield break;

            float _speed = (PETools.PEUtil.MagnitudeH(Entity.position, targetPos) + 1f) / (m_Data._endTime - m_Data._startTime);
            while (Time.time - time < m_Data._startTime)
            {
                m_Move.Stop();
                yield return null;
            }

            while (Time.time - time <= m_Data._endTime)
            {
                m_Move.SetSpeed(_speed);
                m_Move.Move(Entity.forward);
                Vector3 m_Direction = targetPos - Entity.position;
                Vector3 m_DirectionXZ = Vector3.ProjectOnPlane(m_Direction, Vector3.up);
                m_Move.RotateTo(m_DirectionXZ);
                yield return null;
            }

            ResetMonster();
           
            yield return null;
        }

        IEnumerator EloadMount(MousePickRides rides)
        {
            //lz-2017.02.23 保证有视图和有Ragdoll
            if (Entity == null || !Entity.hasView || null == Entity.biologyViewCmpt || null == Entity.biologyViewCmpt.monoRagdollCtrlr)
                yield return null;

            //lz-2017.02.23 保证在其他事件执行之后
            yield return null;

            if (rides && rides.ExecRide(m_Master))
            {
                StartPlayerCtrl(m_Master);
                if (m_Master.mountCmpt) m_Master.mountCmpt.SetMount(Entity);
            }
        }
        #endregion

        #region mono

        public override void Start()
        {
            base.Start();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            UpdataTamingMove();
            UpdateMoveState();
        }

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            r.ReadInt32();

            ctrlType = (ECtrlType)r.ReadInt32();

        }

        public override void Serialize(BinaryWriter w)
        {
            base.Serialize(w);
            w.Write(CURRENT_VERSION);//int
            w.Write((int)ctrlType);//int_type   --------------PETools.Serialize.WriteData();
                                   // w.Write(m_Master.Id);   //int                        
                                   //m_SkillData.Export(w);
                                   //PETools.Serialize.WriteData(m_MountsForceDb.Export, w);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (PeGameMgr.IsMulti && m_Master != null && PeCreature.Instance != null
                && Entity.biologyViewCmpt != null && Entity.biologyViewCmpt.biologyViewRoot != null && Entity.biologyViewCmpt.biologyViewRoot.modelController != null
                )
            {
                if (m_Master.Id == PeCreature.Instance.mainPlayerId)
                {
                    MousePickRides rides = Entity.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
                    if (rides != null)
                    {
                        rides.ExecUnRide(m_Master);
                    }
                }
            }
        }
        #endregion
    }

}
