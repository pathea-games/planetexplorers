using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Random = UnityEngine.Random;

public class SiegeAgent
{
    public class AgentInfo : MonsterEntityCreator.AgentInfo
    {
        SiegeAgent _agent;

		public AgentInfo(EntityMonsterBeacon bcn, SiegeAgent agent) : base(bcn)
        {
            _agent = agent;
        }

        public override void OnSuceededToCreate(SceneEntityPosAgent agent)
        {
            //base.OnSuceededToCreate(agent);

            LodCmpt entityLodCmpt = agent.entity.lodCmpt;
            if (entityLodCmpt != null)
            {
                entityLodCmpt.onDestruct += (e) => { agent.DestroyEntity(); };
            }

            _agent.OnSuceededToCreate();
        }

        public override void OnFailedToCreate(SceneEntityPosAgent agent)
        {
            base.OnFailedToCreate(agent);

            _agent.OnFailedToCreate();
        }
    }

    public static Action<SiegeAgent> DeathEvent;

    float _hpMax;
    float _atkMax;

    float _hp;

    bool _death;

    MonsterSiege _siege;
    SceneEntityPosAgent _agent;
    PeEntity _target;

    public float hpPercent
    {
        get { return _hp / _hpMax; }
        set { _hp = _hpMax * Mathf.Clamp01(value); }
    }

    public Vector3 position
    {
        get {
            if (_agent.entity != null)
                return _agent.entity.position;
            else
                return _agent.Pos;
        }
    }

    public bool death
    {
        get { return _death; }
    }

    public bool hasView
    {
        get { return _agent.entity != null && _agent.entity.hasView; }
    }

    public SiegeAgent(MonsterSiege siege, SceneEntityPosAgent agent, float hp, float atk)
    {
        _siege = siege;
        _agent = agent;

        _hpMax = hp;
        _atkMax = atk;

        _hp = _hpMax;

        siege.StartCoroutine(Move());
        siege.StartCoroutine(Attack());

        //Debug.LogError("Spawn PosAgent at position : " + _agent.Pos);
    }

    public void ApplyDamage(float dmgValue)
    {
        _hp = Mathf.Clamp(_hp-dmgValue, 0.0f, _hpMax);

        if (_agent.entity != null)
            _agent.entity.HPPercent = hpPercent;
        else
        {
            if (!_death && _hp <= PETools.PEMath.Epsilon)
                OnDeath(null, null);
        }
    }

    public void Clear()
    {
        if (_agent != null && _agent.entity != null && _agent.entity.aliveEntity != null)
        {
            _agent.entity.aliveEntity.deathEvent -= OnDeath;
            _agent.entity.aliveEntity.onHpChange -= OnHpChange;
        }

        _siege.StopCoroutine(Move());
        _siege.StopCoroutine(Attack());
    }

