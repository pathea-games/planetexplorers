using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// VOXEL CREATION Marching cubes computer
// (send data to OCLMC and recv retval, then build mesh)
public class VCMCComputer : IVxChunkHelperProc
{
    // Voxel data source
    private VFCreationDataSource m_DataSource;
    // Chunk List to rebuild
    public bool Computing { get { return !SurfExtractorsMan.VxSurfExtractor.IsAllClear; } }
    // Output
    public VCMeshMgr m_MeshMgr = null;
    public int m_OutputLayer = VCConfig.s_EditorLayer;
    public bool m_ForEditor = true;
    public bool m_CreateBoxCollider = false;



    // Voxel number in a chunk - per axis.
    private const int CHUNK_VOXEL_NUM = VoxelTerrainConstants._numVoxelsPerAxis;

    private static int SN = 0;

    // Cons & Des
    public void Init(IntVector3 editor_size, VCMeshMgr mesh_mgr, bool for_editor = true)
    {
        m_ForEditor = for_editor;
        // Create chunk data source
        int nx = Mathf.CeilToInt((float)(editor_size.x + 1) / CHUNK_VOXEL_NUM);
        int ny = Mathf.CeilToInt((float)(editor_size.y + 1) / CHUNK_VOXEL_NUM);
        int nz = Mathf.CeilToInt((float)(editor_size.z + 1) / CHUNK_VOXEL_NUM);

        //int nx05 = Mathf.CeilToInt(0.5f * nx);
        //int nx0 = mesh_mgr.m_DaggerMesh ? mesh_mgr.m_LeftSidePos ? 0 : nx05 : 0;
        //int nxMax = mesh_mgr.m_DaggerMesh ? mesh_mgr.m_LeftSidePos ? nx05 : nx : nx;

        m_DataSource = new VFCreationDataSource(nx, ny, nz);
        SN++;

        // Create new chunk
        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                for (int z = 0; z < nz; z++)
                {
                    VFVoxelChunkData chunk = new VFVoxelChunkData(null, VFVoxelChunkData.S_ChunkDataAir);
                    chunk.HelperProc = this;
                    IntVector3 snpos = PackSNToChunkPos(new IntVector3(x, y, z), SN);
                    m_DataSource.writeChunk(snpos.x, snpos.y, snpos.z, chunk);
                }
            }
        }
        // Mesh manager
        m_MeshMgr = mesh_mgr;
    }

    public void InitClone(VFCreationDataSource dataSource, VCMeshMgr mesh_mgr, bool for_editor = true)
    {
        m_ForEditor = for_editor;
        m_DataSource = dataSource;
        // Mesh manager
        m_MeshMgr = mesh_mgr;

    }
    // Cons & Des for double
    public void Init(IntVector3 editor_size, VCMeshMgr mesh_mgr, bool for_editor, int meshIndex)
    {
        m_ForEditor = for_editor;
        // Create chunk data source
        int nx = Mathf.CeilToInt((float)(editor_size.x + 1) / CHUNK_VOXEL_NUM);
        int ny = Mathf.CeilToInt((float)(editor_size.y + 1) / CHUNK_VOXEL_NUM);
        int nz = Mathf.CeilToInt((float)(editor_size.z + 1) / CHUNK_VOXEL_NUM);
        m_DataSource = new VFCreationDataSource(nx, ny, nz);
        SN++;

        int nx05 = Mathf.CeilToInt(0.5f * nx);
        int nx0 = mesh_mgr.m_DaggerMesh ? mesh_mgr.m_LeftSidePos ? 0 : nx05 : 0;
        int nxMax = mesh_mgr.m_DaggerMesh ? mesh_mgr.m_LeftSidePos ? nx05 : nx : nx;

        // Create new chunk
        for (int x = nx0; x < nxMax; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                for (int z = 0; z < nz; z++)
                {
                    VFVoxelChunkData chunk = new VFVoxelChunkData(null, VFVoxelChunkData.S_ChunkDataAir);
                    chunk.HelperProc = this;
                    IntVector3 snpos = PackSNToChunkPos(new IntVector3(x, y, z), SN);
                    m_DataSource.writeChunk(snpos.x, snpos.y, snpos.z, chunk, 0);
                }
            }
        }

        // Mesh manager
        m_MeshMgr = mesh_mgr;
    }

    public void Clear()
    {
        // Clear current Data Source
        if (m_DataSource != null)
        {
            for (int x = 0; x < m_DataSource.ChunkNum.x; x++)
            {
                for (int y = 0; y < m_DataSource.ChunkNum.y; y++)
                {
                    for (int z = 0; z < m_DataSource.ChunkNum.z; z++)
                    {
                        VFVoxelChunkData vc = m_DataSource.readChunk(x, y, z);
                        if (vc != null)
                        {
                            vc.ClearMem();
                            vc.SetDataVT(new byte[VFVoxel.c_VTSize] { 0, 0 });
                        }
                    }
                }
            }
        }
    }

    public void Destroy()
    {
        // Destroy current Data Source
        if (m_DataSource != null)
        {
            for (int x = 0; x < m_DataSource.ChunkNum.x; x++)
            {
                for (int y = 0; y < m_DataSource.ChunkNum.y; y++)
                {
                    for (int z = 0; z < m_DataSource.ChunkNum.z; z++)
                    {
                        VFVoxelChunkData vc = m_DataSource.readChunk(x, y, z);
                        if (vc != null)
                        {
                            vc.ClearMem();
                        }
                    }
                }
            }
            m_DataSource = null;
        }
        m_MeshMgr = null;
    }

    // Input
    public void AlterVoxel(int poskey, VCVoxel voxel)
    {
        IntVector3 pos = VCIsoData.KeyToIPos(poskey);
        m_DataSource.Write(pos.x +1,pos.y +1,pos.z +1, new VFVoxel(voxel.Volume, voxel.Type), 0, false);
    }
    public void AlterVoxel(int x, int y, int z, VCVoxel voxel)
    {

        m_DataSource.Write(x + 1, y + 1, z + 1, new VFVoxel(voxel.Volume, voxel.Type), 0, false);
    }

    public bool InSide(int poskey, int xsize, bool left)
    {
        IntVector3 pos = VCIsoData.KeyToIPos(poskey);
        int size05 = Mathf.CeilToInt(0.5f * xsize);
        return left ? pos.x <= size05 : pos.x > size05;
    }

    // ReqMesh
    public void ReqMesh()
    {
        if (m_MeshMgr != null)
            m_DataSource.SubmitReq();
    }

    #region IVxChunkHelperProc
    public IVxSurfExtractor SurfExtractor { get { return SurfExtractorsMan.VxSurfExtractor; } }
    public int ChunkSig { get { return 2; } }
    public ILODNodeData CreateLODNodeData(LODOctreeNode node) { return null; }
    public void ChunkProcPreSetDataVT(ILODNodeData ndata, byte[] data, bool bFromPool) { }
    public void ChunkProcPreLoadData(ILODNodeData ndata) { }
    public bool ChunkProcExtractData(ILODNodeData ndata)
    {
        VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
        byte volume = cdata.DataVT[0];
        byte type = cdata.DataVT[1];
        byte[] data = VFVoxelChunkData.s_ChunkDataPool.Get();
        if (volume != 0)
        {
            for (int i = 0; i < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT; )
            {
                data[i++] = volume;
                data[i++] = type;
            }
        }
        else
        {
            Array.Clear(data, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
        }
        cdata.SetDataVT(data, true);
        return true;
    }
    public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
    {
        VFVoxelChunkData cdata = ndata as VFVoxelChunkData;
        return new VFVoxel(cdata.DataVT[0], cdata.DataVT[1]);
    }
    public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
    {
        if (m_MeshMgr == null) return;

        SurfExtractReqMC req = ireq as SurfExtractReqMC;
        if (req.IsInvalid)
        {
            Debug.Log("[VCSystem] RemoveChunkInSet" + req._chunk.ChunkPosLod
                      + ":" + req._chunkStamp + "|" + req._chunk.StampOfUpdating);
            return;
        }

        IntVector3 pos = SNChunkPosToChunkPos(new IntVector3(req._chunk.ChunkPosLod.x, req._chunk.ChunkPosLod.y, req._chunk.ChunkPosLod.z));

        int mesh_cnt = req.FillMesh(null);
        m_MeshMgr.Clamp(pos, mesh_cnt);

        int index = 0;
        while (mesh_cnt > 0)
        {
            GameObject targetgo = m_MeshMgr.QueryAtIndex(pos, index);
            if (targetgo == null)
            {
                targetgo = new GameObject();

                // mf
                targetgo.AddComponent<MeshFilter>();

                // mr
                MeshRenderer mr = targetgo.AddComponent<MeshRenderer>();
                mr.material = m_MeshMgr.m_MeshMat;
                mr.receiveShadows = true;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                // mc
                // MeshCollider mc = targetgo.AddComponent<MeshCollider>();
                //mc.sharedMesh = null;

                // pp
                if (!m_ForEditor)
                {
                    VCParticlePlayer pp = targetgo.AddComponent<VCParticlePlayer>();
                    pp.FunctionTag = VCParticlePlayer.ftDamaged;
                    pp.LocalPosition = Vector3.zero;
                }

                m_MeshMgr.Set(pos, index, targetgo);
            }
            else
            {
                //  MeshCollider mc = targetgo.GetComponent<MeshCollider>();
                //	mc.sharedMesh = null;
            }
            MeshFilter targetmf = targetgo.GetComponent<MeshFilter>();
            Mesh mesh = targetmf.mesh;

            mesh.Clear();
            mesh_cnt = req.FillMesh(mesh);

            // Customize mesh for VCSystem
            int vert_cnt = mesh.vertexCount;
            Color32[] colors = new Color32[vert_cnt];
            Vector3[] normals = new Vector3[vert_cnt];
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vert_cnt; ++i)
                colors[i] = VCIsoData.BLANK_COLOR;
            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3 normal = Vector3.Cross(vertices[i] - vertices[i + 1], vertices[i] - vertices[i + 2]).normalized;
                normals[i] = normal;
                normals[i + 1] = normal;
                normals[i + 2] = normal;
            }
            mesh.normals = normals;
            mesh.colors32 = colors;
            m_MeshMgr.UpdateMeshColor(targetmf);
            PostGenerate(targetmf);
            ++index;
        }
    }
    public void OnBegUpdateNodeData(ILODNodeData ndata) { }
    public void OnEndUpdateNodeData(ILODNodeData ndata) { }
    public void OnDestroyNodeData(ILODNodeData ndata) { }

    #endregion

    private void PostGenerate(MeshFilter mf)
    {
        if (m_ForEditor)
        {
            m_MeshMgr.m_ColliderDirty = true;
        }
        else
        {
            if (m_CreateBoxCollider)
            {
                BoxCollider bc = mf.gameObject.GetComponent<BoxCollider>();
                if (bc != null) BoxCollider.DestroyImmediate(bc);

                RecalcCreationMeshBounds(mf.mesh);
                bc = mf.gameObject.AddComponent<BoxCollider>();

                bc.size *= WhiteCat.PEVCConfig.instance.creationColliderScale;
                bc.material = WhiteCat.PEVCConfig.instance.physicMaterial;
            }

            // particle
            VCParticlePlayer pp = mf.GetComponent<VCParticlePlayer>();
            pp.LocalPosition = VCUtils.RandPosInBoundingBox(mf.mesh.bounds);
        }

        var cc = mf.GetComponentInParent<WhiteCat.CreationController>();
        if (cc) cc.OnNewMeshBuild(mf);
    }

    static void RecalcCreationMeshBounds(Mesh mesh)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3 current;

        int count = mesh.triangles.Length;
        var vertices = mesh.vertices;

        for (int i = 0; i < count; i++)
        {
            current = vertices[i];

            if (current.x < min.x) min.x = current.x;
            if (current.y < min.y) min.y = current.y;
            if (current.z < min.z) min.z = current.z;

            if (current.x > max.x) max.x = current.x;
            if (current.y > max.y) max.y = current.y;
            if (current.z > max.z) max.z = current.z;
        }

        mesh.bounds = new Bounds((min + max) * 0.5f, max - min);
    }

    public static IntVector3 SNChunkPosToChunkPos(IntVector3 pos)
    {
        return new IntVector3(pos.x & 31, pos.y & 31, pos.z & 31);
    }

    public static IntVector3 PackSNToChunkPos(IntVector3 pos, int sn)
    {
        int x = pos.x & 31;
        int y = pos.y & 31;
        int z = pos.z & 31;
        int snx = sn & 31;
        int sny = (sn >> 5) & 31;
        int snz = (sn >> 10) & 31;
        return new IntVector3(x | (snx << 5), y | (sny << 5), z | (snz << 5));
    }
}