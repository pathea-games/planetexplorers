using UnityEngine;
using System.Collections;

public class SPPlayerSummon : MonoBehaviour
{
    [System.Serializable]
    public class CaelumRexData
    {
        public int pathID;
        public float interval;
        public float minRadius;
        public float maxRadius;
        public float probability;
    }

    [System.Serializable]
    public class PlayerSleepData
    {
        public int minCount;
        public int maxCount;
        public float interval;
        public float minRadius;
        public float maxRadius;
        public float probability;
    }

    public CaelumRexData caelumrex;

    public PlayerSleepData sleep;

    void Start()
    {
        StartCoroutine(Spawn(sleep));
        StartCoroutine(Spawn(caelumrex));
    }

    IEnumerator Spawn(CaelumRexData data)
    {
        while (true)
        {
            if (IsCaelumRexReady(data))
            {
                //Vector3 position = GetCaelumRexPosition(
                //    PlayerFactory.mMainPlayer.transform.position, 
                //    data.minRadius, 
                //    data.maxRadius);

                //Quaternion rot = Quaternion.LookRotation(PlayerFactory.mMainPlayer.transform.position - position, Vector3.up);

                //AIResource.Instantiate(data.pathID, position, rot, OnCaelumRexSpawned);
            }
            yield return new WaitForSeconds(data.interval);
        }
    }

    bool IsCaelumRexReady(CaelumRexData data)
    {
        if (data.pathID == 0)
            return false;

        //if (PlayerFactory.mMainPlayer == null)
        //    return false;

        //GameObject carrier = PlayerFactory.mMainPlayer.Carrier;
        //if (carrier == null)
        //    return false;

        //HelicopterController hel = carrier.GetComponent<HelicopterController>();
        //if (hel == null)
        //    return false;

        //VCPVtolCockpitFunc vtol = hel.m_Cockpit as VCPVtolCockpitFunc;
        //if (vtol == null)
        //    return false;

        //if (vtol.FlyingHeight < 50.0f)
        //    return false;

        //if (Random.value > data.probability)
        //    return false;

        return true;
    }

    Vector3 GetCaelumRexPosition(Vector3 center, float minRange, float maxRange)
    {
        Vector2 off = Random.insideUnitCircle.normalized * Random.Range(minRange, maxRange);

        return center + new Vector3(off.x, 0.0f, off.y) + Vector3.up * Random.Range(5, 10);
    }

    void OnCaelumRexSpawned(GameObject obj)
    {
        if (obj == null)
            return;

        //obj.transform.parent = AiManager.Manager.transform;

        //AiObject aiObj = obj.GetComponent<AiObject>();
        //if (aiObj != null && aiObj.aiTarget != null)
        //{
        //    //aiObj.aiTarget.AddExtraHatred(PlayerFactory.mMainPlayer.gameObject, 100);
        //}
    }

    IEnumerator Spawn(PlayerSleepData data)
    {
        while (true)
        {
            if (IsPlayerSleeping(data))
            {
                Vector3 position = Vector3.zero;
                    //GetSpawnPosition(
                    //PlayerFactory.mMainPlayer.transform.position,
                    //data.minRadius,
                    //data.maxRadius);

                Quaternion rot = Quaternion.identity;
                    //Quaternion.LookRotation(PlayerFactory.mMainPlayer.transform.position - position, Vector3.up);

                int pathID = 0;

                int typeID = (int)AiUtil.GetPointType(position);

                if (Application.loadedLevelName.Equals(GameConfig.MainSceneName))
                {
                    pathID = AISpawnDataStory.GetRandomPathIDFromType(typeID, position);
                }
                else if (Application.loadedLevelName.Equals(GameConfig.AdventureSceneName))
                {
                    int mapID = AiUtil.GetMapID(position);
                    int areaID = AiUtil.GetAreaID(position);
                    pathID = AISpawnDataAdvSingle.GetPathID(mapID, areaID, typeID);
                }

                AIResource.Instantiate(pathID, position, rot, OnSleepSpawned);
            }
            yield return new WaitForSeconds(data.interval);
        }
    }

    Vector3 GetSpawnPosition(Vector3 center, float minRange, float maxRange)
    {
        return AiUtil.GetRandomPosition(center, minRange, maxRange, 15.0f, AiUtil.groundedLayer, 10);
    }

    bool IsPlayerSleeping(PlayerSleepData data)
    {
        if (data.minCount == 0 && data.maxCount == 0)
            return false;

        //Player player = PlayerFactory.mMainPlayer;
        //if (player == null || !player.IsSleeping)
        //    return false;

        if (Random.value > data.probability)
            return false;

        return true;
    }

    void OnSleepSpawned(GameObject obj)
    {
        //if (obj == null)
        //    return;

        ////obj.transform.parent = AiManager.Manager.transform;

        //AiObject aiObj = obj.GetComponent<AiObject>();
        //if (aiObj != null && aiObj.aiTarget != null)
        //{
        //    //aiObj.aiTarget.AddExtraHatred(PlayerFactory.mMainPlayer.gameObject, 100);
        //}
    }
}
