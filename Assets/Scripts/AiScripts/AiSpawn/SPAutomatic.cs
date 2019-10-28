using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void AutomaticDelagate(SPAutomatic atuo);

public class SPAutomatic : CommonInterface, ITowerDefenceData
{
    public static List<SPAutomatic> autos = new List<SPAutomatic>();

    //public event AutomaticDelagate ActiveEvent;
    //public event AutomaticDelagate DeActiveEvent;

    Transform mPointParent;

    AISpawnWaveData mCurrentWave;
    AISpawnAutomaticData mCurrentAutomatic;

    int mID;
    float mDelay;

    int mKilledCount;
    int mTotalCount;
    //int mWaveIndex;
    float mStartTime;
    float mDelayTime;

    bool mGrounded;
    bool mSpawning;

    List<AiObject> mAiObjs = new List<AiObject>();

    public static bool IsSpawning()
    {
        foreach (SPAutomatic auto in autos)
        {
            if (auto != null && auto.Spawning)
                return true;
        }

        return false;
    }

    public static SPTowerDefence GetTowerDefence(int missionID)
    {
        foreach (SPAutomatic auto in autos)
        {
            SPTowerDefence td = auto as SPTowerDefence;
            if (td != null && td.MissionID == missionID)
                return td;
        }

        return null;
    }

    public List<AiObject> aiObjs { get { return mAiObjs; } }

    public int ID { get { return mID; } set { mID = value; } }
    public float Delay { get { return mDelay; } set { mDelay = value; } }

    public bool Grounded { get { return mGrounded; } }
    public bool Spawning { get { return mSpawning; } }

    public bool Begin{ get { return mCurrentWave != null; } }
    public float DelayTime { get { return currentDelayTime; } }
    public int KilledCount{ get { return mKilledCount; } }
    public int TotalCount{ get {return mTotalCount; } }
    public float currentDelayTime { get { return Mathf.Max(0.0f, mDelayTime + mStartTime - Time.time); } }

    public AISpawnAutomaticData automaticData
    {
        get { return mCurrentAutomatic; }
    }

    public Transform pointParent
    {
        get
        {
            if (mPointParent == null)
            {
                mPointParent = new GameObject("Points").transform;
                mPointParent.parent = transform;
            }

            return mPointParent;
        }
    }

    public void SyncTotalCount(int argCount)
    {
        mTotalCount = argCount;
    }

    public void SyncKilledCount(int argCount)
    {
		mKilledCount = argCount;
    }

    public void SyncWave(float time, float delay, int index)
    {
        mStartTime = time;
        mDelayTime = delay;
        //mWaveIndex = index;

        if (mCurrentAutomatic != null)
        {
            mCurrentWave = mCurrentAutomatic.GetWaveData(index);
        }
    }

    public virtual void OnSpawned(GameObject obj)
    {
        if (obj == null)
            return;

        obj.transform.parent = transform;

        AiObject aiObj = obj.GetComponent<AiObject>();
        if (aiObj != null)
        {
            aiObj.DeathHandlerEvent += OnDeath;

            if (!mAiObjs.Contains(aiObj))
                mAiObjs.Add(aiObj);
        }
    }

    protected virtual void OnDeath(AiObject aiObj)
    {
        if (aiObj == null)
            return;

        mKilledCount++;
    }

    protected virtual void OnSpawnStart()
    {
    }

    protected virtual void OnSpawnComplete()
    {
    }

    protected virtual SPPoint Spawn(AISpawnData spData)
    {
        return null;
    }

    protected virtual void SpawnWave(AISpawnWaveData wave)
    {
        
    }

    public void SpawnAutomatic(int index = -1)
    {
        AISpawnAutomaticData auto = AISpawnAutomaticData.CreateAutomaticData(mDelay, mID);

        if (auto != null)
            StartCoroutine(SpawnCoroutine(auto, index));
        else
            Debug.LogError("Can't fin automatic data!");
    }

    public void StopAutomatic()
    {
        mStartTime = 0.0f;
        mDelayTime = 0.0f;
        //mWaveIndex = -1;
        mSpawning = false;
        mCurrentWave = null;
        mCurrentAutomatic = null;

        HideAutomaticGUI();

        StopAllCoroutines();
    }

    public void ShowAutomaticGUI()
    {
//        if (GameGui_N.Instance != null && GameGui_N.Instance.mLifeShowGui != null)
//        {
//            GameGui_N.Instance.mLifeShowGui.Activate(null, false);
//            GameGui_N.Instance.mLifeShowGui.Activate(this, true);
//        }
    }

