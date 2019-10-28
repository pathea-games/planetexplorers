using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SoundAsset;

namespace Pathea
{
    public enum NativeProfession
    {
        None,
        Solider,
        Sentry,
        Guard,
        Doctor,
        Civilian,
        General,
        Bodyguard,
        Scout,
        Merchant,
        Max
    }

    public enum NativeAge
    {
        None,
        Child,
        Adult,
        Oldman,
        Max
    }

    public enum NativeSex
    {
        None,
        Male,
        Female,
        Max
    }

    public class MonsterCmpt : PeCmpt, IPeMsg
    {
        AnimatorCmpt m_Animator;
        SkAliveEntity m_SkEntity;
        BehaveCmpt m_Behave;
        Motion_Move_Motor m_Motor;
        PeTrans m_Trans;
        RequestCmpt m_Request;

        PENative m_Native;
        BehaveGroup m_Group;
        Vector3 m_GroupLocal;

        int m_InjuredLevel;

        bool m_IsDark;
        bool m_IsFly;
        bool m_Injury;
        bool m_IsAttacking;
        bool m_IsWaterSurface;
        bool m_SeriousInjury;

        bool m_CanRide = true;

        public bool CanRide { get { return m_CanRide; }}
        public int InjuredLevel { get { return m_InjuredLevel; } set { m_InjuredLevel = value; } }
        public bool IsFly { get { return m_IsFly; } }
        public bool IsGroup { get { return m_Group != null; } }
        public bool IsLeader { get { return m_Group != null && Entity.Equals(m_Group.Leader); } }
        public bool IsMember { get { return m_Group != null && !Entity.Equals(m_Group.Leader); } }
        public bool IsInjury { get { return m_Injury; } }
        public bool IsSeriousInjury { get { return m_SeriousInjury; } set { m_SeriousInjury = value; } }
        public bool IsDark { get { return m_IsDark; } set { m_IsDark = value; } }
        public NativeProfession Profession { get { return m_Native != null ? m_Native.Profession : NativeProfession.None; } }
        public NativeAge Age { get { return  m_Native != null ? m_Native.Age : NativeAge.None; } }
        public NativeSex Sex { get { return  m_Native != null ? m_Native.Sex : NativeSex.None; } }
        public PeEntity Leader { get { return  m_Group != null ? m_Group.Leader : null; } }
        public BehaveGroup Group { get { return  m_Group; } set { m_Group = value; } }
        public Vector3 GroupLocal { get { return  m_GroupLocal; } set { m_GroupLocal = value; } }

        public bool IsAttacking { get { return m_Animator != null && m_Animator.GetBool("Attacking"); } }

        public bool WaterSurface
        {
            get
            {
                return m_IsWaterSurface;
            }
            set
            {
                if(m_IsWaterSurface != value)
                {
                    m_IsWaterSurface = value;

                    m_Animator.SetBool("WaterSurface", m_IsWaterSurface);
                }
            }
        }

        void OnDamage(SkillSystem.SkEntity entity, float damage)
        {
            m_Injury = Entity.HPPercent <= 0.5f;
        }

        public override void Awake()
        {
			base.Awake ();
            m_Animator = GetComponent<AnimatorCmpt>();
            m_SkEntity = GetComponent<SkAliveEntity>();
            m_Behave   = GetComponent<BehaveCmpt>();
            m_Motor = GetComponent<Motion_Move_Motor>();
            m_Trans = GetComponent<PeTrans>();
            m_Request = GetComponent<RequestCmpt>();

            if (m_SkEntity != null)
                m_SkEntity.deathEvent += OnDeath;
        }

        public override void Start()
        {
            base.Start();

            if (Entity.Race == ERace.Monster && Entity.Field != MovementField.water)
            {
                MonsterProtoDb.Item item = MonsterProtoDb.Get(Entity.ProtoID);
                if(item != null && item.idleSounds != null && item.idleSounds.Length > 0)
                    StartCoroutine(IdleAudio(item.idleSounds, item.idleSoundDis));
            }

        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if(m_Animator != null)
            {
                m_Animator.SetBool("Ground", m_Motor != null ? m_Motor.grounded : false);
            }
        }

