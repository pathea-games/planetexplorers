// Stub: Pathfinding.Path, ABPath, NNConstraint, PathNNConstraint, GraphUpdateObject, Int3
// Path.vectorPath is the primary game code access pattern (List<Vector3> of waypoints).

using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// Integer 3D vector used by A* for node coordinates.
    /// </summary>
    public struct Int3
    {
        public int x, y, z;
        public Int3(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
        public static readonly Int3 zero = new Int3(0, 0, 0);
        public static implicit operator UnityEngine.Vector3(Int3 v) => new UnityEngine.Vector3(v.x, v.y, v.z);
        public static explicit operator Int3(UnityEngine.Vector3 v) => new Int3((int)v.x, (int)v.y, (int)v.z);
    }

    /// <summary>
    /// Base path class. Game code checks .error, reads .vectorPath for waypoints.
    /// </summary>
    public class Path
    {
        public List<Vector3> vectorPath = new List<Vector3>();
        public bool error;
        public string errorLog = "";

        public void Claim(object claimer) { }
        public void Release(object claimer) { }
        // Error(): abort in-progress path calculation.
        public void Error() { }

        public float GetTotalLength()
        {
            return 0f;
        }
    }

    /// <summary>
    /// A-to-B path. Used via ABPath.Construct(start, end, callback).
    /// </summary>
    public class ABPath : Path
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public Vector3 originalStartPoint;

        public static ABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback)
        {
            return new ABPath { startPoint = start, endPoint = end };
        }
    }

    /// <summary>
    /// Constraint for nearest-node queries.
    /// </summary>
    public class NNConstraint
    {
        public static readonly NNConstraint Default = new NNConstraint();
        public static readonly NNConstraint None = new NNConstraint();
    }

    /// <summary>
    /// Path-specific nearest-node constraint.
    /// </summary>
    public class PathNNConstraint : NNConstraint
    {
        public new static readonly PathNNConstraint Default = new PathNNConstraint();
    }

    /// <summary>
    /// Describes a graph update for dynamic obstacles. Constructed with a Bounds.
    /// </summary>
    public class GraphUpdateObject
    {
        public Bounds bounds;

        public GraphUpdateObject(Bounds bounds)
        {
            this.bounds = bounds;
        }
    }
}
