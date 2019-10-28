using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Behave.Runtime;
using SkillSystem;
using Pathea;
using Resources = UnityEngine.Resources;

public class BehaveGroup : MonoBehaviour, IBehave
{
    EntityGrp m_Grp;
    PeEntity m_Leader;
    //int m_BehaveID;
    int m_MaxCount;

    List<PeEntity> m_Entities = new List<PeEntity>();

    List<Vector3> m_Locals;

    List<Vector3> m_LocalUse;

    public int atkMin { get { return m_Grp._atkMin; } }
    public int atkMax { get { return m_Grp._atkMax; } }

    public List<PeEntity> Entities
    {
        get { return m_Entities; }
    }

    public float AlivePercent
    {
        get
        {
            int count = 0;

            for (int i = 0; i < m_Entities.Count; i++)
            {
                if (m_Entities[i] != null && !m_Entities[i].IsDeath())
                    count++;
            }

            return count / (float)m_MaxCount;
        }
    }

    public float EscapePercent
    {
        get
        {
            int count = 0;

            for (int i = 0; i < m_Entities.Count; i++)
            {
                if (m_Entities[i] != null 
                    && m_Entities[i].target != null 
                    && m_Entities[i].target.GetEscapeEnemyUnit() != null)
                    count++;
            }

            return count / (float)m_MaxCount;
        }
    }

    public PeEntity Leader
    {
        get
        {
            //if(m_Leader == null || m_Leader.IsDeath() || !m_Leader.hasView)
            //    m_Leader = m_Entities.Find(ret => ret != null && !ret.IsDeath() && ret.hasView);

            return m_Leader;
        }

        set { m_Leader = value; }
    }

    public Vector3 FollowLeader(PeEntity entity)
    {
        if (Leader == null || entity == null)
            return Vector3.zero;

        CalculateLocal(entity);

        Vector3 local = entity.GroupLocal * 5.0f * entity.maxRadius;

        if (entity.Field == MovementField.Sky)
            local += Random.Range(-10.0f, 10.0f) * Vector3.up;
        else if(entity.Field == MovementField.water)
        {
            Vector3 v = Leader.position;
            float height = VFVoxelWater.self.UpToWaterSurface(v.x, v.y, v.z);
            if(height > 0.0f)
            {
                local += Random.Range(-5.0f, Mathf.Max(0.0f, height - entity.maxHeight)) * Vector3.up;
            }
        }

        return Leader.tr.TransformPoint(local);
    }

    public Vector3 FollowEnemy(PeEntity entity, float radius)
    {
        if (Leader == null || entity == null || entity.attackEnemy == null)
            return Vector3.zero;

        CalculateLocal(entity, true);

        return entity.attackEnemy.modelTrans.TransformPoint(entity.GroupLocal * radius);
    }

    void Awake()
    {
        m_Grp = GetComponent<EntityGrp>();
        if(m_Grp != null) m_Grp.handlerMonsterCreated += OnMemberCreated;
    }

    void Start()
    {
        //if (m_Grp != null)
        //{
        //    string behavePath = Pathea.MonsterGroupProtoDb.GetBehavePath(m_Grp._grpProtoId);
        //    if (!string.IsNullOrEmpty(behavePath))
        //    {
        //        SetBehavePath(behavePath);
        //    }
        //}

        InitLocals();
    }

    void Update()
    {
        if (m_Leader == null || m_Leader.IsDeath() || !m_Leader.hasView)
            m_Leader = m_Entities.Find(ret => ret != null && !ret.IsDeath() && ret.hasView);
    }

