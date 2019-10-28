using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SPGroupContinue : SPGroup
{
    public bool overlay;
    public int count;
    public float interval;
    public int[] pathIDs;

//    bool CanSpawn()
//    {
//        if (GameConfig.IsMultiMode)
//            return overlay || AiNetworkManager.Instance.GetGroupAiNetwork(OwnerView.viewID.id).Count <= 0;
//        else
//            return overlay || aiObjects.Count <= 0;
//    }

    bool CheckOnTerrain()
    {
        Vector3 pos = transform.position;
        if (AiUtil.CheckPositionOnGround(ref pos, 10.0f, AiUtil.groundedLayer))
        {
            transform.position = pos;
            return true;
        }

        return false;
    }

//    public override IEnumerator SpawnGroup()
//    {
//        while (count - spawnedCount > 0)
//        {
//            if (CheckOnTerrain() && CanSpawn())
//            {
//                int id = pathIDs[Random.Range(0, pathIDs.Length)];
//                Instantiate(id, transform.position, Quaternion.identity);
//            }
//           
//            yield return new WaitForSeconds(interval);
//        }
//    }
}
