using UnityEngine;
using System.Collections;
using Pathea;

public class MonsterSiege_Camp : MonoBehaviour
{
    public float minHour;
    public float maxHour;
    public float minMinute;
    public float maxMinute;
    public float probability;
    public float maxFailedCount;

    float m_CurHour;
    float m_LastHour;

    float m_RandTime;
    float m_LastRandomTime;

    int m_FailedCout;

    EntityMonsterBeacon m_Beacon;

    void TryCreateMonsterSiege()
    {
        Camp camp = Camp.GetCamp(PeCreature.Instance.mainPlayer.position);
        if(m_Beacon == null && camp != null)
        {
            if(Time.time - m_LastRandomTime > m_RandTime * 60.0f)
            {
                m_LastRandomTime = Time.time;
                m_RandTime = Random.Range(minMinute, maxMinute);

                if (Random.value > probability)
                    m_FailedCout++;
                else
                {
                    m_Beacon = EntityMonsterBeacon.CreateMonsterBeaconByTDID(1, null, null);
                    m_Beacon.TargetPosition = camp.Pos;
                    m_Beacon.transform.position = camp.Pos;
                    m_Beacon.handlerNewWave += OnNewWave;

                    m_FailedCout = 0;
                }
            }
        }
    }

    void OnNewWave(AISpawnTDWavesData.TDWaveSpData spData, int idxWave)
	{
        if(idxWave == spData._waveDatas.Count - 1)
        {
            if(m_Beacon != null)
            {
                m_Beacon.handlerNewWave -= OnNewWave;
                GameObject.Destroy(m_Beacon.gameObject);
            }
        }
	}

    void Awake()
    {
        m_LastHour = (float)GameTime.Timer.Hour;
        m_CurHour = Random.Range(minHour, maxHour);

        m_LastRandomTime = Time.time;
        m_RandTime = Random.Range(minMinute, maxMinute);
    }

    void Update()
    {
        if (PeCreature.Instance.mainPlayer == null)
            return;

        if (GameTime.Timer.Hour - m_LastHour < m_CurHour)
            return;

        if (m_Beacon != null || m_FailedCout >= maxFailedCount)
        {
            m_LastHour = (float)GameTime.Timer.Hour;
            m_CurHour = Random.Range(minHour, maxHour);
            return;
        }

        TryCreateMonsterSiege();
    }
}
