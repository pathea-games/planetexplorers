using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BGEStory : BGEffect
{
    internal override int GetMapID(Vector3 position)
    {
        Vector2 pos = new Vector2(position.x, position.z);
        return PeMappingMgr.Instance.GetAiSpawnMapId(pos);
    }
}
