using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SPPointInitiative : SPPoint
{
    public int count;
    public float interval;
    public int[] pathIDs;

    List<GameObject> objs = new List<GameObject>();
    List<AssetReq> reqList = new List<AssetReq>();

    public List<GameObject> objects
    {
        get { return objs; }
    }

    public override void ClearDeathEvent()
    {
        base.ClearDeathEvent();

        foreach (GameObject obj in objs)
        {
            if (obj != null)
            {
                AiObject aiObj = obj.GetComponent<AiObject>();
                if (aiObj != null)
                {
                    aiObj.DeathHandlerEvent -= OnDeath;
                }
            }
        }
    }

    void Start()
    {
        Init(IntVector4.Zero);
    }

    new public void OnDestroy()
    {
		base.OnDestroy ();

        foreach (AssetReq req in reqList)
        {
            if(req != null)
            {
                req.ReqFinishHandler -= OnSpawned;
            }
        }
    }

    public void ActivateInitiative(bool value)
    {
        if (value)
            StartCoroutine(SpawnAI());
        else
            StopAllCoroutines();
    }

    bool IsRevisePosition()
    {
        float distance = VoxelTerrainConstants._numVoxelsPerAxis;
        RaycastHit hitInfo;
        if (AiUtil.CheckPositionOnGround(position, out hitInfo, distance, distance, AiUtil.groundedLayer))
        {
            position = hitInfo.point;
            return true;
        }

        return false;
    }

    void OnAiDestroy(AiObject aiObj)
    {
        if (aiObj != null && objs.Contains(aiObj.gameObject))
            objs.Remove(aiObj.gameObject);
    }

    protected override void OnSpawned(GameObject obj)
    {
        base.OnSpawned(obj);

        if (!objs.Contains(obj))
        {
            objs.Add(obj);
        }

        AiObject ai = obj.GetComponent<AiObject>();
        if (ai != null)
        {
            ai.DestroyHandlerEvent += OnAiDestroy;
        }
    }

    IEnumerator SpawnAI()
    {
        while (count > 0)
        {
            yield return new WaitForSeconds(interval);

            if (active && IsRevisePosition())
            {
                int id = pathIDs[Random.Range(0, pathIDs.Length)];
                AssetReq req = AIResource.Instantiate(id, position, Quaternion.identity, OnSpawned);
                reqList.Add(req);

                count--;
            }
        }
    }
}
