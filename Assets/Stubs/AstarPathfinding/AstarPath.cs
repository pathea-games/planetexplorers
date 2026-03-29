// Stub: Pathfinding.AstarPath
// 66 AstarPath.active refs, 7 StartPath refs, 2 UpdateGraphs refs.
// Singleton MonoBehaviour providing graph access and pathfinding entry points.

using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// A* Pathfinding Project singleton. Game code accesses AstarPath.active for
    /// graph queries, StartPath for path requests, and UpdateGraphs for dynamic obstacles.
    /// </summary>
    public class AstarPath : MonoBehaviour
    {
        private static AstarPath _active;

        public static AstarPath active
        {
            get { return _active; }
            set { _active = value; }
        }

        public NavGraph[] graphs = new NavGraph[0];

        private AstarData _astarData = new AstarData();
        public AstarData astarData
        {
            get { return _astarData; }
        }

        public static void StartPath(Path path) { }

        public NNInfo GetNearest(Vector3 position)
        {
            return new NNInfo();
        }

        public NNInfo GetNearest(Vector3 position, NNConstraint constraint)
        {
            return new NNInfo();
        }

        public NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
        {
            return new NNInfo();
        }

        public bool SkipOptScanOnStartUp { get; set; }

        public void Scan() { }

        public void FloodFill() { }

        public void UpdateGraphs(GraphUpdateObject guo) { }

        public void UpdateGraphs(Bounds bounds) { }

        void Awake()
        {
            _active = this;
        }
    }

    /// <summary>
    /// Holds graph data for AstarPath. Game code accesses astarData.layerGridGraph.
    /// </summary>
    public class AstarData
    {
        public LayerGridGraph layerGridGraph = new LayerGridGraph();
    }
}