        void OnDeath(SkillSystem.SkEntity skEntity1, SkillSystem.SkEntity skEntity2)
        {
			MonsterProtoDb.Item protoItem = MonsterProtoDb.Get(Entity.ProtoID);
            if (protoItem != null && protoItem.deathAudioID > 0)
            {
                AudioManager.instance.Create(transform.position, protoItem.deathAudioID);
            }
        }

        IEnumerator IdleAudio(int[] sounds, float distance)
        {
            List<int> _tmpSounds = new List<int>();
            while (sounds != null && sounds.Length > 0)
            {
                yield return new WaitForSeconds(Random.Range(20f, 35f));

                _tmpSounds.Clear();

                PeEntity player = PeCreature.Instance.mainPlayer;
                if (player != null && m_Animator != null)
                {
                    float sqrDis = PETools.PEUtil.SqrMagnitude(player.position, Entity.position);
                    if (sqrDis > distance * distance)
                    {
                        if (Random.value < 0.2f)
                        {
                            for (int i = 0; i < sounds.Length; i++)
                            {
                                SESoundBuff buff = SESoundBuff.GetSESoundData(sounds[i]);
                                if (buff != null && sqrDis < buff.mMaxDistance * buff.mMaxDistance * 0.81f)
                                    _tmpSounds.Add(sounds[i]);
                            }

                            if (_tmpSounds.Count > 0)
                            {
                                int soundID = _tmpSounds[Random.Range(0, _tmpSounds.Count)];
                                AudioManager.instance.Create(Entity.position, soundID);
                            }
                        }
                    }
                }
            }
        }

        public void ActivateGravity(bool value)
        {
            if (m_Motor != null && m_Motor.motor != null)
            {
                if(value)
                    m_Motor.motor.gravity = 10.0f;
                else
                    m_Motor.motor.gravity = 0.0f;

                m_IsFly = !value;
            }
        }

        public void Ride(bool value)
        {
            m_CanRide = value;
        }


        public void Fly(bool value)
        {
            if (m_Animator != null && m_Motor != null && m_Motor.Field == MovementField.Sky)
                m_Animator.SetBool("Fly", value);
        }

        public void OnMsg(EMsg msg, params object[] args)
        {
            switch (msg)
            {
                case EMsg.Action_Knocked:
                case EMsg.Action_Repulsed:
                case EMsg.Action_Wentfly:
                case EMsg.Action_Whacked:
                    m_Behave.Reset();
                    m_SkEntity.CancelAllSkills();
                    m_Animator.SetTrigger("Interrupt");
                    break;

			    case EMsg.View_Model_Build:
                    //GameObject obj = args[0] as GameObject;
					BiologyViewRoot viewRoot = args[1] as BiologyViewRoot;
                    m_Native = viewRoot.native;
                    PEMotor m = viewRoot.motor;
                    if (m_Trans != null && m_Animator != null && m != null && m_Motor != null && m_Motor.Field == MovementField.Sky)
				    {
					    if (m.gravity > 0.0f)
                        {
                            m_IsFly = false;
                            m_Animator.SetBool("Fly", false);
                        }
                        else
                        {
						    m_IsFly = true;
						    m_Animator.SetBool("Fly", true);
                        }
				    }

                    PEMonster monster = viewRoot.monster;
                    if(monster != null)
                    {
                        m_IsDark = monster.isDark;
                    }

                    break;
            }
        }

        public Request Req_MoveToPosition(Vector3 position, float stopRadius, bool isForce, SpeedState state)
        {
            return m_Request != null ? m_Request.Register(EReqType.MoveToPoint, position, stopRadius, isForce, state) : null;
        }
    }
}