    public void HideAutomaticGUI()
    {
//        if (GameGui_N.Instance != null && GameGui_N.Instance.mLifeShowGui != null)
//        {
//            GameGui_N.Instance.mLifeShowGui.Activate(null, false);
//        }
    }

    IEnumerator SpawnCoroutine(AISpawnAutomaticData auto, int index)
    {
        mStartTime = Time.time;
        //mWaveIndex = -1;
        mSpawning = true;
        mCurrentWave = null;
        mCurrentAutomatic = auto;

        mTotalCount = 0;

        ShowAutomaticGUI();

        //if (GameConfig.IsMultiMode && IsController)
        //    RPC("RPC_C2S_Start");

        OnSpawnStart();

        if(index <= -1)
        {
            mDelayTime = auto.delayTime;

            if (GameConfig.IsMultiMode && IsController)
                RPCServer(EPacketType.PT_TD_Wave, mDelayTime, -1);

            yield return new WaitForSeconds(auto.delayTime);
        }

        foreach (AISpawnWaveData wave in auto.data.data)
        {
            Debug.LogWarning("wave index = " + wave.index + " : " + "index = " + index);
            if (wave.index < index)
                continue;

            mDelayTime = wave.delayTime;

            if (index > -1 && wave.index == index)
                mDelayTime = 0.0f;

            if (GameConfig.IsMultiMode && IsController)
                RPCServer(EPacketType.PT_TD_Wave, mDelayTime, wave.index);

            mStartTime = Time.time;
            mCurrentWave = wave;
            //mWaveIndex = wave.index;

            yield return new WaitForSeconds(mDelayTime);

			SpawnWave(wave);

            foreach (AISpawnData sp in wave.data.data)
            {
                int number = Random.Range(sp.minCount, sp.maxCount + 1);

                mTotalCount += number;

                for (int i = 0; i < number; i++)
                {
                    SPTerrainEvent.instance.RegisterSPPoint(Spawn(sp));

                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        mStartTime = 0.0f;
        mDelayTime = 0.0f;
        //mWaveIndex = -1;
        mSpawning = false;
        mCurrentWave = null;
        mCurrentAutomatic = null;

        HideAutomaticGUI();

        OnSpawnComplete();

        if (IsController)
            RPCServer(EPacketType.PT_TD_End);
    }

    public void Delete(float delayTime = 0.0f)
    {
        GameObject.Destroy(gameObject, delayTime);
    }

    bool Match(IntVector4 node)
    {
        float dx = transform.position.x - node.x;
        float dy = transform.position.y - node.y;
        float dz = transform.position.z - node.z;

        return dx >= PETools.PEMath.Epsilon && dx <= VoxelTerrainConstants._numVoxelsPerAxis << node.w
            && dy >= PETools.PEMath.Epsilon && dy <= VoxelTerrainConstants._numVoxelsPerAxis << node.w
            && dz >= PETools.PEMath.Epsilon && dz <= VoxelTerrainConstants._numVoxelsPerAxis << node.w;
    }

    protected virtual void OnTerrainEnter(IntVector4 node)
    {
        mGrounded = true;
    }

    protected virtual void OnTerrainExit(IntVector4 node)
    {
        mGrounded = false;

		if (IsController)
		{
			OnSpawnComplete();
			StopAllCoroutines();
			RPCServer(EPacketType.PT_TD_End);
		}
    }

    void OnTerrainColliderCreated(IntVector4 node)
    {
        if (Match(node)) OnTerrainEnter(node);
    }

    void OnTerrainColliderDestroy(IntVector4 node)
    {
        if (Match(node)) OnTerrainExit(node);
    }

    public void Awake()
    {
        autos.Add(this);
    }

    public void Start()
    {
        //foreach (IntVector4 node in AiManager.Manager.terrainColliders)
        //{
        //    if (Match(node)) OnTerrainEnter(node);
        //}

        LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated,  null);
    }

    public void OnDestroy()
    {
        foreach (AiObject aiObj in mAiObjs)
        {
            if (aiObj != null)
            {
                aiObj.DeathHandlerEvent -= OnDeath;
            }
        }

        HideAutomaticGUI();

		if (LODOctreeMan.self != null) {
			LODOctreeMan.self.DetachEvents (null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
		}

        autos.Remove(this);
    }
}
