using UnityEngine;
using System.Collections.Generic;

namespace Transvoxel.SurfaceExtractor
{
    public class TransVertices
    {
		public List<byte> Near = new List<byte>();
		public List<bool> IsLowside = new List<bool>();
        public List<Vector3> Position = new List<Vector3>();
        public List<Vector4> Normal_t = new List<Vector4>();
		public int Count{	get{	return Near.Count;	}	}
    }
}