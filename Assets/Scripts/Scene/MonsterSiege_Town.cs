using UnityEngine;
using System.Collections;
using Pathea;

public class MonsterSiege_Town : MonoBehaviour
{
    public static MonsterSiege_Town Instance;

    public float minHour;
    public float maxHour;
    public float perCheckTime;
    public float probability;

    int m_CurSiegeID;
    EntityMonsterBeacon m_MonsterSiege;
    VArtifactTown m_Town;
    TowerInfoUIData m_UIData;
    bool m_SpawnFinished;
    bool m_IsReady;
    float m_ElapsedTime;

    VArtifactTown m_RecordTown;

    public void OnNewTown(VArtifactTown town)
    {
        if(town != null && town.ms_id > 0)
        {
            m_RecordTown = town;
        }
    }

    void SetSiegeID(int id)
    {
        if(m_CurSiegeID == 0 && m_MonsterSiege == null)
        {
			m_Town.SetMsId(id);
            m_CurSiegeID = id;

            m_IsReady = false;
            m_SpawnFinished = false;
            m_ElapsedTime = 0.0f;

            m_UIData = new TowerInfoUIData();
            m_MonsterSiege = EntityMonsterBeacon.CreateMonsterBeaconByTDID(m_CurSiegeID, null, m_UIData);
            m_MonsterSiege.transform.position = m_Town.TransPos;
            m_MonsterSiege.TargetPosition = m_Town.TransPos;
            m_MonsterSiege.handlerOneDeath += OnMemberDeath;
            m_MonsterSiege.handerNewEntity += OnMemberCreated;
            m_MonsterSiege.handlerNewWave += OnNewWave;
        }
    }

    int GetLevel(VArtifactTown town)
    {
        return Mathf.Clamp(town.level, 1, 5);
    }

    void OnMemberDeath()
    {
        //m_UIData.CurCount++;

        //if(m_SpawnFinished && m_UIData.CurCount == m_UIData.MaxCount)
        //    GameObject.DestroyImmediate(m_MonsterSiege.gameObject);
    }

    void OnMemberCreated(SceneEntityPosAgent agent)
    {
        m_UIData.MaxCount++;
    }

    void OnNewWave(AISpawnTDWavesData.TDWaveSpData data, int idxWave)
    {
        m_IsReady = true;
        m_SpawnFinished = (idxWave >= data._waveDatas.Count - 1);
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (PeCreature.Instance.mainPlayer == null || UITowerInfo.Instance == null)
            return;

        if(m_RecordTown != null)
        {
            m_Town = m_RecordTown;
            SetSiegeID(m_RecordTown.ms_id);

            m_RecordTown = null;
        }

        if (m_MonsterSiege != null && m_SpawnFinished && m_UIData.CurCount == m_UIData.MaxCount)
            GameObject.DestroyImmediate(m_MonsterSiege.gameObject);

        if (m_CurSiegeID > 0 && m_MonsterSiege == null)
        {
            m_CurSiegeID = 0;
            m_SpawnFinished = false;

            if (m_Town != null)
                m_Town.SetMsId(0);
        }

        if (m_IsReady)
            m_ElapsedTime += Time.deltaTime;

        if (m_MonsterSiege != null && m_MonsterSiege.SpData._timeToDelete > PETools.PEMath.Epsilon)
        {
            float remainTime = Mathf.Max(0.0f, m_MonsterSiege.SpData._timeToDelete - m_ElapsedTime);

            if (m_UIData != null)
                m_UIData.RemainTime = remainTime;

            if (remainTime <= PETools.PEMath.Epsilon)
                m_MonsterSiege.Delete();
        }

        if (!EntityMonsterBeacon.IsRunning() && m_CurSiegeID == 0 && m_MonsterSiege == null)
        {
            m_Town = VArtifactTown.GetStandTown(PeCreature.Instance.mainPlayer.position);

            if (m_Town != null)
            {
                if (m_Town.lastHour < PETools.PEMath.Epsilon || m_Town.nextHour < PETools.PEMath.Epsilon)
                    m_Town.RandomSiege(minHour, maxHour);
#if false
				if(Input.GetKey(KeyCode.L)){
					m_Town.lastHour = 0;
					m_Town.lastCheckTime = 0;
					probability = 1.1f;
				}				
#endif
                if (GameTime.Timer.Hour - m_Town.lastHour >= m_Town.nextHour)
                {
                    if (Time.time - m_Town.lastCheckTime >= perCheckTime)
                    {
                        if (Random.value < probability)
                        {
                            SetSiegeID(GetLevel(m_Town));
                            m_Town.RandomSiege(minHour, maxHour);
                        }

                        m_Town.lastCheckTime = Time.time;
                    }
                }
            }
        }
    }
}