    void InitLocals(bool isGravity = true)
    {
        m_Locals = new List<Vector3>();

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                float x = i;
                float y = 0f;
                float z = j;

                m_Locals.Add(new Vector3(x, y ,z));
            }
        }

        m_LocalUse = new List<Vector3>(m_Locals);
    }

    void CalculateLocal(PeEntity entity, bool isForce = false)
    {
        if (!isForce && entity.GroupLocal != Vector3.zero)
            return;

        if (m_LocalUse == null || m_LocalUse.Count == 0)
            return;

        Vector3 local = entity.GroupLocal;
        Vector3 newLocal = m_LocalUse[Random.Range(0, m_LocalUse.Count)];
        entity.GroupLocal = newLocal;
        m_LocalUse.Remove(newLocal);
        if (local != Vector3.zero) m_LocalUse.Add(local);
    }

    public void SetBehavePath(string behavePath)
    {
        BTLauncher.Instance.Instantiate(behavePath, this, true);
    }

    public void RegisterMember(PeEntity skEntity)
    {
        if (!m_Entities.Contains(skEntity))
        {
            skEntity.Group = this;
            m_Entities.Add(skEntity);
            
            m_MaxCount++;
        }
    }

    public void RemoveMember(PeEntity skEntity)
    {
        if (m_Entities.Contains(skEntity))
        {
            if (m_Leader != null && m_Leader.Equals(skEntity))
                m_Leader = null;

            skEntity.Group = null;
            m_Entities.Remove(skEntity);

            m_MaxCount--;
        }
    }

    public void PauseMemberBehave(bool value)
    {
        foreach (PeEntity skEntity in m_Entities)
        {
            if (skEntity != null)
            {
                BehaveCmpt behave = skEntity.GetComponent<BehaveCmpt>();
                if (behave != null)
                {
                    behave.Pause(value);
                }
            }
        }
    }

    public bool HasAttackEnemy()
    {
        foreach (PeEntity skEntity in m_Entities)
        {
            if(skEntity != null && !skEntity.IsDeath())
            {
                TargetCmpt targetCmpt = skEntity.GetComponent<TargetCmpt>();
                if (targetCmpt != null && targetCmpt.GetAttackEnemy() != null)
                    return true;
            }
        }

        return false;
    }

    public bool HasEscapeEnemy()
    {
        foreach (PeEntity skEntity in m_Entities)
        {
            if (skEntity != null && !skEntity.IsDeath())
            {
                TargetCmpt targetCmpt = skEntity.GetComponent<TargetCmpt>();
                if (targetCmpt != null && targetCmpt.GetEscapeEnemy() != null)
                    return true;
            }
        }

        return false;
    }

    public void Fly(bool value)
    {
        foreach (PeEntity entity in m_Entities)
        {
            if (entity != null && !entity.IsDeath())
            {
                MonsterCmpt cmpt = entity.GetComponent<MonsterCmpt>();
                if (cmpt != null)
                {
                    cmpt.Fly(value);
                }
            }
        }
    }

    public void ActivateGravity(bool value)
    {
        foreach (PeEntity entity in m_Entities)
        {
            if (entity != null && !entity.IsDeath())
            {
                MonsterCmpt cmpt = entity.GetComponent<MonsterCmpt>();
                if(cmpt != null)
                {
                    cmpt.ActivateGravity(value);
                }
            }
        }
    }

    public void MoveToPosition(Vector3 pos, SpeedState speed = SpeedState.Walk)
    {
        foreach (PeEntity skEntity in m_Entities)
        {
            if (skEntity != null && !skEntity.IsDeath())
            {
                Motion_Move mover = skEntity.GetComponent<Motion_Move>();
                if(mover != null && Leader != null)
                {
                    PeTrans tr1 = mover.GetComponent<PeTrans>();
                    PeTrans tr2 = Leader.GetComponent<PeTrans>();
                    if(tr1 != null && tr2 != null)
                    {
                        float r = 3 * Random.value * Mathf.Max(1.0f, Mathf.Max(tr1.radius, tr2.radius));
                        mover.MoveTo(pos + (tr1.position - tr2.position).normalized * r, speed);
                    }
                }
            }
        }
    }

    public Enemy EscapeEnemy
    {
        get
        {
            for (int i = 0; i < m_Entities.Count; i++)
            {
                if (m_Entities[i] == null) continue;

                if (m_Entities[i].target != null)
                {
                    Enemy enemy = m_Entities[i].target.GetEscapeEnemyUnit();
                    if (enemy != null) return enemy;
                }
            }

            return null;
        }
    }

    public void SetEscape(PeEntity self, PeEntity escapeEntity)
    {
        for (int i = 0; i < m_Entities.Count; i++)
        {
            if (m_Entities[i] != null && m_Entities[i].target != null && !m_Entities.Equals(self))
            {
                m_Entities[i].target.SetEscapeEntity(escapeEntity);
            }
        }
    }

    public void OnTargetDiscover(PeEntity self, PeEntity target)
    {
        if (self == null || target == null)
            return;

        for (int i = 0; i < m_Entities.Count; i++)
        {
            if(m_Entities[i] != null && !m_Entities.Equals(self))
            {
                m_Entities[i].OnTargetDiscover(target);
            }
        }
    }

    public void OnDamageMember(PeEntity self, PeEntity target, float hatred)
    {
        for (int i = 0; i < m_Entities.Count; i++)
        {
            if(m_Entities[i] != null && !m_Entities.Equals(self))
            {
                m_Entities[i].OnDamageMember(target, hatred);
            }
        }
    }

    void OnMemberCreated(PeEntity e)
    {
        if (e != null)
        {
            RegisterMember(e);
        }
    }

    #region IAgent
    public void Reset(Behave.Runtime.Tree sender)
    {

    }

    public int SelectTopPriority(Behave.Runtime.Tree sender, params int[] IDs)
    {
        return IDs[0]; ;
    }

    public BehaveResult Tick(Behave.Runtime.Tree sender)
    {
        return BehaveResult.Success;
    }

    public bool BehaveActive
    {
        get { return true; }
    }
    #endregion
}
