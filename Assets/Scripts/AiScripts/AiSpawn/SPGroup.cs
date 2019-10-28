using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SPGroup : CommonInterface 
{
    public GameObject aiPrefab;

    public virtual IEnumerator SpawnGroup() { yield return null; }

    public event AssetReq.ReqFinishDelegate OnSpawndEvent;

    AiObject mHeader;
    //AiEnemy mEnemy;
	AiBehaveGroup mBehaveGroup;

    List<AiObject> mAiObjects = new List<AiObject>();
    List<AiObject> mActiveAiObjects = new List<AiObject>();
    List<AssetReq> mReqs = new List<AssetReq>();

    bool mSpawned;
    int mMaxHp;
    int mCount;
    IntVector4 mIndex;

    int mSpawnedCount;

    Transform mTDInfo;


    public int curCount { get { return ++mCount; } }
    public int spawnedCount { get { return mSpawnedCount; } set { mSpawnedCount = value; } }
    public IntVector4 index { get { return mIndex; } set { mIndex = value; } }

    public List<AiObject> aiObjects { get { return mAiObjects; } }
    public List<AiObject> activeAiObjects { get { return mActiveAiObjects; } }

    public Transform tdInfo { get { return mTDInfo; } set { mTDInfo = value; } }

    public bool spawned
    {
        get { return mSpawned; }
        set 
        {
            if (mSpawned != value)
            {
                if(value)
                    StartCoroutine(DestroyGroup(2.0f));

                mSpawned = value;
            }
        }
    }

    #region unity interface

    void Start ()
	{
        SpawnAiPrefab();

        if (!GameConfig.IsMultiMode)
            Spawn();
        else
            InitMember();

        //AiManager.Manager.RegisterGroup(this);
	}

    void OnDestroy()
    {
        //AiManager.Manager.RemoveGroup(this);
    }

    #endregion

    public void SetOwenerView()
    {
        if (IsController)
        {
            if (mBehaveGroup != null)
                mBehaveGroup.enabled = true;
        }
        else
        {
            if (mBehaveGroup != null)
                mBehaveGroup.enabled = false;
        }
    }

    public void SetRunSpeed()
    {
        foreach (AiObject aiObj in mAiObjects)
        {
            if (aiObj != null)
            {
                aiObj.speed = aiObj.runSpeed;
            }
        }
    }

    public void SetWalkSpeed()
    {
        foreach (AiObject aiObj in mAiObjects)
        {
            if (aiObj != null)
            {
                aiObj.speed = aiObj.walkSpeed;
            }
        }
    }

    public void Spawn()
    {
        StartCoroutine(SpawnAIGroup());
    }

	public void Instantiate(int id, Vector3 pos, Quaternion rot)
	{
		Vector3 fixPosition = AIResource.FixedHeightOfAIResource(id, pos);
		if (GameConfig.IsMultiMode)
		{
			//AIGroupNetWork group = Netlayer as AIGroupNetWork;
			//if (null == group)
			//	return;

			//uLink.NetworkViewID tdViewID = group.AiTD == null ? uLink.NetworkViewID.unassigned : group.AiTD.OwnerView.viewID;
   //         IntVector4 _inx = new IntVector4(mIndex.x, mIndex.y, mIndex.z, curCount);
			//RPCServer(EPacketType.PT_AG_SpawnAIGroupMemberAtPoint, _inx, fixPosition, id, 2, tdViewID);
        }
		else
		{
			mSpawnedCount++;
			AssetReq req = AIResource.Instantiate(id, fixPosition, rot, OnSpawned);
            mReqs.Add(req);
		}
	}

    void OnSpawned(GameObject go)
    {
        if (go == null) return;

        go.transform.parent = transform;

        AiObject aiObj = go.GetComponent<AiObject>();
		if (aiObj != null)
        {
            if (mTDInfo != null)
            {
                aiObj.tdInfo = mTDInfo;
            }

            RegisterAiObjects(aiObj);
        }

        if (OnSpawndEvent != null)
        {
            OnSpawndEvent(go);
        }
    }

    void SpawnAiPrefab()
    {
        if (aiPrefab != null)
        {
            GameObject aiObject = Instantiate(aiPrefab, transform.position, transform.rotation) as GameObject;

            if (aiObject != null)
            {
                aiObject.transform.parent = transform;

				mBehaveGroup = aiObject.GetComponent<AiBehaveGroup>();
				if (mBehaveGroup != null)
                {
					mBehaveGroup.RegisterSPGroup(this);
                }
            }
        }
    }

    void AddGroupMaxHP(AiObject aiObj)
    {
        mMaxHp += aiObj.maxLife;
    }

    void Delete()
    {
        foreach (AssetReq ite in mReqs)
        {
            if (ite != null)
            {
				ite.Deactivate();
                ite.ReqFinishHandler -= OnSpawned;
            }
        }

        Destroy(gameObject, 0.5f);
    }

    void InitMember()
    {
        if(GameConfig.IsMultiMode)
        {
            //AIGroupNetWork groupNetwork = Netlayer as AIGroupNetWork;
            //if (groupNetwork != null)
            //{
            //    foreach (AiNetwork aiNet in groupNetwork.GetAiNetworkList())
            //    {
            //        if (aiNet != null && aiNet.Runner != null)
            //        {
            //            AiObject aiObj = aiNet.Runner as AiObject;
            //            if (aiObj != null)
            //            {
            //                RegisterAiObjects(aiObj);
            //            }
            //        }
            //    }
            //}
        }
    }

    IEnumerator SpawnAIGroup()
    {
        OnSpawnGroupStart();

        yield return StartCoroutine(SpawnGroup());

        OnSpawnGroupEnd();
    }

    IEnumerator DestroyGroup(float interval)
    {
        yield return new WaitForSeconds(10.0f);

        while (true)
        {
            //if(!GameConfig.IsMultiMode)
            {
                if (GetComponentsInChildren<AiObject>().Length <= 0)
                {
                    Delete();
                    yield break;
                }
            }

            yield return new WaitForSeconds(interval);
        }
    }

    #region ai list manager

    public void RegisterAiObjects(AiObject aiObj)
    {
        if (!mAiObjects.Contains(aiObj))
        {
            aiObj.transform.parent = transform;

            aiObj.DeathHandlerEvent += OnAiObjectDeath;
            aiObj.DestroyHandlerEvent += OnAiObjectDestroy;
            aiObj.ActiveHandlerEvent += OnAiObjectActive;
            aiObj.InactiveHandlerEvent += OnAiObjectDeActive;

            mAiObjects.Add(aiObj);

            RegisterActiveAiObjects(aiObj);

            AddGroupMaxHP(aiObj);
        }
    }

    public void RemoveAiObjects(AiObject aiObj)
    {
        if (mAiObjects.Contains(aiObj))
        {
            mAiObjects.Remove(aiObj);

            RemoveActiveAiObjects(aiObj);
        }
    }

    void RegisterActiveAiObjects(AiObject aiObj)
    {
        if (!mActiveAiObjects.Contains(aiObj))
            mActiveAiObjects.Add(aiObj);
    }

    void RemoveActiveAiObjects(AiObject aiObj)
    {
        if (mActiveAiObjects.Contains(aiObj))
            mActiveAiObjects.Remove(aiObj);
    }

    #endregion

    #region Callback

    protected virtual void OnSpawnGroupStart()
    {

    }

    protected virtual void OnSpawnGroupEnd()
    {
        Spawned();

        spawned = true;
    }

    protected virtual void OnAiObjectDeath(AiObject aiObj)
    {
        RemoveActiveAiObjects(aiObj);
    }

    protected virtual void OnAiObjectDestroy(AiObject aiObj)
    {
        RemoveAiObjects(aiObj);
        RemoveActiveAiObjects(aiObj);
    }

    protected virtual void OnAiObjectActive(AiObject aiObj)
    {
        RegisterActiveAiObjects(aiObj);
    }

    protected virtual void OnAiObjectDeActive(AiObject aiObj)
    {
        RemoveActiveAiObjects(aiObj);
    }

    #endregion

    #region Ai Interface

    public bool hasEnemy
    {
        get { return false; }
    }

    //public AiEnemy enemy
    //{
    //    get
    //    {
    //        if (mEnemy != null && !mEnemy.valid)
    //            mEnemy = null;

    //        if(mEnemy == null)
    //        {
    //            foreach (AiObject aiObject in mActiveAiObjects)
    //            {
    //                if (aiObject == null || aiObject.aiTarget == null)
    //                    continue;

    //                if (aiObject.enemy != null && aiObject.enemy.valid)
    //                {
    //                    mEnemy = aiObject.enemy;
    //                    break;
    //                }
    //            }
    //        }

    //        return mEnemy;
    //    }
    //}

    public AiObject header
    {
        get
        {
            if(mHeader == null || mHeader.dead)
            {
                foreach (AiObject aiObject in mActiveAiObjects)
                {
                    if (aiObject != null && !aiObject.dead)
                    {
                        mHeader = aiObject;
                        break;
                    }
                }
            }

            return mHeader;
        }
    }

    public float damage
    {
        get
        {
            float _damage = 0.0f;
            foreach (AiObject aiObject in mActiveAiObjects)
            {
                if (aiObject != null && !aiObject.dead)
                {
                    _damage += aiObject.GetAttribute(Pathea.AttribType.Atk);
                }
            }

            return _damage;
        }
    }

    public int Hp
    {
        get
        {
            int _hp = 0;
            foreach (AiObject aiObject in mActiveAiObjects)
            {
                _hp += aiObject.life;
            }

            return _hp;
        }
    }

    public int maxHp
    {
        get
        {
            return mMaxHp;
        }
    }

    public float hpPercent
    {
        get
        {
            return (float)Hp / (float)maxHp;
        }
    }

    #endregion

    #region RPC

    void Spawned()
    {
    }

    public void ClearHatredAll()
    {
        //mEnemy = null;

        //foreach (AiObject aiObject in aiObjects)
        //{
        //    if (aiObject != null && aiObject.aiTarget != null)
        //    {
        //        aiObject.aiTarget.ClearHatred();
        //    }
        //}

        if(GameConfig.IsMultiMode && IsController)
			RPCServer(EPacketType.PT_AG_ClearHatredAll);
    }

    public void ClearMoveAndRotation()
    {
        foreach (AiObject aiObject in mAiObjects)
        {
            if (aiObject != null)
            {
                aiObject.StopMoveAndRotation();
            }
        }

        if (GameConfig.IsMultiMode && IsController)
			RPCServer(EPacketType.PT_AG_ClearMoveAndRotation);
    }

    public void ActivateSingleBehave(bool value)
    {
        foreach (AiObject aiObject in mAiObjects)
        {
            if (aiObject != null && aiObject.behave != null && aiObject.behave.isMember)
            {
                aiObject.behave.isActive = value;
            }
        }

        if (GameConfig.IsMultiMode && IsController)
			RPCServer(EPacketType.PT_AG_ActivateSingleBehave, value);
    }

    public void MoveToPositionArmy(Vector3 position)
    {
        foreach (AiObject aiObject in activeAiObjects)
        {
            //if (aiObject == null || aiObject.enemy == null || !aiObject.enemy.valid)
            //    continue;

            Vector3 patrolPosition = position + header.transform.TransformDirection(aiObject.offset - header.offset);
            aiObject.desiredMoveDestination = patrolPosition;
        }

        if (GameConfig.IsMultiMode && IsController)
			RPCServer(EPacketType.PT_AG_MoveToPositionArmy, position);
    }

    public void MoveToPosition(Vector3 position)
    {
        foreach (AiObject aiObject in activeAiObjects)
        {
            if (aiObject == null 
                || aiObject.behave == null 
                || aiObject.behave.running)
                continue;

            if (GameConfig.IsMultiMode && !aiObject.IsController)
                continue;

            Vector3 patrolPosition = position + header.transform.TransformDirection(aiObject.offset - header.offset);
            if (AiUtil.SqrMagnitudeH(patrolPosition - aiObject.position) > 1f * 1f)
                aiObject.desiredMoveDestination = patrolPosition;
            else
                aiObject.desiredMoveDestination = Vector3.zero;

            aiObject.speed = aiObject.walkSpeed;
        }

        if (GameConfig.IsMultiMode && IsController)
			RPCServer(EPacketType.PT_AG_MoveToPosition, position);
    }

    public void MoveToPositionFree()
    {
        //IAGPatrolFree patrol = mBehaveGroup.GetComponent<IAGPatrolFree>();
        //if (patrol == null)
        //    return;

        //foreach (AiObject aiObject in activeAiObjects)
        //{
        //    if (aiObject == null
        //        || aiObject.behave == null
        //        || aiObject.behave.running)
        //        continue;

        //    if (GameConfig.IsMultiMode && !aiObject.IsController)
        //        continue;

        //    Vector3 patrolPosition = patrol.GetPatrolPosition(aiObject.position);

        //    if (patrolPosition == Vector3.zero)
        //        continue;

        //    aiObject.desiredMoveDestination = patrolPosition;
        //    aiObject.speed = Random.Range(aiObject.walkSpeed, aiObject.runSpeed);
        //}

        //if (GameConfig.IsMultiMode && IsController)
        //    RPCServer(EPacketType.PT_AG_MoveToPositionFree);
    }

    public void RunawayForPosition(Vector3 position)
    {
        if (!GameConfig.IsMultiMode)
        {
            foreach (AiObject aiObject in activeAiObjects)
            {
                if (aiObject != null && !aiObject.dead)
                {
                    if (GameConfig.IsMultiMode && !aiObject.IsController)
                        continue;

                    Vector3 direction = (aiObject.position - position).normalized;
                    direction = AiUtil.RunawayDirectionCorrect(aiObject, direction);

                    aiObject.speed = aiObject.runSpeed;
                    aiObject.desiredMoveDestination = aiObject.position + direction.normalized * 2;
                }
            }
        }

        if (GameConfig.IsMultiMode && IsController)
			RPCServer(EPacketType.PT_AG_RunawayForPosition, position);
    }
    #endregion
}
