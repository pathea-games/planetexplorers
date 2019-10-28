using UnityEngine;
using System.Collections;

public class BGEAdventure : BGEffect
{
    internal override int GetMapID(Vector3 position)
    {
        RandomMapType map = VFDataRTGen.GetXZMapType((int)position.x, (int)position.z);
        switch (map)
        {
            case RandomMapType.GrassLand:   return 2;
            case RandomMapType.Forest:      return 8;
            case RandomMapType.Desert:      return 13;
            case RandomMapType.Redstone:    return 18;
            case RandomMapType.Rainforest:  return 10;
            default:                        return 0;
        }
    }
}
