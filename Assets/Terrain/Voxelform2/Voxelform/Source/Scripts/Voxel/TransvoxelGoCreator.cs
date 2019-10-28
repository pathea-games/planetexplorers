using UnityEngine;
//using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

using Transvoxel.Lengyel;
using Transvoxel.SurfaceExtractor;

public enum ETransBuildStatus{
	Status_Idle,
	Status_ToBuild,
	Status_FinBuild,
}
public class TransvoxelGoCreator {
	private Dictionary<IntVector4, VFVoxelChunkData> transRebuildList = new Dictionary<IntVector4, VFVoxelChunkData>();
	private Thread threadBuildTransVoxel = null;
	private IVxSurfExtractor surfExtractorTrans = new SurfExtractorTrans();
	private bool bTransVoxelEnabled = false;
	public ETransBuildStatus curStatus = ETransBuildStatus.Status_Idle;
	public bool IsTransvoxelEnabled{	// Start/Stop transvoxel creator
		get{	return bTransVoxelEnabled;	}
		set{
			bTransVoxelEnabled = value;
			if(!bTransVoxelEnabled)
			{
				ClearAllTrans();
				threadBuildTransVoxel = null;
			}
			else
			{
				PrepChunkList();
				if(threadBuildTransVoxel == null)
				{
					threadBuildTransVoxel = new Thread(new ThreadStart(ThreadBuildTransExec));
					threadBuildTransVoxel.Start();
				}
			}
		}
	}
	public bool PrepChunkList()
	{
		if(curStatus == ETransBuildStatus.Status_Idle)
		{
			transRebuildList.Clear();
			Transform parTrans = VFVoxelTerrain.self.transform;
			foreach(Transform trans in parTrans)
			{
				if(trans.gameObject.activeSelf)
				{
					try{
						VFVoxelChunkGo chunkGo = trans.GetComponent<VFVoxelChunkGo>();
						VFVoxelChunkData cdata = chunkGo.Data;
						transRebuildList.Add(cdata.ChunkPosLod, cdata);
					}
					catch{}	// Delayed Destroy will cause Dictionary.Add's exception
				}
			}
			curStatus = ETransBuildStatus.Status_ToBuild;
			return true;
		}
		return false;
	}
	public void ClearAllTrans()
	{
		//VFVoxelChunkGo[] chunks = VFVoxelTerrain.self.transform.GetComponentsInChildren<VFVoxelChunkGo>();
		Transform terTransform = VFVoxelTerrain.self.transform;
		foreach(Transform trans in terTransform)
		{
			VFVoxelChunkGo chunkGo = trans.GetComponent<VFVoxelChunkGo>();
			if(chunkGo != null)
			{
				if(chunkGo.TransvoxelGo != null)
				{
					VFGoPool<VFTransVoxelGo>.FreeGo(chunkGo.TransvoxelGo);
					chunkGo.TransvoxelGo = null;
				}
			}
		}
	}	
	public void UpdateTransMesh()
	{
		if(bTransVoxelEnabled)
		{
			if(curStatus == ETransBuildStatus.Status_FinBuild)
			{
				if(0 == surfExtractorTrans.OnFin())
				{
					curStatus = ETransBuildStatus.Status_Idle;
				}
			}
		}
	}
	public void ThreadBuildTransExec()
	{
		surfExtractorTrans.Init();
		
		List<IntVector4> locTransPosList = new List<IntVector4>();
		List<VFVoxelChunkData> locTransChunkList = new List<VFVoxelChunkData>();
		while(bTransVoxelEnabled)
		{
			if(curStatus == ETransBuildStatus.Status_ToBuild)
			{
				locTransPosList = transRebuildList.Keys.ToList();
				locTransChunkList = transRebuildList.Values.ToList();
				for(int i = 0; i < locTransChunkList.Count; i++)
				{
					VFVoxelChunkData cdata = locTransChunkList[i];
					//if(cdata.LOD != 0)	continue;
					if(cdata.LOD == LODOctreeMan._maxLod || null == (System.Object)cdata.ChunkGo)	continue;
					int oldFaceMask = null == (System.Object)cdata.ChunkGo.TransvoxelGo ? 0 : cdata.ChunkGo.TransvoxelGo._faceMask;
					int faceMask = 0;
					int curLod = cdata.LOD;
					int posMask = (-1)<<(curLod+1);
					for(int dirIdx = 0; dirIdx < Transvoxel.Lengyel.Tables.TransitionFaceDir.Length; dirIdx++)
					{
						IntVector3 unitOfs = Transvoxel.Lengyel.Tables.TransitionFaceDir[dirIdx];
						IntVector4 nearLow = new IntVector4((cdata.ChunkPosLod.x + (unitOfs.x << curLod))&posMask,
															(cdata.ChunkPosLod.y + (unitOfs.y << curLod))&posMask,
															(cdata.ChunkPosLod.z + (unitOfs.z << curLod))&posMask,
															curLod+1);
						if(locTransPosList.Contains(nearLow))
						{
							faceMask |= 1<<dirIdx;
						}
					}
					if(faceMask == oldFaceMask)			continue;
					surfExtractorTrans.AddSurfExtractReq(new SurfExtractReqTrans(faceMask, cdata));
				}
				surfExtractorTrans.Exec();
				curStatus = ETransBuildStatus.Status_FinBuild;
			}
			
			System.Threading.Thread.Sleep(16);			
		}
		curStatus = ETransBuildStatus.Status_Idle;
	}
	public static int UnindexedVertex(TransVertices verts, List<int> indices, out Vector3[] vert, out Vector2[] norm01, out Vector2[] norm2t)
	{
		int nVert = indices.Count;
		vert = new Vector3[nVert];
		norm01 = new Vector2[nVert];
		norm2t = new Vector2[nVert];
		int idxVert;
		Vector4 normal;
		float type0, type1, type2;
		for(int i = 0; i < nVert;)
		{
			idxVert = indices[i];
			vert[i] = verts.Position[idxVert];
			normal = verts.Normal_t[idxVert];
			norm01[i] = new Vector2(-normal.x, -normal.y);
			norm2t[i] = new Vector2(normal.z, 0.0f);
			type0 = normal.w;
			i++;
			idxVert = indices[i];
			vert[i] = verts.Position[idxVert];
			normal = verts.Normal_t[idxVert];
			norm01[i] = new Vector2(-normal.x, -normal.y);
			norm2t[i] = new Vector2(normal.z, 0.1f);
			type1 = normal.w;
			i++;
			idxVert = indices[i];
			vert[i] = verts.Position[idxVert];
			normal = verts.Normal_t[idxVert];
			norm01[i] = new Vector2(-normal.x, -normal.y);
			norm2t[i] = new Vector2(normal.z, 0.2f);
			type2 = normal.w;
			i++;
			norm2t[i-1].x += type0*4 + 2;	norm2t[i-1].y += type1*256 + type2;
			norm2t[i-2].x += type0*4 + 2;	norm2t[i-2].y += type1*256 + type2;
			norm2t[i-3].x += type0*4 + 2;	norm2t[i-3].y += type1*256 + type2;
		}
		return nVert;
	}
	public static void CreateTransvoxelGo(VFVoxelChunkGo chunkGo, int faceMask)
	{
		TransVertices verts = new TransVertices();
		List<int> indexedIndices = new List<int>();
		float cellSize = 0.01f;
		TransvoxelExtractor2.BuildTransitionCells(faceMask, chunkGo.Data, cellSize, verts, indexedIndices);
       	Vector3[] vert;
        Vector2[] norm01;
		Vector2[] norm2t;
		int chunkVertsCurCnt = UnindexedVertex(verts, indexedIndices, out vert, out norm01, out norm2t);
		
		SurfExtractReqTrans req = new SurfExtractReqTrans(0, null);
		req.vert = vert;
		req.norm01 = norm01;
		req.norm2t = norm2t;
		req.indice = new int[chunkVertsCurCnt];
		Array.Copy(SurfExtractorsMan.s_indiceMax, req.indice, chunkVertsCurCnt);
		
		chunkGo.SetTransGo(req, faceMask);
	}
}
