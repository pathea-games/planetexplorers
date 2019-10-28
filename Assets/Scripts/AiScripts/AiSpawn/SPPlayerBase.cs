using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SPPlayerBase : SPAutomatic
{
    [System.Serializable]
    public class BaseTimer
    {
        public delegate void OnHourReadyDelegate(BaseTimer timer);
        public static event OnHourReadyDelegate OnHourReadyEvent;

        public int minHour;
        public int maxHour;
        public int id;

        int mStartHour;
        int mCurrectWave;
        bool mDirty;

        float mDelayTime;
        int mSpawnID;

        public float delayTime { get { return mDelayTime; } }
        public int spawnId { get { return mSpawnID; } }

        public int currentWave 
        { 
          get { return mCurrectWave; } 
          set { mCurrectWave = value; } 
        }

        public void Export(BinaryWriter _out, int version)
        {
            _out.Write(minHour);
            _out.Write(maxHour);
            _out.Write(id);

            _out.Write(mStartHour);
            _out.Write(mCurrectWave);
            _out.Write(mDirty);

            if (version == 2)
            {
                _out.Write(mSpawnID);
                _out.Write(mDelayTime);
            }
        }

        public void Import(BinaryReader _in, int version)
        {
            minHour = _in.ReadInt32();
            maxHour = _in.ReadInt32();
            id = _in.ReadInt32();
            mStartHour = _in.ReadInt32();
            mCurrectWave = _in.ReadInt32();
            mDirty = _in.ReadBoolean();

            if (version == 2)
            {
                mSpawnID = _in.ReadInt32();
                mDelayTime = _in.ReadSingle();
            }
        }
        
        public override bool Equals(object obj)
        {
            BaseTimer timer = (BaseTimer)obj;

            if (timer == null)
                return false;
            else
                return id == timer.id && minHour == timer.minHour && maxHour == timer.maxHour;
        }

		//clear warnings
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

        public void SetStartHour()
        {
            mDirty = true;
            mCurrectWave = -1;
            mStartHour = Random.Range(minHour, maxHour - 5);

            AISpawnPlayerBase mData = AISpawnPlayerBase.GetRandomPlayerBase(id, 1, 1);
            if (mData != null)
            {
                mDelayTime = mData.delayTime;
                mSpawnID = mData.spawnID;
            }
        }

        public void SyncTimer(BaseTimer timer)
        {
            mDirty = true;
            mCurrectWave = timer.mCurrectWave;
            mStartHour = timer.mStartHour;

            mSpawnID = timer.spawnId;
            mDelayTime = timer.delayTime;
        }

        public void ClearStartHour()
        {
            mDirty = false;
            mDelayTime = 0.0f;
            mSpawnID = 0;
            mCurrectWave = -1;
            mStartHour = -1;
        }

        public void OnHourTick(int hour)
        {
            if (mDirty && hour >= mStartHour && !SPAutomatic.IsSpawning())
            {
                if (OnHourReadyEvent != null)
                {
                    OnHourReadyEvent(this);
                }
                mDirty = false;
            }
        }
    }

    const int RecordVersion = 2;
    public static event AssetReq.ReqFinishDelegate OnSpawnedEvent;

    [HideInInspector]
    public float damage;
    public float minRadius;
    public float maxRadius;
    public float minInterval;
    public float maxInterval;
    [HideInInspector]
    public float damageRadius;

    public int cycleHours;
    public BaseTimer[] timers;

    BaseTimer mCurrentTimer;
    BaseTimer mLastTimer;

    BaseTimer mRecordTimer;

    List<SPPointSimulate> simulates = new List<SPPointSimulate>();

    static SPPlayerBase mSinglePlayerBase;

    public static SPPlayerBase Single { get { return mSinglePlayerBase; } }

    public void ApplySimulateDamage(float damage)
    {
        List<SPPointSimulate> points = simulates.FindAll(ret => ret != null && ret.isDamage);
		if (points == null || points.Count == 0)
			return;
        points[Random.Range(0, points.Count)].ApplyDamage(damage);
    }

    public void ApplySimulateDamage(float damage, Vector3 position, float radius)
    {
        List<SPPointSimulate> points = simulates.FindAll(ret => ret != null 
                                                                && ret.isDamage 
                                                                && (ret.position - position).sqrMagnitude <= radius * radius);
		if (points == null || points.Count == 0)
			return;
        points[Random.Range(0, points.Count)].ApplyDamage(damage);
    }

	public void Export(BinaryWriter bw)
    {
        bw.Write(RecordVersion);

        if (mCurrentTimer == null)
            bw.Write(0);
        else
        {
            FixWaveIndex();

            bw.Write(1);
            mCurrentTimer.Export(bw, RecordVersion);
        }
    }

    public void Import(byte[] buffer)
    {
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);

        int version = _in.ReadInt32();

        int mark = _in.ReadInt32();
        if (mark == 1)
        {
            mRecordTimer = new BaseTimer();
            mRecordTimer.Import(_in, version);
        }

        _in.Close();
        ms.Close();
    }

    void FixWaveIndex()
    {
        if (mCurrentTimer != null)
        {
            AISpawnAutomaticData auto = AISpawnAutomaticData.CreateAutomaticData(0.0f, mCurrentTimer.spawnId);
            if (auto != null)
            {
                if (mCurrentTimer.currentWave == auto.data.data.Count - 1)
                {
                    if (GetComponentsInChildren<SPPointSimulate>().Length == 0)
                    {
                        mCurrentTimer.currentWave = auto.data.data.Count;
                    }
                }
            }
        }
    }

    protected override void SpawnWave(AISpawnWaveData wave)
    {
        base.SpawnWave(wave);

        if (mCurrentTimer != null)
        {
            mCurrentTimer.currentWave = wave.index;
        }

        Debug.Log("Current wave index = " + wave.index);

        //foreach (Transform tr in pointParent)
        //{
        //    SPPointSimulate simulate = tr.GetComponent<SPPointSimulate>();
        //    if (simulate != null)
        //    {
        //        if (simulates.Contains(simulate))
        //        {
        //            simulates.Remove(simulate);
        //        }

        //        simulate.Delete();
        //    }
        //}
    }

    protected override SPPoint Spawn(AISpawnData spData)
    {
        base.Spawn(spData);

        Vector3 pos;
        Quaternion rot;
        Vector3 target;
        if (GetPositionAndRotation(spData, out pos, out rot, out target))
        {
            SPPointSimulate simulate = SPPoint.InstantiateSPPoint<SPPointSimulate>( pos, 
                                                                                    rot, 
                                                                                    IntVector4.Zero,
                                                                                    pointParent,
                                                                                    spData.isPath ? 0 : spData.spID,
                                                                                    spData.isPath ? spData.spID : 0,
                                                                                    true,
                                                                                    true,
                                                                                    false,
                                                                                    false,
                                                                                    true,
                                                                                    null,
                                                                                    OnSpawned,
                                                                                    this) as SPPointSimulate;

            simulate.SetData(damage, minInterval, maxInterval, damageRadius);
            simulate.targetPos = target;

            if (!simulates.Contains(simulate))
            {
                simulates.Add(simulate);
            }

            return simulate;
        }

        return null;
    }

    Transform GetAttackTransform()
    {
        List<Transform> trs = new List<Transform>();

        if (CSMain.s_MgCreator.Assembly != null && CSMain.s_MgCreator.Assembly.gameObject != null)
            trs.Add(CSMain.s_MgCreator.Assembly.gameObject.transform);

        foreach (KeyValuePair<int, CSCommon> kvp in CSMain.s_MgCreator.GetCommonEntities())
        {
            if (kvp.Value.gameObject != null)
                trs.Add(kvp.Value.gameObject.transform);
        }

        if (trs.Count > 0)
            return trs[Random.Range(0, trs.Count)];
        else
            return null;
    }

    protected override void OnSpawnStart()
    {
        base.OnSpawnStart();

        GameTime.Lock(gameObject);

        simulates.Clear();
    }

    protected override void OnSpawnComplete()
    {
        base.OnSpawnComplete();

        GameTime.UnLock(gameObject);
    }

    public override void OnSpawned(GameObject obj)
    {
        base.OnSpawned(obj);

        AiObject aiObj = obj.GetComponent<AiObject>();
        if (aiObj != null)
        {
            aiObj.tdInfo = GetAttackTransform();
        }

        SPGroup spGroup = obj.GetComponent<SPGroup>();
        if (spGroup != null)
        {
            spGroup.tdInfo = GetAttackTransform();
        }

        if (OnSpawnedEvent != null)
        {
            OnSpawnedEvent(aiObj.gameObject);
        }
    }

    IEnumerator CheckAttacktransform()
    {
        while (true)
        {
            foreach (AiObject aiObj in aiObjs)
            {
                if (aiObj != null && aiObj.tdInfo == null)
                {
                    if (CSMain.s_MgCreator.Assembly != null && CSMain.s_MgCreator.Assembly.Data.m_Durability > PETools.PEMath.Epsilon)
                        aiObj.tdInfo = GetAttackTransform();
                }
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

    Vector3 GetRandom()
    {
        Dictionary<int, CSCommon> commons = CSMain.s_MgCreator.GetCommonEntities();
        List<int> keys = new List<int>(commons.Keys);

        if(commons != null && commons.Count > 0)
        {
            return commons[keys[Random.Range(0, keys.Count)]].Position;
        }

        if (CSMain.s_MgCreator.Assembly != null)
            return CSMain.s_MgCreator.Assembly.Position;

        return Vector3.zero;
    }

    bool IsInCSCommon(Vector3 position)
    {
        foreach (KeyValuePair<int, CSCommon> kp in CSMain.s_MgCreator.GetCommonEntities())
        {
            Vector3 newPos = new Vector3(position.x, kp.Value.Position.y + 0.5f, position.z);
            if (kp.Value.ContainPoint(newPos))
                return true;
        }

        return false;
    }

    IntVector4 GetNode(IntVector4 node)
    {
        int px = 0;
        int mx = 0;
        int pz = 0;
        int mz = 0;

        //foreach (KeyValuePair<IntVector2, List<IntVector4>> kvp in AiManager.Manager.colliders)
        //{
        //    px = Mathf.Max(kvp.Key.x - node.x, px);
        //    mx = Mathf.Min(kvp.Key.x - node.x, mx);
        //    pz = Mathf.Max(kvp.Key.y - node.z, pz);
        //    mz = Mathf.Min(kvp.Key.y - node.z, mz);
        //}

        for (int i = 0; i < 5; i++)
        {
            int tmp = Random.Range(0, 4);

            switch (tmp)
            {
                case 0:
                    if ((px >> 5) > 0)
                    {
                        return new IntVector4(node.x + px, node.y, node.z, node.w);
                    }
                    break;
                case 1:
                    if ((mx >> 5) < 0)
                    {
                        return new IntVector4(node.x + mx, node.y, node.z, node.w);
                    }
                    break;
                case 2:
                    if ((pz >> 5) > 0)
                    {
                        return new IntVector4(node.x, node.y, node.z + pz, node.w);
                    }
                    break;
                case 3:
                    if ((mz >> 5) < 0)
                    {
                        return new IntVector4(node.x, node.y, node.z + mz, node.w);
                    }
                    break;
                default:
                    break;
            }
        }

        return node;
    }

    Vector3 GetPosition(IntVector4 node)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 pos = AiUtil.GetRandomPosition(node);
            if (!IsInCSCommon(pos))
            {
                return pos;
            }
        }

        return Vector3.zero;
    }

    Vector3 GetPositionFromCollider(IntVector4 node)
    {
        //IntVector2 vec2 = new IntVector2(node.x, node.z);

        //if (AiManager.Manager.colliders.ContainsKey(vec2))
        //    return GetPosition(GetNode(node));
        //else
            return GetPosition(node);
    }

    Vector3 GetRandomPosition(AISpawnData data, Vector3 center)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 pos = AiUtil.GetRandomPosition(center, minRadius, maxRadius, Vector3.forward, data.minAngle, data.maxAngle);
            if (!IsInCSCommon(pos))
            {
                return pos;
            }
        }

        return Vector3.zero;
    }

    bool GetPositionAndRotation(AISpawnData data, out Vector3 pos, out Quaternion rot, out Vector3 target)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;
        target = Vector3.zero;

        Vector3 point = GetRandom();

        if (point != Vector3.zero)
        {
            IntVector4 node = AiUtil.ConvertToIntVector4(point, 0);
            //IntVector2 vec2 = new IntVector2(node.x, node.z);

            //if (AiManager.Manager.colliders.ContainsKey(vec2))
            //{
            //    Vector3 center = point;
            //    if (CSMain.s_MgCreator.Assembly != null)
            //        center = CSMain.s_MgCreator.Assembly.Position;

            //    pos = GetRandomPosition(data, center);
            //    target = point;
            //}
            //else
            {
                pos = GetPosition(node);
                target = Vector3.zero;
            }

            //pos = GetPositionFromCollider(node);

            if (pos != Vector3.zero)
            {
                rot = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
                return true;
            }
        }

        return false;
    }

    int GetCurrentHour()
    {
        return (int)CSMain.s_MgCreator.Timer.Hour % cycleHours;
    }

    void RegisterEvent()
    {
        BaseTimer.OnHourReadyEvent += OnBaseTimerReady;
        LODOctreeMan.self.AttachEvents(OnTerrainMeshCreated, null, null, null);
    }

    void RemoveEvent()
    {
        BaseTimer.OnHourReadyEvent -= OnBaseTimerReady;
        LODOctreeMan.self.DetachEvents(OnTerrainMeshCreated, null, null, null);

    }

    BaseTimer GetCurrentTimer()
    {
        foreach (BaseTimer ite in timers)
        {
            if (GetCurrentHour() >= ite.minHour &&  GetCurrentHour() <= ite.maxHour)
            {
                return ite;
            }
        }

        return null;
    }

    void UpdateBaseTimer()
    {
        BaseTimer timer = GetCurrentTimer();

        if (mCurrentTimer != timer)
        {
            if (mCurrentTimer != null)
                OnBaseTimerExit(mCurrentTimer);

            mCurrentTimer = timer;

            if (mCurrentTimer != null)
                OnBaseTimerEnter(mCurrentTimer);
        }

        if(mCurrentTimer != null)
            OnBaseTimerTick(mCurrentTimer);
    }

    void UpdateAssemblyHP()
    {
        if (automaticData != null)
        {
            if (CSMain.s_MgCreator.Assembly == null || CSMain.s_MgCreator.Assembly.Data.m_Durability <= PETools.PEMath.Epsilon)
            {
                StopAutomatic();
            }
        }
    }

    void OnBaseTimerEnter(BaseTimer timer)
    {
        if(mRecordTimer != null && mRecordTimer.Equals(timer))
            timer.SyncTimer(mRecordTimer);
        else
            timer.SetStartHour();
    }

    void OnBaseTimerExit(BaseTimer timer)
    {
        timer.ClearStartHour();
    }

    void OnBaseTimerTick(BaseTimer timer)
    {
        timer.OnHourTick(GetCurrentHour());
    }

    void OnBaseTimerReady(BaseTimer timer)
    {
        if(CSMain.s_MgCreator.Assembly != null && CSMain.s_MgCreator.Assembly.Data.m_Durability > PETools.PEMath.Epsilon)
        {
            if(timer.spawnId > 0)
            {
                ID = timer.spawnId;
                Delay = timer.delayTime;
                SpawnAutomatic(timer.currentWave);

                GameTime.ClearTimerPass();
            }
        }
    }

    void OnTerrainMeshCreated(IntVector4 node)
    {
        if (CSMain.s_MgCreator.Assembly == null)
            return;

        if (!VFVoxelTerrain.TerrainColliderComplete)
            return;

        if(node.w == 0 && automaticData != null)
        {
            float dx = CSMain.s_MgCreator.Assembly.Position.x - node.x;
            float dy = CSMain.s_MgCreator.Assembly.Position.y - node.y;
            float dz = CSMain.s_MgCreator.Assembly.Position.z - node.z;

            float side = VoxelTerrainConstants._numVoxelsPerAxis << node.w;

            if(dx >= PETools.PEMath.Epsilon && dx <= side
                && dy >= PETools.PEMath.Epsilon && dy <= side
                && dz >= PETools.PEMath.Epsilon && dz <= side)
            {
                //To do : destroy block
                foreach (KeyValuePair<int, CSCommon> kp in CSMain.s_MgCreator.GetCommonEntities())
                {
                    if (Random.value < 0.5f)
                    {
                        DigTerrainManager.DestroyTerrainInRange(1, kp.Value.Position, 255.0f, 10.0f);
                    }
                }
            }
        }
    }

    new public void Awake()
    {
        base.Awake();

        if (mSinglePlayerBase == null)
            mSinglePlayerBase = this;
        else
            Debug.LogError("Have too many SPPlayerBase!!");
    }

    new public void Start()
    {
        base.Start();

        RegisterEvent();

        StartCoroutine(CheckAttacktransform());

        //if (Record.m_RecordBuffer.ContainsKey((int)Record.RecordIndex.RECORD_PLAYERBASE))
        //{
        //    Import(Record.m_RecordBuffer[(int)Record.RecordIndex.RECORD_PLAYERBASE]);
        //}
    }

    void Update()
    {
        UpdateBaseTimer();

        UpdateAssemblyHP();
    }

    new public void OnDestroy()
    {
        base.OnDestroy();

        RemoveEvent();

        GameTime.UnLock(gameObject);
    }
}
