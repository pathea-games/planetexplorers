using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Transvoxel.SurfaceExtractor;

public class SurfExtractReqTrans : IVxSurfExtractReq
{
	// return data
	public Vector3[] vert;
	public Vector2[] norm01;
	public Vector2[] norm2t;
	public int[] indice;

	public int _chunkStamp;
	public int _faceMask; 
	public VFVoxelChunkData _chunkData;
	public SurfExtractReqTrans(int faceMask, VFVoxelChunkData chunkData)		
	{
		_faceMask	= faceMask;
		_chunkStamp	= chunkData.ChunkPosLod.GetHashCode();
		_chunkData	= chunkData;
	}
	
	public byte[] VolumeData{		get{ return _chunkData.DataVT;	}	}	// not used, always false
	public bool IsInvalid{			get{ return _chunkData.ChunkPosLod.GetHashCode() != _chunkStamp;	}	}
	public int  Signature{			get{ return _chunkStamp;}	}	// not used
	public int  Priority{			get{ return 0;			}	}	// not used
	public int  MeshSplitThreshold{	get{ return SurfExtractorsMan.c_vertsCntMax;		}	}	// use default number
	public int  FillMesh(Mesh mesh)
	{
		mesh.name 	  = "trans";
		mesh.vertices = vert;
		mesh.uv  	  = norm01;
		mesh.uv2 	  = norm2t;
		mesh.SetTriangles(indice, 0);
		return 0;
	}
	public bool OnReqFinished()
	{
		bool ret = false;
		if (!IsInvalid) {
			VFVoxelChunkGo go = _chunkData.ChunkGo;
			if(go != null && !go.Equals(null)){
				go.SetTransGo (this, _faceMask);
				ret = true;
			}
		}
		return ret;
	}
}

public class SurfExtractorTrans : IVxSurfExtractor
{
	Dictionary<int, SurfExtractReqTrans> _reqList;
	List<IVxSurfExtractReq> _reqFinishedList;
	
	// interfaces
	public bool IsIdle{		get{	return _reqList.Count == 0;	}	}
	public bool IsAllClear{	get{	return _reqList.Count == 0 && _reqFinishedList.Count == 0;	}	}
	public bool Pause{		get;set;}

	public void Init(){
		_reqList = new Dictionary<int, SurfExtractReqTrans>();
		_reqFinishedList = new List<IVxSurfExtractReq>();
		
		Pause = false;
	}
	public void Exec(){
		if(_reqList.Count == 0 || Pause)	return;
		
		foreach(SurfExtractReqTrans req in _reqList.Values)
		{
			// Not check out of date because add_req is just called before this method in the same thread.
			if(req._faceMask != 0)
			{
				TransVertices verts = new TransVertices();
				List<int> indexedIndices = new List<int>();
				float cellSize = 0.01f;
				TransvoxelExtractor2.BuildTransitionCells(req._faceMask, req._chunkData, cellSize, verts, indexedIndices);
				//int nVerts = verts.Count;
				int chunkVertsCurCnt = TransvoxelGoCreator.UnindexedVertex(verts, indexedIndices, out req.vert, out req.norm01, out req.norm2t);
				req.indice = new int[chunkVertsCurCnt];
				Array.Copy(SurfExtractorsMan.s_indiceMax,req.indice,chunkVertsCurCnt);
			}
			lock(_reqFinishedList){
				_reqFinishedList.Add(req);
			}
		}
		
		_reqList.Clear();
	}
	const int NumReqInFin = 12;
	public int OnFin(){
		if(_reqFinishedList.Count == 0)		return 0 ;
		
		int nMax = _reqFinishedList.Count;
		int nMin = nMax > NumReqInFin ? (nMax-NumReqInFin) : 0;
		for(int n = nMax - 1; n >= nMin; n--)
		{
			_reqFinishedList[n].OnReqFinished();
			_reqFinishedList.RemoveAt(n);
		}		
		return _reqFinishedList.Count;
	}
	public void Reset(){
		Init();
	}
	public void CleanUp()
	{
		// do nothing
	}	
	public bool AddSurfExtractReq(IVxSurfExtractReq req){ 
		_reqList[req.Signature] = (SurfExtractReqTrans)req;
		return true; 
	}
}
