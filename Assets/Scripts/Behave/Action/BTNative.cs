using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;

namespace Behave.Runtime
{
    [BehaveAction(typeof(BTProfession), "Profession")]
    public class BTProfession : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string profession = "";

            NativeProfession m_Type = NativeProfession.None;

            public NativeProfession type
            {
                get
                {
                    if (m_Type == NativeProfession.None)
                    {
                        try {
                            m_Type = (NativeProfession)System.Enum.Parse(typeof(NativeProfession), profession);
                        }
                        catch (System.Exception ex) {
                            Debug.LogWarning(ex);
                        }
                    }

                    return m_Type;
                }
            }
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (nativeProfession == m_Data.type)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTNativeSex), "NativeSex")]
    public class BTNativeSex : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string sex = "";

            NativeSex m_Type = NativeSex.None;

            public NativeSex type
            {
                get
                {
                    if (m_Type == NativeSex.None)
                    {
                        try{
                            m_Type = (NativeSex)System.Enum.Parse(typeof(NativeSex), sex);
                        }
                        catch (System.Exception ex){
                            Debug.LogWarning(ex);
                        }
                    }

                    return m_Type;
                }
            }
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (nativeSex == m_Data.type)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTNativeAge), "NativeAge")]
    public class BTNativeAge : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string age = "";

            NativeAge m_Type = NativeAge.None;

            public NativeAge type
            {
                get
                {
                    if (m_Type == NativeAge.None)
                    {
                        try{
                            m_Type = (NativeAge)System.Enum.Parse(typeof(NativeAge), age);
                        }
                        catch (System.Exception ex){
                            Debug.LogWarning(ex);
                        }
                    }

                    return m_Type;
                }
            }
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (nativeAge == m_Data.type)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTReputation), "Reputation")]
    public class BTReputation : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string type = "";
            [BehaveAttribute]
            public string minReputation = "";
            [BehaveAttribute]
            public string maxReputation = "";

            public ReputationSystem.ReputationLevel minType;
            public ReputationSystem.ReputationLevel maxType;

            bool m_Init = false;
            public void Init()
            {
                if (!m_Init)
                {
                    m_Init = true;
                    minType = (ReputationSystem.ReputationLevel)System.Enum.Parse(typeof(ReputationSystem.ReputationLevel), minReputation);
                    maxType = (ReputationSystem.ReputationLevel)System.Enum.Parse(typeof(ReputationSystem.ReputationLevel), maxReputation);
                }
            }
        }

        Data m_Data;

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            PeEntity e = entity.GetReputation(m_Data.minType, m_Data.maxType);

            if (e != null)
            {
                if (m_Data.type == "afraid")
                    entity.Afraid = e;
                else if (m_Data.type == "doubt")
                    entity.Doubt = e;

                return BehaveResult.Success;
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTGuard), "Guard")]
    public class BTGuard : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (spawnPosition == Vector3.zero)
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Success;

            float d = 0.0f;
            if (gravity > PETools.PEMath.Epsilon)
                d = PEUtil.SqrMagnitudeH(position, spawnPosition);
            else
                d = PEUtil.SqrMagnitude(position, spawnPosition);

            if (d > 1f * 1f)
            {
                if(Stucking(3.0f))
                    SetPosition(spawnPosition);
                else
                    MoveToPosition(spawnPosition, SpeedState.Run);

                return BehaveResult.Running;
            }
            else
            {
                MoveToPosition(Vector3.zero);

                if (PEUtil.AngleH(transform.forward, spawnForward) > 5.0f)
                {
                    FaceDirection(spawnForward);
                    return BehaveResult.Running;
                }
                else
                {
                    FaceDirection(Vector3.zero);
                    return BehaveResult.Success;
                }
            }
        }
    }

    [BehaveAction(typeof(BTSneak), "Sneak")]
    public class BTSneak : BTNormal
    {
        float m_LookAroundTime;
        float m_LastLookAroundTime;
        BehaveResult Tick(Tree sender)
        {
            if (GetBool("BehaveWaiting"))
                return BehaveResult.Running;

            if (!Enemy.IsNullOrInvalid(attackEnemy) || !Enemy.IsNullOrInvalid(escapeEnemy))
                SetBool("Sneak", false);
            else
            {
                SetBool("Sneak", true);

                if(Time.time - m_LastLookAroundTime > m_LookAroundTime)
                {
                    m_LookAroundTime = Random.Range(5.0f, 10.0f);
                    m_LastLookAroundTime = Time.time;

                    SetBool("scout_lookaround", true);
                }
            }

            return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTInjured), "Injured")]
    public class BTInjured : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float cancelHpPercent = 0.0f;
            [BehaveAttribute]
            public string anim = "";
        }

        Data m_Data;
        bool m_End;
        bool m_Run;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!entity.IsSeriousInjury)
                return BehaveResult.Failure;

            m_Run = true;
            m_End = false;
            SetBool(m_Data.anim, true);
            StartSkill(entity, 30100326);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!m_End)
            {
                if (HpPercent > m_Data.cancelHpPercent)
                {
                    m_End = true;
                    entity.IsSeriousInjury = false;
                    SetBool(m_Data.anim, false);
                    return BehaveResult.Running;
                }
            }
            else
            {
                if (!GetBool("BehaveWaiting"))
                    return BehaveResult.Success;
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Run)
            {
                StopSkill(30100326);
                m_Run = false;
            }
        }
    }

    [BehaveAction(typeof(BTElude), "Elude")]
    public class BTElude : BTNormal
    {
        EludePoint m_Point;

        BehaveResult Init(Tree sender)
        {
            m_Point = PEEludePoint.GetEludePoint(transform.position, attackEnemy.position);
            if (m_Point == null)
            {
                if(!Enemy.IsNullOrInvalid(attackEnemy))
                    SetEscapeEntity(attackEnemy.entityTarget);

                return BehaveResult.Failure;
            }

            m_Point.Dirty = true;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!m_Point.CanElude(attackEnemy.position))
                return BehaveResult.Failure;

            if (!m_Point.Elude(position))
                MoveToPosition(m_Point.Position, SpeedState.Run);
            else
            {
                MoveToPosition(Vector3.zero);
                FaceDirection(m_Point.FaceDirection);

                Vector3 v1 = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                Vector3 v2 = Vector3.ProjectOnPlane(m_Point.FaceDirection, Vector3.up);

                if(!GetBool("Elude") && Vector3.Angle(v1, v2) < 5.0f)
                    SetBool("Elude", true);
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if(m_Point != null)
            {
                SetBool("Elude", false);
                FaceDirection(Vector3.zero);
                m_Point.Dirty = false;
                m_Point = null;
            }
        }
    }

    [BehaveAction(typeof(BTCallHelp), "CallHelp")]
    public class BTCallHelp : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public string anim = "";
            [BehaveAttribute]
            public float hpPercent = 0.0f;
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float radius;
        }

        Data m_Data;

        float m_LastCallTime;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (HpPercent > m_Data.hpPercent)
                return BehaveResult.Failure;

            if (Time.time - m_LastCallTime < m_Data.cdTime)
                return BehaveResult.Failure;

            m_LastCallTime = Time.time;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            SetBool(m_Data.anim, true);
            CallHelp(m_Data.radius);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_LastCallTime < 0.25f)
                return BehaveResult.Running;

            if (GetBool("BehaveWaitting"))
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTChat), "Chat")]
    public class BTChat : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float radius = 0.0f;
            [BehaveAttribute]
            public float chatRadius = 0.0f;
            [BehaveAttribute]
            public float minTime = 0.0f;
            [BehaveAttribute]
            public float maxTime = 0.0f;
            [BehaveAttribute]
            public float minChatTime = 0.0f;
            [BehaveAttribute]
            public float maxChatTime = 0.0f;
            [BehaveAttribute]
            public string[] chats = new string[0];

            public float m_Time;
            public float m_ChatTime;
            public float m_StartTime;
            public float m_LastChatTime;
            public float m_LastCDTime;
        }

        Data m_Data;

        void GetChatTarget()
        {
            if(entity.Chat == null)
            {
                int playerid = (int)entity.GetAttribute(AttribType.DefaultPlayerID);
                List<PeEntity> entities = EntityMgr.Instance.GetEntitiesFriendly(position, m_Data.radius, playerid, entity.ProtoID, false, entity);
                if(entities.Count > 0)
                {
                    PeEntity chat = entities[Random.Range(0, entities.Count)];
                    entity.Chat = chat;
                    chat.Chat = entity;
                }
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (entity.Chat == null && Random.value > m_Data.prob)
                return BehaveResult.Failure;

            GetChatTarget();

            if (entity.Chat == null)
                return BehaveResult.Failure;

            m_Data.m_StartTime = Time.time;
            m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
            m_Data.m_ChatTime = Random.Range(m_Data.minChatTime, m_Data.maxChatTime);
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (GetBool("Chatting"))
                return BehaveResult.Running;

            if (entity.Chat == null)
                return BehaveResult.Success;

            if (!Enemy.IsNullOrInvalid(attackEnemy))
                return BehaveResult.Failure;

            if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
                return BehaveResult.Success;

            if (PEUtil.MagnitudeH(position, entity.Chat.position) > radius + entity.Chat.maxRadius + m_Data.chatRadius)
                MoveToPosition(entity.Chat.position, SpeedState.Walk);
            else
            {
                MoveToPosition(Vector3.zero);

                if (!PEUtil.IsScopeAngle(entity.Chat.position - position, transform.forward, Vector3.up, -15.0f, 15.0f))
                    FaceDirection(entity.Chat.position - position);
                else
                {
                    FaceDirection(Vector3.zero);

                    if(Time.time - m_Data.m_LastChatTime > m_Data.m_ChatTime)
                    {
                        if(m_Data.chats.Length > 0)
                        {
                            SetBool(m_Data.chats[Random.Range(0, m_Data.chats.Length)], true);
                        }

                        m_Data.m_LastChatTime = Time.time;
                        m_Data.m_ChatTime = Random.Range(m_Data.minChatTime, m_Data.maxChatTime);
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(m_Data.m_StartTime > PETools.PEMath.Epsilon)
            {
                if(entity.Chat != null)
                {
                    entity.Chat.Chat = null;
                    entity.Chat = null;
                }

                m_Data.m_Time = 0.0f;
                m_Data.m_ChatTime = 0.0f;
                m_Data.m_StartTime = 0.0f;
                m_Data.m_LastChatTime = 0.0f;

                m_Data.m_LastCDTime = Time.time;
            }
        }
    }

    [BehaveAction(typeof(BTHasTreat), "HasTreat")]
    public class BTHasTreat : BTNormal
    {
        BehaveResult Tick(Tree sender)
        {
            if (entity.Treat != null)
                return BehaveResult.Success;
            else
                return BehaveResult.Failure;
        }
    }

    [BehaveAction(typeof(BTTreat), "Treat")]
    public class BTTreat : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public int skillId = 0;
            [BehaveAttribute]
            public string anim = "";
        }

        Data m_Data;
        bool m_Treat;

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (entity.Treat == null || entity.Treat.IsDeath() || !entity.Treat.hasView)
                return BehaveResult.Failure;

            m_Treat = false;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (entity.Treat == null || entity.Treat.IsDeath() || !entity.Treat.hasView)
                return BehaveResult.Failure;

            if (!entity.Treat.IsSeriousInjury)
                return BehaveResult.Success;

            //Vector3 targetPos = entity.Treat.position + entity.Treat.tr.right * (radius + entity.Treat.maxRadius + 1.0f);
            Vector3 targetPos = entity.Treat.position;
            if (PEUtil.SqrMagnitudeH(position, targetPos) > 2f * 2f)
                MoveToPosition(targetPos, SpeedState.Run);
            else
            {
                MoveToPosition(Vector3.zero);

                Vector3 v1 = Vector3.ProjectOnPlane(entity.Treat.position - position, Vector3.up);
                Vector3 v2 = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                if (Vector3.Angle(v1, v2) > 5.0f)
                    FaceDirection(v1);
                else
                {
                    FaceDirection(Vector3.zero);

                    if (!m_Treat)
                    {
                        m_Treat = true;

                        entity.Treat.StopSkill(30100326);
                        SetBool(m_Data.anim, true);
                        StartSkill(entity.Treat, m_Data.skillId);
                    }
                }
            }

            return BehaveResult.Running;
        }

        void Reset(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return;

            if(entity.Treat != null)
            {
                SetBool(m_Data.anim, false);
                StopSkill(m_Data.skillId);
                entity.Treat = null;
            }
        }
    }

    [BehaveAction(typeof(BTInspire), "Inspire")]
    public class BTInspire : BTNormal
    {
        class Data
        {
            [BehaveAttribute]
            public float prob = 0.0f;
            [BehaveAttribute]
            public float cdTime = 0.0f;
            [BehaveAttribute]
            public float radius = 0.0f;
            [BehaveAttribute]
            public int skillId = 0;
            [BehaveAttribute]
            public string anim = "";
        }

        Data m_Data;
        float m_LastTime = -1000f;

        void Inspire()
        {
            SetBool(m_Data.anim, true);
            List<PeEntity> entities = EntityMgr.Instance.GetEntitiesFriendly(position, m_Data.radius, (int)GetAttribute(AttribType.DefaultPlayerID), entity.ProtoID, false, entity);
            for (int i = 0; i < entities.Count; i++)
            {
                StartSkill(entities[i].GetComponent<PeEntity>(), m_Data.skillId);
            }
        }

        BehaveResult Init(Tree sender)
        {
            if (!GetData<Data>(sender, ref m_Data))
                return BehaveResult.Failure;

            if (Time.time - m_LastTime < m_Data.cdTime)
                return BehaveResult.Failure;

            if (Random.value > m_Data.prob)
                return BehaveResult.Failure;

            Inspire();
            m_LastTime = Time.time;
            return BehaveResult.Running;
        }

        BehaveResult Tick(Tree sender)
        {
            if (GetBool("Inspire"))
                return BehaveResult.Running;
            else
                return BehaveResult.Success;
        }
    }

    [BehaveAction(typeof(BTSquat), "Squat")]
    public class BTSquat : BTNormal
    {
        float m_LastCheckTime = 0.0f;
        float m_LastSquatTimeCD = -20.0f;
        float m_StartSquatTime = 0.0f;
        float m_CurSquatTime = 10.0f;

        bool m_Squat;

        BehaveResult Tick(Tree sender)
        {
            if (Weapon == null || attackEnemy == null)
                return BehaveResult.Success;

            if (attackEnemy.GroupAttack == EAttackGroup.Threat)
                return BehaveResult.Success;

            if (attackEnemy.velocity.sqrMagnitude > 0.15f * 0.15f)
                return BehaveResult.Success;

            //if (Weapon.ItemObj.protoId != 1138
            //    && Weapon.ItemObj.protoId != 1140
            //    && Weapon.ItemObj.protoId != 1142
            //    && Weapon.ItemObj.protoId != 1516)
            //    return BehaveResult.Success;

            bool isSquat = GetBool("Squat");
            bool isAim = GetBool("Bazooka_Aim") || GetBool("Rifle_Aim");
            bool isAngle = PEUtil.IsScopeAngle(transform.forward, attackEnemy.Direction, Vector3.up, -75.0f, 75.0f);

            if(m_Squat != isSquat)
            {
                m_Squat = isSquat;
                if(!m_Squat)
                {
                    m_LastSquatTimeCD = Time.time;
                }
            }

            if (!m_Squat)
            {
                if (Time.time - m_LastSquatTimeCD > 15.0f)
                {
                    if (Time.time - m_LastCheckTime > 5.0f)
                    {
                        if (Random.value < 0.5f && isAngle && isAim)
                        {
                            SetBool("Squat", true);
                            m_StartSquatTime = Time.time;
                        }

                        m_LastCheckTime = Time.time;
                    }
                }
            }
            else
            {
                if(Time.time - m_StartSquatTime > m_CurSquatTime || !isAim)
                {
                    SetBool("Squat", false);

                    m_CurSquatTime = Random.Range(10.0f, 20.0f);
                }
            }

            return BehaveResult.Success;
        }

        void Reset(Tree sender)
        {
            if (GetBool("Squat") && attackEnemy == null)
            {
                m_Squat = false;
                SetBool("Squat", false);
            }
        }
    }
}