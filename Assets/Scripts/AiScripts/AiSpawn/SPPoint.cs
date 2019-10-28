using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum PointType
{
    PT_NULL,
    PT_Ground,
    PT_Water,
    PT_Cave,
    PT_Slope,
    PT_MAX
}

public class SPPoint : MonoBehaviour
{
    //static int s_Count = 0;

    static Dictionary<IntVector4, SPPoint> PointTable = new Dictionary<IntVector4, SPPoint>();

    public PointType mType;

    int mSpID;
    int mPathID;
    bool mActive;
    bool mIsBoss;
    bool mErode;
    bool mDelete;
    bool mRevisePosition;
    bool mWaitForSpawned;
    GameObject mClone;
    Quaternion mRotation;
    IntVector4 mIndex;
    List<IntVector4> mNodes;
    SimplexNoise mNoise;
    //CommonInterface mCommon;

    bool mDeath;
    Transform mTDInfo;

    AiObject mAiObject;
    SPGroup mAiGroup;

    AssetReq.ReqFinishDelegate mReqFinish;
    List<AssetReq> mReqList = new List<AssetReq>();

    public static T InstantiateSPPoint<T>(  Vector3 position,
                                            Quaternion rotation,
                                            IntVector4 idx,
                                            Transform parent = null,
                                            int spid = 0,
                                            int pathid = 0,
                                            bool isActive = true,
                                            bool revisePos = true,
                                            bool isBoss = false,
                                            bool erode = true,
                                            bool delete = true,
                                            SimplexNoise noise = null,
                                            AssetReq.ReqFinishDelegate onSpawned = null,
                                            CommonInterface common = null) where T : SPPoint
    {
        GameObject obj = new GameObject("[" + position.x + " , " + position.z + "]");
        obj.transform.parent = parent;
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        T point = obj.AddComponent<T>();
        point.Init(idx, parent, spid, pathid, isActive, revisePos, isBoss, erode, delete, noise, onSpawned, common);
        return point;
    }

    public virtual void Init(IntVector4 idx,
                             Transform parent = null,
                             int spid = 0,
                             int pathid = 0, 
                             bool isActive = true,
                             bool revisePos = true,
                             bool isBoss = false,
                             bool isErode = true,
                             bool isDelete = true,
                             SimplexNoise noise = null,
                             AssetReq.ReqFinishDelegate onSpawned = null,
                             CommonInterface common = null)
    {
        mIndex = idx;
        mType = PointType.PT_NULL;
        mSpID = spid;
        mPathID = pathid;
        mActive = isActive;
        mIsBoss = isBoss;
        mErode = isErode;
        mDelete = isDelete;
        mNoise = noise;
        mNodes = new List<IntVector4>();
        mRevisePosition = revisePos;
        mWaitForSpawned = false;
        mReqFinish = onSpawned;
        //mCommon = common;

        AttachEventFromMesh();

        AttachCollider();

        RegisterPoint(this);
    }

    public AiObject aiObject { get { return mAiObject; } }
    public SPGroup aiGroup { get { return mAiGroup; } }

    public bool active
    {
        get { return mActive; }
    }

    public virtual bool isActive
    {
        get
        {
            if (!GameConfig.IsMultiMode)
            {
                bool cloneExist = false;

                if (clone != null)
                {
                    AiObject aiObject = clone.GetComponent<AiObject>();
                    if (aiObject == null || !aiObject.dead)
                        cloneExist = true;
                }

                return mActive && !cloneExist && !mWaitForSpawned;
            }
            else
            {
//                return AiNetworkManager.Instance.Exist(index);
				return false;
            }
        }
    }

    public bool spawning
    {
        get { return mWaitForSpawned; }
    }

    public bool death
    {
        get { return mDeath; }
    }

    public int pathID
    {
        get
        {
            if (mPathID <= 0)
            {
                CalculatePointType();

                if (mSpID <= 0)
                    return GetPathID();
                else
                    return GetPathIDFromSPID(mSpID);
            }

            return mPathID;
        }
    }

    public IntVector4 index
    {
        get { return mIndex; }
        set { mIndex = value; }
    }

    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Transform tdInfo 
    {
        set { mTDInfo = value; }
    }

    public bool revisePosition
    {
        get { return mRevisePosition; }
    }

    public GameObject clone
    {
        get { return mClone; }
    }

    public PointType type
    {
        get { return mType; }
    }

    public int typeID
    {
        get { return (int)type; }
    }

    public static void RegisterPoint(SPPoint point)
    {
        IntVector4 key = point.index;
        if (!PointTable.ContainsKey(key))
        {
            PointTable.Add(key, point);
        }
    }

    public static void RemovePoint(SPPoint point)
    {
        IntVector4 key = point.index;
        if (PointTable.ContainsKey(key))
        {
            PointTable.Remove(key);
        }
    }

    public void SetActive(bool isActive)
    {
        mActive = isActive;
    }

