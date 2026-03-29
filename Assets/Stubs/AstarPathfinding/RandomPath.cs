// Stub: Pathfinding.RandomPath
// 6 RandomPath.Construct references. Generates a random path from a start position.

using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// Random wandering path. Game code calls RandomPath.Construct(start, length, callback).
    /// Used by NpcMgr for NPC idle wandering behavior.
    /// </summary>
    public class RandomPath : ABPath
    {
        public Vector3 aim;
        public int spread;
        public float aimStrength { get; set; }

        public static RandomPath Construct(Vector3 start, int length, OnPathDelegate callback)
        {
            return new RandomPath { startPoint = start };
        }
    }

    public class FleePath : ABPath
    {
        public float flee { get; set; }
        public int spread;
        // aimStrength and aim: used by BTTarget.cs to configure flee path aiming.
        public float aimStrength { get; set; }
        public Vector3 aim;

        public static FleePath Construct(Vector3 start, Vector3 avoid, int length, OnPathDelegate callback = null)
        {
            return new FleePath { startPoint = start };
        }
    }
}
