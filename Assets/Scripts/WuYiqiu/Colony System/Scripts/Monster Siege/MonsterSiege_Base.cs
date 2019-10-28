using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class MonsterSiege_Base : MonoBehaviour
{
    public static bool MonsterSiegeBasePause = false;

    [SerializeField] CSBuildingLogic assembly;
    [SerializeField] float minHours;
    [SerializeField] float maxHours;

    int _lvl;
    float _lastHour;
    float _nextHour;

    bool _Init;

    PETimer m_Timer;
    CSDataMonsterSiege m_Data;
    EntityMonsterBeacon m_Beacon;
	TowerInfoUIData m_UIData;

    int lvl         { set { _lvl        = value; Export(); } }
    float lastHour    { set { _lastHour   = value; Export(); } }
    float nextHour    { set { _nextHour   = value; Export(); } }

    void Init()
    {
        if(!_Init)
        {
            _Init = true;

            m_Timer = assembly.m_Entity.m_Creator.Timer;
            m_Data  = assembly.m_Entity.m_Creator.m_DataInst.m_Siege;

            assembly.m_Entity.AddEventListener(OnEntityEventListener);

            Import();

            if (_nextHour == 0) CreateNextSiege();
        }
    }

    void Import()
    {
        _lvl        = m_Data.lvl;
        _lastHour   = m_Data.lastHour;
        _nextHour   = m_Data.nextHour;

        CreateMonsterBeacon();
    }

    void Export()
    {
        m_Data.lvl      = _lvl;
        m_Data.lastHour = _lastHour;
        m_Data.nextHour = _nextHour;
    }

    void CreateNextSiege()
    {
        lastHour = (int)m_Timer.Hour;
        nextHour = Random.Range(minHours, maxHours);
    }

    void CreateMonsterBeacon()
    {
        if (_lvl > 0 && assembly != null && assembly.m_Entity != null)
        {
            if (m_Beacon != null)
                m_Beacon.Delete();

			m_UIData = new TowerInfoUIData();
			m_Beacon = EntityMonsterBeacon.CreateMonsterBeaconByTDID(_lvl, transform, m_UIData);
            m_Beacon.gameObject.AddComponent<MonsterSiege>().SetCreator(assembly.m_Entity.m_Creator as CSMgCreator, m_UIData);
        }
    }

    void CalculateLvl()
    {
        int level = 0;

        if (assembly != null && assembly.m_Entity != null && (assembly.m_Entity is CSAssembly))
            level = (assembly.m_Entity as CSAssembly).Level;

        lvl = Mathf.Clamp(Random.Range(level, level + 3), 1, 5);

        CreateMonsterBeacon();

        CreateNextSiege();
    }

    void OnEntityEventListener(int event_id, CSEntity entity, object arg)
    {
        if(event_id == CSConst.eetDestroy)
        {
            lvl = 0;
            lastHour = 0.0f;
            nextHour = 0.0f;

            if (m_Beacon != null)
                m_Beacon.Delete();
        }
    }
	
    void Update()
    {
        if (MonsterSiegeBasePause || PeGameMgr.IsMulti || EntityMonsterBeacon.IsRunning() || PeGameMgr.IsBuild)
            return;

        if (assembly == null || assembly.m_Entity == null || !(assembly.m_Entity is CSAssembly))
            return;

        Init();

        if (_lvl > 0 && m_Beacon == null)
            lvl = 0;

        if (m_Timer.Hour - _lastHour >= _nextHour)
            CalculateLvl();
    }

    void OnDestroy()
    {
        if (m_Beacon != null)
            m_Beacon.Delete();

        if(assembly != null && assembly.m_Entity != null)
            assembly.m_Entity.AddEventListener(OnEntityEventListener);
    }
}