    public virtual void Activate(bool value)
    {
        if (value)
            AttachAllEvent();
        else
        {
            DetachAllEvent();
            RemovePoint(this);
        }
    }

    public void AttachEvent(IntVector4 node)
    {
        if (!mNodes.Contains(node))
        {
            mNodes.Add(node);
            LODOctreeMan.self.AttachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3(), 0);
        }
    }

    void AttachAllEvent()
    {
        foreach (IntVector4 node in mNodes)
        {
            LODOctreeMan.self.AttachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3(), 0);
        }
    }

    public void DetachEvent(IntVector4 node)
    {
        if (mNodes.Contains(node))
        {
            mNodes.Remove(node);
            LODOctreeMan.self.DetachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3(), 0);
        }
    }

    void DetachAllEvent()
    {
        foreach (IntVector4 node in mNodes)
        {
            LODOctreeMan.self.DetachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3(), 0);
        }

        mNodes.Clear();
    }

    bool Match(IntVector4 node)
    {
        float dx = position.x - node.x;
        float dz = position.z - node.z;

        return dx >= PETools.PEMath.Epsilon && dx <= VoxelTerrainConstants._numVoxelsPerAxis << node.w
            && dz >= PETools.PEMath.Epsilon && dz <= VoxelTerrainConstants._numVoxelsPerAxis << node.w;
    }

    public void AttachCollider()
    {
//        IntVector4 vec4 = AiUtil.ConvertToIntVector4(transform.position, 0);
        //IntVector2 vec2 = new IntVector2(vec4.x, vec4.z);

        //if (AiManager.Manager != null && AiManager.Manager.colliders.ContainsKey(vec2))
        //{
        //    foreach (IntVector4 node in AiManager.Manager.colliders[vec2])
        //    {
        //        OnTerrainColliderCreated(node);
        //    }
        //}
    }

    public void AttachEventFromMesh()
    {
        if (SPTerrainEvent.instance != null)
        {
            for (int i = mNodes.Count - 1; i >= 0; i--)
            {
                if (!Match(mNodes[i]))
                {
                    DetachEvent(mNodes[i]);
                }
            }

            List<IntVector4> meshNodeList = SPTerrainEvent.instance.meshNodes.FindAll(ret => Match(ret));
            foreach (IntVector4 mesh in meshNodeList)
            {
                AttachEvent(mesh);
            }
        }
    }

    public void InstantiateImmediately()
    {
        if (RevisePosition())
        {
			Instantiate();
        }
    }

    public virtual void ClearDeathEvent()
    {
        if (clone != null)
        {
            AiObject aiObj = clone.GetComponent<AiObject>();
            if (aiObj != null)
            {
                aiObj.DeathHandlerEvent -= OnDeath;
            }
        }
    }

    public void OnDestroy()
    {
        ClearDeathEvent();

        DetachAllEvent();

        foreach (AssetReq req in mReqList)
        {
            if (req != null)
            {
                req.ReqFinishHandler -= OnSpawned;
            }
        }
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    protected virtual void OnSpawnedChild(GameObject obj)
    {
    }

    protected virtual void OnSpawned(GameObject obj)
    {
        if (obj == null)
            return;

        if (mReqFinish != null)
        {
            mReqFinish(obj);
        }

        mClone = obj;
        mWaitForSpawned = false;
        //obj.transform.parent = AiManager.Manager.transform;

        mAiObject = clone.GetComponent<AiObject>();
        if (mAiObject != null)
        {
            if (mAiObject.motor != null && mAiObject.motor.gravity > PETools.PEMath.Epsilon)
            {
                if (!AiUtil.CheckPositionOnTerrainCollider(mAiObject.position))
                    mAiObject.Activate(false);
            }

            mAiObject.DeathHandlerEvent += OnDeath;
        }

        mAiGroup = clone.GetComponent<SPGroup>();
        if (mAiGroup != null)
        {
            mAiGroup.OnSpawndEvent += OnSpawnedChild;
        }
    }

    public virtual void OnDeath(AiObject aiObj)
    {
        mDeath = true;

        if(mDelete)
            Delete();
    }

    void OnTerrainColliderDestroy(IntVector4 node)
    {
        if (mNodes.Contains(node))
        {
            LODOctreeMan.self.DetachNodeEvents(null, null, null, OnTerrainColliderDestroy, OnTerrainColliderCreated, node.ToVector3(), 0);

            mNodes.Remove(node);
        }
    }

    void OnTerrainColliderCreated(IntVector4 node)
    {
        if (RevisePosition(node))
        {
            Instantiate();
        }
    }

    void Instantiate()
    {
        if (!GameConfig.IsMultiMode)
            InstantiateSingleMode();
        else
			InstantiateMultiMode();
    }

    bool RevisePosition()
    {
        if (!mRevisePosition)
            return true;

        Vector3 position = transform.position;

        RaycastHit hitInfo;
        if (AiUtil.CheckPositionOnGround(position, 
                                         out hitInfo, 
                                         256.0f,
                                         256.0f,
                                         AiUtil.groundedLayer))
        {
            transform.position = hitInfo.point;
            if (!mErode || AIErodeMap.IsInErodeArea(transform.position) == null)
            {
                return true;
            }
        }

        return false;
    }

    protected virtual bool RevisePosition(IntVector4 node)
    {
        if (!mRevisePosition)
            return true;

        Vector3 position = new Vector3(transform.position.x, node.y, transform.position.z);

        RaycastHit hitInfo;
        if (AiUtil.CheckPositionOnGround(position, out hitInfo, 0.0f,
            VoxelTerrainConstants._numVoxelsPerAxis << node.w,
            AiUtil.groundedLayer))
        {
            transform.position = hitInfo.point;
            if (!mErode || AIErodeMap.IsInErodeArea(transform.position) == null)
            {
                return true;
            }
        }

        return false;
    }

    void CalculatePointType()
    {
        mType = AiUtil.GetPointType(position);
    }

    void InstantiateSingleMode()
    {
        if (isActive)
        {
            mWaitForSpawned = true;

            int pathid = pathID;

            AssetReq req;

            if (mRevisePosition)
                req = AIResource.Instantiate(pathid, AIResource.FixedHeightOfAIResource(pathid, position), transform.rotation, OnSpawned);
            else
                req = AIResource.Instantiate(pathid, transform.position, transform.rotation, OnSpawned);

            if (req != null)
                mReqList.Add(req);
        }
    }

    void InstantiateMultiMode( int fixId = -1)
    {
        if (isActive)
        {
            int pathid = pathID;
            Vector3 pos = AIResource.FixedHeightOfAIResource(pathid, position);
			SPTerrainEvent.instance.RegisterAIToServer(index, pos, pathid);
        }
    }

    int GetPathIDFromSPID(int spid)
    {
        int tid = typeID;

        if (mTDInfo != null)
        {
            if (mType == PointType.PT_Water && !AiUtil.CheckPositionUnderWater(mTDInfo.position))
                tid = 4;

            if (Mathf.Abs(position.y - mTDInfo.position.y) > 64.0f)
                tid = 4;
        }

        return AISpawnPath.GetPathID(spid, tid);
    }

    int GetPathID()
    {
        if (GameConfig.IsMultiMode)
        {
            return GetPathIDNetwork();
        }
        else
        {
            if (Application.loadedLevelName.Equals(GameConfig.MainSceneName))
            {
                return GetPathIDStory();
            }
            else if (Application.loadedLevelName.Equals(GameConfig.AdventureSceneName))
            {
                return GetPathIDAdventure();
            }
        }

        return 0;
    }

    //0  - 5   kilometer : 1; 
    //5  - 10  kilometer : 2; 
    //10 - 15  kilometer : 3; 
    //15 - 20  kilometer : 4; 
    int GetAreaID()
    {
        return AiUtil.GetAreaID(transform.position);
    }

    int GetMapID()
    {
        return AiUtil.GetMapID(transform.position);
    }

    float GetNoiseValue()
    {
        return (float)mNoise.Noise(transform.position.x, transform.position.z * 100.0f) * 0.5f + 0.5f;
    }

    int GetPathIDStory()
    {
        return AISpawnDataStory.GetRandomPathIDFromType(typeID, position);
    }

    int GetPathIDNetwork()
    {
        if (mNoise == null)
            return AISpawnDataAdvMulti.GetPathID(GetMapID(), GetAreaID(), typeID);
        else
        {
            if(!mIsBoss)
                return AISpawnDataAdvMulti.GetPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
            else
                return AISpawnDataAdvMulti.GetBossPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
        }
    }

    int GetPathIDAdventure()
    {
        if (mNoise == null)
            return AISpawnDataAdvSingle.GetPathID(GetMapID(), GetAreaID(), typeID);
        else
        {
            if(!mIsBoss)
                return AISpawnDataAdvSingle.GetPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
            else
                return AISpawnDataAdvSingle.GetBossPathID(GetMapID(), GetAreaID(), typeID, GetNoiseValue());
        }
    }

    void OnDrawGizmosSelected()
    {
        if (mIsBoss)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Gizmos.DrawSphere(transform.position, 2.0f);
    }

    public static void WriteSPPoint(uLink.BitStream stream, object obj, params object[] codecOptions)
    {
        SPPoint point = obj as SPPoint;

        stream.Write<Vector3>(point.transform.position);
        stream.Write<IntVector4>(point.mIndex);
        stream.Write<PointType>(point.mType);
        stream.Write<IntVector4[]>(point.mNodes.ToArray());
    }

    public static object ReadSPPoint(uLink.BitStream stream, params object[] codecOptions)
    {
        SPPoint point = new SPPoint();
        point.transform.position = stream.Read<Vector3>();
        point.mIndex = stream.Read<IntVector4>();
        point.mType = stream.Read<PointType>();
        point.mNodes = new List<IntVector4>();
        IntVector4[] points = stream.Read<IntVector4[]>();
        point.mNodes.AddRange(points);

        return point;
    }
}