    IEnumerator Move()
    {
        //float startTime = Time.time;
        //Vector3 startPos = _agent.Pos;

        while (!_death)
        {
            if (_agent.entity != null && (_agent.entity is EntityGrp))
            {
                _hp = 0.0f;

                if(!_death)
                    OnDeath(null, null);

                yield break;
            }

            Vector3 pos = _siege.assemblyPosition;
            float radius = _siege.assemblyRadius;

            if (_target != null && PETools.PEUtil.SqrMagnitude(_target.position, pos) > radius * radius)
                _target = null;

            if (_target == null || _target.IsDeath())
            {
                //startTime = Time.time;
                //startPos = _agent.Pos;

                _target = _siege.GetClosestEntity(_agent);
            }

            if (_agent.entity == null && _target != null && _target.hasView && _agent.Pos.y <= Mathf.Epsilon)
            {
                if (PETools.PEUtil.SqrMagnitude(_agent.Pos, _target.position, false) > 5.0f * 5.0f)
                {
                    Vector3 moveDir = _target.position - _agent.Pos;
                    moveDir.y = 0.0f;

                    _agent.Pos += (moveDir.normalized * moveDir.magnitude * 0.5f);
                }
            }

            if(_target != null && _target.hasView)
            {
                if (_agent.entity != null)
                {
                    if (!_agent.entity.hasView)
                    {
                        Vector3 fixedPos = PETools.PEUtil.GetRandomPositionOnGround(_target.position, 3.0f, 8.0f, false);
                        if (fixedPos != Vector3.zero)
                        {
                            _agent.entity.position = fixedPos;
                        }
                    }
                }
                else
                {
                    if (_agent.Step == SceneEntityPosAgent.EStep.Created && !death)
                    {
                        ApplyDamage(_hpMax + 100000f);
                    }
                    else
                    {
                        if (_agent.Pos.y > Mathf.Epsilon)
                        {
                            Vector3 fixedPos = PETools.PEUtil.GetRandomPositionOnGround(_target.position, 3.0f, 8.0f, false);
                            if (fixedPos != Vector3.zero)
                            {
                                _agent.Pos = fixedPos;
                                SceneMan.SetDirty(_agent);
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    bool CanFixedPos()
    {
        if(_agent.entity != null)
        {
            if (_agent.entity.bounds.size == Vector3.zero)
                return true;
        }
        else
        {
            if (_agent.Pos.y > Mathf.Epsilon)
                return true;
        }

        return false;
    }

    IEnumerator Attack()
    {
        while (!_death)
        {
            if (_agent.entity == null && _target != null && !_target.IsDeath())
            {
                AttackEntity(_target);
            }

            yield return new WaitForSeconds(Random.Range(5.0f, 10.0f));
        }
    }

    void AttackEntity(PeEntity entity)
    {
        float hp = entity.GetAttribute(AttribType.Hp);
        float hpMax = entity.GetAttribute(AttribType.HpMax);

        hp = Mathf.Clamp(hp - _atkMax, 0.0f, hpMax);

        entity.SetAttribute(AttribType.Hp, hp, false);
    }

    void OnSuceededToCreate()
    {
        if (_agent.entity != null)
        {
            _agent.entity.HPPercent = hpPercent;

            if (_agent.entity.aliveEntity != null)
            {
                _agent.entity.aliveEntity.deathEvent += OnDeath;
                _agent.entity.aliveEntity.onHpChange += OnHpChange;
            }
        }

        //Debug.LogError("Spawn entity : " + _agent.entity.name + " at position : " + _agent.Pos);
    }

    void OnFailedToCreate()
    {

    }

    void OnDeath(SkillSystem.SkEntity e1, SkillSystem.SkEntity e2)
    {
        _death = true;

        Clear();

        SceneMan.RemoveSceneObj(_agent);

        if (DeathEvent != null)
            DeathEvent(this);
    }

    void OnHpChange(SkillSystem.SkEntity skEntity, float damage)
    {
        if(_agent.entity != null)
            hpPercent = _agent.entity.HPPercent;
    }
}

public class MonsterSiege : MonoBehaviour
{
    bool m_SpawnFinished;
    CSMgCreator m_Creator;
    EntityMonsterBeacon m_Beacon;
	TowerInfoUIData m_UIData;

    List<SiegeAgent> m_Agents;

    List<PeEntity> m_Npcs;
    List<PeEntity> m_Towers;
    List<PeEntity> m_Buildings;
    List<PeEntity> m_Defences;
    List<PeEntity> m_Entities;

    bool m_IsReady;
	int m_KillCount;
	int m_MaxCount;
    float m_ElapsedTime;
    float m_RemainTime = -1.0f;

    public float   assemblyRadius      { get { return m_Creator.Assembly.Radius; } }
    public Vector3 assemblyPosition    { get { return m_Creator.Assembly.Position; } }

	public void SetCreator(CSMgCreator creator, TowerInfoUIData uiData)
    {
        m_Creator = creator;
		m_UIData = uiData;
    }

    public PeEntity GetClosestEntity(SceneEntityPosAgent agent)
    {
        PeEntity result = null;
        float minDis = Mathf.Infinity;
        for (int i = 0; i < m_Entities.Count; i++)
        {
            if(m_Entities[i] != null && !m_Entities[i].IsDeath())
            {
                float tmpDis = PETools.PEUtil.Magnitude(m_Entities[i].position, agent.Pos);
                if(tmpDis < minDis)
                {
                    minDis = tmpDis;
                    result = m_Entities[i];
                }
            }
        }

        return result;
    }

    public PeEntity GetRandomEntity()
    {
        if (m_Entities.Count > 0)
        {
            return m_Entities[Random.Range(0, m_Entities.Count)];
        }

        return null;
    }

    public PeEntity GetRandomEntityView()
    {
        if (m_Entities.Count > 0)
        {
            return m_Entities[Random.Range(0, m_Entities.Count)];
        }

        return null;
    }

    void OnEntitySpawned(SceneEntityPosAgent agent)
    {
		m_MaxCount ++;

		if (!EntityMonsterBeacon.IsBcnMonsterProtoId(agent.protoId))	// not encoded, unexpected agent
			return;

		int spType, lvl, spawnType;
		EntityMonsterBeacon.DecodeBcnMonsterProtoId (agent.protoId, out spType, out lvl, out spawnType);

        int areaType = PeGameMgr.IsStory 
            ? PeMappingMgr.Instance.GetAiSpawnMapId (new Vector2 (agent.Pos.x, agent.Pos.z)) 
            : AiUtil.GetMapID (agent.Pos);

		AISpawnTDWavesData.TDMonsterSpData data = AISpawnTDWavesData.GetMonsterSpData(false, spType, lvl, spawnType, areaType);

        float rhp = data != null ? data._rhp : 200.0f;
        float dps = data != null ? data._dps : 50.0f;

        SiegeAgent siegeAgent = new SiegeAgent(this, agent, rhp, dps);
        agent.spInfo = new SiegeAgent.AgentInfo(m_Beacon, siegeAgent);
        m_Agents.Add(siegeAgent);
    }

    void OnNewWave(AISpawnTDWavesData.TDWaveSpData spData, int idxWave)
    {
        m_IsReady = false;

        m_SpawnFinished = (idxWave >= spData._waveDatas.Count - 1);

        m_Npcs.Clear();
        m_Towers.Clear();
        m_Buildings.Clear();
        m_Defences.Clear();
        m_Entities.Clear();

        List<PeEntity> npcs = CSMain.GetCSNpcs(m_Creator);
        for (int i = 0; i < npcs.Count; i++)
        {
            if (npcs[i] != null && PETools.PEUtil.SqrMagnitude(npcs[i].position, assemblyPosition) < assemblyRadius * assemblyRadius)
                m_Npcs.Add(npcs[i]);
        }


        m_Towers = EntityMgr.Instance.GetTowerEntities(assemblyPosition, assemblyRadius, false);
        m_Buildings = CSMain.GetCSBuildings(m_Creator);

        m_Defences.AddRange(m_Npcs);
        m_Defences.AddRange(m_Towers);

        m_Entities.AddRange(m_Defences);
        m_Entities.AddRange(m_Buildings);
    }

    IEnumerator Defence()
    {
        while (true)
        {
            if (m_Beacon != null)
            {
                List<SiegeAgent> agents = m_Agents.FindAll(ret => !ret.hasView && !ret.death);
                for (int i = 0; i < m_Defences.Count; i++)
                {
                    if (!m_Defences[i].IsDeath() && !m_Defences[i].hasView)
                    {
                        List<SiegeAgent> tmpAgents = agents.FindAll(ret => PETools.PEUtil.SqrMagnitude(ret.position, assemblyPosition, false) <= assemblyRadius*assemblyRadius);
                        if (tmpAgents.Count > 0)
                        {
                            float atk = m_Defences[i].GetAttribute(AttribType.Atk);
                            tmpAgents[Random.Range(0, tmpAgents.Count)].ApplyDamage(atk);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(Random.Range(5.0f, 10.0f));
        }
    }

    IEnumerator WarningOne(int count, float intervals, float delayTime)
    {
        for (int i = 0; i < count; i++)
        {
            string content = !m_IsReady ? PELocalization.GetString(8000189) : PELocalization.GetString(8000190);
            PeTipMsg.Register(content, PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Colony, 800);
            yield return new WaitForSeconds(intervals);
        }

        yield return new WaitForSeconds(delayTime);
    }

    IEnumerator Warning()
    {
        yield return StartCoroutine(WarningOne(5, 12.0f, 60.0f));

        while (true)
        {
            if(m_Creator != null 
                && m_Creator.Assembly != null 
                && PeCreature.Instance.mainPlayer != null)
            {
                Vector3 v1 = m_Creator.Assembly.Position;
                Vector3 v2 = PeCreature.Instance.mainPlayer.position;
                float radius = m_Creator.Assembly.Radius;
                if (PETools.PEUtil.SqrMagnitude(v1, v2, false) < radius * radius)
                {
                    yield return StartCoroutine(WarningOne(5, 12.0f, 60.0f));
                }
            }

			yield return new WaitForSeconds(5.0f);
        }
        

    }

    void OnDeath(SiegeAgent agent)
    {
        m_Agents.Remove(agent);

        if(m_SpawnFinished && m_Agents.Count <= 0)
            GameObject.Destroy(gameObject);

		m_KillCount ++;
    }

    void Awake()
    {
        m_IsReady = true;

        m_Beacon = GetComponent<EntityMonsterBeacon>();

        m_Beacon.handlerNewWave     += OnNewWave;
        m_Beacon.handerNewEntity    += OnEntitySpawned;

        if (m_Creator != null)
            m_Creator.SetSiege(true);

        m_Agents = new List<SiegeAgent>();
        m_Npcs = new List<PeEntity>();
        m_Towers = new List<PeEntity>();
        m_Buildings = new List<PeEntity>();
        m_Defences = new List<PeEntity>();
        m_Entities = new List<PeEntity>();

        SiegeAgent.DeathEvent += OnDeath;

        StartCoroutine(Defence());
        StartCoroutine(Warning());
    }

	void Update() 
	{
        m_Entities = m_Entities.FindAll(ret => ret != null && !ret.IsDeath());

        if(!m_IsReady)
            m_ElapsedTime += Time.deltaTime;

        if (m_Beacon != null && m_Beacon.SpData._timeToDelete > PETools.PEMath.Epsilon)
        {
            m_RemainTime = m_Beacon.SpData._timeToDelete - m_ElapsedTime;

            if(m_RemainTime <= PETools.PEMath.Epsilon)
                m_Beacon.Delete();
        }

        if (m_UIData != null) 
		{
			m_UIData.CurCount = m_KillCount;
			m_UIData.MaxCount = m_MaxCount;

            if (m_RemainTime > -PETools.PEMath.Epsilon)
                m_UIData.RemainTime = m_RemainTime;
		}
	}

    void OnDestroy()
    {
        for (int i = 0; i < m_Agents.Count; i++)
            m_Agents[i].Clear();

        if(m_Beacon != null)
        {
            m_Beacon.handlerNewWave     -= OnNewWave;
            m_Beacon.handerNewEntity    -= OnEntitySpawned;
        }

        if (m_Creator != null)
            m_Creator.SetSiege(false);

        m_Agents.Clear();
        m_Npcs.Clear();
        m_Towers.Clear();
        m_Buildings.Clear();
        m_Defences.Clear();
        m_Entities.Clear();

        SiegeAgent.DeathEvent -= OnDeath;
    }
}
