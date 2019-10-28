using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;
using Resources = UnityEngine.Resources;
using SkillSystem;
using AnimFollow;
using Behave.Runtime.Action;

namespace Pathea
{
    public enum BHPatrolMode
    {
        None,
        SpawnCenter,
        CurrentCenter
    }

    public class BehaveCmpt : PeCmpt, IPeMsg, IBehave
    {
        //static int VERSION = 1;

        string assetPath = "";
        int m_BehaveID;

        //ViewCmpt m_View;
        SkAliveEntity m_SkEntity;
        RequestCmpt m_Request;
        NpcCmpt m_Npc;

        public event Action<int> OnBehaveStop;

        bool m_Start;

        #region behave attribute
        float m_minPatrolRadius = 0.0f;
        public float MinPatrolRadius { get { return m_minPatrolRadius; } set { m_minPatrolRadius = value; } }
        float m_maxPatrolRadius = 0.0f;
        public float MaxPatrolRadius { get { return m_maxPatrolRadius; } set { m_maxPatrolRadius = value; } }
        BHPatrolMode m_PatrolMode = BHPatrolMode.CurrentCenter;
        public BHPatrolMode PatrolMode { get { return m_PatrolMode; } set { m_PatrolMode = value; } }
        #endregion

        #region public function
        public void Excute()
        {
            m_Start = true;

            if(BTLauncher.Instance != null)
                BTLauncher.Instance.Excute(m_BehaveID);
        }

        public void Stop()
        {
            m_Start = false;

            if(BTLauncher.Instance != null)
                BTLauncher.Instance.Stop(m_BehaveID);
        }

        public void Reset()
        {
            if(BTLauncher.Instance != null)
                BTLauncher.Instance.Reset(m_BehaveID);
        }

        public void Pause(bool value)
        {
            if(BTLauncher.Instance != null)
                BTLauncher.Instance.Pause(m_BehaveID, value);
        }

        public void SetAssetPath(string path)
        {
            assetPath = path;
        }

        public void Stopbehave()
        {
            if (m_Request != null && m_Request.HasAnyRequest())
                return;

            if (m_Npc != null && (m_Npc.Type == ENpcType.Follower || m_Npc.Type == ENpcType.Base))
                return;

            DispatchBehaveStopEvent();
            Stop();
        }

        void DispatchBehaveStopEvent()
        {
            if (OnBehaveStop != null)
                OnBehaveStop(m_BehaveID);
        }

        #endregion

        void OnDeath(SkEntity self, SkEntity injurer)
        {
            BTLauncher.Instance.Reset(m_BehaveID);
            BTLauncher.Instance.Pause(m_BehaveID, true);
        }

        void OnRevive(SkEntity entity)
        {
            BTLauncher.Instance.Pause(m_BehaveID, false);
        }

        void InitAttacks()
        {
            if(m_BehaveID > 0)
            {
                TargetCmpt target = GetComponent<TargetCmpt>();
                if (target != null)
                {
                    target.SetActions(BTLauncher.Instance.GetAgent(m_BehaveID).GetActions());
                }
            }
        }

        #region PeCmpt
//        public void OnEnable()
//        {
//            Excute();
//        }
//
//        public void OnDisable()
//        {
//            Stop();
//        }

        public override void Start()
        {
            base.Start();

            //m_View = GetComponent<ViewCmpt>();
            m_SkEntity = GetComponent<SkAliveEntity>();
            m_Request = GetComponent<RequestCmpt>();
            m_Npc = GetComponent<NpcCmpt>();

            if (m_SkEntity != null)
            {
                m_SkEntity.deathEvent += OnDeath;
                m_SkEntity.reviveEvent += OnRevive;
            }

            m_BehaveID = BTLauncher.Instance.Instantiate(assetPath, this, false);

            InitAttacks();

			//添加m_Npc.hasAnyRequest 否则跟随任务读档传送无法继续进行
            if (PeGameMgr.IsSingle && m_Npc != null && (m_Npc.Type == ENpcType.Follower || m_Npc.Type == ENpcType.Base || m_Npc.hasAnyRequest))
                Excute();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (BTLauncher.Instance == null)
                return;

            if (m_BehaveID > 0 && m_Start && !BTLauncher.Instance.IsStart(m_BehaveID))
                BTLauncher.Instance.Excute(m_BehaveID);

            if (m_BehaveID > 0 && !m_Start && BTLauncher.Instance.IsStart(m_BehaveID))
                BTLauncher.Instance.Stop(m_BehaveID);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if(BTLauncher.Instance != null)
                BTLauncher.Instance.Remove(m_BehaveID);

            if (m_SkEntity != null)
            {
                m_SkEntity.deathEvent -= OnDeath;
                m_SkEntity.reviveEvent -= OnRevive;
            }
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            w.Write(assetPath);
        }

        public override void Deserialize(System.IO.BinaryReader r)
        {
            assetPath = r.ReadString();
        }

        #endregion

        public void Reset(Behave.Runtime.Tree sender)
        {
        }

        public int SelectTopPriority(Behave.Runtime.Tree sender, params int[] IDs)
        {
            return IDs[0];
        }

        public BehaveResult Tick(Behave.Runtime.Tree sender)
        {
            return BehaveResult.Success;
        }

        public bool BehaveActive
        {
            get { return true; }
        }

        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.View_Model_Build:
                    if(!PeGameMgr.IsMulti)
                        Excute();
                    break;

                case EMsg.View_Model_Destroy:
                    if (!PeGameMgr.IsMulti)
                    {
                        Pause(false);
                        Stopbehave(); 
                    }
                        
                    break;

                case EMsg.State_Die:
                    Reset();
                    break;

                case EMsg.Net_Controller:
                    if (Entity.monstermountCtrl != null && Entity.monstermountCtrl.ctrlType == ECtrlType.Mount)
                        break;

                    Excute();
                    break;

                case EMsg.Net_Proxy:
                    Stop();
                    break;

                case EMsg.View_Ragdoll_Fall_Begin:
                    Pause(true);
                    break;

                case EMsg.View_Ragdoll_Getup_Finished:
                    Pause(false);
                    break;
            }
        }
    }
}

