using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface ILODNodeData // one Node Data can contain multiple data, This is in TODO LIST.
{
	bool IsEmpty{ 	get; }// Not verts inside
	bool IsIdle{ 	get; }// Not in rebuilding
	void BegUpdateNodeData();
	void EndUpdateNodeData();
	void OnDestroyNodeData();

	//int TimeStamp{ 	get; }
	void UpdateTimeStamp();
}
public interface ILODNodeDataMan
{
	int IdxInLODNodeData{	get;set;}
	LODOctreeMan LodMan{	get;set;}
	ILODNodeData CreateLODNodeData(LODOctreeNode node);	// Use static to delay the need of IVxChunkHelperProc instance
	void ProcPostLodInit();
	void ProcPostLodUpdate();
	void ProcPostLodRefresh();	// may in thread
}
public interface IVxDataLoader
{
	bool IsIdle{			get;	}
	bool ImmMode{ 			get;set;}
	void AddRequest(VFVoxelChunkData data);	
	void ProcessReqs();
	void Close();
}
// Helper interface for chunk data loading and chunk mesh generation
public interface IVxChunkHelperProc
{
	IVxSurfExtractor SurfExtractor{ get; }	// which SurfExtractor is used
	int  ChunkSig{ get;	}	// This peoperty give a relatively unique signature mask of this special chunk
	// callback
	void ChunkProcPreSetDataVT(ILODNodeData ndata, byte[] data, bool bFromPool);
	void ChunkProcPreLoadData(ILODNodeData ndata);	// load data or add data req
	bool ChunkProcExtractData(ILODNodeData ndata);	// extract data from hollow/RLE data, return true if success
	VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z);	// extract data from hollow/RLE data, return voxel at the certain point
	void ChunkProcPostGenMesh(IVxSurfExtractReq ireq);	// generate chun mesh
	// event handler
	void OnBegUpdateNodeData(ILODNodeData ndata);
	void OnEndUpdateNodeData(ILODNodeData ndata);
	void OnDestroyNodeData(ILODNodeData ndata);
}
public interface IVxDataSource
{
//		int Width { get; }
//		int Height { get; }
//		int Depth { get; }
	List<VFVoxelChunkData> DirtyChunkList{	get;	}

	// read a chunk at tshe specified location
    VFVoxelChunkData readChunk(int cx, int cy, int cz, int lod = 0);
	void writeChunk(int cx, int cy, int cz, VFVoxelChunkData data, int lod = 0);
	
	// read a voxel at the specified location
	VFVoxel Read(int x, int y, int z, int lod = 0);
	int Write(int x, int y, int z, VFVoxel voxel,int lod = 0, bool bUpdateLod = true);	// return affected chunks mask(dirty mask)

	VFVoxel SafeRead(int x, int y, int z, int lod = 0);
	bool SafeWrite(int x, int y, int z, VFVoxel voxel, int lod = 0);
	byte this[IntVector3 idx, IntVector4 cposlod]{	get;	}
}

public interface IVxSurfExtractReq
{
	byte[] VolumeData{		get;}
	bool IsInvalid{			get;}	// validation check: out of date or other invalid things
	int  Signature{			get;}	// signature code of this req in order to avoid repeated req
	int  Priority{			get;}	// for sort
	int  MeshSplitThreshold{get;}	// split policy
	int  FillMesh(Mesh mesh);		// return Count if mesh==null
	bool OnReqFinished();			// call
}
public interface IVxSurfExtractor
{
	bool IsIdle{	get;}
	bool IsAllClear{get;}//Idle and finished reqs' list is clear
	bool Pause{		get;set;}
	
	void Init();
	void Exec();
	int  OnFin();		// Proceed  finished reqs' list, ret value is the left list's length
	void Reset();
	void CleanUp();
	bool AddSurfExtractReq(IVxSurfExtractReq req);
}
public class TmpSurfExtractor : IVxSurfExtractor
{
	private TmpSurfExtractor(){}
	private static TmpSurfExtractor _inst;
	public static TmpSurfExtractor Inst{
		get{
			if(_inst==null)	_inst = new TmpSurfExtractor();
			return _inst;
		}
	}
	public bool IsIdle{		get{ return true; } }
	public bool IsAllClear{	get{ return true; } }//Idle and finished reqs' list is clear
	public bool Pause{		get; set; }	
	public void Init(){}
	public void Exec(){}
	public int  OnFin(){return 0;}		// Proceed  finished reqs' list, ret value is the left list's length
	public void Reset(){}
	public void CleanUp(){}
	public bool AddSurfExtractReq(IVxSurfExtractReq req){return true;}
}

