// Stub: Pathfinding.Seeker
// MonoBehaviour component required by PEPathfinder. Provides path request delegation.

using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    // OnPathDelegate is defined globally in GlobalStubs.cs as delegate void OnPathDelegate(Pathfinding.Path path).
    // Do NOT redefine it here — using the global type ensures game code (NpcMgr.cs inside namespace Pathea)
    // and Pathfinding stubs share the same delegate type.

    /// <summary>
    /// Seeker MonoBehaviour. Attached to pathfinding agents to request and manage paths.
    /// Game code uses pathCallback += OnPathComplete and StartPath(start, end).
    /// </summary>
    public class StartEndModifier
    {
        public enum Exactness { SnapToNode, Original, Interpolate, ClosestOnNode }
        public Exactness exactStartPoint { get; set; }
        public Exactness exactEndPoint { get; set; }
        public Exactness exactEndPointKind { get; set; }
        public Exactness exactStartPointKind { get; set; }
    }

    public class Seeker : MonoBehaviour
    {
        public OnPathDelegate pathCallback;
        public List<Vector3> lastCompletedVectorPath;
        public float curSeekerSize { get; set; }
        public StartEndModifier startEndModifier => new StartEndModifier();

        public Path StartPath(Vector3 start, Vector3 end)
        {
            return new ABPath();
        }

        public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback)
        {
            return new ABPath();
        }

        public Path GetCurrentPath()
        {
            return null;
        }

        public bool IsDone() => true;
    }

    /// <summary>
    /// Path modifier for smoothing waypoints. Required by PEPathfinder via [RequireComponent].
    /// </summary>
    public class SimpleSmoothModifier : MonoBehaviour
    {
    }
}
