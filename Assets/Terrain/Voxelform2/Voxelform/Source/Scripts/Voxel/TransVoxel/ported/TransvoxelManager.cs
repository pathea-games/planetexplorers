using UnityEngine;
using System;
using System.Collections.Generic;

#if false
namespace Transvoxel.SurfaceExtractor
{
    public class MeshData
    {
        public MeshData()
        {
            Vertices = new List<Vertex>();
            Indices = new List<int>();
        }

        public List<Vertex> Vertices;
        public List<int> Indices;
    }


    /// <summary>
    /// Stores and creates meshes for chunks, also holds volume data
    /// </summary>
    public class TransvoxelManager
    {
        private readonly Dictionary<IntVector3, MeshData> _meshes;

        public IVolumeData VolumeData;

        public TransvoxelManager(IVolumeData volumeData)
        {
            VolumeData = volumeData;
            _meshes = new Dictionary<IntVector3, MeshData>();
        }

        public MeshData RemoveMesh(IntVector3 position)
        {
            MeshData mesh;
            if (_meshes.TryGetValue(position, out mesh))
                _meshes.Remove(position);

            return mesh;
        }

        public MeshData GetMesh(IntVector3 position) // lod and transition parameter?
        {
            MeshData mesh;
            if (!_meshes.TryGetValue(position, out mesh))
            {
                mesh = GenerateMesh();
                _meshes.Add(position, mesh);
            }

            return mesh;
        }

        private MeshData GenerateMesh()
        {
            // probably need some parameters...lod, transition, regular etc...
            throw new NotImplementedException();
        }
    }
}
#endif