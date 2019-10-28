using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ITowerDefenceData
{
    bool Begin { get; }
    float DelayTime { get; }
    int KilledCount { get; }
    int TotalCount { get; }
}

class SPTowerDefenceManager
{
    static Dictionary<int, SPTowerDefence> SPTowerDefenceDic = new Dictionary<int, SPTowerDefence>();

    public static void AddSPTowerDefence(int nMissionID, SPTowerDefence obj)
    {
        if (null != obj )
            if (SPTowerDefenceDic.ContainsKey(nMissionID))
            {
                DestroySPTowerDefence(nMissionID);
                SPTowerDefenceDic[nMissionID] = obj;
            }
    }
    public static void DestroySPTowerDefence(int nMissionID)
    {
        if (SPTowerDefenceDic.ContainsKey(nMissionID))
            SPTowerDefenceDic[nMissionID].NetWorkDestroyObject();
    }
}

public class SPTowerDefence : SPAutomatic, ITowerDefenceData
{
    int mMissionID;

    float mMinRadius;
    float mMaxRadius;

    public static SPTowerDefence InstantiateTowerDefence(int mission, Vector3 position, float minRadius, float maxRadius, 
        int id, float delayTime = 0.0f, Transform parent = null, bool isPlay = true)
    {
        GameObject obj = new GameObject("SPTowerDefence");
        SPTowerDefence spTowerDefence = obj.AddComponent<SPTowerDefence>() as SPTowerDefence;
        obj.transform.position = position;
        obj.transform.parent = parent;

        spTowerDefence.ID = id;
        spTowerDefence.Delay = delayTime;

        spTowerDefence.mMissionID = mission;
        spTowerDefence.mMinRadius = minRadius;
        spTowerDefence.mMaxRadius = maxRadius;

        if (isPlay)
        {
            spTowerDefence.SpawnAutomatic();
        }

        return spTowerDefence;
    }

    public int MissionID { get { return mMissionID; } }

    protected override void OnSpawnComplete()
    {
        base.OnSpawnComplete();

        //Delete();
    }

    protected override SPPoint Spawn(AISpawnData spData)
    {
        base.Spawn(spData);

        Vector3 pos;
        Quaternion rot;
        if (GetPositionAndRotation(out pos, out rot, spData.minAngle, spData.maxAngle))
        {
            SPPointMovable movable = SPPoint.InstantiateSPPoint<SPPointMovable>(pos,
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
                                                                                this) as SPPointMovable;

            movable.target = transform;

            return movable;
        }

        return null;
    }

    public override void OnSpawned(GameObject obj)
    {
        base.OnSpawned(obj);

        AiObject aiObj = obj.GetComponent<AiObject>();
        if (aiObj != null)
        {
            aiObj.tdInfo = transform;
        }

        SPGroup spGroup = obj.GetComponent<SPGroup>();
        if (spGroup != null)
        {
            spGroup.tdInfo = transform;
        }
    }

    protected override void OnDeath(AiObject aiObj)
    {
        base.OnDeath(aiObj);


        if (mMissionID > 0)
        {
//            GameGui_N.Instance.mMissionTrackGui.SetMonsterLeft(mMissionID, KilledCount);
        }

        //if (KilledCount >= m_data.m_Count)
        //{
        //    PlayerFactory.mMainPlayer.ProcessTowerMission(m_missionID, true);

        //    DestroyObject(this.gameObject, 0.1f);
        //}
    }

    bool GetPositionAndRotation(out Vector3 pos, out Quaternion rot, float minAngle, float maxAngle)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;

        pos = AiUtil.GetRandomPosition(transform.position, mMinRadius, mMaxRadius,
                                        Vector3.forward, minAngle, maxAngle, 10.0f,
                                        AiUtil.groundedLayer, 5);

        if (pos != Vector3.zero)
            return true;

        if (!GameConfig.IsMultiMode)
        {
            pos = AiUtil.GetRandomPosition(transform.position, mMinRadius, mMaxRadius,
                                            Vector3.forward, minAngle, maxAngle);

            if (pos != Vector3.zero)
                return true;
        }
        
        return false;
    }

    public void NetWorkDestroyObject()
    {
        Destroy(this.gameObject, 0.1f);
    }
}
